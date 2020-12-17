Imports System.ComponentModel

Public Class FormC_ImportScreenshots
	Dim bg As New BackgroundWorker

	Private Sub FormC_ImportScreenshots_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		AddHandler bg.DoWork, AddressOf Button1_Click_BG
	End Sub

	Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
		CheckBox2.Enabled = CheckBox1.Enabled
		If Not CheckBox1.Enabled Then CheckBox2.Checked = False
	End Sub

	'Import
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		ProgressBar1.Value = 0
		bg.RunWorkerAsync()
	End Sub
	Private Sub Button1_Click_BG(sender As Object, e As EventArgs)
		Dim screenshotPath_new = TextBox1.Text.Trim
		If Not screenshotPath_new.EndsWith("\") Then screenshotPath_new += "\"
		If Not IO.Directory.Exists(screenshotPath_new) Then MsgBox("Directory does not exist.") : Exit Sub

		Dim f As Form1 = Me.Owner
		If CheckBox1.Checked Then
			Dim screenshotPath_orig = Form1.ini.IniReadValue("Paths", "Screenshots")
			If Not screenshotPath_orig.EndsWith("\") Then screenshotPath_orig += "\"
			If Not IO.Directory.Exists(screenshotPath_orig) Then Me.Invoke(Sub() MsgBox("The default screenshot path is incorrect.")) : Exit Sub

			Me.Invoke(Sub() Label2.Text = "Get product list...")
			Dim sql = "SELECT DISTINCT main.id, main.name, group_concat(category.cat) "
			If f.type = catalog_type.games Then sql += ", data_num1 " 'Year
			sql += "FROM main JOIN category ON main.id = category.id_main GROUP BY main.id, main.name"
			Dim dt = db.queryDataset(sql)

			Dim counter = 0
			Me.Invoke(Sub() ProgressBar1.Maximum = dt.Rows.Count)
			For Each r As DataRow In dt.Rows
				counter += 1
				If counter Mod 20 = 0 Then
					Me.Invoke(Sub() Label2.Text = "Checking " + counter.ToString + " / " + dt.Rows.Count.ToString + ": " + r.Item("name").ToString)
					Me.Invoke(Sub() ProgressBar1.Value = counter)
				End If

				Dim year As String = ""
				If f.type = catalog_type.games Then year = r(3).ToString

				If CheckBox2.Checked Then
					Dim scr As String = f.getScreen(r.Item("name").ToString, r(2).ToString, 0, year)
					If scr <> "" Then Continue For
				End If

				Dim scr_ind = 0
				Dim scr_new As String = ""
				Do
					scr_new = f.getScreen(r.Item("name").ToString, r(2).ToString, scr_ind, year, False, False, screenshotPath_new)

					If scr_new <> "" Then
						Dim target = f.getScreen(r.Item("name").ToString, r(2).ToString, scr_ind, year, False, True)
						target += IO.Path.GetExtension(scr_new)

						If IO.File.Exists(target) Then Exit Do
						If RadioButton1.Checked Then IO.File.Copy(scr_new, target) Else IO.File.Move(scr_new, target)
					End If

					scr_ind += 1
				Loop While scr_new <> ""
			Next
		Else
			Me.Invoke(Sub() MsgBox("Importing screenshots for no recognized products is NOT IMPLIMENTED"))
			Exit Sub
		End If

		Me.Invoke(Sub() Label2.Text = "Idle")
		Me.Invoke(Sub() ProgressBar1.Value = 0)
		Me.Invoke(Sub() MsgBox("Done"))
	End Sub
End Class