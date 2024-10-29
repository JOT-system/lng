Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>経理連携用 キー</description></item>
''' </list>
''' </remarks>
Public Enum SELECT_ACCOUNTING_KEY
    SP_KEIJOYM              '請求年月(計上年月)
    SP_ZERIT                '消費税率
    SP_CREATEDATE           '作成日付
    SP_CREATETIME           '作成時間
    SP_USERID               'ユーザーID
    SP_USERTERMID           '登録端末
    SP_DELFLG               '削除フラグ
End Enum

''' <summary>
''' 請求ヘッダーデータ登録クラス
''' </summary>
''' <remarks>各種請求ヘッダーデータに登録する際はこちらに定義</remarks>
Public Class EntryAccountingData

#Region "経理連携 CSV取得前検索"
    ''' <summary>
    ''' 消費税率取得 検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="KeijoYM">取得対象計上年月</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectZERIT(sqlCon As MySqlConnection, KeijoYM As String) As String

        Dim dt = New DataTable
        Dim TaxRate As String = ""
        Try
            '◯データ検索SQL
            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT                                                            ")
            SQLBldr.AppendLine("    CASE                                                          ")
            SQLBldr.AppendLine("        WHEN NEXTFROMYMD > CAST('" & KeijoYM & "' + '01' AS DATE) ")
            SQLBldr.AppendLine("    THEN CURSETVAL1                                               ")
            SQLBldr.AppendLine("    ELSE NEXTSETVAL1                                              ")
            SQLBldr.AppendLine("    END                                AS ZERIT                   ")       '消費税率
            SQLBldr.AppendLine("FROM                                                              ")
            SQLBldr.AppendLine("    LNG.LNM0001_RECNTM                                            ")
            SQLBldr.AppendLine("WHERE                                                             ")
            SQLBldr.AppendLine("    CNTKEY = 'ZERIT'                                              ")
            SQLBldr.AppendLine("AND DELFLG = '0'                                                  ")

            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)

                'SQL実行
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
                For Each LNM0001tblrow As DataRow In dt.Rows
                    TaxRate = (CDec(CInt(LNM0001tblrow("ZERIT")) / 100)).ToString
                    Exit For
                Next

            End Using
        Catch ex As Exception
            Dim a As String
            a = ex.ToString()
        End Try

        '取得消費税率データ返却
        Return TaxRate

    End Function

#End Region

#Region "経理連携 検索処理(CSV用)"
    ''' <summary>
    ''' 経理連携データ 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectAccountingDataCsv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[CTN_GET_ACCOUNTINGDATA1]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTerm", MySqlDbType.VarChar, 20)         ' 登録端末
            Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERTERMID).ToString
            PARA4.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA5.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[CTN_GET_ACCOUNTINGDATA2]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTerm", MySqlDbType.VarChar, 20)         ' 登録端末
            Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERTERMID).ToString
            PARA4.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA5.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[CTN_GET_ACCOUNTINGDATA3]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTerm", MySqlDbType.VarChar, 20)         ' 登録端末
            Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERTERMID).ToString
            PARA4.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA5.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[CTN_GET_ACCOUNTINGDATA4]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTerm", MySqlDbType.VarChar, 20)         ' 登録端末
            Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERTERMID).ToString
            PARA4.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA5.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

#Region "経理連携 収入管理"
    ''' <summary>
    ''' レンタルデータ 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectRentalCsv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[GET_RENTAL_ACCOUNTINGDATA]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA1.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' リースデータ 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectLeaseCsv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        Using SQLcmd As New MySqlCommand
            SQLcmd.Connection = sqlCon
            SQLcmd.CommandType = CommandType.StoredProcedure

            SQLcmd.CommandText = "lng.[GET_LEASE_ACCOUNTINGDATA]"

            SQLcmd.Parameters.Clear()
            Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@TargetDateYM", MySqlDbType.VarChar, 6)        ' 対象年月
            Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@CreateUser", MySqlDbType.VarChar, 20)         ' 登録ユーザーＩＤ
            Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CreateDate", MySqlDbType.VarChar, 20)         ' 登録日付
            Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@CreateTime", MySqlDbType.VarChar, 20)         ' 登録時間

            PARA0.Value = htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString
            PARA1.Value = htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString
            PARA2.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString
            PARA3.Value = htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' その他販売収入・洗浄ヤード 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectSonotaCsv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
