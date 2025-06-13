using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoCompi.Semantics
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols;
        private readonly SymbolTable _parent;
        private readonly List<SymbolTable> _children;

        public SymbolTable(SymbolTable parent = null)
        {
            _symbols = new Dictionary<string, Symbol>();
            _parent = parent;
            _children = new List<SymbolTable>();
            if (parent != null)
            {
                parent._children.Add(this);
            }
        }

        public void Define(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
            {
                throw new Exception($"Symbol '{symbol.Name}' is already defined in this scope");
            }
            _symbols[symbol.Name] = symbol;
        }

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

        public void InitializeBuiltInSymbols()
        {
            // Tipos básicos
            var intType = new SimpleType("int");
            var charType = new SimpleType("char");
            var stringType = new SimpleType("string");
            var boolType = new SimpleType("bool");

            // Funciones predefinidas
            Define(new FunctionSymbol("ord", intType, new List<ParameterSymbol> { new ParameterSymbol("c", charType) }));
            Define(new FunctionSymbol("chr", charType, new List<ParameterSymbol> { new ParameterSymbol("i", intType) }));
            Define(new FunctionSymbol("len", intType, new List<ParameterSymbol> { new ParameterSymbol("arr", new ArrayType(intType)) }));
            Define(new FunctionSymbol("add", new ArrayType(intType), new List<ParameterSymbol> 
            { 
                new ParameterSymbol("arr", new ArrayType(intType)),
                new ParameterSymbol("value", intType)
            }));
            Define(new FunctionSymbol("del", new ArrayType(intType), new List<ParameterSymbol> 
            { 
                new ParameterSymbol("arr", new ArrayType(intType)),
                new ParameterSymbol("index", intType)
            }));

            // Constantes predefinidas
            Define(new VariableSymbol("null", new SimpleType("null"), true));
        }

        public SymbolTable CreateChildScope()
        {
            return new SymbolTable(this);
        }

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