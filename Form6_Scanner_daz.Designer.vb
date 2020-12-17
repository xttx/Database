<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form6_Scanner_daz
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
		Me.RadioButton1 = New System.Windows.Forms.RadioButton()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.TextBox2 = New System.Windows.Forms.TextBox()
		Me.RadioButton2 = New System.Windows.Forms.RadioButton()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.CheckBox1 = New System.Windows.Forms.CheckBox()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ComboBox1 = New System.Windows.Forms.ComboBox()
		Me.TextBox3 = New System.Windows.Forms.TextBox()
		Me.RadioButton3 = New System.Windows.Forms.RadioButton()
		Me.CheckBox2 = New System.Windows.Forms.CheckBox()
		Me.CheckBox3 = New System.Windows.Forms.CheckBox()
		Me.CheckBox4 = New System.Windows.Forms.CheckBox()
		Me.CheckBox5 = New System.Windows.Forms.CheckBox()
		Me.SuspendLayout()
		'
		'RadioButton1
		'
		Me.RadioButton1.AutoSize = True
		Me.RadioButton1.Checked = True
		Me.RadioButton1.Location = New System.Drawing.Point(12, 22)
		Me.RadioButton1.Name = "RadioButton1"
		Me.RadioButton1.Size = New System.Drawing.Size(74, 17)
		Me.RadioButton1.TabIndex = 0
		Me.RadioButton1.TabStop = True
		Me.RadioButton1.Text = "Scan Files"
		Me.RadioButton1.UseVisualStyleBackColor = True
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(119, 21)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(290, 20)
		Me.TextBox1.TabIndex = 1
		Me.TextBox1.Text = "C:\"
		'
		'TextBox2
		'
		Me.TextBox2.Location = New System.Drawing.Point(119, 47)
		Me.TextBox2.Name = "TextBox2"
		Me.TextBox2.Size = New System.Drawing.Size(290, 20)
		Me.TextBox2.TabIndex = 2
		Me.TextBox2.Text = "C:\MyProg\dir daz3d.txt"
		'
		'RadioButton2
		'
		Me.RadioButton2.AutoSize = True
		Me.RadioButton2.Location = New System.Drawing.Point(12, 48)
		Me.RadioButton2.Name = "RadioButton2"
		Me.RadioButton2.Size = New System.Drawing.Size(90, 17)
		Me.RadioButton2.TabIndex = 3
		Me.RadioButton2.Text = "Scan From txt"
		Me.RadioButton2.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 132)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(39, 13)
		Me.Label1.TabIndex = 4
		Me.Label1.Text = "Label1"
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(584, 153)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(71, 41)
		Me.Button1.TabIndex = 5
		Me.Button1.Text = "Scan"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 160)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(39, 13)
		Me.Label2.TabIndex = 6
		Me.Label2.Text = "Label2"
		'
		'CheckBox1
		'
		Me.CheckBox1.AutoSize = True
		Me.CheckBox1.Location = New System.Drawing.Point(349, 104)
		Me.CheckBox1.Name = "CheckBox1"
		Me.CheckBox1.Size = New System.Drawing.Size(102, 17)
		Me.CheckBox1.TabIndex = 7
		Me.CheckBox1.Text = "Reset Have List"
		Me.CheckBox1.UseVisualStyleBackColor = True
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(11, 105)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(105, 13)
		Me.Label3.TabIndex = 8
		Me.Label3.Text = "Have/Installed Field:"
		'
		'ComboBox1
		'
		Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox1.FormattingEnabled = True
		Me.ComboBox1.Items.AddRange(New Object() {"---Ignore---"})
		Me.ComboBox1.Location = New System.Drawing.Point(119, 102)
		Me.ComboBox1.Name = "ComboBox1"
		Me.ComboBox1.Size = New System.Drawing.Size(148, 21)
		Me.ComboBox1.TabIndex = 9
		'
		'TextBox3
		'
		Me.TextBox3.Location = New System.Drawing.Point(119, 73)
		Me.TextBox3.Name = "TextBox3"
		Me.TextBox3.Size = New System.Drawing.Size(290, 20)
		Me.TextBox3.TabIndex = 10
		Me.TextBox3.Text = "E:\Sandboxes\3d_v3\drive\C\Program Files\Daz Library\"
		'
		'RadioButton3
		'
		Me.RadioButton3.AutoSize = True
		Me.RadioButton3.Location = New System.Drawing.Point(12, 74)
		Me.RadioButton3.Name = "RadioButton3"
		Me.RadioButton3.Size = New System.Drawing.Size(84, 17)
		Me.RadioButton3.TabIndex = 11
		Me.RadioButton3.Text = "Scan Library"
		Me.RadioButton3.UseVisualStyleBackColor = True
		'
		'CheckBox2
		'
		Me.CheckBox2.AutoSize = True
		Me.CheckBox2.Checked = True
		Me.CheckBox2.CheckState = System.Windows.Forms.CheckState.Checked
		Me.CheckBox2.Enabled = False
		Me.CheckBox2.Location = New System.Drawing.Point(349, 173)
		Me.CheckBox2.Name = "CheckBox2"
		Me.CheckBox2.Size = New System.Drawing.Size(204, 17)
		Me.CheckBox2.TabIndex = 12
		Me.CheckBox2.Text = "Get Incomplete Installation Info (Slow)"
		Me.CheckBox2.UseVisualStyleBackColor = True
		'
		'CheckBox3
		'
		Me.CheckBox3.AutoSize = True
		Me.CheckBox3.Checked = True
		Me.CheckBox3.CheckState = System.Windows.Forms.CheckState.Checked
		Me.CheckBox3.Enabled = False
		Me.CheckBox3.Location = New System.Drawing.Point(415, 75)
		Me.CheckBox3.Name = "CheckBox3"
		Me.CheckBox3.Size = New System.Drawing.Size(96, 17)
		Me.CheckBox3.TabIndex = 13
		Me.CheckBox3.Text = "Skip moveable"
		Me.CheckBox3.UseVisualStyleBackColor = True
		'
		'CheckBox4
		'
		Me.CheckBox4.AutoSize = True
		Me.CheckBox4.Location = New System.Drawing.Point(349, 127)
		Me.CheckBox4.Name = "CheckBox4"
		Me.CheckBox4.Size = New System.Drawing.Size(114, 17)
		Me.CheckBox4.TabIndex = 14
		Me.CheckBox4.Text = "Reset Paths Table"
		Me.CheckBox4.UseVisualStyleBackColor = True
		'
		'CheckBox5
		'
		Me.CheckBox5.AutoSize = True
		Me.CheckBox5.Location = New System.Drawing.Point(349, 150)
		Me.CheckBox5.Name = "CheckBox5"
		Me.CheckBox5.Size = New System.Drawing.Size(197, 17)
		Me.CheckBox5.TabIndex = 15
		Me.CheckBox5.Text = "Reset Unrecognized/Double Tables"
		Me.CheckBox5.UseVisualStyleBackColor = True
		'
		'Form6_Scanner_daz
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(667, 206)
		Me.Controls.Add(Me.CheckBox5)
		Me.Controls.Add(Me.CheckBox4)
		Me.Controls.Add(Me.CheckBox3)
		Me.Controls.Add(Me.CheckBox2)
		Me.Controls.Add(Me.RadioButton3)
		Me.Controls.Add(Me.TextBox3)
		Me.Controls.Add(Me.ComboBox1)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.CheckBox1)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.RadioButton2)
		Me.Controls.Add(Me.TextBox2)
		Me.Controls.Add(Me.TextBox1)
		Me.Controls.Add(Me.RadioButton1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "Form6_Scanner_daz"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Scanner - Daz3D Content"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents RadioButton2 As System.Windows.Forms.RadioButton
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents Label3 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents TextBox3 As TextBox
    Friend WithEvents RadioButton3 As RadioButton
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents CheckBox4 As CheckBox
	Friend WithEvents CheckBox5 As CheckBox
End Class
