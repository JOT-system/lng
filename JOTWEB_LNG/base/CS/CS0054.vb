'Option Strict On
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.Serialization.Json
Imports Newtonsoft.Json.Linq
Imports MySql.Data.MySqlClient


''' <summary>
''' Kintoneの管理データを取得するAPI共通クラス
''' </summary>
''' <remarks>基本的に例外はThrowするので呼出し元で制御</remarks>
Public Class CS0054KintoneApi
    ''' <summary>
    ''' APIトークン
    ''' </summary>
    ''' <returns></returns>
    Public Property ApiToken As String = ""
    ''' <summary>
    ''' アプリID
    ''' </summary>
    ''' <returns></returns>
    Public Property ApiApplId As String = ""
    ''' <summary>
    ''' 取引先（荷主）コード
    ''' </summary>
    ''' <returns></returns>
    Public Property ToriCode As String = ""
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <returns></returns>
    Public Property OrgCode As String = ""
    ''' <summary>
    ''' 開始日付
    ''' </summary>
    ''' <returns></returns>
    Public Property YmdFrom As String = ""
    ''' <summary>
    ''' 終了日付
    ''' </summary>
    ''' <returns></returns>
    Public Property YmdTo As String = ""
    ''' <summary>
    ''' APIを実行する元となるURLを設定
    ''' </summary>
    ''' <returns></returns>
    Private Property ApiBaseUrl As String = ""
    ''' <summary>
    ''' ベーシック認証のパスワード
    ''' </summary>
    ''' <returns></returns>
    Private Property ApiBasicPass As String = ""

    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New()
        Me.ApiBaseUrl = ”https://jot.cybozu.com/k/v1/records.json"
        Me.ApiBasicPass = "Basic am90dXNlcjppc3VzZXI="
        Net.ServicePointManager.SecurityProtocol = Net.ServicePointManager.SecurityProtocol Or System.Net.SecurityProtocolType.Tls12
    End Sub

    ''' <summary>
    ''' 複数レコード取得(アプリID)
    ''' </summary>
    ''' <returns>取得データをdatatbleに格納し返却する</returns>
    ''' <remarks>エラー時はスローするので呼出し側で制御</remarks>
    Public Function GetRecords() As DataTable

        '返却用テーブル
        Dim outTbl As DataTable = New DataTable

        '返却用テーブル（カラム）作成
        'KintoneAPIの返却項目と同じ項目名（同じプロパティ）で作成
        CreateDataTable(outTbl)

        '------------------------------------------------------------------------------------------------------------------------------
        'kintone REST API
        '1万件を超えないことが前提でデータ取得を行う
        '但し、1度に取得できる制限が500件までのため、
        'ループながらoffset（※１)をアップしながら1万件までリクエストする
        '※１取得をスキップするレコード数
        '　　たとえばoffset 30を指定すると、レコード先頭から30番目までのレコードは取得せず、31番目のレコードから取得します。）
        '
        'もし、1万件を超える場合があるなら「カーソルAPI」を利用する必要がある（ソースコードを書き換える必要あり）
        '------------------------------------------------------------------------------------------------------------------------------

        ' アプリID：app = xxx
        ' 抽出条件：Query = xxx
        ' 例）app=1&Query = created_datetime > "2024-01-01"
        Dim GetApplId As String = "app=" & ApiApplId
        Dim GetLimit As Integer = 500
        Dim GetOffset As Integer = 0
        Dim GetFields As String = ""
        Dim GetTotalCnt As String = "totalCount=true"
        Dim GetUrl As String = ""
        'Dim repTori As String = ToriCode.Replace(",", """,""")
        Dim splitTori() As String = ToriCode.Split(",")

        If String.IsNullOrEmpty(YmdFrom) Then
            YmdFrom = Date.Now.ToString("yyyy-MM-dd")
        Else
            YmdFrom = YmdFrom.Replace("/", "-")
        End If
        If String.IsNullOrEmpty(YmdTo) Then
            YmdTo = Date.Now.ToString("yyyy-MM-dd")
        Else
            YmdTo = YmdTo.Replace("/", "-")
        End If

        While GetOffset < 10000
            'Dim EditQuery As String = "query= 品名1コード = ""21"" and 届先取引先コード in (""{0}"") and 届日 >= ""{1}"" and 届日 <= ""{2}"" and 実績数量 != ""0"" limit " & GetLimit & " offset " & GetOffset
            Dim EditQuery As String = "query= 品名1コード = ""21"" "
            EditQuery += " and 届日 >= ""{0}"" "
            EditQuery += " and 届日 <= ""{1}"" "
            EditQuery += " and 実績数量 != ""0"" limit " & GetLimit & " offset " & GetOffset
            Dim GetQuery As String = String.Format(EditQuery, YmdFrom, YmdTo)
            'Dim GetQuery As String = "query= limit " & GetLimit & " offset " & GetOffset

            GetUrl = Me.ApiBaseUrl + "?"
            GetUrl += GetApplId
            GetUrl += IIf(String.IsNullOrEmpty(GetQuery), "", "&" & GetQuery).ToString
            GetUrl += IIf(String.IsNullOrEmpty(GetTotalCnt), "", "&" & GetTotalCnt).ToString
            GetUrl += IIf(String.IsNullOrEmpty(GetFields), "", "&" & GetFields).ToString

            Dim request As HttpWebRequest = CType(WebRequest.Create(GetUrl), HttpWebRequest)
            'メソッドにGETを指定（KintoneAPIの仕様）
            request.Method = "GET"
            'リクエストヘッダー
            request.Headers.Add("X-Cybozu-API-Token", ApiToken)
            'Basic認証
            request.Headers.Add("Authorization", ApiBasicPass)

            Try
                'Kintoneサーバーへリクエスト
                Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                    Using reader As New StreamReader(response.GetResponseStream(), Encoding.UTF8)
                        Dim responseText As String = reader.ReadToEnd()
                        ' Kintone（アボカド）から取得したデータ（JSON形式）をリストにデシリアライズ（項目毎に取り出す）
                        Dim avovadList As ApiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(Of ApiResponse)(responseText)
                        '取得が0件の場合、処理終了
                        If avovadList.Records.Count = 0 Then Exit While

                        ' １レコード毎に編集する
                        For Each record As AvocadoRec In avovadList.Records
                            Dim dr As DataRow = outTbl.NewRow()

                            ' リフレクションを使用して階層型プロパティの値を動的に取得
                            Dim properties As Reflection.PropertyInfo() = record.GetType().GetProperties()
                            '項目毎にプロパティより値を取り出す（属性に合わせて取り出す
                            '★　2024/11/22現在は次の４パターンのみ）
                            For Each prop As Reflection.PropertyInfo In properties
                                Dim propInfo As Object = prop.GetValue(record)
                                If propInfo Is Nothing Then
                                    '項目不一致の場合（アボカドに必要項目が存在しない場合）、東京ガス専用の項目ならば空白を設定して次の項目へ
                                    Dim TGSflg As Boolean = False
                                    Dim TGSitemProp = GetType(TGSItem).GetProperties()
                                    For Each TGSprop In TGSitemProp
                                        If prop.Name = TGSprop.Name Then
                                            TGSflg = True
                                            dr(prop.Name) = ""
                                            Exit For
                                        End If
                                    Next
                                    If TGSflg Then
                                        Continue For
                                    Else
                                        Throw New Exception("KintoneAPIエラー（項目不一致）: " & prop.Name & ",部署：" & OrgCode & ",アプリID:" & ApiApplId)
                                    End If
                                End If

                                dr(prop.Name) = ""
                                Select Case prop.PropertyType.Name
                                    Case "NormalStruct"
                                        Dim propRec As Object = prop.GetValue(record)
                                        If propRec IsNot Nothing Then
                                            dr(prop.Name) = propRec.value
                                        End If
                                        '日付形式ならば、形式変換
                                        dr(prop.Name) = ConvertToDate(dr(prop.Name), propInfo.type)
                                    Case "ValListStruct"
                                        Dim valueProp = GetNestedPropertyValue(record, prop.Name + ".value[0]")
                                        If valueProp IsNot Nothing Then
                                            dr(prop.Name) = valueProp
                                        End If
                                        '日付形式ならば、形式変換
                                        dr(prop.Name) = ConvertToDate(dr(prop.Name), propInfo.type)
                                    Case "ValNestStruct"
                                        Dim valueProp = GetNestedPropertyValue(record, prop.Name + ".value.code")
                                        If valueProp IsNot Nothing Then
                                            dr(prop.Name) = valueProp
                                        End If
                                        '日付形式ならば、形式変換
                                        dr(prop.Name) = ConvertToDate(dr(prop.Name), propInfo.type)
                                    Case "ValSelectListStruct"
                                        Dim codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].code")
                                        If codeProp IsNot Nothing Then
                                            dr(prop.Name) = codeProp
                                        End If
                                        '日付形式ならば、形式変換
                                        dr(prop.Name) = ConvertToDate(dr(prop.Name), propInfo.type)

                                        '--------------------------------------------------------
                                        'LNGでは、品名テーブルを取込まない 2025/03/26
                                        '--------------------------------------------------------
                                        'Case "SubTableStruct"
                                        '    '品名テーブル対応
                                        '    Dim additemProp = GetType(AddItemRec).GetProperties()
                                        '    For Each additemP In additemProp
                                        '        Dim ItemName As String = additemP.Name

                                        '        Dim codeProp = GetNestedPropertyValue(record, prop.Name + String.Format(".value[0].value.{0}.value", ItemName))
                                        '        If codeProp IsNot Nothing Then
                                        '            dr(ItemName) = codeProp
                                        '        End If
                                        '    Next
                                        '上記ロジックに変更（下記は、同じ結果）
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名選択_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名選択_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名2コード_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名2コード_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名2名_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名2名_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.油種名_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("油種名_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名詳細_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名詳細_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名1コード_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名1コード_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.数量_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("数量_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.油種コード_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("油種コード_枝番付き") = codeProp
                                        'End If
                                        'codeProp = GetNestedPropertyValue(record, prop.Name + ".value[0].value.品名1名_枝番付き.value")
                                        'If codeProp IsNot Nothing Then
                                        '    dr("品名1名_枝番付き") = codeProp
                                        'End If
                                End Select
                            Next
                            outTbl.Rows.Add(dr)
                        Next
                    End Using
                End Using

            Catch ex As Net.WebException
                '通信出来ていても40x系のエラーはWebExceptionに飛ばされる為ここでトラップ
                Dim responseObj = ex.Response
                'JSON形式ではないサーバー返答のWebExceptionは通信そのものに問題があるのでそのまま上位にスロー
                If Not responseObj.ContentType.Contains("application/json") Then
                    Throw
                End If

                Dim objEncError As Encoding = Encoding.UTF8
                Dim htmlError As String = ""
                Using resStreamError As Stream = responseObj.GetResponseStream()
                    Using srError As StreamReader = New StreamReader(resStreamError, objEncError)
                        htmlError = srError.ReadToEnd()
                    End Using
                End Using
                ' JSON（エラー）をリストにデシリアライズ
                Dim errInfo As ErrInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(Of ErrInfo)(htmlError)

                Dim errorStr As String = "Id=" & errInfo.Id & " Code=" & errInfo.Code & " Message=" & errInfo.Message
                Throw New Exception("KintoneAPIエラー: " & errorStr)
            End Try

            '500件づつカウントアップする。次の500件を取得するためスキップする件数（offset）を設定
            GetOffset += GetLimit

        End While

        Dim selTbl As DataTable = outTbl.Clone
        For Each row As DataRow In outTbl.Rows
            For Each tori In splitTori
                If Mid(tori, 1, 5) = Mid(row("届先取引先コード"), 1, 5) Then
                    Dim dr As DataRow = selTbl.NewRow()
                    dr.ItemArray = row.ItemArray
                    dr("TORICODE_AVOCADO") = row("届先取引先コード")
                    dr("届先取引先コード") = Mid(row("届先取引先コード"), 1, 5) & "00000"
                    selTbl.Rows.Add(dr)
                    Exit For
                End If
            Next
        Next

        Dim outView As New DataView(selTbl)
        outView.Sort = "届日,レコード番号"

        Return outView.ToTable

    End Function
    ''' <summary>
    ''' 階層のクラスプロパティから特定のプロパティ名を指定して値を取り出す
    ''' </summary>
    Function GetNestedPropertyValue(ByVal obj As Object, ByVal basePropertyName As String) As Object
        Dim parts As String() = basePropertyName.Split("."c)
        Dim currentObject As Object = obj

        For Each part As String In parts
            If currentObject Is Nothing Then Return Nothing

            ' インデックスを含むプロパティ名を処理
            '(\w+): 1つ以上の単語文字（アルファベット、数字、アンダースコア）に一致します。
            '\[(\d+)\]: 角括弧内の1つ以上の数字に一致します。
            '例えば、Parents[0]を発見します。
            Dim match = System.Text.RegularExpressions.Regex.Match(part, "(\w+)\[(\d+)\]")
            If match.Success Then
                Dim propName As String = match.Groups(1).Value
                Dim index As Integer = Integer.Parse(match.Groups(2).Value)
                Dim propInfo As PropertyInfo = currentObject.GetType().GetProperty(propName)
                If propInfo Is Nothing Then Return Nothing
                Dim list As IList = CType(propInfo.GetValue(currentObject, Nothing), IList)
                If list.Count = 0 Then
                    currentObject = Nothing
                Else
                    currentObject = list(index)
                End If
            Else
                Dim propInfo As PropertyInfo = currentObject.GetType().GetProperty(part)
                If propInfo Is Nothing Then Return Nothing
                currentObject = propInfo.GetValue(currentObject, Nothing)
            End If
        Next

        Return currentObject
    End Function

    ''' <summary>
    ''' 日付だけの場合と日付と時間を区別して動的に変換する
    ''' </summary>
    Function ConvertToDate(ByVal value As Object, ByVal type As String) As String
        Dim dateValue As DateTime
        Dim timePattern As String = "^\d{1,2}:\d{2}$" ' h:mm または hh:mm 形式の正規表現パターン

        Select Case type
            Case "CREATED_TIME", "UPDATED_TIME", "DATETIME"
                If DateTime.TryParse(value.ToString(), dateValue) Then
                    Return dateValue.ToString("yyyy/MM/dd HH:mm:ss")
                Else
                    Return value.ToString()
                End If
            Case "DATE"
                If DateTime.TryParse(value.ToString(), dateValue) Then
                    Return dateValue.ToString("yyyy/MM/dd")
                Else
                    Return value.ToString()
                End If
            Case "TIME"
                Return value.ToString()
            Case "NUMBER"
                Return value.ToString()
            Case Else
                Return value.ToString()
        End Select

        '不具合のため上記に変更（例えば、NUMBERの"7.5"が「7月5日」と判断されるため）
        '' hh:mm形式の場合は変換しない
        'If Regex.IsMatch(value.ToString(), timePattern) Then
        '    Return value.ToString()
        'End If

        'If DateTime.TryParse(value.ToString(), dateValue) Then
        '    ' 時間部分が0:00:00の場合は日付のみ、それ以外は日付と時間をフォーマット
        '    If dateValue.TimeOfDay = TimeSpan.Zero Then
        '        Return dateValue.ToString("yyyy/MM/dd")
        '    Else
        '        Return dateValue.ToString("yyyy/MM/dd HH:mm:ss")
        '    End If
        'Else
        '    Return value.ToString()
        'End If
    End Function

    ''' <summary>
    ''' DataTable作成（KintoneAPIの返却データ格納用）
    ''' </summary>
    Public Sub CreateDataTable(ByRef ioTbl As DataTable)

        If IsNothing(ioTbl) Then ioTbl = New DataTable

        If ioTbl.Columns.Count <> 0 Then ioTbl.Columns.Clear()

        ioTbl.Clear()

        '返却用テーブル（カラム）作成
        ioTbl.Columns.Add("LINECNT", GetType(String)) 'ダウンロードするのに必要なためとりあえず...
        ioTbl.Columns.Add("OUTTBL", GetType(String)) '出力先テーブル（OK：LNT0001_ZISSEKI,NG:LNT0028_NGZISSEKI)
        ioTbl.Columns.Add("TORICODE_AVOCADO", GetType(String)) '取引先（アボカドコード）
        Dim properties = GetType(AvocadoRec).GetProperties()
        For Each prop In properties
            ioTbl.Columns.Add(prop.Name, GetType(String))
        Next

        '品名テーブルの対応
        'properties = GetType(AddItemRec).GetProperties()
        'For Each prop In properties
        '    ioTbl.Columns.Add(prop.Name, GetType(String))
        'Next
    End Sub

