''************************************************************	
' 画面名称：リース登録　更新用クラス
' 作成日：2022/03/25
' 作成者：杉元　孝行
' 更新日：2024/04/11
' 更新者：杉元　孝行
'
'修正履歴：
' 2024/04/11 杉元孝行 スポットリース一括請求対応
' 2024/08/14 杉元孝行 スポットリース区分追加対応
' 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応
''************************************************************	

Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>リースデータ（ヘッダ）用のキー</description></item>
''' </list>
''' </remarks>
Public Enum LEASE_HDPRM
    HP_LEASENO             ' リース登録番号
    HP_TORICODE            ' 請求先コード(取引先コード)
    HP_INVOICESPLITKBN     ' 請求書分割区分
    HP_CONTRALNTYPE        ' 契約形態
    HP_INVOICEOUTORGCD     ' 請求書出力先組織コード
    HP_KEIJOORGCD          ' 計上先組織コード
    HP_REMODELLEASEKBN     ' 改造費リース区分
    HP_KEIJOMKBN           ' 計上月区分
    HP_CLOSINGINPUTFLG     ' 締日入力フラグ
    HP_CLOSINGDAY          ' 締日
    HP_LEASESTARTYMD       ' リース開始日
    HP_DAYCALCSTART        ' リース開始月日割計算
    HP_LEASEENDYMD         ' リース終了日
    HP_DAYCALCEND          ' リース終了月日割計算
    HP_UPDPERIOD           ' 更新期間
    HP_AUTOCALCKBN         ' 自動更新区分
    HP_MONTHLEASEFEE       ' 月額リース料
    HP_TAXKBN              ' 税区分
    HP_ROUNDKBN            ' 日割端数処理区分
    HP_UPDLEASESTARTYMD    ' 更新後リース開始日
    HP_UPDDAYCALCSTART     ' 更新後リース開始月日割計算
    HP_UPDLEASEENDYMD      ' 更新後リース終了日
    HP_UPDDAYCALCEND       ' 更新後リース終了月日割計算
    HP_UPDUPDPERIOD        ' 更新後更新期間
    HP_UPDAUTOCALCKBN      ' 更新後自動更新区分
    HP_UPDMONTHLEASEFEE    ' 更新後月額リース料
    HP_UPDTAXKBN           ' 更新後税区分
    HP_UPDROUNDKBN         ' 更新後日割端数処理区分
    HP_UPDTORICODE         ' 更新後請求先コード(取引先コード)
    HP_UPDTORICODECHGYMD   ' 取引先変更日
    HP_NOTCANCELSTARTYMD         '途中解約不能期間（開始）
    HP_NOTCANCELENDYMD           '途中解約不能期間（終了）
    HP_PURCHASEPRICE             '購入価格（1個当たり）
    HP_REMODELINGCOST            '改造費（総額）
    HP_LEASESTARTCOST            'リース開始時簿価
    HP_RVGAMOUNT    '残価保証額（1個当たり）
    HP_SURVIVALRATE              '残存率
    HP_SERVICELIFE               '耐用年数
    HP_ELAPSEDYEARS              '経過年数
    HP_DEPOSITMONTHKBN     ' 入金月区分
    HP_DEPOSITINPUTFLG     ' 入金日入力フラグ
    HP_DEPOSITDAY          ' 入金日
    HP_INACCOUNTCD         ' 社内口座コード
    HP_SLIPDESCRIPTION1    ' 伝票摘要１       
    HP_SLIPDESCRIPTION2    ' 伝票摘要２
    HP_INVKESAIKBN         ' 請求書決済区分
    HP_INVSUBCD            ' 請求書細分コード
    HP_UPDINVOICEOUTORGCD  '更新後請求書出力先組織コード
    HP_UPDKEIJOORGCD       '更新後計上先組織コード
    HP_UPDKEIJOMKBN        '更新後計上月区分
    HP_UPDCLOSINGINPUTFLG  '更新後締日入力フラグ
    HP_UPDCLOSINGDAY       '更新後締日
    HP_UPDDEPOSITMONTHKBN  '更新後入金月区分
    HP_UPDDEPOSITINPUTFLG  '更新後入金日入力フラグ
    HP_UPDDEPOSITDAY       '更新後入金日
    HP_UPDINACCOUNTCD      '更新後社内口座コード
    HP_UPDSLIPDESCRIPTION1 '更新後伝票摘要１
    HP_UPDSLIPDESCRIPTION2 '更新後伝票摘要２
    HP_UPDINVKESAIKBN      '更新後請求書決済区分
    HP_UPDINVSUBCD         '更新後請求書細分コード
    HP_DELFLG              ' 削除フラグ
    HP_INITYMD             ' 登録年月日
    HP_INITUSER            ' 登録ユーザーＩＤ
    HP_INITTERMID          ' 登録端末
    HP_INITPGID            ' 登録プログラムＩＤ
    HP_UPDYMD              ' 更新年月日
    HP_UPDUSER             ' 更新ユーザーＩＤ
    HP_UPDTERMID           ' 更新端末
    HP_UPDPGID             ' 更新プログラムＩＤ
    HP_RECEIVEYMD          ' 集信日時
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>リース適用データ用のキー</description></item>
''' </list>
''' </remarks>
Public Enum LEASE_APPLPARAM
    AP_LEASENO            ' リース登録番号
    AP_INITAPPLYSTARTYMD  ' 適用開始日(初期値)
    AP_SEQNO              ' 連番
    AP_APPLYKBN           ' 適用区分
    AP_KEIJOSTATUS        ' 計上状態
    AP_APPLYSTARTYMD      ' 適用開始日
    AP_DAYCALCSTART       ' 適用開始月日割計算
    AP_APPLYENDYMD        ' 適用終了日
    AP_DAYCALCEND         ' 適用終了月日割計算
    AP_MONTHLEASEFEE      ' 月額リース料
    AP_INFOKBN            ' 情報区分
    AP_DELFLG             ' 削除フラグ
    AP_INITYMD            ' 登録年月日
    AP_INITUSER           ' 登録ユーザーＩＤ
    AP_INITTERMID         ' 登録端末
    AP_INITPGID           ' 登録プログラムＩＤ
    AP_UPDYMD             ' 更新年月日
    AP_UPDUSER            ' 更新ユーザーＩＤ
    AP_UPDTERMID          ' 更新端末
    AP_UPDPGID            ' 更新プログラムＩＤ
    AP_RECEIVEYMD         ' 集信日時
    AP_UPDTIMSTP          ' タイムスタンプ
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>リース額データ用のキー</description></item>
''' </list>
''' </remarks>
Public Enum LEASE_FEEPARAM
    FP_LEASENO           ' リース登録番号
    FP_KEIJOYM           ' 計上年月
    FP_KEIJOKBN          ' 計上区分
    FP_USECNT            ' 使用料回数
    FP_DAILYDAYS         ' 日割日数
    FP_MONTHLEASEFEE     ' 月額リース料
    FP_CHANGELEASEFEE    ' リース額変更
    FP_INDUSTRYDISCOUNT  ' 出精値引き
    FP_DELFLG            ' 削除フラグ
    FP_INITYMD           ' 登録年月日
    FP_INITUSER          ' 登録ユーザーＩＤ
    FP_INITTERMID        ' 登録端末
    FP_INITPGID          ' 登録プログラムＩＤ
    FP_UPDYMD            ' 更新年月日
    FP_UPDUSER           ' 更新ユーザーＩＤ
    FP_UPDTERMID         ' 更新端末
    FP_UPDPGID           ' 更新プログラムＩＤ
    FP_RECEIVEYMD        ' 集信日時
    FP_UPDTIMSTP         ' タイムスタンプ
End Enum

''' <summary>
''' リース適用データ（明細画面）
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>リース明細画面データ用のキー</description></item>
''' </list>
''' </remarks>
Public Enum LEASE_DISPPARAM
    DP_LEASENO            'リース登録番号
    DP_INITTORICODE       '初回請求取引先コード
    DP_CTNTYPE            'コンテナ形式
    DP_CTNNO              'コンテナ番号
    DP_APPLYSTARTYMD      '契約開始日
    DP_CONTRALNTYPE       '契約形態
    DP_APPLYKBN           '適用区分
    DP_KEIJOSTATUS        '計上状態
    DP_LEASESTARTYMD      '全体契約開始日
    DP_LEASEENDYMD        '全体契約終了日
    DP_DAYCALCSTART       '契約開始月日割計算
    DP_APPLYENDYMD        '契約終了日
    DP_DAYCALCEND         '契約終了月日割計算
    DP_CANCELYMD          '途中解約日
    DP_DAYCALCCANCEL      '途中解約月日割計算
    DP_MONTHLEASEFEE      '月額リース料
    DP_UPDPERIOD          '更新期間
    DP_AUTOCALCKBN        '自動更新区分
    DP_TORICODE           '請求取引先コード
    DP_INVOICEOUTORGCD    '請求書出力先組織コード
    DP_KEIJOORGCD         '計上先組織コード
    DP_INVKESAIKBN        '請求書決済区分
    DP_INVSUBCD           '請求書細分コード
    DP_INACCOUNTCD        '社内口座コード
    DP_TAXCALCULATION     '税計算区分
    DP_ACCOUNTINGMONTH    '計上月区分
    DP_CLOSINGINPUTFLG    '締日入力フラグ
    DP_CLOSINGDAY         '計上締日
    DP_DEPOSITINPUTFLG    '入金日入力フラグ
    DP_DEPOSITDAY         '入金日
    DP_DEPOSITMONTHKBN    '入金月区分
    DP_SLIPDESCRIPTION1   '伝票摘要１
    DP_SLIPDESCRIPTION2   '伝票摘要２
    DP_TAXKBN             '税区分
    DP_TAXRATE            '税率
    DP_ROUNDKBN           '日割端数処理区分
    DP_REMODELLEASEKBN    '改造費リース区分
    DP_NOTCANCELSTARTYMD  '途中解約不能期間（開始）
    DP_NOTCANCELENDYMD    '途中解約不能期間（終了）
    DP_INFOKBN            '情報区分
    'ファイナルリース用
    DP_PURCHASEPRICE      '購入価格（1個当たり）
    DP_REMODELINGCOST     '改造費（総額）
    DP_LEASESTARTCOST     'リース開始時簿価
    DP_RVGAMOUNT          '残価保証額（1個当たり）
    DP_SURVIVALRATE       '残存率
    DP_SERVICELIFE        '耐用年数
    DP_ELAPSEDYEARS       '経過年数
    DP_RESIDUALPRICE      '残存価格
    DP_MONTHNUM           '月数
    DP_INITRESIDUAL       '初回残存簿価
    DP_COLLECTPLAN        'リース回収予定額
    DP_INTERESTRATE       '利率
    DP_PRESENTVALUE       '現在価値
    DP_PRESENTVALUERATIO  '現在価値割合
    DP_ECONOSERVICELIFE   '経済的耐用年数
    DP_SERVICELIFERATIO   '耐用年数割合
    DP_PURCHASEPRICERATIO '購入価格割合
    DP_LEASETOTALJAGE     'リース会計判定区分
    DP_BULKBILLINGFLG          '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
    DP_SPOTLEASEKBN            'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
    DP_CLOSINGDAYKBN           '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
    DP_BEFORETORICODE          '変更前請求取引先コード
    DP_BEFOREINVOICEOUTORGCD   '変更前請求書出力先組織コード
    DP_BEFORECONTRALNTYPE      '変更前契約形態
    DP_BEFOREKEIJOORGCD        '変更前計上先組織コード
    DP_BEFOREREMODELLEASEKBN   '変更前改造費リース区分
    DP_BEFOREINVKESAIKBN       '変更前請求書決済区分
    DP_BEFOREINVSUBCD          '変更前請求書細分
    DP_BEFOREBULKBILLINGFLG    '変更前一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
    DP_BEFORESPOTLEASEKBN      '変更前スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
    DP_BEFORECLOSINGDAYKBN     '変更前締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
    DP_DELFLG             '削除フラグ
    DP_INITYMD            '登録年月日
    DP_INITUSER           '登録ユーザーＩＤ
    DP_INITTERMID         '登録端末
    DP_INITPGID           '登録プログラムＩＤ
    DP_UPDYMD             '更新年月日
    DP_UPDUSER            '更新ユーザーＩＤ
    DP_UPDTERMID          '更新端末
    DP_UPDPGID            '更新プログラムＩＤ
    DP_RECEIVEYMD         '集信日時
    DP_DISPLINENO         '明細行
