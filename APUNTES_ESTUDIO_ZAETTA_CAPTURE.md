# Apuntes de estudio - Zaetta Capture

Fecha de creacion: 2026-08-01
Zona horaria usada: America/Bogota

Este documento esta escrito para estudiar la app sin perderse en todos los archivos.
La idea no es memorizar codigo, sino entender que pieza hace que cosa, donde vive y como se conecta con las demas.

## 1. Mapa simple de la app

Zaetta Capture se puede entender como 7 bloques grandes:

1. Arranque de la app.
2. Bandeja del sistema.
3. Captura y edicion visual.
4. Preferencias del usuario.
5. Actualizaciones automaticas.
6. Instalador.
7. Pagina web y distribucion.

### 1.1 Arranque de la app

Archivo principal:

- `ZAETTA_CAPTURE_NATIVE/App/Program.cs`

Que hace:

- Es la primera puerta de entrada de Zaetta Capture.
- Activa DPI nativo para que la app se vea bien en Windows.
- Crea una sola instancia usando `Mutex`.
- Si ya hay otra Zaetta abierta, la nueva se cierra para no duplicar iconos en la bandeja.
- Inicia `TrayContext`, que es el controlador principal de la app en bandeja.

En palabras simples:

`Program.cs` prende la app y entrega el control a `TrayContext`.

### 1.2 Bandeja del sistema

Archivo principal:

- `ZAETTA_CAPTURE_NATIVE/App/TrayContext.cs`

Que hace:

- Crea el icono de Zaetta en la bandeja de Windows.
- Crea el menu de click derecho.
- Maneja `Capturar ahora`.
- Maneja `Repetir ultima area`.
- Abre `Opciones`.
- Abre historial.
- Busca actualizaciones.
- Registra el atajo de teclado.
- Recuerda si hay una captura activa.
- Espera a que cierres una captura antes de mostrar una actualizacion pendiente.

En palabras simples:

`TrayContext.cs` es el cerebro de la app mientras Zaetta vive en la bandeja.

### 1.3 Captura y edicion visual

Archivos principales:

- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Input.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Interaction.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Rendering.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Text.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Numbering.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Commands.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Toolbar.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Tools.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Keyboard.cs`
- `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.Adjustments.cs`

Que hace:

- Muestra la captura completa de la pantalla.
- Permite seleccionar un area.
- Permite cambiar de area sin cerrar la captura.
- Permite dibujar flechas, rectangulos, resaltados, texto, numeros y pixelado.
- Permite mover anotaciones.
- Permite copiar o guardar el resultado.
- Maneja teclado como `Esc`, `Ctrl + C`, `Ctrl + S`, `Ctrl + Z`, `M`, `+`, `-`.
- Dibuja el caret de texto y la seleccion estilo Lightshot.

En palabras simples:

`CaptureOverlay` es la pantalla donde realmente ocurre la captura y la edicion.

### 1.4 Preferencias del usuario

Archivos principales:

- `ZAETTA_CAPTURE_NATIVE/App/SettingsForm.cs`
- `ZAETTA_CAPTURE_NATIVE/App/HotkeyCaptureForm.cs`
- `ZAETTA_CAPTURE_NATIVE/Storage/CapturePreferencesStore.cs`
- `ZAETTA_CAPTURE_NATIVE/Storage/HotkeyPreference.cs`
- `ZAETTA_CAPTURE_NATIVE/Storage/LastSelectionStore.cs`

Que hace:

- Muestra la ventana `Opciones`.
- Permite cambiar el atajo de captura.
- Guarda si Zaetta debe mantener la ultima area.
- Guarda si las capturas deben abrir con candado.
- Guarda el atajo elegido para que sobreviva a actualizaciones.
- Guarda la ultima seleccion usada.

En palabras simples:

Este bloque recuerda como el usuario quiere trabajar.

### 1.5 Actualizaciones automaticas

Archivos principales:

- `ZAETTA_CAPTURE_NATIVE/Updates/UpdateService.cs`
- `ZAETTA_CAPTURE_NATIVE/Updates/UpdateInfo.cs`
- `ZAETTA_CAPTURE_NATIVE/App/UpdatePromptForm.cs`
- `ZAETTA_CAPTURE_NATIVE/App/UpdateProgressForm.cs`
- `ZAETTA_CAPTURE_NATIVE/App/UpdateToastForm.cs`
- `website/latest.json`

Que hace:

- Lee `https://www.zaettasoftware.com/latest.json`.
- Compara la version instalada con la version publicada.
- Si hay una version nueva, muestra un aviso.
- Descarga el instalador desde GitHub Releases.
- Muestra barra de progreso.
- Verifica SHA256 para confirmar que el archivo descargado es el correcto.
- Ejecuta el instalador en modo upgrade.
- Cierra la app vieja y abre la nueva.

