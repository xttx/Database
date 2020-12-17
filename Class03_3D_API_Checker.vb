Public Class Class03_3D_API_Checker
    Shared WithEvents p As Process

    Public Shared Sub check(exe As String)
        Dim dw_dir = IO.Path.GetDirectoryName(path_dw)
        If IO.File.Exists(dw_dir + "\out.txt") Then System.IO.File.Delete(dw_dir + "\out.txt")

        Dim psi As New ProcessStartInfo
        psi.FileName = path_dw
        psi.WorkingDirectory = dw_dir
        psi.Arguments = "/c /of:out.txt """ + exe + """"
        psi.CreateNoWindow = True
        psi.UseShellExecute = False
        p = Process.Start(psi)
        'AddHandler p.Exited, AddressOf dw_finished

        Dim bg As New System.ComponentModel.BackgroundWorker
        AddHandler bg.DoWork, AddressOf wait_for_dw_to_finish
        bg.RunWorkerAsync()
    End Sub
    Shared Sub wait_for_dw_to_finish()
        Do While Not p.HasExited
            Threading.Thread.Sleep(100)
        Loop
        dw_finished()
    End Sub

    Shared Sub dw_finished()
        Dim out_path = IO.Path.GetDirectoryName(path_dw) + "\out.txt"
        If Not IO.File.Exists(out_path) Then MsgBox("Dependency walker did not create an output file. Aborting.") : Exit Sub

        Dim s = ""
        Dim current_level = 0
        Dim current_dll = ""
        Dim current_section = ""

        Dim glide = False, glide2 = False, glide3 = False, opengl = False, d3d = False, d3d8 = False, d3d9 = False, ddraw = False
        Dim glide_l = 0, glide2_l = 0, glide3_l = 0, opengl_l = 0, d3d8_l = 0, d3d9_l = 0, ddraw_l = 0
        Using sr As IO.StreamReader = IO.File.OpenText(out_path)
            Do
                s = sr.ReadLine
                If s Is Nothing Then Exit Do

                Dim s_trim = s.Trim
                If s_trim.ToUpper.EndsWith(".DLL") Then
                    current_dll = s_trim.Substring(s_trim.LastIndexOf(" ")).Trim.ToUpper
                    Dim space_n = s.Length - s_trim.Length
                    If space_n > 0 Then
                        current_level = CInt(space_n / 5)
                    Else
                        current_level = 0
                    End If

                    If current_dll = "GLIDE.DLL" Then glide = True : glide_l = current_level
                    If current_dll = "GLIDE2X.DLL" Then glide2 = True : glide2_l = current_level
                    If current_dll = "GLIDE3X.DLL" Then glide3 = True : glide3_l = current_level
                    If current_dll = "OPENGL32.DLL" Then opengl = True : opengl_l = current_level
                    If current_dll = "D3D8.DLL" Then d3d8 = True : d3d8_l = current_level
                    If current_dll = "D3D9.DLL" Then d3d9 = True : d3d9_l = current_level
                    If current_dll = "DDRAW.DLL" Then ddraw = True : ddraw_l = current_level
                End If

                If current_dll = "DDRAW.DLL" Then
                    If s_trim.ToUpper.StartsWith("IMPORT") Then current_section = "I"
                    If s_trim.ToUpper.StartsWith("EXPORT") Then current_section = "E"

                    If s_trim.ToUpper.Contains("DirectDrawEnumerateA".ToUpper) Then
                        If current_section = "I" Then
                            d3d = True
                        End If
                    End If
                End If
            Loop
        End Using

        Dim message = "Found API" + vbCrLf
        If ddraw Then message += "- DDraw (2D) L: " + ddraw_l.ToString + vbCrLf

        If d3d Then message += "- D3D v1-7" + vbCrLf
        If d3d8 Then message += "- D3D v8 L: " + d3d8_l.ToString + vbCrLf
        If d3d9 Then message += "- D3D v9 L: " + d3d9_l.ToString + vbCrLf

        If glide Then message += "- Glide L: " + glide_l.ToString + vbCrLf
        If glide2 Then message += "- Glide2x L: " + glide2_l.ToString + vbCrLf
        If glide3 Then message += "- Glide3x L: " + glide3_l.ToString + vbCrLf
        If opengl Then message += "- OpenGL L: " + opengl_l.ToString + vbCrLf

        'Form1.Invoke(Sub() MsgBox(message))
        MsgBox(message)
    End Sub
End Class
