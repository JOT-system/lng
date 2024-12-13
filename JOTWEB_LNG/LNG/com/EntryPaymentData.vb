Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>コンテナ清算ファイル 検索用</description></item>
''' </list>
''' </remarks>
Public Enum RESSNF_KEY
    SL_KEIJOYM              '計上年月
    SL_TORICODE             '取引先コード
    SL_PAYFILINGBRANCH      '支払項目 支払書提出支店
    SL_DEPOSITYMD           '入金年月日
    SL_DEPOSITYMDHEAD       '入金年月日（ヘッダー）
    SL_DEPOSITMONTHKBN      '入金月区分
    SL_CLOSINGDAY           '締日
    SL_STACKFREEKBN         '積空区分
    SL_ACCOUNTINGASSETSKBN  '経理資産区分
    SL_ACCOUNTINGMONTH      '計上月区分
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>支払ヘッダー項目</description></item>
''' </list>
''' </remarks>
Public Enum PAYHEAD_PARM
    PM_PAYMENTYM            '支払年月
    PM_PAYMENTNUMBER        '支払番号
    PM_PAYMENTORGCODE       '支払支店コード
    PM_TORICODE             '支払取引先コード
    PM_PAYMENTTYPE          '支払書種類
    PM_SCHEDATEPAYMENT      '支払予定日
    PM_SCHEDATEPAYMENTHEAD  '支払予定日（ヘッダー）
    PM_ACCOUNTINGMONTH      '計上月区分
    PM_CLOSINGDAY           '計上締日
    PM_PAYMENTQTY           'コンテナ個数
    PM_PAYMENTTOTAL         '支払額合計
    PM_PAYMENTCTAX          '支払額消費税
    PM_PAYMENTFARE          '適用運賃計
    PM_PAYMENTOTHERFEE      'その他料金計
    PM_PAYMENTSHIPPINGFEE   '発送料計
    PM_PAYMENTLINK          '支払連携状態
    PM_REQUESTSTATUS        '支払申請状態
    PM_REJECTIONCOMMENT     '却下理由
    PM_RQSTAFF              '担当者
    PM_RQDATE               '担当処理日時
    PM_RQACKNOWLEDGER       '確認者
    PM_RQACKNDATE           '確認処理日時
    PM_PAYADDSUB            '加減額
    PM_REMARKS              '備考
    PM_UPDFLG               '変更フラグ
    PM_DELFLG               '削除フラグ
    PM_INITYMD              '登録年月日
    PM_INITUSER             '登録ユーザーID
    PM_INITTERMID           '登録端末
    PM_INITPGID             '登録プログラムID
    PM_UPDYMD               '更新年月日
    PM_UPDUSER              '更新ユーザーID
    PM_UPDTERMID            '更新端末
    PM_UPDPGID              '更新プログラムID
    PM_RECEIVEYMD           '集信日時
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>支払明細回送日費データ 登録用</description></item>
''' </list>
''' </remarks>
Public Enum PAYMENTDATA_PARM
    PM_PAYMENTYM            '支払年月
    PM_PAYMENTNUMBER        '支払番号
    PM_PAYMENTORGCODE       '支払支店コード
    PM_TORICODE             '支払取引先コード
    PM_PAYMENTTYPE          '支払書種類
    PM_SHIPYMD              '発送年月日
    PM_SAMEDAYCNT           '同日内回数
    PM_CTNLINENO            '行番
    PM_KEIJYOBRANCHCD       '計上支店コード
    PM_AMOUNTTYPE           '金額種別
    PM_CTNTYPE              'コンテナ記号
    PM_CTNNO                'コンテナ番号
    PM_SEQNO                'SEQNO
    PM_JOTDEPBRANCHCD       'JOT発組織コード
    PM_DEPSTATION           '発駅コード
    PM_DEPTRUSTEECD         '発受託人コード
    PM_DEPTRUSTEESUBCD      '発受託人サブ
    PM_JOTARRBRANCHCD       'JOT着組織コード
    PM_ARRSTATION           '着駅コード
    PM_ARRTRUSTEECD         '着受託人コード
    PM_ARRTRUSTEESUBCD      '着受託人サブ
    PM_JRITEMCD             'JR品目コード
    PM_PAYMENTTOTAL         '支払額
    PM_PAYMENTCTAX          '支払額消費税
    PM_PAYMENTFARE          '適用運賃
    PM_PAYMENTOTHERFEE      'その他料金
    PM_SHIPFEE              '発送料
    PM_TAXKBN               '税区分
    PM_TAXRATE              '税率
    PM_PAYADDSUB            '加減額
    PM_REMARKS              '備考
    PM_INOUTSIDEKBN         '内外区分
    PM_BILLINGFEE           '請求額
    PM_DELFLG               '削除フラグ
    PM_INITYMD              '登録年月日
    PM_INITUSER             '登録ユーザーID
    PM_INITTERMID           '登録端末
    PM_INITPGID             '登録プログラムID
    PM_RECEIVEYMD           '集信日時
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>支払連携用CSV 検索用</description></item>
''' </list>
''' </remarks>
Public Enum PAYMENTLINK_KEY
    PM_CORPCODE             '会社コード
    PM_PAYMENTYM            '支払年月
    PM_PAYMENTORGCODE       '支払支店コード
    PM_TORICODE             '支払取引先コード
    PM_SCHEDATEPAYMENT      '支払予定日
    PM_SCHEDATEPAYMENTHEAD  '支払予定日（ヘッダー）
    PM_DEPOSITMONTHKBN      '入金月区分
    PM_CLOSINGDAY           '締日
    PM_USERID               'ユーザーID
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>コンテナ清算ファイル 検索用</description></item>
''' </list>
''' </remarks>
Public Enum DRAFT_PAYMENTLOG_KEY
    DRAFT_KEIJOYM               '支払年月
    DRAFT_TORICODE              '支払取引先コード
    DRAFT_PAYMENTORGCODE        '支払支店コード
    DRAFT_SCHEDATEPAYMENT       '支払日
    DRAFT_SCHEDATEPAYMENTHEAD   '支払日（ヘッダー）
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>ドラフト版請求書連携実績テーブル 登録用(主キー)</description></item>
''' </list>
''' </remarks>
Public Enum INSERT_DRAFTPAYMENTLOG
    IP_KEIJOYM               '支払年月
    IP_TORICODE              '支払取引先コード
    IP_PAYMENTORGCODE        '支払支店コード
    IP_SCHEDATEPAYMENT       '支払日
    IP_SCHEDATEPAYMENTHEAD   '支払日（ヘッダー）
    IP_DELFLG                '削除フラグ
    IP_INITYMD               '登録年月日
    IP_INITUSER              '登録ユーザーID
    IP_INITTERMID            '登録端末
    IP_INITPGID              '登録プログラムID
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>ドラフト版請求書連携 検索用(主キー)</description></item>
''' </list>
''' </remarks>
Public Enum DRAFTPAYMENTLINK_KEY
    SL_PAYMENTYM             '支払年月
    SL_TORICODE              '支払取引先コード
    SL_PAYMENTORGCODE        '支払支店コード
    SL_SCHEDATEPAYMENT       '支払日
    SL_SCHEDATEPAYMENTHEAD   '支払日（ヘッダー）
    SL_CAMPCODE              '会社コード
    SL_LOGIN_USER            '操作ユーザー
    SL_NOWDATE               '現在日付
End Enum

