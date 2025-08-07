''************************************************************
' 統合版特別料金マスタメンテナンス・一覧画面
' 作成日 2025/03/18
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/03/18 新規作成
'          : 
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
Public Class LNM0014SprateList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0014tbl As DataTable         '一覧格納用テーブル
    Private LNM0014UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0014Exceltbl As New DataTable  'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0014tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            InputSave()
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            InputSave()
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0014WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0014WRKINC.FILETYPE.PDF)
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
                            MapInitialize()
                        Case "WF_SelectTORIChange",     '荷主(変更)時
                             "WF_SelectORGChange",      '部門(変更)時
                             "WF_SelectKASANORGChange"  '加算先部門(変更)時
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_ButtonExtract"         '検索ボタン押下時
                            GridViewInitialize()
                        Case "WF_ButtonRelease"         '解除ボタンクリック
                            MapInitialize()
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
            If Not IsNothing(LNM0014tbl) Then
                LNM0014tbl.Clear()
                LNM0014tbl.Dispose()
                LNM0014tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0014WRKINC.MAPIDL
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
            Case C_PREV_MAP_LIST.LNM0014D, C_PREV_MAP_LIST.LNM0014H
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
    Protected Sub createListBox()
        '荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0014WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0014WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next

        '加算先部門
        Me.WF_KASANORG.Items.Clear()
        Dim retKASANOrgList As New DropDownList
        retKASANOrgList = LNM0014WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retKASANOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKASANOrgList.Items(index).Text, retKASANOrgList.Items(index).Value))
        Next

#Region "コメント-2025/07/30(分類追加対応のため)"
        ''届先
        'Me.WF_TODOKE.Items.Clear()
        'Dim retTodokeList As New DropDownList
        'retTodokeList = LNM0014WRKINC.getDowpDownTodokeList(Master.MAPID, Master.ROLE_ORG)
        'For index As Integer = 0 To retTodokeList.Items.Count - 1
        '    WF_TODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
        'Next

        ''出荷地
        'Me.WF_DEPARTURE.Items.Clear()
        'Dim retDepartureList As New DropDownList
        'retDepartureList = LNM0014WRKINC.getDowpDownDepartureList(Master.MAPID, Master.ROLE_ORG)
        'For index As Integer = 0 To retDepartureList.Items.Count - 1
        '    WF_DEPARTURE.Items.Add(New ListItem(retDepartureList.Items(index).Text, retDepartureList.Items(index).Value))
        'Next
#End Region

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '参照権限の無いユーザの場合MENUへ
        If LNM0014WRKINC.AdminCheck(Master.ROLE_ORG) = False And '情シス、高圧ガス
             LNM0014WRKINC.IshikariCheck(Master.ROLE_ORG) = False And '石狩営業所
             LNM0014WRKINC.HachinoheCheck(Master.ROLE_ORG) = False And '八戸営業所
             LNM0014WRKINC.TohokuCheck(Master.ROLE_ORG) = False And '東北支店
             LNM0014WRKINC.MizushimaCheck(Master.ROLE_ORG) = False Then '水島営業所

            '○ メニュー画面遷移
            Master.TransitionPrevPage(, LNM0014WRKINC.TITLEKBNS)
        End If

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0014D Or
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0014H Then

            ' 登録画面からの遷移
            Master.RecoverTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)
            Dim WW_YM As String = Replace(work.WF_SEL_TARGETYM_L.Text, "/", "")
            WF_TaishoYm.Value = WW_YM.Substring(0, 4) & "/" & WW_YM.Substring(4, 2)
            '荷主
            WF_TORI.SelectedValue = work.WF_SEL_TORI_L.Text
            '部門
            WF_ORG.SelectedValue = work.WF_SEL_ORG_L.Text
            '加算先部門
            WF_KASANORG.SelectedValue = work.WF_SEL_KASANORG_L.Text
#Region "コメント-2025/07/30(分類追加対応のため)"
            ''届先
            'WF_TODOKE.SelectedValue = work.WF_SEL_TODOKE_L.Text
            ''出荷地
            'WF_DEPARTURE.SelectedValue = work.WF_SEL_DEPARTURE_L.Text
#End Region
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
        If LNM0014WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

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
        Master.SaveTable(LNM0014tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0014tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = "1"

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0014tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0014tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0014tbl.Rows.Count) / CONST_DISPROWCOUNT)
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0014tbl)
        Dim WW_RowFilterCMD As New StringBuilder
        WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        TBLview.RowFilter = WW_RowFilterCMD.ToString

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
        Master.SaveTable(LNM0014tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0014tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = work.WF_SEL_NOWPAGECNT_L.Text

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0014tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0014tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0014tbl.Rows.Count) / CONST_DISPROWCOUNT)
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
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            If LNM0014row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0014row("SELECT") = WW_DataCNT
            End If
        Next

        Dim TBLview As DataView = New DataView(LNM0014tbl)
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

        If IsNothing(LNM0014tbl) Then
            LNM0014tbl = New DataTable
        End If

        If LNM0014tbl.Columns.Count <> 0 Then
            LNM0014tbl.Columns.Clear()
        End If

        LNM0014tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを統合版特別料金マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNM0014.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TARGETYM), '')                                    AS TARGETYM            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TORICODE), '')                                    AS TORICODE            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TORINAME), '')                                    AS TORINAME            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ORGCODE), '')                                     AS ORGCODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ORGNAME), '')                                     AS ORGNAME             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.KASANORGCODE), '')                                AS KASANORGCODE        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.KASANORGNAME), '')                                AS KASANORGNAME        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.BIGCATECODE), '')                                 AS BIGCATECODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.BIGCATENAME), '')                                 AS BIGCATENAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.MIDCATECODE), '')                                 AS MIDCATECODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.MIDCATENAME), '')                                 AS MIDCATENAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.SMALLCATECODE), '')                               AS SMALLCATECODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.SMALLCATENAME), '')                               AS SMALLCATENAME           ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TODOKECODE), '')                                  AS TODOKECODE          ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TODOKENAME), '')                                  AS TODOKENAME          ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.GROUPSORTNO), '')                                 AS GROUPSORTNO         ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.GROUPID), '')                                     AS GROUPID             ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.GROUPNAME), '')                                   AS GROUPNAME           ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DETAILSORTNO), '')                                AS DETAILSORTNO        ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DETAILID), '')                                    AS DETAILID            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DETAILNAME), '')                                  AS DETAILNAME          ")
