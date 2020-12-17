<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormF_FailedTrimmedEntries
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.ListBox1 = New System.Windows.Forms.ListBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.FlowLayoutPanel1 = New Catalog_2016.FlowLayoutPanelEx()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'ListBox1
		'
		Me.ListBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.ListBox1.FormattingEnabled = True
		Me.ListBox1.IntegralHeight = False
		Me.ListBox1.Location = New System.Drawing.Point(12, 12)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.Size = New System.Drawing.Size(291, 403)
		Me.ListBox1.TabIndex = 0
		'
		'Label1
		'
		Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 418)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(38, 13)
		Me.Label1.TabIndex = 1
		Me.Label1.Text = "Count:"
		'
		'Button1
		'
		Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button1.Location = New System.Drawing.Point(463, 435)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(376, 24)
		Me.Button1.TabIndex = 3
		Me.Button1.Text = "Copy all of bigger resolution, or bigger size (if res is the same)"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'Label2
		'
		Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 441)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(422, 13)
		Me.Label2.TabIndex = 4
		Me.Label2.Text = "Green border = same ext, res, size. Red border = same file (trimmed by GetScreen(" &
	") func)"
		'
		'FlowLayoutPanel1
		'
		Me.FlowLayoutPanel1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.FlowLayoutPanel1.AutoScroll = True
		Me.FlowLayoutPanel1.Location = New System.Drawing.Point(309, 12)
		Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
		Me.FlowLayoutPanel1.Size = New System.Drawing.Size(530, 403)
		Me.FlowLayoutPanel1.TabIndex = 2
		Me.FlowLayoutPanel1.WrapContents = False
		'
		'Label3
		'
		Me.Label3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(155, 418)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(49, 13)
		Me.Label3.TabIndex = 5
		Me.Label3.Text = "Screens:"
		'
		'FormF_FailedTrimmedEntries
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(851, 468)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.FlowLayoutPanel1)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.ListBox1)
		Me.Controls.Add(Me.Label2)
		Me.DoubleBuffered = True
		Me.Name = "FormF_FailedTrimmedEntries"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
		Me.Text = "Failed Trimmed Entries"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents ListBox1 As ListBox
	Friend WithEvents Label1 As Label
	'Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
	Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanelEx
	Friend WithEvents Button1 As Button
	Friend WithEvents Label2 As Label
	Friend WithEvents Label3 As Label
End Class
