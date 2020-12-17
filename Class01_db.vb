Imports System.Data.SQLite

Public Class Class01_db
    Private tr As SQLiteTransaction = Nothing
    Private Shared con As SQLiteConnection
    Private command As SQLiteCommand
    Public affectedRows As Integer = -1

    Structure index_info
        Dim name As String
        Dim field As String
        Dim isUnique As Boolean
    End Structure

    Public Sub create_database()
        Try
            Dim i = path_db.LastIndexOf("\")
            Dim tmp = path_db.Substring(0, i)
            If Not FileIO.FileSystem.DirectoryExists(tmp) Then
                My.Computer.FileSystem.CreateDirectory(tmp)
            End If
            SQLiteConnection.CreateFile(path_db)
        Catch ex As Exception
            MsgBox("Can not create database: " + vbCrLf + path_db + vbCrLf + vbCrLf + ex.Message)
            Application.Exit()
        End Try

        con = New SQLiteConnection("Data Source=" + path_db + ";Version=3;")
        con.Open()

        create_database_createMainTable()

        Dim sql = "create table category (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, cat varchar(255) )"
        execute(sql)

        sql = "create table custom_lists (id INTEGER PRIMARY KEY AUTOINCREMENT, name varchar(255) )"
        execute(sql)
        sql = "create table custom_lists_data (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, id_list int )"
        execute(sql)

        'Количество коментарных столбцов должно совпадать с количеством статусов
        sql = "create table status_data (id INTEGER PRIMARY KEY AUTOINCREMENT, main_id int, "
        sql += "status1 varchar(28), status2 varchar(28), status3 varchar(28), status4 varchar(28), status5 varchar(28), status6 varchar(28), status7 varchar(28), "
        sql += "comment1 text, comment2 text, comment3 text, comment4 text, comment5 text, comment6 text, comment7 text )"
        execute(sql)

        'sql = "create table files (id INTEGER PRIMARY KEY AUTOINCREMENT, main_id int, file varchar(511) )"
        'execute(sql)

        sql = "create table unrecognized_files (id INTEGER PRIMARY KEY AUTOINCREMENT, path varchar(255), file varchar(350), t TIMESTAMP DEFAULT (datetime('now','localtime')) )"
        execute(sql)

        sql = "create table groups (id INTEGER PRIMARY KEY AUTOINCREMENT, name varchar(255), field varchar(64), value text )"
        execute(sql)

        sql = "create table paths (id INTEGER PRIMARY KEY AUTOINCREMENT, main_id int, name varchar(255), value text )"
        execute(sql)

        'Tables for installed products
        sql = "create table product_files (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, subName varchar(255), file varchar(511) )"
        execute(sql)
        sql = "create table product_files_found (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, subName varchar(255), files int, files_found int )"
        execute(sql)
        sql = "create table product_files_found_doubles (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, path varchar(255), file varchar(511), t TIMESTAMP DEFAULT (datetime('now','localtime')) )"
        execute(sql)
        sql = "create table product_files_found_details (id INTEGER PRIMARY KEY AUTOINCREMENT, id_main int, subName varchar(255), file varchar(511), found boolean )"
        execute(sql)
        sql = "create table product_files_unrecognized (id INTEGER PRIMARY KEY AUTOINCREMENT, file varchar(350) )"
        execute(sql)
    End Sub
    Public Sub create_database_createMainTable()
        'By default, create main table with 10 fields of each type
        fieldCountArr = Enumerable.Repeat(10, fieldTypeArr.Count).ToArray()

        Dim sql As String = "create table main (id INTEGER PRIMARY KEY AUTOINCREMENT, name varchar(255), "
        For i = 1 To fieldCountArr(0)
            sql += "data_str" + i.ToString + " varchar(255), "
        Next
        For i = 1 To fieldCountArr(1)
            sql += "data_num" + i.ToString + " int, "
        Next
        For i = 1 To fieldCountArr(2)
            sql += "data_dec" + i.ToString + " numeric, "
        Next
        For i = 1 To fieldCountArr(3)
            sql += "data_bool" + i.ToString + " boolean, "
        Next
        For i = 1 To fieldCountArr(4)
            sql += "data_txt" + i.ToString + " text, "
        Next
        sql = sql.Substring(0, sql.Length - 2) + " )"
        execute(sql)
    End Sub

    Public Sub connect()
        If Not My.Computer.FileSystem.FileExists(path_db) Then create_database() : Exit Sub

        con = New SQLiteConnection("Data Source=" + System.IO.Path.GetFullPath(path_db) + ";Version=3;")
        con.Open()

        Dim func As New SqliteFunction_Append
        Dim attr = func.GetType().GetCustomAttributes(GetType(SQLiteFunctionAttribute), True).Cast(Of SQLiteFunctionAttribute).ToArray
        If attr.Count > 0 Then
            con.BindFunction(attr(0), func)
        Else
            MsgBox("SQLiteFunction doesn't have SQLiteFunctionAttribute")
        End If

        'Get all main table fields count by type
        fieldCountArr = New Integer(fieldTypeArr.Count - 1) {}
        Dim r_info = db.queryReader("PRAGMA table_info(main)")
        Do While r_info.Read
            Dim colName = r_info.GetString(1).Trim
            If Not colName.Contains("_") Then Continue Do

            colName = colName.Substring(colName.IndexOf("_"))
            For field_type_N = 0 To fieldTypeArr.Count - 1
                Dim field_type = fieldTypeArr(field_type_N) : field_type = field_type.Substring(field_type.IndexOf("_"))
                If colName.ToUpper.StartsWith(field_type.ToUpper) Then
                    fieldCountArr(field_type_N) += 1 : Exit For
                End If
            Next
        Loop
    End Sub
    Public Sub close()
        If tr IsNot Nothing Then tr.Dispose() : tr = Nothing
        If command IsNot Nothing Then command.Reset() : command.Dispose() : command = Nothing

        con.Close()
        con.Dispose()
        con = Nothing

        GC.Collect()
        GC.WaitForPendingFinalizers()
    End Sub

    Public Sub execute(ByVal sql As String, Optional reuseCommand As Boolean = False)
        If con Is Nothing OrElse con.State <> System.Data.ConnectionState.Open Then Exit Sub
        'Dim tbl As String = "create table highscores (name varchar(20), score int)"
        'Dim sql2 As String = "insert into highscores (name, score) values ('Me', 9001)"
        If command IsNot Nothing And reuseCommand Then
            command.CommandText = sql
        Else
            command = New SQLiteCommand(sql, con)
        End If
        If tr IsNot Nothing Then command.Transaction = tr

        'Try
        affectedRows = command.ExecuteNonQuery()

        'Catch ex As Exception
        'MsgBox(ex.Message)
        'End Try
    End Sub

    Public Function queryReader(ByVal sql As String) As SQLiteDataReader
        If con Is Nothing OrElse con.State <> System.Data.ConnectionState.Open Then Return Nothing
        'Dim sql As String = "select * from highscores order by score desc"
        Try
            Dim cmd As New SQLiteCommand(sql, con)
            Return cmd.ExecuteReader()
        Catch ex As Exception
            MsgBox(ex.Message)
            Return Nothing
        End Try
        'Dim reader = command.ExecuteReader()
        'While reader.Read
        'Console.WriteLine(reader("name"))
        'End While
    End Function

    Public Function queryDataset(ByVal sql As String) As DataTable
        If con Is Nothing OrElse con.State <> System.Data.ConnectionState.Open Then Return Nothing
        Dim da As New SQLiteDataAdapter(sql, con)
        Dim ds As New DataSet
        da.Fill(ds)
        Return ds.Tables(0)
    End Function

    Public Function getLastRowID(Optional reuseCommand As Boolean = False) As Integer
        If command IsNot Nothing And reuseCommand Then
            command.CommandText = "select last_insert_rowid()"
        Else
            command = New SQLiteCommand("select last_insert_rowid()", con)
        End If

        Return CInt(DirectCast(command.ExecuteScalar(), Int64))
    End Function

    Public Function GetIndexInfo(Optional table As String = "Main") As index_info()
        Dim ii_list As New List(Of index_info)
        Dim sql = "SELECT name, sql FROM sqlite_master WHERE type = 'index'"
        If table <> "*" Then sql += " AND tbl_name = '" + table + "'"
        sql += " COLLATE NOCASE"

        Dim rdr = queryReader(sql)
        While rdr.Read
            Dim ii As New index_info()
            ii.name = rdr.GetString(0)
            ii.field = rdr.GetString(1).SubstringBetween("(", ")")
            ii.isUnique = rdr.GetString(1).ToUpper().Contains(" UNIQUE ")
            ii_list.Add(ii)
        End While
        Return ii_list.ToArray()
    End Function

    Public Sub transactionBegin()
        tr = con.BeginTransaction()
    End Sub
    Public Sub transactionCommit()
        If tr Is Nothing Then Exit Sub
        tr.Commit()
        tr.Dispose()
        tr = Nothing
    End Sub
End Class

<SQLiteFunction(Name:="APPEND", Arguments:=2, FuncType:=FunctionType.Scalar)>
Public Class SqliteFunction_Append
    Inherits SQLiteFunction

    Public Overrides Function Invoke(args() As Object) As Object
        'Return MyBase.Invoke(args)
        'Return System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(args(1)), Convert.ToString(args(0)))
        If args(0) Is Nothing Then Return Convert.ToString(args(1))
        If Convert.ToString(args(0)).Trim = "" Then Return Convert.ToString(args(1))

        If args(1) Is Nothing Then Return Convert.ToString(args(0))
        If Convert.ToString(args(1)).Trim = "" Then Return Convert.ToString(args(0))

        Return Convert.ToString(args(0)) + ";" + Convert.ToString(args(1))
    End Function
End Class