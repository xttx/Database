Imports System.ComponentModel

Public Class Form7_Options
    Dim ini As IniFileApi = Form1.ini
    Dim refr = True
    Dim MainForm As Form1
    Dim old_font_size As Decimal = 0
    Public Shared need_refresh_main_form = False

    Public Sub New(main_form As Form1)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        MainForm = main_form
    End Sub

    Private Sub Form7_Options_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Main
        TextBox9.Text = ini.IniReadValue("Main", "DescriptionsBaseUrl", True)
        TextBox1.Text = ini.IniReadValue("Paths", "Screenshots")
        TextBox2.Text = ini.IniReadValue("Paths", "LibraryPath")
        TextBox3.Text = ini.IniReadValue("Paths", "ExoDosPath")
        TextBox4.Text = ini.IniReadValue("Paths", "ExoWinPath")
        TextBox5.Text = ini.IniReadValue("Paths", "ExoAddPath")

        Dim dxWndPaths As String = ini.IniReadValue("Paths", "DxWndPaths")
        If dxWndPaths <> "" Then
            If dxWndPaths.Contains(":") Then
                For Each i In dxWndPaths.Split({";"}, StringSplitOptions.RemoveEmptyEntries)
                    ListBox1.Items.Add(i.Trim)
                Next
            Else
                ListBox1.Items.Add(dxWndPaths)
            End If
        End If

        Dim dosBoxPaths As String = ini.IniReadValue("Paths", "DosBoxPaths")
        If dosBoxPaths <> "" Then
            For Each i In dosBoxPaths.Split({";"}, StringSplitOptions.RemoveEmptyEntries)
                Dim arr = i.Split({"|"}, StringSplitOptions.RemoveEmptyEntries)
                If arr.Count = 2 Then ListBox2.Items.Add(New DosBox_Path_ListItem() With {.name = arr(0), .path = arr(1)})
            Next
        End If

        NumericUpDown1.Value = MainForm.ListBox1.Font.Size
        old_font_size = MainForm.ListBox1.Font.Size
        Dim t = ini.IniReadValue("Main", "DeleteConfirm")
        If t = "0" OrElse t.ToUpper = "FALSE" Then CheckBox4.Checked = False
        t = ini.IniReadValue("Interface", "ShowPathFilter")
        If t = "1" OrElse t.ToUpper = "TRUE" Then CheckBox3.Checked = True

        'Screenshots
        t = ini.IniReadValue("Screenshots", "FirstSpecial")
        If t <> "" Then CheckBox1.Checked = Boolean.Parse(t)
        t = ini.IniReadValue("Screenshots", "FirstOptional")
        If t <> "" Then CheckBox2.Checked = t
        t = ini.IniReadValue("Screenshots", "FirstIndex")
        If t <> "" Then NumericUpDown2.Value = Decimal.Parse(t)
        TextBox6.Text = ini.IniReadValue("Screenshots", "FirstSuffix")
        TextBox7.Text = ini.IniReadValue("Screenshots", "Suffix")
        TextBox8.Text = ini.IniReadValue("Screenshots", "IndexFormat")


        'Show/hide
        Dim HideMenus = ini.IniReadValue("Interface", "HideMenus").ToUpper.Split({","}, StringSplitOptions.RemoveEmptyEntries)
        For Each m As ToolStripMenuItem In MainForm.MenuStrip1.Items.OfType(Of ToolStripMenuItem)
            Dim node = TreeView1.Nodes.Add(m.Name, m.Text)
            If HideMenus.Contains(m.Name.ToUpper) Then node.Checked = False Else node.Checked = True

            For Each sub_item As ToolStripMenuItem In m.DropDownItems.OfType(Of ToolStripMenuItem)
                Dim sub_node = node.Nodes.Add(sub_item.Name, sub_item.Text)
                If HideMenus.Contains(sub_item.Name.ToUpper) Then sub_node.Checked = False Else sub_node.Checked = True
            Next
        Next

        refr = False
    End Sub
    Private Sub Form7_Options_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'if we are just close form, and canceling options, restore old font size
        MainForm.ListBox1.Font = New Font(MainForm.ListBox1.Font.FontFamily, old_font_size, FontStyle.Regular)
    End Sub

    'Save
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ini.IniWriteValue("Main", "DescriptionsBaseUrl", TextBox9.Text.Trim)
        ini.IniWriteValue("Paths", "Screenshots", TextBox1.Text.Trim)
        ini.IniWriteValue("Paths", "LibraryPath", TextBox2.Text.Trim)
        ini.IniWriteValue("Paths", "ExoDosPath", TextBox3.Text.Trim)
        ini.IniWriteValue("Paths", "ExoWinPath", TextBox4.Text.Trim)
        ini.IniWriteValue("Paths", "ExoAddPath", TextBox5.Text.Trim)

        Dim dxWndPaths As String = ""
        For Each item In ListBox1.Items
            dxWndPaths += item.ToString + ";"
        Next
        If dxWndPaths.Length > 1 Then dxWndPaths = dxWndPaths.Substring(0, dxWndPaths.Length - 1)
        ini.IniWriteValue("Paths", "DxWndPaths", dxWndPaths)

        Dim dosBoxPaths As String = ""
        For Each item In ListBox2.Items
            Dim dpli = DirectCast(item, DosBox_Path_ListItem)
            dosBoxPaths += dpli.name + "|" + dpli.path + ";"
        Next
        If dosBoxPaths.Length > 1 Then dosBoxPaths = dosBoxPaths.Substring(0, dosBoxPaths.Length - 1)
        ini.IniWriteValue("Paths", "DosBoxPaths", dosBoxPaths)

        ini.IniWriteValue("Main", "DeleteConfirm", CheckBox4.Checked.ToString)
        ini.IniWriteValue("Interface", "ListFontSize", NumericUpDown1.Value.ToString)
        ini.IniWriteValue("Interface", "ShowPathFilter", CheckBox3.Checked.ToString)

        ini.IniWriteValue("Screenshots", "FirstSpecial", CheckBox1.Checked.ToString)
        ini.IniWriteValue("Screenshots", "FirstSuffix", TextBox6.Text.Trim)
        ini.IniWriteValue("Screenshots", "FirstOptional", CheckBox2.Checked.ToString)
        ini.IniWriteValue("Screenshots", "FirstIndex", NumericUpDown2.Value.ToString)
        ini.IniWriteValue("Screenshots", "Suffix", TextBox7.Text.Trim)
        ini.IniWriteValue("Screenshots", "IndexFormat", TextBox8.Text.Trim)

        Dim unchecked_nodes_names = TreeView1.Nodes.GetAllNodesRecur().Where(Function(n) n.Checked = False).Select(Of String)(Function(x) x.Name)
        ini.IniWriteValue("Interface", "HideMenus", String.Join(",", unchecked_nodes_names))

        need_refresh_main_form = True
        old_font_size = NumericUpDown1.Value
        Me.Close()
    End Sub

    'DxWnd/Dosbox Listboxes - drag'n'drop
    Private Sub ListBox_DragEnter(sender As Object, e As DragEventArgs) Handles ListBox1.DragEnter, ListBox2.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub ListBox1_DragDrop(sender As Object, e As DragEventArgs) Handles ListBox1.DragDrop, ListBox2.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) AndAlso e.Data.GetData(DataFormats.FileDrop) IsNot Nothing Then
            Dim f = e.Data.GetData(DataFormats.FileDrop)(0).ToString
            If sender Is ListBox1 Then
                'DxWnd path listbox
                If FileIO.FileSystem.FileExists(f) Then
                    ListBox1.Items.Add(IO.Path.GetDirectoryName(f))
                ElseIf FileIO.FileSystem.DirectoryExists(f) Then
                    ListBox1.Items.Add(f)
                End If
            ElseIf sender Is ListBox2 Then
                'Dosbox path listbox
                If FileIO.FileSystem.FileExists(f) Then
                    If IO.Path.GetExtension(f).ToUpper = ".EXE" Then ListBox2.Items.Add(New DosBox_Path_ListItem() With {.path = f})
                ElseIf FileIO.FileSystem.DirectoryExists(f) Then
                    If FileIO.FileSystem.FileExists(f + "\DosBox.exe") Then ListBox2.Items.Add(New DosBox_Path_ListItem() With {.path = f + "\DosBox.exe"})
                End If
            End If
        End If
    End Sub
    'DxWnd/Dosbox - Listbox remove
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click, Button4.Click
        If sender Is Button2 Then
            If ListBox1.SelectedIndex < 0 Then Exit Sub
            ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
        ElseIf sender Is Button4 Then
            If ListBox2.SelectedIndex < 0 Then Exit Sub
            ListBox2.Items.RemoveAt(ListBox2.SelectedIndex)
        End If
    End Sub
    'DxWnd Listbox - update games
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim dxWnds As New List(Of String)
        Dim files_upper As New List(Of String)
        Dim indexes As New List(Of Integer)

        Dim list_entries As New List(Of String)
        For Each item In ListBox1.Items
            list_entries.Add(item.ToString)
        Next
        list_entries.Sort()

        Dim ini As New IniFileApi
        For Each item In list_entries
            Dim dxWndIni = item + "\dxwnd.ini"
            If FileIO.FileSystem.FileExists(dxWndIni) Then
                ini.path = dxWndIni
                Dim n As Integer = 0
                Do While ini.IniReadValue("target", "title" + n.ToString) <> ""
                    Dim file1 = ini.IniReadValue("target", "path" + n.ToString)
                    Dim file2 = ini.IniReadValue("target", "launchpath" + n.ToString)
                    If file1 <> "" Then dxWnds.Add(item + "\dxwnd.exe") : files_upper.Add(file1.ToUpper) : indexes.Add(n + 1)
                    If file2 <> "" Then dxWnds.Add(item + "\dxwnd.exe") : files_upper.Add(file2.ToUpper) : indexes.Add(n + 1)

                    'If file1 <> "" Or file2 <> "" Then indexes.Add(n + 1)
                    n += 1
                Loop
            End If
        Next

        db.execute("DELETE FROM paths WHERE name = 'DxWnd'")

        Dim c As Integer = 0
        Dim ind As Integer = 0
        Dim r = db.queryReader("SELECT main_id, name, value FROM paths WHERE name = 'File'")
        Do While r.Read
            Dim f = r.GetString(2).Trim

            ind = f.ToUpper.IndexOf("\DRIVE\C\")
            If ind >= 0 Then f = f.Substring(ind + 7).Insert(1, ":")
            If f <> "" Then
                ind = files_upper.LastIndexOf(f.ToUpper)
                If ind >= 0 Then
                    Dim cli = dxWnds(ind) + "%%%/r:" + indexes(ind).ToString
                    db.execute("INSERT INTO paths (main_id, name, value) VALUES (" + r.GetInt32(0).ToString + ", 'DxWnd', '" + cli + "')")
                    c += 1
                End If
            End If
        Loop

        need_refresh_main_form = True
        MsgBox("Done. " + c.ToString + " games detected in DxWnd.")
    End Sub
    'Dosbox Listbox - browse
    Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox2.SelectedIndexChanged
        refr = True
        If ListBox2.SelectedIndex < 0 Then
            TextBox10.Text = ""
        Else
            TextBox10.Text = DirectCast(ListBox2.SelectedItem, DosBox_Path_ListItem).name
        End If
        refr = False
    End Sub
    'Dosbox Listbox - change dosbox name
    Private Sub TextBox10_TextChanged(sender As Object, e As EventArgs) Handles TextBox10.TextChanged
        If refr Then Exit Sub
        If ListBox2.SelectedIndex < 0 Then refr = True : TextBox10.Text = "" : refr = False : Beep() : Exit Sub
        Dim dpli = DirectCast(ListBox2.SelectedItem, DosBox_Path_ListItem)
        dpli.name = TextBox10.Text.Trim

        'Refresh the litbox item
        'ListBox2.Items(ListBox2.SelectedIndex) = ListBox2.Items(ListBox2.SelectedIndex) 'BAD, because change active textbox cursor position
        ListBox2.DrawMode = DrawMode.OwnerDrawFixed
        ListBox2.DrawMode = DrawMode.Normal
    End Sub

    'Change Font Size
    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        If refr Then Exit Sub
        MainForm.ListBox1.Font = New Font(MainForm.ListBox1.Font.FontFamily, NumericUpDown1.Value, FontStyle.Regular)
    End Sub

    Public Class DosBox_Path_ListItem
        Public path As String = ""
        Public name As String = ""
        Public Overrides Function ToString() As String
            If name = "" Then Return path
            Return name + " - " + path
        End Function
    End Class
End Class