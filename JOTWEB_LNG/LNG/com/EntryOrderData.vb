Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>受注データ（ヘッダ）用のキー</description></item>
''' </list>
''' </remarks>
Public Enum C_HEADPARAM
    HP_ORDERNO               'オーダーNo
    HP_PLANDEPYMD            '発送予定日
    HP_CTNTYPE               'コンテナ形式
    HP_CTNNO                 'コンテナ番号
    HP_STATUS                '状態
    HP_BIGCTNCD              '大分類コード
    HP_MIDDLECTNCD           '中分類コード
    HP_SMALLCTNCD            '小分類コード
    HP_RENTRATE125NEXTFLG    '125キロ賃率次期フラグ
    HP_RENTRATE125           '125キロ賃率
    HP_ROUNDFEENEXTFLG       '端数金額基準次期フラグ
    HP_ROUNDFEE              '端数金額基準
    HP_ROUNDKBNGE            '端数区分金額以上
    HP_ROUNDKBNLT            '端数区分金額未満
    HP_FILEID                'ファイルID
    HP_REFLECTFLG            '反映フラグ
    HP_DELFLG                '削除フラグ
    HP_INITYMD               '登録年月日
    HP_INITUSER              '登録ユーザーＩＤ
    HP_INITTERMID            '登録端末
    HP_INITPGID              '登録プログラムＩＤ
    HP_UPDYMD                '更新年月日
    HP_UPDUSER               '更新ユーザーＩＤ
    HP_UPDTERMID             '更新端末
    HP_UPDPGID               '更新プログラムＩＤ
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>受注データ（明細データ）用のキー</description></item>
''' </list>
''' </remarks>
Public Enum C_DATAPARAM
    DP_ORDERNO              'オーダーNo
    DP_SAMEDAYCNT           '同日内回数
    DP_FILEID               'ファイルID
    DP_FILELINENO           '行数
    DP_ITEMCD               '品目コード
    DP_ITEMATTACHCD         '品目付属コード
    DP_ITEMNM               '品目名
    DP_RAILDEPSTATION       '鉄道発駅コード
    DP_DEPLEASEDLINECD      '発専用線コード
    DP_RAILARRSTATION       '鉄道着駅コード
    DP_ARRLEASEDLINECD      '着専用線コード
    DP_RAWDEPSTATION        '原発駅
    DP_RAWARRSTATION        '原着駅
    DP_DEPSALEPLACECD       '発コンテナ営業所コード
    DP_ARRSALEPLACECD       '着コンテナ営業所コード
    DP_DEPTRUSTEECD         '発受託人コード
    DP_DEPPICKDELTRADERCD   '発集配業者コード
    DP_ARRTRUSTEECD         '着受託人コード
    DP_ARRPICKDELTRADERCD   '着集配業者コード
    DP_ROOTNO               'ルート番号
    DP_DEPTRAINNO           '発列車番号
    DP_ARRTRAINNO           '着列車番号
    DP_POINTFRAMENO         '指定枠番号
    DP_OBGETDISP            'ＯＢ取得表示
    DP_PLANDEPYMD           '発着予定日付-発車予定日時
    DP_PLANARRYMD           '発着予定日付-到着予定日時
    DP_RESULTDEPYMD         '発着実績日付-発車実績日時
    DP_RESULTARRYMD         '発着実績日付-到着実績日時
    DP_CONTRACTCD           '契約コード
    DP_FAREPAYERCD          '運賃支払者コード
    DP_FAREPAYMETHODCD      '運賃支払方法コード
    DP_FARECALCKIRO         '運賃計算キロ程
    DP_FARECALCTUN          '運賃計算屯数
    DP_DISEXTCD             '割引割増コード
    DP_DISRATE              '割引率
    DP_EXTRATE              '割増率
    DP_TOTALNUM             '総個数
    DP_CARGOWEIGHT          '荷重
    DP_COMPENSATION         '要賠償額
    DP_STANDARDYEAR         '運賃計算基準年
    DP_STANDARDMONTH        '運賃計算基準月
    DP_STANDARDDAY          '運賃計算基準日
    DP_RAILFARE             '鉄道運賃
    DP_ADDFARE              '増運賃
    DP_DGADDFARE            '危険物割増運賃
    DP_VALUABLADDFARE       '貴重品割増運賃
    DP_SPECTNADDFARE        '特コン割増運賃
    DP_DEPSALEPLACEFEE      '発営業所料金
    DP_ARRSALEPLACEFEE      '着営業所料金
    DP_COMPENSATIONDISPFEE  '要賠償額表示金額
    DP_OTHERFEE             'その他料金
    DP_SASIZUFEE            'さしず手数料
    DP_TOTALFAREFEE         '合計運賃料金
    DP_STACKFREEKBN         'コンテナ積空区分
    DP_ORDERMONTH           '受付月
    DP_ORDERDAY             '受付日
    DP_LOADENDMONTH         '積載完了月
    DP_LOADENDDAY           '積載完了日
    DP_DEVELOPENDMONTH      '発達完了月
    DP_DEVELOPENDDAY        '発達完了日
    DP_DEVELOPSPETIME       '発達指定時
    DP_CORRECTLOCASTACD     '訂正所在駅コード
    DP_CORRECTNO            '訂正番号
    DP_CORRELNTYPE          '訂正種別
    DP_CORRELNMONTH         '訂正月
    DP_CORRECTDAY           '訂正日
    DP_ONUSLOCASTACD        '責任所在コード
    DP_SHIPPERCD            '荷送人コード
    DP_SHIPPERNM            '荷送人名
    DP_SHIPPERTEL           '荷送人電話番号
    DP_SLCPICKUPADDRESS     '集荷先住所
    DP_SLCPICKUPTEL         '集荷先電話番号
    DP_CONSIGNEECD          '荷受人コード
    DP_CONSIGNEENM          '荷受人名
    DP_CONSIGNEETEL         '荷受人電話番号
    DP_RECEIVERADDRESS      '配達先住所
    DP_RECEIVERTEL          '配達先電話番号
    DP_INSURANCEFEE         '保険料
    DP_SHIPINSURANCEFEE     '運送保険料金
    DP_LOADADVANCEFEE       '荷掛立替金
    DP_SHIPFEE1             '発送料金１
    DP_SHIPFEE2             '発送料金２
    DP_PACKINGFEE           '梱包料金
    DP_ORIGINWORKFEE        '発地作業料
    DP_DEPOTHERFEE          '発その他料金
    DP_PAYMENTFEE           '着払料
    DP_DEPARTUREEETOTAL     '発側料金計
    DP_DEPARTUREEE1         '到着料金１
    DP_DEPARTUREEE2         '到着料金２
    DP_UNPACKINGFEE         '開梱料金
    DP_LANDINGEORKFEE       '着地作業料
    DP_ARROTHERFEE          '着その他料金
    DP_ARRARTUREEETOTAL     '着側料金計
    DP_ARRNITTSUTAX         '着通運消費税額
    DP_SHIPPERPAYMETHOD     '荷主支払方法
    DP_LUCKFEEINVOICENM     '運地料金請求先名
    DP_ARTICLE              '記事
    DP_INPUTHOUR            '入力時刻(時)
    DP_INPUTMINUTE          '入力時刻(分)
    DP_INPUTSECOND          '入力時刻(秒)
    DP_CONSIGNCANCELKBN     '託送取消区分
    DP_WIKUGUTRANKBN        'ウイクグ輸送区分
    DP_YOBI                 '予備
    DP_REFLECTFLG           '反映フラグ
    DP_SKIPFLG              '読み飛ばしフラグ
    DP_DELFLG               '削除フラグ
    DP_INITYMD              '登録年月日
    DP_INITUSER             '登録ユーザーＩＤ
    DP_INITTERMID           '登録端末
    DP_INITPGID             '登録プログラムＩＤ
    DP_UPDYMD               '更新年月日
    DP_UPDUSER              '更新ユーザーＩＤ
    DP_UPDTERMID            '更新端末
    DP_UPDPGID              '更新プログラムＩＤ
End Enum

''' <summary>
''' パラメタ群
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>精算予定ファイル用のキー</description></item>
''' </list>
''' </remarks>
Public Enum C_PAYFPARAM
    PP_ORDERNO                  'オーダーNo
    PP_SAMEDAYCNT               '同日内回数
    PP_SHIPYMD                  '発送年月日
    PP_LINENUM                  '行番
    PP_JOTDEPBRANCHCD           'ＪＯＴ発店所コード
    PP_DEPSTATION               '発駅コード
    PP_DEPTRUSTEECD             '発受託人コード
    PP_DEPTRUSTEESUBCD          '発受託人サブ
    PP_JOTARRBRANCHCD           'ＪＯＴ着店所コード
    PP_ARRSTATION               '着駅コード
    PP_ARRTRUSTEECD             '着受託人コード
    PP_ARRTRUSTEESUBCD          '着受託人サブ
    PP_ARRPLANYMD               '到着予定年月日
    PP_STACKFREEKBN             '積空区分
    PP_STATUSKBN                '状態区分
    PP_CONTRACTCD               '契約コード
    PP_DEPTRAINNO               '発列車番号
    PP_ARRTRAINNO               '着列車番号
    PP_JRITEMCD                 'ＪＲ品目コード
    PP_LEASEPRODUCTCD           'リース品名コード
    PP_DEPSHIPPERCD             '発荷主コード
    PP_QUANTITY                 '個数
    PP_ADDSUBYM                 '加減額の対象年月
    PP_ADDSUBQUANTITY           '加減額の個数
    PP_JRFIXEDFARE              'ＪＲ所定運賃
    PP_USEFEE                   '使用料金額
    PP_OWNDISCOUNTFEE           '私有割引相当額
    PP_RETURNFARE               '割戻し運賃
    PP_NITTSUFREESENDFEE        '通運負担回送運賃
    PP_MANAGEFEE                '運行管理料
    PP_SHIPBURDENFEE            '荷主負担運賃
    PP_SHIPFEE                  '発送料
    PP_ARRIVEFEE                '到着料
    PP_PICKUPFEE                '集荷料
    PP_DELIVERYFEE              '配達料
    PP_OTHER1FEE                'その他１
    PP_OTHER2FEE                'その他２
    PP_FREESENDFEE              '回送運賃
    PP_SPRFITKBN                '冷蔵適合マーク
    PP_JURISDICTIONCD           '所管部コード
    PP_ACCOUNTINGASSETSCD       '経理資産コード
    PP_ACCOUNTINGASSETSKBN      '経理資産区分
    PP_DUMMYKBN                 'ダミー区分
    PP_SPOTKBN                  'スポット区分
    PP_COMPKANKBN               '複合一貫区分
    PP_KEIJOYM                  '計上年月
    PP_PARTNERCAMPCD            '相手先会社コード
    PP_PARTNERDEPTCD            '相手先部門コード
    PP_INVKEIJYOBRANCHCD        '請求項目 計上店コード
    PP_INVFILINGDEPT            '請求項目 請求書提出部店
    PP_INVKESAIKBN              '請求項目 請求書決済区分
    PP_INVSUBCD                 '請求項目 請求書細分コード
    PP_PAYKEIJYOBRANCHCD        '支払項目 費用計上店コード
    PP_PAYFILINGBRANCH          '支払項目 支払書提出支店
    PP_TAXCALCUNIT              '支払項目 消費税計算単位
    PP_TAXKBN                   '税区分
    PP_TAXRATE                  '税率
    PP_BEFDEPTRUSTEECD          '変換前項目-発受託人コード
    PP_BEFDEPTRUSTEESUBCD       '変換前項目-発受託人サブ
    PP_BEFDEPSHIPPERCD          '変換前項目-発荷主コード
    PP_BEFARRTRUSTEECD          '変換前項目-着受託人コード
    PP_BEFARRTRUSTEESUBCD       '変換前項目-着受託人サブ
    PP_BEFJRITEMCD              '変換前項目-ＪＲ品目コード
    PP_BEFSTACKFREEKBN          '変換前項目-積空区分
    PP_SPLBEFDEPSTATION         '分割前項目-発駅コード
    PP_SPLBEFDEPTRUSTEECD       '分割前項目-発受託人コード
    PP_SPLBEFDEPTRUSTEESUBCD    '分割前項目-発受託人サブ
    PP_SPLBEFUSEFEE             '分割前項目-使用料金額
    PP_SPLBEFSHIPFEE            '分割前項目-発送料
    PP_SPLBEFARRIVEFEE          '分割前項目-到着料
    PP_SPLBEFFREESENDFEE        '分割前項目-回送運賃
    PP_PROCFLG1                 '処理フラグ-料金計算済
    PP_PROCFLG2                 '処理フラグ-精算ファイル作成済
    PP_PROCFLG3                 '処理フラグ-運用ファイル作成済
    PP_PROCFLG4                 '処理フラグ-複合一貫作成済
    PP_PROCFLG5                 '処理フラグ-請求支払分割済
    PP_PROCFLG6                 '処理フラグ-コード変換済
    PP_PROCFLG7                 '処理フラグ-ダミーフラグ７
    PP_PROCFLG8                 '処理フラグ-ダミーフラグ８
    PP_PROCFLG9                 '処理フラグ-ダミーフラグ９
    PP_PROCFLG10                '処理フラグ-ダミーフラグ１０
    PP_PICKUPTEL                '集荷先電話番号
    PP_FARECALCTUNAPPLKBN       '運賃計算屯数適用区分
    PP_FARECALCTUNNEXTFLG       '運賃計算屯数次期フラグ
    PP_FARECALCTUN              '運賃計算屯数
    PP_DISNO                    '割引番号
    PP_EXTNO                    '割増番号
    PP_KIROAPPLKBN              'キロ程適用区分
    PP_KIRO                     'キロ程
    PP_RENTRATEAPPLKBN          '賃率適用区分
    PP_RENTRATENEXTFLG          '賃率次期フラグ
    PP_RENTRATE                 '賃率
    PP_APPLYRATEAPPLKBN         '適用率適用区分
    PP_APPLYRATENEXTFLG         '適用率次期フラグ
    PP_APPLYRATE                '適用率
    PP_USEFEERATEAPPLKBN        '使用料率適用区分
    PP_USEFEERATE               '使用料率
    PP_FREESENDRATEAPPLKBN      '回送運賃適用率適用区分
    PP_FREESENDRATENEXTFLG      '回送運賃適用率次期フラグ
    PP_FREESENDRATE             '回送運賃適用率
    PP_SHIPFEEAPPLKBN           '発送料適用区分
    PP_SHIPFEENEXTFLG           '発送料次期フラグ
    PP_TARIFFAPPLKBN            '使用料タリフ適用区分
    PP_OUTISLANDAPPLKBN         '離島向け適用区分
    PP_FREEAPPLKBN              '使用料無料特認 
    PP_SPECIALM1APPLKBN         '特例Ｍ１適用区分
    PP_SPECIALM2APPLKBN         '特例Ｍ２適用区分
    PP_SPECIALM3APPLKBN         '特例Ｍ３適用区分
    PP_HOKKAIDOAPPLKBN          '北海道先方負担
    PP_NIIGATAAPPLKBN           '新潟先方負担
    PP_REFLECTFLG               '反映フラグ
    PP_SKIPFLG                  '読み飛ばしフラグ
    PP_DELFLG                   '削除フラグ
    PP_INITYMD                  '登録年月日
    PP_INITUSER                 '登録ユーザーＩＤ
    PP_INITTERMID               '登録端末
    PP_INITPGID                 '登録プログラムＩＤ
    PP_UPDYMD                   '更新年月日
    PP_UPDUSER                  '更新ユーザーＩＤ
    PP_UPDTERMID                '更新端末
    PP_UPDPGID                  '更新プログラムＩＤ
