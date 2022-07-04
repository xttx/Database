Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Catalog_2016.TinyJson
Imports Microsoft.VisualBasic.Strings
Imports SevenZip

'TODO: More intuitive way to set input file format (separated fields / .ini), need to use radio buttons on top groupbox
'TODO: Handle error in import thread, because if it freeze, we don't even know why
'TODO: This asset is in database, why it want to add it as new when importing? "GOAP NPC: Goal-Oriented Action Planning for Non-Player Characters"
'TODO: filter input rows by field numeric values (< > = <= >=)
'TODO: filter input rows by string NOT match regex (inverse regex)
'TODO: config should be stored in utf, because "₽" symbol can not be loaded from preset - setting utf char in ini works, if .ini in in UTF-16LE encoding
'TODO: handle mysql errors
'TODO: In import, value(0) is shown in the log as product name, but it's not always the name, it can be any field
'TODO: Add/Update for multiple matches (both old (check) and new (primary/secondary match) realisation)
'TODO: Agressive numeric/decimal parsing, using string.join("", str.where(c=>c.IsDigit)) or regex

'DONE: Fixed column size
'DONE: Fixed column size - preprocess replacement of tab by 8 (configurable) spaces
'DONE: Constant column
'DONE: Only parse row matchin regex
'DONE: filter log (show only add, only update, only success, only skip)

