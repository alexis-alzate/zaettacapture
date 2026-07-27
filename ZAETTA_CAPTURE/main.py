import ctypes
import io
import json
import os
import getpass
import socket
import sys
import threading
from ctypes import wintypes
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path

if getattr(sys, "frozen", False) and hasattr(sys, "_MEIPASS"):
    bundle_root = Path(sys._MEIPASS)
    tcl_root = bundle_root / "tcl"
    tcl_lib = tcl_root / "tcl8.6"
    tk_lib = tcl_root / "tk8.6"
    if tcl_lib.exists():
        os.environ["TCL_LIBRARY"] = str(tcl_lib)
    if tk_lib.exists():
        os.environ["TK_LIBRARY"] = str(tk_lib)

import tkinter as tk
from tkinter import filedialog, messagebox, simpledialog

try:
    from PIL import Image, ImageDraw, ImageFont, ImageGrab, ImageTk
except Exception as exc:
    raise SystemExit(
        "Zaetta Capture requiere Pillow.\n"
        "Instale dependencias con: python -m pip install -r requirements.txt\n\n"
        f"Detalle: {exc}"
    )


APP_NAME = "Zaetta Capture"
APP_VERSION = "1.0"
DEFAULT_DIR = Path.home() / "Pictures" / "Zaetta Capture"
HISTORY_DIR = DEFAULT_DIR / "Historial"
CONFIG_PATH = DEFAULT_DIR / "config.json"

BG = "#071019"
PANEL = "#101d29"
PANEL_2 = "#172a3a"
TEXT = "#f6fbff"
MUTED = "#9eb3c6"
ACCENT = "#15add8"
ACCENT_2 = "#20c4f4"
DANGER = "#ff5757"
WARNING = "#ffd166"

MINIMAL_BG = "#f7fafc"
MINIMAL_PANEL = "#ffffff"
MINIMAL_SOFT = "#eef6fa"
MINIMAL_TEXT = "#111827"
MINIMAL_MUTED = "#5d6b7a"
MINIMAL_LINE = "#d7e8f0"


@dataclass
class DrawOp:
    kind: str
    points: list
    color: str
    width: int
    text: str = ""


def make_zaetta_icon(size=64):
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    scale = size / 64

    def p(points):
        return [(int(x * scale), int(y * scale)) for x, y in points]

    draw.ellipse((3 * scale, 3 * scale, 61 * scale, 61 * scale), fill="#ffffff", outline="#d7e8f0", width=max(1, int(1.5 * scale)))
    draw.ellipse((11 * scale, 11 * scale, 53 * scale, 53 * scale), fill="#0f2942")
    draw.polygon(p([(17, 14), (31, 14), (31, 50), (17, 50)]), fill="#15add8")
    draw.polygon(p([(31, 13), (52, 32), (31, 51)]), fill="#ffffff")
    draw.line(p([(11, 55), (53, 10)]), fill="#15add8", width=max(2, int(4 * scale)))
    draw.polygon(p([(49, 8), (58, 5), (55, 15)]), fill="#15add8")
    return image


class RoundedButton(tk.Canvas):
    def __init__(self, parent, text, command, *, width=150, height=40, bg=ACCENT, fg="white", hover=None):
        super().__init__(
            parent,
            width=width,
            height=height,
            bg=parent.cget("bg"),
            highlightthickness=0,
            bd=0,
            cursor="hand2",
        )
        self.text = text
        self.command = command
        self.width_value = width
        self.height_value = height
        self.normal = bg
        self.hover = hover or (ACCENT_2 if bg == ACCENT else "#20364a")
        self.fg = fg
        self.current = self.normal
        self.bind("<Enter>", self._enter)
        self.bind("<Leave>", self._leave)
        self.bind("<Button-1>", lambda _event: self.command())
        self.draw()

    def rounded_rect(self, x1, y1, x2, y2, radius, **kwargs):
        points = [
            x1 + radius, y1,
            x2 - radius, y1,
            x2, y1,
            x2, y1 + radius,
            x2, y2 - radius,
            x2, y2,
            x2 - radius, y2,
            x1 + radius, y2,
            x1, y2,
            x1, y2 - radius,
            x1, y1 + radius,
            x1, y1,
        ]
        self.create_polygon(points, smooth=True, splinesteps=24, **kwargs)

    def draw(self):
        self.delete("all")
        self.rounded_rect(1, 1, self.width_value - 1, self.height_value - 1, 11, fill=self.current, outline="")
        self.create_text(
            self.width_value / 2,
            self.height_value / 2,
            text=self.text,
            fill=self.fg,
            font=("Segoe UI", 10, "bold"),
        )

    def _enter(self, _event):
        self.current = self.hover
        self.draw()

    def _leave(self, _event):
        self.current = self.normal
        self.draw()


def copy_image_to_clipboard(image):
    output = io.BytesIO()
    image.convert("RGB").save(output, "BMP")
    data = output.getvalue()[14:]
    output.close()

    CF_DIB = 8
    GMEM_MOVEABLE = 0x0002
    user32 = ctypes.windll.user32
    kernel32 = ctypes.windll.kernel32

    kernel32.GlobalAlloc.argtypes = [wintypes.UINT, ctypes.c_size_t]
    kernel32.GlobalAlloc.restype = wintypes.HGLOBAL
    kernel32.GlobalLock.argtypes = [wintypes.HGLOBAL]
    kernel32.GlobalLock.restype = ctypes.c_void_p
    kernel32.GlobalUnlock.argtypes = [wintypes.HGLOBAL]
    kernel32.GlobalUnlock.restype = wintypes.BOOL
    kernel32.GlobalFree.argtypes = [wintypes.HGLOBAL]
    kernel32.GlobalFree.restype = wintypes.HGLOBAL
    user32.OpenClipboard.argtypes = [wintypes.HWND]
    user32.OpenClipboard.restype = wintypes.BOOL
    user32.EmptyClipboard.restype = wintypes.BOOL
    user32.SetClipboardData.argtypes = [wintypes.UINT, wintypes.HANDLE]
    user32.SetClipboardData.restype = wintypes.HANDLE
    user32.CloseClipboard.restype = wintypes.BOOL

    global_mem = kernel32.GlobalAlloc(GMEM_MOVEABLE, len(data))
    if not global_mem:
        raise RuntimeError("No se pudo reservar memoria para el portapapeles.")

    locked_mem = kernel32.GlobalLock(global_mem)
    if not locked_mem:
        kernel32.GlobalFree(global_mem)
        raise RuntimeError("No se pudo bloquear memoria para el portapapeles.")
    ctypes.memmove(locked_mem, data, len(data))
    kernel32.GlobalUnlock(global_mem)

    if not user32.OpenClipboard(None):
        kernel32.GlobalFree(global_mem)
        raise RuntimeError("No se pudo abrir el portapapeles.")

    try:
        user32.EmptyClipboard()
        if not user32.SetClipboardData(CF_DIB, global_mem):
            kernel32.GlobalFree(global_mem)
            raise RuntimeError("No se pudo copiar la imagen al portapapeles.")
    finally:
        user32.CloseClipboard()


def _cursor_position():
    class POINT(ctypes.Structure):
        _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]

    point = POINT()
    ctypes.windll.user32.GetCursorPos(ctypes.byref(point))
    return point.x, point.y


def _virtual_screen_rect():
    user32 = ctypes.windll.user32
    left = user32.GetSystemMetrics(76)  # SM_XVIRTUALSCREEN
    top = user32.GetSystemMetrics(77)  # SM_YVIRTUALSCREEN
    width = user32.GetSystemMetrics(78)  # SM_CXVIRTUALSCREEN
    height = user32.GetSystemMetrics(79)  # SM_CYVIRTUALSCREEN
    return left, top, left + width, top + height


def _monitor_rects():
    rects = []

    class RECT(ctypes.Structure):
        _fields_ = [
            ("left", ctypes.c_long),
            ("top", ctypes.c_long),
            ("right", ctypes.c_long),
            ("bottom", ctypes.c_long),
        ]

    MONITORENUMPROC = ctypes.WINFUNCTYPE(
        ctypes.c_int,
        ctypes.c_ulong,
        ctypes.c_ulong,
        ctypes.POINTER(RECT),
        ctypes.c_double,
    )

    def callback(_monitor, _dc, rect_ptr, _data):
        rect = rect_ptr.contents
        rects.append((rect.left, rect.top, rect.right, rect.bottom))
        return 1

    ctypes.windll.user32.EnumDisplayMonitors(0, 0, MONITORENUMPROC(callback), 0)
    return rects


