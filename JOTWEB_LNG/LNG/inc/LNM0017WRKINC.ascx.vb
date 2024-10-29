Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0017WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0017S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0017L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0017D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0017H"       'MAPID(履歴)
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
        ORGCODE   '組織コード
        ORGNAME   '組織名称
        BIGCTNCD   '大分類コード
        BIGCTNNM   '大分類名称
        MIDDLECTNCD   '中分類コード
        MIDDLECTNNM   '中分類名称
        PURPOSE   '使用目的
        SPRUSEFEE   '特例置換項目-使用料金額
        SPRUSEFEERATE   '特例置換項目-使用料率
        SPRUSEFEERATEROUND   '特例置換項目-使用料率端数整理
        SPRUSEFEERATEADDSUB   '特例置換項目-使用料率加減額
        SPRUSEFEERATEADDSUBCOND   '特例置換項目-使用料率加減額端数整理
        SPRROUNDPOINTKBN   '特例置換項目-端数処理時点区分
        SPRUSEFREESPE   '特例置換項目-使用料無料特認
        SPRNITTSUFREESENDFEE   '特例置換項目-通運負担回送運賃
        SPRMANAGEFEE   '特例置換項目-運行管理料
        SPRSHIPBURDENFEE   '特例置換項目-荷主負担運賃
        SPRSHIPFEE   '特例置換項目-発送料
        SPRARRIVEFEE   '特例置換項目-到着料
        SPRPICKUPFEE   '特例置換項目-集荷料
        SPRDELIVERYFEE   '特例置換項目-配達料
        SPROTHER1   '特例置換項目-その他１
        SPROTHER2   '特例置換項目-その他２
        SPRFITKBN   '特例置換項目-適合区分
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
        ORGCODE   '組織コード
        BIGCTNCD   '大分類コード
        MIDDLECTNCD   '中分類コード
        PURPOSE   '使用目的
        SPRUSEFEE   '特例置換項目-使用料金額
        SPRUSEFEERATE   '特例置換項目-使用料率
        SPRUSEFEERATEROUND   '特例置換項目-使用料率端数整理
        SPRUSEFEERATEADDSUB   '特例置換項目-使用料率加減額
        SPRUSEFEERATEADDSUBCOND   '特例置換項目-使用料率加減額端数整理
        SPRROUNDPOINTKBN   '特例置換項目-端数処理時点区分
        SPRUSEFREESPE   '特例置換項目-使用料無料特認
        SPRNITTSUFREESENDFEE   '特例置換項目-通運負担回送運賃
        SPRMANAGEFEE   '特例置換項目-運行管理料
        SPRSHIPBURDENFEE   '特例置換項目-荷主負担運賃
        SPRSHIPFEE   '特例置換項目-発送料
        SPRARRIVEFEE   '特例置換項目-到着料
        SPRPICKUPFEE   '特例置換項目-集荷料
        SPRDELIVERYFEE   '特例置換項目-配達料
        SPROTHER1   '特例置換項目-その他１
        SPROTHER2   '特例置換項目-その他２
        SPRFITKBN   '特例置換項目-適合区分
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
    ''' 大中小分類コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="I_BIGCTNCD"></param>
    ''' <param name="I_MIDDLECTNCD"></param>
    ''' <param name="CLASSTYPE_FLG"></param>
    ''' <returns></returns>
    Public Function CreateClassParam(ByVal CLASSTYPE_FLG As Integer, Optional ByVal I_BIGCTNCD As String = "", Optional ByVal I_MIDDLECTNCD As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CLASSTYPE_FLG
        WW_PrmData.Item(C_PARAMETERS.LP_BIGCTNCD) = I_BIGCTNCD
        WW_PrmData.Item(C_PARAMETERS.LP_MIDDLECTNCD) = I_MIDDLECTNCD

        CreateClassParam = WW_PrmData

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
        Dim WWInt As Integer
        Dim WWDecimal As Decimal

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
        End Select
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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="BIGCTNCD">大分類コード</param>
    ''' <param name="MIDDLECTNCD">中分類コード</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                             ByRef ORGCODE As String, ByRef BIGCTNCD As String,
                             ByRef MIDDLECTNCD As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     ORGCODE                                 " _
            & "   , BIGCTNCD                                " _
            & "   , MIDDLECTNCD                             " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0017_REST2M                      " _
            & " WHERE                                       " _
            & "         ORGCODE      = @P1                  " _
            & "     AND BIGCTNCD     = @P2                  " _
            & "     AND MIDDLECTNCD  = @P3                  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '組織コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '中分類コード

                PARA1.Value = ORGCODE      '組織コード
                PARA2.Value = BIGCTNCD     '大分類コード
                PARA3.Value = MIDDLECTNCD  '中分類コード

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0017Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0017Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0017Chk.Load(SQLdr)

                    If LNM0017Chk.Rows.Count > 0 Then
                        Dim LNM0017row As DataRow
                        LNM0017row = LNM0017Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0017row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0017row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0017C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class