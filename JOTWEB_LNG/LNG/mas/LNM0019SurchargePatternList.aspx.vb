''************************************************************
' サーチャージ定義マスタメンテナンス・一覧画面
' 作成日 2025/08/08
' 更新日 
' 作成者 三宅
' 更新者 
'
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' サーチャージ定義マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0019SurchargePatternList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0019tbl As DataTable         '一覧格納用テーブル
    Private LNM0019UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNM0019Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    Private Const CONST_BTNFEE As String = "<div><input class=""btn-sticky"" id=""btnFee""　type=""button"" value=""料金設定"" readonly onclick=""BtnFeeClick();"" /></div>"
    Private Const CONST_BTNFEE_DEL As String = ""
    Private Const CONST_BTNTANKA As String = "<div><input class=""btn-sticky"" id=""btnTanka""　type=""button"" value=""実勢単価"" readonly onclick=""BtnTankaClick();"" /></div>"
    Private Const CONST_BTNTANKA_DEL As String = ""

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
                    Master.RecoverTable(LNM0019tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            InputSave()
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            InputSave()
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0019WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0019WRKINC.FILETYPE.PDF)
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
                        Case "WF_ButtonExtract"         '検索ボタン押下時
                            GridViewInitialize()
                        Case "WF_StYMDChange"           '対象日チェンジ
                            GridViewInitialize()
                        Case "WF_TORIChange"            '荷主チェンジ
                            Me.WF_ORG.Items.Clear()
                            Dim retOrgList As New DropDownList
                            retOrgList = LNM0019WRKINC.getDowpDownOrgList(Master.MAPID, WF_TORI.SelectedValue, Master.ROLE_ORG)
                            For index As Integer = 0 To retOrgList.Items.Count - 1
                                WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                            Next
                        Case "WF_ButtonFeeClick"        '料金設定
                            InputSave()
                            SurchargeFeeSave()
                        Case "WF_ButtonTankaClick"      '実勢単価
                            InputSave()
                            DieselPriceSiteSave()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" And
                        Not WF_ButtonClick.Value = "WF_StYMDChange" And
                        Not WF_ButtonClick.Value = "WF_ButtonExtract" Then
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
            If Not IsNothing(LNM0019tbl) Then
                LNM0019tbl.Clear()
                LNM0019tbl.Dispose()
                LNM0019tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0019WRKINC.MAPIDL
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
            Case C_PREV_MAP_LIST.LNM0019D, C_PREV_MAP_LIST.LNM0019H
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
        retToriList = LNM0019WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0019WRKINC.getDowpDownOrgList(Master.MAPID, "", Master.ROLE_ORG)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        Select Case Context.Handler.ToString().ToUpper()
            '○ 登録・履歴画面からの遷移
            Case C_PREV_MAP_LIST.LNM0019D, C_PREV_MAP_LIST.LNM0019H, C_PREV_MAP_LIST.LNT0031L
                Master.RecoverTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)
                '対象日
                Dim WW_YMD As String = Replace(work.WF_SEL_TARGETYMD_L.Text, "/", "")
                WF_StYMD.Value = WW_YMD.Substring(0, 4) & "/" & WW_YMD.Substring(4, 2) & "/" & WW_YMD.Substring(6, 2)
                '荷主
                WF_TORI.SelectedValue = work.WF_SEL_TORI_L.Text
                '部門
                WF_ORG.SelectedValue = work.WF_SEL_ORG_L.Text
                '削除済みデータ表示状態
                ChkDelDataFlg.Checked = work.WF_SEL_CHKDELDATAFLG_L.Text
                '○ MENUからの遷移
            Case Else
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
                Master.CreateXMLSaveFile()
                WF_StYMD.Value = Date.Now.ToString("yyyy/MM/dd")
        End Select

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0019WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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
        Master.SaveTable(LNM0019tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0019tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0019tbl)
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
        Master.SaveTable(LNM0019tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0019tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim WW_GridPosition As Integer = 1         '表示位置(開始)

        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0019row As DataRow In LNM0019tbl.Rows
            If LNM0019row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0019row("SELECT") = WW_DataCNT
            End If
        Next

        Dim TBLview As DataView = New DataView(LNM0019tbl)
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

        If IsNothing(LNM0019tbl) Then
            LNM0019tbl = New DataTable
        End If

        If LNM0019tbl.Columns.Count <> 0 Then
            LNM0019tbl.Columns.Clear()
        End If

        LNM0019tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをサーチャージ定義マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                      AS 'SELECT'              ")
        SQLStr.AppendLine("   , 0                                                                      AS HIDDEN                ")
        SQLStr.AppendLine("   , 0                                                                      AS LINECNT               ")
        SQLStr.AppendLine("   , ''                                                                     AS OPERATION             ")
        SQLStr.AppendLine("   , ''                                                                     AS SURCHARGEFEE          ")
        SQLStr.AppendLine("   , ''                                                                     AS TANKA                 ")
        SQLStr.AppendLine("   , LNM0019.UPDTIMSTP                                                      AS UPDTIMSTP             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.TORICODE), '')                                  AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.TORINAME), '')                                  AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.ORGCODE), '')                                   AS ORGCODE               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.ORGNAME), '')                                   AS ORGNAME               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.KASANORGCODE), '')                              AS KASANORGCODE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.KASANORGNAME), '')                              AS KASANORGNAME          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.SURCHARGEPATTERNCODE), '')                      AS SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.SURCHARGEPATTERNNAME), '')                      AS SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.BILLINGCYCLE), '')                              AS BILLINGCYCLE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNS00062.VALUE1), '')                                   AS BILLINGCYCLENAME      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.CALCMETHOD), '')                                AS CALCMETHOD            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNS00063.VALUE1), '')                                   AS CALCMETHODNAME        ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNM0019.STYMD, '%Y/%m/%d'), '')                   AS STYMD                 ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNM0019.ENDYMD, '%Y/%m/%d'), '')                  AS ENDYMD                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.DIESELPRICESITEID), '')                         AS DIESELPRICESITEID     ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DIESELPRICESITENAME), '')                       AS DIESELPRICESITENAME   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.DIESELPRICESITEBRANCH), '')                     AS DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DIESELPRICESITEKBNNAME), '')                    AS DIESELPRICESITEKBNNAME")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DISPLAYNAME), '')                               AS DISPLAYNAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0019.DELFLG), '')                                    AS DELFLG                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNS00061.VALUE1), '')                                   AS DELFLGNAME            ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0019_SURCHARGEPATTERN LNM0019                                                            ")

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
        SQLStr.AppendLine("      ON  LNM0019.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          KEYCODE                                                                                    ")
        SQLStr.AppendLine("         ,VALUE1                                                                                     ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0006_FIXVALUE                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          CAMPCODE = @CAMPCODE                                                                       ")
        SQLStr.AppendLine("      AND CLASS = 'DELFLG'                                                                           ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS00061                                                                                       ")
        SQLStr.AppendLine("      ON  LNM0019.DELFLG = LNS00061.KEYCODE                                                          ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          KEYCODE                                                                                    ")
        SQLStr.AppendLine("         ,VALUE1                                                                                     ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0006_FIXVALUE                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          CAMPCODE = @CAMPCODE                                                                       ")
        SQLStr.AppendLine("      AND CLASS = 'BILLINGCYCLE'                                                                     ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS00062                                                                                       ")
        SQLStr.AppendLine("      ON  LNM0019.BILLINGCYCLE = LNS00062.KEYCODE                                                    ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          KEYCODE                                                                                    ")
        SQLStr.AppendLine("         ,VALUE1                                                                                     ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0006_FIXVALUE                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          CAMPCODE = @CAMPCODE                                                                       ")
        SQLStr.AppendLine("      AND CLASS = 'CALCMETHOD'                                                                       ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS00063                                                                                       ")
        SQLStr.AppendLine("      ON  LNM0019.CALCMETHOD = LNS00063.KEYCODE                                                      ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          DIESELPRICESITEID                                                                          ")
        SQLStr.AppendLine("         ,DIESELPRICESITENAME                                                                        ")
        SQLStr.AppendLine("         ,DIESELPRICESITEBRANCH                                                                      ")
        SQLStr.AppendLine("         ,DIESELPRICESITEKBNNAME                                                                     ")
        SQLStr.AppendLine("         ,DISPLAYNAME                                                                                ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNM0020_DIESELPRICESITE                                                                ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNM0020                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0019.DIESELPRICESITEID = LNM0020.DIESELPRICESITEID                                      ")
        SQLStr.AppendLine("      AND LNM0019.DIESELPRICESITEBRANCH = LNM0020.DIESELPRICESITEBRANCH                              ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim dt As DateTime

        '有効開始日
        If DateTime.TryParse(WF_StYMD.Value, dt) Then
            SQLStr.AppendLine(" AND  @STYMD BETWEEN LNM0019.STYMD AND LNM0019.ENDYMD  ")
        End If
        '取引先コード
        If Not WF_TORI.SelectedValue = "" Then
            SQLStr.AppendLine(" AND  LNM0019.TORICODE = @TORICODE ")
        End If
        '部門コード
        If Not WF_ORG.SelectedValue = "" Then
            SQLStr.AppendLine(" AND  LNM0019.ORGCODE = @ORGCODE ")
        End If
        '削除フラグ
        If Not ChkDelDataFlg.Checked Then
            SQLStr.AppendLine(" AND  LNM0019.DELFLG = '0' ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0019.TORICODE                                                           ")
        SQLStr.AppendLine("    ,LNM0019.ORGCODE                                                            ")
        SQLStr.AppendLine("    ,LNM0019.KASANORGCODE                                                       ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                '会社
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                P_CAMPCODE.Value = Master.USERCAMP

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

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0019tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0019tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0019row As DataRow In LNM0019tbl.Rows
                    i += 1
                    LNM0019row("LINECNT") = i        'LINECNT

                    If LNM0019row("DELFLG") = C_DELETE_FLG.ALIVE Then
                        LNM0019row("SURCHARGEFEE") = CONST_BTNFEE
                        LNM0019row("TANKA") = CONST_BTNTANKA
                    Else
                        LNM0019row("SURCHARGEFEE") = CONST_BTNFEE_DEL
                        LNM0019row("TANKA") = CONST_BTNTANKA_DEL
                    End If

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN Select"
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

        work.WF_SEL_KASANORGCODE.Text = ""                                       '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = ""                                       '加算先部門名称
        work.WF_SEL_SURCHARGEPATTERNCODE.Text = ""                               'サーチャージパターンコード
        work.WF_SEL_SURCHARGEPATTERNNAME.Text = ""                               'サーチャージパターン
        work.WF_SEL_BILLINGCYCLE.Text = ""                                       '請求サイクル
        work.WF_SEL_BILLINGCYCLENAME.Text = ""                                   '請求サイクル名
        work.WF_SEL_CALCMETHOD.Text = ""                                         '距離算定方式
        work.WF_SEL_CALCMETHODNAME.Text = ""                                     '距離算定方式名
        work.WF_SEL_STYMD.Text = Date.Now.ToString("yyyy/MM/01")                 '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0019WRKINC.MAX_ENDYMD                       '有効終了日
        work.WF_SEL_DIESELPRICESITEID.Text = ""                                  '実勢軽油価格参照先ID
        work.WF_SEL_DIESELPRICESITENAME.Text = ""                                '実勢軽油価格参照先名
        work.WF_SEL_DIESELPRICESITEBRANCH.Text = ""                              '実勢軽油価格参照先ID枝番
        work.WF_SEL_DIESELPRICESITEKBNNAME.Text = ""                             '実勢軽油価格参照先区分名
        work.WF_SEL_DISPLAYNAME.Text = ""                                        '実勢軽油価格参照先画面表示名

        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0019tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0019SurchargePatternHistory.aspx")
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0019row As DataRow In LNM0019tbl.Rows
            If LNM0019row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0019row("SELECT") = WW_DataCNT

                If LNM0019row("DELFLG") = C_DELETE_FLG.ALIVE Then
                    LNM0019row("SURCHARGEFEE") = CONST_BTNFEE
                    LNM0019row("TANKA") = CONST_BTNTANKA
                Else
                    LNM0019row("SURCHARGEFEE") = CONST_BTNFEE_DEL
                    LNM0019row("TANKA") = CONST_BTNTANKA_DEL
                End If
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
        Dim TBLview As DataView = New DataView(LNM0019tbl)

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
        Dim TBLview As New DataView(LNM0019tbl)
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

    '入力状態を保持する
    Protected Sub InputSave()
        work.WF_SEL_TARGETYMD_L.Text = WF_StYMD.Value '対象日
        work.WF_SEL_TORI_L.Text = WF_TORI.SelectedValue '荷主
        work.WF_SEL_ORG_L.Text = WF_ORG.SelectedValue '部門

        work.WF_SEL_CHKDELDATAFLG_L.Text = ChkDelDataFlg.Checked '削除済みデータ表示状態

    End Sub

    '選択行の取引先、部門、サーチャージパターン、請求サイクルを保持する
    Protected Sub SurchargeFeeSave()

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0019tbl.Rows(WW_LineCNT)("LINECNT")                               '選択行
        work.WF_SEL_TORICODE.Text = LNM0019tbl.Rows(WW_LineCNT)("TORICODE")                             '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0019tbl.Rows(WW_LineCNT)("TORINAME")                             '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGCODE")                               '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGNAME")                               '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGCODE")                     '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGNAME")                     '加算先部門名称
        work.WF_SEL_SURCHARGEPATTERNCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNCODE")     'サーチャージパターンコード
        work.WF_SEL_SURCHARGEPATTERNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNNAME")     'サーチャージパターン名
        work.WF_SEL_BILLINGCYCLE.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLE")                     '請求サイクル
        work.WF_SEL_BILLINGCYCLENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLENAME")             '請求サイクル名
        work.WF_SEL_CALCMETHOD.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHOD")                         '距離算定方式
        work.WF_SEL_CALCMETHODNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHODNAME")                 '距離算定方式名
        work.WF_SEL_STYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("STYMD")                                   '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("ENDYMD")                                 '有効終了日
        work.WF_SEL_DIESELPRICESITEID.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEID")           '実勢軽油価格参照先ID
        work.WF_SEL_DIESELPRICESITENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITENAME")       '実勢軽油価格参照先名
        work.WF_SEL_DIESELPRICESITEBRANCH.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEBRANCH")   '実勢軽油価格参照先ID枝番
        work.WF_SEL_DIESELPRICESITEKBNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEKBNNAME") '実勢軽油価格参照先区分名
        work.WF_SEL_DISPLAYNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DISPLAYNAME")                       '実勢軽油価格参照先表示名
        work.WF_SEL_DELFLG.Text = LNM0019tbl.Rows(WW_LineCNT)("DELFLG")                                 '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0019tbl.Rows(WW_LineCNT)("UPDTIMSTP")                           'タイムスタンプ

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

        '○ 実績単価履歴画面ページへ遷移
        Server.Transfer("~/LNG/mas/LNT0030SurchargeFee.aspx")

    End Sub

    '選択行の軽油価格参照先を保持する
    Protected Sub DieselPriceSiteSave()

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0019tbl.Rows(WW_LineCNT)("LINECNT")                               '選択行
        work.WF_SEL_TORICODE.Text = LNM0019tbl.Rows(WW_LineCNT)("TORICODE")                             '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0019tbl.Rows(WW_LineCNT)("TORINAME")                             '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGCODE")                               '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGNAME")                               '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGCODE")                     '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGNAME")                     '加算先部門名称
        work.WF_SEL_SURCHARGEPATTERNCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNCODE")     'サーチャージパターンコード
        work.WF_SEL_SURCHARGEPATTERNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNNAME")     'サーチャージパターン名
        work.WF_SEL_BILLINGCYCLE.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLE")                     '請求サイクル
        work.WF_SEL_BILLINGCYCLENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLENAME")             '請求サイクル名
        work.WF_SEL_CALCMETHOD.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHOD")                         '距離算定方式
        work.WF_SEL_CALCMETHODNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHODNAME")                 '距離算定方式名
        work.WF_SEL_STYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("STYMD")                                   '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("ENDYMD")                                 '有効終了日
        work.WF_SEL_DIESELPRICESITEID.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEID")           '実勢軽油価格参照先ID
        work.WF_SEL_DIESELPRICESITENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITENAME")       '実勢軽油価格参照先名
        work.WF_SEL_DIESELPRICESITEBRANCH.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEBRANCH")   '実勢軽油価格参照先ID枝番
        work.WF_SEL_DIESELPRICESITEKBNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEKBNNAME") '実勢軽油価格参照先区分名
        work.WF_SEL_DISPLAYNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DISPLAYNAME")                       '実勢軽油価格参照先表示名
        work.WF_SEL_DELFLG.Text = LNM0019tbl.Rows(WW_LineCNT)("DELFLG")                                 '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0019tbl.Rows(WW_LineCNT)("UPDTIMSTP")                           'タイムスタンプ

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

        '○ 実績単価履歴画面ページへ遷移
        Server.Transfer("~/LNG/mas/LNT0031DieselPriceHist.aspx")

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
        If LNM0019tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE Then
            Dim WW_ROW As DataRow
            WW_ROW = LNM0019tbl.Rows(WW_LineCNT)
            Dim DATENOW As Date = Date.Now
            Dim WW_UPDTIMSTP As Date

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                '履歴登録(変更前)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '削除フラグ有効化
                DelflgValid(SQLcon, WW_ROW, DATENOW, WW_UPDTIMSTP)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '履歴登録(変更後)
                InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                LNM0019tbl.Rows(WW_LineCNT)("DELFLG") = C_DELETE_FLG.ALIVE
                Dim WW_Str As String = ""
                CODENAME_get("DELFLG", LNM0019tbl.Rows(WW_LineCNT)("DELFLG"), WW_Str, WW_RtnSW)
                LNM0019tbl.Rows(WW_LineCNT)("DELFLGNAME") = WW_Str
                LNM0019tbl.Rows(WW_LineCNT)("UPDTIMSTP") = WW_UPDTIMSTP
                Master.SaveTable(LNM0019tbl)
                Master.Output(C_MESSAGE_NO.DELETE_ROW_ACTIVATION, C_MESSAGE_TYPE.NOR, needsPopUp:=True)
            End Using
            Exit Sub
        End If

        work.WF_SEL_LINECNT.Text = LNM0019tbl.Rows(WW_LineCNT)("LINECNT")                               '選択行

        work.WF_SEL_TORICODE.Text = LNM0019tbl.Rows(WW_LineCNT)("TORICODE")                             '取引先コード
        work.WF_SEL_TORINAME.Text = LNM0019tbl.Rows(WW_LineCNT)("TORINAME")                             '取引先名称
        work.WF_SEL_ORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGCODE")                               '部門コード
        work.WF_SEL_ORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("ORGNAME")                               '部門名称
        work.WF_SEL_KASANORGCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGCODE")                     '加算先部門コード
        work.WF_SEL_KASANORGNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("KASANORGNAME")                     '加算先部門名称
        work.WF_SEL_SURCHARGEPATTERNCODE.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNCODE")     'サーチャージパターンコード
        work.WF_SEL_SURCHARGEPATTERNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("SURCHARGEPATTERNNAME")     'サーチャージパターン名
        work.WF_SEL_BILLINGCYCLE.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLE")                     '請求サイクル
        work.WF_SEL_BILLINGCYCLENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("BILLINGCYCLENAME")             '請求サイクル名
        work.WF_SEL_CALCMETHOD.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHOD")                         '距離算定方式
        work.WF_SEL_CALCMETHODNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("CALCMETHODNAME")                 '距離算定方式名
        work.WF_SEL_STYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("STYMD")                                   '有効開始日
        work.WF_SEL_ENDYMD.Text = LNM0019tbl.Rows(WW_LineCNT)("ENDYMD")                                 '有効終了日
        work.WF_SEL_DIESELPRICESITEID.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEID")           '実勢軽油価格参照先ID
        work.WF_SEL_DIESELPRICESITENAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITENAME")       '実勢軽油価格参照先名
        work.WF_SEL_DIESELPRICESITEBRANCH.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEBRANCH")   '実勢軽油価格参照先ID枝番
        work.WF_SEL_DIESELPRICESITEKBNNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DIESELPRICESITEKBNNAME") '実勢軽油価格参照先区分名
        work.WF_SEL_DISPLAYNAME.Text = LNM0019tbl.Rows(WW_LineCNT)("DISPLAYNAME")                       '実勢軽油価格参照先表示名
        work.WF_SEL_DELFLG.Text = LNM0019tbl.Rows(WW_LineCNT)("DELFLG")                                 '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNM0019tbl.Rows(WW_LineCNT)("UPDTIMSTP")                           'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                     '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                            work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_SURCHARGEPATTERNCODE.Text,
                            work.WF_SEL_BILLINGCYCLE.Text, work.WF_SEL_STYMD.Text)
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
        SQLStr.Append("     LNG.LNM0019_SURCHARGEPATTERN            ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '0'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')                       = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')                        = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(SURCHARGEPATTERNCODE, '')           = @SURCHARGEPATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(BILLINGCYCLE, '')                   = @BILLINGCYCLE ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE"
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
        SQLStrTimStp.AppendLine("     LNG.LNM0019_SURCHARGEPATTERN")
        SQLStrTimStp.AppendLine(" WHERE")
        SQLStrTimStp.AppendLine("         COALESCE(TORICODE, '')                       = @TORICODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(ORGCODE, '')                        = @ORGCODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')           = @SURCHARGEPATTERNCODE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')                   = @BILLINGCYCLE ")
        SQLStrTimStp.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrTimStp.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN SELECT"
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
                    LNM0019WRKINC.getOrgName(I_VALUE, O_TEXT, O_RTN)
                Case "DELFLG", "SURCHARGEPATTERN", "BILLINGCYCLE", "CALCMETHOD"          '削除フラグ、サーチャージパターンコード、請求サイクル、距離算定方式
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "TORICODE"
                    LNM0019WRKINC.getToriName(I_VALUE, O_TEXT, O_RTN)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0019WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "サーチャージ定義マスタ一覧"
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
            Case LNM0019WRKINC.FILETYPE.EXCEL
                FileName = "サーチャージ定義マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0019WRKINC.FILETYPE.PDF
                FileName = "サーチャージ定義マスタ.pdf"
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
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.KASANORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '加算先部門コード
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'サーチャージパターンコード
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求サイクル
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '有効開始日
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ

        '入力不要列網掛け
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.TORINAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '取引先名
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.ORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '部門名
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.KASANORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '加算先部門名
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.ENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '有効終了日
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'サーチャージパターン名
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '請求サイクル名
        'sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '距離算定方式
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.CALCMETHODNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '距離算定方式
        'sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先ID
        'sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先ID枝番
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先名
        sheet.Columns(LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先区分名

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
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "（必須）加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE).Value = "（必須）サーチャージパターンコード"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNNAME).Value = "サーチャージパターン名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE).Value = "（必須）請求サイクル"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLENAME).Value = "請求サイクル名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = "距離算定方式"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHODNAME).Value = "距離算定方式名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.STYMD).Value = "（必須）有効開始日"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.ENDYMD).Value = "有効終了日"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Value = "実勢軽油価格参照先ID"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Value = "実勢軽油価格参照先名"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Value = "実勢軽油価格参照先ID枝番"
        sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Value = "実勢軽油価格参照先区分名"

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
                sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '有効終了日
            WW_TEXT = "※未入力の場合は「2099/12/31」が設定されます。"
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.ENDYMD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.ENDYMD).Comment.Shape
                .Width = 180
                .Height = 30
            End With

            'サーチャージパターンコード
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("01(荷主単位)：車両や届先に関わらず、荷主単位でサーチャージ料金が定義されている場合に使用します")
            WW_TEXTLIST.AppendLine("02(届先単位)：輸送距離や基準単価等が、届先(及び出荷場所)毎に定義されている場合に使用します")
            WW_TEXTLIST.AppendLine("03(車型単位)：車型によって基準単価等が異なる場合に使用します。届先を条件に含めることも可能です")
            WW_TEXTLIST.AppendLine("04(車腹単位)：車腹によって基準単価等が異なる場合に使用します。届先を条件に含めることも可能です")
            WW_TEXTLIST.AppendLine("05(車番単位)：車番によって基準単価等が異なる場合に使用します。最小単位です")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE).Comment.Shape
                .Width = 600
                .Height = 60
            End With

            '請求サイクル
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("0：毎月")
            WW_TEXTLIST.AppendLine("1：1回/年度")
            WW_TEXTLIST.AppendLine("2：2回/年度")
            WW_TEXTLIST.AppendLine("3：3回/年度")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE).Comment.Shape
                .Width = 100
                .Height = 50
            End With

            '距離算定方式
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("1:距離定義による計算")
            WW_TEXTLIST.AppendLine("2:距離は実績値を画面に入力")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD).Comment.Shape
                .Width = 200
                .Height = 30
            End With

            '実勢軽油価格参照先ID
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("軽油価格参照先管理マスタの")
            WW_TEXTLIST.AppendLine("実勢軽油価格参照先IDを入力してください")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Comment.Shape
                .Width = 200
                .Height = 30
            End With

            '実勢軽油価格参照先ID枝番
            WW_TEXTLIST.Clear()
            WW_TEXTLIST.AppendLine("軽油価格参照先管理マスタの")
            WW_TEXTLIST.AppendLine("実勢軽油価格参照先ID枝番を入力してください")
            WW_TEXT = WW_TEXTLIST.ToString
            sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Comment.Shape
                .Width = 220
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
        SETFIXVALUELIST(subsheet, "DELFLG", LNM0019WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0019WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        'サーチャージパターンコード
        SETFIXVALUELIST(subsheet, "SURCHARGEPATTERN", LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        '請求サイクル
        SETFIXVALUELIST(subsheet, "BILLINGCYCLE", LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        '距離算定方式
        SETFIXVALUELIST(subsheet, "CALCMETHOD", LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_SUB_STRANGE = subsheet.Cells(0, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD)
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
        'WW_STRANGE = sheet.Cells(WW_STROW, LNM0019WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNM0019WRKINC.INOUTEXCELCOL.BRANCHCODE)
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

        For Each Row As DataRow In LNM0019tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE).Value = Row("SURCHARGEPATTERNCODE") 'サーチャージパターンコード
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNNAME).Value = Row("SURCHARGEPATTERNNAME") 'サーチャージパターン名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE).Value = Row("BILLINGCYCLE") '請求サイクル
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLENAME).Value = Row("BILLINGCYCLENAME") '請求サイクル名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = Row("CALCMETHOD") '距離算定方式
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHODNAME).Value = Row("CALCMETHODNAME") '距離算定方式名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.STYMD).Value = Row("STYMD") '有効開始日
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.ENDYMD).Value = Row("ENDYMD") '有効終了日
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Value = Row("DIESELPRICESITEID") '実勢軽油価格参照先ID
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Value = Row("DIESELPRICESITENAME") '実勢軽油価格参照先名
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Value = Row("DIESELPRICESITEBRANCH") '実勢軽油価格参照先ID枝番
            sheet.Cells(WW_ACTIVEROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Value = Row("DIESELPRICESITEKBNNAME") '実勢軽油価格参照先区分名


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
                Case "DELFLG", "SURCHARGEPATTERN", "BILLINGCYCLE", "CALCMETHOD"   '削除フラグ、サーチャージパターンコード、請求サイクル、距離算定方式
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
        filePath = "D:\サーチャージ定義マスタ一括アップロードテスト.xlsx"

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

            For Each Row As DataRow In LNM0019Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    Dim SkipChk = ValidationSkipChk(SQLcon, Row)
                    If SkipChk = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0019WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0019WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        Continue For
                    End If


                    '有効開始日、有効終了日更新
                    If Not Row("TORICODE") = "" AndAlso
                       Not Row("ORGCODE") = "" AndAlso
                       Not Row("SURCHARGEPATTERNCODE") = "" AndAlso
                       Not Row("BILLINGCYCLE") = "" AndAlso
                       Not Row("STYMD") = Date.MinValue Then

                        WF_AUTOENDYMD.Value = ""

                        '新規、有効開始日が変更されたときの対応
                        If work.AddDataChk(SQLcon, Row) = True Then '新規の場合
                            WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                        Else
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0019WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            Select Case True
                                Case WW_BeforeMAXSTYMD = "" '無いと思うが1件も対象の枝番データが無い場合
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd") '同一の場合
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
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
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
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
                    If WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "サーチャージ定義マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0019Exceltbl) Then
            LNM0019Exceltbl = New DataTable
        End If
        If LNM0019Exceltbl.Columns.Count <> 0 Then
            LNM0019Exceltbl.Columns.Clear()
        End If
        LNM0019Exceltbl.Clear()

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

            For Each Row As DataRow In LNM0019Exceltbl.Rows

                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    Dim SkipChk = ValidationSkipChk(SQLcon, Row)
                    If SkipChk = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNM0019WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNM0019WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    '有効開始日、有効終了日更新
                    If Not Row("TORICODE") = "" AndAlso
                       Not Row("ORGCODE") = "" AndAlso
                       Not Row("SURCHARGEPATTERNCODE") = "" AndAlso
                       Not Row("BILLINGCYCLE") = "" AndAlso
                       Not Row("STYMD") = Date.MinValue Then

                        WF_AUTOENDYMD.Value = ""

                        '新規、有効開始日が変更されたときの対応
                        If work.AddDataChk(SQLcon, Row) = True Then '新規の場合
                            WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                        Else
                            '更新前の最大有効開始日取得
                            WW_BeforeMAXSTYMD = LNM0019WRKINC.GetSTYMD(SQLcon, Row, WW_DBDataCheck)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If

                            Select Case True
                                Case WW_BeforeMAXSTYMD = "" '無いと思うが1件も対象の枝番データが無い場合
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                                Case WW_BeforeMAXSTYMD = CDate(Row("STYMD")).ToString("yyyy/MM/dd") '同一の場合
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
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
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '変更前の有効終了日更新
                                    UpdateENDYMD(SQLcon, Row, WW_DBDataCheck, DATENOW)
                                    If Not isNormal(WW_DBDataCheck) Then
                                        Exit Sub
                                    End If
                                    '履歴テーブルに変更後データを登録
                                    InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                        Exit Sub
                                    End If
                                    '退避した有効開始日を元に戻す
                                    Row("STYMD") = WW_STYMD_SAVE
                                    '退避した有効終了日を元に戻す
                                    Row("ENDYMD") = WW_ENDYMD_SAVE
                                    '有効終了日に最大値を入れる
                                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
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
                    If WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("        ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("        ,BILLINGCYCLE  ")
        SQLStr.AppendLine("        ,CALCMETHOD  ")
        SQLStr.AppendLine("        ,STYMD  ")
        SQLStr.AppendLine("        ,ENDYMD  ")
        SQLStr.AppendLine("        ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("        ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0019_SURCHARGEPATTERN ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0019Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN SELECT"
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

            Dim LNM0019Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNM0019Exceltblrow = LNM0019Exceltbl.NewRow

                'LINECNT
                LNM0019Exceltblrow("LINECNT") = WW_LINECNT

                '◆データセット
                '取引先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.TORICODE))
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNM0019Exceltblrow("TORICODE") = LNM0019WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.TORINAME))
                WW_DATATYPE = DataTypeHT("TORINAME")
                LNM0019Exceltblrow("TORINAME") = LNM0019WRKINC.DataConvert("取引先名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.ORGCODE))
                WW_DATATYPE = DataTypeHT("ORGCODE")
                LNM0019Exceltblrow("ORGCODE") = LNM0019WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.ORGNAME))
                WW_DATATYPE = DataTypeHT("ORGNAME")
                LNM0019Exceltblrow("ORGNAME") = LNM0019WRKINC.DataConvert("部門名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGCODE))
                WW_DATATYPE = DataTypeHT("KASANORGCODE")
                LNM0019Exceltblrow("KASANORGCODE") = LNM0019WRKINC.DataConvert("加算先部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '加算先部門名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.KASANORGNAME))
                WW_DATATYPE = DataTypeHT("KASANORGNAME")
                LNM0019Exceltblrow("KASANORGNAME") = LNM0019WRKINC.DataConvert("加算先部門名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'サーチャージパターンコード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNCODE))
                WW_DATATYPE = DataTypeHT("SURCHARGEPATTERNCODE")
                LNM0019Exceltblrow("SURCHARGEPATTERNCODE") = LNM0019WRKINC.DataConvert("サーチャージパターンコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'サーチャージパターン名
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.SURCHARGEPATTERNNAME))
                WW_DATATYPE = DataTypeHT("SURCHARGEPATTERNNAME")
                LNM0019Exceltblrow("SURCHARGEPATTERNNAME") = LNM0019WRKINC.DataConvert("サーチャージパターン名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求サイクル
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.BILLINGCYCLE))
                WW_DATATYPE = DataTypeHT("BILLINGCYCLE")
                LNM0019Exceltblrow("BILLINGCYCLE") = LNM0019WRKINC.DataConvert("請求サイクル", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '距離算定方式
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.CALCMETHOD))
                WW_DATATYPE = DataTypeHT("CALCMETHOD")
                LNM0019Exceltblrow("CALCMETHOD") = LNM0019WRKINC.DataConvert("距離算定方式", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '有効開始日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.STYMD))
                WW_DATATYPE = DataTypeHT("STYMD")
                LNM0019Exceltblrow("STYMD") = LNM0019WRKINC.DataConvert("有効開始日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '有効終了日
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.ENDYMD))
                WW_DATATYPE = DataTypeHT("ENDYMD")
                LNM0019Exceltblrow("ENDYMD") = LNM0019WRKINC.DataConvert("有効終了日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実勢軽油価格参照先ID
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEID))
                WW_DATATYPE = DataTypeHT("DIESELPRICESITEID")
                LNM0019Exceltblrow("DIESELPRICESITEID") = LNM0019WRKINC.DataConvert("実勢軽油価格参照先ID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実勢軽油価格参照先ID枝番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH))
                WW_DATATYPE = DataTypeHT("DIESELPRICESITEBRANCH")
                LNM0019Exceltblrow("DIESELPRICESITEBRANCH") = LNM0019WRKINC.DataConvert("実勢軽油価格参照先ID枝番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0019WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNM0019Exceltblrow("DELFLG") = LNM0019WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '登録
                LNM0019Exceltbl.Rows.Add(LNM0019Exceltblrow)

                WW_LINECNT = WW_LINECNT + 1
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
        SQLStr.AppendLine("    Select")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(CALCMETHOD, '')             = @CALCMETHOD ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        If Not WW_ROW("ENDYMD") = Date.MinValue Then
            SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
        End If
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEID, '')             = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')             = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 20)     '加算先部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_CALCMETHOD As MySqlParameter = SQLcmd.Parameters.Add("@CALCMETHOD", MySqlDbType.VarChar, 1)     '距離算定方式
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_CALCMETHOD.Value = WW_ROW("CALCMETHOD")           '距離算定方式
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")           '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")           '実勢軽油価格参照先ID枝番

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN SELECT"
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
            WW_ROW("SURCHARGEPATTERNCODE") = "" OrElse
            WW_ROW("BILLINGCYCLE") = "" OrElse
            WW_ROW("STYMD") = Date.MinValue Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN SELECT"
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
        SQLStr.Append("     LNG.LNM0019_SURCHARGEPATTERN            ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE"
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
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("     ,RECEIVEYMD  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@ORGCODE  ")
        SQLStr.AppendLine("     ,@ORGNAME  ")
        SQLStr.AppendLine("     ,@KASANORGCODE  ")
        SQLStr.AppendLine("     ,@KASANORGNAME  ")
        SQLStr.AppendLine("     ,@SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,@SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,@BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,@CALCMETHOD  ")
        SQLStr.AppendLine("     ,@STYMD  ")
        SQLStr.AppendLine("     ,@ENDYMD  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("     ,@RECEIVEYMD  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGCODE =  @ORGCODE")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE =  @SURCHARGEPATTERNCODE")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME =  @SURCHARGEPATTERNNAME")
        SQLStr.AppendLine("     ,BILLINGCYCLE =  @BILLINGCYCLE")
        SQLStr.AppendLine("     ,CALCMETHOD =  @CALCMETHOD")
        SQLStr.AppendLine("     ,STYMD =  @STYMD")
        SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
        SQLStr.AppendLine("     ,DIESELPRICESITEID =  @DIESELPRICESITEID")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH =  @DIESELPRICESITEBRANCH")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 20)     '取引先名
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 20)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_SURCHARGEPATTERNNAME As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNNAME", MySqlDbType.VarChar, 20)     'サーチャージパターン名
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_CALCMETHOD As MySqlParameter = SQLcmd.Parameters.Add("@CALCMETHOD", MySqlDbType.VarChar, 1)     '距離算定方式
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

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
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_SURCHARGEPATTERNNAME.Value = WW_ROW("SURCHARGEPATTERNNAME")           'サーチャージパターン名
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_CALCMETHOD.Value = WW_ROW("CALCMETHOD")           '距離算定方式
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日
                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")           '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")           '実勢軽油価格参照先ID枝番
                P_DELFLG.Value = WW_ROW("DELFLG")           '削除フラグ

                '有効終了日
                If Not WW_ROW("ENDYMD") = Date.MinValue Then
                    P_ENDYMD.Value = WW_ROW("ENDYMD")
                Else
                    P_ENDYMD.Value = WF_AUTOENDYMD.Value
                End If


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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0019_SURCHARGEPATTERN  INSERTUPDATE"
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
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("TORICODE", WW_ROW("TORICODE"), WW_ROW("TORINAME"), WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・取引先コード入力エラーです。"
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
            If LNM0019WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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
                    Else
                        CODENAME_get("ORG", WW_ROW("ORGCODE"), WW_ROW("ORGNAME"), WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・部門コード入力エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                End Using
            Else
                CODENAME_get("ORG", WW_ROW("ORGCODE"), WW_ROW("ORGNAME"), WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・部門コード入力エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
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
        ' 加算先部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGCODE", WW_ROW("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            CODENAME_get("ORG", WW_ROW("KASANORGCODE"), WW_ROW("KASANORGNAME"), WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・加算先部門コード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・加算先部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGNAME", WW_ROW("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' サーチャージパターンコード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SURCHARGEPATTERNCODE", WW_ROW("SURCHARGEPATTERNCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("SURCHARGEPATTERN", WW_ROW("SURCHARGEPATTERNCODE"), WW_ROW("SURCHARGEPATTERNNAME"), WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・サーチャージパターンコード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・サーチャージパターンコードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' サーチャージパターン名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SURCHARGEPATTERNNAME", WW_ROW("SURCHARGEPATTERNNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・サーチャージパターン名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 請求サイクル(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BILLINGCYCLE", WW_ROW("BILLINGCYCLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("BILLINGCYCLE", WW_ROW("BILLINGCYCLE"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・ 請求サイクル入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・請求サイクルエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 距離算定方式(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CALCMETHOD", WW_ROW("CALCMETHOD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("CALCMETHOD", WW_ROW("CALCMETHOD"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・ 距離算定方式入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・距離算定方式エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 有効開始日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "STYMD", WW_ROW("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・有効開始日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 有効終了日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ENDYMD", WW_ROW("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・有効終了日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 実勢軽油価格参照先ID(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESITEID", WW_ROW("DIESELPRICESITEID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢軽油価格参照先IDエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 実勢軽油価格参照先ID枝番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESITEBRANCH", WW_ROW("DIESELPRICESITEBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢軽油価格参照先ID枝番エラーです。"
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

        'サーチャージ定義マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE") 'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE") '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日

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
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0032_SURCHARGEPATTERNHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0032_SURCHARGEPATTERNHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0032_SURCHARGEPATTERNHIST  INSERT"
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
        SQLStr.Append("     LNG.LNM0019_SURCHARGEPATTERN                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 6) 'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 6)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE") 'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE") '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE"
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