#Region "経理連携 その他販売収入・洗浄ヤード 検索処理SQL"
        ' 請求明細リーステーブル
        SQLBldr.AppendLine("SELECT                                                                                                ")
        SQLBldr.AppendLine("    0                                                               AS データ基準                     ")
        SQLBldr.AppendLine("   ,1002                                                            AS 仕訳形式入力                   ")
        SQLBldr.AppendLine("   ,11                                                              AS 入力画面番号                   ")
        SQLBldr.AppendLine("   ,DATE_FORMAT(LAST_DAY(                                                                             ")
        SQLBldr.AppendLine("    CAST(T1.KEIJOYM * 100 + 1 AS NVARCHAR)), '%Y/%m/%d')            AS 伝票日付                       ")
        SQLBldr.AppendLine("   ,0                                                               AS 決算月区分                     ")
        SQLBldr.AppendLine("   ,'AB'                                                            AS 証憑番号                       ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T4.SORT1), '00000000')                               AS 伝票番号                       ")
        SQLBldr.AppendLine("   ,MAX(T4.SORT1)                                                   AS 伝票No                         ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T5.SORT2), '000')                                    AS 明細行番号                     ")
        SQLBldr.AppendLine("   ,CASE                                                                                              ")
        SQLBldr.AppendLine("        WHEN T2.ACCOUNTCODE = '11030416' THEN '11030401'                                              ")
        SQLBldr.AppendLine("        ELSE '11010401'                                                                               ")
        SQLBldr.AppendLine("    END                                                             AS 借方科目                       ")
        SQLBldr.AppendLine("   ,CASE                                                                                              ")
        SQLBldr.AppendLine("        WHEN MAX(T2.INVOICEORGCODE) = '010101' THEN '010102'                                          ")
        SQLBldr.AppendLine("        ELSE MAX(T2.INVOICEORGCODE)                                                                   ")
        SQLBldr.AppendLine("    END                                                             AS 借方部門                       ")
        SQLBldr.AppendLine("   ,MAX(T1.INACCOUNTCD)                                             AS 借方銀行                       ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                     AS 借方取引先                     ")
        SQLBldr.AppendLine("   ,9                                                               AS 借方汎用補助1                  ")
        SQLBldr.AppendLine("   ,'30106'                                                         AS 借方セグメント1                ")
        SQLBldr.AppendLine("   ,'392'                                                           AS 借方セグメント2                ")
        SQLBldr.AppendLine("   ,'30'                                                            AS 借方セグメント3                ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方番号1                      ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方番号2                      ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税区分                 ")
        SQLBldr.AppendLine("   ,40                                                              AS 借方消費税コード               ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税率区分               ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方外税同時入力区分           ")
        SQLBldr.AppendLine("   ,FORMAT(((SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                       ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0)))                                                               ")
        SQLBldr.AppendLine("         + (SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                        ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0))) * 0.1), '0')                AS 借方金額                       ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税額                   ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨金額                   ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨レート                 ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨取引区分               ")
        SQLBldr.AppendLine("   ,T2.ACCOUNTCODE                                                  AS 貸方科目                       ")
        SQLBldr.AppendLine("   ,CASE                                                                                              ")
        SQLBldr.AppendLine("        WHEN MAX(T2.INVKEIJYOBRANCHCD) = '010101' THEN '010102'                                       ")
        SQLBldr.AppendLine("        ELSE MAX(T2.INVKEIJYOBRANCHCD)                                                                ")
        SQLBldr.AppendLine("    END                                                             AS 貸方部門                       ")
        SQLBldr.AppendLine("   ,'0000'                                                          AS 貸方銀行                       ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                     AS 貸方取引先                     ")
        SQLBldr.AppendLine("   ,9                                                               AS 貸方汎用補助1                  ")
        SQLBldr.AppendLine("   ,'30106'                                                         AS 貸方セグメント1                ")
        SQLBldr.AppendLine("   ,'392'                                                           AS 貸方セグメント2                ")
        SQLBldr.AppendLine("   ,'30'                                                            AS 貸方セグメント3                ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方番号1                      ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方番号2                      ")
        SQLBldr.AppendLine("   ,1                                                               AS 貸方消費税区分                 ")
        SQLBldr.AppendLine("   ,20                                                              AS 貸方消費税コード               ")
        SQLBldr.AppendLine("   ,4                                                               AS 貸方消費税率区分               ")
        SQLBldr.AppendLine("   ,1                                                               AS 貸方外税同時入力区分           ")
        SQLBldr.AppendLine("   ,FORMAT(SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0)), '0')                        AS 貸方金額                       ")
        SQLBldr.AppendLine("   ,FORMAT(((SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                       ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0))) * 0.1), '0')                AS 貸方消費税額                   ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨金額                   ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨レート                 ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨取引区分               ")
        SQLBldr.AppendLine("   ,DATE_FORMAT(MAX(T2.SCHEDATEPAYMENT), '%Y/%m/%d')                AS 期日                           ")
        SQLBldr.AppendLine("   ,MAX(T8.TORINAME) + '　\' +                                                                        ")
        SQLBldr.AppendLine("    FORMAT(((SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                       ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0)))                                                               ")
        SQLBldr.AppendLine("         + (SUM(coalesce(T2.RENTALINCENTIVE, 0))                                                        ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTADJUSTMENT, 0))                                                          ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.LEASEADJUSTMENT, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTALATEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.RENTSUBSCADJUST, 0))                                                         ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.MISCELLANEOUSEXPENSE, 0))                                                    ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.WRITEFEE, 0))                                                                ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.SPOTLEASEFEE, 0))                                                            ")
        SQLBldr.AppendLine("         + SUM(coalesce(T2.FIXEDFEE, 0))) * 0.1), '#,##0')                                              ")
        SQLBldr.AppendLine("    + '　' + MAX(T7.NAME)                                                                             ")
        SQLBldr.AppendLine("    + '　' + MAX(T9.VALUE1) + '　' + MAX(T10.VALUE4) + 'コンテナ'   AS 摘要                           ")
        SQLBldr.AppendLine("   ,''                                                              AS 摘要コード1                    ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString & "'  AS 作成日                         ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString & "'  AS 作成時間                       ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString & "'      AS 作成者                         ")
        SQLBldr.AppendLine("FROM                                                                                                  ")
        SQLBldr.AppendLine("    LNG.LNT0064_INVOICEHEAD T1                                                                        ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    LNG.LNT0080_INVOICEDATA_ADDAMOUNT T2                                                              ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T2.KEIJOYM        = T1.KEIJOYM                                                                    ")
        SQLBldr.AppendLine("AND T2.INVOICEORGCODE = T1.INVOICEORGCODE                                                             ")
        SQLBldr.AppendLine("AND T2.TORICODE       = T1.TORICODE                                                                   ")
        SQLBldr.AppendLine("AND T2.INVOICENUMBER  = T1.INVOICENUMBER                                                              ")
        SQLBldr.AppendLine("AND T2.INVOICETYPE    = T1.INVOICETYPE                                                                ")
        SQLBldr.AppendLine("LEFT OUTER JOIN                                                                                       ")
        SQLBldr.AppendLine("    LNG.LNM0002_RECONM T3                                                                             ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T3.CTNTYPE = T2.CTNTYPE                                                                           ")
        SQLBldr.AppendLine("AND T3.CTNNO   = T2.CTNNO                                                                             ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                          ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.KEIJOYM                                                                                    ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJOYM ORDER BY T1.KEIJOYM, T1.TORICODE) AS SORT1         ")
        SQLBldr.AppendLine("    FROM (                                                                                            ")
        SQLBldr.AppendLine("        SELECT                                                                                        ")
        SQLBldr.AppendLine("            T1.KEIJOYM                                                                                ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                               ")
        SQLBldr.AppendLine("        FROM                                                                                          ")
        SQLBldr.AppendLine("            LNG.LNT0080_INVOICEDATA_ADDAMOUNT T1                                                      ")
        SQLBldr.AppendLine("        WHERE                                                                                         ")
        SQLBldr.AppendLine("            T1.INVOICETYPE = '4'                                                                      ")
        SQLBldr.AppendLine("        AND T1.DELFLG      = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'               ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJOYM, T1.TORICODE                                                              ")
        SQLBldr.AppendLine("    ) T1                                                                                              ")
        SQLBldr.AppendLine(") T4                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T4.KEIJOYM  = T1.KEIJOYM                                                                          ")
        SQLBldr.AppendLine("AND T4.TORICODE = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                          ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.KEIJOYM                                                                                    ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,T1.INVOICEORGCODE                                                                             ")
        SQLBldr.AppendLine("       ,T1.INVKEIJYOBRANCHCD                                                                          ")
        SQLBldr.AppendLine("       ,T1.ACCOUNTCODE                                                                                ")
        SQLBldr.AppendLine("       ,T1.SEGMENTCODE                                                                                ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJOYM, T1.TORICODE                                       ")
        SQLBldr.AppendLine("        ORDER BY T1.KEIJOYM, T1.TORICODE,                                                             ")
        SQLBldr.AppendLine("                 T1.INVOICEORGCODE, T1.INVKEIJYOBRANCHCD, T1.ACCOUNTCODE, T1.SEGMENTCODE) AS SORT2    ")
        SQLBldr.AppendLine("    FROM (                                                                                            ")
        SQLBldr.AppendLine("        SELECT                                                                                        ")
        SQLBldr.AppendLine("            T1.KEIJOYM                               AS KEIJOYM                                       ")
        SQLBldr.AppendLine("           ,T1.TORICODE                              AS TORICODE                                      ")
        SQLBldr.AppendLine("           ,T1.INVOICEORGCODE                        AS INVOICEORGCODE                                ")
        SQLBldr.AppendLine("           ,T1.INVKEIJYOBRANCHCD                     AS INVKEIJYOBRANCHCD                             ")
        SQLBldr.AppendLine("           ,T1.ACCOUNTCODE                           AS ACCOUNTCODE                                   ")
        SQLBldr.AppendLine("           ,CASE                                                                                      ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20'                                      ")
        SQLBldr.AppendLine("               THEN 30207                                                                             ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '10' THEN 30201                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '15' THEN 30202                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '11' THEN 30203                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '05' THEN 30204                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '30' THEN 30205                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '35' THEN 30206                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '25' THEN 30208                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '20' THEN 30209                                                     ")
        SQLBldr.AppendLine("               ELSE 99999                                                                             ")
        SQLBldr.AppendLine("            END                                      AS SEGMENTCODE                                   ")
        SQLBldr.AppendLine("        FROM                                                                                          ")
        SQLBldr.AppendLine("            LNG.LNT0080_INVOICEDATA_ADDAMOUNT T1                                                      ")
        SQLBldr.AppendLine("        LEFT OUTER JOIN                                                                               ")
        SQLBldr.AppendLine("            LNG.LNM0002_RECONM T2                                                                     ")
        SQLBldr.AppendLine("        ON                                                                                            ")
        SQLBldr.AppendLine("            T2.CTNTYPE = T1.CTNTYPE                                                                   ")
        SQLBldr.AppendLine("        AND T2.CTNNO   = T1.CTNNO                                                                     ")
        SQLBldr.AppendLine("        WHERE                                                                                         ")
        SQLBldr.AppendLine("            T1.INVOICETYPE = '4'                                                                      ")
        SQLBldr.AppendLine("        AND T1.DELFLG      = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'               ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJOYM, T1.TORICODE, T1.INVOICEORGCODE, T1.INVKEIJYOBRANCHCD, T1.ACCOUNTCODE     ")
        SQLBldr.AppendLine("           ,CASE                                                                                      ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20'                                      ")
        SQLBldr.AppendLine("               THEN 30207                                                                             ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '10' THEN 30201                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '15' THEN 30202                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '11' THEN 30203                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '05' THEN 30204                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '30' THEN 30205                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '35' THEN 30206                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '25' THEN 30208                                                     ")
        SQLBldr.AppendLine("               WHEN T2.BIGCTNCD = '20' THEN 30209                                                     ")
        SQLBldr.AppendLine("               ELSE 99999                                                                             ")
        SQLBldr.AppendLine("            END                                                                                       ")
        SQLBldr.AppendLine("    ) T1                                                                                              ")
        SQLBldr.AppendLine(") T5                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T5.KEIJOYM           = T1.KEIJOYM                                                                 ")
        SQLBldr.AppendLine("AND T5.TORICODE          = T1.TORICODE                                                                ")
        SQLBldr.AppendLine("AND T5.INVOICEORGCODE    = T1.INVOICEORGCODE                                                          ")
        SQLBldr.AppendLine("AND T5.INVKEIJYOBRANCHCD = T2.INVKEIJYOBRANCHCD                                                       ")
        SQLBldr.AppendLine("AND T5.ACCOUNTCODE       = T2.ACCOUNTCODE                                                             ")
        SQLBldr.AppendLine("AND T5.SEGMENTCODE       = CASE                                                                       ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '35' AND T3.MIDDLECTNCD = '20'                       ")
        SQLBldr.AppendLine("                              THEN 30207                                                              ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '10' THEN 30201                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '15' THEN 30202                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '11' THEN 30203                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '05' THEN 30204                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '30' THEN 30205                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '35' THEN 30206                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '25' THEN 30208                                      ")
        SQLBldr.AppendLine("                              WHEN T3.BIGCTNCD = '20' THEN 30209                                      ")
        SQLBldr.AppendLine("                              ELSE 99999                                                              ")
        SQLBldr.AppendLine("                           END                                                                        ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0019_ORG T7                                                                                ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T7.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T7.ORGCODE  = T2.INVKEIJYOBRANCHCD                                                                ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T7.STYMD AND T7.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN(                                                                                           ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,coalesce(T1.TORINAME, '') + coalesce(T1.TORIDIVNAME, '') AS TORINAME                              ")
        SQLBldr.AppendLine("    FROM                                                                                              ")
        SQLBldr.AppendLine("        LNG.LNM0024_KEKKJM T1                                                                         ")
        SQLBldr.AppendLine("    WHERE                                                                                             ")
        SQLBldr.AppendLine("        T1.DELFLG = '0'                                                                               ")
        SQLBldr.AppendLine("    GROUP BY                                                                                          ")
        SQLBldr.AppendLine("        T1.TORICODE, T1.TORINAME, T1.TORIDIVNAME                                                      ")
        SQLBldr.AppendLine(") T8                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T8.TORICODE = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T9                                                                           ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T9.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T9.CLASS    = 'ACCOUNTCODE'                                                                       ")
        SQLBldr.AppendLine("AND T9.KEYCODE  = T2.ACCOUNTCODE                                                                      ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T9.STYMD AND T9.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T10                                                                          ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T10.CAMPCODE = '01'                                                                               ")
        SQLBldr.AppendLine("AND T10.CLASS    = 'SEGMENTCODE'                                                                      ")
        SQLBldr.AppendLine("AND T10.KEYCODE  = T2.SEGMENTCODE                                                                     ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T10.STYMD AND T10.ENDYMD                                                        ")
        SQLBldr.AppendLine("WHERE                                                                                                 ")
        SQLBldr.AppendLine("    T1.KEIJOYM     = '" & htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString & "'                      ")
        SQLBldr.AppendLine("AND T1.INVOICETYPE = '4'                                                                              ")
        SQLBldr.AppendLine("AND T1.DELFLG      = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'                       ")
        SQLBldr.AppendLine("GROUP BY                                                                                              ")
        SQLBldr.AppendLine("    T1.KEIJOYM, T1.TORICODE, T1.INVOICEORGCODE, T2.INVKEIJYOBRANCHCD, T2.ACCOUNTCODE                  ")
        SQLBldr.AppendLine("   ,CASE                                                                                              ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '35' AND T3.MIDDLECTNCD = '20'                                              ")
        SQLBldr.AppendLine("       THEN 30207                                                                                     ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '10' THEN 30201                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '15' THEN 30202                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '11' THEN 30203                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '05' THEN 30204                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '30' THEN 30205                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '35' THEN 30206                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '25' THEN 30208                                                             ")
        SQLBldr.AppendLine("       WHEN T3.BIGCTNCD = '20' THEN 30209                                                             ")
        SQLBldr.AppendLine("       ELSE 99999                                                                                     ")
        SQLBldr.AppendLine("    END                                                                                               ")
        SQLBldr.AppendLine("ORDER BY                                                                                              ")
        SQLBldr.AppendLine("    伝票番号, 明細行番号                                                                              ")
