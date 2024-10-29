'Option Strict On
'Option Explicit On

Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001OrderDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                 '一覧格納用テーブル
    Private LNT0001tbl_tab2 As DataTable                            '一覧格納用テーブル(タブ２用)
    Private LNT0001tbl_tab3 As DataTable                            '一覧格納用テーブル(タブ３用)
    Private LNT0001INPtbl As DataTable                              'チェック用テーブル
    Private LNT0001UPDtbl As DataTable                              '更新用テーブル
    Private LNT0001WKtbl As DataTable                               '作業用テーブル
    Private LNT0001WK2tbl As DataTable                              '作業用2テーブル(同一列車(同一発日)チェック用)
    Private LNT0001WK3tbl As DataTable                              '作業用3テーブル(同一列車(同一発日)タンク車チェック用)
    Private LNT0001WK4tbl As DataTable                              '作業用4テーブル(列車入線順重複チェック用)
    Private LNT0001WK5tbl As DataTable                              '作業用5テーブル(列車発送順重複チェック用)
    Private LNT0001WK6tbl As DataTable                              '作業用6テーブル(異なる列車(同一発日)チェック用)
    Private LNT0001WK7tbl As DataTable                              '作業用7テーブル(異なる列車(同一発日)タンク車チェック用)
    Private LNT0001WK8tbl As DataTable                              '作業用8テーブル(異なる列車(同一積込日)タンク車チェック用)
    Private LNT0001WK9tbl As DataTable                              '作業用9テーブル(他受注オーダーで積込日が同日チェック用)
    Private LNT0001WK10tbl As DataTable                             '作業用10テーブル(同一列車(同一積込日)タンク車チェック用)
    Private LNT0001WK11tbl As DataTable                             '作業用11テーブル(タンク車マスタ)中分類油種チェック用)
    Private LNT0001WKtbl_tab2 As DataTable                          '作業用テーブル(タブ２用)
    Private LNT0001WKtbl_tab3 As DataTable                          '作業用テーブル(タブ３用)
    Private LNT0001Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private LNT0001Oiltermtbl As DataTable                          '作業用テーブル(油種出荷期間マスタ取得用)
    Private LNT0001His1tbl As DataTable                             '履歴格納用テーブル(受注履歴)
    Private LNT0001His2tbl As DataTable                             '履歴格納用テーブル(受注明細履歴)
    Private LNT0001Reporttbl As DataTable                           '帳票用テーブル
    Private LNT0001ReportDeliverytbl As DataTable                   '帳票用(託送指示)テーブル
    Private LNT0001ReportOTLinkagetbl As DataTable                  '帳票用(OT発送日報)テーブル
    Private LNT0001FIDtbl_tab1 As DataTable                         '検索用テーブル(タブ１用)
    'Private LNT0001FIDtbl_tab2 As DataTable                        '検索用テーブル(タブ２用)
    Private LNT0001FIDtbl_tab3 As DataTable                         '検索用1テーブル(タブ３用)
    Private LNT0001FID2tbl_tab3 As DataTable                        '検索用2テーブル(タブ３用)(受注TBLから情報を取得)
    Private LNT0001NEWORDERNOtbl As DataTable                       '取得用(新規受注No取得用)テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部タブID
    Private Const CONST_DETAIL_NEWLIST As String = "5"              '明細一覧(新規作成)
    Private Const CONST_MAX_TABID As Integer = 4                    '詳細タブ数
    Private Const CONST_VBFLG As Integer = 1

    '〇タンク車割当状況
    Private Const CONST_TANKNO_STATUS_WARI As String = "割当"
    Private Const CONST_TANKNO_STATUS_MIWARI As String = "未割当"
    Private Const CONST_TANKNO_STATUS_FUKA As String = "不可"
    Private Const CONST_TANKNO_STATUS_ZAN As String = "残車"

    '◯交検・全件アラート表示用
    Private Const CONST_ALERT_STATUS_SAFE As String = "'<div class=""safe""></div>'"
    Private Const CONST_ALERT_STATUS_WARNING As String = "'<div class=""warning""></div>'"
    Private Const CONST_ALERT_STATUS_CAUTION As String = "'<div class=""caution""></div>'"

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード
    Private WW_UPBUTTONFLG As String = "0"                          '更新用ボタンフラグ(1:割当確定, 2:入力内容登録, 3:明細更新, 4:訂正更新, 5:割当更新)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(LNT0001tbl)
                    Master.RecoverTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                    Master.RecoverTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0001tbl, pnlListArea1) Then
                        Master.SaveTable(LNT0001tbl)
                    End If
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0001tbl_tab2, pnlListArea2) Then
                        Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                    End If
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0001tbl_tab3, pnlListArea3) Then
                        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
                    End If

                    '◯ フラグ初期化
                    Me.WW_UPBUTTONFLG = "0"
                    Me.WF_CheckBoxFLG.Value = "FALSE"
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDetailDownload"        '明細ﾀﾞｳﾝﾛｰﾄﾞボタン押下
                            WF_ButtonDetailDownload_Click()
                        Case "WF_ButtonPAYF"                  '精算ﾌｧｲﾙ作成ボタン押下
                            WF_ButtonPAYF_Click()
                        Case "WF_ButtonFEECALC_TAB1"          '料金計算ボタン押下
                            WF_ButtonFEECALC_TAB1_Click()
                        Case "WF_ButtonINSERT"                '登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"                   '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"               'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT"              'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click(WF_ButtonClick.Value)
                        Case "WF_LeftBoxSelectClick"          'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"                   '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                   '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"              '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT_TAB1",       '全選択ボタン押下
                             "WF_ButtonALLSELECT_TAB2",
                             "WF_ButtonALLSELECT_TAB3",
                             "WF_ButtonALLSELECT_TAB4"
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED_TAB1",   '選択解除ボタン押下
                             "WF_ButtonSELECT_LIFTED_TAB2",
                             "WF_ButtonSELECT_LIFTED_TAB3",
                             "WF_ButtonSELECT_LIFTED_TAB4"
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED_TAB1",     '行削除ボタン押下
                             "WF_ButtonLINE_LIFTED_TAB2",
                             "WF_ButtonLINE_LIFTED_TAB3",
                             "WF_ButtonLINE_LIFTED_TAB4"
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonLINE_ADD_TAB1",        '行追加ボタン押下
                             "WF_ButtonLINE_ADD_TAB2",
                             "WF_ButtonLINE_ADD_TAB3",
                             "WF_ButtonLINE_ADD_TAB4"
                            WF_ButtonLINE_ADD_Click()
                        Case "WF_MouseWheelUp"                'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"              'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_RadioButonClick"             '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"                  '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ListChange"                  'リスト変更
                            WF_ListChange()
                        Case "WF_ComDeleteIconClick"          'リスト削除
                            WF_ListDelete()
                        Case "WF_DTAB_Click"                  '○DetailTab切替処理
                            WF_Detail_TABChange()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            '○ 作成モード(１：新規登録, ２：更新)設定
            If work.WF_SEL_CREATEFLG.Text = "1" Then
                WF_CREATEFLG.Value = "1"
            Else
                WF_CREATEFLG.Value = "2"
            End If

            '◯受注進行ステータスが300:請求済のステータスに変更された場合
            If work.WF_SELROW_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then
                '参照モード
                WF_MAPpermitcode.Value = "FALSE"
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(LNT0001tbl) Then
                LNT0001tbl.Clear()
                LNT0001tbl.Dispose()
                LNT0001tbl = Nothing
            End If

            If Not IsNothing(LNT0001INPtbl) Then
                LNT0001INPtbl.Clear()
                LNT0001INPtbl.Dispose()
                LNT0001INPtbl = Nothing
            End If

            If Not IsNothing(LNT0001UPDtbl) Then
                LNT0001UPDtbl.Clear()
                LNT0001UPDtbl.Dispose()
                LNT0001UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = LNT0001WRKINC.MAPIDD
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.MENU Then
            '詳細画面
            work.WF_SEL_MAPIDBACKUP.Text = LNT0001WRKINC.MAPIDD
        Else
            '登録画面設定
            work.WF_SEL_MAPIDBACKUP.Text = LNT0001WRKINC.MAPIDD + "MAIN"
        End If

        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        '会社コード
        Me.WF_CAMPCODE.Text = Master.USERCAMP
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        WF_ButtonInsertFLG.Value = "FALSE"
        rightview.ResetIndex()
        leftview.ActiveListBox()
        Me.ChkAutoFlg.Checked = True

        '自動計算
        Me.ChkAutoFlg.Attributes("onclick") = "DtabChange(1)"

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

        '○ 詳細-画面初期設定
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

        '〇 タブ切替
        WF_Detail_TABChange()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○ 遷移先(各タブ)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        'オーダー№
        If Trim(work.WF_SELROW_ORDERNO.Text) <> "" Then
            Me.TxtOrderNo.Text = work.WF_SELROW_ORDERNO.Text

            '○ ヘッダ画面表示データ取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_GetOrderHead(SQLcon)
            End Using
        End If

        'ステータス
        If work.WF_SELROW_ORDERSTATUSNM.Text = "" Then
            work.WF_SELROW_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
            CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SELROW_ORDERSTATUSNM.Text, WW_DUMMY)
        Else
            CODENAME_get("ORDERSTATUS", work.WF_SELROW_ORDERSTATUS.Text, work.WF_SELROW_ORDERSTATUSNM.Text, WW_DUMMY)
        End If
        Me.TxtOrderStatus.Text = work.WF_SELROW_ORDERSTATUSNM.Text

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)

        If Me.TxtPlanDepYMD.Text = "" Then
            '年月日(発送日)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATEPLANDEP", Me.TxtPlanDepYMD.Text)
        End If

        '発送年月日が入力されている場合
        If Me.TxtPlanDepYMD.Text <> "" Then
            '125キロ賃率取得処理
            Call GetTinRetu125(Me.TxtPlanDepYMD.Text)
            '端数取得処理
            Call GetHassu(Me.TxtPlanDepYMD.Text)
        End If

        'コンテナ記号、コンテナ番号が入力されている場合
        If Me.TxtCtnTypeCode.Text <> "" AndAlso Me.TxtCtnNoCode.Text <> "" Then
            'コンテナマスタ取得処理
            Call GetCtnMst(Me.TxtCtnTypeCode.Text, Me.TxtCtnNoCode.Text)
        End If

        'フラグ等の初期化

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        'メニュー画面からの遷移の場合
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '作成フラグ(新規登録：1, 更新：2)
            work.WF_SEL_CREATEFLG.Text = "1"

            '○ 画面レイアウト設定
            If Master.VIEWID = "" Then
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "SCREENLAYOUT", Master.MAPID, WW_GetValue)

                Master.VIEWID = WW_GetValue(0)
            End If
        End If

        '〇画面表示設定処理
        WW_ScreenEnabledSet()

        '〇タブ「明細データ」表示用
        GridViewInitializeTab1()

        '〇タブ「精算予定ファイル」表示用
        GridViewInitializeTab2()

        '〇タブ「使用料金判定」表示用
        GridViewInitializeTab3()

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「明細データ」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab1()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab1(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea1
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー", needsPopUp:=True)
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 0
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「精算予定ファイル」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab2()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab2(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl_tab2)

        'TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB2"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = ""
        CS0013ProfView.LFUNC = ""

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー", needsPopUp:=True)
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 1
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「使用料金判定」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab3()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl_tab3)

        'TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB3"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea3
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = ""
        CS0013ProfView.LFUNC = ""

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー", needsPopUp:=True)
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 2
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(タブ「明細データ」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab1(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0001tbl) Then
            LNT0001tbl = New DataTable
        End If

        If LNT0001tbl.Columns.Count <> 0 Then
            LNT0001tbl.Columns.Clear()
        End If

        LNT0001tbl.Clear()

        If IsNothing(LNT0001WKtbl) Then
            LNT0001WKtbl = New DataTable
        End If

        If LNT0001WKtbl.Columns.Count <> 0 Then
            LNT0001WKtbl.Columns.Clear()
        End If

        LNT0001WKtbl.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        '抽出項目
        SQLStr &= " SELECT"
        SQLStr &= "   1                                             AS 'SELECT'"             'SELECT
        SQLStr &= " , 0                                             AS HIDDEN"               'HIDDEN
        SQLStr &= " , LNT0005.SAMEDAYCNT                            AS LINECNT"              '行No
        SQLStr &= " , ''                                            AS OPERATION"            '選択
        SQLStr &= " , @P01                                          AS ORDERNO"              '受注№
        SQLStr &= " , LNT0005.SAMEDAYCNT                            AS DETAILNO"             '受注明細№
        SQLStr &= " , coalesce(RTRIM(LNT0005.ITEMCD), '')             AS JRITEMCD"             'JR品目コード
        SQLStr &= " , coalesce(RTRIM(LNT0005.ITEMNM), '')             AS JRITEMNM"             'JR品目名
        SQLStr &= " , RIGHT('0000' + coalesce(RTRIM(LNT0006.JRITEMCD), '') , 4) AS ITEMCD"     '品目コード
        SQLStr &= " , coalesce(RTRIM(LNM0021.NAME), '')               AS ITEMNM"               '品目名
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPSTATION        ), '') AS DEPSTATION"           '発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_DEP.NAMES         ), '') AS DEPSTATIONNM"         '発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRSTATION        ), '') AS ARRSTATION"           '着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_ARR.NAMES         ), '') AS ARRSTATIONNM"         '着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAILDEPSTATION    ), '') AS RAILDEPSTATION"       '鉄道発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAILDEP.NAMES     ), '') AS RAILDEPSTATIONNM"     '鉄道発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAILARRSTATION    ), '') AS RAILARRSTATION"       '鉄道着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAILARR.NAMES     ), '') AS RAILARRSTATIONNM"     '鉄道着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAWDEPSTATION     ), '') AS RAWDEPSTATION"        '原発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAWDEP.NAMES      ), '') AS RAWDEPSTATIONNM"      '原発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAWARRSTATION     ), '') AS RAWARRSTATION"        '原着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAWARR.NAMES      ), '') AS RAWARRSTATIONNM"      '原着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPTRUSTEECD      ), '') AS DEPTRUSTEECD"         '発受託人コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_DEP.DEPTRUSTEENM  ), '') AS DEPTRUSTEENM"         '発受託人
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPTRUSTEESUBCD   ), '') AS DEPPICKDELTRADERCD"   '発受託人サブコード
        SQLStr &= " , coalesce(RTRIM(LNM0003_DEP.DEPTRUSTEESUBNM),'') AS DEPPICKDELTRADERNM"   '発受託人サブ
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRTRUSTEECD      ), '') AS ARRTRUSTEECD"         '着受託人コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_ARR.DEPTRUSTEENM  ), '') AS ARRTRUSTEENM"         '着受託人
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRTRUSTEESUBCD   ), '') AS ARRPICKDELTRADERCD"   '着受託人サブコード
        SQLStr &= " , coalesce(RTRIM(LNM0003_ARR.DEPTRUSTEESUBNM),'') AS ARRPICKDELTRADERNM"   '着受託人サブ
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPTRAINNO        ), '') AS DEPTRAINNO"           '発列車番号
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRTRAINNO        ), '') AS ARRTRAINNO"           '着列車番号
        SQLStr &= " , Convert(VARCHAR, LNT0006.ARRPLANYMD, 111)     AS PLANARRYMD"           '到着予定日
        SQLStr &= " , coalesce(RTRIM(LNT0005.RESULTARRYMD      ), '') AS RESULTARRYMD"         '到着実績日
        SQLStr &= " , coalesce(RTRIM(LNT0006.STACKFREEKBN      ), '') AS STACKFREEKBNCD"       '積空区分コード
        SQLStr &= " , coalesce(RTRIM(CTS0006.VALUE1            ), '') AS STACKFREEKBNNM"       '積空区分名
        SQLStr &= " , coalesce(RTRIM(LNT0005.CONSIGNEECD       ), '') AS JRSHIPPERCD"          'JR荷送人コード
        SQLStr &= " , coalesce(RTRIM(LNT0005.CONSIGNEENM       ), '') AS JRSHIPPERNM"          'JR荷送人
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPSHIPPERCD      ), '') AS SHIPPERCD"            '荷送人コード
        SQLStr &= " , coalesce(RTRIM(LNM0023_SHIP.NAME         ), '') AS SHIPPERNM"            '荷送人
        SQLStr &= " , coalesce(RTRIM(LNT0006.PICKUPTEL         ), '') AS SLCPICKUPTEL"         '集荷先電話番号
        SQLStr &= " , coalesce(RTRIM(LNT0005.OTHERFEE          ), '') AS OTHERFEE"             'その他料金
        SQLStr &= " , coalesce(RTRIM(LNT0006.CONTRACTCD        ), '') AS CONTRACTCD"           '契約コード
        SQLStr &= " , coalesce(RTRIM(LNT0005.DELFLG            ), '') AS DELFLG"               '削除フラグ
        '受注データ（ヘッダ）
        SQLStr &= " FROM LNG.LNT0004_ORDERHEAD LNT0004 "
        '受注データ（明細データ）
        SQLStr &= " LEFT JOIN LNG.LNT0005_ORDERDATA LNT0005 ON "
        SQLStr &= "       LNT0004.ORDERNO = LNT0005.ORDERNO "
        SQLStr &= "       AND LNT0005.DELFLG <> @P02"
        '受注データ（精算予定ファイル）
        SQLStr &= " INNER JOIN LNG.LNT0006_PAYPLANF LNT0006 "
        SQLStr &= "      ON LNT0005.ORDERNO = LNT0006.ORDERNO "
        SQLStr &= "     AND LNT0005.SAMEDAYCNT = LNT0006.SAMEDAYCNT "
        SQLStr &= "     AND LNT0006.DELFLG <> @P02"
        '品目マスタ 品目名
        SQLStr &= " LEFT JOIN LNG.LNM0021_ITEM LNM0021 "
        SQLStr &= "      ON LNT0006.JRITEMCD = LNM0021.ITEMCD"
        SQLStr &= "     AND LNM0021.DELFLG <> @P02"
        '駅マスタ 発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_DEP "
        SQLStr &= "      ON LNT0006.DEPSTATION = CTS0020_DEP.STATION"
        SQLStr &= "     AND CTS0020_DEP.DELFLG <> @P02"
        '駅マスタ 着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_ARR "
        SQLStr &= "      ON LNT0006.ARRSTATION = CTS0020_ARR.STATION"
        SQLStr &= "     AND CTS0020_ARR.DELFLG <> @P02"
        '駅マスタ 鉄道発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAILDEP "
        SQLStr &= "      ON LNT0005.RAILDEPSTATION = CTS0020_RAILDEP.STATION"
        SQLStr &= "     AND CTS0020_RAILDEP.DELFLG <> @P02"
        '駅マスタ 鉄道着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAILARR "
        SQLStr &= "      ON LNT0005.RAILARRSTATION = CTS0020_RAILARR.STATION"
        SQLStr &= "     AND CTS0020_RAILARR.DELFLG <> @P02"
        '駅マスタ 原発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAWDEP "
        SQLStr &= "      ON LNT0005.RAWDEPSTATION = CTS0020_RAWDEP.STATION"
        SQLStr &= "     AND CTS0020_RAWDEP.DELFLG <> @P02"
        '駅マスタ 原着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAWARR "
        SQLStr &= "      ON LNT0005.RAWARRSTATION = CTS0020_RAWARR.STATION"
        SQLStr &= "     AND CTS0020_RAWARR.DELFLG <> @P02"
        'コンテナ決済マスタ(発駅)
        SQLStr &= " LEFT JOIN LNG.LNM0003_REKEJM LNM0003_DEP "
        SQLStr &= "      ON LNT0006.DEPSTATION = LNM0003_DEP.DEPSTATION"
        SQLStr &= "     AND LNT0006.DEPTRUSTEECD = LNM0003_DEP.DEPTRUSTEECD"
        SQLStr &= "     AND LNT0006.DEPTRUSTEESUBCD = LNM0003_DEP.DEPTRUSTEESUBCD"
        SQLStr &= "     AND LNM0003_DEP.DELFLG <> @P02"
        'コンテナ決済マスタ(着駅)
        SQLStr &= " LEFT JOIN LNG.LNM0003_REKEJM LNM0003_ARR "
        SQLStr &= "      ON LNT0006.ARRSTATION = LNM0003_ARR.DEPSTATION"
        SQLStr &= "     AND LNT0006.ARRTRUSTEECD = LNM0003_ARR.DEPTRUSTEECD"
        SQLStr &= "     AND LNT0006.ARRTRUSTEESUBCD = LNM0003_ARR.DEPTRUSTEESUBCD"
        SQLStr &= "     AND LNM0003_ARR.DELFLG <> @P02"
        '固定値マスタ 積空区分
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006 "
        SQLStr &= "      ON LNT0006.STACKFREEKBN = CONVERT(int, CTS0006.KEYCODE)"
        SQLStr &= "     AND CTS0006.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006.CLASS = 'STACKFREEKBN'"
        SQLStr &= "     AND CTS0006.DELFLG <> @P02"
        '名称マスタ 荷送人
        SQLStr &= " LEFT JOIN LNG.LNM0023_SHIPPER LNM0023_SHIP "
        SQLStr &= "      ON LNT0006.DEPSHIPPERCD = LNM0023_SHIP.SHIPPERCD"
        SQLStr &= "     AND LNM0023_SHIP.DELFLG <> @P02"
        '条件
        SQLStr &= " WHERE LNT0004.ORDERNO = @P01"
        SQLStr &= " AND LNT0004.DELFLG <> @P02"
        'ソート順
        SQLStr &= " ORDER BY"
        SQLStr &= "     DETAILNO"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '受注№
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SELROW_ORDERNO.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D SELECT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(タブ「精算予定ファイル」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab2(ByVal SQLcon As MySqlConnection)
        If IsNothing(LNT0001tbl_tab2) Then
            LNT0001tbl_tab2 = New DataTable
        End If

        If LNT0001tbl_tab2.Columns.Count <> 0 Then
            LNT0001tbl_tab2.Columns.Clear()
        End If

        LNT0001tbl_tab2.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        '抽出項目
        SQLStr &= " SELECT"
        SQLStr &= "   1                                             AS 'SELECT'"             'SELECT
        SQLStr &= " , 0                                             AS HIDDEN"               'HIDDEN
        SQLStr &= " , LNT0006.SAMEDAYCNT                            AS LINECNT"              '行No
        SQLStr &= " , ''                                            AS OPERATION"            '選択
        SQLStr &= " , @P01                                          AS ORDERNO"              '受注№
        SQLStr &= " , LNT0006.SAMEDAYCNT                            AS DETAILNO"             '受注明細№
        SQLStr &= " , ''                                            AS ORDERINFO"            '情報
        SQLStr &= " , coalesce(RTRIM(LNT0006.SHIPFEE          ), '')  AS SHIPFEE"              '通運発送料 
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRIVEFEE        ), '')  AS ARRIVEFEE"            '通運到着料 
        SQLStr &= " , coalesce(RTRIM(LNT0006.JRFIXEDFARE      ), '')  AS JRFIXEDFARE"          'ＪＲ所定運賃
        SQLStr &= " , coalesce(RTRIM(LNT0006.USEFEE           ), '')  AS USEFEE"               '使用料金額
        SQLStr &= " , coalesce(RTRIM(LNT0006.OWNDISCOUNTFEE   ), '')  AS OWNDISCOUNTFEE"       '私有割引相当額
        SQLStr &= " , coalesce(RTRIM(LNT0006.RETURNFARE       ), '')  AS RETURNFARE"           '割戻し運賃
        SQLStr &= " , coalesce(RTRIM(LNT0006.NITTSUFREESENDFEE), '')  AS NITTSUFREESENDFEE"    '通運負担回送運賃
        SQLStr &= " , coalesce(RTRIM(LNT0006.MANAGEFEE        ), '')  AS MANAGEFEE"            '運行管理料
        SQLStr &= " , coalesce(RTRIM(LNT0006.SHIPBURDENFEE    ), '')  AS SHIPBURDENFEE"        '荷主負担運賃
        SQLStr &= " , coalesce(RTRIM(LNT0006.PICKUPFEE        ), '')  AS PICKUPFEE"            '集荷料
        SQLStr &= " , coalesce(RTRIM(LNT0006.DELIVERYFEE      ), '')  AS DELIVERYFEE"          '配達料
        SQLStr &= " , coalesce(RTRIM(LNT0006.OTHER1FEE        ), '')  AS OTHER1FEE"            'その他１
        SQLStr &= " , coalesce(RTRIM(LNT0006.OTHER2FEE        ), '')  AS OTHER2FEE"            'その他２
        SQLStr &= " , coalesce(RTRIM(LNT0006.FREESENDFEE      ), '')  AS FREESENDFEE"          '回送運賃
        SQLStr &= " , coalesce(RTRIM(LNT0006.DELFLG           ), '')  AS DELFLG"               '削除フラグ
        '受注データ（ヘッダ）
        SQLStr &= " FROM LNG.LNT0004_ORDERHEAD LNT0004 "
        '受注データ（明細データ）
        SQLStr &= " INNER JOIN LNG.LNT0006_PAYPLANF LNT0006 ON "
        SQLStr &= "       LNT0004.ORDERNO = LNT0006.ORDERNO "
        SQLStr &= "       AND LNT0006.DELFLG <> @P02"
        '条件
        SQLStr &= " WHERE LNT0004.ORDERNO = @P01"
        SQLStr &= " AND LNT0004.DELFLG <> @P02"
        'ソート順
        SQLStr &= " ORDER BY"
        SQLStr &= "     DETAILNO"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '受注№
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SELROW_ORDERNO.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl_tab2.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl_tab2.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB2 SELECT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(タブ「タンク車明細」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab3(ByVal SQLcon As MySqlConnection)
        If IsNothing(LNT0001tbl_tab3) Then
            LNT0001tbl_tab3 = New DataTable
        End If

        If LNT0001tbl_tab3.Columns.Count <> 0 Then
            LNT0001tbl_tab3.Columns.Clear()
        End If

        LNT0001tbl_tab3.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        '抽出項目
        SQLStr &= " SELECT"
        SQLStr &= "   1                                               AS 'SELECT'"             'SELECT
        SQLStr &= " , 0                                               AS HIDDEN"               'HIDDEN
        SQLStr &= " , LNT0006.SAMEDAYCNT                              AS LINECNT"              '行No
        SQLStr &= " , ''                                              AS OPERATION"            '選択
        SQLStr &= " , @P01                                            AS ORDERNO"              '受注№
        SQLStr &= " , LNT0006.SAMEDAYCNT                              AS DETAILNO"             '受注明細№
        SQLStr &= " , coalesce(RTRIM(LNT0006.FARECALCTUNAPPLKBN ), '')  AS FARECALCTUNAPPLKBN"       '運賃計算屯数 マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_TONSU_MF.VALUE1    ), '')  AS FARECALCTUNAPPLKBNNM"     '運賃計算屯数 マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.FARECALCTUNNEXTFLG ), '')  AS FARECALCTUNNEXTFLG"       '運賃計算屯数 現行／次期
        SQLStr &= " , coalesce(RTRIM(CTS0006_TONSU_NF.VALUE1    ), '')  AS FARECALCTUNDISPNEXTFLG"   '運賃計算屯数 現行／次期 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.FARECALCTUN        ), '')  AS FARECALCTUN"              '運賃計算屯数
        SQLStr &= " , coalesce(RTRIM(LNT0006.DISNO              ), '')  AS DISNO"                    '割引番号
        SQLStr &= " , coalesce(RTRIM(LNT0006.EXTNO              ), '')  AS EXTNO"                    '割増番号
        SQLStr &= " , coalesce(RTRIM(LNT0006.KIROAPPLKBN        ), '')  AS KIROAPPLKBN"              'キロ程 マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_KIRO_MF.VALUE1     ), '')  AS KIROAPPLKBNNM"            'キロ程 マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.KIRO               ), '')  AS KIRO"                     'キロ程
        SQLStr &= " , coalesce(RTRIM(LNT0006.RENTRATEAPPLKBN    ), '')  AS RENTRATEAPPLKBN"          'ＪＲ賃率 マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_TINRT_MF.VALUE1    ), '')  AS RENTRATEAPPLKBNNM"        'ＪＲ賃率 マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.RENTRATENEXTFLG    ), '')  AS RENTRATENEXTFLG"          'ＪＲ賃率 現行／次期
        SQLStr &= " , coalesce(RTRIM(CTS0006_TINRT_NF.VALUE1    ), '')  AS RENTRATEDISPNEXTFLG"      'ＪＲ賃率 現行／次期 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.RENTRATE           ), '')  AS RENTRATE"                 'ＪＲ賃率
        SQLStr &= " , coalesce(RTRIM(LNT0006.APPLYRATEAPPLKBN   ), '')  AS APPLYRATEAPPLKBN"         'コンテナ割引割増率マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_TEKRT_MF.VALUE1    ), '')  AS APPLYRATEAPPLKBNNM"       'コンテナ割引割増率マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.APPLYRATENEXTFLG   ), '')  AS APPLYRATENEXTFLG"         '適用率 現行／次期
        SQLStr &= " , coalesce(RTRIM(CTS0006_TEKRT_NF.VALUE1    ), '')  AS APPLYRATEDISPNEXTFLG"     '適用率 現行／次期 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.APPLYRATE          ), '')  AS APPLYRATE"                '適用率
        SQLStr &= " , coalesce(RTRIM(LNT0006.USEFEERATEAPPLKBN  ), '')  AS USEFEERATEAPPLKBN"        '使用料率マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_USE_MF.VALUE1      ), '')  AS USEFEERATEAPPLKBNNM"      '使用料率マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.USEFEERATE         ), '')  AS USEFEERATE"               '使用料率
        SQLStr &= " , coalesce(RTRIM(LNT0006.FREESENDRATEAPPLKBN), '')  AS FREESENDRATEAPPLKBN"        '回送運賃適用率<br>マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_KAISO_MF.VALUE1    ), '')  AS FREESENDRATEAPPLKBNNM"      '回送運賃適用率<br>マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.FREESENDRATENEXTFLG), '')  AS FREESENDRATENEXTFLG"        '回送運賃現行／次期
        SQLStr &= " , coalesce(RTRIM(CTS0006_KAISO_NF.VALUE1    ), '')  AS FREESENDRATEDISPNEXTFLG"    '回送運賃現行／次期 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.FREESENDRATE       ), '')  AS FREESENDRATE"               '回送運賃適用率
        SQLStr &= " , coalesce(RTRIM(LNT0006.SHIPFEEAPPLKBN     ), '')  AS SHIPFEEAPPLKBN"       '通運発送料マスタ
        SQLStr &= " , coalesce(RTRIM(CTS0006_HASOU_MF.VALUE1    ), '')  AS SHIPFEEAPPLKBNNM"     '通運発送料マスタ 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.SHIPFEENEXTFLG     ), '')  AS SHIPFEENEXTFLG"       '通運発送料現行／次期
        SQLStr &= " , coalesce(RTRIM(CTS0006_HASOU_NF.VALUE1    ), '')  AS SHIPFEEDISPNEXTFLG"   '通運発送料現行／次期 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.SHIPFEE            ), '')  AS SHIPFEE"              '通運発送料 
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRIVEFEE          ), '')  AS ARRIVEFEE"            '通運到着料 
        SQLStr &= " , coalesce(RTRIM(LNT0006.TARIFFAPPLKBN      ), '')  AS TARIFFAPPLKBN"        '使用料タリフ適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_TARIFF_KBN.VALUE1  ), '')  AS TARIFFAPPLKBNNM"      '使用料タリフ適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.OUTISLANDAPPLKBN   ), '')  AS OUTISLANDAPPLKBN"     '離島向け適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_RITO_KBN.VALUE1    ), '')  AS OUTISLANDAPPLKBNNM"   '離島向け適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.FREEAPPLKBN        ), '')  AS FREEAPPLKBN"          '使用料無料特認 区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_FREE_KBN.VALUE1    ), '')  AS FREEAPPLKBNNM"        '使用料無料特認 区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.SPECIALM1APPLKBN   ), '')  AS SPECIALM1APPLKBN"     '特例Ｍ１適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_SPE1_KBN.VALUE1    ), '')  AS SPECIALM1APPLKBNNM"   '特例Ｍ１適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.SPECIALM2APPLKBN   ), '')  AS SPECIALM2APPLKBN"     '特例Ｍ２適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_SPE2_KBN.VALUE1    ), '')  AS SPECIALM2APPLKBNNM"   '特例Ｍ２適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.SPECIALM3APPLKBN   ), '')  AS SPECIALM3APPLKBN"     '特例Ｍ３適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_SPE3_KBN.VALUE1    ), '')  AS SPECIALM3APPLKBNNM"   '特例Ｍ３適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.HOKKAIDOAPPLKBN    ), '')  AS HOKKAIDOAPPLKBN"      '北海道先方負担適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_HOKAI_KBN.VALUE1   ), '')  AS HOKKAIDOAPPLKBNNM"    '北海道先方負担適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.NIIGATAAPPLKBN     ), '')  AS NIIGATAAPPLKBN"       '新潟先方負担適用区分
        SQLStr &= " , coalesce(RTRIM(CTS0006_NIIGATA_KBN.VALUE1 ), '')  AS NIIGATAAPPLKBNNM"     '新潟先方負担適用区分 名称
        SQLStr &= " , coalesce(RTRIM(LNT0006.DELFLG             ), '')  AS DELFLG"               '削除フラグ
        '受注データ（ヘッダ）
        SQLStr &= " FROM LNG.LNT0004_ORDERHEAD LNT0004 "
        '受注データ（明細データ）
        SQLStr &= " INNER JOIN LNG.LNT0006_PAYPLANF LNT0006 ON "
        SQLStr &= "       LNT0004.ORDERNO = LNT0006.ORDERNO "
        SQLStr &= "       AND LNT0006.DELFLG <> @P02"
        '固定値マスタ 運賃計算屯数 マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TONSU_MF "
        SQLStr &= "      ON CTS0006_TONSU_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.FARECALCTUNAPPLKBN)"
        SQLStr &= "     AND CTS0006_TONSU_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TONSU_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_TONSU_MF.DELFLG <> @P02"
        '固定値マスタ 運賃計算屯数 現行／次期 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TONSU_NF "
        SQLStr &= "      ON CTS0006_TONSU_NF.KEYCODE = CONVERT(NVARCHAR, LNT0006.FARECALCTUNNEXTFLG)"
        SQLStr &= "     AND CTS0006_TONSU_NF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TONSU_NF.CLASS = 'NEXTFLG'"
        SQLStr &= "     AND CTS0006_TONSU_NF.DELFLG <> @P02"
        '固定値マスタ キロ程マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_KIRO_MF "
        SQLStr &= "      ON CTS0006_KIRO_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.KIROAPPLKBN)"
        SQLStr &= "     AND CTS0006_KIRO_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_KIRO_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_KIRO_MF.DELFLG <> @P02"
        '固定値マスタ ＪＲ賃率 マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TINRT_MF "
        SQLStr &= "      ON CTS0006_TINRT_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.RENTRATEAPPLKBN)"
        SQLStr &= "     AND CTS0006_TINRT_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TINRT_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_TINRT_MF.DELFLG <> @P02"
        '固定値マスタ ＪＲ賃率 現行／次期 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TINRT_NF "
        SQLStr &= "      ON CTS0006_TINRT_NF.KEYCODE = CONVERT(NVARCHAR, LNT0006.RENTRATENEXTFLG)"
        SQLStr &= "     AND CTS0006_TINRT_NF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TINRT_NF.CLASS = 'NEXTFLG'"
        SQLStr &= "     AND CTS0006_TINRT_NF.DELFLG <> @P02"
        '固定値マスタ 適用率 マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TEKRT_MF "
        SQLStr &= "      ON CTS0006_TEKRT_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.APPLYRATEAPPLKBN)"
        SQLStr &= "     AND CTS0006_TEKRT_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TEKRT_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_TEKRT_MF.DELFLG <> @P02"
        '固定値マスタ 適用率 現行／次期 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TEKRT_NF "
        SQLStr &= "      ON CTS0006_TEKRT_NF.KEYCODE = CONVERT(NVARCHAR, LNT0006.APPLYRATENEXTFLG)"
        SQLStr &= "     AND CTS0006_TEKRT_NF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TEKRT_NF.CLASS = 'NEXTFLG'"
        SQLStr &= "     AND CTS0006_TEKRT_NF.DELFLG <> @P02"
        '固定値マスタ 使用料率 マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_USE_MF "
        SQLStr &= "      ON CTS0006_USE_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.USEFEERATEAPPLKBN)"
        SQLStr &= "     AND CTS0006_USE_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_USE_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_USE_MF.DELFLG <> @P02"
        '固定値マスタ 回送運賃適用率 マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_KAISO_MF "
        SQLStr &= "      ON CTS0006_KAISO_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.FREESENDRATEAPPLKBN)"
        SQLStr &= "     AND CTS0006_KAISO_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_KAISO_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_KAISO_MF.DELFLG <> @P02"
        '固定値マスタ 回送運賃適用率  現行／次期 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_KAISO_NF "
        SQLStr &= "      ON CTS0006_KAISO_NF.KEYCODE = CONVERT(NVARCHAR, LNT0006.FREESENDRATENEXTFLG)"
        SQLStr &= "     AND CTS0006_KAISO_NF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_KAISO_NF.CLASS = 'NEXTFLG'"
        SQLStr &= "     AND CTS0006_KAISO_NF.DELFLG <> @P02"
        '固定値マスタ 発送料マスタ 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_HASOU_MF "
        SQLStr &= "      ON CTS0006_HASOU_MF.KEYCODE = CONVERT(NVARCHAR, LNT0006.SHIPFEEAPPLKBN)"
        SQLStr &= "     AND CTS0006_HASOU_MF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_HASOU_MF.CLASS = 'MSTFLG'"
        SQLStr &= "     AND CTS0006_HASOU_MF.DELFLG <> @P02"
        '固定値マスタ 発送料マスタ  現行／次期 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_HASOU_NF "
        SQLStr &= "      ON CTS0006_HASOU_NF.KEYCODE = CONVERT(NVARCHAR, LNT0006.SHIPFEENEXTFLG)"
        SQLStr &= "     AND CTS0006_HASOU_NF.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_HASOU_NF.CLASS = 'NEXTFLG'"
        SQLStr &= "     AND CTS0006_HASOU_NF.DELFLG <> @P02"
        '固定値マスタ 使用料タリフ適用区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_TARIFF_KBN "
        SQLStr &= "      ON CTS0006_TARIFF_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.TARIFFAPPLKBN)"
        SQLStr &= "     AND CTS0006_TARIFF_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_TARIFF_KBN.CLASS = 'TARIFFAPPLKBN'"
        SQLStr &= "     AND CTS0006_TARIFF_KBN.DELFLG <> @P02"
        '固定値マスタ 離島向け適用区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_RITO_KBN "
        SQLStr &= "      ON CTS0006_RITO_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.OUTISLANDAPPLKBN)"
        SQLStr &= "     AND CTS0006_RITO_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_RITO_KBN.CLASS = 'ISLANDAPPLKBN'"
        SQLStr &= "     AND CTS0006_RITO_KBN.DELFLG <> @P02"
        '固定値マスタ 使用料無料特認区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_FREE_KBN "
        SQLStr &= "      ON CTS0006_FREE_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.FREEAPPLKBN)"
        SQLStr &= "     AND CTS0006_FREE_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_FREE_KBN.CLASS = 'FREEAPPLKBN'"
        SQLStr &= "     AND CTS0006_FREE_KBN.DELFLG <> @P02"
        '固定値マスタ 使用料特例マスタ１適用区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_SPE1_KBN "
        SQLStr &= "      ON CTS0006_SPE1_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.SPECIALM1APPLKBN)"
        SQLStr &= "     AND CTS0006_SPE1_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_SPE1_KBN.CLASS = 'SPECIALMAPPLKBN'"
        SQLStr &= "     AND CTS0006_SPE1_KBN.DELFLG <> @P02"
        '固定値マスタ 使用料特例マスタ２適用区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_SPE2_KBN "
        SQLStr &= "      ON CTS0006_SPE2_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.SPECIALM2APPLKBN)"
        SQLStr &= "     AND CTS0006_SPE2_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_SPE2_KBN.CLASS = 'SPECIALMAPPLKBN'"
        SQLStr &= "     AND CTS0006_SPE2_KBN.DELFLG <> @P02"
        '固定値マスタ 使用料特例マスタ３適用区分 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_SPE3_KBN "
        SQLStr &= "      ON CTS0006_SPE3_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.SPECIALM3APPLKBN)"
        SQLStr &= "     AND CTS0006_SPE3_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_SPE3_KBN.CLASS = 'SPECIALMAPPLKBN'"
        SQLStr &= "     AND CTS0006_SPE3_KBN.DELFLG <> @P02"
        '固定値マスタ 北海道先方負担 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_HOKAI_KBN "
        SQLStr &= "      ON CTS0006_HOKAI_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.HOKKAIDOAPPLKBN)"
        SQLStr &= "     AND CTS0006_HOKAI_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_HOKAI_KBN.CLASS = 'HOKKAIDOAPPLKBN'"
        SQLStr &= "     AND CTS0006_HOKAI_KBN.DELFLG <> @P02"
        '固定値マスタ 新潟先方負担 名称
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_NIIGATA_KBN "
        SQLStr &= "      ON CTS0006_NIIGATA_KBN.KEYCODE = CONVERT(NVARCHAR, LNT0006.NIIGATAAPPLKBN)"
        SQLStr &= "     AND CTS0006_NIIGATA_KBN.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_NIIGATA_KBN.CLASS = 'NIIGATAAPPLKBN'"
        SQLStr &= "     AND CTS0006_NIIGATA_KBN.DELFLG <> @P02"
        '条件
        SQLStr &= " WHERE LNT0004.ORDERNO = @P01"
        SQLStr &= " AND LNT0004.DELFLG <> @P02"
        'ソート順
        SQLStr &= " ORDER BY"
        SQLStr &= "     DETAILNO"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '受注№
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SELROW_ORDERNO.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl_tab3.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl_tab3.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB3 SELECT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB3 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 各タブ(一覧)の再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ReDisplayTabList()

        '○ 画面表示データ再取得(受注(明細)画面表示データ取得)
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            work.WF_SEL_CREATEFLG.Text = 2
            MAPDataGetTab1(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl, work.WF_SEL_INPTAB1TBL.Text)

        '○ 画面表示データ再取得(タブ「精算予定ファイル」表示データ取得)
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab2(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ 画面表示データ再取得(タブ「タンク車明細」表示データ取得)
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub

#Region "一覧再表示処理"
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        '〇 選択されたタブの一覧を再表示
        'タブ「明細データ」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            '○ 画面表示データ復元
            'Master.RecoverTable(LNT0001tbl, work.WF_SEL_INPTAB1TBL.Text)
            '一覧再表示処理(タブ「明細データ」)
            DisplayGrid_TAB1()

            'タブ「精算予定ファイル」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            '○ 画面表示データ復元
            Master.RecoverTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
            '一覧再表示処理(タブ「精算予定ファイル」)
            DisplayGrid_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            Master.RecoverTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
            '一覧再表示処理(タブ「使用料金判定」)
            DisplayGrid_TAB3()

        End If

        '〇 画面表示設定処理
        WW_ScreenEnabledSet()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「明細データ」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB1()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0001row As DataRow In LNT0001tbl.Rows
            If LNT0001row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0001row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea1
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        'WW_ListTextBoxReadControl()
        'Dim divObj = DirectCast(pnlListArea1.FindControl(pnlListArea1.ID & "_DR"), Panel)
        'Dim tblObj = DirectCast(divObj.Controls(0), Table)
        'For Each rowitem As TableRow In tblObj.Rows
        '    For Each cellObj As TableCell In rowitem.Controls
        '        If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
        '            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") Then
        '            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
        '        End If
        '    Next
        'Next

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「精算予定ファイル」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB2()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0001tab2row As DataRow In LNT0001tbl_tab2.Rows
            If LNT0001tab2row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0001tab2row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl_tab2)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB2"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing
    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「使用料金判定」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB3()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0001tab3row As DataRow In LNT0001tbl_tab3.Rows
            If LNT0001tab3row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0001tab3row("SELECT") = WW_DataCNT

            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl_tab3)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB3"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea3
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        'WW_ListTextBoxReadControl()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing
    End Sub