En palabras simples:

El updater pregunta en la pagina web cual es la ultima version y descarga el instalador correcto desde GitHub.

### 1.6 Instalador

Archivo principal:

- `ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`

Que hace:

- Copia la app a `%LOCALAPPDATA%\Zaetta Capture`.
- Crea acceso directo.
- Registra inicio con Windows.
- Registra desinstalador.
- En modo `/upgrade`, instala sin pedir click extra.
- Al finalizar, lanza Zaetta para que quede en bandeja.

En palabras simples:

El instalador coloca Zaetta en la maquina y la deja lista para usar.

### 1.7 Pagina web y distribucion

Archivos principales:

- `website/index.html`
- `website/styles.css`
- `website/latest.json`
- `website/README.md`

Servicios usados:

- DreamHost: dominio y DNS.
- Vercel: aloja la pagina y `latest.json`.
- GitHub Releases: aloja los instaladores `.exe`.

En palabras simples:

La pagina vende/explica Zaetta y tambien le dice a la app cual es la ultima version disponible.

## 2. Inventario de componentes

### `Program.cs`

Tipo:

- Arranque de aplicacion.

Responsabilidad:

- Iniciar Zaetta.
- Evitar instancias duplicadas con `Mutex`.
- Manejar errores graves de arranque.

Se conecta con:

- `TrayContext`
- `NativeDpi`
- `StartupDiagnostics`

### `TrayContext.cs`

Tipo:

- Controlador principal de bandeja.

Responsabilidad:

- Menu de bandeja.
- Icono de bandeja.
- Atajos.
- Inicio de captura.
- Chequeo de updates.
- Apertura de opciones.

Se conecta con:

- `CaptureOverlay`
- `SettingsForm`
- `UpdateService`
- `UpdatePromptForm`
- `UpdateProgressForm`
- `UpdateToastForm`
- `CapturePreferencesStore`
- `StartupService`
- `HistoryService`

### `SettingsForm.cs`

Tipo:

- Formulario de opciones.

Responsabilidad:

- Mostrar opciones en una ventana oscura/dorada.
- Separar `General` y `Atajo`.
- Permitir guardar preferencias.

Se conecta con:

- `TrayContext`
- `HotkeyCaptureForm`
- `CapturePreferencesStore`

### `HotkeyCaptureForm.cs`

Tipo:

- Formulario auxiliar.

Responsabilidad:

- Capturar una combinacion de teclas elegida por el usuario.
- Devolver la tecla y modificadores a `SettingsForm` o `TrayContext`.

### `CaptureOverlay.*.cs`

Tipo:

- Overlay de captura dividido en archivos `partial`.

Responsabilidad:

- Capturar area.
- Pintar pantalla.
- Manejar mouse y teclado.
- Dibujar herramientas.
- Editar texto directo.
- Mover objetos.
- Copiar/guardar resultado.

Por que esta dividido:

- Porque un solo archivo gigante seria dificil de estudiar.
- Cada archivo tiene una responsabilidad mas concreta.

### `ScreenshotService.cs`

Tipo:

- Servicio de captura de pantalla.

Responsabilidad:

- Tomar imagen del escritorio virtual completo.
- Incluir todos los monitores conectados.

