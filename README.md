# Zaetta Capture

Capturador de pantalla interno tipo Lightshot, desarrollado para uso operativo.

## Estructura

- `ZAETTA_CAPTURE_NATIVE/`: version nativa en C# WinForms.
- `ZAETTA_CAPTURE/`: recursos e iconos.
- `CONTEXTO_ZAETTA_CAPTURE.md`: contexto completo para retomar desarrollo.

## Compilar app

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /out:'.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe' /win32icon:'.\ZAETTA_CAPTURE\zaetta_icon.ico' /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll '.\ZAETTA_CAPTURE_NATIVE\ZaettaCapture.cs'
```

## Compilar instalador

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /out:'.\INSTALADOR_ZAETTA_CAPTURE_FINAL.exe' /win32icon:'.\ZAETTA_CAPTURE\zaetta_icon.ico' "/resource:.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe,ZaettaApp" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll '.\ZAETTA_CAPTURE_NATIVE\InstallerZaettaFinal.cs'
```