def _active_monitor_rect():
    x, y = _cursor_position()
    for left, top, right, bottom in _monitor_rects():
        if left <= x < right and top <= y < bottom:
            return left, top, right, bottom
    return _virtual_screen_rect()


def _set_window_rect(window, left, top, width, height):
    window.update_idletasks()
    hwnd = ctypes.windll.user32.GetParent(window.winfo_id()) or window.winfo_id()
    SWP_NOZORDER = 0x0004
    SWP_NOACTIVATE = 0x0010
    ctypes.windll.user32.SetWindowPos(hwnd, 0, left, top, width, height, SWP_NOZORDER | SWP_NOACTIVATE)


def _restore_arrow_cursor():
    try:
        user32 = ctypes.windll.user32
        user32.ReleaseCapture()
        IDC_ARROW = 32512
        arrow = user32.LoadCursorW(None, IDC_ARROW)
        user32.SetCursor(arrow)
    except Exception:
        pass


def normalize_hotkey(value):
    hotkey = " ".join((value or "").strip().lower().replace("+", " + ").split())
    replacements = {
        "suprimir": "delete",
        "supr": "delete",
        "del": "delete",
        "borrar": "delete",
        "imprimir pantalla": "print screen",
        "impr pant": "print screen",
        "impr pantalla": "print screen",
        "pantalla imprimir": "print screen",
        "control": "ctrl",
        "controle": "ctrl",
        "mayus": "shift",
        "mayuscula": "shift",
        "mayusculas": "shift",
        "escape": "esc",
        "espacio": "space",
        "barra espaciadora": "space",
    }
    if hotkey in replacements:
        return replacements[hotkey]
    parts = [part.strip() for part in hotkey.split("+")]
    normalized = [replacements.get(part, part) for part in parts if part]
    return "+".join(normalized)


