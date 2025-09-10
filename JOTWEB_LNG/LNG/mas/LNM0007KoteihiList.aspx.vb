''************************************************************
' 固定費マスタメンテナンス・一覧画面
' 作成日 2025/01/20
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/01/20 新規作成
'          : 2025/05/15 統合版に変更
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' 固定費マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0007KoteihiList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0007tbl As DataTable         '一覧格納用テーブル
    Private LNM0007UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0007Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

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
                    Master.RecoverTable(LNM0007tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            InputSave()
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            InputSave()
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0007WRKINC.FILETYPE.EXCEL)
                        'Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                        '    WF_EXCELPDF(LNM0007WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            InputSave()
                            WF_Grid_DBClick()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                            GridViewInitialize()
                        Case "WF_ButtonDebug"           'デバッグボタン押下
                            WF_ButtonDEBUG_Click()
                        Case "WF_SelectCALENDARChange"  '対象年月(変更)時
                            MapInitialize(WF_ButtonClick.Value)
                        Case "WF_SelectTORIChange",     '荷主(変更)時
                             "WF_SelectORGChange",      '部門(変更)時
                             "WF_SelectSEASONChange"    '季節料金判定区分(変更)時
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_ButtonExtract"         '検索ボタン押下時
                            GridViewInitialize()
                        Case "WF_ButtonRelease"         '解除ボタンクリック
                            MapInitialize(WF_ButtonClick.Value)
                        Case "WF_ButtonPAGE", "WF_ButtonFIRST", "WF_ButtonPREVIOUS", "WF_ButtonNEXT", "WF_ButtonLAST"
                            Me.WF_ButtonPAGE_Click()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" And
                        Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" And
                        Not WF_ButtonClick.Value = "WF_ButtonExtract" And
                        Not WF_ButtonClick.Value = "WF_ButtonRelease" And
                        Not WF_ButtonClick.Value = "WF_ButtonPAGE" And
                        Not WF_ButtonClick.Value = "WF_ButtonFIRST" And
                        Not WF_ButtonClick.Value = "WF_ButtonPREVIOUS" And
                        Not WF_ButtonClick.Value = "WF_ButtonNEXT" And
                        Not WF_ButtonClick.Value = "WF_ButtonLAST" Then
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
            If Not IsNothing(LNM0007tbl) Then
                LNM0007tbl.Clear()
                LNM0007tbl.Dispose()
                LNM0007tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0007WRKINC.MAPIDL
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

        '○ ドロップダウンリスト生成
        createListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

        Select Case Context.Handler.ToString().ToUpper()
            '○ 登録・履歴画面からの遷移
            Case C_PREV_MAP_LIST.LNM0007D, C_PREV_MAP_LIST.LNM0007H
                '○ GridView復元
                GridViewRestore()
            Case Else
                '○ GridView初期設定
                GridViewInitialize()
        End Select

        '〇 更新画面からの遷移もしくは、アップロード完了の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' ドロップダウン生成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub createListBox(Optional ByVal resVal As String = Nothing)
        '荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next

        '-- 対象年月(変更)時 OR 解除ボタン押下
        If resVal = "WF_SelectCALENDARChange" OrElse resVal = "WF_ButtonRelease" Then
            '〇季節料金判定区分
            Me.WF_SEASON.Items.Clear()
            Dim retSeasonList As New DropDownList
            retSeasonList = LNM0007WRKINC.getDowpDownSeasonList(Master.MAPID, Master.ROLE_ORG)
            '★ドロップダウンリスト再作成(季節料金判定区分)
            For index As Integer = 0 To retSeasonList.Items.Count - 1
                WF_SEASON.Items.Add(New ListItem(retSeasonList.Items(index).Text, retSeasonList.Items(index).Value))
            Next
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0007D Or
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0007H Then
            ' 登録画面からの遷移
            Master.RecoverTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)
            '対象日
            Dim WW_YM As String = Replace(work.WF_SEL_TARGETYM_L.Text, "/", "")
            WF_TaishoYm.Value = WW_YM.Substring(0, 4) & "/" & WW_YM.Substring(4, 2)
            '荷主
            WF_TORI.SelectedValue = work.WF_SEL_TORI_L.Text
            '部門
            WF_ORG.SelectedValue = work.WF_SEL_ORG_L.Text
            '車番
            WF_SHABAN_FROM.Text = work.WF_SEL_SHABAN_FROM_L.Text
            WF_SHABAN_TO.Text = work.WF_SEL_SHABAN_TO_L.Text
            '季節料金
            WF_SEASON.SelectedValue = work.WF_SEL_SEASON_L.Text
            '削除済みデータ表示状態
            ChkDelDataFlg.Checked = work.WF_SEL_CHKDELDATAFLG_L.Text
            '入力ページ
            TxtPageNo.Text = work.WF_SEL_INPUTPAGE_L.Text
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

            '対象年月
            'WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM/dd")
            WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")
        End If

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0007WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        ' 車番(From)・車番(To)を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_SHABAN_FROM.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_SHABAN_TO.Attributes("onkeyPress") = "CheckNum()"

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

    End Sub

    ''' <summary>
    ''' 画面初期化処理
    ''' </summary>
    Private Sub MapInitialize(Optional ByVal resVal As String = Nothing)
        '解除ボタンクリック
        If resVal = "WF_ButtonRelease" Then
            'ドロップダウン生成処理
            createListBox(resVal)
        End If

        'GridViewデータ設定
        GridViewInitialize()

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
        Master.SaveTable(LNM0007tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0007tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = "1"

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0007tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0007tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0007tbl.Rows.Count) / CONST_DISPROWCOUNT)
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0007tbl)
        Dim WW_RowFilterCMD As New StringBuilder
        WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        TBLview.RowFilter = WW_RowFilterCMD.ToString

        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightview2.GetViewId(Master.USERCAMP)
        End If

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
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
    ''' GridView復元
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewRestore()

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0007tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = work.WF_SEL_NOWPAGECNT_L.Text

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0007tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0007tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0007tbl.Rows.Count) / CONST_DISPROWCOUNT)
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim intPage As Integer = 0
        intPage = CInt(work.WF_SEL_NOWPAGECNT_L.Text)
        If intPage = 1 Then
            WW_GridPosition = 1
        Else
            WW_GridPosition = (intPage - 1) * CONST_SCROLLCOUNT + 1
        End If

        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            If LNM0007row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0007row("SELECT") = WW_DataCNT
            End If
        Next

        Dim TBLview As DataView = New DataView(LNM0007tbl)
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        'Dim WW_RowFilterCMD As New StringBuilder
        'WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        'TBLview.RowFilter = WW_RowFilterCMD.ToString

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
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
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNM0007tbl) Then
            LNM0007tbl = New DataTable
        End If

        If LNM0007tbl.Columns.Count <> 0 Then
            LNM0007tbl.Columns.Clear()
        End If

        LNM0007tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを固定費マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNM0007.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.TORICODE), '')                                    AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.TORINAME), '')                                    AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.ORGCODE), '')                                     AS ORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.ORGNAME), '')                                     AS ORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.KASANORGCODE), '')                                AS KASANORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.KASANORGNAME), '')                                AS KASANORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.TARGETYM), '')                                    AS TARGETYM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SYABAN), '')                                      AS SYABAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.RIKUBAN), '')                                     AS RIKUBAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SYAGATA), '')                                     AS SYAGATA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SYAGATANAME), '')                                 AS SYAGATANAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SYABARA), '')                                     AS SYABARA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SEASONKBN), '')                                   AS SEASONKBN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SEASONSTART), '')                                 AS SEASONSTART              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SEASONEND), '')                                   AS SEASONEND              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.KOTEIHIM), '')                                    AS KOTEIHIM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.KOTEIHID), '')                                    AS KOTEIHID              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.KAISU), '')                                       AS KAISU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.GENGAKU), '')                                     AS GENGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.AMOUNT), '')                                      AS AMOUNT              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.ACCOUNTCODE), '')                                 AS ACCOUNTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.ACCOUNTNAME), '')                                 AS ACCOUNTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SEGMENTCODE), '')                                 AS SEGMENTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.SEGMENTNAME), '')                                 AS SEGMENTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.JOTPERCENTAGE), '')                               AS JOTPERCENTAGE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.ENEXPERCENTAGE), '')                              AS ENEXPERCENTAGE      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0007.BIKOU3), '')                                      AS BIKOU3              ")

        '画面表示用
        '季節料金判定区分
        SQLStr.AppendLine("   , ''                                                                       AS SCRSEASONKBN        ")
        '車腹
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SYABARA), '') = '' THEN ''                                                 ")
        SQLStr.AppendLine("      ELSE  FORMAT(SYABARA,3)                                                                        ")
        SQLStr.AppendLine("     END AS SCRSYABARA                                                                               ")
        '固定費(月額)
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(KOTEIHIM), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(KOTEIHIM,0)                                                                       ")
        SQLStr.AppendLine("     END AS SCRKOTEIHIM                                                                              ")
        '固定費(日額)
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(KOTEIHID), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(KOTEIHID,0)                                                                       ")
        SQLStr.AppendLine("     END AS SCRKOTEIHID                                                                              ")
        '減額費用
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GENGAKU), '') = '' THEN ''                                                 ")
        SQLStr.AppendLine("      ELSE  FORMAT(GENGAKU,0)                                                                        ")
        SQLStr.AppendLine("     END AS SCRGENGAKU                                                                               ")
        '請求額
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(AMOUNT), '') = '' THEN ''                                                  ")
        SQLStr.AppendLine("      ELSE  FORMAT(AMOUNT,0)                                                                         ")
        SQLStr.AppendLine("     END AS SCRAMOUNT                                                                                ")
        '割合JOT
        SQLStr.AppendLine("   , ''                                                                       AS SCRJOTPERCENTAGE    ")
        '割合ENEX
        SQLStr.AppendLine("   , ''                                                                       AS SCRENEXPERCENTAGE   ")

        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0007_FIXED LNM0007                                                                       ")
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
        SQLStr.AppendLine("      ON  LNM0007.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim Itype As Integer

        '対象年月
        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
            SQLStr.AppendLine(" AND  COALESCE(LNM0007.TARGETYM, '0') = COALESCE(@TARGETYM, '0')  ")
        End If

        '取引先コード
        If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
            'SQLStr.AppendLine(" AND  LNM0007.TORICODE = @TORICODE                                          ")
            If WF_TORI.SelectedValue = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr.AppendFormat(" AND LNM0007.TORICODE IN (@TORICODE,'{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
            Else
                SQLStr.AppendLine(" AND  LNM0007.TORICODE = @TORICODE ")
            End If
        End If

        '部門コード
        If Not String.IsNullOrEmpty(WF_ORG.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM0007.ORGCODE = @ORGCODE                                            ")
        End If

        '車番
        Select Case True
            Case Not Trim(WF_SHABAN_FROM.Text) = "" And Not Trim(WF_SHABAN_TO.Text) = ""
                SQLStr.AppendLine(" AND  CONVERT(COALESCE(RTRIM(LNM0007.SYABAN), '0') , DECIMAL) BETWEEN @SHABAN_FROM AND @SHABAN_TO  ")
            Case Not Trim(WF_SHABAN_FROM.Text) = ""
                SQLStr.AppendLine(" AND  LNM0007.SYABAN = @SHABAN_FROM ")
            Case Not Trim(WF_SHABAN_TO.Text) = ""
                SQLStr.AppendLine(" AND  LNM0007.SYABAN = @SHABAN_TO ")
            Case Else
        End Select

        '季節料金
        If Not String.IsNullOrEmpty(WF_SEASON.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM0007.SEASONKBN = @SEASONKBN                                        ")
        End If

        '削除フラグ
        If Not ChkDelDataFlg.Checked Then
            SQLStr.AppendLine(" AND  LNM0007.DELFLG = '0'                                                  ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0007.TARGETYM                                                           ")
        SQLStr.AppendLine("    ,LNM0007.TORICODE                                                           ")
        SQLStr.AppendLine("    ,LNM0007.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,LNM0007.SYABAN                                                             ")
        SQLStr.AppendLine("    ,LNM0007.SEASONKBN                                                          ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                'ロール
                Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                P_ROLE.Value = Master.ROLE_ORG

                '対象年月
                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)
                    P_TARGETYM.Value = Itype
                End If

                '取引先コード
                If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                    P_TORICODE.Value = WF_TORI.SelectedValue
                End If

                '部門コード
                If Not String.IsNullOrEmpty(WF_ORG.SelectedValue) Then
                    Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)
                    P_ORGCODE.Value = WF_ORG.SelectedValue
                End If

                '車番
                Select Case True
                    Case Not Trim(WF_SHABAN_FROM.Text) = "" And Not Trim(WF_SHABAN_TO.Text) = ""
                        Dim P_SHABAN_FROM As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN_FROM", MySqlDbType.Decimal, 20)
                        Dim P_SHABAN_TO As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN_TO", MySqlDbType.Decimal, 20)
                        P_SHABAN_FROM.Value = CDbl(WF_SHABAN_FROM.Text)
                        P_SHABAN_TO.Value = CDbl(WF_SHABAN_TO.Text)
                    Case Not Trim(WF_SHABAN_FROM.Text) = ""
                        Dim P_SHABAN_FROM As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN_FROM", MySqlDbType.VarChar, 20)
                        P_SHABAN_FROM.Value = WF_SHABAN_FROM.Text
                    Case Not Trim(WF_SHABAN_TO.Text) = ""
                        Dim P_SHABAN_TO As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN_TO", MySqlDbType.VarChar, 20)
                        P_SHABAN_TO.Value = WF_SHABAN_TO.Text
                    Case Else
                End Select

                '季節料金
                If Not String.IsNullOrEmpty(WF_SEASON.SelectedValue) Then
                    Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)
                    P_SEASONKBN.Value = WF_SEASON.SelectedValue
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0007row As DataRow In LNM0007tbl.Rows
                    i += 1
                    LNM0007row("LINECNT") = i        'LINECNT

                    Select Case LNM0007row("SEASONKBN").ToString
                        Case "0" : LNM0007row("SCRSEASONKBN") = "通年"
                        Case "1" : LNM0007row("SCRSEASONKBN") = "夏季料金"
                        Case "2" : LNM0007row("SCRSEASONKBN") = "冬季料金"
                        Case Else : LNM0007row("SCRSEASONKBN") = ""
                    End Select

                    '割合JOT
                    Select Case LNM0007row("JOTPERCENTAGE").ToString
                        Case "" : LNM0007row("SCRJOTPERCENTAGE") = ""
                        Case Else : LNM0007row("SCRJOTPERCENTAGE") = LNM0007row("JOTPERCENTAGE").ToString & "%"
                    End Select

                    '割合ENEX
                    Select Case LNM0007row("ENEXPERCENTAGE").ToString
                        Case "" : LNM0007row("SCRENEXPERCENTAGE") = ""
                        Case Else : LNM0007row("SCRENEXPERCENTAGE") = LNM0007row("ENEXPERCENTAGE").ToString & "%"
                    End Select

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007L Select"
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

        work.WF_SEL_LINECNT.Text = ""                                             '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)    '削除

        work.WF_SEL_TARGETYM.Text = Date.Now.ToString("yyyy/MM")                       '対象年月

        If WF_TORI.SelectedValue = "" Then
            work.WF_SEL_TORICODE.Text = ""                  '取引先コード
            work.WF_SEL_TORINAME.Text = ""                 '取引先名称
        Else
            work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue                         '取引先コード
            work.WF_SEL_TORINAME.Text = WF_TORI.SelectedItem.ToString                 '取引先名称
        End If

        work.WF_SEL_ORGCODE.Text = ""                                                     '部門コード
        work.WF_SEL_ORGNAME.Text = ""                                                     '部門名称
        work.WF_SEL_KASANORGCODE.Text = ""                                                '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = ""                                                '加算先部門名称
        work.WF_SEL_SYABAN.Text = ""                                                      '車番
        work.WF_SEL_RIKUBAN.Text = ""                                                     '陸事番号
        work.WF_SEL_SYAGATA.Text = ""                                                     '車型
        work.WF_SEL_SYAGATANAME.Text = ""                                                 '車型名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SYABARA.Text)           '車腹
        work.WF_SEL_SEASONKBN.Text = ""                                                   '季節料金判定区分
        work.WF_SEL_SEASONSTART.Text = ""                                                 '季節料金判定開始月日
        work.WF_SEL_SEASONEND.Text = ""                                                   '季節料金判定終了月日
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHIM.Text)          '固定費(月額)
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHID.Text)          '固定費(日額)
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KAISU.Text)             '回数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_GENGAKU.Text)           '減額費用
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_AMOUNT.Text)            '請求額
        work.WF_SEL_ACCOUNTCODE.Text = ""                                                 '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = ""                                                 '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = ""                                                 'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = ""                                                 'セグメント名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_JOTPERCENTAGE.Text)     '割合JOT
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_ENEXPERCENTAGE.Text)    '割合ENEX
        work.WF_SEL_BIKOU1.Text = ""                                                      '備考1
        work.WF_SEL_BIKOU2.Text = ""                                                      '備考2
        work.WF_SEL_BIKOU3.Text = ""                                                      '備考3

        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0007KoteihiHistory.aspx")
    End Sub

    ' ******************************************************************************
    ' ***  フィールド変更処理                                                    ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールド(変更)時処理
    ''' </summary>
    ''' <param name="resVal">荷主(変更)時(WF_SelectTORIChange),部門(変更)時(WF_SelectORGChange),季節料金(変更)時(WF_SelectSEASONChange)</param>
    ''' <remarks></remarks>
    Protected Sub WF_SelectFIELD_CHANGE(ByVal resVal As String)
        '■荷主(情報)取得
        Dim selectTORI As String = WF_TORI.SelectedValue
        Dim selectindexTORI As Integer = WF_TORI.SelectedIndex
        '■部門(情報)取得
        Dim selectORG As String = WF_ORG.SelectedValue
        Dim selectindexORG As Integer = WF_ORG.SelectedIndex
        '■季節料金判定区分(情報)取得
        Dim selectSEASON As String = WF_SEASON.SelectedValue
        Dim selectindexSEASON As Integer = WF_SEASON.SelectedIndex

        '〇フィールド(変更)ボタン
        Select Case resVal
            '荷主(変更)時
            Case "WF_SelectTORIChange"
                selectORG = ""              '-- 部門(表示)初期化
                selectindexORG = 0          '-- 部門(INDEX)初期化
                selectSEASON = ""           '-- 季節料金判定区分(表示)初期化
                selectindexSEASON = 0       '-- 季節料金判定区分(INDEX)初期化
            '部門(変更)時
            Case "WF_SelectORGChange"
                selectSEASON = ""           '-- 季節料金判定区分(表示)初期化
                selectindexSEASON = 0       '-- 季節料金判定区分(INDEX)初期化
            '季節料金判定区分(変更)時
            Case "WF_SelectKASANORGChange"
        End Select

        '〇荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        'retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_SEASONKBN:=selectSEASON)
        '★ドロップダウンリスト選択(荷主)の場合
        If retToriList.Items(0).Text <> "全て表示" Then
            WF_TORI.Items.Add(New ListItem("全て表示", ""))
            selectindexTORI = 1
        End If
        '★ドロップダウンリスト再作成(荷主)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next
        WF_TORI.SelectedIndex = selectindexTORI

        '〇部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI)
        'retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_SEASONKBN:=selectSEASON)
        '★ドロップダウンリスト選択(部門)の場合
        If retOrgList.Items(0).Text <> "全て表示" Then
            WF_ORG.Items.Add(New ListItem("全て表示", ""))
            selectindexORG = 1
        End If
        '★ドロップダウンリスト再作成(部門)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next
        WF_ORG.SelectedIndex = selectindexORG

        '〇季節料金判定区分
        Me.WF_SEASON.Items.Clear()
        Dim retSeasonList As New DropDownList
        retSeasonList = LNM0007WRKINC.getDowpDownSeasonList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_SEASONKBN:=selectSEASON)
        '★ドロップダウンリスト選択(季節料金判定区分)の場合
        If retSeasonList.Items(0).Text <> "全て表示" Then
            WF_SEASON.Items.Add(New ListItem("全て表示", ""))
            selectindexSEASON = 1
        End If
        '★ドロップダウンリスト再作成(季節料金判定区分)
        For index As Integer = 0 To retSeasonList.Items.Count - 1
            WF_SEASON.Items.Add(New ListItem(retSeasonList.Items(index).Text, retSeasonList.Items(index).Value))
        Next
        WF_SEASON.SelectedIndex = selectindexSEASON

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNM0007tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNM0007tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
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

        '〇 表示中ページ
        If WF_GridPosition.Text = "1" Then
            Me.WF_NOWPAGECNT.Text = "1"
        Else
            Me.WF_NOWPAGECNT.Text = (CInt(WF_GridPosition.Text) - 1) / CONST_DISPROWCOUNT + 1
        End If

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
        Dim TBLview As New DataView(LNM0007tbl)
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

    ''' <summary>
    ''' ページボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_ButtonPAGE_Click()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim intLineNo As Integer = 0
        Dim intPage As Integer = 0

        Select Case WF_ButtonClick.Value
            Case "WF_ButtonPAGE"            '指定ページボタン押下
                intPage = CInt(Me.TxtPageNo.Text.PadLeft(5, "0"c))
                If intPage < 1 Then
                    intPage = 1
                End If
            Case "WF_ButtonFIRST"           '先頭ページボタン押下
                intPage = 1
            Case "WF_ButtonPREVIOUS"        '前ページボタン押下
                intPage = CInt(Me.WF_NOWPAGECNT.Text)
                If intPage > 1 Then
                    intPage += -1
                End If
            Case "WF_ButtonNEXT"            '次ページボタン押下
                intPage = CInt(Me.WF_NOWPAGECNT.Text)
                If intPage < CInt(Me.WF_TOTALPAGECNT.Text) Then
                    intPage += 1
                End If
            Case "WF_ButtonLAST"            '最終ページボタン押下
                intPage = CInt(Me.WF_TOTALPAGECNT.Text)
        End Select

        Me.WF_NOWPAGECNT.Text = intPage.ToString
        If intPage = 1 Then
            WW_GridPosition = 1
        Else
            WW_GridPosition = (intPage - 1) * CONST_SCROLLCOUNT + 1
        End If

        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0014row As DataRow In LNM0007tbl.Rows
            If LNM0014row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0014row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNM0007tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
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

    '入力状態を保持する
    Protected Sub InputSave()
        work.WF_SEL_TARGETYM_L.Text = WF_TaishoYm.Value '対象日
        work.WF_SEL_TORI_L.Text = WF_TORI.SelectedValue '荷主
        work.WF_SEL_ORG_L.Text = WF_ORG.SelectedValue '部門
        work.WF_SEL_SHABAN_FROM_L.Text = WF_SHABAN_FROM.Text '車番FROM
        work.WF_SEL_SHABAN_TO_L.Text = WF_SHABAN_TO.Text '車番TO
        work.WF_SEL_SEASON_L.Text = WF_SEASON.SelectedValue '季節料金
        work.WF_SEL_CHKDELDATAFLG_L.Text = ChkDelDataFlg.Checked '削除済みデータ表示状態
        work.WF_SEL_INPUTPAGE_L.Text = TxtPageNo.Text '入力ページ
        work.WF_SEL_NOWPAGECNT_L.Text = WF_NOWPAGECNT.Text
    End Sub

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_DBDataCheck As String = ""

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        'ダブルクリックした行が削除行の場合、遷移せずに削除フラグだけ有効にする
        If LNM0007tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE Then
            Dim WW_ROW As DataRow
            WW_ROW = LNM0007tbl.Rows(WW_LineCNT)
            Dim DATENOW As Date = Date.Now
            Dim WW_UPDTIMSTP As Date

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                '履歴登録(変更前)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '削除フラグ有効化
                DelflgValid(SQLcon, WW_ROW, DATENOW, WW_UPDTIMSTP)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '履歴登録(変更後)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0007WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                LNM0007tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.ALIVE
                LNM0007tbl.Rows(WW_LineCNT)("UPDTIMSTP") = WW_UPDTIMSTP
                Master.SaveTable(LNM0007tbl)
                Master.Output(C_MESSAGE_NO.DELETE_ROW_ACTIVATION, C_MESSAGE_TYPE.NOR, needsPopUp:=True)
            End Using
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = LNM0007tbl.Rows(WW_LineCNT)("LINECNT")            '選択行

        work.WF_SEL_TORICODE.Text = LNM0007tbl.Rows(WW_LineCNT)("TORICODE")          '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0007tbl.Rows(WW_LineCNT)("TORINAME")          '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("ORGCODE")            '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("ORGNAME")            '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("KASANORGCODE")  '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("KASANORGNAME")  '加算先部門名称
        work.WF_SEL_TARGETYM.Text = LNM0007tbl.Rows(WW_LineCNT)("TARGETYM")          '対象年月
        work.WF_SEL_SYABAN.Text = LNM0007tbl.Rows(WW_LineCNT)("SYABAN")              '車番
        work.WF_SEL_RIKUBAN.Text = LNM0007tbl.Rows(WW_LineCNT)("RIKUBAN")            '陸事番号
        work.WF_SEL_SYAGATA.Text = LNM0007tbl.Rows(WW_LineCNT)("SYAGATA")            '車型
        work.WF_SEL_SYAGATANAME.Text = LNM0007tbl.Rows(WW_LineCNT)("SYAGATANAME")    '車型名
        work.WF_SEL_SYABARA.Text = LNM0007tbl.Rows(WW_LineCNT)("SYABARA")            '車腹
        work.WF_SEL_SEASONKBN.Text = LNM0007tbl.Rows(WW_LineCNT)("SEASONKBN")        '季節料金判定区分
        work.WF_SEL_SEASONSTART.Text = LNM0007tbl.Rows(WW_LineCNT)("SEASONSTART")    '季節料金判定開始月日
        work.WF_SEL_SEASONEND.Text = LNM0007tbl.Rows(WW_LineCNT)("SEASONEND")        '季節料金判定終了月日
        work.WF_SEL_KOTEIHIM.Text = LNM0007tbl.Rows(WW_LineCNT)("KOTEIHIM")          '固定費(月額)
        work.WF_SEL_KOTEIHID.Text = LNM0007tbl.Rows(WW_LineCNT)("KOTEIHID")          '固定費(日額)
        work.WF_SEL_KAISU.Text = LNM0007tbl.Rows(WW_LineCNT)("KAISU")                '回数
        work.WF_SEL_GENGAKU.Text = LNM0007tbl.Rows(WW_LineCNT)("GENGAKU")            '減額費用
        work.WF_SEL_AMOUNT.Text = LNM0007tbl.Rows(WW_LineCNT)("AMOUNT")              '請求額
        work.WF_SEL_ACCOUNTCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("ACCOUNTCODE")                   '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("ACCOUNTNAME")                   '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("SEGMENTCODE")                   'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("SEGMENTNAME")                   'セグメント名
        work.WF_SEL_JOTPERCENTAGE.Text = LNM0007tbl.Rows(WW_LineCNT)("JOTPERCENTAGE")               '割合JOT
        work.WF_SEL_ENEXPERCENTAGE.Text = LNM0007tbl.Rows(WW_LineCNT)("ENEXPERCENTAGE")             '割合ENEX
        work.WF_SEL_BIKOU1.Text = LNM0007tbl.Rows(WW_LineCNT)("BIKOU1")              '備考1
        work.WF_SEL_BIKOU2.Text = LNM0007tbl.Rows(WW_LineCNT)("BIKOU2")              '備考2
        work.WF_SEL_BIKOU3.Text = LNM0007tbl.Rows(WW_LineCNT)("BIKOU3")              '備考3

        work.WF_SEL_DELFLG.Text = LNM0007tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0007tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_TARGETYM.Text, work.WF_SEL_SYABAN.Text, work.WF_SEL_SEASONKBN.Text)
        End Using

        If Not isNormal(WW_DBDataCheck) Then
            Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' 削除フラグ有効化
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    ''' <remarks></remarks>
    Public Sub DelflgValid(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_NOW As Date, ByRef WW_UPDTIMSTP As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0007_FIXED                       ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '0'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.Append("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.Append("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分
                P_UPDYMD.Value = WW_NOW             '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007L UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        'タイムスタンプ取得
        '○ 対象データ取得
        Dim SQLStrTimStp = New StringBuilder
        SQLStrTimStp.AppendLine(" SELECT                                                ")
        SQLStrTimStp.AppendLine("    UPDTIMSTP                                          ")
        SQLStrTimStp.AppendLine(" FROM                                                  ")
        SQLStrTimStp.AppendLine("     LNG.LNM0007_FIXED                                 ")
        SQLStrTimStp.AppendLine(" WHERE                                                 ")
        SQLStrTimStp.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStrTimStp.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStrTimStp.Append("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStrTimStp.Append("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStrTimStp.Append("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrTimStp.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        WW_UPDTIMSTP = WW_Tbl.Rows(0)("UPDTIMSTP").ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
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
                Case "SYAGATA"           '車型
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "SYAGATA"))
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
        'UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", CS0050SESSION.HTTPS_GET, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = 0
        WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0

        WW_STROW = WW_ACTIVEROW
        SetDETAIL(wb, wb.ActiveSheet, WW_ACTIVEROW)
        WW_ENDROW = WW_ACTIVEROW - 1

        'シート全体設定
        SetALL(wb.ActiveSheet)

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
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        wb.ActiveSheet.Range("B2").Value = "は入力必須"

        wb.ActiveSheet.Range("C1").Value = "固定費マスタ一覧"

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
            Case LNM0007WRKINC.FILETYPE.EXCEL
                FileName = "固定費マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            'Case LNM0007WRKINC.FILETYPE.PDF
            '    FileName = "固定費マスタ.pdf"
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
        '入力必須列網掛け
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '季節料金判定区分

        '入力不要列網掛け
        sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '車型名

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

        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.TARGETYM).Value = "対象年月"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Value = "（必須）車番"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN).Value = "陸事番号"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).Value = "車型"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = "車型名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Value = "車腹"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN).Value = "（必須）季節料金判定区分"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONSTART).Value = "季節料金判定開始月日"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONEND).Value = "季節料金判定終了月日"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHIM).Value = "固定費(月額)"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHID).Value = "固定費(日額)"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KAISU).Value = "回数"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.GENGAKU).Value = "減額費用"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.AMOUNT).Value = "請求額"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = "勘定科目コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = "勘定科目名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = "セグメントコード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = "セグメント名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = "割合JOT"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = "割合ENEX"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU1).Value = "備考1"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU2).Value = "備考2"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU3).Value = "備考3"

        Dim WW_TEXT As String = ""
        Dim WW_TEXTLIST = New StringBuilder
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '車型
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:単車")
            WW_TEXTLIST.AppendLine("2:トレーラ")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).Comment.Shape
                .Width = 100
                .Height = 30
            End With

            '季節料金判定区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("0:通年")
            WW_TEXTLIST.AppendLine("1:夏季料金")
            WW_TEXTLIST.AppendLine("2:冬季料金")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN).Comment.Shape
                .Width = 100
                .Height = 45
            End With

            '割合JOT
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("JOT手数料として収受する割合(JOT収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Comment.Shape
                .Width = 400
                .Height = 30
            End With

            '割合ENEX
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("ENEXへ支払う割合(ENEX収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Comment.Shape
                .Width = 400
                .Height = 30
            End With

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
        '削除フラグ
        SETFIXVALUELIST(subsheet, "DELFLG", LNM0007WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0007WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

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
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0007WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0007WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'sheet.Range(WW_STRANGE.Address & ":" & WW_ENDRANGE.Address).Locked = True

        ''シートを保護する
        'sheet.Protect()
    End Sub


    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        '数値書式(整数)
        Dim IntStyle As IStyle = wb.Styles.Add("IntStyle")
        IntStyle.NumberFormat = "#,##0_);[Red](#,##0)"

        '数値書式(小数点含む)
        Dim DecStyle As IStyle = wb.Styles.Add("DecStyle")
        DecStyle.NumberFormat = "#,##0.000_);[Red](#,##0.000)"

        '数値書式(小数点含む)
        Dim DecStyle2 As IStyle = wb.Styles.Add("DecStyle2")
        DecStyle2.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNM0007tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.TARGETYM).Value = Row("TARGETYM") '対象年月
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Value = Row("SYABAN") '車番
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN).Value = Row("RIKUBAN") '陸事番号
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).Value = Row("SYAGATA") '車型
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = Row("SYAGATANAME") '車型名

            '車腹
            If Row("SYABARA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Value = Row("SYABARA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Value = CDbl(Row("SYABARA"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN).Value = Row("SEASONKBN") '季節料金判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONSTART).Value = Row("SEASONSTART") '季節料金判定開始月日
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONEND).Value = Row("SEASONEND") '季節料金判定終了月日

            '固定費(月額)
            If Row("KOTEIHIM") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHIM).Value = Row("KOTEIHIM")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHIM).Value = CDbl(Row("KOTEIHIM"))
            End If

            '固定費(日額)
            If Row("KOTEIHID") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHID).Value = Row("KOTEIHID")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHID).Value = CDbl(Row("KOTEIHID"))
            End If

            '回数
            If Row("KAISU") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KAISU).Value = Row("KAISU")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KAISU).Value = CDbl(Row("KAISU"))
            End If

            '減額費用
            If Row("GENGAKU") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.GENGAKU).Value = Row("GENGAKU")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.GENGAKU).Value = CDbl(Row("GENGAKU"))
            End If

            '請求額
            If Row("AMOUNT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.AMOUNT).Value = Row("AMOUNT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.AMOUNT).Value = CDbl(Row("AMOUNT"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = Row("ACCOUNTCODE") '勘定科目コード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '勘定科目名
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = Row("SEGMENTCODE") 'セグメントコード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = Row("SEGMENTNAME") 'セグメント名

            '割合JOT
            If Row("JOTPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = Row("JOTPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = CDbl(Row("JOTPERCENTAGE"))
            End If

            '割合ENEX
            If Row("ENEXPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = Row("ENEXPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = CDbl(Row("ENEXPERCENTAGE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3

            '金額を数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHIM).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHID).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KAISU).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.GENGAKU).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.AMOUNT).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Style = DecStyle2
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Style = DecStyle2

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
    ' ******************************************************************************
    ' ***  更新処理                                                              ***
    ' ******************************************************************************
#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\固定費マスタ一括アップロードテスト.xlsx"

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

            For Each Row As DataRow In LNM0007Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0007WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0007WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0007WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "固定費マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0007Exceltbl) Then
            LNM0007Exceltbl = New DataTable
        End If
        If LNM0007Exceltbl.Columns.Count <> 0 Then
            LNM0007Exceltbl.Columns.Clear()
        End If
        LNM0007Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\FIXEDEXCEL"
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
        Dim fileNameHead As String = "FIXEDEXCEL_TMP_"

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
            rightview.AddErrorReport("データ変換に失敗したためアップロードを中断しました。")
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

            For Each Row As DataRow In LNM0007Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0007WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0007WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0007WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If


                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
            If WW_UplErrCnt = 0 Then
                'エラーなし
                WF_RightboxOpen.Value = "OpenI"
            Else
                'エラーあり
                WF_RightboxOpen.Value = "Open"
            End If

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
        SQLStr.AppendLine("        ,TARGETYM  ")
        SQLStr.AppendLine("        ,SYABAN  ")
        SQLStr.AppendLine("        ,RIKUBAN  ")
        SQLStr.AppendLine("        ,SYAGATA  ")
        'SQLStr.AppendLine("        ,SYAGATANAME  ")
        SQLStr.AppendLine("        ,SYABARA  ")
        SQLStr.AppendLine("        ,SEASONKBN  ")
        SQLStr.AppendLine("        ,SEASONSTART  ")
        SQLStr.AppendLine("        ,SEASONEND  ")
        SQLStr.AppendLine("        ,KOTEIHIM  ")
        SQLStr.AppendLine("        ,KOTEIHID  ")
        SQLStr.AppendLine("        ,KAISU  ")
        SQLStr.AppendLine("        ,GENGAKU  ")
        SQLStr.AppendLine("        ,AMOUNT  ")
        SQLStr.AppendLine("        ,ACCOUNTCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,SEGMENTCODE  ")
        SQLStr.AppendLine("        ,SEGMENTNAME  ")
        SQLStr.AppendLine("        ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("        ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("        ,BIKOU1  ")
        SQLStr.AppendLine("        ,BIKOU2  ")
        SQLStr.AppendLine("        ,BIKOU3  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0007_FIXED ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        Try

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

            Dim LNM0007Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNM0007Exceltblrow = LNM0007Exceltbl.NewRow

                'LINECNT
                LNM0007Exceltblrow("LINECNT") = WW_LINECNT
                WW_LINECNT = WW_LINECNT + 1

                '◆データセット
                '取引先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.TORICODE))
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNM0007Exceltblrow("TORICODE") = LNM0007WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.TORINAME))
                WW_DATATYPE = DataTypeHT("TORINAME")
                LNM0007Exceltblrow("TORINAME") = LNM0007WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ORGCODE))
                WW_DATATYPE = DataTypeHT("ORGCODE")
                LNM0007Exceltblrow("ORGCODE") = LNM0007WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ORGNAME))
                WW_DATATYPE = DataTypeHT("ORGNAME")
                LNM0007Exceltblrow("ORGNAME") = LNM0007WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGCODE))
                WW_DATATYPE = DataTypeHT("KASANORGCODE")
                LNM0007Exceltblrow("KASANORGCODE") = LNM0007WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGNAME))
                WW_DATATYPE = DataTypeHT("KASANORGNAME")
                LNM0007Exceltblrow("KASANORGNAME") = LNM0007WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '対象年月
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.TARGETYM))
                WW_DATATYPE = DataTypeHT("TARGETYM")
                LNM0007Exceltblrow("TARGETYM") = LNM0007WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SYABAN))
                WW_DATATYPE = DataTypeHT("SYABAN")
                LNM0007Exceltblrow("SYABAN") = LNM0007WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '陸事番号
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN))
                WW_DATATYPE = DataTypeHT("RIKUBAN")
                LNM0007Exceltblrow("RIKUBAN") = LNM0007WRKINC.DataConvert("陸事番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車型
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA))
                WW_DATATYPE = DataTypeHT("SYAGATA")
                LNM0007Exceltblrow("SYAGATA") = LNM0007WRKINC.DataConvert("車型", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                ''車型名
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME))
                'WW_DATATYPE = DataTypeHT("SYAGATANAME")
                'LNM0007Exceltblrow("SYAGATANAME") = LNM0007WRKINC.DataConvert("車型名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                '車腹
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA)), ",", "")
                WW_DATATYPE = DataTypeHT("SYABARA")
                LNM0007Exceltblrow("SYABARA") = LNM0007WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '季節料金判定区分
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONKBN))
                WW_DATATYPE = DataTypeHT("SEASONKBN")
                LNM0007Exceltblrow("SEASONKBN") = LNM0007WRKINC.DataConvert("季節料金判定区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '季節料金判定開始月日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONSTART))
                WW_DATATYPE = DataTypeHT("SEASONSTART")
                LNM0007Exceltblrow("SEASONSTART") = LNM0007WRKINC.DataConvert("季節料金判定開始月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '季節料金判定終了月日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SEASONEND))
                WW_DATATYPE = DataTypeHT("SEASONEND")
                LNM0007Exceltblrow("SEASONEND") = LNM0007WRKINC.DataConvert("季節料金判定終了月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '固定費(月額)
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHIM)), ",", "")
                WW_DATATYPE = DataTypeHT("KOTEIHIM")
                LNM0007Exceltblrow("KOTEIHIM") = LNM0007WRKINC.DataConvert("固定費(月額)", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '固定費(日額)
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHID)), ",", "")
                WW_DATATYPE = DataTypeHT("KOTEIHID")
                LNM0007Exceltblrow("KOTEIHID") = LNM0007WRKINC.DataConvert("固定費(日額)", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '回数
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KAISU)), ",", "")
                WW_DATATYPE = DataTypeHT("KAISU")
                LNM0007Exceltblrow("KAISU") = LNM0007WRKINC.DataConvert("回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '減額費用
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.GENGAKU)), ",", "")
                WW_DATATYPE = DataTypeHT("GENGAKU")
                LNM0007Exceltblrow("GENGAKU") = LNM0007WRKINC.DataConvert("減額費用", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求額
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.AMOUNT)), ",", "")
                WW_DATATYPE = DataTypeHT("AMOUNT")
                LNM0007Exceltblrow("AMOUNT") = LNM0007WRKINC.DataConvert("請求額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTCODE))
                WW_DATATYPE = DataTypeHT("ACCOUNTCODE")
                LNM0007Exceltblrow("ACCOUNTCODE") = LNM0007WRKINC.DataConvert("勘定科目コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ACCOUNTNAME))
                WW_DATATYPE = DataTypeHT("ACCOUNTNAME")
                LNM0007Exceltblrow("ACCOUNTNAME") = LNM0007WRKINC.DataConvert("勘定科目名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメントコード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTCODE))
                WW_DATATYPE = DataTypeHT("SEGMENTCODE")
                LNM0007Exceltblrow("SEGMENTCODE") = LNM0007WRKINC.DataConvert("セグメントコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメント名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SEGMENTNAME))
                WW_DATATYPE = DataTypeHT("SEGMENTNAME")
                LNM0007Exceltblrow("SEGMENTNAME") = LNM0007WRKINC.DataConvert("セグメント名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合JOT
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.JOTPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("JOTPERCENTAGE")
                LNM0007Exceltblrow("JOTPERCENTAGE") = LNM0007WRKINC.DataConvert("割合JOT", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合ENEX
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("ENEXPERCENTAGE")
                LNM0007Exceltblrow("ENEXPERCENTAGE") = LNM0007WRKINC.DataConvert("割合ENEX", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考1
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU1))
                WW_DATATYPE = DataTypeHT("BIKOU1")
                LNM0007Exceltblrow("BIKOU1") = LNM0007WRKINC.DataConvert("備考1", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考2
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU2))
                WW_DATATYPE = DataTypeHT("BIKOU2")
                LNM0007Exceltblrow("BIKOU2") = LNM0007WRKINC.DataConvert("備考2", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考3
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU3))
                WW_DATATYPE = DataTypeHT("BIKOU3")
                LNM0007Exceltblrow("BIKOU3") = LNM0007WRKINC.DataConvert("備考3", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNM0007Exceltblrow("DELFLG") = LNM0007WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '登録
                LNM0007Exceltbl.Rows.Add(LNM0007Exceltblrow)

            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, "アップロードファイル不正、内容を確認してください。", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "アップロードファイル不正、内容を確認してください。"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.OIL_FREE_MESSAGE
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = "ERR"
            Exit Sub

        End Try

    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(RIKUBAN, '')             = @RIKUBAN ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        'SQLStr.AppendLine("    AND  COALESCE(SYAGATANAME, '')             = @SYAGATANAME ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '')             = @SYABARA ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONSTART, '')             = @SEASONSTART ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONEND, '')             = @SEASONEND ")
        SQLStr.AppendLine("    AND  COALESCE(KOTEIHIM, '0')             = @KOTEIHIM ")
        SQLStr.AppendLine("    AND  COALESCE(KOTEIHID, '0')             = @KOTEIHID ")
        SQLStr.AppendLine("    AND  COALESCE(KAISU, '0')             = @KAISU ")
        SQLStr.AppendLine("    AND  COALESCE(GENGAKU, '0')             = @GENGAKU ")
        SQLStr.AppendLine("    AND  COALESCE(AMOUNT, '0')             = @AMOUNT ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTCODE, '0')             = @ACCOUNTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTNAME, '')             = @ACCOUNTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTCODE, '0')             = @SEGMENTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTNAME, '')             = @SEGMENTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(JOTPERCENTAGE, '')             = @JOTPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(ENEXPERCENTAGE, '')             = @ENEXPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU1, '')             = @BIKOU1 ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU2, '')             = @BIKOU2 ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU3, '')             = @BIKOU3 ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_RIKUBAN As MySqlParameter = SQLcmd.Parameters.Add("@RIKUBAN", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                'Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_SEASONSTART As MySqlParameter = SQLcmd.Parameters.Add("@SEASONSTART", MySqlDbType.VarChar, 4)     '季節料金判定開始月日
                Dim P_SEASONEND As MySqlParameter = SQLcmd.Parameters.Add("@SEASONEND", MySqlDbType.VarChar, 4)     '季節料金判定終了月日
                Dim P_KOTEIHIM As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHIM", MySqlDbType.Decimal, 10)     '固定費(月額)
                Dim P_KOTEIHID As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHID", MySqlDbType.Decimal, 10)     '固定費(日額)
                Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数
                Dim P_GENGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GENGAKU", MySqlDbType.Decimal, 10)     '減額費用
                Dim P_AMOUNT As MySqlParameter = SQLcmd.Parameters.Add("@AMOUNT", MySqlDbType.Decimal, 10)     '請求額
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1
                Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 50)     '備考2
                Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 50)     '備考3
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_RIKUBAN.Value = WW_ROW("RIKUBAN")           '陸事番号
                P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型
                'P_SYAGATANAME.Value = WW_ROW("SYAGATANAME")           '車型名
                P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分
                P_SEASONSTART.Value = WW_ROW("SEASONSTART")           '季節料金判定開始月日
                P_SEASONEND.Value = WW_ROW("SEASONEND")           '季節料金判定終了月日
                P_KOTEIHIM.Value = WW_ROW("KOTEIHIM")           '固定費(月額)
                P_KOTEIHID.Value = WW_ROW("KOTEIHID")           '固定費(日額)
                P_KAISU.Value = WW_ROW("KAISU")           '回数
                P_GENGAKU.Value = WW_ROW("GENGAKU")           '減額費用
                P_AMOUNT.Value = WW_ROW("AMOUNT")           '請求額
                P_ACCOUNTCODE.Value = WW_ROW("ACCOUNTCODE")           '勘定科目コード
                P_ACCOUNTNAME.Value = WW_ROW("ACCOUNTNAME")           '勘定科目名
                P_SEGMENTCODE.Value = WW_ROW("SEGMENTCODE")           'セグメントコード
                P_SEGMENTNAME.Value = WW_ROW("SEGMENTNAME")           'セグメント名
                P_JOTPERCENTAGE.Value = WW_ROW("JOTPERCENTAGE")           '割合JOT
                P_ENEXPERCENTAGE.Value = WW_ROW("ENEXPERCENTAGE")           '割合ENEX
                P_BIKOU1.Value = WW_ROW("BIKOU1")           '備考1
                P_BIKOU2.Value = WW_ROW("BIKOU2")           '備考2
                P_BIKOU3.Value = WW_ROW("BIKOU3")           '備考3
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try

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

        '一意キーが未入力の場合処理を終了する
        If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("TARGETYM") = "" OrElse
                    WW_ROW("SYABAN") = "" OrElse
                    WW_ROW("SEASONKBN") = "" Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try

    End Function

    ''' <summary>
    ''' 削除フラグ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Public Sub SetDelflg(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)

        WW_ErrSW = C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0007_FIXED                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.Append("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.Append("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0007L UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' Excelデータ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdExcelData(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0007_FIXED")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TARGETYM  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,RIKUBAN  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SEASONKBN  ")
        SQLStr.AppendLine("     ,SEASONSTART  ")
        SQLStr.AppendLine("     ,SEASONEND  ")
        SQLStr.AppendLine("     ,KOTEIHIM  ")
        SQLStr.AppendLine("     ,KOTEIHID  ")
        SQLStr.AppendLine("     ,KAISU  ")
        SQLStr.AppendLine("     ,GENGAKU  ")
        SQLStr.AppendLine("     ,AMOUNT  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
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
        SQLStr.AppendLine("      @TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@ORGCODE  ")
        SQLStr.AppendLine("     ,@ORGNAME  ")
        SQLStr.AppendLine("     ,@KASANORGCODE  ")
        SQLStr.AppendLine("     ,@KASANORGNAME  ")
        SQLStr.AppendLine("     ,@TARGETYM  ")
        SQLStr.AppendLine("     ,@SYABAN  ")
        SQLStr.AppendLine("     ,@RIKUBAN  ")
        SQLStr.AppendLine("     ,@SYAGATA  ")
        SQLStr.AppendLine("     ,@SYAGATANAME  ")
        SQLStr.AppendLine("     ,@SYABARA  ")
        SQLStr.AppendLine("     ,@SEASONKBN  ")
        SQLStr.AppendLine("     ,@SEASONSTART  ")
        SQLStr.AppendLine("     ,@SEASONEND  ")
        SQLStr.AppendLine("     ,@KOTEIHIM  ")
        SQLStr.AppendLine("     ,@KOTEIHID  ")
        SQLStr.AppendLine("     ,@KAISU  ")
        SQLStr.AppendLine("     ,@GENGAKU  ")
        SQLStr.AppendLine("     ,@AMOUNT  ")
        SQLStr.AppendLine("     ,@ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,@ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,@SEGMENTCODE  ")
        SQLStr.AppendLine("     ,@SEGMENTNAME  ")
        SQLStr.AppendLine("     ,@JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,@ENEXPERCENTAGE  ")
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
        SQLStr.AppendLine("      TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGCODE =  @ORGCODE")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,TARGETYM =  @TARGETYM")
        SQLStr.AppendLine("     ,SYABAN =  @SYABAN")
        SQLStr.AppendLine("     ,RIKUBAN =  @RIKUBAN")
        SQLStr.AppendLine("     ,SYAGATA =  @SYAGATA")
        SQLStr.AppendLine("     ,SYAGATANAME =  @SYAGATANAME")
        SQLStr.AppendLine("     ,SYABARA =  @SYABARA")
        SQLStr.AppendLine("     ,SEASONKBN =  @SEASONKBN")
        SQLStr.AppendLine("     ,SEASONSTART =  @SEASONSTART")
        SQLStr.AppendLine("     ,SEASONEND =  @SEASONEND")
        SQLStr.AppendLine("     ,KOTEIHIM =  @KOTEIHIM")
        SQLStr.AppendLine("     ,KOTEIHID =  @KOTEIHID")
        SQLStr.AppendLine("     ,KAISU =  @KAISU")
        SQLStr.AppendLine("     ,GENGAKU =  @GENGAKU")
        SQLStr.AppendLine("     ,AMOUNT =  @AMOUNT")
        SQLStr.AppendLine("     ,ACCOUNTCODE =  @ACCOUNTCODE")
        SQLStr.AppendLine("     ,ACCOUNTNAME =  @ACCOUNTNAME")
        SQLStr.AppendLine("     ,SEGMENTCODE =  @SEGMENTCODE")
        SQLStr.AppendLine("     ,SEGMENTNAME =  @SEGMENTNAME")
        SQLStr.AppendLine("     ,JOTPERCENTAGE =  @JOTPERCENTAGE")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE =  @ENEXPERCENTAGE")
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
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_RIKUBAN As MySqlParameter = SQLcmd.Parameters.Add("@RIKUBAN", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_SEASONSTART As MySqlParameter = SQLcmd.Parameters.Add("@SEASONSTART", MySqlDbType.VarChar, 4)     '季節料金判定開始月日
                Dim P_SEASONEND As MySqlParameter = SQLcmd.Parameters.Add("@SEASONEND", MySqlDbType.VarChar, 4)     '季節料金判定終了月日
                Dim P_KOTEIHIM As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHIM", MySqlDbType.Decimal, 10)     '固定費(月額)
                Dim P_KOTEIHID As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHID", MySqlDbType.Decimal, 10)     '固定費(日額)
                Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数
                Dim P_GENGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GENGAKU", MySqlDbType.Decimal, 10)     '減額費用
                Dim P_AMOUNT As MySqlParameter = SQLcmd.Parameters.Add("@AMOUNT", MySqlDbType.Decimal, 10)     '請求額
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
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
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_RIKUBAN.Value = WW_ROW("RIKUBAN")           '陸事番号
                P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型

                '車型名
                Select Case WW_ROW("SYAGATA")
                    Case "1" : P_SYAGATANAME.Value = "単車"
                    Case "2" : P_SYAGATANAME.Value = "トレーラ"
                    Case Else : P_SYAGATANAME.Value = ""
                End Select

                '車腹
                If WW_ROW("SYABARA").ToString = "0" Or WW_ROW("SYABARA").ToString = "" Then
                    P_SYABARA.Value = DBNull.Value
                Else
                    P_SYABARA.Value = WW_ROW("SYABARA")
                End If

                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分

                If WW_ROW("SEASONKBN").ToString = "0" Then
                    P_SEASONSTART.Value = ""           '季節料金判定開始月日
                    P_SEASONEND.Value = ""           '季節料金判定終了月日
                Else
                    P_SEASONSTART.Value = WW_ROW("SEASONSTART")           '季節料金判定開始月日
                    P_SEASONEND.Value = WW_ROW("SEASONEND")           '季節料金判定終了月日
                End If

                '固定費(月額)
                If WW_ROW("KOTEIHIM").ToString = "0" Or WW_ROW("KOTEIHIM").ToString = "" Then
                    P_KOTEIHIM.Value = DBNull.Value
                Else
                    P_KOTEIHIM.Value = WW_ROW("KOTEIHIM")
                End If

                '固定費(日額)
                If WW_ROW("KOTEIHID").ToString = "0" Or WW_ROW("KOTEIHID").ToString = "" Then
                    P_KOTEIHID.Value = DBNull.Value
                Else
                    P_KOTEIHID.Value = WW_ROW("KOTEIHID")
                End If

                '回数
                If WW_ROW("KAISU").ToString = "0" Or WW_ROW("KAISU").ToString = "" Then
                    P_KAISU.Value = DBNull.Value
                Else
                    P_KAISU.Value = WW_ROW("KAISU")
                End If

                '減額費用
                If WW_ROW("GENGAKU").ToString = "0" Or WW_ROW("GENGAKU").ToString = "" Then
                    P_GENGAKU.Value = DBNull.Value
                Else
                    P_GENGAKU.Value = WW_ROW("GENGAKU")
                End If

                '請求額
                If WW_ROW("AMOUNT").ToString = "0" Or WW_ROW("AMOUNT").ToString = "" Then
                    P_AMOUNT.Value = DBNull.Value
                Else
                    P_AMOUNT.Value = WW_ROW("AMOUNT")
                End If

                '勘定科目コード
                If WW_ROW("ACCOUNTCODE").ToString = "0" Then
                    P_ACCOUNTCODE.Value = DBNull.Value
                Else
                    P_ACCOUNTCODE.Value = WW_ROW("ACCOUNTCODE")
                End If

                P_ACCOUNTNAME.Value = WW_ROW("ACCOUNTNAME")           '勘定科目名

                'セグメントコード
                If WW_ROW("SEGMENTCODE").ToString = "0" Then
                    P_SEGMENTCODE.Value = DBNull.Value
                Else
                    P_SEGMENTCODE.Value = WW_ROW("SEGMENTCODE")
                End If

                P_SEGMENTNAME.Value = WW_ROW("SEGMENTNAME")           'セグメント名

                '割合JOT
                If WW_ROW("JOTPERCENTAGE").ToString = "0" Then
                    P_JOTPERCENTAGE.Value = DBNull.Value
                Else
                    P_JOTPERCENTAGE.Value = WW_ROW("JOTPERCENTAGE")
                End If

                '割合ENEX
                If WW_ROW("ENEXPERCENTAGE").ToString = "0" Then
                    P_ENEXPERCENTAGE.Value = DBNull.Value
                Else
                    P_ENEXPERCENTAGE.Value = WW_ROW("ENEXPERCENTAGE")
                End If

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0007_FIXED  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

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

        '取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '取引先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGCODE", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGNAME", WW_ROW("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '加算先部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGCODE", WW_ROW("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '加算先部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGNAME", WW_ROW("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '対象年月(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TARGETYM", WW_ROW("TARGETYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・対象年月エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '車番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SYABAN", WW_ROW("SYABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車番エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '陸事番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "RIKUBAN", WW_ROW("RIKUBAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・陸事番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '車型(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SYAGATA", WW_ROW("SYAGATA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車型エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ''車型名(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "SYAGATANAME", WW_ROW("SYAGATANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・車型名エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        '車腹(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SYABARA", WW_ROW("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車腹エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '季節料金判定区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEASONKBN", WW_ROW("SEASONKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・季節料金判定区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ''季節料金判定開始月日(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "SEASONSTART", WW_ROW("SEASONSTART"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・季節料金判定開始月日エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ''季節料金判定終了月日(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "SEASONEND", WW_ROW("SEASONEND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・季節料金判定終了月日エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        '固定費(月額)(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KOTEIHIM", WW_ROW("KOTEIHIM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・固定費(月額)エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '固定費(日額)(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KOTEIHID", WW_ROW("KOTEIHID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・固定費(日額)エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '回数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KAISU", WW_ROW("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・回数エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '減額費用(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "GENGAKU", WW_ROW("GENGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・減額費用エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '請求額(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AMOUNT", WW_ROW("AMOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求額エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '勘定科目コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", WW_ROW("ACCOUNTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・勘定科目コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '勘定科目名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", WW_ROW("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・勘定科目名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        'セグメントコード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEGMENTCODE", WW_ROW("SEGMENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・セグメントコードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        'セグメント名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEGMENTNAME", WW_ROW("SEGMENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・セグメント名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '割合JOT(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "JOTPERCENTAGE", WW_ROW("JOTPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・割合JOTエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '割合ENEX(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ENEXPERCENTAGE", WW_ROW("ENEXPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・割合ENEXエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '備考1(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIKOU1", WW_ROW("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・備考1エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '備考2(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIKOU2", WW_ROW("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・備考2エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '備考3(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIKOU3", WW_ROW("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・備考3エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '季節料金判定開始月日、季節料金判定終了月日チェック
        Dim dt As DateTime
        '通年以外の場合
        If Not WW_ROW("SEASONKBN") = "0" Then
            '季節料金判定開始月日(バリデーションチェック)
            If Not WW_ROW("SEASONEND").ToString.Length = 4 OrElse DateTime.TryParse("2099/" &
                              WW_ROW("SEASONSTART").ToString.Substring(0, 2) & "/" &
                              WW_ROW("SEASONSTART").ToString.Substring(2, 2), dt) = False Then
                WW_CheckMES1 = "・季節料金判定開始月日エラーです。"
                WW_CheckMES2 = "日付入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '季節料金判定終了月日(バリデーションチェック)
            If Not WW_ROW("SEASONEND").ToString.Length = 4 OrElse DateTime.TryParse("2099/" &
                              WW_ROW("SEASONEND").ToString.Substring(0, 2) & "/" &
                              WW_ROW("SEASONEND").ToString.Substring(2, 2), dt) = False Then
                WW_CheckMES1 = "・季節料金判定終了月日エラーです。"
                WW_CheckMES2 = "日付入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '割合JOT、割合ENEX合計値チェック
        Dim WW_Decimal As Decimal
        Dim WW_JOTPERCENTAGE As Double
        Dim WW_ENEXPERCENTAGE As Double
        Dim WW_TOTALPERCENTAGE As Double

        If Decimal.TryParse(WW_ROW("JOTPERCENTAGE").ToString, WW_Decimal) Then
            WW_JOTPERCENTAGE = WW_Decimal
        Else
            WW_JOTPERCENTAGE = 0
        End If
        If Decimal.TryParse(WW_ROW("ENEXPERCENTAGE").ToString, WW_Decimal) Then
            WW_ENEXPERCENTAGE = WW_Decimal
        Else
            WW_ENEXPERCENTAGE = 0
        End If

        WW_TOTALPERCENTAGE = WW_JOTPERCENTAGE + WW_ENEXPERCENTAGE

        If WW_TOTALPERCENTAGE > 100.0 Then
            WW_CheckMES1 = "・割合JOT＆割合ENEXエラーです。"
            WW_CheckMES2 = "割合合計エラー"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

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

        '固定費マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分

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
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

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

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0006_FIXEDHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TARGETYM  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,RIKUBAN  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SEASONKBN  ")
        SQLStr.AppendLine("     ,SEASONSTART  ")
        SQLStr.AppendLine("     ,SEASONEND  ")
        SQLStr.AppendLine("     ,KOTEIHIM  ")
        SQLStr.AppendLine("     ,KOTEIHID  ")
        SQLStr.AppendLine("     ,KAISU  ")
        SQLStr.AppendLine("     ,GENGAKU  ")
        SQLStr.AppendLine("     ,AMOUNT  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
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
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TARGETYM  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,RIKUBAN  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SEASONKBN  ")
        SQLStr.AppendLine("     ,SEASONSTART  ")
        SQLStr.AppendLine("     ,SEASONEND  ")
        SQLStr.AppendLine("     ,KOTEIHIM  ")
        SQLStr.AppendLine("     ,KOTEIHID  ")
        SQLStr.AppendLine("     ,KAISU  ")
        SQLStr.AppendLine("     ,GENGAKU  ")
        SQLStr.AppendLine("     ,AMOUNT  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
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
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

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
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_SEASONKBN.Value = WW_ROW("SEASONKBN")           '季節料金判定区分

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_FIXEDHIST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0006_FIXEDHIST INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region
#End Region

End Class