#End Region

    ''' <summary>
    ''' 料金計算ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFEECALC_TAB1_Click()

        '料金計算処理
        Call RyokinKeisan_proc()

    End Sub

    Protected Sub RyokinKeisan_proc()

        Try
            '■受注明細データテーブル
            Dim intLineNo As Integer = 1
            For intCnt As Integer = 0 To LNT0001tbl_tab3.Rows.Count - 1

                Dim strKIRO As String = ""            'キロ程
                Dim strStackFreeKbnCd As String = ""  '積空区分
                Dim strDepStation As String = ""      '発駅コード
                Dim strDepTRUSTEECD As String = ""    '発受託人コード
                Dim strDepTRUSTEESUBCD As String = "" '発受託人サブコード
                Dim strArrStation As String = ""      '着駅コード
                Dim strArrTRUSTEECD As String = ""    '発受託人コード
                Dim strArrTRUSTEESUBCD As String = "" '発受託人サブコード
                Dim strJotDepBranchCd As String = ""  'ＪＯＴ発店所コード
                Dim strJotArrBranchCd As String = ""  'ＪＯＴ着店所コード
                Dim strDepTaiou1 As String = ""
                Dim strDepTaiou2 As String = ""
                Dim strArrTaiou1 As String = ""
                Dim strArrTaiou2 As String = ""
                Dim strDEPSHIPPERCD As String = ""    '発荷主コード
                Dim strJRITEMCD As String = ""        'ＪＲ品目コード
                Dim strKEIYAKUCD As String = ""
                Dim blnKeisanFlg As Boolean = True

                '引数用取得
                strKIRO = LNT0001tbl_tab3.Rows(intCnt)("KIRO").ToString
                strStackFreeKbnCd = LNT0001tbl.Rows(intCnt)("STACKFREEKBNCD").ToString()
                strDepStation = LNT0001tbl.Rows(intCnt)("DEPSTATION").ToString
                strDepTRUSTEECD = LNT0001tbl.Rows(intCnt)("DEPTRUSTEECD").ToString
                strDepTRUSTEESUBCD = LNT0001tbl.Rows(intCnt)("DEPPICKDELTRADERCD").ToString
                strArrStation = LNT0001tbl.Rows(intCnt)("ARRSTATION").ToString
                strArrTRUSTEECD = LNT0001tbl.Rows(intCnt)("ARRTRUSTEECD").ToString
                strArrTRUSTEESUBCD = LNT0001tbl.Rows(intCnt)("ARRPICKDELTRADERCD").ToString
                strDEPSHIPPERCD = LNT0001tbl.Rows(intCnt)("SHIPPERCD").ToString
                strJRITEMCD = LNT0001tbl.Rows(intCnt)("ITEMCD").ToString

                'DB接続
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    'JOT店所コード取得処理(ＪＯＴ発店所コード)
                    If GetJotBranchCd(SQLcon, strDepStation, strJotDepBranchCd, strDepTaiou1, strDepTaiou2) = False Then Exit Sub
                    'JOT店所コード取得処理(ＪＯＴ着店所コード)
                    If GetJotBranchCd(SQLcon, strArrStation, strJotArrBranchCd, strArrTaiou1, strArrTaiou2) = False Then Exit Sub
                End Using

                '計算屯数、割引番号、割増番号取得処理
                GetTonsu(Me.TxtPlanDepYMD.Text, Me.TxtBigCtnCode.Text, Me.TxtMiddleCtnCode.Text, strStackFreeKbnCd,
                     LNT0001tbl_tab3.Rows(intCnt))

                'キロ程取得処理
                GetKiro(Me.TxtPlanDepYMD.Text, strDepStation, strArrStation,
                     LNT0001tbl_tab3.Rows(intCnt))

                '賃率取得処理
                GetTinrt(Me.TxtPlanDepYMD.Text, LNT0001tbl_tab3.Rows(intCnt)("KIRO").ToString,
                     LNT0001tbl_tab3.Rows(intCnt))

                '適用率取得処理
                GetTekrt(Me.TxtPlanDepYMD.Text,
                     LNT0001tbl_tab3.Rows(intCnt)("DISNO").ToString,
                     LNT0001tbl_tab3.Rows(intCnt))

                '計算判定
                If ((Val(strStackFreeKbnCd) <> 1 AndAlso Val(strStackFreeKbnCd) <> 2) _
                OrElse (Me.work.WF_UPD_JURISDICTIONCD.Text = "12") _
                OrElse (Me.work.WF_UPD_JURISDICTIONCD.Text = "14" _
                        AndAlso Val(Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text) = 1 _
                        AndAlso Val(Me.work.WF_UPD_SPOTKBN.Text) = 1) _
                OrElse (Me.work.WF_UPD_JURISDICTIONCD.Text = "14" _
                        AndAlso Val(Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text) = 2 _
                        AndAlso Val(Me.work.WF_UPD_SPOTKBN.Text) = 0)
                ) Then
                    blnKeisanFlg = False
                End If

                '計算処理
                If blnKeisanFlg = True Then

                    '基本料金計算処理
                    GET_KYOT(strStackFreeKbnCd,
                     LNT0001tbl_tab3.Rows(intCnt)("FARECALCTUN").ToString,
                     LNT0001tbl_tab3.Rows(intCnt)("RENTRATE").ToString, LNT0001tbl_tab3.Rows(intCnt)("APPLYRATE").ToString,
                     Me.TxtRoundFeeCode.Text, Me.TxtRoundKbnGECode.Text, Me.TxtRoundKbnLTCode.Text,
                     LNT0001tbl_tab2.Rows(intCnt))

                    '積空区分が積の場合
                    If strStackFreeKbnCd = "1" Then
                        '使用料金計算処理
                        GET_SHIY(Me.TxtPlanDepYMD.Text, Me.TxtBigCtnCode.Text, Me.TxtMiddleCtnCode.Text,
                             LNT0001tbl_tab3.Rows(intCnt)("KIRO").ToString,
                             strDepStation, strDepTRUSTEECD, strDepTRUSTEESUBCD,
                             strArrStation, strArrTRUSTEECD, strArrTRUSTEESUBCD,
                             strJotDepBranchCd, strJotArrBranchCd,
                             strDepTaiou2, strArrTaiou2,
                             Me.TxtCtnType.Text, Me.TxtCtnNo.Text,
                             strDEPSHIPPERCD, strJRITEMCD,
                             LNT0001tbl_tab2.Rows(intCnt)("JRFIXEDFARE").ToString,
                             LNT0001tbl_tab2.Rows(intCnt)("OWNDISCOUNTFEE").ToString,
                             LNT0001tbl_tab2.Rows(intCnt)("OTHER1FEE").ToString,
                             Me.TxtRentRate125Code.Text,
                             LNT0001tbl_tab3.Rows(intCnt)("RENTRATE").ToString,
                             LNT0001tbl_tab2.Rows(intCnt), LNT0001tbl_tab3.Rows(intCnt))

                    ElseIf strStackFreeKbnCd = "2" Then
                        '回送費計算処理
                        GET_KAIS(Me.TxtPlanDepYMD.Text, Me.TxtBigCtnCode.Text, Me.TxtMiddleCtnCode.Text,
                             strDepStation, strDepTRUSTEECD, strDepTRUSTEESUBCD,
                             strArrStation, strArrTRUSTEECD, strArrTRUSTEESUBCD,
                             strDepTaiou1, strDepTaiou2, strArrTaiou2,
                             Me.TxtCtnType.Text, Me.TxtCtnNo.Text,
                             strKEIYAKUCD,
                             LNT0001tbl_tab2.Rows(intCnt)("JRFIXEDFARE").ToString,
                             LNT0001tbl_tab2.Rows(intCnt)("OTHER1FEE").ToString,
                             strJotDepBranchCd, strJotArrBranchCd,
                             LNT0001tbl_tab2.Rows(intCnt), LNT0001tbl_tab3.Rows(intCnt))
                    End If
                End If

                intLineNo += 1
            Next

        Finally
            '○ 画面表示データ保存
            Master.SaveTable(LNT0001tbl)
            Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
            Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
        End Try

    End Sub

    ''' <summary>
    ''' 登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""
        Dim WW_Message As String = ""
        Dim bolOrderHead As Boolean = False

        WW_ERR_SW = C_MESSAGE_NO.NORMAL

        '●入力チェック
        WW_Check(WW_RESULT)
        '受注データ存在チェック
        If WW_RESULT <> "ERR" Then
            '受注データ存在チェック
            bolOrderHead = ChkOrderHead(Me.TxtPlanDepYMD.Text, Me.TxtCtnTypeCode.Text, Me.TxtCtnNoCode.Text)
            '新規、更新判定
            If work.WF_SEL_CREATEFLG.Text = "1" Then
                '新規の場合
                If bolOrderHead = True Then
                    Master.Output(C_MESSAGE_NO.CTN_ORDER_REPEAT, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                    WW_RESULT = "ERR"
                End If
            Else
                '更新の場合
                If bolOrderHead = False Then
                    Master.Output(C_MESSAGE_NO.CTN_ORDER_NONE, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                    WW_RESULT = "ERR"
                End If
            End If
        End If

        '●入力チェック(明細データ)
        If WW_RESULT <> "ERR" Then
            If LNT0001tbl.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.CTN_ORDERDETAIL_NONE, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                WW_RESULT = "ERR"
            Else
                'チェック処理(タブ「明細データ」)
                WW_CheckTab1(WW_ERR_SW)
            End If
        End If

        '○ メッセージ表示
        '右BOXクローズ
        WF_RightboxOpen.Value = ""
        If isNormal(WW_ERR_SW) = False Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            '右BOXオープン
            WF_RightboxOpen.Value = "Open"
            WW_RESULT = "ERR"
        End If

        If WW_RESULT = "ERR" Then
            Exit Sub
        End If

        '自動計算の場合
        If Me.ChkAutoFlg.Checked = True Then
            '料金計算処理
            Call RyokinKeisan_proc()
        End If

        '〇 受注データDB更新
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '受信No判定
            If Me.TxtOrderNo.Text.Trim() = "" Then
                '登録処理
                WW_InsertOrderDetail(SQLcon)
            Else
                '更新処理
                If WW_UpdateOrderDetail(SQLcon) = False Then
                    Exit Sub
                End If
            End If
        End Using

        '○ メッセージ表示
        '右BOXクローズ
        WF_RightboxOpen.Value = ""
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

        ElseIf WW_ERR_SW = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR Then
            Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "受注No", needsPopUp:=True)

        Else
            Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            '右BOXオープン
            WF_RightboxOpen.Value = "Open"
        End If

        If isNormal(WW_ERR_SW) Then
            '一覧画面からの遷移の場合
            If (work.WF_SEL_MAPIDBACKUP.Text = LNT0001WRKINC.MAPIDD) Then
                '前ページ遷移
                Master.TransitionPrevPage()
            Else
                '★ 各タブ(一覧)の再表示処理
                ReDisplayTabList()
            End If
        End If

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.MAPID = work.WF_SEL_MAPIDBACKUP.Text

        '一覧画面からの遷移の場合
        If (work.WF_SEL_MAPIDBACKUP.Text = LNT0001WRKINC.MAPIDD) Then
            '前ページ遷移
            Master.TransitionPrevPage()
        Else
            'メニュー画面へ
            Master.TransitionPrevPage(, LNT0001WRKINC.TITLEKBNS)
        End If

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        '〇 コンテナ記号チェック
        '受注営業所が選択されていない場合は、他の検索(LEFTBOX)は表示させない制御をする
        'コンテナ番号
        If WF_FIELD.Value = "TxtCtnNo" Then
            If work.WF_SEL_CTNTYPECODE.Text = "" Then
                Master.Output(C_MESSAGE_NO.CTN_CTNTYPE_UNSELECT, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Me.TxtCtnType.Focus()
                WW_CheckERR("コンテナ記号が未選択。", C_MESSAGE_NO.CTN_CTNTYPE_UNSELECT)
                WF_LeftboxOpen.Value = ""   'LeftBoxを表示させない
                Exit Sub
            End If
        End If

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    End If

                    'コンテナ番号
                    If WF_FIELD.Value = "TxtCtnNo" Then
                        '〇 画面(コンテナ記号).テキストボックスで設定した値で絞る
                        prmData = work.CreateCTNNOParam(work.WF_SEL_CTNTYPECODE.Text, Me.TxtCtnNo.Text)
                    End If

                    '発受託人
                    If WF_FIELD.Value = "DEPTRUSTEECD" Then
                        Dim strStation As String = ""
                        Dim strDepTrusTee As String = ""
                        '○ LINECNT取得
                        Dim WW_LINECNT As Integer = 0
                        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = LNT0001tbl.AsEnumerable.
                                FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        strStation = updHeader.Item("DEPSTATION")
                        strDepTrusTee = updHeader.Item("DEPTRUSTEECD")

                        '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                        prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    End If

                    '着受託人
                    If WF_FIELD.Value = "ARRTRUSTEECD" Then
                        Dim strStation As String = ""
                        Dim strDepTrusTee As String = ""
                        '○ LINECNT取得
                        Dim WW_LINECNT As Integer = 0
                        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = LNT0001tbl.AsEnumerable.
                                FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        strStation = updHeader.Item("ARRSTATION")
                        strDepTrusTee = updHeader.Item("ARRTRUSTEECD")

                        '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                        prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    End If

                    '左リストボックス設定処理
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()

                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '発送予定日
                        Case "TxtPlanDepYMD"
                            .WF_Calendar.Text = Me.TxtPlanDepYMD.Text

                        '(一覧)交検日
                        Case "JRINSPECTIONDATE"

                            '○ LINECNT取得
                            Dim WW_LINECNT As Integer = 0
                            If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                            '○ 対象ヘッダー取得
                            Dim updHeader = LNT0001tbl.AsEnumerable.
                                FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                            If IsNothing(updHeader) Then Exit Sub

                            .WF_Calendar.Text = updHeader.Item("JRINSPECTIONDATE")

                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

#Region "チェックボックス(選択)クリック処理"
    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click(ByVal chkFieldName As String)

        '〇 選択されたチェックボックスを制御
        Me.WF_CheckBoxFLG.Value = "TRUE"
        'タブ「明細データ」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_CheckBoxSELECT_TAB1(chkFieldName)
        End If

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT_TAB1(ByVal chkFieldName As String)
        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        Select Case chkFieldName

            Case "WF_CheckBoxSELECT"
                'チェックボックス判定
                For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
                    If LNT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If LNT0001tbl.Rows(i)("OPERATION") = "on" Then
                            LNT0001tbl.Rows(i)("OPERATION") = ""
                        Else
                            LNT0001tbl.Rows(i)("OPERATION") = "on"
                        End If
                        Exit For
                    End If
                Next

            Case Else
                'チェックボックス判定
                For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
                    If LNT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If LNT0001tbl.Rows(i)("OPERATION") = "on" Then
                            LNT0001tbl.Rows(i)("OPERATION") = ""
                        Else
                            LNT0001tbl.Rows(i)("OPERATION") = "on"
                        End If
                        Exit For
                    End If
                Next
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)
    End Sub

#End Region

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                Case "TxtShippersCode"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtConsigneeCode"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtTrainNo"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case Else
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End Select
        End If
    End Sub

#Region "全選択ボタン押下時処理"
    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '〇 選択されたタブ一覧の全解除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonALLSELECT_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonALLSELECT_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonALLSELECT_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonALLSELECT_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB1()
        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0001tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB2()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB3()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB4()

    End Sub
#End Region

#Region "全解除ボタン押下時処理"
    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '〇 選択されたタブ一覧の全解除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonSELECT_LIFTED_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonSELECT_LIFTED_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonSELECT_LIFTED_TAB3()

        End If

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB1()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0001tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB3()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB4()

    End Sub
#End Region

#Region "行削除ボタン押下時処理"
    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        '〇 選択されたタブ一覧の行削除を制御
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonLINE_LIFTED_TAB1()
        End If

        '各タブ(一覧)の再表示処理
        ReDisplayTabList()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB1()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '■■■ LNT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注明細を一括論理削除
            Dim SQLStr As String =
                    " UPDATE LNG.LNT0005_ORDERDATA      " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND SAMEDAYCNT  = @P02       " _
                    & "    AND DELFLG     <> '1'       ;"

            SQLStr &=
                    " UPDATE LNG.LNT0006_PAYPLANF       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND SAMEDAYCNT  = @P02       " _
                    & "    AND DELFLG      <> '1'      ;"

            Dim SQLcmd As New MySqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)

            Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.DateTime)
            Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar)
            Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar)
            Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.DateTime)

            '件数を取得
            intTblCnt = LNT0001tbl.Rows.Count

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each LNT0001UPDrow In LNT0001tbl.Rows
                If LNT0001UPDrow("OPERATION") = "on" Then

                    If LNT0001UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    LNT0001UPDrow("LINECNT") = j        'LINECNT
                    LNT0001UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    LNT0001UPDrow("HIDDEN") = 1

                    PARA01.Value = LNT0001UPDrow("ORDERNO")
                    PARA02.Value = LNT0001UPDrow("DETAILNO")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()

                Else
                    i += 1
                    LNT0001UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '### 20200609 START(内部No178) #################################################
            '一覧明細の件数を取得
            Dim cntTbl As Integer = LNT0001tbl.Select("DELFLG <> '1'").Count
            If cntTbl = 0 Then
                '★ 一覧明細がすべて削除(0件)になった場合は、ステータスを受注ｷｬﾝｾﾙに更新する
                '◯ 受注TBLのステータス初期化
                WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_900,
                                     InitializeFlg:=True)

                '◯ 画面定義変数の初期化
                '　★受注進行ステータス(900:受注キャンセル)
                Me.TxtOrderStatus.Text = "受注キャンセル"
                work.WF_SELROW_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900
                '　★作成モード(１：新規登録, ２：更新)設定
                work.WF_SEL_CREATEFLG.Text = "1"

                '〇 登録ボタンを無効(False)
                WF_ButtonInsertFLG.Value = "FALSE"
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB1 DELETE", needsPopUp:=True)
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB1 DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        '○メッセージ表示
        '一覧件数が０件の時の行削除の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.CTN_DELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            '一覧件数が１件以上で未選択による行削除の場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.CTN_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

        End If

    End Sub