#End Region
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.TANKA), '')                                       AS TANKA               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.QUANTITY), '')                                    AS QUANTITY            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.CALCUNIT), '')                                    AS CALCUNIT            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DEPARTURE), '')                                   AS DEPARTURE           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.MILEAGE,'0')), '')                         AS MILEAGE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.SHIPPINGCOUNT,'0')), '')                   AS SHIPPINGCOUNT       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.NENPI,'0')), '')                           AS NENPI               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.DIESELPRICECURRENT,'0')), '')              AS DIESELPRICECURRENT  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.DIESELPRICESTANDARD,'0')), '')             AS DIESELPRICESTANDARD ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(IFNULL(LNM0014.DIESELCONSUMPTION,'0')), '')               AS DIESELCONSUMPTION   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.DISPLAYFLG), '')                                  AS DISPLAYFLG          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ASSESSMENTFLG), '')                               AS ASSESSMENTFLG       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ATENACOMPANYNAME), '')                            AS ATENACOMPANYNAME    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ATENACOMPANYDEVNAME), '')                         AS ATENACOMPANYDEVNAME ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.FROMORGNAME), '')                                 AS FROMORGNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.MEISAICATEGORYID), '')                            AS MEISAICATEGORYID    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ACCOUNTCODE), '')                                 AS ACCOUNTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ACCOUNTNAME), '')                                 AS ACCOUNTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.SEGMENTCODE), '')                                 AS SEGMENTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.SEGMENTNAME), '')                                 AS SEGMENTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.JOTPERCENTAGE), '')                               AS JOTPERCENTAGE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.ENEXPERCENTAGE), '')                              AS ENEXPERCENTAGE      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0014.BIKOU3), '')                                      AS BIKOU3              ")

        '画面表示用
        '大分類コード
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(BIGCATECODE), '') = '' THEN ''                                             ")
        SQLStr.AppendLine("      ELSE  FORMAT(BIGCATECODE,0)                                                                    ")
        SQLStr.AppendLine("     END AS SCRBIGCATECODE                                                                           ")
        '中分類コード
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(MIDCATECODE), '') = '' THEN ''                                             ")
        SQLStr.AppendLine("      ELSE  FORMAT(MIDCATECODE,0)                                                                    ")
        SQLStr.AppendLine("     END AS SCRMIDCATECODE                                                                           ")
        '小分類コード
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SMALLCATECODE), '') = '' THEN ''                                           ")
        SQLStr.AppendLine("      ELSE  FORMAT(SMALLCATECODE,0)                                                                  ")
        SQLStr.AppendLine("     END AS SCRSMALLCATECODE                                                                         ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        ''グループソート順
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GROUPSORTNO), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(GROUPSORTNO,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRGROUPSORTNO                                                                                 ")
        ''グループID
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GROUPID), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(GROUPID,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRGROUPID                                                                                 ")
        ''明細ソート順
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DETAILSORTNO), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(DETAILSORTNO,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRDETAILSORTNO                                                                                 ")
        ''明細ID
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DETAILID), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(DETAILID,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRDETAILID                                                                                 ")
#End Region
        '単価
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(TANKA), '') = '' THEN ''                                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(TANKA,2)                                                                          ")
        SQLStr.AppendLine("     END AS SCRTANKA                                                                                 ")
        '数量
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(QUANTITY), '') = '' THEN ''                                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(QUANTITY,2)                                                                          ")
        SQLStr.AppendLine("     END AS SCRQUANTITY                                                                                 ")
        '走行距離
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(MILEAGE), '') = '' THEN ''                                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(MILEAGE,2)                                                                          ")
        SQLStr.AppendLine("     END AS SCRMILEAGE                                                                                 ")
        '輸送回数
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SHIPPINGCOUNT), '') = '' THEN ''                                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(SHIPPINGCOUNT,0)                                                                          ")
        SQLStr.AppendLine("     END AS SCRSHIPPINGCOUNT                                                                                 ")
        '燃費
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(NENPI), '') = '' THEN ''                                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(NENPI,2)                                                                          ")
        SQLStr.AppendLine("     END AS SCRNENPI                                                                                 ")
        '実勢軽油価格
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELPRICECURRENT), '') = '' THEN ''                                      ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELPRICECURRENT,2)                                                             ")
        SQLStr.AppendLine("     END AS SCRDIESELPRICECURRENT                                                                    ")
        '基準経由価格
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELPRICESTANDARD), '') = '' THEN ''                                     ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELPRICESTANDARD,2)                                                            ")
        SQLStr.AppendLine("     END AS SCRDIESELPRICESTANDARD                                                                   ")
        '燃料使用量
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELCONSUMPTION), '') = '' THEN ''                                     ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELCONSUMPTION,2)                                                            ")
        SQLStr.AppendLine("     END AS SCRDIESELCONSUMPTION                                                                   ")

        '表示フラグ
        SQLStr.AppendLine("   , ''                                                                       AS SCRDISPLAYFLG        ")
        '鑑分けフラグ
        SQLStr.AppendLine("   , ''                                                                       AS SCRASSESSMENTFLG     ")
        '明細区分
        SQLStr.AppendLine("   , ''                                                                       AS SCRMEISAICATEGORYID  ")
        '割合JOT
        SQLStr.AppendLine("   , ''                                                                       AS SCRJOTPERCENTAGE   ")
        '割合ENEX
        SQLStr.AppendLine("   , ''                                                                       AS SCRENEXPERCENTAGE  ")

        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 LNM0014                                                                       ")

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
        SQLStr.AppendFormat("      AND DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim Itype As Integer

        '対象年月
        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
            SQLStr.AppendLine(" AND  COALESCE(LNM0014.TARGETYM, '0') = COALESCE(@TARGETYM, '0')  ")
        End If

        '取引先コード
        If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM0014.TORICODE = @TORICODE                                          ")
        End If

        '部門コード
        If Not String.IsNullOrEmpty(WF_ORG.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM0014.ORGCODE = @ORGCODE                                            ")
        End If

        '加算先部門コード
        If Not String.IsNullOrEmpty(WF_KASANORG.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM0014.KASANORGCODE = @KASANORGCODE                                  ")
        End If

#Region "コメント-2025/07/30(分類追加対応のため)"
        ''届先コード
        'If Not String.IsNullOrEmpty(WF_TODOKE.SelectedValue) Then
        '    SQLStr.AppendLine(" AND  LNM0014.TODOKECODE = @TODOKECODE                                      ")
        'End If

        ''出荷地
        'If Not String.IsNullOrEmpty(WF_DEPARTURE.SelectedValue) Then
        '    SQLStr.AppendLine(" AND  LNM0014.DEPARTURE = @DEPARTURE                                        ")
        'End If
#End Region

        '削除フラグ
        If Not ChkDelDataFlg.Checked Then
            SQLStr.AppendFormat(" AND  LNM0014.DELFLG = '{0}' ", C_DELETE_FLG.ALIVE)
            'Else
            '    SQLStr.AppendFormat(" AND  LNM0014.DELFLG = '{0}' ", C_DELETE_FLG.DELETE)
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0014.TARGETYM                                                           ")
        SQLStr.AppendLine("    ,LNM0014.TORICODE                                                           ")
        SQLStr.AppendLine("    ,LNM0014.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,LNM0014.BIGCATECODE                                                        ")
        SQLStr.AppendLine("    ,LNM0014.MIDCATECODE                                                        ")
        SQLStr.AppendLine("    ,LNM0014.SMALLCATECODE                                                      ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    ,LNM0014.GROUPID                                                            ")
        'SQLStr.AppendLine("    ,LNM0014.DETAILID                                                           ")
#End Region

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

                '加算先部門コード
                If Not String.IsNullOrEmpty(WF_KASANORG.SelectedValue) Then
                    Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)
                    P_KASANORGCODE.Value = WF_KASANORG.SelectedValue
                End If

#Region "コメント-2025/07/30(分類追加対応のため)"
                ''届先コード
                'If Not String.IsNullOrEmpty(WF_TODOKE.SelectedValue) Then
                '    Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)
                '    P_TODOKECODE.Value = WF_TODOKE.SelectedValue
                'End If

                ''出荷地
                'If Not String.IsNullOrEmpty(WF_DEPARTURE.SelectedValue) Then
                '    Dim P_DEPARTURE As MySqlParameter = SQLcmd.Parameters.Add("@DEPARTURE", MySqlDbType.VarChar, 50)
                '    P_DEPARTURE.Value = WF_DEPARTURE.SelectedValue
                'End If
#End Region

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0014tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0014tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0014row As DataRow In LNM0014tbl.Rows
                    i += 1
                    LNM0014row("LINECNT") = i        'LINECNT

                    '表示フラグ
                    Select Case LNM0014row("DISPLAYFLG").ToString
                        Case "0" : LNM0014row("SCRDISPLAYFLG") = "表示しない"
                        Case "1" : LNM0014row("SCRDISPLAYFLG") = "表示する"
                        Case Else : LNM0014row("SCRDISPLAYFLG") = ""
                    End Select

                    '鑑分けフラグ
                    Select Case LNM0014row("ASSESSMENTFLG").ToString
                        Case "0" : LNM0014row("SCRASSESSMENTFLG") = "鑑分けしない"
                        Case "1" : LNM0014row("SCRASSESSMENTFLG") = "鑑分けする"
                        Case Else : LNM0014row("SCRASSESSMENTFLG") = ""
                    End Select

                    '明細区分
                    Select Case LNM0014row("MEISAICATEGORYID").ToString
                        Case "1" : LNM0014row("SCRMEISAICATEGORYID") = "請求追加明細(特別料金)"
                        Case "2" : LNM0014row("SCRMEISAICATEGORYID") = "サーチャージ"
                        Case Else : LNM0014row("SCRMEISAICATEGORYID") = ""
                    End Select

                    '割合JOT
                    Select Case LNM0014row("JOTPERCENTAGE").ToString
                        Case "" : LNM0014row("SCRJOTPERCENTAGE") = ""
                        Case Else : LNM0014row("SCRJOTPERCENTAGE") = LNM0014row("JOTPERCENTAGE").ToString & "%"
                    End Select

                    '割合ENEX
                    Select Case LNM0014row("ENEXPERCENTAGE").ToString
                        Case "" : LNM0014row("SCRENEXPERCENTAGE") = ""
                        Case Else : LNM0014row("SCRENEXPERCENTAGE") = LNM0014row("ENEXPERCENTAGE").ToString & "%"
                    End Select

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014L Select"
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

        work.WF_SEL_LINECNT.Text = ""                                                   '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)          '削除

        work.WF_SEL_TARGETYM.Text = Me.WF_TaishoYm.Value                               '対象年月
        'work.WF_SEL_TARGETYM.Text = Date.Now.ToString("yyyy/MM")                       '対象年月

        If WF_TORI.SelectedValue = "" Then
            work.WF_SEL_TORICODE.Text = ""                              '取引先コード
            work.WF_SEL_TORINAME.Text = ""                              '取引先名称
        Else
            work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue           '取引先コード
            work.WF_SEL_TORINAME.Text = WF_TORI.SelectedItem.ToString   '取引先名称
        End If

        If WF_ORG.SelectedValue = "" Then
            work.WF_SEL_ORGCODE.Text = ""                               '部門コード
            work.WF_SEL_ORGNAME.Text = ""                               '部門名称
        Else
            work.WF_SEL_ORGCODE.Text = WF_ORG.SelectedValue             '部門コード
            work.WF_SEL_ORGNAME.Text = WF_ORG.SelectedItem.ToString     '部門名称
        End If

        If WF_KASANORG.SelectedValue = "" Then
            work.WF_SEL_KASANORGCODE.Text = ""                                  '加算先部門コード
            work.WF_SEL_KASANORGNAME.Text = ""                                  '加算先部門名称
        Else
            work.WF_SEL_KASANORGCODE.Text = WF_KASANORG.SelectedValue           '加算先部門コード
            work.WF_SEL_KASANORGNAME.Text = WF_KASANORG.SelectedItem.ToString   '加算先部門名称
        End If

        work.WF_SEL_BIGCATECODE.Text = ""                                               '大分類コード
        work.WF_SEL_BIGCATENAME.Text = ""                                               '大分類名
        work.WF_SEL_MIDCATECODE.Text = ""                                               '中分類コード
        work.WF_SEL_MIDCATENAME.Text = ""                                               '中分類名
        work.WF_SEL_SMALLCATECODE.Text = ""                                             '小分類コード
        work.WF_SEL_SMALLCATENAME.Text = ""                                             '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
        'work.WF_SEL_TODOKECODE.Text = ""                                                '届先コード
        'work.WF_SEL_TODOKENAME.Text = ""                                                '届先名称
        'work.WF_SEL_GROUPSORTNO.Text = ""                                               'グループソート順
        'work.WF_SEL_GROUPID.Text = ""                                                   'グループID
        'work.WF_SEL_GROUPNAME.Text = ""                                                 'グループ名
        'work.WF_SEL_DETAILSORTNO.Text = ""                                              '明細ソート順
        'work.WF_SEL_DETAILID.Text = ""                                                  '明細ID
        'work.WF_SEL_DETAILNAME.Text = ""                                                '明細名
#End Region

        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TANKA.Text)           '単価
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_QUANTITY.Text)        '数量

        work.WF_SEL_CALCUNIT.Text = ""                                                  '計算単位
        work.WF_SEL_DEPARTURE.Text = ""                                                 '出荷地
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_MILEAGE.Text)        '走行距離


        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SHIPPINGCOUNT.Text)        '輸送回数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_NENPI.Text)        '燃費
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DIESELPRICECURRENT.Text)        '実勢軽油価格
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DIESELPRICESTANDARD.Text)        '基準経由価格
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DIESELCONSUMPTION.Text)        '燃料使用量

        work.WF_SEL_DISPLAYFLG.Text = "1"                                               '請求書表示フラグ(1：表示する 0：表示しない)
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_ASSESSMENTFLG.Text)   '鑑分けフラグ

        work.WF_SEL_ATENACOMPANYNAME.Text = ""                                          '宛名会社名
        work.WF_SEL_ATENACOMPANYDEVNAME.Text = ""                                       '宛名会社部門名
        work.WF_SEL_FROMORGNAME.Text = LNM0014WRKINC.DEFAULT_FROMORGNAME                '請求書発行部店名
        work.WF_SEL_MEISAICATEGORYID.Text = "1"                                         '明細区分(1：請求追加明細(特別料金) 2：サーチャージ)
        work.WF_SEL_ACCOUNTCODE.Text = ""                                                      '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = ""                                                      '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = ""                                                      'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = ""                                                      'セグメント名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_JOTPERCENTAGE.Text)        '割合JOT
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_ENEXPERCENTAGE.Text)        '割合ENEX
        work.WF_SEL_BIKOU1.Text = ""                                                    '備考1
        work.WF_SEL_BIKOU2.Text = ""                                                    '備考2
        work.WF_SEL_BIKOU3.Text = ""                                                    '備考3

        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0014SprateHistory.aspx")
    End Sub

    ' ******************************************************************************
    ' ***  フィールド変更処理                                                    ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールド(変更)時処理
    ''' </summary>
    ''' <param name="resVal">荷主(変更)時(WF_SelectTORIChange),部門(変更)時(WF_SelectORGChange),加算先部門(変更)時(WF_SelectKASANORGChange)</param>
    ''' <remarks></remarks>
    Protected Sub WF_SelectFIELD_CHANGE(ByVal resVal As String)
        '■荷主(情報)取得
        Dim selectTORI As String = WF_TORI.SelectedValue
        Dim selectindexTORI As Integer = WF_TORI.SelectedIndex
        '■部門(情報)取得
        Dim selectORG As String = WF_ORG.SelectedValue
        Dim selectindexORG As Integer = WF_ORG.SelectedIndex
        '■加算先部門(情報)取得
        Dim selectKASANORG As String = WF_KASANORG.SelectedValue
        Dim selectindexKASANORG As Integer = WF_KASANORG.SelectedIndex

        '〇フィールド(変更)ボタン
        Select Case resVal
            '荷主(変更)時
            Case "WF_SelectTORIChange"
                selectORG = ""              '-- 部門(表示)初期化
                selectindexORG = 0          '-- 部門(INDEX)初期化
                selectKASANORG = ""         '-- 加算先部門(表示)初期化
                selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            '部門(変更)時
            Case "WF_SelectORGChange"
                selectKASANORG = ""         '-- 加算先部門(表示)初期化
                selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            '加算先部門(変更)時
            Case "WF_SelectKASANORGChange"
        End Select

        '〇荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0014WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG)
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
        retOrgList = LNM0014WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG)
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

        '〇加算先部門
        Me.WF_KASANORG.Items.Clear()
        Dim retKASANOrgList As New DropDownList
        retKASANOrgList = LNM0014WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG)
        '★ドロップダウンリスト選択(加算先部門)の場合
        If retKASANOrgList.Items(0).Text <> "全て表示" Then
            WF_KASANORG.Items.Add(New ListItem("全て表示", ""))
            selectindexKASANORG = 1
        End If
        '★ドロップダウンリスト再作成(加算先部門)
        For index As Integer = 0 To retKASANOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKASANOrgList.Items(index).Text, retKASANOrgList.Items(index).Value))
        Next
        WF_KASANORG.SelectedIndex = selectindexKASANORG

    End Sub

    ''' <summary>
    ''' 画面初期化処理
    ''' </summary>
    Private Sub MapInitialize()
        'ドロップダウン生成処理
        createListBox()

        'GridViewデータ設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNM0014tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNM0014tbl)

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
        Dim TBLview As New DataView(LNM0014tbl)
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
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            If LNM0014row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0014row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNM0014tbl)

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
        work.WF_SEL_TARGETYM_L.Text = WF_TaishoYm.Value         '対象日
        work.WF_SEL_TORI_L.Text = WF_TORI.SelectedValue         '荷主
        work.WF_SEL_ORG_L.Text = WF_ORG.SelectedValue           '部門
        work.WF_SEL_KASANORG_L.Text = WF_KASANORG.SelectedValue '加算先部門
