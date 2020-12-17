Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices

Public Class FormF_FailedTrimmedEntries

	Dim frm1 As Form1 = Nothing
	Public Sub New(ids As String())

		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.
		frm1 = Application.OpenForms.OfType(Of Form1)()(0)
		Me.Width = 1100 : Me.Height = 680
		Me.Left = frm1.Location.X + (frm1.Width / 2) - (Me.Width / 2)
		Me.Top = frm1.Location.Y + (frm1.Height / 2) - (Me.Height / 2)

		Dim sql = "SELECT main.id, main.name, group_concat(category.cat) "
		If frm1.type = catalog_type.games Then sql += ", data_num1 "
		sql += "FROM main JOIN category ON main.id = category.id_main "
		sql += "WHERE main.id IN (" + String.Join(",", ids) + ") "
		sql += "GROUP BY main.id, main.name ORDER BY main.name"

		Dim tbl = db.queryDataset(sql)
		ListBox1.BeginUpdate()
		ListBox1.ValueMember = "id"
		ListBox1.DisplayMember = "name"
		ListBox1.DataSource = tbl
		Label1.Text = "Count: " + ListBox1.Items.Count.ToString()
	End Sub
	Private Sub FormF_FailedTrimmedEntries_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
		MsgBox("Don't forget to launch trim again, after manage screenshots.")
	End Sub

	Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
		For n As Integer = FlowLayoutPanel1.Controls.Count - 1 To 0 Step -1
			FlowLayoutPanel1.Controls.RemoveAt(n)
		Next
		If ListBox1.SelectedItem Is Nothing Then Exit Sub

		Dim row = DirectCast(ListBox1.SelectedItem, DataRowView)
		Dim name = row(1).ToString()
		Dim name_new = name.Trim
		Dim cat = row(2).ToString()
		Dim year = ""
		If frm1.type = catalog_type.games Then year = row(3).ToString()

		Dim i As Integer = 0
		Dim screenshots As New List(Of String)
		While True
			Dim scr = frm1.getScreen(name, cat, i, year)
			If scr = "" Then Exit While
			screenshots.Add(scr) : i += 1
		End While

		i = 0
		Dim screenshots_new As New List(Of String)
		While True
			Dim scr = frm1.getScreen(name_new, cat, i, year)
			If scr = "" Then Exit While
			screenshots_new.Add(scr) : i += 1
		End While

		'FlowLayoutPanel1.SuspendLayout()
		Dim columns = {screenshots.Count, screenshots_new.Count}.Max()
		For col As Integer = 0 To columns - 1
			Dim pnl As New Panel With {.Width = 300, .Height = 520}
			Dim p1 As PictureBoxEx = Nothing
			Dim p2 As PictureBoxEx = Nothing
			Dim msg1 As String = ""
			Dim msg2 As String = ""
			If screenshots.Count > col Then
				p1 = New PictureBoxEx()
				p1.Location = New Point(0, 0)
				p1.Size = New Size(250, 250)
				p1.BorderStyle = BorderStyle.FixedSingle
				p1.SizeMode = PictureBoxSizeMode.Zoom
				'p1.Image = Image.FromFile(screenshots(col))
				Using bmp = New Bitmap(screenshots(col))
					p1.Image = New Bitmap(bmp)
				End Using
				pnl.Controls.Add(p1)
				If screenshots_new.Count > col AndAlso screenshots(col) = screenshots_new(col) Then p1.borderColor = Color.Red

				msg1 = Path.GetExtension(screenshots(col)) + " "
				msg1 += p1.Image.Width.ToString + "x" + p1.Image.Height.ToString() + " "
				msg1 += New IO.FileInfo(screenshots(col)).Length.ToString("### ### ### ###") + " bytes"
				Dim l As New Label With {.Text = msg1, .AutoSize = True}
				pnl.Controls.Add(l) : l.BringToFront()
				l.Location = New Point(1, p1.Top + p1.Height - l.Height - 1)
			End If
			If screenshots_new.Count > col Then
				p2 = New PictureBoxEx()
				p2.Location = New Point(0, 270)
				p2.Size = New Size(250, 250)
				p2.BorderStyle = BorderStyle.FixedSingle
				p2.SizeMode = PictureBoxSizeMode.Zoom
				'p2.Image = Image.FromFile(screenshots_new(col))
				Using bmp = New Bitmap(screenshots_new(col))
					p2.Image = New Bitmap(bmp)
				End Using
				pnl.Controls.Add(p2)
				If screenshots.Count > col AndAlso screenshots(col) = screenshots_new(col) Then p2.borderColor = Color.Red

				msg2 = Path.GetExtension(screenshots_new(col)) + " "
				msg2 += p2.Image.Width.ToString + "x" + p2.Image.Height.ToString() + " "
				msg2 += New IO.FileInfo(screenshots_new(col)).Length.ToString("### ### ### ###") + " bytes"
				Dim l As New Label With {.Text = msg2, .AutoSize = True}
				pnl.Controls.Add(l) : l.BringToFront()
				l.Location = New Point(1, p2.Top + p2.Height - l.Height - 1)
			End If

			If msg1 = msg2 Then
				If p1 IsNot Nothing AndAlso p1.borderColor <> Color.Red Then p1.borderColor = Color.Green
				If p2 IsNot Nothing AndAlso p2.borderColor <> Color.Red Then p2.borderColor = Color.Green
			End If
			FlowLayoutPanel1.Controls.Add(pnl)
		Next

		Label3.Text = "Screens: " + screenshots.Count.ToString + " \ " + screenshots_new.Count.ToString
		If screenshots.Count = screenshots_new.Count Then Label3.ForeColor = Color.Black Else Label3.ForeColor = Color.Red
		'FlowLayoutPanel1.ResumeLayout()
	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		If ListBox1.SelectedItem Is Nothing Then Exit Sub

		Dim row = DirectCast(ListBox1.SelectedItem, DataRowView)
		Dim name = row(1).ToString()
		Dim name_new = name.Trim
		Dim cat = row(2).ToString()
		Dim year = ""
		If frm1.type = catalog_type.games Then year = row(3).ToString()
		log("-----------------------------------------------------------", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
		log("Check product: '" + name + "', trimmed '" + name_new + "'", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)

		Dim i As Integer = 0
		Dim screenshots As New List(Of String)
		While True
			Dim scr = frm1.getScreen(name, cat, i, year)
			If scr = "" Then Exit While
			screenshots.Add(scr) : i += 1
		End While

		i = 0
		Dim screenshots_new As New List(Of String)
		While True
			Dim scr = frm1.getScreen(name_new, cat, i, year)
			If scr = "" Then Exit While
			screenshots_new.Add(scr) : i += 1
		End While

		For ind As Integer = 0 To screenshots.Count
			If screenshots_new.Count <= ind Then Exit For
			log("Check existing screen: '" + screenshots(ind) + "' against trimmed: '" + screenshots_new(ind) + "'", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
			If screenshots(ind) = screenshots_new(ind) Then
				log("Existing and trimmed screen are the same file: '" + screenshots(ind) + "' (was trimmed by getScreen() function).", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				Continue For
			End If

			Dim i1 = Image.FromFile(screenshots(ind))
			Dim i2 = Image.FromFile(screenshots_new(ind))
			Dim res1 = i1.Width * i1.Height
			Dim res2 = i2.Width * i2.Height
			Dim res1_str = i1.Width.ToString + "x" + i1.Height.ToString
			Dim res2_str = i2.Width.ToString + "x" + i2.Height.ToString
			i1.Dispose() : i1 = Nothing
			i2.Dispose() : i2 = Nothing
			If res1 > res2 Then
				log("Resolution of existing screen: " + res1_str + ", trimmed: " + res2_str + ". Deleteng trimmed screen.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Delete(screenshots_new(ind))
			ElseIf res1 < res2 Then
				log("Resolution of existing screen: " + res1_str + ", trimmed: " + res2_str + ". Deleteng existing screen and rename trimmed to match existing.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Delete(screenshots(ind))
				File.Move(screenshots_new(ind), screenshots(ind))
			End If
			If res1 <> res2 Then Continue For

			'If res is the same, check size
			Dim s1 = New IO.FileInfo(screenshots(ind)).Length
			Dim s2 = New IO.FileInfo(screenshots_new(ind)).Length
			If s1 > s2 Then
				log("Screens resolutions are equal (" + res1_str + "). Size of existing screen: " + s1.ToString + ", trimmed: " + s2.ToString + ". Deleteng trimmed screen.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Delete(screenshots_new(ind))
			ElseIf s1 < s2 Then
				log("Screens resolutions are equal (" + res1_str + "). Size of existing screen: " + s1.ToString + ", trimmed: " + s2.ToString + ". Deleteng existing screen and rename trimmed to match existing.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Delete(screenshots(ind))
				File.Move(screenshots_new(ind), screenshots(ind))
			End If
			If s1 <> s2 Then Continue For

			'If res and size are the same, just delete trimmed version
			If res1 = res2 AndAlso s1 = s2 Then
				log("Screens resolutions are equal (" + res1_str + "). Screens size (bytes) are equal (" + s1.ToString + "). Deleteng trimmed screen.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Delete(screenshots_new(ind))
			End If
		Next

		'If there are more trimmed screens then current - just move trimmed version to the current
		For ind As Integer = screenshots.Count To screenshots_new.Count - 1
			Dim scr = frm1.getScreen(name_new, cat, ind, year)
			Dim ext = Path.GetExtension(screenshots_new(ind))
			Dim file_path = scr + "." + ext
			If File.Exists(file_path) Then
				log("There are more trimmed screens than existing. Tried to rename overflowed trimmed '" + screenshots_new(ind) + "' to match existing: '" + file_path + "', but the file already exist.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
			Else
				log("There are more trimmed screens than existing. Renaming overflowed trimmed '" + screenshots_new(ind) + "' to match existing: '" + file_path + "'.", "Name Trimming -> Auto Screen Rename", LOG_FILE_PATH_FILES)
				File.Move(screenshots_new(ind), file_path)
			End If
		Next

		'Refresh screens
		ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
		ListBox1.Focus()
	End Sub
End Class

Class PictureBoxEx
	Inherits PictureBox

	<DllImport("User32.dll")> Public Shared Function GetWindowDC(ByVal hWnd As IntPtr) As IntPtr
	End Function

	Const WM_NCPAINT As Integer = &H85
	Public borderColor As Color = Color.Black
	Protected Overrides Sub WndProc(ByRef m As Message)
		MyBase.WndProc(m)
		If m.Msg = WM_NCPAINT AndAlso MyBase.BorderStyle = BorderStyle.FixedSingle Then
			Dim dc = GetWindowDC(Handle)
			Using g = Graphics.FromHdc(dc)
				g.DrawRectangle(New Pen(borderColor), 0, 0, Width - 1, Height - 1)
			End Using
		End If
	End Sub
End Class
Class FlowLayoutPanelEx
	Inherits FlowLayoutPanel
	Public Sub New()
		'Me.DoubleBuffered = True
		Me.SetStyle(ControlStyles.UserPaint, True)
		Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
		Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
	End Sub

	Protected Overrides Sub OnScroll(se As ScrollEventArgs)
		Me.Invalidate()
		MyBase.OnScroll(se)
	End Sub

	Protected Overrides ReadOnly Property CreateParams As CreateParams
		Get
			Dim cp = MyBase.CreateParams
			cp.ExStyle = cp.ExStyle Or &H2000000 'WS_CLIPCHILDREN
			Return cp
		End Get
	End Property
End Class
