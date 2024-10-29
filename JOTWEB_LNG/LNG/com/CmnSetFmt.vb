Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' 型変換、フォーマット設定クラス
''' </summary>
''' <remarks>
''' </remarks>

Public Class CmnSetFmt

#Region "型変換"
    ''' <summary>
    ''' 型変換(object→string)
    ''' </summary>
    ''' <param name="oPrm">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「String」に変換し、返却する。「変換対象」が「Nothing」「DB Null」の場合、「空文字」を設定する。</remarks>
    Public Overloads Shared Function Nz(ByVal oPrm As Object) As String
        Return Nz(oPrm, "")
    End Function

    ''' <summary>
    ''' 規定値あり型変換(object→string)
    ''' </summary>
    ''' <param name="oPrm">変換対象</param>
    ''' <param name="sRet">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「String」に変換し、返却する。「変換対象」が「Nothing」「DB Null」の場合、「規定値」を設定する。</remarks>
    Public Overloads Shared Function Nz(ByVal oPrm As Object, ByVal sRet As String) As String
        If oPrm Is Nothing OrElse IsDBNull(oPrm) Then
            Return sRet
        End If
        Return oPrm.ToString
    End Function

    ''' <summary>
    ''' NOTHINGハンドリング
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <param name="defaultValue">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「String」に変換し、返却する。「変換対象」が「Nothing」の場合、「規定値」を設定する。</remarks>
    Public Shared Function NothingIf(ByVal value As Object, ByVal defaultValue As String) As String
        If IsNothing(value) = True Then
            Return defaultValue
        End If
        Return CStr(value)
    End Function

    ''' <summary>
    ''' DB NULLハンドリング(string)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <param name="defaultValue">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「String」に変換し、返却する。「変換対象」が「DB NULL」の場合、「規定値」を設定する。</remarks>
    Public Shared Function DbNullIf(ByVal value As Object, ByVal defaultValue As String) As String
        If IsDBNull(value) = True Then
            Return defaultValue
        End If
        Return CStr(value)
    End Function

    ''' <summary>
    ''' DB NULLハンドリング(integer)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <param name="defaultValue">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「integer」に変換し、返却する。「変換対象」が「DB NULL」の場合、「規定値」を設定する。</remarks>
    Public Shared Function DbNullIf(ByVal value As Object, ByVal defaultValue As Integer) As Integer
        If IsDBNull(value) = True Then
            Return defaultValue
        End If
        Return CInt(value)
    End Function

    ''' <summary>
    ''' DB NULLハンドリング(Decimal)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <param name="defaultValue">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「Decimal」に変換し、返却する。「変換対象」が「DB NULL」の場合、「規定値」を設定する。</remarks>
    Public Shared Function DbNullIf(ByVal value As Object, ByVal defaultValue As Decimal) As Decimal
        If IsDBNull(value) = True Then
            Return defaultValue
        End If
        Return CDec(value)
    End Function

    ''' <summary>
    ''' 型変換(string → Decimal)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「Decimal」に変換し、返却する。「変換対象」を「Decimal」に変換できない場合は、「0」を返却する。</remarks>
    Public Shared Function StrToDec(ByVal value As String) As Decimal
        Dim decRet As Decimal = 0D
        Try
            Decimal.TryParse(value, decRet)
        Catch ex As Exception
        End Try
        Return decRet
    End Function

    ''' <summary>
    ''' 型変換(Object → Integer)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「Integer」に変換し、返却する。「変換対象」を「Integer」に変換できない場合は、「0」を返却する。</remarks>
    Public Shared Function ObjToInt(ByVal value As Object) As Integer
        Dim iRet As Integer = 0
        Try
            Integer.TryParse(value.ToString, iRet)
        Catch ex As Exception
        End Try
        Return iRet
    End Function

    ''' <summary>
    ''' 型変換(Object → Decimal)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「Decimal」に変換し、返却する。「変換対象」を「Decimal」に変換できない場合は、「0」を返却する。</remarks>
    Public Overloads Shared Function ObjToDec(ByVal value As Object) As Decimal
        Return ObjToDec(value, 0D)
    End Function

    ''' <summary>
    ''' 規定値あり型変換(Object → Decimal)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <param name="decRet">規定値</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「Decimal」に変換し、返却する。「変換対象」を「Decimal」に変換できない場合は、「規定値」を返却する。</remarks>
    Public Overloads Shared Function ObjToDec(ByVal value As Object, ByVal decRet As Decimal) As Decimal
        Try
            If IsNumeric(value) Then
                Decimal.TryParse(value.ToString, decRet)
            End If
        Catch ex As Exception
        End Try
        Return decRet
    End Function

    ''' <summary>
    ''' 型変換(Object → DateTime)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「DateTime」に変換し、返却する。「変換対象」を「DateTime」に変換できない場合は、「Nothing」を返却する。</remarks>
    Public Shared Function ObjToDate(ByVal value As Object) As DateTime
        Dim ret As DateTime = Nothing
        Try
            If DateTime.TryParse(value.ToString, ret) Then
                Return ret
            End If
        Catch ex As Exception
        End Try
        Return ret
    End Function

    ''' <summary>
    ''' 型変換(Object → DBNULL)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>引き渡された値を「string」型に変換し、変換後の値が「ブランク」の場合、「DBNULL」を返却する。</remarks>
    Public Shared Function ObjToDbNull(ByVal value As Object) As Object
        Dim ret As Object

        If CStr(value) = "" Then
            ret = System.DBNull.Value
        Else
            ret = value
        End If

        Return ret
    End Function

    ''' <summary>
    ''' 空文字ハンドリング
    ''' </summary>
    ''' <param name="strPara">検査対象</param>
    ''' <param name="strRet">規定値</param>
    ''' <returns>ハンドリング結果</returns>
    ''' <remarks>「検査値」が「ブランク」の場合、「規定値」に指定された値を返却する。</remarks>
    Public Shared Function StrCng(ByVal strPara As String, ByVal strRet As String) As String
        If strPara = "" Then
            StrCng = strRet
        Else
            StrCng = strPara
        End If
    End Function

    ''' <summary>
    ''' ディクショナリー取得
    ''' </summary>
    ''' <param name="dt">データテーブル</param>
    ''' <returns>ディクショナリー型に変換したテーブルデータ(1行目のみ)</returns>
    ''' <remarks>データテーブルをディクショナリー型に変換する。</remarks>
    Public Shared Function GetDictionaly(ByVal dt As DataTable) As Dictionary(Of String, String)
        Dim dic As New Dictionary(Of String, String)

        If dt.Rows.Count <> 0 Then
            Dim row As DataRow = dt(0)
            For Each col As DataColumn In dt.Columns
                dic.Add(col.ColumnName, Nz(row(col.ColumnName)))
            Next
        End If

        Return dic
    End Function

    ''' <summary>
    ''' 型変換(DateTime→string(YYYYMMDDHHMMSS))
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「フォーマット対象」を「YYYYMMDDHHMMSS」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function YYYYMMDDHHMMSSToStr(ByVal value As DateTime) As String
        Return value.ToString("G")
    End Function

    ''' <summary>
    ''' 型変換(Object→string(YYYY/MM/DD))
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「変換対象」を「YYYY/MM/DD」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function YYYYMMDDToStr(ByVal value As Object) As String

        If Nz(value) = "" Then
            Return ""
        Else
            Return Format(value, "yyyy/MM/dd")
        End If

    End Function

    ''' <summary>
    ''' 型変換(Object→string(YYYY-MM-DD))
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「変換対象」を「YYYY-MM-DD」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function YYYYMMDDToStrHai(ByVal value As Object) As String

        If Nz(value) = "" Then
            Return ""
        Else
            Return Format(value, "yyyy-MM-dd")
        End If

    End Function

    ''' <summary>
    ''' 型変換(DateTime→string(YYYYMM))
    ''' </summary>
    ''' <param name="value">対象日付</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「変換対象」を「YYYY/MM」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function YYYYMMToStr(ByVal value As Object) As String
        Return Format(value, "yyyyMM")
    End Function

    ''' <summary>
    ''' 型変換(DateTime→string(YYYY/MM))
    ''' </summary>
    ''' <param name="value">対象日付</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「変換対象」を「YYYY/MM」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function YYYYMMSlashToStr(ByVal value As Object) As String
        Return Format(value, "yyyy/MM")
    End Function

#End Region

#Region "カンマ編集"

    ''' <summary>
    ''' カンマ編集有り(小数点以下なし)
    ''' </summary>
    ''' <param name="str">編集対象</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Overloads Shared Function FormatCommaOn(ByVal str As String) As String
        Return FormatComma(str, 0, True)
    End Function

    ''' <summary>
    ''' カンマ編集(小数点以下なし)
    ''' </summary>
    ''' <param name="str">編集対象</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Overloads Shared Function FormatComma(ByVal str As String) As String
        Return FormatComma(str, 0, False)
    End Function

    ''' <summary>
    ''' カンマ編集(小数点以下ゼロフォーマット)
    ''' </summary>
    ''' <param name="str">編集対象</param>
    ''' <param name="decpoint">小数点以下桁数（省略可）初期値0</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Overloads Shared Function FormatComma(ByVal str As String,
                                                 ByVal decPoint As Integer) As String
        Return FormatComma(str, decPoint, False)
    End Function
    ''' <summary>
    ''' カンマ編集(小数点以下ゼロフォーマット)
    ''' </summary>
    ''' <param name="str">編集対象</param>
    ''' <param name="commaFlg">TRUE：カンマ編集する、FALSE：カンマ編集しない</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Overloads Shared Function FormatComma(ByVal str As String,
                                                 ByVal commaFlg As Boolean) As String
        Return FormatComma(str, 0, commaFlg)
    End Function
    ''' <summary>
    ''' カンマ編集(小数点以下ゼロフォーマット)
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <param name="decpoint">小数点以下桁数（省略可）初期値0</param>
    ''' <param name="commaFlg">TRUE：カンマ編集する、FALSE：カンマ編集しない</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Overloads Shared Function FormatComma(ByVal str As String,
                                                 ByVal decpoint As Integer,
                                                 ByVal commaFlg As Boolean) As String
        Dim tmpZero As String = "."
        If Len(Trim(str)) = 0 Then Return str
        For i As Integer = 1 To decpoint
            tmpZero += "0"
        Next
        If tmpZero = "." Then tmpZero = ""
        If commaFlg Then
            Return Double.Parse(str).ToString("#,0" + tmpZero)
        Else
            Return Double.Parse(str).ToString("0" + tmpZero)
        End If
    End Function

    ''' <summary>
    ''' カンマ削除
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>カンマ取り除き後の値</returns>
    ''' <remarks>文字列からカンマを削除する。</remarks>
    Public Shared Function RemoveComma(ByVal str As String) As String
        Return str.Replace(",", "")
    End Function

    ''' <summary>
    ''' 通貨用　カンマ編集(小数点以下ゼロフォーマット)
    ''' </summary>
    ''' <param name="str">編集対象</param>
    ''' <returns>カンマ編集後の値</returns>
    ''' <remarks>文字列をカンマ編集、および小数点以下をゼロフォーマットする。</remarks>
    Public Shared Function FormatCurrency(ByVal str As String) As String
        If Trim(str) = "" Then
            Return ""
        Else
            Return "¥" & FormatComma(str, 0, True)
        End If

    End Function

#End Region

#Region "日付編集"

    ''' <summary>
    ''' スラッシュ編集(年月日)
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>年月日をスラッシュ編集する。</remarks>
    Public Shared Function FormatDateSlash(ByVal str As String) As String
        Dim tmpStr As String = str
        Select Case True
            Case New Regex("\d{4}[/]\d{1,2}[/]\d{1,2}$").IsMatch(tmpStr)
                ' スラッシュ編集された、年付きの文字列は、日付として妥当なら、日付フォーマットして返す
                If IsDate(tmpStr) Then
                    tmpStr = Date.Parse(tmpStr).ToString("yyyy/MM/dd")
                End If
            Case New Regex("\d{1,2}[/]\d{1,2}$").IsMatch(tmpStr)
                ' スラッシュ編集された、年無しの文字列は、今年の日付として妥当なら、日付フォーマットして返す
                Dim thisYear As String = Year(Now) & "/" & tmpStr
                If IsDate(thisYear) Then
                    tmpStr = Date.Parse(thisYear).ToString("yyyy/MM/dd")
                End If
            Case New Regex("\d{8}$").IsMatch(tmpStr)
                ' スラッシュ編集されていない8桁の数字は、スラッシュ編集をして返す
                Dim slashed As String = Int64.Parse(tmpStr).ToString("0000/00/00")
                If IsDate(slashed) Then
                    tmpStr = slashed
                End If
            Case New Regex("^\d{4}$").IsMatch(tmpStr)
                ' スラッシュ編集されていない4桁の数字は、今年として妥当なら、日付フォーマットして返す。

                Dim slashed As String = Year(Now) & Int64.Parse(tmpStr).ToString("/00/00")
                If IsDate(slashed) Then
                    tmpStr = slashed
                End If
            Case Else
        End Select

        Return tmpStr
    End Function

    ''' <summary>
    ''' スラッシュ編集(年月)
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>年月をスラッシュ編集する。</remarks>
    Public Shared Function FormatDateSlashYYYYMM(ByVal str As String) As String
        Dim tmpStr As String = str
        '文字列（月）のゼロフォーマット
        Try
            Dim splStr() As String = tmpStr.Split("/"c)
            If splStr.Length = 2 Then
                If (splStr(0).Length = 4) And (splStr(1).Length = 1) Then
                    tmpStr = splStr(0) + "/0" + splStr(1)
                End If
            End If

            tmpStr = FormatDeleteDateSlash(tmpStr)
            If tmpStr.Trim = "" Then
                Return tmpStr
            End If
            str = Format(CInt(tmpStr), "0000/00")
        Catch ex As Exception
        End Try

        Return str

    End Function

    ''' <summary>
    ''' スラッシュ削除
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>スラッシュ取り除き後の値</returns>
    ''' <remarks>年月日のスラッシュを削除する。</remarks>
    Public Shared Function FormatDeleteDateSlash(ByVal str As String) As String
        Return str.Replace("/", "")
    End Function

    ''' <summary>
    ''' 日付時刻編集(YYYY/MM/DD HH:MM:SS)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「フォーマット対象」を「YYYY/MM/DD HH:MM:SS」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function FormatYYYYMMDDHHMMSS(ByVal value As String) As String
        Dim ret As String = ""

        Try
            ret = DateTime.Parse(value).ToString("yyyy/MM/dd HH:mm:ss")
        Catch ex As Exception
        End Try

        Return ret
    End Function

    ''' <summary>
    ''' 日付編集(YYYY/MM/DD)
    ''' </summary>
    ''' <param name="value">変換対象</param>
    ''' <returns>日付フォーマット後の値</returns>
    ''' <remarks>「フォーマット対象」を「YYYY/MM/DD HH:MM:SS」形式のstring型に変換した値を返却する。</remarks>
    Public Shared Function FormatYYYYMMDD(ByVal value As String) As String
        Dim ret As String = ""

        Try
            ret = DateTime.Parse(value).ToString("yyyy/MM/dd")
        Catch ex As Exception
        End Try

        Return ret
    End Function

