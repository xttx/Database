Imports System.ComponentModel

Public Class FormA_MaintenanceDB_AskOverwrite
    Public last_resp As resp = resp.None
    Enum resp
        None
        UseSrc
        KeepDst
        KeepDstDeleteSrc
    End Enum

    Dim f_src As String = "", f_dst As String = ""
    Dim size1 As Integer = 0, size2 As Integer = 0
    Dim screen_path As String = ""

    Public Function ask(_f_src As String, _f_dst As String, Optional _screen_path As String = "") As resp
        f_src = _f_src : f_dst = _f_dst
        size1 = New IO.FileInfo(_f_src).Length
        size2 = New IO.FileInfo(_f_dst).Length
        screen_path = _screen_path
        If CheckBox1.Checked Then Return last_resp
        If CheckBox2.Checked AndAlso size1 = size2 Then Return last_resp
        Return resp.None
    End Function

    Private Sub FormA_MaintenanceDB_AskOverwrite_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If size1 = size2 Then Label6.Visible = True Else Label6.Visible = False
        Dim f1_to_show = f_src
        Dim f2_to_show = f_dst
        If screen_path.Trim <> "" AndAlso f_src.ToUpper.StartsWith(screen_path) Then
            f1_to_show = f1_to_show.Substring(screen_path)
            f2_to_show = f2_to_show.Substring(screen_path)
        End If
        If f1_to_show.StartsWith("\") Then f1_to_show = f1_to_show.Substring(1)
        If f2_to_show.StartsWith("\") Then f2_to_show = f2_to_show.Substring(1)

        Label2.Text = "Src: " + f1_to_show + " (" + size1.ToString + ")"
        Label3.Text = "Dst: " + f2_to_show + " (" + size2.ToString + ")"

        'PictureBox1.Load(f_src)
        'PictureBox2.Load(f_dst)
        'This is a hack to load an image and not lock the file
        Using img = New Bitmap(f_src)
            PictureBox1.Image = New Bitmap(img)
        End Using
        Using img = New Bitmap(f_dst)
            PictureBox2.Image = New Bitmap(img)
        End Using

        Label4.Text = "Src: " + PictureBox1.Image.Width.ToString + "x" + PictureBox1.Image.Height.ToString
        Label5.Text = "Src: " + PictureBox2.Image.Width.ToString + "x" + PictureBox2.Image.Height.ToString
    End Sub

    Private Sub ButtonOK_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click
        Dim b = DirectCast(sender, Button)

        If b Is Button1 Then last_resp = resp.UseSrc
        If b Is Button2 Then last_resp = resp.KeepDst
        If b Is Button3 Then last_resp = resp.KeepDstDeleteSrc
        Me.Close()
    End Sub

    Private Sub FormA_MaintenanceDB_AskOverwrite_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'If PictureBox1.Image IsNot Nothing Then PictureBox1.Image.Dispose() : PictureBox1.Image = Nothing
        'If PictureBox2.Image IsNot Nothing Then PictureBox2.Image.Dispose() : PictureBox2.Image = Nothing
        'GC.Collect()
        'GC.WaitForPendingFinalizers()
    End Sub
End Class