#Region "コメント-2025/07/30(分類追加対応のため)"
        'work.WF_SEL_TODOKE_L.Text = WF_TODOKE.SelectedValue ' 届先
        'work.WF_SEL_DEPARTURE_L.Text = WF_DEPARTURE.SelectedValue '出荷地
#End Region
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
        If LNM0014tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE Then
            Dim WW_ROW As DataRow
            WW_ROW = LNM0014tbl.Rows(WW_LineCNT)
            Dim DATENOW As Date = Date.Now
            Dim WW_UPDTIMSTP As Date

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                '履歴登録(変更前)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0014WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '削除フラグ有効化
                DelflgValid(SQLcon, WW_ROW, DATENOW, WW_UPDTIMSTP)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '履歴登録(変更後)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0014WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                LNM0014tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.ALIVE
                LNM0014tbl.Rows(WW_LineCNT)("UPDTIMSTP") = WW_UPDTIMSTP
                Master.SaveTable(LNM0014tbl)
                Master.Output(C_MESSAGE_NO.DELETE_ROW_ACTIVATION, C_MESSAGE_TYPE.NOR, needsPopUp:=True)
            End Using
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = LNM0014tbl.Rows(WW_LineCNT)("LINECNT")            '選択行

        work.WF_SEL_TARGETYM.Text = LNM0014tbl.Rows(WW_LineCNT)("TARGETYM")                                  '対象年月
        work.WF_SEL_TORICODE.Text = LNM0014tbl.Rows(WW_LineCNT)("TORICODE")                                  '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0014tbl.Rows(WW_LineCNT)("TORINAME")                                  '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0014tbl.Rows(WW_LineCNT)("ORGCODE")                                    '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("ORGNAME")                                    '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0014tbl.Rows(WW_LineCNT)("KASANORGCODE")                          '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("KASANORGNAME")                          '加算先部門名称
        work.WF_SEL_BIGCATECODE.Text = LNM0014tbl.Rows(WW_LineCNT)("BIGCATECODE")                           '大分類コード
        work.WF_SEL_BIGCATENAME.Text = LNM0014tbl.Rows(WW_LineCNT)("BIGCATENAME")                           '大分類名
        work.WF_SEL_MIDCATECODE.Text = LNM0014tbl.Rows(WW_LineCNT)("MIDCATECODE")                           '中分類コード
        work.WF_SEL_MIDCATENAME.Text = LNM0014tbl.Rows(WW_LineCNT)("MIDCATENAME")                           '中分類名
        work.WF_SEL_SMALLCATECODE.Text = LNM0014tbl.Rows(WW_LineCNT)("SMALLCATECODE")                       '小分類コード
        work.WF_SEL_SMALLCATENAME.Text = LNM0014tbl.Rows(WW_LineCNT)("SMALLCATENAME")                       '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
        'work.WF_SEL_TODOKECODE.Text = LNM0014tbl.Rows(WW_LineCNT)("TODOKECODE")                              '届先コード
        'work.WF_SEL_TODOKENAME.Text = LNM0014tbl.Rows(WW_LineCNT)("TODOKENAME")                              '届先名称
        'work.WF_SEL_GROUPSORTNO.Text = LNM0014tbl.Rows(WW_LineCNT)("GROUPSORTNO")                            'グループソート順
        'work.WF_SEL_GROUPID.Text = LNM0014tbl.Rows(WW_LineCNT)("GROUPID")                                    'グループID
        'work.WF_SEL_GROUPNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("GROUPNAME")                                'グループ名
        'work.WF_SEL_DETAILSORTNO.Text = LNM0014tbl.Rows(WW_LineCNT)("DETAILSORTNO")                          '明細ソート順
        'work.WF_SEL_DETAILID.Text = LNM0014tbl.Rows(WW_LineCNT)("DETAILID")                                  '明細ID
        'work.WF_SEL_DETAILNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("DETAILNAME")                              '明細名
#End Region
        work.WF_SEL_TANKA.Text = LNM0014tbl.Rows(WW_LineCNT)("TANKA")                                        '単価
        work.WF_SEL_QUANTITY.Text = LNM0014tbl.Rows(WW_LineCNT)("QUANTITY")                                  '数量
        work.WF_SEL_CALCUNIT.Text = LNM0014tbl.Rows(WW_LineCNT)("CALCUNIT")                                  '計算単位
        work.WF_SEL_DEPARTURE.Text = LNM0014tbl.Rows(WW_LineCNT)("DEPARTURE")                                '出荷地
        work.WF_SEL_MILEAGE.Text = LNM0014tbl.Rows(WW_LineCNT)("MILEAGE")                                    '走行距離
        work.WF_SEL_SHIPPINGCOUNT.Text = LNM0014tbl.Rows(WW_LineCNT)("SHIPPINGCOUNT")                        '輸送回数
        work.WF_SEL_NENPI.Text = LNM0014tbl.Rows(WW_LineCNT)("NENPI")                                        '燃費
        work.WF_SEL_DIESELPRICECURRENT.Text = LNM0014tbl.Rows(WW_LineCNT)("DIESELPRICECURRENT")              '実勢軽油価格
        work.WF_SEL_DIESELPRICESTANDARD.Text = LNM0014tbl.Rows(WW_LineCNT)("DIESELPRICESTANDARD")            '基準経由価格
        work.WF_SEL_DIESELCONSUMPTION.Text = LNM0014tbl.Rows(WW_LineCNT)("DIESELCONSUMPTION")                '燃料使用量
        work.WF_SEL_DISPLAYFLG.Text = LNM0014tbl.Rows(WW_LineCNT)("DISPLAYFLG")                              '表示フラグ
        work.WF_SEL_ASSESSMENTFLG.Text = LNM0014tbl.Rows(WW_LineCNT)("ASSESSMENTFLG")                        '鑑分けフラグ
        work.WF_SEL_ATENACOMPANYNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("ATENACOMPANYNAME")                  '宛名会社名
        work.WF_SEL_ATENACOMPANYDEVNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("ATENACOMPANYDEVNAME")            '宛名会社部門名
        work.WF_SEL_FROMORGNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("FROMORGNAME")                            '請求書発行部店名
        work.WF_SEL_MEISAICATEGORYID.Text = LNM0014tbl.Rows(WW_LineCNT)("MEISAICATEGORYID")                  '明細区分
        work.WF_SEL_ACCOUNTCODE.Text = LNM0014tbl.Rows(WW_LineCNT)("ACCOUNTCODE")                   '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("ACCOUNTNAME")                   '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = LNM0014tbl.Rows(WW_LineCNT)("SEGMENTCODE")                   'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = LNM0014tbl.Rows(WW_LineCNT)("SEGMENTNAME")                   'セグメント名
        work.WF_SEL_JOTPERCENTAGE.Text = LNM0014tbl.Rows(WW_LineCNT)("JOTPERCENTAGE")               '割合JOT
        work.WF_SEL_ENEXPERCENTAGE.Text = LNM0014tbl.Rows(WW_LineCNT)("ENEXPERCENTAGE")             '割合ENEX
        work.WF_SEL_BIKOU1.Text = LNM0014tbl.Rows(WW_LineCNT)("BIKOU1")                                      '備考1
        work.WF_SEL_BIKOU2.Text = LNM0014tbl.Rows(WW_LineCNT)("BIKOU2")                                      '備考2
        work.WF_SEL_BIKOU3.Text = LNM0014tbl.Rows(WW_LineCNT)("BIKOU3")                                      '備考3

        work.WF_SEL_DELFLG.Text = LNM0014tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0014tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                            work.WF_SEL_TARGETYM.Text, work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                            work.WF_SEL_BIGCATECODE.Text, work.WF_SEL_MIDCATECODE.Text, work.WF_SEL_SMALLCATECODE.Text)
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
        SQLStr.Append("     LNG.LNM0014_SPRATE2                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.AppendFormat("     DELFLG               = '{0}' ", C_DELETE_FLG.ALIVE)
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TARGETYM, '')        = @TARGETYM ")
        SQLStr.Append("    AND  COALESCE(TORICODE, '')        = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')         = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(BIGCATECODE, '0')    = @BIGCATECODE ")
        SQLStr.Append("    AND  COALESCE(MIDCATECODE, '0')    = @MIDCATECODE ")
        SQLStr.Append("    AND  COALESCE(SMALLCATECODE, '0')  = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.Append("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.Append("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                 '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)            '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)        '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)            '更新プログラムＩＤ

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region
                P_UPDYMD.Value = WW_NOW                         '更新年月日
                P_UPDUSER.Value = Master.USERID                 '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID           '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name    '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014L UPDATE"
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
        SQLStrTimStp.AppendLine("     LNG.LNM0014_SPRATE2                               ")
        SQLStrTimStp.AppendLine(" WHERE                                                 ")
        SQLStrTimStp.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStrTimStp.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStrTimStp.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStrTimStp.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 SELECT"
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0014WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "特別料金マスタ一覧"
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

        Dim FileName As String
        Dim FilePath As String
        Select Case WW_FILETYPE
            Case LNM0014WRKINC.FILETYPE.EXCEL
                FileName = "特別料金マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0014WRKINC.FILETYPE.PDF
                FileName = "特別料金マスタ.pdf"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Pdf)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
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
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)          '削除フラグ
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.TARGETYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)        '対象年月
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)         '部門コード
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)     '大分類コード
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)     '中分類コード
        sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)   '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
        'sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        'sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.GROUPID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'グループID
        'sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.DETAILID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '明細ID
