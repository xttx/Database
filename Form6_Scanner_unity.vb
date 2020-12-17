Imports System.ComponentModel
Imports System.Text.RegularExpressions

Public Class Form6_Scanner_unity
    Dim ini As IniFileApi = Form1.ini
    Dim WithEvents bg As New BackgroundWorker
    Dim fileList As New List(Of String)
    Dim refreshing As Boolean = True
    Dim realFieldNames As New List(Of String)

    Private Sub Form6_Scanner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler bg.RunWorkerCompleted, Sub() Button1.Enabled = True

        TextBox1.Text = ini.IniReadValue("Paths", "LastScannerPath")
        TextBox2.Text = ini.IniReadValue("Paths", "LastScannerDirFilePath")

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

        If RadioButton1.Checked Then
            'Files mode
            If Not My.Computer.FileSystem.DirectoryExists(TextBox1.Text) Then
                MsgBox("Directory not exits.")
                Exit Sub
            End If

            fileList = New List(Of String)
            For Each f In My.Computer.FileSystem.GetFiles(TextBox1.Text, FileIO.SearchOption.SearchAllSubDirectories, {"*.*"})
                fileList.Add(f)
            Next
            bg.RunWorkerAsync()
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
            bg.RunWorkerAsync()
        End If
    End Sub
    Private Sub Button1_Click_bg_work() Handles bg.DoWork
        Dim HAVEfield As String = ""
        Dim comboindex As Integer = ComboBox1.Invoke(Function() ComboBox1.SelectedIndex)
        If comboindex > 0 Then HAVEfield = realFieldNames(comboindex - 1)

        db.execute("DELETE FROM unrecognized_files")
        If CheckBox1.Checked And HAVEfield <> "" Then db.execute("UPDATE main SET " + HAVEfield + " = 'false'")

        Dim n As Integer = 0
        Dim found_n As Integer = 0

        Dim sql = "SELECT id, name FROM main"
        Dim r = db.queryReader(sql)
        Dim productList As New List(Of String)
        Dim productListId As New List(Of Integer)
        Do While r.Read
            Dim p = r.GetString(1).Trim.Replace(" ", "").Replace(" ", "").Replace("-", "").Replace(".", "").ToLower
            p = p.Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace(",", "")
            p = Regex.Replace(p, "[0-9]", "")
            productList.Add(r.GetString(1).Trim.Replace(" ", "").Replace(" ", "").Replace("-", "").Replace(".", "").ToLower)
            productListId.Add(r.GetInt32(0))
        Loop

        For Each f As String In fileList
            n += 1
            If f.ToUpper.EndsWith(".JPG") Then Continue For
            If f.ToUpper.EndsWith(".JPEG") Then Continue For
            If f.ToUpper.EndsWith(".PNG") Then Continue For
            If f.ToUpper.EndsWith(".URL") Then Continue For
            If f.ToUpper.EndsWith(".DB") Then Continue For

            Dim file As String = f.Substring(f.LastIndexOf("\") + 1).ToLower
            Dim normalizedfile = file
            If normalizedfile.Contains(".") Then normalizedfile = normalizedfile.Substring(0, file.LastIndexOf("."))
            normalizedfile = normalizedfile.Trim.Replace(" ", "").Replace(" ", "").Replace("-", "").Replace(".", "").ToLower
            normalizedfile = normalizedfile.Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace(",", "")
            normalizedfile = Regex.Replace(normalizedfile, "[0-9]", "")
            Label1.Invoke(Sub() Label1.Text = file)

            Dim index As Integer = productList.IndexOf(normalizedfile)
            If index >= 0 Then
                If HAVEfield <> "" Then
                    db.execute("UPDATE main SET " + HAVEfield + " = 'true' WHERE id = " + productListId(index).ToString)
                End If
                found_n += 1
            Else
                db.execute("INSERT INTO unrecognized_files (file) VALUES ('" + file.Replace("'", "''") + "')")
            End If

            Label2.Invoke(Sub() Label2.Text = "Found: " + found_n.ToString + " / " + n.ToString)
        Next
        Label1.Invoke(Sub() Label1.Text = "DONE")
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
End Class