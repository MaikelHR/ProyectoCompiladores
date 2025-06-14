using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ProyectoCompi.Semantics;

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

        public CodeGeneratorVisitor(string className)
        {
            _symbolTable = new SymbolTable(null, true);
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(className),
                AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(className + ".exe");
            _errors = new List<string>();
            _locals = new Dictionary<string, LocalBuilder>();
            _breakLabels = new Stack<Label>();
            _continueLabels = new Stack<Label>();
        }

        public override SymbolType VisitProgramRule(MiniCSharpParser.ProgramRuleContext context)
        {
            foreach (var classDecl in context.classDecl())
            {
                Visit(classDecl);
            }
            return null;
        }

        public override SymbolType VisitClassDeclaration(MiniCSharpParser.ClassDeclarationContext context)
        {
            string className = context.ID().GetText();
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
            string varName = context.ID().GetText();
            var varType = GetTypeFromContext(context.type());

            if (_symbolTable.Lookup(varName) != null)
            {
                _errors.Add($"Variable '{varName}' ya está definida en este ámbito");
                return null;
            }

            var fieldBuilder = _currentTypeBuilder.DefineField(varName, GetSystemType(varType), FieldAttributes.Private);
            _symbolTable.Define(new VariableSymbol(varName, varType));

            if (context.expr() != null)
            {
                var valueType = Visit(context.expr());
                if (!valueType.IsAssignableTo(varType))
                {
                    _errors.Add($"No se puede asignar un valor de tipo {valueType} a una variable de tipo {varType}");
                }
            }

            return varType;
        }

        public override SymbolType VisitMethodDeclaration(MiniCSharpParser.MethodDeclarationContext context)
        {
            string methodName = context.ID().GetText();
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

            _currentMethodBuilder = _currentTypeBuilder.DefineMethod(
                methodName,
                MethodAttributes.Public,
                GetSystemType(returnType),
                paramTypes.ToArray()
            );

            var methodSymbol = new FunctionSymbol(methodName, returnType, parameters);
            if (_symbolTable.Lookup(methodName) != null)
            {
                _errors.Add($"Método '{methodName}' ya está definido en esta clase");
                return null;
            }

            _symbolTable.Define(methodSymbol);
            _currentReturnType = returnType;
            _isInFunction = true;

            _currentILGenerator = _currentMethodBuilder.GetILGenerator();
            foreach (var param in parameters)
            {
                var localBuilder = _currentILGenerator.DeclareLocal(GetSystemType(param.Type));
                _locals[param.Name] = localBuilder;
            }

            Visit(context.block());

            _isInFunction = false;
            _currentReturnType = null;
            return returnType;
        }

        public override SymbolType VisitBlockStmt(MiniCSharpParser.BlockStmtContext context)
        {
            var oldSymbolTable = _symbolTable;
            _symbolTable = _symbolTable.CreateChildScope();
            foreach (var stmt in context.stmt())
            {
                Visit(stmt);
            }
            _symbolTable = oldSymbolTable;
            return null;
        }

        public override SymbolType VisitIfStatement(MiniCSharpParser.IfStatementContext context)
        {
            var conditionType = Visit(context.expr());
            if (!(conditionType is SimpleType boolType) || boolType.Name != "bool")
            {
                _errors.Add("La condición del if debe ser de tipo bool");
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
            var startLabel = _currentILGenerator.DefineLabel();
            var endLabel = _currentILGenerator.DefineLabel();
            _breakLabels.Push(endLabel);
            _continueLabels.Push(startLabel);

            _currentILGenerator.MarkLabel(startLabel);
            var conditionType = Visit(context.expr());
            if (!(conditionType is SimpleType boolType) || boolType.Name != "bool")
            {
                _errors.Add("La condición del while debe ser de tipo bool");
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
            if (!_isInFunction)
            {
                _errors.Add("Return statement fuera de una función");
                return null;
            }

            if (context.expr() != null)
            {
                var returnType = Visit(context.expr());
                if (!returnType.IsAssignableTo(_currentReturnType))
                {
                    _errors.Add($"No se puede retornar un valor de tipo {returnType} en una función que retorna {_currentReturnType}");
                }
            }
            else if (!(_currentReturnType is SimpleType voidType) || voidType.Name != "void")
            {
                _errors.Add("Se esperaba un valor de retorno");
            }

            return null;
        }

        public override SymbolType VisitMultiplicativeExpression(MiniCSharpParser.MultiplicativeExpressionContext context)
        {
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType) || !(rightType is SimpleType))
            {
                _errors.Add("Operaciones aritméticas solo permitidas entre tipos simples");
                return null;
            }

            var leftSimple = (SimpleType)leftType;
            var rightSimple = (SimpleType)rightType;

            if (leftSimple.Name != "int" && leftSimple.Name != "double" || 
                rightSimple.Name != "int" && rightSimple.Name != "double")
            {
                _errors.Add("Operaciones aritméticas solo permitidas entre números");
                return null;
            }

            return leftSimple.Name == "double" || rightSimple.Name == "double" ? 
                new SimpleType("double") : new SimpleType("int");
        }

        public override SymbolType VisitAdditiveExpression(MiniCSharpParser.AdditiveExpressionContext context)
        {
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType) || !(rightType is SimpleType))
            {
                _errors.Add("Operaciones aritméticas solo permitidas entre tipos simples");
                return null;
            }

            var leftSimple = (SimpleType)leftType;
            var rightSimple = (SimpleType)rightType;

            if (leftSimple.Name != "int" && leftSimple.Name != "double" || 
                rightSimple.Name != "int" && rightSimple.Name != "double")
            {
                _errors.Add("Operaciones aritméticas solo permitidas entre números");
                return null;
            }

            return leftSimple.Name == "double" || rightSimple.Name == "double" ? 
                new SimpleType("double") : new SimpleType("int");
        }

        public override SymbolType VisitRelationalExpression(MiniCSharpParser.RelationalExpressionContext context)
        {
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType) || !(rightType is SimpleType))
            {
                _errors.Add("Operaciones relacionales solo permitidas entre tipos simples");
                return null;
            }

            var leftSimple = (SimpleType)leftType;
            var rightSimple = (SimpleType)rightType;

            if (leftSimple.Name != "int" && leftSimple.Name != "double" || 
                rightSimple.Name != "int" && rightSimple.Name != "double")
            {
                _errors.Add("Operaciones relacionales solo permitidas entre números");
                return null;
            }

            return new SimpleType("bool");
        }

        public override SymbolType VisitLogicalExpression(MiniCSharpParser.LogicalExpressionContext context)
        {
            var leftType = Visit(context.expr(0));
            var rightType = Visit(context.expr(1));

            if (!(leftType is SimpleType leftSimple) || leftSimple.Name != "bool" ||
                !(rightType is SimpleType rightSimple) || rightSimple.Name != "bool")
            {
                _errors.Add("Operaciones lógicas solo permitidas entre booleanos");
                return null;
            }

            return new SimpleType("bool");
        }

        public override SymbolType VisitIdentifier(MiniCSharpParser.IdentifierContext context)
        {
            string name = context.ID().GetText();
            var symbol = _symbolTable.Lookup(name);
            if (symbol == null)
            {
                _errors.Add($"Variable '{name}' no definida");
                return null;
            }
            return symbol.Type;
        }

        public override SymbolType VisitIntegerLiteral(MiniCSharpParser.IntegerLiteralContext context)
        {
            return new SimpleType("int");
        }

        public override SymbolType VisitStringLiteral(MiniCSharpParser.StringLiteralContext context)
        {
            return new SimpleType("string");
        }

        public override SymbolType VisitBooleanLiteral(MiniCSharpParser.BooleanLiteralContext context)
        {
            return new SimpleType("bool");
        }

        public override SymbolType VisitCharLiteral(MiniCSharpParser.CharLiteralContext context)
        {
            return new SimpleType("char");
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

        public void SaveAssembly(string outputPath)
        {
            _currentTypeBuilder.CreateType();
            _moduleBuilder.CreateGlobalFunctions();
            Console.WriteLine("El ensamblado se generó en memoria (no se puede guardar en disco en .NET Core/5+/6+).");
        }
    }
} 