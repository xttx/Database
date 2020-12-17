Imports System.ComponentModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web

'TODO: Disable datagridview column headers highlight
'TODO: Lock TabControl, but not disable it (to allow scroll the table while scanning)
'TODO: Realtime count update
'TODO: Open screen folder by dblclick on a table row
'TODO: Screen-ShowWrongPatch won't detect boxes

'---------------------------------------------------------------------------------------------
'DONE: Split ScreenNotFound and ScreenAtWrongPath into 2 separate options
'DONE: Check if screen is unused, before add it to wrong path
'DONE: Sync table browsing (selection change) to main form game list

Public Class FormA_MaintenanceDB
    Dim screenshotPath As String = ""
    Dim screenshotList As New List(Of String)

    Dim Label_status As New Label With {.AutoSize = False, .TextAlign = ContentAlignment.MiddleCenter, .Size = New Size(550, 70)}

    Structure productInfo
        Dim product As String
        Dim suffix As String
        Dim fileNameNoExt As String
        Dim isBox As Boolean
        Dim suffix_num As Integer
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

    Public Sub New(screen_path As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Label_status.Font = New Font(Label_status.Font.FontFamily, 12)
        Label_status.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        screenshotPath = screen_path

        'Dim ini As IniFileApi = Form1.ini
        'For i As Integer = 0 To fieldTypeArr.Count - 1
        '    For n = 1 To fieldCountArr(i)
        '        Dim fName = ini.IniReadValue("Interface", fieldTypeArr(i) + n.ToString)
        '        If fieldTypeArr(i).ToUpper.Contains("_NUM") Then
        '            Dim fld_std = "data" + fieldTypeArr(i).Substring(fieldTypeArr(i).IndexOf("_")).ToLower
        '            fld_std = fld_std + n.ToString
        '            If fName.Trim <> "" Then fld_std = fld_std + " (" + fName + ")"
        '            ComboBox2.Items.Add(fld_std)
        '        End If
        '    Next
        'Next

        For Each fi In Fields
            If fi.DBname.ToUpper.Contains("_NUM") Then
                If fi.name.Trim <> "" Then
                    ComboBox2.Items.Add(fi.DBname + " (" + fi.name + ")")
                Else
                    ComboBox2.Items.Add(fi.DBname)
                End If
            End If
        Next
    End Sub

    Private Sub Button_ScreenshotCheck_Click(sender As Object, e As EventArgs) Handles Button_ScreenshotCheck.Click
        If screenshotPath.Trim = "" Then MsgBox("Screenshot path not set.") : Exit Sub
        If Not My.Computer.FileSystem.DirectoryExists(screenshotPath) Then MsgBox("Screenshot path not exist.") : Exit Sub

        Label_status.Text = "Prepare..."
        DataGridView1.Rows.Clear()
        DisableControlsAndCreateStatusLabel()
        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button_ScreenshotCheck_ClickBG
        AddHandler bg.RunWorkerCompleted, AddressOf ReEnableControlsAndHideStatusLabel
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button_ScreenshotCheck_ClickBG(sender As Object, e As DoWorkEventArgs)
        screenshotList.Clear()
        Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(screenshotPath)

        Dim Step_Count = CInt(GroupBox1.Invoke(Function() GroupBox1.Controls.OfType(Of CheckBox).Where(Function(c) c.Checked).Count)) + 1
        If CheckBox9.Checked Then Step_Count -= 1
        Dim Step_Current = 2

        'Checking Holes
        If CheckBox1.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking Holes...")

            Dim scr_dict As New Dictionary(Of String, List(Of Integer))
            Dim scr_normal_name As New Dictionary(Of String, String)
            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking Holes... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim dir = IO.Path.GetDirectoryName(f)
                Dim fileExt = IO.Path.GetExtension(f).Replace(".", "")
                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)
                If Not fileName.ToUpper.EndsWith("_BOX") AndAlso screenExtensionsArray.Contains(fileExt.ToLower) Then
                    'TODO: this won't work well with daz screens, because they have number right after product name without separation,
                    ' and can have TshirtForV41, TshirtForV42, TshirtForV43 - number will be 41,42,43 and not 1,2,3 as expected
                    Dim p_info = Button_ScreenshotFix_GetProductFromFileName(f)
                    If p_info.suffix_num > 0 AndAlso Not p_info.isBox Then
                        Dim fullPathU = dir.ToUpper + "\" + p_info.product.ToUpper
                        If Not scr_normal_name.ContainsKey(fullPathU) Then
                            scr_normal_name.Add(fullPathU, dir + "\" + p_info.product)
                            scr_dict.Add(fullPathU, New List(Of Integer))
                        End If
                        If Not scr_dict(fullPathU).Contains(p_info.suffix_num) Then scr_dict(fullPathU).Add(p_info.suffix_num)
                    End If
                    'Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                    'If matchGroups(2).Value.Trim <> "" Then
                    '    Dim product = matchGroups(1).Value
                    '    Dim num = CInt(matchGroups(2).Value)
                    '    Dim fullPathU = dir.ToUpper + "\" + product.ToUpper
                    '    If Not scr_normal_name.ContainsKey(fullPathU) Then
                    '        scr_normal_name.Add(fullPathU, dir + "\" + product)
                    '        scr_dict.Add(fullPathU, New List(Of Integer))
                    '    End If
                    '    If Not scr_dict(fullPathU).Contains(num) Then scr_dict(fullPathU).Add(num)
                    'End If
                End If
            Next

            c = 0
            For Each kv In scr_dict
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Parsing dictionary... " + c.ToString + "/" + scr_dict.Count.ToString)

                Dim max = kv.Value.Max
                If kv.Value.Count < max Then
                    Dim product = scr_normal_name(kv.Key)
                    If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)
                    Dim missing = Enumerable.Range(1, kv.Value.Count).Except(kv.Value)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({product, "Found Hole @ " + String.Join(",", missing), "X", screenshots_problems.Holes.ToString + "|"}))
                End If
            Next

            Step_Current += 1
        End If

        'Checking same name with different extensions
        If CheckBox2.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            Dim info As New Dictionary(Of String, Dictionary(Of String, String))
            Dim info_problem_cache As New List(Of String)
            Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking same name with different extensions...")
            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking same name with different extensions... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim dir = IO.Path.GetDirectoryName(f)
                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)

                Dim product As String = ""
                Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                If matchGroups.Count < 2 Then product = fileName Else product = matchGroups(1).Value
                If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)

                Dim pathU = dir.ToUpper + "\" + product.ToUpper
                If Not info.ContainsKey(pathU) Then info.Add(pathU, New Dictionary(Of String, String))
                If Not info(pathU).ContainsKey(fileName.ToUpper) Then
                    info(pathU).Add(fileName.ToUpper, dir.ToUpper + "\" + fileName + "|" + IO.Path.GetExtension(f))
                Else
                    info(pathU)(fileName.ToUpper) += "|" + IO.Path.GetExtension(f)
                    Dim cache_key = pathU + "|" + fileName.ToUpper
                    If Not info_problem_cache.Contains(cache_key) Then info_problem_cache.Add(cache_key)
                End If
            Next

            c = 0
            For Each prob In info_problem_cache
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Parsing dictionary... " + c.ToString + "/" + info.Count.ToString)

                Dim arr = prob.Split({"|"c})
                Dim arr2 = info(arr(0))(arr(1)).Split({"|"c})
                Dim str = String.Join(",", arr2.Skip(1))
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({arr2(0), "Same num with different extensions (" + str + ")", "X", screenshots_problems.SameNameMultipleExt.ToString + "|" + str}))
            Next

            Step_Current += 1
        End If

        'Convert "The Screen" to "Screen, The"
        If CheckBox3.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            Dim problems As New List(Of String)
            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking 'The'... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim dir = IO.Path.GetDirectoryName(f)
                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)
                If fileName.ToUpper.StartsWith("THE ") Then
                    Dim product As String = ""
                    Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                    If matchGroups.Count < 2 Then product = fileName Else product = matchGroups(1).Value
                    If product.ToUpper.EndsWith("BOX") Then product = product.Substring(0, product.Length - 3)
                    If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)
                    Dim pathU = dir.ToUpper + "\" + product.ToUpper
                    If Not problems.Contains(pathU) Then
                        problems.Add(pathU)
                        DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({dir + "\" + product, "Article 'The' in the beginning", "X", screenshots_problems.WrongPlacedThe.ToString + "|"}))
                    End If
                End If
            Next

            Step_Current += 1
        End If

        'Common replace
        If CheckBox4.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            Dim replacements As New Dictionary(Of String, String)
            For Each i As String In ComboBox1.Items
                Dim arr = i.Split({"->"}, StringSplitOptions.RemoveEmptyEntries)
                If arr.Count = 2 Then
                    replacements.Add(arr(0).ToUpper.Trim, arr(1).Trim)
                End If
            Next

            Dim problems As New List(Of String)
            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking common replace... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim dir = IO.Path.GetDirectoryName(f)
                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)
                Dim product As String = ""
                Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                If matchGroups.Count < 2 Then product = fileName Else product = matchGroups(1).Value
                If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)
                Dim pathU = dir.ToUpper + "\" + product.ToUpper

                For Each k In replacements.Keys
                    If product.ToUpper.Contains(k) AndAlso Not problems.Contains(pathU) Then
                        problems.Add(pathU)
                        DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({dir + "\" + product, "Found substring to replace (" + k + ")", "X", screenshots_problems.Replace.ToString + "|"}))
                    End If
                Next
            Next

            Step_Current += 1
        End If

        'Products with no screens / Unused Screens / Screen count / Wrong paths
        Dim mainForm = DirectCast(Me.Owner, Form1)
        If CheckBox5.Checked Or CheckBox6.Checked Or CheckBox8.Checked Or CheckBox10.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "
            Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Normilizing screenshots names...")

            Dim products As New Dictionary(Of String, String)
            Dim products_with_paths_FindUnused As New Dictionary(Of String, String)
            Dim products_with_paths_ScreenCount As New Dictionary(Of String, Integer)
            Dim products_wrong_paths As New Dictionary(Of String, String)
            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Normilizing screenshots names... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim isBox As Boolean = False
                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)
                Dim product As String = ""
                Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                If matchGroups.Count < 2 Then product = fileName Else product = matchGroups(1).Value
                'If product.ToUpper.EndsWith("BOX") Then Continue For
                If product.ToUpper.EndsWith("BOX") Then product = product.Substring(0, product.Length - 3) : isBox = True
                If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)
                Dim productWPath = IO.Path.GetDirectoryName(f) + "\" + product
                If Not products.ContainsKey(product.ToUpper) Then products.Add(product.ToUpper, productWPath)
                If Not products_with_paths_ScreenCount.ContainsKey(productWPath.ToUpper) Then
                    products_with_paths_FindUnused.Add(productWPath.ToUpper, productWPath)
                    products_with_paths_ScreenCount.Add(productWPath.ToUpper, 0)
                End If
                If Not isBox Then products_with_paths_ScreenCount(productWPath.ToUpper) += 1
            Next
            Dim db_prod_count = db.queryDataset("SELECT count(id) FROM main").Rows(0).Item(0).ToString

            c = 0
            Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Getting all products from database...")
            Dim sql = "SELECT main.id, main.name, group_concat(category.cat) "
            If mainForm.type = catalog_type.games Then sql += ", data_num1 "
            sql += "FROM main "
            sql += "JOIN category ON main.id = category.id_main "
            sql += "GROUP BY main.id, main.name"
            Dim r = db.queryReader(sql)
            Do While r.Read
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Finding products with no screens... " + c.ToString + "/" + db_prod_count)

                Dim cat = r.Item(2).ToString.Replace(",", vbCrLf).Replace("&", "&&")
                Dim scr As String = "", scr_pathU As String = ""
                If mainForm.type = catalog_type.games Then
                    Dim year = r.Item(3).ToString
                    scr = mainForm.getScreen(r.Item("name").ToString, cat, 1, year, True)
                Else
                    scr = mainForm.getScreen(r.Item("name").ToString, cat, 1, "", True)
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
                            products_wrong_paths.Add(products(scrU).ToUpper, r.Item("name") + "|||" + scr_path + "\" + scr + "|||" + products(scrU))
                        End If
                    End If
                    If CheckBox5.Checked Then
                        DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({r.Item("name"), "Does not have any screens (Awaited: " + scr_path + "\" + scr + ")", "-", ""}))
                    End If
                End If
                If CheckBox8.Checked And ScreensFound Then
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({r.Item("name"), "Screen count: " + products_with_paths_ScreenCount(scr_pathU + "\" + scrU).ToString, "X", screenshots_problems.ScreenCount.ToString + "|||" + r.GetInt32(0).ToString + "|||" + products_with_paths_ScreenCount(scr_pathU + "\" + scrU).ToString}))
                End If

                'Unused screenshots
                If products_with_paths_FindUnused.ContainsKey(scr_pathU + "\" + scrU) Then products_with_paths_FindUnused.Remove(scr_pathU + "\" + scrU)
            Loop

            'Show screens with wrong path
            If CheckBox10.Checked Then
                For Each kv In products_wrong_paths
                    If products_with_paths_FindUnused.ContainsKey(kv.Key) Then
                        Dim arr = kv.Value.Split({"|||"}, StringSplitOptions.None)
                        DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({arr(0), "Have screen in wrong path (Awaited: " + arr(1) + ", Found: " + arr(2) + ")", "", screenshots_problems.WrongPath.ToString + "|" + arr(1) + "|" + arr(2)}))
                    End If
                Next
            End If

            'Show unused screenshots
            If CheckBox6.Checked Then
                For Each kv In products_with_paths_FindUnused
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({kv.Value, "Unused screenshot", "", screenshots_problems.Unused.ToString + "|"}))
                Next
            End If

            If CheckBox5.Checked Then Step_Current += 1
            If CheckBox6.Checked Then Step_Current += 1
            If CheckBox8.Checked Then Step_Current += 1
            If CheckBox10.Checked Then Step_Current += 1
        End If

        'HTML Entities in names
        If CheckBox7.Checked Then
            Dim c As Integer = 0
            Dim Step_Str = Step_Current.ToString + " of " + Step_Count.ToString + ": "

            For Each f In screenshotList
                c += 1
                If c Mod 300 = 0 Then Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Checking HTML entities in names... " + c.ToString + "/" + screenshotList.Count.ToString)

                Dim fileName = IO.Path.GetFileNameWithoutExtension(f)
                Dim product As String = ""
                Dim matchGroups = Regex.Match(fileName, "(.+?)(\d+$)").Groups
                If matchGroups.Count < 2 Then product = fileName Else product = matchGroups(1).Value
                If product.EndsWith("_") Then product = product.Substring(0, product.Length - 1)

                'TODO - check if exist, but in wrong folder
                Dim productDecoded = Net.WebUtility.HtmlDecode(product)
                'productDecoded = Net.WebUtility.UrlDecode(productDecoded) - This will give too much false positive
                If product <> productDecoded Then
                    'DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({product, "Found HTML entity (&amp;) or URL encoded string (%2f)"}))
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({product, "Found HTML entity (i.e. &amp;)", "X", screenshots_problems.HTMLEntities.ToString + "|" + f}))
                End If
            Next

            Step_Current += 1
        End If

        Label_status.Invoke(Sub() Label_status.Text = "Free memory...")
        screenshotList.Clear()
        GC.Collect()
        GC.WaitForPendingFinalizers()

        Dim listNotEmpty = DataGridView1.Rows.Count > 0
        Me.Invoke(Sub() Label_Count.Text = "Count: " + DataGridView1.Rows.Count.ToString)
        Me.Invoke(Sub() Button_ScreenshotFix.Enabled = listNotEmpty)
        Me.Invoke(Sub() Button_ScreenshotExport.Enabled = listNotEmpty)
        Me.Invoke(Sub() Screen_CheckAll_Button.Enabled = listNotEmpty)
        Me.Invoke(Sub() Screen_UnCheckAll_Button.Enabled = listNotEmpty)
    End Sub
    Private Sub Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(path As String)
        Dim Step_Count = CInt(GroupBox1.Invoke(Function() GroupBox1.Controls.OfType(Of CheckBox).Where(Function(c) c.Checked).Count)) + 1
        If CheckBox9.Checked Then Step_Count -= 1
        Dim Step_Str = "1 of " + Step_Count.ToString + ": "

        Label_status.Invoke(Sub() Label_status.Text = Step_Str + "Scanning " + path + " (" + screenshotList.Count.ToString + ")")
        For Each f In IO.Directory.GetFiles(path, "*.*", IO.SearchOption.TopDirectoryOnly)
            Dim scr = f.Substring(screenshotPath.Length)
            If scr.StartsWith("\") Then scr = scr.Substring(1)
            screenshotList.Add(scr)
        Next

        'Check size
        If CheckBox9.Checked Then
            Dim req_size = CInt(TextBox1.Text)
            Dim fi_arr = New DirectoryInfo(path).GetFiles("*.*")
            For Each fi In fi_arr
                If fi.Length = req_size Then
                    Dim p = fi.FullName.Substring(screenshotPath.Length)
                    If p.StartsWith("\") Then p = p.Substring(1)
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({p, "Match requested size"}))
                End If
            Next
        End If


        For Each d In IO.Directory.GetDirectories(path, "*.*", IO.SearchOption.TopDirectoryOnly)
            Button_ScreenshotCheck_ClickBG_CreateScreenListRecur(d)
        Next
    End Sub

    Private Sub Button_ScreenshotFix_Click(sender As Object, e As EventArgs) Handles Button_ScreenshotFix.Click
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
            If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.SameNameMultipleExt.ToString.ToUpper) Then
                If r.Cells(2).Value.ToString = "X" Then
                    button.Tag = n.ToString

                    Dim files As New Dictionary(Of Integer, List(Of String))
                    Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(0).Value.ToString)
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
    Private Sub Button_ScreenshotFix_ClickBG(sender As Object, e As DoWorkEventArgs)
        Dim checked_count = (From r As DataGridViewRow In DataGridView1.Rows Where r.Cells(2).Value.ToString = "X").Count

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
                If r.Cells(2).Value.ToString = "X" Then
                    c += 1
                    Dim pass_str = "Pass " + pass.ToString + " of 2 "
                    If pass = 1 Then pass_str += "(Gathering files) - " Else pass_str += "(Fixing) - "

                    'Holes
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.Holes.ToString.ToUpper) Then
                        Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(0).Value.ToString)
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
                                If info.isBox OrElse info.suffix_num < 0 Then Continue For

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
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.WrongPlacedThe.ToString.ToUpper) Then
                        Dim f_info As New FileInfo(screenshotPath + "\" + r.Cells(0).Value.ToString)
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
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.Replace.ToString.ToUpper) Then

                    End If

                    'Wrong path
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.WrongPath.ToString.ToUpper) Then
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing Wrong Path - " + r.Cells(0).Value.ToString)
                        Dim arr = r.Cells(3).Value.ToString.Split({"|"c}) '0 = type (WrongPath), 1 = expected path, 2 = found @ path
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
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.Unused.ToString.ToUpper) Then

                    End If

                    'HTML Entities
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.HTMLEntities.ToString.ToUpper) Then
                        Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing HTML Emtities - " + r.Cells(0).Value.ToString)
                        Dim arr = r.Cells(3).Value.ToString.Split({"|"c}) '0 = type (WrongPath), 2 = found @ path
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
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.MatchedSize.ToString.ToUpper) Then

                    End If

                    'Screen count
                    If r.Cells(3).Value.ToString.ToUpper.StartsWith(screenshots_problems.ScreenCount.ToString.ToUpper) Then
                        If pass = 2 Then
                            Label_status.Invoke(Sub() Label_status.Text = pass_str + c.ToString + " / " + checked_count.ToString + " - Fixing Screenshot Count")

                            'Dim field = ComboBox2.Invoke(Function() ComboBox2.Text).ToString
                            'If field.Contains(" ") Then field = field.Substring(0, field.IndexOf(" ")).Trim

                            If Not screen_num_field = "" Then
                                If Not db_begin_was_called Then db.execute("BEGIN;") : db_begin_was_called = True
                                If Not db_screen_count_reseted Then db.execute("UPDATE main SET " + screen_num_field + " = 0") : db_screen_count_reseted = True
                                Dim arr = r.Cells(3).Value.ToString.Split({"|||"}, StringSplitOptions.None)
                                Dim sql = "UPDATE main SET " + screen_num_field + " = " + arr(2).Trim + " WHERE id = " + arr(1).Trim
                                db.execute(sql)
                            End If
                        End If
                    End If

                    If pass = 2 Then DataGridView1.Invoke(Sub() r.Cells(2).Value = "ok")
                End If
            Next
        Next

        If db_begin_was_called Then db.execute("COMMIT;")
    End Sub
    Private Function Button_ScreenshotFix_GetProductFromFileName(f As String) As productInfo
        Dim i As New productInfo With {.suffix = "", .suffix_num = -1, .isBox = False}

        i.fileNameNoExt = IO.Path.GetFileNameWithoutExtension(f)
        Dim matchGroups = Regex.Match(i.fileNameNoExt, "(.+?)(\d+$)").Groups
        If matchGroups.Count < 2 Then i.product = i.fileNameNoExt Else i.product = matchGroups(1).Value : i.suffix = matchGroups(2).Value : i.suffix_num = CInt(i.suffix)
        If i.product.ToUpper.EndsWith("BOX") Then i.product = i.product.Substring(0, i.product.Length - 3) : i.suffix = "box" + i.suffix : i.isBox = True
        If i.product.EndsWith("_") Then i.product = i.product.Substring(0, i.product.Length - 1) : i.suffix = "_" + i.suffix
        Return i
    End Function
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

    Private Sub Screen_Replacement_Button_Add_Click(sender As Object, e As EventArgs) Handles Screen_Replacement_Button_Add.Click
        Dim arr = ComboBox1.Text.Split({"->"}, StringSplitOptions.RemoveEmptyEntries)
        If arr.Count <> 2 Then MsgBox("Wrong replacement format. Should be ""MATCH -> REPLACEMENT""") : Exit Sub
        ComboBox1.Items.Add(ComboBox1.Text)
    End Sub
    Private Sub Screen_Replacement_Button_Remove_Click(sender As Object, e As EventArgs) Handles Screen_Replacement_Button_Remove.Click
        ComboBox1.Items.Remove(ComboBox1.Text)
    End Sub
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex < 0 Then Exit Sub
        If e.ColumnIndex = 0 Then
            'Find game (product) in main list
            Dim game = DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString
            If game.Contains("\") Then game = game.Substring(game.LastIndexOf("\") + 1)
            Dim f = DirectCast(Me.Owner, Form1)
            f.TextBox1.Text = game
        ElseIf e.ColumnIndex = 1 Then
            'Open screen folder

        End If
    End Sub
    Private Sub DataGridView1_KeyDown(sender As Object, e As KeyEventArgs) Handles DataGridView1.KeyDown
        If DataGridView1.SelectedRows.Count <> 1 Then MsgBox("Nothing selected.") : Exit Sub
        If e.KeyCode = Keys.Space Or e.KeyCode = Keys.Enter Or e.KeyCode = Keys.Return Then
            Dim r = DataGridView1.SelectedRows(0)

            If e.KeyCode = Keys.Space Or e.KeyCode = Keys.Enter Then
                If r.Cells(2).Value.ToString.Trim = "" Then
                    r.Cells(2).Value = "X"
                ElseIf r.Cells(2).Value = "X" Then
                    r.Cells(2).Value = ""
                End If
            End If
            If e.KeyCode = Keys.Enter Or e.KeyCode = Keys.Return Then
                Dim ind = r.Index + 1
                If ind < DataGridView1.Rows.Count Then DataGridView1.Rows(ind).Selected = True
            End If
        End If
    End Sub
    Private Sub Screen_CheckAll_Button_Click(sender As Object, e As EventArgs) Handles Screen_CheckAll_Button.Click
        For Each r As DataGridViewRow In DataGridView1.Rows
            If r.Cells(2).Value.ToString.Trim = "" Then r.Cells(2).Value = "X"
        Next
    End Sub
    Private Sub Screen_UnCheckAll_Button_Click(sender As Object, e As EventArgs) Handles Screen_UnCheckAll_Button.Click
        For Each r As DataGridViewRow In DataGridView1.Rows
            If r.Cells(2).Value.ToString.Trim = "X" Then r.Cells(2).Value = ""
        Next
    End Sub

    Private Sub DisableControlsAndCreateStatusLabel()
        TabControl1.Enabled = False
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
        TabControl1.Enabled = True
    End Sub
End Class