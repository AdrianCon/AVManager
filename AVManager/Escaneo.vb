Imports System.Threading
Imports System.Globalization
Imports Parse
Imports System.IO

Public Class Escaneo
    '   Información del destino (Todos los public se protegen de escritura a través de properties)
    Private _Path As String
    Private _Equipo As String
    Private _Disco As Disco

    '   Información del escaneo
    Private _RowIndex As Integer
    Private ID As String
    Private Manual As Boolean
    Private Congelado As Boolean
    Private _Comenzado As Boolean
    Private _Terminado As Boolean
    Private Terminando As Boolean
    Private Cancelando As Boolean
    Private _ArchivosEscaneados As Integer
    Private _Progreso As Integer
    Private FileCount As Long
    Private FrozenFileCount As Integer
    Private _Frozen As Boolean
    Private Output As String

    '   Controladores del escaneo
    Private _Proceso As Process
    Private ctsContador As CancellationTokenSource

    Public Property Path() As String
        Get
            Return _Path
        End Get
        Private Set(value As String)
            _Path = value
        End Set
    End Property                    ' Path - Public Get, Private Set
    Public Property Equipo() As String
        Get
            Return _Equipo
        End Get
        Private Set(value As String)
            _Equipo = value
        End Set
    End Property                  ' Equipo - Public Get, Private Set
    Public Property Disco() As Disco
        Get
            Return _Disco
        End Get
        Private Set(value As Disco)
            _Disco = value
        End Set
    End Property                    ' Disco - Public Get, Private Set
    Public Property RowIndex() As Integer
        Get
            Return _RowIndex
        End Get
        Private Set(value As Integer)
            _RowIndex = value
        End Set
    End Property               ' RowIndex - Public Get, Private Set
    Public Property Comenzado() As Boolean
        Get
            Return _Comenzado
        End Get
        Private Set(value As Boolean)
            _Comenzado = value
        End Set
    End Property              ' Comenzado - Public Get, Private Set
    Public Property Terminado() As Boolean
        Get
            Return _Terminado
        End Get
        Private Set(value As Boolean)
            _Terminado = value
        End Set
    End Property              ' Terminado - Public Get, Private Set
    Public Property ArchivosEscaneados() As Integer
        Get
            Return _ArchivosEscaneados
        End Get
        Private Set(value As Integer)
            _ArchivosEscaneados = value
        End Set
    End Property     ' ArchivosEscaneados - Public Get, Private Set
    Public Property Progreso() As Integer
        Get
            Return _Progreso
        End Get
        Private Set(value As Integer)
            _Progreso = value
        End Set
    End Property               ' Progreso - Public Get, Private Set
    Public Property Frozen() As Boolean
        Get
            Return _Frozen
        End Get
        Private Set(value As Boolean)
            _Frozen = value
        End Set
    End Property                 ' Frozen - Public Get, Private Set
    Public Property Proceso() As Process
        Get
            Return _Proceso
        End Get
        Private Set(value As Process)
            _Proceso = value
        End Set
    End Property                ' Proceso - Public Get, Private Set


    Public Sub New(ByVal index As Integer, ByRef disco As ParseObject, ByVal manual As Boolean)
        Me.Equipo = disco.Get(Of ParseObject)("Equipo").Get(Of String)("NombreDeRed")
        Me.Disco = New Disco(disco)
        Path = "\\" & Me.Equipo & "\" & Me.Disco.Nombre

        RowIndex = index
        ID = Nothing    ' ID es configurado cuando se incia el proceso
        Me.Manual = manual
        Congelado = False
        Comenzado = False
        Terminado = False
        Terminando = False
        Cancelando = False
        ArchivosEscaneados = 0
        Progreso = 0
        FileCount = 0
        FrozenFileCount = 0
        Proceso = New Process()    ' Generamos el proceso que meteremos al arreglo de procesos despues de configurarlo
        Proceso.StartInfo = New ProcessStartInfo With {
            .Arguments = "scan" & " " & Chr(34) & Path & Chr(34),
            .FileName = Privado.ProgramPath,
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardInput = True
        }
        Output = ""
        ctsContador = New CancellationTokenSource()
    End Sub

    Public Sub Comenzar()
        ContarArchivos()
        Task.Run(Async Function()
                     Escanear()
                 End Function)
        Comenzado = True
    End Sub

    Private Sub Escanear()
        Output = ""
        Try
            Proceso.Start()
            Do While Not Proceso.HasExited  ' Mientras que el escaneo no haya terminado
                Try
                    Output = Proceso.StandardOutput.ReadLine()                          ' Leemos una linea del output
                    If (Output IsNot Nothing) Then
                        If (Output.Contains("Progress ")) Then                          ' Si la linea contiene "Progress" significa que esa linea indica nuestro % de completado
                            If (FileCount = -1) Then
                                Try                                                     ' En caso de que haya errores, se controlará mas delante
                                    Output = Output.Substring(Output.Length - 6, 2)     ' Leemos el % de completado dentro del string (ejemplo: "Progress 17%...")
                                    If (Not IsNumeric(Output(0))) Then                  ' Si el primer caracter no es un número, siginica que el porcentaje esta dado solamente por un dígito
                                        Output = Output.Remove(0, 1)                    ' Eliminamos el texto y dejamos solo el caracter numérico
                                    End If
                                    Progreso = CInt(Output)                             ' Escribimos el % completado en el arreglo de Valores (en su indice correspondiente)
                                Catch ex2 As Exception
                                End Try
                            Else
                                Try
                                    Dim valor As Integer = CInt(CDbl(ArchivosEscaneados / FileCount) * 100)
                                    Progreso = If(valor > 100, 100, valor)
                                Catch ex As Exception
                                End Try
                            End If
                        ElseIf (Output.Contains("Scan_Objects$")) Then
                            Dim iStart As Integer = Output.IndexOf("Scan_Objects$")
                            Dim iLength As Integer = Output.IndexOf(" ", iStart) - iStart
                            ID = Output.Substring(iStart, iLength)
                        ElseIf (Output.Substring(Output.Length - 1 - 2).Contains("ok") Or Output.Contains("skipped: no rights")) Then
                            ArchivosEscaneados += 1
                        ElseIf (Output.Contains("Statistics")) Then
                            Exit Do
                        Else
                            'Debug.WriteLine("No se contó linea: " & output) ' debug
                        End If
                    End If
                Catch ex3 As Exception
                    Debug.WriteLine("Escaneo " & Path & ": Catched exception. " & ex3.Message)
                End Try
            Loop
        Catch ex As Exception
            Debug.WriteLine("Escaneo " & Path & " crasheó. " & ex.Message)
        End Try
    End Sub

    Public Async Sub Reiniciar()
        If (Terminado) Then
            Progreso = 0
            ArchivosEscaneados = 0
            Comenzado = False
            Terminando = False
            Terminado = False
            Cancelando = False
            Manual = True
            Congelado = False
            FrozenFileCount = 0
            ' FileCount no se reinicia, para aprovechar el conteo de archivos que ya tenia antes y comoquiera se inicia uno nuevo
            ctsContador.Cancel()
            ctsContador.Dispose()
            ctsContador = New CancellationTokenSource()
            Output = ""
            Comenzar()
        Else
            MsgBox("Error al reiniciar " & Path)
        End If
    End Sub

    Public Function Cancelar() As Boolean
        Try
            If (Cancelando Or Terminado) Then
                Return False
            End If

            Cancelando = True
            ' Cancelamos el proceso y el conteo de archivos
            Dim scanID As String = ID
            Shell(Chr(34) & Privado.ProgramPath & Chr(34) & " stop " & scanID & " /password=" & Privado.Password, AppWinStyle.Hide)
            ctsContador.Cancel()

            ' Esperamos 5 segundos a que termine el proceso para recibir el código de salida. En caso de no tenerlo, se marca como no terminado y se reintenta registrar
            Task.Run(Async Function()
                         Dim tiempoTotal As Integer = 0
                         Do Until Proceso.WaitForExit(100) Or tiempoTotal > 5 * 1000
                             Application.DoEvents()
                             tiempoTotal += 100
                         Loop
                         If (Not Proceso.HasExited) Then
                             Cancelando = False
                             Cancelar()
                         End If
                     End Function)
        Catch ex As Exception
            Cancelando = False
            Return False
        End Try
        Return True
    End Function

    Public Sub Terminar()
        Try
            If (Terminando Or Terminado) Then
                Exit Sub
            End If

            Terminando = True
            If (Progreso >= 99) Then ' Or Progreso.ExitCode = codeSuccess And FileCount > Threshold
                Progreso = 100
                RegistraReporte()
            Else
                RegistraReporteError()  ' Registra un reporte de el escaneo fallido al igual que su razón de falla. Esto con una finalidad analítica de datos.
            End If

            Terminado = True

        Catch ex As Exception
            Terminando = False
        End Try
    End Sub

    Public Function ChecaCongleado() As Boolean
        If (FrozenFileCount = ArchivosEscaneados) Then
            ' Cancelamos el escaneo debido a que no ha respondido
            Debug.WriteLine(Path & " esta congelado")
            Frozen = True
        End If
        FrozenFileCount = ArchivosEscaneados
        Return Frozen
    End Function

    '   Lee el output de los procesos asíncronos, recibe un entero indicando cuantas lineas quieres brincar
    Private Function LeeLineas(p As Process, lineas As Integer) As String
        Dim output As String = ""
        Try
            For i = 1 To lineas
                output = p.StandardOutput.ReadLine
            Next
        Catch ex As Exception
        End Try
        Return output
    End Function

    '   Registra el reporte del escaneo a la base de datos y actualiza el atributo UltimoEscaneo del equipo
    Private Async Sub RegistraReporte()
        ' También en la parte de UltimoEscaneo hay que hacerlo un arreglo de Dates para que cada uno represente cada uno de sus discos
        ' Inicialización
        Dim linea As String = ""
        Dim statistics As List(Of String) = New List(Of String)

        Try
            ' Leemos el resto del StdOutput hasta encontrar la linea que indica el inicio de los Statistics
            Do While (Not Proceso.StandardOutput.EndOfStream And Not Output.Contains("Statistics"))
                Output = LeeLineas(Proceso, 1)
                Application.DoEvents()
            Loop

            ' Si ya se llego a EndOfStream, no tenemos los datos para registrarlos.
            If (Proceso.StandardOutput.EndOfStream) Then
                Throw (New Exception("EOF Reached."))
                Exit Sub
            End If

            ' Leemos todas las lineas de los statistics
            Do While (Not Proceso.StandardOutput.EndOfStream)
                statistics.Add(LeeLineas(Proceso, 1))
            Loop

            ' Leemos y guardamos los datos correspondientes al escaneo actual
            Dim nombre As String = Path.Substring(2)
            ' Fecha Inicio Escaneo
            linea = statistics(0)
            Dim fechaEscaneo As String = Mid(linea, InStr(linea, ":") + 2)
            Dim fechaInicio As Date = Date.ParseExact(fechaEscaneo, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime
            ' Fecha Fin Escaneo
            linea = statistics(1)
            Dim fechaEscaneoFin As String = Mid(linea, InStr(linea, ":") + 2)
            Dim fechaFin As Date = Date.ParseExact(fechaEscaneoFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime
            ' Processed Objects
            linea = statistics(3)
            Dim totalEscaneado As String = Mid(linea, InStr(linea, ":") + 2)
            ' Total Detected
            linea = statistics(5)
            Dim totalDetectado As String = Mid(linea, InStr(linea, ":") + 2)

            ' Aseguramos que el proceso no se quede esperando que lean algo de el
            Await Proceso.StandardOutput.ReadToEndAsync()
            Proceso.StandardOutput.Close()

            ' Agregamos los datos a un objeto de Parse y damos guardar
            Dim escaneo As ParseObject = New ParseObject("Escaneos")
            escaneo.Add("Nombre", nombre)
            escaneo.Add("FechaInicio", fechaInicio)
            escaneo.Add("FechaFin", fechaFin)
            escaneo.Add("Porcentaje", Progreso & "%")
            escaneo.Add("TotalEscaneado", totalEscaneado)
            escaneo.Add("TotalDetectado", totalDetectado)
            escaneo.Add("ExitCode", Proceso.ExitCode)
            Await escaneo.SaveAsync()

            '   Actualizamos el atibuto UltimoEscaneo del equipo y Total Escaneado
            Dim query = ParseObject.GetQuery("Equipos").WhereEqualTo("NombreDeRed", Equipo)
            Dim result = (Await query.FindAsync()).ToList
            If result.Count >= 1 Then
                For i = 0 To result.Count - 1
                    Dim equipo = result.Item(i)
                    Dim relation As ParseRelation(Of ParseObject) = equipo.Get(Of ParseRelation(Of ParseObject))("Discos")
                    Dim discos As List(Of ParseObject) = (Await relation.Query.FindAsync).ToList
                    Dim NumDiscos = discos.Count
                    For diskIndex = 0 To NumDiscos - 1
                        Dim disco As ParseObject = discos(diskIndex)
                        If (disco.Get(Of String)("Nombre") = Me.Disco.Nombre) Then
                            If (disco.ContainsKey("UltimoEscaneo")) Then
                                disco.Item("UltimoEscaneo") = fechaInicio
                            Else
                                disco.Add("UltimoEscaneo", fechaInicio)
                            End If
                            Dim total As Integer
                            Try
                                total = CInt(totalEscaneado)
                            Catch ex As Exception
                                total = 0
                            Finally
                                If (disco.ContainsKey("TotalEscaneado")) Then
                                    disco.Item("TotalEscaneado") = total
                                Else
                                    disco.Add("TotalEscaneado", total)
                                End If
                            End Try
                            Dim relationDisco As ParseRelation(Of ParseObject) = disco.Get(Of ParseRelation(Of ParseObject))("Escaneos")
                            relationDisco.Add(escaneo)
                            Await disco.SaveAsync()
                            Exit For
                        End If
                    Next
                Next
            Else
                MsgBox("No se encontro: " & Chr(34) & Path & Chr(34) & " en Parse.")
            End If
        Catch ex As Exception
            Debug.WriteLine("Escaneo " & Path & ": Error al guardar el reporte. " & ex.Message)
        End Try
    End Sub

    '   Registra el reporte de error del escaneo a la base de datos
    Private Async Sub RegistraReporteError()
        ' Inicialización
        Dim linea As String = ""
        Dim statistics As List(Of String) = New List(Of String)

        Try
            ' Leemos el resto del StdOutput hasta encontrar la linea que indica el inicio de los Statistics
            Do While (Not Output.Contains("Statistics") And Not Proceso.StandardOutput.EndOfStream)
                Output = LeeLineas(Proceso, 1)
                Application.DoEvents()
            Loop

            If (Proceso.StandardOutput.EndOfStream) Then
                Throw (New Exception("EOF Reached."))
            End If

            ' Leemos todas las lineas de los statistics
            Do While (Not Proceso.StandardOutput.EndOfStream)
                statistics.Add(LeeLineas(Proceso, 1))
            Loop

            ' Leemos y guardamos los datos correspondientes al escaneo actual
            Dim disco As String = Path
            Dim nombre As String = disco.Substring(2)
            ' Fecha Inicio de Escaneo
            linea = statistics(0)
            Dim fechaEscaneo As String = Mid(linea, InStr(linea, ":") + 2)
            Dim fechaInicio As Date = Date.ParseExact(fechaEscaneo, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime
            ' Fecha Fin de Escaneo
            linea = statistics(1)
            Dim fechaEscaneoFin As String = Mid(linea, InStr(linea, ":") + 2)
            Dim fechaFin As Date = Date.ParseExact(fechaEscaneoFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime
            ' Processed Objects
            linea = statistics(3)
            Dim totalEscaneado As String = Mid(linea, InStr(linea, ":") + 2)
            ' Total Detected
            linea = statistics(5)
            Dim totalDetectado As String = Mid(linea, InStr(linea, ":") + 2)

            ' Aseguramos que el proceso no se quede esperando que lean algo de el
            Proceso.StandardOutput.ReadToEndAsync()
            Proceso.StandardOutput.Close()

            ' Agregamos los datos a un objeto de Parse y damos guardar
            Dim escaneo As ParseObject = New ParseObject("Escaneos")
            escaneo.Add("Nombre", nombre)
            escaneo.Add("FechaInicio", fechaInicio)
            escaneo.Add("FechaFin", fechaFin)   ' Fecha fin parece no ser parte de los logs
            escaneo.Add("Porcentaje", Progreso & "%")
            escaneo.Add("TotalEscaneado", totalEscaneado)
            escaneo.Add("TotalDetectado", totalDetectado)
            escaneo.Add("ExitCode", Proceso.ExitCode)
            Await escaneo.SaveAsync()

            '   Actualizamos el atibuto UltimoEscaneo del equipo y Total Escaneado
            Dim query = ParseObject.GetQuery("Equipos").WhereEqualTo("NombreDeRed", Equipo)
            Dim result = (Await query.FindAsync()).ToList
            If result.Count >= 1 Then
                For i = 0 To result.Count - 1
                    Dim equipo = result.Item(i)
                    Dim relation As ParseRelation(Of ParseObject) = equipo.Get(Of ParseRelation(Of ParseObject))("Discos")
                    Dim discos As List(Of ParseObject) = (Await relation.Query.FindAsync).ToList
                    Dim NumDiscos = discos.Count
                    For diskIndex = 0 To NumDiscos - 1
                        Dim discoParse As ParseObject = discos(diskIndex)
                        If (discoParse.Get(Of String)("Nombre") = Me.Disco.Nombre) Then
                            Dim relationDisco As ParseRelation(Of ParseObject) = discoParse.Get(Of ParseRelation(Of ParseObject))("Escaneos")
                            relationDisco.Add(escaneo)
                            Await discoParse.SaveAsync()
                            Debug.WriteLine("Escaneo \\" & nombre & ": Reporte de error guardado correctamente.")
                            Exit For
                        End If
                    Next
                Next
            Else
                MsgBox("No se encontro: " & Chr(34) & Path & Chr(34) & " en Parse.")
            End If
        Catch ex As Exception
            Debug.WriteLine("Escaneo " & Path & ": Error al guardar el reporte de error. " & ex.Message)
        End Try
    End Sub

    '   Inicia el conteo de archivos en el disco a escanear para medir el progreso
    Private Sub ContarArchivos()
        Task.Run(Function()
                     ' Get recursive List of all files starting in this directory.
                     Debug.WriteLine("Empieza el conteo de archivos para " & Path)
                     Dim numArchivos As Long = -1
                     numArchivos = GetFilesRecursive(Path, ctsContador.Token)
                     If numArchivos = -1 Then Debug.WriteLine("Escaneo " & Path & ": Se cancelo el conteo de archivos.")
                     FileCount = numArchivos
                     Debug.WriteLine(Path & " tiene " & numArchivos & " archivos.")
                 End Function, ctsContador.Token)
    End Sub

    ' Busca archivos de manera recursiva, evitando errores de permisos
    Private Function GetFilesRecursive(ByVal initial As String, ct As CancellationToken) As Long
        ' This list stores the results.
        Dim result As Long = 0

        ' This stack stores the directories to process.
        Dim stack As New Stack(Of String)

        ' Add the initial directory
        stack.Push(initial)

        ' Continue processing for each stacked directory
        Dim dir As String
        Do While (stack.Count > 0)
            ' Get top directory string
            dir = stack.Pop
            Try
                ' Add all immediate file paths
                result += (Directory.GetFiles(dir, "*.*")).LongLength

                ' Loop through all subdirectories and add them to the stack.
                Dim directoryName As String
                Dim directories = Directory.GetDirectories(dir)
                For Each directoryName In directories
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