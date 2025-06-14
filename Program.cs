using System;
using System.IO;
using Antlr4.Runtime;
using ProyectoCompi.Semantics;
using ProyectoCompi.Gen;

namespace ProyectoCompi
{
    class Program
    {
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
                
                // Crear el lexer
                var lexer = new MiniCSharpLexer(inputStream);
                var tokens = new CommonTokenStream(lexer);
                
                // Crear el parser
                var parser = new MiniCSharpParser(tokens);
                var tree = parser.program();
                
                Console.WriteLine($"\nEl archivo {inputFile} es semánticamente correcto");
                
                // Generar el código
                string outputFile = Path.ChangeExtension(inputFile, ".exe");
                var codeGenerator = new CodeGeneratorVisitor(Path.GetFileNameWithoutExtension(inputFile));
                codeGenerator.Visit(tree);
                codeGenerator.SaveAssembly(outputFile);
                
                Console.WriteLine($"\nCódigo generado exitosamente en: {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el archivo: {ex.Message}");
            }
        }
    }
}
