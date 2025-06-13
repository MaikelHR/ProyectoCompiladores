using System.Collections.Generic;

namespace ProyectoCompi.Semantics
{
    public abstract class Symbol
    {
        public string Name { get; protected set; }
        public Type Type { get; protected set; }
        public bool IsConstant { get; protected set; }
    }

    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, Type type, bool isConstant = false)
        {
            Name = name;
            Type = type;
            IsConstant = isConstant;
        }
    }

    public class FunctionSymbol : Symbol
    {
        public List<ParameterSymbol> Parameters { get; }
        public Type ReturnType { get; }

        public FunctionSymbol(string name, Type returnType, List<ParameterSymbol> parameters)
        {
            Name = name;
            Type = returnType;
            Parameters = parameters;
            IsConstant = true; // Las funciones son siempre constantes
        }
    }

    public class ParameterSymbol : Symbol
    {
        public ParameterSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
            IsConstant = false;
        }
    }

    public class ArraySymbol : Symbol
    {
        public int Dimensions { get; }
        public Type ElementType { get; }

        public ArraySymbol(string name, Type elementType, int dimensions)
        {
            Name = name;
            Type = new ArrayType(elementType);
            ElementType = elementType;
            Dimensions = dimensions;
            IsConstant = false;
        }
    }
} 