### `ScreenshotCapture.cs`

Tipo:

- Modelo de datos.

Responsabilidad:

- Guardar la imagen capturada y sus bounds.

### `DrawOp.cs`

Tipo:

- Modelo de anotacion.

Responsabilidad:

- Representar una cosa dibujada: flecha, texto, numero, pixelado, rectangulo, etc.

### `DrawingStyle.cs`

Tipo:

- Modelo de estilo visual.

Responsabilidad:

- Guardar color, grosor, tamano y detalles visuales usados al dibujar.

### `Pixelation.cs`

Tipo:

- Utilidad de edicion.

Responsabilidad:

- Aplicar pixelado real a una region.
- Ajustar intensidad.

### `Tool.cs`

Tipo:

- Enum/lista de herramientas.

Responsabilidad:

- Nombrar herramientas como flecha, texto, pixelado, mover, etc.

### `ToolShortcuts.cs`

Tipo:

- Mapa de atajos.

Responsabilidad:

- Relacionar teclas con herramientas.

### `UpdateService.cs`

Tipo:

- Servicio de actualizaciones.

Responsabilidad:

- Descargar `latest.json`.
- Manejar redirects.
- Parsear datos del manifest.
- Comparar versiones.
- Verificar SHA256.

### `UpdateInfo.cs`

Tipo:

- Modelo de datos del update.

Responsabilidad:

- Guardar version, URL, SHA256, notas y tamano del instalador.

### `UpdatePromptForm.cs`

Tipo:

- Ventana de decision.

Responsabilidad:

- Avisar que hay update.
- Permitir actualizar o posponer.

### `UpdateProgressForm.cs`

Tipo:

- Ventana de progreso.

Responsabilidad:

- Descargar instalador.
- Mostrar barra.
- Validar SHA256.
- Ejecutar instalador `/upgrade`.

### `UpdateToastForm.cs`

Tipo:

- Aviso visual cerca de la bandeja.

Responsabilidad:

- Mostrar el aviso bonito antes del prompt grande.

### `InstallerZaettaFinal.cs`

Tipo:

- Instalador.

Responsabilidad:

- Instalar, actualizar y registrar Zaetta en Windows.

### `CapturePreferencesStore.cs`

Tipo:

- Persistencia local.

Responsabilidad:

- Leer y escribir `capture-preferences.txt`.
- Guardar `keepLastSelectionPosition`.
- Guardar `openLocked`.
- Guardar `hotkeyKey`.
- Guardar `hotkeyModifiers`.

### `LastSelectionStore.cs`

Tipo:

- Persistencia local.

Responsabilidad:

- Guardar y cargar la ultima area seleccionada.

### `HistoryService.cs`

Tipo:

- Servicio de historial/carpeta.

Responsabilidad:

- Abrir o administrar la carpeta donde quedan capturas.

### `Paths.cs`

Tipo:

- Centralizador de rutas.

Responsabilidad:

- Definir carpetas usadas por la app: base, updates, capturas, etc.

### `StartupService.cs`

Tipo:

- Integracion con Windows.

Responsabilidad:

- Registrar Zaetta para iniciar con Windows.

### `HotKeyWindow.cs`

Tipo:

- Integracion con Windows.

Responsabilidad:

- Registrar atajos globales como `Impr Pant`.

### `ClipboardHelper.cs`

Tipo:

- Integracion con portapapeles.

Responsabilidad:

- Copiar imagen al portapapeles de Windows.

### `Ui.cs`

Tipo:

- Paleta visual.

Responsabilidad:

- Guardar colores globales: fondo, panel, texto, dorado.

### `ZaettaButton.cs`

Tipo:

- Control visual reutilizable.

Responsabilidad:

- Dibujar botones oscuros/dorados con estilo Zaetta.

### `FloatingToolbarPanel.cs`

Tipo:

- Panel visual.

Responsabilidad:

- Mostrar herramientas flotantes del overlay.

### `DarkMenuRenderer.cs` y `ContextMenus.cs`

