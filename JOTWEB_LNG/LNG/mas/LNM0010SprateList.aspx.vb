''************************************************************
' 【廃止】特別料金マスタメンテナンス・一覧画面
' 作成日 2025/02/06
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/02/06 新規作成
'          : 2025/03/18 廃止　→ LNM0014Sprate(統合版特別料金マスタへ変更)
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' 特別料金マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0010SprateList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0010tbl As DataTable         '一覧格納用テーブル
    Private LNM0010UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0010Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    '〇 帳票用
    Private Const CONST_COLOR_HATCHING_REQUIRED As String = "#FFFF00" '入力必須網掛け色
    Private Const CONST_COLOR_HATCHING_UNNECESSARY As String = "#BFBFBF" '入力不要網掛け色
    Private Const CONST_COLOR_HATCHING_HEADER As String = "#002060" 'ヘッダ網掛け色
    Private Const CONST_COLOR_FONT_HEADER As String = "#FFFFFF" 'ヘッダフォント色
    Private Const CONST_COLOR_BLACK As String = "#000000" '黒
    Private Const CONST_COLOR_GRAY As String = "#808080" '灰色
    Private Const CONST_HEIGHT_PER_ROW As Integer = 14 'セルのコメントの一行あたりの高さ
    Private Const CONST_DATA_START_ROW As Integer = 3 'データ開始行
    Private Const CONST_PULLDOWNSHEETNAME = "PULLLIST"

    '〇 タブ用
    Private Const CONST_COLOR_TAB_ACTIVE As String = "#FFFFFF"　'アクティブ
    Private Const CONST_COLOR_TAB_INACTIVE As String = "#D9D9D9"  '非アクティブ

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ErrSW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード

    '〇 共通定数
    Private Const WW_COLUMNCOUNT As Integer = 21                          'スプレッド列数

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(LNM0010tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0010WRKINC.FILETYPE.EXCEL)
                        'Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                        '    WF_EXCELPDF(LNM0010WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonKGUPD"             '更新ボタン(北海道ガス特別料金)押下
                            WF_ButtonKGUPD_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                            GridViewInitialize()
                        Case "WF_SelectCALENDARChange", "WF_TORIChange" 'カレンダー変更時、荷主ドロップダウン変更時
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                GridViewInitialize()
                            End Using
                        Case "WF_TARGETTABLEChange" '表示対象テーブル変更時
                            WF_ButtonKGUPD.Visible = False
                            Select Case WF_TARGETTABLE.SelectedValue
                                Case LNM0010WRKINC.TableList.八戸特別料金
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLHA
                                Case LNM0010WRKINC.TableList.ENEOS業務委託料
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLEN
                                Case LNM0010WRKINC.TableList.東北電力車両別追加料金
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLTO
                                Case LNM0010WRKINC.TableList.北海道ガス特別料金
                                    WF_ButtonKGUPD.Visible = True
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLKG
                                Case LNM0010WRKINC.TableList.SK特別料金
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLSKSP
                                Case LNM0010WRKINC.TableList.SK燃料サーチャージ
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLSKSU
                            End Select
                            'WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM/dd")
                            WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")
                            DowpDownTORIInitialize()
                            GridViewInitialize()
                        Case "WF_ButtonDebug"           'デバッグボタン押下
                            WF_ButtonDEBUG_Click()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" And
                        Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" And
                        Not WF_ButtonClick.Value = "WF_TORIChange" And
                        Not WF_ButtonClick.Value = "WF_TARGETTABLEChange" Then
                        DisplayGrid()
                    End If

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

        Finally
            '○ 格納Table Close
            If Not IsNothing(LNM0010tbl) Then
                LNM0010tbl.Clear()
                LNM0010tbl.Dispose()
                LNM0010tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0010WRKINC.MAPIDL
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○ 初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '○ 右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = Master.USERCAMP
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize("")

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

        '〇 更新画面からの遷移もしくは、アップロード完了の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        '参照権限の無いユーザの場合MENUへ
        If LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) = False And '情シス、高圧ガス
             LNM0010WRKINC.IshikariCheck(Master.ROLE_ORG) = False And '石狩営業所
             LNM0010WRKINC.HachinoheCheck(Master.ROLE_ORG) = False And '八戸営業所
             LNM0010WRKINC.TohokuCheck(Master.ROLE_ORG) = False And '東北支店
             LNM0010WRKINC.MizushimaCheck(Master.ROLE_ORG) = False Then '水島営業所

            '○ メニュー画面遷移
            Master.TransitionPrevPage(, LNM0006WRKINC.TITLEKBNS)
        End If

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0010D Or
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0010DKG Or
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0010H Then

            '表示テーブルドロップダウン設定
            DowpDownTARGETTABLEInitialize()

            ' 登録画面からの遷移
            Master.RecoverTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.八戸特別料金
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.ENEOS業務委託料
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.東北電力車両別追加料金
                Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.北海道ガス特別料金
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.SK特別料金
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    WF_TARGETTABLE.SelectedValue = LNM0010WRKINC.TableList.SK燃料サーチャージ
            End Select

            '表示荷主ドロップダウン設定
            DowpDownTORIInitialize()
        Else
            ' サブメニューからの画面遷移
            ' メニューからの画面遷移
            If String.IsNullOrEmpty(Master.VIEWID) Then
                rightview2.MAPIDS = Master.MAPID
                rightview2.MAPID = Master.MAPID
                rightview2.COMPCODE = Master.USERCAMP
                rightview2.MAPVARI = Master.MAPvariant
                rightview2.PROFID = Master.PROF_VIEW
                rightview2.MENUROLE = Master.ROLE_MENU
                rightview2.MAPROLE = Master.ROLE_MAP
                rightview2.VIEWROLE = Master.ROLE_VIEWPROF
                rightview2.RPRTROLE = Master.ROLE_RPRTPROF
                rightview2.Initialize("画面レイアウト設定", WW_Dummy)
                Master.VIEWID = rightview2.GetViewId(Master.USERCAMP)
            End If
            ' 画面間の情報クリア
            work.Initialize()
            Master.CreateXMLSaveFile()

            '表示テーブルドロップダウン設定
            DowpDownTARGETTABLEInitialize()

            '初期表示テーブル
            Select Case True
                Case LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) '情シス、高圧ガスの場合
                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLHA　'八戸特別料金
                Case LNM0010WRKINC.IshikariCheck(Master.ROLE_ORG) '石狩営業所の場合
                    WF_ButtonKGUPD.Visible = True
                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLKG　'北海道ガス特別料金
                Case LNM0010WRKINC.HachinoheCheck(Master.ROLE_ORG) '八戸営業所の場合
                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLHA　'八戸特別料金
                Case LNM0010WRKINC.TohokuCheck(Master.ROLE_ORG) '東北支店の場合
                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLTO　'東北電力車両別追加料金
                Case LNM0010WRKINC.MizushimaCheck(Master.ROLE_ORG) '水島営業所の場合
                    work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLEN　'ENEOS業務委託料
            End Select

            '表示荷主ドロップダウン設定
            DowpDownTORIInitialize()
        End If

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyAdminOrgCode.Value = ""
        Else
            VisibleKeyAdminOrgCode.Value = Master.ROLE_ORG
        End If

        '石狩営業所以外の場合
        If LNM0010WRKINC.IshikariCheck(Master.ROLE_ORG) = False Then
            VisibleKeyIshikariOrgCode.Value = ""
        Else
            VisibleKeyIshikariOrgCode.Value = Master.ROLE_ORG
        End If

        '八戸営業所以外の場合
        If LNM0010WRKINC.HachinoheCheck(Master.ROLE_ORG) = False Then
            VisibleKeyHachinoheOrgCode.Value = ""
        Else
            VisibleKeyHachinoheOrgCode.Value = Master.ROLE_ORG
        End If

        '東北支店以外の場合
        If LNM0010WRKINC.TohokuCheck(Master.ROLE_ORG) = False Then
            VisibleKeyTohokuOrgCode.Value = ""
        Else
            VisibleKeyTohokuOrgCode.Value = Master.ROLE_ORG
        End If

        '水島営業所以外の場合
        If LNM0010WRKINC.MizushimaCheck(Master.ROLE_ORG) = False Then
            VisibleKeyMizushimaOrgCode.Value = ""
        Else
            VisibleKeyMizushimaOrgCode.Value = Master.ROLE_ORG
        End If

        '対象年月
        'WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM/dd")
        WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

    End Sub

    ''' <summary>
    ''' 表示対象ドロップダウンリスト初期設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DowpDownTARGETTABLEInitialize()
        '表示テーブル

        Me.WF_TARGETTABLE.Items.Clear()

        Select Case True
            Case LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) '情シス、高圧ガス
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.八戸特別料金), LNM0010WRKINC.TableList.八戸特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.ENEOS業務委託料), LNM0010WRKINC.TableList.ENEOS業務委託料))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.東北電力車両別追加料金), LNM0010WRKINC.TableList.東北電力車両別追加料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.北海道ガス特別料金), LNM0010WRKINC.TableList.北海道ガス特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.SK特別料金), LNM0010WRKINC.TableList.SK特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.SK燃料サーチャージ), LNM0010WRKINC.TableList.SK燃料サーチャージ))
            Case LNM0010WRKINC.IshikariCheck(Master.ROLE_ORG) '石狩営業所
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.北海道ガス特別料金), LNM0010WRKINC.TableList.北海道ガス特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.SK特別料金), LNM0010WRKINC.TableList.SK特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.SK燃料サーチャージ), LNM0010WRKINC.TableList.SK燃料サーチャージ))
            Case LNM0010WRKINC.HachinoheCheck(Master.ROLE_ORG) '八戸営業所
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.八戸特別料金), LNM0010WRKINC.TableList.八戸特別料金))
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.ENEOS業務委託料), LNM0010WRKINC.TableList.ENEOS業務委託料))
            Case LNM0010WRKINC.TohokuCheck(Master.ROLE_ORG) '東北支店
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.東北電力車両別追加料金), LNM0010WRKINC.TableList.東北電力車両別追加料金))
            Case LNM0010WRKINC.MizushimaCheck(Master.ROLE_ORG) '水島営業所
                WF_TARGETTABLE.Items.Add(New ListItem([Enum].GetName(GetType(LNM0010WRKINC.TableList), LNM0010WRKINC.TableList.ENEOS業務委託料), LNM0010WRKINC.TableList.ENEOS業務委託料))
        End Select
    End Sub

    ''' <summary>
    ''' 荷主ドロップダウンリスト初期設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DowpDownTORIInitialize()
        '荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLHACHINOHESPRATE)
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLENEOSCOMFEE)
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLTOHOKUSPRATE)
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLKGSPRATE)
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLSKSPRATE)
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                retToriList = LNM0010WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0010WRKINC.TBLSKSURCHARGE)

        End Select
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next
    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNM0010tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0010tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0010tbl)
        Dim WW_RowFilterCMD As New StringBuilder
        WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        TBLview.RowFilter = WW_RowFilterCMD.ToString

        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightview2.GetViewId(Master.USERCAMP)
        End If

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        'CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.MAPID = work.WF_SEL_CONTROLTABLE.Text
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNM0010tbl) Then
            LNM0010tbl = New DataTable
        End If

        If LNM0010tbl.Columns.Count <> 0 Then
            LNM0010tbl.Columns.Clear()
        End If

        LNM0010tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを特別料金マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , VIW0004.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , VIW0004.TABLEID                                                          AS TABLEID             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.RECOID), '')                                      AS RECOID              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.RECONAME), '')                                      AS RECONAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TORICODE), '')                                      AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TORINAME), '')                                      AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.ORGCODE), '')                                      AS ORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.ORGNAME), '')                                      AS ORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KASANORGCODE), '')                                      AS KASANORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KASANORGNAME), '')                                      AS KASANORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TODOKECODE), '')                                      AS TODOKECODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TODOKENAME), '')                                      AS TODOKENAME              ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(VIW0004.STYMD, '%Y/%m/%d'), '')                     AS STYMD               ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(VIW0004.ENDYMD, '%Y/%m/%d'), '')                    AS ENDYMD              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KINGAKU), '0')                                      AS KINGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.SYABAN), '')                                      AS SYABAN              ")
        SQLStr.AppendLine("   , COALESCE(CONCAT(LEFT(VIW0004.TAISHOYM ,4),'/',RIGHT(VIW0004.TAISHOYM,2)) , '')    AS TAISHOYM               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.ITEMID), '')                                      AS ITEMID              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.ITEMNAME), '')                                      AS ITEMNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.SYABARA), '0')                                      AS SYABARA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KOTEIHI), '0')                                      AS KOTEIHI              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TANKA), '0')                                      AS TANKA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KYORI), '0')                                      AS KYORI              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KEIYU), '0')                                      AS KEIYU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KIZYUN), '0')                                      AS KIZYUN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.TANKASA), '0')                                      AS TANKASA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.KAISU), '0')                                      AS KAISU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.COUNT), '0')                                      AS COUNT              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.FEE), '0')                                      AS FEE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.BIKOU), '')                                      AS BIKOU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.USAGECHARGE), '')                                      AS USAGECHARGE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.SURCHARGE), '0')                                      AS SURCHARGE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0004.BIKOU3), '')                                      AS BIKOU3              ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE VIW0004                                                                      ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  VIW0004.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")

        '対象テーブル
        SQLStr.AppendLine("    VIW0004.TABLEID = @TABLEID                                               ")


        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim dt As DateTime
        Dim Itype As Integer

        '削除フラグ
        'If Not work.WF_SEL_DELFLG_S.Text = "1" Then
        SQLStr.AppendLine(" AND  VIW0004.DELFLG <> '1'                                                      ")
        'End If
        '取引先コード
        If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
            SQLStr.AppendLine(" AND  VIW0004.TORICODE = @TORICODE                                          ")
        End If

        '対象年月
        Select Case work.WF_SEL_CONTROLTABLE.Text
            '有効開始日、有効終了日項目有
            Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                 LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                 LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                 LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                    SQLStr.AppendLine(" AND  @STYMD BETWEEN VIW0004.STYMD AND VIW0004.ENDYMD  ")
                End If

            '対象年月項目有
            Case LNM0010WRKINC.MAPIDLKG, '北海道ガス特別料金マスタ
                 LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ

                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    SQLStr.AppendLine(" AND  COALESCE(VIW0004.TAISHOYM, '0') = COALESCE(@TAISHOYM, '0')  ")
                End If
        End Select

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     VIW0004.RECOID                                                             ")
        SQLStr.AppendLine("    ,VIW0004.TORICODE                                                           ")
        SQLStr.AppendLine("    ,VIW0004.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,VIW0004.STYMD                                                              ")
        SQLStr.AppendLine("    ,VIW0004.ENDYMD                                                             ")
        SQLStr.AppendLine("    ,VIW0004.TAISHOYM                                                           ")
        SQLStr.AppendLine("    ,VIW0004.SYABAN                                                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                'ロール
                Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                P_ROLE.Value = Master.ROLE_ORG

                '対象テーブル
                Dim P_TABLEID As MySqlParameter = SQLcmd.Parameters.Add("@TABLEID", MySqlDbType.VarChar, 30)
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLHACHINOHESPRATE
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLENEOSCOMFEE
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLTOHOKUSPRATE
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLKGSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLSKSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        P_TABLEID.Value = LNM0010WRKINC.TBLSKSURCHARGE
                End Select

                '取引先コード
                If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                    P_TORICODE.Value = WF_TORI.SelectedValue
                End If

                '対象年月
                Select Case work.WF_SEL_CONTROLTABLE.Text
                '有効開始日、有効終了日項目有
                    Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                         LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                         LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                         LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                        If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                            Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)
                            P_STYMD.Value = dt
                        End If
                '対象年月項目有
                    Case LNM0010WRKINC.MAPIDLKG, '北海道ガス特別料金マスタ
                         LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ

                        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                            Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                            P_TAISHOYM.Value = Itype
                        End If
                End Select

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0010row As DataRow In LNM0010tbl.Rows
                    i += 1
                    LNM0010row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0010L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  ボタン押下処理                                                        ***
    ' ******************************************************************************
    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '北海道ガス特別料金マスタ表示中の場合
        If work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLKG Then
            work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue '取引先コード
            work.WF_SEL_TAISHOYM.Text = WF_TaishoYm.Value '対象年月
            work.WF_SEL_LISTCOUNT.Text = "0"

            Server.Transfer("~/LNG/mas/LNM0010SprateDetailKG.aspx")
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = ""                                             '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)    '削除

        work.WF_SEL_RECOID.Text = ""                                                      'レコードID
        work.WF_SEL_RECONAME.Text = ""                                                      'レコード名

        If WF_TORI.SelectedValue = "" Then
            work.WF_SEL_TORICODE.Text = ""                  '取引先コード
            work.WF_SEL_TORINAME.Text = ""                 '取引先名称
        Else
            work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue                         '取引先コード
            work.WF_SEL_TORINAME.Text = WF_TORI.SelectedItem.ToString                 '取引先名称
        End If

        work.WF_SEL_ORGCODE.Text = ""                                                      '部門コード
        work.WF_SEL_ORGNAME.Text = ""                                                      '部門名称
        work.WF_SEL_KASANORGCODE.Text = ""                                                      '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = ""                                                      '加算先部門名称
        work.WF_SEL_TODOKECODE.Text = ""                                                      '届先コード
        work.WF_SEL_TODOKENAME.Text = ""                                                      '届先名称
        work.WF_SEL_STYMD.Text = ""                                                      '有効開始日
        work.WF_SEL_ENDYMD.Text = ""                                                      '有効終了日
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KINGAKU.Text)   '金額
        work.WF_SEL_SYABAN.Text = ""                                                      '車番
        work.WF_SEL_TAISHOYM.Text = ""                                                      '対象年月
        work.WF_SEL_ITEMID.Text = ""                                                      '大項目
        work.WF_SEL_ITEMNAME.Text = ""                                                      '項目名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SYABARA.Text)   '車腹
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHI.Text)   '固定費
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TANKA.Text)   '単価
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KYORI.Text)   '走行距離
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KEIYU.Text)   '実勢軽油価格
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KIZYUN.Text)   '基準価格
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TANKASA.Text)   '単価差
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KAISU.Text)   '輸送回数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_COUNT.Text)   '回数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_FEE.Text)   '料金
        work.WF_SEL_BIKOU.Text = ""                                                      '備考
        work.WF_SEL_USAGECHARGE.Text = ""                                                      '燃料使用量
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SURCHARGE.Text)   'サーチャージ
        work.WF_SEL_BIKOU1.Text = ""                                                      '備考1
        work.WF_SEL_BIKOU2.Text = ""                                                      '備考2
        work.WF_SEL_BIKOU3.Text = ""                                                      '備考3

        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0010tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0010SprateHistory.aspx")
    End Sub

    ''' <summary>
    ''' 更新ボタン(北海道ガス特別料金)押下押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonKGUPD_Click()
        work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue '取引先コード
        work.WF_SEL_TORINAME.Text = WF_TORI.SelectedItem.ToString '取引先名称
        work.WF_SEL_TAISHOYM.Text = WF_TaishoYm.Value '対象年月
        work.WF_SEL_LISTCOUNT.Text = LNM0010tbl.Rows.Count.ToString()

        Server.Transfer("~/LNG/mas/LNM0010SprateDetailKG.aspx")
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNM0010tbl.Rows
            If LNS0008row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNS0008row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If String.IsNullOrEmpty(WF_GridPosition.Text) Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定
        ' 表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If
        ' 表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNM0010tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        'CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.MAPID = work.WF_SEL_CONTROLTABLE.Text
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(LNM0010tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        '北海道ガス特別料金マスタは明細行ダブルクリック遷移不可
        If work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLKG Then
            Exit Sub
        End If

        Dim WW_DBDataCheck As String = ""

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0010tbl.Rows(WW_LineCNT)("LINECNT")            '選択行

        work.WF_SEL_RECOID.Text = LNM0010tbl.Rows(WW_LineCNT)("RECOID")                                      'レコードID
        work.WF_SEL_RECONAME.Text = LNM0010tbl.Rows(WW_LineCNT)("RECONAME")                                      'レコード名
        work.WF_SEL_TORICODE.Text = LNM0010tbl.Rows(WW_LineCNT)("TORICODE")                                      '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0010tbl.Rows(WW_LineCNT)("TORINAME")                                      '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0010tbl.Rows(WW_LineCNT)("ORGCODE")                                      '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0010tbl.Rows(WW_LineCNT)("ORGNAME")                                      '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0010tbl.Rows(WW_LineCNT)("KASANORGCODE")                                      '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0010tbl.Rows(WW_LineCNT)("KASANORGNAME")                                      '加算先部門名称
        work.WF_SEL_TODOKECODE.Text = LNM0010tbl.Rows(WW_LineCNT)("TODOKECODE")                                      '届先コード
        work.WF_SEL_TODOKENAME.Text = LNM0010tbl.Rows(WW_LineCNT)("TODOKENAME")                                      '届先名称
        work.WF_SEL_STYMD.Text = LNM0010tbl.Rows(WW_LineCNT)("STYMD")                                      '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0010tbl.Rows(WW_LineCNT)("ENDYMD")                                      '有効終了日
        work.WF_SEL_KINGAKU.Text = LNM0010tbl.Rows(WW_LineCNT)("KINGAKU")                                      '金額
        work.WF_SEL_SYABAN.Text = LNM0010tbl.Rows(WW_LineCNT)("SYABAN")                                      '車番
        work.WF_SEL_TAISHOYM.Text = LNM0010tbl.Rows(WW_LineCNT)("TAISHOYM")                                      '対象年月
        work.WF_SEL_ITEMID.Text = LNM0010tbl.Rows(WW_LineCNT)("ITEMID")                                      '大項目
        work.WF_SEL_ITEMNAME.Text = LNM0010tbl.Rows(WW_LineCNT)("ITEMNAME")                                      '項目名
        work.WF_SEL_SYABARA.Text = LNM0010tbl.Rows(WW_LineCNT)("SYABARA")                                      '車腹
        work.WF_SEL_KOTEIHI.Text = LNM0010tbl.Rows(WW_LineCNT)("KOTEIHI")                                      '固定費
        work.WF_SEL_TANKA.Text = LNM0010tbl.Rows(WW_LineCNT)("TANKA")                                      '単価
        work.WF_SEL_KYORI.Text = LNM0010tbl.Rows(WW_LineCNT)("KYORI")                                      '走行距離
        work.WF_SEL_KEIYU.Text = LNM0010tbl.Rows(WW_LineCNT)("KEIYU")                                      '実勢軽油価格
        work.WF_SEL_KIZYUN.Text = LNM0010tbl.Rows(WW_LineCNT)("KIZYUN")                                      '基準価格
        work.WF_SEL_TANKASA.Text = LNM0010tbl.Rows(WW_LineCNT)("TANKASA")                                      '単価差
        work.WF_SEL_KAISU.Text = LNM0010tbl.Rows(WW_LineCNT)("KAISU")                                      '輸送回数
        work.WF_SEL_COUNT.Text = LNM0010tbl.Rows(WW_LineCNT)("COUNT")                                      '回数
        work.WF_SEL_FEE.Text = LNM0010tbl.Rows(WW_LineCNT)("FEE")                                      '料金
        work.WF_SEL_BIKOU.Text = LNM0010tbl.Rows(WW_LineCNT)("BIKOU")                                      '備考
        work.WF_SEL_USAGECHARGE.Text = LNM0010tbl.Rows(WW_LineCNT)("USAGECHARGE")                                      '燃料使用量
        work.WF_SEL_SURCHARGE.Text = LNM0010tbl.Rows(WW_LineCNT)("SURCHARGE")                                      'サーチャージ
        work.WF_SEL_BIKOU1.Text = LNM0010tbl.Rows(WW_LineCNT)("BIKOU1")                                      '備考1
        work.WF_SEL_BIKOU2.Text = LNM0010tbl.Rows(WW_LineCNT)("BIKOU2")                                      '備考2
        work.WF_SEL_BIKOU3.Text = LNM0010tbl.Rows(WW_LineCNT)("BIKOU3")                                      '備考3

        work.WF_SEL_DELFLG.Text = LNM0010tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0010tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    work.HaitaCheckHACHINOHESPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    work.HaitaCheckENEOSCOMFEE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    work.HaitaCheckTOHOKUSPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text,
                    work.WF_SEL_SYABAN.Text)
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    work.HaitaCheckSKSPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    work.HaitaCheckSKSURCHARGE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_TAISHOYM.Text)
            End Select
        End Using

        If Not isNormal(WW_DBDataCheck) Then
            Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "ITEMID"        '大項目
                    work.CODEIDGetITEM(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "ORG"              '組織コード
                    If Master.ROLE_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP))
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP))
                    End If
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "ITEMID"        '大項目
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

