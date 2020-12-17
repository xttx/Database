Public Class Form8_paths
    Dim main_form As Form1
    Dim WithEvents l As VirtualListBox.VListBox
    Dim panel As SplitterPanel

    Dim CONTROL_TOP As Integer = 30
    Dim CONTROL_MULTIPLIER As Integer = 30
    Dim controls_count As Integer

    Public Sub New(MainForm As Form1)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        main_form = MainForm
    End Sub

    Private Sub Form8_paths_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        l = main_form.ListBox1
        panel = main_form.SplitContainer3.Panel1

        Me.TopMost = CheckBox1.Checked
        AddHandler l.SelectedIndexChanged, AddressOf listBrowse

        Dim sql = "SELECT DISTINCT name FROM paths"
        Dim r = db.queryReader(sql)
        Do While r.Read
            ComboBox1.Items.Add(r.GetString(0))
        Loop

        listBrowse()
    End Sub

    Private Sub listBrowse()
        Label1.Text = "No Game Selected"
        controls_count = 0
        For i As Integer = Me.Controls.OfType(Of TextBox).Count - 1 To 0 Step -1
            Me.Controls.Remove(Me.Controls.OfType(Of TextBox)()(i))
        Next
        For i As Integer = Me.Controls.OfType(Of Button).Count - 1 To 0 Step -1
            If Me.Controls.OfType(Of Button)()(i).Name.ToUpper.StartsWith("RemBtn".ToUpper) Then
                Me.Controls.Remove(Me.Controls.OfType(Of Button)()(i))
            End If
        Next

        If l.SelectedIndex < 0 Then Exit Sub
        Label1.Text = "Selected Game: " + DirectCast(l.SelectedItem, DataRowView).Row.Item(1).ToString

        Dim id = DirectCast(l.SelectedItem, DataRowView).Row.Item(0).ToString
        Dim sql = "SELECT id, name, value FROM paths WHERE main_id = " + id
        Dim r = db.queryReader(sql)
        Dim n As Integer = 1
        Dim anchors_tx = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Dim anchors_bt = AnchorStyles.Top Or AnchorStyles.Right
        Do While r.Read
            Dim tx1 As New TextBox With {.Name = "TxtName" + n.ToString, .Width = 130, .ReadOnly = True}
            Dim tx2 As New TextBox With {.Name = "TxtPath" + n.ToString, .Width = 370 + Me.Width - 630, .Anchor = anchors_tx}
            Dim btn As New Button With {.Name = "RemBtn" + n.ToString, .Width = 30, .Height = 20, .Text = "X", .Anchor = anchors_bt}
            Me.Controls.Add(tx1)
            Me.Controls.Add(tx2)
            Me.Controls.Add(btn)
            tx1.Left = 10
            tx2.Left = 180
            btn.Left = 570 + Me.Width - 630
            tx1.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (n - 1))
            tx2.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (n - 1))
            btn.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (n - 1))
            tx2.Tag = r.GetInt32(0).ToString
            tx1.Text = r.GetString(1)
            tx2.Text = r.GetString(2)
            tx2.AllowDrop = True
            AddHandler btn.Click, AddressOf removePath
            AddHandler tx2.TextChanged, AddressOf textChange
            AddHandler tx2.DragEnter, AddressOf dragEnter1
            AddHandler tx2.DragDrop, AddressOf drop
            n += 1
        Loop
        controls_count = n - 1
    End Sub

    'Add new
    Private Sub Button1_Click(sender As Object, e As EventArgs, Optional f As String = "") Handles Button1.Click
        If controls_count >= 10 Then Exit Sub

        Dim name = ComboBox1.Text.Trim
        If name = "" Then Exit Sub

        controls_count += 1
        Dim anchors_tx = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Dim anchors_bt = AnchorStyles.Top Or AnchorStyles.Right
        Dim tx1 As New TextBox With {.Name = "TxtName" + controls_count.ToString, .Width = 130, .ReadOnly = True}
        Dim tx2 As New TextBox With {.Name = "TxtPath" + controls_count.ToString, .Width = 370 + Me.Width - 630, .Anchor = anchors_tx}
        Dim btn As New Button With {.Name = "RemBtn" + controls_count.ToString, .Width = 30, .Height = 20, .Text = "X", .Anchor = anchors_bt}
        Me.Controls.Add(tx1)
        Me.Controls.Add(tx2)
        Me.Controls.Add(btn)
        tx1.Left = 10
        tx2.Left = 180
        btn.Left = 570 + Me.Width - 630
        tx1.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (controls_count - 1))
        tx2.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (controls_count - 1))
        btn.Top = CONTROL_TOP + (CONTROL_MULTIPLIER * (controls_count - 1))
        tx1.Text = name
        tx2.Text = ""
        tx2.AllowDrop = True
        AddHandler btn.Click, AddressOf removePath
        AddHandler tx2.TextChanged, AddressOf textChange
        AddHandler tx2.DragEnter, AddressOf dragEnter1
        AddHandler tx2.DragDrop, AddressOf drop
        If f.Trim <> "" Then tx2.Text = f

        tx2.Focus()
    End Sub

    'Text Change
    Private Sub textChange(sender As Object, e As EventArgs)
        If l.SelectedIndex < 0 Then Exit Sub
        Dim id = DirectCast(l.SelectedItem, DataRowView).Row.Item(0).ToString

        Dim tx = CType(sender, TextBox)
        Dim ns = tx.Name.Substring(tx.Name.Length - 2)
        If Not IsNumeric(ns) Then ns = ns.Substring(1)
        Dim n = CInt(ns)

        Dim name = CType(Me.Controls("TxtName" + n.ToString), TextBox).Text.Trim
        If tx.Text.Trim = "" AndAlso tx.Tag IsNot Nothing AndAlso tx.Tag.ToString.Trim <> "" Then
            db.execute("DELETE FROM paths WHERE main_id = " + id + " AND name = '" + name.Replace("'", "''") + "' AND id = " + tx.Tag.ToString)
            If CheckBox3.Checked Then
                'unset HAVE
                For Each chk In panel.Controls.OfType(Of CheckBox)
                    If chk.Text.ToUpper.Contains("HAVE") AndAlso chk.Checked Then chk.Checked = False
                Next
            End If
        ElseIf tx.Text.Trim <> "" Then
            Dim sql = ""
            If tx.Tag Is Nothing OrElse tx.Tag.ToString.Trim = "" Then
                sql = "INSERT INTO paths (main_id, name, value) VALUES (" + id + ", '" + name.Replace("'", "''") + "', '" + tx.Text.Trim.Replace("'", "''") + "')"
                db.execute(sql)
                tx.Tag = db.getLastRowID.ToString
            Else
                sql = "UPDATE paths SET value = '" + tx.Text.Trim.Replace("'", "''") + "' WHERE main_id = " + id + " AND name = '" + name.Replace("'", "''") + "' AND id = " + tx.Tag.ToString
                db.execute(sql)
            End If

            'Dim sql = "SELECT count(*) FROM paths WHERE main_id = " + id + " AND name = '" + name.Replace("'", "''") + "'"
            'Dim r = db.queryReader(sql)
            'r.Read()
            'If r.GetInt32(0) = 0 Then
            '    sql = "INSERT INTO paths (main_id, name, value) VALUES (" + id + ", '" + name.Replace("'", "''") + "', '" + tx.Text.Trim.Replace("'", "''") + "')"
            '    db.execute(sql)
            'Else
            '    sql = "UPDATE paths SET value = '" + tx.Text.Trim.Replace("'", "''") + "' WHERE main_id = " + id + " AND name = '" + name.Replace("'", "''") + "'"
            '    db.execute(sql)
            'End If

            If CheckBox2.Checked Then
                'set HAVE
                For Each chk In panel.Controls.OfType(Of CheckBox)
                    If chk.Text.ToUpper.Contains("HAVE") AndAlso Not chk.Checked Then chk.Checked = True
                Next
            End If
        End If
    End Sub
    'Drag&Drop
    Private Sub dragEnter1(o As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub drop(o As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) AndAlso e.Data.GetData(DataFormats.FileDrop) IsNot Nothing Then
            DirectCast(o, TextBox).Text = e.Data.GetData(DataFormats.FileDrop)(0)
        End If
    End Sub
    'Drag&Drop Label
    Private Sub Label2_DragEnter(sender As Object, e As DragEventArgs) Handles Label2.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub Label2_DragDrop(sender As Object, e As DragEventArgs) Handles Label2.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) AndAlso e.Data.GetData(DataFormats.FileDrop) IsNot Nothing Then
            Dim f = e.Data.GetData(DataFormats.FileDrop)(0).ToString
            If FileIO.FileSystem.FileExists(f) Then
                ComboBox1.Text = "File"
            ElseIf FileIO.FileSystem.DirectoryExists(f) Then
                ComboBox1.Text = "Folder"
            Else
                Exit Sub
            End If
            Button1_Click(Button1, New EventArgs, f)
        End If
    End Sub

    'Remove button
    Private Sub removePath(sender As Object, e As EventArgs)
        Dim btn = CType(sender, Button)
        Dim ns = btn.Name.Substring(btn.Name.Length - 2)
        If Not IsNumeric(ns) Then ns = ns.Substring(1)
        Dim n = CInt(ns)

        DirectCast(Me.Controls("TxtPath" + n.ToString), TextBox).Text = ""
    End Sub

    'Always on top
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Me.TopMost = CheckBox1.Checked
    End Sub
End Class