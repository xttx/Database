Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.CodeDom

'https://github.com/zanders3/json
'c# to vb.net - https://converter.telerik.com/

Namespace TinyJson
    Module JSONWriter
        Dim compact_mode As Boolean = False

        <Extension()>
        Function ToJson(ByVal item As Object, Optional compact As Boolean = False) As String
            compact_mode = compact

            Dim stringBuilder As StringBuilder = New StringBuilder()
            AppendValue(stringBuilder, item)
            Return stringBuilder.ToString()
        End Function

        Private Sub AppendValue(ByVal stringBuilder As StringBuilder, ByVal item As Object)
            If item Is Nothing Then
                stringBuilder.Append("null") : Return
            End If

            Dim type As Type = item.[GetType]()

            If type = GetType(String) OrElse type = GetType(Char) Then
                stringBuilder.Append(""""c)
                Dim str As String = item.ToString()

                For i As Integer = 0 To str.Length - 1

                    If str(i) < " "c OrElse str(i) = """"c OrElse str(i) = "\"c Then
                        stringBuilder.Append("\"c)
                        Dim j As Integer = ("""\" & vbLf & vbCr & vbTab & vbBack & vbFormFeed).IndexOf(str(i))

                        If j >= 0 Then
                            stringBuilder.Append("""\nrtbf"(j))
                        Else
                            stringBuilder.AppendFormat("u{0:X4}", AscW(str(i)))
                        End If
                    Else
                        stringBuilder.Append(str(i))
                    End If
                Next

                stringBuilder.Append(""""c)
            ElseIf type = GetType(Byte) OrElse type = GetType(SByte) Then
                stringBuilder.Append(item.ToString())
            ElseIf type = GetType(Short) OrElse type = GetType(UShort) Then
                stringBuilder.Append(item.ToString())
            ElseIf type = GetType(Integer) OrElse type = GetType(UInteger) Then
                stringBuilder.Append(item.ToString())
            ElseIf type = GetType(Long) OrElse type = GetType(ULong) Then
                stringBuilder.Append(item.ToString())
            ElseIf type = GetType(Single) Then
                stringBuilder.Append((CSng(item)).ToString(System.Globalization.CultureInfo.InvariantCulture))
            ElseIf type = GetType(Double) Then
                stringBuilder.Append((CDbl(item)).ToString(System.Globalization.CultureInfo.InvariantCulture))
            ElseIf type = GetType(Decimal) Then
                stringBuilder.Append((CDec(item)).ToString(System.Globalization.CultureInfo.InvariantCulture))
            ElseIf type = GetType(Boolean) Then
                stringBuilder.Append(If((CBool(item)), "true", "false"))
            ElseIf type.IsEnum Then
                stringBuilder.Append(""""c)
                stringBuilder.Append(item.ToString())
                stringBuilder.Append(""""c)
            ElseIf TypeOf item Is IList Then
                stringBuilder.Append("["c)
                Dim isFirst As Boolean = True
                Dim list As IList = TryCast(item, IList)

                For i As Integer = 0 To list.Count - 1

                    If isFirst Then
                        isFirst = False
                    Else
                        stringBuilder.Append(","c)
                    End If

                    AppendValue(stringBuilder, list(i))
                Next

                stringBuilder.Append("]"c)
            ElseIf type.IsGenericType AndAlso type.GetGenericTypeDefinition() = GetType(Dictionary(Of,)) Then
                Dim keyType As Type = type.GetGenericArguments()(0)

                If keyType <> GetType(String) AndAlso keyType <> GetType(Integer) Then
                    stringBuilder.Append("{}") : Return
                End If

                stringBuilder.Append("{"c)
                Dim dict As IDictionary = TryCast(item, IDictionary)
                Dim isFirst As Boolean = True

                For Each key As Object In dict.Keys

                    If isFirst Then
                        isFirst = False
                    Else
                        stringBuilder.Append(","c)
                    End If

                    stringBuilder.Append(""""c)
                    stringBuilder.Append(CStr(key))
                    stringBuilder.Append(""":")
                    AppendValue(stringBuilder, dict(key))
                Next

                stringBuilder.Append("}"c)
            Else
                stringBuilder.Append("{"c)
                Dim isFirst As Boolean = True
                Dim fieldInfos As FieldInfo() = type.GetFields(BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.FlattenHierarchy)

                For i As Integer = 0 To fieldInfos.Length - 1
                    If fieldInfos(i).IsDefined(GetType(IgnoreDataMemberAttribute), True) Then Continue For
                    Dim value As Object = fieldInfos(i).GetValue(item)

                    If value IsNot Nothing Then

                        If isFirst Then
                            isFirst = False
                        Else
                            stringBuilder.Append(","c)
                        End If

                        If Not compact_mode Then
                            stringBuilder.Append(""""c)
                            stringBuilder.Append(GetMemberName(fieldInfos(i)))
                            stringBuilder.Append(""":")
                        End If
                        AppendValue(stringBuilder, value)
                    End If
                Next

                Dim propertyInfo As PropertyInfo() = type.GetProperties(BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.FlattenHierarchy)

                For i As Integer = 0 To propertyInfo.Length - 1
                    If Not propertyInfo(i).CanRead OrElse propertyInfo(i).IsDefined(GetType(IgnoreDataMemberAttribute), True) Then Continue For
                    Dim value As Object = propertyInfo(i).GetValue(item, Nothing)

                    If value IsNot Nothing Then

                        If isFirst Then
                            isFirst = False
                        Else
                            stringBuilder.Append(","c)
                        End If

                        If Not compact_mode Then
                            stringBuilder.Append(""""c)
                            stringBuilder.Append(GetMemberName(propertyInfo(i)))
                            stringBuilder.Append(""":")
                        End If
                        AppendValue(stringBuilder, value)
                    End If
                Next

                stringBuilder.Append("}"c)
            End If
        End Sub

        Private Function GetMemberName(ByVal member As MemberInfo) As String
            If member.IsDefined(GetType(DataMemberAttribute), True) Then
                Dim dataMemberAttribute As DataMemberAttribute = CType(Attribute.GetCustomAttribute(member, GetType(DataMemberAttribute), True), DataMemberAttribute)
                If Not String.IsNullOrEmpty(dataMemberAttribute.Name) Then Return dataMemberAttribute.Name
            End If

            Return member.Name
        End Function
    End Module

    Module JSONParser
        Dim compact_mode As Boolean = False

        <ThreadStatic>
        Dim splitArrayPool As Stack(Of List(Of String))
        <ThreadStatic>
        Dim stringBuilder As StringBuilder
        <ThreadStatic>
        Dim fieldInfoCache As Dictionary(Of Type, Dictionary(Of String, FieldInfo))
        <ThreadStatic>
        Dim propertyInfoCache As Dictionary(Of Type, Dictionary(Of String, PropertyInfo))
        <ThreadStatic>
        Dim fieldInfo_Order As Dictionary(Of Type, List(Of FieldInfo))
        <ThreadStatic>
        Dim propertyInfo_Order As Dictionary(Of Type, List(Of PropertyInfo))

        <Extension()>
        Function FromJson(Of T)(ByVal json As String, Optional compact As Boolean = False) As T
            compact_mode = compact

            If propertyInfoCache Is Nothing Then propertyInfoCache = New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))()
            If fieldInfoCache Is Nothing Then fieldInfoCache = New Dictionary(Of Type, Dictionary(Of String, FieldInfo))()
            If stringBuilder Is Nothing Then stringBuilder = New StringBuilder()
            If splitArrayPool Is Nothing Then splitArrayPool = New Stack(Of List(Of String))()
            If fieldInfo_Order Is Nothing Then fieldInfo_Order = New Dictionary(Of Type, List(Of FieldInfo))
            If propertyInfo_Order Is Nothing Then propertyInfo_Order = New Dictionary(Of Type, List(Of PropertyInfo))
            stringBuilder.Length = 0

            For i As Integer = 0 To json.Length - 1
                Dim c As Char = json(i)

                If c = """"c Then
                    i = AppendUntilStringEnd(True, i, json)
                    Continue For
                End If

                If Char.IsWhiteSpace(c) Then Continue For
                stringBuilder.Append(c)
            Next

            Return CType(ParseValue(GetType(T), stringBuilder.ToString()), T)
        End Function

        Private Function AppendUntilStringEnd(ByVal appendEscapeCharacter As Boolean, ByVal startIdx As Integer, ByVal json As String) As Integer
            stringBuilder.Append(json(startIdx))

            For i As Integer = startIdx + 1 To json.Length - 1

                If json(i) = "\"c Then
                    If appendEscapeCharacter Then stringBuilder.Append(json(i))
                    stringBuilder.Append(json(i + 1))
                    i += 1
                ElseIf json(i) = """"c Then
                    stringBuilder.Append(json(i))
                    Return i
                Else
                    stringBuilder.Append(json(i))
                End If
            Next

            Return json.Length - 1
        End Function

        Private Function Split(ByVal json As String) As List(Of String)
            Dim splitArray As List(Of String) = If(splitArrayPool.Count > 0, splitArrayPool.Pop(), New List(Of String)())
            splitArray.Clear()
            If json.Length = 2 Then Return splitArray
            Dim parseDepth As Integer = 0
            stringBuilder.Length = 0

            For i As Integer = 1 To json.Length - 1 - 1

                Select Case json(i)
                    Case "["c, "{"c
                        parseDepth += 1
                    Case "]"c, "}"c
                        parseDepth -= 1
                    Case """"c
                        i = AppendUntilStringEnd(True, i, json)
                        Continue For
                    Case ","c, ":"c

                        If parseDepth = 0 Then
                            splitArray.Add(stringBuilder.ToString())
                            stringBuilder.Length = 0
                            Continue For
                        End If
                End Select

                stringBuilder.Append(json(i))
            Next

            splitArray.Add(stringBuilder.ToString())
            Return splitArray
        End Function

        Friend Function ParseValue(ByVal type As Type, ByVal json As String) As Object
            If type = GetType(String) Then
                If json.Length <= 2 Then Return String.Empty
                Dim parseStringBuilder As StringBuilder = New StringBuilder(json.Length)

                For i As Integer = 1 To json.Length - 1 - 1

                    If json(i) = "\"c AndAlso i + 1 < json.Length - 1 Then
                        Dim j As Integer = """\nrtbf/".IndexOf(json(i + 1))

                        If j >= 0 Then
                            parseStringBuilder.Append(("""\" & vbLf & vbCr & vbTab & vbBack & vbFormFeed & "/")(j))
                            i += 1
                            Continue For
                        End If

                        If json(i + 1) = "u"c AndAlso i + 5 < json.Length - 1 Then
                            Dim c As UInt32 = 0

                            If UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, Nothing, c) Then
                                parseStringBuilder.Append(ChrW(c))
                                i += 5
                                Continue For
                            End If
                        End If
                    End If

                    parseStringBuilder.Append(json(i))
                Next

                Return parseStringBuilder.ToString()
            End If

            If type.IsPrimitive Then
                Dim result = Convert.ChangeType(json, type, System.Globalization.CultureInfo.InvariantCulture)
                Return result
            End If

            If type = GetType(Decimal) Then
                Dim result As Decimal
                Decimal.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, result)
                Return result
            End If

            If json = "null" Then
                Return Nothing
            End If

            If type.IsEnum Then
                If json(0) = """"c Then json = json.Substring(1, json.Length - 2)

                Try
                    Return [Enum].Parse(type, json, False)
                Catch
                    Return 0
                End Try
            End If

            If type.IsArray Then
                Dim arrayType As Type = type.GetElementType()
                If json(0) <> "["c OrElse json(json.Length - 1) <> "]"c Then Return Nothing
                Dim elems As List(Of String) = Split(json)
                Dim newArray As Array = Array.CreateInstance(arrayType, elems.Count)

                For i As Integer = 0 To elems.Count - 1
                    newArray.SetValue(ParseValue(arrayType, elems(i)), i)
                Next

                splitArrayPool.Push(elems)
                Return newArray
            End If

            If type.IsGenericType AndAlso type.GetGenericTypeDefinition() = GetType(List(Of)) Then
                Dim listType As Type = type.GetGenericArguments()(0)
                If json(0) <> "["c OrElse json(json.Length - 1) <> "]"c Then Return Nothing
                Dim elems As List(Of String) = Split(json)
                Dim list = CType(type.GetConstructor(New Type() {GetType(Integer)}).Invoke(New Object() {elems.Count}), IList)

                For i As Integer = 0 To elems.Count - 1
                    list.Add(ParseValue(listType, elems(i)))
                Next

                splitArrayPool.Push(elems)
                Return list
            End If

            If type.IsGenericType AndAlso type.GetGenericTypeDefinition() = GetType(Dictionary(Of,)) Then
                Dim keyType, valueType As Type

                Dim args As Type() = type.GetGenericArguments()
                keyType = args(0)
                valueType = args(1)

                If keyType <> GetType(String) And keyType <> GetType(Integer) Then Return Nothing
                If json(0) <> "{"c OrElse json(json.Length - 1) <> "}"c Then Return Nothing
                Dim elems As List(Of String) = Split(json)
                If elems.Count Mod 2 <> 0 Then Return Nothing
                Dim dictionary = CType(type.GetConstructor(New Type() {GetType(Integer)}).Invoke(New Object() {CInt(elems.Count / 2)}), IDictionary)

                For i As Integer = 0 To elems.Count - 1 Step 2
                    If elems(i).Length <= 2 Then Continue For
                    Dim keyValue As String = elems(i).Substring(1, elems(i).Length - 2)
                    Dim val As Object = ParseValue(valueType, elems(i + 1))

                    If keyType = GetType(String) Then
                        dictionary(keyValue) = val
                    ElseIf keyType = GetType(Integer) Then
                        dictionary(CInt(keyValue)) = val
                    End If

                Next

                Return dictionary
            End If

            If type = GetType(Object) Then
                Return ParseAnonymousValue(json)
            End If

            If json(0) = "{"c AndAlso json(json.Length - 1) = "}"c Then
                Return ParseObject(type, json)
            End If

            Return Nothing
        End Function

        Private Function ParseAnonymousValue(ByVal json As String) As Object
            If json.Length = 0 Then Return Nothing

            If json(0) = "{"c AndAlso json(json.Length - 1) = "}"c Then
                Dim elems As List(Of String) = Split(json)
                If elems.Count Mod 2 <> 0 Then Return Nothing
                Dim dict = New Dictionary(Of String, Object)(elems.Count / 2)

                For i As Integer = 0 To elems.Count - 1 Step 2
                    dict(elems(i).Substring(1, elems(i).Length - 2)) = ParseAnonymousValue(elems(i + 1))
                Next

                Return dict
            End If

            If json(0) = "["c AndAlso json(json.Length - 1) = "]"c Then
                Dim items As List(Of String) = Split(json)
                Dim finalList = New List(Of Object)(items.Count)

                For i As Integer = 0 To items.Count - 1
                    finalList.Add(ParseAnonymousValue(items(i)))
                Next

                Return finalList
            End If

            If json(0) = """"c AndAlso json(json.Length - 1) = """"c Then
                Dim str As String = json.Substring(1, json.Length - 2)
                Return str.Replace("\", String.Empty)
            End If

            If Char.IsDigit(json(0)) OrElse json(0) = "-"c Then

                If json.Contains(".") Then
                    Dim result As Double
                    Double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, result)
                    Return result
                Else
                    Dim result As Integer
                    Integer.TryParse(json, result)
                    Return result
                End If
            End If

            If json = "true" Then Return True
            If json = "false" Then Return False
            Return Nothing
        End Function

        Private Function CreateMemberNameDictionary(Of T As MemberInfo)(ByVal members As T()) As Dictionary(Of String, T)
            Dim nameToMember As Dictionary(Of String, T) = New Dictionary(Of String, T)(StringComparer.OrdinalIgnoreCase)

            For i As Integer = 0 To members.Length - 1
                Dim member As T = members(i)
                If member.IsDefined(GetType(IgnoreDataMemberAttribute), True) Then Continue For
                Dim name As String = member.Name

                If member.IsDefined(GetType(DataMemberAttribute), True) Then
                    Dim dataMemberAttribute As DataMemberAttribute = CType(Attribute.GetCustomAttribute(member, GetType(DataMemberAttribute), True), DataMemberAttribute)
                    If Not String.IsNullOrEmpty(dataMemberAttribute.Name) Then name = dataMemberAttribute.Name
                End If

                nameToMember.Add(name, member)
            Next

            Return nameToMember
        End Function

        Private Function ParseObject(ByVal type As Type, ByVal json As String) As Object
            Dim instance As Object = FormatterServices.GetUninitializedObject(type)
            Dim elems As List(Of String) = Split(json)
            If Not compact_mode AndAlso elems.Count Mod 2 <> 0 Then Return instance
            Dim nameToField As Dictionary(Of String, FieldInfo) = Nothing
            Dim nameToProperty As Dictionary(Of String, PropertyInfo) = Nothing

            If Not fieldInfoCache.TryGetValue(type, nameToField) Then
                Dim t1 = type.GetFields(BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.FlattenHierarchy)
                fieldInfo_Order.Add(type, t1.ToList)

                nameToField = CreateMemberNameDictionary(t1)
                fieldInfoCache.Add(type, nameToField)
            End If

            If Not propertyInfoCache.TryGetValue(type, nameToProperty) Then
                Dim t2 = type.GetProperties(BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.FlattenHierarchy)
                propertyInfo_Order.Add(type, t2.ToList)

                nameToProperty = CreateMemberNameDictionary(t2)
                propertyInfoCache.Add(type, nameToProperty)
            End If

            If compact_mode Then
                If nameToField.Count + nameToProperty.Count <> elems.Count Then Return instance
                For i As Integer = 0 To fieldInfo_Order(type).Count - 1
                    Dim value As String = elems(i)
                    Dim fieldInfo As FieldInfo = fieldInfo_Order(type)(i)
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value))
                Next
                For i As Integer = fieldInfo_Order(type).Count To propertyInfo_Order(type).Count - 1
                    Dim value As String = elems(i)
                    Dim propertyInfo As PropertyInfo = propertyInfo_Order(type)(i)
                    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), Nothing)
                Next
            Else
                For i As Integer = 0 To elems.Count - 1 Step 2
                    If elems(i).Length <= 2 Then Continue For
                    Dim key As String = elems(i).Substring(1, elems(i).Length - 2)
                    Dim value As String = elems(i + 1)
                    Dim fieldInfo As FieldInfo = Nothing
                    Dim propertyInfo As PropertyInfo = Nothing

                    If nameToField.TryGetValue(key, fieldInfo) Then
                        fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value))
                    ElseIf nameToProperty.TryGetValue(key, propertyInfo) Then
                        propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), Nothing)
                    End If
                Next
            End If

            Return instance
        End Function
    End Module
End Namespace