Tipo:

- Estilo de menus.

Responsabilidad:

- Hacer que menus contextuales se vean oscuros y consistentes.

## 3. Flujos principales

### 3.1 Que pasa cuando abres Zaetta

1. Windows ejecuta `Zaetta Capture.exe`.
2. Entra a `Program.Main()`.
3. `Program` crea un `Mutex`.
4. Si ya hay una instancia abierta, la nueva se cierra.
5. Si no hay instancia abierta, se activa DPI.
6. Se crea `TrayContext`.
7. `TrayContext` crea icono de bandeja.
8. `TrayContext` carga preferencias.
9. `TrayContext` registra el atajo.
10. `TrayContext` programa chequeos de update.

### 3.2 Que pasa cuando presionas Impr Pant

1. `HotKeyWindow` detecta el atajo global.
2. Llama a `TrayContext.StartCapture()`.
3. `ScreenshotService` captura todos los monitores.
4. Se abre `CaptureOverlay`.
5. El usuario selecciona area.
6. El usuario edita o copia/guarda.
7. Si la captura termina, se guarda la ultima area.
8. Si habia update pendiente, se muestra despues de cerrar el overlay.

### 3.3 Que pasa cuando cambias opciones

1. Click derecho en bandeja.
2. Click en `Opciones...`.
3. `TrayContext.ShowSettings()` abre `SettingsForm`.
4. El usuario cambia `General` o `Atajo`.
5. Al guardar, `TrayContext` recibe los valores.
6. `CapturePreferencesStore` los escribe en disco.
7. Si cambio el atajo, `HotKeyWindow` registra el nuevo.

### 3.4 Que pasa cuando hay una actualizacion

1. `TrayContext` llama `BeginUpdateCheck()`.
2. `UpdateService` descarga `latest.json`.
3. `UpdateService` compara version remota contra `AppInfo.Version`.
4. Si la remota es mayor, devuelve `UpdateInfo`.
5. `TrayContext` muestra `UpdateToastForm`.
6. Luego muestra `UpdatePromptForm`.
7. Si el usuario acepta, abre `UpdateProgressForm`.
8. `UpdateProgressForm` descarga el instalador.
9. Verifica SHA256.
10. Ejecuta el instalador con `/upgrade`.
11. La app vieja se cierra.
12. El instalador copia la nueva version.
13. El instalador abre Zaetta otra vez en bandeja.

### 3.5 Que pasa cuando publicamos una version nueva

1. Se cambia `AppInfo.Version`.
2. Se compila la app y el instalador.
3. Se calcula SHA256 del instalador.
4. Se sube `ZaettaCaptureSetup.exe` a GitHub Releases.
5. Se actualiza `website/latest.json`.
6. Se sube a GitHub.
7. Vercel publica el manifest.
8. Las apps instaladas detectan la version nueva.

## 4. Diccionario C# usando Zaetta

### Clase

Una clase es una pieza de codigo que agrupa datos y acciones.

Ejemplo:

`TrayContext` es una clase. Representa el control principal de la app en bandeja.

### Metodo

Un metodo es una accion que una clase puede hacer.

Ejemplos:

- `StartCapture()` inicia una captura.
- `ShowSettings()` abre opciones.
- `BeginUpdateCheck()` busca actualizaciones.

### Variable

Una variable guarda un dato.

Ejemplo:

`captureActive` guarda si hay una captura abierta en este momento.

### Evento

Un evento es algo que pasa y dispara codigo.

Ejemplo:

Cuando haces click en `Opciones...`, el menu dispara el codigo que abre `SettingsForm`.

### Form

Un `Form` es una ventana de Windows Forms.

Ejemplos:

- `SettingsForm` es la ventana de opciones.
- `UpdatePromptForm` es la ventana que avisa una actualizacion.
- `UpdateProgressForm` es la ventana de descarga.

### DialogResult

Es la respuesta de una ventana.

Ejemplo:

Si el usuario da click en `Guardar`, `SettingsForm` devuelve `DialogResult.OK`.

