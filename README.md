# Zaetta Capture

Capturador de pantalla interno tipo Lightshot, desarrollado para uso operativo.

## Estructura

- `ZAETTA_CAPTURE_NATIVE/`: version nativa en C# WinForms.
- `ZAETTA_CAPTURE_NATIVE/App/`: arranque, metadata, bandeja y hotkeys.
- `ZAETTA_CAPTURE_NATIVE/Capture/`: overlay/editor principal de captura.
- `ZAETTA_CAPTURE_NATIVE/Editing/`: herramientas y operaciones de dibujo.
- `ZAETTA_CAPTURE_NATIVE/Storage/`: historial, ultima area y preferencias.
- `ZAETTA_CAPTURE_NATIVE/SystemIntegration/`: DPI, portapapeles, hotkeys y diagnostico.
- `ZAETTA_CAPTURE_NATIVE/UI/`: controles visuales compartidos.
- `ZAETTA_CAPTURE_NATIVE/Legacy/`: editor anterior conservado como referencia.
- `ZAETTA_CAPTURE/`: recursos e iconos.
- `CONTEXTO_ZAETTA_CAPTURE.md`: contexto completo para retomar desarrollo.

## Funciones clave

- Captura tipo Lightshot desde bandeja o atajo.
- Herramientas de anotacion, texto, numero, pixelado y mover.
- Opcion chuleable `Mantener posicion del area seleccionada`.
- Opcion `Repetir ultima area`.
- Historial local en `Pictures\Zaetta Capture\Historial`.
- Instalador local en `%LOCALAPPDATA%`.

## Compilar

```powershell
.\tools\build-native.ps1
```

En Linux con Mono instalado:

```bash
./tools/build-native.sh
```

Ambos scripts generan:

- `ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe`
- `INSTALADOR_ZAETTA_CAPTURE_FINAL.exe`

En Linux se requiere:

```bash
sudo apt-get install -y mono-devel
```

El instalador embebe:

- `ZaettaApp`: app compilada.
- `ZaettaLogo`: `ZAETTA_CAPTURE/logo_oficial.png`.

Si la app falla al iniciar, revisar:

```text
Pictures\Zaetta Capture\startup-error.log
```
