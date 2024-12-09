Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>精算ファイル用のキー</description></item>
''' </list>
''' </remarks>
Public Enum SEISANF_DP
    DP_SHIPYMD                  '発送年月日
    DP_CTNTYPE                  'コンテナ記号
    DP_CTNNO                    'コンテナ番号
    DP_SAMEDAYCNT               '同日内回数
    DP_CTNLINENO                '行番
    DP_BIGCTNCD                 '大分類コード
    DP_MIDDLECTNCD              '中分類コード
    DP_SMALLCTNCD               '小分類コード
    DP_JOTDEPBRANCHCD           'ＪＯＴ発組織コード
    DP_DEPSTATION               '発駅コード
    DP_DEPTRUSTEECD             '発受託人コード
    DP_DEPTRUSTEESUBCD          '発受託人サブ
    DP_JOTARRBRANCHCD           'ＪＯＴ着組織コード
    DP_ARRSTATION               '着駅コード
    DP_ARRTRUSTEECD             '着受託人コード
    DP_ARRTRUSTEESUBCD          '着受託人サブ
    DP_ARRPLANYMD               '到着予定年月日
    DP_STACKFREEKBN             '積空区分
    DP_STATUSKBN                '状態区分
    DP_CONTRACTCD               '契約コード
    DP_DEPTRAINNO               '発列車番号
    DP_ARRTRAINNO               '着列車番号
    DP_JRITEMCD                 'ＪＲ品目コード
    DP_LEASEPRODUCTCD           'リース品名コード
    DP_DEPSHIPPERCD             '発荷主コード
    DP_QUANTITY                 '個数
    DP_ADDSUBYM                 '加減額の対象年月
    DP_ADDSUBQUANTITY           '加減額の個数
    DP_JRFIXEDFARE              'ＪＲ所定運賃
    DP_USEFEE                   '使用料金額
    DP_OWNDISCOUNTFEE           '私有割引相当額
    DP_RETURNFARE               '割戻し運賃
    DP_NITTSUFREESEND           '通運負担回送運賃
    DP_MANAGEFEE                '運行管理料
    DP_SHIPBURDENFEE            '荷主負担運賃
    DP_SHIPFEE                  '発送料
    DP_ARRIVEFEE                '到着料
    DP_PICKUPFEE                '集荷料
    DP_DELIVERYFEE              '配達料
    DP_OTHER1FEE                'その他１
    DP_OTHER2FEE                'その他２
    DP_FREESENDFEE              '回送運賃
    DP_SPRFITKBN                '冷蔵適合マーク
    DP_JURISDICTIONCD           '所管部コード
    DP_ACCOUNTINGASSETSCD       '経理資産コード
    DP_ACCOUNTINGASSETSKBN      '経理資産区分
    DP_DUMMYKBN                 'ダミー区分
    DP_SPOTKBN                  'スポット区分
    DP_COMPKANKBN               '複合一貫区分
    DP_KEIJOYM                  '計上年月
    DP_TORICODE                 '取引先コード
    DP_PARTNERCAMPCD            '相手先会社コード
    DP_PARTNERDEPTCD            '相手先部門コード
    DP_INVKEIJYOBRANCHCD        '請求項目 計上店コード
    DP_INVFILINGDEPT            '請求項目 請求書提出部店
    DP_INVKESAIKBN              '請求項目 請求書決済区分
    DP_INVSUBCD                 '請求項目 請求書細分コード
    DP_PAYKEIJYOBRANCHCD        '支払項目 費用計上店コード
    DP_PAYFILINGBRANCH          '支払項目 支払書提出支店
    DP_TAXCALCUNIT              '支払項目 消費税計算単位
    DP_TAXKBN                   '税区分
    DP_TAXRATE                  '税率
    DP_BEFDEPTRUSTEECD          '変換前項目-発受託人コード
    DP_BEFDEPTRUSTEESUBCD       '変換前項目-発受託人サブ
    DP_BEFDEPSHIPPERCD          '変換前項目-発荷主コード
    DP_BEFARRTRUSTEECD          '変換前項目-着受託人コード
    DP_BEFARRTRUSTEESUBCD       '変換前項目-着受託人サブ
    DP_BEFJRITEMCD              '変換前項目-ＪＲ品目コード
    DP_BEFSTACKFREEKBN          '変換前項目-積空区分
    DP_BEFSTATUSKBN             '変換前項目-状態区分
    DP_SPLBEFDEPSTATION         '分割前項目-発駅コード
    DP_SPLBEFDEPTRUSTEECD       '分割前項目-発受託人コード
    DP_SPLBEFDEPTRUSTEESUBCD    '分割前項目-発受託人サブ
    DP_SPLBEFUSEFEE             '分割前項目-使用料金額
    DP_SPLBEFSHIPFEE            '分割前項目-発送料
    DP_SPLBEFARRIVEFEE          '分割前項目-到着料
    DP_SPLBEFFREESENDFEE        '分割前項目-回送運賃
    DP_ORDERNO                  'オーダーNo
    DP_ORDERLINENO              'オーダー行No
    DP_ACCOUNTSTATUSKBN         '勘定科目用状態区分
    DP_REFRIGERATIONFLG         '冷蔵適合フラグ
    DP_FIXEDFEE                 '固定使用料
    DP_INCOMEADJUSTFEE          '収入加減額
    DP_TOTALINCOME              '収入合計
    DP_COMMISSIONFEE            '手数料
    DP_COSTADJUSTFEE            '費用加減額
    DP_TOTALCOST                '費用合計
    DP_BILLLINK                 '請求連携状態
    DP_ACNTLINK                 '経理連携状態
    DP_CLOSINGDATE              '締年月日
    DP_SCHEDATEPAYMENT          '入金予定日
    DP_ACCOUNTINGMONTH          '計上月区分
    DP_DEPOSITMONTHKBN          '入金月区分
    DP_INACCOUNTCD              '社内口座コード
    DP_SLIPDESCRIPTION1         '伝票摘要１
    DP_SLIPDESCRIPTION2         '伝票摘要２
    DP_APPLSTATUS               '申請状況
    DP_APPLYMD                  '申請年月日
    DP_APPLUSER                 '申請者ユーザーＩＤ
    DP_CONFUPDYMD               '確認／修正年月日
    DP_CONFUPDUSER              '確認／修正者ユーザーＩＤ
    DP_UPDCAUSE                 '修正理由
    DP_APPROVALYMD              '承認年月日
    DP_APPROVALUSER             '承認者ユーザーＩＤ
    DP_APPROVALCAUSE            '承認／差戻し理由
    DP_INCOMECOSTOMITFLG        '収入費用除外フラグ
    DP_MANUALCREATEFLG          '手動作成フラグ
    DP_DELFLG              ' 削除フラグ
    DP_INITYMD             ' 登録年月日
    DP_INITUSER            ' 登録ユーザーＩＤ
    DP_INITTERMID          ' 登録端末
    DP_INITPGID            ' 登録プログラムＩＤ
    DP_UPDYMD              ' 更新年月日
    DP_UPDUSER             ' 更新ユーザーＩＤ
    DP_UPDTERMID           ' 更新端末
    DP_UPDPGID             ' 更新プログラムＩＤ
    DP_RECEIVEYMD          ' 集信日時
    DP_BEF_KEIJOYM              '計上年月(変更前)
    DP_BEF_TORICODE             '取引先コード(変更前)
    DP_BEF_INVFILINGDEPT        '請求項目 請求書提出部店(変更前)
    DP_BEF_PAYFILINGBRANCH      '支払項目 支払書提出支店(変更前)
    DP_BEF_SCHEDATEPAYMENT      '入金予定日(変更前)
    DP_BEF_STACKFREEKBN         '積空区分(変更前)
    DP_BEF_ACCOUNTSTATUSKBN     '勘定科目用状態区分(変更前)
    DP_BEF_INVSUBCD             '細分コード(変更前)
    DP_BEF_USEFEE               '使用料金額(変更前)
    DP_BEF_NITTSUFREESEND       '通運負担回送運賃(変更前)
    DP_BEF_MANAGEFEE            '運行管理料(変更前)
    DP_BEF_SHIPBURDENFEE        '荷主負担運賃(変更前)
    DP_BEF_PICKUPFEE            '集荷料(変更前)
    DP_BEF_INCOMEADJUSTFEE      '収入加減額(変更前)
    DP_BEF_TOTALINCOME          '収入合計(変更前)
    DP_BEF_FREESENDFEE          '回送運賃(変更前)
    DP_BEF_SHIPFEE              '発送料(変更前)
    DP_BEF_COMMISSIONFEE        '手数料(変更前)
    DP_BEF_COSTADJUSTFEE        '費用加減額(変更前)
    DP_BEF_TOTALCOST            '費用合計(変更前)
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>コンテナ清算ファイル 検索用(清算ファイル画面)</description></item>
''' </list>
''' </remarks>
Public Enum SEL_RESSNF
    SL_CAMPCODE             '会社コード
    SL_STATUS               '状況
    SL_TAISYOYM             '対象年月
    SL_JOTORGCODE           'JOT発組織
    SL_DEPTCODE             '担当部店
    SL_INVDEPTCODE          '請求書提出部店
    SL_TORICODE             '請求先
    SL_TORINAME             '請求先名
    SL_DEPSTATIONCODE       '発駅
    SL_DEPSTATIONNAME       '発駅名
    SL_DEPTRUSTEECODE       '発受託人
    SL_DATESTART            '発送年月日From
    SL_DATEEND              '発送年月日To
    SL_CTNTYPE              'コンテナ記号
    SL_CTNNO                'コンテナ番号
    SL_STACKFREEKBN         '状態
    SL_ARRSTATIONCODE       '着駅
    SL_ARRSTATIONNAME       '着駅名
    SL_SHIPPERCD            '荷主コード
    SL_SHIPPERNM            '荷主名
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>コンテナ清算ファイル 選択したデータ保存用</description></item>
''' </list>
''' </remarks>
Public Enum SEL_RESSNF_PKEY
    PK_SHIPYMD              '発送年月日
    PK_CTNTYPE              'コンテナ記号
    PK_CTNNO                'コンテナ番号
    PK_SAMEDAYCNT           '同日内回数
    PK_CTNLINENO            '行番
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>請求ヘッダーデータのキー</description></item>
''' </list>
''' </remarks>
Public Enum SEIHEAD_KEY
    KEIJOYM           '請求年月
    INVOICENUMBER     '請求番号
    INVOICEORGCODE    '請求担当部店コード
    TORICODE          '請求取引先コード
    INVOICETYPE       '請求書種類
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>支払ヘッダーデータのキー</description></item>
''' </list>
''' </remarks>
Public Enum PAYHEAD_KEY
    PAYMENTYM         '支払年月
    PAYMENTNUMBER     '支払番号
    PAYMENTORGCODE    '支払支店コード
    TORICODE          '支払取引先コード
End Enum

