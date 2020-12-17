Public Class FormA_CategoryEditor
    Dim F1 As Form1 = Application.OpenForms.OfType(Of Form1).First()

    Private Sub FormA_CategoryEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = ""
        Dim t1_old = ""
        Dim t2_old = ""
        TreeView1.BeginUpdate() : TreeView2.BeginUpdate()
        If TreeView1.SelectedNode IsNot Nothing Then t1_old = F1.getNodeCategoryPath(TreeView1.SelectedNode)
        If TreeView2.SelectedNode IsNot Nothing Then t2_old = F1.getNodeCategoryPath(TreeView2.SelectedNode)
        Dim tex1_old = TreeView1.Nodes.GetAllNodesRecur2.Where(Function(n) n.IsExpanded).Select(Of String)(Function(n) F1.getNodeCategoryPath(n)).ToArray()
        Dim tex2_old = TreeView2.Nodes.GetAllNodesRecur2.Where(Function(n) n.IsExpanded).Select(Of String)(Function(n) F1.getNodeCategoryPath(n)).ToArray()

        TreeView1.Nodes.Clear() : TreeView2.Nodes.Clear()

        Dim nodes = F1.init_categories_set()
        TreeView1.Nodes.AddRange(nodes.Item1)
        TreeView2.Nodes.AddRange(nodes.Item1.Select(Of TreeNode)(Function(n) n.Clone()).ToArray)

        'Restore last selected node and expanded state
        If Not String.IsNullOrWhiteSpace(t1_old) OrElse tex1_old.Count > 0 Then
            For Each n In TreeView1.Nodes.GetAllNodesRecur2
                If Not String.IsNullOrWhiteSpace(t1_old) AndAlso F1.getNodeCategoryPath(n) = t1_old Then TreeView1.SelectedNode = n
                If tex1_old.Contains(F1.getNodeCategoryPath(n)) Then n.Expand()
            Next
        End If
        If Not String.IsNullOrWhiteSpace(t2_old) OrElse tex2_old.Count > 0 Then
            For Each n In TreeView2.Nodes.GetAllNodesRecur2
                If Not String.IsNullOrWhiteSpace(t2_old) AndAlso F1.getNodeCategoryPath(n) = t2_old Then TreeView2.SelectedNode = n
                If tex2_old.Contains(F1.getNodeCategoryPath(n)) Then n.Expand()
            Next
        End If
        TreeView1.EndUpdate() : TreeView2.EndUpdate()
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        If e.Node Is Nothing Then TextBox1.Text = "" : Exit Sub

        If CheckBox1.Checked Then
            TextBox1.Text = F1.getNodeCategoryPath(e.Node)
        Else
            TextBox1.Text = e.Node.Name.Trim
        End If
    End Sub

    'Rename
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If TreeView1.SelectedNode Is Nothing Then MsgBox("Select a categorie to rename in the left treeview.") : Exit Sub
        If TextBox1.Text.Trim = "" Then MsgBox("Enter the new categorie name or path.") : Exit Sub

        'Dim path_orig = F1.getNodeCategoryPath(TreeView1.SelectedNode).Trim.Replace("\", "/")
        'Dim path_new = ""
        'If CheckBox1.Checked Then
        '    'Full path rename
        '    path_new = TextBox1.Text.Trim.Replace("\", "/")
        'Else
        '    'Sub category rename
        '    If TextBox1.Text.Contains("\") Or TextBox1.Text.Contains("/") Then MsgBox("Categorie name should not contain slashes.") : Exit Sub
        '    If path_orig.Contains("/") Then
        '        path_new = IO.Path.GetDirectoryName(path_orig).Replace("\", "/") + "/" + TextBox1.Text.Trim
        '    Else
        '        path_new = TextBox1.Text.Trim
        '    End If
        'End If

        'Dim sql = "UPDATE category SET cat = '" + path_new + "' WHERE cat = '" + path_orig + "' COLLATE NOCASE"
        'db.execute(sql)
        'Dim rows = db.affectedRows
        'sql = "UPDATE category SET cat = REPLACE(cat, '" + path_orig + "/', '" + path_new + "/') WHERE cat LIKE '" + path_orig + "/%'"
        'db.execute(sql)
        'Dim rows_sub = db.affectedRows
        Dim res = F1.Edit_Category_Rename(TreeView1.SelectedNode, TextBox1.Text, CheckBox1.Checked)
        If res.err <> "" Then MsgBox(res.err) : Exit Sub

        FormA_CategoryEditor_Load(Me, New EventArgs())
        TextBox2.AppendText("Successfully renamed: '" + res.path_orig + "' to '" + res.path_new + "'. " + vbCrLf + "--- " + (res.rows + res.rows_sub).ToString + " products updated." + vbCrLf)
    End Sub
    'Move categorie to the new root
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TreeView1.SelectedNode Is Nothing Then MsgBox("Select a categorie to rename in the left treeview.") : Exit Sub
        If TreeView2.SelectedNode Is Nothing Then MsgBox("Select a categorie to move to in the right treeview.") : Exit Sub

        Dim path_orig = F1.getNodeCategoryPath(TreeView1.SelectedNode).Trim.Replace("\", "/")
        Dim path_new = F1.getNodeCategoryPath(TreeView2.SelectedNode).Trim.Replace("\", "/")
        path_new += "/" + IO.Path.GetFileName(path_orig)

        Dim sql = "UPDATE category SET cat = '" + path_new + "' WHERE cat = '" + path_orig + "' COLLATE NOCASE"
        db.execute(sql)
        Dim rows = db.affectedRows
        sql = "UPDATE category SET cat = REPLACE(cat, '" + path_orig + "/', '" + path_new + "/') WHERE cat LIKE '" + path_orig + "/%'"
        db.execute(sql)
        Dim rows_sub = db.affectedRows

        FormA_CategoryEditor_Load(Me, New EventArgs())
        TextBox2.AppendText("Successfully moved: '" + path_orig + "' to '" + path_new + "'. " + vbCrLf + "--- " + (rows + rows_sub).ToString + " products updated." + vbCrLf)
    End Sub
    'Move products from categorie to the new categorie
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If TreeView1.SelectedNode Is Nothing Then MsgBox("Select a categorie to rename in the left treeview.") : Exit Sub
        If TreeView2.SelectedNode Is Nothing Then MsgBox("Select a categorie to move to in the right treeview.") : Exit Sub

        Dim path_orig = F1.getNodeCategoryPath(TreeView1.SelectedNode).Trim.Replace("\", "/")
        Dim path_new = F1.getNodeCategoryPath(TreeView2.SelectedNode).Trim.Replace("\", "/")

        Dim sql = "UPDATE category SET cat = '" + path_new + "' WHERE cat = '" + path_orig + "' COLLATE NOCASE"
        db.execute(sql)
        Dim rows = db.affectedRows
        sql = "UPDATE category SET cat = REPLACE(cat, '" + path_orig + "/', '" + path_new + "/') WHERE cat LIKE '" + path_orig + "/%'"
        db.execute(sql)
        Dim rows_sub = db.affectedRows

        FormA_CategoryEditor_Load(Me, New EventArgs())
        TextBox2.AppendText("Successfully moved from: '" + path_orig + "' to '" + path_new + "'. " + vbCrLf + "--- " + (rows + rows_sub).ToString + " products updated." + vbCrLf)
    End Sub

    'Private Function Get_Categorie_Path(node As TreeNode) As String
    '    Dim n = node.Name
    '    Dim p = node.Parent
    '    While p IsNot Nothing
    '        n = p.Name + "/" + n : p = p.Parent
    '    End While
    '    Return n
    'End Function

    'Show Full Path
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        TreeView1_AfterSelect(TreeView1, New TreeViewEventArgs(TreeView1.SelectedNode))
    End Sub
End Class