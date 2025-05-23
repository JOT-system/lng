''************************************************************
' 輸送費明細出力状況画面
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
''' 輸送費明細出力状況（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNT0002TranStatusList
    Inherits Page

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                  '実績（アボカド）データ格納用テーブル
    Private LNT0001Tanktbl As DataTable                              '実績（アボカド）単価データ格納用テーブル
    Private LNT0001Koteihi As DataTable                              '-- 固定費マスタ
    Private LNT0001HachinoheSprate As DataTable                      '-- 八戸特別料金マスタ
    Private LNT0001EneosComfee As DataTable                          '-- ENEOS業務委託料マスタ
    Private LNT0001SKKoteichi As DataTable                           '-- 石油資源開発(固定値(業務車番))マスタ
    Private LNT0001TogouSprate As DataTable                          '-- 統合版特別料金マスタ
    Private LNT0001SKSprate As DataTable                             '-- SK特別料金マスタ
    Private LNT0001SKSurcharge As DataTable                          '-- SK燃料サーチャージマスタ
    Private LNT0001Calendar As DataTable                             '-- カレンダーマスタ
    Private LNT0001HolidayRate As DataTable                          '-- 休日割増単価マスタ

    Private LNT0002tbl As DataTable           '一覧格納用テーブル
    Private LNT0002tblHIST As DataTable       '履歴一覧格納用テーブル
    Private LNT0002UPDtbl As DataTable        '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNT0002Exceltbl As New DataTable  'Excelデータ格納用テーブル
    Private LNT0002Shippers As New DataTable  '荷主一覧格納

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    Private Const CONST_BTNADD As String = "<div><input class=""btn-sticky"" id=""btnAdd""　type=""button"" value=""追加"" readonly onclick=""BtnAddClick();"" /></div>"
    Private Const CONST_BTNOUT As String = "<div><input class=""btn-sticky"" id=""btnOut""　type=""button"" value=""出力"" readonly onclick=""BtnOutputClick();"" /></div>"
    Private Const CONST_BTNREF As String = "<div><input class=""btn-sticky"" id=""btnRef""　type=""button"" value=""参照"" readonly onclick=""BtnReferenceClick();"" /></div>"

    Private Const CONST_BTNHISTOUT As String = "<div><input class=""btn-sticky"" id=""btnHistOut""　type=""button"" value=""出力"" readonly onclick=""BtnHistOutputClick();"" /></div>"

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              '固定値マスタ
    Private CMNPTS As New CmnParts                                  '共通関数
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

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
                    Master.RecoverTable(LNT0002tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonEND" '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_SelectCALENDARChange" 'カレンダー変更時
                            GridViewInitialize()
                        Case "WF_ButtonRefClick" '参照ボタン押下時
                            work.WF_HIST.Text = "visible"
                            WF_ButtonRefClick()
                        Case "WF_ButtonAddClick" '追加ボタン押下時
                            'WF_ButtonAddClick()
                            WF_ButtonAJUST_Click()
                        Case "WF_ButtonOutClick" '出力ボタン押下時
                            Master.MAPID = LNT0001WRKINC.MAPIDI
                            WF_ButtonOutClick()
                            Master.MAPID = LNT0002WRKINC.MAPIDL
                            If isNormal(WW_ErrSW) Then
                                '出力履歴登録
                                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                    SQLcon.Open()  ' DataBase接続
                                    INSHIST(SQLcon)
                                End Using
                            End If
                            GridViewInitialize()
                        '閉じるボタン押下時
                        Case "WF_ButtonCLOSE"
                            work.WF_HIST.Text = "hidden"

                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonOutClick" And
                        Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" Then
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
            If Not IsNothing(LNT0002tbl) Then
                LNT0002tbl.Clear()
                LNT0002tbl.Dispose()
                LNT0002tbl = Nothing
            End If
        End Try

        '請求調整列編集
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_InvoiceCtrlCol();", True)

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0002WRKINC.MAPIDL
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
        If LNT0002WRKINC.AdminCheck(Master.ROLE_ORG) = False And '情シス、高圧ガス
             LNT0002WRKINC.IshikariCheck(Master.ROLE_ORG) = False And '石狩営業所
             LNT0002WRKINC.HachinoheCheck(Master.ROLE_ORG) = False And '八戸営業所
             LNT0002WRKINC.TohokuCheck(Master.ROLE_ORG) = False And '東北支店
             LNT0002WRKINC.MizushimaCheck(Master.ROLE_ORG) = False Then '水島営業所

            '○ メニュー画面遷移
            Master.TransitionPrevPage(, LNT0002WRKINC.TITLEKBNS)
        End If

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001AJ Then

            ' 調整画面からの遷移
            Master.RecoverTable(LNT0002tbl, work.WF_SEL_INPTBL.Text)
            WF_TaishoYm.Value = Left(work.WF_SEL_TARGETYM.Text, 7)
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
        If LNT0002WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

        '〇荷主情報取得
        ShippersGet()

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
        Master.SaveTable(LNT0002tbl)

        '〇 一覧の件数を取得
        'Me.ListCount.Text = "件数：" + LNT0002tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0002tbl)
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
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0002tbl) Then
            LNT0002tbl = New DataTable
        End If

        If LNT0002tbl.Columns.Count <> 0 Then
            LNT0002tbl.Columns.Clear()
        End If

        LNT0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを輸送費明細出力履歴から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , CURDATE()                                                                AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE4), '')                                          AS INDEXKEY            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE5), '')                                          AS TORICODE            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE6), '')                                          AS ORGCODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE1), '')                                          AS TRANDETAILNAME      ")

        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.KEYCODE), '')                                         AS KEYCODE             ")

        'SQLStr.AppendLine("   ,  COALESCE(DATE_FORMAT(B.UPDYMD, '%Y/%m/%d %H:%i:%s'), '')                AS UPDYMD              ")
        'SQLStr.AppendLine("   ,  COALESCE(RTRIM(B.STAFFNAMES), '')                                       AS UPDUSERNAME         ")
        SQLStr.AppendLine("   ,  ''                                                                       AS UPDYMD             ")
        SQLStr.AppendLine("   ,  ''                                                                       AS UPDUSERNAME        ")


        SQLStr.AppendLine("   ,  COALESCE(RTRIM(A.SEQ), '0')                                             AS KAISU               ")
        SQLStr.AppendLine("   ,  COALESCE(DATE_FORMAT(A.INTAKEDATE, '%Y/%m/%d %H:%i:%s'), '')            AS DLYMD               ")
        SQLStr.AppendLine("   ,  COALESCE(RTRIM(A.USERNAME), '')                                         AS DLUSERNAME          ")
        SQLStr.AppendLine("   ,  COALESCE(RTRIM(A.ORGNAME), '')                                          AS DLORGNAME           ")

        '画面ボタン用
        SQLStr.AppendLine("   ,  ''                                                                      AS DETAIL              ")　'明細列
        SQLStr.AppendLine("   ,  ''                                                                      AS CONTROL             ")　'操作列
        SQLStr.AppendLine("   ,  ''                                                                      AS HISTORY             ")　'履歴列

        '画面請求調整用
        SQLStr.AppendLine("   , COALESCE(RTRIM(SPRATE.TORICODE), '')                                     AS CTRLSPRATE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TANKA.TORICODE), '')                                      AS CTRLTANKA           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIXED.TORICODE), '')                                      AS CTRLKOTEIHI         ")
        SQLStr.AppendLine("   ,  ''                                                                      AS CTRLSURCHARGE       ")

        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     COM.LNS0006_FIXVALUE FIX                                                                        ")

        '回数、最新ダウンロード日時、最新ダウンロード実施者
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          A1.TAISHOYM                                                                                ")
        SQLStr.AppendLine("          ,A1.TORICODE                                                                               ")
        SQLStr.AppendLine("          ,A1.SEQ                                                                                    ")
        SQLStr.AppendLine("          ,A1.USERNAME                                                                               ")
        SQLStr.AppendLine("          ,A1.INTAKEDATE                                                                             ")
        SQLStr.AppendLine("          ,A3.NAME AS ORGNAME                                                                        ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNT0026_SEIKYUHIST A1                                                                  ")
        SQLStr.AppendLine("      INNER JOIN                                                                                     ")
        SQLStr.AppendLine("            (                                                                                        ")
        SQLStr.AppendLine("               SELECT                                                                                ")
        SQLStr.AppendLine("                 TAISHOYM                                                                            ")
        SQLStr.AppendLine("                ,TORICODE                                                                            ")
        SQLStr.AppendLine("                ,MAX(SEQ) AS  SEQ                                                                    ")
        SQLStr.AppendLine("               FROM                                                                                  ")
        SQLStr.AppendLine("                 LNG.LNT0026_SEIKYUHIST                                                              ")
        SQLStr.AppendLine("               WHERE DELFLG <> '1'                                                                   ")
        SQLStr.AppendLine("                 AND TAISHOYM = @TAISHOYM                                                            ")
        SQLStr.AppendLine("               GROUP BY                                                                              ")
        SQLStr.AppendLine("                 TAISHOYM                                                                            ")
        SQLStr.AppendLine("                ,TORICODE                                                                            ")
        SQLStr.AppendLine("            )A2                                                                                      ")
        SQLStr.AppendLine("         ON  A1.TAISHOYM  =  A2.TAISHOYM                                                             ")
        SQLStr.AppendLine("        AND  A1.TORICODE = A2.TORICODE                                                               ")
        SQLStr.AppendLine("        AND  A1.SEQ = A2.SEQ                                                                         ")
        SQLStr.AppendLine("      LEFT JOIN                                                                                      ")
        SQLStr.AppendLine("            (                                                                                        ")
        SQLStr.AppendLine("               SELECT                                                                                ")
        SQLStr.AppendLine("                 US.USERID                                                                           ")
        SQLStr.AppendLine("                ,ORG.NAME                                                                            ")
        SQLStr.AppendLine("               FROM COM.LNS0001_USER US                                                              ")
        SQLStr.AppendLine("               INNER JOIN                                                                            ")
        SQLStr.AppendLine("               LNM0002_ORG ORG                                                                       ")
        SQLStr.AppendLine("               ON US.CAMPCODE = ORG.CAMPCODE                                                         ")
        SQLStr.AppendLine("              AND US.ORG = ORG.ORGCODE                                                               ")
        SQLStr.AppendLine("              AND CURDATE() BETWEEN US.STYMD AND US.ENDYMD                                           ")
        SQLStr.AppendLine("              AND CURDATE() BETWEEN ORG.STYMD AND ORG.ENDYMD                                         ")
        SQLStr.AppendLine("              AND US.DELFLG <> '1'                                                                   ")
        SQLStr.AppendLine("              AND ORG.DELFLG <> '1'                                                                  ")
        SQLStr.AppendLine("            )A3                                                                                      ")
        SQLStr.AppendLine("         ON  A1.USERID  =  A3.USERID                                                                 ")
        SQLStr.AppendLine("    ) A                                                                                              ")
        SQLStr.AppendLine("      ON  FIX.KEYCODE = A.TORICODE                                                                   ")

        ''最終更新日時、最終更新者 
        'SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        'SQLStr.AppendLine("    (                                                                                                ")
        'SQLStr.AppendLine("      SELECT DISTINCT                                                                                ")
        'SQLStr.AppendLine("          B1.TORICODE                                                                                ")
        'SQLStr.AppendLine("          , B1.ORGCODE                                                                               ")
        'SQLStr.AppendLine("          , B2.UPDUSER                                                                               ")
        'SQLStr.AppendLine("          , B2.UPDYMD                                                                                ")
        'SQLStr.AppendLine("          , US.STAFFNAMES                                                                            ")
        'SQLStr.AppendLine("          FROM(                                                                                      ")
        'SQLStr.AppendLine("              SELECT                                                                                 ")
        'SQLStr.AppendLine("                  TORICODE                                                                           ")
        'SQLStr.AppendLine("                  , ORGCODE                                                                          ")
        'SQLStr.AppendLine("                  , MAX(UPDYMD) AS UPDYMD                                                            ")
        'SQLStr.AppendLine("              FROM                                                                                   ")
        'SQLStr.AppendLine("                  LNG.VIW0004_SEIKYUUPDUSER                                                          ")
        'SQLStr.AppendLine("              WHERE                                                                                  ")
        'SQLStr.AppendLine("                  (CASE  WHEN COALESCE(TARGETYM,'') = '' THEN @TAISHOYMD BETWEEN  STYMD AND ENDYMD   ")
        'SQLStr.AppendLine("                   ELSE  TARGETYM = @TAISHOYM                                                        ")
        'SQLStr.AppendLine("                   END) = TRUE                                                                       ")
        'SQLStr.AppendLine("              GROUP BY                                                                               ")
        'SQLStr.AppendLine("                  TORICODE                                                                           ")
        'SQLStr.AppendLine("                  , ORGCODE                                                                          ")
        'SQLStr.AppendLine("          ) B1                                                                                       ")
        'SQLStr.AppendLine("          INNER JOIN LNG.VIW0004_SEIKYUUPDUSER B2                                                    ")
        'SQLStr.AppendLine("      ON                                                                                             ")
        'SQLStr.AppendLine("           B1.TORICODE = B2.TORICODE                                                                 ")
        'SQLStr.AppendLine("      AND  B1.ORGCODE = B2.ORGCODE                                                                   ")
        'SQLStr.AppendLine("      AND  B1.UPDYMD = B2.UPDYMD                                                                     ")
        'SQLStr.AppendLine("      LEFT JOIN                                                                                      ")
        'SQLStr.AppendLine("      COM.LNS0001_USER US                                                                            ")
        'SQLStr.AppendLine("      ON  B2.UPDUSER = US.USERID                                                                     ")
        'SQLStr.AppendLine("    ) B                                                                                              ")
        'SQLStr.AppendLine("      ON  FIX.VALUE5 = B.TORICODE                                                                    ")
        'SQLStr.AppendLine("     AND  FIX.VALUE6 = B.ORGCODE                                                                     ")

        '特別料金
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT DISTINCT                                                                                ")
        SQLStr.AppendLine("          TORICODE                                                                                   ")
        SQLStr.AppendLine("          ,ORGCODE                                                                                   ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNM0014_SPRATE                                                                         ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("      AND TARGETYM = @TAISHOYM                                                                       ")
        SQLStr.AppendLine("    ) SPRATE                                                                                         ")
        SQLStr.AppendLine("      ON  FIX.VALUE5 = SPRATE.TORICODE                                                               ")
        SQLStr.AppendLine("     AND  FIX.VALUE6 = SPRATE.ORGCODE                                                                ")

        '単価調整
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT DISTINCT                                                                                          ")
        SQLStr.AppendLine("          TORICODE                                                                                   ")
        SQLStr.AppendLine("          ,ORGCODE                                                                                   ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNM0006_NEWTANKA                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("      AND @TAISHOYMD BETWEEN STYMD AND ENDYMD                                                        ")
        SQLStr.AppendLine("      AND BRANCHCODE > 1                                                                             ")
        SQLStr.AppendLine("    ) TANKA                                                                                          ")
        SQLStr.AppendLine("      ON  FIX.VALUE5 = TANKA.TORICODE                                                                ")
        SQLStr.AppendLine("     AND  FIX.VALUE6 = TANKA.ORGCODE                                                                 ")

        '固定費調整
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT DISTINCT                                                                                ")
        SQLStr.AppendLine("          TORICODE                                                                                   ")
        SQLStr.AppendLine("          ,ORGCODE                                                                                   ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNM0007_FIXED                                                                         ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("      AND TARGETYM = @TAISHOYM                                                                       ")
        SQLStr.AppendLine("    ) FIXED                                                                                         ")
        SQLStr.AppendLine("      ON  FIX.VALUE5 = FIXED.TORICODE                                                               ")
        SQLStr.AppendLine("     AND  FIX.VALUE6 = FIXED.ORGCODE                                                               ")


        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("      FIX.DELFLG = '0'                                                                               ")
        SQLStr.AppendLine("     AND  FIX.CLASS = 'INVOICE'                                                                      ")
        SQLStr.AppendLine("     AND  FIX.CAMPCODE = @CAMPCODE                                                                   ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     FIX.VALUE4                                                                                      ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim Itype As Integer
                Dim dt As DateTime

                ' 会社コード
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)  '会社コード
                P_CAMPCODE.Value = Master.USERCAMP


                '対象年月
                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                    P_TAISHOYM.Value = Itype
                End If

                '対象年月日
                If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                    Dim P_TAISHOYMD As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYMD", MySqlDbType.Date)
                    P_TAISHOYMD.Value = dt
                End If

                ''ロール
                'Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                'P_ROLE.Value = Master.ROLE_ORG



                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0002row As DataRow In LNT0002tbl.Rows
                    i += 1
                    LNT0002row("LINECNT") = i        'LINECNT

                    '最終更新日時、最終更新者取得
                    GETLASTUPDATEUSER(SQLcon,
                                      LNT0002row("TORICODE"), LNT0002row("ORGCODE"),
                                      LNT0002row("UPDYMD"), LNT0002row("UPDUSERNAME"))

                    LNT0002row("DETAIL") = CONST_BTNADD
                    LNT0002row("CONTROL") = CONST_BTNOUT
                    LNT0002row("HISTORY") = CONST_BTNREF

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0002L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    '''  最終更新日時、最終更新者取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GETLASTUPDATEUSER(ByVal SQLcon As MySqlConnection,
                                    ByVal WW_TORICODE As String, ByVal WW_ORGCODE As String,
                                    ByRef WW_UPDYMD As String, ByRef WW_UPDUSERNAME As String)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("            COALESCE(DATE_FORMAT(A2.UPDYMD, '%Y/%m/%d %H:%i:%s'), '')                 AS UPDYMD      ")
        SQLStr.AppendLine("          , COALESCE(US.STAFFNAMES,  A2.UPDUSER)                                      AS STAFFNAMES  ")
        SQLStr.AppendLine("          FROM(                                                                                      ")
        SQLStr.AppendLine("              SELECT                                                                                 ")
        SQLStr.AppendLine("                  TORICODE                                                                           ")
        SQLStr.AppendLine("                  , ORGCODE                                                                          ")
        SQLStr.AppendLine("                  , TARGETYM                                                                         ")
        SQLStr.AppendLine("                  , STYMD                                                                            ")
        SQLStr.AppendLine("                  , ENDYMD                                                                           ")
        SQLStr.AppendLine("                  , MAX(UPDYMD) AS UPDYMD                                                            ")
        SQLStr.AppendLine("              FROM                                                                                   ")
        SQLStr.AppendLine("                  LNG.VIW0004_SEIKYUUPDUSER                                                          ")
        SQLStr.AppendLine("              WHERE                                                                                  ")
        SQLStr.AppendLine("                  (CASE  WHEN COALESCE(TARGETYM,'') = '' THEN @TAISHOYMD BETWEEN  STYMD AND ENDYMD   ")
        SQLStr.AppendLine("                   ELSE  TARGETYM = @TAISHOYM                                                        ")
        SQLStr.AppendLine("                   END) = TRUE                                                                       ")
        SQLStr.AppendLine("                AND TORICODE = @TORICODE                                                             ")
        SQLStr.AppendLine("                AND ORGCODE = @ORGCODE                                                               ")
        SQLStr.AppendLine("              GROUP BY                                                                               ")
        SQLStr.AppendLine("                  TORICODE                                                                           ")
        SQLStr.AppendLine("                , ORGCODE                                                                            ")
        SQLStr.AppendLine("                , TARGETYM                                                                           ")
        SQLStr.AppendLine("                , STYMD                                                                              ")
        SQLStr.AppendLine("                , ENDYMD                                                                             ")
        SQLStr.AppendLine("          ) A1                                                                                       ")
        SQLStr.AppendLine("          INNER JOIN LNG.VIW0004_SEIKYUUPDUSER A2                                                    ")
        SQLStr.AppendLine("      ON                                                                                             ")
        SQLStr.AppendLine("           A1.TORICODE = A2.TORICODE                                                                 ")
        SQLStr.AppendLine("      AND  A1.ORGCODE = A2.ORGCODE                                                                   ")
        SQLStr.AppendLine("      AND  A1.TARGETYM = A2.TARGETYM                                                                 ")
        SQLStr.AppendLine("      AND  A1.STYMD = A2.STYMD                                                                       ")
        SQLStr.AppendLine("      AND  A1.ENDYMD = A2.ENDYMD                                                                     ")
        SQLStr.AppendLine("      AND  A1.UPDYMD = A2.UPDYMD                                                                     ")
        SQLStr.AppendLine("      LEFT JOIN                                                                                      ")
        SQLStr.AppendLine("      COM.LNS0001_USER US                                                                            ")
        SQLStr.AppendLine("      ON  A2.UPDUSER = US.USERID                                                                      ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim Itype As Integer
                Dim dt As DateTime

                '対象年月
                If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                    Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                    P_TAISHOYM.Value = Itype
                End If

                '対象年月日
                If DateTime.TryParse(WF_TaishoYm.Value & "/01", dt) Then
                    Dim P_TAISHOYMD As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYMD", MySqlDbType.Date)
                    P_TAISHOYMD.Value = dt
                End If

                '取引先コード
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
                P_TORICODE.Value = WW_TORICODE

                '部門コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)
                P_ORGCODE.Value = WW_ORGCODE

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                    If WW_Tbl.Rows.Count >= 1 Then
                        WW_UPDYMD = WW_Tbl.Rows(0)("UPDYMD").ToString
                        WW_UPDUSERNAME = WW_Tbl.Rows(0)("STAFFNAMES").ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0002L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    '''  参照ボタン押下時
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRefClick()
        Dim WW_LINECNT As Integer = 0
        Dim WW_HTML As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim WW_ROW As DataRow
        WW_ROW = LNT0002tbl.Rows(WW_LINECNT)

        Me.WF_HISTTITLE.Text = WW_ROW("TRANDETAILNAME")

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            HISTDataGet(SQLcon, WW_ROW)
        End Using

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0002tblHIST)
        '○並び順初期化
        TBLview.Sort = "LINECNT"

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "HIST"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlHISTListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing
    End Sub


    ''' <summary>
    '''  追加ボタン押下時
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonAddClick()
        Dim WW_LINECNT As Integer = 0
        Dim WW_HTML As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim WW_ROW As DataRow
        WW_ROW = LNT0002tbl.Rows(WW_LINECNT)

        work.WF_SEL_LINECNT.Text = WW_ROW("LINECNT")            '選択行
        '対象年月
        Dim Itype As Integer
        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
            work.WF_SEL_TARGETYM.Text = Itype
        End If
        work.WF_SEL_TORICODE.Text = WW_ROW("TORICODE")                                  '取引先コード

        '○ 請求明細追加画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)
        '        Server.Transfer("~/LNG/mas/LNM0014SprateHistory.aspx")

    End Sub

    Private Sub WF_ButtonAJUST_Click()
        Dim WW_LINECNT As Integer = 0
        '○ LINECNT取得
        Try
            Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim WW_ROW As DataRow
        WW_ROW = LNT0002tbl.Rows(WW_LINECNT)
        '選択行
        work.WF_SEL_LINECNT.Text = WW_ROW("LINECNT")
        '〇対象年月
        work.WF_SEL_TARGETYM.Text = WF_TaishoYm.Value
        Me.WF_TORIORG.SelectedIndex = Integer.Parse(WW_ROW("INDEXKEY").ToString())
        '〇取引コード
        work.WF_SEL_TORICODE.Text = Me.WF_TORIORG.SelectedItem.Text
        '〇部署コード
        work.WF_SEL_ORGCODE.Text = Me.WF_TORIORG.SelectedValue

        Dim WW_URL As String = ""
        work.GetURL("LNT0001AJ", WW_URL)
        Server.Transfer(WW_URL)
    End Sub


    ' ******************************************************************************
    ' ***  ボタン押下処理                                                        ***
    ' ******************************************************************************
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNT0002tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNT0002tbl)

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
        Dim TBLview As New DataView(LNT0002tbl)
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

        'Dim WW_DBDataCheck As String = ""

        ''○ LINECNT取得
        'Dim WW_LineCNT As Integer = 0
        'Try
        '    Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
        '    WW_LineCNT -= 1
        'Catch ex As Exception
        '    Exit Sub
        'End Try

        'work.WF_SEL_LINECNT.Text = LNT0002tbl.Rows(WW_LineCNT)("LINECNT")            '選択行

        'work.WF_SEL_TARGETYM.Text = LNT0002tbl.Rows(WW_LineCNT)("TARGETYM")                                  '対象年月
        'work.WF_SEL_TORICODE.Text = LNT0002tbl.Rows(WW_LineCNT)("TORICODE")                                  '取引先コード


        'work.WF_SEL_DELFLG.Text = LNT0002tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        'work.WF_SEL_TIMESTAMP.Text = LNT0002tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        'work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        ''○画面切替設定
        'WF_BOXChange.Value = "detailbox"

        ''○ 遷移先(登録画面)退避データ保存先の作成
        'WW_CreateXMLSaveFile()

        ''○ 画面表示データ保存(遷移先(登録画面)向け)
        'Master.SaveTable(LNT0002tbl, work.WF_SEL_INPTBL.Text)

        ''〇 排他チェック
        'Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
        '    ' DataBase接続
        '    SQLcon.Open()
        '    ' 排他チェック
        '    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
        '                    work.WF_SEL_TARGETYM.Text, work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
        '                    work.WF_SEL_GROUPID.Text, work.WF_SEL_DETAILID.Text)
        'End Using

        'If Not isNormal(WW_DBDataCheck) Then
        '    Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '    Exit Sub
        'End If

        ''○ 登録画面ページへ遷移
        'Master.TransitionPage(Master.USERCAMP)

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

    ''' <summary>
    ''' 荷主情報取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ShippersGet()
        ' ドロップダウンリスト（荷主）作成
        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
        GS0007FIXVALUElst.CLAS = "INVOICE"
        GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = "VALUE4 ASC"
        LNT0002Shippers = GS0007FIXVALUElst.GS0007FIXVALUETbl()
        If Not isNormal(GS0007FIXVALUElst.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "固定値取得エラー")
            Exit Sub
        End If

        'ログインユーザーの操作可能な組織コードを取得
        Dim orgList = GetOrgList(Master.ROLE_ORG)

        WF_TORI.Items.Clear()
        WF_TORI.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To LNT0002Shippers.Rows.Count - 1
            Dim wOrg As String = EditOrgCsv(LNT0002Shippers.Rows(i))
            Dim exists As Boolean = orgList.Any(Function(p) wOrg Like "*" + p + "*")
            If exists Then
                WF_TORI.Items.Add(New ListItem(LNT0002Shippers.Rows(i)("VALUE1"), LNT0002Shippers.Rows(i)("KEYCODE")))
            End If

        Next
        WF_TORI.SelectedIndex = 0

        WF_TORIEXL.Items.Clear()
        WF_TORIEXL.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To LNT0002Shippers.Rows.Count - 1
            Dim wOrg As String = EditOrgCsv(LNT0002Shippers.Rows(i))
            Dim exists As Boolean = orgList.Any(Function(p) wOrg Like "*" + p + "*")
            If exists Then
                WF_TORIEXL.Items.Add(New ListItem(LNT0002Shippers.Rows(i)("VALUE2"), LNT0002Shippers.Rows(i)("KEYCODE")))
            End If
        Next
        WF_TORIEXL.SelectedIndex = 0

        WF_FILENAME.Items.Clear()
        WF_FILENAME.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To LNT0002Shippers.Rows.Count - 1
            Dim wOrg As String = EditOrgCsv(LNT0002Shippers.Rows(i))
            Dim exists As Boolean = orgList.Any(Function(p) wOrg Like "*" + p + "*")
            If exists Then
                WF_FILENAME.Items.Add(New ListItem(LNT0002Shippers.Rows(i)("VALUE3"), LNT0002Shippers.Rows(i)("KEYCODE")))
            End If
        Next
        WF_FILENAME.SelectedIndex = 0

        '取引先、部署（部署は、カンマ区切りで複数あり）
        WF_TORIORG.Items.Clear()
        WF_TORIORG.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To LNT0002Shippers.Rows.Count - 1
            Dim wOrg As String = EditOrgCsv(LNT0002Shippers.Rows(i))
            Dim exists As Boolean = orgList.Any(Function(p) wOrg Like "*" + p + "*")
            If exists Then
                WF_TORIORG.Items.Add(New ListItem(LNT0002Shippers.Rows(i)("VALUE5"), wOrg))
            End If

        Next
        WF_TORIORG.SelectedIndex = 0
    End Sub
    Protected Function EditOrgCsv(ByVal iRow As DataRow) As String
        Dim rtnStr As String = ""

        For i As Integer = 6 To 20
            Dim colName As String = "VALUE" & i.ToString
            If iRow(colName) <> "" Then
                If rtnStr.Length = 0 Then
                    rtnStr = iRow(colName)
                Else
                    rtnStr += ","
                    rtnStr += iRow(colName)
                End If
            End If
        Next

        Return rtnStr
    End Function
    ''' <summary>
    ''' 操作可能な組織コードを取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetOrgList(ByVal iOrg As String) As List(Of String)

        Dim CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
        Dim oList As New List(Of String)

        '検索SQL文
        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                Dim SQLStr As String =
                     " SELECT " _
                   & "             rtrim(A.CODE)    AS CODE                " _
                   & " FROM        COM.LNS0005_ROLE A                      " _
                   & " WHERE                                               " _
                   & "           A.ROLE        = @P1                       " _
                   & "       and A.OBJECT      = @P2                       " _
                   & "       and A.PERMITCODE  = @P3                       " _
                   & "       and A.STYMD      <= @P4                       " _
                   & "       and A.ENDYMD     >= @P5                       " _
                   & "       and A.DELFLG     <> @P6                       " _
                   & " ORDER BY A.SEQ                                      "

                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 20).Value = iOrg
                        .Add("@P2", MySqlDbType.VarChar, 20).Value = C_ROLE_VARIANT.USER_ORG
                        .Add("@P3", MySqlDbType.Int16).Value = "2"
                        .Add("@P4", MySqlDbType.Date).Value = Date.Now
                        .Add("@P5", MySqlDbType.Date).Value = Date.Now
                        .Add("@P6", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With
                    Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    '権限コード初期値(権限なし)設定
                    While SQLdr.Read
                        oList.Add(SQLdr("CODE"))
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNT0002_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return oList
        End Try

        Return oList

    End Function

#Region "出力"
    ''' <summary>
    '''  出力ボタン押下時
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOutClick()
        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim WW_ROW As DataRow
        WW_ROW = LNT0002tbl.Rows(WW_LINECNT)

        '対象年月
        Dim Itype As Integer
        If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
            work.WF_SEL_TARGETYM.Text = Itype
        End If

        Me.WF_TORI.SelectedValue = WW_ROW("KEYCODE")
        Me.WF_TORIEXL.SelectedIndex = Me.WF_TORI.SelectedIndex
        Me.WF_FILENAME.SelectedIndex = Me.WF_TORI.SelectedIndex
        Me.WF_TORIORG.SelectedIndex = Me.WF_TORI.SelectedIndex

        '○ 出力データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            INVOICEDataGet(SQLcon)
        End Using

        WW_ErrSW = C_MESSAGE_NO.NORMAL
        If LNT0001tbl.Rows.Count = 0 Then
            Master.Output(C_MESSAGE_NO.CTN_SELECT_EXIST, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
            WW_ErrSW = C_MESSAGE_NO.CTN_SELECT_EXIST
            Exit Sub
        End If

        Dim selectOrgCode As String = Mid(Me.WF_TORI.SelectedValue, 1, 6)
        Dim CMNCHK As New CmnCheck(Me.WF_TaishoYm.Value, Me.WF_TORI)
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 _
            OrElse selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 Then
            '〇(帳票)項目チェック処理(ENEOS)
            'WW_ReportCheckEneos(Me.WF_TORI.SelectedItem.Text, selectOrgCode)
            CMNCHK.WW_ReportCheckEneos(Me.WF_TORI.SelectedItem.Text, selectOrgCode, LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate,
                                       LNT0001HachinoheSprate:=LNT0001HachinoheSprate, LNT0001EneosComfee:=LNT0001EneosComfee)

            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputReport(Master.MAPID, selectOrgCode, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001Calendar,
                                                                             printHachinoheSprateDataClass:=LNT0001HachinoheSprate,
                                                                             printEneosComfeeDataClass:=LNT0001EneosComfee,
                                                                             printTogouSprateDataClass:=LNT0001TogouSprate,
                                                                             printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                             taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputReport.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub
        End If

        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 _
            OrElse selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_022801 Then
            Dim iCalcNumber As Integer = 1000
            Dim selectOrgCodeSub As String = Me.WF_TORI.SelectedValue
            '★姫路を選択した場合(ENEOSとフォーマットが同一のため)
            If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_022801 Then
                '〇(帳票)項目チェック処理(ENEOS)
                'WW_ReportCheckEneos(Me.WF_TORI.SelectedItem.Text, selectOrgCode)
                CMNCHK.WW_ReportCheckEneos(Me.WF_TORI.SelectedItem.Text, selectOrgCode, LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate)
                iCalcNumber = 1
                selectOrgCodeSub = selectOrgCode
            Else
                '〇(帳票)項目チェック処理(DAIGAS)
                'WW_ReportCheckDaigas(Me.WF_TORI.SelectedItem.Text, selectOrgCode)
                CMNCHK.WW_ReportCheckDaigas(Me.WF_TORI.SelectedItem.Text, selectOrgCode, LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate)
            End If

            Dim dtDummy As New DataTable
            dtDummy.Columns.Add("RECOID", Type.GetType("System.Int32"))
            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputReport(Master.MAPID, selectOrgCodeSub, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001Calendar,
                                                                             printHachinoheSprateDataClass:=dtDummy,
                                                                             printEneosComfeeDataClass:=dtDummy,
                                                                             printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                             taishoYm:=Me.WF_TaishoYm.Value, calcNumber:=iCalcNumber)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputReport.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub
        End If

        '石油資源開発(本州分)新潟・庄内・東北・茨城
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_021502 Then
            '〇(帳票)項目チェック処理(石油資源開発)
            Dim dcNigata As New Dictionary(Of String, String)
            Dim dcSyonai As New Dictionary(Of String, String)
            Dim dcTouhoku As New Dictionary(Of String, String)
            Dim dcIbaraki As New Dictionary(Of String, String)
            'WW_ReportCheckSekiyuSigen(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcNigata, dcSyonai, dcTouhoku, dcIbaraki)
            CMNCHK.WW_ReportCheckSekiyuSigen(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcNigata, dcSyonai, dcTouhoku, dcIbaraki,
                                             LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001SKSurcharge, LNT0001SKKoteichi, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate)

            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputSEKIYUSIGEN(Master.MAPID, selectOrgCode, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text,
                                                                                  LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001SKSurcharge, LNT0001Calendar, LNT0001SKKoteichi, dcNigata, dcSyonai, dcTouhoku, dcIbaraki,
                                                                                  printTogouSprateDataClass:=LNT0001TogouSprate,
                                                                                  printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                                  taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputReport.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub
        End If

        '石油資源開発(北海道)石狩
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇(帳票)項目チェック処理(石油資源開発(北海道))
            Dim dcIshikari As New Dictionary(Of String, String)
            'WW_ReportCheckSekiyuSigenHokaido(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcIshikari)
            CMNCHK.WW_ReportCheckSekiyuSigenHokaido(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcIshikari,
                                                    LNT0001tbl, LNT0001Tanktbl, LNT0001SKSprate, LNT0001SKSurcharge, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate)

            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputSEKIYUSIGENHokaido(Master.MAPID, selectOrgCode, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text,
                                                                                         LNT0001tbl, LNT0001Tanktbl, LNT0001SKSprate, LNT0001SKSurcharge, LNT0001Calendar, dcIshikari,
                                                                                         printTogouSprateDataClass:=LNT0001TogouSprate,
                                                                                         printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                                         taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputReport.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub
        End If

        'シーエナジー・エルネス
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_022302 Then
            '〇(帳票)項目チェック処理(シーエナジー・エルネス)
            Dim dcCenergy As New Dictionary(Of String, String)
            Dim dcElNess As New Dictionary(Of String, String)
            'WW_ReportCheckCenergyElNess(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcCenergy, dcElNess)
            CMNCHK.WW_ReportCheckCenergyElNess(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcCenergy, dcElNess,
                                               LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate)

            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputCENERGY_ELNESS(Master.MAPID, selectOrgCode, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text,
                                                                             LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001Calendar, dcCenergy, dcElNess,
                                                                             printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                             taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputReport.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub

        End If

        'エスジーリキッドサービス（西部ガス）
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_024001 Then
            '〇(帳票)項目チェック処理(西部ガス)
            Dim LNT0001InvoiceOutputSAIBU As New LNT0001InvoiceOutputSAIBU(Master.MAPID, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, LNT0001tbl, taishoYm:=Me.WF_TaishoYm.Value)

            Dim url As String
            Try
                url = LNT0001InvoiceOutputSAIBU.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub
        End If

        '東北天然ガス
        If Me.WF_TORI.SelectedValue = BaseDllConst.CONST_ORDERORGCODE_020402 & "01" Then

            Dim LNT0001InvoiceOutputTNG As New LNT0001InvoiceOutputTNG(Master.MAPID, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, Master.USERID, Master.USERTERMID, taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputTNG.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub

        End If

        '東北電力
        If Me.WF_TORI.SelectedValue = BaseDllConst.CONST_ORDERORGCODE_020402 & "02" Then

            Dim LNT0001InvoiceOutputTOHOKU As New LNT0001InvoiceOutputTOHOKU(Master.MAPID, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, Master.USERID, Master.USERTERMID, taishoYm:=Me.WF_TaishoYm.Value)
            Dim url As String
            Try
                url = LNT0001InvoiceOutputTOHOKU.CreateExcelPrintData()
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            Exit Sub

        End If

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()        '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = LNT0001tbl                       'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT", needsPopUp:=True)
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)



    End Sub

    ''' <summary>
    ''' 出力データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub INVOICEDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0001tbl) Then
            LNT0001tbl = New DataTable
        End If

        If LNT0001tbl.Columns.Count <> 0 Then
            LNT0001tbl.Columns.Clear()
        End If

        LNT0001tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As String =
              " Select                                                                            " _
            & "      1                                                    AS 'SELECT'             " _
            & "     ,0                                                    AS HIDDEN               " _
            & "     ,0                                                    AS LINECNT              " _
            & "     ,''                                                   AS OPERATION            " _
            & "     ,coalesce(LT1.RECONO, '')                             AS RECONO			    " _
            & "     ,coalesce(LT1.LOADUNLOTYPE, '')                       AS LOADUNLOTYPE		    " _
            & "     ,coalesce(LT1.STACKINGTYPE, '')                       AS STACKINGTYPE		    " _
            & "     ,coalesce(LT1.HSETID, '')                             AS HSETID			    " _
            & "     ,coalesce(LT1.ORDERORGSELECT, '')                     AS ORDERORGSELECT	    " _
            & "     ,coalesce(LT1.ORDERORGNAME, '')                       AS ORDERORGNAME		    " _
            & "     ,coalesce(LT1.ORDERORGCODE, '')                       AS ORDERORGCODE		    " _
            & "     ,coalesce(LT1.ORDERORGNAMES, '')                      AS ORDERORGNAMES	    " _
            & "     ,coalesce(LT1.KASANAMEORDERORG, '')                   AS KASANAMEORDERORG	    " _
            & "     ,coalesce(LT1.KASANCODEORDERORG, '')                  AS KASANCODEORDERORG	" _
            & "     ,coalesce(LT1.KASANAMESORDERORG, '')                  AS KASANAMESORDERORG	" _
            & "     ,coalesce(LT1.ORDERORG, '')                           AS ORDERORG				" _
            & "     ,coalesce(LT1.KASANORDERORG, '')                      AS KASANORDERORG		" _
            & "     ,coalesce(LT1.PRODUCTSLCT, '')                        AS PRODUCTSLCT			" _
            & "     ,coalesce(LT1.PRODUCTSYOSAI, '')                      AS PRODUCTSYOSAI		" _
            & "     ,coalesce(LT1.PRODUCT2NAME, '')                       AS PRODUCT2NAME			" _
            & "     ,coalesce(LT1.PRODUCT2, '')                           AS PRODUCT2				" _
            & "     ,coalesce(LT1.PRODUCT1NAME, '')                       AS PRODUCT1NAME			" _
            & "     ,coalesce(LT1.PRODUCT1, '')                           AS PRODUCT1				" _
            & "     ,coalesce(LT1.OILNAME, '')                            AS OILNAME				" _
            & "     ,coalesce(LT1.OILTYPE, '')                            AS OILTYPE				" _
            & "     ,coalesce(LT1.TODOKESLCT, '')                         AS TODOKESLCT			" _
            & "     ,coalesce(LT1.TODOKECODE, '')                         AS TODOKECODE			" _
            & "     ,coalesce(LT1.TODOKENAME, '')                         AS TODOKENAME			" _
            & "     ,coalesce(LT1.TODOKENAMES, '')                        AS TODOKENAMES			" _
            & "     ,coalesce(LT1.TORICODE, '')                           AS TORICODE				" _
            & "     ,coalesce(LT1.TORINAME, '')                           AS TORINAME				" _
            & "     ,coalesce(LT1.TODOKEADDR, '')                         AS TODOKEADDR			" _
            & "     ,coalesce(LT1.TODOKETEL, '')                          AS TODOKETEL			" _
            & "     ,coalesce(LT1.TODOKEMAP, '')                          AS TODOKEMAP			" _
            & "     ,coalesce(LT1.TODOKEIDO, '')                          AS TODOKEIDO			" _
            & "     ,coalesce(LT1.TODOKEKEIDO, '')                        AS TODOKEKEIDO			" _
            & "     ,coalesce(LT1.TODOKEBIKO1, '')                        AS TODOKEBIKO1			" _
            & "     ,coalesce(LT1.TODOKEBIKO2, '')                        AS TODOKEBIKO2			" _
            & "     ,coalesce(LT1.TODOKEBIKO3, '')                        AS TODOKEBIKO3			" _
            & "     ,coalesce(LT1.TODOKECOLOR1, '')                       AS TODOKECOLOR1			" _
            & "     ,coalesce(LT1.TODOKECOLOR2, '')                       AS TODOKECOLOR2			" _
            & "     ,coalesce(LT1.TODOKECOLOR3, '')                       AS TODOKECOLOR3			" _
            & "     ,coalesce(LT1.SHUKASLCT, '')                          AS SHUKASLCT			" _
            & "     ,coalesce(LT1.SHUKABASHO, '')                         AS SHUKABASHO			" _
            & "     ,coalesce(LT1.SHUKANAME, '')                          AS SHUKANAME			" _
            & "     ,coalesce(LT1.SHUKANAMES, '')                         AS SHUKANAMES			" _
            & "     ,coalesce(LT1.SHUKATORICODE, '')                      AS SHUKATORICODE		" _
            & "     ,coalesce(LT1.SHUKATORINAME, '')                      AS SHUKATORINAME		" _
            & "     ,coalesce(LT1.SHUKAADDR, '')                          AS SHUKAADDR			" _
            & "     ,coalesce(LT1.SHUKAADDRTEL, '')                       AS SHUKAADDRTEL			" _
            & "     ,coalesce(LT1.SHUKAMAP, '')                           AS SHUKAMAP				" _
            & "     ,coalesce(LT1.SHUKAIDO, '')                           AS SHUKAIDO				" _
            & "     ,coalesce(LT1.SHUKAKEIDO, '')                         AS SHUKAKEIDO			" _
            & "     ,coalesce(LT1.SHUKABIKOU1, '')                        AS SHUKABIKOU1			" _
            & "     ,coalesce(LT1.SHUKABIKOU2, '')                        AS SHUKABIKOU2			" _
            & "     ,coalesce(LT1.SHUKABIKOU3, '')                        AS SHUKABIKOU3			" _
            & "     ,coalesce(LT1.SHUKACOLOR1, '')                        AS SHUKACOLOR1			" _
            & "     ,coalesce(LT1.SHUKACOLOR2, '')                        AS SHUKACOLOR2			" _
            & "     ,coalesce(LT1.SHUKACOLOR3, '')                        AS SHUKACOLOR3			" _
            & "     ,coalesce(LT1.SHUKADATE, '')                          AS SHUKADATE			" _
            & "     ,coalesce(LT1.LOADTIME, '')                           AS LOADTIME				" _
            & "     ,coalesce(LT1.LOADTIMEIN, '')                         AS LOADTIMEIN			" _
            & "     ,coalesce(LT1.LOADTIMES, '')                          AS LOADTIMES			" _
            & "     ,coalesce(LT1.TODOKEDATE, '')                         AS TODOKEDATE			" _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.SYAGATA, ''),coalesce(LT1.SHUKADATE_MG, '') ORDER BY coalesce(LT1.SYAGATA, ''),coalesce(LT1.SHUKADATE, ''),coalesce(LT1.TODOKEDATE, '') ) TODOKEDATE_ROWNUM " _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.TODOKECODE, ''),coalesce(LT1.TODOKEDATE, '') ORDER BY coalesce(LT1.TODOKECODE, ''),coalesce(LT1.TODOKEDATE, ''),coalesce(LT1.SHITEITIMES, '') ) TODOKEDATE_ORDER " _
            & "     ,coalesce(LT1.SHITEITIME, '')                         AS SHITEITIME			" _
            & "     ,coalesce(LT1.SHITEITIMEIN, '')                       AS SHITEITIMEIN			" _
            & "     ,coalesce(LT1.SHITEITIMES, '')                        AS SHITEITIMES			" _
            & "     ,coalesce(LT1.ZYUTYU, '')                             AS ZYUTYU				" _
            & "     ,coalesce(LT1.ZISSEKI, '')                            AS ZISSEKI				" _
            & "     ,coalesce(LT1.TANNI, '')                              AS TANNI				" _
            & "     ,coalesce(LT1.GYOUMUSIZI1, '')                        AS GYOUMUSIZI1			" _
            & "     ,coalesce(LT1.GYOUMUSIZI2, '')                        AS GYOUMUSIZI2			" _
            & "     ,coalesce(LT1.GYOUMUSIZI3, '')                        AS GYOUMUSIZI3			" _
            & "     ,coalesce(LT1.NINUSHIBIKOU, '')                       AS NINUSHIBIKOU			" _
            & "     ,coalesce(LT1.GYOMUSYABAN, '')                        AS GYOMUSYABAN			" _
            & "     ,coalesce(LT1.SHIPORGNAME, '')                        AS SHIPORGNAME			" _
            & "     ,coalesce(LT1.SHIPORG, '')                            AS SHIPORG				" _
            & "     ,coalesce(LT1.SHIPORGNAMES, '')                       AS SHIPORGNAMES			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAME, '')                   AS KASANSHIPORGNAME	    " _
            & "     ,coalesce(LT1.KASANSHIPORG, '')                       AS KASANSHIPORG			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAMES, '')                  AS KASANSHIPORGNAMES	" _
            & "     ,coalesce(LT1.TANKNUM, '')                            AS TANKNUM				" _
            & "     ,coalesce(LT1.TANKNUMBER, '')                         AS TANKNUMBER			" _
            & "     ,coalesce(LT1.SYAGATA, '')                            AS SYAGATA				" _
            & "     ,coalesce(LT1.SYABARA, '')                            AS SYABARA				" _
            & "     ,coalesce(LT1.NINUSHINAME, '')                        AS NINUSHINAME			" _
            & "     ,coalesce(LT1.CONTYPE, '')                            AS CONTYPE				" _
            & "     ,coalesce(LT1.PRO1SYARYOU, '')                        AS PRO1SYARYOU			" _
            & "     ,coalesce(LT1.TANKMEMO, '')                           AS TANKMEMO				" _
            & "     ,coalesce(LT1.TANKBIKOU1, '')                         AS TANKBIKOU1			" _
            & "     ,coalesce(LT1.TANKBIKOU2, '')                         AS TANKBIKOU2			" _
            & "     ,coalesce(LT1.TANKBIKOU3, '')                         AS TANKBIKOU3			" _
            & "     ,coalesce(LT1.TRACTORNUM, '')                         AS TRACTORNUM			" _
            & "     ,coalesce(LT1.TRACTORNUMBER, '')                      AS TRACTORNUMBER		" _
            & "     ,coalesce(LT1.TRIP, '')                               AS TRIP					" _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.TANKNUMBER, ''),coalesce(LT1.SHUKADATE_MG, '') ORDER BY coalesce(LT1.TANKNUMBER, ''),coalesce(LT1.SHUKADATE, ''),coalesce(LT1.TODOKEDATE, ''),coalesce(LT1.TRIP, '') ) TRIP_REP " _
            & "     ,coalesce(LT1.DRP, '')                                AS DRP					" _
            & "     ,coalesce(LT1.UNKOUMEMO, '')                          AS UNKOUMEMO			" _
            & "     ,coalesce(LT1.SHUKKINTIME, '')                        AS SHUKKINTIME			" _
            & "     ,coalesce(LT1.STAFFSLCT, '')                          AS STAFFSLCT			" _
            & "     ,coalesce(LT1.STAFFNAME, '')                          AS STAFFNAME			" _
            & "     ,coalesce(LT1.STAFFCODE, '')                          AS STAFFCODE			" _
            & "     ,coalesce(LT1.SUBSTAFFSLCT, '')                       AS SUBSTAFFSLCT			" _
            & "     ,coalesce(LT1.SUBSTAFFNAME, '')                       AS SUBSTAFFNAME			" _
            & "     ,coalesce(LT1.SUBSTAFFNUM, '')                        AS SUBSTAFFNUM			" _
            & "     ,coalesce(LT1.CALENDERMEMO1, '')                      AS CALENDERMEMO1		" _
            & "     ,coalesce(LT1.CALENDERMEMO2, '')                      AS CALENDERMEMO2		" _
            & "     ,coalesce(LT1.CALENDERMEMO3, '')                      AS CALENDERMEMO3		" _
            & "     ,coalesce(LT1.CALENDERMEMO4, '')                      AS CALENDERMEMO4		" _
            & "     ,coalesce(LT1.CALENDERMEMO5, '')                      AS CALENDERMEMO5		" _
            & "     ,coalesce(LT1.CALENDERMEMO6, '')                      AS CALENDERMEMO6		" _
            & "     ,coalesce(LT1.CALENDERMEMO7, '')                      AS CALENDERMEMO7		" _
            & "     ,coalesce(LT1.CALENDERMEMO8, '')                      AS CALENDERMEMO8		" _
            & "     ,coalesce(LT1.CALENDERMEMO9, '')                      AS CALENDERMEMO9		" _
            & "     ,coalesce(LT1.CALENDERMEMO10, '')                     AS CALENDERMEMO10		" _
            & "     ,coalesce(LT1.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM			" _
            & "     ,coalesce(LT1.YOUSYA, '')                             AS YOUSYA				" _
            & "     ,coalesce(LT1.RECOTITLE, '')                          AS RECOTITLE			" _
            & "     ,coalesce(LT1.SHUKODATE, '')                          AS SHUKODATE			" _
            & "     ,coalesce(LT1.KIKODATE, '')                           AS KIKODATE				" _
            & "     ,coalesce(LT1.KIKOTIME, '')                           AS KIKOTIME				" _
            & "     ,coalesce(LT1.CREWBIKOU1, '')                         AS CREWBIKOU1			" _
            & "     ,coalesce(LT1.CREWBIKOU2, '')                         AS CREWBIKOU2			" _
            & "     ,coalesce(LT1.SUBCREWBIKOU1, '')                      AS SUBCREWBIKOU1		" _
            & "     ,coalesce(LT1.SUBCREWBIKOU2, '')                      AS SUBCREWBIKOU2		" _
            & "     ,coalesce(LT1.SUBSHUKKINTIME, '')                     AS SUBSHUKKINTIME		" _
            & "     ,coalesce(LT1.CALENDERMEMO11, '')                     AS CALENDERMEMO11		" _
            & "     ,coalesce(LT1.CALENDERMEMO12, '')                     AS CALENDERMEMO12		" _
            & "     ,coalesce(LT1.CALENDERMEMO13, '')                     AS CALENDERMEMO13		" _
            & "     ,coalesce(LT1.SYABARATANNI, '')                       AS SYABARATANNI			" _
            & "     ,coalesce(LT1.TAIKINTIME, '')                         AS TAIKINTIME			" _
            & "     ,coalesce(LT1.SUBTIKINTIME, '')                       AS SUBTIKINTIME			" _
            & "     ,coalesce(LT1.KVTITLE, '')                            AS KVTITLE				" _
            & "     ,coalesce(LT1.KVZYUTYU, '')                           AS KVZYUTYU				" _
            & "     ,coalesce(LT1.KVZISSEKI, '')                          AS KVZISSEKI			" _
            & "     ,coalesce(LT1.KVCREW, '')                             AS KVCREW				" _
            & "     ,coalesce(LT1.CREWCODE, '')                           AS CREWCODE				" _
            & "     ,coalesce(LT1.SUBCREWCODE, '')                        AS SUBCREWCODE			" _
            & "     ,coalesce(LT1.KVSUBCREW, '')                          AS KVSUBCREW			" _
            & "     ,coalesce(LT1.ORDERHENKO, '')                         AS ORDERHENKO			" _
            & "     ,coalesce(LT1.RIKUUNKYOKU, '')                        AS RIKUUNKYOKU			" _
            & "     ,coalesce(LT1.BUNRUINUMBER, '')                       AS BUNRUINUMBER			" _
            & "     ,coalesce(LT1.HIRAGANA, '')                           AS HIRAGANA				" _
            & "     ,coalesce(LT1.ITIRENNUM, '')                          AS ITIRENNUM			" _
            & "     ,coalesce(LT1.TRACTER1, '')                           AS TRACTER1				" _
            & "     ,coalesce(LT1.TRACTER2, '')                           AS TRACTER2				" _
            & "     ,coalesce(LT1.TRACTER3, '')                           AS TRACTER3				" _
            & "     ,coalesce(LT1.TRACTER4, '')                           AS TRACTER4				" _
            & "     ,coalesce(LT1.TRACTER5, '')                           AS TRACTER5				" _
            & "     ,coalesce(LT1.TRACTER6, '')                           AS TRACTER6				" _
            & "     ,coalesce(LT1.TRACTER7, '')                           AS TRACTER7				" _
            & "     ,coalesce(LT1.HAISYAHUKA, '')                         AS HAISYAHUKA			" _
            & "     ,coalesce(LT1.HYOZIZYUNT, '')                         AS HYOZIZYUNT			" _
            & "     ,coalesce(LT1.HYOZIZYUNH, '')                         AS HYOZIZYUNH			" _
            & "     ,coalesce(LT1.HONTRACTER1, '')                        AS HONTRACTER1			" _
            & "     ,coalesce(LT1.HONTRACTER2, '')                        AS HONTRACTER2			" _
            & "     ,coalesce(LT1.HONTRACTER3, '')                        AS HONTRACTER3			" _
            & "     ,coalesce(LT1.HONTRACTER4, '')                        AS HONTRACTER4			" _
            & "     ,coalesce(LT1.HONTRACTER5, '')                        AS HONTRACTER5			" _
            & "     ,coalesce(LT1.HONTRACTER6, '')                        AS HONTRACTER6			" _
            & "     ,coalesce(LT1.HONTRACTER7, '')                        AS HONTRACTER7			" _
            & "     ,coalesce(LT1.HONTRACTER8, '')                        AS HONTRACTER8			" _
            & "     ,coalesce(LT1.HONTRACTER9, '')                        AS HONTRACTER9			" _
            & "     ,coalesce(LT1.HONTRACTER10, '')                       AS HONTRACTER10			" _
            & "     ,coalesce(LT1.HONTRACTER11, '')                       AS HONTRACTER11			" _
            & "     ,coalesce(LT1.HONTRACTER12, '')                       AS HONTRACTER12			" _
            & "     ,coalesce(LT1.HONTRACTER13, '')                       AS HONTRACTER13			" _
            & "     ,coalesce(LT1.HONTRACTER14, '')                       AS HONTRACTER14			" _
            & "     ,coalesce(LT1.HONTRACTER15, '')                       AS HONTRACTER15			" _
            & "     ,coalesce(LT1.HONTRACTER16, '')                       AS HONTRACTER16			" _
            & "     ,coalesce(LT1.HONTRACTER17, '')                       AS HONTRACTER17			" _
            & "     ,coalesce(LT1.HONTRACTER18, '')                       AS HONTRACTER18			" _
            & "     ,coalesce(LT1.HONTRACTER19, '')                       AS HONTRACTER19			" _
            & "     ,coalesce(LT1.HONTRACTER20, '')                       AS HONTRACTER20			" _
            & "     ,coalesce(LT1.HONTRACTER21, '')                       AS HONTRACTER21			" _
            & "     ,coalesce(LT1.HONTRACTER22, '')                       AS HONTRACTER22			" _
            & "     ,coalesce(LT1.HONTRACTER23, '')                       AS HONTRACTER23			" _
            & "     ,coalesce(LT1.HONTRACTER24, '')                       AS HONTRACTER24			" _
            & "     ,coalesce(LT1.HONTRACTER25, '')                       AS HONTRACTER25			" _
            & "     ,coalesce(LT1.CALENDERMEMO14, '')                     AS CALENDERMEMO14		" _
            & "     ,coalesce(LT1.CALENDERMEMO15, '')                     AS CALENDERMEMO15		" _
            & "     ,coalesce(LT1.CALENDERMEMO16, '')                     AS CALENDERMEMO16		" _
            & "     ,coalesce(LT1.CALENDERMEMO17, '')                     AS CALENDERMEMO17		" _
            & "     ,coalesce(LT1.CALENDERMEMO18, '')                     AS CALENDERMEMO18		" _
            & "     ,coalesce(LT1.CALENDERMEMO19, '')                     AS CALENDERMEMO19		" _
            & "     ,coalesce(LT1.CALENDERMEMO20, '')                     AS CALENDERMEMO20		" _
            & "     ,coalesce(LT1.CALENDERMEMO21 , '')                    AS CALENDERMEMO21		" _
            & "     ,coalesce(LT1.CALENDERMEMO22, '')                     AS CALENDERMEMO22		" _
            & "     ,coalesce(LT1.CALENDERMEMO23, '')                     AS CALENDERMEMO23		" _
            & "     ,coalesce(LT1.CALENDERMEMO24, '')                     AS CALENDERMEMO24		" _
            & "     ,coalesce(LT1.CALENDERMEMO25, '')                     AS CALENDERMEMO25		" _
            & "     ,coalesce(LT1.CALENDERMEMO26, '')                     AS CALENDERMEMO26		" _
            & "     ,coalesce(LT1.CALENDERMEMO27, '')                     AS CALENDERMEMO27		" _
            & "     ,coalesce(LT1.BRANCHCODE, '')                         AS BRANCHCODE		" _
            & "     ,coalesce(LT1.UPDATEUSER, '')                         AS UPDATEUSER			" _
            & "     ,coalesce(LT1.CREATEUSER, '')                         AS CREATEUSER			" _
            & "     ,coalesce(LT1.UPDATEYMD, '')                          AS UPDATEYMD			" _
            & "     ,coalesce(LT1.CREATEYMD, '')                          AS CREATEYMD			" _
            & "     ,coalesce(LT1.DELFLG, '')                             AS DELFLG				" _
            & "     ,coalesce(LT1.INITYMD, '')                            AS INITYMD				" _
            & "     ,coalesce(LT1.INITUSER, '')                           AS INITUSER				" _
            & "     ,coalesce(LT1.INITTERMID, '')                         AS INITTERMID			" _
            & "     ,coalesce(LT1.INITPGID, '')                           AS INITPGID				" _
            & "     ,coalesce(LT1.UPDYMD, '')                             AS UPDYMD				" _
            & "     ,coalesce(LT1.UPDUSER, '')                            AS UPDUSER				" _
            & "     ,coalesce(LT1.UPDTERMID, '')                          AS UPDTERMID			" _
            & "     ,coalesce(LT1.UPDPGID, '')                            AS UPDPGID				" _
            & "     ,coalesce(LT1.RECEIVEYMD, '')                         AS RECEIVEYMD			" _
            & "     ,coalesce(LT1.UPDTIMSTP, '')                          AS UPDTIMSTP			" _
            & " FROM (                                                                " _
            & " SELECT                                                                " _
            & "      LT1.*                                                            " _
            & "     ,CASE @P4 " _
            & "      WHEN DATE_FORMAT(LT1.SHUKADATE, '%Y/%m') THEN LT1.TODOKEDATE " _
            & "      ELSE LT1.SHUKADATE " _
            & "      END AS SHUKADATE_MG " _
            & " FROM                                                                " _
            & "     LNG.LNT0001_ZISSEKI LT1                                         " _
            & " WHERE                                                               " _
            & "     date_format(LT1.TODOKEDATE, '%Y/%m/%d') >= @P2                  " _
            & " AND date_format(LT1.TODOKEDATE, '%Y/%m/%d') <= @P3                  " _
            & " AND LT1.ZISSEKI <> 0                                                "

        '〇シーエナジー
        If Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022302 + "01" Then
            '★北陸エルネスも含める
            SQLStr &= String.Format(" AND LT1.TORICODE IN (@P5, '{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
        Else
            SQLStr &= " AND LT1.TORICODE = @P5                                              "
        End If
        SQLStr &= " AND LT1.ORDERORGCODE in (" & WF_TORIORG.SelectedValue & ")"

        '〇西日本支店車庫
        If Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022702 + "01" Then
            '★[Daigas泉北]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE <> '{0}' ", BaseDllConst.CONST_TODOKECODE_001640)
        ElseIf Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022702 + "02" Then
            '★[Daigas新宮]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE = '{0}' ", BaseDllConst.CONST_TODOKECODE_001640)
        ElseIf Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022702 + "03" Then
            '★[エスケイ産業]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE = '{0}' ", BaseDllConst.CONST_TODOKECODE_004559)
        End If

        SQLStr &= String.Format(" AND LT1.DELFLG = '{0}' ", BaseDllConst.C_DELETE_FLG.ALIVE)
        SQLStr &= " ) LT1                                                                "
        SQLStr &= " ORDER BY                                                            "
        SQLStr &= "     LT1.ORDERORGCODE, LT1.SHUKADATE, LT1.TODOKEDATE, LT1.TODOKECODE  "


        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar)  '部署
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)  '届日FROM
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)  '届日TO
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar)  '前月
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar)  '取引先コード
                'PARA1.Value = WF_TORIORG.SelectedValue
                PARA1.Value = WF_TORIORG.Items(WF_TORI.SelectedIndex).Value
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                    PARA2.Value = WF_TaishoYm.Value & "/01"
                    PARA3.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                Else
                    PARA2.Value = Date.Now.ToString("yyyy/MM") & "/01"
                    PARA3.Value = Date.Now.ToString("yyyy/MM") & DateTime.DaysInMonth(Date.Now.Year, Date.Now.Month).ToString("/00")
                End If
                Dim lastMonth As String = Date.Parse(Me.WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM")
                PARA4.Value = lastMonth
                'PARA5.Value = WF_TORIORG.SelectedItem.Text
                PARA5.Value = WF_TORIORG.Items(WF_TORI.SelectedIndex).Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0001row As DataRow In LNT0001tbl.Rows
                    i += 1
                    LNT0001row("LINECNT") = i        'LINECNT
                    LNT0001row("TANKNUMBER") = Replace(LNT0001row("TANKNUMBER").ToString(), Space(1), String.Empty)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001I SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001I Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(ENEOS)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ReportCheckEneos(ByVal reportName As String, ByVal reportCode As String)
        Dim dtEneosTank As New DataTable
        Dim dtEneosTodoke As New DataTable
        'Dim dtENEOSHachinoheTanka As New DataTable
        Dim eneosTankClass As String = ""
        Dim eneosTodokeClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                                    '--★不積(しきい値)

        Select Case reportCode
            '"ENEOS_八戸　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_020202
                eneosTankClass = "ENEOS_HACHINOHE_TANK"
                eneosTodokeClass = "AVOCADO_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0005700000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_020202

            '"ENEOS_水島　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_023301
                eneosTankClass = "ENEOS_MIZUSHIMA_TANK"
                eneosTodokeClass = "MIZUSHIMA_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0005700000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_023301

            '"DAIGAS_姫路　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022801
                eneosTankClass = "DAIGAS_HIMEGI_TANK"
                eneosTodokeClass = "HIMEGI_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0051200000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_022801
                arrToriCode(2) = Nothing

            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, eneosTankClass, dtEneosTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, eneosTodokeClass, dtEneosTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", eneosTodokeClass, LNT0001Tanktbl)
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", eneosTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001Koteihi, I_CLASS:=eneosTankClass)
            'CMNPTS.SelectKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001Koteihi, I_CLASS:=eneosTankClass)
            CMNPTS.SelectHACHINOHESPRATEMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001HachinoheSprate)
            CMNPTS.SelectENEOSCOMFEEMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001EneosComfee)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=arrToriCode(1))
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtEneosTodoke, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=eneosTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)
#Region "コメント"
        'LNT0001tbl.Columns.Add("ROWSORTNO", Type.GetType("System.Int32"))               '// 【入力用】EXCEL用ソート番号
        'LNT0001tbl.Columns.Add("SETCELL01", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(届先名)
        'LNT0001tbl.Columns.Add("SETCELL02", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(実績数量)
        'LNT0001tbl.Columns.Add("SETCELL03", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(備考)
        'LNT0001tbl.Columns.Add("SETLINE", Type.GetType("System.Int32"))                 '// 【入力用】EXCEL用(行数)
        'LNT0001tbl.Columns.Add("TODOKENAME_REP", Type.GetType("System.String"))         '// 【入力用】EXCEL用(届先名)
        'LNT0001tbl.Columns.Add("REMARK_REP", Type.GetType("System.String"))             '// 【入力用】EXCEL用(備考)
        'LNT0001tbl.Columns.Add("DISPLAYCELL_START", Type.GetType("System.String"))      '// 【入力用】EXCEL用(陸事番号)設定用
        'LNT0001tbl.Columns.Add("DISPLAYCELL_END", Type.GetType("System.String"))        '// 【入力用】EXCEL用(受注数量)設定用
        'LNT0001tbl.Columns.Add("DISPLAYCELL_KOTEICHI", Type.GetType("System.String"))   '// 【固定費】EXCEL用(陸事番号)表示用
        'LNT0001tbl.Columns.Add("TODOKECELL_REP", Type.GetType("System.String"))         '// 【届先毎】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("MASTERCELL_REP", Type.GetType("System.String"))         '// 【マスタ】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("SHEETDISPLAY_REP", Type.GetType("System.String"))       '// EXCELシート(届先名)表示用
        'LNT0001tbl.Columns.Add("SHEETSORTNO_REP", Type.GetType("System.Int32"))         '// EXCELシート(届先名)ソート用
        'LNT0001tbl.Columns.Add("SHEETNAME_REP", Type.GetType("System.String"))          '// EXCELシート(届先名)設定用
        'LNT0001tbl.Columns.Add("ZISSEKI_FUZUMI", Type.GetType("System.Decimal"))        '// EXCELシート①(車腹 - 不積(しきい値))設定用
        'LNT0001tbl.Columns.Add("FUZUMI_REFVALUE", Type.GetType("System.Decimal"))       '// EXCELシート②(① - 実績数量)設定用
        'LNT0001tbl.Columns.Add("ZISSEKI_FUZUMIFLG", Type.GetType("System.String"))      '// EXCELシート(不積フラグ)設定用
#End Region

        '〇陸事番号マスタ設定
        For Each dtEneosTankrow As DataRow In dtEneosTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtEneosTankrow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                '★届日より日を取得(セル(行数)の設定のため)
                Dim setDay As String = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("dd")
                Dim lastMonth As Boolean = False
                If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                    setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                    lastMonth = True
                End If
                Dim iLine As Integer = Integer.Parse(setDay) - 1
                iLine = (iLine * Integer.Parse(dtEneosTankrow("VALUE06"))) + Integer.Parse(dtEneosTankrow("VALUE05"))
                '★トリップより位置を取得
                Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
                iTrip += iLine
                LNT0001tblrow("ROWSORTNO") = dtEneosTankrow("VALUE01")
                LNT0001tblrow("SETCELL01") = dtEneosTankrow("VALUE02") + iTrip.ToString()
                LNT0001tblrow("SETCELL02") = dtEneosTankrow("VALUE03") + iTrip.ToString()
                LNT0001tblrow("SETCELL03") = dtEneosTankrow("VALUE04") + iTrip.ToString()
                LNT0001tblrow("SETLINE") = iTrip.ToString()

                '# 不積の判断 ----------------------------------------------------------------------------
                Dim todokeCode As String = LNT0001tblrow("TODOKECODE").ToString()
                Dim decFuzumi As Decimal = Decimal.Parse(LNT0001tblrow("SYABARA").ToString()) - fuzumiLimit
                Dim decZisseki As Decimal = Decimal.Parse(LNT0001tblrow("ZISSEKI").ToString())
                LNT0001tblrow("ZISSEKI_FUZUMI") = decFuzumi
                LNT0001tblrow("FUZUMI_REFVALUE") = decFuzumi - decZisseki
                If Decimal.Parse(LNT0001tblrow("FUZUMI_REFVALUE").ToString()) >= 0 Then
                    LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "TRUE"
                Else
                    LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "FALSE"
                End If
                ' ---------------------------------------------------------------------------------------/

                '★表示セルフラグ(1:表示)
                If dtEneosTankrow("VALUE07").ToString() = "1" Then
                    LNT0001tblrow("DISPLAYCELL_START") = dtEneosTankrow("VALUE02").ToString()
                    LNT0001tblrow("DISPLAYCELL_END") = dtEneosTankrow("VALUE04").ToString()
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtEneosTankrow("VALUE08").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_START") = ""
                    LNT0001tblrow("DISPLAYCELL_END") = ""
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
                End If

                '★備考設定用(出荷日と届日が不一致の場合)
                If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                    If lastMonth = True Then
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                    Else
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                    End If
                End If
            Next
        Next

        '〇(ENEOS)届先出荷場所車庫マスタ設定
        For Each dtEneosTodokerow As DataRow In dtEneosTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", dtEneosTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = dtEneosTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = dtEneosTodokerow("VALUE06")
#Region "コメント"
                ''〇八戸営業所(東部瓦斯)独自仕様
                'If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005487 Then
                '    '★[３台目]に納入
                '    If LNT0001tblrow("TODOKEDATE_ORDER").ToString() = "3" Then
                '        LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01") + LNT0001tblrow("TODOKEDATE_ORDER").ToString()
                '    End If
                'End If
                ''〇水島営業所 ----------------------------------------------------------------------------
                ''■コカ・コーラボトラーズジャパン株式会社(独自仕様)
                'If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005509 Then
                '    '--コカ・コーラ　ボトラーズジャパン(12.3t, 12.5t, 13.2t, 14t, 不積)
                '    Dim arrFuriwake005509 As String() = {"②", "③", "④", "不積"}

                '    '-- 不積判定の設定
                '    If LNT0001tblrow("ZISSEKI_FUZUMIFLG").ToString() = "TRUE" Then
                '        LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01") + arrFuriwake005509(3)
                '    End If
                'End If
                '' ---------------------------------------------------------------------------------------/
#End Region
                '〇届先が追加された場合
                If dtEneosTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = dtEneosTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = dtEneosTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = dtEneosTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

        ''○各シート(届先名)抽出処理
        'Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()  ' DataBase接続
        '    '〇実績WORK作成
        '    WW_InsertHachinoheMoment(SQLcon, reportName)
        'End Using

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(DAIGAS)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ReportCheckDaigas(ByVal reportName As String, ByVal reportCode As String)
        Dim dtDaigasTank As New DataTable
        Dim dtDaigasTodoke As New DataTable
        Dim daigasTankClass As String = ""
        Dim daigasTodokeClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}

        Select Case reportCode
            '"DAIGAS_泉北　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022702
                daigasTankClass = "DAIGAS_SENBOKU_TANK"
                daigasTodokeClass = "SENBOKU_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0051200000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_022702
                '"DAIGAS_新宮　輸送費請求書"を指定した場合
                If Me.WF_TORI.SelectedValue = "02270202" Then
                    daigasTodokeClass = "NIIMIYA_TODOKE_MAS"
                    arrToriCode(2) = BaseDllConst.CONST_TODOKECODE_001640
                ElseIf Me.WF_TORI.SelectedValue = "02270203" Then
                    '"エスケイ産業　輸送費請求書"を指定した場合
                    daigasTankClass = "DAIGAS_ESUKEI_TANK"
                    daigasTodokeClass = "ESUKEI_TODOKE_MAS"
                    arrToriCode(0) = BaseDllConst.CONST_TORICODE_0045200000
                    arrToriCode(2) = BaseDllConst.CONST_TODOKECODE_004559
                Else
                    arrToriCode(2) = Nothing
                End If

            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, daigasTankClass, dtDaigasTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, daigasTodokeClass, dtDaigasTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", daigasTodokeClass, LNT0001Tanktbl, I_TODOKECODE:=arrToriCode(2))
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", daigasTodokeClass, LNT0001Tanktbl, I_TODOKECODE:=arrToriCode(2))
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001Koteihi, I_CLASS:=daigasTankClass)
            'CMNPTS.SelectKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001Koteihi, I_CLASS:=daigasTankClass)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtDaigasTodoke, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=daigasTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)
#Region "コメント"
        'LNT0001tbl.Columns.Add("ROWSORTNO", Type.GetType("System.Int32"))               '// 【入力用】EXCEL用ソート番号
        'LNT0001tbl.Columns.Add("SETCELL01", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(届先名)
        'LNT0001tbl.Columns.Add("SETCELL02", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(実績数量)
        'LNT0001tbl.Columns.Add("SETCELL03", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(備考)
        'LNT0001tbl.Columns.Add("SETLINE", Type.GetType("System.Int32"))                 '// 【入力用】EXCEL用(行数)
        'LNT0001tbl.Columns.Add("TODOKENAME_REP", Type.GetType("System.String"))         '// 【入力用】EXCEL用(届先名)
        'LNT0001tbl.Columns.Add("REMARK_REP", Type.GetType("System.String"))             '// 【入力用】EXCEL用(備考)
        'LNT0001tbl.Columns.Add("DISPLAYCELL_START", Type.GetType("System.String"))      '// 【入力用】EXCEL用(陸事番号)設定用
        'LNT0001tbl.Columns.Add("DISPLAYCELL_END", Type.GetType("System.String"))        '// 【入力用】EXCEL用(受注数量)設定用
        'LNT0001tbl.Columns.Add("DISPLAYCELL_KOTEICHI", Type.GetType("System.String"))   '// 【固定費】EXCEL用(陸事番号)表示用
        'LNT0001tbl.Columns.Add("TODOKECELL_REP", Type.GetType("System.String"))         '// 【届先毎】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("MASTERCELL_REP", Type.GetType("System.String"))         '// 【マスタ】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("SHEETDISPLAY_REP", Type.GetType("System.String"))       '// EXCELシート(届先名)表示用
        'LNT0001tbl.Columns.Add("SHEETSORTNO_REP", Type.GetType("System.Int32"))         '// EXCELシート(届先名)ソート用
        'LNT0001tbl.Columns.Add("SHEETNAME_REP", Type.GetType("System.String"))          '// EXCELシート(届先名)設定用
#End Region

        '〇陸事番号マスタ設定
        For Each dtDaigasTankrow As DataRow In dtDaigasTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtDaigasTankrow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                '★届日より日を取得(セル(行数)の設定のため)
                Dim setDay As String = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("dd")
                Dim lastMonth As Boolean = False
                If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                    setDay = "1"
                    lastMonth = True
                End If
                Dim iLine As Integer = Integer.Parse(setDay) - 1
                iLine = (iLine * Integer.Parse(dtDaigasTankrow("VALUE06"))) + Integer.Parse(dtDaigasTankrow("VALUE05"))
                ''★トリップより位置を取得
                'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
                'iTrip += iLine

                LNT0001tblrow("ROWSORTNO") = dtDaigasTankrow("VALUE01")
                If LNT0001tblrow("TODOKEDATE_ROWNUM") = "1" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE02") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE03") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "2" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE04") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE09") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "3" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE10") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE11") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "4" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE12") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE13") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "5" Then
                    '★単車の枠が４つしかないが５つあった場合はSKIP
                    If LNT0001tblrow("SYAGATA") = "単車" Then Continue For
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE14") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE15") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "6" Then
                    '★単車の枠が４つしかないが６つあった場合はSKIP
                    If LNT0001tblrow("SYAGATA") = "単車" Then Continue For
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE16") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE17") + iLine.ToString()
                Else
                    '★枠が７つ以上の場合はSKIP
                    Continue For
                End If
                LNT0001tblrow("SETLINE") = iLine

                '★表示セルフラグ(1:表示)
                If dtDaigasTankrow("VALUE07").ToString() = "1" Then
                    LNT0001tblrow("DISPLAYCELL_START") = dtDaigasTankrow("VALUE02").ToString()
                    LNT0001tblrow("DISPLAYCELL_END") = dtDaigasTankrow("VALUE04").ToString()
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtDaigasTankrow("VALUE08").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_START") = ""
                    LNT0001tblrow("DISPLAYCELL_END") = ""
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
                End If

                '★備考設定用(出荷日と届日が不一致の場合)
                If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                    If lastMonth = True Then
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                    Else
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                    End If
                End If
            Next
        Next

        '〇(DAIGAS)届先出荷場所車庫マスタ設定
        For Each dtDaigasTodokerow As DataRow In dtDaigasTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", dtDaigasTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = dtDaigasTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = dtDaigasTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = dtDaigasTodokerow("VALUE06")

                '〇届先が追加された場合
                If dtDaigasTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = dtDaigasTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = dtDaigasTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = dtDaigasTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(石油資源開発(本州分))
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ReportCheckSekiyuSigen(ByVal reportName As String, ByVal reportCode As String,
                                           ByRef dcNigataList As Dictionary(Of String, String), ByRef dcSyonaiList As Dictionary(Of String, String),
                                           ByRef dcTouhokuList As Dictionary(Of String, String), ByRef dcIbarakiList As Dictionary(Of String, String))
        Dim dtSekiyuSigenTank As New DataTable
        Dim dtSekiyuSigenTankSub As New DataTable
        Dim dtSekiyuSigenTodoke As New DataTable
        Dim sekiyuSigenTankClass As String = ""
        Dim sekiyuSigenTankSubClass As String = ""
        Dim sekiyuSigenTodokeClass As String = ""
        Dim sekiyuSigenSGKoteihiClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim listOrderOrgCode As New List(Of String)
        Dim commaOrderOrgCode As String = ""
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)
        Dim arrFuzumi002022_302 As String() = {"T", "U"}    '--（ＳＫ）本田金属　喜多方サテライト(302号車(11.4t車)不積)
        Dim arrFuzumi002019_333 As String() = {"T", "U"}    '--（ＳＫ）テーブルマーク　塩沢      (333号車(14.0t車)不積)
        Dim arrFuzumi002019_334 As String() = {"Z", "AA"}   '--（ＳＫ）テーブルマーク　塩沢      (334号車(15.7t車)不積)

        Dim arrOPFCycle_002025_326 As String() = {"Z", "AA"}    ' （ＳＫ）若松ガス　玉川(326号車(若松1.5回転))

        Select Case reportCode
            '"石油資源開発　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_021502
                sekiyuSigenTankClass = "SEKIYUSIGEN_TANK"
                sekiyuSigenTankSubClass = "SEKIYUSIGEN_TANK_OTR"
                sekiyuSigenTodokeClass = "SEKIYUSIG_TODOKE_MAS"
                sekiyuSigenSGKoteihiClass = "SEKIYUSIGEN_KOTEIHI"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0132800000
                arrToriCode(1) = Nothing
                arrToriCode(2) = Nothing
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021502)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020601)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020402)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020804)
                commaOrderOrgCode = String.Join(",", listOrderOrgCode)
            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankClass, dtSekiyuSigenTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankSubClass, dtSekiyuSigenTankSub)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTodokeClass, dtSekiyuSigenTodoke)
            'CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", sekiyuSigenTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", sekiyuSigenTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectSKFIXEDMaster(SQLcon, arrToriCode(0), commaOrderOrgCode, Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001Koteihi, I_CLASS:=sekiyuSigenSGKoteihiClass)
            'CMNPTS.SelectSKKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001Koteihi, I_CLASS:=sekiyuSigenSGKoteihiClass)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value + "/01", LNT0001Calendar)
            CMNPTS.SelectSKKOTEICHIMaster(SQLcon, LNT0001Tanktbl, LNT0001SKKoteichi)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=commaOrderOrgCode, I_CLASS:=sekiyuSigenTodokeClass)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtSekiyuSigenTodoke, I_ORDERORGCODE:=commaOrderOrgCode, I_SHUKABASHO:=arrToriCode(2), I_CLASS:=sekiyuSigenTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)
#Region "コメント"
        'LNT0001tbl.Columns.Add("ROWSORTNO", Type.GetType("System.Int32"))               '// 【入力用】EXCEL用ソート番号
        'LNT0001tbl.Columns.Add("SETCELL01", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(届先名)
        'LNT0001tbl.Columns.Add("SETCELL02", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(実績数量)
        ''LNT0001tbl.Columns.Add("SETCELL03", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(備考)
        'LNT0001tbl.Columns.Add("SETLINE", Type.GetType("System.Int32"))                 '// 【入力用】EXCEL用(行数)
        'LNT0001tbl.Columns.Add("TODOKENAME_REP", Type.GetType("System.String"))         '// 【入力用】EXCEL用(届先名)
        'LNT0001tbl.Columns.Add("REMARK_REP", Type.GetType("System.String"))             '// 【入力用】EXCEL用(備考)
        'LNT0001tbl.Columns.Add("DISPLAYCELL_START", Type.GetType("System.String"))      '// 【入力用】EXCEL用(陸事番号)設定用
        'LNT0001tbl.Columns.Add("DISPLAYCELL_END", Type.GetType("System.String"))        '// 【入力用】EXCEL用(受注数量)設定用
        ''LNT0001tbl.Columns.Add("DISPLAYCELL_KOTEICHI", Type.GetType("System.String"))   '// 【固定費】EXCEL用(陸事番号)表示用
        'LNT0001tbl.Columns.Add("TODOKECELL_REP", Type.GetType("System.String"))         '// 【届先毎】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("MASTERCELL_REP", Type.GetType("System.String"))         '// 【マスタ】EXCEL用(届先名)表示用
        'LNT0001tbl.Columns.Add("ORDERORGCODE_REP", Type.GetType("System.String"))       '// EXCELシート(受注受付部署コード)設定用
        'LNT0001tbl.Columns.Add("GYOMUTANKNUM_REP", Type.GetType("System.String"))       '// EXCELシート(業務車番)設定用
        'LNT0001tbl.Columns.Add("SHEETDISPLAY_REP", Type.GetType("System.String"))       '// EXCELシート(届先名)表示用
        'LNT0001tbl.Columns.Add("SHEETSORTNO_REP", Type.GetType("System.Int32"))         '// EXCELシート(届先名)ソート用
        'LNT0001tbl.Columns.Add("SHEETNAME_REP", Type.GetType("System.String"))          '// EXCELシート(届先名)設定用
        'LNT0001tbl.Columns.Add("GROUPNO_REP", Type.GetType("System.String"))            '// EXCELシート(届先GRP)設定用
        'LNT0001tbl.Columns.Add("ZISSEKI_FUZUMI", Type.GetType("System.Decimal"))        '// EXCELシート①(車腹 - 不積(しきい値))設定用
        'LNT0001tbl.Columns.Add("FUZUMI_REFVALUE", Type.GetType("System.Decimal"))       '// EXCELシート②(① - 実績数量)設定用
        'LNT0001tbl.Columns.Add("ZISSEKI_FUZUMIFLG", Type.GetType("System.String"))      '// EXCELシート(不積フラグ)設定用
#End Region

        '〇陸事番号マスタ設定
        For Each dtSekiyuSigenTankrow As DataRow In dtSekiyuSigenTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenTankrow("KEYCODE01"))
            If Mid(Me.WF_TORI.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_021502 Then
                condition &= String.Format(" AND SHUKABASHO='{0}'", dtSekiyuSigenTankrow("KEYCODE05"))
            End If
            '★届先(個別設定)は除く
            condition &= String.Format(" AND TODOKECODE NOT IN ('{0}', '{1}')",
                                       BaseDllConst.CONST_TODOKECODE_004012,
                                       BaseDllConst.CONST_TODOKECODE_005890)
            '届先(明細)セル値設定
            WW_SekiyuSigenRikugiMas(dtSekiyuSigenTankrow, condition, fuzumiLimit, LNT0001tbl)
#Region "コメント"
            'For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '    '★届日より日を取得(セル(行数)の設定のため)
            '    Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            '    Dim lastMonth As Boolean = False
            '    If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
            '        setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            '        lastMonth = True
            '    End If
            '    Dim iLine As Integer = Integer.Parse(setDay) - 1
            '    iLine = (iLine * Integer.Parse(dtSekiyuSigenTankrow("VALUE06"))) + Integer.Parse(dtSekiyuSigenTankrow("VALUE05"))
            '    ''★トリップより位置を取得
            '    'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            '    'iTrip += iLine
            '    LNT0001tblrow("ROWSORTNO") = dtSekiyuSigenTankrow("VALUE01")
            '    LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
            '    LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
            '    'LNT0001tblrow("SETCELL03") = dtSekiyuSigenTankrow("VALUE04") + iLine.ToString()
            '    LNT0001tblrow("SETLINE") = iLine
            '    LNT0001tblrow("ORDERORGCODE_REP") = dtSekiyuSigenTankrow("KEYCODE04")
            '    LNT0001tblrow("GYOMUTANKNUM_REP") = dtSekiyuSigenTankrow("KEYCODE05")
            '    '★表示セルフラグ(1:表示)
            '    If dtSekiyuSigenTankrow("VALUE07").ToString() = "1" Then
            '        LNT0001tblrow("DISPLAYCELL_START") = dtSekiyuSigenTankrow("VALUE02").ToString()
            '        LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE03").ToString()
            '        'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtSekiyuSigenTankrow("VALUE08").ToString()
            '    Else
            '        LNT0001tblrow("DISPLAYCELL_START") = ""
            '        LNT0001tblrow("DISPLAYCELL_END") = ""
            '        'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            '    End If

            '    '★備考設定用(出荷日と届日が不一致の場合)
            '    If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
            '        If lastMonth = True Then
            '            LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
            '        Else
            '            LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
            '        End If
            '    End If
            'Next
#End Region
        Next
        '〇陸事番号マスタ設定(※個別設定用)
        Dim todokeMerge = CMNPTS.filterItem(dtSekiyuSigenTankSub, "KEYCODE07", "KEYCODE08")
        For Each dtSekiyuSigenTankrow As DataRow In dtSekiyuSigenTankSub.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenTankrow("KEYCODE01"))
            If Mid(Me.WF_TORI.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_021502 Then
                condition &= String.Format(" AND SHUKABASHO='{0}'", dtSekiyuSigenTankrow("KEYCODE05"))
            End If
            '★届先(個別設定)のみ
            'condition &= String.Format(" AND TODOKECODE IN ('{0}', '{1}', '{2}')",
            '                           BaseDllConst.CONST_TODOKECODE_004012,
            '                           BaseDllConst.CONST_TODOKECODE_005890,
            '                           BaseDllConst.CONST_TODOKECODE_007273)
            condition &= String.Format(" AND TODOKECODE IN ({0})", todokeMerge)
            '届先(明細)セル値設定
            WW_SekiyuSigenRikugiMas(dtSekiyuSigenTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇石油資源開発(不積判定の設定) ----------------------------------------------------------
        '■若松ｶﾞｽ(喜多方) 
        '  --302号車(11.4t車)不積
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002022, arrFuzumi002022_302, "302")
        '■ﾃｰﾌﾞﾙﾏｰｸ新潟魚沼工場
        '  --333号車(14.0t車)不積 
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002019, arrFuzumi002019_333, "333")
        '  --334号車(15.7t車)不積
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002019, arrFuzumi002019_334, "334")
        ' ---------------------------------------------------------------------------------------/

        '〇石油資源開発(1.5回転の設定) -----------------------------------------------------------
        '■若松ｶﾞｽ(玉川)
        '  --326号車(若松1.5回転)
        WW_SetSekiyuSigenOnePointFiveCycle(BaseDllConst.CONST_TODOKECODE_002025, "積込", "積置", "326", arrOPFCycle_002025_326, judgeDate:="SHUKADATE")
        WW_SetSekiyuSigenOnePointFiveCycle(BaseDllConst.CONST_TODOKECODE_002025, "荷卸", "積配", "326", arrOPFCycle_002025_326)
        ' ---------------------------------------------------------------------------------------/

        '〇(石油資源開発)届先出荷場所車庫マスタ設定
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", SekiyuSigenTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = SekiyuSigenTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = SekiyuSigenTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = SekiyuSigenTodokerow("VALUE06")
                'LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")

                '〇届先が追加された場合
                If SekiyuSigenTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = SekiyuSigenTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = SekiyuSigenTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = SekiyuSigenTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

        '〇各部署ごとの情報取得
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}' AND SHUKABASHO='{1}'", SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE06"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")
            Next
        Next
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Select("", "KEYCODE08, KEYCODE01")
            Select Case SekiyuSigenTodokerow("KEYCODE08").ToString()
                Case "1"
                    Try
                        dcNigataList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                    Catch ex As Exception
                    End Try
                Case "2"
                    dcSyonaiList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                Case "3"
                    dcTouhokuList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                Case "4"
                    dcIbarakiList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
            End Select
        Next

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(シーエナジー・エルネス)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ReportCheckCenergyElNess(ByVal reportName As String, ByVal reportCode As String,
                                              ByRef dcCenergyList As Dictionary(Of String, String), ByRef dcElNessList As Dictionary(Of String, String))
        Dim dtCenergyElNessTank As New DataTable
        Dim dtCenergyTodoke As New DataTable
        Dim dtElNessTodoke As New DataTable
        Dim cenergyElNessTankClass As String = ""
        Dim cenergyTodokeClass As String = ""
        Dim elNessTodokeClass As String = ""
        'Dim cenergyElNessKoteihiClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)
        Dim listOrderOrgCode As New List(Of String)
        Dim commaOrderOrgCode As String = ""

        Select Case reportCode
            '"シーエナジー・エルネス　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022302
                cenergyElNessTankClass = "CENERGY_TANK"
                cenergyTodokeClass = "CENERGY_TODOKE"
                elNessTodokeClass = "ELNESS_TODOKE"
                'cenergyElNessKoteihiClass = ""
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0110600000
                arrToriCode(1) = Nothing
                arrToriCode(2) = Nothing
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021502)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_022302)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021504)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021601)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_022401)
                commaOrderOrgCode = String.Join(",", listOrderOrgCode)
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, cenergyElNessTankClass, dtCenergyElNessTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, cenergyTodokeClass, dtCenergyTodoke)
            CMNPTS.SelectCONVERTMaster(SQLcon, elNessTodokeClass, dtElNessTodoke)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value + "/01", LNT0001Calendar)
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), commaOrderOrgCode, Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001Koteihi, I_CLASS:=cenergyElNessTankClass)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001TogouSprate)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtCenergyElNessTank, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=cenergyElNessTankClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtCenergyElNessTankrow As DataRow In dtCenergyElNessTank.Rows
            '届先(明細)セル値設定
            Dim condition As String = String.Format("GYOMUTANKNUM='{0}'", dtCenergyElNessTankrow("KEYCODE04"))
            WW_CenergyElnessRikugiMas(dtCenergyElNessTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇業務車番(3XX)取得用
        For Each CenergyElNessTankrow As DataRow In dtCenergyElNessTank.Select("KEYCODE04<>''", "KEYCODE04")
            If CenergyElNessTankrow("KEYCODE04").ToString().Substring(0, 1) <> "3" Then Continue For
            Try
                dcCenergyList.Add(CenergyElNessTankrow("KEYCODE04"), CenergyElNessTankrow("KEYCODE01"))
            Catch ex As Exception
            End Try
        Next
        '〇(シーエナジー)届先出荷場所車庫マスタ設定(3XX)
        For Each CenergyTodokerow As DataRow In dtCenergyTodoke.Select("KEYCODE01<>''", "KEYCODE01")
            If CenergyTodokerow("KEYCODE01").ToString().Substring(0, 3) = "TMP" Then Continue For
            Dim condition As String = String.Format("TODOKECODE='{0}'", CenergyTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "3" Then Continue For
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = CenergyTodokerow("KEYCODE07").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = CenergyTodokerow("KEYCODE08").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKECODE") = CenergyTodokerow("KEYCODE03").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKENAME") = CenergyTodokerow("KEYCODE04").ToString()
                Catch ex As Exception
                End Try
            Next
        Next

        '〇業務車番(6XX)取得用
        For Each CenergyElNessTankrow As DataRow In dtCenergyElNessTank.Select("KEYCODE04<>''", "KEYCODE04")
            If CenergyElNessTankrow("KEYCODE04").ToString().Substring(0, 1) <> "6" Then Continue For
            Try
                dcElNessList.Add(CenergyElNessTankrow("KEYCODE04"), CenergyElNessTankrow("KEYCODE01"))
            Catch ex As Exception
            End Try
        Next
        '〇(エルネス)届先出荷場所車庫マスタ設定(6XX)
        For Each ElNessTodokerow As DataRow In dtElNessTodoke.Select("KEYCODE01<>''", "KEYCODE01")
            If ElNessTodokerow("KEYCODE01").ToString().Substring(0, 3) = "TMP" Then Continue For
            Dim condition As String = String.Format("TODOKECODE='{0}'", ElNessTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "6" Then Continue For
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = ElNessTodokerow("KEYCODE07").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = ElNessTodokerow("KEYCODE08").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKECODE") = ElNessTodokerow("KEYCODE03").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKENAME") = ElNessTodokerow("KEYCODE04").ToString()
                Catch ex As Exception
                End Try
            Next
        Next

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(石油資源開発(北海道))
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ReportCheckSekiyuSigenHokaido(ByVal reportName As String, ByVal reportCode As String, ByRef dcIshikariList As Dictionary(Of String, String))
        Dim dtSekiyuSigenHKDTank As New DataTable
        Dim dtSekiyuSigenHKDTodoke As New DataTable
        Dim sekiyuSigenTankHKDClass As String = ""
        Dim sekiyuSigenTodokeHKDClass As String = ""
        Dim sekiyuSigenKoteihiHKDClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)

        Select Case reportCode
            '"石油資源開発(北海道)　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_020104
                sekiyuSigenTankHKDClass = "SEKIYUSIGEN_HKD_TANK"
                sekiyuSigenTodokeHKDClass = "SEKIYUSIG_HKD_TODOKE"
                sekiyuSigenKoteihiHKDClass = "SEKIYU_HKD_KOTEIHI"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0132800000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_020104
                arrToriCode(2) = Nothing
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankHKDClass, dtSekiyuSigenHKDTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTodokeHKDClass, dtSekiyuSigenHKDTodoke)
            'CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", sekiyuSigenTodokeHKDClass, LNT0001Tanktbl)
            CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", sekiyuSigenTodokeHKDClass, LNT0001Tanktbl)
            CMNPTS.SelectSKSpecialFEEMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value + "/01", LNT0001SKSprate, I_CLASS:=sekiyuSigenKoteihiHKDClass)
            CMNPTS.SelectSKFuelSurchargeMaster(SQLcon, arrToriCode(0), arrToriCode(1), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001SKSurcharge)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=arrToriCode(1), I_CLASS:=sekiyuSigenKoteihiHKDClass)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), Me.WF_TaishoYm.Value + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=LNT0001SKSprate, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=sekiyuSigenKoteihiHKDClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtSekiyuSigenHKDTankrow As DataRow In dtSekiyuSigenHKDTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenHKDTankrow("KEYCODE01"))
            If Mid(Me.WF_TORI.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                condition &= String.Format(" AND TODOKECODE='{0}'", dtSekiyuSigenHKDTankrow("KEYCODE05"))
            End If
            '届先(明細)セル値設定
            WW_SekiyuSigenHKDRikugiMas(dtSekiyuSigenHKDTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇(石油資源開発)届先出荷場所車庫マスタ設定
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", SekiyuSigenTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = SekiyuSigenTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = SekiyuSigenTodokerow("VALUE01")
                'LNT0001tblrow("SHEETNAME_REP") = SekiyuSigenTodokerow("VALUE06")
                'LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")

                '〇届先が追加された場合
                If SekiyuSigenTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = SekiyuSigenTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = SekiyuSigenTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = SekiyuSigenTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next
        '〇各種別ごとの情報取得
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}' AND SETCELL03='{1}'", SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE10"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETNAME_REP") = SekiyuSigenTodokerow("VALUE06")
                LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")
            Next
        Next
        'シート名取得用
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Select("", "KEYCODE01")
            If SekiyuSigenTodokerow("KEYCODE01").ToString() = "" Then Continue For
            Dim subNo As String = SekiyuSigenTodokerow("KEYCODE08") + SekiyuSigenTodokerow("KEYCODE03")
            Try
                dcIshikariList.Add(SekiyuSigenTodokerow("KEYCODE01") + subNo, SekiyuSigenTodokerow("KEYCODE09"))
                'dcIshikariList.Add(SekiyuSigenTodokerow("KEYCODE01") + subNo, SekiyuSigenTodokerow("KEYCODE10"))
            Catch ex As Exception
            End Try
        Next

    End Sub

    Protected Sub WW_ReportMeisaiAdd(ByRef LNT0001tbl As DataTable)
        '〇(帳票)使用項目の設定
        LNT0001tbl.Columns.Add("ROWSORTNO", Type.GetType("System.Int32"))               '// 【入力用】EXCEL用ソート番号
        LNT0001tbl.Columns.Add("SETCELL01", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(届先名)
        LNT0001tbl.Columns.Add("SETCELL02", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(実績数量)
        LNT0001tbl.Columns.Add("SETCELL03", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(備考)
        LNT0001tbl.Columns.Add("SETCELL04", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(予備)
        LNT0001tbl.Columns.Add("SETCELL05", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(予備)
        LNT0001tbl.Columns.Add("SETSTARTLINE", Type.GetType("System.Int32"))            '// 【入力用】EXCEL用(開始行)
        LNT0001tbl.Columns.Add("SETLINE", Type.GetType("System.Int32"))                 '// 【入力用】EXCEL用(行数)
        LNT0001tbl.Columns.Add("TODOKENAME_REP", Type.GetType("System.String"))         '// 【入力用】EXCEL用(届先名)
        LNT0001tbl.Columns.Add("REMARK_REP", Type.GetType("System.String"))             '// 【入力用】EXCEL用(備考)
        LNT0001tbl.Columns.Add("DISPLAYCELL_START", Type.GetType("System.String"))      '// 【入力用】EXCEL用(陸事番号)設定用
        LNT0001tbl.Columns.Add("DISPLAYCELL_END", Type.GetType("System.String"))        '// 【入力用】EXCEL用(受注数量)設定用
        LNT0001tbl.Columns.Add("DISPLAYCELL_KOTEICHI", Type.GetType("System.String"))   '// 【固定費】EXCEL用(陸事番号)表示用
        LNT0001tbl.Columns.Add("TODOKECELL_REP", Type.GetType("System.String"))         '// 【届先毎】EXCEL用(届先名)表示用
        LNT0001tbl.Columns.Add("MASTERCELL_REP", Type.GetType("System.String"))         '// 【マスタ】EXCEL用(届先名)表示用
        LNT0001tbl.Columns.Add("ORDERORGCODE_REP", Type.GetType("System.String"))       '// EXCELシート(受注受付部署コード)設定用
        LNT0001tbl.Columns.Add("GYOMUTANKNUM_REP", Type.GetType("System.String"))       '// EXCELシート(業務車番)設定用
        LNT0001tbl.Columns.Add("SHEETDISPLAY_REP", Type.GetType("System.String"))       '// EXCELシート(届先名)表示用
        LNT0001tbl.Columns.Add("SHEETSORTNO_REP", Type.GetType("System.Int32"))         '// EXCELシート(届先名)ソート用
        LNT0001tbl.Columns.Add("SHEETNAME_REP", Type.GetType("System.String"))          '// EXCELシート(届先名)設定用
        LNT0001tbl.Columns.Add("GROUPNO_REP", Type.GetType("System.String"))            '// EXCELシート(届先GRP)設定用
        LNT0001tbl.Columns.Add("ZISSEKI_FUZUMI", Type.GetType("System.Decimal"))        '// EXCELシート①(車腹 - 不積(しきい値))設定用
        LNT0001tbl.Columns.Add("FUZUMI_REFVALUE", Type.GetType("System.Decimal"))       '// EXCELシート②(① - 実績数量)設定用
        LNT0001tbl.Columns.Add("ZISSEKI_FUZUMIFLG", Type.GetType("System.String"))      '// EXCELシート(不積フラグ)設定用
        LNT0001tbl.Columns.Add("ROLLY_CONTAINER", Type.GetType("System.String"))        '// EXCELシート(ローリー・コンテナ)設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKACODE", Type.GetType("System.Int32"))      '// EXCELシート(シーエナジー・エルネス)出荷コード設定用
        'LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKACODE", Type.GetType("System.String"))      '// EXCELシート(シーエナジー・エルネス)出荷コード設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKANAME", Type.GetType("System.String"))      '// EXCELシート(シーエナジー・エルネス)出荷名　称設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKECODE", Type.GetType("System.Int32"))     '// EXCELシート(シーエナジー・エルネス)届先コード設定用
        'LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKECODE", Type.GetType("System.String"))     '// EXCELシート(シーエナジー・エルネス)届先コード設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKENAME", Type.GetType("System.String"))     '// EXCELシート(シーエナジー・エルネス)届先名　称設定用
    End Sub

    ''' <summary>
    ''' 石油資源開発(届先(明細)セル値設定)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_SekiyuSigenRikugiMas(ByVal dtSekiyuSigenTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtSekiyuSigenTankrow("VALUE06"))) + Integer.Parse(dtSekiyuSigenTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtSekiyuSigenTankrow("VALUE01")
            LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
            LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
            'LNT0001tblrow("SETCELL03") = dtSekiyuSigenTankrow("VALUE04") + iLine.ToString()
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtSekiyuSigenTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtSekiyuSigenTankrow("KEYCODE04")

            '# 不積の判断
            Dim todokeCode As String = LNT0001tblrow("TODOKECODE").ToString()
            Dim decFuzumi As Decimal = Decimal.Parse(LNT0001tblrow("SYABARA").ToString()) - fuzumiLimit
            Dim decZisseki As Decimal = Decimal.Parse(LNT0001tblrow("ZISSEKI").ToString())
            LNT0001tblrow("ZISSEKI_FUZUMI") = decFuzumi
            LNT0001tblrow("FUZUMI_REFVALUE") = decFuzumi - decZisseki
            If Decimal.Parse(LNT0001tblrow("FUZUMI_REFVALUE").ToString()) >= 0 Then
                LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "TRUE"
            Else
                LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "FALSE"
            End If

            '★表示セルフラグ(1:表示)
            If dtSekiyuSigenTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtSekiyuSigenTankrow("VALUE02").ToString()
                LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE03").ToString()
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtSekiyuSigenTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' 石油資源開発(不積判定の設定)
    ''' </summary>
    Protected Sub WW_SetSekiyuSigenFuzumi(ByVal todokeCode As String, cellSet As String(),
                                          Optional ByVal gyomuNo As String = Nothing)
        Dim condition As String = ""
        condition &= String.Format("TODOKECODE='{0}' ", todokeCode)
        condition &= "AND ZISSEKI_FUZUMIFLG='TRUE' "
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            If Not IsNothing(gyomuNo) AndAlso LNT0001tblrow("GYOMUTANKNUM_REP").ToString() <> gyomuNo Then
                Continue For
            End If
            LNT0001tblrow("SETCELL01") = cellSet(0) + LNT0001tblrow("SETLINE").ToString()
            LNT0001tblrow("SETCELL02") = cellSet(1) + LNT0001tblrow("SETLINE").ToString()
        Next

    End Sub

    ''' <summary>
    ''' 石油資源開発(1.5回転の設定)
    ''' </summary>
    ''' <param name="todokeCode">届先コード</param>
    ''' <param name="loadUnloType">積込荷卸区分</param>
    ''' <param name="stackingType">積置区分</param>
    Protected Sub WW_SetSekiyuSigenOnePointFiveCycle(ByVal todokeCode As String, ByVal loadUnloType As String, ByVal stackingType As String, ByVal gyomuNo As String, cellSet As String(),
                                                      Optional ByVal judgeDate As String = "TODOKEDATE")
        Dim condition As String = ""
        condition &= String.Format("TODOKECODE='{0}' ", todokeCode)             '-- 届先
        condition &= String.Format("AND LOADUNLOTYPE='{0}' ", loadUnloType)     '-- 積込荷卸区分
        condition &= String.Format("AND STACKINGTYPE='{0}' ", stackingType)     '-- 積置区分
        condition &= String.Format("AND GYOMUTANKNUM_REP='{0}' ", gyomuNo)      '-- 業務車番

        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            Dim conditionSub As String = ""
            conditionSub &= String.Format("TODOKECODE='{0}' ", LNT0001tblrow("TODOKECODE").ToString())
            conditionSub &= String.Format("AND SHUKADATE='{0}' ", LNT0001tblrow(judgeDate).ToString())
            conditionSub &= String.Format("AND TODOKEDATE='{0}' ", LNT0001tblrow(judgeDate).ToString())
            conditionSub &= String.Format("AND STAFFCODE='{0}' ", LNT0001tblrow("STAFFCODE").ToString())
            conditionSub &= String.Format("AND GYOMUTANKNUM_REP='{0}' ", LNT0001tblrow("GYOMUTANKNUM_REP").ToString())

            For Each LNT0001tblSubrow As DataRow In LNT0001tbl.Select(conditionSub)
                LNT0001tblrow("SETCELL01") = cellSet(0) + LNT0001tblrow("SETLINE").ToString()
                LNT0001tblrow("SETCELL02") = cellSet(1) + LNT0001tblrow("SETLINE").ToString()
            Next
        Next

    End Sub

    ''' <summary>
    ''' シーエナジー・エルネス(届先(明細)セル値設定)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_CenergyElnessRikugiMas(ByVal dtCenergyElnessTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtCenergyElnessTankrow("VALUE06"))) + Integer.Parse(dtCenergyElnessTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtCenergyElnessTankrow("VALUE01")
            LNT0001tblrow("SETCELL01") = dtCenergyElnessTankrow("VALUE02")
            LNT0001tblrow("SETCELL02") = dtCenergyElnessTankrow("VALUE03")
            LNT0001tblrow("SETCELL03") = dtCenergyElnessTankrow("VALUE04")
            LNT0001tblrow("SETCELL04") = dtCenergyElnessTankrow("VALUE09")
            LNT0001tblrow("SETCELL05") = dtCenergyElnessTankrow("VALUE10")
            'LNT0001tblrow("SETCELL01") = dtCenergyElnessTankrow("VALUE02") + iLine.ToString()
            'LNT0001tblrow("SETCELL02") = dtCenergyElnessTankrow("VALUE03") + iLine.ToString()
            'LNT0001tblrow("SETCELL03") = dtCenergyElnessTankrow("VALUE04") + iLine.ToString()
            'LNT0001tblrow("SETCELL04") = dtCenergyElnessTankrow("VALUE09") + iLine.ToString()
            'LNT0001tblrow("SETCELL05") = dtCenergyElnessTankrow("VALUE10") + iLine.ToString()
            LNT0001tblrow("SETSTARTLINE") = dtCenergyElnessTankrow("VALUE05")
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtCenergyElnessTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtCenergyElnessTankrow("KEYCODE04")
            LNT0001tblrow("ROLLY_CONTAINER") = dtCenergyElnessTankrow("KEYCODE03")

            '★表示セルフラグ(1:表示)
            If dtCenergyElnessTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtCenergyElnessTankrow("VALUE02").ToString()
                LNT0001tblrow("DISPLAYCELL_END") = dtCenergyElnessTankrow("VALUE10").ToString()
                LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtCenergyElnessTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If

        Next
    End Sub

    ''' <summary>
    ''' 石油資源開発(北海道(届先(明細)セル値設定))
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_SekiyuSigenHKDRikugiMas(ByVal dtSekiyuSigenTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtSekiyuSigenTankrow("VALUE06"))) + Integer.Parse(dtSekiyuSigenTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtSekiyuSigenTankrow("VALUE01")
            If LNT0001tblrow("TRIP") = "1" Then
                LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
                LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
            ElseIf LNT0001tblrow("TRIP") = "2" Then
                LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE04") + iLine.ToString()
                LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE09") + iLine.ToString()
                If dtSekiyuSigenTankrow("VALUE04").ToString() = "" Then
                    LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
                End If
            End If
            LNT0001tblrow("SETCELL03") = dtSekiyuSigenTankrow("KEYCODE02")
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtSekiyuSigenTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtSekiyuSigenTankrow("KEYCODE04")
            LNT0001tblrow("ROLLY_CONTAINER") = dtSekiyuSigenTankrow("KEYCODE03")

            '★表示セルフラグ(1:表示)
            If dtSekiyuSigenTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtSekiyuSigenTankrow("VALUE02").ToString()
                If dtSekiyuSigenTankrow("KEYCODE05") = BaseDllConst.CONST_TODOKECODE_006915 _
                    OrElse dtSekiyuSigenTankrow("KEYCODE05") = BaseDllConst.CONST_TODOKECODE_005834 Then
                    LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE09").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE03").ToString()
                End If
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtSekiyuSigenTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If

        Next
    End Sub

    ''' <summary>
    ''' 出力履歴登録
    ''' </summary>
    Protected Sub INSHIST(ByVal SQLcon As MySqlConnection)
        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim WW_ROW As DataRow
        WW_ROW = LNT0002tbl.Rows(WW_LINECNT)

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0026_SEIKYUHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TAISHOYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,SEQ  ")
        SQLStr.AppendLine("     ,USERID  ")
        SQLStr.AppendLine("     ,USERNAME  ")
        SQLStr.AppendLine("     ,INTAKEDATE  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @TAISHOYM  ")
        SQLStr.AppendLine("     ,@TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@SEQ  ")
        SQLStr.AppendLine("     ,@USERID  ")
        SQLStr.AppendLine("     ,@USERNAME  ")
        SQLStr.AppendLine("     ,@INTAKEDATE  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID ")
        SQLStr.AppendLine("   )   ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 20)     '取引先名
                Dim P_SEQ As MySqlParameter = SQLcmd.Parameters.Add("@SEQ", MySqlDbType.VarChar, 2)     'シーケンス番号
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 50)     'ユーザーID
                Dim P_USERNAME As MySqlParameter = SQLcmd.Parameters.Add("@USERNAME", MySqlDbType.VarChar, 20)     'ユーザー名
                Dim P_INTAKEDATE As MySqlParameter = SQLcmd.Parameters.Add("@INTAKEDATE", MySqlDbType.DateTime)     '請求書出力日時

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim DATENOW As DateTime
                DATENOW = Date.Now

                ' DB更新
                P_TAISHOYM.Value = Replace(WF_TaishoYm.Value, "/", "") '対象年月

                P_TORICODE.Value = WW_ROW("KEYCODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TRANDETAILNAME")           '取引先名
                P_SEQ.Value = CInt(WW_ROW("KAISU")) + 1          'シーケンス番号
                P_USERID.Value = Master.USERID           'ユーザーID

                'ユーザ名
                CS0051UserInfo.USERID = Master.USERID
                CS0051UserInfo.getInfo()
                If isNormal(CS0051UserInfo.ERR) Then
                    P_USERNAME.Value = CS0051UserInfo.STAFFNAMES
                Else
                    P_USERNAME.Value = ""
                End If

                P_INTAKEDATE.Value = DATENOW           '請求書出力日時
                P_DELFLG.Value = "0"           '削除フラグ
                P_INITYMD.Value = DATENOW           '登録年月日
                P_INITUSER.Value = Master.USERID           '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID           '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0026_SEIKYUHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0026_SEIKYUHIST  INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            Exit Sub
        End Try

    End Sub
#End Region


    ''' <summary>
    ''' 履歴画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub HISTDataGet(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow)
        If IsNothing(LNT0002tblHIST) Then
            LNT0002tblHIST = New DataTable
        End If

        If LNT0002tblHIST.Columns.Count <> 0 Then
            LNT0002tblHIST.Columns.Clear()
        End If

        LNT0002tblHIST.Clear()

        '○ 検索SQL
        '　検索説明
        '条件指定に従い該当データをから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNT0002.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.SEQ), '')                                         AS KAISU               ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNT0002.INTAKEDATE, '%Y/%m/%d %H:%i:%s'), '')       AS DLYMD               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.USERNAME), '')                                    AS DLUSERNAME          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(A.ORGNAME), '')                                           AS DLORGNAME           ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNT0026_SEIKYUHIST LNT0002                                                                  ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("            (                                                                                        ")
        SQLStr.AppendLine("               SELECT                                                                                ")
        SQLStr.AppendLine("                 US.USERID                                                                           ")
        SQLStr.AppendLine("                ,ORG.NAME AS ORGNAME                                                                 ")
        SQLStr.AppendLine("               FROM COM.LNS0001_USER US                                                              ")
        SQLStr.AppendLine("               INNER JOIN                                                                            ")
        SQLStr.AppendLine("               LNM0002_ORG ORG                                                                       ")
        SQLStr.AppendLine("               ON US.CAMPCODE = ORG.CAMPCODE                                                         ")
        SQLStr.AppendLine("              AND US.ORG = ORG.ORGCODE                                                               ")
        SQLStr.AppendLine("              AND CURDATE() BETWEEN US.STYMD AND US.ENDYMD                                           ")
        SQLStr.AppendLine("              AND CURDATE() BETWEEN ORG.STYMD AND ORG.ENDYMD                                         ")
        SQLStr.AppendLine("              AND US.DELFLG <> '1'                                                                   ")
        SQLStr.AppendLine("              AND ORG.DELFLG <> '1'                                                                  ")
        SQLStr.AppendLine("            ) A                                                                                      ")
        SQLStr.AppendLine(" ON  LNT0002.USERID  =  A.USERID                                                                     ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("      LNT0002.DELFLG <> '1'                                                                          ")
        SQLStr.AppendLine(" AND  COALESCE(LNT0002.TAISHOYM, '0') = COALESCE(@TAISHOYM, '0')                                     ")
        SQLStr.AppendLine(" AND  COALESCE(LNT0002.TORICODE, '0') = COALESCE(@TORICODE, '0')                                     ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     LNT0002.SEQ                                                                                     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                '対象年月
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)

                P_TAISHOYM.Value = Replace(WF_TaishoYm.Value, "/", "")
                P_TORICODE.Value = WW_ROW("KEYCODE")

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0002tblHIST.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0002tblHIST.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0002row As DataRow In LNT0002tblHIST.Rows
                    i += 1
                    LNT0002row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0002L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

End Class


