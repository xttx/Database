Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Public Class Class50_CodeParser

    Dim src As String = ""
    Dim STree As SyntaxTree

    'Public HighLights As New List(Of HighLights_Info)
    Public HighLights As New Collections.Concurrent.BlockingCollection(Of HighLights_Info)
    Class HighLights_Info
        Public location As Location
    End Class

    Dim EXCLUDE_NAMESPACES As New List(Of String)
    Dim INITIAL_DEFAULT_USINGS As String() = {"Microsoft.VisualBasic", "System", "System.Collections", "System.Collections.Generic", "System.Data", "System.Diagnostics", "System.Drawing", "System.Linq", "System.Runtime.Serialization", "System.Windows.Forms", "System.Xml.Linq"}

    Shared init_in_process As Boolean = False
    Shared classes_dictionary As Classes_Dict = Nothing
    Public user_variables_field As New Classes_Dict
    Public user_variables_local As New Classes_Dict
    Public user_usings As New Classes_Dict
    Public Shared default_usings As New List(Of Classes_Dict)
    Class Classes_Dict
        Public dict As New Dictionary(Of String, Classes_Dict)(StringComparer.InvariantCultureIgnoreCase)
        Public name As String = ""
        Public full_name As String = ""
        Public t_type As Type_Enum = Type_Enum.NotSet
        Public is_static As Boolean = False
        Public is_generic As Boolean = False
        Public return_type As String = ""
        Public return_type_full As String = ""
        Public derived_from As New List(Of String)
        Public sub_types As New List(Of KeyValuePair(Of String, Classes_Dict))
    End Class

    Public Enum Type_Enum
        NotSet
        N_Namespace
        T_class
        T_Interface
        T_struct
        T_enum
        T_Delegate
        P_Property
        P_Indexer
        M_Method
        M_ExtMethod
        F_Field
        F_Variable
        F_CONST
        F_READONLY
        E_Event
        Enum_Value
        K_Keyword
    End Enum

    Public Sub New()
        EXCLUDE_NAMESPACES.Add("CSharpCompiler")
        EXCLUDE_NAMESPACES.Add("DG")
        EXCLUDE_NAMESPACES.Add("ExCSS")
        EXCLUDE_NAMESPACES.Add("ICSharpCode.NRefactory")
        EXCLUDE_NAMESPACES.Add("IKVM")
        EXCLUDE_NAMESPACES.Add("Internal.Runtime.CompilerServices")
        EXCLUDE_NAMESPACES.Add("Internal.Runtime.Augments")
        EXCLUDE_NAMESPACES.Add("JetBrains.Annotations")
        EXCLUDE_NAMESPACES.Add("Microsoft.Win32.SafeHandles")
        EXCLUDE_NAMESPACES.Add("System.Deployment.Internal")
        EXCLUDE_NAMESPACES.Add("System.Configuration.Assemblies")
        EXCLUDE_NAMESPACES.Add("System.Linq.Parallel")
        EXCLUDE_NAMESPACES.Add("System.Linq.Expressions")
        EXCLUDE_NAMESPACES.Add("System.Linq.Expressions.Compiler")
        EXCLUDE_NAMESPACES.Add("System.Dynamic")
        EXCLUDE_NAMESPACES.Add("System.Configuration")
        EXCLUDE_NAMESPACES.Add("System.Web.UI")
        EXCLUDE_NAMESPACES.Add("XamMac.CoreFoundation")
        EXCLUDE_NAMESPACES.Add("Codice")
        EXCLUDE_NAMESPACES.Add("CollabProxy")
        EXCLUDE_NAMESPACES.Add("GluonGui")
        EXCLUDE_NAMESPACES.Add("Interop")
        EXCLUDE_NAMESPACES.Add("log4net")
        EXCLUDE_NAMESPACES.Add("Microsoft.SqlServer")
        EXCLUDE_NAMESPACES.Add("Microsoft.Unity")
        EXCLUDE_NAMESPACES.Add("Mono.Unix")
        EXCLUDE_NAMESPACES.Add("Newtonsoft")
        EXCLUDE_NAMESPACES.Add("Packages")
        EXCLUDE_NAMESPACES.Add("PlasticGui")
        EXCLUDE_NAMESPACES.Add("PlasticPipe")
        EXCLUDE_NAMESPACES.Add("SyntaxTree")
        EXCLUDE_NAMESPACES.Add("System.Collections.Immutable")
        EXCLUDE_NAMESPACES.Add("System.Reflection.Metadata")
        EXCLUDE_NAMESPACES.Add("System.Runtime.Serialization")
        EXCLUDE_NAMESPACES.Add("System.Web.Configuration")
        EXCLUDE_NAMESPACES.Add("System.Web.Hosting")
        EXCLUDE_NAMESPACES.Add("System.Web.Management")
        EXCLUDE_NAMESPACES.Add("System.Web.ModelBinding")
        EXCLUDE_NAMESPACES.Add("System.Windows")

        If classes_dictionary Is Nothing Then
            If init_in_process Then Exit Sub

            init_in_process = True
            Dim bg As New BackgroundWorker()
            AddHandler bg.DoWork, AddressOf Init_Intellisence
            bg.RunWorkerAsync()
        End If

        Dim bg_parser As New BackgroundWorker()
        AddHandler bg_parser.DoWork, AddressOf Parse_bg
        bg_parser.RunWorkerAsync()
    End Sub

    Sub Init_Intellisence()
        classes_dictionary = New Classes_Dict()

        Dim sw = Stopwatch.StartNew()

        Dim assemblies = AppDomain.CurrentDomain.GetAssemblies()
        For Each asm In assemblies
            Dim namespaces_arr = asm.GetTypes().Select(Function(x) x.Namespace).Distinct()
            For Each nmsp In namespaces_arr
                Dim types = asm.GetTypes().Where(Function(x) x.Namespace = nmsp)
                For Each t In types
                    If Not t.IsVisible Then Continue For

                    Dim t_name = t.FullName
                    t_name = t_name.Replace("+", ".").Replace(">", "").Replace("<", "")
                    If t_name.Contains("`") Then
                        'Generic class definitions
                        Dim ind1 = t_name.IndexOf("`")
                        Dim ind2 = t_name.IndexOf(".", ind1)
                        If ind2 < 0 Then
                            t_name = t_name.Substring(0, ind1)
                        Else
                            t_name = t_name.Substring(0, ind1) + t_name.Substring(ind2)
                        End If
                    End If

                    Dim current_level_dict As Classes_Dict = classes_dictionary
                    For Each n As String In t_name.Split(New Char() {"."c}, StringSplitOptions.RemoveEmptyEntries)
                        If Not current_level_dict.dict.ContainsKey(n) Then current_level_dict.dict.Add(n, New Classes_Dict())
                        current_level_dict = current_level_dict.dict(n)
                    Next
                    current_level_dict.name = t_name
                    current_level_dict.full_name = t.FullName

                    If t.IsInterface Then current_level_dict.t_type = Type_Enum.T_Interface
                    If t.IsValueType AndAlso Not t.IsEnum Then current_level_dict.t_type = Type_Enum.T_struct
                    If t.BaseType = GetType(MulticastDelegate) Then current_level_dict.t_type = Type_Enum.T_Delegate

                    'Class
                    If t.IsClass Then
                        current_level_dict.t_type = Type_Enum.T_class
                        current_level_dict.is_static = t.IsAbstract AndAlso t.IsSealed

                        If t.GetInterfaces().Length <> 0 Then
                            Dim i_str = t.GetInterfaces().Select(Function(i) i.ToString())
                            current_level_dict.derived_from.AddRange(i_str)
                            'Dim interfaces = " (Interfaces: " + String.Join(", ", i_str) + ")"
                        End If
                    End If

                    'Class/Struct generic argument - Dictionary<TKey, TValue>();
                    Dim arguments = t.GetGenericArguments()
                    If arguments.Length > 0 Then
                        Dim generic_class_args = String.Join(", ", arguments.Select(Function(x) x.Name))
                        current_level_dict.is_generic = True
                        current_level_dict.full_name = current_level_dict.full_name.Replace("+", ".")

                        'Remove generic "`1" or "`12" symbols
                        Dim ind = current_level_dict.full_name.IndexOf("`")
                        If ind > 0 Then
                            If current_level_dict.full_name.Length > ind + 7 AndAlso Char.IsDigit(current_level_dict.full_name(ind + 2)) Then
                                'TODO: Check what is this, as this is never happens
                                current_level_dict.full_name = current_level_dict.full_name.Remove(ind, 3)
                            Else
                                current_level_dict.full_name = current_level_dict.full_name.Remove(ind, 2)
                            End If
                        End If
                        current_level_dict.full_name += "<" + generic_class_args + ">"

                        For Each arg In arguments
                            current_level_dict.sub_types.Add(New KeyValuePair(Of String, Classes_Dict)(arg.Name, Nothing))
                        Next
                    End If

                    'Enum
                    If t.IsEnum Then
                        current_level_dict.t_type = Type_Enum.T_enum
                        For Each s In t.GetEnumNames()
                            If Not current_level_dict.dict.ContainsKey(s) Then
                                Dim ncd = New Classes_Dict() : ncd.name = s : ncd.t_type = Type_Enum.Enum_Value
                                current_level_dict.dict.Add(s, ncd)
                            End If
                        Next
                    End If

                    'Constructors
                    Dim methodInfos_constructor = t.GetConstructors()
                    For Each ci In methodInfos_constructor
                        'TODO
                    Next

                    'Methods
                    Dim methodInfos = t.GetMethods(BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.Static)
                    For Each mi In methodInfos
                        Dim m_name = mi.Name

                        'If current_level_dict.dict.ContainsKey(m_name) Then Continue For
                        Dim overload_index = 0
                        While current_level_dict.dict.ContainsKey(m_name)
                            overload_index += 1
                            m_name = mi.Name + "{" + overload_index.ToString() + "}"
                        End While

                        Dim ncd = New Classes_Dict() : ncd.name = m_name : ncd.t_type = Type_Enum.M_Method

                        'Operators + - * /
                        Dim op = ""
                        If mi.IsSpecialName Then
                            'TODO
                            'op = Intellisense_Init_Parse_Operator(mi.Name)
                            'If op is nothing Then Continue For

                            'Dim param_types = mi.GetParameters().Select(Function(p) p.ParameterType.FullName)
                            'Dim operator_signature = op + "|" + String.Join("|", param_types)
                            'If current_level_dict.operators.ContainsKey(operator_signature) Then Continue For
                            'current_level_dict.operators.Add(operator_signature, ncd)
                        Else
                            current_level_dict.dict.Add(m_name, ncd)
                        End If

                        ncd.is_static = mi.IsStatic
                        ncd.full_name = t.FullName + "." + mi.Name
                        ncd.return_type = mi.ReturnType.Name : ncd.return_type_full = mi.ReturnType.FullName
                        If ncd.return_type_full IsNot Nothing Then
                            ncd.return_type_full = ncd.return_type_full.Replace("+", ".")
                        Else
                            'Return generic type - T, T2, TResult, TKey
                            If ncd.return_type IsNot Nothing Then ncd.return_type_full = ncd.return_type
                        End If

                    Next

                    'Properties

                    'Fields

                    'Events
                Next
            Next
        Next

        'Predefined types
        init_in_process = False
        classes_dictionary.dict.Add("Boolean", Search_Chain("System.Boolean"))
        classes_dictionary.dict.Add("Byte", Search_Chain("System.Byte"))
        classes_dictionary.dict.Add("SByte", Search_Chain("System.SByte"))
        classes_dictionary.dict.Add("Short", Search_Chain("System.Int16"))
        classes_dictionary.dict.Add("Integer", Search_Chain("System.Int32"))
        classes_dictionary.dict.Add("Long", Search_Chain("System.Int64"))
        classes_dictionary.dict.Add("UShort", Search_Chain("System.UInt16"))
        classes_dictionary.dict.Add("UInteger", Search_Chain("System.UInt32"))
        classes_dictionary.dict.Add("ULong", Search_Chain("System.UInt64"))
        classes_dictionary.dict.Add("Single", Search_Chain("System.Single"))
        classes_dictionary.dict.Add("Double", Search_Chain("System.Double"))
        classes_dictionary.dict.Add("Decimal", Search_Chain("System.Decimal"))
        classes_dictionary.dict.Add("Char", Search_Chain("System.Char"))
        classes_dictionary.dict.Add("String", Search_Chain("System.String"))
        classes_dictionary.dict.Add("Date", Search_Chain("System.DateTime"))
        classes_dictionary.dict.Add("Object", Search_Chain("System.Object"))

        'Default usings
        For Each u In INITIAL_DEFAULT_USINGS
            'TODO: System.Xml.Linq is not added
            Dim cl = Search_Chain(u)
            If cl IsNot Nothing Then default_usings.Add(cl)
        Next

        sw.Stop()
    End Sub

    Public Sub Parse(source As String, Optional update As Boolean = False)
        src = source
    End Sub
    Public Sub Parse_bg()
        While True
            If src = "" Then
                Threading.Thread.Sleep(10) : Continue While
            End If

            user_variables_field = New Classes_Dict()
            user_variables_local = New Classes_Dict()

            'If STree Is Nothing OrElse Not Update() Then
            If STree Is Nothing Then
                STree = VisualBasicSyntaxTree.ParseText(src)
            Else
                Dim src_text = Text.SourceText.From(src)
                STree = STree.WithChangedText(src_text)
            End If

            Dim root = STree.GetRoot()
            Dim walker = New VBWalker(Me)
            walker.Visit(root)

            For Each var_field In walker.Variables_Fields
                Dim cl = Search_Chain(var_field.var_type.Split({"."c}))
                If cl Is Nothing Then Continue For

                Dim new_variable As New Classes_Dict()
                new_variable.name = var_field.name
                new_variable.dict = cl.dict
                new_variable.return_type = cl.name
                new_variable.return_type_full = cl.full_name
                user_variables_field.dict.Add(var_field.name, new_variable)
            Next

            For Each var_field In walker.Variables_Locals

            Next

            src = ""
        End While
    End Sub

    Public Function Search_Chain(chain As String) As Classes_Dict
        Return Search_Chain(chain.Split({"."c}))
    End Function
    Public Function Search_Chain(chain As String()) As Classes_Dict
        If classes_dictionary Is Nothing OrElse init_in_process Then Return Nothing

        Dim current As Classes_Dict = classes_dictionary

        For n = 0 To chain.Length - 1
            Dim str = chain(n)

            If current.dict.ContainsKey(str) Then
                current = current.dict(str)
            ElseIf user_variables_field.dict.ContainsKey(str) Then
                current = user_variables_field.dict(str)
            ElseIf n = 0 Then
                Dim found = False

                'search in usings
                For Each u In default_usings
                    If u.dict.ContainsKey(str) Then current = u.dict(str) : found = True : Exit For
                Next

                If Not found Then Return Nothing
            Else
                Return Nothing
            End If
        Next

        Return current
    End Function

