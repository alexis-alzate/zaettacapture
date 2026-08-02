# Contexto de Trabajo - Zaetta Capture

## Guia de estudio separada

Para aprender la app sin leer toda la bitacora tecnica, usar primero:

- `APUNTES_ESTUDIO_ZAETTA_CAPTURE.md`

Ese archivo explica Zaetta Capture por niveles:

- mapa simple de la app
- inventario de componentes
- flujos principales
- diccionario C# con ejemplos reales
- bitacora del 2026-08-01 con horas de commits

Este `CONTEXTO_ZAETTA_CAPTURE.md` queda como memoria completa del proyecto, mas larga y tecnica.

## 1. Que es Zaetta Capture

Zaetta Capture es una aplicacion de escritorio independiente para capturar pantalla, editar la evidencia y copiarla rapido al portapapeles.

La idea principal es tener una alternativa interna, liviana y controlada al flujo de herramientas tipo Lightshot o Greenshot, evitando depender de instaladores externos que puedan venir con componentes no deseados o alertas de seguridad.

Zaetta Capture no hace parte de PlayOps Suite. Es otro producto y debe mantenerse en un repositorio separado.

## 2. Objetivo del producto

Permitir que el equipo capture evidencia visual de forma rapida, clara y segura.

El flujo esperado es:

1. El usuario activa la captura desde el icono de bandeja o con un atajo.
2. Selecciona un area de la pantalla.
3. La captura queda visible en un editor flotante.
4. Puede dibujar, marcar, escribir texto, pixelar informacion sensible o mover elementos.
5. Puede copiar la imagen al portapapeles.
6. Al copiar, la captura se cierra y el usuario puede pegarla en Teams, WhatsApp, correo o cualquier sistema.

## 3. Estado actual

La version funcional principal es la nativa en C# WinForms:

- Carpeta: `ZAETTA_CAPTURE_NATIVE/`
- Entrada: `App/Program.cs`
- Overlay/editor principal: `Capture/CaptureOverlay.cs`
- Instalador: `InstallerZaettaFinal.cs`
- Build actual probado en Linux con Mono mediante `tools/build-native.sh`.
- Ejecutables generados actuales:
  - `ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe`
  - `INSTALADOR_ZAETTA_CAPTURE_FINAL.exe`

Tambien existe una version inicial en Python:

- Carpeta: `ZAETTA_CAPTURE/`
- Archivo principal: `main.py`

La version Python fue util para prototipar, pero la version nativa se siente mas rapida para seleccionar pantalla y debe ser la base principal si se busca experiencia tipo Lightshot.

## 4. Funcionalidades implementadas

- Icono en bandeja del sistema.
- Click sobre el icono de bandeja para iniciar captura.
- Menu de opciones desde el icono de bandeja.
- Inicio con Windows nativo: la app se registra sola para arrancar con el usuario y el instalador la abre inmediatamente en la bandeja.
- Opcion chuleable `Mantener posicion del area seleccionada`, inspirada en Lightshot, para que cada captura nueva arranque usando el ultimo rectangulo si existe.
- Opcion `Repetir ultima area` desde el menu de bandeja para forzar el ultimo rectangulo usado.
- Opcion chuleable `Abrir capturas con candado` desde el menu de bandeja para usuarios que prefieren iniciar siempre en modo protegido.
- Captura de pantalla con seleccion de area.
- Soporte para multiples pantallas.
- Captura sobre todo el escritorio virtual de Windows usando `SystemInformation.VirtualScreen`, no solo la pantalla donde esta el mouse.
- Proceso marcado como DPI-aware al iniciar para evitar recortes desplazados o a medias en equipos con multiples monitores y escalas distintas.
- Selector visual tipo Lightshot, con borde punteado y fondo atenuado.
- Editor inmediato sobre la seleccion.
- Barra compacta de herramientas.
- Herramientas visibles principales.
- Menu de mas herramientas con tres puntos.
- Botones compactos con iconos dibujados para herramientas principales, estilo oscuro/minimal y estados hover/activo mas pulidos.
- Render de botones ajustado con bordes alineados y sin doble reduccion de rectangulo para evitar desniveles visuales en iconos compactos.
- Tooltips descriptivos en botones.
- Boton de candado en la barra inferior para bloquear temporalmente el cierre al hacer clic fuera de la seleccion.
- Atajo `Ctrl + L` dentro del overlay para activar o desactivar rapidamente el candado.
- Copiar con boton.
- Copiar con clic derecho sobre la captura.
- Copiar con `Ctrl + C`.
- Cierre automatico despues de copiar.
- Cancelar seleccion al hacer clic fuera o con `Esc`.
- Guardar imagen localmente.
- Atajos globales con `RegisterHotKey`. Se retiro el hook global de bajo nivel para reducir falsos positivos de antivirus/Teams.
- Bloqueo de capturas simultaneas para evitar overlays infinitos o capturas cada vez mas oscuras.
- Persistencia local de la ultima area usada y de la preferencia `Mantener posicion del area seleccionada`; cuando esta chuleada, incluso el atajo normal abre el overlay con ese rectangulo marcado sobre una captura nueva.
- Herramienta de texto con edicion directa sobre la captura, estilo Lightshot, sin caja grande de `TextBox`; el texto activo muestra un borde fino, crece con lo escrito y se puede mover arrastrando su borde.
- Herramienta para mover elementos.
- La herramienta `Mover` tambien puede mover la seleccion completa: clic en una anotacion mueve la anotacion; clic en espacio vacio dentro de la seleccion mueve todo el rectangulo y arrastra sus anotaciones.
- Reseleccion de area dentro del mismo overlay: si ya hay una seleccion y el usuario arrastra fuera de ella, puede elegir otra area o pantalla sin cerrar la captura.
- Herramientas de dibujo.
- Flechas con cabeza agrandada mediante `AdjustableArrowCap` para que se vean mas claras en evidencias.
- Ajuste de color para figuras y trazos.
- Ajuste rapido de grosor mientras se dibuja: clic izquierdo sostenido + rueda del mouse arriba/abajo. Se valida con `Control.MouseButtons` y bandera interna porque `MouseWheel` puede llegar con `e.Button = None`.
- Ajuste posterior: al pasar el mouse sobre una anotacion ya creada, la rueda permite cambiar tamano o grosor segun la herramienta; en pixelado ajusta la intensidad del mosaico.
- Pixelado real por mosaico, con intensidad ajustable por operacion.
- Icono y marca visual Zaetta con el logo oficial.
- Ventana "Acerca de" con desarrollador, version y descripcion.
- Instalador `.exe` con barra de progreso.
- Instalador con logo oficial embebido como recurso `ZaettaLogo`.
- Acceso directo en escritorio.
- Inicio automatico con Windows registrado por el instalador y reforzado por la app al abrir; debe quedar disponible en bandeja inmediatamente despues de instalar y tambien despues de reiniciar el equipo.
- Instalacion local en `%LOCALAPPDATA%`.
- Reemplazo/limpieza de versiones anteriores durante instalacion.
- Diagnostico de arranque: si la app falla al iniciar, muestra un mensaje y escribe `Pictures\Zaetta Capture\startup-error.log`.

## 5. Herramientas de edicion esperadas

La aplicacion debe mantenerse cercana al flujo de Lightshot:

- Flecha.
- Marco o rectangulo.
- Linea.
- Lapiz.
- Resaltador.
- Pixelar.
- Numero.
- Texto.
- Mover elementos.
- Deshacer.
- Copiar.
- Guardar.
- Cerrar.

Las herramientas mas usadas deben estar visibles o cerca del area seleccionada. Las herramientas secundarias pueden estar en un menu compacto.

## 6. Reglas importantes de comportamiento

- El selector de captura no debe cambiar de color segun el color de dibujo.
- El selector debe verse como una seleccion limpia y profesional: borde punteado, fondo atenuado y manejadores discretos.
- El color elegido solo aplica a flechas, texto, marcos, lineas, lapiz y resaltador.
- Las flechas deben tener cabeza visible y profesional. Actualmente se usa `AdjustableArrowCap(5.8f, 7.2f, true)`.
- Al copiar, no debe abrir ventana de guardar.
- Al copiar, no debe quedar ninguna ventana flotante activa.
- Al hacer clic derecho sobre la captura sin candado activo, debe copiar sin abrir el menu contextual de Windows.
- Al arrastrar fuera de una seleccion ya creada, debe iniciar una nueva seleccion dentro del mismo overlay, sin cerrar la captura.
- Si el usuario cancela, debe cerrar todo y devolver el control normal del mouse.
- Clic izquierdo sostenido y arrastre fuera de la seleccion debe permitir reseleccionar otra area sin cerrar el capturador.
- Si el candado esta apagado, clic derecho dentro de la seleccion debe copiar y cerrar rapido.
- Si el candado esta activo, clic derecho no debe copiar de inmediato: debe abrir un menu contextual propio de Zaetta con acciones como Copiar, Guardar, Desbloquear y Cancelar.
- Aunque el candado este activo, boton Copiar y `Ctrl + C` deben copiar y cerrar.
- Cuando el usuario elige Copiar desde el menu contextual del candado, debe copiar y cerrar.
- El candado es un estado del overlay activo. Por defecto cada captura inicia desbloqueada, salvo que el usuario active en bandeja la preferencia `Abrir capturas con candado`.
- La preferencia global solo define el estado inicial de nuevas capturas; el usuario siempre puede activar/desactivar el candado dentro del overlay con el boton o con `Ctrl + L`.
- El objetivo del candado es permitir que el usuario mantenga la seleccion visible mientras interactua accidentalmente por fuera del rectangulo, sin perder el recorte que ya tenia listo.
- El programa debe sentirse inmediato; la seleccion no puede tener lag perceptible.
- El instalador debe sobreescribir versiones anteriores y evitar que queden varias copias con nombres distintos.
- El instalador debe registrar siempre el inicio con Windows para el usuario actual. La app no debe requerir permisos de administrador para esto.
- `Iniciar con Windows` no debe ser una opcion visible: es comportamiento nativo de la app.
- Al terminar la instalacion, el instalador debe lanzar el ejecutable instalado para que el icono aparezca de una vez en la bandeja.
- Al activar captura, el usuario debe poder seleccionar cualquier monitor conectado, incluso si el mouse estaba inicialmente en otro monitor. Por eso `StartCapture` debe usar el escritorio virtual completo.
- En equipos con monitores a 125%, 150% o escalas mixtas, la app debe activar DPI awareness antes de crear ventanas; de lo contrario Windows puede virtualizar coordenadas y copiar una zona incorrecta.
- No se deben abrir multiples overlays al mantener presionado `Impr Pant` o al disparar varias veces el atajo. `TrayContext.captureActive` bloquea una nueva captura hasta que el overlay actual cierre.
- Si `Mantener posicion del area seleccionada` esta chuleado, cualquier captura nueva debe usar la ultima area guardada si existe, incluso cuando se active con atajo normal.
- Si `Mantener posicion del area seleccionada` esta deschuleado, una captura normal debe iniciar desde cero.
- `Repetir ultima area` fuerza el uso de la ultima area aunque la opcion automatica este desactivada.
- Mientras una captura este abierta, arrastrar fuera de la seleccion actual inicia una nueva seleccion sin cerrar el overlay. Esto permite cambiar de decision varias veces, incluso entre monitores, hasta que el usuario copie, guarde o cancele manualmente.
- Si la reseleccion nueva es demasiado pequena, se restaura la seleccion anterior para evitar perderla por un clic accidental.
- Cuando una reseleccion valida reemplaza el area anterior, las anotaciones previas se limpian porque pertenecian al recorte anterior.
- La ultima area se guarda en `Pictures\Zaetta Capture\last-selection.txt` como `x,y,width,height`.
- Las preferencias de captura se guardan en `Pictures\Zaetta Capture\capture-preferences.txt`.
- Formato actual de preferencias:

