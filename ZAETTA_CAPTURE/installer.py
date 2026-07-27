import os
import base64
import shutil
import subprocess
import sys
import threading
import time
from pathlib import Path

if getattr(sys, "frozen", False) and hasattr(sys, "_MEIPASS"):
    bundle_root = Path(sys._MEIPASS)
    tcl_root = bundle_root / "tcl"
    tcl_candidates = sorted(tcl_root.glob("tcl*")) if tcl_root.exists() else []
    tk_candidates = sorted(tcl_root.glob("tk*")) if tcl_root.exists() else []
    if tcl_candidates:
        os.environ["TCL_LIBRARY"] = str(tcl_candidates[0])
    if tk_candidates:
        os.environ["TK_LIBRARY"] = str(tk_candidates[0])

import tkinter as tk
from tkinter import messagebox

from PIL import Image, ImageDraw, ImageTk


APP_NAME = "Zaetta Capture"
APP_VERSION = "1.0"
EXE_NAME = "Zaetta Capture 1.0.exe"
INSTALL_DIR = Path(os.environ.get("LOCALAPPDATA", str(Path.home()))) / "Zaetta Capture"
DESKTOP = Path(os.environ.get("USERPROFILE", str(Path.home()))) / "Desktop"

BG = "#eef5f8"
CARD = "#ffffff"
TEXT = "#111827"
MUTED = "#607082"
ACCENT = "#15add8"
ACCENT_DARK = "#0f2942"
LINE = "#d6e7ef"


def bundle_path(name):
    base = Path(getattr(sys, "_MEIPASS", Path(__file__).resolve().parent))
    return base / name


def make_icon(size=84):
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    scale = size / 84

    def p(points):
        return [(int(x * scale), int(y * scale)) for x, y in points]

    draw.ellipse((3 * scale, 3 * scale, 81 * scale, 81 * scale), fill="#ffffff", outline="#d6e7ef", width=max(1, int(2 * scale)))
    draw.ellipse((16 * scale, 16 * scale, 68 * scale, 68 * scale), fill=ACCENT_DARK)
    draw.polygon(p([(23, 18), (41, 18), (41, 66), (23, 66)]), fill=ACCENT)
    draw.polygon(p([(41, 17), (69, 42), (41, 67)]), fill="#ffffff")
    draw.line(p([(14, 74), (70, 14)]), fill=ACCENT, width=max(2, int(5 * scale)))
    draw.polygon(p([(66, 11), (78, 7), (74, 20)]), fill=ACCENT)
    return image


class ProgressBar(tk.Canvas):
    def __init__(self, parent, width=430, height=14):
        super().__init__(parent, width=width, height=height, bg=parent.cget("bg"), highlightthickness=0)
        self.width_value = width
        self.height_value = height
        self.progress = 0.0
        self.draw()

    def rounded_rect(self, x1, y1, x2, y2, radius, **kwargs):
        points = [
            x1 + radius, y1, x2 - radius, y1, x2, y1, x2, y1 + radius,
            x2, y2 - radius, x2, y2, x2 - radius, y2, x1 + radius, y2,
            x1, y2, x1, y2 - radius, x1, y1 + radius, x1, y1,
        ]
        self.create_polygon(points, smooth=True, splinesteps=24, **kwargs)

    def set(self, value):
        self.progress = max(0.0, min(1.0, value))
        self.draw()

    def draw(self):
        self.delete("all")
        self.rounded_rect(0, 0, self.width_value, self.height_value, 7, fill="#dceaf1", outline="")
        fill_w = max(self.height_value, int(self.width_value * self.progress))
        self.rounded_rect(0, 0, fill_w, self.height_value, 7, fill=ACCENT, outline="")


class PillButton(tk.Canvas):
    def __init__(self, parent, text, command, width=150, height=42, bg=ACCENT, fg="#ffffff"):
        super().__init__(parent, width=width, height=height, bg=parent.cget("bg"), highlightthickness=0, cursor="hand2")
        self.text = text
        self.command = command
        self.width_value = width
        self.height_value = height
        self.bg_color = bg
        self.hover = "#20c4f4" if bg == ACCENT else "#e5f1f6"
        self.fg = fg
        self.current = bg
        self.bind("<Button-1>", lambda _event: self.command())
        self.bind("<Enter>", self._enter)
        self.bind("<Leave>", self._leave)
        self.draw()

    def rounded_rect(self, x1, y1, x2, y2, radius, **kwargs):
        points = [
            x1 + radius, y1, x2 - radius, y1, x2, y1, x2, y1 + radius,
            x2, y2 - radius, x2, y2, x2 - radius, y2, x1 + radius, y2,
            x1, y2, x1, y2 - radius, x1, y1 + radius, x1, y1,
        ]
        self.create_polygon(points, smooth=True, splinesteps=24, **kwargs)

    def draw(self):
        self.delete("all")
        self.rounded_rect(1, 1, self.width_value - 1, self.height_value - 1, 14, fill=self.current, outline="")
        self.create_text(self.width_value / 2, self.height_value / 2, text=self.text, fill=self.fg, font=("Segoe UI", 10, "bold"))

    def _enter(self, _event):
        self.current = self.hover
        self.draw()

    def _leave(self, _event):
        self.current = self.bg_color
        self.draw()


