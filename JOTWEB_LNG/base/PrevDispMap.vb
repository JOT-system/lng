Option Strict On
''' <summary>
''' 共通で利用する定数定義
''' </summary>
Public Module PrevDispMap

    ''' <summary>
    ''' 前画面の識別用名称の一覧
    ''' </summary>
    Public Class C_PREV_MAP_LIST
        ''' <summary>
        ''' ログイン画面
        ''' </summary>
        Public Const LOGIN As String = "ASP.M10000LOGON_ASPX"
        ''' <summary>
        ''' メニュー画面
        ''' </summary>
        Public Const MENU As String = "ASP.LNG_M00001MENU_ASPX"

        ''' <summary>
        ''' サブメニュー画面
        ''' </summary>
        Public Const SUBMENU As String = "ASP.LNG_M00001MENU_ASPX"
        'Public Const SUBMENU As String = "ASP.LNG_M00002MENU_ASPX"
        ''' <summary>
        ''' 請求書出力
        ''' </summary>
        Public Const LNT0001I As String = "ASP.LNG_ZISSEKI_LNT0001INVOICEOUTPUT_ASPX"
        ''' <summary>
        ''' 実績管理
        ''' </summary>
        Public Const LNT0001L As String = "ASP.LNG_ZISSEKI_LNT0001ZISSEKIMANAGE_ASPX"
        ''' <summary>
        ''' 実績取込
        ''' </summary>
        Public Const LNT0001D As String = "ASP.LNG_ZISSEKI_LNT0001ZISSEKIINTAKE_ASPX"
        ''' <summary>
        ''' 実績数量ゼロ
        ''' </summary>
        Public Const LNT0001Z As String = "ASP.LNG_ZISSEKI_LNT0001ZISSEKIZERO_ASPX"
        ''' <summary>
        ''' 調整画面
        ''' </summary>
        Public Const LNT0001AJ As String = "ASP.LNG_ZISSEKI_LNT0001ZISSEKIAJUSTMAP_ASPX"

        ''' <summary>
        ''' 輸送費明細出力状況
        ''' </summary>
        Public Const LNT0002L As String = "ASP.LNG_SEIKYU_LNT0002TRANSTATUSLIST_ASPX"
        ''' <summary>
        ''' 請求明細追加
        ''' </summary>
        Public Const LNT0002D As String = "ASP.LNG_SEIKYU_LNT0002SEIKYUDETAILADD_ASPX"

        ''' <summary>
        ''' サーチャージ料金
        ''' </summary>
        Public Const LNT0030L As String = "ASP.LNG_MAS_LNT0030SURCHARGEFEE_ASPX"

        ''' <summary>
        ''' 実績単価履歴
        ''' </summary>
        Public Const LNT0031L As String = "ASP.LNG_MAS_LNT0031DIESELPRICEHIST_ASPX"

        ''' <summary>
        ''' リース一覧
        ''' </summary>
        Public Const LNT0005L As String = "ASP.LNG_LEA_LNT0005LEASELIST_ASPX"
        ''' <summary>
        ''' リース明細
        ''' </summary>
        Public Const LNT0005D As String = "ASP.LNG_LEA_LNT0005LEASEDETAIL_ASPX"

        ''' <summary>
        ''' リース申請一覧
        ''' </summary>
        Public Const LNT0005B As String = "ASP.LNG_LEA_LNT0005LEASEAPPLYLIST_ASPX"
        ''' <summary>
        ''' リース新規申請画面
        ''' </summary>
        Public Const LNT0005N As String = "ASP.LNG_LEA_LNT0005LEASEAPPLYDETAILNEW_ASPX"
        ''' <summary>
        ''' リース更新申請画面
        ''' </summary>
        Public Const LNT0005C As String = "ASP.LNG_LEA_LNT0005LEASEAPPLYDETAILCHG_ASPX"

        ''' <summary>
        ''' 発送日報・他駅発送明細
        ''' </summary>
        Public Const LNT0006O As String = "ASP.LNG_REPORT_LNT0006SHIPPINGDAILYOUTPUT_ASPX"

        ''' <summary>
        ''' 収入管理一覧
        ''' </summary>
        Public Const LNT0007L As String = "ASP.LNG_PAY_LNT0007INCOMEMANAGE_ASPX"

        ''' <summary>
        ''' 収入管理詳細
        ''' </summary>
        Public Const LNT0007D As String = "ASP.LNG_PAY_LNT0007INCOMEDETAIL_ASPX"

        ''' <summary>
        ''' 清算ファイル対応状況一覧
        ''' </summary>
        Public Const LNT0008L As String = "ASP.LNG_PAY_LNT0008RESSNFLIST_ASPX"

        ''' <summary>
        ''' 清算ファイル登録・更新
        ''' </summary>
        Public Const LNT0008D As String = "ASP.LNG_PAY_LNT0008RESSNFDETAIL_ASPX"

        ''' <summary>
        ''' 賦金表一覧
        ''' </summary>
        Public Const LNT0009L As String = "ASP.LNG_LEA_LNT0009LEVIESLIST_ASPX"

        ''' <summary>
        ''' 賦金表照会
        ''' </summary>
        Public Const LNT0009D As String = "ASP.LNG_LEA_LNT0009LEVIESDISPLAY_ASPX"

        ''' <summary>
        ''' お支払書一覧
        ''' </summary>
        Public Const LNT0013L As String = "ASP.LNG_PAY_LNT0013PAYEELIST_ASPX"

        ''' <summary>
        ''' お支払書詳細
        ''' </summary>
        Public Const LNT0013D As String = "ASP.LNG_PAY_LNT0013PAYEEDETAIL_ASPX"

        ''' <summary>
        ''' 支払先マスタ連携メンテナンス（検索）
        ''' </summary>
        Public Const LNT0023S As String = "ASP.LNG_PAY_LNT0023PAYEELINKSEARCH_ASPX"
        ''' <summary>
        ''' 支払先マスタ連携メンテナンス（一覧）
        ''' </summary>
        Public Const LNT0023L As String = "ASP.LNG_PAY_LNT0023PAYEELINKLIST_ASPX"
        ''' <summary>
        ''' 支払先マスタ連携メンテナンス（詳細）
        ''' </summary>
        Public Const LNT0023D As String = "ASP.LNG_PAY_LNT0023PAYEELINKDETAIL_ASPX"

        ''' <summary>
        ''' ユーザIDマスタメンテナンス（検索）
        ''' </summary>
        Public Const LNS0001S As String = "ASP.LNG_MAS_LNS0001USERSEARCH_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNS0001L As String = "ASP.LNG_MAS_LNS0001USERLIST_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNS0001D As String = "ASP.LNG_MAS_LNS0001USERDETAIL_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（履歴）
        ''' </summary>
        Public Const LNS0001H As String = "ASP.LNG_MAS_LNS0001USERHISTORY_ASPX"

        ''' <summary>
        ''' ガイダンスマスタメンテナンス（検索）
        ''' </summary>
        Public Const LNS0008S As String = "ASP.LNG_MAS_LNS0008GUIDANCESEARCH_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNS0008L As String = "ASP.LNG_MAS_LNS0008GUIDANCELIST_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNS0008D As String = "ASP.LNG_MAS_LNS0008GUIDANCEDETAIL_ASPX"

        ''' <summary>
        ''' コンテナマスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0002S As String = "ASP.LNG_MAS_LNM0002RECONMSEARCH_ASPX"
        ''' <summary>
        ''' コンテナマスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0002L As String = "ASP.LNG_MAS_LNM0002RECONMLIST_ASPX"
        ''' <summary>
        ''' コンテナマスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0002D As String = "ASP.LNG_MAS_LNM0002RECONMDETAIL_ASPX"

        ''' <summary>
        ''' コンテナ取引先マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0003S As String = "ASP.LNG_MAS_LNM0003REKEJMSEARCH_ASPX"
        ''' <summary>
        ''' コンテナ取引先マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0003L As String = "ASP.LNG_MAS_LNM0003REKEJMLIST_ASPX"
        ''' <summary>
        ''' コンテナ取引先マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0003D As String = "ASP.LNG_MAS_LNM0003REKEJMDETAIL_ASPX"

        ''' <summary>
        ''' 単価マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0006S As String = "ASP.LNG_MAS_LNM0006TANKASEARCH_ASPX"
        ''' <summary>
        ''' 単価マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0006L As String = "ASP.LNG_MAS_LNM0006TANKALIST_ASPX"
        ''' <summary>
        ''' 単価マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0006D As String = "ASP.LNG_MAS_LNM0006TANKADETAIL_ASPX"
        ''' <summary>
        ''' 単価マスタメンテナンス（履歴）
        ''' </summary>
        Public Const LNM0006H As String = "ASP.LNG_MAS_LNM0006TANKAHISTORY_ASPX"

        ''' <summary>
        ''' 固定費マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0007L As String = "ASP.LNG_MAS_LNM0007KOTEIHILIST_ASPX"
        ''' <summary>
        ''' 固定費マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0007D As String = "ASP.LNG_MAS_LNM0007KOTEIHIDETAIL_ASPX"
        ''' <summary>
        ''' 固定費スタメンテナンス（履歴）
        ''' </summary>
        Public Const LNM0007H As String = "ASP.LNG_MAS_LNM0007KOTEIHIHISTORY_ASPX"

        ''' <summary>
        ''' コード変換特例２マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0008S As String = "ASP.LNG_MAS_LNM0008RECT2MSEARCH_ASPX"
        ''' <summary>
        ''' コード変換特例２マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0008L As String = "ASP.LNG_MAS_LNM0008RECT2MLIST_ASPX"
        ''' <summary>
        ''' コード変換特例２マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0008D As String = "ASP.LNG_MAS_LNM0008RECT2MDETAIL_ASPX"
        ''' <summary>
        ''' ＪＲ賃率マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0009S As String = "ASP.LNG_MAS_LNM0009RETINMSEARCH_ASPX"
        ''' <summary>
        ''' ＪＲ賃率マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0009L As String = "ASP.LNG_MAS_LNM0009RETINMLIST_ASPX"
        ''' <summary>
        ''' ＪＲ賃率マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0009D As String = "ASP.LNG_MAS_LNM0009RETINMDETAIL_ASPX"

        '''' <summary>
        '''' 特別料金マスタメンテナンス（一覧）
        '''' </summary>
        'Public Const LNM0010L As String = "ASP.LNG_MAS_LNM0010SPRATELIST_ASPX"
        '''' <summary>
        '''' 特別料金マスタメンテナンス（詳細）
        '''' </summary>
        'Public Const LNM0010D As String = "ASP.LNG_MAS_LNM0010SPRATEDETAIL_ASPX"
        '''' <summary>
        '''' 特別料金マスタメンテナンス（北海道ガス詳細）
        '''' </summary>
        'Public Const LNM0010DKG As String = "ASP.LNG_MAS_LNM0010SPRATEDETAILKG_ASPX"
        '''' <summary>
        '''' 特別料金スタメンテナンス（履歴）
        '''' </summary>
        'Public Const LNM0010H As String = "ASP.LNG_MAS_LNM0010SPRATEHISTORY_ASPX"

        ''' <summary>
        ''' キロ程マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0011S As String = "ASP.LNG_MAS_LNM0011REKMTMSEARCH_ASPX"
        ''' <summary>
        ''' キロ程マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0011L As String = "ASP.LNG_MAS_LNM0011REKMTMLIST_ASPX"
        ''' <summary>
        ''' キロ程マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0011D As String = "ASP.LNG_MAS_LNM0011REKMTMDETAIL_ASPX"
        ''' <summary>
        ''' 回送運賃適用率マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0013S As String = "ASP.LNG_MAS_LNM0013REKTRMSEARCH_ASPX"
        ''' <summary>
        ''' 回送運賃適用率マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0013L As String = "ASP.LNG_MAS_LNM0013REKTRMLIST_ASPX"
        ''' <summary>
        ''' 回送運賃適用率マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0013D As String = "ASP.LNG_MAS_LNM0013REKTRMDETAIL_ASPX"

        '''' <summary>
        '''' 通運発送料マスタメンテナンス（検索）
        '''' </summary>
        'Public Const LNM0014S As String = "ASP.LNG_MAS_LNM0014REUTRMSEARCH_ASPX"
        '''' <summary>
        '''' 通運発送料マスタメンテナンス（一覧）
        '''' </summary>
        'Public Const LNM0014L As String = "ASP.LNG_MAS_LNM0014REUTRMLIST_ASPX"
        '''' <summary>
        '''' 通運発送料マスタメンテナンス（詳細）
        '''' </summary>
        'Public Const LNM0014D As String = "ASP.LNG_MAS_LNM0014REUTRMDETAIL_ASPX"
        ''' <summary>
        ''' 特別料金マスタ改メンテナンス（一覧）
        ''' </summary>
        Public Const LNM0014L As String = "ASP.LNG_MAS_LNM0014SPRATELIST_ASPX"
        ''' <summary>
        ''' 特別料金マスタ改メンテナンス（詳細）
        ''' </summary>
        Public Const LNM0014D As String = "ASP.LNG_MAS_LNM0014SPRATEDETAIL_ASPX"
        ''' <summary>
        ''' 特別料金スタ改メンテナンス（履歴）
        ''' </summary>
        Public Const LNM0014H As String = "ASP.LNG_MAS_LNM0014SPRATEHISTORY_ASPX"
        ''' <summary>
        ''' 使用料率マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0015S As String = "ASP.LNG_MAS_LNM0015RESRTMSEARCH_ASPX"
        ''' <summary>
        ''' 使用料率マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0015L As String = "ASP.LNG_MAS_LNM0015RESRTMLIST_ASPX"
        ''' <summary>
        ''' 使用料率マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0015D As String = "ASP.LNG_MAS_LNM0015RESRTMDETAIL_ASPX"
        ''' <summary>
        ''' 使用料特例マスタ１マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0016S As String = "ASP.LNG_MAS_LNM0016REST1MSEARCH_ASPX"
        ''' <summary>
        ''' 使用料特例マスタ１マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0016L As String = "ASP.LNG_MAS_LNM0016REST1MLIST_ASPX"
        ''' <summary>
        ''' 使用料特例マスタ１マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0016D As String = "ASP.LNG_MAS_LNM0016REST1MDETAIL_ASPX"

        '''' <summary>
        '''' 使用料特例マスタ１マスタメンテナンス（検索）
        '''' </summary>
        'Public Const LNM0017S As String = "ASP.LNG_MAS_LNM0017REST2MSEARCH_ASPX"
        '''' <summary>
        '''' 使用料特例マスタ１マスタメンテナンス（一覧）
        '''' </summary>
        'Public Const LNM0017L As String = "ASP.LNG_MAS_LNM0017REST2MLIST_ASPX"
        '''' <summary>
        '''' 使用料特例マスタ１マスタメンテナンス（詳細）
        '''' </summary>
        'Public Const LNM0017D As String = "ASP.LNG_MAS_LNM0017REST2MDETAIL_ASPX"

        ''' <summary>
        ''' 休日割増単価マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0017L As String = "ASP.LNG_MAS_LNM0017HOLIDAYRATELIST_ASPX"
        ''' <summary>
        ''' 休日割増単価マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0017D As String = "ASP.LNG_MAS_LNM0017HOLIDAYRATEDETAIL_ASPX"
        ''' <summary>
        ''' 休日割増単価マスタメンテナンス（履歴）
        ''' </summary>
        Public Const LNM0017H As String = "ASP.LNG_MAS_LNM0017HOLIDAYRATEHISTORY_ASPX"

        ''' <summary>
        ''' サーチャージ定義マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0019L As String = "ASP.LNG_MAS_LNM0019SURCHARGEPATTERNLIST_ASPX"
        ''' <summary>
        ''' サーチャージ定義マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0019D As String = "ASP.LNG_MAS_LNM0019SURCHARGEPATTERNDETAIL_ASPX"
        ''' <summary>
        ''' サーチャージ定義マスタメンテナンス（履歴）
        ''' </summary>
        Public Const LNM0019H As String = "ASP.LNG_MAS_LNM0019SURCHARGEPATTERNHISTORY_ASPX"

        ''' <summary>
        ''' 軽油価格参照先管理マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0020L As String = "ASP.LNG_MAS_LNM0020DIESELPRICESITELIST_ASPX"
        ''' <summary>
        ''' 軽油価格参照先管理マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0020D As String = "ASP.LNG_MAS_LNM0020DIESELPRICESITEDETAIL_ASPX"
        ''' <summary>
        ''' 軽油価格参照先管理スタメンテナンス（履歴）
        ''' </summary>
        Public Const LNM0020H As String = "ASP.LNG_MAS_LNM0020DIESELPRICESITEHISTORY_ASPX"

        ''' <summary>
        ''' 品目マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0021S As String = "ASP.LNG_MAS_LNM0021ITEMSEARCH_ASPX"
        ''' <summary>
        ''' 品目マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0021L As String = "ASP.LNG_MAS_LNM0021ITEMLIST_ASPX"
        ''' <summary>
        ''' 品目マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0021D As String = "ASP.LNG_MAS_LNM0021ITEMDETAIL_ASPX"

        ''' <summary>
        ''' 荷主マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0023S As String = "ASP.LNG_MAS_LNM0023SHIPPERSEARCH_ASPX"
        ''' <summary>
        ''' 荷主マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0023L As String = "ASP.LNG_MAS_LNM0023SHIPPERLIST_ASPX"
        ''' <summary>
        ''' 荷主マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0023D As String = "ASP.LNG_MAS_LNM0023SHIPPERDETAIL_ASPX"

        ''' <summary>
        ''' 営業収入決済条件マスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0024S As String = "ASP.LNG_MAS_LNM0024KEKKJMSEARCH_ASPX"
        ''' <summary>
        ''' 営業収入決済条件マスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0024L As String = "ASP.LNG_MAS_LNM0024KEKKJMLIST_ASPX"
        ''' <summary>
        ''' 営業収入決済条件マスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0024D As String = "ASP.LNG_MAS_LNM0024KEKKJMDETAIL_ASPX"

        ''' <summary>
        ''' インセンティブマスタメンテナンス（検索）
        ''' </summary>
        Public Const LNM0026S As String = "ASP.LNG_MAS_LNM0026INCENTIVESEARCH_ASPX"
        ''' <summary>
        ''' インセンティブマスタメンテナンス（一覧）
        ''' </summary>
        Public Const LNM0026L As String = "ASP.LNG_MAS_LNM0026INCENTIVELIST_ASPX"
        ''' <summary>
        ''' インセンティブマスタメンテナンス（詳細）
        ''' </summary>
        Public Const LNM0026D As String = "ASP.LNG_MAS_LNM0026INCENTIVEDETAIL_ASPX"

        ''' <summary>
        ''' 会社マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0001S As String = "ASP.LNG_MAS_OIM0001CAMPSEARCH_ASPX"
        ''' <summary>
        ''' 会社マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0001L As String = "ASP.LNG_MAS_OIM0001CAMPLIST_ASPX"
        ''' <summary>
        ''' 会社マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0001C As String = "ASP.LNG_MAS_OIM0001CAMPCREATE_ASPX"

        ''' <summary>
        ''' 貨物駅マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0004S As String = "ASP.LNG_MAS_OIM0004STATIONSEARCH_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0004L As String = "ASP.LNG_MAS_OIM0004STATIONLIST_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0004C As String = "ASP.LNG_MAS_OIM0004STATIONCREATE_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0005S As String = "ASP.LNG_MAS_OIM0005TANKSEARCH_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0005L As String = "ASP.LNG_MAS_OIM0005TANKLIST_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0005C As String = "ASP.LNG_MAS_OIM0005TANKCREATE_ASPX"
        ''' <summary>
        ''' 列車マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0007S As String = "ASP.LNG_MAS_OIM0007TRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0007L As String = "ASP.LNG_MAS_OIM0007TRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0007C As String = "ASP.LNG_MAS_OIM0007TRAINCREATE_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0009S As String = "ASP.LNG_MAS_OIM0009PLANTSEARCH_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0009L As String = "ASP.LNG_MAS_OIM0009PLANTLIST_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0009C As String = "ASP.LNG_MAS_OIM0009PLANTCREATE_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0011S As String = "ASP.LNG_MAS_OIM0011TORISEARCH_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0011L As String = "ASP.LNG_MAS_OIM0011TORILIST_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0011C As String = "ASP.LNG_MAS_OIM0011TORICREATE_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0012S As String = "ASP.LNG_MAS_OIM0012NIUKESEARCH_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0012L As String = "ASP.LNG_MAS_OIM0012NIUKELIST_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0012C As String = "ASP.LNG_MAS_OIM0012NIUKECREATE_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0013S As String = "ASP.LNG_MAS_OIM0013LOADSEARCH_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0013L As String = "ASP.LNG_MAS_OIM0013LOADLIST_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0013C As String = "ASP.LNG_MAS_OIM0013LOADCREATE_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0014S As String = "ASP.LNG_MAS_OIM0014LOADCALCSEARCH_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0014L As String = "ASP.LNG_MAS_OIM0014LOADCALCLIST_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0014C As String = "ASP.LNG_MAS_OIM0014LOADCALCCREATE_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0015S As String = "ASP.LNG_MAS_OIM0015SYOGENSEARCH_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0015L As String = "ASP.LNG_MAS_OIM0015SYOGENLIST_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0015C As String = "ASP.LNG_MAS_OIM0015SYOGENCREATE_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（検索）
        ''' </summary>
        Public Const OIM0016S As String = "ASP.LNG_MAS_OIM0016RTRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（一覧）
        ''' </summary>
        Public Const OIM0016L As String = "ASP.LNG_MAS_OIM0016RTRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（登録）
        ''' </summary>
        Public Const OIM0016C As String = "ASP.LNG_MAS_OIM0016RTRAINCREATE_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0017S As String = "ASP.LNG_MAS_OIM0017TRAINOPERATIONSEARCH_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0017L As String = "ASP.LNG_MAS_OIM0017TRAINOPERATIONLIST_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0017C As String = "ASP.LNG_MAS_OIM0017TRAINOPERATIONCREATE_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0019S As String = "ASP.LNG_MAS_OIM0019ACCOUNTSEARCH_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0019L As String = "ASP.LNG_MAS_OIM0019ACCOUNTLIST_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0019C As String = "ASP.LNG_MAS_OIM0019ACCOUNTCREATE_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0020S As String = "ASP.LNG_MAS_OIM0020GUIDANCESEARCH_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0020L As String = "ASP.LNG_MAS_OIM0020GUIDANCELIST_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0020C As String = "ASP.LNG_MAS_OIM0020GUIDANCECREATE_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0021S As String = "ASP.LNG_MAS_OIM0021LOADRESERVESEARCH_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0021L As String = "ASP.LNG_MAS_OIM0021LOADRESERVELIST_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0021C As String = "ASP.LNG_MAS_OIM0021LOADRESERVECREATE_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（検索）
        ''' </summary>
        Public Const OIM0023S As String = "ASP.LNG_MAS_OIM0023BTRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（一覧）
        ''' </summary>
        Public Const OIM0023L As String = "ASP.LNG_MAS_OIM0023BTRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（登録）
        ''' </summary>
        Public Const OIM0023C As String = "ASP.LNG_MAS_OIM0023BTRAINCREATE_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0024S As String = "ASP.LNG_MAS_OIM0024PRIORITYSEARCH_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0024L As String = "ASP.LNG_MAS_OIM0024PRIORITYLIST_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0024C As String = "ASP.LNG_MAS_OIM0024PRIORITYCREATE_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0002S As String = "ASP.LNG_MAS_OIM0002ORGSEARCH_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0002L As String = "ASP.LNG_MAS_OIM0002ORGLIST_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0002C As String = "ASP.LNG_MAS_OIM0002ORGCREATE_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0003S As String = "ASP.LNG_MAS_OIM0003PRODULNSEARCH_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0003L As String = "ASP.LNG_MAS_OIM0003PRODUCTLIST_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0003C As String = "ASP.LNG_MAS_OIM0003PRODUCTCREATE_ASPX"

    End Class

End Module 'End BaseDllConst