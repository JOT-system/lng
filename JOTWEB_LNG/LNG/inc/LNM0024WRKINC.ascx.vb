Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0024WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0024S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0024L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0024D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0024H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "D"   'タイトル区分

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
        TORICODE   '取引先コード
        INVFILINGDEPT   '請求書提出部店
        INVFILINGDEPTNM   '請求書提出部店名称
        INVKESAIKBN   '請求書決済区分
        TORINAME   '取引先名称
        TORINAMES   '取引先略称
        TORINAMEKANA   '取引先カナ名称
        TORIDIVNAME   '取引先部門名称
        TORICHARGE   '取引先担当者
        TORIKBN   '取引先区分
        POSTNUM1   '郵便番号（上）
        POSTNUM2   '郵便番号（下）
        ADDR1   '住所1
        ADDR2   '住所2
        ADDR3   '住所3
        ADDR4   '住所4
        TEL   '電話番号
        FAX   'FAX番号
        MAIL   'メールアドレス
        BANKCODE   '銀行コード
        BANKBRANCHCODE   '支店コード
        ACCOUNTTYPE   '口座種別
        ACCOUNTNUMBER   '口座番号
        ACCOUNTNAME   '口座名義
        INACCOUNTCD   '社内口座コード
        TAXCALCULATION   '税計算区分
        ACCOUNTINGMONTH   '計上月区分
        DEPOSITDAY   '入金日
        DEPOSITMONTHKBN   '入金月区分
        CLOSINGDAY   '計上締日
        SLIPDESCRIPTION1   '伝票摘要1
        SLIPDESCRIPTION2   '伝票摘要2
        NEXTMONTHUNSETTLEDKBN   '運賃翌日未決済区分
        BEFOREINVFILINGDEPT   '変換前請求書提出部店
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
        TORICODE   '取引先コード
        INVFILINGDEPT   '請求書提出部店
        INVKESAIKBN   '請求書決済区分
        TORINAME   '取引先名称
        TORINAMES   '取引先略称
        TORINAMEKANA   '取引先カナ名称
        TORIDIVNAME   '取引先部門名称
        TORICHARGE   '取引先担当者
        TORIKBN   '取引先区分
        POSTNUM1   '郵便番号（上）
        POSTNUM2   '郵便番号（下）
        ADDR1   '住所1
        ADDR2   '住所2
        ADDR3   '住所3
        ADDR4   '住所4
        TEL   '電話番号
        FAX   'FAX番号
        MAIL   'メールアドレス
        BANKCODE   '銀行コード
        BANKBRANCHCODE   '支店コード
        ACCOUNTTYPE   '口座種別
        ACCOUNTNUMBER   '口座番号
        ACCOUNTNAME   '口座名義
        INACCOUNTCD   '社内口座コード
        TAXCALCULATION   '税計算区分
        DEPOSITDAY   '入金日
        DEPOSITMONTHKBN   '入金月区分
        CLOSINGDAY   '計上締日
        SLIPDESCRIPTION1   '伝票摘要1
        SLIPDESCRIPTION2   '伝票摘要2
        NEXTMONTHUNSETTLEDKBN   '運賃翌日未決済区分
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
    ''' 営業収入決済条件マスタ項目取得
    ''' </summary>
    ''' <param name="KEKKJMTYPE_FLG"></param>
    ''' <param name="I_TORICODE"></param>
    ''' <param name="I_INVFILINGDEPT"></param>
    ''' <returns></returns>
    Function CreateKekkjmParam(ByVal KEKKJMTYPE_FLG As Integer, Optional ByVal I_TORICODE As String = "", Optional ByVal I_INVFILINGDEPT As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = KEKKJMTYPE_FLG
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_TORICODE
        prmData.Item(C_PARAMETERS.LP_INVFILINGDEPT) = I_INVFILINGDEPT

        CreateKekkjmParam = prmData

    End Function
    ''' <summary>
    ''' 支店パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateUORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateUORGParam = prmData

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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                          ByRef TORICODE As String, ByRef INVFILINGDEPT As String,
                          ByRef INVKESAIKBN As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     TORICODE                                " _
            & "   , INVFILINGDEPT                           " _
            & "   , INVKESAIKBN                             " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0024_KEKKJM                      " _
            & " WHERE                                       " _
            & "         TORICODE        = @P1               " _
            & "     AND INVFILINGDEPT   = @P3               " _
            & "     AND INVKESAIKBN     = @P4               "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10) '取引先コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 6)  '請求書提出部店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 2)  '請求書決済区分

                PARA1.Value = TORICODE
                PARA3.Value = INVFILINGDEPT
                PARA4.Value = INVKESAIKBN

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0024Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0024Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0024Chk.Load(SQLdr)

                    If LNM0024Chk.Rows.Count > 0 Then
                        Dim LNM0024row As DataRow
                        LNM0024row = LNM0024Chk.Rows(0)
                        If Not LNM0024row("UPDTIMSTP").ToString = "" Then                                 'タイムスタンプ
                            If LNM0024row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0024D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub
End Class