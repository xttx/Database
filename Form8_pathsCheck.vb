Imports System.ComponentModel

Public Class Form8_pathsCheck

    'Check path doubles
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        DataGridView1.DataSource = Nothing
        DataGridView1.Rows.Clear()
        DataGridView1.Columns.Clear()
        Dim sql = "SELECT lower(value) as Path, count(paths.id) as Count FROM paths GROUP BY lower(value) HAVING count(paths.id) > 1"
        DataGridView1.DataSource = db.queryDataset(sql)
        DataGridView1.Columns(0).Width = 300
        Label2.Text = "Rows: " + DataGridView1.Rows.Count.ToString()
        If DataGridView1.Rows.Count > 0 Then Button2.Enabled = True
    End Sub

    'Check path doubles in same game
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        DataGridView1.DataSource = Nothing
        DataGridView1.Rows.Clear()
        DataGridView1.Columns.Clear()
        Dim sql = "SELECT main.name as Game, lower(value) as Path, count(paths.id) as Count FROM paths JOIN main ON main.id = main_id GROUP BY main_id, lower(value) HAVING count(paths.id) > 1"
        DataGridView1.DataSource = db.queryDataset(sql)
        Label2.Text = "Rows: " + DataGridView1.Rows.Count.ToString()
        If DataGridView1.Rows.Count > 0 Then Button2.Enabled = True
    End Sub
    'Clear path doubles in same game
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Button2.Enabled = False
        Dim bg As New BackgroundWorker
        AddHandler bg.DoWork, AddressOf Button2_Click_BG
        AddHandler bg.RunWorkerCompleted, Sub() Button1_Click(Button1, New EventArgs)
        bg.RunWorkerAsync()
    End Sub
    Private Sub Button2_Click_BG()
        Dim c As Integer = 0
        Dim count = " of " + DataGridView1.Rows.Count.ToString()

        Dim r = db.queryReader("SELECT main_id, lower(value) FROM paths GROUP BY main_id, lower(value) HAVING count(paths.id) > 1")
        db.execute("BEGIN;")
        While r.Read
            c += 1
            If c Mod 10 = 0 Then Label2.Invoke(Sub() Label2.Text = "Processing: " + c.ToString() + count)

            Dim sql = "SELECT id FROM paths WHERE main_id = " + r.GetInt32(0).ToString() + " AND value LIKE '" + r.GetString(1).Replace("'", "''") + "' ORDER BY id;"
            Dim ds = db.queryDataset(sql)
            If ds.Rows.Count > 1 Then
                For i As Integer = 1 To ds.Rows.Count - 1
                    db.execute("DELETE FROM paths WHERE id = " + ds.Rows(i).Item(0).ToString() + ";")
                Next
            End If
        End While
        db.execute("COMMIT;")
    End Sub

    'Check path presence in db
    Private Sub Label1_DragEnter(sender As Object, e As DragEventArgs) Handles Label1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub Label1_DragDrop(sender As Object, e As DragEventArgs) Handles Label1.DragDrop
        DataGridView1.DataSource = Nothing
        DataGridView1.Rows.Clear()
        DataGridView1.Columns.Clear()
        DataGridView1.Columns.Add("Game", "Game")
        DataGridView1.Columns.Add("Folder", "Folder")
        DataGridView1.Columns.Add("In DB", "In DB")

        If e.Data.GetDataPresent(DataFormats.FileDrop) AndAlso e.Data.GetData(DataFormats.FileDrop) IsNot Nothing Then
            Dim f = e.Data.GetData(DataFormats.FileDrop)(0).ToString
            If FileIO.FileSystem.DirectoryExists(f) Then
                Dim dirs = FileIO.FileSystem.GetDirectories(f, FileIO.SearchOption.SearchTopLevelOnly)
                For Each d In dirs
                    Dim dir_only = IO.Path.GetFileName(d)
                    Dim sql = "SELECT main.name, value FROM paths JOIN main ON main.id = main_id WHERE lower(value) = '" + d.ToLower.Replace("'", "''") + "'"
                    Dim rdr = db.queryReader(sql)
                    If rdr.HasRows Then
                        Do While rdr.Read
                            DataGridView1.Rows.Add({rdr.GetString(0), dir_only, "YES"})
                        Loop
                    Else
                        DataGridView1.Rows.Add({"%NONE%", dir_only, "NO"})
                    End If
                Next
            End If
        End If
    End Sub
End Class