''' <summary>
''' 請求ヘッダーデータ登録クラス
''' </summary>
''' <remarks>各種請求ヘッダーデータに登録する際はこちらに定義</remarks>
Public Class EntryPaymentData

    Private Const CONST_BAIKYAKU_DATE As Integer = 202310         '売却コンテナ利用開始日付
    Private Const CONST_BAIKYAKU_KAMOKU As String = "J-51110105"  '売却コンテナ科目コード

    ''' <summary>
    ''' 新規用のORDERNOを取得する
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <remarks></remarks>
    Public Shared Function GetNewOrderNo(sqlCon As MySqlConnection, sqlTran As MySqlTransaction) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable()
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim strSequenceNo As String = ""

        With sqlText
            .AppendLine("SELECT")
            .AppendLine("     FORMAT(CURDATE(),'yyyyMMdd') + FORMAT(NEXT VALUE FOR LNG.payment_sequence,'00000') AS PAYMENTNO")
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            strSequenceNo = GetStringValue(sqlRetSet, 0, "PAYMENTNO")
        End If

        Return strSequenceNo

    End Function

    ''' <summary>
    ''' DataTableの指定位置からString値を取得する
    ''' </summary>
    ''' <param name="objOutputData">DataTable</param>
    ''' <param name="nRow">行</param>
    ''' <param name="strCol">列</param>
    ''' <param name="strDefault">規定値</param>
    ''' <returns>取得データ</returns>
    ''' <remarks>値がDBNULLの場合は規定値が返却される</remarks>
    Private Shared Function GetStringValue(ByVal objOutputData As DataTable, ByVal nRow As Integer, ByVal strCol As String, Optional ByVal strDefault As String = "") As String
        Dim strRet As String = strDefault
        Dim objCell As Object = objOutputData.Rows(nRow)(strCol)

        If Not IsDBNull(objCell) Then
            strRet = objCell.ToString
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' 支払ヘッダーデータ 検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectInvoiceHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    PAYMENTYM")                                             '支払年月
        SQLBldr.AppendLine("    , PAYMENTNUMBER")                                       '支払番号
        SQLBldr.AppendLine("    , PAYMENTORGCODE")                                      '支払支店コード
        SQLBldr.AppendLine("    , TORICODE")                                            '支払取引先コード
        SQLBldr.AppendLine("    , PAYMENTTYPE")                                         '支払書種類
        SQLBldr.AppendLine("    , SCHEDATEPAYMENT")                                     '支払予定日
        SQLBldr.AppendLine("    , ACCOUNTINGMONTH")                                     '計上月区分
        SQLBldr.AppendLine("    , CLOSINGDAY")                                          '計上締日
        SQLBldr.AppendLine("    , PAYMENTQTY")                                          'コンテナ個数
        SQLBldr.AppendLine("    , PAYMENTTOTAL")                                        '支払額合計
        SQLBldr.AppendLine("    , PAYMENTCTAX")                                         '支払額消費税
        SQLBldr.AppendLine("    , PAYMENTFARE")                                         '適用運賃計
        SQLBldr.AppendLine("    , PAYMENTOTHERFEE")                                     'その他料計
        SQLBldr.AppendLine("    , PAYMENTSHIPPINGFEE")                                  '発送料計
        SQLBldr.AppendLine("    , PAYMENTLINK")                                         '支払連携状態
        SQLBldr.AppendLine("    , REQUESTSTATUS")                                       '支払申請状態
        SQLBldr.AppendLine("    , REJECTIONCOMMENT")                                    '却下理由
        SQLBldr.AppendLine("    , RQSTAFF")                                             '担当ユーザーID
        SQLBldr.AppendLine("    , RQDATE")                                              '担当処理日時
        SQLBldr.AppendLine("    , RQACKNOWLEDGER")                                      '確認ユーザーID
        SQLBldr.AppendLine("    , RQACKNDATE")                                          '確認処理日時
        SQLBldr.AppendLine("    , PAYADDSUB")                                           '加減額
        SQLBldr.AppendLine("    , REMARKS")                                             '備考
        SQLBldr.AppendLine("    , DELFLG")                                              '削除フラグ
        SQLBldr.AppendLine("    , INITYMD")                                             '登録年月日
        SQLBldr.AppendLine("    , INITUSER")                                            '登録ユーザーID
        SQLBldr.AppendLine("    , INITTERMID")                                          '登録端末
        SQLBldr.AppendLine("    , INITPGID")                                            '登録プログラムID
        SQLBldr.AppendLine("    , UPDYMD")                                              '更新年月日
        SQLBldr.AppendLine("    , UPDUSER")                                             '更新ユーザーID
        SQLBldr.AppendLine("    , UPDTERMID")                                           '更新端末
        SQLBldr.AppendLine("    , RECEIVEYMD")                                          '集信日時
        SQLBldr.AppendLine("    , UPDTIMSTP")                                           'タイムスタンプ
        SQLBldr.AppendLine("FROM")
        'メイン 請求ヘッダーデータ
        SQLBldr.AppendLine("    lng.LNT0077_PAYMENTHEAD")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        '請求年月
        SQLBldr.AppendLine("    PAYMENTYM = '" & htParm(PAYHEAD_PARM.PM_PAYMENTYM).ToString & "'")
        '請求番号
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_PAYMENTNUMBER).ToString) Then
            SQLBldr.AppendLine("    AND INVOICENUMBER = '" & htParm(PAYHEAD_PARM.PM_PAYMENTNUMBER).ToString & "'")
        End If
        '請求担当部店コード
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_PAYMENTORGCODE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTORGCODE = '" & htParm(PAYHEAD_PARM.PM_PAYMENTORGCODE).ToString & "'")
        End If
        '請求取引先コード
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_TORICODE).ToString) Then
            SQLBldr.AppendLine("    AND TORICODE = '" & htParm(PAYHEAD_PARM.PM_TORICODE).ToString & "'")
        End If
        '入金予定日
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_SCHEDATEPAYMENT).ToString) Then
            SQLBldr.AppendLine("    AND SCHEDATEPAYMENT = '" & htParm(PAYHEAD_PARM.PM_SCHEDATEPAYMENT).ToString & "'")
        End If
        '請求書種類
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_PAYMENTTYPE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTTYPE = '" & htParm(PAYHEAD_PARM.PM_PAYMENTTYPE).ToString & "'")
        End If
        '削除フラグ
        If Not String.IsNullOrEmpty(htParm(PAYHEAD_PARM.PM_DELFLG).ToString) Then
            SQLBldr.AppendLine("    AND DELFLG = '" & htParm(PAYHEAD_PARM.PM_DELFLG).ToString & "'")
        End If

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    PAYMENTYM")
        SQLBldr.AppendLine("    , PAYMENTNUMBER")
        SQLBldr.AppendLine("    , PAYMENTORGCODE")
        SQLBldr.AppendLine("    , TORICODE")
        SQLBldr.AppendLine("    , PAYMENTTYPE")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

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
    ''' 支払ヘッダーデータ　登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">支払ヘッダーデータ</param>
    Public Shared Sub InsertPayHeadData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("INSERT INTO LNG.LNT0077_PAYMENTHEAD (")
        sqlPaymentStat.AppendLine("    PAYMENTYM")              '支払年月
        sqlPaymentStat.AppendLine("  , PAYMENTNUMBER")          '支払番号
        sqlPaymentStat.AppendLine("  , PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("  , TORICODE")               '支払取引先コード
        sqlPaymentStat.AppendLine("  , PAYMENTTYPE")            '支払書種類
        sqlPaymentStat.AppendLine("  , SCHEDATEPAYMENT")        '支払予定日
        sqlPaymentStat.AppendLine("  , ACCOUNTINGMONTH")        '計上月区分
        sqlPaymentStat.AppendLine("  , CLOSINGDAY")             '計上締日
        sqlPaymentStat.AppendLine("  , PAYMENTQTY")             'コンテナ個数
        sqlPaymentStat.AppendLine("  , PAYMENTTOTAL")           '支払額合計
        sqlPaymentStat.AppendLine("  , PAYMENTCTAX")            '支払額消費税
        sqlPaymentStat.AppendLine("  , PAYMENTFARE")            '適用運賃計
        sqlPaymentStat.AppendLine("  , PAYMENTOTHERFEE")        'その他料金計
        sqlPaymentStat.AppendLine("  , PAYMENTSHIPPINGFEE")     '発送料計
        sqlPaymentStat.AppendLine("  , PAYMENTLINK")            '支払連携状態
        sqlPaymentStat.AppendLine("  , REQUESTSTATUS")          '支払申請状態
        sqlPaymentStat.AppendLine("  , REJECTIONCOMMENT")       '却下理由
        sqlPaymentStat.AppendLine("  , RQSTAFF")                '担当ユーザーID
        sqlPaymentStat.AppendLine("  , RQDATE")                 '担当処理日時
        sqlPaymentStat.AppendLine("  , RQACKNOWLEDGER")         '確認ユーザーID
        sqlPaymentStat.AppendLine("  , RQACKNDATE")             '確認処理日時
        sqlPaymentStat.AppendLine("  , PAYADDSUB")              '加減額
        sqlPaymentStat.AppendLine("  , REMARKS")                '備考
        sqlPaymentStat.AppendLine("  , DELFLG")                 '削除フラグ
        sqlPaymentStat.AppendLine("  , UPDATEFLG")              '変更ありフラグ
        sqlPaymentStat.AppendLine("  , INITYMD")                '登録年月日
        sqlPaymentStat.AppendLine("  , INITUSER")               '登録ユーザーID
        sqlPaymentStat.AppendLine("  , INITTERMID")             '登録端末
        sqlPaymentStat.AppendLine("  , INITPGID")               '登録プログラムＩＤ
        sqlPaymentStat.AppendLine("  , RECEIVEYMD")             '集信日時
        sqlPaymentStat.AppendLine(")")
        sqlPaymentStat.AppendLine(" VALUES(")
        sqlPaymentStat.AppendLine("    @PAYMENTYM")             '支払年月
        sqlPaymentStat.AppendLine("  , @PAYMENTNUMBER")         '支払番号
        sqlPaymentStat.AppendLine("  , @PAYMENTORGCODE")        '支払支店コード
        sqlPaymentStat.AppendLine("  , @TORICODE")              '支払取引先コード
        sqlPaymentStat.AppendLine("  , @PAYMENTTYPE")           '支払書種類
        sqlPaymentStat.AppendLine("  , @SCHEDATEPAYMENT")       '支払予定日
        sqlPaymentStat.AppendLine("  , @ACCOUNTINGMONTH")       '計上月区分
        sqlPaymentStat.AppendLine("  , @CLOSINGDAY")            '計上締日
        sqlPaymentStat.AppendLine("  , @PAYMENTQTY")            'コンテナ個数
        sqlPaymentStat.AppendLine("  , @PAYMENTTOTAL")          '支払額合計
        sqlPaymentStat.AppendLine("  , @PAYMENTCTAX")           '支払額消費税
        sqlPaymentStat.AppendLine("  , @PAYMENTFARE")           '適用運賃計
        sqlPaymentStat.AppendLine("  , @PAYMENTOTHERFEE")       'その他料金計
        sqlPaymentStat.AppendLine("  , @PAYMENTSHIPPINGFEE")    '発送料計
        sqlPaymentStat.AppendLine("  , @PAYMENTLINK")           '支払連携状態
        sqlPaymentStat.AppendLine("  , @REQUESTSTATUS")         '支払申請状態
        sqlPaymentStat.AppendLine("  , @REJECTIONCOMMENT")      '却下理由
        sqlPaymentStat.AppendLine("  , @RQSTAFF")               '担当ユーザーID
        sqlPaymentStat.AppendLine("  , @RQDATE")                '担当処理日時
        sqlPaymentStat.AppendLine("  , @RQACKNOWLEDGER")        '確認ユーザーID
        sqlPaymentStat.AppendLine("  , @RQACKNDATE")            '確認処理日時
        sqlPaymentStat.AppendLine("  , @PAYADDSUB")             '加減額
        sqlPaymentStat.AppendLine("  , @REMARKS")               '備考
        sqlPaymentStat.AppendLine("  , @UPDATEFLG")             '変更ありフラグ
        sqlPaymentStat.AppendLine("  , @DELFLG")                '削除フラグ
        sqlPaymentStat.AppendLine("  , @INITYMD")               '登録年月日
        sqlPaymentStat.AppendLine("  , @INITUSER")              '登録ユーザーID
        sqlPaymentStat.AppendLine("  , @INITTERMID")            '登録端末
        sqlPaymentStat.AppendLine("  , @INITPGID")              '登録プログラムＩＤ
        sqlPaymentStat.AppendLine("  , @RECEIVEYMD")            '集信日時
        sqlPaymentStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTYM)                                     '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER)                             '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE)                           '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_TORICODE)                                       '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTTYPE)                                 '支払書種類
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENT))              '支払予定日
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_ACCOUNTINGMONTH))          '計上月区分
                .Add("CLOSINGDAY", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_CLOSINGDAY))                         '計上締日
                .Add("PAYMENTQTY", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTQTY))                         'コンテナ個数
                .Add("PAYMENTTOTAL", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTTOTAL))                   '支払額合計
                .Add("PAYMENTCTAX", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTCTAX))                     '支払額消費税
                .Add("PAYMENTFARE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTFARE))                     '適用運賃計
                .Add("PAYMENTOTHERFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTOTHERFEE))             'その他料金計
                .Add("PAYMENTSHIPPINGFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTSHIPPINGFEE))       '発送料計
                .Add("PAYMENTLINK", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTLINK))                  '支払連携状態
                .Add("REQUESTSTATUS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REQUESTSTATUS))              '支払申請状態
                .Add("REJECTIONCOMMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REJECTIONCOMMENT))        '却下理由
                .Add("RQSTAFF", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQSTAFF))                          '担当ユーザーID
                .Add("RQDATE", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQDATE))                            '担当処理日時
                .Add("RQACKNOWLEDGER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNOWLEDGER))            '確認ユーザーID
                .Add("RQACKNDATE", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNDATE))                    '確認処理日時
                .Add("PAYADDSUB", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYADDSUB))                         '加減額
                .Add("REMARKS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REMARKS))                          '備考
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDFLG))                         '変更ありフラグ
                .Add("DELFLG", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_DELFLG))                                 '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_INITYMD))                          '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_INITUSER))                        '登録ユーザーID
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_INITTERMID))                    '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_INITPGID))                        '登録プログラムID
                .Add("RECEIVEYMD", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RECEIVEYMD))                    '集信日時
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダーデータ　更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">請求ヘッダーデータ</param>
    Public Shared Sub UpdatePayHeadData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD ")
        sqlPaymentStat.AppendLine("SET")
        sqlPaymentStat.AppendLine("    REQUESTSTATUS = @REQUESTSTATUS")             '支払申請状態
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTLINK)).ToString) Then
            sqlPaymentStat.AppendLine("  , PAYMENTLINK = @PAYMENTLINK")             '支払連携状態
        End If
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQSTAFF)).ToString) Then
            sqlPaymentStat.AppendLine("  , RQSTAFF = @RQSTAFF")                     '担当ユーザーID
        End If
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQDATE)).ToString) Then
            sqlPaymentStat.AppendLine("  , RQDATE = @RQDATE")                       '担当処理日時
        End If
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNDATE)).ToString) Then
            sqlPaymentStat.AppendLine("  , RQACKNDATE = @RQACKNDATE")               '確認処理日時
        End If
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNOWLEDGER)).ToString) Then
            sqlPaymentStat.AppendLine("  , RQACKNOWLEDGER = @RQACKNOWLEDGER")       '確認ユーザーID
        End If
        If Not String.IsNullOrEmpty(BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REJECTIONCOMMENT)).ToString) Then
            sqlPaymentStat.AppendLine("  , REJECTIONCOMMENT = @REJECTIONCOMMENT")   '却下理由
        End If
        sqlPaymentStat.AppendLine("  , UPDYMD = @UPDYMD")                           '更新年月日
        sqlPaymentStat.AppendLine("  , UPDUSER = @UPDUSER")                         '更新ユーザーＩＤ
        sqlPaymentStat.AppendLine("  , UPDTERMID = @UPDTERMID")                     '更新端末
        sqlPaymentStat.AppendLine("  , UPDPGID = @UPDPGID")                         '更新プログラムID
        sqlPaymentStat.AppendLine("  , ACCOUNTINGMONTH = @ACCOUNTINGMONTH")         '計上月区分
        sqlPaymentStat.AppendLine("  , CLOSINGDAY = @CLOSINGDAY")                   '計上締日
        sqlPaymentStat.AppendLine("  , PAYMENTQTY = @PAYMENTQTY")                   'コンテナ個数
        sqlPaymentStat.AppendLine("  , PAYMENTTOTAL = @PAYMENTTOTAL")               '支払額合計
        sqlPaymentStat.AppendLine("  , PAYMENTCTAX = @PAYMENTCTAX")                 '支払額消費税
        sqlPaymentStat.AppendLine("  , PAYMENTFARE　= @PAYMENTFARE")                '適用運賃計
        sqlPaymentStat.AppendLine("  , PAYMENTOTHERFEE = @PAYMENTOTHERFEE")         'その他料合計
        sqlPaymentStat.AppendLine("  , PAYMENTSHIPPINGFEE = @PAYMENTSHIPPINGFEE")   '発送料計
        sqlPaymentStat.AppendLine("  , UPDATEFLG = @UPDATEFLG")                     '変更フラグ
        sqlPaymentStat.AppendLine("WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                     '支払年月
        sqlPaymentStat.AppendLine("AND PAYMENTNUMBER   = @PAYMENTNUMBER")           '支払番号
        sqlPaymentStat.AppendLine("AND PAYMENTORGCODE   = @PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("AND TORICODE = @TORICODE")                       '支払取引先コード
        sqlPaymentStat.AppendLine("AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT")         '支払予定日
        sqlPaymentStat.AppendLine("AND DELFLG = '0'")                               '削除フラグ

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTYM)                                     '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER)                             '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE)                           '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_TORICODE)                                       '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = htHeadData(PAYHEAD_PARM.PM_PAYMENTTYPE)                                 '支払書種類
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENT))              '支払予定日
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_ACCOUNTINGMONTH))          '計上月区分
                .Add("CLOSINGDAY", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_CLOSINGDAY))                         '計上締日
                .Add("PAYMENTQTY", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTQTY))                         'コンテナ個数
                .Add("PAYMENTTOTAL", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTTOTAL))                   '支払額合計
                .Add("PAYMENTCTAX", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTCTAX))                     '支払額消費税
                .Add("PAYMENTFARE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTFARE))                     '適用運賃計
                .Add("PAYMENTOTHERFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTOTHERFEE))             'その他料金計
                .Add("PAYMENTSHIPPINGFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTSHIPPINGFEE))       '発送料計
                .Add("PAYMENTLINK", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTLINK))                  '支払連携状態
                .Add("REQUESTSTATUS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REQUESTSTATUS))              '支払申請状態
                .Add("REJECTIONCOMMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REJECTIONCOMMENT))        '却下理由
                .Add("RQSTAFF", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQSTAFF))                          '担当ユーザーID
                .Add("RQDATE", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQDATE))                            '担当処理日時
                .Add("RQACKNOWLEDGER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNOWLEDGER))            '確認ユーザーID
                .Add("RQACKNDATE", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQACKNDATE))                    '確認処理日時
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDYMD))                      　　  '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDUSER))                          '更新ユーザーID
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDTERMID))                      '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDPGID))                          '更新プログラムID
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDFLG))                         '変更フラグ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub
    ''' <summary>
    ''' 支払ヘッダーデータ　更新処理（金額更新用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">請求ヘッダーデータ</param>
    Public Shared Sub UpdatePayHeadDataMoney(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD ")
        sqlPaymentStat.AppendLine("SET")
        sqlPaymentStat.AppendLine("    PAYADDSUB = @PAYADDSUB")                     '加減額
        sqlPaymentStat.AppendLine("  , REMARKS = @REMARKS")                         '備考
        sqlPaymentStat.AppendLine("  , PAYMENTTOTAL = @PAYMENTTOTAL")               '支払額合計
        sqlPaymentStat.AppendLine("  , PAYMENTCTAX = @PAYMENTCTAX")                 '支払額消費税
        sqlPaymentStat.AppendLine("  , PAYMENTFARE = @PAYMENTFARE")                 '適用運賃計
        sqlPaymentStat.AppendLine("  , PAYMENTOTHERFEE = @PAYMENTOTHERFEE")         'その他料計
        sqlPaymentStat.AppendLine("  , PAYMENTSHIPPINGFEE = @PAYMENTSHIPPINGFEE")   '発送料計
        sqlPaymentStat.AppendLine("  , PAYMENTQTY = @PAYMENTQTY")                   'コンテナ個数
        sqlPaymentStat.AppendLine("  , UPDATEFLG = @UPDATEFLG")                     '変更ありフラグ
        sqlPaymentStat.AppendLine("  , UPDYMD = @UPDYMD")                           '更新年月日
        sqlPaymentStat.AppendLine("  , UPDUSER = @UPDUSER")                         '更新ユーザーＩＤ
        sqlPaymentStat.AppendLine("  , UPDTERMID = @UPDTERMID")                     '更新端末
        sqlPaymentStat.AppendLine("  , UPDPGID = @UPDPGID")                         '更新プログラムID
        sqlPaymentStat.AppendLine("WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                     '支払年月
        sqlPaymentStat.AppendLine("AND PAYMENTNUMBER   = @PAYMENTNUMBER")           '支払番号
        sqlPaymentStat.AppendLine("AND PAYMENTORGCODE   = @PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("AND TORICODE = @TORICODE")                       '支払取引先コード
        sqlPaymentStat.AppendLine("AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT")         '支払予定日
        sqlPaymentStat.AppendLine("AND DELFLG = '0'")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTTOTAL", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTTOTAL))              '支払額合計
                .Add("PAYMENTCTAX", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTCTAX))                '支払額消費税
                .Add("PAYMENTFARE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTFARE))                '適用運賃計
                .Add("PAYMENTOTHERFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTOTHERFEE))        'その他料計
                .Add("PAYMENTSHIPPINGFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTSHIPPINGFEE))  '発送料計
                .Add("PAYADDSUB", MySqlDbType.Decimal).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYADDSUB))                    '加減額
                .Add("PAYMENTQTY", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTQTY))                    'コンテナ個数
                .Add("REMARKS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REMARKS))                     '備考
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDFLG))                    '変更あり
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDYMD))                       '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDUSER))                     '更新ユーザーID
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDTERMID))                 '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDPGID))                     '更新プログラムID
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTYM))                 '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER))         '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE))       '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_TORICODE))                   '支払取引先コード
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENT))         '支払予定日
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダーデータ　更新処理(取下、却下)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">請求ヘッダーデータ</param>
    Public Shared Sub UpdatePayHeadData_RE(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD")
        sqlPaymentStat.AppendLine("SET")
        sqlPaymentStat.AppendLine("    REQUESTSTATUS = @REQUESTSTATUS")             '支払申請状態
        sqlPaymentStat.AppendLine("  , RQDATE = @RQDATE")                           '担当処理日時
        sqlPaymentStat.AppendLine("  , UPDYMD = @UPDYMD")                           '更新年月日
        sqlPaymentStat.AppendLine("  , UPDUSER = @UPDUSER")                         '更新ユーザーＩＤ
        sqlPaymentStat.AppendLine("  , UPDTERMID = @UPDTERMID")                     '更新端末
        sqlPaymentStat.AppendLine("WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                     '支払年月
        sqlPaymentStat.AppendLine("AND PAYMENTNUMBER   = @PAYMENTNUMBER")           '支払番号
        sqlPaymentStat.AppendLine("AND PAYMENTORGCODE   = @PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("AND TORICODE = @TORICODE")                       '支払取引先コード
        sqlPaymentStat.AppendLine("AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT")         '支払予定日
        sqlPaymentStat.AppendLine("AND DELFLG = '0'")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("REQUESTSTATUS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REQUESTSTATUS))          '支払申請状態
                .Add("RQDATE", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_RQDATE))                        '担当処理日時
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDYMD))                        '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDUSER))                      '更新ユーザーID
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDTERMID))                  '更新端末
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTYM))                       '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER))               '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_TORICODE))                    '支払取引先コード
                .Add("SCHEDATEPAYMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENT))      '支払予定日
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダーデータ　更新処理(却下理由)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">請求ヘッダーデータ</param>
    Public Shared Sub UpdatePayHeadDataRejection(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD")
        sqlPaymentStat.AppendLine("SET")
        sqlPaymentStat.AppendLine("    REJECTIONCOMMENT = @REJECTIONCOMMENT")       '却下理由
        sqlPaymentStat.AppendLine("  , UPDYMD = @UPDYMD")                           '更新年月日
        sqlPaymentStat.AppendLine("  , UPDUSER = @UPDUSER")                         '更新ユーザーＩＤ
        sqlPaymentStat.AppendLine("  , UPDTERMID = @UPDTERMID")                     '更新端末
        sqlPaymentStat.AppendLine("  , UPDPGID = @UPDPGID")                         '更新端末
        sqlPaymentStat.AppendLine("WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                     '支払年月
        sqlPaymentStat.AppendLine("AND PAYMENTNUMBER   = @PAYMENTNUMBER")           '支払番号
        sqlPaymentStat.AppendLine("AND PAYMENTORGCODE   = @PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("AND TORICODE = @TORICODE")                       '支払取引先コード
        sqlPaymentStat.AppendLine("AND DELFLG = '0'")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("REJECTIONCOMMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_REJECTIONCOMMENT))    '却下理由
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDYMD))                        '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDUSER))                      '更新ユーザーID
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDTERMID))                  '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDPGID))                      '更新プログラムID
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTYM))                  '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER))          '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_TORICODE))                    '支払取引先コード
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払明細回送費データTBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求ヘッダーデータ</param>
    Public Shared Sub InsertPaymentData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("INSERT INTO LNG.LNT0078_PAYMENTDATA (")
        sqlPaymentStat.AppendLine("    PAYMENTYM")              '支払年月
        sqlPaymentStat.AppendLine("  , PAYMENTNUMBER")          '支払番号
        sqlPaymentStat.AppendLine("  , PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("  , TORICODE")               '支払取引先コード
        sqlPaymentStat.AppendLine("  , PAYMENTTYPE")            '支払書種類
        sqlPaymentStat.AppendLine("  , SHIPYMD")                '発送年月日
        sqlPaymentStat.AppendLine("  , SAMEDAYCNT")             '同日内回数
        sqlPaymentStat.AppendLine("  , CTNLINENO")              '行番
        sqlPaymentStat.AppendLine("  , KEIJYOBRANCHCD")         '計上支店コード
        sqlPaymentStat.AppendLine("  , AMOUNTTYPE")             '金額種別
        sqlPaymentStat.AppendLine("  , CTNTYPE")                'コンテナ記号
        sqlPaymentStat.AppendLine("  , CTNNO")                  'コンテナ番号
        sqlPaymentStat.AppendLine("  , SEQNO")                  'SEQNO
        sqlPaymentStat.AppendLine("  , JOTDEPBRANCHCD")         'JOT発組織コード
        sqlPaymentStat.AppendLine("  , DEPSTATION")             '発駅コード
        sqlPaymentStat.AppendLine("  , DEPTRUSTEECD")           '発受託人コード
        sqlPaymentStat.AppendLine("  , DEPTRUSTEESUBCD")        '発受託人サブ
        sqlPaymentStat.AppendLine("  , JOTARRBRANCHCD")         'JOT着組織コード
        sqlPaymentStat.AppendLine("  , ARRSTATION")             '着駅コード
        sqlPaymentStat.AppendLine("  , ARRTRUSTEECD")           '着受託人コード
        sqlPaymentStat.AppendLine("  , ARRTRUSTEESUBCD")        '着受託人サブ
        sqlPaymentStat.AppendLine("  , JRITEMCD")               'JR品目コード
        sqlPaymentStat.AppendLine("  , PAYMENTTOTAL")           '支払額
        sqlPaymentStat.AppendLine("  , PAYMENTCTAX")            '支払額消費税
        sqlPaymentStat.AppendLine("  , PAYMENTFARE")            '適用運賃
        sqlPaymentStat.AppendLine("  , PAYMENTOTHERFEE")        'その他料金
        sqlPaymentStat.AppendLine("  , PAYMENTSHIPPINGFEE")     '発送料
        sqlPaymentStat.AppendLine("  , TAXKBN")                 '税区分
        sqlPaymentStat.AppendLine("  , TAXRATE")                '税率
        sqlPaymentStat.AppendLine("  , PAYADDSUB")              '加減額
        sqlPaymentStat.AppendLine("  , REMARKS")                '備考
        sqlPaymentStat.AppendLine("  , INOUTSIDEKBN")           '内外区分
        sqlPaymentStat.AppendLine("  , DELFLG")                 '削除フラグ
        sqlPaymentStat.AppendLine("  , INITYMD")                '登録年月日
        sqlPaymentStat.AppendLine("  , INITUSER")               '登録ユーザーID
        sqlPaymentStat.AppendLine("  , INITTERMID")             '登録端末
        sqlPaymentStat.AppendLine("  , INITPGID")               '登録プログラムID
        sqlPaymentStat.AppendLine("  , RECEIVEYMD")             '集信日時
        sqlPaymentStat.AppendLine(")")
        sqlPaymentStat.AppendLine(" VALUES(")
        sqlPaymentStat.AppendLine("    @PAYMENTYM")             '支払年月
        sqlPaymentStat.AppendLine("  , @PAYMENTNUMBER")         '支払番号
        sqlPaymentStat.AppendLine("  , @PAYMENTORGCODE")        '支払支店コード
        sqlPaymentStat.AppendLine("  , @TORICODE")              '支払取引先コード
        sqlPaymentStat.AppendLine("  , @PAYMENTTYPE")           '支払書種類
        sqlPaymentStat.AppendLine("  , @SHIPYMD")               '発送年月日
        sqlPaymentStat.AppendLine("  , @SAMEDAYCNT")            '同日内回数
        sqlPaymentStat.AppendLine("  , @CTNLINENO")             '行番
        sqlPaymentStat.AppendLine("  , @KEIJYOBRANCHCD")        '計上支店コード
        sqlPaymentStat.AppendLine("  , @AMOUNTTYPE")            '金額種別
        sqlPaymentStat.AppendLine("  , @CTNTYPE")               'コンテナ記号
        sqlPaymentStat.AppendLine("  , @CTNNO")                 'コンテナ番号
        sqlPaymentStat.AppendLine("  , @SEQNO")                 'SEQNO
        sqlPaymentStat.AppendLine("  , @JOTDEPBRANCHCD")        'JOT発組織コード
        sqlPaymentStat.AppendLine("  , @DEPSTATION")            '発駅コード
        sqlPaymentStat.AppendLine("  , @DEPTRUSTEECD")          '発受託人コード
        sqlPaymentStat.AppendLine("  , @DEPTRUSTEESUBCD")       '発受託人サブ
        sqlPaymentStat.AppendLine("  , @JOTARRBRANCHCD")        'JOT着組織コード
        sqlPaymentStat.AppendLine("  , @ARRSTATION")            '着駅コード
        sqlPaymentStat.AppendLine("  , @ARRTRUSTEECD")          '着受託人コード
        sqlPaymentStat.AppendLine("  , @ARRTRUSTEESUBCD")       '着受託人サブ
        sqlPaymentStat.AppendLine("  , @JRITEMCD")              'JR品目コード
        sqlPaymentStat.AppendLine("  , @PAYMENTTOTAL")          '支払額
        sqlPaymentStat.AppendLine("  , @PAYMENTCTAX")           '支払額消費税
        sqlPaymentStat.AppendLine("  , @PAYMENTFARE")           '適用運賃
        sqlPaymentStat.AppendLine("  , @PAYMENTOTHERFEE")       'その他料金
        sqlPaymentStat.AppendLine("  , @SHIPFEE")               '発送料
        sqlPaymentStat.AppendLine("  , @TAXKBN")                '税区分
        sqlPaymentStat.AppendLine("  , @TAXRATE")               '税率
        sqlPaymentStat.AppendLine("  , @PAYADDSUB")             '加減額
        sqlPaymentStat.AppendLine("  , @REMARKS")               '備考
        sqlPaymentStat.AppendLine("  , @INOUTSIDEKBN")          '内外区分
        sqlPaymentStat.AppendLine("  , @DELFLG")                '削除フラグ
        sqlPaymentStat.AppendLine("  , @INITYMD")               '登録年月日
        sqlPaymentStat.AppendLine("  , @INITUSER")              '登録ユーザーID
        sqlPaymentStat.AppendLine("  , @INITTERMID")            '登録端末
        sqlPaymentStat.AppendLine("  , @INITPGID")              '登録プログラムID
        sqlPaymentStat.AppendLine("  , @RECEIVEYMD")            '集信日時

        sqlPaymentStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = htRentData(PAYMENTDATA_PARM.PM_PAYMENTYM)                                 '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER))          '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TORICODE))                    '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE))              '支払書種類
                .Add("SHIPYMD", MySqlDbType.Date).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_SHIPYMD))                          '発送年月日
                .Add("SAMEDAYCNT", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT))                '同日内回数
                .Add("CTNLINENO", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_CTNLINENO))                  '行番
                .Add("KEIJYOBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD))        '計上支店コード
                .Add("AMOUNTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE))                '金額種別
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_CTNTYPE))                      'コンテナ記号
                .Add("CTNNO", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_CTNNO))                          'コンテナ番号
                .Add("SEQNO", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_SEQNO))                               'SEQNO
                .Add("JOTDEPBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_JOTDEPBRANCHCD))        'JOT発組織コード
                .Add("DEPSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_DEPSTATION))                     '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_DEPTRUSTEECD))                 '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_DEPTRUSTEESUBCD))           '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_JOTARRBRANCHCD))        'JOT着組織コード
                .Add("ARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_ARRSTATION))                '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_ARRTRUSTEECD))            '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_ARRTRUSTEESUBCD))      '着受託人サブ
                .Add("JRITEMCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_JRITEMCD))                    'JR品目コード
                .Add("PAYMENTTOTAL", MySqlDbType.Decimal).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTTOTAL))               '支払額
                .Add("PAYMENTCTAX", MySqlDbType.Decimal).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTCTAX))                 '支払額消費税
                .Add("PAYMENTFARE", MySqlDbType.Decimal).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTFARE))                 '適用運賃
                .Add("PAYMENTOTHERFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTOTHERFEE))         'その他料金
                .Add("SHIPFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_SHIPFEE))                         '発送料
                .Add("TAXKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TAXKBN))                        '税区分
                .Add("TAXRATE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TAXRATE))                      '税率
                .Add("PAYADDSUB", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYADDSUB))                  '加減額
                .Add("REMARKS", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_REMARKS))                      '備考
                .Add("INOUTSIDEKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_INOUTSIDEKBN))            '内外区分
                .Add("DELFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_DELFLG))                        '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_INITYMD))                      '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_INITUSER))                    '登録ユーザーID
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_INITTERMID))                '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_INITPGID))                    '登録プログラムID
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_RECEIVEYMD))                '集信日時

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払明細回送費データTBL検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htPaymentData">支払明細ーデータ</param>
    Public Shared Function SelectInvoiceMeisai(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htPaymentData As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    PAYMENTYM")                                             '支払年月
        SQLBldr.AppendLine("    , PAYMENTNUMBER")                                       '支払番号
        SQLBldr.AppendLine("    , PAYMENTORGCODE")                                      '支払支店コード
        SQLBldr.AppendLine("    , TORICODE")                                            '支払取引先コード
        SQLBldr.AppendLine("    , PAYMENTTYPE")                                         '支払書種類
        SQLBldr.AppendLine("    , SHIPYMD")                                             '発送年月日
        SQLBldr.AppendLine("    , SAMEDAYCNT")                                          '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO")                                           '行番
        SQLBldr.AppendLine("    , KEIJYOBRANCHCD")                                      '計上支店コード
        SQLBldr.AppendLine("    , AMOUNTTYPE")                                          '金額種別
        SQLBldr.AppendLine("    , CTNTYPE")                                             'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO")                                               'コンテナ番号
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD")                                      'JOT発組織コード
        SQLBldr.AppendLine("    , DEPSTATION")                                          '発駅コード
        SQLBldr.AppendLine("    , DEPTRUSTEECD")                                        '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEESUBCD")                                     '発受託人サブ
        SQLBldr.AppendLine("    , JOTARRBRANCHCD")                                      'JOT着組織コード
        SQLBldr.AppendLine("    , ARRSTATION")                                          '着駅コード
        SQLBldr.AppendLine("    , ARRTRUSTEECD")                                        '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEESUBCD")                                     '着受託人サブ
        SQLBldr.AppendLine("    , JRITEMCD")                                            'JR品目コード
        SQLBldr.AppendLine("    , PAYMENTTOTAL")                                        '支払額
        SQLBldr.AppendLine("    , PAYMENTCTAX")                                         '支払額消費税
        SQLBldr.AppendLine("    , PAYMENTFARE")                                         '適用運賃
        SQLBldr.AppendLine("    , PAYMENTOTHERFEE")                                     'その他料金
        SQLBldr.AppendLine("    , PAYMENTSHIPPINGFEE")                                  '発送料
        SQLBldr.AppendLine("    , TAXKBN")                                              '税区分
        SQLBldr.AppendLine("    , TAXRATE")                                             '税率
        SQLBldr.AppendLine("    , PAYADDSUB")                                           '加減額
        SQLBldr.AppendLine("    , REMARKS")                                             '備考
        SQLBldr.AppendLine("    , DELFLG")                                              '削除フラグ
        SQLBldr.AppendLine("    , INITYMD")                                             '登録年月日
        SQLBldr.AppendLine("    , INITUSER")                                            '登録ユーザーID
        SQLBldr.AppendLine("    , INITTERMID")                                          '登録端末
        SQLBldr.AppendLine("    , INITPGID")                                            '登録プログラムID
        SQLBldr.AppendLine("    , UPDYMD")                                              '更新年月日
        SQLBldr.AppendLine("    , UPDUSER")                                             '更新ユーザーID
        SQLBldr.AppendLine("    , UPDTERMID")                                           '更新端末
        SQLBldr.AppendLine("    , RECEIVEYMD")                                          '集信日時
        SQLBldr.AppendLine("    , UPDTIMSTP")                                           'タイムスタンプ
        SQLBldr.AppendLine("FROM")
        'メイン 請求ヘッダーデータ
        SQLBldr.AppendLine("    lng.LNT0078_PAYMENTDATA")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        '支払年月
        SQLBldr.AppendLine("    PAYMENTYM = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTYM).ToString & "'")
        '支払番号
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTNUMBER = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER).ToString & "'")
        End If
        '支払支店店コード
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTORGCODE = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE).ToString & "'")
        End If
        '支払取引先コード
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_TORICODE).ToString) Then
            SQLBldr.AppendLine("    AND TORICODE = '" & htPaymentData(PAYMENTDATA_PARM.PM_TORICODE).ToString & "'")
        End If
        '発送年月日
        'If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_SHIPYMD).ToString) Then
        '    SQLBldr.AppendLine("    AND SHIPYMD = '" & htPaymentData(PAYMENTDATA_PARM.PM_SHIPYMD).ToString & "'")
        'End If
        '同日内回数
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT).ToString) Then
            SQLBldr.AppendLine("    AND SAMEDAYCNT = '" & htPaymentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT).ToString & "'")
        End If
        '行番
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_CTNLINENO).ToString) Then
            SQLBldr.AppendLine("    AND CTNLINENO = '" & htPaymentData(PAYMENTDATA_PARM.PM_CTNLINENO).ToString & "'")
        End If
        '計上支店店コード
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD).ToString) Then
            SQLBldr.AppendLine("    AND KEIJYOBRANCHCD = '" & htPaymentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD).ToString & "'")
        End If
        '金額種別
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE).ToString) Then
            SQLBldr.AppendLine("    AND AMOUNTTYPE = '" & htPaymentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE).ToString & "'")
        End If
        '請求書種類
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTTYPE = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE).ToString & "'")
        End If
        '削除フラグ
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_DELFLG).ToString) Then
            SQLBldr.AppendLine("    AND DELFLG = '" & htPaymentData(PAYMENTDATA_PARM.PM_DELFLG).ToString & "'")
        End If

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    PAYMENTYM")
        SQLBldr.AppendLine("    , PAYMENTNUMBER")
        SQLBldr.AppendLine("    , PAYMENTORGCODE")
        SQLBldr.AppendLine("    , TORICODE")
        SQLBldr.AppendLine("    , PAYMENTTYPE")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

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
    ''' 支払明細回送費データTBL検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htPaymentData">支払明細ーデータ</param>
    Public Shared Function SelectPaymentMeisai(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htPaymentData As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    PAYMENTYM")                                             '支払年月
        SQLBldr.AppendLine("    , PAYMENTNUMBER")                                       '支払番号
        SQLBldr.AppendLine("    , PAYMENTORGCODE")                                      '支払支店コード
        SQLBldr.AppendLine("    , TORICODE")                                            '支払取引先コード
        SQLBldr.AppendLine("    , PAYMENTTYPE")                                         '支払書種類
        SQLBldr.AppendLine("    , SHIPYMD")                                             '発送年月日
        SQLBldr.AppendLine("    , SAMEDAYCNT")                                          '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO")                                           '行番
        SQLBldr.AppendLine("    , KEIJYOBRANCHCD")                                      '計上支店コード
        SQLBldr.AppendLine("	, LNS0019.NAME AS KEIJYOBRANCHNM")                      '計上支店名称
        SQLBldr.AppendLine("    , AMOUNTTYPE")                                          '金額種別
        SQLBldr.AppendLine("    , CTNTYPE")                                             'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO")                                               'コンテナ番号
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD")                                      'JOT発組織コード
        SQLBldr.AppendLine("    , DEPSTATION")                                          '発駅コード
        SQLBldr.AppendLine("    , DEPTRUSTEECD")                                        '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEESUBCD")                                     '発受託人サブ
        SQLBldr.AppendLine("    , JOTARRBRANCHCD")                                      'JOT着組織コード
        SQLBldr.AppendLine("    , ARRSTATION")                                          '着駅コード
        SQLBldr.AppendLine("    , ARRTRUSTEECD")                                        '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEESUBCD")                                     '着受託人サブ
        SQLBldr.AppendLine("    , JRITEMCD")                                            'JR品目コード
        SQLBldr.AppendLine("    , PAYMENTTOTAL")                                        '支払額
        SQLBldr.AppendLine("    , PAYMENTCTAX")                                         '支払額消費税
        SQLBldr.AppendLine("    , PAYMENTFARE")                                         '適用運賃
        SQLBldr.AppendLine("    , PAYMENTOTHERFEE")                                     'その他料金
        SQLBldr.AppendLine("    , PAYMENTSHIPPINGFEE")                                  '発送料
        SQLBldr.AppendLine("    , TAXKBN")                                              '税区分
        SQLBldr.AppendLine("    , TAXRATE")                                             '税率
        SQLBldr.AppendLine("    , PAYADDSUB")                                           '加減額
        SQLBldr.AppendLine("    , REMARKS")                                             '備考
        SQLBldr.AppendLine("    , INOUTSIDEKBN")                                        '内外区分
        SQLBldr.AppendLine("    , LNT0078.DELFLG")                                      '削除フラグ
        SQLBldr.AppendLine("    , LNT0078.INITYMD")                                     '登録年月日
        SQLBldr.AppendLine("    , LNT0078.INITUSER")                                    '登録ユーザーID
        SQLBldr.AppendLine("    , LNT0078.INITTERMID")                                  '登録端末
        SQLBldr.AppendLine("    , LNT0078.INITPGID")                                    '登録プログラムID
        SQLBldr.AppendLine("    , LNT0078.UPDYMD")                                      '更新年月日
        SQLBldr.AppendLine("    , LNT0078.UPDUSER")                                     '更新ユーザーID
        SQLBldr.AppendLine("    , LNT0078.UPDTERMID")                                   '更新端末
        SQLBldr.AppendLine("    , LNT0078.RECEIVEYMD")                                  '集信日時
        SQLBldr.AppendLine("    , LNT0078.UPDTIMSTP")                                   'タイムスタンプ
        SQLBldr.AppendLine("FROM")
        'メイン 請求ヘッダーデータ
        SQLBldr.AppendLine("    lng.LNT0078_PAYMENTDATA LNT0078")
        SQLBldr.AppendLine("LEFT JOIN LNG.LNM0002_ORG LNS0019")
        SQLBldr.AppendLine("    ON LNT0078.KEIJYOBRANCHCD = LNS0019.ORGCODE")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        '支払年月
        SQLBldr.AppendLine("    PAYMENTYM = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTYM).ToString & "'")
        '支払番号
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTNUMBER = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER).ToString & "'")
        End If
        '支払支店店コード
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTORGCODE = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE).ToString & "'")
        End If
        '支払取引先コード
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_TORICODE).ToString) Then
            SQLBldr.AppendLine("    AND TORICODE = '" & htPaymentData(PAYMENTDATA_PARM.PM_TORICODE).ToString & "'")
        End If
        '発送年月日
        'If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_SHIPYMD).ToString) Then
        '    SQLBldr.AppendLine("    AND SHIPYMD = '" & htPaymentData(PAYMENTDATA_PARM.PM_SHIPYMD).ToString & "'")
        'End If
        '同日内回数
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT).ToString) Then
            SQLBldr.AppendLine("    AND SAMEDAYCNT = '" & htPaymentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT).ToString & "'")
        End If
        '行番
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_CTNLINENO).ToString) Then
            SQLBldr.AppendLine("    AND CTNLINENO = '" & htPaymentData(PAYMENTDATA_PARM.PM_CTNLINENO).ToString & "'")
        End If
        '金額種別
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE).ToString) Then
            SQLBldr.AppendLine("    AND AMOUNTTYPE = '" & htPaymentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE).ToString & "'")
        End If
        '請求書種類
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTTYPE = '" & htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE).ToString & "'")
        End If
        '削除フラグ
        If Not String.IsNullOrEmpty(htPaymentData(PAYMENTDATA_PARM.PM_DELFLG).ToString) Then
            SQLBldr.AppendLine("    AND LNT0078.DELFLG = '" & htPaymentData(PAYMENTDATA_PARM.PM_DELFLG).ToString & "'")
        End If

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    PAYMENTYM")
        SQLBldr.AppendLine("    , KEIJYOBRANCHCD")
        SQLBldr.AppendLine("    , PAYMENTNUMBER")
        SQLBldr.AppendLine("    , PAYMENTORGCODE")
        SQLBldr.AppendLine("    , TORICODE")
        SQLBldr.AppendLine("    , PAYMENTTYPE")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

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
    ''' 支払明細データ　更新処理（金額更新用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htPaymentData">請求ヘッダーデータ</param>
    Public Shared Sub UpdatePayMeisai(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htPaymentData As Hashtable)

        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine("UPDATE LNG.LNT0078_PAYMENTDATA")
        sqlPaymentStat.AppendLine("SET")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                   '支払年月
        sqlPaymentStat.AppendLine("  , PAYMENTNUMBER = @PAYMENTNUMBER")           '支払番号
        sqlPaymentStat.AppendLine("  , PAYMENTORGCODE = @PAYMENTORGCODE")         '支払支店コード
        sqlPaymentStat.AppendLine("  , TORICODE = @TORICODE")                     '支払取引先コード
        sqlPaymentStat.AppendLine("  , PAYMENTTYPE = @PAYMENTTYPE")               '支払書種類
        sqlPaymentStat.AppendLine("  , SHIPYMD = @SHIPYMD")                       '発送年月日
        sqlPaymentStat.AppendLine("  , SAMEDAYCNT = @SAMEDAYCNT")                 '同日内回数
        sqlPaymentStat.AppendLine("  , CTNLINENO = @CTNLINENO")                   '行番
        sqlPaymentStat.AppendLine("  , KEIJYOBRANCHCD = @KEIJYOBRANCHCD")         '計上支店コード
        sqlPaymentStat.AppendLine("  , AMOUNTTYPE = @AMOUNTTYPE")                 '金額種別
        sqlPaymentStat.AppendLine("  , CTNTYPE = @CTNTYPE")                       'コンテナ記号
        sqlPaymentStat.AppendLine("  , CTNNO = @CTNNO")                           'コンテナ番号
        sqlPaymentStat.AppendLine("  , JOTDEPBRANCHCD = @JOTDEPBRANCHCD")         'JOT発組織コード
        sqlPaymentStat.AppendLine("  , DEPSTATION = @DEPSTATION")                 '発駅コード
        sqlPaymentStat.AppendLine("  , DEPTRUSTEECD = @DEPTRUSTEECD")             '発受託人コード
        sqlPaymentStat.AppendLine("  , DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")       '発受託人サブ
        sqlPaymentStat.AppendLine("  , JOTARRBRANCHCD = @JOTARRBRANCHCD")         'JOT着組織コード
        sqlPaymentStat.AppendLine("  , ARRSTATION = @ARRSTATION")                 '着駅コード
        sqlPaymentStat.AppendLine("  , ARRTRUSTEECD = @ARRTRUSTEECD")             '着受託人コード
        sqlPaymentStat.AppendLine("  , ARRTRUSTEESUBCD = @ARRTRUSTEESUBCD")       '着受託人サブ
        sqlPaymentStat.AppendLine("  , JRITEMCD = @JRITEMCD")                     'JR品目コード
        sqlPaymentStat.AppendLine("  , PAYMENTTOTAL = @PAYMENTTOTAL")             '支払額
        sqlPaymentStat.AppendLine("  , PAYMENTCTAX = @PAYMENTCTAX")               '支払額消費税
        sqlPaymentStat.AppendLine("  , PAYMENTFARE = @PAYMENTFARE")               '適用運賃
        sqlPaymentStat.AppendLine("  , PAYMENTOTHERFEE = @PAYMENTOTHERFEE")       'その他料金
        sqlPaymentStat.AppendLine("  , PAYMENTSHIPPINGFEE = @PAYMENTSHIPPINGFEE") '発送料
        sqlPaymentStat.AppendLine("  , TAXKBN = @TAXKBN")                         '税区分
        sqlPaymentStat.AppendLine("  , TAXRATE = @TAXRATE")                       '税率
        sqlPaymentStat.AppendLine("  , PAYADDSUB = @PAYADDSUB")                   '加減額
        sqlPaymentStat.AppendLine("  , REMARKS = @REMARKS")                       '備考
        sqlPaymentStat.AppendLine("  , INOUTSIDEKBN = @INOUTSIDEKBN")             '内外区分
        sqlPaymentStat.AppendLine("  , DELFLG = @DELFLG")                         '削除フラグ
        sqlPaymentStat.AppendLine("  , INITYMD = @INITYMD")                       '登録年月日
        sqlPaymentStat.AppendLine("  , INITUSER = @INITUSER")                     '登録ユーザーID
        sqlPaymentStat.AppendLine("  , INITTERMID = @INITTERMID")                 '登録端末
        sqlPaymentStat.AppendLine("  , INITPGID = @INITPGID")                     '登録プログラムID
        sqlPaymentStat.AppendLine("  , RECEIVEYMD = @RECEIVEYMD")                 '集信日時
        sqlPaymentStat.AppendLine(" WHERE(")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                   '支払年月
        sqlPaymentStat.AppendLine("  AND PAYMENTNUMBER = @PAYMENTNUMBER")         '支払番号
        sqlPaymentStat.AppendLine("  AND PAYMENTORGCODE = @PAYMENTORGCODE")       '支払支店コード
        sqlPaymentStat.AppendLine("  AND TORICODE = @TORICODE")                   '支払取引先コード
        sqlPaymentStat.AppendLine("  AND PAYMENTTYPE = @PAYMENTTYPE")             '支払書種類
        sqlPaymentStat.AppendLine("  AND SHIPYMD = @SHIPYMD")                     '発送年月日
        sqlPaymentStat.AppendLine("  AND SAMEDAYCNT = @SAMEDAYCNT")               '同日内回数
        sqlPaymentStat.AppendLine("  AND CTNLINENO = @CTNLINENO")                 '行番
        sqlPaymentStat.AppendLine("  AND KEIJYOBRANCHCD = @KEIJYOBRANCHCD")       '計上支店コード
        sqlPaymentStat.AppendLine("  AND AMOUNTTYPE = @AMOUNTTYPE")               '金額種別

        sqlPaymentStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTYM)                                 '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER))          '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_TORICODE))                    '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTYPE))              '支払書種類
                .Add("SHIPYMD", MySqlDbType.Date).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_SHIPYMD))                          '発送年月日
                .Add("SAMEDAYCNT", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_SAMEDAYCNT))                '同日内回数
                .Add("CTNLINENO", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_CTNLINENO))                  '行番
                .Add("KEIJYOBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD))        '計上支店コード
                .Add("AMOUNTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE))                '金額種別
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_CTNTYPE))                      'コンテナ記号
                .Add("CTNNO", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_CTNNO))                          'コンテナ番号
                .Add("JOTDEPBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_JOTDEPBRANCHCD))        'JOT発組織コード
                .Add("DEPSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_DEPSTATION))                '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_DEPTRUSTEECD))            '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_DEPTRUSTEESUBCD))      '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_JOTARRBRANCHCD))        'JOT着組織コード
                .Add("ARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_ARRSTATION))                '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_ARRTRUSTEECD))            '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_ARRTRUSTEESUBCD))      '着受託人サブ
                .Add("JRITEMCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_JRITEMCD))                    'JR品目コード
                .Add("PAYMENTTOTAL", MySqlDbType.Decimal).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTTOTAL))               '支払額
                .Add("PAYMENTCTAX", MySqlDbType.Decimal).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTCTAX))                 '支払額消費税
                .Add("PAYMENTFARE", MySqlDbType.Decimal).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTFARE))                 '適用運賃
                .Add("PAYMENTOTHERFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYMENTOTHERFEE))         'その他料金
                .Add("PAYMENTSHIPPINGFEE", MySqlDbType.Decimal).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_SHIPFEE))              '発送料
                .Add("TAXKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_TAXKBN))                        '税区分
                .Add("TAXRATE", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_TAXRATE))                      '税率
                .Add("PAYADDSUB", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_PAYADDSUB))                  '加減額
                .Add("REMARKS", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_REMARKS))                      '備考
                .Add("INOUTSIDEKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_INOUTSIDEKBN))            '内外区分
                .Add("DELFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_DELFLG))                        '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_INITYMD))                      '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_INITUSER))                    '登録ユーザーID
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_INITTERMID))                '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_INITPGID))                    '登録プログラムID
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htPaymentData(PAYMENTDATA_PARM.PM_RECEIVEYMD))                '集信日時

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払回送費ヘッダーTBL削除処理（0件用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求ヘッダーデータ</param>
    Public Shared Sub DeletePaymentHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine(" DELETE FROM LNG.LNT0077_PAYMENTHEAD")
        sqlPaymentStat.AppendLine("    WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                    '支払年月
        'sqlPaymentStat.AppendLine("  AND PAYMENTNUMBER = @PAYMENTNUMBER")        　'支払番号
        sqlPaymentStat.AppendLine("  AND PAYMENTORGCODE = @PAYMENTORGCODE")        '支払支店コード
        sqlPaymentStat.AppendLine("  AND TORICODE = @TORICODE")                    '支払取引先コード
        sqlPaymentStat.AppendLine("  AND UPDATEFLG = @UPDATEFLG")                  '変更有フラグ

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = htRentData(PAYHEAD_PARM.PM_PAYMENTYM)                                     '支払年月
                '.Add("PAYMENTNUMBER", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYHEAD_PARM.PM_PAYMENTNUMBER))              '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYHEAD_PARM.PM_PAYMENTORGCODE))       '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYHEAD_PARM.PM_TORICODE))                   '支払取引先コード
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYHEAD_PARM.PM_UPDFLG))                    '変更有フラグ

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払明細回送費データTBL削除処理（0件用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求ヘッダーデータ</param>
    Public Shared Sub DeletePaymentMeiaiData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine(" DELETE FROM LNG.LNT0078_PAYMENTDATA")
        sqlPaymentStat.AppendLine("    WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                    '支払年月
        sqlPaymentStat.AppendLine("  AND KEIJYOBRANCHCD = @KEIJYOBRANCHCD")        '計上支店コード
        sqlPaymentStat.AppendLine("  AND TORICODE = @TORICODE")                    '支払取引先コード

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = htRentData(PAYMENTDATA_PARM.PM_PAYMENTYM)                                      '支払年月
                .Add("KEIJYOBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD))        '計上支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TORICODE))                    '支払取引先コード

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払明細回送費データTBL削除処理（加減額用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求ヘッダーデータ</param>
    Public Shared Sub DeletePaymentData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine(" DELETE FROM LNG.LNT0078_PAYMENTDATA")
        sqlPaymentStat.AppendLine("    WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                    '支払年月
        sqlPaymentStat.AppendLine("  AND KEIJYOBRANCHCD = @KEIJYOBRANCHCD")        '計上支店コード
        sqlPaymentStat.AppendLine("  AND TORICODE = @TORICODE")                    '支払取引先コード
        sqlPaymentStat.AppendLine("  AND AMOUNTTYPE = @AMOUNTTYPE")                '金額種別

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = htRentData(PAYMENTDATA_PARM.PM_PAYMENTYM)                                      '支払年月
                .Add("KEIJYOBRANCHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_KEIJYOBRANCHCD))        '計上支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TORICODE))                    '支払取引先コード
                .Add("AMOUNTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE))                '金額種別

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払明細回送費データTBL削除処理（取下→申請用）
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求ヘッダーデータ</param>
    Public Shared Sub DeletePaymentListData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable)

        '◯受注TBL
        Dim sqlPaymentStat As New StringBuilder
        sqlPaymentStat.AppendLine(" DELETE FROM LNG.LNT0078_PAYMENTDATA")
        sqlPaymentStat.AppendLine("    WHERE")
        sqlPaymentStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                    '支払年月
        sqlPaymentStat.AppendLine("  AND PAYMENTNUMBER = @PAYMENTNUMBER")          '支払番号
        sqlPaymentStat.AppendLine("  AND PAYMENTORGCODE = @PAYMENTORGCODE")        '支払支店コード
        sqlPaymentStat.AppendLine("  AND TORICODE = @TORICODE")                    '支払取引先コード
        sqlPaymentStat.AppendLine("  AND AMOUNTTYPE = @AMOUNTTYPE")                '金額種別

        Using sqlOrderCmd As New MySqlCommand(sqlPaymentStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = htRentData(PAYMENTDATA_PARM.PM_PAYMENTYM)                                      '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.Int32).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTNUMBER))            '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_TORICODE))                    '支払取引先コード
                .Add("AMOUNTTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htRentData(PAYMENTDATA_PARM.PM_AMOUNTTYPE))                '金額種別

            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' コンテナ清算ファイルTBL 検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectRessnf(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    SHIPYMD")                               '発送年月日
        SQLBldr.AppendLine("	, CTNTYPE")                             'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO")                               'コンテナ番号
        SQLBldr.AppendLine("    , SAMEDAYCNT")                          '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO")                           '行番
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD")                      'JOT発組織コード
        SQLBldr.AppendLine("    , DEPSTATION")                          '発駅コード
        SQLBldr.AppendLine("    , DEPTRUSTEECD")                        '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEESUBCD")                     '発受託人サブ
        SQLBldr.AppendLine("    , JOTARRBRANCHCD")                      'JOT着組織コード
        SQLBldr.AppendLine("    , ARRSTATION")                          '着駅コード
        SQLBldr.AppendLine("    , ARRTRUSTEECD")                        '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEESUBCD")                     '着受託人サブ
        SQLBldr.AppendLine("	, JRITEMCD")                            'JR品目コード
        SQLBldr.AppendLine("    , USEFEE")                              '支払額
        SQLBldr.AppendLine("    , OWNDISCOUNTFEE")                      '支払額消費税
        SQLBldr.AppendLine("    , FREESENDFEE")                         '適用運賃
        SQLBldr.AppendLine("    , OTHER1FEE + OTHER2FEE AS OTHERFEE")   'その他料金
        SQLBldr.AppendLine("    , SHIPFEE")                             '発送料
        SQLBldr.AppendLine("    , TAXKBN")                              '税区分
        SQLBldr.AppendLine("    , TAXRATE")                             '税率
        SQLBldr.AppendLine("	, PAYKEIJYOBRANCHCD")                   '計上支店コード
        SQLBldr.AppendLine("FROM")
        'メイン コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    DELFLG = @P01")
        SQLBldr.AppendLine("    AND KEIJOYM = '" & htParm(RESSNF_KEY.SL_KEIJOYM).ToString & "'")
        SQLBldr.AppendLine("    AND TORICODE = '" & htParm(RESSNF_KEY.SL_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND PAYFILINGBRANCH = '" & htParm(RESSNF_KEY.SL_PAYFILINGBRANCH).ToString & "'")
        SQLBldr.AppendLine("    AND SCHEDATEPAYMENT = '" & htParm(RESSNF_KEY.SL_DEPOSITYMD).ToString & "'")
        'SQLBldr.AppendLine("    AND DEPOSITMONTHKBN = '" & htParm(RESSNF_KEY.SL_DEPOSITMONTHKBN).ToString & "'")
        SQLBldr.AppendLine("    AND FORMAT(CLOSINGDATE, 'dd') = '" & htParm(RESSNF_KEY.SL_CLOSINGDAY).ToString & "'")
        SQLBldr.AppendLine("    AND STACKFREEKBN = '" & htParm(RESSNF_KEY.SL_STACKFREEKBN).ToString & "'")
        SQLBldr.AppendLine("    AND ACCOUNTINGASSETSKBN = '" & htParm(RESSNF_KEY.SL_ACCOUNTINGASSETSKBN).ToString & "'")
        SQLBldr.AppendLine("    AND ACCOUNTINGMONTH = '" & htParm(RESSNF_KEY.SL_ACCOUNTINGMONTH).ToString & "'")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ

            PARA01.Value = C_DELETE_FLG.ALIVE

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

#Region "支払書連携関連"
    ''' <summary>
    ''' コンテナ清算ファイル 検索処理(請求連携用(お支払書))
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectPaymentCsv(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    主キー")
        SQLBldr.AppendLine("    ,支払書発行年月日")
        SQLBldr.AppendLine("    ,費用計上日付")
        SQLBldr.AppendLine("    ,支払予定年月日")
        SQLBldr.AppendLine("    ,顧客コード")
        SQLBldr.AppendLine("    ,支払先顧客選択")
        SQLBldr.AppendLine("    ,顧客名")
        SQLBldr.AppendLine("    ,提出部店")
        SQLBldr.AppendLine("    ,提出部店名")
        SQLBldr.AppendLine("    ,計上部店")
        SQLBldr.AppendLine("    ,計上部店名")
        SQLBldr.AppendLine("    ,帳票種別")
        SQLBldr.AppendLine("    ,発駅コード")
        SQLBldr.AppendLine("    ,発駅名")
        SQLBldr.AppendLine("    ,着駅コード")
        SQLBldr.AppendLine("    ,着駅名")
        SQLBldr.AppendLine("    ,大分類コード")
        SQLBldr.AppendLine("    ,大分類名")
        SQLBldr.AppendLine("    ,中分類コード")
        SQLBldr.AppendLine("    ,回送個数")
        SQLBldr.AppendLine("    ,[科目（回送運賃）]")
        SQLBldr.AppendLine("    ,[細目（回送運賃）]")
        SQLBldr.AppendLine("    ,[金額（回送運賃）]")
        SQLBldr.AppendLine("    ,[科目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[細目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[金額（修理時運賃）]")
        SQLBldr.AppendLine("    ,[科目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（除却時運賃）]")
        SQLBldr.AppendLine("    ,[科目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（売却時運賃）]")
        SQLBldr.AppendLine("    ,青函付加金")
        SQLBldr.AppendLine("    ,[科目（発送料）]")
        SQLBldr.AppendLine("    ,[細目（発送料）]")
        SQLBldr.AppendLine("    ,[金額（発送料）]")
        SQLBldr.AppendLine("    ,[科目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[細目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[金額（修理時発送料）]")
        SQLBldr.AppendLine("    ,[科目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（除却時発送料）]")
        SQLBldr.AppendLine("    ,[科目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（売却時発送料）]")
        SQLBldr.AppendLine("    ,発行担当者名")
        SQLBldr.AppendLine("    ,宛名欄付記１")
        SQLBldr.AppendLine("FROM (")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '1' AS SORTNO")
        SQLBldr.AppendLine("    ,'01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-20' AS 主キー")              '主キー
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")                                                                 '支払書発行年月日
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")                                                              '費用計上日付
        SQLBldr.AppendLine("    , CASE WHEN A11.SCHEDATEPAYMENT IS NULL THEN A01.SCHEDATEPAYMENT")
        SQLBldr.AppendLine("           ELSE A11.SCHEDATEPAYMENT END AS 支払予定年月日")                                                                 '支払予定年月日
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")                                                                                      '支払先顧客選択
        SQLBldr.AppendLine("    , A10.CLIENTNAME AS 顧客名")                                                                                            '顧客名
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")                                                                             '提出部店
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")                                                                                              '提出部店名
        SQLBldr.AppendLine("    , '01-' + A01.PAYKEIJYOBRANCHCD AS 計上部店")                                                                           '計上部店
        SQLBldr.AppendLine("    , A08.NAME AS 計上部店名")                                                                                              '計上部店名
        SQLBldr.AppendLine("	, '20' AS 帳票種別")                                                                                                    '帳票種別
        SQLBldr.AppendLine("    , A01.DEPSTATION AS 発駅コード")                                                                                        '発駅コード
        SQLBldr.AppendLine("    , A04.NAMES AS 発駅名")                                                                                                 '発駅名
        SQLBldr.AppendLine("    , A01.ARRSTATION AS 着駅コード")                                                                                        '着駅コード
        SQLBldr.AppendLine("    , A05.NAMES AS 着駅名")                                                                                                 '着駅名
        SQLBldr.AppendLine("    , A01.BIGCTNCD AS 大分類コード")                                                                                        '大分類コード
        SQLBldr.AppendLine("    , CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        WHEN '20' THEN '無蓋20'")
        SQLBldr.AppendLine("        ELSE A06.KANJI1")
        SQLBldr.AppendLine("      END AS 大分類名")                                                                                                     '大分類名
        SQLBldr.AppendLine("    , A01.MIDDLECTNCD AS 中分類コード")                                                                                     '中分類コード
        SQLBldr.AppendLine("    , A02.SUM_QUANTITY AS 回送個数")                                                                                        '回送個数
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                 CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                     WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                     WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                     WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                     WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                     WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                     WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                     WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                     WHEN '35' THEN")
        SQLBldr.AppendLine("                         CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                             WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                             ELSE 'J-30206'")
        SQLBldr.AppendLine("                         END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , A02.SUM_OTHER1FEE AS 青函付加金")                                                                                     '青函付加金
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A09.STAFFNAMES AS 発行担当者名")                                                                                      '発行担当者名
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")                                                                 '宛名欄付記１
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , coalesce(A1.BIGCTNCD,'') AS BIGCTNCD")
        SQLBldr.AppendLine("            , coalesce(A1.MIDDLECTNCD,'') AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2 AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("            , SUM(A1.OTHER1FEE) AS SUM_OTHER1FEE")
        SQLBldr.AppendLine("            , SUM(A1.QUANTITY) AS SUM_QUANTITY")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	            AND DUMMYKBN = 0")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("            AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("	        AND A1.DUMMYKBN = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , A1.BIGCTNCD")
        SQLBldr.AppendLine("            , A1.MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        SQLBldr.AppendLine("		AND (A02.SUM_FREESENDFEE <> 0")
        SQLBldr.AppendLine("		     OR A02.SUM_SHIPFEE <> 0")
        SQLBldr.AppendLine("		     OR A02.SUM_OTHER1FEE <> 0)")
        '[結合テーブル]駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A04")
        SQLBldr.AppendLine("        ON A04.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A04.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        '[結合テーブル]駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A05.STATION = A01.ARRSTATION ")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '[結合テーブル]大中小分類マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0022_CLASS A06")
        SQLBldr.AppendLine("        ON A06.BIGCTNCD = A01.BIGCTNCD")
        SQLBldr.AppendLine("        AND A06.MIDDLECTNCD = A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        AND A06.SMALLCTNCD = A01.SMALLCTNCD")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A07")
        SQLBldr.AppendLine("        ON A07.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A07.ORGCODE = A01.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND A07.DELFLG = @P01")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A08")
        SQLBldr.AppendLine("        ON A08.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A08.ORGCODE = A01.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND A08.DELFLG = @P01")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A09")
        SQLBldr.AppendLine("        ON A09.USERID = '" & htParm(PAYMENTLINK_KEY.PM_USERID).ToString & "'")
        SQLBldr.AppendLine("        AND A09.DELFLG = @P01")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(PAYMENTLINK_KEY.PM_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(PAYMENTLINK_KEY.PM_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 0")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        SQLBldr.AppendLine("UNION ALL")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '2' AS SORTNO")
        SQLBldr.AppendLine("    ,'01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-20' AS 主キー")              '主キー
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")                                                                 '支払書発行年月日
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")                                                              '費用計上日付
        SQLBldr.AppendLine("    , CASE WHEN A11.SCHEDATEPAYMENT IS NULL THEN A01.SCHEDATEPAYMENT")
        SQLBldr.AppendLine("           ELSE A11.SCHEDATEPAYMENT END AS 支払予定年月日")                                                                 '支払予定年月日
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")                                                                                      '支払先顧客選択
        SQLBldr.AppendLine("    , A10.CLIENTNAME AS 顧客名")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")
        SQLBldr.AppendLine("    , '01-' + A01.PAYKEIJYOBRANCHCD AS 計上部店")
        SQLBldr.AppendLine("    , A08.NAME AS 計上部店名")
        SQLBldr.AppendLine("	, '20' AS 帳票種別")
        SQLBldr.AppendLine("    , NULL AS 発駅コード")
        SQLBldr.AppendLine("    , '加減額' AS 発駅名")
        SQLBldr.AppendLine("    , NULL AS 着駅コード")
        SQLBldr.AppendLine("    , NULL AS 着駅名")
        SQLBldr.AppendLine("    , NULL AS 大分類コード")
        SQLBldr.AppendLine("    , NULL AS 大分類名")
        SQLBldr.AppendLine("    , NULL AS 中分類コード")
        SQLBldr.AppendLine("    , NULL AS 回送個数")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                 CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                     WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                     WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                     WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                     WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                     WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                     WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                     WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                     WHEN '35' THEN")
        SQLBldr.AppendLine("                         CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                             WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                             ELSE 'J-30206'")
        SQLBldr.AppendLine("                         END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , 0 AS 青函付加金")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A09.STAFFNAMES AS 発行担当者名")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")                                                    '宛名欄付記１
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , coalesce(A1.BIGCTNCD,'') AS BIGCTNCD")
        SQLBldr.AppendLine("            , coalesce(A1.MIDDLECTNCD,'') AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2 AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("				AND DUMMYKBN = 1")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("			AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("			AND A1.DUMMYKBN = 1")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , A1.BIGCTNCD")
        SQLBldr.AppendLine("            , A1.MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        SQLBldr.AppendLine("		AND (A02.SUM_FREESENDFEE <> 0 OR A02.SUM_SHIPFEE <> 0)")
        '[結合テーブル]駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A04")
        SQLBldr.AppendLine("        ON A04.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A04.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        '[結合テーブル]駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A05.STATION = A01.ARRSTATION ")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '[結合テーブル]大中小分類マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0022_CLASS A06")
        SQLBldr.AppendLine("        ON A06.BIGCTNCD = A01.BIGCTNCD")
        SQLBldr.AppendLine("        AND A06.MIDDLECTNCD = A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        AND A06.SMALLCTNCD = A01.SMALLCTNCD")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A07")
        SQLBldr.AppendLine("        ON A07.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A07.ORGCODE = A01.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND A07.DELFLG = @P01")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A08")
        SQLBldr.AppendLine("        ON A08.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A08.ORGCODE = A01.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND A08.DELFLG = @P01")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A09")
        SQLBldr.AppendLine("        ON A09.USERID = '" & htParm(PAYMENTLINK_KEY.PM_USERID).ToString & "'")
        SQLBldr.AppendLine("        AND A09.DELFLG = @P01")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(PAYMENTLINK_KEY.PM_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(PAYMENTLINK_KEY.PM_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 1")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        '並び順
        SQLBldr.AppendLine(") D01")
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    SORTNO")
        SQLBldr.AppendLine("    , D01.発駅コード")
        SQLBldr.AppendLine("    , D01.着駅コード")
        SQLBldr.AppendLine("    , D01.支払予定年月日")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

            PARA01.Value = C_DELETE_FLG.ALIVE
            PARA02.Value = htParm(PAYMENTLINK_KEY.PM_CORPCODE).ToString

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
    ''' 支払ヘッダーデータ 検索処理(支払連携用(お支払書))
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectPaymentCsv2(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    主キー")
        SQLBldr.AppendLine("    ,支払書発行年月日")
        SQLBldr.AppendLine("    ,費用計上日付")
        SQLBldr.AppendLine("    ,支払予定年月日")
        SQLBldr.AppendLine("    ,顧客コード")
        SQLBldr.AppendLine("    ,支払先顧客選択")
        SQLBldr.AppendLine("    ,顧客名")
        SQLBldr.AppendLine("    ,提出部店")
        SQLBldr.AppendLine("    ,提出部店名")
        SQLBldr.AppendLine("    ,計上部店")
        SQLBldr.AppendLine("    ,計上部店名")
        SQLBldr.AppendLine("    ,帳票種別")
        SQLBldr.AppendLine("    ,発駅コード")
        SQLBldr.AppendLine("    ,発駅名")
        SQLBldr.AppendLine("    ,着駅コード")
        SQLBldr.AppendLine("    ,着駅名")
        SQLBldr.AppendLine("    ,大分類コード")
        SQLBldr.AppendLine("    ,大分類名")
        SQLBldr.AppendLine("    ,中分類コード")
        SQLBldr.AppendLine("    ,回送個数")
        SQLBldr.AppendLine("    ,[科目（回送運賃）]")
        SQLBldr.AppendLine("    ,[細目（回送運賃）]")
        SQLBldr.AppendLine("    ,[金額（回送運賃）]")
        SQLBldr.AppendLine("    ,[科目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[細目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[金額（修理時運賃）]")
        SQLBldr.AppendLine("    ,[科目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（除却時運賃）]")
        SQLBldr.AppendLine("    ,[科目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（売却時運賃）]")
        SQLBldr.AppendLine("    ,青函付加金")
        SQLBldr.AppendLine("    ,[科目（発送料）]")
        SQLBldr.AppendLine("    ,[細目（発送料）]")
        SQLBldr.AppendLine("    ,[金額（発送料）]")
        SQLBldr.AppendLine("    ,[科目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[細目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[金額（修理時発送料）]")
        SQLBldr.AppendLine("    ,[科目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（除却時発送料）]")
        SQLBldr.AppendLine("    ,[科目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（売却時発送料）]")
        SQLBldr.AppendLine("    ,発行担当者名")
        SQLBldr.AppendLine("    ,宛名欄付記１")
        SQLBldr.AppendLine("    ,SEQNO")
        SQLBldr.AppendLine("FROM (")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-20' AS 主キー")
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")
        SQLBldr.AppendLine("    , CASE WHEN A12.SCHEDATEPAYMENT IS NULL THEN A01.SCHEDATEPAYMENT")
        SQLBldr.AppendLine("		   ELSE A12.SCHEDATEPAYMENT END AS 支払予定年月日")
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")
        SQLBldr.AppendLine("    , A11.CLIENTNAME AS 顧客名")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")
        SQLBldr.AppendLine("    , '01-' + A09.KEIJYOBRANCHCD AS 計上部店")
        SQLBldr.AppendLine("    , A10.NAME AS 計上部店名")
        SQLBldr.AppendLine("	, '20' AS 帳票種別")
        SQLBldr.AppendLine("    , NULL AS 発駅コード")
        SQLBldr.AppendLine("    , '加減額' AS 発駅名")
        SQLBldr.AppendLine("    , NULL AS 着駅コード")
        SQLBldr.AppendLine("    , A09.REMARKS AS 着駅名")
        SQLBldr.AppendLine("    , NULL AS 大分類コード")
        SQLBldr.AppendLine("    , NULL AS 大分類名")
        SQLBldr.AppendLine("    , NULL AS 中分類コード")
        SQLBldr.AppendLine("    , NULL AS 回送個数")
        SQLBldr.AppendLine("    , 'J-51040101' AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE WHEN (A09.CTNTYPE = 'KAGEN' AND A09.CTNNO = 0) THEN")
        SQLBldr.AppendLine("	      CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("              WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("              WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("              WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("              WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("              WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("              WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("              WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("              WHEN '35' THEN")
        SQLBldr.AppendLine("                  CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                      WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                      ELSE 'J-30206'")
        SQLBldr.AppendLine("                  END")
        SQLBldr.AppendLine("          END")
        SQLBldr.AppendLine("	  ELSE")
        SQLBldr.AppendLine("	      CASE A09.BIGCTNCD")
        SQLBldr.AppendLine("              WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("              WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("              WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("              WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("              WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("              WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("              WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("              WHEN '35' THEN")
        SQLBldr.AppendLine("                  CASE A09.MIDDLECTNCD")
        SQLBldr.AppendLine("                      WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                      ELSE 'J-30206'")
        SQLBldr.AppendLine("                  END")
        SQLBldr.AppendLine("          END")
        SQLBldr.AppendLine("	  END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , A09.PAYADDSUB AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , 0 AS 青函付加金")
        SQLBldr.AppendLine("    , NULL AS '科目（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A08.STAFFNAMES AS 発行担当者名")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")
        SQLBldr.AppendLine("    , SEQNO")
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , MIN(coalesce(A1.BIGCTNCD,'')) AS BIGCTNCD")
        SQLBldr.AppendLine("            , MIN(coalesce(A1.MIDDLECTNCD,'')) AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , MIN(A1.STACKFREEKBN) AS STACKFREEKBN")
        SQLBldr.AppendLine("            , MIN(B1.ACCOUNTSTATUSKBN2) AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("            , SUM(A1.OTHER1FEE) AS SUM_OTHER1FEE")
        SQLBldr.AppendLine("            , SUM(A1.QUANTITY) AS SUM_QUANTITY")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("	        AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        '[結合テーブル]駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A04")
        SQLBldr.AppendLine("        ON A04.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A04.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        '[結合テーブル]駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A05.STATION = A01.ARRSTATION ")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '[結合テーブル]大中小分類マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0022_CLASS A06")
        SQLBldr.AppendLine("        ON A06.BIGCTNCD = A01.BIGCTNCD")
        SQLBldr.AppendLine("        AND A06.MIDDLECTNCD = A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        AND A06.SMALLCTNCD = A01.SMALLCTNCD")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A07")
        SQLBldr.AppendLine("        ON A07.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A07.ORGCODE = A01.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND A07.DELFLG = @P01")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A08")
        SQLBldr.AppendLine("        ON A08.USERID = '" & htParm(PAYMENTLINK_KEY.PM_USERID).ToString & "'")
        SQLBldr.AppendLine("        AND A08.DELFLG = @P01")
        '[結合テーブル]支払ヘッダーデータ
        SQLBldr.AppendLine("    LEFT JOIN (")
        SQLBldr.AppendLine("	    SELECT ")
        SQLBldr.AppendLine("	        LNT0078.KEIJYOBRANCHCD")
        SQLBldr.AppendLine("			,LNT0078.REMARKS")
        SQLBldr.AppendLine("			,LNT0078.PAYADDSUB")
        SQLBldr.AppendLine("			,LNT0077.DELFLG")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTTYPE")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTYM")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTORGCODE")
        SQLBldr.AppendLine("			,LNT0077.TORICODE")
        SQLBldr.AppendLine("			,LNT0078.SEQNO")
        SQLBldr.AppendLine("			,LNT0077.SCHEDATEPAYMENT")
        SQLBldr.AppendLine("			,LNT0078.CTNTYPE")
        SQLBldr.AppendLine("			,LNT0078.CTNNO")
        SQLBldr.AppendLine("			,LNM0002.BIGCTNCD")
        SQLBldr.AppendLine("			,LNM0002.MIDDLECTNCD")
        SQLBldr.AppendLine("	    FROM lng.LNT0077_PAYMENTHEAD LNT0077")
        SQLBldr.AppendLine("        INNER JOIN lng.LNT0078_PAYMENTDATA LNT0078")
        SQLBldr.AppendLine("            ON LNT0078.DELFLG = '0'")
        SQLBldr.AppendLine("            AND LNT0078.PAYMENTTYPE = '1'")
        SQLBldr.AppendLine("		    AND LNT0078.AMOUNTTYPE = '1'")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTYM = LNT0078.PAYMENTYM")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTORGCODE = LNT0078.PAYMENTORGCODE")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTNUMBER = LNT0078.PAYMENTNUMBER")
        SQLBldr.AppendLine("		    AND LNT0077.TORICODE = LNT0078.TORICODE")
        SQLBldr.AppendLine("		LEFT JOIN lng.LNM0002_RECONM LNM0002")
        SQLBldr.AppendLine("			ON LNT0078.CTNTYPE = LNM0002.CTNTYPE")
        SQLBldr.AppendLine("			AND LNT0078.CTNNO = LNM0002.CTNNO")
        SQLBldr.AppendLine("			AND LNM0002.DELFLG = 0")
        SQLBldr.AppendLine("	) A09")
        SQLBldr.AppendLine("        ON A09.DELFLG = '0'")
        SQLBldr.AppendLine("        AND A09.PAYMENTTYPE = '1'")
        SQLBldr.AppendLine("		AND A01.KEIJOYM = A09.PAYMENTYM")
        SQLBldr.AppendLine("		AND A01.PAYFILINGBRANCH = A09.PAYMENTORGCODE")
        SQLBldr.AppendLine("		AND A01.TORICODE = A09.TORICODE")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN LNG.LNM0002_ORG A10")
        SQLBldr.AppendLine("        ON A10.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A10.ORGCODE = A09.KEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND A10.DELFLG = @P01")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A11")
        SQLBldr.AppendLine("	    ON A11.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A12")
        SQLBldr.AppendLine("	    ON A12.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A12.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(PAYMENTLINK_KEY.PM_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(PAYMENTLINK_KEY.PM_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(PAYMENTLINK_KEY.PM_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND TOTALCOST <> 0")
        '並び順
        SQLBldr.AppendLine(") D01")
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    D01.発駅コード")
        SQLBldr.AppendLine("    , D01.着駅コード")
        SQLBldr.AppendLine("    , D01.支払予定年月日")
        SQLBldr.AppendLine("    , D01.SEQNO")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

            PARA01.Value = C_DELETE_FLG.ALIVE
            PARA02.Value = htParm(PAYMENTLINK_KEY.PM_CORPCODE).ToString

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
    ''' 請求ヘッダーデータ 更新処理(支払書連携)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">請求ヘッダーデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Sub UpdateRenkeiHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim sqlSeikyuStat As New StringBuilder
        sqlSeikyuStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD ")
        sqlSeikyuStat.AppendLine("SET")
        sqlSeikyuStat.AppendLine("    PAYMENTLINK = CASE PAYMENTLINK")              '支払連携状態
        sqlSeikyuStat.AppendLine("        WHEN '0' THEN '1'")                       '　支払連携状態 = '0'の場合、'1'
        sqlSeikyuStat.AppendLine("        WHEN '1' THEN '2'")                       '　支払連携状態 = '1'の場合、'2'
        sqlSeikyuStat.AppendLine("        WHEN '2' THEN '2'")                       '　支払連携状態 = '2'の場合、'2'
        sqlSeikyuStat.AppendLine("        ELSE '1'")                                '　上記以外の場合、'1'
        sqlSeikyuStat.AppendLine("        END")
        sqlSeikyuStat.AppendLine("  , UPDYMD = @UPDYMD")                            '更新年月日
        sqlSeikyuStat.AppendLine("  , UPDUSER = @UPDUSER")                          '更新ユーザーＩＤ
        sqlSeikyuStat.AppendLine("  , UPDTERMID = @UPDTERMID")                      '更新端末
        sqlSeikyuStat.AppendLine("  , UPDPGID = @UPDPGID")                          '更新プログラムID
        sqlSeikyuStat.AppendLine("WHERE")
        sqlSeikyuStat.AppendLine("    PAYMENTYM = @PAYMENTYM")                      '支払年月
        sqlSeikyuStat.AppendLine("AND PAYMENTNUMBER = @PAYMENTNUMBER")              '支払番号
        sqlSeikyuStat.AppendLine("AND PAYMENTORGCODE = @PAYMENTORGCODE")            '支払支店コード
        sqlSeikyuStat.AppendLine("AND TORICODE = @TORICODE")                        '支払取引先コード
        sqlSeikyuStat.AppendLine("AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT")          '支払予定日
        sqlSeikyuStat.AppendLine("AND DELFLG = '0'")                                '削除フラグ

        Using sqlOrderCmd As New MySqlCommand(sqlSeikyuStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("PAYMENTYM", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTYM))                  '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTNUMBER))          '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_PAYMENTORGCODE))        '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_TORICODE))                    '支払取引先コード
                If Not String.IsNullOrEmpty(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENTHEAD).ToString) Then
                    .Add("SCHEDATEPAYMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENTHEAD))  '支払予定日
                Else
                    .Add("SCHEDATEPAYMENT", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_SCHEDATEPAYMENT))      '支払予定日
                End If
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDYMD))                        '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDUSER))                      '更新ユーザーID
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDTERMID))                  '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(PAYHEAD_PARM.PM_UPDPGID))                      '更新プログラムID
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub
#End Region

#Region "ドラフト版 お支払書連携"

    ''' <summary>
    ''' 支払ヘッダーデータ 検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectDraftPaymentHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    PAYMENTYM")                                             '支払年月
        SQLBldr.AppendLine("FROM")
        'メイン 請求ヘッダーデータ
        SQLBldr.AppendLine("    lng.LNT0077_PAYMENTHEAD")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        '請求年月
        SQLBldr.AppendLine("    PAYMENTYM = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'")
        '請求担当部店コード
        If Not String.IsNullOrEmpty(htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString) Then
            SQLBldr.AppendLine("    AND PAYMENTORGCODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'")
        End If
        '請求取引先コード
        If Not String.IsNullOrEmpty(htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString) Then
            SQLBldr.AppendLine("    AND TORICODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'")
        End If
        '入金予定日
        If Not String.IsNullOrEmpty(htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString) Then
            If Not String.IsNullOrEmpty(htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString) Then
                SQLBldr.AppendLine("    AND SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString & "'")
            Else
                SQLBldr.AppendLine("    AND SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'")
            End If
        End If
        '削除フラグ
        SQLBldr.AppendLine("    AND DELFLG = '0'")

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    PAYMENTYM")
        SQLBldr.AppendLine("    , PAYMENTORGCODE")
        SQLBldr.AppendLine("    , TORICODE")
        SQLBldr.AppendLine("    , PAYMENTTYPE")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

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
    ''' コンテナ清算ファイル 検索処理(請求連携用(お支払書))
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectDraftPaymentCsv(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    主キー")
        SQLBldr.AppendLine("    ,支払書発行年月日")
        SQLBldr.AppendLine("    ,費用計上日付")
        SQLBldr.AppendLine("    ,支払予定年月日")
        SQLBldr.AppendLine("    ,顧客コード")
        SQLBldr.AppendLine("    ,支払先顧客選択")
        SQLBldr.AppendLine("    ,顧客名")
        SQLBldr.AppendLine("    ,提出部店")
        SQLBldr.AppendLine("    ,提出部店名")
        SQLBldr.AppendLine("    ,計上部店")
        SQLBldr.AppendLine("    ,計上部店名")
        SQLBldr.AppendLine("    ,帳票種別")
        SQLBldr.AppendLine("    ,発駅コード")
        SQLBldr.AppendLine("    ,発駅名")
        SQLBldr.AppendLine("    ,着駅コード")
        SQLBldr.AppendLine("    ,着駅名")
        SQLBldr.AppendLine("    ,大分類コード")
        SQLBldr.AppendLine("    ,大分類名")
        SQLBldr.AppendLine("    ,中分類コード")
        SQLBldr.AppendLine("    ,回送個数")
        SQLBldr.AppendLine("    ,[科目（回送運賃）]")
        SQLBldr.AppendLine("    ,[細目（回送運賃）]")
        SQLBldr.AppendLine("    ,[金額（回送運賃）]")
        SQLBldr.AppendLine("    ,[科目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[細目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[金額（修理時運賃）]")
        SQLBldr.AppendLine("    ,[科目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（除却時運賃）]")
        SQLBldr.AppendLine("    ,[科目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（売却時運賃）]")
        SQLBldr.AppendLine("    ,青函付加金")
        SQLBldr.AppendLine("    ,[科目（発送料）]")
        SQLBldr.AppendLine("    ,[細目（発送料）]")
        SQLBldr.AppendLine("    ,[金額（発送料）]")
        SQLBldr.AppendLine("    ,[科目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[細目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[金額（修理時発送料）]")
        SQLBldr.AppendLine("    ,[科目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（除却時発送料）]")
        SQLBldr.AppendLine("    ,[科目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（売却時発送料）]")
        SQLBldr.AppendLine("    ,発行担当者名")
        SQLBldr.AppendLine("    ,宛名欄付記１")
        SQLBldr.AppendLine("FROM (")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '1' AS SORTNO")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-30' AS 主キー")             '主キー
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")                                                                 '支払書発行年月日
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")                                                              '費用計上日付
        If (htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString) <> "" Then
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString & "'" & " AS 支払予定年月日")               '支払予定年月日（ヘッダー）
        Else
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'" & " AS 支払予定年月日")                   '支払予定年月日
        End If
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")                                                                                      '支払先顧客選択
        SQLBldr.AppendLine("    , A10.CLIENTNAME AS 顧客名")                                                                                            '顧客名
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")                                                                             '提出部店
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")                                                                                              '提出部店名
        SQLBldr.AppendLine("    , '01-' + A01.PAYKEIJYOBRANCHCD AS 計上部店")                                                                           '計上部店
        SQLBldr.AppendLine("    , A08.NAME AS 計上部店名")                                                                                              '計上部店名
        SQLBldr.AppendLine("	, '30' AS 帳票種別")                      　　　　　                                                                    '帳票種別
        SQLBldr.AppendLine("    , A01.DEPSTATION AS 発駅コード")                                                                                        '発駅コード
        SQLBldr.AppendLine("    , A04.NAMES AS 発駅名")                                                                                                 '発駅名
        SQLBldr.AppendLine("    , A01.ARRSTATION AS 着駅コード")                                                                                        '着駅コード
        SQLBldr.AppendLine("    , A05.NAMES AS 着駅名")                                                                                                 '着駅名
        SQLBldr.AppendLine("    , A01.BIGCTNCD AS 大分類コード")                                                                                        '大分類コード
        SQLBldr.AppendLine("    , CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        WHEN '20' THEN '無蓋20'")
        SQLBldr.AppendLine("        ELSE A06.KANJI1")
        SQLBldr.AppendLine("      END AS 大分類名")                                                                                                     '大分類名
        SQLBldr.AppendLine("    , A01.MIDDLECTNCD AS 中分類コード")                                                                                     '中分類コード
        SQLBldr.AppendLine("    , A02.SUM_QUANTITY AS 回送個数")                                                                                        '回送個数
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")                                                                              '金額（除却時運賃）
        SQLBldr.AppendLine("    , A02.SUM_OTHER1FEE AS 青函付加金")                                                                                     '青函付加金
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A09.STAFFNAMES AS 発行担当者名")                                                                                      '発行担当者名
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")                                                                 '宛名欄付記１
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , coalesce(A1.BIGCTNCD,'') AS BIGCTNCD")
        SQLBldr.AppendLine("            , coalesce(A1.MIDDLECTNCD,'') AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2 AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("            , SUM(A1.OTHER1FEE) AS SUM_OTHER1FEE")
        SQLBldr.AppendLine("            , SUM(A1.QUANTITY) AS SUM_QUANTITY")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	            AND DUMMYKBN = 0")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("	        AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("	        AND A1.DUMMYKBN = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , A1.BIGCTNCD")
        SQLBldr.AppendLine("            , A1.MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        SQLBldr.AppendLine("		AND (A02.SUM_FREESENDFEE <> 0")
        SQLBldr.AppendLine("		     OR A02.SUM_SHIPFEE <> 0")
        SQLBldr.AppendLine("		     OR A02.SUM_OTHER1FEE <> 0)")
        '[結合テーブル]駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A04")
        SQLBldr.AppendLine("        ON A04.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A04.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        '[結合テーブル]駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A05.STATION = A01.ARRSTATION ")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '[結合テーブル]大中小分類マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0022_CLASS A06")
        SQLBldr.AppendLine("        ON A06.BIGCTNCD = A01.BIGCTNCD")
        SQLBldr.AppendLine("        AND A06.MIDDLECTNCD = A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        AND A06.SMALLCTNCD = A01.SMALLCTNCD")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A07                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A07.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A07.ORGCODE  = A01.PAYFILINGBRANCH                                                                                         ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A07.STYMD AND A07.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A07.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A08                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A08.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A08.ORGCODE  = A01.PAYKEIJYOBRANCHCD                                                                                       ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A08.STYMD AND A08.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A08.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN(                                                                                                                     ")
        SQLBldr.AppendLine("        SELECT TOP(1)                                                                                                              ")
        SQLBldr.AppendLine("            T1.USERID                                                                                                              ")
        SQLBldr.AppendLine("           ,T1.STAFFNAMES                                                                                                          ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            com.lns0001_user T1                                                                                                    ")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            T1.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                             ")
        SQLBldr.AppendLine("        AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN T1.STYMD AND T1.ENDYMD                              ")
        SQLBldr.AppendLine("        AND T1.DELFLG  = @P01                                                                                                      ")
        SQLBldr.AppendLine("        ORDER BY                                                                                                                   ")
        SQLBldr.AppendLine("            T1.STYMD DESC                                                                                                          ")
        SQLBldr.AppendLine("    ) A09                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A09.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                                ")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 0")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        SQLBldr.AppendLine("UNION ALL")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '2' AS SORTNO")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-30' AS 主キー")           '主キー
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")                                                                 '支払書発行年月日
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")                                                              '費用計上日付
        If (htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString) <> "" Then
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString & "'" & " AS 支払予定年月日")               '支払予定年月日（ヘッダー）
        Else
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'" & " AS 支払予定年月日")                   '支払予定年月日
        End If
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")                                                                                      '支払先顧客選択
        SQLBldr.AppendLine("    , A10.CLIENTNAME AS 顧客名")                                                                                            '顧客名
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")                                                                             '提出部店
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")                                                                                              '提出部店名
        SQLBldr.AppendLine("    , '01-' + A01.PAYKEIJYOBRANCHCD AS 計上部店")                                                                           '計上部店
        SQLBldr.AppendLine("    , A08.NAME AS 計上部店名")                                                                                              '計上部店名
        SQLBldr.AppendLine("	, '30' AS 帳票種別")
        SQLBldr.AppendLine("    , NULL AS 発駅コード")
        SQLBldr.AppendLine("    , '加減額' AS 発駅名")
        SQLBldr.AppendLine("    , NULL AS 着駅コード")
        SQLBldr.AppendLine("    , NULL AS 着駅名")
        SQLBldr.AppendLine("    , NULL AS 大分類コード")
        SQLBldr.AppendLine("    , NULL AS 大分類名")
        SQLBldr.AppendLine("    , NULL AS 中分類コード")
        SQLBldr.AppendLine("    , NULL AS 回送個数")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , 0 AS 青函付加金")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A09.STAFFNAMES AS 発行担当者名")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , coalesce(A1.BIGCTNCD,'') AS BIGCTNCD")
        SQLBldr.AppendLine("            , coalesce(A1.MIDDLECTNCD,'') AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2 AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("				AND DUMMYKBN = 1")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("        	AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("			AND A1.DUMMYKBN = 1")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , A1.BIGCTNCD")
        SQLBldr.AppendLine("            , A1.MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        SQLBldr.AppendLine("		AND (A02.SUM_FREESENDFEE <> 0 OR A02.SUM_SHIPFEE <> 0)")
        '[結合テーブル]駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A04")
        SQLBldr.AppendLine("        ON A04.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A04.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        '[結合テーブル]駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A05.STATION = A01.ARRSTATION ")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '[結合テーブル]大中小分類マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0022_CLASS A06")
        SQLBldr.AppendLine("        ON A06.BIGCTNCD = A01.BIGCTNCD")
        SQLBldr.AppendLine("        AND A06.MIDDLECTNCD = A01.MIDDLECTNCD")
        SQLBldr.AppendLine("        AND A06.SMALLCTNCD = A01.SMALLCTNCD")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A07                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A07.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A07.ORGCODE  = A01.PAYFILINGBRANCH                                                                                         ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A07.STYMD AND A07.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A07.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A08                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A08.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A08.ORGCODE  = A01.PAYKEIJYOBRANCHCD                                                                                       ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A08.STYMD AND A08.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A08.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN(                                                                                                                     ")
        SQLBldr.AppendLine("        SELECT TOP(1)                                                                                                              ")
        SQLBldr.AppendLine("            T1.USERID                                                                                                              ")
        SQLBldr.AppendLine("           ,T1.STAFFNAMES                                                                                                          ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            com.lns0001_user T1                                                                                                    ")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            T1.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                             ")
        SQLBldr.AppendLine("        AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN T1.STYMD AND T1.ENDYMD                              ")
        SQLBldr.AppendLine("        AND T1.DELFLG  = @P01                                                                                                      ")
        SQLBldr.AppendLine("        ORDER BY                                                                                                                   ")
        SQLBldr.AppendLine("            T1.STYMD DESC                                                                                                          ")
        SQLBldr.AppendLine("    ) A09                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A09.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                                ")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 1")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        '並び順
        SQLBldr.AppendLine(") D01")
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    SORTNO")
        SQLBldr.AppendLine("    , D01.発駅コード")
        SQLBldr.AppendLine("    , D01.着駅コード")
        SQLBldr.AppendLine("    , D01.支払予定年月日")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

            PARA01.Value = C_DELETE_FLG.ALIVE
            PARA02.Value = htParm(DRAFTPAYMENTLINK_KEY.SL_CAMPCODE).ToString

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
    ''' 支払ヘッダーデータ 検索処理(支払連携用(お支払書))
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectDraftPaymentCsv2(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    主キー")
        SQLBldr.AppendLine("    ,支払書発行年月日")
        SQLBldr.AppendLine("    ,費用計上日付")
        SQLBldr.AppendLine("    ,支払予定年月日")
        SQLBldr.AppendLine("    ,顧客コード")
        SQLBldr.AppendLine("    ,支払先顧客選択")
        SQLBldr.AppendLine("    ,顧客名")
        SQLBldr.AppendLine("    ,提出部店")
        SQLBldr.AppendLine("    ,提出部店名")
        SQLBldr.AppendLine("    ,計上部店")
        SQLBldr.AppendLine("    ,計上部店名")
        SQLBldr.AppendLine("    ,帳票種別")
        SQLBldr.AppendLine("    ,発駅コード")
        SQLBldr.AppendLine("    ,発駅名")
        SQLBldr.AppendLine("    ,着駅コード")
        SQLBldr.AppendLine("    ,着駅名")
        SQLBldr.AppendLine("    ,大分類コード")
        SQLBldr.AppendLine("    ,大分類名")
        SQLBldr.AppendLine("    ,中分類コード")
        SQLBldr.AppendLine("    ,回送個数")
        SQLBldr.AppendLine("    ,[科目（回送運賃）]")
        SQLBldr.AppendLine("    ,[細目（回送運賃）]")
        SQLBldr.AppendLine("    ,[金額（回送運賃）]")
        SQLBldr.AppendLine("    ,[科目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[細目（修理時運賃）]")
        SQLBldr.AppendLine("    ,[金額（修理時運賃）]")
        SQLBldr.AppendLine("    ,[科目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（除却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（除却時運賃）]")
        SQLBldr.AppendLine("    ,[科目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[細目（売却時運賃）]")
        SQLBldr.AppendLine("    ,[金額（売却時運賃）]")
        SQLBldr.AppendLine("    ,青函付加金")
        SQLBldr.AppendLine("    ,[科目（発送料）]")
        SQLBldr.AppendLine("    ,[細目（発送料）]")
        SQLBldr.AppendLine("    ,[金額（発送料）]")
        SQLBldr.AppendLine("    ,[科目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[細目（修理時発送料）]")
        SQLBldr.AppendLine("    ,[金額（修理時発送料）]")
        SQLBldr.AppendLine("    ,[科目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（除却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（除却時発送料）]")
        SQLBldr.AppendLine("    ,[科目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[細目（売却時発送料）]")
        SQLBldr.AppendLine("    ,[金額（売却時発送料）]")
        SQLBldr.AppendLine("    ,発行担当者名")
        SQLBldr.AppendLine("    ,宛名欄付記１")
        SQLBldr.AppendLine("    ,SEQNO")
        SQLBldr.AppendLine("FROM (")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-30' AS 主キー")
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")
        If (htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString) <> "" Then
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENTHEAD).ToString & "'" & " AS 支払予定年月日")               '支払予定年月日（ヘッダー）
        Else
            SQLBldr.AppendLine("    ," & "'" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'" & " AS 支払予定年月日")                   '支払予定年月日
        End If
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")
        SQLBldr.AppendLine("    , A12.CLIENTNAME AS 顧客名")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")
        SQLBldr.AppendLine("    , A03.NAME AS 提出部店名")
        SQLBldr.AppendLine("    , '01-' + A05.KEIJYOBRANCHCD AS 計上部店")
        SQLBldr.AppendLine("    , A06.NAME AS 計上部店名")
        SQLBldr.AppendLine("	, '30' AS 帳票種別")
        SQLBldr.AppendLine("    , NULL AS 発駅コード")
        SQLBldr.AppendLine("    , '加減額' AS 発駅名")
        SQLBldr.AppendLine("    , NULL AS 着駅コード")
        SQLBldr.AppendLine("    , A05.REMARKS AS 着駅名")
        SQLBldr.AppendLine("    , NULL AS 大分類コード")
        SQLBldr.AppendLine("    , NULL AS 大分類名")
        SQLBldr.AppendLine("    , NULL AS 中分類コード")
        SQLBldr.AppendLine("    , NULL AS 回送個数")
        SQLBldr.AppendLine("    , 'J-51040101' AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE WHEN (A05.CTNTYPE = 'KAGEN' AND A05.CTNNO = 0) THEN")
        SQLBldr.AppendLine("	      CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("              WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("              WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("              WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("              WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("              WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("              WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("              WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("              WHEN '35' THEN")
        SQLBldr.AppendLine("                  CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                      WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                      ELSE 'J-30206'")
        SQLBldr.AppendLine("                  END")
        SQLBldr.AppendLine("          END")
        SQLBldr.AppendLine("	  ELSE")
        SQLBldr.AppendLine("	      CASE A05.BIGCTNCD")
        SQLBldr.AppendLine("              WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("              WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("              WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("              WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("              WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("              WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("              WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("              WHEN '35' THEN")
        SQLBldr.AppendLine("                  CASE A05.MIDDLECTNCD")
        SQLBldr.AppendLine("                      WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                      ELSE 'J-30206'")
        SQLBldr.AppendLine("                  END")
        SQLBldr.AppendLine("          END")
        SQLBldr.AppendLine("	  END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , A05.PAYADDSUB AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , NULL AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , 0 AS 青函付加金")
        SQLBldr.AppendLine("    , NULL AS '科目（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , NULL AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A04.STAFFNAMES AS 発行担当者名")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")
        SQLBldr.AppendLine("    , SEQNO")
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , MIN(coalesce(A1.BIGCTNCD,'')) AS BIGCTNCD")
        SQLBldr.AppendLine("            , MIN(coalesce(A1.MIDDLECTNCD,'')) AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , MIN(A1.STACKFREEKBN) AS STACKFREEKBN")
        SQLBldr.AppendLine("            , MIN(B1.ACCOUNTSTATUSKBN2) AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.TOTALCOST,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("        	AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        '[結合テーブル]組織マスタ(提出部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A03                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A03.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A03.ORGCODE  = A01.PAYFILINGBRANCH                                                                                         ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A03.STYMD AND A03.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A03.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]ユーザーマスタ
        SQLBldr.AppendLine("    LEFT JOIN(                                                                                                                     ")
        SQLBldr.AppendLine("        SELECT TOP(1)                                                                                                              ")
        SQLBldr.AppendLine("            T1.USERID                                                                                                              ")
        SQLBldr.AppendLine("           ,T1.STAFFNAMES                                                                                                          ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            com.lns0001_user T1                                                                                                    ")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            T1.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                             ")
        SQLBldr.AppendLine("        AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN T1.STYMD AND T1.ENDYMD                              ")
        SQLBldr.AppendLine("        AND T1.DELFLG  = @P01                                                                                                      ")
        SQLBldr.AppendLine("        ORDER BY                                                                                                                   ")
        SQLBldr.AppendLine("            T1.STYMD DESC                                                                                                          ")
        SQLBldr.AppendLine("    ) A04                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A04.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                                ")
        '[結合テーブル]支払ヘッダーデータ
        SQLBldr.AppendLine("    LEFT JOIN (")
        SQLBldr.AppendLine("	    SELECT ")
        SQLBldr.AppendLine("	        LNT0078.KEIJYOBRANCHCD")
        SQLBldr.AppendLine("			,LNT0078.REMARKS")
        SQLBldr.AppendLine("			,LNT0078.PAYADDSUB")
        SQLBldr.AppendLine("			,LNT0077.DELFLG")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTTYPE")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTYM")
        SQLBldr.AppendLine("			,LNT0077.PAYMENTORGCODE")
        SQLBldr.AppendLine("			,LNT0077.TORICODE")
        SQLBldr.AppendLine("			,LNT0078.SEQNO")
        SQLBldr.AppendLine("			,LNT0077.SCHEDATEPAYMENT")
        SQLBldr.AppendLine("			,LNT0078.CTNTYPE")
        SQLBldr.AppendLine("			,LNT0078.CTNNO")
        SQLBldr.AppendLine("			,LNM0002.BIGCTNCD")
        SQLBldr.AppendLine("			,LNM0002.MIDDLECTNCD")
        SQLBldr.AppendLine("	    FROM lng.LNT0077_PAYMENTHEAD LNT0077")
        SQLBldr.AppendLine("        INNER JOIN lng.LNT0078_PAYMENTDATA LNT0078")
        SQLBldr.AppendLine("            ON LNT0078.DELFLG = '0'")
        SQLBldr.AppendLine("            AND LNT0078.PAYMENTTYPE = '1'")
        SQLBldr.AppendLine("		    AND LNT0078.AMOUNTTYPE = '1'")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTYM = LNT0078.PAYMENTYM")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTORGCODE = LNT0078.PAYMENTORGCODE")
        SQLBldr.AppendLine("		    AND LNT0077.PAYMENTNUMBER = LNT0078.PAYMENTNUMBER")
        SQLBldr.AppendLine("		    AND LNT0077.TORICODE = LNT0078.TORICODE")
        SQLBldr.AppendLine("		LEFT JOIN lng.LNM0002_RECONM LNM0002")
        SQLBldr.AppendLine("			ON LNT0078.CTNTYPE = LNM0002.CTNTYPE")
        SQLBldr.AppendLine("			AND LNT0078.CTNNO = LNM0002.CTNNO")
        SQLBldr.AppendLine("			AND LNM0002.DELFLG = 0")
        SQLBldr.AppendLine("	) A05")
        SQLBldr.AppendLine("        ON A05.DELFLG = '0'")
        SQLBldr.AppendLine("        AND A05.PAYMENTTYPE = '1'")
        SQLBldr.AppendLine("		AND A01.KEIJOYM = A05.PAYMENTYM")
        SQLBldr.AppendLine("		AND A01.PAYFILINGBRANCH = A05.PAYMENTORGCODE")
        SQLBldr.AppendLine("		AND A01.TORICODE = A05.TORICODE")
        '[結合テーブル]組織マスタ(計上部店)
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A06                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A06.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A06.ORGCODE  = A05.KEIJYOBRANCHCD                                                                                          ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A06.STYMD AND A06.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A06.DELFLG   = @P01                                                                                                        ")
        '[結合テーブル]支払先マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A12")
        SQLBldr.AppendLine("	    ON A12.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("		AND A12.DELFLG = '0' ")
        '[結合テーブル]支払予定日マスタ連携
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A13")
        SQLBldr.AppendLine("	    ON A13.PAYMENTYM = A01.KEIJOYM")
        SQLBldr.AppendLine("		AND A13.DELFLG = '0' ")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        '並び順
        SQLBldr.AppendLine(") D01")
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    D01.発駅コード")
        SQLBldr.AppendLine("    , D01.着駅コード")
        SQLBldr.AppendLine("    , D01.支払予定年月日")
        SQLBldr.AppendLine("    , D01.SEQNO")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

            PARA01.Value = C_DELETE_FLG.ALIVE
            PARA02.Value = htParm(DRAFTPAYMENTLINK_KEY.SL_CAMPCODE).ToString

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
    ''' ドラフト版お支払書連携実績テーブル 検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htRentData">請求明細使用料データ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectDraftPaymentLog(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htRentData As Hashtable) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT                                                                                          ")
        SQLBldr.AppendLine("   KEIJOYM                                                                                      ")
        SQLBldr.AppendLine("FROM                                                                                            ")
        SQLBldr.AppendLine("    LNG.LNT0083_DRAFTPAYMENTLOG                                                                 ")
        SQLBldr.AppendLine("WHERE                                                                                           ")
        SQLBldr.AppendLine("    KEIJOYM         = '" & htRentData(DRAFT_PAYMENTLOG_KEY.DRAFT_KEIJOYM).ToString & "'         ")    '支払年月
        SQLBldr.AppendLine("AND TORICODE        = '" & htRentData(DRAFT_PAYMENTLOG_KEY.DRAFT_TORICODE).ToString & "'        ")    '支払取引先コード
        SQLBldr.AppendLine("AND PAYMENTORGCODE  = '" & htRentData(DRAFT_PAYMENTLOG_KEY.DRAFT_PAYMENTORGCODE).ToString & "'  ")    '支払支店コード
        SQLBldr.AppendLine("AND SCHEDATEPAYMENT = '" & htRentData(DRAFT_PAYMENTLOG_KEY.DRAFT_SCHEDATEPAYMENT).ToString & "' ")    '支払日
        SQLBldr.AppendLine("AND DELFLG          = @P01                                                                      ")    '削除フラグ

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ

            PARA01.Value = C_DELETE_FLG.ALIVE

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
    ''' ドラフト版お支払書連携実績テーブル 登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htInsData">ドラフト版請求書連携実績テーブル</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Sub InsertDraftPaymentLog(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htInsData As Hashtable)

        '◯受注TBL
        Dim sqlSeikyuStat As New StringBuilder
        sqlSeikyuStat.AppendLine("INSERT INTO LNG.LNT0083_DRAFTPAYMENTLOG ( ")
        sqlSeikyuStat.AppendLine("    KEIJOYM                               ")      '支払年月
        sqlSeikyuStat.AppendLine("   ,TORICODE                              ")      '支払取引先コード
        sqlSeikyuStat.AppendLine("   ,PAYMENTORGCODE                        ")      '支払支店コード
        sqlSeikyuStat.AppendLine("   ,SCHEDATEPAYMENT                       ")      '支払日
        sqlSeikyuStat.AppendLine("   ,DELFLG                                ")      '削除フラグ
        sqlSeikyuStat.AppendLine("   ,INITYMD                               ")      '登録年月日
        sqlSeikyuStat.AppendLine("   ,INITUSER                              ")      '登録ユーザーID
        sqlSeikyuStat.AppendLine("   ,INITTERMID                            ")      '登録端末
        sqlSeikyuStat.AppendLine("   ,INITPGID                              ")      '登録プログラムID
        sqlSeikyuStat.AppendLine(")                                         ")
        sqlSeikyuStat.AppendLine(" VALUES(                                  ")
        sqlSeikyuStat.AppendLine("    @KEIJOYM                              ")      '支払年月
        sqlSeikyuStat.AppendLine("   ,@TORICODE                             ")      '支払取引先コード
        sqlSeikyuStat.AppendLine("   ,@PAYMENTORGCODE                       ")      '支払支店コード
        sqlSeikyuStat.AppendLine("   ,@SCHEDATEPAYMENT                      ")      '支払日
        sqlSeikyuStat.AppendLine("   ,@DELFLG                               ")      '削除フラグ
        sqlSeikyuStat.AppendLine("   ,@INITYMD                              ")      '登録年月日
        sqlSeikyuStat.AppendLine("   ,@INITUSER                             ")      '登録ユーザーID
        sqlSeikyuStat.AppendLine("   ,@INITTERMID                           ")      '登録端末
        sqlSeikyuStat.AppendLine("   ,@INITPGID                             ")      '登録プログラムID
        sqlSeikyuStat.AppendLine(")                                         ")

        Using sqlOrderCmd As New MySqlCommand(sqlSeikyuStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("KEIJOYM", MySqlDbType.Int32).Value = htInsData(INSERT_DRAFTPAYMENTLOG.IP_KEIJOYM)                           '支払年月
                .Add("TORICODE", MySqlDbType.VarChar).Value = htInsData(INSERT_DRAFTPAYMENTLOG.IP_TORICODE)                    '支払取引先コード
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = htInsData(INSERT_DRAFTPAYMENTLOG.IP_PAYMENTORGCODE)        '支払支店コード
                .Add("SCHEDATEPAYMENT", MySqlDbType.VarChar).Value = htInsData(INSERT_DRAFTPAYMENTLOG.IP_SCHEDATEPAYMENT)      '支払日
                .Add("DELFLG", MySqlDbType.Int32).Value = BlankToDBNull(htInsData(INSERT_DRAFTPAYMENTLOG.IP_DELFLG))              '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htInsData(INSERT_DRAFTPAYMENTLOG.IP_INITYMD))       '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htInsData(INSERT_DRAFTPAYMENTLOG.IP_INITUSER))     '登録ユーザーID
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htInsData(INSERT_DRAFTPAYMENTLOG.IP_INITTERMID)) '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htInsData(INSERT_DRAFTPAYMENTLOG.IP_INITPGID))     '登録プログラムID
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' コンテナ清算ファイル 検索処理(ドラフト版 お支払書連携))
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htParm">パラメータデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectDraftPaymentCsv_RESSNF(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htParm As Hashtable) As DataTable
        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT                                                                                                                             ")
        SQLBldr.AppendLine("    主キー                                                                                                                         ")
        SQLBldr.AppendLine("    ,支払書発行年月日                                                                                                              ")
        SQLBldr.AppendLine("    ,費用計上日付                                                                                                                  ")
        SQLBldr.AppendLine("    ,支払予定年月日                                                                                                                ")
        SQLBldr.AppendLine("    ,顧客コード                                                                                                                    ")
        SQLBldr.AppendLine("    ,支払先顧客選択                                                                                                                ")
        SQLBldr.AppendLine("    ,顧客名                                                                                                                        ")
        SQLBldr.AppendLine("    ,提出部店                                                                                                                      ")
        SQLBldr.AppendLine("    ,提出部店名                                                                                                                    ")
        SQLBldr.AppendLine("    ,計上部店                                                                                                                      ")
        SQLBldr.AppendLine("    ,計上部店名                                                                                                                    ")
        SQLBldr.AppendLine("    ,帳票種別                                                                                                                      ")
        SQLBldr.AppendLine("    ,発駅コード                                                                                                                    ")
        SQLBldr.AppendLine("    ,発駅名                                                                                                                        ")
        SQLBldr.AppendLine("    ,着駅コード                                                                                                                    ")
        SQLBldr.AppendLine("    ,着駅名                                                                                                                        ")
        SQLBldr.AppendLine("    ,大分類コード                                                                                                                  ")
        SQLBldr.AppendLine("    ,大分類名                                                                                                                      ")
        SQLBldr.AppendLine("    ,中分類コード                                                                                                                  ")
        SQLBldr.AppendLine("    ,回送個数                                                                                                                      ")
        SQLBldr.AppendLine("    ,[科目（回送運賃）]                                                                                                            ")
        SQLBldr.AppendLine("    ,[細目（回送運賃）]                                                                                                            ")
        SQLBldr.AppendLine("    ,[金額（回送運賃）]                                                                                                            ")
        SQLBldr.AppendLine("    ,[科目（修理時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[細目（修理時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[金額（修理時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[科目（除却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[細目（除却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[金額（除却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[科目（売却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[細目（売却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,[金額（売却時運賃）]                                                                                                          ")
        SQLBldr.AppendLine("    ,青函付加金                                                                                                                    ")
        SQLBldr.AppendLine("    ,[科目（発送料）]                                                                                                              ")
        SQLBldr.AppendLine("    ,[細目（発送料）]                                                                                                              ")
        SQLBldr.AppendLine("    ,[金額（発送料）]                                                                                                              ")
        SQLBldr.AppendLine("    ,[科目（修理時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[細目（修理時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[金額（修理時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[科目（除却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[細目（除却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[金額（除却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[科目（売却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[細目（売却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,[金額（売却時発送料）]                                                                                                        ")
        SQLBldr.AppendLine("    ,発行担当者名                                                                                                                  ")
        SQLBldr.AppendLine("    ,宛名欄付記１                                                                                                                  ")
        SQLBldr.AppendLine("FROM (                                                                                                                             ")
        SQLBldr.AppendLine("    SELECT DISTINCT                                                                                                                ")
        SQLBldr.AppendLine("        '1' AS SORTNO                                                                                                                     ")
        SQLBldr.AppendLine("        ,'01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-30' AS 主キー                 ")
        SQLBldr.AppendLine("        ,FORMAT(CURDATE(), 'yyyy/MM/dd')                                                                 AS 支払書発行年月日       ")
        SQLBldr.AppendLine("        ,FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01'                                                          AS 費用計上日付           ")
        SQLBldr.AppendLine("        ,CASE WHEN A11.SCHEDATEPAYMENT IS NULL THEN A01.SCHEDATEPAYMENT                                                            ")
        SQLBldr.AppendLine("		      ELSE A11.SCHEDATEPAYMENT END                                                               AS 支払予定年月日         ")
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("        ,A01.TORICODE                                                                                    AS 支払先顧客選択         ")
        SQLBldr.AppendLine("        ,A10.CLIENTNAME                                                                                  AS 顧客名                 ")
        SQLBldr.AppendLine("        ,'01-' + A01.PAYFILINGBRANCH                                                                     AS 提出部店               ")
        SQLBldr.AppendLine("        ,A07.NAME                                                                                        AS 提出部店名             ")
        SQLBldr.AppendLine("        ,'01-' + A01.PAYKEIJYOBRANCHCD                                                                   AS 計上部店               ")
        SQLBldr.AppendLine("        ,A08.NAME                                                                                        AS 計上部店名             ")
        SQLBldr.AppendLine("        ,'30'                                                                                            AS 帳票種別               ")
        SQLBldr.AppendLine("        ,A01.DEPSTATION                                                                                  AS 発駅コード             ")
        SQLBldr.AppendLine("        ,A04.NAMES                                                                                       AS 発駅名                 ")
        SQLBldr.AppendLine("        ,A01.ARRSTATION                                                                                  AS 着駅コード             ")
        SQLBldr.AppendLine("        ,A05.NAMES                                                                                       AS 着駅名                 ")
        SQLBldr.AppendLine("        ,A01.BIGCTNCD                                                                                    AS 大分類コード           ")
        SQLBldr.AppendLine("        ,CASE A01.MIDDLECTNCD                                                                                                      ")
        SQLBldr.AppendLine("             WHEN '20' THEN '無蓋20'                                                                                               ")
        SQLBldr.AppendLine("             ELSE A06.KANJI1                                                                                                       ")
        SQLBldr.AppendLine("         END                                                                                             AS 大分類名               ")
        SQLBldr.AppendLine("        ,A01.MIDDLECTNCD                                                                                 AS 中分類コード           ")
        SQLBldr.AppendLine("        ,A02.SUM_QUANTITY                                                                                AS 回送個数               ")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("            CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                WHEN '35' THEN")
        SQLBldr.AppendLine("                    CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                        WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                        ELSE 'J-30206'")
        SQLBldr.AppendLine("                    END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("        ,A02.SUM_OTHER1FEE                                                                               AS 青函付加金             ")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("        ,A09.STAFFNAMES                                                                       AS 発行担当者名                      ")
        SQLBldr.AppendLine("        ,FORMAT(A01.SHIPYMD, 'yyyy年MM月分')                                                  AS 宛名欄付記１                      ")
        SQLBldr.AppendLine("    FROM                                                                                                                           ")
        SQLBldr.AppendLine("        LNG.LNT0017_RESSNF A01                                                                                                     ")
        SQLBldr.AppendLine("    INNER JOIN(                                                                                                                    ")
        SQLBldr.AppendLine("        SELECT                                                                                                                     ")
        SQLBldr.AppendLine("             A1.KEIJOYM                                                                      AS KEIJOYM                            ")
        SQLBldr.AppendLine("            ,coalesce(A1.TORICODE, '')                                                         AS TORICODE                           ")
        SQLBldr.AppendLine("            ,coalesce(A1.PAYFILINGBRANCH, '')                                                  AS PAYFILINGBRANCH                    ")
        SQLBldr.AppendLine("            ,coalesce(A1.PAYKEIJYOBRANCHCD, '')                                                AS PAYKEIJYOBRANCHCD                  ")
        SQLBldr.AppendLine("            ,A1.DEPSTATION                                                                   AS DEPSTATION                         ")
        SQLBldr.AppendLine("            ,A1.ARRSTATION                                                                   AS ARRSTATION                         ")
        SQLBldr.AppendLine("            ,coalesce(A1.BIGCTNCD, '')                                                         AS BIGCTNCD                           ")
        SQLBldr.AppendLine("            ,coalesce(A1.MIDDLECTNCD, '')                                                      AS MIDDLECTNCD                        ")
        SQLBldr.AppendLine("            ,A1.STACKFREEKBN                                                                 AS STACKFREEKBN                       ")
        SQLBldr.AppendLine("            ,B1.ACCOUNTSTATUSKBN2                                                            AS ACCOUNTSTATUSKBN                   ")
        SQLBldr.AppendLine("            ,SUM(coalesce(A1.FREESENDFEE, 0) + coalesce(A1.COSTADJUSTFEE,0))                     AS SUM_FREESENDFEE                    ")
        SQLBldr.AppendLine("            ,SUM(coalesce(A1.SHIPFEE, 0) + coalesce(A1.COMMISSIONFEE,0))                         AS SUM_SHIPFEE                        ")
        SQLBldr.AppendLine("            ,SUM(A1.OTHER1FEE)                                                               AS SUM_OTHER1FEE                      ")
        SQLBldr.AppendLine("            ,SUM(A1.QUANTITY)                                                                AS SUM_QUANTITY                       ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                        THEN '3'")
        SQLBldr.AppendLine("                    WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                      CASE ")
        SQLBldr.AppendLine("				        WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                          THEN '5'")
        SQLBldr.AppendLine("					    WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("			    		  THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("			    		END")
        SQLBldr.AppendLine("                    ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("                  END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	            AND DUMMYKBN = 0")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'                                                                                                  ")
        SQLBldr.AppendLine("        AND A1.DELFLG       = @P01                                                                                                 ")
        SQLBldr.AppendLine("        AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')                                                                  ")
        SQLBldr.AppendLine("        AND A1.ACCOUNTINGASSETSKBN = '1'                                                                                           ")
        SQLBldr.AppendLine("	    AND A1.TOTALCOST <> 0                                                                                                      ")
        SQLBldr.AppendLine("	    AND A1.DUMMYKBN = 0                                                                                                        ")
        SQLBldr.AppendLine("        GROUP BY                                                                                                                   ")
        SQLBldr.AppendLine("             A1.KEIJOYM                                                                                                            ")
        SQLBldr.AppendLine("            ,A1.TORICODE                                                                                                           ")
        SQLBldr.AppendLine("            ,A1.PAYFILINGBRANCH                                                                                                    ")
        SQLBldr.AppendLine("            ,A1.PAYKEIJYOBRANCHCD                                                                                                  ")
        SQLBldr.AppendLine("            ,A1.DEPSTATION                                                                                                         ")
        SQLBldr.AppendLine("            ,A1.ARRSTATION                                                                                                         ")
        SQLBldr.AppendLine("            ,A1.BIGCTNCD                                                                                                           ")
        SQLBldr.AppendLine("            ,A1.MIDDLECTNCD                                                                                                        ")
        SQLBldr.AppendLine("            ,A1.STACKFREEKBN                                                                                                       ")
        SQLBldr.AppendLine("            ,B1.ACCOUNTSTATUSKBN2                                                                                                  ")
        SQLBldr.AppendLine("    ) A02                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A01.KEIJOYM                       = A02.KEIJOYM                                                                            ")
        SQLBldr.AppendLine("    AND coalesce(A01.TORICODE, '')          = A02.TORICODE                                                                           ")
        SQLBldr.AppendLine("    AND coalesce(A01.PAYFILINGBRANCH, '')   = A02.PAYFILINGBRANCH                                                                    ")
        SQLBldr.AppendLine("    AND coalesce(A01.PAYKEIJYOBRANCHCD, '') = A02.PAYKEIJYOBRANCHCD                                                                  ")
        SQLBldr.AppendLine("    AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)                                                                      ")
        SQLBldr.AppendLine("    AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)                                                                      ")
        SQLBldr.AppendLine("    AND coalesce(A01.BIGCTNCD, '')          = A02.BIGCTNCD                                                                           ")
        SQLBldr.AppendLine("    AND coalesce(A01.MIDDLECTNCD, '')       = A02.MIDDLECTNCD                                                                        ")
        SQLBldr.AppendLine("    AND (   A02.SUM_FREESENDFEE <> 0                                                                                               ")
        SQLBldr.AppendLine("         OR A02.SUM_SHIPFEE     <> 0                                                                                               ")
        SQLBldr.AppendLine("         OR A02.SUM_OTHER1FEE   <> 0                                                                                               ")
        SQLBldr.AppendLine("        )                                                                                                                          ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        com.LNS0020_STATION A04                                                                                                    ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A04.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A04.STATION  = A01.DEPSTATION                                                                                              ")
        SQLBldr.AppendLine("    AND A04.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        com.LNS0020_STATION A05                                                                                                    ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A05.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A05.STATION  = A01.ARRSTATION                                                                                              ")
        SQLBldr.AppendLine("    AND A05.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0022_CLASS A06                                                                                                      ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A06.BIGCTNCD    = A01.BIGCTNCD                                                                                             ")
        SQLBldr.AppendLine("    AND A06.MIDDLECTNCD = A01.MIDDLECTNCD                                                                                          ")
        SQLBldr.AppendLine("    AND A06.SMALLCTNCD  = A01.SMALLCTNCD                                                                                           ")
        SQLBldr.AppendLine("    AND A06.DELFLG      = @P01                                                                                                     ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A07                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A07.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A07.ORGCODE  = A01.PAYFILINGBRANCH                                                                                         ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A07.STYMD AND A07.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A07.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A08                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A08.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A08.ORGCODE  = A01.PAYKEIJYOBRANCHCD                                                                                       ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A08.STYMD AND A08.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A08.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN(                                                                                                                     ")
        SQLBldr.AppendLine("        SELECT TOP(1)                                                                                                              ")
        SQLBldr.AppendLine("            T1.USERID                                                                                                              ")
        SQLBldr.AppendLine("           ,T1.STAFFNAMES                                                                                                          ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            com.lns0001_user T1                                                                                                    ")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            T1.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                             ")
        SQLBldr.AppendLine("        AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN T1.STYMD AND T1.ENDYMD                              ")
        SQLBldr.AppendLine("        AND T1.DELFLG  = @P01                                                                                                      ")
        SQLBldr.AppendLine("        ORDER BY                                                                                                                   ")
        SQLBldr.AppendLine("            T1.STYMD DESC                                                                                                          ")
        SQLBldr.AppendLine("    ) A09                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A09.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                                ")
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10                                                                                              ")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE                                                                                             ")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0'                                                                                                       ")
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11                                                                                     ")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM                                                                                             ")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0'                                                                                                       ")
        SQLBldr.AppendLine("    WHERE                                                                                                                          ")
        SQLBldr.AppendLine("        A01.KEIJOYM           = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'                                       ")
        SQLBldr.AppendLine("    AND A01.TORICODE          = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'                                        ")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'                                  ")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT   = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'                                 ")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN      = '2'                                                                                                ")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 0")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        SQLBldr.AppendLine("UNION ALL")
        SQLBldr.AppendLine("SELECT DISTINCT")
        SQLBldr.AppendLine("    '2' AS SORTNO")
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH + '-01-' + A01.TORICODE + '-1-' + FORMAT(CURDATE(), 'MM') + '-30' AS 主キー")           '主キー
        SQLBldr.AppendLine("    , FORMAT(CURDATE(), 'yyyy/MM/dd') AS 支払書発行年月日")                                                                 '支払書発行年月日
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy/MM') + '/01' AS 費用計上日付")                                                              '費用計上日付
        SQLBldr.AppendLine("    ,CASE WHEN A11.SCHEDATEPAYMENT IS NULL THEN A01.SCHEDATEPAYMENT")                                                                               '支払予定日
        SQLBldr.AppendLine("	      ELSE A11.SCHEDATEPAYMENT END       AS 支払予定年月日")                                                                               '支払予定日
        If CS0050Session.ENVIRONMENTFLG = "2" Then
            SQLBldr.AppendLine("   ,'01-' + A01.TORICODE + '-1'                                                                             AS 顧客コード              ")
        Else
            SQLBldr.AppendLine("   ,'01-' + 'TestCust01' + '-1'                                                                            AS 顧客コード              ")
        End If
        SQLBldr.AppendLine("    , A01.TORICODE AS 支払先顧客選択")                                                                                      '支払先顧客選択
        SQLBldr.AppendLine("    , A10.CLIENTNAME AS 顧客名")                                                                                            '顧客名
        SQLBldr.AppendLine("    , '01-' + A01.PAYFILINGBRANCH AS 提出部店")                                                                             '提出部店
        SQLBldr.AppendLine("    , A07.NAME AS 提出部店名")                                                                                              '提出部店名
        SQLBldr.AppendLine("    , '01-' + A01.PAYKEIJYOBRANCHCD AS 計上部店")                                                                           '計上部店
        SQLBldr.AppendLine("    , A08.NAME AS 計上部店名")                                                                                              '計上部店名
        SQLBldr.AppendLine("	, '30' AS 帳票種別")
        SQLBldr.AppendLine("    , NULL AS 発駅コード")
        SQLBldr.AppendLine("    , '加減額' AS 発駅名")
        SQLBldr.AppendLine("    , NULL AS 着駅コード")
        SQLBldr.AppendLine("    , NULL AS 着駅名")
        SQLBldr.AppendLine("    , NULL AS 大分類コード")
        SQLBldr.AppendLine("    , NULL AS 大分類名")
        SQLBldr.AppendLine("    , NULL AS 中分類コード")
        SQLBldr.AppendLine("    , NULL AS 回送個数")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51040101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("            CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                WHEN '35' THEN")
        SQLBldr.AppendLine("                    CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                        WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                        ELSE 'J-30206'")
        SQLBldr.AppendLine("                    END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（回送運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（除却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時運賃）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_FREESENDFEE <> 0 THEN A02.SUM_FREESENDFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("	  END AS '金額（売却時運賃）'")
        SQLBldr.AppendLine("    , 0 AS 青函付加金")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51030101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '3' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51050106'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '4' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（修理時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-72040104'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-90101'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '5' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（除却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN 'J-51110105'")
        SQLBldr.AppendLine("			     ELSE '' ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '科目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN")
        SQLBldr.AppendLine("                CASE A01.BIGCTNCD")
        SQLBldr.AppendLine("                    WHEN '05' THEN 'J-30204'")
        SQLBldr.AppendLine("                    WHEN '10' THEN 'J-30201'")
        SQLBldr.AppendLine("                    WHEN '11' THEN 'J-30203'")
        SQLBldr.AppendLine("                    WHEN '15' THEN 'J-30202'")
        SQLBldr.AppendLine("                    WHEN '20' THEN 'J-30209'")
        SQLBldr.AppendLine("                    WHEN '25' THEN 'J-30208'")
        SQLBldr.AppendLine("                    WHEN '30' THEN 'J-30205'")
        SQLBldr.AppendLine("                    WHEN '35' THEN")
        SQLBldr.AppendLine("                        CASE A01.MIDDLECTNCD")
        SQLBldr.AppendLine("                            WHEN '20' THEN 'J-30207'")
        SQLBldr.AppendLine("                            ELSE 'J-30206'")
        SQLBldr.AppendLine("                        END")
        SQLBldr.AppendLine("                END")
        SQLBldr.AppendLine("			    ELSE ''")
        SQLBldr.AppendLine("            END")
        SQLBldr.AppendLine("        ELSE ''")
        SQLBldr.AppendLine("      END AS '細目（売却時発送料）'")
        SQLBldr.AppendLine("    , CASE A02.ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("        WHEN '9' THEN")
        SQLBldr.AppendLine("		    CASE WHEN A02.SUM_SHIPFEE <> 0 THEN A02.SUM_SHIPFEE")
        SQLBldr.AppendLine("			     ELSE NULL ")
        SQLBldr.AppendLine("		    END")
        SQLBldr.AppendLine("        ELSE NULL")
        SQLBldr.AppendLine("      END AS '金額（売却時発送料）'")
        SQLBldr.AppendLine("    , A09.STAFFNAMES AS 発行担当者名")
        SQLBldr.AppendLine("    , FORMAT(A01.SHIPYMD, 'yyyy年MM月分') AS 宛名欄付記１")
        SQLBldr.AppendLine("FROM")
        'メイン [テーブル]コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        '[テーブル]コンテナ清算ファイル(サマリ)
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , coalesce(A1.TORICODE,'') AS TORICODE")
        SQLBldr.AppendLine("            , coalesce(A1.PAYFILINGBRANCH,'') AS PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , coalesce(A1.PAYKEIJYOBRANCHCD,'') AS PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , coalesce(A1.BIGCTNCD,'') AS BIGCTNCD")
        SQLBldr.AppendLine("            , coalesce(A1.MIDDLECTNCD,'') AS MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2 AS ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.FREESENDFEE,0) + coalesce(A1.COSTADJUSTFEE,0)) AS SUM_FREESENDFEE")
        SQLBldr.AppendLine("            , SUM(coalesce(A1.SHIPFEE,0) + coalesce(A1.COMMISSIONFEE,0)) AS SUM_SHIPFEE")
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF A1")
        SQLBldr.AppendLine("	    LEFT JOIN (")
        SQLBldr.AppendLine("		    SELECT")
        SQLBldr.AppendLine("			    SHIPYMD")
        SQLBldr.AppendLine("				,CTNTYPE")
        SQLBldr.AppendLine("				,CTNNO")
        SQLBldr.AppendLine("                ,SAMEDAYCNT")
        SQLBldr.AppendLine("                ,CTNLINENO")
        SQLBldr.AppendLine("                ,CASE")
        SQLBldr.AppendLine("                   WHEN ACCOUNTSTATUSKBN IN ('3', '6', '7')")
        SQLBldr.AppendLine("                     THEN 3")
        SQLBldr.AppendLine("                   WHEN ACCOUNTSTATUSKBN IN ('5', '9') THEN")
        SQLBldr.AppendLine("                     CASE ")
        SQLBldr.AppendLine("		  	       WHEN KEIJOYM < '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("                     THEN 5")
        SQLBldr.AppendLine("		  	       WHEN KEIJOYM >= '" & CONST_BAIKYAKU_DATE.ToString & "' ")
        SQLBldr.AppendLine("		  	         THEN ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("		  	       END")
        SQLBldr.AppendLine("                ELSE ACCOUNTSTATUSKBN")
        SQLBldr.AppendLine("              END AS ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("            FROM")
        SQLBldr.AppendLine("                lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("            WHERE")
        SQLBldr.AppendLine("                STACKFREEKBN = '2'")
        SQLBldr.AppendLine("                AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("                AND DELFLG = @P01")
        SQLBldr.AppendLine("                AND ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("                AND TOTALCOST <> 0")
        SQLBldr.AppendLine("	            AND DUMMYKBN = 1")
        SQLBldr.AppendLine("	        ) B1")
        SQLBldr.AppendLine("		    ON A1.SHIPYMD = B1.SHIPYMD")
        SQLBldr.AppendLine("		    AND A1.CTNTYPE = B1.CTNTYPE")
        SQLBldr.AppendLine("		    AND A1.CTNNO = B1.CTNNO")
        SQLBldr.AppendLine("		    AND A1.SAMEDAYCNT = B1.SAMEDAYCNT")
        SQLBldr.AppendLine("		    AND A1.CTNLINENO = B1.CTNLINENO")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            A1.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("            AND A1.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
        SQLBldr.AppendLine("            AND A1.DELFLG = @P01")
        SQLBldr.AppendLine("            AND A1.ACCOUNTINGASSETSKBN = '1'")
        SQLBldr.AppendLine("	        AND A1.TOTALCOST <> 0")
        SQLBldr.AppendLine("	        AND A1.DUMMYKBN = 1")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            A1.KEIJOYM")
        SQLBldr.AppendLine("            , A1.TORICODE")
        SQLBldr.AppendLine("            , A1.PAYFILINGBRANCH")
        SQLBldr.AppendLine("            , A1.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("            , A1.DEPSTATION")
        SQLBldr.AppendLine("            , A1.ARRSTATION")
        SQLBldr.AppendLine("            , A1.BIGCTNCD")
        SQLBldr.AppendLine("            , A1.MIDDLECTNCD")
        SQLBldr.AppendLine("            , A1.STACKFREEKBN")
        SQLBldr.AppendLine("            , B1.ACCOUNTSTATUSKBN2")
        SQLBldr.AppendLine("    ) A02")
        SQLBldr.AppendLine("        ON A01.KEIJOYM = A02.KEIJOYM")
        SQLBldr.AppendLine("        AND coalesce(A01.TORICODE,'') = A02.TORICODE")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYFILINGBRANCH,'') = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("        AND coalesce(A01.PAYKEIJYOBRANCHCD,'') = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND coalesce(A01.DEPSTATION, 0) = coalesce(A02.DEPSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.ARRSTATION, 0) = coalesce(A02.ARRSTATION, 0)")
        SQLBldr.AppendLine("        AND coalesce(A01.BIGCTNCD,'') = A02.BIGCTNCD")
        SQLBldr.AppendLine("        AND coalesce(A01.MIDDLECTNCD,'') = A02.MIDDLECTNCD")
        SQLBldr.AppendLine("		AND (A02.SUM_FREESENDFEE <> 0 OR A02.SUM_SHIPFEE <> 0)")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        com.LNS0020_STATION A04                                                                                                    ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A04.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A04.STATION  = A01.DEPSTATION                                                                                              ")
        SQLBldr.AppendLine("    AND A04.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        com.LNS0020_STATION A05                                                                                                    ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A05.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A05.STATION  = A01.ARRSTATION                                                                                              ")
        SQLBldr.AppendLine("    AND A05.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0022_CLASS A06                                                                                                      ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A06.BIGCTNCD    = A01.BIGCTNCD                                                                                             ")
        SQLBldr.AppendLine("    AND A06.MIDDLECTNCD = A01.MIDDLECTNCD                                                                                          ")
        SQLBldr.AppendLine("    AND A06.SMALLCTNCD  = A01.SMALLCTNCD                                                                                           ")
        SQLBldr.AppendLine("    AND A06.DELFLG      = @P01                                                                                                     ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A07                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A07.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A07.ORGCODE  = A01.PAYFILINGBRANCH                                                                                         ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A07.STYMD AND A07.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A07.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN                                                                                                                      ")
        SQLBldr.AppendLine("        LNG.LNM0002_ORG A08                                                                                                        ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A08.CAMPCODE = @P02                                                                                                        ")
        SQLBldr.AppendLine("    AND A08.ORGCODE  = A01.PAYKEIJYOBRANCHCD                                                                                       ")
        SQLBldr.AppendLine("    AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN A08.STYMD AND A08.ENDYMD                                ")
        SQLBldr.AppendLine("    AND A08.DELFLG   = @P01                                                                                                        ")
        SQLBldr.AppendLine("    LEFT JOIN(                                                                                                                     ")
        SQLBldr.AppendLine("        SELECT TOP(1)                                                                                                              ")
        SQLBldr.AppendLine("            T1.USERID                                                                                                              ")
        SQLBldr.AppendLine("           ,T1.STAFFNAMES                                                                                                          ")
        SQLBldr.AppendLine("        FROM                                                                                                                       ")
        SQLBldr.AppendLine("            com.lns0001_user T1                                                                                                    ")
        SQLBldr.AppendLine("        WHERE                                                                                                                      ")
        SQLBldr.AppendLine("            T1.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                             ")
        SQLBldr.AppendLine("        AND '" & htParm(DRAFTPAYMENTLINK_KEY.SL_NOWDATE).ToString & "' BETWEEN T1.STYMD AND T1.ENDYMD                              ")
        SQLBldr.AppendLine("        AND T1.DELFLG  = @P01                                                                                                      ")
        SQLBldr.AppendLine("        ORDER BY                                                                                                                   ")
        SQLBldr.AppendLine("            T1.STYMD DESC                                                                                                          ")
        SQLBldr.AppendLine("    ) A09                                                                                                                          ")
        SQLBldr.AppendLine("    ON                                                                                                                             ")
        SQLBldr.AppendLine("        A09.USERID  = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_LOGIN_USER).ToString & "'                                                ")
        SQLBldr.AppendLine("	LEFT JOIN lng.LNT0072_PAYEE A10                                                                                              ")
        SQLBldr.AppendLine("	    ON A10.TORICODE = A01.TORICODE                                                                                             ")
        SQLBldr.AppendLine("		AND A10.DELFLG = '0'                                                                                                       ")
        SQLBldr.AppendLine("	LEFT JOIN lng.LNM0036_PAYMENTDUEDATE A11                                                                                     ")
        SQLBldr.AppendLine("	    ON A11.PAYMENTYM = A01.KEIJOYM                                                                                             ")
        SQLBldr.AppendLine("		AND A11.DELFLG = '0'                                                                                                       ")
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    A01.KEIJOYM = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTYM).ToString & "'")
        SQLBldr.AppendLine("    AND A01.TORICODE = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_TORICODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.PAYFILINGBRANCH = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_PAYMENTORGCODE).ToString & "'")
        SQLBldr.AppendLine("    AND A01.SCHEDATEPAYMENT = '" & htParm(DRAFTPAYMENTLINK_KEY.SL_SCHEDATEPAYMENT).ToString & "'")
        SQLBldr.AppendLine("    AND A01.STACKFREEKBN = '2'")
        SQLBldr.AppendLine("	AND A01.DUMMYKBN = 1")
        SQLBldr.AppendLine("	AND A01.TOTALCOST <> 0")
        SQLBldr.AppendLine(")D01                                                                                                                               ")
        SQLBldr.AppendLine("ORDER BY                                                                                                                           ")
        SQLBldr.AppendLine("     SORTNO                                                                                                                        ")
        SQLBldr.AppendLine("    ,D01.発駅コード                                                                                                                ")
        SQLBldr.AppendLine("    ,D01.着駅コード                                                                                                                ")
        SQLBldr.AppendLine("    ,D01.支払予定年月日                                                                                                            ")

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

            PARA01.Value = C_DELETE_FLG.ALIVE
            PARA02.Value = htParm(DRAFTPAYMENTLINK_KEY.SL_CAMPCODE).ToString

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

    Public Shared Function BlankToDBNull(strTarger As Object) As Object

        If strTarger Is Nothing Then
            Return CType(DBNull.Value, Object)
        ElseIf strTarger.ToString.Trim = "" Then
            Return CType(DBNull.Value, Object)
        Else
            Return strTarger
        End If

    End Function

End Class
