'************************************************************
' 実績取込
' 作成日 2024/12/01
' 更新日 
' 作成者 
' 更新者 
'
' 修正履歴 
'************************************************************

Imports GrapeCity.Documents.Excel
Imports Newtonsoft.Json
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001ZissekiIntake
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNT0003tbl As DataTable                                  '一覧（実績取込履歴）格納用テーブル
    Private LNT0028tbl As DataTable                                  '実績不備データ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数
    Private Const CONST_LNT0001 As String = "LNG.LNT0001_ZISSEKI"   '実績テーブル
    Private Const CONST_LNT0028 As String = "LNG.LNT0028_NGZISSEKI" '実績不備テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0007CheckAuthority As New CS0007CheckAuthority        '更新権限チェック
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザー情報取得
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              '固定値マスタ
    Private CS0054KintoneApi As New CS0054KintoneApi                'KintoneAPI（アボカドデータ取得）
    Private CMNPTS As New CmnParts                                  '共通関数

    '○ 共通処理結果
    Private WW_ErrSW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード

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
                    Master.RecoverTable(LNT0003tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '検索ボタンクリック
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonKintone"         '実績取込（アボカド）ボタンクリック（確認ポップアップ表示）
                            WF_KintoneGetconfirm_Click()
                        Case "btnCommonConfirmOk"       '実績取込ポップアップ（はい）ボタンクリック（アボカドデータ取得処理）
                            WF_KintoneGetRecodes_Click()
                        Case "WF_ButtonZero"， "btnCommonConfirmYes"            '実績数量ゼロボタンクリック
                            WF_ButtonZero_Click()
                        Case "WF_ButtonNgPrint"         '実績不備データボタンクリック
                            WF_ButtonNgPrint_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FiledDBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ButtonEND", "LNT0001L" '戻るボタン押下（LNT0001Lは、パンくずより）
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
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
            If Not IsNothing(LNT0003tbl) Then
                LNT0003tbl.Clear()
                LNT0003tbl.Dispose()
                LNT0003tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0001WRKINC.MAPIDD
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()
        '○ アボカドデータ保存先のファイル名
        WW_CreateXMLSaveFile()

        '○ 初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightviewR.ResetIndex()
        leftview.ActiveListBox()

        '○ 右Boxへの値設定
        rightviewR.MAPID = Master.MAPID
        rightviewR.MAPVARI = Master.MAPvariant
        rightviewR.COMPCODE = Master.USERCAMP
        rightviewR.PROFID = Master.PROF_REPORT
        rightviewR.Initialize("")

        '○ RightBox情報設定
        rightviewD.MAPIDS = Master.MAPID
        rightviewD.MAPID = Master.MAPID
        rightviewD.COMPCODE = Master.USERCAMP
        rightviewD.MAPVARI = Master.MAPvariant
        rightviewD.PROFID = Master.PROF_VIEW
        rightviewD.MENUROLE = Master.ROLE_MENU
        rightviewD.MAPROLE = Master.ROLE_MAP
        rightviewD.VIEWROLE = Master.ROLE_VIEWPROF
        rightviewD.RPRTROLE = Master.ROLE_RPRTPROF

        rightviewD.Initialize("画面レイアウト設定", WW_Dummy)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001L Then
            ' メニューからの画面遷移
            ' 画面間の情報クリア
            work.Initialize()

            ' 初期変数設定処理
            WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001Z Then
            WF_TaishoYm.Value = work.WF_SEL_YM.Text
        End If

        ' ドロップダウンリスト（荷主）作成
        Dim toriList As New ListBox
        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
        GS0007FIXVALUElst.CLAS = "TORICODEDROP"
        GS0007FIXVALUElst.LISTBOX1 = toriList
        GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = ""
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        If Not isNormal(GS0007FIXVALUElst.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "固定値取得エラー")
            Exit Sub
        End If

        'ログインユーザーと指定された荷主より操作可能なアボカド接続情報（営業所毎）取得
        Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, "")

        Dim SaveIdx As Integer = 0
        Dim FindFlg As Integer = 0
        WF_TORI.Items.Clear()
        'WF_TORI.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To toriList.Items.Count - 1
            'ApiInfo(リスト）中に指定された取引先が存在した場合、ドロップダウンリストを作成する
            Dim toriLike As String = "*" & toriList.Items(i).Value & "*"
            Dim exists As Boolean = ApiInfo.Any(Function(p) p.Tori Like toriLike)
            If exists Then
                WF_TORI.Items.Add(New ListItem(toriList.Items(i).Text, toriList.Items(i).Value))
            End If
        Next

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0003tbl)

        '〇 一覧の件数を取得
        'Me.ListCount.Text = "件数：" + LNT0003tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0003tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightviewD.GetViewId(Master.USERCAMP)
        End If
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
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

        If IsNothing(LNT0003tbl) Then
            LNT0003tbl = New DataTable
        End If

        If LNT0003tbl.Columns.Count <> 0 Then
            LNT0003tbl.Columns.Clear()
        End If

        LNT0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを実績取込履歴から取得する
        Dim SQLStr As String =
              " Select                                                                 " _
            & "     1                                                   AS 'SELECT'    " _
            & "   , 0                                                   AS HIDDEN      " _
            & "   , 0                                                   AS LINECNT     " _
            & "   , ''                                                  AS OPERATION   " _
            & "   , coalesce(RTRIM(LT3.TAISHOYM), '')                   AS TAISHOYM    " _
            & "   , coalesce(RTRIM(LT3.TORICODE), '')                   AS TORICODE    " _
            & "   , coalesce(RTRIM(LT3.TORINAME), '')                   AS TORINAME    " _
            & "   , coalesce(RTRIM(LT3.SHIPORG), '')                    AS SHIPORG     " _
            & "   , coalesce(RTRIM(LT3.SHIPORGNAME), '')                AS SHIPORGNAME " _
            & "   , coalesce(RTRIM(LT3.USERID), '')                     AS USERID      " _
            & "   , coalesce(RTRIM(LT3.USERNAME), '')                   AS USERNAME    " _
            & "   , date_format(LT3.INTAKEDATE, '%Y/%m/%d %H:%i:%s')    AS INTAKEDATE  " _
            & " FROM                                                                   " _
            & "     LNG.LNT0003_ZISSEKIHIST LT3                                        " _
            & " WHERE                                                                  " _
            & "     LT3.TAISHOYM = @P1                                                 "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ' 取引先
        If WF_TORIhdn.Value <> "" Then
            SQLStr += " AND LT3.TORICODE in (" & WF_TORIhdn.Value & ")"
        End If

        '部署
        Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, WF_TORIhdn.Value)
        If ApiInfo.Count > 0 Then
            SQLStr += " AND LT3.SHIPORG in ("
            For j As Integer = 0 To ApiInfo.Count - 1
                SQLStr += "'"
                SQLStr += ApiInfo(j).Org
                SQLStr += "'"
                If j < ApiInfo.Count - 1 Then
                    SQLStr += ","
                Else
                    SQLStr += ")"
                End If
            Next
        End If

        SQLStr += " ORDER BY                                                               " _
                & "     LT3.INTAKEDATE DESC                                                "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Decimal, 6)  '対象年月
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                    PARA1.Value = CDate(WF_TaishoYm.Value & "/01").ToString("yyyyMM")
                Else
                    PARA1.Value = Date.Now.ToString("yyyyMM")
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0003row As DataRow In LNT0003tbl.Rows
                    i += 1
                    LNT0003row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0023row As DataRow In LNT0003tbl.Rows
            If LNM0023row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0023row("SELECT") = WW_DataCNT
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

        '〇 一覧の件数を取得
        'Me.ListCount.Text = "件数：" + LNT0003tbl.Rows.Count.ToString()

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0003tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightviewD.GetViewId(Master.USERCAMP)
        End If
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        'CS0013ProfView.HIDENOOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
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
    ''' 絞り込みボタン押下
    ''' </summary>
    Private Sub WF_ButtonExtract_Click()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            ' 画面選択された荷主を取得
            SelectTori()

            ' 画面表示データを取得
            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0003tbl)

        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 実績数量ゼロボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonZero_Click()

        work.WF_SEL_YM.Text = WF_TaishoYm.Value
        '実績数量ゼロボタン押下の場合
        If WF_ButtonClick.Value = "WF_ButtonZero" Then
            ' 画面選択された荷主を取得
            SelectTori()
            work.WF_SEL_TORICODE.Text = WF_TORIhdn.Value
        End If

        Dim WW_URL As String = ""
        work.GetURL(LNT0001WRKINC.MAPIDZ, WW_URL)

        Server.Transfer(WW_URL)

    End Sub

    ''' <summary>
    ''' アボカド不備データﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonNgPrint_Click()

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            ' 画面選択された荷主を取得
            SelectTori()

            '荷主選択チェック
            If Me.WF_TORIhdn.Value = "" Then
                Master.Output(C_MESSAGE_NO.CTN_INPUT_ERR, C_MESSAGE_TYPE.ERR, "荷主", "", True)
                Exit Sub
            End If

            ' 画面表示データを取得
            NgPrintDataGet(SQLcon)

            'データ存在チェック
            If LNT0028tbl.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.WAR, "不備データは存在しません", "", True)
                Exit Sub
            End If
        End Using

        '固定費マスタ（荷主別チェックする項目を取得）
        Dim ChkTbl As DataTable = GetFixvale()

        '○ 画面表示データ保存
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightviewR.GetReportId()        '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = LNT0028tbl                       'データ参照  Table
        CS0030REPORT.CHKTBL = ChkTbl                            '荷主別チェック項目  Table
        CS0030REPORT.FILENAME = "実績不備データ"                '出力ファイル名
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
    ''' 不備データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub NgPrintDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0028tbl) Then
            LNT0028tbl = New DataTable
        End If

        If LNT0028tbl.Columns.Count <> 0 Then
            LNT0028tbl.Columns.Clear()
        End If

        LNT0028tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As String =
              " Select                                                                              " _
            & "      1                                                    AS 'SELECT'               " _
            & "     ,0                                                    AS HIDDEN                 " _
            & "     ,0                                                    AS LINECNT                " _
            & "     ,''                                                   AS OPERATION              " _
            & "     ,coalesce(LT28.RECONO, '')                             AS RECONO			        " _
            & "     ,coalesce(LT28.LOADUNLOTYPE, '')                       AS LOADUNLOTYPE		    " _
            & "     ,coalesce(LT28.STACKINGTYPE, '')                       AS STACKINGTYPE		    " _
            & "     ,coalesce(LT28.HSETID, '')                             AS HSETID			        " _
            & "     ,coalesce(LT28.ORDERORGSELECT, '')                     AS ORDERORGSELECT	        " _
            & "     ,coalesce(LT28.ORDERORGNAME, '')                       AS ORDERORGNAME		    " _
            & "     ,coalesce(LT28.ORDERORGCODE, '')                       AS ORDERORGCODE		    " _
            & "     ,coalesce(LT28.ORDERORGNAMES, '')                      AS ORDERORGNAMES	        " _
            & "     ,coalesce(LT28.KASANAMEORDERORG, '')                   AS KASANAMEORDERORG	    " _
            & "     ,coalesce(LT28.KASANCODEORDERORG, '')                  AS KASANCODEORDERORG	    " _
            & "     ,coalesce(LT28.KASANAMESORDERORG, '')                  AS KASANAMESORDERORG	    " _
            & "     ,coalesce(LT28.ORDERORG, '')                           AS ORDERORG				" _
            & "     ,coalesce(LT28.KASANORDERORG, '')                      AS KASANORDERORG		    " _
            & "     ,coalesce(LT28.PRODUCTSLCT, '')                        AS PRODUCTSLCT			" _
            & "     ,coalesce(LT28.PRODUCTSYOSAI, '')                      AS PRODUCTSYOSAI		    " _
            & "     ,coalesce(LT28.PRODUCT2NAME, '')                       AS PRODUCT2NAME			" _
            & "     ,coalesce(LT28.PRODUCT2, '')                           AS PRODUCT2				" _
            & "     ,coalesce(LT28.PRODUCT1NAME, '')                       AS PRODUCT1NAME			" _
            & "     ,coalesce(LT28.PRODUCT1, '')                           AS PRODUCT1				" _
            & "     ,coalesce(LT28.OILNAME, '')                            AS OILNAME				" _
            & "     ,coalesce(LT28.OILTYPE, '')                            AS OILTYPE				" _
            & "     ,coalesce(LT28.TODOKESLCT, '')                         AS TODOKESLCT			    " _
            & "     ,coalesce(LT28.TODOKECODE, '')                         AS TODOKECODE			    " _
            & "     ,coalesce(LT28.TODOKENAME, '')                         AS TODOKENAME			    " _
            & "     ,coalesce(LT28.TODOKENAMES, '')                        AS TODOKENAMES			" _
            & "     ,coalesce(LT28.TORICODE, '')                           AS TORICODE				" _
            & "     ,coalesce(LT28.TORINAME, '')                           AS TORINAME				" _
            & "     ,coalesce(LT28.TODOKEADDR, '')                         AS TODOKEADDR			    " _
            & "     ,coalesce(LT28.TODOKETEL, '')                          AS TODOKETEL			    " _
            & "     ,coalesce(LT28.TODOKEMAP, '')                          AS TODOKEMAP			    " _
            & "     ,coalesce(LT28.TODOKEIDO, '')                          AS TODOKEIDO			    " _
            & "     ,coalesce(LT28.TODOKEKEIDO, '')                        AS TODOKEKEIDO			" _
            & "     ,coalesce(LT28.TODOKEBIKO1, '')                        AS TODOKEBIKO1			" _
            & "     ,coalesce(LT28.TODOKEBIKO2, '')                        AS TODOKEBIKO2			" _
            & "     ,coalesce(LT28.TODOKEBIKO3, '')                        AS TODOKEBIKO3			" _
            & "     ,coalesce(LT28.SHUKASLCT, '')                          AS SHUKASLCT			    " _
            & "     ,coalesce(LT28.SHUKABASHO, '')                         AS SHUKABASHO			    " _
            & "     ,coalesce(LT28.SHUKANAME, '')                          AS SHUKANAME			    " _
            & "     ,coalesce(LT28.SHUKANAMES, '')                         AS SHUKANAMES			    " _
            & "     ,coalesce(LT28.SHUKATORICODE, '')                      AS SHUKATORICODE		    " _
            & "     ,coalesce(LT28.SHUKATORINAME, '')                      AS SHUKATORINAME		    " _
            & "     ,coalesce(LT28.SHUKAADDR, '')                          AS SHUKAADDR			    " _
            & "     ,coalesce(LT28.SHUKAADDRTEL, '')                       AS SHUKAADDRTEL			" _
            & "     ,coalesce(LT28.SHUKAMAP, '')                           AS SHUKAMAP				" _
            & "     ,coalesce(LT28.SHUKAIDO, '')                           AS SHUKAIDO				" _
            & "     ,coalesce(LT28.SHUKAKEIDO, '')                         AS SHUKAKEIDO			    " _
            & "     ,coalesce(LT28.SHUKABIKOU1, '')                        AS SHUKABIKOU1			" _
            & "     ,coalesce(LT28.SHUKABIKOU2, '')                        AS SHUKABIKOU2			" _
            & "     ,coalesce(LT28.SHUKABIKOU3, '')                        AS SHUKABIKOU3			" _
            & "     ,coalesce(LT28.SHUKADATE, '')                          AS SHUKADATE			    " _
            & "     ,coalesce(LT28.LOADTIME, '')                           AS LOADTIME				" _
            & "     ,coalesce(LT28.LOADTIMEIN, '')                         AS LOADTIMEIN			    " _
            & "     ,coalesce(LT28.LOADTIMES, '')                          AS LOADTIMES			    " _
            & "     ,coalesce(LT28.TODOKEDATE, '')                         AS TODOKEDATE			    " _
            & "     ,coalesce(LT28.SHITEITIME, '')                         AS SHITEITIME			    " _
            & "     ,coalesce(LT28.SHITEITIMEIN, '')                       AS SHITEITIMEIN			" _
            & "     ,coalesce(LT28.SHITEITIMES, '')                        AS SHITEITIMES			" _
            & "     ,coalesce(LT28.ZYUTYU, '')                             AS ZYUTYU				    " _
            & "     ,coalesce(LT28.ZISSEKI, '')                            AS ZISSEKI				" _
            & "     ,coalesce(LT28.TANNI, '')                              AS TANNI				    " _
            & "     ,coalesce(LT28.GYOUMUSIZI1, '')                        AS GYOUMUSIZI1			" _
            & "     ,coalesce(LT28.GYOUMUSIZI2, '')                        AS GYOUMUSIZI2			" _
            & "     ,coalesce(LT28.GYOUMUSIZI3, '')                        AS GYOUMUSIZI3			" _
            & "     ,coalesce(LT28.NINUSHIBIKOU, '')                       AS NINUSHIBIKOU			" _
            & "     ,coalesce(LT28.GYOMUSYABAN, '')                        AS GYOMUSYABAN			" _
            & "     ,coalesce(LT28.SHIPORGNAME, '')                        AS SHIPORGNAME			" _
            & "     ,coalesce(LT28.SHIPORG, '')                            AS SHIPORG				" _
            & "     ,coalesce(LT28.SHIPORGNAMES, '')                       AS SHIPORGNAMES			" _
            & "     ,coalesce(LT28.KASANSHIPORGNAME, '')                   AS KASANSHIPORGNAME	    " _
            & "     ,coalesce(LT28.KASANSHIPORG, '')                       AS KASANSHIPORG			" _
            & "     ,coalesce(LT28.KASANSHIPORGNAMES, '')                  AS KASANSHIPORGNAMES	    " _
            & "     ,coalesce(LT28.TANKNUM, '')                            AS TANKNUM				" _
            & "     ,coalesce(LT28.TANKNUMBER, '')                         AS TANKNUMBER			    " _
            & "     ,coalesce(LT28.SYAGATA, '')                            AS SYAGATA				" _
            & "     ,coalesce(LT28.SYABARA, '')                            AS SYABARA				" _
            & "     ,coalesce(LT28.NINUSHINAME, '')                        AS NINUSHINAME			" _
            & "     ,coalesce(LT28.CONTYPE, '')                            AS CONTYPE				" _
            & "     ,coalesce(LT28.PRO1SYARYOU, '')                        AS PRO1SYARYOU			" _
            & "     ,coalesce(LT28.TANKMEMO, '')                           AS TANKMEMO				" _
            & "     ,coalesce(LT28.TANKBIKOU1, '')                         AS TANKBIKOU1			    " _
            & "     ,coalesce(LT28.TANKBIKOU2, '')                         AS TANKBIKOU2			    " _
            & "     ,coalesce(LT28.TANKBIKOU3, '')                         AS TANKBIKOU3			    " _
            & "     ,coalesce(LT28.TRACTORNUM, '')                         AS TRACTORNUM			    " _
            & "     ,coalesce(LT28.TRACTORNUMBER, '')                      AS TRACTORNUMBER		    " _
            & "     ,coalesce(LT28.TRIP, '')                               AS TRIP					" _
            & "     ,coalesce(LT28.DRP, '')                                AS DRP					" _
            & "     ,coalesce(LT28.UNKOUMEMO, '')                          AS UNKOUMEMO			    " _
            & "     ,coalesce(LT28.SHUKKINTIME, '')                        AS SHUKKINTIME			" _
            & "     ,coalesce(LT28.STAFFSLCT, '')                          AS STAFFSLCT			    " _
            & "     ,coalesce(LT28.STAFFNAME, '')                          AS STAFFNAME			    " _
            & "     ,coalesce(LT28.STAFFCODE, '')                          AS STAFFCODE			    " _
            & "     ,coalesce(LT28.SUBSTAFFSLCT, '')                       AS SUBSTAFFSLCT			" _
            & "     ,coalesce(LT28.SUBSTAFFNAME, '')                       AS SUBSTAFFNAME			" _
            & "     ,coalesce(LT28.SUBSTAFFNUM, '')                        AS SUBSTAFFNUM			" _
            & "     ,coalesce(LT28.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM			" _
            & "     ,coalesce(LT28.YOUSYA, '')                             AS YOUSYA				    " _
            & "     ,coalesce(LT28.RECOTITLE, '')                          AS RECOTITLE			    " _
            & "     ,coalesce(LT28.SHUKODATE, '')                          AS SHUKODATE			    " _
            & "     ,coalesce(LT28.KIKODATE, '')                           AS KIKODATE				" _
            & "     ,coalesce(LT28.KIKOTIME, '')                           AS KIKOTIME				" _
            & "     ,coalesce(LT28.CREWBIKOU1, '')                         AS CREWBIKOU1			    " _
            & "     ,coalesce(LT28.CREWBIKOU2, '')                         AS CREWBIKOU2			    " _
            & "     ,coalesce(LT28.SUBCREWBIKOU1, '')                      AS SUBCREWBIKOU1		    " _
            & "     ,coalesce(LT28.SUBCREWBIKOU2, '')                      AS SUBCREWBIKOU2		    " _
            & "     ,coalesce(LT28.SUBSHUKKINTIME, '')                     AS SUBSHUKKINTIME		    " _
            & "     ,coalesce(LT28.SYABARATANNI, '')                       AS SYABARATANNI			" _
            & "     ,coalesce(LT28.TAIKINTIME, '')                         AS TAIKINTIME			    " _
            & "     ,coalesce(LT28.SUBTIKINTIME, '')                       AS SUBTIKINTIME			" _
            & "     ,coalesce(LT28.KVTITLE, '')                            AS KVTITLE				" _
            & "     ,coalesce(LT28.KVZYUTYU, '')                           AS KVZYUTYU				" _
            & "     ,coalesce(LT28.KVZISSEKI, '')                          AS KVZISSEKI			    " _
            & "     ,coalesce(LT28.KVCREW, '')                             AS KVCREW				    " _
            & "     ,coalesce(LT28.CREWCODE, '')                           AS CREWCODE				" _
            & "     ,coalesce(LT28.SUBCREWCODE, '')                        AS SUBCREWCODE			" _
            & "     ,coalesce(LT28.KVSUBCREW, '')                          AS KVSUBCREW			    " _
            & "     ,coalesce(LT28.ORDERHENKO, '')                         AS ORDERHENKO			    " _
            & "     ,coalesce(LT28.RIKUUNKYOKU, '')                        AS RIKUUNKYOKU			" _
            & "     ,coalesce(LT28.BUNRUINUMBER, '')                       AS BUNRUINUMBER			" _
            & "     ,coalesce(LT28.HIRAGANA, '')                           AS HIRAGANA				" _
            & "     ,coalesce(LT28.ITIRENNUM, '')                          AS ITIRENNUM			    " _
            & "     ,coalesce(LT28.TRACTER1, '')                           AS TRACTER1				" _
            & "     ,coalesce(LT28.TRACTER2, '')                           AS TRACTER2				" _
            & "     ,coalesce(LT28.TRACTER3, '')                           AS TRACTER3				" _
            & "     ,coalesce(LT28.TRACTER4, '')                           AS TRACTER4				" _
            & "     ,coalesce(LT28.TRACTER5, '')                           AS TRACTER5				" _
            & "     ,coalesce(LT28.TRACTER6, '')                           AS TRACTER6				" _
            & "     ,coalesce(LT28.TRACTER7, '')                           AS TRACTER7				" _
            & "     ,coalesce(LT28.HAISYAHUKA, '')                         AS HAISYAHUKA			    " _
            & "     ,coalesce(LT28.HYOZIZYUNT, '')                         AS HYOZIZYUNT			    " _
            & "     ,coalesce(LT28.HYOZIZYUNH, '')                         AS HYOZIZYUNH			    " _
            & "     ,coalesce(LT28.HONTRACTER1, '')                        AS HONTRACTER1			" _
            & "     ,coalesce(LT28.HONTRACTER2, '')                        AS HONTRACTER2			" _
            & "     ,coalesce(LT28.HONTRACTER3, '')                        AS HONTRACTER3			" _
            & "     ,coalesce(LT28.HONTRACTER4, '')                        AS HONTRACTER4			" _
            & "     ,coalesce(LT28.HONTRACTER5, '')                        AS HONTRACTER5			" _
            & "     ,coalesce(LT28.HONTRACTER6, '')                        AS HONTRACTER6			" _
            & "     ,coalesce(LT28.HONTRACTER7, '')                        AS HONTRACTER7			" _
            & "     ,coalesce(LT28.HONTRACTER8, '')                        AS HONTRACTER8			" _
            & "     ,coalesce(LT28.HONTRACTER9, '')                        AS HONTRACTER9			" _
            & "     ,coalesce(LT28.HONTRACTER10, '')                       AS HONTRACTER10			" _
            & "     ,coalesce(LT28.HONTRACTER11, '')                       AS HONTRACTER11			" _
            & "     ,coalesce(LT28.HONTRACTER12, '')                       AS HONTRACTER12			" _
            & "     ,coalesce(LT28.HONTRACTER13, '')                       AS HONTRACTER13			" _
            & "     ,coalesce(LT28.HONTRACTER14, '')                       AS HONTRACTER14			" _
            & "     ,coalesce(LT28.HONTRACTER15, '')                       AS HONTRACTER15			" _
            & "     ,coalesce(LT28.HONTRACTER16, '')                       AS HONTRACTER16			" _
            & "     ,coalesce(LT28.HONTRACTER17, '')                       AS HONTRACTER17			" _
            & "     ,coalesce(LT28.HONTRACTER18, '')                       AS HONTRACTER18			" _
            & "     ,coalesce(LT28.HONTRACTER19, '')                       AS HONTRACTER19			" _
            & "     ,coalesce(LT28.HONTRACTER20, '')                       AS HONTRACTER20			" _
            & "     ,coalesce(LT28.HONTRACTER21, '')                       AS HONTRACTER21			" _
            & "     ,coalesce(LT28.HONTRACTER22, '')                       AS HONTRACTER22			" _
            & "     ,coalesce(LT28.HONTRACTER23, '')                       AS HONTRACTER23			" _
            & "     ,coalesce(LT28.HONTRACTER24, '')                       AS HONTRACTER24			" _
            & "     ,coalesce(LT28.HONTRACTER25, '')                       AS HONTRACTER25			" _
            & "     ,coalesce(LT28.UPDATEUSER, '')                         AS UPDATEUSER			    " _
            & "     ,coalesce(LT28.CREATEUSER, '')                         AS CREATEUSER			    " _
            & "     ,coalesce(LT28.UPDATEYMD, '')                          AS UPDATEYMD			    " _
            & "     ,coalesce(LT28.CREATEYMD, '')                          AS CREATEYMD			    " _
            & "     ,coalesce(LT28.DELFLG, '')                             AS DELFLG				    " _
            & "     ,coalesce(LT28.INITYMD, '')                            AS INITYMD				" _
            & "     ,coalesce(LT28.INITUSER, '')                           AS INITUSER				" _
            & "     ,coalesce(LT28.INITTERMID, '')                         AS INITTERMID			    " _
            & "     ,coalesce(LT28.INITPGID, '')                           AS INITPGID				" _
            & "     ,coalesce(LT28.UPDYMD, '')                             AS UPDYMD				    " _
            & "     ,coalesce(LT28.UPDUSER, '')                            AS UPDUSER				" _
            & "     ,coalesce(LT28.UPDTERMID, '')                          AS UPDTERMID			    " _
            & "     ,coalesce(LT28.UPDPGID, '')                            AS UPDPGID				" _
            & "     ,coalesce(LT28.RECEIVEYMD, '')                         AS RECEIVEYMD			    " _
            & "     ,coalesce(LT28.UPDTIMSTP, '')                          AS UPDTIMSTP			    " _
            & " FROM                                                                                " _
            & "     LNG.LNT0028_NGZISSEKI LT28                                                       " _
            & " WHERE                                                                               " _
            & "     date_format(LT28.TODOKEDATE, '%Y/%m/%d') >= @P1                                  " _
            & " AND date_format(LT28.TODOKEDATE, '%Y/%m/%d') <= @P2                                  " _

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ' 取引先
        If WF_TORIhdn.Value <> "" Then
            SQLStr += " AND LT28.TORICODE in (" & WF_TORIhdn.Value & ")"
        End If

        '部署
        Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, WF_TORIhdn.Value)
        If ApiInfo.Count > 0 Then
            SQLStr += " AND LT28.ORDERORG in ("
            For j As Integer = 0 To ApiInfo.Count - 1
                SQLStr += "'"
                SQLStr += ApiInfo(j).Org
                SQLStr += "'"
                If j < ApiInfo.Count - 1 Then
                    SQLStr += ","
                Else
                    SQLStr += ")"
                End If
            Next
        End If

        SQLStr += " AND LT28.DELFLG = '0'                                                " _
                & " ORDER BY                                                            " _
                & "     LT28.ORDERORG, LT28.TORICODE, LT28.SHUKADATE, LT28.TODOKEDATE, LT28.STAFFCODE  "


        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Date)  '届日FROM
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)  '届日TO
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                    PARA1.Value = WF_TaishoYm.Value & "/01"
                    PARA2.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                Else
                    PARA1.Value = Date.Now.ToString("yyyy/MM") & "/01"
                    PARA2.Value = Date.Now.ToString("yyyy/MM") & DateTime.DaysInMonth(Date.Now.Year, Date.Now.Month).ToString("/00")
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0028tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0028tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0028row As DataRow In LNT0028tbl.Rows
                    i += 1
                    LNT0028row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0028 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0028 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledDBClick()

        Dim WW_PrmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_TaishoYm"         '作成日時
                                .WF_Calendar.Text = WF_TaishoYm.Value
                        End Select
                        .ActiveCalendar()
                End Select
            End With
        End If

    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_SelectDate As Date

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_TaishoYm"             '対象年月
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        WF_TaishoYm.Value = ""
                    Else
                        WF_TaishoYm.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WF_TaishoYm.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_TaishoYm"             '対象年月
                WF_TaishoYm.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
        Dim TBLview As New DataView(LNT0003tbl)
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
    ''' 退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