### Timer

Es un reloj que ejecuta algo cada cierto tiempo.

Ejemplo:

`TrayContext` usa un timer para revisar actualizaciones.

### Mutex

Es un candado de sistema para que solo exista una instancia.

Ejemplo:

`Program.cs` usa `Mutex` para evitar multiples iconos en la bandeja.

### try/catch

Es una forma de intentar algo y manejar el error si falla.

Ejemplo:

Si falla el arranque, `Program.cs` guarda el error y muestra un mensaje.

### JSON

Es un formato de texto para datos.

Ejemplo:

`website/latest.json` dice:

- producto
- version
- URL de descarga
- SHA256
- notas

### SHA256

Es una huella digital de un archivo.

Ejemplo:

El updater descarga el instalador y revisa que su SHA256 coincida con el manifest.

## 5. Bitacora por horas - 2026-08-01

Las horas vienen de Git cuando hay commit. En tareas pequeñas entre commits, la hora es aproximada por secuencia.

### 01:20 - Upgrade features

Commit:

- `8bd74d0` - `upgrade features`

Que representa:

- Primer bloque fuerte de features de upgrade y distribucion.

### 01:24 - Limpieza de archivos accidentales

Commit:

- `c7d415e` - `Remove accidental audio files`

Que representa:

- Se retiraron archivos que no debian entrar al repo.

### 01:51 - Manifest apuntando a GitHub Release

Commit:

- `a62ecb9` - `Point updater manifest to GitHub release`

Que representa:

- `latest.json` queda apuntando al instalador versionado en GitHub Releases.

### 01:59 - Documentacion de release v1

Commit:

- `523e92e` - `Document v1 release`

Que representa:

- Se documenta el primer release oficial.

### 02:10 - Updater interno

Commit:

- `18686c4` - `Add in-app updater`

Que representa:

- Se agrego el sistema para revisar, descargar y ejecutar actualizaciones desde la app.

### 02:16 - Fix redirect del manifest

Commit:

- `82971f1` - `Fix updater manifest redirect`

Que representa:

- Se cambio el endpoint para evitar error `308 Permanent Redirect`.

### 02:20 - Manejo manual de redirects

Commit:

- `250db0b` - `Handle updater redirects manually`

Que representa:

- El updater aprende a seguir redirects HTTP sin depender del comportamiento automatico.

### 02:26 - Release de prueba del updater

Commit:

- `5c7744c` - `Prepare updater test release v1.0.4`

Que representa:

- Version usada para probar que una app instalada detecte una version mayor.

### 02:30 - Prompt automatico mas visible

Commit:

- `471afdf` - `Make update prompt more visible`

Que representa:

- El aviso de update se hace mas invasivo y visible.

### 02:36 - Instalador automatico en upgrade

Commit:

- `998aea2` - `Automate updater installer mode`

Que representa:

- El instalador en modo `/upgrade` ya no debe esperar click manual en `Instalar`.

### 02:41 - Release de prueba v1.0.7

Commit:

- `e35325d` - `Prepare updater test release v1.0.7`

Que representa:

- Version creada para probar de nuevo el flujo completo.

### 02:44 - Reemplazo robusto del upgrade

Commit:

- `39320bf` - `Make installer upgrade replacement robust`

Que representa:

- El instalador deja de depender de borrar toda la carpeta y reemplaza archivos puntuales.

### 02:49 - Descargas del updater en AppData

Commit:

- `dbd637b` - `Move updater downloads to app data`

Que representa:

- Los instaladores descargados por update pasan a `%LOCALAPPDATA%`.

### 02:57 - Upgrade menos agresivo para antivirus

Commit:

- `bde57b8` - `Make upgrade less aggressive for antivirus`

Que representa:

- Se reduce comportamiento sospechoso para Bitdefender: no matar procesos ni limpiar legacy durante upgrade automatico.

### 03:32 - Globo de actualizacion restaurado

Commit:

- `b11116a` - `Restore update tray balloon`

Que representa:

