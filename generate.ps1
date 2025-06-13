# Asegurarse de que estamos en el directorio correcto
Set-Location $PSScriptRoot

# Crear directorio CodeGen si no existe
if (-not (Test-Path "CodeGen")) {
    New-Item -ItemType Directory -Path "CodeGen"
}

# Generar archivos usando el JAR de ANTLR4
java -jar antlr/antlr-4.13.1-complete.jar -Dlanguage=CSharp -visitor -o CodeGen Grammar/MiniCSharpLexer.g4
java -jar antlr/antlr-4.13.1-complete.jar -Dlanguage=CSharp -visitor -o CodeGen Grammar/MiniCSharpParser.g4 