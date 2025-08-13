''************************************************************
' 単価マスタメンテナンス・一覧画面
' 作成日 2024/12/16
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/16 新規作成
'          : 2025/05/23 統合版に変更
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' 単価マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0006TankaList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0006tbl As DataTable         '一覧格納用テーブル
    Private LNM0006UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0006Exceltbl As New DataTable  'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0006tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            InputSave()
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            InputSave()
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0006WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0006WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND" '戻るボタン押下
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
                             "WF_SelectDEPARTUREChange" '出荷(変更)時
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_ButtonExtract"         '検索ボタン押下時
                            GridViewInitialize()
                        Case "WF_ButtonRelease"         '解除ボタンクリック
                            MapInitialize(resVal:=WF_ButtonClick.Value)
                            'WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_ButtonPAGE", "WF_ButtonFIRST", "WF_ButtonPREVIOUS", "WF_ButtonNEXT", "WF_ButtonLAST"
                            Me.WF_ButtonPAGE_Click()
                            'Case "WF_TORIChange"            '荷主チェンジ
                            '    Me.WF_TODOKE.Items.Clear()
                            '    If Not WF_TORI.SelectedValue = "" Then
                            '        Dim retTodokeList As New DropDownList
                            '        retTodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, WF_TORI.SelectedValue)
                            '        For index As Integer = 0 To retTodokeList.Items.Count - 1
                            '            WF_TODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
                            '        Next
                            '    End If
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
            If Not IsNothing(LNM0006tbl) Then
                LNM0006tbl.Clear()
                LNM0006tbl.Dispose()
                LNM0006tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0006WRKINC.MAPIDL
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
        WF_StYMD.Value = Date.Now.ToString("yyyy/MM/dd")
        createListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

        Select Case Context.Handler.ToString().ToUpper()
            '○ 登録・履歴画面からの遷移
            Case C_PREV_MAP_LIST.LNM0006D, C_PREV_MAP_LIST.LNM0006H
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
        retToriList = LNM0006WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0006WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next

        '★解除ボタン押下
        If resVal = "WF_ButtonRelease" Then
            '届先
            Me.WF_TODOKE.Items.Clear()
            'Dim retTodokeList As New DropDownList
            'retTodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value)
            'For index As Integer = 0 To retTodokeList.Items.Count - 1
            '    WF_TODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
            'Next
        End If

        '出荷地
        Me.WF_DEPARTURE.Items.Clear()
        Dim retDepartureList As New DropDownList
        retDepartureList = LNM0006WRKINC.getDowpDownAvocadoshukaList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value)
        For index As Integer = 0 To retDepartureList.Items.Count - 1
            WF_DEPARTURE.Items.Add(New ListItem(retDepartureList.Items(index).Text, retDepartureList.Items(index).Value))
        Next

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        'If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0006S Then
        '    ' Grid情報保存先のファイル名
        '    Master.CreateXMLSaveFile()
        'ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0006D Then
        '    Master.RecoverTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)
        'End If

        Select Case Context.Handler.ToString().ToUpper()
            '○ 登録・履歴画面からの遷移
            Case C_PREV_MAP_LIST.LNM0006D, C_PREV_MAP_LIST.LNM0006H
                Master.RecoverTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)
                '対象日
                Dim WW_YMD As String = Replace(work.WF_SEL_TARGETYMD_L.Text, "/", "")
                WF_StYMD.Value = WW_YMD.Substring(0, 4) & "/" & WW_YMD.Substring(4, 2) & "/" & WW_YMD.Substring(6, 2)
                '荷主
                WF_TORI.SelectedValue = work.WF_SEL_TORI_L.Text
                '部門
                WF_ORG.SelectedValue = work.WF_SEL_ORG_L.Text
                '届先
                RestoreSelectTodoke()
                '出荷地
                WF_DEPARTURE.SelectedValue = work.WF_SEL_DEPARTURE_L.Text
                '車番
                WF_SHABAN_FROM.Text = work.WF_SEL_SHABAN_FROM_L.Text
                WF_SHABAN_TO.Text = work.WF_SEL_SHABAN_TO_L.Text
                '削除済みデータ表示状態
                ChkDelDataFlg.Checked = work.WF_SEL_CHKDELDATAFLG_L.Text
                '入力ページ
                TxtPageNo.Text = work.WF_SEL_INPUTPAGE_L.Text
                '○ MENUからの遷移
            Case Else
                If String.IsNullOrEmpty(Master.VIEWID) Then
                    rightview2.MAPIDS = LNM0006WRKINC.MAPIDS
                    rightview2.MAPID = LNM0006WRKINC.MAPIDL
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
                Master.CreateXMLSaveFile()
                WF_StYMD.Value = Date.Now.ToString("yyyy/MM/dd")
        End Select

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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
        Master.SaveTable(LNM0006tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0006tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = "1"

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0006tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0006tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0006tbl.Rows.Count) / CONST_DISPROWCOUNT)
        End If

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0006tbl)
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
        Master.SaveTable(LNM0006tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0006tbl.Rows.Count.ToString()

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = work.WF_SEL_NOWPAGECNT_L.Text

        '〇 最終ページ
        'Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNM0006tbl.Rows.Count) / CONST_DISPROWCOUNT)
        If LNM0006tbl.Rows.Count < CONST_DISPROWCOUNT Then
            Me.WF_TOTALPAGECNT.Text = 1
        Else
            Me.WF_TOTALPAGECNT.Text = Math.Ceiling((LNM0006tbl.Rows.Count) / CONST_DISPROWCOUNT)
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
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            If LNM0006row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0006row("SELECT") = WW_DataCNT
            End If
        Next

        Dim TBLview As DataView = New DataView(LNM0006tbl)
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

        If IsNothing(LNM0006tbl) Then
            LNM0006tbl = New DataTable
        End If

        If LNM0006tbl.Columns.Count <> 0 Then
            LNM0006tbl.Columns.Clear()
        End If

        LNM0006tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを単価マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNM0006.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TORICODE), '')                                    AS TORICODE            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TORINAME), '')                                    AS TORINAME            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ORGCODE), '')                                     AS ORGCODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ORGNAME), '')                                     AS ORGNAME             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.KASANORGCODE), '')                                AS KASANORGCODE        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.KASANORGNAME), '')                                AS KASANORGNAME        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.AVOCADOSHUKABASHO), '')                           AS AVOCADOSHUKABASHO   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.AVOCADOSHUKANAME), '')                            AS AVOCADOSHUKANAME    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SHUKABASHO), '')                                  AS SHUKABASHO          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SHUKANAME), '')                                   AS SHUKANAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.AVOCADOTODOKECODE), '')                           AS AVOCADOTODOKECODE   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.AVOCADOTODOKENAME), '')                           AS AVOCADOTODOKENAME   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TODOKECODE), '')                                  AS TODOKECODE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TODOKENAME), '')                                  AS TODOKENAME          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TANKNUMBER), '')                                  AS TANKNUMBER          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SHABAN), '')                                      AS SHABAN              ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNM0006.STYMD, '%Y/%m/%d'), '')                     AS STYMD               ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNM0006.ENDYMD, '%Y/%m/%d'), '')                    AS ENDYMD              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.BRANCHCODE), '')                                  AS BRANCHCODE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TANKAKBN), '')                                    AS TANKAKBN            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.MEMO), '')                                        AS MEMO                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TANKA), '')                                       AS TANKA               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ACCOUNTCODE), '')                                 AS ACCOUNTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ACCOUNTNAME), '')                                 AS ACCOUNTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SEGMENTCODE), '')                                 AS SEGMENTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SEGMENTNAME), '')                                 AS SEGMENTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.JOTPERCENTAGE), '')                               AS JOTPERCENTAGE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ENEXPERCENTAGE), '')                              AS ENEXPERCENTAGE      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.CALCKBN), '')                                     AS CALCKBN             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.ROUNDTRIP), '')                                   AS ROUNDTRIP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.TOLLFEE), '')                                     AS TOLLFEE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SYAGATA), '')                                     AS SYAGATA             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SYAGATANAME), '')                                 AS SYAGATANAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.SYABARA), '')                                     AS SYABARA             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0006.BIKOU3), '')                                      AS BIKOU3              ")

        '画面表示用
        '単価区分
        SQLStr.AppendLine("   , ''                                                                       AS SCRTANKAKBN        ")
        '単価
        SQLStr.AppendLine("   , CASE                                                                                           ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(TANKA), '') = '' THEN ''                                                  ")
        SQLStr.AppendLine("      ELSE  FORMAT(TANKA,0)                                                                         ")
        SQLStr.AppendLine("     END AS SCRTANKA                                                                                ")
        '割合JOT
        SQLStr.AppendLine("   , ''                                                                       AS SCRJOTPERCENTAGE   ")
        '割合ENEX
        SQLStr.AppendLine("   , ''                                                                       AS SCRENEXPERCENTAGE  ")
        '往復距離
        SQLStr.AppendLine("   , CASE                                                                                           ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(ROUNDTRIP), '') = '' THEN ''                                              ")
        SQLStr.AppendLine("      ELSE  FORMAT(ROUNDTRIP,3)                                                                   ")
        SQLStr.AppendLine("     END AS SCRROUNDTRIP                                                                            ")
        '通行料
        SQLStr.AppendLine("   , CASE                                                                                           ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(TOLLFEE), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(TOLLFEE,3)                                                                     ")
        SQLStr.AppendLine("     END AS SCRTOLLFEE                                                                              ")
        '車腹
        SQLStr.AppendLine("   , CASE                                                                                           ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SYABARA), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(SYABARA,3)                                                                     ")
        SQLStr.AppendLine("     END AS SCRSYABARA                                                                              ")

        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                    ")

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
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim dt As DateTime

        '有効開始日
        If DateTime.TryParse(WF_StYMD.Value, dt) Then
            SQLStr.AppendLine(" AND  @STYMD BETWEEN LNM0006.STYMD AND LNM0006.ENDYMD  ")
        End If
        '取引先コード
        If Not WF_TORI.SelectedValue = "" Then
            SQLStr.AppendLine(" AND  LNM0006.TORICODE = @TORICODE                                                               ")
        End If
        '部門コード
        If Not WF_ORG.SelectedValue = "" Then
            SQLStr.AppendLine(" AND  LNM0006.ORGCODE = @ORGCODE                                                                       ")
        End If
        ' 実績届先
        SelectTodoke()
        If Not WF_TODOKEhdn.Value = "" Then
            SQLStr.AppendLine(" AND LNM0006.AVOCADOTODOKECODE in (" & WF_TODOKEhdn.Value & ")")
        End If
        '実績出荷場所
        If Not WF_DEPARTURE.SelectedValue = "" Then
            SQLStr.AppendLine(" AND  LNM0006.AVOCADOSHUKABASHO = @AVOCADOSHUKABASHO                                          ")
        End If
        '車番
        Select Case True
            Case Not Trim(WF_SHABAN_FROM.Text) = "" And Not Trim(WF_SHABAN_TO.Text) = ""
                SQLStr.AppendLine(" AND  CONVERT(COALESCE(RTRIM(LNM0006.SHABAN), '0') , DECIMAL) BETWEEN @SHABAN_FROM AND @SHABAN_TO  ")
            Case Not Trim(WF_SHABAN_FROM.Text) = ""
                SQLStr.AppendLine(" AND  LNM0006.SHABAN = @SHABAN_FROM ")
            Case Not Trim(WF_SHABAN_TO.Text) = ""
                SQLStr.AppendLine(" AND  LNM0006.SHABAN = @SHABAN_TO ")
            Case Else
        End Select

        '削除フラグ
        If Not ChkDelDataFlg.Checked Then
            SQLStr.AppendLine(" AND  LNM0006.DELFLG = '0'                                                                       ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.TORICODE                                                           ")
        SQLStr.AppendLine("    ,LNM0006.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,LNM0006.KASANORGCODE                                                       ")
        SQLStr.AppendLine("    ,LNM0006.AVOCADOSHUKABASHO                                                  ")
        SQLStr.AppendLine("    ,LNM0006.AVOCADOTODOKECODE                                                  ")
        SQLStr.AppendLine("    ,LNM0006.SHABAN                                                             ")
        SQLStr.AppendLine("    ,LNM0006.STYMD                                                              ")
        SQLStr.AppendLine("    ,LNM0006.BRANCHCODE                                                         ")
        SQLStr.AppendLine("    ,LNM0006.SYAGATA                                                            ")
        SQLStr.AppendLine("    ,LNM0006.SYABARA                                                            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                'ロール
                Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                P_ROLE.Value = Master.ROLE_ORG

                '有効開始日
                If DateTime.TryParse(WF_StYMD.Value, dt) Then
                    Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)
                    P_STYMD.Value = dt
                End If
                '取引先コード
                If Not WF_TORI.SelectedValue = "" Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                    P_TORICODE.Value = WF_TORI.SelectedValue
                End If
                '部門コード
                If Not WF_ORG.SelectedValue = "" Then
                    Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)
                    P_ORGCODE.Value = WF_ORG.SelectedValue
                End If
                '実績出荷場所
                If Not WF_DEPARTURE.SelectedValue = "" Then
                    Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)
                    P_AVOCADOSHUKABASHO.Value = WF_DEPARTURE.SelectedValue
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

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0006row As DataRow In LNM0006tbl.Rows
                    i += 1
                    LNM0006row("LINECNT") = i        'LINECNT

                    '単価区分
                    Select Case LNM0006row("TANKAKBN").ToString
                        Case "0" : LNM0006row("SCRTANKAKBN") = "通常単価"
                        Case "1" : LNM0006row("SCRTANKAKBN") = "調整単価"
                        Case Else : LNM0006row("SCRTANKAKBN") = ""
                    End Select

                    '割合JOT
                    Select Case LNM0006row("JOTPERCENTAGE").ToString
                        Case "" : LNM0006row("SCRJOTPERCENTAGE") = ""
                        Case Else : LNM0006row("SCRJOTPERCENTAGE") = LNM0006row("JOTPERCENTAGE").ToString & "%"
                    End Select

                    '割合ENEX
                    Select Case LNM0006row("ENEXPERCENTAGE").ToString
                        Case "" : LNM0006row("SCRENEXPERCENTAGE") = ""
                        Case Else : LNM0006row("SCRENEXPERCENTAGE") = LNM0006row("ENEXPERCENTAGE").ToString & "%"
                    End Select

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006L Select"
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

        work.WF_SEL_LINECNT.Text = ""                                            '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)   '削除

        work.WF_SEL_TORICODE.Text = ""                                           '取引先コード
        work.WF_SEL_TORINAME.Text = ""                                           '取引先名称

        work.WF_SEL_ORGCODE.Text = ""                                            '部門コード
        work.WF_SEL_ORGNAME.Text = ""                                            '部門名称

        work.WF_SEL_KASANORGCODE.Text = ""                                           '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = ""                                           '加算先部門名称
        work.WF_SEL_AVOCADOSHUKABASHO.Text = ""                                      '実績出荷場所コード
        work.WF_SEL_AVOCADOSHUKANAME.Text = ""                                       '実績出荷場所名称
        work.WF_SEL_SHUKABASHO.Text = ""                                             '変換後出荷場所コード
        work.WF_SEL_SHUKANAME.Text = ""                                              '変換後出荷場所名称
        work.WF_SEL_AVOCADOTODOKECODE.Text = ""                                      '実績届先コード
        work.WF_SEL_AVOCADOTODOKENAME.Text = ""                                      '実績届先名称
        work.WF_SEL_TODOKECODE.Text = ""                                             '変換後届先コード
        work.WF_SEL_TODOKENAME.Text = ""                                             '変換後届先名称
        work.WF_SEL_TANKNUMBER.Text = ""                                             '陸事番号
        work.WF_SEL_SHABAN.Text = ""                                                 '車番
        work.WF_SEL_STYMD.Text = Date.Now.ToString("yyyy/MM/01")                     '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0006WRKINC.MAX_ENDYMD                           '有効終了日
        work.WF_SEL_BRANCHCODE.Text = "1"                                             '枝番
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TANKAKBN.Text)     '単価区分

        work.WF_SEL_MEMO.Text = ""                                                   '単価用途
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TANKA.Text)        '単価
        work.WF_SEL_ACCOUNTCODE.Text = ""                                                      '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = ""                                                      '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = ""                                                      'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = ""                                                      'セグメント名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_JOTPERCENTAGE.Text)        '割合JOT
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_ENEXPERCENTAGE.Text)        '割合ENEX

        work.WF_SEL_CALCKBN.Text = ""                                                '計算区分
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_ROUNDTRIP.Text)    '往復距離
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_TOLLFEE.Text)      '通行料
        work.WF_SEL_SYAGATA.Text = ""                                                '車型
        work.WF_SEL_SYAGATANAME.Text = ""                                            '車型名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SYABARA.Text)      '車腹
        work.WF_SEL_BIKOU1.Text = ""                                                 '備考1
        work.WF_SEL_BIKOU2.Text = ""                                                 '備考2
        work.WF_SEL_BIKOU3.Text = ""                                                 '備考3

        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0006TankaHistory.aspx")
    End Sub

    ' ******************************************************************************
    ' ***  フィールド変更処理                                                    ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールド(変更)時処理
    ''' </summary>
    ''' <param name="resVal">荷主(変更)時(WF_SelectTORIChange),部門(変更)時(WF_SelectORGChange),届先(変更)時(WF_SelectTODOKEChange)</param>
    ''' <remarks></remarks>
    Protected Sub WF_SelectFIELD_CHANGE(ByVal resVal As String)
        '■荷主(情報)取得
        Dim selectTORI As String = WF_TORI.SelectedValue
        Dim selectindexTORI As Integer = WF_TORI.SelectedIndex
        '■部門(情報)取得
        Dim selectORG As String = WF_ORG.SelectedValue
        Dim selectindexORG As Integer = WF_ORG.SelectedIndex
        '■出荷(情報)取得
        Dim selectDEPARTURE As String = WF_DEPARTURE.SelectedValue
        Dim selectindexDEPARTURE As Integer = WF_DEPARTURE.SelectedIndex

        '〇フィールド(変更)ボタン
        Select Case resVal
            '荷主(変更)時
            Case "WF_SelectTORIChange"
                selectORG = ""              '-- 部門(表示)初期化
                selectindexORG = 0          '-- 部門(INDEX)初期化
                selectDEPARTURE = ""        '-- 出荷(表示)初期化
                selectindexDEPARTURE = 0    '-- 出荷(INDEX)初期化
            '部門(変更)時
            Case "WF_SelectORGChange"
                selectDEPARTURE = ""        '-- 出荷(表示)初期化
                selectindexDEPARTURE = 0    '-- 出荷(INDEX)初期化
            '出荷(変更)時
            Case "WF_SelectDEPARTUREChange"

        End Select

        '〇荷主
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0006WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_TODOKECODE:="", I_SHUKABASHO:=selectDEPARTURE)
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
        retOrgList = LNM0006WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_TODOKECODE:="", I_SHUKABASHO:=selectDEPARTURE)
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

        '〇届先
        Me.WF_TODOKE.Items.Clear()
        If Not WF_TORI.SelectedValue = "" Then
            Dim retTodokeList As New DropDownList
            retTodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_SHUKABASHO:=selectDEPARTURE)
            For index As Integer = 0 To retTodokeList.Items.Count - 1
                WF_TODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
            Next
        End If

        '〇出荷
        Me.WF_DEPARTURE.Items.Clear()
        Dim retDepartureList As New DropDownList
        retDepartureList = LNM0006WRKINC.getDowpDownAvocadoshukaList(Master.MAPID, Master.ROLE_ORG, WF_StYMD.Value, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_SHUKABASHO:=selectDEPARTURE)
        '★ドロップダウンリスト選択(出荷)の場合
        If retDepartureList.Items(0).Text <> "全て表示" Then
            WF_DEPARTURE.Items.Add(New ListItem("全て表示", ""))
            selectindexDEPARTURE = 1
        End If
        '★ドロップダウンリスト再作成(出荷)
        For index As Integer = 0 To retDepartureList.Items.Count - 1
            WF_DEPARTURE.Items.Add(New ListItem(retDepartureList.Items(index).Text, retDepartureList.Items(index).Value))
        Next
        WF_DEPARTURE.SelectedIndex = selectindexDEPARTURE

    End Sub

    ''' <summary>
    ''' 画面初期化処理
    ''' </summary>
    Private Sub MapInitialize(Optional ByVal resVal As String = Nothing)
        'ドロップダウン生成処理
        createListBox(resVal:=WF_ButtonClick.Value)

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
        For Each LNS0008row As DataRow In LNM0006tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNM0006tbl)

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
        Dim TBLview As New DataView(LNM0006tbl)
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
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            If LNM0006row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0006row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNM0006tbl)

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
        work.WF_SEL_TARGETYMD_L.Text = WF_StYMD.Value '対象日
        work.WF_SEL_TORI_L.Text = WF_TORI.SelectedValue '荷主
        work.WF_SEL_ORG_L.Text = WF_ORG.SelectedValue '部門
        ' 届先
        SelectTodoke()
        work.WF_SEL_TODOKE_L.Text = WF_TODOKEhdn.Value

        work.WF_SEL_DEPARTURE_L.Text = WF_DEPARTURE.SelectedValue '出荷地
        work.WF_SEL_SHABAN_FROM_L.Text = WF_SHABAN_FROM.Text '車番FROM
        work.WF_SEL_SHABAN_TO_L.Text = WF_SHABAN_TO.Text '車番TO
        work.WF_SEL_CHKDELDATAFLG_L.Text = ChkDelDataFlg.Checked '削除済みデータ表示状態
        work.WF_SEL_INPUTPAGE_L.Text = TxtPageNo.Text '入力ページ
        work.WF_SEL_NOWPAGECNT_L.Text = WF_NOWPAGECNT.Text

    End Sub

    ''' <summary>
    ''' 実績届先プルダウン選択値取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SelectTodoke()

        Me.WF_TODOKEhdn.Value = ""
        Me.WF_TODOKENAMEhdn.Value = ""

        If Me.WF_TODOKE.Items.Count > 0 Then
            Dim SelectedCount As Integer = 0
            Dim intSelCnt As Integer = 0
            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To WF_TODOKE.Items.Count - 1
                If WF_TODOKE.Items(index).Selected = True Then
                    SelectedCount += 1
                    If intSelCnt = 0 Then
                        Me.WF_TODOKEhdn.Value = WF_TODOKE.Items(index).Value
                        Me.WF_TODOKENAMEhdn.Value = WF_TODOKE.Items(index).Text
                        intSelCnt = 1
                    Else
                        Me.WF_TODOKEhdn.Value = Me.WF_TODOKEhdn.Value & "," & WF_TODOKE.Items(index).Value
                        Me.WF_TODOKENAMEhdn.Value = Me.WF_TODOKENAMEhdn.Value & "," & WF_TODOKE.Items(index).Text
                        intSelCnt = 2
                    End If
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' 実績届先プルダウン選択値復元
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub RestoreSelectTodoke()
        Me.WF_TODOKE.Items.Clear()
        If Not work.WF_SEL_TORI_L.Text = "" Then
            Dim retTodokeList As New DropDownList
            retTodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TORI_L.Text)
            For index As Integer = 0 To retTodokeList.Items.Count - 1
                WF_TODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
            Next
        End If

        Dim TodokeHT As New Hashtable
        Dim arrTodoke() As String = work.WF_SEL_TODOKE_L.Text.Split(",")
        For Each Value In arrTodoke
            TodokeHT.Add(Value, Value)
        Next
        For index As Integer = 0 To WF_TODOKE.Items.Count - 1
            If TodokeHT.ContainsKey(WF_TODOKE.Items(index).Value) Then
                WF_TODOKE.Items(index).Selected = True
            End If
        Next
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
        If LNM0006tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE Then
            Dim WW_ROW As DataRow
            WW_ROW = LNM0006tbl.Rows(WW_LineCNT)
            Dim DATENOW As Date = Date.Now
            Dim WW_UPDTIMSTP As Date

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                '履歴登録(変更前)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '削除フラグ有効化
                DelflgValid(SQLcon, WW_ROW, DATENOW, WW_UPDTIMSTP)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '履歴登録(変更後)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0006WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                LNM0006tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.ALIVE
                LNM0006tbl.Rows(WW_LineCNT)("UPDTIMSTP") = WW_UPDTIMSTP
                Master.SaveTable(LNM0006tbl)
                Master.Output(C_MESSAGE_NO.DELETE_ROW_ACTIVATION, C_MESSAGE_TYPE.NOR, needsPopUp:=True)
            End Using
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = LNM0006tbl.Rows(WW_LineCNT)("LINECNT")                           '選択行

        work.WF_SEL_TORICODE.Text = LNM0006tbl.Rows(WW_LineCNT)("TORICODE")                         '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0006tbl.Rows(WW_LineCNT)("TORINAME")                         '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0006tbl.Rows(WW_LineCNT)("ORGCODE")                           '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0006tbl.Rows(WW_LineCNT)("ORGNAME")                           '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0006tbl.Rows(WW_LineCNT)("KASANORGCODE")                 '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0006tbl.Rows(WW_LineCNT)("KASANORGNAME")                 '加算先部門名称
        work.WF_SEL_AVOCADOSHUKABASHO.Text = LNM0006tbl.Rows(WW_LineCNT)("AVOCADOSHUKABASHO")       '実績出荷場所コード
        work.WF_SEL_AVOCADOSHUKANAME.Text = LNM0006tbl.Rows(WW_LineCNT)("AVOCADOSHUKANAME")         '実績出荷場所名称
        work.WF_SEL_SHUKABASHO.Text = LNM0006tbl.Rows(WW_LineCNT)("SHUKABASHO")                     '変換後出荷場所コード
        work.WF_SEL_SHUKANAME.Text = LNM0006tbl.Rows(WW_LineCNT)("SHUKANAME")                       '変換後出荷場所名称
        work.WF_SEL_AVOCADOTODOKECODE.Text = LNM0006tbl.Rows(WW_LineCNT)("AVOCADOTODOKECODE")       '実績届先コード
        work.WF_SEL_AVOCADOTODOKENAME.Text = LNM0006tbl.Rows(WW_LineCNT)("AVOCADOTODOKENAME")       '実績届先名称
        work.WF_SEL_TODOKECODE.Text = LNM0006tbl.Rows(WW_LineCNT)("TODOKECODE")                     '変換後届先コード
        work.WF_SEL_TODOKENAME.Text = LNM0006tbl.Rows(WW_LineCNT)("TODOKENAME")                     '変換後届先名称
        work.WF_SEL_TANKNUMBER.Text = LNM0006tbl.Rows(WW_LineCNT)("TANKNUMBER")                     '陸事番号
        work.WF_SEL_SHABAN.Text = LNM0006tbl.Rows(WW_LineCNT)("SHABAN")                             '車番
        work.WF_SEL_STYMD.Text = LNM0006tbl.Rows(WW_LineCNT)("STYMD")                               '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0006tbl.Rows(WW_LineCNT)("ENDYMD")                             '有効終了日
        work.WF_SEL_BRANCHCODE.Text = LNM0006tbl.Rows(WW_LineCNT)("BRANCHCODE")                     '枝番
        work.WF_SEL_TANKAKBN.Text = LNM0006tbl.Rows(WW_LineCNT)("TANKAKBN")                         '単価区分
        work.WF_SEL_MEMO.Text = LNM0006tbl.Rows(WW_LineCNT)("MEMO")                                 '単価用途
        work.WF_SEL_TANKA.Text = LNM0006tbl.Rows(WW_LineCNT)("TANKA")                               '単価
        work.WF_SEL_ACCOUNTCODE.Text = LNM0006tbl.Rows(WW_LineCNT)("ACCOUNTCODE")                   '勘定科目コード
        work.WF_SEL_ACCOUNTNAME.Text = LNM0006tbl.Rows(WW_LineCNT)("ACCOUNTNAME")                   '勘定科目名
        work.WF_SEL_SEGMENTCODE.Text = LNM0006tbl.Rows(WW_LineCNT)("SEGMENTCODE")                   'セグメントコード
        work.WF_SEL_SEGMENTNAME.Text = LNM0006tbl.Rows(WW_LineCNT)("SEGMENTNAME")                   'セグメント名
        work.WF_SEL_JOTPERCENTAGE.Text = LNM0006tbl.Rows(WW_LineCNT)("JOTPERCENTAGE")               '割合JOT
        work.WF_SEL_ENEXPERCENTAGE.Text = LNM0006tbl.Rows(WW_LineCNT)("ENEXPERCENTAGE")             '割合ENEX
        work.WF_SEL_CALCKBN.Text = LNM0006tbl.Rows(WW_LineCNT)("CALCKBN")                           '計算区分
        work.WF_SEL_ROUNDTRIP.Text = LNM0006tbl.Rows(WW_LineCNT)("ROUNDTRIP")                       '往復距離
        work.WF_SEL_TOLLFEE.Text = LNM0006tbl.Rows(WW_LineCNT)("TOLLFEE")                           '通行料
        work.WF_SEL_SYAGATA.Text = LNM0006tbl.Rows(WW_LineCNT)("SYAGATA")                           '車型
        work.WF_SEL_SYAGATANAME.Text = LNM0006tbl.Rows(WW_LineCNT)("SYAGATANAME")                   '車型名
        work.WF_SEL_SYABARA.Text = LNM0006tbl.Rows(WW_LineCNT)("SYABARA")                           '車腹
        work.WF_SEL_BIKOU1.Text = LNM0006tbl.Rows(WW_LineCNT)("BIKOU1")                             '備考1
        work.WF_SEL_BIKOU2.Text = LNM0006tbl.Rows(WW_LineCNT)("BIKOU2")                             '備考2
        work.WF_SEL_BIKOU3.Text = LNM0006tbl.Rows(WW_LineCNT)("BIKOU3")                             '備考3
        work.WF_SEL_DELFLG.Text = LNM0006tbl.Rows(WW_LineCNT)("DELFLG")                             '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0006tbl.Rows(WW_LineCNT)("UPDTIMSTP")                       'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                 '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                            work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_KASANORGCODE.Text,
                          work.WF_SEL_AVOCADOSHUKABASHO.Text, work.WF_SEL_AVOCADOTODOKECODE.Text, work.WF_SEL_SHABAN.Text,
                          work.WF_SEL_STYMD.Text, work.WF_SEL_BRANCHCODE.Text,
                          work.WF_SEL_SYAGATA.Text, work.WF_SEL_SYABARA.Text)
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
        SQLStr.Append("     LNG.LNM0006_NEWTANKA                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '0'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.Append("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.Append("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006L UPDATE"
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
        SQLStrTimStp.AppendLine(" SELECT ")
        SQLStrTimStp.AppendLine("    UPDTIMSTP                                          ")
        SQLStrTimStp.AppendLine(" FROM")
        SQLStrTimStp.AppendLine("     LNG.LNM0006_NEWTANKA")
        SQLStrTimStp.AppendLine(" WHERE")
        SQLStrTimStp.AppendLine("       TORICODE  = @TORICODE                           ")
        SQLStrTimStp.AppendLine("   AND ORGCODE  = @ORGCODE                             ")
        SQLStrTimStp.AppendLine("   AND KASANORGCODE  = @KASANORGCODE                   ")
        SQLStrTimStp.AppendLine("   AND AVOCADOSHUKABASHO  = @AVOCADOSHUKABASHO         ")
        SQLStrTimStp.AppendLine("   AND AVOCADOTODOKECODE  = @AVOCADOTODOKECODE         ")
        SQLStrTimStp.AppendLine("   AND SHABAN  = @SHABAN                               ")
        SQLStrTimStp.AppendLine("   AND BRANCHCODE  = @BRANCHCODE                       ")
        SQLStrTimStp.AppendLine("   AND SYAGATA  = @SYAGATA                             ")
        SQLStrTimStp.AppendLine("   AND SYABARA  = @SYABARA                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrTimStp.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0006WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "単価マスタ一覧"
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
            Case LNM0006WRKINC.FILETYPE.EXCEL
                FileName = "単価マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0006WRKINC.FILETYPE.PDF
                FileName = "単価マスタ.pdf"
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
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.KASANORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '加算先部門コード
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '実績出荷場所コード
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '実績届先コード
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.SHABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '枝番
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.SYAGATA).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車型
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.SYABARA).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車腹

        '入力不要列網掛け
        'sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '枝番
        'sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.ENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '有効終了日
        sheet.Columns(LNM0006WRKINC.INOUTEXCELCOL.SYAGATANAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '車型名

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
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "（必須）加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = "（必須）実績出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = "実績出荷場所名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKABASHO).Value = "変換後出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKANAME).Value = "変換後出荷場所名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = "（必須）実績届先コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = "実績届先名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKECODE).Value = "変換後届先コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKENAME).Value = "変換後届先名称"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TANKNUMBER).Value = "陸事番号"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SHABAN).Value = "（必須）車番"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.STYMD).Value = "（必須）有効開始日"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENDYMD).Value = "有効終了日"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Value = "（必須）枝番"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TANKAKBN).Value = "単価区分"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.MEMO).Value = "単価用途"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TANKA).Value = "単価"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = "勘定科目コード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = "勘定科目名"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = "セグメントコード"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = "セグメント名"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = "割合JOT"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = "割合ENEX"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.CALCKBN).Value = "計算区分"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ROUNDTRIP).Value = "往復距離"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TOLLFEE).Value = "通行料"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATA).Value = "（必須）車型"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = "車型名"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SYABARA).Value = "（必須）車腹"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU1).Value = "備考1"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU2).Value = "備考2"
        sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU3).Value = "備考3"

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
                sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '有効終了日
            WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
            sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENDYMD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENDYMD).Comment.Shape
                .Width = 180
                .Height = 30
            End With

            '単価区分
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("0:通常単価")
            WW_TEXTLIST.AppendLine("1:調整単価")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TANKAKBN).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.TANKAKBN).Comment.Shape
                .Width = 100
                .Height = 30
            End With

            '割合JOT
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("JOT手数料として収受する割合(JOT収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Comment.Shape
                .Width = 400
                .Height = 30
            End With

            '割合ENEX
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("ENEXへ支払う割合(ENEX収入分)をパーセンテージで入力してください。")
            WW_TEXTLIST.AppendLine("JOTとENEXの割合は、合計100%となるようにしてください。")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Comment.Shape
                .Width = 400
                .Height = 30
            End With

            '車型
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:単車")
            WW_TEXTLIST.AppendLine("2:トレーラ")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATA).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATA).Comment.Shape
                .Width = 100
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
        SETFIXVALUELIST(subsheet, "DELFLG", LNM0006WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0006WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG)
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
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE)
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
        DecStyle.NumberFormat = "#,##0.000_);[Red](#,##0.000)"

        '数値書式(小数点含む)
        Dim DecStyle2 As IStyle = wb.Styles.Add("DecStyle2")
        DecStyle2.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNM0006tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = Row("AVOCADOSHUKABASHO") '実績出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = Row("AVOCADOSHUKANAME") '実績出荷場所名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKABASHO).Value = Row("SHUKABASHO") '変換後出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKANAME).Value = Row("SHUKANAME") '変換後出荷場所名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = Row("AVOCADOTODOKECODE") '実績届先コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = Row("AVOCADOTODOKENAME") '実績届先名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKECODE).Value = Row("TODOKECODE") '変換後届先コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKENAME).Value = Row("TODOKENAME") '変換後届先名称
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TANKNUMBER).Value = Row("TANKNUMBER") '陸事番号
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SHABAN).Value = Row("SHABAN") '車番
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.STYMD).Value = Row("STYMD") '有効開始日
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ENDYMD).Value = Row("ENDYMD") '有効終了日

            '枝番
            If Row("BRANCHCODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Value = Row("BRANCHCODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Value = CDbl(Row("BRANCHCODE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TANKAKBN).Value = Row("TANKAKBN") '単価区分
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.MEMO).Value = Row("MEMO") '単価用途

            '単価
            If Row("TANKA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TANKA).Value = Row("TANKA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TANKA).Value = CDbl(Row("TANKA"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTCODE).Value = Row("ACCOUNTCODE") '勘定科目コード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '勘定科目名
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTCODE).Value = Row("SEGMENTCODE") 'セグメントコード
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTNAME).Value = Row("SEGMENTNAME") 'セグメント名

            '割合JOT
            If Row("JOTPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = Row("JOTPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Value = CDbl(Row("JOTPERCENTAGE"))
            End If

            '割合ENEX
            If Row("ENEXPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = Row("ENEXPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Value = CDbl(Row("ENEXPERCENTAGE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.CALCKBN).Value = Row("CALCKBN") '計算区分

            '往復距離
            If Row("ROUNDTRIP") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ROUNDTRIP).Value = Row("ROUNDTRIP")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ROUNDTRIP).Value = CDbl(Row("ROUNDTRIP"))
            End If

            '通行料
            If Row("TOLLFEE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TOLLFEE).Value = Row("TOLLFEE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TOLLFEE).Value = CDbl(Row("TOLLFEE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATA).Value = Row("SYAGATA") '車型
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = Row("SYAGATANAME") '車型名

            '車腹
            If Row("SYABARA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SYABARA).Value = Row("SYABARA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SYABARA).Value = CDbl(Row("SYABARA"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3

            '金額を数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TANKA).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ROUNDTRIP).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.TOLLFEE).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.SYABARA).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE).Style = DecStyle2
            sheet.Cells(WW_ACTIVEROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE).Style = DecStyle2


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
        filePath = "D:\単価マスタ一括アップロードテスト.xlsx"

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

            For Each Row As DataRow In LNM0006Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0006WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0006WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0006WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        Continue For
                    End If


                    '有効開始日、有効終了日更新
                    If Not Row("TORICODE") = "" And
                       Not Row("ORGCODE") = "" And
                       Not Row("KASANORGCODE") = "" And
                       Not Row("AVOCADOSHUKABASHO") = "" And
                       Not Row("AVOCADOTODOKECODE") = "" And
                       Not Row("SHABAN") = "" And
                       Not Row("STYMD") = Date.MinValue And
                       Not Row("SYAGATA") = "" Then

                        WF_AUTOENDYMD.Value = ""

                        '枝番が新規、有効開始日が変更されたときの対応
                        If Row("BRANCHCODE").ToString = "" Then '枝番なし(新規の場合)
                            '枝番を生成
                            Row("BRANCHCODE") = LNM0006WRKINC.GenerateBranchCode(SQLcon, Row, WW_DBDataCheck)
                            WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If
                        Else
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0006WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            Select Case True
                                Case WW_BeforeMAXSTYMD = "" '無いと思うが1件も対象の枝番データが無い場合
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd") '同一の場合
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
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
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                                Case Else
                                    '有効終了日に有効開始日の月の末日を入れる
                                    Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                    WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                            End Select
                        End If
                    End If

                    Dim WW_MODIFYKBN As String = ""
                        Dim WW_BEFDELFLG As String = ""

                        '変更チェック
                        MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If

                        '変更がある場合履歴テーブルに変更前データを登録
                        If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA Then
                            '履歴登録(変更前)
                            InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                            If Not isNormal(WW_ErrSW) Then
                                Exit Sub
                            End If
                            '登録後変更区分を変更後にする
                            WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.AFTDATA
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
        If IsNothing(LNM0006Exceltbl) Then
            LNM0006Exceltbl = New DataTable
        End If
        If LNM0006Exceltbl.Columns.Count <> 0 Then
            LNM0006Exceltbl.Columns.Clear()
        End If
        LNM0006Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\TANKAEXCEL"
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
        Dim fileNameHead As String = "TANKAEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0006Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0006WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0006WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0006WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    '有効開始日、有効終了日更新
                    If Not Row("TORICODE") = "" And
                       Not Row("ORGCODE") = "" And
                       Not Row("KASANORGCODE") = "" And
                       Not Row("AVOCADOSHUKABASHO") = "" And
                       Not Row("AVOCADOTODOKECODE") = "" And
                       Not Row("SHABAN") = "" And
                       Not Row("STYMD") = Date.MinValue And
                       Not Row("SYAGATA") = "" Then

                        WF_AUTOENDYMD.Value = ""

                        '枝番が新規、有効開始日が変更されたときの対応
                        If Row("BRANCHCODE").ToString = "" Then '枝番なし(新規の場合)
                            '枝番を生成
                            Row("BRANCHCODE") = LNM0006WRKINC.GenerateBranchCode(SQLcon, Row, WW_DBDataCheck)
                            WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If
                        Else
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0006WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            Select Case True
                                Case WW_BeforeMAXSTYMD = "" '無いと思うが1件も対象の枝番データが無い場合
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd") '同一の場合
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
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
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0006WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                                Case Else
                                    '有効終了日に有効開始日の月の末日を入れる
                                    Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                    WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                            End Select
                        End If
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                        '変更チェック
                        MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If

                        '変更がある場合履歴テーブルに変更前データを登録
                        If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA Then
                            '履歴登録(変更前)
                            InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                            If Not isNormal(WW_ErrSW) Then
                                Exit Sub
                            End If
                            '登録後変更区分を変更後にする
                            WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.AFTDATA
                        End If


                        '件数カウント
                        Select Case True
                            Case Row("DELFLG") = "1" '削除の場合
                                WW_UplDelCnt += 1
                            Case WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("        ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("        ,SHUKABASHO  ")
        SQLStr.AppendLine("        ,SHUKANAME  ")
        SQLStr.AppendLine("        ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("        ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("        ,TODOKECODE  ")
        SQLStr.AppendLine("        ,TODOKENAME  ")
        SQLStr.AppendLine("        ,TANKNUMBER  ")
        SQLStr.AppendLine("        ,SHABAN  ")
        SQLStr.AppendLine("        ,STYMD  ")
        SQLStr.AppendLine("        ,ENDYMD  ")
        SQLStr.AppendLine("        ,BRANCHCODE  ")
        SQLStr.AppendLine("        ,TANKAKBN  ")
        SQLStr.AppendLine("        ,MEMO  ")
        SQLStr.AppendLine("        ,TANKA  ")
        SQLStr.AppendLine("        ,ACCOUNTCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,SEGMENTCODE  ")
        SQLStr.AppendLine("        ,SEGMENTNAME  ")
        SQLStr.AppendLine("        ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("        ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("        ,CALCKBN  ")
        SQLStr.AppendLine("        ,ROUNDTRIP  ")
        SQLStr.AppendLine("        ,TOLLFEE  ")
        SQLStr.AppendLine("        ,SYAGATA  ")
        SQLStr.AppendLine("        ,SYAGATANAME  ")
        SQLStr.AppendLine("        ,SYABARA  ")
        SQLStr.AppendLine("        ,BIKOU1  ")
        SQLStr.AppendLine("        ,BIKOU2  ")
        SQLStr.AppendLine("        ,BIKOU3  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0006_NEWTANKA ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0006Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006_NEWTANKA SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
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

            Dim LNM0006Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNM0006Exceltblrow = LNM0006Exceltbl.NewRow

                'LINECNT
                LNM0006Exceltblrow("LINECNT") = WW_LINECNT
                WW_LINECNT = WW_LINECNT + 1

                '◆データセット
                '取引先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TORICODE))
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNM0006Exceltblrow("TORICODE") = LNM0006WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TORINAME))
                WW_DATATYPE = DataTypeHT("TORINAME")
                LNM0006Exceltblrow("TORINAME") = LNM0006WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ORGCODE))
                WW_DATATYPE = DataTypeHT("ORGCODE")
                LNM0006Exceltblrow("ORGCODE") = LNM0006WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ORGNAME))
                WW_DATATYPE = DataTypeHT("ORGNAME")
                LNM0006Exceltblrow("ORGNAME") = LNM0006WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGCODE))
                WW_DATATYPE = DataTypeHT("KASANORGCODE")
                LNM0006Exceltblrow("KASANORGCODE") = LNM0006WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.KASANORGNAME))
                WW_DATATYPE = DataTypeHT("KASANORGNAME")
                LNM0006Exceltblrow("KASANORGNAME") = LNM0006WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実績出荷場所コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO))
                WW_DATATYPE = DataTypeHT("AVOCADOSHUKABASHO")
                LNM0006Exceltblrow("AVOCADOSHUKABASHO") = LNM0006WRKINC.DataConvert("実績出荷場所コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実績出荷場所名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME))
                WW_DATATYPE = DataTypeHT("AVOCADOSHUKANAME")
                LNM0006Exceltblrow("AVOCADOSHUKANAME") = LNM0006WRKINC.DataConvert("実績出荷場所名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '変換後出荷場所コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKABASHO))
                WW_DATATYPE = DataTypeHT("SHUKABASHO")
                LNM0006Exceltblrow("SHUKABASHO") = LNM0006WRKINC.DataConvert("変換後出荷場所コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '変換後出荷場所名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SHUKANAME))
                WW_DATATYPE = DataTypeHT("SHUKANAME")
                LNM0006Exceltblrow("SHUKANAME") = LNM0006WRKINC.DataConvert("変換後出荷場所名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実績届先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE))
                WW_DATATYPE = DataTypeHT("AVOCADOTODOKECODE")
                LNM0006Exceltblrow("AVOCADOTODOKECODE") = LNM0006WRKINC.DataConvert("実績届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実績届先名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME))
                WW_DATATYPE = DataTypeHT("AVOCADOTODOKENAME")
                LNM0006Exceltblrow("AVOCADOTODOKENAME") = LNM0006WRKINC.DataConvert("実績届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '変換後届先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKECODE))
                WW_DATATYPE = DataTypeHT("TODOKECODE")
                LNM0006Exceltblrow("TODOKECODE") = LNM0006WRKINC.DataConvert("変換後届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '変換後届先名称
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TODOKENAME))
                WW_DATATYPE = DataTypeHT("TODOKENAME")
                LNM0006Exceltblrow("TODOKENAME") = LNM0006WRKINC.DataConvert("変換後届先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '陸事番号
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TANKNUMBER))
                WW_DATATYPE = DataTypeHT("TANKNUMBER")
                LNM0006Exceltblrow("TANKNUMBER") = LNM0006WRKINC.DataConvert("陸事番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SHABAN))
                WW_DATATYPE = DataTypeHT("SHABAN")
                LNM0006Exceltblrow("SHABAN") = LNM0006WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '有効開始日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.STYMD))
                WW_DATATYPE = DataTypeHT("STYMD")
                LNM0006Exceltblrow("STYMD") = LNM0006WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '有効終了日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ENDYMD))
                WW_DATATYPE = DataTypeHT("ENDYMD")
                LNM0006Exceltblrow("ENDYMD") = LNM0006WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '枝番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.BRANCHCODE))
                WW_DATATYPE = DataTypeHT("BRANCHCODE")
                LNM0006Exceltblrow("BRANCHCODE") = LNM0006WRKINC.DataConvert("枝番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '単価区分
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TANKAKBN)), ",", "")
                WW_DATATYPE = DataTypeHT("TANKAKBN")
                LNM0006Exceltblrow("TANKAKBN") = LNM0006WRKINC.DataConvert("単価区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '単価用途
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.MEMO))
                WW_DATATYPE = DataTypeHT("MEMO")
                LNM0006Exceltblrow("MEMO") = LNM0006WRKINC.DataConvert("単価用途", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '単価
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TANKA)), ",", "")
                WW_DATATYPE = DataTypeHT("TANKA")
                LNM0006Exceltblrow("TANKA") = LNM0006WRKINC.DataConvert("単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTCODE))
                WW_DATATYPE = DataTypeHT("ACCOUNTCODE")
                LNM0006Exceltblrow("ACCOUNTCODE") = LNM0006WRKINC.DataConvert("勘定科目コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '勘定科目名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ACCOUNTNAME))
                WW_DATATYPE = DataTypeHT("ACCOUNTNAME")
                LNM0006Exceltblrow("ACCOUNTNAME") = LNM0006WRKINC.DataConvert("勘定科目名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメントコード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTCODE))
                WW_DATATYPE = DataTypeHT("SEGMENTCODE")
                LNM0006Exceltblrow("SEGMENTCODE") = LNM0006WRKINC.DataConvert("セグメントコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'セグメント名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SEGMENTNAME))
                WW_DATATYPE = DataTypeHT("SEGMENTNAME")
                LNM0006Exceltblrow("SEGMENTNAME") = LNM0006WRKINC.DataConvert("セグメント名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合JOT
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.JOTPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("JOTPERCENTAGE")
                LNM0006Exceltblrow("JOTPERCENTAGE") = LNM0006WRKINC.DataConvert("割合JOT", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '割合ENEX
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ENEXPERCENTAGE)), ",", "")
                WW_DATATYPE = DataTypeHT("ENEXPERCENTAGE")
                LNM0006Exceltblrow("ENEXPERCENTAGE") = LNM0006WRKINC.DataConvert("割合ENEX", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '計算区分
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.CALCKBN))
                WW_DATATYPE = DataTypeHT("CALCKBN")
                LNM0006Exceltblrow("CALCKBN") = LNM0006WRKINC.DataConvert("計算区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '往復距離
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.ROUNDTRIP)), ",", "")
                WW_DATATYPE = DataTypeHT("ROUNDTRIP")
                LNM0006Exceltblrow("ROUNDTRIP") = LNM0006WRKINC.DataConvert("往復距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '通行料
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.TOLLFEE)), ",", "")
                WW_DATATYPE = DataTypeHT("TOLLFEE")
                LNM0006Exceltblrow("TOLLFEE") = LNM0006WRKINC.DataConvert("通行料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車型
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATA))
                WW_DATATYPE = DataTypeHT("SYAGATA")
                LNM0006Exceltblrow("SYAGATA") = LNM0006WRKINC.DataConvert("車型", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車型名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SYAGATANAME))
                WW_DATATYPE = DataTypeHT("SYAGATANAME")
                LNM0006Exceltblrow("SYAGATANAME") = LNM0006WRKINC.DataConvert("車型名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車腹
                WW_TEXT = Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.SYABARA)), ",", "")
                WW_DATATYPE = DataTypeHT("SYABARA")
                LNM0006Exceltblrow("SYABARA") = LNM0006WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考1
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU1))
                WW_DATATYPE = DataTypeHT("BIKOU1")
                LNM0006Exceltblrow("BIKOU1") = LNM0006WRKINC.DataConvert("備考1", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考2
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU2))
                WW_DATATYPE = DataTypeHT("BIKOU2")
                LNM0006Exceltblrow("BIKOU2") = LNM0006WRKINC.DataConvert("備考2", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '備考3
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.BIKOU3))
                WW_DATATYPE = DataTypeHT("BIKOU3")
                LNM0006Exceltblrow("BIKOU3") = LNM0006WRKINC.DataConvert("備考3", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0006WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNM0006Exceltblrow("DELFLG") = LNM0006WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '登録
                LNM0006Exceltbl.Rows.Add(LNM0006Exceltblrow)

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
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKANAME, '')             = @AVOCADOSHUKANAME ")
        SQLStr.AppendLine("    AND  COALESCE(SHUKABASHO, '')             = @SHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(SHUKANAME, '')             = @SHUKANAME ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKENAME, '')             = @AVOCADOTODOKENAME ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
        SQLStr.AppendLine("    AND  COALESCE(TANKNUMBER, '')             = @TANKNUMBER ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        If Not WW_ROW("ENDYMD") = Date.MinValue Then
            SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
        End If
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TANKAKBN, '')             = @TANKAKBN ")
        SQLStr.AppendLine("    AND  COALESCE(MEMO, '')             = @MEMO ")
        SQLStr.AppendLine("    AND  COALESCE(TANKA, '0')             = @TANKA ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTCODE, '0')             = @ACCOUNTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ACCOUNTNAME, '')             = @ACCOUNTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTCODE, '0')             = @SEGMENTCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SEGMENTNAME, '')             = @SEGMENTNAME ")
        SQLStr.AppendLine("    AND  COALESCE(JOTPERCENTAGE, '')             = @JOTPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(ENEXPERCENTAGE, '')             = @ENEXPERCENTAGE ")
        SQLStr.AppendLine("    AND  COALESCE(CALCKBN, '')             = @CALCKBN ")
        SQLStr.AppendLine("    AND  COALESCE(ROUNDTRIP, '0')             = @ROUNDTRIP ")
        SQLStr.AppendLine("    AND  COALESCE(TOLLFEE, '0')             = @TOLLFEE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATANAME, '')             = @SYAGATANAME ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")
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
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOSHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKANAME", MySqlDbType.VarChar, 20)     '実績出荷場所名称
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 6)     '変換後出荷場所コード
                Dim P_SHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAME", MySqlDbType.VarChar, 20)     '変換後出荷場所名称
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_AVOCADOTODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKENAME", MySqlDbType.VarChar, 20)     '実績届先名称
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '変換後届先コード
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '変換後届先名称
                Dim P_TANKNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@TANKNUMBER", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2)     '枝番
                Dim P_TANKAKBN As MySqlParameter = SQLcmd.Parameters.Add("@TANKAKBN", MySqlDbType.VarChar, 1)     '単価区分
                Dim P_MEMO As MySqlParameter = SQLcmd.Parameters.Add("@MEMO", MySqlDbType.VarChar, 50)     '単価用途
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal)         '単価
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_CALCKBN As MySqlParameter = SQLcmd.Parameters.Add("@CALCKBN", MySqlDbType.VarChar, 2)     '計算区分
                Dim P_ROUNDTRIP As MySqlParameter = SQLcmd.Parameters.Add("@ROUNDTRIP", MySqlDbType.Decimal, 5, 3)    '往復距離
                Dim P_TOLLFEE As MySqlParameter = SQLcmd.Parameters.Add("@TOLLFEE", MySqlDbType.Decimal, 8, 3)        '通行料
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
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
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")           '実績出荷場所コード
                P_AVOCADOSHUKANAME.Value = WW_ROW("AVOCADOSHUKANAME")           '実績出荷場所名称
                P_SHUKABASHO.Value = WW_ROW("SHUKABASHO")           '変換後出荷場所コード
                P_SHUKANAME.Value = WW_ROW("SHUKANAME")           '変換後出荷場所名称
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")           '実績届先コード
                P_AVOCADOTODOKENAME.Value = WW_ROW("AVOCADOTODOKENAME")           '実績届先名称
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '変換後届先コード
                P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '変換後届先名称
                P_TANKNUMBER.Value = WW_ROW("TANKNUMBER")           '陸事番号
                P_SHABAN.Value = WW_ROW("SHABAN")           '車番
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE")           '枝番
                P_TANKAKBN.Value = WW_ROW("TANKAKBN")           '単価区分
                P_MEMO.Value = WW_ROW("MEMO")           '単価用途
                P_TANKA.Value = WW_ROW("TANKA")           '単価
                P_ACCOUNTCODE.Value = WW_ROW("ACCOUNTCODE")           '勘定科目コード
                P_ACCOUNTNAME.Value = WW_ROW("ACCOUNTNAME")           '勘定科目名
                P_SEGMENTCODE.Value = WW_ROW("SEGMENTCODE")           'セグメントコード
                P_SEGMENTNAME.Value = WW_ROW("SEGMENTNAME")           'セグメント名
                P_JOTPERCENTAGE.Value = WW_ROW("JOTPERCENTAGE")           '割合JOT
                P_ENEXPERCENTAGE.Value = WW_ROW("ENEXPERCENTAGE")           '割合ENEX
                P_CALCKBN.Value = WW_ROW("CALCKBN")           '計算区分
                P_ROUNDTRIP.Value = WW_ROW("ROUNDTRIP")           '往復距離
                P_TOLLFEE.Value = WW_ROW("TOLLFEE")           '通行料
                P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型
                P_SYAGATANAME.Value = WW_ROW("SYAGATANAME")           '車型名
                P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006_NEWTANKA SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
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
            WW_ROW("KASANORGCODE") = "" OrElse
            WW_ROW("AVOCADOSHUKABASHO") = "" OrElse
            WW_ROW("AVOCADOTODOKECODE") = "" OrElse
            WW_ROW("SHABAN") = "" OrElse
            WW_ROW("STYMD") = Date.MinValue OrElse
            WW_ROW("SYAGATA") = "" Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006_NEWTANKA SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
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
        SQLStr.Append("     LNG.LNM0006_NEWTANKA                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.Append("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.Append("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006L UPDATE"
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
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKANAME  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,TANKNUMBER  ")
        SQLStr.AppendLine("     ,SHABAN  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKAKBN  ")
        SQLStr.AppendLine("     ,MEMO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,CALCKBN  ")
        SQLStr.AppendLine("     ,ROUNDTRIP  ")
        SQLStr.AppendLine("     ,TOLLFEE  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
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
        SQLStr.AppendLine("     ,@AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,@AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,@SHUKABASHO  ")
        SQLStr.AppendLine("     ,@SHUKANAME  ")
        SQLStr.AppendLine("     ,@AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,@AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,@TODOKECODE  ")
        SQLStr.AppendLine("     ,@TODOKENAME  ")
        SQLStr.AppendLine("     ,@TANKNUMBER  ")
        SQLStr.AppendLine("     ,@SHABAN  ")
        SQLStr.AppendLine("     ,@STYMD  ")
        SQLStr.AppendLine("     ,@ENDYMD  ")
        SQLStr.AppendLine("     ,@BRANCHCODE  ")
        SQLStr.AppendLine("     ,@TANKAKBN  ")
        SQLStr.AppendLine("     ,@MEMO  ")
        SQLStr.AppendLine("     ,@TANKA  ")
        SQLStr.AppendLine("     ,@ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,@ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,@SEGMENTCODE  ")
        SQLStr.AppendLine("     ,@SEGMENTNAME  ")
        SQLStr.AppendLine("     ,@JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,@ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,@CALCKBN  ")
        SQLStr.AppendLine("     ,@ROUNDTRIP  ")
        SQLStr.AppendLine("     ,@TOLLFEE  ")
        SQLStr.AppendLine("     ,@SYAGATA  ")
        SQLStr.AppendLine("     ,@SYAGATANAME  ")
        SQLStr.AppendLine("     ,@SYABARA  ")
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
        SQLStr.AppendLine("      TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGCODE =  @ORGCODE")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO =  @AVOCADOSHUKABASHO")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME =  @AVOCADOSHUKANAME")
        SQLStr.AppendLine("     ,SHUKABASHO =  @SHUKABASHO")
        SQLStr.AppendLine("     ,SHUKANAME =  @SHUKANAME")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE =  @AVOCADOTODOKECODE")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME =  @AVOCADOTODOKENAME")
        SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
        SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
        SQLStr.AppendLine("     ,TANKNUMBER =  @TANKNUMBER")
        SQLStr.AppendLine("     ,SHABAN =  @SHABAN")
        SQLStr.AppendLine("     ,STYMD =  @STYMD")
        SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
        SQLStr.AppendLine("     ,BRANCHCODE =  @BRANCHCODE")
        SQLStr.AppendLine("     ,TANKAKBN =  @TANKAKBN")
        SQLStr.AppendLine("     ,MEMO =  @MEMO")
        SQLStr.AppendLine("     ,TANKA =  @TANKA")
        SQLStr.AppendLine("     ,ACCOUNTCODE =  @ACCOUNTCODE")
        SQLStr.AppendLine("     ,ACCOUNTNAME =  @ACCOUNTNAME")
        SQLStr.AppendLine("     ,SEGMENTCODE =  @SEGMENTCODE")
        SQLStr.AppendLine("     ,SEGMENTNAME =  @SEGMENTNAME")
        SQLStr.AppendLine("     ,JOTPERCENTAGE =  @JOTPERCENTAGE")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE =  @ENEXPERCENTAGE")
        SQLStr.AppendLine("     ,CALCKBN =  @CALCKBN")
        SQLStr.AppendLine("     ,ROUNDTRIP =  @ROUNDTRIP")
        SQLStr.AppendLine("     ,TOLLFEE =  @TOLLFEE")
        SQLStr.AppendLine("     ,SYAGATA =  @SYAGATA")
        SQLStr.AppendLine("     ,SYAGATANAME =  @SYAGATANAME")
        SQLStr.AppendLine("     ,SYABARA =  @SYABARA")
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
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOSHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKANAME", MySqlDbType.VarChar, 20)     '実績出荷場所名称
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 6)     '変換後出荷場所コード
                Dim P_SHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAME", MySqlDbType.VarChar, 20)     '変換後出荷場所名称
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_AVOCADOTODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKENAME", MySqlDbType.VarChar, 20)     '実績届先名称
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '変換後届先コード
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '変換後届先名称
                Dim P_TANKNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@TANKNUMBER", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2)     '枝番
                Dim P_TANKAKBN As MySqlParameter = SQLcmd.Parameters.Add("@TANKAKBN", MySqlDbType.VarChar, 1)     '単価区分
                Dim P_MEMO As MySqlParameter = SQLcmd.Parameters.Add("@MEMO", MySqlDbType.VarChar, 50)     '単価用途
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal)         '単価
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_CALCKBN As MySqlParameter = SQLcmd.Parameters.Add("@CALCKBN", MySqlDbType.VarChar, 2)     '計算区分
                Dim P_ROUNDTRIP As MySqlParameter = SQLcmd.Parameters.Add("@ROUNDTRIP", MySqlDbType.Decimal, 5, 3)    '往復距離
                Dim P_TOLLFEE As MySqlParameter = SQLcmd.Parameters.Add("@TOLLFEE", MySqlDbType.Decimal, 8, 3)        '通行料
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
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
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")           '実績出荷場所コード
                P_AVOCADOSHUKANAME.Value = WW_ROW("AVOCADOSHUKANAME")           '実績出荷場所名称
                P_SHUKABASHO.Value = WW_ROW("SHUKABASHO")           '変換後出荷場所コード
                P_SHUKANAME.Value = WW_ROW("SHUKANAME")           '変換後出荷場所名称
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")           '実績届先コード
                P_AVOCADOTODOKENAME.Value = WW_ROW("AVOCADOTODOKENAME")           '実績届先名称
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '変換後届先コード
                P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '変換後届先名称
                P_TANKNUMBER.Value = WW_ROW("TANKNUMBER")           '陸事番号
                P_SHABAN.Value = WW_ROW("SHABAN")           '車番
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                '有効終了日
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    P_ENDYMD.Value = WW_ROW("ENDYMD")
                Else
                    P_ENDYMD.Value = WF_AUTOENDYMD.Value
                End If

                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE")           '枝番
                P_TANKAKBN.Value = WW_ROW("TANKAKBN")           '単価区分
                P_MEMO.Value = WW_ROW("MEMO")           '単価用途

                '単価
                If WW_ROW("TANKA").ToString = "" Then
                    P_TANKA.Value = "0"
                Else
                    P_TANKA.Value = WW_ROW("TANKA")
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

                P_CALCKBN.Value = WW_ROW("CALCKBN")           '計算区分

                '往復距離
                If WW_ROW("ROUNDTRIP").ToString = "0" Or WW_ROW("ROUNDTRIP").ToString = "" Then
                    P_ROUNDTRIP.Value = DBNull.Value
                Else
                    P_ROUNDTRIP.Value = WW_ROW("ROUNDTRIP")
                End If

                '通行料
                If WW_ROW("TOLLFEE").ToString = "0" Or WW_ROW("TOLLFEE").ToString = "" Then
                    P_TOLLFEE.Value = DBNull.Value
                Else
                    P_TOLLFEE.Value = WW_ROW("TOLLFEE")
                End If

                P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型
                Dim WW_SYAGATANAME As String = ""
                CODENAME_get("SYAGATA", WW_ROW("SYAGATA"), WW_SYAGATANAME, WW_RtnSW)
                P_SYAGATANAME.Value = WW_SYAGATANAME            '車型名

                '車腹
                If WW_ROW("SYABARA").ToString = "" Then
                    P_SYABARA.Value = "0"
                Else
                    P_SYABARA.Value = WW_ROW("SYABARA")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006_NEWTANKA  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0006_NEWTANKA  INSERTUPDATE"
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
        ' 部門コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ORGCODE", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            '情シス、高圧ガス以外
            If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                Dim WW_OrgPermitHt As New Hashtable
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    '操作権限のある組織コード一覧を取得
                    work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                    '操作権限のある組織コード一覧に含まれていない場合
                    If WW_OrgPermitHt.ContainsKey(WW_ROW("ORGCODE")) = False And WW_ROW("ORGCODE") <> Master.ROLE_ORG Then
                        WW_CheckMES1 = "・部門コード入力エラーです。"
                        WW_CheckMES2 = "対象の部門コードは登録権限がありません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End Using
            End If
        Else
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
        '実績出荷場所コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOSHUKABASHO", WW_ROW("AVOCADOSHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実績出荷場所コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '実績出荷場所名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOSHUKANAME", WW_ROW("AVOCADOSHUKANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実績出荷場所名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '変換後出荷場所コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHUKABASHO", WW_ROW("SHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・変換後出荷場所コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '変換後出荷場所名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHUKANAME", WW_ROW("SHUKANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・変換後出荷場所名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '実績届先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOTODOKECODE", WW_ROW("AVOCADOTODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実績届先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '実績届先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOTODOKENAME", WW_ROW("AVOCADOTODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実績届先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '変換後届先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TODOKECODE", WW_ROW("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・変換後届先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '変換後届先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TODOKENAME", WW_ROW("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・変換後届先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '陸事番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TANKNUMBER", WW_ROW("TANKNUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・陸事番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '車番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHABAN", WW_ROW("SHABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車番エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 有効開始日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "STYMD", WW_ROW("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            WW_ROW("STYMD") = CDate(WW_ROW("STYMD")).ToString("yyyy/MM/dd")
        Else
            WW_CheckMES1 = "・有効開始日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '単価区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TANKAKBN", WW_ROW("TANKAKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・単価区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '単価用途(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MEMO", WW_ROW("MEMO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・単価用途エラーです。"
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
        '計算区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CALCKBN", WW_ROW("CALCKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・計算区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '往復距離(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ROUNDTRIP", WW_ROW("ROUNDTRIP"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・往復距離エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '通行料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TOLLFEE", WW_ROW("TOLLFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・通行料エラーです。"
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

        '単価マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

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
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006_NEWTANKA SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0005_NEWTANKAHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKANAME  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,TANKNUMBER  ")
        SQLStr.AppendLine("     ,SHABAN  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKAKBN  ")
        SQLStr.AppendLine("     ,MEMO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,CALCKBN  ")
        SQLStr.AppendLine("     ,ROUNDTRIP  ")
        SQLStr.AppendLine("     ,TOLLFEE  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
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
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKANAME  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,TANKNUMBER  ")
        SQLStr.AppendLine("     ,SHABAN  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKAKBN  ")
        SQLStr.AppendLine("     ,MEMO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,CALCKBN  ")
        SQLStr.AppendLine("     ,ROUNDTRIP  ")
        SQLStr.AppendLine("     ,TOLLFEE  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
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
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0005_NEWTANKAHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0005_NEWTANKAHIST  INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 有効終了日更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Sub UpdateENDYMD(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow,
                            ByRef O_MESSAGENO As String, ByVal WW_NOW As String)


        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0006_NEWTANKA                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.Append("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.Append("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try
    End Sub

#End Region
#End Region

End Class


