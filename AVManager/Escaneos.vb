Imports System.Globalization
Imports System.ComponentModel
Imports Parse
Imports System.Deployment.Application
Imports System.IO
Imports System.Threading

Public Class Escaneos

    Dim ListaEscaneos As List(Of Escaneo)
    Dim ListaDiscos As List(Of ParseObject)
    Dim Activos As Integer
    Dim Errores As Integer
    Dim Validos As Integer
    Dim Manuales As Integer
    Dim Equipos As List(Of String)
    Dim Cancelado As Boolean
    Dim Pausado As Boolean

    ' Antivirus
    Dim EscaneosActivos As Integer
    Dim MaximoEscaneosActivos As Integer
    Dim MaximoEscaneosTotales As Integer

    ' Exit Codes
    Const codeSuccess As Integer = 0        ' Operation has been executed successfully
    Const codeParameter As Integer = 1      ' Incorrect parameter given
    Const codeInterrupted As Integer = 2    ' Unknown error ' -10 signed
    'Const codeError As UInteger = 3        ' Task finished with error
    Const codeError As Integer = -10        ' Task finished with error
    Const codeCanceled As Integer = 4       ' Task execution has been canceled

    ' Notification Icon
    Public WithEvents Notify As NotifyIcon = New NotifyIcon()
    Public WithEvents mnuShow As ToolStripMenuItem
    Public mnuSep1 As ToolStripSeparator
    Public WithEvents mnuPausar As ToolStripMenuItem
    Public WithEvents mnuReanudar As ToolStripMenuItem
    Public WithEvents mnuDetener As ToolStripMenuItem
    Public WithEvents mnuExit As ToolStripMenuItem
    Public MainMenu As ContextMenuStrip
    Public WasActive As Form

    ' Timers
    Public WithEvents timerDiario As System.Windows.Forms.Timer = New System.Windows.Forms.Timer    '   Timer que se encarga de iniciar los escaneos diario
    Public WithEvents timerUpdates As System.Windows.Forms.Timer = New System.Windows.Forms.Timer   '   Timer que se encarga de modificar los datos que el UI utilizará
    Public WithEvents timerFrozen As System.Windows.Forms.Timer = New System.Windows.Forms.Timer    '   Timer que se encarga de matar escaneos que no respondan
    Public WithEvents timerDBUpdate As System.Windows.Forms.Timer = New System.Windows.Forms.Timer    '   Timer que se encarga de matar escaneos que no respondan

    '   Configuración Inicial
    Private Sub Escaneos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Application.OpenForms.Item(0).MinimizeBox = True
        Application.OpenForms.Item(0).MaximizeBox = False
        If ApplicationDeployment.IsNetworkDeployed Then
            lblVersion.Text = "v" & ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        Else
            lblVersion.Text = "Debug"
        End If

        ' Icono de Notificación
        Notify.Text = "Sistemas Juguel"
        Notify.Icon = My.Resources.ResourceManager.GetObject("Juguel_Icono")
        mnuShow = New ToolStripMenuItem("Show application")
        mnuSep1 = New ToolStripSeparator()
        mnuPausar = New ToolStripMenuItem("Pausar Scanner")
        mnuReanudar = New ToolStripMenuItem("Reanudar Scanner")
        mnuDetener = New ToolStripMenuItem("Detener Scanner")
        mnuExit = New ToolStripMenuItem("Exit application")
        MainMenu = New ContextMenuStrip
        MainMenu.Items.AddRange(New ToolStripItem() {mnuShow, mnuSep1, mnuPausar, mnuReanudar, mnuDetener, mnuSep1, mnuExit})
        Notify.ContextMenuStrip = MainMenu
        Notify.ContextMenuStrip.Enabled = True
        mnuReanudar.Visible = False
        mnuPausar.Visible = False
        mnuDetener.Visible = False
        'Notify.Visible = False
        Notify.Visible = True

        ListaDiscos = New List(Of ParseObject)
        ListaEscaneos = New List(Of Escaneo)
        Manuales = 0

        ' Llenamos la lista de equipos/discos desde la base de dato (async)
        LlenaEquipos()


        ' Auto-Start Escaneos
        timerDiario.Interval = CInt(TimeSpan.FromSeconds(30).TotalMilliseconds) '.FromDays(1).TotalMilliseconds)
        timerDiario.Enabled = True

        ' Update Scan Info
        timerUpdates.Interval = 200
        timerUpdates.Enabled = True

        ' Kill Frozen Scans
        timerFrozen.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds
        'timerFrozen.Enabled = True

        ' Update Scan Info
        timerDBUpdate.Interval = CInt(TimeSpan.FromMinutes(30).TotalMilliseconds) '.FromDays(1).TotalMilliseconds)
        timerDBUpdate.Enabled = True

        ' Settings de AV
        Escaneo.FileThreshold = 10000

        ' TaskBar Progress Report
        'Dim a As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail = New Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail(Me.Handle, Me)
        'a.Title = "Prueba"
        'a.Tooltip = "Prueba2"

        'Dim taskBar As Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance()
        'taskBar.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal)
        'taskBar.SetProgressValue(0.45, 1)

    End Sub

    '   Hace un query a la base de datos de todos los equipos y registra sus discos y rutas de acceso remoto en dtgEquipos
    Private Async Function LlenaEquipos() As Task(Of Boolean)
        Try ' Hacemos un try para evitar que cualquier error genere un crash, regresamos un booleano para saber si se actualizó correctamente
            dtgEquipos.Rows.Clear()     ' Borramos la lista de equipos ya registrados
            Dim query = ParseObject.GetQuery("Discos").WhereEqualTo("Escanear", True).Include("Equipo").Limit(1000)
            ListaDiscos.Clear()
            ListaDiscos = (Await query.FindAsync()).ToList
            If ListaDiscos.Count >= 1 Then
                ' Sort by date then by name
                ListaDiscos = ListaDiscos.OrderBy(Function(indice) indice.Get(Of Date)("UltimoEscaneo").ToLocalTime) _
                                .ThenBy(Function(indice) indice.Get(Of String)("Nombre")).ToList()
                Dim datosFilas As List(Of List(Of Object)) = New List(Of List(Of Object))
                For i = 0 To ListaDiscos.Count - 1
                    Dim disco = ListaDiscos.Item(i)
                    Dim equipo = disco.Get(Of ParseObject)("Equipo")
                    datosFilas.Add(New List(Of Object))
                    If (equipo.ContainsKey("Numero")) Then  ' Numero (de equipo) no es un atributo obligatorio para todos los equipos, pues se llena manualmente.
                        datosFilas(i).Add(equipo.Get(Of Integer)("Numero"))
                    Else
                        datosFilas(i).Add(-1)
                    End If
                    datosFilas(i).Add("\\" + equipo.Get(Of String)("NombreDeRed") + "\" + disco.Get(Of String)("Nombre"))
                    datosFilas(i).Add(Date.MinValue.Date)
                    If (disco.ContainsKey("UltimoEscaneo")) Then   ' Puede haber equipos que nunca se han escaneado y por lo tanto no es obligatorio
                        datosFilas(i)(2) = disco.Get(Of Date)("UltimoEscaneo").ToLocalTime
                    End If

                Next

                ' Escribimos en dtgEquipos
                For i = 0 To datosFilas.Count - 1
                    dtgEquipos.Rows.Add()   ' Agregamos una nueva fila para este equipo
                    If (datosFilas(i)(0) > -1) Then
                        dtgEquipos.Item("Numero", i).Value = datosFilas(i)(0).ToString
                    End If
                    dtgEquipos.Item("Disks", i).Value = datosFilas(i)(1).ToString
                    If (datosFilas(i)(2).Date <> Date.MinValue.Date) Then
                        dtgEquipos.Item("UltimoEscaneo", i).Value = Date.Parse(datosFilas(i)(2)).ToString("dd MMMM yyyy")
                    End If
                Next
            End If
            lblEquipos.Text = "# de Equipos: " & dtgEquipos.RowCount
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            Return False
        End Try

    End Function



    ' ------------------------------------------------    ESCANEOS    ------------------------------------------------

    '   Timer que cada 100 ms actualiza el UI con los datos recibidos de los async threads de cada escaneo
    Private Sub timerUpdates_Tick(sender As Object, e As EventArgs) Handles timerUpdates.Tick
        timerUpdates.Stop()
        Dim EscaneoActual As Escaneo
        For i = 0 To ListaEscaneos.Count - 1
            EscaneoActual = ListaEscaneos(i)
            Try
                If (Not EscaneoActual.Terminado And EscaneoActual.Comenzado) Then
                    ' Poner terminado y terminando en el if para validar que un no se haya comenzado la terminacion o terminado la misma del escaneo
                    If (Not EscaneoActual.Proceso.HasExited) Then
                        dtgEscaneos.Item("EscaneoProgreso", i).Value = EscaneoActual.Progreso ' Actualiza el valor en el ProgressBar correspondiente
                        dtgEscaneos.Item("Archivo", i).Value = EscaneoActual.ArchivosEscaneados
                    Else
                        EscaneoActual.Terminar()
                        If (EscaneoActual.Progreso >= 99 And EscaneoActual.FileCount >= Escaneo.FileThreshold) Then     ' Solamente los equipos que tuvieron un % de escaneo de 99+ se considerarán como válidos ' EscaneoActual.Proceso.ExitCode = codeSuccess Or 
                            RegistraExitoUI(i)
                            ' Actualizamos la fecha de ultimo escaneo en la lista de equipos
                            For c = 0 To dtgEquipos.RowCount - 1
                                If (dtgEscaneos.Item(0, i).Value = dtgEquipos.Item(1, c).Value) Then
                                    dtgEquipos.Item(2, c).Value = Date.Now.ToLocalTime.ToString("dd MMMM yyyy")
                                    Exit For
                                End If
                            Next
                        Else
                            RegistraErrorUI(i)
                        End If
                    End If
                End If
            Catch ex As Exception
                MsgBox("Error al actualizar UI. Escaneo " & EscaneoActual.Path & ": " & ex.Message)
            End Try
            Application.DoEvents()
        Next
        timerUpdates.Start()
    End Sub

    '   Registra Exito en UI
    Private Function RegistraExitoUI(i As Integer)
        dtgEscaneos.Item("EscaneoProgreso", i).Value = "Escaneo exitoso."
        dtgEscaneos.Item(1, i).Style.BackColor = Color.DarkGreen
        dtgEscaneos.Item(1, i).Style.SelectionBackColor = Color.DarkGreen
        Debug.WriteLine("Escaneo " & dtgEscaneos.Item(0, i).Value & " terminó exitosamente.")
        lblValidos.Text = "Válidos: " & Validos + 1

        '   Evitamos disminuir numthreads cuando fue un escaneo manual
        If (dtgEscaneos.Item(0, i).Style.BackColor.Equals(dtgEquipos.RowsDefaultCellStyle.BackColor)) Then
            EscaneosActivos -= 1
        Else
            Manuales -= 1
        End If
        Validos += 1
    End Function

    '   Registra Error en UI
    Private Function RegistraErrorUI(i As Integer)
        Try
            ListaEscaneos(i).Proceso.Refresh()
            If (ListaEscaneos(i).Terminado) Then
                Dim exitCode As Integer = ListaEscaneos(i).Proceso.ExitCode
                Debug.WriteLine("Escaneo " & dtgEscaneos.Item(0, i).Value & ": Exit code " & exitCode)
                Select Case exitCode
                    Case codeParameter
                        dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Parámetros incorrectos al iniciar escaneo."
                    Case codeInterrupted
                        dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Escaneo interrumpido. (" & ListaEscaneos(i).Progreso & "%)"
                    Case codeCanceled
                        dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Escaneo cancelado. (" & ListaEscaneos(i).Progreso & "%)"
                    Case codeError
                        dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: No hay conexión con el equipo."
                    Case Else
                        If (ListaEscaneos(i).ArchivosEscaneados = 1) Then
                            dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Disco compartido sin permisos."
                        Else
                            dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Escaneo interrumpido. (" & ListaEscaneos(i).Progreso & "%)"
                        End If
                End Select
            ElseIf (ListaEscaneos(i).Frozen) Then
                dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Escaneo cancelado (congelado). (" & ListaEscaneos(i).Progreso & "%)"
            Else
                ' No se reconoce la situación, se espera a que termine el proceso para conocer la situacion
                Exit Function
            End If
            dtgEscaneos.Item(1, i).Style.BackColor = Color.DarkRed ' Cuando se detecta un error, se pone un fondo rojo al ProgressBar
            dtgEscaneos.Item(1, i).Style.SelectionBackColor = Color.DarkRed
        Catch ex As Exception
            dtgEscaneos.Item("EscaneoProgreso", i).Value = "Error: Escaneo interrumpido. (" & ListaEscaneos(i).Progreso & "%)"
        End Try
        Debug.WriteLine("Error en escaneo " & dtgEscaneos.Item(0, i).Value & ".")
        Errores += 1
        lblErrores.Text = "Errores: " & Errores
        '   Evitamos disminuir numthreads cuando fue un escaneo manual
        If (dtgEscaneos.Item(0, i).Style.BackColor.Equals(dtgEquipos.RowsDefaultCellStyle.BackColor)) Then
            EscaneosActivos -= 1
        Else
            Manuales -= 1
        End If
    End Function

    '   Trae a todos los equipos pendientes de escaneo y comienza el escaneo de 'MaxEscaneos' equipos en grupos de 'MaxThreads'
    Private Sub btnEmpiezaEscaneos_Click(sender As Object, e As EventArgs) Handles btnEmpiezaEscaneos.Click
        btnEmpiezaEscaneos.Enabled = False  ' No se debe de iniciar el proceso de escaneos varias veces simultaneamente
        mnuDetener.Visible = True
        mnuPausar.Visible = True
        mnuReanudar.Visible = False

        ' Valida que no haya escaneos en progreso
        Dim bProgreso As Boolean = False
        For i = 0 To ListaEscaneos.Count - 1
            If (Not ListaEscaneos(i).Terminado) Then
                bProgreso = True
                Exit For
            End If
        Next

        If (Not bProgreso) Then
            Dim temp As Task = LlenaEquipos()   ' Para evitar hacer el Sub Async, llamamos LlenaEquipos de manera async
            Do While Not temp.Status = temp.Status.RanToCompletion  ' Pero luego detenemos el programa hasta que haya terminado LlenaEquipos
                Thread.Sleep(50)  ' Para reducir la carga de este ciclo en el CPU, agregamos un delay
                Application.DoEvents()      ' Para evitar que el UI se congele despues de dormir el thread principal
            Loop
            dtgEscaneos.Rows.Clear()        ' Reiniciamos la lista de los escaneos actuales
            ListaEscaneos.Clear()
        End If

        Dim tmp As Date = Date.MinValue
        For i = 0 To dtgEquipos.Rows.Count - 1
            Date.TryParse(dtgEquipos.Item(2, i).Value, tmp)
            If (tmp <= Date.Now.AddDays(-7)) Then
                Dim nombreDisco As String = dtgEquipos.Item("Disks", i).Value.ToString.Substring(0, dtgEquipos.Item("Disks", i).Value.ToString.Length - 1)
                Dim oEscaneo As Escaneo = New Escaneo(dtgEscaneos.RowCount, ListaDiscos(i), False)
                If (Not ListaEscaneos.Contains(oEscaneo)) Then
                    ListaEscaneos.Add(oEscaneo)
                End If
            End If
        Next

        MaximoEscaneosActivos = 5   ' Máximo de escaneos paralelos que ocurren simultaneamente
        EscaneosActivos = 0         ' Número actual de escaneos paralelos activos
        Dim DiasActivos As Integer = 6
        MaximoEscaneosTotales = dtgEquipos.RowCount / If(DiasActivos - 1 > 0, DiasActivos - 1, 1) '* 12    ' Máximo de escaneos válidos por día
        Validos = 0 ' Número actual de escaneos válidos
        Errores = 0

        If (MaximoEscaneosTotales > ListaEscaneos.Count) Then    ' El máximo de equipos que podemos escanear es <= al número de equipos que tenemos.
            MaximoEscaneosTotales = ListaEscaneos.Count
        End If

        Dim HoraLimite As Integer
        If (Date.Now.DayOfWeek <= DiasActivos And Date.Now.DayOfWeek > DayOfWeek.Sunday) Then
            HoraLimite = 17  ' Hora a la que ya no se iniciarán mas escaneos (24h)
            If (Date.Now.DayOfWeek = DayOfWeek.Saturday) Then
                HoraLimite = 13             ' 1 PM en Sábados
            End If
        Else
            HoraLimite = 0
        End If

        ' Hay un error al inciar los escaneos diarios cuando ya hay como 5  escaneos manuales iniciados. El numero de escaneos automaticos baja a 2 en vez de 3.

        HoraLimite = 19 ' debug

        timerFrozen.Enabled = True
        Do While (Validos < MaximoEscaneosTotales And Date.Now.Hour < HoraLimite) ' Mientras que no se cumplan los escaneos y aún no sean las 5 PM.
            For index = 0 To ListaEscaneos.Count - 1  ' Recorremos la lista de equipos pendientes de escaneo
                If Cancelado Then
                    Pausado = False
                    Cancelado = False
                    mnuDetener.Visible = False
                    mnuPausar.Visible = False
                    mnuReanudar.Visible = False
                    btnEmpiezaEscaneos.Enabled = True
                    Exit Do
                End If
                Do While Pausado
                    Thread.Sleep(100)
                    Application.DoEvents()
                Loop
                If Validos >= MaximoEscaneosTotales Then  ' Una vez que se hayan escaneado maxEscaneos equipos, terminamos de escanear y salimos del ciclo
                    Exit Do
                End If
                If Validos + EscaneosActivos < MaximoEscaneosTotales Then  ' Checamos si es necesario iniciar otro escaneo
                    If ListaEscaneos(index).Disco.Nombre <> "" Then

                        Dim nombre = ListaEscaneos(index).Path

                        '   Validamos que no haya un escaneo de este equipo en progreso
                        For i = 0 To dtgEscaneos.RowCount - 1
                            If (nombre = dtgEscaneos.Item(0, i).Value) Then
                                If (ListaEscaneos(i).Terminado) Then '   Si ya terminó el escaneo, checamos si fue valido o error
                                    If (ListaEscaneos(i).Progreso = 100) Then
                                        Continue For
                                        'Validos -= 1
                                    Else
                                        Errores -= 1
                                    End If
                                Else
                                    Continue For    '   Si no ha terminado el escaneo, nos lo brincamos
                                End If
                            End If
                        Next

                        Dim row As DataGridViewRow
                        If dtgEscaneos.RowCount = index Then
                            lblEscaneos.Text = "Total Escaneos: " & Validos + Errores + EscaneosActivos + 1 + Manuales
                            dtgEscaneos.Rows.Add()  ' Generamos una nueva fila en la lista de escaneos para registrar uno nuevos
                            row = dtgEscaneos.Rows.Item(dtgEscaneos.Rows.Count - 1)
                            row.Cells.Item(0).Value = nombre    ' Ingresamos la direccion que se esta escaneando en la columna 0
                            row.Cells.Item(1).Value = 0         ' Ingresamos un 0 en la columna 1 para indicar un 0% de escaneo completado
                            ListaEscaneos(index).Comenzar()     ' Inicamos el escaneo por primera vez
                            Debug.WriteLine("Escaneo " & ListaEscaneos(index).Path & ": Se inició por primera vez")
                        Else
                            lblEscaneos.Text = "Total Escaneos: " & Validos + Errores + EscaneosActivos + Manuales
                            Try
                                If (Not ListaEscaneos(index).Proceso.HasExited) Then
                                    Debug.WriteLine("Escaneo " & ListaEscaneos(index).Path & ": Se encuentra en proceso")
                                    Continue For
                                Else
                                    If (ListaEscaneos(index).Progreso = 100) Then ' Checamos que si el escaneo actual ya concluyó, para evitar volver a iniciarlo
                                        Debug.WriteLine("Escaneo " & ListaEscaneos(index).Path & ": Ya se completó exitosamente")
                                        Continue For
                                    ElseIf (dtgEscaneos.Item(1, index).Value.ToString.Contains("Error:")) Then
                                        row = dtgEscaneos.Rows.Item(index)
                                        row.Cells.Item(1).Value = 0
                                        row.Cells.Item(1).Style.BackColor = Color.White
                                        row.Cells.Item(2).Value = 0
                                        dtgEscaneos.Item(0, index).Style.BackColor = Color.WhiteSmoke
                                        dtgEscaneos.Item(0, index).Style.SelectionBackColor = Color.WhiteSmoke
                                        ListaEscaneos(index).Reiniciar()    ' Reinciamos el escaneo
                                        Debug.WriteLine("Escaneo " & ListaEscaneos(index).Path & ": Se reintentará")
                                        Errores -= 1
                                    End If
                                End If
                            Catch ex As Exception
                                Debug.WriteLine("Exception: La lista de Procesos de modificó de manera async afectando la lectura sync.")
                                Continue For
                            End Try
                        End If
                        EscaneosActivos = EscaneosActivos + 1             ' Aumentamos la variable de control que indica cuantos escaneos simultaneos hay actualmente
                        Thread.Sleep(50)          ' Damos 50ms antes de comenzar el siguiente escaneo, para dar tiempo al programa de modificar las variables de control antes de continuar
                        '                                     (NumThreads, Validos, etc)
                        Application.DoEvents()              ' Evitamos que se congele el UI
                    End If
                End If

                ' Mientras que no sea necesario crear un nuevo thread, esperamos a que algun escaneo termine para liberar un thread
                While EscaneosActivos >= MaximoEscaneosActivos Or Validos + EscaneosActivos >= MaximoEscaneosTotales And Validos < MaximoEscaneosTotales
                    Thread.Sleep(100)             ' Checamos cada 100ms si ya hay un thread disponible
                    Application.DoEvents()
                End While
            Next
            Debug.WriteLine("Sleeping.")
            For cs = 1 To 18000                             ' Threading.Thread.Sleep(10000 + 0 * 30 * 60 * 1000) ' Dormimos 30 Minutos
                Thread.Sleep(100)                 ' Se divide en varios para evitar congelar el UI
                Application.DoEvents()
            Next
            Debug.WriteLine("Done Sleeping.")
        Loop

        If (Date.Now.Hour >= HoraLimite) Then
            Debug.WriteLine("Se detuvo la busqueda de equipos por escanear debido a la hora. (" & If(HoraLimite > 12, HoraLimite - 12 & ":00 PM)", HoraLimite & ":00 AM)"))
        End If

        ' Al haberse detenido el scanner, quitamos las opciones del icono.
        mnuDetener.Visible = False
        mnuPausar.Visible = False
        mnuReanudar.Visible = False

        ' Esperamos a que todos los escaneos terminen
        While EscaneosActivos >= 1
            Thread.Sleep(100)     ' Checamos cada 0.1 segundos si ya no hay escaneos activos
            Application.DoEvents()
        End While

        timerUpdates.Stop()
        timerFrozen.Stop()

        Debug.WriteLine("Se hicieron " & Validos & " escaneos válidos.")
        btnEmpiezaEscaneos.Enabled = True   ' Al terminar, reactivamos el boton para poder comenzar los escaneos nuevamente
    End Sub

    '   Ejecuta un comando para cancelar el escaneo que se le indique, segun el inidce en dtgEscaneos
    Private Async Function CancelaEscaneo(index As Integer) As Task
        Try
            If (Not ListaEscaneos(index).Comenzado Or ListaEscaneos(index).Terminado) Then
                Exit Function
            End If

            ' Cancelamos el proceso y el conteo de archivos
            ListaEscaneos(index).Cancelar()
            dtgEscaneos.Item(1, index).Style.BackColor = Color.OrangeRed
            dtgEscaneos.Item(1, index).Style.SelectionBackColor = Color.OrangeRed
        Catch ex As Exception
            MsgBox("ERROR EN CANCELACION: " & ex.Message & vbNewLine & ex.StackTrace)
        End Try
    End Function

    '   Encuentra escaneos congelados cada minuto y los mata para permitir que uno nuevo inice
    Private Sub timerFrozen_Tick(sender As Object, e As EventArgs) Handles timerFrozen.Tick
        timerFrozen.Stop()
        For i = 0 To ListaEscaneos.Count - 1
            If (ListaEscaneos(i).Comenzado And Not ListaEscaneos(i).Terminado) Then
                If (ListaEscaneos(i).ChecaCongleado()) Then
                    CancelaEscaneo(i)
                End If
            End If
        Next
        timerFrozen.Start()
    End Sub



    ' --------------------------------------    TIMER DIARIO    --------------------------------------

    ' Comienza los escaneos todos los días a las 9AM (excepto domingo).
    Private Sub timerDiario_Tick(sender As Object, e As EventArgs) Handles timerDiario.Tick
        Dim fechaInicial As Date = Date.Now
        Dim fechaFinal As Date = fechaInicial
        Dim HoraInicio As Integer = 9   ' Hora de inicio diario de los escaneos (Formato 24 hrs)

        If (btnEmpiezaEscaneos.Enabled And Date.Now.DayOfWeek > 0) Then
            fechaFinal = fechaFinal.AddHours(HoraInicio - fechaFinal.Hour)
            fechaFinal = fechaFinal.AddDays(1)
            fechaFinal.AddMinutes(-fechaFinal.Minute)
            btnEmpiezaEscaneos.PerformClick()
        Else
            fechaFinal = fechaFinal.AddMinutes(30)  ' Si aun no terminan los escaneos, vuelve a intenarlo en 30 minutos.
        End If
        timerDiario.Interval = fechaFinal.Subtract(fechaInicial).TotalMilliseconds
        timerDiario.Enabled = True
        timerDiario.Start()
    End Sub

    ' Actualiza la base de datos periódicamente
    Private Sub timerDBUpdate_Tick(sender As Object, e As EventArgs) Handles timerDBUpdate.Tick
        Shell(Chr(34) & Privado.ProgramPath & Chr(34) & " update", AppWinStyle.Hide)
    End Sub



    ' --------------------------------------    CONTROL DE NOTIFICATION ICON    --------------------------------------

    '   Controlador del boton de "Show Application" en el Notification Icon
    Private Sub mnuShow_Click(sender As Object, e As EventArgs) Handles mnuShow.Click
        If (WasActive IsNot Nothing) Then
            WasActive.Visible = True    ' Hacemos visible la ultima ventana que se mostró
            WasActive.MinimizeBox = False
            WasActive.Activate()        ' Y la activamos para que tenga el enfoque del equipo    
            WasActive = Nothing
        Else
            Application.OpenForms.Item(0).Activate()
            Application.OpenForms.Item(0).WindowState = FormWindowState.Normal
        End If
        'Notify.Visible = False      ' Desactivamos el Notify Icon para cuando se vuelva a cerrar la ventana
    End Sub

    '   Controlador del Click en Notification Icon, valida que sea un click izquierdo
    Private Sub Notify_Click(sender As Object, e As MouseEventArgs) Handles Notify.MouseDown
        If (e.Button = MouseButtons.Left) Then
            mnuShow_Click(sender, e)
        End If
    End Sub

    '   Controlador del boton de "Detener Scanner" en el Notification Icon
    Private Sub mnuDetener_Click(sender As Object, e As EventArgs) Handles mnuDetener.Click
        Cancelado = True
        mnuDetener.Visible = False
        mnuPausar.Visible = False
        mnuReanudar.Visible = False
    End Sub

    '   Controlador del boton de "Pausar Scanner" en el Notification Icon
    Private Sub mnuPausar_Click(sender As Object, e As EventArgs) Handles mnuPausar.Click
        Pausado = True
        mnuPausar.Visible = False
        mnuReanudar.Visible = True
    End Sub

    '   Controlador del boton de "Reanudar Scanner" en el Notification Icon
    Private Sub mnuReanudar_Click(sender As Object, e As EventArgs) Handles mnuReanudar.Click
        Pausado = False
        mnuPausar.Visible = True
        mnuReanudar.Visible = False
    End Sub

    '   Controlador del boton de "Exit Application" en el Notification Icon
    Private Async Sub mnuExit_Click(sender As Object, e As EventArgs) Handles mnuExit.Click
        Cancelado = True
        For i = 0 To ListaEscaneos.Count - 1
            If (ListaEscaneos(i).Comenzado And Not ListaEscaneos(i).Terminado) Then
                Dim tmpIndex = i
                CancelaEscaneo(tmpIndex)
            End If
        Next
        Notify.Visible = False      ' Desactivamos el Notify Icon para que al cerrar la aplicación no se quede ahí
        Environment.Exit(1)         ' Forzamos el cierre de la aplicación
    End Sub

    '   Controla el comportamiento del programa cuando se intente cerrar el mismo
    Private Sub Escaneos_Closing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If (e.CloseReason = CloseReason.UserClosing) Then   ' Si el usuario cerro la ventana
            e.Cancel = True             ' Cancelamos el evento de cerrar el programa
            'Notify.Visible = True       ' Hacemos visible el Notify Icon
            sender.Visible = False      ' Hacemos invisible la ventana para simular que se cerró
            WasActive = sender          ' Definimos la ventana actual como la última ventana que se mostró
        Else                                                ' Si el ususario NO cerro la ventana, entonces debe de terminarse el programa
            Notify.Visible = False      ' Desactivamos el Notifiy Icon
            Cancelado = True            ' Definimos que los escaneos fueron cancelados para que tenga efecto en los threads individuales de cada escaneo
            Try
                For i = 0 To ListaEscaneos.Count - 1
                    If (Not ListaEscaneos(i).Proceso.HasExited) Then     ' Cerramos cada uno de los procesos que no se han cerrado
                        ListaEscaneos(i).Proceso.Kill()                  ' Terminamos el proceso
                        ListaEscaneos(i).Proceso.Close()                 ' Liberamos los recursos asignados ese proceso
                    End If
                Next
            Catch ex As Exception
            End Try
        End If
    End Sub




    ' --------------------------------------    OPCIONES EQUIPOS    --------------------------------------

    ' Deal with hovering over a cell.
    Private mouseLocation As DataGridViewCellEventArgs = New DataGridViewCellEventArgs(-1, -1)
    Private clickedCellRow As Integer = -1
    '   Cierra la ventana en caso de ser necesario
    Private Sub cmsEquipos_Opening(sender As Object, e As CancelEventArgs) Handles cmsEquipos.Opening
        Dim index As Integer = mouseLocation.RowIndex
        If (index = -1) Then
            e.Cancel = True
            Exit Sub
        End If
        clickedCellRow = index
    End Sub

    '   Registra cuando el cursor entra a una celda, actualizando mouseLocation
    Private Sub dtgEquipos_CellMouseEnter(ByVal sender As Object, ByVal location As DataGridViewCellEventArgs) Handles dtgEquipos.CellMouseEnter
        mouseLocation = location
    End Sub

    '   Registra cuando el cursor sale de una celda, actualizando mouseLocation
    Private Sub dtgEquipos_CellMouseLeave(ByVal sender As Object, ByVal location As DataGridViewCellEventArgs) Handles dtgEquipos.CellMouseLeave
        mouseLocation = New DataGridViewCellEventArgs(-1, -1)
    End Sub

    '   Inicia un escaneo de manera manual. Este no cuenta para el límite de maxThreads (escaneos en paralelo).
    Private Sub EscanearDiscoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EscanearDiscoToolStripMenuItem.Click
        Dim indiceEscaneo As Integer = -1
        Dim nombre As String = dtgEquipos.Item(1, clickedCellRow).Value

        For i = 0 To dtgEscaneos.RowCount - 1
            If (nombre = dtgEscaneos.Item(0, i).Value) Then
                indiceEscaneo = i
                Exit For
            End If
        Next
        If indiceEscaneo = -1 Then
            EscaneoManual(indiceEscaneo, nombre)
        ElseIf ListaEscaneos(indiceEscaneo).Terminado Then
            EscaneoManual(indiceEscaneo, nombre)
        End If

    End Sub




    ' --------------------------------------   OPCIONES ESCANEOS   --------------------------------------

    '   Registra cuando el cursor entra a una celda, actualizando mouseLocation
    Private Sub dtgEscaneos_CellMouseEnter(ByVal sender As Object, ByVal location As DataGridViewCellEventArgs) Handles dtgEscaneos.CellMouseEnter
        mouseLocation = location
    End Sub

    '   Registra cuando el cursor sale de una celda, actualizando mouseLocation
    Private Sub dtgEscaneos_CellMouseLeave(ByVal sender As Object, ByVal location As DataGridViewCellEventArgs) Handles dtgEscaneos.CellMouseLeave
        mouseLocation = New DataGridViewCellEventArgs(-1, -1)
    End Sub

    '   Decide que opciones mostrar en base al escaneo que se selecciono y el estatus del mismo. (Cancelar/Reiniciar)
    Private Sub cmsEscaneos_Opening(sender As Object, e As CancelEventArgs) Handles cmsEscaneos.Opening
        Dim index As Integer = mouseLocation.RowIndex
        If (index = -1) Then
            e.Cancel = True
            Exit Sub
        End If
        clickedCellRow = index
        Dim enProceso As Boolean = Not ListaEscaneos(index).Terminado
        ' Validación para que no se genere un optionbox si no hay una fila seleccionada
        If (enProceso) Then
            cmsEscaneos.Items.Item(0).Visible = True
            cmsEscaneos.Items.Item(1).Visible = False
        Else
            cmsEscaneos.Items.Item(0).Visible = False
            cmsEscaneos.Items.Item(1).Visible = True
        End If
    End Sub

    '   Maneja el click a la opción de cancelar un escaneo y manda llamar la función corresponidente
    Private Sub CancelarEscaneoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CancelarEscaneoToolStripMenuItem.Click
        'Task.Run(Async Function()
        '             Await CancelaEscaneo(clickedCellRow)
        '         End Function)
        CancelaEscaneo(clickedCellRow)
    End Sub

    '   Maneja el click a la opción de reintenar un escaneo. Ubica el disco en dtgEquipos y manda a llamar la función EscaneoManual
    Private Sub ReiniciarEscaneoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReiniciarEscaneoToolStripMenuItem.Click
        Dim nombre As String = dtgEscaneos.Item(0, clickedCellRow).Value
        For i = 0 To dtgEquipos.RowCount - 1
            If (nombre = dtgEscaneos.Item(0, i).Value) Then
                EscaneoManual(i, nombre)
                Exit For
            End If
        Next
    End Sub

    '   Recibe como parametros el indice del disco a escanear en dtgEquipos y el nombre de dicho disco. Inicia un escaneo, checando que no haya uno en proceso.
    Private Sub EscaneoManual(indiceEscaneo As Integer, nombre As String)
        mnuDetener.Visible = True
        Dim indiceEquipo As Integer = -1
        For i = 0 To dtgEquipos.RowCount - 1
            If (dtgEquipos.Item(1, i).Value = nombre) Then
                indiceEquipo = i
            End If
        Next
        If (indiceEquipo = -1) Then ' Si no se encontró el equipo
            Exit Sub
        End If
        If (indiceEscaneo = -1) Then
            lblEscaneos.Text = "Total Escaneos: " & Validos + Errores + EscaneosActivos + Manuales

            Dim nombreDisco As String = nombre.Substring(0, nombre.Length - 1)
            Dim escaneo As Escaneo = New Escaneo(dtgEscaneos.RowCount, ListaDiscos(indiceEquipo), True)
            ListaEscaneos.Add(escaneo)
            ListaEscaneos.Last.Comenzar()

            dtgEscaneos.Rows.Add()  ' Generamos una nueva fila en la lista de escaneos para registrar uno nuevos
            indiceEscaneo = dtgEscaneos.RowCount - 1
            dtgEscaneos.Item(0, indiceEscaneo).Value = nombre    ' Ingresamos la direccion que se esta escaneando en la columna 0
            dtgEscaneos.Item(1, indiceEscaneo).Value = 0         ' Ingresamos un 0 en la columna 1 para indicar un 0% de escaneo completado
            dtgEscaneos.Item(0, indiceEscaneo).Style.BackColor = Color.WhiteSmoke
            dtgEscaneos.Item(0, indiceEscaneo).Style.SelectionBackColor = Color.WhiteSmoke
            Debug.WriteLine("Escaneo " & ListaEscaneos(indiceEscaneo).Path & ": Se inició manualmente")

            ' Comenzamos los updates del UI
            timerUpdates.Start()
            timerFrozen.Start()
        Else
            Try
                ' Si ya hay un escaneo activo para este equipo, no iniciamos otro
                If (Not ListaEscaneos(indiceEscaneo).Terminado) Then
                    Exit Sub
                End If
                dtgEscaneos.Item(1, indiceEscaneo).Value = 0
                dtgEscaneos.Item(0, indiceEscaneo).Style.BackColor = Color.WhiteSmoke
                dtgEscaneos.Item(0, indiceEscaneo).Style.SelectionBackColor = Color.WhiteSmoke
                dtgEscaneos.Item(1, indiceEscaneo).Style.SelectionBackColor = Color.White
                dtgEscaneos.Item(1, indiceEscaneo).Style.BackColor = Color.White
                dtgEscaneos.Item(2, indiceEscaneo).Value = 0
                ListaEscaneos(indiceEscaneo).Reiniciar()
                Debug.WriteLine("Escaneo " & ListaEscaneos(indiceEscaneo).Path & ": Se reintentará manualmente")
                If (ListaEscaneos(indiceEscaneo).Progreso = 100) Then
                    Validos -= 1
                    lblValidos.Text = "Válidos: " & Validos
                Else
                    Errores -= 1
                    lblErrores.Text = "Errores: " & Errores
                End If
            Catch ex As Exception
                Debug.WriteLine("Exception: La lista de Procesos de modificó de manera async afectando la lectura sync.")
            End Try
        End If
        Manuales += 1
        lblEscaneos.Text = "Total Escaneos: " & Validos + Errores + EscaneosActivos + Manuales
        If Validos + Errores + EscaneosActivos + Manuales = 0 Then
            mnuDetener.Visible = False
        End If
    End Sub




    ' Pendientes por arreglar:
    '   Encontrar la razon de procesos colgados. (No son permisos ni escaneos validos, posiblemente cancelacion de contador de archivos y errores que lanza)
    '   Cuando escanea 1 archivo marcar como "Error de permisos en disco" (implementado)
    '   Cuando es pingeable pero escanea 0, marcar como "Disco no compartido(permisos????)" (implementado)
    '   Checar porque los escaneos cancelados no tienen texto (algunos) (implementado crasheo por no tener process con trycatch)
    '   Corregir cuenta de archivos en escaneo (se encuentran 1/3 de los archivos)
    '   Mejorar la forma de leer el porcentaje de archivos escaneados (escaneados y totales) (al final del escaneo viene un "running 100%")
    '   Buscar la razón de los congelamientos de escaneo (No es RAM)

    ' Agrega un equipo a la base de datos
    Private Async Sub btnAgregar_Click(sender As Object, e As EventArgs) Handles btnAgregar.Click
        btnAgregar.Enabled = False
        Dim nombre As String = InputBox("Ingresa el nombre del equipo:")
        Dim discos As List(Of String) = New List(Of String)
        Dim discosCapacidad As List(Of String) = New List(Of String)
        Dim discosEscaneo As List(Of Date) = New List(Of Date)
        Dim disco As String = InputBox("Ingresa el nombre del siguiente disco.")
        If (disco <> "") Then
            Do While disco <> ""
                discos.Add(disco & ":")
                discosEscaneo.Add(Date.MinValue.Date)
                discosCapacidad.Add("-")
                disco = InputBox("Ingresa el nombre del siguiente disco.")
            Loop
            Dim equipo As ParseObject = New ParseObject("Equipos")
            equipo.Add("NombreDeRed", nombre)
            equipo.Add("DiscosNombre", discos)
            equipo.Add("DiscosDisponible", discosCapacidad)
            equipo.Add("DiscosTotal", discosCapacidad)
            equipo.Add("UltimoEscaneo", discosEscaneo)
            equipo.Add("Actualizar", True)
            equipo.Add("Escaneos", True)
            Try
                Await equipo.SaveAsync()
            Catch ex As Exception
                MsgBox("No se pudo guardar")
            End Try
        End If
        Await LlenaEquipos()
        btnAgregar.Enabled = True
    End Sub


    ' Busca archivos de manera recursiva, evitando errores de permisos
    Public Shared Function GetFilesRecursive(ByVal initial As String, ct As CancellationToken) As Integer
        ' This list stores the results.
        Dim result As Integer = 0

        ' This stack stores the directories to process.
        Dim stack As New Stack(Of String)

        ' Add the initial directory
        stack.Push(initial)

        ' Continue processing for each stacked directory
        Do While (stack.Count > 0)
            ' Get top directory string
            Dim dir As String = stack.Pop
            Try
                ' Add all immediate file paths
                result += (Directory.GetFiles(dir, "*.*")).Length

                ' Loop through all subdirectories and add them to the stack.
                Dim directoryName As String
                For Each directoryName In Directory.GetDirectories(dir)
                    stack.Push(directoryName)
                Next

                ct.ThrowIfCancellationRequested()
            Catch cancel As OperationCanceledException
                stack.Clear()
                Return -1
            Catch ex As Exception
            End Try
        Loop
        Return result
    End Function

End Class