#Region "ﾀﾞｳﾝﾛｰﾄﾞ"

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン、ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCELPDF(ByVal WW_FILETYPE As Integer)
        'ファイル保存先
        Dim UploadRootPath As String = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
        'ディレクトリが存在しない場合は生成
        If IO.Directory.Exists(UploadRootPath) = False Then
            IO.Directory.CreateDirectory(UploadRootPath)
        End If
        '前日プリフィックスのアップロードファイルが残っていた場合は削除
        Dim targetFiles = IO.Directory.GetFiles(UploadRootPath, "*.*")
        Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
        For Each targetFile In targetFiles
            Dim targetfileName As String = IO.Path.GetFileName(targetFile)
            '今日の日付が先頭のファイル名の場合は残す
            If targetfileName.StartsWith(keepFilePrefix) Then
                Continue For
            End If
            Try
                IO.File.Delete(targetFile)
            Catch ex As Exception
                '削除時のエラーは無視
            End Try
        Next targetFile

        Dim UrlRoot As String
        'URLのルートを表示
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = 0
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLHA)).Cast(Of Integer)().Max()
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLEN)).Cast(Of Integer)().Max()
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLTO)).Cast(Of Integer)().Max()
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLKG)).Cast(Of Integer)().Max()
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLSKSP)).Cast(Of Integer)().Max()
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0010WRKINC.INOUTEXCELCOLSKSU)).Cast(Of Integer)().Max()
        End Select

        'シート名
        wb.ActiveSheet.Name = "入出力"

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0

        WW_STROW = WW_ACTIVEROW
        SetDETAIL(wb.ActiveSheet, WW_ACTIVEROW)
        WW_ENDROW = WW_ACTIVEROW - 1

        'プルダウンリスト作成
        SetPULLDOWNLIST(wb, WW_STROW, WW_ENDROW)

        '入力不可列設定
        SetCOLLOCKED(wb.ActiveSheet, WW_STROW, WW_ENDROW)

        '明細の線を引く
        Dim WW_MAXRANGE As String = wb.ActiveSheet.Cells(WW_ACTIVEROW - 1, WW_MAXCOL).Address
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Dotted
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeLeft).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeRight).LineStyle = BorderLineStyle.Thin

        '入力必須列、入力不要列網掛け設定
        SetREQUNNECEHATCHING(wb.ActiveSheet)

        'ヘッダ設定
        SetHEADER(wb, wb.ActiveSheet, WW_MAXCOL)

        'その他設定
        'wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLHA
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLEN
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLTO
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLKG
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLSKSP
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                wb.ActiveSheet.Range("A1").Value = "ID:" + LNM0010WRKINC.MAPIDLSKSU
        End Select

        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        wb.ActiveSheet.Range("B2").Value = "は入力必須"

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                wb.ActiveSheet.Range("C1").Value = "八戸特別料金マスタ一覧"
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                wb.ActiveSheet.Range("C1").Value = "ENEOS業務委託料マスタ一覧"
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                wb.ActiveSheet.Range("C1").Value = "東北電力車両別追加料金マスタ一覧"
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                wb.ActiveSheet.Range("C1").Value = "北海道ガス特別料金マスタ一覧"
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                wb.ActiveSheet.Range("C1").Value = "SK特別料金マスタ一覧"
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                wb.ActiveSheet.Range("C1").Value = "SK燃料サーチャージマスタ一覧"
        End Select

        wb.ActiveSheet.Range("C2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY)
        wb.ActiveSheet.Range("D2").Value = "は入力不要"

        '列幅自動調整
        wb.ActiveSheet.Range("A3:" + WW_MAXRANGE).EntireColumn.AutoFit()

        '印刷設定
        With wb.ActiveSheet.PageSetup
            .PrintArea = "A1:" + WW_MAXRANGE '印刷範囲
            .PaperSize = PaperSize.A4 '用紙サイズ　
            .Orientation = PageOrientation.Landscape '横向き
            '.Zoom = 80 '倍率
            .IsPercentScale = False 'FalseでFitToPages有効化
            .FitToPagesWide = 1 'すべての列を1ページに印刷
            .FitToPagesTall = 99 '設定しないと全て1ページにされる
            .LeftMargin = 16 '左余白(ポイント)
            .RightMargin = 16 '右余白(ポイント)
            .PrintTitleRows = "$3:$3" 'ページヘッダ
            .RightFooter = "&P / &N" 'ページフッタにページ番号設定
        End With

        Dim FileName As String = ""
        Dim FilePath As String
        Select Case WW_FILETYPE
            Case LNM0010WRKINC.FILETYPE.EXCEL
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        FileName = "八戸特別料金マスタ.xlsx"
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        FileName = "ENEOS業務委託料マスタ.xlsx"
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        FileName = "東北電力車両別追加料金マスタ.xlsx"
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        FileName = "北海道ガス特別料金マスタ.xlsx"
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        FileName = "SK特別料金マスタ.xlsx"
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        FileName = "SK燃料サーチャージマスタ.xlsx"
                End Select

                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            'Case LNM0010WRKINC.FILETYPE.PDF
            '    FileName = "特別料金マスタ.pdf"
            '    FilePath = IO.Path.Combine(UploadRootPath, FileName)

            '    '保存
            '    wb.Save(FilePath, SaveFileFormat.Pdf)

            '    'ダウンロード
            '    WF_PrintURL.Value = UrlRoot & FileName
            '    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
            Case Else
        End Select
    End Sub

    ''' <summary>
    ''' シート全体設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetALL(ByVal sheet As IWorksheet)
        ' ウィンドウ枠を固定
        'sheet.FreezePanes(1, 3)
        sheet.FreezePanes(3, 0)

        ' ワークシートのビューを構成
        Dim sheetView As IWorksheetView = sheet.SheetView
        'sheetView.DisplayFormulas = False
        'sheetView.DisplayRightToLeft = True
        '表示倍率
        sheetView.Zoom = 90

        '列幅
        sheet.Columns.ColumnWidth = 5
        '行幅
        sheet.Rows.RowHeight = 15.75
        'フォント
        With sheet.Columns.Font
            .Color = Color.FromArgb(0, 0, 0)
            .Name = "Meiryo UI"
            .Size = 11
        End With
        '配置
        sheet.Columns.VerticalAlignment = VerticalAlignment.Center
        'sheet.Rows.HorizontalAlignment = HorizontalAlignment.Center
    End Sub

    ''' <summary>
    ''' 入力必須列、入力不要列網掛け設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetREQUNNECEHATCHING(ByVal sheet As IWorksheet)
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLHA.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLHA.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLHA.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日

                '入力不要列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLHA.RECOID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'レコードID
                'sheet.Columns(LNM0010WRKINC.INOUTEXCELCOL.ENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '有効終了日
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLEN.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLEN.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLEN.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日

                '入力不要列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLEN.RECOID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'レコードID
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLTO.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLTO.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLTO.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLTO.SYABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番

                '入力不要列網掛け
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.TAISHOYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '対象年月
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.ITEMNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '項目名
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.RECONAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'レコード名

                '入力不要列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.RECOID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'レコードID
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLKG.ITEMID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '大項目
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSP.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSP.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日

                '入力不要列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSP.RECOID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'レコードID
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                '入力必須列網掛け
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSU.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0010WRKINC.INOUTEXCELCOLSKSU.TAISHOYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '対象年月

                '入力不要列網掛け
        End Select

        '1,2行の網掛けは消す
        sheet.Rows(0).Interior.ColorIndex = 0
        sheet.Rows(1).Interior.ColorIndex = 0
    End Sub

    ''' <summary>
    ''' 行幅設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetROWSHEIGHT(ByVal sheet As IWorksheet)

    End Sub

    ''' <summary>
    ''' ヘッダ設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetHEADER(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByVal WW_MAXCOL As Integer)
        '行幅
        sheet.Rows(0).RowHeight = 15.75 '１行目
        sheet.Rows(1).RowHeight = 15.75 '２行目
        sheet.Rows(2).RowHeight = 31.5 '３行目

        Dim WW_MAXRANGE As String = sheet.Cells(2, WW_MAXCOL).Address

        '線
        sheet.Range("A3:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Thin
        sheet.Range("A3:" + WW_MAXRANGE).Borders.Color = ColorTranslator.FromHtml(CONST_COLOR_BLACK)

        '背景色
        sheet.Range("A3:" + WW_MAXRANGE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_HEADER)

        'フォント
        sheet.Range("A3:" + WW_MAXRANGE).Font.Color = ColorTranslator.FromHtml(CONST_COLOR_FONT_HEADER)
        sheet.Range("A3:" + WW_MAXRANGE).Font.Bold = True

        '配置
        sheet.Range("A3:" + WW_MAXRANGE).HorizontalAlignment = HorizontalAlignment.Center

        'オートフィルタ
        sheet.Range("A3:" + WW_MAXRANGE).AutoFilter()

        '折り返して全体を表示
        'sheet.Range("J1:M1").WrapText = True

        '値
        Dim WW_HEADERROW As Integer = 2

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECOID).Value = "レコードID"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECONAME).Value = "レコード名"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.STYMD).Value = "（必須）有効開始日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.ENDYMD).Value = "有効終了日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.KINGAKU).Value = "金額"
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECOID).Value = "レコードID"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECONAME).Value = "レコード名"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.STYMD).Value = "（必須）有効開始日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.ENDYMD).Value = "有効終了日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.KINGAKU).Value = "金額"
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.STYMD).Value = "（必須）有効開始日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.ENDYMD).Value = "有効終了日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.SYABAN).Value = "（必須）車番"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.KOTEIHI).Value = "固定費"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.KAISU).Value = "回数"
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKECODE).Value = "届先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKENAME).Value = "届先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.SYABAN).Value = "車番"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TAISHOYM).Value = "（必須）対象年月"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMID).Value = "大項目"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMNAME).Value = "（必須）項目名"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECOID).Value = "レコードID"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECONAME).Value = "（必須）レコード名"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.TANKA).Value = "単価"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.COUNT).Value = "回数"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.FEE).Value = "料金"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.BIKOU).Value = "備考"
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECOID).Value = "レコードID"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECONAME).Value = "レコード名"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKECODE).Value = "届先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKENAME).Value = "届先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.STYMD).Value = "（必須）有効開始日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ENDYMD).Value = "有効終了日"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.SYABARA).Value = "車腹"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KOTEIHI).Value = "固定費"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU1).Value = "備考1"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU2).Value = "備考2"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU3).Value = "備考3"
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKECODE).Value = "届先コード"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKENAME).Value = "届先名称"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TAISHOYM).Value = "（必須）対象年月"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KYORI).Value = "走行距離"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KEIYU).Value = "実勢軽油価格"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KIZYUN).Value = "基準価格"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TANKASA).Value = "単価差"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KAISU).Value = "輸送回数"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.USAGECHARGE).Value = "燃料使用量"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.SURCHARGE).Value = "サーチャージ"
                sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.BIKOU1).Value = "備考1"
        End Select

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

                    '有効終了日
                    WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
                    '選択比較項目-発荷主コード
                    sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.ENDYMD).AddComment(WW_TEXT)
                    With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLHA.ENDYMD).Comment.Shape
                        .Width = 150
                        .Height = 30
                    End With
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

                    '有効終了日
                    WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
                    '選択比較項目-発荷主コード
                    sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.ENDYMD).AddComment(WW_TEXT)
                    With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLEN.ENDYMD).Comment.Shape
                        .Width = 150
                        .Height = 30
                    End With
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

                    '有効終了日
                    WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
                    '選択比較項目-発荷主コード
                    sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.ENDYMD).AddComment(WW_TEXT)
                    With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLTO.ENDYMD).Comment.Shape
                        .Width = 150
                        .Height = 30
                    End With
                Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

                    '有効終了日
                    WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
                    '選択比較項目-発荷主コード
                    sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ENDYMD).AddComment(WW_TEXT)
                    With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ENDYMD).Comment.Shape
                        .Width = 150
                        .Height = 30
                    End With
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If

            End Select
        End Using

    End Sub

    ''' <summary>
    ''' プルダウンリスト作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetPULLDOWNLIST(ByVal wb As Workbook, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'メインシートを取得
        Dim mainsheet As IWorksheet = wb.ActiveSheet
        'サブシートを作成
        Dim subsheet As IWorksheet = wb.Worksheets.Add()
        subsheet.Name = CONST_PULLDOWNSHEETNAME

        Dim WW_COL As String = ""
        Dim WW_MAIN_STRANGE As IRange
        Dim WW_MAIN_ENDRANGE As IRange
        Dim WW_SUB_STRANGE As IRange
        Dim WW_SUB_ENDRANGE As IRange
        Dim WW_FIXENDROW As Integer = 0
        Dim WW_FORMULA1 As String = ""

        '○入力リスト取得
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
        End Select

        'メインシートをアクティブにする
        mainsheet.Activate()
        'サブシートを非表示にする
        subsheet.Visible = Visibility.Hidden
    End Sub

    ''' <summary>
    ''' 入力不可列設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetCOLLOCKED(ByVal sheet As IWorksheet, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'Dim WW_STRANGE As IRange
        'Dim WW_ENDRANGE As IRange

        ''シートの保護を解除
        'sheet.Unprotect()
        'sheet.Cells.Locked = False

        ''枝番
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0010WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0010WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'sheet.Range(WW_STRANGE.Address & ":" & WW_ENDRANGE.Address).Locked = True

        ''シートを保護する
        'sheet.Protect()
    End Sub


    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNM0010tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECOID).Value = Row("RECOID") 'レコードID
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECONAME).Value = Row("RECONAME") 'レコード名
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.STYMD).Value = Row("STYMD") '有効開始日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.ENDYMD).Value = Row("ENDYMD") '有効終了日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLHA.KINGAKU).Value = Row("KINGAKU") '金額
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECOID).Value = Row("RECOID") 'レコードID
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECONAME).Value = Row("RECONAME") 'レコード名
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.STYMD).Value = Row("STYMD") '有効開始日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.ENDYMD).Value = Row("ENDYMD") '有効終了日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLEN.KINGAKU).Value = Row("KINGAKU") '金額
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.STYMD).Value = Row("STYMD") '有効開始日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.ENDYMD).Value = Row("ENDYMD") '有効終了日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.SYABAN).Value = Row("SYABAN") '車番
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.KOTEIHI).Value = Row("KOTEIHI") '固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLTO.KAISU).Value = Row("KAISU") '回数
                Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKECODE).Value = Row("TODOKECODE") '届先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKENAME).Value = Row("TODOKENAME") '届先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.SYABAN).Value = Row("SYABAN") '車番
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TAISHOYM).Value = Row("TAISHOYM") '対象年月
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMID).Value = Row("ITEMID") '大項目
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMNAME).Value = Row("ITEMNAME") '項目名
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECOID).Value = Row("RECOID") 'レコードID
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECONAME).Value = Row("RECONAME") 'レコード名
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.TANKA).Value = Row("TANKA") '単価
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.COUNT).Value = Row("COUNT") '回数
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.FEE).Value = Row("FEE") '料金
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLKG.BIKOU).Value = Row("BIKOU") '備考
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECOID).Value = Row("RECOID") 'レコードID
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECONAME).Value = Row("RECONAME") 'レコード名
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKECODE).Value = Row("TODOKECODE") '届先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKENAME).Value = Row("TODOKENAME") '届先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.STYMD).Value = Row("STYMD") '有効開始日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ENDYMD).Value = Row("ENDYMD") '有効終了日
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.SYABARA).Value = Row("SYABARA") '車腹
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KOTEIHI).Value = Row("KOTEIHI") '固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU1).Value = Row("BIKOU1") '備考1
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU2).Value = Row("BIKOU2") '備考2
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU3).Value = Row("BIKOU3") '備考3
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKECODE).Value = Row("TODOKECODE") '届先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKENAME).Value = Row("TODOKENAME") '届先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TAISHOYM).Value = Row("TAISHOYM") '対象年月
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KYORI).Value = Row("KYORI") '走行距離
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KEIYU).Value = Row("KEIYU") '実勢軽油価格
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KIZYUN).Value = Row("KIZYUN") '基準価格
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TANKASA).Value = Row("TANKASA") '単価差
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KAISU).Value = Row("KAISU") '輸送回数
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.USAGECHARGE).Value = Row("USAGECHARGE") '燃料使用量
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.SURCHARGE).Value = Row("SURCHARGE") 'サーチャージ
                    sheet.Cells(WW_ACTIVEROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.BIKOU1).Value = Row("BIKOU1") '備考1
            End Select

            WW_ACTIVEROW += 1
        Next
    End Sub

    ''' <summary>
    ''' セル表示用のコメント取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_CNT"></param>
    ''' <remarks></remarks>
    Protected Sub COMMENT_get(ByVal SQLcon As MySqlConnection,
                                   ByVal I_FIELD As String,
                                   ByRef O_TEXT As String,
                                   ByRef O_CNT As Integer)

        O_TEXT = ""
        O_CNT = 0

        Dim WW_PrmData As New Hashtable
        Dim WW_PrmDataList = New StringBuilder
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""

        With leftview
            Select Case I_FIELD
                Case "DELFLG"   '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
            End Select
            .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

            For i As Integer = 0 To .WF_LeftListBox.Items.Count - 1
                If Not Trim(.WF_LeftListBox.Items(i).Text) = "" Then
                    WW_PrmDataList.AppendLine(.WF_LeftListBox.Items(i).Value + "：" + .WF_LeftListBox.Items(i).Text)
                End If
            Next

            O_TEXT = WW_PrmDataList.ToString
            O_CNT = .WF_LeftListBox.Items.Count

        End With
    End Sub

    ''' <summary>
    ''' プルダウンシートにリストを作成
    ''' </summary>
    ''' <param name="sheet"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_COL"></param>
    ''' <remarks></remarks>
    Protected Sub SETFIXVALUELIST(ByVal sheet As IWorksheet, ByVal I_FIELD As String, ByVal I_COL As Integer, ByRef WW_FIXENDROW As Integer)

        Dim WW_PrmData As New Hashtable
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""
        Dim WW_ROW As Integer = 0

        With leftview
            Select Case I_FIELD
                Case "DELFLG"   '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
            End Select
            .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

            For i As Integer = 0 To .WF_LeftListBox.Items.Count - 1
                If Not Trim(.WF_LeftListBox.Items(i).Text) = "" Then
                    sheet.Cells(WW_ROW, I_COL).Value = .WF_LeftListBox.Items(i).Value
                    WW_ROW += 1
                End If
            Next

            WW_FIXENDROW = WW_ROW - 1

        End With
    End Sub


