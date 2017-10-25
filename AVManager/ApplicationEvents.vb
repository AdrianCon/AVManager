Imports System.Deployment.Application
Imports Microsoft.VisualBasic.ApplicationServices
Imports Parse

Namespace My
    ' Los siguientes eventos están disponibles para MyApplication:
    ' Inicio: se desencadena cuando se inicia la aplicación, antes de que se cree el formulario de inicio.
    ' Apagado: generado después de cerrar todos los formularios de la aplicación. Este evento no se genera si la aplicación termina de forma anómala.
    ' UnhandledException: generado si la aplicación detecta una excepción no controlada.
    ' StartupNextInstance: se desencadena cuando se inicia una aplicación de instancia única y la aplicación ya está activa. 
    ' NetworkAvailabilityChanged: se desencadena cuando la conexión de red está conectada o desconectada.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup

            ' Configuramos la conexión con el servidor de Parse local hosteado en el Servidor
            Dim config As ParseClient.Configuration = New ParseClient.Configuration
            config.Server = Privado.ParseServer
            config.ApplicationId = Privado.ParseApplicationID
            ParseClient.Initialize(config)

            ' Registramos/Actualizamos la instalación en la base de datos
            If ApplicationDeployment.IsNetworkDeployed Then ' Si no estamos debugging
                If Application.Deployment.IsFirstRun Then   ' Solo la primera vez que se corra el programa despues de instalar una actualización
                    Dim install As ParseInstallation = ParseInstallation.CurrentInstallation
                    install.Add("NombreDeRed", My.Computer.Name)
                    install.SaveAsync()
                End If
            End If
        End Sub

        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            ' Registramos el error e informamos al usuario de reportar el error.
            Debug.WriteLine("Unhandled Exception: " & e.Exception.Message)
            MsgBox("Se encontró un problema con el programa." & vbNewLine & "Favor de reportarlo con el administrador del sistema.")
        End Sub
    End Class
End Namespace
