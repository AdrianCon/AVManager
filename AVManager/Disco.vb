Imports Parse

Public Class Disco
    Private _Nombre As String
    Private _Escaneos As Boolean
    Private _EspacioUtilizado As Double
    Private _EspacioTotal As Double
    Private _Serial As String
    Private _UltimoEscaneo As Date

    Public Property Nombre() As String
        Get
            Return _Nombre
        End Get
        Private Set(value As String)
            _Nombre = value
        End Set
    End Property               ' Nombre - Public Get, Private Set
    Public Property Escaneos() As Boolean
        Get
            Return _Escaneos
        End Get
        Private Set(value As Boolean)
            _Escaneos = value
        End Set
    End Property            ' Escaneos - Public Get, Private Set
    Public Property EspacioUtilizado() As Double
        Get
            Return _EspacioUtilizado
        End Get
        Private Set(value As Double)
            _EspacioUtilizado = value
        End Set
    End Property     ' EspacioUtilizado - Public Get, Private Set
    Public Property EspacioTotal() As Double
        Get
            Return _EspacioTotal
        End Get
        Private Set(value As Double)
            _EspacioTotal = value
        End Set
    End Property         ' EspacioTotal - Public Get, Private Set
    Public Property Serial() As String
        Get
            Return _Serial
        End Get
        Private Set(value As String)
            _Serial = value
        End Set
    End Property               ' Serial - Public Get, Private Set
    Public Property UltimoEscaneo() As Date
        Get
            Return _UltimoEscaneo
        End Get
        Private Set(value As Date)
            _UltimoEscaneo = value
        End Set
    End Property          ' UltimoEscaneo - Public Get, Private Set


    Public Sub New(ByVal disco As ParseObject)
        Try
            If disco.ContainsKey("Nombre") Then
                Nombre = disco.Get(Of String)("Nombre")
            Else
                Throw (New MissingFieldException("Disco", "Nombre"))
            End If

            If disco.ContainsKey("Escaneos") Then
                Escaneos = disco.Get(Of Boolean)("Escanear")
            Else
                Throw (New MissingFieldException("Disco", "Escaneos"))
            End If

            If disco.ContainsKey("UltimoEscaneo") Then
                UltimoEscaneo = disco.Get(Of Date)("UltimoEscaneo").ToLocalTime
            Else
                Throw (New MissingFieldException("Disco", "UltimoEscaneo"))
            End If

            If disco.ContainsKey("EspacioUtilizado") Then
                EspacioUtilizado = disco.Get(Of Double)("EspacioUtilizado")
            Else
                'Throw (New MissingFieldException("Disco", "EspacioUtilizado"))
            End If

            If disco.ContainsKey("EspacioTotal") Then
                EspacioTotal = disco.Get(Of Double)("EspacioTotal")
            Else
                'Throw (New MissingFieldException("Disco", "EspacioTotal"))
            End If

            If disco.ContainsKey("Serial") Then
                Serial = disco.Get(Of String)("Serial")
            Else
                'Throw (New MissingFieldException("Disco", "Serial"))
            End If
        Catch ex As MissingFieldException
            Debug.WriteLine("Error al crear instancia de disco nuevo. (" & ex.Message & ")")
        End Try
    End Sub
End Class