Namespace CLUK.BatchSplit
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class frmMain
#Region "Windows Form Designer generated code "

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
            If Disposing Then
                If Not components Is Nothing Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(Disposing)
        End Sub
        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer
        Public ToolTip1 As System.Windows.Forms.ToolTip
        Public WithEvents tmrMainTimer As System.Windows.Forms.Timer
        Public WithEvents lblBatchname As System.Windows.Forms.Label
        Public WithEvents lblBatch As System.Windows.Forms.Label
        Public WithEvents frmStatus As System.Windows.Forms.GroupBox
        Public WithEvents lblFeature As System.Windows.Forms.Label
        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            Me.tmrMainTimer = New System.Windows.Forms.Timer(Me.components)
            Me.frmStatus = New System.Windows.Forms.GroupBox()
            Me.lblBatchname = New System.Windows.Forms.Label()
            Me.lblBatch = New System.Windows.Forms.Label()
            Me.lblFeature = New System.Windows.Forms.Label()
            Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
            Me.frmStatus.SuspendLayout()
            Me.SuspendLayout()
            '
            'ToolTip1
            '
            Me.ToolTip1.IsBalloon = True
            Me.ToolTip1.ShowAlways = True
            Me.ToolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
            Me.ToolTip1.ToolTipTitle = "Batch Split"
            '
            'tmrMainTimer
            '
            Me.tmrMainTimer.Interval = 5000
            '
            'frmStatus
            '
            Me.frmStatus.BackColor = System.Drawing.SystemColors.Control
            Me.frmStatus.Controls.Add(Me.lblBatchname)
            Me.frmStatus.Controls.Add(Me.lblBatch)
            Me.frmStatus.FlatStyle = System.Windows.Forms.FlatStyle.System
            resources.ApplyResources(Me.frmStatus, "frmStatus")
            Me.frmStatus.ForeColor = System.Drawing.SystemColors.ControlText
            Me.frmStatus.Name = "frmStatus"
            Me.frmStatus.TabStop = False
            '
            'lblBatchname
            '
            Me.lblBatchname.BackColor = System.Drawing.SystemColors.Control
            Me.lblBatchname.Cursor = System.Windows.Forms.Cursors.Default
            Me.lblBatchname.FlatStyle = System.Windows.Forms.FlatStyle.System
            resources.ApplyResources(Me.lblBatchname, "lblBatchname")
            Me.lblBatchname.ForeColor = System.Drawing.SystemColors.ControlText
            Me.lblBatchname.Name = "lblBatchname"
            '
            'lblBatch
            '
            Me.lblBatch.BackColor = System.Drawing.SystemColors.Control
            Me.lblBatch.Cursor = System.Windows.Forms.Cursors.Default
            Me.lblBatch.FlatStyle = System.Windows.Forms.FlatStyle.System
            resources.ApplyResources(Me.lblBatch, "lblBatch")
            Me.lblBatch.ForeColor = System.Drawing.SystemColors.ControlText
            Me.lblBatch.Name = "lblBatch"
            '
            'lblFeature
            '
            Me.lblFeature.BackColor = System.Drawing.SystemColors.Control
            Me.lblFeature.Cursor = System.Windows.Forms.Cursors.Default
            Me.lblFeature.FlatStyle = System.Windows.Forms.FlatStyle.System
            resources.ApplyResources(Me.lblFeature, "lblFeature")
            Me.lblFeature.ForeColor = System.Drawing.SystemColors.ControlText
            Me.lblFeature.Name = "lblFeature"
            '
            'NotifyIcon1
            '
            Me.NotifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info
            resources.ApplyResources(Me.NotifyIcon1, "NotifyIcon1")
            '
            'frmMain
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.BackColor = System.Drawing.SystemColors.Control
            Me.Controls.Add(Me.frmStatus)
            Me.Controls.Add(Me.lblFeature)
            Me.Cursor = System.Windows.Forms.Cursors.Default
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.Name = "frmMain"
            Me.frmStatus.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents NotifyIcon1 As System.Windows.Forms.NotifyIcon
#End Region
    End Class

End Namespace