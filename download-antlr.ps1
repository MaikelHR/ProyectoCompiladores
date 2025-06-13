# Crear directorio para ANTLR si no existe
if (-not (Test-Path "antlr")) {
    New-Item -ItemType Directory -Path "antlr"
}

# Descargar ANTLR4
$url = "https://www.antlr.org/download/antlr-4.13.1-complete.jar"
$output = "antlr/antlr-4.13.1-complete.jar"
Invoke-WebRequest -Uri $url -OutFile $output

Write-Host "ANTLR4 descargado exitosamente en $output" 