#End Region

        '入力不要列網掛け
        'sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'グループソート順
        'sheet.Columns(LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '明細ソート順

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
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TARGETYM).Value = "（必須）対象年月"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE).Value = "（必須）大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATENAME).Value = "大分類名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE).Value = "（必須）中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATENAME).Value = "中分類名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE).Value = "（必須）小分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATENAME).Value = "小分類名"
#Region "コメント-2025/07/30(分類追加対応のため)"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKECODE).Value = "届先コード"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKENAME).Value = "届先名称"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO).Value = "グループソート順"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPID).Value = "（必須）グループID"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPNAME).Value = "グループ名"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO).Value = "明細ソート順"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILID).Value = "（必須）明細ID"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILNAME).Value = "明細名"
#End Region
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.TANKA).Value = "単価"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.QUANTITY).Value = "数量"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.CALCUNIT).Value = "計算単位"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DEPARTURE).Value = "出荷地"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MILEAGE).Value = "走行距離"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = "輸送回数"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.NENPI).Value = "燃費"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = "実勢軽油価格"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = "基準経由価格"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELCONSUMPTION).Value = "燃料使用量"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DISPLAYFLG).Value = "表示フラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ASSESSMENTFLG).Value = "鑑分けフラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYNAME).Value = "宛名会社名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYDEVNAME).Value = "宛名会社部門名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.FROMORGNAME).Value = "請求書発行部店名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MEISAICATEGORYID).Value = "明細区分"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = "勘定科目コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = "勘定科目名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = "セグメントコード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = "セグメント名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = "割合JOT"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = "割合ENEX"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU1).Value = "備考1"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU2).Value = "備考2"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU3).Value = "備考3"

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
                sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '表示フラグ
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("0:表示しない")
            WW_TEXTLIST.AppendLine("1:表示する")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DISPLAYFLG).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.DISPLAYFLG).Comment.Shape
                .Width = 100
                .Height = 30
            End With

            '鑑分けフラグ
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("0:鑑分けしない")
            WW_TEXTLIST.AppendLine("1:鑑分けする")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ASSESSMENTFLG).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ASSESSMENTFLG).Comment.Shape
                .Width = 100
                .Height = 30
            End With

            '明細区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:請求追加明細(特別料金)")
            WW_TEXTLIST.AppendLine("2:サーチャージ")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MEISAICATEGORYID).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.MEISAICATEGORYID).Comment.Shape
                .Width = 150
                .Height = 30
            End With

            '割合JOT
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("JOT手数料として収受する割合(JOT収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Comment.Shape
                .Width = 400
                .Height = 30
            End With

            '割合ENEX
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("ENEXへ支払う割合(ENEX収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Comment.Shape
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
        SETFIXVALUELIST(subsheet, "DELFLG", LNM0014WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0014WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG)
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

        ''シートの保護をかけるとリボンも操作できなくなるため
        ''データの入力規則で対応(該当セルの入力可能文字数を0にする)

        ''枝番
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0014WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0014WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'With sheet.Range(WW_STRANGE.Address & ":" & WW_ENDRANGE.Address).Validation
        '    .Add(type:=ValidationType.TextLength, validationOperator:=ValidationOperator.LessEqual, formula1:=0)
        'End With

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
        DecStyle.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        '数値書式(小数点含む)
        Dim DecStyle2 As IStyle = wb.Styles.Add("DecStyle2")
        DecStyle2.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNM0014tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TARGETYM).Value = Row("TARGETYM") '対象年月
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称

            '大分類コード
            If Row("BIGCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE).Value = Row("BIGCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE).Value = CDbl(Row("BIGCATECODE"))
            End If
            '大分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATENAME).Value = Row("BIGCATENAME")

            '中分類コード
            If Row("MIDCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE).Value = Row("MIDCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE).Value = CDbl(Row("MIDCATECODE"))
            End If
            '中分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATENAME).Value = Row("MIDCATENAME")

            '小分類コード
            If Row("SMALLCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE).Value = Row("SMALLCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE).Value = CDbl(Row("SMALLCATECODE"))
            End If
            '小分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATENAME).Value = Row("SMALLCATENAME")

#Region "コメント-2025/07/30(分類追加対応のため)"
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKECODE).Value = Row("TODOKECODE") '届先コード
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKENAME).Value = Row("TODOKENAME") '届先名称

            ''グループソート順
            'If Row("GROUPSORTNO") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO).Value = Row("GROUPSORTNO")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO).Value = CDbl(Row("GROUPSORTNO"))
            'End If

            ''グループID
            'If Row("GROUPID") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPID).Value = Row("GROUPID")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPID).Value = CDbl(Row("GROUPID"))
            'End If

            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPNAME).Value = Row("GROUPNAME") 'グループ名

            ''明細ソート順
            'If Row("DETAILSORTNO") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO).Value = Row("DETAILSORTNO")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO).Value = CDbl(Row("DETAILSORTNO"))
            'End If

            ''明細ID
            'If Row("DETAILID") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILID).Value = Row("DETAILID")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILID).Value = CDbl(Row("DETAILID"))
            'End If

            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILNAME).Value = Row("DETAILNAME") '明細名
#End Region

            '単価
            If Row("TANKA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TANKA).Value = Row("TANKA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TANKA).Value = CDbl(Row("TANKA"))
            End If

            '数量
            If Row("QUANTITY") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.QUANTITY).Value = Row("QUANTITY")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.QUANTITY).Value = CDbl(Row("QUANTITY"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.CALCUNIT).Value = Row("CALCUNIT") '計算単位
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DEPARTURE).Value = Row("DEPARTURE") '出荷地

            '走行距離
            If Row("MILEAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MILEAGE).Value = Row("MILEAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MILEAGE).Value = CDbl(Row("MILEAGE"))
            End If

            '輸送回数
            If Row("SHIPPINGCOUNT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = Row("SHIPPINGCOUNT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = CDbl(Row("SHIPPINGCOUNT"))
            End If

            '燃費
            If Row("NENPI") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.NENPI).Value = Row("NENPI")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.NENPI).Value = CDbl(Row("NENPI"))
            End If

            '実勢軽油価格
            If Row("DIESELPRICECURRENT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = Row("DIESELPRICECURRENT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = CDbl(Row("DIESELPRICECURRENT"))
            End If

            '基準経由価格
            If Row("DIESELPRICESTANDARD") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = Row("DIESELPRICESTANDARD")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = CDbl(Row("DIESELPRICESTANDARD"))
            End If

            '燃料使用量
            If Row("DIESELCONSUMPTION") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELCONSUMPTION).Value = Row("DIESELCONSUMPTION")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELCONSUMPTION).Value = CDbl(Row("DIESELCONSUMPTION"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DISPLAYFLG).Value = Row("DISPLAYFLG") '表示フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ASSESSMENTFLG).Value = Row("ASSESSMENTFLG") '鑑分けフラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYNAME).Value = Row("ATENACOMPANYNAME") '宛名会社名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYDEVNAME).Value = Row("ATENACOMPANYDEVNAME") '宛名会社部門名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.FROMORGNAME).Value = Row("FROMORGNAME") '請求書発行部店名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MEISAICATEGORYID).Value = Row("MEISAICATEGORYID") '明細区分

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = Row("ACCOUNTCODE") '勘定科目コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '勘定科目名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = Row("SEGMENTCODE") 'セグメントコード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = Row("SEGMENTNAME") 'セグメント名

            '割合JOT
            If Row("JOTPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = Row("JOTPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = CDbl(Row("JOTPERCENTAGE"))
            End If

            '割合ENEX
            If Row("ENEXPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = Row("ENEXPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = CDbl(Row("ENEXPERCENTAGE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3

            '数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE).Style = IntStyle
#Region "コメント-2025/07/30(分類追加対応のため)"
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPID).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILID).Style = IntStyle
#End Region
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.TANKA).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.QUANTITY).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.MILEAGE).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.NENPI).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELCONSUMPTION).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Style = DecStyle2
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Style = DecStyle2

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

            For Each Row As DataRow In LNM0014Exceltbl.Rows
                Select Case True
                    Case Row("BIGCATECODE").ToString = "0"      '大分類コードが無い場合
                        '大分類コードを生成
                        Row("BIGCATECODE") = LNM0014WRKINC.GenerateBigcateCode(SQLcon, Row, WW_DBDataCheck)
                        'Row("MIDCATECODE") = "1"
                        'Row("SMALLCATECODE") = "1"
                    Case Row("MIDCATECODE").ToString = "0"      '中分類コードが無い場合
                        '中分類コードを生成
                        Row("MIDCATECODE") = LNM0014WRKINC.GenerateMidcateCode(SQLcon, Row, WW_DBDataCheck)
                        'Row("SMALLCATECODE") = "1"
                    Case Row("SMALLCATECODE").ToString = "0"    '小分類コードが無い場合
                        '小分類コードを生成
                        Row("SMALLCATECODE") = LNM0014WRKINC.GenerateSmallcateCode(SQLcon, Row, WW_DBDataCheck)
#Region "コメント-2025/07/30(分類追加対応のため)"
                        'Case Row("GROUPID").ToString = "0" 'グループIDが無い場合
                        '    'グループIDを生成
                        '    Row("GROUPID") = LNM0014WRKINC.GenerateGroupId(SQLcon, Row, WW_DBDataCheck)
                        '    Row("DETAILID") = "1"
                        'Case Row("DETAILID").ToString = "0"  '明細IDが無い場合
                        '    Row("DETAILID") = LNM0014WRKINC.GenerateDetailId(SQLcon, Row, WW_DBDataCheck)
#End Region
                    Case Else '大分類, 中分類, 小分類が設定されている場合は何もしない
                End Select

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0014WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0014WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0014WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0014WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "特別料金マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0014Exceltbl) Then
            LNM0014Exceltbl = New DataTable
        End If
        If LNM0014Exceltbl.Columns.Count <> 0 Then
            LNM0014Exceltbl.Columns.Clear()
        End If
        LNM0014Exceltbl.Clear()

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

            For Each Row As DataRow In LNM0014Exceltbl.Rows

                Select Case True
                    Case Row("BIGCATECODE").ToString = "0" '大分類コードが無い場合
                        '大分類コードを生成
                        Row("BIGCATECODE") = LNM0014WRKINC.GenerateBigcateCode(SQLcon, Row, WW_DBDataCheck)
                        'Row("MIDCATECODE") = "1"
                        'Row("SMALLCATECODE") = "1"
                    Case Row("MIDCATECODE").ToString = "0" '中分類コードが無い場合
                        '中分類コードを生成
                        Row("MIDCATECODE") = LNM0014WRKINC.GenerateMidcateCode(SQLcon, Row, WW_DBDataCheck)
                        'Row("SMALLCATECODE") = "1"
                    Case Row("SMALLCATECODE").ToString = "0" '小分類コードが無い場合
                        '小分類コードを生成
                        Row("SMALLCATECODE") = LNM0014WRKINC.GenerateSmallcateCode(SQLcon, Row, WW_DBDataCheck)
