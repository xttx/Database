Imports Gecko
Imports Gecko.Events
Imports Microsoft.CSharp
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.ComponentModel

'TODO - Output path does not change appropriately on 'auto' checkbox checkedChanged
'TODO - Sometimes first unreal product info grab make error in extension module sending NULL to it as element, error is not handled
'TODO - Option to bypass parsed url list
'TODO - List to choose input/output files
'TODO - Proper background downloader thread cancelation on FormClose (to not leave partially downloaded files)

'DONE - "Go To Module Start Page" button for easier "parse single page" mode
'DONE - for 'single page parse' mode, need to check if browser is busy
'DONE - modules parse error handling
'DONE - brawser documentComplete event is not deactivated when a module is loaded but not running. And if we reload a page manually, it will be triggered
'DONE - Save output path to ini

Public Class FormE_Grabber
	Dim modules As New Dictionary(Of String, List(Of String))

	Dim browser_event_active = False
	Dim inst As Object = Nothing
	Dim method_parse As MethodInfo = Nothing
	Dim method_navigate_next As MethodInfo = Nothing
	Dim method_preprocess As MethodInfo = Nothing

	Dim input_urls As New List(Of String)
	Dim input_urls_current = 0

	Dim urls_done As New List(Of String)

	Dim refr As Boolean = False
	Dim WithEvents wait_timer As New Timer With {.Interval = 1000, .Enabled = False}
	Dim WithEvents bg_worker As New BackgroundWorker() With {.WorkerSupportsCancellation = True}
	Dim bg_worker_thread As Threading.Thread = Nothing


	Dim browser_context_menu As New ContextMenuStrip()
	Dim browser_context_menu_items(6) As ToolStripMenuItem
	Dim listbox_context_menu_items(0) As ToolStripMenuItem
	Dim stop_button_text = {"Stop", "Parse Current Page", "Wait for background thread..."}

	Const EM_SETTABSTOPS As Integer = &HCB
	Declare Function SendMessageA Lib "user32" (ByVal TBHandle As IntPtr, ByVal EM_SETTABSTOPS As Integer, ByVal wParam As Integer, ByRef lParam As Integer) As Boolean

	Dim Code_Parser As New Class50_CodeParser()
	Dim Dont_Parse As Boolean = False

	Private Sub FormE_Grabber_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Me.Height += 80
		SendMessageA(RichTextBox1.Handle, EM_SETTABSTOPS, 1, 4 * 3)
		'both left and right margins set to 10 pixels (A) with the last parm: 0x000A000A
		'SendMessage(RichTextBox1.Handle, &HD3, &H3, &H40004)
		RichTextBox1_TextChanged(RichTextBox1, New EventArgs)

		TextBox4.DragDrop_Files_Setup()
		TextBox2.DragDrop_Files_Setup()
		TextBox7.DragDrop_Files_Setup()
		TextBox10.DragDrop_Files_Setup()

		'Setup browser context menu, for screenshot view options
		browser_context_menu_items(0) = New ToolStripMenuItem("Show First Screenshot - Fit") With {.Checked = True}
		browser_context_menu_items(1) = New ToolStripMenuItem("Show First Screenshot - Width 100%")
		browser_context_menu_items(2) = New ToolStripMenuItem("Show First Screenshot - Height 100%")
		browser_context_menu_items(3) = New ToolStripMenuItem("Show All Screenshot - Grid 3 Column")
		browser_context_menu_items(4) = New ToolStripMenuItem("Show All Screenshot - Grid 4 Column")
		browser_context_menu_items(5) = New ToolStripMenuItem("Show All Screenshot - Grid 5 Column")
		browser_context_menu_items(6) = New ToolStripMenuItem("Autoscroll to Bottom in Gridview Mode") With {.Checked = True}
		For Each mi In browser_context_menu_items.Except({browser_context_menu_items(6)})
			browser_context_menu.Items.Add(mi)
			AddHandler mi.Click, Sub(o As Object, ea As EventArgs)
									 browser_context_menu_items.ForEach(New Action(Of ToolStripMenuItem)(Sub(t) t.Checked = False))
									 DirectCast(o, ToolStripMenuItem).Checked = True
									 show_screenshot_in_browser(True)
								 End Sub
		Next
		browser_context_menu.Items.Add(New ToolStripSeparator())
		browser_context_menu.Items.Add(browser_context_menu_items(6))
		AddHandler browser_context_menu_items(6).Click, Sub() browser_context_menu_items(6).Checked = Not browser_context_menu_items(6).Checked

		'Setup listbox context menu
		Dim listbox_context_menu As New ContextMenuStrip()
		listbox_context_menu_items(0) = New ToolStripMenuItem("Autoscroll to Bottom") With {.Checked = True}
		listbox_context_menu.Items.Add(listbox_context_menu_items(0))
		AddHandler listbox_context_menu_items(0).Click, Sub() listbox_context_menu_items(0).Checked = Not listbox_context_menu_items(0).Checked
		ListBox1.ContextMenuStrip = listbox_context_menu

		GeckoWebBrowser1 = New GeckoWebBrowser()
		GeckoWebBrowser1.Parent = Panel1
		GeckoWebBrowser1.Dock = DockStyle.Fill
		TabPage1.Controls.Add(GroupBox2) : TabPage1.Controls.Add(GroupBox3)
		TabControl1.TabPages.Remove(TabPage5)
		GroupBox2.Left = GroupBox1.Left : GroupBox3.Left = GroupBox1.Left
		GroupBox2.Top = GroupBox1.Top + GroupBox1.Height + 6 : GroupBox3.Top = GroupBox2.Top
		AddHandler wait_timer.Tick, Sub() GeckoWebBrowser1_DocumentCompleted(GeckoWebBrowser1, New GeckoDocumentCompletedEventArgs(Nothing, Nothing))
		AddHandler GeckoWebBrowser1.Navigating, Sub(o As Object, arg As GeckoNavigatingEventArgs) If arg.Uri IsNot Nothing Then TextBox1.Text = arg.Uri.ToString Else TextBox1.Text = ""
		GeckoPreferences.User("browser.cache.disk.enable") = False
		GeckoPreferences.User("browser.cache.memory.enable") = False
		'GeckoWebBrowser1.UseHttpActivityObserver = True

		'Workaround for memory leaks - not working
		'Dim heapMinimizerTimer = New Timer()
		'heapMinimizerTimer.Interval = (2 * 60 * 1000) '2 mins
		'AddHandler heapMinimizerTimer.Tick, Sub()
		'										Dim _memoryService = Xpcom.GetService(Of nsIMemory)("@mozilla.org/xpcom/memory-service;1")
		'										_memoryService.HeapMinimize(False)
		'									End Sub
		'heapMinimizerTimer.Start()

		For Each d In Directory.GetDirectories(".\Grabbers")
			If d.ToUpper.EndsWith("\DATA") Then Continue For

			Dim l As New List(Of String)
			For Each f In Directory.GetFiles(d).Where(Function(x) x.ToLower.EndsWith(".cs") Or x.ToLower.EndsWith(".vb"))
				l.Add(f)
				ComboBox4.Items.Add(f.Substring(10))
			Next
			modules.Add(Path.GetFileName(d), l)
			ComboBox1.Items.Add(Path.GetFileName(d))
		Next

		ComboBox3.Items.Add("[Don't check]")
		ComboBox3.SelectedIndex = 0
		Dim rdr = db.queryReader("PRAGMA table_info(main)")
		Do While rdr.Read
			Dim n = rdr.GetString(1)
			Dim t = rdr.GetString(2)
			If n.ToUpper = "NAME" OrElse n.ToUpper = "ID" Then Continue Do
			If t.ToUpper.StartsWith("VARCHAR") Then
				Dim f_name = ""
				Dim f_info = Fields.Where(Function(fi) fi.DBname.ToUpper = n.ToUpper).FirstOrDefault()
				If f_info IsNot Nothing Then f_name = f_info.name

				'Dim f_name = Form1.ini.IniReadValue("Interface", "Field" + n.Substring(n.IndexOf("_"))).Split({"|||"}, StringSplitOptions.None)
				If f_name <> "" Then f_name = " - " + f_name
				ComboBox3.Items.Add(n + f_name)
				ComboBox5.Items.Add(n + f_name) 'Check Parsed Data -> Find Already in DB
			End If
		Loop
		If ComboBox5.Items.Count > 0 Then refr = True : ComboBox5.SelectedIndex = 0 : refr = False

		CheckParsedData_OptionLoad()

		GeckoWebBrowser1.Navigate("www.google.com")
		Options.browser = GeckoWebBrowser1
	End Sub
	Private Sub FormE_Grabber_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
		GeckoWebBrowser1.Stop()
		GeckoWebBrowser1.Dispose()
		GeckoWebBrowser1 = Nothing

		If bg_worker_thread IsNot Nothing AndAlso bg_worker_thread.IsAlive Then bg_worker_thread.Abort()
	End Sub

	Private Sub GeckoWebBrowser1_DocumentCompleted(sender As Object, e As GeckoDocumentCompletedEventArgs) Handles GeckoWebBrowser1.DocumentCompleted
		If GeckoWebBrowser1 Is Nothing Then Exit Sub
		If GeckoWebBrowser1.IsBusy Then
			MsgBox("Busy?")
			Exit Sub
		End If
		If Options.mode = Options.modes.input_file_direct_download Then
			'Mode3 (screenshots) - If grid screen mode, scroll browser to bottom
			If browser_context_menu_items(3).Checked OrElse browser_context_menu_items(4).Checked OrElse browser_context_menu_items(5).Checked Then
				If browser_context_menu_items(6).Checked Then GeckoWebBrowser1.Window.ScrollTo(0, GeckoWebBrowser1.Window.ScrollMaxY)
			End If
			Exit Sub
		End If
		If Not browser_event_active Then Exit Sub

		wait_timer.Enabled = False
		Options.wait_request = False
		For Each k In Options.fields_data.Keys.ToArray
			Options.fields_data(k) = ""
		Next
		For Each k In Options.fields_data_arr.Keys.ToArray
			Options.fields_data_arr(k) = New List(Of String)
		Next
		For Each k In Options.additional_files_content.Keys.ToArray
			Options.additional_files_content(k) = New List(Of String)
		Next

		If inst Is Nothing OrElse method_parse Is Nothing Then Exit Sub

		Options.err = ""
		Dim html = DirectCast(GeckoWebBrowser1.Document.DocumentElement, GeckoHtmlElement)
		'Dim cur_url = input_urls(input_urls_current)

		'TEST MODULES HERE

		'TEST MODULES END



		Try
			If Options.mode = Options.modes.start_page Then
				method_parse.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {html, GeckoWebBrowser1.Url.ToString}, Globalization.CultureInfo.CurrentCulture)
				'parse(html, GeckoWebBrowser1.Url.ToString)
			ElseIf Options.mode = Options.modes.input_file Then
				method_parse.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {html, input_urls(input_urls_current)}, Globalization.CultureInfo.CurrentCulture)
			End If
		Catch ex As Exception
			MsgBox("Module parse() function error: " + vbCrLf + ex.Message + vbCrLf + ex.InnerException.ToString + vbCrLf + vbCrLf + ex.StackTrace)
			Button1.Enabled = True
			'Button4.Enabled = True
			browser_event_active = False
			Exit Sub
		End Try

		If (String.IsNullOrEmpty(Options.err)) Then
			If Options.wait_request Then wait_timer.Enabled = True : Exit Sub
			Save_Data()
		Else
			Dim res = MsgBox("Parser throw an error while parsing:" + vbCrLf + GeckoWebBrowser1.Url.ToString + vbCrLf + Options.err + vbCrLf + vbCrLf + "Do you want to continue?", MsgBoxStyle.YesNo)
			If res = MsgBoxResult.No Then browser_event_active = False : wait_timer.Enabled = False : Exit Sub
		End If

		If Options.all_done Then
			browser_event_active = False : wait_timer.Enabled = False
			System.IO.File.WriteAllText(".\Grabbers\DONE_HTML_Source.txt", html.OuterHtml)
			MsgBox("Done")
			Exit Sub
		End If

		Got_To_Next_Url()
	End Sub
	'Private Sub Browser1_ObserveHttpModifyRequest(sender As Object, e As Gecko.GeckoObserveHttpModifyRequestEventArgs) Handles GeckoWebBrowser1.ObserveHttpModifyRequest
	'	Dim str = e.Uri.ToString
	'	If str.Substring(str.Length - 4).ToLower() = ".jpg" Then e.Cancel = True
	'	If str.Substring(str.Length - 4).ToLower() = ".png" Then e.Cancel = True
	'	If str.Substring(str.Length - 4).ToLower() = ".gif" Then e.Cancel = True
	'End Sub

	'Run module
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		If inst Is Nothing OrElse method_parse Is Nothing Then Exit Sub
		'Button4.Enabled = False
		'Button5.Enabled = False
		Button4.Text = stop_button_text(0)
		TabControl1.SelectedIndex = 1
		TabControl1.TabPages(0).Enabled = False
		TabControl1.TabPages(2).Enabled = False
		TabControl1.TabPages(3).Enabled = False
		GeckoWebBrowser1.Stop()

		'Fill already done list
		urls_done = New List(Of String)
		Dim done_path = ".\Grabbers\" + ComboBox1.SelectedItem.ToString + "\" + ComboBox2.SelectedItem.ToString + "_parsed_urls.txt"
		If File.Exists(done_path) Then
			urls_done = File.ReadAllLines(done_path).Select(Of String)(Function(x) x.ToLower().Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim({vbTab.ToCharArray()(0), " "c, "\"c, "/"c, "&"c, "?"c, "#"c})).ToList()
		End If

		'Parse input url file if needed
		Dim regex As Regex = Nothing
		If Options.input_file_regex <> "" Then regex = New Regex(Options.input_file_regex)
		If Options.mode = Options.modes.start_page AndAlso Not String.IsNullOrEmpty(Options.start_page) Then
			browser_event_active = True
		ElseIf Options.mode = Options.modes.input_file Then
			Dim lines = File.ReadAllLines(TextBox2.Text)
			For Each l In lines
				If regex IsNot Nothing Then
					Dim m = regex.Match(l)
					If m.Success Then input_urls.Add(m.Value)
				Else
					input_urls.Add(l.Trim())
				End If
			Next

			''Check links already in db
			'If ComboBox3.SelectedIndex > 0 Then
			'	Dim field = ComboBox3.SelectedItem.ToString
			'	If field.Contains(" ") Then field = field.Substring(0, field.IndexOf(" "))

			'	For i As Integer = input_urls.Count - 1 To 0 Step -1
			'		Dim u = input_urls(i).ToLower.Replace("http://", "").Replace("https://", "").Replace("www.", "")
			'		Dim rdr = db.queryReader("SELECT id FROM main where " + field + " Like '%" + u + "'")
			'		If rdr.HasRows Then input_urls.RemoveAt(i)
			'	Next
			'End If

			browser_event_active = True
		ElseIf Options.mode = Options.modes.input_file_direct_download Then
			GeckoWebBrowser1.ContextMenuStrip = browser_context_menu

			If Not Directory.Exists(TextBox4.Text.Trim) Then
				Try
					Directory.CreateDirectory(TextBox4.Text.Trim)
				Catch ex As Exception
					MsgBox("Can not create directory: " + vbCrLf + TextBox4.Text.Trim) : Exit Sub
				End Try
			End If

			Options.links = New List(Of KeyValuePair(Of String, List(Of String)))
			Dim lines = File.ReadAllLines(TextBox7.Text.Trim)
			For Each l In lines
				l = l.Trim
				If l.StartsWith("[") AndAlso l.EndsWith("]") Then
					Dim kv = New KeyValuePair(Of String, List(Of String))(l.Substring(1, l.Length - 2), New List(Of String))
					Options.links.Add(kv)
				Else
					Options.links(Options.links.Count - 1).Value.Add(l)
				End If
			Next
			bg_worker.RunWorkerAsync()
			Exit Sub
		End If

		Got_To_Next_Url()
	End Sub
	Private Sub Save_Data()
		'Main fields data
		Dim sep = TextBox3.Text
		Dim line = ""
		If Options.fields_data_arr(Options.fields(0)).Count > 0 Then
			Dim lines As New List(Of String)
			For i As Integer = 0 To Options.fields_data_arr(Options.fields(0)).Count - 1
				line = ""
				For Each f In Options.fields
					line += Options.fields_data_arr(f)(i) + sep
				Next
				If line.Length >= sep.Length Then line = line.Substring(0, line.Length - sep.Length)
				If line.Length > 0 Then lines.Add(line)
			Next
			If lines.Count > 0 Then File.AppendAllLines(TextBox4.Text, lines)
		Else
			For Each f In Options.fields
				line += Options.fields_data(f) + sep
			Next
			If line.Length >= sep.Length Then line = line.Substring(0, line.Length - sep.Length)
			If line.Length > 0 Then File.AppendAllLines(TextBox4.Text, {line})

			'If line.Length > 0 Then
			'	FileOpen(1, TextBox4.Text, OpenMode.Append)
			'	PrintLine(1, line)
			'	FileClose(1)
			'End If
		End If

		'Additional data
		If Options.additional_files.Count > 0 Then
			FileOpen(1, TextBox5.Text, OpenMode.Append)
			For Each l In Options.additional_files_content(0)
				PrintLine(1, l)
			Next
			FileClose(1)
		End If

		If Options.additional_files.Count > 1 Then
			FileOpen(1, TextBox6.Text, OpenMode.Append)
			For Each l In Options.additional_files_content(1)
				PrintLine(1, l)
			Next
			FileClose(1)
		End If

		'Add to done_urls
		'urls_done.Add(urls(cur_url))
		FileOpen(1, ".\Grabbers\" + ComboBox1.SelectedItem.ToString + "\" + ComboBox2.SelectedItem.ToString + "_parsed_urls.txt", OpenMode.Append)
		If Options.mode = Options.modes.start_page Then
			PrintLine(1, GeckoWebBrowser1.Url.ToString)
			urls_done.Add(GeckoWebBrowser1.Url.ToString.ToLower)
		ElseIf Options.mode = Options.modes.input_file Then
			PrintLine(1, input_urls(input_urls_current))
			urls_done.Add(input_urls(input_urls_current))
		End If
		FileClose(1)

		'Log
		Dim n = (input_urls_current + 1).ToString + "/" + input_urls.Count.ToString + " "
		If Options.progress_total > 0 Then n = Options.progress_cur.ToString + "/" + Options.progress_total.ToString + " "
		If Options.fields.Contains("name") Then
			If Options.fields_data_arr("name").Count > 0 Then
				For Each v In Options.fields_data_arr("name")
					ListBox1.Items.Add(n + v)
				Next
			Else
				ListBox1.Items.Add(n + Options.fields_data("name"))
			End If
		Else
			ListBox1.Items.Add(n + input_urls(input_urls_current).Substring(input_urls(input_urls_current).LastIndexOf("/")))
		End If
		If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1
	End Sub
	Private Sub Got_To_Next_Url()
		GeckoWebBrowser1.Stop()

		Dim url As String = ""
		If Options.mode = Options.modes.start_page Then
			Dim html = DirectCast(GeckoWebBrowser1.Document.DocumentElement, GeckoHtmlElement)
			If method_navigate_next IsNot Nothing Then
				If Not String.IsNullOrEmpty(Options.start_page) Then
					If Not CheckBox3.Checked Then url = Options.start_page Else url = GeckoWebBrowser1.Url.AbsoluteUri
					Options.start_page = ""
				Else
					Dim ret = method_navigate_next.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {html, GeckoWebBrowser1.Url.ToString}, Globalization.CultureInfo.CurrentCulture)
					url = DirectCast(ret, String)
				End If

				If Not String.IsNullOrEmpty(url) Then
					While Check_If_Url_Is_Already_Done(url)
						Dim ret = method_navigate_next.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {html, url}, Globalization.CultureInfo.CurrentCulture)
						url = DirectCast(ret, String)
						If String.IsNullOrEmpty(url) Then browser_event_active = False : MsgBox("Done") : Exit Sub
					End While
				Else
					browser_event_active = False : MsgBox("Done") : Exit Sub
				End If
			End If
		ElseIf Options.mode = Options.modes.input_file Then
			input_urls_current += 1
			While input_urls_current < input_urls.Count AndAlso Check_If_Url_Is_Already_Done(input_urls(input_urls_current))
				input_urls_current += 1
			End While

			If (input_urls_current < input_urls.Count) Then
				url = input_urls(input_urls_current)
			Else
				browser_event_active = False : MsgBox("Done") : Exit Sub
			End If
		End If

		Recreate_Gecko_Brawser() 'trying to workaround memory leak
		If Options.gecko_load_flags = GeckoLoadFlags.None Then
			'GeckoWebBrowser1.Navigate(url, GeckoLoadFlags.BypassCache Or GeckoLoadFlags.FirstLoad Or GeckoLoadFlags.StopContent) -- trying to workaround memory leak, but it does not work
			GeckoWebBrowser1.Navigate(url)
		Else
			GeckoWebBrowser1.Navigate(url, Options.gecko_load_flags)
		End If
	End Sub
	Private Function Check_If_Url_Is_Already_Done(url As String)
		'Check in _parsed.txt
		url = url.ToLower().Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim({vbTab.ToCharArray()(0), " "c, "\"c, "/"c, "&"c, "?"c, "#"c})
		If urls_done.Contains(url) Then
			ListBox1.Items.Add((input_urls_current + 1).ToString + "/" + input_urls.Count.ToString + " Skipped (already parsed)")
			If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1
			Return True
		End If

		'Check already in db
		If Options.mode = Options.modes.input_file AndAlso ComboBox3.SelectedIndex > 0 Then
			Dim field = ComboBox3.SelectedItem.ToString
			If field.Contains(" ") Then field = field.Substring(0, field.IndexOf(" "))

			url = url.Replace("'", "''")
			Dim rdr = db.queryReader("SELECT id FROM main where " + field + " Like '%" + url + "'")
			If rdr.HasRows Then
				ListBox1.Items.Add((input_urls_current + 1).ToString + "/" + input_urls.Count.ToString + " Skipped (already in db)")
				If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1
				Return True
			End If
		End If

		Return False
	End Function

	'Screenshot downloader (mode 3) background method
	Private Sub downloader_bg(o As Object, e As DoWorkEventArgs) Handles bg_worker.DoWork
		bg_worker_thread = Threading.Thread.CurrentThread
		Dim mainForm = DirectCast(Me.Owner, Form1)
		Dim screenshot_path = Form1.ini.IniReadValue("Paths", "Screenshots")
		Dim target_dir = TextBox4.Text.Trim
		Dim action_on_fail = "1" 'Ask
		Dim action_on_fail2 = "1" 'Ask
		If RadioButton9.Checked Then action_on_fail = "2"   'Continue
		If RadioButton10.Checked Then action_on_fail = "3"  'Cancel
		If RadioButton11.Checked Then action_on_fail = "4"  'Retry
		If RadioButton15.Checked Then action_on_fail2 = "2" 'Continue
		If RadioButton16.Checked Then action_on_fail2 = "3" 'Cancel
		ListBox1.Invoke(Sub() ListBox1.Items.Clear())
		ListBox1.Invoke(Sub() ListBox1.Items.Add("Numbers are: (output dir/screen sir/download count)"))
		Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml(""))

		'preprocess
		Options.err = ""
		For i As Integer = 0 To Options.links.Count - 1
			Dim start_from = 0
			If CheckBox2.Checked Then start_from = 1
			Dim url_done_for_this_product As New List(Of String)
			Dim urls_parsed As New List(Of String)
			If inst IsNot Nothing AndAlso method_preprocess IsNot Nothing Then

				'''MODULE UNREAL GET_IMAGES
				'Dim links = Options.links(i)
				'Dim l = links.Value
				'Dim l_distinct = l.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
				'Dim count_to_remove = l.Count - l_distinct.Count
				''If count_to_remove Mod 2 <> 0 Then Options.err = "ERROR Preprocess: """ + links.Key + """ count_to_remove not divisible per 2" : Return Nothing
				'If count_to_remove Mod 2 <> 0 Then Options.err = "ERROR Preprocess: """ + links.Key + """ count_to_remove not divisible per 2"
				'Dim count_to_remove_from_each_side = CInt(count_to_remove / 2)
				'For n As Integer = 1 To count_to_remove_from_each_side
				'	l.RemoveAt(0)
				'	l.RemoveAt(l.Count - 1)
				'Next
				''If Not l.Count = l_distinct.Count Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links count not equal to distinct links count" : Return Nothing
				'If Not l.Count = l_distinct.Count Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links count not equal to distinct links count"
				''If Not l.Count = l.Distinct().Count Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links not unique" : Return Nothing
				'If Not l.Count = l.Distinct().Count Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links not unique"
				'For n As Integer = 0 To l_distinct.Count - 1
				'	'If Not l.Contains(l_distinct(n)) Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links does not contains all distinct" : Return Nothing
				'	If Not l.Contains(l_distinct(n)) Then Options.err = "ERROR Preprocess: """ + links.Key + """ processed links does not contains all distinct"
				'Next
				''Return New KeyValuePair(Of String, List(Of String))(links.Key, l)



				'''END MODULE

				Options.links(i) = DirectCast(method_preprocess.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {Options.links(i)}, Globalization.CultureInfo.CurrentCulture), KeyValuePair(Of String, List(Of String)))
				If Options.err <> "" Then
					Me.Invoke(Sub() MsgBox(Options.err)) : Options.err = "" : bg_worker_thread = Nothing : Exit Sub
				End If
			End If
			For l As Integer = start_from To Options.links(i).Value.Count - 1
				Dim url = Options.links(i).Value(l)
				If inst IsNot Nothing AndAlso method_parse IsNot Nothing Then
					url = DirectCast(method_parse.Invoke(inst, BindingFlags.InvokeMethod, Nothing, {url}, Globalization.CultureInfo.CurrentCulture), String)
				End If
				If url_done_for_this_product.Contains(url.ToLower) Then Continue For

				urls_parsed.Add(url)
				url_done_for_this_product.Add(url.ToLower)
			Next
			Options.links(i) = New KeyValuePair(Of String, List(Of String))(Options.links(i).Key, urls_parsed)
		Next

		'Download
		Dim total_process = 0
		Dim total_count = Options.links.Select(Of Integer)(Function(x) x.Value.Count).Sum()
		Me.Invoke(Sub() ProgressBar1.Maximum = total_count)
		For i As Integer = 0 To Options.links.Count - 1
			'Check if this screen already exist in both target dire and real screenshot path
			Dim product_name = Options.links(i).Key
			Dim screen_file = mainForm.getScreen(product_name, "", 0, "", True, True)
			Dim screen_file_trg = mainForm.getScreen(product_name, "", 0, "", True, True, target_dir)
			Dim screen_file_pattern = Path.GetFileName(screen_file + "*.*")

			'Check existibg screenshots in output folder, and then in database screenshots folder
			Dim skip_download = False
			Dim current_product_img_count = Options.links(i).Value.Count
			Dim count_str_out = "NA"
			Dim count_str_scr = "/NA"
			Dim count_str_ttl = "/" + current_product_img_count.ToString()
			Dim existing_files_count_target_dir = 0
			Dim existing_files_count_screen_dir = 0
			'Check in output folder
			If Not RadioButton3.Checked AndAlso CheckBox8.Checked Then
				If Directory.Exists(Path.GetDirectoryName(screen_file_trg)) Then existing_files_count_target_dir = Directory.GetFiles(Path.GetDirectoryName(screen_file_trg), screen_file_pattern).Count
				count_str_out = existing_files_count_target_dir.ToString
				If RadioButton4.Checked Then skip_download = (existing_files_count_target_dir = current_product_img_count)
				If RadioButton5.Checked Then skip_download = (existing_files_count_target_dir >= current_product_img_count)
			End If
			If skip_download Then
				total_process += Options.links(i).Value.Count
				ListBox1.Invoke(Sub() ListBox1.Items.Add("Skip: " + product_name + " (" + count_str_out + "/NA" + count_str_ttl + ") - found in output dir"))
				ListBox1.Invoke(Sub() If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1)
				Continue For
			End If
			'Check in DB screenshots folder
			If Not RadioButton3.Checked AndAlso CheckBox9.Checked Then
				existing_files_count_screen_dir = Directory.GetFiles(Path.GetDirectoryName(screen_file), screen_file_pattern).Count
				count_str_scr = "/" + existing_files_count_screen_dir.ToString
				If RadioButton4.Checked Then skip_download = (existing_files_count_screen_dir = current_product_img_count)
				If RadioButton5.Checked Then skip_download = (existing_files_count_screen_dir >= current_product_img_count)
			End If
			If skip_download Then
				total_process += Options.links(i).Value.Count
				ListBox1.Invoke(Sub() ListBox1.Items.Add("Skip: " + product_name + " (" + count_str_out + count_str_scr + count_str_ttl + ") - found in database screenshots dir"))
				ListBox1.Invoke(Sub() If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1)
				Continue For
			End If

			ListBox1.Invoke(Sub() ListBox1.Items.Add("Download: " + product_name + " (" + count_str_out + count_str_scr + count_str_ttl + ") - downloading"))
			ListBox1.Invoke(Sub() If (listbox_context_menu_items(0).Checked) Then ListBox1.SelectedIndex = ListBox1.Items.Count - 1)

			Dim screen_shown As Boolean = False
			Dim screens_urls As New List(Of String)
			GeckoWebBrowser1.Tag = (product_name, screens_urls)
			For l As Integer = 0 To Options.links(i).Value.Count - 1
				total_process += 1
				Dim url = Options.links(i).Value(l)

				Dim ext = Path.GetExtension(url)
				If String.IsNullOrWhiteSpace(ext) Then MsgBox("Error: could not get url extention" + vbCrLf + url) : Exit Sub

				Dim screen_file_name = mainForm.getScreen(Options.links(i).Key, "", l, "", False, True, target_dir)
				Dim target_path = screen_file_name + ext
				Dim target_folder = Path.GetDirectoryName(target_path)
				If Not Directory.Exists(target_folder) Then Directory.CreateDirectory(target_folder)

Retry_Label:
				Dim err = download_file(url, target_path)
				If err <> "" Then
					'Download fail
					If action_on_fail = "4" Then
						'Retry
						For r As Integer = 1 To CInt(NumericUpDown2.Value)
							err = download_file(url, target_path)
							If err = "" Then Exit For
						Next
						If err <> "" Then action_on_fail = action_on_fail2
					End If

					If action_on_fail = "1" Then
						'Ask
						Dim res = DirectCast(Me.Invoke(Function() MsgBoxEx("Error downloading image: " + vbCrLf + url + vbCrLf + vbCrLf, "Cancel|Retry|Continue", "", "Remember My Choice")), MsgBox_Custom.DialogResult2)
						If MsgBox_Custom.Last_Check_State Then
							If res = MsgBox_Custom.DialogResult2.Button2 Then action_on_fail = "4"
							If res = MsgBox_Custom.DialogResult2.Button3 Then action_on_fail = "2"
						End If
						If res = MsgBox_Custom.DialogResult2.Button1 Then bg_worker_thread = Nothing : Exit Sub
						If res = MsgBox_Custom.DialogResult2.Button2 Then GoTo Retry_Label
					ElseIf action_on_fail = "3" Then
						'Cancel
						bg_worker_thread = Nothing : Exit Sub
					End If
				Else
					'Download ok, show screen in browser
					If target_path.StartsWith(".\") Then target_path = Application.StartupPath + target_path.Substring(1)
					target_path = "file:///" + Web.HttpUtility.UrlEncode(target_path.Replace("\", "/")).Replace("%3a", ":").Replace("%2f", "/").Replace("+", "%20")
					screens_urls.Add(target_path)
					show_screenshot_in_browser()
				End If

				Dim current_p = i + 1
				Dim current_i = l + 1
				Me.Invoke(Sub()
							  Label10.Text = "Total Images: " + total_process.ToString + " / " + total_count.ToString + " (" + total_process.PercentOf(total_count).ToString() + "%)"
							  Label11.Text = "Current Product: " + current_p.ToString + " / " + Options.links.Count.ToString + " (" + current_p.PercentOf(Options.links.Count).ToString() + "%)"
							  Label12.Text = "Current Image: " + current_i.ToString + " / " + current_product_img_count.ToString + " (" + current_i.PercentOf(current_product_img_count).ToString() + "%)"
							  ProgressBar1.Value = total_process
						  End Sub)

				If bg_worker.CancellationPending Then e.Cancel = True : Exit Sub
			Next
		Next

		MsgBox("Done.")
		bg_worker_thread = Nothing
	End Sub
	Private Sub show_screenshot_in_browser(Optional force_refresh As Boolean = False)
		If GeckoWebBrowser1.Tag Is Nothing Then Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml("")) : Exit Sub

		Dim param = DirectCast(GeckoWebBrowser1.Tag, ValueTuple(Of String, List(Of String)))
		Dim screens_urls = param.Item2
		Dim product_name = System.Net.WebUtility.HtmlEncode(param.Item1)
		If screens_urls.Count = 0 Then Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml("")) : Exit Sub

		If browser_context_menu_items(0).Checked Or browser_context_menu_items(1).Checked Or browser_context_menu_items(2).Checked Then
			'Only show first screenshot
			If screens_urls.Count = 1 Or force_refresh Then
				Dim HTML_str = "<html><body><center style='-webkit-text-stroke: 1.5px lightblue; -webkit-text-fill-color: black; font-size: 200%;'>"
				HTML_str += "<img src='" + screens_urls(0) + "' style='???@%%%@???: 100%; background-color: white;'>"
				HTML_str += "<div style='position: absolute; top: 10px; left: 50%; transform: translate(-50%, 0%);'>" + product_name + "</div></center></body></html>"
				If browser_context_menu_items(0).Checked Then Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml(HTML_str.Replace("???@%%%@???", "object-fit: contain; width: 100%; height")))
				If browser_context_menu_items(1).Checked Then Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml(HTML_str.Replace("???@%%%@???", "width")))
				If browser_context_menu_items(2).Checked Then Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml(HTML_str.Replace("???@%%%@???", "height")))
			End If
		Else
			'Show all screenshots in grid
			Dim col = 1
			Dim HTML_str = "<html><body><table>"
			Dim TD = "<TD style='width: "
			If browser_context_menu_items(3).Checked Then TD += "33%;'>"
			If browser_context_menu_items(4).Checked Then TD += "25%;'>"
			If browser_context_menu_items(5).Checked Then TD += "20%;'>"
			For Each scr In screens_urls
				If col = 1 Then HTML_str += "<TR>"

				HTML_str += TD
				HTML_str += "<img src='" + scr + "' style='background-color: white; width: 100%;'>"
				HTML_str += "</TD>"

				If browser_context_menu_items(3).Checked AndAlso col = 3 Then col = 0
				If browser_context_menu_items(4).Checked AndAlso col = 4 Then col = 0
				If browser_context_menu_items(5).Checked AndAlso col = 5 Then col = 0
				If col = 0 Then HTML_str += "</TR>"
				col += 1
			Next

			If browser_context_menu_items(3).Checked AndAlso screens_urls.Count < 3 Then HTML_str += TD.Repeat(3 - screens_urls.Count)
			If browser_context_menu_items(4).Checked AndAlso screens_urls.Count < 4 Then HTML_str += TD.Repeat(4 - screens_urls.Count)
			If browser_context_menu_items(5).Checked AndAlso screens_urls.Count < 5 Then HTML_str += TD.Repeat(5 - screens_urls.Count)

			If Not HTML_str.EndsWith("</TR>") Then HTML_str += "</TR>"
			HTML_str += "</table>"
			HTML_str += "<div style='position: absolute; top: 10px; left: 50%; transform: translate(-50%, 0%); -webkit-text-stroke: 1.5px lightblue; -webkit-text-fill-color: black; font-size: 200%;'>" + product_name + "</div></body></html>"
			Me.Invoke(Sub() GeckoWebBrowser1.LoadHtml(HTML_str))
		End If
	End Sub
	Private Function download_file(ByVal url As String, ByVal local_path As String) As String
		Dim wc As New System.Net.WebClient()
		'Dim n As Long = Environment.TickCount 'cette variable n'est pas obligatoire, elle servira à deduire le temps que le téléchargement à pris
		Try
			wc.DownloadFile(url, local_path) : wc.Dispose()
			'Return "Downloaded in " & ((Environment.TickCount - n) / 1000) & " seconds"
			Return ""
		Catch ex As Exception
			Return ex.Message.ToString
		End Try
	End Function

	'Select module folder
	Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
		ComboBox2.Items.Clear()
		Button1.Enabled = False
		Button6.Enabled = False : Button7.Enabled = False : Button13.Enabled = False
		inst = Nothing : method_parse = Nothing : method_preprocess = Nothing

		For Each m In modules(ComboBox1.SelectedItem.ToString)
			ComboBox2.Items.Add(Path.GetFileNameWithoutExtension(m))
		Next
		ComboBox2_SelectedIndexChanged(ComboBox2, New EventArgs)
	End Sub
	'Select module
	Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
		'Set defaults
		GeckoWebBrowser1.Stop()
		Options.all_done = False
		Options.mode = Options.modes.none
		Options.gecko_load_flags = GeckoLoadFlags.None
		Options.input_file_regex = ""
		Options.fields = New List(Of String)
		Options.fields_data = New Dictionary(Of String, String)
		Options.fields_data_arr = New Dictionary(Of String, List(Of String))
		Options.start_page = ""
		Options.additional_files = New List(Of String)
		Options.additional_files_content = New Dictionary(Of Integer, List(Of String))
		Options.progress_cur = 0 : Options.progress_total = 0
		inst = Nothing
		method_parse = Nothing
		method_navigate_next = Nothing
		method_preprocess = Nothing
		input_urls = New List(Of String)
		input_urls_current = -1
		TextBox5.Text = "Not used"
		TextBox6.Text = "Not used"
		GroupBox2.Visible = False
		GroupBox3.Visible = False
		GroupBox4.Visible = False
		GeckoWebBrowser1.Dock = DockStyle.Fill
		Button1.Enabled = False : Button4.Enabled = False : Button5.Enabled = False
		Button6.Enabled = False : Button7.Enabled = False : Button13.Enabled = False
		GeckoWebBrowser1.Tag = Nothing
		GeckoWebBrowser1.ContextMenuStrip = Nothing

		If ComboBox2.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem.ToString = "" Then Exit Sub
		Dim module_path = modules(ComboBox1.SelectedItem.ToString)(ComboBox2.SelectedIndex)

		'Compile
		Dim sourceCode As String = ""
		Dim cp = New CompilerParameters()
		Dim cr As CompilerResults = Nothing
		Dim line_offset = 0
		cp.ReferencedAssemblies.Add("System.dll")
		cp.ReferencedAssemblies.Add("System.Core.dll")
		cp.ReferencedAssemblies.Add("System.Windows.Forms.dll")
		cp.ReferencedAssemblies.Add("Geckofx-Core.dll")
		cp.ReferencedAssemblies.Add("Geckofx-Winforms.dll")
		cp.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll")
		cp.ReferencedAssemblies.Add("System.Runtime.Extensions.dll")
		cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly.Location)
		cp.GenerateExecutable = False
		cp.IncludeDebugInformation = True
		If module_path.ToLower.EndsWith(".cs") Then
			Dim cscp = New CSharpCodeProvider()
			sourceCode = "using Catalog_2016; using Gecko; using System; using System.Collections.Generic; using System.Windows.Forms; using System.Linq;" + vbCrLf
			sourceCode += "public class grb { " + vbCrLf + vbCrLf
			line_offset = sourceCode.Split({vbCrLf}, StringSplitOptions.None).Count
			sourceCode += File.ReadAllText(module_path) + vbCrLf + "}"
			cr = cscp.CompileAssemblyFromSource(cp, sourceCode)
		ElseIf module_path.ToLower.EndsWith(".vb") Then
			Dim vbcp = New VBCodeProvider()
			sourceCode += "Option Compare Binary" + vbCrLf
			sourceCode += "Option Infer On" + vbCrLf
			sourceCode += "Option Strict Off" + vbCrLf
			sourceCode += "Option Explicit On" + vbCrLf
			sourceCode += "Imports Catalog_2016" + vbCrLf
			sourceCode += "Imports Gecko" + vbCrLf
			sourceCode += "Imports System" + vbCrLf
			sourceCode += "Imports System.Collections.Generic" + vbCrLf
			sourceCode += "Imports System.Windows.Forms" + vbCrLf
			sourceCode += "Imports Microsoft.VisualBasic" + vbCrLf
			sourceCode += "Imports System.Linq" + vbCrLf + vbCrLf
			sourceCode += "Public Class grb" + vbCrLf
			line_offset = sourceCode.Split({vbCrLf}, StringSplitOptions.None).Count
			sourceCode += File.ReadAllText(module_path) + vbCrLf
			sourceCode += "End Class"
			cr = vbcp.CompileAssemblyFromSource(cp, sourceCode)
		End If
		If cr.Errors.Count > 0 Then
			Dim msg = "Module compilation error(s)" + vbCrLf
			For Each er As CompilerError In cr.Errors
				msg += "- " + (er.Line - line_offset).ToString + ":" + er.Column.ToString + " - " + er.ErrorText + vbCrLf
			Next
			MsgBox(msg) : Exit Sub
		End If
		Dim compiled_assembly = cr.CompiledAssembly

		'Inject and call setup()
		Dim grb = compiled_assembly.GetType("grb")
		Dim setup_method = grb.GetMethod("setup")
		method_parse = grb.GetMethod("parse")
		method_navigate_next = grb.GetMethod("get_next_url")
		method_preprocess = grb.GetMethod("preprocess")
		inst = compiled_assembly.CreateInstance("grb")

		If setup_method Is Nothing OrElse inst Is Nothing OrElse method_parse Is Nothing Then Exit Sub
		setup_method.Invoke(inst, BindingFlags.InvokeMethod, Nothing, Nothing, Globalization.CultureInfo.CurrentCulture)

		'Load Options From .ini
		refr = True
		Dim module_name = ComboBox1.SelectedItem.ToString().Replace(" ", "") + "_" + ComboBox2.SelectedItem.ToString().Replace(" ", "")
		Dim settings = Form1.ini.IniReadValue("Grabber", module_name + "_mode0").Split({"|"c}, StringSplitOptions.None)
		If settings.Count >= 1 Then
			If settings(0) = "1" Then RadioButton1.Checked = True
			If settings(0) = "2" Then RadioButton2.Checked = True
		End If

		'Start_From_Current_Page checkbox
		CheckBox3.Checked = False
		If Options.mode = Options.modes.start_page Then
			CheckBox3.Enabled = True
			If settings.Count >= 5 Then CheckBox3.Checked = CBool(settings(4))
		Else
			CheckBox3.Enabled = False
		End If

		'Set output file name for auto name
		If settings.Count >= 4 Then CheckBox1.Checked = CBool(settings(3))
		If CheckBox1.Checked Then
			If Options.mode = Options.modes.input_file_direct_download Then
				TextBox4.Text = ".\Grabbers\data\" + ComboBox1.SelectedItem.ToString + " - Screenshots\"
			Else
				TextBox4.Text = ".\Grabbers\data\" + ComboBox1.SelectedItem.ToString + " - " + ComboBox2.SelectedItem.ToString + ".txt"
			End If
		Else
			If settings.Count >= 3 Then TextBox4.Text = settings(2)

			'To refresh additional filenames, if loaded filename is the same as before
			TextBox4_TextChanged(TextBox4, New EventArgs)
		End If

		If Options.mode = Options.modes.input_file Then
			GroupBox2.Visible = True
			Dim settings2 = Form1.ini.IniReadValue("Grabber", module_name + "_mode2").Split({"|"c}, StringSplitOptions.None)
			If settings2.Count >= 1 Then TextBox2.Text = settings2(0)
			If settings2.Count >= 2 Then ComboBox3.SelectedItem = settings2(1)
		ElseIf Options.mode = Options.modes.input_file_direct_download Then
			GroupBox3.Visible = True
			GroupBox4.Visible = True

			Dim b_loc = GeckoWebBrowser1.Location
			Dim b_size = GeckoWebBrowser1.Size
			GeckoWebBrowser1.Dock = DockStyle.None
			GeckoWebBrowser1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
			GeckoWebBrowser1.Location = b_loc : GeckoWebBrowser1.Size = b_size
			GeckoWebBrowser1.Height -= (GroupBox4.Height + 15)

			Label10.Text = "Total Images: 0 / 0"
			Label11.Text = "Current Product: 0 / 0"
			Label12.Text = "Current Image: 0 / 0"
			ProgressBar1.Value = 0
			TextBox12.Text = Options.input_file_regex
			Dim settings3 = Form1.ini.IniReadValue("Grabber", module_name + "_mode3").Split({"|"c}, StringSplitOptions.None)
			If settings3.Count >= 1 Then TextBox7.Text = settings3(0)
			If settings3.Count >= 3 Then CheckBox2.Checked = CBool(settings3(2))
			If settings3.Count >= 4 Then TextBox13.Text = settings3(3)
			If settings3.Count >= 5 Then CheckBox10.Checked = CBool(settings3(4))
			If settings3.Count >= 7 Then CheckBox8.Checked = CBool(settings3(6))
			If settings3.Count >= 8 Then CheckBox9.Checked = CBool(settings3(7))
			If settings3.Count >= 10 Then NumericUpDown2.Value = CDec(settings3(9))
			If settings3.Count >= 12 Then TextBox14.Text = settings3(11)
			If settings3.Count >= 2 Then
				If settings3(1) = "1" Then RadioButton6.Checked = True
				If settings3(1) = "2" Then RadioButton7.Checked = True
			End If
			If settings3.Count >= 6 Then
				If settings3(5) = "1" Then RadioButton3.Checked = True
				If settings3(5) = "2" Then RadioButton4.Checked = True
				If settings3(5) = "3" Then RadioButton5.Checked = True
				If settings3(5) = "4" Then RadioButton12.Checked = True
				If settings3(5) = "5" Then RadioButton13.Checked = True
			End If
			If settings3.Count >= 9 Then
				If settings3(8) = "1" Then RadioButton8.Checked = True
				If settings3(8) = "2" Then RadioButton9.Checked = True
				If settings3(8) = "3" Then RadioButton10.Checked = True
				If settings3(8) = "4" Then RadioButton11.Checked = True
			End If
			If settings3.Count >= 11 Then
				If settings3(10) = "1" Then RadioButton14.Checked = True
				If settings3(10) = "2" Then RadioButton15.Checked = True
				If settings3(10) = "3" Then RadioButton16.Checked = True
			End If
		End If
		refr = False

		For i As Integer = 0 To Options.additional_files.Count - 1
			Options.additional_files_content.Add(i, New List(Of String))
		Next
		For Each f In Options.fields
			Options.fields_data.Add(f, "")
			Options.fields_data_arr.Add(f, New List(Of String))
		Next

		Button1.Enabled = True : Button4.Enabled = True : Button5.Enabled = True
		Button6.Enabled = True : Button7.Enabled = True : Button13.Enabled = True
	End Sub



	'Save settings
	Private Sub OptionsChanged_Mode0(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, TextBox3.TextChanged, TextBox4.TextChanged, CheckBox1.CheckedChanged, CheckBox3.CheckedChanged
		If refr Then Exit Sub
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing Then Exit Sub
		If (sender Is RadioButton1 Or sender Is RadioButton2) AndAlso Not DirectCast(sender, RadioButton).Checked Then Exit Sub
		Dim module_name = ComboBox1.SelectedItem.ToString().Replace(" ", "") + "_" + ComboBox2.SelectedItem.ToString().Replace(" ", "") + "_mode0"

		Dim save_mode = DirectCast(IIf(RadioButton1.Checked, "1", "2"), String)
		Dim save_str = save_mode + "|" + TextBox3.Text.Trim + "|" + TextBox4.Text.Trim + "|" + CheckBox1.Checked.ToString() + "|" + CheckBox3.Checked.ToString()
		Form1.ini.IniWriteValue("Grabber", module_name, save_str)
	End Sub
	Private Sub OptionsChanged_Mode2(sender As Object, e As EventArgs) Handles TextBox2.TextChanged, ComboBox3.SelectedIndexChanged
		If refr Then Exit Sub
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing Then Exit Sub
		Dim module_name = ComboBox1.SelectedItem.ToString().Replace(" ", "") + "_" + ComboBox2.SelectedItem.ToString().Replace(" ", "") + "_mode2"

		Dim save_str = TextBox2.Text.Trim + "|" + ComboBox3.SelectedItem.ToString
		Form1.ini.IniWriteValue("Grabber", module_name, save_str)
	End Sub
	Private Sub OptionsChanged_Mode3(sender As Object, e As EventArgs) Handles TextBox7.TextChanged, RadioButton6.CheckedChanged, RadioButton7.CheckedChanged, CheckBox2.CheckedChanged,
		TextBox13.TextChanged, CheckBox10.CheckedChanged,
		RadioButton3.CheckedChanged, RadioButton4.CheckedChanged, RadioButton5.CheckedChanged, RadioButton12.CheckedChanged, RadioButton13.CheckedChanged, CheckBox8.CheckedChanged, CheckBox9.CheckedChanged,
		RadioButton8.CheckedChanged, RadioButton9.CheckedChanged, RadioButton10.CheckedChanged, RadioButton11.CheckedChanged, NumericUpDown2.ValueChanged,
		TextBox14.TextChanged, RadioButton14.CheckedChanged, RadioButton15.CheckedChanged, RadioButton16.CheckedChanged

		TextBox13.Enabled = CheckBox10.Checked
		CheckBox2.Enabled = RadioButton7.Checked
		NumericUpDown2.Enabled = RadioButton11.Checked : Label20.Enabled = RadioButton11.Checked
		RadioButton14.Enabled = RadioButton11.Checked : RadioButton15.Enabled = RadioButton11.Checked : RadioButton16.Enabled = RadioButton11.Checked
		CheckBox8.Enabled = Not RadioButton3.Checked : CheckBox9.Enabled = Not RadioButton3.Checked

		If refr Then Exit Sub
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing Then Exit Sub
		If sender.GetType() Is GetType(RadioButton) AndAlso Not DirectCast(sender, RadioButton).Checked Then Exit Sub

		Dim module_name = ComboBox1.SelectedItem.ToString().Replace(" ", "") + "_" + ComboBox2.SelectedItem.ToString().Replace(" ", "") + "_mode3"

		Dim input_format = "1"
		If RadioButton7.Checked Then input_format = "2"
		Dim download_mode = "1"
		If RadioButton4.Checked Then download_mode = "2"
		If RadioButton5.Checked Then download_mode = "3"
		If RadioButton12.Checked Then download_mode = "4"
		If RadioButton13.Checked Then download_mode = "5"
		Dim action_failure = "1"
		If RadioButton9.Checked Then action_failure = "2"
		If RadioButton10.Checked Then action_failure = "3"
		If RadioButton11.Checked Then action_failure = "4"
		Dim action_failure2 = "1"
		If RadioButton15.Checked Then action_failure = "2"
		If RadioButton16.Checked Then action_failure = "3"

		Dim save_str = TextBox7.Text.Trim + "|" + input_format + "|" + CheckBox2.Checked.ToString + "|"
		save_str += TextBox13.Text.Trim + "|" + CheckBox10.Checked.ToString + "|"
		save_str += download_mode + "|" + CheckBox8.Checked.ToString + "|" + CheckBox9.Checked.ToString + "|"
		save_str += action_failure + "|" + NumericUpDown2.Value.ToString + "|" + action_failure2 + "|"
		save_str += TextBox14.Text.Trim
		Form1.ini.IniWriteValue("Grabber", module_name, save_str)
	End Sub

	'Input filename changed - adjust additional filenames
	Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged
		If Options.mode <> Options.modes.input_file Then TextBox5.Text = "Not used" : TextBox6.Text = "Not used" : Exit Sub

		Try
			Path.GetFullPath(TextBox4.Text)
			'If we don't have exception here - the path is valid
			Dim p = Path.GetDirectoryName(TextBox4.Text) + "\" + Path.GetFileNameWithoutExtension(TextBox4.Text) + "_"
			If Options.additional_files.Count > 0 Then TextBox5.Text = p + Options.additional_files(0) + ".txt" Else TextBox5.Text = "Not used"
			If Options.additional_files.Count > 1 Then TextBox6.Text = p + Options.additional_files(1) + ".txt" Else TextBox6.Text = "Not used"
		Catch ex As Exception
			'Path is invalid
			If Options.additional_files.Count > 0 Then TextBox5.Text = Options.additional_files(0) + ".txt" Else TextBox5.Text = "Not used"
			If Options.additional_files.Count > 1 Then TextBox6.Text = Options.additional_files(1) + ".txt" Else TextBox6.Text = "Not used"
		End Try
	End Sub
	'Browser url - press enter
	Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
		If e.KeyCode = Keys.Enter Then
			GeckoWebBrowser1.Navigate(TextBox1.Text.Trim)
		End If
	End Sub
	'Input File Auto button - mode 2
	Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
		Dim files = Directory.GetFiles(".\Grabbers\data\", ComboBox1.SelectedItem.ToString() + " - *.txt").ToList
		files.Sort()
		Dim first_letter = ComboBox2.SelectedItem.ToString().Substring(0, 1).ToUpper()
		Dim letter_before = ""
		Dim last_file = ""
		For i As Integer = files.Count - 1 To 0 Step -1
			Dim cmb_len = ComboBox1.SelectedItem.ToString().Length + 3
			If Path.GetFileName(files(i)).Length <= cmb_len Then Continue For

			Dim cur = Path.GetFileName(files(i)).Substring(cmb_len, 1).ToUpper()
			If letter_before <> "" Then
				If cur <> letter_before Then Exit For
				last_file = files(i)
			Else
				If cur < first_letter Then letter_before = cur : last_file = files(i)
			End If
		Next

		If String.IsNullOrWhiteSpace(last_file) Then MsgBox("Could not auto find appropriate input file.") : Exit Sub
		TextBox2.Text = last_file
	End Sub
	'Input File Auto button - mode 3
	Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
		Dim files = Directory.GetFiles(".\Grabbers\data\", ComboBox1.SelectedItem.ToString() + " - *.txt").ToList
		files.Sort()
		Dim first_letter = ComboBox2.SelectedItem.ToString().Substring(0, 1).ToUpper()
		Dim last_file = ""
		For i As Integer = files.Count - 1 To 0 Step -1
			Dim cmb_len = ComboBox1.SelectedItem.ToString().Length + 3
			If Path.GetFileName(files(i)).Length <= cmb_len Then Continue For

			Dim cur = Path.GetFileName(files(i)).Substring(cmb_len, 1).ToUpper()
			If cur < first_letter Then last_file = files(i) : Exit For
		Next

		If String.IsNullOrWhiteSpace(last_file) Then MsgBox("Could not auto find appropriate input file.") : Exit Sub
		TextBox7.Text = last_file
	End Sub
	'Edit Parsed Urls .txt
	Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing Then Exit Sub
		Dim pu = ".\Grabbers\" + ComboBox1.SelectedItem.ToString + "\" + ComboBox2.SelectedItem.ToString + "_parsed_urls.txt"
		If File.Exists(pu) Then Process.Start(pu) Else MsgBox("Parsed url file not found: " + vbCrLf + pu)
	End Sub
	'Clear Parsed Urls .txt
	Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing Then Exit Sub
		Dim pu = ".\Grabbers\" + ComboBox1.SelectedItem.ToString + "\" + ComboBox2.SelectedItem.ToString + "_parsed_urls.txt"
		If File.Exists(pu) Then File.Delete(pu) : MsgBox("Parsed url file deleted: " + vbCrLf + pu) Else MsgBox("Parsed url file not found: " + vbCrLf + pu)
	End Sub
	'Edit Module Script
	Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
		If ComboBox1.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem Is Nothing OrElse ComboBox2.SelectedItem.ToString = "" Then Exit Sub
		Dim module_path = modules(ComboBox1.SelectedItem.ToString)(ComboBox2.SelectedIndex)
		If File.Exists(module_path) Then Process.Start("notepad.exe", module_path) Else MsgBox("File not found: " + vbCrLf + module_path)
	End Sub
	'Edit Input/Output File Button
	Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click, Button18.Click, Button19.Click, Button20.Click, Button21.Click
		Dim f As String = ""
		If sender Is Button17 Then f = TextBox2.Text.Trim
		If sender Is Button18 Then f = TextBox7.Text.Trim
		If sender Is Button19 Then f = TextBox4.Text.Trim
		If sender Is Button20 Then f = TextBox5.Text.Trim
		If sender Is Button21 Then f = TextBox6.Text.Trim
		If File.Exists(f) Then Process.Start("notepad.exe", f) Else MsgBox("File not found: " + vbCrLf + f)
	End Sub
	'Stop/Parse Current Page with Selected Parser Button
	Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
		Button4.Enabled = False
		If Button4.Text = stop_button_text(0) Then
			'Stop mode

			'Work should be reseted from 5 places:
			'- When background thread stops
			'- When parsing are complete
			'- When module error occurs
			'- When form is closed
			'- When STOP button pressed
			If bg_worker_thread IsNot Nothing AndAlso bg_worker_thread.IsAlive Then
				'bg_worker_thread.Abort()
				bg_worker.CancelAsync()
				Button4.Text = stop_button_text(2)
				While bg_worker.IsBusy
					Application.DoEvents()
				End While
			End If
			bg_worker_thread = Nothing

			GeckoWebBrowser1.Stop()
			browser_event_active = False
			wait_timer.Enabled = False
			urls_done = New List(Of String)
			input_urls = New List(Of String)
			input_urls_current = -1

			Options.err = ""
			Options.all_done = False
			Options.wait_request = False
			Options.progress_cur = 0 : Options.progress_total = 0
			Options.fields_data = New Dictionary(Of String, String)
			Options.fields_data_arr = New Dictionary(Of String, List(Of String))
			Options.additional_files_content = New Dictionary(Of Integer, List(Of String))
			Options.links = New List(Of KeyValuePair(Of String, List(Of String)))

			TabControl1.TabPages(0).Enabled = True
			TabControl1.TabPages(2).Enabled = True
			TabControl1.TabPages(3).Enabled = True

			Button4.Text = stop_button_text(1)
		ElseIf Button4.Text = stop_button_text(1) Then
			'Parse Current Page mode
			If inst Is Nothing OrElse method_parse Is Nothing Then MsgBox("The module is not loaded, please load a module.") : Exit Sub
			If GeckoWebBrowser1.IsBusy Then MsgBox("Browser is busy, wait for page to complete loading.") : Exit Sub
			Options.all_done = True
			browser_event_active = True
			GeckoWebBrowser1_DocumentCompleted(GeckoWebBrowser1, New GeckoDocumentCompletedEventArgs(GeckoWebBrowser1.Url, GeckoWebBrowser1.Window))
			browser_event_active = False
			Options.all_done = False
		End If
		Button4.Enabled = True
	End Sub
	'Navigate to Module Start Page Button
	Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
		If inst Is Nothing OrElse method_parse Is Nothing Then MsgBox("The module is not loaded, please load a module.") : Exit Sub
		If String.IsNullOrWhiteSpace(Options.start_page) Then MsgBox("This module have no start page.") : Exit Sub
		GeckoWebBrowser1.Stop()
		GeckoWebBrowser1.Navigate(Options.start_page)
		TabControl1.SelectedTab = TabControl1.TabPages(1)
	End Sub

	'Check Parsed Data / Find Duplicated
	Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
		If Not File.Exists(TextBox10.Text.Trim) Then MsgBox("Input file does not exist: " + vbCrLf + TextBox10.Text.Trim) : Exit Sub

		ProgressBar2.Value = 0
		Label17.Text = "Preparing..." : Label17.Refresh()

		Dim parsed_data = File.ReadAllLines(TextBox10.Text.Trim)
		ProgressBar2.Maximum = parsed_data.Length

		Dim c_total As Integer = 0
		Dim c_uniques As Integer = 0
		Dim c_duplicated_product As Integer = 0
		Dim c_duplicated_entries As Integer = 0
		Dim lines_all As New List(Of String)
		Dim duplicated As New List(Of String)
		For Each entry In parsed_data
			c_total += 1
			If lines_all.Contains(entry.ToUpper) Then
				If duplicated.Contains(entry.ToUpper) Then
					c_duplicated_entries += 1
				Else
					c_duplicated_product += 1
					c_duplicated_entries += 2
					c_uniques -= 1
					duplicated.Add(entry.ToUpper)
				End If
			Else
				c_uniques += 1
				lines_all.Add(entry.ToUpper)
			End If

			'Show Progress
			If c_total Mod 30 = 0 Then
				ProgressBar2.Value = c_total : ProgressBar2.Refresh()
				Label17.Text = c_total.ToString() + " / " + parsed_data.Length.ToString() : Label17.Refresh()
				Application.DoEvents()
			End If
		Next

		Dim counter = 0
		Dim lines_duplicated As New List(Of String)
		Dim lines_no_duplicated As New List(Of String)
		If CheckBox6.Checked Or CheckBox7.Checked Then
			counter += 1
			For Each entry In parsed_data
				If duplicated.Contains(entry.ToUpper) Then
					If CheckBox6.Checked Then
						lines_duplicated.Add(entry)
					End If
				Else
					If CheckBox7.Checked Then
						lines_no_duplicated.Add(entry)
					End If
				End If

				'Show Progress
				If c_total Mod 100 = 0 Then
					Label17.Text = "Saving: " + counter.ToString() + " / " + parsed_data.Length.ToString() : Label17.Refresh()
					Application.DoEvents()
				End If
			Next

			Dim f_path = Path.GetDirectoryName(TextBox10.Text.Trim)
			Dim f_no_ext = Path.GetFileNameWithoutExtension(TextBox10.Text.Trim)
			If File.Exists(f_path + "\" + f_no_ext + "_Dupes.txt") Then File.Delete(f_path + "\" + f_no_ext + "_Dupes.txt")
			If File.Exists(f_path + "\" + f_no_ext + "_NoDupes.txt") Then File.Delete(f_path + "\" + f_no_ext + "_NoDupes.txt")
			If CheckBox6.Checked Then File.WriteAllLines(f_path + "\" + f_no_ext + "_Dupes.txt", lines_duplicated.ToArray())
			If CheckBox7.Checked Then File.WriteAllLines(f_path + "\" + f_no_ext + "_NoDupes.txt", lines_no_duplicated.ToArray())
		End If

		Dim str = "Total: " + c_total.ToString() + vbCrLf + "Uniques: " + c_uniques.ToString() + vbCrLf + "Duplicated products: " + c_duplicated_product.ToString() + vbCrLf + "Duplicated Lines: " + c_duplicated_entries.ToString()
		MsgBox(str)
		Label17.Text = "Idle" : ProgressBar2.Value = 0
	End Sub
	'Check Parsed Data / Find Already in DB
	Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
		If Not File.Exists(TextBox10.Text.Trim) Then MsgBox("Input file does not exist: " + vbCrLf + TextBox10.Text.Trim) : Exit Sub
		If ComboBox5.SelectedIndex < 0 Then MsgBox("Please, set DB field to check against.") : Exit Sub

		ProgressBar2.Value = 0
		Label17.Text = "Preparing..." : Label17.Refresh()

		Dim data_field = ComboBox5.SelectedItem.ToString()
		If data_field.Contains(" ") Then data_field = data_field.Substring(0, data_field.IndexOf(" "))
		Dim ds = db.queryDataset("SELECT DISTINCT " + data_field + " FROM main")
		Dim ds_arr = ds.AsEnumerable.Select(Function(r) r.Field(Of String)(0)).ToArray()

		Dim c_total As Integer = 0
		Dim c_error As Integer = 0
		Dim c_found As Integer = 0
		Dim c_not_found As Integer = 0
		Dim parsed_data = File.ReadAllLines(TextBox10.Text.Trim)
		Dim lines_Found As New List(Of String)
		Dim lines_notFound As New List(Of String)
		ProgressBar2.Maximum = parsed_data.Length
		For Each entry In parsed_data
			c_total += 1
			Dim match As Boolean = False
			If NumericUpDown1.Value >= 0 AndAlso TextBox11.Text <> "" Then
				Dim arr = entry.Split({TextBox11.Text}, StringSplitOptions.None)
				If arr.Length > NumericUpDown1.Value Then
					If ds_arr.Contains(arr(NumericUpDown1.Value)) Then match = True
				Else
					c_error += 1
				End If
			Else
				If ds_arr.Contains(entry) Then match = True
			End If

			If match Then
				c_found += 1
				lines_Found.Add(entry)
			Else
				c_not_found += 1
				lines_notFound.Add(entry)
			End If

			'Show Progress
			If c_total Mod 30 = 0 Then
				ProgressBar2.Value = c_total : ProgressBar2.Refresh()
				Label17.Text = c_total.ToString() + " / " + parsed_data.Length.ToString() : Label17.Refresh()
				Application.DoEvents()
			End If
		Next

		Dim f_path = Path.GetDirectoryName(TextBox10.Text.Trim)
		Dim f_no_ext = Path.GetFileNameWithoutExtension(TextBox10.Text.Trim)
		If File.Exists(f_path + "\" + f_no_ext + "_InDB.txt") Then File.Delete(f_path + "\" + f_no_ext + "_InDB.txt")
		If File.Exists(f_path + "\" + f_no_ext + "_NotInDB.txt") Then File.Delete(f_path + "\" + f_no_ext + "_NotInDB.txt")
		If CheckBox4.Checked Then File.WriteAllLines(f_path + "\" + f_no_ext + "_InDB.txt", lines_Found.ToArray())
		If CheckBox5.Checked Then File.WriteAllLines(f_path + "\" + f_no_ext + "_NotInDB.txt", lines_notFound.ToArray())

		Dim str = "Total: " + c_total.ToString() + ", Found: " + c_found.ToString() + ", Not Found: " + c_not_found.ToString() + ", Errors: " + c_error.ToString()
		MsgBox(str)
		Label17.Text = "Idle" : ProgressBar2.Value = 0
	End Sub
	'Check Parsed Data / Get Input File From Output
	Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
		TextBox10.Text = TextBox4.Text
	End Sub
	'Check Parsed Data / Load Options
	Private Sub CheckParsedData_OptionLoad()
		refr = True
		Dim str = Form1.ini.IniReadValue("Grabber", "CheckParsedData").Split({"|"c}, StringSplitOptions.None)
		If str.Length >= 1 Then TextBox10.Text = str(0)
		If str.Length >= 2 Then CheckBox6.Checked = CBool(str(1))
		If str.Length >= 3 Then CheckBox7.Checked = CBool(str(2))
		If str.Length >= 4 Then NumericUpDown1.Value = CDec(str(3))
		If str.Length >= 5 Then TextBox11.Text = str(4)
		If str.Length >= 6 Then ComboBox5.SelectedItem = str(5)
		If str.Length >= 7 Then CheckBox4.Checked = CBool(str(6))
		If str.Length >= 8 Then CheckBox5.Checked = CBool(str(7))
		refr = False
	End Sub
	'Check Parsed Data / Save Options
	Private Sub CheckParsedData_OptionChanged(sender As Object, e As EventArgs) Handles TextBox10.TextChanged,
		CheckBox6.CheckedChanged, CheckBox7.CheckedChanged,
		NumericUpDown1.ValueChanged, TextBox11.TextChanged, ComboBox5.SelectedIndexChanged, CheckBox4.CheckedChanged, CheckBox5.CheckedChanged

		If Not Me.Visible Then Exit Sub

		If sender Is TextBox10 Then
			If File.Exists(TextBox10.Text.Trim) Then
				Dim f_no_ext = Path.GetFileNameWithoutExtension(TextBox10.Text.Trim)
				CheckBox4.Text = "Save " + f_no_ext + "_InDB.txt"
				CheckBox5.Text = "Save " + f_no_ext + "_NotInDB.txt"
				CheckBox6.Text = "Save " + f_no_ext + "_Dupes.txt"
				CheckBox7.Text = "Save " + f_no_ext + "_NoDupes.txt"
			Else
				CheckBox4.Text = "---" : CheckBox5.Text = "---"
				CheckBox6.Text = "---" : CheckBox7.Text = "---"
			End If
		End If
		If refr Then Exit Sub

		Dim cmb5 = ""
		If ComboBox5.SelectedItem IsNot Nothing Then cmb5 = ComboBox5.SelectedItem.ToString()

		Dim save_str = TextBox10.Text.Trim() + "|"
		save_str += CheckBox6.Checked.ToString() + "|" + CheckBox7.Checked.ToString() + "|"
		save_str += NumericUpDown1.Value.ToString() + "|" + TextBox11.Text + "|" + cmb5 + "|" + CheckBox4.Checked.ToString() + "|" + CheckBox5.Checked.ToString()
		Form1.ini.IniWriteValue("Grabber", "CheckParsedData", save_str)
	End Sub

	'Debugger / Run Script
	Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
		Dim sourceCode As String = ""
		Dim cp = New CompilerParameters()
		Dim cr As CompilerResults = Nothing
		Dim line_offset = 0
		cp.ReferencedAssemblies.Add("System.dll")
		cp.ReferencedAssemblies.Add("System.Core.dll")
		cp.ReferencedAssemblies.Add("System.Windows.Forms.dll")
		cp.ReferencedAssemblies.Add("Geckofx-Core.dll")
		cp.ReferencedAssemblies.Add("Geckofx-Winforms.dll")
		cp.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll")
		cp.ReferencedAssemblies.Add("System.Runtime.Extensions.dll")
		cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly.Location)
		cp.GenerateExecutable = False
		cp.IncludeDebugInformation = True

		Dim vbcp = New VBCodeProvider()
		sourceCode += "Option Compare Binary" + vbCrLf
		sourceCode += "Option Infer On" + vbCrLf
		sourceCode += "Option Strict Off" + vbCrLf
		sourceCode += "Option Explicit On" + vbCrLf
		sourceCode += "Imports Catalog_2016" + vbCrLf
		sourceCode += "Imports Gecko" + vbCrLf
		sourceCode += "Imports System" + vbCrLf
		sourceCode += "Imports System.Windows.Forms" + vbCrLf
		sourceCode += "Imports Microsoft.VisualBasic" + vbCrLf
		sourceCode += "Imports System.Linq" + vbCrLf + vbCrLf
		sourceCode += "Public Class grb" + vbCrLf
		line_offset = sourceCode.Split({vbCrLf}, StringSplitOptions.None).Count
		sourceCode += RichTextBox1.Text.Replace("Options.", "Options_For_Debugger.") + vbCrLf
		sourceCode += "End Class"
		cr = vbcp.CompileAssemblyFromSource(cp, sourceCode)
		If cr.Errors.Count > 0 Then
			Dim err = ""
			For Each ex As CompilerError In cr.Errors
				err += ex.ErrorText + vbCrLf
			Next
			MsgBox(err) : Exit Sub
		End If
		Dim compiled_assembly = cr.CompiledAssembly

		'Inject and call setup()
		Dim grb = compiled_assembly.GetType("grb")
		Dim _setup_method = grb.GetMethod("setup")
		Dim _method_parse = grb.GetMethod("parse")
		Dim _method_navigate_next = grb.GetMethod("get_next_url")
		Dim _inst = compiled_assembly.CreateInstance("grb")
		If _inst Is Nothing Then Exit Sub
		If _setup_method IsNot Nothing Then _setup_method.Invoke(_inst, BindingFlags.InvokeMethod, Nothing, Nothing, Globalization.CultureInfo.CurrentCulture)

		Options_For_Debugger.fields_data.Clear()
		Options_For_Debugger.fields_data_arr.Clear()
		Options_For_Debugger.additional_files_content.Clear()
		Dim html = DirectCast(GeckoWebBrowser1.Document.DocumentElement, GeckoHtmlElement)
		If _method_parse IsNot Nothing Then
			Try
				_method_parse.Invoke(_inst, BindingFlags.InvokeMethod, Nothing, {html, GeckoWebBrowser1.Url.ToString}, Globalization.CultureInfo.CurrentCulture)

				TextBox9.Text = ""
				For Each field In Options_For_Debugger.fields_data.Keys
					TextBox9.Text += field + ": " + Options_For_Debugger.fields_data(field) + vbCrLf
				Next
				TextBox9.Text += vbCrLf

				For Each ac_key In Options_For_Debugger.additional_files_content.Keys
					TextBox9.Text += "Additional Content: " + ac_key.ToString() + vbCrLf
					For Each ac In Options_For_Debugger.additional_files_content(ac_key)
						TextBox9.Text += "  " + ac + vbCrLf
					Next
					TextBox9.Text += vbCrLf
				Next
			Catch ex As Exception
				If ex.InnerException Is Nothing Then
					MsgBox(ex.Message)
				Else
					MsgBox(ex.Message + vbCrLf + ex.InnerException.Message)
				End If
			End Try
		End If
	End Sub
	'Debugger / Add Script Snippet
	Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click, ListBox2.DoubleClick
		If ListBox2.SelectedIndex < 0 Then Exit Sub
		'TextBox8.Paste(ListBox2.SelectedItem.ToString())
		RichTextBox1.SelectionLength = 1
		RichTextBox1.SelectedText = ListBox2.SelectedItem.ToString()
	End Sub
	'Debugger / Load Script
	Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
		Dim f = ComboBox4.Text
		If String.IsNullOrEmpty(f) Then Exit Sub
		f = ".\Grabbers\" + f
		If Not File.Exists(f) Then Exit Sub

		RichTextBox1.Text = File.ReadAllText(f)
	End Sub
	'Debugger / Save Script
	Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
		Dim f = ComboBox4.Text
		If String.IsNullOrEmpty(f) Then Exit Sub
		f = ".\Grabbers\" + f

		Dim dr = Path.GetDirectoryName(f)
		If Not Directory.Exists(dr) Then Directory.CreateDirectory(dr)

		If File.Exists(f) Then
			Dim res = MsgBox("Overwrite file: '" + f + "'?", MsgBoxStyle.YesNo)
			If res = MsgBoxResult.No Then Exit Sub
			File.Delete(f)
		End If

		File.WriteAllText(f, RichTextBox1.Text)
	End Sub


	Private Sub Recreate_Gecko_Brawser()
		GeckoWebBrowser1.Parent.Controls.Remove(GeckoWebBrowser1)
		GeckoWebBrowser1.Window.Dispose()
		GeckoWebBrowser1.Document.Dispose()
		GeckoWebBrowser1.Dispose()
		GeckoWebBrowser1 = Nothing
		GC.Collect()

		GeckoWebBrowser1 = New Gecko.GeckoWebBrowser()
		GeckoWebBrowser1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		GeckoWebBrowser1.ConsoleMessageEventReceivesConsoleLogCalls = True
		GeckoWebBrowser1.FrameEventsPropagateToMainWindow = False
		GeckoWebBrowser1.Location = New System.Drawing.Point(35, 36)
		GeckoWebBrowser1.Name = "GeckoWebBrowser1"
		GeckoWebBrowser1.Size = New System.Drawing.Size(592, 333)
		GeckoWebBrowser1.TabIndex = 0
		GeckoWebBrowser1.UseHttpActivityObserver = False
		Me.Panel1.Controls.Add(Me.GeckoWebBrowser1)
		Options.browser = GeckoWebBrowser1

		GeckoWebBrowser1.Dock = DockStyle.Fill

		AddHandler GeckoWebBrowser1.Navigating, Sub(o As Object, arg As GeckoNavigatingEventArgs) If arg.Uri IsNot Nothing Then TextBox1.Text = arg.Uri.ToString Else TextBox1.Text = ""
	End Sub

	'TEST PARSE
	Dim ajax_wait_counter As Integer = 0
	Public Sub parse(html As GeckoHtmlElement, cur_url As String)

	End Sub

	Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged
		If Dont_Parse Then Exit Sub
		Code_Parser.Parse(RichTextBox1.Text, True)

		Dont_Parse = True
		Dim old_selection = {RichTextBox1.SelectionStart, RichTextBox1.SelectionLength}

		For Each hi In Code_Parser.HighLights
			RichTextBox1.SelectionStart = hi.location.SourceSpan.Start
			RichTextBox1.SelectionLength = hi.location.SourceSpan.Length
			RichTextBox1.SelectionColor = Color.Blue
		Next

		RichTextBox1.SelectionStart = old_selection(0)
		RichTextBox1.SelectionLength = old_selection(1)
		Dont_Parse = False
	End Sub
	Private Sub RichTextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles RichTextBox1.KeyPress
		Dim txt = RichTextBox1.Text.Insert(RichTextBox1.SelectionStart, e.KeyChar)

		Dim chain = ""
		For i As Integer = RichTextBox1.SelectionStart To 0 Step -1
			Dim chr = txt(i)
			If Char.IsWhiteSpace(chr) Then Exit For
			chain = chr + chain
		Next
		Dim chain_arr = chain.Split({"."c}, StringSplitOptions.None)

		Dim chain_last_word = chain_arr(chain_arr.Length - 1).ToUpper()
		chain_arr = chain_arr.Take(chain_arr.Length - 1).ToArray()

		Dim result = Code_Parser.Search_Chain(chain_arr)
		Dim search_list As New List(Of Class50_CodeParser.Classes_Dict)

		If result IsNot Nothing Then search_list.Add(result)
		If chain_arr.Length = 0 Then
			search_list.Add(Code_Parser.user_variables_field)
			search_list.Add(Code_Parser.user_variables_local)
			search_list.AddRange(Class50_CodeParser.default_usings)
		End If

		ListBox3.Items.Clear()
		If search_list.Count > 0 Then
			For Each s_list In search_list
				For Each s In s_list.dict
					If chain_last_word = "" Then
						ListBox3.Items.Add(s.Key)
					Else
						If s.Key.ToUpper().StartsWith(chain_last_word) Then ListBox3.Items.Add(s.Key)
					End If
				Next
			Next
		End If
	End Sub
End Class

Public Class Options
	Public Enum modes
		none
		start_page
		input_file
		input_file_direct_download
	End Enum
	Public Shared mode As modes = modes.none
	Public Shared wait_request As Boolean = False
	Public Shared gecko_load_flags As GeckoLoadFlags = GeckoLoadFlags.None
	Public Shared all_done As Boolean = False

	'All modes
	Public Shared fields As New List(Of String)
	Public Shared fields_data As New Dictionary(Of String, String)
	Public Shared fields_data_arr As New Dictionary(Of String, List(Of String))
	Public Shared additional_files As New List(Of String)
	Public Shared additional_files_content As New Dictionary(Of Integer, List(Of String))
	Public Shared progress_cur = 0
	Public Shared progress_total = 0

	'Start page mode
	Public Shared start_page = ""

	'Input file mode
	Public Shared input_file_regex = ""

	'Direct download mode
	Public Shared links As New List(Of KeyValuePair(Of String, List(Of String)))

	Public Shared browser As GeckoWebBrowser = Nothing
	Public Shared err As String = ""
End Class
Public Class Options_For_Debugger
	Inherits Options

	Public Shared Shadows mode As modes = modes.none
	Public Shared Shadows wait_request As Boolean = False
	Public Shared Shadows gecko_load_flags As GeckoLoadFlags = GeckoLoadFlags.None
	Public Shared Shadows all_done As Boolean = False
	Public Shared Shadows fields_data As New Dictionary_my(Of String, String)
	Public Shared Shadows fields_data_arr As New Dictionary_my(Of String, List(Of String))
	Public Shared Shadows additional_files_content As New Dictionary_my(Of Integer, List(Of String))
	Public Shared Shadows fields As New List(Of String)
	Public Shared Shadows additional_files As New List(Of String)
	Public Shared Shadows progress_cur = 0
	Public Shared Shadows progress_total = 0
	Public Shared Shadows start_page = ""
	Public Shared Shadows input_file_regex = ""
	Public Shared Shadows links As New List(Of KeyValuePair(Of String, List(Of String)))
	Public Shared Shadows browser As GeckoWebBrowser = Nothing
	Public Shared Shadows err As String = ""

	Public Class Dictionary_my(Of TKey, TValue)
		Inherits Dictionary(Of TKey, TValue)

		Default Public Overloads Property Item(ByVal key As TKey) As TValue
			Get
				If Not MyBase.ContainsKey(key) Then MyBase.Add(key, Activator.CreateInstance(Of TValue)())
				Return MyBase.Item(key)
			End Get
			Set(ByVal value As TValue)
				If MyBase.ContainsKey(key) Then
					MyBase.Item(key) = value
				Else
					MyBase.Add(key, value)
				End If
			End Set
		End Property

	End Class
End Class

