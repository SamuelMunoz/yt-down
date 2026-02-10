# Script para generar imágenes placeholder para el instalador MSIX
# Ejecutar con: Add-Type -AssemblyName System.Drawing antes de correr este script
# O usar en GitHub Actions donde el assembly está disponible

param(
    [string]$OutputDir = "./Installer/Images"
)

# Cargar assembly de System.Drawing
Add-Type -AssemblyName System.Drawing

# Crear directorio si no existe
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Función para crear imagen con texto
function Create-PlaceholderImage {
    param(
        [int]$Width,
        [int]$Height,
        [string]$Text,
        [string]$OutputPath
    )
    
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Fondo rojo (estilo YouTube)
    $graphics.Clear([System.Drawing.Color]::FromArgb(255, 255, 0, 0))
    
    # Texto
    $fontSize = [math]::Max(8, [math]::Min($Width, $Height) / 4)
    $font = New-Object System.Drawing.Font("Arial", $fontSize, [System.Drawing.FontStyle]::Bold)
    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
    
    $graphics.DrawString(
        $Text,
        $font,
        [System.Drawing.Brushes]::White,
        $Width / 2,
        $Height / 2,
        $stringFormat
    )
    
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
    
    Write-Host "Created: $OutputPath (${Width}x${Height})" -ForegroundColor Green
}

# Generar imágenes necesarias para MSIX
$images = @(
    @{ Name = "StoreLogo.png"; Width = 50; Height = 50; Text = "YT" },
    @{ Name = "Square44x44Logo.png"; Width = 44; Height = 44; Text = "YT" },
    @{ Name = "Square150x150Logo.png"; Width = 150; Height = 150; Text = "YT" },
    @{ Name = "Wide310x150Logo.png"; Width = 310; Height = 150; Text = "YtDownloader" }
)

foreach ($img in $images) {
    $outputPath = Join-Path $OutputDir $img.Name
    Create-PlaceholderImage -Width $img.Width -Height $img.Height -Text $img.Text -OutputPath $outputPath
}

Write-Host "`nPlaceholder images generated successfully!" -ForegroundColor Green
Write-Host "Replace these images with your actual app icons before releasing." -ForegroundColor Yellow
