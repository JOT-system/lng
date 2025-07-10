''************************************************************
' 休日割増単価マスタメンテナンス・一覧画面
' 作成日 2025/07/01
' 更新日 
' 作成者 三宅
' 更新者 
'
' 修正履歴 : 2025/07/01 新規作成
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
Public Class LNM0017HolidayRateList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0017tbl As DataTable         '一覧格納用テーブル
    Private LNM0017UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0017Exceltbl As New DataTable  'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0017tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            InputSave()
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            InputSave()
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0017WRKINC.FILETYPE.EXCEL)
                        'Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                        '    WF_EXCELPDF(LNM0017WRKINC.FILETYPE.PDF)
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
                        Case "WF_ButtonExtract" '検索ボタン押下時
                            GridViewInitialize()
                        Case "WF_ButtonPAGE", "WF_ButtonFIRST", "WF_ButtonPREVIOUS", "WF_ButtonNEXT", "WF_ButtonLAST"
                            Me.WF_ButtonPAGE_Click()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" And
                        Not WF_ButtonClick.Value = "WF_ButtonExtract" And
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
            If Not IsNothing(LNM0017tbl) Then
                LNM0017tbl.Clear()
                LNM0017tbl.Dispose()
                LNM0017tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0017WRKINC.MAPIDL
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
            Case C_PREV_MAP_LIST.LNM0017D, C_PREV_MAP_LIST.LNM0017H
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
        retToriList = LNM0017WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0017D Or
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0017H Then
            ' 登録画面からの遷移
            Master.RecoverTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)
            '荷主
            WF_TORI.SelectedValue = work.WF_SEL_TORI_L.Text
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

        End If

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0017WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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
        Master.SaveTable(LNM0017tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0017tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = "1"

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0017tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0017tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0017tbl.Rows.Count) / CONST_DISPROWCOUNT)
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0017tbl)
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
        Master.SaveTable(LNM0017tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0017tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = work.WF_SEL_NOWPAGECNT_L.Text

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0017tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0017tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0017tbl.Rows.Count) / CONST_DISPROWCOUNT)
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
        For Each LNM0017row As DataRow In LNM0017tbl.Rows
            If LNM0017row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0017row("SELECT") = WW_DataCNT
            End If
        Next

        Dim TBLview As DataView = New DataView(LNM0017tbl)
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

        If IsNothing(LNM0017tbl) Then
            LNM0017tbl = New DataTable
        End If

        If LNM0017tbl.Columns.Count <> 0 Then
            LNM0017tbl.Columns.Clear()
        End If

        LNM0017tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを固定費マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNM17.UPDTIMSTP                                                          AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , LNM17.ID                                                                 AS ID                  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.TORICODE), '')                                      AS TORICODE            ")
        SQLStr.AppendLine("   , (SELECT L06.TORINAME                                                                            ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.TORICODE = LNM17.TORICODE                                                            ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.TORINAME                                                                          ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS TORINAME                                                                                    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.ORDERORGCODE), '')                                  AS ORDERORGCODE        ")
        SQLStr.AppendLine("   , (SELECT L06.ORGNAME                                                                             ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.ORGCODE = LNM17.ORDERORGCODE                                                         ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.ORGNAME                                                                           ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS ORDERORGNAME                                                                                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.ORDERORGCATEGORY), '')                              AS ORDERORGCATEGORY    ")
        SQLStr.AppendLine("   , (CASE LNM17.ORDERORGCATEGORY                                                                    ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS ORDERORGCATEGORYNAME                                                                        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.SHUKABASHO), '')                                    AS SHUKABASHO          ")
        SQLStr.AppendLine("   , (SELECT L06.AVOCADOSHUKANAME                                                                    ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.AVOCADOSHUKABASHO = LNM17.SHUKABASHO                                                 ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.AVOCADOSHUKANAME                                                                  ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS SHUKABASHONAME                                                                              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.SHUKABASHOCATEGORY), '')                            AS SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("   , (CASE LNM17.SHUKABASHOCATEGORY                                                                  ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS SHUKABASHOCATEGORYNAME                                                                      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.TODOKECODE), '')                                    AS TODOKECODE          ")
        SQLStr.AppendLine("   , (SELECT L06.AVOCADOTODOKENAME                                                                   ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.AVOCADOTODOKECODE = LNM17.TODOKECODE                                                 ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.AVOCADOTODOKENAME                                                                 ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS TODOKENAME                                                                                  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.TODOKECATEGORY), '')                                AS TODOKECATEGORY      ")
        SQLStr.AppendLine("   , (CASE LNM17.TODOKECATEGORY                                                                      ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS TODOKECATEGORYNAME                                                                          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.RANGECODE), '')                                     AS RANGECODE           ")
        SQLStr.AppendLine("   , (SELECT GROUP_CONCAT(LNS06.VALUE1 ORDER BY LOCATE(LNS06.KEYCODE, LNM17.RANGECODE) SEPARATOR '／') ")
        SQLStr.AppendLine("        FROM COM.LNS0006_FIXVALUE LNS06                                                              ")
        SQLStr.AppendLine("       WHERE LNS06.CAMPCODE = '01'                                                                   ")
        SQLStr.AppendLine("         AND LNS06.CLASS    = 'HOLIDAYRANGE'                                                         ")
        SQLStr.AppendLine("         AND LOCATE(LNS06.KEYCODE, LNM17.RANGECODE) > 0                                              ")
        SQLStr.AppendLine("         AND CURDATE() BETWEEN LNS06.STYMD AND LNS06.ENDYMD                                          ")
        SQLStr.AppendLine("         AND LNS06.DELFLG <> '1'                                                                     ")
        SQLStr.AppendLine("       ORDER BY LNS06.KEYCODE                                                                        ")
        SQLStr.AppendLine("    ) AS RANGENAME                                                                                   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.GYOMUTANKNUMFROM), '')                              AS GYOMUTANKNUMFROM    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.GYOMUTANKNUMTO), '')                                AS GYOMUTANKNUMTO      ")
        SQLStr.AppendLine("   , LNM17.TANKA                                                              AS TANKA               ")
        SQLStr.AppendLine("   , FORMAT(LNM17.TANKA,0)                                                    AS SRCTANKA            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM17.DELFLG), '')                                        AS DELFLG              ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0017_HOLIDAYRATE LNM17                                                                   ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '取引先コード
        If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
            SQLStr.AppendLine(" AND  LNM17.TORICODE = @TORICODE                                          ")
        End If

        '削除フラグ
        If Not ChkDelDataFlg.Checked Then
            SQLStr.AppendLine(" AND  LNM17.DELFLG = '0'                                                  ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                     ")
        SQLStr.AppendLine("     LNM17.TORICODE                                                           ")
        SQLStr.AppendLine("    ,LNM17.ORDERORGCODE                                                       ")
        SQLStr.AppendLine("    ,LNM17.SHUKABASHO                                                         ")
        SQLStr.AppendLine("    ,LNM17.TODOKECODE                                                         ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                '取引先コード
                If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                    P_TORICODE.Value = WF_TORI.SelectedValue
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0017tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0017tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0017row As DataRow In LNM0017tbl.Rows
                    i += 1
                    LNM0017row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017 SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017 Select"
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

        work.WF_SEL_ID.Text = ""                                                  'ユニークID
        work.WF_SEL_TORICODE.Text = ""                                            '取引先コード
        work.WF_SEL_TORINAME.Text = ""                                            '取引先名称
        work.WF_SEL_ORDERORGCODE.Text = ""                                        '受注受付部署コード
        work.WF_SEL_ORDERORGNAME.Text = ""                                        '受注受付部署名称
        work.WF_SEL_ORDERORGCATEGORY.Text = ""                                    '受注受付部署判定区分
        work.WF_SEL_ORDERORGCATEGORYNAME.Text = ""                                '受注受付部署判定区分名称
        work.WF_SEL_SHUKABASHO.Text = ""                                          '出荷場所コード
        work.WF_SEL_SHUKABASHONAME.Text = ""                                      '出荷場所名称
        work.WF_SEL_SHUKABASHOCATEGORY.Text = ""                                  '出荷場所判定区分
        work.WF_SEL_SHUKABASHOCATEGORYNAME.Text = ""                              '出荷場所判定区分名称
        work.WF_SEL_TODOKECODE.Text = ""                                          '届先コード
        work.WF_SEL_TODOKENAME.Text = ""                                          '届先名称
        work.WF_SEL_TODOKECATEGORY.Text = ""                                      '届先判定区分
        work.WF_SEL_TODOKECATEGORYNAME.Text = ""                                  '届先判定区分名称
        work.WF_SEL_RANGECODE.Text = ""                                           '休日範囲コード
        work.WF_SEL_GYOMUTANKNUMFROM.Text = ""                                    '車番（開始）
        work.WF_SEL_GYOMUTANKNUMTO.Text = ""                                      '車番（終了）
        work.WF_SEL_TANKA.Text = ""                                               '単価

        work.WF_SEL_TIMESTAMP.Text = ""         　                                'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                               '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0017tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0017HolidayRateHistory.aspx")
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNM0017tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNM0017tbl)

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
        Dim TBLview As New DataView(LNM0017tbl)
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
        For Each LNM0014row As DataRow In LNM0017tbl.Rows
            If LNM0014row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0014row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNM0017tbl)

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
        work.WF_SEL_TORI_L.Text = WF_TORI.SelectedValue '荷主
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
        If LNM0017tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE Then
            Dim WW_ROW As DataRow
            WW_ROW = LNM0017tbl.Rows(WW_LineCNT)
            Dim DATENOW As Date = Date.Now
            Dim WW_UPDTIMSTP As Date

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                '履歴登録(変更前)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0017WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '削除フラグ有効化
                DelflgValid(SQLcon, WW_ROW, DATENOW, WW_UPDTIMSTP)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '履歴登録(変更後)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0017WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                LNM0017tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.ALIVE
                LNM0017tbl.Rows(WW_LineCNT)("UPDTIMSTP") = WW_UPDTIMSTP
                Master.SaveTable(LNM0017tbl)
                Master.Output(C_MESSAGE_NO.DELETE_ROW_ACTIVATION, C_MESSAGE_TYPE.NOR, needsPopUp:=True)
            End Using
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = LNM0017tbl.Rows(WW_LineCNT)("LINECNT")                               '選択行

        work.WF_SEL_ID.Text = LNM0017tbl.Rows(WW_LineCNT)("ID")                                         'ユニークID
        work.WF_SEL_TORICODE.Text = LNM0017tbl.Rows(WW_LineCNT)("TORICODE")                             '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0017tbl.Rows(WW_LineCNT)("TORINAME")                             '取引先名称
        work.WF_SEL_ORDERORGCODE.Text = LNM0017tbl.Rows(WW_LineCNT)("ORDERORGCODE")                     '受注受付部署コード
        work.WF_SEL_ORDERORGNAME.Text = LNM0017tbl.Rows(WW_LineCNT)("ORDERORGNAME")                     '受注受付部署名称
        work.WF_SEL_ORDERORGCATEGORY.Text = LNM0017tbl.Rows(WW_LineCNT)("ORDERORGCATEGORY")             '受注受付部署判定区分
        work.WF_SEL_ORDERORGCATEGORYNAME.Text = LNM0017tbl.Rows(WW_LineCNT)("ORDERORGCATEGORYNAME")     '受注受付部署判定区分名称
        work.WF_SEL_SHUKABASHO.Text = LNM0017tbl.Rows(WW_LineCNT)("SHUKABASHO")                         '出荷場所コード
        work.WF_SEL_SHUKABASHONAME.Text = LNM0017tbl.Rows(WW_LineCNT)("SHUKABASHONAME")                 '出荷場所名称
        work.WF_SEL_SHUKABASHOCATEGORY.Text = LNM0017tbl.Rows(WW_LineCNT)("SHUKABASHOCATEGORY")         '出荷場所判定区分
        work.WF_SEL_SHUKABASHOCATEGORYNAME.Text = LNM0017tbl.Rows(WW_LineCNT)("SHUKABASHOCATEGORYNAME") '出荷場所判定区分名称
        work.WF_SEL_TODOKECODE.Text = LNM0017tbl.Rows(WW_LineCNT)("TODOKECODE")                         '届先コード
        work.WF_SEL_TODOKENAME.Text = LNM0017tbl.Rows(WW_LineCNT)("TODOKENAME")                         '届先名称
        work.WF_SEL_TODOKECATEGORY.Text = LNM0017tbl.Rows(WW_LineCNT)("TODOKECATEGORY")                 '届先判定区分
        work.WF_SEL_TODOKECATEGORYNAME.Text = LNM0017tbl.Rows(WW_LineCNT)("TODOKECATEGORYNAME")         '届先判定区分名称
        work.WF_SEL_RANGECODE.Text = LNM0017tbl.Rows(WW_LineCNT)("RANGECODE")                           '休日範囲コード
        work.WF_SEL_GYOMUTANKNUMFROM.Text = LNM0017tbl.Rows(WW_LineCNT)("GYOMUTANKNUMFROM")             '車番（開始）
        work.WF_SEL_GYOMUTANKNUMTO.Text = LNM0017tbl.Rows(WW_LineCNT)("GYOMUTANKNUMTO")                 '車番（終了）
        work.WF_SEL_TANKA.Text = LNM0017tbl.Rows(WW_LineCNT)("TANKA")                                      '単価

        work.WF_SEL_DELFLG.Text = LNM0017tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0017tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text, work.WF_SEL_ID.Text)
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
        SQLStr.Append("     LNG.LNM0017_HOLIDAYRATE                 ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '0'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         ID               = @ID              ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                  'ID
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)             '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)    '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ

                P_ID.Value = WW_ROW("ID")                       'ID
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE UPDATE"
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
        SQLStrTimStp.Append(" SELECT                                                ")
        SQLStrTimStp.Append("    UPDTIMSTP                                          ")
        SQLStrTimStp.Append(" FROM                                                  ")
        SQLStrTimStp.Append("     LNG.LNM0017_HOLIDAYRATE                           ")
        SQLStrTimStp.Append(" WHERE                                                 ")
        SQLStrTimStp.Append("     ID               = @ID                            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrTimStp.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.VarChar, 10)     'ID

                P_ID.Value = WW_ROW("ID")                   'ID

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE SELECT"
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

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "TORICODE"             '取引先コード
                    work.CODENAMEGetTORI(SQLcon, WW_NAMEht)
                Case "ORDERORGCODE"         '部門コード
                    work.CODENAMEGetORG(SQLcon, WW_NAMEht)
                Case "SHUKABASHO"           '出荷場所コード
                    work.CODENAMEGetSHUKABASHO(SQLcon, WW_NAMEht)
                Case "TODOKECODE"           '届先コード
                    work.CODENAMEGetTODOKE(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "CATEGORY"         '範疇フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "CATEGORY"))
                Case "HOLIDAYRANGE"     '休日範囲
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HOLIDAYRANGE"))
                Case "TORICODE", "ORDERORGCODE", "SHUKABASHO", "TODOKECODE"        '取引先、部門、出荷場所、届先
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

        O_RTN = C_MESSAGE_NO.NORMAL

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
        WW_MAXCOL = [Enum].GetValues(GetType(LNM0017WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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

        wb.ActiveSheet.Range("C1").Value = "休日割増単価マスタ一覧"

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
            Case LNM0017WRKINC.FILETYPE.EXCEL
                FileName = "休日割増単価マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                'Case LNM0017WRKINC.FILETYPE.PDF
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
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.RANGECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '休日範囲コード
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.TANKA).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '単価

        '入力不要列網掛け
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.ID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'ユニークID
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.ORDERORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '受注受付部署名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORYNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '受注受付部署判定区分名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHONAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '出荷場所名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORYNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '出荷場所判定名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.TODOKENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '届先名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORYNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '届先判定区分名
        sheet.Columns(LNM0017WRKINC.INOUTEXCELCOL.RANGENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '休日範囲名

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

        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ID).Value = "ユニークID"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCODE).Value = "受注受付部署コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGNAME).Value = "受注受付部署名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORY).Value = "受注受付部署判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORYNAME).Value = "受注受付部署判定区分名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHO).Value = "出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHONAME).Value = "出荷場所名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORY).Value = "出荷場所判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORYNAME).Value = "出荷場所判定名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECODE).Value = "届先コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKENAME).Value = "届先名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORY).Value = "届先判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORYNAME).Value = "届先判定区分名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.RANGECODE).Value = "（必須）休日範囲コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.RANGENAME).Value = "休日範囲名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMFROM).Value = "車番（開始）"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMTO).Value = "車番（終了）"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TANKA).Value = "（必須）単価"

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
                sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '受注受付部署判定区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:対象")
            WW_TEXTLIST.AppendLine("2:除外")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORY).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORY).Comment.Shape
                .Width = 50
                .Height = 30
            End With

            '出荷場所判定区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:対象")
            WW_TEXTLIST.AppendLine("2:除外")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORY).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORY).Comment.Shape
                .Width = 50
                .Height = 30
            End With

            '届先判定区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:対象")
            WW_TEXTLIST.AppendLine("2:除外")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORY).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORY).Comment.Shape
                .Width = 50
                .Height = 30
            End With

            '休日範囲コード
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:日曜")
            WW_TEXTLIST.AppendLine("2:祝日")
            WW_TEXTLIST.AppendLine("3:元旦")
            WW_TEXTLIST.AppendLine("4:年末年始")
            WW_TEXTLIST.AppendLine("5:メーデー")
            WW_TEXTLIST.AppendLine("")
            WW_TEXTLIST.AppendLine("例）日曜のみ場合、1")
            WW_TEXTLIST.AppendLine("    日曜／祝日の場合、12")
            WW_TEXTLIST.AppendLine("    日曜／元旦／年末年始の場合、134")
            WW_TEXTLIST.AppendLine("    日曜／祝日／元旦／年末年始／メーデーの場合、12345")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.RANGECODE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0017WRKINC.INOUTEXCELCOL.RANGECODE).Comment.Shape
                .Width = 350
                .Height = 150
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
        SETFIXVALUELIST(subsheet, "DELFLG", LNM0017WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0017WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG)
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
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0017WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0017WRKINC.INOUTEXCELCOL.BRANCHCODE)
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

        For Each Row As DataRow In LNM0017tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.ID).Value = Row("ID") 'ユニークID
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCODE).Value = Row("ORDERORGCODE") '受注受付部署コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGNAME).Value = Row("ORDERORGNAME") '受注受付部署名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORY).Value = Row("ORDERORGCATEGORY") '受注受付部署判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORYNAME).Value = Row("ORDERORGCATEGORYNAME") '受注受付部署判定区分名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHO).Value = Row("SHUKABASHO") '出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHONAME).Value = Row("SHUKABASHONAME") '出荷場所名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORY).Value = Row("SHUKABASHOCATEGORY") '出荷場所判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORYNAME).Value = Row("SHUKABASHOCATEGORYNAME") '出荷場所判定名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECODE).Value = Row("TODOKECODE") '届先コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKENAME).Value = Row("TODOKENAME") '届先名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORY).Value = Row("TODOKECATEGORY") '届先判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORYNAME).Value = Row("TODOKECATEGORYNAME") '届先判定区分名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.RANGECODE).Value = Row("RANGECODE") '休日範囲コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.RANGENAME).Value = Row("RANGENAME") '休日範囲名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMFROM).Value = Row("GYOMUTANKNUMFROM") '車番（開始）
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMTO).Value = Row("GYOMUTANKNUMTO") '車番（終了）

            '単価
            If Convert.ToString(Row("TANKA")) = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TANKA).Value = Row("TANKA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TANKA).Value = CInt(Row("TANKA"))
            End If

            '金額を数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.INOUTEXCELCOL.TANKA).Style = IntStyle

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

            For Each Row As DataRow In LNM0017Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0017WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0017WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0017WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0017WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "休日割増単価マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0017Exceltbl) Then
            LNM0017Exceltbl = New DataTable
        End If
        If LNM0017Exceltbl.Columns.Count <> 0 Then
            LNM0017Exceltbl.Columns.Clear()
        End If
        LNM0017Exceltbl.Clear()

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

            For Each Row As DataRow In LNM0017Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0017WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0017WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0017WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0017WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.NEWDATA '新規の場合
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

        '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("   0   AS LINECNT ")
        SQLStr.AppendLine("        ,ID  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ORDERORGCODE  ")
        SQLStr.AppendLine("        ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("        ,SHUKABASHO  ")
        SQLStr.AppendLine("        ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("        ,TODOKECODE  ")
        SQLStr.AppendLine("        ,TODOKECATEGORY  ")
        SQLStr.AppendLine("        ,RANGECODE  ")
        SQLStr.AppendLine("        ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("        ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("        ,TANKA  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0017_HOLIDAYRATE ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0017Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

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

        Dim LNM0017Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0017Exceltblrow = LNM0017Exceltbl.NewRow

            'LINECNT
            LNM0017Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            'ユニークID
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.ID))
            WW_DATATYPE = DataTypeHT("ID")
            LNM0017Exceltblrow("ID") = LNM0017WRKINC.DataConvert("ユニークID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.TORICODE))
            WW_DATATYPE = DataTypeHT("TORICODE")
            LNM0017Exceltblrow("TORICODE") = LNM0017WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '受注受付部署コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCODE))
            WW_DATATYPE = DataTypeHT("ORDERORGCODE")
            LNM0017Exceltblrow("ORDERORGCODE") = LNM0017WRKINC.DataConvert("受注受付部署コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '受注受付部署判定区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.ORDERORGCATEGORY))
            WW_DATATYPE = DataTypeHT("ORDERORGCATEGORY")
            LNM0017Exceltblrow("ORDERORGCATEGORY") = LNM0017WRKINC.DataConvert("受注受付部署判定区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '出荷場所コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHO))
            WW_DATATYPE = DataTypeHT("SHUKABASHO")
            LNM0017Exceltblrow("SHUKABASHO") = LNM0017WRKINC.DataConvert("出荷場所コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '出荷場所判定区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.SHUKABASHOCATEGORY))
            WW_DATATYPE = DataTypeHT("SHUKABASHOCATEGORY")
            LNM0017Exceltblrow("SHUKABASHOCATEGORY") = LNM0017WRKINC.DataConvert("出荷場所判定区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '届先コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECODE))
            WW_DATATYPE = DataTypeHT("TODOKECODE")
            LNM0017Exceltblrow("TODOKECODE") = LNM0017WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '届先判定区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.TODOKECATEGORY))
            WW_DATATYPE = DataTypeHT("TODOKECATEGORY")
            LNM0017Exceltblrow("TODOKECATEGORY") = LNM0017WRKINC.DataConvert("届先判定区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '休日範囲コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.RANGECODE))
            WW_DATATYPE = DataTypeHT("RANGECODE")
            LNM0017Exceltblrow("RANGECODE") = LNM0017WRKINC.DataConvert("休日範囲コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '車番（開始）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMFROM))
            WW_DATATYPE = DataTypeHT("GYOMUTANKNUMFROM")
            LNM0017Exceltblrow("GYOMUTANKNUMFROM") = LNM0017WRKINC.DataConvert("車番（開始）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '車番（終了）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.GYOMUTANKNUMTO))
            WW_DATATYPE = DataTypeHT("GYOMUTANKNUMTO")
            LNM0017Exceltblrow("GYOMUTANKNUMTO") = LNM0017WRKINC.DataConvert("車番（終了）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '単価
            WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.TANKA)), ",", "")
            WW_DATATYPE = DataTypeHT("TANKA")
            LNM0017Exceltblrow("TANKA") = LNM0017WRKINC.DataConvert("単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0017WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0017Exceltblrow("DELFLG") = LNM0017WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0017Exceltbl.Rows.Add(LNM0017Exceltblrow)

        Next

    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    Select")
        SQLStr.AppendLine("        ID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(ID, '')                  = @ID ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')            = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORDERORGCODE, '')        = @ORDERORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORDERORGCATEGORY, '')    = @ORDERORGCATEGORY ")
        SQLStr.AppendLine("    AND  COALESCE(SHUKABASHO, '')          = @SHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(SHUKABASHOCATEGORY, '')  = @SHUKABASHOCATEGORY ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')          = @TODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKECATEGORY, '')      = @TODOKECATEGORY ")
        SQLStr.AppendLine("    AND  COALESCE(RANGECODE, '')           = @RANGECODE ")
        SQLStr.AppendLine("    AND  COALESCE(GYOMUTANKNUMFROM, '')    = @GYOMUTANKNUMFROM ")
        SQLStr.AppendLine("    AND  COALESCE(GYOMUTANKNUMTO, '')      = @GYOMUTANKNUMTO ")
        SQLStr.AppendLine("    AND  COALESCE(TANKA, '')               = @TANKA ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')              = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)     'ユニークID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar, 20)     '受注受付部署コード
                Dim P_ORDERORGCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCATEGORY", MySqlDbType.VarChar, 1)     '受注受付部署判定区分
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 20)     '出荷場所コード
                Dim P_SHUKABASHOCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHOCATEGORY", MySqlDbType.VarChar, 1)     '出荷場所判定区分
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 20)     '届先コード
                Dim P_TODOKECATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECATEGORY", MySqlDbType.VarChar, 1)     '届先判定区分
                Dim P_RANGECODE As MySqlParameter = SQLcmd.Parameters.Add("@RANGECODE", MySqlDbType.VarChar, 5)     '休日範囲コード
                Dim P_GYOMUTANKNUMFROM As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMFROM", MySqlDbType.VarChar, 20)     '車番（開始）
                Dim P_GYOMUTANKNUMTO As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMTO", MySqlDbType.VarChar, 20)     '車番（終了）
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)     '単価
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                P_ID.Value = WW_ROW("ID")           'ユニークID
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORDERORGCODE.Value = WW_ROW("ORDERORGCODE")           '受注受付部署コード
                P_ORDERORGCATEGORY.Value = WW_ROW("ORDERORGCATEGORY")           '受注受付部署判定区分
                P_SHUKABASHO.Value = WW_ROW("SHUKABASHO")           '出荷場所コード
                P_SHUKABASHOCATEGORY.Value = WW_ROW("SHUKABASHOCATEGORY")           '出荷場所判定区分
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                P_TODOKECATEGORY.Value = WW_ROW("TODOKECATEGORY")           '届先判定区分
                P_RANGECODE.Value = WW_ROW("RANGECODE")           '休日範囲コード
                P_GYOMUTANKNUMFROM.Value = WW_ROW("GYOMUTANKNUMFROM")           '車番（開始）
                P_GYOMUTANKNUMTO.Value = WW_ROW("GYOMUTANKNUMTO")           '車番（終了）
                P_TANKA.Value = WW_ROW("TANKA")           '単価
                P_DELFLG.Value = WW_ROW("DELFLG")           '削除フラグ

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE SELECT"
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
        If WW_ROW("ID") = 0 Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         ID  = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)     'ユニークID

                P_ID.Value = WW_ROW("ID")           'ユニークID

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE SELECT"
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
        SQLStr.Append("     LNG.LNM0017_HOLIDAYRATE                 ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         ID               = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                        'ID
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)             '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)    '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ

                P_ID.Value = WW_ROW("ID")                               'ID
                P_UPDYMD.Value = WW_DATENOW                             '更新年月日
                P_UPDUSER.Value = Master.USERID                         '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                   '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name            '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE UPDATE"
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
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @ID  ")
        SQLStr.AppendLine("     ,@TORICODE  ")
        SQLStr.AppendLine("     ,@ORDERORGCODE  ")
        SQLStr.AppendLine("     ,@ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,@SHUKABASHO  ")
        SQLStr.AppendLine("     ,@SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,@TODOKECODE  ")
        SQLStr.AppendLine("     ,@TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,@RANGECODE  ")
        SQLStr.AppendLine("     ,@GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,@GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,@TANKA  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      ID =  @ID")
        SQLStr.AppendLine("     ,TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,ORDERORGCODE =  @ORDERORGCODE")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY =  @ORDERORGCATEGORY")
        SQLStr.AppendLine("     ,SHUKABASHO =  @SHUKABASHO")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY =  @SHUKABASHOCATEGORY")
        SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
        SQLStr.AppendLine("     ,TODOKECATEGORY =  @TODOKECATEGORY")
        SQLStr.AppendLine("     ,RANGECODE =  @RANGECODE")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM =  @GYOMUTANKNUMFROM")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO =  @GYOMUTANKNUMTO")
        SQLStr.AppendLine("     ,TANKA =  @TANKA")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("     ,RECEIVEYMD =  @RECEIVEYMD")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                                        'ユニークID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)                      '取引先コード
                Dim P_ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar, 20)              '受注受付部署コード
                Dim P_ORDERORGCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCATEGORY", MySqlDbType.VarChar, 1)       '受注受付部署判定区分
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 20)                  '出荷場所コード
                Dim P_SHUKABASHOCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHOCATEGORY", MySqlDbType.VarChar, 1)   '出荷場所判定区分
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 20)                  '届先コード
                Dim P_TODOKECATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECATEGORY", MySqlDbType.VarChar, 1)           '届先判定区分
                Dim P_RANGECODE As MySqlParameter = SQLcmd.Parameters.Add("@RANGECODE", MySqlDbType.VarChar, 5)                     '休日範囲コード
                Dim P_GYOMUTANKNUMFROM As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMFROM", MySqlDbType.VarChar, 20)      '車番（開始）
                Dim P_GYOMUTANKNUMTO As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMTO", MySqlDbType.VarChar, 20)          '車番（終了）
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)                             '単価
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                           '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                           '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)                      '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)                  '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)                      '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                             '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                        '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                    '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                        '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)                     '集信日時
                'DB更新

                ' ユニークID取得
                Dim WW_DBDataCheck As String = ""
                Dim WW_ID As Integer = 0
                If WW_ROW("ID") = 0 Then
                    work.GetMaxID(SQLcon, WW_DBDataCheck, WW_ID)
                    If isNormal(WW_DBDataCheck) Then
                        WW_ROW("ID") = WW_ID                                'ユニークID
                    End If
                End If
                P_ID.Value = WW_ROW("ID")                                   'ユニークID
                P_TORICODE.Value = WW_ROW("TORICODE")                       '取引先コード
                P_ORDERORGCODE.Value = WW_ROW("ORDERORGCODE")               '受注受付部署コード
                P_ORDERORGCATEGORY.Value = WW_ROW("ORDERORGCATEGORY")       '受注受付部署判定区分
                P_SHUKABASHO.Value = WW_ROW("SHUKABASHO")                   '出荷場所コード
                P_SHUKABASHOCATEGORY.Value = WW_ROW("SHUKABASHOCATEGORY")   '出荷場所判定区分
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")                   '届先コード
                P_TODOKECATEGORY.Value = WW_ROW("TODOKECATEGORY")           '届先判定区分
                P_RANGECODE.Value = WW_ROW("RANGECODE")                     '休日範囲コード
                P_GYOMUTANKNUMFROM.Value = WW_ROW("GYOMUTANKNUMFROM")       '車番（開始）
                P_GYOMUTANKNUMTO.Value = WW_ROW("GYOMUTANKNUMTO")           '車番（終了）
                P_TANKA.Value = WW_ROW("TANKA")                             '単価
                P_DELFLG.Value = WW_ROW("DELFLG")                           '削除フラグ

                P_INITYMD.Value = WW_DATENOW                                '登録年月日
                P_INITUSER.Value = Master.USERID                            '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID                      '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name               '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                                 '更新年月日
                P_UPDUSER.Value = Master.USERID                             '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                       '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name                '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                          '集信日時

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0017_HOLIDAYRATE  INSERTUPDATE"
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

        'ユニークID(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ID", WW_ROW("ID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・ユニークIDエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("TORICODE", WW_ROW("TORICODE"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '受注受付部署コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORDERORGCODE", WW_ROW("ORDERORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("ORDERORGCODE", WW_ROW("ORDERORGCODE"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・受注受付部署コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・受注受付部署コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '受注受付部署判定区分(バリデーションチェック)
        '受注受付部署が指定されている時のみチェックし、指定なしの場合は、強制クリア
        If WW_ROW("ORDERORGCODE") = "" Then
            WW_ROW("ORDERORGCATEGORY") = ""
        Else
            Master.CheckField(Master.USERCAMP, "ORDERORGCATEGORY", WW_ROW("ORDERORGCATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(WW_ROW("ORDERORGCATEGORY")) Then
                    WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                    WW_CheckMES2 = "受付部署が入力された場合、必須です。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    ' 名称存在チェック
                    CODENAME_get("CATEGORY", WW_ROW("ORDERORGCATEGORY"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '出荷場所コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHUKABASHO", WW_ROW("SHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("SHUKABASHO", WW_ROW("SHUKABASHO"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・出荷場所コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・出荷場所コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '出荷場所判定区分(バリデーションチェック)
        '出荷場所が指定されている時のみチェックし、指定なしの場合は、強制クリア
        If WW_ROW("SHUKABASHO") = "" Then
            WW_ROW("SHUKABASHOCATEGORY") = ""
        Else
            Master.CheckField(Master.USERCAMP, "SHUKABASHOCATEGORY", WW_ROW("SHUKABASHOCATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(WW_ROW("SHUKABASHOCATEGORY")) Then
                    WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                    WW_CheckMES2 = "出荷場所が入力された場合、必須です。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    ' 名称存在チェック
                    CODENAME_get("CATEGORY", WW_ROW("SHUKABASHOCATEGORY"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '届先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TODOKECODE", WW_ROW("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("TODOKECODE", WW_ROW("TODOKECODE"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・届先コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・届先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '届先判定区分(バリデーションチェック)
        '届先が指定されている時のみチェックし、指定なしの場合は、強制クリア
        If WW_ROW("TODOKECODE") = "" Then
            WW_ROW("TODOKECATEGORY") = ""
        Else
            Master.CheckField(Master.USERCAMP, "TODOKECATEGORY", WW_ROW("TODOKECATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(WW_ROW("TODOKECATEGORY")) Then
                    WW_CheckMES1 = "・届先判定区エラーです。"
                    WW_CheckMES2 = "届先が入力された場合、必須です。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    ' 名称存在チェック
                    CODENAME_get("CATEGORY", WW_ROW("TODOKECATEGORY"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・届先判定区エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・届先判定区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '休日範囲コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "RANGECODE", WW_ROW("RANGECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            For i As Integer = 1 To WW_ROW("RANGECODE").ToString.Length
                ' 名称存在チェック
                CODENAME_get("CATEGORY", Mid(WW_ROW("RANGECODE"), i, 1), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Exit For
                End If
            Next
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・休日範囲コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・休日範囲コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '車番（開始）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "GYOMUTANKNUMFROM", WW_ROW("GYOMUTANKNUMFROM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車番（開始）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '車番（終了）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "GYOMUTANKNUMTO", WW_ROW("GYOMUTANKNUMTO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車番（終了）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TANKA", WW_ROW("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
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

        '休日割増単価マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ID")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         ID              = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)     'ID

                P_ID.Value = WW_ROW("ID")           'ID

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
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0029_HOLIDAYRATEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
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
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
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
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         ID             = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)     'ID

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_ID.Value = WW_ROW("ID")           'ID

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDAYRATEHIST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0029_HOLIDAYRATEHIST INSERT"
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