class Installer(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(f"Instalador - {APP_NAME}")
        self.geometry("560x390")
        self.resizable(False, False)
        self.configure(bg=BG)
        self.attributes("-alpha", 0.97)
        self.icon_image = ImageTk.PhotoImage(make_icon(64))
        self.iconphoto(True, self.icon_image)
        self.source_exe = bundle_path(EXE_NAME)
        self._build()

    def _build(self):
        outer = tk.Frame(self, bg=BG, padx=26, pady=24)
        outer.pack(fill="both", expand=True)

        header = tk.Frame(outer, bg=BG)
        header.pack(fill="x")
        tk.Label(header, image=self.icon_image, bg=BG).pack(side="left", padx=(0, 16))
        title = tk.Frame(header, bg=BG)
        title.pack(side="left", fill="x", expand=True)
        tk.Label(title, text="Zaetta Capture", bg=BG, fg=TEXT, font=("Segoe UI", 25, "bold")).pack(anchor="w")
        tk.Label(title, text="Instalador local, rapido y limpio.", bg=BG, fg=MUTED, font=("Segoe UI", 10)).pack(anchor="w", pady=(2, 0))

        self.card = tk.Frame(outer, bg=CARD, padx=22, pady=22, highlightthickness=1, highlightbackground=LINE)
        self.card.pack(fill="x", pady=(24, 0))

        self.status = tk.Label(self.card, text="Listo para instalar", bg=CARD, fg=TEXT, font=("Segoe UI", 13, "bold"))
        self.status.pack(anchor="w")
        self.detail = tk.Label(
            self.card,
            text="Se copiara la aplicacion y se creara un acceso directo en el escritorio.",
            bg=CARD,
            fg=MUTED,
            font=("Segoe UI", 10),
            wraplength=440,
            justify="left",
        )
        self.detail.pack(anchor="w", pady=(8, 18))

        self.progress = ProgressBar(self.card, width=470, height=14)
        self.progress.pack(anchor="w")

        actions = tk.Frame(outer, bg=BG)
        actions.pack(fill="x", pady=(24, 0))
        self.install_button = PillButton(actions, "Instalar", self.start_install, width=150)
        self.install_button.pack(side="right")
        self.close_button = PillButton(actions, "Cerrar", self.destroy, width=130, bg="#e8f2f7", fg=TEXT)
        self.close_button.pack(side="right", padx=(0, 12))

    def start_install(self):
        self.install_button.configure(state="disabled")
        threading.Thread(target=self._install, daemon=True).start()

    def _set_status(self, text, detail, progress):
        self.after(0, lambda: self.status.configure(text=text))
        self.after(0, lambda: self.detail.configure(text=detail))
        self.after(0, lambda: self.progress.set(progress))

    def _install(self):
        try:
            steps = [
                ("Preparando instalacion", "Validando archivos necesarios.", 0.12),
                ("Copiando aplicacion", f"Instalando en {INSTALL_DIR}.", 0.42),
                ("Creando acceso directo", "Preparando acceso directo en el escritorio.", 0.72),
                ("Finalizando", "Guardando configuracion final.", 0.92),
            ]
            for title, detail, progress in steps:
                self._set_status(title, detail, progress)
                time.sleep(0.35)

            if not self.source_exe.exists():
                raise FileNotFoundError(f"No se encontro {EXE_NAME} dentro del instalador.")

            INSTALL_DIR.mkdir(parents=True, exist_ok=True)
            target = INSTALL_DIR / EXE_NAME
            shutil.copy2(self.source_exe, target)
            shortcut_ok = self._create_shortcut(target)
            detail = "Zaetta Capture quedo listo en el escritorio."
            if not shortcut_ok:
                detail = "Zaetta Capture quedo instalado. Se creo un lanzador alterno en el escritorio."
            self._set_status("Instalacion completada", detail, 1.0)
            self.after(0, self._finish)
        except Exception as exc:
            self.after(0, lambda: self.install_button.configure(state="normal"))
            self._set_status("La instalacion fallo", str(exc), 0.0)
            self.after(0, lambda: messagebox.showerror(APP_NAME, str(exc), parent=self))

    def _create_shortcut(self, target):
        shortcut = DESKTOP / "Zaetta Capture.lnk"
        try:
            shortcut.parent.mkdir(parents=True, exist_ok=True)
            shortcut_text = str(shortcut).replace("'", "''")
            target_text = str(target).replace("'", "''")
            working_text = str(target.parent).replace("'", "''")
            ps = f"""
$ErrorActionPreference = 'Stop'
$shortcutPath = '{shortcut_text}'
$targetPath = '{target_text}'
$workingDir = '{working_text}'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetPath
$shortcut.WorkingDirectory = $workingDir
$shortcut.Description = 'Zaetta Capture'
$shortcut.IconLocation = $targetPath
$shortcut.Save()
"""
            encoded = base64.b64encode(ps.encode("utf-16le")).decode("ascii")
            subprocess.run(
                ["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-EncodedCommand", encoded],
                check=True,
                creationflags=0x08000000,
            )
            return True
        except Exception:
            launcher = DESKTOP / "Zaetta Capture.cmd"
            launcher.write_text(f'@echo off\nstart "" "{target}"\n', encoding="utf-8")
            return False

    def _finish(self):
        self.install_button.destroy()
        self.close_button.destroy()
        actions = tk.Frame(self, bg=BG)
        actions.place(relx=1.0, rely=1.0, x=-26, y=-24, anchor="se")
        PillButton(actions, "Finalizar", self.destroy, width=150).pack(side="right")


if __name__ == "__main__":
    Installer().mainloop()
