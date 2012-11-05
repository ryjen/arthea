
Option Explicit On
Option Strict On

Imports System
Imports System.IO
Imports System.Text
Imports Thought.Net.Telnet

' ========================================================================
' Session
' ------------------------------------------------------------------------
' This class represents a session to the server.
' ========================================================================

Public Class Session

    Private _client As TelnetClient ' The core telnet client
    Private _echoed As Boolean
    Private _reader As StreamReader ' A reader for the telnet stream
    Private _stream As TelnetStream ' A stream provided by the client
    Private _writer As StreamWriter ' A writer for the telnet stream

    Public Sub New(ByVal client As TelnetClient)

        If client Is Nothing Then
            Throw New ArgumentNullException("client")
        End If

        _client = client

    End Sub

    ' ====================================================================
    ' Execute
    ' --------------------------------------------------------------------
    ' Executes a text command entered by the user.
    ' ====================================================================

    Public Sub Execute(ByVal command As String)

        If String.IsNullOrEmpty(command) Then
            Throw New ArgumentNullException("command")
        End If

        Dim spaceIndex As Integer = _
            command.IndexOfAny(New Char() {" "c})

        Dim name As String
        Dim params As String

        If spaceIndex = -1 Then

            name = command.Trim()
            params = Nothing

        Else

            name = command.Substring(0, spaceIndex)
            params = command.Substring(spaceIndex).Trim()

        End If

        Select Case name.ToUpperInvariant()

            Case "AO"
                _client.Parser.SendCommand(TelnetCommand.AbortOutput)

            Case "AYT"
                _client.Parser.SendCommand(TelnetCommand.AreYouThere)

            Case "GA"
                _client.Parser.SendGoAhead()

            Case "IP"
                _client.Parser.SendCommand(TelnetCommand.InterruptProcess)

            Case "NOP"
                _client.Parser.SendCommand(TelnetCommand.NoOperation)

        End Select

        ' Later implement WILL, WONT, etc.

    End Sub

    ' ====================================================================
    ' HandleCommandReceived
    ' --------------------------------------------------------------------
    ' Executed when the server receives a simple command.
    ' ====================================================================

    Public Sub HandleCommandReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetCommandEventArgs)

        _writer.WriteLine("Server: Received command " + e.Command.ToString())

    End Sub

    ' ====================================================================
    ' HandleDoReceived
    ' --------------------------------------------------------------------
    ' Executed when the server receives a DO command from the client.
    ' ====================================================================

    Public Sub HandleDoReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs)

        _writer.WriteLine("Server: Received DO " & e.Option.ToString())

        Select Case e.Option

            Case TelnetOption.Echo
                e.Agreed = True
                _echoed = True

        End Select

    End Sub

    ' ====================================================================
    ' HandleDontReceived
    ' --------------------------------------------------------------------
    ' Executed when the server receives a DONT command from the client.
    ' ====================================================================

    Public Sub HandleDontReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs)

        _writer.WriteLine("Server: Received DONT " & e.Option.ToString())

        Select Case e.Option

            Case TelnetOption.Echo
                e.Agreed = True
                _echoed = False

        End Select

    End Sub

    ' ====================================================================
    ' HandleWillReceived
    ' --------------------------------------------------------------------
    ' Executed when the server receives a WILL command from the client.
    ' ====================================================================

    Public Sub HandleWillReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs)

        _writer.WriteLine("Server: Received WILL " & e.Option.ToString())

        Select Case e.Option

            Case TelnetOption.Echo
                e.Agreed = False

            Case TelnetOption.SuppressGoAhead
                e.Agreed = True

        End Select

    End Sub

    ' ====================================================================
    ' HandleWontReceived
    ' --------------------------------------------------------------------
    ' Executed when the server receives a WONT command from the client.
    ' ====================================================================

    Public Sub HandleWontReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs)

        _writer.WriteLine("Server: Received WONT " & e.Option.ToString())

        Select Case e.Option

            Case TelnetOption.Echo
                e.Agreed = True

        End Select

    End Sub

    ' ====================================================================
    ' PromptCommand
    ' ====================================================================

    Private Function PromptCommand() As String

        _writer.WriteLine("Enter a TELNET command (AO, AYT, GA, NOP, etc).")

        Dim builder As New StringBuilder

        Do

            Dim buffer(1) As Char
            Dim count As Integer = _reader.ReadBlock(buffer, 0, 1)

            If count = -1 Then

                Return String.Empty

            ElseIf buffer(0) = vbCr Then

                If _echoed Then
                    _writer.WriteLine()
                End If

                Return builder.ToString()

            Else

                builder.Append(buffer(0))

                If _echoed Then
                    _writer.Write(buffer(0))
                End If

            End If

        Loop

    End Function

    ' ====================================================================
    ' StartSession 
    ' --------------------------------------------------------------------
    ' This is the starting method for a new thread.  The Main function
    ' creates an instance of this class and then sets this method as the
    ' starting point for a new thread.
    ' ====================================================================

    Friend Sub StartSession()

        ' The core TelnetClient object provides low-level I/O functions.
        ' Create a stream and then a stream reader and stream writer
        ' in order to have more convenient, high-level I/O functions.

        _stream = New TelnetStream(_client)
        _reader = New StreamReader(_stream)
        _writer = New StreamWriter(_stream)

        ' The .NET stream writer buffers output by default; pending
        ' output is flushed once it builds to a certain size or
        ' is manually flushed by the programmer.  This makes sense
        ' when writing to a file because it is optimal to write disk
        ' data in larger blocks.  However, a telnet connection is
        ' interactive and the remote user will expect less latency.
        ' The AutoFlush option tells the writer to flush any pending
        ' output immediately.

        _writer.AutoFlush = True

        ' The Telnet protocol is very different then command-oriented
        ' protocols like POP3 or SMTP.  In those protocols, a client
        ' sends one command at a time to the server, which then executes
        ' the command and sends back a response.
        '
        ' Telnet is isometric.  Each side of the connection can send
        ' commands and receive responses.  Both sides are on an equal
        ' level at the protocol level.  There is no "server" or "client"
        ' at the Telnet protocol level.  It does not matter to Telnet
        ' who initiated the connection.
        '
        ' Obviously there is a "server" -- but only because the developer
        ' is creating a server application at a higher level.  At the
        ' protocol level, both sides of the connection are equal peers that
        ' can send and receive commands.
        '
        ' The core of the Telnet protocol is option negotiation.  An option
        ' represents some feature, such as the ability send the monitor
        ' size to the remote system (which might be useful so that system
        ' can adjust its output).  There are many different options and
        ' not all of them are implemented in all Telnet programs.  The
        ' negotiation mechanism allows both systems to determine the exact
        ' capabilities (i.e. which options are supported) by the other.
        '
        ' A system sends a "DO" code when it wants the other system to 
        ' perform some option.  It sends a DONT code when it wants the other
        ' system to stop.  Similar to these, a system sends a "WILL" code
        ' to indicate it is willing to perform an option, and a "WONT" code
        ' to indicate refusal.

        AddHandler _client.CommandReceived, AddressOf HandleCommandReceived
        AddHandler _client.DontReceived, AddressOf HandleDontReceived
        AddHandler _client.DoReceived, AddressOf HandleDoReceived
        AddHandler _client.WillReceived, AddressOf HandleWillReceived
        AddHandler _client.WontReceived, AddressOf HandleWontReceived

        Dim command As String = Nothing

        Try

            _writer.WriteLine("Server: Sending WILL ECHO...")
            _client.SendWill(TelnetOption.Echo)

            _writer.WriteLine("Server: Sending DO SUPPRESS-GO-AHEAD...")
            _client.SendDo(TelnetOption.SuppressGoAhead)
            _writer.WriteLine("Press ENTER twice to exit.")

            Do

                command = PromptCommand()

                If Not String.IsNullOrEmpty(command) Then
                    Execute(command)
                End If

            Loop While Not String.IsNullOrEmpty(command)

        Catch ex As Exception

            ' Ignore any exceptions and exit the session.

            Program.Log("Error: " & ex.Message, ConsoleColor.Red)

        End Try

        _client.Close()

        RemoveHandler _client.CommandReceived, AddressOf HandleCommandReceived
        RemoveHandler _client.DontReceived, AddressOf HandleDontReceived
        RemoveHandler _client.DoReceived, AddressOf HandleDoReceived
        RemoveHandler _client.WillReceived, AddressOf HandleWillReceived
        RemoveHandler _client.WontReceived, AddressOf HandleWontReceived

    End Sub

End Class
