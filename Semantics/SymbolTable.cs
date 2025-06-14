using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoCompi.Semantics
{
    /// <summary>
    /// Clase base abstracta para todos los tipos del lenguaje (simples y compuestos).
    /// </summary>
    public abstract class SymbolType
    {
        /// <summary>
        /// Determina si el tipo actual puede asignarse al tipo 'other'.
        /// </summary>
        public abstract bool IsAssignableTo(SymbolType other);
    }

    /// <summary>
    /// Representa un tipo simple (int, char, string, bool, void, etc.).
    /// </summary>
    public class SimpleType : SymbolType
    {
        public string Name { get; }

        public SimpleType(string name)
        {
            Name = name;
        }

        public override bool IsAssignableTo(SymbolType other)
        {
            return other is SimpleType simpleType && Name == simpleType.Name;
        }
    }

    /// <summary>
    /// Representa un tipo de arreglo (solo de int o char).
    /// </summary>
    public class ArrayType : SymbolType
    {
        public SymbolType BaseType { get; }

        public ArrayType(SymbolType baseType)
        {
            // Solo se permiten arreglos de int o char
            if (baseType is SimpleType simpleType)
            {
                if (simpleType.Name != "int" && simpleType.Name != "char")
                {
                    throw new Exception("Arrays can only be of type int or char");
                }
            }
            else
            {
                throw new Exception("Arrays cannot be of class type");
            }
            BaseType = baseType;
        }

        public override bool IsAssignableTo(SymbolType other)
        {
            return other is ArrayType arrayType && BaseType.IsAssignableTo(arrayType.BaseType);
        }
    }

    /// <summary>
    /// Clase base para todos los símbolos (variables, parámetros, funciones).
    /// </summary>
    public abstract class Symbol
    {
        public string Name { get; }
        public SymbolType Type { get; }

        protected Symbol(string name, SymbolType type)
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// Representa una variable en la tabla de símbolos.
    /// </summary>
    public class VariableSymbol : Symbol
    {
        public bool IsConstant { get; }

        public VariableSymbol(string name, SymbolType type, bool isConstant = false)
            : base(name, type)
        {
            IsConstant = isConstant;
        }
    }

    /// <summary>
    /// Representa un parámetro de función o método.
    /// </summary>
    public class ParameterSymbol : Symbol
    {
        public ParameterSymbol(string name, SymbolType type) : base(name, type) { }
    }

    /// <summary>
    /// Representa una función o método en la tabla de símbolos.
    /// </summary>
    public class FunctionSymbol : Symbol
    {
        public List<ParameterSymbol> Parameters { get; }
        public bool IsBuiltIn { get; }

        public FunctionSymbol(string name, SymbolType returnType, List<ParameterSymbol> parameters, bool isBuiltIn = false)
            : base(name, returnType)
        {
            Parameters = parameters;
            IsBuiltIn = isBuiltIn;
        }
    }

    /// <summary>
    /// Tabla de símbolos con soporte para ámbitos anidados.
    /// </summary>
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols;
        private readonly SymbolTable _parent;
        private readonly List<SymbolTable> _children;
        private readonly bool _isGlobalScope;

        /// <summary>
        /// Crea una nueva tabla de símbolos. Si es global, inicializa los símbolos predefinidos.
        /// </summary>
        public SymbolTable(SymbolTable parent = null, bool isGlobalScope = false)
        {
            _symbols = new Dictionary<string, Symbol>();
            _parent = parent;
            _children = new List<SymbolTable>();
            _isGlobalScope = isGlobalScope;

            if (parent != null)
            {
                parent._children.Add(this);
            }

            if (isGlobalScope)
            {
                InitializeBuiltInSymbols();
            }
        }

        /// <summary>
        /// Inicializa los símbolos y funciones predefinidas del lenguaje.
        /// </summary>
        private void InitializeBuiltInSymbols()
        {
            // Tipos básicos
            var intType = new SimpleType("int");
            var charType = new SimpleType("char");
            var stringType = new SimpleType("string");
            var boolType = new SimpleType("bool");

            // Constante null
            Define(new VariableSymbol("null", new SimpleType("null"), true));

            // Funciones predefinidas
            Define(new FunctionSymbol("chr", charType, new List<ParameterSymbol> { new ParameterSymbol("i", intType) }, true));
            Define(new FunctionSymbol("ord", intType, new List<ParameterSymbol> { new ParameterSymbol("ch", charType) }, true));
            Define(new FunctionSymbol("len", intType, new List<ParameterSymbol> { new ParameterSymbol("a", new ArrayType(intType)) }, true));
            Define(new FunctionSymbol("add", new SimpleType("void"), new List<ParameterSymbol> { new ParameterSymbol("e", intType) }, true));
            Define(new FunctionSymbol("del", new SimpleType("void"), new List<ParameterSymbol> { new ParameterSymbol("i", intType) }, true));
        }

        /// <summary>
        /// Crea un nuevo ámbito hijo.
        /// </summary>
        public SymbolTable CreateChildScope()
        {
            return new SymbolTable(this);
        }

        /// <summary>
        /// Define un nuevo símbolo en el ámbito actual.
        /// </summary>
        public void Define(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
            {
                throw new Exception($"Symbol '{symbol.Name}' is already defined in this scope");
            }
            _symbols[symbol.Name] = symbol;
        }

        /// <summary>
        /// Busca un símbolo por nombre en el ámbito actual y padres (si searchParent es true).
        /// </summary>
        public Symbol Lookup(string name, bool searchParent = true)
        {
            if (_symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }

            if (searchParent && _parent != null)
            {
                return _parent.Lookup(name, true);
            }

            return null;
        }

        /// <summary>
        /// Busca un símbolo solo en el ámbito actual.
        /// </summary>
        public Symbol LookupInCurrentScope(string name)
        {
            _symbols.TryGetValue(name, out var symbol);
            return symbol;
        }

        /// <summary>
        /// Limpia todos los símbolos de este ámbito y sus hijos.
        /// </summary>
        public void Clear()
        {
            _symbols.Clear();
            foreach (var child in _children)
            {
                child.Clear();
            }
        }
    }
} 