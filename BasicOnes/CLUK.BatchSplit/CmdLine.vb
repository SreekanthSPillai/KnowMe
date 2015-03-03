'******************************************************************************
'***
'*** Module:    clsCmdLine
'*** Purpose:   Used to parse the command line. Stores each command line
'***            option. Each option may also have a parameter.
'***
'*** Example:   /b:00000001 -f "C:\Program Files\App\File.txt"
'***            Option  Parameter
'***            b       00000001
'***            f       "C:\Program Files\App\File.txt"
'***
'*** (c) Copyright 2006 Kofax Image Products.
'*** All rights reserved.
'***
'******************************************************************************

Option Strict Off
Option Explicit On
Imports VB = Microsoft.VisualBasic
Imports System.Diagnostics
Namespace CLUK.BatchSplit
    Friend Class clsCmdLine

        '*** Member variables
        Public OptSeparators As String '*** Option separators
        Public OptParamSeparators As String '*** Option parameter separators

        Private m_strCmdLine As String '*** Command line string
        Private m_collOptions As Collection '*** Collection of options.
        Private m_collParams As Collection '*** Collection of option parameters.

        '*** Constants
        Private Const DEFAULT_OPT_SEPARATORS As String = "-/"
        Private Const DEFAULT_OPT_PARAM_SEPARATORS As String = ""

        '**************************************************************************
        '*** Property:  CmdLine
        '*** Purpose:   Parses the command line. Initializes the object variables.
        '**************************************************************************	
        Public Property CmdLine() As String
            Get
                CmdLine = m_strCmdLine
            End Get
            Set(ByVal Value As String)
                '*** The string to parse. This is modified
                Dim strCmd As String

                '*** Position within the string
                Dim lOptionStart As Integer

                '*** Starting location of option parameter string
                Dim lParamStart As Integer

                '*** Next option
                Dim strOption As String

                '*** Parameter to the option
                Dim strParam As String

                '*** Initialize class members
                m_collOptions = Nothing
                m_collOptions = New Collection
                m_collParams = Nothing
                m_collParams = New Collection
                m_strCmdLine = Value

                '*** Parse the command line.
                '*** Look for option separators.
                strCmd = Value
                lOptionStart = find_OneOf(strCmd, OptSeparators)

                '*** If no option separators are found, then use the whole command
                '*** line as an option so that it isn't lost.
                If lOptionStart = 0 And Len(strCmd) > 0 Then

                    '*** Add the whole command line as a parameter
                    m_collOptions.Add(strCmd, strCmd)
                    m_collParams.Add("", strCmd)
                End If

                '*** Loop through the options and add them to the collection
                While lOptionStart > 0

                    '*** Look for characters that separate options from 
                    '*** their parameters
                    strOption = parse_String(strCmd, lOptionStart, _
                                    OptParamSeparators, lParamStart)

                    '*** Look for the next option
                    strParam = parse_String(strCmd, lParamStart, OptSeparators, _
                                    lOptionStart)

                    '*** Add the parameter to the collection
                    Debug.Assert(Len(strOption) > 0, "")
                    If Len(strOption) > 0 Then
                        m_collOptions.Add(strOption, strOption)
                        m_collParams.Add(strParam, strOption)
                    End If
                End While
            End Set
        End Property

        '**************************************************************************
        '*** Function:  IsOption
        '*** Purpose:   Determine if the specified option has been set
        '*** Inputs:    strOption - A possible option
        '*** Outputs:   True if the option was specified on the command line
        '**************************************************************************
        Public Function IsOption(ByVal strOption As String) As Boolean
            Dim bRet As Boolean
            Try
                Dim strTempString As String = m_collOptions.Item(UCase(strOption))
                bRet = True
            Catch ex As Exception
                bRet = False
            End Try
            IsOption = bRet
        End Function

        '**************************************************************************
        '*** Function:  GetOptionParameter
        '*** Purpose:   Returns the option parameter for a particular option
        '*** Inputs:    strOption - The option
        '*** Outputs:   Option parameter
        '*** Notes:     Returns blank if the parameter has not been specified.
        '***            Raises if the option has not been specified.
        '***            Options are not case sensitive.
        '**************************************************************************
        Public Function GetOptionParameter(ByVal strOption As String) As String
            GetOptionParameter = m_collParams.Item(UCase(strOption))
        End Function

        '**************************************************************************
        '*** Function:  New
        '*** Purpose:   Initialize the class members from the command line 
        '***            arguments.
        '*** Inputs:    Command()
        '*** Outputs:   None
        '**************************************************************************
        Public Sub New()
            MyBase.New()

            OptSeparators = DEFAULT_OPT_SEPARATORS
            OptParamSeparators = DEFAULT_OPT_PARAM_SEPARATORS
            CmdLine = VB.Command()
        End Sub

        '**************************************************************************
        '*** Function:  find_OneOf
        '*** Purpose:   Searches for the first character in a string that matches 
        '***            any character contained in another string.
        '*** Inputs:    strSrc - The string to search
        '***            strCharSet - The characters to search for
        '*** Outputs:   Returns the position of the first character in strSrc that 
        '***            is also in strCharSet; 0 if there is no match.
        '**************************************************************************
        Private Function find_OneOf(ByVal strSrc As String, _
        ByVal strCharSet As String) As Integer
            Dim lPos As Integer '*** Used to walk through the source string

            For lPos = 1 To Len(strSrc)
                If InStr(strCharSet, Mid(strSrc, lPos, 1)) > 0 Then

                    '*** The character is one we were looking for
                    find_OneOf = lPos
                    Exit For
                End If
            Next
        End Function

        '**************************************************************************
        '*** Function:  parse_String
        '*** Purpose:   Returns a parameter from the string and removes that string
        '***            from the source string.
        '*** Inputs:    strSrc - Source string
        '***            lStart - Position that current substring starts at
        '***            strCharSet - Characters that identify the next substring
        '*** Outputs:   Returns the next string as separated by strCharSet
        '***            lNextStart - Position that current substring starts at
        '**************************************************************************
        Private Function parse_String(ByRef strSrc As String, _
        ByVal lStart As Integer, ByVal strCharSet As String, _
        ByRef lNextStart As Integer) As Object
            Dim lLen As Integer

            '*** Trim the string as each part is processed
            If lStart > 0 Then
                strSrc = Mid(strSrc, 1 + lStart)
            Else
                strSrc = ""
            End If

            '*** Find starting location of next string
            If Len(strCharSet) = 0 Then
                '*** If no separators are specified, ASSUME a single
                '*** character string is desired.
                If Len(strSrc) > 0 Then
                    lNextStart = 1
                Else
                    lNextStart = 0
                End If
                lLen = lNextStart
            Else
                lNextStart = find_OneOf(strSrc, strCharSet)
                If lNextStart > 0 Then
                    lLen = lNextStart - 1
                Else
                    lLen = Len(strSrc)
                End If
            End If

            '*** Trim the result
            parse_String = Trim(Mid(strSrc, 1, lLen))
        End Function
    End Class
End Namespace