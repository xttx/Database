Imports System.Data
Imports System.ComponentModel
Imports System.IO
Imports System.Threading.Tasks
Imports GemBox.Document.Tables

Public Class Form1
	Implements IMessageFilter

	Dim loadScreenRequest As loadScreenRequestStruct = Nothing
	Structure loadScreenRequestStruct
		Dim name As String
		Dim cat As String
		Dim i As Integer
		Dim year As String
	End Structure

	Dim Screenshot_Options As New Screenshot_Options_Info
	Class Screenshot_Options_Info
		Public FirstSpecial As Boolean = False
		Public FirstOptional As Boolean = False
		Public FirstSuffix As String = ""
		Public FirstIndex As Integer = 0
		Public IndexFormat As String = ""
		Public Suffix As String = ""
	End Class

	Friend type As catalog_type
	Dim customListImages As New List(Of Image)
	Dim customListImagesGray As New List(Of Image)
	Dim screenshotPath As String = ""
	Dim descriptionsBaseUrl As String = ""
	Dim delete_confirmation = True
	Dim dosbox_paths As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

	Friend BigPictureBox As New PictureBox With {.Visible = False, .SizeMode = PictureBoxSizeMode.Zoom, .BorderStyle = BorderStyle.FixedSingle}
	Dim liveSearchMenu As New ContextMenu
	Dim labelName As Label
	Dim labelNameEdit As TextBox
	Dim lblcat As Label
	Dim TreeView2 As New TreeView With {.Visible = False, .CheckBoxes = True, .Width = 200, .Height = 300}
	Dim filter_txt_file As New List(Of String)
	Dim WithEvents ReverseSortOrder As ToolStripMenuItem = New ToolStripMenuItem With {.Text = "Reverse Sort Order"}

	Dim listbox_searchAsYouTypeStr As String = ""
	Dim WithEvents listbox_searchAsYouTypeTimer As New Timer With {.Interval = 1000, .Enabled = False}

	Dim refreshing As Boolean = False
	Public Shared ini As New IniFileApi With {.path = Application.StartupPath + "\config.ini"}

	Dim WithEvents bg_scr_loader As New BackgroundWorker() With {.WorkerSupportsCancellation = True}
	Dim WithEvents bg_descr_loader As New BackgroundWorker() With {.WorkerSupportsCancellation = True}
	Dim bg_scr_loader_thread As Threading.Thread = Nothing
	Dim bg_scr_loader_images As New Collections.Concurrent.ConcurrentDictionary(Of Threading.Thread, List(Of Image))
	Dim bg_descr_loader_threads As New List(Of Threading.Thread)

	Dim Form_Loader As Form0_Loader
	Public Event OnFinishedLoading()

	Public Shared NEW_CONTROL_TOP_ADD As Integer = 15
	Public Shared NEW_CONTROL_TOP_MULTIPLIER As Integer = 25
	Public Shared NEW_CONTROL_COL_WIDTH As Integer = 180
	Public Shared NEW_FILTER_TOP_ADD As Integer = 20
	Public Shared NEW_FILTER_TOP_MULTIPLIER As Integer = 25
	Public Shared LABEL_CAT_HEIGHT As Integer = 60

	'Dim override_selected_dosbox As String = ""

	'TODO - new custom tooltip not showing after alt+tab and back
	'TODO - add tooltips to Field Association options form
	'TODO - add tooltips to Group options form
	'TODO - merge screenshots from another folder (all, only new, only different), use crc check
	'TODO - handle exodos v4 different dosbox versions
	'TODO - check all references to db.execute, to find boolean incosistency (1, True, true - are 3 different things)
	'TODO - add 'date' type in sql

	'TODO - Form9_ExportCustoms - Export product_files and related tables from daz catalog
	'TODO - ", the" for game screenshots

	'TODO - Launch submenu items should only react to left click - looks like it's impossible

	'DONE - When no category selected, should show nothing
	'DONE - Eliminate reboot need after importing
	'DONE - Custom lists (favorites, other)
	'DONE - Writeable field changes not yet wroten to db
	'DONE - sorting
	'DONE - Category panel hide mechanism not working as expected
	'DONE - view custom lists
	'DONE - livesearch modes (contain and begin with modes)
	'DONE - Scaner set all 'have' fields to true (actually was not working at all)
	'DONE - Category textbox allways show 'all'
	'DONE PARTIALLY - if item is in multiple categories, it is double in list (a bull's life) - Used SELECT DISTINCT, need check with games with same name
	'DONE - screen-buttons shows screens from 0 (which is second one), not from the first one (which is without number in name)
	'DONE PARTIALLY - Number of items in treeview category list (empty root categoryes shows count of first child)
	'DONE - Category collapse/expand all
	'DONE - Optional check sub categories when checking category
	'DONE - Show item category in fields
	'DONE - bigScreen aspect need to be configured by preloading image and calculating aspect

	'DONE - 'paths' menu options is hidden, if there is no corresponding table
	'DONE - clicking on the label will copy its value to clipboard
	'DONE - live search can now be used to search any field (very slow, though, and have problems with numeric fields)
	'DONE - export 'what-you-see' list
	'DONE - fixed bug with autocreating config file
	'DONE - erase from database
	'DONE - need to write import for custom data
	'DONE - add product to database

	'DONE - lastUsedFilter saved only when filter is added (also need to save when it's removed)
	'DONE - renderosity scanner
	'DONE - install/launch button tooltip with path

	'DONE - Live search crash when using both []
	'DONE - drag & drop path
	'DONE - game scanner need to strip ':', '-' and other non-alphanumeric and bracket and paranthesis
	'DONE - add NAME label

	'DONE - more adequate windows titles
	'DONE - can now handle multiple paths with some name
	'DONE - refresh labels, after show/hide name label
	'DONE - Save 'show name label' and 'options/live search mode' states
	'DONE - Filter presets

	'DONE - status table need to store paths (let's say 7 for begin), done this another way
	'DONE - Make last filter save when "apply" And Not when "add"/"remove"
	'DONE - Need to handle multiple launch buttons
	'DONE PARTIALLY - Add ccd, mdx, img(?) And nrg to image extension (img extension is questionable)
	'DONE PARTIALLY - Only show mdf, if there Is no mds, And the same for ccd And cue (done mds/mdf only, second part is questionable)
	'DONE - Extracting games - need check if there is only one subfolder in archive, And rename it if needed
	'DONE - Status table filter year error
	'DONE - Status table status filter Not working well (maybe other filters too)
	'DONE - when listbox browsing, fields should be reseted before check if selectedindex < 0

	'DONE - configurable spaces between data labels - it is configured in database fields association
	'DONE - filter by num/dec fields >, <, >=, <=
	'DONE - fields order does not work
	'DONE - show text in list field with multiple values (checked combobox)
	'DONE - try to load screenshots in background, because the way it is now - it's slow
	'DONE - after closing fiels association form window by pressing OK, current product is not refreshed
	'DONE - Description fields are broken
	'DONE - noBR layout option in Field Association does not work
	'DONE - links in the description, when not writen as plain text, are not clickable
	'DONE - filter by empty / not empty field (for non-list fields it seems to already work)
	'DONE - collapse multiple empties in filter comboboxes to single one (null, '', whitespaces)
	'DONE - why there are two 'true' in have filter? - because 1, true and TRUE are considered as different values, added option to fix, need to normalize in filter
	'DONE PARTIALLY - deleting cache_list in field association can throw System.Data.SQLite.SQLiteException: 'database table is locked: database table Is locked'. Fixed by close and reopen connection. It can probably lead to wrong behavior
	'DONE - when closing Field association form, after doing some operation that require refresh (i.e. create/delete cache), need to send 'ok' to refresh controls
	'DONE - after closing groups options, filter comboboxes not refresh
	'DONE - changing "Show path filter" in options require reboot
	'DONE - refresh custom lists icon require reboot (Do I mean if it's currently highlighted i.e. current product added to this list? It it's the case - just need to update the product (done), otherwise can not reproduce)
	'DONE - choose catalog to load, on form0 if config was not set by argument
	'DONE - when only default category exist, unchecking it does not change category textbox from [all] to [none]
	'DONE - old unreal filter (artist not in 'starting with x' group) prevent new entries to show in new database - null was not added to NOT filters
	'DONE - new entry is sometimes added in the middle of the list and sometimes at the end (Because if it's added to default category and it's not shown, it will be set to shown and the list will update and rearrange) it's easy to lost it, need to select it after adding
	'DONE - when edit a entry, need to deny to remove all categories
	'DONE - add new button adds a entry without category, and after clicking a different category you can not see those entries with empty category before reboot, when new 'none' directory is added
	'DONE - SearchOnType does not work with numbers on numpad
	'DONE - form0-loader should autoselect the catalog in autorun, when holding shift
	'DONE - games, 1c space collection, click Parkan link in description - error, because there is no file "/games/parkan", need to add base url before link
	'DONE - configurable baseUrlForDescriptions
	'DONE - delete product button, with configurable confirmation
	'DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)

	'INIT
	Public Sub New(loader As Form0_Loader)

		' This call is required by the designer.
		InitializeComponent()

		'Test custom messagebox
		'MsgBoxEx("test1", "OK", "", "Always do")
		'MsgBoxEx("DONEa-adeleteaproductabuttonaneedacheckasomeatablesaifaexistsa(iaeaaafiles'awhichaisanotapresentainagames)" + vbCrLf + "DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games) DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)", "OK")
		'MsgBoxEx("DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)" + vbCrLf + "DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games) DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)", "OK")
		'MsgBoxEx("DONEa-adeleteaproductabuttonaneedacheckasomeatablesaifaexistsa(iaeaaafiles'awhichaisanotapresentainagames)" + vbCrLf + "DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games) DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)", "OK")
		'MsgBoxEx("DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)" + vbCrLf + "DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games) DONE - delete product button need check some tables if exists (i.e. 'files' which is not present in games)", "OK")

		' Add any initialization after the InitializeComponent() call.
		'Dim bf = Reflection.BindingFlags.SetProperty Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer1.Panel1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer1.Panel2, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer2, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer2.Panel1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer2.Panel2, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer3, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer3.Panel1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, SplitContainer3.Panel2, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, TableLayoutPanel1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, GroupBox1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, Panel1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, PictureBox1, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, PictureBox2, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, PictureBox3, {True})
		'GetType(Control).InvokeMember("DoubleBuffered", bf, Nothing, PictureBox4, {True})

		'TEST REGEXP
		'Dim r As New System.Text.RegularExpressions.Regex("(\[autoexec\][\s\S]*?)(\n|\r)(@?)(mount)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
		'Dim aaa = IO.File.ReadAllText("Z:\G\eXoDOS 2.0\Games\!dos\airball\dosbox.conf")
		'aaa = r.Replace(aaa, "$1$2$3REPLACEMENT", 1)
		'END TEST REGEXP

		Form_Loader = loader
		Dim tmp = Me.Handle 'This is needed to create window handle (descriptor) before the window is shown
		Dim bg As New BackgroundWorker
		AddHandler bg.DoWork, AddressOf Form1_LoadBG
		bg.RunWorkerAsync()
	End Sub
	Private Sub Form1_LoadBG()
		'Me.Invoke(Sub() Me.Visible = False)
		GemBox.Document.ComponentInfo.SetLicense("DTJX-2LSB-QJV3-R3XP") '2.7 version
		'GemBox.Document.ComponentInfo.SetLicense("DH5L-ED6Q-R7O0-DY0H") '3.1 version

		'Rename log file, if it's grater then 1mb
		If IO.File.Exists(LOG_FILE_PATH) AndAlso New IO.FileInfo(LOG_FILE_PATH).Length >= 1 * 1024 * 1024 Then IO.File.Move(LOG_FILE_PATH, LOG_FILE_PATH + "-" + Format(DateTime.Now, "yyyy-MM-dd-HH-mm-ss"))

		Dim t = DateTime.Now
		log("-----------------------------------------------------------")
		log("Catalog started")

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(1, 6, "Reading options..."))
		Dim tmp As String
		tmp = ini.IniReadValue("Main", "CatalogType")
		If tmp.ToUpper = "GAMES" Then
			type = catalog_type.games
			Me.Invoke(Sub() Me.Text = "Catalog - Games")
			'fieldCountArr = {15, 5, 3, 8, 5}
		ElseIf tmp.ToUpper = "UNITY" Then
			type = catalog_type.unity
			Me.Invoke(Sub() Me.Text = "Catalog - Unity")
			'fieldCountArr = {10, 10, 10, 10, 10}
		ElseIf tmp.ToUpper = "DAZ" Then
			type = catalog_type.daz
			Me.Invoke(Sub() Me.Text = "Catalog - Daz3D")
			'fieldCountArr = {10, 10, 10, 10, 10}
		Else
			type = catalog_type.generic
			Me.Invoke(Sub() Me.Text = "Catalog - Generic")
		End If

		tmp = ini.IniReadValue("Main", "Database")
		If tmp <> "" Then path_db = tmp

		tmp = ini.IniReadValue("Interface", "LastOpenedPos")
		If tmp <> "" Then
			Me.Invoke(Sub() Me.Left = CInt(tmp.Split("x")(0)))
			Me.Invoke(Sub() Me.Top = CInt(tmp.Split("x")(1)))
		End If
		tmp = ini.IniReadValue("Interface", "LastOpenedSize")
		If tmp <> "" Then
			Me.Invoke(Sub() Me.Width = CInt(tmp.Split("x")(0)))
			Me.Invoke(Sub() Me.Height = CInt(tmp.Split("x")(1)))
		End If

		Dim ws As FormWindowState
		If [Enum].TryParse(ini.IniReadValue("Interface", "LastOpenedState"), True, ws) Then
			If ws = FormWindowState.Maximized Then Me.WindowState = FormWindowState.Maximized
		End If

		tmp = ini.IniReadValue("Interface", "FieldsLayout_TopOffset")
		If tmp <> "" AndAlso IsNumeric(tmp) Then NEW_CONTROL_TOP_ADD = CInt(tmp)
		tmp = ini.IniReadValue("Interface", "FieldsLayout_VerticalGap")
		If tmp <> "" AndAlso IsNumeric(tmp) Then NEW_CONTROL_TOP_MULTIPLIER = CInt(tmp)
		tmp = ini.IniReadValue("Interface", "FieldsLayout_ColumnWidth")
		If tmp <> "" AndAlso IsNumeric(tmp) Then NEW_CONTROL_COL_WIDTH = CInt(tmp)
		tmp = ini.IniReadValue("Interface", "ListFontSize")
		If tmp <> "" AndAlso IsNumeric(tmp) Then Me.Invoke(Sub() ListBox1.Font = New Font(ListBox1.Font.FontFamily, CSng(tmp)))
		tmp = ini.IniReadValue("Interface", "Splitter1")
		If tmp <> "" AndAlso IsNumeric(tmp) Then Me.Invoke(Sub() SplitContainer1.SplitterDistance = CInt(tmp))
		tmp = ini.IniReadValue("Interface", "Splitter2")
		If tmp <> "" AndAlso IsNumeric(tmp) Then Me.Invoke(Sub() SplitContainer2.SplitterDistance = CInt(tmp))
		tmp = ini.IniReadValue("Interface", "ShowNameLabel")
		If tmp <> "" Then
			'If tmp <> "" And tmp.ToUpper <> "FALSE" And tmp.ToUpper <> "0" Then Me.Invoke(Sub() ShowNameLabelforEasyCopyingToolStripMenuItem.Checked = True)
			'If tmp.ToUpper <> "FALSE" And tmp.ToUpper <> "0" Then ShowNameLabelforEasyCopyingToolStripMenuItem.Checked = True
			Dim b1 As Boolean = False, b2 As Boolean = False
			If tmp.ToUpper = "1" Then b1 = True : b2 = False
			If tmp.ToUpper = "2" Then b1 = False : b2 = True
			Me.Invoke(Sub() ShowNameLabelforEasyCopyingToolStripMenuItem.Checked = b1)
			Me.Invoke(Sub() EditNameToolStripMenuItem.Checked = b2)
		End If
		tmp = ini.IniReadValue("Interface", "LiveSearchMode")
		If tmp.ToUpper = "STARTWITH" Then Me.Invoke(Sub() ContainToolStripMenuItem.Checked = False) : Me.Invoke(Sub() StartWithToolStripMenuItem.Checked = True)
		tmp = ini.IniReadValue("FilterPresets", "Names")
		For Each f In tmp.Split({"%%%"}, StringSplitOptions.RemoveEmptyEntries)
			Preset_ComboBox.Items.Add(f)
		Next

		'Decompress last selected categories
		Dim cat_save_lst As New List(Of Integer)
		Dim cat_save_str = ini.IniReadValue("Interface", "LastCheckedCategories")
		'Try to 
		If cat_save_str.Length > 0 Then
			Dim dbytes = Convert.FromBase64String(cat_save_str)
			Dim decomp_bin = ""
			Dim decomp_str = ""
			For i = 0 To dbytes.Length - 1
				decomp_bin += Convert.ToString(dbytes(i), 2).PadLeft(8, "0"c)
			Next
			decomp_bin = decomp_bin.Substring(decomp_bin.Length Mod 9)
			For i = 0 To decomp_bin.Length - 1 Step 9
				cat_save_lst.Add(Convert.ToInt32(decomp_bin.Substring(i, 9), 2))
			Next
		End If

		'AddHandler RichTextBox1.LinkClicked, Sub(o, e) Process.Start(e.LinkText)
		'AddHandler RichTextBox2.LinkClicked, Sub(o, e) Process.Start(e.LinkText)
		'AddHandler RichTextBox3.LinkClicked, Sub(o, e) Process.Start(e.LinkText)
		'AddHandler ReverseSortOrder.Click, Sub() ReverseSortOrder.Checked = Not ReverseSortOrder.Checked

		AddHandler TreeView2.BeforeCheck, Sub(o, e)
											  If refreshing Then Exit Sub
											  If e.Node.Checked = False Then Exit Sub
											  Dim count = TreeView2.Nodes.GetAllNodesRecur2().Where(Function(n) n.Checked).Count
											  If count = 1 Then MsgBox("You can not remove last category - the entry needs to be at least in one category.") : e.Cancel = True
										  End Sub
		AddHandler TreeView2.AfterCheck, AddressOf Update_Entry_Category
		AddHandler TreeView2.MouseDown, AddressOf Edit_Category_Context_Menu
		Me.Invoke(Sub() Me.Controls.Add(TreeView2))

		Dim ts = DateTime.Now - t : log("Initialization - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(2, 6, "Init options...")) : init_options()
		ts = DateTime.Now - t : log("Init_Options() - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(3, 6, "Connect to database...")) : db.connect()
		ts = DateTime.Now - t : log("db.connect() - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now
		If db.queryDataset("SELECT name FROM sqlite_master WHERE type='table' AND name='paths'").Rows.Count = 0 Then Me.Invoke(Sub() PathsToolStripMenuItem.Visible = False)

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(4, 6, "Init fieldset...")) : Me.Invoke(Sub() init_fieldset())
		ts = DateTime.Now - t : log("init_fieldset() - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(5, 6, "Init custom lists...")) : Me.Invoke(Sub() init_customLists())
		ts = DateTime.Now - t : log("init_customLists() - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now

		Form_Loader.Invoke(Sub() Form_Loader.changeProgress(6, 6, "Init categories...")) : init_categories(cat_save_lst)
		ts = DateTime.Now - t : log("init_categories() - " + ts.Seconds.ToString + "." + ts.Milliseconds.ToString) : t = DateTime.Now

		Form_Loader.Invoke(Sub() init_index_menu())

		RaiseEvent OnFinishedLoading()

		Me.Invoke(Sub() Me.Controls.Add(BigPictureBox))
		Me.Invoke(Sub() BigPictureBox.BringToFront())
		Me.Invoke(Sub() Me.Controls.Add(Progress_GroupBox))
		Me.Invoke(Sub() Progress_GroupBox.BringToFront())
		Me.Invoke(Sub() Progress_GroupBox.Size = New Size(Me.Width - 60, Progress_GroupBox.Height))
	End Sub
	Public Sub init_options()
		descriptionsBaseUrl = ini.IniReadValue("Main", "DescriptionsBaseUrl", True)
		Dim t = ini.IniReadValue("Main", "DeleteConfirm")
		If t = "0" OrElse t.ToUpper = "FALSE" Then delete_confirmation = False

		screenshotPath = ini.IniReadValue("Paths", "Screenshots")
		If Not screenshotPath.EndsWith("\") Then screenshotPath += "\"
		If Not My.Computer.FileSystem.DirectoryExists(screenshotPath) Then screenshotPath = ""

		'Screenshot options
		t = ini.IniReadValue("Screenshots", "FirstSpecial")
		If t <> "" Then Screenshot_Options.FirstSpecial = Boolean.Parse(t)
		t = ini.IniReadValue("Screenshots", "FirstOptional")
		If t <> "" Then Screenshot_Options.FirstOptional = t
		t = ini.IniReadValue("Screenshots", "FirstIndex")
		If t <> "" Then Screenshot_Options.FirstIndex = Integer.Parse(t)
		Screenshot_Options.FirstSuffix = ini.IniReadValue("Screenshots", "FirstSuffix")
		Screenshot_Options.Suffix = ini.IniReadValue("Screenshots", "Suffix")
		Screenshot_Options.IndexFormat = ini.IniReadValue("Screenshots", "IndexFormat")

		'Dosbox Paths
		dosbox_paths.Clear()
		Dim dosBoxPaths As String = ini.IniReadValue("Paths", "DosBoxPaths")
		If dosBoxPaths <> "" Then
			For Each i In dosBoxPaths.Split({";"}, StringSplitOptions.RemoveEmptyEntries)
				Dim arr = i.Split({"|"}, StringSplitOptions.RemoveEmptyEntries)
				While dosbox_paths.ContainsKey(arr(0))
					arr(0) = arr(0) + "[a]"
				End While
				If arr.Count = 2 Then dosbox_paths.Add(arr(0), arr(1))
			Next
		End If

		'Hide menus
		Dim HideMenus = ini.IniReadValue("Interface", "HideMenus").ToUpper.Split({","}, StringSplitOptions.RemoveEmptyEntries)
		For Each m As ToolStripMenuItem In MenuStrip1.Items.OfType(Of ToolStripMenuItem)
			If m.Tag IsNot Nothing AndAlso m.Tag.ToString = "FORCE_HIDE" Then Continue For
			If HideMenus.Contains(m.Name.ToUpper) Then m.Visible = False Else m.Visible = True

			For Each sub_item As ToolStripMenuItem In m.DropDownItems.OfType(Of ToolStripMenuItem)
				If sub_item.Tag IsNot Nothing AndAlso sub_item.Tag.ToString = "FORCE_HIDE" Then Continue For
				If HideMenus.Contains(sub_item.Name.ToUpper) Then sub_item.Visible = False Else sub_item.Visible = True
			Next
		Next
	End Sub
	Public Sub init_fieldset(Optional onlyRebuildVisibleFields = False)
		Dim tsi As ToolStripMenuItem = Nothing
		If Not onlyRebuildVisibleFields Then
			'Remove all from live search context menu
			liveSearchMenu.MenuItems.Clear()
			Dim m As New MenuItem With {.Text = "Product Name"}
			m.Checked = True
			AddHandler m.Click, Sub(o As Object, e As EventArgs)
									For Each mi In liveSearchMenu.MenuItems
										mi.Checked = False
									Next
									DirectCast(o, MenuItem).Checked = True
									TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(Nothing))
									If liveSearchMenu.MenuItems(0).Checked Then TextBox1_TextChanged(TextBox1, New EventArgs)
								End Sub
			liveSearchMenu.MenuItems.Add(m)

			'Remove all items from sort menu
			For i As Integer = SortToolStripMenuItem.DropDownItems.Count - 1 To 0 Step -1
				SortToolStripMenuItem.DropDownItems.RemoveAt(i)
			Next
			tsi = DirectCast(SortToolStripMenuItem.DropDownItems.Add("Name"), ToolStripMenuItem)
			tsi.Tag = "name" : tsi.Checked = True
			AddHandler tsi.Click, AddressOf sort

			'Remove all controls from filter panel
			For i As Integer = GroupBox1.Controls.Count - 1 To 0 Step -1
				If GroupBox1.Controls(i).Name.ToUpper = "BUTTON4" Then Continue For
				If GroupBox1.Controls(i).Name.ToUpper = "BUTTON5" Then Continue For
				If GroupBox1.Controls(i).Name.ToUpper = "BUTTON6" Then Continue For
				If GroupBox1.Controls(i).Name.ToUpper = "LISTBOX2" Then Continue For
				If GroupBox1.Controls(i).Name.ToUpper = "LABEL1" Then Continue For
				If GroupBox1.Controls(i).Name.ToUpper.StartsWith("FILTERBTN") Then RemoveHandler CType(GroupBox1.Controls(i), Button).Click, AddressOf filter_add_handler
				If GroupBox1.Controls(i).Name.ToUpper.StartsWith("PRESET_") Then Continue For
				GroupBox1.Controls.RemoveAt(i)
			Next
		End If

		'Remove all labels/textbox/checkboxes/buttons from Panel2 (fields & data main panel)
		Dim ctrl_to_remove As New List(Of Control)
		For Each lbl In SplitContainer3.Panel1.Controls.OfType(Of Label)()
			ctrl_to_remove.Add(lbl)
		Next
		For Each txt In SplitContainer3.Panel1.Controls.OfType(Of TextBox)()
			ctrl_to_remove.Add(txt)
		Next
		For Each chk In SplitContainer3.Panel1.Controls.OfType(Of CheckBox)()
			ctrl_to_remove.Add(chk)
		Next
		For Each cmb In SplitContainer3.Panel1.Controls.OfType(Of ComboBox)()
			ctrl_to_remove.Add(cmb)
		Next
		For Each btn In SplitContainer3.Panel1.Controls.OfType(Of Button)()
			ctrl_to_remove.Add(btn)
		Next
		For Each ctrl In ctrl_to_remove
			SplitContainer3.Panel1.Controls.Remove(ctrl)
		Next
		'Remove End

		'Add category label
		lblcat = New Label
		lblcat.Name = "LabelCat" : lblcat.Text = "Category:"
		lblcat.AutoSize = True
		SplitContainer3.Panel1.Controls.Add(lblcat)
		lblcat.Left = 10 : lblcat.Top = NEW_CONTROL_TOP_ADD

		lblcat = New Label
		lblcat.Name = "LabelCatData" : lblcat.Text = ""
		lblcat.AutoSize = False
		lblcat.Width = 250 : lblcat.Height = LABEL_CAT_HEIGHT
		lblcat.BorderStyle = BorderStyle.FixedSingle
		SplitContainer3.Panel1.Controls.Add(lblcat)
		lblcat.Left = 80 : lblcat.Top = NEW_CONTROL_TOP_ADD
		AddHandler lblcat.Click, AddressOf Show_Category_Edit_Tree

		'Add Name label if needed
		Dim Field_Row As Integer = 0
		Dim Field_Col As Integer = 0
		labelName = Nothing
		labelNameEdit = Nothing
		If ShowNameLabelforEasyCopyingToolStripMenuItem.Checked Then
			'Show no-editable name
			labelName = New Label
			labelName.Cursor = Cursors.Hand
			labelName.Name = "LabelName"
			labelName.AutoSize = True
			labelName.Text = "Name: "
			SplitContainer3.Panel1.Controls.Add(labelName)
			labelName.Left = 10
			labelName.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD + lblcat.Bottom
			AddHandler labelName.Click, Sub(o As Object, e As EventArgs)
											Dim txt = DirectCast(o, Label).Text
											Dim ind = txt.IndexOf(":")
											If ind > -1 And ind < txt.Length - 1 Then txt = txt.Substring(ind + 1).Trim
											If Not String.IsNullOrEmpty(txt) Then Clipboard.SetText(txt)
										End Sub
			Field_Row += 1
		ElseIf EditNameToolStripMenuItem.Checked Then
			'Show editable name
			labelNameEdit = New TextBox
			SplitContainer3.Panel1.Controls.Add(labelNameEdit)
			labelNameEdit.Left = 10
			labelNameEdit.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD - 4 + lblcat.Bottom
			labelNameEdit.Width = 300
			AddHandler labelNameEdit.TextChanged, Sub()
													  If refreshing Then Exit Sub
													  If ListBox1.SelectedIndex < 0 Then Exit Sub

													  'Update DB
													  Dim sql = "UPDATE main SET name = '" + labelNameEdit.Text.Trim.Replace("'", "''") + "' "
													  sql += "WHERE id = " + ListBox1.SelectedValue.ToString
													  db.execute(sql)

													  'Update listbox
													  refreshing = True
													  CType(ListBox1.SelectedItem, DataRowView).Row.Item("name") = labelNameEdit.Text.Trim
													  refreshing = False
												  End Sub
			Field_Row += 1
		End If

		'Read ini - Fill fields array
		Fields.Clear()
		Fields_ordered.Clear()
		Dim f_counter = 1
		Dim f_type_counter = 0
		For Each fieldType In fieldTypeArr
			For i As Integer = 1 To fieldCountArr(f_type_counter)
				'New Method
				Dim field_data = ini.IniReadValue("Interface", fieldType + i.ToString).Split({"|||"}, StringSplitOptions.None)
				Dim name = ""
				Dim w = ""
				Dim l = ""
				Dim n = ""
				Dim s = ""
				Dim f = ""
				If field_data.Count > 0 Then name = field_data(0).Trim 'Name
				If field_data.Count > 1 Then w = field_data(1) 'Writeable
				If field_data.Count > 2 Then s = field_data(2) 'Sortable
				If field_data.Count > 3 Then f = field_data(3) 'Filtrable
				If field_data.Count > 4 Then l = field_data(4) 'IsList
				If field_data.Count > 5 Then n = field_data(5) 'IsLink

				'Old Method
				'Dim name = ini.IniReadValue("Interface", fieldType + i.ToString)
				'Dim w = ini.IniReadValue("Interface", fieldType + i.ToString + "_write")
				'Dim l = ini.IniReadValue("Interface", fieldType + i.ToString + "_isList")
				'Dim n = ini.IniReadValue("Interface", fieldType + i.ToString + "_isLink")
				'Dim s = ini.IniReadValue("Interface", fieldType + i.ToString + "_sortable")
				'Dim f = ini.IniReadValue("Interface", fieldType + i.ToString + "_filtrable")
				Dim lv = ini.IniReadValue("Interface", fieldType + i.ToString + "_listValues")
				Dim fi As New Field_Info()
				fi.enabled = name <> ""
				fi.name = name
				If fieldType.ToLower().EndsWith("_str") Then fi.field_type = Field_Info.field_types.str
				If fieldType.ToLower().EndsWith("_num") Then fi.field_type = Field_Info.field_types.num
				If fieldType.ToLower().EndsWith("_dec") Then fi.field_type = Field_Info.field_types.dec
				If fieldType.ToLower().EndsWith("_bool") Then fi.field_type = Field_Info.field_types.bool
				If fieldType.ToLower().EndsWith("_txt") Then fi.field_type = Field_Info.field_types.txt
				fi.DBname = "data_" + fi.field_type.ToString() + i.ToString()
				fi.index_of_total = f_counter
				fi.index_of_type = i
				fi.writable = w.ToUpper = "TRUE" Or w = "1"
				fi.sortable = s.ToUpper = "TRUE" Or s = "1"
				fi.filtrable = f.ToUpper = "TRUE" Or f = "1"
				fi.is_nameLink = n.ToUpper = "TRUE" Or n = "1"
				fi.is_list = l.ToUpper = "TRUE" Or l = "1"
				fi.list_values = lv.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
				Fields.Add(fi)

				f_counter += 1
			Next
			f_type_counter += 1
		Next

		'Read ini - Fill ordered fields array
		Dim order = ini.IniReadValue("Interface", "Field_order")
		For Each f In order.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
			Dim index As Integer = -1
			If Not Integer.TryParse(f, index) Then Continue For
			index -= 1
			If index >= Fields.Count Then Continue For
			Fields_ordered.Add(Fields(index))
		Next
		'Add all fields, that have no order for whatever reason
		Fields_ordered.AddRange(Fields.Where(Function(fi) Not Fields_ordered.Contains(fi)))

		'Add label to main panel, filter panel, sort menu, and live filter
		Dim filterCount As Integer = 0
		Dim Layout_NoBR = ini.IniReadValue("Interface", "FieldsLayout_NoBR").Trim.ToUpper
		For Each field In Fields_ordered.Where(Function(f) f.enabled)
			If field.field_type <> Field_Info.field_types.txt Then
				'Add label to main panel
				If field.field_type <> Field_Info.field_types.bool Then
					'IF NOT BOOL (means text or number) - print label
					Dim lbl = New Label
					lbl.Cursor = Cursors.Hand
					lbl.Name = "Label" + field.index_of_total.ToString()
					lbl.AutoSize = True
					lbl.Text = field.name
					SplitContainer3.Panel1.Controls.Add(lbl)
					lbl.Left = 10 + (Field_Col * NEW_CONTROL_COL_WIDTH)
					lbl.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD + lblcat.Bottom
					AddHandler lbl.MouseClick, AddressOf field_click_handler
					field.assoc_lbl = lbl
					If field.writable And field.list_values.Count > 0 Then
						'IF writeable AND ListValues filled - show combobox
						Dim cmb As New ComboBox
						cmb.Name = "Cmb_bx" + field.index_of_total.ToString()
						cmb.DropDownStyle = ComboBoxStyle.DropDownList
						For Each item In field.list_values
							If Not item.Trim.ToUpper = "{MULTIPLE}" Then
								cmb.Items.Add(item)
							End If
						Next

						If Not field.list_values(field.list_values.Count - 1).ToUpper.EndsWith("{MULTIPLE}") Then
							'Standard combobox
							AddHandler cmb.SelectedIndexChanged, AddressOf fieldset_change_handler
							cmb.Left = 150 + (Field_Col * NEW_CONTROL_COL_WIDTH)
							cmb.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD - 4 + lblcat.Bottom
							SplitContainer3.Panel1.Controls.Add(cmb)

							'Add reset button to combobox
							Dim b As New Button
							b.Name = "Cmb_bx_reset" + field.index_of_total.ToString() : b.Text = "R"
							b.Top = cmb.Top - 1 : b.Left = cmb.Right + 5
							b.Width = cmb.Height + 2 : b.Height = cmb.Height + 2
							SplitContainer3.Panel1.Controls.Add(b)
							AddHandler b.Click, Sub() cmb.SelectedIndex = -1
						Else
							'Checked combobox with multiple choices
							cmb = New checkedCombo(cmb)
							cmb.Name = "CCmb_bx" + field.index_of_total.ToString()
							cmb.Left = 150 + (Field_Col * NEW_CONTROL_COL_WIDTH)
							cmb.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD - 4 + lblcat.Bottom
							AddHandler DirectCast(cmb, checkedCombo).checkedChanged, AddressOf fieldset_change_handler
							SplitContainer3.Panel1.Controls.Add(cmb)
						End If

						field.assoc_cmb = cmb
					ElseIf field.writable Then
						'IF writeable AND ListValues NOT filled - show textbox
						Dim txt As New TextBox
						txt.Name = "Txt_bx" + field.index_of_total.ToString()
						SplitContainer3.Panel1.Controls.Add(txt)
						txt.Left = 150 + (Field_Col * NEW_CONTROL_COL_WIDTH)
						txt.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD - 4 + lblcat.Bottom
						field.assoc_txt = txt
						AddHandler txt.TextChanged, AddressOf fieldset_change_handler
					End If
				Else
					'If bool
					Dim chk As New CheckBox
					chk.Name = "Check" + field.index_of_total.ToString()
					chk.AutoSize = False
					chk.Width = 100
					chk.Text = field.name + ":"
					chk.CheckAlign = ContentAlignment.MiddleRight
					SplitContainer3.Panel1.Controls.Add(chk)
					chk.Left = 8 + (Field_Col * NEW_CONTROL_COL_WIDTH)
					chk.Top = ((Field_Row) * NEW_CONTROL_TOP_MULTIPLIER) + NEW_CONTROL_TOP_ADD + lblcat.Bottom - 4
					field.assoc_chk = chk
					AddHandler chk.CheckedChanged, AddressOf fieldset_change_handler
				End If

				If Not onlyRebuildVisibleFields Then
					'If sortable add to sort menu
					If field.sortable Then
						tsi = DirectCast(SortToolStripMenuItem.DropDownItems.Add(field.name), ToolStripMenuItem)
						tsi.Tag = field.DBname
						AddHandler tsi.Click, AddressOf sort
					End If

					'If filtrable add to filters
					If field.filtrable Then
						Dim lbl As New Label With {.Name = "filterLabel" + filterCount.ToString, .Text = field.name + ":"}
						GroupBox1.Controls.Add(lbl)
						lbl.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount)
						lbl.Left = 6

						If (field.field_type = Field_Info.field_types.num OrElse field.field_type = Field_Info.field_types.dec) AndAlso field.name.ToUpper <> "YEAR" Then
							'Filter for numeric types (_num and _dec excluding year)
							Dim cmb_num As New ComboBox With {.Name = "filterNumCmb" + filterCount.ToString, .DropDownStyle = ComboBoxStyle.DropDownList}
							cmb_num.Anchor = AnchorStyles.Top Or AnchorStyles.Left
							GroupBox1.Controls.Add(cmb_num)
							cmb_num.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
							cmb_num.Left = 110
							cmb_num.Width = 30
							cmb_num.Items.AddRange({"=", "!=", "<=", ">=", "<", ">"})
							cmb_num.SelectedIndex = 0

							Dim num As New NumericUpDown With {.Name = "filterNum" + filterCount.ToString, .Maximum = 100000}
							num.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
							GroupBox1.Controls.Add(num)
							num.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
							num.Left = cmb_num.Left + cmb_num.Width + 10
							num.Width = GroupBox1.Width - 240
							If field.field_type = Field_Info.field_types.dec Then num.DecimalPlaces = 2 : num.ThousandsSeparator = True

							Dim btn = New Button() With {.Name = "filterBtnNum" + filterCount.ToString, .Text = "add"}
							btn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
							GroupBox1.Controls.Add(btn)
							btn.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
							btn.Left = num.Left + num.Width + 45
							btn.Height = cmb_num.Height : btn.Width = 40
							btn.Visible = True
							AddHandler btn.Click, AddressOf filter_add_handler

							btn.Tag = field.DBname
						Else
							Dim cmb As New ComboBox With {.Name = "filterCmb" + filterCount.ToString, .DropDownStyle = ComboBoxStyle.DropDownList}
							cmb.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
							GroupBox1.Controls.Add(cmb)
							cmb.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
							cmb.Left = 110
							cmb.Width = GroupBox1.Width - 200

							Dim chk As New CheckBox With {.Name = "filterChk" + filterCount.ToString, .Text = "!=", .AutoSize = True}
							chk.Anchor = AnchorStyles.Top Or AnchorStyles.Right
							GroupBox1.Controls.Add(chk)
							chk.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 2
							chk.Left = cmb.Left + cmb.Width + 10

							Dim btn = New Button() With {.Name = "filterBtn" + filterCount.ToString, .Text = "add"}
							btn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
							GroupBox1.Controls.Add(btn)
							btn.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
							btn.Left = chk.Left + chk.Width
							btn.Height = cmb.Height : btn.Width = 40
							btn.Visible = True
							AddHandler btn.Click, AddressOf filter_add_handler

							'Fill comboboxes with available values
							cmb.BeginUpdate()
							Dim rdr = db.queryReader("SELECT name FROM groups WHERE field = '" + field.DBname + "' ORDER BY name")
							If rdr.HasRows Then
								'If groupped value present
								Dim dt As New DataTable
								dt.Columns.Add(field.DBname)
								Do While rdr.Read
									dt.Rows.Add(rdr.GetString(0))
								Loop
								lbl.Tag = "GRP"
								If field.is_list Then cmb.Tag = "LIST"
								cmb.DataSource = dt : cmb.DisplayMember = field.DBname : cmb.EndUpdate()
							Else
								'If regular (no groupped) field
								lbl.Tag = "REGULAR"
								Dim dt As DataTable = Nothing 'db.queryDataset("SELECT DISTINCT " + realFieldName + " FROM main ORDER BY " + realFieldName)
								If field.is_list Then
									''TEST
									'Dim fld = realFieldName
									'Dim sql = "WITH x(firstone, rest) AS ("
									'sql += "SELECT substr(" + fld + ", 1, instr(" + fld + ", ',') - 1) as firstone, substr(" + fld + ", instr(" + fld + ", ',') + 1) as rest FROM main WHERE " + fld + " like '%,%' "
									'sql += " UNION ALL "
									'sql += "SELECT substr(rest, 1, instr(rest, ',') - 1) as firstone, substr(rest, instr(rest, ',') + 1) as rest FROM x WHERE rest like '%,%' " 'LIMIT 200 "
									'sql += ") "
									'sql += "SELECT DISTINCT firstone AS " + fld + " FROM x UNION ALL SELECT DISTINCT rest FROM x WHERE rest NOT LIKE '%,%' AND trim(rest) != '' ORDER by firstone"
									'Dim dtMod = db.queryDataset(sql)
									''TEST END

									Dim dtMod As New DataTable
									Dim cacheExist = db.queryReader("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE '_" + field.DBname + "_listCache'").HasRows
									If cacheExist Then
										dtMod = db.queryDataset("SELECT value AS " + field.DBname + " FROM _" + field.DBname + "_listCache ORDER BY value")
									Else
										'If no cache - use old realization
										'dt = db.queryDataset("SELECT DISTINCT " + field.DBname + " FROM main ORDER BY " + field.DBname)
										'GROUP BY lower(column) is like SELECT DISTINCT but case insensitive
										dt = db.queryDataset("SELECT " + field.DBname + " FROM main GROUP BY lower(" + field.DBname + ") ORDER BY " + field.DBname)

										Dim alreadyAdded As New List(Of String)
										dtMod.Columns.Add(field.DBname)
										For Each r As DataRow In dt.Rows
											If r.Item(0).ToString = "" And Not alreadyAdded.Contains(" {Empty}") Then
												dtMod.Rows.Add({" {Empty}"}) : alreadyAdded.Add(" {Empty}")
											End If
											For Each item In r.Item(0).ToString.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
												If Not alreadyAdded.Contains(item.Trim.ToUpper) Then
													dtMod.Rows.Add({item.Trim})
													alreadyAdded.Add(item.Trim.ToUpper)
												End If
											Next
										Next
									End If

									Dim dv = dtMod.DefaultView
									dv.Sort = field.DBname
									dt = dv.ToTable
									cmb.Tag = "LIST"
								Else
									'GROUP BY lower(column) is like SELECT DISTINCT but case insensitive
									'This COALESCE version replace null and empty and whitespace values to {Empty}
									dt = db.queryDataset("SELECT COALESCE(TRIM(NULLIF(" + field.DBname + ", '')), ' {Empty}') AS column FROM main GROUP BY lower(column) ORDER BY column")
								End If

								'cmb.DataSource = dt : cmb.DisplayMember = realFieldName
								cmb.DisplayMember = dt.Columns(0).ColumnName : cmb.DataSource = dt : cmb.EndUpdate()
							End If
							btn.Tag = field.DBname
						End If

						filterCount += 1
					End If

					'add to liveSearchMenu
					Dim mi As New MenuItem With {.Text = field.name, .Tag = field.DBname}
					AddHandler mi.Click, Sub(o As Object, e As EventArgs)
											 For Each mi In liveSearchMenu.MenuItems
												 mi.Checked = False
											 Next
											 DirectCast(o, MenuItem).Checked = True
											 TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(Nothing))
											 If liveSearchMenu.MenuItems(0).Checked Then TextBox1_TextChanged(TextBox1, New EventArgs)
										 End Sub
					liveSearchMenu.MenuItems.Add(mi)
				End If
				'Move to the next field row
				If Field_Col = 0 AndAlso Layout_NoBR.Contains("FIELD_" + field.field_type.ToString().ToUpper() + field.index_of_type.ToString) Then Field_Col += 1 Else Field_Col = 0 : Field_Row += 1
			End If
		Next

		If Not onlyRebuildVisibleFields Then
			'Add path filter to games catalog
			Dim add_path_filter = ini.IniReadValue("Interface", "ShowPathFilter")
			If add_path_filter = "1" OrElse add_path_filter.ToUpper = "TRUE" Then
				Dim lbl As New Label With {.Name = "filterLabel" + filterCount.ToString, .Text = "Path:"}
				GroupBox1.Controls.Add(lbl)
				lbl.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount)
				lbl.Left = 6

				Dim cmb As New ComboBox With {.Name = "filterCmb" + filterCount.ToString, .DropDownStyle = ComboBoxStyle.DropDownList}
				cmb.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
				GroupBox1.Controls.Add(cmb)
				cmb.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
				cmb.Left = 110
				cmb.Width = GroupBox1.Width - 200

				Dim chk As New CheckBox With {.Name = "filterChk" + filterCount.ToString, .Text = "!=", .AutoSize = True}
				chk.Anchor = AnchorStyles.Top Or AnchorStyles.Right
				GroupBox1.Controls.Add(chk)
				chk.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 2
				chk.Left = cmb.Left + cmb.Width + 10

				Dim btn = New Button() With {.Name = "filterBtn" + filterCount.ToString, .Text = "add"}
				btn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
				GroupBox1.Controls.Add(btn)
				btn.Top = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) - 4
				btn.Left = chk.Left + chk.Width
				btn.Height = cmb.Height : btn.Width = 40
				btn.Visible = True
				AddHandler btn.Click, AddressOf filter_add_handler

				'Fill comboboxes with available values
				Dim dt = db.queryDataset("SELECT DISTINCT name FROM paths ORDER BY name DESC")
				cmb.DataSource = dt : cmb.DisplayMember = "name"
				btn.Tag = "paths.name"
				lbl.Tag = "REGULAR"

				filterCount += 1
			End If

			'Add option to reverse sort order
			If SortToolStripMenuItem.DropDownItems.Count > 0 Then
				SortToolStripMenuItem.DropDownItems.Add(New ToolStripSeparator())
				SortToolStripMenuItem.DropDownItems.Add(ReverseSortOrder)
				Dim lastSortReverse = (ini.IniReadValue("Interface", "LastSortReverse").Trim.ToUpper() = "TRUE")
				ReverseSortOrder.Checked = lastSortReverse
			Else
				ReverseSortOrder.Checked = False
			End If

			'Restore last sorting
			Dim lastSort = ini.IniReadValue("Interface", "LastSort").Trim.ToUpper
			For Each item As ToolStripItem In SortToolStripMenuItem.DropDownItems
				If item Is ReverseSortOrder Then Continue For
				If item.GetType Is GetType(ToolStripSeparator) Then Continue For
				If item.Text.ToUpper.Trim = lastSort Then
					DirectCast(SortToolStripMenuItem.DropDownItems(0), ToolStripMenuItem).Checked = False
					DirectCast(item, ToolStripMenuItem).Checked = True
					Exit For
				End If
			Next

			'Restore last filter
			ListBox2.Items.Clear()
			Dim c As Integer = 0
			Do While ini.IniReadValue("Interface", "LastFilter" + c.ToString).Trim <> ""
				Dim flt = ini.IniReadValue("Interface", "LastFilter" + c.ToString).Trim.Split({";;;"}, StringSplitOptions.RemoveEmptyEntries)
				If flt.Count = 2 Then
					Dim fi As New filterItem
					fi.filterShown = flt(0)
					fi.filterReal = flt(1)
					ListBox2.Items.Add(fi)
				End If
				c += 1
			Loop

			'Adjust filter panel height
			GroupBox1.Height = NEW_FILTER_TOP_ADD + (NEW_FILTER_TOP_MULTIPLIER * filterCount) + 130
			GroupBox1.Top = Button3.Top - GroupBox1.Height + 10
		End If

		'Set textarea tabs names
		Dim text_fields = Fields_ordered.Where(Function(f) f.field_type = Field_Info.field_types.txt AndAlso f.enabled)
		If text_fields.Count > 0 Then TabPage1.Text = text_fields(0).name Else TabControl1.TabPages.Remove(TabPage1)
		If text_fields.Count > 1 Then TabPage1.Text = text_fields(1).name Else TabControl1.TabPages.Remove(TabPage2)
		If text_fields.Count > 2 Then TabPage1.Text = text_fields(2).name Else TabControl1.TabPages.Remove(TabPage3)
	End Sub
	Public Sub field_click_handler(o As Object, e As MouseEventArgs)
		Dim txt = DirectCast(o, Label).Text
		Dim ind = txt.IndexOf(":")
		If ind > -1 And ind < txt.Length - 1 Then txt = txt.Substring(ind + 1).Trim
		If String.IsNullOrEmpty(txt) Then Exit Sub

		Dim fi = Fields.Where(Function(f) f.assoc_lbl Is DirectCast(o, Label)).FirstOrDefault()
		If e.Button = MouseButtons.Right AndAlso fi IsNot Nothing AndAlso fi.is_nameLink Then
			Dim names As New List(Of String)
			If fi.is_list Then
				names.AddRange(txt.Split({","c}, StringSplitOptions.RemoveEmptyEntries).Select(Of String)(Function(s) s.Trim))
			Else
				names.Add(txt)
			End If

			Dim ctxt_menu As New ContextMenu()
			'names.ForEach(Sub(x) ctxt_menu.MenuItems.Add(x))
			For Each n In names
				Dim tstrmi As New MenuItem(n)
				AddHandler tstrmi.Click, Sub(o1 As Object, e1 As EventArgs)
											 If ListBox1.DataSource Is Nothing Then Exit Sub

											 Dim prd = CType(o1, MenuItem).Text.ToUpper
											 Dim dt = CType(ListBox1.DataSource, DataTable)
											 For i As Integer = 0 To dt.Rows.Count - 1
												 Dim name = dt.DefaultView(i).Item("name").ToString().ToUpper()
												 If name = prd Then ListBox1.SelectedIndex = i : Exit For
											 Next
										 End Sub
				ctxt_menu.MenuItems.Add(tstrmi)
			Next
			ctxt_menu.Show(DirectCast(o, Label), New Point(5, 5))
		Else
			Clipboard.SetText(txt)
		End If
	End Sub
	Public Sub fieldset_change_handler(ByVal sender As Object, ByVal e As EventArgs)
		If refreshing Then Exit Sub
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim n As Integer
		Dim val As String = ""
		Dim cntrl As Control = DirectCast(sender, Control)
		If cntrl.Name.ToUpper.StartsWith("CHECK") Then
			n = CInt(cntrl.Name.Substring(5))
			Dim check As CheckBox = DirectCast(cntrl, CheckBox)
			If check.Checked Then val = "true" Else val = "false"
		ElseIf cntrl.Name.ToUpper.StartsWith("TXT") Then
			n = CInt(cntrl.Name.Substring(6))
			Dim txt As TextBox = DirectCast(cntrl, TextBox)
			val = txt.Text
		ElseIf cntrl.Name.ToUpper.StartsWith("CMB") Then
			n = CInt(cntrl.Name.Substring(6))
			Dim cmb As ComboBox = DirectCast(cntrl, ComboBox)
			If cmb.SelectedItem IsNot Nothing Then val = cmb.SelectedItem.ToString Else val = ""
		ElseIf cntrl.Name.ToUpper.StartsWith("CCMB") Then
			n = CInt(cntrl.Name.Substring(7))
			Dim ccmb As checkedCombo = DirectCast(cntrl, checkedCombo)
			val = ccmb.CheckedItems
		End If

		Dim sql = "UPDATE main SET " + Fields(n - 1).DBname + " = '" + val.Replace("'", "''") + "' "
		sql += "WHERE id = " + ListBox1.SelectedValue.ToString
		db.execute(sql)
	End Sub
	Public Sub init_categories(Optional selected_categories As List(Of Integer) = Nothing, Optional restore_last As Boolean = True)
		'Save last checked categories, if needed
		Dim last_unchecked As String() = {}
		If selected_categories Is Nothing AndAlso restore_last AndAlso TreeView1.Nodes.Count > 0 Then
			last_unchecked = TreeView1.Nodes.GetAllNodesRecur2().Where(Function(n) Not n.Checked).Select(Of String)(Function(n) getNodeCategoryPath(n).ToUpper()).ToArray()
		End If

		TreeView1.Nodes.Clear() : TreeView2.Nodes.Clear()

		'Dim sw As New Stopwatch : sw.Start()
		Dim nodes_new = init_categories_set()
		'sw.Stop() : MsgBox(sw.ElapsedMilliseconds.ToString)

		If nodes_new.Item1 Is Nothing Then
			Me.Invoke(Sub() TextBox2.Text = "There is no categories in this database...")
		Else
			refreshing = True
			Me.Invoke(Sub() TextBox2.Text = "[All]")
			TreeView1.Nodes.AddRange(nodes_new.Item1)
			TreeView2.Nodes.AddRange(nodes_new.Item2)

			Dim cnt As Integer = 0
			If selected_categories IsNot Nothing AndAlso selected_categories.Count > 0 Then
				'Restore selected categories, saved on last exit
				For Each n In TreeView1.Nodes.GetAllNodesRecur
					If Not selected_categories.Contains(cnt) Then n.Checked = False
					cnt += 1
				Next
			Else
				'Restore last checked, if needed
				If last_unchecked.Count > 0 Then
					For Each n In TreeView1.Nodes.GetAllNodesRecur
						If last_unchecked.Contains(getNodeCategoryPath(n).ToUpper()) Then n.Checked = False
					Next
				End If
			End If

			refreshing = False
		End If

		'Dim dt = db.queryDataset("SELECT cat, count(id) as cnt FROM category GROUP BY cat")
		'If dt.Rows.Count = 0 Then
		'	Me.Invoke(Sub() TextBox2.Text = "There is no categories in this database...")
		'Else
		'	refreshing = True
		'	Me.Invoke(Sub() TextBox2.Text = "[All]")
		'	Dim dt_arr = dt.AsEnumerable().Select(Function(dr) (dr.Field(Of String)("cat"), dr.Field(Of Int64)("cnt"))).ToArray
		'	For i As Integer = 0 To dt.Rows.Count - 1
		'		Dim nodes = TreeView1.Nodes
		'		Dim nodes2 = TreeView2.Nodes
		'		Dim current_path = ""
		'		For Each cat_sub In dt.Rows(i).Field(Of String)(0).Split({"/"c}, StringSplitOptions.RemoveEmptyEntries)
		'			current_path += cat_sub + "/"
		'			If Not nodes.ContainsKey(cat_sub) Then
		'				Dim count_strict = dt_arr.Where(Function(x) x.Item1 = current_path.Substring(0, current_path.Length - 1)).Select(Of Integer)(Function(y) y.Item2).Sum
		'				Dim count_sub = count_strict + dt_arr.Where(Function(x) x.Item1.StartsWith(current_path)).Select(Of Integer)(Function(y) y.Item2).Sum
		'				nodes.Add(cat_sub, cat_sub + " (" + count_strict.ToString + ")(" + count_sub.ToString + ")").Checked = True
		'				nodes2.Add(cat_sub, cat_sub)
		'			End If

		'			nodes = nodes(cat_sub).Nodes
		'			nodes2 = nodes2(cat_sub).Nodes
		'		Next
		'	Next
		'	'sw.Stop() : MsgBox(sw.ElapsedMilliseconds.ToString)

		'	'Restore selected categories, saved on last exit
		'	Dim cnt As Integer = 0
		'	If selected_categories IsNot Nothing AndAlso selected_categories.Count > 0 Then
		'		For Each n In TreeView1.Nodes.GetAllNodesRecur
		'			If Not selected_categories.Contains(cnt) Then n.Checked = False
		'			cnt += 1
		'		Next
		'	End If

		'	refreshing = False
		'End If

		Me.Invoke(Sub() TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode)))
	End Sub
	Public Function init_categories_set() As (TreeNode(), TreeNode())

		'Dim sw As New Stopwatch : sw.Start()
		Dim dt = db.queryDataset("SELECT cat, count(id) as cnt FROM category GROUP BY cat")
		If dt.Rows.Count = 0 Then Return (Nothing, Nothing)
		'sw.Stop()

		'sw = New Stopwatch : sw.Start()
		Dim root_node As New TreeNode()
		Dim root_node_no_numbers As New TreeNode()
		Dim dt_arr = dt.AsEnumerable().Select(Function(dr) (dr.Field(Of String)("cat"), dr.Field(Of Int64)("cnt"))).ToArray
		For i As Integer = 0 To dt.Rows.Count - 1
			Dim nodes = root_node.Nodes
			Dim nodes2 = root_node_no_numbers.Nodes
			Dim current_path = ""
			For Each cat_sub In dt.Rows(i).Field(Of String)(0).Split({"/"c}, StringSplitOptions.RemoveEmptyEntries)
				current_path += cat_sub + "/"
				If Not nodes.ContainsKey(cat_sub) Then
					Dim count_strict = dt_arr.Where(Function(x) x.Item1 = current_path.Substring(0, current_path.Length - 1)).Select(Of Integer)(Function(y) y.Item2).Sum
					Dim count_sub = count_strict + dt_arr.Where(Function(x) x.Item1.StartsWith(current_path)).Select(Of Integer)(Function(y) y.Item2).Sum
					nodes.Add(cat_sub, cat_sub + " (" + count_strict.ToString + ")(" + count_sub.ToString + ")").Checked = True
					nodes2.Add(cat_sub, cat_sub)
				End If

				nodes = nodes(cat_sub).Nodes
				nodes2 = nodes2(cat_sub).Nodes
			Next
		Next
		'sw.Stop()

		'sw = New Stopwatch : sw.Start()
		'Dim xxx = (root_node.Nodes.Cast(Of TreeNode).ToArray, root_node_no_numbers.Nodes.Cast(Of TreeNode).ToArray)
		'sw.Stop()

		Return (root_node.Nodes.Cast(Of TreeNode).ToArray, root_node_no_numbers.Nodes.Cast(Of TreeNode).ToArray)
	End Function

	Public Sub init_customLists()
		'remove lists from view list menu
		For i As Integer = ViewListToolStripMenuItem.DropDownItems.Count - 1 To 0 Step -1
			RemoveHandler ViewListToolStripMenuItem.DropDownItems(i).Click, AddressOf viewListClick
			ViewListToolStripMenuItem.DropDownItems.RemoveAt(i)
		Next
		Dim item As New ToolStripMenuItem() With {.Text = "ALL", .Tag = "-1", .Checked = True}
		AddHandler item.Click, AddressOf viewListClick
		ViewListToolStripMenuItem.DropDownItems.Add(item)

		'remove buttons
		Dim btn_to_remove As New List(Of Button)
		For Each btn In SplitContainer3.Panel2.Controls.OfType(Of Button)()
			btn_to_remove.Add(btn)
		Next
		For Each btn In btn_to_remove
			SplitContainer3.Panel2.Controls.Remove(btn)
		Next
		customListImages = New List(Of Image)
		customListImagesGray = New List(Of Image)

		Dim dt = db.queryDataset("SELECT id, name FROM custom_lists ORDER by id")
		For i As Integer = 1 To dt.Rows.Count
			'Add button
			Dim btn As New Button
			btn.Name = "ButtonList" + i.ToString
			btn.Width = 24
			btn.Height = 24

			btn.BackgroundImageLayout = ImageLayout.Stretch
			Dim fname As String = ".\images\" + dt.Rows(i - 1).Item("name") + ".png"
			If My.Computer.FileSystem.FileExists(fname) Then
				Dim img As New Bitmap(fname)
				Dim imgGray = ClassZZ_helpers.convertToGrayscale(img)
				customListImages.Add(img)
				customListImagesGray.Add(imgGray)
				btn.BackgroundImage = imgGray
			Else
				customListImages.Add(Nothing)
				customListImagesGray.Add(Nothing)
				btn.Text = i.ToString
			End If
			btn.Tag = {i, CInt(dt.Rows(i - 1).Item("id"))}
			AddHandler btn.Click, AddressOf press_custom_list_button

			SplitContainer3.Panel2.Controls.Add(btn)
			btn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
			btn.Top = SplitContainer3.Panel2.Height - 30
			'btn.Left = SplitContainer3.Panel2.Width - 320 + ((i - 1) * 20)
			btn.Left = ((i - 1) * 28) + TabControl1.Left + TabPage1.Left

			'Add to view list menu
			item = New ToolStripMenuItem() With {.Text = dt.Rows(i - 1).Item("name"), .Tag = dt.Rows(i - 1).Item("id")}
			AddHandler item.Click, AddressOf viewListClick
			ViewListToolStripMenuItem.DropDownItems.Add(item)
		Next
	End Sub
	Public Sub init_index_menu()
		Dim indices As Class01_db.index_info() = db.GetIndexInfo("*")
		Dim ind_exist = indices.Where(Function(i) i.field.ToUpper() = "NAME").Count > 0
		Dim ind_exist_cat = indices.Where(Function(i) i.field.ToUpper() = "CAT").Count > 0
		CreateIndexFornameFieldToolStripMenuItem.Enabled = Not ind_exist
		DeleteIndexFornameFieldToolStripMenuItem.Enabled = ind_exist
		CreateIndexForCategorycatFieldToolStripMenuItem.Enabled = Not ind_exist_cat
		DeleteIndexForCategorycatFieldToolStripMenuItem.Enabled = ind_exist_cat
	End Sub


	'ON EXIT
	Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
		'Save Selected Categories
		Dim nodes_all = TreeView1.Nodes.GetAllNodesRecur()
		Dim nodes_checked_bin = ""
		For i = 0 To nodes_all.Count - 1
			If nodes_all(i).Checked Then nodes_checked_bin += Convert.ToString(i, 2).PadLeft(9, "0"c)
		Next
		If nodes_checked_bin.Length Mod 8 > 0 Then nodes_checked_bin = nodes_checked_bin.PadLeft(nodes_checked_bin.Length + (8 - (nodes_checked_bin.Length Mod 8)), "0"c)
		Dim b As New List(Of Byte)
		For i = 0 To nodes_checked_bin.Length - 1 Step 8
			b.Add(Convert.ToByte(nodes_checked_bin.Substring(i, 8), 2))
		Next
		Dim nodes_checked_str = Convert.ToBase64String(b.ToArray())
		ini.IniWriteValue("Interface", "LastCheckedCategories", nodes_checked_str)

		'Save Window State
		If Me.WindowState = FormWindowState.Normal Then
			ini.IniWriteValue("Interface", "LastOpenedPos", Me.Left.ToString + "x" + Me.Top.ToString)
			ini.IniWriteValue("Interface", "LastOpenedSize", Me.Width.ToString + "x" + Me.Height.ToString)
		Else
			ini.IniWriteValue("Interface", "LastOpenedPos", RestoreBounds.X.ToString + "x" + RestoreBounds.Y.ToString)
			ini.IniWriteValue("Interface", "LastOpenedSize", RestoreBounds.Width.ToString + "x" + RestoreBounds.Height.ToString)
		End If
		ini.IniWriteValue("Interface", "LastOpenedState", Me.WindowState.ToString)

		ini.IniWriteValue("Interface", "Splitter1", SplitContainer1.SplitterDistance.ToString)
		ini.IniWriteValue("Interface", "Splitter2", SplitContainer2.SplitterDistance.ToString)
		Application.Exit()
	End Sub

	'Category click (show categories list)
	Private Sub TextBox2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.Click
		Panel1.Visible = True
		'Panel1.Capture = True
		TreeView1.Focus()
		Application.AddMessageFilter(Me)
	End Sub
	Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
		'List Of Windows Messages
		'https://wiki.winehq.org/List_Of_Windows_Messages
		If (m.Msg = &H201) Then
			Dim inPanelBounds = Panel1.Bounds.Contains(Panel1.Parent.PointToClient(Cursor.Position))
			Dim inTreeView2Bounds = TreeView2.Bounds.Contains(TreeView2.Parent.PointToClient(Cursor.Position))

			If Not inPanelBounds Then
				'Close category filter
				Panel1.Visible = False
			End If
			If Not inTreeView2Bounds Then
				'Close category change panel, on category data field
				TreeView2.Visible = False
			End If

			If Not Panel1.Visible And Not TreeView2.Visible Then Application.RemoveMessageFilter(Me)
		End If
		Return False
	End Function
	'Check all
	Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
		If TreeView1.Nodes.Count = 0 Then Exit Sub
		refreshing = True
		checkRecur(TreeView1.Nodes, True)
		refreshing = False
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode))
	End Sub
	'Uncheck all
	Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
		If TreeView1.Nodes.Count = 0 Then Exit Sub
		refreshing = True
		checkRecur(TreeView1.Nodes, False)
		refreshing = False
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode))
	End Sub
	Private Sub checkRecur(ByVal nodes As TreeNodeCollection, ByVal check As Boolean)
		For Each node As TreeNode In nodes
			node.Checked = check
			If node.Nodes.Count > 0 Then checkRecur(node.Nodes, check)
		Next
	End Sub
	'Category node check (Also used to refresh list)
	Private Sub TreeView1_AfterCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TreeView1.AfterCheck
		If refreshing Then Exit Sub
		'If TreeView1.Nodes.Count = 0 Then Exit Sub 'I don't know why it is here, but it prevent to allow new entry in new database without categories

		'If check-sub
		If e.Node IsNot Nothing AndAlso CheckBox1.Checked Then
			refreshing = True
			Dim checked As Boolean = e.Node.Checked
			checkRecur(e.Node.Nodes, checked)
			refreshing = False
		End If

		'Get selected customList id
		Dim customList As Integer = -1
		For Each i As ToolStripMenuItem In ViewListToolStripMenuItem.DropDownItems
			If i.Checked Then customList = CInt(i.Tag) : Exit For
		Next

		'Get list of selected categories
		Dim tmp = getCheckedRecur(TreeView1.Nodes)
		'If tmp.Length > 2 Then tmp = tmp.Substring(0, tmp.Length - 2).Replace("\", "/")
		If tmp.Length > 2 Then tmp = tmp.Substring(0, tmp.Length - 2)

		'Get filters
		Dim joins As String = ""
		Dim filter As String = ""
		For Each o In ListBox2.Items
			Dim fItem = CType(o, filterItem)
			'filter += " AND " + fItem.filterReal
			filter += fItem.filterReal + " " + Label1.Text.Trim + " "

			'If db field contains '.' then need to add join - this is for additional path table filter, available in Games catalog
			Dim field = fItem.filterReal.Substring(0, fItem.filterReal.IndexOf(" "))
			If field.Contains(".") Then
				Dim table_name = field.Substring(0, field.IndexOf("."))
				If table_name.StartsWith("(") Then table_name = table_name.Substring(1)
				If Not joins.ToUpper.Contains(table_name.ToUpper) Then
					joins += "LEFT OUTER JOIN " + table_name + " ON main.id = " + table_name + ".main_id "
				End If
			End If
		Next
		If filter <> "" Then filter = " AND (" + filter.Substring(0, filter.Length - Label1.Text.Trim.Length - 2) + ")"

		'Get live filter
		Dim liveFilter = ""
		If Not liveSearchMenu.MenuItems(0).Checked And TextBox1.Text.Trim <> "" Then
			For i As Integer = 1 To liveSearchMenu.MenuItems.Count - 1
				If liveSearchMenu.MenuItems(i).Checked Then liveFilter = liveSearchMenu.MenuItems(i).Tag
			Next
			If ContainToolStripMenuItem.Checked Then
				liveFilter = liveFilter + " LIKE '%" + TextBox1.Text.Replace("'", "''") + "%'"
			Else
				liveFilter = liveFilter + " LIKE '" + TextBox1.Text.Replace("'", "''") + "%'"
			End If
		End If

		'Get order from menu
		Dim order As String = ""
		For i As Integer = 0 To SortToolStripMenuItem.DropDownItems.Count - 1
			If SortToolStripMenuItem.DropDownItems.Item(i) Is ReverseSortOrder Then Continue For
			If SortToolStripMenuItem.DropDownItems.Item(i).GetType Is GetType(ToolStripSeparator) Then Continue For
			Dim tsi = DirectCast(SortToolStripMenuItem.DropDownItems(i), ToolStripMenuItem)
			If tsi.Checked Then order = tsi.Tag.ToString : Exit For
		Next
		If order = "" Then order = "name"
		If order.Trim.ToUpper = "NAME" Then order = "main." + order

		'Dim sql = "SELECT DISTINCT main.id, main.name, group_concat(category.cat) "
		Dim sql = "SELECT main.id, main.name, group_concat(category.cat) "
		'Dim sql = "SELECT main.id, main.name, group_concat(c2.cat) "
		If type = catalog_type.games Then sql += ", data_num1 "
		sql += "FROM main "
		sql += "JOIN category ON main.id = category.id_main "
		'sql += "JOIN category AS c2 ON main.id = c2.id_main "
		If customList <> -1 Then sql += "JOIN custom_lists_data on main.id = custom_lists_data.id_main "
		sql += joins
		sql += "WHERE category.cat IN (" + tmp + ") " + filter
		If liveFilter <> "" Then sql += " AND " + liveFilter
		If customList <> -1 Then sql += " AND custom_lists_data.id_list = " + customList.ToString
		'If filter_txt_file.Count > 0 Then sql += " AND UPPER(main.name) IN (" + String.Join(",", filter_txt_file) + ") "
		sql += " GROUP BY main.id, main.name"
		sql += " ORDER BY " + order
		If ReverseSortOrder.Checked Then sql += " DESC"

		'Dim sw As New Stopwatch() : sw.Start()
		Dim dt As DataTable = db.queryDataset(sql)
		'sw.Stop()

		'Show currently selected categories in textbox
		If tmp.Trim = "" Then
			TextBox2.Text = "[None]"
		ElseIf tmp.Split(",").Count = TreeView1.GetNodeCount(True) Then
			TextBox2.Text = "[All]"
		Else
			TextBox2.Text = tmp
		End If

		'ListBox1.Visible = False
		'ListBox1.BeginUpdate()
		ListBox1.ValueMember = "id"
		ListBox1.DisplayMember = "name"
		ListBox1.DataSource = dt

		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
		'ListBox1.Visible = True

		'Live filter
		If liveSearchMenu.MenuItems(0).Checked And TextBox1.Text.Trim <> "" Then TextBox1_TextChanged(TextBox1, New System.EventArgs)
		'ListBox1.EndUpdate()
	End Sub
	Private Function getCheckedRecur(ByVal nodes As TreeNodeCollection) As String
		Dim tmp As String = ""
		For Each node As TreeNode In nodes
			'v1
			'Dim n = node.FullPath
			''Remove items count in paranthesis
			'Do While n.Contains(" (")
			'	Dim ind1 = n.IndexOf(" (")
			'	Dim ind2 = n.IndexOf(")", ind1)
			'	'Dim ind2 = n.LastIndexOf(")") -- this not working, because we are removing from fullPath, i.e. '3D Models (2)(4)/Animals (1)(5)/Dinosaurs (3)(7)'
			'	n = n.Substring(0, ind1) + n.Substring(ind2 + 1)
			'Loop

			'v2
			'Dim n = node.Name
			'Dim p = node.Parent
			'While p IsNot Nothing
			'	n = p.Name + "\" + n : p = p.Parent
			'End While

			'v3
			Dim n = getNodeCategoryPath(node)

			If node.Checked Then tmp = tmp + "'" + n + "', "
			If node.Nodes.Count > 0 Then tmp = tmp + getCheckedRecur(node.Nodes)
		Next
		Return tmp
	End Function
	Public Function getNodeCategoryPath(node As TreeNode) As String
		Dim n = node.Name
		Dim p = node.Parent
		While p IsNot Nothing
			n = p.Name + "/" + n : p = p.Parent
		End While
		Return n
	End Function

	'Listbox browse
	Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
		If refreshing Then Exit Sub

		'Reset custom list buttons
		Dim n As Integer = 0
		For Each btn In SplitContainer3.Panel2.Controls.OfType(Of Button)()
			btn.BackgroundImage = customListImagesGray(n)
			n += 1
		Next
		'Reset screenshots
		PictureBox1.Image = Nothing
		PictureBox2.Image = Nothing
		PictureBox3.Image = Nothing
		PictureBox4.Image = Nothing
		'Reset screenshots buttons
		For i As Integer = 1 To 20
			Dim b = DirectCast(SplitContainer2.Panel1.Controls("ButtonScreen" + i.ToString), Button)
			b.Tag = ""
			b.Visible = False
		Next
		'Reset launch button
		'Dim lb = SplitContainer3.Panel1.Controls("LaunchButton1")
		'If lb IsNot Nothing Then SplitContainer3.Panel1.Controls.Remove(lb)
		Dim lb As New List(Of Button)
		For Each btn In SplitContainer3.Panel1.Controls.OfType(Of Button)()
			If btn.Name.ToUpper.StartsWith("LAUNCHBUTTON") Then lb.Add(btn)
		Next
		For t As Integer = 0 To lb.Count - 1
			SplitContainer3.Panel1.Controls.Remove(lb(t))
		Next
		'override_selected_dosbox = ""

		If ListBox1.SelectedIndex < 0 Then
			'reset
			refreshing = True

			SplitContainer3.Panel1.Controls("LabelCatData").Text = ""
			If labelName IsNot Nothing Then labelName.Text = "Name: "
			If labelNameEdit IsNot Nothing Then labelNameEdit.Text = ""

			For Each field In Fields_ordered
				If field.assoc_txt IsNot Nothing Then field.assoc_txt.Text = ""
				If field.assoc_cmb IsNot Nothing Then field.assoc_cmb.SelectedIndex = -1
				If field.assoc_chk IsNot Nothing Then field.assoc_chk.Checked = False
				If field.assoc_lbl IsNot Nothing Then field.assoc_lbl.Text = field.name + ":"
			Next
			refreshing = False
			Exit Sub
		End If

		'Fill category field
		Dim cat = CType(ListBox1.SelectedItem, DataRowView).Row.Item(2).ToString.Replace(",", vbCrLf).Replace("&", "&&")
		SplitContainer3.Panel1.Controls("LabelCatData").Text = cat

		'Fill data to show
		Dim sql = "SELECT * FROM main WHERE id = " + ListBox1.SelectedValue.ToString
		Dim r = db.queryReader(sql)
		r.Read()
		refreshing = True
		If labelName IsNot Nothing Then labelName.Text = "Name: " + r.Item("name")
		If labelNameEdit IsNot Nothing Then labelNameEdit.Text = r.Item("name")

		Dim nFType As Integer = 0
		For Each field In Fields_ordered
			If field.assoc_txt IsNot Nothing Then
				field.assoc_txt.Text = r.Item(field.DBname).ToString
			ElseIf field.assoc_cmb IsNot Nothing Then
				Dim ind = field.assoc_cmb.Items.IndexOf(r.Item(field.DBname).ToString)
				field.assoc_cmb.SelectedIndex = ind
				If TypeOf field.assoc_cmb Is checkedCombo Then DirectCast(field.assoc_cmb, checkedCombo).CheckedItems = r.Item(field.DBname).ToString
			ElseIf field.assoc_lbl IsNot Nothing Then
				field.assoc_lbl.Text = field.name + ": " + r.Item(field.DBname).ToString.Replace("&", "&&")
			ElseIf field.assoc_chk IsNot Nothing Then
				Dim tmp As String = r.Item(field.DBname).ToString
				If tmp = "" Or tmp = "0" Or tmp.ToUpper = "FALSE" Then field.assoc_chk.Checked = False Else field.assoc_chk.Checked = True
			End If
		Next
		refreshing = False

		'Descriptions
		bg_descr_loader.CancelAsync()
		Dim d1 = r.Item("data_txt1").ToString 'using r.Item() in lambda directly - bug, thread is only started 1 time of 5
		Dim d2 = r.Item("data_txt2").ToString 'using r.Item() in lambda directly - bug, thread is only started 1 time of 5
		Dim d3 = r.Item("data_txt3").ToString 'using r.Item() in lambda directly - bug, thread is only started 1 time of 5
		bg_descr_loader = New BackgroundWorker() With {.WorkerSupportsCancellation = True}
		bg_descr_loader.RunWorkerAsync({d1, d2, d3})

		'Screenshots - async load
		'If bg_scr_loader_thread IsNot Nothing Then bg_scr_loader_thread.Abort() : bg_scr_loader_thread = Nothing
		bg_scr_loader_thread = Nothing
		bg_scr_loader.CancelAsync()
		loadScreenRequest = New loadScreenRequestStruct With {.name = r.Item("name").ToString, .cat = cat}
		If type = catalog_type.games Then
			Dim year = CType(ListBox1.SelectedItem, DataRowView).Row.Item(3).ToString
			loadScreenRequest.year = year
		Else
			loadScreenRequest.year = ""
		End If
		bg_scr_loader_images.Clear()
		If bg_scr_loader IsNot Nothing Then bg_scr_loader.Dispose() : bg_scr_loader = Nothing
		GC.Collect()
		bg_scr_loader = New BackgroundWorker() With {.WorkerSupportsCancellation = True}
		bg_scr_loader.RunWorkerAsync(ListBox1.SelectedValue.ToString)

		'Handle custom lists
		sql = "SELECT id_main, id_list FROM custom_lists_data WHERE id_main = " + ListBox1.SelectedValue.ToString
		r = db.queryReader(sql)
		Do While r.Read
			Dim id_list = r.GetInt32(r.GetOrdinal("id_list"))
			For Each btn In SplitContainer3.Panel2.Controls.OfType(Of Button)()
				n = DirectCast(btn.Tag, Integer())(0) - 1
				Dim id = DirectCast(btn.Tag, Integer())(1)
				If id_list = id Then
					btn.BackgroundImage = customListImages(n)
					Exit For
				End If
			Next
		Loop

		'Handle launch button
		If type = catalog_type.generic Then
			sql = "SELECT value, name FROM paths WHERE main_id = " + ListBox1.SelectedValue.ToString + " ORDER BY name DESC"
			r = db.queryReader(sql)
			Dim count As Integer = 1
			Dim offset As Integer = 0
			Do While r.Read()
				Dim lButton As New Button With {.Name = "LaunchButton" + count.ToString, .Text = r.GetString(1)}
				SplitContainer3.Panel1.Controls.Add(lButton)
				lButton.Width = 60 : lButton.Height = 30
				lButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
				lButton.Left = SplitContainer3.Panel1.Width - 80 - offset
				lButton.Top = SplitContainer3.Panel1.Height - 50
				lButton.Tag = r.GetString(0)
				AddHandler lButton.MouseDown, AddressOf launch

				Dim tt As New ToolTip With {.ShowAlways = True, .AutoPopDelay = 32000}
				tt.SetToolTip(lButton, lButton.Tag)
				count += 1
				offset += lButton.Width + 10
			Loop
		ElseIf type = catalog_type.games Then
			sql = "SELECT value, name FROM paths WHERE main_id = " + ListBox1.SelectedValue.ToString + " ORDER BY name DESC"
			r = db.queryReader(sql)
			Dim count As Integer = 1
			Dim offset As Integer = 0
			Do While r.Read()
				Dim lButton As New Button With {.Name = "LaunchButton" + count.ToString, .Text = r.GetString(1)}
				'If r.GetString(1).Trim.ToUpper = "FOLDER" Then lButton.Text += vbCrLf + "right click for options"
				SplitContainer3.Panel1.Controls.Add(lButton)
				lButton.Width = 60 : lButton.Height = 30
				lButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
				lButton.Left = SplitContainer3.Panel1.Width - 80 - offset
				lButton.Top = SplitContainer3.Panel1.Height - 50
				lButton.Tag = r.GetString(0)
				AddHandler lButton.MouseDown, AddressOf launch

				Dim tt As New ToolTip With {.ShowAlways = True, .AutoPopDelay = 32000}
				tt.SetToolTip(lButton, lButton.Tag)
				count += 1
				offset += lButton.Width + 10
			Loop
		ElseIf type = catalog_type.daz Then
			sql = "SELECT value FROM paths WHERE main_id = " + ListBox1.SelectedValue.ToString
			r = db.queryReader(sql)
			Do While r.Read()
				Dim f = r.GetString(0)
				Dim caption = ""
				If f.ToUpper.EndsWith(".EXE") Then
					caption = "Launch setup"
				ElseIf f.ToUpper.EndsWith(".ZIP") Then
					caption = "Extract to library"
				End If

				If caption <> "" Then
					Dim lButton As New Button With {.Name = "LaunchButton1", .Text = caption}
					SplitContainer3.Panel1.Controls.Add(lButton)
					lButton.Width = 60 : lButton.Height = 40
					lButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
					lButton.Left = SplitContainer3.Panel1.Width - 80
					lButton.Top = SplitContainer3.Panel1.Height - 60
					lButton.Tag = f
					AddHandler lButton.MouseDown, AddressOf launch

					Dim tt As New ToolTip With {.ShowAlways = True, .AutoPopDelay = 32000}
					tt.SetToolTip(lButton, lButton.Tag)
					Exit Do
				End If
			Loop
		End If
	End Sub
	'Get Screenshot
	Dim found_box As Boolean = False
	Public Function getScreen(name As String, cat As String, index As Integer, Optional year As String = "", Optional Dont_Check_If_Exist As Boolean = False, Optional justGiveMeTheName As Boolean = False, Optional alt_path As String = "") As String
		'Dont_Check_If_Exist - will return screenshot path with filename without index, without extention
		'justGiveMeTheName   - will return screenshot path with filename with index, without extention

		Dim screen_path = screenshotPath.Trim
		If alt_path.Trim <> "" Then screen_path = alt_path.Trim
		If screen_path = "" Then Return ""

		If index = 0 Then found_box = False
		Dim letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : letters += letters.ToLower
		Dim normalized_name = name
		normalized_name = normalized_name.Replace("""", "'").Replace("*", "-").Replace("/", "-").Replace("\", "-").Replace("?", "–").Replace(":", " - ").Replace("|", "")
		normalized_name = normalized_name.Replace("  ", " ").Replace("  ", " ").TrimStart

		Dim first_letter = normalized_name.Substring(0, 1)
		If Not letters.Contains(first_letter) Then first_letter = "#"

		Dim folder As String = ""
		Dim subCatList As New List(Of String)
		If type = catalog_type.daz Then
			If cat.ToUpper.StartsWith("RENDEROSITY") Then folder = "Z_Renderosity"
		End If
		If type = catalog_type.games AndAlso year <> "" Then
			For Each subCat In cat.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
				subCatList.Add(subCat.ToUpper)
			Next
			If subCatList.Contains("DOS") Then
				folder = "Dos"
			ElseIf subCatList.Contains("WIN3X") Then
				folder = "Win3x"
			ElseIf subCatList.Contains("WINDOWS") Then
				year = year.Trim
				If year.Length > 4 Then year = year.Substring(year.Length - 4)
				Dim y As Integer = CInt(year)
				folder = "Windows"
				If year >= 2004 Then folder += "\" + year
			Else
				folder = "PC Booter"
			End If
		End If

		Dim path = screen_path + folder + "\" + first_letter.ToUpper() + "\" + normalized_name

		'Get optional special first screen - box for games or main for daz
		If index = 0 AndAlso Screenshot_Options.FirstSpecial Then
			For Each ext In screenExtensionsArray
				Dim path_first = path + Screenshot_Options.FirstSuffix + "." + ext
				If My.Computer.FileSystem.FileExists(path_first) Then
					found_box = True
					Return path_first
				End If
			Next
			If Not found_box AndAlso Not Screenshot_Options.FirstOptional Then
				If justGiveMeTheName Then Return path Else Return ""
			End If
		End If
		path += Screenshot_Options.Suffix

		'Next screens
		index += Screenshot_Options.FirstIndex
		If Screenshot_Options.FirstSpecial Then
			If Not Screenshot_Options.FirstOptional Then
				index -= 1
			Else
				If found_box Then index -= 1
			End If
		End If

		If Dont_Check_If_Exist Then Return path

		For Each ext In screenExtensionsArray
			Dim _path As String = path
			If justGiveMeTheName Then Return _path + index.ToString(Screenshot_Options.IndexFormat)
			_path += index.ToString(Screenshot_Options.IndexFormat) + "." + ext

			If My.Computer.FileSystem.FileExists(_path) Then
				Return _path
			End If
		Next
		Return ""
	End Function
	Private Sub loadDescriptionsAsync(o As Object, e As DoWorkEventArgs) Handles bg_descr_loader.DoWork
		Try
			'SyncLock description_loading_threads
			'	description_loading_threads.Add(Threading.Thread.CurrentThread)
			'	While description_loading_threads.Count > 1
			'		If description_loading_threads(1).IsAlive Then description_loading_threads(1).Abort()
			'		description_loading_threads.RemoveAt(1)
			'	End While
			'End SyncLock
			'Threading.Thread.Sleep(500)

			bg_descr_loader_threads.Add(Threading.Thread.CurrentThread)
			While bg_descr_loader_threads(0) IsNot Threading.Thread.CurrentThread
				Threading.Thread.Sleep(10)
			End While

			Dim this_worker = DirectCast(o, BackgroundWorker)
			If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub

			Dim rtf_arr() As RichTextBox = {RichTextBox1, RichTextBox2, RichTextBox3}
			Dim str_arr() = DirectCast(e.Argument, String())
			For i As Integer = 0 To 2
				Dim _i = i 'because compilator don't want we use iteration variable :(

				If str_arr(i).Trim = "" Then
					If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub
					Me.Invoke(Sub() rtf_arr(_i).Rtf = "")
				ElseIf str_arr(i).Trim.StartsWith("{\") Then
					If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub
					Me.Invoke(Sub() rtf_arr(_i).Rtf = str_arr(_i))
				Else
					Dim doc As New GemBox.Document.DocumentModel()
					Dim pf As New GemBox.Document.ParagraphFormat With {.LineSpacing = 0} ', .SpecialIndentation = -10}
					doc.DefaultParagraphFormat = pf
					doc.Content.LoadText(str_arr(i), New GemBox.Document.HtmlLoadOptions())

					If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub
					Dim saveOptions = New GemBox.Document.RtfSaveOptions()
					Using s As IO.MemoryStream = New IO.MemoryStream
						doc.Save(s, saveOptions)
						If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub

						'Fix links
						Dim rtf = saveOptions.Encoding.GetString(s.ToArray())
						Dim ind = rtf.IndexOf("HYPERLINK")
						While ind >= 0
							ind = rtf.IndexOf(" ", ind + 12)
							rtf = rtf.Insert(ind + 1, """")
							ind = rtf.IndexOf("}", ind)
							rtf = rtf.Insert(ind, """")
							ind = rtf.IndexOf("HYPERLINK", ind)
						End While
						If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub

						'Set rtf to RichTextBox
						Me.Invoke(Sub() rtf_arr(_i).Rtf = rtf)
						If this_worker.CancellationPending Then bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread) : e.Cancel = True : Exit Sub
					End Using
				End If
			Next

			'Threading.Thread.Sleep(250) 'To test concurency
			bg_descr_loader_threads.Remove(Threading.Thread.CurrentThread)
		Catch ex As Threading.ThreadAbortException

		Catch ex As Exception
			ex = ex
		End Try
	End Sub
	Private Sub LoadScreenshotAsync(sender As Object, e As DoWorkEventArgs) Handles bg_scr_loader.DoWork
		bg_scr_loader_thread = Threading.Thread.CurrentThread
		Try
			Dim this_worker = DirectCast(sender, BackgroundWorker)

			If loadScreenRequest.name = "" Then Exit Sub
			Dim name = loadScreenRequest.name : loadScreenRequest.name = ""
			Dim images As New List(Of Image)
			Dim images_files As New List(Of String)

			Dim found_screen_count = 0
			For i As Integer = 0 To 19
				Dim _i = i
				Dim scr As String = ""
				If loadScreenRequest.year <> "" Then
					scr = getScreen(name, loadScreenRequest.cat, i, loadScreenRequest.year)
				Else
					scr = getScreen(name, loadScreenRequest.cat, i)
				End If
				If scr = "" Then Exit For
				If this_worker.CancellationPending Then e.Cancel = True : Exit Sub

				'Set base picturebox
				If i <= 3 Then
					If this_worker.CancellationPending Then e.Cancel = True : Exit Sub
					Dim img = Image.FromFile(scr)
					If this_worker.CancellationPending Then e.Cancel = True : Exit Sub
					images.Add(img)
				End If

				images_files.Add(scr)

				If this_worker.CancellationPending Then e.Cancel = True : Exit Sub
				found_screen_count += 1
			Next

			'Additional Exodos Screenshots
			If type = catalog_type.games Then
				Dim sql = "SELECT value, name FROM paths WHERE main_id = " + e.Argument.ToString + " ORDER BY name DESC"
				Dim r = db.queryReader(sql)
				Do While r.Read()
					If r.GetString(1).ToUpper = "EXODOS" Then
						Dim path = IO.Path.GetDirectoryName(r.GetString(0)) + "\Meagre\"
						Dim exo_screen_folders = {"Title", "Screen", "Front", "Back", "Media"}
						Dim screen_list As New List(Of String)
						For Each exo_folder In exo_screen_folders
							For Each ext In screenExtensionsArray
								If IO.Directory.Exists(path + exo_folder) Then
									screen_list.AddRange(IO.Directory.GetFiles(path + exo_folder, "*." + ext))
								End If
							Next
						Next

						Dim c = 0
						For i As Integer = found_screen_count To 19
							If screen_list.Count - 1 < c Then Exit For

							'Set base picturebox
							If i <= 3 Then
								If this_worker.CancellationPending Then e.Cancel = True : Exit Sub
								Dim img = Image.FromFile(screen_list(c))
								If this_worker.CancellationPending Then e.Cancel = True : Exit Sub
								images.Add(img)
							End If

							'Set buttons
							images_files.Add(screen_list(c))

							c += 1
						Next
						found_screen_count = images_files.Count
					End If
				Loop
			End If

			'Add found screens to result list
			Dim t = Threading.Thread.CurrentThread
			bg_scr_loader_images.TryAdd(t, images)
			e.Result = images_files.ToArray()
		Catch ex As Threading.ThreadAbortException
			e.Cancel = True : Threading.Thread.ResetAbort()
		End Try
	End Sub
	Private Sub LoadScreenshotAsyncDone(sender As Object, e As RunWorkerCompletedEventArgs) Handles bg_scr_loader.RunWorkerCompleted
		If e.Cancelled Then Exit Sub
		If Not bg_scr_loader_images.ContainsKey(bg_scr_loader_thread) Then Exit Sub

		'Main images (pictureboxes)
		For i As Integer = 0 To bg_scr_loader_images(bg_scr_loader_thread).Count - 1
			DirectCast(TableLayoutPanel1.Controls("PictureBox" + (i + 1).ToString), PictureBox).Image = bg_scr_loader_images(bg_scr_loader_thread)(i)
		Next

		'Hidden images (little buttons)
		Dim arr = DirectCast(e.Result, String())
		For i As Integer = 0 To arr.Count - 1
			Dim b = DirectCast(SplitContainer2.Panel1.Controls("ButtonScreen" + (i + 1).ToString), Button)
			b.Visible = True : b.Tag = arr(i)
		Next
	End Sub

	'Listbox search as you type mechanic
	Private Sub ListBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles ListBox1.KeyDown
		If ListBox1.Items.Count = 0 Then Exit Sub

		Dim ch = ChrW(e.KeyCode)
		'Fix numpad keys, as they are interpreted wrong
		Select Case e.KeyCode
			Case Keys.NumPad1 : ch = "1"c
			Case Keys.NumPad2 : ch = "2"c
			Case Keys.NumPad3 : ch = "3"c
			Case Keys.NumPad4 : ch = "4"c
			Case Keys.NumPad5 : ch = "5"c
			Case Keys.NumPad6 : ch = "6"c
			Case Keys.NumPad7 : ch = "7"c
			Case Keys.NumPad8 : ch = "8"c
			Case Keys.NumPad9 : ch = "9"c
			Case Keys.NumPad0 : ch = "0"c
			Case Keys.Add : ch = "+"c
			Case Keys.Subtract : ch = "-"c
			Case Keys.Multiply : ch = "*"c
			Case Keys.Divide : ch = "*"c
			Case Keys.Decimal : ch = "."c
		End Select
		If Char.IsLetterOrDigit(ch) Then
			listbox_searchAsYouTypeStr += ch.ToString.ToUpper

			Dim dt = CType(ListBox1.DataSource, DataTable)
			For i As Integer = 0 To dt.Rows.Count - 1
				Dim name = dt.DefaultView(i).Item("name").ToString().ToUpper()
				If name.StartsWith(listbox_searchAsYouTypeStr) Then
					ListBox1.SelectedIndex = i : Exit For
				End If
			Next

			e.Handled = True
			e.SuppressKeyPress = True
			listbox_searchAsYouTypeTimer.Enabled = False
			listbox_searchAsYouTypeTimer.Enabled = True
		Else
			listbox_searchAsYouTypeStr = ""
			listbox_searchAsYouTypeTimer.Enabled = False
		End If
	End Sub
	Private Sub ListBox1_SearchAsYouTypeTimer() Handles listbox_searchAsYouTypeTimer.Tick
		listbox_searchAsYouTypeStr = ""
		listbox_searchAsYouTypeTimer.Enabled = False
	End Sub

	'Launch button click handler
	Private Sub launch(sender As Object, e As MouseEventArgs)
		Dim f As String = CType(sender, Button).Tag.ToString
		Dim t As String = CType(sender, Button).Text

		If e.Button = MouseButtons.Right Then
			'Right click
			Dim m As New ContextMenu()
			If type = catalog_type.games And t.Trim.ToUpper.StartsWith("EXO") Then
				Dim ex1 As New MenuItem With {.Text = "Launch to DOS cmd prompt"}
				AddHandler ex1.Click, Sub()
										  launch_exodos_custom(f, 1)
									  End Sub
				m.MenuItems.Add(ex1)

				Dim ex2 As New MenuItem With {.Text = "Launch to shell"}
				AddHandler ex2.Click, Sub()
										  launch_exodos_custom(f, 2)
									  End Sub
				m.MenuItems.Add(ex2)

				Dim ex3 As New MenuItem With {.Text = "Dosbox settings for this game"}
				AddHandler ex3.Click, Sub()
										  Dim frm As New Games_ExodosDosboxSingle(IO.Path.GetDirectoryName(f) + "\dosbox.conf")
										  frm.ShowDialog()
										  If frm.was_saved Then launch_exodos_custom(f, 0)
									  End Sub
				m.MenuItems.Add(ex3)

				Dim ex4 As New MenuItem With {.Text = "Revert dosbox settings to Exodos original"}
				AddHandler ex4.Click, Sub()
										  Dim reverted = False
										  Dim fname_orig = IO.Path.GetDirectoryName(f) + "\dosbox.orig.conf"
										  If File.Exists(fname_orig) Then
											  reverted = True
											  Dim fname = IO.Path.GetDirectoryName(f) + "\dosbox.conf"
											  If IO.File.Exists(fname) Then IO.File.Delete(fname)
											  IO.File.Move(fname_orig, fname)
										  End If

										  Dim dir = IO.Path.GetDirectoryName(f)
										  Dim bat_name = IO.Path.GetFileNameWithoutExtension(f)
										  Dim bat_custom = dir + "\" + bat_name + "_custom.bat"
										  Dim conf_custom = dir + "\dosbox_custom.conf"
										  If File.Exists(bat_custom) Then File.Delete(bat_custom) : reverted = True
										  If File.Exists(conf_custom) Then File.Delete(conf_custom) : reverted = True

										  If Not reverted Then MsgBox("The settings for this game was not altered.") : Exit Sub
									  End Sub
				m.MenuItems.Add(ex4)

				'Fill 'select dosbox' menu
				Dim dosbox_path = launch_exodos_get_selected_dosbox_path(f)
				Dim ex5 As New MenuItem With {.Text = "Select DosBox Version"}
				m.MenuItems.Add(ex5)
				For Each kv In dosbox_paths
					Dim checked = False
					If dosbox_path <> "" AndAlso kv.Value.ToUpper = dosbox_path Then checked = True
					Dim ex5sub As New MenuItem With {.Text = kv.Key, .Checked = checked, .Tag = kv.Value}
					ex5.MenuItems.Add(ex5sub)
					AddHandler ex5sub.Click, Sub(o As Object, ec As EventArgs)
												 Dim mi = DirectCast(o, MenuItem)
												 Dim pi = DirectCast(mi.Parent, MenuItem)
												 For Each sub_mi As MenuItem In pi.MenuItems
													 sub_mi.Checked = False
												 Next
												 mi.Checked = True
												 launch_exodos_custom(f, 0, mi.Tag.ToString())
											 End Sub
				Next
			ElseIf f.ToUpper.EndsWith("EXE") Then
				Dim ex1 As New MenuItem With {.Text = "Check 3D acceleration API"}
				'Dim ex1 As New MenuItem_My With {.Text = "Check 3D acceleration API"}
				AddHandler ex1.Click, Sub()
										  Class03_3D_API_Checker.check(f)
									  End Sub
				m.MenuItems.Add(ex1)
			ElseIf t.Trim.ToUpper = "FOLDER" Then
				Dim arr = {"*.exe", "*.bat", "*.iso", "*.isz", "*.cue", "*.mdf", "*.mds", "*.ccd", "*.nrg", "*.mdx", "*.zip", "*.rar", "*.7z"}
				If Not My.Computer.FileSystem.DirectoryExists(f) Then MsgBox("Directory " + vbCrLf + f + vbCrLf + " does not exist") : Exit Sub
				Dim files = My.Computer.FileSystem.GetFiles(f, FileIO.SearchOption.SearchTopLevelOnly, arr).ToList
				Dim files_uppercase = (From s As String In files Select s.ToUpper).ToList
				For fc As Integer = 0 To files.Count - 1
					Dim file = files(fc)
					Dim fu = file.ToUpper
					Dim fuNoExt = fu.Substring(0, fu.LastIndexOf("."))
					Dim fi = file.Substring(file.LastIndexOf("\") + 1)
					Dim mi As MenuItem = Nothing

					Dim _case As Integer = 0
					If fu.EndsWith(".EXE") Or fu.EndsWith(".BAT") Then _case = 1
					If fu.EndsWith(".ISO") Or fu.EndsWith(".ISZ") Or fu.EndsWith(".CUE") Or fu.EndsWith(".NRG") Or fu.EndsWith(".CCD") Or fu.EndsWith(".MDS") Or fu.EndsWith(".MDX") Then _case = 2
					If fu.EndsWith(".MDF") And Not files_uppercase.Contains(fuNoExt + ".MDS") Then _case = 2
					If fu.EndsWith(".ZIP") Or fu.EndsWith(".RAR") Or fu.EndsWith(".7Z") Then _case = 3
					Select Case _case
						Case 1
							'Launch
							mi = New MenuItem
							mi.Text = "Launch " + fi
							AddHandler mi.Click, Sub()
													 Dim psi As New ProcessStartInfo
													 psi.FileName = file
													 psi.WorkingDirectory = file.Substring(0, file.LastIndexOf("\"))
													 Process.Start(psi)
												 End Sub
						Case 2
							'Mount
							mi = New MenuItem
							mi.Text = "Mount " + fi
							AddHandler mi.Click, Sub()
													 Dim dt_path = ini.IniReadValue("Paths", "DaemonToolsPath")
													 If dt_path = "" Then MsgBox("Please, set [Paths] 'DaemonToolsPath' in config.") : Exit Sub
													 If Not My.Computer.FileSystem.FileExists(dt_path) Then
														 MsgBox("File " + vbCrLf + "'" + dt_path + "'" + vbCrLf + " not found. Check [Paths] 'DaemonToolsPath' in config.")
														 Exit Sub
													 End If
													 'DTLite.exe -mount dt, 0,"C:\My Images\name_of_image.ape"
													 'DTLite.exe -mount scsi, 0,"C:\My Images\name_of_image.ape"
													 Dim psi As New ProcessStartInfo With {.FileName = dt_path}
													 psi.WorkingDirectory = dt_path.Substring(0, dt_path.LastIndexOf("\"))
													 psi.Arguments = "-mount dt, 0,""" + file + """"
													 Process.Start(psi)
												 End Sub
						Case 3
							'Extract
							mi = New MenuItem
							mi.Text = "Extract " + fi
							AddHandler mi.Click, Sub()
													 Dim folder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(file)).Trim
													 Dim libraryPath = ini.IniReadValue("Paths", "LibraryPath")
													 If libraryPath = "" Then MsgBox("Please, set library path in config.") : Exit Sub
													 If Not My.Computer.FileSystem.DirectoryExists(libraryPath) Then
														 MsgBox("Directory " + vbCrLf + "'" + libraryPath + "'" + vbCrLf + " not found. Please, set library path in config.")
														 Exit Sub
													 End If
													 Dim sz = New SevenZip.SevenZipExtractor(file)
													 Dim root As String = ""
													 Dim rootIndex As Integer = 0
													 Dim rootFilesCount As Integer = 0
													 For ff As Integer = 0 To sz.ArchiveFileData.Count - 1
														 If Not sz.ArchiveFileData(ff).FileName.Contains("\") Then
															 rootFilesCount += 1
															 root = sz.ArchiveFileData(ff).FileName
															 rootIndex = ff
														 End If
													 Next
													 If rootFilesCount = 1 AndAlso sz.ArchiveFileData(rootIndex).IsDirectory = True Then
														 sz.ExtractArchive(libraryPath)
														 If folder IsNot Nothing AndAlso folder <> "" Then
															 If root.ToUpper <> folder.ToUpper Then
																 If My.Computer.FileSystem.DirectoryExists(libraryPath + "\" + folder) Then
																	 For Each f In My.Computer.FileSystem.GetDirectories(libraryPath + "\" + root)
																		 My.Computer.FileSystem.MoveDirectory(f, libraryPath + "\" + folder)
																	 Next
																	 For Each f In My.Computer.FileSystem.GetFiles(libraryPath + "\" + root)
																		 My.Computer.FileSystem.MoveFile(f, libraryPath + "\" + folder + "\" + System.IO.Path.GetFileName(f))
																	 Next
																	 My.Computer.FileSystem.DeleteDirectory(libraryPath + "\" + root, FileIO.DeleteDirectoryOption.DeleteAllContents)
																 Else
																	 My.Computer.FileSystem.RenameDirectory(libraryPath + "\" + root, folder)
																 End If
															 End If
														 End If
													 Else
														 Dim fi_no_ext = fi
														 If fi_no_ext.Contains(".") Then fi_no_ext = fi_no_ext.Substring(0, fi_no_ext.LastIndexOf("."))
														 If folder IsNot Nothing AndAlso folder <> "" Then
															 sz.ExtractArchive(libraryPath + "\" + folder)
														 Else
															 sz.ExtractArchive(libraryPath + "\" + fi_no_ext)
														 End If
													 End If
													 MsgBox("Done.")
												 End Sub
					End Select
					If mi IsNot Nothing Then m.MenuItems.Add(mi)
				Next
			End If

			'Additional entry 'open containing folder' for all path types
			If f.Trim <> "" Then
				If f.Contains("%%%") Then f = f.Split({"%%%"}, StringSplitOptions.RemoveEmptyEntries)(0)
				Dim mi As New MenuItem
				Dim dir As String = ""
				If My.Computer.FileSystem.DirectoryExists(f) Then
					mi.Text = "Open Folder"
					dir = f
				ElseIf My.Computer.FileSystem.FileExists(f) Then
					mi.Text = "Open Containing Folder"
					dir = IO.Path.GetDirectoryName(f)
				End If

				If dir <> "" Then
					AddHandler mi.Click, Sub()
											 Process.Start(dir)
										 End Sub
					m.MenuItems.Add(New MenuItem With {.Text = "-"})
					m.MenuItems.Add(mi)
				End If
			End If

			If m.MenuItems.Count = 0 Then m.MenuItems.Add("No suitable files / folders found.")
			m.Show(sender, New Point(10, 10))
		ElseIf e.Button = MouseButtons.Left Then
			'Left click
			If type = catalog_type.games AndAlso t.Trim.ToUpper.StartsWith("EXO") AndAlso f.ToUpper.EndsWith(".BAT") Then
				launch_exodos_custom(f, 3)
			ElseIf t.ToUpper = "FOLDER" Then
				If Not My.Computer.FileSystem.DirectoryExists(f) Then MsgBox("Directory " + vbCrLf + f + vbCrLf + " does not exist") : Exit Sub
				Process.Start(f)
			ElseIf t.ToUpper = "DXWND" Then
				Dim args = f.Split({"%%%"}, StringSplitOptions.RemoveEmptyEntries)
				If args.Count <> 2 Then MsgBox("Wrong arguments.")
				If Not My.Computer.FileSystem.FileExists(args(0)) Then MsgBox("File " + vbCrLf + args(0) + vbCrLf + " does not exist") : Exit Sub
				Dim psi As New ProcessStartInfo
				psi.FileName = args(0)
				psi.Arguments = args(1)
				psi.WorkingDirectory = args(0).Substring(0, args(0).LastIndexOf("\"))
				Process.Start(psi)
			ElseIf f.ToUpper.EndsWith(".EXE") Or f.ToUpper.EndsWith(".BAT") Then
				If Not My.Computer.FileSystem.FileExists(f) Then MsgBox("File " + vbCrLf + f + vbCrLf + " does not exist") : Exit Sub
				Dim psi As New ProcessStartInfo
				psi.FileName = f
				psi.WorkingDirectory = f.Substring(0, f.LastIndexOf("\"))
				Process.Start(psi)
			ElseIf f.ToUpper.EndsWith(".ZIP") Then
				If Not My.Computer.FileSystem.FileExists(f) Then MsgBox("File " + vbCrLf + f + vbCrLf + " does not exist") : Exit Sub
				Dim libraryPath = ini.IniReadValue("Paths", "LibraryPath")
				If libraryPath = "" Then MsgBox("Please, set library path in config.") : Exit Sub

				Dim sz = New SevenZip.SevenZipExtractor(f)
				sz.ExtractArchive(libraryPath)

				For Each f In sz.ArchiveFileNames
					If f.ToUpper.StartsWith("CONTENT") Then
						If My.Computer.FileSystem.FileExists(libraryPath + "\" + f) Then
							Dim moveTo = f.Substring(8)
							My.Computer.FileSystem.MoveFile(libraryPath + "\" + f, libraryPath + "\" + moveTo, True)
						End If
					End If
				Next
				If My.Computer.FileSystem.GetFiles(libraryPath + "\Content", FileIO.SearchOption.SearchAllSubDirectories, {"*.*"}).Count = 0 Then
					My.Computer.FileSystem.DeleteDirectory(libraryPath + "\Content", FileIO.DeleteDirectoryOption.DeleteAllContents)
				End If

				MsgBox("Done.")
			End If
		End If
	End Sub
	Private Sub launch_exodos_custom(bat As String, type As Integer, Optional override_dosbox_path As String = "")
		'type = 0 - just create/rewrite .bat file, and .conf file (if not exist), to save new dosbox path
		'type = 1 - launch to DOS
		'type = 2 - launch to shell (norton commander)
		'type = 3 - launch exodos original autoexec, but with modified dosbox version and .conf file

		Dim dir = IO.Path.GetDirectoryName(bat)
		Dim bat_name = IO.Path.GetFileNameWithoutExtension(bat)
		Dim bat_custom = dir + "\" + bat_name + "_custom.bat"
		Dim conf_custom = dir + "\dosbox_custom.conf"
		Dim conf_tmp = dir + "\dosbox_tmp.conf"
		If String.IsNullOrEmpty(override_dosbox_path) Then override_dosbox_path = launch_exodos_get_selected_dosbox_path(bat)

		Dim conf_main = dir + "\dosbox.conf"
		If FileIO.FileSystem.FileExists(bat_custom) Then FileIO.FileSystem.DeleteFile(bat_custom)
		If FileIO.FileSystem.FileExists(conf_tmp) Then File.Delete(conf_tmp)
		If FileIO.FileSystem.FileExists(conf_custom) Then conf_main = conf_custom

		'Replace reference in new .bat from "dosbox.conf" to "dosbox_custom.conf"
		Dim sr = My.Computer.FileSystem.OpenTextFileReader(bat)
		Dim sw = My.Computer.FileSystem.OpenTextFileWriter(bat_custom, False, System.Text.Encoding.ASCII)
		Do While Not sr.EndOfStream
			Dim s = sr.ReadLine
			If s.ToUpper.Contains("DOSBOX") AndAlso s.ToUpper.Contains(".EXE") Then
				s = """" + override_dosbox_path + """" + s.Substring(s.IndexOf(" -conf"))
				s = System.Text.RegularExpressions.Regex.Replace(s, "dosbox.conf", "dosbox_custom.conf", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
				If type = 1 Or type = 2 Then s = Replace(s, "-EXIT", "", 1, 1, CompareMethod.Text)
			End If
			sw.WriteLine(s)
		Loop
		sr.Close()
		sw.Close()
		If type = 0 AndAlso Not File.Exists(conf_custom) Then File.Copy(dir + "\dosbox.conf", conf_custom)
		If type = 0 Then Exit Sub

		'Comment all lines in autoexec except "CD*", "MOUNT*", "IMGMOUNT*", "C:\", "@C:\"
		Dim sr_main = My.Computer.FileSystem.OpenTextFileReader(conf_main)
		sw = My.Computer.FileSystem.OpenTextFileWriter(conf_tmp, False, New System.Text.UTF8Encoding(False))
		Do While Not sr_main.EndOfStream
			Dim s = sr_main.ReadLine
			sw.WriteLine(s)
			If s.Trim.ToUpper = "[autoexec]".ToUpper Then Exit Do
		Loop
		sr_main.Close()

		Dim sr_autoexec = My.Computer.FileSystem.OpenTextFileReader(dir + "\dosbox.conf")
		Dim in_autoecxec As Boolean = False
		Do While Not sr_autoexec.EndOfStream
			Dim s = sr_autoexec.ReadLine
			If in_autoecxec AndAlso type <> 3 Then
				Dim up = s.Trim.ToUpper
				Dim b1 = up.StartsWith("CD") Or up.StartsWith("@CD")
				Dim b2 = up.StartsWith("MOUNT") Or up.StartsWith("@MOUNT")
				Dim b3 = up.StartsWith("IMGMOUNT") Or up.StartsWith("@IMGMOUNT")
				Dim b4 = up.Length > 1 AndAlso up.Substring(1, 1) = ":"
				Dim b5 = up.Length > 2 AndAlso up.StartsWith("@") AndAlso up.Substring(2, 1) = ":"
				If Not b1 And Not b2 And Not b3 And Not b4 And Not b5 Then s = "REM " + s
			End If
			If in_autoecxec Then sw.WriteLine(s)
			If s.Trim.ToUpper = "[autoexec]".ToUpper Then in_autoecxec = True
		Loop

		If type = 2 Then
			'Launch to shell
			sw.WriteLine("mount x ""G:\eXoDOS Addon\!DOS""")
			sw.WriteLine("x:\UTILS\SHELL\NC4\nc")
		End If

		sr_autoexec.Close()
		sw.Close()

		If FileIO.FileSystem.FileExists(conf_custom) Then File.Delete(conf_custom)
		File.Move(conf_tmp, conf_custom)

		Dim psi As New ProcessStartInfo
		psi.FileName = bat_custom
		psi.WorkingDirectory = dir
		Process.Start(psi)
	End Sub
	Private Function launch_exodos_get_selected_dosbox_path(bat As String) As String
		Dim dir = IO.Path.GetDirectoryName(bat)
		Dim bat_name = IO.Path.GetFileNameWithoutExtension(bat)
		Dim bat_custom = dir + "\" + bat_name + "_custom.bat"
		If File.Exists(bat_custom) Then bat = bat_custom

		Dim bat_content = File.ReadAllLines(bat)
		Dim current_dir = Path.GetDirectoryName(bat)
		'Dim dosbox_path = override_selected_dosbox.ToUpper
		Dim dosbox_path = ""
		''If dosbox_path = "" Then
		For Each line In bat_content
			If line.Replace(" ", "").ToUpper.Trim = "CD.." Then current_dir += "\.."
			If line.ToUpper.Contains("DOSBOX") AndAlso line.ToUpper.Contains(".EXE") Then dosbox_path = line.ToUpper : Exit For
		Next
		If dosbox_path <> "" Then
			dosbox_path = dosbox_path.SubstringBetween("""", """").ToUpper
			If dosbox_path.StartsWith(".") Then dosbox_path = current_dir + "\" + dosbox_path
			dosbox_path = Path.GetFullPath(dosbox_path).ToUpper()
		End If
		''End If
		Return dosbox_path
	End Function

	'Show big screenshot
	Private Sub ButtonScreen2_MouseEnter(sender As Object, e As EventArgs) Handles ButtonScreen1.MouseEnter, ButtonScreen2.MouseEnter,
			ButtonScreen3.MouseEnter, ButtonScreen4.MouseEnter, ButtonScreen5.MouseEnter, ButtonScreen6.MouseEnter,
			ButtonScreen7.MouseEnter, ButtonScreen8.MouseEnter, ButtonScreen9.MouseEnter, ButtonScreen10.MouseEnter,
			ButtonScreen11.MouseEnter, ButtonScreen12.MouseEnter, ButtonScreen13.MouseEnter, ButtonScreen14.MouseEnter,
			ButtonScreen15.MouseEnter, ButtonScreen16.MouseEnter, ButtonScreen17.MouseEnter, ButtonScreen18.MouseEnter,
			ButtonScreen19.MouseEnter, ButtonScreen20.MouseEnter

		Dim b = DirectCast(sender, Button)
		If b.Tag IsNot Nothing AndAlso b.Tag.ToString <> "" Then
			Dim p = Me.PointToClient(b.Parent.PointToScreen(b.Location))
			Dim img = Bitmap.FromFile(b.Tag.ToString)
			Dim aspect = img.Width / img.Height

			'BigPictureBox.Top = p.Y + 15
			'BigPictureBox.Left = p.X + 15
			BigPictureBox.Top = p.Y + 25
			BigPictureBox.Left = Me.PointToClient(ButtonScreen1.PointToScreen(ButtonScreen1.Location)).X

			Dim maxW = Me.Width - BigPictureBox.Left - 50
			Dim maxH = Me.Height - BigPictureBox.Top - 50

			BigPictureBox.Image = img
			If maxH * aspect > maxW Then
				BigPictureBox.Width = Me.Width - BigPictureBox.Left - 50
				BigPictureBox.Height = CInt(BigPictureBox.Width / aspect)
			ElseIf maxW / aspect > maxH Then
				BigPictureBox.Height = Me.Height - BigPictureBox.Top - 50
				BigPictureBox.Width = CInt(BigPictureBox.Height * aspect)
			Else
				'Tut mojno vycheslyat maximalnuu storonu
			End If
			BigPictureBox.Visible = True
		End If
	End Sub
	Private Sub ButtonScreen9_MouseLeave(sender As Object, e As EventArgs) Handles ButtonScreen1.MouseLeave, ButtonScreen2.MouseLeave,
			ButtonScreen3.MouseLeave, ButtonScreen4.MouseLeave, ButtonScreen5.MouseLeave, ButtonScreen6.MouseLeave,
			ButtonScreen7.MouseLeave, ButtonScreen8.MouseLeave, ButtonScreen9.MouseLeave, ButtonScreen10.MouseLeave,
			ButtonScreen11.MouseLeave, ButtonScreen12.MouseLeave, ButtonScreen13.MouseLeave, ButtonScreen14.MouseLeave,
			ButtonScreen15.MouseLeave, ButtonScreen16.MouseLeave, ButtonScreen17.MouseLeave, ButtonScreen18.MouseLeave,
			ButtonScreen19.MouseLeave, ButtonScreen20.MouseLeave

		BigPictureBox.Visible = False
	End Sub

	'Live search
	Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
		Dim dv = DirectCast(ListBox1.DataSource, DataTable).DefaultView
		Dim old_selected_ind = ListBox1.SelectedIndex
		Dim old_selected_item = DirectCast(ListBox1.SelectedItem, DataRowView)
		refreshing = True
		ListBox1.BeginUpdate()
		If TextBox1.Text.Trim = "" Then
			dv.RowFilter = ""
		Else
			If liveSearchMenu.MenuItems(0).Checked Then
				If ContainToolStripMenuItem.Checked Then
					dv.RowFilter = "name LIKE '%" + TextBox1.Text.Replace("'", "''").Replace("[", "[[]").Replace("\*", "[*]") + "%'"
				Else
					dv.RowFilter = "name LIKE '" + TextBox1.Text.Replace("'", "''").Replace("[", "[[]").Replace("\*", "[*]") + "%'"
				End If
			Else
				TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(Nothing))
			End If
		End If
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString

		ListBox1.DontChangeSelectionIndexOnDataSourceChange = False
		Dim new_ind As DataRow = Nothing
		If old_selected_item IsNot Nothing Then new_ind = dv.ToTable.AsEnumerable.Where(Function(r) r.Field(Of Int64)(0) = UInt64.Parse(old_selected_item.Item(0))).FirstOrDefault()
		If new_ind IsNot Nothing Then
			'We found the same item as was selected before, reselect it in listbox, and don't need to update entry fields
			ListBox1.SelectedIndex = new_ind.Table.Rows.IndexOf(new_ind)
			refreshing = False
		Else
			'Previously selected item was filtered out
			If old_selected_ind < 0 AndAlso ListBox1.Count > 0 Then
				ListBox1.SelectedIndex = 0
			Else
				If ListBox1.Count > old_selected_ind Then ListBox1.SelectedIndex = old_selected_ind Else ListBox1.SelectedIndex = ListBox1.Count - 1
			End If
			refreshing = False
			ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
		End If
		ListBox1.EndUpdate()
		ListBox1.DontChangeSelectionIndexOnDataSourceChange = True
	End Sub
	Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
		liveSearchMenu.Show(Button9, New Point(5, 5))
	End Sub

	'Show filter panel
	Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
		GroupBox1.Visible = True
	End Sub
	Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
		GroupBox1.Visible = False
	End Sub
	'Filter add handler
	Public Sub filter_add_handler(sender As Object, e As EventArgs)
		Dim b = CType(sender, Button)
		Dim t = Microsoft.VisualBasic.Right(b.Name, 2)
		Dim num As Integer = 0
		If Not IsNumeric(t) Then t = t.Substring(1)
		num = CInt(t)

		Dim fi As New filterItem
		Dim lbl = DirectCast(GroupBox1.Controls("filterLabel" + t), Label)
		Dim filter = lbl.Text + " "

		If b.Name.ToUpper.StartsWith("filterBtnNum".ToUpper) Then
			Dim numeric = DirectCast(GroupBox1.Controls("filterNum" + t), NumericUpDown)
			Dim numcomb = DirectCast(GroupBox1.Controls("filterNumCmb" + t), ComboBox)

			fi.filterShown = filter + numcomb.Text.ToString + " " + numeric.Value.ToString.Replace(",", ".")

			filter = b.Tag + " "
			fi.filterReal = filter + numcomb.Text.ToString + " " + numeric.Value.ToString.Replace(",", ".")

			ListBox2.Items.Add(fi)
			Exit Sub
		End If

		Dim chk = DirectCast(GroupBox1.Controls("filterChk" + t), CheckBox)
		Dim cmb = DirectCast(GroupBox1.Controls("filterCmb" + t), ComboBox)
		If chk.Checked Then filter += "!= " Else filter += "= "
		Dim search = CType(cmb.SelectedItem, DataRowView).Item(0).ToString.Replace("'", "''")

		If String.IsNullOrWhiteSpace(search) Or search.Trim.ToUpper() = "{EMPTY}" Then
			filter += CType(cmb.SelectedItem, DataRowView).Item(0).ToString.Trim() : fi.filterShown = filter

			If Not chk.Checked Then
				filter = "(" + b.Tag + " is null OR TRIM(" + b.Tag + ") = '')"
			Else
				filter = "(" + b.Tag + " is not null AND TRIM(" + b.Tag + ") != '')"
			End If
			fi.filterReal = filter
			ListBox2.Items.Add(fi)
			Exit Sub
		End If

		filter += CType(cmb.SelectedItem, DataRowView).Item(0).ToString : fi.filterShown = filter
		If lbl.Tag.ToString <> "GRP" Then
			'Not value-groupped
			If cmb.Tag = "LIST" Then
				'If LIST values (Not value-groupped)
				filter = "(" + b.Tag + " "
				If chk.Checked Then filter += "NOT "
				filter += "LIKE '" + search + "' "
				If chk.Checked Then filter += "AND " Else filter += "OR "
				filter += b.Tag + " "
				If chk.Checked Then filter += "NOT "
				filter += "LIKE '" + search + ",%' "
				If chk.Checked Then filter += "AND " Else filter += "OR "
				filter += b.Tag + " "
				If chk.Checked Then filter += "NOT "
				filter += "LIKE '%, " + search + "' "
				If chk.Checked Then filter += "AND " Else filter += "OR "
				filter += b.Tag + " "
				If chk.Checked Then filter += "NOT "
				filter += "LIKE '%, " + search + ",%' )"
				If chk.Checked Then filter = "( " + filter + " OR " + b.Tag + " IS NULL )"
				fi.filterReal = filter
			Else
				'If regular filter (Not value-groupped)
				filter = "(" + b.Tag + " "
				If chk.Checked Then filter += "NOT "
				filter += "LIKE '" + search + "'"

				'We need this hack, because LIKE does not return row if a field is a null
				If chk.Checked Then filter += " OR " + b.Tag + " is null)" Else filter += ")"

				fi.filterReal = filter
			End If
		Else
			'Value-groupped
			If cmb.Tag = "LIST" Then
				'If LIST values (value-groupped)
				Dim filterlist As New List(Of String)
				Dim r = db.queryReader("SELECT value FROM groups WHERE name = '" + search + "'")
				r.Read()
				For Each listVal In r.GetString(0).Split({";;%;;"}, StringSplitOptions.RemoveEmptyEntries)
					filter = "(" + b.Tag + " "
					If chk.Checked Then filter += "NOT "
					filter += "LIKE '" + listVal.Replace("'", "''") + "' "
					If chk.Checked Then filter += "AND " Else filter += "OR "
					filter += b.Tag + " "
					If chk.Checked Then filter += "NOT "
					filter += "LIKE '" + listVal.Replace("'", "''") + ",%' "
					If chk.Checked Then filter += "AND " Else filter += "OR "
					filter += b.Tag + " "
					If chk.Checked Then filter += "NOT "
					filter += "LIKE '%, " + listVal.Replace("'", "''") + "' "
					If chk.Checked Then filter += "AND " Else filter += "OR "
					filter += b.Tag + " "
					If chk.Checked Then filter += "NOT "
					filter += "LIKE '%, " + listVal.Replace("'", "''") + ",%' )"
					filterlist.Add(filter)
				Next

				If chk.Checked Then filter = "( ( " + String.Join(" AND ", filterlist) + " ) OR " + b.Tag + " IS NULL )" Else filter = "( " + String.Join(" OR ", filterlist) + " )"
				fi.filterReal = filter
			Else
				'If regular filter (value-groupped)
				Dim r = db.queryReader("SELECT value FROM groups WHERE name = '" + search + "'")
				r.Read()
				Dim list = ""
				For Each listVal In r.GetString(0).Split({";;%;;"}, StringSplitOptions.RemoveEmptyEntries)
					list += "'" + listVal.Replace("'", "''") + "', "
				Next
				list = list.Substring(0, list.Length - 2)

				filter = "(" + b.Tag + " "
				If chk.Checked Then filter += " NOT IN " Else filter += " IN "
				filter += "(" + list + " )"
				If chk.Checked Then filter += " OR  " + b.Tag + " IS NULL"
				filter += " )"
				fi.filterReal = filter
			End If
		End If

		ListBox2.Items.Add(fi)
	End Sub
	'Remove filter
	Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
		If ListBox2.SelectedIndex < 0 Then Exit Sub

		Dim ind = ListBox2.SelectedIndex
		ListBox2.Items.RemoveAt(ListBox2.SelectedIndex)
		If ListBox2.Items.Count > ind Then
			ListBox2.SelectedIndex = ind
		ElseIf ListBox2.Items.Count > 0 Then
			ListBox2.SelectedIndex = ListBox2.Items.Count - 1
		End If
	End Sub
	'Apply filter
	Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode))

		'Save lastFilter in config
		Dim c As Integer = 0
		For i As Integer = 0 To ListBox2.Items.Count - 1
			Dim f As filterItem = DirectCast(ListBox2.Items(i), filterItem)
			ini.IniWriteValue("Interface", "LastFilter" + i.ToString, f.filterShown + ";;;" + f.filterReal)
			c += 1
		Next
		ini.IniWriteValue("Interface", "LastFilter" + c.ToString, "")
	End Sub
	'Change filter logic
	Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
		If Label1.Text.ToUpper = "AND" Then Label1.Text = "OR" Else Label1.Text = "AND"
	End Sub
	'Filter Preset Load
	Private Sub Preset_FilterLoad_Click(sender As Object, e As EventArgs) Handles Preset_FilterLoad.Click
		If Preset_ComboBox.SelectedIndex < 0 Then MsgBox("You have to choose the filter to load from drop down list.") : Exit Sub
		Dim n = Preset_ComboBox.Items(Preset_ComboBox.SelectedIndex).ToString

		ListBox2.Items.Clear()
		Dim c As Integer = 0
		Do While ini.IniReadValue("FilterPresets", n + c.ToString).Trim <> ""
			Dim flt = ini.IniReadValue("FilterPresets", n + c.ToString).Trim.Split({";;;"}, StringSplitOptions.RemoveEmptyEntries)
			If flt.Count = 2 Then
				Dim fi As New filterItem
				fi.filterShown = flt(0)
				fi.filterReal = flt(1)
				ListBox2.Items.Add(fi)
			End If
			c += 1
		Loop
	End Sub
	'Filter Preset Save
	Private Sub Preset_FilterSave_Click(sender As Object, e As EventArgs) Handles Preset_FilterSave.Click
		If ListBox2.Items.Count = 0 Then MsgBox("Can't save empty preset.") : Exit Sub
		If Preset_ComboBox.Text.Trim = "" Then MsgBox("Preset name can't be empty.") : Exit Sub

		If Preset_ComboBox.SelectedIndex < 0 Then
			For i As Integer = 0 To Preset_ComboBox.Items.Count - 1
				If Preset_ComboBox.Text.Trim.ToUpper = Preset_ComboBox.Items(i).ToString.ToUpper Then Preset_ComboBox.SelectedIndex = i : Exit For
			Next
		End If
		If Preset_ComboBox.SelectedIndex >= 0 Then
			Dim res = MsgBox("Overwrite '" + Preset_ComboBox.Items(Preset_ComboBox.SelectedIndex) + "'", MsgBoxStyle.YesNo)
			If res = MsgBoxResult.No Then Exit Sub
		Else
			Preset_ComboBox.Items.Add(Preset_ComboBox.Text)
			Preset_ComboBox.SelectedIndex = Preset_ComboBox.Items.Count - 1
		End If

		'Save filter preset in config
		Dim n As String = ""
		For i As Integer = 0 To Preset_ComboBox.Items.Count - 1
			n += Preset_ComboBox.Items(i).ToString + "%%%"
		Next
		If n.Length > 3 Then n = n.Substring(0, n.Length - 3)
		ini.IniWriteValue("FilterPresets", "Names", n)

		Dim c As Integer = 0
		For i As Integer = 0 To ListBox2.Items.Count - 1
			Dim f As filterItem = DirectCast(ListBox2.Items(i), filterItem)
			ini.IniWriteValue("FilterPresets", Preset_ComboBox.Items(Preset_ComboBox.SelectedIndex) + i.ToString, f.filterShown + ";;;" + f.filterReal)
			c += 1
		Next
		ini.IniWriteValue("FilterPresets", Preset_ComboBox.Items(Preset_ComboBox.SelectedIndex) + c.ToString, "")
	End Sub

	'Press custom list button (favorites, remember e.t.c.)
	Private Sub press_custom_list_button(ByVal sender As Object, ByVal e As System.EventArgs)
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim b = DirectCast(sender, Button)
		Dim n = DirectCast(b.Tag, Integer())(0) - 1
		Dim id = DirectCast(b.Tag, Integer())(1)
		If b.BackgroundImage Is customListImages(n) Then
			Dim sql As String = "DELETE FROM custom_lists_data "
			sql += "WHERE id_main = " + ListBox1.SelectedValue.ToString + " and id_list = " + id.ToString
			db.execute(sql)
			b.BackgroundImage = customListImagesGray(n)
		Else
			Dim sql As String = "INSERT INTO custom_lists_data (id_main, id_list) "
			sql += "VALUES (" + ListBox1.SelectedValue.ToString + ", " + id.ToString + ")"
			db.execute(sql)
			b.BackgroundImage = customListImages(n)
		End If
		ListBox1.Focus()
	End Sub

	'--------MENU---------
	'File / Import (Daz catalog yet)
	Private Sub ImportToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ImportToolStripMenuItem.Click
		Dim fb As New OpenFileDialog
		fb.InitialDirectory = ini.IniReadValue("Paths", "LastOpenedImportDir")
		fb.Multiselect = True
		fb.ShowDialog()
		If fb.FileNames.Count = 0 Then Exit Sub

		Dim LastOpenedImportDir = fb.FileNames(0).Substring(0, fb.FileNames(0).LastIndexOf("\"))
		ini.IniWriteValue("Paths", "LastOpenedImportDir", LastOpenedImportDir)

		Dim bg As New BackgroundWorker
		If type = catalog_type.daz Then
			AddHandler bg.DoWork, AddressOf Import_bgWork_daz
		ElseIf type = catalog_type.games Then
			AddHandler bg.DoWork, AddressOf Import_bgWork_games
		ElseIf type = catalog_type.unity Then
			AddHandler bg.DoWork, AddressOf Import_bgWork_unity
		End If
		AddHandler bg.RunWorkerCompleted, Sub() init_categories()
		bg.RunWorkerAsync(fb.FileNames)
	End Sub
	Private Sub Import_bgWork_daz(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs)
		Try
			Dim filelistReseted = False
			Dim files = DirectCast(e.Argument, String())
			Array.Sort(files)
			For Each file In files
				Dim fileJustName = file.Substring(file.LastIndexOf("\") + 1)
				If Not fileJustName.ToUpper.StartsWith("PRODUCT_RO") And file.ToUpper.EndsWith("_DATA.TXT") Then
					Dim tmp = file
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")))
					Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					db.execute("BEGIN;")
					For Each line In lines
						Dim f As String = file.Substring(file.LastIndexOf("\") + 1)
						Dim cat As String = f.Substring(0, f.IndexOf("_data")).Replace(" - ", ".")
						Dim cat_arr = cat.Split({"."c}, StringSplitOptions.RemoveEmptyEntries)
						cat = Join(cat_arr, "/")

						Dim str = line.Split({"%%@%%"}, System.StringSplitOptions.None)
						If str.Count < 9 Then
							ReDim Preserve str(8)
							str(8) = ""
						End If
						For i As Integer = 0 To 8
							str(i) = Net.WebUtility.HtmlDecode(str(i))
						Next

						Dim sql As String = "SELECT id FROM main "
						sql += "WHERE name = '" + str(1).Replace("'", "''") + "'"
						Dim reader = db.queryReader(sql)

						If Not reader.HasRows Then
							Dim prices = str(2).Trim.Replace(" ", "").Split({"/"c}, System.StringSplitOptions.None)
							If prices(0).Trim = "" Or prices(0).Trim.ToLower = "free" Then prices(0) = 0
							If prices(1).Trim = "" Or prices(1).Trim.ToLower = "free" Then prices(1) = 0
							If prices(2).Trim = "" Or prices(2).Trim.ToLower = "free" Then prices(2) = 0
							sql = "INSERT INTO main "
							sql += "(name, data_dec1, data_dec2, data_dec3, "
							sql += "data_str1, data_str2, data_str3, "
							sql += "data_str4, data_str5, data_str6, data_num1) "
							sql += " VALUES ( '" + str(1).Replace("'", "''") + "', "
							sql += prices(0).Replace("$", "") + ", " + prices(1).Replace("$", "") + ", " + prices(2).Replace("$", "") + ", "
							sql += "'" + str(3).Replace("'", "''") + "', '" + str(4).Replace("'", "''") + "', '" + str(5).Replace("'", "''") + "', "
							sql += "'" + str(6).Replace("'", "''") + "', '" + str(7).Replace("'", "''") + "', '" + str(8).Replace("'", "''") + "', '" + str(0) + "'); "
							db.execute(sql)

							Dim lastRow = db.getLastRowID
							sql = "INSERT INTO category (id_main, cat) VALUES ("
							sql += lastRow.ToString + ", '" + cat + "'); "
							db.execute(sql)
						Else
							reader.Read()
							Dim id_main = reader.GetInt32(0)
							sql = "SELECT main.id FROM main "
							sql += "JOIN category ON main.id = category.id_main "
							sql += "WHERE name = '" + str(1).Replace("'", "''") + "' "
							sql += "AND category.cat = '" + cat + "'"
							reader = db.queryReader(sql) : reader.Read()

							If Not reader.HasRows Then
								sql = "INSERT INTO category (id_main, cat) VALUES ("
								sql += id_main.ToString + ", '" + cat + "'); "
								db.execute(sql)
							End If
						End If
					Next
					db.execute("COMMIT;")
				End If

				If fileJustName.ToUpper.StartsWith("PRODUCT_RO") And Not fileJustName.ToUpper.EndsWith("_DESCR.TXT") And Not fileJustName.ToUpper.EndsWith("_IMAGES.TXT") Then
					Dim tmp = file
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")))
					Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					db.execute("BEGIN;")
					For Each line In lines
						Dim str = line.Split({"%%@%%"}, System.StringSplitOptions.None)
						'0 - name
						'1 - cat, separated by commas
						'2 - artist
						'3 - price
						'4 - software
						'5 - file types
						'6 - base figure
						'7 - required

						Dim sql As String = "SELECT id FROM main "
						sql += "WHERE name = '" + str(1).Replace("'", "''") + "'"
						Dim reader = db.queryReader(sql)

						Dim price = str(3).Replace("USD", "").Trim.Replace(" ", "")
						If Not reader.HasRows Then
							sql = "INSERT INTO main "
							sql += "(name, data_str1, data_dec1, data_str3, data_str4, data_str2, data_str5) "
							sql += " VALUES ( '" + str(0).Replace("'", "''") + "', "
							sql += "'" + str(2).Replace("'", "''") + "', " + price + ", '" + str(4).Replace("'", "''") + "', "
							sql += "'" + str(5).Replace("'", "''") + "', '" + str(6).Replace("'", "''") + "', '" + str(7).Replace("'", "''") + "'); "
							db.execute(sql)


							Dim cat = str(1).Split({","}, System.StringSplitOptions.RemoveEmptyEntries)
							Dim lastRow = db.getLastRowID
							For Each c In cat
								sql = "INSERT INTO category (id_main, cat) VALUES ("
								sql += lastRow.ToString + ", '" + c.Trim + "'); "
								db.execute(sql)
							Next
						End If
					Next
					db.execute("COMMIT;")
				End If
			Next
			For Each file In files
				Dim fileJustName = file.Substring(file.LastIndexOf("\") + 1)
				If Not fileJustName.ToUpper.StartsWith("PRODUCT_RO") And file.ToUpper.EndsWith("_DESCR.TXT") Then
					Dim tmp = file
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")))

					Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					Dim curName As String = ""
					Dim nextLineIs As String = ""
					db.execute("BEGIN;")
					For Each line In lines
						If line.StartsWith("[") And Not line.ToUpper.StartsWith("[included]".ToUpper) And Not line.ToUpper.StartsWith("[notes]".ToUpper) Then
							curName = line.Substring(1, line.Length - 2)
							nextLineIs = "link"
						ElseIf line.ToUpper.StartsWith("[included]".ToUpper) Then
							nextLineIs = "ink"
						ElseIf line.ToUpper.StartsWith("[notes]".ToUpper) Then
							nextLineIs = "notes"
						Else
							If nextLineIs = "link" Then
								Dim sql As String = "UPDATE main "
								sql += "SET data_str7 = '" + line.Replace("'", "''") + "' "
								sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
								db.execute(sql)
								nextLineIs = "descr"
							ElseIf nextLineIs = "descr" Then
								Dim sql As String = "UPDATE main "
								sql += "SET data_txt1 = '" + line.Replace("'", "''") + "' "
								sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
								db.execute(sql)
							ElseIf nextLineIs = "ink" Then
								Dim sql As String = "UPDATE main "
								sql += "SET data_txt2 = '" + line.Replace("'", "''") + "' "
								sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
								db.execute(sql)
							ElseIf nextLineIs = "notes" Then
								Dim sql As String = "UPDATE main "
								sql += "SET data_txt3 = '" + line.Replace("'", "''") + "' "
								sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
								db.execute(sql)
							End If
						End If
					Next
					db.execute("COMMIT;")
				End If


				If fileJustName.ToUpper.StartsWith("PRODUCT_RO") And file.ToUpper.EndsWith("_DESCR.TXT") Then
					Dim tmp = file
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")))

					Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					Dim curName As String = ""
					Dim curLink As String = ""
					db.execute("BEGIN;")
					For Each line In lines
						If line.StartsWith("[") Then
							curName = line.Substring(1, line.Length - 2)
						ElseIf line.StartsWith("%%%LINK=") Then
							curLink = line.Substring(8)
						Else
							Dim sql As String = "UPDATE main "
							sql += "SET "
							sql += "data_str7 = '" + curLink.Replace("'", "''") + "', "
							sql += "data_txt1 = '" + line.Replace("'", "''") + "' "
							sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
							db.execute(sql)
						End If
					Next
					db.execute("COMMIT;")
				End If
			Next
			For Each file In files
				If file.ToLower.Contains("sku_master_daz") Then
					Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					db.execute("BEGIN;")
					For Each line In lines
						Dim spc1 = line.IndexOf(" ")
						Dim tmp = line.Substring(spc1).Trim
						Dim spc2 = tmp.IndexOf(" ")
						Dim code1 As String = line.Substring(0, spc1).Trim
						Dim code2 As String = tmp.Substring(0, spc2).Trim
						Dim name As String = tmp.Substring(spc2 + 1).Trim
						Label_zero.BeginInvoke(Sub() Label_zero.Text = "Importing SKU - " + name)

						Dim sql = "UPDATE main "
						sql += "SET data_num1 = " + code2 + ", data_str8 = '" + code1 + "' "
						sql += "WHERE name = '" + Net.WebUtility.HtmlDecode(name).Replace("'", "''") + "';"
						db.execute(sql)

						sql = "UPDATE main "
						sql += "SET data_num1 = " + code2 + ", data_str8 = '" + code1 + "' "
						sql += "WHERE name = 'zz - " + Net.WebUtility.HtmlDecode(name).Replace("'", "''") + "';"
						db.execute(sql)
					Next
					db.execute("COMMIT;")
				End If
			Next
			For Each file In files
				If file.ToLower.Contains("fileLists-".ToLower) Then
					Dim sql = ""
					Dim main_id As Integer = 0
					'If Not filelistReseted Then filelistReseted = True : db.execute("DELETE FROM product_files")

					'Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
					'Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

					Dim str = My.Computer.FileSystem.OpenTextFileReader(file)

					db.execute("BEGIN;")
					Dim c As Integer = 0
					Dim cUpdated As Integer = 0
					Dim curName As String = ""
					Dim curcode As Integer = 0
					Dim curSubName As String = "default"
					Do While Not str.EndOfStream
						'For Each line In lines
						Dim line = str.ReadLine
						c += 1
						If line.StartsWith("[") Then
							main_id = 0
							curcode = 0
							curSubName = "default"
							curName = line.Substring(1, line.Length - 2)
						ElseIf line.ToUpper.StartsWith("CODE=") Then
							curcode = CInt(line.Substring(5))
							Dim status = "Processing SKU - " + curcode.ToString + " "
							'status += "(" + c.ToString + "/" + lines.Count.ToString + ") "
							status += "up=" + cUpdated.ToString
							Label_zero.BeginInvoke(Sub() Label_zero.Text = status)
						ElseIf line.ToUpper.StartsWith("SUBNAME=") Then
							curSubName = line.Substring(8)
						ElseIf line.StartsWith("/") Or line.StartsWith("\") Then
							line = line.Replace("\", "/")
							If curcode > 0 Then
								If main_id = 0 Then
									sql = "SELECT id FROM main WHERE data_num1 = " + curcode.ToString
									Dim r = db.queryReader(sql)
									If r.HasRows Then
										r.Read()
										main_id = r.GetInt32(0)
									Else
										main_id = -1
									End If
								End If

								If main_id > 0 Then
									sql = "INSERT INTO product_files (id_main, subName, file) "
									sql += "VALUES (" + main_id.ToString + ", '" + curSubName.Trim.Replace("'", "''") + "', '" + line.Trim.Replace("'", "''") + "'); "
									db.execute(sql)
									cUpdated += 1
								End If
							End If
						Else
							'MsgBox("Unknown Line")
						End If
						'Next
					Loop
					db.execute("COMMIT;")
				End If
			Next
			Label_zero.BeginInvoke(Sub() Label_zero.Text = "DONE")

		Catch ex As Exception
			MsgBox(ex.Message)
		End Try
	End Sub
	Private Sub Import_bgWork_games(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs)
		Dim files = DirectCast(e.Argument, String())
		Array.Sort(files)
		For Each file In files
			If file.ToUpper.EndsWith("FULL0.TXT") Then
				Dim tmp = file
				Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
				Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)
				Dim all_descr = My.Computer.FileSystem.OpenTextFileReader(file.ToUpper.Replace("FULL0", "FULL_DESCRIPTIONS0")).ReadToEnd
				Dim lines_descr = all_descr.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)
				Dim descrs As New List(Of String())
				Dim nextIsLink As Boolean = False
				For Each line In lines_descr
					If line.StartsWith("[") Then
						Name = line.Substring(1, line.Length - 2)
						descrs.Add({Name, "", ""})
						nextIsLink = True
					ElseIf nextIsLink Then
						descrs(descrs.Count - 1)(1) = line
						nextIsLink = False
					Else
						descrs(descrs.Count - 1)(2) += line + vbCrLf
					End If
				Next

				db.execute("BEGIN;")
				Dim counter As Integer = 0
				For Each line In lines
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")) + " " + counter.ToString + "/" + lines.Count.ToString)

					Dim f As String = file.Substring(file.LastIndexOf("\") + 1)
					Dim cat As String = f.Substring(0, f.IndexOf("DB")).Trim
					Dim str = line.Split({"%@%"}, System.StringSplitOptions.None)
					str(0) = Net.WebUtility.HtmlDecode(str(0))

					Dim mobyid = descrs(counter)(1).Substring(descrs(counter)(1).LastIndexOf("/") + 1)
					Dim sql As String = "SELECT main.id FROM main "
					sql += "LEFT OUTER JOIN category ON main.id = category.id_main "
					sql += "WHERE data_str15 = '" + mobyid.Replace("'", "''") + "'"
					Dim reader = db.queryReader(sql)

					If Not reader.HasRows Then
						sql = "INSERT INTO main "
						sql += "(name, data_num1, data_str1, "
						sql += "data_str2, data_str3, data_str4, "
						sql += "data_str5, data_str6, data_str7, "
						sql += "data_str8, data_str9, data_str10, "
						sql += "data_str11, data_str12, data_str13, "
						sql += "data_str15, "
						sql += "data_txt1) "
						sql += " VALUES ( '" + str(1).Replace("'", "''") + "', " + str(2) + ", '" + descrs(counter)(1).Replace("'", "''") + "', "
						sql += "'" + str(3).Replace("'", "''") + "', '" + str(4).Replace("'", "''") + "', '" + str(5).Replace("'", "''") + "', "
						sql += "'" + str(6).Replace("'", "''") + "', '" + str(7).Replace("'", "''") + "', '" + str(8).Replace("'", "''") + "', "
						sql += "'" + str(9).Replace("'", "''") + "', '" + str(10).Replace("'", "''") + "', '" + str(11).Replace("'", "''") + "', "
						sql += "'" + str(12).Replace("'", "''") + "', '" + str(13).Replace("'", "''") + "', '" + str(14).Replace("'", "''") + "', "
						sql += "'" + mobyid.Replace("'", "''") + "', "
						sql += "'" + descrs(counter)(2).Replace("'", "''") + "'); "
						db.execute(sql)

						Dim lastRow = db.getLastRowID
						sql = "INSERT INTO category (id_main, cat) VALUES ("
						sql += lastRow.ToString + ", '" + cat + "'); "
						db.execute(sql)
					Else
						reader.Read()
						Dim id_main = reader.GetInt32(0)
						sql = "SELECT main.id FROM main "
						sql += "JOIN category ON main.id = category.id_main "
						sql += "WHERE data_str15 = '" + mobyid.Replace("'", "''") + "'"
						sql += "AND category.cat = '" + cat + "'"
						reader = db.queryReader(sql) : reader.Read()
						If Not reader.HasRows Then
							sql = "INSERT INTO category (id_main, cat) VALUES ("
							sql += id_main.ToString + ", '" + cat + "'); "
							db.execute(sql)
						End If
					End If
					counter += 1
				Next
				db.execute("COMMIT;")
			End If
		Next
		Label_zero.BeginInvoke(Sub() Label_zero.Text = "DONE")
	End Sub
	Private Sub Import_bgWork_unity(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs)
		Dim files = DirectCast(e.Argument, String())
		Array.Sort(files)
		For Each file In files
			If Not file.ToUpper.EndsWith("_Descriptions.txt".ToUpper) Then
				Dim tmp = file
				Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
				Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)
				Dim all_descr = My.Computer.FileSystem.OpenTextFileReader(file.ToUpper.Replace(".TXT", "_Descriptions.txt")).ReadToEnd
				Dim lines_descr = all_descr.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)
				Dim descrs As New List(Of String())
				Dim nextIsLink As Boolean = False
				For Each line In lines_descr
					If line.StartsWith("[") Then
						Name = line.Substring(1, line.Length - 2)
						descrs.Add({Name, "", ""})
						nextIsLink = True
					ElseIf nextIsLink Then
						descrs(descrs.Count - 1)(1) = line
						nextIsLink = False
					Else
						descrs(descrs.Count - 1)(2) += line + vbCrLf
					End If
				Next

				'Name
				'-Category
				'Publisher
				'Rating
				'Rating Voted
				'-
				'Price
				'Version
				'Size
				'-
				'Originaly released
				'Submited
				'---
				'Link

				db.execute("BEGIN;")
				Dim sql As String = ""
				Dim counter As Integer = 0
				For Each line In lines
					Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp.Substring(tmp.LastIndexOf("\")) + " " + counter.ToString + "/" + lines.Count.ToString)

					Dim str = line.Split({"%%@%%"}, System.StringSplitOptions.None)
					str(0) = Net.WebUtility.HtmlDecode(str(0))
					'Dim link = descrs(counter)(1).Substring(descrs(counter)(1).LastIndexOf("/") + 1)
					Dim link = descrs(counter)(1).Trim

					'Rating
					str(3) = str(3).Trim
					If str(3) = "" Or str(3).ToUpper = "NOT" Then str(3) = "0"
					If str(4).Trim = "" Then str(4) = "0"
					'Price
					str(5) = str(5).Trim.Replace(" ", "").Replace("$", "")
					If str(5).ToUpper = "FREE" Then str(5) = "0"
					'Size
					str(7) = str(7).Trim.Replace(" ", "").ToUpper
					Dim multiplier = 1
					If str(7).EndsWith("KB") Then multiplier = 1024 : str(7) = str(7).Replace("KB", "")
					If str(7).EndsWith("MB") Then multiplier = 1048576 : str(7) = str(7).Replace("MB", "")
					If str(7).EndsWith("GB") Then multiplier = 1073741824 : str(7) = str(7).Replace("GB", "")
					If str(7).EndsWith("BYTES") Then str(7) = str(7).Replace("BYTES", "")
					Try
						str(7) = (CDec(str(7).Replace(".", ",")) * multiplier).ToString
						If str(7).Contains(",") Then str(7) = str(7).Substring(0, str(7).LastIndexOf(","))
					Catch ex As Exception
						MsgBox(ex.Message + vbCrLf + "Product: " + str(0) + vbCrLf + vbCrLf + "'" + str(7) + "'") : Exit Sub
					End Try

					sql = "INSERT INTO main "
					sql += "(name, "
					sql += "data_str1, data_num1, data_num2, "
					sql += "data_dec1, data_str2, data_num3, "
					sql += "data_str3, data_str4, data_str5, "
					sql += "data_txt1) "
					sql += " VALUES ( '" + str(0).Replace("'", "''") + "', "
					sql += "'" + str(2).Replace("'", "''") + "', " + str(3) + ", " + str(4) + ", "
					sql += "" + str(5) + ", '" + str(6).Replace("'", "''") + "', " + str(7) + ", "
					sql += "'" + str(8).Replace("'", "''") + "', '" + str(9).Replace("'", "''") + "', '" + link.Replace("'", "''") + "', "
					sql += "'" + descrs(counter)(2).Replace("'", "''") + "'); "
					Try
						db.execute(sql)
					Catch ex As Exception
						MsgBox(ex.Message + vbCrLf + vbCrLf + sql) : Exit Sub
					End Try

					Dim lastRow = db.getLastRowID
					sql = "INSERT INTO category (id_main, cat) VALUES ("
					sql += lastRow.ToString + ", '" + str(1).Replace("'", "''") + "'); "
					Try
						db.execute(sql)
					Catch ex As Exception
						MsgBox(ex.Message + vbCrLf + vbCrLf + sql) : Exit Sub
					End Try

					counter += 1
				Next
				db.execute("COMMIT;")
			End If
		Next
		Label_zero.BeginInvoke(Sub() Label_zero.Text = "DONE")
	End Sub
	'File / Import Advanced
	Private Sub ImportAdvancedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportAdvancedToolStripMenuItem.Click
		Dim f As New FormB_ImportContent
		f.ShowDialog(Me)
		init_categories()
	End Sub
	'File / Import Screenshots
	Private Sub ImportScreenshotsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportScreenshotsToolStripMenuItem.Click
		Dim f As New FormC_ImportScreenshots
		f.ShowDialog(Me)
		ListBox1_SelectedIndexChanged(ListBox1, New System.EventArgs)
	End Sub
	'File / Scan
	Private Sub ScanToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ScanToolStripMenuItem.Click
		If type = catalog_type.daz Then
			Dim f As New Form6_Scanner_daz
			f.ShowDialog(Me)
		ElseIf type = catalog_type.games Then
			Dim f As New Form6_Scanner_games
			f.ShowDialog(Me)
		ElseIf type = catalog_type.unity Then
			Dim f As New Form6_Scanner_unity
			f.ShowDialog(Me)
		End If
	End Sub
	'File / Export List
	Private Sub ExportListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportListToolStripMenuItem.Click
		Dim fd As New SaveFileDialog
		fd.Filter = "Text Files|*.txt"
		fd.Title = "Export List"
		fd.ShowDialog()

		If fd.FileName <> "" Then
			FileOpen(1, fd.FileName, OpenMode.Output)
			For i As Integer = 0 To ListBox1.Items.Count - 1
				PrintLine(1, DirectCast(ListBox1.Items(i), DataRowView).Item(1).ToString)
			Next
			FileClose(1)
		End If
	End Sub
	'File / Grabber
	Private Sub GrabberToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GrabberToolStripMenuItem.Click
		Gecko.Xpcom.Initialize("GeckoFX\Firefox")
		Dim f As New FormE_Grabber
		f.Show(Me)
	End Sub
	'File / Import Custom Data
	Private Sub ImportCustomDataToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportCustomDataToolStripMenuItem.Click
		Dim fb As New OpenFileDialog
		fb.Filter = "Text Files|*.txt"
		fb.Title = "Import List"
		fb.ShowDialog()
		If fb.FileName <> "" Then
			Dim f = New Form9_ImportCustoms(fb.FileName) : f.ShowDialog()
		End If
		Exit Sub

		If fb.FileName <> "" Then
			FileOpen(1, fb.FileName, OpenMode.Input)
			Dim curID As Integer = 0
			Dim curName As String = ""
			Dim mismatchCount As Integer = 0
			db.execute("BEGIN;")
			Do While Not EOF(1)
				Dim l = LineInput(1).Trim
				If l.StartsWith("[") Then
					curID = CInt(l.Substring(1, l.Length - 2))
				ElseIf l.ToUpper.StartsWith("NAME=") Then
					curName = l.Substring(l.IndexOf("=") + 1).Replace("'", "''").Trim
				Else
					Dim field = l.Substring(0, l.IndexOf("="))
					Dim data = l.Substring(l.IndexOf("=") + 1).Replace("'", "''").Trim
					db.execute("UPDATE main SET " + field + " = '" + data + "' WHERE id = " + curID.ToString + " AND trim(name) = '" + curName + "'")
					If db.affectedRows = 0 Then
						mismatchCount += 1
						log("entry with ID=" + curID.ToString + " and name=" + curName.Trim + " not found in database.", "customDataImporter")
					End If
				End If
			Loop
			db.execute("COMMIT;")
			FileClose(1)

			If mismatchCount = 0 Then
				MsgBox("Done.")
			Else
				MsgBox("Done." + vbCrLf + mismatchCount.ToString + " name/id pair mismatch warnings. See Catalog 2016.log for details.")
			End If
		End If
	End Sub
	'File / Export Custom Data
	Private Sub ExportCustomDataToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportCustomDataToolStripMenuItem.Click
		Dim f As New Form9_ExportCustoms
		f.ShowDialog(Me)
	End Sub
	'File / Exit
	Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
		Application.Exit()
	End Sub

	'View / Show status table
	Private Sub ShowStatusTableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ShowStatusTableToolStripMenuItem.Click
		Dim f As New Form4_statusTable(Me)
		f.Show(Me)
	End Sub
	'View / Show Library scan details
	Private Sub ShowLibraryScanDetailsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowLibraryScanDetailsToolStripMenuItem.Click
		Dim f As New Form6_Scanner_Details
		f.Show()
	End Sub
	'View / Show Files scan details
	Private Sub ShowFilesScanDetailsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowFilesScanDetailsToolStripMenuItem.Click
		Dim f As New Form6_Scanner_Details2
		f.Show()
	End Sub
	'View / Show Name Label
	Private Sub ShowNameLabelforEasyCopyingToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowNameLabelforEasyCopyingToolStripMenuItem.Click
		ShowNameLabelforEasyCopyingToolStripMenuItem.Checked = Not ShowNameLabelforEasyCopyingToolStripMenuItem.Checked
		If ShowNameLabelforEasyCopyingToolStripMenuItem.Checked Then EditNameToolStripMenuItem.Checked = False
		'ini.IniWriteValue("Interface", "ShowNameLabel", ShowNameLabelforEasyCopyingToolStripMenuItem.Checked.ToString)

		If Not ShowNameLabelforEasyCopyingToolStripMenuItem.Checked And Not EditNameToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "False")
		ElseIf ShowNameLabelforEasyCopyingToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "1")
		ElseIf EditNameToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "2")
		Else
			MsgBox("Internal error: ququndra.")
		End If

		init_fieldset(True)
		ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub
	'View / Edit Name Label
	Private Sub EditNameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditNameToolStripMenuItem.Click
		EditNameToolStripMenuItem.Checked = Not EditNameToolStripMenuItem.Checked
		If EditNameToolStripMenuItem.Checked Then ShowNameLabelforEasyCopyingToolStripMenuItem.Checked = False

		If Not ShowNameLabelforEasyCopyingToolStripMenuItem.Checked And Not EditNameToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "False")
		ElseIf ShowNameLabelforEasyCopyingToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "1")
		ElseIf EditNameToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "ShowNameLabel", "2")
		Else
			MsgBox("Internal error: ququndra.")
		End If

		init_fieldset(True)
		ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub
	'View / Sorting
	Private Sub sort(sender As Object, e As EventArgs) Handles ReverseSortOrder.Click
		If sender Is ReverseSortOrder Then
			'Reverse Sort Order
			ReverseSortOrder.Checked = Not ReverseSortOrder.Checked
			ini.IniWriteValue("Interface", "LastSortReverse", ReverseSortOrder.Checked.ToString())
		Else
			'Sort By
			For i As Integer = 0 To SortToolStripMenuItem.DropDownItems.Count - 1
				If SortToolStripMenuItem.DropDownItems.Item(i) Is ReverseSortOrder Then Continue For
				If SortToolStripMenuItem.DropDownItems.Item(i).GetType Is GetType(ToolStripSeparator) Then Continue For
				DirectCast(SortToolStripMenuItem.DropDownItems.Item(i), ToolStripMenuItem).Checked = False
			Next
			DirectCast(sender, ToolStripMenuItem).Checked = True
			ini.IniWriteValue("Interface", "LastSort", DirectCast(sender, ToolStripMenuItem).Text.Trim)
		End If
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode))
	End Sub
	'View / View custom list
	Private Sub viewListClick(sender As Object, e As EventArgs)
		Dim item = DirectCast(sender, ToolStripMenuItem)
		For Each i As ToolStripMenuItem In ViewListToolStripMenuItem.DropDownItems
			i.Checked = False
		Next
		item.Checked = True
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(New TreeNode))
	End Sub
	'View / Filter by .txt file
	Private Sub FilterBytxtFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FilterBytxtFileToolStripMenuItem.Click
		filter_txt_file.Clear()

		Dim fb As New OpenFileDialog
		fb.Filter = "*.txt|*.txt"
		fb.Multiselect = False
		fb.ShowDialog()
		If fb.FileName = "" Then Exit Sub

		Dim f_arr = IO.File.ReadAllLines(fb.FileName)
		For Each l In f_arr
			l = l.Trim
			If l = "" Then Continue For
			l = l.ToUpper
			If l.StartsWith("THE ") Then l = l.Substring(4) + ", THE"
			l = l.Replace("-", " ").Replace(":", " ").Replace("=", " ").Replace("!", " ").Replace(".", " ").Replace(",", " ")
			l = System.Text.RegularExpressions.Regex.Replace(l, "\s+", " ")
			'l = "'" + l.Replace("'", "''") + "'"
			If Not filter_txt_file.Contains(l) Then filter_txt_file.Add(l)
		Next

		'TreeView1_AfterCheck(TreeView1, New System.Windows.Forms.TreeViewEventArgs(Nothing))
		Dim dt = DirectCast(ListBox1.DataSource, DataTable)
		Dim dt_target = dt.Clone
		For i As Integer = 0 To dt.Rows.Count - 1
			Dim name = dt.Rows(i).Item(1).ToString.Trim.ToUpper
			name = name.Replace("-", " ").Replace(":", " ").Replace("=", " ").Replace("!", " ").Replace(".", " ").Replace(",", " ")
			name = System.Text.RegularExpressions.Regex.Replace(name, "\s+", " ")
			If filter_txt_file.Contains(name) Then dt_target.ImportRow(dt.Rows(i))
		Next

		ListBox1.ValueMember = "id"
		ListBox1.DisplayMember = "name"
		ListBox1.DataSource = dt_target
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
	End Sub
	'View / Maintenance / Filter Entries With Screenshots From Current List
	Private Sub FilterEntriesWithScreenshotsFromCurrentListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FilterEntriesWithScreenshotsFromCurrentListToolStripMenuItem.Click
		Dim pr As New ProgressBar With {.Minimum = 0, .Value = 0}
		Me.Controls.Add(pr)
		pr.Top = CInt((Me.Height / 2) - (pr.Height / 2))
		pr.Left = 100
		pr.Width = Me.Width - 200
		pr.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
		pr.BringToFront()

		Dim dt As DataTable = DirectCast(ListBox1.DataSource, DataTable)
		pr.Maximum = dt.Rows.Count
		For r As Integer = dt.Rows.Count - 1 To 0 Step -1
			Dim name = dt.Rows(r).Item(1).ToString
			Dim cat = dt.Rows(r).Item(2).ToString
			'cat = cat

			Dim year = ""
			If type = catalog_type.games Then year = dt.Rows(r).Item(3).ToString

			If Not getScreen(name, cat, 0, year) = "" Then
				dt.Rows.RemoveAt(r)
			End If

			pr.Value += 1
			If pr.Value Mod 20 = 0 Then Application.DoEvents()
		Next

		Me.Controls.Remove(pr)
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
	End Sub
	'View / Maintenance / Show Duplicated
	Private Sub ShowDuplicatedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowDuplicatedToolStripMenuItem.Click
		Dim pr As New ProgressBar With {.Minimum = 0, .Value = 0}
		Me.Controls.Add(pr)
		pr.Top = CInt((Me.Height / 2) - (pr.Height / 2))
		pr.Left = 100
		pr.Width = Me.Width - 200
		pr.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
		pr.BringToFront()

		Dim dt As DataTable = DirectCast(ListBox1.DataSource, DataTable)
		pr.Maximum = dt.Rows.Count

		Dim names As New List(Of String)
		For r As Integer = 0 To dt.Rows.Count - 1
			names.Add(dt.Rows(r).Item(1).ToString.ToUpper().Replace("[A]", ""))
		Next

		Dim dupes_indexes As New List(Of Integer)
		For n As Integer = names.Count - 1 To 0 Step -1
			Dim cur_name = names(n)
			names.RemoveAt(n)
			If names.Contains(cur_name) Then
				names.Add(cur_name) : dupes_indexes.Add(n)
			End If

			pr.Value += 1
			If pr.Value Mod 20 = 0 Then Application.DoEvents()
		Next

		pr.Value = 0
		Dim dt2 As New DataTable()
		For Each c As DataColumn In dt.Columns
			dt2.Columns.Add(c.ColumnName, c.DataType)
		Next

		For n As Integer = dt.Rows.Count - 1 To 0 Step -1
			'If Not dupes_indexes.Contains(n) Then dt.Rows.RemoveAt(n)
			If dupes_indexes.Contains(n) Then dt2.Rows.Add(dt.Rows(n).ItemArray)

			pr.Value += 1
			If pr.Value Mod 20 = 0 Then Application.DoEvents()
		Next

		ListBox1.BeginUpdate()
		ListBox1.ValueMember = "id"
		ListBox1.DisplayMember = "name"
		ListBox1.DataSource = dt2
		ListBox1.EndUpdate()

		Me.Controls.Remove(pr)
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
	End Sub
	'View / Maintenance / Show entries with multiple paths
	Private Sub FilterEntriesWithSingleOrNoPathFromCurrentListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FilterEntriesWithSingleOrNoPathFromCurrentListToolStripMenuItem.Click
		Dim pr As New ProgressBar With {.Minimum = 0, .Value = 0}
		Me.Controls.Add(pr)
		pr.Top = CInt((Me.Height / 2) - (pr.Height / 2))
		pr.Left = 100
		pr.Width = Me.Width - 200
		pr.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
		pr.BringToFront()

		Dim dt As DataTable = DirectCast(ListBox1.DataSource, DataTable)
		pr.Maximum = dt.Rows.Count

		'Get main_ids of entries with multiple paths
		Dim sql = "SELECT main_id FROM paths GROUP BY main_id HAVING count(id) >= 2"
		Dim rdr = db.queryReader(sql)
		Dim main_ids As New List(Of String)
		While rdr.Read
			main_ids.Add(rdr.GetInt32(0).ToString())
		End While

		'Filter from current list
		Dim dt2 As New DataTable()
		For Each c As DataColumn In dt.Columns
			dt2.Columns.Add(c.ColumnName, c.DataType)
		Next
		For r As Integer = dt.Rows.Count - 1 To 0 Step -1
			If main_ids.Contains(dt.Rows(r).Item(0).ToString()) Then dt2.Rows.Add(dt.Rows(r).ItemArray)

			pr.Value += 1
			If pr.Value Mod 50 = 0 Then Application.DoEvents()
		Next
		ListBox1.BeginUpdate()
		ListBox1.ValueMember = "id"
		ListBox1.DisplayMember = "name"
		ListBox1.DataSource = dt2
		ListBox1.EndUpdate()

		Me.Controls.Remove(pr)
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
	End Sub

	'Database / Field Associations
	Private Sub FieldAssociationsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FieldAssociationsToolStripMenuItem.Click
		Form2_fieldAssociations.need_refresh_main_form = False
		Dim f As New Form2_fieldAssociations
		f.ShowDialog(Me)
		If Form2_fieldAssociations.need_refresh_main_form Then init_fieldset() : ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub
	'Database / Category Editor
	Private Sub CategoryEditorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CategoryEditorToolStripMenuItem.Click
		Dim f As New FormA_CategoryEditor
		f.ShowDialog(Me)
		init_categories()
	End Sub
	'Database / Custom List Manager
	Private Sub CustomListsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CustomListsToolStripMenuItem.Click
		Form3_customListMngr.need_refresh_main_form = False
		Dim f As New Form3_customListMngr
		f.ShowDialog(Me)
		If Form3_customListMngr.need_refresh_main_form Then init_customLists() : ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub
	'Database / Paths
	Private Sub PathsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PathsToolStripMenuItem.Click
		Dim f As New Form8_paths(Me)
		f.Show(Me)
	End Sub
	'Database / Convert STR column to Int, Dec or Date
	Private Sub ConvertStrColumbToIntDecOrDateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConvertStrColumbToIntDecOrDateToolStripMenuItem.Click
		Dim f As New FormD_ConvertColumn
		f.ShowDialog(Me)
	End Sub
	'Database / Raw database view
	Private Sub RawDatabaseViewToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RawDatabaseViewToolStripMenuItem.Click
		Dim f As New Form3_rawDatabaseView
		f.Show()
	End Sub
	'Database / Maintenance / Fix Screenshots
	Private Sub FixScreenshotsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FixScreenshotsToolStripMenuItem.Click
		Dim ff = New FormA_MaintenanceDB(screenshotPath)
		ff.Show(Me)
		'ff.ShowDialog(Me)
		Exit Sub

		If screenshotPath.Trim = "" Then MsgBox("Screenshot path not set.") : Exit Sub
		If Not My.Computer.FileSystem.DirectoryExists(screenshotPath) Then MsgBox("Screenshot path not exist.") : Exit Sub
		If type = catalog_type.daz Then FixScreenshotsRecur(screenshotPath)

		If type = catalog_type.games Then
			'Convert 'The Screen' to 'Screen, The'
			If screenshotPath.Trim = "" Then MsgBox("Screenshot path not set.") : Exit Sub
			If Not My.Computer.FileSystem.DirectoryExists(screenshotPath) Then MsgBox("Screenshot path not exist.") : Exit Sub
			Dim letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : letters += letters.ToLower
			For Each d In My.Computer.FileSystem.GetDirectories(screenshotPath, FileIO.SearchOption.SearchAllSubDirectories, {"T"})
				For Each f In My.Computer.FileSystem.GetFiles(d, FileIO.SearchOption.SearchTopLevelOnly, {"The *.*"})
					Dim fName = System.IO.Path.GetFileNameWithoutExtension(f)
					Dim ext = System.IO.Path.GetExtension(f)

					'Dim newName = fName.Substring(4) + ", The" + ext
					Dim ind = fName.LastIndexOf("_")
					Dim newName = fName.Insert(ind, ", The").Substring(4) + ext

					Dim firstLetter = newName.Substring(0, 1)
					If Not letters.Contains(firstLetter) Then firstLetter = "#"
					Dim newPath = System.IO.Path.GetDirectoryName(f) + "\..\" + firstLetter + "\" + newName

					If Not My.Computer.FileSystem.FileExists(newPath) Then
						My.Computer.FileSystem.MoveFile(f, newPath)
					Else
						'Log existed files
						FileOpen(1, "Couldn't Rename.log", OpenMode.Append)
						PrintLine(1, f.Substring(f.LastIndexOf("\") + 1) + " -> " + newName)
						FileClose(1)
					End If
				Next
			Next
		End If
		If type = catalog_type.daz Then
			Dim sql = "SELECT DISTINCT main.id, name, group_concat(category.cat) FROM main "
			sql += "JOIN category ON main.id = category.id_main "
			sql += "WHERE name like '%?%' "
			sql += "GROUP BY  main.id, name "
			Dim dt = db.queryDataset(sql)
			For Each r In dt.Rows
				If getScreen(r(1).ToString, r(2).ToString, 0) = "" Then
					'Check '?' in names, because grabber replace ? by empty and now it should be replaced by '-'
					If getScreen(r(1).ToString.Replace("?", ""), r(2).ToString, 0) <> "" Then
						Dim i As Integer = 0
						Do While getScreen(r(1).ToString.Replace("?", ""), r(2).ToString, i) <> ""
							Dim oldFile = getScreen(r(1).ToString.Replace("?", ""), r(2).ToString, i)
							Dim fld = oldFile.Substring(0, oldFile.LastIndexOf("\"))
							Dim ext = oldFile.Substring(oldFile.LastIndexOf("."))

							Dim num As String = ""
							If type = catalog_type.daz And i > 0 Then num = (i - 1).ToString

							Dim normalized_name = r(1).ToString
							normalized_name = normalized_name.Replace("""", "'").Replace("*", "-").Replace("/", "-").Replace("\", "-").Replace("?", "–").Replace(":", " - ").Replace("|", "")
							normalized_name = normalized_name.Replace("  ", " ").Replace("  ", " ").TrimStart
							Dim newFile = fld + "\" + normalized_name + num + ext

							My.Computer.FileSystem.MoveFile(oldFile, newFile)

							i += 1
						Loop
					End If
					'Check '?' in names, because grabber (old version) replace ? by � and now it should be replaced by '-'
					If getScreen(r(1).ToString.Replace("?", "�"), r(2).ToString, 0) <> "" Then
						Dim i As Integer = 0
						Do While getScreen(r(1).ToString.Replace("?", "�"), r(2).ToString, i) <> ""
							Dim oldFile = getScreen(r(1).ToString.Replace("?", "�"), r(2).ToString, i)
							Dim fld = oldFile.Substring(0, oldFile.LastIndexOf("\"))
							Dim ext = oldFile.Substring(oldFile.LastIndexOf("."))

							Dim num As String = ""
							If type = catalog_type.daz And i > 0 Then num = (i - 1).ToString

							Dim normalized_name = r(1).ToString
							normalized_name = normalized_name.Replace("""", "'").Replace("*", "-").Replace("/", "-").Replace("\", "-").Replace("?", "–").Replace(":", " - ").Replace("|", "")
							normalized_name = normalized_name.Replace("  ", " ").Replace("  ", " ").TrimStart
							Dim newFile = fld + "\" + normalized_name + num + ext

							My.Computer.FileSystem.MoveFile(oldFile, newFile)

							i += 1
						Loop
					End If
				End If
			Next
		End If

		MsgBox("Done. See 'Couldn't xxx.log' for details.")
	End Sub
	Private Sub FixScreenshotsRecur(path As String)
		Dim letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : letters += letters.ToLower

		Dim files = My.Computer.FileSystem.GetFiles(path).ToList
		For Each f In files
			Dim fld = f.Substring(0, f.LastIndexOf("\"))
			Dim fname = f.Substring(f.LastIndexOf("\") + 1)

			If fname.ToUpper.Contains("&AMP;") Then
				fname = System.Text.RegularExpressions.Regex.Replace(fname, "&amp;", "&", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

				Dim newName As String = fld + "\" + fname
				Dim newNameWoExt = fname.Substring(0, fname.LastIndexOf("."))
				If My.Computer.FileSystem.GetFiles(fld, FileIO.SearchOption.SearchTopLevelOnly, {newNameWoExt + "*.*"}).Count > 0 Then
					'Log existed files
					FileOpen(1, "Couldn't Rename.log", OpenMode.Append)
					PrintLine(1, f.Substring(f.LastIndexOf("\") + 1) + " -> " + newName)
					FileClose(1)
				Else
					My.Computer.FileSystem.MoveFile(f, newName)
				End If
			End If

			'Move letters based screenshots out of the # folder, to appropriate folders
			If fld.EndsWith("#") And letters.Contains(fname.Substring(0, 1)) Then
				fld = fld.Substring(0, fld.Length - 1) + fname.Substring(0, 1).ToUpper

				Dim newName As String = fld + "\" + fname
				Dim newNameWoExt = fname.Substring(0, fname.LastIndexOf("."))
				If My.Computer.FileSystem.GetFiles(fld, FileIO.SearchOption.SearchTopLevelOnly, {newNameWoExt + "*.*"}).Count > 0 Then
					'Log existed files
					FileOpen(1, "Couldn't Move.log", OpenMode.Append)
					PrintLine(1, f.Substring(f.LastIndexOf("\") + 1) + " -> " + newName)
					FileClose(1)
				Else
					My.Computer.FileSystem.MoveFile(f, newName)
				End If
			End If
		Next

		Dim folders = My.Computer.FileSystem.GetDirectories(path)
		For Each f In folders
			FixScreenshotsRecur(f)
		Next
	End Sub
	'Database / Maintenance / Fix Boolean Fields
	Private Sub FixBooleanFieldsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FixBooleanFieldsToolStripMenuItem.Click
		For Each r As DataRow In db.queryDataset("PRAGMA table_info(main);").Rows
			Dim field = r(1).ToString()
			If field.ToUpper().StartsWith("DATA_BOOL") Then
				db.execute("UPDATE main SET " + field + " = 'true' WHERE " + field + " = 1")
				db.execute("UPDATE main SET " + field + " = 'true' WHERE " + field + " = 'True' COLLATE NOCASE")
				db.execute("UPDATE main SET " + field + " = 'false' WHERE " + field + " = 0")
				db.execute("UPDATE main SET " + field + " = 'false' WHERE " + field + " is null")
				db.execute("UPDATE main SET " + field + " = 'false' WHERE " + field + " = 'False' COLLATE NOCASE")
			End If
		Next
	End Sub
	'Database / Maintenance / Delete All Displayed Products From Database
	Private Sub DeleteAllDisplayedProductsFromDatabaseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteAllDisplayedProductsFromDatabaseToolStripMenuItem.Click
		Dim msg As String = "All products currently displayed in the list will be permanently deleted from DB." + vbCrLf
		msg += "All related information (such as files, custom lists e.t.c will also be deleted." + vbCrLf
		msg += "Are you sure?"
		Dim r = MsgBox(msg, MsgBoxStyle.YesNo)
		If r = MsgBoxResult.Yes Then
			db.execute("BEGIN;")
			Dim counter As Integer = 0
			Dim list As String = ""
			Label_zero.Text = "Preparing query ..."
			Label_zero.Refresh() : Application.DoEvents()
			For Each l As DataRowView In ListBox1.Items
				list += l(0).ToString + ","
			Next
			list = list.Substring(0, list.Length - 1)
			Label_zero.Text = "Processing tables 1/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM main WHERE id IN (" + list + ")")
			Label_zero.Text = "Processing tables 2/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM category WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 3/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM custom_lists_data WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 4/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM paths WHERE main_id IN (" + list + ")")
			Label_zero.Text = "Processing tables 5/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM product_files WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 6/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM product_files_found WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 7/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM product_files_found_details WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 8/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM product_files_found_doubles WHERE id_main IN (" + list + ")")
			Label_zero.Text = "Processing tables 9/9"
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("DELETE FROM status_data WHERE main_id IN (" + list + ")")

			Label_zero.Text = "Commit transaction..."
			Label_zero.Refresh() : Application.DoEvents()
			db.execute("COMMIT;")

			Label_zero.Text = "Compacting database..."
			db.close()
			db = Nothing
			db = New Class01_db
			db.connect()
			db.execute("VACUUM")
			Label_zero.Text = "Done"
			MsgBox("Done. Database will be fully refreshed after restart.")
		End If
	End Sub
	'Database / Maintenance / Trim Product Names
	Private Sub TrimProductNamesfixProductsStartingOrEndingWithSpacesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TrimProductNamesfixProductsStartingOrEndingWithSpacesToolStripMenuItem.Click
		Progress_Bar.Value = 0
		Me.Invoke(Sub() Progress_Label.Text = "Preparing...")
		Progress_GroupBox.Location = New Point((Me.Width / 2) - Progress_GroupBox.Width / 2, (Me.Height / 2) - Progress_GroupBox.Height / 2)
		Progress_GroupBox.Visible = True

		'Task.Run
		Task.Factory.StartNew(Sub()
								  log("-----------------------------------------------------------", "Name Trimming", LOG_FILE_PATH_FILES)
								  log("Database Name Trimming Started", "Name Trimming", LOG_FILE_PATH_FILES)

								  Dim progress = 0
								  Dim trimmed = 0
								  Dim failures As New List(Of String)
								  Dim sql = "SELECT main.id, main.name, group_concat(category.cat)"
								  If type = catalog_type.games Then sql += ", data_num1"
								  sql += " FROM main JOIN category ON main.id = category.id_main WHERE main.name LIKE ' %' OR main.name LIKE '% '"
								  sql += " GROUP BY main.id, main.name"
								  Dim tbl = db.queryDataset(sql)
								  Me.Invoke(Sub() Progress_Bar.Maximum = tbl.Rows.Count)
								  db.execute("BEGIN;")
								  For Each r As DataRow In tbl.Rows
									  progress += 1
									  If progress Mod 5 = 0 Then
										  Me.Invoke(Sub() Progress_Bar.Value = progress)
										  Me.Invoke(Sub() Progress_Label.Text = "Triming names and fixing screenshots: " + progress.ToString + " / " + tbl.Rows.Count.ToString)
									  End If

									  Dim i As Integer = 0
									  Dim screenshots As New List(Of String)
									  Dim name_old = r.Field(Of String)(1)
									  Dim name_new = name_old.Trim()

									  Dim year = ""
									  If type = catalog_type.games Then year = r.Field(Of Integer)(3).ToString()

									  'Get screenshot
									  While True
										  Dim scr = getScreen(name_old, r.Field(Of String)(2), i, year)
										  If scr = "" Then Exit While
										  i += 1 : screenshots.Add(scr)
									  End While

									  'Check if we will not overwrite another screenshots
									  Dim fail = False
									  Dim same = 0
									  Dim screenshots_new As New List(Of String)
									  For i = 0 To screenshots.Count - 1
										  Dim scr = getScreen(name_new, r.Field(Of String)(2), i, year, False, True)
										  Dim scr_path = IO.Path.GetDirectoryName(scr)
										  Dim file_mask = IO.Path.GetFileName(scr) + ".*"
										  Dim scr_exist = IO.Directory.GetFiles(scr_path, file_mask)
										  If scr_exist.Count > 0 Then
											  'Check if this screen is exactly the same as old one
											  fail = True
											  If scr_exist.Count <> 1 Then Exit For

											  Dim size1 = New IO.FileInfo(screenshots(i)).Length
											  Dim size2 = New IO.FileInfo(scr_exist(0)).Length
											  If size1 <> size2 Then Exit For

											  Dim crc1 = GetCRC32(screenshots(i))
											  Dim crc2 = GetCRC32(scr_exist(0))
											  If crc1 <> crc2 Then Exit For

											  same += 1
										  End If
										  screenshots_new.Add(scr + IO.Path.GetExtension(screenshots(i)))
									  Next
									  If fail Then
										  If same = screenshots.Count Then
											  log("Screenshots already exists for '" + name_old + "', but they are exactly the same as for old one. DELETE old screenshots, as they are not used anymore.", "Name Trimming", LOG_FILE_PATH_FILES)
										  Else
											  failures.Add(r.Field(Of Int64)(0).ToString())
											  log("Could not trim '" + name_old + "' because at least one of trimmed screenshots already exists in destination folder.", "Name Trimming", LOG_FILE_PATH_FILES)
											  Continue For
										  End If
									  End If

									  'Update DB
									  db.execute("UPDATE main SET name = """ + name_new.Replace("'", "''") + """ WHERE id = " + r.Field(Of Int64)(0).ToString() + ";")

									  'Rename screenshots
									  If Not fail Then
										  For i = 0 To screenshots.Count - 1
											  log("Renaming: """ + screenshots(i) + """ to """ + screenshots_new(i) + """", "Name Trimming", LOG_FILE_PATH_FILES)
											  IO.File.Move(screenshots(i), screenshots_new(i))
										  Next
									  End If

									  trimmed += 1
									  log("Successfully trimmed: """ + name_old + """ to """ + name_new + """", "Name Trimming", LOG_FILE_PATH_FILES)
								  Next

								  Me.Invoke(Sub() Progress_Bar.Value = Progress_Bar.Maximum)
								  Me.Invoke(Sub() Progress_Label.Text = "Commit changes...")
								  db.execute("COMMIT;")

								  Dim msg = tbl.Rows.Count.ToString + " product names was requested to trim. " + vbCrLf
								  msg += trimmed.ToString() + " was successfully trimmed. " + vbCrLf
								  msg += failures.Count.ToString() + " was failed (see Catalog 2016.logFileOps.log for details). "
								  log("Database Name Trimming Ended. " + msg.Replace(vbCrLf, ""), "Name Trimming", LOG_FILE_PATH_FILES)

								  Me.Invoke(Sub() Progress_GroupBox.Visible = False)
								  Me.Invoke(Sub()
												If failures.Count = 0 Then
													MsgBox(msg)
												Else
													msg += vbCrLf + vbCrLf
													msg += "Show failed products?"
													Dim res = MsgBox(msg, MsgBoxStyle.YesNo)
													If res = MsgBoxResult.Yes Then
														Dim f As New FormF_FailedTrimmedEntries(failures.ToArray())
														f.Show(Me)
													End If
												End If
											End Sub)
							  End Sub)
	End Sub
	'Database / Maintenance / Check Paths
	Private Sub CheckPathsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckPathsToolStripMenuItem.Click
		Dim f As New Form8_pathsCheck
		f.Show()
	End Sub
	'Database / Maintenance / Compact database (VACUUM)
	Private Sub CompactDatabaseVACUUMToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CompactDatabaseVACUUMToolStripMenuItem.Click
		Dim length_before = New IO.FileInfo(path_db).Length
		db.close() : db.connect()
		db.execute("VACUUM")
		Dim length_after = New IO.FileInfo(path_db).Length
		Dim length_diff = length_before - length_after

		Dim msg = "Done." + vbCrLf
		msg += "Database size before VACUUM:   " + Math.Round(length_before / 1024).ToString("### ### ### ##0").Trim + " kb" + vbCrLf
		msg += "Database size after VACUUM:   " + Math.Round(length_after / 1024).ToString("### ### ### ##0").Trim + " kb" + vbCrLf
		msg += "Size gain:   " + Math.Round(length_diff / 1024).ToString("### ### ### ##0").Trim + " kb"
		MsgBox(msg)
	End Sub
	'Database / Maintenance / Create or Delete DB index for 'name' field
	Private Sub CreateIndexFornameFieldToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateIndexFornameFieldToolStripMenuItem.Click, DeleteIndexFornameFieldToolStripMenuItem.Click
		Try
			If sender Is CreateIndexFornameFieldToolStripMenuItem Then
				'create
				db.execute("CREATE INDEX IF NOT EXISTS name_ind ON main(name)")
			ElseIf sender Is DeleteIndexFornameFieldToolStripMenuItem Then
				'delete
				db.close() : db.connect()
				db.execute("DROP INDEX IF EXISTS name_ind")
			End If
		Catch ex As Exception
			MsgBox(ex.Message)
		Finally
			init_index_menu()
		End Try
	End Sub
	'Database / Maintenance / Create or Delete DB index for category 'cat' field
	Private Sub CreateIndexForCategorycatFieldToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateIndexForCategorycatFieldToolStripMenuItem.Click, DeleteIndexForCategorycatFieldToolStripMenuItem.Click
		Try
			If sender Is CreateIndexForCategorycatFieldToolStripMenuItem Then
				'create
				db.execute("CREATE INDEX IF NOT EXISTS category_cat_ind ON category(cat)")
			ElseIf sender Is DeleteIndexForCategorycatFieldToolStripMenuItem Then
				'delete
				db.close() : db.connect()
				db.execute("DROP INDEX IF EXISTS category_cat_ind")
			End If
		Catch ex As Exception
			MsgBox(ex.Message)
		Finally
			init_index_menu()
		End Try
	End Sub

	'Options / Status table settings
	Private Sub StatusTableSettingsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StatusTableSettingsToolStripMenuItem.Click
		Dim f As New Form5_statusTableSettings
		f.ShowDialog(Me)
	End Sub
	'Options / Live Search Mode
	Private Sub StartWithToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StartWithToolStripMenuItem.Click, ContainToolStripMenuItem.Click
		StartWithToolStripMenuItem.Checked = False
		ContainToolStripMenuItem.Checked = False
		DirectCast(sender, ToolStripMenuItem).Checked = True

		If ContainToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "LiveSearchMode", "Contains")
		ElseIf StartWithToolStripMenuItem.Checked Then
			ini.IniWriteValue("Interface", "LiveSearchMode", "StartWith")
		End If

		TextBox1_TextChanged(sender, New EventArgs)
	End Sub
	'Options / Value Groupping
	Private Sub ConfigValuesGrouppingToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConfigValuesGrouppingToolStripMenuItem.Click
		Form7_OptionsGroupping.need_refresh_main_form = False
		Dim f As New Form7_OptionsGroupping
		f.ShowDialog(Me)
		If Form7_OptionsGroupping.need_refresh_main_form Then init_fieldset() : ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub
	'Options / Configuration
	Private Sub ConfigurationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConfigurationToolStripMenuItem.Click
		Form7_Options.need_refresh_main_form = False
		Dim f As New Form7_Options(Me)
		f.ShowDialog(Me)
		If Form7_Options.need_refresh_main_form Then
			init_options() : init_fieldset() : ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
		End If
	End Sub
	'Options / Exodos Dosbox Options
	Private Sub ExodosDosboxOptionsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExodosDosboxOptionsToolStripMenuItem.Click
		Dim f As New Games_ExodosDosboxConfig
		f.ShowDialog(Me)
	End Sub

	'Categories - Expand all
	Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
		TreeView1.ExpandAll()
	End Sub
	'Categories - Collapse all
	Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
		TreeView1.CollapseAll()
	End Sub

	'Edit DB - Add new entry
	Private Sub Button_AddNew_Click(sender As Object, e As EventArgs) Handles Button_AddNew.Click
		Dim cat = "Default Category"
		Dim cur_cat_arr = TextBox2.Text.Trim.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
		If cur_cat_arr.Count = 1 Then
			cur_cat_arr(0) = cur_cat_arr(0).Trim.Trim({"'"c}).Trim
			If Not cur_cat_arr(0).ToLower = "[all]" And Not cur_cat_arr(0).ToLower = "[none]" Then cat = cur_cat_arr(0)
		End If

		'If it's the very first entry, and 'Default Category' is not yet added to the treeview - add it
		If cat = "Default Category" Then
			If Not TreeView1.Nodes().ContainsKey("Default Category") Then TreeView1.Nodes().Insert(0, "Default Category", "Default Category")
			If Not TreeView2.Nodes().ContainsKey("Default Category") Then TreeView2.Nodes().Insert(0, "Default Category", "Default Category")
			Dim cur_cat_node = TreeView1.Nodes.Find("Default Category", False)(0)
			If Not cur_cat_node.Checked Then cur_cat_node.Checked = True
		End If

		db.execute("INSERT INTO main (name) VALUES ('New Entry')")
		Dim id = db.getLastRowID()
		db.execute("INSERT INTO category (id_main, cat) VALUES (" + id.ToString + ", '" + cat + "')")

		Dim dt = DirectCast(ListBox1.DataSource, DataTable)
		dt.Rows.Add({id, "New Entry", cat})

		ListBox1.SelectedIndex = ListBox1.Items.Count - 1
		Label_zero.Text = "Total: " + ListBox1.Items.Count.ToString
	End Sub
	'Edit DB - Delete entry
	Private Sub Button_Delete_Click(sender As Object, e As EventArgs) Handles Button_Delete.Click
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim row = DirectCast(ListBox1.SelectedItem, DataRowView)
		If delete_confirmation Then
			Dim res = MsgBox("Are you sure, you want to delete """ + row.Item(1).ToString() + """? (id: " + row.Item(0).ToString() + ")", MsgBoxStyle.YesNo)
			If res = MsgBoxResult.No Then Exit Sub
		End If

		Dim t = db.queryDataset("SELECT name FROM sqlite_master")
		Dim available_tables As New List(Of String)
		For Each r As DataRow In t.Rows
			available_tables.Add(r(0).ToString().ToLower())
		Next

		Dim id = CInt(ListBox1.SelectedValue)
		db.execute("DELETE FROM main WHERE id = " + id.ToString + ";")
		db.execute("DELETE FROM category WHERE id_main = " + id.ToString + ";")
		db.execute("DELETE FROM custom_lists_data WHERE id_main = " + id.ToString + ";")
		If available_tables.Contains("files") Then db.execute("DELETE FROM files WHERE main_id = " + id.ToString + ";")
		If available_tables.Contains("paths") Then db.execute("DELETE FROM paths WHERE main_id = " + id.ToString + ";")
		If available_tables.Contains("status_data") Then db.execute("DELETE FROM status_data WHERE main_id = " + id.ToString + ";")
		If available_tables.Contains("product_files") Then db.execute("DELETE FROM product_files WHERE id_main = " + id.ToString + ";")
		If available_tables.Contains("product_files_found") Then db.execute("DELETE FROM product_files_found WHERE id_main = " + id.ToString + ";")
		If available_tables.Contains("product_files_found_details") Then db.execute("DELETE FROM product_files_found_details WHERE id_main = " + id.ToString + ";")
		If available_tables.Contains("product_files_found_doubles") Then db.execute("DELETE FROM product_files_found_doubles WHERE id_main = " + id.ToString + ";")

		Dim old_selected = ListBox1.SelectedIndex
		Dim old_selected_top = ListBox1.TopIndex
		'Dim ds = DirectCast(ListBox1.DataSource, DataTable):ds.Rows.RemoveAt(old_selected)
		TreeView1_AfterCheck(TreeView1, New TreeViewEventArgs(Nothing))
		If old_selected < ListBox1.Items.Count Then ListBox1.SelectedIndex = old_selected Else ListBox1.SelectedIndex = ListBox1.Items.Count - 1
		ListBox1.TopIndex = old_selected_top
	End Sub
	'Edit DB - Show category edit tree
	Private Sub Show_Category_Edit_Tree()
		TreeView2.Location = Me.PointToClient(MousePosition)
		TreeView2.Visible = True
		TreeView2.BringToFront()
		TreeView2.CollapseAll()

		refreshing = True
		Dim cur_cats = lblcat.Text.Trim.Replace("/", "\").Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries).Select(Of String)(Function(s) s.Replace("&&", "&"))
		For Each node In TreeView2.Nodes.GetAllNodesRecur
			If cur_cats.Contains(node.FullPath) Then
				node.Checked = True

				Dim parent = node.Parent
				While parent IsNot Nothing
					parent.Expand()
					parent = parent.Parent
				End While
			Else
				node.Checked = False
			End If
		Next
		refreshing = False

		Application.AddMessageFilter(Me)
	End Sub
	'Edit DB - Update entry category
	Private Sub Update_Entry_Category()
		If refreshing Then Exit Sub
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim new_cat_lst As New List(Of String)
		Dim id = ListBox1.SelectedValue.ToString

		db.execute("DELETE FROM category WHERE id_main = " + id)
		For Each node In TreeView2.Nodes.GetAllNodesRecur
			If node.Checked Then
				new_cat_lst.Add(node.FullPath.Replace("\", "/"))
				db.execute("INSERT INTO category (id_main, cat) VALUES (" + id + ", '" + node.FullPath.Replace("\", "/") + "')")
			End If
		Next

		Dim new_cat = String.Join(",", new_cat_lst)
		CType(ListBox1.SelectedItem, DataRowView).Row.Item(2) = new_cat
		lblcat.Text = new_cat.Replace(",", vbCrLf).Replace("&", "&&")
	End Sub
	'Edit DB - Show category tree context menu (add/remove categories)
	Private Sub Edit_Category_Context_Menu(o As Object, e As MouseEventArgs)
		If e.Button = MouseButtons.Right Then
			TreeView2.Focus()
			Dim node = TreeView2.GetNodeAt(TreeView2.PointToClient(MousePosition))
			Dim mu As New List(Of MenuItem)
			If node Is Nothing Then
				mu.Add(New MenuItem("Add new category", New EventHandler(Sub() Edit_Category_Add_New_or_Remove(Nothing))))
			Else
				mu.Add(New MenuItem("Add new root category", New EventHandler(Sub() Edit_Category_Add_New_or_Remove(Nothing))))
				mu.Add(New MenuItem("Add new sub category to " + node.Text, New EventHandler(Sub() Edit_Category_Add_New_or_Remove(node))))
				mu.Add(New MenuItem("Rename category " + node.Text, New EventHandler(Sub() Edit_Category_Rename(node))))
				mu.Add(New MenuItem("Remove category " + node.Text, New EventHandler(Sub() Edit_Category_Add_New_or_Remove(node, True))))
				TreeView2.SelectedNode = node
			End If

			Dim cm As New ContextMenu(mu.ToArray)
			cm.Show(TreeView2, TreeView2.PointToClient(MousePosition))
		End If
	End Sub
	Private Sub Edit_Category_Add_New_or_Remove(parent As TreeNode, Optional remove As Boolean = False)
		If Not remove Then
			'add
			Dim name = InputBox("Category name?", "Category name", "New Category").Trim
			If name = "" Then Exit Sub
			If parent Is Nothing Then
				'add to root
				TreeView2.Nodes.Add(name, name)
				TreeView1.Nodes.Add(name, name + " (0)(0)")
			Else
				'add to parent
				parent.Nodes.Add(name, name)
				Dim p2 = TreeView1.Nodes.GetAllNodesRecur.Where(Function(n) n.FullPath.SubstringTo("(").Trim = parent.FullPath).FirstOrDefault
				If p2 IsNot Nothing Then p2.Nodes.Add(name, name + " (0)(0)")
			End If
		Else
			'remove
			'TODO
		End If
	End Sub
	Private Sub Edit_Category_Rename(node As TreeNode)
		Dim new_name = InputBox("New name?", "Rename Category", node.Text)
		If new_name = "" Then Exit Sub

		'Dim cat = node.FullPath.Replace("\", "/")
		'Dim new_cat = IO.Path.GetDirectoryName(node.FullPath) + "\" + new_name
		'new_cat = new_cat.Replace("\", "/")
		'If new_cat.StartsWith("/") Then new_cat = new_cat.Substring(1)

		'Dim r = db.queryReader("SELECT id, cat from category WHERE cat LIKE '" + cat + "%'")
		'If Not r.HasRows Then Exit Sub

		'db.execute("BEGIN;")
		'While r.Read
		'	Dim t = r("cat").ToString()
		'	If t.ToUpper <> cat.ToUpper AndAlso Not t.ToUpper.Contains(cat.ToUpper + "/") Then Continue While

		'	t = Microsoft.VisualBasic.Strings.Replace(t, cat, new_cat, 1, 1, CompareMethod.Text)

		'	Dim sql = "UPDATE category SET cat = '" + t + "' WHERE id = " + r("id").ToString() + ";"
		'	db.execute(sql)
		'End While
		'db.execute("COMMIT;")

		Edit_Category_Rename(node, new_name, False)

		init_categories()
	End Sub
	Public Function Edit_Category_Rename(node As TreeNode, new_name As String, New_Name_Is_Path As Boolean) As (rows As Integer, rows_sub As Integer, path_orig As String, path_new As String, err As String)
		Dim path_orig = getNodeCategoryPath(node).Trim
		Dim path_new = ""
		If New_Name_Is_Path Then
			'Full path rename
			path_new = new_name.Trim().Trim({"\"c, "/"c}).Replace("\", "/")
		Else
			'Sub category rename
			If new_name.Contains("\") Or new_name.Contains("/") Then Return (0, 0, "", "", "Categorie name should not contain slashes.")
			If path_orig.Contains("/") Then
				'Is sub category
				path_new = IO.Path.GetDirectoryName(path_orig).Replace("\", "/") + "/" + new_name.Trim
			Else
				'Is root category
				path_new = new_name.Trim
			End If
		End If

		Dim sql = "UPDATE category SET cat = '" + path_new + "' WHERE cat = '" + path_orig + "' COLLATE NOCASE"
		db.execute(sql)
		Dim rows = db.affectedRows
		sql = "UPDATE category SET cat = REPLACE(cat, '" + path_orig + "/', '" + path_new + "/') WHERE cat LIKE '" + path_orig + "/%'"
		db.execute(sql)
		Dim rows_sub = db.affectedRows

		Return (rows, rows_sub, path_orig, path_new, "")
	End Function
	'Edit DB - Edit description
	Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged, RichTextBox2.TextChanged, RichTextBox3.TextChanged
		If refreshing Then Exit Sub
		If bg_descr_loader_threads.Count > 0 Then Exit Sub
		If ListBox1.SelectedIndex < 0 Then Exit Sub

		Dim DB_fieldName = ""
		Dim rtf = DirectCast(sender, RichTextBox)
		If rtf Is RichTextBox1 Then DB_fieldName = "data_txt1"
		If rtf Is RichTextBox2 Then DB_fieldName = "data_txt2"
		If rtf Is RichTextBox3 Then DB_fieldName = "data_txt3"

		Dim txt = ""
		If rtf.Text.Trim <> "" Then txt = rtf.Rtf

		Dim sql = "UPDATE main SET " + DB_fieldName + " = '" + txt.Replace("'", "''") + "' "
		sql += "WHERE id = " + ListBox1.SelectedValue.ToString
		db.execute(sql)
	End Sub

	'RichTextBox Descriptions - link click
	Private Sub RichTextBox_LinkClick(o As Object, e As LinkClickedEventArgs) Handles RichTextBox1.LinkClicked, RichTextBox2.LinkClicked, RichTextBox3.LinkClicked
		Dim link = e.LinkText
		If Not String.IsNullOrWhiteSpace(descriptionsBaseUrl) Then
			If Not e.LinkText.Contains("://") Then link = descriptionsBaseUrl + link
		End If

		Try
			Process.Start(link)
		Catch ex As Exception
			MsgBox("Can not open link: " + vbCrLf + link)
		End Try
	End Sub

	'Drag'n'drop
	Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles Me.DragEnter
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
		Else
			e.Effect = DragDropEffects.None
		End If
	End Sub
	Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles Me.DragDrop
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			If ListBox1.SelectedIndex < 0 Then MsgBox("No db entry selected. Aborting.") : Exit Sub

			Dim img = {".JPG", ".PNG", ".GIF"}
			Dim rtf = {".RTF", ".ZAM"}
			Dim files = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
			Dim warningShown As Boolean = False
			For Each f In files
				Dim ext = IO.Path.GetExtension(f).ToUpper
				If img.Contains(ext) Then
					If screenshotPath.Trim = "" Then MsgBox("Screenshot path is not valid. Aborting.") : Exit Sub

					'Adding screens
					Dim sql = "SELECT * FROM main WHERE id = " + ListBox1.SelectedValue.ToString
					Dim r = db.queryReader(sql)
					If Not r.HasRows Then Exit Sub
					r.Read()

					Dim cat = CType(ListBox1.SelectedItem, DataRowView).Row.Item(2).ToString.Replace(",", vbCrLf).Replace("&", "&&")
					Dim year = ""
					Dim rowView = CType(ListBox1.SelectedItem, DataRowView)
					If rowView.Row.ItemArray.Count = 4 Then year = CType(ListBox1.SelectedItem, DataRowView).Row.Item(3).ToString

					Dim i = 0
					While True
						If getScreen(r.Item("name").ToString, cat, i, year) = "" Then Exit While
						i += 1
					End While

					Dim scr_path_no_ext = getScreen(r.Item("name").ToString, cat, i, year, False, True)
					Dim fld = IO.Path.GetDirectoryName(scr_path_no_ext)
					If Not IO.Directory.Exists(fld) Then
						If MsgBox("Directory " + fld + " does not exist. Try to create?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
						IO.Directory.CreateDirectory(fld)
					End If

					IO.File.Copy(f, scr_path_no_ext + ext.ToLower)
				ElseIf rtf.Contains(ext) Then

				ElseIf Not warningShown Then
					Dim msg = "Only following files are allowed:" + vbCrLf + vbCrLf
					msg += ".jpg, .jpeg, .png, .gif - will be added as screenshot" + vbCrLf
					msg += ".rtf, .zam - will be added as description" + vbCrLf
					MsgBox(msg)
					warningShown = True
				End If
			Next
		End If

		ListBox1_SelectedIndexChanged(ListBox1, New EventArgs)
	End Sub

	'One Time Sub - rename all titles starting with THE to '_name_, THE'
	Private Sub normilizeThe()
		Dim sql = "SELECT id, name FROM main"
		Dim r = db.queryReader(sql)
		Do While r.Read
			Dim name = r.GetString(1)
			If name.ToUpper.StartsWith("THE ") Then
				name = name.Substring(4) + ", The"
				db.execute("UPDATE main SET name = '" + name.Replace("'", "''") + "' WHERE id = " + r.GetInt32(0).ToString)
			End If
		Loop
	End Sub
	'One time sub - move renderosity categories under Renderosity
	Sub move_Categories()
		Dim r = db.queryReader("Select id, id_main, cat FROM category")
		db.execute("BEGIN;")
		Do While r.Read
			Dim cat = r.GetString(2)
			If cat.ToUpper.StartsWith("2D") Or cat.ToUpper.StartsWith("3D") Or cat.ToUpper.StartsWith("EXTENDED") Or cat.ToUpper.StartsWith("GOING") Or cat.ToUpper.StartsWith("MARCH") Or cat.ToUpper.StartsWith("MERCHANT") Or cat.ToUpper.StartsWith("MUSIC") Or cat.ToUpper.StartsWith("OFF") Or cat.ToUpper.StartsWith("PRIME") Or cat.ToUpper.StartsWith("TUTORIALS") Then
				cat = "Renderosity/" + cat
				db.execute("UPDATE category SET cat = '" + cat.Replace("'", "''") + "' WHERE id = " + r.GetInt32(0).ToString)
			End If
		Loop
		db.execute("COMMIT;")
	End Sub
	'One time sub - fill renderosity product codes from links to data_num1
	Sub fill_RO_Codes()
		Dim lowestRO As Integer = 1000000
		Dim r = db.queryReader("SELECT id, data_str7, data_str8 FROM main")
		db.execute("BEGIN;")
		Do While r.Read
			If Not r.IsDBNull(1) And r.IsDBNull(2) Then
				Dim link = r.GetString(1)
				If link.ToUpper.StartsWith("https://www.renderosity.com".ToUpper) Then
					If link.EndsWith("/") Then link = link.Substring(0, link.Length - 1)
					Dim RO = link.Substring(link.LastIndexOf("/") + 1).Trim
					If CInt(RO) < lowestRO Then lowestRO = CInt(RO)
					RO = "RO-" + RO
					db.execute("UPDATE main SET data_str8 = '" + RO + "' WHERE id = " + r.GetInt32(0).ToString)
				End If
			End If
		Loop
		db.execute("COMMIT;")
		MsgBox("Done. LowestRO = " + lowestRO.ToString)
	End Sub
	'OLD CODE
	Private Sub OLD_Import_bgWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs)
		Dim files = DirectCast(e.Argument, String())
		Array.Sort(files)
		For Each file In files
			If file.ToUpper.EndsWith("_INFO.TXT") Then
				Dim tmp = file
				Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp)
				Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
				Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

				db.execute("BEGIN;")
				For Each line In lines
					Dim f As String = file.Substring(file.LastIndexOf("\") + 1)
					Dim cat As String = f.Substring(0, f.IndexOf(".txt")).Replace(" - ", ".")
					Dim cat_arr = cat.Split({"."c}, StringSplitOptions.RemoveEmptyEntries)
					cat = Join(cat_arr, "/")

					Dim str = line.Split({"%%%"}, System.StringSplitOptions.None)
					str(0) = Net.WebUtility.HtmlDecode(str(0))
					If str.Count < 7 Then
						ReDim Preserve str(6)
						str(6) = "0"
					End If
					'Label2.BeginInvoke(Sub() Label2.Text = str(0))

					Dim sql As String = "SELECT id FROM main "
					'sql += "LEFT OUTER JOIN category ON main.id = category.id_main "
					sql += "WHERE name = '" + str(0).Replace("'", "''") + "'"
					Dim reader = db.queryReader(sql)

					If Not reader.HasRows Then
						Dim prices = str(1).Replace(",", "").Replace(" ", "").Split({"/"c}, System.StringSplitOptions.None)
						If prices(0).Trim = "" Then prices(0) = 0
						If prices(1).Trim = "" Then prices(1) = 0
						If prices(2).Trim = "" Then prices(2) = 0
						sql = "INSERT INTO main "
						sql += "(name, data_dec1, data_dec2, data_dec3, "
						sql += "data_str1, data_str2, data_str3, data_txt1, "
						sql += "data_num1) "
						sql += " VALUES ( '" + str(0).Replace("'", "''") + "', "
						sql += prices(0).Replace("$", "") + ", " + prices(1).Replace("$", "") + ", " + prices(2).Replace("$", "") + ", "
						sql += "'" + str(2).Replace("'", "''") + "', '" + str(3).Replace("'", "''") + "', '" + str(4).Replace("'", "''") + "', "
						sql += "'" + str(5).Replace("'", "''") + "', '" + str(6) + "'); "
						db.execute(sql)

						Dim lastRow = db.getLastRowID
						sql = "INSERT INTO category (id_main, cat) VALUES ("
						sql += lastRow.ToString + ", '" + cat + "'); "
						db.execute(sql)
					Else
						reader.Read()
						Dim id_main = reader.GetInt32(0)
						sql = "SELECT main.id FROM main "
						sql += "JOIN category ON main.id = category.id_main "
						sql += "WHERE name = '" + str(0).Replace("'", "''") + "' "
						sql += "AND category.cat = '" + cat + "'"
						reader = db.queryReader(sql) : reader.Read()
						'If Not reader.GetString(0).ToUpper.Contains(cat.ToUpper) Then
						'cat = reader.GetString(0) + ";" + cat
						'sql = "UPDATE main SET data_str4 = '" + cat + "' WHERE name = '" + str(0).Replace("'", "''") + "';"
						'db.execute(sql)
						'End If
						If Not reader.HasRows Then
							sql = "INSERT INTO category (id_main, cat) VALUES ("
							sql += id_main.ToString + ", '" + cat + "'); "
							db.execute(sql)
						End If
					End If
				Next
				db.execute("COMMIT;")
			End If
		Next
		For Each file In files
			If file.ToUpper.EndsWith("_DESCRIPTIONS.TXT") Then
				Dim tmp = file
				Label_zero.BeginInvoke(Sub() Label_zero.Text = tmp)

				Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
				Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

				Dim curName As String = ""
				Dim wIncluded As Boolean = False
				db.execute("BEGIN;")
				For Each line In lines
					If line.StartsWith("[") And Not line.ToUpper.StartsWith("[%WHAT'S") Then
						curName = line.Substring(1, line.Length - 2)
						'Label2.BeginInvoke(Sub() Label2.Text = curName)
						wIncluded = False
					ElseIf line.ToUpper.StartsWith("[%WHAT'S") Then
						wIncluded = True
					Else
						If Not wIncluded Then
							Dim sql As String = "UPDATE main "
							sql += "SET data_txt2 = '" + line.Replace("'", "''") + "' "
							sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
							db.execute(sql)
						Else
							Dim sql As String = "UPDATE main "
							sql += "SET data_txt3 = '" + line.Replace("'", "''") + "' "
							sql += "WHERE name = '" + curName.Replace("'", "''") + "';"
							db.execute(sql)
						End If
					End If
				Next
				db.execute("COMMIT;")
			End If
		Next
		For Each file In files
			If file.ToLower.Contains("sku_master_daz") Then
				Dim all = My.Computer.FileSystem.OpenTextFileReader(file).ReadToEnd
				Dim lines = all.Split({vbCrLf}, System.StringSplitOptions.RemoveEmptyEntries)

				db.execute("BEGIN;")
				For Each line In lines
					Dim spc1 = line.IndexOf(" ")
					Dim tmp = line.Substring(spc1).Trim
					Dim spc2 = tmp.IndexOf(" ")
					Dim code1 As String = line.Substring(0, spc1).Trim
					Dim code2 As String = tmp.Substring(0, spc2).Trim
					Dim name As String = tmp.Substring(spc2 + 1).Trim
					Label_zero.BeginInvoke(Sub() Label_zero.Text = "Importing SKU - " + name)

					Dim sql = "UPDATE main "
					sql += "SET data_num1 = " + code2 + ", data_str5 = '" + code1 + "' "
					sql += "WHERE name = '" + Net.WebUtility.HtmlDecode(name).Replace("'", "''") + "';"
					db.execute(sql)

					sql = "UPDATE main "
					sql += "SET data_num1 = " + code2 + ", data_str5 = '" + code1 + "' "
					sql += "WHERE name = 'zz - " + Net.WebUtility.HtmlDecode(name).Replace("'", "''") + "';"
					db.execute(sql)
				Next
				db.execute("COMMIT;")
			End If
		Next
		Label_zero.BeginInvoke(Sub() Label_zero.Text = "DONE")
	End Sub
End Class

Public Class filterItem
    Public filterShown As String = ""
    Public filterReal As String = ""

    Public Overrides Function ToString() As String
        Return filterShown
        'Return filterReal
    End Function
End Class
Public Class checkedCombo
	Inherits ComboBox

	Public Event checkedChanged(o As Object, e As EventArgs)
	'Public Shadows Event SelectedIndexChanged()
	Dim WithEvents l As New CheckedListBox

	Public Sub New(c As ComboBox)
		For Each i As String In c.Items
			l.Items.Add(i)
		Next
		l.Visible = False
		l.CheckOnClick = True

		Me.Name = c.Name
		'MyBase.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed 'I don't know what is it for, but it prevent text in combobox to show
		MyBase.DropDownStyle = ComboBoxStyle.DropDownList
		MyBase.DropDownHeight = 1
		MyBase.DropDownWidth = 1
		MyBase.FormattingEnabled = True
		MyBase.IntegralHeight = False

		''this.comboBox1.DropDown += New System.EventHandler(this.comboBox1_DropDown);
		''this.comboBox1.DropDownClosed += New System.EventHandler(this.comboBox1_DropDownClosed);
	End Sub

	Sub open() Handles MyBase.DropDown
		'l.Visible = True
	End Sub
	Sub OpenClose() Handles MyBase.MouseClick
		MyBase.DroppedDown = False 'this throw error, if we try to close combobox with 0 items, after clearing the items in it
		l.Top = Me.Top + Me.Height
		l.Left = Me.Left
		l.Visible = Not l.Visible
	End Sub
	Shadows Sub parentChanged(o As Object, e As EventArgs) Handles MyBase.ParentChanged
		If l.Parent IsNot Nothing Then l.Parent.Controls.Remove(l)
		If Me.Parent IsNot Nothing Then Me.Parent.Controls.Add(l)
	End Sub

	Dim refr = False
	Sub ListCheckedChanged(o As Object, e As ItemCheckEventArgs) Handles l.ItemCheck
		If refr Then Exit Sub

		refr = True : l.SetItemChecked(e.Index, e.NewValue) : refr = False

		'Show checked items in combo
		Dim str = ""
		For n As Integer = 0 To l.Items.Count - 1
			If l.GetItemChecked(n) Then
				str += l.Items(n).ToString.Trim + ", "
			End If
		Next
		MyBase.Items.Clear()
		If str <> "" Then str = str.Substring(0, str.Length - 2)
		MyBase.Items.Add(str) : MyBase.SelectedIndex = 0

		RaiseEvent checkedChanged(Me, New EventArgs)
		'RaiseEvent SelectedIndexChanged()
	End Sub

	Public Property CheckedItems As String
		Get
			Return String.Join(";", l.CheckedItems.Cast(Of String))
		End Get
		Set(value As String)
			Dim needToCheck = value.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).ToList.Select(Of String)(Function(x) x.Trim.ToUpper)
			Dim str = ""
			For n As Integer = 0 To l.Items.Count - 1
				If needToCheck.Contains(l.Items(n).ToString.Trim.ToUpper) Then
					l.SetItemChecked(n, True)
					str += l.Items(n).ToString.Trim + ", "
				Else
					l.SetItemChecked(n, False)
				End If
			Next

			MyBase.Items.Clear()
			If str <> "" Then str = str.Substring(0, str.Length - 2)
			MyBase.Items.Add(str) : MyBase.SelectedIndex = 0
		End Set
	End Property
End Class
Public Class MenuItem_My
	Inherits MenuItem

	Protected Overrides Sub OnClick(e As EventArgs)
		Dim btnL As Boolean = False
		Dim btnR As Boolean = False
		If Control.MouseButtons = MouseButtons.Left Then btnL = True
		If Control.MouseButtons = MouseButtons.Right Then btnR = True
		Exit Sub
		MyBase.OnClick(e)
	End Sub
End Class