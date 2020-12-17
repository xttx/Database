Public Class Form5_statusTableSettings
    Dim ini = Form1.ini

    Private Sub Form5_statusTableSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Text = ini.IniReadValue("StatusTable", "StatusValues")

        Dim statusFields As New List(Of String)
        Dim statusFilters As New List(Of String)
        Dim tmp As String = ""
        tmp = ini.IniReadValue("StatusTable", "StatusFields")
        If tmp <> "" Then statusFields = tmp.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).ToList
        tmp = ini.IniReadValue("StatusTable", "StatusFilters")
        If tmp <> "" Then statusFilters = tmp.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).ToList

        Dim fieldTypeN As Integer = 0
        ListView1.Columns.Add("main")
        ListView1.Columns(0).Width = ListView1.Width - 20
        For Each fieldType In fieldTypeArr
            For i As Integer = 1 To fieldCountArr(fieldTypeN)
                Dim f As String = fieldType + i.ToString
                Dim t As String = ini.IniReadValue("Interface", f)
                If t <> "" Then
                    Dim lvi As New ListViewItem(f + " - " + t)
                    If statusFilters.Contains(f) Then lvi.Checked = True
                    ListView1.Items.Add(lvi)
                End If
            Next
            fieldTypeN += 1
        Next

        For Each item In statusFields
            ListBox1.Items.Add(item)
        Next
    End Sub

    'Add a field
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If TextBox2.Text.Trim = "" Then MsgBox("Field name not entered.") : Exit Sub
        ListBox1.Items.Add(TextBox2.Text.Trim)
    End Sub
    'Update a field
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        If TextBox2.Text.Trim = "" Then MsgBox("Field name not entered.") : Exit Sub
        ListBox1.Items(ListBox1.SelectedIndex) = TextBox2.Text.Trim
    End Sub
    'Remove a field
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
    End Sub

    'OK
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim tmp As String = ""
        For Each item As ListViewItem In ListView1.Items
            If item.Checked Then
                tmp += item.Text.Substring(0, item.Text.IndexOf(" ")) + ";"
            End If
        Next
        ini.IniWriteValue("StatusTable", "StatusFilters", tmp)

        tmp = ""
        For Each item In ListBox1.Items
            tmp += item.ToString + ";"
        Next
        ini.IniWriteValue("StatusTable", "StatusFields", tmp)

        ini.IniwriteValue("StatusTable", "StatusValues", TextBox1.Text.Trim)
        Me.Close()
    End Sub
End Class