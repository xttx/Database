Public Class Form3_customListMngr
    Dim refreshing As Boolean = False
    Public Shared need_refresh_main_form = False

    Private Sub Form3_customListMngr_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        need_refresh_main_form = False
        ListBox1.ValueMember = "id"
        ListBox1.DisplayMember = "name"
        ListBox1.DataSource = db.queryDataset("SELECT * FROM custom_lists")
        db.execute("BEGIN;")
        If ListBox1.Items.Count > 0 Then ListBox1.SelectedIndex = 0
    End Sub
    Private Sub Form3_customListMngr_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not need_refresh_main_form Then db.execute("ROLLBACK;")
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        If refreshing Then Exit Sub

        refreshing = True
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            TextBox1.Text = DirectCast(ListBox1.SelectedItem, DataRowView).Item("name").ToString
        End If
        refreshing = False
    End Sub

    'Add
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        db.execute("INSERT INTO custom_lists (name) VALUES ('New List');")
        ListBox1.DataSource = db.queryDataset("SELECT * FROM custom_lists")
        ListBox1.SelectedIndex = ListBox1.Items.Count - 1
    End Sub
    'Remove
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        Dim id = CInt(ListBox1.SelectedValue)
        db.execute("DELETE FROM custom_lists WHERE id = " + id.ToString + ";")

        'This looks much pretier, but it don't want to delete row some times, maybe it's related to rows having same name
        'Dim dt = DirectCast(ListBox1.DataSource, DataTable)
        'dt.Rows(ListBox1.SelectedIndex).Delete()

        Dim old_selected = ListBox1.SelectedIndex
        ListBox1.DataSource = db.queryDataset("SELECT * FROM custom_lists")
        If old_selected < ListBox1.Items.Count Then ListBox1.SelectedIndex = old_selected Else ListBox1.SelectedIndex = ListBox1.Items.Count - 1
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        If refreshing Then Exit Sub
        If ListBox1.SelectedIndex < 0 Then TextBox1.Text = "" : Exit Sub
        Dim id = CInt(ListBox1.SelectedValue)
        Dim sql = "UPDATE custom_lists SET name = '" + TextBox1.Text.Replace("'", "''") + "' "
        sql += "WHERE id = " + id.ToString
        db.execute(sql)

        refreshing = True
        Dim dt = DirectCast(ListBox1.DataSource, DataTable)
        dt.Rows(ListBox1.SelectedIndex).Item("name") = TextBox1.Text
        refreshing = False
    End Sub

    'OK
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        need_refresh_main_form = True
        db.execute("COMMIT;")
        Me.Close()
    End Sub
End Class