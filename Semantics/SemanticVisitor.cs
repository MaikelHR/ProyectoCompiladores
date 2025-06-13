using System;
using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

namespace ProyectoCompi.Semantics
{
    public class SemanticVisitor : MiniCSharpParserBaseVisitor<Type>
    {
        private readonly SymbolTable _symbolTable;
        private readonly SemanticErrorCollector _errorCollector;
        private Type _currentReturnType;
        private bool _isInFunction;

        public SemanticVisitor()
        {
            _symbolTable = new SymbolTable();
            _errorCollector = new SemanticErrorCollector();
            _symbolTable.InitializeBuiltInSymbols();
            _isInFunction = false;
        }

        public override Type VisitProgram(MiniCSharpParser.ProgramContext context)
        {
            foreach (var classDecl in context.classDecl())
            {
                VisitClassDecl(classDecl);
            }
            return null;
        }

        public override Type VisitClassDecl(MiniCSharpParser.ClassDeclContext context)
        {
            string className = context.ID().GetText();
            var classScope = _symbolTable.CreateChildScope();
            VisitClassBody(context.classBody());
            return null;
        }

        public override Type VisitMethodDecl(MiniCSharpParser.MethodDeclContext context)
        {
            string methodName = context.ID().GetText();
            var returnType = GetTypeFromContext(context.type());
            var parameters = new List<ParameterSymbol>();

            // Verificar que no sea una redefinición
            if (_symbolTable.Lookup(methodName) != null)
            {
                _errorCollector.AddError($"Método '{methodName}' ya está definido", context.Start.Line, context.Start.Column);
                return null;
            }

            if (context.paramList() != null)
            {
                foreach (var param in context.paramList().param())
                {
                    var paramType = GetTypeFromContext(param.type());
                    parameters.Add(new ParameterSymbol(param.ID().GetText(), paramType));
                }
            }

            var methodSymbol = new FunctionSymbol(methodName, returnType, parameters);
            _symbolTable.Define(methodSymbol);

            _currentReturnType = returnType;
            _isInFunction = true;
            var methodScope = _symbolTable.CreateChildScope();
            VisitBlock(context.block());
            _currentReturnType = null;
            _isInFunction = false;

            return returnType;
        }

