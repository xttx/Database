Imports System.IO
Imports System.Text.RegularExpressions

Public Class Games_ExodosDosboxSingle
    Public was_saved As Boolean = False


    Dim refr As Boolean = False
    Dim conf As String = ""
    Dim initial_values As New Dictionary(Of Control, String)
    Dim ini As New IniFileApi()

    Dim sound_fonts As New List(Of String)

    Const MOUNT_ULTRASOUND_DIR_REMARK As String = "@REM Mount GUS Ultrasound driver dir"

    Public Sub New(conf_file As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Dim conf_custom = Path.GetDirectoryName(conf_file) + "\" + Path.GetFileNameWithoutExtension(conf_file) + "_custom" + Path.GetExtension(conf_file)
        If File.Exists(conf_custom) Then conf = conf_custom Else conf = conf_file
        ini.path = conf

        Dim gus_path_arr = Form1.ini.IniReadValue("Paths", "DosboxGus").Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        For Each gus In gus_path_arr
            If gus.Trim <> "" AndAlso ComboBox24.Items.Cast(Of String).Where(Function(s) s.ToUpper = gus.ToUpper).Count = 0 Then ComboBox24.Items.Add(gus)
        Next
    End Sub

    Private Sub Games_ExodosDosboxSingle_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Custom_ToolTip.Setup(CheckBox11, "Dosbox ECE builds", Me)

        refr = True
        Dim dir = IO.Path.GetDirectoryName(conf)
        Dim dir_dosbox = dir + "\..\..\..\dosbox"
        Dim dir_shaders = dir + "\..\..\..\dosbox\Shaders"
        If FileIO.FileSystem.DirectoryExists(dir_shaders) Then
            Dim shaders = FileIO.FileSystem.GetFiles(dir_shaders, FileIO.SearchOption.SearchTopLevelOnly, {"*.fx"})
            For Each shader In shaders
                ComboBox5.Items.Add(IO.Path.GetFileNameWithoutExtension(shader))
            Next
        End If

        If FileIO.FileSystem.DirectoryExists(dir_dosbox + "\..") Then
            Dim sf = FileIO.FileSystem.GetFiles(dir_dosbox + "\..", FileIO.SearchOption.SearchTopLevelOnly, {"*.sf2"})
            For Each s In sf
                sound_fonts.Add(IO.Path.GetFileName(s))
                ComboBox26.Items.Add(IO.Path.GetFileName(s))
            Next
        End If

        setValue(CheckBox1, ini.IniReadValue("sdl", "fullscreen"))
        setValue(ComboBox1, ini.IniReadValue("sdl", "fullresolution"))
        setValue(ComboBox2, ini.IniReadValue("sdl", "windowresolution"))
        setValue(ComboBox3, ini.IniReadValue("sdl", "output"))
        setValue(CheckBox2, ini.IniReadValue("render", "aspect"))
        setValue(ComboBox4, ini.IniReadValue("render", "scaler"))
        setValue(ComboBox5, ini.IniReadValue("sdl", "pixelshader"))

        setValue(ComboBox6, ini.IniReadValue("dosbox", "machine"))
        setValue(ComboBox7, ini.IniReadValue("dosbox", "memsize"))
        Dim vmem = ini.IniReadValue("dosbox", "vmemsize")
        If IsNumeric(vmem) Then
            Dim val = ComboBox8.Items(CInt(vmem))
            setValue(ComboBox8, val)
        Else
            setValue(ComboBox8, "")
        End If

        setValue(ComboBox9, ini.IniReadValue("cpu", "core"))
        setValue(ComboBox10, ini.IniReadValue("cpu", "cputype"))
        setValue(ComboBox11, ini.IniReadValue("cpu", "cycles"))
        setValue(ComboBox12, ini.IniReadValue("cpu", "cycleup"))
        setValue(ComboBox13, ini.IniReadValue("cpu", "cycledown"))

        setValue(ComboBox14, ini.IniReadValue("glide", "glide"))
        setValue(CheckBox3, ini.IniReadValue("glide", "splash"))
        setValue(ComboBox15, ini.IniReadValue("pci", "voodoo"))

        setValue(ComboBox16, ini.IniReadValue("sblaster", "sbtype"))
        setValue(TextBox2, ini.IniReadValue("sblaster", "sbbase"))
        setValue(TextBox3, ini.IniReadValue("sblaster", "irq"))
        setValue(TextBox4, ini.IniReadValue("sblaster", "dma"))
        setValue(ComboBox17, ini.IniReadValue("sblaster", "oplemu"))

        setValue(CheckBox8, ini.IniReadValue("gus", "gus"))
        setValue(TextBox7, ini.IniReadValue("gus", "ultradir"))
        setValue(CheckBox7, ini.IniReadValue("innova", "innova"))
        setValue(ComboBox22, ini.IniReadValue("innova", "samplerate"))
        setValue(ComboBox23, ini.IniReadValue("innova", "quality"))

        setValue(CheckBox9, ini.IniReadValue("speaker", "tandy"))
        setValue(CheckBox10, ini.IniReadValue("speaker", "disney"))

        setValue(ComboBox18, ini.IniReadValue("midi", "mididevice"))
        setValue(ComboBox19, ini.IniReadValue("midi", "midiconfig"))
        Dim dac = ini.IniReadValue("midi", "mt32.dac")
        If IsNumeric(dac) Then
            Dim val = ComboBox20.Items(CInt(dac))
            setValue(ComboBox20, val)
        Else
            setValue(ComboBox20, "")
        End If
        setValue(TextBox5, ini.IniReadValue("midi", "mt32.partials"))
        setValue(ComboBox26, ini.IniReadValue("midi", "fluid.soundfont"))
        If ComboBox26.SelectedIndex >= 1 Then CheckBox11.Checked = True

        setValue(CheckBox4, ini.IniReadValue("dos", "xms"))
        setValue(CheckBox5, ini.IniReadValue("dos", "ems"))
        setValue(CheckBox6, ini.IniReadValue("dos", "umb"))

        'Check GUS mount drive and folder
        Dim ini_content = IO.File.ReadAllText(ini.path)
        Dim ind = ini_content.IndexOf(MOUNT_ULTRASOUND_DIR_REMARK.ToUpper, StringComparison.OrdinalIgnoreCase)
        If ind >= 0 Then
            'Guest drive
            ind = ini_content.IndexOf("mount", ind + MOUNT_ULTRASOUND_DIR_REMARK.Length, StringComparison.OrdinalIgnoreCase)
            Dim gus_mount_drive = ini_content.Substring(ind + 5).Trim.Substring(0, 1).ToUpper

            For Each item As String In ComboBox21.Items
                If item = "" Then Continue For
                If item.Substring(0, 1).ToUpper = gus_mount_drive Then ComboBox21.SelectedItem = item : Exit For
            Next
            If ComboBox21.SelectedIndex < 0 Then
                ComboBox21.Items.Add(gus_mount_drive + "\:")
                ComboBox21.SelectedIndex = ComboBox21.Items.Count - 1
            End If

            'Host folder
            ind = ini_content.IndexOf(" ", ind) + 1
            ind = ini_content.IndexOf(" ", ind) + 1
            Dim ind2 = ini_content.IndexOf(vbCr, ind)
            Dim ind2b = ini_content.IndexOf(vbCr, ind)
            If ind2b < ind2 Then ind2 = ind2b
            Dim gus_mount_host_folder = ini_content.Substring(ind, ind2b - ind).Trim().Trim({""""c})
            For Each item As String In ComboBox24.Items
                If item.ToUpper = gus_mount_host_folder.ToUpper Then ComboBox24.SelectedItem = item : Exit For
            Next
            If ComboBox24.SelectedIndex < 0 Then
                ComboBox24.Items.Add(gus_mount_host_folder)
                ComboBox24.SelectedIndex = ComboBox24.Items.Count - 1
            End If
        End If
        refr = False
    End Sub
    Sub setValue(c As Control, v As String)
        initial_values.Add(c, v)
        If TypeOf c Is ComboBox Then
            Dim cc = DirectCast(c, ComboBox)
            If Not cc.Items.Contains(v) Then cc.Items.Add(v)
            cc.SelectedItem = v
        ElseIf TypeOf c Is CheckBox Then
            Dim cc = DirectCast(c, CheckBox)
            If v.ToUpper = "TRUE" Or v = "1" Or v.ToUpper = "ON" Then cc.Checked = True Else cc.Checked = False
            If v.ToUpper = "AUTO" Then cc.CheckState = CheckState.Indeterminate
        ElseIf TypeOf c Is TextBox Then
            DirectCast(c, TextBox).Text = v
        End If
    End Sub

    'Save
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim gus_paths = String.Join(";", ComboBox24.Items.Cast(Of String).Select(Of String)(Function(s) s.Trim()).Append(ComboBox24.Text.Trim()).Distinct(StringComparer.InvariantCultureIgnoreCase))
        Form1.ini.IniWriteValue("Paths", "DosboxGus", gus_paths)

        Dim file = Path.GetFileNameWithoutExtension(conf)
        If Not file.ToUpper().EndsWith("_CUSTOM") Then
            Dim new_conf = Path.GetDirectoryName(conf) + "\" + Path.GetFileNameWithoutExtension(conf) + "_custom" + Path.GetExtension(conf)
            If IO.File.Exists(new_conf) Then MsgBox("Save Error - trying to create config_custom, but it's already exist.") : Exit Sub
            IO.File.Copy(conf, new_conf)
            ini.path = new_conf
        End If

        saveValue(CheckBox1, "sdl", "fullscreen")
        saveValue(ComboBox1, "sdl", "fullresolution")
        saveValue(ComboBox2, "sdl", "windowresolution")
        saveValue(ComboBox3, "sdl", "output")
        saveValue(CheckBox2, "render", "aspect")
        saveValue(ComboBox4, "render", "scaler")
        saveValue(ComboBox5, "sdl", "pixelshader")

        saveValue(ComboBox6, "dosbox", "machine")
        saveValue(ComboBox7, "dosbox", "memsize")
        saveValue(ComboBox8, "dosbox", "vmemsize", True)

        saveValue(ComboBox9, "cpu", "core")
        saveValue(ComboBox10, "cpu", "cputype")
        saveValue(ComboBox11, "cpu", "cycles")
        saveValue(ComboBox12, "cpu", "cycleup")
        saveValue(ComboBox13, "cpu", "cycledown")

        saveValue(ComboBox14, "glide", "glide")
        saveValue(CheckBox3, "glide", "splash")
        saveValue(ComboBox15, "pci", "voodoo")

        saveValue(ComboBox16, "sblaster", "sbtype")
        saveValue(TextBox2, "sblaster", "sbbase")
        saveValue(TextBox3, "sblaster", "irq")
        saveValue(TextBox4, "sblaster", "dma")
        saveValue(ComboBox17, "sblaster", "oplemu")

        'Assure we have [innova] section added before [autoexec]
        Dim ini_content = IO.File.ReadAllText(ini.path)
        If (CheckBox7.Checked AndAlso ini.IniListKey("innova").Count = 0) Then
            ini_content = Replace(ini_content, "[AUTOEXEC]", "[innova]" + vbCrLf + "innova=true" + vbCrLf + "[autoexec]", , , CompareMethod.Text)
            IO.File.WriteAllText(ini.path, ini_content, New System.Text.UTF8Encoding(False))
        End If
        saveValue(CheckBox7, "innova", "innova")
        saveValue(ComboBox22, "innova", "samplerate")
        saveValue(ComboBox23, "innova", "quality")

        saveValue(CheckBox8, "gus", "gus")
        saveValue(TextBox7, "gus", "ultradir")
        'Add mount command to mount ultrasound folder
        If ComboBox21.SelectedIndex >= 0 AndAlso ComboBox21.SelectedItem.ToString.Trim.Length >= 3 AndAlso ComboBox24.Text.Trim() <> "" Then
            Dim drive = ComboBox21.SelectedItem.ToString.Trim.Substring(0, 1)
            Dim host_dir = ComboBox24.Text.Trim()
            If Not host_dir.StartsWith("""") Then host_dir = """" + host_dir + """"

            Dim ind = ini_content.IndexOf(MOUNT_ULTRASOUND_DIR_REMARK.ToUpper, StringComparison.OrdinalIgnoreCase)
            If ind < 0 Then
                ini_content = Replace(ini_content, "[AUTOEXEC]", "[autoexec]" + vbCrLf + MOUNT_ULTRASOUND_DIR_REMARK + vbCrLf + "mount " + drive + " " + host_dir + vbCrLf, 1, 1, CompareMethod.Text)
            Else
                'Dim r As New Regex("(\[autoexec\][\s\S]*?)(\n|\r)(@?)(mount)", RegexOptions.IgnoreCase)
                'ini_content = r.Replace(ini_content, "$1$2$3mount " + drive + " " + host_dir + vbCrLf + "mount", 1)
                ind = ini_content.IndexOf("mount", ind + MOUNT_ULTRASOUND_DIR_REMARK.Length, StringComparison.OrdinalIgnoreCase)
                Dim ind2 = ini_content.IndexOf(vbCr, ind)
                Dim ind2b = ini_content.IndexOf(vbLf, ind)
                If ind2b < ind2 Then ind2 = ind2b
                ini_content = ini_content.Substring(0, ind) + "mount " + drive + " " + host_dir + ini_content.Substring(ind2)
            End If

            IO.File.WriteAllText(ini.path, ini_content, New System.Text.UTF8Encoding(False))
        Else
            'Empty GUS guest drive or host path. Remove gus mount, if it exist
            Dim ind = ini_content.IndexOf(MOUNT_ULTRASOUND_DIR_REMARK.ToUpper, StringComparison.OrdinalIgnoreCase)
            If ind >= 0 Then
                Dim ind2 = ini_content.IndexOf(vbLf, ind) + 1
                ind2 = ini_content.IndexOf(vbLf, ind2) + 1
                ind2 = ini_content.IndexOf(vbLf, ind2) + 1
                ini_content = ini_content.Substring(0, ind) + ini_content.Substring(ind2)
                IO.File.WriteAllText(ini.path, ini_content, New System.Text.UTF8Encoding(False))
            End If
        End If

        saveValue(CheckBox9, "speaker", "tandy")
        saveValue(CheckBox10, "speaker", "disney")

        saveValue(ComboBox18, "midi", "mididevice")
        saveValue(ComboBox19, "midi", "midiconfig")
        saveValue(ComboBox20, "midi", "mt32.dac", True)

        saveValue(TextBox5, "midi", "mt32.partials")

        saveValue(CheckBox4, "dos", "xms")
        saveValue(CheckBox5, "dos", "ems")
        saveValue(CheckBox6, "dos", "umb")

        was_saved = True : Me.Close()
    End Sub
    Private Sub saveValue(c As Control, section As String, key As String, Optional saveIndex As Boolean = False)
        If TypeOf c Is ComboBox Then
            Dim cc = DirectCast(c, ComboBox)
            If initial_values(c).Trim = "" And cc.SelectedItem.ToString.Trim = "" Then Exit Sub

            Dim val As String = ""
            If saveIndex Then
                val = cc.SelectedIndex.ToString
            Else
                val = cc.SelectedItem.ToString
            End If
            If String.IsNullOrWhiteSpace(val) Then val = Nothing
            ini.IniWriteValue(section, key, val)
        ElseIf TypeOf c Is CheckBox Then
            Dim cc = DirectCast(c, CheckBox)
            If initial_values(c).Trim = "" And Not cc.Checked Then Exit Sub
            If cc IsNot CheckBox9 Then
                ini.IniWriteValue(section, key, cc.Checked.ToString.ToLower)
            Else
                If cc.CheckState = CheckState.Unchecked Then
                    ini.IniWriteValue(section, key, "off")
                ElseIf cc.CheckState = CheckState.Indeterminate Then
                    ini.IniWriteValue(section, key, "auto")
                ElseIf cc.CheckState = CheckState.Checked Then
                    ini.IniWriteValue(section, key, "on")
                End If
            End If
        ElseIf TypeOf c Is TextBox Then
            Dim val As String = DirectCast(c, TextBox).Text.Trim
            If initial_values(c).Trim.ToLower = val.ToLower Then Exit Sub
            If String.IsNullOrWhiteSpace(val) Then val = Nothing
            ini.IniWriteValue(section, key, val)
        End If
    End Sub

    'Set midi device - reset device config (list sound fonts for fluidSynth and list numerics values for other devices, show tooltip for selected device)
    Private Sub ComboBox18_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox18.SelectedIndexChanged
        ComboBox19.Items.Clear()
        ComboBox19.Items.Add("")

        Dim cur = ComboBox18.SelectedItem.ToString.Trim.ToLower
        If cur = "synth" Then
            ComboBox19.Items.AddRange(sound_fonts.ToArray)
            ComboBox19.SelectedIndex = 0
        Else
            ComboBox19.Items.AddRange({"0", "1", "2"})
            ComboBox19.SelectedIndex = 1
        End If

        If cur = "synth" Or cur = "mt32" Then
            Label32.Text = "* To use MT32 or soundfonts with exodos, MT32 roms and soundfonts should be placed in dosbox PARENT directory (exoDos root), not in dosbox dir."
        ElseIf cur = "default" Or cur = "win32" Then
            Label32.Text = "* Config field is the midi device ID. To get this id, run following command INSIDE DosBox: 'MIXER /LISTMIDI'"
        ElseIf cur = "alsa" Then
            Label32.Text = "* Linux's Advanced Linux Sound Architecture playback interface"
        ElseIf cur = "oss" Then
            Label32.Text = "* Linux's Open Sound System playback interface"
        ElseIf cur = "coreaudio" Then
            Label32.Text = "* MacOS X's framework to render the music through the built-in OS X synthesizer."
        ElseIf cur = "coremidi" Then
            Label32.Text = "* MacOS X's framework to route MIDI commands to any device that has been configured in Audio MIDI Setup."
        Else
            Label32.Text = ""
        End If

    End Sub

    'Set ECE build (use fluid.soundfont setting instead of midiconfig)
    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged
        Label39.Enabled = CheckBox11.Checked
        ComboBox26.Enabled = CheckBox11.Checked
        If CheckBox11.Checked Then ComboBox18.Items(8) = "fluidsynth" Else ComboBox18.Items(8) = "synth"
    End Sub

    'Change guest ultrasnd drive
    Private Sub ComboBox21_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox21.SelectedIndexChanged
        If refr Then Exit Sub
        If ComboBox21.SelectedIndex < 0 Then Exit Sub
        Dim p = TextBox7.Text.Trim
        Dim d = ComboBox21.SelectedItem.ToString.Trim
        If d.Length >= 3 AndAlso Char.IsLetter(d(0)) AndAlso p.Length >= 3 AndAlso Char.IsLetter(p(0)) Then
            If d.Substring(1, 2) = ":\" AndAlso p.Substring(1, 2) = ":\" Then TextBox7.Text = d(0) + p.Substring(1)
        End If
    End Sub

End Class