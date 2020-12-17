
'TODO - Agressive numeric/decimal parsing, using string.join("", str.where(c=>c.IsDigit)) or regex

'DONE - output data_dec1 field is not showed as price in unreal catalog

Public Class FormD_ConvertColumn
    Private Sub FormD_ConvertColumn_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim ini = Form1.ini

        Dim rdr = db.queryReader("PRAGMA table_info(main)")
        Do While rdr.Read
            Dim n = rdr.GetString(1)
            Dim t = rdr.GetString(2)
            If n.ToUpper = "NAME" OrElse n.ToUpper = "ID" Then Continue Do

            'Dim f_name = ini.IniReadValue("Interface", "Field" + n.Substring(n.IndexOf("_")))
            Dim f_name = ""
            Dim f_info = Fields.Where(Function(fi) fi.DBname.ToUpper = n.ToUpper).FirstOrDefault()
            If f_info IsNot Nothing Then f_name = f_info.name
            If t.ToUpper.StartsWith("VARCHAR") Then
                If f_name <> "" Then f_name = " - " + f_name
                ComboBox1.Items.Add(n + f_name)
                ComboBox2.Items.Add(n + " (date)" + f_name)
            ElseIf t.ToUpper.StartsWith("INT") Then
                If f_name <> "" Then f_name = " - " + f_name
                ComboBox2.Items.Add(n + " (int)" + f_name)
            ElseIf t.ToUpper = "NUMERIC" Then
                If f_name <> "" Then f_name = " - " + f_name
                ComboBox2.Items.Add(n + " (dec)" + f_name)
            End If
        Loop

        If ComboBox1.Items.Count > 0 Then ComboBox1.SelectedIndex = 0
        If ComboBox2.Items.Count > 0 Then ComboBox2.SelectedIndex = 0
    End Sub

    'Convert
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim f_from = ComboBox1.SelectedItem.ToString
        Dim f_to = ComboBox2.SelectedItem.ToString

        Dim use_default = TextBox1.Text.Trim
        Dim default_date = DateTime.Now
        Dim default_date_str = ""
        Dim default_int = 0
        Dim default_dec As Decimal = 0
        Dim g_ns = Globalization.NumberStyles.Number
        Dim g_cl = Globalization.CultureInfo.InvariantCulture

        Dim mode = ""
        If f_to.Contains(" (date)") Then
            mode = "DATE"
            If use_default <> "" AndAlso Not DateTime.TryParse(use_default, default_date) Then use_default = ""
            default_date_str = default_date.Year.ToString + "." + default_date.Month.ToString("00") + "." + default_date.Day.ToString("00")
        ElseIf f_to.Contains(" (int)") Then
            mode = "INT"
            If use_default <> "" AndAlso Not Integer.TryParse(use_default, default_int) Then use_default = ""
        ElseIf f_to.Contains(" (dec)") Then
            mode = "DEC"
            If use_default <> "" AndAlso Not Decimal.TryParse(use_default, g_ns, g_cl, default_dec) Then use_default = ""
        End If

        If f_from.Contains(" ") Then f_from = f_from.Substring(0, f_from.IndexOf(" ")).Trim
        If f_to.Contains(" ") Then f_to = f_to.Substring(0, f_to.IndexOf(" ")).Trim

        Dim src As String = ""
        Dim res_date = DateTime.Now
        Dim res_int = 0
        Dim res_dec As Decimal = 0

        Dim rdr = db.queryReader("SELECT id, " + f_from + " FROM main")
        If Not rdr.HasRows Then MsgBox("Main table is empty.") : Exit Sub

        db.execute("BEGIN;")
        Do While rdr.Read
            If Not rdr.IsDBNull(1) Then src = rdr.GetString(1) Else src = ""
            Select Case mode
                Case "DATE"
                    If DateTime.TryParse(src, res_date) Then
                        Dim res_date_str = res_date.Year.ToString + "." + res_date.Month.ToString("00") + "." + res_date.Day.ToString("00")
                        db.execute("UPDATE main SET " + f_to + " = '" + res_date_str + "' WHERE id = " + rdr.GetInt32(0).ToString)
                    ElseIf use_default <> "" Then
                        db.execute("UPDATE main SET " + f_to + " = '" + default_date_str + "' WHERE id = " + rdr.GetInt32(0).ToString)
                    End If
                Case "INT"
                    If Integer.TryParse(src, res_int) Then
                        db.execute("UPDATE main SET " + f_to + " = " + res_int.ToString + " WHERE id = " + rdr.GetInt32(0).ToString)
                    ElseIf use_default <> "" Then
                        db.execute("UPDATE main SET " + f_to + " = " + default_int.ToString + " WHERE id = " + rdr.GetInt32(0).ToString)
                    End If
                Case "DEC"
                    If Decimal.TryParse(src, g_ns, g_cl, res_dec) Then
                        db.execute("UPDATE main SET " + f_to + " = " + res_dec.ToString.Replace(",", ".") + " WHERE id = " + rdr.GetInt32(0).ToString)
                    ElseIf use_default <> "" Then
                        db.execute("UPDATE main SET " + f_to + " = " + default_dec.ToString.Replace(",", ".") + " WHERE id = " + rdr.GetInt32(0).ToString)
                    End If
            End Select
        Loop
        db.execute("COMMIT;")
    End Sub
End Class