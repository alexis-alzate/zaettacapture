# Contexto de Trabajo - Zaetta Capture

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
- Instalador publico: `website/downloads/ZaettaCaptureSetup.exe`.
- Manifiesto para futuras actualizaciones: `website/latest.json`.

URLs esperadas al publicar:

- `https://zaettasoftware.com/`
- `https://zaettasoftware.com/downloads/ZaettaCaptureSetup.exe`
- `https://zaettasoftware.com/latest.json`

`latest.json` no actualiza la app por si solo. Es el contrato que podra leer una futura funcion interna de Zaetta Capture para saber version disponible, URL de descarga, hash y notas.

Opciones para alojar la pagina y los upgrades:

- Contratar hosting en DreamHost y subir la carpeta `website/` alli.
- Usar un hosting estatico externo como Cloudflare Pages, GitHub Pages, Netlify o Vercel y apuntar el DNS del dominio desde DreamHost.
- Usar almacenamiento publico para descargas y JSON, siempre que entregue URLs HTTPS estables.

Decision pendiente: elegir donde se alojaran realmente los archivos publicos. Sin hosting, el dominio todavia no puede servir la pagina ni mandar upgrades.

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

Componentes futuros en la app:

- `UpdateService.cs`: consulta `latest.json`, compara versiones, descarga instalador y valida hash.
- `UpdatePromptForm.cs`: mensaje para aceptar o posponer update.
- `UpdateProgressForm.cs`: ventana con barra de progreso durante descarga/validacion.
- Integracion en `TrayContext.cs`: check programado de updates y bloqueo si `captureActive` esta activo.

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

- Se uso `Root Directory: website` porque `index.html`, `styles.css`, `latest.json` y `downloads/` viven dentro de esa carpeta.
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

## 16. Ultimos cambios registrados

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