#End Region

#Region "行追加ボタン押下時処理"
    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()

        WW_ButtonLINE_ADD_TAB1()
        WW_ButtonLINE_ADD_TAB2()
        WW_ButtonLINE_ADD_TAB3()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB1()
        If IsNothing(LNT0001WKtbl) Then
            LNT0001WKtbl = New DataTable
        End If

        If LNT0001WKtbl.Columns.Count <> 0 Then
            LNT0001WKtbl.Columns.Clear()
        End If

        LNT0001WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String

        'If work.WF_SEL_ORDERNUMBER.Text = "" Then
        '○ 作成モード(１：新規登録, ２：更新)設定
        If work.WF_SEL_CREATEFLG.Text = "1" OrElse LNT0001tbl.Rows.Count = 0 Then
            SQLStrNum =
            " SELECT " _
            & "  @P01   AS ORDERNO" _
            & ", '001'  AS DETAILNO"

        Else
            SQLStrNum =
            " SELECT " _
            & "  coalesce(LNT0005.ORDERNO,'')                                     AS ORDERNO" _
            & ", coalesce(FORMAT(CONVERT(INT, LNT0005.SAMEDAYCNT) + 1,'000'),'000') AS DETAILNO" _
            & " FROM (" _
            & "  SELECT LNT0005.ORDERNO" _
            & "       , LNT0005.SAMEDAYCNT" _
            & "       , ROW_NUMBER() OVER(PARTITION BY LNT0005.ORDERNO ORDER BY LNT0005.ORDERNO, LNT0005.SAMEDAYCNT DESC) RNUM" _
            & "  FROM LNG.LNT0005_ORDERDATA LNT0005" _
            & "  WHERE LNT0005.ORDERNO = @P01" _
            & " ) LNT0005 " _
            & " WHERE LNT0005.RNUM = 1"
        End If

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0                   AS LINECNT" _
            & " , ''                  AS OPERATION" _
            & " , 1                   AS 'SELECT'" _
            & " , 0                   AS HIDDEN" _
            & " , @P01                AS ORDERNO" _
            & " , @P02                AS DETAILNO" _
            & " , ''                  AS JRITEMCD" _
            & " , ''                  AS JRITEMNM" _
            & " , ''                  AS ITEMCD" _
            & " , ''                  AS ITEMNM" _
            & " , ''                  AS DEPSTATION" _
            & " , ''                  AS DEPSTATIONNM" _
            & " , ''                  AS ARRSTATION" _
            & " , ''                  AS ARRSTATIONNM" _
            & " , ''                  AS RAILDEPSTATION" _
            & " , ''                  AS RAILDEPSTATIONNM" _
            & " , ''                  AS RAILARRSTATION" _
            & " , ''                  AS RAILARRSTATIONNM" _
            & " , ''                  AS RAWDEPSTATION" _
            & " , ''                  AS RAWDEPSTATIONNM" _
            & " , ''                  AS RAWARRSTATION" _
            & " , ''                  AS RAWARRSTATIONNM" _
            & " , ''                  AS DEPTRUSTEECD" _
            & " , ''                  AS DEPTRUSTEENM" _
            & " , ''                  AS DEPPICKDELTRADERCD" _
            & " , ''                  AS DEPPICKDELTRADERNM" _
            & " , ''                  AS ARRTRUSTEECD" _
            & " , ''                  AS ARRTRUSTEENM" _
            & " , ''                  AS ARRPICKDELTRADERCD" _
            & " , ''                  AS ARRPICKDELTRADERNM" _
            & " , ''                  AS DEPTRAINNO" _
            & " , ''                  AS ARRTRAINNO" _
            & " , ''                  AS PLANARRYMD" _
            & " , ''                  AS RESULTARRYMD" _
            & " , ''                  AS STACKFREEKBNCD" _
            & " , ''                  AS STACKFREEKBNNM" _
            & " , ''                  AS JRSHIPPERCD" _
            & " , ''                  AS JRSHIPPERNM" _
            & " , ''                  AS SHIPPERCD" _
            & " , ''                  AS SHIPPERNM" _
            & " , ''                  AS SLCPICKUPTEL" _
            & " , ''                  AS OTHERFEE" _
            & " , ''                  AS CONTRACTCD" _
            & " , '0'                 AS DELFLG" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdNum As New MySqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As MySqlParameter = SQLcmdNum.Parameters.Add("@P01", MySqlDbType.VarChar) '受注№
                PARANUM1.Value = work.WF_SELROW_ORDERNO.Text

                Using SQLdrNum As MySqlDataReader = SQLcmdNum.ExecuteReader()

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        LNT0001WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '受注№
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 3)  '受注明細№

                Dim intDetailNo As Integer = 0
                For Each LNT0001WKrow As DataRow In LNT0001WKtbl.Rows
                    intDetailNo = LNT0001WKrow("DETAILNO")
                    PARA1.Value = LNT0001WKrow("ORDERNO")
                    PARA2.Value = LNT0001WKrow("DETAILNO")
                Next

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each LNT0001row As DataRow In LNT0001tbl.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If LNT0001row("LINECNT") = 0 Then
                        LNT0001row("DETAILNO") = intDetailNo.ToString("000")

                    ElseIf LNT0001row("DETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf LNT0001row("HIDDEN") = 1 Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If LNT0001row("HIDDEN") = 1 Then
                        j += 1
                        LNT0001row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        LNT0001row("LINECNT") = i        'LINECNT
                    End If

                Next

                'CLOSE
                SQLcmd.Dispose()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB1 LINEADD", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB1 LINEADD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB2()

        If IsNothing(LNT0001WKtbl_tab2) Then
            LNT0001WKtbl_tab2 = New DataTable
        End If

        If LNT0001WKtbl_tab2.Columns.Count <> 0 Then
            LNT0001WKtbl_tab2.Columns.Clear()
        End If

        LNT0001WKtbl_tab2.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0                                        AS LINECNT" _
            & " , ''                                       AS OPERATION" _
            & " , 1                                        AS 'SELECT'" _
            & " , 0                                        AS HIDDEN" _
            & " , ''                                       AS ORDERNO" _
            & " , '001'                                    AS DETAILNO" _
            & " , ''                                       AS ORDERINFO" _
            & " , 0                                        AS SHIPFEE" _
            & " , 0                                        AS ARRIVEFEE" _
            & " , 0                                        AS JRFIXEDFARE" _
            & " , 0                                        AS USEFEE" _
            & " , 0                                        AS OWNDISCOUNTFEE" _
            & " , 0                                        AS RETURNFARE" _
            & " , 0                                        AS NITTSUFREESENDFEE" _
            & " , 0                                        AS MANAGEFEE" _
            & " , 0                                        AS SHIPBURDENFEE" _
            & " , 0                                        AS PICKUPFEE" _
            & " , 0                                        AS DELIVERYFEE" _
            & " , 0                                        AS OTHER1FEE" _
            & " , 0                                        AS OTHER2FEE" _
            & " , 0                                        AS FREESENDFEE" _
            & " , 0                                        AS DELFLG" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl_tab2.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                Dim intDetailNo As Integer = 0
                For Each LNT0001row As DataRow In LNT0001tbl_tab2.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If LNT0001row("LINECNT") = 0 Then
                        LNT0001row("DETAILNO") = intDetailNo.ToString("000")

                    ElseIf LNT0001row("DETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf LNT0001row("HIDDEN") = 1 Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If LNT0001row("HIDDEN") = 1 Then
                        j += 1
                        LNT0001row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        LNT0001row("LINECNT") = i        'LINECNT
                    End If

                Next

                'CLOSE
                SQLcmd.Dispose()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB2 LINEADD", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB2 LINEADD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB3()

        If IsNothing(LNT0001WKtbl_tab3) Then
            LNT0001WKtbl_tab3 = New DataTable
        End If

        If LNT0001WKtbl_tab3.Columns.Count <> 0 Then
            LNT0001WKtbl_tab3.Columns.Clear()
        End If

        LNT0001WKtbl_tab3.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0           AS LINECNT" _
            & " , ''          AS OPERATION" _
            & " , 1           AS 'SELECT'" _
            & " , 0           AS HIDDEN" _
            & " , ''          AS ORDERNO" _
            & " , '001'       AS DETAILNO" _
            & " , ''          AS FARECALCTUNAPPLKBN" _
            & " , ''          AS FARECALCTUNAPPLKBNNM" _
            & " , ''          AS FARECALCTUNNEXTFLG" _
            & " , ''          AS FARECALCTUNDISPNEXTFLG" _
            & " , ''          AS FARECALCTUN" _
            & " , ''          AS DISNO" _
            & " , ''          AS EXTNO" _
            & " , ''          AS KIROAPPLKBN" _
            & " , ''          AS KIROAPPLKBNNM" _
            & " , ''          AS KIRO" _
            & " , ''          AS RENTRATEAPPLKBN" _
            & " , ''          AS RENTRATEAPPLKBNNM" _
            & " , ''          AS RENTRATENEXTFLG" _
            & " , ''          AS RENTRATEDISPNEXTFLG" _
            & " , ''          AS RENTRATE" _
            & " , ''          AS APPLYRATEAPPLKBN" _
            & " , ''          AS APPLYRATEAPPLKBNNM" _
            & " , ''          AS APPLYRATENEXTFLG" _
            & " , ''          AS APPLYRATEDISPNEXTFLG" _
            & " , ''          AS APPLYRATE" _
            & " , ''          AS USEFEERATEAPPLKBN" _
            & " , ''          AS USEFEERATEAPPLKBNNM" _
            & " , ''          AS USEFEERATE" _
            & " , ''          AS FREESENDRATEAPPLKBN" _
            & " , ''          AS FREESENDRATEAPPLKBNNM" _
            & " , ''          AS FREESENDRATENEXTFLG" _
            & " , ''          AS FREESENDRATEDISPNEXTFLG" _
            & " , ''          AS FREESENDRATE" _
            & " , ''          AS SHIPFEEAPPLKBN" _
            & " , ''          AS SHIPFEEAPPLKBNNM" _
            & " , ''          AS SHIPFEENEXTFLG" _
            & " , ''          AS SHIPFEEDISPNEXTFLG" _
            & " , ''          AS SHIPFEE" _
            & " , ''          AS ARRIVEFEE" _
            & " , ''          AS TARIFFAPPLKBN" _
            & " , ''          AS TARIFFAPPLKBNNM" _
            & " , ''          AS OUTISLANDAPPLKBN" _
            & " , ''          AS OUTISLANDAPPLKBNNM" _
            & " , ''          AS FREEAPPLKBN" _
            & " , ''          AS FREEAPPLKBNNM" _
            & " , ''          AS SPECIALM1APPLKBN" _
            & " , ''          AS SPECIALM1APPLKBNNM" _
            & " , ''          AS SPECIALM2APPLKBN" _
            & " , ''          AS SPECIALM2APPLKBNNM" _
            & " , ''          AS SPECIALM3APPLKBN" _
            & " , ''          AS SPECIALM3APPLKBNNM" _
            & " , ''          AS HOKKAIDOAPPLKBN" _
            & " , ''          AS HOKKAIDOAPPLKBNNM" _
            & " , ''          AS NIIGATAAPPLKBN" _
            & " , ''          AS NIIGATAAPPLKBNNM" _
            & " , 0           AS DELFLG" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl_tab3.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                Dim intDetailNo As Integer = 0
                For Each LNT0001row As DataRow In LNT0001tbl_tab3.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If LNT0001row("LINECNT") = 0 Then
                        LNT0001row("DETAILNO") = intDetailNo.ToString("000")

                    ElseIf LNT0001row("DETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf LNT0001row("HIDDEN") = 1 Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If LNT0001row("HIDDEN") = 1 Then
                        j += 1
                        LNT0001row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        LNT0001row("LINECNT") = i        'LINECNT
                    End If

                Next

                'CLOSE
                SQLcmd.Dispose()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_TAB3 LINEADD", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_TAB3 LINEADD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

#Region "明細情報ダウンロード"
    ''' <summary>
    ''' 帳票(明細情報)出力
    ''' </summary>
    Protected Sub WF_ButtonDetailDownload_Click()

    End Sub

#End Region

    ''' <summary>
    ''' OT発送日報送信ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPAYF_Click()
        'Dim otLinkage As New LNT0001OTLinkageList
        'Dim selectedOrderInfo As New List(Of LNT0001OTLinkageList.OutputOrdedrInfo)

        Dim otMasterSts() As String = {Master.MAPID, Master.USERID, Master.USERTERMID}
        If IsNothing(LNT0001ReportOTLinkagetbl) Then
            LNT0001ReportOTLinkagetbl = New DataTable
        End If

        If LNT0001ReportOTLinkagetbl.Columns.Count <> 0 Then
            LNT0001ReportOTLinkagetbl.Columns.Clear()
        End If
        LNT0001ReportOTLinkagetbl.Clear()

        '******************************
        'OT発送日報データ取得処理
        '******************************
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'selectedOrderInfo = otLinkage.OTLinkageDataGet(SQLcon,
            '                           I_MASTERSTS:=otMasterSts,
            '                           I_ORDERNO:=Me.TxtOrderNo.Text,
            '                           I_LNT0001CsvOTLinkage:=LNT0001ReportOTLinkagetbl)
        End Using

        '******************************
        'CSV作成処理の実行
        '******************************
        'Dim OTFileName As String = otLinkage.SetCSVFileName(Me.TxtOrderOfficeCode.Text)
        'Using repCbj = New CsvCreate(LNT0001ReportOTLinkagetbl,
        '                             I_FolderPath:=CS0050SESSION.OTFILESEND_PATH,
        '                             I_FileName:=OTFileName,
        '                             I_Enc:="EBCDIC")
        '    'I_Enc:="UTF8N")
        '    'I_Enc:="EBCDIC")
        '    Dim url As String
        '    Try
        '        url = repCbj.ConvertDataTableToCsv(False, blnNewline:=False)
        '    Catch ex As Exception
        '        Return
        '    End Try
        '    '○ 別画面でExcelを表示
        '    WF_PrintURL.Value = url
        '    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        'End Using

        '******************************
        'OT発送日報データの（本体）ダウンロードフラグ更新
        '                  （明細）ダウンロード数インクリメント
        '******************************
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            MySqlConnection.ClearPool(SQLcon)
            Dim procDate As Date = Now
            Dim resProc As Boolean = False
            Dim orderDlFlags As Dictionary(Of String, String) = Nothing
            Using sqlTran As MySqlTransaction = SQLcon.BeginTransaction
                ''オーダー明細のダウンロードカウントのインクリメント
                'resProc = otLinkage.IncrementDetailOutputCount(selectedOrderInfo, "WF_ButtonOtSend", SQLcon, sqlTran, procDate, masterSts:=otMasterSts)
                'If resProc = False Then
                '    Return
                'End If
                ''オーダー明細よりダウンロードフラグを取得
                'orderDlFlags = otLinkage.GetOutputFlag(selectedOrderInfo, "WF_ButtonOtSend", SQLcon, sqlTran)
                'If orderDlFlags Is Nothing Then
                '    Return
                'End If
                ''オーダーを更新
                'resProc = otLinkage.UpdateOrderOutputFlag(orderDlFlags, "WF_ButtonOtSend", SQLcon, sqlTran, procDate, masterSts:=otMasterSts)
                'If resProc = False Then
                '    Return
                'End If
                '履歴登録用直近データ取得
                '直近履歴番号取得
                'Dim historyNo As String = otLinkage.GetNewOrderHistoryNo(SQLcon, sqlTran)
                'If historyNo = "" Then
                '    Return
                'End If
                'Dim orderTbl As DataTable = otLinkage.GetUpdatedOrder(selectedOrderInfo, SQLcon, sqlTran)
                'Dim detailTbl As DataTable = otLinkage.GetUpdatedOrderDetail(selectedOrderInfo, SQLcon, sqlTran)
                'If orderTbl IsNot Nothing AndAlso detailTbl IsNot Nothing Then
                '    Dim hisOrderTbl As DataTable = otLinkage.ModifiedHistoryDatatable(orderTbl, historyNo, masterSts:=otMasterSts)
                '    Dim hisDetailTbl As DataTable = otLinkage.ModifiedHistoryDatatable(detailTbl, historyNo, masterSts:=otMasterSts)

                '    '履歴テーブル登録
                '    For Each dr As DataRow In hisOrderTbl.Rows
                '        EntryHistory.InsertOrderHistory(SQLcon, sqlTran, dr)
                '    Next
                '    For Each dr As DataRow In hisDetailTbl.Rows
                '        EntryHistory.InsertOrderDetailHistory(SQLcon, sqlTran, dr)
                '    Next
                '    'ジャーナル登録
                '    otLinkage.OutputJournal(orderTbl, "OIT0002_ORDER")
                '    otLinkage.OutputJournal(detailTbl, "LNT0001_DETAIL")
                'End If

                'ここまで来たらコミット
                sqlTran.Commit()
            End Using

        End Using
    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

#Region "リスト変更時処理"
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        Select Case WF_DetailMView.ActiveViewIndex
                'タンク車割当
            Case 0
                WW_ListChange_TAB1()

                '入換・積込指示
            Case 1
                WW_ListChange_TAB2()

                'タンク車明細
            Case 2
                WW_ListChange_TAB3()

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「明細データ」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB1()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = LNT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '〇 一覧の件数取得
        Dim intListCnt As Integer = LNT0001tbl.Rows.Count

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea1.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
        Dim WW_ListName As String = ""

        Select Case WF_FIELD.Value
            Case "ITEMCD"      '品目コード
                If WW_ListValue <> "" Then
                    CODENAME_get("ITEMCD", WW_ListValue, WW_ListName, WW_RTN_SW)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("ITEMNM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("ITEMNM") = ""
                End If

            Case "DEPSTATION"  '発駅コード
                If WW_ListValue <> "" Then
                    CODENAME_get("DEPSTATION", WW_ListValue, WW_ListName, WW_RTN_SW)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("DEPSTATIONNM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("DEPSTATIONNM") = ""
                End If

            Case "ARRSTATION"  '着駅コード
                If WW_ListValue <> "" Then
                    CODENAME_get("ARRSTATION", WW_ListValue, WW_ListName, WW_RTN_SW)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("ARRSTATIONNM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("ARRSTATIONNM") = ""
                End If

            Case "DEPTRUSTEECD"  '発受託人コード
                If WW_ListValue <> "" Then
                    Dim strStationCd As String = updHeader.Item("DEPSTATION")
                    CODENAME_get("DEPTRUSTEECD", WW_ListValue, WW_ListName, WW_RTN_SW, strStationCd)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("DEPTRUSTEENM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("DEPTRUSTEENM") = ""
                End If

            Case "ARRTRUSTEECD"  '着受託人コード
                If WW_ListValue <> "" Then
                    Dim strStationCd As String = updHeader.Item("ARRSTATION")
                    CODENAME_get("ARRTRUSTEECD", WW_ListValue, WW_ListName, WW_RTN_SW, strStationCd)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("ARRTRUSTEENM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("ARRTRUSTEENM") = ""
                End If

            Case "SHIPPERCD"  '荷送人コード
                If WW_ListValue <> "" Then
                    CODENAME_get("SHIPPERCD", WW_ListValue, WW_ListName, WW_RTN_SW)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("SHIPPERNM") = WW_ListName
                Else
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("SHIPPERNM") = ""
                End If
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB2()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = LNT0001tbl_tab2.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '〇 一覧の件数取得
        Dim intListCnt As Integer = LNT0001tbl_tab2.Rows.Count

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea2.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "LOADINGIRILINEORDER"      '(一覧)積込入線順
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGOUTLETORDER") = ""
                    '入力された値が0、または一覧の件数より大きい場合
                ElseIf Integer.Parse(WW_ListValue) = 0 Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGOUTLETORDER") = ""
                ElseIf Integer.Parse(WW_ListValue) > intListCnt Then
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("LOADINGOUTLETORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)
                End If

            Case "FILLINGPOINT"             '(一覧)充填ポイント
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "LOADINGOUTLETORDER"       '(一覧)積込出線順
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGIRILINEORDER") = ""
                    '入力された値が0、または一覧の件数より大きい場合
                ElseIf Integer.Parse(WW_ListValue) = 0 Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGIRILINEORDER") = ""
                ElseIf Integer.Parse(WW_ListValue) > intListCnt Then
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("LOADINGIRILINEORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)
                End If

            'Case "LOADINGIRILINETRAINNO"    '(一覧)積込入線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGIRILINETRAINNAME") = ""

            'Case "LOADINGOUTLETTRAINNO"     '(一覧)積込出線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGOUTLETTRAINNAME") = ""

            Case "LINE"                     '(一覧)回線を一覧に設定
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    '入線列車番号
                    updHeader.Item("LOADINGIRILINETRAINNO") = ""
                    '入線列車名
                    updHeader.Item("LOADINGIRILINETRAINNAME") = ""
                    '出線列車番号
                    updHeader.Item("LOADINGOUTLETTRAINNO") = ""
                    '出線列車名
                    updHeader.Item("LOADINGOUTLETTRAINNAME") = ""
                    Exit Select
                End If

                ''〇営業所配下情報を取得・設定
                'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                '    '〇 画面(受注営業所).テキストボックスが未設定
                '    If Me.TxtOrderOffice.Text = "" Then
                '        WW_FixvalueMasterSearch(Master.USER_ORG, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                '    Else
                '        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                '    End If
                'Else
                '    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                'End If

                '入線列車番号
                updHeader.Item("LOADINGIRILINETRAINNO") = WW_GetValue(1)
                '入線列車名
                updHeader.Item("LOADINGIRILINETRAINNAME") = WW_GetValue(9)
                '出線列車番号
                updHeader.Item("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                '出線列車名
                updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB3()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = LNT0001tbl_tab3.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea3.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "CARSAMOUNT"            '(一覧)数量
                'updHeader.Item(WF_FIELD.Value) = WW_ListValue

                Dim regChkAmount As New Regex("^(?<seisu>(\d*))\.*(?<syosu>(\d*))$", RegexOptions.Singleline)
                Dim strSeisu As String  '整数部取得
                Dim strSyosu As String  '小数部取得

                Try
                    strSeisu = regChkAmount.Match(WW_ListValue).Result("${seisu}")
                    strSyosu = regChkAmount.Match(WW_ListValue).Result("${syosu}")
                    If strSyosu.Length > 0 _
                    OrElse strSeisu.Length <> 5 Then
                        'updHeader.Item(WF_FIELD.Value) = strSeisu.Substring(0, strSeisu.Length) & "." & "000"
                        updHeader.Item(WF_FIELD.Value) = "0.000"
                        Exit Select

                    End If

                    updHeader.Item(WF_FIELD.Value) = strSeisu.Substring(0, 2) & "." & strSeisu.Substring(2, 3)

                Catch ex As Exception
                    updHeader.Item(WF_FIELD.Value) = "0.000"
                    Exit Select

                End Try

            Case "JOINT"                 '(一覧)ジョイント先
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "CHANGETRAINNO",        '(一覧)本線列車番号変更
                 "SECONDARRSTATIONNAME", '(一覧)第2着駅
                 "SECONDCONSIGNEENAME",  '(一覧)第2荷受人
                 "CHANGERETSTATIONNAME"  '(一覧)空車着駅(変更)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub

#End Region

#Region "リスト削除時処理"
    ''' <summary>
    ''' リスト削除時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListDelete()
        '紐付けているリストのID
        Dim ListId As String = Master.DELETE_FIELDINFO.ListId
        'フィールド名
        Dim FieldName As String = Master.DELETE_FIELDINFO.FieldName
        '行番号
        Dim LineCnt As String = Master.DELETE_FIELDINFO.LineCnt

        Select Case ListId
                'タンク車割当
            Case "pnlListArea1"
                WW_ListDelete_TAB1(FieldName, LineCnt)

                '入換・積込指示
            Case "pnlListArea2"
                WW_ListDelete_TAB2(FieldName, LineCnt)

                'タンク車明細
            Case "pnlListArea3"
                WW_ListDelete_TAB3(FieldName, LineCnt)

                '費用入力
            Case "pnlListArea4"
                WW_ListDelete_TAB4(FieldName, LineCnt)
        End Select

    End Sub

    ''' <summary>
    ''' リスト削除時処理(タンク車割当)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TAB1(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)
        '○ 対象ヘッダー取得
        Dim updHeader = LNT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = I_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        Select Case I_FIELDNAME
            Case "JOINT"
                updHeader.Item("JOINT") = ""
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub
    ''' <summary>
    ''' リスト削除時処理(入換・積込指示)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TAB2(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)

    End Sub
    ''' <summary>
    ''' リスト削除時処理(タンク車明細)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TAB3(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)
        '○ 対象ヘッダー取得
        Dim updHeader = LNT0001tbl_tab3.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = I_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        Select Case I_FIELDNAME
            Case "JOINT"
                updHeader.Item("JOINT") = ""
            Case "ACTUALACCDATE"
                '### 20201210 START 指摘票対応(No246) #######################################
                updHeader.Item("OPERATION") = "on"
                updHeader.Item("ACTUALACCDATE") = ""
                updHeader.Item("ACTUALEMPARRDATE") = ""
            Case "ACTUALEMPARRDATE"
                updHeader.Item("OPERATION") = "on"
                updHeader.Item("ACTUALEMPARRDATE") = ""
                '### 20201210 END   指摘票対応(No246) #######################################
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub
    ''' <summary>
    ''' リスト削除時処理(費用入力)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TAB4(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)

    End Sub
#End Region

    ''' <summary>
    ''' タブ切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        'タブIndexを設定
        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        'タンク車割当
        WF_Dtab01.CssClass = ""
        '入換・積込指示
        WF_Dtab02.CssClass = ""
        'タンク車明細
        WF_Dtab03.CssClass = ""

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.CssClass = "selected"
            Case 1
                '入換・積込指示
                WF_Dtab02.CssClass = "selected"
            Case 2
                'タンク車明細
                WF_Dtab03.CssClass = "selected"
        End Select

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String,
                               Optional ByVal strSTATION_CD As String = "")

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                '会社コード
                Case "CAMPCODE"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                '運用部署
                Case "UORG"
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                '受注状態
                Case "ORDERSTATUS"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERSTATUS"))

                'コンテナ記号
                Case "CTNTYPE"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CTNTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CTNTYPE"))

                'コンテナ番号
                Case "CTNNO"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CTNNO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CTNNO"))

                '品目コード
                Case "ITEMCD"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ITEMCD, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, ""))

                '発駅コード
                Case "DEPSTATION"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPSTATION, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, ""))

                '着駅コード
                Case "ARRSTATION"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ARRSTATION, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, ""))

                '発受託人コード
                Case "DEPTRUSTEECD"
                    Dim strStation As String = ""
                    Dim strDepTrusTee As String = ""

                    strStation = strSTATION_CD
                    strDepTrusTee = I_VALUE

                    '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                    prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    '名称取得
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPTRUSTEECD, I_VALUE, O_TEXT, O_RTN, prmData)

                '着受託人コード
                Case "ARRTRUSTEECD"
                    Dim strStation As String = ""
                    Dim strDepTrusTee As String = ""

                    strStation = strSTATION_CD
                    strDepTrusTee = I_VALUE

                    '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                    prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    '名称取得
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ARRTRUSTEECD, I_VALUE, O_TEXT, O_RTN, prmData)

                '荷送人コード
                Case "SHIPPERCD"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPERCD, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, ""))
            End Select

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each LNT0001row As DataRow In LNT0001tbl.Rows
            Select Case LNT0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    LNT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    LNT0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNT0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT

    End Sub

#Region "各テーブル更新"
    ''' <summary>
    ''' 受注明細TBL登録
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_InsertOrderDetail(ByVal SQLcon As MySqlConnection)

        Try
            Using tran = SQLcon.BeginTransaction

                Dim htHeadDataParm As New Hashtable
                Dim strOrderNo As String
                Dim WW_DATENOW As DateTime = Date.Now

                'オーダーNo採番
                strOrderNo = EntryOrderData.GetNewOrderNo(SQLcon, tran)
                TxtOrderNo.Text = strOrderNo
                work.WF_SELROW_ORDERNO.Text = strOrderNo
                '■受注データ（ヘッダ） パラメータ設定処理
                htHeadDataParm = SetOrderHeadParam(strOrderNo, WW_DATENOW)
                '■受注データ（ヘッダ）登録処理
                EntryOrderData.InsertOrderHead(SQLcon, tran, htHeadDataParm)

                '■受注明細データテーブル
                Dim intLineNo As Integer = 1
                For intCnt As Integer = 0 To LNT0001tbl.Rows.Count - 1
                    Dim htDetailDataParm As New Hashtable
                    Dim htPayPlanFDataParm As New Hashtable

                    '■受注データ（明細データ） パラメータ設定処理
                    htDetailDataParm = SetOrderDetailParam(strOrderNo, WW_DATENOW, LNT0001tbl.Rows(intCnt), intLineNo)
                    '■受注データ（明細データ）登録処理
                    EntryOrderData.InsertOrderDetail(SQLcon, tran, htDetailDataParm)

                    '■精算予定ファイル パラメータ設定処理
                    htPayPlanFDataParm = SetPayPlanFParam(SQLcon, tran,
                                                          strOrderNo, WW_DATENOW, LNT0001tbl.Rows(intCnt), intLineNo,
                                                          LNT0001tbl_tab2.Rows(intCnt), LNT0001tbl_tab3.Rows(intCnt))
                    '■精算予定ファイル 登録処理
                    EntryOrderData.InsertPayPlanf(SQLcon, tran, htPayPlanFDataParm)

                    intLineNo += 1
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D ORDERDATA", needsPopUp:=True)
            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D ORDERDATA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 受注明細TBL更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Private Function WW_UpdateOrderDetail(ByVal SQLcon As MySqlConnection) As Boolean

        WW_UpdateOrderDetail = False

        Try
            Using tran = SQLcon.BeginTransaction

                Dim htHeadDataParm As New Hashtable
                Dim strOrderNo As String
                Dim WW_DATENOW As DateTime = Date.Now
                Dim blnChkCodeChangeErrFlg As Boolean = False

                'オーダーNo取得
                strOrderNo = Me.TxtOrderNo.Text
                '■受注データ（ヘッダ） パラメータ設定処理
                htHeadDataParm = SetOrderHeadParam(strOrderNo, WW_DATENOW)
                '■受注データ（ヘッダ）更新処理
                EntryOrderData.UpdateOrderHead(SQLcon, tran, htHeadDataParm)

                '■受注明細データテーブル
                Dim intLineNo As Integer = 1
                For intCnt As Integer = 0 To LNT0001tbl.Rows.Count - 1
                    Dim htDetailDataParm As New Hashtable
                    Dim htPayPlanFDataParm As New Hashtable

                    '■受注データ（明細データ） パラメータ設定処理
                    htDetailDataParm = SetOrderDetailParam(strOrderNo, WW_DATENOW, LNT0001tbl.Rows(intCnt), intLineNo)
                    '■精算予定ファイル パラメータ設定処理
                    htPayPlanFDataParm = SetPayPlanFParam(SQLcon, tran,
                                                          strOrderNo, WW_DATENOW, LNT0001tbl.Rows(intCnt), intLineNo,
                                                          LNT0001tbl_tab2.Rows(intCnt), LNT0001tbl_tab3.Rows(intCnt))

                    If ChkOrderDetail(SQLcon, tran, strOrderNo, intLineNo) = False Then
                        '■受注データ（明細データ）登録処理
                        EntryOrderData.InsertOrderDetail(SQLcon, tran, htDetailDataParm)
                        '■精算予定ファイル 登録処理
                        EntryOrderData.InsertPayPlanf(SQLcon, tran, htPayPlanFDataParm)
                    Else
                        '■精算予定ファイル 更新処理
                        EntryOrderData.UpdatePayPlanf(SQLcon, tran, htPayPlanFDataParm)
                    End If

                    'コード変換判定
                    If blnChkCodeChangeErrFlg = True Then
                        'コード変換特例処理
                        Call UpdCodeChange(SQLcon, tran, strOrderNo, intLineNo, WW_DATENOW, Master.USERID, Master.USERTERMID)
                    End If

                    intLineNo += 1
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D ORDERDATA", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D ORDERDATA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Function
        End Try

        WW_UpdateOrderDetail = True

    End Function

    ''' <summary>
    ''' 受注データ（ヘッダ） パラメータ設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Function SetOrderHeadParam(ByVal strPrmOrderNo As String, dtSysDateTime As DateTime) As Hashtable

        Dim htHeadDataTbl As New Hashtable

        htHeadDataTbl(C_HEADPARAM.HP_ORDERNO) = strPrmOrderNo                         'オーダーNo
        htHeadDataTbl(C_HEADPARAM.HP_PLANDEPYMD) = Me.TxtPlanDepYMD.Text              '発送予定日
        htHeadDataTbl(C_HEADPARAM.HP_CTNTYPE) = Me.TxtCtnTypeCode.Text                'コンテナ形式
        htHeadDataTbl(C_HEADPARAM.HP_CTNNO) = Me.TxtCtnNoCode.Text                    'コンテナ番号
        htHeadDataTbl(C_HEADPARAM.HP_STATUS) = BaseDllConst.CONST_ORDERSTATUS_200     '状態
        htHeadDataTbl(C_HEADPARAM.HP_BIGCTNCD) = Me.TxtBigCtnCode.Text                '大分類コード
        htHeadDataTbl(C_HEADPARAM.HP_MIDDLECTNCD) = Me.TxtMiddleCtnCode.Text          '中分類コード
        htHeadDataTbl(C_HEADPARAM.HP_SMALLCTNCD) = Me.TxtSmallCtnCode.Text            '小分類コード
        htHeadDataTbl(C_HEADPARAM.HP_RENTRATE125NEXTFLG) = Me.TxtTunNext125Code.Text  '125キロ賃率次期フラグ
        htHeadDataTbl(C_HEADPARAM.HP_RENTRATE125) = Me.TxtRentRate125Code.Text        '125キロ賃率
        htHeadDataTbl(C_HEADPARAM.HP_ROUNDFEENEXTFLG) = Me.TxtRoundTunNextCode.Text   '端数金額基準次期フラグ
        htHeadDataTbl(C_HEADPARAM.HP_ROUNDFEE) = Me.TxtRoundFeeCode.Text              '端数金額基準
        htHeadDataTbl(C_HEADPARAM.HP_ROUNDKBNGE) = Me.TxtRoundKbnGECode.Text          '端数区分金額以上
        htHeadDataTbl(C_HEADPARAM.HP_ROUNDKBNLT) = Me.TxtRoundKbnLTCode.Text          '端数区分金額未満
        htHeadDataTbl(C_HEADPARAM.HP_FILEID) = ""                                     'ファイルID
        htHeadDataTbl(C_HEADPARAM.HP_REFLECTFLG) = "0"                                '反映フラグ
        htHeadDataTbl(C_HEADPARAM.HP_DELFLG) = "0"                                    '削除フラグ
        htHeadDataTbl(C_HEADPARAM.HP_INITYMD) = dtSysDateTime                         '登録年月日
        htHeadDataTbl(C_HEADPARAM.HP_INITUSER) = Master.USERID                        '登録ユーザーＩＤ
        htHeadDataTbl(C_HEADPARAM.HP_INITTERMID) = Master.USERTERMID                  '登録端末
        htHeadDataTbl(C_HEADPARAM.HP_INITPGID) = Me.GetType().BaseType.Name           '登録プログラムＩＤ
        htHeadDataTbl(C_HEADPARAM.HP_UPDYMD) = dtSysDateTime                          '更新年月日
        htHeadDataTbl(C_HEADPARAM.HP_UPDUSER) = Master.USERID                         '更新ユーザーＩＤ
        htHeadDataTbl(C_HEADPARAM.HP_UPDTERMID) = Master.USERTERMID                   '更新端末
        htHeadDataTbl(C_HEADPARAM.HP_UPDPGID) = Me.GetType().BaseType.Name            '更新プログラムＩＤ

        Return htHeadDataTbl

    End Function

    ''' <summary>
    ''' 受注データ（明細データ） パラメータ設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Function SetOrderDetailParam(ByVal strPrmOrderNo As String, dtSysDateTime As DateTime,
                                         drOrder As DataRow, ByVal intSamedayCnt As Integer) As Hashtable

        Dim htDetailDataTbl As New Hashtable

        htDetailDataTbl(C_DATAPARAM.DP_ORDERNO) = strPrmOrderNo                            'オーダーNo
        htDetailDataTbl(C_DATAPARAM.DP_SAMEDAYCNT) = intSamedayCnt                         '同日内回数
        htDetailDataTbl(C_DATAPARAM.DP_FILEID) = ""                                        'ファイルID
        htDetailDataTbl(C_DATAPARAM.DP_FILELINENO) = ""                                    '行数
        htDetailDataTbl(C_DATAPARAM.DP_ITEMCD) = drOrder("ITEMCD")                         '品目コード
        htDetailDataTbl(C_DATAPARAM.DP_ITEMATTACHCD) = ""                                  '品目付属コード
        htDetailDataTbl(C_DATAPARAM.DP_ITEMNM) = drOrder("ITEMNM")                         '品目名
        htDetailDataTbl(C_DATAPARAM.DP_RAILDEPSTATION) = drOrder("DEPSTATION")             '鉄道発駅コード
        htDetailDataTbl(C_DATAPARAM.DP_DEPLEASEDLINECD) = ""                               '発専用線コード
        htDetailDataTbl(C_DATAPARAM.DP_RAILARRSTATION) = drOrder("ARRSTATION")             '鉄道着駅コード
        htDetailDataTbl(C_DATAPARAM.DP_ARRLEASEDLINECD) = ""                               '着専用線コード
        htDetailDataTbl(C_DATAPARAM.DP_RAWDEPSTATION) = ""                                 '原発駅
        htDetailDataTbl(C_DATAPARAM.DP_RAWARRSTATION) = ""                                 '原着駅
        htDetailDataTbl(C_DATAPARAM.DP_DEPSALEPLACECD) = ""                                '発コンテナ営業所コード
        htDetailDataTbl(C_DATAPARAM.DP_ARRSALEPLACECD) = ""                                '着コンテナ営業所コード
        htDetailDataTbl(C_DATAPARAM.DP_DEPTRUSTEECD) = drOrder("DEPTRUSTEECD")             '発受託人コード
        htDetailDataTbl(C_DATAPARAM.DP_DEPPICKDELTRADERCD) = drOrder("DEPPICKDELTRADERCD") '発集配業者コード
        htDetailDataTbl(C_DATAPARAM.DP_ARRTRUSTEECD) = drOrder("ARRTRUSTEECD")             '着受託人コード
        htDetailDataTbl(C_DATAPARAM.DP_ARRPICKDELTRADERCD) = drOrder("ARRPICKDELTRADERCD") '着集配業者コード
        htDetailDataTbl(C_DATAPARAM.DP_ROOTNO) = ""                                        'ルート番号
        htDetailDataTbl(C_DATAPARAM.DP_DEPTRAINNO) = drOrder("DEPTRAINNO")                 '発列車番号
        htDetailDataTbl(C_DATAPARAM.DP_ARRTRAINNO) = drOrder("ARRTRAINNO")                 '着列車番号
        htDetailDataTbl(C_DATAPARAM.DP_POINTFRAMENO) = ""                                  '指定枠番号
        htDetailDataTbl(C_DATAPARAM.DP_OBGETDISP) = ""                                     'ＯＢ取得表示
        htDetailDataTbl(C_DATAPARAM.DP_PLANDEPYMD) = ""                                    '発着予定日付-発車予定日時
        htDetailDataTbl(C_DATAPARAM.DP_PLANARRYMD) = drOrder("PLANARRYMD")                 '発着予定日付-到着予定日時
        htDetailDataTbl(C_DATAPARAM.DP_RESULTDEPYMD) = ""                                  '発着実績日付-発車実績日時
        htDetailDataTbl(C_DATAPARAM.DP_RESULTARRYMD) = ""                                  '発着実績日付-到着実績日時
        htDetailDataTbl(C_DATAPARAM.DP_CONTRACTCD) = ""                                    '契約コード
        htDetailDataTbl(C_DATAPARAM.DP_FAREPAYERCD) = ""                                   '運賃支払者コード
        htDetailDataTbl(C_DATAPARAM.DP_FAREPAYMETHODCD) = ""                               '運賃支払方法コード
        htDetailDataTbl(C_DATAPARAM.DP_FARECALCKIRO) = ""                                  '運賃計算キロ程
        htDetailDataTbl(C_DATAPARAM.DP_FARECALCTUN) = ""                                   '運賃計算屯数
        htDetailDataTbl(C_DATAPARAM.DP_DISEXTCD) = ""                                      '割引割増コード
        htDetailDataTbl(C_DATAPARAM.DP_DISRATE) = ""                                       '割引率
        htDetailDataTbl(C_DATAPARAM.DP_EXTRATE) = ""                                       '割増率
        htDetailDataTbl(C_DATAPARAM.DP_TOTALNUM) = ""                                      '総個数
        htDetailDataTbl(C_DATAPARAM.DP_CARGOWEIGHT) = ""                                   '荷重
        htDetailDataTbl(C_DATAPARAM.DP_COMPENSATION) = ""                                  '要賠償額
        htDetailDataTbl(C_DATAPARAM.DP_STANDARDYEAR) = ""                                  '運賃計算基準年
        htDetailDataTbl(C_DATAPARAM.DP_STANDARDMONTH) = ""                                 '運賃計算基準月
        htDetailDataTbl(C_DATAPARAM.DP_STANDARDDAY) = ""                                   '運賃計算基準日
        htDetailDataTbl(C_DATAPARAM.DP_RAILFARE) = ""                                      '鉄道運賃
        htDetailDataTbl(C_DATAPARAM.DP_ADDFARE) = ""                                       '増運賃
        htDetailDataTbl(C_DATAPARAM.DP_DGADDFARE) = ""                                     '危険物割増運賃
        htDetailDataTbl(C_DATAPARAM.DP_VALUABLADDFARE) = ""                                '貴重品割増運賃
        htDetailDataTbl(C_DATAPARAM.DP_SPECTNADDFARE) = ""                                 '特コン割増運賃
        htDetailDataTbl(C_DATAPARAM.DP_DEPSALEPLACEFEE) = ""                               '発営業所料金
        htDetailDataTbl(C_DATAPARAM.DP_ARRSALEPLACEFEE) = ""                               '着営業所料金
        htDetailDataTbl(C_DATAPARAM.DP_COMPENSATIONDISPFEE) = ""                           '要賠償額表示金額
        htDetailDataTbl(C_DATAPARAM.DP_OTHERFEE) = Replace(drOrder("OTHERFEE"), ",", "")   'その他料金
        htDetailDataTbl(C_DATAPARAM.DP_SASIZUFEE) = ""                                     'さしず手数料
        htDetailDataTbl(C_DATAPARAM.DP_TOTALFAREFEE) = ""                                  '合計運賃料金
        htDetailDataTbl(C_DATAPARAM.DP_STACKFREEKBN) = drOrder("STACKFREEKBNCD")           'コンテナ積空区分
        htDetailDataTbl(C_DATAPARAM.DP_ORDERMONTH) = ""                                    '受付月
        htDetailDataTbl(C_DATAPARAM.DP_ORDERDAY) = ""                                      '受付日
        htDetailDataTbl(C_DATAPARAM.DP_LOADENDMONTH) = ""                                  '積載完了月
        htDetailDataTbl(C_DATAPARAM.DP_LOADENDDAY) = ""                                    '積載完了日
        htDetailDataTbl(C_DATAPARAM.DP_DEVELOPENDMONTH) = ""                               '発達完了月
        htDetailDataTbl(C_DATAPARAM.DP_DEVELOPENDDAY) = ""                                 '発達完了日
        htDetailDataTbl(C_DATAPARAM.DP_DEVELOPSPETIME) = ""                                '発達指定時
        htDetailDataTbl(C_DATAPARAM.DP_CORRECTLOCASTACD) = ""                              '訂正所在駅コード
        htDetailDataTbl(C_DATAPARAM.DP_CORRECTNO) = ""                                     '訂正番号
        htDetailDataTbl(C_DATAPARAM.DP_CORRELNTYPE) = ""                                   '訂正種別
        htDetailDataTbl(C_DATAPARAM.DP_CORRELNMONTH) = ""                                  '訂正月
        htDetailDataTbl(C_DATAPARAM.DP_CORRECTDAY) = ""                                    '訂正日
        htDetailDataTbl(C_DATAPARAM.DP_ONUSLOCASTACD) = ""                                 '責任所在コード
        htDetailDataTbl(C_DATAPARAM.DP_SHIPPERCD) = drOrder("SHIPPERCD")                   '荷送人コード
        htDetailDataTbl(C_DATAPARAM.DP_SHIPPERNM) = drOrder("SHIPPERNM")                   '荷送人名
        htDetailDataTbl(C_DATAPARAM.DP_SHIPPERTEL) = ""                                    '荷送人電話番号
        htDetailDataTbl(C_DATAPARAM.DP_SLCPICKUPADDRESS) = ""                              '集荷先住所
        htDetailDataTbl(C_DATAPARAM.DP_SLCPICKUPTEL) = drOrder("SLCPICKUPTEL")             '集荷先電話番号
        htDetailDataTbl(C_DATAPARAM.DP_CONSIGNEECD) = ""                                   '荷受人コード
        htDetailDataTbl(C_DATAPARAM.DP_CONSIGNEENM) = ""                                   '荷受人名
        htDetailDataTbl(C_DATAPARAM.DP_CONSIGNEETEL) = ""                                  '荷受人電話番号
        htDetailDataTbl(C_DATAPARAM.DP_RECEIVERADDRESS) = ""                               '配達先住所
        htDetailDataTbl(C_DATAPARAM.DP_RECEIVERTEL) = ""                                   '配達先電話番号
        htDetailDataTbl(C_DATAPARAM.DP_INSURANCEFEE) = ""                                  '保険料
        htDetailDataTbl(C_DATAPARAM.DP_SHIPINSURANCEFEE) = ""                              '運送保険料金
        htDetailDataTbl(C_DATAPARAM.DP_LOADADVANCEFEE) = ""                                '荷掛立替金
        htDetailDataTbl(C_DATAPARAM.DP_SHIPFEE1) = ""                                      '発送料金１
        htDetailDataTbl(C_DATAPARAM.DP_SHIPFEE2) = ""                                      '発送料金２
        htDetailDataTbl(C_DATAPARAM.DP_PACKINGFEE) = ""                                    '梱包料金
        htDetailDataTbl(C_DATAPARAM.DP_ORIGINWORKFEE) = ""                                 '発地作業料
        htDetailDataTbl(C_DATAPARAM.DP_DEPOTHERFEE) = ""                                   '発その他料金
        htDetailDataTbl(C_DATAPARAM.DP_PAYMENTFEE) = ""                                    '着払料
        htDetailDataTbl(C_DATAPARAM.DP_DEPARTUREEETOTAL) = ""                              '発側料金計
        htDetailDataTbl(C_DATAPARAM.DP_DEPARTUREEE1) = ""                                  '到着料金１
        htDetailDataTbl(C_DATAPARAM.DP_DEPARTUREEE2) = ""                                  '到着料金２
        htDetailDataTbl(C_DATAPARAM.DP_UNPACKINGFEE) = ""                                  '開梱料金
        htDetailDataTbl(C_DATAPARAM.DP_LANDINGEORKFEE) = ""                                '着地作業料
        htDetailDataTbl(C_DATAPARAM.DP_ARROTHERFEE) = ""                                   '着その他料金
        htDetailDataTbl(C_DATAPARAM.DP_ARRARTUREEETOTAL) = ""                              '着側料金計
        htDetailDataTbl(C_DATAPARAM.DP_ARRNITTSUTAX) = ""                                  '着通運消費税額
        htDetailDataTbl(C_DATAPARAM.DP_SHIPPERPAYMETHOD) = ""                              '荷主支払方法
        htDetailDataTbl(C_DATAPARAM.DP_LUCKFEEINVOICENM) = ""                              '運地料金請求先名
        htDetailDataTbl(C_DATAPARAM.DP_ARTICLE) = ""                                       '記事
        htDetailDataTbl(C_DATAPARAM.DP_INPUTHOUR) = ""                                     '入力時刻(時)
        htDetailDataTbl(C_DATAPARAM.DP_INPUTMINUTE) = ""                                   '入力時刻(分)
        htDetailDataTbl(C_DATAPARAM.DP_INPUTSECOND) = ""                                   '入力時刻(秒)
        htDetailDataTbl(C_DATAPARAM.DP_CONSIGNCANCELKBN) = ""                              '託送取消区分
        htDetailDataTbl(C_DATAPARAM.DP_WIKUGUTRANKBN) = ""                                 'ウイクグ輸送区分
        htDetailDataTbl(C_DATAPARAM.DP_YOBI) = ""                                          '予備
        htDetailDataTbl(C_DATAPARAM.DP_REFLECTFLG) = "0"                                   '反映フラグ
        htDetailDataTbl(C_DATAPARAM.DP_SKIPFLG) = "0"                                      '読み飛ばしフラグ
        htDetailDataTbl(C_DATAPARAM.DP_DELFLG) = "0"                                       '削除フラグ
        htDetailDataTbl(C_DATAPARAM.DP_INITYMD) = dtSysDateTime                            '登録年月日
        htDetailDataTbl(C_DATAPARAM.DP_INITUSER) = Master.USERID                           '登録ユーザーＩＤ
        htDetailDataTbl(C_DATAPARAM.DP_INITTERMID) = Master.USERTERMID                     '登録端末
        htDetailDataTbl(C_DATAPARAM.DP_INITPGID) = Me.GetType().BaseType.Name              '登録プログラムＩＤ
        htDetailDataTbl(C_DATAPARAM.DP_UPDYMD) = dtSysDateTime                             '更新年月日
        htDetailDataTbl(C_DATAPARAM.DP_UPDUSER) = Master.USERID                            '更新ユーザーＩＤ
        htDetailDataTbl(C_DATAPARAM.DP_UPDTERMID) = Master.USERTERMID                      '更新端末
        htDetailDataTbl(C_DATAPARAM.DP_UPDPGID) = Me.GetType().BaseType.Name               '更新プログラムＩＤ

        Return htDetailDataTbl

    End Function

    ''' <summary>
    ''' 精算予定ファイル パラメータ設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Function SetPayPlanFParam(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                         ByVal strPrmOrderNo As String, dtSysDateTime As DateTime,
                                         ByVal drOrder As DataRow, ByVal intSamedayCnt As Integer,
                                         ByVal drTab2Data As DataRow, ByVal drTab3Data As DataRow) As Hashtable

        Dim htPayFDataTbl As New Hashtable
        Dim htLamasm As New Hashtable
        Dim htEGMNOO As New Hashtable
        Dim htREKEJM As New Hashtable
        Dim htZerit As New Hashtable

        Dim strBigCtnCd As String = ""         '大分類コード
        Dim strDepStation As String = ""       '発駅コード
        Dim strArrStation As String = ""       '着駅コード
        Dim strJotDepBranchCd As String = ""   'ＪＯＴ発店所コード
        Dim strJotArrBranchCd As String = ""   'ＪＯＴ着店所コード
        Dim strDepTaiou1 As String = ""
        Dim strDepTaiou2 As String = ""
        Dim strArrTaiou1 As String = ""
        Dim strArrTaiou2 As String = ""
        Dim strStackFreeKbnCd As String = ""   '積空区分
        Dim strStatusKbn As String = ""        '状態区分
        Dim strDepTrusteeCd As String = ""     '発受託人コード
        Dim strDepTrusteeSubCd As String = ""  '発受託人サブ
        Dim strArrTrusteeCd As String = ""     '着受託人コード
        Dim strArrTrusteeSubCd As String = ""  '着受託人サブ
        Dim strItemCd As String = ""           '品目コード
        Dim blnLeaseFlg As Boolean = True         'リース読込フラグ
        Dim strPartnerCampCd As String = ""       '相手先会社コード
        Dim strPartnerDeptCd As String = ""       '相手先部門コード
        Dim strInvKeijyoBranchCd As String = "0"  '計上店コード
        Dim strInvfilingDept As String = "0"      '請求書提出部店コード
        Dim strLeaseProductCd As String = ""      'リース品名コード
        Dim blnKessaiFlg As Boolean = False       '決済マスタ読込フラグ
        Dim strInvKesaiKbn As String = ""         '請求項目 請求書決済区分
        Dim strInvSubCd As String = ""            '請求項目 請求書細分コード
        Dim strPayKeijyoBranchCd As String = ""   '支払項目 費用計上店コード
        Dim strPayFilingBranch As String = ""     '支払項目 支払書提出支店
        Dim strTaxRate As String = ""             '税率
        Dim strTaxKbn As String = ""              '税区分

        '大分類コード
        strBigCtnCd = Me.TxtBigCtnCode.Text
        '発駅コード
        strDepStation = drOrder("DEPSTATION").ToString
        '着駅コード
        strArrStation = drOrder("ARRSTATION").ToString
        '積空区分
        strStackFreeKbnCd = drOrder("STACKFREEKBNCD").ToString
        '発受託人コード
        strDepTrusteeCd = drOrder("DEPTRUSTEECD").ToString
        '発受託人サブ
        strDepTrusteeSubCd = drOrder("DEPPICKDELTRADERCD").ToString
        '着受託人コード
        strArrTrusteeCd = drOrder("ARRTRUSTEECD").ToString
        '着受託人サブ
        strArrTrusteeSubCd = drOrder("ARRPICKDELTRADERCD").ToString
        '品目コード
        strItemCd = drOrder("ITEMCD").ToString

        'JOT店所コード取得処理(ＪＯＴ発店所コード)
        GetJotBranchCd(SQLcon, strDepStation, strJotDepBranchCd, strDepTaiou1, strDepTaiou2, p_tran)
        'JOT店所コード取得処理(ＪＯＴ着店所コード)
        GetJotBranchCd(SQLcon, strArrStation, strJotArrBranchCd, strArrTaiou1, strArrTaiou2, p_tran)

        '税率取得処理
        htZerit = GetZerit(SQLcon, p_tran, Me.TxtPlanDepYMD.Text)
        '税率
        strTaxRate = htZerit("SETVAL1").ToString

        '積空区分判定
        If strStackFreeKbnCd = "1" Then
            '状態区分に「10:積発」をセット
            strStatusKbn = "10"
            '税区分
            strTaxKbn = htZerit("SETVAL2").ToString
        Else
            '状態区分に「20:空発」をセット
            strStatusKbn = "20"
            '税区分
            strTaxKbn = htZerit("SETVAL3").ToString
        End If

        '全国通運以外サブコード０にする 発受託人判定
        If strDepTrusteeCd < 70000 OrElse strDepTrusteeCd > 79999 Then
            If strDepTrusteeCd <> 64552 Then
                strDepTrusteeSubCd = "0"
            End If
        End If

        '全国通運以外サブコード０にする 発受託人判定
        If strArrTrusteeCd < 70000 OrElse strArrTrusteeCd > 79999 Then
            If strArrTrusteeCd <> 64552 Then
                strArrTrusteeSubCd = "0"
            End If
        End If

        '複合一貫輸送品目コード入替
        If Val(Me.work.WF_UPD_COMPKANKBN.Text) <> 0 Then
            'ＪＲ品目コードが2501：ゴムの場合
            If strItemCd = "2501" Then
                strItemCd = "2509"
            End If
        End If

        '大分類コードが「20:Ｌ１０屯」、
        '且つ、発駅コードが「152500:札幌（タ）」、
        '且つ、着駅コードが「161700:苫小牧貨物」以外　の場合
        If Not (strBigCtnCd = "20" AndAlso strDepStation = "152500" AndAlso strArrStation = "161700") Then
            '無蓋コンテナ状態「積」でも品目「貨物積付用品」は「空」にする
            '大分類コードが「35:無蓋」、
            '且つ、積空区分が「1:積」、
            '且つ、ＪＲ品目コードが「4591:貨物積付用品」の場合
            If strBigCtnCd = "35" AndAlso strStackFreeKbnCd = "1" AndAlso strItemCd = "4591" Then
                strStackFreeKbnCd = "2"  '積空区分 2:空
                strStatusKbn = "25"      '状態区分 25:段積下
            Else
                '品目コ―ド＝ゼロ・返コン・回コンは空にする
                If Val(strItemCd) = 0 OrElse Val(strItemCd) = 17 OrElse Val(strItemCd) = 18 Then
                    strStackFreeKbnCd = "2"  '積空区分 2:空
                    strStatusKbn = "20"      '状態区分 20:空発
                End If
            End If
        End If

        '青函トンネル対応
        '発受託人コード、着受託人コード、品目コ―ド
        If Val(strDepTrusteeCd) = 0 AndAlso Val(strArrTrusteeCd) = 0 AndAlso Val(strItemCd) = 0 Then
            strStackFreeKbnCd = "2"  '積空区分 2:空
            strStatusKbn = "20"      '状態区分 20:空発
        End If

        '経理資産フラグが「2：リース」以外、
        '且つ、積空区分が「1:積」以外、
        'スポット区分が「2：スポットレンタル」
        '複合一貫区分が「0:該当せず」以外
        If Val(Me.work.WF_UPD_ACCOUNTINGASSETSCD.Text) <> 2 _
        AndAlso strStackFreeKbnCd <> "1" _
        AndAlso Val(Me.work.WF_UPD_SPOTKBN.Text) = 2 _
        AndAlso Val(Me.work.WF_UPD_COMPKANKBN.Text) <> 0 Then
            blnLeaseFlg = False
        End If

        'リース物件マスタ読む
        If blnLeaseFlg = True Then
            'レンタルシステム用リース物件マスタ取得処理
            htLamasm = GetLamasm(SQLcon, p_tran, Me.TxtCtnTypeCode.Text, Me.TxtCtnNoCode.Text)
            '相手先会社コード
            strPartnerCampCd = htLamasm("INVOICECAMPCD").ToString
            '相手先部門コード
            strPartnerDeptCd = htLamasm("INVOICEDEPTCD").ToString

            '積空区分が「1:積」
            If strStackFreeKbnCd = "1" Then
                'リース品名コード設定
                strLeaseProductCd = htLamasm("PRODUCTCD")
            End If

            'GAL部門新旧変換マスタ取得処理(計上店用)
            htEGMNOO = GetEgmnoo(SQLcon, p_tran, htLamasm("KEIJYOBRANCHCD"))
            '請求項目 計上店コード
            strInvKeijyoBranchCd = Val(htEGMNOO("NOOC2O"))

            'GAL部門新旧変換マスタ取得処理（請求書提出部店）
            htEGMNOO = GetEgmnoo(SQLcon, p_tran, htLamasm("INVFILINGDEPT"))
            '請求項目 請求書提出部店
            strInvfilingDept = Val(htEGMNOO("NOOC2O"))
        End If

        '決済マスタ読む条件判定
        '複合一貫区分が０以外
        If Val(Me.work.WF_UPD_COMPKANKBN.Text) <> 0 Then
            blnKessaiFlg = True
        End If

        '積空区分が1:積以外
        If strStackFreeKbnCd <> "1" Then
            blnKessaiFlg = True
        End If

        '経理資産区分が2:リース
        If Val(Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text) = 2 Then
            blnKessaiFlg = False
        End If

        'コンテナ決済マスタ取得
        If blnKessaiFlg = True Then
            htREKEJM = GetRekejm(SQLcon, p_tran, strDepStation, strDepTrusteeCd, strDepTrusteeSubCd)

            'データが存在する場合
            If Val(htREKEJM("MSTFLG").ToString) = 1 Then
                '積空区分が1:積
                If strStackFreeKbnCd = "1" Then
                    '相手先会社コード
                    strPartnerCampCd = htREKEJM("PARTNERCAMPCD").ToString
                    '相手先部門コード
                    strPartnerDeptCd = htREKEJM("PARTNERDEPTCD").ToString
                    '請求項目 計上店コード
                    strInvKeijyoBranchCd = Val(htREKEJM("INVKEIJYOBRANCHCD")).ToString
                    '請求項目 請求書提出部店
                    strInvfilingDept = Val(htREKEJM("INVFILINGDEPT")).ToString
                    '請求項目 請求書決済区分
                    strInvKesaiKbn = Val(htREKEJM("INVKESAIKBN")).ToString
                    '請求項目 請求書細分コード
                    strInvSubCd = Val(htREKEJM("INVSUBCD")).ToString
                Else
                    '支払項目 費用計上店コード
                    strPayKeijyoBranchCd = Val(htREKEJM("PAYKEIJYOBRANCHCD")).ToString
                    '支払項目 支払書提出支店
                    strPayFilingBranch = Val(htREKEJM("PAYFILINGBRANCH")).ToString
                End If

                '複合一貫区分が０以外、且つ、積空区分が2:空
                If Val(Me.work.WF_UPD_COMPKANKBN.Text) <> 0 AndAlso strStackFreeKbnCd = "2" Then
                    '支払項目 費用計上店コード
                    strPayKeijyoBranchCd = Val(htREKEJM("INVKEIJYOBRANCHCD")).ToString
                    '支払項目 支払書提出支店
                    strPayFilingBranch = Val(htREKEJM("INVFILINGDEPT")).ToString
                End If
            End If
        End If

        htPayFDataTbl(C_PAYFPARAM.PP_ORDERNO) = strPrmOrderNo                            'オーダーNo
        htPayFDataTbl(C_PAYFPARAM.PP_SAMEDAYCNT) = intSamedayCnt                         '同日内回数
        htPayFDataTbl(C_PAYFPARAM.PP_SHIPYMD) = Me.TxtPlanDepYMD.Text                    '発送年月日
        htPayFDataTbl(C_PAYFPARAM.PP_LINENUM) = "1"                                      '行番
        htPayFDataTbl(C_PAYFPARAM.PP_JOTDEPBRANCHCD) = strJotDepBranchCd                 'ＪＯＴ発店所コード
        htPayFDataTbl(C_PAYFPARAM.PP_DEPSTATION) = strDepStation                         '発駅コード
        htPayFDataTbl(C_PAYFPARAM.PP_DEPTRUSTEECD) = strDepTrusteeCd                     '発受託人コード
        htPayFDataTbl(C_PAYFPARAM.PP_DEPTRUSTEESUBCD) = strDepTrusteeSubCd               '発受託人サブ
        htPayFDataTbl(C_PAYFPARAM.PP_JOTARRBRANCHCD) = strJotArrBranchCd                 'ＪＯＴ着店所コード
        htPayFDataTbl(C_PAYFPARAM.PP_ARRSTATION) = strArrStation                         '着駅コード
        htPayFDataTbl(C_PAYFPARAM.PP_ARRTRUSTEECD) = strArrTrusteeCd                     '着受託人コード
        htPayFDataTbl(C_PAYFPARAM.PP_ARRTRUSTEESUBCD) = strArrTrusteeSubCd               '着受託人サブ
        htPayFDataTbl(C_PAYFPARAM.PP_ARRPLANYMD) = drOrder("PLANARRYMD")                 '到着予定年月日
        htPayFDataTbl(C_PAYFPARAM.PP_STACKFREEKBN) = strStackFreeKbnCd                   '積空区分
        htPayFDataTbl(C_PAYFPARAM.PP_STATUSKBN) = strStatusKbn                           '状態区分
        htPayFDataTbl(C_PAYFPARAM.PP_CONTRACTCD) = ""                                    '契約コード
        htPayFDataTbl(C_PAYFPARAM.PP_DEPTRAINNO) = drOrder("DEPTRAINNO")                 '発列車番号
        htPayFDataTbl(C_PAYFPARAM.PP_ARRTRAINNO) = drOrder("ARRTRAINNO")                 '着列車番号
        htPayFDataTbl(C_PAYFPARAM.PP_JRITEMCD) = strItemCd                               'ＪＲ品目コード
        htPayFDataTbl(C_PAYFPARAM.PP_LEASEPRODUCTCD) = strLeaseProductCd                 'リース品名コード
        htPayFDataTbl(C_PAYFPARAM.PP_DEPSHIPPERCD) = drOrder("SHIPPERCD")                '発荷主コード
        htPayFDataTbl(C_PAYFPARAM.PP_QUANTITY) = 1                                       '個数
        htPayFDataTbl(C_PAYFPARAM.PP_ADDSUBYM) = ""                                      '加減額の対象年月
        htPayFDataTbl(C_PAYFPARAM.PP_ADDSUBQUANTITY) = ""                                '加減額の個数
        htPayFDataTbl(C_PAYFPARAM.PP_JRFIXEDFARE) = Replace(drTab2Data("JRFIXEDFARE"), ",", "")                'ＪＲ所定運賃
        htPayFDataTbl(C_PAYFPARAM.PP_USEFEE) = Replace(drTab2Data("USEFEE"), ",", "")                          '使用料金額
        htPayFDataTbl(C_PAYFPARAM.PP_OWNDISCOUNTFEE) = Replace(drTab2Data("OWNDISCOUNTFEE"), ",", "")          '私有割引相当額
        htPayFDataTbl(C_PAYFPARAM.PP_RETURNFARE) = Replace(drTab2Data("RETURNFARE"), ",", "")                  '割戻し運賃
        htPayFDataTbl(C_PAYFPARAM.PP_NITTSUFREESENDFEE) = Replace(drTab2Data("NITTSUFREESENDFEE"), ",", "")    '通運負担回送運賃
        htPayFDataTbl(C_PAYFPARAM.PP_MANAGEFEE) = Replace(drTab2Data("MANAGEFEE"), ",", "")                    '運行管理料
        htPayFDataTbl(C_PAYFPARAM.PP_SHIPBURDENFEE) = Replace(drTab2Data("SHIPBURDENFEE"), ",", "")            '荷主負担運賃
        htPayFDataTbl(C_PAYFPARAM.PP_SHIPFEE) = Replace(drTab2Data("SHIPFEE"), ",", "")                        '発送料
        htPayFDataTbl(C_PAYFPARAM.PP_ARRIVEFEE) = Replace(drTab2Data("ARRIVEFEE"), ",", "")                    '到着料
        htPayFDataTbl(C_PAYFPARAM.PP_PICKUPFEE) = Replace(drTab2Data("PICKUPFEE"), ",", "")                    '集荷料
        htPayFDataTbl(C_PAYFPARAM.PP_DELIVERYFEE) = Replace(drTab2Data("DELIVERYFEE"), ",", "")                '配達料
        htPayFDataTbl(C_PAYFPARAM.PP_OTHER1FEE) = Replace(drTab2Data("OTHER1FEE"), ",", "")                    'その他１
        htPayFDataTbl(C_PAYFPARAM.PP_OTHER2FEE) = Replace(drTab2Data("OTHER2FEE"), ",", "")                    'その他２
        htPayFDataTbl(C_PAYFPARAM.PP_FREESENDFEE) = Replace(drTab2Data("FREESENDFEE"), ",", "")                '回送運賃
        htPayFDataTbl(C_PAYFPARAM.PP_SPRFITKBN) = ""                                                           '冷蔵適合マーク
        htPayFDataTbl(C_PAYFPARAM.PP_JURISDICTIONCD) = Me.work.WF_UPD_JURISDICTIONCD.Text                      '所管部コード
        htPayFDataTbl(C_PAYFPARAM.PP_ACCOUNTINGASSETSCD) = Me.work.WF_UPD_ACCOUNTINGASSETSCD.Text              '経理資産コード
        htPayFDataTbl(C_PAYFPARAM.PP_ACCOUNTINGASSETSKBN) = Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text            '経理資産区分
        htPayFDataTbl(C_PAYFPARAM.PP_DUMMYKBN) = Me.work.WF_UPD_DUMMYKBN.Text                                  'ダミー区分
        htPayFDataTbl(C_PAYFPARAM.PP_SPOTKBN) = Me.work.WF_UPD_SPOTKBN.Text                                    'スポット区分
        htPayFDataTbl(C_PAYFPARAM.PP_COMPKANKBN) = Me.work.WF_UPD_COMPKANKBN.Text                              '複合一貫区分
        htPayFDataTbl(C_PAYFPARAM.PP_KEIJOYM) = ""    '計上年月
        htPayFDataTbl(C_PAYFPARAM.PP_PARTNERCAMPCD) = strPartnerCampCd          '相手先会社コード
        htPayFDataTbl(C_PAYFPARAM.PP_PARTNERDEPTCD) = strPartnerDeptCd          '相手先部門コード
        htPayFDataTbl(C_PAYFPARAM.PP_INVKEIJYOBRANCHCD) = strInvKeijyoBranchCd  '請求項目 計上店コード
        htPayFDataTbl(C_PAYFPARAM.PP_INVFILINGDEPT) = strInvfilingDept          '請求項目 請求書提出部店
        htPayFDataTbl(C_PAYFPARAM.PP_INVKESAIKBN) = strInvKesaiKbn              '請求項目 請求書決済区分
        htPayFDataTbl(C_PAYFPARAM.PP_INVSUBCD) = strInvSubCd                    '請求項目 請求書細分コード
        htPayFDataTbl(C_PAYFPARAM.PP_PAYKEIJYOBRANCHCD) = strPayKeijyoBranchCd  '支払項目 費用計上店コード
        htPayFDataTbl(C_PAYFPARAM.PP_PAYFILINGBRANCH) = strPayFilingBranch      '支払項目 支払書提出支店
        htPayFDataTbl(C_PAYFPARAM.PP_TAXCALCUNIT) = ""                          '支払項目 消費税計算単位
        htPayFDataTbl(C_PAYFPARAM.PP_TAXKBN) = strTaxKbn         '税区分
        htPayFDataTbl(C_PAYFPARAM.PP_TAXRATE) = strTaxRate       '税率
        htPayFDataTbl(C_PAYFPARAM.PP_BEFDEPTRUSTEECD) = ""       '変換前項目-発受託人コード
        htPayFDataTbl(C_PAYFPARAM.PP_BEFDEPTRUSTEESUBCD) = ""    '変換前項目-発受託人サブ
        htPayFDataTbl(C_PAYFPARAM.PP_BEFDEPSHIPPERCD) = ""       '変換前項目-発荷主コード
        htPayFDataTbl(C_PAYFPARAM.PP_BEFARRTRUSTEECD) = ""       '変換前項目-着受託人コード
        htPayFDataTbl(C_PAYFPARAM.PP_BEFARRTRUSTEESUBCD) = ""    '変換前項目-着受託人サブ
        htPayFDataTbl(C_PAYFPARAM.PP_BEFJRITEMCD) = ""           '変換前項目-ＪＲ品目コード
        htPayFDataTbl(C_PAYFPARAM.PP_BEFSTACKFREEKBN) = ""       '変換前項目-積空区分
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFDEPSTATION) = ""      '分割前項目-発駅コード
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEECD) = ""    '分割前項目-発受託人コード
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFDEPTRUSTEESUBCD) = "" '分割前項目-発受託人サブ
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFUSEFEE) = ""          '分割前項目-使用料金額
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFSHIPFEE) = ""         '分割前項目-発送料
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFARRIVEFEE) = ""       '分割前項目-到着料
        htPayFDataTbl(C_PAYFPARAM.PP_SPLBEFFREESENDFEE) = ""     '分割前項目-回送運賃
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG1) = ""    '処理フラグ-料金計算済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG2) = ""    '処理フラグ-精算ファイル作成済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG3) = ""    '処理フラグ-運用ファイル作成済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG4) = ""    '処理フラグ-複合一貫作成済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG5) = ""    '処理フラグ-請求支払分割済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG6) = ""    '処理フラグ-コード変換済
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG7) = ""    '処理フラグ-ダミーフラグ７
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG8) = ""    '処理フラグ-ダミーフラグ８
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG9) = ""    '処理フラグ-ダミーフラグ９
        htPayFDataTbl(C_PAYFPARAM.PP_PROCFLG10) = ""    '処理フラグ-ダミーフラグ１０
        htPayFDataTbl(C_PAYFPARAM.PP_PICKUPTEL) = drOrder("SLCPICKUPTEL")  '集荷先電話番号
        htPayFDataTbl(C_PAYFPARAM.PP_FARECALCTUNAPPLKBN) = drTab3Data("FARECALCTUNAPPLKBN")    '運賃計算屯数適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_FARECALCTUNNEXTFLG) = drTab3Data("FARECALCTUNNEXTFLG")    '運賃計算屯数次期フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_FARECALCTUN) = drTab3Data("FARECALCTUN")                  '運賃計算屯数
        htPayFDataTbl(C_PAYFPARAM.PP_DISNO) = drTab3Data("DISNO")    '割引番号
        htPayFDataTbl(C_PAYFPARAM.PP_EXTNO) = drTab3Data("EXTNO")    '割増番号
        htPayFDataTbl(C_PAYFPARAM.PP_KIROAPPLKBN) = drTab3Data("KIROAPPLKBN")          'キロ程適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_KIRO) = Replace(drTab3Data("KIRO"), ",", "")      'キロ程
        htPayFDataTbl(C_PAYFPARAM.PP_RENTRATEAPPLKBN) = drTab3Data("RENTRATEAPPLKBN")  '賃率適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_RENTRATENEXTFLG) = drTab3Data("RENTRATENEXTFLG")  '賃率次期フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_RENTRATE) = drTab3Data("RENTRATE")                '賃率
        htPayFDataTbl(C_PAYFPARAM.PP_APPLYRATEAPPLKBN) = drTab3Data("APPLYRATEAPPLKBN")   '適用率適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_APPLYRATENEXTFLG) = drTab3Data("APPLYRATENEXTFLG")   '適用率次期フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_APPLYRATE) = drTab3Data("APPLYRATE")                 '適用率
        htPayFDataTbl(C_PAYFPARAM.PP_USEFEERATEAPPLKBN) = drTab3Data("USEFEERATEAPPLKBN") '使用料率適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_USEFEERATE) = drTab3Data("USEFEERATE")               '使用料率
        htPayFDataTbl(C_PAYFPARAM.PP_FREESENDRATEAPPLKBN) = drTab3Data("FREESENDRATEAPPLKBN")   '回送運賃適用率適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_FREESENDRATENEXTFLG) = drTab3Data("FREESENDRATENEXTFLG")   '回送運賃適用率次期フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_FREESENDRATE) = drTab3Data("FREESENDRATE")                 '回送運賃適用率
        htPayFDataTbl(C_PAYFPARAM.PP_SHIPFEEAPPLKBN) = drTab3Data("SHIPFEEAPPLKBN")       '発送料適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_SHIPFEENEXTFLG) = drTab3Data("SHIPFEENEXTFLG")       '発送料次期フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_TARIFFAPPLKBN) = drTab3Data("TARIFFAPPLKBN")         '使用料タリフ適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_OUTISLANDAPPLKBN) = drTab3Data("OUTISLANDAPPLKBN")   '離島向け適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_FREEAPPLKBN) = drTab3Data("FREEAPPLKBN")             '使用料無料特認 
        htPayFDataTbl(C_PAYFPARAM.PP_SPECIALM1APPLKBN) = drTab3Data("SPECIALM1APPLKBN")   '特例Ｍ１適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_SPECIALM2APPLKBN) = drTab3Data("SPECIALM2APPLKBN")   '特例Ｍ２適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_SPECIALM3APPLKBN) = drTab3Data("SPECIALM3APPLKBN")   '特例Ｍ３適用区分
        htPayFDataTbl(C_PAYFPARAM.PP_HOKKAIDOAPPLKBN) = drTab3Data("HOKKAIDOAPPLKBN")     '適用区分-北海道先方負担
        htPayFDataTbl(C_PAYFPARAM.PP_NIIGATAAPPLKBN) = drTab3Data("NIIGATAAPPLKBN")       '適用区分-新潟先方負担
        htPayFDataTbl(C_PAYFPARAM.PP_REFLECTFLG) = "0"                                    '反映フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_SKIPFLG) = "0"                                       '読み飛ばしフラグ
        htPayFDataTbl(C_PAYFPARAM.PP_DELFLG) = "0"                                        '削除フラグ
        htPayFDataTbl(C_PAYFPARAM.PP_INITYMD) = dtSysDateTime                             '登録年月日
        htPayFDataTbl(C_PAYFPARAM.PP_INITUSER) = Master.USERID                            '登録ユーザーＩＤ
        htPayFDataTbl(C_PAYFPARAM.PP_INITTERMID) = Master.USERTERMID                      '登録端末
        htPayFDataTbl(C_PAYFPARAM.PP_INITPGID) = Me.GetType().BaseType.Name               '登録プログラムＩＤ

        Return htPayFDataTbl

    End Function

    ''' <summary>
    ''' 受注明細データ存在チェック
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Private Function ChkOrderDetail(ByVal SQLcon As MySqlConnection, sqlTran As MySqlTransaction,
                                    ByVal strOrderNo As String, ByVal strSameDayCnt As String) As Boolean
        Dim sql As New StringBuilder
        Dim param As New Hashtable
        Dim dtOrderDetail As New DataTable

        'SQL作成
        With sql
            .AppendLine("SELECT")
            .AppendLine("    ORDERNO")
            .AppendLine("  , SAMEDAYCNT")
            .AppendLine("FROM")
            .AppendLine("    LNG.LNT0005_ORDERDATA ")
            .AppendLine("WHERE")
            .AppendLine("    ORDERNO = @ORDERNO")
            .AppendLine("    AND SAMEDAYCNT = @SAMEDAYCNT")
        End With

        'パラメータ設定
        With param
            .Add("@ORDERNO", strOrderNo)
            .Add("@SAMEDAYCNT", strSameDayCnt)
        End With

        'SQL発行
        CS0050SESSION.GetDataTable(SQLcon, sql.ToString, param, dtOrderDetail, sqlTran)

        If dtOrderDetail.Rows.Count = 0 Then
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' 受注データ存在チェック
    ''' </summary>
    ''' <param name="strPlanDepYMD">SQL接続</param>
    ''' <remarks></remarks>
    Private Function ChkOrderHead(ByVal strPlanDepYMD As String,
                                  ByVal strCtnType As String, ByVal strCtnNo As String) As Boolean

        Dim sql As New StringBuilder
        Dim param As New Hashtable
        Dim dtPayPlanF As New DataTable

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'SQL作成
            With sql
                .AppendLine("SELECT")
                .AppendLine("    ORDERNO")
                .AppendLine("FROM")
                .AppendLine("    LNG.LNT0004_ORDERHEAD ")
                .AppendLine("WHERE")
                .AppendLine("    PLANDEPYMD = @PLANDEPYMD")
                .AppendLine("    AND CTNTYPE = @CTNTYPE")
                .AppendLine("    AND CTNNO = @CTNNO")
            End With

            'パラメータ設定
            With param
                .Add("@PLANDEPYMD", strPlanDepYMD)
                .Add("@CTNTYPE", strCtnType)
                .Add("@CTNNO", strCtnNo)
            End With

            'SQL発行
            CS0050SESSION.GetDataTable(SQLcon, sql.ToString, param, dtPayPlanF)

            If dtPayPlanF.Rows.Count = 0 Then
                Return False
            End If

        End Using

        Return True

    End Function

#End Region

    ''' <summary>
    ''' (受注TBL)受注進行ステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatus(ByVal I_Value As String,
                                       Optional ByVal InitializeFlg As Boolean = False,
                                       Optional ByVal ReuseFlg As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの受注進行ステータスを更新
            Dim SQLStr As String =
                    " UPDATE LNG.LNT0004_ORDERHEAD " _
                    & "    SET STATUS = @P03, "

            SQLStr &=
                      "        UPDYMD      = @P11, " _
                    & "        UPDUSER     = @P12, " _
                    & "        UPDTERMID   = @P13, " _
                    & "        RECEIVEYMD  = @P14  " _
                    & "  WHERE ORDERNO     = @P01  " _
                    & "    AND DELFLG     <> @P02; "

            Dim SQLcmd As New MySqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)
            Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)
            Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)

            Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.DateTime)
            Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar)
            Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar)
            Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.DateTime)

            PARA01.Value = work.WF_SELROW_ORDERNO.Text
            PARA02.Value = C_DELETE_FLG.DELETE
            PARA03.Value = I_Value

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D_ORDERSTATUS UPDATE", needsPopUp:=True)
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D_ORDERSTATUS UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ 選択内容を取得
        '### LeftBoxマルチ対応(20200217) START #####################################################
        If leftview.ActiveViewIdx = 2 Then
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            WW_SelectText = selectedLeftTableVal("VALUE1")
            '### LeftBoxマルチ対応(20200217) END   #####################################################
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            '運用部署
            Case "WF_UORG"
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()

            '発送予定日
            Case "TxtPlanDepYMD"
                Dim WW_DATE As Date

                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtPlanDepYMD.Text = ""
                    Else
                        Me.TxtPlanDepYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                        '125キロ賃率取得処理
                        Call GetTinRetu125(Me.TxtPlanDepYMD.Text)
                        '端数取得処理
                        Call GetHassu(Me.TxtPlanDepYMD.Text)
                    End If

                Catch ex As Exception
                End Try
                Me.TxtPlanDepYMD.Focus()

            'コンテナ記号
            Case "TxtCtnType"
                Me.TxtCtnType.Text = WW_SelectValue
                Me.TxtCtnTypeCode.Text = WW_SelectText
                work.WF_SEL_CTNTYPECODE.Text = WW_SelectValue
                work.WF_SEL_CTNTYPENAME.Text = WW_SelectText

                'コンテナ記号、コンテナ番号両方が入力された場合
                If Me.TxtCtnTypeCode.Text <> "" AndAlso Me.TxtCtnNoCode.Text <> "" Then
                    'コンテナマスタ取得処理
                    Call GetCtnMst(Me.TxtCtnTypeCode.Text, Me.TxtCtnNoCode.Text)
                End If

                Me.TxtCtnType.Focus()

            'コンテナ番号
            Case "TxtCtnNo"
                Me.TxtCtnNo.Text = WW_SelectValue
                Me.TxtCtnNoCode.Text = WW_SelectText
                work.WF_SEL_CTNNOCODE.Text = WW_SelectValue
                work.WF_SEL_CTNNONAME.Text = WW_SelectText

                'コンテナ記号、コンテナ番号両方が入力された場合
                If Me.TxtCtnTypeCode.Text <> "" AndAlso Me.TxtCtnNoCode.Text <> "" Then
                    'コンテナマスタ取得処理
                    Call GetCtnMst(Me.TxtCtnTypeCode.Text, Me.TxtCtnNoCode.Text)
                End If

                Me.TxtCtnNo.Focus()

            'タブ「明細データ」(一覧)
            Case "ITEMCD", "DEPSTATION", "ARRSTATION", "RAILDEPSTATIONNM", "RAILARRSTATIONNM", "RAWDEPSTATIONNM", "RAWARRSTATIONNM",
                 "DEPTRUSTEECD", "ARRTRUSTEECD", "PLANARRYMD", "RESULTARRYMD",
                 "STACKFREEKBNCD", "SHIPPERCD"

                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '各タブにより設定を制御
                Select Case WF_DetailMView.ActiveViewIndex
                '◆明細データ
                    Case 0
                        '○ 画面表示データ復元
                        If Not Master.RecoverTable(LNT0001tbl) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = LNT0001tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '〇 一覧項目へ設定
                        '(一覧)品目名
                        If WF_FIELD.Value = "ITEMCD" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("ITEMNM") = WW_SETTEXT

                            '(一覧)発駅名
                        ElseIf WF_FIELD.Value = "DEPSTATION" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("DEPSTATIONNM") = WW_SETTEXT

                            '(一覧)着駅名
                        ElseIf WF_FIELD.Value = "ARRSTATION" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("ARRSTATIONNM") = WW_SETTEXT

                            '(一覧)鉄道発駅名
                        ElseIf WF_FIELD.Value = "RAILDEPSTATIONNM" Then
                            updHeader.Item("RAILDEPSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)鉄道着駅名
                        ElseIf WF_FIELD.Value = "RAILARRSTATIONNM" Then
                            updHeader.Item("RAILARRSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)原発駅名
                        ElseIf WF_FIELD.Value = "RAWDEPSTATIONNM" Then
                            updHeader.Item("RAWDEPSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)原着駅名
                        ElseIf WF_FIELD.Value = "RAWARRSTATIONNM" Then
                            updHeader.Item("RAWARRSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)発受託人
                        ElseIf WF_FIELD.Value = "DEPTRUSTEECD" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("DEPTRUSTEENM") = WW_SETTEXT

                            '(一覧)着受託人
                        ElseIf WF_FIELD.Value = "ARRTRUSTEECD" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("ARRTRUSTEENM") = WW_SETTEXT

                            '(一覧)積空区分
                        ElseIf WF_FIELD.Value = "STACKFREEKBNCD" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("STACKFREEKBNNM") = WW_SETTEXT

                            '(一覧)荷送人
                        ElseIf WF_FIELD.Value = "SHIPPERCD" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("SHIPPERNM") = WW_SETTEXT

                            '(一覧)到着予定日を一覧に設定
                        ElseIf WF_FIELD.Value = "PLANARRYMD" OrElse WF_FIELD.Value = "RESULTARRYMD" Then
                            Dim WW_DATE As Date
                            Try
                                Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                                If WW_DATE < C_DEFAULT_YMD Then
                                    updHeader.Item(WF_FIELD.Value) = ""
                                Else
                                    updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                                End If

                            Catch ex As Exception
                            End Try

                        End If

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(LNT0001tbl) Then Exit Sub

                End Select

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"               '会社コード
                Me.WF_CAMPCODE.Focus()
            Case "WF_UORG"                   '運用部署
                Me.WF_UORG.Focus()
            Case "TxtPlanDepYMD"            '(予定)積込日
                Me.TxtPlanDepYMD.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub WW_FixvalueMasterSearch(ByVal I_CODE As String,
                                          ByVal I_CLASS As String,
                                          ByVal I_KEYCODE As String,
                                          ByRef O_VALUE() As String,
                                          Optional ByVal I_PARA01 As String = Nothing)

        If IsNothing(LNT0001Fixvaltbl) Then
            LNT0001Fixvaltbl = New DataTable
        End If

        If LNT0001Fixvaltbl.Columns.Count <> 0 Then
            LNT0001Fixvaltbl.Columns.Clear()
        End If

        LNT0001Fixvaltbl.Clear()

        Try

            'DBより取得
            LNT0001Fixvaltbl = WW_FixvalueMasterDataGet(I_CODE, I_CLASS, I_KEYCODE, I_PARA01)

            If I_KEYCODE.Equals("") Then

                If IsNothing(I_PARA01) Then
                    'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                    For Each LNT0001WKrow As DataRow In LNT0001Fixvaltbl.Rows '(全抽出結果回るので要検討
                        'O_VALUE(i) = LNT0001WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = LNT0001WKrow("VALUE" & i.ToString())
                        Next
                        'i += 1 '2020/3/23 三宅 Delete
                    Next

                ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                    Dim i As Integer = 0
                    For Each LNT0001WKrow As DataRow In LNT0001Fixvaltbl.Rows
                        '### 20201030 START 積込日(予定)基準で油種の開始終了を制御 ################################################
                        'O_VALUE(i) = Convert.ToString(LNT0001WKrow("KEYCODE"))
                        'i += 1
                        Try
                            If LNT0001WKrow("STYMD") <= Date.Parse(Me.TxtPlanDepYMD.Text) _
                                AndAlso LNT0001WKrow("ENDYMD") >= Date.Parse(Me.TxtPlanDepYMD.Text) Then
                                O_VALUE(i) = Convert.ToString(LNT0001WKrow("KEYCODE")).Replace(Convert.ToString(LNT0001WKrow("VALUE2")), "")
                                i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                        '### 20201030 END   積込日(予定)基準で油種の開始終了を制御 ################################################
                    Next
                End If

            Else
                If IsNothing(I_PARA01) Then
                    For Each LNT0001WKrow As DataRow In LNT0001Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = LNT0001WKrow("VALUE" & i.ToString())
                        Next
                    Next
                ElseIf I_PARA01 = "1" Then
                    Dim i As Integer = 0
                    For Each LNT0001WKrow As DataRow In LNT0001Fixvaltbl.Rows
                        Try
                            If LNT0001WKrow("STYMD") <= Date.Parse(Me.TxtPlanDepYMD.Text) _
                                AndAlso LNT0001WKrow("ENDYMD") >= Date.Parse(Me.TxtPlanDepYMD.Text) Then
                                O_VALUE(0) = Convert.ToString(LNT0001WKrow("KEYCODE")).Replace(Convert.ToString(LNT0001WKrow("VALUE2")), "")
                                O_VALUE(1) = LNT0001WKrow("VALUE3")
                                O_VALUE(2) = LNT0001WKrow("VALUE2")
                                O_VALUE(3) = LNT0001WKrow("VALUE1")
                                'O_VALUE(i) = Convert.ToString(LNT0001WKrow("KEYCODE")).Replace(Convert.ToString(LNT0001WKrow("VALUE2")), "")
                                'i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                    Next
                End If
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D MASTER_SELECT", needsPopUp:=True)
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 125キロ賃率取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetTinRetu125(ByVal strPrmPlanDepYMD As String)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)    '発送年月日
            param.Add("@piVBFLG", CONST_VBFLG)           'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poReturnTunRentRate", "")
            param.Add("@poNextFlg", "")
            param.Add("@poDispNextFlg", "")

            Try
                Dim dtTinr As DataTable = Nothing
                '賃率取得２ ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_TINR2, param, dtTinr)
                '戻り値設定
                If dtTinr.Rows.Count > 0 Then
                    Me.TxtRentRate125.Text = dtTinr.Rows(0)("TUNRENTRATE").ToString
                    Me.TxtRentRate125Code.Text = dtTinr.Rows(0)("TUNRENTRATE").ToString
                    Me.TxtTunNext125.Text = dtTinr.Rows(0)("NEXTFLG").ToString
                    Me.TxtTunNext125Code.Text = dtTinr.Rows(0)("NEXTFLGCODE").ToString
                Else
                    Me.TxtRentRate125.Text = ""
                    Me.TxtRentRate125Code.Text = ""
                    Me.TxtTunNext125.Text = ""
                    Me.TxtTunNext125Code.Text = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_GET_TINR2_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("１２５キロ賃率取得エラー。", C_MESSAGE_NO.CTN_GET_TINR2_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 端数取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetHassu(ByVal strPrmPlanDepYMD As String)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)    '発送年月日
            param.Add("@piVBFLG", CONST_VBFLG)           'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poRetSetVal1", "")
            param.Add("@poRetSetVal2", "")
            param.Add("@poRetSetVal3", "")
            param.Add("@poNextFlg", "")
            param.Add("@poDispNextFlg", "")
            param.Add("@poRetSetVal2Nm", "")
            param.Add("@poRetSetVal3Nm", "")

            Try
                Dim dtHasuu As DataTable = Nothing
                '端数取得処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_HASUU, param, dtHasuu)
                '戻り値設定
                If dtHasuu.Rows.Count > 0 Then
                    Me.TxtRoundFee.Text = dtHasuu.Rows(0)("SETVAL1").ToString             '端数金額基準
                    Me.TxtRoundFeeCode.Text = dtHasuu.Rows(0)("SETVAL1").ToString
                    Me.TxtRoundKbnGECode.Text = dtHasuu.Rows(0)("SETVAL2").ToString       '端数区分金額以上
                    Me.TxtRoundKbnGE.Text = dtHasuu.Rows(0)("SETVAL2NM").ToString
                    Me.TxtRoundKbnLTCode.Text = dtHasuu.Rows(0)("SETVAL3").ToString       '端数区分金額未満
                    Me.TxtRoundKbnLT.Text = dtHasuu.Rows(0)("SETVAL3NM").ToString
                    Me.TxtRoundTunNextCode.Text = dtHasuu.Rows(0)("NEXTFLGCODE").ToString '現行／次期
                    Me.TxtRoundTunNext.Text = dtHasuu.Rows(0)("NEXTFLG").ToString
                Else
                    Me.TxtRoundFee.Text = ""
                    Me.TxtRoundFeeCode.Text = ""
                    Me.TxtRoundKbnGECode.Text = ""
                    Me.TxtRoundKbnGE.Text = ""
                    Me.TxtRoundKbnLTCode.Text = ""
                    Me.TxtRoundKbnLT.Text = ""
                    Me.TxtRoundTunNextCode.Text = ""
                    Me.TxtRoundTunNext.Text = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_GET_HASUU_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("端数取得エラー。", C_MESSAGE_NO.CTN_GET_HASUU_ERR)
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' コンテナマスタ取得処理(大分類、中分類の名称をセットする)
    ''' </summary>
    Private Sub GetCtnMst(ByVal strPrmCtnType As String, ByVal strPrmCtnNo As String)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piCTNTYPE", strPrmCtnType)   'コンテナ記号
            param.Add("@piCTNTNO", strPrmCtnNo)      'コンテナ番号
            param.Add("@piVBFLG", CONST_VBFLG)       'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poBIGCTNCD", "")
            param.Add("@poMIDDLECTNCD", "")
            param.Add("@poSMALLCTNCD", "")
            param.Add("@poBIGCTNNM", "")
            param.Add("@poMIDDLECTNNM", "")
            param.Add("@poSMALLCTNNM", "")
            param.Add("@poJURISDICTIONCD", "")
            param.Add("@poACCOUNTINGASSETSCD", "")
            param.Add("@poACCOUNTINGASSETSKBN", "")
            param.Add("@poDUMMYKBN", "")
            param.Add("@poSPOTKBN", "")
            param.Add("@poSPOTSTYMD", "")
            param.Add("@poSPOTENDYMD", "")
            param.Add("@poCOMPKANKBN", "")
            param.Add("@OPERATIONENDYMD", "")
            param.Add("@RETIRMENTYMD", "")

            Try
                Dim dtCtnm As DataTable = Nothing
                'コンテナマスタ取得 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_RECONM, param, dtCtnm)
                '戻り値設定
                If dtCtnm.Rows.Count > 0 Then
                    Me.TxtBigCtnCode.Text = dtCtnm.Rows(0)("BIGCTNCD").ToString        '大分類コード
                    Me.TxtBigCtnName.Text = dtCtnm.Rows(0)("BIGCTNNM").ToString        '大分類名称
                    Me.TxtMiddleCtnCode.Text = dtCtnm.Rows(0)("MIDDLECTNCD").ToString  '中分類コード
                    Me.TxtMiddleCtnName.Text = dtCtnm.Rows(0)("MIDDLECTNNM").ToString  '中分類名称
                    Me.TxtSmallCtnCode.Text = dtCtnm.Rows(0)("SMALLCTNCD").ToString    '小分類コード
                    Me.TxtSmallCtnName.Text = dtCtnm.Rows(0)("SMALLCTNNM").ToString    '小分類名称
                    Me.work.WF_UPD_JURISDICTIONCD.Text = dtCtnm.Rows(0)("JURISDICTIONCD").ToString　          '所管部コード
                    Me.work.WF_UPD_ACCOUNTINGASSETSCD.Text = dtCtnm.Rows(0)("ACCOUNTINGASSETSCD").ToString　  '経理資産コード
                    Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text = dtCtnm.Rows(0)("ACCOUNTINGASSETSKBN").ToString  '経理資産区分
                    Me.work.WF_UPD_DUMMYKBN.Text = dtCtnm.Rows(0)("DUMMYKBN").ToString　                      'ダミー区分
                    Me.work.WF_UPD_SPOTKBN.Text = dtCtnm.Rows(0)("SPOTKBN").ToString        'スポット区分
                    Me.work.WF_UPD_SPOTSTYMD.Text = dtCtnm.Rows(0)("SPOTSTYMD").ToString    'スポット区分　開始年月日
                    Me.work.WF_UPD_SPOTENDYMD.Text = dtCtnm.Rows(0)("SPOTENDYMD").ToString  'スポット区分　終了年月日
                    Me.work.WF_UPD_COMPKANKBN.Text = dtCtnm.Rows(0)("COMPKANKBN").ToString  '複合一貫区分
                    Me.work.WF_UPD_OPERATIONENDYMD.Text = dtCtnm.Rows(0)("OPERATIONENDYMD").ToString  '運用除外年月日
                    Me.work.WF_UPD_RETIRMENTYMD.Text = dtCtnm.Rows(0)("RETIRMENTYMD").ToString        '除却年月日
                    'スポット区分の判定
                    Dim strSpotKbn As String = "0"
                    If Val(Me.work.WF_UPD_SPOTKBN.Text) <> 0 Then
                        '発送年月日がスポット区分　開始年月日～終了年月日内の場合
                        If Val(Me.TxtPlanDepYMD.Text) >= Val(Me.work.WF_UPD_SPOTSTYMD.Text) _
                        AndAlso Val(Me.TxtPlanDepYMD.Text) <= Val(Me.work.WF_UPD_SPOTENDYMD.Text) Then
                            strSpotKbn = Me.work.WF_UPD_SPOTKBN.Text
                        End If
                    End If
                    Me.work.WF_UPD_SPOTKBN.Text = strSpotKbn
                Else
                    Me.TxtBigCtnCode.Text = ""                    '大分類コード
                    Me.TxtBigCtnName.Text = ""                    '大分類名称
                    Me.TxtMiddleCtnCode.Text = ""                 '中分類コード
                    Me.TxtMiddleCtnName.Text = ""                 '中分類名称
                    Me.TxtSmallCtnCode.Text = ""                  '小分類コード
                    Me.TxtSmallCtnName.Text = ""                  '小分類名称
                    Me.work.WF_UPD_JURISDICTIONCD.Text = ""       '所管部コード
                    Me.work.WF_UPD_ACCOUNTINGASSETSCD.Text = ""   '経理資産コード
                    Me.work.WF_UPD_ACCOUNTINGASSETSKBN.Text = ""  '経理資産区分
                    Me.work.WF_UPD_DUMMYKBN.Text = ""             'ダミー区分
                    Me.work.WF_UPD_SPOTKBN.Text = ""              'スポット区分
                    Me.work.WF_UPD_SPOTSTYMD.Text = ""            'スポット区分　開始年月日
                    Me.work.WF_UPD_SPOTENDYMD.Text = ""           'スポット区分　終了年月日
                    Me.work.WF_UPD_COMPKANKBN.Text = ""           '複合一貫区分
                    Me.work.WF_UPD_OPERATIONENDYMD.Text = ""      '運用除外年月日
                    Me.work.WF_UPD_RETIRMENTYMD.Text = ""         '除却年月日
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_GET_RECONM_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("コンテナマスタ取得エラー。", C_MESSAGE_NO.CTN_GET_RECONM_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' JOT店所コード取得処理(駅マスタ)
    ''' </summary>
    ''' <param name="strPrmStation">駅コード</param>
    Private Function GetJotBranchCd(ByVal SQLcon As MySqlConnection, ByVal strPrmStation As String,
                                    ByRef strRetORGCODE As String,
                                    ByRef strRetTAIOU1 As String, ByRef strRetTAIOU2 As String,
                                    Optional ByRef p_tran As MySqlTransaction = Nothing) As Boolean

        Dim procFlg As Boolean = False
        Dim strORGCODE As String = ""
        Dim strTAIOU1 As String = ""
        Dim strTAIOU2 As String = ""

        Dim param As New Dictionary(Of String, String)
        'パラメータ作成
        param.Add("@piCAMPCODE", work.WF_SEL_CAMPCODE.Text)   '会社コード
        param.Add("@piSTATION", strPrmStation)                '駅コード
        param.Add("@piVBFLG", CONST_VBFLG)                    'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poORGCODE", "")
        param.Add("@poTAIOU1", "")
        param.Add("@poTAIOU2", "")

        Try
            Dim dtTinr As DataTable = Nothing
            'JOT店所コード取得処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_JOTBRANCHCD, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                strORGCODE = dtTinr.Rows(0)("ORGCODE").ToString
                strTAIOU1 = dtTinr.Rows(0)("TAIOU1").ToString
                strTAIOU2 = dtTinr.Rows(0)("TAIOU2").ToString
            Else
                strORGCODE = ""
                strTAIOU1 = ""
                strTAIOU2 = ""
            End If

            '返却
            strRetORGCODE = strORGCODE
            strRetTAIOU1 = strTAIOU1
            strRetTAIOU2 = strTAIOU2

            procFlg = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_GET_JOTBRANCHCD_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("ＪＯＴ発店所コード取得エラー。", C_MESSAGE_NO.CTN_GET_JOTBRANCHCD_ERR)
        End Try

        '返却
        Return procFlg

    End Function

    ''' <summary>
    ''' レンタルシステム用リース物件マスタ取得処理
    ''' </summary>
    ''' <param name="strPrmCtnType">コンテナ記号</param>
    ''' <param name="strPrmCtnNo">コンテナ番号</param>
    Private Function GetLamasm(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                    ByVal strPrmCtnType As String, ByVal strPrmCtnNo As String) As Hashtable

        Dim htLamasm As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piCTNTYPE", strPrmCtnType)  'コンテナ記号
        param.Add("@piCTNNO", strPrmCtnNo)      'コンテナ番号
        param.Add("@piVBFLG", CONST_VBFLG)      'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poPRODUCTCD", "")
        param.Add("@poINVOICECAMPCD", "")
        param.Add("@poINVOICEDEPTCD", "")
        param.Add("@poINVFILINGDEPT", "")
        param.Add("@poKEIJYOBRANCHCD", "")

        Try
            Dim dtTinr As DataTable = Nothing
            'レンタルシステム用リース物件マスタ取得処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_LAMASM, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                htLamasm("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                htLamasm("PRODUCTCD") = dtTinr.Rows(0)("PRODUCTCD").ToString
                htLamasm("INVOICECAMPCD") = dtTinr.Rows(0)("INVOICECAMPCD").ToString
                htLamasm("INVOICEDEPTCD") = dtTinr.Rows(0)("INVOICEDEPTCD").ToString
                htLamasm("INVFILINGDEPT") = dtTinr.Rows(0)("INVFILINGDEPT").ToString
                htLamasm("KEIJYOBRANCHCD") = dtTinr.Rows(0)("KEIJYOBRANCHCD").ToString
            Else
                htLamasm("MSTFLG") = ""
                htLamasm("PRODUCTCD") = ""
                htLamasm("INVOICECAMPCD") = ""
                htLamasm("INVOICEDEPTCD") = ""
                htLamasm("INVFILINGDEPT") = ""
                htLamasm("KEIJYOBRANCHCD") = ""
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_GET_LAMASM_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("レンタルシステム用リース物件マスタ取得エラー。", C_MESSAGE_NO.CTN_GET_LAMASM_ERR)
        End Try

        '返却
        Return htLamasm

    End Function

    ''' <summary>
    ''' GAL部門新旧変換マスタ取得処理
    ''' </summary>
    ''' <param name="strPrmNOOCDN">部門コード</param>
    Private Function GetEgmnoo(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                    ByVal strPrmNOOCDN As String) As Hashtable

        Dim htLamasm As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piNOOCDN", strPrmNOOCDN)  '部門コード
        param.Add("@piVBFLG", CONST_VBFLG)   'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poNOOCDO", "")
        param.Add("@poNOOC2O", "")

        Try
            Dim dtTinr As DataTable = Nothing
            'GAL部門新旧変換マスタ取得処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_EGMNOO, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                htLamasm("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                htLamasm("NOOCDO") = dtTinr.Rows(0)("NOOCDO").ToString
                htLamasm("NOOC2O") = dtTinr.Rows(0)("NOOC2O").ToString
            Else
                htLamasm("MSTFLG") = ""
                htLamasm("NOOCDO") = ""
                htLamasm("NOOC2O") = ""
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_GET_EGMNOO_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("GAL部門新旧変換マスタ取得エラー。", C_MESSAGE_NO.CTN_GET_EGMNOO_ERR)
        End Try

        '返却
        Return htLamasm

    End Function

    ''' <summary>
    ''' コンテナ決済マスタ取得処理
    ''' </summary>
    ''' <param name="strPrmDepStation">発駅コード</param>
    ''' <param name="strPrmDepTrusteeCd">発受託人コード</param>
    ''' <param name="strPrmDepTrusteeSubCd">発受託人サブコード</param>
    Private Function GetRekejm(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                   ByVal strPrmDepStation As String,
                                   ByVal strPrmDepTrusteeCd As String, ByVal strPrmDepTrusteeSubCd As String) As Hashtable

        Dim htRekejm As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piDEPSTATION", strPrmDepStation)            '発駅コード
        param.Add("@piDEPTRUSTEECD", strPrmDepTrusteeCd)        '発受託人コード
        param.Add("@piDEPTRUSTEESUBCD", strPrmDepTrusteeSubCd)  '発受託人サブコード
        param.Add("@piVBFLG", CONST_VBFLG)   'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poPARTNERCAMPCD", "")
        param.Add("@poPARTNERDEPTCD", "")
        param.Add("@poINVKEIJYOBRANCHCD", "")
        param.Add("@poINVFILINGDEPT", "")
        param.Add("@poINVKESAIKBN", "")
        param.Add("@poINVSUBCD", "")
        param.Add("@poPAYKEIJYOBRANCHCD", "")
        param.Add("@poPAYFILINGBRANCH", "")
        param.Add("@poTAXCALCUNIT", "")

        Try
            Dim dtTinr As DataTable = Nothing
            'コンテナ決済マスタ取得処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_REKEJM, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                htRekejm("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                htRekejm("PARTNERCAMPCD") = dtTinr.Rows(0)("PARTNERCAMPCD").ToString
                htRekejm("PARTNERDEPTCD") = dtTinr.Rows(0)("PARTNERDEPTCD").ToString
                htRekejm("INVKEIJYOBRANCHCD") = dtTinr.Rows(0)("INVKEIJYOBRANCHCD").ToString
                htRekejm("INVFILINGDEPT") = dtTinr.Rows(0)("INVFILINGDEPT").ToString
                htRekejm("INVKESAIKBN") = dtTinr.Rows(0)("INVKESAIKBN").ToString
                htRekejm("INVSUBCD") = dtTinr.Rows(0)("INVSUBCD").ToString
                htRekejm("PAYKEIJYOBRANCHCD") = dtTinr.Rows(0)("PAYKEIJYOBRANCHCD").ToString
                htRekejm("PAYFILINGBRANCH") = dtTinr.Rows(0)("PAYFILINGBRANCH").ToString
                htRekejm("TAXCALCUNIT") = dtTinr.Rows(0)("TAXCALCUNIT").ToString
            Else
                htRekejm("MSTFLG") = ""
                htRekejm("PARTNERCAMPCD") = ""
                htRekejm("PARTNERDEPTCD") = ""
                htRekejm("INVKEIJYOBRANCHCD") = ""
                htRekejm("INVFILINGDEPT") = ""
                htRekejm("INVKESAIKBN") = ""
                htRekejm("INVSUBCD") = ""
                htRekejm("PAYKEIJYOBRANCHCD") = ""
                htRekejm("PAYFILINGBRANCH") = ""
                htRekejm("TAXCALCUNIT") = ""
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_GET_REKEJM_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("コンテナ決済マスタ取得エラー。", C_MESSAGE_NO.CTN_GET_REKEJM_ERR)
        End Try

        '返却
        Return htRekejm

    End Function

    ''' <summary>
    ''' コード変換特例処理
    ''' </summary>
    ''' <param name="strPrmOrderNo">受注No</param>
    ''' <param name="strPrmSameDayCnt">同日内回数</param>
    ''' <param name="strPrmUpdDate">更新日時</param>
    ''' <param name="strPrmUpdUser">更新ユーザ</param>
    ''' <param name="strPrmUpdTermId">更新端末</param>
    Private Function UpdCodeChange(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                   ByVal strPrmOrderNo As String, ByVal strPrmSameDayCnt As String,
                                   ByVal strPrmUpdDate As String, ByVal strPrmUpdUser As Date, ByVal strPrmUpdTermId As String) As Hashtable

        Dim htCodeChange As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piORDERNO", strPrmOrderNo)          'オーダNo
        param.Add("@piSAMEDAYCNT", strPrmSameDayCnt)    '同日内回数
        param.Add("@piUPDDATE", strPrmUpdDate)          '更新日時
        param.Add("@piUPDUSER", strPrmUpdUser)          '更新ユーザ
        param.Add("@piUPDTERMID", strPrmUpdTermId)      '更新端末
        param.Add("@piVBFLG", CONST_VBFLG)   'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poWKUPFG", "")

        Try
            Dim dtTinr As DataTable = Nothing
            'コード変換特例処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_UPD_CODECHANGE, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                htCodeChange("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                htCodeChange("WKUPFG") = dtTinr.Rows(0)("WKUPFG").ToString
            Else
                htCodeChange("MSTFLG") = ""
                htCodeChange("WKUPFG") = ""
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_UPD_CODECHANGE_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("コード変換特例処理エラー。", C_MESSAGE_NO.CTN_UPD_CODECHANGE_ERR)
        End Try

        '返却
        Return htCodeChange

    End Function


    ''' <summary>
    ''' 計算屯数、割引番号、割増番号取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetTonsu(ByVal strPrmPlanDepYMD As String,
                         ByVal strPrmBIGCTNCD As String, ByVal strPrmMIDDLECTNCD As String, ByVal strSTACKFREEKBN As String,
                         ByRef drMeisaiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)         '発送年月日
            param.Add("@piBIGCTNCD", strPrmBIGCTNCD)          '大分類コード
            param.Add("@piMIDDLECTNCD", strPrmMIDDLECTNCD)    '中分類コード
            param.Add("@piSTACKFREEKBN", strSTACKFREEKBN)     '積空区分
            param.Add("@piVBFLG", CONST_VBFLG)                'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poMSTFLGNM", "")
            param.Add("@poReturnFARECALCTUN", "0.0")
            param.Add("@poReturnDISNO", "")
            param.Add("@poReturnEXTNO", "")
            param.Add("@poNextFlg", "")
            param.Add("@poDispNextFlg", "")

            Try
                Dim dtTonsu As DataTable = Nothing
                '計算屯数、割引番号、割増番号取得 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_TONSU, param, dtTonsu)
                '戻り値設定
                If dtTonsu.Rows.Count > 0 Then
                    drMeisaiData("FARECALCTUNAPPLKBN") = dtTonsu.Rows(0)("MSTFLG").ToString
                    drMeisaiData("FARECALCTUNAPPLKBNNM") = dtTonsu.Rows(0)("MSTFLGNM").ToString
                    drMeisaiData("FARECALCTUN") = dtTonsu.Rows(0)("FARECALCTUN").ToString
                    drMeisaiData("DISNO") = dtTonsu.Rows(0)("DISNO").ToString
                    drMeisaiData("EXTNO") = dtTonsu.Rows(0)("EXTNO").ToString
                    drMeisaiData("FARECALCTUNNEXTFLG") = dtTonsu.Rows(0)("NEXTFLGCODE").ToString
                    drMeisaiData("FARECALCTUNDISPNEXTFLG") = dtTonsu.Rows(0)("NEXTFLG").ToString
                Else
                    drMeisaiData("FARECALCTUNAPPLKBN") = ""
                    drMeisaiData("FARECALCTUNAPPLKBNNM") = ""
                    drMeisaiData("FARECALCTUN") = ""
                    drMeisaiData("DISNO") = ""
                    drMeisaiData("EXTNO") = ""
                    drMeisaiData("FARECALCTUNNEXTFLG") = ""
                    drMeisaiData("FARECALCTUNDISPNEXTFLG") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_TONSU_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("計算屯数、割引番号、割増番号取得処理エラー。", C_MESSAGE_NO.CTN_TONSU_ERR)
            End Try

        End Using

    End Sub


    ''' <summary>
    ''' 税率取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Function GetZerit(ByVal SQLcon As MySqlConnection, ByRef p_tran As MySqlTransaction,
                                    ByVal strPrmPlanDepYMD As String) As Hashtable

        Dim htZerit As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piCTNNO", strPrmPlanDepYMD) '発送年月日
        param.Add("@piVBFLG", CONST_VBFLG)      'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poRetSetVal1", "")
        param.Add("@poRetSetVal2", "")
        param.Add("@poRetSetVal3", "")
        param.Add("@poNextFlg", "")
        param.Add("@poDispNextFlg", "")
        param.Add("@poDispNextFlg2", "")
        param.Add("@poDispNextFlg3", "")

        Try
            Dim dtTinr As DataTable = Nothing
            '税率取得処理 ストアド実行
            CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_ZERIT, param, dtTinr, p_tran)
            '戻り値設定
            If dtTinr.Rows.Count > 0 Then
                htZerit("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                htZerit("SETVAL1") = dtTinr.Rows(0)("SETVAL1").ToString
                htZerit("SETVAL2") = dtTinr.Rows(0)("SETVAL2").ToString
                htZerit("SETVAL3") = dtTinr.Rows(0)("SETVAL3").ToString
                htZerit("NEXTFLGCODE") = dtTinr.Rows(0)("NEXTFLGCODE").ToString
                htZerit("NEXTFLG") = dtTinr.Rows(0)("NEXTFLG").ToString
                htZerit("SETVAL2NM") = dtTinr.Rows(0)("SETVAL2NM").ToString
                htZerit("SETVAL3NM") = dtTinr.Rows(0)("SETVAL3NM").ToString
            Else
                htZerit("MSTFLG") = ""
                htZerit("SETVAL1") = ""
                htZerit("SETVAL2") = ""
                htZerit("SETVAL3") = ""
                htZerit("NEXTFLGCODE") = ""
                htZerit("NEXTFLG") = ""
                htZerit("SETVAL2NM") = ""
                htZerit("SETVAL3NM") = ""
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_ZERIT_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WW_CheckERR("税率取得エラー。", C_MESSAGE_NO.CTN_ZERIT_ERR)
        End Try

        '返却
        Return htZerit

    End Function

    ''' <summary>
    ''' キロ程取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetKiro(ByVal strPrmPlanDepYMD As String,
                         ByVal strPrmDEPSTATION As String, ByVal strPrmARRSTATION As String,
                         ByRef drMeisaiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)         '発送年月日
            param.Add("@piBIGCTNCD", strPrmDEPSTATION)        '発駅コード
            param.Add("@piMIDDLECTNCD", strPrmARRSTATION)     '着駅コード
            param.Add("@piVBFLG", CONST_VBFLG)                'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poMSTFLGNM", "")
            param.Add("@poReturnKIRO", "0.0")

            Try
                Dim dtData As DataTable = Nothing
                'キロ程取得処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_KIRO, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    drMeisaiData("KIROAPPLKBN") = dtData.Rows(0)("MSTFLG").ToString
                    drMeisaiData("KIROAPPLKBNNM") = dtData.Rows(0)("MSTFLGNM").ToString
                    drMeisaiData("KIRO") = dtData.Rows(0)("KIRO").ToString
                Else
                    drMeisaiData("KIROAPPLKBN") = ""
                    drMeisaiData("KIROAPPLKBNNM") = ""
                    drMeisaiData("KIRO") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_KIRO_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("キロ程取得処理エラー。", C_MESSAGE_NO.CTN_KIRO_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 賃率取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetTinrt(ByVal strPrmPlanDepYMD As String, ByVal strPrmKIRO As String,
                         ByRef drMeisaiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)     '発送年月日
            param.Add("@piKIRO", strPrmKIRO)              'キロ程
            param.Add("@piVBFLG", CONST_VBFLG)            'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poMSTFLGNM", "")
            param.Add("@poReturnTUNRENTRATE", "")
            param.Add("@poNextFlg", "")
            param.Add("@poDispNextFlg", "")

            Try
                Dim dtData As DataTable = Nothing
                '賃率取得処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_TINRT, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    drMeisaiData("RENTRATEAPPLKBN") = dtData.Rows(0)("MSTFLG").ToString
                    drMeisaiData("RENTRATEAPPLKBNNM") = dtData.Rows(0)("MSTFLGNM").ToString
                    drMeisaiData("RENTRATE") = dtData.Rows(0)("TUNRENTRATE").ToString
                    drMeisaiData("RENTRATENEXTFLG") = dtData.Rows(0)("NEXTFLGCODE").ToString
                    drMeisaiData("RENTRATEDISPNEXTFLG") = dtData.Rows(0)("NEXTFLG").ToString
                Else
                    drMeisaiData("RENTRATEAPPLKBN") = ""
                    drMeisaiData("RENTRATEAPPLKBNNM") = ""
                    drMeisaiData("RENTRATE") = ""
                    drMeisaiData("RENTRATENEXTFLG") = ""
                    drMeisaiData("RENTRATEDISPNEXTFLG") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_TINRT_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("賃率取得処理エラー。", C_MESSAGE_NO.CTN_TINRT_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 適用率取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GetTekrt(ByVal strPrmPlanDepYMD As String, ByVal strPrmEXTNO As String,
                         ByRef drMeisaiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)     '発送年月日
            param.Add("@piKIRO", strPrmEXTNO)             '割引番号
            param.Add("@piVBFLG", CONST_VBFLG)            'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poMSTFLG", "")
            param.Add("@poMSTFLGNM", "")
            param.Add("@poReturnAPPLYRATE", "0.0000")
            param.Add("@poNextFlg", "")
            param.Add("@poDispNextFlg", "")

            Try
                Dim dtData As DataTable = Nothing
                '適用率取得処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_TEKRT, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    drMeisaiData("APPLYRATEAPPLKBN") = dtData.Rows(0)("MSTFLG").ToString
                    drMeisaiData("APPLYRATEAPPLKBNNM") = dtData.Rows(0)("MSTFLGNM").ToString
                    drMeisaiData("APPLYRATE") = dtData.Rows(0)("APPLYRATE").ToString
                    drMeisaiData("APPLYRATENEXTFLG") = dtData.Rows(0)("NEXTFLGCODE").ToString
                    drMeisaiData("APPLYRATEDISPNEXTFLG") = dtData.Rows(0)("NEXTFLG").ToString
                Else
                    drMeisaiData("APPLYRATEAPPLKBN") = ""
                    drMeisaiData("APPLYRATEAPPLKBNNM") = ""
                    drMeisaiData("APPLYRATE") = ""
                    drMeisaiData("APPLYRATENEXTFLG") = ""
                    drMeisaiData("APPLYRATEDISPNEXTFLG") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_TEKRT_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("適用率取得処理エラー。", C_MESSAGE_NO.CTN_TEKRT_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 基本料金計算処理
    ''' </summary>
    ''' <param name="strPrmTum">運賃計算屯数</param>
    Private Sub GET_KYOT(ByVal strSTACKFREEKBNCD As String,
                         ByVal strPrmTum As String, ByVal strPrmRentRate As String, ByVal strPrmApplyRate As String,
                         ByVal strPrmRoundFee As String, ByVal strPrmRoundKbnGE As String, ByVal strPrmRoundKbnLT As String,
                         ByRef drMeisaiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piFARECALCTUN", strPrmTum)        '運賃計算屯数
            param.Add("@piRENTRATE", strPrmRentRate)      '賃率
            param.Add("@piAPPLYRATE", strPrmApplyRate)    '適用率
            param.Add("@piROUNDFEE", strPrmRoundFee)      '端数金額基準
            param.Add("@piROUNDKBNGE", strPrmRoundKbnGE)  '端数区分金額以上
            param.Add("@piROUNDKBNLT", strPrmRoundKbnLT)  '端数区分金額未満
            param.Add("@piVBFLG", CONST_VBFLG)            'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poJRFIXEDFARE", "")
            param.Add("@poJRFIXEDFARE_KAI", "")
            param.Add("@poOWNDISCOUNTFEE", "")

            Try
                Dim dtData As DataTable = Nothing
                '基本料金計算処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_KYOT, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    If strSTACKFREEKBNCD = "1" Then
                        drMeisaiData("JRFIXEDFARE") = dtData.Rows(0)("JRFIXEDFARE").ToString
                    ElseIf strSTACKFREEKBNCD = "2" Then
                        drMeisaiData("JRFIXEDFARE") = dtData.Rows(0)("JRFIXEDFARE_KAI").ToString
                    End If

                    drMeisaiData("OWNDISCOUNTFEE") = dtData.Rows(0)("OWNDISCOUNTFEE").ToString
                Else
                    drMeisaiData("JRFIXEDFARE") = ""
                    drMeisaiData("OWNDISCOUNTFEE") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_KYOT_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("基本料金計算処理エラー。", C_MESSAGE_NO.CTN_KYOT_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 使用料金計算処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GET_SHIY(ByVal strPrmPlanDepYMD As String, ByVal strPrmBIGCTNCD As String, ByVal strPrmMIDDLECTNCD As String,
                         ByVal strPrmKiro As String,
                         ByVal strPrmDEPSTATION As String, ByVal strDEPTRUSTEECD As String, ByVal strDEPTRUSTEESUBCD As String,
                         ByVal strPrmARRSTATION As String, ByVal strARRTRUSTEECD As String, ByVal strARRTRUSTEESUBCD As String,
                         ByVal strPrmJOTDEPBRANCHCD As String, ByVal strPrmJOTARRBRANCHCD As String,
                         ByVal strPrmDepTAIOU2 As String, ByVal strPrmArrTAIOU2 As String,
                         ByVal strPrmCTNTYPE As String, ByVal strPrmCTNNO As String,
                         ByVal strPrmDEPSHIPPERCD As String, ByVal strPrmJRITEMCD As String,
                         ByVal strPrmJRFIXEDFARE As String, ByVal strPrmOWNDISCOUNTFEE As String, ByVal strPrmOTHER1FEE As String,
                         ByVal strPrmRENTRATE125 As String, ByVal strPrmRENTRATE As String,
                         ByRef drMeisaiData As DataRow, ByRef drHanteiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)             '発送年月日
            param.Add("@piBIGCTNCD", strPrmBIGCTNCD)              '大分類コード
            param.Add("@piMIDDLECTNCD", strPrmMIDDLECTNCD)        '中分類コード
            param.Add("@piKIRO", strPrmKiro)                      'キロ程
            param.Add("@piDEPSTATION", strPrmDEPSTATION)          '発駅コード
            param.Add("@piDEPTRUSTEECD", strDEPTRUSTEECD)         '発受託人コード
            param.Add("@piDEPTRUSTEESUBCD", strDEPTRUSTEESUBCD)   '発受託人サブコード
            param.Add("@piARRSTATION", strPrmARRSTATION)          '着駅コード
            param.Add("@piARRTRUSTEECD", strARRTRUSTEECD)         '着受託人コード
            param.Add("@piARRTRUSTEESUBCD", strARRTRUSTEESUBCD)   '着受託人サブコード
            param.Add("@piJOTDEPBRANCHCD", strPrmJOTDEPBRANCHCD)  'ＪＯＴ発組織コード
            param.Add("@piJOTARRBRANCHCD", strPrmJOTARRBRANCHCD)  'ＪＯＴ着組織コード
            param.Add("@piDEPTAIOU2", strPrmDepTAIOU2)            'ＪＯＴ発店所コード(対応C2)
            param.Add("@piARRTAIOU2", strPrmArrTAIOU2)            'ＪＯＴ着店所コード(対応C2)
            param.Add("@piCTNTYPE", strPrmCTNTYPE)                'コンテナ記号
            param.Add("@piCTNNO", strPrmCTNNO)                    'コンテナ番号
            param.Add("@piDEPSHIPPERCD", strPrmDEPSHIPPERCD)      '発荷主コード
            param.Add("@piJRITEMCD", strPrmJRITEMCD)              'ＪＲ品目コード
            param.Add("@piJRFIXEDFARE", strPrmJRFIXEDFARE)        'ＪＲ所定運賃
            param.Add("@piOWNDISCOUNTFEE", strPrmOWNDISCOUNTFEE)  '私有割引相当額
            param.Add("@piOTHER1FEE", strPrmOTHER1FEE)            'その他１
            param.Add("@piRENTRATE125", strPrmRENTRATE125)        '125キロの賃率
            param.Add("@piRENTRATE", strPrmRENTRATE)              '賃率
            param.Add("@piVBFLG", CONST_VBFLG)                    'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poJRFIXEDFARE", "")
            param.Add("@poOWNDISCOUNTFEE", "")
            param.Add("@poUSEFEE", "")
            param.Add("@poRETURNFARE", "")
            param.Add("@poNITTSUFREESENDFEE", "")
            param.Add("@poMANAGEFEE", "")
            param.Add("@poSHIPBURDENFEE", "")
            param.Add("@poSHIPFEE", "")
            param.Add("@poARRIVEFEE", "")
            param.Add("@poPICKUPFEE", "")
            param.Add("@poDELIVERYFEE", "")
            param.Add("@poOTHER1", "")
            param.Add("@poOTHER2", "")
            param.Add("@poUSEFEERATE", "0.0")
            param.Add("@poURMSTFLG", "")
            param.Add("@poURMSTFLGNM", "")
            param.Add("@poSPRFITKBN", "")
            param.Add("@poSPECIALM1APPLKBN", "")
            param.Add("@poSPECIALM1APPLKBNNM", "")
            param.Add("@poSPECIALM2APPLKBN", "")
            param.Add("@poSPECIALM2APPLKBNNM", "")
            param.Add("@poTARIFFAPPLKBN", "")
            param.Add("@poTARIFFAPPLKBNNM", "")
            param.Add("@poHOKKAIDOAPPLKBN", "")
            param.Add("@poHOKKAIDOAPPLKBNNM", "")
            param.Add("@poNIIGATAAPPLKBN", "")
            param.Add("@poNIIGATAAPPLKBNNM", "")
            param.Add("@poFREEAPPLKBN", "")
            param.Add("@poFREEAPPLKBNNM", "")
            param.Add("@poISLANDAPPLKBN", "")
            param.Add("@poISLANDAPPLKBNNM", "")

            Try
                Dim dtData As DataTable = Nothing
                '使用料金計算処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_SHIY, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    drMeisaiData("JRFIXEDFARE") = dtData.Rows(0)("JRFIXEDFARE").ToString
                    drMeisaiData("OWNDISCOUNTFEE") = dtData.Rows(0)("OWNDISCOUNTFEE").ToString
                    drMeisaiData("USEFEE") = dtData.Rows(0)("USEFEE").ToString
                    drMeisaiData("RETURNFARE") = dtData.Rows(0)("RETURNFARE").ToString
                    drMeisaiData("NITTSUFREESENDFEE") = dtData.Rows(0)("NITTSUFREESENDFEE").ToString
                    drMeisaiData("MANAGEFEE") = dtData.Rows(0)("MANAGEFEE").ToString
                    drMeisaiData("SHIPBURDENFEE") = dtData.Rows(0)("SHIPBURDENFEE").ToString
                    drMeisaiData("SHIPFEE") = dtData.Rows(0)("SHIPFEE").ToString
                    drMeisaiData("ARRIVEFEE") = dtData.Rows(0)("ARRIVEFEE").ToString
                    drMeisaiData("PICKUPFEE") = dtData.Rows(0)("PICKUPFEE").ToString
                    drMeisaiData("DELIVERYFEE") = dtData.Rows(0)("DELIVERYFEE").ToString
                    drMeisaiData("OTHER1FEE") = dtData.Rows(0)("OTHER1").ToString
                    drMeisaiData("OTHER2FEE") = dtData.Rows(0)("OTHER2").ToString
                    drHanteiData("USEFEERATEAPPLKBN") = dtData.Rows(0)("URMSTFLG").ToString
                    drHanteiData("USEFEERATEAPPLKBNNM") = dtData.Rows(0)("URMSTFLGNM").ToString
                    drHanteiData("USEFEERATE") = dtData.Rows(0)("USEFEERATE").ToString
                    drHanteiData("TARIFFAPPLKBN") = dtData.Rows(0)("TARIFFAPPLKBN").ToString
                    drHanteiData("TARIFFAPPLKBNNM") = dtData.Rows(0)("TARIFFAPPLKBNNM").ToString
                    drHanteiData("SPECIALM1APPLKBN") = dtData.Rows(0)("SPECIALM1APPLKBN").ToString
                    drHanteiData("SPECIALM1APPLKBNNM") = dtData.Rows(0)("SPECIALM1APPLKBNNM").ToString
                    drHanteiData("SPECIALM2APPLKBN") = dtData.Rows(0)("SPECIALM2APPLKBN").ToString
                    drHanteiData("SPECIALM2APPLKBNNM") = dtData.Rows(0)("SPECIALM2APPLKBNNM").ToString
                    drHanteiData("HOKKAIDOAPPLKBN") = dtData.Rows(0)("HOKKAIDOAPPLKBN").ToString
                    drHanteiData("HOKKAIDOAPPLKBNNM") = dtData.Rows(0)("HOKKAIDOAPPLKBNNM").ToString
                    drHanteiData("NIIGATAAPPLKBN") = dtData.Rows(0)("NIIGATAAPPLKBN").ToString
                    drHanteiData("NIIGATAAPPLKBNNM") = dtData.Rows(0)("NIIGATAAPPLKBNNM").ToString
                    drHanteiData("FREEAPPLKBN") = dtData.Rows(0)("FREEAPPLKBN").ToString
                    drHanteiData("FREEAPPLKBNNM") = dtData.Rows(0)("FREEAPPLKBNNM").ToString
                    drHanteiData("OUTISLANDAPPLKBN") = dtData.Rows(0)("ISLANDAPPLKBN").ToString
                    drHanteiData("OUTISLANDAPPLKBNNM") = dtData.Rows(0)("ISLANDAPPLKBNNM").ToString
                Else
                    drMeisaiData("JRFIXEDFARE") = ""
                    drMeisaiData("OWNDISCOUNTFEE") = ""
                    drMeisaiData("USEFEE") = ""
                    drMeisaiData("RETURNFARE") = ""
                    drMeisaiData("NITTSUFREESENDFEE") = ""
                    drMeisaiData("MANAGEFEE") = ""
                    drMeisaiData("SHIPBURDENFEE") = ""
                    drMeisaiData("SHIPFEE") = ""
                    drMeisaiData("ARRIVEFEE") = ""
                    drMeisaiData("PICKUPFEE") = ""
                    drMeisaiData("DELIVERYFEE") = ""
                    drMeisaiData("OTHER1FEE") = ""
                    drMeisaiData("OTHER2FEE") = ""
                    drHanteiData("USEFEERATEAPPLKBN") = ""
                    drHanteiData("USEFEERATEAPPLKBNNM") = ""
                    drHanteiData("USEFEERATE") = ""
                    drHanteiData("TARIFFAPPLKBN") = ""
                    drHanteiData("TARIFFAPPLKBNNM") = ""
                    drHanteiData("SPECIALM1APPLKBN") = ""
                    drHanteiData("SPECIALM1APPLKBNNM") = ""
                    drHanteiData("SPECIALM2APPLKBN") = ""
                    drHanteiData("SPECIALM2APPLKBNNM") = ""
                    drHanteiData("HOKKAIDOAPPLKBN") = ""
                    drHanteiData("HOKKAIDOAPPLKBNNM") = ""
                    drHanteiData("NIIGATAAPPLKBN") = ""
                    drHanteiData("NIIGATAAPPLKBNNM") = ""
                    drHanteiData("FREEAPPLKBN") = ""
                    drHanteiData("FREEAPPLKBNNM") = ""
                    drHanteiData("OUTISLANDAPPLKBN") = ""
                    drHanteiData("OUTISLANDAPPLKBNNM") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_SHIY_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("使用料金計算処理エラー。", C_MESSAGE_NO.CTN_SHIY_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 回送費計算処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Sub GET_KAIS(ByVal strPrmPlanDepYMD As String, ByVal strPrmBIGCTNCD As String, ByVal strPrmMIDDLECTNCD As String,
                         ByVal strPrmDEPSTATION As String, ByVal strDEPTRUSTEECD As String, ByVal strDEPTRUSTEESUBCD As String,
                         ByVal strPrmARRSTATION As String, ByVal strARRTRUSTEECD As String, ByVal strARRTRUSTEESUBCD As String,
                         ByVal strPrmDepTAIOU1 As String, ByVal strPrmDepTAIOU2 As String, ByVal strPrmArrTAIOU2 As String,
                         ByVal strPrmCTNTYPE As String, ByVal strPrmCTNNO As String,
                         ByVal strPrmKEIYAKUCD As String,
                         ByVal strPrmJRFIXEDFARE As String, ByVal strPrmOTHER1FEE As String,
                         ByVal strPrmJOTDEPBRANCHCD As String, ByVal strPrmJOTARRBRANCHCD As String,
                         ByRef drMeisaiData As DataRow, ByRef drHanteiData As DataRow)

        'DB接続
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim param As New Dictionary(Of String, String)
            'パラメータ作成
            param.Add("@piSHIPYMD", strPrmPlanDepYMD)             '発送年月日
            param.Add("@piBIGCTNCD", strPrmBIGCTNCD)              '大分類コード
            param.Add("@piMIDDLECTNCD", strPrmMIDDLECTNCD)        '中分類コード
            param.Add("@piDEPSTATION", strPrmDEPSTATION)          '発駅コード
            param.Add("@piDEPTRUSTEECD", strDEPTRUSTEECD)         '発受託人コード
            param.Add("@piDEPTRUSTEESUBCD", strDEPTRUSTEESUBCD)   '発受託人サブコード
            param.Add("@piARRSTATION", strPrmARRSTATION)          '着駅コード
            param.Add("@piARRTRUSTEECD", strARRTRUSTEECD)         '着受託人コード
            param.Add("@piARRTRUSTEESUBCD", strARRTRUSTEESUBCD)   '着受託人サブコード
            param.Add("@piDEPTAIOU1", strPrmDepTAIOU1)            'ＪＯＴ発店所コード(対応C1)
            param.Add("@piDEPTAIOU2", strPrmDepTAIOU2)            'ＪＯＴ発店所コード(対応C2)
            param.Add("@piARRTAIOU2", strPrmArrTAIOU2)            'ＪＯＴ着店所コード(対応C2)
            param.Add("@piCTNTYPE", strPrmCTNTYPE)                'コンテナ記号
            param.Add("@piCTNNO", strPrmCTNNO)                    'コンテナ番号
            param.Add("@piKEIYAKUCD", strPrmKEIYAKUCD)            '契約コード
            param.Add("@piJRFIXEDFARE", strPrmJRFIXEDFARE)        'ＪＲ所定運賃
            param.Add("@piOTHER1FEE", strPrmOTHER1FEE)            'その他１
            param.Add("@piDEPORGCD", strPrmJOTDEPBRANCHCD)        'ＪＯＴ発組織コード
            param.Add("@piARRORGCD", strPrmJOTARRBRANCHCD)        'ＪＯＴ着組織コード
            param.Add("@piVBFLG", CONST_VBFLG)                    'VBから呼ばれたかのフラグ
            '戻り値 VBでは未使用
            param.Add("@poJRFIXEDFARE", "")
            param.Add("@poOWNDISCOUNTFEE", "")
            param.Add("@poFREESENDFEE_BE", "")
            param.Add("@poFREESENDFEE_AF", "")
            param.Add("@poFREESENDFEE", "")
            param.Add("@poSHIPFEE", "")
            param.Add("@poARRIVEFEE", "")
            param.Add("@poFREESENDRATEAPPLKBN", "")
            param.Add("@poFREESENDRATEAPPLKBNNM", "")
            param.Add("@poKaisouAPPLYRATE", "0.0")
            param.Add("@poKaisouNextFlg", "")
            param.Add("@poKaisouDispNextFlg", "")
            param.Add("@poHASOUKBN", "")
            param.Add("@poHASOUKBNNM", "")
            param.Add("@poHasouNextFlg", "")
            param.Add("@poHasouDispNextFlg", "")

            Try
                Dim dtData As DataTable = Nothing
                '使用料金計算処理 ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_KAIS, param, dtData)
                '戻り値設定
                If dtData.Rows.Count > 0 Then
                    drMeisaiData("JRFIXEDFARE") = dtData.Rows(0)("JRFIXEDFARE").ToString
                    drMeisaiData("OWNDISCOUNTFEE") = dtData.Rows(0)("OWNDISCOUNTFEE").ToString
                    drMeisaiData("FREESENDFEE") = dtData.Rows(0)("FREESENDFEE").ToString
                    drMeisaiData("SHIPFEE") = dtData.Rows(0)("SHIPFEE").ToString
                    drMeisaiData("ARRIVEFEE") = dtData.Rows(0)("ARRIVEFEE").ToString

                    drHanteiData("FREESENDRATEAPPLKBN") = dtData.Rows(0)("FREESENDRATEAPPLKBN").ToString
                    drHanteiData("FREESENDRATEAPPLKBNNM") = dtData.Rows(0)("FREESENDRATEAPPLKBNNM").ToString
                    drHanteiData("FREESENDRATE") = dtData.Rows(0)("KAISOUAPPLYRATE").ToString
                    drHanteiData("FREESENDRATENEXTFLG") = dtData.Rows(0)("KAISOUNEXTFLG").ToString
                    drHanteiData("FREESENDRATEDISPNEXTFLG") = dtData.Rows(0)("KAISOUDISPNEXTFLG").ToString
                    drHanteiData("SHIPFEEAPPLKBN") = dtData.Rows(0)("HASOUKBN").ToString
                    drHanteiData("SHIPFEEAPPLKBNNM") = dtData.Rows(0)("HASOUKBNNM").ToString
                    drHanteiData("SHIPFEENEXTFLG") = dtData.Rows(0)("HASOUNEXTFLG").ToString
                    drHanteiData("SHIPFEEDISPNEXTFLG") = dtData.Rows(0)("HASOUDISPNEXTFLG").ToString
                Else
                    drMeisaiData("JRFIXEDFARE") = ""
                    drMeisaiData("OWNDISCOUNTFEE") = ""
                    drMeisaiData("FREESENDFEE") = ""
                    drMeisaiData("SHIPFEE") = ""
                    drMeisaiData("ARRIVEFEE") = ""
                    drHanteiData("FREESENDRATEAPPLKBN") = ""
                    drHanteiData("FREESENDRATEAPPLKBNNM") = ""
                    drHanteiData("FREESENDRATE") = ""
                    drHanteiData("FREESENDRATENEXTFLG") = ""
                    drHanteiData("FREESENDRATEDISPNEXTFLG") = ""
                    drHanteiData("SHIPFEEAPPLKBN") = ""
                    drHanteiData("SHIPFEEAPPLKBNNM") = ""
                    drHanteiData("SHIPFEENEXTFLG") = ""
                    drHanteiData("SHIPFEEDISPNEXTFLG") = ""
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_KAIS_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckERR("回送費計算処理エラー。", C_MESSAGE_NO.CTN_KAIS_ERR)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' マスタ検索処理（同じパラメータならDB抽出せずに保持内容を返却）
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="I_PARA01"></param>
    ''' <returns></returns>
    Private Function WW_FixvalueMasterDataGet(I_CODE As String, I_CLASS As String, I_KEYCODE As String, I_PARA01 As String) As DataTable
        Static keyValues As Dictionary(Of String, String)
        Static retDt As DataTable
        Dim retFilterdDt As DataTable
        'キー情報を比較または初期状態または異なるキーの場合は再抽出
        If keyValues Is Nothing OrElse
            I_CLASS = "NEWHISTORYNOGET" OrElse
           (Not (keyValues("I_CODE") = I_CODE _
                 AndAlso keyValues("I_CLASS") = I_CLASS _
                 AndAlso keyValues("I_PARA01") = I_PARA01)) Then
            keyValues = New Dictionary(Of String, String) _
                      From {{"I_CODE", I_CODE}, {"I_CLASS", I_CLASS}, {"I_PARA01", I_PARA01}}
            retDt = New DataTable
        Else
            retFilterdDt = retDt
            '抽出キー情報が一致しているので保持内容を返却
            If I_KEYCODE <> "" Then
                Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
                If qKeyFilterd.Any Then
                    retFilterdDt = qKeyFilterd.CopyToDataTable
                Else
                    retFilterdDt = retDt.Clone
                End If
            End If

            Return retFilterdDt
        End If
        'キーが変更された場合の抽出処理
        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        MySqlConnection.ClearPool(SQLcon)

        '検索SQL文
        Dim SQLStr As String =
           " SELECT" _
            & "   coalesce(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
            & " , coalesce(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
            & " , coalesce(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
            & " , coalesce(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
            & " , coalesce(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
            & " , coalesce(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
            & " , coalesce(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
            & " , coalesce(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
            & " , coalesce(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
            & " , coalesce(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
            & " , coalesce(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
            & " , coalesce(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
            & " , coalesce(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
            & " , coalesce(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
            & " , coalesce(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
            & " , coalesce(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
            & " , coalesce(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
            & " , coalesce(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
            & " , coalesce(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
            & " , coalesce(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
            & " , coalesce(RTRIM(VIW0001.VALUE16), '')     AS VALUE16" _
            & " , coalesce(RTRIM(VIW0001.VALUE17), '')     AS VALUE17" _
            & " , coalesce(RTRIM(VIW0001.VALUE18), '')     AS VALUE18" _
            & " , coalesce(RTRIM(VIW0001.VALUE19), '')     AS VALUE19" _
            & " , coalesce(RTRIM(VIW0001.VALUE20), '')     AS VALUE20" _
            & " , coalesce(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
            & " , coalesce(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
            & " FROM  LNG.VIW0001_FIXVALUE VIW0001" _
            & " WHERE VIW0001.CLASS = @P01" _
            & " AND VIW0001.DELFLG <> @P03"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '会社コード
        If Not String.IsNullOrEmpty(I_CODE) Then
            SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    VIW0001.KEYCODE"

        Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)
            'Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)
            Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)

            PARA01.Value = I_CLASS
            'PARA02.Value = I_KEYCODE
            PARA03.Value = C_DELETE_FLG.DELETE

            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    retDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                retDt.Load(SQLdr)
            End Using
            'CLOSE
            SQLcmd.Dispose()
        End Using

        retFilterdDt = retDt
        '抽出キー情報が一致しているので保持内容を返却
        If I_KEYCODE <> "" Then
            Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
            If qKeyFilterd.Any Then
                retFilterdDt = qKeyFilterd.CopyToDataTable
            Else
                retFilterdDt = retDt.Clone
            End If
        End If

        Return retFilterdDt
    End Function

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        '〇 タブの使用可否制御
        WF_Dtab01.Enabled = True
        WF_Dtab02.Enabled = True
        WF_Dtab03.Enabled = True

        '〇 受注内容の制御
        '100:受注受付以外の場合は、受注内容(ヘッダーの内容)の変更を不可とする。
        If work.WF_SELROW_ORDERSTATUS.Text <> BaseDllConst.CONST_ORDERSTATUS_100 Then
            '発送予定日
            Me.TxtPlanDepYMD.Enabled = False
            'コンテナ記号
            Me.TxtCtnType.Enabled = False
            'コンテナ番号
            Me.TxtCtnNo.Enabled = False
        Else
            '発送予定日
            Me.TxtPlanDepYMD.Enabled = True
            'コンテナ記号
            Me.TxtCtnType.Enabled = True
            'コンテナ番号
            Me.TxtCtnNo.Enabled = True
        End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text, needsPopUp:=True)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '年月日(発送日)
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANDEPYMD", TxtPlanDepYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtPlanDepYMD.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発送日", needsPopUp:=True)
            TxtPlanDepYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        'コンテナ記号
        Master.CheckField(WF_CAMPCODE.Text, "CTNTYPE", TxtCtnType.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "コンテナ記号", needsPopUp:=True)
            TxtCtnType.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        'コンテナ番号
        Master.CheckField(WF_CAMPCODE.Text, "CTNNO", TxtCtnNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "コンテナ番号", needsPopUp:=True)
            TxtCtnNo.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        ''○ 画面表示データ保存
        'Master.SaveTable(LNT0001tbl)

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' チェック処理(タブ「明細データ」)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTab1(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・ユーザ更新権限なしです。"
            WW_CheckMES2 = ""
            WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '(一覧)明細データチェック
        '○ 単項目チェック
        Dim intDetailNo As Integer = 0
        Dim WK_DetailNo As Integer = 0
        Dim strUpdateMsg As String = "件目は更新できないレコードです。"
        For Each LNT0001row As DataRow In LNT0001tbl.Rows
            intDetailNo += 1
            WW_LINE_ERR = ""

            '品目コード(バリデーションチェック)
            WW_TEXT = LNT0001row("ITEMCD")
            Master.CheckField(Master.USERCAMP, "ITEMCD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・品目コード"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '発駅コード(バリデーションチェック)
            WW_TEXT = LNT0001row("DEPSTATION")
            Master.CheckField(Master.USERCAMP, "DEPSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・発駅コード"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '着駅コード(バリデーションチェック)
            WW_TEXT = LNT0001row("ARRSTATION")
            Master.CheckField(Master.USERCAMP, "ARRSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・着駅コード"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '発受託人コード(バリデーションチェック)
            WW_TEXT = LNT0001row("DEPTRUSTEECD")
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・発受託人コード"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '着受託人コード(バリデーションチェック)
            WW_TEXT = LNT0001row("ARRTRUSTEECD")
            Master.CheckField(Master.USERCAMP, "ARRTRUSTEECD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・着受託人コード"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積空区分コード(バリデーションチェック)
            WW_TEXT = LNT0001row("STACKFREEKBNCD")
            Master.CheckField(Master.USERCAMP, "STACKFREEKBNCD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・積空区分"
                If intDetailNo <> WK_DetailNo Then
                    WW_CheckMES1 = "○" & intDetailNo & strUpdateMsg & ControlChars.NewLine & WW_CheckMES1
                    WK_DetailNo = intDetailNo
                End If
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR_Detail(WW_CheckMES1, WW_CheckMES2, LNT0001row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Next

    End Sub

    ''' <summary>
    ''' チェック処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTab3(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '### 20200622 START((全体)No82対応) ######################################
        '発送順でソートし、重複がないかチェックする。
        Dim LNT0001tbltab3_DUMMY As DataTable = LNT0001tbl_tab3.Copy
        'LNT0001tbltab3_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer), "Convert(SHIPORDER, 'System.Int32')")
        LNT0001tbltab3_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer))
        For Each LNT0001row As DataRow In LNT0001tbltab3_DUMMY.Rows
            Try
                LNT0001row("SHIPORDER_SORT") = LNT0001row("SHIPORDER")
            Catch ex As Exception
                LNT0001row("SHIPORDER_SORT") = 0
            End Try
        Next
        Dim LNT0001tbltab3_dv As DataView = New DataView(LNT0001tbltab3_DUMMY)
        Dim chkShipOrder As String = ""

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        WW_ERR_MES &= ControlChars.NewLine & "  --> オーダー№         =" & Me.TxtOrderNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 発送日             =" & Me.TxtPlanDepYMD.Text & " , "

        rightview.SetErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CPM0006row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR_Detail(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CPM0006row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            'WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
            WW_ERR_MES &= " -->" & MESSAGE2
        End If

        'If Not IsNothing(CPM0006row) Then
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先コード =" & CPM0006row("TORICODE") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & CPM0006row("STYMD") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & CPM0006row("ENDYMD") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先名称 =" & CPM0006row("TORINAME") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先略称 =" & CPM0006row("TORINAMES") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先カナ名称 =" & CPM0006row("TORINAMEKANA") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 部門名称 =" & CPM0006row("DEPTNAME") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（上） =" & CPM0006row("POSTNUM1") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（下） =" & CPM0006row("POSTNUM2") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 住所１ =" & CPM0006row("ADDR1") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 住所２ =" & CPM0006row("ADDR2") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 住所３ =" & CPM0006row("ADDR3") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 住所４ =" & CPM0006row("ADDR4") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 電話番号 =" & CPM0006row("TEL") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> ＦＡＸ番号 =" & CPM0006row("FAX") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> メールアドレス =" & CPM0006row("MAIL") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 石油利用フラグ =" & CPM0006row("PAYKBN") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 銀行コード =" & CPM0006row("BANKCODE") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 支店コード =" & CPM0006row("BANKBRANCHCODE") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 口座種別 =" & CPM0006row("ACCOUNTTYPE") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 口座番号 =" & CPM0006row("ACCOUNTNUMBER") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 口座名義 =" & CPM0006row("ACCOUNTNAME") & " , "
        '    WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & CPM0006row("DELFLG")
        'End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()

        Select Case WF_DetailMView.ActiveViewIndex
           '受付データ
            Case 0
                '〇 (一覧)テキストボックスの制御(読取専用)
                WW_ListTextBoxReadControlTab1()

            '入換・積込指示
            Case 1
                If Me.ChkAutoFlg.Checked = True Then
                    '〇 (一覧)テキストボックスの制御(読取専用)
                    WW_ListTextBoxReadControlTab2()
                End If

        End Select

    End Sub

    ''' <summary>
    ''' タブ(タンク車割当)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab1()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea1.FindControl(pnlListArea1.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        '　データテーブルの行Index
        Dim rowIdx As Integer = 0
        '### ★積置（チェックボックス）を非活性にするための準備 ################
        Dim chkObjST As CheckBox = Nothing
        Dim chkObjIdWOSTcnt As String = "chk" & pnlListArea1.ID & "STACKINGFLG"
        Dim chkObjSTId As String
        '#######################################################################
        Dim chkStackingOrderNo As String = ""

        '### 20201110 START 指摘票No199対応 ####################################
        Dim chkObjOT As CheckBox = Nothing
        Dim chkObjIdWOOTcnt As String = "chk" & pnlListArea1.ID & "OTTRANSPORTFLG"
        Dim chkObjOTId As String
        '### 20201110 END   指摘票No199対応 ####################################

        '### 20201208 START 指摘票No248対応 ####################################
        Dim chkObjUP As CheckBox = Nothing
        Dim chkObjIdWOUPcnt As String = "chk" & pnlListArea1.ID & "UPGRADEFLG"
        Dim chkObjUPId As String
        '### 20201208 END   指摘票No248対応 ####################################

        '### 20210125 START 指摘票No300対応 ####################################
        Dim chkObjDOWN As CheckBox = Nothing
        Dim chkObjIdWODOWNcnt As String = "chk" & pnlListArea1.ID & "DOWNGRADEFLG"
        Dim chkObjDOWNId As String
        '### 20210125 END   指摘票No300対応 ####################################

        '受注進行ステータスが"受注受付"の場合
        '※但し、受注営業所が"011203"(袖ヶ浦営業所)以外の場合は、貨物駅入線順を読取専用(入力不可)とする。
        '※但し、受注営業所が"010402"(仙台新港営業所)以外の場合は、積込日を読取専用(入力不可)とする。
        '※但し、発送順区分が"2"(発送対象外)の場合は、発送順を読取専用(入力不可)とする。
        If work.WF_SELROW_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then

            '受注進行ステータスが"310：手配完了"以降のステータスの場合
        Else
            '### ★選択（チェックボックス）を非活性にするための準備 ################
            Dim chkObj As CheckBox = Nothing
            '　LINECNTを除いたチェックボックスID
            Dim chkObjIdWOLincnt As String = "chk" & pnlListArea1.ID & "OPERATION"
            '　LINECNTを含むチェックボックスID
            Dim chkObjId As String
            'Dim chkObjType As String
            '#######################################################################

            For Each rowitem As TableRow In tblObj.Rows
                '### ★選択（チェックボックス）を非活性にする ##########################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjId = chkObjIdWOLincnt & Convert.ToString(loopdr("LINECNT"))
                'chkObjType = Convert.ToString(loopdr("CALCACCOUNT"))
                chkObj = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObj = DirectCast(cellObj.FindControl(chkObjId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObj IsNot Nothing Then
                        '選択(チェックボックス)を非活性
                        chkObj.Enabled = False
                        Exit For
                    End If
                Next
                '#######################################################################

                '### ★積置選択（チェックボックス）を非活性にする ######################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjST = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjST IsNot Nothing Then
                        '積置可否フラグ(チェックボックス)を非活性
                        chkObjST.Enabled = False
                        Exit For
                    End If
                Next
                '#######################################################################
                '### 20201110 START 指摘票No199対応 ####################################
                chkObjOTId = chkObjIdWOOTcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjOT = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjOT = DirectCast(cellObj.FindControl(chkObjOTId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjOT IsNot Nothing Then
                        'OT輸送可否フラグ(チェックボックス)を非活性
                        chkObjOT.Enabled = False
                        Exit For
                    End If
                Next
                '### 20201110 END   指摘票No199対応 ####################################
                '### 20201208 START 指摘票対応(No248)全体 ################################
                chkObjUPId = chkObjIdWOUPcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjUP = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjUP = DirectCast(cellObj.FindControl(chkObjUPId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjUP IsNot Nothing Then
                        '格上可否フラグ(チェックボックス)を非活性
                        chkObjUP.Enabled = False
                        Exit For
                    End If
                Next
                '### 20201208 END   指摘票対応(No248)全体 ################################
                '### 20210125 START 指摘票対応(No300)全体 ################################
                chkObjDOWNId = chkObjIdWODOWNcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjDOWN = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjDOWN = DirectCast(cellObj.FindControl(chkObjDOWNId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjDOWN IsNot Nothing Then
                        '格上可否フラグ(チェックボックス)を非活性
                        chkObjDOWN.Enabled = False
                        Exit For
                    End If
                Next
                '### 20210125 END   指摘票対応(No300)全体 ################################

                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "TANKNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "LINEORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JRINSPECTIONDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGETRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGERETSTATIONNAME") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next
                rowIdx += 1
            Next
        End If
    End Sub

    ''' <summary>
    ''' タブ(入換・積込指示)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab2()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea2.FindControl(pnlListArea2.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim rowCount As Integer = 0
        Dim colCount As Integer = 0
        Dim tabIndex As Integer = 0

        For Each rowitem As TableRow In tblObj.Rows
            For Each cellObj As TableCell In rowitem.Controls
                'セル毎に編集不可を設定
                Select Case colCount
                    Case 0 To 13
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                End Select

                colCount += 1
            Next
            rowCount += 1
            colCount = 0
        Next

    End Sub

    ''' <summary>
    ''' 受注データ（ヘッダ）取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Private Sub WW_GetOrderHead(ByVal SQLcon As MySqlConnection)

        Dim LNT0001ORDERHEADtbl As New DataTable
        Dim sqlText As New StringBuilder()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        With sqlText
            .AppendLine("SELECT")
            .AppendLine("     PLANDEPYMD")
            .AppendLine("    ,CTNTYPE")
            .AppendLine("    ,RIGHT('00000000' + convert(varchar, coalesce(CTNNO, 0)), 8) AS CTNNO")
            .AppendLine("    ,STATUS")
            .AppendLine("    ,BIGCTNCD")
            .AppendLine("    ,MIDDLECTNCD")
            .AppendLine("    ,SMALLCTNCD")
            .AppendLine("    ,RENTRATE125NEXTFLG")
            .AppendLine("    ,RENTRATE125")
            .AppendLine("    ,ROUNDFEENEXTFLG")
            .AppendLine("    ,ROUNDFEE")
            .AppendLine("    ,ROUNDKBNGE")
            .AppendLine("    ,ROUNDKBNLT")
            .AppendLine("FROM")
            .AppendLine("    LNG.LNT0004_ORDERHEAD")
            .AppendLine("WHERE")
            .AppendLine("        ORDERNO = @P01")
            .AppendLine("    AND DELFLG <> @P02")
        End With

        Try
            Using SQLcmd As New MySqlCommand(sqlText.ToString, SQLcon)

                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '受注№
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SELROW_ORDERNO.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001ORDERHEADtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001ORDERHEADtbl.Load(SQLdr)
                End Using

                Me.TxtPlanDepYMD.Text = LNT0001ORDERHEADtbl.Rows(0)("PLANDEPYMD")
                Me.TxtCtnType.Text = LNT0001ORDERHEADtbl.Rows(0)("CTNTYPE")
                Me.TxtCtnTypeCode.Text = LNT0001ORDERHEADtbl.Rows(0)("CTNTYPE")
                Me.TxtCtnNo.Text = LNT0001ORDERHEADtbl.Rows(0)("CTNNO")
                Me.TxtCtnNoCode.Text = LNT0001ORDERHEADtbl.Rows(0)("CTNNO")
                Me.work.WF_SELROW_ORDERSTATUS.Text = LNT0001ORDERHEADtbl.Rows(0)("STATUS")
                Me.TxtBigCtnCode.Text = LNT0001ORDERHEADtbl.Rows(0)("BIGCTNCD")
                Me.TxtMiddleCtnCode.Text = LNT0001ORDERHEADtbl.Rows(0)("MIDDLECTNCD")
                Me.TxtSmallCtnCode.Text = LNT0001ORDERHEADtbl.Rows(0)("SMALLCTNCD")

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D GET_ORDERHEAD", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D GET_ORDERHEAD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 各タブ用退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTAB1TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB1TBL.txt"
        work.WF_SEL_INPTAB2TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB2TBL.txt"
        work.WF_SEL_INPTAB3TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB3TBL.txt"

        '〇メニュー画面から遷移した場合の対応(一覧の保存場所を作成)
        If work.WF_SEL_INPTBL.Text = "" Then
            work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

        End If
    End Sub

#Region "ViewStateを圧縮 これをしないとViewStateが7万文字近くなり重くなる,実行すると9000文字"

    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
#End Region

End Class