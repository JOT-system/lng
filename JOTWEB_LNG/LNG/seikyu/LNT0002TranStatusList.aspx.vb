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
                        '閉じるボタン押下時
                        Case "WF_ButtonCLOSE"
                            work.WF_HIST.Text = "hidden"

                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" Then
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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0002D Then

            ' 登録画面からの遷移
            Master.RecoverTable(LNT0002tbl, work.WF_SEL_INPTBL.Text)
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
        If LNT0002WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        '対象年月
        'WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM/dd")
        WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")

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
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , CURDATE()                                                                AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE4), '')                                          AS INDEXKEY            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE5), '')                                          AS TORICODE            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE6), '')                                          AS ORGCODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FIX.VALUE1), '')                                          AS TRANDETAILNAME      ")
        SQLStr.AppendLine("   ,  ''                                                                      AS DETAIL              ")
        SQLStr.AppendLine("   ,  ''                                                                      AS UPDYMD              ")
        SQLStr.AppendLine("   ,  ''                                                                      AS UPDUSERNAME         ")
        SQLStr.AppendLine("   ,  ''                                                                      AS CONTROL             ")
        SQLStr.AppendLine("   , '0'                                                                      AS KAISU               ")
        SQLStr.AppendLine("   ,  ''                                                                      AS DLYMD               ")
        SQLStr.AppendLine("   ,  ''                                                                      AS DLUSERNAME          ")
        SQLStr.AppendLine("   ,  ''                                                                      AS HISTORY             ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     COM.LNS0006_FIXVALUE FIX                                                                        ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("      FIX.DELFLG = '0'                                                                               ")
        SQLStr.AppendLine("     AND  FIX.CLASS = 'INVOICE'                                                                 ")
        SQLStr.AppendLine("     AND  FIX.CAMPCODE = '" & Master.USERCAMP & "'                                                   ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     FIX.VALUE4                                                                                     ")

        'SQLStr.AppendLine(" Select                                                                                              ")
        'SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        'SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        'SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        'SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        'SQLStr.AppendLine("   , LNT0002.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.DELFLG), '')                                      AS DELFLG              ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.TARGETYM), '')                                    AS TARGETYM            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.TORICODE), '')                                    AS TORICODE            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.ORGCODE), '')                                     AS ORGCODE             ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.TRANDETAILNAME), '')                              AS TRANDETAILNAME      ")
        'SQLStr.AppendLine("   ,  ''                                                                      AS DETAIL              ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(B.UPDYMD), '')                                            AS UPDYMD              ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(B.UPDUSERNAME), '')                                       AS UPDUSERNAME         ")
        'SQLStr.AppendLine("   ,  ''                                                                      AS CONTROL             ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(A.KAISU), '0')                                            AS KAISU               ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(C.DLYMD), '')                                             AS DLYMD               ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(C.DLUSERNAME), '')                                        AS DLUSERNAME          ")
        'SQLStr.AppendLine("   ,  ''                                                                      AS HISTORY             ")
        'SQLStr.AppendLine(" FROM                                                                                                ")
        'SQLStr.AppendLine("     LNG.LNT0016_TRANOUTPUTHIST LNT0002                                                              ")

        ''--参照権限
        'SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        'SQLStr.AppendLine("    (                                                                                                ")
        'SQLStr.AppendLine("      SELECT                                                                                         ")
        'SQLStr.AppendLine("          CODE                                                                                       ")
        'SQLStr.AppendLine("      FROM                                                                                           ")
        'SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        'SQLStr.AppendLine("      WHERE                                                                                          ")
        'SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        'SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        'SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        'SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        'SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        'SQLStr.AppendLine("      ON  LNT0002.ORGCODE = LNS0005.CODE                                                             ")

        ''回数
        'SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        'SQLStr.AppendLine("    (                                                                                                ")
        'SQLStr.AppendLine("      SELECT                                                                                         ")
        'SQLStr.AppendLine("          TARGETYM                                                                                   ")
        'SQLStr.AppendLine("          ,TORICODE                                                                                  ")
        'SQLStr.AppendLine("          ,ORGCODE                                                                                   ")
        'SQLStr.AppendLine("          ,SUM(KAISU) AS  KAISU                                                                      ")
        'SQLStr.AppendLine("      FROM                                                                                           ")
        'SQLStr.AppendLine("          LNG.LNT0016_TRANOUTPUTHIST                                                                 ")
        'SQLStr.AppendLine("      WHERE                                                                                          ")
        'SQLStr.AppendLine("       DELFLG <> '1'                                                                                 ")
        'SQLStr.AppendLine("      GROUP BY                                                                                       ")
        'SQLStr.AppendLine("       TARGETYM                                                                                      ")
        'SQLStr.AppendLine("      ,TORICODE                                                                                      ")
        'SQLStr.AppendLine("      ,ORGCODE                                                                                       ")
        'SQLStr.AppendLine("    ) A                                                                                              ")
        'SQLStr.AppendLine("      ON  LNT0002.TARGETYM = A.TARGETYM                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.TORICODE = A.TORICODE                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.ORGCODE = A.ORGCODE                                                                ")

        ''最終更新日時、最終更新者
        'SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        'SQLStr.AppendLine("    (                                                                                                ")
        'SQLStr.AppendLine("      SELECT                                                                                         ")
        'SQLStr.AppendLine("          B1.TARGETYM                                                                                ")
        'SQLStr.AppendLine("          ,B1.TORICODE                                                                               ")
        'SQLStr.AppendLine("          ,B1.ORGCODE                                                                                ")
        'SQLStr.AppendLine("          ,MAX(B1.UPDYMD) AS UPDYMD                                                                  ")
        'SQLStr.AppendLine("          ,US.STAFFNAMES AS UPDUSERNAME                                                              ")
        'SQLStr.AppendLine("      FROM                                                                                           ")
        'SQLStr.AppendLine("          LNG.LNT0016_TRANOUTPUTHIST B1                                                              ")
        'SQLStr.AppendLine("      LEFT JOIN                                                                                      ")
        'SQLStr.AppendLine("          COM.LNS0001_USER US                                                                        ")
        'SQLStr.AppendLine("      ON  B1.UPDUSER = US.USERID                                                                     ")
        'SQLStr.AppendLine("      WHERE                                                                                          ")
        'SQLStr.AppendLine("       B1.DELFLG <> '1'                                                                              ")
        'SQLStr.AppendLine("      GROUP BY                                                                                       ")
        'SQLStr.AppendLine("       B1.TARGETYM                                                                                   ")
        'SQLStr.AppendLine("      ,B1.TORICODE                                                                                   ")
        'SQLStr.AppendLine("      ,B1.ORGCODE                                                                                    ")
        'SQLStr.AppendLine("      ,US.STAFFNAMES                                                                                 ")
        'SQLStr.AppendLine("    ) B                                                                                              ")
        'SQLStr.AppendLine("      ON  LNT0002.TARGETYM = B.TARGETYM                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.TORICODE = B.TORICODE                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.ORGCODE = B.ORGCODE                                                                ")

        ''最新ダウンロード日時、最新ダウンロード実施者
        'SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        'SQLStr.AppendLine("    (                                                                                                ")
        'SQLStr.AppendLine("      SELECT                                                                                         ")
        'SQLStr.AppendLine("          C1.TARGETYM                                                                                ")
        'SQLStr.AppendLine("          ,C1.TORICODE                                                                               ")
        'SQLStr.AppendLine("          ,C1.ORGCODE                                                                                ")
        'SQLStr.AppendLine("          ,MAX(C1.DLYMD) AS DLYMD                                                                    ")
        'SQLStr.AppendLine("          ,US.STAFFNAMES AS DLUSERNAME                                                               ")
        'SQLStr.AppendLine("      FROM                                                                                           ")
        'SQLStr.AppendLine("          LNG.LNT0016_TRANOUTPUTHIST C1                                                              ")
        'SQLStr.AppendLine("      LEFT JOIN                                                                                      ")
        'SQLStr.AppendLine("          COM.LNS0001_USER US                                                                        ")
        'SQLStr.AppendLine("      ON  C1.DLUSER = US.USERID                                                                      ")
        'SQLStr.AppendLine("      WHERE                                                                                          ")
        'SQLStr.AppendLine("       C1.DELFLG <> '1'                                                                              ")
        'SQLStr.AppendLine("      GROUP BY                                                                                       ")
        'SQLStr.AppendLine("       C1.TARGETYM                                                                                   ")
        'SQLStr.AppendLine("      ,C1.TORICODE                                                                                   ")
        'SQLStr.AppendLine("      ,C1.ORGCODE                                                                                    ")
        'SQLStr.AppendLine("      ,US.STAFFNAMES                                                                                 ")
        'SQLStr.AppendLine("    ) C                                                                                              ")
        'SQLStr.AppendLine("      ON  LNT0002.TARGETYM = C.TARGETYM                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.TORICODE = C.TORICODE                                                              ")
        'SQLStr.AppendLine("     AND  LNT0002.ORGCODE = C.ORGCODE                                                                ")

        'SQLStr.AppendLine(" WHERE                                                                                               ")
        'SQLStr.AppendLine("      LNT0002.DELFLG = '0'                                                                       ")

        ''○ 条件指定で指定されたものでSQLで可能なものを追加する
        'Dim Itype As Integer

        ''対象年月
        'If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
        '    SQLStr.AppendLine(" AND  COALESCE(LNT0002.TARGETYM, '0') = COALESCE(@TARGETYM, '0')  ")
        'End If

        'SQLStr.AppendLine(" ORDER BY                                                                       ")
        'SQLStr.AppendLine("     LNT0002.TARGETYM                                                           ")
        'SQLStr.AppendLine("    ,LNT0002.TORICODE                                                           ")
        'SQLStr.AppendLine("    ,LNT0002.ORGCODE                                                            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                ''ロール
                'Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20)
                'P_ROLE.Value = Master.ROLE_ORG

                ''対象年月
                'If Integer.TryParse(Replace(WF_TaishoYm.Value, "/", ""), Itype) Then
                '    Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)
                '    P_TARGETYM.Value = Itype
                'End If

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
    '''  参照ボタン押下時
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRefClick()
        'Dim WW_LINECNT As Integer = 0
        'Dim WW_HTML As String = ""

        ''○ LINECNT取得
        'Try
        '    Integer.TryParse(Me.WF_SelectedIndex.Value, WW_LINECNT)
        '    WW_LINECNT -= 1
        'Catch ex As Exception
        '    Exit Sub
        'End Try

        'Dim WW_ROW As DataRow
        'WW_ROW = LNT0002tbl.Rows(WW_LINECNT)

        'Me.WF_HISTTITLE.Text = WW_ROW("TRANDETAILNAME")

        ''○ 画面表示データ取得
        'Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()       'DataBase接続

        '    HISTDataGet(SQLcon, WW_ROW)
        'End Using

        ''○ 一覧表示データ編集(性能対策)
        'Dim TBLview As DataView = New DataView(LNT0002tblHIST)
        ''○並び順初期化
        'TBLview.Sort = "LINECNT"

        'CS0013ProfView.CAMPCODE = Master.USERCAMP
        'CS0013ProfView.PROFID = Master.PROF_VIEW
        'CS0013ProfView.MAPID = Master.MAPID + "HIST"
        'CS0013ProfView.VARI = Master.VIEWID
        'CS0013ProfView.SRCDATA = TBLview.ToTable
        'CS0013ProfView.TBLOBJ = pnlHISTListArea
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        'CS0013ProfView.TITLEOPT = True
        'CS0013ProfView.HIDEOPERATIONOPT = True
        'CS0013ProfView.CS0013ProfView()
        'If Not isNormal(CS0013ProfView.ERR) Then
        '    Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
        '    Exit Sub
        'End If

        'TBLview.Dispose()
        'TBLview = Nothing
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

    ''' <summary>
    ''' 履歴画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub HISTDataGet(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow)
        'If IsNothing(LNT0002tblHIST) Then
        '    LNT0002tblHIST = New DataTable
        'End If

        'If LNT0002tblHIST.Columns.Count <> 0 Then
        '    LNT0002tblHIST.Columns.Clear()
        'End If

        'LNT0002tblHIST.Clear()

        ''○ 検索SQL
        ''　検索説明
        ''条件指定に従い該当データをから取得する
        'Dim SQLStr = New StringBuilder
        'SQLStr.AppendLine(" Select                                                                                              ")
        'SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        'SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        'SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        'SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        'SQLStr.AppendLine("   , LNT0002.UPDTIMSTP                                                        AS UPDTIMSTP           ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.DELFLG), '')                                      AS DELFLG              ")
        'SQLStr.AppendLine("   ,  ''                                                                      AS HISTCONTROL         ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.TARGETYM), '')                                    AS TARGETYM            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.TORICODE), '')                                    AS TORICODE            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.ORGCODE), '')                                     AS ORGCODE             ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.KAISU), '')                                       AS KAISU               ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0002.DLYMD), '')                                       AS DLYMD               ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(US.STAFFNAMES), '')                                       AS DLUSERNAME          ")
        'SQLStr.AppendLine(" FROM                                                                                                ")
        'SQLStr.AppendLine("     LNG.LNT0016_TRANOUTPUTHIST LNT0002                                                              ")
        'SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        'SQLStr.AppendLine("     COM.LNS0001_USER US                                                                             ")
        'SQLStr.AppendLine("   ON  LNT0002.DLUSER = US.USERID                                                                    ")
        'SQLStr.AppendLine(" WHERE                                                                                               ")
        'SQLStr.AppendLine("      LNT0002.DELFLG = '0'                                                                           ")
        'SQLStr.AppendLine(" AND  COALESCE(LNT0002.TARGETYM, '0') = COALESCE(@TARGETYM, '0')                                     ")
        'SQLStr.AppendLine(" AND  COALESCE(LNT0002.TORICODE, '0') = COALESCE(@TORICODE, '0')                                     ")
        'SQLStr.AppendLine(" AND  COALESCE(LNT0002.ORGCODE, '0') = COALESCE(@ORGCODE, '0')                                       ")

        'Try
        '    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
        '        '対象年月
        '        Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)
        '        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)
        '        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)

        '        P_TARGETYM.Value = WW_ROW("TARGETYM")
        '        P_TORICODE.Value = WW_ROW("TORICODE")
        '        P_ORGCODE.Value = WW_ROW("ORGCODE")

        '        Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
        '            '○ フィールド名とフィールドの型を取得
        '            For index As Integer = 0 To SQLdr.FieldCount - 1
        '                LNT0002tblHIST.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
        '            Next

        '            '○ テーブル検索結果をテーブル格納
        '            LNT0002tblHIST.Load(SQLdr)
        '        End Using

        '        Dim i As Integer = 0
        '        For Each LNT0002row As DataRow In LNT0002tblHIST.Rows
        '            i += 1
        '            LNT0002row("LINECNT") = i        'LINECNT

        '            LNT0002row("HISTCONTROL") = CONST_BTNHISTOUT

        '        Next
        '    End Using
        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002L SELECT")
        '    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '    CS0011LOGWrite.INFPOSI = "DB:LNT0002L Select"
        '    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '    CS0011LOGWrite.TEXT = ex.ToString()
        '    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        '    Exit Sub
        'End Try
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
End Class