End Enum

''' <summary>
''' リースデータテーブル登録クラス
''' </summary>
''' <remarks>各種リースデータテーブルに登録する際はこちらに定義</remarks>
Public Class EntryLeaseData

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
    ''' リース登録番号新規採番処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <remarks>リース登録番号を採番する</remarks>
    Public Shared Function GetNewLeaseNo(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable()
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim strLeaseNo As String = ""

        With sqlText
            .AppendLine("SELECT")
            .AppendLine("    FORMAT(CURDATE(),'yyyyMMdd') + FORMAT(NEXT VALUE FOR LNG.lease_sequence,'000') AS LEASENO")
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            strLeaseNo = GetStringValue(sqlRetSet, 0, "LEASENO")
        End If

        Return strLeaseNo

    End Function

    ''' <summary>
    ''' リース適用データ 最新データ取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmToricode">取引先コード</param>
    ''' <param name="prmApplyStartYMD">契約開始日</param>
    ''' <param name="refLeaseNo">リース番号</param>
    ''' <remarks>ース適用データの最新データを取得する</remarks>
    Public Shared Sub GetLeaseApplyLeaseNo(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmToricode As String, ByVal prmApplyStartYMD As String,
                                           ByRef refLeaseNo As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        refLeaseNo = ""

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    LEASENO")
            .AppendLine("FROM")
            'メイン リース適用データ
            .AppendLine("     LNG.LNT0041_LEASEAPPLY")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     TORICODE = @TORICODE")
            .AppendLine(" AND APPLYSTARTYMD = @APPLYSTARTYMD")
            '並び順
            .AppendLine(" ORDER BY")
            .AppendLine("     LEASENO DESC")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@TORICODE", prmToricode)
            .Add("@APPLYSTARTYMD", prmApplyStartYMD)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refLeaseNo = GetStringValue(sqlRetSet, 0, "LEASENO")
        End If

    End Sub

    ''' <summary>
    ''' リース適用データ キー件数取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmLeaseNo">リース登録番号</param>
    ''' <param name="prmCtntype">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="prmApplyStartYMD">契約開始日</param>
    ''' <param name="refCnt">取得件数</param>
    ''' <remarks>ース適用データの最新データを取得する</remarks>
    Public Shared Sub GetLeaseApplyKey(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmLeaseNo As String,
                                           ByVal prmCtntype As String, ByVal prmCtnNo As String,
                                           ByVal prmApplyStartYMD As String,
                                           ByRef refCnt As Integer)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        refCnt = 0

        With sqlText
            .AppendLine("SELECT ")
            .AppendLine("    COUNT(LEASENO) AS CNT")
            .AppendLine("FROM")
            'メイン リース適用データ
            .AppendLine("     LNG.LNT0041_LEASEAPPLY")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     LEASENO = @LEASENO")
            .AppendLine(" AND CTNTYPE = @CTNTYPE")
            .AppendLine(" AND CTNNO = @CTNNO")
            .AppendLine(" AND APPLYSTARTYMD = @APPLYSTARTYMD")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@LEASENO", prmLeaseNo)
            .Add("@CTNTYPE", prmCtntype)
            .Add("@CTNNO", prmCtnNo)
            .Add("@APPLYSTARTYMD", prmApplyStartYMD)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refCnt = CInt(GetStringValue(sqlRetSet, 0, "CNT"))
        End If

    End Sub

    ''' <summary>
    ''' リース適用データ 最新データ取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmLeaseNo">リース登録番号</param>
    ''' <param name="refInitApplyStartYMD">適用開始日初期値</param>
    ''' <param name="refSeqNo">連番</param>
    ''' <param name="refApplyStartYMD">適用開始日</param>
    ''' <param name="refApplyEndYMD">適用終了日</param>
    ''' <remarks>リース明細画面データの最新データを取得する</remarks>
    Public Shared Sub GetLeaseApplyNewData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmLeaseNo As String,
                                           ByRef refInitApplyStartYMD As String, ByRef refSeqNo As String,
                                           ByRef refApplyStartYMD As String, ByRef refApplyEndYMD As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    INITAPPLYSTARTYMD, SEQNO, APPLYSTARTYMD, APPLYENDYMD")
            .AppendLine("FROM")
            'メイン リース適用データ
            .AppendLine("     LNG.LNT0041_LEASEAPPLY")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     LEASENO = @LEASENO")
            '並び順
            .AppendLine(" ORDER BY")
            .AppendLine("     SEQNO DESC")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@LEASENO", prmLeaseNo)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refInitApplyStartYMD = CmnSetFmt.YYYYMMDDToStr(sqlRetSet.Rows(0)("INITAPPLYSTARTYMD"))
            refSeqNo = GetStringValue(sqlRetSet, 0, "SEQNO")
            refApplyStartYMD = CmnSetFmt.YYYYMMDDToStr(sqlRetSet.Rows(0)("APPLYSTARTYMD"))
            refApplyEndYMD = CmnSetFmt.YYYYMMDDToStr(sqlRetSet.Rows(0)("APPLYENDYMD"))
        End If

    End Sub

    ''' <summary>
    ''' リース明細データ 計上状態取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmLeaseNo">リース登録番号</param>
    ''' <remarks>リース明細画面データの最新データを取得する</remarks>
    Public Shared Function GetLeaseKeijoStatus(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmLeaseNo As String) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim strKeijoStatus As String = ""
        Dim blnKeijo As Boolean = False
        Dim blnNoKeijo As Boolean = False

        With sqlText
            .AppendLine("SELECT ")
            .AppendLine("    KEIJOKBN")
            .AppendLine("FROM")
            'メイン リース適用データ
            .AppendLine("     LNG.LNT0042_LEASEDATA")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     DELFLG = @DELFLG")
            .AppendLine("     AND LEASENO = @LEASENO")
            '並び順
            .AppendLine(" GROUP BY")
            .AppendLine("     KEIJOKBN")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
            .Add("@LEASENO", prmLeaseNo)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        For Each rowData As DataRow In sqlRetSet.Rows
            If rowData("KEIJOKBN").ToString = C_KEIJO_KBN.RECORDED Then
                blnKeijo = True
            Else
                blnNoKeijo = True
            End If
        Next

        If blnKeijo = False Then
            strKeijoStatus = C_LEASE_KEIJOSTATUS.NOT_RECORDED
        ElseIf blnNoKeijo = False Then
            strKeijoStatus = C_LEASE_KEIJOSTATUS.RECORDED_ALL
        Else
            strKeijoStatus = C_LEASE_KEIJOSTATUS.RECORDED_UNIT
        End If

        Return strKeijoStatus

    End Function

    ''' <summary>
    ''' リースヘッダデータ 追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>ヘッダデータを登録する</remarks>
    Public Shared Sub InsertLeaseHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htHeadData As Hashtable)

        '◯リースヘッダデータ
        Dim sqlLeaseHedaStat As New StringBuilder
        sqlLeaseHedaStat.AppendLine("INSERT INTO LNG.LNT0040_LEASEHEAD (")
        sqlLeaseHedaStat.AppendLine("    LEASENO")            'リース登録番号
        sqlLeaseHedaStat.AppendLine("  , TORICODE")           '請求先コード
        sqlLeaseHedaStat.AppendLine("  , INVOICESPLITKBN")    '請求書分割区分
        sqlLeaseHedaStat.AppendLine("  , CONTRALNTYPE")       '契約形態
        sqlLeaseHedaStat.AppendLine("  , INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlLeaseHedaStat.AppendLine("  , KEIJOORGCD")         '計上先組織コード
        sqlLeaseHedaStat.AppendLine("  , REMODELLEASEKBN")    '改造費リース区分
        sqlLeaseHedaStat.AppendLine("  , KEIJOMKBN")          '計上月区分
        sqlLeaseHedaStat.AppendLine("  , CLOSINGINPUTFLG")    '締日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , CLOSINGDAY")         '締日
        sqlLeaseHedaStat.AppendLine("  , LEASESTARTYMD")      'リース開始日
        sqlLeaseHedaStat.AppendLine("  , DAYCALCSTART")       'リース開始月日割計算
        sqlLeaseHedaStat.AppendLine("  , LEASEENDYMD")        'リース終了日
        sqlLeaseHedaStat.AppendLine("  , DAYCALCEND")         'リース終了月日割計算
        sqlLeaseHedaStat.AppendLine("  , UPDPERIOD")          '更新期間
        sqlLeaseHedaStat.AppendLine("  , AUTOCALCKBN")        '自動更新区分
        sqlLeaseHedaStat.AppendLine("  , MONTHLEASEFEE")      '月額リース料
        sqlLeaseHedaStat.AppendLine("  , TAXKBN")             '税区分
        sqlLeaseHedaStat.AppendLine("  , ROUNDKBN")           '日割端数処理区分
        sqlLeaseHedaStat.AppendLine("  , NOTCANCELSTARTYMD")      '途中解約不能期間（開始）
        sqlLeaseHedaStat.AppendLine("  , NOTCANCELENDYMD")        '途中解約不能期間（終了）
        sqlLeaseHedaStat.AppendLine("  , PURCHASEPRICE")          '購入価格（1個当たり）
        sqlLeaseHedaStat.AppendLine("  , REMODELINGCOST")         '改造費（総額）
        sqlLeaseHedaStat.AppendLine("  , LEASESTARTCOST")         'リース開始時簿価
        sqlLeaseHedaStat.AppendLine("  , RVGAMOUNT")              '残価保証額（1個当たり）
        sqlLeaseHedaStat.AppendLine("  , SURVIVALRATE")           '残存率
        sqlLeaseHedaStat.AppendLine("  , SERVICELIFE")            '耐用年数
        sqlLeaseHedaStat.AppendLine("  , ELAPSEDYEARS")           '経過年数
        sqlLeaseHedaStat.AppendLine("  , DEPOSITMONTHKBN")    '入金月区分
        sqlLeaseHedaStat.AppendLine("  , DEPOSITINPUTFLG")    '入金日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , DEPOSITDAY")         '入金日
        sqlLeaseHedaStat.AppendLine("  , INACCOUNTCD")        '社内口座コード
        sqlLeaseHedaStat.AppendLine("  , SLIPDESCRIPTION1")   '伝票摘要１
        sqlLeaseHedaStat.AppendLine("  , SLIPDESCRIPTION2")   '伝票摘要２
        sqlLeaseHedaStat.AppendLine("  , INVKESAIKBN")        '請求書決済区分
        sqlLeaseHedaStat.AppendLine("  , INVSUBCD")           '請求書細分コード
        sqlLeaseHedaStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlLeaseHedaStat.AppendLine("  , INITYMD")            '登録年月日
        sqlLeaseHedaStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlLeaseHedaStat.AppendLine("  , INITTERMID")         '登録端末
        sqlLeaseHedaStat.AppendLine("  , INITPGID")           '登録プログラムＩＤ
        sqlLeaseHedaStat.AppendLine("  , RECEIVEYMD")         '集信日時
        sqlLeaseHedaStat.AppendLine(")")
        sqlLeaseHedaStat.AppendLine(" VALUES(")
        sqlLeaseHedaStat.AppendLine("    @LEASENO")            'リース登録番号
        sqlLeaseHedaStat.AppendLine("  , @TORICODE")           '請求先コード
        sqlLeaseHedaStat.AppendLine("  , @INVOICESPLITKBN")    '請求書分割区分
        sqlLeaseHedaStat.AppendLine("  , @CONTRALNTYPE")       '契約形態
        sqlLeaseHedaStat.AppendLine("  , @INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlLeaseHedaStat.AppendLine("  , @KEIJOORGCD")         '計上先組織コード
        sqlLeaseHedaStat.AppendLine("  , @REMODELLEASEKBN")    '改造費リース区分
        sqlLeaseHedaStat.AppendLine("  , @KEIJOMKBN")          '計上月区分
        sqlLeaseHedaStat.AppendLine("  , @CLOSINGINPUTFLG")    '締日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , @CLOSINGDAY")         '締日
        sqlLeaseHedaStat.AppendLine("  , @LEASESTARTYMD")      'リース開始日
        sqlLeaseHedaStat.AppendLine("  , @DAYCALCSTART")       'リース開始月日割計算
        sqlLeaseHedaStat.AppendLine("  , @LEASEENDYMD")        'リース終了日
        sqlLeaseHedaStat.AppendLine("  , @DAYCALCEND")         'リース終了月日割計算
        sqlLeaseHedaStat.AppendLine("  , @UPDPERIOD")          '更新期間
        sqlLeaseHedaStat.AppendLine("  , @AUTOCALCKBN")        '自動更新区分
        sqlLeaseHedaStat.AppendLine("  , @MONTHLEASEFEE")      '月額リース料
        sqlLeaseHedaStat.AppendLine("  , @TAXKBN")             '税区分
        sqlLeaseHedaStat.AppendLine("  , @ROUNDKBN")           '日割端数処理区分
        sqlLeaseHedaStat.AppendLine("  , @NOTCANCELSTARTYMD")      '途中解約不能期間（開始）
        sqlLeaseHedaStat.AppendLine("  , @NOTCANCELENDYMD")        '途中解約不能期間（終了）
        sqlLeaseHedaStat.AppendLine("  , @PURCHASEPRICE")          '購入価格（1個当たり）
        sqlLeaseHedaStat.AppendLine("  , @REMODELINGCOST")         '改造費（総額）
        sqlLeaseHedaStat.AppendLine("  , @LEASESTARTCOST")         'リース開始時簿価
        sqlLeaseHedaStat.AppendLine("  , @RVGAMOUNT")              '残価保証額（1個当たり）
        sqlLeaseHedaStat.AppendLine("  , @SURVIVALRATE")           '残存率
        sqlLeaseHedaStat.AppendLine("  , @SERVICELIFE")            '耐用年数
        sqlLeaseHedaStat.AppendLine("  , @ELAPSEDYEARS")           '経過年数
        sqlLeaseHedaStat.AppendLine("  , @DEPOSITMONTHKBN")    '入金月区分
        sqlLeaseHedaStat.AppendLine("  , @DEPOSITINPUTFLG")    '入金日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , @DEPOSITDAY")         '入金日
        sqlLeaseHedaStat.AppendLine("  , @INACCOUNTCD")        '社内口座コード
        sqlLeaseHedaStat.AppendLine("  , @SLIPDESCRIPTION1")   '伝票摘要１
        sqlLeaseHedaStat.AppendLine("  , @SLIPDESCRIPTION2")   '伝票摘要２
        sqlLeaseHedaStat.AppendLine("  , @INVKESAIKBN")        '請求書決済区分
        sqlLeaseHedaStat.AppendLine("  , @INVSUBCD")           '請求書細分コード
        sqlLeaseHedaStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlLeaseHedaStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlLeaseHedaStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlLeaseHedaStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlLeaseHedaStat.AppendLine("  , @INITPGID")           '登録プログラムＩＤ
        sqlLeaseHedaStat.AppendLine("  , @RECEIVEYMD")         '集信日時
        sqlLeaseHedaStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlLeaseHedaStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htHeadData(LEASE_HDPRM.HP_LEASENO)    'リース登録番号
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_TORICODE))                 '請求先コード
                .Add("INVOICESPLITKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INVOICESPLITKBN))   '請求書分割区分
                .Add("CONTRALNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_CONTRALNTYPE))         '契約形態
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INVOICEOUTORGCD))   '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_KEIJOORGCD))             '計上先組織コード
                .Add("REMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_REMODELLEASEKBN))        '改造費リース区分
                .Add("KEIJOMKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_KEIJOMKBN))               '計上月区分
                .Add("CLOSINGINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_CLOSINGINPUTFLG))   '締日入力フラグ
                .Add("CLOSINGDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_CLOSINGDAY))                  '締日
                .Add("LEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_LEASESTARTYMD))       'リース開始日
                .Add("DAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DAYCALCSTART))         'リース開始月日割計算
                .Add("LEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_LEASEENDYMD))           'リース終了日
                .Add("DAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DAYCALCEND))             'リース終了月日割計算
                .Add("UPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDPERIOD))                    '更新期間
                .Add("AUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_AUTOCALCKBN))           '自動更新区分
                .Add("MONTHLEASEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_MONTHLEASEFEE))            '月額リース料
                .Add("TAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_TAXKBN))                     '税区分
                .Add("ROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_ROUNDKBN))                      '日割端数処理区分
                .Add("NOTCANCELSTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_NOTCANCELSTARTYMD))    '途中解約不能期間（開始）
                .Add("NOTCANCELENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_NOTCANCELENDYMD))   '途中解約不能期間（終了）
                .Add("PURCHASEPRICE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_PURCHASEPRICE))            '購入価格（1個当たり）
                .Add("REMODELINGCOST", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_REMODELINGCOST))          '改造費（総額）
                .Add("LEASESTARTCOST", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_LEASESTARTCOST))          'リース開始時簿価
                .Add("RVGAMOUNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_RVGAMOUNT))                    '残価保証額（1個当たり）
                .Add("SURVIVALRATE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_SURVIVALRATE))         '残存率
                .Add("SERVICELIFE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_SERVICELIFE))                '耐用年数
                .Add("ELAPSEDYEARS", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_ELAPSEDYEARS))              '経過年数
                .Add("DEPOSITMONTHKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DEPOSITMONTHKBN))        '入金月区分
                .Add("DEPOSITINPUTFLG", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DEPOSITINPUTFLG))        '入金日入力フラグ
                .Add("DEPOSITDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DEPOSITDAY))                  '入金日
                .Add("INACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INACCOUNTCD))           '社内口座コード
                .Add("SLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_SLIPDESCRIPTION1)) '伝票摘要１
                .Add("SLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_SLIPDESCRIPTION2)) '伝票摘要２
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INVKESAIKBN))                '請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INVSUBCD))                      '請求書細分コード
                .Add("DELFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_DELFLG))                     '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INITYMD))                   '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INITUSER))                 '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INITTERMID))             '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INITPGID))                 '登録プログラムＩＤ
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_RECEIVEYMD))             '集信日時
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リースヘッダデータ 更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateLeaseHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlLeaseHedaStat As New StringBuilder
        sqlLeaseHedaStat.AppendLine("UPDATE LNG.LNT0040_LEASEHEAD ")
        sqlLeaseHedaStat.AppendLine("SET")
        sqlLeaseHedaStat.AppendLine("    INVOICEOUTORGCD = @INVOICEOUTORGCD")          '請求書出力先組織コード
        sqlLeaseHedaStat.AppendLine("  , KEIJOORGCD = @KEIJOORGCD")                    '計上先組織コード
        sqlLeaseHedaStat.AppendLine("  , REMODELLEASEKBN = @REMODELLEASEKBN")          '改造費リース区分
        sqlLeaseHedaStat.AppendLine("  , UPDLEASESTARTYMD = @UPDLEASESTARTYMD")        '更新後リース開始日
        sqlLeaseHedaStat.AppendLine("  , UPDDAYCALCSTART = @UPDDAYCALCSTART")          '更新後リース開始月日割計算
        sqlLeaseHedaStat.AppendLine("  , UPDLEASEENDYMD = @UPDLEASEENDYMD")            '更新後リース終了日
        sqlLeaseHedaStat.AppendLine("  , UPDDAYCALCEND = @UPDDAYCALCEND")              '更新後リース終了月日割計算
        sqlLeaseHedaStat.AppendLine("  , UPDUPDPERIOD = @UPDUPDPERIOD")                '更新後更新期間
        sqlLeaseHedaStat.AppendLine("  , UPDAUTOCALCKBN = @UPDAUTOCALCKBN")            '更新後自動更新区分
        sqlLeaseHedaStat.AppendLine("  , UPDMONTHLEASEFEE = @UPDMONTHLEASEFEE")        '更新後月額リース料
        sqlLeaseHedaStat.AppendLine("  , UPDTAXKBN = @UPDTAXKBN")                      '更新後税区分
        sqlLeaseHedaStat.AppendLine("  , UPDROUNDKBN = @UPDROUNDKBN")                  '更新後日割端数処理区分
        sqlLeaseHedaStat.AppendLine("  , UPDTORICODE = @UPDTORICODE")                  '更新後請求取引先コード
        sqlLeaseHedaStat.AppendLine("  , UPDTORICODECHGYMD = @UPDTORICODECHGYMD")      '取引先変更日
        sqlLeaseHedaStat.AppendLine("  , UPDYMD = @UPDYMD")                            '更新年月日
        sqlLeaseHedaStat.AppendLine("  , UPDUSER = @UPDUSER")                          '更新ユーザーＩＤ
        sqlLeaseHedaStat.AppendLine("  , UPDTERMID = @UPDTERMID")                      '更新端末
        sqlLeaseHedaStat.AppendLine("  , UPDPGID = @UPDPGID")                          '更新プログラムＩＤ
        sqlLeaseHedaStat.AppendLine("WHERE")
        sqlLeaseHedaStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号

        Using sqlOrderCmd As New MySqlCommand(sqlLeaseHedaStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htHeadData(LEASE_HDPRM.HP_LEASENO)                                  'リース登録番号
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_INVOICEOUTORGCD))   '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_KEIJOORGCD))             '計上先組織コード
                .Add("REMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_REMODELLEASEKBN))        '改造費リース区分
                .Add("UPDLEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDLEASESTARTYMD)) '更新後リース開始日
                .Add("UPDDAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDAYCALCSTART))   '更新後リース開始月日割計算
                .Add("UPDLEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDLEASEENDYMD))     '更新後リース終了日
                .Add("UPDDAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDAYCALCEND))       '更新後リース終了月日割計算
                .Add("UPDUPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDUPDPERIOD))              '更新後更新期間
                .Add("UPDAUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDAUTOCALCKBN))     '更新後自動更新区分
                .Add("UPDMONTHLEASEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDMONTHLEASEFEE))      '更新後月額リース料
                .Add("UPDTAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTAXKBN))               '更新後税区分
                .Add("UPDROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDROUNDKBN))                '更新後日割端数処理区分
                .Add("UPDTORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTORICODE))           '更新後請求取引先コード
                .Add("UPDTORICODECHGYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTORICODECHGYMD))   '取引先変更開始日
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDYMD))                     '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDUSER))                   '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTERMID))               '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDPGID))                   '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リースヘッダデータ 更新処理(請求先変更用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateLeaseHeadSeikyu(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlLeaseHedaStat As New StringBuilder
        sqlLeaseHedaStat.AppendLine("UPDATE LNG.LNT0040_LEASEHEAD ")
        sqlLeaseHedaStat.AppendLine("SET")
        sqlLeaseHedaStat.AppendLine("    UPDTORICODE         = @UPDTORICODE")            '更新後請求取引先コード
        sqlLeaseHedaStat.AppendLine("  , UPDTORICODECHGYMD   = @UPDTORICODECHGYMD")      '取引先変更日
        sqlLeaseHedaStat.AppendLine("  , UPDINVOICEOUTORGCD  = @UPDINVOICEOUTORGCD")     '更新後請求書出力先組織コード
        sqlLeaseHedaStat.AppendLine("  , UPDKEIJOORGCD       = @UPDKEIJOORGCD")          '更新後計上先組織コード
        sqlLeaseHedaStat.AppendLine("  , UPDKEIJOMKBN        = @UPDKEIJOMKBN")           '更新後計上月区分
        sqlLeaseHedaStat.AppendLine("  , UPDCLOSINGINPUTFLG  = @UPDCLOSINGINPUTFLG")     '更新後締日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , UPDCLOSINGDAY       = @UPDCLOSINGDAY")          '更新後締日
        sqlLeaseHedaStat.AppendLine("  , UPDDEPOSITMONTHKBN  = @UPDDEPOSITMONTHKBN")     '更新後入金月区分
        sqlLeaseHedaStat.AppendLine("  , UPDDEPOSITINPUTFLG  = @UPDDEPOSITINPUTFLG")     '更新後入金日入力フラグ
        sqlLeaseHedaStat.AppendLine("  , UPDDEPOSITDAY       = @UPDDEPOSITDAY")          '更新後入金日
        sqlLeaseHedaStat.AppendLine("  , UPDINACCOUNTCD      = @UPDINACCOUNTCD")         '更新後社内口座コード
        sqlLeaseHedaStat.AppendLine("  , UPDSLIPDESCRIPTION1 = @UPDSLIPDESCRIPTION1")    '更新後伝票摘要１
        sqlLeaseHedaStat.AppendLine("  , UPDSLIPDESCRIPTION2 = @UPDSLIPDESCRIPTION2")    '更新後伝票摘要２
        sqlLeaseHedaStat.AppendLine("  , UPDINVKESAIKBN      = @UPDINVKESAIKBN")         '更新後請求書決済区分
        sqlLeaseHedaStat.AppendLine("  , UPDINVSUBCD         = @UPDINVSUBCD")            '更新後請求書細分コード
        sqlLeaseHedaStat.AppendLine("  , UPDYMD = @UPDYMD")                            '更新年月日
        sqlLeaseHedaStat.AppendLine("  , UPDUSER = @UPDUSER")                          '更新ユーザーＩＤ
        sqlLeaseHedaStat.AppendLine("  , UPDTERMID = @UPDTERMID")                      '更新端末
        sqlLeaseHedaStat.AppendLine("  , UPDPGID = @UPDPGID")                          '更新プログラムＩＤ
        sqlLeaseHedaStat.AppendLine("WHERE")
        sqlLeaseHedaStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号

        Using sqlOrderCmd As New MySqlCommand(sqlLeaseHedaStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htHeadData(LEASE_HDPRM.HP_LEASENO)                                  'リース登録番号
                .Add("UPDTORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTORICODE))           '更新後請求取引先コード
                .Add("UPDTORICODECHGYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTORICODECHGYMD))   '取引先変更開始日
                .Add("UPDINVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDINVOICEOUTORGCD))   '更新後請求書出力先組織コード
                .Add("UPDKEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDKEIJOORGCD))             '更新後計上先組織コード
                .Add("UPDKEIJOMKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDKEIJOMKBN))               '更新後計上月区分
                .Add("UPDCLOSINGINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDCLOSINGINPUTFLG))   '更新後締日入力フラグ
                .Add("UPDCLOSINGDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDCLOSINGDAY))                  '更新後締日
                .Add("UPDDEPOSITMONTHKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDEPOSITMONTHKBN))   '更新後入金月区分
                .Add("UPDDEPOSITINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDEPOSITINPUTFLG))   '更新後入金日入力フラグ
                .Add("UPDDEPOSITDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDEPOSITDAY))                  '更新後入金日
                .Add("UPDINACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDINACCOUNTCD))           '更新後社内口座コード
                .Add("UPDSLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDSLIPDESCRIPTION1)) '更新後伝票摘要１
                .Add("UPDSLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDSLIPDESCRIPTION2)) '更新後伝票摘要２
                .Add("UPDINVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDINVKESAIKBN))                '更新後請求書決済区分
                .Add("UPDINVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDINVSUBCD))                      '更新後請求書細分コード
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDYMD))                     '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDUSER))                   '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTERMID))               '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDPGID))                   '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リースヘッダデータ 更新処理(行削除用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub DeleteLineLeaseHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlLeaseHedaStat As New StringBuilder
        sqlLeaseHedaStat.AppendLine("UPDATE LNG.LNT0040_LEASEHEAD ")
        sqlLeaseHedaStat.AppendLine("SET")
        sqlLeaseHedaStat.AppendLine("    UPDLEASESTARTYMD = @UPDLEASESTARTYMD")        '更新後リース開始日
        sqlLeaseHedaStat.AppendLine("  , UPDDAYCALCSTART = @UPDDAYCALCSTART")          '更新後リース開始月日割計算
        sqlLeaseHedaStat.AppendLine("  , UPDLEASEENDYMD = @UPDLEASEENDYMD")            '更新後リース終了日
        sqlLeaseHedaStat.AppendLine("  , UPDDAYCALCEND = @UPDDAYCALCEND")              '更新後リース終了月日割計算
        sqlLeaseHedaStat.AppendLine("  , UPDUPDPERIOD = @UPDUPDPERIOD")                '更新後更新期間
        sqlLeaseHedaStat.AppendLine("  , UPDAUTOCALCKBN = @UPDAUTOCALCKBN")            '更新後自動更新区分
        sqlLeaseHedaStat.AppendLine("  , UPDMONTHLEASEFEE = @UPDMONTHLEASEFEE")        '更新後月額リース料
        sqlLeaseHedaStat.AppendLine("  , UPDTAXKBN = @UPDTAXKBN")                      '更新後税区分
        sqlLeaseHedaStat.AppendLine("  , UPDROUNDKBN = @UPDROUNDKBN")                  '更新後日割端数処理区分
        sqlLeaseHedaStat.AppendLine("  , UPDYMD = @UPDYMD")                            '更新年月日
        sqlLeaseHedaStat.AppendLine("  , UPDUSER = @UPDUSER")                          '更新ユーザーＩＤ
        sqlLeaseHedaStat.AppendLine("  , UPDTERMID = @UPDTERMID")                      '更新端末
        sqlLeaseHedaStat.AppendLine("  , UPDPGID = @UPDPGID")                          '更新プログラムＩＤ
        sqlLeaseHedaStat.AppendLine("WHERE")
        sqlLeaseHedaStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号

        Using sqlOrderCmd As New MySqlCommand(sqlLeaseHedaStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htHeadData(LEASE_HDPRM.HP_LEASENO)  'リース登録番号
                .Add("UPDLEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDLEASESTARTYMD)) '更新後リース開始日
                .Add("UPDDAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDAYCALCSTART))   '更新後リース開始月日割計算
                .Add("UPDLEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDLEASEENDYMD))     '更新後リース終了日
                .Add("UPDDAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDDAYCALCEND))       '更新後リース終了月日割計算
                .Add("UPDUPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDUPDPERIOD))              '更新後更新期間
                .Add("UPDAUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDAUTOCALCKBN))     '更新後自動更新区分
                .Add("UPDMONTHLEASEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDMONTHLEASEFEE))      '更新後月額リース料
                .Add("UPDTAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTAXKBN))               '更新後税区分
                .Add("UPDROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDROUNDKBN))                '更新後日割端数処理区分
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDYMD))                     '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDUSER))                   '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDTERMID))               '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htHeadData(LEASE_HDPRM.HP_UPDPGID))                   '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リース適用データ 行削除処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htApplData">リース適用データ</param>
    ''' <remarks>リース適用データを更新する（行削除用）</remarks>
    Public Shared Sub DeleteLeaseAppl(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htApplData As Hashtable)

        '◯リース適用データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("DELETE FROM LNG.LNT0041_LEASEAPPLY ")
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")              'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")              'コンテナ記号
        sqlDetailStat.AppendLine("AND CTNNO = @CTNNO")                  'コンテナ番号
        sqlDetailStat.AppendLine("AND APPLYSTARTYMD = @APPLYSTARTYMD")  '契約開始日

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htApplData(LEASE_DISPPARAM.DP_LEASENO)    'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htApplData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ記号
                .Add("CTNNO", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htApplData(LEASE_DISPPARAM.DP_CTNNO))     'コンテナ番号
                .Add("APPLYSTARTYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htApplData(LEASE_DISPPARAM.DP_APPLYSTARTYMD)) '契約開始日
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リース明細画面データ 件数取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">リース明細画面データ</param>
    ''' <remarks>リース明細画面データの件数を取得する</remarks>
    Public Shared Function GetCountLeaseDispData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                                 ByVal htWKData As Hashtable) As Integer
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        With sqlText
            .AppendLine("SELECT")
            .AppendLine("    COUNT(*) CNT")
            .AppendLine("FROM")
            'メイン リース明細画面データ
            .AppendLine("     LNG.LNT0057_LEASEDISPDATA")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     LEASENO = @LEASENO")
            .AppendLine("     AND CTNTYPE = @CTNTYPE")
            .AppendLine("     AND CTNNO   = @CTNNO")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@LEASENO", htWKData(LEASE_DISPPARAM.DP_LEASENO))
            .Add("@CTNTYPE", htWKData(LEASE_DISPPARAM.DP_CTNTYPE))
            .Add("@CTNNO", htWKData(LEASE_DISPPARAM.DP_CTNNO))
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            intLeaseCnt = CInt(GetStringValue(sqlRetSet, 0, "CNT"))
        End If

        Return intLeaseCnt

    End Function

    ''' <summary>
    ''' リース明細画面データ一時ファイル 削除処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <remarks>リース明細データ(一時ファイル)を削除する</remarks>
    Public Shared Sub DeleteLeaseDispWKData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                            ByVal strUserID As String)

        '◯リース明細画面データ一時ファイル
        Dim sqlHistryStat As New StringBuilder
        sqlHistryStat.AppendLine("DELETE")
        sqlHistryStat.AppendLine("    LNG.LNT0058_LEASEDISPDATA_WK")
        sqlHistryStat.AppendLine("WHERE")
        sqlHistryStat.AppendLine("    USERID = @USERID")

        Using sqlWKCmd As New MySqlCommand(sqlHistryStat.ToString, sqlCon, sqlTran)
            With sqlWKCmd.Parameters
                .Add("USERID", MySqlDbType.VarChar).Value = strUserID   'リース登録番号
            End With
            sqlWKCmd.CommandTimeout = 300
            sqlWKCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リース明細画面データ一時ファイル 登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">画面の明細データ</param>
    ''' <remarks>リース明細画面データ一時ファイルを登録する</remarks>
    Public Shared Sub InsertLeaseDispWKData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                            ByVal htWKData As Hashtable)

        '◯リース明細画面データ一時ファイル
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO LNG.LNT0058_LEASEDISPDATA_WK (")
        sqlDetailStat.AppendLine("    USERID")             'ユーザID
        sqlDetailStat.AppendLine("  , LEASENO")            'リース登録番号
        sqlDetailStat.AppendLine("  , CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , APPLYSTARTYMD")      '契約開始日
        sqlDetailStat.AppendLine("  , CONTRALNTYPE")       '契約形態
        sqlDetailStat.AppendLine("  , APPLYKBN")           '適用区分
        sqlDetailStat.AppendLine("  , KEIJOSTATUS")        '計上状態
        sqlDetailStat.AppendLine("  , LEASESTARTYMD")      '全体契約開始日
        sqlDetailStat.AppendLine("  , LEASEENDYMD")        '全体契約終了日
        sqlDetailStat.AppendLine("  , APPLYENDYMD")        '契約終了日
        sqlDetailStat.AppendLine("  , CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , BULKBILLINGFLG")     '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , SPOTLEASEKBN")       'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , CLOSINGDAYKBN")      '締日区分   2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        sqlDetailStat.AppendLine("  , BEFORETORICODE")          '変更前請求取引先コード
        sqlDetailStat.AppendLine("  , BEFOREINVOICEOUTORGCD")   '変更前請求書出力先組織コード
        sqlDetailStat.AppendLine("  , BEFORECONTRALNTYPE")      '変更前契約形態
        sqlDetailStat.AppendLine("  , BEFOREKEIJOORGCD")        '変更前計上先組織コード
        sqlDetailStat.AppendLine("  , BEFOREREMODELLEASEKBN")   '変更前改造費リース区分
        sqlDetailStat.AppendLine("  , BEFOREINVKESAIKBN")       '変更前請求書決済区分
        sqlDetailStat.AppendLine("  , BEFOREINVSUBCD")          '変更前請求書細分コード
        sqlDetailStat.AppendLine("  , BEFOREBULKBILLINGFLG")    '変更前一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , BEFORESPOTLEASEKBN")      '変更前スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , BEFORECLOSINGDAYKBN")     '変更前締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        sqlDetailStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , DISPLINENO")         '画面行
        sqlDetailStat.AppendLine("  , INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine("  , RECEIVEYMD")         '集信日時
        sqlDetailStat.AppendLine(")")
        sqlDetailStat.AppendLine(" VALUES(")
        sqlDetailStat.AppendLine("    @USERID")             'ユーザID
        sqlDetailStat.AppendLine("  , @LEASENO")            'リース登録番号
        sqlDetailStat.AppendLine("  , @CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , @CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , @APPLYSTARTYMD")      '契約開始日
        sqlDetailStat.AppendLine("  , @CONTRALNTYPE")       '契約形態
        sqlDetailStat.AppendLine("  , @APPLYKBN")           '適用区分
        sqlDetailStat.AppendLine("  , @KEIJOSTATUS")        '計上状態
        sqlDetailStat.AppendLine("  , @LEASESTARTYMD")      '全体契約開始日
        sqlDetailStat.AppendLine("  , @LEASEENDYMD")        '全体契約終了日
        sqlDetailStat.AppendLine("  , @APPLYENDYMD")        '契約終了日
        sqlDetailStat.AppendLine("  , @CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , @BULKBILLINGFLG")     '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , @SPOTLEASEKBN")       'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , @CLOSINGDAYKBN")      '締日区分   2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        sqlDetailStat.AppendLine("  , @BEFORETORICODE")          '変更前請求取引先コード
        sqlDetailStat.AppendLine("  , @BEFOREKEIJOORGCD")        '変更前請求書出力先組織コード
        sqlDetailStat.AppendLine("  , @BEFORECONTRALNTYPE")      '変更前契約形態
        sqlDetailStat.AppendLine("  , @BEFOREKEIJOORGCD")        '変更前計上先組織コード
        sqlDetailStat.AppendLine("  , @BEFOREREMODELLEASEKBN")   '変更前改造費リース区分
        sqlDetailStat.AppendLine("  , @BEFOREINVKESAIKBN")       '変更前請求書決済区分
        sqlDetailStat.AppendLine("  , @BEFOREINVSUBCD")          '変更前請求書細分コード
        sqlDetailStat.AppendLine("  , @BEFOREBULKBILLINGFLG")    '変更前一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , @BEFORESPOTLEASEKBN")      '変更前スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , @BEFORECLOSINGDAYKBN")     '変更前締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        sqlDetailStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , @DISPLINENO")         '画面行
        sqlDetailStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , @INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine("  , @RECEIVEYMD")         '集信日時
        sqlDetailStat.AppendLine(")")

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("USERID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_INITUSER)) 'ユーザーＩＤ
                .Add("LEASENO", MySqlDbType.VarChar).Value = htWKData(LEASE_DISPPARAM.DP_LEASENO)                        'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("APPLYSTARTYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_APPLYSTARTYMD))    '契約開始日
                .Add("CONTRALNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_CONTRALNTYPE))  '契約形態
                .Add("APPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_APPLYKBN))          '適用区分
                .Add("KEIJOSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_KEIJOSTATUS))    '計上状態
                .Add("LEASESTARTYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_LEASESTARTYMD))    '全体契約開始日
                .Add("LEASEENDYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_LEASEENDYMD))        '全体契約終了日
                .Add("APPLYENDYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_APPLYENDYMD))        '契約終了日
                .Add("CANCELYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_CANCELYMD))            '途中解約日
                .Add("BULKBILLINGFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BULKBILLINGFLG))  '一括請求フラグ 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
                .Add("SPOTLEASEKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_SPOTLEASEKBN))  'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
                .Add("CLOSINGDAYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_CLOSINGDAYKBN)) '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
                .Add("BEFORETORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFORETORICODE))  '変更前請求取引先コード
                .Add("BEFOREINVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREINVOICEOUTORGCD))  '変更前請求書出力先組織コード
                .Add("BEFORECONTRALNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFORECONTRALNTYPE))  '変更前契約形態
                .Add("BEFOREKEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREKEIJOORGCD))      '変更前計上先組織コード
                .Add("BEFOREREMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREREMODELLEASEKBN)) '変更前改造費リース区分
                .Add("BEFOREINVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREINVKESAIKBN))     '変更前請求書決済区分
                .Add("BEFOREINVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREINVSUBCD))        '変更前請求書細分コード
                .Add("BEFOREBULKBILLINGFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFOREBULKBILLINGFLG))  '変更前一括請求フラグ 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
                .Add("BEFORESPOTLEASEKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFORESPOTLEASEKBN))  '変更前スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
                .Add("BEFORECLOSINGDAYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_BEFORECLOSINGDAYKBN)) '変更前締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
                .Add("DELFLG", MySqlDbType.VarChar).Value = htWKData(LEASE_DISPPARAM.DP_DELFLG)                                     '削除フラグ
                .Add("DISPLINENO", MySqlDbType.Int32).Value = htWKData(LEASE_DISPPARAM.DP_DISPLINENO)                                  '画面行
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_INITYMD))          '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_INITUSER))        '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_INITTERMID))    '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_INITPGID))        '登録プログラムＩＤ
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htWKData(LEASE_DISPPARAM.DP_RECEIVEYMD))    '集信日時
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース適用データ 登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">画面の明細データ</param>
    ''' <remarks>リース登録画面の明細データを登録する</remarks>
    Public Shared Sub InsertLeaseAppl(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htDetailData As Hashtable)
        Dim strContType As String = ""
        strContType = htDetailData(LEASE_DISPPARAM.DP_CONTRALNTYPE).ToString
        '◯リース適用データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO LNG.LNT0041_LEASEAPPLY (")
        sqlDetailStat.AppendLine("    LEASENO")            'リース登録番号
        sqlDetailStat.AppendLine("  , INITTORICODE")       '初回請求取引先コード
        sqlDetailStat.AppendLine("  , CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , APPLYSTARTYMD")      '契約開始日
        sqlDetailStat.AppendLine("  , CONTRALNTYPE")       '契約形態
        sqlDetailStat.AppendLine("  , APPLYKBN")           '適用区分
        sqlDetailStat.AppendLine("  , KEIJOSTATUS")        '計上状態
        sqlDetailStat.AppendLine("  , LEASESTARTYMD")      '全体契約開始日
        sqlDetailStat.AppendLine("  , LEASEENDYMD")        '全体契約終了日
        sqlDetailStat.AppendLine("  , DAYCALCSTART")       '契約開始月日割計算
        sqlDetailStat.AppendLine("  , APPLYENDYMD")        '契約終了日
        sqlDetailStat.AppendLine("  , DAYCALCEND")         '契約終了月日割計算
        sqlDetailStat.AppendLine("  , CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , DAYCALCCANCEL")      '途中解約月日割計算
        sqlDetailStat.AppendLine("  , MONTHLEASEFEE")      '月額リース料
        sqlDetailStat.AppendLine("  , UPDPERIOD")          '更新期間
        sqlDetailStat.AppendLine("  , AUTOCALCKBN")        '自動更新区分
        sqlDetailStat.AppendLine("  , TORICODE")           '請求取引先コード
        sqlDetailStat.AppendLine("  , INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlDetailStat.AppendLine("  , KEIJOORGCD")         '計上先組織コード
        sqlDetailStat.AppendLine("  , REMODELLEASEKBN")    '改造費リース区分
        sqlDetailStat.AppendLine("  , INVKESAIKBN")        '請求書決済区分
        sqlDetailStat.AppendLine("  , INVSUBCD")           '請求書細分コード
        sqlDetailStat.AppendLine("  , INACCOUNTCD")        '社内口座コード
        sqlDetailStat.AppendLine("  , TAXCALCULATION")     '税計算区分
        sqlDetailStat.AppendLine("  , ACCOUNTINGMONTH")    '計上月区分
        sqlDetailStat.AppendLine("  , CLOSINGINPUTFLG")    '締日入力フラグ
        sqlDetailStat.AppendLine("  , CLOSINGDAY")         '計上締日
        sqlDetailStat.AppendLine("  , DEPOSITINPUTFLG")    '入金日入力フラグ
        sqlDetailStat.AppendLine("  , DEPOSITDAY")         '入金日
        sqlDetailStat.AppendLine("  , DEPOSITMONTHKBN")    '入金月区分
        sqlDetailStat.AppendLine("  , SLIPDESCRIPTION1")   '伝票摘要１
        sqlDetailStat.AppendLine("  , SLIPDESCRIPTION2")   '伝票摘要２
        sqlDetailStat.AppendLine("  , TAXKBN")             '税区分
        sqlDetailStat.AppendLine("  , TAXRATE")            '税率
        sqlDetailStat.AppendLine("  , ROUNDKBN")           '日割端数処理区分
        sqlDetailStat.AppendLine("  , INFOKBN")            '情報区分
        '契約形態がファイナンスの場合
        If strContType = C_LEASE_CONTRACT_TYPE.TYPE_FINANCE Then
            sqlDetailStat.AppendLine("  , NOTCANCELSTARTYMD")  '途中解約不能期間（開始）
            sqlDetailStat.AppendLine("  , NOTCANCELENDYMD")    '途中解約不能期間（終了）
            sqlDetailStat.AppendLine("  , PURCHASEPRICE")      '購入価格（1個当たり）
            sqlDetailStat.AppendLine("  , REMODELINGCOST")     '改造費（総額）
            sqlDetailStat.AppendLine("  , LEASESTARTCOST")     'リース開始時簿価
            sqlDetailStat.AppendLine("  , RVGAMOUNT")          '残価保証額（1個当たり）
            sqlDetailStat.AppendLine("  , SURVIVALRATE")       '残存率
            sqlDetailStat.AppendLine("  , SERVICELIFE")        '耐用年数
            sqlDetailStat.AppendLine("  , ELAPSEDYEARS")       '経過年数
            sqlDetailStat.AppendLine("  , RESIDUALPRICE")      '残存価格
            sqlDetailStat.AppendLine("  , MONTHNUM")           '月数
            sqlDetailStat.AppendLine("  , INITRESIDUAL")       '初回残存簿価
            sqlDetailStat.AppendLine("  , COLLECTPLAN")        'リース回収予定額
            sqlDetailStat.AppendLine("  , INTERESTRATE")       '利率
            sqlDetailStat.AppendLine("  , PRESENTVALUE")       '現在価値
            sqlDetailStat.AppendLine("  , PRESENTVALUERATIO")  '現在価値割合
            sqlDetailStat.AppendLine("  , ECONOSERVICELIFE")   '経済的耐用年数
            sqlDetailStat.AppendLine("  , SERVICELIFERATIO")   '耐用年数割合
            sqlDetailStat.AppendLine("  , PURCHASEPRICERATIO") '購入価格割合
            sqlDetailStat.AppendLine("  , LEASETOTALJAGE")     'リース会計判定区分
        End If
        sqlDetailStat.AppendLine("  , BULKBILLINGFLG")     '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , SPOTLEASEKBN")       'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , CLOSINGDAYKBN")      '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        '登録情報
        sqlDetailStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine("  , RECEIVEYMD")         '集信日時
        sqlDetailStat.AppendLine(")")
        sqlDetailStat.AppendLine(" VALUES(")
        sqlDetailStat.AppendLine("    @LEASENO")            'リース登録番号
        sqlDetailStat.AppendLine("  , @INITTORICODE")       '初回請求取引先コード
        sqlDetailStat.AppendLine("  , @CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , @CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , @APPLYSTARTYMD")      '契約開始日
        sqlDetailStat.AppendLine("  , @CONTRALNTYPE")       '契約形態
        sqlDetailStat.AppendLine("  , @APPLYKBN")           '適用区分
        sqlDetailStat.AppendLine("  , @KEIJOSTATUS")        '計上状態
        sqlDetailStat.AppendLine("  , @LEASESTARTYMD")      '全体契約開始日
        sqlDetailStat.AppendLine("  , @LEASEENDYMD")        '全体契約終了日
        sqlDetailStat.AppendLine("  , @DAYCALCSTART")       '契約開始月日割計算
        sqlDetailStat.AppendLine("  , @APPLYENDYMD")        '契約終了日
        sqlDetailStat.AppendLine("  , @DAYCALCEND")         '契約終了月日割計算
        sqlDetailStat.AppendLine("  , @CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , @DAYCALCCANCEL")      '途中解約月日割計算
        sqlDetailStat.AppendLine("  , @MONTHLEASEFEE")      '月額リース料
        sqlDetailStat.AppendLine("  , @UPDPERIOD")          '更新期間
        sqlDetailStat.AppendLine("  , @AUTOCALCKBN")        '自動更新区分
        sqlDetailStat.AppendLine("  , @TORICODE")           '請求取引先コード
        sqlDetailStat.AppendLine("  , @INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlDetailStat.AppendLine("  , @KEIJOORGCD")         '計上先組織コード
        sqlDetailStat.AppendLine("  , @REMODELLEASEKBN")    '改造費リース区分
        sqlDetailStat.AppendLine("  , @INVKESAIKBN")        '請求書決済区分
        sqlDetailStat.AppendLine("  , @INVSUBCD")           '請求書細分コード
        sqlDetailStat.AppendLine("  , @INACCOUNTCD")        '社内口座コード
        sqlDetailStat.AppendLine("  , @TAXCALCULATION")     '税計算区分
        sqlDetailStat.AppendLine("  , @ACCOUNTINGMONTH")    '計上月区分
        sqlDetailStat.AppendLine("  , @CLOSINGINPUTFLG")    '締日入力フラグ
        sqlDetailStat.AppendLine("  , @CLOSINGDAY")         '計上締日
        sqlDetailStat.AppendLine("  , @DEPOSITINPUTFLG")    '入金日入力フラグ
        sqlDetailStat.AppendLine("  , @DEPOSITDAY")         '入金日
        sqlDetailStat.AppendLine("  , @DEPOSITMONTHKBN")    '入金月区分
        sqlDetailStat.AppendLine("  , @SLIPDESCRIPTION1")   '伝票摘要１
        sqlDetailStat.AppendLine("  , @SLIPDESCRIPTION2")   '伝票摘要２
        sqlDetailStat.AppendLine("  , @TAXKBN")             '税区分
        sqlDetailStat.AppendLine("  , @TAXRATE")            '税率
        sqlDetailStat.AppendLine("  , @ROUNDKBN")           '日割端数処理区分
        sqlDetailStat.AppendLine("  , @INFOKBN")            '情報区分
        '契約形態がファイナンスの場合
        If strContType = C_LEASE_CONTRACT_TYPE.TYPE_FINANCE Then
            sqlDetailStat.AppendLine("  , @NOTCANCELSTARTYMD")  '途中解約不能期間（開始）
            sqlDetailStat.AppendLine("  , @NOTCANCELENDYMD")    '途中解約不能期間（終了）
            sqlDetailStat.AppendLine("  , @PURCHASEPRICE")      '購入価格（1個当たり）
            sqlDetailStat.AppendLine("  , @REMODELINGCOST")     '改造費（総額）
            sqlDetailStat.AppendLine("  , @LEASESTARTCOST")     'リース開始時簿価
            sqlDetailStat.AppendLine("  , @RVGAMOUNT")          '残価保証額（1個当たり）
            sqlDetailStat.AppendLine("  , @SURVIVALRATE")       '残存率
            sqlDetailStat.AppendLine("  , @SERVICELIFE")        '耐用年数
            sqlDetailStat.AppendLine("  , @ELAPSEDYEARS")       '経過年数
            sqlDetailStat.AppendLine("  , @RESIDUALPRICE")      '残存価格
            sqlDetailStat.AppendLine("  , @MONTHNUM")           '月数
            sqlDetailStat.AppendLine("  , @INITRESIDUAL")       '初回残存簿価
            sqlDetailStat.AppendLine("  , @COLLECTPLAN")        'リース回収予定額
            sqlDetailStat.AppendLine("  , @INTERESTRATE")       '利率
            sqlDetailStat.AppendLine("  , @PRESENTVALUE")       '現在価値
            sqlDetailStat.AppendLine("  , @PRESENTVALUERATIO")  '現在価値割合
            sqlDetailStat.AppendLine("  , @ECONOSERVICELIFE")   '経済的耐用年数
            sqlDetailStat.AppendLine("  , @SERVICELIFERATIO")   '耐用年数割合
            sqlDetailStat.AppendLine("  , @PURCHASEPRICERATIO") '購入価格割合
            sqlDetailStat.AppendLine("  , @LEASETOTALJAGE")     'リース会計判定区分
        End If
        sqlDetailStat.AppendLine("  , @BULKBILLINGFLG")     '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  , @SPOTLEASEKBN")       'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  , @CLOSINGDAYKBN")      '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        '登録情報
        sqlDetailStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , @INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine("  , @RECEIVEYMD")         '集信日時
        sqlDetailStat.AppendLine(")")

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)              'リース登録番号
                .Add("INITTORICODE", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_INITTORICODE)    '初回請求取引先コード
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("APPLYSTARTYMD", MySqlDbType.DateTime).Value = htDetailData(LEASE_DISPPARAM.DP_APPLYSTARTYMD) '契約開始日
                .Add("CONTRALNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CONTRALNTYPE))   '契約形態
                .Add("APPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_APPLYKBN))           '適用区分
                .Add("KEIJOSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOSTATUS))     '計上状態
                .Add("LEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASESTARTYMD)) '全体契約開始日
                .Add("LEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEENDYMD))     '全体契約終了日
                .Add("DAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCSTART))   '契約開始月日割計算
                .Add("APPLYENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_APPLYENDYMD))     '契約終了日
                .Add("DAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCEND))       '契約終了月日割計算
                .Add("CANCELYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CANCELYMD))         '途中解約日
                .Add("DAYCALCCANCEL", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCCANCEL)) '途中解約月日割計算
                .Add("MONTHLEASEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_MONTHLEASEFEE))    '月額リース料
                .Add("UPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPERIOD))          '更新期間
                .Add("AUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_AUTOCALCKBN)) '自動更新区分
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TORICODE))       '請求取引先コード
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVOICEOUTORGCD)) '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOORGCD))   '計上先組織コード
                .Add("REMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_REMODELLEASEKBN)) '改造費リース区分
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVKESAIKBN)) '請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVSUBCD))       '請求書細分コード
                .Add("INACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INACCOUNTCD)) '社内口座コード
                .Add("TAXCALCULATION", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXCALCULATION))   '税計算区分
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ACCOUNTINGMONTH)) '計上月区分
                .Add("CLOSINGINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGINPUTFLG)) '締日入力フラグ
                .Add("CLOSINGDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGDAY)) '計上締日
                .Add("DEPOSITINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITINPUTFLG)) '入金日入力フラグ
                .Add("DEPOSITDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITDAY)) '入金日
                .Add("DEPOSITMONTHKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITMONTHKBN))   '入金月区分
                .Add("SLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SLIPDESCRIPTION1)) '伝票摘要１
                .Add("SLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SLIPDESCRIPTION2)) '伝票摘要２
                .Add("TAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXKBN)) '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXRATE))    '税率
                .Add("ROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ROUNDKBN))  '日割端数処理区分
                .Add("INFOKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INFOKBN)) '情報区分
                '契約形態がファイナンスの場合
                If strContType = C_LEASE_CONTRACT_TYPE.TYPE_FINANCE Then
                    .Add("NOTCANCELSTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_NOTCANCELSTARTYMD)) '途中解約不能期間（開始）
                    .Add("NOTCANCELENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_NOTCANCELENDYMD))     '途中解約不能期間（終了）
                    .Add("PURCHASEPRICE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_PURCHASEPRICE))     '購入価格（1個当たり）
                    .Add("REMODELINGCOST", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_REMODELINGCOST))   '改造費（総額）
                    .Add("LEASESTARTCOST", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASESTARTCOST))   'リース開始時簿価
                    .Add("RVGAMOUNT", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_RVGAMOUNT))             '残価保証額（1個当たり）
                    .Add("SURVIVALRATE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SURVIVALRATE))  '残存率
                    .Add("SERVICELIFE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SERVICELIFE))         '耐用年数
                    .Add("ELAPSEDYEARS", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ELAPSEDYEARS))       '経過年数
                    .Add("RESIDUALPRICE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_RESIDUALPRICE))   '残存価格
                    .Add("MONTHNUM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_MONTHNUM))               '月数
                    .Add("INITRESIDUAL", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INITRESIDUAL))     '初回残存簿価
                    .Add("COLLECTPLAN", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_COLLECTPLAN))       'リース回収予定額
                    .Add("INTERESTRATE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INTERESTRATE))  '利率
                    .Add("PRESENTVALUE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_PRESENTVALUE))     '現在価値
                    .Add("PRESENTVALUERATIO", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_PRESENTVALUERATIO))   '現在価値割合
                    .Add("ECONOSERVICELIFE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ECONOSERVICELIFE))         '経済的耐用年数
                    .Add("SERVICELIFERATIO", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SERVICELIFERATIO))     '耐用年数割合
                    .Add("PURCHASEPRICERATIO", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_PURCHASEPRICERATIO)) '購入価格割合
                    .Add("LEASETOTALJAGE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASETOTALJAGE))        'リース会計判定区分
                End If
                .Add("BULKBILLINGFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_BULKBILLINGFLG))  '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
                .Add("SPOTLEASEKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SPOTLEASEKBN))    'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
                .Add("CLOSINGDAYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGDAYKBN))  '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
                '登録情報
                .Add("DELFLG", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_DELFLG)                                   '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INITYMD))          '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INITUSER))        '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INITTERMID))    '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INITPGID))        '登録プログラムＩＤ
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_RECEIVEYMD))    '集信日時
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース適用データ 更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">リース適用データ</param>
    ''' <remarks>リース適用データを更新する</remarks>
    Public Shared Sub UpdateLeaseAppl(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htDetailData As Hashtable)

        '◯リース適用データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0041_LEASEAPPLY ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("   CONTRALNTYPE      = @CONTRALNTYPE")       '契約形態
        'sqlDetailStat.AppendLine("  ,APPLYKBN          = @APPLYKBN")           '適用区分
        'sqlDetailStat.AppendLine("  ,KEIJOSTATUS       = @KEIJOSTATUS")        '計上状態
        sqlDetailStat.AppendLine("  ,LEASESTARTYMD     = @LEASESTARTYMD")      '全体契約開始日
        sqlDetailStat.AppendLine("  ,LEASEENDYMD       = @LEASEENDYMD")        '全体契約終了日
        sqlDetailStat.AppendLine("  ,DAYCALCSTART      = @DAYCALCSTART")       '契約開始月日割計算
        sqlDetailStat.AppendLine("  ,APPLYENDYMD       = @APPLYENDYMD")        '契約終了日
        sqlDetailStat.AppendLine("  ,DAYCALCEND        = @DAYCALCEND")         '契約終了月日割計算
        sqlDetailStat.AppendLine("  ,CANCELYMD         = @CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  ,DAYCALCCANCEL     = @DAYCALCCANCEL")      '途中解約月日割計算
        sqlDetailStat.AppendLine("  ,MONTHLEASEFEE     = @MONTHLEASEFEE")      '月額リース料
        sqlDetailStat.AppendLine("  ,UPDPERIOD         = @UPDPERIOD")          '更新期間
        sqlDetailStat.AppendLine("  ,AUTOCALCKBN       = @AUTOCALCKBN")        '自動更新区分
        sqlDetailStat.AppendLine("  ,TORICODE          = @TORICODE")           '請求取引先コード
        sqlDetailStat.AppendLine("  ,INVOICEOUTORGCD   = @INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlDetailStat.AppendLine("  ,KEIJOORGCD        = @KEIJOORGCD")         '計上先組織コード
        sqlDetailStat.AppendLine("  ,REMODELLEASEKBN   = @REMODELLEASEKBN")    '改造費リース区分
        sqlDetailStat.AppendLine("  ,INVKESAIKBN       = @INVKESAIKBN")        '請求書決済区分
        sqlDetailStat.AppendLine("  ,INVSUBCD          = @INVSUBCD")           '請求書細分コード
        sqlDetailStat.AppendLine("  ,INACCOUNTCD       = @INACCOUNTCD")        '社内口座コード
        sqlDetailStat.AppendLine("  ,TAXCALCULATION    = @TAXCALCULATION")     '税計算区分
        sqlDetailStat.AppendLine("  ,ACCOUNTINGMONTH   = @ACCOUNTINGMONTH")    '計上月区分
        sqlDetailStat.AppendLine("  ,CLOSINGINPUTFLG   = @CLOSINGINPUTFLG")    '締日入力フラグ
        sqlDetailStat.AppendLine("  ,CLOSINGDAY        = @CLOSINGDAY")         '計上締日
        sqlDetailStat.AppendLine("  ,DEPOSITINPUTFLG   = @DEPOSITINPUTFLG")    '入金日入力フラグ
        sqlDetailStat.AppendLine("  ,DEPOSITDAY        = @DEPOSITDAY")         '入金日
        sqlDetailStat.AppendLine("  ,DEPOSITMONTHKBN   = @DEPOSITMONTHKBN")    '入金月区分
        sqlDetailStat.AppendLine("  ,SLIPDESCRIPTION1  = @SLIPDESCRIPTION1")   '伝票摘要１
        sqlDetailStat.AppendLine("  ,SLIPDESCRIPTION2  = @SLIPDESCRIPTION2")   '伝票摘要２
        sqlDetailStat.AppendLine("  ,TAXKBN            = @TAXKBN")             '税区分
        sqlDetailStat.AppendLine("  ,TAXRATE           = @TAXRATE")            '税率
        sqlDetailStat.AppendLine("  ,ROUNDKBN          = @ROUNDKBN")           '日割端数処理区分
        sqlDetailStat.AppendLine("  ,INFOKBN           = @INFOKBN")            '情報区分
        sqlDetailStat.AppendLine("  ,BULKBILLINGFLG    = @BULKBILLINGFLG")     '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
        sqlDetailStat.AppendLine("  ,SPOTLEASEKBN      = @SPOTLEASEKBN")       'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
        sqlDetailStat.AppendLine("  ,CLOSINGDAYKBN     = @CLOSINGDAYKBN")      '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
        '更新情報
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")          '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")         '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")       '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID         = @UPDPGID")         '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")                  'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")                  'コンテナ記号
        sqlDetailStat.AppendLine("AND CTNNO = @CTNNO")                      'コンテナ番号
        sqlDetailStat.AppendLine("AND APPLYSTARTYMD = @APPLYSTARTYMD")      '契約開始日

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)                'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("APPLYSTARTYMD", MySqlDbType.DateTime).Value = htDetailData(LEASE_DISPPARAM.DP_APPLYSTARTYMD) '契約開始日
                .Add("CONTRALNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CONTRALNTYPE))   '契約形態
                '.Add("APPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_APPLYKBN))           '適用区分
                '.Add("KEIJOSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOSTATUS))     '計上状態
                .Add("LEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASESTARTYMD)) '全体契約開始日
                .Add("LEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEENDYMD))     '全体契約終了日
                .Add("DAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCSTART))   '契約開始月日割計算
                .Add("APPLYENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_APPLYENDYMD))     '契約終了日
                .Add("DAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCEND))       '契約終了月日割計算
                .Add("CANCELYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CANCELYMD))         '途中解約日
                .Add("DAYCALCCANCEL", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCCANCEL)) '途中解約月日割計算
                .Add("MONTHLEASEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_MONTHLEASEFEE))    '月額リース料
                .Add("UPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPERIOD))              '更新期間
                .Add("AUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_AUTOCALCKBN))     '自動更新区分
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TORICODE))           '請求取引先コード
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVOICEOUTORGCD)) '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOORGCD))      '計上先組織コード
                .Add("REMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_REMODELLEASEKBN)) '改造費リース区分
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVKESAIKBN)) '請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVSUBCD))       '請求書細分コード
                .Add("INACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INACCOUNTCD)) '社内口座コード
                .Add("TAXCALCULATION", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXCALCULATION))   '税計算区分
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ACCOUNTINGMONTH)) '計上月区分
                .Add("CLOSINGINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGINPUTFLG)) '締日入力フラグ
                .Add("CLOSINGDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGDAY)) '計上締日
                .Add("DEPOSITINPUTFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITINPUTFLG)) '入金日入力フラグ
                .Add("DEPOSITDAY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITDAY))                '入金日
                .Add("DEPOSITMONTHKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DEPOSITMONTHKBN))   '入金月区分
                .Add("SLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SLIPDESCRIPTION1)) '伝票摘要１
                .Add("SLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SLIPDESCRIPTION2)) '伝票摘要２
                .Add("TAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXKBN))   '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXRATE))      '税率
                .Add("ROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ROUNDKBN))    '日割端数処理区分
                .Add("INFOKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INFOKBN)) '情報区分
                .Add("BULKBILLINGFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_BULKBILLINGFLG))  '一括請求フラグ  2024/04/11 杉元孝行 スポットリース一括請求対応 ADD
                .Add("SPOTLEASEKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_SPOTLEASEKBN))  'スポットリース区分  2024/08/14 杉元孝行 スポットリース区分追加対応 ADD
                .Add("CLOSINGDAYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CLOSINGDAYKBN))  '締日区分  2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))                '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))              '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID))          '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))              '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' リース明細画面データ  更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">画面の明細データ</param>
    ''' <remarks>リース登録画面の明細データを更新する</remarks>
    Public Shared Sub UpdateLeaseDispData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htDetailData As Hashtable)

        '◯リース明細画面データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0057_LEASEDISPDATA ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    INVOICEOUTORGCD = @INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlDetailStat.AppendLine("  , KEIJOORGCD      = @KEIJOORGCD")         '計上先組織コード
        sqlDetailStat.AppendLine("  , LEASESTARTYMD   = @LEASESTARTYMD")      'リース開始日
        sqlDetailStat.AppendLine("  , DAYCALCSTART    = @DAYCALCSTART")       'リース開始月日割計算
        sqlDetailStat.AppendLine("  , LEASEENDYMD     = @LEASEENDYMD")        'リース終了日
        sqlDetailStat.AppendLine("  , DAYCALCEND      = @DAYCALCEND")         'リース終了月日割計算
        sqlDetailStat.AppendLine("  , UPDPERIOD       = @UPDPERIOD")          '更新期間
        sqlDetailStat.AppendLine("  , AUTOCALCKBN     = @AUTOCALCKBN")        '自動更新区分
        sqlDetailStat.AppendLine("  , MONTHLEASEFEE   = @MONTHLEASEFEE")      '月額リース料
        sqlDetailStat.AppendLine("  , TAXKBN          = @TAXKBN")             '税区分
        sqlDetailStat.AppendLine("  , ROUNDKBN        = @ROUNDKBN")           '日割端数処理区分
        sqlDetailStat.AppendLine("  , LEASEAPPLYKBN   = @LEASEAPPLYKBN")      'リース適用区分
        sqlDetailStat.AppendLine("  , CANCELYMD       = @CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , DELFLG          = @DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")             '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")            '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")          '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID         = @UPDPGID")            '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")    'コンテナ形式
        sqlDetailStat.AppendLine("AND CTNNO   = @CTNNO")      'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)                'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVOICEOUTORGCD))  '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOORGCD))    '計上先組織コード
                .Add("LEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASESTARTYMD))  'リース開始日
                .Add("DAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCSTART))    'リース開始月日割計算
                .Add("LEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEENDYMD))      'リース終了日
                .Add("DAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCEND))        'リース終了月日割計算
                .Add("UPDPERIOD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPERIOD))               '更新期間
                .Add("AUTOCALCKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_AUTOCALCKBN))      '自動更新区分
                .Add("MONTHLEASEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_MONTHLEASEFEE))       '月額リース料
                .Add("TAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXKBN))                '税区分
                .Add("ROUNDKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_ROUNDKBN))                 '日割端数処理区分
                '.Add("LEASEAPPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEAPPLYKBN))  'リース適用区分
                .Add("CANCELYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CANCELYMD))          '途中解約日
                .Add("DELFLG", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_DELFLG)                                       '削除フラグ
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))                '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))              '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID))          '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))              '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース明細画面データ  更新処理(途中解約日)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">画面の明細データ</param>
    ''' <remarks>リース登録画面の明細データを更新する</remarks>
    Public Shared Sub UpdateCanselLeaseDispData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                                ByVal htDetailData As Hashtable)

        '◯リース明細画面データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0057_LEASEDISPDATA ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    CANCELYMD       = @CANCELYMD")          '途中解約日
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")             '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")            '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")          '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID         = @UPDPGID")            '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")    'コンテナ形式
        sqlDetailStat.AppendLine("AND CTNNO   = @CTNNO")      'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)                'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("CANCELYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CANCELYMD))          '途中解約日
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))                '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))              '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID))          '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))              '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース明細画面データ  更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">画面の明細データ</param>
    ''' <remarks>リース登録画面の明細データを更新する</remarks>
    Public Shared Sub UpdateEndLeaseDispData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                             ByVal htDetailData As Hashtable)

        '◯リース明細画面データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0057_LEASEDISPDATA ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    INVOICEOUTORGCD = @INVOICEOUTORGCD")    '請求書出力先組織コード
        sqlDetailStat.AppendLine("  , KEIJOORGCD      = @KEIJOORGCD")         '計上先組織コード
        sqlDetailStat.AppendLine("  , LEASESTARTYMD   = @LEASESTARTYMD")      'リース開始日
        sqlDetailStat.AppendLine("  , DAYCALCSTART    = @DAYCALCSTART")       'リース開始月日割計算
        sqlDetailStat.AppendLine("  , LEASEENDYMD     = @LEASEENDYMD")        'リース終了日
        sqlDetailStat.AppendLine("  , DAYCALCEND      = @DAYCALCEND")         'リース終了月日割計算
        '        sqlDetailStat.AppendLine("  , LEASEAPPLYKBN   = @LEASEAPPLYKBN")      'リース適用区分
        sqlDetailStat.AppendLine("  , MONTHLEASEFEE   = @MONTHLEASEFEE")      '月額リース料
        sqlDetailStat.AppendLine("  , TAXKBN          = @TAXKBN")             '税区分
        sqlDetailStat.AppendLine("  , REMODELLEASEKBN = @REMODELLEASEKBN")    '改造費リース区分
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")             '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")            '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")          '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID         = @UPDPGID")            '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")    'コンテナ形式
        sqlDetailStat.AppendLine("AND CTNNO   = @CTNNO")      'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)                'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                .Add("INVOICEOUTORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_INVOICEOUTORGCD))  '請求書出力先組織コード
                .Add("KEIJOORGCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_KEIJOORGCD))        '計上先組織コード
                .Add("LEASESTARTYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASESTARTYMD))  'リース開始日
                .Add("DAYCALCSTART", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCSTART))    'リース開始月日割計算
                .Add("LEASEENDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEENDYMD))      'リース終了日
                .Add("DAYCALCEND", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_DAYCALCEND))        'リース終了月日割計算
                '                .Add("LEASEAPPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEAPPLYKBN))  'リース適用区分
                .Add("MONTHLEASEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_MONTHLEASEFEE))     '月額リース料
                .Add("TAXKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_TAXKBN))                '税区分
                .Add("REMODELLEASEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_REMODELLEASEKBN))   '改造費リース区分
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))                '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))              '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID))          '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))              '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース明細画面データ  更新処理(行削除用)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">画面の明細データ</param>
    ''' <remarks>リース登録画面の明細データを更新する</remarks>
    Public Shared Sub DeleteLineLeaseDispData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                             ByVal htDetailData As Hashtable)

        '◯リース明細画面データ
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0057_LEASEDISPDATA ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    LEASEAPPLYKBN   = @LEASEAPPLYKBN")      'リース適用区分
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")             '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")            '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")          '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID         = @UPDPGID")            '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    LEASENO = @LEASENO")    'リース登録番号
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")    'コンテナ形式
        sqlDetailStat.AppendLine("AND CTNNO   = @CTNNO")      'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)                'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_CTNNO))          'コンテナ番号
                '.Add("LEASEAPPLYKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_LEASEAPPLYKBN))  'リース適用区分
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))                '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))              '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID))          '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))              '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' リース削除データチェック用件数取得処理(計上済チェック)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="strLeaseNo">リース登録番号</param>
    ''' <remarks>リース明細画面データの件数を取得する</remarks>
    Public Shared Function GetCountDelChkLeaseData(ByVal sqlCon As MySqlConnection, ByVal strLeaseNo As String) As Integer
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        With sqlText
            .AppendLine("SELECT")
            .AppendLine("    COUNT(*) CNT")
            .AppendLine("FROM")
            'リース明細データ
            .AppendLine("    LNG.LNT0042_LEASEDATA")
            '抽出条件
            .AppendLine("WHERE")
            .AppendLine("    LEASENO = @LEASENO")
            .AppendLine("    AND DELFLG = @DELFLG")
            .AppendLine("    AND KEIJOKBN = @KEIJOKBN")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@LEASENO", strLeaseNo)
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
            .Add("@KEIJOKBN", C_KEIJO_KBN.RECORDED)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet)

        If sqlRetSet.Rows.Count > 0 Then
            intLeaseCnt = CInt(GetStringValue(sqlRetSet, 0, "CNT"))
        End If

        Return intLeaseCnt

    End Function

    ''' <summary>
    ''' リース明細データ 削除処理(物理削除)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">明細データ</param>
    ''' <remarks>リース明細データを物理削除する</remarks>
    Public Shared Sub DeleteLeaseData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htDetailData As Hashtable)

        '◯リース明細データ
        Dim sqlHistryStat As New StringBuilder
        sqlHistryStat.AppendLine("DELETE FROM")
        sqlHistryStat.AppendLine("    LNG.LNT0042_LEASEDATA")
        sqlHistryStat.AppendLine("WHERE")
        sqlHistryStat.AppendLine("        LEASENO = @LEASENO")  'リース登録番号
        sqlHistryStat.AppendLine("    AND CTNTYPE = @CTNTYPE")  'コンテナ記号
        sqlHistryStat.AppendLine("    AND CTNNO   = @CTNNO")    'コンテナ番号
        sqlHistryStat.AppendLine("    AND LEASEMONTHSTARTYMD >= @LEASEMONTHSTARTYMD")   '当月のリース期間 開始日
        sqlHistryStat.AppendLine("    AND KEIJOKBN = @KEIJOKBN") '計上区分

        Using sqlHistoryCmd As New MySqlCommand(sqlHistryStat.ToString, sqlCon, sqlTran)
            With sqlHistoryCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)    'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)    'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = htDetailData(LEASE_DISPPARAM.DP_CTNNO)             'コンテナ番号
                .Add("LEASEMONTHSTARTYMD", MySqlDbType.Date).Value = htDetailData(LEASE_DISPPARAM.DP_APPLYSTARTYMD) '契約開始日
                .Add("KEIJOKBN", MySqlDbType.VarChar).Value = C_KEIJO_KBN.NOT_RECORDED   '計上区分
            End With

            sqlHistoryCmd.CommandTimeout = 300
            sqlHistoryCmd.ExecuteNonQuery()
        End Using

    End Sub

    '2024/04/11 杉元孝行 スポットリース一括請求対応 ADD START
    ''' <summary>
    ''' リース一括請求明細データ 削除処理(物理削除)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">明細データ</param>
    ''' <remarks>リース明細データを物理削除する</remarks>
    Public Shared Sub DeleteLeaseDataAllInvoice(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htDetailData As Hashtable)

        '◯リース明細データ
        Dim sqlHistryStat As New StringBuilder
        sqlHistryStat.AppendLine("DELETE FROM")
        sqlHistryStat.AppendLine("    LNG.LNT0125_LEASEDATA_ALLINVOICE")
        sqlHistryStat.AppendLine("WHERE")
        sqlHistryStat.AppendLine("        LEASENO = @LEASENO")  'リース登録番号
        sqlHistryStat.AppendLine("    AND CTNTYPE = @CTNTYPE")  'コンテナ記号
        sqlHistryStat.AppendLine("    AND CTNNO   = @CTNNO")    'コンテナ番号
        sqlHistryStat.AppendLine("    AND LEASEMONTHSTARTYMD >= @LEASEMONTHSTARTYMD")   '当月のリース期間 開始日
        sqlHistryStat.AppendLine("    AND KEIJOKBN = @KEIJOKBN") '計上区分

        Using sqlHistoryCmd As New MySqlCommand(sqlHistryStat.ToString, sqlCon, sqlTran)
            With sqlHistoryCmd.Parameters
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)    'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)    'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = htDetailData(LEASE_DISPPARAM.DP_CTNNO)             'コンテナ番号
                .Add("LEASEMONTHSTARTYMD", MySqlDbType.Date).Value = htDetailData(LEASE_DISPPARAM.DP_APPLYSTARTYMD) '契約開始日
                .Add("KEIJOKBN", MySqlDbType.VarChar).Value = C_KEIJO_KBN.NOT_RECORDED   '計上区分
            End With

            sqlHistoryCmd.CommandTimeout = 300
            sqlHistoryCmd.ExecuteNonQuery()
        End Using

    End Sub
    '2024/04/11 杉元孝行 スポットリース一括請求対応 ADD END

    ''' <summary>
    ''' 請求ヘッダデータ 更新処理(変更フラグ更新)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htDetailData">明細データ</param>
    ''' <remarks>リース明細データを物理削除する</remarks>
    Public Shared Sub UpdateInvoiceHeadData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal htDetailData As Hashtable)

        '◯請求ヘッダーデータ
        Dim sqlHistryStat As New StringBuilder
        sqlHistryStat.AppendLine("UPDATE")
        sqlHistryStat.AppendLine("    LNG.LNT0064_INVOICEHEAD")
        sqlHistryStat.AppendLine("SET")
        sqlHistryStat.AppendLine("    UPDATEFLG = @CHGUPDATEFLG")   '変更有りフラグ
        sqlHistryStat.AppendLine("  , UPDYMD    = @UPDYMD")         '更新年月日
        sqlHistryStat.AppendLine("  , UPDUSER   = @UPDUSER")        '更新ユーザーＩＤ
        sqlHistryStat.AppendLine("  , UPDTERMID = @UPDTERMID")      '更新端末
        'sqlHistryStat.AppendLine("  , UPDPGID   = @UPDPGID")        '更新プログラムＩＤ
        sqlHistryStat.AppendLine("FROM")
        sqlHistryStat.AppendLine("    LNG.LNT0042_LEASEDATA AS BK01")
        sqlHistryStat.AppendLine("WHERE")
        sqlHistryStat.AppendLine("        LNG.LNT0064_INVOICEHEAD.KEIJOYM = BK01.KEIJOYM")  '請求年月
        sqlHistryStat.AppendLine("    AND LNG.LNT0064_INVOICEHEAD.TORICODE= BK01.TORICODE") '請求取引先コード
        sqlHistryStat.AppendLine("    AND LNG.LNT0064_INVOICEHEAD.INVOICEORGCODE= BK01.INVOICEOUTORGCD") '請求担当部店コード
        sqlHistryStat.AppendLine("    AND LNG.LNT0064_INVOICEHEAD.INVOICETYPE <> @INVOICETYPE")  '請求書種類
        sqlHistryStat.AppendLine("    AND LNG.LNT0064_INVOICEHEAD.DELFLG = @DELFLG")        '削除フラグ
        sqlHistryStat.AppendLine("    AND LNG.LNT0064_INVOICEHEAD.UPDATEFLG = @UPDATEFLG")  '変更フラグ
        sqlHistryStat.AppendLine("    AND LEASENO = @LEASENO")  'リース登録番号
        sqlHistryStat.AppendLine("    AND CTNTYPE = @CTNTYPE")  'コンテナ記号
        sqlHistryStat.AppendLine("    AND CTNNO   = @CTNNO")    'コンテナ番号
        sqlHistryStat.AppendLine("    AND LEASEMONTHSTARTYMD = @LEASEMONTHSTARTYMD")   '当月のリース期間 開始日
        sqlHistryStat.AppendLine("    AND KEIJOKBN = @KEIJOKBN") '計上区分

        Using sqlHistoryCmd As New MySqlCommand(sqlHistryStat.ToString, sqlCon, sqlTran)
            With sqlHistoryCmd.Parameters
                .Add("CHGUPDATEFLG", MySqlDbType.VarChar).Value = "1"   '変更有りフラグ(更新用)
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDYMD))       '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDUSER))     '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDTERMID)) '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htDetailData(LEASE_DISPPARAM.DP_UPDPGID))     '更新プログラムＩＤ
                .Add("INVOICETYPE", MySqlDbType.VarChar).Value = "2"            '請求書種類
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "0"              '変更有りフラグ(条件用)
                .Add("LEASENO", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_LEASENO)    'リース登録番号
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htDetailData(LEASE_DISPPARAM.DP_CTNTYPE)    'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = htDetailData(LEASE_DISPPARAM.DP_CTNNO)             'コンテナ番号
                .Add("LEASEMONTHSTARTYMD", MySqlDbType.Date).Value = htDetailData(LEASE_DISPPARAM.DP_APPLYSTARTYMD) '契約開始日
                .Add("KEIJOKBN", MySqlDbType.VarChar).Value = C_KEIJO_KBN.NOT_RECORDED   '計上区分
            End With

            sqlHistoryCmd.CommandTimeout = 300
            sqlHistoryCmd.ExecuteNonQuery()
        End Using

    End Sub

End Class
