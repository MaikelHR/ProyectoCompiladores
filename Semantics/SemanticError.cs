using System;

namespace ProyectoCompi.Semantics
{
    public class SemanticError : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public SemanticError(string message, int line, int column) 
            : base($"Error semántico en línea {line}, columna {column}: {message}")
        {
            Line = line;
            Column = column;
        }
    }

    public class SemanticErrorCollector
    {
        private readonly List<SemanticError> _errors;

        public SemanticErrorCollector()
        {
            _errors = new List<SemanticError>();
        }

        public void AddError(string message, int line, int column)
        {
            _errors.Add(new SemanticError(message, line, column));
        }

        public bool HasErrors => _errors.Count > 0;

        public void PrintErrors()
        {
            foreach (var error in _errors)
            {
                Console.WriteLine(error.Message);
            }
        }

        public void Clear()
        {
            _errors.Clear();
        }
    }
} 