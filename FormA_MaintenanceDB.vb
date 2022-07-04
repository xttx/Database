Imports System.ComponentModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web

'TODO: Disable datagridview column headers highlight
'TODO: Screen-ShowWrongPatch won't detect boxes
'TODO: Step 2 (converting names) is very slow

'---------------------------------------------------------------------------------------------
'DONE: Split ScreenNotFound and ScreenAtWrongPath into 2 separate options
'DONE: Check if screen is unused, before add it to wrong path
'DONE: Sync table browsing (selection change) to main form game list
'DONE: Separate name and path in dataGridView table in two columns
'DONE: Open screen folder by clicking on path column in the table row
'DONE: Disable "Don't count boxes as screen" checkbox, if first screen is not configured in screenshot options OR when "Product with 0 screens" checkbox is unchecked
'DONE: Disable "Including boxes" checkbox, if first screen is not configured in screenshot options OR when "Screen count" checkbox is unchecked
'DONE: Realtime count update
'DONE: Lock TabControl, but not disable it (to allow scroll the table while scanning)
'DONE: Better sync table browsing to main form game list - add OnSelectionChange and try to handle " - " replacement to ": " etc.
'DONE PARTIAL: Form is slow to resize (changed most of controls to custom doublebouffered, but resizing still quite slow)