- Se vuelve a mostrar aviso de bandeja antes de la ventana grande.

### 03:39 - Toast propio de update

Commit:

- `43ce889` - `Add update toast notification`

Que representa:

- Se agrega una ventana propia tipo toast cerca de la bandeja.

### 03:46 - Toast mas visible

Commit:

- `0b383ba` - `Make update toast more visible`

Que representa:

- El toast dura mas tiempo, aparece al frente y se ubica en la pantalla activa.

### 03:52 - Instancia unica

Commit:

- `6d844f8` - `Prevent duplicate tray instances`

Que representa:

- Se evita que abrir Zaetta varias veces cree varios iconos en bandeja.

### 04:03 - Numeracion corregida

Commit:

- `0afb747` - `Fix number marker sequencing`

Que representa:

- Los numeros ya no se reinician despues de 9 y `Undo`/`Esc` no rompen el contador.

### 04:12 - Chequeo de updates mas agresivo

Commit:

- `bcebd96` - `Improve update detection cadence`

Que representa:

- La app revisa updates cada 30 segundos al iniciar y luego cada 5 minutos.

### 04:22 - Atajo persistente

Commit:

- `a2b3de9` - `Persist capture hotkey preference`

Que representa:

- El shortcut elegido por el usuario se conserva despues de actualizar.

### 14:18 - Toast en detecciones automaticas

Commit:

- `e37c413` - `Show toast for automatic updates`

Que representa:

- El toast bonito aparece tambien cuando la app detecta updates sola, no solo al buscar manualmente.

### 14:27 - Ventana Opciones

Commit:

- `29bba2e` - `Add tray options dialog`

Que representa:

- Se agrega `Opciones...` para configurar comportamiento y atajo.

### 14:38 - Opciones pulidas y menu limpio

Commit:

- `1492350` - `Polish tray options UI`

Que representa:

- Se quitan tabs blancos.
- Se agrega navegacion lateral oscura/dorada.
- El menu de bandeja deja solo acciones importantes.

### 14:44 - Web actualizada a v1.0.20

Commit:

- `6348f2d` - `Update website download version`

Que representa:

- La landing deja de mostrar `1.0.11` y apunta a `1.0.20`.

### 14:52 - Rediseño de landing

Commit:

- `bb45665` - `Redesign marketing landing page`

Que representa:

- La pagina pasa de minima a landing completa de producto.

### 14:54 - Ajuste fino contra referencia visual

Commit:

- `5a640d1` - `Refine landing page against reference`

Que representa:

- La landing se acerca mas a la imagen de referencia: header, hero, mockup, footer y microdetalles.

### 20:55 - Pixelador con maximo mas fuerte

Commit:

- `cf22147` - `Adjust pixelation intensity`

Release:

- `v1.0.21`

Que representa:

- `Pixelation.MaxIntensity` sube a `70`.
- `Pixelation.DefaultIntensity` se mantiene en `12`.
- La herramienta Pixelar sigue arrancando suave/normal, pero el usuario puede subir mucho mas la fuerza cuando necesita ocultar informacion sensible.

Que se aprendio:

- `MaxIntensity` es el techo maximo permitido.
- `DefaultIntensity` es el valor inicial cuando se crea un pixelado nuevo.
- Cambiar solo el maximo no altera el arranque; solo permite llegar mas lejos con `+`, scroll o ajuste sobre la anotacion.

## 6. Como usar estos apuntes

Si quieres pedir una feature nueva:

1. Mira si es bandeja, overlay, update, instalador o web.
2. Busca el componente en el inventario.
3. Pide el cambio mencionando el componente.

Ejemplos:

- "Esto es del overlay, ajustemos `CaptureOverlay.Text.cs`."
- "Esto es del updater, revisemos `UpdateService.cs`."
- "Esto es del menu de bandeja, miremos `TrayContext.cs`."
- "Esto es del instalador, miremos `InstallerZaettaFinal.cs`."

La meta es que puedas dirigir Codex con mas precision y entender por que se toca cada archivo.
