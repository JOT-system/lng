Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0013WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0013S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0013L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0013D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0013H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "5"   'タイトル区分

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
        BIGCTNCD   '大分類コード
        BIGCTNNM   '大分類名称
        MIDDLECTNCD   '中分類コード
        MIDDLECTNNM   '中分類名称
        PRIORITYNO   '優先順位
        DEPSTATION   '発駅コード
        DEPSTATIONNM   '発駅名称
        JRDEPBRANCHCD   'ＪＲ発支社支店コード
        JRDEPBRANCHNM   'ＪＲ発支社支店名称
        ARRSTATION   '着駅コード
        ARRSTATIONNM   '着駅名称
        JRARRBRANCHCD   'ＪＲ着支社支店コード
        JRARRBRANCHNM    'ＪＲ着支社支店名称
        PURPOSE   '使用目的
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEESUBCD   '発受託人サブコード
        CTNTYPE   'コンテナ記号
        CTNSTNO   'コンテナ番号（開始）
        CTNENDNO   'コンテナ番号（終了）
        SPRCURSTYMD   '特例置換項目-現行開始適用日
        SPRCURENDYMD   '特例置換項目-現行終了摘要日
        SPRCURAPPLYRATE   '特例置換項目-現行摘要率
        SPRCURROUNDKBN   '特例置換項目-現行端数処理区分
        SPRNEXTSTYMD   '特例置換項目-次期開始適用日
        SPRNEXTENDYMD   '特例置換項目-次期終了摘要日
        SPRNEXTAPPLYRATE   '特例置換項目-次期摘要率
        SPRNEXTROUNDKBN   '特例置換項目-次期端数処理区分
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        OPERATEKBNNAME   '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        BIGCTNCD   '大分類コード
        MIDDLECTNCD   '中分類コード
        PRIORITYNO   '優先順位
        DEPSTATION   '発駅コード
        JRDEPBRANCHCD   'ＪＲ発支社支店コード
        ARRSTATION   '着駅コード
        JRARRBRANCHCD   'ＪＲ着支社支店コード
        PURPOSE   '使用目的
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEESUBCD   '発受託人サブコード
        CTNTYPE   'コンテナ記号
        CTNSTNO   'コンテナ番号（開始）
        CTNENDNO   'コンテナ番号（終了）
        SPRCURSTYMD   '特例置換項目-現行開始適用日
        SPRCURENDYMD   '特例置換項目-現行終了摘要日
        SPRCURAPPLYRATE   '特例置換項目-現行摘要率
        SPRCURROUNDKBN   '特例置換項目-現行端数処理区分
        SPRNEXTSTYMD   '特例置換項目-次期開始適用日
        SPRNEXTENDYMD   '特例置換項目-次期終了摘要日
        SPRNEXTAPPLYRATE   '特例置換項目-次期摘要率
        SPRNEXTROUNDKBN   '特例置換項目-次期端数処理区分
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

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
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

    '' <summary>
    '' 大中小分類コード取得パラメータ設定
    '' </summary>
    '' <param name="I_BIGCTNCD"></param>
    '' <param name="I_MIDDLECTNCD"></param>
    '' <param name="CLASSTYPE_FLG"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateClassParam(ByVal CLASSTYPE_FLG As Integer, Optional ByVal I_BIGCTNCD As String = "", Optional ByVal I_MIDDLECTNCD As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CLASSTYPE_FLG
        WW_PrmData.Item(C_PARAMETERS.LP_BIGCTNCD) = I_BIGCTNCD
        WW_PrmData.Item(C_PARAMETERS.LP_MIDDLECTNCD) = I_MIDDLECTNCD

        CreateClassParam = WW_PrmData


    End Function

    ''' <summary>
    ''' 組織コード取得パラメーター設定
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
    ''' 受託人コード取得パラメーター設定
    ''' </summary>
    ''' <param name="CODETYPE_FLG"></param>
    ''' <param name="I_STATION"></param>
    ''' <param name="I_TRUSTEECD"></param>
    ''' <returns></returns>
    Function CreateTrusteeCdParam(ByVal CODETYPE_FLG As Integer, ByVal I_STATION As String, Optional ByVal I_TRUSTEECD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = CODETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_STATION) = I_STATION
        prmData.Item(C_PARAMETERS.LP_TRUSTEECD) = I_TRUSTEECD

        CreateTrusteeCdParam = prmData

    End Function

    ''' <summary>
    ''' コンテナ記号・番号取得パラメーター設定
    ''' </summary>
    ''' <param name="CNTENATYPE_FLG"></param>
    ''' <param name="I_CTNTYPE"></param>
    ''' <returns></returns>
    Public Function CreateContenaParam(ByVal CNTENATYPE_FLG As Integer, Optional ByVal I_CTNTYPE As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_CTNTYPE) = I_CTNTYPE
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CNTENATYPE_FLG

        CreateContenaParam = WW_PrmData

    End Function

    '' <summary>
    '' ロールマスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateRoleList(ByVal I_COMPCODE As String, ByVal I_OBJCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_CLASSCODE) = I_OBJCODE
        CreateRoleList = prmData
    End Function

    ''' <summary>
    ''' 駅コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_ORGCODE"></param>
    ''' <returns></returns>
    Function CreateStationParam(ByVal I_COMPCODE As String, Optional ByVal I_ORGCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = I_ORGCODE
        CreateStationParam = prmData
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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="BIGCTNCD">大分類コード</param>
    ''' <param name="MIDDLECTNCD">中分類コード</param>
    ''' <param name="PRIORITYNO">優先順位</param>
    ''' <param name="DEPSTATION">発駅コード</param>
    ''' <param name="JRDEPBRANCHCD">JR発支社支店コード</param>
    ''' <param name="ARRSTATION">着駅コード</param>
    ''' <param name="JRARRBRANCHCD">JR着支社支店コード</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                             ByRef BIGCTNCD As String, ByRef MIDDLECTNCD As String,
                             ByRef PRIORITYNO As String, ByRef DEPSTATION As String,
                             ByRef JRDEPBRANCHCD As Integer, ByRef ARRSTATION As String,
                             ByRef JRARRBRANCHCD As Integer, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     BIGCTNCD                                " _
            & "   , MIDDLECTNCD                             " _
            & "   , PRIORITYNO                              " _
            & "   , DEPSTATION                              " _
            & "   , JRDEPBRANCHCD                           " _
            & "   , ARRSTATION                              " _
            & "   , JRARRBRANCHCD                           " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0013_REKTRM                      " _
            & " WHERE                                       " _
            & "         BIGCTNCD        = @P1               " _
            & "     AND MIDDLECTNCD     = @P2               " _
            & "     AND PRIORITYNO      = @P3               " _
            & "     AND DEPSTATION      = @P4               " _
            & "     AND JRDEPBRANCHCD   = @P5               " _
            & "     AND ARRSTATION      = @P6               " _
            & "     AND JRARRBRANCHCD   = @P7               "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 5) '優先順位
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 6) '着駅コード
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.Int32)         'ＪＲ着支社支店コード

                PARA1.Value = BIGCTNCD
                PARA2.Value = MIDDLECTNCD
                PARA3.Value = PRIORITYNO
                PARA4.Value = DEPSTATION
                PARA5.Value = JRDEPBRANCHCD
                PARA6.Value = ARRSTATION
                PARA7.Value = JRARRBRANCHCD

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0013Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0013Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0013Chk.Load(SQLdr)

                    If LNM0013Chk.Rows.Count > 0 Then
                        Dim LNM0013row As DataRow
                        LNM0013row = LNM0013Chk.Rows(0)
                        If Not LNM0013row("UPDTIMSTP").ToString = "" Then                                 'タイムスタンプ
                            If LNM0013row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0013D HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class