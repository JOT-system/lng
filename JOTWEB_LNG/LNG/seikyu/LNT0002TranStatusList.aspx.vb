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
    Private LNT0001HolidayRateNum As DataTable                       '-- 休日割増単価マスタ(回数)※実績データより取得
    Private LNT0001KihonFeeA As DataTable                            '-- ★北海道LNG(基本料金A)データ格納用
    Private LNT0001KihonSyabanFeeA As DataTable                      '-- ★北海道LNG(基本料金A)データ格納用(車番)

    Private LNT0002tbl As DataTable           '一覧格納用テーブル
    Private LNT0002tblHIST As DataTable       '履歴一覧格納用テーブル
    Private LNT0002UPDtbl As DataTable        '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNT0002Exceltbl As New DataTable  'Excelデータ格納用テーブル
    Private LNT0002Shippers As New DataTable  '荷主一覧格納
    Private LNS0012tbl As DataTable           'プロファイルマスタ（レポート)

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    Private Const CONST_BTNADD As String = "<div><input class=""btn-sticky"" id=""btnAdd""　type=""button"" value=""追加"" readonly onclick=""BtnAddClick();"" /></div>"
    Private Const CONST_BTNOUT As String = "<div><input class=""btn-sticky"" id=""btnOut""　type=""button"" value=""出力"" readonly onclick=""BtnOutputClick();"" /></div>"
    Private Const CONST_BTNCOMOUT As String = "<div><input class=""btn-sticky"" id=""btnComOut""　type=""button"" value=""出力(共通)"" readonly onclick=""BtnComOutputClick();"" /></div>"
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
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_SelectCALENDARChange"  'カレンダー変更時
                            GridViewInitialize()
                        Case "WF_ButtonRefClick"        '参照ボタン押下時
                            work.WF_HIST.Text = "visible"
                            WF_ButtonRefClick()
                        Case "WF_ButtonAddClick"        '追加ボタン押下時
                            'WF_ButtonAddClick()
                            WF_ButtonAJUST_Click()
                        Case "WF_ButtonOutClick"        '出力ボタン押下時
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
                        Case "WF_ButtonComOutClick"     '出力（共通）ボタン押下時
                            Master.MAPID = LNT0001WRKINC.MAPIDI
                            WF_ButtonComOutClick()
                            Master.MAPID = LNT0002WRKINC.MAPIDL
                            If isNormal(WW_ErrSW) Then
                                '出力履歴登録
                                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                    SQLcon.Open()  ' DataBase接続
                                    INSHIST(SQLcon)
                                End Using
                            End If
                            GridViewInitialize()
                        Case "WF_ButtonCLOSE"           '閉じるボタン押下時
                            work.WF_HIST.Text = "hidden"

                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonOutClick" And
                        Not WF_ButtonClick.Value = "WF_ButtonComOutClick" And
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
            'Master.RecoverTable(LNT0002tbl, work.WF_SEL_INPTBL.Text)
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
        SQLStr.AppendLine("   ,  ''                                                                      AS COMCONTROL          ")　'操作列(共通明細）
        SQLStr.AppendLine("   ,  ''                                                                      AS HISTORY             ")　'履歴列

        '画面請求調整用
        SQLStr.AppendLine("   , COALESCE(RTRIM(SPRATE.TORICODE), '')                                     AS CTRLSPRATE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TANKA.TORICODE), '')                                      AS CTRLTANKA           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIXED.TORICODE), '')                                      AS CTRLKOTEIHI         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SURCHRGE.TORICODE), '')                                   AS CTRLSURCHARGE       ")

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
        SQLStr.AppendLine("                ,MAX(CONVERT(COALESCE(RTRIM(SEQ), '0') , DECIMAL)) AS  SEQ                           ")
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
        SQLStr.AppendLine("          LNG.LNM0014_SPRATE2                                                                         ")
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
        'サーチャージ料金
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT DISTINCT                                                                                ")
        SQLStr.AppendLine("          TORICODE                                                                                   ")
        SQLStr.AppendLine("          ,ORGCODE                                                                                   ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNT0030_SURCHARGEFEE                                                                   ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("      AND SEIKYUYM = REPLACE(@TAISHOYM,'/','')                                                       ")
        SQLStr.AppendLine("      AND (SHIPPINGCOUNT > 0                                                                         ")
        SQLStr.AppendLine("      OR  FUELRESULT    < 0)                                                                         ")
        SQLStr.AppendLine("    ) SURCHRGE                                                                                       ")
        SQLStr.AppendLine("      ON  FIX.VALUE5   = SURCHRGE.TORICODE                                                           ")
        SQLStr.AppendLine("     AND  (FIX.VALUE6  = SURCHRGE.ORGCODE                                                            ")
        SQLStr.AppendLine("      OR   FIX.VALUE7  = SURCHRGE.ORGCODE                                                            ")
        SQLStr.AppendLine("      OR   FIX.VALUE8  = SURCHRGE.ORGCODE                                                            ")
        SQLStr.AppendLine("      OR   FIX.VALUE9  = SURCHRGE.ORGCODE                                                            ")
        SQLStr.AppendLine("      OR   FIX.VALUE10 = SURCHRGE.ORGCODE)                                                           ")


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
                    LNT0002row("COMCONTROL") = CONST_BTNCOMOUT
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
        SQLStr.AppendLine("      ON  A2.UPDUSER = US.USERID                                                                     ")
        SQLStr.AppendLine("    ORDER BY                                                                                         ")
        SQLStr.AppendLine("           A2.UPDYMD DESC                                                                            ")

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Vertical
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
        '〇部署コード(画面)
        work.WF_SEL_ORGCODE_MAP.Text = Me.WF_TORI.Items(WW_ROW("LINECNT")).Value

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

        WF_COMEXL.Items.Clear()
        WF_COMEXL.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To LNT0002Shippers.Rows.Count - 1
            Dim wOrg As String = EditOrgCsv(LNT0002Shippers.Rows(i))
            Dim exists As Boolean = orgList.Any(Function(p) wOrg Like "*" + p + "*")
            If exists Then
                WF_COMEXL.Items.Add(New ListItem(LNT0002Shippers.Rows(i)("VALUE20"), LNT0002Shippers.Rows(i)("KEYCODE")))
            End If
        Next
        WF_COMEXL.SelectedIndex = 0

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
        '固定値のVALUE6～VALUE19を対象とする（VALUE20に共通レイアウトを設定したため）2025/09/30
        For i As Integer = 6 To 19
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
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_020104 _
            AndAlso WF_TORIORG.Items(WF_TORI.SelectedIndex).Text = BaseDllConst.CONST_TORICODE_0132800000 Then
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

        '北海道ＬＮＧ
        If selectOrgCode = BaseDllConst.CONST_ORDERORGCODE_020104 _
            AndAlso WF_TORIORG.Items(WF_TORI.SelectedIndex).Text = BaseDllConst.CONST_TORICODE_0239900000 Then
            '〇(帳票)項目チェック処理(北海道LNG)
            Dim dcHokkaidoLNG As New Dictionary(Of String, String)
            CMNCHK.WW_ReportCheckHokaidoLNG(Me.WF_TORI.SelectedItem.Text, selectOrgCode, dcHokkaidoLNG,
                                            LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001KihonFeeA, LNT0001KihonSyabanFeeA, LNT0001TogouSprate, LNT0001Calendar, LNT0001HolidayRate, LNT0001HolidayRateNum)

            Dim LNT0001InvoiceOutputReport As New LNT0001InvoiceOutputHOKAIDOLng(Master.MAPID, selectOrgCode, Me.WF_TORIEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text,
                                                                                 LNT0001tbl, LNT0001Tanktbl, LNT0001Koteihi, LNT0001KihonFeeA, LNT0001KihonSyabanFeeA, LNT0001Calendar, dcHokkaidoLNG,
                                                                                 printTogouSprateDataClass:=LNT0001TogouSprate,
                                                                                 printHolidayRateDataClass:=LNT0001HolidayRate,
                                                                                 printHolidayRateNumDataClass:=LNT0001HolidayRateNum,
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
    '''  出力（共通）ボタン押下時
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonComOutClick()
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
        Me.WF_COMEXL.SelectedIndex = Me.WF_TORI.SelectedIndex
        Me.WF_FILENAME.SelectedIndex = Me.WF_TORI.SelectedIndex
        Me.WF_TORIORG.SelectedIndex = Me.WF_TORI.SelectedIndex

        '------------------------------
        ' データ取得
        '------------------------------
        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                '実績データ取得（輸送費明細）
                ToriINVOICEDataGet(SQLcon)

                '固定費データ取得（固定費明細）
                ToriFIXEDDataGet(SQLcon)

                '特別料金データ取得（その他請求明細）
                ToriSPRATEDataGet(SQLcon)
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True, I_PARA01:="異常終了")
            WW_ErrSW = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

        WW_ErrSW = C_MESSAGE_NO.NORMAL
        If LNT0001tbl.Rows.Count = 0 AndAlso LNT0001Koteihi.Rows.Count = 0 AndAlso LNT0001TogouSprate.Rows.Count = 0 Then
            Master.Output(C_MESSAGE_NO.CTN_SELECT_EXIST, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
            WW_ErrSW = C_MESSAGE_NO.CTN_SELECT_EXIST
            Exit Sub
        End If


        '------------------------------
        '帳票出力準備処理
        '------------------------------
        Dim LNT0001InvoiceOutputCOM As New LNT0001InvoiceOutputCOM(Master.MAPID, Me.WF_COMEXL.SelectedItem.Text, Me.WF_FILENAME.SelectedItem.Text, taishoYm:=Me.WF_TaishoYm.Value)

        '運賃明細（共通化）出力TBL
        Dim PrintUnchin = New DataTable
        LNT0001InvoiceOutputCOM.CreUnchinTable(PrintUnchin)

        '固定費明細（共通化）出力TBL
        Dim PrintKotei = New DataTable
        LNT0001InvoiceOutputCOM.CreKoteiTable(PrintKotei)

        'その他請求（特別料金）（共通化）出力TBL
        Dim PrintEtc = New DataTable
        LNT0001InvoiceOutputCOM.CreEtcTable(PrintEtc)
        '------------------------------
        '集計処理
        '------------------------------
        Select Case WF_TORIORG.SelectedItem.Text
            Case BaseDllConst.CONST_TORICODE_0005700000     'ENEOS（八戸、水島）
                LNT0001InvoiceOutputCOM.SumUnchinENEOS(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedENEOS(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcENEOS(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0175300000     '東北天然ガス
                LNT0001InvoiceOutputCOM.SumUnchinTNG(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedTNG(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcTNG(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0175400000     '東北電力
                LNT0001InvoiceOutputCOM.SumUnchinTOHOKU(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedTOHOKU(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcTOHOKU(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0045300000     'エスジーリキッドサービス（西部ガス）
                LNT0001InvoiceOutputCOM.SumUnchinSAIBU(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedSAIBU(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcSAIBU(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0045200000     'エスケイ産業
                LNT0001InvoiceOutputCOM.SumUnchinESUKEI(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedESUKEI(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcESUKEI(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0132800000     '石油資源開発
                If WF_TORIORG.SelectedValue <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    '(本州分)新潟・庄内・東北・茨城
                    LNT0001InvoiceOutputCOM.SumUnchinSEKIYUSHIGEN(LNT0001tbl, PrintUnchin)
                    LNT0001InvoiceOutputCOM.SumFixedSEKIYUSHIGEN(LNT0001Koteihi, PrintKotei)
                    LNT0001InvoiceOutputCOM.SumEtcSEKIYUSHIGEN(LNT0001TogouSprate, PrintEtc)
                End If
                If WF_TORIORG.SelectedValue = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    '(北海道)石狩
                    LNT0001InvoiceOutputCOM.SumUnchinSEKIYUSHIGENHokkaido(LNT0001tbl, PrintUnchin)
                    LNT0001InvoiceOutputCOM.SumFixedSEKIYUSHIGENHokkaido(LNT0001Koteihi, PrintKotei)
                    LNT0001InvoiceOutputCOM.SumEtcSEKIYUSHIGENHokkaido(LNT0001TogouSprate, PrintEtc)
                End If
            Case BaseDllConst.CONST_TORICODE_0051200000     'OG（西日本、姫路）
                LNT0001InvoiceOutputCOM.SumUnchinDAIGAS(LNT0001tbl, PrintUnchin)
                If Me.WF_TORI.SelectedValue <> CONST_ORDERORGCODE_022702 + "02" AndAlso
                   Me.WF_TORI.SelectedValue <> CONST_ORDERORGCODE_022702 + "03" Then
                    '★[Daigas泉北、姫路]選択時
                    LNT0001InvoiceOutputCOM.SumFixedDAIGAS(LNT0001Koteihi, PrintKotei)
                    LNT0001InvoiceOutputCOM.SumEtcDAIGAS(LNT0001TogouSprate, PrintEtc)
                End If
            Case BaseDllConst.CONST_TORICODE_0239900000     '北海道ＬＮＧ
                LNT0001InvoiceOutputCOM.SumUnchinHOKKAIDOLNG(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedHOKKAIDOLNG(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcHOKKAIDOLNG(LNT0001TogouSprate, PrintEtc)
            Case BaseDllConst.CONST_TORICODE_0110600000     'シーエナジー・エルネス
                LNT0001InvoiceOutputCOM.SumUnchinCENERGY(LNT0001tbl, PrintUnchin)
                LNT0001InvoiceOutputCOM.SumFixedCENERGY(LNT0001Koteihi, PrintKotei)
                LNT0001InvoiceOutputCOM.SumEtcCENERGY(LNT0001TogouSprate, PrintEtc)
        End Select

        '----------------------------------------
        '帳票出力処理
        '----------------------------------------
        Dim PrintUrl As String
        Try
            PrintUrl = LNT0001InvoiceOutputCOM.CreateExcelPrintData(PrintUnchin, PrintKotei, PrintEtc)
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True, I_PARA01:="異常終了")
            WW_ErrSW = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = PrintUrl
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
            & "     ,coalesce(LT1.TORICODE_AVOCADO, '')                   AS TORICODE_AVOCADO				" _
            & "     ,coalesce(LT1.TODOKECONTNAME, '')                     AS TODOKECONTNAME				" _
            & "     ,coalesce(LT1.TODOKEADDR, '')                         AS TODOKEADDR			" _
            & "     ,coalesce(LT1.TODOKETEL, '')                          AS TODOKETEL			" _
            & "     ,coalesce(LT1.TODOKEMAP, '')                          AS TODOKEMAP			" _
            & "     ,coalesce(LT1.TODOKEIDO, '')                          AS TODOKEIDO			" _
            & "     ,coalesce(LT1.TODOKEKEIDO, '')                        AS TODOKEKEIDO			" _
            & "     ,coalesce(LT1.TODOKEBIKO1, '')                        AS TODOKEBIKO1			" _
            & "     ,coalesce(LT1.TODOKEBIKO2, '')                        AS TODOKEBIKO2			" _
            & "     ,coalesce(LT1.TODOKEBIKO3, '')                        AS TODOKEBIKO3			" _
            & "     ,coalesce(LT1.SHUKASLCT, '')                          AS SHUKASLCT			" _
            & "     ,CASE LT1.SHUKABASHO WHEN '006928' " _
            & "       THEN (SELECT SHUKABASHO " _
            & "               FROM LNG.LNT0001_ZISSEKI" _
            & "              WHERE " _
            & "                   TORICODE     = LT1.TORICODE " _
            & "               AND ORDERORG     = LT1.ORDERORG " _
            & "               AND GYOMUTANKNUM = LT1.GYOMUTANKNUM " _
            & "               AND TRIP         = LT1.TRIP -1 " _
            & "               AND TODOKEDATE   = LT1.TODOKEDATE " _
            & "               AND ZISSEKI      > 0 " _
            & "               AND DELFLG       = '0' " _
            & "             ) " _
            & "       ELSE LT1.SHUKABASHO " _
            & "      END AS SHUKABASHO " _
            & "     ,CASE SHUKABASHO WHEN '006928' " _
            & "       THEN (SELECT SHUKANAME " _
            & "               FROM LNG.LNT0001_ZISSEKI" _
            & "              WHERE " _
            & "                   TORICODE     = LT1.TORICODE " _
            & "               AND ORDERORG     = LT1.ORDERORG " _
            & "               AND GYOMUTANKNUM = LT1.GYOMUTANKNUM " _
            & "               AND TRIP         = LT1.TRIP -1 " _
            & "               AND TODOKEDATE   = LT1.TODOKEDATE " _
            & "               AND ZISSEKI      > 0 " _
            & "               AND DELFLG       = '0' " _
            & "             ) " _
            & "       ELSE LT1.SHUKANAME " _
            & "      END AS SHUKANAME " _
            & "     ,coalesce(LT1.SHUKANAMES, '')                         AS SHUKANAMES			" _
            & "     ,coalesce(LT1.SHUKATORICODE, '')                      AS SHUKATORICODE		" _
            & "     ,coalesce(LT1.SHUKATORINAME, '')                      AS SHUKATORINAME		" _
            & "     ,coalesce(LT1.SHUKACONTNAME, '')                      AS SHUKACONTNAME		" _
            & "     ,coalesce(LT1.SHUKAADDR, '')                          AS SHUKAADDR			" _
            & "     ,coalesce(LT1.SHUKAADDRTEL, '')                       AS SHUKAADDRTEL			" _
            & "     ,coalesce(LT1.SHUKAMAP, '')                           AS SHUKAMAP				" _
            & "     ,coalesce(LT1.SHUKAIDO, '')                           AS SHUKAIDO				" _
            & "     ,coalesce(LT1.SHUKAKEIDO, '')                         AS SHUKAKEIDO			" _
            & "     ,coalesce(LT1.SHUKABIKOU1, '')                        AS SHUKABIKOU1			" _
            & "     ,coalesce(LT1.SHUKABIKOU2, '')                        AS SHUKABIKOU2			" _
            & "     ,coalesce(LT1.SHUKABIKOU3, '')                        AS SHUKABIKOU3			" _
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
            & "     ,coalesce(LT1.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM			" _
            & "     ,coalesce(LT1.YOUSYA, '')                             AS YOUSYA				" _
            & "     ,coalesce(LT1.RECOTITLE, '')                          AS RECOTITLE			" _
            & "     ,coalesce(LT1.SHUKODATE, '')                          AS SHUKODATE			" _
            & "     ,coalesce(LT1.KIKODATE, '')                           AS KIKODATE				" _
            & "     ,coalesce(LT1.KIKOTIME, '')                           AS KIKOTIME				" _
            & "     ,coalesce(LT1.DISTANCE, '')                           AS DISTANCE				" _
            & "     ,coalesce(LT1.CREWBIKOU1, '')                         AS CREWBIKOU1			" _
            & "     ,coalesce(LT1.CREWBIKOU2, '')                         AS CREWBIKOU2			" _
            & "     ,coalesce(LT1.SUBCREWBIKOU1, '')                      AS SUBCREWBIKOU1		" _
            & "     ,coalesce(LT1.SUBCREWBIKOU2, '')                      AS SUBCREWBIKOU2		" _
            & "     ,coalesce(LT1.SUBSHUKKINTIME, '')                     AS SUBSHUKKINTIME		" _
            & "     ,coalesce(LT1.SUBNGSYAGATA, '')                       AS SUBNGSYAGATA		" _
            & "     ,coalesce(LT1.SYABARATANNI, '')                       AS SYABARATANNI			" _
            & "     ,coalesce(LT1.TAIKINTIME, '')                         AS TAIKINTIME			" _
            & "     ,coalesce(LT1.MARUYO, '')                             AS MARUYO			" _
            & "     ,coalesce(LT1.SUBTIKINTIME, '')                       AS SUBTIKINTIME			" _
            & "     ,coalesce(LT1.SUBMARUYO, '')                          AS SUBMARUYO			" _
            & "     ,coalesce(LT1.KVTITLE, '')                            AS KVTITLE				" _
            & "     ,coalesce(LT1.KVTITLETODOKE, '')                      AS KVTITLETODOKE				" _
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
    ''' 荷主毎の輸送費データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ToriINVOICEDataGet(ByVal SQLcon As MySqlConnection)

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
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" Select                                                                          ")
        SQLStr.Append("      1                                                    AS 'SELECT'           ")
        SQLStr.Append("     ,0                                                    AS HIDDEN             ")
        SQLStr.Append("     ,0                                                    AS LINECNT            ")
        SQLStr.Append("     ,''                                                   AS OPERATION          ")
        SQLStr.Append("     ,coalesce(LT1.RECONO, '')                             AS RECONO			    ")
        SQLStr.Append("     ,coalesce(LT1.LOADUNLOTYPE, '')                       AS LOADUNLOTYPE		")
        SQLStr.Append("     ,coalesce(LT1.STACKINGTYPE, '')                       AS STACKINGTYPE		")
        SQLStr.Append("     ,coalesce(LT1.ORDERORGNAME, '')                       AS ORDERORGNAME		")
        SQLStr.Append("     ,coalesce(LT1.ORDERORGCODE, '')                       AS ORDERORGCODE		")
        SQLStr.Append("     ,coalesce(LT1.KASANAMEORDERORG, '')                   AS KASANAMEORDERORG	")
        SQLStr.Append("     ,coalesce(LT1.KASANCODEORDERORG, '')                  AS KASANCODEORDERORG	")
        SQLStr.Append("     ,coalesce(LT1.ORDERORG, '')                           AS ORDERORG			")
        SQLStr.Append("     ,coalesce(LT1.PRODUCT2NAME, '')                       AS PRODUCT2NAME		")
        SQLStr.Append("     ,coalesce(LT1.PRODUCT2, '')                           AS PRODUCT2			")
        SQLStr.Append("     ,coalesce(LT1.PRODUCT1NAME, '')                       AS PRODUCT1NAME		")
        SQLStr.Append("     ,coalesce(LT1.PRODUCT1, '')                           AS PRODUCT1			")
        SQLStr.Append("     ,coalesce(LT1.OILNAME, '')                            AS OILNAME			")
        SQLStr.Append("     ,coalesce(LT1.OILTYPE, '')                            AS OILTYPE			")
        SQLStr.Append("     ,coalesce(LT1.TODOKECODE, '')                         AS TODOKECODE			")
        SQLStr.Append("     ,coalesce(LT1.TODOKENAME, '')                         AS TODOKENAME			")
        SQLStr.Append("     ,coalesce(LT1.TODOKENAMES, '')                        AS TODOKENAMES		")
        SQLStr.Append("     ,coalesce(LT1.TORICODE, '')                           AS TORICODE			")
        SQLStr.Append("     ,coalesce(LT1.TORINAME, '')                           AS TORINAME			")
        SQLStr.Append("     ,coalesce(LT1.SHUKABASHO, '')                         AS SHUKABASHO			")
        SQLStr.Append("     ,coalesce(LT1.SHUKANAME, '')                          AS SHUKANAME			")
        SQLStr.Append("     ,coalesce(LT1.SHUKANAMES, '')                         AS SHUKANAMES			")
        SQLStr.Append("     ,coalesce(LT1.SHUKATORICODE, '')                      AS SHUKATORICODE		")
        SQLStr.Append("     ,coalesce(LT1.SHUKATORINAME, '')                      AS SHUKATORINAME		")
        SQLStr.Append("     ,coalesce(LT1.SHUKADATE, '')                          AS SHUKADATE			")
        SQLStr.Append("     ,coalesce(LT1.LOADTIME, '')                           AS LOADTIME			")
        SQLStr.Append("     ,coalesce(LT1.LOADTIMEIN, '')                         AS LOADTIMEIN			")
        SQLStr.Append("     ,coalesce(LT1.TODOKEDATE, '')                         AS TODOKEDATE			")
        SQLStr.Append("     ,coalesce(LT1.SHITEITIME, '')                         AS SHITEITIME			")
        SQLStr.Append("     ,coalesce(LT1.SHITEITIMEIN, '')                       AS SHITEITIMEIN		")
        SQLStr.Append("     ,coalesce(LT1.ZYUTYU, '')                             AS ZYUTYU				")
        SQLStr.Append("     ,coalesce(LT1.ZISSEKI, '')                            AS ZISSEKI			")
        SQLStr.Append("     ,coalesce(LT1.TANNI, '')                              AS TANNI				")
        SQLStr.Append("     ,coalesce(LT1.TANKNUM, '')                            AS TANKNUM			")
        SQLStr.Append("     ,coalesce(LT1.TANKNUMBER, '')                         AS TANKNUMBER			")
        SQLStr.Append("     ,coalesce(LT1.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM	    ")
        SQLStr.Append("     ,coalesce(LT1.SYAGATA, '')                            AS SYAGATA			")
        SQLStr.Append("     ,coalesce(LT1.SYABARA, '')                            AS SYABARA			")
        SQLStr.Append("     ,coalesce(LT1.NINUSHINAME, '')                        AS NINUSHINAME		")
        SQLStr.Append("     ,coalesce(LT1.CONTYPE, '')                            AS CONTYPE			")
        SQLStr.Append("     ,coalesce(LT1.TRIP, '')                               AS TRIP				")
        SQLStr.Append("     ,coalesce(LT1.DRP, '')                                AS DRP				")
        SQLStr.Append("     ,coalesce(LT1.STAFFSLCT, '')                          AS STAFFSLCT			")
        SQLStr.Append("     ,coalesce(LT1.STAFFNAME, '')                          AS STAFFNAME			")
        SQLStr.Append("     ,coalesce(LT1.STAFFCODE, '')                          AS STAFFCODE			")
        SQLStr.Append("     ,coalesce(LT1.SUBSTAFFSLCT, '')                       AS SUBSTAFFSLCT		")
        SQLStr.Append("     ,coalesce(LT1.SUBSTAFFNAME, '')                       AS SUBSTAFFNAME		")
        SQLStr.Append("     ,coalesce(LT1.SUBSTAFFNUM, '')                        AS SUBSTAFFNUM		")
        SQLStr.Append("     ,coalesce(LT1.SHUKODATE, '')                          AS SHUKODATE			")
        SQLStr.Append("     ,coalesce(LT1.KIKODATE, '')                           AS KIKODATE			")
        SQLStr.Append("     ,coalesce(LT1.TANKA, '0')                             AS TANKA				")
        SQLStr.Append("     ,coalesce(LT1.JURYORYOKIN, '0')                       AS JURYORYOKIN		")
        SQLStr.Append("     ,coalesce(LT1.TSUKORYO, '0')                          AS TSUKORYO			")
        SQLStr.Append("     ,coalesce(LT1.KYUZITUTANKA, '0')                      AS KYUZITUTANKA		")
        SQLStr.Append("     ,coalesce(LT1.YUSOUHI, '0')                           AS YUSOUHI			")
        SQLStr.Append("     ,coalesce(LT1.WORKINGDAY, '')                         AS WORKINGDAY		    ")
        SQLStr.Append("     ,coalesce(LT1.PUBLICHOLIDAYNAME, '')                  AS PUBLICHOLIDAYNAME	")
        SQLStr.Append("     ,coalesce(LT1.DELFLG, '')                             AS DELFLG				")
        SQLStr.Append("     ,(SELECT VALUE1                                                             ")
        SQLStr.Append("         FROM COM.LNS0006_FIXVALUE                                               ")
        SQLStr.Append("        WHERE CAMPCODE = @CAMPCODE                                               ")
        SQLStr.Append("          AND CLASS    = 'TAXRATE'                                               ")
        SQLStr.Append("          AND KEYCODE  = '1'                                                     ")
        SQLStr.Append("          AND CURDATE() BETWEEN STYMD AND ENDYMD                                 ")
        SQLStr.Append("          AND DELFLG  = @DELFLG                                                  ")
        SQLStr.Append("      )                                                   AS TAXRATE             ")
        SQLStr.Append("     ,'1'                                                 AS COUNT   			")
        SQLStr.Append("     ,'0'                                                 AS TOTAL   			")
        SQLStr.Append(" FROM                                                                            ")
        Select Case WF_TORIORG.SelectedItem.Text
            Case BaseDllConst.CONST_TORICODE_0005700000     'ENEOS（八戸、水島）
                SQLStr.Append(" LNG.LNT0016_ENEOSYUSOUHI LT1                                            ")
            Case BaseDllConst.CONST_TORICODE_0175300000     '東北天然ガス
                SQLStr.Append(" LNG.LNT0017_TNGYUSOUHI LT1                                              ")
            Case BaseDllConst.CONST_TORICODE_0175400000     '東北電力
                SQLStr.Append(" LNG.LNT0018_TOHOKUYUSOUHI LT1                                           ")
            Case BaseDllConst.CONST_TORICODE_0045300000     'エスジーリキッドサービス（西部ガス）
                SQLStr.Append(" LNG.LNT0019_SAIBUGUSYUSOUHI LT1                                         ")
            Case BaseDllConst.CONST_TORICODE_0045200000     'エスケイ産業
                SQLStr.Append(" LNG.LNT0020_ESUKEIYUSOUHI LT1                                           ")
            Case BaseDllConst.CONST_TORICODE_0132800000     '石油資源開発
                If WF_TORIORG.SelectedValue <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    '(本州分)新潟・庄内・東北・茨城
                    SQLStr.Append(" LNG.LNT0021_SEKIYUHONSYUYUSOUHI LT1                                 ")
                End If
                If WF_TORIORG.SelectedValue = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    '(北海道)石狩
                    SQLStr.Append(" LNG.LNT0023_SEKIYUHOKKAIDOYUSOUHI LT1                               ")
                End If
            Case BaseDllConst.CONST_TORICODE_0051200000     'OG（西日本、姫路）
                SQLStr.Append(" LNG.LNT0022_OGYUSOUHI LT1                                               ")
            Case BaseDllConst.CONST_TORICODE_0239900000     '北海道ＬＮＧ
                SQLStr.Append(" LNG.LNT0024_HOKKAIDOLNGYUSOUHI LT1                                      ")
            Case BaseDllConst.CONST_TORICODE_0110600000     'シーエナジー・エルネス
                SQLStr.Append(" LNG.LNT0025_CENALNESUYUSOUHI LT1                                        ")
        End Select
        SQLStr.Append(" WHERE                                                                           ")
        SQLStr.Append("     date_format(LT1.TODOKEDATE, '%Y/%m/%d') >= @TODOKEDATE_FR                   ")
        SQLStr.Append(" AND date_format(LT1.TODOKEDATE, '%Y/%m/%d') <= @TODOKEDATE_TO                   ")

        '〇シーエナジー
        If WF_TORIORG.SelectedItem.Text = BaseDllConst.CONST_TORICODE_0110600000 Then
            '★北陸エルネスも含める
            Dim whereStr As String = String.Format(" AND LT1.TORICODE IN (@TORICODE, '{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
            SQLStr.Append(whereStr)
        Else
            Dim whereStr As String = String.Format(" AND LT1.TORICODE IN (@TORICODE) ")
            SQLStr.Append(whereStr)
        End If
        SQLStr.Append(" AND LT1.ORDERORGCODE in (" & WF_TORIORG.SelectedValue & ")")

        '〇西日本支店車庫
        If Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022702 + "01" Then
            '★[Daigas泉北]選択時
            SQLStr.Append(" AND LT1.TODOKECODE <> @TODOKECODE ")
        ElseIf Me.WF_TORI.SelectedValue = CONST_ORDERORGCODE_022702 + "02" Then
            '★[Daigas新宮]選択時
            SQLStr.Append(" AND LT1.TODOKECODE = @TODOKECODE ")
        End If

        SQLStr.Append(" AND LT1.DELFLG = @DELFLG ")
        SQLStr.Append(" ORDER BY ")
        SQLStr.Append(" LT1.ORDERORGCODE, LT1.SHUKADATE, LT1.TODOKEDATE, LT1.TODOKECODE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar)            '会社コード
                Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)            '取引先コード
                Dim TODOKEDATE_FR As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEDATE_FR", MySqlDbType.Date)     '届日FROM
                Dim TODOKEDATE_TO As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEDATE_TO", MySqlDbType.Date)     '届日TO
                Dim TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@@TODOKECODE", MySqlDbType.VarChar)       '届先コード
                Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)                '削除フラグ

                CAMPCODE.Value = Master.USERCAMP
                TORICODE.Value = WF_TORIORG.SelectedItem.Text
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                    TODOKEDATE_FR.Value = WF_TaishoYm.Value & "/01"
                    TODOKEDATE_TO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                Else
                    TODOKEDATE_FR.Value = Date.Now.ToString("yyyy/MM") & "/01"
                    TODOKEDATE_TO.Value = Date.Now.ToString("yyyy/MM") & DateTime.DaysInMonth(Date.Now.Year, Date.Now.Month).ToString("/00")
                End If
                TODOKECODE.Value = BaseDllConst.CONST_TODOKECODE_001640
                DELFLG.Value = BaseDllConst.C_DELETE_FLG.ALIVE

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
                Next
            End Using
        Catch ex As Exception
            Dim tblName As String = ""
            Select Case WF_TORIORG.SelectedItem.Text
                Case BaseDllConst.CONST_TORICODE_0005700000     'ENEOS（八戸、水島）
                    tblName = "LNT0016_ENEOSYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0175300000     '東北天然ガス
                    tblName = "LNT0017_TNGYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0175400000     '東北電力
                    tblName = "LNT0018_TOHOKUYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0045300000     'エスジーリキッドサービス（西部ガス）
                    tblName = "LNT0019_SAIBUGUSYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0045200000     'エスケイ産業
                    tblName = "LNT0020_ESUKEIYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0132800000     '石油資源開発(本州分)新潟・庄内・東北・茨城
                    If WF_TORI.SelectedValue <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                        tblName = "LNT0021_SEKIYUHONSYUYUSOUHI"
                    End If
                Case BaseDllConst.CONST_TORICODE_0051200000     'OG（西日本、姫路）
                    tblName = "LNT0022_OGYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0132800000     '石油資源開発(北海道)石狩
                    If WF_TORI.SelectedValue = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                        tblName = "LNT0023_SEKIYUHOKKAIDOYUSOUHI"
                    End If
                Case BaseDllConst.CONST_TORICODE_0239900000     '北海道ＬＮＧ
                    tblName = "LNT0024_HOKKAIDOLNGYUSOUHI"
                Case BaseDllConst.CONST_TORICODE_0110600000     'シーエナジー・エルネス
                    tblName = "LNT0025_CENALNESUYUSOUHI"
            End Select
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, tblName & " Select")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" & tblName & " Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元にThrow
        End Try

    End Sub

    ''' <summary>
    ''' 荷主毎の固定費データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ToriFIXEDDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0001Koteihi) Then
            LNT0001Koteihi = New DataTable
        End If

        If LNT0001Koteihi.Columns.Count <> 0 Then
            LNT0001Koteihi.Columns.Clear()
        End If

        LNT0001Koteihi.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" Select                                                                          ")
        SQLStr.Append("      coalesce(LNM7.TORICODE, '')                           AS TORICODE			")
        SQLStr.Append("     ,coalesce(LNM7.TORINAME, '')                           AS TORINAME			")
        SQLStr.Append("     ,coalesce(LNM7.ORGCODE, '')                            AS ORGCODE		    ")
        SQLStr.Append("     ,coalesce(LNM7.ORGNAME, '')                            AS ORGNAME		    ")
        SQLStr.Append("     ,coalesce(LNM7.KASANORGCODE, '')                       AS KASANORGCODE		")
        SQLStr.Append("     ,coalesce(LNM7.KASANORGNAME, '')                       AS KASANORGNAME		")
        SQLStr.Append("     ,coalesce(LNM7.TARGETYM, '')                           AS TARGETYM			")
        SQLStr.Append("     ,coalesce(LNM7.SYABAN, '')                             AS SYABAN			")
        SQLStr.Append("     ,coalesce(LNM7.RIKUBAN, '')                            AS RIKUBAN			")
        SQLStr.Append("     ,coalesce(LNM7.SYAGATA, '')                            AS SYAGATA			")
        SQLStr.Append("     ,coalesce(LNM7.SYAGATANAME, '')                        AS SYAGATANAME		")
        SQLStr.Append("     ,coalesce(LNM7.SYABARA, '')                            AS SYABARA		    ")
        SQLStr.Append("     ,coalesce(LNM7.SEASONKBN, '')                          AS SEASONKBN			")
        SQLStr.Append("     ,coalesce(LNM7.SEASONSTART, '')                        AS SEASONSTART		")
        SQLStr.Append("     ,coalesce(LNM7.SEASONEND, '')                          AS SEASONEND			")
        SQLStr.Append("     ,coalesce(LNM7.KOTEIHIM, '0')                          AS KOTEIHIM			")
        SQLStr.Append("     ,coalesce(LNM7.KOTEIHID, '0')                          AS KOTEIHID			")
        SQLStr.Append("     ,coalesce(LNM7.KAISU, '0')                             AS KAISU		        ")
        SQLStr.Append("     ,coalesce(LNM7.GENGAKU, '0')                           AS GENGAKU			")
        SQLStr.Append("     ,coalesce(LNM7.AMOUNT, '0')                            AS AMOUNT			")
        SQLStr.Append("     ,coalesce(LNM7.BIKOU1, '')                             AS BIKOU1			")
        SQLStr.Append("     ,coalesce(LNM7.BIKOU2, '')                             AS BIKOU2			")
        SQLStr.Append("     ,coalesce(LNM7.BIKOU3, '')                             AS BIKOU3		    ")
        SQLStr.Append("     ,coalesce(LNM7.DELFLG, '')                             AS DELFLG		    ")
        SQLStr.Append("     ,(SELECT VALUE1                                                             ")
        SQLStr.Append("         FROM COM.LNS0006_FIXVALUE                                               ")
        SQLStr.Append("        WHERE CAMPCODE = @CAMPCODE                                               ")
        SQLStr.Append("          AND CLASS    = 'TAXRATE'                                               ")
        SQLStr.Append("          AND KEYCODE  = '1'                                                     ")
        SQLStr.Append("          AND CURDATE() BETWEEN STYMD AND ENDYMD                                 ")
        SQLStr.Append("          AND DELFLG  = @DELFLG                                                  ")
        SQLStr.Append("      )                                                   AS TAXRATE             ")
        SQLStr.Append(" FROM                                                                            ")
        SQLStr.Append(" LNG.LNM0007_FIXED LNM7                                                          ")
        SQLStr.Append(" WHERE                                                                           ")
        SQLStr.Append("     LNM7.TARGETYM = @TARGETYM                                                   ")
        '〇シーエナジー
        If WF_TORIORG.SelectedItem.Text = BaseDllConst.CONST_TORICODE_0110600000 Then
            '★北陸エルネスも含める
            Dim whereStr As String = String.Format(" AND LNM7.TORICODE IN (@TORICODE, '{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
            SQLStr.Append(whereStr)
        Else
            Dim whereStr As String = String.Format(" AND LNM7.TORICODE IN (@TORICODE) ")
            SQLStr.Append(whereStr)
        End If
        SQLStr.Append(" AND LNM7.ORGCODE in (" & WF_TORIORG.SelectedValue & ")")
        SQLStr.Append(" AND LNM7.DELFLG = @DELFLG ")
        SQLStr.Append(" ORDER BY ")
        SQLStr.Append(" LNM7.TORICODE, LNM7.ORGCODE, LNM7.SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar)            '会社コード
                Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)            '取引先コード
                Dim TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar)            '対象年月
                Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)                '削除フラグ

                CAMPCODE.Value = Master.USERCAMP
                TORICODE.Value = WF_TORIORG.SelectedItem.Text
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) Then
                    TARGETYM.Value = WF_TaishoYm.Value.Replace("/", "")
                Else
                    TARGETYM.Value = Date.Now.ToString("yyyyMM")
                End If
                DELFLG.Value = BaseDllConst.C_DELETE_FLG.ALIVE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001Koteihi.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001Koteihi.Load(SQLdr)
                End Using

                For Each LNT0001row In LNT0001Koteihi.Rows
                    If LNT0001row("SEASONKBN") <> "0" Then
                        LNT0001row("DELFLG") = "1"
                        If LNT0001row("SEASONSTART") < LNT0001row("SEASONEND") Then
                            '例） 0401　＜　1130 の場合、
                            If LNT0001row("TARGETYM") >= Left(LNT0001row("TARGETYM"), 4) & Left(LNT0001row("SEASONSTART"), 2) AndAlso
                               LNT0001row("TARGETYM") <= Left(LNT0001row("TARGETYM"), 4) & Left(LNT0001row("SEASONEND"), 2) Then
                                LNT0001row("DELFLG") = "0"
                            End If
                        Else
                            '例） 1201　＞　0331
                            If LNT0001row("TARGETYM") >= Left(LNT0001row("TARGETYM"), 4) & Left(LNT0001row("SEASONSTART"), 2) AndAlso
                               LNT0001row("TARGETYM") <= Left(LNT0001row("TARGETYM"), 4) + 1 & Left(LNT0001row("SEASONEND"), 2) Then
                                LNT0001row("DELFLG") = "0"
                            End If
                        End If
                    End If
                Next
                Dim view As DataView = LNT0001Koteihi.DefaultView
                view.RowFilter = "DELFLG = '0'"
                LNT0001Koteihi = view.ToTable

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007_FIXED Select")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_FIXED Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元にThrow
        End Try

    End Sub
    ''' <summary>
    ''' 荷主毎の特別料金データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ToriSPRATEDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0001TogouSprate) Then
            LNT0001TogouSprate = New DataTable
        End If

        If LNT0001TogouSprate.Columns.Count <> 0 Then
            LNT0001TogouSprate.Columns.Clear()
        End If

        LNT0001TogouSprate.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" Select                                                                              ")
        SQLStr.Append("      coalesce(LNM14.TARGETYM, '')                           AS TARGETYM			    ")
        SQLStr.Append("     ,coalesce(LNM14.TORICODE, '')                           AS TORICODE			    ")
        SQLStr.Append("     ,coalesce(LNM14.TORINAME, '')                           AS TORINAME		        ")
        SQLStr.Append("     ,coalesce(LNM14.ORGCODE, '')                            AS ORGCODE		        ")
        SQLStr.Append("     ,coalesce(LNM14.ORGNAME, '')                            AS ORGNAME		        ")
        SQLStr.Append("     ,coalesce(LNM14.KASANORGCODE, '')                       AS KASANORGCODE		    ")
        SQLStr.Append("     ,coalesce(LNM14.KASANORGNAME, '')                       AS KASANORGNAME		    ")
        SQLStr.Append("     ,coalesce(LNM14.GROUPCODE, '')                          AS GROUPCODE		    ")
        SQLStr.Append("     ,coalesce(LNM14.BIGCATECODE, '')                        AS BIGCATECODE		    ")
        SQLStr.Append("     ,coalesce(LNM14.BIGCATENAME, '')                        AS BIGCATENAME		    ")
        SQLStr.Append("     ,coalesce(LNM14.MIDCATECODE, '')                        AS MIDCATECODE		    ")
        SQLStr.Append("     ,coalesce(LNM14.MIDCATENAME, '')                        AS MIDCATENAME		    ")
        SQLStr.Append("     ,coalesce(LNM14.SMALLCATECODE, '')                      AS SMALLCATECODE	    ")
        SQLStr.Append("     ,coalesce(LNM14.SMALLCATENAME, '')                      AS SMALLCATENAME        ")
        SQLStr.Append("     ,coalesce(LNM14.TANKA, '0')                             AS TANKA		        ")
        SQLStr.Append("     ,coalesce(LNM14.QUANTITY, '0')                          AS QUANTITY		        ")
        SQLStr.Append("     ,coalesce(LNM14.CALCUNIT, '')                           AS CALCUNIT		        ")
        SQLStr.Append("     ,coalesce(LNM14.DEPARTURE, '')                          AS DEPARTURE		    ")
        SQLStr.Append("     ,coalesce(LNM14.MILEAGE, '0')                           AS MILEAGE			    ")
        SQLStr.Append("     ,coalesce(LNM14.SHIPPINGCOUNT, '0')                     AS SHIPPINGCOUNT        ")
        SQLStr.Append("     ,coalesce(LNM14.NENPI, '0')                             AS NENPI			    ")
        SQLStr.Append("     ,coalesce(LNM14.DIESELPRICECURRENT, '0')                AS DIESELPRICECURRENT   ")
        SQLStr.Append("     ,coalesce(LNM14.DIESELPRICESTANDARD, '0')               AS DIESELPRICESTANDARD  ")
        SQLStr.Append("     ,coalesce(LNM14.DIESELCONSUMPTION, '0')                 AS DIESELCONSUMPTION    ")
        SQLStr.Append("     ,coalesce(LNM14.DISPLAYFLG, '0')                        AS DISPLAYFLG           ")
        SQLStr.Append("     ,coalesce(LNM14.ASSESSMENTFLG, '0')                     AS ASSESSMENTFLG        ")
        SQLStr.Append("     ,coalesce(LNM14.ATENACOMPANYNAME, '')                   AS ATENACOMPANYNAME     ")
        SQLStr.Append("     ,coalesce(LNM14.ATENACOMPANYDEVNAME, '')                AS ATENACOMPANYDEVNAME  ")
        SQLStr.Append("     ,coalesce(LNM14.FROMORGNAME, '')                        AS FROMORGNAME          ")
        SQLStr.Append("     ,coalesce(LNM14.MEISAICATEGORYID, '1')                  AS MEISAICATEGORYID     ")
        SQLStr.Append("     ,coalesce(LNM14.BIKOU1, '')                             AS BIKOU1			    ")
        SQLStr.Append("     ,coalesce(LNM14.BIKOU2, '')                             AS BIKOU2			    ")
        SQLStr.Append("     ,coalesce(LNM14.BIKOU3, '')                             AS BIKOU3		        ")
        SQLStr.Append("     ,coalesce(LNM14.DELFLG, '')                             AS DELFLG		        ")
        SQLStr.Append("     ,(SELECT VALUE1                                                                 ")
        SQLStr.Append("         FROM COM.LNS0006_FIXVALUE                                                   ")
        SQLStr.Append("        WHERE CAMPCODE = @CAMPCODE                                                   ")
        SQLStr.Append("          AND CLASS    = 'TAXRATE'                                                   ")
        SQLStr.Append("          AND KEYCODE  = '1'                                                         ")
        SQLStr.Append("          AND CURDATE() BETWEEN STYMD AND ENDYMD                                     ")
        SQLStr.Append("          AND DELFLG  = @DELFLG                                                      ")
        SQLStr.Append("      )                                                   AS TAXRATE                 ")
        SQLStr.Append(" FROM                                                                                ")
        SQLStr.Append(" LNG.LNM0014_SPRATE2 LNM14                                                           ")
        SQLStr.Append(" WHERE                                                                               ")
        SQLStr.Append("     LNM14.TARGETYM = @TARGETYM                                                      ")
        '〇シーエナジー
        If WF_TORIORG.SelectedItem.Text = BaseDllConst.CONST_TORICODE_0110600000 Then
            '★北陸エルネスも含める
            Dim whereStr As String = String.Format(" AND LNM14.TORICODE IN (@TORICODE, '{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
            SQLStr.Append(whereStr)
        Else
            Dim whereStr As String = String.Format(" AND LNM14.TORICODE IN (@TORICODE) ")
            SQLStr.Append(whereStr)
        End If
        SQLStr.Append(" AND LNM14.ORGCODE in (" & WF_TORIORG.SelectedValue & ")")
        SQLStr.Append(" AND LNM14.DISPLAYFLG = '1' ")
        SQLStr.Append(" AND LNM14.MEISAICATEGORYID = '1' ")
        SQLStr.Append(" AND LNM14.DELFLG = @DELFLG ")
        SQLStr.Append(" ORDER BY ")
        SQLStr.Append(" LNM14.ORGCODE, LNM14.BIGCATECODE, LNM14.MIDCATECODE, LNM14.SMALLCATECODE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar)            '会社コード
                Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)            '取引先コード
                Dim TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar)            '対象年月
                Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)                '削除フラグ

                CAMPCODE.Value = Master.USERCAMP
                TORICODE.Value = WF_TORIORG.SelectedItem.Text
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) Then
                    TARGETYM.Value = WF_TaishoYm.Value.Replace("/", "")
                Else
                    TARGETYM.Value = Date.Now.ToString("yyyyMM")
                End If
                DELFLG.Value = BaseDllConst.C_DELETE_FLG.ALIVE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001TogouSprate.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001TogouSprate.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SPRATE2 Select")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元にThrow
        End Try

    End Sub
    ''' <summary>
    ''' 出力項目指示データ取得（LNS0012_PROFMXLS)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ProfmXlsGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNS0012tbl) Then
            LNS0012tbl = New DataTable
        End If

        If LNS0012tbl.Columns.Count <> 0 Then
            LNS0012tbl.Columns.Clear()
        End If

        LNS0012tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" Select                                                                      ")
        SQLStr.Append("      coalesce(LS12.CAMPCODE, '')                            AS RECONO		")
        SQLStr.Append("     ,coalesce(LS12.PROFID, '')                              AS PROFID		")
        SQLStr.Append("     ,coalesce(LS12.MAPID, '')                               AS MAPID		")
        SQLStr.Append("     ,coalesce(LS12.REPORTID, '')                            AS REPORTID		")
        SQLStr.Append("     ,coalesce(LS12.TITLEKBN, '')                            AS TITLEKBN		")
        SQLStr.Append("     ,coalesce(LS12.FIELD, '')                               AS FIELD	    ")
        SQLStr.Append("     ,coalesce(LS12.STYMD, '')                               AS STYMD	    ")
        SQLStr.Append("     ,coalesce(LS12.ENDYMD, '')                              AS ENDYMD		")
        SQLStr.Append("     ,coalesce(LS12.FIELDNAMES, '')                          AS FIELDNAMES	")
        SQLStr.Append("     ,coalesce(LS12.POSISTART, '')                           AS POSISTART	")
        SQLStr.Append("     ,coalesce(LS12.POSIROW, '')                             AS POSIROW		")
        SQLStr.Append("     ,coalesce(LS12.POSICOL, '')                             AS POSICOL		")
        SQLStr.Append("     ,coalesce(LS12.WIDTH, '')                               AS WIDTH		")
        SQLStr.Append("     ,coalesce(LS12.EXCELFILE, '')                           AS EXCELFILE	")
        SQLStr.Append("     ,coalesce(LS12.STRUCTCODE, '')                          AS STRUCTCODE	")
        SQLStr.Append("     ,coalesce(LS12.SORTORDER, '')                           AS SORTORDER	")
        SQLStr.Append("     ,coalesce(LS12.EFFECT, '')                              AS EFFECT		")
        SQLStr.Append("     ,coalesce(LS12.FORMATTYPE, '')                          AS FORMATTYPE	")
        SQLStr.Append(" FROM                                                                        ")
        SQLStr.Append("     COM.LNS0012_PROFMXLS LS12                                               ")
        SQLStr.Append(" WHERE                                                                       ")
        SQLStr.Append("     LS12.CAMPCODE  = @CAMPCODE                                              ")
        SQLStr.Append(" AND LS12.PROFID    = @PROFID                                                ")
        SQLStr.Append(" AND LS12.MAPID     = @MAPID                                                 ")
        SQLStr.Append(" AND LS12.REPORTID  = @REPORTID                                              ")
        SQLStr.Append(" AND LS12.TITLEKBN  = @TITLEKBN                                              ")
        SQLStr.Append(" AND CURDATE() BETWEEN LS12.STYMD AND LS12.ENDYMD                            ")
        SQLStr.Append(" AND LS12.DELFLG    = @DELFLG                                                ")
        SQLStr.Append(" ORDER BY LS12.POSICOL                                                       ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar)            '会社コード
                Dim PROFID As MySqlParameter = SQLcmd.Parameters.Add("@PROFID", MySqlDbType.VarChar)                'プロファイルＩＤ
                Dim MAPID As MySqlParameter = SQLcmd.Parameters.Add("@MAPID", MySqlDbType.VarChar)                  '画面ＩＤ
                Dim REPORTID As MySqlParameter = SQLcmd.Parameters.Add("@REPORTID", MySqlDbType.VarChar)            'レポートＩＤ
                Dim TITLEKBN As MySqlParameter = SQLcmd.Parameters.Add("@@TITLEKBN", MySqlDbType.VarChar)           'タイトル区分
                Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)                '削除フラグ

                CAMPCODE.Value = Master.USERCAMP
                PROFID.Value = "def"
                MAPID.Value = LNT0002WRKINC.MAPIDL
                REPORTID.Value = Me.WF_TORI.SelectedValue
                TITLEKBN.Value = "I"
                DELFLG.Value = BaseDllConst.C_DELETE_FLG.ALIVE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0012tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0012tbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0012_PROFMXLS Select")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0012_PROFMXLS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元にThrow
        End Try

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
        SQLStr.AppendLine("   , LPAD(COALESCE(RTRIM(LNT0002.SEQ), ''),2,'0')                             AS KAISU               ")
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
        SQLStr.AppendLine("     LPAD(COALESCE(RTRIM(LNT0002.SEQ), ''),2,'0')                                                    ")

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


