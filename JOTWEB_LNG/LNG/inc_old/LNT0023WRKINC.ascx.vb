Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0023WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNT0023S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNT0023L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNT0023D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNT0023H"       'MAPID(履歴)
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
        TORICODE   '支払先コード
        CLIENTCODE   '顧客コード
        INVOICENUMBER   'インボイス登録番号
        CLIENTNAME   '顧客名
        TORINAME   '会社名
        TORIDIVNAME   '部門名
        PAYBANKCODE   '振込先銀行コード
        PAYBANKNAME   '振込先銀行名
        PAYBANKNAMEKANA   '振込先銀行名カナ
        PAYBANKBRANCHCODE   '振込先支店コード
        PAYBANKBRANCHNAME   '振込先支店名
        PAYBANKBRANCHNAMEKANA   '振込先支店名カナ
        PAYACCOUNTTYPENAME   '預金種別
        PAYACCOUNTTYPE   '預金種別コード
        PAYACCOUNT   '口座番号
        PAYACCOUNTNAME   '口座名義
        PAYORBANKCODE   '支払元銀行コード
        PAYTAXCALCUNIT   '消費税計算処理区分
        LINKSTATUS   '連携状態区分
        LASTLINKYMD   '最終連携日
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
        TORICODE   '支払先コード
        CLIENTCODE   '顧客コード
        INVOICENUMBER   'インボイス登録番号
        CLIENTNAME   '顧客名
        TORINAME   '会社名
        TORIDIVNAME   '部門名
        PAYBANKCODE   '振込先銀行コード
        PAYBANKNAME   '振込先銀行名
        PAYBANKNAMEKANA   '振込先銀行名カナ
        PAYBANKBRANCHCODE   '振込先支店コード
        PAYBANKBRANCHNAME   '振込先支店名
        PAYBANKBRANCHNAMEKANA   '振込先支店名カナ
        PAYACCOUNTTYPENAME   '預金種別
        PAYACCOUNTTYPE   '預金種別コード
        PAYACCOUNT   '口座番号
        PAYACCOUNTNAME   '口座名義
        PAYORBANKCODE   '支払元銀行コード
        PAYTAXCALCUNIT   '消費税計算処理区分
        LINKSTATUS   '連携状態区分
        LASTLINKYMD   '最終連携日
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
    ''' 名称取得(会社名、顧客名)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_TORINAMEht">会社名格納HT</param>
    ''' <param name="O_CLIENTNAMEht">顧客名格納HT</param>
    Public Sub CODENAMEGetPAYEE(ByVal SQLcon As MySqlConnection,
                                ByRef O_TORINAMEht As Hashtable,
                                ByRef O_CLIENTNAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT")
        SQLStr.AppendLine("       TORICODE AS TORICODE")
        SQLStr.AppendLine("      ,CLIENTCODE AS CLIENTCODE")
        SQLStr.AppendLine("      ,RTRIM(CLIENTNAME) AS CLIENTNAME")
        SQLStr.AppendLine("      ,RTRIM(TORINAME) AS TORINAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     lng.LNT0072_PAYEE")
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
                    '支払先コード、会社名格納
                    If Not O_TORINAMEht.ContainsKey(WW_Row("TORICODE")) Then
                        O_TORINAMEht.Add(WW_Row("TORICODE"), WW_Row("TORINAME"))
                    End If
                    '顧客コード、顧客名格納
                    If Not O_CLIENTNAMEht.ContainsKey(WW_Row("CLIENTCODE")) Then
                        O_CLIENTNAMEht.Add(WW_Row("CLIENTCODE"), WW_Row("CLIENTNAME"))
                    End If
                Next
            End Using
        Catch ex As Exception
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

    ''' <summary>
    ''' 名称取得(社内口座銀行名、社内口座銀行カナ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">社内口座銀行名格納HT</param>
    Public Sub CODENAMEGetBANKACCOUNT(ByVal SQLcon As MySqlConnection,
                                      ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT")
        SQLStr.AppendLine("       A.BANKCODE AS BANKCODE")
        SQLStr.AppendLine("      ,RTRIM(B.BANKNAME) AS BANKNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("    (")
        SQLStr.AppendLine("       SELECT DISTINCT")
        SQLStr.AppendLine("             BANKCODE")
        SQLStr.AppendLine("            ,BANKBRANCHCODE")
        SQLStr.AppendLine("       FROM")
        SQLStr.AppendLine("           com.LNS0023_BANKACCOUNT")
        SQLStr.AppendLine("       WHERE")
        SQLStr.AppendLine("           DELFLG = '0'")
        SQLStr.AppendLine("    ) A")
        SQLStr.AppendLine(" LEFT JOIN ")
        SQLStr.AppendLine("    com.LNS0022_BANK B ")
        SQLStr.AppendLine("   ON A.BANKCODE = B.BANKCODE ")
        SQLStr.AppendLine("   AND A.BANKBRANCHCODE = B.BANKBRANCHCODE ")
        SQLStr.AppendLine("   AND B.DELFLG = '0' ")

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
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="TORICODE">支払先コード</param>
    ''' <param name="CLIENTCODE">顧客コード</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                             ByRef TORICODE As String, ByRef CLIENTCODE As String,
                             ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     TORICODE                                " _
            & "   , CLIENTCODE                              " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNT0072_PAYEE                       " _
            & " WHERE                                       " _
            & "         TORICODE      = @TORICODE           " _
            & "     AND CLIENTCODE      = @CLIENTCODE       "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15) '顧客コード

                P_TORICODE.Value = TORICODE     '支払先コード
                P_CLIENTCODE.Value = CLIENTCODE     '顧客コード

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNT0023Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0023Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0023Chk.Load(SQLdr)

                    If LNT0023Chk.Rows.Count > 0 Then
                        Dim LNT0023row As DataRow
                        LNT0023row = LNT0023Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNT0023row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNT0023row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNT0023C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class