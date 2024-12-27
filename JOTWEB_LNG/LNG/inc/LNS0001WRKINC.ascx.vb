Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNS0001WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNS0001S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNS0001L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNS0001D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNS0001H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    'パスワード(Excel)ダウンロード時
    Public Const FILEDOWNLOAD_PASSWORD = "********"

    ''' <summary>
    ''' ファイルタイプ
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum FILETYPE
        EXCEL
        PDF
    End Enum

    ''' <summary>
    ''' 入出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOL
        DELFLG   '削除フラグ
        USERID   'ユーザーID
        PASSWORD   'パスワード
        MISSCNT   '誤り回数
        PASSENDYMD   'パスワード有効期限
        STYMD   '開始年月日
        ENDYMD   '終了年月日
        ORG   '組織コード
        STAFFNAMES   '社員名（短）
        STAFFNAMEL   '社員名（長）
        EMAIL   'メールアドレス
        MENUROLE   'メニュー表示制御ロール
        MAPROLE   '画面参照更新制御ロール
        VIEWPROFID   '画面表示項目制御ロール
        RPRTPROFID   'エクセル出力制御ロール
        MAPID   '画面ＩＤ
        _VARIANT   '画面初期値ロール
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        USERID   'ユーザーID
        STYMD   '開始年月日
        ENDYMD   '終了年月日
        ORG   '組織コード
        STAFFNAMES   '社員名（短）
        STAFFNAMEL   '社員名（長）
        EMAIL   'メールアドレス
        MENUROLE   'メニュー表示制御ロール
        MAPROLE   '画面参照更新制御ロール
        VIEWPROFID   '画面表示項目制御ロール
        RPRTPROFID   'エクセル出力制御ロール
        MAPID   '画面ＩＤ
        _VARIANT   '画面初期値ロール
    End Enum

    '操作区分
    Public Enum OPERATEKBN
        NEWDATA = 1 '新規
        UPDDATA = 2 '更新
        DELDATA = 3 '削除
    End Enum

    '変更区分
    Public Enum MODIFYKBN
        NEWDATA = 1 '新規
        BEFDATA = 2 '変更前
        AFTDATA = 3　'変更後
    End Enum

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 日付がシリアル値になっている場合正しい日付に変換する
    ''' </summary>
    ''' <param name="I_VALUE">対象文字列</param>
    ''' <remarks></remarks>
    Public Shared Function DateConvert(ByVal I_VALUE As Object) As String
        Dim dt As DateTime
        Dim i As Integer
        '日付に変換できる場合
        If DateTime.TryParse(I_VALUE, dt) Then
            DateConvert = dt
        Else
            '数値に変換できる場合
            If Integer.TryParse(I_VALUE, i) Then
                DateConvert = DateTime.FromOADate(i)
            Else
                DateConvert = ""
            End If
        End If
    End Function

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
    ''' データ変換(データ型チェック)
    ''' </summary>
    ''' <param name="I_FIELDNAME"></param>
    ''' <param name="I_DATATYPE"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_RESULT"></param>
    Public Shared Function DataConvert(ByVal I_FIELDNAME As String,
                               ByVal I_VALUE As String,
                               ByVal I_DATATYPE As String,
                               ByRef O_RESULT As Boolean,
                               ByRef O_MESSAGE1 As String,
                               ByRef O_MESSAGE2 As String) As Object
        O_RESULT = True
        Dim WW_VALUE As String
        Dim WWInt As Integer
        Dim WWDecimal As Decimal
        Dim WWdt As DateTime

        DataConvert = I_VALUE
        Select Case I_DATATYPE
            Case "String" '文字型は変換の必要がないので何もしない
            Case "Int32" '数値型(小数点含まない)
                '""の場合"0"をセット
                If I_VALUE = "" Then
                    DataConvert = "0"
                Else
                    '数値に変換できる場合
                    If Integer.TryParse(I_VALUE, WWInt) Then
                        DataConvert = WWInt
                        '数値に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = "0"
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "数値形式で入力してください。(小数点不可)"
                    End If
                End If
            Case "Decimal" '数値型(小数点含む)
                '""の場合"0"をセット
                If I_VALUE = "" Then
                    DataConvert = "0"
                Else
                    '数値に変換できる場合
                    If Decimal.TryParse(I_VALUE, WWDecimal) Then
                        DataConvert = WWDecimal
                        '数値に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = "0"
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "数値形式で入力してください。(小数点可)"
                    End If
                End If
            Case "DateTime" '日付型
                '""の場合最小値の日付をセット
                If I_VALUE = "" Then
                    DataConvert = Date.MinValue
                Else
                    'シリアル値の場合日付型に変換
                    WW_VALUE = DateConvert(I_VALUE)
                    '日付に変換できる場合
                    If DateTime.TryParse(WW_VALUE, WWdt) Then
                        DataConvert = WWdt
                        '日付に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = Date.MinValue
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "日付形式(yyyy/MM/dd)で入力してください。"
                    End If
                End If
        End Select
    End Function

    ''' <summary>
    ''' ユーザーパスワードポリシーチェック
    ''' </summary>
    ''' <param name="I_userPassWd">ユーザーパスワード</param>
    ''' <remarks></remarks>
    Public Shared Function ChkUserPassword(ByVal I_userPassWd As String, ByRef errMsg As String) As Boolean

        errMsg = "パスワードは「英字大文字・小文字・数字・記号を含む12文字以上30文字以下」で設定してください。"

        '○文字数チェック(12文字以上)
        If I_userPassWd.Count < 12 Then
            'errMsg = "文字数が12文字以上ではありません。"
            Return False
        End If

        'Dim aaa As String = "^[a-zA-Z0-9!-/:-@?[-`{-~]+$"
        '○数字チェック(含まれているか)
        Dim chkNum As String = "[0-9]"
        If Regex.IsMatch(I_userPassWd, chkNum) = False Then
            'errMsg = "数字が含まれておりません。"
            Return False
        End If

        '○大文字(英字)チェック(含まれているか)
        Dim chkUpper As String = "[A-Z]"
        If Regex.IsMatch(I_userPassWd, chkUpper) = False Then
            'errMsg = "大文字(英字)が含まれておりません。"
            Return False
        End If

        '○小文字(英字)チェック(含まれているか)
        Dim chkLower As String = "[a-z]"
        If Regex.IsMatch(I_userPassWd, chkLower) = False Then
            'errMsg = "小文字(英字)が含まれておりません。"
            Return False
        End If

        '○記号チェック(含まれているか)
        Dim chkSymbol As String = "[!-/:-@?[-`{-~]"
        '★数値を取り除いてから記号チェックを実施
        Dim symbolPassWd As String = I_userPassWd
        For i As Integer = 0 To 9
            symbolPassWd = symbolPassWd.Replace(i.ToString(), "")
        Next
        If Regex.IsMatch(symbolPassWd, chkSymbol) = False Then
            'errMsg = "記号が含まれておりません。"
            Return False
        End If

        '○文字数チェック(30文字以内)
        If I_userPassWd.Count > 30 Then
            'errMsg = "文字数は30文字以内でおねがいします。"
            Return False
        End If

        errMsg = ""
        Return True
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
            & "     COM.LNS0001_USER                        " _
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

                    Dim LNS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0001Chk.Load(SQLdr)

                    If LNS0001Chk.Rows.Count > 0 Then
                        Dim LNS0001row As DataRow
                        LNS0001row = LNS0001Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNS0001row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNS0001row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D UPDATE_INSERT"
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