Public Class FormB_ImportContent
	Dim fieldsNames_Display As New List(Of String)
	Dim fieldsNames_DB As New List(Of String)

	Dim Groupboxes_Enable_States As Boolean() = {False, False}

	Dim bg_worker As New BackgroundWorker

	Dim log As New List(Of String)

	'<Serializable>
	Class FileInfo
		Public type As Integer = 0
		Public file As String = ""
		Public preview As New List(Of String)
		Public field_association As String() = Nothing
		Public separator As String = "%%@%%"
		Public fixed_width As Integer = 8
		Public use_separator As Boolean = True
		Public field_splits As New List(Of Integer)
		Public field_options As New Dictionary(Of Integer, field_opt)
		Public headers As New List(Of ini_headers)
		Public match_primary_multiple As Integer = 0
		Public match_secondary_zero As Integer = 0
		Public match_secondary_multiple As Integer = 0

		'<Serializable>
		Class field_opt
			Public sub_separator As String = ""
			Public assoc As String() = {}
			Public replace As String() = {}
			Public replace_with As String() = {}
			Public parse_date As Boolean = False
			Public parse_date_format As String = "yyyy.MM.dd"
			Public set_to_constant As Boolean = False
			Public set_to_constant_data As String = ""
			Public require_regex As String = ""
			Public match_mode As Integer = 0
			Public match_replace As String() = {"", "", ""}
			Public match_replace_with As String() = {"", "", ""}
			Public match_replace_regex = ""
			Public match_replace_regex_with = ""
			Public match_query_mode As Integer = 1
		End Class

		'<Serializable>
		Class ini_headers
			Public header As String = ""
			Public db_field As String = ""
			Public header_is_db_field As Boolean = False
			Public fields As New List(Of header_fields)

			'<Serializable>
			Class header_fields
				Public type As Integer = 0
				Public name As String = ""
				'Public index As Integer = 0
				Public db_field As String = ""
				Public Overrides Function ToString() As String
					If type = 0 Then
						Return name + " -> " + db_field
					ElseIf type = 1 Then
						'Return "#" + Index.ToString() + " -> " + db_field
						Return "Get Line -> " + db_field
					Else
						Return "All The Rest -> " + db_field
					End If
				End Function
			End Class
			Public Overrides Function ToString() As String
				If header_is_db_field Then
					Return "[%" + db_field + "%]"
				Else
					Return "[" + header + "]"
				End If
			End Function
		End Class
		Public Sub New(f As String)
			file = f
		End Sub

		Public Overrides Function ToString() As String
			Return IO.Path.GetFileName(file)
		End Function
	End Class

	'Form Load - Initialization
	Private Sub FormB_ImportContent_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		AddHandler bg_worker.DoWork, AddressOf Button5_Click_BG
		DataGridView1.GetType.InvokeMember("DoubleBuffered", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.SetProperty, Nothing, DataGridView1, New Object() {True})

		'Setup Addition Match Otions Panels (global and per field)
		GroupBox3.Parent.Controls.Remove(GroupBox3)
		GroupBox4.Parent.Controls.Remove(GroupBox4)
		Me.Controls.Add(GroupBox3) : GroupBox3.BringToFront()
		Me.Controls.Add(GroupBox4) : GroupBox4.BringToFront()
		GroupBox3.Location = New Point(CInt(Math.Round((Me.Width / 2) - (GroupBox3.Width / 2))), CInt(Math.Round((Me.Height / 2) - (GroupBox3.Height / 2))))
		GroupBox4.Location = New Point(CInt(Math.Round((Me.Width / 2) - (GroupBox4.Width / 2))), CInt(Math.Round((Me.Height / 2) - (GroupBox4.Height / 2))))

		fieldsNames_Display.Add("name") : fieldsNames_DB.Add("name")
		fieldsNames_Display.Add("category") : fieldsNames_DB.Add("category")
		For Each f In Fields_ordered.Where(Function(x) x.enabled)
			fieldsNames_Display.Add(f.name)
			fieldsNames_DB.Add(f.DBname)
		Next

		CheckedListBox1.Size = New Size(160, 200)
		CheckedListBox1.Parent = Me
		CheckedListBox1.BringToFront()
		AddHandler TextBox2.Click, Sub()
									   CheckedListBox1.Visible = True : CheckedListBox1.Select()
								   End Sub
		'ComboBox3.Items.Add("- - Use Full Header Content - -")
		ComboBox9.Items.Add("Not Used") : ComboBox10.Items.Add("Not Used") : ComboBox11.Items.Add("Not Used") : ComboBox12.Items.Add("Not Used") : ComboBox13.Items.Add("Not Used")
		For Each fName As String In fieldsNames_Display
			CheckedListBox1.Items.Add(fName)
			ComboBox1.Items.Add(fName) : ComboBox3.Items.Add(fName)
			'Sub-split comboboxes
			ComboBox9.Items.Add(fName) : ComboBox10.Items.Add(fName) : ComboBox11.Items.Add(fName) : ComboBox12.Items.Add(fName) : ComboBox13.Items.Add(fName)
		Next
		If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0
		If ComboBox3.Items.Count > 0 Then ComboBox3.SelectedIndex = 0
		ComboBox9.SelectedIndex = 0 : ComboBox10.SelectedIndex = 0 : ComboBox11.SelectedIndex = 0 : ComboBox12.SelectedIndex = 0 : ComboBox13.SelectedIndex = 0

		DataGridView1.EditMode = DataGridViewEditMode.EditOnEnter
		DataGridView1.Rows.Add(1)
		For col As Integer = 0 To DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.None) - 1
			Dim cc = New DataGridViewComboBoxCell()
			DataGridView1.Rows(0).Cells(col) = cc
			cc.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton

			cc.Items.Add("---skip---")
			For Each fName As String In fieldsNames_Display
				cc.Items.Add(fName)
			Next
			cc.Items.Add("---SUB SPLIT---")
			cc.Value = "---skip---"
		Next

		'Main Mode
		ComboBox5.SelectedIndex = 0
		'Category Mode
		ComboBox7.SelectedIndex = 1
		'Log level
		ComboBox6.SelectedIndex = 0

		'Presets
		For Each p In Form1.ini.IniListKey("Presets_ImportAdv")
			ComboBox2.Items.Add(p)
		Next
	End Sub
	Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub
		If TabControl1.SelectedIndex = 3 Then Exit Sub

		Dim fi As FileInfo = ListBox1.SelectedItem
		fi.type = TabControl1.SelectedIndex
		If fi.type = 2 Then fi.type = 0
	End Sub
	Private Sub Unique_Field_Selector_Hide() Handles Me.Click, CheckedListBox1.Leave, GroupBox1.Click, GroupBox2.Click,
		TabControl1.Click, TabPage1.Click, TabPage2.Click, TabPage3.Click

		CheckedListBox1.Visible = False
		TextBox2.Text = String.Join(", ", CheckedListBox1.CheckedItems.Cast(Of String))
	End Sub
	'Changed separator / or separator parameters
	Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged, RadioButton4.CheckedChanged, NumericUpDown4.ValueChanged
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		fi.separator = TextBox1.Text '.Trim
		fi.use_separator = RadioButton4.Checked
		fi.fixed_width = NumericUpDown4.Value

		Dim need_refresh = False
		If sender Is RadioButton4 Then need_refresh = True
		If RadioButton4.Checked AndAlso sender Is TextBox1 Then need_refresh = True
		If RadioButton5.Checked AndAlso sender Is NumericUpDown4 Then need_refresh = True

		If need_refresh Then Refresh_Preview()
	End Sub
	'Changed field association in datagrid
	Private Sub DataGridView1_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellValueChanged
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If e.RowIndex = 0 Then
			'Update field name in field list
			Dim str = "Column" + e.ColumnIndex.ToString
			Dim val = DataGridView1.Rows(0).Cells(e.ColumnIndex).Value.ToString
			If val.ToLower() <> "---skip---" Then str += " - " + val
			ListBox4.Items(e.ColumnIndex) = str

			If Not GroupBox2.Enabled Then Exit Sub

			Dim fi As FileInfo = ListBox1.SelectedItem
			Dim edited_cell As DataGridViewComboBoxCell = DataGridView1.Rows(0).Cells(e.ColumnIndex)
			'If edited_cell.Value = edited_cell.Items(edited_cell.Items.Count - 1) Then
			'	'Show SUB-SPLIT options
			'	Dim cmb_arr = {ComboBox4, ComboBox5, ComboBox6, ComboBox7, ComboBox8}
			'	If fi.sub_split IsNot Nothing AndAlso fi.sub_split.ContainsKey(e.ColumnIndex) Then
			'		TextBox5.Text = fi.sub_split(e.ColumnIndex).sub_separator
			'		TextBox6.Text = fi.sub_split(e.ColumnIndex).replace
			'		TextBox7.Text = fi.sub_split(e.ColumnIndex).replace_with

			'		For i As Integer = 0 To cmb_arr.Count - 1
			'			If fi.sub_split(e.ColumnIndex).assoc(i).Trim <> "" Then
			'				Dim ind = fieldsNames_DB.IndexOf(fi.sub_split(e.ColumnIndex).assoc(i).Trim)
			'				If ind >= 0 Then cmb_arr(i).SelectedIndex = ind + 1 Else cmb_arr(i).SelectedIndex = 0
			'			End If
			'		Next
			'	Else
			'		TextBox5.Text = "" : TextBox6.Text = "" : TextBox7.Text = ""
			'		For i As Integer = 0 To cmb_arr.Count - 1
			'			cmb_arr(i).SelectedIndex = 0
			'		Next
			'	End If
			'	GroupBox3.Tag = e.ColumnIndex : GroupBox3.Visible = True
			'End If

			'TODO - check if the same DB field is associated multiple times
			Dim l As New List(Of String)
			For i As Integer = 0 To DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.None) - 1
				Dim cc As DataGridViewComboBoxCell = DataGridView1.Rows(0).Cells(i)
				Dim item_ind = cc.Items.IndexOf(cc.Value)
				If item_ind < 1 Then l.Add("") : Continue For
				If item_ind = fieldsNames_DB.Count + 1 Then l.Add("SUB-SPLIT") : Continue For 'SUB-SPLIT

				l.Add(fieldsNames_DB(item_ind - 1))

				'Dim str = "Column" + i.ToString
				'If item_ind >= 1 Then str += " - " + cc.Value
				'ListBox4.Items(i) = str
			Next

			fi.field_association = l.ToArray()
		End If
	End Sub
	Private Sub DataGridView1_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles DataGridView1.CurrentCellDirtyStateChanged
		'To catch build-in datagrid combobox selection change event we use CellValueChanged event
		'but it is raised only after we are leaving the cell
		'this call CommitEdit right after cell is marked dirty - combobox selection changed
		If DataGridView1.IsCurrentCellDirty Then DataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit)
	End Sub
	'Change Field Option - Select Field
	Private Sub ListBox4_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox4.SelectedIndexChanged
		'Reset
		ListBox4.Tag = "UPDATE_OPTIONS"
		Dim cmb_arr = {ComboBox9, ComboBox10, ComboBox11, ComboBox12, ComboBox13}
		For Each cmb In cmb_arr
			cmb.SelectedIndex = 0
		Next
		TextBox8.Text = ""
		TextBox9.Text = "" : TextBox10.Text = ""
		TextBox11.Text = "" : TextBox12.Text = ""
		TextBox13.Text = "" : TextBox14.Text = ""
		CheckBox2.Checked = False
		TextBox6.Text = "yyyy.MM.dd"
		CheckBox3.Checked = False
		TextBox15.Text = ""
		TextBox25.Text = ""
		RadioButton6.Checked = True
		ListBox4.Tag = ""

		'Field match options reset
		TextBox17.Text = "" : TextBox19.Text = "" : TextBox21.Text = "" : TextBox23.Text = ""
		TextBox18.Text = "" : TextBox20.Text = "" : TextBox22.Text = "" : TextBox24.Text = ""
		RadioButton16.Checked = True
		Button18.BackColor = Color.Transparent


		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If ListBox4.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		'Fill
		ListBox4.Tag = "UPDATE_OPTIONS"
		Dim fieldN = ListBox4.SelectedIndex
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		If fi.field_options IsNot Nothing AndAlso fi.field_options.ContainsKey(fieldN) Then
			TextBox8.Text = fi.field_options(fieldN).sub_separator
			For i As Integer = 0 To cmb_arr.Count - 1
				If fi.field_options(fieldN).assoc(i).Trim <> "" Then
					Dim ind = fieldsNames_DB.IndexOf(fi.field_options(fieldN).assoc(i).Trim)
					If ind >= 0 Then cmb_arr(i).SelectedIndex = ind + 1 Else cmb_arr(i).SelectedIndex = 0
				End If
			Next

			Dim txt_rep_arr = {{TextBox9, TextBox10}, {TextBox11, TextBox12}, {TextBox13, TextBox14}}
			For i As Integer = 0 To fi.field_options(fieldN).replace.Count - 1
				txt_rep_arr(i, 0).Text = fi.field_options(fieldN).replace(i)
				txt_rep_arr(i, 1).Text = fi.field_options(fieldN).replace_with(i)
			Next

			CheckBox2.Checked = fi.field_options(fieldN).parse_date
			TextBox6.Text = fi.field_options(fieldN).parse_date_format
			CheckBox3.Checked = fi.field_options(fieldN).set_to_constant
			TextBox15.Text = fi.field_options(fieldN).set_to_constant_data
			TextBox25.Text = fi.field_options(fieldN).require_regex
			If fi.field_options(fieldN).match_mode = 1 Then RadioButton7.Checked = True
			If fi.field_options(fieldN).match_mode = 2 Then RadioButton8.Checked = True

			Dim res = String.Join("", fi.field_options(fieldN).match_replace) + String.Join("", fi.field_options(fieldN).match_replace_with)
			res += fi.field_options(fieldN).match_replace_regex + fi.field_options(fieldN).match_replace_regex_with
			If res <> "" Or fi.field_options(fieldN).match_query_mode <> 1 Then Button18.BackColor = Color.GreenYellow
		End If
		ListBox4.Tag = ""
	End Sub
	'Change Field Option
	Private Sub OnChangeFieldOptions(sender As Object, e As EventArgs) Handles TextBox8.TextChanged, ComboBox9.SelectedIndexChanged, ComboBox10.SelectedIndexChanged,
		ComboBox11.SelectedIndexChanged, ComboBox12.SelectedIndexChanged, ComboBox13.SelectedIndexChanged,
		TextBox9.TextChanged, TextBox10.TextChanged, TextBox11.TextChanged, TextBox12.TextChanged, TextBox13.TextChanged, TextBox14.TextChanged,
		CheckBox2.CheckedChanged, TextBox6.TextChanged, CheckBox3.CheckedChanged, TextBox15.TextChanged, TextBox25.TextChanged,
		RadioButton6.CheckedChanged, RadioButton7.CheckedChanged, RadioButton8.CheckedChanged

		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If ListBox4.SelectedIndex < 0 Then Exit Sub
		If ListBox4.Tag <> "" Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		If sender Is RadioButton6 And RadioButton6.Checked = False Then Exit Sub
		If sender Is RadioButton7 And RadioButton7.Checked = False Then Exit Sub
		If sender Is RadioButton8 And RadioButton8.Checked = False Then Exit Sub

		Dim cmb_arr = {ComboBox9, ComboBox10, ComboBox11, ComboBox12, ComboBox13}
		Dim field_arr() As String = {"", "", "", "", ""}
		For i As Integer = 0 To cmb_arr.Count - 1
			Dim field_D = cmb_arr(i).SelectedItem.ToString()
			If field_D.Trim = "" Then Continue For
			Dim ind = fieldsNames_Display.IndexOf(field_D)
			If ind < 0 Then Continue For
			field_arr(i) = fieldsNames_DB(ind)
		Next

		Dim fieldN = ListBox4.SelectedIndex
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)

		If fi.field_options Is Nothing Then fi.field_options = New Dictionary(Of Integer, FileInfo.field_opt)
		If fi.field_options.ContainsKey(fieldN) Then fi.field_options.Remove(fieldN)

		Dim sso = New FileInfo.field_opt With {.sub_separator = TextBox8.Text.Trim, .assoc = field_arr}
		sso.replace = {TextBox9.Text.Trim, TextBox11.Text.Trim, TextBox13.Text.Trim}
		sso.replace_with = {TextBox10.Text, TextBox12.Text, TextBox14.Text}
		sso.parse_date = CheckBox2.Checked : sso.parse_date_format = TextBox6.Text
		sso.set_to_constant = CheckBox3.Checked : sso.set_to_constant_data = TextBox15.Text
		sso.require_regex = TextBox25.Text.Trim

		sso.match_mode = 0
		If RadioButton7.Checked Then sso.match_mode = 1
		If RadioButton8.Checked Then sso.match_mode = 2

		fi.field_options.Add(fieldN, sso)
	End Sub


	'INI Format - add header
	Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		If Not CheckBox1.Checked AndAlso TextBox3.Text.Trim = "" Then MsgBox("Can not have header, which is not a field content without valid header string.") : Exit Sub

		Dim h = New FileInfo.ini_headers
		h.header = TextBox3.Text.Trim
		h.db_field = ComboBox1.Text
		h.header_is_db_field = CheckBox1.Checked
		ListBox2.Items.Add(h)

		'Reset
		TextBox3.Text = ""
		CheckBox1.Checked = False
		ComboBox1.SelectedIndex = 0

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		fi.headers.Add(h)
	End Sub
	'INI Format - add field
	Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If ListBox2.SelectedIndex < 0 Then MsgBox("Select a header, to add field to.") : Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub
		If RadioButton1.Checked AndAlso TextBox4.Text.Trim = "" Then MsgBox("To use named field you need to specify its name.") : Exit Sub

		Dim h = DirectCast(ListBox2.SelectedItem, FileInfo.ini_headers)
		Dim f = New FileInfo.ini_headers.header_fields

		If RadioButton1.Checked Then f.type = 0
		If RadioButton2.Checked Then f.type = 1
		If RadioButton3.Checked Then f.type = 2
		f.name = TextBox4.Text.Trim
		f.db_field = ComboBox3.Text
		ListBox3.Items.Add(f)

		'Reset
		RadioButton1.Checked = True
		TextBox4.Text = ""
		ComboBox3.SelectedIndex = 0

		h.fields.Add(f)
	End Sub
	'INI Format - header select
	Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox2.SelectedIndexChanged
		ListBox3.Items.Clear()
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If ListBox2.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		Dim h = DirectCast(ListBox2.SelectedItem, FileInfo.ini_headers)
		For Each f In h.fields
			ListBox3.Items.Add(f)
		Next
	End Sub
	'INI Format - Header is db field content checkbox
	Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
		ComboBox1.Enabled = CheckBox1.Checked
	End Sub
	'INI Format - Remove field
	Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If ListBox2.SelectedIndex < 0 Then Exit Sub
		If ListBox3.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		Dim h = DirectCast(ListBox2.SelectedItem, FileInfo.ini_headers)
		Dim f As FileInfo.ini_headers.header_fields = ListBox3.SelectedItem
		h.fields.Remove(f)
		ListBox3.Items.Remove(f)
	End Sub

	'Preset - Load
	Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
		If ComboBox2.Text.Trim = "" Then Exit Sub
		Dim option_str = Form1.ini.IniReadValue("Presets_ImportAdv", ComboBox2.Text.Trim)
		Dim options = option_str.Split({"---"}, StringSplitOptions.None)
		If options.Count < 2 Then Exit Sub

		Dim u_arr = options(0).ToUpper().Split({";"}, StringSplitOptions.RemoveEmptyEntries)
		For i As Integer = 0 To CheckedListBox1.Items.Count - 1
			If u_arr.Contains(CheckedListBox1.Items(i).ToString().ToUpper()) Then
				CheckedListBox1.SetItemChecked(i, True)
			Else
				CheckedListBox1.SetItemChecked(i, False)
			End If
		Next
		Unique_Field_Selector_Hide()

		Dim o = 1
		For i As Integer = 0 To ListBox1.Items.Count - 1
			Dim fi As FileInfo = ListBox1.Items(i)
			Dim opts = options(o).Split({"|"}, StringSplitOptions.None) 'Get a FileInfo Options

			fi.field_options.Clear()
			fi.headers.Clear()
			fi.type = CInt(opts(0).Trim)
			fi.separator = opts(1)
			If opts.Count > 5 Then fi.use_separator = Boolean.Parse(opts(5))
			If opts.Count > 6 Then fi.fixed_width = Integer.Parse(opts(6))
			If opts.Count > 7 Then fi.field_splits = opts(7).Split({"@"}, StringSplitOptions.RemoveEmptyEntries).Select(Of Integer)(Function(x) Integer.Parse(x)).ToList
			If opts.Count > 8 Then fi.match_primary_multiple = Integer.Parse(opts(8))
			If opts.Count > 9 Then fi.match_secondary_zero = Integer.Parse(opts(9))
			If opts.Count > 10 Then fi.match_secondary_multiple = Integer.Parse(opts(10))
			If opts.Count > 11 Then ComboBox5.SelectedIndex = Integer.Parse(opts(11)) Else ComboBox5.SelectedIndex = 0
			If opts.Count > 12 Then ComboBox7.SelectedIndex = Integer.Parse(opts(12)) Else ComboBox7.SelectedIndex = 1
			If opts(2).Trim <> "" Then fi.field_association = opts(2).Split({";"}, StringSplitOptions.None) 'Get FileInfo -> Fields Associations

			Dim sub_split = opts(3).Split({";"}, StringSplitOptions.None) 'Get FileInfo -> Fields Options
			For c As Integer = 0 To sub_split.Count - 1
				If sub_split(c).Trim <> "" Then
					fi.field_options.Add(c, New FileInfo.field_opt)
					Dim sub_split_param = sub_split(c).Split({"@"}, StringSplitOptions.None)
					fi.field_options(c).sub_separator = sub_split_param(0)
					fi.field_options(c).replace = sub_split_param(1).Split({"%"}, StringSplitOptions.None)
					fi.field_options(c).replace_with = sub_split_param(2).Split({"%"}, StringSplitOptions.None)
					fi.field_options(c).assoc = sub_split_param(3).Split({"%"}, StringSplitOptions.None)
					If sub_split_param.Count > 4 Then fi.field_options(c).parse_date = Boolean.Parse(sub_split_param(4))
					If sub_split_param.Count > 5 Then fi.field_options(c).parse_date_format = sub_split_param(5)
					If sub_split_param.Count > 6 Then fi.field_options(c).set_to_constant = Boolean.Parse(sub_split_param(6))
					If sub_split_param.Count > 7 Then fi.field_options(c).set_to_constant_data = sub_split_param(7)
					If sub_split_param.Count > 8 Then fi.field_options(c).require_regex = sub_split_param(8)
					If sub_split_param.Count > 9 Then fi.field_options(c).match_mode = Integer.Parse(sub_split_param(9))
					If sub_split_param.Count > 10 Then fi.field_options(c).match_replace = sub_split_param(10).Split({"%"}, StringSplitOptions.None)
					If sub_split_param.Count > 11 Then fi.field_options(c).match_replace_with = sub_split_param(11).Split({"%"}, StringSplitOptions.None)
					If sub_split_param.Count > 12 Then fi.field_options(c).match_replace_regex = sub_split_param(12)
					If sub_split_param.Count > 13 Then fi.field_options(c).match_replace_regex_with = sub_split_param(13)
					If sub_split_param.Count > 14 Then fi.field_options(c).match_query_mode = Integer.Parse(sub_split_param(14))
				End If
			Next

			Dim headers = opts(4).Split({";"}, StringSplitOptions.None)
			For h As Integer = 0 To headers.Count - 1
				If headers(h).Trim = "" Then Continue For

				Dim header_ref = New FileInfo.ini_headers
				Dim header_param = headers(h).Split({"@"}, StringSplitOptions.None)
				header_ref.db_field = header_param(0)
				header_ref.header = header_param(1)
				If header_param(2).ToUpper = "TRUE" Then header_ref.header_is_db_field = True

				Dim fields = header_param(3).Split({"*"}, StringSplitOptions.None)
				For f As Integer = 0 To fields.Count - 1
					Dim field_ref = New FileInfo.ini_headers.header_fields
					Dim field_param = fields(f).Split({"%"}, StringSplitOptions.None)

					field_ref.type = CInt(field_param(0))
					field_ref.db_field = field_param(1)
					field_ref.name = field_param(2)

					header_ref.fields.Add(field_ref)
				Next

				fi.headers.Add(header_ref)
			Next

			o += 1
			If o >= options.Count Then o = 1
		Next

		ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)



		''TEST Serialization - it's working, but end size is 11kb, or 4.5 kb, if we erase preview list
		'Dim fi_arr As New List(Of FileInfo)
		'For i As Integer = 0 To ListBox1.Items.Count - 1
		'	Dim fi1 As FileInfo = ListBox1.Items(i)
		'	fi1.preview = New List(Of String)
		'	fi_arr.Add(fi1)
		'Next

		'948 bytes, 417 bytes in compact mode, 248 after compression, 332 in base64
		'Dim str = fi_arr.ToJson(True)

		''Compress - JSON
		'Dim stream_zip = New MemoryStream()
		'Dim stream_zip_input As New MemoryStream(System.Text.Encoding.UTF8.GetBytes(str))
		'Dim zip As New Compression.GZipStream(stream_zip, Compression.CompressionLevel.Optimal)
		'stream_zip_input.CopyTo(zip)
		'zip.Close()
		'Dim bytes_zip = stream_zip.ToArray()
		'stream_zip.Close() : stream_zip_input.Close()
		'Dim save_str_zip = Convert.ToBase64String(bytes_zip)

		''From Json
		'Dim fi_arr2 = str.FromJson(Of List(Of FileInfo))(True)
		'fi_arr2 = fi_arr2


		''Serialize
		'Dim b = New BinaryFormatter
		'Dim stream = New MemoryStream()
		'b.Serialize(stream, fi_arr)
		'Dim bytes = stream.ToArray()
		'stream.Close()
		'Dim save_str = Convert.ToBase64String(bytes)

		''Compress - Still to much 1.1k
		'Dim stream_zip = New MemoryStream()
		'Dim stream_zip_input As New MemoryStream(bytes)
		'Dim zip As New Compression.GZipStream(stream_zip, Compression.CompressionLevel.Optimal)
		'stream_zip_input.CopyTo(zip)
		'zip.Close()
		'Dim bytes_zip = stream_zip.ToArray()
		'stream_zip.Close() : stream_zip_input.Close()
		'Dim save_str_zip = Convert.ToBase64String(bytes_zip)

		'Dim unzip_bytes(bytes.Length - 1) As Byte
		'Dim stream_unzip = New MemoryStream(bytes_zip)
		'Dim unzip As New Compression.GZipStream(stream_unzip, Compression.CompressionMode.Decompress)
		'unzip.Read(unzip_bytes, 0, bytes.Length)


		''Deserialize
		'Dim bytes2 = Convert.FromBase64String(save_str)
		'b = New BinaryFormatter
		'stream = New MemoryStream(bytes2)
		'Dim fi_arr2 As List(Of FileInfo) = b.Deserialize(stream)
		'fi_arr2 = fi_arr2
	End Sub
	'Preset - Save
	Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
		If ListBox1.Items.Count = 0 Then MsgBox("Nothing to save") : Exit Sub
		If ComboBox2.Text.Trim = "" Then MsgBox("Enter preset name") : Exit Sub

		Dim option_list As New List(Of String)
		option_list.Add(String.Join(";", CheckedListBox1.CheckedItems.Cast(Of String)))

		For i As Integer = 0 To ListBox1.Items.Count - 1
			Dim fi As FileInfo = ListBox1.Items(i)

			Dim str = fi.type.ToString + "|" + fi.separator + "|"
			If fi.field_association IsNot Nothing Then str += String.Join(";", fi.field_association)
			str += "|"

			Dim sub_split As String = ""
			For c As Integer = 0 To DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.None) - 1
				If fi.field_options.ContainsKey(c) Then
					sub_split += fi.field_options(c).sub_separator + "@"
					sub_split += String.Join("%", fi.field_options(c).replace) + "@"
					sub_split += String.Join("%", fi.field_options(c).replace_with) + "@"
					sub_split += String.Join("%", fi.field_options(c).assoc) + "@"
					sub_split += fi.field_options(c).parse_date.ToString + "@"
					sub_split += fi.field_options(c).parse_date_format + "@"
					sub_split += fi.field_options(c).set_to_constant.ToString + "@"
					sub_split += fi.field_options(c).set_to_constant_data + "@"
					sub_split += fi.field_options(c).require_regex + "@"
					sub_split += fi.field_options(c).match_mode.ToString + "@"
					sub_split += String.Join("%", fi.field_options(c).match_replace) + "@"
					sub_split += String.Join("%", fi.field_options(c).match_replace_with) + "@"
					sub_split += fi.field_options(c).match_replace_regex + "@"
					sub_split += fi.field_options(c).match_replace_regex_with + "@"
					sub_split += fi.field_options(c).match_query_mode.ToString()
				End If
				sub_split += ";"
			Next
			sub_split = sub_split.Substring(0, sub_split.Length - 1)
			str += sub_split + "|"

			For Each h In fi.headers
				str += h.db_field + "@" + h.header + "@" + h.header_is_db_field.ToString() + "@"
				For Each f In h.fields
					str += f.type.ToString() + "%" + f.db_field + "%" + f.name + "*"
				Next
				If str.EndsWith("*") Then str = str.Substring(0, str.Length - 1)
				str += ";"
			Next
			If str.EndsWith(";") Then str = str.Substring(0, str.Length - 1)

			str += "|" + fi.use_separator.ToString() + "|" + fi.fixed_width.ToString() + "|" + String.Join("@", fi.field_splits)
			str += "|" + fi.match_primary_multiple.ToString() + "|" + fi.match_secondary_zero.ToString() + "|" + fi.match_secondary_multiple.ToString()
			str += "|" + ComboBox5.SelectedIndex.ToString()
			str += "|" + ComboBox7.SelectedIndex.ToString()

			option_list.Add(str)
		Next

		Form1.ini.IniWriteValue("Presets_ImportAdv", ComboBox2.Text.Trim, String.Join("---", option_list))
		If Not ComboBox2.Items.Contains(ComboBox2.Text.Trim) Then ComboBox2.Items.Add(ComboBox2.Text.Trim)
	End Sub
	'Preset - Delete
	Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
		If ComboBox2.Text.Trim = "" Then Exit Sub
		Form1.ini.IniWriteValue("Presets_ImportAdv", ComboBox2.Text.Trim, Nothing)
		If ComboBox2.Items.Contains(ComboBox2.Text.Trim) Then ComboBox2.Items.Remove(ComboBox2.Text.Trim)
	End Sub

	'Add files
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim fb As New OpenFileDialog
		fb.InitialDirectory = Form1.ini.IniReadValue("Paths", "LastOpenedImportAdvDir")
		fb.Multiselect = True
		fb.ShowDialog()
		If fb.FileNames.Count = 0 Then Exit Sub

		Dim LastOpenedImportDir = fb.FileNames(0).Substring(0, fb.FileNames(0).LastIndexOf("\"))
		Form1.ini.IniWriteValue("Paths", "LastOpenedImportAdvDir", LastOpenedImportDir)

		For Each f In fb.FileNames
			Dim fi = New FileInfo(f)
			ListBox1.Items.Add(fi)

			Button1_Click_Fill_Preview_From_File(fi)
		Next

		If ListBox1.Items.Count > 0 Then ListBox1.SelectedIndex = 0
	End Sub
	Private Sub Button1_Click_Fill_Preview_From_File(fi As FileInfo)
		Dim regex As Regex = Nothing
		TextBox7.BackColor = Color.White
		If TextBox7.Text.Trim <> "" Then
			Try
				regex = New Regex(TextBox7.Text.Trim, RegexOptions.IgnoreCase)
			Catch ex As Exception
				regex = Nothing
				TextBox7.BackColor = Color.Red
			End Try
		End If

		Dim line = 0
		fi.preview = New List(Of String)
		Dim sr = IO.File.OpenText(fi.file)
		While Not sr.EndOfStream
			Dim str = sr.ReadLine

			If regex IsNot Nothing AndAlso Not regex.Match(str).Success Then Continue While

			fi.preview.Add(str)

			line += 1
			If line >= NumericUpDown1.Value Then Exit While
		End While
	End Sub
	'Select a file in listview
	Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
		For i As Integer = DataGridView1.Rows.Count - 1 To 1 Step -1
			DataGridView1.Rows.RemoveAt(i)
		Next

		GroupBox2.Enabled = False
		If ListBox1.SelectedIndex < 0 Then
			TextBox1.Text = ""
			NumericUpDown4.Value = 8
			RadioButton4.Checked = True
			TabControl1.SelectedIndex = 0
			For i As Integer = 0 To DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.None) - 1
				Dim cc As DataGridViewComboBoxCell = DataGridView1.Rows(0).Cells(i)
				cc.Value = cc.Items(0)
			Next

			Exit Sub
		End If

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		TabControl1.SelectedIndex = fi.type
		TextBox1.Text = fi.separator
		NumericUpDown4.Value = fi.fixed_width
		If fi.use_separator Then RadioButton4.Checked = True Else RadioButton5.Checked = True
		If fi.match_primary_multiple + fi.match_secondary_zero + fi.match_secondary_multiple = 0 Then Button21.BackColor = Color.Transparent Else Button21.BackColor = Color.GreenYellow
		ReGenerate_Preview_In_Table()

		ListBox2.Items.Clear()
		ListBox3.Items.Clear()
		For Each h In fi.headers
			ListBox2.Items.Add(h)
		Next
		GroupBox2.Enabled = True

		If ListBox2.Items.Count > 0 Then ListBox2.SelectedIndex = 0
		ListBox4_SelectedIndexChanged(ListBox4, New EventArgs)
	End Sub
	'Generate Preview
	Private Sub Refresh_Preview()
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)

		For i As Integer = DataGridView1.Rows.Count - 1 To 1 Step -1
			DataGridView1.Rows.RemoveAt(i)
		Next
		ReGenerate_Preview_In_Table()
	End Sub
	Private Sub ReGenerate_Preview_In_Table()
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		For Each p In fi.preview
			Dim fields = Preprocess_Row(p)

			Dim ind = DataGridView1.Rows.Add(fields)
			DataGridView1.Rows(ind).ReadOnly = True
		Next

		'Update association comboboxes in first row (when refreshing from changing file in listbox)
		If fi.field_association Is Nothing Then
			For i As Integer = 0 To DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.None) - 1
				Dim cc As DataGridViewComboBoxCell = DataGridView1.Rows(0).Cells(i)
				cc.Value = cc.Items(0)
			Next
		Else
			For i As Integer = 0 To fi.field_association.Count - 1
				Dim cc As DataGridViewComboBoxCell = DataGridView1.Rows(0).Cells(i)
				If fi.field_association(i) = "SUB-SPLIT" Then
					cc.Value = cc.Items(cc.Items.Count - 1)
				Else
					Dim field_ind = fieldsNames_DB.IndexOf(fi.field_association(i))
					cc.Value = cc.Items(field_ind + 1)
				End If
			Next
		End If

		Label27.Text = (DataGridView1.Rows.Count - 1).ToString + " Rows"
	End Sub
	Private Function Preprocess_Row(r As String, Optional fi As FileInfo = Nothing) As String()
		Dim fields As New List(Of String)
		If fi Is Nothing Then fi = DirectCast(ListBox1.SelectedItem, FileInfo)

		r = Handle_Tabs(r)
		If RadioButton4.Checked Then
			'Split by separator
			fields = r.Split({fi.separator}, StringSplitOptions.None).ToList()
		Else
			'Split by fixed width
			If fi.field_splits.Count = 0 Then
				'All fields have same width
				For i As Integer = 0 To r.Length - 1 Step NumericUpDown4.Value
					fields.Add(r.Substring(i, Math.Min(NumericUpDown4.Value, r.Length - i)).Trim)
				Next
			Else
				'Fields have separated width
				Dim start = 0
				For i As Integer = 0 To fi.field_splits.Count - 1
					If start >= r.Length Then Exit For
					If fi.field_splits(i) >= r.Length Then fields.Add(r.Substring(start)) : start = r.Length : Exit For
					fields.Add(r.Substring(start, fi.field_splits(i) - start).Trim)
					start = fi.field_splits(i)
				Next
				If start < r.Length Then fields.Add(r.Substring(start))
			End If
		End If

		'If we have a constant field at index superiour then fields count in current row, we need to add empty fields or
		'  the constant field will not be handled
		Dim need_add = fi.field_options.Where(Function(kv) kv.Value.set_to_constant AndAlso kv.Key >= fields.Count)
		If need_add.Count > 0 Then
			Dim max_add = need_add.Max(Function(x) x.Key)
			fields.AddRange(Enumerable.Repeat("", max_add - fields.Count + 1))
		End If

		For n As Integer = 0 To fields.Count - 1
			fields(n) = Net.WebUtility.HtmlDecode(fields(n))
			If Not fi.field_options.ContainsKey(n) Then Continue For

			If fi.field_options(n).replace(0) <> "" Then fields(n) = Replace(fields(n), fi.field_options(n).replace(0), fi.field_options(n).replace_with(0), 1, -1, CompareMethod.Text)
			If fi.field_options(n).replace(1) <> "" Then fields(n) = Replace(fields(n), fi.field_options(n).replace(1), fi.field_options(n).replace_with(1), 1, -1, CompareMethod.Text)
			If fi.field_options(n).replace(2) <> "" Then fields(n) = Replace(fields(n), fi.field_options(n).replace(2), fi.field_options(n).replace_with(2), 1, -1, CompareMethod.Text)

			If fi.field_options(n).parse_date AndAlso Not String.IsNullOrWhiteSpace(fi.field_options(n).parse_date_format) Then
				Dim res As Date
				If Date.TryParse(fields(n), res) Then fields(n) = res.ToString(fi.field_options(n).parse_date_format)
			End If

			If fi.field_options(n).set_to_constant Then fields(n) = fi.field_options(n).set_to_constant_data
		Next

		Return fields.ToArray()
	End Function
	Private Function Handle_Tabs(l As String)
		While l.Contains(vbTab)
			Dim ind = l.IndexOf(vbTab)
			l = Microsoft.VisualBasic.Replace(l, vbTab, New String(" "c, 8 - (ind Mod 8)), 1, 1)
		End While
		Return l
	End Function

	'IMPORT
	Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		GroupBox1.Enabled = False
		GroupBox2.Enabled = False
		If CheckBox4.Checked Then TextBox16.Text = "" : log.Clear() : ComboBox6.SelectedIndex = 0
		ComboBox6.Enabled = False

		ProgressBar1.Maximum = ListBox1.Items.Count * 100
		bg_worker.RunWorkerAsync()
	End Sub
	Private Sub Button5_Click_BG()
		Dim unique_fields As New List(Of String)
		For Each c As String In CheckedListBox1.CheckedItems
			Dim ind = fieldsNames_Display.IndexOf(c)
			If ind >= 0 Then unique_fields.Add(fieldsNames_DB(ind))
		Next

		'Get regex
		Dim regex As Regex = Nothing
		Me.Invoke(Sub() TextBox7.BackColor = Color.White)
		If TextBox7.Text.Trim <> "" Then
			Try
				regex = New Regex(TextBox7.Text.Trim, RegexOptions.IgnoreCase)
			Catch ex As Exception
				regex = Nothing
				Me.Invoke(Sub() TextBox7.BackColor = Color.Red)
			End Try
		End If

		Dim n = 0
		For Each fi As FileInfo In ListBox1.Items
			Me.Invoke(Sub() ProgressBar1.Value = n * 100)
			Me.Invoke(Sub() ProgressLabel.Text = "Reading " + IO.Path.GetFileName(fi.file) + "...")
			Dim lines = IO.File.ReadAllLines(fi.file)

			'Get real db fields from current fileinfo headers and header's fields
			Dim headers_real_db_fields As New List(Of String)
			Dim header_fields_real_db_fields As New Dictionary(Of Integer, List(Of String))
			If fi.type = 1 Then
				If fi.headers.Count = 0 Then Continue For
				For i As Integer = 0 To fi.headers.Count - 1
					If fi.headers(i).header_is_db_field Then
						Dim ind = fieldsNames_Display.IndexOf(fi.headers(i).db_field)
						If ind >= 0 Then headers_real_db_fields.Add(fieldsNames_DB(ind)) Else headers_real_db_fields.Add("")
					Else
						headers_real_db_fields.Add("")
					End If

					Dim list_t As New List(Of String)
					For f As Integer = 0 To fi.headers(i).fields.Count - 1
						Dim ind = fieldsNames_Display.IndexOf(fi.headers(i).fields(f).db_field)
						If ind >= 0 Then list_t.Add(fieldsNames_DB(ind)) Else list_t.Add("")
					Next
					header_fields_real_db_fields.Add(i, list_t)
				Next
			End If

			Dim header_order_metters = fi.headers.Where(Function(h) h.header_is_db_field).Count > 0 And fi.headers.Count > 1
			Dim current_header_index = -1
			Dim current_header_field_index = -1
			Dim target_main_id = -1
			Dim target_main_name = ""
			Dim target_db_field As String = ""
			Dim MODE_get_all_the_rest = False
			Dim MODE_get_all_the_rest_content As String = ""

			Dim l = 0
			Dim count_added = 0
			Dim count_added_category = 0
			Dim count_updated = 0
			Dim count_skipped = 0
			db.execute("BEGIN;")
			For Each line In lines
				l += 1
				If (l - 1) Mod 15 = 0 Then
					Me.Invoke(Sub() ProgressBar1.Value = n * 100 + Math.Round((l / lines.Count) * 100))
					Me.Invoke(Sub() ProgressLabel.Text = "Line " + l.ToString() + " of " + lines.Count.ToString())
				End If

				If (fi.type = 0) Then
					'Separated fields format
					If (fi.field_association Is Nothing) Then Continue For
					If regex IsNot Nothing AndAlso Not regex.Match(line).Success Then Continue For

					'Dim str = line.Split({fi.separator}, System.StringSplitOptions.None)
					Dim str = Preprocess_Row(line, fi)
					Dim cat As String = ""
					Dim names As New List(Of String)
					Dim values As New List(Of String)
					Dim check As New List(Of String)
					Dim primary_matches As New List(Of String)
					Dim secondary_matches As New List(Of KeyValuePair(Of String, String))
					For fieldN As Integer = 0 To str.Count - 1
						Dim f = fi.field_association(fieldN).ToLower()
						If f = "" Then Continue For

						Dim is_string_field = (f = "name" Or f.Contains("_str") Or f.Contains("_txt"))
						Dim data = str(fieldN).Replace("'", "''")

						'The field is in Match Mode
						If fi.field_options.ContainsKey(fieldN) AndAlso fi.field_options(fieldN).match_mode > 0 Then
							Dim preprocess = str(fieldN)
							If fi.field_options(fieldN).match_replace(0) <> "" Then preprocess = Replace(preprocess, fi.field_options(fieldN).match_replace(0), fi.field_options(fieldN).match_replace_with(0), 1, -1, CompareMethod.Text)
							If fi.field_options(fieldN).match_replace(1) <> "" Then preprocess = Replace(preprocess, fi.field_options(fieldN).match_replace(1), fi.field_options(fieldN).match_replace_with(1), 1, -1, CompareMethod.Text)
							If fi.field_options(fieldN).match_replace(2) <> "" Then preprocess = Replace(preprocess, fi.field_options(fieldN).match_replace(2), fi.field_options(fieldN).match_replace_with(2), 1, -1, CompareMethod.Text)

							'Primary Match
							Dim q = f + " = " + preprocess
							If is_string_field Then
								q = f + " = '" + preprocess.Replace("'", "''") + "'"
								Select Case fi.field_options(fieldN).match_query_mode
									Case 1 : q = f + " LIKE '" + preprocess.Replace("'", "''") + "'"
									Case 2 : q = f + " LIKE '%" + preprocess.Replace("'", "''") + "%'"
									Case 3 : q = f + " LIKE '%" + preprocess.Replace("'", "''") + "'"
									Case 4 : q = f + " LIKE '" + preprocess.Replace("'", "''") + "%'"
								End Select
							End If
							If fi.field_options(fieldN).match_mode = 1 Then primary_matches.Add(q)
							If fi.field_options(fieldN).match_mode = 2 Then secondary_matches.Add(New KeyValuePair(Of String, String)(f, preprocess.ToLower()))
						End If

						If f = "SUB-SPLIT".ToLower() Then
							If Not fi.field_options.ContainsKey(fieldN) Then Continue For
							Dim sub_split As String()
							If fi.field_options(fieldN).sub_separator <> "" Then sub_split = data.Split({fi.field_options(fieldN).sub_separator}, StringSplitOptions.None) Else sub_split = {data}
							For i As Integer = 0 To fi.field_options(fieldN).assoc.Count - 1
								f = fi.field_options(fieldN).assoc(i).Trim
								If f <> "" AndAlso sub_split.Count > i Then
									names.Add(f)
									If f = "name" Or f.Contains("_str") Or f.Contains("_txt") Then sub_split(i) = "'" + sub_split(i) + "'"
									If sub_split(i) = "" Then sub_split(i) = 0
									values.Add(sub_split(i))
								End If
							Next
						ElseIf f = "category" Then
							If data.Trim() <> "" Then cat = data.Trim()
						Else
							If is_string_field Then
								'string db field
								data = "'" + data + "'"
							ElseIf f.Contains("_num") Or f.Contains("_dec") Then
								'numeric db field
								data = data.Replace(",", ".")
								If data = "" Then data = "0"
							ElseIf f.Contains("_bool") Then
								'boolean db field
								If data = "" OrElse data.Substring(0, 1) = "0" OrElse data.Trim().ToLower() = "false" Then data = "'false'" Else data = "'true'"
							End If

							names.Add(f) : values.Add(data)
							If unique_fields.Contains(f) Then check.Add(f + " = " + data)
						End If
					Next

					If names.Count = 0 Then Continue For

					Dim matches = Button5_Click_BG_CheckMatches(fi, primary_matches, secondary_matches, check, values(0))
					If matches.need_skip Then count_skipped += 1 : Continue For

					'Process - Add or Update database
					If matches.id < 0 Then
						'Add new entry
						Dim sql = "INSERT INTO main (" + String.Join(", ", names) + ") VALUES ( " + String.Join(", ", values) + ");"
						Dim success = Button5_Click_BG_QueryDB_And_Log(sql, values(0))
						If success Then count_added += 1 Else count_skipped += 1

						'Add category
						If success AndAlso cat <> "" Then
							Dim lastRow = db.getLastRowID
							sql = "INSERT INTO category (id_main, cat) VALUES (" + lastRow.ToString + ", '" + cat + "');"
							If Button5_Click_BG_QueryDB_And_Log(sql, "Category (for new product): " + cat) Then count_added_category += 1
						End If
					Else
						'Update existing entry
						Dim sql = "UPDATE main SET "
						For i As Integer = 0 To names.Count - 1
							sql += names(i) + " = " + values(i) + ", "
						Next
						sql = sql.Substring(0, sql.Length - 2) + " WHERE id = " + matches.id.ToString() + ";"
						Dim success = Button5_Click_BG_QueryDB_And_Log(sql, values(0) + "(id = " + matches.id.ToString() + ")")
						If success Then count_updated += 1 Else count_skipped += 1

						'Update category
						If cat <> "" Then
							Dim replace As Boolean = Me.Invoke(Function() ComboBox7.SelectedIndex = 2)
							If replace Then
								db.execute("DELETE FROM category WHERE id_main = " + matches.id.ToString())
								sql = "INSERT INTO category (id_main, cat) VALUES (" + matches.id.ToString() + ", '" + cat + "');"
								If Button5_Click_BG_QueryDB_And_Log(sql, "Category (for existing product): " + cat) Then count_added_category += 1
							Else
								sql = "SELECT count(id) FROM category WHERE id_main = " + matches.id.ToString() + " AND cat = '" + cat + "' COLLATE NOCASE"
								Dim reader = db.queryReader(sql) : reader.Read()
								If reader.GetInt32(0) = 0 Then
									Dim add_only_if_null As Boolean = Me.Invoke(Function() ComboBox7.SelectedIndex = 1)
									If add_only_if_null Then
										sql = "SELECT count(id) FROM category WHERE id_main = " + matches.id.ToString()
										Dim reader2 = db.queryReader(sql) : reader2.Read()
										If reader2.GetInt32(0) = 0 Then
											sql = "INSERT INTO category (id_main, cat) VALUES (" + matches.id.ToString() + ", '" + cat + "');"
											If Button5_Click_BG_QueryDB_And_Log(sql, "Category (for existing product): " + cat) Then count_added_category += 1
										End If
									Else
										sql = "INSERT INTO category (id_main, cat) VALUES (" + matches.id.ToString() + ", '" + cat + "');"
										If Button5_Click_BG_QueryDB_And_Log(sql, "Category (for existing product): " + cat) Then count_added_category += 1
									End If
								End If
							End If
						End If
					End If
				ElseIf (fi.type = 1) Then
					'INI format
					If line.StartsWith("[") AndAlso line.EndsWith("]") Then
						'Header
						If MODE_get_all_the_rest AndAlso target_db_field.Trim <> "" AndAlso target_main_id > 0 Then
							MODE_get_all_the_rest_content = MODE_get_all_the_rest_content.Replace("'", "''")
							Dim sql = "UPDATE main SET " + target_db_field + " = '" + MODE_get_all_the_rest_content + "' WHERE id = " + target_main_id.ToString() + ";"
							Button5_Click_BG_QueryDB_And_Log(sql, target_main_name + " -> " + target_db_field + " = {CONTENT}")
						End If

						line = line.Substring(1, line.Length - 2)

						current_header_index += 1
						If current_header_index >= fi.headers.Count Then current_header_index = 0
						current_header_field_index = -1

						target_db_field = ""
						MODE_get_all_the_rest = False
						MODE_get_all_the_rest_content = ""

						Dim h = fi.headers(current_header_index)
						If h.header_is_db_field Then
							target_main_id = -1 : target_main_name = ""
							If headers_real_db_fields(current_header_index) = "" Then Continue For
							Dim sql = "SELECT id, name from main WHERE " + headers_real_db_fields(current_header_index) + " like '" + line.Replace("'", "''") + "';"
							Dim r = db.queryReader(sql)
							If r.HasRows Then r.Read() : target_main_id = r.GetInt32(0) : target_main_name = r.GetString(1)
						Else
							If header_order_metters AndAlso line.ToUpper <> h.header.ToUpper Then
								MsgBox("Awaited ini header [" + h.header + "], received [" + line + "]. Broken order. Abort this file.") : Exit For
							End If
						End If
					Else
						'Content
						If MODE_get_all_the_rest Then MODE_get_all_the_rest_content += line : Continue For

						If target_main_id < 0 Then Continue For
						If fi.headers(current_header_index).fields.Count = 0 Then Continue For

						current_header_field_index += 1
						If current_header_field_index >= fi.headers(current_header_index).fields.Count Then current_header_field_index = 0

						If line.Contains("=") Then
							Dim field_in_line = line.Substring(0, line.IndexOf("=")).ToUpper
							For i As Integer = 0 To fi.headers(current_header_index).fields.Count - 1
								Dim field = fi.headers(current_header_index).fields(i)
								If field.type = 0 And field.name.ToUpper = field_in_line Then
									Dim db_field = header_fields_real_db_fields(current_header_index)(i)
									If db_field <> "" Then
										Dim sql = "UPDATE main SET " + db_field + " = '" + line.Replace("'", "''") + "' WHERE id = " + target_main_id + ";"
										Button5_Click_BG_QueryDB_And_Log(sql, target_main_name + " -> " + db_field + " = '" + line.Replace("'", "''") + "' ")
									End If
								End If
							Next
						End If

						target_db_field = header_fields_real_db_fields(current_header_index)(current_header_field_index)
						If target_db_field.Trim = "" Then Continue For

						Dim f = fi.headers(current_header_index).fields(current_header_field_index)
						If f.type = 1 Then
							'Get Full Line
							Dim sql = "UPDATE main SET " + target_db_field + " = '" + line.Replace("'", "''") + "' WHERE id = " + target_main_id.ToString() + ";"
							Button5_Click_BG_QueryDB_And_Log(sql, target_main_name + " -> " + line)
						ElseIf f.type = 2 Then
							'All the rest
							MODE_get_all_the_rest = True
							MODE_get_all_the_rest_content = line
						End If
					End If
				End If
			Next
			Me.Invoke(Sub() ProgressLabel.Text = "Commit changes...")
			db.execute("COMMIT;")

			n += 1

			'Show final log (per file)
			If CheckBox4.Checked Then
				Dim txt = "Added: " + count_added.ToString() + ", "
				txt += "Added Categories: " + count_added_category.ToString + ", "
				txt += "Updated: " + count_updated.ToString + ", "
				txt += "Skipped: " + count_skipped.ToString + ", "
				txt += "Total: " + (count_added + count_updated + count_skipped).ToString
				Log_Message("0", "0", "---------------------------------")
				Log_Message("0", "0", txt, 3)
			End If
		Next


		Me.Invoke(Sub() ProgressBar1.Value = 0)
		Me.Invoke(Sub() ProgressLabel.Text = "Idle")
		Me.Invoke(Sub() GroupBox1.Enabled = True)
		Me.Invoke(Sub() GroupBox2.Enabled = True)
		Me.Invoke(Sub() ComboBox6.Enabled = True)
		MsgBox("Done")
	End Sub
	Private Function Button5_Click_BG_CheckMatches(fi As FileInfo, primary_matches As List(Of String), secondary_matches As List(Of KeyValuePair(Of String, String)), check As List(Of String), product_name As String) As (need_skip As Boolean, id As Integer)
		'Check if a product already exist in database
		Dim id_main = -1
		If check.Count > 0 Then
			Dim sql As String = "SELECT id FROM main WHERE " + String.Join(" AND ", check)
			Dim dt = db.queryDataset(sql)
			If dt.Rows.Count > 0 Then id_main = CInt(dt.Rows(0)(0))
			If dt.Rows.Count > 1 Then
				Log_Message("1", "2", "Skip    - " + product_name + " because it have multiple mathes withing database")
				Return (True, -1)
			End If
		End If

		'Check if a product already exist in database - Using Primary/Secondary Matches
		If primary_matches.Count > 0 Then
			Dim sql As String = "SELECT * FROM main WHERE " + String.Join(" AND ", primary_matches)
			Dim dt = db.queryDataset(sql)
			If dt.Rows.Count > 0 Then id_main = CInt(dt.Rows(0).Item("id"))
			If dt.Rows.Count > 1 Then
				If fi.match_primary_multiple = 0 Then
					'Primary matches multiple - skip
					Log_Message("1", "2", "Skip    - " + product_name + " because it have multiple primary matches")
					Return (True, -1)
				Else
					'Primary matches multiple - process secondary / update all
					'TODO: - not implimented
					Log_Message("1", "2", "Skip    - " + product_name + " because it have multiple primary matches, but PROCESS SECONDARY / UPDATE ALL is NOT IMPLEMENTED")
					Return (True, -1)
				End If
			End If

			If secondary_matches.Count > 0 AndAlso dt.Rows.Count > 0 Then
				For r As Integer = dt.Rows.Count - 1 To 0 Step -1
					For Each kv In secondary_matches
						Dim db_field_content = dt.Rows(0).Item(kv.Key).ToString()
						If db_field_content.ToLower() <> kv.Value.ToLower() Then dt.Rows.RemoveAt(r) : Exit For
					Next
				Next

				If dt.Rows.Count = 0 Then
					If fi.match_secondary_zero = 0 Then
						'Secondary zero match = skip
						Log_Message("1", "2", "Skip    - " + product_name + " because secondary matches failed")
						Return (True, -1)
					Else
						'Secondary zero match = add new
						id_main = -1
					End If
				ElseIf dt.Rows.Count > 1 Then
					If fi.match_secondary_multiple = 0 Then
						'Secondary multiple matches = skip
						Log_Message("1", "2", "Skip    - " + product_name + " because it have multiple secondary matches")
						Return (True, -1)
					Else
						'Secondary multiple matches = update all
						'TODO: - not implimented
						Log_Message("1", "2", "Skip    - " + product_name + " because it have multiple secondary matches, but UPDATE ALL is NOT IMPLEMENTED")
						Return (True, -1)
					End If
				End If
			End If
		End If

		Return (False, id_main)
	End Function
	Private Function Button5_Click_BG_QueryDB_And_Log(sql As String, product_name As String) As Boolean
		If sql.ToUpper().StartsWith("INSERT") Then
			If Not product_name.ToUpper.StartsWith("CATEGORY (FOR") Then
				Dim main_mode_can_add As Boolean = Me.Invoke(Function() ComboBox5.SelectedIndex = 0 Or ComboBox5.SelectedIndex = 1)
				If Not main_mode_can_add Then
					Log_Message("1", "1", "Skip    - " + product_name + " because main mode is not allow to ADD NEW entries")
					Return False
				End If
				Log_Message("0", "1", "Add New - " + product_name)
			Else
				If product_name.ToUpper.StartsWith("CATEGORY (FOR NEW PRODUCT):") Then
					Dim category_can_add As Boolean = Me.Invoke(Function() ComboBox7.SelectedIndex = 0 Or ComboBox7.SelectedIndex = 1 Or ComboBox7.SelectedIndex = 2)
					If Not category_can_add Then
						Log_Message("1", "1", "Skip    - " + product_name + " because category mode is not allow ADD NEW entries")
						Return False
					End If
				ElseIf product_name.ToUpper.StartsWith("CATEGORY (FOR EXISTING PRODUCT):") Then
					Dim category_can_add As Boolean = Me.Invoke(Function() ComboBox7.SelectedIndex = 0 Or ComboBox7.SelectedIndex = 1 Or ComboBox7.SelectedIndex = 2)
					If Not category_can_add Then
						Log_Message("1", "1", "Skip    - " + product_name + " because category mode is not allow UPDATE existing entries")
						Return False
					End If
				Else
					Return False
				End If
				Log_Message("0", "1", "Add New - " + product_name)
			End If
		ElseIf sql.ToUpper().StartsWith("UPDATE") Then
			Dim main_mode_can_update As Boolean = Me.Invoke(Function() ComboBox5.SelectedIndex = 0 Or ComboBox5.SelectedIndex = 2)
			If Not main_mode_can_update Then
				Log_Message("1", "2", "Skip    - " + product_name + " because main mode is not allow to UPDATE existing entries")
				Return False
			End If
			Log_Message("0", "2", "Update  - " + product_name)
		End If

		If Not CheckBox5.Checked Then db.execute(sql)
		Return True
	End Function
	Private Sub Log_Message(success As String, category As String, msg As String, Optional new_line_count As Integer = 1)
		'success = 0 - success, 1 - fail
		'category = 0 - system, 1 - add new, 2 - update, 3 - other
		If CheckBox4.Checked Then
			'Me.Invoke(Sub() TextBox16.AppendText(msg + New String(vbCrLf, new_line_count)))
			Me.Invoke(Sub() TextBox16.AppendText(msg + vbCrLf))
			log.Add(success + category + msg)

			For i As Integer = 2 To new_line_count
				Me.Invoke(Sub() TextBox16.AppendText("   " + vbCrLf))
				log.Add(success + category + "   ")
			Next
		End If
	End Sub

	'Custom per field split markup system
	'Set width per column button
	Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If Not GroupBox2.Enabled Then Exit Sub

		Dim r = DataGridView1.SelectedRows
		If r Is Nothing OrElse r.Count = 0 Then MsgBox("Select a row in the table.") : Exit Sub
		Dim r_ind = DataGridView1.SelectedRows(0).Index - 1
		If r_ind < 0 Then MsgBox("Select a row in the table.") : Exit Sub

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		Dim str = Handle_Tabs(fi.preview(r_ind))

		Panel1.Visible = True
		DataGridView2.Rows.Clear()
		DataGridView2.Columns.Clear()
		For n As Integer = 0 To str.Length - 1
			DataGridView2.Columns.Add("col_" + n.ToString, "col_" + n.ToString)
			DataGridView2.Columns(n).Width = 16
		Next
		DataGridView2.Rows.Add()
		For n As Integer = 0 To str.Length - 1
			DataGridView2.Rows(0).Cells(n).Value = str(n).ToString
		Next
	End Sub
	Private Sub DataGridView2_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView2.CellClick
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim cell_bounds = DataGridView2.GetCellDisplayRectangle(e.ColumnIndex, 0, False)
		Dim cell_middle = DataGridView2.PointToScreen(cell_bounds.Location).X + CInt(Math.Round(cell_bounds.Width / 2))

		Dim split = 0
		If Cursor.Position.X < cell_middle Then split = e.ColumnIndex Else split = e.ColumnIndex + 1
		If split = 0 Then Exit Sub
		If split >= DataGridView2.Columns.Count Then Exit Sub

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		If fi.field_splits.Contains(split) Then fi.field_splits.Remove(split) Else fi.field_splits.Add(split) : fi.field_splits.Sort()

		DataGridView2.InvalidateCell(DataGridView2.Rows(0).Cells(split))
		If split > 0 Then DataGridView2.InvalidateCell(DataGridView2.Rows(0).Cells(split - 1))
	End Sub
	Private Sub DataGridView2_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles DataGridView2.CellPainting
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim back_brush = New SolidBrush(e.CellStyle.BackColor)
		e.Graphics.FillRectangle(back_brush, e.CellBounds)
		If e.Value IsNot Nothing Then
			e.Graphics.DrawString(e.Value.ToString, e.CellStyle.Font, New SolidBrush(e.CellStyle.ForeColor), e.CellBounds.X, e.CellBounds.Y + 2, StringFormat.GenericDefault)
		End If

		'e.Paint(e.ClipBounds, DataGridViewPaintParts.All)
		'e.Paint(e.CellBounds, DataGridViewPaintParts.All)

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		Dim br = Brushes.White
		If fi.field_splits.Contains(e.ColumnIndex) Then br = Brushes.Red
		Dim pa1 = New Point(e.CellBounds.Left - 1, e.CellBounds.Y)
		Dim pa2 = New Point(e.CellBounds.Left - 1, e.CellBounds.Bottom)
		If pa1.X >= 1 AndAlso pa1.X < DataGridView2.Width - 1 Then e.Graphics.DrawLine(New Pen(br, 3), pa1, pa2)

		br = Brushes.White
		If fi.field_splits.Contains(e.ColumnIndex + 1) Then br = Brushes.Red
		Dim pb1 = New Point(e.CellBounds.Right - 3, e.CellBounds.Y)
		Dim pb2 = New Point(e.CellBounds.Right - 3, e.CellBounds.Bottom)
		If pb1.X > 1 AndAlso pb1.X < DataGridView2.Width - 1 Then e.Graphics.DrawLine(New Pen(br, 3), pb1, pb2)

		e.Handled = True
	End Sub
	'Clear splits
	Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		fi.field_splits.Clear()
		DataGridView2.Refresh()
	End Sub
	'Close splits markup
	Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
		Panel1.Visible = False
		Refresh_Preview()
	End Sub

	'Global and Field Match Options Button Show
	Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click, Button21.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		If sender Is Button18 AndAlso ListBox4.SelectedIndex < 0 Then Exit Sub

		Groupboxes_Enable_States(0) = GroupBox1.Enabled
		Groupboxes_Enable_States(1) = GroupBox2.Enabled
		GroupBox1.Enabled = False
		GroupBox2.Enabled = False

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		If sender Is Button21 Then
			'Global Match Options
			GroupBox3.Visible = True

			If fi.match_primary_multiple = 0 Then RadioButton9.Checked = True Else RadioButton10.Checked = True
			If fi.match_secondary_zero = 0 Then RadioButton11.Checked = True Else RadioButton12.Checked = True
			If fi.match_secondary_multiple = 0 Then RadioButton13.Checked = True Else RadioButton14.Checked = True
		Else
			'Field Match Options
			GroupBox4.Visible = True

			Dim fieldN = ListBox4.SelectedIndex
			If fi.field_options Is Nothing OrElse Not fi.field_options.ContainsKey(fieldN) Then
				For Each t In GroupBox4.Controls.OfType(Of TextBox)
					t.Text = ""
				Next
			Else
				TextBox17.Text = fi.field_options(fieldN).match_replace(0)
				TextBox18.Text = fi.field_options(fieldN).match_replace_with(0)
				TextBox19.Text = fi.field_options(fieldN).match_replace(1)
				TextBox20.Text = fi.field_options(fieldN).match_replace_with(1)
				TextBox21.Text = fi.field_options(fieldN).match_replace(2)
				TextBox22.Text = fi.field_options(fieldN).match_replace_with(2)
				TextBox23.Text = fi.field_options(fieldN).match_replace_regex
				TextBox24.Text = fi.field_options(fieldN).match_replace_regex_with
				Select Case fi.field_options(fieldN).match_query_mode
					Case 0 : RadioButton15.Checked = True
					Case 1 : RadioButton16.Checked = True
					Case 2 : RadioButton17.Checked = True
					Case 3 : RadioButton18.Checked = True
					Case 4 : RadioButton19.Checked = True
				End Select
			End If
		End If
	End Sub
	'Global and Field Match Options Button Hide
	Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click, Button19.Click
		GroupBox1.Enabled = Groupboxes_Enable_States(0)
		GroupBox2.Enabled = Groupboxes_Enable_States(1)

		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		If sender Is Button20 Then
			'Global Match Options Hide
			GroupBox3.Visible = False

			If RadioButton9.Checked Then fi.match_primary_multiple = 0 Else fi.match_primary_multiple = 1
			If RadioButton11.Checked Then fi.match_secondary_zero = 0 Else fi.match_secondary_zero = 1
			If RadioButton13.Checked Then fi.match_secondary_multiple = 0 Else fi.match_secondary_multiple = 1

			Dim res = fi.match_primary_multiple + fi.match_secondary_zero + fi.match_secondary_multiple
			If res = 0 Then Button21.BackColor = Color.Transparent Else Button21.BackColor = Color.GreenYellow
		Else
			'Field Match Options Hide
			GroupBox4.Visible = False

			Dim fieldN = ListBox4.SelectedIndex
			If fi.field_options Is Nothing OrElse Not fi.field_options.ContainsKey(fieldN) Then OnChangeFieldOptions(Me, New EventArgs)
			If fi.field_options Is Nothing OrElse Not fi.field_options.ContainsKey(fieldN) Then Exit Sub

			fi.field_options(fieldN).match_replace = {TextBox17.Text, TextBox19.Text, TextBox21.Text}
			fi.field_options(fieldN).match_replace_with = {TextBox18.Text, TextBox20.Text, TextBox22.Text}
			fi.field_options(fieldN).match_replace_regex = TextBox23.Text.Trim
			fi.field_options(fieldN).match_replace_regex_with = TextBox24.Text.Trim
			Select Case GroupBox4.Controls.OfType(Of RadioButton).Where(Function(r) r.Checked).FirstOrDefault.Name
				Case "RadioButton15" : fi.field_options(fieldN).match_query_mode = 0
				Case "RadioButton16" : fi.field_options(fieldN).match_query_mode = 1
				Case "RadioButton17" : fi.field_options(fieldN).match_query_mode = 2
				Case "RadioButton18" : fi.field_options(fieldN).match_query_mode = 3
				Case "RadioButton19" : fi.field_options(fieldN).match_query_mode = 4
			End Select

			Dim res = String.Join("", GroupBox4.Controls.OfType(Of TextBox).Select(Of String)(Function(t) t.Text.Trim))
			If res = "" And RadioButton16.Checked Then Button18.BackColor = Color.Transparent Else Button18.BackColor = Color.GreenYellow
		End If
	End Sub

	'Refresh preview regex button
	Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub
		Dim fi = DirectCast(ListBox1.SelectedItem, FileInfo)
		Button1_Click_Fill_Preview_From_File(fi)
		Refresh_Preview()
	End Sub
	'Filter Log
	Private Sub ComboBox6_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox6.SelectedIndexChanged
		TextBox16.Text = ""
		Dim txt As String = ""
		For Each l In log
			Dim filtered = False
			If l.Substring(1, 1) <> "0" Then
				Select Case ComboBox6.SelectedIndex
					Case 1 : If l.Substring(1, 1) <> "1" Then filtered = True 'add
					Case 2 : If l.Substring(1, 1) <> "2" Then filtered = True 'update
					Case 3 : If l.Substring(0, 1) <> "0" Then filtered = True 'success
					Case 4 : If l.Substring(0, 1) <> "1" Then filtered = True 'skipped
				End Select
			End If
			'If Not filtered Then TextBox16.AppendText(l.Substring(2) + vbCrLf)
			If Not filtered Then txt += l.Substring(2) + vbCrLf
		Next
		TextBox16.Text = txt
	End Sub
	'Show Tooltip
	Private Sub Button17_MouseEnter(sender As Object, e As EventArgs) Handles Button17.MouseEnter
		Dim msg = "Use this to check if imported entry already exist in database." + vbCrLf + vbCrLf
		msg += "Before adding a entry, the sql query to existing db is performed with all PRIMARY fields." + vbCrLf
		msg += "i.e. if you have 'Name' and 'Published by' fields set as match primary," + vbCrLf
		msg += "the following query will be performed based on data being imported: " + vbCrLf
		msg += "SELECT * WHERE name = 'Call of Duty' AND field_str3 = 'Activision'" + vbCrLf + vbCrLf
		msg += "If one or more rows will be found, additional checks will be performed using SECONDARY field." + vbCrLf + vbCrLf
		msg += "The final import behaviour is configured in the global section."
		Custom_ToolTip.Show(msg, Me, Button17)
	End Sub
	Private Sub Button17_MouseLeave(sender As Object, e As EventArgs) Handles Button17.MouseLeave
		Custom_ToolTip.Hide()
	End Sub
End Class