<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormA_MaintenanceDB
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.DataGridView1 = New System.Windows.Forms.DataGridView()
		Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.CheckBox10 = New System.Windows.Forms.CheckBox()
		Me.Screen_UnCheckAll_Button = New System.Windows.Forms.Button()
		Me.Screen_CheckAll_Button = New System.Windows.Forms.Button()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.CheckBox9 = New System.Windows.Forms.CheckBox()
		Me.Label_Count = New System.Windows.Forms.Label()
		Me.Button_ScreenshotExport = New System.Windows.Forms.Button()
		Me.CheckBox7 = New System.Windows.Forms.CheckBox()
		Me.ComboBox2 = New System.Windows.Forms.ComboBox()
		Me.CheckBox8 = New System.Windows.Forms.CheckBox()
		Me.Button_ScreenshotFix = New System.Windows.Forms.Button()
		Me.Button_ScreenshotCheck = New System.Windows.Forms.Button()
		Me.Screen_Replacement_Button_Remove = New System.Windows.Forms.Button()
		Me.Screen_Replacement_Button_Add = New System.Windows.Forms.Button()
		Me.ComboBox1 = New System.Windows.Forms.ComboBox()
		Me.CheckBox6 = New System.Windows.Forms.CheckBox()
		Me.CheckBox5 = New System.Windows.Forms.CheckBox()
		Me.CheckBox4 = New System.Windows.Forms.CheckBox()
		Me.CheckBox3 = New System.Windows.Forms.CheckBox()
		Me.CheckBox2 = New System.Windows.Forms.CheckBox()
		Me.CheckBox1 = New System.Windows.Forms.CheckBox()
		Me.TabControl1.SuspendLayout()
		Me.TabPage1.SuspendLayout()
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox1.SuspendLayout()
		Me.SuspendLayout()
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TabControl1.Location = New System.Drawing.Point(0, 0)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(898, 454)
		Me.TabControl1.TabIndex = 0
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.DataGridView1)
		Me.TabPage1.Controls.Add(Me.GroupBox1)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage1.Size = New System.Drawing.Size(890, 428)
		Me.TabPage1.TabIndex = 0
		Me.TabPage1.Text = "Screenshots"
		Me.TabPage1.UseVisualStyleBackColor = True
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
		Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3, Me.Column4})
		Me.DataGridView1.Location = New System.Drawing.Point(8, 149)
		Me.DataGridView1.MultiSelect = False
		Me.DataGridView1.Name = "DataGridView1"
		Me.DataGridView1.ReadOnly = True
		Me.DataGridView1.RowHeadersVisible = False
		Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.DataGridView1.ShowCellErrors = False
		Me.DataGridView1.ShowEditingIcon = False
		Me.DataGridView1.ShowRowErrors = False
		Me.DataGridView1.Size = New System.Drawing.Size(874, 271)
		Me.DataGridView1.TabIndex = 1
		'
		'Column1
		'
		Me.Column1.HeaderText = "Product"
		Me.Column1.Name = "Column1"
		Me.Column1.ReadOnly = True
		Me.Column1.Width = 380
		'
		'Column2
		'
		Me.Column2.HeaderText = "Problem"
		Me.Column2.Name = "Column2"
		Me.Column2.ReadOnly = True
		Me.Column2.Width = 400
		'
		'Column3
		'
		Me.Column3.HeaderText = "A"
		Me.Column3.Name = "Column3"
		Me.Column3.ReadOnly = True
		Me.Column3.Width = 30
		'
		'Column4
		'
		Me.Column4.HeaderText = "Param"
		Me.Column4.Name = "Column4"
		Me.Column4.ReadOnly = True
		Me.Column4.Visible = False
		Me.Column4.Width = 45
		'
		'GroupBox1
		'
		Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.GroupBox1.Controls.Add(Me.CheckBox10)
		Me.GroupBox1.Controls.Add(Me.Screen_UnCheckAll_Button)
		Me.GroupBox1.Controls.Add(Me.Screen_CheckAll_Button)
		Me.GroupBox1.Controls.Add(Me.TextBox1)
		Me.GroupBox1.Controls.Add(Me.CheckBox9)
		Me.GroupBox1.Controls.Add(Me.Label_Count)
		Me.GroupBox1.Controls.Add(Me.Button_ScreenshotExport)
		Me.GroupBox1.Controls.Add(Me.CheckBox7)
		Me.GroupBox1.Controls.Add(Me.ComboBox2)
		Me.GroupBox1.Controls.Add(Me.CheckBox8)
		Me.GroupBox1.Controls.Add(Me.Button_ScreenshotFix)
		Me.GroupBox1.Controls.Add(Me.Button_ScreenshotCheck)
		Me.GroupBox1.Controls.Add(Me.Screen_Replacement_Button_Remove)
		Me.GroupBox1.Controls.Add(Me.Screen_Replacement_Button_Add)
		Me.GroupBox1.Controls.Add(Me.ComboBox1)
		Me.GroupBox1.Controls.Add(Me.CheckBox6)
		Me.GroupBox1.Controls.Add(Me.CheckBox5)
		Me.GroupBox1.Controls.Add(Me.CheckBox4)
		Me.GroupBox1.Controls.Add(Me.CheckBox3)
		Me.GroupBox1.Controls.Add(Me.CheckBox2)
		Me.GroupBox1.Controls.Add(Me.CheckBox1)
		Me.GroupBox1.Location = New System.Drawing.Point(8, 6)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(874, 137)
		Me.GroupBox1.TabIndex = 0
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Options"
		'
		'CheckBox10
		'
		Me.CheckBox10.AutoSize = True
		Me.CheckBox10.Location = New System.Drawing.Point(667, 19)
		Me.CheckBox10.Name = "CheckBox10"
		Me.CheckBox10.Size = New System.Drawing.Size(82, 17)
		Me.CheckBox10.TabIndex = 20
		Me.CheckBox10.Text = "Wrong path"
		Me.CheckBox10.UseVisualStyleBackColor = True
		'
		'Screen_UnCheckAll_Button
		'
		Me.Screen_UnCheckAll_Button.Enabled = False
		Me.Screen_UnCheckAll_Button.Location = New System.Drawing.Point(101, 107)
		Me.Screen_UnCheckAll_Button.Name = "Screen_UnCheckAll_Button"
		Me.Screen_UnCheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.Screen_UnCheckAll_Button.TabIndex = 19
		Me.Screen_UnCheckAll_Button.Text = "UnCheck all"
		Me.Screen_UnCheckAll_Button.UseVisualStyleBackColor = True
		'
		'Screen_CheckAll_Button
		'
		Me.Screen_CheckAll_Button.Enabled = False
		Me.Screen_CheckAll_Button.Location = New System.Drawing.Point(17, 107)
		Me.Screen_CheckAll_Button.Name = "Screen_CheckAll_Button"
		Me.Screen_CheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.Screen_CheckAll_Button.TabIndex = 18
		Me.Screen_CheckAll_Button.Text = "Check all"
		Me.Screen_CheckAll_Button.UseVisualStyleBackColor = True
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(574, 109)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(69, 20)
		Me.TextBox1.TabIndex = 17
		Me.TextBox1.Text = "0"
		'
		'CheckBox9
		'
		Me.CheckBox9.AutoSize = True
		Me.CheckBox9.Location = New System.Drawing.Point(437, 111)
		Me.CheckBox9.Name = "CheckBox9"
		Me.CheckBox9.Size = New System.Drawing.Size(124, 17)
		Me.CheckBox9.TabIndex = 16
		Me.CheckBox9.Text = "Check if size (byte) ="
		Me.CheckBox9.UseVisualStyleBackColor = True
		'
		'Label_Count
		'
		Me.Label_Count.AutoSize = True
		Me.Label_Count.Location = New System.Drawing.Point(680, 46)
		Me.Label_Count.Name = "Label_Count"
		Me.Label_Count.Size = New System.Drawing.Size(47, 13)
		Me.Label_Count.TabIndex = 15
		Me.Label_Count.Text = "Count: 0"
		'
		'Button_ScreenshotExport
		'
		Me.Button_ScreenshotExport.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_ScreenshotExport.Enabled = False
		Me.Button_ScreenshotExport.Location = New System.Drawing.Point(769, 85)
		Me.Button_ScreenshotExport.Name = "Button_ScreenshotExport"
		Me.Button_ScreenshotExport.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotExport.TabIndex = 14
		Me.Button_ScreenshotExport.Text = "Export list"
		Me.Button_ScreenshotExport.UseVisualStyleBackColor = True
		'
		'CheckBox7
		'
		Me.CheckBox7.AutoSize = True
		Me.CheckBox7.Location = New System.Drawing.Point(437, 65)
		Me.CheckBox7.Name = "CheckBox7"
		Me.CheckBox7.Size = New System.Drawing.Size(138, 17)
		Me.CheckBox7.TabIndex = 13
		Me.CheckBox7.Text = "HTML Entities in names"
		Me.CheckBox7.UseVisualStyleBackColor = True
		'
		'ComboBox2
		'
		Me.ComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.ComboBox2.FormattingEnabled = True
		Me.ComboBox2.Location = New System.Drawing.Point(574, 86)
		Me.ComboBox2.Name = "ComboBox2"
		Me.ComboBox2.Size = New System.Drawing.Size(153, 21)
		Me.ComboBox2.TabIndex = 12
		'
		'CheckBox8
		'
		Me.CheckBox8.AutoSize = True
		Me.CheckBox8.Location = New System.Drawing.Point(437, 88)
		Me.CheckBox8.Name = "CheckBox8"
		Me.CheckBox8.Size = New System.Drawing.Size(131, 17)
		Me.CheckBox8.TabIndex = 11
		Me.CheckBox8.Text = "Save screen count to:"
		Me.CheckBox8.UseVisualStyleBackColor = True
		'
		'Button_ScreenshotFix
		'
		Me.Button_ScreenshotFix.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_ScreenshotFix.Enabled = False
		Me.Button_ScreenshotFix.Location = New System.Drawing.Point(769, 52)
		Me.Button_ScreenshotFix.Name = "Button_ScreenshotFix"
		Me.Button_ScreenshotFix.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotFix.TabIndex = 10
		Me.Button_ScreenshotFix.Text = "Fix"
		Me.Button_ScreenshotFix.UseVisualStyleBackColor = True
		'
		'Button_ScreenshotCheck
		'
		Me.Button_ScreenshotCheck.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_ScreenshotCheck.Location = New System.Drawing.Point(769, 19)
		Me.Button_ScreenshotCheck.Name = "Button_ScreenshotCheck"
		Me.Button_ScreenshotCheck.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotCheck.TabIndex = 9
		Me.Button_ScreenshotCheck.Text = "Check"
		Me.Button_ScreenshotCheck.UseVisualStyleBackColor = True
		'
		'Screen_Replacement_Button_Remove
		'
		Me.Screen_Replacement_Button_Remove.Location = New System.Drawing.Point(353, 84)
		Me.Screen_Replacement_Button_Remove.Name = "Screen_Replacement_Button_Remove"
		Me.Screen_Replacement_Button_Remove.Size = New System.Drawing.Size(63, 23)
		Me.Screen_Replacement_Button_Remove.TabIndex = 8
		Me.Screen_Replacement_Button_Remove.Text = "Remove"
		Me.Screen_Replacement_Button_Remove.UseVisualStyleBackColor = True
		'
		'Screen_Replacement_Button_Add
		'
		Me.Screen_Replacement_Button_Add.Location = New System.Drawing.Point(304, 84)
		Me.Screen_Replacement_Button_Add.Name = "Screen_Replacement_Button_Add"
		Me.Screen_Replacement_Button_Add.Size = New System.Drawing.Size(43, 23)
		Me.Screen_Replacement_Button_Add.TabIndex = 7
		Me.Screen_Replacement_Button_Add.Text = "Add"
		Me.Screen_Replacement_Button_Add.UseVisualStyleBackColor = True
		'
		'ComboBox1
		'
		Me.ComboBox1.FormattingEnabled = True
		Me.ComboBox1.Items.AddRange(New Object() {"&amp; -> &"})
		Me.ComboBox1.Location = New System.Drawing.Point(136, 86)
		Me.ComboBox1.Name = "ComboBox1"
		Me.ComboBox1.Size = New System.Drawing.Size(162, 21)
		Me.ComboBox1.TabIndex = 6
		'
		'CheckBox6
		'
		Me.CheckBox6.AutoSize = True
		Me.CheckBox6.Location = New System.Drawing.Point(437, 42)
		Me.CheckBox6.Name = "CheckBox6"
		Me.CheckBox6.Size = New System.Drawing.Size(123, 17)
		Me.CheckBox6.TabIndex = 5
		Me.CheckBox6.Text = "Unused screenshots"
		Me.CheckBox6.UseVisualStyleBackColor = True
		'
		'CheckBox5
		'
		Me.CheckBox5.AutoSize = True
		Me.CheckBox5.Location = New System.Drawing.Point(437, 19)
		Me.CheckBox5.Name = "CheckBox5"
		Me.CheckBox5.Size = New System.Drawing.Size(224, 17)
		Me.CheckBox5.TabIndex = 4
		Me.CheckBox5.Text = "Products with 0 screens (boxes not count)"
		Me.CheckBox5.UseVisualStyleBackColor = True
		'
		'CheckBox4
		'
		Me.CheckBox4.AutoSize = True
		Me.CheckBox4.Location = New System.Drawing.Point(17, 88)
		Me.CheckBox4.Name = "CheckBox4"
		Me.CheckBox4.Size = New System.Drawing.Size(113, 17)
		Me.CheckBox4.TabIndex = 3
		Me.CheckBox4.Text = "Common Replace:"
		Me.CheckBox4.UseVisualStyleBackColor = True
		'
		'CheckBox3
		'
		Me.CheckBox3.AutoSize = True
		Me.CheckBox3.Location = New System.Drawing.Point(17, 65)
		Me.CheckBox3.Name = "CheckBox3"
		Me.CheckBox3.Size = New System.Drawing.Size(186, 17)
		Me.CheckBox3.TabIndex = 2
		Me.CheckBox3.Text = "Convert 'The thing' to 'Thing, The'"
		Me.CheckBox3.UseVisualStyleBackColor = True
		'
		'CheckBox2
		'
		Me.CheckBox2.AutoSize = True
		Me.CheckBox2.Location = New System.Drawing.Point(17, 42)
		Me.CheckBox2.Name = "CheckBox2"
		Me.CheckBox2.Size = New System.Drawing.Size(306, 17)
		Me.CheckBox2.TabIndex = 1
		Me.CheckBox2.Text = "Same name with different extentions (""scr1.jpg"" ""scr1.png"")"
		Me.CheckBox2.UseVisualStyleBackColor = True
		'
		'CheckBox1
		'
		Me.CheckBox1.AutoSize = True
		Me.CheckBox1.Location = New System.Drawing.Point(17, 19)
		Me.CheckBox1.Name = "CheckBox1"
		Me.CheckBox1.Size = New System.Drawing.Size(236, 17)
		Me.CheckBox1.TabIndex = 0
		Me.CheckBox1.Text = "Holes (i,e, ""scr1.jpg"" ""scr3.jpg"" without scr2)"
		Me.CheckBox1.UseVisualStyleBackColor = True
		'
		'FormA_MaintenanceDB
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(898, 454)
		Me.Controls.Add(Me.TabControl1)
		Me.Name = "FormA_MaintenanceDB"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "FormA_MaintenanceDB - NOT TESTED WITH UNITY AND DAZ"
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage1.ResumeLayout(False)
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub

	Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents CheckBox4 As CheckBox
    Friend WithEvents CheckBox6 As CheckBox
    Friend WithEvents CheckBox5 As CheckBox
    Friend WithEvents Screen_Replacement_Button_Remove As Button
    Friend WithEvents Screen_Replacement_Button_Add As Button
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents Button_ScreenshotFix As Button
    Friend WithEvents Button_ScreenshotCheck As Button
    Friend WithEvents ComboBox2 As ComboBox
    Friend WithEvents CheckBox8 As CheckBox
    Friend WithEvents CheckBox7 As CheckBox
    Friend WithEvents Button_ScreenshotExport As Button
    Friend WithEvents Label_Count As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents CheckBox9 As CheckBox
    Friend WithEvents Screen_UnCheckAll_Button As Button
    Friend WithEvents Screen_CheckAll_Button As Button
    Friend WithEvents Column1 As DataGridViewTextBoxColumn
    Friend WithEvents Column2 As DataGridViewTextBoxColumn
    Friend WithEvents Column3 As DataGridViewTextBoxColumn
    Friend WithEvents Column4 As DataGridViewTextBoxColumn
    Friend WithEvents CheckBox10 As CheckBox
End Class
