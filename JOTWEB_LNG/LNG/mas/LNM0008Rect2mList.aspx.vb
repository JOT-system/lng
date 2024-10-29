''************************************************************
' コード変換特例２マスタメンテ一覧画面
' 作成日 2022/02/16
' 更新日 2023/12/21
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/16 新規作成
'          : 2023/12/21 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コード変換特例２マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0008Rect2mList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0008tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNM0008Exceltbl As New DataTable                        'Excelデータ格納用テーブル

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

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""

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
                    Master.RecoverTable(LNM0008tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0008WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0008WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                        Case "WF_ButtonDebug"           'デバッグボタン押下
                            WF_ButtonDEBUG_Click()
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
            If Not IsNothing(LNM0008tbl) Then
                LNM0008tbl.Clear()
                LNM0008tbl.Dispose()
                LNM0008tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0008WRKINC.MAPIDL
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

        '〇 更新画面からの遷移の場合、更新完了メッセージを出力
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

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0008S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0008D Then
            Master.RecoverTable(LNM0008tbl, work.WF_SEL_INPTBL.Text)
        End If

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
        Master.SaveTable(LNM0008tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0008tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0008tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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

        If IsNothing(LNM0008tbl) Then
            LNM0008tbl = New DataTable
        End If

        If LNM0008tbl.Columns.Count <> 0 Then
            LNM0008tbl.Columns.Clear()
        End If

        LNM0008tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをコード変換特例２マスタから取得する
        Dim SQLStr As String =
              " Select                                                                                 " _
            & "     1                                                         AS 'SELECT'              " _
            & "   , 0                                                         AS HIDDEN                " _
            & "   , 0                                                         AS LINECNT               " _
            & "   , ''                                                        AS OPERATION             " _
            & "   , LNM0008.UPDTIMSTP                                         AS UPDTIMSTP             " _
            & "   , coalesce(RTRIM(LNM0008.DELFLG), '')                         AS DELFLG                " _
            & "   , coalesce(RTRIM(LNM0008.ORGCODE), '')                        AS ORGCODE               " _
            & "   , coalesce(RTRIM(LNM0008.BIGCTNCD), '')                       AS BIGCTNCD              " _
            & "   , coalesce(RTRIM(LNM0008.MIDDLECTNCD), '')                    AS MIDDLECTNCD           " _
            & "   , coalesce(RTRIM(LNM0008.PURPOSE), '')                        AS PURPOSE               " _
            & "   , coalesce(RTRIM(LNM0008.STACKFREEKBN), '')                   AS STACKFREEKBN          " _
            & "   , coalesce(RTRIM(LNM0008.SPRDEPTRUSTEECD), '')                AS SPRDEPTRUSTEECD       " _
            & "   , coalesce(RTRIM(LNM0008.SPRDEPTRUSTEESUBCD), '')             AS SPRDEPTRUSTEESUBCD    " _
            & "   , coalesce(RTRIM(LNM0008.SPRDEPTRUSTEESUBZKBN), '')           AS SPRDEPTRUSTEESUBZKBN  " _
            & "   , coalesce(RTRIM(LNM0008.SPRDEPSHIPPERCD), '')                AS SPRDEPSHIPPERCD       " _
            & "   , coalesce(RTRIM(LNM0008.SPRARRTRUSTEECD), '')                AS SPRARRTRUSTEECD       " _
            & "   , coalesce(RTRIM(LNM0008.SPRARRTRUSTEESUBCD), '')             AS SPRARRTRUSTEESUBCD    " _
            & "   , coalesce(RTRIM(LNM0008.SPRARRTRUSTEESUBZKBN), '')           AS SPRARRTRUSTEESUBZKBN  " _
            & "   , coalesce(RTRIM(LNM0008.SPRJRITEMCD), '')                    AS SPRJRITEMCD           " _
            & "   , coalesce(RTRIM(LNM0008.SPRSTACKFREEKBN), '')                AS SPRSTACKFREEKBN       " _
            & "   , coalesce(RTRIM(LNM0008.SPRSTATUSKBN), '')                   AS SPRSTATUSKBN          " _
            & "   , coalesce(RTRIM(LNM0008.BEFOREORGCODE), '')                  AS BEFOREORGCODE          " _
            & " FROM                                                                                   " _
            & "     LNG.LNM0008_RECT2M LNM0008                                                         "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLWhereStr = " WHERE                     " _
                        & "     LNM0008.ORGCODE = @P1 "
        End If
        ' 大分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_BIGCTNCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                          " _
                            & "     LNM0008.BIGCTNCD = @P2     "
            Else
                SQLWhereStr &= "    AND LNM0008.BIGCTNCD = @P2 "
            End If
        End If
        ' 中分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLECTNCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                             " _
                            & "     LNM0008.MIDDLECTNCD = @P3     "
            Else
                SQLWhereStr &= "    AND LNM0008.MIDDLECTNCD = @P3 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                  " _
                            & "     LNM0008.DELFLG = 0 "
            Else
                SQLWhereStr &= "    AND LNM0008.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY                  " _
            & "     LNM0008.ORGCODE       " _
            & "   , LNM0008.BIGCTNCD      " _
            & "   , LNM0008.MIDDLECTNCD   "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6)  '組織コード
                    PARA1.Value = work.WF_SEL_ORG.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_BIGCTNCD.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2)  '大分類コード
                    PARA2.Value = work.WF_SEL_BIGCTNCD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLECTNCD.Text) Then
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2)  '中分類コード
                    PARA3.Value = work.WF_SEL_MIDDLECTNCD.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0008tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0008tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0008row As DataRow In LNM0008tbl.Rows
                    i += 1
                    LNM0008row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0008L Select"
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
        For Each LNM0008row As DataRow In LNM0008tbl.Rows
            If LNM0008row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0008row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
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
        Dim TBLview As DataView = New DataView(LNM0008tbl)

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
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()


        work.WF_SEL_LINECNT.Text = ""                                                         '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)                '削除
        work.WF_SEL_ORG2.Text = ""                                                            '組織コード
        work.WF_SEL_BIGCTNCD2.Text = ""                                                       '大分類コード
        work.WF_SEL_MIDDLECTNCD2.Text = ""                                                    '中分類コード
        work.WF_SEL_PURPOSE.Text = ""                                                         '使用目的
        work.WF_SEL_STACKFREEKBN.Text = ""                                                    '積空区分
        work.WF_SEL_SPRDEPTRUSTEECD.Text = ""                                                 '特例置換項目-発受託人コード
        work.WF_SEL_SPRDEPTRUSTEESUBCD.Text = ""                                              '特例置換項目-発受託人サブコード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRDEPTRUSTEESUBZKBN.Text)  '特例置換項目-発受託人サブゼロ変換区分
        work.WF_SEL_SPRDEPSHIPPERCD.Text = ""                                                 '特例置換項目-発荷主コード
        work.WF_SEL_SPRARRTRUSTEECD.Text = ""                                                 '特例置換項目-着受託人コード
        work.WF_SEL_SPRARRTRUSTEESUBCD.Text = ""                                              '特例置換項目-着受託人サブ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRARRTRUSTEESUBZKBN.Text)  '特例置換項目-着受託人サブゼロ変換区分
        work.WF_SEL_SPRJRITEMCD.Text = ""                                                     '特例置換項目-ＪＲ品目コード
        work.WF_SEL_SPRSTACKFREEKBN.Text = ""                                                 '特例置換項目-積空区分
        work.WF_SEL_SPRSTATUSKBN.Text = ""                                                    '特例置換項目-状態区分
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                           '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0008tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0008tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0008Rect2mHistory.aspx")
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
        Dim TBLview As New DataView(LNM0008tbl)
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
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_DBDataCheck As String = ""
        Dim WW_LineCNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0008tbl.Rows(WW_LineCNT)("LINECNT")                            '選択行
        work.WF_SEL_DELFLG.Text = LNM0008tbl.Rows(WW_LineCNT)("DELFLG")                              '削除フラグ
        work.WF_SEL_ORG2.Text = LNM0008tbl.Rows(WW_LineCNT)("ORGCODE")                               '組織コード
        work.WF_SEL_BIGCTNCD2.Text = LNM0008tbl.Rows(WW_LineCNT)("BIGCTNCD")                         '大分類コード
        work.WF_SEL_MIDDLECTNCD2.Text = LNM0008tbl.Rows(WW_LineCNT)("MIDDLECTNCD")                   '中分類コード
        work.WF_SEL_PURPOSE.Text = LNM0008tbl.Rows(WW_LineCNT)("PURPOSE")                            '使用目的
        work.WF_SEL_STACKFREEKBN.Text = LNM0008tbl.Rows(WW_LineCNT)("STACKFREEKBN")                  '積空区分
        work.WF_SEL_SPRDEPTRUSTEECD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRDEPTRUSTEECD")            '特例置換項目-発受託人コード
        work.WF_SEL_SPRDEPTRUSTEESUBCD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRDEPTRUSTEESUBCD")      '特例置換項目-発受託人サブコード
        work.WF_SEL_SPRDEPTRUSTEESUBZKBN.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRDEPTRUSTEESUBZKBN")  '特例置換項目-発受託人サブゼロ変換区分
        work.WF_SEL_SPRDEPSHIPPERCD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRDEPSHIPPERCD")            '特例置換項目-発荷主コード
        work.WF_SEL_SPRARRTRUSTEECD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRARRTRUSTEECD")            '特例置換項目-着受託人コード
        work.WF_SEL_SPRARRTRUSTEESUBCD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRARRTRUSTEESUBCD")      '特例置換項目-着受託人サブ
        work.WF_SEL_SPRARRTRUSTEESUBZKBN.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRARRTRUSTEESUBZKBN")  '特例置換項目-着受託人サブゼロ変換区分
        work.WF_SEL_SPRJRITEMCD.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRJRITEMCD")                    '特例置換項目-ＪＲ品目コード
        work.WF_SEL_SPRSTACKFREEKBN.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRSTACKFREEKBN")            '特例置換項目-積空区分
        work.WF_SEL_SPRSTATUSKBN.Text = LNM0008tbl.Rows(WW_LineCNT)("SPRSTATUSKBN")                  '特例置換項目-状態区分
        work.WF_SEL_TIMESTAMP.Text = LNM0008tbl.Rows(WW_LineCNT)("UPDTIMSTP")                        'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                  '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0008row As DataRow In LNM0008tbl.Rows
            Select Case LNM0008row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0008tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0008tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0008tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0008tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0008tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0008tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0008tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0008tbl, work.WF_SEL_INPTBL.Text)

        ' 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG2.Text) Then  '組織コード
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_ORG2.Text, work.WF_SEL_BIGCTNCD2.Text,
                                work.WF_SEL_MIDDLECTNCD2.Text, work.WF_SEL_TIMESTAMP.Text)
            End Using

            If Not isNormal(WW_DBDataCheck) Then
                Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0008WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        SetDETAIL(wb.ActiveSheet, WW_ACTIVEROW)

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
        wb.ActiveSheet.Range("C1").Value = "コード変換特例マスタ２一覧"
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
            Case LNM0008WRKINC.FILETYPE.EXCEL
                FileName = "コード変換特例マスタ２.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0008WRKINC.FILETYPE.PDF
                FileName = "コード変換特例マスタ２.pdf"
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
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '組織コード
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '大分類コード
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '中分類コード

        '入力不要列網掛け
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.ORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '組織名称
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.BIGCTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '大分類名称
        sheet.Columns(LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '中分類名称

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
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）組織コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.ORGNAME).Value = "組織名称"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = "（必須）大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = "大分類名称"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = "（必須）中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = "中分類名称"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.PURPOSE).Value = "使用目的"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.STACKFREEKBN).Value = "積空区分"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEECD).Value = "特例置換項目-発受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBCD).Value = "特例置換項目-発受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBZKBN).Value = "特例置換項目-発受託人サブゼロ変換区分"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPSHIPPERCD).Value = "特例置換項目-発荷主コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEECD).Value = "特例置換項目-着受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBCD).Value = "特例置換項目-着受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBZKBN).Value = "特例置換項目-着受託人サブゼロ変換区分"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRJRITEMCD).Value = "特例置換項目-ＪＲ品目コード"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTACKFREEKBN).Value = "特例置換項目-積空区分"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTATUSKBN).Value = "特例置換項目-状態区分"
        sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.BEFOREORGCODE).Value = "変換前組織コード"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '組織コード
            COMMENT_get(SQLcon, "ORG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.ORGCODE).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.ORGCODE).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '積空区分
            COMMENT_get(SQLcon, "STACKFREEKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                '積空区分
                sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.STACKFREEKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.STACKFREEKBN).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                '特例置換項目-積空区分
                sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTACKFREEKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTACKFREEKBN).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '状態区分
            COMMENT_get(SQLcon, "OPERATIONKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                '特例置換項目-状態区分
                sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTATUSKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTATUSKBN).Comment.Shape
                    .Width = 80
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '○ コメントに表示が難しいデータは別シートに作成
            WW_TEXT = "シート:大中小分類一覧参照"
            SETSUBSHEET(wb, "CTNCD")
            '大分類コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '中分類コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With

            WW_TEXT = "シート:受託人一覧参照"
            SETSUBSHEET(wb, "REKEJM")
            '特例置換項目-発受託人コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEECD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEECD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '特例置換項目-発受託人サブコード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '特例置換項目-着受託人コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEECD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEECD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '特例置換項目-着受託人サブコード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With

            '発荷主コード
            WW_TEXT = "シート:発荷主一覧参照"
            SETSUBSHEET(wb, "SHIPPER")
            '特例置換項目-発荷主コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPSHIPPERCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPSHIPPERCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With

            'ＪＲ品目コード
            WW_TEXT = "シート:ＪＲ品目一覧参照"
            SETSUBSHEET(wb, "ITEM")
            '特例置換項目-ＪＲ品目コード
            sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRJRITEMCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0008WRKINC.INOUTEXCELCOL.SPRJRITEMCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
        End Using

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        Dim WW_ORGCODE As String
        Dim WW_BIGCTNCD As String
        Dim WW_MIDDLECTNCD As String

        Dim WW_ORGNAME As String
        Dim WW_BIGCTNNM As String
        Dim WW_MIDDLECTNNM As String

        For Each Row As DataRow In LNM0008tbl.Rows
            WW_ORGCODE = Row("ORGCODE") '組織コード
            WW_BIGCTNCD = Row("BIGCTNCD") '大分類コード
            WW_MIDDLECTNCD = Row("MIDDLECTNCD") '中分類コード

            '名称取得
            WW_ORGNAME = ""
            WW_BIGCTNNM = ""
            WW_MIDDLECTNNM = ""

            CODENAME_get("ORG", WW_ORGCODE, WW_Dummy, WW_ORGNAME, WW_RtnSW) '組織名称
            CODENAME_get("BIGCTNCD", WW_BIGCTNCD, WW_Dummy, WW_BIGCTNNM, WW_RtnSW) '大分類名称
            CODENAME_get("MIDDLECTNCD", WW_MIDDLECTNCD, WW_BIGCTNCD, WW_MIDDLECTNNM, WW_RtnSW) '中分類名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.ORGCODE).Value = WW_ORGCODE '組織コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.ORGNAME).Value = WW_ORGNAME '組織名称
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = WW_BIGCTNCD '大分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = WW_BIGCTNNM '大分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = WW_MIDDLECTNCD '中分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = WW_MIDDLECTNNM '中分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.PURPOSE).Value = Row("PURPOSE") '使用目的
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.STACKFREEKBN).Value = Row("STACKFREEKBN") '積空区分
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEECD).Value = Row("SPRDEPTRUSTEECD") '特例置換項目-発受託人コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBCD).Value = Row("SPRDEPTRUSTEESUBCD") '特例置換項目-発受託人サブコード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBZKBN).Value = Row("SPRDEPTRUSTEESUBZKBN") '特例置換項目-発受託人サブゼロ変換区分
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPSHIPPERCD).Value = Row("SPRDEPSHIPPERCD") '特例置換項目-発荷主コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEECD).Value = Row("SPRARRTRUSTEECD") '特例置換項目-着受託人コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBCD).Value = Row("SPRARRTRUSTEESUBCD") '特例置換項目-着受託人サブコード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBZKBN).Value = Row("SPRARRTRUSTEESUBZKBN") '特例置換項目-着受託人サブゼロ変換区分
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRJRITEMCD).Value = Row("SPRJRITEMCD") '特例置換項目-ＪＲ品目コード
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTACKFREEKBN).Value = Row("SPRSTACKFREEKBN") '特例置換項目-積空区分
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTATUSKBN).Value = Row("SPRSTATUSKBN") '特例置換項目-状態区分
            sheet.Cells(WW_ACTIVEROW, LNM0008WRKINC.INOUTEXCELCOL.BEFOREORGCODE).Value = Row("BEFOREORGCODE") '変換前組織コード

            WW_ACTIVEROW += 1
        Next
    End Sub

    Public Sub SETSUBSHEET(ByVal wb As Workbook, ByVal I_FIELD As String)
        'メインシートを取得
        Dim mainsheet As IWorksheet = wb.ActiveSheet
        'サブシートを作成
        Dim subsheet As IWorksheet = wb.Worksheets.Add()
        subsheet.FreezePanes(1, 0)
        subsheet.TabColor = ColorTranslator.FromHtml(CONST_COLOR_GRAY)

        Dim WW_PrmData As New Hashtable
        Dim WW_PrmDataList = New StringBuilder
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""
        Dim WW_ROW As Integer = 0

        With leftview
            '○入力リスト取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Select Case I_FIELD
                    Case "CTNCD"
                        subsheet.Name = "大中小分類一覧"
                        SETCTNCDLIST(SQLcon, subsheet)
                    Case "REKEJM"
                        subsheet.Name = "受託人一覧"
                        SETREKEJMLIST(SQLcon, subsheet)
                    Case "SHIPPER"
                        subsheet.Name = "発荷主一覧"
                        SETSHIPPERLIST(SQLcon, subsheet)
                    Case "ITEM"
                        subsheet.Name = "ＪＲ品目一覧"
                        SETITEMLIST(SQLcon, subsheet)
                End Select

            End Using
        End With

        'サブシートの列幅自動調整
        subsheet.Cells.EntireColumn.AutoFit()

        'メインシートをアクティブにする
        mainsheet.Activate()

    End Sub

    ''' <summary>
    ''' 入力一覧作成(大中小分類一覧)
    ''' </summary>
    Protected Sub SETCTNCDLIST(ByVal SQLcon As MySqlConnection,
                                   ByVal WW_SHEET As IWorksheet)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(BIGCTNCD) AS BIGCTNCD ")
        SQLStr.AppendLine("   ,RTRIM(MIDDLECTNCD) AS MIDDLECTNCD ")
        SQLStr.AppendLine("   ,RTRIM(SMALLCTNCD) AS SMALLCTNCD ")
        SQLStr.AppendLine("   ,RTRIM(KANJI1) AS BIGCTNNAME ")
        SQLStr.AppendLine("   ,RTRIM(KANJI2) AS MIDDLECTNNAME ")
        SQLStr.AppendLine("   ,RTRIM(KANJI3) AS SMALLCTNNAME ")
        SQLStr.AppendLine(" FROM LNG.LNM0022_CLASS ")
        SQLStr.AppendLine(" WHERE DELFLG <> @DELFLG ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      BIGCTNCD")
        SQLStr.AppendLine("     ,MIDDLECTNCD")
        SQLStr.AppendLine("     ,SMALLCTNCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_ROW As Integer = 0
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_SHEET.Cells(WW_ROW, 0).Value = "大分類コード" '1列目
                        WW_SHEET.Cells(WW_ROW, 1).Value = "中分類コード" '2列目
                        WW_SHEET.Cells(WW_ROW, 2).Value = "小分類コード" '3列目
                        WW_SHEET.Cells(WW_ROW, 3).Value = "大分類名称" '4列目
                        WW_SHEET.Cells(WW_ROW, 4).Value = "中分類名称" '5列目
                        WW_SHEET.Cells(WW_ROW, 5).Value = "小分類名称" '6列目
                        WW_ROW += 1
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_SHEET.Cells(WW_ROW, 0).Value = Row("BIGCTNCD") '1列目
                            WW_SHEET.Cells(WW_ROW, 1).Value = Row("MIDDLECTNCD") '2列目
                            WW_SHEET.Cells(WW_ROW, 2).Value = Row("SMALLCTNCD") '3列目
                            WW_SHEET.Cells(WW_ROW, 3).Value = Row("BIGCTNNAME") '4列目
                            WW_SHEET.Cells(WW_ROW, 4).Value = Row("MIDDLECTNNAME") '5列目
                            WW_SHEET.Cells(WW_ROW, 5).Value = Row("SMALLCTNNAME") '6列目

                            WW_ROW += 1
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0022_CLASS SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0022_CLASS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
    End Sub

    ''' <summary>
    ''' 入力一覧作成(受託人一覧)
    ''' </summary>
    Protected Sub SETREKEJMLIST(ByVal SQLcon As MySqlConnection,
                                   ByVal WW_SHEET As IWorksheet)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(LNM0003.DEPSTATION) AS DEPSTATION ")
        SQLStr.AppendLine("   ,RTRIM(LNS0020.NAMES) AS DEPSTATIONNM ")
        SQLStr.AppendLine("   ,RTRIM(LNM0003.DEPTRUSTEECD) AS DEPTRUSTEECD ")
        SQLStr.AppendLine("   ,RTRIM(LNM0003.DEPTRUSTEESUBCD) AS DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("   ,RTRIM(LNM0003.DEPTRUSTEENM) AS DEPTRUSTEENM ")
        SQLStr.AppendLine("   ,RTRIM(LNM0003.DEPTRUSTEESUBNM) AS DEPTRUSTEESUBNM ")
        SQLStr.AppendLine(" FROM LNG.LNM0003_REKEJM LNM0003")
        SQLStr.AppendLine(" LEFT JOIN COM.LNS0020_STATION LNS0020")
        SQLStr.AppendLine("   ON LNM0003.DEPSTATION = LNS0020.STATION")
        SQLStr.AppendLine("  AND LNS0020.DELFLG <> @DELFLG")
        SQLStr.AppendLine("  AND LNS0020.CAMPCODE = @CAMPCODE")
        SQLStr.AppendLine(" WHERE LNM0003.DELFLG <> @DELFLG ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      LNM0003.DEPSTATION")
        SQLStr.AppendLine("     ,LNM0003.DEPTRUSTEECD")
        SQLStr.AppendLine("     ,LNM0003.DEPTRUSTEESUBCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE
                '会社コード
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 2)
                P_CAMPCODE.Value = Master.USERCAMP

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_ROW As Integer = 0
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_SHEET.Cells(WW_ROW, 0).Value = "発駅コード" '1列目
                        WW_SHEET.Cells(WW_ROW, 1).Value = "発受託人コード" '2列目
                        WW_SHEET.Cells(WW_ROW, 2).Value = "発受託人サブコード" '3列目
                        WW_SHEET.Cells(WW_ROW, 3).Value = "発駅名称" '4列目
                        WW_SHEET.Cells(WW_ROW, 4).Value = "発受託人名称" '5列目
                        WW_SHEET.Cells(WW_ROW, 5).Value = "発受託人サブ名称" '6列目
                        WW_ROW += 1
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_SHEET.Cells(WW_ROW, 0).Value = Row("DEPSTATION") '1列目
                            WW_SHEET.Cells(WW_ROW, 1).Value = Row("DEPTRUSTEECD") '2列目
                            WW_SHEET.Cells(WW_ROW, 2).Value = Row("DEPTRUSTEESUBCD") '3列目
                            WW_SHEET.Cells(WW_ROW, 3).Value = Row("DEPSTATIONNM") '4列目
                            WW_SHEET.Cells(WW_ROW, 4).Value = Row("DEPTRUSTEENM") '5列目
                            WW_SHEET.Cells(WW_ROW, 5).Value = Row("DEPTRUSTEESUBNM") '6列目

                            WW_ROW += 1
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003_REKEJM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003_REKEJM Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
    End Sub

    ''' <summary>
    ''' 入力一覧作成(発荷主一覧)
    ''' </summary>
    Protected Sub SETSHIPPERLIST(ByVal SQLcon As MySqlConnection,
                                   ByVal WW_SHEET As IWorksheet)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(SHIPPERCD) AS SHIPPERCD ")
        SQLStr.AppendLine("   ,RTRIM(NAME) AS NAME ")
        SQLStr.AppendLine(" FROM LNG.LNM0023_SHIPPER ")
        SQLStr.AppendLine(" WHERE DELFLG <> @DELFLG ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      SHIPPERCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_ROW As Integer = 0
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_SHEET.Cells(WW_ROW, 0).Value = "荷主コード" '1列目
                        WW_SHEET.Cells(WW_ROW, 1).Value = "荷主名称" '2列目
                        WW_ROW += 1
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_SHEET.Cells(WW_ROW, 0).Value = Row("SHIPPERCD") '1列目
                            WW_SHEET.Cells(WW_ROW, 1).Value = Row("NAME") '2列目

                            WW_ROW += 1
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0023_SHIPPER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0023_SHIPPER Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
    End Sub

    ''' <summary>
    ''' 入力一覧作成(ＪＲ品目一覧)
    ''' </summary>
    Protected Sub SETITEMLIST(ByVal SQLcon As MySqlConnection,
                                   ByVal WW_SHEET As IWorksheet)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(ITEMCD) AS ITEMCD ")
        SQLStr.AppendLine("   ,RTRIM(NAME) AS NAME ")
        SQLStr.AppendLine(" FROM LNG.LNM0021_ITEM ")
        SQLStr.AppendLine(" WHERE DELFLG <> @DELFLG ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      ITEMCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_ROW As Integer = 0
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_SHEET.Cells(WW_ROW, 0).Value = "品目コード" '1列目
                        WW_SHEET.Cells(WW_ROW, 1).Value = "品目名称" '2列目
                        WW_ROW += 1
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_SHEET.Cells(WW_ROW, 0).Value = Row("ITEMCD") '1列目
                            WW_SHEET.Cells(WW_ROW, 1).Value = Row("NAME") '2列目

                            WW_ROW += 1
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0021_ITEM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0021_ITEM Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
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
                Case "STACKFREEKBN",            '積空区分
                     "OPERATIONKBN",        '状態区分
                     "DELFLG"           '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                Case "ORG"                '組織コード
                    WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG

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
#End Region

#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\コード変換特例マスタ２一括アップロードテスト.xlsx"

        Dim DATENOW As DateTime
        Dim WW_ErrData As Boolean = False

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ERR_SW)
            If WW_ERR_SW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            DATENOW = Date.Now
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")
            For Each Row As DataRow In LNM0008Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0008WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0008WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    RECT2MEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "コード変換特例マスタ２の更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0008Exceltbl) Then
            LNM0008Exceltbl = New DataTable
        End If
        If LNM0008Exceltbl.Columns.Count <> 0 Then
            LNM0008Exceltbl.Columns.Clear()
        End If
        LNM0008Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\RECT2MEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "RECT2MEXCEL_TMP_"

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
            SetExceltbl(SQLcon, filePath, WW_ERR_SW)
            If WW_ERR_SW = "ERR" Then
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

            For Each Row As DataRow In LNM0008Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0008WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0008WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    RECT2MEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.NEWDATA '新規の場合
                            WW_UplInsCnt += 1
                        Case Else
                            WW_UplUpdCnt += 1
                    End Select

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
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
        SQLStr.AppendLine(" SELECT TOP 0")
        SQLStr.AppendLine("   0   AS LINECNT ")
        SQLStr.AppendLine("        ,ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,STACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0008_RECT2M ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0008Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_RECT2M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0008_RECT2M Select"
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

        Dim LNM0008Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0008Exceltblrow = LNM0008Exceltbl.NewRow

            'LINECNT
            LNM0008Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.ORGCODE))
            WW_DATATYPE = DataTypeHT("ORGCODE")
            LNM0008Exceltblrow("ORGCODE") = LNM0008WRKINC.DataConvert("組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '大分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.BIGCTNCD))
            WW_DATATYPE = DataTypeHT("BIGCTNCD")
            LNM0008Exceltblrow("BIGCTNCD") = LNM0008WRKINC.DataConvert("大分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '中分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.MIDDLECTNCD))
            WW_DATATYPE = DataTypeHT("MIDDLECTNCD")
            LNM0008Exceltblrow("MIDDLECTNCD") = LNM0008WRKINC.DataConvert("中分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '使用目的
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.PURPOSE))
            WW_DATATYPE = DataTypeHT("PURPOSE")
            LNM0008Exceltblrow("PURPOSE") = LNM0008WRKINC.DataConvert("使用目的", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '積空区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.STACKFREEKBN))
            WW_DATATYPE = DataTypeHT("STACKFREEKBN")
            LNM0008Exceltblrow("STACKFREEKBN") = LNM0008WRKINC.DataConvert("積空区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-発受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEECD))
            WW_DATATYPE = DataTypeHT("SPRDEPTRUSTEECD")
            LNM0008Exceltblrow("SPRDEPTRUSTEECD") = LNM0008WRKINC.DataConvert("特例置換項目-発受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-発受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("SPRDEPTRUSTEESUBCD")
            LNM0008Exceltblrow("SPRDEPTRUSTEESUBCD") = LNM0008WRKINC.DataConvert("特例置換項目-発受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-発受託人サブゼロ変換区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPTRUSTEESUBZKBN))
            WW_DATATYPE = DataTypeHT("SPRDEPTRUSTEESUBZKBN")
            LNM0008Exceltblrow("SPRDEPTRUSTEESUBZKBN") = LNM0008WRKINC.DataConvert("特例置換項目-発受託人サブゼロ変換区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-発荷主コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRDEPSHIPPERCD))
            WW_DATATYPE = DataTypeHT("SPRDEPSHIPPERCD")
            LNM0008Exceltblrow("SPRDEPSHIPPERCD") = LNM0008WRKINC.DataConvert("特例置換項目-発荷主コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-着受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEECD))
            WW_DATATYPE = DataTypeHT("SPRARRTRUSTEECD")
            LNM0008Exceltblrow("SPRARRTRUSTEECD") = LNM0008WRKINC.DataConvert("特例置換項目-着受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-着受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("SPRARRTRUSTEESUBCD")
            LNM0008Exceltblrow("SPRARRTRUSTEESUBCD") = LNM0008WRKINC.DataConvert("特例置換項目-着受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-着受託人サブゼロ変換区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRARRTRUSTEESUBZKBN))
            WW_DATATYPE = DataTypeHT("SPRARRTRUSTEESUBZKBN")
            LNM0008Exceltblrow("SPRARRTRUSTEESUBZKBN") = LNM0008WRKINC.DataConvert("特例置換項目-着受託人サブゼロ変換区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-ＪＲ品目コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRJRITEMCD))
            WW_DATATYPE = DataTypeHT("SPRJRITEMCD")
            LNM0008Exceltblrow("SPRJRITEMCD") = LNM0008WRKINC.DataConvert("特例置換項目-ＪＲ品目コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-積空区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTACKFREEKBN))
            WW_DATATYPE = DataTypeHT("SPRSTACKFREEKBN")
            LNM0008Exceltblrow("SPRSTACKFREEKBN") = LNM0008WRKINC.DataConvert("特例置換項目-積空区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-状態区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.SPRSTATUSKBN))
            WW_DATATYPE = DataTypeHT("SPRSTATUSKBN")
            LNM0008Exceltblrow("SPRSTATUSKBN") = LNM0008WRKINC.DataConvert("特例置換項目-状態区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.BEFOREORGCODE))
            WW_DATATYPE = DataTypeHT("BEFOREORGCODE")
            LNM0008Exceltblrow("BEFOREORGCODE") = LNM0008WRKINC.DataConvert("変換前組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0008WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0008Exceltblrow("DELFLG") = LNM0008WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0008Exceltbl.Rows.Add(LNM0008Exceltblrow)

        Next
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0008_RECT2M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(BIGCTNCD, '')             = @BIGCTNCD ")
        SQLStr.AppendLine("    AND  coalesce(MIDDLECTNCD, '')             = @MIDDLECTNCD ")
        SQLStr.AppendLine("    AND  coalesce(PURPOSE, '')             = @PURPOSE ")
        SQLStr.AppendLine("    AND  coalesce(STACKFREEKBN, '')             = @STACKFREEKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRDEPTRUSTEECD, '0')             = @SPRDEPTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(SPRDEPTRUSTEESUBCD, '0')             = @SPRDEPTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(SPRDEPTRUSTEESUBZKBN, '0')             = @SPRDEPTRUSTEESUBZKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRDEPSHIPPERCD, '0')             = @SPRDEPSHIPPERCD ")
        SQLStr.AppendLine("    AND  coalesce(SPRARRTRUSTEECD, '0')             = @SPRARRTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(SPRARRTRUSTEESUBCD, '0')             = @SPRARRTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(SPRARRTRUSTEESUBZKBN, '0')             = @SPRARRTRUSTEESUBZKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRJRITEMCD, '0')             = @SPRJRITEMCD ")
        SQLStr.AppendLine("    AND  coalesce(SPRSTACKFREEKBN, '0')             = @SPRSTACKFREEKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRSTATUSKBN, '0')             = @SPRSTATUSKBN ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREORGCODE, '')             = @BEFOREORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_STACKFREEKBN As MySqlParameter = SQLcmd.Parameters.Add("@STACKFREEKBN", MySqlDbType.VarChar, 1)         '積空区分
                Dim P_SPRDEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEECD", MySqlDbType.VarChar, 5)         '特例置換項目-発受託人コード
                Dim P_SPRDEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '特例置換項目-発受託人サブコード
                Dim P_SPRDEPTRUSTEESUBZKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEESUBZKBN", MySqlDbType.VarChar, 1)         '特例置換項目-発受託人サブゼロ変換区分
                Dim P_SPRDEPSHIPPERCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPSHIPPERCD", MySqlDbType.VarChar, 6)         '特例置換項目-発荷主コード
                Dim P_SPRARRTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEECD", MySqlDbType.VarChar, 5)         '特例置換項目-着受託人コード
                Dim P_SPRARRTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '特例置換項目-着受託人サブコード
                Dim P_SPRARRTRUSTEESUBZKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEESUBZKBN", MySqlDbType.VarChar, 1)         '特例置換項目-着受託人サブゼロ変換区分
                Dim P_SPRJRITEMCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRJRITEMCD", MySqlDbType.VarChar, 6)         '特例置換項目-ＪＲ品目コード
                Dim P_SPRSTACKFREEKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRSTACKFREEKBN", MySqlDbType.VarChar, 1)         '特例置換項目-積空区分
                Dim P_SPRSTATUSKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRSTATUSKBN", MySqlDbType.VarChar, 2)         '特例置換項目-状態区分
                Dim P_BEFOREORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREORGCODE", MySqlDbType.VarChar, 6)         '変換前組織コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_PURPOSE.Value = WW_ROW("PURPOSE")               '使用目的
                P_STACKFREEKBN.Value = WW_ROW("STACKFREEKBN")               '積空区分
                P_SPRDEPTRUSTEECD.Value = WW_ROW("SPRDEPTRUSTEECD")               '特例置換項目-発受託人コード
                P_SPRDEPTRUSTEESUBCD.Value = WW_ROW("SPRDEPTRUSTEESUBCD")               '特例置換項目-発受託人サブコード
                P_SPRDEPTRUSTEESUBZKBN.Value = WW_ROW("SPRDEPTRUSTEESUBZKBN")               '特例置換項目-発受託人サブゼロ変換区分
                P_SPRDEPSHIPPERCD.Value = WW_ROW("SPRDEPSHIPPERCD")               '特例置換項目-発荷主コード
                P_SPRARRTRUSTEECD.Value = WW_ROW("SPRARRTRUSTEECD")               '特例置換項目-着受託人コード
                P_SPRARRTRUSTEESUBCD.Value = WW_ROW("SPRARRTRUSTEESUBCD")               '特例置換項目-着受託人サブコード
                P_SPRARRTRUSTEESUBZKBN.Value = WW_ROW("SPRARRTRUSTEESUBZKBN")               '特例置換項目-着受託人サブゼロ変換区分
                P_SPRJRITEMCD.Value = WW_ROW("SPRJRITEMCD")               '特例置換項目-ＪＲ品目コード
                P_SPRSTACKFREEKBN.Value = WW_ROW("SPRSTACKFREEKBN")               '特例置換項目-積空区分
                P_SPRSTATUSKBN.Value = WW_ROW("SPRSTATUSKBN")               '特例置換項目-状態区分
                P_BEFOREORGCODE.Value = WW_ROW("BEFOREORGCODE")               '変換前組織コード
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_RECT2M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0008_RECT2M SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try
        SameDataChk = True
    End Function

    ''' <summary>
    ''' Excelデータ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdExcelData(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)
        WW_ERR_SW = C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0008_RECT2M LNM0008")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @ORGCODE AS ORGCODE ")
        SQLStr.AppendLine("             ,@BIGCTNCD AS BIGCTNCD ")
        SQLStr.AppendLine("             ,@MIDDLECTNCD AS MIDDLECTNCD ")
        SQLStr.AppendLine("             ,@PURPOSE AS PURPOSE ")
        SQLStr.AppendLine("             ,@STACKFREEKBN AS STACKFREEKBN ")
        SQLStr.AppendLine("             ,@SPRDEPTRUSTEECD AS SPRDEPTRUSTEECD ")
        SQLStr.AppendLine("             ,@SPRDEPTRUSTEESUBCD AS SPRDEPTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@SPRDEPTRUSTEESUBZKBN AS SPRDEPTRUSTEESUBZKBN ")
        SQLStr.AppendLine("             ,@SPRDEPSHIPPERCD AS SPRDEPSHIPPERCD ")
        SQLStr.AppendLine("             ,@SPRARRTRUSTEECD AS SPRARRTRUSTEECD ")
        SQLStr.AppendLine("             ,@SPRARRTRUSTEESUBCD AS SPRARRTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@SPRARRTRUSTEESUBZKBN AS SPRARRTRUSTEESUBZKBN ")
        SQLStr.AppendLine("             ,@SPRJRITEMCD AS SPRJRITEMCD ")
        SQLStr.AppendLine("             ,@SPRSTACKFREEKBN AS SPRSTACKFREEKBN ")
        SQLStr.AppendLine("             ,@SPRSTATUSKBN AS SPRSTATUSKBN ")
        SQLStr.AppendLine("             ,@BEFOREORGCODE AS BEFOREORGCODE ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0008.ORGCODE = EXCEL.ORGCODE ")
        SQLStr.AppendLine("         AND LNM0008.BIGCTNCD = EXCEL.BIGCTNCD ")
        SQLStr.AppendLine("         AND LNM0008.MIDDLECTNCD = EXCEL.MIDDLECTNCD ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0008.PURPOSE =  EXCEL.PURPOSE")
        SQLStr.AppendLine("         ,LNM0008.STACKFREEKBN =  EXCEL.STACKFREEKBN")
        SQLStr.AppendLine("         ,LNM0008.SPRDEPTRUSTEECD =  EXCEL.SPRDEPTRUSTEECD")
        SQLStr.AppendLine("         ,LNM0008.SPRDEPTRUSTEESUBCD =  EXCEL.SPRDEPTRUSTEESUBCD")
        SQLStr.AppendLine("         ,LNM0008.SPRDEPTRUSTEESUBZKBN =  EXCEL.SPRDEPTRUSTEESUBZKBN")
        SQLStr.AppendLine("         ,LNM0008.SPRDEPSHIPPERCD =  EXCEL.SPRDEPSHIPPERCD")
        SQLStr.AppendLine("         ,LNM0008.SPRARRTRUSTEECD =  EXCEL.SPRARRTRUSTEECD")
        SQLStr.AppendLine("         ,LNM0008.SPRARRTRUSTEESUBCD =  EXCEL.SPRARRTRUSTEESUBCD")
        SQLStr.AppendLine("         ,LNM0008.SPRARRTRUSTEESUBZKBN =  EXCEL.SPRARRTRUSTEESUBZKBN")
        SQLStr.AppendLine("         ,LNM0008.SPRJRITEMCD =  EXCEL.SPRJRITEMCD")
        SQLStr.AppendLine("         ,LNM0008.SPRSTACKFREEKBN =  EXCEL.SPRSTACKFREEKBN")
        SQLStr.AppendLine("         ,LNM0008.SPRSTATUSKBN =  EXCEL.SPRSTATUSKBN")
        SQLStr.AppendLine("         ,LNM0008.BEFOREORGCODE =  EXCEL.BEFOREORGCODE")
        SQLStr.AppendLine("         ,LNM0008.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0008.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0008.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0008.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0008.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,STACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @ORGCODE  ")
        SQLStr.AppendLine("        ,@BIGCTNCD  ")
        SQLStr.AppendLine("        ,@MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,@PURPOSE  ")
        SQLStr.AppendLine("        ,@STACKFREEKBN  ")
        SQLStr.AppendLine("        ,@SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,@SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,@SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,@SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,@SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,@SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,@SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,@SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,@BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_STACKFREEKBN As MySqlParameter = SQLcmd.Parameters.Add("@STACKFREEKBN", MySqlDbType.VarChar, 1)         '積空区分
                Dim P_SPRDEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEECD", MySqlDbType.VarChar, 5)         '特例置換項目-発受託人コード
                Dim P_SPRDEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '特例置換項目-発受託人サブコード
                Dim P_SPRDEPTRUSTEESUBZKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPTRUSTEESUBZKBN", MySqlDbType.VarChar, 1)         '特例置換項目-発受託人サブゼロ変換区分
                Dim P_SPRDEPSHIPPERCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRDEPSHIPPERCD", MySqlDbType.VarChar, 6)         '特例置換項目-発荷主コード
                Dim P_SPRARRTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEECD", MySqlDbType.VarChar, 5)         '特例置換項目-着受託人コード
                Dim P_SPRARRTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '特例置換項目-着受託人サブコード
                Dim P_SPRARRTRUSTEESUBZKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRTRUSTEESUBZKBN", MySqlDbType.VarChar, 1)         '特例置換項目-着受託人サブゼロ変換区分
                Dim P_SPRJRITEMCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRJRITEMCD", MySqlDbType.VarChar, 6)         '特例置換項目-ＪＲ品目コード
                Dim P_SPRSTACKFREEKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRSTACKFREEKBN", MySqlDbType.VarChar, 1)         '特例置換項目-積空区分
                Dim P_SPRSTATUSKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRSTATUSKBN", MySqlDbType.VarChar, 2)         '特例置換項目-状態区分
                Dim P_BEFOREORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREORGCODE", MySqlDbType.VarChar, 6)         '変換前組織コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                'DB更新
                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                '使用目的
                If Not WW_ROW("PURPOSE") = "" Then
                    P_PURPOSE.Value = WW_ROW("PURPOSE")
                Else
                    P_PURPOSE.Value = DBNull.Value
                End If
                '積空区分
                If Not WW_ROW("STACKFREEKBN") = "" Then
                    P_STACKFREEKBN.Value = WW_ROW("STACKFREEKBN")
                Else
                    P_STACKFREEKBN.Value = DBNull.Value
                End If
                '特例置換項目-発受託人コード
                P_SPRDEPTRUSTEECD.Value = WW_ROW("SPRDEPTRUSTEECD")

                '特例置換項目-発受託人サブコード
                P_SPRDEPTRUSTEESUBCD.Value = WW_ROW("SPRDEPTRUSTEESUBCD")

                '特例置換項目-発受託人サブゼロ変換区分
                P_SPRDEPTRUSTEESUBZKBN.Value = WW_ROW("SPRDEPTRUSTEESUBZKBN")

                '特例置換項目-発荷主コード
                P_SPRDEPSHIPPERCD.Value = WW_ROW("SPRDEPSHIPPERCD")

                '特例置換項目-着受託人コード
                P_SPRARRTRUSTEECD.Value = WW_ROW("SPRARRTRUSTEECD")

                '特例置換項目-着受託人サブコード
                P_SPRARRTRUSTEESUBCD.Value = WW_ROW("SPRARRTRUSTEESUBCD")

                '特例置換項目-着受託人サブゼロ変換区分
                P_SPRARRTRUSTEESUBZKBN.Value = WW_ROW("SPRARRTRUSTEESUBZKBN")

                '特例置換項目-ＪＲ品目コード
                P_SPRJRITEMCD.Value = WW_ROW("SPRJRITEMCD")

                '特例置換項目-積空区分
                P_SPRSTACKFREEKBN.Value = WW_ROW("SPRSTACKFREEKBN")

                '特例置換項目-状態区分
                P_SPRSTATUSKBN.Value = WW_ROW("SPRSTATUSKBN")

                '変換前組織コード
                If Not WW_ROW("BEFOREORGCODE") = "" Then
                    P_BEFOREORGCODE.Value = WW_ROW("BEFOREORGCODE")
                Else
                    P_BEFOREORGCODE.Value = DBNull.Value
                End If

                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                P_INITYMD.Value = WW_DATENOW                '登録年月日
                P_INITUSER.Value = Master.USERID               '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID               '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_RECT2M  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0008_RECT2M  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR
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
        Dim WW_SlcStMD As String = ""
        Dim WW_SlcEndMD As String = ""

        WW_LineErr = ""

        ' 削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラーです"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 組織コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ORG", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("ORG", WW_ROW("ORGCODE"), WW_Dummy, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・組織コード入力エラー"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・組織コード入力エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 大分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIGCTNCD", WW_ROW("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("BIGCTNCD", WW_ROW("BIGCTNCD"), WW_Dummy, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・大分類コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・大分類コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 中分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", WW_ROW("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("MIDDLECTNCD", WW_ROW("MIDDLECTNCD"), WW_ROW("BIGCTNCD"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・中分類コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・中分類コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 使用目的(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PURPOSE", WW_ROW("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・使用目的エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 積空区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "STACKFREEKBN", WW_ROW("STACKFREEKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("STACKFREEKBN")) Then
                ' 名称存在チェック
                CODENAME_get("STACKFREEKBN", WW_ROW("STACKFREEKBN"), WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・積空区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・積空区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 特例置換項目-発受託人コード(バリデーションチェック)
        If Not WW_ROW("SPRDEPTRUSTEECD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEECD", WW_ROW("SPRDEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-発受託人サブコード(バリデーションチェック)
        If Not WW_ROW("SPRDEPTRUSTEESUBCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEESUBCD", WW_ROW("SPRDEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-発受託人サブゼロ変換区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEESUBZKBN", WW_ROW("SPRDEPTRUSTEESUBZKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) OrElse CInt(WW_ROW("SPRDEPTRUSTEESUBZKBN")) > 1 Then
            WW_CheckMES1 = "・特例置換項目-発受託人サブゼロ変換区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 特例置換項目-発荷主コード(バリデーションチェック)
        If Not WW_ROW("SPRDEPSHIPPERCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRDEPSHIPPERCD", WW_ROW("SPRDEPSHIPPERCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SPRDEPSHIPPERCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SPRDEPSHIPPERCD"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-発荷主コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-発荷主コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-着受託人コード(バリデーションチェック)
        If Not WW_ROW("SPRARRTRUSTEECD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEECD", WW_ROW("SPRARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-着受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-着受託人サブコード(バリデーションチェック)
        If Not WW_ROW("SPRARRTRUSTEESUBCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEESUBCD", WW_ROW("SPRARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-着受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-着受託人サブゼロ変換区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEESUBZKBN", WW_ROW("SPRARRTRUSTEESUBZKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) OrElse CInt(WW_ROW("SPRARRTRUSTEESUBZKBN")) > 1 Then
                WW_CheckMES1 = "・特例置換項目-着受託人サブゼロ変換区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-ＪＲ品目コード(バリデーションチェック)
        If Not WW_ROW("SPRJRITEMCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRJRITEMCD", WW_ROW("SPRJRITEMCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SPRJRITEMCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SPRJRITEMCD"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-ＪＲ品目コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-ＪＲ品目コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-積空区分(バリデーションチェック)
        If Not WW_ROW("SPRSTACKFREEKBN") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRSTACKFREEKBN", WW_ROW("SPRSTACKFREEKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SPRSTACKFREEKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("STACKFREEKBN", WW_ROW("SPRSTACKFREEKBN"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-積空区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-積空区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-状態区分(バリデーションチェック)
        If Not WW_ROW("SPRSTATUSKBN") = "0" Then
            Master.CheckField(Master.USERCAMP, "SPRSTATUSKBN", WW_ROW("SPRSTATUSKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SPRSTATUSKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("OPERATIONKBN", WW_ROW("SPRSTATUSKBN"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-状態区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-状態区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
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
    Protected Sub RECT2MEXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        'コード変換特例マスタ２に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0008_RECT2M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード

                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    '更新の場合(データが存在した場合)は変更区分に変更前をセット、更新前の削除フラグを取得する
                    If WW_Tbl.Rows.Count > 0 Then
                        WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0008_RECT2M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0008_RECT2M Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0113_RECT2HIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,STACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,OPERATEKBN  ")
        SQLStr.AppendLine("        ,MODIFYKBN  ")
        SQLStr.AppendLine("        ,MODIFYYMD  ")
        SQLStr.AppendLine("        ,MODIFYUSER  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("  SELECT  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,STACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,@OPERATEKBN AS OPERATEKBN ")
        SQLStr.AppendLine("        ,@MODIFYKBN AS MODIFYKBN ")
        SQLStr.AppendLine("        ,@MODIFYYMD AS MODIFYYMD ")
        SQLStr.AppendLine("        ,@MODIFYUSER AS MODIFYUSER ")
        SQLStr.AppendLine("        ,DELFLG ")
        SQLStr.AppendLine("        ,@INITYMD AS INITYMD ")
        SQLStr.AppendLine("        ,@INITUSER AS INITUSER ")
        SQLStr.AppendLine("        ,@INITTERMID AS INITTERMID ")
        SQLStr.AppendLine("        ,@INITPGID AS INITPGID ")
        SQLStr.AppendLine("  FROM   ")
        SQLStr.AppendLine("        LNG.LNM0008_RECT2M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0008WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0008WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0008WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0008WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0113_RECT2HIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0113_RECT2HIST  INSERT"
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

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE1"></param>
    ''' <param name="I_VALUE2"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByVal I_VALUE1 As String,
                               ByVal I_VALUE2 As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE1) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "ORG"                '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"           '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"        '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, I_VALUE2))
                Case "STACKFREEKBN",      '積空区分
                     "OPERATIONKBN"       '状態区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "SHIPPER"            '荷主コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPER, CInt(I_VALUE1), O_TEXT, O_RTN)
                Case "ITEM"               '品目コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ITEM, CInt(I_VALUE1), O_TEXT, O_RTN)

                Case "OUTPUTID"           '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"              '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

