$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$appOut = Join-Path $root "ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe"
$installerOut = Join-Path $root "INSTALADOR_ZAETTA_CAPTURE_FINAL.exe"
$icon = Join-Path $root "ZAETTA_CAPTURE\zaetta_icon.ico"
$logo = Join-Path $root "ZAETTA_CAPTURE\logo_oficial.png"
$installerSource = Join-Path $root "ZAETTA_CAPTURE_NATIVE\InstallerZaettaFinal.cs"

$appSources = Get-ChildItem (Join-Path $root "ZAETTA_CAPTURE_NATIVE") -Recurse -Filter "*.cs" |
    Where-Object { $_.Name -ne "InstallerZaettaFinal.cs" } |
    ForEach-Object { $_.FullName }

& $csc /nologo /target:winexe "/out:$appOut" "/win32icon:$icon" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $appSources

& $csc /nologo /target:winexe "/out:$installerOut" "/win32icon:$icon" "/resource:$appOut,ZaettaApp" "/resource:$logo,ZaettaLogo" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $installerSource