Public Class FormA_MaintenanceDB
    Dim screenshotPath As String = ""
    Dim screenshotFiles As New List(Of String)
    Dim screenshotFilesParsed As New List(Of screenshotInfo)
    Dim screenshotFiles_byProduct As New Dictionary(Of String, productInfo)(StringComparer.InvariantCultureIgnoreCase)
    Dim screenshotFiles_byProductPath As New Dictionary(Of String, productInfo)(StringComparer.InvariantCultureIgnoreCase)
    Dim screenshotFiles_byProductPath_FindUnused As New Dictionary(Of String, productInfo)(StringComparer.InvariantCultureIgnoreCase)
    Dim screenshotNeeded_byProduct As New Dictionary(Of String, productInfo)(StringComparer.InvariantCultureIgnoreCase)

    Dim Main_Form As Form1 = Form0_Loader.main_form
    Dim Screenshot_Start_Ind As Integer = Form0_Loader.main_form.Screenshot_Options.FirstIndex
    Dim Screenshot_Suffix As String = Form0_Loader.main_form.Screenshot_Options.Suffix
    Dim Screenshot_Special_First As Boolean = Form0_Loader.main_form.Screenshot_Options.FirstSpecial
    Dim Screenshot_Special_Suffix As String = Form0_Loader.main_form.Screenshot_Options.FirstSuffix

    Dim Step_String = ""

    Dim Label_status As New Label With {.AutoSize = False, .TextAlign = ContentAlignment.MiddleCenter, .Size = New Size(550, 70)}

    Dim EndsWithNumber_regex As Regex = New Regex("(.+?)(\d+$)", RegexOptions.Compiled)

    Class productInfo
        Public db_product_id As Integer = -1
        Public screens As New List(Of screenshotInfo)
    End Class
    Structure screenshotInfo
        Dim product As String
        Dim suffix As String
        Dim fileNameNoExt As String
        Dim isFirstSpecial As Boolean
        Dim suffix_num As Integer
        Dim screenshot_dir As String
        Dim screenshot_ext As String
    End Structure
    Enum screenshots_problems
        Holes
        SameNameMultipleExt
        WrongPlacedThe
        Replace
        WrongPath
        Unused
        HTMLEntities
        ScreenCount
        MatchedSize
    End Enum
    Enum db_problems
        NoCategory
    End Enum

    Public Sub New(screen_path As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Label_status.Font = New Font(Label_status.Font.FontFamily, 12)
        Label_status.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        screenshotPath = screen_path

        For Each fi In Fields
            If fi.DBname.ToUpper.Contains("_NUM") Then
                If fi.name.Trim <> "" Then
                    ComboBox2.Items.Add(fi.DBname + " (" + fi.name + ")")
                Else
                    ComboBox2.Items.Add(fi.DBname)
                End If
            End If
        Next

        'Disable controls related to Screenshot_Special_First if it's disable in options
        'Add handler to disable/enable context controls, depending on parent state
        If Not Screenshot_Special_First Then
            CheckBox11.Checked = False : CheckBox11.Enabled = False
            CheckBox12.Checked = False : CheckBox12.Enabled = False
            Dim b1 As New Button With {.Text = "?"}
            Dim b2 As New Button With {.Text = "?"}
            b1.Size = New Size(CheckBox12.Height + 2, CheckBox12.Height + 2)
            b2.Size = New Size(CheckBox12.Height + 2, CheckBox12.Height + 2)
            b1.Location = New Point(CheckBox12.Left + CheckBox12.Width, CheckBox11.Top - 1)
            b2.Location = New Point(CheckBox12.Left + CheckBox12.Width, CheckBox12.Top - 1)
            CheckBox12.Parent.Controls.Add(b1) : CheckBox12.Parent.Controls.Add(b2)
            Custom_ToolTip.Setup(b1, """FirstScreenSpecial"" is disabled in screenshot options. This feature is disabled.", Me)
            Custom_ToolTip.Setup(b2, """FirstScreenSpecial"" is disabled in screenshot options. This feature is disabled.", Me)
        Else
            CheckBox12.Enabled = CheckBox5.Checked
            CheckBox11.Enabled = CheckBox8.Checked
            AddHandler CheckBox5.CheckedChanged, Sub() CheckBox12.Enabled = CheckBox5.Checked
            AddHandler CheckBox8.CheckedChanged, Sub() CheckBox11.Enabled = CheckBox8.Checked
        End If
        ComboBox2.Enabled = CheckBox8.Checked
        AddHandler CheckBox8.CheckedChanged, Sub() ComboBox2.Enabled = CheckBox8.Checked

        CheckBox13.Checked = False : CheckBox13.Enabled = False
        Custom_ToolTip.Setup(CheckBox13, "Don't show unused screens in the table, but use this data to colorize output of other checks.", Me)
        AddHandler CheckBox6.CheckedChanged, Sub() CheckBox13.Enabled = CheckBox6.Checked
    End Sub

    'Check routines - Screenshots
    Private Sub Button_ScreenshotCheck_Click(sender As Object, e As EventArgs) Handles Button_ScreenshotCheck.Click
        If screenshotPath.Trim = "" Then MsgBox("Screenshot path not set.") : Exit Sub
        If Not My.Computer.FileSystem.DirectoryExists(screenshotPath) Then MsgBox("Screenshot path not exist.") : Exit Sub

        Label_status.Text = "Prepare..."
        DataGridView1.Rows.Clear()
        DataGridView1.SuspendLayout()
        DisableControlsAndCreateStatusLabel()
        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_ScreenshotCheck_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_ScreenshotCheck_ClickBG(sender As Object, e As DoWorkEventArgs)
        Dim c As Integer = 0
        Dim Step_Current = 1
        Dim Step_Count = CInt(GroupBox1.Invoke(Function() GroupBox1.Controls.OfType(Of CheckBox).Where(Function(chk) chk.Checked).Except({CheckBox11, CheckBox12, CheckBox13}).Count))
        If Step_Count = 0 Then Me.Invoke(Sub() MsgBox("No action selected.")) : Exit Sub
        Step_Count += 2
        If CheckBox9.Checked Then Step_Count -= 1
        If CheckBox5.Checked Or CheckBox6.Checked Or CheckBox8.Checked Or CheckBox10.Checked Then Step_Count += 1
        Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "

        'Pre-step 1 - Get all screen files list
        screenshotFiles.Clear()
        screenshotFilesParsed.Clear()
        screenshotFiles_byProduct.Clear()
        screenshotFiles_byProductPath.Clear()
        screenshotFiles_byProductPath_FindUnused.Clear()
        screenshotNeeded_byProduct.Clear()
        Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(screenshotPath)
        Step_Current += 1

        'Pre-step 2 - Convert screenshot names to product names
        Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
        For Each f In screenshotFiles
            c += 1
            If c Mod 15000 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Convert screenshot names to product names... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            'Screen list
            Dim s_info = Button_ScreenshotFix_GetProductFromFileName(f)
            screenshotFilesParsed.Add(s_info)

            'Product list
            'Only required in Products with no screens / Unused Screens / Screen count / Wrong paths
            If CheckBox5.Checked OrElse CheckBox6.Checked OrElse CheckBox8.Checked OrElse CheckBox10.Checked Then
                Dim p_info As New productInfo
                If Not screenshotFiles_byProduct.ContainsKey(s_info.product) Then
                    screenshotFiles_byProduct.Add(s_info.product, p_info)
                Else
                    p_info = screenshotFiles_byProduct(s_info.product)
                End If
                p_info.screens.Add(s_info)

                Dim path = s_info.screenshot_dir + "\" + s_info.product
                Dim p_info2 As New productInfo
                If Not screenshotFiles_byProductPath.ContainsKey(path) Then
                    screenshotFiles_byProductPath.Add(path, p_info2)
                Else
                    p_info2 = screenshotFiles_byProductPath(path)
                End If
                p_info2.screens.Add(s_info)
            End If
        Next
        Step_Current += 1

        'Get copy of productListByPath for finding unused screenshots
        screenshotFiles_byProductPath_FindUnused = New Dictionary(Of String, productInfo)(screenshotFiles_byProductPath, StringComparer.InvariantCultureIgnoreCase)

        'Database cache
        'Only required in Products with no screens / Unused Screens / Screen count / Wrong paths
        If CheckBox5.Checked Or CheckBox6.Checked Or CheckBox8.Checked Or CheckBox10.Checked Then
            c = 0
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            Dim db_prod_count = db.queryDataset("SELECT count(id) FROM main").Rows(0).Item(0).ToString
            Dim sql = "SELECT main.id, main.name, group_concat(category.cat) "
            'TODO - Get rid of checking catalog type
            If Main_Form.type = catalog_type.games Then sql += ", data_num1 "
            sql += "FROM main "
            sql += "JOIN category ON main.id = category.id_main "
            sql += "GROUP BY main.id, main.name"
            Dim r = db.queryReader(sql)
            Do While r.Read
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Caching database screens... " + c.ToString + "/" + db_prod_count)

                Dim p_info = New productInfo With {.screens = New List(Of screenshotInfo), .db_product_id = r.GetInt32(0)}
                'TODO - this should never happen, but it does (OverKill in game db). Need to handle this
                If screenshotNeeded_byProduct.ContainsKey(r.Item("name").ToString) Then Continue Do

                screenshotNeeded_byProduct.Add(r.Item("name").ToString, p_info)
                Dim cat = r.Item(2).ToString.Replace(",", vbCrLf).Replace("&", "&&")
                Dim scr As String = ""
                Dim needed = 0
                If Main_Form.Screenshot_Options.FirstSpecial Then needed = 1
                For n As Integer = 0 To needed
                    If Main_Form.type = catalog_type.games Then
                        Dim year = r.Item(3).ToString
                        scr = Main_Form.getScreen(r.Item("name").ToString, cat, n, year, True)
                    Else
                        scr = Main_Form.getScreen(r.Item("name").ToString, cat, n, "", True)
                    End If
                    If (Not Main_Form.Screenshot_Options.FirstSpecial Or needed = 1) AndAlso scr.EndsWith(Screenshot_Suffix) Then
                        scr = scr.Substring(0, scr.Length - Screenshot_Suffix.Length)
                    End If

                    Dim s_info As New screenshotInfo
                    Dim scr_path = IO.Path.GetDirectoryName(scr)
                    scr_path = scr_path.Substring(screenshotPath.Length)
                    If scr_path.StartsWith("\") Then scr_path = scr_path.Substring(1)
                    s_info.fileNameNoExt = IO.Path.GetFileName(scr)
                    s_info.screenshot_dir = scr_path
                    p_info.screens.Add(s_info)
                Next
            Loop
            Step_Current += 1
        End If

        'Unused Screens
        If CheckBox6.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Unused_Screens() : Step_Current += 1
        End If

        'Checking Holes
        If CheckBox1.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Holes() : Step_Current += 1
        End If

        'Checking same name with different extensions
        If CheckBox2.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_SameName_DifferentExtension() : Step_Current += 1
        End If

        'Convert "The Screen" to "Screen, The"
        If CheckBox3.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Article_THE_At_Begining() : Step_Current += 1
        End If

        'Common replace
        If CheckBox4.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Common_Replace_Substring() : Step_Current += 1
        End If

        'Products with no screens
        If CheckBox5.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Product_With_No_Screens() : Step_Current += 1
        End If

        'Screen count
        If CheckBox8.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Screens_Count() : Step_Current += 1
        End If

        'Wrong screen paths
        If CheckBox10.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_Wrong_Screens_Path() : Step_Current += 1
        End If

        'HTML Entities in names
        If CheckBox7.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Check_HTML_Entities_in_Filenames() : Step_Current += 1
        End If

        'OBSOLETE - Check Products with no screens / Unused Screens / Screen count / Wrong paths
        If False And (CheckBox5.Checked Or CheckBox6.Checked Or CheckBox8.Checked Or CheckBox10.Checked) Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            OBSOLETE_Check()

            If CheckBox5.Checked Then Step_Current += 1
            If CheckBox6.Checked Then Step_Current += 1
            If CheckBox8.Checked Then Step_Current += 1
            If CheckBox10.Checked Then Step_Current += 1
        End If

        Label_status.Invoke(Sub() Label_status.Text = "Free memory...")
        screenshotFiles.Clear()
        GC.Collect()
        GC.WaitForPendingFinalizers()

        ShowRowCount(True)
        Dim listNotEmpty = DataGridView1.Rows.Count > 0
        Me.Invoke(Sub() Button_ScreenshotFix.Enabled = listNotEmpty)
        Me.Invoke(Sub() Button_ScreenshotExport.Enabled = listNotEmpty)
        Me.Invoke(Sub() Screen_CheckAll_Button.Enabled = listNotEmpty)
        Me.Invoke(Sub() Screen_UnCheckAll_Button.Enabled = listNotEmpty)
    End Sub
    Private Sub Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(path As String)
        Label_status.Invoke(Sub() Label_status.Text = Step_String + "Scanning " + path + " (" + screenshotFiles.Count.ToString + ")")
        For Each f In IO.Directory.GetFiles(path, "*.*", IO.SearchOption.TopDirectoryOnly)
            Dim scr = f.Substring(screenshotPath.Length)
            If scr.StartsWith("\") Then scr = scr.Substring(1)
            screenshotFiles.Add(scr)
        Next

        'Check size
        If CheckBox9.Checked Then
            Dim req_size = CInt(TextBox1.Text)
            Dim fi_arr = New DirectoryInfo(path).GetFiles("*.*")
            For Each fi In fi_arr
                If fi.Length = req_size Then
                    Dim p = fi.FullName.Substring(screenshotPath.Length)
                    If p.StartsWith("\") Then p = p.Substring(1)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({IO.Path.GetFileName(p), IO.Path.GetDirectoryName(p), "Match requested size"}))
                End If
            Next
        End If


        For Each d In IO.Directory.GetDirectories(path, "*.*", IO.SearchOption.TopDirectoryOnly)
            Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(d)
        Next
    End Sub
    Private Function Button_ScreenshotFix_GetProductFromFileName(f As String) As screenshotInfo
        'TODO: this won't work well with daz screens, because they have number right after product name without separation,
        ' and can have TshirtForV41, TshirtForV42, TshirtForV43 - number will be 41,42,43 and not 1,2,3 as expected
        Dim i As New screenshotInfo With {.suffix = "", .suffix_num = -1, .isFirstSpecial = False}
        i.fileNameNoExt = IO.Path.GetFileNameWithoutExtension(f)
        i.screenshot_dir = IO.Path.GetDirectoryName(f)
        i.screenshot_ext = IO.Path.GetExtension(f).Replace(".", "")

        'Check for first special suffix
        If Screenshot_Special_First AndAlso i.fileNameNoExt.ToUpper.EndsWith(Screenshot_Special_Suffix.ToUpper()) Then
            i.isFirstSpecial = True
            i.suffix = Screenshot_Special_Suffix
            i.product = i.fileNameNoExt.Substring(0, i.fileNameNoExt.Length - Screenshot_Special_Suffix.Length)
        Else
            'Dim matchGroups = EndsWithNumber_regex.Match(i.fileNameNoExt).Groups
            'Dim numbers_at_the_end = String.Concat(i.fileNameNoExt.ToArray().Reverse().TakeWhile(Function(c) Char.IsNumber(c)).Reverse())

            'Get numbers, at the end of fileNameNoExt
            Dim numbers_at_the_end = ""
            Dim cur = i.fileNameNoExt.Length - 1
            While cur >= 0 AndAlso Char.IsDigit(i.fileNameNoExt(cur))
                numbers_at_the_end = i.fileNameNoExt(cur) + numbers_at_the_end : cur -= 1
            End While

            If numbers_at_the_end = "" Then ' matchGroups.Count < 2 Then
                'There is no number at the end
                i.product = i.fileNameNoExt
            Else
                'Get rid of number at the end, and put the number to the suffix
                'i.product = matchGroups(1).Value
                'i.suffix = matchGroups(2).Value
                i.product = i.fileNameNoExt.Substring(0, i.fileNameNoExt.Length - numbers_at_the_end.Length)
                i.suffix = numbers_at_the_end
                i.suffix_num = CInt(i.suffix)

                If i.product.EndsWith(Screenshot_Suffix.ToUpper()) Then
                    i.product = i.product.Substring(0, i.product.Length - Screenshot_Suffix.Length)
                    i.suffix = Screenshot_Suffix + i.suffix 'i.e. "_" + "1"
                End If
            End If
        End If
        Return i
    End Function
    'Check routines - DB
    Private Sub Button_DBCheck_Click(sender As Object, e As EventArgs) Handles Button_DBCheck.Click
        Label_status.Text = "Prepare..."
        DataGridView2.Rows.Clear()
        DataGridView2.SuspendLayout()
        DisableControlsAndCreateStatusLabel()
        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_DBCheck_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_DBCheck_ClickBG(sender As Object, e As DoWorkEventArgs)
        Dim c As Integer = 0
        Dim Step_Current = 1
        Dim Step_Count = CInt(GroupBox2.Invoke(Function() GroupBox2.Controls.OfType(Of CheckBox).Where(Function(chk) chk.Checked).Count))
        If Step_Count = 0 Then Me.Invoke(Sub() MsgBox("No action selected.")) : Exit Sub
        Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "

        'Products with no category
        If CheckBox15.Checked Then
            Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            c = 0
            Dim sql = "SELECT main.id, main.name FROM main LEFT JOIN category ON main.id = category.id_main WHERE category.id_main IS NULL "
            Dim ds = db.queryDataset(sql)
            For Each row As DataRow In ds.Rows
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking products with no category... " + c.ToString + "/" + ds.Rows.Count.ToString)

                DataGridView2.Invoke(Sub() DataGridView2.Rows.Add({row.Item(1).ToString(), "No Assigned Category", "X", db_problems.NoCategory.ToString + "|||" + row.Item(0).ToString()}))
                ShowRowCount(False, 1)
            Next

            Step_Current += 1
        End If

        Label_status.Invoke(Sub() Label_status.Text = "Free memory...")
        GC.Collect()
        GC.WaitForPendingFinalizers()

        ShowRowCount(True, 1)
        Dim listNotEmpty = DataGridView2.Rows.Count > 0
        Me.Invoke(Sub() Button_DBFix.Enabled = listNotEmpty)
        Me.Invoke(Sub() Button_DBExport.Enabled = listNotEmpty)
        Me.Invoke(Sub() DB_CheckAll_Button.Enabled = listNotEmpty)
        Me.Invoke(Sub() DB_UnCheckAll_Button.Enabled = listNotEmpty)
    End Sub

    'Fix routines - Screenshot
    Private Sub Button_ScreenshotFix_Click(sender As Object, e As EventArgs) Handles Button_ScreenshotFix.Click
        DisableControlsAndCreateStatusLabel()
        Label_status.Text = "Prepare..."

        'Unload currently displayed screen in form1, because otherwise it cannot be accessed
        For Each p As PictureBox In {Main_Form.PictureBox1, Main_Form.PictureBox2, Main_Form.PictureBox3, Main_Form.PictureBox4, Main_Form.BigPictureBox}
            If p.Image IsNot Nothing Then p.Image.Dispose() : p.Image = Nothing 'This is not enough
            Try
                p.Load(".\img.png") 'I didn't found a better way to dispose existing image and unlock file
            Catch ex As Exception

            End Try
        Next

        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_ScreenshotFix_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_ScreenshotFix_Click_OBSOLETE(sender As Object, e As EventArgs)
        Dim button = DirectCast(sender, Button)
        If button.Tag Is Nothing Then
            DisableControlsAndCreateStatusLabel()
            Label_status.Text = "Prepare..."

            'Unload currently displayed screen in form1, because otherwise it cannot be accessed
            Dim f = DirectCast(Me.Owner, Form1)
            For Each p As PictureBox In {f.PictureBox1, f.PictureBox2, f.PictureBox3, f.PictureBox4, f.BigPictureBox}
                If p.Image IsNot Nothing Then p.Image.Dispose() : p.Image = Nothing 'This is not enough
                Try
                    p.Load(".\img.png") 'I didn't found a better way to dispose existing image and unlock file
                Catch ex As Exception

                End Try
            Next
        End If

        'We need to fix SameNameMultipleExt problem in this thread, because it's require to show additional panel
        Dim start_from = 0
        If button.Tag IsNot Nothing Then start_from = CInt(button.Tag) + 1
        button.Tag = Nothing
        For n As Integer = start_from To DataGridView1.Rows.Count - 1
            Dim r = DataGridView1.Rows(n)
            If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.SameNameMultipleExt.ToString.ToUpper) Then
                If r.Cells(3).Value.ToString = "X" Then
                    button.Tag = n.ToString

                    Dim files As New Dictionary(Of Integer, List(Of String))
                    Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(1).Value.ToString + "\" + r.Cells(0).Value.ToString)
                    Button_ScreenshotFix_FillGatheredFiles(f_info.DirectoryName, f_info.Name + ".*", 0, files)

                    Dim panel As New TableLayoutPanel With {.RowCount = 4, .ColumnCount = files(0).Count, .BorderStyle = BorderStyle.FixedSingle}
                    panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                    panel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                    panel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns
                    panel.RowStyles.Clear()
                    panel.RowStyles.Add(New RowStyle(SizeType.Percent, 10))
                    panel.RowStyles.Add(New RowStyle(SizeType.Percent, 5))
                    panel.RowStyles.Add(New RowStyle(SizeType.Percent, 75))
                    panel.RowStyles.Add(New RowStyle(SizeType.Percent, 10))

                    panel.ColumnStyles.Clear()

                    Dim col = 0
                    For Each file In files(0)
                        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, CSng(Math.Floor(100 / files(0).Count))))

                        Dim format = ""
                        Dim p As New PictureBox With {.Dock = DockStyle.Fill, .SizeMode = PictureBoxSizeMode.Zoom}
                        Using img = Drawing.Image.FromFile(file)
                            p.Image = New Bitmap(img)
                            format = New ImageFormatConverter().ConvertToString(img.RawFormat)
                        End Using

                        Dim l1 As New Label With {.Text = file, .Anchor = AnchorStyles.Left, .Dock = DockStyle.Fill, .AutoSize = False}
                        l1.Padding = New Padding(0, 3, 0, 0)
                        panel.Controls.Add(l1) : panel.SetCellPosition(l1, New TableLayoutPanelCellPosition(col, 0))
                        Dim l2 As New Label With {.Text = format, .Anchor = AnchorStyles.Left, .TextAlign = ContentAlignment.MiddleLeft}
                        panel.Controls.Add(l2) : panel.SetCellPosition(l2, New TableLayoutPanelCellPosition(col, 1))
                        If format.ToUpper = IO.Path.GetExtension(file).Substring(1).ToUpper.Replace("JPG", "JPEG") Then
                            l2.BackColor = Color.LightGreen
                        Else
                            l2.BackColor = Color.Red
                        End If

                        panel.Controls.Add(p) : panel.SetCellPosition(p, New TableLayoutPanelCellPosition(col, 2))

                        Dim b = New Button With {.Text = "Use This", .Tag = file}
                        AddHandler b.Click, AddressOf Button_ScreenshotFix_UseThis_Click
                        panel.Controls.Add(b) : panel.SetCellPosition(b, New TableLayoutPanelCellPosition(col, 4))

                        col += 1
                    Next

                    Me.Controls.Add(panel)
                    panel.Location = New Point(10, 10)
                    panel.Size = New Size(Me.Width - 38, Me.Height - 55)
                    panel.BringToFront()

                    Dim b_skip = New Button With {.Text = "Skip", .Anchor = AnchorStyles.Top Or AnchorStyles.Right}
                    AddHandler b_skip.Click, AddressOf Button_ScreenshotFix_UseThis_Click
                    Me.Controls.Add(b_skip)
                    b_skip.Location = New Point(panel.Right - b_skip.Width - 10, panel.Top + 10)
                    b_skip.BringToFront()
                    Exit Sub
                End If
            End If
        Next

        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_ScreenshotFix_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_ScreenshotFix_ClickBG_OBSOLETE(sender As Object, e As DoWorkEventArgs)
        Dim checked_count = (From r As DataGridViewRow In DataGridView1.Rows Where r.Cells(3).Value.ToString = "X").Count

        Dim AskOverwriteForm As New FormA_MaintenanceDB_AskOverwrite
        Dim gathered_files As New Dictionary(Of Integer, List(Of String))
        Dim changed_files_remap As New Dictionary(Of String, String)

        Dim db_begin_was_called = False
        Dim db_screen_count_reseted = False

        Dim screen_num_field = ""
        If ComboBox2.Invoke(Function() ComboBox2.Text) IsNot Nothing Then
            screen_num_field = ComboBox2.Invoke(Function() ComboBox2.Text).ToString.Trim
            If screen_num_field.Contains(" ") Then screen_num_field = screen_num_field.Substring(0, screen_num_field.IndexOf(" ")).Trim
        End If


        For pass As Integer = 1 To 2
            Dim c As Integer = 0
            For Each r As DataGridViewRow In DataGridView1.Rows
                If r.Cells(3).Value.ToString = "X" Then
                    c += 1
                    Dim pass_str = "Pass " + pass.ToString + " of 2 "
                    If pass = 1 Then pass_str += "(Gathering files) - " Else pass_str += "(Fixing) - "

                    'Holes
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.Holes.ToString.ToUpper) Then
                        Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(1).Value.ToString + "\" + r.Cells(0).Value.ToString)
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing Holes - " + f_info.Name)

                        If pass = 1 Then
                            Button_ScreenshotFix_FillGatheredFiles(f_info.DirectoryName, f_info.Name + "*.*", r.Index, gathered_files)
                        ElseIf pass = 2 Then
                            Dim files As New Dictionary(Of Integer, String)
                            For Each f In gathered_files(r.Index).ToList
                                If changed_files_remap.ContainsKey(f.ToUpper) Then f = changed_files_remap(f.ToUpper)

                                Dim info = Button_ScreenshotFix_GetProductFromFileName(f)
                                'To avoid renaming 'The Game 2' when 'The Game' was selected
                                If Not info.product.ToUpper = f_info.Name.ToUpper Then Continue For
                                If info.isFirstSpecial OrElse info.suffix_num < 0 Then Continue For

                                files.Add(info.suffix_num, f)
                            Next

                            Dim n As Integer = 0
                            Dim keys = files.Keys.ToList : keys.Sort()
                            For Each k In keys
                                n += 1
                                Dim info = Button_ScreenshotFix_GetProductFromFileName(files(k))
                                If Not info.product.ToUpper = f_info.Name.ToUpper Then Continue For
                                If info.suffix_num = n Then Continue For
                                If info.suffix_num < n Then MsgBox("Something goes wrong: Trying to rename hole to GREATER number.") : Exit Sub
                                Dim product_corrected = info.product + info.suffix.Replace(info.suffix_num.ToString, n.ToString) + IO.Path.GetExtension(files(k))

                                Dim dest_f = f_info.DirectoryName + "\" + product_corrected
                                IO.File.Move(files(k), dest_f)
                                If Not changed_files_remap.ContainsKey(files(k).ToUpper) Then changed_files_remap.Add(files(k).ToUpper, dest_f) Else changed_files_remap(files(k).ToUpper) = dest_f
                            Next
                        End If
                    End If

                    'Wrong placed 'The'
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.WrongPlacedThe.ToString.ToUpper) Then
                        Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(1).Value.ToString + "\" + r.Cells(0).Value.ToString)
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing 'THE' - " + f_info.Name)

                        If pass = 1 Then
                            Button_ScreenshotFix_FillGatheredFiles(f_info.DirectoryName, f_info.Name + "*.*", r.Index, gathered_files)
                        ElseIf pass = 2 Then
                            For Each f In gathered_files(r.Index)
                                If changed_files_remap.ContainsKey(f.ToUpper) Then f = changed_files_remap(f.ToUpper)

                                Dim info = Button_ScreenshotFix_GetProductFromFileName(f)

                                'To avoid renaming 'The Game 2' when 'The Game' was selected
                                If Not info.product.ToUpper = f_info.Name.ToUpper Then Continue For
                                If Not info.product.ToUpper.StartsWith("THE ") Then MsgBox("Something really wrong: can't find 'The', when fixing 'The'.") : Exit Sub
                                Dim product_corrected = info.product.Substring(4).Trim + ", The" + info.suffix + IO.Path.GetExtension(f)

                                Dim dest_f = f_info.DirectoryName + "\" + product_corrected
                                IO.File.Move(f, dest_f)
                                If Not changed_files_remap.ContainsKey(f.ToUpper) Then changed_files_remap.Add(f.ToUpper, dest_f) Else changed_files_remap(f.ToUpper) = dest_f
                            Next
                        End If
                    End If

                    'Replace
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.Replace.ToString.ToUpper) Then

                    End If

                    'Wrong path
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.WrongPath.ToString.ToUpper) Then
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing Wrong Path - " + r.Cells(1).Value.ToString + "\" + r.Cells(0).Value.ToString)
                        Dim arr = r.Cells(4).Value.ToString.Split({"|"c}) '0 = type (WrongPath), 1 = expected path, 2 = found @ path
                        If arr.Count <> 3 Then MsgBox("Something wrong: Array count <> 3.") : Exit Sub

                        If pass = 1 Then
                            Dim f_info As New FileInfo(screenshotPath + "\" + arr(2))
                            Button_ScreenshotFix_FillGatheredFiles(f_info.DirectoryName, f_info.Name + "*.*", r.Index, gathered_files)
                        ElseIf pass = 2 Then
                            For Each f In gathered_files(r.Index)
                                If changed_files_remap.ContainsKey(f.ToUpper) Then f = changed_files_remap(f.ToUpper)

                                Dim f_info As New FileInfo(screenshotPath + "\" + arr(1))
                                Dim info = Button_ScreenshotFix_GetProductFromFileName(f)

                                'To avoid renaming 'The Game 2' when 'The Game' was selected
                                If Not info.product.ToUpper = f_info.Name.ToUpper Then Continue For

                                Dim dest_f = f_info.DirectoryName + "\" + IO.Path.GetFileName(f)
                                If IO.File.Exists(dest_f) Then
                                    Dim resp = AskOverwriteForm.ask(f, dest_f, screenshotPath)
                                    If resp = FormA_MaintenanceDB_AskOverwrite.resp.None Then Me.Invoke(Sub() AskOverwriteForm.ShowDialog(Me)) : resp = AskOverwriteForm.last_resp

                                    If resp = FormA_MaintenanceDB_AskOverwrite.resp.None Then Continue For
                                    If resp = FormA_MaintenanceDB_AskOverwrite.resp.UseSrc Then IO.File.Delete(dest_f)
                                    If resp = FormA_MaintenanceDB_AskOverwrite.resp.KeepDst Then Continue For
                                    If resp = FormA_MaintenanceDB_AskOverwrite.resp.KeepDstDeleteSrc Then IO.File.Delete(f) : Continue For
                                End If
                                If Not Directory.Exists(f_info.DirectoryName) Then Directory.CreateDirectory(f_info.DirectoryName)
                                IO.File.Move(f, dest_f)
                                If Not changed_files_remap.ContainsKey(f.ToUpper) Then changed_files_remap.Add(f.ToUpper, dest_f) Else changed_files_remap(f.ToUpper) = dest_f
                            Next
                        End If
                    End If

                    'Unused
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.Unused.ToString.ToUpper) Then

                    End If

                    'HTML Entities
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.HTMLEntities.ToString.ToUpper) Then
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing HTML Emtities - " + r.Cells(1).Value.ToString + "\" + r.Cells(0).Value.ToString)
                        Dim arr = r.Cells(4).Value.ToString.Split({"|"c}) '0 = type (WrongPath), 2 = found @ path
                        If arr.Count <> 2 Then MsgBox("Something wrong: Array count <> 3.") : Exit Sub

                        If pass = 1 Then
                            Dim f_info As New FileInfo(screenshotPath + "\" + arr(1))
                            Button_ScreenshotFix_FillGatheredFiles(f_info.DirectoryName, f_info.Name + "*.*", r.Index, gathered_files)
                        Else
                            For Each f In gathered_files(r.Index)
                                If changed_files_remap.ContainsKey(f.ToUpper) Then f = changed_files_remap(f.ToUpper)

                                Dim f_name = Path.GetFileNameWithoutExtension(f)
                                Dim f_name_new = HttpUtility.HtmlDecode(f_name)
                                If f_name <> f_name_new Then
                                    Dim f_new = Path.GetDirectoryName(f) + "\" + f_name_new + Path.GetExtension(f)
                                    IO.File.Move(f, f_new)

                                    If Not changed_files_remap.ContainsKey(f.ToUpper) Then changed_files_remap.Add(f.ToUpper, f_new) Else changed_files_remap(f.ToUpper) = f_new
                                End If
                            Next
                        End If
                    End If

                    'Found matched size
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.MatchedSize.ToString.ToUpper) Then

                    End If

                    'Screen count
                    If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.ScreenCount.ToString.ToUpper) Then
                        If pass = 2 Then
                            Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing Screenshot Count")

                            'Dim field = ComboBox2.Invoke(Function() ComboBox2.Text).ToString
                            'If field.Contains(" ") Then field = field.Substring(0, field.IndexOf(" ")).Trim

                            If Not screen_num_field = "" Then
                                If Not db_begin_was_called Then db.execute("BEGIN;") : db_begin_was_called = True
                                If Not db_screen_count_reseted Then db.execute("UPDATE main SET " + screen_num_field + " = 0") : db_screen_count_reseted = True
                                Dim arr = r.Cells(4).Value.ToString.Split({"|||"}, StringSplitOptions.None)
                                Dim sql = "UPDATE main SET " + screen_num_field + " = " + arr(2).Trim + " WHERE id = " + arr(1).Trim
                                db.execute(sql)
                            End If
                        End If
                    End If

                    If pass = 2 Then DataGridView1.Invoke(Sub() r.Cells(3).Value = "ok")
                End If
            Next
        Next

        If db_begin_was_called Then db.execute("COMMIT;")
    End Sub
    Private Sub Button_ScreenshotFix_ClickBG(sender As Object, e As DoWorkEventArgs)
        Dim c As Integer = 0
        Dim Step_Current = 1
        Dim Step_Count = CInt(GroupBox1.Invoke(Function() GroupBox1.Controls.OfType(Of CheckBox).Where(Function(chk) chk.Checked).Except({CheckBox11, CheckBox12, CheckBox13}).Count))
        If Step_Count = 0 Then Me.Invoke(Sub() MsgBox("No action selected.")) : Exit Sub

        Dim rows_to_process As New List(Of DataGridViewRow)

        'Holes
        c = 0
        rows_to_process.Clear()
        Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
        Label_status.Invoke(Sub() Label_status.Text = Step_String + " - Fixing Holes...")
        For Each r As DataGridViewRow In DataGridView1.Rows
            If r.Cells(3).Value.ToString = "X" Then
                If r.Cells(4).Value.ToString.ToUpper.StartsWith(screenshots_problems.Holes.ToString.ToUpper) Then
                    rows_to_process.Add(r)
                End If
            End If
        Next
        For Each r In rows_to_process
            c += 1
            Label_status.Invoke(Sub() Label_status.Text = Step_String + " - Fixing Holes... " + c.ToString + "/" + rows_to_process.Count.ToString)
            Fix_Holes(r.Cells(0).Value.ToString, r.Cells(1).Value.ToString, r.Cells(4).Value.ToString)
        Next

    End Sub
    Private Sub Button_ScreenshotFix_FillGatheredFiles(dir As String, mask As String, key As Integer, ref As Dictionary(Of Integer, List(Of String)))
        'Get all screenshots for the requested products
        ref.Add(key, New List(Of String))
        Dim files = IO.Directory.GetFiles(dir, mask, SearchOption.TopDirectoryOnly)
        For Each f In files
            ref(key).Add(f)
        Next
    End Sub
    Private Sub Button_ScreenshotFix_UseThis_Click(o As Object, e As EventArgs)
        Dim b = Me.Controls.OfType(Of Button)()(0)
        Dim t = Me.Controls.OfType(Of TableLayoutPanel)()(0)
        Dim b_pressed = DirectCast(o, Button)

        If Not b_pressed Is b Then 'If it's not 'SKIP' button
            For n As Integer = 0 To t.ColumnCount - 1
                Dim p = CType(t.GetControlFromPosition(n, 2), PictureBox)
                p.Image.Dispose() : p.Image = Nothing : p.Dispose() : t.Controls.Remove(p)

                Dim b_use = CType(t.GetControlFromPosition(n, 3), Button)
                If Not b_pressed Is b_use Then
                    Dim file = b_use.Tag.ToString
                    IO.File.Delete(file)
                End If
            Next
        End If

        b.Dispose()
        t.Dispose()
        Me.Controls.Remove(b)
        Me.Controls.Remove(t)
        Button_ScreenshotFix_Click(Button_ScreenshotFix, New EventArgs)
    End Sub
    'Fix routines - DB
    Private Sub Button_DBFix_Click(sender As Object, e As EventArgs) Handles Button_DBFix.Click
        DisableControlsAndCreateStatusLabel()
        Label_status.Text = "Prepare..."

        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_DBFix_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_DBFix_ClickBG(sender As Object, e As DoWorkEventArgs)
        Dim c As Integer = 0
        Dim Step_Current = 1
        Dim Step_Count = CInt(GroupBox2.Invoke(Function() GroupBox2.Controls.OfType(Of CheckBox).Where(Function(chk) chk.Checked).Count))
        If Step_Count = 0 Then Me.Invoke(Sub() MsgBox("No action selected.")) : Exit Sub

        Dim rows_to_process As New List(Of DataGridViewRow)

        c = 0
        rows_to_process.Clear()
        Step_String = Step_Current.ToString + " of " + Step_Count.ToString + ": "
        Label_status.Invoke(Sub() Label_status.Text = Step_String + " - Fixing Products with no category...")
        For Each r As DataGridViewRow In DataGridView2.Rows
            If r.Cells(2).Value.ToString = "X" Then
                If r.Cells(3).Value.ToString.ToUpper.StartsWith(db_problems.NoCategory.ToString.ToUpper) Then
                    rows_to_process.Add(r)
                End If
            End If
        Next

        db.execute("BEGIN;")
        For Each r In rows_to_process
            c += 1
            Label_status.Invoke(Sub() Label_status.Text = Step_String + " - Fixing Products with no category... " + c.ToString + "/" + rows_to_process.Count.ToString)

            Dim id = r.Cells(3).Value.ToString().Split({"|||"}, StringSplitOptions.None)(1).Trim()
            Dim sql = "INSERT INTO category (id_main, cat) VALUES (" + id + ", 'uncotegorized');"
            db.execute(sql)
        Next
        db.execute("COMMIT;")
    End Sub

    'Screenshot Export Button
    Private Sub Button_ScreenshotExport_Click(sender As Object, e As EventArgs) Handles Button_ScreenshotExport.Click
        Dim sw As New StreamWriter(".\Maintenance_Screenshots_Result.txt", False, System.Text.Encoding.UTF8)
        For Each r As DataGridViewRow In DataGridView1.Rows
            Dim valueList As New List(Of String)
            For Each c As DataGridViewCell In r.Cells
                valueList.Add(c.Value.ToString)
            Next
            sw.WriteLine(String.Join(";", valueList.ToArray))
        Next
        sw.Close()

        MsgBox("Exported as .\Maintenance_Screenshots_Result.txt")
    End Sub
    'Screenshot Replacement Add/Remove
    Private Sub Screen_Replacement_Button_Add_Click(sender As Object, e As EventArgs) Handles Screen_Replacement_Button_Add.Click
        Dim arr = ComboBox1.Text.Split({"->"}, StringSplitOptions.RemoveEmptyEntries)
        If arr.Count <> 2 Then MsgBox("Wrong replacement format. Should be ""MATCH -> REPLACEMENT""") : Exit Sub
        ComboBox1.Items.Add(ComboBox1.Text)
    End Sub
    Private Sub Screen_Replacement_Button_Remove_Click(sender As Object, e As EventArgs) Handles Screen_Replacement_Button_Remove.Click
        ComboBox1.Items.Remove(ComboBox1.Text)
    End Sub

    'Checking Sub Routines - Screenshots
    Private Sub Check_Holes()
        Dim c As Integer = 0
        Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking Holes...")

        Dim scr_dict As New Dictionary(Of String, List(Of Integer))
        Dim scr_normal_name As New Dictionary(Of String, String)
        For Each p_info In screenshotFilesParsed
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking Holes... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            If screenExtensionsArray.Contains(p_info.screenshot_ext.ToLower) Then
                If p_info.suffix_num >= 0 AndAlso Not p_info.isFirstSpecial Then
                    Dim fullPathU = p_info.screenshot_dir.ToUpper + "\" + p_info.product.ToUpper
                    If Not scr_normal_name.ContainsKey(fullPathU) Then
                        scr_normal_name.Add(fullPathU, p_info.screenshot_dir + "\" + p_info.product)
                        scr_dict.Add(fullPathU, New List(Of Integer))
                    End If
                    If Not scr_dict(fullPathU).Contains(p_info.suffix_num) Then scr_dict(fullPathU).Add(p_info.suffix_num)
                End If
            End If
        Next

        c = 0
        For Each kv In scr_dict
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking Holes - Adding results... " + c.ToString + "/" + scr_dict.Count.ToString)

            Dim max = kv.Value.Max
            If kv.Value.Count + Screenshot_Start_Ind <= max Then
                Dim product = scr_normal_name(kv.Key)
                Dim missing = Enumerable.Range(Screenshot_Start_Ind, kv.Value.Count).Except(kv.Value)
                Dim added_row_obj = DataGridView1.Invoke(Function() DataGridView1.Rows.Add({IO.Path.GetFileName(product), IO.Path.GetDirectoryName(product), "Found Hole @ " + String.Join(",", missing), "X", screenshots_problems.Holes.ToString + "|"}))

                ''If we have filled unused_screens array, then we can color used/unused screenshots to green/red colors
                If CheckBox6.Checked Then
                    Dim color = Drawing.Color.DarkGreen
                    If screenshotFiles_byProductPath_FindUnused.ContainsKey(kv.Key) Then color = Drawing.Color.DarkRed

                    Dim added_row = DataGridView1.Rows(DirectCast(added_row_obj, Integer))
                    added_row.Cells(0).Style.ForeColor = color
                End If
                ShowRowCount()
            End If
        Next
    End Sub
    Private Sub Check_SameName_DifferentExtension()
        Dim c As Integer = 0

        Dim info As New Dictionary(Of String, Dictionary(Of String, String))
        Dim info_problem_cache As New List(Of String)
        For Each p_info In screenshotFilesParsed
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking same name with different extensions... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            Dim pathU = p_info.screenshot_dir.ToUpper + "\" + p_info.product.ToUpper
            If Not info.ContainsKey(pathU) Then info.Add(pathU, New Dictionary(Of String, String))
            If Not info(pathU).ContainsKey(p_info.fileNameNoExt.ToUpper) Then
                info(pathU).Add(p_info.fileNameNoExt.ToUpper, p_info.screenshot_dir.ToUpper + "\" + p_info.fileNameNoExt + "|" + "." + p_info.screenshot_ext)
            Else
                info(pathU)(p_info.fileNameNoExt.ToUpper) += "|" + p_info.screenshot_ext
                Dim cache_key = pathU + "|" + p_info.fileNameNoExt.ToUpper
                If Not info_problem_cache.Contains(cache_key) Then info_problem_cache.Add(cache_key)
            End If
        Next

        c = 0
        For Each prob In info_problem_cache
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Parsing dictionary... " + c.ToString + "/" + info.Count.ToString)

            Dim arr = prob.Split({"|"c})
            Dim arr2 = info(arr(0))(arr(1)).Split({"|"c})
            Dim str = String.Join(",", arr2.Skip(1))
            DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({IO.Path.GetFileName(arr2(0)), IO.Path.GetDirectoryName(arr2(0)), "Same num with different extensions (" + str + ")", "X", screenshots_problems.SameNameMultipleExt.ToString + "|" + str}))
            ShowRowCount()
        Next
    End Sub
    Private Sub Check_Article_THE_At_Begining()
        Dim c As Integer = 0
        Dim problems As New List(Of String)
        For Each p_info In screenshotFilesParsed
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking 'The'... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            If p_info.fileNameNoExt.ToUpper.StartsWith("THE ") Then
                Dim pathU = p_info.screenshot_dir.ToUpper + "\" + p_info.product.ToUpper
                If Not problems.Contains(pathU) Then
                    problems.Add(pathU)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({p_info.product, p_info.screenshot_dir, "Article 'The' in the beginning", "X", screenshots_problems.WrongPlacedThe.ToString + "|"}))
                    ShowRowCount()
                End If
            End If
        Next
    End Sub
    Private Sub Check_Common_Replace_Substring()
        Dim c As Integer = 0
        Dim replacements As New Dictionary(Of String, String)
        For Each i As String In ComboBox1.Items
            Dim arr = i.Split({"->"}, StringSplitOptions.RemoveEmptyEntries)
            If arr.Count = 2 Then
                replacements.Add(arr(0).ToUpper.Trim, arr(1).Trim)
            End If
        Next

        Dim problems As New List(Of String)
        For Each p_info In screenshotFilesParsed
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking common replace... " + c.ToString + "/" + screenshotFiles.Count.ToString)
            For Each k In replacements.Keys
                Dim pathU = p_info.screenshot_dir.ToUpper + "\" + p_info.product.ToUpper
                If p_info.product.ToUpper.Contains(k) AndAlso Not problems.Contains(pathU) Then
                    problems.Add(pathU)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({p_info.product, p_info.screenshot_dir, "Found substring to replace (" + k + ")", "X", screenshots_problems.Replace.ToString + "|"}))
                    ShowRowCount()
                End If
            Next
        Next
    End Sub
    Private Sub Check_HTML_Entities_in_Filenames()
        Dim c As Integer = 0
        Dim regex As New Regex("\%[0-9a-z][0-9a-z]", RegexOptions.IgnoreCase)

        'TODO - check if exist, but in wrong folder
        'TODO - before renaming, need to check for forbidden symbols - * < > ?
        For Each p_info In screenshotFilesParsed
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking HTML entities in names... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            Dim productDecoded = Net.WebUtility.HtmlDecode(p_info.product)
            Dim matches = regex.Matches(productDecoded)
            For Each m As Capture In matches
                productDecoded = productDecoded.Replace(m.Value, Net.WebUtility.UrlDecode(m.Value))
            Next

            If p_info.product <> productDecoded Then
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({p_info.product, p_info.screenshot_dir, "Found HTML entity (i.e. &amp;) or URL encoded string (%2f)", "X", screenshots_problems.HTMLEntities.ToString + "|" + p_info.screenshot_dir + "\" + p_info.fileNameNoExt + "." + p_info.screenshot_ext}))
                ShowRowCount()
            End If
        Next
    End Sub
    Private Sub Check_Product_With_No_Screens()
        Dim c As Integer = 0
        For Each kv In screenshotNeeded_byProduct
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking DB entities with no screenshots... " + c.ToString + "/" + screenshotNeeded_byProduct.Count.ToString)

            Dim ScreensFound = False
            Dim scr_path = kv.Value.screens.Last().screenshot_dir + "\" + kv.Value.screens.Last().fileNameNoExt
            If screenshotFiles_byProductPath.ContainsKey(scr_path) Then
                Dim screens = screenshotFiles_byProductPath(scr_path).screens
                ScreensFound = screens.Count > 0

                If CheckBox12.Checked AndAlso ScreensFound AndAlso kv.Value.screens.Count > 1 Then
                    Dim special_screen_path = (kv.Value.screens(0).screenshot_dir + "\" + kv.Value.screens(0).fileNameNoExt).ToUpper()
                    ScreensFound = screens.Where(Function(s) (s.screenshot_dir + "\" + s.fileNameNoExt).ToUpper() <> special_screen_path).Count > 0
                End If
            End If

            If Not ScreensFound Then
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({IO.Path.GetFileName(kv.Key), IO.Path.GetDirectoryName(kv.Key), "Does not have any screens (Awaited: " + scr_path + ")", "-", ""}))
                ShowRowCount()
            End If
        Next
    End Sub
    Private Sub Check_Unused_Screens(Optional dont_fill_table = False)
        Dim c As Integer = 0
        For Each kv In screenshotNeeded_byProduct
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking unused screenshots... " + c.ToString + "/" + screenshotNeeded_byProduct.Count.ToString)

            Dim scr_path = kv.Value.screens.Last().screenshot_dir + "\" + kv.Value.screens.Last().fileNameNoExt
            If screenshotFiles_byProductPath_FindUnused.ContainsKey(scr_path) Then screenshotFiles_byProductPath_FindUnused.Remove(scr_path)
        Next

        If Not dont_fill_table And Not CheckBox13.Checked Then
            c = 0
            For Each kv In screenshotFiles_byProductPath_FindUnused
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking unused screenshots - Adding results... " + c.ToString + "/" + screenshotFiles_byProductPath_FindUnused.Count.ToString)

                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({IO.Path.GetFileName(kv.Key), IO.Path.GetDirectoryName(kv.Key), "Unused screenshot", "", screenshots_problems.Unused.ToString + "|"}))
                ShowRowCount()
            Next
        End If
    End Sub
    Private Sub Check_Screens_Count()
        Dim c As Integer = 0
        For Each kv In screenshotNeeded_byProduct
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking screenshots count... " + c.ToString + "/" + screenshotNeeded_byProduct.Count.ToString)

            Dim ScreensCount = 0
            Dim scr_path = kv.Value.screens.Last().screenshot_dir + "\" + kv.Value.screens.Last().fileNameNoExt
            If screenshotFiles_byProductPath.ContainsKey(scr_path) Then
                Dim screens = screenshotFiles_byProductPath(scr_path).screens
                ScreensCount = screens.Count

                If Not CheckBox11.Checked AndAlso ScreensCount > 0 AndAlso kv.Value.screens.Count > 1 Then
                    Dim special_screen_path = (kv.Value.screens(0).screenshot_dir + "\" + kv.Value.screens(0).fileNameNoExt).ToUpper()
                    ScreensCount = screens.Where(Function(s) (s.screenshot_dir + "\" + s.fileNameNoExt).ToUpper() <> special_screen_path).Count
                End If
            End If

            If ScreensCount > 0 Then
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({IO.Path.GetFileName(kv.Key), IO.Path.GetDirectoryName(kv.Key), "Screen count: " + ScreensCount.ToString, "X", screenshots_problems.ScreenCount.ToString + "|||" + kv.Value.db_product_id.ToString + "|||" + ScreensCount.ToString}))
                ShowRowCount()
            End If
        Next
    End Sub
    Private Sub Check_Wrong_Screens_Path()
        'Ensure we have correct UnusedScreenshots array
        If Not CheckBox6.Checked Then Check_Unused_Screens(True)

        Dim c As Integer = 0
        For Each kv In screenshotFiles_byProductPath_FindUnused
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Checking unused screenshots... " + c.ToString + "/" + screenshotNeeded_byProduct.Count.ToString)

            Dim product = kv.Value.screens(0).product
            If screenshotNeeded_byProduct.ContainsKey(product) Then
                Dim awaited_in = screenshotNeeded_byProduct(product).screens(0).screenshot_dir
                Dim found_in = kv.Value.screens(0).screenshot_dir

                Dim match = ""
                Dim exist = False
                Dim existing_size = 0
                Dim unneeded_size = 0
                For Each scr In kv.Value.screens
                    unneeded_size += New FileInfo(screenshotPath + "\" + scr.screenshot_dir + "\" + scr.fileNameNoExt + "." + scr.screenshot_ext).Length
                    Dim awaited_full_path = screenshotPath + "\" + awaited_in + "\" + scr.fileNameNoExt + "." + scr.screenshot_ext
                    If File.Exists(awaited_full_path) Then
                        exist = True
                        existing_size += New FileInfo(awaited_full_path).Length
                    Else
                        Exit For
                    End If
                Next
                If exist Then match = "(Awaited exists)"
                If unneeded_size = existing_size Then match += "(Awaited size match)"

                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({product, found_in, "Have screen in wrong path (Awaited: " + awaited_in + ", Found: " + found_in + ")" + match, "", screenshots_problems.WrongPath.ToString + "|" + awaited_in + "|" + found_in}))
                ShowRowCount()
            End If
        Next
    End Sub

    'Fixing Sub Routines - Screenshots
    Private Sub Fix_Holes(name As String, path As String, param As String)
        'TODO: Handle 'changed_files'
        Dim gathered_files As New Dictionary(Of Integer, List(Of String))
        Button_ScreenshotFix_FillGatheredFiles(screenshotPath + "\" + path, name + "*.*", 0, gathered_files)

        Dim files As New Dictionary(Of Integer, String)
        For Each f In gathered_files(0)
            Dim info = Button_ScreenshotFix_GetProductFromFileName(f)
            'To avoid renaming 'The Game 2' when 'The Game' was selected
            If Not info.product.ToUpper = name.ToUpper Then Continue For
            If info.isFirstSpecial OrElse info.suffix_num < Main_Form.Screenshot_Options.FirstIndex Then Continue For

            files.Add(info.suffix_num, f)
        Next

        Dim n As Integer = Main_Form.Screenshot_Options.FirstIndex - 1
        Dim keys = files.Keys.ToList : keys.Sort()
        For Each k In keys
            n += 1
            Dim info = Button_ScreenshotFix_GetProductFromFileName(files(k))
            If info.product.ToUpper <> name.ToUpper Then Continue For
            If info.suffix_num = n Then Continue For
            'TODO: Exit Sub is not enough, because the fixing process will continue. Need to add return codes
            If info.suffix_num < n Then MsgBox("Something goes wrong: Trying to rename hole to GREATER number.") : Exit Sub
            Dim product_corrected = info.product + info.suffix.Replace(info.suffix_num.ToString, n.ToString) + IO.Path.GetExtension(files(k))

            Dim dest_f = screenshotPath + "\" + path + "\" + product_corrected
            IO.File.Move(files(k), dest_f)
        Next

        'TODO: mark the handled row as "ok" and don't allow to check/uncheck it again
    End Sub
    Private Sub Fix_SameName_DifferentExtension()

    End Sub
    Private Sub Fix_Article_THE_At_Begining()

    End Sub
    Private Sub Fix_Common_Replace_Substring()

    End Sub
    Private Sub Fix_HTML_Entities_in_Filenames()

    End Sub
    Private Sub Fix_Unused_Screens(Optional dont_fill_table = False)

    End Sub
    Private Sub Fix_Screens_Count()

    End Sub
    Private Sub Fix_Wrong_Screens_Path()

    End Sub

    'DataGrid cell click/key down
    Private Sub DataGridView1_CellMouseUp(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseUp
        If e.RowIndex < 0 Then Exit Sub
        If e.ColumnIndex = 0 Then
            'Find game (product) in main list
            Dim game = DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString
            If game.Contains("\") Then game = game.Substring(game.LastIndexOf("\") + 1)
            Main_Form.TextBox1.Text = game

            'If we didn't find a games in db, matching the requested pattern, try different " - " resolving
            If Main_Form.ListBox1.Items.Count = 0 Then
                Dim r = db.queryReader("SELECT name FROM main WHERE name LIKE '" + game.Replace("'", "''").Replace(" - ", "%") + "'")
                If r.HasRows Then
                    r.Read()
                    Main_Form.TextBox1.Text = r.GetString(0)
                End If
            End If

            'If we have multiple games in db, matching the requested pattern, try to select the correct one
            If Main_Form.ListBox1.Items.Count > 1 Then
                For n As Integer = 0 To Main_Form.ListBox1.Items.Count() - 1
                    Dim drv = DirectCast(Main_Form.ListBox1.Items(n), DataRowView)
                    If drv.Item(1).ToString().ToUpper() = game.ToUpper() Then Main_Form.ListBox1.SelectedIndex = n : Exit Sub
                Next
            End If
        ElseIf e.ColumnIndex = 1 Then
            'Open screen folder
            Dim folder = screenshotPath + "\" + DataGridView1.Rows(e.RowIndex).Cells(1).Value.ToString
            Dim full_path = screenshotPath + "\" + DataGridView1.Rows(e.RowIndex).Cells(1).Value.ToString + "\" + DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString

            Dim files = Directory.GetFiles(folder, DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString + "*.*", SearchOption.TopDirectoryOnly)
            If files.Count = 0 Then
                Process.Start(folder)
            Else
                Process.Start("explorer.exe", "/select, """ + IO.Path.GetFullPath(files(0)) + """")
            End If
        ElseIf e.ColumnIndex = 3 Then
            DataGridView1_KeyDown(DataGridView1, New KeyEventArgs(Keys.Space))
        End If
    End Sub
    Private Sub DataGridView1_KeyDown(sender As Object, e As KeyEventArgs) Handles DataGridView1.KeyDown
        If DataGridView1.SelectedRows.Count <> 1 Then MsgBox("Nothing selected.") : Exit Sub
        If e.KeyCode = Keys.Space Or e.KeyCode = Keys.Enter Or e.KeyCode = Keys.Return Then
            Dim r = DataGridView1.SelectedRows(0)

            If e.KeyCode = Keys.Space Or e.KeyCode = Keys.Enter Then
                If r.Cells(3).Value.ToString.Trim = "" Then
                    r.Cells(3).Value = "X"
                ElseIf r.Cells(3).Value = "X" Then
                    r.Cells(3).Value = ""
                End If
            End If
            If e.KeyCode = Keys.Enter Or e.KeyCode = Keys.Return Then
                Dim ind = r.Index + 1
                If ind < DataGridView1.Rows.Count Then DataGridView1.Rows(ind).Selected = True
            End If
        End If
    End Sub
    Private Sub DataGridView1_SelectionChanged(sender As Object, e As EventArgs) Handles DataGridView1.SelectionChanged
        If DataGridView1.SelectedRows.Count = 0 Then Exit Sub
        If Not SyncBrowsingWithMainWindowToolStripMenuItem.Checked Then Exit Sub

        Dim r = DataGridView1.SelectedRows(0).Index
        Dim ee As New DataGridViewCellMouseEventArgs(0, r, 0, 0, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0))
        DataGridView1_CellMouseUp(DataGridView1, ee)
    End Sub
    'Check/Uncheck all Button
    Private Sub Screen_CheckAll_Button_Click(sender As Object, e As EventArgs) Handles Screen_CheckAll_Button.Click
        For Each r As DataGridViewRow In DataGridView1.Rows
            If r.Cells(3).Value.ToString.Trim = "" Then r.Cells(3).Value = "X"
        Next
    End Sub
    Private Sub Screen_UnCheckAll_Button_Click(sender As Object, e As EventArgs) Handles Screen_UnCheckAll_Button.Click
        For Each r As DataGridViewRow In DataGridView1.Rows
            If r.Cells(3).Value.ToString.Trim = "X" Then r.Cells(3).Value = ""
        Next
    End Sub

    Private Sub DisableControlsAndCreateStatusLabel()
        'TabControl1.Enabled = False
        GroupBox1.Enabled = False
        GroupBox2.Enabled = False
        Dim tab = TabControl1.SelectedTab
        Label_status.Width = Me.Width - 100
        Dim center = New Point(CInt(Me.Width / 2) - CInt(Label_status.Width / 2), CInt(Me.Height / 2) - CInt(Label_status.Height / 2))
        tab.Controls.Add(Label_status)
        Label_status.Location = center
        Label_status.BringToFront()
    End Sub
    Private Sub ReEnableControlsAndHideStatusLabel()
        Dim tab = TabControl1.SelectedTab
        tab.Controls.Remove(Label_status)
        'TabControl1.Enabled = True
        GroupBox1.Enabled = True
        GroupBox2.Enabled = True
    End Sub
    Private Sub ShowRowCount(Optional force As Boolean = False, Optional dataGridIndex As Integer = 0)
        Dim dataGrid = DataGridView1
        If dataGridIndex = 1 Then dataGrid = DataGridView2

        If force Then
            Me.Invoke(Sub() dataGrid.ResumeLayout())
            Me.Invoke(Sub() Label_Count.Text = "Count: " + dataGrid.Rows.Count.ToString)
            Me.Invoke(Sub() TabPage_Screenshots.Text = "Screenshots (" + dataGrid.Rows.Count.ToString + ")")
        Else
            If dataGrid.Rows.Count Mod 30 = 0 Then
                Me.Invoke(Sub() Label_Count.Text = "Count: " + dataGrid.Rows.Count.ToString)
                Me.Invoke(Sub() TabPage_Screenshots.Text = "Screenshots (" + dataGrid.Rows.Count.ToString + ")")

                'ShowRowCount(True)
                'Me.Invoke(Sub() DataGridView1.SuspendLayout())
            End If
        End If
    End Sub

    'Menu
    Private Sub SyncBrowsingWithMainWindowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SyncBrowsingWithMainWindowToolStripMenuItem.Click
        SyncBrowsingWithMainWindowToolStripMenuItem.Checked = Not SyncBrowsingWithMainWindowToolStripMenuItem.Checked
        If SyncBrowsingWithMainWindowToolStripMenuItem.Checked Then
            DataGridView1_SelectionChanged(DataGridView1, New EventArgs)
        End If
    End Sub

    'OBSOLETE CODE - check Products with no screens / Unused Screens / Screen count / Wrong paths
    Private Sub OBSOLETE_Check()
        Dim c = 0
        Dim products As New Dictionary(Of String, String)
        Dim products_with_paths_FindUnused As New Dictionary(Of String, String)
        Dim products_with_paths_ScreenCount As New Dictionary(Of String, Integer)
        Dim products_wrong_paths As New Dictionary(Of String, String)
        For Each f In screenshotFiles
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Normalizing screenshots names... " + c.ToString + "/" + screenshotFiles.Count.ToString)

            Dim i = Button_ScreenshotFix_GetProductFromFileName(f)
            Dim productWPath = IO.Path.GetDirectoryName(f) + "\" + i.product
            If Not products.ContainsKey(i.product.ToUpper) Then products.Add(i.product.ToUpper, productWPath)
            If Not products_with_paths_ScreenCount.ContainsKey(productWPath.ToUpper) Then
                products_with_paths_FindUnused.Add(productWPath.ToUpper, productWPath)
                products_with_paths_ScreenCount.Add(productWPath.ToUpper, 0)
            End If
            If CheckBox11.Checked OrElse Not i.isFirstSpecial Then products_with_paths_ScreenCount(productWPath.ToUpper) += 1
        Next

        c = 0
        Label_status.Invoke(Sub() Label_status.Text = Step_String + "Getting all products from database...")
        Dim db_prod_count = db.queryDataset("SELECT count(id) FROM main").Rows(0).Item(0).ToString
        Dim sql = "SELECT main.id, main.name, group_concat(category.cat) "
        If Main_Form.type = catalog_type.games Then sql += ", data_num1 "
        sql += "FROM main "
        sql += "JOIN category ON main.id = category.id_main "
        sql += "GROUP BY main.id, main.name"
        Dim db_reader = db.queryReader(sql)
        Do While db_reader.Read
            c += 1
            If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_String + "Finding products with no screens... " + c.ToString + "/" + db_prod_count)

            Dim cat = db_reader.Item(2).ToString.Replace(",", vbCrLf).Replace("&", "&&")
            Dim scr As String = "", scr_pathU As String = ""
            If Main_Form.type = catalog_type.games Then
                Dim year = db_reader.Item(3).ToString
                scr = Main_Form.getScreen(db_reader.Item("name").ToString, cat, 1, year, True)
            Else
                scr = Main_Form.getScreen(db_reader.Item("name").ToString, cat, 1, "", True)
            End If

            Dim scr_path = IO.Path.GetDirectoryName(scr)
            scr_path = scr_path.Substring(screenshotPath.Length)
            If scr_path.StartsWith("\") Then scr_path = scr_path.Substring(1)
            scr_pathU = scr_path.ToUpper

            scr = IO.Path.GetFileName(scr)
            If scr.ToUpper.EndsWith("BOX") Then scr = scr.Substring(0, scr.Length - 3)
            If scr.EndsWith("_") Then scr = scr.Substring(0, scr.Length - 1)
            Dim scrU = scr.ToUpper

            'TEST
            'If r.Item("name").ToString.ToUpper.Contains("World Karate Championship".ToUpper) Then
            'scr = scr
            'End If
            'TEST

            Dim ScreensFound = products_with_paths_ScreenCount.ContainsKey(scr_pathU + "\" + scrU)
            ScreensFound = ScreensFound AndAlso products_with_paths_ScreenCount(scr_pathU + "\" + scrU) > 0
            If Not ScreensFound Then
                If products.ContainsKey(scrU) Then
                    If Not products_wrong_paths.ContainsKey(products(scrU).ToUpper) Then
                        products_wrong_paths.Add(products(scrU).ToUpper, db_reader.Item("name") + "|||" + scr_path + "\" + scr + "|||" + products(scrU))
                    End If
                End If
                If CheckBox5.Checked Then
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({db_reader.Item("name"), "", "Does not have any screens (Awaited: " + scr_path + "\" + scr + ")", "-", ""}))
                End If
            End If
            If CheckBox8.Checked And ScreensFound Then
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({db_reader.Item("name"), "", "Screen count: " + products_with_paths_ScreenCount(scr_pathU + "\" + scrU).ToString, "X", screenshots_problems.ScreenCount.ToString + "|||" + db_reader.GetInt32(0).ToString + "|||" + products_with_paths_ScreenCount(scr_pathU + "\" + scrU).ToString}))
            End If

            'Unused screenshots
            If products_with_paths_FindUnused.ContainsKey(scr_pathU + "\" + scrU) Then products_with_paths_FindUnused.Remove(scr_pathU + "\" + scrU)
        Loop

        'Show screens with wrong path
        If CheckBox10.Checked Then
            For Each kv In products_wrong_paths
                If products_with_paths_FindUnused.ContainsKey(kv.Key) Then
                    Dim arr = kv.Value.Split({"|||"}, StringSplitOptions.None)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({arr(0), "", "Have screen in wrong path (Awaited: " + arr(1) + ", Found: " + arr(2) + ")", "", screenshots_problems.WrongPath.ToString + "|" + arr(1) + "|" + arr(2)}))
                End If
            Next
        End If

        'Show unused screenshots
        If CheckBox6.Checked Then
            For Each kv In products_with_paths_FindUnused
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({kv.Value, "", "Unused screenshot", "", screenshots_problems.Unused.ToString + "|"}))
            Next
        End If
    End Sub







    'Private Sub DataGridView1_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles DataGridView1.CellPainting
    '    If e.RowIndex = -1 Then
    '        Dim header = DataGridView1.Columns(e.ColumnIndex).HeaderCell
    '        Dim state = header.GetType().GetProperty("ButtonState", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
    '        Dim value = DirectCast(state.GetValue(header), ButtonState)
    '        If value <> ButtonState.Normal Then
    '            value = value
    '        End If

    '        'e.Handled = True
    '    End If
    'End Sub
    'Private Sub DataGridView1_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles DataGridView1.CellPainting
    '    'Source of datagridview column header - https://referencesource.microsoft.com/#system.windows.forms/winforms/managed/system/winforms/DataGridViewColumnHeaderCell.cs
    '    'Painting order - border, background, content
    '    If e.RowIndex = -1 Then
    '        If DataGridView1.SelectedColumns.Count > 0 AndAlso DataGridView1.SelectedColumns(0).Index = e.ColumnIndex Then
    '            Dim r = DataGridView1.SelectedRows(0).Index
    '            DataGridView1.ClearSelection()
    '            e.Paint(e.ClipBounds, e.PaintParts)
    '            DataGridView1.Rows(r).Selected = True

    '            'e.State = e.State And Not DataGridViewElementStates.Selected
    '            'e.PaintBackground(e.ClipBounds, False)

    '            'e.Paint(e.ClipBounds, DataGridViewPaintParts.Border)
    '            'Dim v_hdr = VisualStyles.VisualStyleElement.Header.Item.Normal
    '            'Dim vsr = New VisualStyles.VisualStyleRenderer(v_hdr)
    '            'vsr.SetParameters(v_hdr.ClassName, v_hdr.Part, 0)
    '            'vsr.DrawBackground(e.Graphics, e.CellBounds, Rectangle.Truncate(e.Graphics.ClipBounds))
    '            'e.Paint(e.ClipBounds, DataGridViewPaintParts.All And Not DataGridViewPaintParts.Background And Not DataGridViewPaintParts.Border)

    '            e.Handled = True
    '        End If
    '    End If
    'End Sub
End Class