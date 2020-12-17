Public Class Form4_statusTable
    Dim ini = Form1.ini
    Dim refreshing As Boolean = False
    Dim statusValues As String = ""
    Dim statusFields As New List(Of String)
    Dim statusFilters As New List(Of String)

    Dim sql As String = "SELECT * FROM main LEFT JOIN status_data ON main.id = status_data.main_id "
    Dim COMBOBOX_WIDTH As Integer = 135
    Dim COMBOBOX_OFFSET As Integer = 20

    Dim update_comments As New List(Of String)
    Dim status_where As New List(Of String)

    Dim WithEvents l As ListBox

    Public Sub New(MainForm As Form1)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        l = MainForm.ListBox1
    End Sub

    Private Sub Form4_statusTable_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        statusValues = ini.IniReadValue("StatusTable", "StatusValues")

        Dim tmp As String = ""
        tmp = ini.IniReadValue("StatusTable", "StatusFields")
        If tmp <> "" Then statusFields = tmp.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).ToList
        tmp = ini.IniReadValue("StatusTable", "StatusFilters")
        If tmp <> "" Then statusFilters = tmp.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).ToList

        Dim n As Integer = 1
        Dim anyStatusWhere = ""
        For Each stField In statusFields
            For Each item In statusValues.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
                ComboBox1.Items.Add(stField + " - " + item.Replace("%SPC%", "Not Set"))
                status_where.Add("status" + n.ToString + " = '" + item.Replace("%SPC%", "Not Set").Replace("'", "''") + "'")
                anyStatusWhere += "status" + n.ToString + " != '' OR "
            Next
            n += 1
        Next
        If anyStatusWhere.Length > 4 Then
            anyStatusWhere += anyStatusWhere.Replace("status", "comment") 'This is needed to also include comments
            anyStatusWhere = "(" + anyStatusWhere.Substring(0, anyStatusWhere.Length - 4) + ")"
        End If
        status_where.Insert(0, anyStatusWhere)

        refreshing = True
        ComboBox1.SelectedIndex = 0
        AddHandler ComboBox1.SelectedIndexChanged, AddressOf filtersHandler

        n = 1
        For Each filter As String In statusFilters
            Dim realFieldName = "data" + filter.Substring(filter.IndexOf("_"))

            Dim r = db.queryReader("SELECT DISTINCT " + realFieldName + " FROM main ORDER BY " + realFieldName)

            Dim cmb As New ComboBox
            cmb.DropDownStyle = ComboBoxStyle.DropDownList
            cmb.Tag = realFieldName
            cmb.Items.Add("[All]")
            Do While r.Read
                cmb.Items.Add(r.GetValue(0).ToString)
            Loop
            AddHandler cmb.SelectedIndexChanged, AddressOf filtersHandler

            Me.Controls.Add(cmb)
            cmb.Top = ComboBox1.Top
            cmb.Left = ComboBox1.Left + ComboBox1.Width + (COMBOBOX_OFFSET * n) + (COMBOBOX_WIDTH * (n - 1))
            cmb.Width = COMBOBOX_WIDTH
            cmb.SelectedIndex = 0
            n = n + 1
        Next

        refreshing = False
        filtersHandler(ComboBox1, New EventArgs)

        Dim sync As String = ini.IniReadValue("StatusTable", "Sync")
        If sync = "1" Or sync.ToUpper = "TRUE" Then CheckBox1.Checked = True Else CheckBox1.Checked = False
    End Sub

    Private Sub filtersHandler(ByVal sender As Object, ByVal e As System.EventArgs)
        If refreshing Then Exit Sub

        Dim n As Integer = 1
        Dim fields As String = "main.id, name, "
        For Each stField In statusFields
            fields += "status" + n.ToString + " AS " + stField + ", "
            n += 1
        Next
        n = n - 1
        For i As Integer = 1 To n
            fields += "comment" + i.ToString + ", "
        Next
        fields = fields.Substring(0, fields.Length - 2)

        Dim where As String = "WHERE "
        If ComboBox1.SelectedIndex > 0 Then
            where += status_where(ComboBox1.SelectedIndex - 1) + " AND "
        End If

        For Each cmb As ComboBox In Me.Controls.OfType(Of ComboBox)()
            If cmb.Name.ToUpper = ("COMBOBOX1") Then Continue For
            If cmb.SelectedIndex > 0 Then
                If cmb.Tag.ToString.ToUpper.StartsWith("DATA_BOOL") Then
                    where += cmb.Tag + " LIKE '" + cmb.SelectedItem.ToString.Replace("'", "''") + "' AND "
                Else
                    where += cmb.Tag + " = '" + cmb.SelectedItem.ToString.Replace("'", "''") + "' AND "
                End If
            End If
        Next
        Dim query = sql.Replace("*", fields)
        If where.Length > 6 Then query = query + where.Substring(0, where.Length - 5)

        DataGridView1.DataSource = db.queryDataset(query)
        For i As Integer = DataGridView1.ColumnCount - n To DataGridView1.ColumnCount - 1
            DataGridView1.Columns(i).Visible = False
        Next
        DataGridView1.Columns(0).Visible = False
    End Sub

    Private Sub DataGridView1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DataGridView1.KeyDown
        If e.KeyCode = Keys.Space Then
            If DataGridView1.SelectedCells(0).ColumnIndex >= 2 Then
                If DataGridView1.SelectedCells.Count > 0 Then
                    Dim status_arr = statusValues.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
                    Dim cur = DataGridView1.SelectedCells(0).Value.ToString.Trim

                    If cur = "" Then
                        DataGridView1.SelectedCells(0).Value = status_arr(0)
                    Else
                        If Not status_arr.Contains(cur) Then
                            DataGridView1.SelectedCells(0).Value = ""
                        Else
                            Dim ind = Array.IndexOf(status_arr, cur) + 1
                            If ind > status_arr.Length - 1 Then
                                DataGridView1.SelectedCells(0).Value = ""
                            Else
                                DataGridView1.SelectedCells(0).Value = status_arr(ind)
                            End If
                        End If
                    End If

                    Dim c = DataGridView1.SelectedCells(0).ColumnIndex
                    Dim id = DataGridView1.SelectedCells(0).OwningRow.Cells(0).Value.ToString
                    cur = DataGridView1.SelectedCells(0).Value.ToString

                    Dim _sql As String = "SELECT id FROM status_data WHERE main_id = " + id
                    Dim rdr = db.queryReader(_sql)
                    If rdr.HasRows Then
                        _sql = "UPDATE status_data SET status" + (c - 1).ToString + " = '" + cur.Replace("'", "''") + "' "
                        _sql += "WHERE main_id = " + id
                        db.execute(_sql)
                    Else
                        _sql = "INSERT INTO status_data (main_id, status" + (c - 1).ToString + ") "
                        _sql += "VALUES (" + id + ", '" + cur.Replace("'", "''") + "') "
                        db.execute(_sql)
                    End If

                End If
            End If
        End If
    End Sub

    Private Sub DataGridView1_CellPainting(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles DataGridView1.CellPainting
        If e.RowIndex < 0 Then Exit Sub
        If e.ColumnIndex < 2 Then Exit Sub

        Dim g = e.Graphics
        Dim bounds = e.CellBounds
        e.PaintBackground(New Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height), True)
        e.PaintContent(New Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height))

        Dim v = DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex + statusFields.Count).Value
        If v IsNot Nothing AndAlso v.ToString <> "" Then
            Dim pts(2) As Point
            pts(0) = New Point(bounds.Right - 10, bounds.Top)
            pts(1) = New Point(bounds.Right, bounds.Top + 10)
            pts(2) = New Point(bounds.Right, bounds.Top)
            g.FillPolygon(Brushes.Green, pts)
            'g.FillRectangle(Brushes.Green, bounds.Right - 10, bounds.Top, 10, 10)
        End If
        e.Handled = True
    End Sub

    'Comment textbox
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        If refreshing Then Exit Sub
        If DataGridView1.SelectedCells.Count < 1 Then TextBox1.Text = "" : Exit Sub
        If DataGridView1.SelectedCells(0).ColumnIndex < 2 Then Exit Sub

        Dim r = DataGridView1.SelectedCells(0).RowIndex
        Dim c = DataGridView1.SelectedCells(0).ColumnIndex
        DataGridView1.Rows(r).Cells(c + statusFields.Count).Value = TextBox1.Text

        Dim id = DataGridView1.Rows(r).Cells(0).Value.ToString
        Dim _sql As String = "SELECT id FROM status_data WHERE main_id = " + id
        Dim rdr = db.queryReader(_sql)
        If rdr.HasRows Then
            _sql = "UPDATE status_data SET comment" + (c - 1).ToString + " = '" + TextBox1.Text.Replace("'", "''") + "' "
            _sql += "WHERE main_id = " + id
            db.execute(_sql)
        Else
            _sql = "INSERT INTO status_data (main_id, comment" + (c - 1).ToString + ") "
            _sql += "VALUES (" + id + ", '" + TextBox1.Text.Replace("'", "''") + "') "
            db.execute(_sql)
        End If
    End Sub

    Private Sub DataGridView1_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGridView1.SelectionChanged
        If DataGridView1.SelectedCells.Count < 1 Then
            refreshing = True
            TextBox1.Text = "" : TextBox1.ReadOnly = True
            refreshing = False
            Exit Sub
        End If
        If DataGridView1.SelectedCells(0).ColumnIndex < 2 Then
            Dim row = DataGridView1.SelectedCells(0).RowIndex

            refreshing = True
            TextBox1.Text = ""
            For n As Integer = 0 To statusFields.Count - 1
                Dim ind = statusFields.Count + n + 2
                TextBox1.Text += statusFields(n) + vbCrLf
                TextBox1.Text += DataGridView1.Rows(row).Cells(ind).Value + vbCrLf
                TextBox1.Text += "----------" + vbCrLf + vbCrLf
            Next
            TextBox1.ReadOnly = True
            refreshing = False
            Exit Sub
        End If

        TextBox1.ReadOnly = False
        Dim r = DataGridView1.SelectedCells(0).RowIndex
        Dim c = DataGridView1.SelectedCells(0).ColumnIndex
        refreshing = True
        TextBox1.Text = DataGridView1.Rows(r).Cells(c + statusFields.Count).Value.ToString
        refreshing = False
    End Sub

    'Sync with main window selected game
    Private Sub sync(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged, l.SelectedIndexChanged
        If TryCast(sender, CheckBox) Is CheckBox1 Then
            If CheckBox1.Checked Then Me.TopMost = True Else Me.TopMost = False
            ini.IniWriteValue("StatusTable", "Sync", CheckBox1.Checked.ToString)
        End If

        If Not CheckBox1.Checked Then Exit Sub
        If l.SelectedIndex < 0 Then Exit Sub

        Dim selected_id = DirectCast(l.SelectedItem, DataRowView).Row.Item(0).ToString
        Dim selected_game = DirectCast(l.SelectedItem, DataRowView).Row.Item(1).ToString

        Dim row = DataGridView1.Rows.Cast(Of DataGridViewRow).Where(Function(r) String.Compare(r.Cells(0).Value, selected_id) = 0).FirstOrDefault
        If row IsNot Nothing Then
            DataGridView1.ClearSelection()
            DataGridView1.Rows(row.Index).Selected = True
            DataGridView1.FirstDisplayedScrollingRowIndex = row.Index
        End If
    End Sub
End Class