        public override Type VisitVarDecl(MiniCSharpParser.VarDeclContext context)
        {
            string varName = context.ID().GetText();
            var varType = GetTypeFromContext(context.type());

            // Verificar que no sea una redefinición
            if (_symbolTable.Lookup(varName) != null)
            {
                _errorCollector.AddError($"Variable '{varName}' ya está definida", context.Start.Line, context.Start.Column);
                return null;
            }

            // Verificar tipo de arreglo
            if (varType is ArrayType arrayType)
            {
                if (!(arrayType.BaseType is SimpleType baseType) || 
                    (baseType.Name != "int" && baseType.Name != "char"))
                {
                    _errorCollector.AddError("Los arreglos solo pueden ser de tipo int o char", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
            }

            // Verificar inicialización
            if (context.expr() != null)
            {
                var exprType = Visit(context.expr());
                if (exprType == null)
                {
                    return null;
                }

                // No permitir asignación directa de arreglos
                if (exprType is ArrayType && varType is ArrayType)
                {
                    _errorCollector.AddError("No se permite la asignación directa de arreglos", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }

                // Verificar tipos compatibles
                if (!exprType.IsAssignableTo(varType))
                {
                    _errorCollector.AddError($"Tipo incompatible en la asignación de '{varName}'", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
            }

            _symbolTable.Define(new VariableSymbol(varName, varType));
            return varType;
        }

        public override Type VisitExpr(MiniCSharpParser.ExprContext context)
        {
            // Operador unario menos
            if (context.ChildCount == 2 && context.GetChild(0).GetText() == "-")
            {
                var operandType = Visit(context.expr(0));
                if (operandType == null || !(operandType is SimpleType st) || st.Name != "int")
                {
                    _errorCollector.AddError("El operador unario '-' solo acepta operandos de tipo int", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
                return new SimpleType("int");
            }

            // Identificador
            if (context.ID() != null)
            {
                string id = context.ID().GetText();
                var symbol = _symbolTable.Lookup(id);
                if (symbol == null)
                {
                    _errorCollector.AddError($"Identificador '{id}' no definido", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
                return symbol.Type;
            }

            // Literal
            if (context.literal() != null)
            {
                return VisitLiteral(context.literal());
            }

            // Llamada a función
            if (context.ID() != null && context.LPAREN() != null)
            {
                string funcName = context.ID().GetText();
                var funcSymbol = _symbolTable.Lookup(funcName) as FunctionSymbol;
                
                if (funcSymbol == null)
                {
                    _errorCollector.AddError($"Función '{funcName}' no definida", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }

                // Verificar argumentos
                var args = context.argList()?.expr() ?? Array.Empty<MiniCSharpParser.ExprContext>();
                if (args.Length != funcSymbol.Parameters.Count)
                {
                    _errorCollector.AddError($"Número incorrecto de argumentos en llamada a '{funcName}'", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }

                for (int i = 0; i < args.Length; i++)
                {
                    var argType = Visit(args[i]);
                    if (argType == null || !argType.IsAssignableTo(funcSymbol.Parameters[i].Type))
                    {
                        _errorCollector.AddError($"Tipo incompatible en argumento {i + 1} de '{funcName}'", 
                            context.Start.Line, context.Start.Column);
                        return null;
                    }
                }

                return funcSymbol.ReturnType;
            }

            // Operaciones binarias
            if (context.expr().Length == 2)
            {
                var leftType = Visit(context.expr(0));
                var rightType = Visit(context.expr(1));

                if (leftType == null || rightType == null)
                {
                    return null;
                }

                // Operadores aritméticos
                if (context.MULOP() != null || context.ADDOP() != null)
                {
                    if (leftType is SimpleType l && rightType is SimpleType r && l.Name == "int" && r.Name == "int")
                    {
                        return new SimpleType("int");
                    }
                    _errorCollector.AddError("Operación aritmética solo acepta operandos de tipo int", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }

                // Operadores relacionales
                if (context.RELOP() != null)
                {
                    string op = context.RELOP().GetText();
                    if (op == "==" || op == "!=")
                    {
                        if (leftType is SimpleType l && rightType is SimpleType r && l.Name == r.Name)
                        {
                            return new SimpleType("bool");
                        }
                        _errorCollector.AddError("Los operandos de '==' y '!=' deben ser del mismo tipo simple", 
                            context.Start.Line, context.Start.Column);
                        return null;
                    }
                    else
                    {
                        if (leftType is SimpleType l && rightType is SimpleType r && l.Name == "int" && r.Name == "int")
                        {
                            return new SimpleType("bool");
                        }
                        _errorCollector.AddError($"El operador relacional '{op}' solo acepta operandos de tipo int", 
                            context.Start.Line, context.Start.Column);
                        return null;
                    }
                }

                // Operadores lógicos
                if (context.LOGOP() != null)
                {
                    if (leftType is SimpleType l && rightType is SimpleType r && l.Name == "bool" && r.Name == "bool")
                    {
                        return new SimpleType("bool");
                    }
                    _errorCollector.AddError("Operación lógica solo acepta operandos de tipo bool", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
            }

            return null;
        }

        public override Type VisitStmt(MiniCSharpParser.StmtContext context)
        {
            // Verificar return
            if (context.RETURN() != null)
            {
                if (!_isInFunction)
                {
                    _errorCollector.AddError("Return solo puede usarse dentro de una función", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }

                if (context.expr() != null)
                {
                    var returnType = Visit(context.expr());
                    if (returnType == null || !returnType.IsAssignableTo(_currentReturnType))
                    {
                        _errorCollector.AddError("Tipo de retorno incompatible con la declaración de la función", 
                            context.Start.Line, context.Start.Column);
                        return null;
                    }
                }
                else if (_currentReturnType != null)
                {
                    _errorCollector.AddError("La función debe retornar un valor", 
                        context.Start.Line, context.Start.Column);
                    return null;
                }
            }

            return base.VisitStmt(context);
        }

        private Type GetTypeFromContext(MiniCSharpParser.TypeContext context)
        {
            if (context.ID() != null)
            {
                return new SimpleType(context.ID().GetText());
            }

            string typeName = context.GetText();
            if (typeName.EndsWith("[]"))
            {
                var baseType = new SimpleType(typeName.Substring(0, typeName.Length - 2));
                return new ArrayType(baseType);
            }

            return new SimpleType(typeName);
        }

        public override Type VisitLiteral(MiniCSharpParser.LiteralContext context)
        {
            if (context.INT() != null) return new SimpleType("int");
            if (context.CHAR() != null) return new SimpleType("char");
            if (context.STRING() != null) return new SimpleType("string");
            if (context.BOOL() != null) return new SimpleType("bool");
            if (context.DOUBLE() != null)
            {
                _errorCollector.AddError("No se permiten constantes de tipo double", 
                    context.Start.Line, context.Start.Column);
                return null;
            }
            return null;
        }

        public bool HasErrors => _errorCollector.HasErrors;
        public void PrintErrors() => _errorCollector.PrintErrors();
    }
} 