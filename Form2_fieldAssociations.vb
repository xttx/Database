Imports System.ComponentModel

Public Class Form2_fieldAssociations
    Public Shared need_refresh_main_form = False

    Dim refreshing As Boolean = False
    Dim indices As Class01_db.index_info() = db.GetIndexInfo()

    Private Sub Form2_fieldAssociations_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Setup tooltips
        Dim msg = "The name of the field." + vbCrLf + "Leave empty, to disable this field."
        Custom_ToolTip.Setup(Label1, msg, Me) : Custom_ToolTip.Setup(TextBox1, msg, Me)         'Field Name
        msg = "Allow change value of this field in the main form (i.e. make field editable)." + vbCrLf + "Bool fields became checkboxes, String fields - textboxes." + vbCrLf + "String fields with list values become dropdowns, optionally with multiple choices."
        Custom_ToolTip.Setup(CheckBox1, msg, Me)                                                'Writable
        msg = "With writeable string fields theese values will be shown in dropdown menu, to choose from." + vbCrLf + "The values should be separated by "";"""
        Custom_ToolTip.Setup(Label2, msg, Me) : Custom_ToolTip.Setup(TextBox2, msg, Me)         'List Values
        msg = "With writable string fields with list values, this will allow to choose multiple values," + vbCrLf + "wich will be stored in database separated by ';'."
        Custom_ToolTip.Setup(CheckBox5, msg, Me)                                                'List Values - Multiple
        msg = "Show this field in 'sort' menu."
        Custom_ToolTip.Setup(CheckBox2, msg, Me)                                                'Sortable
        msg = "Show this field in filters."
        Custom_ToolTip.Setup(CheckBox3, msg, Me)                                                'Filtrable
        msg = "Indicates, that value of this field can be multiple, comma separated values." + vbCrLf + "This will split this field by comma before filling the filter dropdown."
        Custom_ToolTip.Setup(CheckBox4, msg, Me)                                                'Is List
        msg = "For 'Is List' values, this will create cache table in the database, with all distinct splitted values for filter." + vbCrLf + "This can speedup catalog loading time, but when adding a new value to this field, you'll need to recreate this cache, ot new values will not be shown in the filter."
        Custom_ToolTip.Setup(Button4, msg, Me)                                                  'Is List - Create List Cache
        msg = "Show next value to the right of this one in the same row, instead of showing it on the next row."
        Custom_ToolTip.Setup(CheckBox6, msg, Me)                                                'Layout - NoBR
        msg = "Add context menu to this field label, to go directly to showing product."
        Custom_ToolTip.Setup(CheckBox7, msg, Me)                                                'Search for Name
        msg = "Move this field up in the list. The fields are shown on the main form in the same order as in list."
        Custom_ToolTip.Setup(Button2, msg, Me)                                                  'Move Field Up
        msg = "Move this field down in the list. The fields are shown on the main form in the same order as in list."
        Custom_ToolTip.Setup(Button3, msg, Me)                                                  'Move Field Down
        msg = "The offset of the topmost fieldset 'categorie' field, from the pictureboxes."
        Custom_ToolTip.Setup(Label3, msg, Me) : Custom_ToolTip.Setup(NumericUpDown1, msg, Me)   'Layout - Top Offset
        msg = "Vertical space between fields."
        Custom_ToolTip.Setup(Label4, msg, Me) : Custom_ToolTip.Setup(NumericUpDown2, msg, Me)   'Layout - Vertical Gap
        msg = "Horizontal space between a field and the following field with enabled 'Layout NoBR'."
        Custom_ToolTip.Setup(Label5, msg, Me) : Custom_ToolTip.Setup(NumericUpDown3, msg, Me)   'Layout - Col Width

        need_refresh_main_form = False

        'Fields layout
        NumericUpDown1.Value = Form1.NEW_CONTROL_TOP_ADD
        NumericUpDown2.Value = Form1.NEW_CONTROL_TOP_MULTIPLIER
        NumericUpDown3.Value = Form1.NEW_CONTROL_COL_WIDTH

        Form2_fieldAssociations_Load_FillList()
    End Sub
    Private Sub Form2_fieldAssociations_Load_FillList()
        Dim t As String = ""
        Dim t1 As String = ""
        Dim t1a As Boolean = False
        Dim t2 As String = ""
        Dim t3 As String = ""
        Dim t3a As Boolean = False
        Dim t4 As String = ""
        Dim t4a As Boolean = False
        Dim t5 As String = ""
        Dim t5a As Boolean = False
        Dim t6 As String = ""
        Dim t6a As Boolean = False
        Dim cacheExist As Boolean = False
        Dim multiple As Boolean = False
        Dim ini = Form1.ini
        Dim lst As New List(Of item)

        Dim noBR = ini.IniReadValue("Interface", "FieldsLayout_NoBR").ToUpper

        Dim fieldTypeN As Integer = 0
        For Each fieldType In fieldTypeArr
            For i As Integer = 1 To fieldCountArr(fieldTypeN)
                Dim f As String = fieldType + i.ToString

                'New Method
                Dim field_data = ini.IniReadValue("Interface", f).Split({"|||"}, StringSplitOptions.None)
                t = "" 'name
                If field_data.Count > 0 Then t = field_data(0)
                If field_data.Count > 1 AndAlso field_data(1) = "1" Then t1a = True Else t1a = False
                If field_data.Count > 2 AndAlso field_data(2) = "1" Then t3a = True Else t3a = False
                If field_data.Count > 3 AndAlso field_data(3) = "1" Then t4a = True Else t4a = False
                If field_data.Count > 4 AndAlso field_data(4) = "1" Then t5a = True Else t5a = False
                If field_data.Count > 5 AndAlso field_data(5) = "1" Then t6a = True Else t6a = False

                'Old Method
                't = ini.IniReadValue("Interface", f)
                't1 = ini.IniReadValue("Interface", f + "_write")
                'If t1.ToUpper = "TRUE" Or t1 = "1" Then t1a = True Else t1a = False
                't3 = ini.IniReadValue("Interface", f + "_sortable")
                'If t3.ToUpper = "TRUE" Or t3 = "1" Then t3a = True Else t3a = False
                't4 = ini.IniReadValue("Interface", f + "_filtrable")
                'If t4.ToUpper = "TRUE" Or t4 = "1" Then t4a = True Else t4a = False
                't6 = ini.IniReadValue("Interface", f + "_isLink")
                'If t6.ToUpper = "TRUE" Or t6 = "1" Then t6a = True Else t6a = False
                't5 = ini.IniReadValue("Interface", f + "_isList")
                'If t5.ToUpper = "TRUE" Or t5 = "1" Then t5a = True Else t5a = False

                t2 = ini.IniReadValue("Interface", f + "_listValues")
                multiple = t2.ToUpper.EndsWith(";{Multiple}".ToUpper)
                If multiple Then t2 = t2.Substring(0, t2.LastIndexOf(";")).Trim

                Dim realFieldName = ("data" + f.Substring(f.IndexOf("_"))).ToLower
                cacheExist = db.queryReader("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE '_" + realFieldName + "_listCache'").HasRows
                Dim l As New item With {.field = f, .fieldname = t, .writeable = t1a, .listValues = t2, .sortable = t3a, .filtrable = t4a, .isLink = t6a, .isList = t5a, .cacheExist = cacheExist, .listValuesMultiple = multiple, .DBfieldname = realFieldName}
                l.noBR = noBR.Contains(f.ToUpper)
                lst.Add(l)
            Next
            fieldTypeN += 1
        Next

        'Order handling
        t = ini.IniReadValue("Interface", "Field_order")
        If t = "" Then
            For i As Integer = 1 To fieldCountArr.Sum
                t = t + i.ToString + ","
            Next
            t = t.Substring(0, t.Length - 1)
        End If
        't = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43"

        Dim lst_reordered As New List(Of item)
        Dim lst_reordered_done(fieldCountArr.Sum - 1) As Boolean
        For i As Integer = 0 To fieldCountArr.Sum - 1
            lst_reordered_done(i) = False
        Next
        For Each ord As String In t.Split({","c}, System.StringSplitOptions.RemoveEmptyEntries)
            Dim onum As Integer = CInt(ord)
            If onum > lst.Count Then Exit For
            lst_reordered.Add(lst(onum - 1))
            lst_reordered_done(onum - 1) = True
        Next
        For i As Integer = 0 To fieldCountArr.Sum - 1
            If lst_reordered_done(i) = False Then
                lst_reordered.Add(lst(i))
            End If
        Next

        ListBox1.Items.Clear()
        For Each l In lst_reordered
            ListBox1.Items.Add(l)
        Next

        'Select first item
        ListBox1.SelectedIndex = 0
    End Sub

    'Browse fields in listbox
    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        If refreshing Then Exit Sub

        refreshing = True
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
            TextBox2.Text = ""
            CheckBox1.Checked = False
            CheckBox5.Checked = False
            CheckBox6.Checked = False
            CheckBox7.Checked = False
            Button5.Enabled = False : Button6.Enabled = False : Button7.Enabled = False
            Button4.Text = "Create List Cache" : Button4.Tag = "" : Button4.Enabled = False
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            If l.cacheExist Then
                Button4.Text = "Delete List Cache" : Button4.Tag = "DEL"
            Else
                Button4.Text = "Create List Cache" : Button4.Tag = "CREATE"
            End If
            TextBox1.Text = l.fieldname
            TextBox2.Text = l.listValues
            CheckBox1.Checked = l.writeable
            CheckBox2.Checked = l.sortable
            CheckBox3.Checked = l.filtrable
            CheckBox4.Checked = l.isList
            CheckBox5.Checked = l.listValuesMultiple
            CheckBox6.Checked = l.noBR
            CheckBox7.Checked = l.isLink
            If Not CheckBox1.Checked And CheckBox4.Checked Then Button4.Enabled = True Else Button4.Enabled = False

            If l.field.ToUpper.Contains("STR") Then
                TextBox2.Enabled = True
                CheckBox5.Enabled = True
            Else
                TextBox2.Text = ""
                TextBox2.Enabled = False
                CheckBox5.Checked = False
                CheckBox5.Enabled = False
            End If
            If Not l.field.ToUpper.Contains("TXT") Then
                CheckBox1.Enabled = True
            Else
                CheckBox1.Checked = False
                CheckBox1.Enabled = False
            End If

            Dim indexed = indices.Where(Function(i) i.field.ToUpper() = l.DBfieldname.ToUpper()).Count > 0
            Button5.Enabled = Not indexed : Button6.Enabled = Not indexed : Button7.Enabled = indexed
        End If
        refreshing = False
    End Sub

    'Change field settings
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        If refreshing Then Exit Sub
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.fieldname = TextBox1.Text.Trim

            'To refresh item in listbox
            refreshing = True
            ListBox1.Items(ListBox1.SelectedIndex) = ListBox1.SelectedItem
            refreshing = False
        End If
    End Sub
    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.writeable = CheckBox1.Checked
            Button4_CheckStatus()
        End If
    End Sub
    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged, CheckBox5.CheckedChanged
        If refreshing Then Exit Sub
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
            CheckBox5.Checked = False
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.listValues = TextBox2.Text.Trim
            l.listValuesMultiple = CheckBox5.Checked
        End If
    End Sub
    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.sortable = CheckBox2.Checked
        End If
    End Sub
    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.filtrable = CheckBox3.Checked
        End If
    End Sub
    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.isList = CheckBox4.Checked
            Button4_CheckStatus()
        End If
    End Sub
    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.noBR = CheckBox6.Checked
        End If
    End Sub
    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        If ListBox1.SelectedIndex < 0 Then
            TextBox1.Text = ""
        Else
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            l.isLink = CheckBox7.Checked
        End If
    End Sub

    'Create list cache
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If ListBox1.SelectedIndex >= 0 Then
            Dim l = DirectCast(ListBox1.SelectedItem, item)

            If Button4.Tag IsNot Nothing AndAlso Button4.Tag = "CREATE" Then
                Dim realFieldName = ("data" + l.field.Substring(l.field.IndexOf("_"))).ToLower
                Dim dt As DataTable = db.queryDataset("SELECT DISTINCT " + realFieldName + " FROM main")
                Dim dtMod As New DataTable
                Dim alreadyAdded As New List(Of String)
                dtMod.Columns.Add(l.field)
                For Each r As DataRow In dt.Rows
                    If r.Item(0).ToString = "" And Not alreadyAdded.Contains("{Empty}") Then
                        dtMod.Rows.Add({"{Empty}"}) : alreadyAdded.Add("{Empty}")
                    End If
                    For Each item In r.Item(0).ToString.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
                        If Not alreadyAdded.Contains(item.Trim.ToUpper) Then
                            dtMod.Rows.Add({item.Trim})
                            alreadyAdded.Add(item.Trim.ToUpper)
                        End If
                    Next
                Next

                Dim tbl = "_" + realFieldName + "_listCache"
                db.execute("create table " + tbl + " (id INTEGER PRIMARY KEY AUTOINCREMENT, value varchar(255) )")
                db.transactionBegin()
                For Each r As DataRow In dtMod.Rows
                    db.execute("INSERT INTO " + tbl + " (value) VALUES ('" + r(0).ToString.Replace("'", "''") + "')", True)
                Next
                db.transactionCommit()

                l.cacheExist = True
                Button4.Text = "Delete List Cache" : Button4.Tag = "DEL"
            ElseIf Button4.Tag IsNot Nothing AndAlso Button4.Tag = "DEL" Then
                Dim frm = DirectCast(Me.Owner, Form1)
                'Unlock table, that can be used by filter comboboxes
                db.close() : db.connect()

                Dim realFieldName = ("data" + l.field.Substring(l.field.IndexOf("_"))).ToLower
                db.execute("DROP TABLE _" + realFieldName + "_listCache")
                l.cacheExist = False
                Button4.Text = "Create List Cache" : Button4.Tag = "CREATE"
            End If
        End If
        need_refresh_main_form = True
    End Sub
    Private Sub Button4_CheckStatus()
        If Not CheckBox1.Checked And CheckBox4.Checked Then Button4.Enabled = True Else Button4.Enabled = False
        If refreshing Then Exit Sub
        If ListBox1.SelectedIndex >= 0 Then
            Dim l = DirectCast(ListBox1.SelectedItem, item)
            If l.cacheExist And (CheckBox1.Checked Or Not CheckBox4.Checked) Then
                Dim resp = MsgBox("This will delete list_cache. Do you want to continue?", MsgBoxStyle.YesNo)
                If resp = MsgBoxResult.Yes Then
                    Button4_Click(Button4, New EventArgs)
                ElseIf resp = MsgBoxResult.No Then
                    CheckBox1.Checked = False
                    CheckBox4.Checked = True
                End If
            End If
        End If
    End Sub

    'move up
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If ListBox1.SelectedIndex < 1 Then Exit Sub
        Dim tmp As item = DirectCast(ListBox1.Items(ListBox1.SelectedIndex - 1), item)
        ListBox1.Items(ListBox1.SelectedIndex - 1) = ListBox1.Items(ListBox1.SelectedIndex)
        ListBox1.Items(ListBox1.SelectedIndex) = tmp
        ListBox1.SelectedIndex = ListBox1.SelectedIndex - 1
    End Sub
    'move down
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex >= ListBox1.Items.Count - 1 Then Exit Sub
        Dim tmp As item = DirectCast(ListBox1.Items(ListBox1.SelectedIndex), item)
        ListBox1.Items(ListBox1.SelectedIndex) = ListBox1.Items(ListBox1.SelectedIndex + 1)
        ListBox1.Items(ListBox1.SelectedIndex + 1) = tmp
        ListBox1.SelectedIndex = ListBox1.SelectedIndex + 1
    End Sub

    'OK
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim ini = Form1.ini
        Dim ord(fieldCountArr.Sum - 1) As Integer

        Dim n As Integer = 1
        Dim fieldTypeN As Integer = 0
        Dim nobr As New List(Of String)
        For Each field_type In fieldTypeArr
            For i As Integer = 1 To fieldCountArr(fieldTypeN)
                Dim search = field_type + i.ToString
                For l As Integer = 0 To ListBox1.Items.Count - 1
                    Dim item = DirectCast(ListBox1.Items(l), item)
                    If item.field.ToUpper = search.ToUpper Then
                        ord(l) = n
                        'New method
                        Dim w_str = item.fieldname + "|||"
                        If item.writeable Then w_str += "1|||" Else w_str += "0|||"
                        If item.sortable Then w_str += "1|||" Else w_str += "0|||"
                        If item.filtrable Then w_str += "1|||" Else w_str += "0|||"
                        If item.isList Then w_str += "1|||" Else w_str += "0|||"
                        If item.isLink Then w_str += "1|||" Else w_str += "0"
                        ini.IniWriteValue("Interface", search, w_str)

                        'Old method
                        'ini.IniWriteValue("Interface", search, item.fieldname)
                        'ini.IniWriteValue("Interface", search + "_write", item.writeable.ToString)
                        'ini.IniWriteValue("Interface", search + "_sortable", item.sortable.ToString)
                        'ini.IniWriteValue("Interface", search + "_filtrable", item.filtrable.ToString)
                        'ini.IniWriteValue("Interface", search + "_isList", item.isList.ToString)
                        'ini.IniWriteValue("Interface", search + "_isLink", item.isLink.ToString)
                        If item.listValuesMultiple Then
                            ini.IniWriteValue("Interface", search + "_listValues", item.listValues + ";{Multiple}")
                        Else
                            If Not String.IsNullOrWhiteSpace(item.listValues) Then
                                ini.IniWriteValue("Interface", search + "_listValues", item.listValues)
                            Else
                                ini.IniWriteValue("Interface", search + "_listValues", Nothing)
                            End If
                        End If
                        If item.noBR Then nobr.Add(search)
                    End If
                Next
                n += 1
            Next
            fieldTypeN += 1
        Next

        Dim ord_str = String.Join(",", ord)
        ini.IniWriteValue("Interface", "Field_order", ord_str)
        If nobr.Count > 0 Then ini.IniWriteValue("Interface", "FieldsLayout_NoBR", String.Join(",", nobr.ToArray)) Else ini.IniWriteValue("Interface", "FieldsLayout_NoBR", Nothing)

        'Fields layout
        Form1.NEW_CONTROL_TOP_ADD = NumericUpDown1.Value
        Form1.NEW_CONTROL_TOP_MULTIPLIER = NumericUpDown2.Value
        Form1.NEW_CONTROL_COL_WIDTH = NumericUpDown3.Value
        ini.IniWriteValue("Interface", "FieldsLayout_TopOffset", NumericUpDown1.Value.ToString)
        ini.IniWriteValue("Interface", "FieldsLayout_VerticalGap", NumericUpDown2.Value.ToString)
        ini.IniWriteValue("Interface", "FieldsLayout_ColumnWidth", NumericUpDown3.Value.ToString)

        need_refresh_main_form = True
        Me.Close()
    End Sub

    'Add a field to database
    Private Sub Add_Field_Button_Click(sender As Object, e As EventArgs) Handles Add_STR_Field_Button.Click, Add_BOOL_Field_Button.Click, Add_NUM_Field_Button.Click, Add_DEC_Field_Button.Click
        need_refresh_main_form = True

        Dim Field_Type_To_Add = ""
        If sender Is Add_STR_Field_Button Then
            Field_Type_To_Add = "str"
        ElseIf sender Is Add_BOOL_Field_Button Then
            Field_Type_To_Add = "bool"
        ElseIf sender Is Add_NUM_Field_Button Then
            Field_Type_To_Add = "num"
        ElseIf sender Is Add_DEC_Field_Button Then
            Field_Type_To_Add = "dec"
        Else
            MsgBox("This button not handled.") : Exit Sub
        End If
        Me.Enabled = False

        'Get field_type index
        Dim Field_Type_To_Add_N = 0
        For n = 0 To fieldTypeArr.Count - 1
            If fieldTypeArr(n).ToLower = "field_" + Field_Type_To_Add Then Field_Type_To_Add_N = n : Exit For
        Next

        fieldCountArr(Field_Type_To_Add_N) += 1

        Dim bg As New BackgroundWorker()
        AddHandler bg.DoWork, AddressOf Add_Field_Button_Click_BG
        AddHandler bg.RunWorkerCompleted, AddressOf Recrete_main_table_Complete
        bg.RunWorkerAsync(Field_Type_To_Add_N)
    End Sub
    Private Sub Add_Field_Button_Click_BG(o As Object, e As DoWorkEventArgs)
        Dim Field_Type_To_Add_N = DirectCast(e.Argument, Integer)
        Dim old_label_text = Label6.Text

        Label6.Invoke(Sub() Label6.Text = "Geting main table fields...")
        Dim FieldsStr = ""
        Dim FieldsList As New List(Of String)
        Dim r_info = db.queryReader("PRAGMA table_info(main)")
        Do While r_info.Read
            FieldsList.Add(r_info.GetString(1))
        Loop
        FieldsStr = String.Join(",", FieldsList.ToArray)

        Recrete_main_table(FieldsStr, FieldsStr, Label6)

        Label6.Invoke(Sub() Label6.Text = old_label_text)
    End Sub
    'Remove a field from database
    Private Sub Remove_Field_Button_Click(sender As Object, e As EventArgs) Handles Remove_Field_Button.Click
        If ListBox1.SelectedIndex < 0 Then MsgBox("Please, select a field to remove in the list.") : Exit Sub
        Me.Enabled = False

        Dim field = DirectCast(ListBox1.SelectedItem, item).field.ToLower
        Dim realFieldName = ("data" + field.Substring(field.IndexOf("_")))

        Dim empty = ""
        If realFieldName.Contains("_str") Then
            empty = "''"
        ElseIf realFieldName.Contains("_bool") Then
            empty = "'false'"
        ElseIf realFieldName.Contains("_num") Then
            empty = "0"
        ElseIf realFieldName.Contains("_dec") Then
            empty = "0"
        Else
            MsgBox("Can not delete this type of field.") : Me.Enabled = True : Exit Sub
        End If

        'Get field_type index
        Dim Field_Type_To_Add_N = 0
        For n = 0 To fieldTypeArr.Count - 1
            If field.StartsWith(fieldTypeArr(n).ToLower) Then Field_Type_To_Add_N = n : Exit For
        Next

        If fieldCountArr(Field_Type_To_Add_N) < 1 Then MsgBox("Error: fieldCount less then 1.") : Me.Enabled = True : Exit Sub
        need_refresh_main_form = True

        Dim r = db.queryReader("SELECT " + realFieldName + " FROM main WHERE " + realFieldName + " IS NOT NULL AND " + realFieldName + " <> " + empty)
        If r.HasRows Then
            If MsgBox("The " + realFieldName + " column have data in the main table. Are you sure you want to delete this column?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Me.Enabled = True : Exit Sub
        End If

        fieldCountArr(Field_Type_To_Add_N) -= 1

        Dim bg As New BackgroundWorker()
        AddHandler bg.DoWork, AddressOf Remove_Field_Button_Click_BG
        AddHandler bg.RunWorkerCompleted, AddressOf Recrete_main_table_Complete
        bg.RunWorkerAsync(realFieldName)
    End Sub
    Private Sub Remove_Field_Button_Click_BG(o As Object, e As DoWorkEventArgs)
        Dim Field_To_Remove = DirectCast(e.Argument, String).ToLower
        Dim old_label_text = Label7.Text

        Label7.Invoke(Sub() Label7.Text = "Geting main table fields...")
        Dim FieldsOldStr = ""
        Dim FieldsOldList As New List(Of String)
        Dim FieldsAllList As New List(Of String)
        Dim r_info = db.queryReader("PRAGMA table_info(main)")
        Do While r_info.Read
            Dim f = r_info.GetString(1)
            FieldsAllList.Add(r_info.GetString(1))
            If f.ToLower <> Field_To_Remove Then FieldsOldList.Add(r_info.GetString(1))
        Loop
        FieldsOldStr = String.Join(",", FieldsOldList.ToArray)

        Dim FieldsNewStr = ""
        Dim Field_wo_Number = System.Text.RegularExpressions.Regex.Replace(Field_To_Remove, "[\d-]", String.Empty).ToLower
        For n = FieldsAllList.Count - 1 To 0 Step -1
            If FieldsAllList(n).ToLower.StartsWith(Field_wo_Number) Then FieldsAllList.RemoveAt(n) : Exit For
        Next
        FieldsNewStr = String.Join(",", FieldsAllList.ToArray)

        If FieldsOldList.Count <> FieldsAllList.Count Then MsgBox("Error: Old fieldlist and new fieldlist count does not match.") : Exit Sub

        Recrete_main_table(FieldsOldStr, FieldsNewStr, Label7)

        Label7.Invoke(Sub() Label7.Text = old_label_text)
    End Sub
    Private Sub Recrete_main_table(OldFieldSet As String, Newfieldset As String, StatusLabel As Label)
        StatusLabel.Invoke(Sub() StatusLabel.Text = "Renaming main table...")
        db.execute("ALTER TABLE main RENAME TO main_OLD")

        StatusLabel.Invoke(Sub() StatusLabel.Text = "Creating new main table...")
        db.create_database_createMainTable()

        StatusLabel.Invoke(Sub() StatusLabel.Text = "Populating new main table with data...")
        Dim sql = "INSERT INTO main (" + Newfieldset + ") SELECT " + OldFieldSet + " FROM main_OLD"
        db.execute("BEGIN;") : db.execute(sql) : db.execute("COMMIT;")
        db.close() : db.connect()

        StatusLabel.Invoke(Sub() StatusLabel.Text = "Deleting old main table...")
        db.execute("DROP TABLE main_OLD")

        If MsgBox("Compact database? (VACUUM)", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            StatusLabel.Invoke(Sub() StatusLabel.Text = "Compacting database (VACUUM)...")
            db.execute("VACUUM")
        End If
    End Sub
    Private Sub Recrete_main_table_Complete()
        Form2_fieldAssociations_Load_FillList()
        Me.Enabled = True
    End Sub
    'Create/Delete DB index
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click, Button6.Click, Button7.Click
        Dim l = DirectCast(ListBox1.SelectedItem, item)
        Try
            If sender Is Button5 Then
                'Create unique index
                'Create index unique - CREATE UNIQUE INDEX IF NOT EXISTS MyUniqueIndexName ON auth_user (email)
                db.execute("CREATE UNIQUE INDEX IF NOT EXISTS " + l.DBfieldname + "_uind ON main(" + l.DBfieldname + ")")
            ElseIf sender Is Button6 Then
                'Create no-unique index
                'Create index no-unique - CREATE INDEX IF NOT EXISTS MyUniqueIndexName ON auth_user (email)
                db.execute("CREATE INDEX IF NOT EXISTS " + l.DBfieldname + "_ind ON main(" + l.DBfieldname + ")")
            ElseIf sender Is Button7 Then
                'Delete index
                Dim index = indices.Where(Function(i) i.field.ToUpper() = l.DBfieldname.ToUpper())
                If index.Count = 0 Then MsgBox("Can't find index to delete.") : Exit Sub
                If index.Count > 1 Then MsgBox("Multiple indices found, can't delete. You'll need to remove the index manually.") : Exit Sub
                db.close() : db.connect()
                db.execute("DROP INDEX IF EXISTS " + index(0).name)
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            indices = db.GetIndexInfo()
            Dim indexed = indices.Where(Function(i) i.field.ToUpper() = l.DBfieldname.ToUpper()).Count > 0
            Button5.Enabled = Not indexed : Button6.Enabled = Not indexed : Button7.Enabled = indexed
        End Try
    End Sub
End Class

Public Class item
    Public field As String = ""
    Public fieldname As String = ""
    Public DBfieldname As String = ""
    Public writeable As Boolean = False
    Public sortable As Boolean = False
    Public filtrable As Boolean = False
    Public isList As Boolean = False
    Public listValues As String = ""
    Public listValuesMultiple As Boolean = False
    Public cacheExist As Boolean = False
    Public isLink As Boolean = False
    Public noBR As Boolean = False

    Public Overrides Function ToString() As String
        If fieldname <> "" Then
            Return field + " - " + fieldname
        Else
            Return field
        End If
    End Function
End Class