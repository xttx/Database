'TODO - Rename to "Export"
'TODO - .ini format export does not handle 'only visible'

Imports GemBox.Document

Public Class Form9_ExportCustoms
    Dim ini As IniFileApi = Form1.ini

    Private Sub Form9_ExportCustoms_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each f In Fields_ordered.Where(Function(x) x.enabled)
            Dim l As New ListViewItem With {.Text = f.name, .Tag = f.DBname}
            ListView1.Items.Add(l)
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim fields = ""
        Dim where = ""
        Dim atLeastOneChecked As Boolean = False
        For Each l As ListViewItem In ListView1.Items
            If l.Checked Then
                atLeastOneChecked = True
                'Dim realFieldName = "data_" + l.Tag.ToString.Substring(6)
                Dim realFieldName = l.Tag.ToString()
                fields += realFieldName + ", "
                where += "(" + realFieldName + " != "
                If realFieldName.ToUpper.Contains("_STR") Then
                    where += "'') OR "
                ElseIf realFieldName.ToUpper.Contains("_BOOL") Then
                    where += "0 AND " + realFieldName + " != 'false') OR "
                Else
                    where += "0) OR "
                End If
            End If
        Next
        If Not atLeastOneChecked Then MsgBox("Nothing to export.") : Exit Sub
        fields = fields.Substring(0, fields.Length - 2)
        where = where.Substring(0, where.Length - 3)

        Dim fs As New SaveFileDialog
        fs.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        fs.FilterIndex = 1
        fs.RestoreDirectory = True
        If fs.ShowDialog() <> DialogResult.OK Then Exit Sub

        Dim bg As New System.ComponentModel.BackgroundWorker
        If RadioButton1.Checked Then
            '.ini format
            Dim sw As New IO.StreamWriter(fs.FileName, False, System.Text.Encoding.UTF8)
            sw.WriteLine("[_METADATA_]")
            For Each l As ListViewItem In ListView1.Items
                'If l.Checked Then sw.WriteLine("field=" + "data_" + l.Tag.ToString.Substring(6) + "|||" + l.Text)
                If l.Checked Then sw.WriteLine("field=" + l.Tag.ToString() + "|||" + l.Text)
            Next
            sw.Close()

            AddHandler bg.DoWork, AddressOf Button1_Click_subBG
            bg.RunWorkerAsync({fs.FileName, fields, where})
        Else
            '.csv format
            AddHandler bg.DoWork, AddressOf Button1_Click_subBG_CSV
            bg.RunWorkerAsync({fs.FileName, fields, where, CheckBox1.Checked.ToString()})
        End If

    End Sub
    Sub Button1_Click_subBG(o As Object, e As System.ComponentModel.DoWorkEventArgs)
        Dim arr = CType(e.Argument, String())
        Dim fileName = arr(0)
        Dim fields = arr(1)
        Dim where = arr(2)
        Label1.Invoke(Sub() Label1.Text = "Status: Initialization...")

        Dim sql = "SELECT count(id) FROM main"
        Dim count = CInt(db.queryDataset(sql).Rows(0).Item(0))

        Dim table_paths_exist As Boolean = False
        Dim r = db.queryReader("SELECT name FROM sqlite_master WHERE type='table' AND name='paths'")
        If r.HasRows Then table_paths_exist = True

        Dim table_files_exist As Boolean = False
        r = db.queryReader("SELECT name FROM sqlite_master WHERE type='table' AND name='files'")
        If r.HasRows Then table_files_exist = True

        Dim sw As New IO.StreamWriter(fileName, True, System.Text.Encoding.UTF8)
        r = db.queryReader("SELECT name, field, value FROM groups")
        Do While r.Read
            sw.WriteLine("group=" + r.GetString(0) + "|||" + r.GetString(1) + "|||" + r(2).ToString)
        Loop
        r = db.queryReader("SELECT id, name FROM custom_lists ORDER BY id")
        Do While r.Read
            sw.WriteLine("list=" + r.GetInt32(0).ToString + "|||" + r.GetString(1))
        Loop
        sw.WriteLine("")

        Label1.Invoke(Sub() Label1.Text = "Status: Starting...")
        sql = "SELECT id, name, " + fields + " FROM main" ' + " WHERE " + where
        r = db.queryReader(sql)
        Dim c As Integer = 0
        Do While r.Read
            Dim id = r.GetInt32(0).ToString
            sw.WriteLine("[" + id + "]")
            sw.WriteLine("name=" + r.GetString(1))
            For n = 2 To r.FieldCount - 1
                If Not r.IsDBNull(n) Then sw.WriteLine(r.GetName(n) + "=" + r.GetString(n))
            Next

            'Custom Lists
            Dim r2 = db.queryReader("SELECT id_list FROM custom_lists_data WHERE id_main = " + id)
            Do While r2.Read
                sw.WriteLine("Custom_list=" + r2.GetInt32(0).ToString)
            Loop

            'Files (Daz catalog)
            If table_files_exist Then
                r2 = db.queryReader("SELECT file FROM files WHERE main_id = " + id)
                Do While r2.Read
                    sw.WriteLine("File=" + r2.GetString(0))
                Loop
            End If

            'Paths (Games catalog)
            If table_paths_exist Then
                r2 = db.queryReader("SELECT name, value FROM paths WHERE main_id = " + id)
                Do While r2.Read
                    sw.WriteLine("Path=" + r2.GetString(0) + "|||" + r2.GetString(1))
                Loop
            End If

            'Status
            r2 = db.queryReader("SELECT * FROM status_data WHERE main_id = " + id)
            Do While r2.Read
                For col As Integer = 2 To r2.FieldCount - 1
                    Dim str = r2.GetValue(col).ToString.Replace(vbCrLf, "<br/>").Replace(vbCr, "<br/>").Replace(vbLf, "<br/>").Trim
                    If str <> "" Then sw.WriteLine("Status=" + r2.GetOriginalName(col) + "|||" + str)
                Next
            Loop


            c += 1
            Label1.Invoke(Sub() Label1.Text = "Status: " + c.ToString + " / " + count.ToString)
        Loop
        'FileClose(1)
        sw.Close()

        Label1.Invoke(Sub() Label1.Text = "Status: Idle")
        MsgBox("Done.")
        Me.Invoke(Sub() Me.Close())
    End Sub
    Sub Button1_Click_subBG_CSV(o As Object, e As System.ComponentModel.DoWorkEventArgs)
        Dim arr = CType(e.Argument, String())
        Dim fileName = arr(0)
        Dim fields = arr(1)
        Dim where = arr(2)
        Dim only_visible = arr(3)
        Dim frm = DirectCast(Me.Owner, Form1)

        Dim sep = TextBox1.Text
        Dim visible_only = False
        Dim visible_ids As New List(Of Integer)

        If only_visible.ToUpper = "TRUE" Then
            visible_only = True
            Label1.Invoke(Sub() Label1.Text = "Status: Get Visible IDs...")
            For Each i In frm.ListBox1.Items
                visible_ids.Add(CInt(CType(i, DataRowView).Row().Item(0)))
            Next
        End If

        Label1.Invoke(Sub() Label1.Text = "Status: Starting...")
        Dim sql = "SELECT count(id) FROM main"
        Dim count = CInt(db.queryDataset(sql).Rows(0).Item(0))

        Dim sw As New IO.StreamWriter(fileName, True, System.Text.Encoding.UTF8)

        Dim c As Integer = 0
        sql = "SELECT id, name, " + fields + " FROM main"
        Dim r = db.queryReader(sql)
        Do While r.Read
            Dim id = r.GetInt32(0)
            If visible_only AndAlso Not visible_ids.Contains(id) Then Continue Do

            Dim line = ""
            For n = 1 To r.FieldCount - 1
                If Not r.IsDBNull(n) Then line += r.Item(n).ToString()
                line += sep
            Next
            If line.Length >= sep.Length Then
                line = line.Substring(0, line.Length - sep.Length)
                sw.WriteLine(line)
            End If

            c += 1
            If c Mod 8 = 0 Then Label1.Invoke(Sub() Label1.Text = "Status: " + c.ToString + " / " + count.ToString)
        Loop

        sw.Close()

        Label1.Invoke(Sub() Label1.Text = "Status: Idle")
        MsgBox("Done.")
    End Sub
End Class