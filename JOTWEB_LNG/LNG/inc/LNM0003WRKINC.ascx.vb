Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0003WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0003S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0003L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0003D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0003H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

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
        DEPSTATION   '発駅コード
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEESUBCD   '発受託人サブコード
        DEPTRUSTEENM   '発受託人名称
        DEPTRUSTEESUBNM   '発受託人サブ名称
        DEPTRUSTEESUBKANA   '発受託人名称（カナ）
        TORICODE   '取引先コード
        ELIGIBLEINVOICENUMBER   '適格請求書登録番号
        INVKEIJYOBRANCHCD   '請求項目 計上店コード
        INVCYCL   '請求項目 請求サイクル
        INVFILINGDEPT   '請求項目 請求書提出部店
        INVKESAIKBN   '請求項目 請求書決済区分
        INVSUBCD   '請求項目 請求書細分コード
        PAYKEIJYOBRANCHCD   '支払項目 費用計上店コード
        PAYFILINGBRANCH   '支払項目 支払書提出支店
        TAXCALCUNIT   '支払項目 消費税計算単位
        PAYKESAIKBN   '支払項目 決済区分
        PAYBANKCD   '支払項目 銀行コード
        PAYBANKBRANCHCD   '支払項目 銀行支店コード
        PAYACCOUNTTYPE   '支払項目 口座種別
        PAYACCOUNTNO   '支払項目 口座番号
        PAYACCOUNTNM   '支払項目 口座名義人
        PAYTEKIYO   '支払項目 支払摘要
        BEFOREINVKEIJYOBRANCHCD   '変換前 請求項目 計上店コード
        BEFOREINVFILINGDEPT   '変換前 請求項目 請求書提出部店
        BEFOREPAYKEIJYOBRANCHCD   '変換前 支払項目 費用計上店コード
        BEFOREPAYFILINGBRANCH   '変換前 支払項目 支払書提出支店
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
        DEPSTATION   '発駅コード
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEESUBCD   '発受託人サブコード
        DEPTRUSTEENM   '発受託人名称
        DEPTRUSTEESUBNM   '発受託人サブ名称
        DEPTRUSTEESUBKANA   '発受託人名称（カナ）
        TORICODE   '取引先コード
        ELIGIBLEINVOICENUMBER   '適格請求書登録番号
        INVKEIJYOBRANCHCD   '請求項目 計上店コード
        INVCYCL   '請求項目 請求サイクル
        INVFILINGDEPT   '請求項目 請求書提出部店
        INVKESAIKBN   '請求項目 請求書決済区分
        INVSUBCD   '請求項目 請求書細分コード
        PAYKEIJYOBRANCHCD   '支払項目 費用計上店コード
        PAYFILINGBRANCH   '支払項目 支払書提出支店
        TAXCALCUNIT   '支払項目 消費税計算単位
        PAYKESAIKBN   '支払項目 決済区分
        PAYBANKCD   '支払項目 銀行コード
        PAYBANKBRANCHCD   '支払項目 銀行支店コード
        PAYACCOUNTTYPE   '支払項目 口座種別
        PAYACCOUNTNO   '支払項目 口座番号
        PAYACCOUNTNM   '支払項目 口座名義人
        PAYTEKIYO   '支払項目 支払摘要
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
    ''' 受託人コードの取得
    ''' </summary>
    ''' <param name="CODETYPE_FLG"></param>
    ''' <param name="I_STATION"></param>
    ''' <param name="I_TRUSTEECD"></param>
    ''' <returns></returns>
    Function CreateDepTrusteeCdParam(ByVal CODETYPE_FLG As Integer, ByVal I_STATION As String, Optional ByVal I_TRUSTEECD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = CODETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_STATION) = I_STATION
        prmData.Item(C_PARAMETERS.LP_TRUSTEECD) = I_TRUSTEECD

        CreateDepTrusteeCdParam = prmData

    End Function

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
    ''' 請求項目請求書細分コードの取得
    ''' </summary>
    ''' <param name="I_TORICODE"></param>
    ''' <param name="I_INVFILINGDEPT"></param>
    ''' <param name="I_INVKESAIKBN"></param>
    ''' <returns></returns>
    Function CreateInvSubCdParam(ByVal I_TORICODE As String, ByVal I_INVFILINGDEPT As String, ByVal I_INVKESAIKBN As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_TORICODE
        prmData.Item(C_PARAMETERS.LP_INVFILINGDEPT) = I_INVFILINGDEPT
        prmData.Item(C_PARAMETERS.LP_INVKESAIKBN) = I_INVKESAIKBN

        CreateInvSubCdParam = prmData

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
    ''' 組織コードパラメーター
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
    ''' 駅マスタから一覧の取得
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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="DEPSTATION">発駅コード</param>
    ''' <param name="DEPTRUSTEECD">発受託人コード</param>
    ''' <param name="DEPTRUSTEESUBCD">発受託人サブコード</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                          ByRef DEPSTATION As String, ByRef DEPTRUSTEECD As String,
                          ByRef DEPTRUSTEESUBCD As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     DEPSTATION                              " _
            & "   , DEPTRUSTEECD                            " _
            & "   , DEPTRUSTEESUBCD                         " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0003_REKEJM                      " _
            & " WHERE                                       " _
            & "         DEPSTATION       = @P1              " _
            & "     AND DEPTRUSTEECD     = @P2              " _
            & "     AND DEPTRUSTEESUBCD  = @P3              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 3) '発受託人サブコード

                PARA1.Value = DEPSTATION
                PARA2.Value = DEPTRUSTEECD
                PARA3.Value = DEPTRUSTEESUBCD

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0003Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0003Chk.Load(SQLdr)

                    If LNM0003Chk.Rows.Count > 0 Then
                        Dim LNM0003row As DataRow
                        LNM0003row = LNM0003Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0003row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0003row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0003C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 名称取得(銀行名、銀行カナ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">銀行名格納HT</param>
    ''' <param name="O_KANAht">銀行カナ格納HT</param>
    Public Sub CODENAMEGetBANK(ByVal SQLcon As MySqlConnection,
                               ByRef O_NAMEht As Hashtable,
                               ByRef O_KANAht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       BANKCODE AS BANKCODE")
        SQLStr.AppendLine("      ,RTRIM(BANKNAME) AS BANKNAME")
        SQLStr.AppendLine("      ,RTRIM(BANKNAMEKANA) AS BANKNAMEKANA")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     com.LNS0022_BANK")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("     DELFLG = '0'")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using
                'ハッシュテーブルにコードと名称を格納
                For Each WW_Row As DataRow In WW_Tbl.Rows
                    '銀行コード、銀行名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("BANKCODE")) Then
                        O_NAMEht.Add(WW_Row("BANKCODE"), WW_Row("BANKNAME"))
                    End If
                    '銀行コード、銀行カナ格納
                    If Not O_KANAht.ContainsKey(WW_Row("BANKCODE")) Then
                        O_KANAht.Add(WW_Row("BANKCODE"), WW_Row("BANKNAMEKANA"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 名称取得(支店名、支店カナ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_BANKCODE">銀行コード</param>
    ''' <param name="O_NAMEht">支店名格納HT</param>
    ''' <param name="O_KANAht">支店カナ格納HT</param>
    Public Sub CODENAMEGetBANKBRANCH(ByVal SQLcon As MySqlConnection,
                                     ByVal I_BANKCODE As String,
                                     ByRef O_NAMEht As Hashtable,
                                     ByRef O_KANAht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       BANKCODE AS BANKCODE")
        SQLStr.AppendLine("      ,RTRIM(BANKNAME) AS BANKNAME")
        SQLStr.AppendLine("      ,BANKBRANCHCODE AS BANKBRANCHCODE")
        SQLStr.AppendLine("      ,RTRIM(BANKBRANCHNAME) AS BANKBRANCHNAME")
        SQLStr.AppendLine("      ,RTRIM(BANKBRANCHNAMEKANA) AS BANKBRANCHNAMEKANA")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     com.LNS0022_BANK")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("     DELFLG = '0'")
        SQLStr.AppendLine("    AND BANKCODE = '" & I_BANKCODE & "'")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using
                'ハッシュテーブルにコードと名称を格納
                For Each WW_Row As DataRow In WW_Tbl.Rows
                    '支店コード、支店名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("BANKBRANCHCODE")) Then
                        O_NAMEht.Add(WW_Row("BANKBRANCHCODE"), WW_Row("BANKBRANCHNAME"))
                    End If
                    '支店コード、支店カナ格納
                    If Not O_KANAht.ContainsKey(WW_Row("BANKBRANCHCODE")) Then
                        O_KANAht.Add(WW_Row("BANKBRANCHCODE"), WW_Row("BANKBRANCHNAMEKANA"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

End Class