#Region "コメント-2025/07/30(分類追加対応のため)"
                        'Case Row("GROUPID").ToString = "0" 'グループIDが無い場合
                        '    'グループIDを生成
                        '    Row("GROUPID") = LNM0014WRKINC.GenerateGroupId(SQLcon, Row, WW_DBDataCheck)
                        '    Row("DETAILID") = "1"
                        'Case Row("DETAILID").ToString = "0" '明細IDが無い場合
                        '    Row("DETAILID") = LNM0014WRKINC.GenerateDetailId(SQLcon, Row, WW_DBDataCheck)
#End Region
                    Case Else '大分類, 中分類, 小分類が設定されている場合は何もしない
                End Select

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0014WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0014WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0014WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0014WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = C_DELETE_FLG.DELETE            '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,TARGETYM  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,ORGCODE  ")
        SQLStr.AppendLine("        ,ORGNAME  ")
        SQLStr.AppendLine("        ,KASANORGCODE  ")
        SQLStr.AppendLine("        ,KASANORGNAME  ")
        SQLStr.AppendLine("        ,BIGCATECODE  ")
        SQLStr.AppendLine("        ,BIGCATENAME  ")
        SQLStr.AppendLine("        ,MIDCATECODE  ")
        SQLStr.AppendLine("        ,MIDCATENAME  ")
        SQLStr.AppendLine("        ,SMALLCATECODE  ")
        SQLStr.AppendLine("        ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("        ,TODOKECODE  ")
        'SQLStr.AppendLine("        ,TODOKENAME  ")
        'SQLStr.AppendLine("        ,GROUPSORTNO  ")
        'SQLStr.AppendLine("        ,GROUPID  ")
        'SQLStr.AppendLine("        ,GROUPNAME  ")
        'SQLStr.AppendLine("        ,DETAILSORTNO  ")
        'SQLStr.AppendLine("        ,DETAILID  ")
        'SQLStr.AppendLine("        ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("        ,TANKA  ")
        SQLStr.AppendLine("        ,QUANTITY  ")
        SQLStr.AppendLine("        ,CALCUNIT  ")
        SQLStr.AppendLine("        ,DEPARTURE  ")
        SQLStr.AppendLine("        ,MILEAGE  ")
        SQLStr.AppendLine("        ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("        ,NENPI  ")
        SQLStr.AppendLine("        ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("        ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("        ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("        ,DISPLAYFLG  ")
        SQLStr.AppendLine("        ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("        ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("        ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("        ,FROMORGNAME  ")
        SQLStr.AppendLine("        ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine(" FROM LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0014Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 SELECT"
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

            Dim LNM0014Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNM0014Exceltblrow = LNM0014Exceltbl.NewRow

                'LINECNT
                LNM0014Exceltblrow("LINECNT") = WW_LINECNT
                WW_LINECNT = WW_LINECNT + 1

                '◆データセット
                '対象年月
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TARGETYM))
                WW_DATATYPE = DataTypeHT("TARGETYM")
                LNM0014Exceltblrow("TARGETYM") = LNM0014WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TORICODE))
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNM0014Exceltblrow("TORICODE") = LNM0014WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TORINAME))
                WW_DATATYPE = DataTypeHT("TORINAME")
                LNM0014Exceltblrow("TORINAME") = LNM0014WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ORGCODE))
                WW_DATATYPE = DataTypeHT("ORGCODE")
                LNM0014Exceltblrow("ORGCODE") = LNM0014WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ORGNAME))
                WW_DATATYPE = DataTypeHT("ORGNAME")
                LNM0014Exceltblrow("ORGNAME") = LNM0014WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGCODE))
                WW_DATATYPE = DataTypeHT("KASANORGCODE")
                LNM0014Exceltblrow("KASANORGCODE") = LNM0014WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.KASANORGNAME))
                WW_DATATYPE = DataTypeHT("KASANORGNAME")
                LNM0014Exceltblrow("KASANORGNAME") = LNM0014WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '大分類コード
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATECODE)), ",", "")
                WW_DATATYPE = DataTypeHT("BIGCATECODE")
                LNM0014Exceltblrow("BIGCATECODE") = LNM0014WRKINC.DataConvert("大分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '大分類名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.BIGCATENAME))
                WW_DATATYPE = DataTypeHT("BIGCATENAME")
                LNM0014Exceltblrow("BIGCATENAME") = LNM0014WRKINC.DataConvert("大分類名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '中分類コード
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATECODE)), ",", "")
                WW_DATATYPE = DataTypeHT("MIDCATECODE")
                LNM0014Exceltblrow("MIDCATECODE") = LNM0014WRKINC.DataConvert("中分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '中分類名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.MIDCATENAME))
                WW_DATATYPE = DataTypeHT("MIDCATENAME")
                LNM0014Exceltblrow("MIDCATENAME") = LNM0014WRKINC.DataConvert("中分類名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '小分類コード
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATECODE)), ",", "")
                WW_DATATYPE = DataTypeHT("SMALLCATECODE")
                LNM0014Exceltblrow("SMALLCATECODE") = LNM0014WRKINC.DataConvert("小分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '小分類名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.SMALLCATENAME))
                WW_DATATYPE = DataTypeHT("SMALLCATENAME")
                LNM0014Exceltblrow("SMALLCATENAME") = LNM0014WRKINC.DataConvert("小分類名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
#Region "コメント-2025/07/30(分類追加対応のため)"
                ''届先コード
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKECODE))
                'WW_DATATYPE = DataTypeHT("TODOKECODE")
                'LNM0014Exceltblrow("TODOKECODE") = LNM0014WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''届先名称
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TODOKENAME))
                'WW_DATATYPE = DataTypeHT("TODOKENAME")
                'LNM0014Exceltblrow("TODOKENAME") = LNM0014WRKINC.DataConvert("届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''グループソート順
                'WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPSORTNO)), ",", "")
                'WW_DATATYPE = DataTypeHT("GROUPSORTNO")
                'LNM0014Exceltblrow("GROUPSORTNO") = LNM0014WRKINC.DataConvert("グループソート順", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''グループID
                'WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPID)), ",", "")
                'WW_DATATYPE = DataTypeHT("GROUPID")
                'LNM0014Exceltblrow("GROUPID") = LNM0014WRKINC.DataConvert("グループID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''グループ名
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.GROUPNAME))
                'WW_DATATYPE = DataTypeHT("GROUPNAME")
                'LNM0014Exceltblrow("GROUPNAME") = LNM0014WRKINC.DataConvert("グループ名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''明細ソート順
                'WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILSORTNO)), ",", "")
                'WW_DATATYPE = DataTypeHT("DETAILSORTNO")
                'LNM0014Exceltblrow("DETAILSORTNO") = LNM0014WRKINC.DataConvert("明細ソート順", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''明細ID
                'WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILID)), ",", "")
                'WW_DATATYPE = DataTypeHT("DETAILID")
                'LNM0014Exceltblrow("DETAILID") = LNM0014WRKINC.DataConvert("明細ID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''明細名
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DETAILNAME))
                'WW_DATATYPE = DataTypeHT("DETAILNAME")
                'LNM0014Exceltblrow("DETAILNAME") = LNM0014WRKINC.DataConvert("明細名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
