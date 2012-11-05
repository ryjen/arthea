
Option Explicit On
Option Strict On

Imports System
Imports System.IO
Imports System.Text

' ========================================================================
' FileQuote
' ========================================================================

''' <summary>
'''     Loads a random quote from a simple text file.
''' </summary>
''' <remarks>
'''     The text file is expected to contain any number of quotes,
'''     each separated with a blank line (essentially, each quote must
'''     be a single paragraph).
''' </remarks>
Public Class FileQuote : Inherits Quote

    ' ====================================================================
    ' ParserState (enum)
    ' ====================================================================

    ''' <summary>
    '''     Defines the possible states of the parsing code
    '''     that extracts a quote from the file.
    ''' </summary>
    Private Enum ParserState
        Initialized
        ReadingJunk
        ReadingWhitespace
    End Enum

    ' ====================================================================
    ' Private Fields
    ' ====================================================================

    Private _path As String

    ' ====================================================================
    ' New (path) Constructor
    ' ====================================================================

    ''' <summary>
    '''     Initializes the class against the specified quote file.
    ''' </summary>
    ''' <param name="path">
    '''     Path to a quote file.
    ''' </param>
    ''' <remarks>
    '''     An exception is raised if the path is not specified
    '''     or does not point to an existing file.  The file is
    '''     not opened and validated for its format until the
    '''     <see cref="GetQuote" /> function is called.
    ''' </remarks>
    Public Sub New(ByVal path As String)

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentNullException("path")
        End If

        If Not File.Exists(path) Then
            Throw New FileNotFoundException()
        End If

        _path = path

    End Sub

    ' ====================================================================
    ' GetQuote
    ' ====================================================================

    ''' <summary>
    '''     Returns a random quote from the text file.
    ''' </summary>
    ''' 
    ''' <remarks>
    '''     
    '''     <para>
    '''         The function will open the file at a random position,
    '''         then scan for the beginning of the next quote after
    '''         the initial starting point.  It will cycle to the 
    '''         beginning of the file if necessary.
    '''     </para>
    ''' 
    ''' </remarks>
    ''' 
    Public Overrides Function GetQuote() As String

        ' Open the quote data file as a file stream.  The file is
        ' opened in read-only mode and also allows other processes
        ' to open the file in read-only mode.

        Dim quoteStream As New FileStream( _
          _path, _
          FileMode.Open, _
          FileAccess.Read, _
          FileShare.Read)

        ' Create a new random number generator.  The generator
        ' is seeded with a timer value if a seed is not
        ' specified in the constructor.

        Dim random As New Random()

        ' Use the generator to get a random starting point.
        ' The Next() method accepts a lower bound (inclusive)
        ' and an upper bound (exclusive).

        Dim startingPoint As Integer = _
          random.Next(0, Convert.ToInt32(quoteStream.Length))

        quoteStream.Seek(startingPoint, SeekOrigin.Begin)

        ' Create a stream reader.  This makes it easier
        ' to read text lines from the file.

        Dim quoteReader As New StreamReader(quoteStream)

        ' Call the SeekNextQuote method, which scans through
        ' the file, looking for the start of the next quote.

        Dim rawLine As String = SeekNextQuote(quoteReader)

        ' At this point, the stream reader is positioned
        ' at the start of the first line of the quote.
        ' The quote can be collected.

        Dim builder As New StringBuilder()

        builder.AppendLine(rawLine)

        Do

            rawLine = quoteReader.ReadLine()

            If rawLine Is Nothing Then

                Exit Do

            ElseIf rawLine.Trim.Length = 0 Then

                Exit Do

            Else

                builder.AppendLine(rawLine)

            End If

        Loop

        quoteReader.Close()

        Return builder.ToString()

    End Function

    ' ====================================================================
    ' SeekNextQuote
    ' ====================================================================

    ''' <summary>
    '''     Locates the next quote givan a stream reader at an arbitrary
    '''     position in a file.  Upon return, the function returns the
    '''     first line of the quote.
    ''' </summary>
    Private Function SeekNextQuote(ByVal reader As StreamReader) As String

        ' First, perform some basic parameter validation EVEN though
        ' this is a private method and the parameters are highly
        ' likely to be OK.  This ensures that problems caused by any
        ' customizations are caught quickly.

        If reader Is Nothing Then
            Throw New ArgumentNullException("reader")
        End If

        ' The quote stream must be readable because each line
        ' will be examined.  It must also be seekable because
        ' the function may need to cycle to the beginning of
        ' the stream (file), and will also position the reader
        ' before returning to the caller.

        If Not reader.BaseStream.CanRead Then
            Throw New IOException("The quote stream must be readable.")
        End If

        If Not reader.BaseStream.CanSeek Then
            Throw New IOException("The quote stream must be seekable.")
        End If

        ' The next step is to loop through the file and locate
        ' the beginning of the next quote.  There are several
        ' possible states:
        '
        '   0 = Unknown
        '   1 = The random position is in a blank line
        '   2 = The random position is in a quote

        Dim state As ParserState = ParserState.ReadingJunk

        Dim hasCycled As Boolean = False
        Dim rawLine As String

        Do

            ' Grab the next line

            rawLine = reader.ReadLine()

            If rawLine Is Nothing Then

                ' The end of the stream was reached.  Jump to
                ' the beginning of the file and look for the quote
                ' there.  The parser state is reset to "whitespace".

                If hasCycled Then

                    ' Huh?  How did the parser get to the
                    ' end of the file twice?

                    Throw New IOException("Unable to find a quote")

                End If

                reader.DiscardBufferedData()
                reader.BaseStream.Seek(0, SeekOrigin.Begin)
                state = ParserState.ReadingWhitespace
                hasCycled = True

            ElseIf rawLine.Trim().Length = 0 Then

                ' This is a blank line.  However, it is only
                ' save to assume it is a TRUE blank line if the
                ' parser has been running for a bit.  There is
                ' a slight chance the random starting position
                ' will be immediately before a CR/LF in the middle
                ' of a quote.

                If state = ParserState.Initialized Then

                    ' It is not safe to assume this a blank line.
                    ' However, anything beyond is safe.

                    state = ParserState.ReadingJunk

                Else

                    ' The previous line was either junk (in the
                    ' middle of an arbitrary quote) or was whitespace.
                    ' Mark as parser as reading whitespace.

                    state = ParserState.ReadingWhitespace

                End If

            Else

                ' The execution reached this point because the
                ' loaded line was non-blank.

                If state = ParserState.Initialized Then

                    ' However, the parser has not yet synchronized
                    ' with the lines in the file.  This is probably
                    ' a partial line (a statistical likelyhood).

                    state = ParserState.ReadingJunk

                ElseIf state = ParserState.ReadingJunk Then

                    ' Still reading junk -- don't change the state.

                Else

                    ' Yes!  Previously, the parser read a blank line
                    ' or cycled from the end of the file.  This means
                    ' the current line is the first line of the file.

                    Return rawLine

                End If

            End If

        Loop

    End Function

End Class
