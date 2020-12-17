Imports Microsoft.VisualBasic.FileIO.FileSystem

Public Class Games_ExodosDosboxConfig
    Public exo_paths As New List(Of String)
    Private conf_list As New List(Of List(Of String))

    Private Sub Form6_ExodosDosboxConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        exo_paths.Add(Form1.ini.IniReadValue("Paths", "ExoDosPath"))
        exo_paths.Add(Form1.ini.IniReadValue("Paths", "ExoWinPath"))

        For Each exo_path As String In exo_paths
            Dim path As String = ""

            'TODO - This is wrong, need to check one by one
            If DirectoryExists(exo_path + "\Games\!dos") Then path = exo_path + "\Games\!dos"
            If DirectoryExists(exo_path + "\Games\!win3x") Then path = exo_path + "\Games\!win3x"

            conf_list.Add(New List(Of String))
            If path <> "" Then
                For Each d As String In GetDirectories(path)
                    If FileExists(d + "\dosbox.conf") Then conf_list(conf_list.Count - 1).Add(d + "\dosbox.conf")
                Next
                ComboBox1.Items.Add(exo_path + " (" + conf_list(conf_list.Count - 1).Count.ToString + " games)")
            End If
        Next
        If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim fullResList As New List(Of String)
        Dim winResList As New List(Of String)
        Dim outputList As New List(Of String)
        Dim scalerList As New List(Of String)
        Dim fullResCount() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim winResCount() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim outputCount() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim scalerCount() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        For Each i1 As String In ComboBox2.Items
            fullResList.Add(i1.ToUpper)
        Next
        For Each i2 As String In ComboBox3.Items
            winResList.Add(i2.ToUpper)
        Next
        For Each i3 As String In ComboBox4.Items
            outputList.Add(i3.ToUpper)
        Next
        For Each i4 As String In ComboBox5.Items
            scalerList.Add(i4.ToUpper)
        Next

        Dim i As Integer = 0
        For Each conf In conf_list(ComboBox1.SelectedIndex)
            Dim ini As New IniFileApi() With {.path = conf}

            Dim fullRes As String = ini.IniReadValue("sdl", "fullresolution")
            Dim winRes As String = ini.IniReadValue("sdl", "windowresolution")
            Dim output As String = ini.IniReadValue("sdl", "output")
            Dim scaler As String = ini.IniReadValue("render", "scaler")

            i = fullResList.IndexOf(fullRes.ToUpper)
            If i = -1 Then i = 29
            fullResCount(i) = fullResCount(i) + 1
            i = winResList.IndexOf(winRes.ToUpper)
            If i = -1 Then i = 29
            winResCount(i) = winResCount(i) + 1
            i = outputList.IndexOf(output.ToUpper)
            If i = -1 Then i = 29
            outputCount(i) = outputCount(i) + 1
            i = scalerList.IndexOf(scaler.ToUpper)
            If i = -1 Then i = 29
            scalerCount(i) = scalerCount(i) + 1
        Next

        Dim str As String = ""
        For i = 0 To fullResCount.Count - 1
            If fullResCount(i) <> 0 Then
                If i <> 29 Then
                    str = str + fullResCount(i).ToString + " " + ComboBox2.Items(i).ToString + ", "
                Else
                    str = str + fullResCount(i).ToString + " unknown, "
                End If
            End If
        Next
        Label1.Text = "Full Resolution: " + str.Substring(0, str.Length - 2)

        str = ""
        For i = 0 To winResCount.Count - 1
            If winResCount(i) <> 0 Then
                If i <> 29 Then
                    str = str + winResCount(i).ToString + " " + ComboBox3.Items(i).ToString + ", "
                Else
                    str = str + winResCount(i).ToString + " unknown, "
                End If
            End If
        Next
        Label2.Text = "Window Resolution: " + str.Substring(0, str.Length - 2)

        str = ""
        For i = 0 To outputCount.Count - 1
            If outputCount(i) <> 0 Then
                If i <> 29 Then
                    str = str + outputCount(i).ToString + " " + ComboBox4.Items(i).ToString + ", "
                Else
                    str = str + outputCount(i).ToString + " unknown, "
                End If
            End If
        Next
        Label3.Text = "Output: " + str.Substring(0, str.Length - 2)

        str = ""
        For i = 0 To scalerCount.Count - 1
            If scalerCount(i) <> 0 Then
                If i <> 29 Then
                    str = str + scalerCount(i).ToString + " " + ComboBox5.Items(i).ToString + ", "
                Else
                    str = str + scalerCount(i).ToString + " unknown, "
                End If
            End If
        Next
        Label4.Text = "Scaler: " + str.Substring(0, str.Length - 2)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ComboBox2.SelectedIndex = -1
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ComboBox3.SelectedIndex = -1
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ComboBox4.SelectedIndex = -1
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ComboBox5.SelectedIndex = -1
    End Sub

    'SET button
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        For Each conf In conf_list(ComboBox1.SelectedIndex)
            Dim ini As New IniFileApi() With {.path = conf}
            If ComboBox2.SelectedIndex <> -1 Then ini.IniWriteValue("sdl", "fullresolution", ComboBox2.SelectedItem.ToString)
            If ComboBox3.SelectedIndex <> -1 Then ini.IniWriteValue("sdl", "windowresolution", ComboBox3.SelectedItem.ToString)
            If ComboBox4.SelectedIndex <> -1 Then ini.IniWriteValue("sdl", "output", ComboBox4.SelectedItem.ToString)
            If ComboBox5.SelectedIndex <> -1 Then ini.IniWriteValue("render", "scaler", ComboBox5.SelectedItem.ToString)
        Next
        MsgBox("Done.")
    End Sub
End Class