Imports System.ComponentModel

Public Class Form9_ImportCustoms
    Dim filename As String = ""
    Dim matches As New List(Of String)
    Dim customListsInFile As New Dictionary(Of String, String)
    Dim fieldsActive As New Dictionary(Of String, Boolean)
    Dim fieldsAssociation As New Dictionary(Of String, String)
    Dim onlyNoEmty As New Dictionary(Of String, Boolean)

    Public Sub New(f As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        filename = f
        Dim fieldsInFile As New Dictionary(Of String, String)
        Dim r = New IO.StreamReader(filename)
        Do While Not r.EndOfStream
            Dim l = r.ReadLine.Trim
            If l.StartsWith("[") Then
                If l.ToUpper = "[_METADATA_]" Then
                    'it's ok, just skip
                Else
                    Exit Do
                End If
            Else
                If l.ToLower.StartsWith("field=") Then
                    Dim arr = l.Substring(l.IndexOf("=") + 1).Split({"|||"}, StringSplitOptions.None)
                    fieldsInFile.Add(arr(0).Trim, arr(1).Trim)
                End If
                If l.ToLower.StartsWith("list=") Then
                    Dim arr = l.Substring(l.IndexOf("=") + 1).Split({"|||"}, StringSplitOptions.None)
                    customListsInFile.Add(arr(0).Trim, arr(1).Trim)
                End If
            End If
        Loop
        r.Close()

        Dim ini As IniFileApi = Form1.ini
        Dim fields As New Dictionary(Of String, String)
        'For type As Integer = 0 To fieldTypeArr.Count - 1
        '    For n As Integer = 1 To fieldCountArr(type)
        '        Dim v = ini.IniReadValue("Interface", fieldTypeArr(type) + n.ToString)
        '        fields.Add("data_" + fieldTypeArr(type).Substring(6).ToLower + n.ToString, v)
        '    Next
        'Next
        For Each fi In Module1.Fields
            fields.Add(fi.DBname, fi.name)
        Next

        Dim x = 17
        Dim y = 42
        TableLayoutPanel1.SuspendLayout()

        'ID and name fields
        Dim arr2() = {"ID", "Name"}
        For t = 0 To 1
            TableLayoutPanel1.Controls.Add(New Label With {.Text = ""})
            TableLayoutPanel1.Controls.Add(New CheckBox With {.Anchor = AnchorStyles.Right, .Checked = True, .Text = "Match", .Tag = arr2(t)})
            TableLayoutPanel1.Controls.Add(New Label With {.Text = arr2(t), .AutoSize = True, .Anchor = AnchorStyles.Right})
            TableLayoutPanel1.Controls.Add(New Label With {.Text = ""})
            TableLayoutPanel1.Controls.Add(New Label With {.Text = ""})
            TableLayoutPanel1.Controls.Add(New Label With {.Text = ""})
        Next

        'Other fields found in file
        For Each kv In fieldsInFile
            'Dim chk As New CheckBox With {.AutoSize = True}
            'chk.Text = kv.Key
            'chk.Location = New Point(x, y) : x += 100
            'If x > 600 Then x = 17 : y += 23
            'GroupBox1.Controls.Add(chk)

            Dim c As New CheckBox With {.Anchor = AnchorStyles.Right, .Checked = True, .Tag = kv.Key, .Text = "Import"}
            TableLayoutPanel1.Controls.Add(c)
            Dim cm As New CheckBox With {.Anchor = AnchorStyles.Right, .Checked = False, .Tag = kv.Key, .Text = "Match"}
            TableLayoutPanel1.Controls.Add(cm)
            Dim l As New Label With {.Text = kv.Key + " (" + kv.Value + ")", .AutoSize = True, .Anchor = AnchorStyles.Right}
            TableLayoutPanel1.Controls.Add(l)
            Dim l2 As New Label With {.Text = "-->", .AutoSize = True, .Anchor = AnchorStyles.Left}
            TableLayoutPanel1.Controls.Add(l2)
            Dim cmb As New ComboBox With {.Anchor = AnchorStyles.Left Or AnchorStyles.Right, .DropDownStyle = ComboBoxStyle.DropDownList}
            TableLayoutPanel1.Controls.Add(cmb)
            Dim c2 As New CheckBox With {.AutoSize = True, .Anchor = AnchorStyles.Left, .Checked = True}
            TableLayoutPanel1.Controls.Add(c2)

            AddHandler c.CheckedChanged, AddressOf ImportOrMatchCheckedChanged
            AddHandler cm.CheckedChanged, AddressOf ImportOrMatchCheckedChanged

            Dim type = kv.Key.ToLower.Substring(5, 3).Trim
            If type.ToLower = "str" Or type.ToLower = "txt" Then c2.Text = "Only add if not empty"
            If type.ToLower = "num" Or type.ToLower = "dec" Then c2.Text = "Only add if not zero"
            If type.ToLower = "boo" Then c2.Text = "Only add if true"
            For Each kv2 In fields
                If kv2.Key.Substring(5, 3) = type Then
                    cmb.Items.Add(kv2.Key + " (" + kv2.Value + ")")
                    If kv.Value.ToLower = kv2.Value.ToLower Then cmb.SelectedIndex = cmb.Items.Count - 1
                End If
            Next
        Next

        'add last line
        For t As Integer = 1 To 5
            TableLayoutPanel1.Controls.Add(New Label With {.Text = ""})
        Next
        TableLayoutPanel1.ResumeLayout()
    End Sub
    Sub ImportOrMatchCheckedChanged(o As Object, e As EventArgs)
        Dim c = DirectCast(o, CheckBox)
        If c.Checked Then
            Dim pos = TableLayoutPanel1.GetPositionFromControl(c)
            Dim c2 = DirectCast(TableLayoutPanel1.GetControlFromPosition(1 - pos.Column, pos.Row), CheckBox)
            If c2.Checked Then c2.Checked = False
        End If
    End Sub

    'GO
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        matches.Clear()
        'For Each chk In GroupBox1.Controls.OfType(Of CheckBox)
        '    If chk.Checked Then
        '        matches.Add(chk.Text.ToLower)
        '    End If
        'Next

        fieldsActive.Clear()
        onlyNoEmty.Clear()
        fieldsAssociation.Clear()
        For Each chk In TableLayoutPanel1.Controls.OfType(Of CheckBox)
            If chk.Tag Is Nothing Then Continue For 'Wrong checkbox :)

            Dim field = chk.Tag.ToString.ToLower
            Dim col = TableLayoutPanel1.GetPositionFromControl(chk).Column
            If col = 1 Then
                If chk.Checked Then matches.Add(field)
                Continue For
            End If

            Dim row = TableLayoutPanel1.GetPositionFromControl(chk).Row
            Dim cmb = DirectCast(TableLayoutPanel1.GetControlFromPosition(4, row), ComboBox)
            Dim onp = DirectCast(TableLayoutPanel1.GetControlFromPosition(5, row), CheckBox).Checked

            onlyNoEmty.Add(field, onp)
            fieldsActive.Add(field, chk.Checked)
            If cmb.SelectedIndex >= 0 Then
                Dim assoc = cmb.SelectedItem.ToString.ToLower
                fieldsAssociation.Add(field, assoc.Substring(0, assoc.IndexOf(" ")))
            End If
        Next

        If matches.Count = 0 Then MsgBox("At least one matching field must be selected. (better 2 or 3).") : Exit Sub

        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button1_Click_BG
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button1_Click_BG()
        'Reassociate custom_lists_id
        log("Begin import.", "Import")
        Dim rdr = db.queryReader("SELECT id, name FROM custom_lists ORDER BY id")
        Dim lists_id_assoc As New Dictionary(Of String, String)
        Do While rdr.Read
            For Each kv In customListsInFile
                If kv.Value.Trim.ToLower = rdr.GetString(1).Trim.ToLower Then lists_id_assoc.Add(kv.Key, rdr.GetInt32(0).ToString) : Exit For
            Next
        Loop
        For Each kv In customListsInFile
            If Not lists_id_assoc.ContainsKey(kv.Key) Then
                log("Couldn't find a custom list '" + kv.Value + "'. Creating it.", "Import")
                db.execute("INSERT INTO custom_lists (name) VALUES ('" + kv.Value.Replace("'", "''") + "')")
                lists_id_assoc.Add(kv.Key, db.getLastRowID().ToString)
            End If
        Next


        'Fill sql array data
        Dim n As Integer = 0
        Dim sql As New List(Of List(Of String))
        Dim cur_name As String = ""
        Dim cur_main As New List(Of String)
        Dim cur_list As New List(Of String)
        Dim cur_file As New List(Of String)
        Dim cur_path As New List(Of String)
        Dim cur_stat As New List(Of String)
        Dim cur_match As New List(Of String)
        Dim r = New IO.StreamReader(filename)
        log("Reading file.", "Import")
        Do While Not r.EndOfStream
            Dim l = r.ReadLine.Trim
            If l = "" Then Continue Do
            If l.StartsWith("[") Then
                If Not l.ToUpper = "[_METADATA_]" Then
                    n += 1

                    If cur_main.Count > 0 Or cur_list.Count > 0 Or cur_file.Count > 0 Or cur_path.Count > 0 Or cur_stat.Count > 0 Then
                        Dim tmp_list As New List(Of String)
                        tmp_list.Add(String.Join(" AND ", cur_match.ToArray))
                        tmp_list.Add(String.Join(",", cur_main.ToArray))
                        tmp_list.Add(String.Join(",", cur_list.ToArray))
                        tmp_list.Add(String.Join("|||", cur_file.ToArray))
                        tmp_list.Add(String.Join("<%@|@%>", cur_path.ToArray))
                        tmp_list.Add(String.Join("<%@|@%>", cur_stat.ToArray))
                        sql.Add(tmp_list)
                    End If

                    cur_match.Clear() : cur_main.Clear() : cur_list.Clear() : cur_file.Clear() : cur_path.Clear() : cur_stat.Clear()
                    If n Mod 20 = 0 Then Label1.Invoke(Sub() Label1.Text = "Status: Reading file - " + n.ToString)

                    If matches.Contains("id") Then cur_match.Add("id = " + l.Substring(1, l.Length - 2))
                End If
            Else
                Dim lSub = l.Substring(l.IndexOf("=") + 1).Trim
                If l.ToLower.StartsWith("name=") Then
                    cur_name = lSub
                    If matches.Contains("name") Then cur_match.Add("name = '" + cur_name.Replace("'", "''") + "'")
                ElseIf l.ToLower.StartsWith("data_") Then
                    Dim field_orig = l.Substring(0, l.IndexOf("=")).Trim
                    Dim field_assoc = field_orig
                    If fieldsAssociation.ContainsKey(field_orig.ToLower) Then field_assoc = fieldsAssociation(field_orig.ToLower) Else Continue Do

                    If matches.Contains(field_orig.ToLower) Then
                        cur_match.Add(field_assoc + " = '" + lSub.Replace("'", "''") + "'")
                    ElseIf fieldsActive.ContainsKey(field_orig.ToLower) AndAlso fieldsActive(field_orig.ToLower) Then
                        Dim needAdd = True
                        If onlyNoEmty.ContainsKey(field_orig.ToLower) AndAlso onlyNoEmty(field_orig.ToLower) Then
                            If (field_orig.ToLower.Substring(5, 3) = "str" Or field_orig.ToLower.Substring(5, 3) = "txt") And lSub.Trim = "" Then needAdd = False
                            If (field_orig.ToLower.Substring(5, 3) = "dec" Or field_orig.ToLower.Substring(5, 3) = "num") And lSub.Trim = "0" Then needAdd = False
                            If field_orig.ToLower.Substring(5, 3) = "boo" And lSub.Trim.ToLower = "false" Then needAdd = False
                        End If
                        If needAdd Then cur_main.Add(field_assoc + " = '" + lSub.Replace("'", "''") + "'")
                    End If
                ElseIf l.ToLower.StartsWith("custom_list='" + lSub.Replace("'", "''") + "'") Then
                    cur_list.Add(lSub)
                ElseIf l.ToLower.StartsWith("file=") Then
                    cur_file.Add(lSub.Replace("'", "''"))
                ElseIf l.ToLower.StartsWith("path=") Then
                    cur_path.Add(lSub)
                ElseIf l.ToLower.StartsWith("status=") Then
                    cur_stat.Add(lSub.Replace("'", "''").Replace("|||", "|||'") + "'")
                End If
            End If
        Loop
        r.Close()

        'Add last entry
        If cur_main.Count > 0 Or cur_list.Count > 0 Or cur_file.Count > 0 Or cur_path.Count > 0 Or cur_stat.Count > 0 Then
            Dim tmp_list2 As New List(Of String)
            tmp_list2.Add(String.Join(" AND ", cur_match.ToArray))
            tmp_list2.Add(String.Join(",", cur_main.ToArray))
            tmp_list2.Add(String.Join(",", cur_list.ToArray))
            tmp_list2.Add(String.Join(",", cur_file.ToArray))
            tmp_list2.Add(String.Join(",", cur_path.ToArray))
            tmp_list2.Add(String.Join(",", cur_stat.ToArray))
            sql.Add(tmp_list2)
        End If

        'Update database from created array
        log("Begin sql transaction.", "Import")
        'db.execute("BEGIN;")
        db.transactionBegin()

        Dim updated As Integer = 0
        Dim updated_fail As Integer = 0
        Dim updated_fail_add As Integer = 0
        For i As Integer = 0 To sql.Count - 1
            Dim t = i 'if not, it complains about using iteration variable in lambda expression
            If (t + 1) Mod 20 = 0 Then Label1.Invoke(Sub() Label1.Text = "Status: Update data - " + (t + 1).ToString + " / " + sql.Count.ToString)
            Dim cur = sql(i)

            If cur(1).Trim <> "" Then
                'main
                Dim q As String = "UPDATE main SET " + cur(1) + " WHERE " + cur(0)
                db.execute(q, True)
                If db.affectedRows = 0 Then log("Couldn't find a game with " + cur(0), "Import") : updated_fail += 0 Else updated += 1
            End If
            If cur(2).Trim <> "" Or cur(3).Trim <> "" Or cur(4).Trim <> "" Or cur(5).Trim <> "" Then
                Dim rd = db.queryReader("SELECT id FROM main WHERE " + cur(0))
                If rd.HasRows Then
                    rd.Read()
                    Dim id = rd.GetInt32(0).ToString
                    If cur(2).Trim <> "" Then
                        'custom lists
                        For Each list In cur(2).Split({","}, StringSplitOptions.RemoveEmptyEntries)
                            If Not lists_id_assoc.ContainsKey(list.Trim) Then
                                log("Couldn't find a custom list with ID " + list.Trim + ". Creating a new one: 'New Imported List " + list.Trim + "'", "Import")
                                db.execute("INSERT INTO custom_lists (name) VALUES ('New Imported List " + list.Trim.Replace("'", "''") + "')", True)
                                lists_id_assoc.Add(list.Trim, db.getLastRowID(True).ToString)
                            End If

                            Dim list_local = lists_id_assoc(list.Trim)
                            rdr = db.queryReader("SELECT id_main FROM custom_lists_data WHERE id_main = " + id + " AND id_list = " + list_local)
                            If Not rdr.HasRows Then db.execute("INSERT INTO custom_lists_data (id_main, id_list) VALUES (" + id + ", " + list_local + ")", True)
                        Next
                    End If
                    If cur(3).Trim <> "" Then
                        'file table
                        For Each f As String In cur(3).Split({"|||"}, StringSplitOptions.RemoveEmptyEntries)
                            rdr = db.queryReader("SELECT main_id FROM files WHERE main_id = " + id + " AND file = '" + f + "' COLLATE NOCASE")
                            If Not rdr.HasRows Then db.execute("INSERT INTO files (main_id, file) VALUES (" + id + ", '" + f + "')", True)
                        Next
                    End If
                    If cur(4).Trim <> "" Then
                        'paths table
                        For Each q As String In cur(4).Split({"<%@|@%>"}, StringSplitOptions.RemoveEmptyEntries)
                            Dim arr = q.Split({"|||"}, StringSplitOptions.None)
                            If arr.Count = 2 Then
                                rdr = db.queryReader("SELECT main_id FROM paths WHERE main_id = " + id + " AND name = '" + arr(0).Trim.Replace("'", "''") + "' COLLATE NOCASE AND value = '" + arr(1).Trim.Replace("'", "''") + "' COLLATE NOCASE")
                                If Not rdr.HasRows Then db.execute("INSERT INTO paths (main_id, name, value) VALUES (" + id + ", '" + arr(0).Trim.Replace("'", "''") + "', '" + arr(1).Trim.Replace("'", "''") + "')", True)
                            Else
                                Dim msg = "Something goes wrong - path split by '|||' array count != 2. Looks like corrupted data file. See log for detail."
                                MsgBox(msg) : log(msg + " Entry: '" + cur(4) + "', Precisely: " + q, "Import") : db.execute("ROLLBACK;") : Exit Sub
                            End If
                        Next
                    End If
                    If cur(5).Trim <> "" Then
                        'status table
                        Dim rd2 = db.queryReader("SELECT main_id FROM status_data WHERE main_id = " + id)
                        If rd2.HasRows Then
                            Dim sql_update = "UPDATE status_data SET " + cur(5).Replace("<%@|@%>", ", ").Replace("|||", " = ") + " WHERE main_id = " + id
                            db.execute(sql_update, True)
                            If db.affectedRows = 0 Then
                                Dim msg = "Something goes wrong - Could not update status table. Looks like corrupted data file. See log for detail."
                                MsgBox(msg) : log(msg + " Affected_rows = 0. Query: " + sql_update, "Import") : db.execute("ROLLBACK;") : Exit Sub
                            End If
                        Else
                            Dim sql_insert = "INSERT INTO status_data "
                            Dim fields As New List(Of String)
                            Dim data As New List(Of String)
                            For Each st In cur(5).Split({"<%@|@%>"}, StringSplitOptions.RemoveEmptyEntries)
                                Dim arr = st.Split({"|||"}, StringSplitOptions.None)
                                If arr.Count = 2 Then
                                    fields.Add(arr(0))
                                    data.Add(arr(1))
                                Else
                                    Dim msg = "Something goes wrong - Could not insert into status table. Looks like corrupted data file. See log for detail."
                                    MsgBox(msg) : log(msg + " split by '|||' array count != 0. Entry: '" + cur(5) + "', Precisely: " + st, "Import") : Exit Sub
                                End If
                            Next
                            sql_insert += "(main_id, " + String.Join(", ", fields.ToArray) + ") "
                            sql_insert += "VALUES (" + id + ", " + String.Join(", ", data.ToArray) + ") "
                            db.execute(sql_insert, True)
                        End If
                    End If
                Else
                    updated_fail_add += 1
                    log("Couldn't find a game with " + cur(0) + " for aditional lists.", "Import")
                End If
            End If
        Next

        log("Import processor result below,", "Import")
        log("_Processed data: File contains " + n.ToString + " entries.", "Import")
        log("_Selected based on update criteria: " + sql.Count.ToString + " entries.", "Import")
        log("_Successfully updated: " + updated.ToString + ".", "Import")
        log("_Couldn't match: " + updated_fail.ToString + " (main table) and " + updated_fail_add.ToString + " for additional tables.", "Import")
        log("Commiting transaction to db...", "Import")
        Label1.Invoke(Sub() Label1.Text = "Status: Commit data...")
        'db.execute("COMMIT;")
        db.transactionCommit()
        Label1.Invoke(Sub() Label1.Text = "Status: Idle")
        log("End import.", "Import")
    End Sub
End Class