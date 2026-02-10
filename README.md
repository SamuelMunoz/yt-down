# YouTube Video Downloader

Aplicación de escritorio WPF en .NET para descargar videos de YouTube y otras plataformas usando yt-dlp.

## Características

- Interfaz moderna y fácil de usar
- Soporte para múltiples formatos:
  - Video en la mejor calidad disponible
  - Solo audio (MP3, M4A)
  - Video en calidad específica (1080p, 720p, 480p, 360p)
- Barra de progreso en tiempo real
- Logs detallados del proceso
- Posibilidad de cancelar descargas
- Pegar URL desde el portapapeles
- Selección de carpeta de destino
- **Recordar última carpeta utilizada** - La aplicación guarda automáticamente la última carpeta de descarga
- **Recordar último formato seleccionado** - Persiste tu preferencia de formato

## Requisitos

1. **.NET 10.0 SDK** o superior
2. **yt-dlp** instalado y disponible en el PATH

### Instalación de yt-dlp

**Windows:**
```powershell
# Usando winget
winget install yt-dlp

# O descargar desde: https://github.com/yt-dlp/yt-dlp/releases
```

**Linux:**
```bash
sudo curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
sudo chmod a+rx /usr/local/bin/yt-dlp
```

**macOS:**
```bash
brew install yt-dlp
```

## Compilación

```bash
# Clonar o navegar al directorio
cd YtDownloader

# Compilar
dotnet build

# O compilar en modo Release
dotnet build -c Release
```

## Ejecución

```bash
# Ejecutar desde código fuente
dotnet run

# O ejecutar el ejecutable compilado
./bin/Debug/net10.0-windows/YtDownloader.exe
```

## Uso

1. **Ingresar URL**: Pega o escribe la URL del video que quieres descargar
2. **Seleccionar formato**: Elige entre video o solo audio
3. **Elegir carpeta**: Selecciona dónde guardar el archivo (se recuerda la última)
4. **Descargar**: Haz clic en "Descargar" y espera a que termine

## Configuración persistente

La aplicación guarda automáticamente:
- Última carpeta de descarga utilizada
- Último formato seleccionado

Los datos se almacenan en: `%AppData%\YtDownloader\settings.json`

## Estructura del proyecto

```
YtDownloader/
├── YtDownloader.csproj    # Archivo de proyecto
├── App.xaml               # Recursos de la aplicación
├── App.xaml.cs            # Código de inicio
├── MainWindow.xaml        # Interfaz de usuario
├── MainWindow.xaml.cs     # Lógica de la aplicación
├── AppSettings.cs         # Gestión de configuración persistente
├── LICENSE                # Licencia MIT
└── README.md              # Este archivo
```

## Notas

- La aplicación verifica automáticamente si yt-dlp está instalado al iniciar
- Puedes descargar videos de múltiples plataformas soportadas por yt-dlp (YouTube, Vimeo, Dailymotion, etc.)
- La conversión a MP3/M4A requiere FFmpeg instalado (yt-dlp lo descarga automáticamente si no está presente)

## Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.

Copyright (c) 2026
