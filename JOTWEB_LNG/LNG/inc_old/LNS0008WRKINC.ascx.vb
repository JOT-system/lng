Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNS0008WRKINC
    Inherits System.Web.UI.UserControl
    Public Const MAPIDS As String = "LNS0008S"       'MAPID(条件)
    Public Const MAPIDL As String = "LNS0008L"       'MAPID(実行)
    Public Const MAPIDC As String = "LNS0008D"       'MAPID(更新)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    Public Const GUIDANCEROOT As String = "GUIDANCE"

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <returns></returns>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 対象フラグの初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewDisplayFlags() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)

        retVal.Add(New DisplayFlag("社外", "OUTFLG", 0, ""))
        retVal.Add(New DisplayFlag("高圧ガス１部", "INFLG1", 1, "011310"))
        retVal.Add(New DisplayFlag("北海道支店", "INFLG2", 2, "010102"))
        retVal.Add(New DisplayFlag("東北支店", "INFLG3", 3, "010401"))
        retVal.Add(New DisplayFlag("関東支店", "INFLG4", 4, "011402"))
        'retVal.Add(New DisplayFlag("新潟事業所", "INFLG5", 5, "011501"))
        retVal.Add(New DisplayFlag("中部支店", "INFLG6", 6, "012401"))
        retVal.Add(New DisplayFlag("関西支店", "INFLG7", 7, "012701"))
        retVal.Add(New DisplayFlag("九州支店", "INFLG8", 8, "014001"))
        Return retVal
    End Function

    ''' <summary>
    ''' リストアイテムを受け渡し用にエンコードする
    ''' </summary>
    ''' <param name="dispFlags"></param>
    ''' <returns></returns>
    Public Function EncodeDisplayFlags(dispFlags As List(Of DisplayFlag)) As String
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noCompressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, dispFlags)
            noCompressionByte = ms.ToArray
        End Using

        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noCompressionByte, 0, noCompressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        Return base64Str
    End Function

    ''' <summary>
    ''' リストアイテムを受け渡し用にエンコードする
    ''' </summary>
    ''' <param name="base64Str">base64エンコードした文字列</param>
    ''' <returns></returns>
    Public Function DecodeDisplayFlags(base64Str As String) As List(Of DisplayFlag)
        Dim retVal As List(Of DisplayFlag)
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim compressedByte As Byte()
        compressedByte = Convert.FromBase64String(base64Str)
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(compressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            retVal = DirectCast(formatter.Deserialize(outMs), List(Of DisplayFlag))
        End Using
        Return retVal
    End Function

    ''' <summary>
    ''' チェックボックスの状態をフラグリストに設定
    ''' </summary>
    ''' <param name="chklObj"></param>
    ''' <param name="dispFlags"></param>
    ''' <returns></returns>
    Public Function SetSelectedDispFlags(chklObj As CheckBoxList, dispFlags As List(Of DisplayFlag)) As List(Of DisplayFlag)
        Dim chkFieldNames As New List(Of String)
        Dim qSelectedChk = From chkitm In chklObj.Items.Cast(Of ListItem) Where chkitm.Selected Select chkitm.Value
        If qSelectedChk.Any Then
            chkFieldNames = qSelectedChk.ToList
        End If
        Dim retObj = dispFlags
        For Each retItm In retObj
            retItm.Checked = False
            If chkFieldNames.Contains(retItm.FieldName) Then
                retItm.Checked = True
            End If
        Next
        Return retObj
    End Function

    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="GUIDANCENO">ガイダンスNo.</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                          ByRef GUIDANCENO As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     GUIDANCENO                              " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     COM.LNS0008_GUIDANCE                    " _
            & " WHERE                                       " _
            & "     GUIDANCENO       = @P1                  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 12) 'ガイダンスNo.

                PARA1.Value = GUIDANCENO       'ガイダンスNo

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNS0008Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0008Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0008Chk.Load(SQLdr)

                    If LNS0008Chk.Rows.Count > 0 Then
                        Dim LNS0008row As DataRow
                        LNS0008row = LNS0008Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNS0008row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNS0008row("UPDTIMSTP").ToString <> TIMESTAMP Then
                                ' 排他エラー
                                O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                            End If
                        End If
                    Else
                        ' 排他エラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                    End If
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0008C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 掲載フラグ関連クラス
    ''' </summary>
    <Serializable>
    Public Class DisplayFlag
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="dispName">画面表示名</param>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispOrder">並び順</param>
        Public Sub New(dispName As String, fieldName As String, dispOrder As Integer, officeCode As String)
            Me.DispName = dispName
            Me.FieldName = fieldName
            Me.DispOrder = dispOrder
            Me.OfficeCode = officeCode
        End Sub
        ''' <summary>
        ''' 表示名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispName As String
        ''' <summary>
        ''' 対象フィールド
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 表示順
        ''' </summary>
        ''' <returns></returns>
        Public Property DispOrder As Integer
        ''' <summary>
        ''' 表示グループ（仮）
        ''' </summary>
        ''' <returns></returns>
        Public Property Group As String = ""
        ''' <summary>
        ''' 選択フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Checked As Boolean = False
        ''' <summary>
        ''' オフィスコード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
    End Class

    ''' <summary>
    ''' ガイダンス情報クラス
    ''' </summary>
    <Serializable>
    Public Class GuidanceItemClass
        ''' <summary>
        ''' ガイダンス番号
        ''' </summary>
        ''' <returns></returns>
        Public Property GuidanceNo As String
        ''' <summary>
        ''' 掲載開始日
        ''' </summary>
        ''' <returns></returns>
        Public Property FromYmd As String
        ''' <summary>
        ''' 掲載終了日
        ''' </summary>
        ''' <returns></returns>
        Public Property EndYmd As String
        ''' <summary>
        ''' 種類(I:インフォメーション,W:注意,E:障害)
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As String
        ''' <summary>
        ''' タイトル
        ''' </summary>
        ''' <returns></returns>
        Public Property Title As String
        ''' <summary>
        ''' 対象
        ''' </summary>
        ''' <returns></returns>
        Public Property DispFlags As New List(Of DisplayFlag)
        ''' <summary>
        ''' 内容
        ''' </summary>
        ''' <returns></returns>
        Public Property Naiyo As String
        ''' <summary>
        ''' 添付ファイル
        ''' </summary>
        ''' <returns></returns>
        Public Property Attachments As New List(Of FileItemClass)
        ''' <summary>
        ''' 削除フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property DelFlg As String

        Public Property InitYmd As String
        Public Property InitUser As String
        Public Property InitTermId As String
        Public Property UpdYmd As String
        Public Property UpdUserUser As String
        Public Property UpdTermId As String
    End Class

    ''' <summary>
    ''' ファイル情報クラス
    ''' </summary>
    <Serializable>
    Public Class FileItemClass
        ''' <summary>
        ''' ファイル名
        ''' </summary>
        ''' <returns></returns>
        Public Property FileName As String

    End Class

    ''' <summary>
    ''' ガイダンス番号,ファイル番号を元にパラメータを生成する
    ''' </summary>
    ''' <param name="guidanceNo"></param>
    ''' <param name="FileInfo"></param>
    ''' <returns></returns>
    Public Shared Function GetParamString(guidanceNo As String, FileInfo As String, Optional isRefOnly As String = "1") As String
        Dim r As New Random
        Dim number As Integer = r.Next(1, 1001)
        '文字列で推察できなくする
        Dim dateNow As String = Now.ToString("yyyyMMddHHmmssFFF")
        Dim fileItm As New List(Of String) From {number.ToString("00#"), guidanceNo, FileInfo, isRefOnly, dateNow}
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noCompressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, fileItm)
            noCompressionByte = ms.ToArray
        End Using

        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noCompressionByte, 0, noCompressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        Return HttpUtility.UrlEncode(base64Str)
    End Function

    ''' <summary>
    ''' ガイダンス番号,ファイル番号をデコードする
    ''' </summary>
    ''' <param name="base64Str">base64エンコードした文字列</param>
    ''' <returns>配列（添字0:ガイダンス番号、添字1:ファイル情報(添字2が"0":ファイル名,"1":ファイル番号)、添字3：参照先 0:作業フォルダ、1:実体フォルダ</returns>
    Public Shared Function DecodeParamString(base64Str As String) As List(Of String)
        Dim retVal As New List(Of String)
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim compressedByte As Byte()
        compressedByte = Convert.FromBase64String(base64Str)
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(compressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            Dim decVal = DirectCast(formatter.Deserialize(outMs), List(Of String))
            retVal.Add(decVal(1)) 'ガイダンス番号
            retVal.Add(decVal(2)) 'ファイル情報
            retVal.Add(decVal(3)) '参照(1：参照、2：作業)
        End Using
        Return retVal
    End Function

End Class