```text
keepLastSelectionPosition=1
openLocked=0
```

- Compatibilidad: si el archivo antiguo solo contiene `1`, `0`, `true` o `false`, se interpreta como el valor legacy de `Mantener posicion del area seleccionada`.

## 7. Atajos actuales y deseados

Atajos deseados:

- `Print Screen`: iniciar captura.
- `Delete` o `Supr`: opcion configurable para iniciar captura si el usuario lo prefiere.
- `Ctrl + Shift + S`: atajo alternativo.
- `Ctrl + C`: copiar captura editada.
- `Ctrl + L`: activar o desactivar candado del overlay.
- `Esc`: cancelar captura o cerrar editor.
- `R`: seleccionar rectangulo.
- `T`: seleccionar texto.
- `F`: seleccionar flecha.
- `L`: seleccionar linea.
- `P`: seleccionar lapiz o pixelar segun configuracion.

Pendiente importante: dejar un panel simple para cambiar el atajo sin tocar codigo.

Nota actual: `Print Screen` se maneja con `RegisterHotKey`, no con hook global de bajo nivel. Esto reduce alertas de seguridad al compartir el instalador por canales corporativos.

## 8. Arquitectura actual

### Version nativa

`ZAETTA_CAPTURE_NATIVE/`

Contiene:

- `App/`: arranque, metadatos de producto, bandeja del sistema, dialogo de atajo y flujo inicial de captura.
- `SystemIntegration/`: DPI awareness, portapapeles, hotkeys globales mediante `RegisterHotKey` e inicio con Windows.
- `Editing/`: herramientas, operaciones de dibujo, pixelado y estilos de dibujo.
- `Storage/`: rutas locales, historial, ultima area y preferencias de captura.
- `UI/`: helpers visuales compartidos.
- `Capture/CaptureOverlay.cs`: estado principal, constructor y helpers comunes del overlay.
- `Capture/CaptureOverlay.Keyboard.cs`: foco, atajos del overlay y supresion de menu contextual.
- `Capture/CaptureOverlay.Input.cs`: eventos de mouse, rueda, seleccion y drag.
- `Capture/CaptureOverlay.Toolbar.cs`: barra de herramientas, menus, colores, grosor e iconos de herramientas.
- `Capture/CaptureOverlay.Tools.cs`: seleccion de herramientas, menus, colores, grosor, iconos y alternancia del candado.
- `Capture/CaptureOverlay.Rendering.cs`: pintado de seleccion, anotaciones, handles y render final.
- `Capture/CaptureOverlay.Interaction.cs`: hit testing, mover, escalar y redimensionar anotaciones/seleccion.
- `Capture/CaptureOverlay.Adjustments.cs`: cambios de tamano/grosor/intensidad por rueda o botones.
- `Capture/CaptureOverlay.Text.cs`: edicion de texto sobre la captura.
- `Capture/CaptureOverlay.Commands.cs`: copiar, guardar y deshacer.
- `Capture/ScreenshotService.cs`: captura del escritorio virtual de Windows.
- `Capture/ScreenshotCapture.cs`: resultado de captura con bounds e imagen.
- `Storage/HistoryService.cs`: guardado y apertura del historial local.
- `Storage/LastSelectionStore.cs`: persistencia de la ultima area seleccionada.
- `Storage/CapturePreferencesStore.cs`: persistencia de `Mantener posicion del area seleccionada`.
- `Storage/CapturePreferencesStore.cs`: tambien persiste `Abrir capturas con candado`.
- `SystemIntegration/StartupService.cs`: lee y actualiza la entrada de inicio automatico del usuario actual en `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- `SystemIntegration/StartupDiagnostics.cs`: log de errores de arranque.
- `Legacy/EditorForm.cs`: editor anterior conservado como referencia.
- `ZaettaCapture.cs`: archivo historico minimo que apunta a la nueva separacion.
- `TrayContext.StartCapture`, actualmente captura `SystemInformation.VirtualScreen` para permitir seleccion libre en cualquier pantalla.
- `TrayContext.captureActive`, bandera que impide abrir mas de una captura al mismo tiempo.

### Candado de captura

El candado vive dentro de `CaptureOverlay` porque el bloqueo real aplica a la captura abierta en ese momento. La bandeja solo guarda una preferencia opcional para definir si nuevas capturas empiezan bloqueadas.

Archivos relacionados:

- `Capture/CaptureOverlay.cs`: campo `selectionLocked`.
- `Capture/CaptureOverlay.Input.cs`: evita cerrar con clic izquierdo fuera de la seleccion cuando el candado esta activo.
- `Capture/CaptureOverlay.Toolbar.cs`: agrega el boton `Lock` en la barra inferior.
- `Capture/CaptureOverlay.Keyboard.cs`: permite alternar el candado con `Ctrl + L`.
- `Capture/CaptureOverlay.Tools.cs`: metodo `ToggleSelectionLock` y menu contextual del modo bloqueado.
- `UI/ZaettaButton.cs`: dibujo del icono de candado.
- `App/TrayContext.cs`: opcion chuleable `Abrir capturas con candado`.
- `Storage/CapturePreferencesStore.cs`: guarda/carga la preferencia `openLocked`.

Comportamiento esperado:

- Desbloqueado o bloqueado: arrastrar fuera de la seleccion actual inicia una nueva seleccion sin cerrar el overlay.
- Si el usuario solo hace un clic accidental fuera de la seleccion y no crea un area valida, se restaura la seleccion anterior.
- Si `Abrir capturas con candado` esta chuleado en bandeja, toda nueva captura inicia bloqueada.
- Si `Abrir capturas con candado` esta deschuleado, toda nueva captura inicia desbloqueada.
- Desbloqueado: clic derecho dentro de la seleccion copia y cierra.
- Bloqueado: clic derecho abre menu contextual de Zaetta y no cierra hasta que el usuario elija una accion.
- Bloqueado: la opcion `Copiar` del menu contextual copia y cierra.
- Bloqueado: la opcion `Guardar` abre el guardado sin cerrar automaticamente.
- Bloqueado: la opcion `Desbloquear candado` apaga el bloqueo y mantiene la captura abierta.
- Bloqueado: la opcion `Cancelar captura` cierra sin copiar.
- El menu contextual del candado debe usar colores propios (`BackColor`/`ForeColor`) para evitar texto negro sobre fondo oscuro.
- No desechar manualmente el `ContextMenuStrip` dentro del evento `Closed`; WinForms puede seguir consultando el objeto durante el cierre y lanzar `ObjectDisposedException`.
- Bloqueado o desbloqueado: boton Copiar copia y cierra.
- Bloqueado o desbloqueado: `Ctrl + C` copia y cierra.
- Bloqueado o desbloqueado: `Ctrl + L` alterna el candado sin cerrar.
- `Esc` y boton `X` siguen funcionando como cancelacion manual.

### Reseleccion de area

La reseleccion vive dentro de `CaptureOverlay` y permite cambiar de decision sin cerrar la captura. El objetivo es imitar el flujo de Lightshot: mientras el usuario no copie, guarde o cancele manualmente, puede seleccionar otra zona o incluso otra pantalla desde el mismo overlay.

Archivos relacionados:

- `Capture/CaptureOverlay.cs`: campos `reselecting` y `previousSelectionBeforeReselect`.
- `Capture/CaptureOverlay.Input.cs`: si ya hay seleccion y el usuario arrastra fuera del rectangulo, inicia reseleccion en lugar de cerrar.
- `Capture/CaptureOverlay.Interaction.cs`: metodo `BeginReselect(Point point)`, encargado de preparar el overlay para seleccionar de nuevo.
- `Capture/CaptureOverlay.Rendering.cs`: durante la reseleccion oculta anotaciones viejas para que no se pinten encima de la nueva area.

Comportamiento esperado:

- Con una seleccion activa, arrastrar fuera del rectangulo inicia una nueva seleccion.
- La nueva seleccion puede estar en cualquier monitor porque el overlay cubre el escritorio virtual completo.
- Si la nueva seleccion mide al menos `10 x 10`, reemplaza la seleccion anterior.
- Si la nueva seleccion es demasiado pequena, se restaura la seleccion anterior para proteger contra clics accidentales.
- Al confirmar una nueva seleccion valida, se limpian las anotaciones anteriores porque pertenecian al recorte previo.
- El usuario puede repetir este flujo todas las veces que quiera hasta ejecutar Copiar, Guardar, `Esc` o boton `X`.
- `Ctrl + C`, boton Copiar y clic derecho sin candado siguen cerrando despues de copiar.

### Texto estilo Lightshot

La herramienta de texto ya no depende de una caja blanca grande de WinForms. Cuando el usuario elige `T` y hace clic dentro de la seleccion, el overlay entra en modo de edicion directa y pinta el texto encima de la captura.

Archivos relacionados:

- `Capture/CaptureOverlay.cs`: campos `textEditing`, `activeTextPoint`, `activeTextBounds`, `movingActiveText`, `activeTextMoveOffset`, `activeTextSize` y `activeTextValue`.
- `Capture/CaptureOverlay.Text.cs`: metodos para iniciar, medir, mover, redimensionar visualmente y confirmar el texto activo.
- `Capture/CaptureOverlay.Input.cs`: eventos de mouse para detectar el borde del texto activo, cambiar el cursor a mover y arrastrar el texto sin usar la herramienta `Mover`.
- `Capture/CaptureOverlay.Keyboard.cs`: escritura directa, `Backspace`, `Enter` para confirmar y `Esc` para cancelar.
- `Capture/CaptureOverlay.Rendering.cs`: dibuja el texto activo, el cursor de escritura translucido y el borde de seleccion tipo recorte.

Comportamiento esperado:

- Con la herramienta `T`, un clic dentro de la captura abre una seleccion de texto fina, no una caja grande.
- El usuario escribe directamente sobre la captura.
- El borde del texto activo usa una mezcla parecida al borde del recorte: sombra oscura, linea clara y punteado, para que se vea fuerte pero siga siendo liviano.
- El cursor de escritura parpadea como una barra fina blanca/gris translucida dentro del rectangulo activo; no hereda el color elegido para el texto y no debe salirse de la seleccion de texto.
- Al acercar el mouse al borde del texto activo, el cursor cambia a `SizeAll`.
- Al arrastrar desde ese borde, se mueve el texto activo dentro de la seleccion.
- Al hacer clic fuera del texto activo, se confirma el texto actual y puede empezar otro texto si la herramienta `T` sigue activa.
- `Enter` confirma el texto.
- `Esc` cancela el texto activo sin agregarlo a la captura.
- `Ctrl + C`, `Ctrl + S`, `Ctrl + L` y clic derecho confirman primero el texto activo para no perder lo escrito.
- `Shift + rueda del mouse` mientras el texto esta activo aumenta o reduce el tamano antes de confirmar.
- Despues de confirmar, el texto queda como una anotacion normal y se puede mover con la herramienta `Mover`.

Detalle de codigo:

- `BeginTextEdit(Point location)` prepara el estado temporal del texto: limpia una edicion anterior, fija el punto inicial dentro de la seleccion, crea el rectangulo activo y enfoca el overlay para recibir teclado.
- `UpdateActiveTextBoundsForContent()` mide el texto con `Graphics.MeasureString` y ajusta el rectangulo para que crezca con el contenido sin salirse del area capturada.
- `StartActiveTextCaret()` activa un timer liviano para alternar la visibilidad del cursor de escritura.
- `HitTestActiveTextBorder(Point point)` detecta si el mouse esta cerca de la raya del rectangulo; esa zona activa el cursor de mover.
- `MoveActiveTextTo(Point requestedTopLeft)` usa `ClampBoundsTopLeft` para mover el rectangulo sin dejar que se salga de la captura.
- `CommitTextEdit()` convierte el texto temporal en un `DrawOp` de tipo `Tool.Text`.
- `CancelTextEdit()` borra el texto temporal si el usuario cancela con `Esc`.

`ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`

Contiene:

- Instalacion local.
- Copia del ejecutable final.
- Creacion de acceso directo.
- Registro de inicio automatico con Windows para el usuario actual.
- Lanzamiento automatico de Zaetta Capture al terminar la instalacion para dejarla activa en bandeja.
- Limpieza/reemplazo de versiones anteriores.
- Barra de progreso.
- Mensajes de exito o error.

### Version Python

`ZAETTA_CAPTURE/main.py`

Fue la primera version. Sirve como referencia funcional y visual, pero no debe ser la prioridad para rendimiento.

## 9. Comandos de compilacion

Desde:

```powershell
cd C:\Automatizaciones\zaettacapture_repo
```

Compilar aplicacion nativa:

```powershell
.\tools\build-native.ps1
```

En Linux con Mono instalado:

```bash
./tools/build-native.sh
```

Requisito Linux:

```bash
sudo apt-get install -y mono-devel
```

Compilar instalador final:

```powershell
.\tools\build-native.ps1
```

En Linux con Mono instalado:

```bash
./tools/build-native.sh
```

El build actual embebe en el instalador:

- `ZaettaApp`: ejecutable final de la aplicacion.
- `ZaettaLogo`: `ZAETTA_CAPTURE/logo_oficial.png`.

El instalador tambien registra la app en:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

Valor:

```text
Zaetta Capture = "%LOCALAPPDATA%\Zaetta Capture\Zaetta Capture.exe"
```

Tamanos recientes de referencia:

- `ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe`: ~172 KB.
- `INSTALADOR_ZAETTA_CAPTURE_FINAL.exe`: ~1.7 MB cuando incluye `ZaettaLogo`.
- `ZAETTA_CAPTURE/logo_oficial.png`: ~1.4 MB.

## 10. Como ejecutar en desarrollo

Version nativa:

```powershell
cd C:\Automatizaciones\zaettacapture_repo
& '.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe'
```

Version Python:

```powershell
cd C:\Automatizaciones\zaettacapture_repo\ZAETTA_CAPTURE
python -m pip install -r requirements.txt
python main.py
```

## 11. Repositorio

Repositorio GitHub:

```text
https://github.com/alexis-alzate/zaettacapture.git
```

Este repositorio debe contener codigo fuente, README y contexto. No debe versionar instaladores pesados, archivos temporales ni capturas personales.

## 12. Pendientes recomendados

Prioridad alta:

- Mejorar completamente el movimiento de elementos dibujados para que no se queden pegados.
- Permitir redimensionar flechas, rectangulos y textos con manejadores.
- Implementar configuracion persistente de atajos.
- Confirmar en varios equipos que `Print Screen` y `Supr` se puedan capturar de forma estable.
- Asegurar que copiar por boton, clic derecho y `Ctrl + C` compartan exactamente la misma logica.
- Validar que al copiar/cancelar no quede ningun overlay activo.

Prioridad media:

- Evaluar presets visuales para pixelado suave/medio/fuerte si el equipo los necesita.
- Agregar menu compacto con iconos y descripcion.
- Guardar ultima carpeta usada.
- Agregar historial local de ultimas capturas guardadas.
- Agregar opcion de abrir carpeta de evidencias.
- Agregar modo de color claro/oscuro para la barra.

Prioridad baja:

- Exportar a PDF.
- Subida opcional a SharePoint o OneDrive.
- Firma visual automatica en evidencias.
- Modo corporativo con marca de agua.

## 13. Decisiones tomadas

- Zaetta Capture debe ser independiente de PlayOps Suite.
- La version nativa es preferible por rendimiento.
- El instalador puede pesar mas que Lightshot porque incluye runtime/logica propia y no depende de un instalador externo comercial.
- El peso del instalador no es un problema grave si la app sera usada constantemente y evita instalar herramientas externas con alertas.
- La prioridad no es competir con todas las funciones de Lightshot, sino cubrir el flujo operativo seguro: capturar, marcar, copiar y cerrar rapido.

## 14. Criterio de calidad visual

La app debe verse:

- Minimalista.
- Compacta.
- Rapida.
- No corporativa pesada.
- Con barra de herramientas fina.
- Con botones limpios.
- Con iconos claros.
- Con selector parecido a Lightshot.
- Con color principal sobrio.

Evitar:

- Botones muy grandes.
- Bordes gruesos.
- Colores chillones por defecto.
- Barras invasivas.
- Ventanas innecesarias.
- Menus gigantes.

## 15. Nota para retomar trabajo

Cuando se vuelva a trabajar en esta app, empezar revisando:

1. `ZAETTA_CAPTURE_NATIVE/App/Program.cs`
2. `ZAETTA_CAPTURE_NATIVE/App/TrayContext.cs`
3. `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.cs`
4. `ZAETTA_CAPTURE_NATIVE/Editing/`
5. `ZAETTA_CAPTURE_NATIVE/UI/`
6. `ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`
7. Este archivo `CONTEXTO_ZAETTA_CAPTURE.md`
8. `README.md`

Si hay que corregir funcionalidad de captura, hacerlo primero en la version nativa.

Si hay que corregir el instalador, tocar solo `InstallerZaettaFinal.cs`.

Si hay que cambiar branding/icono, el logo fuente oficial esta en `ZAETTA_CAPTURE/logo_oficial.png` y el icono que se embebe en los ejecutables es `ZAETTA_CAPTURE/zaetta_icon.ico`. Para regenerar el ICO desde el PNG se puede usar `tools/make-ico.ps1`; el script recorta el canvas alrededor de la placa y la Z para que el icono se lea mejor en bandeja y accesos directos pequenos.

### Sitio web y dominio

Dominio comprado: `zaettasoftware.com`.

Proveedor del dominio: DreamHost.

Estado importante: por ahora se compro el dominio, no necesariamente un alojamiento web. El dominio es la direccion publica; el hosting es el servidor donde viven `index.html`, el instalador y `latest.json`.

Primera version del sitio:

- Carpeta local: `website/`.
- Pagina principal: `website/index.html`.
- Estilos: `website/styles.css`.
- Logo publico: `website/assets/logo_oficial.png`.
- Manifiesto de actualizaciones: `website/latest.json`.
- Boton de descarga: apunta al asset versionado en GitHub Releases.

URLs esperadas al publicar:

- `https://zaettasoftware.com/`
- `https://zaettasoftware.com/latest.json`
- `https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.1/ZaettaCaptureSetup.exe`