End Enum

''' <summary>
''' 受注データテーブル登録クラス
''' </summary>
''' <remarks>各種受注データテーブルに登録する際はこちらに定義</remarks>
Public Class EntryOrderData


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
        Dim strOrderNo As String = ""

        With sqlText
            .AppendLine("SELECT")
            .AppendLine("    'CT' + FORMAT(CURDATE(),'yyMMdd') + FORMAT(NEXT VALUE FOR LNG.order_sequence,'00000') AS ORDERNO")
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            strOrderNo = GetStringValue(sqlRetSet, 0, "ORDERNO")
        End If

        Return strOrderNo

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
    ''' 受注データ(ヘッダ)TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertOrderHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("INSERT INTO LNG.LNT0004_ORDERHEAD (")
        sqlOrderStat.AppendLine("    ORDERNO")                'オーダーNo
        sqlOrderStat.AppendLine("  , PLANDEPYMD")             '発送予定日
        sqlOrderStat.AppendLine("  , CTNTYPE")                'コンテナ形式
        sqlOrderStat.AppendLine("  , CTNNO")                  'コンテナ番号
        sqlOrderStat.AppendLine("  , STATUS")                 '状態
        sqlOrderStat.AppendLine("  , BIGCTNCD")               '大分類コード
        sqlOrderStat.AppendLine("  , MIDDLECTNCD")            '中分類コード
        sqlOrderStat.AppendLine("  , SMALLCTNCD")             '小分類コード
        sqlOrderStat.AppendLine("  , RENTRATE125NEXTFLG")     '125キロ賃率次期フラグ
        sqlOrderStat.AppendLine("  , RENTRATE125")            '125キロ賃率
        sqlOrderStat.AppendLine("  , ROUNDFEENEXTFLG")        '端数金額基準次期フラグ
        sqlOrderStat.AppendLine("  , ROUNDFEE")               '端数金額基準
        sqlOrderStat.AppendLine("  , ROUNDKBNGE")             '端数区分金額以上
        sqlOrderStat.AppendLine("  , ROUNDKBNLT")             '端数区分金額未満
        sqlOrderStat.AppendLine("  , FILEID")                 'ファイルID
        sqlOrderStat.AppendLine("  , REFLECTFLG")             '反映フラグ
        sqlOrderStat.AppendLine("  , DELFLG")                 '削除フラグ
        sqlOrderStat.AppendLine("  , INITYMD")                '登録年月日
        sqlOrderStat.AppendLine("  , INITUSER")               '登録ユーザーＩＤ
        sqlOrderStat.AppendLine("  , INITTERMID")             '登録端末
        sqlOrderStat.AppendLine("  , INITPGID")               '登録プログラムＩＤ
        sqlOrderStat.AppendLine(")")
        sqlOrderStat.AppendLine(" VALUES(")
        sqlOrderStat.AppendLine("    @ORDERNO")               'オーダーNo             
        sqlOrderStat.AppendLine("  , @PLANDEPYMD")            '発送予定日
        sqlOrderStat.AppendLine("  , @CTNTYPE")               'コンテナ形式
        sqlOrderStat.AppendLine("  , @CTNNO")                 'コンテナ番号
        sqlOrderStat.AppendLine("  , @STATUS")                '状態
        sqlOrderStat.AppendLine("  , @BIGCTNCD")              '大分類コード
        sqlOrderStat.AppendLine("  , @MIDDLECTNCD")           '中分類コード
        sqlOrderStat.AppendLine("  , @SMALLCTNCD")            '小分類コード
        sqlOrderStat.AppendLine("  , @RENTRATE125NEXTFLG")    '125キロ賃率次期フラグ
        sqlOrderStat.AppendLine("  , @RENTRATE125")           '125キロ賃率
        sqlOrderStat.AppendLine("  , @ROUNDFEENEXTFLG")       '端数金額基準次期フラグ
        sqlOrderStat.AppendLine("  , @ROUNDFEE")              '端数金額基準
        sqlOrderStat.AppendLine("  , @ROUNDKBNGE")            '端数区分金額以上
        sqlOrderStat.AppendLine("  , @ROUNDKBNLT")            '端数区分金額未満
        sqlOrderStat.AppendLine("  , @FILEID")                'ファイルID
        sqlOrderStat.AppendLine("  , @REFLECTFLG")            '反映フラグ
        sqlOrderStat.AppendLine("  , @DELFLG")                '削除フラグ
        sqlOrderStat.AppendLine("  , @INITYMD")               '登録年月日
        sqlOrderStat.AppendLine("  , @INITUSER")              '登録ユーザーＩＤ
        sqlOrderStat.AppendLine("  , @INITTERMID")            '登録端末
        sqlOrderStat.AppendLine("  , @INITPGID")              '登録プログラムＩＤ
        sqlOrderStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htHeadData(C_HEADPARAM.HP_ORDERNO)                               'オーダーNo
                .Add("PLANDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_PLANDEPYMD))             '発送予定日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_CTNTYPE))                   'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_CTNNO))                            'コンテナ番号
                .Add("STATUS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_STATUS))                     '状態
                .Add("BIGCTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_BIGCTNCD))                      '大分類コード
                .Add("MIDDLECTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_MIDDLECTNCD))                '中分類コード
                .Add("SMALLCTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_SMALLCTNCD))                  '小分類コード
                .Add("RENTRATE125NEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_RENTRATE125NEXTFLG))  '125キロ賃率次期フラグ
                .Add("RENTRATE125", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_RENTRATE125))                '125キロ賃率
                .Add("ROUNDFEENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDFEENEXTFLG))        '端数金額基準次期フラグ
                .Add("ROUNDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDFEE))                       '端数金額基準
                .Add("ROUNDKBNGE", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDKBNGE))                  '端数区分金額以上
                .Add("ROUNDKBNLT", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDKBNLT))                  '端数区分金額未満
                .Add("FILEID", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_FILEID))                          'ファイルID
                .Add("REFLECTFLG", MySqlDbType.VarChar).Value = htHeadData(C_HEADPARAM.HP_REFLECTFLG)                            '反映フラグ
                .Add("DELFLG", MySqlDbType.VarChar).Value = htHeadData(C_HEADPARAM.HP_DELFLG)                                    '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_INITYMD))                   '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_INITUSER))                 '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_INITTERMID))             '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_INITPGID))                 '登録プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 受注データ(ヘッダ)TBL更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub UpdateOrderHead(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)

        '◯受注TBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("UPDATE LNG.LNT0004_ORDERHEAD ")
        sqlOrderStat.AppendLine("SET")
        sqlOrderStat.AppendLine("    PLANDEPYMD = @PLANDEPYMD")                    '発送予定日
        sqlOrderStat.AppendLine("  , CTNTYPE = @CTNTYPE")                          'コンテナ形式
        sqlOrderStat.AppendLine("  , CTNNO = @CTNNO")                              'コンテナ番号
        sqlOrderStat.AppendLine("  , STATUS = @STATUS")                            '状態
        sqlOrderStat.AppendLine("  , BIGCTNCD = @BIGCTNCD")                        '大分類コード
        sqlOrderStat.AppendLine("  , MIDDLECTNCD = @MIDDLECTNCD")                  '中分類コード
        sqlOrderStat.AppendLine("  , SMALLCTNCD = @SMALLCTNCD")                    '小分類コード
        sqlOrderStat.AppendLine("  , RENTRATE125NEXTFLG = @RENTRATE125NEXTFLG")    '125キロ賃率次期フラグ
        sqlOrderStat.AppendLine("  , RENTRATE125 = @RENTRATE125")                  '125キロ賃率
        sqlOrderStat.AppendLine("  , ROUNDFEENEXTFLG = @ROUNDFEENEXTFLG")          '端数金額基準次期フラグ
        sqlOrderStat.AppendLine("  , ROUNDFEE = @ROUNDFEE")                        '端数金額基準
        sqlOrderStat.AppendLine("  , ROUNDKBNGE = @ROUNDKBNGE")                    '端数区分金額以上
        sqlOrderStat.AppendLine("  , ROUNDKBNLT = @ROUNDKBNLT")                    '端数区分金額未満
        sqlOrderStat.AppendLine("  , UPDYMD = @UPDYMD")                            '更新年月日
        sqlOrderStat.AppendLine("  , UPDUSER = @UPDUSER")                          '更新ユーザーＩＤ
        sqlOrderStat.AppendLine("  , UPDTERMID = @UPDTERMID")                      '更新端末
        sqlOrderStat.AppendLine("  , UPDPGID = @UPDPGID")                          '更新プログラムＩＤ
        sqlOrderStat.AppendLine("WHERE")
        sqlOrderStat.AppendLine("    ORDERNO = @ORDERNO")    '受注No
        sqlOrderStat.AppendLine("AND DELFLG     <> '1'")

        Using sqlOrderCmd As New MySqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htHeadData(C_HEADPARAM.HP_ORDERNO)  'オーダーNo
                .Add("PLANDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_PLANDEPYMD)) '発送予定日
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_CTNTYPE))       'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_CTNNO))                'コンテナ番号
                .Add("STATUS", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_STATUS))              '状態
                .Add("BIGCTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_BIGCTNCD))                      '大分類コード
                .Add("MIDDLECTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_MIDDLECTNCD))                '中分類コード
                .Add("SMALLCTNCD", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_SMALLCTNCD))                  '小分類コード
                .Add("RENTRATE125NEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_RENTRATE125NEXTFLG))  '125キロ賃率次期フラグ
                .Add("RENTRATE125", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_RENTRATE125))                '125キロ賃率
                .Add("ROUNDFEENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDFEENEXTFLG))        '端数金額基準次期フラグ
                .Add("ROUNDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDFEE))                      '端数金額基準
                .Add("ROUNDKBNGE", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDKBNGE))                  '端数区分金額以上
                .Add("ROUNDKBNLT", MySqlDbType.Int32).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_ROUNDKBNLT))                  '端数区分金額未満
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDYMD))                     '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDUSER))                   '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDTERMID))               '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDPGID))                   '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 受注データ(ヘッダ)TBL ステータス更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htHeadData">ヘッダデータ</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Sub UpdateOrderHeadStatus(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htHeadData As Hashtable)
        '◯受注TBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("UPDATE LNG.LNT0004_ORDERHEAD ")
        sqlOrderStat.AppendLine("SET")
        sqlOrderStat.AppendLine("    STATUS = @STATUS")        '状態
        sqlOrderStat.AppendLine("  , UPDYMD = @UPDYMD")        '更新年月日
        sqlOrderStat.AppendLine("  , UPDUSER = @UPDUSER")      '更新ユーザーＩＤ
        sqlOrderStat.AppendLine("  , UPDTERMID = @UPDTERMID")  '更新端末
        sqlOrderStat.AppendLine("  , UPDPGID = @UPDPGID")      '更新プログラムＩＤ
        sqlOrderStat.AppendLine("WHERE")
        sqlOrderStat.AppendLine("    ORDERNO = @ORDERNO")      '受注No
        sqlOrderStat.AppendLine("AND DELFLG     <> '1'")

        Using sqlOrderCmd As New MySqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htHeadData(C_HEADPARAM.HP_ORDERNO)                      'オーダーNo
                .Add("STATUS", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_STATUS))         '状態
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDYMD))         '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDUSER))       '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDTERMID))   '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htHeadData(C_HEADPARAM.HP_UPDPGID))       '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Shared Function BlankToDBNull(strTarger As Object) As Object

        If strTarger Is Nothing Then
            Return CType(DBNull.Value, Object)
        ElseIf strTarger.ToString.Trim = "" Then
            Return CType(DBNull.Value, Object)
        Else
            Return strTarger
        End If

    End Function

    ''' <summary>
    ''' 受注データ（明細）TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="htDetailData"></param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」(InsertOrderHistoryで採番した履歴番号と合わせる)
    ''' と「画面ID」(呼出し側のMe.Title）の
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertOrderDetail(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htDetailData As Hashtable)

        '◯受注明細データTBL
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO LNG.LNT0005_ORDERDATA (")
        sqlDetailStat.AppendLine("    ORDERNO")              'オーダーNo
        sqlDetailStat.AppendLine("  , SAMEDAYCNT")           '同日内回数
        sqlDetailStat.AppendLine("  , FILEID")               'ファイルID
        sqlDetailStat.AppendLine("  , FILELINENO")           '行数
        sqlDetailStat.AppendLine("  , ITEMCD")               '品目コード
        sqlDetailStat.AppendLine("  , ITEMATTACHCD")         '品目付属コード
        sqlDetailStat.AppendLine("  , ITEMNM")               '品目名
        sqlDetailStat.AppendLine("  , RAILDEPSTATION")       '鉄道発駅コード
        sqlDetailStat.AppendLine("  , DEPLEASEDLINECD")      '発専用線コード
        sqlDetailStat.AppendLine("  , RAILARRSTATION")       '鉄道着駅コード
        sqlDetailStat.AppendLine("  , ARRLEASEDLINECD")      '着専用線コード
        sqlDetailStat.AppendLine("  , RAWDEPSTATION")        '原発駅
        sqlDetailStat.AppendLine("  , RAWARRSTATION")        '原着駅
        sqlDetailStat.AppendLine("  , DEPSALEPLACECD")       '発コンテナ営業所コード
        sqlDetailStat.AppendLine("  , ARRSALEPLACECD")       '着コンテナ営業所コード
        sqlDetailStat.AppendLine("  , DEPTRUSTEECD")         '発受託人コード
        sqlDetailStat.AppendLine("  , DEPPICKDELTRADERCD")   '発集配業者コード
        sqlDetailStat.AppendLine("  , ARRTRUSTEECD")         '着受託人コード
        sqlDetailStat.AppendLine("  , ARRPICKDELTRADERCD")   '着集配業者コード
        sqlDetailStat.AppendLine("  , ROOTNO")               'ルート番号
        sqlDetailStat.AppendLine("  , DEPTRAINNO")           '発列車番号
        sqlDetailStat.AppendLine("  , ARRTRAINNO")           '着列車番号
        sqlDetailStat.AppendLine("  , POINTFRAMENO")         '指定枠番号
        sqlDetailStat.AppendLine("  , OBGETDISP")            'ＯＢ取得表示
        sqlDetailStat.AppendLine("  , PLANDEPYMD")           '発着予定日付-発車予定日時
        sqlDetailStat.AppendLine("  , PLANARRYMD")           '発着予定日付-到着予定日時
        sqlDetailStat.AppendLine("  , RESULTDEPYMD")         '発着実績日付-発車実績日時
        sqlDetailStat.AppendLine("  , RESULTARRYMD")         '発着実績日付-到着実績日時
        sqlDetailStat.AppendLine("  , CONTRACTCD")           '契約コード
        sqlDetailStat.AppendLine("  , FAREPAYERCD")          '運賃支払者コード
        sqlDetailStat.AppendLine("  , FAREPAYMETHODCD")      '運賃支払方法コード
        sqlDetailStat.AppendLine("  , FARECALCKIRO")         '運賃計算キロ程
        sqlDetailStat.AppendLine("  , FARECALCTUN")          '運賃計算屯数
        sqlDetailStat.AppendLine("  , DISEXTCD")             '割引割増コード
        sqlDetailStat.AppendLine("  , DISRATE")              '割引率
        sqlDetailStat.AppendLine("  , EXTRATE")              '割増率
        sqlDetailStat.AppendLine("  , TOTALNUM")             '総個数
        sqlDetailStat.AppendLine("  , CARGOWEIGHT")          '荷重
        sqlDetailStat.AppendLine("  , COMPENSATION")         '要賠償額
        sqlDetailStat.AppendLine("  , STANDARDYEAR")         '運賃計算基準年
        sqlDetailStat.AppendLine("  , STANDARDMONTH")        '運賃計算基準月
        sqlDetailStat.AppendLine("  , STANDARDDAY")          '運賃計算基準日
        sqlDetailStat.AppendLine("  , RAILFARE")             '鉄道運賃
        sqlDetailStat.AppendLine("  , ADDFARE")              '増運賃
        sqlDetailStat.AppendLine("  , DGADDFARE")            '危険物割増運賃
        sqlDetailStat.AppendLine("  , VALUABLADDFARE")       '貴重品割増運賃
        sqlDetailStat.AppendLine("  , SPECTNADDFARE")        '特コン割増運賃
        sqlDetailStat.AppendLine("  , DEPSALEPLACEFEE")      '発営業所料金
        sqlDetailStat.AppendLine("  , ARRSALEPLACEFEE")      '着営業所料金
        sqlDetailStat.AppendLine("  , COMPENSATIONDISPFEE")  '要賠償額表示金額
        sqlDetailStat.AppendLine("  , OTHERFEE")             'その他料金
        sqlDetailStat.AppendLine("  , SASIZUFEE")            'さしず手数料
        sqlDetailStat.AppendLine("  , TOTALFAREFEE")         '合計運賃料金
        sqlDetailStat.AppendLine("  , STACKFREEKBN")         'コンテナ積空区分
        sqlDetailStat.AppendLine("  , ORDERMONTH")           '受付月
        sqlDetailStat.AppendLine("  , ORDERDAY")             '受付日
        sqlDetailStat.AppendLine("  , LOADENDMONTH")         '積載完了月
        sqlDetailStat.AppendLine("  , LOADENDDAY")           '積載完了日
        sqlDetailStat.AppendLine("  , DEVELOPENDMONTH")      '発達完了月
        sqlDetailStat.AppendLine("  , DEVELOPENDDAY")        '発達完了日
        sqlDetailStat.AppendLine("  , DEVELOPSPETIME")       '発達指定時
        sqlDetailStat.AppendLine("  , CORRECTLOCASTACD")     '訂正所在駅コード
        sqlDetailStat.AppendLine("  , CORRECTNO")            '訂正番号
        sqlDetailStat.AppendLine("  , CORRELNTYPE")          '訂正種別
        sqlDetailStat.AppendLine("  , CORRELNMONTH")         '訂正月
        sqlDetailStat.AppendLine("  , CORRECTDAY")           '訂正日
        sqlDetailStat.AppendLine("  , ONUSLOCASTACD")        '責任所在コード
        sqlDetailStat.AppendLine("  , SHIPPERCD")            '荷送人コード
        sqlDetailStat.AppendLine("  , SHIPPERNM")            '荷送人名
        sqlDetailStat.AppendLine("  , SHIPPERTEL")           '荷送人電話番号
        sqlDetailStat.AppendLine("  , SLCPICKUPADDRESS")     '集荷先住所
        sqlDetailStat.AppendLine("  , SLCPICKUPTEL")         '集荷先電話番号
        sqlDetailStat.AppendLine("  , CONSIGNEECD")          '荷受人コード
        sqlDetailStat.AppendLine("  , CONSIGNEENM")          '荷受人名
        sqlDetailStat.AppendLine("  , CONSIGNEETEL")         '荷受人電話番号
        sqlDetailStat.AppendLine("  , RECEIVERADDRESS")      '配達先住所
        sqlDetailStat.AppendLine("  , RECEIVERTEL")          '配達先電話番号
        sqlDetailStat.AppendLine("  , INSURANCEFEE")         '保険料
        sqlDetailStat.AppendLine("  , SHIPINSURANCEFEE")     '運送保険料金
        sqlDetailStat.AppendLine("  , LOADADVANCEFEE")       '荷掛立替金
        sqlDetailStat.AppendLine("  , SHIPFEE1")             '発送料金１
        sqlDetailStat.AppendLine("  , SHIPFEE2")             '発送料金２
        sqlDetailStat.AppendLine("  , PACKINGFEE")           '梱包料金
        sqlDetailStat.AppendLine("  , ORIGINWORKFEE")        '発地作業料
        sqlDetailStat.AppendLine("  , DEPOTHERFEE")          '発その他料金
        sqlDetailStat.AppendLine("  , PAYMENTFEE")           '着払料
        sqlDetailStat.AppendLine("  , DEPARTUREEETOTAL")     '発側料金計
        sqlDetailStat.AppendLine("  , DEPARTUREEE1")         '到着料金１
        sqlDetailStat.AppendLine("  , DEPARTUREEE2")         '到着料金２
        sqlDetailStat.AppendLine("  , UNPACKINGFEE")         '開梱料金
        sqlDetailStat.AppendLine("  , LANDINGEORKFEE")       '着地作業料
        sqlDetailStat.AppendLine("  , ARROTHERFEE")          '着その他料金
        sqlDetailStat.AppendLine("  , ARRARTUREEETOTAL")     '着側料金計
        sqlDetailStat.AppendLine("  , ARRNITTSUTAX")         '着通運消費税額
        sqlDetailStat.AppendLine("  , SHIPPERPAYMETHOD")     '荷主支払方法
        sqlDetailStat.AppendLine("  , LUCKFEEINVOICENM")     '運地料金請求先名
        sqlDetailStat.AppendLine("  , ARTICLE")              '記事
        sqlDetailStat.AppendLine("  , INPUTHOUR")            '入力時刻(時)
        sqlDetailStat.AppendLine("  , INPUTMINUTE")          '入力時刻(分)
        sqlDetailStat.AppendLine("  , INPUTSECOND")          '入力時刻(秒)
        sqlDetailStat.AppendLine("  , CONSIGNCANCELKBN")     '託送取消区分
        sqlDetailStat.AppendLine("  , WIKUGUTRANKBN")        'ウイクグ輸送区分
        sqlDetailStat.AppendLine("  , YOBI")                 '予備
        sqlDetailStat.AppendLine("  , REFLECTFLG")           '反映フラグ
        sqlDetailStat.AppendLine("  , SKIPFLG")              '読み飛ばしフラグ
        sqlDetailStat.AppendLine("  , DELFLG")               '削除フラグ
        sqlDetailStat.AppendLine("  , INITYMD")              '登録年月日
        sqlDetailStat.AppendLine("  , INITUSER")             '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , INITTERMID")           '登録端末
        sqlDetailStat.AppendLine("  , INITPGID")             '登録プログラムＩＤ
        sqlDetailStat.AppendLine(")")
        sqlDetailStat.AppendLine(" VALUES(")
        sqlDetailStat.AppendLine("    @ORDERNO")               'オーダーNo             
        sqlDetailStat.AppendLine("  , @SAMEDAYCNT")           '同日内回数
        sqlDetailStat.AppendLine("  , @FILEID")               'ファイルID
        sqlDetailStat.AppendLine("  , @FILELINENO")           '行数
        sqlDetailStat.AppendLine("  , @ITEMCD")               '品目コード
        sqlDetailStat.AppendLine("  , @ITEMATTACHCD")         '品目付属コード
        sqlDetailStat.AppendLine("  , @ITEMNM")               '品目名
        sqlDetailStat.AppendLine("  , @RAILDEPSTATION")       '鉄道発駅コード
        sqlDetailStat.AppendLine("  , @DEPLEASEDLINECD")      '発専用線コード
        sqlDetailStat.AppendLine("  , @RAILARRSTATION")       '鉄道着駅コード
        sqlDetailStat.AppendLine("  , @ARRLEASEDLINECD")      '着専用線コード
        sqlDetailStat.AppendLine("  , @RAWDEPSTATION")        '原発駅
        sqlDetailStat.AppendLine("  , @RAWARRSTATION")        '原着駅
        sqlDetailStat.AppendLine("  , @DEPSALEPLACECD")       '発コンテナ営業所コード
        sqlDetailStat.AppendLine("  , @ARRSALEPLACECD")       '着コンテナ営業所コード
        sqlDetailStat.AppendLine("  , @DEPTRUSTEECD")         '発受託人コード
        sqlDetailStat.AppendLine("  , @DEPPICKDELTRADERCD")   '発集配業者コード
        sqlDetailStat.AppendLine("  , @ARRTRUSTEECD")         '着受託人コード
        sqlDetailStat.AppendLine("  , @ARRPICKDELTRADERCD")   '着集配業者コード
        sqlDetailStat.AppendLine("  , @ROOTNO")               'ルート番号
        sqlDetailStat.AppendLine("  , @DEPTRAINNO")           '発列車番号
        sqlDetailStat.AppendLine("  , @ARRTRAINNO")           '着列車番号
        sqlDetailStat.AppendLine("  , @POINTFRAMENO")         '指定枠番号
        sqlDetailStat.AppendLine("  , @OBGETDISP")            'ＯＢ取得表示
        sqlDetailStat.AppendLine("  , @PLANDEPYMD")           '発着予定日付-発車予定日時
        sqlDetailStat.AppendLine("  , @PLANARRYMD")           '発着予定日付-到着予定日時
        sqlDetailStat.AppendLine("  , @RESULTDEPYMD")         '発着実績日付-発車実績日時
        sqlDetailStat.AppendLine("  , @RESULTARRYMD")         '発着実績日付-到着実績日時
        sqlDetailStat.AppendLine("  , @CONTRACTCD")           '契約コード
        sqlDetailStat.AppendLine("  , @FAREPAYERCD")          '運賃支払者コード
        sqlDetailStat.AppendLine("  , @FAREPAYMETHODCD")      '運賃支払方法コード
        sqlDetailStat.AppendLine("  , @FARECALCKIRO")         '運賃計算キロ程
        sqlDetailStat.AppendLine("  , @FARECALCTUN")          '運賃計算屯数
        sqlDetailStat.AppendLine("  , @DISEXTCD")             '割引割増コード
        sqlDetailStat.AppendLine("  , @DISRATE")              '割引率
        sqlDetailStat.AppendLine("  , @EXTRATE")              '割増率
        sqlDetailStat.AppendLine("  , @TOTALNUM")             '総個数
        sqlDetailStat.AppendLine("  , @CARGOWEIGHT")          '荷重
        sqlDetailStat.AppendLine("  , @COMPENSATION")         '要賠償額
        sqlDetailStat.AppendLine("  , @STANDARDYEAR")         '運賃計算基準年
        sqlDetailStat.AppendLine("  , @STANDARDMONTH")        '運賃計算基準月
        sqlDetailStat.AppendLine("  , @STANDARDDAY")          '運賃計算基準日
        sqlDetailStat.AppendLine("  , @RAILFARE")             '鉄道運賃
        sqlDetailStat.AppendLine("  , @ADDFARE")              '増運賃
        sqlDetailStat.AppendLine("  , @DGADDFARE")            '危険物割増運賃
        sqlDetailStat.AppendLine("  , @VALUABLADDFARE")       '貴重品割増運賃
        sqlDetailStat.AppendLine("  , @SPECTNADDFARE")        '特コン割増運賃
        sqlDetailStat.AppendLine("  , @DEPSALEPLACEFEE")      '発営業所料金
        sqlDetailStat.AppendLine("  , @ARRSALEPLACEFEE")      '着営業所料金
        sqlDetailStat.AppendLine("  , @COMPENSATIONDISPFEE")  '要賠償額表示金額
        sqlDetailStat.AppendLine("  , @OTHERFEE")             'その他料金
        sqlDetailStat.AppendLine("  , @SASIZUFEE")            'さしず手数料
        sqlDetailStat.AppendLine("  , @TOTALFAREFEE")         '合計運賃料金
        sqlDetailStat.AppendLine("  , @STACKFREEKBN")         'コンテナ積空区分
        sqlDetailStat.AppendLine("  , @ORDERMONTH")           '受付月
        sqlDetailStat.AppendLine("  , @ORDERDAY")             '受付日
        sqlDetailStat.AppendLine("  , @LOADENDMONTH")         '積載完了月
        sqlDetailStat.AppendLine("  , @LOADENDDAY")           '積載完了日
        sqlDetailStat.AppendLine("  , @DEVELOPENDMONTH")      '発達完了月
        sqlDetailStat.AppendLine("  , @DEVELOPENDDAY")        '発達完了日
        sqlDetailStat.AppendLine("  , @DEVELOPSPETIME")       '発達指定時
        sqlDetailStat.AppendLine("  , @CORRECTLOCASTACD")     '訂正所在駅コード
        sqlDetailStat.AppendLine("  , @CORRECTNO")            '訂正番号
        sqlDetailStat.AppendLine("  , @CORRELNTYPE")          '訂正種別
        sqlDetailStat.AppendLine("  , @CORRELNMONTH")         '訂正月
        sqlDetailStat.AppendLine("  , @CORRECTDAY")           '訂正日
        sqlDetailStat.AppendLine("  , @ONUSLOCASTACD")        '責任所在コード
        sqlDetailStat.AppendLine("  , @SHIPPERCD")            '荷送人コード
        sqlDetailStat.AppendLine("  , @SHIPPERNM")            '荷送人名
        sqlDetailStat.AppendLine("  , @SHIPPERTEL")           '荷送人電話番号
        sqlDetailStat.AppendLine("  , @SLCPICKUPADDRESS")     '集荷先住所
        sqlDetailStat.AppendLine("  , @SLCPICKUPTEL")         '集荷先電話番号
        sqlDetailStat.AppendLine("  , @CONSIGNEECD")          '荷受人コード
        sqlDetailStat.AppendLine("  , @CONSIGNEENM")          '荷受人名
        sqlDetailStat.AppendLine("  , @CONSIGNEETEL")         '荷受人電話番号
        sqlDetailStat.AppendLine("  , @RECEIVERADDRESS")      '配達先住所
        sqlDetailStat.AppendLine("  , @RECEIVERTEL")          '配達先電話番号
        sqlDetailStat.AppendLine("  , @INSURANCEFEE")         '保険料
        sqlDetailStat.AppendLine("  , @SHIPINSURANCEFEE")     '運送保険料金
        sqlDetailStat.AppendLine("  , @LOADADVANCEFEE")       '荷掛立替金
        sqlDetailStat.AppendLine("  , @SHIPFEE1")             '発送料金１
        sqlDetailStat.AppendLine("  , @SHIPFEE2")             '発送料金２
        sqlDetailStat.AppendLine("  , @PACKINGFEE")           '梱包料金
        sqlDetailStat.AppendLine("  , @ORIGINWORKFEE")        '発地作業料
        sqlDetailStat.AppendLine("  , @DEPOTHERFEE")          '発その他料金
        sqlDetailStat.AppendLine("  , @PAYMENTFEE")           '着払料
        sqlDetailStat.AppendLine("  , @DEPARTUREEETOTAL")     '発側料金計
        sqlDetailStat.AppendLine("  , @DEPARTUREEE1")         '到着料金１
        sqlDetailStat.AppendLine("  , @DEPARTUREEE2")         '到着料金２
        sqlDetailStat.AppendLine("  , @UNPACKINGFEE")         '開梱料金
        sqlDetailStat.AppendLine("  , @LANDINGEORKFEE")       '着地作業料
        sqlDetailStat.AppendLine("  , @ARROTHERFEE")          '着その他料金
        sqlDetailStat.AppendLine("  , @ARRARTUREEETOTAL")     '着側料金計
        sqlDetailStat.AppendLine("  , @ARRNITTSUTAX")         '着通運消費税額
        sqlDetailStat.AppendLine("  , @SHIPPERPAYMETHOD")     '荷主支払方法
        sqlDetailStat.AppendLine("  , @LUCKFEEINVOICENM")     '運地料金請求先名
        sqlDetailStat.AppendLine("  , @ARTICLE")              '記事
        sqlDetailStat.AppendLine("  , @INPUTHOUR")            '入力時刻(時)
        sqlDetailStat.AppendLine("  , @INPUTMINUTE")          '入力時刻(分)
        sqlDetailStat.AppendLine("  , @INPUTSECOND")          '入力時刻(秒)
        sqlDetailStat.AppendLine("  , @CONSIGNCANCELKBN")     '託送取消区分
        sqlDetailStat.AppendLine("  , @WIKUGUTRANKBN")        'ウイクグ輸送区分
        sqlDetailStat.AppendLine("  , @YOBI")                 '予備
        sqlDetailStat.AppendLine("  , @REFLECTFLG")           '反映フラグ
        sqlDetailStat.AppendLine("  , @SKIPFLG")              '読み飛ばしフラグ
        sqlDetailStat.AppendLine("  , @DELFLG")               '削除フラグ
        sqlDetailStat.AppendLine("  , @INITYMD")              '登録年月日
        sqlDetailStat.AppendLine("  , @INITUSER")             '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , @INITTERMID")           '登録端末
        sqlDetailStat.AppendLine("  , @INITPGID")             '登録プログラムＩＤ
        sqlDetailStat.AppendLine(")")

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_ORDERNO)                                            'オーダーNo
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SAMEDAYCNT))                       '同日内回数
                .Add("FILEID", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FILEID))                               'ファイルID
                .Add("FILELINENO", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FILELINENO))                       '行数
                .Add("ITEMCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ITEMCD))                          '品目コード
                .Add("ITEMATTACHCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ITEMATTACHCD))              '品目付属コード
                .Add("ITEMNM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ITEMNM))                          '品目名
                .Add("RAILDEPSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAILDEPSTATION))          '鉄道発駅コード
                .Add("DEPLEASEDLINECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPLEASEDLINECD))        '発専用線コード
                .Add("RAILARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAILARRSTATION))          '鉄道着駅コード
                .Add("ARRLEASEDLINECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRLEASEDLINECD))        '着専用線コード
                .Add("RAWDEPSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAWDEPSTATION))            '原発駅
                .Add("RAWARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAWARRSTATION))            '原着駅
                .Add("DEPSALEPLACECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPSALEPLACECD))          '発コンテナ営業所コード
                .Add("ARRSALEPLACECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRSALEPLACECD))          '着コンテナ営業所コード
                .Add("DEPTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPTRUSTEECD))              '発受託人コード
                .Add("DEPPICKDELTRADERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPPICKDELTRADERCD))  '発集配業者コード
                .Add("ARRTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRTRUSTEECD))              '着受託人コード
                .Add("ARRPICKDELTRADERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRPICKDELTRADERCD))  '着集配業者コード
                .Add("ROOTNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ROOTNO))                          'ルート番号
                .Add("DEPTRAINNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPTRAINNO))                  '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRTRAINNO))                  '着列車番号
                .Add("POINTFRAMENO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_POINTFRAMENO))              '指定枠番号
                .Add("OBGETDISP", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_OBGETDISP))                    'ＯＢ取得表示
                .Add("PLANDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PLANDEPYMD))                  '発着予定日付-発車予定日時
                .Add("PLANARRYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PLANARRYMD))                  '発着予定日付-到着予定日時
                .Add("RESULTDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RESULTDEPYMD))              '発着実績日付-発車実績日時
                .Add("RESULTARRYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RESULTARRYMD))              '発着実績日付-到着実績日時
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONTRACTCD))                  '契約コード
                .Add("FAREPAYERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FAREPAYERCD))                '運賃支払者コード
                .Add("FAREPAYMETHODCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FAREPAYMETHODCD))        '運賃支払方法コード
                .Add("FARECALCKIRO", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FARECALCKIRO))                   '運賃計算キロ程
                .Add("FARECALCTUN", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_FARECALCTUN))                     '運賃計算屯数
                .Add("DISEXTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DISEXTCD))                      '割引割増コード
                .Add("DISRATE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DISRATE))                             '割引率
                .Add("EXTRATE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_EXTRATE))                             '割増率
                .Add("TOTALNUM", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_TOTALNUM))                           '総個数
                .Add("CARGOWEIGHT", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CARGOWEIGHT))                     '荷重
                .Add("COMPENSATION", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_COMPENSATION))                   '要賠償額
                .Add("STANDARDYEAR", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_STANDARDYEAR))              '運賃計算基準年
                .Add("STANDARDMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_STANDARDMONTH))            '運賃計算基準月
                .Add("STANDARDDAY", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_STANDARDDAY))                '運賃計算基準日
                .Add("RAILFARE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAILFARE))                           '鉄道運賃
                .Add("ADDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ADDFARE))                             '増運賃
                .Add("DGADDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DGADDFARE))                         '危険物割増運賃
                .Add("VALUABLADDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_VALUABLADDFARE))               '貴重品割増運賃
                .Add("SPECTNADDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SPECTNADDFARE))                 '特コン割増運賃
                .Add("DEPSALEPLACEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPSALEPLACEFEE))             '発営業所料金
                .Add("ARRSALEPLACEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRSALEPLACEFEE))             '着営業所料金
                .Add("COMPENSATIONDISPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_COMPENSATIONDISPFEE))     '要賠償額表示金額
                .Add("OTHERFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_OTHERFEE))                           'その他料金
                .Add("SASIZUFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SASIZUFEE))                         'さしず手数料
                .Add("TOTALFAREFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_TOTALFAREFEE))                   '合計運賃料金
                .Add("STACKFREEKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_STACKFREEKBN))              'コンテナ積空区分
                .Add("ORDERMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ORDERMONTH))                  '受付月
                .Add("ORDERDAY", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ORDERDAY))                      '受付日
                .Add("LOADENDMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_LOADENDMONTH))              '積載完了月
                .Add("LOADENDDAY", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_LOADENDDAY))                  '積載完了日
                .Add("DEVELOPENDMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEVELOPENDMONTH))        '発達完了月
                .Add("DEVELOPENDDAY", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEVELOPENDDAY))            '発達完了日
                .Add("DEVELOPSPETIME", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEVELOPSPETIME))          '発達指定時
                .Add("CORRECTLOCASTACD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CORRECTLOCASTACD))      '訂正所在駅コード
                .Add("CORRECTNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CORRECTNO))                    '訂正番号
                .Add("CORRELNTYPE", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CORRELNTYPE))                '訂正種別
                .Add("CORRELNMONTH", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CORRELNMONTH))              '訂正月
                .Add("CORRECTDAY", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CORRECTDAY))                  '訂正日
                .Add("ONUSLOCASTACD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ONUSLOCASTACD))            '責任所在コード
                .Add("SHIPPERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERCD))                    '荷送人コード
                .Add("SHIPPERNM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERNM))                    '荷送人名
                .Add("SHIPPERTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERTEL))                  '荷送人電話番号
                .Add("SLCPICKUPADDRESS", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SLCPICKUPADDRESS))      '集荷先住所
                .Add("SLCPICKUPTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SLCPICKUPTEL))              '集荷先電話番号
                .Add("CONSIGNEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONSIGNEECD))                '荷受人コード
                .Add("CONSIGNEENM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONSIGNEENM))                '荷受人名
                .Add("CONSIGNEETEL", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONSIGNEETEL))              '荷受人電話番号
                .Add("RECEIVERADDRESS", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RECEIVERADDRESS))        '配達先住所
                .Add("RECEIVERTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RECEIVERTEL))                '配達先電話番号
                .Add("INSURANCEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INSURANCEFEE))                   '保険料
                .Add("SHIPINSURANCEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPINSURANCEFEE))           '運送保険料金
                .Add("LOADADVANCEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_LOADADVANCEFEE))               '荷掛立替金
                .Add("SHIPFEE1", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPFEE1))                           '発送料金１
                .Add("SHIPFEE2", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPFEE2))                           '発送料金２
                .Add("PACKINGFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PACKINGFEE))                       '梱包料金
                .Add("ORIGINWORKFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ORIGINWORKFEE))                 '発地作業料
                .Add("DEPOTHERFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPOTHERFEE))                     '発その他料金
                .Add("PAYMENTFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PAYMENTFEE))                       '着払料
                .Add("DEPARTUREEETOTAL", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPARTUREEETOTAL))           '発側料金計
                .Add("DEPARTUREEE1", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPARTUREEE1))                   '到着料金１
                .Add("DEPARTUREEE2", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPARTUREEE2))                   '到着料金２
                .Add("UNPACKINGFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_UNPACKINGFEE))                   '開梱料金
                .Add("LANDINGEORKFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_LANDINGEORKFEE))               '着地作業料
                .Add("ARROTHERFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARROTHERFEE))                     '着その他料金
                .Add("ARRARTUREEETOTAL", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRARTUREEETOTAL))           '着側料金計
                .Add("ARRNITTSUTAX", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRNITTSUTAX))                   '着通運消費税額
                .Add("SHIPPERPAYMETHOD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERPAYMETHOD))      '荷主支払方法
                .Add("LUCKFEEINVOICENM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_LUCKFEEINVOICENM))      '運地料金請求先名
                .Add("ARTICLE", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARTICLE))                        '記事
                .Add("INPUTHOUR", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INPUTHOUR))                    '入力時刻(時)
                .Add("INPUTMINUTE", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INPUTMINUTE))                '入力時刻(分)
                .Add("INPUTSECOND", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INPUTSECOND))                '入力時刻(秒)
                .Add("CONSIGNCANCELKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONSIGNCANCELKBN))      '託送取消区分
                .Add("WIKUGUTRANKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_WIKUGUTRANKBN))            'ウイクグ輸送区分
                .Add("YOBI", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_YOBI))                              '予備
                .Add("REFLECTFLG", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_REFLECTFLG)                                 '反映フラグ
                .Add("SKIPFLG", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_SKIPFLG)                                       '読み飛ばしフラグ
                .Add("DELFLG", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_DELFLG)                                         '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INITYMD))                        '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INITUSER))                      '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INITTERMID))                  '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_INITPGID))                      '登録プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' 受注データ（明細）TBL更新処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="htDetailData"></param>
    ''' <remarks>フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub UpdateOrderDetail(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htDetailData As Hashtable)

        '◯受注明細データTBL
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0005_ORDERDATA ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    ITEMCD = @ITEMCD")                           '品目コード
        sqlDetailStat.AppendLine("  , ITEMNM = @ITEMNM")                           '品目名
        sqlDetailStat.AppendLine("  , RAILDEPSTATION = @RAILDEPSTATION")           '鉄道発駅コード
        sqlDetailStat.AppendLine("  , RAILARRSTATION = @RAILARRSTATION")           '鉄道着駅コード
        sqlDetailStat.AppendLine("  , RAWDEPSTATION = @RAWDEPSTATION")             '原発駅
        sqlDetailStat.AppendLine("  , RAWARRSTATION = @RAWARRSTATION")             '原着駅
        sqlDetailStat.AppendLine("  , DEPTRUSTEECD = @DEPTRUSTEECD")               '発受託人コード
        sqlDetailStat.AppendLine("  , DEPPICKDELTRADERCD = @DEPPICKDELTRADERCD")   '発集配業者コード
        sqlDetailStat.AppendLine("  , ARRTRUSTEECD = @ARRTRUSTEECD")               '着受託人コード
        sqlDetailStat.AppendLine("  , ARRPICKDELTRADERCD = @ARRPICKDELTRADERCD")   '着集配業者コード
        sqlDetailStat.AppendLine("  , DEPTRAINNO = @DEPTRAINNO")                   '発列車番号
        sqlDetailStat.AppendLine("  , ARRTRAINNO = @ARRTRAINNO")                   '着列車番号
        sqlDetailStat.AppendLine("  , PLANDEPYMD = @PLANDEPYMD")                   '発着予定日付-発車予定日時
        sqlDetailStat.AppendLine("  , PLANARRYMD = @PLANARRYMD")                   '発着予定日付-到着予定日時
        sqlDetailStat.AppendLine("  , RESULTDEPYMD = @RESULTDEPYMD")               '発着実績日付-発車実績日時
        sqlDetailStat.AppendLine("  , RESULTARRYMD = @RESULTARRYMD")               '発着実績日付-到着実績日時
        sqlDetailStat.AppendLine("  , CONTRACTCD = @CONTRACTCD")                   '契約コード
        sqlDetailStat.AppendLine("  , OTHERFEE = @OTHERFEE")                       'その他料金
        sqlDetailStat.AppendLine("  , STACKFREEKBN = @STACKFREEKBN")               'コンテナ積空区分
        sqlDetailStat.AppendLine("  , SHIPPERCD = @SHIPPERCD")                     '荷送人コード
        sqlDetailStat.AppendLine("  , SHIPPERNM = @SHIPPERNM")                     '荷送人名
        sqlDetailStat.AppendLine("  , SLCPICKUPTEL = @SLCPICKUPTEL")               '集荷先電話番号
        sqlDetailStat.AppendLine("  , DELFLG = @DELFLG")                           '削除フラグ
        sqlDetailStat.AppendLine("  , UPDYMD = @UPDYMD")                           '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER = @UPDUSER")                         '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID = @UPDTERMID")                     '更新端末
        sqlDetailStat.AppendLine("  , UPDPGID = @UPDPGID")                         '更新プログラムＩＤ
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    ORDERNO = @ORDERNO")          '受注No
        sqlDetailStat.AppendLine("AND SAMEDAYCNT = @SAMEDAYCNT")    '同日内回数

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_ORDERNO)                                       'オーダーNo
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SAMEDAYCNT))                       '同日内回数
                .Add("ITEMCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ITEMCD))                          '品目コード
                .Add("ITEMNM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ITEMNM))                          '品目名
                .Add("RAILDEPSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAILDEPSTATION))          '鉄道発駅コード
                .Add("RAILARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAILARRSTATION))          '鉄道着駅コード
                .Add("RAWDEPSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAWDEPSTATION))            '原発駅
                .Add("RAWARRSTATION", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RAWARRSTATION))            '原着駅
                .Add("DEPTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPTRUSTEECD))              '発受託人コード
                .Add("DEPPICKDELTRADERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPPICKDELTRADERCD))  '発集配業者コード
                .Add("ARRTRUSTEECD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRTRUSTEECD))              '着受託人コード
                .Add("ARRPICKDELTRADERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRPICKDELTRADERCD))  '着集配業者コード
                .Add("DEPTRAINNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_DEPTRAINNO))                  '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_ARRTRAINNO))                  '着列車番号
                .Add("PLANDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PLANDEPYMD))                  '発着予定日付-発車予定日時
                .Add("PLANARRYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_PLANARRYMD))                  '発着予定日付-到着予定日時
                .Add("RESULTDEPYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RESULTDEPYMD))              '発着実績日付-発車実績日時
                .Add("RESULTARRYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_RESULTARRYMD))              '発着実績日付-到着実績日時
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_CONTRACTCD))                  '契約コード
                .Add("OTHERFEE", MySqlDbType.Int32).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_OTHERFEE))                           'その他料金
                .Add("STACKFREEKBN", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_STACKFREEKBN))              'コンテナ積空区分
                .Add("SHIPPERCD", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERCD))                    '荷送人コード
                .Add("SHIPPERNM", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SHIPPERNM))                    '荷送人名
                .Add("SLCPICKUPTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_SLCPICKUPTEL))              '集荷先電話番号
                .Add("DELFLG", MySqlDbType.VarChar).Value = htDetailData(C_DATAPARAM.DP_DELFLG)                                         '削除フラグ
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_UPDYMD))                          '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_UPDUSER))                        '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_UPDTERMID))                    '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htDetailData(C_DATAPARAM.DP_UPDPGID))                        '更新プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' 精算予定ファイルTBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htPlanfData">精算予定ファイルデータ</param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertPayPlanf(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htPlanfData As Hashtable)
        '◯精算予定ファイルTBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("INSERT INTO LNG.LNT0006_PAYPLANF (")
        sqlOrderStat.AppendLine("    ORDERNO              ")    'オーダーNo
        sqlOrderStat.AppendLine("  , SAMEDAYCNT           ")    '同日内回数
        sqlOrderStat.AppendLine("  , SHIPYMD              ")    '発送年月日
        sqlOrderStat.AppendLine("  , LINENUM              ")    '行番
        sqlOrderStat.AppendLine("  , JOTDEPBRANCHCD       ")    'ＪＯＴ発店所コード
        sqlOrderStat.AppendLine("  , DEPSTATION           ")    '発駅コード
        sqlOrderStat.AppendLine("  , DEPTRUSTEECD         ")    '発受託人コード
        sqlOrderStat.AppendLine("  , DEPTRUSTEESUBCD      ")    '発受託人サブ
        sqlOrderStat.AppendLine("  , JOTARRBRANCHCD       ")    'ＪＯＴ着店所コード
        sqlOrderStat.AppendLine("  , ARRSTATION           ")    '着駅コード
        sqlOrderStat.AppendLine("  , ARRTRUSTEECD         ")    '着受託人コード
        sqlOrderStat.AppendLine("  , ARRTRUSTEESUBCD      ")    '着受託人サブ
        sqlOrderStat.AppendLine("  , ARRPLANYMD           ")    '到着予定年月日
        sqlOrderStat.AppendLine("  , STACKFREEKBN         ")    '積空区分
        sqlOrderStat.AppendLine("  , STATUSKBN            ")    '状態区分
        sqlOrderStat.AppendLine("  , CONTRACTCD           ")    '契約コード
        sqlOrderStat.AppendLine("  , DEPTRAINNO           ")    '発列車番号
        sqlOrderStat.AppendLine("  , ARRTRAINNO           ")    '着列車番号
        sqlOrderStat.AppendLine("  , JRITEMCD             ")    'ＪＲ品目コード
        sqlOrderStat.AppendLine("  , LEASEPRODUCTCD       ")    'リース品名コード
        sqlOrderStat.AppendLine("  , DEPSHIPPERCD         ")    '発荷主コード
        sqlOrderStat.AppendLine("  , QUANTITY             ")    '個数
        sqlOrderStat.AppendLine("  , ADDSUBYM             ")    '加減額の対象年月
        sqlOrderStat.AppendLine("  , ADDSUBQUANTITY       ")    '加減額の個数
        sqlOrderStat.AppendLine("  , JRFIXEDFARE          ")    'ＪＲ所定運賃
        sqlOrderStat.AppendLine("  , USEFEE               ")    '使用料金額
        sqlOrderStat.AppendLine("  , OWNDISCOUNTFEE       ")    '私有割引相当額
        sqlOrderStat.AppendLine("  , RETURNFARE           ")    '割戻し運賃
        sqlOrderStat.AppendLine("  , NITTSUFREESENDFEE    ")    '通運負担回送運賃
        sqlOrderStat.AppendLine("  , MANAGEFEE            ")    '運行管理料
        sqlOrderStat.AppendLine("  , SHIPBURDENFEE        ")    '荷主負担運賃
        sqlOrderStat.AppendLine("  , SHIPFEE              ")    '発送料
        sqlOrderStat.AppendLine("  , ARRIVEFEE            ")    '到着料
        sqlOrderStat.AppendLine("  , PICKUPFEE            ")    '集荷料
        sqlOrderStat.AppendLine("  , DELIVERYFEE          ")    '配達料
        sqlOrderStat.AppendLine("  , OTHER1FEE            ")    'その他１
        sqlOrderStat.AppendLine("  , OTHER2FEE            ")    'その他２
        sqlOrderStat.AppendLine("  , FREESENDFEE          ")    '回送運賃
        sqlOrderStat.AppendLine("  , SPRFITKBN            ")    '冷蔵適合マーク
        sqlOrderStat.AppendLine("  , JURISDICTIONCD       ")    '所管部コード
        sqlOrderStat.AppendLine("  , ACCOUNTINGASSETSCD   ")    '経理資産コード
        sqlOrderStat.AppendLine("  , ACCOUNTINGASSETSKBN  ")    '経理資産区分
        sqlOrderStat.AppendLine("  , DUMMYKBN             ")    'ダミー区分
        sqlOrderStat.AppendLine("  , SPOTKBN              ")    'スポット区分
        sqlOrderStat.AppendLine("  , COMPKANKBN           ")    '複合一貫区分
        sqlOrderStat.AppendLine("  , KEIJOYM              ")    '計上年月
        sqlOrderStat.AppendLine("  , PARTNERCAMPCD        ")    '相手先会社コード
        sqlOrderStat.AppendLine("  , PARTNERDEPTCD        ")    '相手先部門コード
        sqlOrderStat.AppendLine("  , INVKEIJYOBRANCHCD    ")    '請求項目 計上店コード
        sqlOrderStat.AppendLine("  , INVFILINGDEPT        ")    '請求項目 請求書提出部店
        sqlOrderStat.AppendLine("  , INVKESAIKBN          ")    '請求項目 請求書決済区分
        sqlOrderStat.AppendLine("  , INVSUBCD             ")    '請求項目 請求書細分コード
        sqlOrderStat.AppendLine("  , PAYKEIJYOBRANCHCD    ")    '支払項目 費用計上店コード
        sqlOrderStat.AppendLine("  , PAYFILINGBRANCH      ")    '支払項目 支払書提出支店
        sqlOrderStat.AppendLine("  , TAXCALCUNIT          ")    '支払項目 消費税計算単位
        sqlOrderStat.AppendLine("  , TAXKBN               ")    '税区分
        sqlOrderStat.AppendLine("  , TAXRATE              ")    '税率
        sqlOrderStat.AppendLine("  , BEFDEPTRUSTEECD      ")    '変換前項目-発受託人コード
        sqlOrderStat.AppendLine("  , BEFDEPTRUSTEESUBCD   ")    '変換前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , BEFDEPSHIPPERCD      ")    '変換前項目-発荷主コード
        sqlOrderStat.AppendLine("  , BEFARRTRUSTEECD      ")    '変換前項目-着受託人コード
        sqlOrderStat.AppendLine("  , BEFARRTRUSTEESUBCD   ")    '変換前項目-着受託人サブ
        sqlOrderStat.AppendLine("  , BEFJRITEMCD          ")    '変換前項目-ＪＲ品目コード
        sqlOrderStat.AppendLine("  , BEFSTACKFREEKBN      ")    '変換前項目-積空区分
        sqlOrderStat.AppendLine("  , SPLBEFDEPSTATION     ")    '分割前項目-発駅コード
        sqlOrderStat.AppendLine("  , SPLBEFDEPTRUSTEECD   ")    '分割前項目-発受託人コード
        sqlOrderStat.AppendLine("  , SPLBEFDEPTRUSTEESUBCD")    '分割前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , SPLBEFUSEFEE         ")    '分割前項目-使用料金額
        sqlOrderStat.AppendLine("  , SPLBEFSHIPFEE        ")    '分割前項目-発送料
        sqlOrderStat.AppendLine("  , SPLBEFARRIVEFEE      ")    '分割前項目-到着料
        sqlOrderStat.AppendLine("  , SPLBEFFREESENDFEE    ")    '分割前項目-回送運賃
        sqlOrderStat.AppendLine("  , PROCFLG1             ")    '処理フラグ-料金計算済
        sqlOrderStat.AppendLine("  , PROCFLG2             ")    '処理フラグ-精算ファイル作成済
        sqlOrderStat.AppendLine("  , PROCFLG3             ")    '処理フラグ-運用ファイル作成済
        sqlOrderStat.AppendLine("  , PROCFLG4             ")    '処理フラグ-複合一貫作成済
        sqlOrderStat.AppendLine("  , PROCFLG5             ")    '処理フラグ-請求支払分割済
        sqlOrderStat.AppendLine("  , PROCFLG6             ")    '処理フラグ-コード変換済
        sqlOrderStat.AppendLine("  , PROCFLG7             ")    '処理フラグ-ダミーフラグ７
        sqlOrderStat.AppendLine("  , PROCFLG8             ")    '処理フラグ-ダミーフラグ８
        sqlOrderStat.AppendLine("  , PROCFLG9             ")    '処理フラグ-ダミーフラグ９
        sqlOrderStat.AppendLine("  , PROCFLG10            ")    '処理フラグ-ダミーフラグ１０
        sqlOrderStat.AppendLine("  , PICKUPTEL            ")    '集荷先電話番号
        sqlOrderStat.AppendLine("  , FARECALCTUNAPPLKBN   ")    '運賃計算屯数適用区分
        sqlOrderStat.AppendLine("  , FARECALCTUNNEXTFLG   ")    '運賃計算屯数次期フラグ
        sqlOrderStat.AppendLine("  , FARECALCTUN          ")    '運賃計算屯数
        sqlOrderStat.AppendLine("  , DISNO                ")    '割引番号
        sqlOrderStat.AppendLine("  , EXTNO                ")    '割増番号
        sqlOrderStat.AppendLine("  , KIROAPPLKBN          ")    'キロ程適用区分
        sqlOrderStat.AppendLine("  , KIRO                 ")    'キロ程
        sqlOrderStat.AppendLine("  , RENTRATEAPPLKBN      ")    '賃率適用区分
        sqlOrderStat.AppendLine("  , RENTRATENEXTFLG      ")    '賃率次期フラグ
        sqlOrderStat.AppendLine("  , RENTRATE             ")    '賃率
        sqlOrderStat.AppendLine("  , APPLYRATEAPPLKBN     ")    '適用率適用区分
        sqlOrderStat.AppendLine("  , APPLYRATENEXTFLG     ")    '適用率次期フラグ
        sqlOrderStat.AppendLine("  , APPLYRATE            ")    '適用率
        sqlOrderStat.AppendLine("  , USEFEERATEAPPLKBN    ")    '使用料率適用区分
        sqlOrderStat.AppendLine("  , USEFEERATE           ")    '使用料率
        sqlOrderStat.AppendLine("  , FREESENDRATEAPPLKBN  ")    '回送運賃適用率適用区分
        sqlOrderStat.AppendLine("  , FREESENDRATENEXTFLG  ")    '回送運賃適用率次期フラグ
        sqlOrderStat.AppendLine("  , FREESENDRATE         ")    '回送運賃適用率
        sqlOrderStat.AppendLine("  , SHIPFEEAPPLKBN       ")    '発送料適用区分
        sqlOrderStat.AppendLine("  , SHIPFEENEXTFLG       ")    '発送料次期フラグ
        sqlOrderStat.AppendLine("  , TARIFFAPPLKBN        ")    '使用料タリフ適用区分
        sqlOrderStat.AppendLine("  , OUTISLANDAPPLKBN     ")    '離島向け適用区分
        sqlOrderStat.AppendLine("  , FREEAPPLKBN          ")    '使用料無料特認 
        sqlOrderStat.AppendLine("  , SPECIALM1APPLKBN     ")    '特例Ｍ１適用区分
        sqlOrderStat.AppendLine("  , SPECIALM2APPLKBN     ")    '特例Ｍ２適用区分
        sqlOrderStat.AppendLine("  , SPECIALM3APPLKBN     ")    '特例Ｍ３適用区分
        sqlOrderStat.AppendLine("  , HOKKAIDOAPPLKBN      ")    '北海道先方負担
        sqlOrderStat.AppendLine("  , NIIGATAAPPLKBN       ")    '新潟先方負担
        sqlOrderStat.AppendLine("  , REFLECTFLG           ")    '反映フラグ
        sqlOrderStat.AppendLine("  , SKIPFLG              ")    '読み飛ばしフラグ
        sqlOrderStat.AppendLine("  , DELFLG               ")    '削除フラグ
        sqlOrderStat.AppendLine("  , INITYMD              ")    '登録年月日
        sqlOrderStat.AppendLine("  , INITUSER             ")    '登録ユーザーＩＤ
        sqlOrderStat.AppendLine("  , INITTERMID           ")    '登録端末
        sqlOrderStat.AppendLine("  , INITPGID             ")    '登録プログラムＩＤ
        sqlOrderStat.AppendLine(")")
        sqlOrderStat.AppendLine(" VALUES(")
        sqlOrderStat.AppendLine("    @ORDERNO              ")    'オーダーNo
        sqlOrderStat.AppendLine("  , @SAMEDAYCNT           ")    '同日内回数
        sqlOrderStat.AppendLine("  , @SHIPYMD              ")    '発送年月日
        sqlOrderStat.AppendLine("  , @LINENUM              ")    '行番
        sqlOrderStat.AppendLine("  , @JOTDEPBRANCHCD       ")    'ＪＯＴ発店所コード
        sqlOrderStat.AppendLine("  , @DEPSTATION           ")    '発駅コード
        sqlOrderStat.AppendLine("  , @DEPTRUSTEECD         ")    '発受託人コード
        sqlOrderStat.AppendLine("  , @DEPTRUSTEESUBCD      ")    '発受託人サブ
        sqlOrderStat.AppendLine("  , @JOTARRBRANCHCD       ")    'ＪＯＴ着店所コード
        sqlOrderStat.AppendLine("  , @ARRSTATION           ")    '着駅コード
        sqlOrderStat.AppendLine("  , @ARRTRUSTEECD         ")    '着受託人コード
        sqlOrderStat.AppendLine("  , @ARRTRUSTEESUBCD      ")    '着受託人サブ
        sqlOrderStat.AppendLine("  , @ARRPLANYMD           ")    '到着予定年月日
        sqlOrderStat.AppendLine("  , @STACKFREEKBN         ")    '積空区分
        sqlOrderStat.AppendLine("  , @STATUSKBN            ")    '状態区分
        sqlOrderStat.AppendLine("  , @CONTRACTCD           ")    '契約コード
        sqlOrderStat.AppendLine("  , @DEPTRAINNO           ")    '発列車番号
        sqlOrderStat.AppendLine("  , @ARRTRAINNO           ")    '着列車番号
        sqlOrderStat.AppendLine("  , @JRITEMCD             ")    'ＪＲ品目コード
        sqlOrderStat.AppendLine("  , @LEASEPRODUCTCD       ")    'リース品名コード
        sqlOrderStat.AppendLine("  , @DEPSHIPPERCD         ")    '発荷主コード
        sqlOrderStat.AppendLine("  , @QUANTITY             ")    '個数
        sqlOrderStat.AppendLine("  , @ADDSUBYM             ")    '加減額の対象年月
        sqlOrderStat.AppendLine("  , @ADDSUBQUANTITY       ")    '加減額の個数
        sqlOrderStat.AppendLine("  , @JRFIXEDFARE          ")    'ＪＲ所定運賃
        sqlOrderStat.AppendLine("  , @USEFEE               ")    '使用料金額
        sqlOrderStat.AppendLine("  , @OWNDISCOUNTFEE       ")    '私有割引相当額
        sqlOrderStat.AppendLine("  , @RETURNFARE           ")    '割戻し運賃
        sqlOrderStat.AppendLine("  , @NITTSUFREESENDFEE    ")    '通運負担回送運賃
        sqlOrderStat.AppendLine("  , @MANAGEFEE            ")    '運行管理料
        sqlOrderStat.AppendLine("  , @SHIPBURDENFEE        ")    '荷主負担運賃
        sqlOrderStat.AppendLine("  , @SHIPFEE              ")    '発送料
        sqlOrderStat.AppendLine("  , @ARRIVEFEE            ")    '到着料
        sqlOrderStat.AppendLine("  , @PICKUPFEE            ")    '集荷料
        sqlOrderStat.AppendLine("  , @DELIVERYFEE          ")    '配達料
        sqlOrderStat.AppendLine("  , @OTHER1FEE            ")    'その他１
        sqlOrderStat.AppendLine("  , @OTHER2FEE            ")    'その他２
        sqlOrderStat.AppendLine("  , @FREESENDFEE          ")    '回送運賃
        sqlOrderStat.AppendLine("  , @SPRFITKBN            ")    '冷蔵適合マーク
        sqlOrderStat.AppendLine("  , @JURISDICTIONCD       ")    '所管部コード
        sqlOrderStat.AppendLine("  , @ACCOUNTINGASSETSCD   ")    '経理資産コード
        sqlOrderStat.AppendLine("  , @ACCOUNTINGASSETSKBN  ")    '経理資産区分
        sqlOrderStat.AppendLine("  , @DUMMYKBN             ")    'ダミー区分
        sqlOrderStat.AppendLine("  , @SPOTKBN              ")    'スポット区分
        sqlOrderStat.AppendLine("  , @COMPKANKBN           ")    '複合一貫区分
        sqlOrderStat.AppendLine("  , @KEIJOYM              ")    '計上年月
        sqlOrderStat.AppendLine("  , @PARTNERCAMPCD        ")    '相手先会社コード
        sqlOrderStat.AppendLine("  , @PARTNERDEPTCD        ")    '相手先部門コード
        sqlOrderStat.AppendLine("  , @INVKEIJYOBRANCHCD    ")    '請求項目 計上店コード
        sqlOrderStat.AppendLine("  , @INVFILINGDEPT        ")    '請求項目 請求書提出部店
        sqlOrderStat.AppendLine("  , @INVKESAIKBN          ")    '請求項目 請求書決済区分
        sqlOrderStat.AppendLine("  , @INVSUBCD             ")    '請求項目 請求書細分コード
        sqlOrderStat.AppendLine("  , @PAYKEIJYOBRANCHCD    ")    '支払項目 費用計上店コード
        sqlOrderStat.AppendLine("  , @PAYFILINGBRANCH      ")    '支払項目 支払書提出支店
        sqlOrderStat.AppendLine("  , @TAXCALCUNIT          ")    '支払項目 消費税計算単位
        sqlOrderStat.AppendLine("  , @TAXKBN               ")    '税区分
        sqlOrderStat.AppendLine("  , @TAXRATE              ")    '税率
        sqlOrderStat.AppendLine("  , @BEFDEPTRUSTEECD      ")    '変換前項目-発受託人コード
        sqlOrderStat.AppendLine("  , @BEFDEPTRUSTEESUBCD   ")    '変換前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , @BEFDEPSHIPPERCD      ")    '変換前項目-発荷主コード
        sqlOrderStat.AppendLine("  , @BEFARRTRUSTEECD      ")    '変換前項目-着受託人コード
        sqlOrderStat.AppendLine("  , @BEFARRTRUSTEESUBCD   ")    '変換前項目-着受託人サブ
        sqlOrderStat.AppendLine("  , @BEFJRITEMCD          ")    '変換前項目-ＪＲ品目コード
        sqlOrderStat.AppendLine("  , @BEFSTACKFREEKBN      ")    '変換前項目-積空区分
        sqlOrderStat.AppendLine("  , @SPLBEFDEPSTATION     ")    '分割前項目-発駅コード
        sqlOrderStat.AppendLine("  , @SPLBEFDEPTRUSTEECD   ")    '分割前項目-発受託人コード
        sqlOrderStat.AppendLine("  , @SPLBEFDEPTRUSTEESUBCD")    '分割前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , @SPLBEFUSEFEE         ")    '分割前項目-使用料金額
        sqlOrderStat.AppendLine("  , @SPLBEFSHIPFEE        ")    '分割前項目-発送料
        sqlOrderStat.AppendLine("  , @SPLBEFARRIVEFEE      ")    '分割前項目-到着料
        sqlOrderStat.AppendLine("  , @SPLBEFFREESENDFEE    ")    '分割前項目-回送運賃
        sqlOrderStat.AppendLine("  , @PROCFLG1             ")    '処理フラグ-料金計算済
        sqlOrderStat.AppendLine("  , @PROCFLG2             ")    '処理フラグ-精算ファイル作成済
        sqlOrderStat.AppendLine("  , @PROCFLG3             ")    '処理フラグ-運用ファイル作成済
        sqlOrderStat.AppendLine("  , @PROCFLG4             ")    '処理フラグ-複合一貫作成済
        sqlOrderStat.AppendLine("  , @PROCFLG5             ")    '処理フラグ-請求支払分割済
        sqlOrderStat.AppendLine("  , @PROCFLG6             ")    '処理フラグ-コード変換済
        sqlOrderStat.AppendLine("  , @PROCFLG7             ")    '処理フラグ-ダミーフラグ７
        sqlOrderStat.AppendLine("  , @PROCFLG8             ")    '処理フラグ-ダミーフラグ８
        sqlOrderStat.AppendLine("  , @PROCFLG9             ")    '処理フラグ-ダミーフラグ９
        sqlOrderStat.AppendLine("  , @PROCFLG10            ")    '処理フラグ-ダミーフラグ１０
        sqlOrderStat.AppendLine("  , @PICKUPTEL            ")    '集荷先電話番号
        sqlOrderStat.AppendLine("  , @FARECALCTUNAPPLKBN   ")    '運賃計算屯数適用区分
        sqlOrderStat.AppendLine("  , @FARECALCTUNNEXTFLG   ")    '運賃計算屯数次期フラグ
        sqlOrderStat.AppendLine("  , @FARECALCTUN          ")    '運賃計算屯数
        sqlOrderStat.AppendLine("  , @DISNO                ")    '割引番号
        sqlOrderStat.AppendLine("  , @EXTNO                ")    '割増番号
        sqlOrderStat.AppendLine("  , @KIROAPPLKBN          ")    'キロ程適用区分
        sqlOrderStat.AppendLine("  , @KIRO                 ")    'キロ程
        sqlOrderStat.AppendLine("  , @RENTRATEAPPLKBN      ")    '賃率適用区分
        sqlOrderStat.AppendLine("  , @RENTRATENEXTFLG      ")    '賃率次期フラグ
        sqlOrderStat.AppendLine("  , @RENTRATE             ")    '賃率
        sqlOrderStat.AppendLine("  , @APPLYRATEAPPLKBN     ")    '適用率適用区分
        sqlOrderStat.AppendLine("  , @APPLYRATENEXTFLG     ")    '適用率次期フラグ
        sqlOrderStat.AppendLine("  , @APPLYRATE            ")    '適用率
        sqlOrderStat.AppendLine("  , @USEFEERATEAPPLKBN    ")    '使用料率適用区分
        sqlOrderStat.AppendLine("  , @USEFEERATE           ")    '使用料率
        sqlOrderStat.AppendLine("  , @FREESENDRATEAPPLKBN  ")    '回送運賃適用率適用区分
        sqlOrderStat.AppendLine("  , @FREESENDRATENEXTFLG  ")    '回送運賃適用率次期フラグ
        sqlOrderStat.AppendLine("  , @FREESENDRATE         ")    '回送運賃適用率
        sqlOrderStat.AppendLine("  , @SHIPFEEAPPLKBN       ")    '発送料適用区分
        sqlOrderStat.AppendLine("  , @SHIPFEENEXTFLG       ")    '発送料次期フラグ
        sqlOrderStat.AppendLine("  , @TARIFFAPPLKBN        ")    '使用料タリフ適用区分
        sqlOrderStat.AppendLine("  , @OUTISLANDAPPLKBN     ")    '離島向け適用区分
        sqlOrderStat.AppendLine("  , @FREEAPPLKBN          ")    '使用料無料特認 
        sqlOrderStat.AppendLine("  , @SPECIALM1APPLKBN     ")    '特例Ｍ１適用区分
        sqlOrderStat.AppendLine("  , @SPECIALM2APPLKBN     ")    '特例Ｍ２適用区分
        sqlOrderStat.AppendLine("  , @SPECIALM3APPLKBN     ")    '特例Ｍ３適用区分
        sqlOrderStat.AppendLine("  , @HOKKAIDOAPPLKBN      ")    '北海道先方負担
        sqlOrderStat.AppendLine("  , @NIIGATAAPPLKBN       ")    '新潟先方負担
        sqlOrderStat.AppendLine("  , @REFLECTFLG           ")    '反映フラグ
        sqlOrderStat.AppendLine("  , @SKIPFLG              ")    '読み飛ばしフラグ
        sqlOrderStat.AppendLine("  , @DELFLG               ")    '削除フラグ
        sqlOrderStat.AppendLine("  , @INITYMD              ")    '登録年月日
        sqlOrderStat.AppendLine("  , @INITUSER             ")    '登録ユーザーＩＤ
        sqlOrderStat.AppendLine("  , @INITTERMID           ")    '登録端末
        sqlOrderStat.AppendLine("  , @INITPGID             ")    '登録プログラムＩＤ
        sqlOrderStat.AppendLine(")")

        Using sqlOrderCmd As New MySqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_ORDERNO)                                   'オーダーNo
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SAMEDAYCNT))                    '同日内回数
                .Add("SHIPYMD", MySqlDbType.Date).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPYMD))                         '発送年月日
                .Add("LINENUM", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_LINENUM))                          '行番
                .Add("JOTDEPBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JOTDEPBRANCHCD))            'ＪＯＴ発店所コード
                .Add("DEPSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPSTATION))                    '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRUSTEECD))                '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRUSTEESUBCD))          '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JOTARRBRANCHCD))            'ＪＯＴ着店所コード
                .Add("ARRSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRSTATION))                    '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRUSTEECD))                '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRUSTEESUBCD))          '着受託人サブ
                .Add("ARRPLANYMD", MySqlDbType.Date).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRPLANYMD))                   '到着予定年月日
                .Add("STACKFREEKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_STACKFREEKBN))                '積空区分
                .Add("STATUSKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_STATUSKBN))                      '状態区分
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_CONTRACTCD))               '契約コード
                .Add("DEPTRAINNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRAINNO))                    '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRAINNO))                    '着列車番号
                .Add("JRITEMCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JRITEMCD))                        'ＪＲ品目コード
                .Add("LEASEPRODUCTCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_LEASEPRODUCTCD))            'リース品名コード
                .Add("DEPSHIPPERCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPSHIPPERCD))                '発荷主コード
                .Add("QUANTITY", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_QUANTITY))                        '個数
                .Add("ADDSUBYM", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ADDSUBYM))                        '加減額の対象年月
                .Add("ADDSUBQUANTITY", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ADDSUBQUANTITY))            '加減額の個数
                .Add("JRFIXEDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JRFIXEDFARE))                  'ＪＲ所定運賃
                .Add("USEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEE))                            '使用料金額
                .Add("OWNDISCOUNTFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OWNDISCOUNTFEE))            '私有割引相当額
                .Add("RETURNFARE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RETURNFARE))                    '割戻し運賃
                .Add("NITTSUFREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_NITTSUFREESENDFEE))      '通運負担回送運賃
                .Add("MANAGEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_MANAGEFEE))                      '運行管理料
                .Add("SHIPBURDENFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPBURDENFEE))              '荷主負担運賃
                .Add("SHIPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEE))                          '発送料
                .Add("ARRIVEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRIVEFEE))                      '到着料
                .Add("PICKUPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PICKUPFEE))                      '集荷料
                .Add("DELIVERYFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DELIVERYFEE))                  '配達料
                .Add("OTHER1FEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OTHER1FEE))                      'その他１
                .Add("OTHER2FEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OTHER2FEE))                      'その他２
                .Add("FREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDFEE))                  '回送運賃
                .Add("SPRFITKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPRFITKBN))                      '冷蔵適合マーク
                .Add("JURISDICTIONCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JURISDICTIONCD))            '所管部コード
                .Add("ACCOUNTINGASSETSCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ACCOUNTINGASSETSCD))    '経理資産コード
                .Add("ACCOUNTINGASSETSKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ACCOUNTINGASSETSKBN))  '経理資産区分
                .Add("DUMMYKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DUMMYKBN))                        'ダミー区分
                .Add("SPOTKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPOTKBN))                          'スポット区分
                .Add("COMPKANKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_COMPKANKBN))                    '複合一貫区分
                .Add("KEIJOYM", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KEIJOYM))                          '計上年月
                .Add("PARTNERCAMPCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PARTNERCAMPCD))         '相手先会社コード
                .Add("PARTNERDEPTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PARTNERDEPTCD))         '相手先部門コード
                .Add("INVKEIJYOBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVKEIJYOBRANCHCD))      '請求項目 計上店コード
                .Add("INVFILINGDEPT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVFILINGDEPT))              '請求項目 請求書提出部店
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVKESAIKBN))                  '請求項目 請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVSUBCD))                        '請求項目 請求書細分コード
                .Add("PAYKEIJYOBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PAYKEIJYOBRANCHCD))      '支払項目 費用計上店コード
                .Add("PAYFILINGBRANCH", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PAYFILINGBRANCH))          '支払項目 支払書提出支店
                .Add("TAXCALCUNIT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXCALCUNIT))                  '支払項目 消費税計算単位
                .Add("TAXKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXKBN))                            '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXRATE))                          '税率
                .Add("BEFDEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPTRUSTEECD))          '変換前項目-発受託人コード
                .Add("BEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPTRUSTEESUBCD))    '変換前項目-発受託人サブ
                .Add("BEFDEPSHIPPERCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPSHIPPERCD))          '変換前項目-発荷主コード
                .Add("BEFARRTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFARRTRUSTEECD))          '変換前項目-着受託人コード
                .Add("BEFARRTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFARRTRUSTEESUBCD))    '変換前項目-着受託人サブ
                .Add("BEFJRITEMCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFJRITEMCD))                  '変換前項目-ＪＲ品目コード
                .Add("BEFSTACKFREEKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFSTACKFREEKBN))          '変換前項目-積空区分
                .Add("SPLBEFDEPSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPSTATION))        '分割前項目-発駅コード
                .Add("SPLBEFDEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEECD))    '分割前項目-発受託人コード
                .Add("SPLBEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEESUBCD))  '分割前項目-発受託人サブ
                .Add("SPLBEFUSEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFUSEFEE))                    '分割前項目-使用料金額
                .Add("SPLBEFSHIPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFSHIPFEE))                  '分割前項目-発送料
                .Add("SPLBEFARRIVEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFARRIVEFEE))              '分割前項目-到着料
                .Add("SPLBEFFREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFFREESENDFEE))          '分割前項目-回送運賃
                .Add("PROCFLG1", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG1))    '処理フラグ-料金計算済
                .Add("PROCFLG2", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG2))    '処理フラグ-精算ファイル作成済
                .Add("PROCFLG3", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG3))    '処理フラグ-運用ファイル作成済
                .Add("PROCFLG4", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG4))    '処理フラグ-複合一貫作成済
                .Add("PROCFLG5", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG5))    '処理フラグ-請求支払分割済
                .Add("PROCFLG6", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG6))    '処理フラグ-コード変換済
                .Add("PROCFLG7", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG7))    '処理フラグ-ダミーフラグ７
                .Add("PROCFLG8", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG8))    '処理フラグ-ダミーフラグ８
                .Add("PROCFLG9", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG9))    '処理フラグ-ダミーフラグ９
                .Add("PROCFLG10", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG10))  '処理フラグ-ダミーフラグ１０
                .Add("PICKUPTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PICKUPTEL))                 '集荷先電話番号
                .Add("FARECALCTUNAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUNAPPLKBN))    '運賃計算屯数適用区分
                .Add("FARECALCTUNNEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUNNEXTFLG))    '運賃計算屯数次期フラグ
                .Add("FARECALCTUN", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUN))              '運賃計算屯数
                .Add("DISNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DISNO))                              '割引番号
                .Add("EXTNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_EXTNO))                              '割増番号
                .Add("KIROAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KIROAPPLKBN))                  'キロ程適用区分
                .Add("KIRO", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KIRO))                            'キロ程
                .Add("RENTRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATEAPPLKBN))          '賃率適用区分
                .Add("RENTRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATENEXTFLG))          '賃率次期フラグ
                .Add("RENTRATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATE))                    '賃率
                .Add("APPLYRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATEAPPLKBN))        '適用率適用区分
                .Add("APPLYRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATENEXTFLG))        '適用率次期フラグ
                .Add("APPLYRATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATE))                  '適用率
                .Add("USEFEERATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEERATEAPPLKBN))      '使用料率適用区分
                .Add("USEFEERATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEERATE))                '使用料率
                .Add("FREESENDRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATEAPPLKBN))  '回送運賃適用率適用区分
                .Add("FREESENDRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATENEXTFLG))  '回送運賃適用率次期フラグ
                .Add("FREESENDRATE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATE))                '回送運賃適用率
                .Add("SHIPFEEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEEAPPLKBN))            '発送料適用区分
                .Add("SHIPFEENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEENEXTFLG))            '発送料次期フラグ
                .Add("TARIFFAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TARIFFAPPLKBN))              '使用料タリフ適用区分
                .Add("OUTISLANDAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OUTISLANDAPPLKBN))        '離島向け適用区分
                .Add("FREEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREEAPPLKBN))                  '使用料無料特認 
                .Add("SPECIALM1APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM1APPLKBN))        '特例Ｍ１適用区分
                .Add("SPECIALM2APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM2APPLKBN))        '特例Ｍ２適用区分
                .Add("SPECIALM3APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM3APPLKBN))        '特例Ｍ３適用区分
                .Add("HOKKAIDOAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_HOKKAIDOAPPLKBN))          '北海道先方負担
                .Add("NIIGATAAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_NIIGATAAPPLKBN))            '新潟先方負担
                .Add("REFLECTFLG", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_REFLECTFLG)                              '反映フラグ
                .Add("SKIPFLG", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_SKIPFLG)                                    '読み飛ばしフラグ
                .Add("DELFLG", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_DELFLG)                                      '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INITYMD))                     '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INITUSER))                   '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INITTERMID))               '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INITPGID))                   '登録プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 精算予定ファイルTBL更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htPlanfData">精算予定ファイルデータ</param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub UpdatePayPlanf(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, htPlanfData As Hashtable)

        '◯精算予定ファイルTBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("UPDATE LNG.LNT0006_PAYPLANF ")
        sqlOrderStat.AppendLine("SET")
        sqlOrderStat.AppendLine("    SHIPYMD = @SHIPYMD")                  '発送年月日
        sqlOrderStat.AppendLine("  , JOTDEPBRANCHCD = @JOTDEPBRANCHCD")    'ＪＯＴ発店所コード
        sqlOrderStat.AppendLine("  , DEPSTATION = @DEPSTATION")            '発駅コード
        sqlOrderStat.AppendLine("  , DEPTRUSTEECD = @DEPTRUSTEECD")        '発受託人コード
        sqlOrderStat.AppendLine("  , DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")  '発受託人サブ
        sqlOrderStat.AppendLine("  , JOTARRBRANCHCD = @JOTARRBRANCHCD")    'ＪＯＴ着店所コード
        sqlOrderStat.AppendLine("  , ARRSTATION = @ARRSTATION")            '着駅コード
        sqlOrderStat.AppendLine("  , ARRTRUSTEECD = @ARRTRUSTEECD")        '着受託人コード
        sqlOrderStat.AppendLine("  , ARRTRUSTEESUBCD = @ARRTRUSTEESUBCD")  '着受託人サブ
        sqlOrderStat.AppendLine("  , ARRPLANYMD = @ARRPLANYMD")            '到着予定年月日
        sqlOrderStat.AppendLine("  , STACKFREEKBN = @STACKFREEKBN")        '積空区分
        sqlOrderStat.AppendLine("  , STATUSKBN = @STATUSKBN")    '状態区分
        sqlOrderStat.AppendLine("  , CONTRACTCD = @CONTRACTCD")  '契約コード
        sqlOrderStat.AppendLine("  , DEPTRAINNO = @DEPTRAINNO")  '発列車番号
        sqlOrderStat.AppendLine("  , ARRTRAINNO = @ARRTRAINNO")  '着列車番号
        sqlOrderStat.AppendLine("  , JRITEMCD = @JRITEMCD")              'ＪＲ品目コード
        sqlOrderStat.AppendLine("  , LEASEPRODUCTCD = @LEASEPRODUCTCD")  'リース品名コード
        sqlOrderStat.AppendLine("  , DEPSHIPPERCD = @DEPSHIPPERCD")      '発荷主コード
        sqlOrderStat.AppendLine("  , QUANTITY = @QUANTITY")              '個数
        sqlOrderStat.AppendLine("  , ADDSUBYM = @ADDSUBYM")              '加減額の対象年月
        sqlOrderStat.AppendLine("  , ADDSUBQUANTITY = @ADDSUBQUANTITY")  '加減額の個数
        sqlOrderStat.AppendLine("  , JRFIXEDFARE = @JRFIXEDFARE")        'ＪＲ所定運賃
        sqlOrderStat.AppendLine("  , USEFEE = @USEFEE")                  '使用料金額
        sqlOrderStat.AppendLine("  , OWNDISCOUNTFEE = @OWNDISCOUNTFEE")  '私有割引相当額
        sqlOrderStat.AppendLine("  , RETURNFARE = @RETURNFARE")          '割戻し運賃
        sqlOrderStat.AppendLine("  , NITTSUFREESENDFEE = @NITTSUFREESENDFEE")  '通運負担回送運賃
        sqlOrderStat.AppendLine("  , MANAGEFEE = @MANAGEFEE")                  '運行管理料
        sqlOrderStat.AppendLine("  , SHIPBURDENFEE = @SHIPBURDENFEE")    '荷主負担運賃
        sqlOrderStat.AppendLine("  , SHIPFEE = @SHIPFEE")                '発送料
        sqlOrderStat.AppendLine("  , ARRIVEFEE = @ARRIVEFEE")            '到着料
        sqlOrderStat.AppendLine("  , PICKUPFEE = @PICKUPFEE")            '集荷料
        sqlOrderStat.AppendLine("  , DELIVERYFEE = @DELIVERYFEE")        '配達料
        sqlOrderStat.AppendLine("  , OTHER1FEE = @OTHER1FEE")            'その他１
        sqlOrderStat.AppendLine("  , OTHER2FEE = @OTHER2FEE")            'その他２
        sqlOrderStat.AppendLine("  , FREESENDFEE = @FREESENDFEE")  '回送運賃
        sqlOrderStat.AppendLine("  , SPRFITKBN = @SPRFITKBN")      '冷蔵適合マーク
        sqlOrderStat.AppendLine("  , JURISDICTIONCD = @JURISDICTIONCD")            '所管部コード
        sqlOrderStat.AppendLine("  , ACCOUNTINGASSETSCD = @ACCOUNTINGASSETSCD")    '経理資産コード
        sqlOrderStat.AppendLine("  , ACCOUNTINGASSETSKBN = @ACCOUNTINGASSETSKBN")  '経理資産区分
        sqlOrderStat.AppendLine("  , DUMMYKBN = @DUMMYKBN")      'ダミー区分
        sqlOrderStat.AppendLine("  , SPOTKBN = @SPOTKBN")        'スポット区分
        sqlOrderStat.AppendLine("  , COMPKANKBN = @COMPKANKBN")  '複合一貫区分
        sqlOrderStat.AppendLine("  , KEIJOYM = @KEIJOYM")        '計上年月
        sqlOrderStat.AppendLine("  , PARTNERCAMPCD = @PARTNERCAMPCD")          '相手先会社コード
        sqlOrderStat.AppendLine("  , PARTNERDEPTCD = @PARTNERDEPTCD")          '相手先部門コード
        sqlOrderStat.AppendLine("  , INVKEIJYOBRANCHCD = @INVKEIJYOBRANCHCD")  '請求項目 計上店コード
        sqlOrderStat.AppendLine("  , INVFILINGDEPT = @INVFILINGDEPT")          '請求項目 請求書提出部店
        sqlOrderStat.AppendLine("  , INVKESAIKBN = @INVKESAIKBN")              '請求項目 請求書決済区分
        sqlOrderStat.AppendLine("  , INVSUBCD = @INVSUBCD")                    '請求項目 請求書細分コード
        sqlOrderStat.AppendLine("  , PAYKEIJYOBRANCHCD = @PAYKEIJYOBRANCHCD")  '支払項目 費用計上店コード
        sqlOrderStat.AppendLine("  , PAYFILINGBRANCH = @PAYFILINGBRANCH")      '支払項目 支払書提出支店
        sqlOrderStat.AppendLine("  , TAXCALCUNIT = @TAXCALCUNIT")              '支払項目 消費税計算単位
        sqlOrderStat.AppendLine("  , TAXKBN = @TAXKBN")    '税区分
        sqlOrderStat.AppendLine("  , TAXRATE = @TAXRATE")  '税率
        sqlOrderStat.AppendLine("  , BEFDEPTRUSTEECD = @BEFDEPTRUSTEECD")        '変換前項目-発受託人コード
        sqlOrderStat.AppendLine("  , BEFDEPTRUSTEESUBCD = @BEFDEPTRUSTEESUBCD")  '変換前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , BEFDEPSHIPPERCD = @BEFDEPSHIPPERCD")        '変換前項目-発荷主コード
        sqlOrderStat.AppendLine("  , BEFARRTRUSTEECD = @BEFARRTRUSTEECD")        '変換前項目-着受託人コード
        sqlOrderStat.AppendLine("  , BEFARRTRUSTEESUBCD = @BEFARRTRUSTEESUBCD")  '変換前項目-着受託人サブ
        sqlOrderStat.AppendLine("  , BEFJRITEMCD = @BEFJRITEMCD")                '変換前項目-ＪＲ品目コード
        sqlOrderStat.AppendLine("  , BEFSTACKFREEKBN = @BEFSTACKFREEKBN")        '変換前項目-積空区分
        sqlOrderStat.AppendLine("  , SPLBEFDEPSTATION = @SPLBEFDEPSTATION")            '分割前項目-発駅コード
        sqlOrderStat.AppendLine("  , SPLBEFDEPTRUSTEECD = @SPLBEFDEPTRUSTEECD")        '分割前項目-発受託人コード
        sqlOrderStat.AppendLine("  , SPLBEFDEPTRUSTEESUBCD = @SPLBEFDEPTRUSTEESUBCD")  '分割前項目-発受託人サブ
        sqlOrderStat.AppendLine("  , SPLBEFUSEFEE = @SPLBEFUSEFEE")                    '分割前項目-使用料金額
        sqlOrderStat.AppendLine("  , SPLBEFSHIPFEE = @SPLBEFSHIPFEE")                  '分割前項目-発送料
        sqlOrderStat.AppendLine("  , SPLBEFARRIVEFEE = @SPLBEFARRIVEFEE")              '分割前項目-到着料
        sqlOrderStat.AppendLine("  , SPLBEFFREESENDFEE = @SPLBEFFREESENDFEE")          '分割前項目-回送運賃
        sqlOrderStat.AppendLine("  , PROCFLG1 = @PROCFLG1")    '処理フラグ-料金計算済
        sqlOrderStat.AppendLine("  , PROCFLG2 = @PROCFLG2")    '処理フラグ-精算ファイル作成済
        sqlOrderStat.AppendLine("  , PROCFLG3 = @PROCFLG3")    '処理フラグ-運用ファイル作成済
        sqlOrderStat.AppendLine("  , PROCFLG4 = @PROCFLG4")    '処理フラグ-複合一貫作成済
        sqlOrderStat.AppendLine("  , PROCFLG5 = @PROCFLG5")    '処理フラグ-請求支払分割済
        sqlOrderStat.AppendLine("  , PROCFLG6 = @PROCFLG6")    '処理フラグ-コード変換済
        sqlOrderStat.AppendLine("  , PROCFLG7 = @PROCFLG7")    '処理フラグ-ダミーフラグ７
        sqlOrderStat.AppendLine("  , PROCFLG8 = @PROCFLG8")    '処理フラグ-ダミーフラグ８
        sqlOrderStat.AppendLine("  , PROCFLG9 = @PROCFLG9")    '処理フラグ-ダミーフラグ９
        sqlOrderStat.AppendLine("  , PROCFLG10 = @PROCFLG10")  '処理フラグ-ダミーフラグ１０
        sqlOrderStat.AppendLine("  , PICKUPTEL = @PICKUPTEL")    '集荷先電話番号
        sqlOrderStat.AppendLine("  , FARECALCTUNAPPLKBN = @FARECALCTUNAPPLKBN")    '運賃計算屯数適用区分
        sqlOrderStat.AppendLine("  , FARECALCTUNNEXTFLG = @FARECALCTUNNEXTFLG")    '運賃計算屯数次期フラグ
        sqlOrderStat.AppendLine("  , FARECALCTUN = @FARECALCTUN")    '運賃計算屯数
        sqlOrderStat.AppendLine("  , DISNO = @DISNO")    '割引番号
        sqlOrderStat.AppendLine("  , EXTNO = @EXTNO")    '割増番号
        sqlOrderStat.AppendLine("  , KIROAPPLKBN = @KIROAPPLKBN")    'キロ程適用区分
        sqlOrderStat.AppendLine("  , KIRO = @KIRO")                  'キロ程
        sqlOrderStat.AppendLine("  , RENTRATEAPPLKBN = @RENTRATEAPPLKBN")    '賃率適用区分
        sqlOrderStat.AppendLine("  , RENTRATENEXTFLG = @RENTRATENEXTFLG")    '賃率次期フラグ
        sqlOrderStat.AppendLine("  , RENTRATE = @RENTRATE")                  '賃率
        sqlOrderStat.AppendLine("  , APPLYRATEAPPLKBN = @APPLYRATEAPPLKBN")  '適用率適用区分
        sqlOrderStat.AppendLine("  , APPLYRATENEXTFLG = @APPLYRATENEXTFLG")  '適用率次期フラグ
        sqlOrderStat.AppendLine("  , APPLYRATE = @APPLYRATE")                '適用率
        sqlOrderStat.AppendLine("  , USEFEERATEAPPLKBN = @USEFEERATEAPPLKBN")    '使用料率適用区分
        sqlOrderStat.AppendLine("  , USEFEERATE = @USEFEERATE")                  '使用料率
        sqlOrderStat.AppendLine("  , FREESENDRATEAPPLKBN = @FREESENDRATEAPPLKBN")    '回送運賃適用率適用区分
        sqlOrderStat.AppendLine("  , FREESENDRATENEXTFLG = @FREESENDRATENEXTFLG")    '回送運賃適用率次期フラグ
        sqlOrderStat.AppendLine("  , FREESENDRATE = @FREESENDRATE")                  '回送運賃適用率
        sqlOrderStat.AppendLine("  , SHIPFEEAPPLKBN = @SHIPFEEAPPLKBN")        '発送料適用区分
        sqlOrderStat.AppendLine("  , SHIPFEENEXTFLG = @SHIPFEENEXTFLG")        '発送料次期フラグ
        sqlOrderStat.AppendLine("  , TARIFFAPPLKBN = @TARIFFAPPLKBN")          '使用料タリフ適用区分
        sqlOrderStat.AppendLine("  , OUTISLANDAPPLKBN = @OUTISLANDAPPLKBN")    '離島向け適用区分
        sqlOrderStat.AppendLine("  , FREEAPPLKBN = @FREEAPPLKBN")              '使用料無料特認 
        sqlOrderStat.AppendLine("  , SPECIALM1APPLKBN = @SPECIALM1APPLKBN")    '特例Ｍ１適用区分
        sqlOrderStat.AppendLine("  , SPECIALM2APPLKBN = @SPECIALM2APPLKBN")    '特例Ｍ２適用区分
        sqlOrderStat.AppendLine("  , SPECIALM3APPLKBN = @SPECIALM3APPLKBN")    '特例Ｍ３適用区分
        sqlOrderStat.AppendLine("  , HOKKAIDOAPPLKBN = @HOKKAIDOAPPLKBN")      '北海道先方負担
        sqlOrderStat.AppendLine("  , NIIGATAAPPLKBN = @NIIGATAAPPLKBN")        '新潟先方負担
        sqlOrderStat.AppendLine("  , DELFLG = @DELFLG")                        '削除フラグ
        sqlOrderStat.AppendLine("  , UPDYMD = @UPDYMD")                        '更新年月日
        sqlOrderStat.AppendLine("  , UPDUSER = @UPDUSER")                      '更新ユーザーＩＤ
        sqlOrderStat.AppendLine("  , UPDTERMID = @UPDTERMID")                  '更新端末
        sqlOrderStat.AppendLine("  , UPDPGID = @UPDPGID")                      '更新プログラムＩＤ
        sqlOrderStat.AppendLine("WHERE")
        sqlOrderStat.AppendLine("    ORDERNO = @ORDERNO")          '受注No
        sqlOrderStat.AppendLine("AND SAMEDAYCNT = @SAMEDAYCNT")    '同日内回数

        Using sqlOrderCmd As New MySqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("ORDERNO", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_ORDERNO)                                    'オーダーNo
                .Add("SAMEDAYCNT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SAMEDAYCNT))                    '同日内回数
                .Add("SHIPYMD", MySqlDbType.Date).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPYMD))                         '発送年月日
                .Add("JOTDEPBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JOTDEPBRANCHCD))            'ＪＯＴ発店所コード
                .Add("DEPSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPSTATION))                    '発駅コード
                .Add("DEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRUSTEECD))                '発受託人コード
                .Add("DEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRUSTEESUBCD))          '発受託人サブ
                .Add("JOTARRBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JOTARRBRANCHCD))            'ＪＯＴ着店所コード
                .Add("ARRSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRSTATION))                    '着駅コード
                .Add("ARRTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRUSTEECD))                '着受託人コード
                .Add("ARRTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRUSTEESUBCD))          '着受託人サブ
                .Add("ARRPLANYMD", MySqlDbType.Date).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRPLANYMD))                   '到着予定年月日
                .Add("STACKFREEKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_STACKFREEKBN))                '積空区分
                .Add("STATUSKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_STATUSKBN))                      '状態区分
                .Add("CONTRACTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_CONTRACTCD))               '契約コード
                .Add("DEPTRAINNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPTRAINNO))                    '発列車番号
                .Add("ARRTRAINNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRTRAINNO))                    '着列車番号
                .Add("JRITEMCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JRITEMCD))                        'ＪＲ品目コード
                .Add("LEASEPRODUCTCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_LEASEPRODUCTCD))            'リース品名コード
                .Add("DEPSHIPPERCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DEPSHIPPERCD))                '発荷主コード
                .Add("QUANTITY", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_QUANTITY))                        '個数
                .Add("ADDSUBYM", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ADDSUBYM))                        '加減額の対象年月
                .Add("ADDSUBQUANTITY", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ADDSUBQUANTITY))            '加減額の個数
                .Add("JRFIXEDFARE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JRFIXEDFARE))                  'ＪＲ所定運賃
                .Add("USEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEE))                            '使用料金額
                .Add("OWNDISCOUNTFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OWNDISCOUNTFEE))            '私有割引相当額
                .Add("RETURNFARE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RETURNFARE))                    '割戻し運賃
                .Add("NITTSUFREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_NITTSUFREESENDFEE))      '通運負担回送運賃
                .Add("MANAGEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_MANAGEFEE))                      '運行管理料
                .Add("SHIPBURDENFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPBURDENFEE))              '荷主負担運賃
                .Add("SHIPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEE))                          '発送料
                .Add("ARRIVEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ARRIVEFEE))                      '到着料
                .Add("PICKUPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PICKUPFEE))                      '集荷料
                .Add("DELIVERYFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DELIVERYFEE))                  '配達料
                .Add("OTHER1FEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OTHER1FEE))                      'その他１
                .Add("OTHER2FEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OTHER2FEE))                      'その他２
                .Add("FREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDFEE))                  '回送運賃
                .Add("SPRFITKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPRFITKBN))                      '冷蔵適合マーク
                .Add("JURISDICTIONCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_JURISDICTIONCD))            '所管部コード
                .Add("ACCOUNTINGASSETSCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ACCOUNTINGASSETSCD))    '経理資産コード
                .Add("ACCOUNTINGASSETSKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_ACCOUNTINGASSETSKBN))  '経理資産区分
                .Add("DUMMYKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DUMMYKBN))                        'ダミー区分
                .Add("SPOTKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPOTKBN))                          'スポット区分
                .Add("COMPKANKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_COMPKANKBN))                    '複合一貫区分
                .Add("KEIJOYM", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KEIJOYM))                          '計上年月
                .Add("PARTNERCAMPCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PARTNERCAMPCD))         '相手先会社コード
                .Add("PARTNERDEPTCD", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PARTNERDEPTCD))         '相手先部門コード
                .Add("INVKEIJYOBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVKEIJYOBRANCHCD))      '請求項目 計上店コード
                .Add("INVFILINGDEPT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVFILINGDEPT))              '請求項目 請求書提出部店
                .Add("INVKESAIKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVKESAIKBN))                  '請求項目 請求書決済区分
                .Add("INVSUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_INVSUBCD))                        '請求項目 請求書細分コード
                .Add("PAYKEIJYOBRANCHCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PAYKEIJYOBRANCHCD))      '支払項目 費用計上店コード
                .Add("PAYFILINGBRANCH", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PAYFILINGBRANCH))          '支払項目 支払書提出支店
                .Add("TAXCALCUNIT", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXCALCUNIT))                  '支払項目 消費税計算単位
                .Add("TAXKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXKBN))                            '税区分
                .Add("TAXRATE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TAXRATE))                          '税率
                .Add("BEFDEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPTRUSTEECD))          '変換前項目-発受託人コード
                .Add("BEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPTRUSTEESUBCD))    '変換前項目-発受託人サブ
                .Add("BEFDEPSHIPPERCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFDEPSHIPPERCD))          '変換前項目-発荷主コード
                .Add("BEFARRTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFARRTRUSTEECD))          '変換前項目-着受託人コード
                .Add("BEFARRTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFARRTRUSTEESUBCD))    '変換前項目-着受託人サブ
                .Add("BEFJRITEMCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFJRITEMCD))                  '変換前項目-ＪＲ品目コード
                .Add("BEFSTACKFREEKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_BEFSTACKFREEKBN))          '変換前項目-積空区分
                .Add("SPLBEFDEPSTATION", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPSTATION))        '分割前項目-発駅コード
                .Add("SPLBEFDEPTRUSTEECD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEECD))    '分割前項目-発受託人コード
                .Add("SPLBEFDEPTRUSTEESUBCD", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEESUBCD))  '分割前項目-発受託人サブ
                .Add("SPLBEFUSEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFUSEFEE))                    '分割前項目-使用料金額
                .Add("SPLBEFSHIPFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFSHIPFEE))                  '分割前項目-発送料
                .Add("SPLBEFARRIVEFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFARRIVEFEE))              '分割前項目-到着料
                .Add("SPLBEFFREESENDFEE", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPLBEFFREESENDFEE))          '分割前項目-回送運賃
                .Add("PROCFLG1", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG1))    '処理フラグ-料金計算済
                .Add("PROCFLG2", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG2))    '処理フラグ-精算ファイル作成済
                .Add("PROCFLG3", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG3))    '処理フラグ-運用ファイル作成済
                .Add("PROCFLG4", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG4))    '処理フラグ-複合一貫作成済
                .Add("PROCFLG5", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG5))    '処理フラグ-請求支払分割済
                .Add("PROCFLG6", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG6))    '処理フラグ-コード変換済
                .Add("PROCFLG7", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG7))    '処理フラグ-ダミーフラグ７
                .Add("PROCFLG8", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG8))    '処理フラグ-ダミーフラグ８
                .Add("PROCFLG9", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG9))    '処理フラグ-ダミーフラグ９
                .Add("PROCFLG10", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PROCFLG10))  '処理フラグ-ダミーフラグ１０
                .Add("PICKUPTEL", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_PICKUPTEL))                 '集荷先電話番号
                .Add("FARECALCTUNAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUNAPPLKBN))    '運賃計算屯数適用区分
                .Add("FARECALCTUNNEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUNNEXTFLG))    '運賃計算屯数次期フラグ
                .Add("FARECALCTUN", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FARECALCTUN))              '運賃計算屯数
                .Add("DISNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_DISNO))                              '割引番号
                .Add("EXTNO", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_EXTNO))                              '割増番号
                .Add("KIROAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KIROAPPLKBN))                  'キロ程適用区分
                .Add("KIRO", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_KIRO))                            'キロ程
                .Add("RENTRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATEAPPLKBN))          '賃率適用区分
                .Add("RENTRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATENEXTFLG))          '賃率次期フラグ
                .Add("RENTRATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_RENTRATE))                    '賃率
                .Add("APPLYRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATEAPPLKBN))        '適用率適用区分
                .Add("APPLYRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATENEXTFLG))        '適用率次期フラグ
                .Add("APPLYRATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_APPLYRATE))                  '適用率
                .Add("USEFEERATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEERATEAPPLKBN))      '使用料率適用区分
                .Add("USEFEERATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_USEFEERATE))                '使用料率
                .Add("FREESENDRATEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATEAPPLKBN))  '回送運賃適用率適用区分
                .Add("FREESENDRATENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATENEXTFLG))  '回送運賃適用率次期フラグ
                .Add("FREESENDRATE", MySqlDbType.Decimal).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREESENDRATE))            '回送運賃適用率
                .Add("SHIPFEEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEEAPPLKBN))            '発送料適用区分
                .Add("SHIPFEENEXTFLG", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SHIPFEENEXTFLG))            '発送料次期フラグ
                .Add("TARIFFAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_TARIFFAPPLKBN))              '使用料タリフ適用区分
                .Add("OUTISLANDAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_OUTISLANDAPPLKBN))        '離島向け適用区分
                .Add("FREEAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_FREEAPPLKBN))                  '使用料無料特認 
                .Add("SPECIALM1APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM1APPLKBN))        '特例Ｍ１適用区分
                .Add("SPECIALM2APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM2APPLKBN))        '特例Ｍ２適用区分
                .Add("SPECIALM3APPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_SPECIALM3APPLKBN))        '特例Ｍ３適用区分
                .Add("HOKKAIDOAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_HOKKAIDOAPPLKBN))          '北海道先方負担
                .Add("NIIGATAAPPLKBN", MySqlDbType.Int32).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_NIIGATAAPPLKBN))            '新潟先方負担
                .Add("REFLECTFLG", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_REFLECTFLG)                              '反映フラグ
                .Add("DELFLG", MySqlDbType.VarChar).Value = htPlanfData(C_PAYFPARAM.PP_DELFLG)                                      '削除フラグ
                .Add("UPDYMD", MySqlDbType.DateTime).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_UPDYMD))                       '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_UPDUSER))                     '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_UPDTERMID))                 '更新端末
                .Add("UPDPGID", MySqlDbType.VarChar).Value = BlankToDBNull(htPlanfData(C_PAYFPARAM.PP_UPDPGID))                     '更新プログラムＩＤ
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

End Class
