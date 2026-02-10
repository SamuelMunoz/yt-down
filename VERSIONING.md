# Versionado Automático con GitVersion

Este proyecto usa **GitVersion** para calcular automáticamente las versiones basándose en la historia de Git.

## ¿Cómo funciona?

GitVersion analiza los commits, tags y ramas para determinar la versión automáticamente:

| Rama | Tag | Incremento | Ejemplo |
|------|-----|------------|---------|
| `main` | (ninguno) | Patch | `1.2.3` |
| `develop` | beta | Minor | `1.3.0-beta.5` |
| `feature/*` | alpha | Inherit | `1.3.0-alpha.3` |
| `release/*` | rc | None | `1.3.0-rc.2` |
| `pull-request/*` | alpha | Inherit | `1.3.0-alpha.4` |

## Versionado Semántico

El formato es: `MAJOR.MINOR.PATCH[-prerelease][+build]`

- **MAJOR**: Cambios breaking (incompatibles)
- **MINOR**: Nuevas features (compatibles)
- **PATCH**: Bug fixes

## Cómo crear un release

### Opción 1: Automático (recomendado)
Cada vez que haces merge a `main`, GitVersion calcula automáticamente la siguiente versión:

```bash
# Desarrolla en develop
git checkout develop
git add .
git commit -m "feat: nueva funcionalidad"
git push origin develop

# Merge a main para release
git checkout main
git merge develop
git push origin main
```

### Opción 2: Manual con tag
Para un release específico:

```bash
# En main o desde un tag
git checkout main
git tag -a v1.2.3 -m "Release version 1.2.3"
git push origin v1.2.3
```

El CI/CD detectará el tag y creará automáticamente el release en GitHub.

## Convención de Commits

GitVersion funciona mejor con **Conventional Commits**:

```
feat: nueva funcionalidad (incrementa MINOR)
fix: corrección de bug (incrementa PATCH)
BREAKING CHANGE: cambio incompatible (incrementa MAJOR)
docs: documentación
style: cambios de formato
refactor: refactorización de código
test: tests
chore: tareas de mantenimiento
```

Ejemplo con breaking change:
```bash
git commit -m "feat: nuevo sistema de autenticación

BREAKING CHANGE: la API de login ha cambiado"
```

## Ejemplos de versiones generadas

```
main (sin tag previo):           0.1.0
main (tag v1.2.3):               1.2.3
develop (desde main):            1.3.0-beta.1
feature/login (desde develop):   1.3.0-alpha.2
release/1.3.0 (desde develop):   1.3.0-rc.1
```

## Artefactos generados

Cada build genera:

### Archivos ZIP (Portables)
- `YtDownloader-v{version}-win-x64.zip` - Versión portable x64
- `YtDownloader-v{version}-win-x86.zip` - Versión portable x86

### Instaladores MSIX
- `YtDownloader-v{version}-x64.msix` - Instalador para Windows x64
- `YtDownloader-v{version}-x86.msix` - Instalador para Windows x86

Los artefactos están disponibles en:
1. **Actions tab**: Descarga desde el workflow run
2. **Releases**: Cuando se crea un tag `v*`

## Instalación con MSIX

### Para desarrollo/testing (certificado de prueba)
El MSIX está firmado con un certificado de prueba. Para instalarlo:

1. Descarga el archivo `.msix` desde GitHub Releases
2. Instala el certificado de prueba primero (solo una vez):
   ```powershell
   # Abrir PowerShell como Administrador
   certutil -addstore -f "Root" "C:\ruta\al\certificado.cer"
   ```
3. Haz doble clic en el archivo `.msix` y confirma la instalación

### Para producción (recomendado)
Para un release oficial, necesitas:
1. Un certificado de firma de código de confianza (ej: de DigiCert, Sectigo)
2. Configurar el secret `CERTIFICATE_PFX` en GitHub con tu certificado base64
3. Actualizar el workflow para usar tu certificado real

O subir la app a Microsoft Store donde se firma automáticamente.