#End Region
    '******************************************************************************
    '***  更新処理                                                              ***
    '******************************************************************************
#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\特別料金マスタ一括アップロードテスト.xlsx"

        Dim DATENOW As DateTime
        Dim WW_ErrData As Boolean = False

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ErrSW)
            If WW_ErrSW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            DATENOW = Date.Now
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")
            Dim WW_DBDataCheck As String = ""
            Dim WW_BeforeMAXSTYMD As String = ""
            Dim WW_STYMD_SAVE As String = ""
            Dim WW_ENDYMD_SAVE As String = ""

            For Each Row As DataRow In LNM0010Exceltbl.Rows
                '大項目が無い場合生成
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        If Row("ITEMID") = "" Then
                            '既存に同一名称が存在する場合そのIDを取得
                            Dim WW_ITEMID As String = ""
                            CODENAME_get("ITEMID", Row("ITEMNAME"), Row("ITEMID"), WW_RtnSW)
                            '既存に同一名称が存在しない場合新しく作成
                            If Row("ITEMID") = "" Then
                                Row("ITEMID") = GenerateITEMID(Row)
                                'レコードIDも新しく生成するため空にする
                                Row("RECOID") = ""
                            End If
                        End If
                End Select

                'レコードIDがない場合生成
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLHACHINOHESPRATE)
                        End If
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLENEOSCOMFEE)
                        End If
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLKGSPRATE)
                        End If
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLSKSPRATE)
                        End If
                End Select

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0010WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0010WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0010WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        Continue For
                    End If


                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0010WRKINC.MAPIDLHA, LNM0010WRKINC.MAPIDLEN, LNM0010WRKINC.MAPIDLTO,
                             LNM0010WRKINC.MAPIDLSKSP