''' <summary>
''' リースデータテーブル登録クラス
''' </summary>
''' <remarks>各種リースデータテーブルに登録する際はこちらに定義</remarks>
Public Class EntrySeisanFData

    Private Const CONST_APPROVAL As String = "approval"                     '全支店許可
    Private Const CONST_ONLY_MY_DEPARTMENT As String = "only_my_department" '自支店のみ許可
    Private Const CONST_DISAPPROVAL As String = "disapproval"               '全支店不許可

    ''' <summary>
    ''' DataTableの指定位置からString値を取得する
    ''' </summary>
    ''' <param name="objOutputData">DataTable</param>
    ''' <param name="nRow">行</param>
    ''' <param name="strCol">列</param>
    ''' <param name="strDefault">規定値</param>
    ''' <returns>取得データ</returns>
    ''' <remarks>値がDBNULLの場合は規定値が返却される</remarks>
    Public Shared Function GetStringValue(ByVal objOutputData As DataTable, ByVal nRow As Integer, ByVal strCol As String, Optional ByVal strDefault As String = "") As String
        Dim strRet As String = strDefault
        Dim objCell As Object = objOutputData.Rows(nRow)(strCol)

        If Not IsDBNull(objCell) Then
            strRet = objCell.ToString
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' コンテナ清算ファイルTBL 清算ファイル一覧・更新画面検索処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="strCampCpde">会社コード</param>
    ''' <param name="strUserID">ユーザーID</param>
    ''' <param name="intGetLineNo">開始データ行番号</param>
    ''' <param name="intGetNum">データ取得件数</param>
    ''' <param name="dtSelPKEY">選択したデータKEY</param>
    ''' <param name="blnRessnfDetaliFlg">ダイレクト修正フラグ false：清算ファイル対応状況一覧 true：ダイレクト修正</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function SelectRessnfPage(sqlCon As MySqlConnection, sqlTran As MySqlTransaction,
                                        strCampCpde As String, strUserID As String, intGetLineNo As Integer, intGetNum As Integer,
                                        dtSelPKEY As DataTable,
                                        Optional blnRessnfDetaliFlg As Boolean = False, Optional sortSQL As String = "",
                                        Optional str_ref_role As String = "", Optional str_upd_role As String = "", Optional strOrgCd As String = "") As DataTable
        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now
        Dim param As New Hashtable
        Dim CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

        ' 一覧からの呼び出しの場合、参照権限ロール不許可の場合は取得を行わない。
        If Not blnRessnfDetaliFlg AndAlso str_ref_role = CONST_DISAPPROVAL Then
            '取得データなしで返却
            Return dt
        End If

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder

        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine(" TOP " & intGetNum.ToString)                            '取得件数
        SQLBldr.AppendLine("      DEPTRUSTEECD AS DEPTRUSTEECD")                    '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEENM AS DEPTRUSTEENM")                    '発受託人名称
        SQLBldr.AppendLine("	, DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD")              '発受託人サブコード
        SQLBldr.AppendLine("	, DEPTRUSTEESUBNM AS DEPTRUSTEESUBNM")              '発受託人サブ名称
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD AS JOTDEPBRANCHCD")                'JOT発組織コード
        SQLBldr.AppendLine("    , JOTDEPBRANCHNM AS JOTDEPBRANCHNM")                'JOT発組織名
        SQLBldr.AppendLine("    , DEPSTATIONCD AS DEPSTATIONCD")                    '発駅コード
        SQLBldr.AppendLine("    , DEPSTATIONNM AS DEPSTATIONNM")                    '発駅名称
        SQLBldr.AppendLine("    , SHIPYMD AS SHIPYMD")                              '発送年月日
        SQLBldr.AppendLine("    , STATUSKBNCD AS STATUSKBNCD")                      '状態区分
        SQLBldr.AppendLine("    , STATUSKBNNM AS STATUSKBNNM")                      '状態名称
        SQLBldr.AppendLine("    , ITEMCODE AS ITEMCODE")                            '品目コード
        SQLBldr.AppendLine("    , ITEMNAME AS ITEMNAME")                            '品目名称
        SQLBldr.AppendLine("    , JRITEMNM AS JRITEMNM")                            'JR品目名
        SQLBldr.AppendLine("    , CTNTYPE AS CTNTYPE")                              'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO AS CTNNO")                                  'コンテナ番号
        SQLBldr.AppendLine("    , DISPCTNNO AS DISPCTNNO")                          'コンテナ番号(表示用)
        SQLBldr.AppendLine("    , SAMEDAYCNT AS SAMEDAYCNT")                        '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO AS CTNLINENO")                          '行番
        SQLBldr.AppendLine("    , SPRFITKBNCD AS SPRFITKBNCD")                      '冷蔵区分
        SQLBldr.AppendLine("    , SPRFITKBNNM AS SPRFITKBNNM")                      '冷蔵名称
        SQLBldr.AppendLine("    , ARRTRUSTEECD AS ARRTRUSTEECD")                    '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEENM AS ARRTRUSTEENM")                    '着受託人名称
        SQLBldr.AppendLine("	, ARRTRUSTEESUBCD AS ARRTRUSTEESUBCD")              '着受託人サブコード
        SQLBldr.AppendLine("	, ARRTRUSTEESUBNM AS ARRTRUSTEESUBNM")              '着受託人サブ名称
        SQLBldr.AppendLine("    , ARRPLANYMD AS ARRPLANYMD")                        '到着年月日
        SQLBldr.AppendLine("    , JOTARRBRANCHCD AS JOTARRBRANCHCD")                'JOT着組織コード
        SQLBldr.AppendLine("    , JOTARRBRANCHNM AS JOTARRBRANCHNM")                'JOT発組織名
        SQLBldr.AppendLine("    , ARRSTATIONCD AS ARRSTATIONCD")                    '着駅コード
        SQLBldr.AppendLine("    , ARRSTATIONNM AS ARRSTATIONNM")                    '着駅名称
        SQLBldr.AppendLine("    , USEFEE AS USEFEE")                                '使用料
        SQLBldr.AppendLine("    , FREESENDFEE AS FREESENDFEE")                      '回送運賃
        SQLBldr.AppendLine("    , KEIJOYM AS KEIJOYM")                              '計上年月
        SQLBldr.AppendLine("    , CONTRACTCD AS CONTRACTCD")                        '契約コード
        SQLBldr.AppendLine("    , DEPSHIPPERCD AS DEPSHIPPERCD")                    '発荷主コード
        SQLBldr.AppendLine("    , DEPSHIPPERNM AS DEPSHIPPERNM")                    '発荷主名称
        SQLBldr.AppendLine("    , DEPTRAINNO AS DEPTRAINNO")                        '発列車番号
        SQLBldr.AppendLine("    , QUANTITY AS QUANTITY")                            '個数
        SQLBldr.AppendLine("    , ARRTRAINNO AS ARRTRAINNO")                        '着列車番号
        SQLBldr.AppendLine("    , NITTSUFREESEND AS NITTSUFREESEND")                '通運負担運賃
        SQLBldr.AppendLine("    , MANAGEFEE AS MANAGEFEE")                          '運行管理料
        SQLBldr.AppendLine("    , JRFIXEDFARE AS JRFIXEDFARE")                      'JR所定運賃
        SQLBldr.AppendLine("    , OWNDISCOUNTFEE AS OWNDISCOUNTFEE")                '私有割引相当額
        SQLBldr.AppendLine("    , RETURNFARE AS RETURNFARE")                        '割戻し運賃
        SQLBldr.AppendLine("    , SHIPBURDENFEE AS SHIPBURDENFEE")                  '荷主負担運賃
        SQLBldr.AppendLine("    , SHIPFEE AS SHIPFEE")                              '発送料
        SQLBldr.AppendLine("    , ARRIVEFEE AS ARRIVEFEE")                          '到着料
        SQLBldr.AppendLine("    , PICKUPFEE AS PICKUPFEE")                          '集荷料
        SQLBldr.AppendLine("    , DELIVERYFEE AS DELIVERYFEE")                      '配達料
        SQLBldr.AppendLine("    , OTHER1FEE AS OTHER1FEE")                          'その他１
        SQLBldr.AppendLine("    , OTHER2FEE AS OTHER2FEE")                          'その他２
        SQLBldr.AppendLine("    , TORICODE AS TORICODE")                            '取引先コード
        SQLBldr.AppendLine("    , TORINAME AS TORINAME")                            '取引先名称
        SQLBldr.AppendLine("    , TORINAMEDISP AS TORINAMEDISP")                    '取引先名称(一覧用)
        SQLBldr.AppendLine("    , INVSUBCD AS INVSUBCD")                            '細分コード
        SQLBldr.AppendLine("    , KEIJOBRCNAME AS KEIJOBRCNAME")                    '計上支店名
        SQLBldr.AppendLine("    , INVPAYBRCNAME AS INVPAYBRCNAME")                  '提出部店名
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNCD AS ACCOUNTSTATUSKBNCD")        '勘定科目用状態区分
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNNM AS ACCOUNTSTATUSKBNNM")        '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , REFRIGERATIONFLG AS REFRIGERATIONFLG")            '冷蔵適合フラグ
        SQLBldr.AppendLine("    , REFRIGERATIONFLGNM AS REFRIGERATIONFLGNM")        '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , FIXEDFEE AS FIXEDFEE")                            '固定使用料
        SQLBldr.AppendLine("    , INCOMEADJUSTFEE AS INCOMEADJUSTFEE")              '収入加減額
        SQLBldr.AppendLine("    , TOTALINCOME AS TOTALINCOME")                      '収入合計
        SQLBldr.AppendLine("    , COMMISSIONFEE AS COMMISSIONFEE")                  '手数料
        SQLBldr.AppendLine("    , COSTADJUSTFEE AS COSTADJUSTFEE")                  '費用加減額
        SQLBldr.AppendLine("    , TOTALCOST AS TOTALCOST")                          '費用合計
        SQLBldr.AppendLine("    , APPLSTATUS AS APPLSTATUS")                        '申請状況
        SQLBldr.AppendLine("    , APPLSTATUSNM AS APPLSTATUSNM")                    '申請状況名称
        SQLBldr.AppendLine("    , APPLYMD AS APPLYMD")                              '申請年月日
        SQLBldr.AppendLine("    , APPLUSER AS APPLUSER")                            '申請者ユーザーＩＤ
        SQLBldr.AppendLine("    , APPLUSERNM AS APPLUSERNM")                        '申請者
        SQLBldr.AppendLine("    , CONFUPDYMD AS CONFUPDYMD")                        '確認／修正年月日
        SQLBldr.AppendLine("    , CONFUPDUSER AS CONFUPDUSER")                      '確認／修正者ユーザID
        SQLBldr.AppendLine("    , CONFUPDUSERNM AS CONFUPDUSERNM")                  '確認／修正者
        SQLBldr.AppendLine("    , UPDCAUSE AS UPDCAUSE")                            '修正理由
        SQLBldr.AppendLine("    , APPROVALYMD AS APPROVALYMD")                      '承認年月日
        SQLBldr.AppendLine("    , APPROVALUSER AS APPROVALUSER")                    '承認者ユーザーＩＤ
        SQLBldr.AppendLine("    , APPROVALUSERNM AS APPROVALUSERNM")                '承認者
        SQLBldr.AppendLine("    , APPROVALCAUSE AS APPROVALCAUSE")                  '承認／差戻し理由
        SQLBldr.AppendLine("    , BIGCTNCD AS BIGCTNCD")                            '大分類コード
        SQLBldr.AppendLine("    , MIDDLECTNCD AS MIDDLECTNCD")                      '中分類コード
        SQLBldr.AppendLine("    , STACKFREEKBN AS STACKFREEKBN")                    '積空区分
        SQLBldr.AppendLine("    , BEF_KEIJOYM AS BEF_KEIJOYM")                      '計上年月(変更前)
        SQLBldr.AppendLine("    , BEF_TORICODE AS BEF_TORICODE")                    '取引先コード(変更前)
        SQLBldr.AppendLine("    , BEF_INVFILINGDEPT AS BEF_INVFILINGDEPT")          '請求項目 請求書提出部店(変更前)
        SQLBldr.AppendLine("    , BEF_PAYFILINGBRANCH AS BEF_PAYFILINGBRANCH")      '支払項目 支払書提出支店(変更前)
        SQLBldr.AppendLine("    , BEF_SCHEDATEPAYMENT AS BEF_SCHEDATEPAYMENT")      '入金予定日(変更前)
        SQLBldr.AppendLine("    , BEF_STACKFREEKBN AS BEF_STACKFREEKBN")            '積空区分(変更前)
        SQLBldr.AppendLine("    , BEF_ACCOUNTSTATUSKBN AS BEF_ACCOUNTSTATUSKBN")    '勘定科目用状態区分(変更前)
        SQLBldr.AppendLine("    , BEF_INVSUBCD AS BEF_INVSUBCD")                    '細分コード(変更前)
        SQLBldr.AppendLine("    , BEF_USEFEE AS BEF_USEFEE")                        '使用料(変更前)
        SQLBldr.AppendLine("    , BEF_NITTSUFREESEND AS BEF_NITTSUFREESEND")        '通運負担運賃(変更前)
        SQLBldr.AppendLine("    , BEF_MANAGEFEE AS BEF_MANAGEFEE")                  '運行管理料(変更前)
        SQLBldr.AppendLine("    , BEF_SHIPBURDENFEE AS BEF_SHIPBURDENFEE")          '荷主負担運賃(変更前)
        SQLBldr.AppendLine("    , BEF_PICKUPFEE AS BEF_PICKUPFEE")                  '集荷料(変更前)
        SQLBldr.AppendLine("    , BEF_INCOMEADJUSTFEE AS BEF_INCOMEADJUSTFEE")      '収入加減額(変更前)
        SQLBldr.AppendLine("    , BEF_TOTALINCOME AS BEF_TOTALINCOME")              '収入合計(変更前)
        SQLBldr.AppendLine("    , BEF_FREESENDFEE AS BEF_FREESENDFEE")              '回送運賃(変更前)
        SQLBldr.AppendLine("    , BEF_SHIPFEE AS BEF_SHIPFEE")                      '発送料(変更前)
        SQLBldr.AppendLine("    , BEF_COMMISSIONFEE AS BEF_COMMISSIONFEE")          '手数料(変更前)
        SQLBldr.AppendLine("    , BEF_COSTADJUSTFEE AS BEF_COSTADJUSTFEE")          '費用加減額(変更前)
        SQLBldr.AppendLine("    , BEF_TOTALCOST AS BEF_TOTALCOST")                  '費用合計(変更前)
        '変更前(初回)
        SQLBldr.AppendLine("    , DEPTRUSTEECD_B AS DEPTRUSTEECD_B")                '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEENM_B AS DEPTRUSTEENM_B")                '発受託人名称
        SQLBldr.AppendLine("	, DEPTRUSTEESUBCD_B AS DEPTRUSTEESUBCD_B")          '発受託人サブコード
        SQLBldr.AppendLine("	, DEPTRUSTEESUBNM_B AS DEPTRUSTEESUBNM_B")          '発受託人サブ名称
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD_B AS JOTDEPBRANCHCD_B")            'JOT発組織コード
        SQLBldr.AppendLine("    , JOTDEPBRANCHNM_B AS JOTDEPBRANCHNM_B")            'JOT発組織名
        SQLBldr.AppendLine("    , DEPSTATIONCD_B AS DEPSTATIONCD_B")                '発駅コード
        SQLBldr.AppendLine("    , DEPSTATIONNM_B AS DEPSTATIONNM_B")                '発駅名称
        SQLBldr.AppendLine("    , SHIPYMD_B AS SHIPYMD_B")                          '発送年月日
        SQLBldr.AppendLine("    , STATUSKBNCD_B AS STATUSKBNCD_B")                  '状態区分
        SQLBldr.AppendLine("    , ITEMCODE_B AS ITEMCODE_B")                        '品目コード
        SQLBldr.AppendLine("    , ITEMNAME_B AS ITEMNAME_B")                        '品目名称
        SQLBldr.AppendLine("    , JRITEMNM_B AS JRITEMNM_B")                        'JR品目名
        SQLBldr.AppendLine("    , CTNTYPE_B AS CTNTYPE_B")                          'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO_B AS CTNNO_B")                              'コンテナ番号
        SQLBldr.AppendLine("    , SAMEDAYCNT_B AS SAMEDAYCNT_B")                    '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO_B AS CTNLINENO_B")                      '行番
        SQLBldr.AppendLine("    , SPRFITKBNCD_B AS SPRFITKBNCD_B")                  '冷蔵区分
        SQLBldr.AppendLine("    , SPRFITKBNNM_B AS SPRFITKBNNM_B")                  '冷蔵名称
        SQLBldr.AppendLine("    , ARRTRUSTEECD_B AS ARRTRUSTEECD_B")                '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEENM_B AS ARRTRUSTEENM_B")                '着受託人名称
        SQLBldr.AppendLine("	, ARRTRUSTEESUBCD_B AS ARRTRUSTEESUBCD_B")          '着受託人サブコード
        SQLBldr.AppendLine("	, ARRTRUSTEESUBNM_B AS ARRTRUSTEESUBNM_B")          '着受託人サブ名称
        SQLBldr.AppendLine("    , ARRPLANYMD_B AS ARRPLANYMD_B")                    '到着年月日
        SQLBldr.AppendLine("    , JOTARRBRANCHCD_B AS JOTARRBRANCHCD_B")            'JOT着組織コード
        SQLBldr.AppendLine("    , JOTARRBRANCHNM_B AS JOTARRBRANCHNM_B")            'JOT発組織名
        SQLBldr.AppendLine("    , ARRSTATIONCD_B AS ARRSTATIONCD_B")                '着駅コード
        SQLBldr.AppendLine("    , ARRSTATIONNM_B AS ARRSTATIONNM_B")                '着駅名称
        SQLBldr.AppendLine("    , USEFEE_B AS USEFEE_B")                            '使用料
        SQLBldr.AppendLine("    , FREESENDFEE_B AS FREESENDFEE_B")                  '回送運賃
        SQLBldr.AppendLine("    , KEIJOYM_B AS KEIJOYM_B")                          '計上年月
        SQLBldr.AppendLine("    , CONTRACTCD_B AS CONTRACTCD_B")                    '契約コード
        SQLBldr.AppendLine("    , DEPSHIPPERCD_B AS DEPSHIPPERCD_B")                '発荷主コード
        SQLBldr.AppendLine("    , DEPSHIPPERNM_B AS DEPSHIPPERNM_B")                '発荷主名称
        SQLBldr.AppendLine("    , DEPTRAINNO_B AS DEPTRAINNO_B")                    '発列車番号
        SQLBldr.AppendLine("    , QUANTITY_B AS QUANTITY_B")                        '個数
        SQLBldr.AppendLine("    , ARRTRAINNO_B AS ARRTRAINNO_B")                    '着列車番号
        SQLBldr.AppendLine("    , NITTSUFREESEND_B AS NITTSUFREESEND_B")            '通運負担運賃
        SQLBldr.AppendLine("    , MANAGEFEE_B AS MANAGEFEE_B")                      '運行管理料
        SQLBldr.AppendLine("    , JRFIXEDFARE_B AS JRFIXEDFARE_B")                  'JR所定運賃
        SQLBldr.AppendLine("    , OWNDISCOUNTFEE_B AS OWNDISCOUNTFEE_B")            '私有割引相当額
        SQLBldr.AppendLine("    , RETURNFARE_B AS RETURNFARE_B")                    '割戻し運賃
        SQLBldr.AppendLine("    , SHIPBURDENFEE_B AS SHIPBURDENFEE_B")              '荷主負担運賃
        SQLBldr.AppendLine("    , SHIPFEE_B AS SHIPFEE_B")                          '発送料
        SQLBldr.AppendLine("    , ARRIVEFEE_B AS ARRIVEFEE_B")                      '到着料
        SQLBldr.AppendLine("    , PICKUPFEE_B AS PICKUPFEE_B")                      '集荷料
        SQLBldr.AppendLine("    , DELIVERYFEE_B AS DELIVERYFEE_B")                  '配達料
        SQLBldr.AppendLine("    , OTHER1FEE_B AS OTHER1FEE_B")                      'その他１
        SQLBldr.AppendLine("    , OTHER2FEE_B AS OTHER2FEE_B")                      'その他２
        SQLBldr.AppendLine("    , TORICODE_B AS TORICODE_B")                        '取引先コード
        SQLBldr.AppendLine("    , TORINAME_B AS TORINAME_B")                        '取引先名称
        SQLBldr.AppendLine("    , INVSUBCD_B AS INVSUBCD_B")                        '細分コード
        SQLBldr.AppendLine("    , KEIJOBRCNAME_B AS KEIJOBRCNAME_B")                '計上支店名
        SQLBldr.AppendLine("    , INVPAYBRCNAME_B AS INVPAYBRCNAME_B")              '提出部店名
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNCD_B AS ACCOUNTSTATUSKBNCD_B")    '勘定科目用状態区分
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNNM_B AS ACCOUNTSTATUSKBNNM_B")    '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , REFRIGERATIONFLG_B AS REFRIGERATIONFLG_B")        '冷蔵適合フラグ
        SQLBldr.AppendLine("    , REFRIGERATIONFLGNM_B AS REFRIGERATIONFLGNM_B")    '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , FIXEDFEE_B AS FIXEDFEE_B")                        '固定使用料
        SQLBldr.AppendLine("    , INCOMEADJUSTFEE_B AS INCOMEADJUSTFEE_B")          '収入加減額
        SQLBldr.AppendLine("    , TOTALINCOME_B AS TOTALINCOME_B")                  '収入合計
        SQLBldr.AppendLine("    , COMMISSIONFEE_B AS COMMISSIONFEE_B")              '手数料
        SQLBldr.AppendLine("    , COSTADJUSTFEE_B AS COSTADJUSTFEE_B")              '費用加減額
        SQLBldr.AppendLine("    , TOTALCOST_B AS TOTALCOST_B")                      '費用合計
        SQLBldr.AppendLine("    , APPLSTATUS_B AS APPLSTATUS_B")                    '申請状況
        SQLBldr.AppendLine("    , BIGCTNCD_B AS BIGCTNCD_B")                        '大分類コード
        SQLBldr.AppendLine("    , MIDDLECTNCD_B AS MIDDLECTNCD_B")                  '中分類コード
        SQLBldr.AppendLine("    , STACKFREEKBN_B AS STACKFREEKBN_B")                '積空区分
        SQLBldr.AppendLine("    , ROW_SEL AS ROW_SEL")                              '申請状況
        SQLBldr.AppendLine("    , GETLINENO AS GETLINENO")                          'LINENO
        SQLBldr.AppendLine("    , INITUSER AS INITUSER")                            '登録ユーザ
        SQLBldr.AppendLine("    , INITUSERORG AS INITUSERORG")                      '登録ユーザORG
        SQLBldr.AppendLine("FROM (")

        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    ROW_NUMBER() OVER(ORDER BY ")
        If String.IsNullOrEmpty(sortSQL) Then
            SQLBldr.AppendLine("GETLINENO")
        Else
            SQLBldr.AppendLine(sortSQL)
        End If
        SQLBldr.AppendLine(") AS ROWNUM ")

        SQLBldr.AppendLine("    , DEPTRUSTEECD AS DEPTRUSTEECD")                        '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEENM AS DEPTRUSTEENM")                        '発受託人名称
        SQLBldr.AppendLine("	, DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD")                  '発受託人サブコード
        SQLBldr.AppendLine("	, DEPTRUSTEESUBNM AS DEPTRUSTEESUBNM")                  '発受託人サブ名称
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD AS JOTDEPBRANCHCD")                    'JOT発組織コード
        SQLBldr.AppendLine("    , JOTDEPBRANCHNM AS JOTDEPBRANCHNM")                    'JOT発組織名
        SQLBldr.AppendLine("    , DEPSTATIONCD AS DEPSTATIONCD")                        '発駅コード
        SQLBldr.AppendLine("    , DEPSTATIONNM AS DEPSTATIONNM")                        '発駅名称
        SQLBldr.AppendLine("    , SHIPYMD AS SHIPYMD")                                  '発送年月日
        SQLBldr.AppendLine("    , STATUSKBNCD AS STATUSKBNCD")                          '状態区分
        SQLBldr.AppendLine("    , STATUSKBNNM AS STATUSKBNNM")                          '状態名称
        SQLBldr.AppendLine("    , ITEMCODE AS ITEMCODE")                                '品目コード
        SQLBldr.AppendLine("    , ITEMNAME AS ITEMNAME")                                '品目名称
        SQLBldr.AppendLine("    , JRITEMNM AS JRITEMNM")                                'JR品目名
        SQLBldr.AppendLine("    , CTNTYPE AS CTNTYPE")                                  'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO AS CTNNO")                                      'コンテナ番号
        SQLBldr.AppendLine("    , DISPCTNNO AS DISPCTNNO")                              'コンテナ番号(表示用)
        SQLBldr.AppendLine("    , SAMEDAYCNT AS SAMEDAYCNT")                            '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO AS CTNLINENO")                              '行番
        SQLBldr.AppendLine("    , SPRFITKBNCD AS SPRFITKBNCD")                          '冷蔵区分
        SQLBldr.AppendLine("    , SPRFITKBNNM AS SPRFITKBNNM")                          '冷蔵名称
        SQLBldr.AppendLine("    , ARRTRUSTEECD AS ARRTRUSTEECD")                        '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEENM AS ARRTRUSTEENM")                        '着受託人名称
        SQLBldr.AppendLine("	, ARRTRUSTEESUBCD AS ARRTRUSTEESUBCD")                  '着受託人サブコード
        SQLBldr.AppendLine("	, ARRTRUSTEESUBNM AS ARRTRUSTEESUBNM")                  '着受託人サブ名称
        SQLBldr.AppendLine("    , ARRPLANYMD AS ARRPLANYMD")                            '到着年月日
        SQLBldr.AppendLine("    , JOTARRBRANCHCD AS JOTARRBRANCHCD")                    'JOT着組織コード
        SQLBldr.AppendLine("    , JOTARRBRANCHNM AS JOTARRBRANCHNM")                    'JOT発組織名
        SQLBldr.AppendLine("    , ARRSTATIONCD AS ARRSTATIONCD")                        '着駅コード
        SQLBldr.AppendLine("    , ARRSTATIONNM AS ARRSTATIONNM")                        '着駅名称
        SQLBldr.AppendLine("    , USEFEE AS USEFEE")                                    '使用料
        SQLBldr.AppendLine("    , FREESENDFEE AS FREESENDFEE")                          '回送運賃
        SQLBldr.AppendLine("    , KEIJOYM AS KEIJOYM")                                  '計上年月
        SQLBldr.AppendLine("    , CONTRACTCD AS CONTRACTCD")                            '契約コード
        SQLBldr.AppendLine("    , DEPSHIPPERCD AS DEPSHIPPERCD")                        '発荷主コード
        SQLBldr.AppendLine("    , DEPSHIPPERNM AS DEPSHIPPERNM")                        '発荷主名称
        SQLBldr.AppendLine("    , DEPTRAINNO AS DEPTRAINNO")                            '発列車番号
        SQLBldr.AppendLine("    , QUANTITY AS QUANTITY")                                '個数
        SQLBldr.AppendLine("    , ARRTRAINNO AS ARRTRAINNO")                            '着列車番号
        SQLBldr.AppendLine("    , NITTSUFREESEND AS NITTSUFREESEND")                    '通運負担運賃
        SQLBldr.AppendLine("    , MANAGEFEE AS MANAGEFEE")                              '運行管理料
        SQLBldr.AppendLine("    , JRFIXEDFARE AS JRFIXEDFARE")                          'JR所定運賃
        SQLBldr.AppendLine("    , OWNDISCOUNTFEE AS OWNDISCOUNTFEE")                    '私有割引相当額
        SQLBldr.AppendLine("    , RETURNFARE AS RETURNFARE")                            '割戻し運賃
        SQLBldr.AppendLine("    , SHIPBURDENFEE AS SHIPBURDENFEE")                      '荷主負担運賃
        SQLBldr.AppendLine("    , SHIPFEE AS SHIPFEE")                                  '発送料
        SQLBldr.AppendLine("    , ARRIVEFEE AS ARRIVEFEE")                              '到着料
        SQLBldr.AppendLine("    , PICKUPFEE AS PICKUPFEE")                              '集荷料
        SQLBldr.AppendLine("    , DELIVERYFEE AS DELIVERYFEE")                          '配達料
        SQLBldr.AppendLine("    , OTHER1FEE AS OTHER1FEE")                              'その他１
        SQLBldr.AppendLine("    , OTHER2FEE AS OTHER2FEE")                              'その他２
        SQLBldr.AppendLine("    , TORICODE AS TORICODE")                                '取引先コード
        SQLBldr.AppendLine("    , TORINAME AS TORINAME")                                '取引先名称
        SQLBldr.AppendLine("    , TORINAMEDISP AS TORINAMEDISP")                        '取引先名称(一覧用)
        SQLBldr.AppendLine("    , INVSUBCD AS INVSUBCD")                                '細分コード
        SQLBldr.AppendLine("    , KEIJOBRCNAME AS KEIJOBRCNAME")                        '計上支店名
        SQLBldr.AppendLine("    , INVPAYBRCNAME AS INVPAYBRCNAME")                      '提出部店名
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNCD AS ACCOUNTSTATUSKBNCD")            '勘定科目用状態区分
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNNM AS ACCOUNTSTATUSKBNNM")            '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , REFRIGERATIONFLG AS REFRIGERATIONFLG")                '冷蔵適合フラグ
        SQLBldr.AppendLine("    , REFRIGERATIONFLGNM AS REFRIGERATIONFLGNM")            '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , FIXEDFEE AS FIXEDFEE")                                '固定使用料
        SQLBldr.AppendLine("    , INCOMEADJUSTFEE AS INCOMEADJUSTFEE")                  '収入加減額
        SQLBldr.AppendLine("    , TOTALINCOME AS TOTALINCOME")                          '収入合計
        SQLBldr.AppendLine("    , COMMISSIONFEE AS COMMISSIONFEE")                      '手数料
        SQLBldr.AppendLine("    , COSTADJUSTFEE AS COSTADJUSTFEE")                      '費用加減額
        SQLBldr.AppendLine("    , TOTALCOST AS TOTALCOST")                              '費用合計
        SQLBldr.AppendLine("    , APPLSTATUS AS APPLSTATUS")                            '申請状況
        SQLBldr.AppendLine("    , APPLSTATUSNM AS APPLSTATUSNM")                        '申請状況名称
        SQLBldr.AppendLine("    , APPLYMD AS APPLYMD")                                  '申請年月日
        SQLBldr.AppendLine("    , APPLUSER AS APPLUSER")                                '申請者ユーザーＩＤ
        SQLBldr.AppendLine("    , APPLUSERNM AS APPLUSERNM")                            '申請者
        SQLBldr.AppendLine("    , CONFUPDYMD AS CONFUPDYMD")                            '確認／修正年月日
        SQLBldr.AppendLine("    , CONFUPDUSER AS CONFUPDUSER")                          '確認／修正者ユーザID
        SQLBldr.AppendLine("    , CONFUPDUSERNM AS CONFUPDUSERNM")                      '確認／修正者
        SQLBldr.AppendLine("    , UPDCAUSE AS UPDCAUSE")                                '修正理由
        SQLBldr.AppendLine("    , APPROVALYMD AS APPROVALYMD")                          '承認年月日
        SQLBldr.AppendLine("    , APPROVALUSER AS APPROVALUSER")                        '承認者ユーザーＩＤ
        SQLBldr.AppendLine("    , APPROVALUSERNM AS APPROVALUSERNM")                    '承認者
        SQLBldr.AppendLine("    , APPROVALCAUSE AS APPROVALCAUSE")                      '承認／差戻し理由
        SQLBldr.AppendLine("    , BIGCTNCD AS BIGCTNCD")                                '大分類コード
        SQLBldr.AppendLine("    , MIDDLECTNCD AS MIDDLECTNCD")                          '中分類コード
        SQLBldr.AppendLine("    , STACKFREEKBN AS STACKFREEKBN")                        '積空区分
        SQLBldr.AppendLine("    , BEF_KEIJOYM AS BEF_KEIJOYM")                          '計上年月(変更前)
        SQLBldr.AppendLine("    , BEF_TORICODE AS BEF_TORICODE")                        '取引先コード(変更前)
        SQLBldr.AppendLine("    , BEF_INVFILINGDEPT AS BEF_INVFILINGDEPT")              '請求項目 請求書提出部店(変更前)
        SQLBldr.AppendLine("    , BEF_PAYFILINGBRANCH AS BEF_PAYFILINGBRANCH")          '支払項目 支払書提出支店(変更前)
        SQLBldr.AppendLine("    , BEF_SCHEDATEPAYMENT AS BEF_SCHEDATEPAYMENT")          '入金予定日(変更前)
        SQLBldr.AppendLine("    , BEF_STACKFREEKBN AS BEF_STACKFREEKBN")                '積空区分(変更前)
        SQLBldr.AppendLine("    , BEF_ACCOUNTSTATUSKBN AS BEF_ACCOUNTSTATUSKBN")        '勘定科目用状態区分(変更前)
        SQLBldr.AppendLine("    , BEF_INVSUBCD AS BEF_INVSUBCD")                        '細分コード(変更前)
        SQLBldr.AppendLine("    , BEF_USEFEE AS BEF_USEFEE")                            '使用料(変更前)
        SQLBldr.AppendLine("    , BEF_NITTSUFREESEND AS BEF_NITTSUFREESEND")            '通運負担運賃(変更前)
        SQLBldr.AppendLine("    , BEF_MANAGEFEE AS BEF_MANAGEFEE")                      '運行管理料(変更前)
        SQLBldr.AppendLine("    , BEF_SHIPBURDENFEE AS BEF_SHIPBURDENFEE")              '荷主負担運賃(変更前)
        SQLBldr.AppendLine("    , BEF_PICKUPFEE AS BEF_PICKUPFEE")                      '集荷料(変更前)
        SQLBldr.AppendLine("    , BEF_INCOMEADJUSTFEE AS BEF_INCOMEADJUSTFEE")          '収入加減額(変更前)
        SQLBldr.AppendLine("    , BEF_TOTALINCOME AS BEF_TOTALINCOME")                  '収入合計(変更前)
        SQLBldr.AppendLine("    , BEF_FREESENDFEE AS BEF_FREESENDFEE")                  '回送運賃(変更前)
        SQLBldr.AppendLine("    , BEF_SHIPFEE AS BEF_SHIPFEE")                          '発送料(変更前)
        SQLBldr.AppendLine("    , BEF_COMMISSIONFEE AS BEF_COMMISSIONFEE")              '手数料(変更前)
        SQLBldr.AppendLine("    , BEF_COSTADJUSTFEE AS BEF_COSTADJUSTFEE")              '費用加減額(変更前)
        SQLBldr.AppendLine("    , BEF_TOTALCOST AS BEF_TOTALCOST")                      '費用合計(変更前)
        '変更前
        SQLBldr.AppendLine("    , DEPTRUSTEECD_B AS DEPTRUSTEECD_B")                      '発受託人コード
        SQLBldr.AppendLine("    , DEPTRUSTEENM_B AS DEPTRUSTEENM_B")                      '発受託人名称
        SQLBldr.AppendLine("	, DEPTRUSTEESUBCD_B AS DEPTRUSTEESUBCD_B")                '発受託人サブコード
        SQLBldr.AppendLine("	, DEPTRUSTEESUBNM_B AS DEPTRUSTEESUBNM_B")                '発受託人サブ名称
        SQLBldr.AppendLine("    , JOTDEPBRANCHCD_B AS JOTDEPBRANCHCD_B")                  'JOT発組織コード
        SQLBldr.AppendLine("    , JOTDEPBRANCHNM_B AS JOTDEPBRANCHNM_B")                  'JOT発組織名
        SQLBldr.AppendLine("    , DEPSTATIONCD_B AS DEPSTATIONCD_B")                      '発駅コード
        SQLBldr.AppendLine("    , DEPSTATIONNM_B AS DEPSTATIONNM_B")                      '発駅名称
        SQLBldr.AppendLine("    , SHIPYMD_B AS SHIPYMD_B")                                '発送年月日
        SQLBldr.AppendLine("    , STATUSKBNCD_B AS STATUSKBNCD_B")                        '状態区分
        SQLBldr.AppendLine("    , ITEMCODE_B AS ITEMCODE_B")                              '品目コード
        SQLBldr.AppendLine("    , ITEMNAME_B AS ITEMNAME_B")                              '品目名称
        SQLBldr.AppendLine("    , JRITEMNM_B AS JRITEMNM_B")                              'JR品目名
        SQLBldr.AppendLine("    , CTNTYPE_B AS CTNTYPE_B")                                'コンテナ記号
        SQLBldr.AppendLine("    , CTNNO_B AS CTNNO_B")                                    'コンテナ番号
        SQLBldr.AppendLine("    , SAMEDAYCNT_B AS SAMEDAYCNT_B")                          '同日内回数
        SQLBldr.AppendLine("    , CTNLINENO_B AS CTNLINENO_B")                            '行番
        SQLBldr.AppendLine("    , SPRFITKBNCD_B AS SPRFITKBNCD_B")                        '冷蔵区分
        SQLBldr.AppendLine("    , SPRFITKBNNM_B AS SPRFITKBNNM_B")                        '冷蔵名称
        SQLBldr.AppendLine("    , ARRTRUSTEECD_B AS ARRTRUSTEECD_B")                      '着受託人コード
        SQLBldr.AppendLine("    , ARRTRUSTEENM_B AS ARRTRUSTEENM_B")                      '着受託人名称
        SQLBldr.AppendLine("	, ARRTRUSTEESUBCD_B AS ARRTRUSTEESUBCD_B")                '着受託人サブコード
        SQLBldr.AppendLine("	, ARRTRUSTEESUBNM_B AS ARRTRUSTEESUBNM_B")                '着受託人サブ名称
        SQLBldr.AppendLine("    , ARRPLANYMD_B AS ARRPLANYMD_B")                          '到着年月日
        SQLBldr.AppendLine("    , JOTARRBRANCHCD_B AS JOTARRBRANCHCD_B")                  'JOT着組織コード
        SQLBldr.AppendLine("    , JOTARRBRANCHNM_B AS JOTARRBRANCHNM_B")                  'JOT発組織名
        SQLBldr.AppendLine("    , ARRSTATIONCD_B AS ARRSTATIONCD_B")                      '着駅コード
        SQLBldr.AppendLine("    , ARRSTATIONNM_B AS ARRSTATIONNM_B")                      '着駅名称
        SQLBldr.AppendLine("    , USEFEE_B AS USEFEE_B")                                  '使用料
        SQLBldr.AppendLine("    , FREESENDFEE_B AS FREESENDFEE_B")                        '回送運賃
        SQLBldr.AppendLine("    , KEIJOYM_B AS KEIJOYM_B")                                '計上年月
        SQLBldr.AppendLine("    , CONTRACTCD_B AS CONTRACTCD_B")                          '契約コード
        SQLBldr.AppendLine("    , DEPSHIPPERCD_B AS DEPSHIPPERCD_B")                      '発荷主コード
        SQLBldr.AppendLine("    , DEPSHIPPERNM_B AS DEPSHIPPERNM_B")                      '発荷主名称
        SQLBldr.AppendLine("    , DEPTRAINNO_B AS DEPTRAINNO_B")                          '発列車番号
        SQLBldr.AppendLine("    , QUANTITY_B AS QUANTITY_B")                              '個数
        SQLBldr.AppendLine("    , ARRTRAINNO_B AS ARRTRAINNO_B")                          '着列車番号
        SQLBldr.AppendLine("    , NITTSUFREESEND_B AS NITTSUFREESEND_B")                  '通運負担運賃
        SQLBldr.AppendLine("    , MANAGEFEE_B AS MANAGEFEE_B")                            '運行管理料
        SQLBldr.AppendLine("    , JRFIXEDFARE_B AS JRFIXEDFARE_B")                        'JR所定運賃
        SQLBldr.AppendLine("    , OWNDISCOUNTFEE_B AS OWNDISCOUNTFEE_B")                  '私有割引相当額
        SQLBldr.AppendLine("    , RETURNFARE_B AS RETURNFARE_B")                          '割戻し運賃
        SQLBldr.AppendLine("    , SHIPBURDENFEE_B AS SHIPBURDENFEE_B")                    '荷主負担運賃
        SQLBldr.AppendLine("    , SHIPFEE_B AS SHIPFEE_B")                                '発送料
        SQLBldr.AppendLine("    , ARRIVEFEE_B AS ARRIVEFEE_B")                            '到着料
        SQLBldr.AppendLine("    , PICKUPFEE_B AS PICKUPFEE_B")                            '集荷料
        SQLBldr.AppendLine("    , DELIVERYFEE_B AS DELIVERYFEE_B")                        '配達料
        SQLBldr.AppendLine("    , OTHER1FEE_B AS OTHER1FEE_B")                            'その他１
        SQLBldr.AppendLine("    , OTHER2FEE_B AS OTHER2FEE_B")                            'その他２
        SQLBldr.AppendLine("    , TORICODE_B AS TORICODE_B")                              '取引先コード
        SQLBldr.AppendLine("    , TORINAME_B AS TORINAME_B")                              '取引先名称
        SQLBldr.AppendLine("    , INVSUBCD_B AS INVSUBCD_B")                              '細分コード
        SQLBldr.AppendLine("    , KEIJOBRCNAME_B AS KEIJOBRCNAME_B")                      '計上支店名
        SQLBldr.AppendLine("    , INVPAYBRCNAME_B AS INVPAYBRCNAME_B")                    '提出部店名
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNCD_B AS ACCOUNTSTATUSKBNCD_B")          '勘定科目用状態区分
        SQLBldr.AppendLine("    , ACCOUNTSTATUSKBNNM_B AS ACCOUNTSTATUSKBNNM_B")          '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , REFRIGERATIONFLG_B AS REFRIGERATIONFLG_B")              '冷蔵適合フラグ
        SQLBldr.AppendLine("    , REFRIGERATIONFLGNM_B AS REFRIGERATIONFLGNM_B")          '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , FIXEDFEE_B AS FIXEDFEE_B")                              '固定使用料
        SQLBldr.AppendLine("    , INCOMEADJUSTFEE_B AS INCOMEADJUSTFEE_B")                '収入加減額
        SQLBldr.AppendLine("    , TOTALINCOME_B AS TOTALINCOME_B")                        '収入合計
        SQLBldr.AppendLine("    , COMMISSIONFEE_B AS COMMISSIONFEE_B")                    '手数料
        SQLBldr.AppendLine("    , COSTADJUSTFEE_B AS COSTADJUSTFEE_B")                    '費用加減額
        SQLBldr.AppendLine("    , TOTALCOST_B AS TOTALCOST_B")                            '費用合計
        SQLBldr.AppendLine("    , APPLSTATUS_B AS APPLSTATUS_B")                          '申請状況
        SQLBldr.AppendLine("    , BIGCTNCD_B AS BIGCTNCD_B")                              '大分類コード
        SQLBldr.AppendLine("    , MIDDLECTNCD_B AS MIDDLECTNCD_B")                        '中分類コード
        SQLBldr.AppendLine("    , STACKFREEKBN_B AS STACKFREEKBN_B")                      '積空区分
        SQLBldr.AppendLine("    , ROW_SEL AS ROW_SEL")                                    '申請状況
        SQLBldr.AppendLine("    , GETLINENO AS GETLINENO")                                'LINENO
        SQLBldr.AppendLine("    , INITUSER AS INITUSER")                                  '登録ユーザ
        SQLBldr.AppendLine("    , INITUSERORG AS INITUSERORG")                            '登録ユーザORG
        SQLBldr.AppendLine("FROM (")

        SQLBldr.AppendLine("SELECT")
        'SQLBldr.AppendLine(" TOP " & intGetNum.ToString)                                   '取得件数
        SQLBldr.AppendLine("      A01.DEPTRUSTEECD AS DEPTRUSTEECD")                        '発受託人コード
        SQLBldr.AppendLine("    , A02.DEPTRUSTEENM AS DEPTRUSTEENM")                        '発受託人名称
        SQLBldr.AppendLine("	, A01.DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD")                  '発受託人サブコード
        SQLBldr.AppendLine("	, A02.DEPTRUSTEESUBNM AS DEPTRUSTEESUBNM")                  '発受託人サブ名称
        SQLBldr.AppendLine("    , A01.JOTDEPBRANCHCD AS JOTDEPBRANCHCD")                    'JOT発組織コード
        SQLBldr.AppendLine("    , A14.NAMES AS JOTDEPBRANCHNM")                             'JOT発組織名
        SQLBldr.AppendLine("    , A01.DEPSTATION AS DEPSTATIONCD")                          '発駅コード
        SQLBldr.AppendLine("    , A03.NAMES AS DEPSTATIONNM")                               '発駅名称
        SQLBldr.AppendLine("    , A01.SHIPYMD AS SHIPYMD")                                  '発送年月日
        SQLBldr.AppendLine("    , A01.STATUSKBN AS STATUSKBNCD")                            '状態区分
        SQLBldr.AppendLine("    , A06.VALUE1 AS STATUSKBNNM")                               '状態名称
        SQLBldr.AppendLine("    , A01.JRITEMCD AS ITEMCODE")                                '品目コード
        SQLBldr.AppendLine("    , A07.NAME AS ITEMNAME")                                    '品目名称
        SQLBldr.AppendLine("    , A01.JRITEMNM AS JRITEMNM")                                'JR品目名
        SQLBldr.AppendLine("    , A01.CTNTYPE AS CTNTYPE")                                  'コンテナ記号
        SQLBldr.AppendLine("    , A01.CTNNO AS CTNNO")                                      'コンテナ番号
        SQLBldr.AppendLine("    , Right('00000000' + convert(varchar, coalesce(A01.CTNNO, 0)), 8)  AS DISPCTNNO")  'コンテナ番号(表示用)
        SQLBldr.AppendLine("    , A01.SAMEDAYCNT AS SAMEDAYCNT")                            '同日内回数
        SQLBldr.AppendLine("    , A01.CTNLINENO AS CTNLINENO")                              '行番
        SQLBldr.AppendLine("    , A01.SPRFITKBN AS SPRFITKBNCD")                            '冷蔵区分
        SQLBldr.AppendLine("    , A08.VALUE1 AS SPRFITKBNNM")                               '冷蔵名称
        SQLBldr.AppendLine("    , A01.ARRTRUSTEECD AS ARRTRUSTEECD")                        '着受託人コード
        SQLBldr.AppendLine("    , coalesce(A04.DEPTRUSTEENM, A042.DEPTRUSTEENM) AS ARRTRUSTEENM")  '着受託人名称
        SQLBldr.AppendLine("	, A01.ARRTRUSTEESUBCD AS ARRTRUSTEESUBCD")                  '着受託人サブコード
        SQLBldr.AppendLine("	, A04.DEPTRUSTEESUBNM AS ARRTRUSTEESUBNM")                  '着受託人サブ名称
        SQLBldr.AppendLine("    , A01.ARRPLANYMD AS ARRPLANYMD")                            '到着年月日
        SQLBldr.AppendLine("    , A01.JOTARRBRANCHCD AS JOTARRBRANCHCD")                    'JOT着組織コード
        SQLBldr.AppendLine("    , A15.NAMES AS JOTARRBRANCHNM")                             'JOT発組織名
        SQLBldr.AppendLine("    , A01.ARRSTATION AS ARRSTATIONCD")                          '着駅コード
        SQLBldr.AppendLine("    , A05.NAMES AS ARRSTATIONNM")                               '着駅名称
        SQLBldr.AppendLine("    , coalesce(A01.USEFEE,0) AS USEFEE")                          '使用料
        SQLBldr.AppendLine("    , coalesce(A01.FREESENDFEE,0) AS FREESENDFEE")                '回送運賃
        SQLBldr.AppendLine("    , A01.KEIJOYM AS KEIJOYM")                                  '計上年月
        SQLBldr.AppendLine("    , A01.CONTRACTCD AS CONTRACTCD")                            '契約コード
        SQLBldr.AppendLine("    , A01.DEPSHIPPERCD AS DEPSHIPPERCD")                        '発荷主コード
        SQLBldr.AppendLine("    , A09.NAME AS DEPSHIPPERNM")                                '発荷主名称
        SQLBldr.AppendLine("    , A01.DEPTRAINNO AS DEPTRAINNO")                            '発列車番号
        SQLBldr.AppendLine("    , A01.QUANTITY AS QUANTITY")                                '個数
        SQLBldr.AppendLine("    , A01.ARRTRAINNO AS ARRTRAINNO")                            '着列車番号
        SQLBldr.AppendLine("    , A01.NITTSUFREESEND AS NITTSUFREESEND")                    '通運負担運賃
        SQLBldr.AppendLine("    , A01.MANAGEFEE AS MANAGEFEE")                              '運行管理料
        SQLBldr.AppendLine("    , A01.JRFIXEDFARE AS JRFIXEDFARE")                          'JR所定運賃
        SQLBldr.AppendLine("    , A01.OWNDISCOUNTFEE AS OWNDISCOUNTFEE")                    '私有割引相当額
        SQLBldr.AppendLine("    , A01.RETURNFARE AS RETURNFARE")                            '割戻し運賃
        SQLBldr.AppendLine("    , A01.SHIPBURDENFEE AS SHIPBURDENFEE")                      '荷主負担運賃
        SQLBldr.AppendLine("    , A01.SHIPFEE AS SHIPFEE")                                  '発送料
        SQLBldr.AppendLine("    , A01.ARRIVEFEE AS ARRIVEFEE")                              '到着料
        SQLBldr.AppendLine("    , A01.PICKUPFEE AS PICKUPFEE")                              '集荷料
        SQLBldr.AppendLine("    , A01.DELIVERYFEE AS DELIVERYFEE")                          '配達料
        SQLBldr.AppendLine("    , A01.OTHER1FEE AS OTHER1FEE")                              'その他１
        SQLBldr.AppendLine("    , A01.OTHER2FEE AS OTHER2FEE")                              'その他２
        SQLBldr.AppendLine("    , A01.TORICODE AS TORICODE")                                '取引先コード
        SQLBldr.AppendLine("    , CASE WHEN A01.STACKFREEKBN = '1' THEN A10.TORINAME ")
        SQLBldr.AppendLine("           WHEN A01.STACKFREEKBN = '2' THEN A25.TORINAME END AS TORINAME") '取引先名称
        SQLBldr.AppendLine("    , CASE WHEN A01.STACKFREEKBN = '1' THEN A10.TORINAME")
        SQLBldr.AppendLine("           WHEN A01.STACKFREEKBN = '2' THEN A25.TORINAME END AS TORINAMEDISP") '取引先名称(一覧用)
        SQLBldr.AppendLine("    , A01.INVSUBCD AS INVSUBCD")                                '細分コード
        SQLBldr.AppendLine("    , CASE A01.STACKFREEKBN")
        SQLBldr.AppendLine("      WHEN '1' THEN A21.NAME ELSE A23.NAME")
        SQLBldr.AppendLine("      END AS KEIJOBRCNAME")                                     '計上支店名
        SQLBldr.AppendLine("    , CASE A01.STACKFREEKBN")
        SQLBldr.AppendLine("      WHEN '1' THEN A22.NAME ELSE A24.NAME")
        SQLBldr.AppendLine("      END AS INVPAYBRCNAME")                                    '提出部店名
        SQLBldr.AppendLine("    , A01.ACCOUNTSTATUSKBN AS ACCOUNTSTATUSKBNCD")              '勘定科目用状態区分
        SQLBldr.AppendLine("    , A16.VALUE1 AS ACCOUNTSTATUSKBNNM")                        '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , A01.REFRIGERATIONFLG AS REFRIGERATIONFLG")                '冷蔵適合フラグ
        SQLBldr.AppendLine("    , A17.VALUE1 AS REFRIGERATIONFLGNM")                        '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , (CASE ")
        SQLBldr.AppendLine("       WHEN coalesce(A01.FIXEDFEE, 0) > 0 THEN A01.FIXEDFEE ELSE coalesce(A20.A20FIXEDFEE, 0) ")
        SQLBldr.AppendLine("       END) AS FIXEDFEE")                                       '固定使用料
        SQLBldr.AppendLine("    , A01.INCOMEADJUSTFEE AS INCOMEADJUSTFEE")                  '収入加減額
        SQLBldr.AppendLine("    , A01.TOTALINCOME AS TOTALINCOME")                          '収入合計
        SQLBldr.AppendLine("    , A01.COMMISSIONFEE AS COMMISSIONFEE")                      '手数料
        SQLBldr.AppendLine("    , A01.COSTADJUSTFEE AS COSTADJUSTFEE")                      '費用加減額
        SQLBldr.AppendLine("    , A01.TOTALCOST AS TOTALCOST")                              '費用合計
        SQLBldr.AppendLine("    , A01.APPLSTATUS AS APPLSTATUS")                            '申請状況
        SQLBldr.AppendLine("    , A13.VALUE1 AS APPLSTATUSNM")                              '申請状況名称
        SQLBldr.AppendLine("    , A01.APPLYMD AS APPLYMD")                                  '申請年月日
        SQLBldr.AppendLine("    , A01.APPLUSER AS APPLUSER")                                '申請者ユーザーＩＤ
        SQLBldr.AppendLine("    , A11.STAFFNAMES AS APPLUSERNM")                            '申請者
        SQLBldr.AppendLine("    , A01.CONFUPDYMD AS CONFUPDYMD")                            '確認／修正年月日
        SQLBldr.AppendLine("    , A01.CONFUPDUSER AS CONFUPDUSER")                          '確認／修正者ユーザID
        SQLBldr.AppendLine("    , A18.STAFFNAMES AS CONFUPDUSERNM")                         '確認／修正者
        SQLBldr.AppendLine("    , A01.UPDCAUSE AS UPDCAUSE")                                '修正理由
        SQLBldr.AppendLine("    , A01.APPROVALYMD AS APPROVALYMD")                          '承認年月日
        SQLBldr.AppendLine("    , A01.APPROVALUSER AS APPROVALUSER")                        '承認者ユーザーＩＤ
        SQLBldr.AppendLine("    , A12.STAFFNAMES AS APPROVALUSERNM")                        '承認者
        SQLBldr.AppendLine("    , A01.APPROVALCAUSE AS APPROVALCAUSE")                      '承認／差戻し理由
        SQLBldr.AppendLine("    , A19.BIGCTNCD AS BIGCTNCD")                                '大分類コード
        SQLBldr.AppendLine("    , A19.MIDDLECTNCD AS MIDDLECTNCD")                          '中分類コード
        SQLBldr.AppendLine("    , A01.STACKFREEKBN AS STACKFREEKBN")                        '積空区分
        SQLBldr.AppendLine("    , A01.KEIJOYM AS BEF_KEIJOYM")                              '計上年月(変更前)
        SQLBldr.AppendLine("    , A01.TORICODE AS BEF_TORICODE")                            '取引先コード(変更前)
        SQLBldr.AppendLine("    , A01.INVFILINGDEPT AS BEF_INVFILINGDEPT")                  '請求項目 請求書提出部店(変更前)
        SQLBldr.AppendLine("    , A01.PAYFILINGBRANCH AS BEF_PAYFILINGBRANCH")              '支払項目 支払書提出支店(変更前)
        SQLBldr.AppendLine("    , A01.SCHEDATEPAYMENT AS BEF_SCHEDATEPAYMENT")              '入金予定日(変更前)
        SQLBldr.AppendLine("    , A01.STACKFREEKBN AS BEF_STACKFREEKBN")                    '積空区分(変更前)
        SQLBldr.AppendLine("    , A01.ACCOUNTSTATUSKBN AS BEF_ACCOUNTSTATUSKBN")            '勘定科目用状態区分(変更前)
        SQLBldr.AppendLine("    , A01.INVSUBCD AS BEF_INVSUBCD")                            '細分コード(変更前)
        SQLBldr.AppendLine("    , A01.USEFEE AS BEF_USEFEE")                                '使用料(変更前)
        SQLBldr.AppendLine("    , A01.NITTSUFREESEND AS BEF_NITTSUFREESEND")                '通運負担運賃(変更前)
        SQLBldr.AppendLine("    , A01.MANAGEFEE AS BEF_MANAGEFEE")                          '運行管理料(変更前)
        SQLBldr.AppendLine("    , A01.SHIPBURDENFEE AS BEF_SHIPBURDENFEE")                  '荷主負担運賃(変更前)
        SQLBldr.AppendLine("    , A01.PICKUPFEE AS BEF_PICKUPFEE")                          '集荷料(変更前)
        SQLBldr.AppendLine("    , A01.INCOMEADJUSTFEE AS BEF_INCOMEADJUSTFEE")              '収入加減額(変更前)
        SQLBldr.AppendLine("    , A01.TOTALINCOME AS BEF_TOTALINCOME")                      '収入合計(変更前)
        SQLBldr.AppendLine("    , A01.FREESENDFEE AS BEF_FREESENDFEE")                      '回送運賃(変更前)
        SQLBldr.AppendLine("    , A01.SHIPFEE AS BEF_SHIPFEE")                              '発送料(変更前)
        SQLBldr.AppendLine("    , A01.COMMISSIONFEE AS BEF_COMMISSIONFEE")                  '手数料(変更前)
        SQLBldr.AppendLine("    , A01.COSTADJUSTFEE AS BEF_COSTADJUSTFEE")                  '費用加減額(変更前)
        SQLBldr.AppendLine("    , A01.TOTALCOST AS BEF_TOTALCOST")                          '費用合計(変更前)
        '変更前
        SQLBldr.AppendLine("    , FIR.DEPTRUSTEECD AS DEPTRUSTEECD_B")                      '発受託人コード
        SQLBldr.AppendLine("    , B02.DEPTRUSTEENM AS DEPTRUSTEENM_B")                      '発受託人名称
        SQLBldr.AppendLine("	, FIR.DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD_B")                '発受託人サブコード
        SQLBldr.AppendLine("	, B02.DEPTRUSTEESUBNM AS DEPTRUSTEESUBNM_B")                '発受託人サブ名称
        SQLBldr.AppendLine("    , FIR.JOTDEPBRANCHCD AS JOTDEPBRANCHCD_B")                  'JOT発組織コード
        SQLBldr.AppendLine("    , B14.NAMES AS JOTDEPBRANCHNM_B")                           'JOT発組織名
        SQLBldr.AppendLine("    , FIR.DEPSTATION AS DEPSTATIONCD_B")                        '発駅コード
        SQLBldr.AppendLine("    , B03.NAMES AS DEPSTATIONNM_B")                             '発駅名称
        SQLBldr.AppendLine("    , FIR.SHIPYMD AS SHIPYMD_B")                                '発送年月日
        SQLBldr.AppendLine("    , FIR.STATUSKBN AS STATUSKBNCD_B")                          '状態区分
        SQLBldr.AppendLine("    , FIR.JRITEMCD AS ITEMCODE_B")                              '品目コード
        SQLBldr.AppendLine("    , B07.NAME AS ITEMNAME_B")                                  '品目名称
        SQLBldr.AppendLine("    , FIR.JRITEMNM AS JRITEMNM_B")                              'JR品目名
        SQLBldr.AppendLine("    , FIR.CTNTYPE AS CTNTYPE_B")                                'コンテナ記号
        SQLBldr.AppendLine("    , FIR.CTNNO AS CTNNO_B")                                    'コンテナ番号
        SQLBldr.AppendLine("    , FIR.SAMEDAYCNT AS SAMEDAYCNT_B")                          '同日内回数
        SQLBldr.AppendLine("    , FIR.CTNLINENO AS CTNLINENO_B")                            '行番
        SQLBldr.AppendLine("    , FIR.SPRFITKBN AS SPRFITKBNCD_B")                          '冷蔵区分
        SQLBldr.AppendLine("    , B08.VALUE1 AS SPRFITKBNNM_B")                             '冷蔵名称
        SQLBldr.AppendLine("    , FIR.ARRTRUSTEECD AS ARRTRUSTEECD_B")                      '着受託人コード
        SQLBldr.AppendLine("    , coalesce(B04.DEPTRUSTEENM, B042.DEPTRUSTEENM) AS ARRTRUSTEENM_B")  '着受託人名称
        SQLBldr.AppendLine("	, FIR.ARRTRUSTEESUBCD AS ARRTRUSTEESUBCD_B")                '着受託人サブコード
        SQLBldr.AppendLine("	, B04.DEPTRUSTEESUBNM AS ARRTRUSTEESUBNM_B")                '着受託人サブ名称
        SQLBldr.AppendLine("    , FIR.ARRPLANYMD AS ARRPLANYMD_B")                          '到着年月日
        SQLBldr.AppendLine("    , FIR.JOTARRBRANCHCD AS JOTARRBRANCHCD_B")                  'JOT着組織コード
        SQLBldr.AppendLine("    , B15.NAMES AS JOTARRBRANCHNM_B")                           'JOT発組織名
        SQLBldr.AppendLine("    , FIR.ARRSTATION AS ARRSTATIONCD_B")                        '着駅コード
        SQLBldr.AppendLine("    , B05.NAMES AS ARRSTATIONNM_B")                             '着駅名称
        SQLBldr.AppendLine("    , coalesce(FIR.USEFEE,0) AS USEFEE_B")                        '使用料
        SQLBldr.AppendLine("    , coalesce(FIR.FREESENDFEE,0) AS FREESENDFEE_B")              '回送運賃
        SQLBldr.AppendLine("    , FIR.KEIJOYM AS KEIJOYM_B")                                '計上年月
        SQLBldr.AppendLine("    , FIR.CONTRACTCD AS CONTRACTCD_B")                          '契約コード
        SQLBldr.AppendLine("    , FIR.DEPSHIPPERCD AS DEPSHIPPERCD_B")                      '発荷主コード
        SQLBldr.AppendLine("    , B09.NAME AS DEPSHIPPERNM_B")                              '発荷主名称
        SQLBldr.AppendLine("    , FIR.DEPTRAINNO AS DEPTRAINNO_B")                          '発列車番号
        SQLBldr.AppendLine("    , FIR.QUANTITY AS QUANTITY_B")                              '個数
        SQLBldr.AppendLine("    , FIR.ARRTRAINNO AS ARRTRAINNO_B")                          '着列車番号
        SQLBldr.AppendLine("    , FIR.NITTSUFREESEND AS NITTSUFREESEND_B")                  '通運負担運賃
        SQLBldr.AppendLine("    , FIR.MANAGEFEE AS MANAGEFEE_B")                            '運行管理料
        SQLBldr.AppendLine("    , FIR.JRFIXEDFARE AS JRFIXEDFARE_B")                        'JR所定運賃
        SQLBldr.AppendLine("    , FIR.OWNDISCOUNTFEE AS OWNDISCOUNTFEE_B")                  '私有割引相当額
        SQLBldr.AppendLine("    , FIR.RETURNFARE AS RETURNFARE_B")                          '割戻し運賃
        SQLBldr.AppendLine("    , FIR.SHIPBURDENFEE AS SHIPBURDENFEE_B")                    '荷主負担運賃
        SQLBldr.AppendLine("    , FIR.SHIPFEE AS SHIPFEE_B")                                '発送料
        SQLBldr.AppendLine("    , FIR.ARRIVEFEE AS ARRIVEFEE_B")                            '到着料
        SQLBldr.AppendLine("    , FIR.PICKUPFEE AS PICKUPFEE_B")                            '集荷料
        SQLBldr.AppendLine("    , FIR.DELIVERYFEE AS DELIVERYFEE_B")                        '配達料
        SQLBldr.AppendLine("    , FIR.OTHER1FEE AS OTHER1FEE_B")                            'その他１
        SQLBldr.AppendLine("    , FIR.OTHER2FEE AS OTHER2FEE_B")                            'その他２
        SQLBldr.AppendLine("    , FIR.TORICODE AS TORICODE_B")                              '取引先コード
        SQLBldr.AppendLine("    , CASE WHEN FIR.STACKFREEKBN = '1' THEN  B10.TORINAME")
        SQLBldr.AppendLine("           WHEN FIR.STACKFREEKBN = '2' THEN  B24.TORINAME END AS TORINAME_B") '取引先名称
        SQLBldr.AppendLine("    , FIR.INVSUBCD AS INVSUBCD_B")                              '細分コード
        SQLBldr.AppendLine("    , CASE FIR.STACKFREEKBN")
        SQLBldr.AppendLine("      WHEN '1' THEN B20.NAME ELSE B22.NAME")
        SQLBldr.AppendLine("      END AS KEIJOBRCNAME_B")                                   '計上支店名
        SQLBldr.AppendLine("    , CASE FIR.STACKFREEKBN")
        SQLBldr.AppendLine("      WHEN '1' THEN B21.NAME ELSE B23.NAME")
        SQLBldr.AppendLine("      END AS INVPAYBRCNAME_B")                                  '提出部店名
        SQLBldr.AppendLine("    , FIR.ACCOUNTSTATUSKBN AS ACCOUNTSTATUSKBNCD_B")            '勘定科目用状態区分
        SQLBldr.AppendLine("    , B16.VALUE1 AS ACCOUNTSTATUSKBNNM_B")                      '勘定科目用状態区分名称
        SQLBldr.AppendLine("    , FIR.REFRIGERATIONFLG AS REFRIGERATIONFLG_B")              '冷蔵適合フラグ
        SQLBldr.AppendLine("    , B17.VALUE1 AS REFRIGERATIONFLGNM_B")                      '冷蔵適合フラグ名称
        SQLBldr.AppendLine("    , FIR.FIXEDFEE AS FIXEDFEE_B")                              '固定使用料
        SQLBldr.AppendLine("    , FIR.INCOMEADJUSTFEE AS INCOMEADJUSTFEE_B")                '収入加減額
        SQLBldr.AppendLine("    , FIR.TOTALINCOME AS TOTALINCOME_B")                        '収入合計
        SQLBldr.AppendLine("    , FIR.COMMISSIONFEE AS COMMISSIONFEE_B")                    '手数料
        SQLBldr.AppendLine("    , FIR.COSTADJUSTFEE AS COSTADJUSTFEE_B")                    '費用加減額
        SQLBldr.AppendLine("    , FIR.TOTALCOST AS TOTALCOST_B")                            '費用合計
        SQLBldr.AppendLine("    , FIR.APPLSTATUS AS APPLSTATUS_B")                          '申請状況
        SQLBldr.AppendLine("    , B19.BIGCTNCD AS BIGCTNCD_B")                              '大分類コード
        SQLBldr.AppendLine("    , B19.MIDDLECTNCD AS MIDDLECTNCD_B")                        '中分類コード
        SQLBldr.AppendLine("    , FIR.STACKFREEKBN AS STACKFREEKBN_B")                      '積空区分
        SQLBldr.AppendLine("    , '1' AS ROW_SEL")                                          '申請状況
        SQLBldr.AppendLine("    , SEL_RF.GETLINENO AS GETLINENO")                           'LINENO
        SQLBldr.AppendLine("    , A01.INITUSER AS INITUSER")                                '登録ユーザ
        SQLBldr.AppendLine("    , INITUSERORG.ORG AS INITUSERORG")                          '登録ユーザORG

        SQLBldr.AppendLine("FROM")
        'メイン コンテナ清算ファイル
        SQLBldr.AppendLine("    lng.LNT0017_RESSNF A01")
        'コンテナ精算ファイル(ページ制御用)
        SQLBldr.AppendLine("    INNER JOIN lng.LNT0073_RESSNF_SEL SEL_RF")
        SQLBldr.AppendLine("        ON SEL_RF.SHIPYMD = A01.SHIPYMD")
        SQLBldr.AppendLine("        AND SEL_RF.CTNTYPE = A01.CTNTYPE")
        SQLBldr.AppendLine("		AND SEL_RF.CTNNO = A01.CTNNO")
        SQLBldr.AppendLine("		AND SEL_RF.SAMEDAYCNT = A01.SAMEDAYCNT")
        SQLBldr.AppendLine("		AND SEL_RF.CTNLINENO = A01.CTNLINENO")
        SQLBldr.AppendLine("		AND SEL_RF.USERID = @P04")
        'SQLBldr.AppendLine("		AND SEL_RF.GETLINENO >= @P05")
        'コンテナ取引先マスタ(発受託人)
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0003_REKEJM A02")
        SQLBldr.AppendLine("        ON A02.DEPSTATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A02.DEPTRUSTEECD = A01.DEPTRUSTEECD")
        SQLBldr.AppendLine("		AND A02.DEPTRUSTEESUBCD = A01.DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("        AND A02.DELFLG = @P01")
        '駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A03")
        SQLBldr.AppendLine("        ON A03.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A03.ORGCODE = A01.JOTDEPBRANCHCD")
        SQLBldr.AppendLine("        AND A03.STATION = A01.DEPSTATION")
        SQLBldr.AppendLine("        AND A03.DELFLG = @P01")
        'コンテナ取引先マスタ(着受託人)
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0003_REKEJM A04")
        SQLBldr.AppendLine("        ON A04.DEPSTATION = A01.ARRSTATION")
        SQLBldr.AppendLine("        AND A04.DEPTRUSTEECD = A01.ARRTRUSTEECD")
        SQLBldr.AppendLine("		AND A04.DEPTRUSTEESUBCD = A01.ARRTRUSTEESUBCD")
        SQLBldr.AppendLine("        AND A04.DELFLG = @P01")
        'コンテナ取引先マスタ(着受託人)(サブなし)
        SQLBldr.AppendLine("    LEFT JOIN ")
        SQLBldr.AppendLine("    (")
        SQLBldr.AppendLine("     SELECT")
        SQLBldr.AppendLine("        MAIN.DEPSTATION, MAIN.DEPTRUSTEECD, MAIN.DEPTRUSTEESUBCD, MAIN.DEPTRUSTEENM")
        SQLBldr.AppendLine("     FROM")
        SQLBldr.AppendLine("         lng.LNM0003_REKEJM MAIN")
        SQLBldr.AppendLine("     INNER JOIN")
        SQLBldr.AppendLine("         (")
        SQLBldr.AppendLine("          SELECT")
        SQLBldr.AppendLine("              DEPSTATION, DEPTRUSTEECD, MIN(DEPTRUSTEESUBCD) DEPTRUSTEESUBCD ")
        SQLBldr.AppendLine("          FROM")
        SQLBldr.AppendLine("              lng.LNM0003_REKEJM")
        SQLBldr.AppendLine("          GROUP BY")
        SQLBldr.AppendLine("              DEPSTATION, DEPTRUSTEECD")
        SQLBldr.AppendLine("          ) SUB")
        SQLBldr.AppendLine("         ON MAIN.DEPSTATION = SUB.DEPSTATION ")
        SQLBldr.AppendLine("         AND MAIN.DEPTRUSTEECD = SUB.DEPTRUSTEECD ")
        SQLBldr.AppendLine("         AND MAIN.DEPTRUSTEESUBCD = SUB.DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("         AND MAIN.DELFLG = @P01")
        SQLBldr.AppendLine("     ) A042")
        SQLBldr.AppendLine("        ON A042.DEPSTATION = A01.ARRSTATION")
        SQLBldr.AppendLine("        AND A042.DEPTRUSTEECD = A01.ARRTRUSTEECD")
        '駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION A05")
        SQLBldr.AppendLine("        ON A05.STATION = A01.ARRSTATION")
        SQLBldr.AppendLine("        AND A05.DELFLG = @P01")
        '固定値マスタ(状態区分)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE A06")
        SQLBldr.AppendLine("        ON A06.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A06.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A06.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A06.CLASS = 'OPERATIONKBN'")
        SQLBldr.AppendLine("        AND A06.KEYCODE = CONVERT(NVARCHAR,A01.STATUSKBN)")
        SQLBldr.AppendLine("        AND A06.DELFLG = @P01")
        '品目マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0021_ITEM A07")
        SQLBldr.AppendLine("        ON A07.ITEMCD = A01.JRITEMCD")
        SQLBldr.AppendLine("        AND A07.DELFLG = @P01")
        '固定値マスタ(冷蔵区分)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE A08")
        SQLBldr.AppendLine("        ON A08.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A08.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A08.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A08.CLASS = 'SPRFITKBN'")
        SQLBldr.AppendLine("        AND A08.KEYCODE = CONVERT(NVARCHAR, A01.SPRFITKBN)")
        SQLBldr.AppendLine("        AND A08.DELFLG = @P01")
        '荷主マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0023_SHIPPER A09")
        SQLBldr.AppendLine("        ON A09.SHIPPERCD = A01.DEPSHIPPERCD")
        SQLBldr.AppendLine("        AND A09.DELFLG = @P01")
        '営業収入決済条件マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0024_KEKKJM A10")
        SQLBldr.AppendLine("        ON A10.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("        AND A10.INVFILINGDEPT = A01.INVFILINGDEPT")
        SQLBldr.AppendLine("        AND A10.INVKESAIKBN = A01.INVKESAIKBN")
        SQLBldr.AppendLine("        AND A10.DELFLG = @P01")
        'ユーザマスタ(申請者)
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A11")
        SQLBldr.AppendLine("        ON A11.USERID = A01.APPLUSER")
        SQLBldr.AppendLine("        AND A11.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A11.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A11.DELFLG = @P01")
        'ユーザマスタ(承認者)
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A12")
        SQLBldr.AppendLine("        ON A12.USERID = A01.APPROVALUSER")
        SQLBldr.AppendLine("        AND A12.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A12.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A12.DELFLG = @P01")
        '固定値マスタ(申請状況)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE A13")
        SQLBldr.AppendLine("        ON A13.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A13.CLASS = 'RESSNFSTATUS'")
        SQLBldr.AppendLine("        AND A13.KEYCODE = CONVERT(NVARCHAR,A01.APPLSTATUS)")
        SQLBldr.AppendLine("        AND A13.DELFLG = @P01")
        '組織マスタ（JOT発組織）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A14")
        SQLBldr.AppendLine("    ON A14.ORGCODE = A01.JOTDEPBRANCHCD")
        SQLBldr.AppendLine("    AND A14.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A14.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A14.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A14.ENDYMD >= @P03")
        '組織マスタ（JOT着組織）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A15")
        SQLBldr.AppendLine("    ON A15.ORGCODE = A01.JOTARRBRANCHCD")
        SQLBldr.AppendLine("    AND A15.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A15.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A15.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A15.ENDYMD >= @P03")
        '固定値マスタ(勘定科目用状態区分)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE A16")
        SQLBldr.AppendLine("        ON A16.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A16.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A16.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A16.CLASS = 'ACCOUNTSTATUSKBN'")
        SQLBldr.AppendLine("        AND A16.KEYCODE = CONVERT(NVARCHAR,A01.ACCOUNTSTATUSKBN)")
        SQLBldr.AppendLine("        AND A16.DELFLG = @P01")
        '固定値マスタ(冷蔵適合フラグ)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE A17")
        SQLBldr.AppendLine("        ON A17.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND A17.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A17.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A17.CLASS = 'REFRIGERATIONFLG'")
        SQLBldr.AppendLine("        AND A17.KEYCODE = CONVERT(NVARCHAR, A01.REFRIGERATIONFLG)")
        SQLBldr.AppendLine("        AND A17.DELFLG = @P01")
        'ユーザマスタ(承認者)
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user A18")
        SQLBldr.AppendLine("        ON A18.USERID = A01.CONFUPDUSER")
        SQLBldr.AppendLine("        AND A18.STYMD <= @P03")
        SQLBldr.AppendLine("        AND A18.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND A18.DELFLG = @P01")
        'コンテナマスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0002_RECONM A19")
        SQLBldr.AppendLine("        ON A19.CTNTYPE = A01.CTNTYPE")
        SQLBldr.AppendLine("        AND A19.CTNNO = A01.CTNNO")
        SQLBldr.AppendLine("        AND A19.DELFLG = @P01")
        '固定使用料マスタ
        SQLBldr.AppendLine("    LEFT JOIN (SELECT TOP 1 ")
        SQLBldr.AppendLine("                   A20SEI.SHIPYMD SHIPYMD")
        SQLBldr.AppendLine("                 , A20SEI.CTNTYPE CTNTYPE")
        SQLBldr.AppendLine("                 , A20SEI.CTNNO CTNNO")
        SQLBldr.AppendLine("                 , A20MAIN.DEPSTATION A20DEPSTATION")
        SQLBldr.AppendLine("                 , A20MAIN.DEPTRUSTEECD A20DEPTRUSTEECD")
        SQLBldr.AppendLine("                 , A20MAIN.DEPTRUSTEESUBCD A20DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("                 , A20MAIN.FIXEDFEE A20FIXEDFEE")
        SQLBldr.AppendLine("               FROM ")
        SQLBldr.AppendLine("                   lng.LNM0028_FIXEDFEE A20MAIN")
        SQLBldr.AppendLine("               LEFT JOIN lng.LNT0017_RESSNF A20SEI")
        SQLBldr.AppendLine("                   ON A20MAIN.DEPSTATION = A20SEI.DEPSTATION")
        SQLBldr.AppendLine("                   AND A20MAIN.DEPTRUSTEECD = A20SEI.DEPTRUSTEECD")
        SQLBldr.AppendLine("                   AND A20MAIN.DEPTRUSTEESUBCD = A20SEI.DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("                   AND A20SEI.SHIPYMD >= A20MAIN.APPLICABLESTARTDATE")
        SQLBldr.AppendLine("                   AND A20SEI.SHIPYMD <= A20MAIN.APPLICABLEENDDATE")
        SQLBldr.AppendLine("               WHERE ")
        SQLBldr.AppendLine("                   A20MAIN.DELFLG = @P01")
        SQLBldr.AppendLine("              ) A20")
        SQLBldr.AppendLine("        ON A20.SHIPYMD = A01.SHIPYMD")
        SQLBldr.AppendLine("        AND A20.CTNTYPE = A01.CTNTYPE")
        SQLBldr.AppendLine("        AND A20.CTNNO = A01.CTNNO")
        '組織マスタ（請求項目 計上店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A21")
        SQLBldr.AppendLine("    ON A21.ORGCODE = A02.INVKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    AND A21.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A21.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A21.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A21.ENDYMD >= @P03")
        '組織マスタ（請求項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A22")
        SQLBldr.AppendLine("    ON A22.ORGCODE = A02.INVFILINGDEPT")
        SQLBldr.AppendLine("    AND A22.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A22.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A22.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A22.ENDYMD >= @P03")
        '組織マスタ（支払項目 計上店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A23")
        SQLBldr.AppendLine("    ON A23.ORGCODE = A02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    AND A23.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A23.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A23.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A23.ENDYMD >= @P03")
        '組織マスタ（支払項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG A24")
        SQLBldr.AppendLine("    ON A24.ORGCODE = A02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("    AND A24.DELFLG = @P01")
        SQLBldr.AppendLine("    AND A24.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND A24.STYMD <= @P03")
        SQLBldr.AppendLine("    AND A24.ENDYMD >= @P03")
        '組織マスタ（支払項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     lng.LNT0072_PAYEE A25")
        SQLBldr.AppendLine("    ON  A25.TORICODE = A01.TORICODE")
        SQLBldr.AppendLine("    AND A25.DELFLG = @P01")

        '変更前
        'コンテナ精算ファイル(初回状態) 変更前用
        SQLBldr.AppendLine("    LEFT JOIN lng.LNT0070_RESSNF_INIT FIR")
        SQLBldr.AppendLine("        ON A01.SHIPYMD = FIR.SHIPYMD")
        SQLBldr.AppendLine("        AND A01.CTNTYPE = FIR.CTNTYPE")
        SQLBldr.AppendLine("        AND A01.CTNNO = FIR.CTNNO")
        SQLBldr.AppendLine("        AND A01.SAMEDAYCNT = FIR.SAMEDAYCNT")
        SQLBldr.AppendLine("        AND A01.CTNLINENO = FIR.CTNLINENO")
        'コンテナ取引先マスタ(発受託人)
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0003_REKEJM B02")
        SQLBldr.AppendLine("        ON B02.DEPSTATION = FIR.DEPSTATION")
        SQLBldr.AppendLine("        AND B02.DEPTRUSTEECD = FIR.DEPTRUSTEECD")
        SQLBldr.AppendLine("		AND B02.DEPTRUSTEESUBCD = FIR.DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("        AND B02.DELFLG = @P01")
        '駅マスタ(発駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION B03")
        SQLBldr.AppendLine("        ON B03.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND B03.ORGCODE = FIR.JOTDEPBRANCHCD")
        SQLBldr.AppendLine("        AND B03.STATION = FIR.DEPSTATION")
        SQLBldr.AppendLine("        AND B03.DELFLG = @P01")
        'コンテナ取引先マスタ(着受託人)
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0003_REKEJM B04")
        SQLBldr.AppendLine("        ON B04.DEPSTATION = FIR.ARRSTATION")
        SQLBldr.AppendLine("        AND B04.DEPTRUSTEECD = FIR.ARRTRUSTEECD")
        SQLBldr.AppendLine("		AND B04.DEPTRUSTEESUBCD = FIR.ARRTRUSTEESUBCD")
        SQLBldr.AppendLine("        AND B04.DELFLG = @P01")
        'コンテナ取引先マスタ(着受託人)(サブなし)
        SQLBldr.AppendLine("    LEFT JOIN ")
        SQLBldr.AppendLine("    (")
        SQLBldr.AppendLine("     SELECT")
        SQLBldr.AppendLine("        MAIN.DEPSTATION, MAIN.DEPTRUSTEECD, MAIN.DEPTRUSTEESUBCD, MAIN.DEPTRUSTEENM")
        SQLBldr.AppendLine("     FROM")
        SQLBldr.AppendLine("         lng.LNM0003_REKEJM MAIN")
        SQLBldr.AppendLine("     INNER JOIN")
        SQLBldr.AppendLine("         (")
        SQLBldr.AppendLine("          SELECT")
        SQLBldr.AppendLine("              DEPSTATION, DEPTRUSTEECD, MIN(DEPTRUSTEESUBCD) DEPTRUSTEESUBCD ")
        SQLBldr.AppendLine("          FROM")
        SQLBldr.AppendLine("              lng.LNM0003_REKEJM")
        SQLBldr.AppendLine("          GROUP BY")
        SQLBldr.AppendLine("              DEPSTATION, DEPTRUSTEECD")
        SQLBldr.AppendLine("          ) SUB")
        SQLBldr.AppendLine("         ON MAIN.DEPSTATION = SUB.DEPSTATION ")
        SQLBldr.AppendLine("         AND MAIN.DEPTRUSTEECD = SUB.DEPTRUSTEECD ")
        SQLBldr.AppendLine("         AND MAIN.DEPTRUSTEESUBCD = SUB.DEPTRUSTEESUBCD")
        SQLBldr.AppendLine("         AND MAIN.DELFLG = @P01")
        SQLBldr.AppendLine("     ) B042")
        SQLBldr.AppendLine("        ON B042.DEPSTATION = FIR.ARRSTATION")
        SQLBldr.AppendLine("        AND B042.DEPTRUSTEECD = FIR.ARRTRUSTEECD")
        '駅マスタ(着駅)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0020_STATION B05")
        SQLBldr.AppendLine("        ON B05.STATION = FIR.ARRSTATION")
        SQLBldr.AppendLine("        AND B05.DELFLG = @P01")
        '品目マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0021_ITEM B07")
        SQLBldr.AppendLine("        ON B07.ITEMCD = FIR.JRITEMCD")
        SQLBldr.AppendLine("        AND B07.DELFLG = @P01")
        '固定値マスタ(冷蔵区分)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE B08")
        SQLBldr.AppendLine("        ON B08.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND B08.STYMD <= @P03")
        SQLBldr.AppendLine("        AND B08.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND B08.CLASS = 'SPRFITKBN'")
        SQLBldr.AppendLine("        AND B08.KEYCODE = CONVERT(NVARCHAR, FIR.SPRFITKBN)")
        SQLBldr.AppendLine("        AND B08.DELFLG = @P01")
        '荷主マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0023_SHIPPER B09")
        SQLBldr.AppendLine("        ON B09.SHIPPERCD = FIR.DEPSHIPPERCD")
        SQLBldr.AppendLine("        AND B09.DELFLG = @P01")
        '営業収入決済条件マスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0024_KEKKJM B10")
        SQLBldr.AppendLine("        ON B10.TORICODE = FIR.TORICODE")
        SQLBldr.AppendLine("        AND B10.INVFILINGDEPT = FIR.INVFILINGDEPT")
        SQLBldr.AppendLine("        AND B10.INVKESAIKBN = FIR.INVKESAIKBN")
        SQLBldr.AppendLine("        AND B10.DELFLG = @P01")
        '組織マスタ（JOT発組織）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B14")
        SQLBldr.AppendLine("    ON B14.ORGCODE = FIR.JOTDEPBRANCHCD")
        SQLBldr.AppendLine("    AND B14.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B14.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B14.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B14.ENDYMD >= @P03")
        '組織マスタ（JOT着組織）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B15")
        SQLBldr.AppendLine("    ON B15.ORGCODE = FIR.JOTARRBRANCHCD")
        SQLBldr.AppendLine("    AND B15.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B15.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B15.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B15.ENDYMD >= @P03")
        '固定値マスタ(勘定科目用状態区分)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE B16")
        SQLBldr.AppendLine("        ON B16.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND B16.STYMD <= @P03")
        SQLBldr.AppendLine("        AND B16.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND B16.CLASS = 'ACCOUNTSTATUSKBN'")
        SQLBldr.AppendLine("        AND B16.KEYCODE = CONVERT(NVARCHAR, FIR.ACCOUNTSTATUSKBN)")
        SQLBldr.AppendLine("        AND B16.DELFLG = @P01")
        '固定値マスタ(冷蔵適合フラグ)
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0006_FIXVALUE B17")
        SQLBldr.AppendLine("        ON B17.CAMPCODE = @P02")
        SQLBldr.AppendLine("        AND B17.STYMD <= @P03")
        SQLBldr.AppendLine("        AND B17.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND B17.CLASS = 'REFRIGERATIONFLG'")
        SQLBldr.AppendLine("        AND B17.KEYCODE = CONVERT(NVARCHAR, FIR.REFRIGERATIONFLG)")
        SQLBldr.AppendLine("        AND B17.DELFLG = @P01")
        'コンテナマスタ
        SQLBldr.AppendLine("    LEFT JOIN lng.LNM0002_RECONM B19")
        SQLBldr.AppendLine("        ON B19.CTNTYPE = FIR.CTNTYPE")
        SQLBldr.AppendLine("        AND B19.CTNNO = FIR.CTNNO")
        SQLBldr.AppendLine("        AND B19.DELFLG = @P01")
        'ユーザマスタ(登録ユーザ)
        SQLBldr.AppendLine("    LEFT JOIN com.lns0001_user INITUSERORG")
        SQLBldr.AppendLine("        ON INITUSERORG.USERID = A01.INITUSER")
        SQLBldr.AppendLine("        AND INITUSERORG.STYMD <= @P03")
        SQLBldr.AppendLine("        AND INITUSERORG.ENDYMD >= @P03")
        SQLBldr.AppendLine("        AND INITUSERORG.DELFLG = @P01")
        '組織マスタ（請求項目 計上店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B20")
        SQLBldr.AppendLine("    ON B20.ORGCODE = B02.INVKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    AND B20.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B20.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B20.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B20.ENDYMD >= @P03")
        '組織マスタ（請求項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B21")
        SQLBldr.AppendLine("    ON B21.ORGCODE = B02.INVFILINGDEPT")
        SQLBldr.AppendLine("    AND B21.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B21.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B21.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B21.ENDYMD >= @P03")
        '組織マスタ（支払項目 計上店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B22")
        SQLBldr.AppendLine("    ON B22.ORGCODE = B02.PAYKEIJYOBRANCHCD")
        SQLBldr.AppendLine("    AND B22.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B22.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B22.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B22.ENDYMD >= @P03")
        '組織マスタ（支払項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG B23")
        SQLBldr.AppendLine("    ON B23.ORGCODE = B02.PAYFILINGBRANCH")
        SQLBldr.AppendLine("    AND B23.DELFLG = @P01")
        SQLBldr.AppendLine("    AND B23.CAMPCODE = @P02")
        SQLBldr.AppendLine("    AND B23.STYMD <= @P03")
        SQLBldr.AppendLine("    AND B23.ENDYMD >= @P03")
        '組織マスタ（支払項目 提出部店）
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     lng.LNT0072_PAYEE B24")
        SQLBldr.AppendLine("    ON  B24.TORICODE = FIR.TORICODE")
        SQLBldr.AppendLine("    AND B24.DELFLG = @P01")

        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("     A01.DELFLG = @P01")

        '' 一覧からの呼び出しの場合、参照権限ロール自支店の場合、他支店のレコードは除外する。不許可の場合は当処理は呼び出しを行わない。
        'If Not blnRessnfDetaliFlg AndAlso str_ref_role = CONST_ONLY_MY_DEPARTMENT Then
        '    SQLBldr.AppendLine(" AND ((INITUSERORG.ORG IS NULL AND A01.JOTDEPBRANCHCD = @P06)")
        '    SQLBldr.AppendLine("      OR (INITUSERORG.ORG IS NOT NULL AND INITUSERORG.ORG = @P06))")
        'End If

        '選択されているものを抽出条件に追加
        If dtSelPKEY IsNot Nothing AndAlso dtSelPKEY.Rows.Count > 0 Then
            Dim blnFirstFlg As Boolean = True
            SQLBldr.AppendLine("    AND (")
            For Each rowData As DataRow In dtSelPKEY.Rows
                If blnFirstFlg = True Then
                    blnFirstFlg = False
                Else
                    SQLBldr.AppendLine("        OR ")
                End If
                If rowData(SEL_RESSNF_PKEY.PK_SHIPYMD).ToString = "" Then
                    SQLBldr.AppendLine("        (")
                    SQLBldr.AppendLine("            A01.SHIPYMD = ''")
                    SQLBldr.AppendLine("        )")
                Else
                    SQLBldr.AppendLine("        (")
                    SQLBldr.AppendLine("            A01.SHIPYMD = '" & rowData(SEL_RESSNF_PKEY.PK_SHIPYMD).ToString & "'")
                    SQLBldr.AppendLine("        AND A01.CTNTYPE = '" & rowData(SEL_RESSNF_PKEY.PK_CTNTYPE).ToString & "'")
                    SQLBldr.AppendLine("        AND A01.CTNNO = " & rowData(SEL_RESSNF_PKEY.PK_CTNNO).ToString)
                    SQLBldr.AppendLine("        AND A01.SAMEDAYCNT = " & rowData(SEL_RESSNF_PKEY.PK_SAMEDAYCNT).ToString)
                    SQLBldr.AppendLine("        AND A01.CTNLINENO = " & rowData(SEL_RESSNF_PKEY.PK_CTNLINENO).ToString)
                    SQLBldr.AppendLine("        )")
                End If
            Next
            SQLBldr.AppendLine("    )")
        End If

        '並び順
        'SQLBldr.AppendLine("ORDER BY")
        'SQLBldr.AppendLine("    SEL_RF.GETLINENO")
        SQLBldr.AppendLine(") SORT ")
        'SQLBldr.AppendLine("WHERE ")
        'SQLBldr.AppendLine("		    GETLINENO >= @P05")
        'SQLBldr.AppendLine("ORDER BY")
        'If String.IsNullOrEmpty(sortSQL) Then
        '    SQLBldr.AppendLine("    GETLINENO")
        'Else
        '    SQLBldr.AppendLine(sortSQL)
        'End If

        SQLBldr.AppendLine(") SORT2 ")

        If Not blnRessnfDetaliFlg Then
            SQLBldr.AppendLine("WHERE ROWNUM >= @P05")
        End If
        SQLBldr.AppendLine("ORDER BY ROWNUM")

        'パラメータ設定
        With param
            .Add("@P01", C_DELETE_FLG.ALIVE)
            .Add("@P02", strCampCpde)
            .Add("@P03", Format(WW_DATENOW, "yyyy/MM/dd"))
            .Add("@P04", strUserID)
            .Add("@P05", intGetLineNo)
            .Add("@P06", strOrgCd)
        End With
        'SQL発行
        CS0050SESSION.GetDataTable(sqlCon, SQLBldr.ToString, param, dt, sqlTran)

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' 精算ファイル 同日内回数取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmShipYMD">発送年月日</param>
    ''' <param name="prmCtnType">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="refSameDayCnt">同日内回数</param>
    ''' <param name="refCtnineNo">行番</param>
    ''' <remarks>リース明細画面データの最新データを取得する</remarks>
    Public Shared Sub GetSeisanFSameDayCnt(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmShipYMD As String, ByVal prmCtnType As String, ByVal prmCtnNo As String,
                                           ByRef refSameDayCnt As String, ByRef refCtnineNo As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    SAMEDAYCNT, CTNLINENO")
            .AppendLine("FROM")
            'メイン 精算ファイル
            .AppendLine("     LNG.LNT0017_RESSNF")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("         SHIPYMD = @SHIPYMD")
            .AppendLine("     AND CTNTYPE = @CTNTYPE")
            .AppendLine("     AND CTNNO   = @CTNNO")
            '並び順
            .AppendLine(" ORDER BY")
            .AppendLine("     SAMEDAYCNT DESC, CTNLINENO DESC")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@SHIPYMD", prmShipYMD)
            .Add("@CTNTYPE", prmCtnType)
            .Add("@CTNNO", prmCtnNo)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refSameDayCnt = GetStringValue(sqlRetSet, 0, "SAMEDAYCNT")
            refCtnineNo = GetStringValue(sqlRetSet, 0, "CTNLINENO")
        Else
            refSameDayCnt = "0"
            refCtnineNo = "0"
        End If

    End Sub

    ''' <summary>
    ''' 精算ファイル 申請状況取得処理
    ''' </summary>
    ''' <param name="prmShipYMD">発送年月日</param>
    ''' <param name="prmCtnType">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="prmSameDayCnt">同日内回数</param>
    ''' <param name="prmCtnineNo">行番</param>
    ''' <remarks>リース明細画面データの最新データを取得する</remarks>
    Public Shared Function GetSeisanFSinsei(ByVal prmShipYMD As String, ByVal prmCtnType As String, ByVal prmCtnNo As String,
                                           ByVal prmSameDayCnt As String, ByVal prmCtnineNo As String) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0
        Dim strApplStatus As String = "0"

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            With sqlText
                .AppendLine("SELECT ")
                .AppendLine("     APPLSTATUS AS APPLSTATUS")
                .AppendLine("FROM")
                'メイン 精算ファイル
                .AppendLine("     LNG.LNT0017_RESSNF")
                '抽出条件
                .AppendLine(" WHERE")
                .AppendLine("         SHIPYMD = @SHIPYMD")
                .AppendLine("     AND CTNTYPE = @CTNTYPE")
                .AppendLine("     AND CTNNO   = @CTNNO")
                .AppendLine("     AND SAMEDAYCNT = @SAMEDAYCNT")
                .AppendLine("     AND CTNLINENO = @CTNLINENO")
            End With

            'パラメータ設定
            With sqlParam
                .Add("@SHIPYMD", prmShipYMD)
                .Add("@CTNTYPE", prmCtnType)
                .Add("@CTNNO", prmCtnNo)
                .Add("@SAMEDAYCNT", prmSameDayCnt)
                .Add("@CTNLINENO", prmCtnineNo)
            End With

            'SQL実行
            CS0050SESSION.GetDataTable(SQLcon, sqlText.ToString, sqlParam, sqlRetSet)

            If sqlRetSet.Rows.Count > 0 Then
                strApplStatus = GetStringValue(sqlRetSet, 0, "APPLSTATUS")
            End If

            Return strApplStatus

        End Using

    End Function

    ''' <summary>
    ''' 精算ファイル 追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <remarks>ヘッダデータを登録する</remarks>
    Public Shared Sub InsertSeisanFData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("INSERT INTO LNG.LNT0017_RESSNF (")
        sqlSeisanFStat.AppendLine("    SHIPYMD")                  '発送年月日
        sqlSeisanFStat.AppendLine("  , CTNTYPE")                  'コンテナ記号
        sqlSeisanFStat.AppendLine("  , CTNNO")                    'コンテナ番号
        sqlSeisanFStat.AppendLine("  , SAMEDAYCNT")               '同日内回数
        sqlSeisanFStat.AppendLine("  , CTNLINENO")                '行番
        sqlSeisanFStat.AppendLine("  , BIGCTNCD")                 '大分類コード
        sqlSeisanFStat.AppendLine("  , MIDDLECTNCD")              '中分類コード
        sqlSeisanFStat.AppendLine("  , SMALLCTNCD")               '小分類コード
        sqlSeisanFStat.AppendLine("  , JOTDEPBRANCHCD")           'ＪＯＴ発組織コード
        sqlSeisanFStat.AppendLine("  , DEPSTATION")               '発駅コード
        sqlSeisanFStat.AppendLine("  , DEPTRUSTEECD")             '発受託人コード
        sqlSeisanFStat.AppendLine("  , DEPTRUSTEESUBCD")          '発受託人サブ
        sqlSeisanFStat.AppendLine("  , JOTARRBRANCHCD")           'ＪＯＴ着組織コード
        sqlSeisanFStat.AppendLine("  , ARRSTATION")               '着駅コード
        sqlSeisanFStat.AppendLine("  , ARRTRUSTEECD")             '着受託人コード
        sqlSeisanFStat.AppendLine("  , ARRTRUSTEESUBCD")          '着受託人サブ
        sqlSeisanFStat.AppendLine("  , ARRPLANYMD")               '到着予定年月日
        sqlSeisanFStat.AppendLine("  , STACKFREEKBN")             '積空区分
        sqlSeisanFStat.AppendLine("  , STATUSKBN")                '状態区分
        sqlSeisanFStat.AppendLine("  , CONTRACTCD")               '契約コード
        sqlSeisanFStat.AppendLine("  , DEPTRAINNO")               '発列車番号
        sqlSeisanFStat.AppendLine("  , ARRTRAINNO")               '着列車番号
        sqlSeisanFStat.AppendLine("  , JRITEMCD")                 'ＪＲ品目コード
        sqlSeisanFStat.AppendLine("  , LEASEPRODUCTCD")           'リース品名コード
        sqlSeisanFStat.AppendLine("  , DEPSHIPPERCD")             '発荷主コード
        sqlSeisanFStat.AppendLine("  , QUANTITY")                 '個数
        sqlSeisanFStat.AppendLine("  , ADDSUBYM")                 '加減額の対象年月
        sqlSeisanFStat.AppendLine("  , ADDSUBQUANTITY")           '加減額の個数
        sqlSeisanFStat.AppendLine("  , JRFIXEDFARE")              'ＪＲ所定運賃
        sqlSeisanFStat.AppendLine("  , USEFEE")                   '使用料金額
        sqlSeisanFStat.AppendLine("  , OWNDISCOUNTFEE")           '私有割引相当額
        sqlSeisanFStat.AppendLine("  , RETURNFARE")               '割戻し運賃
        sqlSeisanFStat.AppendLine("  , NITTSUFREESEND")           '通運負担回送運賃
        sqlSeisanFStat.AppendLine("  , MANAGEFEE")                '運行管理料
        sqlSeisanFStat.AppendLine("  , SHIPBURDENFEE")            '荷主負担運賃
        sqlSeisanFStat.AppendLine("  , SHIPFEE")                  '発送料
        sqlSeisanFStat.AppendLine("  , ARRIVEFEE")                '到着料
        sqlSeisanFStat.AppendLine("  , PICKUPFEE")                '集荷料
        sqlSeisanFStat.AppendLine("  , DELIVERYFEE")              '配達料
        sqlSeisanFStat.AppendLine("  , OTHER1FEE")                'その他１
        sqlSeisanFStat.AppendLine("  , OTHER2FEE")                'その他２
        sqlSeisanFStat.AppendLine("  , FREESENDFEE")              '回送運賃
        sqlSeisanFStat.AppendLine("  , SPRFITKBN")                '冷蔵適合マーク
        sqlSeisanFStat.AppendLine("  , JURISDICTIONCD")           '所管部コード
        sqlSeisanFStat.AppendLine("  , ACCOUNTINGASSETSCD")       '経理資産コード
        sqlSeisanFStat.AppendLine("  , ACCOUNTINGASSETSKBN")      '経理資産区分
        sqlSeisanFStat.AppendLine("  , DUMMYKBN")                 'ダミー区分
        sqlSeisanFStat.AppendLine("  , SPOTKBN")                  'スポット区分
        sqlSeisanFStat.AppendLine("  , COMPKANKBN")               '複合一貫区分
        sqlSeisanFStat.AppendLine("  , KEIJOYM")                  '計上年月
        sqlSeisanFStat.AppendLine("  , TORICODE")                 '取引先コード
        sqlSeisanFStat.AppendLine("  , PARTNERCAMPCD")            '相手先会社コード
        sqlSeisanFStat.AppendLine("  , PARTNERDEPTCD")            '相手先部門コード
        sqlSeisanFStat.AppendLine("  , INVKEIJYOBRANCHCD")        '請求項目 計上店コード
        sqlSeisanFStat.AppendLine("  , INVFILINGDEPT")            '請求項目 請求書提出部店
        sqlSeisanFStat.AppendLine("  , INVKESAIKBN")              '請求項目 請求書決済区分
        sqlSeisanFStat.AppendLine("  , INVSUBCD")                 '請求項目 請求書細分コード
        sqlSeisanFStat.AppendLine("  , PAYKEIJYOBRANCHCD")        '支払項目 費用計上店コード
        sqlSeisanFStat.AppendLine("  , PAYFILINGBRANCH")          '支払項目 支払書提出支店
        sqlSeisanFStat.AppendLine("  , TAXCALCUNIT")              '支払項目 消費税計算単位
        sqlSeisanFStat.AppendLine("  , TAXKBN")                   '税区分
        sqlSeisanFStat.AppendLine("  , TAXRATE")                  '税率
        sqlSeisanFStat.AppendLine("  , BEFDEPTRUSTEECD")          '変換前項目-発受託人コード
        sqlSeisanFStat.AppendLine("  , BEFDEPTRUSTEESUBCD")       '変換前項目-発受託人サブ
        sqlSeisanFStat.AppendLine("  , BEFDEPSHIPPERCD")          '変換前項目-発荷主コード
        sqlSeisanFStat.AppendLine("  , BEFARRTRUSTEECD")          '変換前項目-着受託人コード
        sqlSeisanFStat.AppendLine("  , BEFARRTRUSTEESUBCD")       '変換前項目-着受託人サブ
        sqlSeisanFStat.AppendLine("  , BEFJRITEMCD")              '変換前項目-ＪＲ品目コード
        sqlSeisanFStat.AppendLine("  , BEFSTACKFREEKBN")          '変換前項目-積空区分
        sqlSeisanFStat.AppendLine("  , BEFSTATUSKBN")             '変換前項目-状態区分
        sqlSeisanFStat.AppendLine("  , SPLBEFDEPSTATION")         '分割前項目-発駅コード
        sqlSeisanFStat.AppendLine("  , SPLBEFDEPTRUSTEECD")       '分割前項目-発受託人コード
        sqlSeisanFStat.AppendLine("  , SPLBEFDEPTRUSTEESUBCD")    '分割前項目-発受託人サブ
        sqlSeisanFStat.AppendLine("  , SPLBEFUSEFEE")             '分割前項目-使用料金額
        sqlSeisanFStat.AppendLine("  , SPLBEFSHIPFEE")            '分割前項目-発送料
        sqlSeisanFStat.AppendLine("  , SPLBEFARRIVEFEE")          '分割前項目-到着料
        sqlSeisanFStat.AppendLine("  , SPLBEFFREESENDFEE")        '分割前項目-回送運賃
        sqlSeisanFStat.AppendLine("  , ORDERNO")                  'オーダーNo
        sqlSeisanFStat.AppendLine("  , ORDERLINENO")              'オーダー行No
        sqlSeisanFStat.AppendLine("  , ACCOUNTSTATUSKBN")         '勘定科目用状態区分
        sqlSeisanFStat.AppendLine("  , REFRIGERATIONFLG")         '冷蔵適合フラグ
        sqlSeisanFStat.AppendLine("  , FIXEDFEE")                 '固定使用料
        sqlSeisanFStat.AppendLine("  , INCOMEADJUSTFEE")          '収入加減額
        sqlSeisanFStat.AppendLine("  , TOTALINCOME")              '収入合計
        sqlSeisanFStat.AppendLine("  , COMMISSIONFEE")            '手数料
        sqlSeisanFStat.AppendLine("  , COSTADJUSTFEE")            '費用加減額
        sqlSeisanFStat.AppendLine("  , TOTALCOST")                '費用合計
        sqlSeisanFStat.AppendLine("  , BILLLINK")                 '請求連携状態
        sqlSeisanFStat.AppendLine("  , ACNTLINK")                 '経理連携状態
        sqlSeisanFStat.AppendLine("  , CLOSINGDATE")          　　'締年月日
        sqlSeisanFStat.AppendLine("  , SCHEDATEPAYMENT")          '入金予定日
        sqlSeisanFStat.AppendLine("  , ACCOUNTINGMONTH")          '計上月区分
        sqlSeisanFStat.AppendLine("  , DEPOSITMONTHKBN")          '入金月区分
        sqlSeisanFStat.AppendLine("  , INACCOUNTCD")              '社内口座コード
        sqlSeisanFStat.AppendLine("  , SLIPDESCRIPTION1")         '伝票摘要１
        sqlSeisanFStat.AppendLine("  , SLIPDESCRIPTION2")         '伝票摘要２
        sqlSeisanFStat.AppendLine("  , APPLSTATUS")               '申請状況
        sqlSeisanFStat.AppendLine("  , APPLYMD")                  '申請年月日
        sqlSeisanFStat.AppendLine("  , APPLUSER")                 '申請者ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , MANUALCREATEFLG")          '手動作成フラグ
        sqlSeisanFStat.AppendLine("  , UPDCAUSE")                 '修正理由
        sqlSeisanFStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlSeisanFStat.AppendLine("  , INITYMD")            '登録年月日
        sqlSeisanFStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , INITTERMID")         '登録端末
        sqlSeisanFStat.AppendLine("  , INITPGID")           '登録プログラムＩＤ
        sqlSeisanFStat.AppendLine("  , RECEIVEYMD")         '集信日時
        sqlSeisanFStat.AppendLine(")")
        sqlSeisanFStat.AppendLine(" VALUES(")
        sqlSeisanFStat.AppendLine("    @SHIPYMD")                  '発送年月日
        sqlSeisanFStat.AppendLine("  , @CTNTYPE")                  'コンテナ記号
        sqlSeisanFStat.AppendLine("  , @CTNNO")                    'コンテナ番号
        sqlSeisanFStat.AppendLine("  , @SAMEDAYCNT")               '同日内回数
        sqlSeisanFStat.AppendLine("  , @CTNLINENO")                '行番
        sqlSeisanFStat.AppendLine("  , @BIGCTNCD")                 '大分類コード
        sqlSeisanFStat.AppendLine("  , @MIDDLECTNCD")              '中分類コード
        sqlSeisanFStat.AppendLine("  , @SMALLCTNCD")               '小分類コード
        sqlSeisanFStat.AppendLine("  , @JOTDEPBRANCHCD")           'ＪＯＴ発組織コード
        sqlSeisanFStat.AppendLine("  , @DEPSTATION")               '発駅コード
        sqlSeisanFStat.AppendLine("  , @DEPTRUSTEECD")             '発受託人コード
        sqlSeisanFStat.AppendLine("  , @DEPTRUSTEESUBCD")          '発受託人サブ
        sqlSeisanFStat.AppendLine("  , @JOTARRBRANCHCD")           'ＪＯＴ着組織コード
        sqlSeisanFStat.AppendLine("  , @ARRSTATION")               '着駅コード
        sqlSeisanFStat.AppendLine("  , @ARRTRUSTEECD")             '着受託人コード
        sqlSeisanFStat.AppendLine("  , @ARRTRUSTEESUBCD")          '着受託人サブ
        sqlSeisanFStat.AppendLine("  , @ARRPLANYMD")               '到着予定年月日
        sqlSeisanFStat.AppendLine("  , @STACKFREEKBN")             '積空区分
        sqlSeisanFStat.AppendLine("  , @STATUSKBN")                '状態区分
        sqlSeisanFStat.AppendLine("  , @CONTRACTCD")               '契約コード
        sqlSeisanFStat.AppendLine("  , @DEPTRAINNO")               '発列車番号
        sqlSeisanFStat.AppendLine("  , @ARRTRAINNO")               '着列車番号
        sqlSeisanFStat.AppendLine("  , @JRITEMCD")                 'ＪＲ品目コード
        sqlSeisanFStat.AppendLine("  , @LEASEPRODUCTCD")           'リース品名コード
        sqlSeisanFStat.AppendLine("  , @DEPSHIPPERCD")             '発荷主コード
        sqlSeisanFStat.AppendLine("  , @QUANTITY")                 '個数
        sqlSeisanFStat.AppendLine("  , @ADDSUBYM")                 '加減額の対象年月
        sqlSeisanFStat.AppendLine("  , @ADDSUBQUANTITY")           '加減額の個数
        sqlSeisanFStat.AppendLine("  , @JRFIXEDFARE")              'ＪＲ所定運賃
        sqlSeisanFStat.AppendLine("  , @USEFEE")                   '使用料金額
        sqlSeisanFStat.AppendLine("  , @OWNDISCOUNTFEE")           '私有割引相当額
        sqlSeisanFStat.AppendLine("  , @RETURNFARE")               '割戻し運賃
        sqlSeisanFStat.AppendLine("  , @NITTSUFREESEND")           '通運負担回送運賃
        sqlSeisanFStat.AppendLine("  , @MANAGEFEE")                '運行管理料
        sqlSeisanFStat.AppendLine("  , @SHIPBURDENFEE")            '荷主負担運賃
        sqlSeisanFStat.AppendLine("  , @SHIPFEE")                  '発送料
        sqlSeisanFStat.AppendLine("  , @ARRIVEFEE")                '到着料
        sqlSeisanFStat.AppendLine("  , @PICKUPFEE")                '集荷料
        sqlSeisanFStat.AppendLine("  , @DELIVERYFEE")              '配達料
        sqlSeisanFStat.AppendLine("  , @OTHER1FEE")                'その他１
        sqlSeisanFStat.AppendLine("  , @OTHER2FEE")                'その他２
        sqlSeisanFStat.AppendLine("  , @FREESENDFEE")              '回送運賃
        sqlSeisanFStat.AppendLine("  , @SPRFITKBN")                '冷蔵適合マーク
        sqlSeisanFStat.AppendLine("  , @JURISDICTIONCD")           '所管部コード
        sqlSeisanFStat.AppendLine("  , @ACCOUNTINGASSETSCD")       '経理資産コード
        sqlSeisanFStat.AppendLine("  , @ACCOUNTINGASSETSKBN")      '経理資産区分
        sqlSeisanFStat.AppendLine("  , @DUMMYKBN")                 'ダミー区分
        sqlSeisanFStat.AppendLine("  , @SPOTKBN")                  'スポット区分
        sqlSeisanFStat.AppendLine("  , @COMPKANKBN")               '複合一貫区分
        sqlSeisanFStat.AppendLine("  , @KEIJOYM")                  '計上年月
        sqlSeisanFStat.AppendLine("  , @TORICODE")                 '取引先コード
        sqlSeisanFStat.AppendLine("  , @PARTNERCAMPCD")            '相手先会社コード
        sqlSeisanFStat.AppendLine("  , @PARTNERDEPTCD")            '相手先部門コード
        sqlSeisanFStat.AppendLine("  , @INVKEIJYOBRANCHCD")        '請求項目 計上店コード
        sqlSeisanFStat.AppendLine("  , @INVFILINGDEPT")            '請求項目 請求書提出部店
        sqlSeisanFStat.AppendLine("  , @INVKESAIKBN")              '請求項目 請求書決済区分
        sqlSeisanFStat.AppendLine("  , @INVSUBCD")                 '請求項目 請求書細分コード
        sqlSeisanFStat.AppendLine("  , @PAYKEIJYOBRANCHCD")        '支払項目 費用計上店コード
        sqlSeisanFStat.AppendLine("  , @PAYFILINGBRANCH")          '支払項目 支払書提出支店
        sqlSeisanFStat.AppendLine("  , @TAXCALCUNIT")              '支払項目 消費税計算単位
        sqlSeisanFStat.AppendLine("  , @TAXKBN")                   '税区分
        sqlSeisanFStat.AppendLine("  , @TAXRATE")                  '税率
        sqlSeisanFStat.AppendLine("  , @BEFDEPTRUSTEECD")          '変換前項目-発受託人コード
        sqlSeisanFStat.AppendLine("  , @BEFDEPTRUSTEESUBCD")       '変換前項目-発受託人サブ
        sqlSeisanFStat.AppendLine("  , @BEFDEPSHIPPERCD")          '変換前項目-発荷主コード
        sqlSeisanFStat.AppendLine("  , @BEFARRTRUSTEECD")          '変換前項目-着受託人コード
        sqlSeisanFStat.AppendLine("  , @BEFARRTRUSTEESUBCD")       '変換前項目-着受託人サブ
        sqlSeisanFStat.AppendLine("  , @BEFJRITEMCD")              '変換前項目-ＪＲ品目コード
        sqlSeisanFStat.AppendLine("  , @BEFSTACKFREEKBN")          '変換前項目-積空区分
        sqlSeisanFStat.AppendLine("  , @BEFSTATUSKBN")             '変換前項目-状態区分
        sqlSeisanFStat.AppendLine("  , @SPLBEFDEPSTATION")         '分割前項目-発駅コード
        sqlSeisanFStat.AppendLine("  , @SPLBEFDEPTRUSTEECD")       '分割前項目-発受託人コード
        sqlSeisanFStat.AppendLine("  , @SPLBEFDEPTRUSTEESUBCD")    '分割前項目-発受託人サブ
        sqlSeisanFStat.AppendLine("  , @SPLBEFUSEFEE")             '分割前項目-使用料金額
        sqlSeisanFStat.AppendLine("  , @SPLBEFSHIPFEE")            '分割前項目-発送料
        sqlSeisanFStat.AppendLine("  , @SPLBEFARRIVEFEE")          '分割前項目-到着料
        sqlSeisanFStat.AppendLine("  , @SPLBEFFREESENDFEE")        '分割前項目-回送運賃
        sqlSeisanFStat.AppendLine("  , @ORDERNO")                  'オーダーNo
        sqlSeisanFStat.AppendLine("  , @ORDERLINENO")              'オーダー行No
        sqlSeisanFStat.AppendLine("  , @ACCOUNTSTATUSKBN")         '勘定科目用状態区分
        sqlSeisanFStat.AppendLine("  , @REFRIGERATIONFLG")         '冷蔵適合フラグ
        sqlSeisanFStat.AppendLine("  , @FIXEDFEE")                 '固定使用料
        sqlSeisanFStat.AppendLine("  , @INCOMEADJUSTFEE")          '収入加減額
        sqlSeisanFStat.AppendLine("  , @TOTALINCOME")              '収入合計
        sqlSeisanFStat.AppendLine("  , @COMMISSIONFEE")            '手数料
        sqlSeisanFStat.AppendLine("  , @COSTADJUSTFEE")            '費用加減額
        sqlSeisanFStat.AppendLine("  , @TOTALCOST")                '費用合計
        sqlSeisanFStat.AppendLine("  , @BILLLINK")                 '請求連携状態
        sqlSeisanFStat.AppendLine("  , @ACNTLINK")                 '経理連携状態
        sqlSeisanFStat.AppendLine("  , @CLOSINGDATE")          　　'締年月日
        sqlSeisanFStat.AppendLine("  , @SCHEDATEPAYMENT")          '入金予定日
        sqlSeisanFStat.AppendLine("  , @ACCOUNTINGMONTH")          '計上月区分
        sqlSeisanFStat.AppendLine("  , @DEPOSITMONTHKBN")          '入金月区分
        sqlSeisanFStat.AppendLine("  , @INACCOUNTCD")              '社内口座コード
        sqlSeisanFStat.AppendLine("  , @SLIPDESCRIPTION1")         '伝票摘要１
        sqlSeisanFStat.AppendLine("  , @SLIPDESCRIPTION2")         '伝票摘要２
        sqlSeisanFStat.AppendLine("  , @APPLSTATUS")               '申請状況
        sqlSeisanFStat.AppendLine("  , @APPLYMD")                  '申請年月日
        sqlSeisanFStat.AppendLine("  , @APPLUSER")                 '申請者ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , @MANUALCREATEFLG")          '手動作成フラグ
        sqlSeisanFStat.AppendLine("  , @UPDCAUSE")                 '修正理由
        sqlSeisanFStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlSeisanFStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlSeisanFStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlSeisanFStat.AppendLine("  , @INITPGID")           '登録プログラムＩＤ
        sqlSeisanFStat.AppendLine("  , @RECEIVEYMD")         '集信日時
        sqlSeisanFStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("SHIPYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPYMD))                     '発送年月日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNTYPE))                 'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNNO))                          'コンテナ番号
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SAMEDAYCNT))                '同日内回数
                .Add("CTNLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNLINENO))                  '行番
                .Add("BIGCTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BIGCTNCD))               '大分類コード
                .Add("MIDDLECTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_MIDDLECTNCD))         '中分類コード
                .Add("SMALLCTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SMALLCTNCD))           '小分類コード
                .Add("JOTDEPBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JOTDEPBRANCHCD))   'ＪＯＴ発組織コード
                .Add("DEPSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPSTATION))                '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRUSTEECD))            '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRUSTEESUBCD))      '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JOTARRBRANCHCD))   'ＪＯＴ着組織コード
                .Add("ARRSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRSTATION))                '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRUSTEECD))            '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRUSTEESUBCD))      '着受託人サブ
                .Add("ARRPLANYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRPLANYMD))               '到着予定年月日
                .Add("STACKFREEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_STACKFREEKBN))            '積空区分
                .Add("STATUSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_STATUSKBN))                  '状態区分
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONTRACTCD))           '契約コード
                .Add("DEPTRAINNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRAINNO))                '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRAINNO))                '着列車番号
                .Add("JRITEMCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JRITEMCD))               'ＪＲ品目コード
                .Add("LEASEPRODUCTCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_LEASEPRODUCTCD))   'リース品名コード
                .Add("DEPSHIPPERCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPSHIPPERCD))       '発荷主コード
                .Add("QUANTITY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_QUANTITY))               '個数
                .Add("ADDSUBYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ADDSUBYM))               '加減額の対象年月
                .Add("ADDSUBQUANTITY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ADDSUBQUANTITY))   '加減額の個数
                .Add("JRFIXEDFARE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JRFIXEDFARE))         'ＪＲ所定運賃
                .Add("USEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_USEFEE))                   '使用料金額
                .Add("OWNDISCOUNTFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OWNDISCOUNTFEE))   '私有割引相当額
                .Add("RETURNFARE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_RETURNFARE))           '割戻し運賃
                .Add("NITTSUFREESEND", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_NITTSUFREESEND))   '通運負担回送運賃
                .Add("MANAGEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_MANAGEFEE))             '運行管理料
                .Add("SHIPBURDENFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPBURDENFEE))     '荷主負担運賃
                .Add("SHIPFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPFEE))                 '発送料
                .Add("ARRIVEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRIVEFEE))             '到着料
                .Add("PICKUPFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PICKUPFEE))             '集荷料
                .Add("DELIVERYFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DELIVERYFEE))         '配達料
                .Add("OTHER1FEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OTHER1FEE))             'その他１
                .Add("OTHER2FEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OTHER2FEE))             'その他２
                .Add("FREESENDFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_FREESENDFEE))         '回送運賃
                .Add("SPRFITKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPRFITKBN))             '冷蔵適合マーク
                .Add("JURISDICTIONCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JURISDICTIONCD))            '所管部コード
                .Add("ACCOUNTINGASSETSCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTINGASSETSCD))    '経理資産コード
                .Add("ACCOUNTINGASSETSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTINGASSETSKBN))  '経理資産区分
                .Add("DUMMYKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DUMMYKBN))                        'ダミー区分
                .Add("SPOTKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPOTKBN))                          'スポット区分
                .Add("COMPKANKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_COMPKANKBN))                    '複合一貫区分
                .Add("KEIJOYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_KEIJOYM))                          '計上年月
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TORICODE))                   '取引先コード
                .Add("PARTNERCAMPCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PARTNERCAMPCD))         '相手先会社コード
                .Add("PARTNERDEPTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PARTNERDEPTCD))         '相手先部門コード
                .Add("INVKEIJYOBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVKEIJYOBRANCHCD)) '請求項目 計上店コード
                .Add("INVFILINGDEPT", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVFILINGDEPT))         '請求項目 請求書提出部店
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVKESAIKBN))                  '請求項目 請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVSUBCD))                        '請求項目 請求書細分コード
                .Add("PAYKEIJYOBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PAYKEIJYOBRANCHCD)) '支払項目 費用計上店コード
                .Add("PAYFILINGBRANCH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PAYFILINGBRANCH))     '支払項目 支払書提出支店
                .Add("TAXCALCUNIT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXCALCUNIT))                  '支払項目 消費税計算単位
                .Add("TAXKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXKBN))                            '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXRATE))                          '税率
                .Add("BEFDEPTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFDEPTRUSTEECD))        '変換前項目-発受託人コード
                .Add("BEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFDEPTRUSTEESUBCD))  '変換前項目-発受託人サブ
                .Add("BEFDEPSHIPPERCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFDEPSHIPPERCD))        '変換前項目-発荷主コード
                .Add("BEFARRTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFARRTRUSTEECD))        '変換前項目-着受託人コード
                .Add("BEFARRTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFARRTRUSTEESUBCD))  '変換前項目-着受託人サブ
                .Add("BEFJRITEMCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFJRITEMCD))                '変換前項目-ＪＲ品目コード
                .Add("BEFSTACKFREEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFSTACKFREEKBN))        '変換前項目-積空区分
                .Add("BEFSTATUSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEFSTATUSKBN))              '変換前項目-状態区分
                .Add("SPLBEFDEPSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFDEPSTATION))           '分割前項目-発駅コード
                .Add("SPLBEFDEPTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFDEPTRUSTEECD))       '分割前項目-発受託人コード
                .Add("SPLBEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFDEPTRUSTEESUBCD)) '分割前項目-発受託人サブ
                .Add("SPLBEFUSEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFUSEFEE))                   '分割前項目-使用料金額
                .Add("SPLBEFSHIPFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFSHIPFEE))                 '分割前項目-発送料
                .Add("SPLBEFARRIVEFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFARRIVEFEE))             '分割前項目-到着料
                .Add("SPLBEFFREESENDFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPLBEFFREESENDFEE))         '分割前項目-回送運賃
                .Add("ORDERNO", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ORDERNO))            'オーダーNo
                .Add("ORDERLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ORDERLINENO))         'オーダー行No
                .Add("ACCOUNTSTATUSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTSTATUSKBN))    '勘定科目用状態区分
                .Add("REFRIGERATIONFLG", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_REFRIGERATIONFLG))    '冷蔵適合フラグ
                .Add("FIXEDFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_FIXEDFEE))                    '固定使用料
                .Add("INCOMEADJUSTFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INCOMEADJUSTFEE))      '収入加減額
                .Add("TOTALINCOME", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TOTALINCOME))              '収入合計
                .Add("COMMISSIONFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_COMMISSIONFEE))          '手数料
                .Add("COSTADJUSTFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_COSTADJUSTFEE))          '費用加減額
                .Add("TOTALCOST", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TOTALCOST))                  '費用合計
                .Add("BILLLINK", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BILLLINK))               '請求連携状態
                .Add("ACNTLINK", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACNTLINK))               '経理連携状態
                .Add("CLOSINGDATE", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CLOSINGDATE))             '締年月日
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SCHEDATEPAYMENT))     '入金予定日
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTINGMONTH))      '計上月区分
                .Add("DEPOSITMONTHKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPOSITMONTHKBN))      '入金月区分
                .Add("INACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INACCOUNTCD))              '社内口座コード
                .Add("SLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SLIPDESCRIPTION1))    '伝票摘要１
                .Add("SLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SLIPDESCRIPTION2))    '伝票摘要２
                .Add("APPLSTATUS", MySqlDbType.VarChar).Value = "1"                                                                 '申請状況
                .Add("APPLYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLYMD))                     '申請年月日
                .Add("APPLUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLUSER))               '申請者ユーザーＩＤ
                .Add("MANUALCREATEFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_MANUALCREATEFLG)) '手動作成フラグ
                .Add("UPDCAUSE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDCAUSE))               '修正理由
                .Add("DELFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DELFLG))           '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INITYMD))         '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INITUSER))       '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INITTERMID))   '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INITPGID))       '登録プログラムＩＤ
                .Add("RECEIVEYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_RECEIVEYMD))   '集信日時
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 精算ファイル 更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">データ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateSeisanFData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0017_RESSNF ")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDCAUSE = @UPDCAUSE")               '修正理由
        sqlSeisanFStat.AppendLine("  , BIGCTNCD = @BIGCTNCD")               '大分類コード
        sqlSeisanFStat.AppendLine("  , MIDDLECTNCD = @MIDDLECTNCD")         '中分類コード
        sqlSeisanFStat.AppendLine("  , SMALLCTNCD = @SMALLCTNCD")           '小分類コード
        sqlSeisanFStat.AppendLine("  , JOTDEPBRANCHCD = @JOTDEPBRANCHCD")   'ＪＯＴ発組織コード
        sqlSeisanFStat.AppendLine("  , DEPSTATION = @DEPSTATION")           '発駅コード
        sqlSeisanFStat.AppendLine("  , DEPTRUSTEECD = @DEPTRUSTEECD")       '発受託人コード
        sqlSeisanFStat.AppendLine("  , DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD") '発受託人サブ
        sqlSeisanFStat.AppendLine("  , JOTARRBRANCHCD = @JOTARRBRANCHCD")   'ＪＯＴ着組織コード
        sqlSeisanFStat.AppendLine("  , ARRSTATION = @ARRSTATION")           '着駅コード
        sqlSeisanFStat.AppendLine("  , ARRTRUSTEECD = @ARRTRUSTEECD")       '着受託人コード
        sqlSeisanFStat.AppendLine("  , ARRTRUSTEESUBCD = @ARRTRUSTEESUBCD") '着受託人サブ
        sqlSeisanFStat.AppendLine("  , ARRPLANYMD = @ARRPLANYMD")           '到着予定年月日
        sqlSeisanFStat.AppendLine("  , STACKFREEKBN = @STACKFREEKBN")       '積空区分
        sqlSeisanFStat.AppendLine("  , STATUSKBN = @STATUSKBN")             '状態区分
        sqlSeisanFStat.AppendLine("  , CONTRACTCD = @CONTRACTCD")           '契約コード
        sqlSeisanFStat.AppendLine("  , DEPTRAINNO = @DEPTRAINNO")           '発列車番号
        sqlSeisanFStat.AppendLine("  , ARRTRAINNO = @ARRTRAINNO")           '着列車番号
        sqlSeisanFStat.AppendLine("  , JRITEMCD = @JRITEMCD")               'ＪＲ品目コード
        sqlSeisanFStat.AppendLine("  , DEPSHIPPERCD = @DEPSHIPPERCD")       '発荷主コード
        sqlSeisanFStat.AppendLine("  , JRFIXEDFARE = @JRFIXEDFARE")         'ＪＲ所定運賃
        sqlSeisanFStat.AppendLine("  , USEFEE = @USEFEE")                   '使用料金額
        sqlSeisanFStat.AppendLine("  , OWNDISCOUNTFEE = @OWNDISCOUNTFEE")   '私有割引相当額
        sqlSeisanFStat.AppendLine("  , RETURNFARE = @RETURNFARE")           '割戻し運賃
        sqlSeisanFStat.AppendLine("  , NITTSUFREESEND = @NITTSUFREESEND")   '通運負担回送運賃
        sqlSeisanFStat.AppendLine("  , MANAGEFEE = @MANAGEFEE")             '運行管理料
        sqlSeisanFStat.AppendLine("  , SHIPBURDENFEE = @SHIPBURDENFEE")     '荷主負担運賃
        sqlSeisanFStat.AppendLine("  , SHIPFEE = @SHIPFEE")                 '発送料
        sqlSeisanFStat.AppendLine("  , ARRIVEFEE = @ARRIVEFEE")             '到着料
        sqlSeisanFStat.AppendLine("  , PICKUPFEE = @PICKUPFEE")             '集荷料
        sqlSeisanFStat.AppendLine("  , DELIVERYFEE = @DELIVERYFEE")         '配達料
        sqlSeisanFStat.AppendLine("  , OTHER1FEE = @OTHER1FEE")             'その他１
        sqlSeisanFStat.AppendLine("  , OTHER2FEE = @OTHER2FEE")             'その他２
        sqlSeisanFStat.AppendLine("  , FREESENDFEE = @FREESENDFEE")         '回送運賃
        sqlSeisanFStat.AppendLine("  , TORICODE = @TORICODE")               '取引先コード
        sqlSeisanFStat.AppendLine("  , PARTNERCAMPCD = @PARTNERCAMPCD")     '相手先会社コード
        sqlSeisanFStat.AppendLine("  , PARTNERDEPTCD = @PARTNERDEPTCD")     '相手先部門コード
        sqlSeisanFStat.AppendLine("  , INVKEIJYOBRANCHCD = @INVKEIJYOBRANCHCD")  '請求項目 計上店コード
        sqlSeisanFStat.AppendLine("  , INVFILINGDEPT = @INVFILINGDEPT")          '請求項目 請求書提出部店
        sqlSeisanFStat.AppendLine("  , INVKESAIKBN = @INVKESAIKBN")              '請求項目 請求書決済区分
        sqlSeisanFStat.AppendLine("  , INVSUBCD = @INVSUBCD")                    '請求項目 請求書細分コード
        sqlSeisanFStat.AppendLine("  , PAYKEIJYOBRANCHCD = @PAYKEIJYOBRANCHCD")  '支払項目 費用計上店コード
        sqlSeisanFStat.AppendLine("  , PAYFILINGBRANCH = @PAYFILINGBRANCH")      '支払項目 支払書提出支店
        sqlSeisanFStat.AppendLine("  , TAXCALCUNIT = @TAXCALCUNIT")              '支払項目 消費税計算単位
        sqlSeisanFStat.AppendLine("  , TAXKBN = @TAXKBN")                        '税区分
        sqlSeisanFStat.AppendLine("  , TAXRATE = @TAXRATE")                      '税率
        sqlSeisanFStat.AppendLine("  , ACCOUNTSTATUSKBN = @ACCOUNTSTATUSKBN")    '勘定科目用状態区分
        sqlSeisanFStat.AppendLine("  , SPRFITKBN = @SPRFITKBN")                  '冷蔵適合マーク
        sqlSeisanFStat.AppendLine("  , REFRIGERATIONFLG = @REFRIGERATIONFLG")    '冷蔵適合フラグ
        sqlSeisanFStat.AppendLine("  , FIXEDFEE = @FIXEDFEE")                    '固定使用料
        sqlSeisanFStat.AppendLine("  , INCOMEADJUSTFEE = @INCOMEADJUSTFEE")      '収入加減額
        sqlSeisanFStat.AppendLine("  , TOTALINCOME = @TOTALINCOME")      　　　　'収入合計
        sqlSeisanFStat.AppendLine("  , COMMISSIONFEE = @COMMISSIONFEE")          '手数料
        sqlSeisanFStat.AppendLine("  , COSTADJUSTFEE = @COSTADJUSTFEE")          '費用加減額
        sqlSeisanFStat.AppendLine("  , TOTALCOST = @TOTALCOST")      　　    　　'費用合計
        sqlSeisanFStat.AppendLine("  , CLOSINGDATE      = @CLOSINGDATE")         '締年月日
        sqlSeisanFStat.AppendLine("  , SCHEDATEPAYMENT  = @SCHEDATEPAYMENT")     '入金予定日
        sqlSeisanFStat.AppendLine("  , ACCOUNTINGMONTH  = @ACCOUNTINGMONTH")     '計上月区分
        sqlSeisanFStat.AppendLine("  , DEPOSITMONTHKBN  = @DEPOSITMONTHKBN")     '入金月区分
        sqlSeisanFStat.AppendLine("  , INACCOUNTCD      = @INACCOUNTCD")         '社内口座コード
        sqlSeisanFStat.AppendLine("  , SLIPDESCRIPTION1 = @SLIPDESCRIPTION1")    '伝票摘要１
        sqlSeisanFStat.AppendLine("  , SLIPDESCRIPTION2 = @SLIPDESCRIPTION2")    '伝票摘要２
        sqlSeisanFStat.AppendLine("  , QUANTITY = @QUANTITY")                    '個数
        sqlSeisanFStat.AppendLine("  , APPLSTATUS = @APPLSTATUS")           '申請状況
        sqlSeisanFStat.AppendLine("  , APPLYMD = @APPLYMD")                 '申請年月日
        sqlSeisanFStat.AppendLine("  , APPLUSER = @APPLUSER")               '申請者ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , CONFUPDYMD = @CONFUPDYMD")           '確認／修正年月日
        sqlSeisanFStat.AppendLine("  , CONFUPDUSER = @CONFUPDUSER")         '確認／修正者ユーザーＩＤ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        SHIPYMD    = @SHIPYMD")    '発送年月日
        sqlSeisanFStat.AppendLine("    AND CTNTYPE    = @CTNTYPE")    'コンテナ記号
        sqlSeisanFStat.AppendLine("    AND CTNNO      = @CTNNO")      'コンテナ番号
        sqlSeisanFStat.AppendLine("    AND SAMEDAYCNT = @SAMEDAYCNT") '同日内回数
        sqlSeisanFStat.AppendLine("    AND CTNLINENO  = @CTNLINENO")  '行番

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                'KEY
                .Add("SHIPYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPYMD))             '発送年月日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNTYPE))         'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNNO))                  'コンテナ番号
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SAMEDAYCNT))        '同日内回数
                .Add("CTNLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNLINENO))          '行番
                '更新データ
                .Add("UPDCAUSE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDCAUSE))       '修正理由
                .Add("BIGCTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BIGCTNCD))       '大分類コード
                .Add("MIDDLECTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_MIDDLECTNCD)) '中分類コード
                .Add("SMALLCTNCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SMALLCTNCD))   '小分類コード
                .Add("JOTDEPBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JOTDEPBRANCHCD))  'ＪＯＴ発組織コード
                .Add("DEPSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPSTATION))               '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRUSTEECD))           '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRUSTEESUBCD))     '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JOTARRBRANCHCD))  'ＪＯＴ着組織コード
                .Add("ARRSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRSTATION))               '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRUSTEECD))           '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRUSTEESUBCD))     '着受託人サブ
                .Add("ARRPLANYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRPLANYMD))       '到着予定年月日
                .Add("STACKFREEKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_STACKFREEKBN))    '積空区分
                .Add("STATUSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_STATUSKBN))          '状態区分
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONTRACTCD))   '契約コード
                .Add("DEPTRAINNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPTRAINNO))        '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRTRAINNO))        '着列車番号
                .Add("JRITEMCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JRITEMCD))            'ＪＲ品目コード
                .Add("DEPSHIPPERCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPSHIPPERCD))    '発荷主コード
                .Add("JRFIXEDFARE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_JRFIXEDFARE))       'ＪＲ所定運賃
                .Add("USEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_USEFEE))                 '使用料金額
                .Add("OWNDISCOUNTFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OWNDISCOUNTFEE)) '私有割引相当額
                .Add("RETURNFARE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_RETURNFARE))         '割戻し運賃
                .Add("NITTSUFREESEND", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_NITTSUFREESEND)) '通運負担回送運賃
                .Add("MANAGEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_MANAGEFEE))           '運行管理料
                .Add("SHIPBURDENFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPBURDENFEE))   '荷主負担運賃
                .Add("SHIPFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPFEE))               '発送料
                .Add("ARRIVEFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ARRIVEFEE))           '到着料
                .Add("PICKUPFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PICKUPFEE))           '集荷料
                .Add("DELIVERYFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DELIVERYFEE))       '配達料
                .Add("OTHER1FEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OTHER1FEE))           'その他１
                .Add("OTHER2FEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_OTHER2FEE))           'その他２
                .Add("FREESENDFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_FREESENDFEE))       '回送運賃
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TORICODE))          '取引先コード
                .Add("PARTNERCAMPCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PARTNERCAMPCD)) '相手先会社コード
                .Add("PARTNERDEPTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PARTNERDEPTCD)) '相手先部門コード
                .Add("INVKEIJYOBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVKEIJYOBRANCHCD)) '請求項目 計上店コード
                .Add("INVFILINGDEPT", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVFILINGDEPT))         '請求項目 請求書提出部店
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVKESAIKBN))          '請求項目 請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVSUBCD))                '請求項目 請求書細分コード
                .Add("PAYKEIJYOBRANCHCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PAYKEIJYOBRANCHCD))  '支払項目 費用計上店コード
                .Add("PAYFILINGBRANCH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PAYFILINGBRANCH))      '支払項目 支払書提出支店
                .Add("TAXCALCUNIT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXCALCUNIT))          '支払項目 消費税計算単位
                .Add("TAXKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXKBN))                    '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TAXRATE))                  '税率
                '追加項目
                .Add("ACCOUNTSTATUSKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTSTATUSKBN)) '勘定科目用状態区分
                .Add("SPRFITKBN", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SPRFITKBN))               '冷蔵適合マーク
                .Add("REFRIGERATIONFLG", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_REFRIGERATIONFLG)) '冷蔵適合フラグ
                .Add("FIXEDFEE", MySqlDbType.Decimal).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_FIXEDFEE))               '固定使用料
                .Add("INCOMEADJUSTFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INCOMEADJUSTFEE))   '収入加減額
                .Add("TOTALINCOME", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TOTALINCOME))           '収入合計
                .Add("COMMISSIONFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_COMMISSIONFEE))       '手数料
                .Add("COSTADJUSTFEE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_COSTADJUSTFEE))       '費用加減額
                .Add("TOTALCOST", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TOTALCOST))               '費用合計
                .Add("CLOSINGDATE", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CLOSINGDATE))          '締年月日
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SCHEDATEPAYMENT))       '入金予定日
                .Add("ACCOUNTINGMONTH", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_ACCOUNTINGMONTH))   '計上月区分
                .Add("DEPOSITMONTHKBN", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_DEPOSITMONTHKBN))   '入金月区分
                .Add("INACCOUNTCD", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INACCOUNTCD))           '社内口座コード
                .Add("SLIPDESCRIPTION1", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SLIPDESCRIPTION1)) '伝票摘要１
                .Add("SLIPDESCRIPTION2", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SLIPDESCRIPTION2)) '伝票摘要２
                .Add("QUANTITY", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_QUANTITY))            '個数
                .Add("APPLSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLSTATUS))   '申請状況
                .Add("APPLYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLYMD))             '申請年月日
                .Add("APPLUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLUSER))       '申請者ユーザーＩＤ
                .Add("CONFUPDYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDYMD))       '確認／修正年月日
                .Add("CONFUPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDUSER)) '確認／修正者ユーザーＩＤ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))           '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))         '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))     '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))         '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 精算ファイル 更新処理(承認／差戻し)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">データ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateSeisanFApprovalRemand(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0017_RESSNF ")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    APPROVALCAUSE = @APPROVALCAUSE")     '承認／差戻し理由
        sqlSeisanFStat.AppendLine("  , APPLSTATUS = @APPLSTATUS")           '申請状況
        sqlSeisanFStat.AppendLine("  , APPROVALYMD = @APPROVALYMD")         '承認年月日
        sqlSeisanFStat.AppendLine("  , APPROVALUSER = @APPROVALUSER")       '承認者ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , CONFUPDYMD = @CONFUPDYMD")           '確認／修正年月日
        sqlSeisanFStat.AppendLine("  , CONFUPDUSER = @CONFUPDUSER")         '確認／修正者ユーザーＩＤ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        SHIPYMD    = @SHIPYMD")    '発送年月日
        sqlSeisanFStat.AppendLine("    AND CTNTYPE    = @CTNTYPE")    'コンテナ記号
        sqlSeisanFStat.AppendLine("    AND CTNNO      = @CTNNO")      'コンテナ番号
        sqlSeisanFStat.AppendLine("    AND SAMEDAYCNT = @SAMEDAYCNT") '同日内回数
        sqlSeisanFStat.AppendLine("    AND CTNLINENO  = @CTNLINENO")  '行番

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                'KEY
                .Add("SHIPYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPYMD))             '発送年月日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNTYPE))         'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNNO))                  'コンテナ番号
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SAMEDAYCNT))        '同日内回数
                .Add("CTNLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNLINENO))          '行番
                '更新データ
                .Add("APPROVALCAUSE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPROVALCAUSE)) '承認／差戻し理由
                .Add("APPLSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLSTATUS))       '申請状況
                .Add("APPROVALYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPROVALYMD))         '承認年月日
                .Add("APPROVALUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPROVALUSER))   '承認者ユーザーＩＤ
                .Add("CONFUPDYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDYMD))       '確認／修正年月日
                .Add("CONFUPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDUSER)) '確認／修正者ユーザーＩＤ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))           '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))         '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))     '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))         '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 精算ファイル 更新処理(取下)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">データ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateSeisanFWithdrawal(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0017_RESSNF ")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    APPLSTATUS = @APPLSTATUS")           '申請状況
        sqlSeisanFStat.AppendLine("  , CONFUPDYMD = @CONFUPDYMD")           '確認／修正年月日
        sqlSeisanFStat.AppendLine("  , CONFUPDUSER = @CONFUPDUSER")         '確認／修正者ユーザーＩＤ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        SHIPYMD    = @SHIPYMD")    '発送年月日
        sqlSeisanFStat.AppendLine("    AND CTNTYPE    = @CTNTYPE")    'コンテナ記号
        sqlSeisanFStat.AppendLine("    AND CTNNO      = @CTNNO")      'コンテナ番号
        sqlSeisanFStat.AppendLine("    AND SAMEDAYCNT = @SAMEDAYCNT") '同日内回数
        sqlSeisanFStat.AppendLine("    AND CTNLINENO  = @CTNLINENO")  '行番

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                'KEY
                .Add("SHIPYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPYMD))             '発送年月日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNTYPE))         'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNNO))                  'コンテナ番号
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SAMEDAYCNT))        '同日内回数
                .Add("CTNLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNLINENO))          '行番
                '更新データ
                .Add("APPLSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_APPLSTATUS))       '申請状況
                .Add("CONFUPDYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDYMD))       '確認／修正年月日
                .Add("CONFUPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CONFUPDUSER)) '確認／修正者ユーザーＩＤ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))           '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))         '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))     '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))         '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub


    ''' <summary>
    ''' 精算ファイル 更新処理(取下)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">データ</param>
    ''' <remarks>リースヘッダデータを更新する</remarks>
    Public Shared Sub UpdateSeisanFDelete(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0017_RESSNF ")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    DELFLG = '1'")                       '削除フラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        SHIPYMD          = @SHIPYMD")    '発送年月日
        sqlSeisanFStat.AppendLine("    AND CTNTYPE          = @CTNTYPE")    'コンテナ記号
        sqlSeisanFStat.AppendLine("    AND CTNNO            = @CTNNO")      'コンテナ番号
        sqlSeisanFStat.AppendLine("    AND SAMEDAYCNT       = @SAMEDAYCNT") '同日内回数
        sqlSeisanFStat.AppendLine("    AND CTNLINENO        = @CTNLINENO")  '行番
        sqlSeisanFStat.AppendLine("    AND MANUALCREATEFLG  = '1'")         '手動作成フラグ（'1'のレコード固定）

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                'KEY
                .Add("SHIPYMD", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SHIPYMD))             '発送年月日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNTYPE))         'コンテナ記号
                .Add("CTNNO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNNO))                  'コンテナ番号
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SAMEDAYCNT))        '同日内回数
                .Add("CTNLINENO", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_CTNLINENO))          '行番
                '更新データ（削除フラグの更新の為、無し）
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))           '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))         '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))     '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))         '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub


    ''' <summary>
    ''' 精算ファイル 手動作成フラグ取得処理
    ''' </summary>
    ''' <param name="prmShipYMD">発送年月日</param>
    ''' <param name="prmCtnType">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="prmSameDayCnt">同日内回数</param>
    ''' <param name="prmCtnineNo">行番</param>
    ''' <remarks>精算ファイルより該当レコードの手動作成フラグを取得する</remarks>
    Public Shared Function GetSeisanFSinseiManualCreateFlg(ByVal prmShipYMD As String, ByVal prmCtnType As String, ByVal prmCtnNo As String,
                                           ByVal prmSameDayCnt As String, ByVal prmCtnineNo As String) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0
        Dim strManualCreateFlg As String = "0"

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            With sqlText
                .AppendLine("SELECT ")
                .AppendLine("     MANUALCREATEFLG AS MANUALCREATEFLG")
                .AppendLine("FROM")
                'メイン 精算ファイル
                .AppendLine("     LNG.LNT0017_RESSNF")
                '抽出条件
                .AppendLine(" WHERE")
                .AppendLine("         SHIPYMD = @SHIPYMD")
                .AppendLine("     AND CTNTYPE = @CTNTYPE")
                .AppendLine("     AND CTNNO   = @CTNNO")
                .AppendLine("     AND SAMEDAYCNT = @SAMEDAYCNT")
                .AppendLine("     AND CTNLINENO = @CTNLINENO")
                .AppendLine("     AND DELFLG   = '0'")
            End With

            'パラメータ設定
            With sqlParam
                .Add("@SHIPYMD", prmShipYMD)
                .Add("@CTNTYPE", prmCtnType)
                .Add("@CTNNO", prmCtnNo)
                .Add("@SAMEDAYCNT", prmSameDayCnt)
                .Add("@CTNLINENO", prmCtnineNo)
            End With

            'SQL実行
            CS0050SESSION.GetDataTable(SQLcon, sqlText.ToString, sqlParam, sqlRetSet)

            If sqlRetSet.Rows.Count > 0 Then
                strManualCreateFlg = GetStringValue(sqlRetSet, 0, "MANUALCREATEFLG")
            End If

            Return strManualCreateFlg

        End Using

    End Function

    ''' <summary>
    ''' 権限ロール取得処理
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmUserOrg">ユーザー権限</param>
    ''' <param name="prmMapID">画面ＩＤ</param>
    ''' <param name="refRefRole">参照権限ロール</param>
    ''' <param name="refUpdRole">更新権限ロール</param>
    ''' <remarks>指定したメニュー表示制御ロール、画面ＩＤの参照・更新権限ロールを取得する</remarks>
    Public Shared Sub GetSeisanFSinseiRole(ByVal prmCampCode As String, ByVal prmUserOrg As String, ByVal prmMapID As String,
                                           ByRef refRefRole As String, ByRef refUpdRole As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0
        Dim strManualCreateFlg As String = "0"

        Dim WW_DATENOW As Date = Date.Now

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            With sqlText
                .AppendLine("SELECT ")
                .AppendLine("     REFERENCEAUTHORIZATION AS REFERENCEAUTHORIZATION")
                .AppendLine("    ,UPDATEAUTHORIZATION AS UPDATEAUTHORIZATION")
                .AppendLine("FROM")
                '権限コントロールマスタ
                .AppendLine("     COM.LNS0027_PRIVILEGECONTROL")
                '抽出条件
                .AppendLine(" WHERE")
                .AppendLine("         CAMPCODE   = @CAMPCODE")
                .AppendLine("     AND ORGCODE    = @USERORG")
                .AppendLine("     AND FUNCTIONID = @MAPID")
                .AppendLine("     AND DELFLG     = '0'")
            End With

            'パラメータ設定
            With sqlParam
                .Add("@CAMPCODE", prmCampCode)
                .Add("@USERORG", prmUserOrg)
                .Add("@MAPID", prmMapID)
            End With

            'SQL実行
            CS0050SESSION.GetDataTable(SQLcon, sqlText.ToString, sqlParam, sqlRetSet)

            If sqlRetSet.Rows.Count > 0 Then
                refRefRole = GetStringValue(sqlRetSet, 0, "REFERENCEAUTHORIZATION")
                refUpdRole = GetStringValue(sqlRetSet, 0, "UPDATEAUTHORIZATION")
            Else
                '取得が出来なかった場合、明示的に"不許可"を設定する。
                refRefRole = CONST_DISAPPROVAL
                refUpdRole = CONST_DISAPPROVAL
            End If

        End Using


    End Sub

    ''' <summary>
    ''' 請求ヘッダ 更新処理(変更フラグ)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <remarks>請求ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub UpdateSeikyuHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0064_INVOICEHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        KEIJOYM         = @KEIJOYM")         '請求年月
        sqlSeisanFStat.AppendLine("    AND INVOICEORGCODE  = @INVOICEORGCODE")  '請求担当部店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE        = @TORICODE")        '請求取引先コード
        sqlSeisanFStat.AppendLine("    AND INVOICETYPE     IN('2', '4')")       '請求書種類(リース、売却在庫以外)
        sqlSeisanFStat.AppendLine("    AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT") '入金予定日
        sqlSeisanFStat.AppendLine("    AND DELFLG          = @DELFLG")          '削除フラグ

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("KEIJOYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_KEIJOYM))                   '請求年月
                .Add("INVOICEORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_INVFILINGDEPT)) '請求担当部店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TORICODE))            '請求取引先コード
                '.Add("INVOICETYPE", MySqlDbType.VarChar).Value = "3"    '請求書種類(リース以外)
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SCHEDATEPAYMENT))  '入金予定日
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 請求ヘッダ 更新処理(変更フラグ)(変更前)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <remarks>請求ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub BefUpdateSeikyuHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0064_INVOICEHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        KEIJOYM         = @KEIJOYM")         '請求年月
        sqlSeisanFStat.AppendLine("    AND INVOICEORGCODE  = @INVOICEORGCODE")  '請求担当部店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE        = @TORICODE")        '請求取引先コード
        sqlSeisanFStat.AppendLine("    AND INVOICETYPE     IN('2', '4')")       '請求書種類(リース、売却在庫以外)
        sqlSeisanFStat.AppendLine("    AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT") '入金予定日
        sqlSeisanFStat.AppendLine("    AND DELFLG          = @DELFLG")          '削除フラグ

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("KEIJOYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_KEIJOYM))                   '請求年月
                .Add("INVOICEORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_INVFILINGDEPT)) '請求担当部店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_TORICODE))            '請求取引先コード
                '                .Add("INVOICETYPE", MySqlDbType.VarChar).Value = "3"    '請求書種類(リース以外)
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_SCHEDATEPAYMENT))  '入金予定日
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダ 更新処理(変更フラグ)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <remarks>支払ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub UpdatePaymentHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        PAYMENTYM       = @PAYMENTYM")       '支払年月
        sqlSeisanFStat.AppendLine("    AND PAYMENTORGCODE  = @PAYMENTORGCODE")  '支払支店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE        = @TORICODE")        '支払取引先コード
        sqlSeisanFStat.AppendLine("    AND PAYMENTTYPE     = @PAYMENTTYPE")     '支払書種類
        sqlSeisanFStat.AppendLine("    AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT") '支払予定日

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_KEIJOYM))          '支払年月
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_PAYFILINGBRANCH)) '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_TORICODE))       '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = "1"    '請求書種類
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_SCHEDATEPAYMENT))  '入金予定日
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダ 更新処理(変更フラグ)(変更前)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <remarks>支払ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub BefUpdatePaymentHead(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        PAYMENTYM       = @PAYMENTYM")       '支払年月
        sqlSeisanFStat.AppendLine("    AND PAYMENTORGCODE  = @PAYMENTORGCODE")  '支払支店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE        = @TORICODE")        '支払取引先コード
        sqlSeisanFStat.AppendLine("    AND PAYMENTTYPE     = @PAYMENTTYPE")     '支払書種類
        sqlSeisanFStat.AppendLine("    AND SCHEDATEPAYMENT = @SCHEDATEPAYMENT") '支払予定日

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_KEIJOYM))          '支払年月
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_PAYFILINGBRANCH)) '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_TORICODE))       '支払取引先コード
                .Add("PAYMENTTYPE", MySqlDbType.VarChar).Value = "1"    '請求書種類
                .Add("SCHEDATEPAYMENT", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_BEF_SCHEDATEPAYMENT))  '入金予定日
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 請求ヘッダ 更新処理(変更フラグ)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <param name="htSeikey">請求ヘッダデータ</param>
    ''' <remarks>請求ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub UpdateSeikyuHeadKey(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable, ByVal htSeikey As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0064_INVOICEHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        KEIJOYM        = @KEIJOYM")         '請求年月
        sqlSeisanFStat.AppendLine("    AND INVOICENUMBER  = @INVOICENUMBER")   '請求番号
        sqlSeisanFStat.AppendLine("    AND INVOICEORGCODE = @INVOICEORGCODE")  '請求担当部店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE = @TORICODE")              '請求取引先コード

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("KEIJOYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSeikey(SEIHEAD_KEY.KEIJOYM))              '請求年月
                .Add("INVOICENUMBER", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htSeikey(SEIHEAD_KEY.INVOICENUMBER))     '請求番号
                .Add("INVOICEORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSeikey(SEIHEAD_KEY.INVOICEORGCODE)) '請求担当部店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSeikey(SEIHEAD_KEY.TORICODE))       '請求取引先コード
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 支払ヘッダ 更新処理(変更フラグ)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htSFData">精算ファイルデータ</param>
    ''' <param name="htPayment">支払ヘッダデータ</param>
    ''' <remarks>支払ヘッダデータの変更フラグを更新する</remarks>
    Public Shared Sub UpdatePaymentHeadKey(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal htSFData As Hashtable, ByVal htPayment As Hashtable)

        '◯精算ファイル
        Dim sqlSeisanFStat As New StringBuilder
        sqlSeisanFStat.AppendLine("UPDATE LNG.LNT0077_PAYMENTHEAD")
        sqlSeisanFStat.AppendLine("SET")
        sqlSeisanFStat.AppendLine("    UPDATEFLG = @UPDATEFLG")     '変更有りフラグ
        '更新情報
        sqlSeisanFStat.AppendLine("  , UPDYMD = @UPDYMD")                   '更新年月日
        sqlSeisanFStat.AppendLine("  , UPDUSER = @UPDUSER")                 '更新ユーザーＩＤ
        sqlSeisanFStat.AppendLine("  , UPDTERMID = @UPDTERMID")             '更新端末
        'sqlSeisanFStat.AppendLine("  , UPDPGID = @UPDPGID")                 '更新プログラムＩＤ
        sqlSeisanFStat.AppendLine("WHERE")
        sqlSeisanFStat.AppendLine("        PAYMENTYM      = @PAYMENTYM")       '支払年月
        sqlSeisanFStat.AppendLine("    AND PAYMENTNUMBER  = @PAYMENTNUMBER")   '支払番号
        sqlSeisanFStat.AppendLine("    AND PAYMENTORGCODE = @PAYMENTORGCODE")  '支払支店コード
        sqlSeisanFStat.AppendLine("    AND TORICODE = @TORICODE")              '支払取引先コード

        Using sqlOrderCmd As New MySqlCommand(sqlSeisanFStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                '値
                .Add("UPDATEFLG", MySqlDbType.VarChar).Value = "1"  '変更有りフラグ
                'KEY
                .Add("PAYMENTYM", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htPayment(PAYHEAD_KEY.PAYMENTYM))          '支払年月
                .Add("PAYMENTNUMBER", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htPayment(PAYHEAD_KEY.PAYMENTNUMBER))     '支払番号
                .Add("PAYMENTORGCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htPayment(PAYHEAD_KEY.PAYMENTORGCODE)) '支払支店コード
                .Add("TORICODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htPayment(PAYHEAD_KEY.TORICODE))       '支払取引先コード
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDTERMID))   '更新端末
                '.Add("UPDPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htSFData(SEISANF_DP.DP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

End Class
