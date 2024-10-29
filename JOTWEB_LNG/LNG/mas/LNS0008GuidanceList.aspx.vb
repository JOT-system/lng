''************************************************************
' ガイダンスマスタメンテ一覧画面
' 作成日 2022/02/28
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/02/28 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient

''' <summary>
''' ガイダンスマスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNS0008GuidanceList
    Inherits Page

    '○ 検索結果格納Table
    Private LNS0008tbl As DataTable                                 '一覧格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

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
                    Master.RecoverTable(LNS0008tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
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
            If Not IsNothing(LNS0008tbl) Then
                LNS0008tbl.Clear()
                LNS0008tbl.Dispose()
                LNS0008tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNS0008WRKINC.MAPIDL
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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0008S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0008D Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

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
        Master.SaveTable(LNS0008tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNS0008tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNS0008tbl)

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

        If IsNothing(LNS0008tbl) Then
            LNS0008tbl = New DataTable
        End If

        If LNS0008tbl.Columns.Count <> 0 Then
            LNS0008tbl.Columns.Clear()
        End If

        LNS0008tbl.Clear()

        ' 前画面より選択した対象フラグクラスを復元
        Dim flagList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)
        ' 対象フラグよりチェックしたもののみを抜き出す
        Dim selectedList = (From itm In flagList Where itm.Checked).ToList

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをガイダンスマスタから取得する
        Dim SQLStr As String =
              " Select                                                                                                                " _
            & "     1                                                                                               AS 'SELECT'       " _
            & "   , 0                                                                                               AS HIDDEN         " _
            & "   , 0                                                                                               AS LINECNT        " _
            & "   , ''                                                                                              AS OPERATION      " _
            & "   , LNS0008.UPDTIMSTP                                                                               AS UPDTIMSTP      " _
            & "   , coalesce(RTRIM(LNS0008.DELFLG), '')                                                             AS DELFLG         " _
            & "   , coalesce(RTRIM(LNS0008.GUIDANCENO), '')                                                         AS GUIDANCENO     " _
            & "   , coalesce(DATE_FORMAT(LNS0008.FROMYMD, '%Y/%m/%d'), '')                                          AS FROMYMD        " _
            & "   , coalesce(DATE_FORMAT(LNS0008.ENDYMD, '%Y/%m/%d'), '')                                           AS ENDYMD         " _
            & "   , CONCAT('<div class=""type', LNS0008.TYPE, '"" ></div> ')                                          AS DISPTYPE       " _
            & "   , coalesce(RTRIM(LNS0008.TITLE), '')                                                                AS TITLE          " _
            & "   , coalesce(RTRIM(LNS0008.OUTFLG), '')                                                               AS OUTFLG         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG1), '')                                                               AS INFLG1         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG2), '')                                                               AS INFLG2         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG3), '')                                                               AS INFLG3         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG4), '')                                                               AS INFLG4         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG5), '')                                                               AS INFLG5         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG6), '')                                                               AS INFLG6         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG7), '')                                                               AS INFLG7         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG8), '')                                                               AS INFLG8         " _
            & "   , CASE WHEN LNS0008.OUTFLG='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPOUTFLG     " _
            & "   , CASE WHEN LNS0008.INFLG1='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG1     " _
            & "   , CASE WHEN LNS0008.INFLG2='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG2     " _
            & "   , CASE WHEN LNS0008.INFLG3='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG3     " _
            & "   , CASE WHEN LNS0008.INFLG4='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG4     " _
            & "   , CASE WHEN LNS0008.INFLG5='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG5     " _
            & "   , CASE WHEN LNS0008.INFLG6='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG6     " _
            & "   , CASE WHEN LNS0008.INFLG7='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG7     " _
            & "   , CASE WHEN LNS0008.INFLG8='1'  THEN '<div class=""checked""></div>' ELSE '' END                  AS DISPINFLG8     " _
            & "   , coalesce(RTRIM(LNS0008.NAIYOU), '')                                                               AS NAIYOU         " _
            & "   , coalesce(RTRIM(LNS0008.FILE1), '')                                                                AS FILE1          " _
            & "   , coalesce(RTRIM(LNS0008.FILE2), '')                                                                AS FILE2          " _
            & "   , coalesce(RTRIM(LNS0008.FILE3), '')                                                                AS FILE3          " _
            & "   , coalesce(RTRIM(LNS0008.FILE4), '')                                                                AS FILE4          " _
            & "   , coalesce(RTRIM(LNS0008.FILE5), '')                                                                AS FILE5          " _
            & "   , CASE WHEN coalesce(LNS0008.FILE1,'') <> '' THEN '<div class=""hasAttachment""></div>' ELSE '' END AS HASATTACHMENT  " _
            & "   , coalesce(DATE_FORMAT(LNS0008.INITYMD, '%Y/%m/%d %H:%i:%s'), '')                                   AS INITYMD        " _
            & "   , coalesce(DATE_FORMAT(LNS0008.UPDYMD, '%Y/%m/%dd %H:%i:%s'), '')                                   AS UPDYMD         " _
            & " FROM                                                                                                                  " _
            & "     COM.LNS0008_GUIDANCE LNS0008                                                                                      "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 掲載終了日
        If Not String.IsNullOrEmpty(work.WF_SEL_FROMYMD.Text) Then
            SQLWhereStr = " WHERE                      " _
                        & "     LNS0008.FROMYMD <= @P1 "
        End If
        ' 掲載終了日
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                         " _
                            & "     LNS0008.ENDYMD >= @P2     "
            Else
                SQLWhereStr &= "    AND LNS0008.ENDYMD >= @P2 "
            End If
        End If
        ' 対象フラグ
        If selectedList IsNot Nothing AndAlso selectedList.Count > 0 Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE ("
            Else
                SQLWhereStr &= " AND ("
            End If
        End If
        Dim isFirst = True
        For Each selectedFlg In selectedList
            If Not isFirst Then
                SQLWhereStr &= "    OR "
            End If
            isFirst = False
            SQLWhereStr &= " LNS0008." & selectedFlg.FieldName & " = '1' "
        Next
        If selectedList IsNot Nothing AndAlso selectedList.Count > 0 Then
            SQLWhereStr &= "   )"
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNS0008.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNS0008.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY                " _
            & "     LNS0008.GUIDANCENO  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_FROMYMD.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Date)            '掲載開始日
                    PARA1.Value = work.WF_SEL_FROMYMD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)            '掲載終了日
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0008tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0008tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNS0008row As DataRow In LNS0008tbl.Rows
                    i += 1
                    LNS0008row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0008L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0008L Select"
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
        For Each LNS0008row As DataRow In LNS0008tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNS0008tbl)

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
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        work.WF_SEL_GUIDANCENO2.Text = ""                                         'ガイダンス№

        '○ 画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNS0008tbl)

        WF_GridDBclick.Text = ""

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPRINT_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = LNS0008tbl                       'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

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
        Dim TBLview As New DataView(LNS0008tbl)
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

        work.WF_SEL_GUIDANCENO2.Text = LNS0008tbl.Rows(WW_LineCNT)("GUIDANCENO")       'ガイダンスNo.
        work.WF_SEL_TIMESTAMP.Text = LNS0008tbl.Rows(WW_LineCNT)("UPDTIMSTP")          'タイムスタンプ

        '○ 画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNS0008tbl)

        WF_GridDBclick.Text = ""

        '〇 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_GUIDANCENO2.Text) Then  'ガイダンスNo.
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_GUIDANCENO2.Text, work.WF_SEL_TIMESTAMP.Text)
            End Using

            If Not isNormal(WW_DBDataCheck) Then
                Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

    End Sub

End Class

