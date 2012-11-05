
Option Explicit On
Option Strict On

''' <summary>
'''     Base class for quote data sources.
''' </summary>
Public MustInherit Class Quote

    ''' <summary>
    '''     Returns a random quote.
    ''' </summary>
    Public MustOverride Function GetQuote() As String

End Class