#Region "八戸特別料金マスタ、ENEOS業務委託料マスタ、東北電力車両別追加料金マスタ、SK特別料金マスタ"
                            '有効開始日、有効終了日更新
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0010WRKINC.GetSTYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                                                       Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            WF_AUTOENDYMD.Value = ""

                            Select Case True
                                    'DBに登録されている有効開始日が無かった場合
                                Case WW_BeforeMAXSTYMD = ""
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                        '同一の場合
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                                Case WW_BeforeMAXSTYMD < CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                    'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする
                                    '変更後の有効開始日退避
                                    WW_STYMD_SAVE = Row("STYMD")
                                    '変更後の有効終了日退避
                                    WW_ENDYMD_SAVE = Row("ENDYMD")
                                    '変更後テーブルに変更前の有効開始日格納
                                    Row("STYMD") = WW_BeforeMAXSTYMD
                                    '変更後テーブルに更新用の有効終了日格納
                                    Row("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                                    '履歴テーブルに変更前データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                                     Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                Case Else
                                    '有効終了日に有効開始日の月の末日を入れる
                                    Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                    WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                            End Select
#End Region
                    End Select


                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If
                End If
            Next

            'エラーデータが存在した場合Rightboxを表示する
            If WW_ErrData = True Then
                WF_RightboxOpen.Value = "Open"
            Else
                rightview.InitMemoErrList(WW_Dummy)
            End If

            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

        End Using
    End Sub


    ''' <summary>
    ''' ｱｯﾌﾟﾛｰﾄﾞボタン押下処理
    ''' </summary>
    Protected Sub WF_ButtonUPLOAD_Click()
        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "単価マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0010Exceltbl) Then
            LNM0010Exceltbl = New DataTable
        End If
        If LNM0010Exceltbl.Columns.Count <> 0 Then
            LNM0010Exceltbl.Columns.Clear()
        End If
        LNM0010Exceltbl.Clear()

        '添付ファイルテーブルの初期化
        If IsNothing(UploadFileTbl) Then
            UploadFileTbl = New DataTable
        End If
        If UploadFileTbl.Columns.Count <> 0 Then
            UploadFileTbl.Columns.Clear()
        End If
        UploadFileTbl.Clear()

        '添付ファイルテーブル
        UploadFileTbl.Columns.Add("FILENAME", Type.GetType("System.String"))
        UploadFileTbl.Columns.Add("FILEPATH", Type.GetType("System.String"))

        'アップロードファイル名と拡張子を取得する
        Dim fileName As String = ""
        fileName = WF_UPLOAD_BTN.FileName

        Dim fileNameParts = fileName.Split(CType(".", Char()))
        Dim fileExtention = fileNameParts(fileNameParts.Length - 1)

        'アップロードフォルダ作成
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\SPRATEEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            Try
                IO.File.Delete(fileUploadPath & "\" & file.Name)
            Catch ex As Exception
            End Try
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "SPRATEEXCEL_TMP_"

        'ファイルパスの決定
        Dim newfileName As String = fileNameHead & DateTime.Now.ToString("yyyyMMddHHmmss") & "." & fileExtention
        Dim filePath As String = fileUploadPath & "\" & newfileName
        'ファイルの保存
        WF_UPLOAD_BTN.SaveAs(filePath)

        Dim DATENOW As DateTime
        Dim WW_ErrData As Boolean = False

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続
            'Excelデータ格納用テーブルに格納する
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ErrSW)
            If WW_ErrSW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            DATENOW = Date.Now
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")

            '件数初期化
            Dim WW_UplInsCnt As Integer = 0                             'アップロード件数(登録)
            Dim WW_UplUpdCnt As Integer = 0                             'アップロード件数(更新)
            Dim WW_UplDelCnt As Integer = 0                             'アップロード件数(削除)
            Dim WW_UplErrCnt As Integer = 0                             'アップロード件数(エラー)
            Dim WW_UplUnnecessaryCnt As Integer = 0                     'アップロード件数(更新不要)
            Dim WW_DBDataCheck As String = ""
            Dim WW_BeforeMAXSTYMD As String = ""
            Dim WW_STYMD_SAVE As String = ""
            Dim WW_ENDYMD_SAVE As String = ""

            For Each Row As DataRow In LNM0010Exceltbl.Rows
                '大項目が無い場合生成
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        If Row("ITEMID") = "" Then
                            '既存に同一名称が存在する場合そのIDを取得
                            Dim WW_ITEMID As String = ""
                            CODENAME_get("ITEMID", Row("ITEMNAME"), Row("ITEMID"), WW_RtnSW)
                            '既存に同一名称が存在しない場合新しく作成
                            If Row("ITEMID") = "" Then
                                Row("ITEMID") = GenerateITEMID(Row)
                                'レコードIDも新しく生成するため空にする
                                Row("RECOID") = ""
                            End If
                        End If
                End Select

                'レコードIDがない場合生成
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLHACHINOHESPRATE)
                        End If
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLENEOSCOMFEE)
                        End If
                    Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLKGSPRATE)
                        End If
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        If Row("RECOID") = "" Then
                            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLSKSPRATE)
                        End If
                End Select

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0010WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0010WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0010WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0010WRKINC.MAPIDLHA, LNM0010WRKINC.MAPIDLEN, LNM0010WRKINC.MAPIDLTO,
                             LNM0010WRKINC.MAPIDLSKSP

#Region "八戸特別料金マスタ、ENEOS業務委託料マスタ、東北電力車両別追加料金マスタ、SK特別料金マスタ"
                            '有効開始日、有効終了日更新
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0010WRKINC.GetSTYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                                                       Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            WF_AUTOENDYMD.Value = ""

                            Select Case True
                                    'DBに登録されている有効開始日が無かった場合
                                Case WW_BeforeMAXSTYMD = ""
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                        '同一の場合
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                                Case WW_BeforeMAXSTYMD < CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                    'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする
                                    '変更後の有効開始日退避
                                    WW_STYMD_SAVE = Row("STYMD")
                                    '変更後の有効終了日退避
                                    WW_ENDYMD_SAVE = Row("ENDYMD")
                                    '変更後テーブルに変更前の有効開始日格納
                                    Row("STYMD") = WW_BeforeMAXSTYMD
                                    '変更後テーブルに更新用の有効終了日格納
                                    Row("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                                    '履歴テーブルに変更前データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                                     Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0010WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                                Case Else
                                    '有効終了日に有効開始日の月の末日を入れる
                                    Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                    WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                            End Select
#End Region
                    End Select

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If


                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規の場合
                            WW_UplInsCnt += 1
                        Case Else
                            WW_UplUpdCnt += 1
                    End Select

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                Else '同一データの場合
                    WW_UplUnnecessaryCnt += 1
                End If
            Next

            Dim WW_OutPutCount As String
            WW_OutPutCount = WW_UplInsCnt.ToString + "件登録完了 " _
                           + WW_UplUpdCnt.ToString + "件更新完了 " _
                           + WW_UplDelCnt.ToString + "件削除完了 " _
                           + WW_UplUnnecessaryCnt.ToString + "件更新不要 " _
                           + WW_UplErrCnt.ToString + "件エラーが起きました。"

            Dim WW_GetErrorReport As String = rightview.GetErrorReport()

            'エラーデータが存在した場合
            If WW_ErrData = True Then
                rightview.InitMemoErrList(WW_Dummy)
                rightview.AddErrorReport(WW_OutPutCount)
                rightview.AddErrorReport(WW_GetErrorReport)
            Else
                rightview.InitMemoErrList(WW_Dummy)
                rightview.AddErrorReport(WW_OutPutCount)
            End If

            'Rightboxを表示する
            WF_RightboxOpen.Value = "Open"

            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

        End Using
    End Sub

    ''' <summary>
    ''' アップロードしたファイルの内容をExcelデータ格納用テーブルに格納する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="FilePath"></param>
    ''' <remarks></remarks>
    Protected Sub SetExceltbl(ByVal SQLcon As MySqlConnection, ByVal FilePath As String, ByRef O_RTN As String)
        Dim DataTypeHT As Hashtable = New Hashtable

        'Excelファイルを開く
        Dim fileStream As FileStream
        fileStream = File.OpenRead(FilePath)

        'ファイル内のシート名を取得
        Dim sheetname = GrapeCity.Documents.Excel.Workbook.GetNames(fileStream)

        'データを取得
        Dim WW_EXCELDATA = GrapeCity.Documents.Excel.Workbook.ImportData(fileStream, sheetname(0))

        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_DATATYPE As String = ""
        Dim WW_RESULT As Boolean

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        'アップロードしたExcelファイルがどのテーブルのデータか確認する(Excel左上のIDを確認する)
        Select Case True
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLHA) >= 0 '八戸特別料金マスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLHA
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLEN) >= 0 'ENEOS業務委託料マスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLEN
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLTO) >= 0 '東北電力車両別追加料金マスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLTO
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLKG) >= 0 '北海道ガス特別料金マスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLKG
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLSKSP) >= 0 'SK特別料金マスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLSKSP
            Case WW_EXCELDATA(0, 0).IndexOf(LNM0010WRKINC.MAPIDLSKSU) >= 0 'SK燃料サーチャージマスタ
                work.WF_SEL_CONTROLTABLE.Text = LNM0010WRKINC.MAPIDLSKSU
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,RECOID  ")
                SQLStr.AppendLine("        ,RECONAME  ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,STYMD  ")
                SQLStr.AppendLine("        ,ENDYMD  ")
                SQLStr.AppendLine("        ,KINGAKU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0010_HACHINOHESPRATE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_HACHINOHESPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010_HACHINOHESPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,RECOID  ")
                SQLStr.AppendLine("        ,RECONAME  ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,STYMD  ")
                SQLStr.AppendLine("        ,ENDYMD  ")
                SQLStr.AppendLine("        ,KINGAKU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0011_ENEOSCOMFEE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0011_ENEOSCOMFEE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,STYMD  ")
                SQLStr.AppendLine("        ,ENDYMD  ")
                SQLStr.AppendLine("        ,SYABAN  ")
                SQLStr.AppendLine("        ,KOTEIHI  ")
                SQLStr.AppendLine("        ,KAISU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0012_TOHOKUSPRATE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0012_TOHOKUSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,TODOKECODE  ")
                SQLStr.AppendLine("        ,TODOKENAME  ")
                SQLStr.AppendLine("        ,SYABAN  ")
                SQLStr.AppendLine("        ,TAISHOYM  ")
                SQLStr.AppendLine("        ,ITEMID  ")
                SQLStr.AppendLine("        ,ITEMNAME  ")
                SQLStr.AppendLine("        ,RECOID  ")
                SQLStr.AppendLine("        ,RECONAME  ")
                SQLStr.AppendLine("        ,TANKA  ")
                SQLStr.AppendLine("        ,COUNT  ")
                SQLStr.AppendLine("        ,FEE  ")
                SQLStr.AppendLine("        ,BIKOU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0013_KGSPRATE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,RECOID  ")
                SQLStr.AppendLine("        ,RECONAME  ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,TODOKECODE  ")
                SQLStr.AppendLine("        ,TODOKENAME  ")
                SQLStr.AppendLine("        ,STYMD  ")
                SQLStr.AppendLine("        ,ENDYMD  ")
                SQLStr.AppendLine("        ,SYABARA  ")
                SQLStr.AppendLine("        ,KOTEIHI  ")
                SQLStr.AppendLine("        ,BIKOU1  ")
                SQLStr.AppendLine("        ,BIKOU2  ")
                SQLStr.AppendLine("        ,BIKOU3  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0014_SKSPRATE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0014_SKSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("   0   AS LINECNT ")
                SQLStr.AppendLine("        ,TORICODE  ")
                SQLStr.AppendLine("        ,TORINAME  ")
                SQLStr.AppendLine("        ,ORGCODE  ")
                SQLStr.AppendLine("        ,ORGNAME  ")
                SQLStr.AppendLine("        ,KASANORGCODE  ")
                SQLStr.AppendLine("        ,KASANORGNAME  ")
                SQLStr.AppendLine("        ,TODOKECODE  ")
                SQLStr.AppendLine("        ,TODOKENAME  ")
                SQLStr.AppendLine("        ,TAISHOYM  ")
                SQLStr.AppendLine("        ,KYORI  ")
                SQLStr.AppendLine("        ,KEIYU  ")
                SQLStr.AppendLine("        ,KIZYUN  ")
                SQLStr.AppendLine("        ,TANKASA  ")
                SQLStr.AppendLine("        ,KAISU  ")
                SQLStr.AppendLine("        ,USAGECHARGE  ")
                SQLStr.AppendLine("        ,SURCHARGE  ")
                SQLStr.AppendLine("        ,BIKOU1  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0015_SKSURCHARGE ")
                SQLStr.AppendLine(" LIMIT 0 ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNM0010Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                            Next
                        End Using
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0015_SKSURCHARGE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
        End Select

        Dim LNM0010Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    'レコードID
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECOID))
                    WW_DATATYPE = DataTypeHT("RECOID")
                    LNM0010Exceltblrow("RECOID") = LNM0010WRKINC.DataConvert("レコードID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'レコード名
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.RECONAME))
                    WW_DATATYPE = DataTypeHT("RECONAME")
                    LNM0010Exceltblrow("RECONAME") = LNM0010WRKINC.DataConvert("レコード名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効開始日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.STYMD))
                    WW_DATATYPE = DataTypeHT("STYMD")
                    LNM0010Exceltblrow("STYMD") = LNM0010WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効終了日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.ENDYMD))
                    WW_DATATYPE = DataTypeHT("ENDYMD")
                    LNM0010Exceltblrow("ENDYMD") = LNM0010WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '金額
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.KINGAKU))
                    WW_DATATYPE = DataTypeHT("KINGAKU")
                    LNM0010Exceltblrow("KINGAKU") = LNM0010WRKINC.DataConvert("金額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLHA.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    'レコードID
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECOID))
                    WW_DATATYPE = DataTypeHT("RECOID")
                    LNM0010Exceltblrow("RECOID") = LNM0010WRKINC.DataConvert("レコードID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'レコード名
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.RECONAME))
                    WW_DATATYPE = DataTypeHT("RECONAME")
                    LNM0010Exceltblrow("RECONAME") = LNM0010WRKINC.DataConvert("レコード名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効開始日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.STYMD))
                    WW_DATATYPE = DataTypeHT("STYMD")
                    LNM0010Exceltblrow("STYMD") = LNM0010WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効終了日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.ENDYMD))
                    WW_DATATYPE = DataTypeHT("ENDYMD")
                    LNM0010Exceltblrow("ENDYMD") = LNM0010WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '金額
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.KINGAKU))
                    WW_DATATYPE = DataTypeHT("KINGAKU")
                    LNM0010Exceltblrow("KINGAKU") = LNM0010WRKINC.DataConvert("金額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLEN.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効開始日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.STYMD))
                    WW_DATATYPE = DataTypeHT("STYMD")
                    LNM0010Exceltblrow("STYMD") = LNM0010WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効終了日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.ENDYMD))
                    WW_DATATYPE = DataTypeHT("ENDYMD")
                    LNM0010Exceltblrow("ENDYMD") = LNM0010WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車番
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.SYABAN))
                    WW_DATATYPE = DataTypeHT("SYABAN")
                    LNM0010Exceltblrow("SYABAN") = LNM0010WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.KOTEIHI))
                    WW_DATATYPE = DataTypeHT("KOTEIHI")
                    LNM0010Exceltblrow("KOTEIHI") = LNM0010WRKINC.DataConvert("固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '回数
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.KAISU))
                    WW_DATATYPE = DataTypeHT("KAISU")
                    LNM0010Exceltblrow("KAISU") = LNM0010WRKINC.DataConvert("回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLTO.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKECODE))
                    WW_DATATYPE = DataTypeHT("TODOKECODE")
                    LNM0010Exceltblrow("TODOKECODE") = LNM0010WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TODOKENAME))
                    WW_DATATYPE = DataTypeHT("TODOKENAME")
                    LNM0010Exceltblrow("TODOKENAME") = LNM0010WRKINC.DataConvert("届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車番
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.SYABAN))
                    WW_DATATYPE = DataTypeHT("SYABAN")
                    LNM0010Exceltblrow("SYABAN") = LNM0010WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '対象年月
                    'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TAISHOYM))
                    WW_TEXT = Replace(Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TAISHOYM)), "/", ""), "／", "")
                    WW_DATATYPE = DataTypeHT("TAISHOYM")
                    LNM0010Exceltblrow("TAISHOYM") = LNM0010WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '大項目
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMID))
                    WW_DATATYPE = DataTypeHT("ITEMID")
                    LNM0010Exceltblrow("ITEMID") = LNM0010WRKINC.DataConvert("大項目", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '項目名
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.ITEMNAME))
                    WW_DATATYPE = DataTypeHT("ITEMNAME")
                    LNM0010Exceltblrow("ITEMNAME") = LNM0010WRKINC.DataConvert("項目名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'レコードID
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECOID))
                    WW_DATATYPE = DataTypeHT("RECOID")
                    LNM0010Exceltblrow("RECOID") = LNM0010WRKINC.DataConvert("レコードID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'レコード名
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.RECONAME))
                    WW_DATATYPE = DataTypeHT("RECONAME")
                    LNM0010Exceltblrow("RECONAME") = LNM0010WRKINC.DataConvert("レコード名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '単価
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.TANKA))
                    WW_DATATYPE = DataTypeHT("TANKA")
                    LNM0010Exceltblrow("TANKA") = LNM0010WRKINC.DataConvert("単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '回数
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.COUNT))
                    WW_DATATYPE = DataTypeHT("COUNT")
                    LNM0010Exceltblrow("COUNT") = LNM0010WRKINC.DataConvert("回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '料金
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.FEE))
                    WW_DATATYPE = DataTypeHT("FEE")
                    LNM0010Exceltblrow("FEE") = LNM0010WRKINC.DataConvert("料金", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.BIKOU))
                    WW_DATATYPE = DataTypeHT("BIKOU")
                    LNM0010Exceltblrow("BIKOU") = LNM0010WRKINC.DataConvert("備考", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLKG.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    'レコードID
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECOID))
                    WW_DATATYPE = DataTypeHT("RECOID")
                    LNM0010Exceltblrow("RECOID") = LNM0010WRKINC.DataConvert("レコードID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'レコード名
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.RECONAME))
                    WW_DATATYPE = DataTypeHT("RECONAME")
                    LNM0010Exceltblrow("RECONAME") = LNM0010WRKINC.DataConvert("レコード名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKECODE))
                    WW_DATATYPE = DataTypeHT("TODOKECODE")
                    LNM0010Exceltblrow("TODOKECODE") = LNM0010WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.TODOKENAME))
                    WW_DATATYPE = DataTypeHT("TODOKENAME")
                    LNM0010Exceltblrow("TODOKENAME") = LNM0010WRKINC.DataConvert("届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効開始日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.STYMD))
                    WW_DATATYPE = DataTypeHT("STYMD")
                    LNM0010Exceltblrow("STYMD") = LNM0010WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '有効終了日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.ENDYMD))
                    WW_DATATYPE = DataTypeHT("ENDYMD")
                    LNM0010Exceltblrow("ENDYMD") = LNM0010WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車腹
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.SYABARA))
                    WW_DATATYPE = DataTypeHT("SYABARA")
                    LNM0010Exceltblrow("SYABARA") = LNM0010WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.KOTEIHI))
                    WW_DATATYPE = DataTypeHT("KOTEIHI")
                    LNM0010Exceltblrow("KOTEIHI") = LNM0010WRKINC.DataConvert("固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考1
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU1))
                    WW_DATATYPE = DataTypeHT("BIKOU1")
                    LNM0010Exceltblrow("BIKOU1") = LNM0010WRKINC.DataConvert("備考1", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考2
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU2))
                    WW_DATATYPE = DataTypeHT("BIKOU2")
                    LNM0010Exceltblrow("BIKOU2") = LNM0010WRKINC.DataConvert("備考2", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考3
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.BIKOU3))
                    WW_DATATYPE = DataTypeHT("BIKOU3")
                    LNM0010Exceltblrow("BIKOU3") = LNM0010WRKINC.DataConvert("備考3", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSP.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0010Exceltblrow = LNM0010Exceltbl.NewRow

                    'LINECNT
                    LNM0010Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0010Exceltblrow("TORICODE") = LNM0010WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0010Exceltblrow("TORINAME") = LNM0010WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0010Exceltblrow("ORGCODE") = LNM0010WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0010Exceltblrow("ORGNAME") = LNM0010WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0010Exceltblrow("KASANORGCODE") = LNM0010WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0010Exceltblrow("KASANORGNAME") = LNM0010WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKECODE))
                    WW_DATATYPE = DataTypeHT("TODOKECODE")
                    LNM0010Exceltblrow("TODOKECODE") = LNM0010WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '届先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TODOKENAME))
                    WW_DATATYPE = DataTypeHT("TODOKENAME")
                    LNM0010Exceltblrow("TODOKENAME") = LNM0010WRKINC.DataConvert("届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '対象年月
                    'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TAISHOYM))
                    WW_TEXT = Replace(Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TAISHOYM)), "/", ""), "／", "")
                    WW_DATATYPE = DataTypeHT("TAISHOYM")
                    LNM0010Exceltblrow("TAISHOYM") = LNM0010WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '走行距離
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KYORI))
                    WW_DATATYPE = DataTypeHT("KYORI")
                    LNM0010Exceltblrow("KYORI") = LNM0010WRKINC.DataConvert("走行距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '実勢軽油価格
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KEIYU))
                    WW_DATATYPE = DataTypeHT("KEIYU")
                    LNM0010Exceltblrow("KEIYU") = LNM0010WRKINC.DataConvert("実勢軽油価格", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '基準価格
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KIZYUN))
                    WW_DATATYPE = DataTypeHT("KIZYUN")
                    LNM0010Exceltblrow("KIZYUN") = LNM0010WRKINC.DataConvert("基準価格", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '単価差
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.TANKASA))
                    WW_DATATYPE = DataTypeHT("TANKASA")
                    LNM0010Exceltblrow("TANKASA") = LNM0010WRKINC.DataConvert("単価差", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '輸送回数
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.KAISU))
                    WW_DATATYPE = DataTypeHT("KAISU")
                    LNM0010Exceltblrow("KAISU") = LNM0010WRKINC.DataConvert("輸送回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '燃料使用量
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.USAGECHARGE))
                    WW_DATATYPE = DataTypeHT("USAGECHARGE")
                    LNM0010Exceltblrow("USAGECHARGE") = LNM0010WRKINC.DataConvert("燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    'サーチャージ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.SURCHARGE))
                    WW_DATATYPE = DataTypeHT("SURCHARGE")
                    LNM0010Exceltblrow("SURCHARGE") = LNM0010WRKINC.DataConvert("サーチャージ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考1
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.BIKOU1))
                    WW_DATATYPE = DataTypeHT("BIKOU1")
                    LNM0010Exceltblrow("BIKOU1") = LNM0010WRKINC.DataConvert("備考1", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0010WRKINC.INOUTEXCELCOLSKSU.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0010Exceltblrow("DELFLG") = LNM0010WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0010Exceltbl.Rows.Add(LNM0010Exceltblrow)

                Next
#End Region
        End Select
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(RECONAME, '')             = @RECONAME ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
                End If
                SQLStr.AppendLine("    AND  COALESCE(KINGAKU, '0')             = @KINGAKU ")
                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日                  
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        End If

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_KOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010_KOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(RECONAME, '')             = @RECONAME ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
                End If
                SQLStr.AppendLine("    AND  COALESCE(KINGAKU, '0')             = @KINGAKU ")
                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日                  
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        End If

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0011_ENEOSCOMFEE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
                End If
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHI, '0')             = @KOTEIHI ")
                SQLStr.AppendLine("    AND  COALESCE(KAISU, '0')             = @KAISU ")
                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
                        P_KAISU.Value = WW_ROW("KAISU")           '回数
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        End If

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0012_TOHOKUSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(ITEMID, '')             = @ITEMID ")
                SQLStr.AppendLine("    AND  COALESCE(ITEMNAME, '')             = @ITEMNAME ")
                SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(RECONAME, '')             = @RECONAME ")
                SQLStr.AppendLine("    AND  COALESCE(TANKA, '0')             = @TANKA ")
                SQLStr.AppendLine("    AND  COALESCE(COUNT, '0')             = @COUNT ")
                SQLStr.AppendLine("    AND  COALESCE(FEE, '0')             = @FEE ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU, '')             = @BIKOU ")
                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                        Dim P_ITEMID As MySqlParameter = SQLcmd.Parameters.Add("@ITEMID", MySqlDbType.VarChar, 2)     '大項目
                        Dim P_ITEMNAME As MySqlParameter = SQLcmd.Parameters.Add("@ITEMNAME", MySqlDbType.VarChar, 100)     '項目名
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 5)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)     '単価
                        Dim P_COUNT As MySqlParameter = SQLcmd.Parameters.Add("@COUNT", MySqlDbType.Decimal, 3)     '回数
                        Dim P_FEE As MySqlParameter = SQLcmd.Parameters.Add("@FEE", MySqlDbType.Decimal, 8)     '料金
                        Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考

                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_ITEMID.Value = WW_ROW("ITEMID")           '大項目
                        P_ITEMNAME.Value = WW_ROW("ITEMNAME")           '項目名
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TANKA.Value = WW_ROW("TANKA")           '単価
                        P_COUNT.Value = WW_ROW("COUNT")           '回数
                        P_FEE.Value = WW_ROW("FEE")           '料金
                        P_BIKOU.Value = WW_ROW("BIKOU")           '備考

                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(RECONAME, '')             = @RECONAME ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
                End If

                SQLStr.AppendLine("    AND  COALESCE(SYABARA, '')             = @SYABARA ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHI, '0')             = @KOTEIHI ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU1, '')             = @BIKOU1 ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU2, '')             = @BIKOU2 ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU3, '')             = @BIKOU3 ")

                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1
                        Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 50)     '備考2
                        Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 50)     '備考3

                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
                        P_BIKOU1.Value = WW_ROW("BIKOU1")           '備考1
                        P_BIKOU2.Value = WW_ROW("BIKOU2")           '備考2
                        P_BIKOU3.Value = WW_ROW("BIKOU3")           '備考3

                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        End If

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0014_SKSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
                SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(KYORI, '')             = @KYORI ")
                SQLStr.AppendLine("    AND  COALESCE(KEIYU, '')             = @KEIYU ")
                SQLStr.AppendLine("    AND  COALESCE(KIZYUN, '')             = @KIZYUN ")
                SQLStr.AppendLine("    AND  COALESCE(TANKASA, '')             = @TANKASA ")
                SQLStr.AppendLine("    AND  COALESCE(KAISU, '')             = @KAISU ")
                SQLStr.AppendLine("    AND  COALESCE(USAGECHARGE, '')             = @USAGECHARGE ")
                SQLStr.AppendLine("    AND  COALESCE(SURCHARGE, '0')             = @SURCHARGE ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU1, '')             = @BIKOU1 ")
                SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                        Dim P_KYORI As MySqlParameter = SQLcmd.Parameters.Add("@KYORI", MySqlDbType.Decimal, 5, 1)     '走行距離
                        Dim P_KEIYU As MySqlParameter = SQLcmd.Parameters.Add("@KEIYU", MySqlDbType.Decimal, 5, 1)     '実勢軽油価格
                        Dim P_KIZYUN As MySqlParameter = SQLcmd.Parameters.Add("@KIZYUN", MySqlDbType.Decimal, 5, 1)     '基準価格
                        Dim P_TANKASA As MySqlParameter = SQLcmd.Parameters.Add("@TANKASA", MySqlDbType.Decimal, 5, 1)     '単価差
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.VarChar, 3)     '輸送回数
                        Dim P_USAGECHARGE As MySqlParameter = SQLcmd.Parameters.Add("@USAGECHARGE", MySqlDbType.VarChar, 5)     '燃料使用量
                        Dim P_SURCHARGE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGE", MySqlDbType.Decimal, 8)     'サーチャージ
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1


                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_KYORI.Value = WW_ROW("KYORI")           '走行距離
                        P_KEIYU.Value = WW_ROW("KEIYU")           '実勢軽油価格
                        P_KIZYUN.Value = WW_ROW("KIZYUN")           '基準価格
                        P_TANKASA.Value = WW_ROW("TANKASA")           '単価差
                        P_KAISU.Value = WW_ROW("KAISU")           '輸送回数
                        P_USAGECHARGE.Value = WW_ROW("USAGECHARGE")           '燃料使用量
                        P_SURCHARGE.Value = WW_ROW("SURCHARGE")           'サーチャージ
                        P_BIKOU1.Value = WW_ROW("BIKOU1")           '備考1

                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable

                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            If WW_Tbl.Rows.Count >= 1 Then
                                Exit Function
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0015_SKSURCHARGE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
        End Select

        SameDataChk = True
    End Function

    '' <summary>
    '' 更新前の削除フラグが"0"、アップロードした削除フラグが"1"の場合Trueを返す
    '' </summary>
    Protected Function ValidationSkipChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        ValidationSkipChk = False
        'アップロードした削除フラグが"1"以外の場合処理を終了する
        If Not WW_ROW("DELFLG") = C_DELETE_FLG.DELETE Then
            Exit Function
        End If

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("RECOID") = "" OrElse
                    WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("STYMD") = Date.MinValue Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日


                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_HACHINOHESPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010_HACHINOHESPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("RECOID") = "" OrElse
                    WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("STYMD") = Date.MinValue Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日


                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0011_ENEOSCOMFEE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("STYMD") = Date.MinValue OrElse
                    WW_ROW("SYABAN") = "" Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0012_TOHOKUSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("TAISHOYM") = "0" OrElse
                    WW_ROW("RECOID") = "" Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("RECOID") = "" OrElse
                    WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("STYMD") = Date.MinValue Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0014_SKSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("TAISHOYM") = "0" Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            'データが存在した場合
                            If WW_Tbl.Rows.Count > 0 Then
                                '更新前の削除フラグが無効の場合
                                If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE Then
                                    ValidationSkipChk = True
                                    Exit Function
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0015_SKSURCHARGE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
        End Select

    End Function

    ''' <summary>
    ''' 削除フラグ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Public Sub SetDelflg(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)

        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0010_HACHINOHESPRATE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0011_ENEOSCOMFEE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0012_TOHOKUSPRATE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0013_KGSPRATE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0014_SKSPRATE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0015_SKSURCHARGE                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_UPDYMD.Value = WW_DATENOW                '更新年月日
                        P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
        End Select

    End Sub

    ''' <summary>
    ''' Excelデータ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdExcelData(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("     RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @RECOID  ")
                SQLStr.AppendLine("     ,@RECONAME  ")
                SQLStr.AppendLine("     ,@TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@STYMD  ")
                SQLStr.AppendLine("     ,@ENDYMD  ")
                SQLStr.AppendLine("     ,@KINGAKU  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      RECONAME =  @RECONAME")
                SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
                SQLStr.AppendLine("     ,KINGAKU =  @KINGAKU")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日   
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_HACHINOHESPRATE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0010_HACHINOHESPRATE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("     RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @RECOID  ")
                SQLStr.AppendLine("     ,@RECONAME  ")
                SQLStr.AppendLine("     ,@TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@STYMD  ")
                SQLStr.AppendLine("     ,@ENDYMD  ")
                SQLStr.AppendLine("     ,@KINGAKU  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      RECONAME =  @RECONAME")
                SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
                SQLStr.AppendLine("     ,KINGAKU =  @KINGAKU")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日   
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0011_ENEOSCOMFEE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("     TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@STYMD  ")
                SQLStr.AppendLine("     ,@ENDYMD  ")
                SQLStr.AppendLine("     ,@SYABAN  ")
                SQLStr.AppendLine("     ,@KOTEIHI  ")
                SQLStr.AppendLine("     ,@KAISU  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
                SQLStr.AppendLine("     ,KOTEIHI =  @KOTEIHI")
                SQLStr.AppendLine("     ,KAISU =  @KAISU")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
                        P_KAISU.Value = WW_ROW("KAISU")           '回数

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0012_TOHOKUSPRATE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0013_KGSPRATE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,ITEMID  ")
                SQLStr.AppendLine("     ,ITEMNAME  ")
                SQLStr.AppendLine("     ,RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TANKA  ")
                SQLStr.AppendLine("     ,COUNT  ")
                SQLStr.AppendLine("     ,FEE  ")
                SQLStr.AppendLine("     ,BIKOU  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@TODOKECODE  ")
                SQLStr.AppendLine("     ,@TODOKENAME  ")
                SQLStr.AppendLine("     ,@SYABAN  ")
                SQLStr.AppendLine("     ,@TAISHOYM  ")
                SQLStr.AppendLine("     ,@ITEMID  ")
                SQLStr.AppendLine("     ,@ITEMNAME  ")
                SQLStr.AppendLine("     ,@RECOID  ")
                SQLStr.AppendLine("     ,@RECONAME  ")
                SQLStr.AppendLine("     ,@TANKA  ")
                SQLStr.AppendLine("     ,@COUNT  ")
                SQLStr.AppendLine("     ,@FEE  ")
                SQLStr.AppendLine("     ,@BIKOU  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
                SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
                SQLStr.AppendLine("     ,SYABAN =  @SYABAN")
                SQLStr.AppendLine("     ,ITEMNAME =  @ITEMNAME")
                SQLStr.AppendLine("     ,RECONAME =  @RECONAME")
                SQLStr.AppendLine("     ,TANKA =  @TANKA")
                SQLStr.AppendLine("     ,COUNT =  @COUNT")
                SQLStr.AppendLine("     ,FEE =  @FEE")
                SQLStr.AppendLine("     ,BIKOU =  @BIKOU")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                        Dim P_ITEMID As MySqlParameter = SQLcmd.Parameters.Add("@ITEMID", MySqlDbType.VarChar, 2)     '大項目
                        Dim P_ITEMNAME As MySqlParameter = SQLcmd.Parameters.Add("@ITEMNAME", MySqlDbType.VarChar, 100)     '項目名
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 5)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)     '単価
                        Dim P_COUNT As MySqlParameter = SQLcmd.Parameters.Add("@COUNT", MySqlDbType.Decimal, 3)     '回数
                        Dim P_FEE As MySqlParameter = SQLcmd.Parameters.Add("@FEE", MySqlDbType.Decimal, 8)     '料金
                        Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_ITEMID.Value = WW_ROW("ITEMID")           '大項目
                        P_ITEMNAME.Value = WW_ROW("ITEMNAME")           '項目名
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TANKA.Value = WW_ROW("TANKA")           '単価
                        P_COUNT.Value = WW_ROW("COUNT")           '回数
                        P_FEE.Value = WW_ROW("FEE")           '料金
                        P_BIKOU.Value = WW_ROW("BIKOU")           '備考

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0013_KGSPRATE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @RECOID  ")
                SQLStr.AppendLine("     ,@RECONAME  ")
                SQLStr.AppendLine("     ,@TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@TODOKECODE  ")
                SQLStr.AppendLine("     ,@TODOKENAME  ")
                SQLStr.AppendLine("     ,@STYMD  ")
                SQLStr.AppendLine("     ,@ENDYMD  ")
                SQLStr.AppendLine("     ,@SYABARA  ")
                SQLStr.AppendLine("     ,@KOTEIHI  ")
                SQLStr.AppendLine("     ,@BIKOU1  ")
                SQLStr.AppendLine("     ,@BIKOU2  ")
                SQLStr.AppendLine("     ,@BIKOU3  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      RECONAME =  @RECONAME")
                SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
                SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
                SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
                SQLStr.AppendLine("     ,SYABARA =  @SYABARA")
                SQLStr.AppendLine("     ,KOTEIHI =  @KOTEIHI")
                SQLStr.AppendLine("     ,BIKOU1 =  @BIKOU1")
                SQLStr.AppendLine("     ,BIKOU2 =  @BIKOU2")
                SQLStr.AppendLine("     ,BIKOU3 =  @BIKOU3")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1
                        Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 50)     '備考2
                        Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 50)     '備考3

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        '有効終了日
                        If Not WW_ROW("ENDYMD") = Date.MinValue Then
                            P_ENDYMD.Value = WW_ROW("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
                        P_BIKOU1.Value = WW_ROW("BIKOU1")           '備考1
                        P_BIKOU2.Value = WW_ROW("BIKOU2")           '備考2
                        P_BIKOU3.Value = WW_ROW("BIKOU3")           '備考3

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0014_SKSPRATE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("   )  ")
                SQLStr.AppendLine("   VALUES  ")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      @TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@TODOKECODE  ")
                SQLStr.AppendLine("     ,@TODOKENAME  ")
                SQLStr.AppendLine("     ,@TAISHOYM  ")
                SQLStr.AppendLine("     ,@KYORI  ")
                SQLStr.AppendLine("     ,@KEIYU  ")
                SQLStr.AppendLine("     ,@KIZYUN  ")
                SQLStr.AppendLine("     ,@TANKASA  ")
                SQLStr.AppendLine("     ,@KAISU  ")
                SQLStr.AppendLine("     ,@USAGECHARGE  ")
                SQLStr.AppendLine("     ,@SURCHARGE  ")
                SQLStr.AppendLine("     ,@BIKOU1  ")
                SQLStr.AppendLine("     ,@DELFLG  ")
                SQLStr.AppendLine("     ,@INITYMD  ")
                SQLStr.AppendLine("     ,@INITUSER  ")
                SQLStr.AppendLine("     ,@INITTERMID  ")
                SQLStr.AppendLine("     ,@INITPGID  ")
                SQLStr.AppendLine("   )   ")
                SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
                SQLStr.AppendLine("      TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
                SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
                SQLStr.AppendLine("     ,KYORI =  @KYORI")
                SQLStr.AppendLine("     ,KEIYU =  @KEIYU")
                SQLStr.AppendLine("     ,KIZYUN =  @KIZYUN")
                SQLStr.AppendLine("     ,TANKASA =  @TANKASA")
                SQLStr.AppendLine("     ,KAISU =  @KAISU")
                SQLStr.AppendLine("     ,USAGECHARGE =  @USAGECHARGE")
                SQLStr.AppendLine("     ,SURCHARGE =  @SURCHARGE")
                SQLStr.AppendLine("     ,BIKOU1 =  @BIKOU1")
                SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
                SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
                SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
                SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
                SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
                SQLStr.AppendLine("    ;  ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                        Dim P_KYORI As MySqlParameter = SQLcmd.Parameters.Add("@KYORI", MySqlDbType.Decimal, 5, 1)     '走行距離
                        Dim P_KEIYU As MySqlParameter = SQLcmd.Parameters.Add("@KEIYU", MySqlDbType.Decimal, 5, 1)     '実勢軽油価格
                        Dim P_KIZYUN As MySqlParameter = SQLcmd.Parameters.Add("@KIZYUN", MySqlDbType.Decimal, 5, 1)     '基準価格
                        Dim P_TANKASA As MySqlParameter = SQLcmd.Parameters.Add("@TANKASA", MySqlDbType.Decimal, 5, 1)     '単価差
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.VarChar, 3)     '輸送回数
                        Dim P_USAGECHARGE As MySqlParameter = SQLcmd.Parameters.Add("@USAGECHARGE", MySqlDbType.VarChar, 5)     '燃料使用量
                        Dim P_SURCHARGE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGE", MySqlDbType.Decimal, 8)     'サーチャージ
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        'DB更新
                        P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_KYORI.Value = WW_ROW("KYORI")           '走行距離
                        P_KEIYU.Value = WW_ROW("KEIYU")           '実勢軽油価格
                        P_KIZYUN.Value = WW_ROW("KIZYUN")           '基準価格
                        P_TANKASA.Value = WW_ROW("TANKASA")           '単価差
                        P_KAISU.Value = WW_ROW("KAISU")           '輸送回数
                        P_USAGECHARGE.Value = WW_ROW("USAGECHARGE")           '燃料使用量
                        P_SURCHARGE.Value = WW_ROW("SURCHARGE")           'サーチャージ
                        P_BIKOU1.Value = WW_ROW("BIKOU1")           '備考1

                        P_INITYMD.Value = WW_DATENOW                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        '登録
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0015_SKSURCHARGE  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
        End Select

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByVal WW_ROW As DataRow, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim NowDate As DateTime = Date.Now

        WW_LineErr = ""

        ' 削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If


        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                    LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                '' レコードID(バリデーションチェック)
                'Master.CheckField(Master.USERCAMP, "RECOID", WW_ROW("RECOID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                'If Not isNormal(WW_CS0024FCheckerr) Then
                '    WW_CheckMES1 = "・レコードIDエラーです。"
                '    WW_CheckMES2 = WW_CS0024FCheckReport
                '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
                ' レコード名(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "RECONAME", WW_ROW("RECONAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・レコード名エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        ' 取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 取引先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGCODE", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGNAME", WW_ROW("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGCODE", WW_ROW("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGNAME", WW_ROW("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLKG, '北海道ガス特別料金マスタ
                 LNM0010WRKINC.MAPIDLSKSP, 'SK特別料金マスタ
                     LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                ' 届先コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TODOKECODE", WW_ROW("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・届先コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 届先名称(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TODOKENAME", WW_ROW("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・届先名称エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                     LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                ' 有効開始日(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "STYMD", WW_ROW("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・有効開始日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '入力済みの場合のみ
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    ' 有効終了日(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "ENDYMD", WW_ROW("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・有効終了日エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ

                ' 金額(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KINGAKU", WW_ROW("KINGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・金額エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                ' 車番(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "SYABAN", WW_ROW("SYABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・車番エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLKG, '北海道ガス特別料金マスタ
                 LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                ' 対象年月(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TAISHOYM", WW_ROW("TAISHOYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・対象年月エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                ''大項目(バリデーションチェック)
                'Master.CheckField(Master.USERCAMP, "ITEMID", WW_ROW("ITEMID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                'If Not isNormal(WW_CS0024FCheckerr) Then
                '    WW_CheckMES1 = "・大項目エラーです。"
                '    WW_CheckMES2 = WW_CS0024FCheckReport
                '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
                '項目名(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "ITEMNAME", WW_ROW("ITEMNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・項目名エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                ' 車腹(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "SYABARA", WW_ROW("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・車腹エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                ' 固定費(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KOTEIHI", WW_ROW("KOTEIHI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・固定費エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                '単価(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TANKA", WW_ROW("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・単価エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                ' 走行距離(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KYORI", WW_ROW("KYORI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・走行距離エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 実勢軽油価格(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KEIYU", WW_ROW("KEIYU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・実勢軽油価格エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 基準価格(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KIZYUN", WW_ROW("KIZYUN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・基準価格エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 単価差(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TANKASA", WW_ROW("TANKASA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・単価差エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 輸送回数(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KAISU", WW_ROW("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・輸送回数エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select


        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                ' 回数(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KAISU", WW_ROW("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・回数エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                '回数(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "COUNT", WW_ROW("COUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・回数エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '料金(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "FEE", WW_ROW("FEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・料金エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '備考(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "BIKOU", WW_ROW("BIKOU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・備考エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                ' 燃料使用量(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "USAGECHARGE", WW_ROW("USAGECHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・燃料使用量エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' サーチャージ(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "SURCHARGE", WW_ROW("SURCHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・サーチャージエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLSKSP, 'SK特別料金マスタ
                     LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                ' 備考1(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "BIKOU1", WW_ROW("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・備考1エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                ' 備考2(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "BIKOU2", WW_ROW("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・備考2エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 備考3(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "BIKOU3", WW_ROW("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・備考3エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                     LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                '画面で入力済みの場合のみ
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    ' 日付大小チェック
                    If Not String.IsNullOrEmpty(WW_ROW("STYMD")) AndAlso
                                Not String.IsNullOrEmpty(WW_ROW("ENDYMD")) Then
                        If CDate(WW_ROW("STYMD")) > CDate(WW_ROW("ENDYMD")) Then
                            WW_CheckMES1 = "・有効開始日＆有効終了日エラーです。"
                            WW_CheckMES2 = "日付大小入力エラー"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                End If
        End Select

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="LINECNT"></param>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal LINECNT As String, ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ErrMes As String = ""
        WW_ErrMes = "【" + LINECNT + "行目】"
        WW_ErrMes &= vbCr & MESSAGE1
        If Not String.IsNullOrEmpty(MESSAGE2) Then

            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '八戸特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_HACHINOHESPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010_HACHINOHESPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                'ENEOS業務委託料マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0011_ENEOSCOMFEE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '東北電力車両別追加料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番


                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0012_TOHOKUSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                '北海道ガス特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '八戸特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0014_SKSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '八戸特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月

                        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                            Dim WW_Tbl = New DataTable
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next

                            '○ テーブル検索結果をテーブル格納
                            WW_Tbl.Load(SQLdr)

                            '更新の場合(データが存在した場合)は変更区分に変更前をセット
                            If WW_Tbl.Rows.Count > 0 Then
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0015_SKSURCHARGE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
        End Select
    End Sub

    ''' <summary>
    ''' 履歴テーブル登録
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertHist(ByVal SQLcon As MySqlConnection,
                             ByVal WW_ROW As DataRow,
                             ByVal WW_BEFDELFLG As String,
                             ByVal WW_MODIFYKBN As String,
                             ByVal WW_NOW As Date,
                             ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0009_HACHINOHESPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0009_HACHINOHESPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0009_HACHINOHESPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0010_ENEOSCOMFEEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0010_ENEOSCOMFEEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0010_ENEOSCOMFEEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0011_TOHOKUSPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0011_TOHOKUSPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0011_TOHOKUSPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
#Region "北海道ガス特別料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0012_KGSPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,ITEMID  ")
                SQLStr.AppendLine("     ,ITEMNAME  ")
                SQLStr.AppendLine("     ,RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TANKA  ")
                SQLStr.AppendLine("     ,COUNT  ")
                SQLStr.AppendLine("     ,FEE  ")
                SQLStr.AppendLine("     ,BIKOU  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,ITEMID  ")
                SQLStr.AppendLine("     ,ITEMNAME  ")
                SQLStr.AppendLine("     ,RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TANKA  ")
                SQLStr.AppendLine("     ,COUNT  ")
                SQLStr.AppendLine("     ,FEE  ")
                SQLStr.AppendLine("     ,BIKOU  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0012_KGSPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0012_KGSPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0013_SKSPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0013_SKSPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0013_SKSPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0014_SKSURCHARGEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,OPERATEKBN  ")
                SQLStr.AppendLine("     ,MODIFYKBN  ")
                SQLStr.AppendLine("     ,MODIFYYMD  ")
                SQLStr.AppendLine("     ,MODIFYUSER  ")
                SQLStr.AppendLine("     ,DELFLG  ")
                SQLStr.AppendLine("     ,INITYMD  ")
                SQLStr.AppendLine("     ,INITUSER  ")
                SQLStr.AppendLine("     ,INITTERMID  ")
                SQLStr.AppendLine("     ,INITPGID  ")
                SQLStr.AppendLine("  )  ")
                SQLStr.AppendLine("  SELECT  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
                SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
                SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
                SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
                SQLStr.AppendLine("     ,DELFLG ")
                SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
                SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
                SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
                SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
                SQLStr.AppendLine("  FROM   ")
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        ' DB更新
                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
                            End If
                        End If

                        P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                        P_MODIFYYMD.Value = WW_NOW               '変更日時
                        P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                        P_INITYMD.Value = WW_NOW              '登録年月日
                        P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID                '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0014_SKSURCHARGEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0014_SKSURCHARGEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
        End Select
    End Sub

    '' <summary>
    '' 大項目採番
    '' </summary>
    Protected Function GenerateITEMID(ByVal WW_ROW As DataRow) As String
        GenerateITEMID = ""

        Dim CS0050Session As New CS0050SESSION

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        'SQLStr.AppendLine("       MAX(ITEMID) AS ITEMID")
        SQLStr.AppendLine("       MAX(LPAD(ITEMID, 2, '0')) AS ITEMID ")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        'SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = WW_ROW("TORICODE")
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = WW_ROW("ORGCODE")
                    '.Add("@TAISHOYM", MySqlDbType.VarChar).Value = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return ""
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count >= 1 Then
                        Return (CInt(WW_Tbl.Rows(0)("ITEMID")) + 1).ToString
                    Else
                        Return "1"
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try
    End Function


    ''' <summary>
    ''' 有効終了日更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Sub UpdateENDYMD(ByVal SQLcon As MySqlConnection, ByVal WW_CONTROLTABLE As String, ByVal WW_ROW As DataRow,
                            ByRef O_MESSAGENO As String, ByVal WW_NOW As String)


        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        Select Case WW_CONTROLTABLE
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.Append("     LNG.LNM0010_HACHINOHESPRATE             ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.Append("     LNG.LNM0011_ENEOSCOMFEE           ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.Append("     LNG.LNM0012_TOHOKUSPRATE          ")
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.Append("     LNG.LNM0014_SKSPRATE          ")
        End Select
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        Select Case WW_CONTROLTABLE
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.Append("       TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.Append("   AND SYABAN  = @SYABAN             ")
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        End Select

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                Select Case WW_CONTROLTABLE
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                End Select

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ


                P_UPDYMD.Value = WW_NOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try
    End Sub
#End Region

End Class


