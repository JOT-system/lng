Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNS0002WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNS0002S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNS0002L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNS0002D"       'MAPID(更新)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' ロールマスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_OBJCODE"></param>
    ''' <returns></returns>
    Function CreateRoleList(ByVal I_COMPCODE As String, ByVal I_OBJCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_CLASSCODE) = I_OBJCODE
        CreateRoleList = prmData
    End Function

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
    ''' 会社コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="COMPANY_FLG"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateCOMPANYParam(ByVal COMPANY_FLG As Integer, ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = COMPANY_FLG

        CreateCOMPANYParam = prmData

    End Function

    ''' <summary>
    ''' 組織コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="AUTHORITYALL_FLG"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateORGParam(ByVal AUTHORITYALL_FLG As Integer, ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = AUTHORITYALL_FLG
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateORGParam = prmData

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
    ''' コンテナ種別の初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewDisplayFlags() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)
        retVal.Add(New DisplayFlag(True, "通風", "BIGCTN00", 0, "05"))
        retVal.Add(New DisplayFlag(True, "冷蔵", "BIGCTN01", 1, "10"))
        retVal.Add(New DisplayFlag(True, "ｽｰﾊﾟｰUR", "BIGCTN02", 2, "11"))
        retVal.Add(New DisplayFlag(True, "冷凍", "BIGCTN03", 3, "15"))
        retVal.Add(New DisplayFlag(True, "L10屯", "BIGCTN04", 4, "20"))
        retVal.Add(New DisplayFlag(True, "ウイング", "BIGCTN05", 5, "25"))
        retVal.Add(New DisplayFlag(True, "有蓋", "BIGCTN06", 6, "30"))
        retVal.Add(New DisplayFlag(True, "無蓋", "BIGCTN07", 7, "35"))
        Return retVal
    End Function

    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="USERID">ユーザーID</param>
    ''' <param name="STYMD2">開始年月日</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                                 ByRef USERID As String, ByRef STYMD2 As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     USERID                                  " _
            & "   , STYMD                                   " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     COM.lns0001_user                        " _
            & " WHERE                                       " _
            & "         USERID  = @P1                       " _
            & "     AND STYMD   = @P2                       "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 20) 'ユーザーID
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 20) '利用開始日

                PARA1.Value = USERID 'ユーザーID
                PARA2.Value = STYMD2 '利用開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNS0002Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0002Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0002Chk.Load(SQLdr)

                    If LNS0002Chk.Rows.Count > 0 Then
                        Dim LNS0002row As DataRow
                        LNS0002row = LNS0002Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNS0002row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNS0002row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNS0002D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' コンテナ種別関連クラス
    ''' </summary>
    <Serializable>
    Public Class DisplayFlag
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="checked">チェック</param>
        ''' <param name="dispName">画面表示名</param>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispOrder">並び順</param>
        Public Sub New(checked As Boolean, dispName As String, fieldName As String, dispOrder As Integer, selectCode As String)
            Me.Checked = checked
            Me.DispName = dispName
            Me.FieldName = fieldName
            Me.DispOrder = dispOrder
            Me.selectCode = selectCode
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
        Public Property Checked As Boolean
        ''' <summary>
        ''' 選択コード
        ''' </summary>
        ''' <returns></returns>
        Public Property selectCode As String
    End Class

End Class