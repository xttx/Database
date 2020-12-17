<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormD_ConvertColumn
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
		Me.Label1 = New System.Windows.Forms.Label()
		Me.ComboBox1 = New System.Windows.Forms.ComboBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.ComboBox2 = New System.Windows.Forms.ComboBox()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(24, 19)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(96, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Column to convert:"
		'
		'ComboBox1
		'
		Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox1.FormattingEnabled = True
		Me.ComboBox1.Location = New System.Drawing.Point(27, 35)
		Me.ComboBox1.Name = "ComboBox1"
		Me.ComboBox1.Size = New System.Drawing.Size(189, 21)
		Me.ComboBox1.TabIndex = 1
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(320, 19)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(108, 13)
		Me.Label2.TabIndex = 2
		Me.Label2.Text = "Column to convert to:"
		'
		'ComboBox2
		'
		Me.ComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox2.FormattingEnabled = True
		Me.ComboBox2.Location = New System.Drawing.Point(323, 35)
		Me.ComboBox2.Name = "ComboBox2"
		Me.ComboBox2.Size = New System.Drawing.Size(189, 21)
		Me.ComboBox2.TabIndex = 3
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(24, 82)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(154, 13)
		Me.Label3.TabIndex = 4
		Me.Label3.Text = "Default (used on convert error):"
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(186, 79)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(123, 20)
		Me.TextBox1.TabIndex = 5
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(24, 104)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(153, 13)
		Me.Label4.TabIndex = 6
		Me.Label4.Text = "Leave empty, to just skip errors"
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
		Me.Label5.Location = New System.Drawing.Point(250, 30)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(33, 24)
		Me.Label5.TabIndex = 7
		Me.Label5.Text = "-->"
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(486, 104)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(91, 30)
		Me.Button1.TabIndex = 8
		Me.Button1.Text = "CONVERT"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'FormD_ConvertColumn
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(589, 146)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.Label5)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.TextBox1)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.ComboBox2)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.ComboBox1)
		Me.Controls.Add(Me.Label1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.Name = "FormD_ConvertColumn"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Convert STR Column"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents Label1 As Label
	Friend WithEvents ComboBox1 As ComboBox
	Friend WithEvents Label2 As Label
	Friend WithEvents ComboBox2 As ComboBox
	Friend WithEvents Label3 As Label
	Friend WithEvents TextBox1 As TextBox
	Friend WithEvents Label4 As Label
	Friend WithEvents Label5 As Label
	Friend WithEvents Button1 As Button
End Class
