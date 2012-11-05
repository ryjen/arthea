
Option Explicit On
Option Strict On

Imports Thought.Terminals
Imports Thought.Terminals.Translators

Module QuoteProgram

    Sub Main()

        ' Create a new system terminal and add translators for
        ' ANSI X3.64 escape sequences and PCBoard BBS color codes.
        ' The PCBoard color codes are human-readable, friendly
        ' ways of changing a color

        Dim terminal As New SystemTerminal()

        terminal.Translators.Add(New AnsiTranslator)
        terminal.Translators.Add(New PCBoardColorTranslator)

        ' A real Quote of the Day application would not
        ' prompt the user like this -- however, this sample
        ' application is intended to demonstrate a few of
        ' the prompts the terminal/console library.

        If terminal.PromptHotString("Would you like to E&xit or &View a quote?") = "x"c Then
            End
        End If

        ' Define the settings for the boolean prompt.  The
        ' prompt has many options and a few overloads, but
        ' for maximum control you can create a settings objects
        ' to access all of the properties (including ones not
        ' accessible through arguments in one of the overloads).

        Dim settings As New Terminal.BooleanPromptSettings
        With settings
            .BracketStyle = CommonStyles.BrightWhiteOnBlack()
            .FalseCharacter = "N"c
            .FalseWord = "No"
            .LeftBracket = "["c
            .RightBracket = "]"c
            .Separator = "/"c
            .SeparatorStyle = CommonStyles.DarkGrayOnBlack()
            .TrueCharacter = "Y"c
            .TrueWord = "Yes"
            .WriteMenu = True
            .WriteNewLine = True
            .WriteWord = True
        End With

        Dim quoteFile As New FileQuote("Quotes.txt")

        Do

            ' Write the header.  The @XX@ are special color
            ' codes that allow you to change the current color
            ' using its hexadecimal CGA encoding.

            terminal.Translate( _
                "@08@-@07@-@0F@-@07@ Quote of the day: @0F@-@07@-@08@-@07@")

            ' Load the next quote from the file and display
            ' it on the screen.  The contents of the quote are
            ' not translated (e.g. no embedded macros or color
            ' codes are supported).  You can enable support by
            ' calling the Translate method instead of the Write 
            ' method.

            Dim quote As String = quoteFile.GetQuote()

            terminal.WriteLine()
            terminal.Write(quote)
            terminal.WriteLine()

            If Not terminal.PromptBoolean("Show another?", False, settings) Then
                Exit Do
            End If

        Loop


    End Sub

End Module
