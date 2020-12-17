Public Class Form6_Scanner_Details
    Dim productHash As New Dictionary(Of String, Dictionary(Of String, List(Of String)))
    Dim productFoundFiles As New Dictionary(Of String, Dictionary(Of String, List(Of String)))

    Private Sub Form6_Scanner_Details_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sql = "SELECT main.name, subName, file, found FROM product_files_found_details "
        sql += "JOIN main ON main.id = id_main "
        sql += "ORDER BY name, subName"
        Try
            Dim r = db.queryReader(sql)
            If Not r.HasRows Then MsgBox("There is no any details info found. Run library scan with generate details option on") : Exit Sub

            Dim curName As String = ""
            Dim curSubName As String = ""
            Do While r.Read
                If curName <> r.GetString(0) Then
                    curName = r.GetString(0)
                    curSubName = r.GetString(1)
                    productHash.Add(curName, New Dictionary(Of String, List(Of String)))
                    productHash(curName).Add(curSubName, New List(Of String))
                    productFoundFiles.Add(curName, New Dictionary(Of String, List(Of String)))
                    productFoundFiles(curName).Add(curSubName, New List(Of String))
                End If
                If curSubName <> r.GetString(1) Then
                    curSubName = r.GetString(1)
                    productHash(curName).Add(curSubName, New List(Of String))
                    productFoundFiles(curName).Add(curSubName, New List(Of String))
                End If
                productHash(curName)(curSubName).Add(r.GetString(2))
                If r.GetString(3).Trim = "TRUE" Then productFoundFiles(curName)(curSubName).Add(r.GetString(2))
            Loop
        Catch ex As Exception
            MsgBox(ex.Message) : Exit Sub
        End Try

        For Each k In productHash.Keys
            Dim allFilesFound As Boolean = True
            Dim atLeastOneSubCatFound As Boolean = False
            For Each subName In productHash(k).Keys
                If productHash(k)(subName).Count = productFoundFiles(k)(subName).Count Then
                    atLeastOneSubCatFound = True
                Else
                    allFilesFound = False
                End If
            Next

            Dim l As New ListViewItem(k)
            If allFilesFound Then
                l.ForeColor = Color.DarkGreen
            ElseIf atLeastOneSubCatFound Then
                l.ForeColor = Color.Yellow
            Else
                l.ForeColor = Color.DarkRed
            End If
            ListView1.Items.Add(l)
        Next

        If ListView1.Items.Count > 0 Then ListView1.Items(0).Selected = True
    End Sub

    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged
        DataGridView1.Rows.Clear()
        If ListView1.SelectedIndices.Count <> 1 Then Exit Sub
        If ListView1.SelectedIndices(0) < 0 Then Exit Sub

        Dim name = ListView1.SelectedItems(0).Text
        For Each subName In productHash(name).Keys
            For Each file In productHash(name)(subName)
                Dim found = productFoundFiles(name)(subName).Contains(file)
                DataGridView1.Rows.Add(file, subName, found.ToString)
                Dim r = DataGridView1.Rows(DataGridView1.Rows.Count - 1)
                If found Then
                    r.Cells(0).Style.ForeColor = Color.DarkGreen
                    r.Cells(1).Style.ForeColor = Color.DarkGreen
                    r.Cells(2).Style.ForeColor = Color.DarkGreen
                Else
                    r.Cells(0).Style.ForeColor = Color.DarkRed
                    r.Cells(1).Style.ForeColor = Color.DarkRed
                    r.Cells(2).Style.ForeColor = Color.DarkRed
                End If
            Next
        Next
    End Sub
End Class