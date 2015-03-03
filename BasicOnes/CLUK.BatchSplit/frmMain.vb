'********************************************************************************
'***
'*** Module:    frmMain.vb - frmMain
'*** Purpose:   Main Form
'***
'*** (c) Copyright 2000-2013 Kofax Image Products.
'*** All rights reserved.
'***
'********************************************************************************
Option Strict Off

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic
Imports System.Threading
Imports Kofax.Capture.SDK.CustomModule

Namespace CLUK.BatchSplit
    Friend Class frmMain
        Inherits System.Windows.Forms.Form

        ' AutoSignOut feature: define this window message to process timeout session
        Private Const WM_QUERYENDSESSION As Integer = &H11
        '********************************************************************************
        '***
        '*** Module:    Sample Separation Custom Module
        '*** Purpose:   Main application form
        '***
        '*** (c) Copyright 2003 Kofax Image Products.
        '*** All rights reserved.
        '***
        '********************************************************************************
        Public Sub New()

            'Set UI Culture
            Kofax.SDK.CaptureInfo.CaptureInfo.SetThreadUILanguage(Thread.CurrentThread)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        '*** Class members
        Private m_oBatMan As BatchManager
        Private WithEvents m_oProcessor As IProcessor
        Private m_lBatchId As Integer '***  Batch ID of a batch to process

        Dim mAllowVisible As Boolean
        Dim mAllowClose As Boolean
        Dim mLoadFired As Boolean



        '********************************************************************************
        '*** Sub:     PollingProcessing()
        '*** Purpose: Get all available batch(es), and take one of them to process
        '*** Input:   None
        '*** Output:  None
        '********************************************************************************
        '*** This routine is called every 5 seconds by a timer.

        Private Sub PollingProcessing()

            '** CLear the status text
            lblBatchname.Text = ""

            Dim bBatchOpened As Boolean
            If m_lBatchId > 0 Then
                bBatchOpened = m_oBatMan.BatchOpen( _
                                m_lBatchId, _
                                BatchManager.bmProcessMode.bmProcessByBatch)
            Else
                bBatchOpened = m_oBatMan.BatchOpenNext( _
                                BatchManager.bmProcessMode.bmProcessByBatch, _
                                False)
            End If

            If bBatchOpened And m_oBatMan.ActiveBatch IsNot Nothing Then
                '*** Set label with the active batch name
                lblBatchname.Text = m_oBatMan.ActiveBatch.Name

                Dim lErr As Integer = 0
                Dim strErrMsg As String = ""
                Dim eCloseState As Kofax.Capture.SDK.CustomModule.KfxDbState = Kofax.Capture.SDK.CustomModule.KfxDbState.KfxDbBatchReady

                '*** Process runtime information on the active batch
                Try
                    m_oProcessor.Process(m_oBatMan.ActiveBatch)
                Catch ex As Exception
                    '*** Log and show the error
                    Error_Msgbox("PollingProcessing", ex.Message)

                    '*** Save the error for later used
                    strErrMsg = ex.Message
                    If TypeOf ex Is COMException Then
                        Dim exCOM As COMException = DirectCast(ex, COMException)
                        lErr = exCOM.ErrorCode
                    Else
                        Const c_ErrProcess As Integer = 1001
                        lErr = c_ErrProcess
                    End If

                    eCloseState = Kofax.Capture.SDK.CustomModule.KfxDbState.KfxDbBatchError
                End Try

                Try
                    m_oBatMan.BatchClose(eCloseState, lErr, strErrMsg, False)
                Catch ex As Exception
                    '*** Log and show the error
                    Error_Msgbox("PollingProcessing", ex.Message)
                End Try
            Else
                '*** no batches are available
                lblBatchname.Text = My.Resources.NoBatchesAvailable
            End If

            If m_lBatchId > 0 And bBatchOpened Then
                Me.Close()
            End If
        End Sub

        '********************************************************************************
        '*** Sub:       FormClosed
        '*** Purpose:   This function is called when the form is closing down. This allows
        '***            us to release our com objects and logout of Ascent Capture and
        '***            do house cleaning tasks
        '*** Input:     None
        '*** Output:    None
        '********************************************************************************
        Protected Overrides Sub OnFormClosed( _
            ByVal e As System.Windows.Forms.FormClosedEventArgs)

            MyBase.OnFormClosed(e)

            '*** we don't need the timer anymore so shut it down
            tmrMainTimer.Stop()

            Try
                '*** clean up com objects and log out of AC runtime session
                RemoveHandler m_oBatMan.OnSessionTimeOut, AddressOf ProcessTimeOutSession
                m_oBatMan.ReleaseComObjects()
            Catch ex As Exception
                '*** We could fail here if the database connection is lost.
                '*** We would like to log an error in such case, but as we're
                '*** already at the exit point we're out of tools to do so.
            End Try
        End Sub

        '********************************************************************************
        '*** Sub:     OnLoad()
        '*** Purpose: Initialize batch manager object, and
        '***          set interval for timer
        '*** Input:   None
        '*** Output:  None
        '********************************************************************************
        Protected Overrides Sub OnLoad( _
            ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            Application.EnableVisualStyles()

            Try
                '*** Initialize the batch manager class
                m_oProcessor = New Processor()
                m_oBatMan = New BatchManager(m_oProcessor.CustomModuleID)

                '*** Log on to the runtime session before setting the export paths
                '*** If SecurtyBoost is enabled, the user must be logged on prior to
                '*** creating the folder.
                m_oBatMan.LoginToRuntimeSession()

                UpdateUI()

                ' AutoSignOut feature
                ' Handling timeout warning message, if there isn't handle, default message will be shown

                AddHandler m_oBatMan.OnSessionTimeOut, AddressOf ProcessTimeOutSession
                ' Assign form handle, that will process WM_QUERRYENDSESSION and show warning message
                ' if there isn't handle, monitor will use main handle via Process.MainWindowHandle
                m_oBatMan.AssignHandler(Me.Handle)

                '*** Process command line options
                Dim oCmdLine As New clsCmdLine
                If oCmdLine.IsOption("B") = True Then

                    '*** Set XML files names
                    m_oBatMan.XmlRuntimeExportFile = "RtExport.xml"
                    m_oBatMan.XmlRuntimeImportFile = "RtImport.xml"

                    '*** Get the specified BatchID
                    m_lBatchId = Val(oCmdLine.GetOptionParameter("B"))

                    '*** Set a shorter wait interval so that the batch will be
                    '*** processed more quickly
                    tmrMainTimer.Interval = 500
                End If

                tmrMainTimer.Enabled = True

                Me.WindowState = FormWindowState.Minimized

                NotifyIcon1.Visible = True
                NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                NotifyIcon1.ShowBalloonTip(500)

            Catch exception As COMException
                If exception.ErrorCode = &H80040FA9 Then ' ERR_DATAREADER_FROM_STORED_PROC
                    MessageBox.Show(My.Resources.NotRegistered, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    MessageBox.Show(exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
                Close()
            End Try
        End Sub


        '********************************************************************************
        '*** Sub:       Error_Msgbox
        '*** Purpose:   Displays an error message box
        '*** Inputs:    strFunction - The function where the error occurred
        '*** Outputs:   None
        '********************************************************************************
        Private Sub Error_Msgbox( _
            ByVal strFunction As String, _
            ByVal strError As String)

            Dim lErr As Integer = Err.Number
            Dim strSource As String = String.Format( _
                "Error #{0} occurred in ({1}) {2}", lErr, strFunction, Err.Source)
            If strError.Length = 0 Then
                strError = Err.Description
            End If

            MsgBox(strSource & vbCrLf & vbCrLf & strError, _
                   MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical)

            Try
                m_oBatMan.LogError(lErr, strSource, Erl(), strError)
            Catch
                '*** Try to log the error but ignore failure if 
                '*** we cannot write to the log file (i.e. network error).
            End Try

        End Sub

        '********************************************************************************
        '*** Sub:       tmrMainTimer_Timer
        '*** Purpose:   Call PollingProcessing and UpdateUI
        '*** Inputs:    none
        '*** Outputs:
        '********************************************************************************
        Private Sub tmrMainTimer_Tick( _
            ByVal eventSender As System.Object, _
            ByVal eventArgs As System.EventArgs) Handles tmrMainTimer.Tick

            Try
                tmrMainTimer.Enabled = False
                PollingProcessing()
                UpdateUI()
            Finally
                tmrMainTimer.Enabled = True
            End Try
        End Sub

        '********************************************************************************
        '*** Sub:       UpdateUI
        '*** Purpose:   Update user interface
        '*** Inputs:    none
        '*** Outputs:   none
        '********************************************************************************
        Private Sub UpdateUI()
            lblFeature.Text = m_oProcessor.ProcessDescription
            Me.Text = m_oProcessor.ProcessCaption

            frmStatus.Text = My.Resources.Status
            lblBatch.Text = My.Resources.BatchName
        End Sub

        Private Sub m_oProcessor_PageStatus( _
            ByVal strStatus As String) _
        Handles m_oProcessor.PageStatus
        End Sub

        Private Sub m_oProcessor_DocumentStatus( _
            ByVal strStatus As String) _
        Handles m_oProcessor.DocumentStatus

        End Sub
        '********************************************************************************
        '*** Sub:       ProcessTimeOutSession
        '*** Purpose:   Process timeout custom warning message
        '*** Inputs:    none
        '*** Outputs:   bShownCustomMsg - if CM show warning message, bShownCustomMsg must
        ' set to TRUE
        '********************************************************************************
        Public Sub ProcessTimeOutSession(arg As TimeoutEventArg)
            MessageBox.Show(String.Format(My.Resources.TimeOutMsg, arg.Expire), Text)
            arg.Processed = True
        End Sub

        Protected Overrides Sub WndProc(ByRef recWinMessage As Message)
            If recWinMessage.Msg = WM_QUERYENDSESSION Then

                tmrMainTimer.Stop()

                Try
                    '*** clean up com objects and log out of AC runtime session
                    RemoveHandler m_oBatMan.OnSessionTimeOut, AddressOf ProcessTimeOutSession
                    m_oBatMan.ReleaseComObjects()
                Catch ex As Exception
                    '*** We could fail here if the database connection is lost.
                    '*** We would like to log an error in such case, but as we're
                    '*** already at the exit point we're out of tools to do so.
                End Try

                recWinMessage.Result = True
            End If
            MyBase.WndProc(recWinMessage)
        End Sub



        Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                NotifyIcon1.Visible = True
                NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                NotifyIcon1.ShowBalloonTip(500)
                Me.WindowState = FormWindowState.Minimized

            End If
        End Sub

        Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load

        End Sub
    End Class

End Namespace