#End Region

#Region "文字列編集"

    ''' <summary>
    ''' ブランク0変換
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>ブランクを0に変換する。</remarks>
    Public Shared Function FormatBrankToZero(ByVal str As String) As String
        If Trim(str) = "" Then
            Return "0"
        End If
        Return str
    End Function

    ''' <summary>
    ''' SQL部分一致文字列変換
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>%で囲った引数の文字列</returns>
    ''' <remarks></remarks>
    Public Shared Function PartialMatch(ByVal str As String) As String
        Return "%" & EscSQLSpCharForLIKE(str) & "%"
    End Function

    ''' <summary>
    ''' SQL前方一致文字列変換
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>%で囲った引数の文字列</returns>
    ''' <remarks>SQLのLIKE文にて「前方一致」となる様文字列の変換を行う。</remarks>
    Public Shared Function BeginWithMatch(ByVal str As String) As String
        Return EscSQLSpCharForLIKE(str) & "%"
    End Function

    ''' <summary>
    ''' SQL後方一致文字列変換
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>%で囲った引数の文字列</returns>
    ''' <remarks>SQLのLIKE文にて「後方一致」となる様文字列の変換を行う。</remarks>
    Public Shared Function EndsWithMatch(ByVal str As String) As String
        Return "%" & EscSQLSpCharForLIKE(str)
    End Function

    ''' <summary>
    ''' ハイフン削除処理
    ''' </summary>
    ''' <param name="str">対象文字列</param>
    ''' <returns>引数の文字列からハイフンを削除した値</returns>
    ''' <remarks>文字列からハイフンを削除する。</remarks>
    Public Shared Function RemoveHyphen(ByVal str As String) As String
        Return CStr(IIf(Len(Trim(str)) = 0, "", str.Replace("-", "")))
    End Function

    ''' <summary>
    ''' 左文字埋め
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="length">桁数</param>
    ''' <param name="paddingChar">埋める文字</param>
    ''' <returns>フォーマット結果</returns>
    ''' <remarks>左側を指定桁数分、指定文字で埋める。</remarks>
    Public Shared Function LPad(ByVal value As String,
                                ByVal length As Integer,
                                ByVal paddingChar As Char) As String
        If Len(Trim(value)) = 0 Then
            Return value
        End If
        Return value.PadLeft(length, paddingChar)
    End Function

    ''' <summary>
    ''' 右文字埋め
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="length">桁数</param>
    ''' <param name="paddingChar">埋める文字</param>
    ''' <returns>フォーマット結果</returns>
    ''' <remarks>右側を指定桁数分、指定文字で埋める。</remarks>
    Public Shared Function RPad(ByVal value As String,
                                ByVal length As Integer,
                                ByVal paddingChar As Char) As String

        If Len(Trim(value)) = 0 Then
            Return value
        End If

        Return value.PadRight(length, paddingChar)

    End Function

    ''' <summary>
    ''' バイト数取得(文字コード：Shift_JIS)
    ''' </summary>
    ''' <param name="value">バイト数取得文字列</param>
    ''' <returns>バイト数</returns>
    ''' <remarks>引き渡された値を元に、バイト数を「文字コード：Shift_JIS」にて算出し返却する。</remarks>
    Public Shared Function LenB(ByVal value As String) As Integer
        Return Encoding.GetEncoding("Shift_JIS").GetByteCount(value)
    End Function

    ''' <summary>
    ''' バイト文字列取得(左側)
    ''' </summary>
    ''' <param name="strPara">取得対象</param>
    ''' <param name="lngPara">バイト数指定</param>
    ''' <returns>引数で指定されたバイト数までの文字列</returns>
    ''' <remarks>文字列の左側から指定のバイト数文取得し返す。</remarks>
    Public Shared Function LeftBB(ByVal strPara As String, ByVal lngPara As Integer) As String
        Dim hEnc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
        Dim btBytes As Byte() = hEnc.GetBytes(strPara)

        If btBytes.Length >= lngPara Then
            Return hEnc.GetString(btBytes, 0, lngPara)
        Else
            Return strPara
        End If
    End Function

    ''' <summary>
    ''' バイト文字列取得(右側)
    ''' </summary>
    ''' <param name="strPara">取得対象</param>
    ''' <param name="lngPara">バイト数指定</param>
    ''' <returns>引数で指定されたバイト数までの文字列</returns>
    ''' <remarks>文字列の右側から指定のバイト数文取得し返す。</remarks>
    Public Shared Function RightBB(ByVal strPara As String, ByVal lngPara As Integer) As String
        Dim hEnc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
        Dim btBytes As Byte() = hEnc.GetBytes(strPara)

        If btBytes.Length >= lngPara Then
            Return hEnc.GetString(btBytes, btBytes.Length - lngPara, lngPara)
        Else
            Return strPara
        End If
    End Function

    ''' <summary>
    ''' バイト文字列取得(FromTo)
    ''' </summary>
    ''' <param name="strPara">取得対象</param>
    ''' <param name="lngPara1">バイト数From指定</param>
    ''' <param name="lngPara2">バイト数To指定</param>
    ''' <returns>引数で指定されたバイト数の範囲の文字列を返す。</returns>
    ''' <remarks></remarks>
    Public Shared Function MidBB(ByVal strPara As String, ByVal lngPara1 As Integer, ByVal lngPara2 As Integer) As String
        Dim hEncoding As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
        Dim btBytes As Byte() = hEncoding.GetBytes(strPara)
        Dim sumlngPart As Integer = lngPara1 + lngPara2

        If btBytes.Length >= sumlngPart Then
            Return hEncoding.GetString(btBytes, lngPara1 - 1, lngPara2)
        ElseIf btBytes.Length >= lngPara1 And btBytes.Length < sumlngPart Then
            Return hEncoding.GetString(btBytes, lngPara1 - 1, btBytes.Length - lngPara1 + 1)
        Else
            Return ""
        End If
    End Function

    ''' <summary>
    ''' SQL(LIKE句)特殊文字エスケープ
    ''' </summary>
    ''' <param name="pStr">エスケープ対象</param>
    ''' <returns>SQL特殊文字エスケープ後の文字列</returns>
    ''' <remarks>SQL(LIKE句)での特殊文字をエスケープする。</remarks>
    Public Shared Function EscSQLSpCharForLIKE(ByVal pStr As String) As String
        If True = IsNothing(pStr) Then
            Return Nothing
        End If
        Return pStr.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")
    End Function

    ''' <summary>
    ''' 改行コード変換
    ''' </summary>
    ''' <param name="pStr">変換対象</param>
    ''' <returns>変換結果</returns>
    ''' <remarks>文字列中に含まれる改行コードをブランクに変換する。</remarks>
    Public Shared Function LineFeedToBlank(ByVal pStr As String) As String

        If Not pStr Is Nothing Then
            pStr = pStr.Replace(vbCrLf, "")
        End If

        Return pStr

    End Function

    ''' <summary>
    ''' 文字列型日付書式フォーマット
    ''' </summary>
    ''' <param name="val"></param>
    ''' <param name="fmt"></param>
    ''' <returns></returns>
    Public Shared Function FormatDate(ByVal val As String, ByVal fmt As String) As String
        Dim dtVal As DateTime = ObjToDate(val)
        If IsNothing(dtVal) OrElse Len(Trim(val)) = 0 Then
            Return ""
        End If
        Return dtVal.ToString(fmt)
    End Function
#End Region

End Class