#End Region

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

#End Region

#Region "経理連携 賦金表"
    ''' <summary>
    ''' ファイナンスリース償却費 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectFinanceDepreciationCsv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
#Region "経理連携 ファイナンスリース償却費 検索処理SQL"
        ' 請求明細リーステーブル
        SQLBldr.AppendLine("SELECT                                                                                          ")
        SQLBldr.AppendLine("    0                                                               AS データ基準               ")
        SQLBldr.AppendLine("   ,1002                                                            AS 仕訳形式入力             ")
        SQLBldr.AppendLine("   ,11                                                              AS 入力画面番号             ")
        SQLBldr.AppendLine("   ,DATE_FORMAT(LAST_DAY(                                                                       ")
        SQLBldr.AppendLine("    CAST(T1.KEIJYOYM * 100 + 1 AS VARCHAR)), '%Y/%m/%d')            AS 伝票日付                 ")
        SQLBldr.AppendLine("   ,0                                                               AS 決算月区分               ")
        SQLBldr.AppendLine("   ,'FE'                                                            AS 証憑番号                 ")
        SQLBldr.AppendLine("   ,'4' + FORMAT(MAX(T3.SORT1), '0000000')                          AS 伝票番号                 ")
        SQLBldr.AppendLine("   ,40000000 + MAX(T3.SORT1)                                        AS 伝票No                   ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T4.SORT2), '000')                                    AS 明細行番号               ")
        SQLBldr.AppendLine("   ,'51100101'                                                      AS 借方科目                 ")
        SQLBldr.AppendLine("   ,'011307'                                                        AS 借方部門                 ")
        SQLBldr.AppendLine("   ,'0000'                                                          AS 借方銀行                 ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                     AS 借方取引先               ")
        SQLBldr.AppendLine("   ,'9'                                                             AS 借方汎用補助1            ")
        SQLBldr.AppendLine("   ,'30211'                                                         AS 借方セグメント1          ")
        SQLBldr.AppendLine("   ,'392'                                                           AS 借方セグメント2          ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方セグメント3          ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方番号1                ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方番号2                ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税区分           ")
        SQLBldr.AppendLine("   ,40                                                              AS 借方消費税コード         ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税率区分         ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方外税同時入力区分     ")
        SQLBldr.AppendLine("   ,FORMAT(SUM(T1.PAYPRINCIPAL), '0')                               AS 借方金額                 ")
        SQLBldr.AppendLine("   ,0                                                               AS 借方消費税額             ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨金額             ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨レート           ")
        SQLBldr.AppendLine("   ,''                                                              AS 借方外貨取引区分         ")
        SQLBldr.AppendLine("   ,11030102                                                        AS 貸方科目                 ")
        SQLBldr.AppendLine("   ,'011307'                                                        AS 貸方部門                 ")
        SQLBldr.AppendLine("   ,'0000'                                                          AS 貸方銀行                 ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                     AS 貸方取引先               ")
        SQLBldr.AppendLine("   ,'9'                                                             AS 貸方汎用補助1            ")
        SQLBldr.AppendLine("   ,'30211'                                                         AS 貸方セグメント1          ")
        SQLBldr.AppendLine("   ,'392'                                                           AS 貸方セグメント2          ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方セグメント3          ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方番号1                ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方番号2                ")
        SQLBldr.AppendLine("   ,0                                                               AS 貸方消費税区分           ")
        SQLBldr.AppendLine("   ,40                                                              AS 貸方消費税コード         ")
        SQLBldr.AppendLine("   ,0                                                               AS 貸方消費税率区分         ")
        SQLBldr.AppendLine("   ,0                                                               AS 貸方外税同時入力区分     ")
        SQLBldr.AppendLine("   ,FORMAT(SUM(T1.PAYPRINCIPAL), '0')                               AS 貸方金額                 ")
        SQLBldr.AppendLine("   ,0                                                               AS 貸方消費税額             ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨金額             ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨レート           ")
        SQLBldr.AppendLine("   ,''                                                              AS 貸方外貨取引区分         ")
        SQLBldr.AppendLine("   ,''                                                              AS 期日                     ")
        SQLBldr.AppendLine("   ,MAX(T6.TORINAME) + '　\' + FORMAT(SUM(T1.PAYPRINCIPAL), '#,##0')                            ")
        SQLBldr.AppendLine("    + '　' + MAX(T5.NAME)                                                                       ")
        SQLBldr.AppendLine("    + '　' + MAX(T7.VALUE1) + '　' + MAX(T8.VALUE4) + 'コンテナ'    AS 摘要                     ")
        SQLBldr.AppendLine("   ,''                                                              AS 摘要コード1              ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString & "'  AS 作成日                   ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString & "'  AS 作成時間                 ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString & "'      AS 作成者                   ")
        SQLBldr.AppendLine("FROM                                                                                            ")
        SQLBldr.AppendLine("    LNG.LNT0065_FL_LEVIES T1                                                                    ")
        SQLBldr.AppendLine("INNER JOIN                                                                                      ")
        SQLBldr.AppendLine("    LNG.LNM0002_RECONM T2                                                                       ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T2.CTNTYPE        = T1.CTNTYPE                                                              ")
        SQLBldr.AppendLine("AND T2.CTNNO          = T1.CTNNO                                                                ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                    ")
        SQLBldr.AppendLine("    SELECT                                                                                      ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                             ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                             ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM ORDER BY T1.KEIJYOYM, T1.TORICODE) AS SORT1 ")
        SQLBldr.AppendLine("    FROM (                                                                                      ")
        SQLBldr.AppendLine("        SELECT                                                                                  ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                         ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("        FROM                                                                                    ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                            ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE                                                       ")
        SQLBldr.AppendLine("    ) T1                                                                                        ")
        SQLBldr.AppendLine(") T3                                                                                            ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T3.KEIJYOYM = T1.KEIJYOYM                                                                   ")
        SQLBldr.AppendLine("AND T3.TORICODE = T1.TORICODE                                                                   ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                    ")
        SQLBldr.AppendLine("    SELECT                                                                                      ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                             ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                             ")
        SQLBldr.AppendLine("       ,T1.ACCOUNTCODE                                                                          ")
        SQLBldr.AppendLine("       ,T1.SEGMENTCODE                                                                          ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM, T1.TORICODE                                ")
        SQLBldr.AppendLine("        ORDER BY T1.KEIJYOYM, T1.TORICODE, T1.ACCOUNTCODE, T1.SEGMENTCODE) AS SORT2             ")
        SQLBldr.AppendLine("    FROM (                                                                                      ")
        SQLBldr.AppendLine("        SELECT                                                                                  ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                              AS KEIJYOYM                                ")
        SQLBldr.AppendLine("           ,T1.TORICODE                              AS TORICODE                                ")
        SQLBldr.AppendLine("           ,'51100101'                               AS ACCOUNTCODE                             ")
        SQLBldr.AppendLine("           ,CASE                                                                                ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                    ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '10' THEN 30201                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '15' THEN 30202                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '11' THEN 30203                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '05' THEN 30204                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '30' THEN 30205                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' THEN 30206                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '25' THEN 30208                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '20' THEN 30209                                              ")
        SQLBldr.AppendLine("                ELSE 99999                                                                      ")
        SQLBldr.AppendLine("            END                                      AS SEGMENTCODE                             ")
        SQLBldr.AppendLine("        FROM                                                                                    ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                            ")
        SQLBldr.AppendLine("        INNER JOIN                                                                              ")
        SQLBldr.AppendLine("            LNG.LNM0002_RECONM T2                                                               ")
        SQLBldr.AppendLine("        ON                                                                                      ")
        SQLBldr.AppendLine("            T2.CTNTYPE        = T1.CTNTYPE                                                      ")
        SQLBldr.AppendLine("        AND T2.CTNNO          = T1.CTNNO                                                        ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE,                                                      ")
        SQLBldr.AppendLine("            CASE                                                                                ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                    ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '10' THEN 30201                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '15' THEN 30202                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '11' THEN 30203                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '05' THEN 30204                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '30' THEN 30205                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' THEN 30206                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '25' THEN 30208                                              ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '20' THEN 30209                                              ")
        SQLBldr.AppendLine("                ELSE 99999                                                                      ")
        SQLBldr.AppendLine("            END                                                                                 ")
        SQLBldr.AppendLine("    ) T1                                                                                        ")
        SQLBldr.AppendLine(") T4                                                                                            ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T4.KEIJYOYM    = T1.KEIJYOYM                                                                ")
        SQLBldr.AppendLine("AND T4.TORICODE    = T1.TORICODE                                                                ")
        SQLBldr.AppendLine("AND T4.ACCOUNTCODE = '51100101'                                                                 ")
        SQLBldr.AppendLine("AND T4.SEGMENTCODE = CASE                                                                       ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207           ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '10' THEN 30201                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '15' THEN 30202                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '11' THEN 30203                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '05' THEN 30204                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '30' THEN 30205                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '35' THEN 30206                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '25' THEN 30208                                     ")
        SQLBldr.AppendLine("                         WHEN T2.BIGCTNCD = '20' THEN 30209                                     ")
        SQLBldr.AppendLine("                         ELSE 99999                                                             ")
        SQLBldr.AppendLine("                     END                                                                        ")
        SQLBldr.AppendLine("INNER JOIN                                                                                      ")
        SQLBldr.AppendLine("    com.LNS0019_ORG T5                                                                          ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T5.CAMPCODE = '01'                                                                          ")
        SQLBldr.AppendLine("AND T5.ORGCODE  = T1.KEIJOORGCD                                                                 ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T5.STYMD AND T5.ENDYMD                                                    ")
        SQLBldr.AppendLine("INNER JOIN(                                                                                     ")
        SQLBldr.AppendLine("    SELECT                                                                                      ")
        SQLBldr.AppendLine("        T1.TORICODE                                                                             ")
        SQLBldr.AppendLine("       ,coalesce(T1.TORINAME, '') + coalesce(T1.TORIDIVNAME, '') AS TORINAME                        ")
        SQLBldr.AppendLine("    FROM                                                                                        ")
        SQLBldr.AppendLine("        LNG.LNM0024_KEKKJM T1                                                                   ")
        SQLBldr.AppendLine("    WHERE                                                                                       ")
        SQLBldr.AppendLine("        T1.DELFLG = '0'                                                                         ")
        SQLBldr.AppendLine("    GROUP BY                                                                                    ")
        SQLBldr.AppendLine("        T1.TORICODE, T1.TORINAME, T1.TORIDIVNAME                                                ")
        SQLBldr.AppendLine(") T6                                                                                            ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T6.TORICODE = T1.TORICODE                                                                   ")
        SQLBldr.AppendLine("INNER JOIN                                                                                      ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T7                                                                     ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T7.CAMPCODE = '01'                                                                          ")
        SQLBldr.AppendLine("AND T7.CLASS    = 'ACCOUNTCODE'                                                                 ")
        SQLBldr.AppendLine("AND T7.KEYCODE  = '11030102'                                                                    ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T7.STYMD AND T7.ENDYMD                                                    ")
        SQLBldr.AppendLine("INNER JOIN                                                                                      ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T8                                                                     ")
        SQLBldr.AppendLine("ON                                                                                              ")
        SQLBldr.AppendLine("    T8.CAMPCODE = '01'                                                                          ")
        SQLBldr.AppendLine("AND T8.CLASS    = 'SEGMENTCODE'                                                                 ")
        SQLBldr.AppendLine("AND T8.KEYCODE  = CASE                                                                          ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN '30207'            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '10' THEN '30201'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '15' THEN '30202'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '11' THEN '30203'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '05' THEN '30204'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '30' THEN '30205'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' THEN '30206'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '25' THEN '30208'                                      ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '20' THEN '30209'                                      ")
        SQLBldr.AppendLine("                      ELSE 90101                                                                ")
        SQLBldr.AppendLine("                  END                                                                           ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T8.STYMD AND T8.ENDYMD                                                    ")
        SQLBldr.AppendLine("WHERE                                                                                           ")
        SQLBldr.AppendLine("    T1.KEIJYOYM = '" & htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString & "'                   ")
        SQLBldr.AppendLine("AND T1.DELFLG   = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'                    ")
        SQLBldr.AppendLine("GROUP BY                                                                                        ")
        SQLBldr.AppendLine("    T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD                                                ")
        SQLBldr.AppendLine("   ,CASE                                                                                        ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                            ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '10' THEN 30201                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '15' THEN 30202                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '11' THEN 30203                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '05' THEN 30204                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '30' THEN 30205                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '35' THEN 30206                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '25' THEN 30208                                                      ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '20' THEN 30209                                                      ")
        SQLBldr.AppendLine("        ELSE 99999                                                                              ")
        SQLBldr.AppendLine("    END                                                                                         ")
        SQLBldr.AppendLine("ORDER BY                                                                                        ")
        SQLBldr.AppendLine("    伝票番号, 明細行番号                                                                        ")
#End Region

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ファイナンスリース仮受消費税① 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectFinanceInputTax1Csv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
#Region "経理連携 ファイナンスリース仮受消費税① 検索処理SQL"
        ' 請求明細リーステーブル
        SQLBldr.AppendLine("SELECT                                                                                                ")
        SQLBldr.AppendLine("    0                                                                    AS データ基準                ")
        SQLBldr.AppendLine("   ,1002                                                                 AS 仕訳形式入力              ")
        SQLBldr.AppendLine("   ,11                                                                   AS 入力画面番号              ")
        SQLBldr.AppendLine("   ,FORMAT(EOMONTH(                                                                                   ")
        SQLBldr.AppendLine("    CAST(MIN(T1.KEIJYOYM) * 100 + 1 AS NVARCHAR)), 'yyyy/MM/dd')         AS 伝票日付                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 決算月区分                ")
        SQLBldr.AppendLine("   ,'AB'                                                                 AS 証憑番号                  ")
        SQLBldr.AppendLine("   ,'3' + FORMAT(MAX(T3.SORT1), '0000000')                               AS 伝票番号                  ")
        SQLBldr.AppendLine("   ,30000000 + MAX(T3.SORT1)                                             AS 伝票No                    ")
        SQLBldr.AppendLine("   ,'001'                                                                AS 明細行番号                ")
        SQLBldr.AppendLine("   ,'21011501'                                                           AS 借方科目                  ")
        SQLBldr.AppendLine("   ,'011301'                                                             AS 借方部門                  ")
        SQLBldr.AppendLine("   ,'0000'                                                               AS 借方銀行                  ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 借方取引先                ")
        SQLBldr.AppendLine("   ,9                                                                    AS 借方汎用補助1             ")
        SQLBldr.AppendLine("   ,'90101'                                                              AS 借方セグメント1           ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 借方セグメント2           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方セグメント3           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号1                 ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号2                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税区分            ")
        SQLBldr.AppendLine("   ,40                                                                   AS 借方消費税コード          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税率区分          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方外税同時入力区分      ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '0')                          AS 借方金額                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨金額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨レート            ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨取引区分          ")
        SQLBldr.AppendLine("   ,'11030401'                                                           AS 貸方科目                  ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                   AS 貸方部門                  ")
        SQLBldr.AppendLine("   ,MAX(T1.INACCOUNTCD)                                                  AS 貸方銀行                  ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 貸方取引先                ")
        SQLBldr.AppendLine("   ,9                                                                    AS 貸方汎用補助1             ")
        SQLBldr.AppendLine("   ,'90101'                                                              AS 貸方セグメント1           ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 貸方セグメント2           ")
        SQLBldr.AppendLine("   ,'90'                                                                 AS 貸方セグメント3           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号1                 ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号2                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税区分            ")
        SQLBldr.AppendLine("   ,40                                                                   AS 貸方消費税コード          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税率区分          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方外税同時入力区分      ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '0')                          AS 貸方金額                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨金額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨レート            ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨取引区分          ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T1.PAYDATE), 'yyyy/MM/dd')                                AS 期日                      ")
        SQLBldr.AppendLine("   ,MAX(T5.TORINAME) + '　\' + FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '#,##0')                        ")
        SQLBldr.AppendLine("    + '　' + MAX(T4.NAME)                                                                             ")
        SQLBldr.AppendLine("    + '　' + MAX(T6.VALUE1) + '　' + MAX(T7.VALUE4) + 'コンテナ'         AS 摘要                      ")
        SQLBldr.AppendLine("   ,''                                                                   AS 摘要コード1               ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString & "'       AS 作成日                    ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString & "'       AS 作成時間                  ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString & "'           AS 作成者                    ")
        SQLBldr.AppendLine("FROM                                                                                                  ")
        SQLBldr.AppendLine("    LNG.LNT0065_FL_LEVIES T1                                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    LNG.LNM0002_RECONM T2                                                                             ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T2.CTNTYPE = T1.CTNTYPE                                                                           ")
        SQLBldr.AppendLine("AND T2.CTNNO   = T1.CTNNO                                                                             ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                          ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                                   ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,T1.INVOICEOUTORGCD                                                                            ")
        SQLBldr.AppendLine("       ,SUM(T1.LEASEAMOUNT) AS LEASEAMOUNT                                                            ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM                                                   ")
        SQLBldr.AppendLine("                               ORDER BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD) AS SORT1        ")
        SQLBldr.AppendLine("    FROM (                                                                                            ")
        SQLBldr.AppendLine("        SELECT                                                                                        ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                               ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                               ")
        SQLBldr.AppendLine("           ,T1.INVOICEOUTORGCD                                                                        ")
        SQLBldr.AppendLine("           ,ROUND(T1.PAYMONTHLYAMOUNT * (T1.TAXRATE / 100), 0) AS LEASEAMOUNT                         ")
        SQLBldr.AppendLine("        FROM                                                                                          ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                                  ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD, T1.PAYMONTHLYAMOUNT, T1.TAXRATE        ")
        SQLBldr.AppendLine("    ) T1                                                                                              ")
        SQLBldr.AppendLine("    GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD                                             ")
        SQLBldr.AppendLine(") T3                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T3.KEIJYOYM        = T1.KEIJYOYM                                                                  ")
        SQLBldr.AppendLine("AND T3.TORICODE        = T1.TORICODE                                                                  ")
        SQLBldr.AppendLine("AND T3.INVOICEOUTORGCD = T1.INVOICEOUTORGCD                                                           ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0019_ORG T4                                                                                ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T4.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T4.ORGCODE  = T1.KEIJOORGCD                                                                       ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T4.STYMD AND T4.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN(                                                                                           ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,coalesce(T1.TORINAME, '') + coalesce(T1.TORIDIVNAME, '') AS TORINAME                              ")
        SQLBldr.AppendLine("    FROM                                                                                              ")
        SQLBldr.AppendLine("        LNG.LNM0024_KEKKJM T1                                                                         ")
        SQLBldr.AppendLine("    WHERE                                                                                             ")
        SQLBldr.AppendLine("        T1.DELFLG = '0'                                                                               ")
        SQLBldr.AppendLine("    GROUP BY                                                                                          ")
        SQLBldr.AppendLine("        T1.TORICODE, T1.TORINAME, T1.TORIDIVNAME                                                      ")
        SQLBldr.AppendLine(") T5                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T5.TORICODE = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T6                                                                           ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T6.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T6.CLASS    = 'ACCOUNTCODE'                                                                       ")
        SQLBldr.AppendLine("AND T6.KEYCODE  = '11030401'                                                                          ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T6.STYMD AND T6.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T7                                                                           ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T7.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T7.CLASS    = 'SEGMENTCODE'                                                                       ")
        SQLBldr.AppendLine("AND T7.KEYCODE  = CASE                                                                                ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN '30207'                  ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '10' THEN '30201'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '15' THEN '30202'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '11' THEN '30203'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '05' THEN '30204'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '30' THEN '30205'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' THEN '30206'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '25' THEN '30208'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '20' THEN '30209'                                            ")
        SQLBldr.AppendLine("                      ELSE 90101                                                                      ")
        SQLBldr.AppendLine("                  END                                                                                 ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T7.STYMD AND T7.ENDYMD                                                          ")
        SQLBldr.AppendLine("WHERE                                                                                                 ")
        SQLBldr.AppendLine("    T1.KEIJYOYM = '" & htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString & "'                         ")
        SQLBldr.AppendLine("AND T1.DELFLG   = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'                          ")
        SQLBldr.AppendLine("GROUP BY                                                                                              ")
        SQLBldr.AppendLine("    T1.KEIJYOYM                                                                                       ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                                                       ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                                                ")
        SQLBldr.AppendLine("ORDER BY                                                                                              ")
        SQLBldr.AppendLine("    伝票番号, 明細行番号                                                                              ")
#End Region

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ファイナンスリース仮受消費税② 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectFinanceInputTax2Csv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
#Region "経理連携 ファイナンスリース仮受消費税② 検索処理SQL"
        ' 請求明細リーステーブル
        SQLBldr.AppendLine("SELECT                                                                                                ")
        SQLBldr.AppendLine("    0                                                                    AS データ基準                ")
        SQLBldr.AppendLine("   ,1002                                                                 AS 仕訳形式入力              ")
        SQLBldr.AppendLine("   ,11                                                                   AS 入力画面番号              ")
        SQLBldr.AppendLine("   ,FORMAT(EOMONTH(                                                                                   ")
        SQLBldr.AppendLine("    CAST(MIN(T1.KEIJYOYM) * 100 + 1 AS NVARCHAR)), 'yyyy/MM/dd')         AS 伝票日付                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 決算月区分                ")
        SQLBldr.AppendLine("   ,'AB'                                                                 AS 証憑番号                  ")
        SQLBldr.AppendLine("   ,'3' + FORMAT(MAX(T3.SORT1), '0000000')                               AS 伝票番号                  ")
        SQLBldr.AppendLine("   ,30000000 + MAX(T3.SORT1)                                             AS 伝票No                    ")
        SQLBldr.AppendLine("   ,'002'                                                                AS 明細行番号                ")
        SQLBldr.AppendLine("   ,'11030401'                                                           AS 借方科目                  ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                   AS 借方部門                  ")
        SQLBldr.AppendLine("   ,MAX(T1.INACCOUNTCD)                                                  AS 借方銀行                  ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 借方取引先                ")
        SQLBldr.AppendLine("   ,9                                                                    AS 借方汎用補助1             ")
        SQLBldr.AppendLine("   ,'90101'                                                              AS 借方セグメント1           ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 借方セグメント2           ")
        SQLBldr.AppendLine("   ,'90'                                                                 AS 借方セグメント3           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号1                 ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号2                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税区分            ")
        SQLBldr.AppendLine("   ,40                                                                   AS 借方消費税コード          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税率区分          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方外税同時入力区分      ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '0')                          AS 借方金額                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨金額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨レート            ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨取引区分          ")
        SQLBldr.AppendLine("   ,'11030416'                                                           AS 貸方科目                  ")
        SQLBldr.AppendLine("   ,'011301'                                                             AS 貸方部門                  ")
        SQLBldr.AppendLine("   ,'0000'                                                               AS 貸方銀行                  ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 貸方取引先                ")
        SQLBldr.AppendLine("   ,9                                                                    AS 貸方汎用補助1             ")
        SQLBldr.AppendLine("   ,'90101'                                                              AS 貸方セグメント1           ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 貸方セグメント2           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方セグメント3           ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号1                 ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号2                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税区分            ")
        SQLBldr.AppendLine("   ,40                                                                   AS 貸方消費税コード          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税率区分          ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方外税同時入力区分      ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '0')                          AS 貸方金額                  ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨金額              ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨レート            ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨取引区分          ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T1.PAYDATE), 'yyyy/MM/dd')                                AS 期日                      ")
        SQLBldr.AppendLine("   ,MAX(T5.TORINAME) + '　\' + FORMAT(coalesce(MAX(T3.LEASEAMOUNT), 0), '#,##0')                        ")
        SQLBldr.AppendLine("    + '　' + MAX(T4.NAME)                                                                             ")
        SQLBldr.AppendLine("    + '　' + MAX(T6.VALUE1) + '　' + MAX(T7.VALUE4) + 'コンテナ'         AS 摘要                      ")
        SQLBldr.AppendLine("   ,''                                                                   AS 摘要コード1               ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString & "'       AS 作成日                    ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString & "'       AS 作成時間                  ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString & "'           AS 作成者                    ")
        SQLBldr.AppendLine("FROM                                                                                                  ")
        SQLBldr.AppendLine("    LNG.LNT0065_FL_LEVIES T1                                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    LNG.LNM0002_RECONM T2                                                                             ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T2.CTNTYPE = T1.CTNTYPE                                                                           ")
        SQLBldr.AppendLine("AND T2.CTNNO   = T1.CTNNO                                                                             ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                          ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                                   ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,T1.INVOICEOUTORGCD                                                                            ")
        SQLBldr.AppendLine("       ,SUM(T1.LEASEAMOUNT) AS LEASEAMOUNT                                                            ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM                                                   ")
        SQLBldr.AppendLine("                               ORDER BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD) AS SORT1        ")
        SQLBldr.AppendLine("    FROM (                                                                                            ")
        SQLBldr.AppendLine("        SELECT                                                                                        ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                               ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                               ")
        SQLBldr.AppendLine("           ,T1.INVOICEOUTORGCD                                                                        ")
        SQLBldr.AppendLine("           ,ROUND(T1.PAYMONTHLYAMOUNT * (T1.TAXRATE / 100), 0) AS LEASEAMOUNT                         ")
        SQLBldr.AppendLine("        FROM                                                                                          ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                                  ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD, T1.PAYMONTHLYAMOUNT, T1.TAXRATE        ")
        SQLBldr.AppendLine("    ) T1                                                                                              ")
        SQLBldr.AppendLine("    GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD                                             ")
        SQLBldr.AppendLine(") T3                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T3.KEIJYOYM        = T1.KEIJYOYM                                                                  ")
        SQLBldr.AppendLine("AND T3.TORICODE        = T1.TORICODE                                                                  ")
        SQLBldr.AppendLine("AND T3.INVOICEOUTORGCD = T1.INVOICEOUTORGCD                                                           ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0019_ORG T4                                                                                ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T4.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T4.ORGCODE  = T1.KEIJOORGCD                                                                       ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T4.STYMD AND T4.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN(                                                                                           ")
        SQLBldr.AppendLine("    SELECT                                                                                            ")
        SQLBldr.AppendLine("        T1.TORICODE                                                                                   ")
        SQLBldr.AppendLine("       ,coalesce(T1.TORINAME, '') + coalesce(T1.TORIDIVNAME, '') AS TORINAME                              ")
        SQLBldr.AppendLine("    FROM                                                                                              ")
        SQLBldr.AppendLine("        LNG.LNM0024_KEKKJM T1                                                                         ")
        SQLBldr.AppendLine("    WHERE                                                                                             ")
        SQLBldr.AppendLine("        T1.DELFLG = '0'                                                                               ")
        SQLBldr.AppendLine("    GROUP BY                                                                                          ")
        SQLBldr.AppendLine("        T1.TORICODE, T1.TORINAME, T1.TORIDIVNAME                                                      ")
        SQLBldr.AppendLine(") T5                                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T5.TORICODE = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T6                                                                           ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T6.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T6.CLASS    = 'ACCOUNTCODE'                                                                       ")
        SQLBldr.AppendLine("AND T6.KEYCODE  = '11030416'                                                                          ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T6.STYMD AND T6.ENDYMD                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                            ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T7                                                                           ")
        SQLBldr.AppendLine("ON                                                                                                    ")
        SQLBldr.AppendLine("    T7.CAMPCODE = '01'                                                                                ")
        SQLBldr.AppendLine("AND T7.CLASS    = 'SEGMENTCODE'                                                                       ")
        SQLBldr.AppendLine("AND T7.KEYCODE  = CASE                                                                                ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN '30207'                  ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '10' THEN '30201'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '15' THEN '30202'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '11' THEN '30203'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '05' THEN '30204'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '30' THEN '30205'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' THEN '30206'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '25' THEN '30208'                                            ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '20' THEN '30209'                                            ")
        SQLBldr.AppendLine("                      ELSE 90101                                                                      ")
        SQLBldr.AppendLine("                  END                                                                                 ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T7.STYMD AND T7.ENDYMD                                                          ")
        SQLBldr.AppendLine("WHERE                                                                                                 ")
        SQLBldr.AppendLine("    T1.KEIJYOYM = '" & htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString & "'                         ")
        SQLBldr.AppendLine("AND T1.DELFLG   = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'                          ")
        SQLBldr.AppendLine("GROUP BY                                                                                              ")
        SQLBldr.AppendLine("    T1.KEIJYOYM                                                                                       ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                                                       ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                                                ")
        SQLBldr.AppendLine("ORDER BY                                                                                              ")
        SQLBldr.AppendLine("    伝票番号, 明細行番号                                                                              ")
#End Region

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ファイナンスリース仮受消費税③ 検索処理(CSV用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="htParm">パラメータデータ</param>
    Public Shared Function SelectFinanceInputTax3Csv(sqlCon As MySqlConnection, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
#Region "経理連携 ファイナンスリース仮受消費税③ 検索処理SQL"
        ' 請求明細リーステーブル
        SQLBldr.AppendLine("SELECT                                                                                                       ")
        SQLBldr.AppendLine("    0                                                                    AS データ基準                       ")
        SQLBldr.AppendLine("   ,1002                                                                 AS 仕訳形式入力                     ")
        SQLBldr.AppendLine("   ,11                                                                   AS 入力画面番号                     ")
        SQLBldr.AppendLine("   ,FORMAT(EOMONTH(                                                                                          ")
        SQLBldr.AppendLine("    CAST(MIN(T1.KEIJYOYM) * 100 + 1 AS NVARCHAR)), 'yyyy/MM/dd')         AS 伝票日付                         ")
        SQLBldr.AppendLine("   ,0                                                                    AS 決算月区分                       ")
        SQLBldr.AppendLine("   ,'AB'                                                                 AS 証憑番号                         ")
        SQLBldr.AppendLine("   ,'3' + FORMAT(MAX(T3.SORT1), '0000000')                               AS 伝票番号                         ")
        SQLBldr.AppendLine("   ,30000000 + MAX(T3.SORT1)                                             AS 伝票No                           ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T4.SORT2) + 2, '000')                                     AS 明細行番号                       ")
        SQLBldr.AppendLine("   ,'11010401'                                                           AS 借方科目                         ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                   AS 借方部門                         ")
        SQLBldr.AppendLine("   ,MAX(T1.INACCOUNTCD)                                                  AS 借方銀行                         ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 借方取引先                       ")
        SQLBldr.AppendLine("   ,9                                                                    AS 借方汎用補助1                    ")
        SQLBldr.AppendLine("   ,'30211'                                                              AS 借方セグメント1                  ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 借方セグメント2                  ")
        SQLBldr.AppendLine("   ,'30'                                                                 AS 借方セグメント3                  ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号1                        ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方番号2                        ")
        SQLBldr.AppendLine("   ,1                                                                    AS 借方消費税区分                   ")
        SQLBldr.AppendLine("   ,40                                                                   AS 借方消費税コード                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税率区分                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方外税同時入力区分             ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T5.LEASEAMOUNT), 0), '0')                          AS 借方金額                         ")
        SQLBldr.AppendLine("   ,0                                                                    AS 借方消費税額                     ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨金額                     ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨レート                   ")
        SQLBldr.AppendLine("   ,''                                                                   AS 借方外貨取引区分                 ")
        SQLBldr.AppendLine("   ,'21011501'                                                           AS 貸方科目                         ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                   AS 貸方部門                         ")
        SQLBldr.AppendLine("   ,'0000'                                                               AS 貸方銀行                         ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                          AS 貸方取引先                       ")
        SQLBldr.AppendLine("   ,9                                                                    AS 貸方汎用補助1                    ")
        SQLBldr.AppendLine("   ,'90101'                                                              AS 貸方セグメント1                  ")
        SQLBldr.AppendLine("   ,'392'                                                                AS 貸方セグメント2                  ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方セグメント3                  ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号1                        ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方番号2                        ")
        SQLBldr.AppendLine("   ,1                                                                    AS 貸方消費税区分                   ")
        SQLBldr.AppendLine("   ,40                                                                   AS 貸方消費税コード                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税率区分                 ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方外税同時入力区分             ")
        SQLBldr.AppendLine("   ,FORMAT(coalesce(MAX(T5.LEASEAMOUNT), 0), '0')                          AS 貸方金額                         ")
        SQLBldr.AppendLine("   ,0                                                                    AS 貸方消費税額                     ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨金額                     ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨レート                   ")
        SQLBldr.AppendLine("   ,''                                                                   AS 貸方外貨取引区分                 ")
        SQLBldr.AppendLine("   ,FORMAT(MAX(T1.PAYDATE), 'yyyy/MM/dd')                                AS 期日                             ")
        SQLBldr.AppendLine("   ,MAX(T7.TORINAME) + '　\' + FORMAT(coalesce(MAX(T5.LEASEAMOUNT), 0), '#,##0')                               ")
        SQLBldr.AppendLine("    + '　' + MAX(T6.NAME)                                                                                    ")
        SQLBldr.AppendLine("    + '　' + MAX(T8.VALUE1) + '　' + MAX(T9.VALUE4) + 'コンテナ'         AS 摘要                             ")
        SQLBldr.AppendLine("   ,''                                                                   AS 摘要コード1                      ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATEDATE).ToString & "'       AS 作成日                           ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_CREATETIME).ToString & "'       AS 作成時間                         ")
        SQLBldr.AppendLine("   ,'" & htParm(SELECT_ACCOUNTING_KEY.SP_USERID).ToString & "'           AS 作成者                           ")
        SQLBldr.AppendLine("FROM                                                                                                         ")
        SQLBldr.AppendLine("    LNG.LNT0065_FL_LEVIES T1                                                                                 ")
        SQLBldr.AppendLine("INNER JOIN                                                                                                   ")
        SQLBldr.AppendLine("    LNG.LNM0002_RECONM T2                                                                                    ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T2.CTNTYPE = T1.CTNTYPE                                                                                  ")
        SQLBldr.AppendLine("AND T2.CTNNO   = T1.CTNNO                                                                                    ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                                 ")
        SQLBldr.AppendLine("    SELECT                                                                                                   ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                                          ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                          ")
        SQLBldr.AppendLine("       ,T1.INVOICEOUTORGCD                                                                                   ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM                                                          ")
        SQLBldr.AppendLine("                               ORDER BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD) AS SORT1               ")
        SQLBldr.AppendLine("    FROM (                                                                                                   ")
        SQLBldr.AppendLine("        SELECT                                                                                               ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                                      ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                                      ")
        SQLBldr.AppendLine("           ,T1.INVOICEOUTORGCD                                                                               ")
        SQLBldr.AppendLine("        FROM                                                                                                 ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                                         ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD                                                ")
        SQLBldr.AppendLine("    ) T1                                                                                                     ")
        SQLBldr.AppendLine(") T3                                                                                                         ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T3.KEIJYOYM        = T1.KEIJYOYM                                                                         ")
        SQLBldr.AppendLine("AND T3.TORICODE        = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("AND T3.INVOICEOUTORGCD = T1.INVOICEOUTORGCD                                                                  ")
        SQLBldr.AppendLine("INNER JOIN (                                                                                                 ")
        SQLBldr.AppendLine("    SELECT                                                                                                   ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                                          ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                          ")
        SQLBldr.AppendLine("       ,T1.INVOICEOUTORGCD                                                                                   ")
        SQLBldr.AppendLine("       ,T1.KEIJOORGCD                                                                                        ")
        SQLBldr.AppendLine("       ,T1.SEGMENTCODE                                                                                       ")
        SQLBldr.AppendLine("       ,T1.TAXRATE                                                                                           ")
        SQLBldr.AppendLine("       ,ROW_NUMBER() OVER (PARTITION BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD                         ")
        SQLBldr.AppendLine("                           ORDER BY T1.KEIJYOYM, T1.TORICODE,                                                ")
        SQLBldr.AppendLine("                                    T1.INVOICEOUTORGCD, T1.KEIJOORGCD, T1.SEGMENTCODE, T1.TAXRATE) AS SORT2  ")
        SQLBldr.AppendLine("    FROM (                                                                                                   ")
        SQLBldr.AppendLine("        SELECT                                                                                               ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                                      ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                                      ")
        SQLBldr.AppendLine("           ,T1.INVOICEOUTORGCD                                                                               ")
        SQLBldr.AppendLine("           ,T1.KEIJOORGCD                                                                                    ")
        SQLBldr.AppendLine("           ,CASE                                                                                             ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                                 ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '10' THEN 30201                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '15' THEN 30202                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '11' THEN 30203                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '05' THEN 30204                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '30' THEN 30205                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' THEN 30206                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '25' THEN 30208                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '20' THEN 30209                                                           ")
        SQLBldr.AppendLine("                ELSE 99999                                                                                   ")
        SQLBldr.AppendLine("            END                                                AS SEGMENTCODE                                ")
        SQLBldr.AppendLine("           ,T1.TAXRATE                                                                                       ")
        SQLBldr.AppendLine("        FROM                                                                                                 ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                                         ")
        SQLBldr.AppendLine("        INNER JOIN                                                                                           ")
        SQLBldr.AppendLine("            LNG.LNM0002_RECONM T2                                                                            ")
        SQLBldr.AppendLine("        ON                                                                                                   ")
        SQLBldr.AppendLine("            T2.CTNTYPE = T1.CTNTYPE                                                                          ")
        SQLBldr.AppendLine("        AND T2.CTNNO   = T1.CTNNO                                                                            ")
        SQLBldr.AppendLine("        GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD, T1.KEIJOORGCD                                 ")
        SQLBldr.AppendLine("                ,CASE                                                                                        ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                            ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '10' THEN 30201                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '15' THEN 30202                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '11' THEN 30203                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '05' THEN 30204                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '30' THEN 30205                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '35' THEN 30206                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '25' THEN 30208                                                      ")
        SQLBldr.AppendLine("                     WHEN T2.BIGCTNCD = '20' THEN 30209                                                      ")
        SQLBldr.AppendLine("                     ELSE 99999                                                                              ")
        SQLBldr.AppendLine("                 END                                                                                         ")
        SQLBldr.AppendLine("                ,T1.TAXRATE                                                                                  ")
        SQLBldr.AppendLine("    ) T1                                                                                                     ")
        SQLBldr.AppendLine(") T4                                                                                                         ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T4.KEIJYOYM        = T1.KEIJYOYM                                                                         ")
        SQLBldr.AppendLine("AND T4.TORICODE        = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("AND T4.INVOICEOUTORGCD = T1.INVOICEOUTORGCD                                                                  ")
        SQLBldr.AppendLine("AND T4.KEIJOORGCD      = T1.KEIJOORGCD                                                                       ")
        SQLBldr.AppendLine("AND T4.SEGMENTCODE     = CASE                                                                                ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                    ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '10' THEN 30201                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '15' THEN 30202                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '11' THEN 30203                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '05' THEN 30204                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '30' THEN 30205                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '35' THEN 30206                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '25' THEN 30208                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '20' THEN 30209                                              ")
        SQLBldr.AppendLine("                             ELSE 99999                                                                      ")
        SQLBldr.AppendLine("                         END                                                                                 ")
        SQLBldr.AppendLine("AND T4.TAXRATE         = T1.TAXRATE                                                                          ")
        SQLBldr.AppendLine("LEFT OUTER  JOIN (                                                                                           ")
        SQLBldr.AppendLine("    SELECT                                                                                                   ")
        SQLBldr.AppendLine("        T1.KEIJYOYM                                                                                          ")
        SQLBldr.AppendLine("       ,T1.TORICODE                                                                                          ")
        SQLBldr.AppendLine("       ,T1.INVOICEOUTORGCD                                                                                   ")
        SQLBldr.AppendLine("       ,T1.KEIJOORGCD                                                                                        ")
        SQLBldr.AppendLine("       ,T1.SEGMENTCODE                                                                                       ")
        SQLBldr.AppendLine("       ,T1.TAXRATE                                                                                           ")
        SQLBldr.AppendLine("       ,SUM(T1.LEASEAMOUNT)      AS LEASEAMOUNT                                                              ")
        SQLBldr.AppendLine("    FROM (                                                                                                   ")
        SQLBldr.AppendLine("        SELECT                                                                                               ")
        SQLBldr.AppendLine("            T1.KEIJYOYM                                                                                      ")
        SQLBldr.AppendLine("           ,T1.TORICODE                                                                                      ")
        SQLBldr.AppendLine("           ,T1.INVOICEOUTORGCD                                                                               ")
        SQLBldr.AppendLine("           ,T1.KEIJOORGCD                                                                                    ")
        SQLBldr.AppendLine("           ,CASE                                                                                             ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                                 ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '10' THEN 30201                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '15' THEN 30202                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '11' THEN 30203                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '05' THEN 30204                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '30' THEN 30205                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '35' THEN 30206                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '25' THEN 30208                                                           ")
        SQLBldr.AppendLine("                WHEN T2.BIGCTNCD = '20' THEN 30209                                                           ")
        SQLBldr.AppendLine("                ELSE 99999                                                                                   ")
        SQLBldr.AppendLine("            END                                                AS SEGMENTCODE                                ")
        SQLBldr.AppendLine("           ,T1.TAXRATE                                                                                       ")
        SQLBldr.AppendLine("           ,ROUND(T1.PAYMONTHLYAMOUNT * (T1.TAXRATE / 100), 0) AS LEASEAMOUNT                                ")
        SQLBldr.AppendLine("        FROM                                                                                                 ")
        SQLBldr.AppendLine("            LNG.LNT0065_FL_LEVIES T1                                                                         ")
        SQLBldr.AppendLine("        INNER JOIN                                                                                           ")
        SQLBldr.AppendLine("            LNG.LNM0002_RECONM T2                                                                            ")
        SQLBldr.AppendLine("        ON                                                                                                   ")
        SQLBldr.AppendLine("            T2.CTNTYPE = T1.CTNTYPE                                                                          ")
        SQLBldr.AppendLine("        AND T2.CTNNO   = T1.CTNNO                                                                            ")
        SQLBldr.AppendLine("    ) T1                                                                                                     ")
        SQLBldr.AppendLine("    GROUP BY T1.KEIJYOYM, T1.TORICODE, T1.INVOICEOUTORGCD, T1.KEIJOORGCD, T1.SEGMENTCODE, T1.TAXRATE         ")
        SQLBldr.AppendLine(") T5                                                                                                         ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T5.KEIJYOYM        = T1.KEIJYOYM                                                                         ")
        SQLBldr.AppendLine("AND T5.TORICODE        = T1.TORICODE                                                                         ")
        SQLBldr.AppendLine("AND T5.INVOICEOUTORGCD = T1.INVOICEOUTORGCD                                                                  ")
        SQLBldr.AppendLine("AND T5.KEIJOORGCD      = T1.KEIJOORGCD                                                                       ")
        SQLBldr.AppendLine("AND T5.SEGMENTCODE     = CASE                                                                                ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN 30207                    ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '10' THEN 30201                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '15' THEN 30202                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '11' THEN 30203                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '05' THEN 30204                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '30' THEN 30205                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '35' THEN 30206                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '25' THEN 30208                                              ")
        SQLBldr.AppendLine("                             WHEN T2.BIGCTNCD = '20' THEN 30209                                              ")
        SQLBldr.AppendLine("                             ELSE 99999                                                                      ")
        SQLBldr.AppendLine("                         END                                                                                 ")
        SQLBldr.AppendLine("AND T5.TAXRATE         = T1.TAXRATE                                                                          ")
        SQLBldr.AppendLine("INNER JOIN                                                                                                   ")
        SQLBldr.AppendLine("    com.LNS0019_ORG T6                                                                                       ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T6.CAMPCODE = '01'                                                                                       ")
        SQLBldr.AppendLine("AND T6.ORGCODE  = T1.KEIJOORGCD                                                                              ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T6.STYMD AND T6.ENDYMD                                                                 ")
        SQLBldr.AppendLine("INNER JOIN(                                                                                                  ")
        SQLBldr.AppendLine("    SELECT                                                                                                   ")
        SQLBldr.AppendLine("        T1.TORICODE                                                                                          ")
        SQLBldr.AppendLine("       ,coalesce(T1.TORINAME, '') + coalesce(T1.TORIDIVNAME, '') AS TORINAME                                     ")
        SQLBldr.AppendLine("    FROM                                                                                                     ")
        SQLBldr.AppendLine("        LNG.LNM0024_KEKKJM T1                                                                                ")
        SQLBldr.AppendLine("    WHERE                                                                                                    ")
        SQLBldr.AppendLine("        T1.DELFLG = '0'                                                                                      ")
        SQLBldr.AppendLine("    GROUP BY                                                                                                 ")
        SQLBldr.AppendLine("        T1.TORICODE, T1.TORINAME, T1.TORIDIVNAME                                                             ")
        SQLBldr.AppendLine(") T7                                                                                                         ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T7.TORICODE = T1.TORICODE                                                                                ")
        SQLBldr.AppendLine("INNER JOIN                                                                                                   ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T8                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T8.CAMPCODE = '01'                                                                                       ")
        SQLBldr.AppendLine("AND T8.CLASS    = 'ACCOUNTCODE'                                                                              ")
        SQLBldr.AppendLine("AND T8.KEYCODE  = '11030416'                                                                                 ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T8.STYMD AND T8.ENDYMD                                                                 ")
        SQLBldr.AppendLine("INNER JOIN                                                                                                   ")
        SQLBldr.AppendLine("    com.LNS0006_FIXVALUE T9                                                                                  ")
        SQLBldr.AppendLine("ON                                                                                                           ")
        SQLBldr.AppendLine("    T9.CAMPCODE = '01'                                                                                       ")
        SQLBldr.AppendLine("AND T9.CLASS    = 'SEGMENTCODE'                                                                              ")
        SQLBldr.AppendLine("AND T9.KEYCODE  = CASE                                                                                       ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20' THEN '30207'                         ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '10' THEN '30201'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '15' THEN '30202'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '11' THEN '30203'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '05' THEN '30204'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '30' THEN '30205'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '35' THEN '30206'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '25' THEN '30208'                                                   ")
        SQLBldr.AppendLine("                      WHEN T2.BIGCTNCD = '20' THEN '30209'                                                   ")
        SQLBldr.AppendLine("                      ELSE 90101                                                                             ")
        SQLBldr.AppendLine("                  END                                                                                        ")
        SQLBldr.AppendLine("AND CURDATE() BETWEEN T9.STYMD AND T9.ENDYMD                                                                 ")
        SQLBldr.AppendLine("WHERE                                                                                                        ")
        SQLBldr.AppendLine("    T1.KEIJYOYM = '" & htParm(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString & "'                                ")
        SQLBldr.AppendLine("AND T1.DELFLG   = '" & htParm(SELECT_ACCOUNTING_KEY.SP_DELFLG).ToString & "'                                 ")
        SQLBldr.AppendLine("GROUP BY                                                                                                     ")
        SQLBldr.AppendLine("    T1.KEIJYOYM                                                                                              ")
        SQLBldr.AppendLine("   ,T1.TORICODE                                                                                              ")
        SQLBldr.AppendLine("   ,T1.INVOICEOUTORGCD                                                                                       ")
        SQLBldr.AppendLine("   ,T1.KEIJOORGCD                                                                                            ")
        SQLBldr.AppendLine("   ,CASE                                                                                                     ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '35' AND T2.MIDDLECTNCD = '20'  THEN 30207                                        ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '10' THEN 30201                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '15' THEN 30202                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '11' THEN 30203                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '05' THEN 30204                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '30' THEN 30205                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '35' THEN 30206                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '25' THEN 30208                                                                   ")
        SQLBldr.AppendLine("        WHEN T2.BIGCTNCD = '20' THEN 30209                                                                   ")
        SQLBldr.AppendLine("        ELSE 99999                                                                                           ")
        SQLBldr.AppendLine("    END                                                                                                      ")
        SQLBldr.AppendLine("   ,T1.TAXRATE                                                                                               ")
        SQLBldr.AppendLine("ORDER BY                                                                                                     ")
        SQLBldr.AppendLine("    伝票番号, 明細行番号                                                                                     ")
#End Region

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

#End Region

#End Region

End Class
