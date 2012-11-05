
Option Explicit On
Option Strict On

Imports System
Imports Thought.Terminals

''' <summary>
'''     Demonstrates a simple login prompt.
''' </summary>
Module Program

    Public Const MaximumAttempts As Integer = 3
    Public Const ValidUserName As String = "username"
    Public Const ValidPassword As String = "password"


    Sub Main()

        Dim attempts As Integer = 0
        Dim authenticated As Boolean = False
        Dim username As String
        Dim password As String
        Dim terminal As Terminal = New SystemTerminal

        ' The Terminal.PromptText method has overloaded versions
        ' that do not require you to specify a settings object.
        ' However, the settings object is useful when you want
        ' maximum control over the behavior of the prompt.

        Dim settings As New Terminal.TextPromptSettings
        With settings

            .BackgroundCharacter = "."c
            .BackgroundStyle = CommonStyles.DarkGrayOnBlue()
            .BorderStyle = CommonStyles.DarkGrayOnBlack()
            .EntryStyle = CommonStyles.WhiteOnBlue()
            .FillOut = True
            .LeftBorder = "["
            .Length = 30
            .MaximumEntryLength = 30
            .RightBorder = "]"
            .WriteNewLine = True

        End With

        Do

            ' Count the number of logon attempts.  If the
            ' maximum has been exceeded, display an error
            ' message and abort the logon process.

            attempts += 1
            If attempts > MaximumAttempts Then
                terminal.WriteLine("Maximum attempts exceeded.")
                End
            End If

            username = terminal.PromptText("Username: ", Nothing, settings)
            password = terminal.PromptText("Password: ", Nothing, settings)

            authenticated = _
                (String.Compare(username, ValidUserName, True) = 0) AndAlso _
                (String.Compare(password, ValidPassword, True) = 0)

            If Not authenticated Then
                terminal.WriteLine("Invalid username or password.")
            End If

        Loop While Not authenticated

    End Sub

End Module