#End Region
                '単価
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.TANKA)), ",", "")
                WW_DATATYPE = DataTypeHT("TANKA")
                LNM0014Exceltblrow("TANKA") = LNM0014WRKINC.DataConvert("単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '数量
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.QUANTITY)), ",", "")
                WW_DATATYPE = DataTypeHT("QUANTITY")
                LNM0014Exceltblrow("QUANTITY") = LNM0014WRKINC.DataConvert("数量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '計算単位
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.CALCUNIT))
                WW_DATATYPE = DataTypeHT("CALCUNIT")
                LNM0014Exceltblrow("CALCUNIT") = LNM0014WRKINC.DataConvert("計算単位", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '出荷地
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DEPARTURE))
                WW_DATATYPE = DataTypeHT("DEPARTURE")
                LNM0014Exceltblrow("DEPARTURE") = LNM0014WRKINC.DataConvert("出荷地", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '走行距離
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.MILEAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("MILEAGE")
                LNM0014Exceltblrow("MILEAGE") = LNM0014WRKINC.DataConvert("走行距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '輸送回数
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT)), ",", "")
                WW_DATATYPE = DataTypeHT("SHIPPINGCOUNT")
                LNM0014Exceltblrow("SHIPPINGCOUNT") = LNM0014WRKINC.DataConvert("輸送回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '燃費
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.NENPI)), ",", "")
                WW_DATATYPE = DataTypeHT("NENPI")
                LNM0014Exceltblrow("NENPI") = LNM0014WRKINC.DataConvert("燃費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実勢軽油価格
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT)), ",", "")
                WW_DATATYPE = DataTypeHT("DIESELPRICECURRENT")
                LNM0014Exceltblrow("DIESELPRICECURRENT") = LNM0014WRKINC.DataConvert("実勢軽油価格", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '基準経由価格
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD)), ",", "")
                WW_DATATYPE = DataTypeHT("DIESELPRICESTANDARD")
                LNM0014Exceltblrow("DIESELPRICESTANDARD") = LNM0014WRKINC.DataConvert("基準経由価格", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '燃料使用量
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DIESELCONSUMPTION)), ",", "")
                WW_DATATYPE = DataTypeHT("DIESELCONSUMPTION")
                LNM0014Exceltblrow("DIESELCONSUMPTION") = LNM0014WRKINC.DataConvert("燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '表示フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DISPLAYFLG))
                WW_DATATYPE = DataTypeHT("DISPLAYFLG")
                LNM0014Exceltblrow("DISPLAYFLG") = LNM0014WRKINC.DataConvert("表示フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '鑑分けフラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ASSESSMENTFLG))
                WW_DATATYPE = DataTypeHT("ASSESSMENTFLG")
                LNM0014Exceltblrow("ASSESSMENTFLG") = LNM0014WRKINC.DataConvert("鑑分けフラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '宛名会社名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYNAME))
                WW_DATATYPE = DataTypeHT("ATENACOMPANYNAME")
                LNM0014Exceltblrow("ATENACOMPANYNAME") = LNM0014WRKINC.DataConvert("宛名会社名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '宛名会社部門名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ATENACOMPANYDEVNAME))
                WW_DATATYPE = DataTypeHT("ATENACOMPANYDEVNAME")
                LNM0014Exceltblrow("ATENACOMPANYDEVNAME") = LNM0014WRKINC.DataConvert("宛名会社部門名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求書発行部店名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.FROMORGNAME))
                WW_DATATYPE = DataTypeHT("FROMORGNAME")
                LNM0014Exceltblrow("FROMORGNAME") = LNM0014WRKINC.DataConvert("請求書発行部店名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '明細区分
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.MEISAICATEGORYID))
                WW_DATATYPE = DataTypeHT("MEISAICATEGORYID")
                LNM0014Exceltblrow("MEISAICATEGORYID") = LNM0014WRKINC.DataConvert("明細区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTCODE))
                WW_DATATYPE = DataTypeHT("ACCOUNTCODE")
                LNM0014Exceltblrow("ACCOUNTCODE") = LNM0014WRKINC.DataConvert("勘定科目コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ACCOUNTNAME))
                WW_DATATYPE = DataTypeHT("ACCOUNTNAME")
                LNM0014Exceltblrow("ACCOUNTNAME") = LNM0014WRKINC.DataConvert("勘定科目名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメントコード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTCODE))
                WW_DATATYPE = DataTypeHT("SEGMENTCODE")
                LNM0014Exceltblrow("SEGMENTCODE") = LNM0014WRKINC.DataConvert("セグメントコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメント名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.SEGMENTNAME))
                WW_DATATYPE = DataTypeHT("SEGMENTNAME")
                LNM0014Exceltblrow("SEGMENTNAME") = LNM0014WRKINC.DataConvert("セグメント名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合JOT
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.JOTPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("JOTPERCENTAGE")
                LNM0014Exceltblrow("JOTPERCENTAGE") = LNM0014WRKINC.DataConvert("割合JOT", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合ENEX
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("ENEXPERCENTAGE")
                LNM0014Exceltblrow("ENEXPERCENTAGE") = LNM0014WRKINC.DataConvert("割合ENEX", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考1
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU1))
                WW_DATATYPE = DataTypeHT("BIKOU1")
                LNM0014Exceltblrow("BIKOU1") = LNM0014WRKINC.DataConvert("備考1", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考2
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU2))
                WW_DATATYPE = DataTypeHT("BIKOU2")
                LNM0014Exceltblrow("BIKOU2") = LNM0014WRKINC.DataConvert("備考2", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考3
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.BIKOU3))
                WW_DATATYPE = DataTypeHT("BIKOU3")
                LNM0014Exceltblrow("BIKOU3") = LNM0014WRKINC.DataConvert("備考3", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0014WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNM0014Exceltblrow("DELFLG") = LNM0014WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '登録
                LNM0014Exceltbl.Rows.Add(LNM0014Exceltblrow)

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
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')              = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')              = @ORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')         = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')         = @KASANORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')         = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATENAME, '')          = @BIGCATENAME ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')         = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATENAME, '')          = @MIDCATENAME ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0')       = @SMALLCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATENAME, '')        = @SMALLCATENAME ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
        'SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
        'SQLStr.AppendLine("    AND  COALESCE(GROUPSORTNO, '0')             = @GROUPSORTNO ")
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(GROUPNAME, '')             = @GROUPNAME ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILSORTNO, '0')             = @DETAILSORTNO ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILNAME, '')             = @DETAILNAME ")
#End Region
        SQLStr.AppendLine("    AND  COALESCE(TANKA, '0')               = @TANKA ")
        SQLStr.AppendLine("    AND  COALESCE(QUANTITY, '0')            = @QUANTITY ")
        SQLStr.AppendLine("    AND  COALESCE(CALCUNIT, '')             = @CALCUNIT ")
        SQLStr.AppendLine("    AND  COALESCE(DEPARTURE, '')            = @DEPARTURE ")
        SQLStr.AppendLine("    AND  COALESCE(MILEAGE, '0')             = @MILEAGE ")
        SQLStr.AppendLine("    AND  COALESCE(SHIPPINGCOUNT, '0')       = @SHIPPINGCOUNT ")
        SQLStr.AppendLine("    AND  COALESCE(NENPI, '0')               = @NENPI ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICECURRENT, '0')  = @DIESELPRICECURRENT ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESTANDARD, '0') = @DIESELPRICESTANDARD ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELCONSUMPTION, '0')   = @DIESELCONSUMPTION ")
        SQLStr.AppendLine("    AND  COALESCE(DISPLAYFLG, '')           = @DISPLAYFLG ")
        SQLStr.AppendLine("    AND  COALESCE(ASSESSMENTFLG, '')        = @ASSESSMENTFLG ")
        SQLStr.AppendLine("    AND  COALESCE(ATENACOMPANYNAME, '')     = @ATENACOMPANYNAME ")
        SQLStr.AppendLine("    AND  COALESCE(ATENACOMPANYDEVNAME, '')  = @ATENACOMPANYDEVNAME ")
        SQLStr.AppendLine("    AND  COALESCE(FROMORGNAME, '')          = @FROMORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(MEISAICATEGORYID, '')     = @MEISAICATEGORYID ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTCODE, '0')         = @ACCOUNTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTNAME, '')          = @ACCOUNTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTCODE, '0')         = @SEGMENTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTNAME, '')          = @SEGMENTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(JOTPERCENTAGE, '')        = @JOTPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(ENEXPERCENTAGE, '')       = @ENEXPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU1, '')               = @BIKOU1 ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU2, '')               = @BIKOU2 ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU3, '')               = @BIKOU3 ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')               = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)               '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)              '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)              '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)                 '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)                '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)       '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)      '加算先部門名称
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)         '大分類コード
                Dim P_BIGCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATENAME", MySqlDbType.VarChar, 100)       '大分類名
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)         '中分類コード
                Dim P_MIDCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATENAME", MySqlDbType.VarChar, 100)       '中分類名
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2)     '小分類コード
                Dim P_SMALLCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATENAME", MySqlDbType.VarChar, 100)   '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                'Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                'Dim P_GROUPSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@GROUPSORTNO", MySqlDbType.Decimal, 2)     'グループソート順
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_GROUPNAME As MySqlParameter = SQLcmd.Parameters.Add("@GROUPNAME", MySqlDbType.VarChar, 100)     'グループ名
                'Dim P_DETAILSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@DETAILSORTNO", MySqlDbType.Decimal, 2)     '明細ソート順
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
                'Dim P_DETAILNAME As MySqlParameter = SQLcmd.Parameters.Add("@DETAILNAME", MySqlDbType.VarChar, 100)     '明細名
#End Region
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 10, 2)                 '単価
                Dim P_QUANTITY As MySqlParameter = SQLcmd.Parameters.Add("@QUANTITY", MySqlDbType.Decimal, 10, 2)           '数量
                Dim P_CALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@CALCUNIT", MySqlDbType.VarChar, 20)              '計算単位
                Dim P_DEPARTURE As MySqlParameter = SQLcmd.Parameters.Add("@DEPARTURE", MySqlDbType.VarChar, 50)            '出荷地
                Dim P_MILEAGE As MySqlParameter = SQLcmd.Parameters.Add("@MILEAGE", MySqlDbType.Decimal, 10, 2)             '走行距離
                Dim P_SHIPPINGCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@SHIPPINGCOUNT", MySqlDbType.Decimal, 3)     '輸送回数
                Dim P_NENPI As MySqlParameter = SQLcmd.Parameters.Add("@NENPI", MySqlDbType.Decimal, 5, 2)                  '燃費
                Dim P_DIESELPRICECURRENT As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICECURRENT", MySqlDbType.Decimal, 5, 2)     '実勢軽油価格
                Dim P_DIESELPRICESTANDARD As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESTANDARD", MySqlDbType.Decimal, 5, 2)     '基準経由価格
                Dim P_DIESELCONSUMPTION As MySqlParameter = SQLcmd.Parameters.Add("@DIESELCONSUMPTION", MySqlDbType.Decimal, 10, 2)     '燃料使用量
                Dim P_DISPLAYFLG As MySqlParameter = SQLcmd.Parameters.Add("@DISPLAYFLG", MySqlDbType.VarChar, 1)     '表示フラグ
                Dim P_ASSESSMENTFLG As MySqlParameter = SQLcmd.Parameters.Add("@ASSESSMENTFLG", MySqlDbType.VarChar, 1)     '鑑分けフラグ
                Dim P_ATENACOMPANYNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYNAME", MySqlDbType.VarChar, 50)     '宛名会社名
                Dim P_ATENACOMPANYDEVNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYDEVNAME", MySqlDbType.VarChar, 50)     '宛名会社部門名
                Dim P_FROMORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@FROMORGNAME", MySqlDbType.VarChar, 50)     '請求書発行部店名
                Dim P_MEISAICATEGORYID As MySqlParameter = SQLcmd.Parameters.Add("@MEISAICATEGORYID", MySqlDbType.VarChar, 1)     '明細区分
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 100)     '備考1
                Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 100)     '備考2
                Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 100)     '備考3

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")             '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")   '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")   '加算先部門名称
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_BIGCATENAME.Value = WW_ROW("BIGCATENAME")     '大分類名
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_MIDCATENAME.Value = WW_ROW("MIDCATENAME")     '中分類名
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
                P_SMALLCATENAME.Value = WW_ROW("SMALLCATENAME") '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                'P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                'P_GROUPSORTNO.Value = WW_ROW("GROUPSORTNO")           'グループソート順
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_GROUPNAME.Value = WW_ROW("GROUPNAME")           'グループ名
                'P_DETAILSORTNO.Value = WW_ROW("DETAILSORTNO")           '明細ソート順
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
                'P_DETAILNAME.Value = WW_ROW("DETAILNAME")           '明細名
