using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ProyectoCompi.Semantics;
using System.IO;
using System.Runtime.InteropServices;

namespace ProyectoCompi.Gen
{
    public class CodeGeneratorVisitor : MiniCSharpParserBaseVisitor<SymbolType>
    {
        private SymbolTable _symbolTable;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private TypeBuilder _currentTypeBuilder;
        private MethodBuilder _currentMethodBuilder;
        private ILGenerator _currentILGenerator;
        private readonly Dictionary<string, LocalBuilder> _localVariables = new Dictionary<string, LocalBuilder>();
        private readonly Dictionary<string, Label> _labels = new Dictionary<string, Label>();
        private readonly List<string> _errors;
        private SymbolType _currentReturnType;
        private bool _isInFunction;
        private readonly Dictionary<string, LocalBuilder> _locals;
        private readonly Stack<Label> _breakLabels;
        private readonly Stack<Label> _continueLabels;
        private readonly Dictionary<string, MethodBuilder> _methodBuilders = new Dictionary<string, MethodBuilder>();

        public CodeGeneratorVisitor(string assemblyName)
        {
            _symbolTable = new SymbolTable(null, true);
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName),
                AssemblyBuilderAccess.Run
            );
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);
            _errors = new List<string>();
            _locals = new Dictionary<string, LocalBuilder>();
            _breakLabels = new Stack<Label>();
            _continueLabels = new Stack<Label>();
        }

        public override SymbolType VisitProgramRule(MiniCSharpParser.ProgramRuleContext context)
        {
            Console.WriteLine("Visitando programa");
            foreach (var classDecl in context.classDecl())
            {
                Visit(classDecl);
            }
            return null;
        }

        public override SymbolType VisitClassDeclaration(MiniCSharpParser.ClassDeclarationContext context)
        {
            string className = context.ID().GetText();
            Console.WriteLine($"Visitando declaración de clase: {className}");
            _currentTypeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);

            var classBody = context.classBody();
            if (classBody is MiniCSharpParser.ClassBodyRuleContext bodyRule)
            {
                foreach (var varDecl in bodyRule.varDecl())
                    Visit(varDecl);
                foreach (var methodDecl in bodyRule.methodDecl())
                    Visit(methodDecl);
            }

            return null;
        }

        public override SymbolType VisitVariableDeclaration(MiniCSharpParser.VariableDeclarationContext context)
        {
            var type = GetTypeFromContext(context.type());
            var name = context.ID().GetText();
            Console.WriteLine($"Visitando declaración de variable: {name} de tipo {type}");

            // Verificar si la variable ya está definida
            var existingSymbol = _symbolTable.Lookup(name);
            if (existingSymbol != null)
            {
                // Solo reportar error si es una variable, no si es una función
                if (existingSymbol is VariableSymbol)
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Variable '{name}' ya definida");
                    return null;
                }
            }

            // Verificar inicialización
            if (context.expr() != null)
            {
                var initType = Visit(context.expr());
                if (initType == null) return null;

                if (!initType.IsAssignableTo(type))
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: No se puede asignar un valor de tipo {initType} a una variable de tipo {type}");
                    return null;
                }
            }

            try
            {
                _symbolTable.Define(new VariableSymbol(name, type));
                
                // Declarar la variable local en el IL
                if (_currentILGenerator != null)
                {
                    var localBuilder = _currentILGenerator.DeclareLocal(GetSystemType(type));
                    _locals[name] = localBuilder;
                }
            }
            catch (Exception ex)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: {ex.Message}");
                return null;
            }

            // Agregar errores de ArrayType si existen
            if (ArrayType.HasErrors())
            {
                foreach (var error in ArrayType.GetErrors())
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: {error}");
                }
                ArrayType.ClearErrors();
            }

            return type;
        }

        public override SymbolType VisitMethodDeclaration(MiniCSharpParser.MethodDeclarationContext context)
        {
            string methodName = context.ID().GetText();
            Console.WriteLine($"Visitando declaración de método: {methodName}");
            var returnType = GetTypeFromContext(context.type());
            var paramTypes = new List<Type>();
            var parameters = new List<ParameterSymbol>();

            if (context.paramList() is MiniCSharpParser.ParameterListContext paramList)
            {
                foreach (var param in paramList.param())
                {
                    if (param is MiniCSharpParser.ParameterContext parameter)
                    {
                        var paramType = GetTypeFromContext(parameter.type());
                        paramTypes.Add(GetSystemType(paramType));
                        parameters.Add(new ParameterSymbol(parameter.ID().GetText(), paramType));
                    }
                }
            }

            var methodAttributes = MethodAttributes.Public;
            if (methodName == "Main")
            {
                methodAttributes |= MethodAttributes.Static;
            }

            _currentMethodBuilder = _currentTypeBuilder.DefineMethod(
                methodName,
                methodAttributes,
                GetSystemType(returnType),
                paramTypes.ToArray()
            );

            // Guardar el MethodBuilder para uso posterior
            _methodBuilders[methodName] = _currentMethodBuilder;

            var methodSymbol = new FunctionSymbol(methodName, returnType, parameters);
            if (_symbolTable.Lookup(methodName) != null)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Método '{methodName}' ya está definido en esta clase");
                return null;
            }

            _symbolTable.Define(methodSymbol);
            _currentReturnType = returnType;
            _isInFunction = true;

            _currentILGenerator = _currentMethodBuilder.GetILGenerator();
            
            // Declarar variables locales para los parámetros
            for (int i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                var localBuilder = _currentILGenerator.DeclareLocal(GetSystemType(param.Type));
                _locals[param.Name] = localBuilder;
                
                // Cargar el parámetro en la variable local
                _currentILGenerator.Emit(OpCodes.Ldarg, i);
                _currentILGenerator.Emit(OpCodes.Stloc, localBuilder.LocalIndex);
            }

            // Visitar el cuerpo del método
            if (context.block() != null)
            {
                Visit(context.block());
            }

            // Asegurar que el método tenga un cuerpo válido
            if (_currentILGenerator != null)
            {
                // Si el método es void, agregar un retorno implícito
                if (returnType is SimpleType simpleType && simpleType.Name == "void")
                {
                    _currentILGenerator.Emit(OpCodes.Ret);
                }
                // Si no es void y no hay un return explícito, agregar un retorno con valor por defecto
                else if (context.block() == null || !context.block().GetText().Contains("return"))
                {
                    if (returnType is SimpleType returnSimpleType)
                    {
                        switch (returnSimpleType.Name)
                        {
                            case "int":
                                _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                                break;
                            case "bool":
                                _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                                break;
                            case "char":
                                _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                                break;
                        }
                    }
                    _currentILGenerator.Emit(OpCodes.Ret);
                }
            }

            _isInFunction = false;
            _currentReturnType = null;
            return returnType;
        }

        public override SymbolType VisitBlockStmt(MiniCSharpParser.BlockStmtContext context)
        {
            Console.WriteLine("Visitando bloque");
            var oldSymbolTable = _symbolTable;
            _symbolTable = _symbolTable.CreateChildScope();
            
            // Visitar cada declaración en el bloque
            foreach (var stmt in context.stmt())
            {
                Visit(stmt);
            }
            
            _symbolTable = oldSymbolTable;
            return null;
        }

        public override SymbolType VisitIfStatement(MiniCSharpParser.IfStatementContext context)
        {
            Console.WriteLine("Visitando if");
            var conditionType = Visit(context.expr());
            if (!(conditionType is SimpleType boolType) || boolType.Name != "bool")
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: La condición del if debe ser de tipo bool");
                return null;
            }

            var endLabel = _currentILGenerator.DefineLabel();
            var elseLabel = _currentILGenerator.DefineLabel();

            _currentILGenerator.Emit(OpCodes.Brfalse, elseLabel);
            Visit(context.stmt(0));
            _currentILGenerator.Emit(OpCodes.Br, endLabel);

            _currentILGenerator.MarkLabel(elseLabel);
            if (context.stmt().Length > 1)
            {
                Visit(context.stmt(1));
            }

            _currentILGenerator.MarkLabel(endLabel);
            return null;
        }

        public override SymbolType VisitWhileStatement(MiniCSharpParser.WhileStatementContext context)
        {
            Console.WriteLine("Visitando while");
            var startLabel = _currentILGenerator.DefineLabel();
            var endLabel = _currentILGenerator.DefineLabel();
            _breakLabels.Push(endLabel);
            _continueLabels.Push(startLabel);

            _currentILGenerator.MarkLabel(startLabel);
            var conditionType = Visit(context.expr());
            if (!(conditionType is SimpleType boolType) || boolType.Name != "bool")
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: La condición del while debe ser de tipo bool");
                return null;
            }

            _currentILGenerator.Emit(OpCodes.Brfalse, endLabel);
            Visit(context.stmt());
            _currentILGenerator.Emit(OpCodes.Br, startLabel);

            _currentILGenerator.MarkLabel(endLabel);
            _breakLabels.Pop();
            _continueLabels.Pop();
            return null;
        }

        public override SymbolType VisitReturnStatement(MiniCSharpParser.ReturnStatementContext context)
        {
            Console.WriteLine("Visitando return");
            if (!_isInFunction)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Return fuera de una función");
                return null;
            }

            if (context.expr() != null)
            {
                var returnType = Visit(context.expr());
                if (returnType == null) return null;

                if (_currentReturnType is SimpleType simpleType && simpleType.Name == "void")
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: No se puede retornar un valor en una función void");
                    return null;
                }

                if (!returnType.IsAssignableTo(_currentReturnType))
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Tipo de retorno incompatible. Se esperaba {_currentReturnType}, se recibió {returnType}");
                }

                // Generar código IL para el retorno
                if (_currentILGenerator != null)
                {
                    _currentILGenerator.Emit(OpCodes.Ret);
                }
            }
            else if (!(_currentReturnType is SimpleType simpleType && simpleType.Name == "void"))
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Se esperaba un valor de retorno de tipo {_currentReturnType}");
            }
            else
            {
                // Generar código IL para retorno void
                if (_currentILGenerator != null)
                {
                    _currentILGenerator.Emit(OpCodes.Ret);
                }
            }

            return null;
        }

        public override SymbolType VisitExpressionStatement(MiniCSharpParser.ExpressionStatementContext context)
        {
            Console.WriteLine("Visitando expresión");
            var exprType = Visit(context.expr());
            
            // Si estamos dentro de un método, generar código IL para la expresión
            if (_currentILGenerator != null && exprType != null)
            {
                // Si la expresión es una asignación, necesitamos almacenar el resultado
                if (context.expr().GetText().Contains("="))
                {
                    _currentILGenerator.Emit(OpCodes.Pop); // Descartar el resultado de la asignación
                }
            }
            
            return exprType;
        }

        public override SymbolType VisitMethodCall(MiniCSharpParser.MethodCallContext context)
        {
            string methodName = context.ID().GetText();
            Console.WriteLine($"Visitando llamada a método: {methodName}");
            
            var methodSymbol = _symbolTable.Lookup(methodName) as FunctionSymbol;
            if (methodSymbol == null)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Método '{methodName}' no definido");
                return null;
            }

            var argTypes = VisitArgList(context.argList());
            if (argTypes == null) return null;

            if (argTypes.Count != methodSymbol.Parameters.Count)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Número incorrecto de argumentos para el método '{methodName}'");
                return null;
            }

            for (int i = 0; i < argTypes.Count; i++)
            {
                if (!argTypes[i].IsAssignableTo(methodSymbol.Parameters[i].Type))
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Tipo de argumento incompatible en la posición {i + 1} para el método '{methodName}'");
                    return null;
                }
            }

            // Generar código IL para la llamada al método
            if (_currentILGenerator != null)
            {
                if (_methodBuilders.TryGetValue(methodName, out var methodBuilder))
                {
                    _currentILGenerator.Emit(OpCodes.Call, methodBuilder);
                }
                else
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: No se pudo encontrar el método '{methodName}' para generar el código");
                }
            }

            return methodSymbol.Type;
        }

        private List<SymbolType> VisitArgList(MiniCSharpParser.ArgListContext context)
        {
            Console.WriteLine("Visitando lista de argumentos");
            var types = new List<SymbolType>();
            if (context is MiniCSharpParser.ArgumentListContext argList)
            {
                foreach (var expr in argList.expr())
                {
                    types.Add(Visit(expr));
                }
            }
            return types;
        }

        public override SymbolType VisitArgumentList(MiniCSharpParser.ArgumentListContext context)
        {
            Console.WriteLine("Visitando lista de argumentos");
            var types = new List<SymbolType>();
            foreach (var expr in context.expr())
            {
                types.Add(Visit(expr));
            }
            return null;
        }

        public override SymbolType VisitParameterList(MiniCSharpParser.ParameterListContext context)
        {
            Console.WriteLine("Visitando lista de parámetros");
            foreach (var param in context.param())
            {
                Visit(param);
            }
            return null;
        }

        public override SymbolType VisitParameter(MiniCSharpParser.ParameterContext context)
        {
            var type = GetTypeFromContext(context.type());
            var name = context.ID().GetText();
            Console.WriteLine($"Visitando parámetro: {name} de tipo {type}");
            _symbolTable.Define(new VariableSymbol(name, type));
            return type;
        }

        public override SymbolType VisitIdentifier(MiniCSharpParser.IdentifierContext context)
        {
            string name = context.ID().GetText();
            Console.WriteLine($"Visitando identificador: {name}");
            var symbol = _symbolTable.Lookup(name);
            
            if (symbol == null)
            {
                // No reportar error si estamos dentro de una declaración de método
                if (!_isInFunction)
                {
                    _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Variable '{name}' no definida");
                }
                return null;
            }

            // Generar código IL para cargar la variable
            if (_currentILGenerator != null)
            {
                if (symbol is ParameterSymbol)
                {
                    if (_locals.ContainsKey(name))
                    {
                        _currentILGenerator.Emit(OpCodes.Ldarg, _locals[name].LocalIndex);
                    }
                }
                else if (symbol is VariableSymbol)
                {
                    if (_locals.ContainsKey(name))
                    {
                        _currentILGenerator.Emit(OpCodes.Ldloc, _locals[name].LocalIndex);
                    }
                }
            }
            
            return symbol.Type;
        }

        public override SymbolType VisitIntegerLiteral(MiniCSharpParser.IntegerLiteralContext context)
        {
            Console.WriteLine("Visitando literal entero");
            int value = int.Parse(context.INT().GetText());
            
            // Generar código IL para cargar el valor
            if (_currentILGenerator != null)
            {
                _currentILGenerator.Emit(OpCodes.Ldc_I4, value);
            }
            
            return new SimpleType("int");
        }

        public override SymbolType VisitStringLiteral(MiniCSharpParser.StringLiteralContext context)
        {
            Console.WriteLine("Visitando literal string");
            return new SimpleType("string");
        }

        public override SymbolType VisitBooleanLiteral(MiniCSharpParser.BooleanLiteralContext context)
        {
            Console.WriteLine("Visitando literal booleano");
            return new SimpleType("bool");
        }

        public override SymbolType VisitCharLiteral(MiniCSharpParser.CharLiteralContext context)
        {
            Console.WriteLine("Visitando literal char");
            return new SimpleType("char");
        }

        public override SymbolType VisitAdditiveExpression(MiniCSharpParser.AdditiveExpressionContext context)
        {
            Console.WriteLine("Visitando expresión aditiva");
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));
            
            if (leftType == null || rightType == null) return null;
            
            if (!(leftType is SimpleType leftSimple) || !(rightType is SimpleType rightSimple))
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Operación aritmética inválida entre tipos {leftType} y {rightType}");
                return null;
            }
            
            if (leftSimple.Name != "int" || rightSimple.Name != "int")
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Operación aritmética solo permitida entre enteros");
                return null;
            }

            // Generar código IL para la suma
            if (_currentILGenerator != null)
            {
                _currentILGenerator.Emit(OpCodes.Add);
            }
            
            return new SimpleType("int");
        }

        public override SymbolType VisitMultiplicativeExpression(MiniCSharpParser.MultiplicativeExpressionContext context)
        {
            Console.WriteLine("Visitando expresión multiplicativa");
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType) || !(rightType is SimpleType))
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Operaciones aritméticas solo permitidas entre tipos simples");
                return null;
            }

            var leftSimple = (SimpleType)leftType;
            var rightSimple = (SimpleType)rightType;

            if (leftSimple.Name != "int" && leftSimple.Name != "double" || 
                rightSimple.Name != "int" && rightSimple.Name != "double")
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Operaciones aritméticas solo permitidas entre números");
                return null;
            }

            return leftSimple.Name == "double" || rightSimple.Name == "double" ? 
                new SimpleType("double") : new SimpleType("int");
        }

        public override SymbolType VisitRelationalExpression(MiniCSharpParser.RelationalExpressionContext context)
        {
            Console.WriteLine("Visitando expresión relacional");
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));
            
            if (leftType == null || rightType == null) return null;
            
            if (!(leftType is SimpleType leftSimple) || !(rightType is SimpleType rightSimple))
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Comparación inválida entre tipos {leftType} y {rightType}");
                return null;
            }
            
            if (leftSimple.Name != rightSimple.Name)
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: No se pueden comparar tipos diferentes ({leftType} y {rightType})");
                return null;
            }

            // Generar código IL para la comparación
            if (_currentILGenerator != null)
            {
                string op = context.GetChild(1).GetText();
                switch (op)
                {
                    case ">":
                        _currentILGenerator.Emit(OpCodes.Cgt);
                        break;
                    case "<":
                        _currentILGenerator.Emit(OpCodes.Clt);
                        break;
                    case ">=":
                        _currentILGenerator.Emit(OpCodes.Clt);
                        _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                        _currentILGenerator.Emit(OpCodes.Ceq);
                        break;
                    case "<=":
                        _currentILGenerator.Emit(OpCodes.Cgt);
                        _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                        _currentILGenerator.Emit(OpCodes.Ceq);
                        break;
                    case "==":
                        _currentILGenerator.Emit(OpCodes.Ceq);
                        break;
                    case "!=":
                        _currentILGenerator.Emit(OpCodes.Ceq);
                        _currentILGenerator.Emit(OpCodes.Ldc_I4_0);
                        _currentILGenerator.Emit(OpCodes.Ceq);
                        break;
                }
            }
            
            return new SimpleType("bool");
        }

        public override SymbolType VisitLogicalExpression(MiniCSharpParser.LogicalExpressionContext context)
        {
            Console.WriteLine("Visitando expresión lógica");
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType leftSimple) || leftSimple.Name != "bool" ||
                !(rightType is SimpleType rightSimple) || rightSimple.Name != "bool")
            {
                _errors.Add($"Error en línea {context.Start.Line}, columna {context.Start.Column}: Operaciones lógicas solo permitidas entre booleanos");
                return null;
            }

            return new SimpleType("bool");
        }

        private SymbolType GetTypeFromContext(MiniCSharpParser.TypeContext context)
        {
            if (context is MiniCSharpParser.IntTypeContext) return new SimpleType("int");
            if (context is MiniCSharpParser.StringTypeContext) return new SimpleType("string");
            if (context is MiniCSharpParser.BoolTypeContext) return new SimpleType("bool");
            if (context is MiniCSharpParser.DoubleTypeContext) return new SimpleType("double");
            if (context is MiniCSharpParser.CharTypeContext) return new SimpleType("char");
            if (context is MiniCSharpParser.ArrayTypeContext arrayType)
            {
                var baseType = GetTypeFromContext(arrayType.type());
                return new ArrayType(baseType);
            }
            if (context is MiniCSharpParser.ClassTypeContext classType)
            {
                return new SimpleType(classType.ID().GetText());
            }
            return null;
        }

        private Type GetSystemType(SymbolType symbolType)
        {
            if (symbolType is SimpleType simpleType)
            {
                switch (simpleType.Name)
                {
                    case "int": return typeof(int);
                    case "string": return typeof(string);
                    case "bool": return typeof(bool);
                    case "double": return typeof(double);
                    case "char": return typeof(char);
                    case "void": return typeof(void);
                    default: return Type.GetType(simpleType.Name);
                }
            }
            else if (symbolType is ArrayType arrayType)
            {
                return GetSystemType(arrayType.BaseType).MakeArrayType();
            }
            return typeof(object);
        }

        public bool HasErrors()
        {
            return _errors.Count > 0 || SymbolTable.HasErrors() || ArrayType.HasErrors();
        }

        public IEnumerable<string> GetErrors()
        {
            var allErrors = new List<string>(_errors);
            allErrors.AddRange(SymbolTable.GetErrors());
            allErrors.AddRange(ArrayType.GetErrors());
            return allErrors;
        }

        public void SaveAssembly(string outputPath)
        {
            try
            {
                // Imprimir todos los errores semánticos encontrados
                if (_errors.Count > 0)
                {
                    Console.WriteLine("\nErrores semánticos encontrados:");
                    foreach (var error in _errors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                    return;
                }

                // Crear el tipo y las funciones globales
                _currentTypeBuilder.CreateType();
                _moduleBuilder.CreateGlobalFunctions();
                
                // Obtener el punto de entrada (insensible a mayúsculas/minúsculas y a static)
                var entryPoint = _currentTypeBuilder.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name.Equals("main", StringComparison.OrdinalIgnoreCase));
                
                if (entryPoint == null)
                {
                    Console.WriteLine("No se encontró el método main");
                    return;
                }

                Console.WriteLine("Ensamblado generado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el ensamblado: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
} 