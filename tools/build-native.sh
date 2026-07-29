#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
app_out="$root/ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe"
installer_out="$root/INSTALADOR_ZAETTA_CAPTURE_FINAL.exe"
icon="$root/ZAETTA_CAPTURE/zaetta_icon.ico"
logo="$root/ZAETTA_CAPTURE/logo_oficial.png"
installer_source="$root/ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs"

if ! command -v mcs >/dev/null 2>&1; then
  echo "No se encontro mcs. Instala Mono primero: sudo apt-get install -y mono-devel" >&2
  exit 1
fi

mapfile -t app_sources < <(
  find "$root/ZAETTA_CAPTURE_NATIVE" -name '*.cs' ! -name 'InstallerZaettaFinal.cs' | sort
)

mcs \
  -nologo \
  -target:winexe \
  "-out:$app_out" \
  "-win32icon:$icon" \
  -r:System.dll \
  -r:System.Drawing.dll \
  -r:System.Windows.Forms.dll \
  "${app_sources[@]}"

mcs \
  -nologo \
  -target:winexe \
  "-out:$installer_out" \
  "-win32icon:$icon" \
  "-resource:$app_out,ZaettaApp" \
  "-resource:$logo,ZaettaLogo" \
  -r:System.dll \
  -r:System.Drawing.dll \
  -r:System.Windows.Forms.dll \
  "$installer_source"
