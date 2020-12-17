Public Class Form3_rawDatabaseView
    Dim db As New Class01_db
    Dim last_where As String = ""

    Private Sub Form3_rawDatabaseView_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim r = db.queryReader("SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1")

        Do While r.Read()
            ComboBox1.Items.Add(r.GetString(0))
        Loop
        If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.SelectedIndex < 0 Then Exit Sub
        TextBox1.Text = ""
        Button1_Click(Button1, New EventArgs)
    End Sub

    'Where
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim tbl As New DataTable
        If TextBox1.Text.Trim = "" Then
            tbl = db.queryDataset("SELECT * FROM " + ComboBox1.SelectedItem.ToString)
        Else
            Try
                tbl = db.queryDataset("SELECT * FROM " + ComboBox1.SelectedItem.ToString + " WHERE " + TextBox1.Text)
            Catch ex As Exception
                MsgBox("Error in WHERE clause." + vbCrLf + ex.Message)
                Exit Sub
            End Try
        End If
        last_where = TextBox1.Text.Trim
        DataGridView1.DataSource = tbl
        DataGridView1.Columns(0).ReadOnly = True
        Label3.Text = "Count: " + DataGridView1.RowCount.ToString
    End Sub
    'Query
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim tbl As New DataTable
        Dim affected_rows = ""
        If TextBox1.Text.Trim.ToUpper.StartsWith("SELECT") Then
            'MsgBox("SELECT query non implimented yet.")
            last_where = ""
            Try
                tbl = db.queryDataset(TextBox1.Text.Trim)
            Catch ex As Exception
                MsgBox("Error in query." + vbCrLf + ex.Message) : Exit Sub
            End Try
        Else
            Try
                db.execute(TextBox1.Text.Trim)
            Catch ex As Exception
                MsgBox(ex.Message)
                Exit Sub
            End Try

            affected_rows = ", Affected rows: " + db.affectedRows.ToString

            If last_where = "" Then
                tbl = db.queryDataset("SELECT * FROM " + ComboBox1.SelectedItem.ToString)
            Else
                Try
                    tbl = db.queryDataset("SELECT * FROM " + ComboBox1.SelectedItem.ToString + " WHERE " + last_where)
                Catch ex As Exception
                    MsgBox("Error in WHERE clause." + vbCrLf + ex.Message) : Exit Sub
                End Try
            End If
        End If

        DataGridView1.DataSource = tbl
        DataGridView1.Columns(0).ReadOnly = True
        Label3.Text = "Count: " + DataGridView1.RowCount.ToString + affected_rows
    End Sub

    Private Sub DataGridView1_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit
        Dim tbl = ComboBox1.SelectedItem.ToString
        Dim id = DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString
        Dim field = DataGridView1.Columns(e.ColumnIndex).DataPropertyName
        Dim fieldId = DataGridView1.Columns(0).DataPropertyName
        Dim data = DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString.Replace("'", "''")
        db.execute("UPDATE " + tbl + " SET " + field + " = '" + data + "' WHERE " + fieldId + " = " + id.ToString)
    End Sub

    Private Sub DataGridView1_UserAddedRow(sender As Object, e As DataGridViewRowEventArgs) Handles DataGridView1.UserAddedRow
        Dim tbl = ComboBox1.SelectedItem.ToString
        Dim fieldId = DataGridView1.Columns(0).DataPropertyName
        db.execute("INSERT INTO " + tbl + " (" + fieldId + ") VALUES (NULL)")
        Dim id = db.getLastRowID()
        DataGridView1.Rows(e.Row.Index - 1).Cells(0).Value = id
    End Sub

    Private Sub DataGridView1_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError
        MsgBox("Data Error!")
    End Sub


End Class