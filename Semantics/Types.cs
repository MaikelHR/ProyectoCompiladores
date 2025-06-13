using System;

namespace ProyectoCompi.Semantics
{
    public abstract class Type
    {
        public string Name { get; protected set; }
        public bool IsArray { get; protected set; }
        public Type BaseType { get; protected set; }

        public abstract bool IsAssignableTo(Type other);
    }

    public class SimpleType : Type
    {
        public SimpleType(string name)
        {
            Name = name;
            IsArray = false;
        }

        public override bool IsAssignableTo(Type other)
        {
            if (other is SimpleType simpleType)
            {
                return Name == simpleType.Name;
            }
            return false;
        }
    }

    public class ArrayType : Type
    {
        public ArrayType(Type baseType)
        {
            Name = $"{baseType.Name}[]";
            IsArray = true;
            BaseType = baseType;
        }

        public override bool IsAssignableTo(Type other)
        {
            if (other is ArrayType arrayType)
            {
                return BaseType.IsAssignableTo(arrayType.BaseType);
            }
            return false;
        }
    }
} 