`latest.json` no actualiza la app por si solo. Es el contrato que lee `UpdateService` dentro de Zaetta Capture para saber version disponible, URL de descarga, hash y notas.

Opciones para alojar la pagina y los upgrades:

- Contratar hosting en DreamHost y subir la carpeta `website/` alli.
- Usar un hosting estatico externo como Cloudflare Pages, GitHub Pages, Netlify o Vercel y apuntar el DNS del dominio desde DreamHost.
- Usar almacenamiento publico para descargas y JSON, siempre que entregue URLs HTTPS estables.

Decision tomada: el dominio queda en DreamHost, la pagina y `latest.json` quedan en Vercel, y los instaladores versionados quedan en GitHub Releases.

### Arquitectura propuesta de upgrades

Decision propuesta:

- DreamHost: conserva el dominio `zaettasoftware.com` y administra DNS.
- Vercel: aloja la pagina publica y el archivo `latest.json`.
- GitHub Releases: aloja los instaladores `.exe` de cada version.
- Zaetta Capture: contiene la logica interna para consultar updates, pedir confirmacion, descargar, validar y ejecutar el instalador.

Flujo esperado:

```text
Zaetta Capture esta en bandeja
    ↓
consulta https://zaettasoftware.com/latest.json
    ↓
compara version remota contra AppInfo.Version
    ↓
si hay version nueva y no hay captura activa, muestra aviso
    ↓
usuario acepta
    ↓
descarga instalador desde GitHub Releases con barra de progreso
    ↓
valida SHA256
    ↓
ejecuta el instalador
    ↓
cierra la app vieja
    ↓
el instalador reemplaza archivos
    ↓
abre la nueva version en bandeja
```

Regla de experiencia:

- No interrumpir una captura activa.
- Si `captureActive == true`, posponer el aviso de update hasta que el overlay se cierre.
- El update debe mostrarse cuando el usuario no este en mitad de una captura.
- La descarga debe mostrar progreso visible.
- El instalador descargado debe validarse con SHA256 antes de ejecutarse.
- Si el hash no coincide, cancelar el update y mostrar alerta.

Contrato esperado para `latest.json`:

```json
{
  "version": "1.0.1",
  "downloadUrl": "https://github.com/USUARIO/REPO/releases/download/v1.0.1/ZaettaCaptureSetup.exe",
  "sha256": "...",
  "mandatory": false,
  "notes": [
    "Mejoras de texto estilo Lightshot",
    "Correcciones del instalador"
  ]
}
```

Componentes implementados en la app:

- `UpdateService.cs`: consulta `latest.json`, compara versiones, descarga instalador y valida hash.
- `UpdatePromptForm.cs`: mensaje para aceptar o posponer update.
- `UpdateProgressForm.cs`: ventana con barra de progreso durante descarga/validacion.
- Integracion en `TrayContext.cs`: check programado de updates y bloqueo si `captureActive` esta activo.

Detalle de codigo:

- `Updates/UpdateInfo.cs` es un objeto simple de datos. Guarda `Version`, `DownloadUrl`, `Sha256`, `FileSizeBytes` y `Notes`. Se separo para que los formularios no tengan que entender el JSON.
- `Updates/UpdateService.cs` contiene `ManifestUrl = "https://www.zaettasoftware.com/latest.json"`. Se usa `www` directo para evitar el `308 Permanent Redirect` que Vercel devuelve desde el dominio apex.
- `UpdateService.CheckForUpdate()` descarga el JSON, lo parsea, compara `AppInfo.Version` contra la version remota y devuelve `null` si no hay nada nuevo.
- `UpdateService.VerifySha256()` calcula el SHA256 del instalador descargado. Si no coincide con `latest.json`, no se ejecuta el archivo.
- `UpdatePromptForm.cs` es la ventana de decision. Existe para que la app no instale en silencio: el usuario ve version/notas y acepta o pospone.
- `UpdateProgressForm.cs` descarga con `WebClient.DownloadFileAsync`, actualiza la barra de progreso, valida hash y abre el instalador con `Process.Start`.
- `TrayContext.ScheduleUpdateChecks()` programa una revision inicial despues de abrir la app y luego revisiones cada 6 horas.
- `TrayContext.BeginUpdateCheck()` corre la consulta en `ThreadPool` para no congelar la bandeja ni la UI.
- `TrayContext.ShowPendingUpdateIfReady()` evita mostrar el aviso si hay una captura activa. Si el usuario esta capturando, guarda `pendingUpdate` y espera a que cierre el overlay.

Por que se tomo esta decision:

- El dominio propio da una URL estable y de marca para `latest.json`.
- Vercel entrega el JSON rapidamente y redeploya cada cambio que subamos a GitHub.
- GitHub Releases es mejor para `.exe` porque mantiene assets por version y evita depender de Vercel para binarios.
- SHA256 protege contra descargas corruptas o archivos que no correspondan al release esperado.
- El aviso no aparece durante una captura porque interrumpiria justo el flujo principal de la app.
- El updater debe consultar la URL final sin redirect siempre que sea posible. En .NET Framework, `WebClient` puede fallar con `308 Permanent Redirect`, por eso se usa `www.zaettasoftware.com/latest.json`.

Por que GitHub Releases para instaladores:

- Mantiene historial por version.
- Permite adjuntar el instalador como asset de cada release.
- Evita meter binarios pesados en Vercel.
- Deja Vercel enfocado en pagina, dominio y manifiesto.

Por que Vercel para pagina/manifest:

- Ya es el flujo conocido.
- Sirve estaticos facilmente.
- Permite conectar `zaettasoftware.com` por DNS desde DreamHost.
- `latest.json` queda en una URL propia de la marca.

### Despliegue inicial en Vercel y DNS DreamHost

Fecha: 2026-08-01.

Se valido que el repo `alexis-alzate/zaettacapture` ya estaba en GitHub y se creo un proyecto en Vercel usando:

```text
Repository: alexis-alzate/zaettacapture
Branch: main
Root Directory: website
Application Preset: Other
Build Command: vacio/default
Output Directory: default
```

Decision importante:

- Se uso `Root Directory: website` porque `index.html`, `styles.css`, `latest.json` y los assets publicos viven dentro de esa carpeta.
- Si se dejaba `Root Directory: ./`, Vercel iba a mirar la raiz del repo y podia no publicar la pagina correcta.

Dominio en Vercel:

- Se agrego `zaettasoftware.com`.
- Vercel tambien creo/configuro `www.zaettasoftware.com`.
- Se dejo chuleada la opcion recomendada de redirigir apex a `www`, por eso Vercel mostro `308` hacia `www.zaettasoftware.com`.

Registros DNS pedidos por Vercel y cargados en DreamHost:

```text
Type: A
Name/Host: @
Value/Apunta a: 216.198.79.1
TTL: predeterminado
```

```text
Type: CNAME
Name/Host: www
Value/Apunta a: b6f74b7a12af6643.vercel-dns-017.com
TTL: predeterminado
```

Pantalla correcta en DreamHost:

- Se mantuvieron los nameservers de DreamHost.
- No se cambio a "nameservers de otro host".
- Se entro a la seccion `DNS`.
- Se uso `Agregar Registro`.
- Los registros quedaron bajo `Registros Personalizados`.

Estado observado:

- DreamHost mostro "Enviando Registros" / "Actualizando DNS".
- Esto significa que los registros quedaron en proceso de aplicacion.
- Despues de terminar en DreamHost, se debe volver a Vercel y presionar `Refresh` en los dominios.

Validaciones esperadas:

```text
https://zaettasoftware.com/
https://www.zaettasoftware.com/
https://zaettasoftware.com/latest.json
https://zaettasoftware.com/downloads/ZaettaCaptureSetup.exe
```

Notas:

- La propagacion DNS puede tardar desde minutos hasta varias horas.
- Si Vercel sigue mostrando "Invalid Configuration" justo despues de guardar DNS, no necesariamente esta mal; puede ser propagacion.
- No se deben borrar los registros `NS` de DreamHost.
- Si existen otros registros `A` para `@` o `CNAME/A` para `www`, pueden entrar en conflicto y deben revisarse.

### Release oficial v1.0

Fecha: 2026-08-01.

Se creo el release oficial en GitHub:

```text
Release: v1.0
Titulo: Zaetta Capture v1.0
URL: https://github.com/alexis-alzate/zaettacapture/releases/tag/v1.0
Asset: ZaettaCaptureSetup.exe
Tamano: 1723904 bytes
SHA256: 8b571227d196cd58f90a04b9a34862602b908b670ed7e4566fe12ca25539a570
```

URL publica del instalador versionado:

```text
https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0/ZaettaCaptureSetup.exe
```

`website/latest.json` quedo apuntando a ese asset de GitHub Releases:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0/ZaettaCaptureSetup.exe",
  "sha256": "8b571227d196cd58f90a04b9a34862602b908b670ed7e4566fe12ca25539a570",
  "fileSizeBytes": 1723904
}
```

Validaciones realizadas:

- `gh release view v1.0` confirmo release publicado, no draft, no prerelease.
- El asset `ZaettaCaptureSetup.exe` aparece con estado `uploaded`.
- `curl -I -L` contra la URL del asset respondio `200` despues del redirect de GitHub.
- `https://zaettasoftware.com/latest.json` respondio con el `downloadUrl` de GitHub Releases.

Si la app no aparece en bandeja al ejecutarse, revisar primero:

1. La flecha de iconos ocultos de Windows.
2. `Pictures\Zaetta Capture\startup-error.log`.
3. Compatibilidad de metodos .NET Framework si el build fue generado con Mono.

### Release oficial v1.0.1

Fecha: 2026-08-01.

Objetivo: publicar la primera version con actualizador interno.

Cambios principales:

- `AppInfo.Version` subio a `1.0.1` para que la app pueda comparar contra `latest.json`.
- `InstallerZaettaFinal.Version` subio a `1.0.1` para que el instalador muestre e instale la misma version que la app.
- Se agregaron `Updates/UpdateInfo.cs` y `Updates/UpdateService.cs`.
- Se agregaron `App/UpdatePromptForm.cs` y `App/UpdateProgressForm.cs`.
- `TrayContext.cs` ahora revisa actualizaciones en segundo plano, agrega menu `Buscar actualizaciones` y pospone avisos si hay captura activa.
- `Paths.cs` ahora expone `UpdatesDir`, usado para descargar instaladores en `Pictures\Zaetta Capture\Updates`.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo web: website/downloads/ZaettaCaptureSetup.exe
Tamano: 1736704 bytes
SHA256: 60e2ee927623b7fb3d7a50a70a918394be77edff812b57bb73c7c2cd464858ae
```

### Release oficial v1.0.2

Fecha: 2026-08-01.

Motivo: corregir el error del updater al revisar actualizaciones.

Bug observado:

```text
No se pudo revisar actualizaciones.
The remote server returned an error: (308) Permanent Redirect.
```

### Release oficial v1.0.3

Fecha: 2026-08-01.

Motivo: blindar el updater para que no dependa del comportamiento automatico de redirects de `WebClient`.

Cambio tecnico:

- `UpdateService.CheckForUpdate()` dejo de usar `WebClient.DownloadString()` directamente.
- Se agrego `DownloadStringFollowingRedirects()`, que usa `HttpWebRequest` con `AllowAutoRedirect = false`.
- El codigo detecta manualmente `301`, `302`, `303`, `307` y `308`.
- Si recibe `Location`, construye la URL final con `BuildRedirectUrl()` y reintenta hasta 5 veces.
- Esto evita que un redirect `308 Permanent Redirect` rompa la revision de actualizaciones.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1738240 bytes
SHA256: b5a94e0b194631a71e3c9d9e1c7099018c8c21367cf2d1863c18389f3886fce2
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.3",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.3/ZaettaCaptureSetup.exe",
  "sha256": "b5a94e0b194631a71e3c9d9e1c7099018c8c21367cf2d1863c18389f3886fce2",
  "fileSizeBytes": 1738240
}
```

### Release oficial v1.0.4

Fecha: 2026-08-01.

Motivo: release de prueba para validar el flujo real del updater desde una instalacion `v1.0.3`.

No se cambio la logica principal respecto a `v1.0.3`; se subio la version para que `latest.json` sea mayor que la version instalada y asi la app muestre el prompt de actualizacion.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1738240 bytes
SHA256: 16bda5ad5a0ed0a9ea2efc269ebf1eff2fc21bdc4e94d4629050a1f239bbaa92
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.4",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.4/ZaettaCaptureSetup.exe",
  "sha256": "16bda5ad5a0ed0a9ea2efc269ebf1eff2fc21bdc4e94d4629050a1f239bbaa92",
  "fileSizeBytes": 1738240
}
```

### Release oficial v1.0.5

Fecha: 2026-08-01.

Motivo: hacer que el aviso de actualizacion aparezca automaticamente y sea mas visible, sin depender de que el usuario use `Buscar actualizaciones`.

Cambios:

- `TrayContext.ScheduleUpdateChecks()` ahora lanza un chequeo automatico apenas se inicializa la bandeja.
- El primer timer baja de 15 segundos a 5 segundos.
- Si se detecta update, `TrayContext.ShowPendingUpdateIfReady()` muestra un globo de bandeja con la version disponible.
- `UpdatePromptForm` ahora usa `TopMost = true`, `ShowInTaskbar = true`, `BringToFront()` y `Activate()` al mostrarse.
- `UpdateProgressForm` tambien usa `TopMost = true`, `ShowInTaskbar = true`, `BringToFront()` y `Activate()` para que la descarga no quede escondida.

Regla importante:

- Si hay una captura activa, el prompt no invade el overlay; queda pendiente hasta cerrar la captura.
- Si no hay captura activa, el aviso debe aparecer al frente automaticamente cuando el updater detecte una version mayor.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1738752 bytes
SHA256: 0ae78dc5515911d9d3ed642ff1027eebc5017d5afdd06e632f00ab8910a2f4e2
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.5",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.5/ZaettaCaptureSetup.exe",
  "sha256": "0ae78dc5515911d9d3ed642ff1027eebc5017d5afdd06e632f00ab8910a2f4e2",
  "fileSizeBytes": 1738752
}
```

