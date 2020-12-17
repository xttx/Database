Public Class Form7_OptionsGroupping
    Dim ini As IniFileApi = Form1.ini
    Dim realFieldNames As New List(Of String)
    Dim fieldsIsList As New List(Of String)
    Dim refreshing As Boolean = False
    Public Shared need_refresh_main_form = False

    Private Sub Form7_OptionsGroupping_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Fill field list with frendly names and store real names in arraylist
        For Each fi In Fields
            If fi.is_list Then fieldsIsList.Add(fi.DBname.ToUpper)
            If fi.name.Trim <> "" Then
                ComboBox1.Items.Add(fi.name)
                realFieldNames.Add(fi.DBname)
            End If
        Next
        'Dim fieldTypeN As Integer = 0
        'For Each fieldType In fieldTypeArr
        '    For i As Integer = 1 To fieldCountArr(fieldTypeN)
        '        Dim f As String = fieldType + i.ToString
        '        Dim t = ini.IniReadValue("Interface", f)
        '        Dim t1 = ini.IniReadValue("Interface", f + "_isList")
        '        If t1.ToUpper = "TRUE" Or t1 = "1" Then fieldsIsList.Add("DATA" + f.Substring(f.IndexOf("_")).ToUpper)
        '        If t <> "" Then
        '            ComboBox1.Items.Add(t)
        '            realFieldNames.Add("data" + f.Substring(f.IndexOf("_")))
        '        End If
        '    Next
        '    fieldTypeN += 1
        'Next

        'Get groups from db
        Dim sql = "Select name, field, value FROM groups"
        Dim r = db.queryReader(sql)
        Do While r.Read
            Dim g As New group
            g.name = r.GetString(0)
            g.field = r.GetString(1)
            'g.fieldFriendlyName = ini.IniReadValue("Interface", g.field.Replace("data", "field"))
            g.values = r.GetValue(2).ToString

            Dim f_name As String = ""
            Dim f_info = Fields.Where(Function(fi) fi.DBname.ToUpper = g.field.ToUpper()).FirstOrDefault()
            If f_info IsNot Nothing Then g.fieldFriendlyName = f_info.name

            ListBox1.Items.Add(g)
        Loop

        If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0
        If ListBox1.Items.Count > 0 Then ListBox1.SelectedIndex = 0
    End Sub

    'Listbox group browse
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If refreshing Then Exit Sub
        If ListBox1.SelectedIndex < 0 Then
            ListBox2.Items.Clear()
            ListBox3.DataSource = Nothing
            ListBox3.Items.Clear()
            Exit Sub
        End If

        Dim g = DirectCast(ListBox1.SelectedItem, group)
        ListBox2.Items.Clear()
        For Each v In g.values.Split({";;%;;"}, StringSplitOptions.RemoveEmptyEntries)
            ListBox2.Items.Add(v)
        Next


        Dim dt = db.queryDataset("Select DISTINCT " + g.field + " FROM main ORDER BY " + g.field)

        If fieldsIsList.Contains(g.field.ToUpper) Then
            Dim dtMod As New DataTable
            Dim alreadyInsertedValues As New List(Of String)
            dtMod.Columns.Add(g.field)
            For Each r As DataRow In dt.Rows
                For Each subItem In r.Item(0).ToString.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
                    If Not alreadyInsertedValues.Contains(subItem.Trim.ToUpper) Then
                        alreadyInsertedValues.Add(subItem.Trim.ToUpper)
                        dtMod.Rows.Add(subItem.Trim)
                    End If
                Next
            Next

            Dim dv = dtMod.DefaultView
            dv.Sort = g.field
            dt = dv.ToTable
        End If

        For i As Integer = dt.Rows.Count - 1 To 0 Step -1
            If ListBox2.Items.Contains(dt.Rows(i).Item(0).ToString) Then dt.Rows.RemoveAt(i)
        Next
        ListBox3.DataSource = dt
        ListBox3.DisplayMember = g.field
        Label4.Text = "Values count " + dt.Rows.Count.ToString
    End Sub

    'Add group
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text.Trim = "" Then MsgBox("You need To Set group name.") : Exit Sub
        If ComboBox1.SelectedIndex < 0 Then MsgBox("You need To Set a field For this group") : Exit Sub
        need_refresh_main_form = True

        Dim g As New group
        g.name = TextBox1.Text.Trim
        g.field = realFieldNames(ComboBox1.SelectedIndex)

        'g.fieldFriendlyName = ini.IniReadValue("Interface", g.field.Replace("data", "field"))
        Dim f_name As String = ""
        Dim f_info = Fields.Where(Function(fi) fi.DBname.ToUpper = g.field.ToUpper()).FirstOrDefault()
        If f_info IsNot Nothing Then g.fieldFriendlyName = f_info.name

        ListBox1.Items.Add(g)
        db.execute("INSERT INTO groups (name, field) VALUES ('" + g.name.Replace("'", "''") + "', '" + g.field + "')")
    End Sub
    'Change group name
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        If TextBox1.Text.Trim = "" Then MsgBox("You need To Set group name.") : Exit Sub
        need_refresh_main_form = True

        Dim g As group = DirectCast(ListBox1.SelectedItem, group)
        db.execute("UPDATE groups SET name = '" + TextBox1.Text.Trim.Replace("'", "''") + "' WHERE name = '" + g.name.Replace("'", "''") + "'")
        g.name = TextBox1.Text.Trim
        TextBox1.Text = ""

        refreshing = True
        ListBox1.Items(ListBox1.SelectedIndex) = ListBox1.SelectedItem
        refreshing = False
    End Sub
    'Remove group
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        need_refresh_main_form = True

        Dim g As group = DirectCast(ListBox1.SelectedItem, group)
        db.execute("DELETE FROM groups WHERE name = '" + g.name.Replace("'", "''") + "'")

        Dim ind = ListBox1.SelectedIndex
        ListBox1.Items.RemoveAt(ind)
        If ListBox1.Items.Count > ind Then
            ListBox1.SelectedIndex = ind
        ElseIf ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = ListBox1.Items.Count - 1
        End If
    End Sub

    'Add value
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        If ListBox3.SelectedIndex < 0 Then Exit Sub
        need_refresh_main_form = True

        Dim g As group = DirectCast(ListBox1.SelectedItem, group)
        Dim dr = DirectCast(ListBox3.SelectedItem, DataRowView).Row

        ListBox2.Items.Add(dr.Item(0).ToString)
        Dim tmp As String = ""
        For Each v In ListBox2.Items
            tmp = tmp + v + ";;%;;"
        Next
        If tmp.Length > 3 Then tmp = tmp.Substring(0, tmp.Length - 5)
        g.values = tmp

        db.execute("UPDATE groups SET value = '" + tmp.Replace("'", "''") + "' WHERE name = '" + g.name.Replace("'", "''") + "'")
        dr.Delete()
    End Sub
    'Remove value
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        If ListBox2.SelectedIndex < 0 Then Exit Sub
        need_refresh_main_form = True

        Dim dt = DirectCast(ListBox3.DataSource, DataTable)
        dt.Rows.Add({ListBox2.SelectedItem.ToString})
        Dim dv = dt.DefaultView
        dv.Sort = dt.Columns(0).ColumnName
        ListBox3.DataSource = dv.ToTable

        Dim ind = ListBox2.SelectedIndex
        ListBox2.Items.RemoveAt(ind)
        If ListBox2.Items.Count > ind Then
            ListBox2.SelectedIndex = ind
        ElseIf ListBox2.Items.Count > 0 Then
            ListBox2.SelectedIndex = ListBox2.Items.Count - 1
        End If

        Dim g As group = DirectCast(ListBox1.SelectedItem, group)
        Dim tmp As String = ""
        For Each v In ListBox2.Items
            tmp = tmp + v + ";;%;;"
        Next
        If tmp.Length > 3 Then tmp = tmp.Substring(0, tmp.Length - 5)
        g.values = tmp
        db.execute("UPDATE groups SET value = '" + tmp.Replace("'", "''") + "' WHERE name = '" + g.name.Replace("'", "''") + "'")
    End Sub
End Class

Public Class group
    Public name As String = ""
    Public field As String = ""
    Public fieldFriendlyName As String = ""
    Public values As String = ""

    Public Overrides Function ToString() As String
        Return name + " (" + fieldFriendlyName + ")"
    End Function
End Class