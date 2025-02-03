''************************************************************
' 固定費マスタメンテナンス・一覧画面
' 作成日 2025/01/20
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/01/20 新規作成
'          : 
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
                    Master.RecoverTable(LNM0007tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0007WRKINC.FILETYPE.EXCEL)
                        'Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                        '    WF_EXCELPDF(LNM0007WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                            DowpDownInitialize()
                            GridViewInitialize()
                            SetTabColor()
                        Case "WF_SelectCALENDARChange", "WF_TORIChange" 'カレンダー変更時、荷主ドロップダウン変更時
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                GridViewInitialize()
                            End Using
                        Case "WF_ButtonKOTEIHI", "WF_ButtonSKKOTEIHI", "WF_ButtonTNGKOTEIHI"
                            Select Case WF_ButtonClick.Value
                                Case "WF_ButtonKOTEIHI"
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDL
                                Case "WF_ButtonSKKOTEIHI"
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDLSK
                                Case "WF_ButtonTNGKOTEIHI"
                                    work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDLTNG
                            End Select
                            WF_TaishoYm.Value = ""
                            DowpDownInitialize()
                            GridViewInitialize()
                            SetTabColor()
                        Case "WF_ButtonDebug"           'デバッグボタン押下
                            WF_ButtonDEBUG_Click()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" And
                        Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" And
                        Not WF_ButtonClick.Value = "WF_TORIChange" And
                        Not WF_ButtonClick.Value = "WF_ButtonKOTEIHI" And
                        Not WF_ButtonClick.Value = "WF_ButtonSKKOTEIHI" And
                        Not WF_ButtonClick.Value = "WF_ButtonTNGKOTEIHI" Then
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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0007D Then
            ' 登録画面からの遷移
            Master.RecoverTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)
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
            work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDL
            Master.CreateXMLSaveFile()
        End If

        'タブ色設定
        SetTabColor()
        '荷主ドロップダウン設定
        DowpDownInitialize()

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0007WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        '東北支店以外の場合
        If LNM0007WRKINC.TohokuCheck(Master.ROLE_ORG) = False Then
            VisibleKeyTohokuOrgCode.Value = ""
        Else
            VisibleKeyTohokuOrgCode.Value = Master.ROLE_ORG
            WF_ButtonKOTEIHI.Text = LNM0007WRKINC.BTNNAMETOHOKU
        End If

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

    End Sub

    ''' <summary>
    ''' タブ色設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetTabColor()
        WF_ButtonKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_INACTIVE)
        WF_ButtonSKKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_INACTIVE)
        WF_ButtonTNGKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_INACTIVE)

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                WF_ButtonKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_ACTIVE)
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                WF_ButtonSKKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_ACTIVE)
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                WF_ButtonTNGKOTEIHI.BackColor = ColorTranslator.FromHtml(CONST_COLOR_TAB_ACTIVE)
        End Select
    End Sub

    ''' <summary>
    ''' 荷主一覧初期設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DowpDownInitialize()
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                retToriList = LNM0007WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0007WRKINC.TBLKOTEIHI)
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                retToriList = LNM0007WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0007WRKINC.TBLSKKOTEIHI)
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                retToriList = LNM0007WRKINC.getDowpDownToriList(Master.ROLE_ORG, LNM0007WRKINC.TBLTNGKOTEIHI)
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
        Master.SaveTable(LNM0007tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0007tbl.Rows.Count.ToString()

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
        SQLStr.AppendLine("   , VIW0002.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , VIW0002.TABLEID                                                          AS TABLEID             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.TORICODE), '')                                    AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.TORINAME), '')                                    AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.ORGCODE), '')                                     AS ORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.ORGNAME), '')                                     AS ORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KASANORGCODE), '')                                AS KASANORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KASANORGNAME), '')                                AS KASANORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(VIW0002.STYMD, '%Y/%m/%d'), '')                     AS STYMD               ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(VIW0002.ENDYMD, '%Y/%m/%d'), '')                    AS ENDYMD              ")
        'SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(VIW0002.TAISHOYM, '%Y/%m'), '')                     AS TAISHOYM               ")
        SQLStr.AppendLine("   , COALESCE(CONCAT(LEFT(VIW0002.TAISHOYM ,4),'/',RIGHT(VIW0002.TAISHOYM,2)) , '')    AS TAISHOYM               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.SYABAN), '')                                      AS SYABAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.RIKUBAN), '')                                     AS RIKUBAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.SYAGATA), '')                                     AS SYAGATA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.SYAGATANAME), '')                                 AS SYAGATANAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.SYABARA), '0')                                    AS SYABARA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.GETSUGAKU), '0')                                   AS GETSUGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.GENGAKU), '0')                                     AS GENGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KOTEIHI), '0')                                     AS KOTEIHI              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KOTEIHIM), '0')                                    AS KOTEIHIM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KOTEIHID), '0')                                    AS KOTEIHID              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KAISU), '0')                                       AS KAISU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.KINGAKU), '0')                                     AS KINGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.BIKOU), '')                                       AS BIKOU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(VIW0002.BIKOU3), '')                                      AS BIKOU3              ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0002_KOTEIHI VIW0002                                                                       ")
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
        SQLStr.AppendLine("      ON  VIW0002.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")

        '対象テーブル
        SQLStr.AppendLine("    VIW0002.TABLEID = @TABLEID                                               ")


        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim dt As DateTime
        Dim Itype As Integer

        '削除フラグ
        'If Not work.WF_SEL_DELFLG_S.Text = "1" Then
        SQLStr.AppendLine(" AND  VIW0002.DELFLG = '0'                                                      ")
        'End If
        '取引先コード
        If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
            SQLStr.AppendLine(" AND  VIW0002.TORICODE = @TORICODE                                          ")
        End If

        '対象年月
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                    SQLStr.AppendLine(" AND  @STYMD BETWEEN VIW0002.STYMD AND VIW0002.ENDYMD  ")
                End If
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    SQLStr.AppendLine(" AND  COALESCE(VIW0002.TAISHOYM, '0') = COALESCE(@TAISHOYM, '0')  ")
                End If
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    SQLStr.AppendLine(" AND  COALESCE(VIW0002.TAISHOYM, '0') = COALESCE(@TAISHOYM, '0')  ")
                End If
        End Select

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     VIW0002.TORICODE                                                           ")
        SQLStr.AppendLine("    ,VIW0002.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,VIW0002.STYMD                                                              ")
        SQLStr.AppendLine("    ,VIW0002.ENDYMD                                                             ")
        SQLStr.AppendLine("    ,VIW0002.TAISHOYM                                                           ")
        SQLStr.AppendLine("    ,VIW0002.SYABAN                                                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                'ロール
                Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                P_ROLE.Value = Master.ROLE_ORG

                '対象テーブル
                Dim P_TABLEID As MySqlParameter = SQLcmd.Parameters.Add("@TABLEID", MySqlDbType.VarChar, 30)
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0007WRKINC.MAPIDL '固定費マスタ
                        P_TABLEID.Value = LNM0007WRKINC.TBLKOTEIHI
                    Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                        P_TABLEID.Value = LNM0007WRKINC.TBLSKKOTEIHI
                    Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                        P_TABLEID.Value = LNM0007WRKINC.TBLTNGKOTEIHI
                End Select

                '取引先コード
                If Not String.IsNullOrEmpty(WF_TORI.SelectedValue) Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                    P_TORICODE.Value = WF_TORI.SelectedValue
                End If

                '対象年月
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0007WRKINC.MAPIDL '固定費マスタ
                        If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                            Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)
                            P_STYMD.Value = dt
                        End If
                    Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                            Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                            P_TAISHOYM.Value = Itype
                        End If
                    Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                            Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                            P_TAISHOYM.Value = Itype
                        End If
                End Select

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

        work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue                         '取引先コード
        work.WF_SEL_TORINAME.Text = WF_TORI.SelectedItem.ToString                 '取引先名称
        work.WF_SEL_ORGCODE.Text = ""                                             '部門コード
        work.WF_SEL_ORGNAME.Text = ""                                             '部門名称
        work.WF_SEL_KASANORGCODE.Text = ""                                        '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = ""                                        '加算先部門名称
        work.WF_SEL_STYMD.Text = ""                                               '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0007WRKINC.MAX_ENDYMD                        '有効終了日
        work.WF_SEL_TAISHOYM.Text = ""                                            '対象年月
        work.WF_SEL_SYABAN.Text = ""                                              '車番
        work.WF_SEL_RIKUBAN.Text = ""                                             '陸事番号
        work.WF_SEL_SYAGATA.Text = ""                                             '車型
        work.WF_SEL_SYAGATANAME.Text = ""                                         '車型名
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SYABARA.Text)   '車腹
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_GETSUGAKU.Text) '月額運賃
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_GENGAKU.Text)   '減額対象額
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHI.Text)   '固定費
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHIM.Text)  '月額固定費
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KOTEIHID.Text)  '日額固定費
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KAISU.Text)     '使用回数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_KINGAKU.Text)   '金額
        work.WF_SEL_BIKOU.Text = ""                                               '備考
        work.WF_SEL_BIKOU1.Text = ""                                              '備考1
        work.WF_SEL_BIKOU2.Text = ""                                              '備考2
        work.WF_SEL_BIKOU3.Text = ""                                              '備考3

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

        Dim WW_DBDataCheck As String = ""

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0007tbl.Rows(WW_LineCNT)("LINECNT")            '選択行

        work.WF_SEL_TORICODE.Text = LNM0007tbl.Rows(WW_LineCNT)("TORICODE")          '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0007tbl.Rows(WW_LineCNT)("TORINAME")          '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("ORGCODE")            '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("ORGNAME")            '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0007tbl.Rows(WW_LineCNT)("KASANORGCODE")  '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0007tbl.Rows(WW_LineCNT)("KASANORGNAME")  '加算先部門名称
        work.WF_SEL_STYMD.Text = LNM0007tbl.Rows(WW_LineCNT)("STYMD")                '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0007tbl.Rows(WW_LineCNT)("ENDYMD")              '有効終了日
        work.WF_SEL_TAISHOYM.Text = LNM0007tbl.Rows(WW_LineCNT)("TAISHOYM")          '対象年月
        work.WF_SEL_SYABAN.Text = LNM0007tbl.Rows(WW_LineCNT)("SYABAN")              '車番
        work.WF_SEL_RIKUBAN.Text = LNM0007tbl.Rows(WW_LineCNT)("RIKUBAN")            '陸事番号
        work.WF_SEL_SYAGATA.Text = LNM0007tbl.Rows(WW_LineCNT)("SYAGATA")            '車型
        work.WF_SEL_SYAGATANAME.Text = LNM0007tbl.Rows(WW_LineCNT)("SYAGATANAME")    '車型名
        work.WF_SEL_SYABARA.Text = LNM0007tbl.Rows(WW_LineCNT)("SYABARA")            '車腹
        work.WF_SEL_GETSUGAKU.Text = LNM0007tbl.Rows(WW_LineCNT)("GETSUGAKU")        '月額運賃
        work.WF_SEL_GENGAKU.Text = LNM0007tbl.Rows(WW_LineCNT)("GENGAKU")            '減額対象額
        work.WF_SEL_KOTEIHI.Text = LNM0007tbl.Rows(WW_LineCNT)("KOTEIHI")            '固定費
        work.WF_SEL_KOTEIHIM.Text = LNM0007tbl.Rows(WW_LineCNT)("KOTEIHIM")          '月額固定費
        work.WF_SEL_KOTEIHID.Text = LNM0007tbl.Rows(WW_LineCNT)("KOTEIHID")          '日額固定費
        work.WF_SEL_KAISU.Text = LNM0007tbl.Rows(WW_LineCNT)("KAISU")                '使用回数
        work.WF_SEL_KINGAKU.Text = LNM0007tbl.Rows(WW_LineCNT)("KINGAKU")            '金額
        work.WF_SEL_BIKOU.Text = LNM0007tbl.Rows(WW_LineCNT)("BIKOU")                '備考
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
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0007WRKINC.MAPIDL '固定費マスタ
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text, work.WF_SEL_SYABAN.Text)
                Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                    work.HaitaCheckSK(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_SYABAN.Text)
                Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                    work.HaitaCheckTNG(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_SYABAN.Text)
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
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = 0
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.INOUTEXCELCOLSK)).Cast(Of Integer)().Max()
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.INOUTEXCELCOLTNG)).Cast(Of Integer)().Max()
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
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        wb.ActiveSheet.Range("B2").Value = "は入力必須"

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                wb.ActiveSheet.Range("C1").Value = "固定費マスタ一覧"
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                wb.ActiveSheet.Range("C1").Value = "SK固定費マスタ一覧"
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                wb.ActiveSheet.Range("C1").Value = "TNG固定費マスタ一覧"
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
            Case LNM0007WRKINC.FILETYPE.EXCEL
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0007WRKINC.MAPIDL '固定費マスタ
                        FileName = "固定費マスタ.xlsx"
                    Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                        FileName = "SK固定費マスタ.xlsx"
                    Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                        FileName = "TNG固定費マスタ.xlsx"
                End Select
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
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番

                '入力不要列網掛け
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.ENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '有効終了日
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '車型名
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLSK.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLSK.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLSK.TAISHOYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '対象年月
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLSK.SYABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番

                '入力不要列網掛け

            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                '入力必須列網掛け
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLTNG.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLTNG.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLTNG.TAISHOYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '対象年月
                sheet.Columns(LNM0007WRKINC.INOUTEXCELCOLTNG.SYABAN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '車番

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
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.STYMD).Value = "（必須）有効開始日"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.ENDYMD).Value = "有効終了日"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Value = "（必須）車番"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN).Value = "陸事番号"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).Value = "車型"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = "車型名"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Value = "車腹"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHI).Value = "固定費"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU1).Value = "備考1"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU2).Value = "備考2"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU3).Value = "備考3"
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.TAISHOYM).Value = "（必須）対象年月"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABAN).Value = "（必須）車番"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABARA).Value = "車腹"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.GETSUGAKU).Value = "月額運賃"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.GENGAKU).Value = "減額対象額"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.KOTEIHI).Value = "固定費"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.BIKOU).Value = "備考"
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG).Value = "（必須）削除フラグ"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORICODE).Value = "（必須）取引先コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORINAME).Value = "取引先名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGCODE).Value = "（必須）部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGNAME).Value = "部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGCODE).Value = "加算先部門コード"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGNAME).Value = "加算先部門名称"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TAISHOYM).Value = "（必須）対象年月"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.SYABAN).Value = "（必須）車番"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHIM).Value = "月額固定費"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHID).Value = "日額固定費"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KAISU).Value = "使用回数"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KINGAKU).Value = "金額"
                sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.BIKOU).Value = "備考"
        End Select

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0007WRKINC.MAPIDL '固定費マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If
                Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG).Comment.Shape
                            .Width = 50
                            .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                        End With
                    End If
                Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                    '削除フラグ
                    COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
                    If Not WW_CNT = 0 Then
                        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG).AddComment(WW_TEXT)
                        With sheet.Cells(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG).Comment.Shape
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
            Case LNM0007WRKINC.MAPIDL '固定費マスタ
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
            Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG)
                    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
                    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
                    End With
                End If
            Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                '削除フラグ
                SETFIXVALUELIST(subsheet, "DELFLG", LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG, WW_FIXENDROW)
                If Not WW_FIXENDROW = -1 Then
                    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG)
                    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG)
                    WW_SUB_STRANGE = subsheet.Cells(0, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG)
                    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG)
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
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNM0007tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0007WRKINC.MAPIDL '固定費マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.STYMD).Value = Row("STYMD") '有効開始日
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.ENDYMD).Value = Row("ENDYMD") '有効終了日
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABAN).Value = Row("SYABAN") '車番
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN).Value = Row("RIKUBAN") '陸事番号
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATA).Value = Row("SYAGATA") '車型
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYAGATANAME).Value = Row("SYAGATANAME") '車型名
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA).Value = Row("SYABARA") '車腹
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHI).Value = Row("KOTEIHI") '固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3
                Case LNM0007WRKINC.MAPIDLSK 'SK固定費マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.TAISHOYM).Value = Row("TAISHOYM") '対象年月
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABAN).Value = Row("SYABAN") '車番
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABARA).Value = Row("SYABARA") '車腹
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.GETSUGAKU).Value = Row("GETSUGAKU") '月額運賃
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.GENGAKU).Value = Row("GENGAKU") '減額対象額
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.KOTEIHI).Value = Row("KOTEIHI") '固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLSK.BIKOU).Value = Row("BIKOU") '備考
                Case LNM0007WRKINC.MAPIDLTNG 'TNG固定費マスタ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG).Value = Row("DELFLG") '削除フラグ
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORICODE).Value = Row("TORICODE") '取引先コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORINAME).Value = Row("TORINAME") '取引先名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGCODE).Value = Row("ORGCODE") '部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGNAME).Value = Row("ORGNAME") '部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TAISHOYM).Value = Row("TAISHOYM") '対象年月
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.SYABAN).Value = Row("SYABAN") '車番
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHIM).Value = Row("KOTEIHIM") '月額固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHID).Value = Row("KOTEIHID") '日額固定費
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KAISU).Value = Row("KAISU") '使用回数
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KINGAKU).Value = Row("KINGAKU") '金額
                    sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.INOUTEXCELCOLTNG.BIKOU).Value = Row("BIKOU") '備考
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
            Dim WW_PASTSTYMD As String = "" '過去有効開始日格納
            Dim WW_PASTENDYMD As String = "" '過去有効終了日格納

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


                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                            '有効開始日、有効終了日更新
                            If Not Row("TORICODE") = "" And
                               Not Row("ORGCODE") = "" And
                               Not Row("STYMD") = Date.MinValue And
                               Not Row("SYABAN") = "" Then


                                '更新前の最大有効開始日取得
                                WW_BeforeMAXSTYMD = LNM0007WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                                If Not isNormal(WW_DBDataCheck) Then
                                    Exit Sub
                                End If

                                Select Case True
                                    Case WW_BeforeMAXSTYMD = "" '無い場合
                                        Row("ENDYMD") = LNM0007WRKINC.MAX_ENDYMD
                                    Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd") '同一の場合
                                        Row("ENDYMD") = LNM0007WRKINC.MAX_ENDYMD
                                '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                                    Case WW_BeforeMAXSTYMD < CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                        'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする

                                        '変更後の有効開始日退避
                                        WW_STYMD_SAVE = Row("STYMD")
                                        '変更後テーブルに変更前の有効開始日格納
                                        Row("STYMD") = WW_BeforeMAXSTYMD
                                        '変更後テーブルに更新用の有効終了日格納
                                        Row("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                                        '履歴テーブルに変更前データを登録
                                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                            Exit Sub
                                        End If
                                        '変更前の有効終了日更新
                                        UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                        If Not isNormal(WW_DBDataCheck) Then
                                            Exit Sub
                                        End If
                                        '履歴テーブルに変更後データを登録
                                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                            Exit Sub
                                        End If
                                        '退避した有効開始日を元に戻す
                                        Row("STYMD") = WW_STYMD_SAVE
                                        '有効終了日に最大値を入れる
                                        Row("ENDYMD") = LNM0007WRKINC.MAX_ENDYMD
                                    Case Else
                                        '有効終了日に有効開始日の月の末日を入れる
                                        Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                        Row("ENDYMD") = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                                End Select
                            End If
#End Region
                        Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
#End Region
                        Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "単価マスタの更新権限がありません")
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
            Dim WW_PASTSTYMD As String = "" '過去有効開始日格納
            Dim WW_PASTENDYMD As String = "" '過去有効終了日格納

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

                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                            '有効開始日、有効終了日更新
                            If Not Row("TORICODE") = "" And
                               Not Row("ORGCODE") = "" And
                               Not Row("STYMD") = Date.MinValue And
                               Not Row("SYABAN") = "" Then


                                '更新前の最大有効開始日取得
                                WW_BeforeMAXSTYMD = LNM0007WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                                If Not isNormal(WW_DBDataCheck) Then
                                    Exit Sub
                                End If

                                Select Case True
                                    'DBに登録されている有効開始日が無かった場合
                                    Case WW_BeforeMAXSTYMD = ""
                                        Row("ENDYMD") = LNM0007WRKINC.MAX_ENDYMD
                                        '同一の場合
                                    Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                    '何もしない
                                '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                                    Case WW_BeforeMAXSTYMD < CDate(Row("STYMD")).ToString("yyyy/MM/dd")
                                        'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする

                                        '変更後の有効開始日退避
                                        WW_STYMD_SAVE = Row("STYMD")
                                        '変更後テーブルに変更前の有効開始日格納
                                        Row("STYMD") = WW_BeforeMAXSTYMD
                                        '変更後テーブルに更新用の有効終了日格納
                                        Row("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                                        '履歴テーブルに変更前データを登録
                                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                            Exit Sub
                                        End If
                                        '変更前の有効終了日更新
                                        UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                        If Not isNormal(WW_DBDataCheck) Then
                                            Exit Sub
                                        End If
                                        '履歴テーブルに変更後データを登録
                                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0007WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                            Exit Sub
                                        End If
                                        '退避した有効開始日を元に戻す
                                        Row("STYMD") = WW_STYMD_SAVE
                                        '有効終了日に最大値を入れる
                                        Row("ENDYMD") = LNM0007WRKINC.MAX_ENDYMD
                                    Case Else
                                        '有効終了日に有効開始日の月の末日を入れる
                                        Dim WW_NEXT_YM As String = DateTime.Parse(Row("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                        Row("ENDYMD") = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                                End Select
                            End If
#End Region
                        Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
#End Region
                        Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
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

        'アップロードしたExcelファイルがどのテーブルのデータか確認する
        Dim WW_HEADERROW As Integer = 2
        Select Case True
            '陸事番号のヘッダが含まれている場合は固定費マスタ
            Case WW_EXCELDATA(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOL.RIKUBAN).IndexOf("陸事番号") >= 0
                work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDL
                '月額運賃のヘッダが含まれている場合はSK固定費マスタ
            Case WW_EXCELDATA(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLSK.GETSUGAKU).IndexOf("月額運賃") >= 0
                work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDLSK
                '月額固定費のヘッダが含まれている場合はTNG固定費マスタ
            Case WW_EXCELDATA(WW_HEADERROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHIM).IndexOf("月額固定費") >= 0
                work.WF_SEL_CONTROLTABLE.Text = LNM0007WRKINC.MAPIDLTNG
        End Select

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
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
                SQLStr.AppendLine("        ,RIKUBAN  ")
                SQLStr.AppendLine("        ,SYAGATA  ")
                SQLStr.AppendLine("        ,SYAGATANAME  ")
                SQLStr.AppendLine("        ,SYABARA  ")
                SQLStr.AppendLine("        ,KOTEIHI  ")
                SQLStr.AppendLine("        ,BIKOU1  ")
                SQLStr.AppendLine("        ,BIKOU2  ")
                SQLStr.AppendLine("        ,BIKOU3  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0007_KOTEIHI ")
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_KOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
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
                SQLStr.AppendLine("        ,TAISHOYM  ")
                SQLStr.AppendLine("        ,SYABAN  ")
                SQLStr.AppendLine("        ,SYABARA  ")
                SQLStr.AppendLine("        ,GETSUGAKU  ")
                SQLStr.AppendLine("        ,GENGAKU  ")
                SQLStr.AppendLine("        ,KOTEIHI  ")
                SQLStr.AppendLine("        ,BIKOU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0008_SKKOTEIHI ")
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_SKKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0008_SKKOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
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
                SQLStr.AppendLine("        ,TAISHOYM  ")
                SQLStr.AppendLine("        ,SYABAN  ")
                SQLStr.AppendLine("        ,KOTEIHIM  ")
                SQLStr.AppendLine("        ,KOTEIHID  ")
                SQLStr.AppendLine("        ,KAISU  ")
                SQLStr.AppendLine("        ,KINGAKU  ")
                SQLStr.AppendLine("        ,BIKOU  ")
                SQLStr.AppendLine("        ,DELFLG  ")
                SQLStr.AppendLine(" FROM LNG.LNM0009_TNGKOTEIHI ")
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_TNGKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0009_TNGKOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
#End Region
        End Select

        Dim LNM0007Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
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
                    '有効開始日
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.STYMD))
                    WW_DATATYPE = DataTypeHT("STYMD")
                    LNM0007Exceltblrow("STYMD") = LNM0007WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    ''有効終了日
                    'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.ENDYMD))
                    'WW_DATATYPE = DataTypeHT("ENDYMD")
                    'LNM0007Exceltblrow("ENDYMD") = LNM0007WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    'If WW_RESULT = False Then
                    '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    '    O_RTN = "ERR"
                    'End If
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
                    ''車腹
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.SYABARA))
                    WW_DATATYPE = DataTypeHT("SYABARA")
                    LNM0007Exceltblrow("SYABARA") = LNM0007WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOL.KOTEIHI))
                    WW_DATATYPE = DataTypeHT("KOTEIHI")
                    LNM0007Exceltblrow("KOTEIHI") = LNM0007WRKINC.DataConvert("固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
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
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0007Exceltblrow = LNM0007Exceltbl.NewRow

                    'LINECNT
                    LNM0007Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0007Exceltblrow("TORICODE") = LNM0007WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0007Exceltblrow("TORINAME") = LNM0007WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0007Exceltblrow("ORGCODE") = LNM0007WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0007Exceltblrow("ORGNAME") = LNM0007WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0007Exceltblrow("KASANORGCODE") = LNM0007WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0007Exceltblrow("KASANORGNAME") = LNM0007WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '対象年月
                    WW_TEXT = Replace(Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.TAISHOYM)), "/", ""), "／", "")
                    WW_DATATYPE = DataTypeHT("TAISHOYM")
                    LNM0007Exceltblrow("TAISHOYM") = LNM0007WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車番
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABAN))
                    WW_DATATYPE = DataTypeHT("SYABAN")
                    LNM0007Exceltblrow("SYABAN") = LNM0007WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車腹
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.SYABARA))
                    WW_DATATYPE = DataTypeHT("SYABARA")
                    LNM0007Exceltblrow("SYABARA") = LNM0007WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '月額運賃
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.GETSUGAKU))
                    WW_DATATYPE = DataTypeHT("GETSUGAKU")
                    LNM0007Exceltblrow("GETSUGAKU") = LNM0007WRKINC.DataConvert("月額運賃", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '減額対象額
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.GENGAKU))
                    WW_DATATYPE = DataTypeHT("GENGAKU")
                    LNM0007Exceltblrow("GENGAKU") = LNM0007WRKINC.DataConvert("減額対象額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.KOTEIHI))
                    WW_DATATYPE = DataTypeHT("KOTEIHI")
                    LNM0007Exceltblrow("KOTEIHI") = LNM0007WRKINC.DataConvert("固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.BIKOU))
                    WW_DATATYPE = DataTypeHT("BIKOU")
                    LNM0007Exceltblrow("BIKOU") = LNM0007WRKINC.DataConvert("備考", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLSK.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0007Exceltblrow("DELFLG") = LNM0007WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0007Exceltbl.Rows.Add(LNM0007Exceltblrow)

                Next
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                    LNM0007Exceltblrow = LNM0007Exceltbl.NewRow

                    'LINECNT
                    LNM0007Exceltblrow("LINECNT") = WW_LINECNT
                    WW_LINECNT = WW_LINECNT + 1

                    '◆データセット
                    '取引先コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORICODE))
                    WW_DATATYPE = DataTypeHT("TORICODE")
                    LNM0007Exceltblrow("TORICODE") = LNM0007WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '取引先名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TORINAME))
                    WW_DATATYPE = DataTypeHT("TORINAME")
                    LNM0007Exceltblrow("TORINAME") = LNM0007WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGCODE))
                    WW_DATATYPE = DataTypeHT("ORGCODE")
                    LNM0007Exceltblrow("ORGCODE") = LNM0007WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.ORGNAME))
                    WW_DATATYPE = DataTypeHT("ORGNAME")
                    LNM0007Exceltblrow("ORGNAME") = LNM0007WRKINC.DataConvert("部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門コード
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGCODE))
                    WW_DATATYPE = DataTypeHT("KASANORGCODE")
                    LNM0007Exceltblrow("KASANORGCODE") = LNM0007WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '加算先部門名称
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KASANORGNAME))
                    WW_DATATYPE = DataTypeHT("KASANORGNAME")
                    LNM0007Exceltblrow("KASANORGNAME") = LNM0007WRKINC.DataConvert("加算先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '対象年月
                    WW_TEXT = Replace(Replace(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.TAISHOYM)), "/", ""), "／", "")
                    WW_DATATYPE = DataTypeHT("TAISHOYM")
                    LNM0007Exceltblrow("TAISHOYM") = LNM0007WRKINC.DataConvert("対象年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '車番
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.SYABAN))
                    WW_DATATYPE = DataTypeHT("SYABAN")
                    LNM0007Exceltblrow("SYABAN") = LNM0007WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '月額固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHIM))
                    WW_DATATYPE = DataTypeHT("KOTEIHIM")
                    LNM0007Exceltblrow("KOTEIHIM") = LNM0007WRKINC.DataConvert("月額固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '日額固定費
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KOTEIHID))
                    WW_DATATYPE = DataTypeHT("KOTEIHID")
                    LNM0007Exceltblrow("KOTEIHID") = LNM0007WRKINC.DataConvert("日額固定費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '使用回数
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KAISU))
                    WW_DATATYPE = DataTypeHT("KAISU")
                    LNM0007Exceltblrow("KAISU") = LNM0007WRKINC.DataConvert("使用回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '金額
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.KINGAKU))
                    WW_DATATYPE = DataTypeHT("KINGAKU")
                    LNM0007Exceltblrow("KINGAKU") = LNM0007WRKINC.DataConvert("金額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                    '備考
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.BIKOU))
                    WW_DATATYPE = DataTypeHT("BIKOU")
                    LNM0007Exceltblrow("BIKOU") = LNM0007WRKINC.DataConvert("備考", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '削除フラグ
                    WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0007WRKINC.INOUTEXCELCOLTNG.DELFLG))
                    WW_DATATYPE = DataTypeHT("DELFLG")
                    LNM0007Exceltblrow("DELFLG") = LNM0007WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                    If WW_RESULT = False Then
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If

                    '登録
                    LNM0007Exceltbl.Rows.Add(LNM0007Exceltblrow)

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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0007_KOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
                SQLStr.AppendLine("    AND  COALESCE(RIKUBAN, '')             = @RIKUBAN ")
                SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
                SQLStr.AppendLine("    AND  COALESCE(SYABARA, '')             = @SYABARA ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHI, '0')             = @KOTEIHI ")
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
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_RIKUBAN As MySqlParameter = SQLcmd.Parameters.Add("@RIKUBAN", MySqlDbType.VarChar, 20)     '陸事番号
                        Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                        Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal)     '固定費
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
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_RIKUBAN.Value = WW_ROW("RIKUBAN")           '陸事番号
                        P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型
                        P_SYAGATANAME.Value = WW_ROW("SYAGATANAME")           '車型名
                        P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_KOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0008_SKKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
                SQLStr.AppendLine("    AND  COALESCE(SYABARA, '')             = @SYABARA ")
                SQLStr.AppendLine("    AND  COALESCE(GETSUGAKU, '0')             = @GETSUGAKU ")
                SQLStr.AppendLine("    AND  COALESCE(GENGAKU, '0')             = @GENGAKU ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHI, '0')             = @KOTEIHI ")
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
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_GETSUGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GETSUGAKU", MySqlDbType.Decimal)     '月額運賃
                        Dim P_GENGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GENGAKU", MySqlDbType.Decimal)     '減額対象額
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal)     '固定費
                        Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考

                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                        P_GETSUGAKU.Value = WW_ROW("GETSUGAKU")           '月額運賃
                        P_GENGAKU.Value = WW_ROW("GENGAKU")           '減額対象額
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_SKKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0008_SKKOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    Exit Function
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0009_TNGKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHIM, '0')             = @KOTEIHIM ")
                SQLStr.AppendLine("    AND  COALESCE(KOTEIHID, '0')             = @KOTEIHID ")
                SQLStr.AppendLine("    AND  COALESCE(KAISU, '0')             = @KAISU ")
                SQLStr.AppendLine("    AND  COALESCE(KINGAKU, '0')             = @KINGAKU ")
                SQLStr.AppendLine("    AND  COALESCE(BIKOU, '')             = @BIKOU ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_KOTEIHIM As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHIM", MySqlDbType.Decimal)     '月額固定費
                        Dim P_KOTEIHID As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHID", MySqlDbType.Decimal)     '日額固定費
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal)     '使用回数
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal)     '金額
                        Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考

                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_KOTEIHIM.Value = WW_ROW("KOTEIHIM")           '月額固定費
                        P_KOTEIHID.Value = WW_ROW("KOTEIHID")           '日額固定費
                        P_KAISU.Value = WW_ROW("KAISU")           '使用回数
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_TNGKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0009_TNGKOTEIHI SELECT"
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
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
                SQLStr.AppendLine("        LNG.LNM0007_KOTEIHI")
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_KOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("TAISHOYM") = "0" OrElse
                    WW_ROW("SYABAN") = "" Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0008_SKKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_SKKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0008_SKKOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Exit Function
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                '一意キーが未入力の場合処理を終了する
                If WW_ROW("TORICODE") = "" OrElse
                    WW_ROW("ORGCODE") = "" OrElse
                    WW_ROW("TAISHOYM") = "0" OrElse
                    WW_ROW("SYABAN") = "" Then
                    Exit Function
                End If

                '更新前の削除フラグを取得
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0009_TNGKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_TNGKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0009_TNGKOTEIHI SELECT"
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0007_KOTEIHI                     ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
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
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0008_SKKOTEIHI                   ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007L UPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                '○ 対象データ取得
                Dim SQLStr As New StringBuilder
                SQLStr.Append(" UPDATE                                      ")
                SQLStr.Append("     LNG.LNM0009_TNGKOTEIHI                  ")
                SQLStr.Append(" SET                                         ")
                SQLStr.Append("     DELFLG               = '1'              ")
                SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
                SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
                SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
                SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
                SQLStr.Append(" WHERE                                       ")
                SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007L UPDATE"
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0007_KOTEIHI")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,RIKUBAN  ")
                SQLStr.AppendLine("     ,SYAGATA  ")
                SQLStr.AppendLine("     ,SYAGATANAME  ")
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
                SQLStr.AppendLine("      @TORICODE  ")
                SQLStr.AppendLine("     ,@TORINAME  ")
                SQLStr.AppendLine("     ,@ORGCODE  ")
                SQLStr.AppendLine("     ,@ORGNAME  ")
                SQLStr.AppendLine("     ,@KASANORGCODE  ")
                SQLStr.AppendLine("     ,@KASANORGNAME  ")
                SQLStr.AppendLine("     ,@STYMD  ")
                SQLStr.AppendLine("     ,@ENDYMD  ")
                SQLStr.AppendLine("     ,@SYABAN  ")
                SQLStr.AppendLine("     ,@RIKUBAN  ")
                SQLStr.AppendLine("     ,@SYAGATA  ")
                SQLStr.AppendLine("     ,@SYAGATANAME  ")
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
                SQLStr.AppendLine("      TORINAME =  @TORINAME")
                SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
                SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
                SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
                SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
                SQLStr.AppendLine("     ,RIKUBAN =  @RIKUBAN")
                SQLStr.AppendLine("     ,SYAGATA =  @SYAGATA")
                SQLStr.AppendLine("     ,SYAGATANAME =  @SYAGATANAME")
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
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_RIKUBAN As MySqlParameter = SQLcmd.Parameters.Add("@RIKUBAN", MySqlDbType.VarChar, 20)     '陸事番号
                        Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                        Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal)     '固定費
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
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD")           '有効終了日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_RIKUBAN.Value = WW_ROW("RIKUBAN")           '陸事番号
                        P_SYAGATA.Value = WW_ROW("SYAGATA")           '車型
                        '車型名
                        Dim WW_SYAGATANAME As String = ""
                        CODENAME_get("SYAGATA", WW_ROW("SYAGATA"), WW_SYAGATANAME, WW_RtnSW)
                        P_SYAGATANAME.Value = WW_SYAGATANAME

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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_KOTEIHI  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0007_KOTEIHI  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0008_SKKOTEIHI")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,GETSUGAKU  ")
                SQLStr.AppendLine("     ,GENGAKU  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
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
                SQLStr.AppendLine("     ,@TAISHOYM  ")
                SQLStr.AppendLine("     ,@SYABAN  ")
                SQLStr.AppendLine("     ,@SYABARA  ")
                SQLStr.AppendLine("     ,@GETSUGAKU  ")
                SQLStr.AppendLine("     ,@GENGAKU  ")
                SQLStr.AppendLine("     ,@KOTEIHI  ")
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
                SQLStr.AppendLine("     ,SYABARA =  @SYABARA")
                SQLStr.AppendLine("     ,GETSUGAKU =  @GETSUGAKU")
                SQLStr.AppendLine("     ,GENGAKU =  @GENGAKU")
                SQLStr.AppendLine("     ,KOTEIHI =  @KOTEIHI")
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
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                        Dim P_GETSUGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GETSUGAKU", MySqlDbType.Decimal)     '月額運賃
                        Dim P_GENGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GENGAKU", MySqlDbType.Decimal)     '減額対象額
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal)     '固定費
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
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_SYABARA.Value = WW_ROW("SYABARA")           '車腹
                        P_GETSUGAKU.Value = WW_ROW("GETSUGAKU")           '月額運賃
                        P_GENGAKU.Value = WW_ROW("GENGAKU")           '減額対象額
                        P_KOTEIHI.Value = WW_ROW("KOTEIHI")           '固定費
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_SKKOTEIHI  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0008_SKKOTEIHI  INSERTUPDATE"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("  INSERT INTO LNG.LNM0009_TNGKOTEIHI")
                SQLStr.AppendLine("   (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHIM  ")
                SQLStr.AppendLine("     ,KOTEIHID  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
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
                SQLStr.AppendLine("     ,@TAISHOYM  ")
                SQLStr.AppendLine("     ,@SYABAN  ")
                SQLStr.AppendLine("     ,@KOTEIHIM  ")
                SQLStr.AppendLine("     ,@KOTEIHID  ")
                SQLStr.AppendLine("     ,@KAISU  ")
                SQLStr.AppendLine("     ,@KINGAKU  ")
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
                SQLStr.AppendLine("     ,KOTEIHIM =  @KOTEIHIM")
                SQLStr.AppendLine("     ,KOTEIHID =  @KOTEIHID")
                SQLStr.AppendLine("     ,KAISU =  @KAISU")
                SQLStr.AppendLine("     ,KINGAKU =  @KINGAKU")
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
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_KOTEIHIM As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHIM", MySqlDbType.Decimal)     '月額固定費
                        Dim P_KOTEIHID As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHID", MySqlDbType.Decimal)     '日額固定費
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal)     '使用回数
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal)     '金額
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
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                        P_KOTEIHIM.Value = WW_ROW("KOTEIHIM")           '月額固定費
                        P_KOTEIHID.Value = WW_ROW("KOTEIHID")           '日額固定費
                        P_KAISU.Value = WW_ROW("KAISU")           '使用回数
                        P_KINGAKU.Value = WW_ROW("KINGAKU")           '金額
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_TNGKOTEIHI  INSERTUPDATE")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNM0009_TNGKOTEIHI  INSERTUPDATE"
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
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
                '有効開始日(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "STYMD", WW_ROW("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・有効開始日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ''有効終了日(バリデーションチェック)
                'Master.CheckField(Master.USERCAMP, "ENDYMD", WW_ROW("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                'If Not isNormal(WW_CS0024FCheckerr) Then
                '    WW_CheckMES1 = "・有効終了日エラーです。"
                '    WW_CheckMES2 = WW_CS0024FCheckReport
                '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
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
                '固定費(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KOTEIHI", WW_ROW("KOTEIHI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・固定費エラーです。"
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
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
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
                Master.CheckField(Master.USERCAMP, "TAISHOYM", WW_ROW("TAISHOYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
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
                '車腹(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "SYABARA", WW_ROW("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・車腹エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '月額運賃(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "GETSUGAKU", WW_ROW("GETSUGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・月額運賃エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '減額対象額(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "GENGAKU", WW_ROW("GENGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・減額対象額エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '固定費(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KOTEIHI", WW_ROW("KOTEIHI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・固定費エラーです。"
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
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
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
                Master.CheckField(Master.USERCAMP, "TAISHOYM", WW_ROW("TAISHOYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
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
                '月額固定費(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KOTEIHIM", WW_ROW("KOTEIHIM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・月額固定費エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '日額固定費(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KOTEIHID", WW_ROW("KOTEIHID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・日額固定費エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '使用回数(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KAISU", WW_ROW("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・使用回数エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '金額(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "KINGAKU", WW_ROW("KINGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・金額エラーです。"
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
#End Region
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                '固定費マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0007_KOTEIHI")
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
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_KOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                'SK固定費マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0008_SKKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_SKKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0008_SKKOTEIHI SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                'TNG固定費マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("       ,DELFLG")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0009_TNGKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
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
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                                WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                            Else
                                WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_TNGKOTEIHI SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0009_TNGKOTEIHI SELECT"
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
            Case LNM0007WRKINC.MAPIDL
#Region "固定費マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0006_KOTEIHIHIST ")
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
                SQLStr.AppendLine("     ,RIKUBAN  ")
                SQLStr.AppendLine("     ,SYAGATA  ")
                SQLStr.AppendLine("     ,SYAGATANAME  ")
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
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,RIKUBAN  ")
                SQLStr.AppendLine("     ,SYAGATA  ")
                SQLStr.AppendLine("     ,SYAGATANAME  ")
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
                SQLStr.AppendLine("        LNG.LNM0007_KOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("   AND SYABAN  = @SYABAN            ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
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
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_KOTEIHIHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0006_KOTEIHIHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLSK
#Region "SK固定費マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0007_SKKOTEIHIHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,GETSUGAKU  ")
                SQLStr.AppendLine("     ,GENGAKU  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
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
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,GETSUGAKU  ")
                SQLStr.AppendLine("     ,GENGAKU  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
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
                SQLStr.AppendLine("        LNG.LNM0008_SKKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("   AND SYABAN  = @SYABAN            ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
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
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0007_SKKOTEIHIHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0007_SKKOTEIHIHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0007WRKINC.MAPIDLTNG
#Region "TNG固定費マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0008_TNGKOTEIHIHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHIM  ")
                SQLStr.AppendLine("     ,KOTEIHID  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
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
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHIM  ")
                SQLStr.AppendLine("     ,KOTEIHID  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
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
                SQLStr.AppendLine("        LNG.LNM0009_TNGKOTEIHI")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
                SQLStr.AppendLine("   AND SYABAN  = @SYABAN            ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)    '対象年月
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
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番

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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0008_TNGKOTEIHIHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0008_TNGKOTEIHIHIST INSERT"
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
        SQLStr.Append("     LNG.LNM0007_KOTEIHI                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("       TORICODE  = @TORICODE                 ")
        SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("   AND SYABAN  = @SYABAN             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                'P_ENDYMD.Value = DateTime.Parse(WW_NEWSTYMD).AddDays(-1).ToString("yyyy/MM/dd") '有効終了日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
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
#End Region

End Class


