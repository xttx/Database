Imports System.IO
Imports System.Reflection

Public Class Form0_Loader
    'Debug EXE size before adding no-image to resources - 752 КБ (770 560 байт)
    'Debug EXE size after adding no-image to resources  - 756 КБ (774 144 байт)
    Public Shared main_form As Form1

    Dim ini As New IniFileApi With {.path = Application.StartupPath + "\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini"}

    Dim hide_config_str As String = ""
    Dim added_controls As New List(Of Control)

    Private Sub Form0_Loader_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = ".\GeckoFX"
        'AddHandler AppDomain.CurrentDomain.AssemblyResolve, Function(o As Object, args As ResolveEventArgs) As Assembly
        '                                                        Dim name = args.Name.Substring(0, args.Name.IndexOf(","))
        '                                                        Dim path = IO.Path.Combine(Application.StartupPath, "GeckoFX\" + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll")
        '                                                        Return Assembly.LoadFrom(path)
        '                                                    End Function

        If Control.ModifierKeys = Keys.Shift Or Control.ModifierKeys = Keys.Control Then
            'Shift key pressed
            Show_Catalog_Selector()
        ElseIf My.Application.CommandLineArgs.Count > 0 Then
            'CLI
            Form1.ini.path = My.Application.CommandLineArgs(0) : Load_Main_Form()
        Else
            'Try autostart
            Dim cfg = ini.IniReadValue("StartUp", "StartConfig")
            If Not String.IsNullOrWhiteSpace(cfg) Then
                cfg = Application.StartupPath + "\" + cfg
                If File.Exists(cfg) Then
                    Form1.ini.path = cfg : Load_Main_Form()
                Else
                    MsgBox("Autostart config is set, but not found:" + vbCrLf + cfg) : Show_Catalog_Selector()
                End If
            Else
                Show_Catalog_Selector()
            End If
        End If
    End Sub

    Sub Show_Catalog_Selector()
        Me.FormBorderStyle = FormBorderStyle.Fixed3D
        Dim inis = Directory.GetFiles(Application.StartupPath, "*.ini").ToList()
        inis.Sort()

        'Dim ini As New IniFileApi With {.path = Application.StartupPath + "\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini"}
        Dim startup_cfg = ini.IniReadValue("StartUp", "StartConfig")
        hide_config_str = ini.IniReadValue("StartUp", "HideConfig")
        Dim hide_config_arr = hide_config_str.Split({","}, StringSplitOptions.RemoveEmptyEntries).Select(Of String)(Function(s) s.ToUpper)

        Dim x = 50
        Dim offset_top = 30
        For Each i In inis
            Dim ini_file = Path.GetFileName(i)
            If hide_config_arr.Contains(ini_file.ToUpper) Then Continue For
            If ini_file.ToUpper = (Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini").ToUpper Then Continue For

            Dim radio As New RadioButton With {.Text = ini_file, .AutoSize = True}
            radio.Location = New Point(x, offset_top)
            added_controls.Add(radio) : Panel1.Controls.Add(radio)

            Dim pb As New PictureBox With {.BorderStyle = BorderStyle.FixedSingle, .SizeMode = PictureBoxSizeMode.StretchImage}
            pb.Size = New Size(80, 45)
            pb.Location = New Point(x + 200, CInt(radio.Top + (radio.Height / 2) - (pb.Height / 2)))
            added_controls.Add(pb) : Panel1.Controls.Add(pb)

            'Hide button
            Dim hide_btn As New Button With {.Text = "X", .Tag = ini_file}
            hide_btn.Size = New Size(20, 20)
            hide_btn.Location = New Point(x - 30, CInt(radio.Top + (radio.Height / 2) - (hide_btn.Height / 2)))
            added_controls.Add(hide_btn) : Panel1.Controls.Add(hide_btn)
            AddHandler hide_btn.Click, AddressOf Hide_Config
            AddHandler hide_btn.MouseEnter, Sub() Custom_ToolTip.Show("Hide " + ini_file + " from this list.", Me, hide_btn)
            AddHandler hide_btn.MouseLeave, Sub() Custom_ToolTip.Hide()

            'Load image in picturebox
            Dim img_found = False
            For Each ext In {".jpg", ".png"}
                Dim fp = Application.StartupPath + "\images\" + Path.GetFileNameWithoutExtension(i) + ext
                If IO.File.Exists(fp) Then pb.Load(fp) : img_found = True : Exit For
            Next
            If Not img_found Then pb.Image = My.Resources.no_image_found_80x45

            offset_top += 50
            If offset_top > 400 Then x = 400 : offset_top = 30
        Next

        Dim startup_radio = Panel1.Controls.OfType(Of RadioButton).Where(Function(c) c.Text.Trim.ToUpper() = startup_cfg.Trim.ToUpper).FirstOrDefault()
        If startup_radio IsNot Nothing Then
            startup_radio.Checked = True
        Else
            Dim first_radio = Panel1.Controls.OfType(Of RadioButton).FirstOrDefault()
            If first_radio IsNot Nothing Then first_radio.Checked = True
        End If

        If x <= 50 Then Me.Height = offset_top + 150 Else Me.Height = 600
        Panel1.Visible = True
    End Sub

    Sub Hide_Config(o As Object, e As EventArgs)
        Dim b = DirectCast(o, Button)
        hide_config_str = String.Join(",", hide_config_str.Split({","}, StringSplitOptions.RemoveEmptyEntries).Append(b.Tag.ToString()))

        'Dim ini As New IniFileApi With {.path = Application.StartupPath + "\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini"}
        ini.IniWriteValue("StartUp", "HideConfig", hide_config_str)

        'Remove controls from panel
        For i As Integer = added_controls.Count - 1 To 0 Step -1
            Panel1.Controls.Remove(added_controls(i))
        Next
        added_controls.Clear()

        Show_Catalog_Selector()
    End Sub

    Sub Load_Main_Form()
        Dim f = IO.Path.GetFileNameWithoutExtension(Form1.ini.path)
        For Each ext In {".jpg", ".png"}
            Dim fp = Application.StartupPath + "\images\" + f + ext
            If IO.File.Exists(fp) Then PictureBox1.Load(fp) : Exit For
        Next

        main_form = New Form1(Me)
        AddHandler main_form.OnFinishedLoading, Sub()
                                                    main_form.Invoke(Sub() main_form.Show())
                                                    main_form.Invoke(Sub() Custom_ToolTip.message_filter.Init_Shared())
                                                    Me.Invoke(Sub() Me.Hide())
                                                End Sub
    End Sub

    Public Sub changeProgress(val As Integer, max As Integer, message As String)
        If ProgressBar1.Maximum <> max Then ProgressBar1.Maximum = max
        ProgressBar1.Value = val
        Label1.Text = message
        ProgressBar1.Refresh() : Label1.Refresh()
    End Sub

    'Start button
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim selected_radio = Panel1.Controls.OfType(Of RadioButton).Where(Function(c) c.Checked).FirstOrDefault
        If selected_radio IsNot Nothing Then
            If CheckBox1.Checked Then
                'Dim ini As New IniFileApi With {.path = Application.StartupPath + "\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini"}
                ini.IniWriteValue("StartUp", "StartConfig", selected_radio.Text.Trim)
            End If

            Panel1.Visible = False
            Me.Height = 450
            Me.FormBorderStyle = FormBorderStyle.None
            Form1.ini.path = Application.StartupPath + "\" + selected_radio.Text : Load_Main_Form()
        Else
            MsgBox("Nothing selected.")
        End If
    End Sub

    'Exit button
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Application.Exit()
    End Sub
End Class