Imports System.ComponentModel
Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form6_Scanner_daz
    Dim ini As IniFileApi = Form1.ini
    Dim WithEvents bg As New BackgroundWorker
    Dim fileList As New List(Of String)
    Dim refreshing As Boolean = True
    Dim realFieldNames As New List(Of String)

    Private Sub Form6_Scanner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = ini.IniReadValue("Paths", "LastScannerPath")
        TextBox2.Text = ini.IniReadValue("Paths", "LastScannerDirFilePath")
        TextBox3.Text = ini.IniReadValue("Paths", "LastScannerLibraryPath")

        'For i As Integer = 0 To fieldTypeArr.Count - 1
        '    If fieldTypeArr(i).ToUpper.Contains("BOOL") Then
        '        For n = 1 To fieldCountArr(i)
        '            Dim fName = ini.IniReadValue("Interface", fieldTypeArr(i) + n.ToString)
        '            If fName <> "" Then
        '                ComboBox1.Items.Add(fName)
        '                realFieldNames.Add("data" + fieldTypeArr(i).Substring(fieldTypeArr(i).IndexOf("_")) + n.ToString)
        '            End If
        '        Next
        '    End If
        'Next
        For Each f_bool In Fields.Where(Function(fi) fi.field_type = Field_Info.field_types.bool)
            If f_bool.name <> "" Then
                ComboBox1.Items.Add(f_bool.name)
                realFieldNames.Add(f_bool.DBname)
            End If
        Next
        ComboBox1.SelectedIndex = 0

        refreshing = False
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Button1.Enabled = False
        bg = New BackgroundWorker
        If RadioButton1.Checked Then
            'Files mode
            If Not My.Computer.FileSystem.DirectoryExists(TextBox1.Text) Then
                MsgBox("Directory not exits.")
                Exit Sub
            End If

            Label1.Text = "Retriving file list..."
            Me.Refresh() : Label1.Refresh()
            fileList = IO.Directory.GetFiles(TextBox1.Text, "*.*", IO.SearchOption.AllDirectories).ToList

            AddHandler bg.DoWork, AddressOf Button1_Click_bg_work
        ElseIf RadioButton2.Checked Then
            'TXT File mode
            If Not My.Computer.FileSystem.FileExists(TextBox2.Text) Then
                MsgBox("File not exits.")
                Exit Sub
            End If

            fileList = New List(Of String)
            Dim sr = My.Computer.FileSystem.OpenTextFileReader(TextBox2.Text)
            Do While Not sr.EndOfStream
                fileList.Add(sr.ReadLine)
            Loop
            sr.Close()

            AddHandler bg.DoWork, AddressOf Button1_Click_bg_work
        ElseIf RadioButton3.Checked Then
            'Scan library
            If Not My.Computer.FileSystem.DirectoryExists(TextBox3.Text) Then
                MsgBox("Directory not exits.")
                Exit Sub
            End If

            Label1.Text = "Retriving file list..."
            Me.Refresh() : Label1.Refresh()
            fileList = New List(Of String)

            'For Each f In My.Computer.FileSystem.GetFiles(TextBox3.Text, FileIO.SearchOption.SearchAllSubDirectories, {"*.*"})
            'fileList.Add(f)
            'Next
            fileList = IO.Directory.GetFiles(TextBox3.Text, "*.*", IO.SearchOption.AllDirectories).ToList

            AddHandler bg.DoWork, AddressOf Button1_Click_bg_work_library
        End If

        AddHandler bg.RunWorkerCompleted, Sub() Button1.Enabled = True
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button1_Click_bg_work()
        '3s_ac, bn, br_bn, br_br, br_li, br_ma, br_ob, br_sc, br_sk, br_tr, 
        'ca_ac, ca_ap, ca_bn, ca_ob, ca_sc, ca_sh, ca_sk, ds_ac, ds_ap, ds_bn, ds_cm, ds_lt, ds_mr, ds_sh, ds_tx,
        'gs_ch, lw_ac, pc_ac, ps_ac, ps_, ps_an, ps_ap, ps_bn, ps_ch, ps_dc, ps_dl, ps_fr, ps_ma, ps_mo, ps_mr, ps_pc, ps_pe, ps_tx, 
        'tg_sc, vu_ac, vu_bn, vu_sc
        Dim HAVEfield As String = ""
        Dim comboindex As Integer = ComboBox1.Invoke(Function() ComboBox1.SelectedIndex)
        If comboindex > 0 Then HAVEfield = realFieldNames(comboindex - 1)

        If CheckBox4.Checked Then db.execute("DELETE FROM paths")
        If CheckBox5.Checked Then db.execute("DELETE FROM unrecognized_files")
        If CheckBox5.Checked Then db.execute("DELETE FROM product_files_found_doubles")
        If CheckBox1.Checked And HAVEfield <> "" Then db.execute("UPDATE main SET " + HAVEfield + " = 'false'")

        Label1.Invoke(Sub() Label1.Text = "Caching database...")
        Dim hashCodes As New HashSet(Of Integer)
        Dim hashCodesOld As New HashSet(Of String)
        Dim hashIDCode As New Dictionary(Of Integer, Integer)
        Dim hashIDCodeOld As New Dictionary(Of String, Integer)
        Dim hashNamesCode As New Dictionary(Of Integer, String)
        Dim hashNamesCodeOld As New Dictionary(Of String, String)
        Dim r = db.queryReader("SELECT id, name, data_str8, data_num1 FROM main")
        Try
            Do While r.Read
                If Not r.IsDBNull(3) Then
                    Dim code = r.GetInt32(3)
                    If code <> 0 AndAlso Not hashCodes.Contains(code) Then
                        hashCodes.Add(code)
                        hashIDCode.Add(code, r.GetInt32(0))
                    End If
                End If
                If Not r.IsDBNull(2) Then
                    Dim oldCode = r.GetValue(2).ToString.ToUpper.Trim.Replace("-", "_")
                    If oldCode <> "" AndAlso Not hashCodesOld.Contains(oldCode) Then
                        hashCodesOld.Add(oldCode)
                        hashIDCodeOld.Add(oldCode, r.GetInt32(0))
                    End If
                End If
            Loop
        Catch ex As Exception
            MsgBox(ex.Message)
            Exit Sub
        End Try

        'Get paths (to check if it already exist)
        Dim paths As New List(Of String)
        Dim r_path = db.queryReader("SELECT DISTINCT main_id, value FROM paths")
        While r_path.Read
            paths.Add(r_path.GetInt32(0).ToString() + "|" + r_path.GetString(1).ToUpper())
        End While

        Dim n As Integer = 0
        Dim id As Integer = 0
        Dim sql As String = ""
        Dim found As Boolean = False
        Dim found_n As Integer = 0
        Dim found_but_already_present As Integer = 0
        Dim foundIDs As New List(Of Integer)
        Dim foundIDs_fileReference As New List(Of Integer)
        Dim doubles As New Dictionary(Of Integer, List(Of String))
        Dim old_code_arr() = {"3s_ac",
                            "br_bn", "br_br", "br_li", "br_ma", "br_ob", "br_sc", "br_sk", "br_tr",
                            "ca_ac", "ca_ap", "ca_bn", "ca_ob", "ca_sc", "ca_sh", "ca_sk", "ds_ac", "ds_ap", "ds_bn", "ds_cm", "ds_lt", "ds_mr", "ds_sh", "ds_tx",
                            "gs_ch", "lw_ac", "pc_ac", "ps_ac", "ps_an", "ps_ap", "ps_bn", "ps_ch", "ps_dc", "ps_dl", "ps_fr", "ps_ma", "ps_mo", "ps_mr", "ps_pc", "ps_pe", "ps_tx",
                            "tg_sc", "vu_ac", "vu_bn", "vu_sc", "ro_"}

        Dim file As String = ""
        Dim biggestNumber As String = ""
        Try
            db.execute("BEGIN;")
            For Each f As String In fileList
                If f.ToUpper.EndsWith(".JPG") Then n += 1 : Continue For
                If f.ToUpper.EndsWith(".JPEG") Then n += 1 : Continue For
                If f.ToUpper.EndsWith(".PNG") Then n += 1 : Continue For
                If f.ToUpper.EndsWith(".URL") Then n += 1 : Continue For
                If f.ToUpper.EndsWith(".DB") Then n += 1 : Continue For

                found = False
                file = f.Substring(f.LastIndexOf("\") + 1).ToLower.Replace("-", "_")
                If n Mod 128 = 0 Then Label1.Invoke(Sub() Label1.Text = file)

                'Check old code (ps_ac)
                For Each old_code In old_code_arr
                    If file.Contains(old_code) Then
                        Dim ind As Integer = file.IndexOf(old_code) + old_code.Length
                        Do While IsNumeric(file.Substring(ind, 1))
                            ind += 1
                        Loop
                        Dim code = file.Substring(file.IndexOf(old_code), ind - file.IndexOf(old_code))
                        code = code.ToUpper
                        If hashCodesOld.Contains(code) Then
                            id = hashIDCodeOld(code)
                            found = True : found_n += 1
                        ElseIf hashCodesOld.Contains(code + "B") Then
                            id = hashIDCodeOld(code + "B")
                            found = True : found_n += 1
                        End If
                        Exit For
                    End If
                Next

                'Check sku code (im0000xx)
                If Not found Then
                    biggestNumber = ""
                    Dim collection As MatchCollection = Regex.Matches(file, "\d+")
                    For Each m As Match In collection
                        If m.ToString.Length > biggestNumber.Length Then biggestNumber = m.ToString
                    Next

                    Dim condition As Boolean = biggestNumber.Length >= 5
                    Dim condition_numIsFirst As Boolean = file.IndexOf(biggestNumber) = 0 OrElse file.Substring(file.IndexOf(biggestNumber) - 1, 1) = "_"
                    condition = condition Or (biggestNumber.Length >= 4 And condition_numIsFirst)
                    If condition AndAlso CLng(biggestNumber) < 2000000000 Then
                        Dim code = CInt(biggestNumber)
                        If hashCodes.Contains(code) Then
                            id = hashIDCode(code)
                            found = True : found_n += 1
                        End If
                    End If
                End If

                'Doubles handle
                If found Then
                    If foundIDs.Contains(id) Then
                        If Not doubles.ContainsKey(id) Then
                            doubles.Add(id, New List(Of String))
                            Dim fff = fileList(foundIDs_fileReference(foundIDs.IndexOf(id)))
                            doubles(id).Add(fff)
                            Dim _p = IO.Path.GetDirectoryName(fff).Replace("'", "''")
                            Dim _f = IO.Path.GetFileName(fff).Replace("'", "''")
                            db.execute("INSERT INTO product_files_found_doubles (id_main, path, file) VALUES (" + id.ToString + ", '" + _p + "', '" + _f + "')")
                        End If
                        doubles(id).Add(f)
                        Dim _p2 = IO.Path.GetDirectoryName(f).Replace("'", "''")
                        Dim _f2 = IO.Path.GetFileName(f).Replace("'", "''")
                        db.execute("INSERT INTO product_files_found_doubles (id_main, path, file) VALUES (" + id.ToString + ", '" + _p2 + "', '" + _f2 + "')")
                    Else
                        foundIDs.Add(id)
                        foundIDs_fileReference.Add(n)
                    End If
                End If

                'Update HAVE field and add to 'paths' table, or add to unrecognized_files if not recognized
                If found Then
                    If HAVEfield <> "" Then
                        db.execute("UPDATE main SET " + HAVEfield + " = 'true' WHERE id = " + id.ToString)
                    End If
                    If paths.Contains(id.ToString() + "|" + f.ToUpper()) Then
                        found_but_already_present += 1
                    Else
                        db.execute("INSERT INTO paths (main_id, name, value) VALUES (" + id.ToString() + ", 'Scanned File', '" + f.Replace("'", "''") + "')")
                    End If
                Else
                    db.execute("INSERT INTO unrecognized_files (path, file) VALUES ('" + IO.Path.GetDirectoryName(f).Replace("'", "''") + "', '" + file.Replace("'", "''") + "')")
                End If

                n += 1
                If n Mod 128 = 0 Then Label2.Invoke(Sub() Label2.Text = "Scanned: " + n.ToString + ", Found: " + found_n.ToString + "/" + fileList.Count.ToString)
            Next
            db.execute("COMMIT;")
        Catch ex As Exception
            MsgBox(ex.Message + vbCrLf + vbCrLf + "Last sql:" + vbCrLf + sql) : Exit Sub
        End Try
        Label1.Invoke(Sub() Label1.Text = "DONE")
        Label2.Invoke(Sub() Label2.Text = "Scanned: " + n.ToString + ", Found: " + found_n.ToString + ", Already in DB: " + found_but_already_present.ToString())
    End Sub
    Private Sub Button1_Click_bg_work_library()
        Dim HAVEfield As String = ""
        Dim comboindex As Integer = ComboBox1.Invoke(Function() ComboBox1.SelectedIndex)
        If comboindex > 0 Then HAVEfield = realFieldNames(comboindex - 1)

        If CheckBox1.Checked And HAVEfield <> "" Then db.execute("UPDATE main SET " + HAVEfield + " = 'false'")
        db.execute("DELETE FROM product_files_found")
        db.execute("DELETE FROM product_files_found_details")
        db.execute("DELETE FROM product_files_unrecognized")

        Dim substr = TextBox3.Text.Trim.Length
        If TextBox3.Text.Trim.EndsWith("\") Then substr -= 1

        Dim found_products As New List(Of String)
        Dim found_products_id As New List(Of Integer)
        Dim noMoveable = {"/DATA", "/RUNTIME/GEOMETRIES", "/RUNTIME/LIBRARIES", "/RUNTIME/PYTHON", "/RUNTIME/TEXTURES", "/RUNTIME/UI"}
        Dim c As Integer = 0
        Dim found As Integer = 0

        Label1.Invoke(Sub() Label1.Text = "Normilize filenames...")
        For i As Integer = fileList.Count - 1 To 0 Step -1
            fileList(i) = fileList(i).ToUpper.Replace("\", "/").Substring(substr)
            If CheckBox3.Checked Then
                Dim isNom = False
                For Each nom In noMoveable
                    If fileList(i).StartsWith(nom) Then isNom = True : Exit For
                Next
                If Not isNom Then fileList.RemoveAt(i)
            End If
        Next

        Dim where = ""
        Dim _filelist = New HashSet(Of String)(fileList)
        Dim filesFound As New Dictionary(Of String, List(Of String))
        Dim filesTotal As New Dictionary(Of String, Integer)
        Dim filesUnrecognized = New HashSet(Of String)(fileList)
        Dim filesUnrecognizedProducts As New List(Of String)
        Dim cur_idMain As Integer = -1
        Dim cur_subName As String = ""

        Label1.Invoke(Sub() Label1.Text = "Query Database...")
        Dim sql = "SELECT id_main, subName, file FROM product_files "
        If CheckBox3.Checked Then
            where += "WHERE "
            For Each nom In noMoveable
                where += "file like '" + nom + "%' OR "
            Next
            where = where.Substring(0, where.Length - 3)
        End If
        sql += where
        sql += "GROUP BY file "
        sql += "HAVING COUNT(file) = 1 "
        sql += "ORDER BY id_main, subName "

        Dim r = db.queryReader(sql)
        Dim total = db.queryDataset("Select count(*) FROM product_files " + where).Rows(0).Item(0).ToString

        Label1.Invoke(Sub() Label1.Text = "Found: 0")
        Try
            Do While r.Read
                Dim id = r.GetInt32(0)
                Dim id_main = r.GetInt32(0)
                Dim subName = r.GetString(1)
                Dim file = r.GetString(2).ToUpper
                If cur_idMain <> cur_idMain Or cur_subName <> subName Then
                    cur_idMain = id
                    cur_subName = subName
                    filesTotal.Add(cur_idMain.ToString + "---" + cur_subName, 0)
                End If
                filesTotal(cur_idMain.ToString + "---" + cur_subName) += 1

                c += 1
                If c Mod 65536 = 0 Then Label2.Invoke(Sub() Label2.Text = c.ToString + "/" + total + " - " + file)

                If _filelist.Contains(file) Then
                    Dim r1 = db.queryReader("SELECT id, name FROM main where id = " + id.ToString)
                    If r1.HasRows Then
                        r1.Read()
                        If Not found_products.Contains(r1.GetString(1)) Then
                            found_products_id.Add(r1.GetInt32(0))
                            found_products.Add(r1.GetString(1))
                            found += 1
                        End If
                        If Not filesFound.ContainsKey(cur_idMain.ToString + "---" + cur_subName) Then filesFound.Add(cur_idMain.ToString + "---" + cur_subName, New List(Of String))
                        filesFound(cur_idMain.ToString + "---" + cur_subName).Add(file)
                    Else
                        filesUnrecognizedProducts.Add(id.ToString)
                    End If

                    If filesUnrecognized.Contains(file) Then filesUnrecognized.Remove(file)
                    Label1.Invoke(Sub() Label1.Text = "Found: " + found.ToString)
                End If
            Loop
        Catch ex As Exception
            MsgBox(ex.Message)
            Exit Sub
        End Try


        Label1.Invoke(Sub() Label1.Text = "Updating database...")
        db.execute("BEGIN;")
        c = 0
        Dim product_files As New Dictionary(Of String, List(Of String))
        Try
            For Each k In filesFound.Keys
                Dim arr = k.Split({"---"}, StringSplitOptions.RemoveEmptyEntries)
                sql = "INSERT INTO product_files_found (id_main, subName, files_found, files) VALUES "
                sql += "( " + arr(0) + ", '" + arr(1).Replace("'", "''") + "', " + filesFound(k).Count.ToString + ", " + filesTotal(k).ToString + " )"
                db.execute(sql)

                If CheckBox2.Checked Then
                    sql = "SELECT file FROM product_files WHERE id_main = " + arr(0) + " AND subName = '" + arr(1).Replace("'", "''") + "' "
                    If CheckBox3.Checked Then sql += "AND (" + where.Substring(6) + " )"

                    r = db.queryReader(sql)
                    product_files.Add(k, New List(Of String))
                    Do While r.Read
                        product_files(k).Add(r.GetString(0))

                        sql = "INSERT INTO product_files_found_details (id_main, subName, file, found) VALUES "
                        sql += "( " + arr(0) + ", '" + arr(1).Replace("'", "''") + "', '" + r.GetString(0).Replace("'", "''") + "', "
                        If filesFound(k).Contains(r.GetString(0).ToUpper) Then
                            sql += "'TRUE' )"
                        Else
                            sql += "'FALSE' )"
                        End If
                        db.execute(sql)
                    Loop

                    c += 1
                    Label2.Invoke(Sub() Label2.Text = c.ToString + "/" + filesFound.Count.ToString)
                End If
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
            Exit Sub
        End Try
        db.execute("COMMIT;")

        db.execute("BEGIN;")
        For Each unz In filesUnrecognized
            sql = "INSERT INTO product_files_unrecognized (file) VALUES ('" + unz.Replace("'", "''") + "')"
            db.execute(sql)
        Next
        db.execute("COMMIT;")

        Label1.Invoke(Sub() Label1.Text = "Done.")
        Label2.Invoke(Sub() Label2.Text = "Done.")
    End Sub

    'Save paths to ini on change
    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerPath", TextBox1.Text)
    End Sub
    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerDirFilePath", TextBox2.Text)
    End Sub
    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If refreshing Then Exit Sub
        ini.IniWriteValue("Paths", "LastScannerLibraryPath", TextBox3.Text)
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If RadioButton3.Checked Then
            CheckBox2.Enabled = True
            CheckBox3.Enabled = True
        Else
            CheckBox2.Enabled = False
            CheckBox3.Enabled = False
        End If
    End Sub
End Class