End Class

Class VBWalker
    Inherits VisualBasicSyntaxWalker

    Dim Parser As Class50_CodeParser

    Public Variables_Fields As New List(Of Variable_Info)
    Public Variables_Locals As New List(Of Variable_Info)
    Class Variable_Info
        Public name As String = ""
        Public var_type As String = ""
    End Class

    Public Sub New(_parser As Class50_CodeParser)
        MyBase.New(SyntaxWalkerDepth.Trivia)

        Parser = _parser
        'Parser.HighLights.Clear()
        Parser.HighLights = New Concurrent.BlockingCollection(Of Class50_CodeParser.HighLights_Info)
    End Sub

    Public Overrides Sub VisitFieldDeclaration(node As FieldDeclarationSyntax)
        'Tests
        'Dim x As Integer
        'Dim y As New xxx()
        'Dim z = 5
        'Dim w = New aaa()
        'Dim d = New Dictionary(of string, integer)

        For Each d In node.Declarators
            Dim t As String = "System.Object"
            If d.AsClause IsNot Nothing Then
                t = Parse_Type(d.AsClause.Type())
            ElseIf d.Initializer IsNot Nothing Then
                t = Parse_Type(d.Initializer.Value)
            End If

            For Each n In d.Names
                Dim v As New Variable_Info()
                v.name = n.Identifier.ValueText
                v.var_type = t
                Variables_Fields.Add(v)
            Next
        Next
        MyBase.VisitFieldDeclaration(node)
    End Sub

    'Public Overrides Sub VisitVariableDeclarator(node As VariableDeclaratorSyntax)
    '    MyBase.VisitVariableDeclarator(node)
    'End Sub
    Public Overrides Sub VisitLocalDeclarationStatement(node As LocalDeclarationStatementSyntax)
        MyBase.VisitLocalDeclarationStatement(node)
    End Sub
    Public Overrides Sub VisitMethodStatement(node As MethodStatementSyntax)
        For Each m In node.Modifiers
            Dim hi As New Class50_CodeParser.HighLights_Info()
            hi.location = m.GetLocation()
            Parser.HighLights.Add(hi)
        Next

        MyBase.VisitMethodStatement(node)
    End Sub

    Public Overrides Sub Visit(node As SyntaxNode)
        MyBase.Visit(node)
    End Sub

    Function Parse_Type(t As TypeSyntax) As String
        Return t.ToString()
    End Function
    Function Parse_Type(t As ExpressionSyntax) As String
        Return t.ToString()
    End Function

End Class
Class CSharpWalker
    Inherits CSharpSyntaxWalker

End Class