class LightshotOverlay(tk.Toplevel):
    def __init__(self, master, on_close):
        super().__init__(master)
        self.on_close = on_close
        self.virtual_left, self.virtual_top, virtual_right, virtual_bottom = _active_monitor_rect()
        try:
            self.screenshot = ImageGrab.grab(
                bbox=(self.virtual_left, self.virtual_top, virtual_right, virtual_bottom),
                all_screens=True,
            ).convert("RGB")
        except Exception:
            self.virtual_left, self.virtual_top, virtual_right, virtual_bottom = _virtual_screen_rect()
            self.screenshot = ImageGrab.grab(all_screens=True).convert("RGB")
        self.screen_w = self.screenshot.width
        self.screen_h = self.screenshot.height
        self.photo = ImageTk.PhotoImage(self.screenshot)

        self.selection = None
        self.select_start = None
        self.drag_start = None
        self.current_point = None
        self.tool = "arrow"
        self.color = "#ff3b30"
        self.width = 4
        self.counter_value = 1
        self.ops = []
        self.pixel_photos = []
        self.preview_photo = None
        self.text_entry = None
        self.toolbar_window = None
        self.quick_toolbar = None
        self.quick_toolbar_window = None
        self.quick_buttons = {}
        self.status_id = None
        self.context_menu = None
        self._closing = False
        self._selection_redraw_pending = False

        self.overrideredirect(True)
        self.attributes("-topmost", True)
        self.geometry(f"{self.screen_w}x{self.screen_h}+0+0")
        self.configure(bg=BG)
        _set_window_rect(
            self,
            self.virtual_left,
            self.virtual_top,
            virtual_right - self.virtual_left,
            virtual_bottom - self.virtual_top,
        )

        self.canvas = tk.Canvas(self, width=self.screen_w, height=self.screen_h, highlightthickness=0, cursor="crosshair")
        self.canvas.pack(fill="both", expand=True)
        self.canvas.create_image(0, 0, image=self.photo, anchor="nw", tags="base")

        self._draw_selection_layer()
        self._build_hint()

        self.canvas.bind("<ButtonPress-1>", self._mouse_down)
        self.canvas.bind("<B1-Motion>", self._mouse_drag)
        self.canvas.bind("<ButtonRelease-1>", self._mouse_up)
        self.canvas.bind("<Button-3>", self._open_context_menu)
        self.bind("<Escape>", lambda _event: self.cancel())
        self.bind("<Return>", lambda _event: self.copy())
        self.bind("<Control-c>", lambda _event: self.copy())
        self.bind("<Control-s>", lambda _event: self.save())
        self.bind("<Control-z>", lambda _event: self.undo())
        self.focus_force()

    def _build_hint(self):
        self.canvas.create_rectangle(28, 28, 530, 82, fill="#071019", outline="#26485d", tags="hint")
        self.canvas.create_text(
            48,
            55,
            text="Arrastre para seleccionar. Esc cancela. Ctrl+C copia. Ctrl+S guarda.",
            fill=TEXT,
            font=("Segoe UI", 11),
            anchor="w",
            tags="hint",
        )

    def _mouse_down(self, event):
        if self.text_entry:
            return
        if not self.selection:
            self.select_start = (event.x, event.y)
            self.current_point = (event.x, event.y)
            self.canvas.delete("hint")
            self._draw_selection_layer()
            return
        if not self._inside_selection(event.x, event.y):
            self.cancel()
            return
        if self.tool == "text":
            self._start_text(event.x, event.y)
            return
        if self.tool == "number":
            x, y = self._clamp_to_selection(event.x, event.y)
            self.ops.append(DrawOp("number", [(x, y)], self.color, max(20, self.width * 7), str(self.counter_value)))
            self.counter_value += 1
            self._redraw_all()
            return
        self.drag_start = (event.x, event.y)
        self.current_point = (event.x, event.y)
        if self.tool in ("pencil", "highlight"):
            self.ops.append(DrawOp(self.tool, [(event.x, event.y)], self.color, self.width))
        self._redraw_all()

    def _mouse_drag(self, event):
        x, y = self._clamp_to_screen(event.x, event.y)
        if not self.selection and self.select_start:
            self.current_point = (x, y)
            self._schedule_selection_layer()
            return
        if not self.drag_start:
            return
        x, y = self._clamp_to_selection(x, y)
        self.current_point = (x, y)
        if self.tool in ("pencil", "highlight") and self.ops:
            self.ops[-1].points.append((x, y))
            self._redraw_all()
        else:
            self._draw_preview()

    def _mouse_up(self, event):
        x, y = self._clamp_to_screen(event.x, event.y)
        if not self.selection and self.select_start:
            x1, y1 = self.select_start
            x2, y2 = x, y
            left, top, right, bottom = min(x1, x2), min(y1, y2), max(x1, x2), max(y1, y2)
            if right - left < 12 or bottom - top < 12:
                self.cancel()
                return
            self.selection = (left, top, right, bottom)
            self.select_start = None
            self.current_point = None
            self._redraw_all()
            self._show_toolbar()
            return
        if not self.drag_start:
            return
        end = self._clamp_to_selection(x, y)
        if self.tool in ("arrow", "line", "rect", "pixelate"):
            self.ops.append(DrawOp(self.tool, [self.drag_start, end], self.color, self.width))
        self.drag_start = None
        self.current_point = None
        self.canvas.delete("preview")
        self._redraw_all()

    def _show_toolbar(self):
        if not self.selection:
            return
        x1, y1, x2, y2 = self.selection
        self.toolbar = tk.Frame(
            self.canvas,
            bg="#0a1017",
            padx=8,
            pady=7,
            highlightthickness=1,
            highlightbackground="#263848",
        )

        brand = tk.Label(
            self.toolbar,
            text="Z",
            bg="#0a1017",
            fg=ACCENT_2,
            font=("Segoe UI", 11, "bold"),
            cursor="hand2",
        )
        brand.pack(side="left", padx=(2, 8))
        brand.bind("<Button-1>", lambda _event: self._show_about())

        self.tool_label = tk.Label(
            self.toolbar,
            text=self._tool_name(self.tool),
            bg="#121d29",
            fg=TEXT,
            font=("Segoe UI", 8, "bold"),
            padx=10,
            pady=6,
        )
        self.tool_label.pack(side="left", padx=(0, 4))

        self.color_button = tk.Canvas(
            self.toolbar,
            width=42,
            height=30,
            bg="#0a1017",
            highlightthickness=0,
            cursor="hand2",
        )
        self.color_button.create_oval(5, 5, 37, 25, fill="#121d29", outline="#2d4557", width=1)
        self.color_button.create_oval(14, 8, 28, 22, fill=self.color, outline="#ffffff", width=1, tags="dot")
        self.color_button.bind("<Button-1>", self._open_color_menu)
        self.color_button.pack(side="left", padx=2)

        for label, command, tip in (
            ("-", self.thinner, "Menos grosor"),
            ("+", self.thicker, "Mas grosor"),
            ("↩", self.undo, "Deshacer"),
            ("...", self._open_tools_menu, "Mas herramientas"),
        ):
            button = self._toolbar_button(label, command, width=38)
            button.pack(side="left", padx=2)
            button.bind("<Enter>", lambda _event, name=tip: self._show_status(name))

        self._toolbar_button("Copiar", self.copy, width=76, bg=ACCENT, hover=ACCENT_2).pack(side="left", padx=(8, 2))
        self._toolbar_button("Guardar", self.save, width=78).pack(side="left", padx=2)
        self._toolbar_button("X", self.cancel, width=38, bg="#301820", hover="#4a2232", fg="#ffd2d2").pack(side="left", padx=(6, 0))

        toolbar_width = 474
        toolbar_x = min(max(x2 - toolbar_width, 18), max(18, self.screen_w - toolbar_width - 18))
        toolbar_y = y2 + 12 if y2 + 58 < self.screen_h else max(18, y1 - 58)
        self.toolbar_window = self.canvas.create_window(toolbar_x, toolbar_y, window=self.toolbar, anchor="nw", tags="toolbar")
        self._show_quick_toolbar()

    def _show_about(self):
        messagebox.showinfo(
            "Acerca de Zaetta",
            "Zaetta Capture\n\n"
            "Desarrollado por:\n"
            "VICTOR ALEXIS ALZATE CORTES\n\n"
            f"Version: {APP_VERSION}\n\n"
            "Capturador local para evidencia interna.",
            parent=self,
        )

    def _show_quick_toolbar(self):
        if not self.selection:
            return
        x1, y1, x2, y2 = self.selection
        self.quick_toolbar = tk.Frame(
            self.canvas,
            bg="#0a1017",
            padx=6,
            pady=6,
            highlightthickness=1,
            highlightbackground="#263848",
        )
        self.quick_buttons = {}
        for label, tool, tip in (
            ("->", "arrow", "Flecha"),
            ("[]", "rect", "Marco"),
            ("T", "text", "Texto"),
            ("Px", "pixelate", "Pixelar"),
        ):
            button = self._quick_button(label, lambda t=tool: self.set_tool(t))
            button.pack(pady=3)
            button.bind("<Enter>", lambda _event, name=tip: self._show_status(name))
            self.quick_buttons[tool] = button
        more = self._quick_button("...", self._open_tools_menu)
        more.pack(pady=(7, 3))
        more.bind("<Enter>", lambda _event: self._show_status("Todas las herramientas"))
        self._sync_quick_buttons()

        quick_x = x2 + 10
        if quick_x + 48 > self.screen_w:
            quick_x = max(12, x1 - 48)
        quick_y = max(12, min(y1, self.screen_h - 150))
        self.quick_toolbar_window = self.canvas.create_window(
            quick_x,
            quick_y,
            window=self.quick_toolbar,
            anchor="nw",
            tags="toolbar",
        )

    def _quick_button(self, text, command):
        return tk.Button(
            self.quick_toolbar,
            text=text,
            width=5,
            bg="#121d29",
            fg=TEXT,
            activebackground=ACCENT,
            activeforeground="white",
            relief="flat",
            bd=0,
            padx=3,
            pady=7,
            cursor="hand2",
            font=("Segoe UI", 9, "bold"),
            command=command,
        )

    def _sync_quick_buttons(self):
        for tool, button in getattr(self, "quick_buttons", {}).items():
            if tool == self.tool:
                button.configure(bg=ACCENT, activebackground=ACCENT_2, fg="white")
            else:
                button.configure(bg="#121d29", activebackground="#1b4258", fg=TEXT)

    def _toolbar_button(self, text, command, *, width=96, bg="#13283a", hover="#1b4258", fg=TEXT):
        button = tk.Button(
            self.toolbar,
            text=text,
            width=max(4, int(width / 10)),
            bg=bg,
            fg=fg,
            activebackground=hover,
            activeforeground="white",
            relief="flat",
            bd=0,
            padx=7,
            pady=6,
            cursor="hand2",
            font=("Segoe UI", 8, "bold"),
            command=command,
        )
        return button

    def _tool_name(self, tool):
        names = {
            "arrow": "Flecha",
            "rect": "Marco",
            "line": "Linea",
            "pencil": "Lapiz",
            "highlight": "Resaltar",
            "pixelate": "Pixelar",
            "number": "Numero",
            "text": "Texto",
        }
        return names.get(tool, tool)

    def _open_tools_menu(self):
        menu = tk.Menu(self, tearoff=False, bg="#0f1f2d", fg=TEXT, activebackground=ACCENT, activeforeground="white")
        tools = [
            ("Flecha", "arrow"),
            ("Marco", "rect"),
            ("Linea", "line"),
            ("Lapiz", "pencil"),
            ("Resaltar", "highlight"),
            ("Pixelar", "pixelate"),
            ("Numero", "number"),
            ("Texto", "text"),
        ]
        for label, tool in tools:
            prefix = "✓ " if tool == self.tool else "  "
            menu.add_command(label=f"{prefix}{label}", command=lambda t=tool: self.set_tool(t))
        menu.tk_popup(self.toolbar.winfo_rootx() + 132, self.toolbar.winfo_rooty() + 38)

    def _open_color_menu(self, event=None):
        menu = tk.Menu(self, tearoff=False, bg="#0f1f2d", fg=TEXT, activebackground=ACCENT, activeforeground="white")
        colors = [
            ("Rojo", "#ff3b30"),
            ("Amarillo", "#ffcc00"),
            ("Verde", "#34c759"),
            ("Azul", "#00a3e0"),
            ("Blanco", "#ffffff"),
            ("Negro", "#111111"),
        ]
        for label, color in colors:
            prefix = "✓ " if color.lower() == self.color.lower() else "  "
            menu.add_command(label=f"{prefix}{label}", command=lambda c=color: self.set_color(c))
        root_x = self.color_button.winfo_rootx() if hasattr(self, "color_button") else self.toolbar.winfo_rootx()
        root_y = self.color_button.winfo_rooty() + 38 if hasattr(self, "color_button") else self.toolbar.winfo_rooty() + 38
        menu.tk_popup(root_x, root_y)

    def set_tool(self, tool):
        self.tool = tool
        self.canvas.configure(cursor="xterm" if tool == "text" else "crosshair")
        if hasattr(self, "tool_label"):
            self.tool_label.configure(text=self._tool_name(tool))
        self._sync_quick_buttons()
        self._show_status(f"Herramienta: {self._tool_name(tool)}")

    def set_color(self, color):
        self.color = color
        if hasattr(self, "color_button"):
            self.color_button.delete("dot")
            self.color_button.create_oval(10, 8, 26, 24, fill=self.color, outline="#ffffff", width=1, tags="dot")
        self._show_status("Color seleccionado")

    def thinner(self):
        self.width = max(1, self.width - 1)
        self._show_status(f"Grosor {self.width}")

    def thicker(self):
        self.width = min(20, self.width + 1)
        self._show_status(f"Grosor {self.width}")

    def undo(self):
        if self.ops:
            self.ops.pop()
            self._redraw_all()
            self._show_status("Ultimo cambio revertido")

    def copy(self):
        if self._closing or not self.selection:
            return
        self._closing = True
        try:
            final_image = self._with_evidence_footer(self._render_crop())
            copy_image_to_clipboard(final_image)
            self._save_history(final_image, "copiada")
            self._close_clean()
        except Exception as exc:
            self._closing = False
            messagebox.showerror(APP_NAME, str(exc))

    def _open_context_menu(self, event):
        if not self.selection or not self._inside_selection(event.x, event.y):
            return
        if self.context_menu:
            self.context_menu.destroy()
        self.context_menu = tk.Menu(self, tearoff=False, bg=PANEL, fg=TEXT, activebackground=ACCENT, activeforeground="white")
        self.context_menu.add_command(label="Copiar", command=self.copy)
        self.context_menu.add_command(label="Guardar como PNG", command=self.save)
        self.context_menu.add_separator()
        self.context_menu.add_command(label="Deshacer", command=self.undo)
        self.context_menu.add_command(label="Cancelar captura", command=self.cancel)
        self.context_menu.tk_popup(event.x_root, event.y_root)

    def save(self):
        if self._closing or not self.selection:
            return
        DEFAULT_DIR.mkdir(parents=True, exist_ok=True)
        default_name = f"zaetta_capture_{datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}.png"
        path = filedialog.asksaveasfilename(
            title="Guardar captura",
            initialdir=str(DEFAULT_DIR),
            initialfile=default_name,
            defaultextension=".png",
            filetypes=[("PNG", "*.png")],
        )
        if not path:
            return
        final_image = self._with_evidence_footer(self._render_crop())
        final_image.save(path)
        self._save_history(final_image, "guardada")
        self._show_status(f"Guardado: {path}")

    def _with_evidence_footer(self, image):
        enabled = bool(getattr(self.master, "config_data", {}).get("evidence_mode", False))
        if not enabled:
            return image
        footer_h = 38
        result = Image.new("RGB", (image.width, image.height + footer_h), "#f5f9fc")
        result.paste(image, (0, 0))
        draw = ImageDraw.Draw(result)
        draw.rectangle((0, image.height, image.width, image.height + footer_h), fill="#071019")
        draw.rectangle((0, image.height, image.width, image.height + 3), fill=ACCENT)
        detail = (
            f"Zaetta Capture | {datetime.now().strftime('%Y-%m-%d %H:%M:%S')} | "
            f"{getpass.getuser()} | {socket.gethostname()}"
        )
        draw.text((14, image.height + 11), detail, fill="#f6fbff", font=self._font(13))
        return result

    def _save_history(self, image, action):
        try:
            HISTORY_DIR.mkdir(parents=True, exist_ok=True)
            path = HISTORY_DIR / f"zaetta_{action}_{datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}.png"
            image.save(path)
            latest = DEFAULT_DIR / "ultima_captura.png"
            image.save(latest)
        except Exception:
            pass

    def cancel(self):
        if self._closing:
            return
        self._closing = True
        self._close_clean()

    def _close_clean(self):
        _restore_arrow_cursor()
        try:
            self.canvas.configure(cursor="arrow")
            self.configure(cursor="arrow")
            self.grab_release()
            self.update_idletasks()
        except Exception:
            pass
        self.destroy()
        self.on_close()

    def _start_text(self, x, y):
        x, y = self._clamp_to_selection(x, y)
        self.text_entry = tk.Entry(self.canvas, bg="#ffffff", fg="#000000", relief="solid", font=("Segoe UI", 12))
        self.text_entry.insert(0, "")
        window_id = self.canvas.create_window(x, y, window=self.text_entry, anchor="nw", tags="text_entry")
        self.text_entry.focus_set()

        def commit(_event=None):
            value = self.text_entry.get().strip()
            self.canvas.delete(window_id)
            self.text_entry.destroy()
            self.text_entry = None
            if value:
                self.ops.append(DrawOp("text", [(x, y)], self.color, max(12, self.width * 4), value))
                self._redraw_all()

        self.text_entry.bind("<Return>", commit)
        self.text_entry.bind("<Escape>", lambda _e: self._cancel_text(window_id))

    def _cancel_text(self, window_id):
        self.canvas.delete(window_id)
        if self.text_entry:
            self.text_entry.destroy()
            self.text_entry = None

    def _redraw_all(self):
        self.canvas.delete("drawop")
        self.canvas.delete("shade")
        self.canvas.delete("selection")
        self.canvas.delete("size_label")
        self._draw_ops()
        self._draw_selection_layer()

    def _draw_selection_layer(self):
        self.canvas.delete("shade")
        self.canvas.delete("selection")
        self.canvas.delete("size_label")
        box = self.selection
        if not box and self.select_start and self.current_point:
            x1, y1 = self.select_start
            x2, y2 = self.current_point
            box = (min(x1, x2), min(y1, y2), max(x1, x2), max(y1, y2))

        if not box:
            self.canvas.create_rectangle(0, 0, self.screen_w, self.screen_h, fill="#000000", stipple="gray50", outline="", tags="shade")
            return

        x1, y1, x2, y2 = box
        self.canvas.create_rectangle(0, 0, self.screen_w, y1, fill="#000000", stipple="gray50", outline="", tags="shade")
        self.canvas.create_rectangle(0, y2, self.screen_w, self.screen_h, fill="#000000", stipple="gray50", outline="", tags="shade")
        self.canvas.create_rectangle(0, y1, x1, y2, fill="#000000", stipple="gray50", outline="", tags="shade")
        self.canvas.create_rectangle(x2, y1, self.screen_w, y2, fill="#000000", stipple="gray50", outline="", tags="shade")
        self.canvas.create_rectangle(x1, y1, x2, y2, outline=ACCENT, width=2, tags="selection")
        for hx, hy in ((x1, y1), (x2, y1), (x1, y2), (x2, y2)):
            self.canvas.create_rectangle(hx - 4, hy - 4, hx + 4, hy + 4, fill=ACCENT, outline="", tags="selection")
        self.canvas.create_rectangle(x1, max(0, y1 - 27), x1 + 122, max(0, y1 - 5), fill=BG, outline=ACCENT, tags="size_label")
        self.canvas.create_text(
            x1 + 10,
            max(10, y1 - 16),
            text=f"{x2 - x1} x {y2 - y1}",
            fill=TEXT,
            font=("Segoe UI", 9, "bold"),
            anchor="w",
            tags="size_label",
        )

    def _draw_ops(self):
        self.pixel_photos = []
        for op in self.ops:
            if op.kind == "rect" and len(op.points) >= 2:
                self.canvas.create_rectangle(*op.points[0], *op.points[1], outline=op.color, width=op.width, tags="drawop")
            elif op.kind == "line" and len(op.points) >= 2:
                self.canvas.create_line(*op.points[0], *op.points[1], fill=op.color, width=op.width, tags="drawop")
            elif op.kind == "arrow" and len(op.points) >= 2:
                self.canvas.create_line(*op.points[0], *op.points[1], fill=op.color, width=op.width, arrow="last", arrowshape=(18, 22, 7), tags="drawop")
            elif op.kind in ("pencil", "highlight") and len(op.points) >= 2:
                fill = "#fff34d" if op.kind == "highlight" else op.color
                width = max(op.width * 4, 10) if op.kind == "highlight" else op.width
                self.canvas.create_line(*self._flatten(op.points), fill=fill, width=width, smooth=True, capstyle="round", tags="drawop")
            elif op.kind == "pixelate" and len(op.points) >= 2:
                photo, left, top = self._pixelated_photo_from_screen(op.points[0], op.points[1])
                if photo:
                    self.pixel_photos.append(photo)
                    self.canvas.create_image(left, top, image=photo, anchor="nw", tags="drawop")
            elif op.kind == "number" and op.points:
                x, y = op.points[0]
                radius = max(13, op.width // 2)
                self.canvas.create_oval(x - radius, y - radius, x + radius, y + radius, fill=op.color, outline="white", width=2, tags="drawop")
                self.canvas.create_text(x, y, text=op.text, fill="white", font=("Segoe UI", max(12, radius), "bold"), tags="drawop")
            elif op.kind == "text" and op.points:
                self.canvas.create_text(*op.points[0], text=op.text, fill=op.color, font=("Segoe UI", max(12, op.width), "bold"), anchor="nw", tags="drawop")

    def _draw_preview(self):
        self.canvas.delete("preview")
        if not self.drag_start or not self.current_point:
            return
        x1, y1 = self.drag_start
        x2, y2 = self.current_point
        if self.tool == "rect":
            self.canvas.create_rectangle(x1, y1, x2, y2, outline=self.color, width=self.width, dash=(6, 4), tags="preview")
        elif self.tool == "pixelate":
            photo, left, top = self._pixelated_photo_from_screen((x1, y1), (x2, y2))
            if photo:
                self.preview_photo = photo
                self.canvas.create_image(left, top, image=photo, anchor="nw", tags="preview")
                self.canvas.create_rectangle(left, top, left + photo.width(), top + photo.height(), outline=ACCENT, width=1, dash=(5, 4), tags="preview")
        elif self.tool == "line":
            self.canvas.create_line(x1, y1, x2, y2, fill=self.color, width=self.width, dash=(6, 4), tags="preview")
        elif self.tool == "arrow":
            self.canvas.create_line(x1, y1, x2, y2, fill=self.color, width=self.width, arrow="last", dash=(6, 4), tags="preview")

    def _render_crop(self):
        if not self.selection:
            raise RuntimeError("No hay seleccion activa.")
        x1, y1, x2, y2 = self._ordered_box(*self.selection)
        crop = self.screenshot.crop((x1, y1, x2, y2)).convert("RGBA")
        draw = ImageDraw.Draw(crop, "RGBA")
        for op in self.ops:
            pts = [(px - x1, py - y1) for px, py in op.points]
            if op.kind == "rect" and len(pts) >= 2:
                draw.rectangle(self._ordered_box(*pts[0], *pts[1]), outline=op.color, width=op.width)
            elif op.kind == "line" and len(pts) >= 2:
                draw.line([pts[0], pts[1]], fill=op.color, width=op.width)
            elif op.kind == "arrow" and len(pts) >= 2:
                self._draw_pil_arrow(draw, pts[0], pts[1], op.color, op.width)
            elif op.kind == "pencil" and len(pts) >= 2:
                draw.line(pts, fill=op.color, width=op.width, joint="curve")
            elif op.kind == "highlight" and len(pts) >= 2:
                draw.line(pts, fill=(255, 243, 77, 120), width=max(op.width * 4, 10), joint="curve")
            elif op.kind == "pixelate" and len(pts) >= 2:
                self._apply_pixelate(crop, self._ordered_box(*pts[0], *pts[1]))
            elif op.kind == "number" and pts:
                x, y = pts[0]
                radius = max(13, op.width // 2)
                draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=op.color, outline="white", width=2)
                font = self._font(max(12, radius))
                try:
                    bbox = draw.textbbox((0, 0), op.text, font=font)
                    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
                except Exception:
                    tw, th = 10, 10
                draw.text((x - tw / 2, y - th / 2 - 1), op.text, fill="white", font=font)
            elif op.kind == "text" and pts:
                font = self._font(max(12, op.width))
                draw.text(pts[0], op.text, fill=op.color, font=font)
        return crop.convert("RGB")

    def _apply_pixelate(self, image, box):
        left, top, right, bottom = box
        left = max(0, min(image.width, left))
        right = max(0, min(image.width, right))
        top = max(0, min(image.height, top))
        bottom = max(0, min(image.height, bottom))
        if right - left < 2 or bottom - top < 2:
            return
        region = image.crop((left, top, right, bottom))
        small_w = max(1, region.width // 28)
        small_h = max(1, region.height // 28)
        mosaic = region.resize((small_w, small_h), Image.Resampling.BILINEAR)
        mosaic = mosaic.resize((right - left, bottom - top), Image.Resampling.NEAREST).convert("RGBA")

        veil = Image.new("RGBA", mosaic.size, (18, 32, 45, 95))
        mosaic = Image.alpha_composite(mosaic, veil)

        draw = ImageDraw.Draw(mosaic, "RGBA")
        step = max(10, min(mosaic.width, mosaic.height) // 4)
        for x in range(0, mosaic.width, step):
            draw.line((x, 0, x, mosaic.height), fill=(255, 255, 255, 24), width=1)
        for y in range(0, mosaic.height, step):
            draw.line((0, y, mosaic.width, y), fill=(255, 255, 255, 24), width=1)

        image.paste(mosaic, (left, top), mosaic)

    def _pixelated_photo_from_screen(self, start, end):
        left, top, right, bottom = self._ordered_box(*start, *end)
        left = max(0, min(self.screen_w, left))
        right = max(0, min(self.screen_w, right))
        top = max(0, min(self.screen_h, top))
        bottom = max(0, min(self.screen_h, bottom))
        if right - left < 2 or bottom - top < 2:
            return None, left, top
        region = self.screenshot.crop((left, top, right, bottom)).convert("RGBA")
        self._apply_pixelate(region, (0, 0, region.width, region.height))
        return ImageTk.PhotoImage(region), left, top

    def _draw_pil_arrow(self, draw, start, end, color, width):
        draw.line([start, end], fill=color, width=width)
        x1, y1 = start
        x2, y2 = end
        angle = __import__("math").atan2(y2 - y1, x2 - x1)
        length = max(12, width * 5)
        left = (
            x2 - length * __import__("math").cos(angle - 0.55),
            y2 - length * __import__("math").sin(angle - 0.55),
        )
        right = (
            x2 - length * __import__("math").cos(angle + 0.55),
            y2 - length * __import__("math").sin(angle + 0.55),
        )
        draw.polygon([end, left, right], fill=color)

    def _font(self, size):
        for name in ("arial.ttf", "segoeui.ttf"):
            try:
                return ImageFont.truetype(name, size)
            except Exception:
                pass
        return ImageFont.load_default()

    def _show_status(self, message):
        if self.status_id:
            self.canvas.delete("status")
        x = 24
        y = self.screen_h - 52
        self.canvas.create_rectangle(x, y, x + 360, y + 34, fill=BG, outline=ACCENT, tags="status")
        self.canvas.create_text(x + 14, y + 17, text=message, fill=TEXT, font=("Segoe UI", 10, "bold"), anchor="w", tags="status")
        self.after(1700, lambda: self.canvas.delete("status"))

    def _inside_selection(self, x, y):
        if not self.selection:
            return False
        x1, y1, x2, y2 = self.selection
        return x1 <= x <= x2 and y1 <= y <= y2

    def _clamp_to_screen(self, x, y):
        return max(0, min(self.screen_w, x)), max(0, min(self.screen_h, y))

    def _clamp_to_selection(self, x, y):
        x1, y1, x2, y2 = self.selection
        return max(x1, min(x2, x)), max(y1, min(y2, y))

    @staticmethod
    def _flatten(points):
        values = []
        for x, y in points:
            values.extend([x, y])
        return values

    @staticmethod
    def _ordered_box(x1, y1, x2, y2):
        left, right = sorted((int(x1), int(x2)))
        top, bottom = sorted((int(y1), int(y2)))
        if right <= left:
            right = left + 1
        if bottom <= top:
            bottom = top + 1
        return left, top, right, bottom


class ImageEditorWindow(tk.Toplevel):
    def __init__(self, master, image, on_close):
        super().__init__(master)
        self.on_close = on_close
        self.base_image = image.convert("RGB")
        self.ops = []
        self.tool = "select"
        self.color = "#ff3b30"
        self.width = 4
        self.drag_start = None
        self.current_point = None
        self.photo = None
        self.text_entry = None

        self.title(f"{APP_NAME} Image editor")
        self.geometry(self._initial_geometry())
        self.minsize(840, 560)
        self.configure(bg="#f3f5f8")
        self.protocol("WM_DELETE_WINDOW", self.close)

        self._build_menu()
        self._build_ui()
        self._redraw()

        self.bind("<Control-c>", lambda _event: self.copy())
        self.bind("<Control-s>", lambda _event: self.save())
        self.bind("<Control-z>", lambda _event: self.undo())
        self.bind("<Escape>", lambda _event: self.close())
        self.focus_force()

    def _schedule_selection_layer(self):
        if self._selection_redraw_pending:
            return
        self._selection_redraw_pending = True

        def redraw():
            if self._closing:
                return
            self._selection_redraw_pending = False
            self._draw_selection_layer()

        self.after(8, redraw)

    def _initial_geometry(self):
        width = min(max(self.base_image.width + 110, 980), self.winfo_screenwidth() - 80)
        height = min(max(self.base_image.height + 115, 640), self.winfo_screenheight() - 80)
        return f"{width}x{height}+35+35"

    def _build_menu(self):
        menu = tk.Menu(self)
        file_menu = tk.Menu(menu, tearoff=False)
        file_menu.add_command(label="Save as PNG", command=self.save)
        file_menu.add_command(label="Copy", command=self.copy)
        file_menu.add_separator()
        file_menu.add_command(label="Close", command=self.close)
        edit_menu = tk.Menu(menu, tearoff=False)
        edit_menu.add_command(label="Undo", command=self.undo)
        edit_menu.add_command(label="Clear annotations", command=self.clear)
        object_menu = tk.Menu(menu, tearoff=False)
        object_menu.add_command(label="Copy image", command=self.copy)
        zoom_menu = tk.Menu(menu, tearoff=False)
        zoom_menu.add_command(label="100%", command=lambda: None)
        help_menu = tk.Menu(menu, tearoff=False)
        help_menu.add_command(label=f"{APP_NAME} {APP_VERSION}", command=lambda: None)
        menu.add_cascade(label="File", menu=file_menu)
        menu.add_cascade(label="Edit", menu=edit_menu)
        menu.add_cascade(label="Object", menu=object_menu)
        menu.add_cascade(label="Zoom", menu=zoom_menu)
        menu.add_cascade(label="Help", menu=help_menu)
        self.config(menu=menu)

    def _build_ui(self):
        top = tk.Frame(self, bg="#071019", height=52)
        top.pack(fill="x")
        tk.Label(
            top,
            text="Zaetta Editor",
            bg="#071019",
            fg="#f6fbff",
            font=("Segoe UI", 11, "bold"),
        ).pack(side="left", padx=(14, 10), pady=10)
        self.editor_tool_label = tk.Label(
            top,
            text=self._tool_name(self.tool),
            bg="#102436",
            fg="#d9f7ff",
            font=("Segoe UI", 9, "bold"),
            padx=12,
            pady=7,
        )
        self.editor_tool_label.pack(side="left", padx=(0, 6), pady=9)
        self._top_button(top, "Herramientas", self._open_editor_tools_menu, width=14).pack(side="left", padx=3, pady=9)
        self._top_button(top, "Color", self._open_editor_color_menu, width=8).pack(side="left", padx=3, pady=9)
        self._top_button(top, "-", self.thinner, width=4).pack(side="left", padx=(10, 2), pady=9)
        self._top_button(top, "+", self.thicker, width=4).pack(side="left", padx=2, pady=9)
        self._top_button(top, "Undo", self.undo, width=7).pack(side="left", padx=3, pady=9)
        self._top_button(top, "Limpiar", self.clear, width=8).pack(side="left", padx=3, pady=9)
        self._top_button(top, "Guardar", self.save, width=9).pack(side="right", padx=(3, 14), pady=9)
        self._top_button(top, "Copiar", self.copy, width=9, accent=True).pack(side="right", padx=3, pady=9)
        tk.Label(
            top,
            text="Ctrl+C copia | Ctrl+S guarda",
            bg="#071019",
            fg="#9eb3c6",
            font=("Segoe UI", 9),
        ).pack(side="right", padx=10)

        body = tk.Frame(self, bg="#101d29")
        body.pack(fill="both", expand=True)
        left = tk.Frame(body, bg="#0d1824", width=42)
        left.pack(side="left", fill="y")
        for label, tool, tip in (
            ("Sel", "select", "Seleccion"),
            ("Box", "rect", "Rectangulo"),
            ("Cir", "ellipse", "Circulo"),
            ("Lin", "line", "Linea"),
            ("Arr", "arrow", "Flecha"),
            ("Pen", "pencil", "Lapiz"),
            ("H", "highlight", "Resaltador"),
            ("T", "text", "Texto"),
        ):
            btn = tk.Button(
                left,
                text=label,
                width=4,
                bg="#13283a",
                fg="#d9f7ff",
                activebackground="#1b4258",
                activeforeground="white",
                relief="flat",
                bd=0,
                cursor="hand2",
                command=lambda t=tool: self.set_tool(t),
            )
            btn.pack(padx=5, pady=4)
            btn.bind("<Enter>", lambda _e, name=tip: self.status.configure(text=name))

        canvas_wrap = tk.Frame(body, bg="#d8dde3")
        canvas_wrap.pack(side="left", fill="both", expand=True)
        self.canvas = tk.Canvas(canvas_wrap, bg="#ffffff", highlightthickness=1, highlightbackground="#8b98a6")
        self.hbar = tk.Scrollbar(canvas_wrap, orient="horizontal", command=self.canvas.xview)
        self.vbar = tk.Scrollbar(canvas_wrap, orient="vertical", command=self.canvas.yview)
        self.canvas.configure(xscrollcommand=self.hbar.set, yscrollcommand=self.vbar.set)
        self.canvas.grid(row=0, column=0, sticky="nsew", padx=4, pady=4)
        self.vbar.grid(row=0, column=1, sticky="ns")
        self.hbar.grid(row=1, column=0, sticky="ew")
        canvas_wrap.rowconfigure(0, weight=1)
        canvas_wrap.columnconfigure(0, weight=1)
        self.canvas.bind("<ButtonPress-1>", self._press)
        self.canvas.bind("<B1-Motion>", self._drag)
        self.canvas.bind("<ButtonRelease-1>", self._release)
        self.canvas.bind("<Button-3>", self._context)

        self.status = tk.Label(self, bg="#071019", fg="#d9f7ff", anchor="w", font=("Segoe UI", 9))
        self.status.pack(fill="x")
        self.status.configure(text=f"{self.base_image.width}x{self.base_image.height} - listo para editar")

    def _top_button(self, parent, text, command, *, width=None, accent=False):
        return tk.Button(
            parent,
            text=text,
            width=width if width is not None else (4 if len(text) <= 2 else 10),
            bg=ACCENT if accent else "#13283a",
            fg="#ffffff" if accent else "#d9f7ff",
            activebackground=ACCENT_2 if accent else "#1b4258",
            activeforeground="white",
            relief="flat",
            bd=0,
            padx=4,
            pady=5,
            cursor="hand2",
            command=command,
        )

    def _open_editor_tools_menu(self):
        menu = tk.Menu(self, tearoff=False, bg="#0f1f2d", fg=TEXT, activebackground=ACCENT, activeforeground="white")
        for label, tool in (
            ("Seleccion", "select"),
            ("Rectangulo", "rect"),
            ("Circulo", "ellipse"),
            ("Linea", "line"),
            ("Flecha", "arrow"),
            ("Lapiz", "pencil"),
            ("Resaltar", "highlight"),
            ("Texto", "text"),
        ):
            prefix = "✓ " if tool == self.tool else "  "
            menu.add_command(label=f"{prefix}{label}", command=lambda t=tool: self.set_tool(t))
        menu.tk_popup(self.winfo_rootx() + 170, self.winfo_rooty() + 48)

    def _open_editor_color_menu(self):
        menu = tk.Menu(self, tearoff=False, bg="#0f1f2d", fg=TEXT, activebackground=ACCENT, activeforeground="white")
        for label, color in (
            ("Rojo", "#ff3b30"),
            ("Amarillo", "#ffcc00"),
            ("Verde", "#34c759"),
            ("Azul", "#00a3e0"),
            ("Morado", "#7b61ff"),
            ("Negro", "#111111"),
        ):
            prefix = "✓ " if color.lower() == self.color.lower() else "  "
            menu.add_command(label=f"{prefix}{label}", command=lambda c=color: self.set_color(c))
        menu.tk_popup(self.winfo_rootx() + 292, self.winfo_rooty() + 48)

    def _tool_name(self, tool):
        names = {
            "select": "Seleccion",
            "rect": "Rectangulo",
            "ellipse": "Circulo",
            "line": "Linea",
            "arrow": "Flecha",
            "pencil": "Lapiz",
            "highlight": "Resaltar",
            "text": "Texto",
        }
        return names.get(tool, tool)

    def set_tool(self, tool):
        self.tool = tool
        self.canvas.configure(cursor="xterm" if tool == "text" else "crosshair")
        if hasattr(self, "editor_tool_label"):
            self.editor_tool_label.configure(text=self._tool_name(tool))
        self.status.configure(text=f"Herramienta: {self._tool_name(tool)}")

    def set_color(self, color):
        self.color = color
        self.status.configure(text="Color seleccionado")

    def thinner(self):
        self.width = max(1, self.width - 1)
        self.status.configure(text=f"Grosor: {self.width}")

    def thicker(self):
        self.width = min(24, self.width + 1)
        self.status.configure(text=f"Grosor: {self.width}")

    def _press(self, event):
        x = int(self.canvas.canvasx(event.x))
        y = int(self.canvas.canvasy(event.y))
        if self.tool == "text":
            self._start_text(x, y)
            return
        self.drag_start = (x, y)
        self.current_point = (x, y)
        if self.tool in ("pencil", "highlight"):
            self.ops.append(DrawOp(self.tool, [(x, y)], self.color, self.width))

    def _drag(self, event):
        if not self.drag_start:
            return
        x = int(self.canvas.canvasx(event.x))
        y = int(self.canvas.canvasy(event.y))
        self.current_point = (x, y)
        if self.tool in ("pencil", "highlight") and self.ops:
            self.ops[-1].points.append((x, y))
            self._redraw()
        else:
            self._preview()

    def _release(self, event):
        if not self.drag_start:
            return
        x = int(self.canvas.canvasx(event.x))
        y = int(self.canvas.canvasy(event.y))
        if self.tool in ("rect", "ellipse", "line", "arrow"):
            self.ops.append(DrawOp(self.tool, [self.drag_start, (x, y)], self.color, self.width))
        self.drag_start = None
        self.current_point = None
        self.canvas.delete("preview")
        self._redraw()

    def _preview(self):
        self.canvas.delete("preview")
        if not self.drag_start or not self.current_point:
            return
        x1, y1 = self.drag_start
        x2, y2 = self.current_point
        if self.tool == "rect":
            self.canvas.create_rectangle(x1, y1, x2, y2, outline=self.color, width=self.width, dash=(5, 4), tags="preview")
        elif self.tool == "ellipse":
            self.canvas.create_oval(x1, y1, x2, y2, outline=self.color, width=self.width, dash=(5, 4), tags="preview")
        elif self.tool == "line":
            self.canvas.create_line(x1, y1, x2, y2, fill=self.color, width=self.width, dash=(5, 4), tags="preview")
        elif self.tool == "arrow":
            self.canvas.create_line(x1, y1, x2, y2, fill=self.color, width=self.width, arrow="last", dash=(5, 4), tags="preview")

    def _redraw(self):
        self.rendered = self._render_image()
        self.photo = ImageTk.PhotoImage(self.rendered)
        self.canvas.delete("all")
        self.canvas.create_image(0, 0, image=self.photo, anchor="nw")
        self.canvas.configure(scrollregion=(0, 0, self.rendered.width, self.rendered.height))

    def _render_image(self):
        image = self.base_image.convert("RGBA")
        draw = ImageDraw.Draw(image, "RGBA")
        for op in self.ops:
            if op.kind == "rect" and len(op.points) >= 2:
                draw.rectangle([op.points[0], op.points[1]], outline=op.color, width=op.width)
            elif op.kind == "ellipse" and len(op.points) >= 2:
                draw.ellipse([op.points[0], op.points[1]], outline=op.color, width=op.width)
            elif op.kind == "line" and len(op.points) >= 2:
                draw.line([op.points[0], op.points[1]], fill=op.color, width=op.width)
            elif op.kind == "arrow" and len(op.points) >= 2:
                self._draw_arrow(draw, op.points[0], op.points[1], op.color, op.width)
            elif op.kind == "pencil" and len(op.points) >= 2:
                draw.line(op.points, fill=op.color, width=op.width, joint="curve")
            elif op.kind == "highlight" and len(op.points) >= 2:
                draw.line(op.points, fill=(255, 243, 77, 120), width=max(op.width * 4, 10), joint="curve")
            elif op.kind == "text" and op.points:
                draw.text(op.points[0], op.text, fill=op.color, font=self._font(max(13, op.width * 4)))
        return image.convert("RGB")

    def _draw_arrow(self, draw, start, end, color, width):
        import math

        draw.line([start, end], fill=color, width=width)
        x1, y1 = start
        x2, y2 = end
        angle = math.atan2(y2 - y1, x2 - x1)
        length = max(14, width * 5)
        left = (x2 - length * math.cos(angle - 0.55), y2 - length * math.sin(angle - 0.55))
        right = (x2 - length * math.cos(angle + 0.55), y2 - length * math.sin(angle + 0.55))
        draw.polygon([end, left, right], fill=color)

    def _font(self, size):
        for name in ("arial.ttf", "segoeui.ttf"):
            try:
                return ImageFont.truetype(name, size)
            except Exception:
                pass
        return ImageFont.load_default()

    def _start_text(self, x, y):
        if self.text_entry:
            return
        self.text_entry = tk.Entry(self.canvas, bg="#ffffff", fg="#000000", relief="solid", font=("Segoe UI", 12))
        window_id = self.canvas.create_window(x, y, window=self.text_entry, anchor="nw")
        self.text_entry.focus_set()

        def commit(_event=None):
            value = self.text_entry.get().strip()
            self.canvas.delete(window_id)
            self.text_entry.destroy()
            self.text_entry = None
            if value:
                self.ops.append(DrawOp("text", [(x, y)], self.color, self.width, value))
                self._redraw()

        self.text_entry.bind("<Return>", commit)
        self.text_entry.bind("<Escape>", lambda _event: self._cancel_text(window_id))

    def _cancel_text(self, window_id):
        self.canvas.delete(window_id)
        if self.text_entry:
            self.text_entry.destroy()
            self.text_entry = None

    def copy(self):
        try:
            copy_image_to_clipboard(self._render_image())
            self.status.configure(text="Imagen copiada al portapapeles.")
        except Exception as exc:
            messagebox.showerror(APP_NAME, str(exc))

    def save(self):
        DEFAULT_DIR.mkdir(parents=True, exist_ok=True)
        default_name = f"zaetta_capture_{datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}.png"
        path = filedialog.asksaveasfilename(
            title="Guardar captura",
            initialdir=str(DEFAULT_DIR),
            initialfile=default_name,
            defaultextension=".png",
            filetypes=[("PNG", "*.png")],
        )
        if path:
            self._render_image().save(path)
            self.status.configure(text=f"Guardado: {path}")

    def undo(self):
        if self.ops:
            self.ops.pop()
            self._redraw()
            self.status.configure(text="Ultimo cambio revertido.")

    def clear(self):
        self.ops.clear()
        self._redraw()
        self.status.configure(text="Anotaciones eliminadas.")

    def _context(self, event):
        menu = tk.Menu(self, tearoff=False)
        menu.add_command(label="Copy", command=self.copy)
        menu.add_command(label="Save as PNG", command=self.save)
        menu.add_separator()
        menu.add_command(label="Duplicate selected element", command=lambda: None)
        menu.add_command(label="Cut", command=self.undo)
        menu.add_command(label="Delete", command=self.undo)
        menu.add_separator()
        menu.add_command(label="Reset size", command=lambda: None)
        menu.tk_popup(event.x_root, event.y_root)

    def close(self):
        self.destroy()
        self.on_close()


class ZaettaCaptureApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(APP_NAME)
        self.configure(bg=MINIMAL_BG)
        self.geometry("500x390")
        self.resizable(False, False)
        self.attributes("-alpha", 0.97)
        self.app_icon_photo = ImageTk.PhotoImage(make_zaetta_icon(64))
        self.iconphoto(True, self.app_icon_photo)
        self.hotkey_enabled = False
        self.hotkey_handle = None
        self.hotkey_handles = []
        self.special_hotkey_stop = threading.Event()
        self.special_hotkey_thread = None
        self.config_data = self._load_config()
        self.hotkey = self.config_data.get("hotkey", "ctrl+shift+s")
        self.evidence_mode = bool(self.config_data.get("evidence_mode", False))
        self.tray_icon = None
        self._build()
        self._register_hotkey()
        self._setup_tray()
        self.protocol("WM_DELETE_WINDOW", self.hide_window)

    def _build(self):
        container = tk.Frame(self, bg=MINIMAL_BG, padx=26, pady=24)
        container.pack(fill="both", expand=True)

        header = tk.Frame(container, bg=MINIMAL_BG)
        header.pack(fill="x")
        tk.Label(header, image=self.app_icon_photo, bg=MINIMAL_BG).pack(side="left", padx=(0, 16))
        title_box = tk.Frame(header, bg=MINIMAL_BG)
        title_box.pack(side="left", fill="x", expand=True)
        tk.Label(title_box, text="Zaetta", bg=MINIMAL_BG, fg=MINIMAL_TEXT, font=("Segoe UI", 25, "bold")).pack(anchor="w")
        tk.Label(title_box, text="Captura rapida, privada y local.", bg=MINIMAL_BG, fg=MINIMAL_MUTED, font=("Segoe UI", 10)).pack(anchor="w", pady=(2, 0))

        actions = tk.Frame(container, bg=MINIMAL_PANEL, padx=18, pady=18, highlightthickness=1, highlightbackground=MINIMAL_LINE)
        actions.pack(fill="x", pady=(24, 0))
        RoundedButton(
            actions,
            "Capturar",
            self.start_capture,
            width=150,
            height=42,
            bg="#111827",
            hover="#243244",
        ).pack(side="left", padx=(0, 12))
        RoundedButton(
            actions,
            "Atajo",
            self.change_hotkey,
            width=108,
            height=42,
            bg=MINIMAL_SOFT,
            fg=MINIMAL_TEXT,
            hover="#dcecf4",
        ).pack(side="left", padx=(0, 12))
        self.evidence_button = RoundedButton(
            actions,
            self._short_evidence_label(),
            self.toggle_evidence_mode,
            width=138,
            height=42,
            bg=MINIMAL_SOFT,
            fg=MINIMAL_TEXT,
            hover="#dcecf4",
        )
        self.evidence_button.pack(side="left")

        info = tk.Frame(container, bg=MINIMAL_BG)
        info.pack(fill="x", pady=(20, 0))
        tk.Label(info, text="Atajo activo", bg=MINIMAL_BG, fg=MINIMAL_MUTED, font=("Segoe UI", 9, "bold")).pack(anchor="w")
        self.hotkey_label = tk.Label(info, text=self.hotkey, bg=MINIMAL_BG, fg=MINIMAL_TEXT, font=("Segoe UI", 18, "bold"))
        self.hotkey_label.pack(anchor="w", pady=(2, 10))

        detail = tk.Frame(container, bg=MINIMAL_SOFT, padx=16, pady=12)
        detail.pack(fill="x")
        tk.Label(
            detail,
            text="Vive en la bandeja del sistema. Copia al portapapeles, guarda PNG local y permite marcar evidencia sin subir datos.",
            bg=MINIMAL_SOFT,
            fg=MINIMAL_MUTED,
            font=("Segoe UI", 9),
            wraplength=410,
            justify="left",
        ).pack(anchor="w")

        self.status = tk.Label(container, text="", bg=MINIMAL_BG, fg=MINIMAL_MUTED, font=("Segoe UI", 9))
        self.status.pack(anchor="w", pady=(16, 0))

    def _register_hotkey(self):
        try:
            import keyboard

            self._remove_hotkeys()
            if "+" not in self.hotkey and self.hotkey in {"delete", "del", "supr", "print screen", "prtscn"}:
                self._start_special_hotkey_watcher(self.hotkey)
            else:
                self.hotkey_handles.append(keyboard.add_hotkey(self.hotkey, lambda: self.after(0, self.start_capture)))
            self.hotkey_enabled = True
            self.status.configure(text=f"Listo. Use {self.hotkey} o el boton Capturar.")
            return True
        except Exception as exc:
            self.hotkey_enabled = False
            self.status.configure(text=f"Atajo global no disponible. Use el boton Capturar. Detalle: {exc}")
            return False

    def _remove_hotkeys(self):
        self.special_hotkey_stop.set()
        self.special_hotkey_thread = None
        self.special_hotkey_stop = threading.Event()
        try:
            import keyboard

            for handle in self.hotkey_handles:
                try:
                    keyboard.remove_hotkey(handle)
                except Exception:
                    try:
                        keyboard.unhook(handle)
                    except Exception:
                        pass
            self.hotkey_handles = []
            self.hotkey_handle = None
        except Exception:
            self.hotkey_handles = []
            self.hotkey_handle = None

    def _start_special_hotkey_watcher(self, hotkey):
        vk_map = {
            "delete": 0x2E,
            "del": 0x2E,
            "supr": 0x2E,
            "print screen": 0x2C,
            "prtscn": 0x2C,
        }
        vk_code = vk_map.get(hotkey)
        if not vk_code:
            raise ValueError(f"Tecla especial no soportada: {hotkey}")

        stop_event = self.special_hotkey_stop

        def watch():
            user32 = ctypes.windll.user32
            previous_down = False
            while not stop_event.is_set():
                pressed = bool(user32.GetAsyncKeyState(vk_code) & 0x8000)
                if pressed and not previous_down:
                    self.after(0, self.start_capture)
                previous_down = pressed
                stop_event.wait(0.045)

        self.special_hotkey_thread = threading.Thread(target=watch, daemon=True)
        self.special_hotkey_thread.start()

    def _load_config(self):
        try:
            if CONFIG_PATH.exists():
                return json.loads(CONFIG_PATH.read_text(encoding="utf-8"))
        except Exception:
            pass
        return {}

    def _save_config(self):
        DEFAULT_DIR.mkdir(parents=True, exist_ok=True)
        CONFIG_PATH.write_text(json.dumps(self.config_data, indent=2), encoding="utf-8")

    def change_hotkey(self):
        value = simpledialog.askstring(
            "Cambiar atajo",
            "Escriba el nuevo atajo.\nEjemplos: print screen, impr pant, suprimir, ctrl+shift+s, alt+q",
            initialvalue=self.hotkey,
            parent=self,
        )
        if not value:
            return
        value = normalize_hotkey(value)
        old_hotkey = self.hotkey
        self.hotkey = value
        self.config_data["hotkey"] = value
        if self._register_hotkey():
            self._save_config()
            self.hotkey_label.configure(text=self.hotkey)
            messagebox.showinfo(APP_NAME, f"Atajo actualizado: {self.hotkey}")
        else:
            self.hotkey = old_hotkey
            self.config_data["hotkey"] = old_hotkey
            self._register_hotkey()
            messagebox.showerror(APP_NAME, f"No se pudo registrar el atajo '{value}'.")

    def _setup_tray(self):
        try:
            import pystray
        except Exception:
            self.status.configure(text=f"{self.status.cget('text')} Bandeja no disponible: instale pystray.")
            return

        image = self._tray_image()
        menu = pystray.Menu(
            pystray.MenuItem("Capturar ahora", lambda _icon, _item: self.after(0, self.start_capture), default=True),
            pystray.MenuItem("Copiar ultima captura", lambda _icon, _item: self.after(0, self.copy_last_capture)),
            pystray.MenuItem("Abrir historial", lambda _icon, _item: self.after(0, self.open_history)),
            pystray.MenuItem("Modo evidencia", lambda _icon, _item: self.after(0, self.toggle_evidence_mode), checked=lambda _item: self.evidence_mode),
            pystray.MenuItem("Acerca de Zaetta", lambda _icon, _item: self.after(0, self.show_about)),
            pystray.MenuItem("Abrir Zaetta Capture", lambda _icon, _item: self.after(0, self.show_window)),
            pystray.MenuItem("Cambiar atajo", lambda _icon, _item: self.after(0, self._open_hotkey_window)),
            pystray.MenuItem("Salir", lambda _icon, _item: self.after(0, self.exit_app)),
        )
        self.tray_icon = pystray.Icon("zaetta_capture", image, APP_NAME, menu)
        threading.Thread(target=self.tray_icon.run, daemon=True).start()
        self.after(350, self.hide_window)

    def _tray_image(self):
        return make_zaetta_icon(64)

    def hide_window(self):
        self.withdraw()

    def show_window(self):
        self.deiconify()
        self.lift()
        self.focus_force()

    def _open_hotkey_window(self):
        self.show_window()
        self.change_hotkey()

    def show_about(self):
        self.show_window()
        messagebox.showinfo(
            "Acerca de Zaetta",
            "Zaetta Capture\n\n"
            "Desarrollado por:\n"
            "VICTOR ALEXIS ALZATE CORTES\n\n"
            f"Version: {APP_VERSION}\n\n"
            "Capturador local para evidencia interna.",
            parent=self,
        )

    def _evidence_label(self):
        return "Modo evidencia: ON" if self.evidence_mode else "Modo evidencia: OFF"

    def _short_evidence_label(self):
        return "Evidencia ON" if self.evidence_mode else "Evidencia OFF"

    def toggle_evidence_mode(self):
        self.evidence_mode = not self.evidence_mode
        self.config_data["evidence_mode"] = self.evidence_mode
        self._save_config()
        if hasattr(self, "evidence_button"):
            self.evidence_button.text = self._short_evidence_label()
            self.evidence_button.draw()
        self.status.configure(text=f"{self._evidence_label()}.")

    def copy_last_capture(self):
        latest = DEFAULT_DIR / "ultima_captura.png"
        if not latest.exists():
            messagebox.showinfo(APP_NAME, "No hay ultima captura guardada todavia.")
            return
        try:
            image = Image.open(latest).convert("RGB")
            copy_image_to_clipboard(image)
        except Exception as exc:
            messagebox.showerror(APP_NAME, f"No se pudo copiar la ultima captura.\n\n{exc}")

    def open_history(self):
        HISTORY_DIR.mkdir(parents=True, exist_ok=True)
        os.startfile(str(HISTORY_DIR))

    def exit_app(self):
        if self.tray_icon:
            self.tray_icon.stop()
        self._remove_hotkeys()
        self.destroy()

    def start_capture(self):
        self.withdraw()
        self.after(160, lambda: LightshotOverlay(self, self._after_capture))

    def _after_capture(self):
        if self.tray_icon:
            self.withdraw()
            return
        self.show_window()


def main():
    app = ZaettaCaptureApp()
    app.mainloop()


if __name__ == "__main__":
    main()

