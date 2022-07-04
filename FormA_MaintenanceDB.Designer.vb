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
		Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
		Me.OptionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.SyncBrowsingWithMainWindowToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.TabControl1 = New Catalog_2016.TabControlEx()
		Me.TabPage_Screenshots = New Catalog_2016.TabPageEx()
		Me.DataGridView1 = New Catalog_2016.DataGridViewEx()
		Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.GroupBox1 = New Catalog_2016.GroupBoxEx()
		Me.CheckBox13 = New System.Windows.Forms.CheckBox()
		Me.CheckBox12 = New System.Windows.Forms.CheckBox()
		Me.CheckBox11 = New System.Windows.Forms.CheckBox()
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
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.DataGridView2 = New Catalog_2016.DataGridViewEx()
		Me.GroupBox2 = New Catalog_2016.GroupBoxEx()
		Me.CheckBox15 = New System.Windows.Forms.CheckBox()
		Me.DB_UnCheckAll_Button = New System.Windows.Forms.Button()
		Me.DB_CheckAll_Button = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Button_DBExport = New System.Windows.Forms.Button()
		Me.Button_DBFix = New System.Windows.Forms.Button()
		Me.Button_DBCheck = New System.Windows.Forms.Button()
		Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.MenuStrip1.SuspendLayout()
		Me.TabControl1.SuspendLayout()
		Me.TabPage_Screenshots.SuspendLayout()
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox1.SuspendLayout()
		Me.TabPage1.SuspendLayout()
		CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox2.SuspendLayout()
		Me.SuspendLayout()
		'
		'MenuStrip1
		'
		Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OptionsToolStripMenuItem})
		Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
		Me.MenuStrip1.Name = "MenuStrip1"
		Me.MenuStrip1.Size = New System.Drawing.Size(1044, 24)
		Me.MenuStrip1.TabIndex = 1
		Me.MenuStrip1.Text = "MenuStrip1"
		'
		'OptionsToolStripMenuItem
		'
		Me.OptionsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SyncBrowsingWithMainWindowToolStripMenuItem})
		Me.OptionsToolStripMenuItem.Name = "OptionsToolStripMenuItem"
		Me.OptionsToolStripMenuItem.Size = New System.Drawing.Size(61, 20)
		Me.OptionsToolStripMenuItem.Text = "Options"
		'
		'SyncBrowsingWithMainWindowToolStripMenuItem
		'
		Me.SyncBrowsingWithMainWindowToolStripMenuItem.Name = "SyncBrowsingWithMainWindowToolStripMenuItem"
		Me.SyncBrowsingWithMainWindowToolStripMenuItem.Size = New System.Drawing.Size(252, 22)
		Me.SyncBrowsingWithMainWindowToolStripMenuItem.Text = "Sync browsing with main window"
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage_Screenshots)
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TabControl1.Location = New System.Drawing.Point(0, 24)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(1044, 448)
		Me.TabControl1.TabIndex = 0
		'
		'TabPage_Screenshots
		'
		Me.TabPage_Screenshots.Controls.Add(Me.DataGridView1)
		Me.TabPage_Screenshots.Controls.Add(Me.GroupBox1)
		Me.TabPage_Screenshots.Location = New System.Drawing.Point(4, 22)
		Me.TabPage_Screenshots.Name = "TabPage_Screenshots"
		Me.TabPage_Screenshots.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage_Screenshots.Size = New System.Drawing.Size(1036, 422)
		Me.TabPage_Screenshots.TabIndex = 0
		Me.TabPage_Screenshots.Text = "Screenshots"
		Me.TabPage_Screenshots.UseVisualStyleBackColor = True
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
		Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column5, Me.Column2, Me.Column3, Me.Column4})
		Me.DataGridView1.Location = New System.Drawing.Point(8, 149)
		Me.DataGridView1.MultiSelect = False
		Me.DataGridView1.Name = "DataGridView1"
		Me.DataGridView1.ReadOnly = True
		Me.DataGridView1.RowHeadersVisible = False
		Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.DataGridView1.ShowCellErrors = False
		Me.DataGridView1.ShowEditingIcon = False
		Me.DataGridView1.ShowRowErrors = False
		Me.DataGridView1.Size = New System.Drawing.Size(1020, 265)
		Me.DataGridView1.TabIndex = 1
		'
		'Column1
		'
		Me.Column1.HeaderText = "Product"
		Me.Column1.Name = "Column1"
		Me.Column1.ReadOnly = True
		Me.Column1.Width = 180
		'
		'Column5
		'
		Me.Column5.HeaderText = "Screenshot Path"
		Me.Column5.Name = "Column5"
		Me.Column5.ReadOnly = True
		Me.Column5.Width = 270
		'
		'Column2
		'
		Me.Column2.HeaderText = "Problem"
		Me.Column2.Name = "Column2"
		Me.Column2.ReadOnly = True
		Me.Column2.Width = 500
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
		Me.GroupBox1.Controls.Add(Me.CheckBox13)
		Me.GroupBox1.Controls.Add(Me.CheckBox12)
		Me.GroupBox1.Controls.Add(Me.CheckBox11)
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
		Me.GroupBox1.Size = New System.Drawing.Size(1020, 137)
		Me.GroupBox1.TabIndex = 0
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Options"
		'
		'CheckBox13
		'
		Me.CheckBox13.AutoSize = True
		Me.CheckBox13.Location = New System.Drawing.Point(740, 42)
		Me.CheckBox13.Name = "CheckBox13"
		Me.CheckBox13.Size = New System.Drawing.Size(175, 17)
		Me.CheckBox13.TabIndex = 23
		Me.CheckBox13.Text = "Don't show, colorize others only"
		Me.CheckBox13.UseVisualStyleBackColor = True
		'
		'CheckBox12
		'
		Me.CheckBox12.AutoSize = True
		Me.CheckBox12.Checked = True
		Me.CheckBox12.CheckState = System.Windows.Forms.CheckState.Checked
		Me.CheckBox12.Location = New System.Drawing.Point(440, 42)
		Me.CheckBox12.Name = "CheckBox12"
		Me.CheckBox12.Size = New System.Drawing.Size(161, 17)
		Me.CheckBox12.TabIndex = 22
		Me.CheckBox12.Text = "Don't count boxes as screen"
		Me.CheckBox12.UseVisualStyleBackColor = True
		'
		'CheckBox11
		'
		Me.CheckBox11.AutoSize = True
		Me.CheckBox11.Location = New System.Drawing.Point(440, 88)
		Me.CheckBox11.Name = "CheckBox11"
		Me.CheckBox11.Size = New System.Drawing.Size(100, 17)
		Me.CheckBox11.TabIndex = 21
		Me.CheckBox11.Text = "Including boxes"
		Me.CheckBox11.UseVisualStyleBackColor = True
		'
		'CheckBox10
		'
		Me.CheckBox10.AutoSize = True
		Me.CheckBox10.Location = New System.Drawing.Point(720, 65)
		Me.CheckBox10.Name = "CheckBox10"
		Me.CheckBox10.Size = New System.Drawing.Size(82, 17)
		Me.CheckBox10.TabIndex = 20
		Me.CheckBox10.Text = "Wrong path"
		Me.CheckBox10.UseVisualStyleBackColor = True
		'
		'Screen_UnCheckAll_Button
		'
		Me.Screen_UnCheckAll_Button.Enabled = False
		Me.Screen_UnCheckAll_Button.Location = New System.Drawing.Point(110, 107)
		Me.Screen_UnCheckAll_Button.Name = "Screen_UnCheckAll_Button"
		Me.Screen_UnCheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.Screen_UnCheckAll_Button.TabIndex = 19
		Me.Screen_UnCheckAll_Button.Text = "UnCheck all"
		Me.Screen_UnCheckAll_Button.UseVisualStyleBackColor = True
		'
		'Screen_CheckAll_Button
		'
		Me.Screen_CheckAll_Button.Enabled = False
		Me.Screen_CheckAll_Button.Location = New System.Drawing.Point(20, 107)
		Me.Screen_CheckAll_Button.Name = "Screen_CheckAll_Button"
		Me.Screen_CheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.Screen_CheckAll_Button.TabIndex = 18
		Me.Screen_CheckAll_Button.Text = "Check all"
		Me.Screen_CheckAll_Button.UseVisualStyleBackColor = True
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(550, 108)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(69, 20)
		Me.TextBox1.TabIndex = 17
		Me.TextBox1.Text = "0"
		'
		'CheckBox9
		'
		Me.CheckBox9.AutoSize = True
		Me.CheckBox9.Location = New System.Drawing.Point(420, 111)
		Me.CheckBox9.Name = "CheckBox9"
		Me.CheckBox9.Size = New System.Drawing.Size(124, 17)
		Me.CheckBox9.TabIndex = 16
		Me.CheckBox9.Text = "Check if size (byte) ="
		Me.CheckBox9.UseVisualStyleBackColor = True
		'
		'Label_Count
		'
		Me.Label_Count.AutoSize = True
		Me.Label_Count.Location = New System.Drawing.Point(885, 116)
		Me.Label_Count.Name = "Label_Count"
		Me.Label_Count.Size = New System.Drawing.Size(47, 13)
		Me.Label_Count.TabIndex = 15
		Me.Label_Count.Text = "Count: 0"
		'
		'Button_ScreenshotExport
		'
		Me.Button_ScreenshotExport.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_ScreenshotExport.Enabled = False
		Me.Button_ScreenshotExport.Location = New System.Drawing.Point(915, 85)
		Me.Button_ScreenshotExport.Name = "Button_ScreenshotExport"
		Me.Button_ScreenshotExport.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotExport.TabIndex = 14
		Me.Button_ScreenshotExport.Text = "Export list"
		Me.Button_ScreenshotExport.UseVisualStyleBackColor = True
		'
		'CheckBox7
		'
		Me.CheckBox7.AutoSize = True
		Me.CheckBox7.Location = New System.Drawing.Point(720, 88)
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
		Me.ComboBox2.Location = New System.Drawing.Point(550, 63)
		Me.ComboBox2.Name = "ComboBox2"
		Me.ComboBox2.Size = New System.Drawing.Size(150, 21)
		Me.ComboBox2.TabIndex = 12
		'
		'CheckBox8
		'
		Me.CheckBox8.AutoSize = True
		Me.CheckBox8.Location = New System.Drawing.Point(420, 65)
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
		Me.Button_ScreenshotFix.Location = New System.Drawing.Point(915, 52)
		Me.Button_ScreenshotFix.Name = "Button_ScreenshotFix"
		Me.Button_ScreenshotFix.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotFix.TabIndex = 10
		Me.Button_ScreenshotFix.Text = "Fix"
		Me.Button_ScreenshotFix.UseVisualStyleBackColor = True
		'
		'Button_ScreenshotCheck
		'
		Me.Button_ScreenshotCheck.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_ScreenshotCheck.Location = New System.Drawing.Point(915, 19)
		Me.Button_ScreenshotCheck.Name = "Button_ScreenshotCheck"
		Me.Button_ScreenshotCheck.Size = New System.Drawing.Size(99, 27)
		Me.Button_ScreenshotCheck.TabIndex = 9
		Me.Button_ScreenshotCheck.Text = "Check"
		Me.Button_ScreenshotCheck.UseVisualStyleBackColor = True
		'
		'Screen_Replacement_Button_Remove
		'
		Me.Screen_Replacement_Button_Remove.Location = New System.Drawing.Point(341, 85)
		Me.Screen_Replacement_Button_Remove.Name = "Screen_Replacement_Button_Remove"
		Me.Screen_Replacement_Button_Remove.Size = New System.Drawing.Size(63, 23)
		Me.Screen_Replacement_Button_Remove.TabIndex = 8
		Me.Screen_Replacement_Button_Remove.Text = "Remove"
		Me.Screen_Replacement_Button_Remove.UseVisualStyleBackColor = True
		'
		'Screen_Replacement_Button_Add
		'
		Me.Screen_Replacement_Button_Add.Location = New System.Drawing.Point(292, 85)
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
		Me.ComboBox1.Size = New System.Drawing.Size(150, 21)
		Me.ComboBox1.TabIndex = 6
		'
		'CheckBox6
		'
		Me.CheckBox6.AutoSize = True
		Me.CheckBox6.Location = New System.Drawing.Point(720, 19)
		Me.CheckBox6.Name = "CheckBox6"
		Me.CheckBox6.Size = New System.Drawing.Size(123, 17)
		Me.CheckBox6.TabIndex = 5
		Me.CheckBox6.Text = "Unused screenshots"
		Me.CheckBox6.UseVisualStyleBackColor = True
		'
		'CheckBox5
		'
		Me.CheckBox5.AutoSize = True
		Me.CheckBox5.Location = New System.Drawing.Point(420, 19)
		Me.CheckBox5.Name = "CheckBox5"
		Me.CheckBox5.Size = New System.Drawing.Size(139, 17)
		Me.CheckBox5.TabIndex = 4
		Me.CheckBox5.Text = "Products with 0 screens"
		Me.CheckBox5.UseVisualStyleBackColor = True
		'
		'CheckBox4
		'
		Me.CheckBox4.AutoSize = True
		Me.CheckBox4.Location = New System.Drawing.Point(20, 88)
		Me.CheckBox4.Name = "CheckBox4"
		Me.CheckBox4.Size = New System.Drawing.Size(113, 17)
		Me.CheckBox4.TabIndex = 3
		Me.CheckBox4.Text = "Common Replace:"
		Me.CheckBox4.UseVisualStyleBackColor = True
		'
		'CheckBox3
		'
		Me.CheckBox3.AutoSize = True
		Me.CheckBox3.Location = New System.Drawing.Point(20, 65)
		Me.CheckBox3.Name = "CheckBox3"
		Me.CheckBox3.Size = New System.Drawing.Size(186, 17)
		Me.CheckBox3.TabIndex = 2
		Me.CheckBox3.Text = "Convert 'The thing' to 'Thing, The'"
		Me.CheckBox3.UseVisualStyleBackColor = True
		'
		'CheckBox2
		'
		Me.CheckBox2.AutoSize = True
		Me.CheckBox2.Location = New System.Drawing.Point(20, 42)
		Me.CheckBox2.Name = "CheckBox2"
		Me.CheckBox2.Size = New System.Drawing.Size(306, 17)
		Me.CheckBox2.TabIndex = 1
		Me.CheckBox2.Text = "Same name with different extentions (""scr1.jpg"" ""scr1.png"")"
		Me.CheckBox2.UseVisualStyleBackColor = True
		'
		'CheckBox1
		'
		Me.CheckBox1.AutoSize = True
		Me.CheckBox1.Location = New System.Drawing.Point(20, 19)
		Me.CheckBox1.Name = "CheckBox1"
		Me.CheckBox1.Size = New System.Drawing.Size(263, 17)
		Me.CheckBox1.TabIndex = 0
		Me.CheckBox1.Text = "Holes (i,e, ""scr1.jpg"" ""scr3.jpg"" without ""scr2.jpg"")"
		Me.CheckBox1.UseVisualStyleBackColor = True
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.DataGridView2)
		Me.TabPage1.Controls.Add(Me.GroupBox2)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Size = New System.Drawing.Size(1036, 422)
		Me.TabPage1.TabIndex = 1
		Me.TabPage1.Text = "Database"
		Me.TabPage1.UseVisualStyleBackColor = True
		'
		'DataGridView2
		'
		Me.DataGridView2.AllowUserToAddRows = False
		Me.DataGridView2.AllowUserToDeleteRows = False
		Me.DataGridView2.AllowUserToResizeRows = False
		Me.DataGridView2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.DataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.DataGridView2.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn3, Me.DataGridViewTextBoxColumn4, Me.DataGridViewTextBoxColumn5})
		Me.DataGridView2.Location = New System.Drawing.Point(8, 149)
		Me.DataGridView2.MultiSelect = False
		Me.DataGridView2.Name = "DataGridView2"
		Me.DataGridView2.ReadOnly = True
		Me.DataGridView2.RowHeadersVisible = False
		Me.DataGridView2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.DataGridView2.ShowCellErrors = False
		Me.DataGridView2.ShowEditingIcon = False
		Me.DataGridView2.ShowRowErrors = False
		Me.DataGridView2.Size = New System.Drawing.Size(1020, 265)
		Me.DataGridView2.TabIndex = 2
		'
		'GroupBox2
		'
		Me.GroupBox2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.GroupBox2.Controls.Add(Me.CheckBox15)
		Me.GroupBox2.Controls.Add(Me.DB_UnCheckAll_Button)
		Me.GroupBox2.Controls.Add(Me.DB_CheckAll_Button)
		Me.GroupBox2.Controls.Add(Me.Label1)
		Me.GroupBox2.Controls.Add(Me.Button_DBExport)
		Me.GroupBox2.Controls.Add(Me.Button_DBFix)
		Me.GroupBox2.Controls.Add(Me.Button_DBCheck)
		Me.GroupBox2.Location = New System.Drawing.Point(8, 6)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(1020, 137)
		Me.GroupBox2.TabIndex = 1
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Options"
		'
		'CheckBox15
		'
		Me.CheckBox15.AutoSize = True
		Me.CheckBox15.Location = New System.Drawing.Point(20, 19)
		Me.CheckBox15.Name = "CheckBox15"
		Me.CheckBox15.Size = New System.Drawing.Size(233, 17)
		Me.CheckBox15.TabIndex = 24
		Me.CheckBox15.Text = "Products without category (not shown in db)"
		Me.CheckBox15.UseVisualStyleBackColor = True
		'
		'DB_UnCheckAll_Button
		'
		Me.DB_UnCheckAll_Button.Enabled = False
		Me.DB_UnCheckAll_Button.Location = New System.Drawing.Point(110, 107)
		Me.DB_UnCheckAll_Button.Name = "DB_UnCheckAll_Button"
		Me.DB_UnCheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.DB_UnCheckAll_Button.TabIndex = 19
		Me.DB_UnCheckAll_Button.Text = "UnCheck all"
		Me.DB_UnCheckAll_Button.UseVisualStyleBackColor = True
		'
		'DB_CheckAll_Button
		'
		Me.DB_CheckAll_Button.Enabled = False
		Me.DB_CheckAll_Button.Location = New System.Drawing.Point(20, 107)
		Me.DB_CheckAll_Button.Name = "DB_CheckAll_Button"
		Me.DB_CheckAll_Button.Size = New System.Drawing.Size(78, 23)
		Me.DB_CheckAll_Button.TabIndex = 18
		Me.DB_CheckAll_Button.Text = "Check all"
		Me.DB_CheckAll_Button.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(885, 116)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(47, 13)
		Me.Label1.TabIndex = 15
		Me.Label1.Text = "Count: 0"
		'
		'Button_DBExport
		'
		Me.Button_DBExport.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_DBExport.Enabled = False
		Me.Button_DBExport.Location = New System.Drawing.Point(915, 85)
		Me.Button_DBExport.Name = "Button_DBExport"
		Me.Button_DBExport.Size = New System.Drawing.Size(99, 27)
		Me.Button_DBExport.TabIndex = 14
		Me.Button_DBExport.Text = "Export list"
		Me.Button_DBExport.UseVisualStyleBackColor = True
		'
		'Button_DBFix
		'
		Me.Button_DBFix.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_DBFix.Enabled = False
		Me.Button_DBFix.Location = New System.Drawing.Point(915, 52)
		Me.Button_DBFix.Name = "Button_DBFix"
		Me.Button_DBFix.Size = New System.Drawing.Size(99, 27)
		Me.Button_DBFix.TabIndex = 10
		Me.Button_DBFix.Text = "Fix"
		Me.Button_DBFix.UseVisualStyleBackColor = True
		'
		'Button_DBCheck
		'
		Me.Button_DBCheck.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Button_DBCheck.Location = New System.Drawing.Point(915, 19)
		Me.Button_DBCheck.Name = "Button_DBCheck"
		Me.Button_DBCheck.Size = New System.Drawing.Size(99, 27)
		Me.Button_DBCheck.TabIndex = 9
		Me.Button_DBCheck.Text = "Check"
		Me.Button_DBCheck.UseVisualStyleBackColor = True
		'
		'DataGridViewTextBoxColumn1
		'
		Me.DataGridViewTextBoxColumn1.HeaderText = "Product"
		Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
		Me.DataGridViewTextBoxColumn1.ReadOnly = True
		Me.DataGridViewTextBoxColumn1.Width = 420
		'
		'DataGridViewTextBoxColumn3
		'
		Me.DataGridViewTextBoxColumn3.HeaderText = "Problem"
		Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
		Me.DataGridViewTextBoxColumn3.ReadOnly = True
		Me.DataGridViewTextBoxColumn3.Width = 500
		'
		'DataGridViewTextBoxColumn4
		'
		Me.DataGridViewTextBoxColumn4.HeaderText = "A"
		Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
		Me.DataGridViewTextBoxColumn4.ReadOnly = True
		Me.DataGridViewTextBoxColumn4.Width = 30
		'
		'DataGridViewTextBoxColumn5
		'
		Me.DataGridViewTextBoxColumn5.HeaderText = "Param"
		Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
		Me.DataGridViewTextBoxColumn5.ReadOnly = True
		Me.DataGridViewTextBoxColumn5.Visible = False
		Me.DataGridViewTextBoxColumn5.Width = 45
		'
		'FormA_MaintenanceDB
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(1044, 472)
		Me.Controls.Add(Me.TabControl1)
		Me.Controls.Add(Me.MenuStrip1)
		Me.DoubleBuffered = True
		Me.Name = "FormA_MaintenanceDB"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "FormA_MaintenanceDB - NOT TESTED WITH UNITY AND DAZ"
		Me.MenuStrip1.ResumeLayout(False)
		Me.MenuStrip1.PerformLayout()
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage_Screenshots.ResumeLayout(False)
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.TabPage1.ResumeLayout(False)
		CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox2.ResumeLayout(False)
		Me.GroupBox2.PerformLayout()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents TabControl1 As TabControlEx
	Friend WithEvents TabPage_Screenshots As TabPageEx
	Friend WithEvents GroupBox1 As GroupBoxEx
	Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents CheckBox4 As CheckBox
    Friend WithEvents CheckBox6 As CheckBox
    Friend WithEvents CheckBox5 As CheckBox
    Friend WithEvents Screen_Replacement_Button_Remove As Button
    Friend WithEvents Screen_Replacement_Button_Add As Button
    Friend WithEvents ComboBox1 As ComboBox
	Friend WithEvents DataGridView1 As DataGridViewEx
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
	Friend WithEvents CheckBox10 As CheckBox
	Friend WithEvents CheckBox11 As CheckBox
	Friend WithEvents CheckBox12 As CheckBox
	Friend WithEvents MenuStrip1 As MenuStrip
	Friend WithEvents OptionsToolStripMenuItem As ToolStripMenuItem
	Friend WithEvents SyncBrowsingWithMainWindowToolStripMenuItem As ToolStripMenuItem
	Friend WithEvents CheckBox13 As CheckBox
	Friend WithEvents Column1 As DataGridViewTextBoxColumn
	Friend WithEvents Column5 As DataGridViewTextBoxColumn
	Friend WithEvents Column2 As DataGridViewTextBoxColumn
	Friend WithEvents Column3 As DataGridViewTextBoxColumn
	Friend WithEvents Column4 As DataGridViewTextBoxColumn
	Friend WithEvents TabPage1 As TabPage
	Friend WithEvents DataGridView2 As DataGridViewEx
	Friend WithEvents GroupBox2 As GroupBoxEx
	Friend WithEvents CheckBox15 As CheckBox
	Friend WithEvents DB_UnCheckAll_Button As Button
	Friend WithEvents DB_CheckAll_Button As Button
	Friend WithEvents Label1 As Label
	Friend WithEvents Button_DBExport As Button
	Friend WithEvents Button_DBFix As Button
	Friend WithEvents Button_DBCheck As Button
	Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
	Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
	Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
	Friend WithEvents DataGridViewTextBoxColumn5 As DataGridViewTextBoxColumn
End Class