### Release oficial v1.0.6

Fecha: 2026-08-01.

Motivo: corregir el flujo de upgrade para que el usuario no tenga que presionar `Instalar` y para reducir fallos por archivos bloqueados durante reemplazo.

Bug observado:

- El updater descargaba y abria el instalador, pero el instalador quedaba esperando click en `Instalar`.
- Durante el upgrade aparecio `La instalacion fallo` con un error de acceso a archivos dentro de `%LOCALAPPDATA%\\Zaetta Capture`.

Causa:

- El instalador no distinguia entre instalacion manual y upgrade iniciado desde la app.
- El updater abria el instalador y luego llamaba `Application.Exit()`, pero la app vieja podia seguir viva unos instantes mientras el instalador intentaba borrar la carpeta instalada.
- Si el `.exe` anterior seguia cargado o la carpeta estaba bloqueada, `Directory.Delete(installDir, true)` fallaba.

Solucion:

- `UpdateProgressForm` ahora abre el instalador con argumento `/upgrade`.
- Despues de abrir el instalador, el updater llama `Environment.Exit(0)` para cerrar el proceso viejo de forma inmediata.
- `InstallerForm` recibe `upgradeMode`.
- En `upgradeMode`, el boton muestra `Actualizando`, queda deshabilitado y el instalador llama `Install()` automaticamente en `Shown`.
- `DeleteDirectoryWithRetry()` reintenta borrar la carpeta instalada hasta 8 veces.
- Antes de cada intento se llama `StopRunningZaetta()`.
- `StopRunningZaetta()` ahora intenta `CloseMainWindow()`, espera, y si el proceso sigue vivo usa `Kill()` con espera mas larga.
- Si el upgrade termina bien, el instalador cierra solo despues de mostrar `Instalacion completada`.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1739776 bytes
SHA256: 0a3604cea108ff743fb69334069703495f2d61941445f533e73a07ab414454b7
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.6",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.6/ZaettaCaptureSetup.exe",
  "sha256": "0a3604cea108ff743fb69334069703495f2d61941445f533e73a07ab414454b7",
  "fileSizeBytes": 1739776
}
```

### Release oficial v1.0.7

Fecha: 2026-08-01.

Motivo: release de prueba para validar el upgrade automatico desde una instalacion local `v1.0.6`.

No se cambio la logica principal respecto a `v1.0.6`; se subio la version para que `latest.json` sea mayor que la version instalada y la app dispare el updater.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1739776 bytes
SHA256: c720052fafbcaec395713ab17a9926458f0fb8af72021109917f73e443805d1b
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.7",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.7/ZaettaCaptureSetup.exe",
  "sha256": "c720052fafbcaec395713ab17a9926458f0fb8af72021109917f73e443805d1b",
  "fileSizeBytes": 1739776
}
```

### Release oficial v1.0.8

Fecha: 2026-08-01.

Motivo: corregir el fallo persistente de acceso a `%LOCALAPPDATA%\\Zaetta Capture` durante el upgrade.

Bug observado:

- El instalador `v1.0.7` ya arrancaba solo, pero seguia fallando con `The process cannot access the file...`.
- El punto fragil era borrar toda la carpeta instalada con `Directory.Delete(installDir, true)`.

Solucion:

- En `Install()` ya no se borra toda la carpeta `installDir`.
- Se crea la carpeta si no existe con `Directory.CreateDirectory(installDir)`.
- `ExtractResourceWithRetry()` extrae la app embebida a un archivo temporal `.new`.
- `CopyFileWithRetry()` copia el instalador actual a un archivo temporal `.new`.
- `ReplaceFileWithRetry()` reemplaza cada archivo puntual con hasta 10 reintentos.
- `DeleteOrMoveOldFile()` intenta borrar el archivo viejo; si no puede, lo renombra con extension `.old`.
- Esta estrategia evita que un bloqueo de carpeta completa tumbe todo el upgrade.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1740800 bytes
SHA256: 51723302f3c2b091aa9cdd4530a2112545c0f9b137ff339af67214dd2631ef8f
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.8",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.8/ZaettaCaptureSetup.exe",
  "sha256": "51723302f3c2b091aa9cdd4530a2112545c0f9b137ff339af67214dd2631ef8f",
  "fileSizeBytes": 1740800
}
```

### Release oficial v1.0.9

Fecha: 2026-08-01.

Motivo: mitigacion por alerta de Bitdefender durante descarga del updater.

Observacion:

- Bitdefender puso en cuarentena el instalador descargado desde el updater.
- La ruta detectada era `Pictures\\Zaetta Capture\\Updates\\ZaettaCaptureSetup-...exe`.
- El chequeo de URL con Bitdefender marco el link de GitHub Releases como seguro.
- Por tanto, el problema apunta a heuristica sobre el ejecutable local: `.exe` unsigned, autoextraible, que reemplaza procesos/archivos.

Mitigacion aplicada:

- `Paths.UpdatesDir` ya no usa `Pictures\\Zaetta Capture\\Updates`.
- Ahora usa `%LOCALAPPDATA%\\Zaetta Capture\\Updates`.
- Esto evita guardar instaladores ejecutables en una carpeta de usuario pensada para documentos/imagenes.

Limitacion importante:

- Esta mitigacion no reemplaza la solucion profesional.
- Para distribucion real a clientes se necesita firmar el ejecutable/instalador con certificado de code signing o usar un instalador estandar firmado.
- Tambien conviene enviar el falso positivo a Bitdefender cuando el binario final este estable.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1740800 bytes
SHA256: 118c7626caff4f6faf5b7685da5451c2e87944817ae45de34e3d2c482e4e5f37
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.9",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.9/ZaettaCaptureSetup.exe",
  "sha256": "118c7626caff4f6faf5b7685da5451c2e87944817ae45de34e3d2c482e4e5f37",
  "fileSizeBytes": 1740800
}
```

### Release oficial v1.0.10

Fecha: 2026-08-01.

Motivo: reducir comportamiento agresivo detectado por Bitdefender Advanced Threat Defense.

Observacion:

- Bitdefender mostro bloqueos en `Advanced Threat Defense` para el instalador y la app.
- Ese modulo mira comportamiento, no solo firma/hash/URL.
- Acciones como matar procesos, ejecutar un `.exe` descargado y reemplazar binarios en AppData son sensibles si el binario no esta firmado.

Mitigacion aplicada:

- En modo `/upgrade`, el instalador ya no ejecuta `CleanupLegacyInstallations()`.
- En modo `/upgrade`, el instalador ya no llama `StopRunningZaetta()` con `Kill()`.
- Se agrego `WaitForRunningZaettaToExit()`, que solo espera a que la app vieja salga.
- `ExtractResourceWithRetry()` y `CopyFileWithRetry()` reciben `allowProcessKill`.
- En instalacion manual se mantiene el comportamiento de limpieza.
- En upgrade automatico se evita matar procesos y se limita el reemplazo a archivos puntuales.

Limitacion:

- Esto baja el riesgo heuristico, pero no garantiza que Bitdefender deje de bloquear un instalador unsigned.
- Para distribucion publica se necesita certificado de code signing y/o reporte de falso positivo al proveedor de antivirus.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1740800 bytes
SHA256: fb5d50871dc9de4fe427301d59a6c5f968215cf87a978363297ce6523a2d0c02
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.10",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.10/ZaettaCaptureSetup.exe",
  "sha256": "fb5d50871dc9de4fe427301d59a6c5f968215cf87a978363297ce6523a2d0c02",
  "fileSizeBytes": 1740800
}
```

### Release oficial v1.0.11

Fecha: 2026-08-01.

Motivo: restaurar el mensaje/globo de bandeja relacionado con actualizaciones.

Problema:

- El globo seguia existiendo en codigo, pero se mostraba justo antes de abrir el prompt `TopMost`.
- Windows podia ocultarlo o no dejarlo visible suficiente tiempo.

Cambio:

- `TrayContext.ShowPendingUpdateIfReady()` ahora configura `BalloonTipIcon = ToolTipIcon.Info`.
- El texto del globo indica que la actualizacion esta lista y que se abrira el asistente.
- `ShowBalloonTip()` sube a 10 segundos.
- Se ejecuta `Application.DoEvents()` y una espera corta de 1.8 segundos antes de abrir el prompt.
- Con esto el usuario vuelve a ver primero el mensaje de bandeja y luego la ventana de actualizacion.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1740800 bytes
SHA256: ed815efdf95f23d1b0cab7c85d51e70868584205a53154c3988fda9b5e6e3b95
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.11",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.11/ZaettaCaptureSetup.exe",
  "sha256": "ed815efdf95f23d1b0cab7c85d51e70868584205a53154c3988fda9b5e6e3b95",
  "fileSizeBytes": 1740800
}
```

### Release oficial v1.0.12

Fecha: 2026-08-01.

Motivo: reemplazar el globo clasico por una notificacion visual tipo Windows toast, similar a la tarjeta blanca que aparece encima de la bandeja.

Problema:

- `NotifyIcon.ShowBalloonTip()` depende del estilo y la configuracion de notificaciones de Windows.
- En algunos equipos puede verse como globo antiguo, ocultarse rapido o no parecerse a la notificacion blanca esperada.
- El usuario queria recuperar el aviso visual que sale cerca de la bandeja, antes de la ventana grande de actualizacion.

Cambio:

- Se agrego `App/UpdateToastForm.cs`.
- `UpdateToastForm` crea una ventana borderless, blanca, `TopMost`, sin aparecer en la barra de tareas.
- La tarjeta se posiciona en la esquina inferior derecha usando `Screen.PrimaryScreen.WorkingArea`.
- El lado izquierdo muestra una franja clara con el icono oficial extraido desde `Application.ExecutablePath`.
- El cuerpo muestra titulo, version detectada y el texto `Abriendo asistente...`.
- Tiene boton `x` para cerrarla manualmente.
- Usa dos timers: uno mantiene el aviso visible un momento y otro hace fade-out antes de cerrar.
- `TrayContext.ShowPendingUpdateIfReady()` ya no llama `ShowBalloonTip`; ahora llama `UpdateToastForm.ShowFor(info)` y despues abre `UpdatePromptForm`.

Decision tecnica:

- Se hizo un formulario propio para que el look no dependa de Windows ni de la configuracion del centro de notificaciones.
- El prompt invasivo se mantiene igual porque es el paso donde el usuario acepta la actualizacion y luego se descarga el instalador.
- El toast solo cumple el rol visual previo: avisar de manera elegante que la actualizacion fue detectada.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1742336 bytes
SHA256: 64c41905a46ddddfcdb85e44905352cf0539b31df997a5a0357137a9d771a6bf
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.12",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.12/ZaettaCaptureSetup.exe",
  "sha256": "64c41905a46ddddfcdb85e44905352cf0539b31df997a5a0357137a9d771a6bf",
  "fileSizeBytes": 1742336
}
```

### Release oficial v1.0.13

Fecha: 2026-08-01.

Motivo: el toast de `v1.0.12` existia, pero en prueba real el usuario no lo vio. Se reforzo para que sea mas facil de notar y para generar una version mayor que permita probar el updater desde una instalacion `1.0.12`.

Cambio:

- `UpdateToastForm` ahora permanece visible 5 segundos antes de hacer fade-out.
- Al mostrarse ejecuta `BringToFront()` y `Activate()`.
- La posicion usa `Screen.FromPoint(Cursor.Position).WorkingArea` en vez de `Screen.PrimaryScreen`, para aparecer en la pantalla donde el usuario esta trabajando.
- Se agrego un borde gris fino para que la tarjeta blanca se lea mejor sobre fondos claros.
- `AppInfo.Version` sube a `1.0.13`.
- `website/latest.json` apunta al release `v1.0.13`.

Decision tecnica:

- El problema no estaba en la deteccion del update, sino en que la notificacion podia pasar desapercibida.
- Hacerla durar mas y traerla al frente mejora la prueba visual sin cambiar el flujo de seguridad del updater.
- Se publico una version nueva porque el updater solo se dispara si `latest.json` tiene una version mayor que la instalada.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1742848 bytes
SHA256: 41adebd4ddcf02c2c8da3b009b2d405ed2abc6278a77dfd04601a964eb481da8
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.13",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.13/ZaettaCaptureSetup.exe",
  "sha256": "41adebd4ddcf02c2c8da3b009b2d405ed2abc6278a77dfd04601a964eb481da8",
  "fileSizeBytes": 1742848
}
```

### Release oficial v1.0.14

Fecha: 2026-08-01.

Motivo: corregir el bug donde abrir Zaetta Capture varias veces creaba varios iconos en la bandeja del sistema.

Problema:

- `Program.Main()` ejecutaba siempre `Application.Run(new TrayContext())`.
- Cada instancia nueva creaba su propio `NotifyIcon`.
- Windows mostraba varios iconos de Zaetta en la bandeja si el usuario abria el acceso directo varias veces.

Cambio:

- Se agrego `using System.Threading;` en `App/Program.cs`.
- Al inicio de `Main()` se crea un `Mutex` nombrado: `Local\\ZaettaCaptureNative`.
- Si el mutex ya existe, significa que Zaetta Capture ya esta corriendo y la nueva instancia sale inmediatamente con `return`.
- Si el mutex se crea por primera vez, la app continua y abre `TrayContext`.
- En `finally`, el mutex se libera y se destruye al salir.

Decision tecnica:

- `Mutex` es apropiado para una app WinForms de bandeja porque bloquea instancias duplicadas a nivel de sistema/usuario.
- Se uso prefijo `Local\\` para limitarlo a la sesion actual de Windows.
- No se muestra ventana de advertencia al abrir una segunda instancia; simplemente se evita crear otro icono.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1742848 bytes
SHA256: eb6c4415d22e7809e9ab0e61694a74f34fe814315114e009c63b9d247cb24204
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.14",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.14/ZaettaCaptureSetup.exe",
  "sha256": "eb6c4415d22e7809e9ab0e61694a74f34fe814315114e009c63b9d247cb24204",
  "fileSizeBytes": 1742848
}
```

### Release oficial v1.0.15

Fecha: 2026-08-01.

Motivo: corregir bug de numeracion en la herramienta `Numero`.

Problemas reportados:

- Al llegar a `9`, los numeros podian reiniciarse o repetirse.
- El usuario necesitaba poder seguir con `10`, `11`, `12`, etc.
- Al usar `Undo`, el siguiente numero podia quedar incorrecto.
- Al presionar `Esc` intentando descartar una accion, el conteo podia quedar reiniciado o trabado.

Cambio:

- Se agrego `Capture/CaptureOverlay.Numbering.cs`.
- Se agrego `GetNextNumberValue()` para calcular el siguiente numero leyendo todos los marcadores `Tool.Number` existentes en `ops`.
- Se agrego `RefreshNextNumberValue()` para actualizar `counterValue` desde el estado real del overlay.
- En `CaptureOverlay.Input.cs`, antes de crear un nuevo marcador, se llama `RefreshNextNumberValue()`.
- En `CaptureOverlay.Commands.cs`, cuando `Undo()` elimina un marcador numerado, se vuelve a recalcular el siguiente numero.
- Cuando una reseleccion valida limpia todas las anotaciones con `ops.Clear()`, el contador vuelve a `1` porque ya no quedan numeros en la captura actual.
- En `CaptureOverlay.Rendering.cs`, el circulo del marcador crece segun el texto medido para que `10`, `11`, `100` no queden apretados.

Decision tecnica:

- El contador ya no depende de incrementar/decrementar a ciegas.
- La fuente de verdad ahora es la lista `ops`, es decir, los numeros que realmente existen en pantalla.
- Esto hace que `Undo`, reseleccion y futuras operaciones no dejen el contador en un estado fantasma.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1743360 bytes
SHA256: f0cd4ecea583a698a05c740ebba88e3e38cfc23db04bfb1e5452152a8b7c1427
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.15",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.15/ZaettaCaptureSetup.exe",
  "sha256": "f0cd4ecea583a698a05c740ebba88e3e38cfc23db04bfb1e5452152a8b7c1427",
  "fileSizeBytes": 1743360
}
```

### Release oficial v1.0.16

Fecha: 2026-08-01.

Motivo: hacer que Zaetta Capture detecte actualizaciones sin que el usuario tenga que cerrar y abrir la app.

Objetivo de experiencia:

- El usuario solo debe presionar `Actualizar` una vez.
- Despues de eso, la app descarga, valida, cierra la version vieja, abre el instalador en modo `/upgrade` y levanta la nueva version.
- No debe aparecer un paso adicional de `Instalar` iniciado manualmente por el usuario.
- La app debe detectar releases nuevos mientras sigue viva en bandeja.

Cambio:

- `TrayContext.ScheduleUpdateChecks()` ya no deja el timer en 6 horas despues del primer chequeo.
- Al iniciar, la app revisa cada 30 segundos durante 20 ciclos.
- Eso equivale a 10 minutos de deteccion agresiva al abrir o al iniciar Windows.
- Despues de ese periodo, revisa cada 5 minutos.
- El boton `Buscar actualizaciones` sigue existiendo para forzar una revision manual.
- Si el usuario presiona `Mas tarde`, la version detectada queda en pausa por 30 minutos con `SnoozeUpdate()`.
- Si hay captura activa (`captureActive == true`), el aviso sigue esperando hasta que el overlay se cierre.
- `UpdatePromptForm` ahora explica mejor el flujo: presionar `Actualizar` una vez y dejar que Zaetta haga el resto.

Constantes agregadas en `TrayContext.cs`:

```c#
private const int StartupUpdateCheckIntervalMs = 30 * 1000;
private const int NormalUpdateCheckIntervalMs = 5 * 60 * 1000;
private const int StartupFastCheckCount = 20;
private const int UpdateSnoozeMinutes = 30;
```

Decision tecnica:

- Revisar cada 30 segundos para siempre seria innecesario.
- Revisar agresivo solo al inicio ayuda a detectar updates recientes sin castigar tanto red/manifest.
- Cada 5 minutos despues del arranque mantiene la app sensible a nuevos releases sin depender de reiniciar.
- `Mas tarde` necesita pausa para que la ventana no vuelva a molestar al usuario cada pocos minutos.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1743872 bytes
SHA256: e3e457940141b7f09b543d498788283f71e19ac7bef36f8defd6d3c553e5e0db
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.16",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.16/ZaettaCaptureSetup.exe",
  "sha256": "e3e457940141b7f09b543d498788283f71e19ac7bef36f8defd6d3c553e5e0db",
  "fileSizeBytes": 1743872
}
```

### Release oficial v1.0.17

Fecha: 2026-08-01.

Motivo: conservar el atajo de captura elegido despues de una actualizacion.

Problema:

- Antes, el atajo solo vivia en memoria dentro de `TrayContext`.
- Cuando el updater cerraba la app vieja y abria la nueva version, la app volvia a registrar `Impr Pant`.
- Si el usuario habia elegido otro shortcut, por ejemplo `Ctrl + Shift + S`, se perdia despues del upgrade.

Cambio:

- Se agrego `Storage/HotkeyPreference.cs`.
- `CapturePreferencesStore` ahora guarda tambien:
  - `hotkeyKey`
  - `hotkeyModifiers`
- `TrayContext.RegisterSavedHotkey()` lee el atajo guardado al arrancar.
- Si el atajo guardado se puede registrar, marca ese item en el menu.
- Si el atajo guardado esta ocupado por Windows u otra app, hace fallback silencioso a `Impr Pant`.
- `SetHotkey()` guarda el atajo cuando el usuario elige uno de los presets.
- `CaptureCustomHotkey()` guarda el atajo personalizado cuando el usuario lo define.
- El archivo `capture-preferences.txt` sigue siendo compatible con las preferencias anteriores.

Decision tecnica:

- Se reutilizo `capture-preferences.txt` para no crear otro archivo de configuracion.
- La preferencia vive en `Paths.BaseDir`, por eso sobrevive a reinstalaciones y upgrades en AppData.
- El fallback evita que la app arranque rota si el atajo guardado ya no esta disponible.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1745920 bytes
SHA256: 1cef16c9d92c8980663cf183bebdbd9d2288c8221795079591f80718ec993c11
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.17",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.17/ZaettaCaptureSetup.exe",
  "sha256": "1cef16c9d92c8980663cf183bebdbd9d2288c8221795079591f80718ec993c11",
  "fileSizeBytes": 1745920
}
```

### Release oficial v1.0.18

Fecha: 2026-08-01.

Motivo: el toast bonito de actualizacion se veia al usar `Buscar actualizaciones`, pero podia no verse cuando el update era detectado automaticamente.

Cambio:

- `TrayContext` ahora guarda si el update pendiente vino de deteccion automatica con `pendingUpdateAutomatic`.
- Si el update es automatico, antes de mostrar el toast ejecuta `Application.DoEvents()` y espera 1.2 segundos.
- `UpdateToastForm.ShowFor(info, automatic)` permite modo automatico.
- En modo automatico, el toast dura 7 segundos en vez de 5.
- El flujo manual sigue igual.

Decision tecnica:

- En detecciones automaticas, especialmente al inicio de la app, Windows puede estar cambiando foco o la bandeja puede estar terminando de inicializar.
- La pausa corta hace que el toast aparezca cuando la UI ya esta mas estable.
- Mantenerlo mas tiempo ayuda a que el usuario lo alcance a ver antes del prompt grande.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1745920 bytes
SHA256: f6ead3240bd5b8e84ff58f4939eb7ffcffca5b527fa4b1d2e70c8c9f2ecd2815
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.18",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.18/ZaettaCaptureSetup.exe",
  "sha256": "f6ead3240bd5b8e84ff58f4939eb7ffcffca5b527fa4b1d2e70c8c9f2ecd2815",
  "fileSizeBytes": 1745920
}
```

### Release oficial v1.0.19

Fecha: 2026-08-01.

Motivo: agregar un menu de configuracion tipo Lightshot desde la bandeja.

Cambio:

- Se agrego `App/SettingsForm.cs`.
- El menu de bandeja ahora incluye `Opciones...`.
- La ventana tiene pestañas:
  - `General`: `Mantener posicion del area seleccionada`, `Abrir capturas con candado` e informacion de inicio en bandeja.
  - `Atajo`: muestra el atajo actual, permite cambiarlo, y ofrece presets `Impr Pant`, `Ctrl Shift S`, `Ctrl Alt S`.
- Al guardar, se actualizan las preferencias existentes en `capture-preferences.txt`.
- Si se cambia el atajo, se registra inmediatamente y queda persistido para upgrades.
- `TrayContext` ahora conserva `currentHotkeyKey` y `currentHotkeyModifiers` para mostrar el estado real en opciones.

Decision tecnica:

- Se conectaron solo opciones que ya tienen comportamiento real en la app.
- No se agregaron toggles falsos para features no implementadas.
- Se mantiene compatibilidad con el menu rapido de bandeja: las opciones directas siguen disponibles.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1750016 bytes
SHA256: 794a854358b112af84f694f6a44d1ed7a05f743b883aad16d4b28f6b683e3358
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.19",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.19/ZaettaCaptureSetup.exe",
  "sha256": "794a854358b112af84f694f6a44d1ed7a05f743b883aad16d4b28f6b683e3358",
  "fileSizeBytes": 1750016
}
```

### Cierre de features del 2026-08-01

Fecha: 2026-08-01, hora Colombia (`America/Bogota`).

Features y correcciones completadas hoy:

- Dominio `zaettasoftware.com` conectado a Vercel con DNS desde DreamHost.
- Sitio publico de Zaetta Capture publicado en `https://www.zaettasoftware.com`.
- `latest.json` publico funcionando como manifest de actualizaciones.
- GitHub Releases funcionando como almacenamiento de instaladores versionados.
- Updater interno agregado: consulta manifest, compara version, descarga instalador, valida SHA256 y abre upgrade.
- Manejo de redirect `308` hacia `www.zaettasoftware.com`.
- Manejo manual de redirects `301/302/303/307/308`.
- Prompt automatico de update al frente.
- Instalador en modo `/upgrade` automatico, sin que el usuario tenga que presionar `Instalar`.
- Reemplazo de archivos mas robusto durante upgrade.
- Descargas movidas a `%LOCALAPPDATA%\\Zaetta Capture\\Updates`.
- Upgrade menos agresivo ante antivirus: no mata procesos ni limpia legacy durante update automatico.
- Toast propio tipo Windows para avisar update cerca de la bandeja.
- Toast mas visible: dura mas, se trae al frente y aparece en la pantalla activa.
- Instancia unica con `Mutex`: abrir varias veces ya no duplica iconos de bandeja.
- Numeracion corregida: permite `10`, `11`, `12` y recalcula contador despues de `Undo`.
- Deteccion mas agresiva de updates: cada 30 segundos al inicio y luego cada 5 minutos.
- Atajo de captura persistente: el shortcut elegido se conserva despues de actualizar.
- Ventana `Opciones...` desde bandeja con configuracion general y atajo de captura.

Apuntes de estudio creados/actualizados:

- Arquitectura Vercel + GitHub Releases + DreamHost.
- Updater interno y validacion SHA256.
- Redirects HTTP y por que afectan a .NET Framework.
- Modo `/upgrade` del instalador.
- Antivirus/Bitdefender y necesidad futura de firma digital.
- Toast propio en WinForms.
- `Mutex` para instancia unica.
- Contador robusto calculado desde `ops`.
- Scheduler de updates con checks rapidos, checks normales y snooze.
- Persistencia del hotkey con `hotkeyKey` y `hotkeyModifiers`.
- `SettingsForm` como primera ventana centralizada de configuracion.

Causa:

- `UpdateService` estaba consultando `https://zaettasoftware.com/latest.json`.
- Vercel redirige el dominio apex hacia `https://www.zaettasoftware.com/latest.json` con codigo `308`.
- `WebClient` de .NET Framework no manejo bien ese redirect permanente.

Solucion:

- Cambiar `ManifestUrl` a `https://www.zaettasoftware.com/latest.json`.
- Subir version interna a `1.0.2`.
- Publicar nuevo release `v1.0.2`.

Instalador generado:

```text
Archivo local: INSTALADOR_ZAETTA_CAPTURE_FINAL.exe
Archivo publico: ZaettaCaptureSetup.exe
Tamano: 1736704 bytes
SHA256: 7437e3d54407deea951fa22bc52fe85e09141f5023719f22b051d521fef40ed9
```

Manifest esperado:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.2",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.2/ZaettaCaptureSetup.exe",
  "sha256": "7437e3d54407deea951fa22bc52fe85e09141f5023719f22b051d521fef40ed9",
  "fileSizeBytes": 1736704
}
```

Manifest esperado para publicar:

```json
{
  "product": "Zaetta Capture",
  "version": "1.0.1",
  "releasedAt": "2026-08-01",
  "downloadUrl": "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.1/ZaettaCaptureSetup.exe",
  "sha256": "60e2ee927623b7fb3d7a50a70a918394be77edff812b57bb73c7c2cd464858ae",
  "fileSizeBytes": 1736704
}
```

## 16. Ultimos cambios registrados

### 2026-08-01

- Se implemento el actualizador interno de Zaetta Capture.
- Se agrego consulta a `https://zaettasoftware.com/latest.json`.
- Se agrego comparacion de version remota contra `AppInfo.Version`.
- Se agrego ventana para aceptar o posponer la actualizacion.
- Se agrego ventana de descarga con barra de progreso.
- Se agrego validacion SHA256 antes de ejecutar el instalador descargado.
- Se agrego carpeta local `Pictures\Zaetta Capture\Updates` para guardar instaladores temporales de upgrade.
- Se agrego menu de bandeja `Buscar actualizaciones`.
- Si hay una captura activa, el aviso de update queda pendiente hasta cerrar el overlay.
- Se subio la version interna a `1.0.1`.
- Se recompilo app e instalador con PowerShell/csc de Windows.
- Se actualizo `website/latest.json` para apuntar al release `v1.0.1`.
- Se corrigio el updater en `v1.0.2` para consultar `https://www.zaettasoftware.com/latest.json` y evitar el error `308 Permanent Redirect`.
- Se reforzo el updater en `v1.0.3` con manejo manual de redirects HTTP `301/302/303/307/308`.
- Se creo `v1.0.4` como release de prueba para validar el updater completo desde una instalacion anterior.
- Se creo `v1.0.5` para que la deteccion automatica muestre un prompt mas visible: chequeo inmediato, globo de bandeja y ventana al frente.
- Se creo `v1.0.6` para que el instalador arranque solo en modo `/upgrade` y reintente reemplazar archivos bloqueados.
- Se creo `v1.0.7` como release de prueba para validar el upgrade automatico desde una instalacion `v1.0.6`.
- Se creo `v1.0.8` para reemplazar archivos puntuales durante upgrade y no depender de borrar toda la carpeta instalada.
- Se creo `v1.0.9` para mover descargas del updater a `%LOCALAPPDATA%\\Zaetta Capture\\Updates` y documentar la alerta de Bitdefender.
- Se creo `v1.0.10` para que el upgrade automatico sea menos agresivo ante Bitdefender: no mata procesos ni limpia instalaciones legacy.
- Se creo `v1.0.11` para restaurar el globo de bandeja antes de abrir la ventana de actualizacion.
- Se creo `v1.0.12` para reemplazar el globo clasico por un toast propio tipo Windows cerca de la bandeja.
- Se creo `v1.0.13` para hacer mas visible el toast de actualizacion: 5 segundos, al frente y en la pantalla activa.
- Se creo `v1.0.14` para evitar multiples instancias y multiples iconos duplicados en la bandeja usando `Mutex`.
- Se creo `v1.0.15` para corregir la numeracion: continua despues de 9 y `Undo`/`Esc` no rompen el contador.
- Se creo `v1.0.16` para detectar actualizaciones de forma mas agresiva sin cerrar/abrir la app: checks cada 30 segundos al inicio y luego cada 5 minutos.
- Se creo `v1.0.17` para persistir el atajo de captura elegido y conservarlo despues de actualizar.
- Se creo `v1.0.18` para reforzar el toast bonito cuando el update se detecta automaticamente, no solo al usar `Buscar actualizaciones`.
- Se creo `v1.0.19` para agregar una ventana `Opciones...` desde la bandeja, estilo configuracion de app de captura.
- Se creo `v1.0.20` para pulir el frente de `Opciones...`: se retiro el `TabControl` blanco nativo, se agrego navegacion lateral oscura/dorada y se simplifico el menu de bandeja para dejar solo acciones importantes.
- Se preparo `v1.0.21` para ampliar el rango maximo del pixelador: `MaxIntensity = 70`, manteniendo `DefaultIntensity = 12` para que el pixelado no arranque exagerado.
- Se preparo `v1.0.22` para reforzar el updater ante bloqueos de red: ahora intenta varias URLs de manifest y muestra un diagnostico claro si Windows/Firewall/Bitdefender bloquea el socket.

#### v1.0.20 - Opciones mas limpias y menu de bandeja menos cargado

Fecha: 2026-08-01.

Archivos tocados:

- `ZAETTA_CAPTURE_NATIVE/App/SettingsForm.cs`: se reemplazo la ventana con pestanas nativas por una interfaz propia con panel lateral `General` / `Atajo`, panel oscuro central y botones dorados. Decision: el `TabControl` de WinForms metia una franja blanca que rompia la estetica negra/dorada de Zaetta.
- `ZAETTA_CAPTURE_NATIVE/App/TrayContext.cs`: el menu blanco de bandeja dejo de mostrar `Mantener posicion`, `Abrir capturas con candado` y el submenu completo de atajo. Decision: esas opciones ya viven en `Opciones...`; en la bandeja deben quedar acciones rapidas, no configuracion duplicada.
- `ZAETTA_CAPTURE_NATIVE/App/AppInfo.cs`: version interna subida a `1.0.20`.
- `website/latest.json`: manifest actualizado para que el updater detecte y descargue `v1.0.20`.

