Option Strict On
Imports System.Runtime.Serialization.Json
''' <summary>
''' 楽々精算用のWebAPI共通クラス
''' </summary>
''' <remarks>基本的に例外はThrowするので呼出し元で制御</remarks>
Public Class CS0053WebApi
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    ''' <summary>
    ''' APIトークン
    ''' </summary>
    ''' <returns></returns>
    Public Property ApiToken As String = ""
    ''' <summary>
    ''' APIを実行する元となるURLを設定(例 {0}にアカウント（引数指定）,{1}にAPIメソッド名（自動）、{2}にバージョン（自動） https://hnyeti.rakurakuhanbai.jp/{0}/api/{1}/version/{2}
    ''' </summary>
    ''' <returns></returns>
    Public Property ApiBaseUrl As String = ""
    ''' <summary>
    ''' ApiBaseUrlに付与するアカウント名
    ''' </summary>
    ''' <returns></returns>
    Public Property ApiAccount As String = ""
    ''' <summary>
    ''' API名称定義(名称,バージョン)
    ''' </summary>
    Private ApiTypes As New Dictionary(Of String, String) From {{"fileupload", "v1"},
                                                                {"csvimport", "v1"}}

    ''' <summary>
    ''' 最後にAPIを実行した結果の詳細はこのプロパティーに収める
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>後続処理で詳細が知りたい場合はこのプロパティーを参照</remarks>
    Public Property LastResponseValue As APIResultClass
    ''' <summary>
    ''' テスト用　引数無しのこのコンストラクタは仕様しないこと！引数なしのNewはしない！
    ''' </summary>
    Public Sub New()
        Me.ApiBaseUrl = "https://hnyeti.rakurakuhanbai.jp/{0}/api/{1}/version/{2}"
        Me.ApiAccount = "xgpq6aa"
        Me.ApiToken = "FU7i9qDBVt2sZBMKeEvUJ0MIYhmkbrMexlmXUBPtiWOJj3BDrXItFuqKhJxoU2E9"
    End Sub
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="apiBaseUrl">{0}ApiアカウントID,{1}Apiメソッド,{2}ApiバージョンをプレースフォルダとしたURLを指定</param>
    ''' <param name="apiAccount">APIアカウント</param>
    ''' <param name="apiToken">APIトークン</param>
    Public Sub New(apiBaseUrl As String, apiAccount As String, apiToken As String)
        Me.ApiBaseUrl = apiBaseUrl
        Me.ApiAccount = apiAccount
        Me.ApiToken = apiToken
        Net.ServicePointManager.SecurityProtocol = Net.ServicePointManager.SecurityProtocol Or System.Net.SecurityProtocolType.Tls12
    End Sub

    ''' <summary>
    ''' アップロードしたファイルのIDを保持（基本外で設定しない想定）
    ''' </summary>
    ''' <returns></returns>
    Public Property FileId As String
    ''' <summary>
    ''' 送信ファイルのバックアップ保存パス（未指定時は保存しない）
    ''' </summary>
    ''' <returns></returns>
    Public Property BackUpPath As String = ""
    ''' <summary>
    ''' ファイルアップロード(ファイルパス)
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns>楽々側で返却するファイル番号</returns>
    ''' <remarks>エラー時はスローするので呼出し側で制御
    ''' テキストファイルの対応なのでバイナリ(Excelファイル等)の場合は別途オーバーロードが必要
    ''' またレコード0件の場合は例外を投げるので注意</remarks>
    Public Function Upload(filePath As String) As String
        Me.LastResponseValue = Nothing
        Dim retVal As String
        'パスが空白
        If filePath.Trim = "" Then
            Throw New Exception("楽々へのアップロード対象のファイルパスが空白です。")
        End If
        'ファイルが存在しない場合
        If IO.File.Exists(filePath) = False Then
            Throw New Exception(String.Format("楽々へのアップロード対象のファイルが存在しません。パス:{0}", filePath))
        End If
        'ファイル名のみ取得
        Dim fileName As String = IO.Path.GetFileName(filePath)
        'ファイル内容の確認（中身が無い場合はエラー）
        Using fs As New IO.FileStream(filePath, IO.FileMode.Open, IO.FileAccess.Read)
            If fs.Length = 0 Then
                Throw New Exception("対象のファイル内容が無し（0バイト）")
            End If
        End Using
        retVal = ExecuteUploadApi(filePath)
        Return retVal
    End Function
    ''' <summary>
    ''' ファイルアップロードAPI実行
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns></returns>
    Private Function ExecuteUploadApi(filePath As String) As String


        'APIアップロード用のアドレス生成
        Dim aploadApiUrl As String = String.Format(Me.ApiBaseUrl, Me.ApiAccount, Me.ApiTypes.Keys(0), Me.ApiTypes.Values(0))

        Dim boundary = "----------------------------" & DateTime.Now.Ticks.ToString("x")
        Dim request = Net.WebRequest.Create(aploadApiUrl)
        '複数ファイルをアップする指定（シングルファイルしか送信しないがこの設定を自作しないと通らない）
        request.ContentType = "multipart/form-data; boundary=" & boundary
        request.Method = "POST"
        'ヘッダーにAPIのトークンの設定
        request.Headers.Add("X-HD-apitoken", Me.ApiToken)
        '送信内容を一旦展開するためのメモリーストリーム
        Dim memStream = New System.IO.MemoryStream()
        Dim crlfStr As String = ControlChars.CrLf
        Dim boundarybytes = System.Text.Encoding.ASCII.GetBytes(crlfStr & "--" & boundary + crlfStr)
        Dim endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes(crlfStr & "--" & boundary & "--")


        '送信ファイルのヘッダーをメモリーストリームに書き込む
        Dim headerTemplate =
        "Content-Disposition: form-data; name=""{0}""; filename=""{1}""" & crlfStr &
        "Content-Type: text/csv" & crlfStr & crlfStr

        memStream.Write(boundarybytes, 0, boundarybytes.Length)
        Dim fileName As String = System.IO.Path.GetFileName(filePath)
        'Dim header = String.Format(headerTemplate, "uploadFile", "uploadFile")
        Dim header = String.Format(headerTemplate, "uploadFile", fileName)
        'Dim headerbytes = System.Text.Encoding.UTF8.GetBytes(header)
        Dim headerbytes = System.Text.Encoding.GetEncoding("Shift_JIS").GetBytes(header)

        memStream.Write(headerbytes, 0, headerbytes.Length)
        '送信ファイルの実体をメモリーストリームに書き込む
        Using fileStream = New IO.FileStream(filePath, IO.FileMode.Open, IO.FileAccess.Read)
            Dim buf(1024) As Byte
            Dim bytesRead = 0
            While True
                bytesRead = fileStream.Read(buf, 0, buf.Length)
                If bytesRead = 0 Then
                    Exit While
                End If
                memStream.Write(buf, 0, bytesRead)
            End While
        End Using

        memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length)
        request.ContentLength = memStream.Length
        'メモリーストリームの内容を要求情報に書き出し
        Using requestStream = request.GetRequestStream()

            memStream.Position = 0
            Dim tempBuffer(CInt(memStream.Length) - 1) As Byte
            memStream.Read(tempBuffer, 0, tempBuffer.Length)
            memStream.Close()
            requestStream.Write(tempBuffer, 0, tempBuffer.Length)
        End Using
        '実際の要求をWebAPIのサーバー投げ結果を取得
        Try
            Dim jsonResult As String
            Using response = request.GetResponse()

                Dim stream2 = response.GetResponseStream()
                Dim reader2 = New IO.StreamReader(stream2)
                jsonResult = reader2.ReadToEnd()
            End Using
            'JSON形式のレスポンスを独自クラスに分離
            Dim desObj = JsonDeserialize(jsonResult)
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0007D CS0053"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "ResponseCode:" + desObj.Code
            CS0011LOGWrite.MESSAGENO = "ResponseCodeCheck"
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            '使わないかもしれないが最終実行結果プロパティーに保持
            Me.LastResponseValue = desObj
            'ここにくる場合はsuccess以外はありえないと思うが念のため失敗してたらスロー
            If desObj Is Nothing OrElse desObj.Status <> "success" Then
                Throw New Exception("APIエラー:" & jsonResult)
            End If
            '当関数の結果（ファイルID）を返却
            Return desObj.Items.FileId
        Catch ex As Net.WebException
            '通信出来ていても40x系のエラーはWebExceptionに飛ばされる為ここでトラップ
            Dim responseObj = ex.Response
            'JSON形式ではないサーバー返答のWebExceptionは通信そのものに問題があるのでそのまま上位にスロー
            If Not responseObj.ContentType.Contains("application/json") Then
                Throw
            End If
            Dim objEncError As Encoding = Encoding.UTF8
            Dim htmlError As String = ""
            Using resStreamError As IO.Stream = responseObj.GetResponseStream()
                Using srError As IO.StreamReader = New IO.StreamReader(resStreamError, objEncError)
                    htmlError = srError.ReadToEnd()
                End Using
            End Using
            Dim desObj = JsonDeserialize(htmlError)
            '使わないかもしれないが最終実行結果プロパティーに保持
            Me.LastResponseValue = desObj
            'JSONデシリアライズを行い値を分離
            Throw New Exception("APIエラー:" & htmlError)
        End Try

    End Function
    ''' <summary>
    ''' 楽々精算APIより返却されたJSON形式のレスポンスを.NETで扱いやすい独自クラスに変換
    ''' </summary>
    ''' <param name="jsonString"></param>
    ''' <remarks>構造の変更に耐える為、自作クラス型</remarks>
    ''' <returns></returns>
    Private Function JsonDeserialize(jsonString As String) As APIResultClass
        Dim result As New APIResultClass
        Dim serializer As New DataContractJsonSerializer(GetType(APIResultClass))
        Using stream As New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString))
            result = DirectCast(serializer.ReadObject(stream), APIResultClass)
        End Using
        Return result
    End Function

    ''' <summary>
    ''' CSVファイル登録処理(仕様上本来ここれは使わない想定でアップロードのみ)
    ''' </summary>
    ''' <param name="fileID">Uploadメソッドで取得したファイルID</param>
    ''' <param name="schemaID">スキーマID(楽々側の取込先情報)</param>
    ''' <param name="importID">インポートID(楽々側の取込先情報)</param>
    ''' <returns>APIの処理結果であるプロセスIDを返却（後続で使うならご使用を）</returns>
    ''' <remarks></remarks>
    Public Function CsvImport(fileID As String, schemaID As String, importID As String) As String

        Me.LastResponseValue = Nothing
        If fileID.Trim = "" Then
            Throw New Exception("ファイルIDが空の為、楽々へのCSVインポートが出来ません。")
        End If
        If schemaID.Trim = "" Then
            Throw New Exception("スキーマIDが空の為、楽々へのCSVインポートが出来ません。")
        End If
        If importID.Trim = "" Then
            Throw New Exception("インポートIDが空の為、楽々へのCSVインポートが出来ません。")
        End If

        Dim importApiUrl As String = String.Format(Me.ApiBaseUrl, Me.ApiAccount, Me.ApiTypes.Keys(1), Me.ApiTypes.Values(1))

        Using wcObj = New Net.WebClient
            wcObj.Headers.Add("Content-Type", "application/json; charset=utf-8")
            wcObj.Headers.Add("X-HD-apitoken", Me.ApiToken)
            Dim requestParam As String = ""
            requestParam = requestParam & "{""dbSchemaId"":""" & schemaID & """"
            requestParam = requestParam & ",""importId"":""" & importID & """"
            requestParam = requestParam & ",""fileId"":""" & fileID & """"
            requestParam = requestParam & "}"
            'WebAPI実行
            Try
                Dim retJson = wcObj.UploadString(importApiUrl, requestParam)
                Dim desObj = JsonDeserialize(retJson)
                Me.LastResponseValue = desObj
                'ここにくる場合はsuccess以外はありえないと思うが念のため失敗してたらスロー
                If desObj Is Nothing OrElse desObj.Status <> "success" Then
                    Throw New Exception("APIエラー:" & retJson)
                End If
                Return desObj.ProcessId
            Catch ex As Net.WebException
                '通信出来ていても40x系のエラーはWebExceptionに飛ばされる為ここでトラップ
                Dim responseObj = ex.Response
                'JSON形式ではないサーバー返答のWebExceptionは通信そのものに問題があるのでそのまま上位にスロー
                If Not responseObj.ContentType.Contains("application/json") Then
                    Throw
                End If
                Dim objEncError As Encoding = Encoding.UTF8
                Dim htmlError As String = ""
                Using resStreamError As IO.Stream = responseObj.GetResponseStream()
                    Using srError As IO.StreamReader = New IO.StreamReader(resStreamError, objEncError)
                        htmlError = srError.ReadToEnd()
                    End Using
                End Using
                Dim desObj = JsonDeserialize(htmlError)
                '使わないかもしれないが最終実行結果プロパティーに保持
                Me.LastResponseValue = desObj
                'JSONデシリアライズを行い値を分離
                Throw New Exception("APIエラー:" & htmlError)
            End Try

        End Using

    End Function
#Region "レスポンス内容格納クラス"
    ''' <summary>
    ''' APIレスポンス情報格納クラス
    ''' </summary>
    ''' <remarks>JSON形式の情報をその当クラスに割振</remarks>
    <Runtime.Serialization.DataContract>
    Public Class APIResultClass
        ''' <summary>
        ''' ステータス
        ''' </summary>
        ''' <returns>リクエストが成功したかどうか (success:成功 error:異常)</returns>
        <Runtime.Serialization.DataMember(Name:="status")>
        Public Property Status As String
        ''' <summary>
        ''' レスポンスコード
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="code")>
        Public Property Code As String
        ''' <summary>
        ''' リクエスト URL
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="url")>
        Public Property Url As String
        ''' <summary>
        ''' 取得・更新件数
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="count")>
        Public Property Count As String
        ''' <summary>
        ''' API のバージョン
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="version")>
        Public Property Version As String
        ''' <summary>
        ''' サービス名称
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="service")>
        Public Property Service As String
        ''' <summary>
        ''' サービス名称
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="accessTime")>
        Public Property AccessTime As String
        ''' <summary>
        ''' 取得データ
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="items")>
        Public Property Items As ApiItemClass
        ''' <summary>
        ''' プロセス ID
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>キューイングが必要な処理の場合、処理を示す ID を格納します。
        ''' </remarks>
        <Runtime.Serialization.DataMember(Name:="processId")>
        Public Property ProcessId As String
        ''' <summary>
        ''' バッチの状態
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>キューイングが必要な処理の場合、現在の状態を格納します。(wait:待ち active:実行中 complete : 完了)
        ''' </remarks>
        <Runtime.Serialization.DataMember(Name:="processStatus")>
        Public Property ProcessStatus As String

        ''' <summary>
        ''' エラー情報
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="errors")>
        Public Property Errors As ApiErrorClass
    End Class
    ''' <summary>
    ''' 楽々WebAPIの返却の取得データ(Item)情報の詳細
    ''' </summary>
    <Runtime.Serialization.DataContract>
    Public Class ApiItemClass
        ''' <summary>
        ''' ファイルID(アップロードAPIで取得)
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="fileId")>
        Public Property FileId As String
        ''' <summary>
        ''' インポート予約番号
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="processId")>
        Public Property ProcessId As String
    End Class
    ''' <summary>
    ''' 楽々WebAPIの返却のエラー情報の詳細
    ''' </summary>
    <Runtime.Serialization.DataContract>
    Public Class ApiErrorClass
        ''' <summary>
        ''' エラーコード
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="code")>
        Public Property Code As String
        ''' <summary>
        ''' エラー状態
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="msg")>
        Public Property Msg As String
        ''' <summary>
        ''' 詳細情報
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="description")>
        Public Property Description As ApiErrorDescription()
    End Class
    ''' <summary>
    '''楽々WebApiのエラー情報Description詳細
    ''' </summary>
    <Runtime.Serialization.DataContract>
    Public Class ApiErrorDescription
        ''' <summary>
        ''' 詳細情報
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="name")>
        Public Property Name As String
        ''' <summary>
        ''' パラメータ値
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="value")>
        Public Property Value As String
        ''' <summary>
        ''' 詳細コード
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="code")>
        Public Property Code As String
        ''' <summary>
        ''' エラーメッセージ
        ''' </summary>
        ''' <returns></returns>
        <Runtime.Serialization.DataMember(Name:="msg")>
        Public Property Msg As String
    End Class
#End Region
End Class