#End Region
                P_TANKA.Value = WW_ROW("TANKA")           '単価
                P_QUANTITY.Value = WW_ROW("QUANTITY")           '数量
                P_CALCUNIT.Value = WW_ROW("CALCUNIT")           '計算単位
                P_DEPARTURE.Value = WW_ROW("DEPARTURE")           '出荷地
                P_MILEAGE.Value = WW_ROW("MILEAGE")           '走行距離
                P_SHIPPINGCOUNT.Value = WW_ROW("SHIPPINGCOUNT")           '輸送回数
                P_NENPI.Value = WW_ROW("NENPI")           '燃費
                P_DIESELPRICECURRENT.Value = WW_ROW("DIESELPRICECURRENT")           '実勢軽油価格
                P_DIESELPRICESTANDARD.Value = WW_ROW("DIESELPRICESTANDARD")           '基準経由価格
                P_DIESELCONSUMPTION.Value = WW_ROW("DIESELCONSUMPTION")           '燃料使用量
                P_DISPLAYFLG.Value = WW_ROW("DISPLAYFLG")           '表示フラグ
                P_ASSESSMENTFLG.Value = WW_ROW("ASSESSMENTFLG")           '鑑分けフラグ
                P_ATENACOMPANYNAME.Value = WW_ROW("ATENACOMPANYNAME")           '宛名会社名
                P_ATENACOMPANYDEVNAME.Value = WW_ROW("ATENACOMPANYDEVNAME")           '宛名会社部門名
                P_FROMORGNAME.Value = WW_ROW("FROMORGNAME")           '請求書発行部店名
                P_MEISAICATEGORYID.Value = WW_ROW("MEISAICATEGORYID")           '明細区分
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 SELECT"
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
        If WW_ROW("TARGETYM") = "" OrElse
            WW_ROW("TORICODE") = "" OrElse
            WW_ROW("ORGCODE") = "" OrElse
            WW_ROW("BIGCATECODE") = "0" OrElse
            WW_ROW("MIDCATECODE") = "0" OrElse
            WW_ROW("SMALLCATECODE") = "0" Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 SELECT"
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
        SQLStr.Append("     LNG.LNM0014_SPRATE2                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.AppendFormat("     DELFLG               = '{0}' ", C_DELETE_FLG.DELETE)
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                 '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)            '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)        '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)            '更新プログラムＩＤ

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region
                P_UPDYMD.Value = WW_DATENOW                     '更新年月日
                P_UPDUSER.Value = Master.USERID                 '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID           '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name    '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014L UPDATE"
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
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TARGETYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,BIGCATECODE  ")
        SQLStr.AppendLine("     ,BIGCATENAME  ")
        SQLStr.AppendLine("     ,MIDCATECODE  ")
        SQLStr.AppendLine("     ,MIDCATENAME  ")
        SQLStr.AppendLine("     ,SMALLCATECODE  ")
        SQLStr.AppendLine("     ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE  ")
        'SQLStr.AppendLine("     ,TODOKENAME  ")
        'SQLStr.AppendLine("     ,GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,GROUPID  ")
        'SQLStr.AppendLine("     ,GROUPNAME  ")
        'SQLStr.AppendLine("     ,DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,DETAILID  ")
        'SQLStr.AppendLine("     ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,QUANTITY  ")
        SQLStr.AppendLine("     ,CALCUNIT  ")
        SQLStr.AppendLine("     ,DEPARTURE  ")
        SQLStr.AppendLine("     ,MILEAGE  ")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,NENPI  ")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,DISPLAYFLG  ")
        SQLStr.AppendLine("     ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,FROMORGNAME  ")
        SQLStr.AppendLine("     ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("     ,UPDTIMSTP ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @TARGETYM  ")
        SQLStr.AppendLine("     ,@TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@ORGCODE  ")
        SQLStr.AppendLine("     ,@ORGNAME  ")
        SQLStr.AppendLine("     ,@KASANORGCODE  ")
        SQLStr.AppendLine("     ,@KASANORGNAME  ")
        SQLStr.AppendLine("     ,@BIGCATECODE  ")
        SQLStr.AppendLine("     ,@BIGCATENAME  ")
        SQLStr.AppendLine("     ,@MIDCATECODE  ")
        SQLStr.AppendLine("     ,@MIDCATENAME  ")
        SQLStr.AppendLine("     ,@SMALLCATECODE  ")
        SQLStr.AppendLine("     ,@SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,@TODOKECODE  ")
        'SQLStr.AppendLine("     ,@TODOKENAME  ")
        'SQLStr.AppendLine("     ,@GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,@GROUPID  ")
        'SQLStr.AppendLine("     ,@GROUPNAME  ")
        'SQLStr.AppendLine("     ,@DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,@DETAILID  ")
        'SQLStr.AppendLine("     ,@DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,@TANKA  ")
        SQLStr.AppendLine("     ,@QUANTITY  ")
        SQLStr.AppendLine("     ,@CALCUNIT  ")
        SQLStr.AppendLine("     ,@DEPARTURE  ")
        SQLStr.AppendLine("     ,@MILEAGE  ")
        SQLStr.AppendLine("     ,@SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,@NENPI  ")
        SQLStr.AppendLine("     ,@DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,@DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,@DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,@DISPLAYFLG  ")
        SQLStr.AppendLine("     ,@ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,@ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,@ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,@FROMORGNAME  ")
        SQLStr.AppendLine("     ,@MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("     ,@UPDTIMSTP ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      TARGETYM =  @TARGETYM")
        SQLStr.AppendLine("     ,TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGCODE =  @ORGCODE")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,BIGCATECODE =  @BIGCATECODE")
        SQLStr.AppendLine("     ,BIGCATENAME =  @BIGCATENAME")
        SQLStr.AppendLine("     ,MIDCATECODE =  @MIDCATECODE")
        SQLStr.AppendLine("     ,MIDCATENAME =  @MIDCATENAME")
        SQLStr.AppendLine("     ,SMALLCATECODE =  @SMALLCATECODE")
        SQLStr.AppendLine("     ,SMALLCATENAME =  @SMALLCATENAME")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
        'SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
        'SQLStr.AppendLine("     ,GROUPSORTNO =  @GROUPSORTNO")
        'SQLStr.AppendLine("     ,GROUPID =  @GROUPID")
        'SQLStr.AppendLine("     ,GROUPNAME =  @GROUPNAME")
        'SQLStr.AppendLine("     ,DETAILSORTNO =  @DETAILSORTNO")
        'SQLStr.AppendLine("     ,DETAILID =  @DETAILID")
        'SQLStr.AppendLine("     ,DETAILNAME =  @DETAILNAME")
#End Region
        SQLStr.AppendLine("     ,TANKA =  @TANKA")
        SQLStr.AppendLine("     ,QUANTITY =  @QUANTITY")
        SQLStr.AppendLine("     ,CALCUNIT =  @CALCUNIT")
        SQLStr.AppendLine("     ,DEPARTURE =  @DEPARTURE")
        SQLStr.AppendLine("     ,MILEAGE =  @MILEAGE")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT =  @SHIPPINGCOUNT")
        SQLStr.AppendLine("     ,NENPI =  @NENPI")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT =  @DIESELPRICECURRENT")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD =  @DIESELPRICESTANDARD")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION =  @DIESELCONSUMPTION")
        SQLStr.AppendLine("     ,DISPLAYFLG =  @DISPLAYFLG")
        SQLStr.AppendLine("     ,ASSESSMENTFLG =  @ASSESSMENTFLG")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME =  @ATENACOMPANYNAME")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME =  @ATENACOMPANYDEVNAME")
        SQLStr.AppendLine("     ,FROMORGNAME =  @FROMORGNAME")
        SQLStr.AppendLine("     ,MEISAICATEGORYID =  @MEISAICATEGORYID")
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
        SQLStr.AppendLine("     ,UPDTIMSTP = @UPDTIMSTP ")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                   '削除フラグ
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)               '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)              '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)              '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)                 '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)                '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)       '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)      '加算先部門名称
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)         '大分類コード
                Dim P_BIGCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATENAME", MySqlDbType.VarChar, 100)       '大分類名
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)         '中分類コード
                Dim P_MIDCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATENAME", MySqlDbType.VarChar, 100)       '中分類名
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2)     '小分類コード
                Dim P_SMALLCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATENAME", MySqlDbType.VarChar, 100)   '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                'Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                'Dim P_GROUPSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@GROUPSORTNO", MySqlDbType.Decimal, 2)     'グループソート順
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_GROUPNAME As MySqlParameter = SQLcmd.Parameters.Add("@GROUPNAME", MySqlDbType.VarChar, 100)     'グループ名
                'Dim P_DETAILSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@DETAILSORTNO", MySqlDbType.Decimal, 2)     '明細ソート順
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
                'Dim P_DETAILNAME As MySqlParameter = SQLcmd.Parameters.Add("@DETAILNAME", MySqlDbType.VarChar, 100)     '明細名
#End Region
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 10, 2)                 '単価
                Dim P_QUANTITY As MySqlParameter = SQLcmd.Parameters.Add("@QUANTITY", MySqlDbType.Decimal, 10, 2)           '数量
                Dim P_CALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@CALCUNIT", MySqlDbType.VarChar, 20)              '計算単位
                Dim P_DEPARTURE As MySqlParameter = SQLcmd.Parameters.Add("@DEPARTURE", MySqlDbType.VarChar, 50)            '出荷地
                Dim P_MILEAGE As MySqlParameter = SQLcmd.Parameters.Add("@MILEAGE", MySqlDbType.Decimal, 10, 2)             '走行距離
                Dim P_SHIPPINGCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@SHIPPINGCOUNT", MySqlDbType.Decimal, 3)     '輸送回数
                Dim P_NENPI As MySqlParameter = SQLcmd.Parameters.Add("@NENPI", MySqlDbType.Decimal, 5, 2)     '燃費
                Dim P_DIESELPRICECURRENT As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICECURRENT", MySqlDbType.Decimal, 5, 2)     '実勢軽油価格
                Dim P_DIESELPRICESTANDARD As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESTANDARD", MySqlDbType.Decimal, 5, 2)     '基準経由価格
                Dim P_DIESELCONSUMPTION As MySqlParameter = SQLcmd.Parameters.Add("@DIESELCONSUMPTION", MySqlDbType.Decimal, 10, 2)     '燃料使用量
                Dim P_DISPLAYFLG As MySqlParameter = SQLcmd.Parameters.Add("@DISPLAYFLG", MySqlDbType.VarChar, 1)     '表示フラグ
                Dim P_ASSESSMENTFLG As MySqlParameter = SQLcmd.Parameters.Add("@ASSESSMENTFLG", MySqlDbType.VarChar, 1)     '鑑分けフラグ
                Dim P_ATENACOMPANYNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYNAME", MySqlDbType.VarChar, 50)     '宛名会社名
                Dim P_ATENACOMPANYDEVNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYDEVNAME", MySqlDbType.VarChar, 50)     '宛名会社部門名
                Dim P_FROMORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@FROMORGNAME", MySqlDbType.VarChar, 50)     '請求書発行部店名
                Dim P_MEISAICATEGORYID As MySqlParameter = SQLcmd.Parameters.Add("@MEISAICATEGORYID", MySqlDbType.VarChar, 1)     '明細区分
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 100)     '備考1
                Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 100)     '備考2
                Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 100)     '備考3
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時
                Dim P_UPDTIMSTP As MySqlParameter = SQLcmd.Parameters.Add("@UPDTIMSTP", MySqlDbType.DateTime)     'タイムスタンプ

                'DB更新
                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")             '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")   '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")   '加算先部門名称
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_BIGCATENAME.Value = WW_ROW("BIGCATENAME")     '大分類名
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_MIDCATENAME.Value = WW_ROW("MIDCATENAME")     '中分類名
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
                P_SMALLCATENAME.Value = WW_ROW("SMALLCATENAME") '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                'P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称

                ''グループソート順が空(新規)の場合グループIDを入れる
                'If WW_ROW("GROUPSORTNO").ToString = "0" Then
                '    P_GROUPSORTNO.Value = WW_ROW("GROUPID")
                'Else
                '    P_GROUPSORTNO.Value = WW_ROW("GROUPSORTNO")
                'End If

                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_GROUPNAME.Value = WW_ROW("GROUPNAME")           'グループ名

                ''明細ソート順が空(新規)の場合明細IDを入れる
                'If WW_ROW("DETAILSORTNO").ToString = "0" Then
                '    P_DETAILSORTNO.Value = WW_ROW("DETAILID")
                'Else
                '    P_DETAILSORTNO.Value = WW_ROW("DETAILSORTNO")
                'End If

                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
                'P_DETAILNAME.Value = WW_ROW("DETAILNAME")           '明細名
