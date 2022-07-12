Imports System.ComponentModel
Imports System.Text.RegularExpressions

Public Class Form6_Scanner_generic
    Dim ini As IniFileApi = Form1.ini
    Dim WithEvents bg As New BackgroundWorker
    Dim fileList As New List(Of String)
    Dim refreshing As Boolean = True
    Dim refreshing_custom_import_option As Boolean = False
    Dim realFieldNames As New List(Of String)

    Private Sub Form6_Scanner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler bg.RunWorkerCompleted, Sub() Button1.Enabled = True

        TextBox1.Text = ini.IniReadValue("Paths", "LastScannerPath")
        TextBox2.Text = ini.IniReadValue("Paths", "LastScannerCustomFilePath")
        TextBox3.Text = ini.IniReadValue("Paths", "LastScannerDirFilePath")

        Dim tmp As String = ""
        tmp = ini.IniReadValue("Interface", "Scanner_LastScanType")
        If tmp <> "" AndAlso IsNumeric(tmp) Then Me.Controls.OfType(Of RadioButton)()(CInt(tmp)).Checked = True
        tmp = ini.IniReadValue("Interface", "Scanner_LastCustomType")
        If tmp <> "" AndAlso IsNumeric(tmp) Then
            Dim rd = {RadioButton4, RadioButton5, RadioButton6}(CInt(tmp)) : rd.Checked = True
        End If
        tmp = ini.IniReadValue("Interface", "Scanner_LastCustomSeparator")
        If tmp <> "" Then
            RadioButton5.Tag = tmp
            If RadioButton5.Checked Then TextBox6.Text = tmp
        End If
        tmp = ini.IniReadValue("Interface", "Scanner_LastCustomWidth")
        If tmp <> "" AndAlso IsNumeric(tmp) Then
            RadioButton6.Tag = tmp
            If RadioButton6.Checked Then TextBox6.Text = tmp
        End If
        tmp = ini.IniReadValue("Interface", "Scanner_LastCustomSeparatorAsOne")
        If tmp = "1" OrElse tmp.ToUpper = "TRUE" Then CheckBox9.Checked = True
        Dim c = 1
        Do While True
            tmp = ini.IniReadValue("Interface", "Scanner_LastCustomItem" + c.ToString)
            If tmp = "" Then Exit Do
            Dim arr = tmp.Split({";;;"}, StringSplitOptions.None)

            Dim i = New custom_import_field_listitem
            i.name = arr(0)
            i.db_field = arr(1)
            i.match = CBool(arr(2))
            ListBox1.Items.Add(i)
            If RadioButton2.Checked Then DataGridView1.Columns.Add(i.db_field, i.db_field)
            c += 1
        Loop

        For Each f In Fields
            If f.field_type = Field_Info.field_types.bool AndAlso f.name <> "" Then
                ComboBox1.Items.Add(f.name)
                realFieldNames.Add(f.DBname)
                If f.name.Trim.ToLower = "have" Then ComboBox1.SelectedIndex = ComboBox1.Items.Count - 1
            End If

            ComboBox3.Items.Add(f.DBname.ToLower + " (" + f.name + ")")
        Next
        If ComboBox1.SelectedIndex < 0 Then ComboBox1.SelectedIndex = 0

        RadioButtonType_CheckedChanged(RadioButton1, New EventArgs)

        refreshing = False
    End Sub

    'Scan
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Button1.Enabled = False
        DataGridView1.Rows.Clear()
        TabControl1.SelectedTab = TabPage4

        If RadioButton1.Checked Then
            'Exodos mode
            Dim exodos_dir1 = TextBox1.Text + "\Games\"
            Dim exodos_dir2 = TextBox1.Text + "\eXoDOS\"
            Dim exodos_dir3 = TextBox1.Text + "\exo\eXoDOS\"
            Dim exodos_dir = exodos_dir1
            If Not My.Computer.FileSystem.DirectoryExists(exodos_dir) Then
                exodos_dir = exodos_dir2
                If Not My.Computer.FileSystem.DirectoryExists(exodos_dir) Then
                    exodos_dir = exodos_dir3
                    If Not My.Computer.FileSystem.DirectoryExists(exodos_dir) Then
                        Dim msg = "Directory '" + "' not exits. " + vbCrLf
                        msg += "Tried in:" + vbCrLf
                        msg += exodos_dir1 + vbCrLf
                        msg += exodos_dir2 + vbCrLf
                        msg += exodos_dir3 + vbCrLf
                        MsgBox(msg) : Button1.Enabled = True : Exit Sub
                    End If
                End If
            End If


            fileList = New List(Of String)
            For Each f In My.Computer.FileSystem.GetDirectories(exodos_dir, FileIO.SearchOption.SearchTopLevelOnly)
                fileList.Add(f)
            Next
            bg.RunWorkerAsync()
        ElseIf RadioButton2.Checked Then
            'Custom ini
            If Not My.Computer.FileSystem.FileExists(TextBox2.Text) Then
                MsgBox("File not exits. " + TextBox2.Text)
                Exit Sub
            End If

            If DataGridView1.Columns.Count < ListBox1.Items.Count + 2 Then
                DataGridView1.Columns.Add("Column_Status", "Status")
                DataGridView1.Columns.Add("Column_Query", "Query")
            End If

            fileList = New List(Of String)
            bg.RunWorkerAsync()
            Exit Sub


            If CheckBox4.Checked Then db.execute("DELETE FROM custom_lists_data")
            Dim gameName As String = ""
            Dim stream = My.Computer.FileSystem.OpenTextFileReader(TextBox2.Text)
            Do While Not stream.EndOfStream
                Dim l = stream.ReadLine
                If l.StartsWith("[") Then
                    gameName = l.Substring(1, l.Length - 2)
                ElseIf l.Replace(" ", "").ToUpper = "Remember=1".ToUpper Then
                    Dim sql = "SELECT id FROM main WHERE name = '" + gameName.Replace("'", "''") + "'"
                    Dim r = db.queryReader(sql)
                    If Not r.HasRows Then MsgBox(gameName + " not found!") : Continue Do
                    Do While r.Read
                        sql = "INSERT INTO custom_lists_data (id_main, id_list) VALUES (" + r.GetInt32(0).ToString + ", 2)"
                        db.execute(sql)
                    Loop
                ElseIf l.Replace(" ", "").ToUpper = "Fav".ToUpper Then
                    Dim sql = "SELECT id FROM main WHERE name = '" + gameName.Replace("'", "''") + "'"
                    Dim r = db.queryReader(sql)
                    If Not r.HasRows Then MsgBox(gameName + " not found!") : Continue Do
                    Do While r.Read
                        sql = "INSERT INTO custom_lists_data (id_main, id_list) VALUES (" + r.GetInt32(0).ToString + ", 1)"
                        db.execute(sql)
                    Loop
                ElseIf l.Replace(" ", "").ToUpper = "List1".ToUpper Then
                    Dim sql = "SELECT id FROM main WHERE name = '" + gameName.Replace("'", "''") + "'"
                    Dim r = db.queryReader(sql)
                    If Not r.HasRows Then MsgBox(gameName + " not found!") : Continue Do
                    Do While r.Read
                        sql = "INSERT INTO custom_lists_data (id_main, id_list) VALUES (" + r.GetInt32(0).ToString + ", 3)"
                        db.execute(sql)
                    Loop
                End If
            Loop
            stream.Close()
        ElseIf RadioButton3.Checked Then
            'Folders mode
            If Not My.Computer.FileSystem.DirectoryExists(TextBox3.Text) Then
                MsgBox("Directory not exits. " + TextBox3.Text)
                Exit Sub
            End If

            Dim searchOption = FileIO.SearchOption.SearchTopLevelOnly
            If CheckBox7.Checked Then searchOption = FileIO.SearchOption.SearchAllSubDirectories

            fileList = New List(Of String)
            'Add folders
            If CheckBox5.Checked Then
                For Each f In My.Computer.FileSystem.GetDirectories(TextBox3.Text, searchOption)
                    fileList.Add(f)
                Next
            End If

            'Add files
            If CheckBox6.Checked Then
                Dim wildCardsArray = TextBox4.Text.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).Select(Of String)(Function(x) "*." + x.Trim).ToArray
                For Each f In My.Computer.FileSystem.GetFiles(TextBox3.Text, searchOption, wildCardsArray)
                    fileList.Add(f)
                Next
            End If

            bg.RunWorkerAsync()
        End If
    End Sub
    Private Sub Button1_Click_bg_work() Handles bg.DoWork
        Dim HAVEfield As String = ""
        Dim comboindex As Integer = ComboBox1.Invoke(Function() ComboBox1.SelectedIndex)
        If comboindex > 0 Then HAVEfield = realFieldNames(comboindex - 1)

        If CheckBox1.Checked And HAVEfield <> "" Then db.execute("UPDATE main SET " + HAVEfield + " = 'false'")
        If CheckBox2.Checked Then
            If RadioButton1.Checked Then db.execute("DELETE FROM paths WHERE name like 'exodos'")
            If RadioButton3.Checked Then db.execute("DELETE FROM paths WHERE name like 'folder'")
        End If
        If CheckBox3.Checked Then db.execute("DELETE FROM unrecognized_files")

        Dim sep = "^^@%*^^"
        Dim rgx = New Regex("[^a-zA-Z0-9]")
        Dim fields_names As String() = Nothing
        Dim fields_matches As Boolean() = Nothing
        Dim custom_list As New List(Of String())
        Dim custom_db_cache As New List(Of String)
        Dim custom_db_cache_id As New List(Of Integer)
        Dim additional_set As String = ""
        If RadioButton2.Checked Then
            additional_set = TextBox8.Invoke(Function() TextBox8.Text).ToString.Trim
            'If additional_set <> "" Then additional_set = ", " + additional_set
            fields_names = ListBox1.Items.Cast(Of custom_import_field_listitem).Select(Of String)(Function(x) x.db_field.ToLower).ToArray
            fields_matches = ListBox1.Items.Cast(Of custom_import_field_listitem).Select(Of Boolean)(Function(x) x.match).ToArray
            If Not fields_matches.Contains(True) Then MsgBox("You should have at least one field marked as 'match'.") : Exit Sub
            For i As Integer = 0 To fields_names.Count - 1
                If fields_names(i).Contains(" ") Then fields_names(i) = fields_names(i).Substring(0, fields_names(i).IndexOf(" ")).Trim
            Next

            Label1.Invoke(Sub() Label1.Text = "Parsing file...")
            Dim c = 0
            Dim custom_file = CStr(TextBox2.Invoke(Function() TextBox2.Text))
            Dim stream = My.Computer.FileSystem.OpenTextFileReader(custom_file)
            Do While Not stream.EndOfStream
                fileList.Add(c.ToString)
                Dim arr = proceed_line(stream.ReadLine)
                custom_list.Add(arr)
                c += 1
            Loop
            stream.Close()

            Label1.Invoke(Sub() Label1.Text = "Caching database...")
            Dim where = TextBox9.Invoke(Function() TextBox9.Text).ToString
            Dim sql = "SELECT id, " + String.Join(", ", fields_names) + " FROM main"
            If where <> "" Then sql += " " + where.Trim
            Dim rr = db.queryReader(sql)
            Do While rr.Read
                custom_db_cache_id.Add(rr.GetInt32(0))
                Dim str = ""
                For i As Integer = 0 To fields_names.Count - 1
                    If (fields_matches(i)) Then
                        If fields_names(i) = "name" Then str += rgx.Replace(rr.GetString(i + 1), "").ToUpper + sep
                        If fields_names(i).Contains("_str") Then str += rr.GetString(i + 1).ToUpper + sep
                        If fields_names(i).Contains("_num") Then str += rr.GetInt32(i + 1).ToString + sep
                        If fields_names(i).Contains("_dec") Then str += rr.GetDecimal(i + 1).ToString + sep
                        If fields_names(i).Contains("_txt") Then str += rr.GetString(i + 1).ToUpper + sep
                        If fields_names(i).Contains("_bool") Then str += rr.GetBoolean(i + 1).ToString.ToUpper + sep
                    End If
                Next
                custom_db_cache.Add(str)
            Loop
        End If

        Dim cache As New Dictionary(Of String, Integer)
        Dim cache_Dupes As New Dictionary(Of String, List(Of Integer))
        If RadioButton3.Checked Then
            'cache database
            Label1.Invoke(Sub() Label1.Text = "Caching database...")
            Dim rr = db.queryReader("SELECT id, name FROM main")
            Do While rr.Read
                Dim nName As String = rgx.Replace(rr.GetString(1), "").ToUpper
                If Not cache.ContainsKey(nName) Then
                    cache.Add(nName, rr.GetInt32(0))
                Else
                    If Not cache_Dupes.ContainsKey(nName) Then cache_Dupes.Add(nName, New List(Of Integer))
                    cache_Dupes(nName).Add(rr.GetInt32(0))
                End If
            Loop
        End If

        Dim n As Integer = 0
        Dim id As Integer = 0
        Dim found As Boolean = False
        Dim found_n As Integer = 0
        Dim fCount = fileList.Count.ToString

        db.execute("BEGIN;")
        For Each f As String In fileList
            n += 1
            found = False
            If RadioButton1.Checked Then
                'Exodos mode
                Dim iniFile As String = My.Computer.FileSystem.GetFiles(f + "\Meagre\IniFile", FileIO.SearchOption.SearchTopLevelOnly, {"*.ini"})(0)
                Label1.Invoke(Sub() Label1.Text = iniFile)

                Dim ini As New IniFileApi With {.path = iniFile}
                Dim mobyLink = ini.IniReadValue("Main", "ExtraLink1", True)

                Dim sql = "SELECT id FROM main WHERE data_str15 = '" + mobyLink.Substring(mobyLink.LastIndexOf("/") + 1).Replace("'", "''") + "'"
                Dim r = db.queryReader(sql)
                If r.HasRows Then
                    found = True
                    found_n += 1
                    r.Read()
                    id = r.GetInt32(0)
                End If

                If found Then
                    'add path
                    Dim bat_arr = My.Computer.FileSystem.GetFiles(f, FileIO.SearchOption.SearchTopLevelOnly, {"*.bat"})
                    Dim bat As String = bat_arr(0)
                    If bat.ToUpper.EndsWith("INSTALL.BAT") Then bat = bat_arr(1)
                    db.execute("INSERT INTO paths (main_id, name, value) VALUES (" + id.ToString + ", 'ExoDos', '" + bat.Replace("'", "''") + "' )")
                End If

                If found And HAVEfield <> "" Then
                    db.execute("UPDATE main SET " + HAVEfield + " = 'true' WHERE id = " + id.ToString)
                ElseIf Not found Then
                    Dim file = f.Substring(f.LastIndexOf("\") + 1)
                    db.execute("INSERT INTO unrecognized_files (file) VALUES ('" + file.Replace("'", "''") + "')")
                End If
            ElseIf RadioButton2.Checked Then
                'Custom ini mode
                Dim arr = custom_list(CInt(f))
                Dim match = ""
                Dim update_data As New List(Of String)
                Dim sql = ""

                For i As Integer = 0 To fields_names.Count - 1
                    If (fields_matches(i)) Then
                        If arr(i) Is Nothing Then arr(i) = ""
                        If fields_names(i) = "name" Then match += rgx.Replace(arr(i), "").ToUpper + sep
                        If fields_names(i).Contains("_str") Then match += arr(i).ToUpper + sep
                        If fields_names(i).Contains("_num") Then match += arr(i) + sep
                        If fields_names(i).Contains("_dec") Then match += arr(i) + sep
                        If fields_names(i).Contains("_txt") Then match += arr(i).ToUpper + sep
                        If fields_names(i).Contains("_bool") Then match += arr(i).ToUpper + sep
                    Else
                        Dim data = ""
                        If arr(i) IsNot Nothing Then data = arr(i).Trim
                        If fields_names(i).Contains("_str") Or fields_names(i).Contains("_txt") Then data = "'" + data.Replace("'", "''") + "'"
                        If fields_names(i).Contains("_bool") Then data = IIf(data = "0" Or data.ToUpper = "FALSE", "0", "1").ToString
                        update_data.Add(fields_names(i) + " = " + data)
                    End If
                Next

                Dim indexes = Enumerable.Range(0, custom_db_cache.Count).Where(Function(i) custom_db_cache(i) = match).ToArray
                For Each ind In indexes
                    id = custom_db_cache_id(ind)

                    Dim sql_set = String.Join(", ", update_data.ToArray).Trim
                    If additional_set <> "" Then
                        If sql_set <> "" Then sql_set += ", "
                        sql_set += additional_set
                    End If

                    sql += "UPDATE main SET " + sql_set + " WHERE id = " + id.ToString + ";;;"
                    'sql += "UPDATE main SET " + String.Join(", ", update_data.ToArray) + additional_set + " WHERE id = " + id.ToString + ";;;"
                Next

                If indexes.Count = 0 Then
                    arr(arr.Count - 2) = "Not Found"
                ElseIf indexes.Count = 1 Then
                    arr(arr.Count - 2) = "Found" : found_n += 1
                Else
                    arr(arr.Count - 2) = "Found Multiple Matches" : found_n += 1
                End If

                arr(arr.Count - 1) = sql
                DataGridView1.Invoke(Sub() DataGridView1.Rows.Add(arr))
            ElseIf RadioButton3.Checked Then
                'Folder mode
                Dim f_loc = f.Substring(f.LastIndexOf("\", f.LastIndexOf("\") - 1))
                Label1.Invoke(Sub() Label1.Text = f_loc)

                'f_loc = f_loc.Substring(f_loc.LastIndexOf("\") + 1)
                Dim nName = IO.Path.GetFileNameWithoutExtension(f).Trim.ToUpper.Replace(" ", "").Replace(" ", "")
                If nName.Contains("[") Then nName = nName.Substring(0, nName.IndexOf("["))
                If nName.Contains("(") Then nName = nName.Substring(0, nName.IndexOf("("))
                nName = rgx.Replace(nName, "")

                For Each remove In TextBox5.Text.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
                    nName = nName.Replace(remove.Trim.ToUpper, "")
                Next

                If cache.ContainsKey(nName) Then
                    If Not cache_Dupes.ContainsKey(nName) Then
                        found_n += 1
                        id = cache(nName)

                        'Test, if it's not already in db
                        Dim q As New List(Of String)
                        Dim TEST = String.Join(";", New List(Of String))
                        If HAVEfield <> "" Then
                            q.Add("UPDATE main SET " + HAVEfield + " = 'true' WHERE id = " + id.ToString)
                            'db.execute("UPDATE main SET " + HAVEfield + " = 'true' WHERE id = " + id.ToString)
                        End If

                        Dim t = db.queryDataset("SELECT main_id FROM paths WHERE main_id = " + id.ToString + " AND value LIKE '" + f.ToString.Replace("'", "''") + "'")
                        If t.Rows.Count = 0 Then
                            q.Add("INSERT INTO paths (main_id, name, value) VALUES (" + id.ToString + ", 'Folder', '" + f.ToString.Replace("'", "''") + "')")
                            'db.execute("INSERT INTO paths (main_id, name, value) VALUES (" + id.ToString + ", 'Folder', '" + f.ToString.Replace("'", "''") + "')")
                            DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({f_loc, "Found", String.Join(";", q.ToArray)}))
                        Else
                            DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({f_loc, "Already in database", String.Join(";", q.ToArray)}))
                        End If
                    Else
                        DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({f_loc, "Found Multiple Matches", ""}))
                    End If
                Else
                    db.execute("INSERT INTO unrecognized_files (file) VALUES ('" + f_loc.Replace("'", "''") + "')")
                    DataGridView1.Invoke(Sub() DataGridView1.Rows.Add({f_loc, "Not Found", ""}))
                End If
            End If

            Label2.Invoke(Sub() Label2.Text = "Found / Done / Total: " + found_n.ToString + " / " + n.ToString + " / " + fCount)
        Next

        Label1.Invoke(Sub() Label1.Text = "Commit changes...")
        db.execute("COMMIT;")
        Label1.Invoke(Sub() Label1.Text = "DONE")
    End Sub
    'Commit result
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim bg2 As New BackgroundWorker
        AddHandler bg2.DoWork, AddressOf Button2_Click_bg_work
        bg2.RunWorkerAsync()
    End Sub
    Private Sub Button2_Click_bg_work()
        Dim entries_total = DataGridView1.Rows.Count
        Dim entries_current = 0

        db.execute("BEGIN;")
        For Each r As DataGridViewRow In DataGridView1.Rows
            entries_current += 1
            Dim q = r.Cells(r.Cells.Count - 1).Value.ToString().Trim
            Dim sep = ";"
            If q.Contains(";;;") Then sep = ";;;"
            For Each q_split In q.Split({sep}, StringSplitOptions.RemoveEmptyEntries)
                If q_split <> "" Then db.execute(q_split)
            Next

            If entries_current Mod 10 = 0 Then Label2.Invoke(Sub() Label2.Text = "Processing: " + entries_current.ToString + " / " + entries_total.ToString)
        Next

        Label2.Invoke(Sub() Label2.Text = "Processing: " + entries_current.ToString + " / " + entries_total.ToString)
        db.execute("COMMIT;")
        Label1.Invoke(Sub() Label1.Text = "DONE")
    End Sub
    'Export result
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim sw As New IO.StreamWriter(".\Result.txt", False, System.Text.Encoding.UTF8)
        For Each r As DataGridViewRow In DataGridView1.Rows
            Dim f = r.Cells(0).Value.ToString()
            If f.Contains("\") Then f = f.Substring(f.LastIndexOf("\") + 1)
            sw.WriteLine(f + vbTab + r.Cells(1).Value.ToString())
        Next
        sw.Close()
        MsgBox("Result was successfully exported as 'Result.txt'")
    End Sub

    'Show/hide unused options
    Private Sub RadioButtonType_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, RadioButton3.CheckedChanged
        Dim rad = DirectCast(sender, RadioButton)
        If Not rad.Checked Then Exit Sub

        DataGridView1.Columns.Clear()
        TabPage1.Enabled = RadioButton1.Checked
        TabPage2.Enabled = RadioButton2.Checked
        TabPage3.Enabled = RadioButton3.Checked

        If RadioButton1.Checked Then
            TabControl1.SelectedTab = TabPage1
        ElseIf RadioButton2.Checked Then
            For Each i As custom_import_field_listitem In ListBox1.Items
                DataGridView1.Columns.Add(i.db_field, i.db_field)
            Next
            TabControl1.SelectedTab = TabPage2
        ElseIf RadioButton3.Checked Then
            DataGridView1.Columns.Add("Column1", "File")
            DataGridView1.Columns.Add("Column2", "Status")
            DataGridView1.Columns.Add("Column3", "Query")
            DataGridView1.Columns(0).Width = 350
            DataGridView1.Columns(1).Width = 210
            DataGridView1.Columns(2).Width = 100
            DataGridView1.Columns(2).Visible = False
            TabControl1.SelectedTab = TabPage3
        End If

        If Not refreshing Then
            Dim ind = {RadioButton1, RadioButton2, RadioButton3}.ToList.IndexOf(rad).ToString
            ini.IniWriteValue("Interface", "Scanner_LastScanType", ind)
        End If
    End Sub
    'Save paths to ini on change
    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerPath", TextBox1.Text)
    End Sub
    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerCustomFilePath", TextBox2.Text)
    End Sub
    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerDirFilePath", TextBox3.Text)
    End Sub


    'Custom.ini import options tab

    'Ini type change
    Private Sub RadioButton_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton4.CheckedChanged, RadioButton5.CheckedChanged, RadioButton6.CheckedChanged
        Dim rad = DirectCast(sender, RadioButton)
        If Not rad.Checked Then Exit Sub

        refreshing_custom_import_option = True
        If rad Is RadioButton4 Then
            'ini
            Label7.Visible = False
            TextBox6.Visible = False
            CheckBox9.Visible = False
        ElseIf rad Is RadioButton5 Then
            'separator
            Label7.Visible = True
            TextBox6.Visible = True
            CheckBox9.Visible = True
            Label7.Text = "Separator:"
            TextBox6.Text = RadioButton5.Tag.ToString
        ElseIf rad Is RadioButton6 Then
            'fixed width
            Label7.Visible = True
            TextBox6.Visible = True
            CheckBox9.Visible = False
            Label7.Text = "Width:"
            TextBox6.Text = CInt(RadioButton6.Tag)
        End If
        refreshing_custom_import_option = False

        If Not refreshing Then
            Dim ind = {RadioButton4, RadioButton5, RadioButton6}.ToList.IndexOf(rad).ToString
            ini.IniWriteValue("Interface", "Scanner_LastCustomType", ind)
        End If
    End Sub
    'Textbox type param changed
    Private Sub TextBox6_TextChanged(sender As Object, e As EventArgs) Handles TextBox6.TextChanged
        If refreshing_custom_import_option Then Exit Sub
        If RadioButton5.Checked Then
            'separator
            If TextBox6.Text.Contains(vbTab) Then TextBox6.Text = TextBox6.Text.Replace(vbTab, "{TAB}")
            RadioButton5.Tag = TextBox6.Text.Trim
            If Not refreshing Then ini.IniWriteValue("Interface", "Scanner_LastCustomSeparator", TextBox6.Text)
        ElseIf RadioButton6.Checked Then
            'fixed width
            If TextBox6.Text.Trim <> "" AndAlso Not IsNumeric(TextBox6.Text) Then Beep() : TextBox6.Text = RadioButton6.Tag.ToString
            If TextBox6.Text.Contains(".") OrElse TextBox6.Text.Contains(",") Then Beep() : TextBox6.Text = RadioButton6.Tag.ToString
            If TextBox6.Text.Trim = "" Then RadioButton6.Tag = "0" Else RadioButton6.Tag = TextBox6.Text.Trim
            If Not refreshing Then ini.IniWriteValue("Interface", "Scanner_LastCustomWidth", TextBox6.Text)
        End If
    End Sub
    'Add field
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim i = New custom_import_field_listitem
        ListBox1.Items.Add(i)
        ListBox1.SelectedIndex = ListBox1.Items.Count - 1
        DataGridView1.Columns.Add(i.db_field, i.db_field)

        Dim ind = ListBox1.Items.Count.ToString
        ini.IniWriteValue("Interface", "Scanner_LastCustomItem" + ind, i.name + ";;;" + i.db_field + ";;;" + i.match.ToString)
    End Sub
    'Remove field
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If ListBox1.SelectedIndex < 0 Then MsgBox("Select a field, please.") : Exit Sub
        Dim sel = ListBox1.SelectedIndex
        ListBox1.Items.RemoveAt(sel)

        For ind As Integer = 0 To ListBox1.Items.Count - 1
            Dim i = DirectCast(ListBox1.Items(ind), custom_import_field_listitem)
            ini.IniWriteValue("Interface", "Scanner_LastCustomItem" + (ind + 1).ToString, i.name + ";;;" + i.db_field + ";;;" + i.match.ToString)
        Next
        ini.IniWriteValue("Interface", "Scanner_LastCustomItem" + (ListBox1.Items.Count + 1).ToString, Nothing)

        If sel < ListBox1.Items.Count Then
            ListBox1.SelectedIndex = sel
        ElseIf ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = ListBox1.Items.Count - 1
        End If
    End Sub
    'Fields listbox selected changed
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If refreshing_custom_import_option Then Exit Sub

        refreshing_custom_import_option = True
        If ListBox1.SelectedIndex < 0 Then
            TextBox7.Enabled = False : TextBox7.Text = ""
            ComboBox3.Enabled = False : ComboBox3.SelectedIndex = -1
            CheckBox8.Enabled = False : CheckBox8.Checked = False
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, custom_import_field_listitem)
            TextBox7.Enabled = True : TextBox7.Text = l.name
            CheckBox8.Enabled = True : CheckBox8.Checked = l.match
            ComboBox3.Enabled = True : ComboBox3.SelectedIndex = -1
            For Each i In ComboBox3.Items
                If i.ToString.ToLower = l.db_field.ToLower OrElse i.ToString.ToLower.StartsWith(l.db_field.ToLower + " ") Then
                    ComboBox3.SelectedItem = i
                    Exit For
                End If
            Next
        End If
        refreshing_custom_import_option = False
    End Sub
    'Fields option changed
    Private Sub FieldsOptionChanged() Handles TextBox7.TextChanged, ComboBox3.SelectedIndexChanged, CheckBox8.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        If refreshing_custom_import_option Then Exit Sub

        Dim l = DirectCast(ListBox1.SelectedItem, custom_import_field_listitem)
        l.name = TextBox7.Text
        l.match = CheckBox8.Checked
        If ComboBox3.SelectedItem IsNot Nothing Then l.db_field = ComboBox3.SelectedItem.ToString Else l.db_field = ""

        refreshing_custom_import_option = True
        ListBox1.Items(ListBox1.SelectedIndex) = l
        DataGridView1.Columns(ListBox1.SelectedIndex).HeaderText = l.db_field

        Dim ind = (ListBox1.SelectedIndex + 1).ToString
        ini.IniWriteValue("Interface", "Scanner_LastCustomItem" + ind, l.name + ";;;" + l.db_field + ";;;" + l.match.ToString)

        refreshing_custom_import_option = False
    End Sub
    'Preview button
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        DataGridView1.Rows.Clear()

        If Not My.Computer.FileSystem.FileExists(TextBox2.Text) Then
            MsgBox("File not exits. " + TextBox2.Text) : Exit Sub
        End If
        If ListBox1.Items.Count = 0 Then
            MsgBox("No fields added.") : Exit Sub
        End If

        Dim counter = 1
        Dim stream = My.Computer.FileSystem.OpenTextFileReader(TextBox2.Text)
        Do While Not stream.EndOfStream
            If counter = 100 Then Exit Do
            Dim arr = proceed_line(stream.ReadLine)
            DataGridView1.Rows.Add(arr)
            counter += 1
        Loop
        stream.Close()

        TabControl1.SelectedTab = TabPage4
    End Sub
    'Proceed Line
    Private Function proceed_line(l As String) As String()
        Dim arr As String() = Nothing

        If RadioButton4.Checked Then
            'ini format
            'TODO
        ElseIf RadioButton5.Checked Then
            'separator
            Dim opt = StringSplitOptions.None
            If CheckBox9.Checked Then opt = StringSplitOptions.RemoveEmptyEntries
            arr = l.Split({TextBox6.Text.Replace("{TAB}", vbTab)}, opt)
        ElseIf RadioButton6.Checked Then
            'fixed width
            Dim lst As New List(Of String)
            Dim w = CInt(TextBox6.Text)
            Do While True
                If l.Length > w Then
                    lst.Add(l.Substring(0, w))
                    l = l.Substring(w)
                Else
                    lst.Add(l) : Exit Do
                End If
            Loop
            arr = lst.ToArray
        End If

        If arr.Count <> DataGridView1.ColumnCount Then ReDim Preserve arr(DataGridView1.ColumnCount - 1)
        Return arr
    End Function
    'Threat multiple separators as one - just saving
    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged
        If Not refreshing Then ini.IniWriteValue("Interface", "Scanner_LastCustomSeparatorAsOne", CheckBox9.Checked.ToString)
    End Sub

    Public Class custom_import_field_listitem
        Public name As String = "New Field"
        Public db_field As String = "data_str1"
        Public match As Boolean = False

        Public Overrides Function ToString() As String
            Return name + " -> " + db_field
        End Function
    End Class
End Class