#Region "アボカドシステム（Kintone）レコード取得"

    ''' <summary>
    ''' アボカド（Kintone）受信確認
    ''' </summary>
    Private Sub WF_KintoneGetconfirm_Click()

        '対象年月チェック
        Dim result As DateTime
        If Not DateTime.TryParseExact(Me.WF_TaishoYm.Value & "/01", "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, result) Then
            Master.Output(C_MESSAGE_NO.CTN_INPUT_DATE_ERR, C_MESSAGE_TYPE.ERR, "対象年月", "", True)
            Exit Sub
        End If

        ' 画面選択された荷主を取得
        SelectTori()

        '荷主選択チェック
        If Me.WF_TORIhdn.Value = "" Then
            Master.Output(C_MESSAGE_NO.CTN_INPUT_ERR, C_MESSAGE_TYPE.ERR, "荷主", "", True)
            Exit Sub
        End If

        ' 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        Dim Msg1 As String = ""
        Dim Msg2 As String = ""
        Dim selCnt As Integer = 0
        Dim MsgType As String = C_MESSAGE_TYPE.INF
        Dim toriSplit() As String = WF_TORIhdn.Value.Split(",")
        Dim toriNameList() As String = WF_TORINAMEhdn.Value.Split(",")
        Dim sp As String = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"

        Msg1 += "実績取込を行います"
        Msg1 += "<BR>対象年月：" & Me.WF_TaishoYm.Value
        For i As Integer = 0 To toriSplit.Count - 1
            Dim condition As String = String.Format("TORICODE='{0}'", toriSplit(i))
            Dim selRow() = LNT0003tbl.Select(condition)
            If selRow.Count = 0 Then
                Msg1 += "<BR>" & sp & "荷主：" & toriNameList(i)
            End If
        Next

        selCnt = 0
        For i As Integer = 0 To toriSplit.Count - 1
            Dim condition As String = String.Format("TORICODE='{0}'", toriSplit(i))
            Dim selRow() = LNT0003tbl.Select(condition)
            If selRow.Count = 0 Then
            Else
                selCnt += 1
                If selCnt = 1 Then
                    MsgType = C_MESSAGE_TYPE.WAR
                    Msg1 += "<BR>" & sp & "<span style='color:red;'>次の荷主は、既に実績を取り込み済ですがよろしいですか？</span>"
                End If
                Msg1 += "<BR>" & sp & "荷主：" & toriNameList(i)
            End If
        Next


        'If LNT0003tbl.Rows.Count = 0 Then
        '    Msg1 = "実績取込を行います"
        '    MsgType = C_MESSAGE_TYPE.INF
        'Else
        '    Msg1 = "既に実績を取り込み済ですがよろしいですか？"
        '    MsgType = C_MESSAGE_TYPE.WAR
        'End If

        'Dim Msg2 As String = "<BR>対象年月：" & Me.WF_TaishoYm.Value
        'Msg2 += "&nbsp;&nbsp;&nbsp;&nbsp;荷主：" & Me.WF_TORI.Items(Me.WF_TORI.SelectedIndex).Text

        Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, MsgType, Msg1, Msg2, True, "", True)

    End Sub
    ''' <summary>
    ''' 荷主プルダウン選択値取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SelectTori()

        Me.WF_TORIhdn.Value = ""
        Me.WF_TORINAMEhdn.Value = ""

        If Me.WF_TORI.Items.Count > 0 Then
            Dim SelectedCount As Integer = 0
            Dim intSelCnt As Integer = 0
            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To WF_TORI.Items.Count - 1
                If WF_TORI.Items(index).Selected = True Then
                    SelectedCount += 1
                    If intSelCnt = 0 Then
                        Me.WF_TORIhdn.Value = WF_TORI.Items(index).Value
                        Me.WF_TORINAMEhdn.Value = WF_TORI.Items(index).Text
                        intSelCnt = 1
                    Else
                        Me.WF_TORIhdn.Value = Me.WF_TORIhdn.Value & "," & WF_TORI.Items(index).Value
                        Me.WF_TORINAMEhdn.Value = Me.WF_TORINAMEhdn.Value & "," & WF_TORI.Items(index).Text
                        intSelCnt = 2
                    End If
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' アボカドシステム（Kintone）レコード取得
    ''' </summary>
    Private Sub WF_KintoneGetRecodes_Click()

        Dim LNT0001tbl As DataTable = New DataTable
        Dim LNT0001tbl_SV As DataTable = New DataTable

        Try
            'アボカドデータ取得テーブル作成
            CS0054KintoneApi.CreateDataTable(LNT0001tbl)
            LNT0001tbl_SV = LNT0001tbl.Clone

            'ログインユーザーと指定された荷主より操作可能なアボカド接続情報（営業所毎）取得
            Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, WF_TORIhdn.Value)

            '○ 画面表示データ取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続
                '-----------------------------------------------------
                '指定された荷主に該当する営業所分の処理を行う
                '-----------------------------------------------------
                For i As Integer = 0 To ApiInfo.Count - 1
                    'WebAPI実行（アボカドデータ取得）
                    CS0054KintoneApi.ApiApplId = ApiInfo(i).AppId
                    CS0054KintoneApi.ApiToken = ApiInfo(i).Token
                    CS0054KintoneApi.ToriCode = WF_TORIhdn.Value
                    CS0054KintoneApi.OrgCode = ApiInfo(i).Org
                    CS0054KintoneApi.YmdFrom = WF_TaishoYm.Value & "/01"
                    CS0054KintoneApi.YmdTo = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    LNT0001tbl = CS0054KintoneApi.GetRecords()

                    If LNT0001tbl.Rows.Count > 0 Then
                        'アボカドデータチェック（荷主別）
                        ZissekiCheck(LNT0001tbl)

                        '実績テーブル、実績履歴テーブル更新（アボカドデータ保存）
                        ZissekiUpdate(ApiInfo(i).Org, WF_TORIhdn.Value, LNT0001tbl, WW_ErrSW)
                        If WW_ErrSW <> C_MESSAGE_NO.NORMAL Then
                            Exit Sub
                        End If
                    End If

                    '取得データ保存（累積）
                    LNT0001tbl_SV.Merge(LNT0001tbl)
                Next

                '更新された実績テーブルから輸送費テーブルの金額計算をし、更新
                YusouhiUpdate(WF_TORIhdn.Value, WF_TaishoYm.ToString)
                If WW_ErrSW <> C_MESSAGE_NO.NORMAL Then
                    Exit Sub
                End If

                '〇届先マスタ差分抽出＆追加処理
                TODOKESAKIMAS_Insert()

                ' 画面選択された荷主を取得
                SelectTori()
                '○ 画面表示データ取得
                MAPDataGet(SQLcon)
            End Using

            '○ 画面表示データ保存
            Master.SaveTable(LNT0003tbl)

            'アボカドデータ保存（念のため調査用にダウンロードできるようにする）
            Master.SaveTable(LNT0001tbl_SV, work.WF_SEL_INPTBL.Text)

            Dim Cnt As Integer = 0
            Dim Msg1 As String = ""
            Dim Msg2 As String = ""
            Dim sp As String = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
            '実績数量=未入力の存在確認（数量=0は、AVOCADOではキャンセルオーダーはCS0054KintoneApiで既に読み飛ばし）
            Dim dv As New DataView(LNT0001tbl_SV)
            dv.RowFilter = "積置区分 <> '積置' and 実績数量 = ''"
            If dv.Count > 0 Then
                ' LINQを使用してグループ化とカウントを取得
                Dim query = From row In dv.ToTable().AsEnumerable()
                            Group row By toricode = row.Field(Of String)("届先取引先コード") Into Group
                            Select New With {
                            .toricode = toricode,
                            .Count = Group.Count()
                        }

                ' 結果を表示
                Msg1 += "実績数量=0のデータが存在します。画面表示しますか"
                Msg1 += "<BR>対象年月：" & Me.WF_TaishoYm.Value
                For Each result In query
                    If result.Count > 0 Then
                        Dim tori = WF_TORI.Items.FindByValue(result.toricode)
                        Msg1 += "<BR>" & sp & "荷主：" & tori.Text

                        '実績数量ゼロ画面への引き渡し情報（複数存在する場合、カンマ区切り：取1,取2,取3）
                        Cnt += 1
                        If Cnt = 1 Then
                            work.WF_SEL_TORICODE.Text = tori.Value
                        Else
                            work.WF_SEL_TORICODE.Text += "," & tori.Value
                        End If
                    End If
                Next
            End If

            dv.RowFilter = "OUTTBL = '" & CONST_LNT0028 & "'"
            If dv.Count > 0 Then
                ' LINQを使用してグループ化とカウントを取得
                Dim query = From row In dv.ToTable().AsEnumerable()
                            Group row By toricode = row.Field(Of String)("届先取引先コード") Into Group
                            Select New With {
                            .toricode = toricode,
                            .Count = Group.Count()
                        }

                ' 結果を表示
                If Cnt > 0 Then
                    Msg1 += "<BR><BR>"
                    Msg1 += sp & "不備データが存在します。"
                    Msg1 += "<BR>" & sp & "対象年月：" & Me.WF_TaishoYm.Value
                Else
                    Msg1 += "不備データが存在します。"
                    Msg1 += "<BR>" & "対象年月：" & Me.WF_TaishoYm.Value
                End If
                For Each result In query
                    If result.Count > 0 Then
                        Dim tori = WF_TORI.Items.FindByValue(result.toricode)
                        Msg1 += "<BR>" & sp & "荷主：" & tori.Text
                    End If
                Next

                '実績数量ゼロあり＆不備データありメッセージ出力
                'Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.WAR, Msg1, Msg2, True, "", True, "btnCommonConfirmYes")
                Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.WAR, Msg1, Msg2, True)
                Exit Sub
            End If

            If LNT0001tbl_SV.Rows.Count > 0 Then
                '正常メッセージ出力
                Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR, "", "", True)
            Else
                Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.WAR, "実績データが存在しません", "", True)
            End If

        Catch ex As Exception
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "実績取込（アボカド）データ取得失敗", "", True)
            'ログ出力
            CS0011LOGWrite.INFSUBCLASS = "LNT0001C"  'SUBクラス名
            CS0011LOGWrite.INFPOSI = "WF_Download"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()             'ログ出力
            Exit Sub
        Finally
            LNT0001tbl.Clear()
            LNT0001tbl.Dispose()
            LNT0001tbl = Nothing
            LNT0001tbl_SV.Clear()
            LNT0001tbl_SV.Dispose()
            LNT0001tbl_SV = Nothing
        End Try


    End Sub

    ''' <summary>
    ''' 固定値マスタ取得
    ''' </summary>
    Private Function GetFixvale() As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            '-- SELECT
            SQLStr &= " SELECT "
            SQLStr &= " S6.CAMPCODE"
            SQLStr &= ",S6.CLASS"
            SQLStr &= ",S6.KEYCODE"
            SQLStr &= ",S6.STYMD"
            SQLStr &= ",S6.ENDYMD"
            SQLStr &= ",S6.VALUE1"
            SQLStr &= ",S6.VALUE2"
            SQLStr &= ",S6.VALUE3"
            SQLStr &= ",S6.VALUE4"
            SQLStr &= ",S6.VALUE5"
            SQLStr &= ",S6.VALUE6"
            SQLStr &= ",S6.VALUE7"
            SQLStr &= ",S6.VALUE8"
            SQLStr &= ",S6.VALUE9"
            SQLStr &= ",S6.VALUE10"
            SQLStr &= ",S6.VALUE11"
            SQLStr &= ",S6.VALUE12"
            SQLStr &= ",S6.VALUE13"
            SQLStr &= ",S6.VALUE14"
            SQLStr &= ",S6.VALUE15"
            SQLStr &= ",S6.VALUE16"
            SQLStr &= ",S6.VALUE17"
            SQLStr &= ",S6.VALUE18"
            SQLStr &= ",S6.VALUE19"
            SQLStr &= ",S6.VALUE20"
            SQLStr &= ",S6.NAMES"
            SQLStr &= ",S6.NAMEL"
            SQLStr &= ",S6.SYSTEMKEYFLG"

            '-- FROM
            SQLStr &= " FROM COM.LNS0006_FIXVALUE S6 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     S6.CAMPCODE = '{0}' ", Master.USERCAMP)
            SQLStr &= String.Format(" AND S6.CLASS    = '{0}' ", "AVOCADOITEMCHK")
            SQLStr &= String.Format(" AND S6.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using
                End Using
            Catch ex As Exception
                Throw '呼び出し元の例外にスロー
            End Try
        End Using

        Return dt
    End Function



    ''' <summary>
    ''' アボカドデータチェック（荷主別）
    ''' </summary>
    Private Sub ZissekiCheck(ByVal iTbl As DataTable)

        '固定費マスタ（荷主別チェックする項目を取得）
        Dim ChkTbl As DataTable = GetFixvale()

        For Each DataRow As DataRow In iTbl.Rows
            '実績テーブルへの出力指示
            DataRow("OUTTBL") = CONST_LNT0001

            For Each ChkRow As DataRow In ChkTbl.Select("KEYCODE ='" & DataRow("届先取引先コード") & "'")
                For i As Integer = 1 To 20
                    'VALUNE1～20に値が存在する分、処理する
                    Dim ValueStr As String = "VALUE" & i
                    If String.IsNullOrEmpty(ChkRow(ValueStr)) Then
                        Exit For
                    End If
                    If String.IsNullOrEmpty(DataRow(ChkRow(ValueStr))) Then
                        'チェック項目がNGの場合、実績不備テーブルへの出力指示
                        DataRow("OUTTBL") = CONST_LNT0028
                    End If
                Next
            Next
        Next

    End Sub

    ''' <summary>
    ''' 実績テーブル更新
    ''' </summary>
    Private Sub ZissekiUpdate(ByVal iOrg As String, ByVal iTori As String, ByVal iTbl As DataTable, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim SaveTori As String = Nothing
        Dim SaveToriName As String = Nothing
        Dim SaveOrg As String = Nothing
        Dim SaveOrgName As String = Nothing
        Dim SaveRecordNo As String = Nothing
        Dim WW_DateNow As DateTime = Date.Now
        Dim repTori As String = "('" & iTori.Replace(",", "','") & "')"

            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(実績テーブル)
            Dim SQLStr As String = ""
            Dim TblNames As String() = {CONST_LNT0001, CONST_LNT0028}
            For Each TblName In TblNames
                '一旦すべて削除
                SQLStr =
                    String.Format(" UPDATE {0} ", TblName) _
                & " SET                                                             " _
                & "     DELFLG      = @DELFLG                                       " _
                & "   , UPDYMD      = @UPDYMD                                       " _
                & "   , UPDUSER     = @UPDUSER                                      " _
                & "   , UPDTERMID   = @UPDTERMID                                    " _
                & "   , UPDPGID     = @UPDPGID                                      " _
                & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
                & " WHERE                                                           " _
                & "     ORDERORG = @ORDERORG                                        " _
                & " AND TORICODE in " & repTori _
                & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
                & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                        ' DB更新用パラメータ(ユーザーパスワードマスタ)
                        Dim ORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORG", MySqlDbType.VarChar)                '営業所コード
                        Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                        Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                        Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                        Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                        Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                        Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                        Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                        Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                        Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                        ' DB更新

                        ' DB更新
                        ORDERORG.Value = iOrg                                                   '営業所コード
                        TORICODE.Value = iTori                                                  '取引先コード
                        DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                        If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                            YMDFROM.Value = WF_TaishoYm.Value & "/01"
                            YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                        End If
                        UPDYMD.Value = WW_DateNow                                               '更新年月日
                        UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                        UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                        UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                        RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = String.Format("DB:{0} UPDATE(DELETE)", TblName)
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                    oResult = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try

                '○ DB更新SQL(ユーザーマスタ)
                SQLStr =
                  String.Format("     INSERT INTO {0}     ", TblName) _
                & "      (RECONO						            " _
                & "     , LOADUNLOTYPE						        " _
                & "     , STACKINGTYPE						        " _
                & "     , HSETID						            " _
                & "     , ORDERORGSELECT						    " _
                & "     , ORDERORGNAME						        " _
                & "     , ORDERORGCODE						        " _
                & "     , ORDERORGNAMES						        " _
                & "     , KASANAMEORDERORG						    " _
                & "     , KASANCODEORDERORG						    " _
                & "     , KASANAMESORDERORG						    " _
                & "     , ORDERORG						            " _
                & "     , KASANORDERORG						        " _
                & "     , PRODUCTSLCT						        " _
                & "     , PRODUCTSYOSAI						        " _
                & "     , PRODUCT2NAME						        " _
                & "     , PRODUCT2						            " _
                & "     , PRODUCT1NAME						        " _
                & "     , PRODUCT1						            " _
                & "     , OILNAME						            " _
                & "     , OILTYPE						            " _
                & "     , TODOKESLCT						        " _
                & "     , TODOKECODE						        " _
                & "     , TODOKENAME						        " _
                & "     , TODOKENAMES						        " _
                & "     , TORICODE						            " _
                & "     , TORINAME						            " _
                & "     , TORICODE_AVOCADO  			            " _
                & "     , TODOKECONTNAME  			                " _
                & "     , TODOKEADDR						        " _
                & "     , TODOKETEL						            " _
                & "     , TODOKEMAP						            " _
                & "     , TODOKEIDO						            " _
                & "     , TODOKEKEIDO						        " _
                & "     , TODOKEBIKO1						        " _
                & "     , TODOKEBIKO2						        " _
                & "     , TODOKEBIKO3						        " _
                & "     , SHUKASLCT						            " _
                & "     , SHUKABASHO						        " _
                & "     , SHUKANAME						            " _
                & "     , SHUKANAMES						        " _
                & "     , SHUKATORICODE						        " _
                & "     , SHUKATORINAME						        " _
                & "     , SHUKACONTNAME						        " _
                & "     , SHUKAADDR						            " _
                & "     , SHUKAADDRTEL						        " _
                & "     , SHUKAMAP						            " _
                & "     , SHUKAIDO						            " _
                & "     , SHUKAKEIDO						        " _
                & "     , SHUKABIKOU1						        " _
                & "     , SHUKABIKOU2						        " _
                & "     , SHUKABIKOU3						        " _
                & "     , REQUIREDTIME						        " _
                & "     , SHUKADATE						            " _
                & "     , LOADTIME						            " _
                & "     , LOADTIMEIN						        " _
                & "     , LOADTIMES						            " _
                & "     , TODOKEDATE						        " _
                & "     , SHITEITIME						        " _
                & "     , SHITEITIMEIN						        " _
                & "     , SHITEITIMES						        " _
                & "     , ZYUTYU						            " _
                & "     , ZISSEKI						            " _
                & "     , TANNI						                " _
                & "     , GYOUMUSIZI1						        " _
                & "     , GYOUMUSIZI2						        " _
                & "     , GYOUMUSIZI3						        " _
                & "     , NINUSHIBIKOU						        " _
                & "     , MAXCAPACITY						        " _
                & "     , GYOMUSYABAN						        " _
                & "     , SHIPORGNAME						        " _
                & "     , SHIPORG						            " _
                & "     , SHIPORGNAMES						        " _
                & "     , KASANSHIPORGNAME						    " _
                & "     , KASANSHIPORG						        " _
                & "     , KASANSHIPORGNAMES						    " _
                & "     , TANKNUM						            " _
                & "     , TANKNUMBER						        " _
                & "     , SYAGATA						            " _
                & "     , SYABARA						            " _
                & "     , NINUSHINAME						        " _
                & "     , CONTYPE						            " _
                & "     , PRO1SYARYOU						        " _
                & "     , TANKMEMO						            " _
                & "     , TANKBIKOU1						        " _
                & "     , TANKBIKOU2						        " _
                & "     , TANKBIKOU3						        " _
                & "     , TRACTORNUM						        " _
                & "     , TRACTORNUMBER						        " _
                & "     , TRIP						                " _
                & "     , DRP						                " _
                & "     , ROTATION					                " _
                & "     , UNKOUMEMO						            " _
                & "     , SHUKKINTIME						        " _
                & "     , STAFFSLCT						            " _
                & "     , STAFFNAME						            " _
                & "     , STAFFCODE						            " _
                & "     , SUBSTAFFSLCT						        " _
                & "     , SUBSTAFFNAME						        " _
                & "     , SUBSTAFFNUM						        " _
                & "     , GYOMUTANKNUM						        " _
                & "     , YOUSYA						            " _
                & "     , RECOTITLE						            " _
                & "     , SHUKODATE						            " _
                & "     , KIKODATE						            " _
                & "     , KIKOTIME						            " _
                & "     , DISTANCE						            " _
                & "     , CREWBIKOU1						        " _
                & "     , CREWBIKOU2						        " _
                & "     , CREWSEQ						            " _
                & "     , SUBCREWBIKOU1						        " _
                & "     , SUBCREWBIKOU2						        " _
                & "     , SUBCREWSEQ						        " _
                & "     , SUBNGSYAGATA						        " _
                & "     , SUBSHUKKINTIME						    " _
                & "     , SYABARATANNI						        " _
                & "     , TAIKINTIME						        " _
                & "     , MARUYO						            " _
                & "     , SUBTIKINTIME						        " _
                & "     , SUBMARUYO						            " _
                & "     , KVTITLE						            " _
                & "     , KVTITLETODOKE 				            " _
                & "     , KVZYUTYU						            " _
                & "     , KVZISSEKI						            " _
                & "     , KVCREW						            " _
                & "     , CREWCODE						            " _
                & "     , SUBCREWCODE						        " _
                & "     , KVSUBCREW						            " _
                & "     , ORDERHENKO						        " _
                & "     , RIKUUNKYOKU						        " _
                & "     , BUNRUINUMBER						        " _
                & "     , HIRAGANA						            " _
                & "     , ITIRENNUM						            " _
                & "     , TRACTER1						            " _
                & "     , TRACTER2						            " _
                & "     , TRACTER3						            " _
                & "     , TRACTER4						            " _
                & "     , TRACTER5						            " _
                & "     , TRACTER6						            " _
                & "     , TRACTER7						            " _
                & "     , HAISYAHUKA						        " _
                & "     , HYOZIZYUNT						        " _
                & "     , HYOZIZYUNH						        " _
                & "     , HONTRACTER1						        " _
                & "     , HONTRACTER2						        " _
                & "     , HONTRACTER3						        " _
                & "     , HONTRACTER4						        " _
                & "     , HONTRACTER5						        " _
                & "     , HONTRACTER6						        " _
                & "     , HONTRACTER7						        " _
                & "     , HONTRACTER8						        " _
                & "     , HONTRACTER9						        " _
                & "     , HONTRACTER10						        " _
                & "     , HONTRACTER11						        " _
                & "     , HONTRACTER12						        " _
                & "     , HONTRACTER13						        " _
                & "     , HONTRACTER14						        " _
                & "     , HONTRACTER15						        " _
                & "     , HONTRACTER16						        " _
                & "     , HONTRACTER17						        " _
                & "     , HONTRACTER18						        " _
                & "     , HONTRACTER19						        " _
                & "     , HONTRACTER20						        " _
                & "     , HONTRACTER21						        " _
                & "     , HONTRACTER22						        " _
                & "     , HONTRACTER23						        " _
                & "     , HONTRACTER24						        " _
                & "     , HONTRACTER25						        " _
                & "     , ORDSTDATE						            " _
                & "     , ORDENDATE						            " _
                & "     , LUPDKEY						            " _
                & "     , HUPDKEY						            " _
                & "     , JXORDUPDKEY						        " _
                & "     , JXORDFILE						            " _
                & "     , JXORDROUTE						        " _
                & "     , JXORDTODOKENAME						    " _
                & "     , NIPPONO						            " _
                & "     , BRANCHCODE						        " _
                & "     , UPDATEUSER						        " _
                & "     , CREATEUSER						        " _
                & "     , UPDATEYMD						            " _
                & "     , CREATEYMD						            " _
                & "     , DELFLG						            " _
                & "     , INITYMD						            " _
                & "     , INITUSER						            " _
                & "     , INITTERMID						        " _
                & "     , INITPGID						            " _
                & "     , UPDYMD						            " _
                & "     , UPDUSER						            " _
                & "     , UPDTERMID						            " _
                & "     , UPDPGID						            " _
                & "     , RECEIVEYMD)						        " _
                & "     VALUES                                      " _
                & "      (@RECONO						            " _
                & "     , @LOADUNLOTYPE						        " _
                & "     , @STACKINGTYPE						        " _
                & "     , @HSETID						            " _
                & "     , @ORDERORGSELECT						    " _
                & "     , @ORDERORGNAME						        " _
                & "     , @ORDERORGCODE						        " _
                & "     , @ORDERORGNAMES						    " _
                & "     , @KASANAMEORDERORG						    " _
                & "     , @KASANCODEORDERORG						" _
                & "     , @KASANAMESORDERORG						" _
                & "     , @ORDERORG						            " _
                & "     , @KASANORDERORG						    " _
                & "     , @PRODUCTSLCT						        " _
                & "     , @PRODUCTSYOSAI						    " _
                & "     , @PRODUCT2NAME						        " _
                & "     , @PRODUCT2						            " _
                & "     , @PRODUCT1NAME						        " _
                & "     , @PRODUCT1						            " _
                & "     , @OILNAME						            " _
                & "     , @OILTYPE						            " _
                & "     , @TODOKESLCT						        " _
                & "     , @TODOKECODE						        " _
                & "     , @TODOKENAME						        " _
                & "     , @TODOKENAMES						        " _
                & "     , @TORICODE						            " _
                & "     , @TORINAME						            " _
                & "     , @TORICODE_AVOCADO				            " _
                & "     , @TODOKECONTNAME  			                " _
                & "     , @TODOKEADDR						        " _
                & "     , @TODOKETEL						        " _
                & "     , @TODOKEMAP						        " _
                & "     , @TODOKEIDO						        " _
                & "     , @TODOKEKEIDO						        " _
                & "     , @TODOKEBIKO1						        " _
                & "     , @TODOKEBIKO2						        " _
                & "     , @TODOKEBIKO3						        " _
                & "     , @SHUKASLCT						        " _
                & "     , @SHUKABASHO						        " _
                & "     , @SHUKANAME						        " _
                & "     , @SHUKANAMES						        " _
                & "     , @SHUKATORICODE						    " _
                & "     , @SHUKATORINAME						    " _
                & "     , @SHUKACONTNAME						    " _
                & "     , @SHUKAADDR						        " _
                & "     , @SHUKAADDRTEL						        " _
                & "     , @SHUKAMAP						            " _
                & "     , @SHUKAIDO						            " _
                & "     , @SHUKAKEIDO						        " _
                & "     , @SHUKABIKOU1						        " _
                & "     , @SHUKABIKOU2						        " _
                & "     , @SHUKABIKOU3						        " _
                & "     , @REQUIREDTIME						        " _
                & "     , @SHUKADATE						        " _
                & "     , @LOADTIME						            " _
                & "     , @LOADTIMEIN						        " _
                & "     , @LOADTIMES						        " _
                & "     , @TODOKEDATE						        " _
                & "     , @SHITEITIME						        " _
                & "     , @SHITEITIMEIN						        " _
                & "     , @SHITEITIMES						        " _
                & "     , @ZYUTYU						            " _
                & "     , @ZISSEKI						            " _
                & "     , @TANNI						            " _
                & "     , @GYOUMUSIZI1						        " _
                & "     , @GYOUMUSIZI2						        " _
                & "     , @GYOUMUSIZI3						        " _
                & "     , @NINUSHIBIKOU						        " _
                & "     , @MAXCAPACITY						        " _
                & "     , @GYOMUSYABAN						        " _
                & "     , @SHIPORGNAME						        " _
                & "     , @SHIPORG						            " _
                & "     , @SHIPORGNAMES						        " _
                & "     , @KASANSHIPORGNAME						    " _
                & "     , @KASANSHIPORG						        " _
                & "     , @KASANSHIPORGNAMES						" _
                & "     , @TANKNUM						            " _
                & "     , @TANKNUMBER						        " _
                & "     , @SYAGATA						            " _
                & "     , @SYABARA						            " _
                & "     , @NINUSHINAME						        " _
                & "     , @CONTYPE						            " _
                & "     , @PRO1SYARYOU						        " _
                & "     , @TANKMEMO						            " _
                & "     , @TANKBIKOU1						        " _
                & "     , @TANKBIKOU2						        " _
                & "     , @TANKBIKOU3						        " _
                & "     , @TRACTORNUM						        " _
                & "     , @TRACTORNUMBER						    " _
                & "     , @TRIP						                " _
                & "     , @DRP						                " _
                & "     , @ROTATION						            " _
                & "     , @UNKOUMEMO						        " _
                & "     , @SHUKKINTIME						        " _
                & "     , @STAFFSLCT						        " _
                & "     , @STAFFNAME						        " _
                & "     , @STAFFCODE						        " _
                & "     , @SUBSTAFFSLCT						        " _
                & "     , @SUBSTAFFNAME						        " _
                & "     , @SUBSTAFFNUM						        " _
                & "     , @GYOMUTANKNUM						        " _
                & "     , @YOUSYA						            " _
                & "     , @RECOTITLE						        " _
                & "     , @SHUKODATE						        " _
                & "     , @KIKODATE						            " _
                & "     , @KIKOTIME						            " _
                & "     , @DISTANCE						            " _
                & "     , @CREWBIKOU1						        " _
                & "     , @CREWBIKOU2						        " _
                & "     , @CREWSEQ						            " _
                & "     , @SUBCREWBIKOU1						    " _
                & "     , @SUBCREWBIKOU2						    " _
                & "     , @SUBCREWSEQ						        " _
                & "     , @SUBNGSYAGATA						        " _
                & "     , @SUBSHUKKINTIME						    " _
                & "     , @SYABARATANNI						        " _
                & "     , @TAIKINTIME						        " _
                & "     , @MARUYO						            " _
                & "     , @SUBTIKINTIME						        " _
                & "     , @SUBMARUYO						        " _
                & "     , @KVTITLE						            " _
                & "     , @KVTITLETODOKE						    " _
                & "     , @KVZYUTYU						            " _
                & "     , @KVZISSEKI						        " _
                & "     , @KVCREW						            " _
                & "     , @CREWCODE						            " _
                & "     , @SUBCREWCODE						        " _
                & "     , @KVSUBCREW						        " _
                & "     , @ORDERHENKO						        " _
                & "     , @RIKUUNKYOKU						        " _
                & "     , @BUNRUINUMBER						        " _
                & "     , @HIRAGANA						            " _
                & "     , @ITIRENNUM						        " _
                & "     , @TRACTER1						            " _
                & "     , @TRACTER2						            " _
                & "     , @TRACTER3						            " _
                & "     , @TRACTER4						            " _
                & "     , @TRACTER5						            " _
                & "     , @TRACTER6						            " _
                & "     , @TRACTER7						            " _
                & "     , @HAISYAHUKA						        " _
                & "     , @HYOZIZYUNT						        " _
                & "     , @HYOZIZYUNH						        " _
                & "     , @HONTRACTER1						        " _
                & "     , @HONTRACTER2						        " _
                & "     , @HONTRACTER3						        " _
                & "     , @HONTRACTER4						        " _
                & "     , @HONTRACTER5						        " _
                & "     , @HONTRACTER6						        " _
                & "     , @HONTRACTER7						        " _
                & "     , @HONTRACTER8						        " _
                & "     , @HONTRACTER9						        " _
                & "     , @HONTRACTER10						        " _
                & "     , @HONTRACTER11						        " _
                & "     , @HONTRACTER12						        " _
                & "     , @HONTRACTER13						        " _
                & "     , @HONTRACTER14						        " _
                & "     , @HONTRACTER15						        " _
                & "     , @HONTRACTER16						        " _
                & "     , @HONTRACTER17						        " _
                & "     , @HONTRACTER18						        " _
                & "     , @HONTRACTER19						        " _
                & "     , @HONTRACTER20						        " _
                & "     , @HONTRACTER21						        " _
                & "     , @HONTRACTER22						        " _
                & "     , @HONTRACTER23						        " _
                & "     , @HONTRACTER24						        " _
                & "     , @HONTRACTER25						        " _
                & "     , @ORDSTDATE						        " _
                & "     , @ORDENDATE						        " _
                & "     , @LUPDKEY						            " _
                & "     , @HUPDKEY						            " _
                & "     , @JXORDUPDKEY						        " _
                & "     , @JXORDFILE						        " _
                & "     , @JXORDROUTE						        " _
                & "     , @JXORDTODOKENAME					        " _
                & "     , @NIPPONO					                " _
                & "     , @BRANCHCODE						        " _
                & "     , @UPDATEUSER						        " _
                & "     , @CREATEUSER						        " _
                & "     , @UPDATEYMD						        " _
                & "     , @CREATEYMD						        " _
                & "     , @DELFLG						            " _
                & "     , @INITYMD						            " _
                & "     , @INITUSER						            " _
                & "     , @INITTERMID						        " _
                & "     , @INITPGID						            " _
                & "     , @UPDYMD						            " _
                & "     , @UPDUSER						            " _
                & "     , @UPDTERMID						        " _
                & "     , @UPDPGID						            " _
                & "     , @RECEIVEYMD)						        " _
                & "     ON DUPLICATE KEY UPDATE                     " _
                & "       RECONO = @RECONO						    " _
                & "     , LOADUNLOTYPE = @LOADUNLOTYPE				" _
                & "     , STACKINGTYPE = @STACKINGTYPE				" _
                & "     , HSETID = @HSETID						    " _
                & "     , ORDERORGSELECT = @ORDERORGSELECT			" _
                & "     , ORDERORGNAME = @ORDERORGNAME				" _
                & "     , ORDERORGCODE = @ORDERORGCODE				" _
                & "     , ORDERORGNAMES = @ORDERORGNAMES			" _
                & "     , KASANAMEORDERORG = @KASANAMEORDERORG		" _
                & "     , KASANCODEORDERORG = @KASANCODEORDERORG	" _
                & "     , KASANAMESORDERORG = @KASANAMESORDERORG	" _
                & "     , ORDERORG = @ORDERORG						" _
                & "     , KASANORDERORG = @KASANORDERORG			" _
                & "     , PRODUCTSLCT = @PRODUCTSLCT				" _
                & "     , PRODUCTSYOSAI = @PRODUCTSYOSAI			" _
                & "     , PRODUCT2NAME = @PRODUCT2NAME				" _
                & "     , PRODUCT2 = @PRODUCT2						" _
                & "     , PRODUCT1NAME = @PRODUCT1NAME				" _
                & "     , PRODUCT1 = @PRODUCT1						" _
                & "     , OILNAME = @OILNAME						" _
                & "     , OILTYPE = @OILTYPE						" _
                & "     , TODOKESLCT = @TODOKESLCT					" _
                & "     , TODOKECODE = @TODOKECODE					" _
                & "     , TODOKENAME = @TODOKENAME					" _
                & "     , TODOKENAMES = @TODOKENAMES				" _
                & "     , TORICODE = @TORICODE						" _
                & "     , TORINAME = @TORINAME						" _
                & "     , TORICODE_AVOCADO = @TORICODE_AVOCADO		" _
                & "     , TODOKECONTNAME = @TODOKECONTNAME		    " _
                & "     , TODOKEADDR = @TODOKEADDR					" _
                & "     , TODOKETEL = @TODOKETEL					" _
                & "     , TODOKEMAP = @TODOKEMAP					" _
                & "     , TODOKEIDO = @TODOKEIDO					" _
                & "     , TODOKEKEIDO = @TODOKEKEIDO				" _
                & "     , TODOKEBIKO1 = @TODOKEBIKO1				" _
                & "     , TODOKEBIKO2 = @TODOKEBIKO2				" _
                & "     , TODOKEBIKO3 = @TODOKEBIKO3				" _
                & "     , SHUKASLCT = @SHUKASLCT					" _
                & "     , SHUKABASHO = @SHUKABASHO					" _
                & "     , SHUKANAME = @SHUKANAME					" _
                & "     , SHUKANAMES = @SHUKANAMES					" _
                & "     , SHUKATORICODE = @SHUKATORICODE			" _
                & "     , SHUKATORINAME = @SHUKATORINAME			" _
                & "     , SHUKACONTNAME = @SHUKACONTNAME			" _
                & "     , SHUKAADDR = @SHUKAADDR					" _
                & "     , SHUKAADDRTEL = @SHUKAADDRTEL				" _
                & "     , SHUKAMAP = @SHUKAMAP						" _
                & "     , SHUKAIDO = @SHUKAIDO						" _
                & "     , SHUKAKEIDO = @SHUKAKEIDO					" _
                & "     , SHUKABIKOU1 = @SHUKABIKOU1				" _
                & "     , SHUKABIKOU2 = @SHUKABIKOU2				" _
                & "     , SHUKABIKOU3 = @SHUKABIKOU3				" _
                & "     , REQUIREDTIME = @REQUIREDTIME				" _
                & "     , SHUKADATE = @SHUKADATE					" _
                & "     , LOADTIME = @LOADTIME						" _
                & "     , LOADTIMEIN = @LOADTIMEIN					" _
                & "     , LOADTIMES = @LOADTIMES					" _
                & "     , TODOKEDATE = @TODOKEDATE					" _
                & "     , SHITEITIME = @SHITEITIME					" _
                & "     , SHITEITIMEIN = @SHITEITIMEIN				" _
                & "     , SHITEITIMES = @SHITEITIMES				" _
                & "     , ZYUTYU = @ZYUTYU						    " _
                & "     , ZISSEKI = @ZISSEKI						" _
                & "     , TANNI = @TANNI						    " _
                & "     , GYOUMUSIZI1 = @GYOUMUSIZI1				" _
                & "     , GYOUMUSIZI2 = @GYOUMUSIZI2				" _
                & "     , GYOUMUSIZI3 = @GYOUMUSIZI3				" _
                & "     , NINUSHIBIKOU = @NINUSHIBIKOU				" _
                & "     , MAXCAPACITY = @MAXCAPACITY				" _
                & "     , GYOMUSYABAN = @GYOMUSYABAN				" _
                & "     , SHIPORGNAME = @SHIPORGNAME				" _
                & "     , SHIPORG = @SHIPORG						" _
                & "     , SHIPORGNAMES = @SHIPORGNAMES				" _
                & "     , KASANSHIPORGNAME = @KASANSHIPORGNAME		" _
                & "     , KASANSHIPORG = @KASANSHIPORG				" _
                & "     , KASANSHIPORGNAMES = @KASANSHIPORGNAMES	" _
                & "     , TANKNUM = @TANKNUM						" _
                & "     , TANKNUMBER = @TANKNUMBER					" _
                & "     , SYAGATA = @SYAGATA						" _
                & "     , SYABARA = @SYABARA						" _
                & "     , NINUSHINAME = @NINUSHINAME				" _
                & "     , CONTYPE = @CONTYPE						" _
                & "     , PRO1SYARYOU = @PRO1SYARYOU				" _
                & "     , TANKMEMO = @TANKMEMO						" _
                & "     , TANKBIKOU1 = @TANKBIKOU1					" _
                & "     , TANKBIKOU2 = @TANKBIKOU2					" _
                & "     , TANKBIKOU3 = @TANKBIKOU3					" _
                & "     , TRACTORNUM = @TRACTORNUM					" _
                & "     , TRACTORNUMBER = @TRACTORNUMBER			" _
                & "     , TRIP = @TRIP						        " _
                & "     , DRP = @DRP						        " _
                & "     , ROTATION = @ROTATION						" _
                & "     , UNKOUMEMO = @UNKOUMEMO					" _
                & "     , SHUKKINTIME = @SHUKKINTIME				" _
                & "     , STAFFSLCT = @STAFFSLCT					" _
                & "     , STAFFNAME = @STAFFNAME					" _
                & "     , STAFFCODE = @STAFFCODE					" _
                & "     , SUBSTAFFSLCT = @SUBSTAFFSLCT				" _
                & "     , SUBSTAFFNAME = @SUBSTAFFNAME				" _
                & "     , SUBSTAFFNUM = @SUBSTAFFNUM				" _
                & "     , GYOMUTANKNUM = @GYOMUTANKNUM				" _
                & "     , YOUSYA = @YOUSYA						    " _
                & "     , RECOTITLE = @RECOTITLE					" _
                & "     , SHUKODATE = @SHUKODATE					" _
                & "     , KIKODATE = @KIKODATE						" _
                & "     , KIKOTIME = @KIKOTIME						" _
                & "     , DISTANCE = @DISTANCE						" _
                & "     , CREWBIKOU1 = @CREWBIKOU1					" _
                & "     , CREWBIKOU2 = @CREWBIKOU2					" _
                & "     , CREWSEQ = @CREWSEQ					    " _
                & "     , SUBCREWBIKOU1 = @SUBCREWBIKOU1			" _
                & "     , SUBCREWBIKOU2 = @SUBCREWBIKOU2			" _
                & "     , SUBCREWSEQ = @SUBCREWSEQ					" _
                & "     , SUBNGSYAGATA = @SUBNGSYAGATA				" _
                & "     , SUBSHUKKINTIME = @SUBSHUKKINTIME			" _
                & "     , SYABARATANNI = @SYABARATANNI				" _
                & "     , TAIKINTIME = @TAIKINTIME					" _
                & "     , MARUYO = @MARUYO					        " _
                & "     , SUBTIKINTIME = @SUBTIKINTIME				" _
                & "     , SUBMARUYO = @SUBMARUYO				    " _
                & "     , KVTITLE = @KVTITLE						" _
                & "     , KVTITLETODOKE = @KVTITLETODOKE			" _
                & "     , KVZYUTYU = @KVZYUTYU						" _
                & "     , KVZISSEKI = @KVZISSEKI					" _
                & "     , KVCREW = @KVCREW						    " _
                & "     , CREWCODE = @CREWCODE						" _
                & "     , SUBCREWCODE = @SUBCREWCODE				" _
                & "     , KVSUBCREW = @KVSUBCREW					" _
                & "     , ORDERHENKO = @ORDERHENKO					" _
                & "     , RIKUUNKYOKU = @RIKUUNKYOKU				" _
                & "     , BUNRUINUMBER = @BUNRUINUMBER				" _
                & "     , HIRAGANA = @HIRAGANA						" _
                & "     , ITIRENNUM = @ITIRENNUM					" _
                & "     , TRACTER1 = @TRACTER1						" _
                & "     , TRACTER2 = @TRACTER2						" _
                & "     , TRACTER3 = @TRACTER3						" _
                & "     , TRACTER4 = @TRACTER4						" _
                & "     , TRACTER5 = @TRACTER5						" _
                & "     , TRACTER6 = @TRACTER6						" _
                & "     , TRACTER7 = @TRACTER7						" _
                & "     , HAISYAHUKA = @HAISYAHUKA					" _
                & "     , HYOZIZYUNT = @HYOZIZYUNT					" _
                & "     , HYOZIZYUNH = @HYOZIZYUNH					" _
                & "     , HONTRACTER1 = @HONTRACTER1				" _
                & "     , HONTRACTER2 = @HONTRACTER2				" _
                & "     , HONTRACTER3 = @HONTRACTER3				" _
                & "     , HONTRACTER4 = @HONTRACTER4				" _
                & "     , HONTRACTER5 = @HONTRACTER5				" _
                & "     , HONTRACTER6 = @HONTRACTER6				" _
                & "     , HONTRACTER7 = @HONTRACTER7				" _
                & "     , HONTRACTER8 = @HONTRACTER8				" _
                & "     , HONTRACTER9 = @HONTRACTER9				" _
                & "     , HONTRACTER10 = @HONTRACTER10				" _
                & "     , HONTRACTER11 = @HONTRACTER11				" _
                & "     , HONTRACTER12 = @HONTRACTER12				" _
                & "     , HONTRACTER13 = @HONTRACTER13				" _
                & "     , HONTRACTER14 = @HONTRACTER14				" _
                & "     , HONTRACTER15 = @HONTRACTER15				" _
                & "     , HONTRACTER16 = @HONTRACTER16				" _
                & "     , HONTRACTER17 = @HONTRACTER17				" _
                & "     , HONTRACTER18 = @HONTRACTER18				" _
                & "     , HONTRACTER19 = @HONTRACTER19				" _
                & "     , HONTRACTER20 = @HONTRACTER20				" _
                & "     , HONTRACTER21 = @HONTRACTER21				" _
                & "     , HONTRACTER22 = @HONTRACTER22				" _
                & "     , HONTRACTER23 = @HONTRACTER23				" _
                & "     , HONTRACTER24 = @HONTRACTER24				" _
                & "     , HONTRACTER25 = @HONTRACTER25				" _
                & "     , ORDSTDATE = @ORDSTDATE					" _
                & "     , ORDENDATE = @ORDENDATE					" _
                & "     , LUPDKEY = @LUPDKEY					    " _
                & "     , HUPDKEY = @HUPDKEY					    " _
                & "     , JXORDUPDKEY = @JXORDUPDKEY				" _
                & "     , JXORDFILE = @JXORDFILE				    " _
                & "     , JXORDROUTE = @JXORDROUTE				    " _
                & "     , JXORDTODOKENAME = @JXORDTODOKENAME	    " _
                & "     , NIPPONO = @NIPPONO	                    " _
                & "     , BRANCHCODE = CASE WHEN BRANCHCODE <> '1' THEN BRANCHCODE ELSE @BRANCHCODE END    " _
                & "     , UPDATEUSER = @UPDATEUSER					" _
                & "     , CREATEUSER = @CREATEUSER					" _
                & "     , UPDATEYMD = @UPDATEYMD					" _
                & "     , CREATEYMD = @CREATEYMD					" _
                & "     , DELFLG = @DELFLG						    " _
                & "     , UPDYMD = @UPDYMD						    " _
                & "     , UPDUSER = @UPDUSER						" _
                & "     , UPDTERMID = @UPDTERMID					" _
                & "     , UPDPGID = @UPDPGID						" _
                & "     , RECEIVEYMD = @RECEIVEYMD					"

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                        ' DB更新用パラメータ
                        Dim RECONO As MySqlParameter = SQLcmd.Parameters.Add("@RECONO", MySqlDbType.VarChar)    'レコード番号
                        Dim LOADUNLOTYPE As MySqlParameter = SQLcmd.Parameters.Add("@LOADUNLOTYPE", MySqlDbType.VarChar)    '積込荷卸区分
                        Dim STACKINGTYPE As MySqlParameter = SQLcmd.Parameters.Add("@STACKINGTYPE", MySqlDbType.VarChar)    '積置区分
                        Dim HSETID As MySqlParameter = SQLcmd.Parameters.Add("@HSETID", MySqlDbType.VarChar)    '配送セットID
                        Dim ORDERORGSELECT As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGSELECT", MySqlDbType.VarChar)    '受注受付部署選択
                        Dim ORDERORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGNAME", MySqlDbType.VarChar)    '受注受付部署名
                        Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)    '受注受付部署コード
                        Dim ORDERORGNAMES As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGNAMES", MySqlDbType.VarChar)  '受注受付部署略名
                        Dim KASANAMEORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@KASANAMEORDERORG", MySqlDbType.VarChar)    '加算先部署名_受注受付部署
                        Dim KASANCODEORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@KASANCODEORDERORG", MySqlDbType.VarChar)  '加算先部署コード_受注受付部署
                        Dim KASANAMESORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@KASANAMESORDERORG", MySqlDbType.VarChar)  '加算先部署略名_受注受付部署
                        Dim ORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORG", MySqlDbType.VarChar)    '受注受付部署
                        Dim KASANORDERORG As MySqlParameter = SQLcmd.Parameters.Add("@KASANORDERORG", MySqlDbType.VarChar)  '加算先受注受付部署
                        Dim PRODUCTSLCT As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCTSLCT", MySqlDbType.VarChar)  '品名選択
                        Dim PRODUCTSYOSAI As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCTSYOSAI", MySqlDbType.VarChar)  '品名詳細
                        Dim PRODUCT2NAME As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCT2NAME", MySqlDbType.VarChar)    '品名2名
                        Dim PRODUCT2 As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCT2", MySqlDbType.VarChar)    '品名2コード
                        Dim PRODUCT1NAME As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCT1NAME", MySqlDbType.VarChar)    '品名1名
                        Dim PRODUCT1 As MySqlParameter = SQLcmd.Parameters.Add("@PRODUCT1", MySqlDbType.VarChar)    '品名1コード
                        Dim OILNAME As MySqlParameter = SQLcmd.Parameters.Add("@OILNAME", MySqlDbType.VarChar)  '油種名
                        Dim OILTYPE As MySqlParameter = SQLcmd.Parameters.Add("@OILTYPE", MySqlDbType.VarChar)  '油種コード
                        Dim TODOKESLCT As MySqlParameter = SQLcmd.Parameters.Add("@TODOKESLCT", MySqlDbType.VarChar)    '届先選択
                        Dim TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar)    '届先コード
                        Dim TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar)    '届先名称
                        Dim TODOKENAMES As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAMES", MySqlDbType.VarChar)  '届先略名
                        Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)    '届先取引先コード
                        Dim TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar)    '届先取引先名称
                        Dim TORICODE_AVOCADO As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE_AVOCADO", MySqlDbType.VarChar)    '届先取引先コード_アボカド
                        Dim TODOKECONTNAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECONTNAME", MySqlDbType.VarChar)    '届先契約先名称
                        Dim TODOKEADDR As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEADDR", MySqlDbType.VarChar)    '届先住所
                        Dim TODOKETEL As MySqlParameter = SQLcmd.Parameters.Add("@TODOKETEL", MySqlDbType.VarChar)  '届先電話番号
                        Dim TODOKEMAP As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEMAP", MySqlDbType.VarChar)  '届先Googleマップ
                        Dim TODOKEIDO As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEIDO", MySqlDbType.VarChar)  '届先緯度
                        Dim TODOKEKEIDO As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEKEIDO", MySqlDbType.VarChar)  '届先経度
                        Dim TODOKEBIKO1 As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEBIKO1", MySqlDbType.VarChar)  '届先備考1
                        Dim TODOKEBIKO2 As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEBIKO2", MySqlDbType.VarChar)  '届先備考2
                        Dim TODOKEBIKO3 As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEBIKO3", MySqlDbType.VarChar)  '届先備考3
                        Dim SHUKASLCT As MySqlParameter = SQLcmd.Parameters.Add("@SHUKASLCT", MySqlDbType.VarChar)  '出荷場所選択
                        Dim SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar)    '出荷場所コード
                        Dim SHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAME", MySqlDbType.VarChar)  '出荷場所名称
                        Dim SHUKANAMES As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAMES", MySqlDbType.VarChar)    '出荷場所略名
                        Dim SHUKATORICODE As MySqlParameter = SQLcmd.Parameters.Add("@SHUKATORICODE", MySqlDbType.VarChar)  '出荷場所取引先コード
                        Dim SHUKATORINAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKATORINAME", MySqlDbType.VarChar)  '出荷場所取引先名称
                        Dim SHUKACONTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKACONTNAME", MySqlDbType.VarChar)  '出荷場所契約先名称
                        Dim SHUKAADDR As MySqlParameter = SQLcmd.Parameters.Add("@SHUKAADDR", MySqlDbType.VarChar)  '出荷場所住所
                        Dim SHUKAADDRTEL As MySqlParameter = SQLcmd.Parameters.Add("@SHUKAADDRTEL", MySqlDbType.VarChar)    '出荷場所電話番号
                        Dim SHUKAMAP As MySqlParameter = SQLcmd.Parameters.Add("@SHUKAMAP", MySqlDbType.VarChar)    '出荷場所Googleマップ
                        Dim SHUKAIDO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKAIDO", MySqlDbType.VarChar)    '出荷場所緯度
                        Dim SHUKAKEIDO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKAKEIDO", MySqlDbType.VarChar)    '出荷場所経度
                        Dim SHUKABIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABIKOU1", MySqlDbType.VarChar)  '出荷場所備考1
                        Dim SHUKABIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABIKOU2", MySqlDbType.VarChar)  '出荷場所備考2
                        Dim SHUKABIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABIKOU3", MySqlDbType.VarChar)  '出荷場所備考3
                        Dim REQUIREDTIME As MySqlParameter = SQLcmd.Parameters.Add("@REQUIREDTIME", MySqlDbType.VarChar)  '標準所要時間
                        Dim SHUKADATE As MySqlParameter = SQLcmd.Parameters.Add("@SHUKADATE", MySqlDbType.Date) '出荷日
                        Dim LOADTIME As MySqlParameter = SQLcmd.Parameters.Add("@LOADTIME", MySqlDbType.VarChar)   '積込時間
                        Dim LOADTIMEIN As MySqlParameter = SQLcmd.Parameters.Add("@LOADTIMEIN", MySqlDbType.VarChar)    '積込時間手入力
                        Dim LOADTIMES As MySqlParameter = SQLcmd.Parameters.Add("@LOADTIMES", MySqlDbType.VarChar)  '積込時間_画面表示用
                        Dim TODOKEDATE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKEDATE", MySqlDbType.Date)   '届日
                        Dim SHITEITIME As MySqlParameter = SQLcmd.Parameters.Add("@SHITEITIME", MySqlDbType.VarChar)   '指定時間
                        Dim SHITEITIMEIN As MySqlParameter = SQLcmd.Parameters.Add("@SHITEITIMEIN", MySqlDbType.VarChar)    '指定時間手入力
                        Dim SHITEITIMES As MySqlParameter = SQLcmd.Parameters.Add("@SHITEITIMES", MySqlDbType.VarChar)  '指定時間_画面表示用
                        Dim ZYUTYU As MySqlParameter = SQLcmd.Parameters.Add("@ZYUTYU", MySqlDbType.Decimal)    '受注数量
                        Dim ZISSEKI As MySqlParameter = SQLcmd.Parameters.Add("@ZISSEKI", MySqlDbType.Decimal)  '実績数量
                        Dim TANNI As MySqlParameter = SQLcmd.Parameters.Add("@TANNI", MySqlDbType.VarChar)  '数量単位
                        Dim GYOUMUSIZI1 As MySqlParameter = SQLcmd.Parameters.Add("@GYOUMUSIZI1", MySqlDbType.VarChar)  '業務指示1
                        Dim GYOUMUSIZI2 As MySqlParameter = SQLcmd.Parameters.Add("@GYOUMUSIZI2", MySqlDbType.VarChar)  '業務指示2
                        Dim GYOUMUSIZI3 As MySqlParameter = SQLcmd.Parameters.Add("@GYOUMUSIZI3", MySqlDbType.VarChar)  '業務指示3
                        Dim NINUSHIBIKOU As MySqlParameter = SQLcmd.Parameters.Add("@NINUSHIBIKOU", MySqlDbType.VarChar)    '荷主備考
                        Dim MAXCAPACITY As MySqlParameter = SQLcmd.Parameters.Add("@MAXCAPACITY", MySqlDbType.VarChar)    '最大積載量
                        Dim GYOMUSYABAN As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUSYABAN", MySqlDbType.VarChar)  '業務車番選択
                        Dim SHIPORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@SHIPORGNAME", MySqlDbType.VarChar)  '出荷部署名
                        Dim SHIPORG As MySqlParameter = SQLcmd.Parameters.Add("@SHIPORG", MySqlDbType.VarChar)  '出荷部署コード
                        Dim SHIPORGNAMES As MySqlParameter = SQLcmd.Parameters.Add("@SHIPORGNAMES", MySqlDbType.VarChar)    '出荷部署略名
                        Dim KASANSHIPORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANSHIPORGNAME", MySqlDbType.VarChar)    '加算先出荷部署名
                        Dim KASANSHIPORG As MySqlParameter = SQLcmd.Parameters.Add("@KASANSHIPORG", MySqlDbType.VarChar)    '加算先出荷部署コード
                        Dim KASANSHIPORGNAMES As MySqlParameter = SQLcmd.Parameters.Add("@KASANSHIPORGNAMES", MySqlDbType.VarChar)  '加算先出荷部署略名
                        Dim TANKNUM As MySqlParameter = SQLcmd.Parameters.Add("@TANKNUM", MySqlDbType.VarChar)  '統一車番
                        Dim TANKNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@TANKNUMBER", MySqlDbType.VarChar)    '陸事番号
                        Dim SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar)  '車型
                        Dim SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.VarChar)    '車腹
                        Dim NINUSHINAME As MySqlParameter = SQLcmd.Parameters.Add("@NINUSHINAME", MySqlDbType.VarChar)  '荷主名
                        Dim CONTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CONTYPE", MySqlDbType.VarChar)  '契約区分
                        Dim PRO1SYARYOU As MySqlParameter = SQLcmd.Parameters.Add("@PRO1SYARYOU", MySqlDbType.VarChar)  '品名1名_車両
                        Dim TANKMEMO As MySqlParameter = SQLcmd.Parameters.Add("@TANKMEMO", MySqlDbType.VarChar)    '車両メモ
                        Dim TANKBIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@TANKBIKOU1", MySqlDbType.VarChar)    '車両備考1
                        Dim TANKBIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@TANKBIKOU2", MySqlDbType.VarChar)    '車両備考2
                        Dim TANKBIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@TANKBIKOU3", MySqlDbType.VarChar)    '車両備考3
                        Dim TRACTORNUM As MySqlParameter = SQLcmd.Parameters.Add("@TRACTORNUM", MySqlDbType.VarChar)    '統一車番_トラクタ
                        Dim TRACTORNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@TRACTORNUMBER", MySqlDbType.VarChar)  '陸事番号_トラクタ
                        Dim TRIP As MySqlParameter = SQLcmd.Parameters.Add("@TRIP", MySqlDbType.Int16)  'トリップ
                        Dim DRP As MySqlParameter = SQLcmd.Parameters.Add("@DRP", MySqlDbType.Int16)    'ドロップ
                        Dim ROTATION As MySqlParameter = SQLcmd.Parameters.Add("@ROTATION", MySqlDbType.Int16)    '回転数
                        Dim UNKOUMEMO As MySqlParameter = SQLcmd.Parameters.Add("@UNKOUMEMO", MySqlDbType.VarChar)  '当日前後運行メモ
                        Dim SHUKKINTIME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKKINTIME", MySqlDbType.VarChar) '出勤時間
                        Dim STAFFSLCT As MySqlParameter = SQLcmd.Parameters.Add("@STAFFSLCT", MySqlDbType.VarChar)  '乗務員選択
                        Dim STAFFNAME As MySqlParameter = SQLcmd.Parameters.Add("@STAFFNAME", MySqlDbType.VarChar)  '氏名_乗務員
                        Dim STAFFCODE As MySqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", MySqlDbType.VarChar)  '社員番号_乗務員
                        Dim SUBSTAFFSLCT As MySqlParameter = SQLcmd.Parameters.Add("@SUBSTAFFSLCT", MySqlDbType.VarChar)    '副乗務員選択
                        Dim SUBSTAFFNAME As MySqlParameter = SQLcmd.Parameters.Add("@SUBSTAFFNAME", MySqlDbType.VarChar)    '氏名_副乗務員
                        Dim SUBSTAFFNUM As MySqlParameter = SQLcmd.Parameters.Add("@SUBSTAFFNUM", MySqlDbType.VarChar)  '社員番号_副乗務員
                        Dim GYOMUTANKNUM As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUM", MySqlDbType.VarChar)    '業務車番
                        Dim YOUSYA As MySqlParameter = SQLcmd.Parameters.Add("@YOUSYA", MySqlDbType.VarChar)    '用車先
                        Dim RECOTITLE As MySqlParameter = SQLcmd.Parameters.Add("@RECOTITLE", MySqlDbType.VarChar)  'レコードタイトル用
                        Dim SHUKODATE As MySqlParameter = SQLcmd.Parameters.Add("@SHUKODATE", MySqlDbType.Date)   '出庫日
                        Dim KIKODATE As MySqlParameter = SQLcmd.Parameters.Add("@KIKODATE", MySqlDbType.Date)   '帰庫日
                        Dim KIKOTIME As MySqlParameter = SQLcmd.Parameters.Add("@KIKOTIME", MySqlDbType.VarChar)   '帰庫時間
                        Dim DISTANCE As MySqlParameter = SQLcmd.Parameters.Add("@DISTANCE", MySqlDbType.Decimal)   '距離
                        Dim CREWBIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@CREWBIKOU1", MySqlDbType.VarChar)    '乗務員備考1
                        Dim CREWBIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@CREWBIKOU2", MySqlDbType.VarChar)    '乗務員備考2
                        Dim CREWSEQ As MySqlParameter = SQLcmd.Parameters.Add("@CREWSEQ", MySqlDbType.VarChar)    '表示順_乗務員
                        Dim SUBCREWBIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@SUBCREWBIKOU1", MySqlDbType.VarChar)  '副乗務員備考1
                        Dim SUBCREWBIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@SUBCREWBIKOU2", MySqlDbType.VarChar)  '副乗務員備考2
                        Dim SUBCREWSEQ As MySqlParameter = SQLcmd.Parameters.Add("@SUBCREWSEQ", MySqlDbType.VarChar)    '表示順_副乗務員
                        Dim SUBNGSYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SUBNGSYAGATA", MySqlDbType.VarChar)    '表示順_副乗務員
                        Dim SUBSHUKKINTIME As MySqlParameter = SQLcmd.Parameters.Add("@SUBSHUKKINTIME", MySqlDbType.VarChar)   '乗務不可車型_副乗務員
                        Dim SYABARATANNI As MySqlParameter = SQLcmd.Parameters.Add("@SYABARATANNI", MySqlDbType.VarChar)    '車腹単位
                        Dim TAIKINTIME As MySqlParameter = SQLcmd.Parameters.Add("@TAIKINTIME", MySqlDbType.VarChar)   '退勤見込み時間
                        Dim MARUYO As MySqlParameter = SQLcmd.Parameters.Add("@MARUYO", MySqlDbType.Decimal)   'マルヨ
                        Dim SUBTIKINTIME As MySqlParameter = SQLcmd.Parameters.Add("@SUBTIKINTIME", MySqlDbType.VarChar)   '退勤見込み時間_副乗務員
                        Dim SUBMARUYO As MySqlParameter = SQLcmd.Parameters.Add("@SUBMARUYO", MySqlDbType.Decimal)   'マルヨ_副乗務員
                        Dim KVTITLE As MySqlParameter = SQLcmd.Parameters.Add("@KVTITLE", MySqlDbType.VarChar)  'kViewer用タイトル_届先名なし
                        Dim KVTITLETODOKE As MySqlParameter = SQLcmd.Parameters.Add("@KVTITLETODOKE", MySqlDbType.VarChar)  'kViewer用タイトル_届先名あり
                        Dim KVZYUTYU As MySqlParameter = SQLcmd.Parameters.Add("@KVZYUTYU", MySqlDbType.VarChar)    'kViewer用受注数量
                        Dim KVZISSEKI As MySqlParameter = SQLcmd.Parameters.Add("@KVZISSEKI", MySqlDbType.VarChar)  'kViewer用実績数量
                        Dim KVCREW As MySqlParameter = SQLcmd.Parameters.Add("@KVCREW", MySqlDbType.VarChar)    'kViewer用乗務員情報
                        Dim CREWCODE As MySqlParameter = SQLcmd.Parameters.Add("@CREWCODE", MySqlDbType.VarChar)    '乗務員コード_乗務員
                        Dim SUBCREWCODE As MySqlParameter = SQLcmd.Parameters.Add("@SUBCREWCODE", MySqlDbType.VarChar)  '乗務員コード_副乗務員
                        Dim KVSUBCREW As MySqlParameter = SQLcmd.Parameters.Add("@KVSUBCREW", MySqlDbType.VarChar)  'kViewer用副乗務員情報
                        Dim ORDERHENKO As MySqlParameter = SQLcmd.Parameters.Add("@ORDERHENKO", MySqlDbType.VarChar)    'オーダー変更・削除
                        Dim RIKUUNKYOKU As MySqlParameter = SQLcmd.Parameters.Add("@RIKUUNKYOKU", MySqlDbType.VarChar)  '陸運局
                        Dim BUNRUINUMBER As MySqlParameter = SQLcmd.Parameters.Add("@BUNRUINUMBER", MySqlDbType.VarChar)    '分類番号
                        Dim HIRAGANA As MySqlParameter = SQLcmd.Parameters.Add("@HIRAGANA", MySqlDbType.VarChar)    'ひらがな
                        Dim ITIRENNUM As MySqlParameter = SQLcmd.Parameters.Add("@ITIRENNUM", MySqlDbType.VarChar)  '一連指定番号
                        Dim TRACTER1 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER1", MySqlDbType.VarChar)    '陸運局_トラクタ
                        Dim TRACTER2 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER2", MySqlDbType.VarChar)    '分類番号_トラクタ
                        Dim TRACTER3 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER3", MySqlDbType.VarChar)    'ひらがな_トラクタ
                        Dim TRACTER4 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER4", MySqlDbType.VarChar)    '一連指定番号_トラクタ
                        Dim TRACTER5 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER5", MySqlDbType.VarChar)    '車両備考1_トラクタ
                        Dim TRACTER6 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER6", MySqlDbType.VarChar)    '車両備考2_トラクタ
                        Dim TRACTER7 As MySqlParameter = SQLcmd.Parameters.Add("@TRACTER7", MySqlDbType.VarChar)    '車両備考3_トラクタ
                        Dim HAISYAHUKA As MySqlParameter = SQLcmd.Parameters.Add("@HAISYAHUKA", MySqlDbType.VarChar)    '配車・配乗不可[不可]
                        Dim HYOZIZYUNT As MySqlParameter = SQLcmd.Parameters.Add("@HYOZIZYUNT", MySqlDbType.VarChar)    '表示順_届先
                        Dim HYOZIZYUNH As MySqlParameter = SQLcmd.Parameters.Add("@HYOZIZYUNH", MySqlDbType.VarChar)    '表示順_配車
                        Dim HONTRACTER1 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER1", MySqlDbType.VarChar)  '本トラクタ選択
                        Dim HONTRACTER2 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER2", MySqlDbType.VarChar)  '出荷部署名_本トラクタ
                        Dim HONTRACTER3 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER3", MySqlDbType.VarChar)  '業務車番_本トラクタ
                        Dim HONTRACTER4 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER4", MySqlDbType.VarChar)  '出荷部署コード_本トラクタ
                        Dim HONTRACTER5 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER5", MySqlDbType.VarChar)  '出荷部署略名_本トラクタ
                        Dim HONTRACTER6 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER6", MySqlDbType.VarChar)  '加算先出荷部署略名_本トラクタ
                        Dim HONTRACTER7 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER7", MySqlDbType.VarChar)  '加算先出荷部署コード_本トラクタ
                        Dim HONTRACTER8 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER8", MySqlDbType.VarChar)  '加算先出荷部署名_本トラクタ
                        Dim HONTRACTER9 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER9", MySqlDbType.VarChar)  '用車先_本トラクタ
                        Dim HONTRACTER10 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER10", MySqlDbType.VarChar)    '統一車番_本トラクタ
                        Dim HONTRACTER11 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER11", MySqlDbType.VarChar)    '陸事番号_本トラクタ
                        Dim HONTRACTER12 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER12", MySqlDbType.VarChar)    '車型_本トラクタ
                        Dim HONTRACTER13 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER13", MySqlDbType.VarChar)    '車腹_本トラクタ
                        Dim HONTRACTER14 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER14", MySqlDbType.VarChar)    '車腹単位_本トラクタ
                        Dim HONTRACTER15 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER15", MySqlDbType.VarChar)    '陸運局_本トラクタ
                        Dim HONTRACTER16 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER16", MySqlDbType.VarChar)    '分類番号_本トラクタ
                        Dim HONTRACTER17 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER17", MySqlDbType.VarChar)    'ひらがな_本トラクタ
                        Dim HONTRACTER18 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER18", MySqlDbType.VarChar)    '一連指定番号_本トラクタ
                        Dim HONTRACTER19 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER19", MySqlDbType.VarChar)    '荷主名_本トラクタ
                        Dim HONTRACTER20 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER20", MySqlDbType.VarChar)    '契約区分_本トラクタ
                        Dim HONTRACTER21 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER21", MySqlDbType.VarChar)    '品名1名_車両_本トラクタ
                        Dim HONTRACTER22 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER22", MySqlDbType.VarChar)    '車両メモ_本トラクタ
                        Dim HONTRACTER23 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER23", MySqlDbType.VarChar)    '車両備考1_本トラクタ
                        Dim HONTRACTER24 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER24", MySqlDbType.VarChar)    '車両備考2_本トラクタ
                        Dim HONTRACTER25 As MySqlParameter = SQLcmd.Parameters.Add("@HONTRACTER25", MySqlDbType.VarChar)    '車両備考3_本トラクタ
                        Dim ORDSTDATE As MySqlParameter = SQLcmd.Parameters.Add("@ORDSTDATE", MySqlDbType.Date)   'オーダー開始日
                        Dim ORDENDATE As MySqlParameter = SQLcmd.Parameters.Add("@ORDENDATE", MySqlDbType.Date)   'オーダー終了日
                        Dim LUPDKEY As MySqlParameter = SQLcmd.Parameters.Add("@LUPDKEY", MySqlDbType.VarChar)    'L配更新キー
                        Dim HUPDKEY As MySqlParameter = SQLcmd.Parameters.Add("@HUPDKEY", MySqlDbType.VarChar)    'はこぶわ更新キー
                        Dim JXORDUPDKEY As MySqlParameter = SQLcmd.Parameters.Add("@JXORDUPDKEY", MySqlDbType.VarChar)    'JX形式オーダー更新キー
                        Dim JXORDFILE As MySqlParameter = SQLcmd.Parameters.Add("@JXORDROUTE", MySqlDbType.VarChar)    'JX形式オーダーファイル名
                        Dim JXORDROUTE As MySqlParameter = SQLcmd.Parameters.Add("@JXORDFILE", MySqlDbType.VarChar)    'JX形式オーダールート番号
                        Dim JXORDTODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@JXORDTODOKENAME", MySqlDbType.VarChar)    'JX形式オーダー先頭届先名称
                        Dim NIPPONO As MySqlParameter = SQLcmd.Parameters.Add("@NIPPONO", MySqlDbType.VarChar)    '運転日報番号
                        Dim BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar)    '枝番
                        Dim UPDATEUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDATEUSER", MySqlDbType.VarChar)    '更新者
                        Dim CREATEUSER As MySqlParameter = SQLcmd.Parameters.Add("@CREATEUSER", MySqlDbType.VarChar)    '作成者
                        Dim UPDATEYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDATEYMD", MySqlDbType.DateTime) '更新日時
                        Dim CREATEYMD As MySqlParameter = SQLcmd.Parameters.Add("@CREATEYMD", MySqlDbType.DateTime) '作成日時
                        Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)    '削除フラグ
                        Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime) '登録年月日
                        Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar)    '登録ユーザーＩＤ
                        Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar)    '登録端末
                        Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar)    '登録プログラムＩＤ
                        Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)   '更新年月日
                        Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar)  '更新ユーザーＩＤ
                        Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar)  '更新端末
                        Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar)  '更新プログラムＩＤ
                        Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)   '集信日時

                        '前処理でチェックした出力先と同じテーブルに出力する
                        For Each updRow As DataRow In iTbl.Select("OUTTBL='" & TblName & "'")
                            SaveRecordNo = updRow("レコード番号")
                            SaveTori = updRow("届先取引先コード")
                            SaveToriName = updRow("届先取引先名称")
                            SaveOrg = updRow("受注受付部署コード")
                            SaveOrgName = updRow("受注受付部署名")

                            RECONO.Value = updRow("レコード番号") 'レコード番号
                            LOADUNLOTYPE.Value = updRow("積込荷卸区分")   '積込荷卸区分
                            STACKINGTYPE.Value = updRow("積置区分") '積置区分
                            HSETID.Value = updRow("配送セットID")    '配送セットID
                            ORDERORGSELECT.Value = updRow("受注受付部署選択")   '受注受付部署選択
                            ORDERORGNAME.Value = updRow("受注受付部署名")  '受注受付部署名
                            ORDERORGCODE.Value = updRow("受注受付部署コード")    '受注受付部署コード
                            ORDERORGNAMES.Value = updRow("受注受付部署略名")    '受注受付部署略名
                            KASANAMEORDERORG.Value = updRow("加算先部署名_受注受付部署")    '加算先部署名_受注受付部署
                            KASANCODEORDERORG.Value = updRow("加算先部署コード_受注受付部署") '加算先部署コード_受注受付部署
                            KASANAMESORDERORG.Value = updRow("加算先部署略名_受注受付部署")  '加算先部署略名_受注受付部署
                            ORDERORG.Value = updRow("受注受付部署")   '受注受付部署
                            KASANORDERORG.Value = updRow("加算先受注受付部署")   '加算先受注受付部署
                            PRODUCTSLCT.Value = updRow("品名選択")  '品名選択
                            PRODUCTSYOSAI.Value = updRow("品名詳細")    '品名詳細
                            PRODUCT2NAME.Value = updRow("品名2名") '品名2名
                            PRODUCT2.Value = updRow("品名2コード")   '品名2コード
                            PRODUCT1NAME.Value = updRow("品名1名") '品名1名
                            PRODUCT1.Value = updRow("品名1コード")   '品名1コード
                            OILNAME.Value = updRow("油種名")   '油種名
                            OILTYPE.Value = updRow("油種コード") '油種コード
                            TODOKESLCT.Value = updRow("届先選択")   '届先選択
                            TODOKECODE.Value = updRow("届先コード")  '届先コード
                            TODOKENAME.Value = updRow("届先名称")   '届先名称
                            TODOKENAMES.Value = updRow("届先略名")  '届先略名
                            TORICODE.Value = updRow("届先取引先コード") '届先取引先コード（先頭5桁＋"00000"に編集済）
                            TORINAME.Value = updRow("届先取引先名称")  '届先取引先名称
                            TORICODE_AVOCADO.Value = updRow("TORICODE_AVOCADO") '届先取引先コード（アボカドコードをそのまま）
                            TODOKECONTNAME.Value = updRow("届先契約先名称") '届先契約先名称
                            TODOKEADDR.Value = updRow("届先住所")   '届先住所
                            TODOKETEL.Value = updRow("届先電話番号")  '届先電話番号
                            If updRow("届先緯度") = "" AndAlso updRow("届先経度") = "" Then
                                TODOKEMAP.Value = updRow("届先Googleマップ")  '届先Googleマップ
                            Else
                                TODOKEMAP.Value = String.Format("https://www.google.com/maps?q={0},{1}", updRow("届先緯度"), updRow("届先経度"))  '届先Googleマップ
                            End If
                            TODOKEIDO.Value = updRow("届先緯度")    '届先緯度
                            TODOKEKEIDO.Value = updRow("届先経度")  '届先経度
                            TODOKEBIKO1.Value = updRow("届先備考1") '届先備考1
                            TODOKEBIKO2.Value = updRow("届先備考2") '届先備考2
                            TODOKEBIKO3.Value = updRow("届先備考3") '届先備考3
                            SHUKASLCT.Value = updRow("出荷場所選択")  '出荷場所選択
                            SHUKABASHO.Value = updRow("出荷場所コード")    '出荷場所コード
                            SHUKANAME.Value = updRow("出荷場所名称")  '出荷場所名称
                            SHUKANAMES.Value = updRow("出荷場所略名") '出荷場所略名
                            SHUKATORICODE.Value = updRow("出荷場所取引先コード")  '出荷場所取引先コード
                            SHUKATORINAME.Value = updRow("出荷場所取引先名称")   '出荷場所取引先名称
                            SHUKACONTNAME.Value = updRow("出荷場所契約先名称")   '出荷場所契約先名称
                            SHUKAADDR.Value = updRow("出荷場所住所")  '出荷場所住所
                            SHUKAADDRTEL.Value = updRow("出荷場所電話番号") '出荷場所電話番号
                            If updRow("出荷場所緯度") = "" AndAlso updRow("出荷場所経度") = "" Then
                                SHUKAMAP.Value = updRow("出荷場所Googleマップ")    '出荷場所Googleマップ
                            Else
                                SHUKAMAP.Value = String.Format("https://www.google.com/maps?q={0},{1}", updRow("出荷場所緯度"), updRow("出荷場所経度"))  '出荷場所Googleマップ
                            End If
                            SHUKAIDO.Value = updRow("出荷場所緯度")   '出荷場所緯度
                            SHUKAKEIDO.Value = updRow("出荷場所経度") '出荷場所経度
                            SHUKABIKOU1.Value = updRow("出荷場所備考1")   '出荷場所備考1
                            SHUKABIKOU2.Value = updRow("出荷場所備考2")   '出荷場所備考2
                            SHUKABIKOU3.Value = updRow("出荷場所備考3")   '出荷場所備考3
                            If iTbl.Columns.Contains("標準所要時間") Then
                                REQUIREDTIME.Value = updRow("標準所要時間") '標準所要時間
                            Else
                                REQUIREDTIME.Value = "" '標準所要時間
                            End If
                            If String.IsNullOrEmpty(updRow("出荷日")) Then
                                SHUKADATE.Value = DBNull.Value    '出荷日
                            Else
                                SHUKADATE.Value = updRow("出荷日") '出荷日
                            End If
                            LOADTIME.Value = updRow("積込時間") '積込時間
                            LOADTIMEIN.Value = updRow("積込時間手入力")    '積込時間手入力
                            LOADTIMES.Value = updRow("積込時間_画面表示用")  '積込時間_画面表示用
                            If String.IsNullOrEmpty(updRow("届日")) Then
                                TODOKEDATE.Value = DBNull.Value '届日
                            Else
                                TODOKEDATE.Value = updRow("届日") '届日
                            End If
                            SHITEITIME.Value = updRow("指定時間")   '指定時間
                            SHITEITIMEIN.Value = updRow("指定時間手入力")  '指定時間手入力
                            SHITEITIMES.Value = updRow("指定時間_画面表示用")    '指定時間_画面表示用
                            If String.IsNullOrEmpty(updRow("受注数量")) Then
                                ZYUTYU.Value = 0   '受注数量
                            Else
                                ZYUTYU.Value = updRow("受注数量")   '受注数量
                            End If
                            If String.IsNullOrEmpty(updRow("実績数量")) Then
                                ZISSEKI.Value = 0  '実績数量
                            Else
                                ZISSEKI.Value = updRow("実績数量")  '実績数量
                            End If
                            TANNI.Value = updRow("数量単位")    '数量単位
                            GYOUMUSIZI1.Value = updRow("業務指示1") '業務指示1
                            GYOUMUSIZI2.Value = updRow("業務指示2") '業務指示2
                            GYOUMUSIZI3.Value = updRow("業務指示3") '業務指示3
                            If iTbl.Columns.Contains("荷主備考") Then
                                NINUSHIBIKOU.Value = updRow("荷主備考") '荷主備考
                            Else
                                NINUSHIBIKOU.Value = ""    '荷主備考
                            End If
                            MAXCAPACITY.Value = updRow("最大積載量") '最大積載量
                            GYOMUSYABAN.Value = updRow("業務車番選択")    '業務車番選択
                            SHIPORGNAME.Value = updRow("出荷部署名") '出荷部署名
                            SHIPORG.Value = updRow("出荷部署コード")   '出荷部署コード
                            SHIPORGNAMES.Value = updRow("出荷部署略名")   '出荷部署略名
                            KASANSHIPORGNAME.Value = updRow("加算先出荷部署名") '加算先出荷部署名
                            KASANSHIPORG.Value = updRow("加算先出荷部署コード")   '加算先出荷部署コード
                            KASANSHIPORGNAMES.Value = updRow("加算先出荷部署略名")   '加算先出荷部署略名
                            TANKNUM.Value = updRow("統一車番")  '統一車番
                            TANKNUMBER.Value = updRow("陸事番号")   '陸事番号
                            SYAGATA.Value = updRow("車型")    '車型
                            SYABARA.Value = updRow("車腹")    '車腹
                            NINUSHINAME.Value = updRow("荷主名")   '荷主名
                            CONTYPE.Value = updRow("契約区分")  '契約区分
                            PRO1SYARYOU.Value = updRow("品名1名_車両")   '品名1名_車両
                            TANKMEMO.Value = updRow("車両メモ") '車両メモ
                            TANKBIKOU1.Value = updRow("車両備考1")  '車両備考1
                            TANKBIKOU2.Value = updRow("車両備考2")  '車両備考2
                            TANKBIKOU3.Value = updRow("車両備考3")  '車両備考3
                            TRACTORNUM.Value = updRow("統一車番_トラクタ")  '統一車番_トラクタ
                            TRACTORNUMBER.Value = updRow("陸事番号_トラクタ")   '陸事番号_トラクタ
                            If String.IsNullOrEmpty(updRow("トリップ")) Then
                                TRIP.Value = 0 'トリップ
                            Else
                                TRIP.Value = updRow("トリップ") 'トリップ
                            End If
                            If String.IsNullOrEmpty(updRow("ドロップ")) Then
                                DRP.Value = 0  'ドロップ
                            Else
                                DRP.Value = updRow("ドロップ")  'ドロップ
                            End If
                            If iTbl.Columns.Contains("回転数") Then
                                If String.IsNullOrEmpty(updRow("回転数")) Then
                                    ROTATION.Value = 0  '回転数
                                Else
                                    ROTATION.Value = updRow("回転数")  '回転数
                                End If
                            Else
                                ROTATION.Value = 0  '回転数
                            End If
                            'UNKOUMEMO.Value = updRow("当日前後運行メモ")    '当日前後運行メモ      2025/06/05 削除
                            SHUKKINTIME.Value = updRow("出勤時間")  '出勤時間
                            STAFFSLCT.Value = updRow("乗務員選択")   '乗務員選択
                            STAFFNAME.Value = updRow("氏名_乗務員")  '氏名_乗務員
                            STAFFCODE.Value = updRow("社員番号_乗務員")    '社員番号_乗務員
                            SUBSTAFFSLCT.Value = updRow("副乗務員選択")   '副乗務員選択
                            SUBSTAFFNAME.Value = updRow("氏名_副乗務員")  '氏名_副乗務員
                            SUBSTAFFNUM.Value = updRow("社員番号_副乗務員") '社員番号_副乗務員
                            GYOMUTANKNUM.Value = updRow("業務車番") '業務車番
                            YOUSYA.Value = updRow("用車先")    '用車先
                            RECOTITLE.Value = updRow("レコードタイトル用")   'レコードタイトル用
                            If String.IsNullOrEmpty(updRow("出庫日")) Then
                                SHUKODATE.Value = DBNull.Value '出庫日
                            Else
                                SHUKODATE.Value = updRow("出庫日") '出庫日
                            End If
                            If String.IsNullOrEmpty(updRow("帰庫日")) Then
                                KIKODATE.Value = DBNull.Value  '帰庫日
                            Else
                                KIKODATE.Value = updRow("帰庫日")  '帰庫日
                            End If
                            KIKOTIME.Value = updRow("帰庫時間") '帰庫時間
                            If String.IsNullOrEmpty(updRow("距離")) Then
                                DISTANCE.Value = 0 '距離
                            Else
                                DISTANCE.Value = updRow("距離") '距離
                            End If
                            CREWBIKOU1.Value = updRow("乗務員備考1") '乗務員備考1
                            CREWBIKOU2.Value = updRow("乗務員備考2") '乗務員備考2
                            CREWSEQ.Value = updRow("表示順_乗務員") '表示順_乗務員
                            SUBCREWBIKOU1.Value = updRow("副乗務員備考1") '副乗務員備考1
                            SUBCREWBIKOU2.Value = updRow("副乗務員備考2") '副乗務員備考2
                            SUBCREWSEQ.Value = updRow("表示順_副乗務員") '表示順_副乗務員
                            SUBNGSYAGATA.Value = updRow("乗務不可車型_副乗務員") '乗務不可車型_副乗務員
                            SUBSHUKKINTIME.Value = updRow("出勤時間_副乗務員")  '出勤時間_副乗務員
                            SYABARATANNI.Value = updRow("車腹単位") '車腹単位
                            TAIKINTIME.Value = updRow("退勤見込み時間")   '退勤見込み時間　2025/08/14 更新
                            If String.IsNullOrEmpty(updRow("マルヨ")) Then
                                MARUYO.Value = 0   'マルヨ　2025/08/14 更新
                            Else
                                MARUYO.Value = updRow("マルヨ")   'マルヨ　2025/08/14 更新
                            End If
                            SUBTIKINTIME.Value = updRow("退勤見込み時間_副乗務員")    '退勤時間_副乗務員　2025/08/14 更新
                            If String.IsNullOrEmpty(updRow("マルヨ_副乗務員")) Then
                                SUBMARUYO.Value = 0    'マルヨ_副乗務員　2025/08/14 更新
                            Else
                                SUBMARUYO.Value = updRow("マルヨ_副乗務員")    'マルヨ_副乗務員　2025/08/14 更新
                            End If
                            KVTITLE.Value = updRow("kViewer用タイトル_届先名なし")  'kViewer用タイトル_届先名なし　2025/08/14 更新
                            KVTITLETODOKE.Value = updRow("kViewer用タイトル_届先名あり")  'kViewer用タイトル_届先名あり　2025/08/14 更新
                            KVZYUTYU.Value = updRow("kViewer用受注数量") 'kViewer用受注数量
                            KVZISSEKI.Value = updRow("kViewer用実績数量")    'kViewer用実績数量
                            KVCREW.Value = updRow("kViewer用乗務員情報")  'kViewer用乗務員情報
                            CREWCODE.Value = updRow("乗務員コード_乗務員")   '乗務員コード_乗務員
                            SUBCREWCODE.Value = updRow("乗務員コード_副乗務員")   '乗務員コード_副乗務員
                            KVSUBCREW.Value = updRow("kViewer用副乗務員情報")  'kViewer用副乗務員情報
                            ORDERHENKO.Value = updRow("オーダー変更削除")  'オーダー変更・削除
                            RIKUUNKYOKU.Value = updRow("陸運局")   '陸運局
                            BUNRUINUMBER.Value = updRow("分類番号") '分類番号
                            HIRAGANA.Value = updRow("ひらがな") 'ひらがな
                            ITIRENNUM.Value = updRow("一連指定番号")  '一連指定番号
                            TRACTER1.Value = updRow("陸運局_トラクタ") '陸運局_トラクタ
                            TRACTER2.Value = updRow("分類番号_トラクタ")    '分類番号_トラクタ
                            TRACTER3.Value = updRow("ひらがな_トラクタ")    'ひらがな_トラクタ
                            TRACTER4.Value = updRow("一連指定番号_トラクタ")  '一連指定番号_トラクタ
                            TRACTER5.Value = updRow("車両備考1_トラクタ")   '車両備考1_トラクタ
                            TRACTER6.Value = updRow("車両備考2_トラクタ")   '車両備考2_トラクタ
                            TRACTER7.Value = updRow("車両備考3_トラクタ")   '車両備考3_トラクタ
                            HAISYAHUKA.Value = updRow("配車配乗不可")    '配車・配乗不可[不可]
                            HYOZIZYUNT.Value = updRow("表示順_届先") '表示順_届先
                            HYOZIZYUNH.Value = updRow("表示順_配車") '表示順_配車
                            HONTRACTER1.Value = updRow("本トラクタ選択")   '本トラクタ選択
                            HONTRACTER2.Value = updRow("出荷部署名_本トラクタ")   '出荷部署名_本トラクタ
                            HONTRACTER3.Value = updRow("業務車番_本トラクタ")    '業務車番_本トラクタ
                            HONTRACTER4.Value = updRow("出荷部署コード_本トラクタ") '出荷部署コード_本トラクタ
                            HONTRACTER5.Value = updRow("出荷部署略名_本トラクタ")  '出荷部署略名_本トラクタ
                            HONTRACTER6.Value = updRow("加算先出荷部署略名_本トラクタ")   '加算先出荷部署略名_本トラクタ
                            HONTRACTER7.Value = updRow("加算先出荷部署コード_本トラクタ")  '加算先出荷部署コード_本トラクタ
                            HONTRACTER8.Value = updRow("加算先出荷部署名_本トラクタ")    '加算先出荷部署名_本トラクタ
                            HONTRACTER9.Value = updRow("用車先_本トラクタ") '用車先_本トラクタ
                            HONTRACTER10.Value = updRow("統一車番_本トラクタ")   '統一車番_本トラクタ
                            HONTRACTER11.Value = updRow("陸事番号_本トラクタ")   '陸事番号_本トラクタ
                            HONTRACTER12.Value = updRow("車型_本トラクタ") '車型_本トラクタ
                            HONTRACTER13.Value = updRow("車腹_本トラクタ") '車腹_本トラクタ
                            HONTRACTER14.Value = updRow("車腹単位_本トラクタ")   '車腹単位_本トラクタ
                            HONTRACTER15.Value = updRow("陸運局_本トラクタ")    '陸運局_本トラクタ
                            HONTRACTER16.Value = updRow("分類番号_本トラクタ")   '分類番号_本トラクタ
                            HONTRACTER17.Value = updRow("ひらがな_本トラクタ")   'ひらがな_本トラクタ
                            HONTRACTER18.Value = updRow("一連指定番号_本トラクタ") '一連指定番号_本トラクタ
                            HONTRACTER19.Value = updRow("荷主名_本トラクタ")    '荷主名_本トラクタ
                            HONTRACTER20.Value = updRow("契約区分_本トラクタ")   '契約区分_本トラクタ
                            HONTRACTER21.Value = updRow("品名1名_車両_本トラクタ")    '品名1名_車両_本トラクタ
                            HONTRACTER22.Value = updRow("車両メモ_本トラクタ")   '車両メモ_本トラクタ
                            HONTRACTER23.Value = updRow("車両備考1_本トラクタ")  '車両備考1_本トラクタ
                            HONTRACTER24.Value = updRow("車両備考2_本トラクタ")  '車両備考2_本トラクタ
                            HONTRACTER25.Value = updRow("車両備考3_本トラクタ")  '車両備考3_本トラクタ
                            If String.IsNullOrEmpty(updRow("オーダー開始日")) Then
                                ORDSTDATE.Value = DBNull.Value 'オーダー開始日
                            Else
                                ORDSTDATE.Value = updRow("オーダー開始日") 'オーダー開始日
                            End If
                            If String.IsNullOrEmpty(updRow("オーダー終了日")) Then
                                ORDENDATE.Value = DBNull.Value 'オーダー終了日
                            Else
                                ORDENDATE.Value = updRow("オーダー終了日") 'オーダー終了日
                            End If
                            '2025/08/14 UPDATE
                            If iTbl.Columns.Contains("L配更新キー") Then
                                LUPDKEY.Value = updRow("L配更新キー")    'L配更新キー
                            Else
                                LUPDKEY.Value = ""    'L配更新キー
                            End If
                            If iTbl.Columns.Contains("はこぶわ更新キー") Then
                                HUPDKEY.Value = updRow("はこぶわ更新キー")    'はこぶわ更新キー
                            Else
                                HUPDKEY.Value = ""    'はこぶわ更新キー
                            End If
                            If iTbl.Columns.Contains("JX形式オーダー更新キー") Then
                                JXORDUPDKEY.Value = updRow("JX形式オーダー更新キー")    'JX形式オーダー更新キー
                            Else
                                JXORDUPDKEY.Value = ""    'JX形式オーダー更新キー
                            End If
                            If iTbl.Columns.Contains("JX形式オーダーファイル名") Then
                                JXORDFILE.Value = updRow("JX形式オーダーファイル名")    'JX形式オーダーファイル名
                            Else
                                JXORDFILE.Value = ""    'JX形式オーダーファイル名
                            End If
                            If iTbl.Columns.Contains("JX形式オーダールート番号") Then
                                JXORDROUTE.Value = updRow("JX形式オーダールート番号")   'JX形式オーダールート番号
                            Else
                                JXORDROUTE.Value = ""   'JX形式オーダールート番号
                            End If
                            If iTbl.Columns.Contains("JX形式オーダー先頭届先名称") Then
                                JXORDTODOKENAME.Value = updRow("JX形式オーダー先頭届先名称")   'JX形式オーダー先頭届先名称
                            Else
                                JXORDTODOKENAME.Value = ""   'JX形式オーダー先頭届先名称
                            End If
                            NIPPONO.Value = updRow("運転日報番号")    '運転日報番号
                            BRANCHCODE.Value = "1"    '枝番
                            UPDATEUSER.Value = updRow("更新者")    '更新者
                            CREATEUSER.Value = updRow("作成者")    '作成者
                            If String.IsNullOrEmpty(updRow("更新日時")) Then
                                UPDATEYMD.Value = DBNull.Value    '更新日時
                            Else
                                UPDATEYMD.Value = updRow("更新日時")    '更新日時
                            End If
                            If String.IsNullOrEmpty(updRow("作成日時")) Then
                                CREATEYMD.Value = DBNull.Value    '作成日時
                            Else
                                CREATEYMD.Value = updRow("作成日時")    '作成日時
                            End If
                            DELFLG.Value = C_DELETE_FLG.ALIVE  '削除フラグ
                            INITYMD.Value = WW_DateNow                                      '登録年月日
                            INITUSER.Value = Master.USERID                                   '登録ユーザーＩＤ
                            INITTERMID.Value = Master.USERTERMID                               '登録端末
                            INITPGID.Value = Me.GetType().BaseType.Name                      '登録プログラムＩＤ
                            UPDYMD.Value = WW_DateNow                                      '更新年月日
                            UPDUSER.Value = Master.USERID                                   '更新ユーザーＩＤ
                            UPDTERMID.Value = Master.USERTERMID                               '更新端末
                            UPDPGID.Value = Me.GetType().BaseType.Name                      '更新プログラムＩＤ
                            RECEIVEYMD.Value = C_DEFAULT_YMD                                   '集信日時

                            SQLcmd.CommandTimeout = 300
                            SQLcmd.ExecuteNonQuery()
                        Next
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                    CS0011LOGWrite.INFPOSI = String.Format("DB:{0} UPDATE_INSERT", TblName)
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = "（レコード番号：" & SaveRecordNo & " 営業所：" & SaveOrg & "）" & ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                    oResult = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
            Next

            '○ DB更新SQL(実績取込履歴)
            SQLStr =
                  "     INSERT INTO LNG.LNT0003_ZISSEKIHIST                               " _
                & "        (TAISHOYM                                                      " _
                & "       , TORICODE                                                      " _
                & "       , TORINAME                                                      " _
                & "       , SHIPORG                                                       " _
                & "       , SHIPORGNAME                                                   " _
                & "       , USERID                                                        " _
                & "       , USERNAME                                                      " _
                & "       , INTAKEDATE                                                    " _
                & "       , DELFLG                                                        " _
                & "       , INITYMD                                                       " _
                & "       , INITUSER                                                      " _
                & "       , INITTERMID                                                    " _
                & "       , INITPGID                                                      " _
                & "       , UPDYMD                                                        " _
                & "       , UPDUSER                                                       " _
                & "       , UPDTERMID                                                     " _
                & "       , UPDPGID                                                       " _
                & "       , RECEIVEYMD)                                                   " _
                & "     VALUES                                                            " _
                & "        (@TAISHOYM                                                     " _
                & "       , @TORICODE                                                      " _
                & "       , @TORINAME                                                      " _
                & "       , @SHIPORG                                                      " _
                & "       , @SHIPORGNAME                                                  " _
                & "       , @USERID                                                       " _
                & "       , @USERNAME                                                     " _
                & "       , @INTAKEDATE                                                   " _
                & "       , @DELFLG                                                       " _
                & "       , @INITYMD                                                      " _
                & "       , @INITUSER                                                     " _
                & "       , @INITTERMID                                                   " _
                & "       , @INITPGID                                                     " _
                & "       , @UPDYMD                                                       " _
                & "       , @UPDUSER                                                      " _
                & "       , @UPDTERMID                                                    " _
                & "       , @UPDPGID                                                      " _
                & "       , @RECEIVEYMD)                                                  " _
                & "     ON DUPLICATE KEY UPDATE                                           " _
                & "         USERID      = @USERID                                         " _
                & "       , USERNAME    = @USERNAME                                       " _
                & "       , SHIPORGNAME = @SHIPORGNAME                                    " _
                & "       , DELFLG      = @DELFLG                                         " _
                & "       , UPDYMD      = @UPDYMD                                         " _
                & "       , UPDUSER     = @UPDUSER                                        " _
                & "       , UPDTERMID   = @UPDTERMID                                      " _
                & "       , UPDPGID     = @UPDPGID                                        " _
                & "       , RECEIVEYMD  = @RECEIVEYMD                                     "

            Try
                Dim toriList As New ListBox
                GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
                GS0007FIXVALUElst.CLAS = "TORICODEDROP"
                GS0007FIXVALUElst.LISTBOX1 = toriList
                GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = ""
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                If Not isNormal(GS0007FIXVALUElst.ERR) Then
                    Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "固定値取得エラー")
                    Exit Sub
                End If

                'グルーピング（受注部署、取引先）
                Dim queryG = From row In iTbl.AsEnumerable()
                             Group row By ORDERORG = row.Field(Of String)("受注受付部署コード"),
                                          ORDERORGNAME = row.Field(Of String)("受注受付部署名"),
                                          TORICODE = row.Field(Of String)("届先取引先コード") Into Group
                             Select New With {
                            .ORDERORG = ORDERORG,
                            .ORDERORGNAME = ORDERORGNAME,
                            .TORICODE = TORICODE
                        }

                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(ユーザーパスワードマスタ)
                    Dim TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)             '対象年月
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)            '取引先コード
                    Dim TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 20)            '取引先名
                    Dim SHIPORG As MySqlParameter = SQLcmd.Parameters.Add("@SHIPORG", MySqlDbType.VarChar, 6)               '営業所コード
                    Dim SHIPORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@SHIPORGNAME", MySqlDbType.VarChar)          '営業所名
                    Dim USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 50)                'ユーザーID
                    Dim USERNAME As MySqlParameter = SQLcmd.Parameters.Add("@USERNAME", MySqlDbType.VarChar, 20)            'ユーザー名
                    Dim INTAKEDATE As MySqlParameter = SQLcmd.Parameters.Add("@INTAKEDATE", MySqlDbType.DateTime)           '最終事績取込日
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    For Each result In queryG
                        ' DB更新
                        TAISHOYM.Value = WF_TaishoYm.Value.Replace("/", "")                     '対象年月
                        TORICODE.Value = result.TORICODE                                        '取引先コード
                        For i As Integer = 0 To toriList.Items.Count - 1
                            If result.TORICODE = toriList.Items(i).Value Then
                                TORINAME.Value = toriList.Items(i).Text                         '取引先名
                            End If
                        Next
                        SHIPORG.Value = result.ORDERORG                                         '営業所コード
                        SHIPORGNAME.Value = result.ORDERORGNAME                                 '営業所名
                        USERID.Value = Master.USERID                                            'ユーザーID
                        CS0051UserInfo.USERID = Master.USERID
                        CS0051UserInfo.getInfo()
                        If isNormal(CS0051UserInfo.ERR) Then
                            USERNAME.Value = CS0051UserInfo.STAFFNAMES                          'ユーザー名
                        Else
                            USERNAME.Value = ""                                                 'ユーザー名
                        End If
                        INTAKEDATE.Value = WW_DateNow                                           '最終事績取込日
                        DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ
                        INITYMD.Value = WW_DateNow                                              '登録年月日
                        INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                        INITTERMID.Value = Master.USERTERMID                                    '登録端末
                        INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                        UPDYMD.Value = WW_DateNow                                               '更新年月日
                        UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                        UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                        UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                        RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                    Next

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0003 INSERT"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub
    ''' <summary>
    ''' 輸送費テーブル更新
    ''' </summary>
    Private Sub YusouhiUpdate(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim ToriCodeArray() As String = iTori.Split(",")

        For Each ToriCode As String In ToriCodeArray

            '荷主選択
            Select Case ToriCode
                Case CONST_TORICODE_0005700000    'ＥＮＥＯＳ株式会社ガス事業部
                    ENEOS_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0045200000    'エスケイ産業株式会社
                    ESUKEI_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0045300000    'エスジーリキッドサービス株式会社
                    SAIBUGUS_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0051200000    'Ｄａｉｇａｓエナジー株式会社液化ガスエネ
                    OG_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0110600000    '株式会社シーエナジー
                    CENALNESU_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0132800000    '石油資源開発株式会社営業本部
                    SEKIYUHOKKAIDO_Update(ToriCode, iTaishoYm, WW_ErrSW)
                    SEKIYUHONSYU_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case "0167600000"    '東京ガスケミカル株式会社
                    TOKYOGUS_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case "0175300000"    '東北天然ガス株式会社営業部
                    TNG_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case "0175400000"    '東北電力株式会社グループ事業推進部
                    TOHOKU_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case CONST_TORICODE_0238900000    '北陸エルネス
                    CENALNESU_Update(ToriCode, iTaishoYm, WW_ErrSW)
                Case "0239900000"    '北海道ＬＮＧ株式会社
                    HOKKAIDOLNG_Update(ToriCode, iTaishoYm, WW_ErrSW)

            End Select
        Next

    End Sub

    ''' <summary>
    ''' ENEOS輸送費テーブル更新
    ''' </summary>
    Private Sub ENEOS_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(ENEOS輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0016_ENEOSYUSOUHI                                 " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(ENEOS輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0016_ENEOSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(ENEOS輸送費テーブル)
            SQLStr =
              " INSERT INTO LNG.LNT0016_ENEOSYUSOUHI(                                                                                   " _
            & "     RECONO,                                                                                                             " _
            & "     LOADUNLOTYPE,                                                                                                       " _
            & "     STACKINGTYPE,                                                                                                       " _
            & "     ORDERORGCODE,                                                                                                       " _
            & "     ORDERORGNAME,                                                                                                       " _
            & "     KASANAMEORDERORG,                                                                                                   " _
            & "     KASANCODEORDERORG,                                                                                                  " _
            & "     ORDERORG,                                                                                                           " _
            & "     PRODUCT2NAME,                                                                                                       " _
            & "     PRODUCT2,                                                                                                           " _
            & "     PRODUCT1NAME,                                                                                                       " _
            & "     PRODUCT1,                                                                                                           " _
            & "     OILNAME,                                                                                                            " _
            & "     OILTYPE,                                                                                                            " _
            & "     TODOKECODE,                                                                                                         " _
            & "     TODOKENAME,                                                                                                         " _
            & "     TODOKENAMES,                                                                                                        " _
            & "     TORICODE,                                                                                                           " _
            & "     TORINAME,                                                                                                           " _
            & "     SHUKABASHO,                                                                                                         " _
            & "     SHUKANAME,                                                                                                          " _
            & "     SHUKANAMES,                                                                                                         " _
            & "     SHUKATORICODE,                                                                                                      " _
            & "     SHUKATORINAME,                                                                                                      " _
            & "     SHUKADATE,                                                                                                          " _
            & "     LOADTIME,                                                                                                           " _
            & "     LOADTIMEIN,                                                                                                         " _
            & "     TODOKEDATE,                                                                                                         " _
            & "     SHITEITIME,                                                                                                         " _
            & "     SHITEITIMEIN,                                                                                                       " _
            & "     ZYUTYU,                                                                                                             " _
            & "     ZISSEKI,                                                                                                            " _
            & "     TANNI,                                                                                                              " _
            & "     TANKNUM,                                                                                                            " _
            & "     TANKNUMBER,                                                                                                         " _
            & "     GYOMUTANKNUM,                                                                                                       " _
            & "     SYAGATA,                                                                                                            " _
            & "     SYABARA,                                                                                                            " _
            & "     NINUSHINAME,                                                                                                        " _
            & "     CONTYPE,                                                                                                            " _
            & "     TRIP,                                                                                                               " _
            & "     DRP,                                                                                                                " _
            & "     STAFFSLCT,                                                                                                          " _
            & "     STAFFNAME,                                                                                                          " _
            & "     STAFFCODE,                                                                                                          " _
            & "     SUBSTAFFSLCT,                                                                                                       " _
            & "     SUBSTAFFNAME,                                                                                                       " _
            & "     SUBSTAFFNUM,                                                                                                        " _
            & "     SHUKODATE,                                                                                                          " _
            & "     KIKODATE,                                                                                                           " _
            & "     TANKA,                                                                                                              " _
            & "     JURYORYOKIN,                                                                                                        " _
            & "     TSUKORYO,                                                                                                           " _
            & "     KYUZITUTANKA,                                                                                                       " _
            & "     YUSOUHI,                                                                                                            " _
            & "     WORKINGDAY,                                                                                                         " _
            & "     PUBLICHOLIDAYNAME,                                                                                                  " _
            & "     DELFLG,                                                                                                             " _
            & "     INITYMD,                                                                                                            " _
            & "     INITUSER,                                                                                                           " _
            & "     INITTERMID,                                                                                                         " _
            & "     INITPGID,                                                                                                           " _
            & "     UPDYMD,                                                                                                             " _
            & "     UPDUSER,                                                                                                            " _
            & "     UPDTERMID,                                                                                                          " _
            & "     UPDPGID,                                                                                                            " _
            & "     RECEIVEYMD)                                                                                                         " _
            & " SELECT                                                                                                                  " _
            & "     ZISSEKIMAIN.RECONO            AS RECONO,                                                                            " _
            & "     ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                      " _
            & "     ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                                                                      " _
            & "     ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                                                                      " _
            & "     ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                                                                      " _
            & "     ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                  " _
            & "     ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                 " _
            & "     ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                                                          " _
            & "     ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                                                                      " _
            & "     ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                                                          " _
            & "     ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                                                                      " _
            & "     ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                                                          " _
            & "     ZISSEKIMAIN.OILNAME           AS OILNAME,                                                                           " _
            & "     ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                                                           " _
            & "     ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                                                                        " _
            & "     ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                                                                        " _
            & "     ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                                                                       " _
            & "     ZISSEKIMAIN.TORICODE          AS TORICODE,                                                                          " _
            & "     ZISSEKIMAIN.TORINAME          AS TORINAME,                                                                          " _
            & "     ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                                                                        " _
            & "     ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                                                         " _
            & "     ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                                                                        " _
            & "     ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                                                                     " _
            & "     ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                                                                     " _
            & "     ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                                                         " _
            & "     ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                                                          " _
            & "     ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                                                                        " _
            & "     ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                                                                        " _
            & "     ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                                                                        " _
            & "     ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                                                                      " _
            & "     ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                                                            " _
            & "     ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                                                           " _
            & "     ZISSEKIMAIN.TANNI             AS TANNI,                                                                             " _
            & "     ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                                                           " _
            & "     ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                                                                        " _
            & "     ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                      " _
            & "     ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                                                           " _
            & "     ZISSEKIMAIN.SYABARA           AS SYABARA,                                                                           " _
            & "     ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                                                                       " _
            & "     ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                                                           " _
            & "     ZISSEKIMAIN.TRIP              AS TRIP,                                                                              " _
            & "     ZISSEKIMAIN.DRP               AS DRP,                                                                               " _
            & "     ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                                                         " _
            & "     ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                                                         " _
            & "     ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                                                         " _
            & "     ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                      " _
            & "     ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                      " _
            & "     ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                       " _
            & "     ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                                                         " _
            & "     ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                                                          " _
            & "     ZISSEKIMAIN.TANKA             AS TANKA,                                                                             " _
            & "     NULL                          AS JURYORYOKIN,                                                                       " _
            & "     NULL                          AS TSUKORYO,                                                                          " _
            & "     ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                                                                      " _
            & "     ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                                                           " _
            & "     ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                                                                        " _
            & "     ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                 " _
            & "     ZISSEKIMAIN.DELFLG            AS DELFLG,                                                                            " _
            & "     @INITYMD                      AS INITYMD,                                                                           " _
            & "     @INITUSER                     AS INITUSER,                                                                          " _
            & "     @INITTERMID                   AS INITTERMID,                                                                        " _
            & "     @INITPGID                     AS INITPGID,                                                                          " _
            & "     NULL                          AS UPDYMD,                                                                            " _
            & "     NULL                          AS UPDUSER,                                                                           " _
            & "     NULL                          AS UPDTERMID,                                                                         " _
            & "     NULL                          AS UPDPGID,                                                                           " _
            & "     @RECEIVEYMD                   AS RECEIVEYMD                                                                         " _
            & " FROM(                                                                                                                   " _
            & "      SELECT                                                                                                             " _
            & "          ZISSEKI.RECONO            AS RECONO,                                                                           " _
            & "          ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                     " _
            & "          ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                                                                     " _
            & "          ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                                                                     " _
            & "          ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                                                                     " _
            & "          ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                 " _
            & "          ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                " _
            & "          ZISSEKI.ORDERORG          AS ORDERORG,                                                                         " _
            & "          ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                                                                     " _
            & "          ZISSEKI.PRODUCT2          AS PRODUCT2,                                                                         " _
            & "          ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                                                                     " _
            & "          ZISSEKI.PRODUCT1          AS PRODUCT1,                                                                         " _
            & "          ZISSEKI.OILNAME           AS OILNAME,                                                                          " _
            & "          ZISSEKI.OILTYPE           AS OILTYPE,                                                                          " _
            & "          ZISSEKI.TODOKECODE        AS TODOKECODE,                                                                       " _
            & "          ZISSEKI.TODOKENAME        AS TODOKENAME,                                                                       " _
            & "          ZISSEKI.TODOKENAMES       AS TODOKENAMES,                                                                      " _
            & "          ZISSEKI.TORICODE          AS TORICODE,                                                                         " _
            & "          ZISSEKI.TORINAME          AS TORINAME,                                                                         " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                          " _
            & "          THEN (SELECT SHUKABASHO                                                                                        " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                                                               " _
            & "                 WHERE                                                                                                   " _
            & "                       TORICODE     = ZISSEKI.TORICODE                                                                   " _
            & "                   AND ORDERORG     = ZISSEKI.ORDERORG                                                                   " _
            & "                   AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                               " _
            & "                   AND TRIP         = ZISSEKI.TRIP -1                                                                    " _
            & "                   AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                 " _
            & "                   AND ZISSEKI      > 0                                                                                  " _
            & "                   AND DELFLG       = '0'                                                                                " _
            & "               )                                                                                                         " _
            & "          ELSE ZISSEKI.SHUKABASHO                                                                                        " _
            & "          END AS SHUKABASHO,                                                                                             " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                          " _
            & "          THEN (SELECT SHUKANAME                                                                                         " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                                                               " _
            & "                 WHERE                                                                                                   " _
            & "                       TORICODE     = ZISSEKI.TORICODE                                                                   " _
            & "                   AND ORDERORG     = ZISSEKI.ORDERORG                                                                   " _
            & "                   AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                               " _
            & "                   AND TRIP         = ZISSEKI.TRIP -1                                                                    " _
            & "                   AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                 " _
            & "                   AND ZISSEKI      > 0                                                                                  " _
            & "                   AND DELFLG       = '0'                                                                                " _
            & "               )                                                                                                         " _
            & "          ELSE ZISSEKI.SHUKANAME                                                                                         " _
            & "          END AS SHUKANAME,                                                                                              " _
            & "          ZISSEKI.SHUKANAMES        AS SHUKANAMES,                                                                       " _
            & "          ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                                                                    " _
            & "          ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                                                                    " _
            & "          ZISSEKI.SHUKADATE         AS SHUKADATE,                                                                        " _
            & "          ZISSEKI.LOADTIME          AS LOADTIME,                                                                         " _
            & "          ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                                                                       " _
            & "          ZISSEKI.TODOKEDATE        AS TODOKEDATE,                                                                       " _
            & "          ZISSEKI.SHITEITIME        AS SHITEITIME,                                                                       " _
            & "          ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                                                                     " _
            & "          ZISSEKI.ZYUTYU            AS ZYUTYU,                                                                           " _
            & "          ZISSEKI.ZISSEKI           AS ZISSEKI,                                                                          " _
            & "          ZISSEKI.TANNI             AS TANNI,                                                                            " _
            & "          ZISSEKI.TANKNUM           AS TANKNUM,                                                                          " _
            & "          ZISSEKI.TANKNUMBER        AS TANKNUMBER,                                                                       " _
            & "          ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                     " _
            & "          ZISSEKI.SYAGATA           AS SYAGATA,                                                                          " _
            & "          ZISSEKI.SYABARA           AS SYABARA,                                                                          " _
            & "          ZISSEKI.NINUSHINAME       AS NINUSHINAME,                                                                      " _
            & "          ZISSEKI.CONTYPE           AS CONTYPE,                                                                          " _
            & "          ZISSEKI.TRIP              AS TRIP,                                                                             " _
            & "          ZISSEKI.DRP               AS DRP,                                                                              " _
            & "          ZISSEKI.STAFFSLCT         AS STAFFSLCT,                                                                        " _
            & "          ZISSEKI.STAFFNAME         AS STAFFNAME,                                                                        " _
            & "          ZISSEKI.STAFFCODE         AS STAFFCODE,                                                                        " _
            & "          ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                     " _
            & "          ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                     " _
            & "          ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                      " _
            & "          ZISSEKI.SHUKODATE         AS SHUKODATE,                                                                        " _
            & "          ZISSEKI.KIKODATE          AS KIKODATE,                                                                         " _
            & "          HOLIDAYRATE.TANKA         AS KYUZITUTANKA,                                                                     " _
            & "          TANKA.TANKA               AS TANKA,                                                                            " _
            & "          COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)  + COALESCE(HOLIDAYRATE.TANKA, 0) AS YUSOUHI,          " _
            & "          CALENDAR.WORKINGDAY       AS WORKINGDAY,                                                                       " _
            & "          CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                               " _
            & "          ZISSEKI.DELFLG            AS DELFLG                                                                            " _
            & "      FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                   " _
            & "      LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                               " _
            & "          ON @TORICODE = TANKA.TORICODE                                                                                  " _
            & "          AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                       " _
            & "          AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                             " _
            & "          AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                               " _
            & "          AND REPLACE(ZISSEKI.SYAGATA, '単車タンク', '単車') = TANKA.SYAGATANAME                                         " _
            & "          AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                                                                      " _
            & "          AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                         " _
            & "          AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                         " _
            & "          AND TANKA.DELFLG = @DELFLG                                                                                     " _
            & "      LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                            " _
            & "          ON @TORICODE = CALENDAR.TORICODE                                                                               " _
            & "          AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                          " _
            & "          AND CALENDAR.DELFLG = @DELFLG                                                                                  " _
            & "      LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                      " _
            & "         ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                      " _
            & "         AND ZISSEKI.ORDERORGCODE = HOLIDAYRATE.ORDERORGCODE                                                             " _
            & "         AND ZISSEKI.TODOKECODE = HOLIDAYRATE.TODOKECODE                                                                 " _
            & "         AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                             " _
            & "         AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                " _
            & "      WHERE                                                                                                              " _
            & "          ZISSEKI.TORICODE = @TORICODE                                                                                   " _
            & "          AND ZISSEKI.ZISSEKI <> 0                                                                                       " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                            " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                              " _
            & "          AND ZISSEKI.STACKINGTYPE <> '積置'                                                                             " _
            & "          AND ZISSEKI.DELFLG = @DELFLG                                                                                   " _
            & " ) ZISSEKIMAIN                                                                                                           " _
            & " ON DUPLICATE KEY UPDATE                                                                                                 " _
            & "         RECONO                    = VALUES(RECONO),                                                                     " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                               " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                               " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                               " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                               " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                           " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                          " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                   " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                               " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                   " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                               " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                   " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                    " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                    " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                 " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                 " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                   " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                   " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                 " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                  " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                 " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                              " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                              " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                  " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                   " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                 " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                 " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                 " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                               " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                     " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                    " _
            & "         TANNI                     = VALUES(TANNI),                                                                      " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                    " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                 " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                               " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                    " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                    " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                    " _
            & "         TRIP                      = VALUES(TRIP),                                                                       " _
            & "         DRP                       = VALUES(DRP),                                                                        " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                  " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                  " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                  " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                               " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                               " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                  " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                   " _
            & "         TANKA                     = VALUES(TANKA),                                                                      " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                   " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                               " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                    " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                 " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                          " _
            & "         DELFLG                    = @DELFLG,                                                                            " _
            & "         UPDYMD                    = @UPDYMD,                                                                            " _
            & "         UPDUSER                   = @UPDUSER,                                                                           " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                         " _
            & "         UPDPGID                   = @UPDPGID,                                                                           " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                        "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(ENEOS輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0016_ENEOSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' エスケイ輸送費テーブル更新
    ''' </summary>
    Private Sub ESUKEI_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(エスケイ輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0020_ESUKEIYUSOUHI                                 " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(エスケイ輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0020_ESUKEIYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(エスケイ輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0020_ESUKEIYUSOUHI(                                                                   " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "               WHERE                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                       " _
            & "                 AND ZISSEKI      > 0                                                                        " _
            & "                 AND DELFLG       = '0'                                                                      " _
            & "             )                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "               WHERE                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                       " _
            & "                 AND ZISSEKI      > 0                                                                        " _
            & "                 AND DELFLG       = '0'                                                                      " _
            & "             )                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                              " _
            & "        TANKA.TANKA                AS TANKA,                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                           " _
            & "        NULL                       AS TSUKORYO,                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                          " _
            & "        COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) + COALESCE(HOLIDAYRATE.TANKA, 0) AS YUSOUHI, " _
            & "        CALENDAR.WORKINGDAY        AS WORKINGDAY,                                                            " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                     " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                                                " _
            & "        @INITYMD                   AS INITYMD,                                                               " _
            & "        @INITUSER                  AS INITUSER,                                                              " _
            & "        @INITTERMID                AS INITTERMID,                                                            " _
            & "        @INITPGID                  AS INITPGID,                                                              " _
            & "        NULL                       AS UPDYMD,                                                                " _
            & "        NULL                       AS UPDUSER,                                                               " _
            & "        NULL                       AS UPDTERMID,                                                             " _
            & "        NULL                       AS UPDPGID,                                                               " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                                             " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                            " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                            " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                            " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                   " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                      " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & "    ORDER BY                                                                                                 " _
            & "       SHUKADATE,                                                                                            " _
            & "       TODOKEDATE                                                                                            " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(エスケイ輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0020_ESUKEIYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 西部ガス輸送費テーブル更新
    ''' </summary>
    Private Sub SAIBUGUS_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(西部ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0019_SAIBUGUSYUSOUHI                              " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(西部ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019_SAIBUGUSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(西部ガス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0019_SAIBUGUSYUSOUHI(                                             " _
            & "        RECONO,                                                                          " _
            & "        LOADUNLOTYPE,                                                                    " _
            & "        STACKINGTYPE,                                                                    " _
            & "        ORDERORGCODE,                                                                    " _
            & "        ORDERORGNAME,                                                                    " _
            & "        KASANAMEORDERORG,                                                                " _
            & "        KASANCODEORDERORG,                                                               " _
            & "        ORDERORG,                                                                        " _
            & "        PRODUCT2NAME,                                                                    " _
            & "        PRODUCT2,                                                                        " _
            & "        PRODUCT1NAME,                                                                    " _
            & "        PRODUCT1,                                                                        " _
            & "        OILNAME,                                                                         " _
            & "        OILTYPE,                                                                         " _
            & "        TODOKECODE,                                                                      " _
            & "        TODOKENAME,                                                                      " _
            & "        TODOKENAMES,                                                                     " _
            & "        TORICODE,                                                                        " _
            & "        TORINAME,                                                                        " _
            & "        SHUKABASHO,                                                                      " _
            & "        SHUKANAME,                                                                       " _
            & "        SHUKANAMES,                                                                      " _
            & "        SHUKATORICODE,                                                                   " _
            & "        SHUKATORINAME,                                                                   " _
            & "        SHUKADATE,                                                                       " _
            & "        LOADTIME,                                                                        " _
            & "        LOADTIMEIN,                                                                      " _
            & "        TODOKEDATE,                                                                      " _
            & "        SHITEITIME,                                                                      " _
            & "        SHITEITIMEIN,                                                                    " _
            & "        ZYUTYU,                                                                          " _
            & "        ZISSEKI,                                                                         " _
            & "        TANNI,                                                                           " _
            & "        TANKNUM,                                                                         " _
            & "        TANKNUMBER,                                                                      " _
            & "        GYOMUTANKNUM,                                                                    " _
            & "        SYAGATA,                                                                         " _
            & "        SYABARA,                                                                         " _
            & "        NINUSHINAME,                                                                     " _
            & "        CONTYPE,                                                                         " _
            & "        TRIP,                                                                            " _
            & "        DRP,                                                                             " _
            & "        STAFFSLCT,                                                                       " _
            & "        STAFFNAME,                                                                       " _
            & "        STAFFCODE,                                                                       " _
            & "        SUBSTAFFSLCT,                                                                    " _
            & "        SUBSTAFFNAME,                                                                    " _
            & "        SUBSTAFFNUM,                                                                     " _
            & "        SHUKODATE,                                                                       " _
            & "        KIKODATE,                                                                        " _
            & "        TANKA,                                                                           " _
            & "        JURYORYOKIN,                                                                     " _
            & "        TSUKORYO,                                                                        " _
            & "        KYUZITUTANKA,                                                                    " _
            & "        YUSOUHI,                                                                         " _
            & "        WORKINGDAY,                                                                      " _
            & "        PUBLICHOLIDAYNAME,                                                               " _
            & "        DELFLG,                                                                          " _
            & "        INITYMD,                                                                         " _
            & "        INITUSER,                                                                        " _
            & "        INITTERMID,                                                                      " _
            & "        INITPGID,                                                                        " _
            & "        UPDYMD,                                                                          " _
            & "        UPDUSER,                                                                         " _
            & "        UPDTERMID,                                                                       " _
            & "        UPDPGID,                                                                         " _
            & "        RECEIVEYMD)                                                                      " _
            & "    SELECT                                                                               " _
            & "        ZISSEKI.RECONO             AS RECONO,                                            " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                      " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                      " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                      " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                      " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                  " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                 " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                          " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                      " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                          " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                      " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                          " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                           " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                           " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                        " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                        " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                       " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                          " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                          " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                            " _
            & "        THEN (SELECT SHUKABASHO                                                          " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                 " _
            & "               WHERE                                                                     " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                     " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                     " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                 " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                      " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                   " _
            & "                 AND ZISSEKI      > 0                                                    " _
            & "                 AND DELFLG       = '0'                                                  " _
            & "             )                                                                           " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                          " _
            & "        END AS SHUKABASHO,                                                               " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                            " _
            & "        THEN (SELECT SHUKANAME                                                           " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                 " _
            & "               WHERE                                                                     " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                     " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                     " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                 " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                      " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                   " _
            & "                 AND ZISSEKI      > 0                                                    " _
            & "                 AND DELFLG       = '0'                                                  " _
            & "             )                                                                           " _
            & "        ELSE ZISSEKI.SHUKANAME                                                           " _
            & "        END AS SHUKANAME,                                                                " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                        " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                     " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                     " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                         " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                          " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                        " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                        " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                        " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                      " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                            " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                           " _
            & "        ZISSEKI.TANNI              AS TANNI,                                             " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                           " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                        " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                      " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                           " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                           " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                       " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                           " _
            & "        ZISSEKI.TRIP               AS TRIP,                                              " _
            & "        ZISSEKI.DRP                AS DRP,                                               " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                         " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                         " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                         " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                      " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                      " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                       " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                         " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                          " _
            & "        TANKA.TANKA                AS TANKA,                                             " _
            & "        NULL                       AS JURYORYOKIN,                                       " _
            & "        NULL                       AS TSUKORYO,                                          " _
            & "        NULL                       AS KYUZITUTANKA,                                      " _
            & "        TANKA.TANKA * ZISSEKI.ZISSEKI AS YUSOUHI,                                        " _
            & "        CALENDAR.WORKINGDAY        AS WORKINGDAY,                                        " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                 " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                            " _
            & "        @INITYMD                   AS INITYMD,                                           " _
            & "        @INITUSER                  AS INITUSER,                                          " _
            & "        @INITTERMID                AS INITTERMID,                                        " _
            & "        @INITPGID                  AS INITPGID,                                          " _
            & "        NULL                       AS UPDYMD,                                            " _
            & "        NULL                       AS UPDUSER,                                           " _
            & "        NULL                       AS UPDTERMID,                                         " _
            & "        NULL                       AS UPDPGID,                                           " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                         " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                     " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                 " _
            & "        ON @TORICODE = TANKA.TORICODE                                                    " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                         " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                               " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                 " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                           " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                           " _
            & "        AND TANKA.DELFLG = @DELFLG                                                       " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                        " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                              " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                 " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                            " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                    " _
            & "    WHERE                                                                                " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                     " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                         " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                               " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                              " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                     " _
            & " ON DUPLICATE KEY UPDATE                                                                 " _
            & "         RECONO                    = VALUES(RECONO),                                     " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                               " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                               " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                               " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                               " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                           " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                          " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                   " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                               " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                   " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                               " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                   " _
            & "         OILNAME                   = VALUES(OILNAME),                                    " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                    " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                 " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                 " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                " _
            & "         TORICODE                  = VALUES(TORICODE),                                   " _
            & "         TORINAME                  = VALUES(TORINAME),                                   " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                 " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                  " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                 " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                              " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                              " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                  " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                   " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                 " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                 " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                 " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                               " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                     " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                    " _
            & "         TANNI                     = VALUES(TANNI),                                      " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                    " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                 " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                               " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                    " _
            & "         SYABARA                   = VALUES(SYABARA),                                    " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                    " _
            & "         TRIP                      = VALUES(TRIP),                                       " _
            & "         DRP                       = VALUES(DRP),                                        " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                  " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                  " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                  " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                               " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                               " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                  " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                   " _
            & "         TANKA                     = VALUES(TANKA),                                      " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                   " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                               " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                    " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                 " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                          " _
            & "         DELFLG                    = @DELFLG,                                            " _
            & "         UPDYMD                    = @UPDYMD,                                            " _
            & "         UPDUSER                   = @UPDUSER,                                           " _
            & "         UPDTERMID                 = @UPDTERMID,                                         " _
            & "         UPDPGID                   = @UPDPGID,                                           " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                        "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(西部ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019_SAIBUGUSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' OG輸送費テーブル更新
    ''' </summary>
    Private Sub OG_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(OG輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0022_OGYUSOUHI                                    " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(OG輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0022_OGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(OG輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0022_OGYUSOUHI(                                                                                                       " _
            & "        RECONO,                                                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                                                        " _
            & "        STACKINGTYPE,                                                                                                                        " _
            & "        ORDERORGCODE,                                                                                                                        " _
            & "        ORDERORGNAME,                                                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                                                   " _
            & "        ORDERORG,                                                                                                                            " _
            & "        PRODUCT2NAME,                                                                                                                        " _
            & "        PRODUCT2,                                                                                                                            " _
            & "        PRODUCT1NAME,                                                                                                                        " _
            & "        PRODUCT1,                                                                                                                            " _
            & "        OILNAME,                                                                                                                             " _
            & "        OILTYPE,                                                                                                                             " _
            & "        TODOKECODE,                                                                                                                          " _
            & "        TODOKENAME,                                                                                                                          " _
            & "        TODOKENAMES,                                                                                                                         " _
            & "        TORICODE,                                                                                                                            " _
            & "        TORINAME,                                                                                                                            " _
            & "        SHUKABASHO,                                                                                                                          " _
            & "        SHUKANAME,                                                                                                                           " _
            & "        SHUKANAMES,                                                                                                                          " _
            & "        SHUKATORICODE,                                                                                                                       " _
            & "        SHUKATORINAME,                                                                                                                       " _
            & "        SHUKADATE,                                                                                                                           " _
            & "        LOADTIME,                                                                                                                            " _
            & "        LOADTIMEIN,                                                                                                                          " _
            & "        TODOKEDATE,                                                                                                                          " _
            & "        SHITEITIME,                                                                                                                          " _
            & "        SHITEITIMEIN,                                                                                                                        " _
            & "        ZYUTYU,                                                                                                                              " _
            & "        ZISSEKI,                                                                                                                             " _
            & "        TANNI,                                                                                                                               " _
            & "        TANKNUM,                                                                                                                             " _
            & "        TANKNUMBER,                                                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                                                        " _
            & "        SYAGATA,                                                                                                                             " _
            & "        SYABARA,                                                                                                                             " _
            & "        NINUSHINAME,                                                                                                                         " _
            & "        CONTYPE,                                                                                                                             " _
            & "        TRIP,                                                                                                                                " _
            & "        DRP,                                                                                                                                 " _
            & "        STAFFSLCT,                                                                                                                           " _
            & "        STAFFNAME,                                                                                                                           " _
            & "        STAFFCODE,                                                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                                                         " _
            & "        SHUKODATE,                                                                                                                           " _
            & "        KIKODATE,                                                                                                                            " _
            & "        TANKA,                                                                                                                               " _
            & "        JURYORYOKIN,                                                                                                                         " _
            & "        TSUKORYO,                                                                                                                            " _
            & "        KYUZITUTANKA,                                                                                                                        " _
            & "        YUSOUHI,                                                                                                                             " _
            & "        WORKINGDAY,                                                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                                                   " _
            & "        DELFLG,                                                                                                                              " _
            & "        INITYMD,                                                                                                                             " _
            & "        INITUSER,                                                                                                                            " _
            & "        INITTERMID,                                                                                                                          " _
            & "        INITPGID,                                                                                                                            " _
            & "        UPDYMD,                                                                                                                              " _
            & "        UPDUSER,                                                                                                                             " _
            & "        UPDTERMID,                                                                                                                           " _
            & "        UPDPGID,                                                                                                                             " _
            & "        RECEIVEYMD)                                                                                                                          " _
            & "    SELECT                                                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                                     " _
            & "               WHERE                                                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                       " _
            & "                 AND ZISSEKI      > 0                                                                                                        " _
            & "                 AND DELFLG       = '0'                                                                                                      " _
            & "             )                                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                                                              " _
            & "        END AS SHUKABASHO,                                                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                                     " _
            & "               WHERE                                                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                       " _
            & "                 AND ZISSEKI      > 0                                                                                                        " _
            & "                 AND DELFLG       = '0'                                                                                                      " _
            & "             )                                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                                                               " _
            & "        END AS SHUKANAME,                                                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                                                              " _
            & "        CASE                                                                                                                                 " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022702'                                                                                             " _
            & "                THEN TANKA_SENBOKU.TANKA                                                                                                     " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022801'                                                                                             " _
            & "                THEN TANKA_HIMEZI.TANKA                                                                                                      " _
            & "        END                        AS TANKA,                                                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                                                           " _
            & "        NULL                       AS TSUKORYO,                                                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                                                          " _
            & "        CASE                                                                                                                                 " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022702'                                                                                             " _
            & "                THEN TANKA_SENBOKU.TANKA * ZISSEKI.ZISSEKI                                                                                   " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022801'                                                                                             " _
            & "                THEN TANKA_HIMEZI.TANKA * ZISSEKI.ZISSEKI                                                                                    " _
            & "        END AS YUSOUHI,                                                                                                                      " _
            & "        CALENDAR.WORKINGDAY AS WORKINGDAY,                                                                                                   " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                     " _
            & "        ZISSEKI.DELFLG AS DELFLG,                                                                                                            " _
            & "        @INITYMD                   AS INITYMD,                                                                                               " _
            & "        @INITUSER                  AS INITUSER,                                                                                              " _
            & "        @INITTERMID                AS INITTERMID,                                                                                            " _
            & "        @INITPGID                  AS INITPGID,                                                                                              " _
            & "        NULL                       AS UPDYMD,                                                                                                " _
            & "        NULL                       AS UPDUSER,                                                                                               " _
            & "        NULL                       AS UPDTERMID,                                                                                             " _
            & "        NULL                       AS UPDPGID,                                                                                               " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                                                                             " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA_SENBOKU                                                                                             " _
            & "        ON @TORICODE = TANKA_SENBOKU.TORICODE                                                                                                " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA_SENBOKU.ORGCODE                                                                                     " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA_SENBOKU.KASANORGCODE                                                                           " _
            & "        AND ZISSEKI.TODOKECODE = TANKA_SENBOKU.AVOCADOTODOKECODE                                                                             " _
            & "        AND ZISSEKI.SYABARA = TANKA_SENBOKU.SYABARA                                                                                          " _
            & "        AND TANKA_SENBOKU.STYMD  <= ZISSEKI.TODOKEDATE                                                                                       " _
            & "        AND TANKA_SENBOKU.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                       " _
            & "        AND TANKA_SENBOKU.DELFLG = @DELFLG                                                                                                   " _
            & "        AND TANKA_SENBOKU.ORGCODE = '022702'                                                                                                 " _
            & "        AND TANKA_SENBOKU.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                    " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA_HIMEZI                                                                                              " _
            & "        ON @TORICODE = TANKA_HIMEZI.TORICODE                                                                                                 " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA_HIMEZI.ORGCODE                                                                                      " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA_HIMEZI.KASANORGCODE                                                                            " _
            & "        AND ZISSEKI.TODOKECODE = TANKA_HIMEZI.AVOCADOTODOKECODE                                                                              " _
            & "        AND REPLACE(ZISSEKI.SYAGATA, '単車タンク', '単車') = TANKA_HIMEZI.SYAGATANAME                                                        " _
            & "        AND TANKA_HIMEZI.STYMD  <= ZISSEKI.TODOKEDATE                                                                                        " _
            & "        AND TANKA_HIMEZI.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                        " _
            & "        AND TANKA_HIMEZI.DELFLG = @DELFLG                                                                                                    " _
            & "        AND TANKA_HIMEZI.ORGCODE = '022801'                                                                                                  " _
            & "        AND TANKA_HIMEZI.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                     " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                                            " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                                            " _
            & "       AND (ZISSEKI.ORDERORGCODE = HOLIDAYRATE.ORDERORGCODE                                                                                  " _
            & "            OR (ZISSEKI.ORDERORGCODE = HOLIDAYRATE.ORDERORGCODE OR ZISSEKI.TODOKECODE = HOLIDAYRATE.TODOKECODE))                             " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                                   " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                                      " _
            & "    WHERE                                                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                                                             " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                                    " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                                   " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                                                           " _
            & "         DRP                       = VALUES(DRP),                                                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                                              " _
            & "         DELFLG                    = @DELFLG,                                                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(OG輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0022_OGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' シーエナジーエルネス輸送費テーブル更新
    ''' </summary>
    Private Sub CENALNESU_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(シーエナジーエルネス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0025_CENALNESUYUSOUHI                             " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(シーエナジーエルネス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0025_CENALNESUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(シーエナジーエルネス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0025_CENALNESUYUSOUHI(                                                                                                                              " _
            & "        RECONO,                                                                                                                                                            " _
            & "        LOADUNLOTYPE,                                                                                                                                                      " _
            & "        STACKINGTYPE,                                                                                                                                                      " _
            & "        ORDERORGCODE,                                                                                                                                                      " _
            & "        ORDERORGNAME,                                                                                                                                                      " _
            & "        KASANAMEORDERORG,                                                                                                                                                  " _
            & "        KASANCODEORDERORG,                                                                                                                                                 " _
            & "        ORDERORG,                                                                                                                                                          " _
            & "        PRODUCT2NAME,                                                                                                                                                      " _
            & "        PRODUCT2,                                                                                                                                                          " _
            & "        PRODUCT1NAME,                                                                                                                                                      " _
            & "        PRODUCT1,                                                                                                                                                          " _
            & "        OILNAME,                                                                                                                                                           " _
            & "        OILTYPE,                                                                                                                                                           " _
            & "        TODOKECODE,                                                                                                                                                        " _
            & "        TODOKENAME,                                                                                                                                                        " _
            & "        TODOKENAMES,                                                                                                                                                       " _
            & "        TORICODE,                                                                                                                                                          " _
            & "        TORINAME,                                                                                                                                                          " _
            & "        SHUKABASHO,                                                                                                                                                        " _
            & "        SHUKANAME,                                                                                                                                                         " _
            & "        SHUKANAMES,                                                                                                                                                        " _
            & "        SHUKATORICODE,                                                                                                                                                     " _
            & "        SHUKATORINAME,                                                                                                                                                     " _
            & "        SHUKADATE,                                                                                                                                                         " _
            & "        LOADTIME,                                                                                                                                                          " _
            & "        LOADTIMEIN,                                                                                                                                                        " _
            & "        TODOKEDATE,                                                                                                                                                        " _
            & "        SHITEITIME,                                                                                                                                                        " _
            & "        SHITEITIMEIN,                                                                                                                                                      " _
            & "        ZYUTYU,                                                                                                                                                            " _
            & "        ZISSEKI,                                                                                                                                                           " _
            & "        TANNI,                                                                                                                                                             " _
            & "        TANKNUM,                                                                                                                                                           " _
            & "        TANKNUMBER,                                                                                                                                                        " _
            & "        GYOMUTANKNUM,                                                                                                                                                      " _
            & "        SYAGATA,                                                                                                                                                           " _
            & "        SYABARA,                                                                                                                                                           " _
            & "        NINUSHINAME,                                                                                                                                                       " _
            & "        CONTYPE,                                                                                                                                                           " _
            & "        TRIP,                                                                                                                                                              " _
            & "        DRP,                                                                                                                                                               " _
            & "        STAFFSLCT,                                                                                                                                                         " _
            & "        STAFFNAME,                                                                                                                                                         " _
            & "        STAFFCODE,                                                                                                                                                         " _
            & "        SUBSTAFFSLCT,                                                                                                                                                      " _
            & "        SUBSTAFFNAME,                                                                                                                                                      " _
            & "        SUBSTAFFNUM,                                                                                                                                                       " _
            & "        SHUKODATE,                                                                                                                                                         " _
            & "        KIKODATE,                                                                                                                                                          " _
            & "        TANKA,                                                                                                                                                             " _
            & "        JURYORYOKIN,                                                                                                                                                       " _
            & "        TSUKORYO,                                                                                                                                                          " _
            & "        KYUZITUTANKA,                                                                                                                                                      " _
            & "        YUSOUHI,                                                                                                                                                           " _
            & "        WORKINGDAY,                                                                                                                                                        " _
            & "        PUBLICHOLIDAYNAME,                                                                                                                                                 " _
            & "        DELFLG,                                                                                                                                                            " _
            & "        INITYMD,                                                                                                                                                           " _
            & "        INITUSER,                                                                                                                                                          " _
            & "        INITTERMID,                                                                                                                                                        " _
            & "        INITPGID,                                                                                                                                                          " _
            & "        UPDYMD,                                                                                                                                                            " _
            & "        UPDUSER,                                                                                                                                                           " _
            & "        UPDTERMID,                                                                                                                                                         " _
            & "        UPDPGID,                                                                                                                                                           " _
            & "        RECEIVEYMD)                                                                                                                                                        " _
            & "    SELECT                                                                                                                                                                 " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                                                                                                           " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                                                                     " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                                                                                                                     " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                                                                                                                     " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                                                                 " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                                                                " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                                                                                                         " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                                                                                                         " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                                                                                                         " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                                                                                                          " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                                                                                                          " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                                                                                                                      " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                                                                                                         " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                                                                                                         " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                                                                                                        " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                                                                                                                    " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                                                                                                                    " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                                                                                                        " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                                                                                                         " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                                                                                                                     " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                                                                                                           " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                                                                                                          " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                                                                                                            " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                                                                                                          " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                                                                                                                       " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                                                                     " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                                                                                                          " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                                                                                                          " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                                                                                                                      " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                                                                                                          " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                                                                                                             " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                                                                                                              " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                                                                                                        " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                                                                                                        " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                                                                                                        " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                                                                     " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                                                                      " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                                                                                                        " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                                                                                                         " _
            & "        NULL                          AS TANKA,                                                                                                                            " _
            & "        ZISSEKIMAIN.JURYORYOKIN       AS JURYORYOKIN,                                                                                                                      " _
            & "        ZISSEKIMAIN.TSUKORYO          AS TSUKORYO,                                                                                                                         " _
            & "        ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                                                                                                                     " _
            & "        CASE                                                                                                                                                               " _
            & "            WHEN ZISSEKIMAIN.TORICODE = '0110600000' THEN COALESCE(ZISSEKIMAIN.JURYORYOKIN, 0) + COALESCE(ZISSEKIMAIN.TSUKORYO, 0) + COALESCE(ZISSEKIMAIN.KYUZITUTANKA, 0) " _
            & "            WHEN ZISSEKIMAIN.TORICODE = '0238900000' THEN COALESCE(ZISSEKIMAIN.JURYORYOKIN, 0) + COALESCE(ZISSEKIMAIN.KYUZITUTANKA, 0)                                     " _
            & "        END                           AS YUSOUHI,                                                                                                                          " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                                                                                                                       " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                                                " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                                                                                                           " _
            & "        @INITYMD                      AS INITYMD,                                                                                                                          " _
            & "        @INITUSER                     AS INITUSER,                                                                                                                         " _
            & "        @INITTERMID                   AS INITTERMID,                                                                                                                       " _
            & "        @INITPGID                     AS INITPGID,                                                                                                                         " _
            & "        NULL                          AS UPDYMD,                                                                                                                           " _
            & "        NULL                          AS UPDUSER,                                                                                                                          " _
            & "        NULL                          AS UPDTERMID,                                                                                                                        " _
            & "        NULL                          AS UPDPGID,                                                                                                                          " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                                                                                        " _
            & "    FROM(                                                                                                                                                                  " _
            & "         SELECT                                                                                                                                                            " _
            & "             ZISSEKI.RECONO             AS RECONO,                                                                                                                         " _
            & "             ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                                                                                   " _
            & "             ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                                                                                   " _
            & "             ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                                                                                   " _
            & "             ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                                                                                   " _
            & "             ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                                                                               " _
            & "             ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                                                                              " _
            & "             ZISSEKI.ORDERORG           AS ORDERORG,                                                                                                                       " _
            & "             ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                                                                                   " _
            & "             ZISSEKI.PRODUCT2           AS PRODUCT2,                                                                                                                       " _
            & "             ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                                                                                   " _
            & "             ZISSEKI.PRODUCT1           AS PRODUCT1,                                                                                                                       " _
            & "             ZISSEKI.OILNAME            AS OILNAME,                                                                                                                        " _
            & "             ZISSEKI.OILTYPE            AS OILTYPE,                                                                                                                        " _
            & "             ZISSEKI.TODOKECODE         AS TODOKECODE,                                                                                                                     " _
            & "             ZISSEKI.TODOKENAME         AS TODOKENAME,                                                                                                                     " _
            & "             ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                                                                                    " _
            & "             ZISSEKI.TORICODE           AS TORICODE,                                                                                                                       " _
            & "             ZISSEKI.TORINAME           AS TORINAME,                                                                                                                       " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                                         " _
            & "             THEN (SELECT SHUKABASHO                                                                                                                                       " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                                                                                                              " _
            & "                     WHERE                                                                                                                                                 " _
            & "                         TORICODE     = ZISSEKI.TORICODE                                                                                                                   " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                                                                                                                   " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                                               " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                                                                                                                    " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                                                 " _
            & "                     AND ZISSEKI      > 0                                                                                                                                  " _
            & "                     AND DELFLG       = '0'                                                                                                                                " _
            & "                 )                                                                                                                                                         " _
            & "             ELSE ZISSEKI.SHUKABASHO                                                                                                                                       " _
            & "             END AS SHUKABASHO,                                                                                                                                            " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                                         " _
            & "             THEN (SELECT SHUKANAME                                                                                                                                        " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                                                                                                              " _
            & "                     WHERE                                                                                                                                                 " _
            & "                         TORICODE     = ZISSEKI.TORICODE                                                                                                                   " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                                                                                                                   " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                                               " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                                                                                                                    " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                                                 " _
            & "                     AND ZISSEKI      > 0                                                                                                                                  " _
            & "                     AND DELFLG       = '0'                                                                                                                                " _
            & "                 )                                                                                                                                                         " _
            & "             ELSE ZISSEKI.SHUKANAME                                                                                                                                        " _
            & "             END AS SHUKANAME,                                                                                                                                             " _
            & "             ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                                                                                     " _
            & "             ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                                                                                  " _
            & "             ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                                                                                  " _
            & "             ZISSEKI.SHUKADATE          AS SHUKADATE,                                                                                                                      " _
            & "             ZISSEKI.LOADTIME           AS LOADTIME,                                                                                                                       " _
            & "             ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                                                                                     " _
            & "             ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                                                                                     " _
            & "             ZISSEKI.SHITEITIME         AS SHITEITIME,                                                                                                                     " _
            & "             ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                                                                                   " _
            & "             ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                                                                         " _
            & "             ZISSEKI.ZISSEKI            AS ZISSEKI,                                                                                                                        " _
            & "             ZISSEKI.TANNI              AS TANNI,                                                                                                                          " _
            & "             ZISSEKI.TANKNUM            AS TANKNUM,                                                                                                                        " _
            & "             ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                                                                                     " _
            & "             ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                                                                                   " _
            & "             ZISSEKI.SYAGATA            AS SYAGATA,                                                                                                                        " _
            & "             ZISSEKI.SYABARA            AS SYABARA,                                                                                                                        " _
            & "             ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                                                                                    " _
            & "             ZISSEKI.CONTYPE            AS CONTYPE,                                                                                                                        " _
            & "             ZISSEKI.TRIP               AS TRIP,                                                                                                                           " _
            & "             ZISSEKI.DRP                AS DRP,                                                                                                                            " _
            & "             ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                                                                                      " _
            & "             ZISSEKI.STAFFNAME          AS STAFFNAME,                                                                                                                      " _
            & "             ZISSEKI.STAFFCODE          AS STAFFCODE,                                                                                                                      " _
            & "             ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                                                                                   " _
            & "             ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                                                                                   " _
            & "             ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                                                                                    " _
            & "             ZISSEKI.SHUKODATE          AS SHUKODATE,                                                                                                                      " _
            & "             ZISSEKI.KIKODATE           AS KIKODATE,                                                                                                                       " _
            & "             CASE                                                                                                                                                          " _
            & "                 WHEN ZISSEKI.TORICODE = '0110600000' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                                                         " _
            & "                 WHEN ZISSEKI.TORICODE = '0238900000' THEN COALESCE(TANKA.TANKA, 0)                                                                                        " _
            & "             END                        AS JURYORYOKIN,                                                                                                                    " _
            & "             CASE                                                                                                                                                          " _
            & "                 WHEN ZISSEKI.TORICODE = '0110600000' THEN COALESCE(TANKA.TOLLFEE, 0)                                                                                      " _
            & "                 WHEN ZISSEKI.TORICODE = '0238900000' THEN 0                                                                                                               " _
            & "             END                        AS TSUKORYO,                                                                                                                       " _
            & "             HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                                                                                   " _
            & "             CALENDAR.WORKINGDAY        AS WORKINGDAY,                                                                                                                     " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                                              " _
            & "             ZISSEKI.DELFLG             AS DELFLG                                                                                                                          " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                                                                  " _
            & "          LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                                                                             " _
            & "              ON @TORICODE = TANKA.TORICODE                                                                                                                                " _
            & "              AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                                                                     " _
            & "              AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                                                                           " _
            & "              AND ZISSEKI.SHUKABASHO = TANKA.AVOCADOSHUKABASHO                                                                                                             " _
            & "              AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                                                                             " _
            & "              AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                                                                                      " _
            & "              AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                                                                       " _
            & "              AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                                                       " _
            & "              AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                                                    " _
            & "              AND TANKA.DELFLG = @DELFLG                                                                                                                                   " _
            & "          LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                                                          " _
            & "             ON ZISSEKI.TORICODE = CALENDAR.TORICODE                                                                                                                       " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                                                                         " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                                                                                                                 " _
            & "          LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                                                                    " _
            & "             ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                                                                    " _
            & "             AND ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                                                                                      " _
            & "             AND ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                                                                        " _
            & "             AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                                                           " _
            & "             AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                                                              " _
            & "         WHERE                                                                                                                                                             " _
            & "             ZISSEKI.TORICODE = @TORICODE                                                                                                                                  " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                                                                                                                      " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                                                            " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                                                           " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                                                             " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                                                                                                                  " _
            & "    ) ZISSEKIMAIN                                                                                                                                                          " _
            & " ON DUPLICATE KEY UPDATE                                                                                                                                                   " _
            & "         RECONO                    = VALUES(RECONO),                                                                                                                       " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                                                                 " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                                                                 " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                                                                 " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                                                                 " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                                                                             " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                                                                            " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                                                                     " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                                                                 " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                                                                     " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                                                                 " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                                                                     " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                                                                      " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                                                                      " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                                                                   " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                                                                   " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                                                                  " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                                                                     " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                                                                     " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                                                                   " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                                                                    " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                                                                   " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                                                                " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                                                                " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                                                                    " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                                                                     " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                                                                   " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                                                                   " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                                                                   " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                                                                 " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                                                                       " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                                                                      " _
            & "         TANNI                     = VALUES(TANNI),                                                                                                                        " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                                                                      " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                                                                   " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                                                                 " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                                                                      " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                                                                      " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                                                                  " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                                                                      " _
            & "         TRIP                      = VALUES(TRIP),                                                                                                                         " _
            & "         DRP                       = VALUES(DRP),                                                                                                                          " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                                                                    " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                                                                    " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                                                                    " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                                                                 " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                                                                 " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                                                                  " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                                                                    " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                                                                     " _
            & "         TANKA                     = VALUES(TANKA),                                                                                                                        " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                                                                  " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                                                                     " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                                                                 " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                                                                      " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                                                                   " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                                                                            " _
            & "         DELFLG                    = @DELFLG,                                                                                                                              " _
            & "         UPDYMD                    = @UPDYMD,                                                                                                                              " _
            & "         UPDUSER                   = @UPDUSER,                                                                                                                             " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                                                                           " _
            & "         UPDPGID                   = @UPDPGID,                                                                                                                             " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                                                                          "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(シーエナジーエルネス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0025_CENALNESUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 石油資源開発(北海道)輸送費テーブル更新
    ''' </summary>
    Private Sub SEKIYUHOKKAIDO_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(石油資源開発(北海道)輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0023_SEKIYUHOKKAIDOYUSOUHI                        " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(北海道)輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0023_SEKIYUHOKKAIDOYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(石油資源開発(北海道)輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0023_SEKIYUHOKKAIDOYUSOUHI(                                 " _
            & "        RECONO,                                                                    " _
            & "        LOADUNLOTYPE,                                                              " _
            & "        STACKINGTYPE,                                                              " _
            & "        ORDERORGCODE,                                                              " _
            & "        ORDERORGNAME,                                                              " _
            & "        KASANAMEORDERORG,                                                          " _
            & "        KASANCODEORDERORG,                                                         " _
            & "        ORDERORG,                                                                  " _
            & "        PRODUCT2NAME,                                                              " _
            & "        PRODUCT2,                                                                  " _
            & "        PRODUCT1NAME,                                                              " _
            & "        PRODUCT1,                                                                  " _
            & "        OILNAME,                                                                   " _
            & "        OILTYPE,                                                                   " _
            & "        TODOKECODE,                                                                " _
            & "        TODOKENAME,                                                                " _
            & "        TODOKENAMES,                                                               " _
            & "        TORICODE,                                                                  " _
            & "        TORINAME,                                                                  " _
            & "        SHUKABASHO,                                                                " _
            & "        SHUKANAME,                                                                 " _
            & "        SHUKANAMES,                                                                " _
            & "        SHUKATORICODE,                                                             " _
            & "        SHUKATORINAME,                                                             " _
            & "        SHUKADATE,                                                                 " _
            & "        LOADTIME,                                                                  " _
            & "        LOADTIMEIN,                                                                " _
            & "        TODOKEDATE,                                                                " _
            & "        SHITEITIME,                                                                " _
            & "        SHITEITIMEIN,                                                              " _
            & "        ZYUTYU,                                                                    " _
            & "        ZISSEKI,                                                                   " _
            & "        TANNI,                                                                     " _
            & "        TANKNUM,                                                                   " _
            & "        TANKNUMBER,                                                                " _
            & "        GYOMUTANKNUM,                                                              " _
            & "        SYAGATA,                                                                   " _
            & "        SYABARA,                                                                   " _
            & "        NINUSHINAME,                                                               " _
            & "        CONTYPE,                                                                   " _
            & "        TRIP,                                                                      " _
            & "        DRP,                                                                       " _
            & "        STAFFSLCT,                                                                 " _
            & "        STAFFNAME,                                                                 " _
            & "        STAFFCODE,                                                                 " _
            & "        SUBSTAFFSLCT,                                                              " _
            & "        SUBSTAFFNAME,                                                              " _
            & "        SUBSTAFFNUM,                                                               " _
            & "        SHUKODATE,                                                                 " _
            & "        KIKODATE,                                                                  " _
            & "        TANKA,                                                                     " _
            & "        JURYORYOKIN,                                                               " _
            & "        TSUKORYO,                                                                  " _
            & "        KYUZITUTANKA,                                                              " _
            & "        YUSOUHI,                                                                   " _
            & "        WORKINGDAY,                                                                " _
            & "        PUBLICHOLIDAYNAME,                                                         " _
            & "        DELFLG,                                                                    " _
            & "        INITYMD,                                                                   " _
            & "        INITUSER,                                                                  " _
            & "        INITTERMID,                                                                " _
            & "        INITPGID,                                                                  " _
            & "        UPDYMD,                                                                    " _
            & "        UPDUSER,                                                                   " _
            & "        UPDTERMID,                                                                 " _
            & "        UPDPGID,                                                                   " _
            & "        RECEIVEYMD)                                                                " _
            & "    SELECT                                                                         " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                   " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                             " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                             " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                             " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                             " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                         " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                        " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                 " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                 " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                 " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                  " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                  " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                               " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                               " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                              " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                 " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                 " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                               " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                               " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                            " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                            " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                 " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                               " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                               " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                               " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                             " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                   " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                  " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                    " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                  " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                               " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                             " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                  " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                  " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                              " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                  " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                     " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                      " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                              " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                 " _
            & "        ZISSEKIMAIN.TANKA             AS TANKA,                                    " _
            & "        NULL                          AS JURYORYOKIN,                              " _
            & "        NULL                          AS TSUKORYO,                                 " _
            & "        ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                             " _
            & "        ZISSEKIMAIN.YUSOUHI + COALESCE(ZISSEKIMAIN.KYUZITUTANKA, 0) AS YUSOUHI,    " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                               " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                        " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                   " _
            & "        @INITYMD                      AS INITYMD,                                  " _
            & "        @INITUSER                     AS INITUSER,                                 " _
            & "        @INITTERMID                   AS INITTERMID,                               " _
            & "        @INITPGID                     AS INITPGID,                                 " _
            & "        NULL                          AS UPDYMD,                                   " _
            & "        NULL                          AS UPDUSER,                                  " _
            & "        NULL                          AS UPDTERMID,                                " _
            & "        NULL                          AS UPDPGID,                                  " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                " _
            & "    FROM(                                                                          " _
            & "         SELECT                                                                    " _
            & "             ZISSEKI.RECONO            AS RECONO,                                  " _
            & "             ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "             ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "             ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "             ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "             ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "             ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "             ZISSEKI.ORDERORG          AS ORDERORG,                                " _
            & "             ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "             ZISSEKI.PRODUCT2          AS PRODUCT2,                                " _
            & "             ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "             ZISSEKI.PRODUCT1          AS PRODUCT1,                                " _
            & "             ZISSEKI.OILNAME           AS OILNAME,                                 " _
            & "             ZISSEKI.OILTYPE           AS OILTYPE,                                 " _
            & "             ZISSEKI.TODOKECODE        AS TODOKECODE,                              " _
            & "             ZISSEKI.TODOKENAME        AS TODOKENAME,                              " _
            & "             ZISSEKI.TODOKENAMES       AS TODOKENAMES,                             " _
            & "             ZISSEKI.TORICODE          AS TORICODE,                                " _
            & "             ZISSEKI.TORINAME          AS TORINAME,                                " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKABASHO                                               " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND ZISSEKI      > 0                                          " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKABASHO                                               " _
            & "             END AS SHUKABASHO,                                                    " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKANAME                                                " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND ZISSEKI      > 0                                          " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKANAME                                                " _
            & "             END AS SHUKANAME,                                                     " _
            & "             ZISSEKI.SHUKANAMES        AS SHUKANAMES,                              " _
            & "             ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "             ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "             ZISSEKI.SHUKADATE         AS SHUKADATE,                               " _
            & "             ZISSEKI.LOADTIME          AS LOADTIME,                                " _
            & "             ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "             ZISSEKI.TODOKEDATE        AS TODOKEDATE,                              " _
            & "             ZISSEKI.SHITEITIME        AS SHITEITIME,                              " _
            & "             ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "             ZISSEKI.ZYUTYU            AS ZYUTYU,                                  " _
            & "             ZISSEKI.ZISSEKI           AS ZISSEKI,                                 " _
            & "             ZISSEKI.TANNI             AS TANNI,                                   " _
            & "             ZISSEKI.TANKNUM           AS TANKNUM,                                 " _
            & "             ZISSEKI.TANKNUMBER        AS TANKNUMBER,                              " _
            & "             ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "             ZISSEKI.SYAGATA           AS SYAGATA,                                 " _
            & "             ZISSEKI.SYABARA           AS SYABARA,                                 " _
            & "             ZISSEKI.NINUSHINAME       AS NINUSHINAME,                             " _
            & "             ZISSEKI.CONTYPE           AS CONTYPE,                                 " _
            & "             ZISSEKI.TRIP              AS TRIP,                                    " _
            & "             ZISSEKI.DRP               AS DRP,                                     " _
            & "             ZISSEKI.STAFFSLCT         AS STAFFSLCT,                               " _
            & "             ZISSEKI.STAFFNAME         AS STAFFNAME,                               " _
            & "             ZISSEKI.STAFFCODE         AS STAFFCODE,                               " _
            & "             ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "             ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "             ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "             ZISSEKI.SHUKODATE         AS SHUKODATE,                               " _
            & "             ZISSEKI.KIKODATE          AS KIKODATE,                                " _
            & "             CASE WHEN HOLIDAYRATE.SHUKABASHOCATEGORY = '1'                        " _
            & "                       AND HOLIDAYRATE.TODOKECATEGORY = '1' THEN HOLIDAYRATE.TANKA " _
            & "                  ELSE NULL                                                        " _
            & "             END                       AS KYUZITUTANKA,                            " _
            & "             TANKA.TANKA               AS TANKA,                                   " _
            & "             TANKA.TANKA * ZISSEKI.ZISSEKI AS YUSOUHI,                             " _
            & "             CALENDAR.WORKINGDAY       AS WORKINGDAY,                              " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                      " _
            & "             ZISSEKI.DELFLG            AS DELFLG                                   " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                          " _
            & "         LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                      " _
            & "             ON @TORICODE = TANKA.TORICODE                                         " _
            & "             AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                              " _
            & "             AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                    " _
            & "             AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                      " _
            & "             AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.DELFLG = @DELFLG                                            " _
            & "             AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                             " _
            & "         LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                   " _
            & "             ON @TORICODE = CALENDAR.TORICODE                                      " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                 " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                         " _
            & "         LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                             " _
            & "            ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                             " _
            & "            AND ZISSEKI.ORDERORGCODE = HOLIDAYRATE.ORDERORGCODE                    " _
            & "            AND ZISSEKI.SHUKABASHO = HOLIDAYRATE.SHUKABASHO                        " _
            & "            AND ZISSEKI.TODOKECODE = HOLIDAYRATE.TODOKECODE                        " _
            & "            AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')    " _
            & "            AND HOLIDAYRATE.DELFLG = @DELFLG                                       " _
            & "         WHERE                                                                     " _
            & "             ZISSEKI.TORICODE = @TORICODE                                          " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                              " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                   " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                     " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                    " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                          " _
            & "             AND ZISSEKI.ORDERORGCODE = '020104'                                   " _
            & "         ) ZISSEKIMAIN                                                             " _
            & " ON DUPLICATE KEY UPDATE                                                           " _
            & "         RECONO                    = VALUES(RECONO),                               " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                         " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                         " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                         " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                         " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                     " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                    " _
            & "         ORDERORG                  = VALUES(ORDERORG),                             " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                         " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                             " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                         " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                             " _
            & "         OILNAME                   = VALUES(OILNAME),                              " _
            & "         OILTYPE                   = VALUES(OILTYPE),                              " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                           " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                           " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                          " _
            & "         TORICODE                  = VALUES(TORICODE),                             " _
            & "         TORINAME                  = VALUES(TORINAME),                             " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                           " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                            " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                           " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                        " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                        " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                            " _
            & "         LOADTIME                  = VALUES(LOADTIME),                             " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                           " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                           " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                           " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                         " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                               " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                              " _
            & "         TANNI                     = VALUES(TANNI),                                " _
            & "         TANKNUM                   = VALUES(TANKNUM),                              " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                           " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                         " _
            & "         SYAGATA                   = VALUES(SYAGATA),                              " _
            & "         SYABARA                   = VALUES(SYABARA),                              " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                          " _
            & "         CONTYPE                   = VALUES(CONTYPE),                              " _
            & "         TRIP                      = VALUES(TRIP),                                 " _
            & "         DRP                       = VALUES(DRP),                                  " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                            " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                            " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                            " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                         " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                         " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                          " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                            " _
            & "         KIKODATE                  = VALUES(KIKODATE),                             " _
            & "         TANKA                     = VALUES(TANKA),                                " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                          " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                             " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                         " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                              " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                           " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                    " _
            & "         DELFLG                    = @DELFLG,                                      " _
            & "         UPDYMD                    = @UPDYMD,                                      " _
            & "         UPDUSER                   = @UPDUSER,                                     " _
            & "         UPDTERMID                 = @UPDTERMID,                                   " _
            & "         UPDPGID                   = @UPDPGID,                                     " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                  "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(北海道)輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0023_SEKIYUHOKKAIDOYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 石油資源開発(本州分)輸送費テーブル更新
    ''' </summary>
    Private Sub SEKIYUHONSYU_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(石油資源開発(本州分)輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0021_SEKIYUHONSYUYUSOUHI                          " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(本州分)輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0021_SEKIYUHONSYUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(石油資源開発(本州分)輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0021_SEKIYUHONSYUYUSOUHI(                                   " _
            & "        RECONO,                                                                    " _
            & "        LOADUNLOTYPE,                                                              " _
            & "        STACKINGTYPE,                                                              " _
            & "        ORDERORGCODE,                                                              " _
            & "        ORDERORGNAME,                                                              " _
            & "        KASANAMEORDERORG,                                                          " _
            & "        KASANCODEORDERORG,                                                         " _
            & "        ORDERORG,                                                                  " _
            & "        PRODUCT2NAME,                                                              " _
            & "        PRODUCT2,                                                                  " _
            & "        PRODUCT1NAME,                                                              " _
            & "        PRODUCT1,                                                                  " _
            & "        OILNAME,                                                                   " _
            & "        OILTYPE,                                                                   " _
            & "        TODOKECODE,                                                                " _
            & "        TODOKENAME,                                                                " _
            & "        TODOKENAMES,                                                               " _
            & "        TORICODE,                                                                  " _
            & "        TORINAME,                                                                  " _
            & "        SHUKABASHO,                                                                " _
            & "        SHUKANAME,                                                                 " _
            & "        SHUKANAMES,                                                                " _
            & "        SHUKATORICODE,                                                             " _
            & "        SHUKATORINAME,                                                             " _
            & "        SHUKADATE,                                                                 " _
            & "        LOADTIME,                                                                  " _
            & "        LOADTIMEIN,                                                                " _
            & "        TODOKEDATE,                                                                " _
            & "        SHITEITIME,                                                                " _
            & "        SHITEITIMEIN,                                                              " _
            & "        ZYUTYU,                                                                    " _
            & "        ZISSEKI,                                                                   " _
            & "        TANNI,                                                                     " _
            & "        TANKNUM,                                                                   " _
            & "        TANKNUMBER,                                                                " _
            & "        GYOMUTANKNUM,                                                              " _
            & "        SYAGATA,                                                                   " _
            & "        SYABARA,                                                                   " _
            & "        NINUSHINAME,                                                               " _
            & "        CONTYPE,                                                                   " _
            & "        TRIP,                                                                      " _
            & "        DRP,                                                                       " _
            & "        STAFFSLCT,                                                                 " _
            & "        STAFFNAME,                                                                 " _
            & "        STAFFCODE,                                                                 " _
            & "        SUBSTAFFSLCT,                                                              " _
            & "        SUBSTAFFNAME,                                                              " _
            & "        SUBSTAFFNUM,                                                               " _
            & "        SHUKODATE,                                                                 " _
            & "        KIKODATE,                                                                  " _
            & "        TANKA,                                                                     " _
            & "        JURYORYOKIN,                                                               " _
            & "        TSUKORYO,                                                                  " _
            & "        KYUZITUTANKA,                                                              " _
            & "        YUSOUHI,                                                                   " _
            & "        WORKINGDAY,                                                                " _
            & "        PUBLICHOLIDAYNAME,                                                         " _
            & "        DELFLG,                                                                    " _
            & "        INITYMD,                                                                   " _
            & "        INITUSER,                                                                  " _
            & "        INITTERMID,                                                                " _
            & "        INITPGID,                                                                  " _
            & "        UPDYMD,                                                                    " _
            & "        UPDUSER,                                                                   " _
            & "        UPDTERMID,                                                                 " _
            & "        UPDPGID,                                                                   " _
            & "        RECEIVEYMD)                                                                " _
            & "    SELECT                                                                         " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                   " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                             " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                             " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                             " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                             " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                         " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                        " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                 " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                 " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                 " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                  " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                  " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                               " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                               " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                              " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                 " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                 " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                               " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                               " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                            " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                            " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                 " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                               " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                               " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                               " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                             " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                   " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                  " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                    " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                  " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                               " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                             " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                  " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                  " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                              " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                  " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                     " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                      " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                              " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                 " _
            & "        ZISSEKIMAIN.TANKA             AS TANKA,                                    " _
            & "        NULL                          AS JURYORYOKIN,                              " _
            & "        NULL                          AS TSUKORYO,                                 " _
            & "        ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                             " _
            & "        ZISSEKIMAIN.YUSOUHI + COALESCE(ZISSEKIMAIN.KYUZITUTANKA, 0) AS YUSOUHI,    " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                               " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                        " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                   " _
            & "        @INITYMD                      AS INITYMD,                                  " _
            & "        @INITUSER                     AS INITUSER,                                 " _
            & "        @INITTERMID                   AS INITTERMID,                               " _
            & "        @INITPGID                     AS INITPGID,                                 " _
            & "        NULL                          AS UPDYMD,                                   " _
            & "        NULL                          AS UPDUSER,                                  " _
            & "        NULL                          AS UPDTERMID,                                " _
            & "        NULL                          AS UPDPGID,                                  " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                " _
            & "    FROM(                                                                          " _
            & "         SELECT                                                                    " _
            & "             ZISSEKI.RECONO            AS RECONO,                                  " _
            & "             ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "             ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "             ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "             ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "             ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "             ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "             ZISSEKI.ORDERORG          AS ORDERORG,                                " _
            & "             ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "             ZISSEKI.PRODUCT2          AS PRODUCT2,                                " _
            & "             ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "             ZISSEKI.PRODUCT1          AS PRODUCT1,                                " _
            & "             ZISSEKI.OILNAME           AS OILNAME,                                 " _
            & "             ZISSEKI.OILTYPE           AS OILTYPE,                                 " _
            & "             ZISSEKI.TODOKECODE        AS TODOKECODE,                              " _
            & "             ZISSEKI.TODOKENAME        AS TODOKENAME,                              " _
            & "             ZISSEKI.TODOKENAMES       AS TODOKENAMES,                             " _
            & "             ZISSEKI.TORICODE          AS TORICODE,                                " _
            & "             ZISSEKI.TORINAME          AS TORINAME,                                " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKABASHO                                               " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND ZISSEKI      > 0                                          " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKABASHO                                               " _
            & "             END AS SHUKABASHO,                                                    " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKANAME                                                " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND ZISSEKI      > 0                                          " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKANAME                                                " _
            & "             END AS SHUKANAME,                                                     " _
            & "             ZISSEKI.SHUKANAMES        AS SHUKANAMES,                              " _
            & "             ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "             ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "             ZISSEKI.SHUKADATE         AS SHUKADATE,                               " _
            & "             ZISSEKI.LOADTIME          AS LOADTIME,                                " _
            & "             ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "             ZISSEKI.TODOKEDATE        AS TODOKEDATE,                              " _
            & "             ZISSEKI.SHITEITIME        AS SHITEITIME,                              " _
            & "             ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "             ZISSEKI.ZYUTYU            AS ZYUTYU,                                  " _
            & "             ZISSEKI.ZISSEKI           AS ZISSEKI,                                 " _
            & "             ZISSEKI.TANNI             AS TANNI,                                   " _
            & "             ZISSEKI.TANKNUM           AS TANKNUM,                                 " _
            & "             ZISSEKI.TANKNUMBER        AS TANKNUMBER,                              " _
            & "             ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "             ZISSEKI.SYAGATA           AS SYAGATA,                                 " _
            & "             ZISSEKI.SYABARA           AS SYABARA,                                 " _
            & "             ZISSEKI.NINUSHINAME       AS NINUSHINAME,                             " _
            & "             ZISSEKI.CONTYPE           AS CONTYPE,                                 " _
            & "             ZISSEKI.TRIP              AS TRIP,                                    " _
            & "             ZISSEKI.DRP               AS DRP,                                     " _
            & "             ZISSEKI.STAFFSLCT         AS STAFFSLCT,                               " _
            & "             ZISSEKI.STAFFNAME         AS STAFFNAME,                               " _
            & "             ZISSEKI.STAFFCODE         AS STAFFCODE,                               " _
            & "             ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "             ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "             ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "             ZISSEKI.SHUKODATE         AS SHUKODATE,                               " _
            & "             ZISSEKI.KIKODATE          AS KIKODATE,                                " _
            & "             CASE WHEN ZISSEKI.TODOKECODE = HOLIDAYRATE.TODOKECODE                 " _
            & "                       AND HOLIDAYRATE.TODOKECATEGORY = '2' THEN NULL              " _
            & "                  ELSE HOLIDAYRATE.TANKA                                           " _
            & "             END                       AS KYUZITUTANKA,                            " _
            & "             TANKA.TANKA               AS TANKA,                                   " _
            & "             TANKA.TANKA * ZISSEKI.ZISSEKI AS YUSOUHI,                             " _
            & "             CALENDAR.WORKINGDAY AS WORKINGDAY,                                    " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                      " _
            & "             ZISSEKI.DELFLG AS DELFLG                                              " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                          " _
            & "         LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                      " _
            & "             ON @TORICODE = TANKA.TORICODE                                         " _
            & "             AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                              " _
            & "             AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                    " _
            & "             AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                      " _
            & "             AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                               " _
            & "             AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.DELFLG = @DELFLG                                            " _
            & "             AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                             " _
            & "         LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                   " _
            & "             ON @TORICODE = CALENDAR.TORICODE                                      " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                 " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                         " _
            & "          LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                            " _
            & "             ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                            " _
            & "             AND ZISSEKI.ORDERORGCODE = HOLIDAYRATE.ORDERORGCODE                   " _
            & "             AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')   " _
            & "             AND HOLIDAYRATE.DELFLG = @DELFLG                                      " _
            & "         WHERE                                                                     " _
            & "             ZISSEKI.TORICODE = @TORICODE                                          " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                              " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                   " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                     " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                    " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                          " _
            & "             AND ZISSEKI.ORDERORGCODE <> '020104'                                  " _
            & "         ) ZISSEKIMAIN                                                             " _
            & " ON DUPLICATE KEY UPDATE                                                           " _
            & "         RECONO                    = VALUES(RECONO),                               " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                         " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                         " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                         " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                         " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                     " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                    " _
            & "         ORDERORG                  = VALUES(ORDERORG),                             " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                         " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                             " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                         " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                             " _
            & "         OILNAME                   = VALUES(OILNAME),                              " _
            & "         OILTYPE                   = VALUES(OILTYPE),                              " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                           " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                           " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                          " _
            & "         TORICODE                  = VALUES(TORICODE),                             " _
            & "         TORINAME                  = VALUES(TORINAME),                             " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                           " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                            " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                           " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                        " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                        " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                            " _
            & "         LOADTIME                  = VALUES(LOADTIME),                             " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                           " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                           " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                           " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                         " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                               " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                              " _
            & "         TANNI                     = VALUES(TANNI),                                " _
            & "         TANKNUM                   = VALUES(TANKNUM),                              " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                           " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                         " _
            & "         SYAGATA                   = VALUES(SYAGATA),                              " _
            & "         SYABARA                   = VALUES(SYABARA),                              " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                          " _
            & "         CONTYPE                   = VALUES(CONTYPE),                              " _
            & "         TRIP                      = VALUES(TRIP),                                 " _
            & "         DRP                       = VALUES(DRP),                                  " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                            " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                            " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                            " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                         " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                         " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                          " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                            " _
            & "         KIKODATE                  = VALUES(KIKODATE),                             " _
            & "         TANKA                     = VALUES(TANKA),                                " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                          " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                             " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                         " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                              " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                           " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                    " _
            & "         DELFLG                    = @DELFLG,                                      " _
            & "         UPDYMD                    = @UPDYMD,                                      " _
            & "         UPDUSER                   = @UPDUSER,                                     " _
            & "         UPDTERMID                 = @UPDTERMID,                                   " _
            & "         UPDPGID                   = @UPDPGID,                                     " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                  "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(本州分)輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0021_SEKIYUHONSYUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東京ガス輸送費テーブル更新
    ''' </summary>
    Private Sub TOKYOGUS_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東京ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0027_TOKYOGUSYUSOUHI                              " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東京ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0027_TOKYOGUSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(東京ガス輸送費テーブル)
            SQLStr =
              " INSERT INTO LNG.LNT0027_TOKYOGUSYUSOUHI(                                      " _
            & "     RECONO,                                                                   " _
            & "     LOADUNLOTYPE,                                                             " _
            & "     STACKINGTYPE,                                                             " _
            & "     ORDERORGCODE,                                                             " _
            & "     ORDERORGNAME,                                                             " _
            & "     KASANAMEORDERORG,                                                         " _
            & "     KASANCODEORDERORG,                                                        " _
            & "     ORDERORG,                                                                 " _
            & "     PRODUCT2NAME,                                                             " _
            & "     PRODUCT2,                                                                 " _
            & "     PRODUCT1NAME,                                                             " _
            & "     PRODUCT1,                                                                 " _
            & "     OILNAME,                                                                  " _
            & "     OILTYPE,                                                                  " _
            & "     TODOKECODE,                                                               " _
            & "     TODOKENAME,                                                               " _
            & "     TODOKENAMES,                                                              " _
            & "     TORICODE,                                                                 " _
            & "     TORINAME,                                                                 " _
            & "     SHUKABASHO,                                                               " _
            & "     SHUKANAME,                                                                " _
            & "     SHUKANAMES,                                                               " _
            & "     SHUKATORICODE,                                                            " _
            & "     SHUKATORINAME,                                                            " _
            & "     SHUKADATE,                                                                " _
            & "     LOADTIME,                                                                 " _
            & "     LOADTIMEIN,                                                               " _
            & "     TODOKEDATE,                                                               " _
            & "     SHITEITIME,                                                               " _
            & "     SHITEITIMEIN,                                                             " _
            & "     ZYUTYU,                                                                   " _
            & "     ZISSEKI,                                                                  " _
            & "     TANNI,                                                                    " _
            & "     TANKNUM,                                                                  " _
            & "     TANKNUMBER,                                                               " _
            & "     GYOMUTANKNUM,                                                             " _
            & "     SYAGATA,                                                                  " _
            & "     SYABARA,                                                                  " _
            & "     NINUSHINAME,                                                              " _
            & "     CONTYPE,                                                                  " _
            & "     TRIP,                                                                     " _
            & "     DRP,                                                                      " _
            & "     STAFFSLCT,                                                                " _
            & "     STAFFNAME,                                                                " _
            & "     STAFFCODE,                                                                " _
            & "     SUBSTAFFSLCT,                                                             " _
            & "     SUBSTAFFNAME,                                                             " _
            & "     SUBSTAFFNUM,                                                              " _
            & "     SHUKODATE,                                                                " _
            & "     KIKODATE,                                                                 " _
            & "     TANKA,                                                                    " _
            & "     JURYORYOKIN,                                                              " _
            & "     TSUKORYO,                                                                 " _
            & "     KYUZITUTANKA,                                                             " _
            & "     YUSOUHI,                                                                  " _
            & "     WORKINGDAY,                                                               " _
            & "     PUBLICHOLIDAYNAME,                                                        " _
            & "     DELFLG,                                                                   " _
            & "     INITYMD,                                                                  " _
            & "     INITUSER,                                                                 " _
            & "     INITTERMID,                                                               " _
            & "     INITPGID,                                                                 " _
            & "     UPDYMD,                                                                   " _
            & "     UPDUSER,                                                                  " _
            & "     UPDTERMID,                                                                " _
            & "     UPDPGID,                                                                  " _
            & "     RECEIVEYMD)                                                               " _
            & " SELECT                                                                        " _
            & "     ZISSEKIMAIN.RECONO            AS RECONO,                                  " _
            & "     ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "     ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "     ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "     ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "     ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "     ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "     ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                " _
            & "     ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "     ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                " _
            & "     ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "     ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                " _
            & "     ZISSEKIMAIN.OILNAME           AS OILNAME,                                 " _
            & "     ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                 " _
            & "     ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                              " _
            & "     ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                              " _
            & "     ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                             " _
            & "     ZISSEKIMAIN.TORICODE          AS TORICODE,                                " _
            & "     ZISSEKIMAIN.TORINAME          AS TORINAME,                                " _
            & "     ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                              " _
            & "     ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                               " _
            & "     ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                              " _
            & "     ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "     ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "     ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                               " _
            & "     ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                " _
            & "     ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "     ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                              " _
            & "     ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                              " _
            & "     ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "     ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                  " _
            & "     ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                 " _
            & "     ZISSEKIMAIN.TANNI             AS TANNI,                                   " _
            & "     ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                 " _
            & "     ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                              " _
            & "     ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "     ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                 " _
            & "     ZISSEKIMAIN.SYABARA           AS SYABARA,                                 " _
            & "     ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                             " _
            & "     ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                 " _
            & "     ZISSEKIMAIN.TRIP              AS TRIP,                                    " _
            & "     ZISSEKIMAIN.DRP               AS DRP,                                     " _
            & "     ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                               " _
            & "     ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                               " _
            & "     ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                               " _
            & "     ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "     ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "     ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "     ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                               " _
            & "     ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                " _
            & "     ZISSEKIMAIN.TANKA             AS TANKA,                                   " _
            & "     NULL                          AS JURYORYOKIN,                             " _
            & "     NULL                          AS TSUKORYO,                                " _
            & "     ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                            " _
            & "     ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                 " _
            & "     ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                              " _
            & "     ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                       " _
            & "     ZISSEKIMAIN.DELFLG            AS DELFLG,                                  " _
            & "     @INITYMD                      AS INITYMD,                                 " _
            & "     @INITUSER                     AS INITUSER,                                " _
            & "     @INITTERMID                   AS INITTERMID,                              " _
            & "     @INITPGID                     AS INITPGID,                                " _
            & "     NULL                          AS UPDYMD,                                  " _
            & "     NULL                          AS UPDUSER,                                 " _
            & "     NULL                          AS UPDTERMID,                               " _
            & "     NULL                          AS UPDPGID,                                 " _
            & "     @RECEIVEYMD                   AS RECEIVEYMD                               " _
            & " FROM(                                                                         " _
            & "      SELECT                                                                   " _
            & "          ZISSEKI.RECONO            AS RECONO,                                 " _
            & "          ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                           " _
            & "          ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                           " _
            & "          ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                           " _
            & "          ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                           " _
            & "          ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                       " _
            & "          ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                      " _
            & "          ZISSEKI.ORDERORG          AS ORDERORG,                               " _
            & "          ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                           " _
            & "          ZISSEKI.PRODUCT2          AS PRODUCT2,                               " _
            & "          ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                           " _
            & "          ZISSEKI.PRODUCT1          AS PRODUCT1,                               " _
            & "          ZISSEKI.OILNAME           AS OILNAME,                                " _
            & "          ZISSEKI.OILTYPE           AS OILTYPE,                                " _
            & "          ZISSEKI.TODOKECODE        AS TODOKECODE,                             " _
            & "          ZISSEKI.TODOKENAME        AS TODOKENAME,                             " _
            & "          ZISSEKI.TODOKENAMES       AS TODOKENAMES,                            " _
            & "          ZISSEKI.TORICODE          AS TORICODE,                               " _
            & "          ZISSEKI.TORINAME          AS TORINAME,                               " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                " _
            & "          THEN (SELECT SHUKABASHO                                              " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                     " _
            & "                  WHERE                                                        " _
            & "                      TORICODE     = ZISSEKI.TORICODE                          " _
            & "                  AND ORDERORG     = ZISSEKI.ORDERORG                          " _
            & "                  AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                      " _
            & "                  AND TRIP         = ZISSEKI.TRIP -1                           " _
            & "                  AND TODOKEDATE   = ZISSEKI.TODOKEDATE                        " _
            & "                  AND ZISSEKI      > 0                                         " _
            & "                  AND DELFLG       = '0'                                       " _
            & "              )                                                                " _
            & "          ELSE ZISSEKI.SHUKABASHO                                              " _
            & "          END AS SHUKABASHO,                                                   " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                " _
            & "          THEN (SELECT SHUKANAME                                               " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                     " _
            & "                  WHERE                                                        " _
            & "                      TORICODE     = ZISSEKI.TORICODE                          " _
            & "                  AND ORDERORG     = ZISSEKI.ORDERORG                          " _
            & "                  AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                      " _
            & "                  AND TRIP         = ZISSEKI.TRIP -1                           " _
            & "                  AND TODOKEDATE   = ZISSEKI.TODOKEDATE                        " _
            & "                  AND ZISSEKI      > 0                                         " _
            & "                  AND DELFLG       = '0'                                       " _
            & "              )                                                                " _
            & "          ELSE ZISSEKI.SHUKANAME                                               " _
            & "          END AS SHUKANAME,                                                    " _
            & "          ZISSEKI.SHUKANAMES        AS SHUKANAMES,                             " _
            & "          ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                          " _
            & "          ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                          " _
            & "          ZISSEKI.SHUKADATE         AS SHUKADATE,                              " _
            & "          ZISSEKI.LOADTIME          AS LOADTIME,                               " _
            & "          ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                             " _
            & "          ZISSEKI.TODOKEDATE        AS TODOKEDATE,                             " _
            & "          ZISSEKI.SHITEITIME        AS SHITEITIME,                             " _
            & "          ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                           " _
            & "          ZISSEKI.ZYUTYU            AS ZYUTYU,                                 " _
            & "          ZISSEKI.ZISSEKI           AS ZISSEKI,                                " _
            & "          ZISSEKI.TANNI             AS TANNI,                                  " _
            & "          ZISSEKI.TANKNUM           AS TANKNUM,                                " _
            & "          ZISSEKI.TANKNUMBER        AS TANKNUMBER,                             " _
            & "          ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                           " _
            & "          ZISSEKI.SYAGATA           AS SYAGATA,                                " _
            & "          ZISSEKI.SYABARA           AS SYABARA,                                " _
            & "          ZISSEKI.NINUSHINAME       AS NINUSHINAME,                            " _
            & "          ZISSEKI.CONTYPE           AS CONTYPE,                                " _
            & "          ZISSEKI.TRIP              AS TRIP,                                   " _
            & "          ZISSEKI.DRP               AS DRP,                                    " _
            & "          ZISSEKI.STAFFSLCT         AS STAFFSLCT,                              " _
            & "          ZISSEKI.STAFFNAME         AS STAFFNAME,                              " _
            & "          ZISSEKI.STAFFCODE         AS STAFFCODE,                              " _
            & "          ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                           " _
            & "          ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                           " _
            & "          ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                            " _
            & "          ZISSEKI.SHUKODATE         AS SHUKODATE,                              " _
            & "          ZISSEKI.KIKODATE          AS KIKODATE,                               " _
            & "          NULL                      AS KYUZITUTANKA,                           " _
            & "          TANKA.TANKA               AS TANKA,                                  " _
            & "          COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) AS YUSOUHI,  " _
            & "          CALENDAR.WORKINGDAY       AS WORKINGDAY,                             " _
            & "          CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                     " _
            & "          ZISSEKI.DELFLG            AS DELFLG                                  " _
            & "      FROM LNG.LNT0001_ZISSEKI ZISSEKI                                         " _
            & "      LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                     " _
            & "          ON @TORICODE = TANKA.TORICODE                                        " _
            & "          AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                             " _
            & "          AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                   " _
            & "          AND ZISSEKI.SHUKABASHO = TANKA.AVOCADOSHUKABASHO                     " _
            & "          AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                     " _
            & "          AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                            " _
            & "          AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                               " _
            & "          AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                               " _
            & "          AND TANKA.DELFLG = @DELFLG                                           " _
            & "      LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                  " _
            & "          ON @TORICODE = CALENDAR.TORICODE                                     " _
            & "          AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                " _
            & "          AND CALENDAR.DELFLG = @DELFLG                                        " _
            & "      WHERE                                                                    " _
            & "          ZISSEKI.TORICODE = @TORICODE                                         " _
            & "          AND ZISSEKI.ZISSEKI <> 0                                             " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                  " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                    " _
            & "          AND ZISSEKI.STACKINGTYPE <> '積置'                                   " _
            & "          AND ZISSEKI.DELFLG = @DELFLG                                         " _
            & " ) ZISSEKIMAIN                                                                 " _
            & " ON DUPLICATE KEY UPDATE                                                       " _
            & "         RECONO                    = VALUES(RECONO),                           " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                     " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                     " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                     " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                     " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                 " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                " _
            & "         ORDERORG                  = VALUES(ORDERORG),                         " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                     " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                         " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                     " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                         " _
            & "         OILNAME                   = VALUES(OILNAME),                          " _
            & "         OILTYPE                   = VALUES(OILTYPE),                          " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                       " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                       " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                      " _
            & "         TORICODE                  = VALUES(TORICODE),                         " _
            & "         TORINAME                  = VALUES(TORINAME),                         " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                       " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                        " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                       " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                    " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                    " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                        " _
            & "         LOADTIME                  = VALUES(LOADTIME),                         " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                       " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                       " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                       " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                     " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                           " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                          " _
            & "         TANNI                     = VALUES(TANNI),                            " _
            & "         TANKNUM                   = VALUES(TANKNUM),                          " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                       " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                     " _
            & "         SYAGATA                   = VALUES(SYAGATA),                          " _
            & "         SYABARA                   = VALUES(SYABARA),                          " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                      " _
            & "         CONTYPE                   = VALUES(CONTYPE),                          " _
            & "         TRIP                      = VALUES(TRIP),                             " _
            & "         DRP                       = VALUES(DRP),                              " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                        " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                        " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                        " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                     " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                     " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                      " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                        " _
            & "         KIKODATE                  = VALUES(KIKODATE),                         " _
            & "         TANKA                     = VALUES(TANKA),                            " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                      " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                         " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                     " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                          " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                       " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                " _
            & "         DELFLG                    = @DELFLG,                                  " _
            & "         UPDYMD                    = @UPDYMD,                                  " _
            & "         UPDUSER                   = @UPDUSER,                                 " _
            & "         UPDTERMID                 = @UPDTERMID,                               " _
            & "         UPDPGID                   = @UPDPGID,                                 " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                              "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東京ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0027_TOKYOGUSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東北天然ガス輸送費テーブル更新
    ''' </summary>
    Private Sub TNG_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東北天然ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0017_TNGYUSOUHI                                   " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北天然ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0017_TNGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(東北天然ガス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0017_TNGYUSOUHI(                                                                      " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO                AS RECONO,                                                             " _
            & "        ZISSEKI.LOADUNLOTYPE          AS LOADUNLOTYPE,                                                       " _
            & "        ZISSEKI.STACKINGTYPE          AS STACKINGTYPE,                                                       " _
            & "        ZISSEKI.ORDERORGCODE          AS ORDERORGCODE,                                                       " _
            & "        ZISSEKI.ORDERORGNAME          AS ORDERORGNAME,                                                       " _
            & "        ZISSEKI.KASANAMEORDERORG      AS KASANAMEORDERORG,                                                   " _
            & "        ZISSEKI.KASANCODEORDERORG     AS KASANCODEORDERORG,                                                  " _
            & "        ZISSEKI.ORDERORG              AS ORDERORG,                                                           " _
            & "        ZISSEKI.PRODUCT2NAME          AS PRODUCT2NAME,                                                       " _
            & "        ZISSEKI.PRODUCT2              AS PRODUCT2,                                                           " _
            & "        ZISSEKI.PRODUCT1NAME          AS PRODUCT1NAME,                                                       " _
            & "        ZISSEKI.PRODUCT1              AS PRODUCT1,                                                           " _
            & "        ZISSEKI.OILNAME               AS OILNAME,                                                            " _
            & "        ZISSEKI.OILTYPE               AS OILTYPE,                                                            " _
            & "        ZISSEKI.TODOKECODE            AS TODOKECODE,                                                         " _
            & "        ZISSEKI.TODOKENAME            AS TODOKENAME,                                                         " _
            & "        ZISSEKI.TODOKENAMES           AS TODOKENAMES,                                                        " _
            & "        ZISSEKI.TORICODE              AS TORICODE,                                                           " _
            & "        ZISSEKI.TORINAME              AS TORINAME,                                                           " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND ZISSEKI      > 0                                                                         " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND ZISSEKI      > 0                                                                         " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES            AS SHUKANAMES,                                                         " _
            & "        ZISSEKI.SHUKATORICODE         AS SHUKATORICODE,                                                      " _
            & "        ZISSEKI.SHUKATORINAME         AS SHUKATORINAME,                                                      " _
            & "        ZISSEKI.SHUKADATE             AS SHUKADATE,                                                          " _
            & "        ZISSEKI.LOADTIME              AS LOADTIME,                                                           " _
            & "        ZISSEKI.LOADTIMEIN            AS LOADTIMEIN,                                                         " _
            & "        ZISSEKI.TODOKEDATE            AS TODOKEDATE,                                                         " _
            & "        ZISSEKI.SHITEITIME            AS SHITEITIME,                                                         " _
            & "        ZISSEKI.SHITEITIMEIN          AS SHITEITIMEIN,                                                       " _
            & "        ZISSEKI.ZYUTYU                AS ZYUTYU,                                                             " _
            & "        ZISSEKI.ZISSEKI               AS ZISSEKI,                                                            " _
            & "        ZISSEKI.TANNI                 AS TANNI,                                                              " _
            & "        ZISSEKI.TANKNUM               AS TANKNUM,                                                            " _
            & "        ZISSEKI.TANKNUMBER            AS TANKNUMBER,                                                         " _
            & "        ZISSEKI.GYOMUTANKNUM          AS GYOMUTANKNUM,                                                       " _
            & "        ZISSEKI.SYAGATA               AS SYAGATA,                                                            " _
            & "        ZISSEKI.SYABARA               AS SYABARA,                                                            " _
            & "        ZISSEKI.NINUSHINAME           AS NINUSHINAME,                                                        " _
            & "        ZISSEKI.CONTYPE               AS CONTYPE,                                                            " _
            & "        ZISSEKI.TRIP                  AS TRIP,                                                               " _
            & "        ZISSEKI.DRP                   AS DRP,                                                                " _
            & "        ZISSEKI.STAFFSLCT             AS STAFFSLCT,                                                          " _
            & "        ZISSEKI.STAFFNAME             AS STAFFNAME,                                                          " _
            & "        ZISSEKI.STAFFCODE             AS STAFFCODE,                                                          " _
            & "        ZISSEKI.SUBSTAFFSLCT          AS SUBSTAFFSLCT,                                                       " _
            & "        ZISSEKI.SUBSTAFFNAME          AS SUBSTAFFNAME,                                                       " _
            & "        ZISSEKI.SUBSTAFFNUM           AS SUBSTAFFNUM,                                                        " _
            & "        ZISSEKI.SHUKODATE             AS SHUKODATE,                                                          " _
            & "        ZISSEKI.KIKODATE              AS KIKODATE,                                                           " _
            & "        TANKA.TANKA                   AS TANKA,                                                              " _
            & "        NULL                          AS JURYORYOKIN,                                                        " _
            & "        NULL                          AS TSUKORYO,                                                           " _
            & "        HOLIDAYRATE.TANKA             AS KYUZITUTANKA,                                                       " _
            & "        COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) + COALESCE(HOLIDAYRATE.TANKA, 0) AS YUSOUHI, " _
            & "        CALENDAR.WORKINGDAY           AS WORKINGDAY,                                                         " _
            & "        CALENDAR.PUBLICHOLIDAYNAME    AS PUBLICHOLIDAYNAME,                                                  " _
            & "        ZISSEKI.DELFLG                AS DELFLG,                                                             " _
            & "        @INITYMD                      AS INITYMD,                                                            " _
            & "        @INITUSER                     AS INITUSER,                                                           " _
            & "        @INITTERMID                   AS INITTERMID,                                                         " _
            & "        @INITPGID                     AS INITPGID,                                                           " _
            & "        NULL                          AS UPDYMD,                                                             " _
            & "        NULL                          AS UPDUSER,                                                            " _
            & "        NULL                          AS UPDTERMID,                                                          " _
            & "        NULL                          AS UPDPGID,                                                            " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                          " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                              " _
            & "        AND ZISSEKI.SHUKABASHO = TANKA.AVOCADOSHUKABASHO                                                     " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "        AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                                                            " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                            " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                            " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                   " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                      " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北天然ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0017_TNGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東北電力輸送費テーブル更新
    ''' </summary>
    Private Sub TOHOKU_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東北電力輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0018_TOHOKUYUSOUHI                                " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北電力輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0018_TOHOKUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(東北電力輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0018_TOHOKUYUSOUHI(                                                                   " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND ZISSEKI      > 0                                                                         " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND ZISSEKI      > 0                                                                         " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                              " _
            & "        TANKA.TANKA                AS TANKA,                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                           " _
            & "        NULL                       AS TSUKORYO,                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                          " _
            & "        COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) + COALESCE(HOLIDAYRATE.TANKA, 0) AS YUSOUHI, " _
            & "        CALENDAR.WORKINGDAY AS WORKINGDAY,                                                                   " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                     " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                                                " _
            & "        @INITYMD                      AS INITYMD,                                                            " _
            & "        @INITUSER                     AS INITUSER,                                                           " _
            & "        @INITTERMID                   AS INITTERMID,                                                         " _
            & "        @INITPGID                     AS INITPGID,                                                           " _
            & "        NULL                          AS UPDYMD,                                                             " _
            & "        NULL                          AS UPDUSER,                                                            " _
            & "        NULL                          AS UPDTERMID,                                                          " _
            & "        NULL                          AS UPDPGID,                                                            " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                          " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                              " _
            & "        AND ZISSEKI.SHUKABASHO = TANKA.AVOCADOSHUKABASHO                                                     " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                            " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                            " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                            " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                   " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                      " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北電力輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0018_TOHOKUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 北海道LNG輸送費テーブル更新
    ''' </summary>
    Private Sub HOKKAIDOLNG_Update(ByVal iTori As String, ByVal iTaishoYm As String, ByRef oResult As String)

        oResult = C_MESSAGE_NO.NORMAL

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(北海道LNG輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0024_HOKKAIDOLNGYUSOUHI                           " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = " & iTori _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(北海道LNG輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0024_HOKKAIDOLNGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            '○ DB更新SQL(北海道LNG輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0024_HOKKAIDOLNGYUSOUHI(                                                                             " _
            & "        RECONO,                                                                                                             " _
            & "        LOADUNLOTYPE,                                                                                                       " _
            & "        STACKINGTYPE,                                                                                                       " _
            & "        ORDERORGCODE,                                                                                                       " _
            & "        ORDERORGNAME,                                                                                                       " _
            & "        KASANAMEORDERORG,                                                                                                   " _
            & "        KASANCODEORDERORG,                                                                                                  " _
            & "        ORDERORG,                                                                                                           " _
            & "        PRODUCT2NAME,                                                                                                       " _
            & "        PRODUCT2,                                                                                                           " _
            & "        PRODUCT1NAME,                                                                                                       " _
            & "        PRODUCT1,                                                                                                           " _
            & "        OILNAME,                                                                                                            " _
            & "        OILTYPE,                                                                                                            " _
            & "        TODOKECODE,                                                                                                         " _
            & "        TODOKENAME,                                                                                                         " _
            & "        TODOKENAMES,                                                                                                        " _
            & "        TORICODE,                                                                                                           " _
            & "        TORINAME,                                                                                                           " _
            & "        SHUKABASHO,                                                                                                         " _
            & "        SHUKANAME,                                                                                                          " _
            & "        SHUKANAMES,                                                                                                         " _
            & "        SHUKATORICODE,                                                                                                      " _
            & "        SHUKATORINAME,                                                                                                      " _
            & "        SHUKADATE,                                                                                                          " _
            & "        LOADTIME,                                                                                                           " _
            & "        LOADTIMEIN,                                                                                                         " _
            & "        TODOKEDATE,                                                                                                         " _
            & "        SHITEITIME,                                                                                                         " _
            & "        SHITEITIMEIN,                                                                                                       " _
            & "        ZYUTYU,                                                                                                             " _
            & "        ZISSEKI,                                                                                                            " _
            & "        TANNI,                                                                                                              " _
            & "        TANKNUM,                                                                                                            " _
            & "        TANKNUMBER,                                                                                                         " _
            & "        GYOMUTANKNUM,                                                                                                       " _
            & "        SYAGATA,                                                                                                            " _
            & "        SYABARA,                                                                                                            " _
            & "        NINUSHINAME,                                                                                                        " _
            & "        CONTYPE,                                                                                                            " _
            & "        TRIP,                                                                                                               " _
            & "        DRP,                                                                                                                " _
            & "        STAFFSLCT,                                                                                                          " _
            & "        STAFFNAME,                                                                                                          " _
            & "        STAFFCODE,                                                                                                          " _
            & "        SUBSTAFFSLCT,                                                                                                       " _
            & "        SUBSTAFFNAME,                                                                                                       " _
            & "        SUBSTAFFNUM,                                                                                                        " _
            & "        SHUKODATE,                                                                                                          " _
            & "        KIKODATE,                                                                                                           " _
            & "        TANKA,                                                                                                              " _
            & "        JURYORYOKIN,                                                                                                        " _
            & "        TSUKORYO,                                                                                                           " _
            & "        KYUZITUTANKA,                                                                                                       " _
            & "        YUSOUHI,                                                                                                            " _
            & "        WORKINGDAY,                                                                                                         " _
            & "        PUBLICHOLIDAYNAME,                                                                                                  " _
            & "        DELFLG,                                                                                                             " _
            & "        INITYMD,                                                                                                            " _
            & "        INITUSER,                                                                                                           " _
            & "        INITTERMID,                                                                                                         " _
            & "        INITPGID,                                                                                                           " _
            & "        UPDYMD,                                                                                                             " _
            & "        UPDUSER,                                                                                                            " _
            & "        UPDTERMID,                                                                                                          " _
            & "        UPDPGID,                                                                                                            " _
            & "        RECEIVEYMD)                                                                                                         " _
            & "    SELECT                                                                                                                  " _
            & "        ZISSEKI.RECONO                AS RECONO,                                                                            " _
            & "        ZISSEKI.LOADUNLOTYPE          AS LOADUNLOTYPE,                                                                      " _
            & "        ZISSEKI.STACKINGTYPE          AS STACKINGTYPE,                                                                      " _
            & "        ZISSEKI.ORDERORGCODE          AS ORDERORGCODE,                                                                      " _
            & "        ZISSEKI.ORDERORGNAME          AS ORDERORGNAME,                                                                      " _
            & "        ZISSEKI.KASANAMEORDERORG      AS KASANAMEORDERORG,                                                                  " _
            & "        ZISSEKI.KASANCODEORDERORG     AS KASANCODEORDERORG,                                                                 " _
            & "        ZISSEKI.ORDERORG              AS ORDERORG,                                                                          " _
            & "        ZISSEKI.PRODUCT2NAME          AS PRODUCT2NAME,                                                                      " _
            & "        ZISSEKI.PRODUCT2              AS PRODUCT2,                                                                          " _
            & "        ZISSEKI.PRODUCT1NAME          AS PRODUCT1NAME,                                                                      " _
            & "        ZISSEKI.PRODUCT1              AS PRODUCT1,                                                                          " _
            & "        ZISSEKI.OILNAME               AS OILNAME,                                                                           " _
            & "        ZISSEKI.OILTYPE               AS OILTYPE,                                                                           " _
            & "        ZISSEKI.TODOKECODE            AS TODOKECODE,                                                                        " _
            & "        ZISSEKI.TODOKENAME            AS TODOKENAME,                                                                        " _
            & "        ZISSEKI.TODOKENAMES           AS TODOKENAMES,                                                                       " _
            & "        ZISSEKI.TORICODE              AS TORICODE,                                                                          " _
            & "        ZISSEKI.TORINAME              AS TORINAME,                                                                          " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                               " _
            & "        THEN (SELECT SHUKABASHO                                                                                             " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                    " _
            & "                WHERE                                                                                                       " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                                         " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                                         " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                     " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                                          " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                       " _
            & "                AND ZISSEKI      > 0                                                                                        " _
            & "                AND DELFLG       = '0'                                                                                      " _
            & "            )                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                                             " _
            & "        END AS SHUKABASHO,                                                                                                  " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                               " _
            & "        THEN (SELECT SHUKANAME                                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                    " _
            & "                WHERE                                                                                                       " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                                         " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                                         " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                     " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                                          " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                       " _
            & "                AND ZISSEKI      > 0                                                                                        " _
            & "                AND DELFLG       = '0'                                                                                      " _
            & "            )                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                                              " _
            & "        END AS SHUKANAME,                                                                                                   " _
            & "        ZISSEKI.SHUKANAMES            AS SHUKANAMES,                                                                        " _
            & "        ZISSEKI.SHUKATORICODE         AS SHUKATORICODE,                                                                     " _
            & "        ZISSEKI.SHUKATORINAME         AS SHUKATORINAME,                                                                     " _
            & "        ZISSEKI.SHUKADATE             AS SHUKADATE,                                                                         " _
            & "        ZISSEKI.LOADTIME              AS LOADTIME,                                                                          " _
            & "        ZISSEKI.LOADTIMEIN            AS LOADTIMEIN,                                                                        " _
            & "        ZISSEKI.TODOKEDATE            AS TODOKEDATE,                                                                        " _
            & "        ZISSEKI.SHITEITIME            AS SHITEITIME,                                                                        " _
            & "        ZISSEKI.SHITEITIMEIN          AS SHITEITIMEIN,                                                                      " _
            & "        ZISSEKI.ZYUTYU                AS ZYUTYU,                                                                            " _
            & "        ZISSEKI.ZISSEKI               AS ZISSEKI,                                                                           " _
            & "        ZISSEKI.TANNI                 AS TANNI,                                                                             " _
            & "        ZISSEKI.TANKNUM               AS TANKNUM,                                                                           " _
            & "        ZISSEKI.TANKNUMBER            AS TANKNUMBER,                                                                        " _
            & "        ZISSEKI.GYOMUTANKNUM          AS GYOMUTANKNUM,                                                                      " _
            & "        ZISSEKI.SYAGATA               AS SYAGATA,                                                                           " _
            & "        ZISSEKI.SYABARA               AS SYABARA,                                                                           " _
            & "        ZISSEKI.NINUSHINAME           AS NINUSHINAME,                                                                       " _
            & "        ZISSEKI.CONTYPE               AS CONTYPE,                                                                           " _
            & "        ZISSEKI.TRIP                  AS TRIP,                                                                              " _
            & "        ZISSEKI.DRP                   AS DRP,                                                                               " _
            & "        ZISSEKI.STAFFSLCT             AS STAFFSLCT,                                                                         " _
            & "        ZISSEKI.STAFFNAME             AS STAFFNAME,                                                                         " _
            & "        ZISSEKI.STAFFCODE             AS STAFFCODE,                                                                         " _
            & "        ZISSEKI.SUBSTAFFSLCT          AS SUBSTAFFSLCT,                                                                      " _
            & "        ZISSEKI.SUBSTAFFNAME          AS SUBSTAFFNAME,                                                                      " _
            & "        ZISSEKI.SUBSTAFFNUM           AS SUBSTAFFNUM,                                                                       " _
            & "        ZISSEKI.SHUKODATE             AS SHUKODATE,                                                                         " _
            & "        ZISSEKI.KIKODATE              AS KIKODATE,                                                                          " _
            & "        CASE                                                                                                                " _
            & "            WHEN ZISSEKI.TODOKECODE = '004460' THEN TANKA_TETSUGEN.TANKA                                                    " _
            & "            ELSE TANKA.TANKA                                                                                                " _
            & "        END                           AS TANKA,                                                                             " _
            & "        NULL                          AS JURYORYOKIN,                                                                       " _
            & "        NULL                          AS TSUKORYO,                                                                          " _
            & "        HOLIDAYRATE.TANKA             AS KYUZITUTANKA,                                                                      " _
            & "        CASE                                                                                                                " _
            & "            WHEN ZISSEKI.TODOKECODE = '004460' THEN COALESCE(TANKA_TETSUGEN.TANKA, 0) + COALESCE(HOLIDAYRATE.TANKA, 0)      " _
            & "            ELSE COALESCE(TANKA.TANKA, 0) + COALESCE(HOLIDAYRATE.TANKA, 0)                                                  " _
            & "        END                           AS YUSOUHI,                                                                           " _
            & "        CALENDAR.WORKINGDAY           AS WORKINGDAY,                                                                        " _
            & "        CALENDAR.PUBLICHOLIDAYNAME    AS PUBLICHOLIDAYNAME,                                                                 " _
            & "        ZISSEKI.DELFLG                AS DELFLG,                                                                            " _
            & "        @INITYMD                      AS INITYMD,                                                                           " _
            & "        @INITUSER                     AS INITUSER,                                                                          " _
            & "        @INITTERMID                   AS INITTERMID,                                                                        " _
            & "        @INITPGID                     AS INITPGID,                                                                          " _
            & "        NULL                          AS UPDYMD,                                                                            " _
            & "        NULL                          AS UPDUSER,                                                                           " _
            & "        NULL                          AS UPDTERMID,                                                                         " _
            & "        NULL                          AS UPDPGID,                                                                           " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                                         " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                        " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                                    " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                                       " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                            " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                                  " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                                    " _
            & "        AND TANKA.AVOCADOTODOKECODE <> '004460'                                                                             " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                              " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                              " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                                          " _
            & "        AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                                                                           " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA_TETSUGEN                                                                           " _
            & "        ON @TORICODE = TANKA_TETSUGEN.TORICODE                                                                              " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA_TETSUGEN.ORGCODE                                                                   " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA_TETSUGEN.KASANORGCODE                                                         " _
            & "        AND ZISSEKI.TODOKECODE = TANKA_TETSUGEN.AVOCADOTODOKECODE                                                           " _
            & "        AND REPLACE(ZISSEKI.SYAGATA, '単車タンク', '単車') = TANKA_TETSUGEN.SYAGATANAME                                     " _
            & "        AND TANKA_TETSUGEN.AVOCADOTODOKECODE = '004460'                                                                     " _
            & "        AND TANKA_TETSUGEN.STYMD  <= ZISSEKI.TODOKEDATE                                                                     " _
            & "        AND TANKA_TETSUGEN.ENDYMD >= ZISSEKI.TODOKEDATE                                                                     " _
            & "        AND TANKA_TETSUGEN.DELFLG = @DELFLG                                                                                 " _
            & "        AND ZISSEKI.BRANCHCODE = TANKA_TETSUGEN.BRANCHCODE                                                                  " _
            & "     LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                                    " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                               " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                                       " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                                   " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                                        " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                                            " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                 " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                   " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                                        " _
            & " ON DUPLICATE KEY UPDATE                                                                                                    " _
            & "         RECONO                    = VALUES(RECONO),                                                                        " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                  " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                  " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                  " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                  " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                              " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                             " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                      " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                  " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                      " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                  " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                      " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                       " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                       " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                    " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                    " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                   " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                      " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                      " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                    " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                     " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                    " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                 " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                 " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                     " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                      " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                    " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                    " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                    " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                  " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                        " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                       " _
            & "         TANNI                     = VALUES(TANNI),                                                                         " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                       " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                    " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                  " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                       " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                       " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                   " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                       " _
            & "         TRIP                      = VALUES(TRIP),                                                                          " _
            & "         DRP                       = VALUES(DRP),                                                                           " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                     " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                     " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                     " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                  " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                  " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                   " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                     " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                      " _
            & "         TANKA                     = VALUES(TANKA),                                                                         " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                   " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                      " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                  " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                       " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                    " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                             " _
            & "         DELFLG                    = @DELFLG,                                                                               " _
            & "         UPDYMD                    = @UPDYMD,                                                                               " _
            & "         UPDUSER                   = @UPDUSER,                                                                              " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                            " _
            & "         UPDPGID                   = @UPDPGID,                                                                              " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                           "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(北海道LNG輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                        YMDFROM.Value = WF_TaishoYm.Value & "/01"
                        YMDTO.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい", "", True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0024_HOKKAIDOLNGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                oResult = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 届先マスタ差分抽出＆追加処理
    ''' </summary>
    Private Sub TODOKESAKIMAS_Insert()
        Dim dtTodokeMas As New DataTable
        dtTodokeMas = CMNPTS.SelectZissekiTodokeSQL()

        Dim SQLStr As String = ""
        SQLStr &= " INSERT INTO LNG.LNM0021_TODOKE "
        SQLStr &= "  ( TORICODE ,TORINAME ,ORGCODE ,ORGNAME ,KASANORGCODE ,KASANORGNAME "
        SQLStr &= "  , SHUKABASHO ,SHUKANAME ,TODOKECODE ,TODOKENAME "
        SQLStr &= "  , DELFLG ,INITYMD ,INITUSER ,INITTERMID ,INITPGID"
        SQLStr &= "  , UPDYMD ,UPDUSER ,UPDTERMID ,UPDPGID ,RECEIVEYMD) "

        SQLStr &= " VALUES "
        SQLStr &= "  ( @TORICODE ,@TORINAME ,@ORGCODE ,@ORGNAME ,@KASANORGCODE ,@KASANORGNAME "
        SQLStr &= "  , @SHUKABASHO ,@SHUKANAME ,@TODOKECODE ,@TODOKENAME "
        SQLStr &= "  , @DELFLG ,@INITYMD ,@INITUSER ,@INITTERMID ,@INITPGID"
        SQLStr &= "  , @UPDYMD ,@UPDUSER ,@UPDTERMID ,@UPDPGID ,@RECEIVEYMD) "

        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                Dim SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                SQLcmd.CommandTimeout = 300

                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar)
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar)
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar)
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar)
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar)
                Dim P_SHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAME", MySqlDbType.VarChar)
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar)
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar)

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar)
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar)
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar)
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar)
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar)
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar)
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar)
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)

                For Each dtTodokeMasrow As DataRow In dtTodokeMas.Rows
                    P_TORICODE.Value = dtTodokeMasrow("TORICODE")
                    P_TORINAME.Value = dtTodokeMasrow("TORINAME")
                    P_ORGCODE.Value = dtTodokeMasrow("ORGCODE")
                    P_ORGNAME.Value = dtTodokeMasrow("ORGNAME")
                    P_KASANORGCODE.Value = dtTodokeMasrow("KASANORGCODE")
                    P_KASANORGNAME.Value = dtTodokeMasrow("KASANORGNAME")
                    P_SHUKABASHO.Value = dtTodokeMasrow("SHUKABASHO")
                    P_SHUKANAME.Value = dtTodokeMasrow("SHUKANAME")
                    P_TODOKECODE.Value = dtTodokeMasrow("TODOKECODE")
                    P_TODOKENAME.Value = dtTodokeMasrow("TODOKENAME")

                    P_DELFLG.Value = C_DELETE_FLG.ALIVE
                    P_INITYMD.Value = Date.Now
                    P_INITUSER.Value = Master.USERID
                    P_INITTERMID.Value = Master.USERTERMID
                    P_INITPGID.Value = "LNT0001ZissekiIntake"
                    P_UPDYMD.Value = Date.Now
                    P_UPDUSER.Value = Master.USERID
                    P_UPDTERMID.Value = Master.USERTERMID
                    P_UPDPGID.Value = "LNT0001ZissekiIntake"
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Next
                'CLOSE
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001Z TODOKESAKIMAS_Insert")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001Z TODOKESAKIMAS_Insert"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

    End Sub
#End Region

End Class