#End Region

                '単価
                If WW_ROW("TANKA").ToString = "0" Or WW_ROW("TANKA").ToString = "" Then
                    P_TANKA.Value = DBNull.Value
                Else
                    P_TANKA.Value = WW_ROW("TANKA")
                End If

                '数量
                If WW_ROW("QUANTITY").ToString = "0" Or WW_ROW("QUANTITY").ToString = "" Then
                    P_QUANTITY.Value = 0.00
                    'P_QUANTITY.Value = DBNull.Value
                Else
                    P_QUANTITY.Value = WW_ROW("QUANTITY")
                End If

                P_CALCUNIT.Value = WW_ROW("CALCUNIT")           '計算単位
                P_DEPARTURE.Value = WW_ROW("DEPARTURE")           '出荷地

                '走行距離
                If WW_ROW("MILEAGE").ToString = "0" Or WW_ROW("MILEAGE").ToString = "" Then
                    P_MILEAGE.Value = DBNull.Value
                Else
                    P_MILEAGE.Value = WW_ROW("MILEAGE")
                End If

                '輸送回数
                If WW_ROW("SHIPPINGCOUNT").ToString = "0" Or WW_ROW("SHIPPINGCOUNT").ToString = "" Then
                    P_SHIPPINGCOUNT.Value = DBNull.Value
                Else
                    P_SHIPPINGCOUNT.Value = WW_ROW("SHIPPINGCOUNT")
                End If

                '燃費
                If WW_ROW("NENPI").ToString = "0" Or WW_ROW("NENPI").ToString = "" Then
                    P_NENPI.Value = DBNull.Value
                Else
                    P_NENPI.Value = WW_ROW("NENPI")
                End If

                '実勢軽油価格
                If WW_ROW("DIESELPRICECURRENT").ToString = "0" Or WW_ROW("DIESELPRICECURRENT").ToString = "" Then
                    P_DIESELPRICECURRENT.Value = DBNull.Value
                Else
                    P_DIESELPRICECURRENT.Value = WW_ROW("DIESELPRICECURRENT")
                End If

                '基準経由価格
                If WW_ROW("DIESELPRICESTANDARD").ToString = "0" Or WW_ROW("DIESELPRICESTANDARD").ToString = "" Then
                    P_DIESELPRICESTANDARD.Value = DBNull.Value
                Else
                    P_DIESELPRICESTANDARD.Value = WW_ROW("DIESELPRICESTANDARD")
                End If

                '燃料使用量
                If WW_ROW("DIESELCONSUMPTION").ToString = "0" Or WW_ROW("DIESELCONSUMPTION").ToString = "" Then
                    P_DIESELCONSUMPTION.Value = DBNull.Value
                Else
                    P_DIESELCONSUMPTION.Value = WW_ROW("DIESELCONSUMPTION")
                End If

                P_DISPLAYFLG.Value = WW_ROW("DISPLAYFLG")           '表示フラグ
                P_ASSESSMENTFLG.Value = WW_ROW("ASSESSMENTFLG")           '鑑分けフラグ

                '宛名会社名
                '鑑分けフラグ1かつ宛名会社名が未設定の場合
                If WW_ROW("ASSESSMENTFLG").ToString = "1" And WW_ROW("ATENACOMPANYNAME").ToString = "" Then
                    P_ATENACOMPANYNAME.Value = WW_ROW("TORINAME")
                Else
                    P_ATENACOMPANYNAME.Value = WW_ROW("ATENACOMPANYNAME")
                End If

                '宛名会社部門名
                '鑑分けフラグ1かつ宛名会社部門名が未設定の場合
                If WW_ROW("ASSESSMENTFLG").ToString = "1" And WW_ROW("ATENACOMPANYDEVNAME").ToString = "" Then
                    P_ATENACOMPANYDEVNAME.Value = ""
                Else
                    P_ATENACOMPANYDEVNAME.Value = WW_ROW("ATENACOMPANYDEVNAME")
                End If

                '請求書発行部店名
                '鑑分けフラグ1かつ本項目が請求書発行部店名の場合
                If WW_ROW("ASSESSMENTFLG").ToString = "1" And WW_ROW("FROMORGNAME").ToString = "" Then
                    P_FROMORGNAME.Value = LNM0014WRKINC.DEFAULT_FROMORGNAME
                Else
                    P_FROMORGNAME.Value = WW_ROW("FROMORGNAME")           '請求書発行部店名
                End If

                P_MEISAICATEGORYID.Value = WW_ROW("MEISAICATEGORYID")           '明細区分

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
                P_UPDTIMSTP.Value = WW_DATENOW

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0014_SPRATE2  INSERTUPDATE"
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

        '対象年月(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TARGETYM", WW_ROW("TARGETYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・対象年月エラーです。"
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
        '大分類名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIGCATENAME", WW_ROW("BIGCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・大分類名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '中分類名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MIDCATENAME", WW_ROW("MIDCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・中分類名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '小分類名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SMALLCATENAME", WW_ROW("SMALLCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・小分類名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
#Region "コメント-2025/07/30(分類追加対応のため)"
        ''届先コード(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "TODOKECODE", WW_ROW("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・届先コードエラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ''届先名称(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "TODOKENAME", WW_ROW("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・届先名称エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        '''グループソート順(バリデーションチェック)
        ''Master.CheckField(Master.USERCAMP, "GROUPSORTNO", WW_ROW("GROUPSORTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        ''If Not isNormal(WW_CS0024FCheckerr) Then
        ''    WW_CheckMES1 = "・グループソート順エラーです。"
        ''    WW_CheckMES2 = WW_CS0024FCheckReport
        ''    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        ''    WW_LineErr = "ERR"
        ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        ''End If
        '''グループID(バリデーションチェック)
        ''Master.CheckField(Master.USERCAMP, "GROUPID", WW_ROW("GROUPID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        ''If Not isNormal(WW_CS0024FCheckerr) Then
        ''    WW_CheckMES1 = "・グループIDエラーです。"
        ''    WW_CheckMES2 = WW_CS0024FCheckReport
        ''    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        ''    WW_LineErr = "ERR"
        ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        ''End If
        ''グループ名(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "GROUPNAME", WW_ROW("GROUPNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・グループ名エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        '''明細ソート順(バリデーションチェック)
        ''Master.CheckField(Master.USERCAMP, "DETAILSORTNO", WW_ROW("DETAILSORTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        ''If Not isNormal(WW_CS0024FCheckerr) Then
        ''    WW_CheckMES1 = "・明細ソート順エラーです。"
        ''    WW_CheckMES2 = WW_CS0024FCheckReport
        ''    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        ''    WW_LineErr = "ERR"
        ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        ''End If
        '''明細ID(バリデーションチェック)
        ''Master.CheckField(Master.USERCAMP, "DETAILID", WW_ROW("DETAILID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        ''If Not isNormal(WW_CS0024FCheckerr) Then
        ''    WW_CheckMES1 = "・明細IDエラーです。"
        ''    WW_CheckMES2 = WW_CS0024FCheckReport
        ''    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        ''    WW_LineErr = "ERR"
        ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        ''End If
        ''明細名(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "DETAILNAME", WW_ROW("DETAILNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・明細名エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
#End Region
        '単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TANKA", WW_ROW("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '数量(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "QUANTITY", WW_ROW("QUANTITY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・数量エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '計算単位(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CALCUNIT", WW_ROW("CALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・計算単位エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '出荷地(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPARTURE", WW_ROW("DEPARTURE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・出荷地エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '走行距離(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MILEAGE", WW_ROW("MILEAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・走行距離エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '輸送回数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHIPPINGCOUNT", WW_ROW("SHIPPINGCOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・輸送回数エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '燃費(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "NENPI", WW_ROW("NENPI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・燃費エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '実勢軽油価格(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICECURRENT", WW_ROW("DIESELPRICECURRENT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢軽油価格エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '基準経由価格(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESTANDARD", WW_ROW("DIESELPRICESTANDARD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・基準経由価格エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '燃料使用量(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELCONSUMPTION", WW_ROW("DIESELCONSUMPTION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・燃料使用量エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '表示フラグ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DISPLAYFLG", WW_ROW("DISPLAYFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・表示フラグエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '鑑分けフラグ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ASSESSMENTFLG", WW_ROW("ASSESSMENTFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・鑑分けフラグエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '宛名会社名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ATENACOMPANYNAME", WW_ROW("ATENACOMPANYNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・宛名会社名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '宛名会社部門名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ATENACOMPANYDEVNAME", WW_ROW("ATENACOMPANYDEVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・宛名会社部門名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '請求書発行部店名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FROMORGNAME", WW_ROW("FROMORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求書発行部店名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '明細区分(バリデーションチェック)
        If WW_ROW("MEISAICATEGORYID") = "0" Then
            Master.CheckField(Master.USERCAMP, "MEISAICATEGORYID", "", WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        Else
            Master.CheckField(Master.USERCAMP, "MEISAICATEGORYID", WW_ROW("MEISAICATEGORYID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        End If
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・明細区分エラーです。"
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

        '特別料金マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')     = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')     = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')      = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0') = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0') = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region

                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region

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
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0015_SPRATEHIST2 ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TARGETYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,BIGCATECODE  ")
        SQLStr.AppendLine("     ,BIGCATENAME  ")
        SQLStr.AppendLine("     ,MIDCATECODE  ")
        SQLStr.AppendLine("     ,MIDCATENAME  ")
        SQLStr.AppendLine("     ,SMALLCATECODE  ")
        SQLStr.AppendLine("     ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE  ")
        'SQLStr.AppendLine("     ,TODOKENAME  ")
        'SQLStr.AppendLine("     ,GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,GROUPID  ")
        'SQLStr.AppendLine("     ,GROUPNAME  ")
        'SQLStr.AppendLine("     ,DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,DETAILID  ")
        'SQLStr.AppendLine("     ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,QUANTITY  ")
        SQLStr.AppendLine("     ,CALCUNIT  ")
        SQLStr.AppendLine("     ,DEPARTURE  ")
        SQLStr.AppendLine("     ,MILEAGE  ")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,NENPI  ")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,DISPLAYFLG  ")
        SQLStr.AppendLine("     ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,FROMORGNAME  ")
        SQLStr.AppendLine("     ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("      TARGETYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,BIGCATECODE  ")
        SQLStr.AppendLine("     ,BIGCATENAME  ")
        SQLStr.AppendLine("     ,MIDCATECODE  ")
        SQLStr.AppendLine("     ,MIDCATENAME  ")
        SQLStr.AppendLine("     ,SMALLCATECODE  ")
        SQLStr.AppendLine("     ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE  ")
        'SQLStr.AppendLine("     ,TODOKENAME  ")
        'SQLStr.AppendLine("     ,GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,GROUPID  ")
        'SQLStr.AppendLine("     ,GROUPNAME  ")
        'SQLStr.AppendLine("     ,DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,DETAILID  ")
        'SQLStr.AppendLine("     ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,QUANTITY  ")
        SQLStr.AppendLine("     ,CALCUNIT  ")
        SQLStr.AppendLine("     ,DEPARTURE  ")
        SQLStr.AppendLine("     ,MILEAGE  ")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,NENPI  ")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,DISPLAYFLG  ")
        SQLStr.AppendLine("     ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,FROMORGNAME  ")
        SQLStr.AppendLine("     ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)           '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)      '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)               '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)          '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)      '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)          '登録プログラムＩＤ

                ' DB更新
                P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = WW_ROW("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = WW_ROW("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_GROUPID.Value = WW_ROW("GROUPID")           'グループID
                'P_DETAILID.Value = WW_ROW("DETAILID")           '明細ID
#End Region

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = C_DELETE_FLG.DELETE Then
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.UPDDATA).ToString
                    End If
                End If

                P_MODIFYKBN.Value = WW_MODIFYKBN                '変更区分
                P_MODIFYYMD.Value = WW_NOW                      '変更日時
                P_MODIFYUSER.Value = Master.USERID              '変更ユーザーＩＤ

                P_INITYMD.Value = WW_NOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID          '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name   '登録プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015_SPRATEHIST2  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0015_SPRATEHIST2  INSERT"
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