Comportamiento esperado:

- Click derecho en bandeja muestra solo acciones principales: capturar, repetir ultima area, opciones, historial, actualizaciones, acerca de y salir.
- `Opciones...` abre una ventana oscura, sin pestanas blancas, con navegacion lateral.
- En `General` quedan las preferencias persistentes de area y candado.
- En `Atajo` queda el shortcut actual, boton `Cambiar` y presets rapidos.
- Guardar aplica las preferencias y vuelve a registrar el atajo igual que antes.

#### v1.0.21 - Pixelador con maximo mas fuerte

Fecha y hora: 2026-08-01, 8:55 PM aprox. Colombia (`America/Bogota`).

Archivos tocados:

- `ZAETTA_CAPTURE_NATIVE/Editing/Pixelation.cs`: `MaxIntensity` queda en `70`; `DefaultIntensity` queda en `12`.
- `ZAETTA_CAPTURE_NATIVE/App/AppInfo.cs`: version interna subida a `1.0.21`.
- `website/latest.json`: manifest actualizado para que el updater descargue `v1.0.21`.
- `.gitignore`: se agregaron patrones para ignorar archivos temporales `.swp` y referencias visuales/Zone.Identifier.

Decision:

- No se subio `DefaultIntensity` porque el usuario confirmo que esta bien arrancar en `12`.
- Se subio el maximo para permitir un pixelado mas fuerte cuando sea necesario ocultar informacion sensible.
- Aprendizaje clave: `MaxIntensity` controla el techo; `DefaultIntensity` controla el arranque.

#### v1.0.22 - Updater con fallback y diagnostico de socket bloqueado

Fecha y hora: 2026-08-01, 9:09 PM aprox. Colombia (`America/Bogota`).

Problema observado:

- La URL `https://www.zaettasoftware.com/latest.json` abria correctamente en el navegador.
- Zaetta Capture mostraba `Unable to connect to the remote server`.
- El log `Pictures\\Zaetta Capture\\startup-error.log` mostro:
  - `SocketException`
  - `An attempt was made to access a socket in a way forbidden by its access permissions`
  - destino `216.198.79.1:443`

Interpretacion:

- El dominio y Vercel estaban funcionando.
- El bloqueo venia del entorno Windows para el proceso de Zaetta Capture: Firewall, Bitdefender, VPN o regla de red.

Archivos tocados:

- `ZAETTA_CAPTURE_NATIVE/Updates/UpdateService.cs`: `ManifestUrl` unico se reemplazo por `ManifestUrls`, con fallback en dominio principal, Vercel y GitHub Raw.
- `ZAETTA_CAPTURE_NATIVE/App/TrayContext.cs`: el error manual de update ahora detecta `SocketError.AccessDenied` y muestra un mensaje claro para permitir el `.exe` en firewall/antivirus.
- `ZAETTA_CAPTURE_NATIVE/App/AppInfo.cs`: version interna subida a `1.0.22`.
- `website/latest.json`: manifest actualizado para publicar `v1.0.22`.

Decision:

- Si Vercel o su IP quedan bloqueados, el updater puede intentar otra fuente del mismo manifest.
- Si Windows bloquea todas las conexiones del `.exe`, el mensaje ya no queda generico; explica que se debe revisar Firewall, Bitdefender, VPN o proteccion de amenazas.
- Como el updater instalado antes de `v1.0.22` no tiene fallback, puede ser necesario instalar manualmente `v1.0.22` una vez desde la web.

### 2026-07-28

- Se refactorizo la app nativa para salir del monolito original `ZaettaCapture.cs`.
- Se separaron responsabilidades en `App/`, `Capture/`, `Editing/`, `Storage/`, `SystemIntegration/`, `UI/` y `Legacy/`.
- `ZaettaCapture.cs` quedo como archivo historico minimo; el overlay real vive en `Capture/`.
- `CaptureOverlay` se dividio como `partial class` en archivos por responsabilidad: estado/base, teclado, mouse/input, toolbar, tools, rendering, interaction, adjustments, text y commands.
- Se agregaron servicios internos:
  - `AppInfo`.
  - `ScreenshotService`.
  - `ScreenshotCapture`.
  - `HistoryService`.
  - `LastSelectionStore`.
  - `CapturePreferencesStore`.
  - `StartupDiagnostics`.
- Se agrego build multiplataforma:
  - `tools/build-native.ps1` para Windows/.NET Framework.
  - `tools/build-native.sh` para Linux con Mono.
- Se compilo exitosamente con Mono 6.8 en Ubuntu.
- Se agrego fallback de icono de bandeja con `SystemIcons.Application` si falla `Icon.ExtractAssociatedIcon`.
- Se agrego manejo global de error de arranque en `Program.Main`; los errores se muestran y se guardan en `Pictures\Zaetta Capture\startup-error.log`.
- Se corrigio compatibilidad .NET de `String.Split` en `LastSelectionStore` usando `Split(new[] { ',' }, StringSplitOptions.None)`.
- Se implemento `Repetir ultima area`.
- Se implemento la opcion chuleable `Mantener posicion del area seleccionada`, inspirada en Lightshot.
- Se persistio la ultima area seleccionada en `Pictures\Zaetta Capture\last-selection.txt`.
- Se persistio la preferencia de mantener area en `Pictures\Zaetta Capture\capture-preferences.txt`.
- Se implemento movimiento de la seleccion completa con la herramienta `Mover`; las anotaciones se desplazan junto con el rectangulo.
- Se implemento boton de candado en el overlay para impedir que un clic izquierdo fuera de la seleccion cierre accidentalmente la captura.
- Se ajusto el clic derecho: sin candado copia y cierra rapido; con candado abre menu contextual de Zaetta para Copiar, Guardar, Desbloquear o Cancelar.
- Se corrigio el menu contextual del candado para que el texto sea visible y para evitar el error `ObjectDisposedException` de `ContextMenuStrip`.
- Se agrego la opcion de bandeja `Abrir capturas con candado`, persistida en `capture-preferences.txt` como `openLocked`.
- Se agrego el atajo `Ctrl + L` para activar/desactivar el candado desde el overlay.
- Se cambio `capture-preferences.txt` a formato de llaves (`keepLastSelectionPosition`, `openLocked`) manteniendo compatibilidad con el formato legacy de un solo valor.
- Se mantuvo el cierre normal al copiar con boton Copiar o `Ctrl + C`, incluso cuando el candado esta activo.
- Se agrego inicio automatico con Windows mediante registro `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- El inicio con Windows queda como comportamiento nativo: la opcion visible de bandeja fue retirada y la app asegura el registro al arrancar.
- El instalador ahora activa el inicio con Windows, lanza la app al finalizar para dejarla en bandeja y limpia entradas antiguas durante instalacion/desinstalacion.
- Se recompilo la app y el instalador.
- Se incrusto `ZAETTA_CAPTURE/logo_oficial.png` como recurso `ZaettaLogo` en el instalador.
- El instalador quedo nuevamente alrededor de 1.7 MB por incluir el logo oficial nitido.

### 2026-07-27

- Se agrego `CONTEXTO_ZAETTA_CAPTURE.md` para que cualquier agente o desarrollador pueda retomar el proyecto sin mezclarlo con PlayOps Suite.
- Se corrigio `README.md` para apuntar a este archivo de contexto.
- Se retiro el hook nativo de teclado para reducir falsos positivos de antivirus/Teams.
- `Impr Pant` queda gestionado por `RegisterHotKey`; si algun equipo intercepta esa tecla, se recomienda configurar un atajo alternativo desde la app.
- Se recompilo la aplicacion nativa y el instalador final.
- Se aumento el tamano de la cabeza de las flechas usando `AdjustableArrowCap`.
- Se cambio la captura para cubrir el escritorio virtual completo y permitir seleccionar cualquier monitor conectado.
- Se corrigio el bug de overlays/capturas infinitas que oscurecian progresivamente la pantalla al dispararse varias capturas seguidas.
- Se pulio el render de botones e iconos compactos con coordenadas `RectangleF`, `PixelOffsetMode.Half` y centrado real para evitar fondos descuadrados o bordes asimetricos.
- Se agrego ajuste rapido por hover + scroll: al pasar el mouse sobre una anotacion y mover la rueda, flechas/lineas/trazos cambian grosor, texto/numeros cambian tamano, rectangulos escalan su area y pixelado ajusta intensidad.
- Se separo el comportamiento de rueda: scroll normal agranda o achica el objeto bajo el cursor; `Ctrl + scroll` ajusta el grosor de flechas, lineas, marcos, lapiz y resaltador.
- Se agrego intensidad ajustable de pixelado: `+`/`-` cambian la intensidad cuando Pixelar esta activo y cada operacion guarda su propio nivel.
- Se agrego el logo oficial como fuente en `ZAETTA_CAPTURE/logo_oficial.png`, se regenero `ZAETTA_CAPTURE/zaetta_icon.ico` multi-tamano y se recompilaron app e instalador con ese icono.
- Se reemplazo la Z azul hardcodeada del instalador por el logo/icono oficial embebido.
- Se ajusto el encabezado del instalador para usar una Z dorada recortada desde el logo oficial y se cambio la paleta de boton/progreso de cyan a dorado.
- Se dejo una sola Z visual en el instalador: el icono oficial funciona como la Z del titulo y el texto queda como "aetta Capture", evitando una Z blanca redundante.
- Se movio el boton principal del instalador hacia adentro para que "Finalizar" no quede pegado/cortado contra el borde derecho.
- Se reintrodujo el glow/difuminado de la Z como fondo pintado directamente en el formulario y ubicado fuera del texto, para dar profundidad sin tapar titulo/subtitulo.
- Se acerco la Z/icono del titulo al texto, dibujandola por encima del label para que funcione mejor como la Z de "Zaetta", y se elimino el blur por pixel calculado al abrir el instalador para mejorar el tiempo de arranque.
- Se cambio la paleta visual de la app principal de cyan/azul a negro/dorado: acentos globales, Z de la barra, botones activos, hover y menus.
- Se implemento reseleccion de area dentro del overlay: arrastrar fuera del rectangulo actual permite seleccionar otra zona/pantalla sin cerrar la captura; una reseleccion invalida restaura el rectangulo anterior.
- Se cambio la herramienta de texto para editar directamente sobre la captura con previsualizacion y caret dibujados en el overlay, evitando la caja grande visualmente pesada.
- Se corrigio `Ctrl + Z` para que al deshacer una anotacion tambien se limpien seleccion y estados de mover/redimensionar, evitando contornos fantasma.
- Se subieron estos cambios a GitHub.

Commits relevantes:

- `afc10cf` - Agregar contexto de trabajo de Zaetta Capture.
- `74dac16` - Corregir captura con Impr Pant en pantallas pequenas.
- `39806c4` - Agrandar cabeza de flechas.
