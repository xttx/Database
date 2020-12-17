Public Class Form6_Scanner_Details2
	Dim refr As Boolean = False

	Private Sub Form6_Scanner_Details2_Load(sender As Object, e As EventArgs) Handles Me.Load
		'Dim sql = "SELECT * FROM unrecognized_files"
		'Update_Grid(db.queryDataset(sql))
	End Sub

	Private Sub Form6_Scanner_Details2_Shown(sender As Object, e As EventArgs) Handles Me.Shown
		Button1_Click(Button1, New EventArgs)
	End Sub
	Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged
		If DirectCast(sender, RadioButton).Checked Then Button1_Click(Button1, New EventArgs)
	End Sub
	Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged, CheckBox2.CheckedChanged
		If refr Then Exit Sub

		refr = True
		Dim chk = DirectCast(sender, CheckBox)
		If chk.Checked Then
			If chk Is CheckBox1 Then CheckBox2.Checked = False Else CheckBox1.Checked = False
		End If
		refr = False

		Button1_Click(Button1, New EventArgs)
	End Sub

	'Filter button
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim table = ""
		Dim columns = ""
		If RadioButton1.Checked Then
			table = "unrecognized_files"
			columns = "id, path, file, t"
		ElseIf RadioButton2.Checked Then
			table = "product_files_found_doubles"
			columns = "id_main, path, file, t"
		End If

		Dim where = ""
		If TextBox1.Text.Trim <> "" Then where = " file LIKE '" + TextBox1.Text.Trim.Replace("'", "''") + "'"
		If ComboBox1.SelectedIndex > 0 Then
			If where <> "" Then where += " AND"
			If ComboBox1.SelectedItem.ToString() = "=" Then
				where += " t LIKE '" + DateTimePicker1.Value.ToString("yyyy-MM-dd") + "%'"
			Else
				where += " t " + ComboBox1.SelectedItem.ToString() + " '" + DateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'"
			End If
		End If
		If ComboBox2.SelectedIndex > 0 Then
			If where <> "" Then where += " AND"
			If ComboBox2.SelectedItem.ToString() = "=" Then
				where += " t LIKE '" + DateTimePicker2.Value.ToString("yyyy-MM-dd") + "%'"
			Else
				where += " t " + ComboBox2.SelectedItem.ToString() + " '" + DateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'"
			End If
		End If
		If CheckBox2.Checked Then
			If where <> "" Then where += " AND"
			where += " UPPER(file) IN (SELECT UPPER(file) FROM " + table + " GROUP BY file HAVING count(file) > 1 )"
		End If
		If where <> "" Then where = " WHERE" + where

		Dim sql = "SELECT " + columns + " FROM " + table + where

		If CheckBox1.Checked Then sql += " GROUP BY file"
		Update_Grid(db.queryDataset(sql))
	End Sub

	Private Sub Update_Grid(ds As DataTable)
		DataGridView1.DataSource = ds
		If DataGridView1.Columns.Count = 0 Then Exit Sub 'Because it will not work on initialization, when handle is not initialized yet
		DataGridView1.Columns(0).Width = 70
		DataGridView1.Columns(1).Width = 400
		DataGridView1.Columns(2).Width = 300
		DataGridView1.Columns(3).Width = 115
		DataGridView1.Columns(3).DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss"
		'If RadioButton1.Checked Then
		'	DataGridView1.Columns(0).Width = 70
		'	DataGridView1.Columns(1).Width = 400
		'	DataGridView1.Columns(2).Width = 300
		'	DataGridView1.Columns(3).Width = 115
		'	DataGridView1.Columns(3).DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss"
		'ElseIf RadioButton2.Checked Then
		'	DataGridView1.Columns(0).Width = 70
		'	DataGridView1.Columns(1).Width = 70
		'	DataGridView1.Columns(2).Width = 600
		'	DataGridView1.Columns(3).Width = 115
		'	DataGridView1.Columns(3).DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss"
		'End If
		Label4.Text = "Rows: " + DataGridView1.Rows.Count.ToString()
	End Sub
End Class