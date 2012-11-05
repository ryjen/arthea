
Option Explicit On
Option Strict On

Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Thought.Net.Telnet

' ========================================================================
' Telnet Client Sample
' ========================================================================

Module Program

    ' ====================================================================
    ' Client
    ' --------------------------------------------------------------------
    ' This is a reference to the TelnetClient class that provides support
    ' for the Telnet API.  The class does not implement any protocol
    ' options (such as ECHO or TRANSMIT-BINARY) although it will raise
    ' events when the remote system requests such options and it will keep
    ' track of the status of any options.
    ' ====================================================================

    Private WithEvents Client As Thought.Net.Telnet.TelnetClient


    ' ====================================================================
    ' _consoleLock
    ' --------------------------------------------------------------------
    ' Used to synchronized writing to the console (e.g. if the user types
    ' a key, the key should not be written until all pending characters
    ' from the server are written).
    ' ====================================================================

    Private _consoleLock As Object = New Object


    ' ====================================================================
    ' _localEcho
    ' --------------------------------------------------------------------
    ' Indicates the client is echoing characters typed locally (default).
    ' The server can indicate it wants to echo characters by using the
    ' WILL ECHO telnet command.  If it does this, the HandleWillReceived
    ' event handler (found below) is fired.  That event handler will
    ' agree to let the server echo characters (and set the _localEcho
    ' variable to false).  This is an example of a simple telnet option
    ' implementation.
    ' ====================================================================

    Private _localEcho As Boolean = True


    ' ====================================================================
    ' _reader, _writer
    ' --------------------------------------------------------------------
    ' Enables reading and writing of strings (Telnet is byte-oriented).
    ' ====================================================================

    Private _reader As StreamReader
    Private _writer As StreamWriter


    ' ====================================================================
    ' Main
    ' ====================================================================

    Sub Main()

        ' This sample application only connects to the server
        ' on the localhost.  Your firewall application may raise
        ' a warning; you will need to allow this program to
        ' access the Internet.

        Console.WriteLine("Local telnet sample.")
        Console.WriteLine("Press Control-C to exit, or ENTER when the server is running.")
        Console.ReadLine()

        ' Create an Internet (IP) socket for the TCP protocol.

        Dim socket As New Socket( _
            AddressFamily.InterNetwork, _
            SocketType.Stream, _
            ProtocolType.Tcp)

        ' Attempt to connect to port 23 (the default Telnet port).
        ' This call will fail if a local telnet server is not running.
        ' No error handling is implemented in this simple example.
        ' In real life you would properly close resources.

        socket.Connect(IPAddress.Loopback, 23)

        ' Create a new instance of the TelnetClient class.

        Client = New Thought.Net.Telnet.TelnetClient(socket)

        ' The TelnetClient class makes it easy to fetch Telnet
        ' commands (and also read/write data).  However, a stream
        ' provides a more agnostic way of reading and writing data.

        Dim stream As New TelnetStream(Client)

        ' The TelnetStream class derives from IO.Stream.  This
        ' means common I/O classes (like StreamReader) can now
        ' be used against the Telnet connection.  The class library
        ' will transparently package the data for transmission
        ' over the Telnet connection (e.g. Telnet IAC characters
        ' are automatically escaped).

        _reader = New StreamReader(stream)
        _writer = New StreamWriter(stream)
        _writer.AutoFlush = True

        ' Create a background thread that waits for incoming
        ' data from the server (and writes it to the screen).

        Dim backgroundThread As New Thread(AddressOf BackgroundOutputThread)
        backgroundThread.Start()

        ' The background thread is now waiting for incoming
        ' characters and displaying them on the screen as they
        ' arrive.  Collect keys from the user and send to the
        ' server as they are typed.

        Do

            Dim key As ConsoleKeyInfo = Console.ReadKey(True)
            If (_localEcho) Then
                UpdateScreen(key.KeyChar.ToString())
            End If

            Try

                _writer.Write(key.KeyChar)

            Catch ex As Exception

                Console.WriteLine("Client: Error writing: " & ex.Message)
                Exit Do

            End Try

        Loop

    End Sub


    ' ====================================================================
    ' BackgroundOutputThread
    ' --------------------------------------------------------------------
    ' This is the background thread that handles characters typed by the
    ' user.  It automatically sends typed characters to the server.
    ' This is not strictly Telnet-compliant because the basic Telnet
    ' terminal is supposed to be line-oriented.  Compliant code will be
    ' implemented in the future.
    ' ====================================================================

    Public Sub BackgroundOutputThread()

        ' Create a buffer to hold incoming characters from
        ' the remote system.

        Dim chars(100) As Char

        Do

            ' Read the next set of characters from the remote
            ' system.  The ReadLine function is not used because
            ' the remote system is not guaranteed to send characters
            ' as single lines!

            Dim totalRead As Integer

            Try

                totalRead = _reader.Read(chars, 0, chars.Length)

            Catch ex As Exception

                UpdateScreen("Client: Error Reading: " & ex.Message & vbCrLf)
                Exit Do

            End Try

            UpdateScreen(New String(chars, 0, totalRead))

        Loop

    End Sub


    ' ====================================================================
    ' HandleCommandReceived
    ' --------------------------------------------------------------------
    ' Raised by the Telnet client when a simple command is received.
    ' ====================================================================

    Private Sub HandleCommandReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetCommandEventArgs) Handles Client.CommandReceived

        UpdateScreen("Client: Received command: " & e.Command.ToString() & vbCrLf)

    End Sub

    ' ====================================================================
    ' HandleDontReceived
    ' --------------------------------------------------------------------
    ' Raised by the Telnet client when a DONT command is received.
    ' ====================================================================

    Private Sub HandleDontReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs) Handles Client.DontReceived

        UpdateScreen("Client: Received DONT " & e.Option.ToString() & vbCrLf)

    End Sub

    ' ====================================================================
    ' HandleDoReceived
    ' --------------------------------------------------------------------
    ' Raised by the Telnet client when a DO command is received.
    ' ====================================================================

    Private Sub HandleDoReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs) Handles Client.DoReceived

        UpdateScreen("Client: Received DO " & e.Option.ToString() & vbCrLf)

    End Sub

    ' ====================================================================
    ' HandleWillReceived
    ' --------------------------------------------------------------------
    ' Raised by the Telnet client when a WILL command is received.
    ' ====================================================================

    Private Sub HandleWillReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs) Handles Client.WillReceived

        UpdateScreen("Client: Received WILL " & e.Option.ToString() & vbCrLf)

        If (e.Option = TelnetOption.Echo) Then

            ' The server sent a WILL ECHO command.  This means the
            ' server is willing to ECHO typed characters (instead of
            ' the local client doing so).  This is a common option 
            ' because the server can then fully control the output
            ' of typed characters.
            '
            ' This client agrees to let the server handle echoing.
            ' Local echoing is turned off.

            e.Agreed = True
            _localEcho = False

        End If

    End Sub

    ' ====================================================================
    ' HandleWontReceived
    ' --------------------------------------------------------------------
    ' Raised by the Telnet client when a WONT command is received.
    ' ====================================================================

    Private Sub HandleWontReceived( _
          ByVal sender As Object, _
          ByVal e As TelnetOptionEventArgs) Handles Client.WontReceived

        UpdateScreen("Client: Received WONT " & e.Option.ToString() & vbCrLf)

        If e.Option = TelnetOption.Echo Then

            ' The server sent a WONT ECHO command.  This means
            ' it refuses to continue echoing typed characters.

            e.Agreed = True
            _localEcho = True

        End If

    End Sub

    ' ====================================================================
    ' UpdateScreen
    ' ====================================================================

    Public Sub UpdateScreen(ByVal text As String)

        SyncLock _consoleLock
            Console.Write(text)
        End SyncLock

    End Sub

End Module
