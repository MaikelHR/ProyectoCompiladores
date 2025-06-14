using System;
using System.IO;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ProyectoCompi.Semantics;
using ProyectoCompi.Gen;

namespace ProyectoCompi
{
    class Program
    {
        private class SyntaxErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
        {
            private readonly List<string> _errors = new List<string>();

            public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                _errors.Add($"Error de sintaxis en línea {line}, posición {charPositionInLine}: {msg}");
            }

            public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                _errors.Add($"Error de sintaxis en línea {line}, posición {charPositionInLine}: {msg}");
            }

            public bool HasErrors => _errors.Count > 0;
            public IEnumerable<string> Errors => _errors;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Por favor, especifica el archivo a compilar");
                return;
            }

            string inputFile = args[0];
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"El archivo {inputFile} no existe");
                return;
            }

            try
            {
                // Leer el archivo de entrada
                string input = File.ReadAllText(inputFile);
                var inputStream = new AntlrInputStream(input);
                
                // Crear el lexer con manejador de errores
                var lexer = new MiniCSharpLexer(inputStream);
                var syntaxErrorListener = new SyntaxErrorListener();
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(syntaxErrorListener);
                
                var tokens = new CommonTokenStream(lexer);
                
                // Crear el parser con manejador de errores
                var parser = new MiniCSharpParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(syntaxErrorListener);
                
                var tree = parser.program();
                
                // Imprimir el árbol de sintaxis
                Console.WriteLine("\nÁrbol de sintaxis:");
                PrintTree(tree, parser);
                
                // Generar el código
                string outputFile = Path.ChangeExtension(inputFile, ".exe");
                var codeGenerator = new CodeGeneratorVisitor(Path.GetFileNameWithoutExtension(inputFile));
                codeGenerator.Visit(tree);

                // Mostrar todos los errores encontrados
                bool hasErrors = false;
                
                if (syntaxErrorListener.HasErrors)
                {
                    hasErrors = true;
                    Console.WriteLine("\nErrores de sintaxis encontrados:");
                    foreach (var error in syntaxErrorListener.Errors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                }
                
                if (codeGenerator.HasErrors())
                {
                    hasErrors = true;
                    Console.WriteLine("\nErrores semánticos encontrados:");
                    foreach (var error in codeGenerator.GetErrors())
                    {
                        Console.WriteLine($"- {error}");
                    }
                }

                if (!hasErrors)
                {
                    Console.WriteLine($"\nEl archivo {inputFile} es sintáctica y semánticamente correcto");
                    codeGenerator.SaveAssembly(outputFile);
                    Console.WriteLine($"\nCódigo generado exitosamente en: {outputFile}");
                }
                else
                {
                    Console.WriteLine("\nEl archivo contiene errores. Por favor, corrija los errores antes de continuar.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el archivo: {ex.Message}");
            }
        }

        private static void PrintTree(IParseTree tree, Parser parser, string indent = "", bool last = true)
        {
            // Obtener el texto del nodo y su tipo
            string nodeText = Antlr4.Runtime.Tree.Trees.GetNodeText(tree, parser);
            string nodeType = tree.GetType().Name.Replace("Context", "");

            // Construir la línea con el formato: [Tipo] Texto
            string line = $"[{nodeType}] {nodeText}";
            
            // Imprimir la línea con el formato del árbol
            Console.WriteLine(indent + (last ? "└─ " : "├─ ") + line);

            // Imprimir los hijos
            for (int i = 0; i < tree.ChildCount; i++)
            {
                PrintTree(tree.GetChild(i), parser, indent + (last ? "    " : "│   "), i == tree.ChildCount - 1);
            }
        }
    }
}