#Region "レスポンス内容格納クラス"
    ''' <summary>
    ''' KintoneAPIの返却データ（JSON）情報：全体構造
    ''' </summary>
    Public Class ApiResponse
        Public Property Records As List(Of AvocadoRec)
        Public Property Totalcount As String
    End Class
    ''' <summary>
    ''' KintoneAPIのデータ構造（一般）
    ''' (例）{"出荷場所名称": {"type" "SINGLE_LINE_TEXT","value": "ＸＸ名称"}}
    ''' </summary>
    Public Class NormalStruct
        Public Property type As String
        Public Property value As String
    End Class

    ''' <summary>
    ''' KintoneAPIのデータ構造（Valueが配列）
    ''' (例）{"配車配乗不可": {"type" "CHECK_BOX","value": []}}
    ''' </summary>
    Public Class ValListStruct
        Public Property type As String
        Public Property value As List(Of String)
    End Class

    ''' <summary>
    ''' KintoneAPIのデータ構造（Valueがネスト）
    ''' (例） "作成者": {"type": "CREATOR","value": {"code": "xxxx@jot.co.jp","name": "ＸＸ ＸＸ"}]}
    ''' </summary>
    Public Class ValNestStruct
        Public Property type As String
        Public Property value As NestedValue
        Public Class NestedValue
            Public Property code As String
            Public Property name As String
        End Class
    End Class

    ''' <summary>
    ''' KintoneAPIのデータ構造（リスト選択）
    ''' (例） "加算先受注受付部署": {"type": "ORGANIZATION_SELECT","value": [{"code": "020401","name": "EX 東北支店"}]}
    ''' </summary>
    Public Class ValSelectListStruct
        Public Property type As String
        Public Property value As List(Of SelectList)

        Public Class SelectList
            Public Property code As String
            Public Property name As String
        End Class
    End Class

    ''' <summary>
    ''' KintoneAPIのデータ構造（SUBTABLE）
    ''' (例１） "品名コード": {"type": "SUBTABLE","value": [{"id": "19755130","value":{"枝番":{"type":"NUMBER","value":"1"},"品名選択_枝番付き":{"type":"SINGLE_LINE_TEXT","value":""},....,"品名1名_枝番付き": {"type": "SINGLE_LINE_TEXT",value": ""}}}]}
    ''' (例２） "品名コード": {"type": "SUBTABLE","value": []}　　← データなしの場合
    ''' </summary>
    Public Class SubTableStruct
        Public Property type As String
        Public Property value As List(Of SelectList)

        Public Class SelectList
            Public Property id As String
            Public Property value As ValListStruct
        End Class

        Public Class ValListStruct
            Public Property 枝番 As NormalStruct
            Public Property 品名選択_枝番付き As NormalStruct
            Public Property 品名2コード_枝番付き As NormalStruct
            Public Property 品名2名_枝番付き As NormalStruct
            Public Property 油種名_枝番付き As NormalStruct
            Public Property 品名詳細_枝番付き As NormalStruct
            Public Property 品名1コード_枝番付き As NormalStruct
            Public Property 数量_枝番付き As NormalStruct
            Public Property 油種コード_枝番付き As NormalStruct
            Public Property 品名1名_枝番付き As NormalStruct
        End Class
    End Class
    ''' <summary>
    ''' KintoneAPIの返却データ（JSON）：業務データ部分
    ''' </summary>
    Public Class AvocadoRec
        Public Property レコード番号 As NormalStruct
        Public Property 積込荷卸区分 As NormalStruct
        Public Property 積置区分 As NormalStruct
        Public Property 配送セットID As NormalStruct
        Public Property 受注受付部署選択 As NormalStruct
        Public Property 受注受付部署名 As NormalStruct
        Public Property 受注受付部署コード As NormalStruct
        Public Property 受注受付部署略名 As NormalStruct
        Public Property 加算先部署名_受注受付部署 As NormalStruct
        Public Property 加算先部署コード_受注受付部署 As NormalStruct
        Public Property 加算先部署略名_受注受付部署 As NormalStruct
        Public Property 受注受付部署 As ValSelectListStruct
        Public Property 加算先受注受付部署 As ValSelectListStruct
        Public Property 品名選択 As NormalStruct
        Public Property 品名詳細 As NormalStruct
        Public Property 品名2名 As NormalStruct
        Public Property 品名2コード As NormalStruct
        Public Property 品名1名 As NormalStruct
        Public Property 品名1コード As NormalStruct
        Public Property 油種名 As NormalStruct
        Public Property 油種コード As NormalStruct
        Public Property 届先選択 As NormalStruct
        Public Property 届先コード As NormalStruct
        Public Property 届先名称 As NormalStruct
        Public Property 届先略名 As NormalStruct
        Public Property 届先取引先コード As NormalStruct
        Public Property 届先取引先名称 As NormalStruct
        Public Property 届先住所 As NormalStruct
        Public Property 届先電話番号 As NormalStruct
        Public Property 届先Googleマップ As NormalStruct
        Public Property 届先緯度 As NormalStruct
        Public Property 届先経度 As NormalStruct
        Public Property 届先備考1 As NormalStruct
        Public Property 届先備考2 As NormalStruct
        Public Property 届先備考3 As NormalStruct
        Public Property 届先カラーコード_背景色 As NormalStruct
        Public Property 届先カラーコード_境界色 As NormalStruct
        Public Property 届先カラーコード_文字色 As NormalStruct
        Public Property 出荷場所選択 As NormalStruct
        Public Property 出荷場所コード As NormalStruct
        Public Property 出荷場所名称 As NormalStruct
        Public Property 出荷場所略名 As NormalStruct
        Public Property 出荷場所取引先コード As NormalStruct
        Public Property 出荷場所取引先名称 As NormalStruct
        Public Property 出荷場所住所 As NormalStruct
        Public Property 出荷場所電話番号 As NormalStruct
        Public Property 出荷場所Googleマップ As NormalStruct
        Public Property 出荷場所緯度 As NormalStruct
        Public Property 出荷場所経度 As NormalStruct
        Public Property 出荷場所備考1 As NormalStruct
        Public Property 出荷場所備考2 As NormalStruct
        Public Property 出荷場所備考3 As NormalStruct
        Public Property 出荷場所カラーコード_背景色 As NormalStruct
        Public Property 出荷場所カラーコード_境界色 As NormalStruct
        Public Property 出荷場所カラーコード_文字色 As NormalStruct
        Public Property 出荷日 As NormalStruct
        Public Property 積込時間 As NormalStruct
        Public Property 積込時間手入力 As NormalStruct
        Public Property 積込時間_画面表示用 As NormalStruct
        Public Property 届日 As NormalStruct
        Public Property 指定時間 As NormalStruct
        Public Property 指定時間手入力 As NormalStruct
        Public Property 指定時間_画面表示用 As NormalStruct
        Public Property 受注数量 As NormalStruct
        Public Property 実績数量 As NormalStruct
        Public Property 数量単位 As NormalStruct
        Public Property 業務指示1 As NormalStruct
        Public Property 業務指示2 As NormalStruct
        Public Property 業務指示3 As NormalStruct
        Public Property 荷主備考 As NormalStruct
        Public Property 業務車番選択 As NormalStruct
        Public Property 出荷部署名 As NormalStruct
        Public Property 出荷部署コード As NormalStruct
        Public Property 出荷部署略名 As NormalStruct
        Public Property 加算先出荷部署名 As NormalStruct
        Public Property 加算先出荷部署コード As NormalStruct
        Public Property 加算先出荷部署略名 As NormalStruct
        Public Property 統一車番 As NormalStruct
        Public Property 陸事番号 As NormalStruct
        Public Property 車型 As NormalStruct
        Public Property 車腹 As NormalStruct
        Public Property 荷主名 As NormalStruct
        Public Property 契約区分 As NormalStruct
        Public Property 品名1名_車両 As NormalStruct
        Public Property 車両メモ As NormalStruct
        Public Property 車両備考1 As NormalStruct
        Public Property 車両備考2 As NormalStruct
        Public Property 車両備考3 As NormalStruct
        Public Property 統一車番_トラクタ As NormalStruct
        Public Property 陸事番号_トラクタ As NormalStruct
        Public Property トリップ As NormalStruct
        Public Property ドロップ As NormalStruct
        'Public Property 当日前後運行メモ As NormalStruct           '2025/06/05 削除
        Public Property 出勤時間 As NormalStruct
        Public Property 乗務員選択 As NormalStruct
        Public Property 氏名_乗務員 As NormalStruct
        Public Property 社員番号_乗務員 As NormalStruct
        Public Property 副乗務員選択 As NormalStruct
        Public Property 氏名_副乗務員 As NormalStruct
        Public Property 社員番号_副乗務員 As NormalStruct
        Public Property カレンダー画面メモ表示 As ValListStruct
        Public Property 業務車番選択_カレンダー画面メモ As NormalStruct
        Public Property 開始日_カレンダー画面メモ As NormalStruct
        Public Property 終了日_カレンダー画面メモ As NormalStruct
        Public Property 背景色_カレンダー画面メモ As NormalStruct
        Public Property 境界色_カレンダー画面メモ As NormalStruct
        Public Property 文字色_カレンダー画面メモ As NormalStruct
        Public Property 表示内容_カレンダー画面メモ As NormalStruct
        Public Property 業務車番_カレンダー画面メモ As NormalStruct
        Public Property 表示用終了日_カレンダー画面メモ As NormalStruct
        Public Property 業務車番 As NormalStruct
        Public Property 用車先 As NormalStruct
        Public Property レコードタイトル用 As NormalStruct
        Public Property 出庫日 As NormalStruct
        Public Property 帰庫日 As NormalStruct
        Public Property 帰庫時間 As NormalStruct
        Public Property 乗務員備考1 As NormalStruct
        Public Property 表示順_乗務員 As NormalStruct                 '2025/06/30 追加
        Public Property 乗務員備考2 As NormalStruct
        Public Property 副乗務員備考1 As NormalStruct
        Public Property 副乗務員備考2 As NormalStruct
        Public Property 表示順_副乗務員 As NormalStruct               '2025/06/30 追加
        Public Property 出勤時間_副乗務員 As NormalStruct
        Public Property 乗務員選択_カレンダー画面メモ As NormalStruct
        Public Property 社員番号_カレンダー画面メモ As NormalStruct
        Public Property 内容詳細_カレンダー画面メモ As NormalStruct
        Public Property 車腹単位 As NormalStruct
        Public Property 退勤時間 As NormalStruct
        Public Property 退勤時間_副乗務員 As NormalStruct
        Public Property kViewer用タイトル As NormalStruct
        Public Property kViewer用受注数量 As NormalStruct
        Public Property kViewer用実績数量 As NormalStruct
        Public Property kViewer用乗務員情報 As NormalStruct
        Public Property 乗務員コード_乗務員 As NormalStruct
        Public Property 乗務員コード_副乗務員 As NormalStruct
        Public Property kViewer用副乗務員情報 As NormalStruct
        Public Property オーダー変更削除 As NormalStruct
        Public Property 陸運局 As NormalStruct
        Public Property 分類番号 As NormalStruct
        Public Property ひらがな As NormalStruct
        Public Property 一連指定番号 As NormalStruct
        Public Property 陸運局_トラクタ As NormalStruct
        Public Property 分類番号_トラクタ As NormalStruct
        Public Property ひらがな_トラクタ As NormalStruct
        Public Property 一連指定番号_トラクタ As NormalStruct
        Public Property 車両備考1_トラクタ As NormalStruct
        Public Property 車両備考2_トラクタ As NormalStruct
        Public Property 車両備考3_トラクタ As NormalStruct
        Public Property 配車配乗不可 As ValListStruct
        Public Property 表示順_届先 As NormalStruct
        Public Property 表示順_配車 As NormalStruct
        Public Property 本トラクタ選択 As NormalStruct
        Public Property 出荷部署名_本トラクタ As NormalStruct
        Public Property 業務車番_本トラクタ As NormalStruct
        Public Property 出荷部署コード_本トラクタ As NormalStruct
        Public Property 出荷部署略名_本トラクタ As NormalStruct
        Public Property 加算先出荷部署略名_本トラクタ As NormalStruct
        Public Property 加算先出荷部署コード_本トラクタ As NormalStruct
        Public Property 加算先出荷部署名_本トラクタ As NormalStruct
        Public Property 用車先_本トラクタ As NormalStruct
        Public Property 統一車番_本トラクタ As NormalStruct
        Public Property 陸事番号_本トラクタ As NormalStruct
        Public Property 車型_本トラクタ As NormalStruct
        Public Property 車腹_本トラクタ As NormalStruct
        Public Property 車腹単位_本トラクタ As NormalStruct
        Public Property 陸運局_本トラクタ As NormalStruct
        Public Property 分類番号_本トラクタ As NormalStruct
        Public Property ひらがな_本トラクタ As NormalStruct
        Public Property 一連指定番号_本トラクタ As NormalStruct
        Public Property 荷主名_本トラクタ As NormalStruct
        Public Property 契約区分_本トラクタ As NormalStruct
        Public Property 品名1名_車両_本トラクタ As NormalStruct
        Public Property 車両メモ_本トラクタ As NormalStruct
        Public Property 車両備考1_本トラクタ As NormalStruct
        Public Property 車両備考2_本トラクタ As NormalStruct
        Public Property 車両備考3_本トラクタ As NormalStruct
        Public Property 用車先_カレンダー画面メモ As NormalStruct
        Public Property 車型_カレンダー画面メモ As NormalStruct
        Public Property 陸事番号_カレンダー画面メモ As NormalStruct
        Public Property 車腹_カレンダー画面メモ As NormalStruct
        Public Property 車腹単位_カレンダー画面メモ As NormalStruct
        Public Property 陸運局_カレンダー画面メモ As NormalStruct
        Public Property 分類番号_カレンダー画面メモ As NormalStruct
        Public Property ひらがな_カレンダー画面メモ As NormalStruct
        Public Property 一連指定番号_カレンダー画面メモ As NormalStruct
        Public Property 陸事番号_トラクタ_カレンダー画面メモ As NormalStruct
        Public Property 陸運局_トラクタ_カレンダー画面メモ As NormalStruct
        Public Property 分類番号_トラクタ_カレンダー画面メモ As NormalStruct
        Public Property ひらがな_トラクタ_カレンダー画面メモ As NormalStruct
        Public Property 一連指定番号_トラクタ_カレンダー画面メモ As NormalStruct
        Public Property オーダー開始日 As NormalStruct
        Public Property 表示用オーダー終了日 As NormalStruct
        Public Property オーダー終了日 As NormalStruct
        Public Property 更新者 As ValNestStruct
        Public Property 作成者 As ValNestStruct
        Public Property 更新日時 As NormalStruct
        Public Property 作成日時 As NormalStruct
        '品名テーブルは、LNGでは取込まない 2025/3/26
        'Public Property 品名テーブル As SubTableStruct
        Public Property 標準所要時間 As NormalStruct
        Public Property JX形式オーダー更新キー As NormalStruct
        Public Property JX形式オーダーファイル名 As NormalStruct
        Public Property JX形式オーダールート番号 As NormalStruct
        Public Property JX形式オーダー先頭届先名称 As NormalStruct              '2025/06/05 追加
        Public Property 運転日報番号 As NormalStruct                            '2025/06/30 追加
        '東ガスのみの項目
        Public Property 回転数 As NormalStruct
        Public Property L配更新キー As NormalStruct
        Public Property はこぶわ更新キー As NormalStruct
        Public Property 最大積載量 As NormalStruct           '2025/06/05 追加
    End Class
    ''' <summary>
    ''' KintoneAPIの返却データ（東ガスのみの項目）を格納
    ''' </summary>
    Public Class TGSItem
        '東ガスのみの項目
        Public Property 回転数 As NormalStruct
        Public Property L配更新キー As NormalStruct
        Public Property はこぶわ更新キー As NormalStruct
        Public Property 最大積載量 As NormalStruct
    End Class
    ''' <summary>
    ''' KintoneAPIの返却データ（品名テーブル）を格納
    ''' </summary>
    Public Class AddItemRec
        Public Property 枝番 As NormalStruct
        Public Property 品名選択_枝番付き As NormalStruct
        Public Property 品名2コード_枝番付き As NormalStruct
        Public Property 品名2名_枝番付き As NormalStruct
        Public Property 油種名_枝番付き As NormalStruct
        Public Property 品名詳細_枝番付き As NormalStruct
        Public Property 品名1コード_枝番付き As NormalStruct
        Public Property 数量_枝番付き As NormalStruct
        Public Property 油種コード_枝番付き As NormalStruct
        Public Property 品名1名_枝番付き As NormalStruct
    End Class

    ''' <summary>
    ''' KintoneAPIの返却エラー情報
    ''' </summary>
    Public Class ErrInfo
        ''' <summary>
        ''' エラーID
        ''' </summary>
        Public Property Id As String
        ''' <summary>
        ''' エラーの種類を表すコード
        ''' </summary>
        Public Property Code As String
        ''' <summary>
        ''' エラーメッセージ
        ''' </summary>
        Public Property Message As String
    End Class

#End Region
End Class
