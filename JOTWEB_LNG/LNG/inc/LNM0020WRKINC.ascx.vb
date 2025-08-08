Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0020WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNM0020L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0020D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0020H"       'MAPID(履歴)

    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    Public Const MAX_ENDYMD As String = "2099/12/31"

    ''' <summary>
    ''' ファイルタイプ
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum FILETYPE
        EXCEL
        PDF
    End Enum

    ''' <summary>
    ''' 入出力項目位置(固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOL
        DELFLG   '削除フラグ
        DIESELPRICESITEID   '実勢軽油価格参照先ID
        DIESELPRICESITEBRANCH   '実勢軽油価格参照先ID枝番
        DIESELPRICESITENAME   '実勢軽油価格参照先名
        DIESELPRICESITEKBNNAME   '実勢軽油価格参照先区分名
        DISPLAYNAME   '画面表示名称
        DIESELPRICESITEURL   '実勢軽油価格参照先URL
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置(固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        DIESELPRICESITEID   '実勢軽油価格参照先ID
        DIESELPRICESITEBRANCH   '実勢軽油価格参照先ID枝番
        DIESELPRICESITENAME   '実勢軽油価格参照先名
        DIESELPRICESITEKBNNAME   '実勢軽油価格参照先区分名
        DISPLAYNAME   '画面表示名称
        DIESELPRICESITEURL   '実勢軽油価格参照先URL
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
    ''' 管理権限のある組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function AdminCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("011308", "情報システム部")
        WW_HT.Add("011310", "高圧ガス１部")

        Return WW_HT.ContainsKey(I_ORG)
    End Function

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
    ''' 排他チェック(軽油価格参照先マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_ID">ID</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_ID As String,
                          ByVal I_BRANCH As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0020_DIESELPRICESITE             ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("     DIESELPRICESITEID       = @ID           ")
        SQLStr.AppendLine(" AND DIESELPRICESITEBRANCH   = @BRANCH       ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.VarChar)          'ID
                Dim P_BRANCH As MySqlParameter = SQLcmd.Parameters.Add("@BRANCH", MySqlDbType.VarChar)  'BRANCH

                P_ID.Value = I_ID           'ID
                P_BRANCH.Value = I_BRANCH   'BRANCH

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0020Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0020Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0020Chk.Load(SQLdr)

                    If LNM0020Chk.Rows.Count > 0 Then
                        Dim LNM0020row As DataRow
                        LNM0020row = LNM0020Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0020row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0020row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_DIESELPRICESITE 排他"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 実勢軽油価格参照先ID(軽油価格参照先マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="O_ID">ID</param>
    Public Sub GetMaxID(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByRef O_ID As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
        O_ID = "01"

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                             ")
        SQLStr.AppendLine("    MAX(DIESELPRICESITEID) + 1 AS ID                ")
        SQLStr.AppendLine(" FROM LNG.LNM0020_DIESELPRICESITE                   ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0020Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0020Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0020Chk.Load(SQLdr)

                    O_ID = CInt(LNM0020Chk.Rows(0)("ID")).ToString("00")
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_HOLIDAYRATE select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            O_MESSAGENO = Messages.C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 実勢軽油価格参照先ID枝番(軽油価格参照先マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_ID"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="O_BRANCH">ID</param>
    Public Sub GetMaxBRANCH(ByVal SQLcon As MySqlConnection, ByVal I_ID As String, ByRef O_MESSAGENO As String, ByRef O_BRANCH As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
        O_BRANCH = "01"

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                             ")
        SQLStr.AppendLine("    MAX(DIESELPRICESITEBRANCH) + 1 AS BRANCH        ")
        SQLStr.AppendLine(" FROM LNG.LNM0020_DIESELPRICESITE                   ")
        SQLStr.AppendLine(" WHERE                                              ")
        SQLStr.AppendLine("     DIESELPRICESITEID       = @ID                  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.VarChar)          'ID

                P_ID.Value = I_ID           'ID

                Dim LNM0020Chk = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0020Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0020Chk.Load(SQLdr)

                    O_BRANCH = CInt(LNM0020Chk.Rows(0)("BRANCH")).ToString("00")
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_DIESELPRICESITE select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            O_MESSAGENO = Messages.C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class