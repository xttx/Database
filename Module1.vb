Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Security.Permissions
Imports Gecko
Imports Gecko.WebIDL

Public Module Module1
    Enum catalog_type
        daz
        games
        unity
        generic
    End Enum

    Public db As New Class01_db
    Public path_db As String = Application.StartupPath + "\db\main.db"
    Public path_dw As String = "D:\Installs\Programming\Dependency Walker 2.2_x86\depends.exe"
    Public screenExtensionsArray As String() = {"jpg", "jpeg", "png", "gif"}
    Public Const LOG_FILE_PATH = ".\Catalog 2016.log"
    Public Const LOG_FILE_PATH_FILES = ".\Catalog 2016.logFileOps.log"

    'Field_txt should always be last
    Public fieldTypeArr() As String = {"Field_str", "Field_num", "Field_dec", "Field_bool", "Field_txt"}
    Public fieldCountArr() As Integer = Nothing

    Public Fields As New List(Of Field_Info)
    Public Fields_ordered As New List(Of Field_Info)
    Class Field_Info
        Public enabled As Boolean = False
        Public name As String = ""
        Public DBname As String = ""
        Public index_of_type As Integer = 0
        Public index_of_total As Integer = 0
        Public writable As Boolean = False
        Public sortable As Boolean = False
        Public filtrable As Boolean = False
        Public is_list As Boolean = False
        Public is_nameLink As Boolean = False
        Public list_values As String() = {}
        Public field_type As field_types = field_types.none

        Public assoc_lbl As Label = Nothing
        Public assoc_txt As TextBox = Nothing
        Public assoc_chk As CheckBox = Nothing
        Public assoc_cmb As ComboBox = Nothing
        Public Enum field_types
            none
            str
            num
            dec
            bool
            txt
        End Enum
    End Class

    Sub log(message As String, Optional modul As String = "", Optional file As String = "")
        If String.IsNullOrWhiteSpace(file) Then file = LOG_FILE_PATH

        If modul <> "" Then modul = " (" + modul + ")"
        Dim f = My.Computer.FileSystem.OpenTextFileWriter(file, True)
        f.WriteLine(DateTime.Now.ToString + modul + " " + message)
        f.Close()
    End Sub
    Function MsgBoxEx(text As String, buttons As String, Optional title As String = Nothing, Optional check_text As String = Nothing) As MsgBox_Custom.DialogResult2
        Return MsgBox_Custom.Show(text, buttons, title, check_text)
    End Function
    'Get CRC32
    Public Function GetCRC32(ByVal sFileName As String) As String
        Try
            Dim FS As FileStream = New FileStream(sFileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            Dim CRC32Result As Integer = &HFFFFFFFF
            Dim Buffer(4096) As Byte
            Dim ReadSize As Integer = 4096
            Dim Count As Integer = FS.Read(Buffer, 0, ReadSize)
            Dim CRC32Table(256) As Integer
            Dim DWPolynomial As Integer = &HEDB88320
            Dim DWCRC As Long
            Dim i As Integer, j As Integer, n As Integer

            'Create CRC32 Table
            For i = 0 To 255
                DWCRC = i
                For j = 8 To 1 Step -1
                    If CBool(DWCRC And 1) Then
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                        DWCRC = DWCRC Xor DWPolynomial
                    Else
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                    End If
                Next j
                CRC32Table(i) = CInt(DWCRC)
            Next i

            'Calcualting CRC32 Hash
            Do While (Count > 0)
                For i = 0 To Count - 1
                    n = (CRC32Result And &HFF) Xor Buffer(i)
                    CRC32Result = ((CRC32Result And &HFFFFFF00) \ &H100) And &HFFFFFF
                    CRC32Result = CRC32Result Xor CRC32Table(n)
                Next i
                Count = FS.Read(Buffer, 0, ReadSize)
            Loop
            FS.Close()
            Return Hex(Not (CRC32Result)).ToUpper.PadLeft(8, "0"c)
        Catch ex As Exception
            Return ""
        End Try
    End Function


    'Extensions
    <Extension()>
    Public Function GetAllNodesRecur(nodes As TreeNodeCollection) As List(Of TreeNode)
        Dim l As New List(Of TreeNode)
        For Each node As TreeNode In nodes
            l.Add(node)
            If node.Nodes.Count > 0 Then l.AddRange(GetAllNodesRecur(node.Nodes))
        Next
        Return l
    End Function
    <Extension()>
    Public Iterator Function GetAllNodesRecur2(nodes As TreeNodeCollection) As IEnumerable(Of TreeNode)
        For Each node In nodes.OfType(Of TreeNode)
            Yield node

            For Each child In node.Nodes.GetAllNodesRecur2
                Yield child
            Next
        Next
    End Function
    <Extension()>
    Public Sub ForEach(Of T)(source As T(), action As System.Action(Of T))
        For Each v In source
            action.Invoke(v)
        Next
    End Sub

    'Extensions - Control
    <Extension()>
    Public Sub DragDrop_Files_Setup(c As Control)
        c.AllowDrop = True
        AddHandler c.DragEnter, Sub(sender As Object, e As DragEventArgs) If e.Data.GetDataPresent(DataFormats.FileDrop) Then e.Effect = DragDropEffects.Copy
        AddHandler c.DragDrop, Sub(sender As Object, e As DragEventArgs)
                                   Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
                                   If files.Count <= 0 Then Exit Sub
                                   Dim f = files(0)
                                   If f.ToUpper().StartsWith(Application.StartupPath.ToUpper()) Then f = "." + f.Substring(Application.StartupPath.Length)
                                   DirectCast(sender, Control).Text = f
                               End Sub
    End Sub

    'Extensions - Int
    <Extension()>
    Public Function PercentOf(current As Integer, max As Integer, Optional decimal_count As Integer = 0) As Decimal
        Return Math.Round(CDec((current / max) * 100), decimal_count)
    End Function

    'Extensions - String
    <Extension()>
    Public Function Repeat(s As String, count As Integer) As String
        Return String.Concat(Enumerable.Repeat(s, count))
    End Function
    <Extension()>
    Public Function SubstringTo(s As String, str As String) As String
        If str = "" Then Return s

        Dim i As Integer = s.IndexOf(str)
        If i < 0 Then Return s
        If i = 0 Then Return ""

        Return s.Substring(0, i)
    End Function
    <Extension()>
    Public Function SubstringFrom(s As String, str As String, Optional exclude As Boolean = False) As String
        If str = "" Then Return s

        Dim i As Integer = s.IndexOf(str)
        If i < 0 Then Return ""

        If exclude Then
            i += str.Length
            If s.Length > i Then Return s.Substring(i) Else Return ""
        Else
            Return s.Substring(i)
        End If
    End Function
    <Extension()>
    Public Function SubstringBetween(s As String, str1 As String, str2 As String, Optional start_from As Integer = 0, Optional ignoreCase As Boolean = False, Optional inBetweenStrict As Boolean = True) As String
        Dim cmp As StringComparison = StringComparison.Ordinal
        If ignoreCase Then cmp = StringComparison.OrdinalIgnoreCase
        Dim i1 = s.IndexOf(str1, start_from, cmp)
        Dim i2 = s.IndexOf(str2, i1 + str1.Length, cmp)
        If i1 < 0 Or i2 < i1 Then Return ""

        s = s.Substring(i1, i2 - i1 + 1)
        If inBetweenStrict Then
            If s.Length < str1.Length + str2.Length + 1 Then Return ""
            Return s.Substring(str1.Length, s.Length - str1.Length - str2.Length)
        Else
            Return s
        End If
    End Function

    'Extensions - Web Browser DOM
    <Extension()>
    Public Function FilterElementsByClassName(el As IEnumerable(Of GeckoElement), class_name As String) As List(Of GeckoElement)
        Dim res = el.Where(Function(x) DirectCast(x, GeckoHtmlElement).ClassName.ToLower = class_name.ToLower)
        Return res.ToList()
    End Function
    <Extension()>
    Public Function FilterElementsByTagName(el As IEnumerable(Of GeckoElement), tag_name As String) As List(Of GeckoElement)
        Dim res = el.Where(Function(x) DirectCast(x, GeckoHtmlElement).TagName.ToLower = tag_name.ToLower)
        Return res.ToList()
    End Function
    <Extension()>
    Public Function GetElementByTagName(el As GeckoElement, tag_name As String) As GeckoHtmlElement
        Dim res = el.GetElementsByTagName(tag_name)
        If res.Count > 0 Then Return res(0UI) Else Return Nothing
    End Function
    <Extension()>
    Public Function GetElementByTagAndClassName(el As GeckoElement, tag_name As String, class_name As String) As GeckoHtmlElement
        Dim res = el.GetElementsByTagName(tag_name)
        Dim res2 = res.FilterElementsByClassName(class_name)
        If res2.Count > 0 Then Return res2(0) Else Return Nothing
    End Function
    <Extension()>
    Public Function GetElementByID(el As GeckoElement, tag As String, id As String) As GeckoHtmlElement
        Dim res = el.GetElementsByTagName(tag).Cast(Of GeckoHtmlElement)().Where(Function(e) e.Id.ToLower = id.ToLower)
        If res.Count > 0 Then Return res(0) Else Return Nothing
    End Function
    <Extension()>
    Public Function GetElementsByTagAndClassName(el As GeckoElement, tag_name As String, class_name As String) As Collections.IDomHtmlCollection(Of GeckoHtmlElement)
        Dim res = el.GetElementsByTagName(tag_name)
        Dim res2 = res.FilterElementsByClassName(class_name)
        If res2.Count > 0 Then Return res2 Else Return Nothing
    End Function
End Module

Public Class Custom_ToolTip
    Shared form As Form_Tooltip = Nothing
    Shared parent_form As Form = Nothing

    Shared lbl As Label = Nothing
    Shared ctrl As Control = Nothing
    Public Shared Sub Setup(c As Control, text As String, owner As Form)
        If c.GetType() Is GetType(NumericUpDown) Then
            AddHandler c.MouseEnter, Sub() Custom_ToolTip.Show(text, owner, c)
            AddHandler c.MouseLeave, Sub() Custom_ToolTip.Hide()
            AddHandler c.Controls(0).MouseEnter, Sub() Custom_ToolTip.Show(text, owner, c)
            AddHandler c.Controls(0).MouseLeave, Sub() Custom_ToolTip.Hide()
            AddHandler c.Controls(1).MouseEnter, Sub() Custom_ToolTip.Show(text, owner, c)
            AddHandler c.Controls(1).MouseLeave, Sub() Custom_ToolTip.Hide()
        Else
            AddHandler c.MouseEnter, Sub() Custom_ToolTip.Show(text, owner, c)
            AddHandler c.MouseLeave, Sub() Custom_ToolTip.Hide()
        End If

        'Hide tooltip on alt + tab
        AddHandler c.FindForm().Deactivate, Sub() Hide(True)
    End Sub
    Public Shared Sub Show(text As String, owner As Form, c As Control)
        ctrl = c
        If parent_form IsNot owner Then
            parent_form = owner
        End If

        'Don't show tooltip on background (not active) forms
        If System.Windows.Forms.Form.ActiveForm IsNot parent_form Then Exit Sub

        If form Is Nothing Then
            form = New Form_Tooltip
            'form.TopMost = True
            form.ShowInTaskbar = False
            form.FormBorderStyle = FormBorderStyle.None
            form.BackColor = SystemColors.Info
            AddHandler form.MouseLeave, Sub() Hide()
            AddHandler form.Activated, Sub() parent_form.Activate()

            lbl = New Label With {.AutoSize = True, .Location = New Point(1, 1)}
            lbl.BackColor = Color.Transparent : lbl.ForeColor = SystemColors.InfoText
            lbl.Font = New Font(lbl.Font.FontFamily, 10, FontStyle.Regular)
            lbl.Padding = New Padding(5)
            form.Controls.Add(lbl)
        End If
        lbl.Text = text
        lbl.Update()
        form.Size = lbl.Size

        form.Show()
        MouseMove() 'To set form position
        'owner.Activate()
    End Sub
    Public Shared Sub Hide(Optional force As Boolean = False)
        If form Is Nothing OrElse Not form.Visible OrElse ctrl Is Nothing Then Exit Sub

        If Not force Then
            Dim tmp = Cursor.Position
            If form.DesktopBounds.Contains(tmp) Then Exit Sub
            If New Rectangle(ctrl.PointToScreen(New Point(0, 0)), ctrl.Size).Contains(tmp) Then Exit Sub
            'If ctrl.GetType Is GetType(NumericUpDown) Then
            '    If New Rectangle(ctrl.Controls(0).PointToScreen(New Point(0, 0)), ctrl.Controls(0).Size).Contains(tmp) Then Exit Sub
            '    If New Rectangle(ctrl.Controls(1).PointToScreen(New Point(0, 0)), ctrl.Controls(1).Size).Contains(tmp) Then Exit Sub
            'Else
            '    If New Rectangle(ctrl.PointToScreen(New Point(0, 0)), ctrl.Size).Contains(tmp) Then Exit Sub
            'End If
        End If

        form.Hide()
        ctrl = Nothing
    End Sub
    Public Shared Sub MouseMove()
        If form Is Nothing Then Exit Sub

        Dim target_point = Cursor.Position + New Point(5, 5)
        If target_point.X > My.Computer.Screen.Bounds.Width - form.Width Then target_point.X = My.Computer.Screen.Bounds.Width - form.Width
        If target_point.Y > My.Computer.Screen.Bounds.Height - form.Height - 10 Then target_point.Y = My.Computer.Screen.Bounds.Height - form.Height - 10

        form.Location = target_point
    End Sub

    'Impliment custom filter for windows messages, to handle mouse move globally
    <SecurityPermission(SecurityAction.LinkDemand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Public Class message_filter
        Implements IMessageFilter

        Public Shared Sub Init_Shared()
            Dim t = New Custom_ToolTip.message_filter()
        End Sub

        Public Sub New()
            Application.AddMessageFilter(Me)
        End Sub

        Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            'Messages list - https://wiki.winehq.org/List_Of_Windows_Messages
            'WM_MOUSEMOVE - 512
            'WM_LBUTTONDOWN - 513
            'WM_LBUTTONUP - 514
            'WM_RBUTTONDOWN - 516
            'WM_RBUTTONUP - 517
            If m.Msg = 512 Then Custom_ToolTip.MouseMove()
            Return False
        End Function
    End Class

    Public Class Form_Tooltip
        Inherits Form

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Dim WS_EX_NOACTIVATE As Integer = &H8000000
                Dim WS_EX_TOOLWINDOW As Integer = &H80
                Dim WS_EX_TOPMOST As Integer = &H8
                Dim baseParams = MyBase.CreateParams
                baseParams.ExStyle = baseParams.ExStyle Or (WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW Or WS_EX_TOPMOST)

                Return baseParams
            End Get
        End Property

        Protected Overrides Sub DefWndProc(ByRef m As Message)
            Const WM_MOUSEACTIVATE = &H21
            Const MA_NOACTIVATE = &H3
            If m.Msg = WM_MOUSEACTIVATE Then
                m.Result = New IntPtr(MA_NOACTIVATE)
            Else
                MyBase.DefWndProc(m)
            End If
        End Sub
    End Class
End Class

Public Class MsgBox_Custom
    Dim txtMsg As Label = New Label()
    Dim newForm As Form = New Form()
    Dim chk As CheckBox = Nothing
    Public res As DialogResult2 = DialogResult2.None

    Public Shared Last_Check_State = False

    Enum DialogResult2
        None
        Button1
        Button2
        Button3
        Button4
        Button5
        Button6
        Button7
        Button8
        Button9
    End Enum

    Public Sub SpawnForm(text As String, buttons As String, Optional title As String = Nothing, Optional check_text As String = Nothing)
        Dim btn_text_arr = buttons.Split({"|"c})
        If String.IsNullOrWhiteSpace(title) Then title = Application.ProductName

        txtMsg.Text = text
        txtMsg.AutoSize = True
        txtMsg.Font = SystemFonts.MessageBoxFont
        txtMsg.TextAlign = ContentAlignment.MiddleLeft
        txtMsg.Padding = New Padding(9, 19, 9, 19)
        txtMsg.BackColor = SystemColors.Window
        txtMsg.MaximumSize = New Size(400, 0)
        txtMsg.MinimumSize = New Size(35 + (btn_text_arr.Count * 85), 59)

        newForm.FormBorderStyle = FormBorderStyle.FixedDialog
        newForm.MaximizeBox = False
        newForm.MinimizeBox = False
        newForm.ShowInTaskbar = False
        newForm.Text = title
        newForm.Controls.Add(txtMsg)
        newForm.Width = txtMsg.Width + 18
        newForm.Height = txtMsg.Height + 81

        Dim ind As Integer = btn_text_arr.Count()
        Dim first_x = newForm.Width - 75 - 32
        For Each btn_text In btn_text_arr.Reverse()
            Dim btn As New Button() With {.Text = btn_text, .Tag = CType(ind, DialogResult2)}
            btn.Location = New Point(first_x, txtMsg.Location.Y + txtMsg.Height + 9)
            btn.Anchor = AnchorStyles.Right
            newForm.Controls.Add(btn)
            AddHandler btn.Click, Sub(o As Object, e As EventArgs)
                                      If chk IsNot Nothing Then Last_Check_State = chk.Checked Else Last_Check_State = False
                                      res = DirectCast(DirectCast(o, Button).Tag, DialogResult2) : newForm.Close()
                                  End Sub
            first_x -= 85 : ind -= 1
        Next

        If Not String.IsNullOrWhiteSpace(check_text) Then
            chk = New CheckBox() With {.Text = check_text, .Left = 10}
            chk.AutoSize = True
            chk.Top = newForm.Height - chk.Height - 45
            Dim first_button_x = newForm.Controls(newForm.Controls.Count - 1).Left
            newForm.Controls.Add(chk)

            If first_button_x < chk.Left + chk.Width + 10 Then
                Dim width_diff = (chk.Left + chk.Width + 10) - first_button_x
                txtMsg.AutoSize = False
                txtMsg.Width += width_diff
                newForm.Width += width_diff
            End If
        End If

        newForm.StartPosition = FormStartPosition.Manual
        newForm.Top = CInt(My.Computer.Screen.Bounds.Height / 2) - CInt(newForm.Height / 2) + 13
        newForm.Left = CInt(My.Computer.Screen.Bounds.Width / 2) - CInt(newForm.Width / 2) + 10
        newForm.ShowDialog()
    End Sub

    Public Shared Function Show(text As String, buttons As String, Optional title As String = Nothing, Optional check_text As String = Nothing) As DialogResult2
        Dim f = New MsgBox_Custom()
        f.SpawnForm(text, buttons, title, check_text)
        Return f.res
    End Function
End Class