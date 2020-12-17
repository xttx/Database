<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form6_Scanner_Details2
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
		Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.DateTimePicker1 = New System.Windows.Forms.DateTimePicker()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ComboBox1 = New System.Windows.Forms.ComboBox()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.DataGridView1 = New System.Windows.Forms.DataGridView()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.CheckBox1 = New System.Windows.Forms.CheckBox()
		Me.ComboBox2 = New System.Windows.Forms.ComboBox()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.DateTimePicker2 = New System.Windows.Forms.DateTimePicker()
		Me.RadioButton1 = New System.Windows.Forms.RadioButton()
		Me.RadioButton2 = New System.Windows.Forms.RadioButton()
		Me.CheckBox2 = New System.Windows.Forms.CheckBox()
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(209, 12)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(168, 20)
		Me.TextBox1.TabIndex = 0
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 15)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(32, 13)
		Me.Label1.TabIndex = 1
		Me.Label1.Text = "Filter:"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(54, 15)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(149, 13)
		Me.Label2.TabIndex = 2
		Me.Label2.Text = "File (use % and _ as wildcard):"
		'
		'DateTimePicker1
		'
		Me.DateTimePicker1.CustomFormat = "dd.MM.yyyy HH:mm:ss"
		Me.DateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom
		Me.DateTimePicker1.Location = New System.Drawing.Point(494, 9)
		Me.DateTimePicker1.Name = "DateTimePicker1"
		Me.DateTimePicker1.Size = New System.Drawing.Size(141, 20)
		Me.DateTimePicker1.TabIndex = 3
		Me.DateTimePicker1.Value = New Date(2020, 11, 26, 0, 0, 0, 0)
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(394, 15)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(33, 13)
		Me.Label3.TabIndex = 4
		Me.Label3.Text = "Date:"
		'
		'ComboBox1
		'
		Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox1.FormattingEnabled = True
		Me.ComboBox1.Items.AddRange(New Object() {"", "=", "!=", ">", "<", ">=", "<="})
		Me.ComboBox1.Location = New System.Drawing.Point(433, 11)
		Me.ComboBox1.Name = "ComboBox1"
		Me.ComboBox1.Size = New System.Drawing.Size(55, 21)
		Me.ComboBox1.TabIndex = 5
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(791, 12)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(87, 20)
		Me.Button1.TabIndex = 6
		Me.Button1.Text = "Filter"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'DataGridView1
		'
		Me.DataGridView1.AllowUserToAddRows = False
		Me.DataGridView1.AllowUserToDeleteRows = False
		Me.DataGridView1.AllowUserToResizeRows = False
		Me.DataGridView1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.DataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
		Me.DataGridView1.Location = New System.Drawing.Point(12, 65)
		Me.DataGridView1.MultiSelect = False
		Me.DataGridView1.Name = "DataGridView1"
		DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
		DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark
		DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
		DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
		DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
		DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
		DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
		Me.DataGridView1.RowHeadersDefaultCellStyle = DataGridViewCellStyle2
		Me.DataGridView1.RowHeadersVisible = False
		Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.DataGridView1.Size = New System.Drawing.Size(1021, 373)
		Me.DataGridView1.TabIndex = 7
		'
		'Label4
		'
		Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(898, 15)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(40, 13)
		Me.Label4.TabIndex = 8
		Me.Label4.Text = "Rows: "
		'
		'CheckBox1
		'
		Me.CheckBox1.AutoSize = True
		Me.CheckBox1.Location = New System.Drawing.Point(651, 11)
		Me.CheckBox1.Name = "CheckBox1"
		Me.CheckBox1.Size = New System.Drawing.Size(85, 17)
		Me.CheckBox1.TabIndex = 9
		Me.CheckBox1.Text = "Distinct Files"
		Me.CheckBox1.UseVisualStyleBackColor = True
		'
		'ComboBox2
		'
		Me.ComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox2.FormattingEnabled = True
		Me.ComboBox2.Items.AddRange(New Object() {"", "=", "!=", ">", "<", ">=", "<="})
		Me.ComboBox2.Location = New System.Drawing.Point(433, 38)
		Me.ComboBox2.Name = "ComboBox2"
		Me.ComboBox2.Size = New System.Drawing.Size(55, 21)
		Me.ComboBox2.TabIndex = 10
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(394, 41)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(29, 13)
		Me.Label5.TabIndex = 11
		Me.Label5.Text = "And:"
		'
		'DateTimePicker2
		'
		Me.DateTimePicker2.CustomFormat = "dd.MM.yyyy HH:mm:ss"
		Me.DateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom
		Me.DateTimePicker2.Location = New System.Drawing.Point(494, 38)
		Me.DateTimePicker2.Name = "DateTimePicker2"
		Me.DateTimePicker2.Size = New System.Drawing.Size(141, 20)
		Me.DateTimePicker2.TabIndex = 12
		Me.DateTimePicker2.Value = New Date(2020, 11, 26, 0, 0, 0, 0)
		'
		'RadioButton1
		'
		Me.RadioButton1.AutoSize = True
		Me.RadioButton1.Checked = True
		Me.RadioButton1.Location = New System.Drawing.Point(791, 39)
		Me.RadioButton1.Name = "RadioButton1"
		Me.RadioButton1.Size = New System.Drawing.Size(121, 17)
		Me.RadioButton1.TabIndex = 13
		Me.RadioButton1.TabStop = True
		Me.RadioButton1.Text = "Show Unrecognized"
		Me.RadioButton1.UseVisualStyleBackColor = True
		'
		'RadioButton2
		'
		Me.RadioButton2.AutoSize = True
		Me.RadioButton2.Location = New System.Drawing.Point(918, 39)
		Me.RadioButton2.Name = "RadioButton2"
		Me.RadioButton2.Size = New System.Drawing.Size(94, 17)
		Me.RadioButton2.TabIndex = 14
		Me.RadioButton2.Text = "Show Doubles"
		Me.RadioButton2.UseVisualStyleBackColor = True
		'
		'CheckBox2
		'
		Me.CheckBox2.AutoSize = True
		Me.CheckBox2.Location = New System.Drawing.Point(651, 40)
		Me.CheckBox2.Name = "CheckBox2"
		Me.CheckBox2.Size = New System.Drawing.Size(101, 17)
		Me.CheckBox2.TabIndex = 15
		Me.CheckBox2.Text = "Duplicated Files"
		Me.CheckBox2.UseVisualStyleBackColor = True
		'
		'Form6_Scanner_Details2
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(1045, 450)
		Me.Controls.Add(Me.CheckBox2)
		Me.Controls.Add(Me.RadioButton2)
		Me.Controls.Add(Me.RadioButton1)
		Me.Controls.Add(Me.DateTimePicker2)
		Me.Controls.Add(Me.Label5)
		Me.Controls.Add(Me.ComboBox2)
		Me.Controls.Add(Me.CheckBox1)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.DataGridView1)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.ComboBox1)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.DateTimePicker1)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.TextBox1)
		Me.Name = "Form6_Scanner_Details2"
		Me.Text = "Scan Details (files)"
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents TextBox1 As TextBox
	Friend WithEvents Label1 As Label
	Friend WithEvents Label2 As Label
	Friend WithEvents DateTimePicker1 As DateTimePicker
	Friend WithEvents Label3 As Label
	Friend WithEvents ComboBox1 As ComboBox
	Friend WithEvents Button1 As Button
	Friend WithEvents DataGridView1 As DataGridView
	Friend WithEvents Label4 As Label
	Friend WithEvents CheckBox1 As CheckBox
	Friend WithEvents ComboBox2 As ComboBox
	Friend WithEvents Label5 As Label
	Friend WithEvents DateTimePicker2 As DateTimePicker
	Friend WithEvents RadioButton1 As RadioButton
	Friend WithEvents RadioButton2 As RadioButton
	Friend WithEvents CheckBox2 As CheckBox
End Class
