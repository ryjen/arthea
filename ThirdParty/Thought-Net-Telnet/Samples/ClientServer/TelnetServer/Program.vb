
Option Explicit On
Option Strict On

Imports System
Imports System.Net
Imports System.Text
Imports System.Threading
Imports Thought.Net.Telnet

' ========================================================================
' Telnet Client/Server Example
' ========================================================================

Module Program

    Private WithEvents Listener As TelnetListener

    ' ====================================================================
    ' HandleListenerStarted
    ' --------------------------------------------------------------------
    ' Handles the Started event of the telnet listener.
    ' ====================================================================

    Private Sub HandleListenerStarted( _
          ByVal sender As Object, _
          ByVal e As System.EventArgs) Handles Listener.Started

        Log("Listener started on " & Listener.LocalEndPoint.ToString())
        Log("Press Control-C to exit this server.")

    End Sub

    ' ====================================================================
    ' HandleListenerStopped
    ' --------------------------------------------------------------------
    ' Handles the Stopped event of the telnet listener.
    ' ====================================================================

    Private Sub HandleListenerStopped( _
          ByVal sender As Object, _
          ByVal e As EventArgs) Handles Listener.Stopped

        Log("Listener stopped.")

    End Sub

    ' ====================================================================
    ' Log(string)
    ' --------------------------------------------------------------------
    ' Writes a message to the console window.
    ' ====================================================================

    Public Sub Log(ByVal text As String)
        Log(text, ConsoleColor.Gray)
    End Sub

    ' ====================================================================
    ' Log
    ' --------------------------------------------------------------------
    ' Logs a message on the screen.
    ' ====================================================================

    Public Sub Log(ByVal text As String, ByVal color As ConsoleColor)

        If Not String.IsNullOrEmpty(text) Then

            Console.Write("[")
            Console.Write(Now)
            Console.Write("] ")

            Console.ForegroundColor = color
            Console.WriteLine(text)
            Console.ResetColor()

        End If

    End Sub

    ' ====================================================================
    ' Main
    ' --------------------------------------------------------------------
    ' This is the entry point of the application.
    ' ====================================================================

    Public Sub Main()

        Listener = New TelnetListener(IPAddress.Loopback, 23)
        listener.Start()

        Do

            ' The Accept method is a blocking call -- that is, it
            ' will not return until a new connection is pending
            ' or the server has been terminated.

            Dim client As TelnetClient = Listener.Accept()

            Log("Incoming from " & client.Parser.Socket.RemoteEndPoint.ToString)

            ' Each connection will be serviced on a separate thread.  A
            ' thread is an instance of executing code that runs at
            ' the same time as other code.  The server will go back and
            ' wait for the next connection request once the thread has
            ' been started.  Each telnet connection gets its own thread.

            Dim session As New Session(client)
            Dim sessionThread As New Thread(AddressOf session.StartSession)
            sessionThread.Start()

        Loop

    End Sub

End Module
