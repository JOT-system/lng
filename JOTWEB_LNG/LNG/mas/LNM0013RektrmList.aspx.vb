''************************************************************
' 回送運賃適用率マスタメンテ一覧画面
' 作成日 2022/02/18
' 更新日 2024/01/15
' 作成者 瀬口
' 更新者 大浜
'
' 修正履歴 : 2022/02/18 新規作成
'          : 2024/01/15 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 回送運賃適用率マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0013RektrmList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0013tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNM0013Exceltbl As New DataTable                        'Excelデータ格納用テーブル

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
    Private WW_ChkDate As Integer = 0
    Private WW_ChkDate8str As String = ""
    Private WW_ChkDate8ymd As String = ""

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
                    Master.RecoverTable(LNM0013tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0013WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0013WRKINC.FILETYPE.PDF)
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
            If Not IsNothing(LNM0013tbl) Then
                LNM0013tbl.Clear()
                LNM0013tbl.Dispose()
                LNM0013tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0013WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0013S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0013D Then
            Master.RecoverTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNM0013tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0013tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0013tbl)

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

        Dim WW_Dateint As Integer = "0"
        Dim WW_Datestr As String = ""

        If IsNothing(LNM0013tbl) Then
            LNM0013tbl = New DataTable
        End If

        If LNM0013tbl.Columns.Count <> 0 Then
            LNM0013tbl.Columns.Clear()
        End If

        LNM0013tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを回送運賃適用率マスタから取得する
        Dim SQLStr As String =
            " OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotctn; " _
            & " Select " _
            & "     1                                                                          AS 'SELECT'         " _
            & "   , 0                                                                          AS HIDDEN           " _
            & "   , 0                                                                          AS LINECNT          " _
            & "   , ''                                                                         AS OPERATION        " _
            & "   , LNM0013.UPDTIMSTP                                                          AS UPDTIMSTP        " _
            & "   , coalesce(RTRIM(LNM0013.DELFLG), '')                                          AS DELFLG           " _
            & "   , coalesce(RTRIM(LNM0013.BIGCTNCD), '')                                        AS BIGCTNCD         " _
            & "   , coalesce(RTRIM(LNM0013.MIDDLECTNCD), '')                                     AS MIDDLECTNCD      " _
            & "   , coalesce(RTRIM(LNM0013.PRIORITYNO), '')                                      AS PRIORITYNO       " _
            & "   , coalesce(RTRIM(LNM0013.DEPSTATION), '')                                      AS DEPSTATION       " _
            & "   , coalesce(RTRIM(LNM0013.JRDEPBRANCHCD), '')                                   AS JRDEPBRANCHCD    " _
            & "   , coalesce(RTRIM(LNM0013.ARRSTATION), '')                                      AS ARRSTATION       " _
            & "   , coalesce(RTRIM(LNM0013.JRARRBRANCHCD), '')                                   AS JRARRBRANCHCD    " _
            & "   , coalesce(RTRIM(LNM0013.PURPOSE), '')                                         AS PURPOSE          " _
            & "   , coalesce(RTRIM(LNM0013.DEPTRUSTEECD), '')                                    AS DEPTRUSTEECD     " _
            & "   , coalesce(RTRIM(LNM0013.DEPTRUSTEESUBCD), '')                                 AS DEPTRUSTEESUBCD  " _
            & "   , coalesce(RTRIM(LNM0013.CTNTYPE), '')                                         AS CTNTYPE          " _
            & "   , coalesce(RTRIM(LNM0013.CTNSTNO), '')                                         AS CTNSTNO          " _
            & "   , coalesce(RTRIM(LNM0013.CTNENDNO), '')                                        AS CTNENDNO         " _
            & "   , coalesce(RTRIM(LNM0013.SPRCURSTYMD), '')                                     AS SPRCURSTYMD      " _
            & "   , coalesce(RTRIM(LNM0013.SPRCURENDYMD), '')                                    AS SPRCURENDYMD     " _
            & "   , coalesce(RTRIM(LNM0013.SPRCURAPPLYRATE), '')                                 AS SPRCURAPPLYRATE  " _
            & "   , NULLIF(RTRIM(LNM0013.SPRCURROUNDKBN), 0)                                   AS SPRCURROUNDKBN   " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0013.SPRCURROUNDKBN), 0), ''),1,1)        AS SPRCURROUNDKBN1  " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0013.SPRCURROUNDKBN), 0), ''),2,1)        AS SPRCURROUNDKBN2  " _
            & "   , coalesce(RTRIM(LNM0013.SPRNEXTSTYMD), '')                                    AS SPRNEXTSTYMD     " _
            & "   , coalesce(RTRIM(LNM0013.SPRNEXTENDYMD), '')                                   AS SPRNEXTENDYMD    " _
            & "   , coalesce(RTRIM(LNM0013.SPRNEXTAPPLYRATE), '')                                AS SPRNEXTAPPLYRATE " _
            & "   , NULLIF(RTRIM(LNM0013.SPRNEXTROUNDKBN), 0)                                  AS SPRNEXTROUNDKBN  " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0013.SPRNEXTROUNDKBN), 0), ''),1,1)       AS SPRNEXTROUNDKBN1 " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0013.SPRNEXTROUNDKBN), 0), ''),2,1)       AS SPRNEXTROUNDKBN2 " _
            & "   , coalesce(LNM0013.UPDYMD, '')                                                 AS UPDYMD           " _
            & " FROM                                                                                               " _
            & "     LNG.LNM0013_REKTRM LNM0013                                                                     "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する

        Dim SQLWhereStr As String = ""
        ' 大分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_BIGCTNCD.Text) Then
            SQLWhereStr = " WHERE                        " _
                        & "     LNM0013.BIGCTNCD = @P1 "
        End If

        ' 中分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLECTNCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                             " _
                            & "     LNM0013.MIDDLECTNCD = @P2     "
            Else
                SQLWhereStr &= "    AND LNM0013.MIDDLECTNCD = @P2 "
            End If
        End If

        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNM0013.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNM0013.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY" _
            & "    LNM0013.BIGCTNCD" _
            & "  , LNM0013.MIDDLECTNCD"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_BIGCTNCD.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 2)                '大分類コード
                    PARA1.Value = work.WF_SEL_BIGCTNCD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLECTNCD.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2)                '中分類コード
                    PARA2.Value = work.WF_SEL_MIDDLECTNCD.Text
                End If
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 1)                    '削除フラグ

                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0013tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0013tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0013row As DataRow In LNM0013tbl.Rows
                    i += 1
                    LNM0013row("LINECNT") = i                                                'LINECNT

                    If Not String.IsNullOrEmpty(LNM0013row("SPRCURSTYMD")) Then
                        WW_Dateint = Integer.Parse(LNM0013row("SPRCURSTYMD"))
                        If WW_Dateint > 9999 Then
                            WW_Datestr = WW_Dateint.ToString("0000/00/00")
                            LNM0013row("SPRCURSTYMD") = WW_Datestr                           '現行開始適用日
                        End If
                    End If

                    If Not String.IsNullOrEmpty(LNM0013row("SPRCURENDYMD")) Then
                        WW_Dateint = Integer.Parse(LNM0013row("SPRCURENDYMD"))
                        If WW_Dateint > 9999 Then
                            WW_Datestr = WW_Dateint.ToString("0000/00/00")
                            LNM0013row("SPRCURENDYMD") = WW_Datestr                          '現行終了適用日
                        End If
                    End If

                    If Not String.IsNullOrEmpty(LNM0013row("SPRNEXTSTYMD")) Then
                        WW_Dateint = Integer.Parse(LNM0013row("SPRNEXTSTYMD"))
                        If WW_Dateint > 9999 Then
                            WW_Datestr = WW_Dateint.ToString("0000/00/00")
                            LNM0013row("SPRNEXTSTYMD") = WW_Datestr                          '次期開始適用日
                        End If
                    End If

                    If Not String.IsNullOrEmpty(LNM0013row("SPRNEXTENDYMD")) Then
                        WW_Dateint = Integer.Parse(LNM0013row("SPRNEXTENDYMD"))
                        If WW_Dateint > 9999 Then
                            WW_Datestr = WW_Dateint.ToString("0000/00/00")
                            LNM0013row("SPRNEXTENDYMD") = WW_Datestr                         '次期終了適用日
                        End If
                    End If

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013L Select"
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
        For Each LNM0013row As DataRow In LNM0013tbl.Rows
            If LNM0013row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0013row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0013tbl)

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


        work.WF_SEL_LINECNT.Text = ""                           '選択行

        work.WF_SEL_BIGCTNCD2.Text = ""                          '大分類コード
        work.WF_SEL_MIDDLECTNCD.Text = ""                       '中分類コード
        work.WF_SEL_PRIORITYNO.Text = "0"                        '優先順位
        work.WF_SEL_DEPSTATION.Text = "0"                        '発駅コード
        work.WF_SEL_JRDEPBRANCHCD.Text = "0"                     'ＪＲ発支社支店コード
        work.WF_SEL_ARRSTATION.Text = "0"                       '着駅コード
        work.WF_SEL_JRARRBRANCHCD.Text = "0"                     'ＪＲ着支社支店コード
        work.WF_SEL_PURPOSE.Text = ""                           '使用目的
        work.WF_SEL_DEPTRUSTEECD.Text = ""                      '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD.Text = ""                   '発受託人サブコード
        work.WF_SEL_CTNTYPE.Text = ""                           'コンテナ記号
        work.WF_SEL_CTNSTNO.Text = ""                           'コンテナ番号（開始）
        work.WF_SEL_CTNENDNO.Text = ""                          'コンテナ番号（終了）
        work.WF_SEL_SPRCURSTYMD.Text = ""                       '特例置換項目-現行開始適用日
        work.WF_SEL_SPRCURENDYMD.Text = ""                      '特例置換項目-現行終了摘要日
        work.WF_SEL_SPRCURAPPLYRATE.Text = ""                   '特例置換項目-現行摘要率
        work.WF_SEL_SPRCURROUNDKBN.Text = ""                    '特例置換項目-現行端数処理区分
        work.WF_SEL_SPRCURROUNDKBN1.Text = ""                   '特例置換項目-現行端数処理区分1
        work.WF_SEL_SPRCURROUNDKBN2.Text = ""                   '特例置換項目-現行端数処理区分2
        work.WF_SEL_SPRNEXTSTYMD.Text = ""                      '特例置換項目-次期開始適用日
        work.WF_SEL_SPRNEXTENDYMD.Text = ""                     '特例置換項目-次期終了摘要日
        work.WF_SEL_SPRNEXTAPPLYRATE.Text = ""                  '特例置換項目-次期摘要率
        work.WF_SEL_SPRNEXTROUNDKBN.Text = ""                   '特例置換項目-次期端数処理区分
        work.WF_SEL_SPRNEXTROUNDKBN1.Text = ""                  '特例置換項目-次期端数処理区分1
        work.WF_SEL_SPRNEXTROUNDKBN2.Text = ""                  '特例置換項目-次期端数処理区分2

        work.WF_SEL_DELFLG.Text = "0"                           '削除
        work.WF_SEL_UPDYMD.Text = ""         　                 '更新年月日
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""             '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0013tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0013RektrmHistory.aspx")
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
        Dim TBLview As New DataView(LNM0013tbl)
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

        work.WF_SEL_LINECNT.Text = LNM0013tbl.Rows(WW_LineCNT)("LINECNT")                   '選択行

        work.WF_SEL_BIGCTNCD2.Text = LNM0013tbl.Rows(WW_LineCNT)("BIGCTNCD")                '大分類コード
        work.WF_SEL_MIDDLECTNCD2.Text = LNM0013tbl.Rows(WW_LineCNT)("MIDDLECTNCD")          '中分類コード
        work.WF_SEL_PRIORITYNO.Text = LNM0013tbl.Rows(WW_LineCNT)("PRIORITYNO")             '優先順位
        work.WF_SEL_DEPSTATION.Text = LNM0013tbl.Rows(WW_LineCNT)("DEPSTATION")             '発駅コード
        work.WF_SEL_JRDEPBRANCHCD.Text = LNM0013tbl.Rows(WW_LineCNT)("JRDEPBRANCHCD")       'ＪＲ発支社支店コード
        work.WF_SEL_ARRSTATION.Text = LNM0013tbl.Rows(WW_LineCNT)("ARRSTATION")             '着駅コード
        work.WF_SEL_JRARRBRANCHCD.Text = LNM0013tbl.Rows(WW_LineCNT)("JRARRBRANCHCD")       'ＪＲ着支社支店コード
        work.WF_SEL_PURPOSE.Text = LNM0013tbl.Rows(WW_LineCNT)("PURPOSE")                   '使用目的
        work.WF_SEL_DEPTRUSTEECD.Text = LNM0013tbl.Rows(WW_LineCNT)("DEPTRUSTEECD")         '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD.Text = LNM0013tbl.Rows(WW_LineCNT)("DEPTRUSTEESUBCD")   '発受託人サブコード
        work.WF_SEL_CTNTYPE.Text = LNM0013tbl.Rows(WW_LineCNT)("CTNTYPE")                   'コンテナ記号
        work.WF_SEL_CTNSTNO.Text = LNM0013tbl.Rows(WW_LineCNT)("CTNSTNO")                   'コンテナ番号（開始）
        work.WF_SEL_CTNENDNO.Text = LNM0013tbl.Rows(WW_LineCNT)("CTNENDNO")                 'コンテナ番号（終了）
        work.WF_SEL_SPRCURSTYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURSTYMD")           '特例置換項目-現行開始適用日
        work.WF_SEL_SPRCURENDYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURENDYMD")         '特例置換項目-現行終了摘要日
        work.WF_SEL_SPRCURAPPLYRATE.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURAPPLYRATE")   '特例置換項目-現行摘要率

        work.WF_SEL_SPRCURROUNDKBN.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURROUNDKBN")     '特例置換項目-現行端数処理区分
        work.WF_SEL_SPRCURROUNDKBN1.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURROUNDKBN1")   '特例置換項目-現行端数処理区分1
        work.WF_SEL_SPRCURROUNDKBN2.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRCURROUNDKBN2")   '特例置換項目-現行端数処理区分2

        work.WF_SEL_SPRNEXTSTYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTSTYMD")         '特例置換項目-次期開始適用日
        work.WF_SEL_SPRNEXTENDYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTENDYMD")       '特例置換項目-次期終了摘要日
        work.WF_SEL_SPRNEXTAPPLYRATE.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTAPPLYRATE") '特例置換項目-次期摘要率

        work.WF_SEL_SPRNEXTROUNDKBN.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTROUNDKBN")   '特例置換項目-次期端数処理区分
        work.WF_SEL_SPRNEXTROUNDKBN1.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTROUNDKBN1") '特例置換項目-次期端数処理区分1
        work.WF_SEL_SPRNEXTROUNDKBN2.Text = LNM0013tbl.Rows(WW_LineCNT)("SPRNEXTROUNDKBN2") '特例置換項目-次期端数処理区分2

        work.WF_SEL_DELFLG.Text = LNM0013tbl.Rows(WW_LineCNT)("DELFLG")                     '削除
        work.WF_SEL_UPDYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("UPDYMD")                     '更新年月日
        work.WF_SEL_UPDYMD.Text = LNM0013tbl.Rows(WW_LineCNT)("UPDYMD")                     '更新年月日
        work.WF_SEL_UPDTIMSTP.Text = LNM0013tbl.Rows(WW_LineCNT)("UPDTIMSTP")               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                         '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0013row As DataRow In LNM0013tbl.Rows
            Select Case LNM0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0013tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0013tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0013tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0013tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0013tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0013tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0013tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        If Not work.WF_SEL_BIGCTNCD2.Text = "" Then
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_BIGCTNCD2.Text, work.WF_SEL_MIDDLECTNCD2.Text,
                                work.WF_SEL_PRIORITYNO.Text, work.WF_SEL_DEPSTATION.Text,
                                work.WF_SEL_JRDEPBRANCHCD.Text, work.WF_SEL_ARRSTATION.Text,
                                work.WF_SEL_JRARRBRANCHCD.Text, work.WF_SEL_UPDTIMSTP.Text)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0013WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "回送運賃適用率マスタ一覧"
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
            Case LNM0013WRKINC.FILETYPE.EXCEL
                FileName = "回送運賃適用率マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0013WRKINC.FILETYPE.PDF
                FileName = "回送運賃適用率マスタ.pdf"
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
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '大分類コード
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '中分類コード
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.PRIORITYNO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '優先順位
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発駅コード
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'ＪＲ発支社支店コード
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '着駅コード
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'ＪＲ着支社支店コード

        '入力不要列網掛け
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.BIGCTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '大分類名称
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '中分類名称
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発駅名称
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'ＪＲ発支社支店名称
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '着駅名称
        sheet.Columns(LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'ＪＲ着支社支店名称

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
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = "（必須）大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = "大分類名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = "（必須）中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = "中分類名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.PRIORITYNO).Value = "（必須）優先順位"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION).Value = "（必須）発駅コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = "発駅名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD).Value = "（必須）ＪＲ発支社支店コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHNM).Value = "ＪＲ発支社支店名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION).Value = "（必須）着駅コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Value = "着駅名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD).Value = "（必須）ＪＲ着支社支店コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHNM).Value = "ＪＲ着支社支店名称"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.PURPOSE).Value = "使用目的"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = "発受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = "発受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.CTNTYPE).Value = "コンテナ記号"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.CTNSTNO).Value = "コンテナ番号（開始）"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.CTNENDNO).Value = "コンテナ番号（終了）"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURSTYMD).Value = "特例置換項目-現行開始適用日"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURENDYMD).Value = "特例置換項目-現行終了摘要日"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURAPPLYRATE).Value = "特例置換項目-現行摘要率"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURROUNDKBN).Value = "特例置換項目-現行端数処理区分"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTSTYMD).Value = "特例置換項目-次期開始適用日"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTENDYMD).Value = "特例置換項目-次期終了摘要日"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTAPPLYRATE).Value = "特例置換項目-次期摘要率"
        sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTROUNDKBN).Value = "特例置換項目-次期端数処理区分"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '大分類コード
            COMMENT_get(SQLcon, "BIGCTNCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '中分類コード
            COMMENTCHILD_get(SQLcon, "MIDDLECTNCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            COMMENT_get(SQLcon, "STATION", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                '発駅コード
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                '着駅コード
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            COMMENT_get(SQLcon, "JRBRANCHCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                'ＪＲ発支社支店コード
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                'ＪＲ着支社支店コード
                sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '○ コメントに表示が難しいデータは別シートに作成
            WW_TEXT = "シート:受託人一覧参照"
            '発受託人コード、選択比較項目-着受託人コード
            SETSUBSHEET(wb, "REKEJM")
            '発受託人コード
            sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Comment.Shape
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

        Dim WW_BIGCTNCD As String
        Dim WW_MIDDLECTNCD As String
        Dim WW_DEPSTATION As String
        Dim WW_JRDEPBRANCHCD As String
        Dim WW_ARRSTATION As String
        Dim WW_JRARRBRANCHCD As String

        Dim WW_BIGCTNNM As String
        Dim WW_MIDDLECTNNM As String
        Dim WW_DEPSTATIONNM As String
        Dim WW_JRDEPBRANCHNM As String
        Dim WW_ARRSTATIONNM As String
        Dim WW_JRARRBRANCHNM As String

        For Each Row As DataRow In LNM0013tbl.Rows

            WW_BIGCTNCD = Row("BIGCTNCD") '大分類コード
            WW_MIDDLECTNCD = Row("MIDDLECTNCD") '中分類コード
            WW_DEPSTATION = Row("DEPSTATION") '発駅コード
            WW_JRDEPBRANCHCD = Row("JRDEPBRANCHCD") 'ＪＲ発支社支店コード
            WW_ARRSTATION = Row("ARRSTATION") '着駅コード
            WW_JRARRBRANCHCD = Row("JRARRBRANCHCD") 'ＪＲ着支社支店コード

            '名称取得
            WW_BIGCTNNM = ""
            WW_MIDDLECTNNM = ""
            WW_DEPSTATIONNM = ""
            WW_JRDEPBRANCHNM = ""
            WW_ARRSTATIONNM = ""
            WW_JRARRBRANCHNM = ""

            CODENAME_get("BIGCTNCD", WW_BIGCTNCD, WW_Dummy, WW_Dummy, WW_BIGCTNNM, WW_RtnSW) '大分類名称
            CODENAME_get("MIDDLECTNCD", WW_MIDDLECTNCD, WW_BIGCTNCD, WW_Dummy, WW_MIDDLECTNNM, WW_RtnSW) '中分類名称
            CODENAME_get("DEPSTATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称
            CODENAME_get("JRBRANCHCD", WW_JRDEPBRANCHCD, WW_Dummy, WW_Dummy, WW_JRDEPBRANCHNM, WW_RtnSW) 'ＪＲ発支社名称
            CODENAME_get("ARRSTATION", WW_ARRSTATION, WW_Dummy, WW_Dummy, WW_ARRSTATIONNM, WW_RtnSW) '着駅名称
            CODENAME_get("JRBRANCHCD", WW_JRARRBRANCHCD, WW_Dummy, WW_Dummy, WW_JRARRBRANCHNM, WW_RtnSW) 'ＪＲ着支社名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = WW_BIGCTNCD '大分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = WW_BIGCTNNM '大分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = WW_MIDDLECTNCD '中分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = WW_MIDDLECTNNM '中分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.PRIORITYNO).Value = Row("PRIORITYNO") '優先順位
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION).Value = WW_DEPSTATION '発駅コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = WW_DEPSTATIONNM '発駅名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD).Value = WW_JRDEPBRANCHCD 'ＪＲ発支社支店コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHNM).Value = WW_JRDEPBRANCHNM 'ＪＲ発支社支店名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION).Value = WW_ARRSTATION '着駅コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Value = WW_ARRSTATIONNM '着駅名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD).Value = WW_JRARRBRANCHCD 'ＪＲ着支社支店コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHNM).Value = WW_JRARRBRANCHNM 'ＪＲ着支社支店名称
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.PURPOSE).Value = Row("PURPOSE") '使用目的
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = Row("DEPTRUSTEECD") '発受託人コード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = Row("DEPTRUSTEESUBCD") '発受託人サブコード
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.CTNTYPE).Value = Row("CTNTYPE") 'コンテナ記号
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.CTNSTNO).Value = Row("CTNSTNO") 'コンテナ番号（開始）
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.CTNENDNO).Value = Row("CTNENDNO") 'コンテナ番号（終了）
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURSTYMD).Value = Row("SPRCURSTYMD") '特例置換項目-現行開始適用日
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURENDYMD).Value = Row("SPRCURENDYMD") '特例置換項目-現行終了摘要日
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURAPPLYRATE).Value = Row("SPRCURAPPLYRATE") '特例置換項目-現行摘要率
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURROUNDKBN).Value = Row("SPRCURROUNDKBN") '特例置換項目-現行端数処理区分
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTSTYMD).Value = Row("SPRNEXTSTYMD") '特例置換項目-次期開始適用日
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTENDYMD).Value = Row("SPRNEXTENDYMD") '特例置換項目-次期終了摘要日
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTAPPLYRATE).Value = Row("SPRNEXTAPPLYRATE") '特例置換項目-次期摘要率
            sheet.Cells(WW_ACTIVEROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTROUNDKBN).Value = Row("SPRNEXTROUNDKBN") '特例置換項目-次期端数処理区分

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
                Case "DELFLG"           '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                Case "BIGCTNCD"                       '大分類コード
                    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_CLASS
                Case "STATION"                       '発駅コード、着駅コード
                    WW_PrmData = work.CreateStationParam(Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STATION
                Case "JRBRANCHCD"                'ＪＲ発支社支店コード、ＪＲ着支社支店コード
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "JRBRANCHCD")
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE

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
    ''' セル表示用のコメント取得(子分類)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_CNT"></param>
    ''' <remarks></remarks>
    Protected Sub COMMENTCHILD_get(ByVal SQLcon As MySqlConnection,
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

            '親分類取得
            Select Case I_FIELD
                Case "MIDDLECTNCD"                    '中分類コード(親:大分類コード)
                    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_CLASS

            End Select
            .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

            '取得した親分類一覧を退避
            Dim WW_ListBox As New ListBox
            For Each list In .WF_LeftListBox.Items
                WW_ListBox.Items.Add(list)
            Next

            WW_PrmData.Clear()
            WW_VALUE = ""

            '子分類取得
            For i As Integer = 0 To WW_ListBox.Items.Count - 1
                If Not Trim(WW_ListBox.Items(i).Text) = "" Then
                    WW_PrmDataList.AppendLine("【" + WW_ListBox.Items(i).Value + "(" + WW_ListBox.Items(i).Text + ")】")

                    Select Case I_FIELD
                        Case "MIDDLECTNCD"　'中分類コード
                            WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, WW_ListBox.Items(i).Value)
                            WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_CLASS

                    End Select
                    .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

                    For j As Integer = 0 To .WF_LeftListBox.Items.Count - 1
                        If Not Trim(.WF_LeftListBox.Items(j).Text) = "" Then
                            WW_PrmDataList.AppendLine(.WF_LeftListBox.Items(j).Value + "：" + .WF_LeftListBox.Items(j).Text)
                        End If
                    Next

                    O_CNT += .WF_LeftListBox.Items.Count + 1 '(+1は親行分)

                End If
            Next
            O_TEXT = WW_PrmDataList.ToString

        End With

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
            Select Case I_FIELD
                Case "REKEJM"
                    subsheet.Name = "受託人一覧"

            End Select

            '○入力リスト取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Select Case I_FIELD
                    Case "REKEJM"
                        SETREKEJMLIST(SQLcon, subsheet)
                End Select

            End Using
        End With

        'サブシートの列幅自動調整
        subsheet.Cells.EntireColumn.AutoFit()

        'メインシートをアクティブにする
        mainsheet.Activate()

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
#End Region

#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\回送運賃適用率マスタ一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNM0013Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0013WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0013WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REKTRMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "回送運賃適用率マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0013Exceltbl) Then
            LNM0013Exceltbl = New DataTable
        End If
        If LNM0013Exceltbl.Columns.Count <> 0 Then
            LNM0013Exceltbl.Columns.Clear()
        End If
        LNM0013Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\REKTRMEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "REKTRMEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0013Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0013WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0013WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REKTRMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0013_REKTRM ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0013Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_REKTRM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013_REKTRM Select"
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

        Dim LNM0013Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0013Exceltblrow = LNM0013Exceltbl.NewRow

            'LINECNT
            LNM0013Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '大分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.BIGCTNCD))
            WW_DATATYPE = DataTypeHT("BIGCTNCD")
            LNM0013Exceltblrow("BIGCTNCD") = LNM0013WRKINC.DataConvert("大分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '中分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.MIDDLECTNCD))
            WW_DATATYPE = DataTypeHT("MIDDLECTNCD")
            LNM0013Exceltblrow("MIDDLECTNCD") = LNM0013WRKINC.DataConvert("中分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '優先順位
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.PRIORITYNO))
            If WW_TEXT = "" Then
                WW_CheckMES1 = "・[優先順位]を取得できませんでした。"
                WW_CheckMES2 = "入力必須項目です。"
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            Else
                WW_DATATYPE = DataTypeHT("PRIORITYNO")
                LNM0013Exceltblrow("PRIORITYNO") = LNM0013WRKINC.DataConvert("優先順位", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
            End If
            '発駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.DEPSTATION))
            WW_DATATYPE = DataTypeHT("DEPSTATION")
            LNM0013Exceltblrow("DEPSTATION") = LNM0013WRKINC.DataConvert("発駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'ＪＲ発支社支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.JRDEPBRANCHCD))
            WW_DATATYPE = DataTypeHT("JRDEPBRANCHCD")
            LNM0013Exceltblrow("JRDEPBRANCHCD") = LNM0013WRKINC.DataConvert("ＪＲ発支社支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '着駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.ARRSTATION))
            WW_DATATYPE = DataTypeHT("ARRSTATION")
            LNM0013Exceltblrow("ARRSTATION") = LNM0013WRKINC.DataConvert("着駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'ＪＲ着支社支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.JRARRBRANCHCD))
            WW_DATATYPE = DataTypeHT("JRARRBRANCHCD")
            LNM0013Exceltblrow("JRARRBRANCHCD") = LNM0013WRKINC.DataConvert("ＪＲ着支社支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '使用目的
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.PURPOSE))
            WW_DATATYPE = DataTypeHT("PURPOSE")
            LNM0013Exceltblrow("PURPOSE") = LNM0013WRKINC.DataConvert("使用目的", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEECD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEECD")
            LNM0013Exceltblrow("DEPTRUSTEECD") = LNM0013WRKINC.DataConvert("発受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEESUBCD")
            LNM0013Exceltblrow("DEPTRUSTEESUBCD") = LNM0013WRKINC.DataConvert("発受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'コンテナ記号
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.CTNTYPE)) = "" Then
                WW_TEXT = Strings.StrConv(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.CTNTYPE)), VbStrConv.Narrow)
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("CTNTYPE")
            LNM0013Exceltblrow("CTNTYPE") = LNM0013WRKINC.DataConvert("コンテナ記号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'コンテナ番号（開始）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.CTNSTNO))
            WW_DATATYPE = DataTypeHT("CTNSTNO")
            LNM0013Exceltblrow("CTNSTNO") = LNM0013WRKINC.DataConvert("コンテナ番号（開始）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'コンテナ番号（終了）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.CTNENDNO))
            WW_DATATYPE = DataTypeHT("CTNENDNO")
            LNM0013Exceltblrow("CTNENDNO") = LNM0013WRKINC.DataConvert("コンテナ番号（終了）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-現行開始適用日
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURSTYMD)) = "" Then
                WW_TEXT = Replace(LNM0013WRKINC.DateConvert(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURSTYMD))), "/", "")
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("SPRCURSTYMD")
            LNM0013Exceltblrow("SPRCURSTYMD") = LNM0013WRKINC.DataConvert("特例置換項目-現行開始適用日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-現行終了適用日
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURENDYMD)) = "" Then
                WW_TEXT = Replace(LNM0013WRKINC.DateConvert(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURENDYMD))), "/", "")
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("SPRCURENDYMD")
            LNM0013Exceltblrow("SPRCURENDYMD") = LNM0013WRKINC.DataConvert("特例置換項目-現行終了適用日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-現行適用率
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURAPPLYRATE))
            WW_DATATYPE = DataTypeHT("SPRCURAPPLYRATE")
            LNM0013Exceltblrow("SPRCURAPPLYRATE") = LNM0013WRKINC.DataConvert("特例置換項目-現行適用率", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-現行端数処理区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRCURROUNDKBN))
            WW_DATATYPE = DataTypeHT("SPRCURROUNDKBN")
            LNM0013Exceltblrow("SPRCURROUNDKBN") = LNM0013WRKINC.DataConvert("特例置換項目-現行端数処理区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-次期開始適用日
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTSTYMD)) = "" Then
                WW_TEXT = Replace(LNM0013WRKINC.DateConvert(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTSTYMD))), "/", "")
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("SPRNEXTSTYMD")
            LNM0013Exceltblrow("SPRNEXTSTYMD") = LNM0013WRKINC.DataConvert("特例置換項目-次期開始適用日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-次期終了適用日
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTENDYMD)) = "" Then
                WW_TEXT = Replace(LNM0013WRKINC.DateConvert(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTENDYMD))), "/", "")
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("SPRNEXTENDYMD")
            LNM0013Exceltblrow("SPRNEXTENDYMD") = LNM0013WRKINC.DataConvert("特例置換項目-次期終了適用日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-次期適用率
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTAPPLYRATE))
            WW_DATATYPE = DataTypeHT("SPRNEXTAPPLYRATE")
            LNM0013Exceltblrow("SPRNEXTAPPLYRATE") = LNM0013WRKINC.DataConvert("特例置換項目-次期適用率", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-次期端数処理区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.SPRNEXTROUNDKBN))
            WW_DATATYPE = DataTypeHT("SPRNEXTROUNDKBN")
            LNM0013Exceltblrow("SPRNEXTROUNDKBN") = LNM0013WRKINC.DataConvert("特例置換項目-次期端数処理区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0013WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0013Exceltblrow("DELFLG") = LNM0013WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0013Exceltbl.Rows.Add(LNM0013Exceltblrow)

        Next
    End Sub

    ''' <summary>
    ''' 今回アップロードしたデータと完全一致するデータがあるか確認する
    ''' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        BIGCTNCD")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0013_REKTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(BIGCTNCD, '')             = @BIGCTNCD ")
        SQLStr.AppendLine("    AND  coalesce(MIDDLECTNCD, '')             = @MIDDLECTNCD ")
        SQLStr.AppendLine("    AND  coalesce(PRIORITYNO, '0')             = @PRIORITYNO ")
        SQLStr.AppendLine("    AND  coalesce(DEPSTATION, '0')             = @DEPSTATION ")
        SQLStr.AppendLine("    AND  coalesce(JRDEPBRANCHCD, '0')             = @JRDEPBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(ARRSTATION, '0')             = @ARRSTATION ")
        SQLStr.AppendLine("    AND  coalesce(JRARRBRANCHCD, '0')             = @JRARRBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(PURPOSE, '')             = @PURPOSE ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEECD, '0')             = @DEPTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEESUBCD, '0')             = @DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(CTNTYPE, '')             = @CTNTYPE ")
        SQLStr.AppendLine("    AND  coalesce(CTNSTNO, '0')             = @CTNSTNO ")
        SQLStr.AppendLine("    AND  coalesce(CTNENDNO, '0')             = @CTNENDNO ")
        SQLStr.AppendLine("    AND  coalesce(SPRCURSTYMD, '')             = @SPRCURSTYMD ")
        SQLStr.AppendLine("    AND  coalesce(SPRCURENDYMD, '')             = @SPRCURENDYMD ")
        SQLStr.AppendLine("    AND  coalesce(SPRCURAPPLYRATE, '0')             = @SPRCURAPPLYRATE ")
        SQLStr.AppendLine("    AND  coalesce(SPRCURROUNDKBN, '0')             = @SPRCURROUNDKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRNEXTSTYMD, '')             = @SPRNEXTSTYMD ")
        SQLStr.AppendLine("    AND  coalesce(SPRNEXTENDYMD, '')             = @SPRNEXTENDYMD ")
        SQLStr.AppendLine("    AND  coalesce(SPRNEXTAPPLYRATE, '0')             = @SPRNEXTAPPLYRATE ")
        SQLStr.AppendLine("    AND  coalesce(SPRNEXTROUNDKBN, '0')             = @SPRNEXTROUNDKBN ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNSTNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNSTNO", MySqlDbType.VarChar, 8)         'コンテナ番号（開始）
                Dim P_CTNENDNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNENDNO", MySqlDbType.VarChar, 8)         'コンテナ番号（終了）
                Dim P_SPRCURSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURSTYMD", MySqlDbType.VarChar, 8)         '特例置換項目-現行開始適用日
                Dim P_SPRCURENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURENDYMD", MySqlDbType.VarChar, 8)         '特例置換項目-現行終了適用日
                Dim P_SPRCURAPPLYRATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURAPPLYRATE", MySqlDbType.Decimal, 5, 4)         '特例置換項目-現行適用率
                Dim P_SPRCURROUNDKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURROUNDKBN", MySqlDbType.VarChar, 2)         '特例置換項目-現行端数処理区分
                Dim P_SPRNEXTSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTSTYMD", MySqlDbType.VarChar, 8)         '特例置換項目-次期開始適用日
                Dim P_SPRNEXTENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTENDYMD", MySqlDbType.VarChar, 8)         '特例置換項目-次期終了適用日
                Dim P_SPRNEXTAPPLYRATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTAPPLYRATE", MySqlDbType.Decimal, 5, 4)         '特例置換項目-次期適用率
                Dim P_SPRNEXTROUNDKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTROUNDKBN", MySqlDbType.VarChar, 2)         '特例置換項目-次期端数処理区分
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = WW_ROW("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = WW_ROW("JRARRBRANCHCD")               'ＪＲ着支社支店コード
                P_PURPOSE.Value = WW_ROW("PURPOSE")               '使用目的
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_CTNTYPE.Value = WW_ROW("CTNTYPE")               'コンテナ記号
                P_CTNSTNO.Value = WW_ROW("CTNSTNO")               'コンテナ番号（開始）
                P_CTNENDNO.Value = WW_ROW("CTNENDNO")               'コンテナ番号（終了）
                P_SPRCURSTYMD.Value = WW_ROW("SPRCURSTYMD")               '特例置換項目-現行開始適用日
                P_SPRCURENDYMD.Value = WW_ROW("SPRCURENDYMD")               '特例置換項目-現行終了適用日
                P_SPRCURAPPLYRATE.Value = WW_ROW("SPRCURAPPLYRATE")               '特例置換項目-現行適用率
                P_SPRCURROUNDKBN.Value = WW_ROW("SPRCURROUNDKBN")               '特例置換項目-現行端数処理区分
                P_SPRNEXTSTYMD.Value = WW_ROW("SPRNEXTSTYMD")               '特例置換項目-次期開始適用日
                P_SPRNEXTENDYMD.Value = WW_ROW("SPRNEXTENDYMD") '特例置換項目-次期終了適用日
                P_SPRNEXTAPPLYRATE.Value = WW_ROW("SPRNEXTAPPLYRATE")               '特例置換項目-次期適用率
                P_SPRNEXTROUNDKBN.Value = WW_ROW("SPRNEXTROUNDKBN")               '特例置換項目-次期端数処理区分
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_REKTRM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013_REKTRM SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0013_REKTRM LNM0013")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @BIGCTNCD AS BIGCTNCD ")
        SQLStr.AppendLine("             ,@MIDDLECTNCD AS MIDDLECTNCD ")
        SQLStr.AppendLine("             ,@PRIORITYNO AS PRIORITYNO ")
        SQLStr.AppendLine("             ,@DEPSTATION AS DEPSTATION ")
        SQLStr.AppendLine("             ,@JRDEPBRANCHCD AS JRDEPBRANCHCD ")
        SQLStr.AppendLine("             ,@ARRSTATION AS ARRSTATION ")
        SQLStr.AppendLine("             ,@JRARRBRANCHCD AS JRARRBRANCHCD ")
        SQLStr.AppendLine("             ,@PURPOSE AS PURPOSE ")
        SQLStr.AppendLine("             ,@DEPTRUSTEECD AS DEPTRUSTEECD ")
        SQLStr.AppendLine("             ,@DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@CTNTYPE AS CTNTYPE ")
        SQLStr.AppendLine("             ,@CTNSTNO AS CTNSTNO ")
        SQLStr.AppendLine("             ,@CTNENDNO AS CTNENDNO ")
        SQLStr.AppendLine("             ,@SPRCURSTYMD AS SPRCURSTYMD ")
        SQLStr.AppendLine("             ,@SPRCURENDYMD AS SPRCURENDYMD ")
        SQLStr.AppendLine("             ,@SPRCURAPPLYRATE AS SPRCURAPPLYRATE ")
        SQLStr.AppendLine("             ,@SPRCURROUNDKBN AS SPRCURROUNDKBN ")
        SQLStr.AppendLine("             ,@SPRNEXTSTYMD AS SPRNEXTSTYMD ")
        SQLStr.AppendLine("             ,@SPRNEXTENDYMD AS SPRNEXTENDYMD ")
        SQLStr.AppendLine("             ,@SPRNEXTAPPLYRATE AS SPRNEXTAPPLYRATE ")
        SQLStr.AppendLine("             ,@SPRNEXTROUNDKBN AS SPRNEXTROUNDKBN ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0013.BIGCTNCD = EXCEL.BIGCTNCD ")
        SQLStr.AppendLine("         AND LNM0013.MIDDLECTNCD = EXCEL.MIDDLECTNCD ")
        SQLStr.AppendLine("         AND LNM0013.PRIORITYNO = EXCEL.PRIORITYNO ")
        SQLStr.AppendLine("         AND LNM0013.DEPSTATION = EXCEL.DEPSTATION ")
        SQLStr.AppendLine("         AND LNM0013.JRDEPBRANCHCD = EXCEL.JRDEPBRANCHCD ")
        SQLStr.AppendLine("         AND LNM0013.ARRSTATION = EXCEL.ARRSTATION ")
        SQLStr.AppendLine("         AND LNM0013.JRARRBRANCHCD = EXCEL.JRARRBRANCHCD ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0013.PURPOSE =  EXCEL.PURPOSE")
        SQLStr.AppendLine("         ,LNM0013.DEPTRUSTEECD =  EXCEL.DEPTRUSTEECD")
        SQLStr.AppendLine("         ,LNM0013.DEPTRUSTEESUBCD =  EXCEL.DEPTRUSTEESUBCD")
        SQLStr.AppendLine("         ,LNM0013.CTNTYPE =  EXCEL.CTNTYPE")
        SQLStr.AppendLine("         ,LNM0013.CTNSTNO =  EXCEL.CTNSTNO")
        SQLStr.AppendLine("         ,LNM0013.CTNENDNO =  EXCEL.CTNENDNO")
        SQLStr.AppendLine("         ,LNM0013.SPRCURSTYMD =  EXCEL.SPRCURSTYMD")
        SQLStr.AppendLine("         ,LNM0013.SPRCURENDYMD =  EXCEL.SPRCURENDYMD")
        SQLStr.AppendLine("         ,LNM0013.SPRCURAPPLYRATE =  EXCEL.SPRCURAPPLYRATE")
        SQLStr.AppendLine("         ,LNM0013.SPRCURROUNDKBN =  EXCEL.SPRCURROUNDKBN")
        SQLStr.AppendLine("         ,LNM0013.SPRNEXTSTYMD =  EXCEL.SPRNEXTSTYMD")
        SQLStr.AppendLine("         ,LNM0013.SPRNEXTENDYMD =  EXCEL.SPRNEXTENDYMD")
        SQLStr.AppendLine("         ,LNM0013.SPRNEXTAPPLYRATE =  EXCEL.SPRNEXTAPPLYRATE")
        SQLStr.AppendLine("         ,LNM0013.SPRNEXTROUNDKBN =  EXCEL.SPRNEXTROUNDKBN")
        SQLStr.AppendLine("         ,LNM0013.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0013.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0013.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0013.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0013.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @BIGCTNCD  ")
        SQLStr.AppendLine("        ,@MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,@PRIORITYNO  ")
        SQLStr.AppendLine("        ,@DEPSTATION  ")
        SQLStr.AppendLine("        ,@JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,@ARRSTATION  ")
        SQLStr.AppendLine("        ,@JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,@PURPOSE  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@CTNTYPE  ")
        SQLStr.AppendLine("        ,@CTNSTNO  ")
        SQLStr.AppendLine("        ,@CTNENDNO  ")
        SQLStr.AppendLine("        ,@SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,@SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,@SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,@SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,@SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,@SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,@SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,@SPRNEXTROUNDKBN  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNSTNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNSTNO", MySqlDbType.VarChar, 8)         'コンテナ番号（開始）
                Dim P_CTNENDNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNENDNO", MySqlDbType.VarChar, 8)         'コンテナ番号（終了）
                Dim P_SPRCURSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURSTYMD", MySqlDbType.VarChar, 8)         '特例置換項目-現行開始適用日
                Dim P_SPRCURENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURENDYMD", MySqlDbType.VarChar, 8)         '特例置換項目-現行終了適用日
                Dim P_SPRCURAPPLYRATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURAPPLYRATE", MySqlDbType.Decimal, 5, 4)         '特例置換項目-現行適用率
                Dim P_SPRCURROUNDKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRCURROUNDKBN", MySqlDbType.VarChar, 2)         '特例置換項目-現行端数処理区分
                Dim P_SPRNEXTSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTSTYMD", MySqlDbType.VarChar, 8)         '特例置換項目-次期開始適用日
                Dim P_SPRNEXTENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTENDYMD", MySqlDbType.VarChar, 8)         '特例置換項目-次期終了適用日
                Dim P_SPRNEXTAPPLYRATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTAPPLYRATE", MySqlDbType.Decimal, 5, 4)         '特例置換項目-次期適用率
                Dim P_SPRNEXTROUNDKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRNEXTROUNDKBN", MySqlDbType.VarChar, 2)         '特例置換項目-次期端数処理区分
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
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = WW_ROW("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = WW_ROW("JRARRBRANCHCD") 'ＪＲ着支社支店コード
                P_PURPOSE.Value = WW_ROW("PURPOSE")               '使用目的
                '発受託人コード
                If Not WW_ROW("DEPTRUSTEECD") = "0" Then
                    P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")
                Else
                    P_DEPTRUSTEECD.Value = DBNull.Value
                End If
                '発受託人サブコード
                If Not WW_ROW("DEPTRUSTEESUBCD") = "0" Then
                    P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")
                Else
                    P_DEPTRUSTEESUBCD.Value = DBNull.Value
                End If
                'コンテナ記号
                If Not WW_ROW("CTNTYPE") = "" Then
                    P_CTNTYPE.Value = WW_ROW("CTNTYPE")
                Else
                    P_CTNTYPE.Value = DBNull.Value
                End If
                'コンテナ番号（開始）
                If Not WW_ROW("CTNSTNO") = "0" Then
                    P_CTNSTNO.Value = WW_ROW("CTNSTNO")
                Else
                    P_CTNSTNO.Value = DBNull.Value
                End If
                'コンテナ番号（終了）
                If Not WW_ROW("CTNENDNO") = "0" Then
                    P_CTNENDNO.Value = WW_ROW("CTNENDNO")
                Else
                    P_CTNENDNO.Value = DBNull.Value
                End If
                '特例置換項目-現行開始適用日
                If Not WW_ROW("SPRCURSTYMD") = "" Then
                    P_SPRCURSTYMD.Value = WW_ROW("SPRCURSTYMD")
                Else
                    P_SPRCURSTYMD.Value = DBNull.Value
                End If
                '特例置換項目-現行終了適用日
                If Not WW_ROW("SPRCURENDYMD") = "" Then
                    P_SPRCURENDYMD.Value = WW_ROW("SPRCURENDYMD")
                Else
                    P_SPRCURENDYMD.Value = DBNull.Value
                End If
                '特例置換項目-現行適用率
                If Not WW_ROW("SPRCURAPPLYRATE") = "0" Then
                    P_SPRCURAPPLYRATE.Value = WW_ROW("SPRCURAPPLYRATE")
                Else
                    P_SPRCURAPPLYRATE.Value = DBNull.Value
                End If
                '特例置換項目-現行端数処理区分
                If Not WW_ROW("SPRCURROUNDKBN") = "0" Then
                    P_SPRCURROUNDKBN.Value = WW_ROW("SPRCURROUNDKBN")
                Else
                    P_SPRCURROUNDKBN.Value = DBNull.Value
                End If
                '特例置換項目-次期開始適用日
                If Not WW_ROW("SPRNEXTSTYMD") = "" Then
                    P_SPRNEXTSTYMD.Value = WW_ROW("SPRNEXTSTYMD")
                Else
                    P_SPRNEXTSTYMD.Value = DBNull.Value
                End If
                '特例置換項目-次期終了適用日
                If Not WW_ROW("SPRNEXTENDYMD") = "" Then
                    P_SPRNEXTENDYMD.Value = WW_ROW("SPRNEXTENDYMD")
                Else
                    P_SPRNEXTENDYMD.Value = DBNull.Value
                End If
                '特例置換項目-次期適用率
                P_SPRNEXTAPPLYRATE.Value = WW_ROW("SPRNEXTAPPLYRATE")
                '特例置換項目-次期端数処理区分
                P_SPRNEXTROUNDKBN.Value = WW_ROW("SPRNEXTROUNDKBN")

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_REKTRM  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0013_REKTRM  INSERTUPDATE"
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
        Dim WW_DBDataCheck As String = ""
        Dim WW_ConstructionYMD As String = ""

        Dim WW_SPRCURSTYMD_KETA As Integer = "0"
        Dim WW_SPRCURENDYMD_KETA As Integer = "0"
        Dim WW_SPRNEXTSTYMD_KETA As Integer = "0"
        Dim WW_SPRNEXTENDYMD_KETA As Integer = "0"

        Dim WW_SPRCURAPPLYRATE_INT As Integer = 0
        Dim WW_SPRNEXTAPPLYRATE_INT As Integer = 0

        Dim WW_SPRCURROUNDKBN1 As String = ""
        Dim WW_SPRCURROUNDKBN2 As String = ""
        Dim WW_SPRNEXTROUNDKBN1 As String = ""
        Dim WW_SPRNEXTROUNDKBN2 As String = ""

        '特例置換項目-現行端数処理区分1、2取得
        If Not WW_ROW("SPRCURROUNDKBN") = "0" And WW_ROW("SPRCURROUNDKBN").ToString.Length = 2 Then
            WW_SPRCURROUNDKBN1 = WW_ROW("SPRCURROUNDKBN").ToString.Substring(0, 1)
            WW_SPRCURROUNDKBN2 = WW_ROW("SPRCURROUNDKBN").ToString.Substring(1, 1)
        End If
        '特例置換項目-次期端数処理区分1、2取得
        If Not WW_ROW("SPRNEXTROUNDKBN") = "0" And WW_ROW("SPRNEXTROUNDKBN").ToString.Length = 2 Then
            WW_SPRNEXTROUNDKBN1 = WW_ROW("SPRNEXTROUNDKBN").ToString.Substring(0, 1)
            WW_SPRNEXTROUNDKBN2 = WW_ROW("SPRNEXTROUNDKBN").ToString.Substring(1, 1)
        End If

        WW_LineErr = ""

        ' 削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
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

        ' 大分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIGCTNCD", WW_ROW("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 値存在チェック
            CODENAME_get("BIGCTNCD", WW_ROW("BIGCTNCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
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
            ' 値存在チェック
            CODENAME_get("MIDDLECTNCD", WW_ROW("MIDDLECTNCD"), WW_ROW("BIGCTNCD"), WW_Dummy, WW_Dummy, WW_RtnSW)
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

        ' 優先順位(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "PRIORITYNO", WW_ROW("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・優先順位エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 発駅コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPSTATION", WW_ROW("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then

            If Not String.IsNullOrEmpty(WW_ROW("DEPSTATION")) AndAlso
               Not WW_ROW("DEPSTATION") = "0" Then
                ' 値存在チェック
                'CODENAME_get("DEPSTATION", WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                'If Not isNormal(WW_RtnSW) Then
                '    WW_CheckMES1 = "・発駅コードエラーです。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            End If
        Else
            WW_CheckMES1 = "・発駅コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' ＪＲ発支社支店コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "JRDEPBRANCHCD", WW_ROW("JRDEPBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then

            If Not String.IsNullOrEmpty(WW_ROW("JRDEPBRANCHCD")) AndAlso
               Not WW_ROW("JRDEPBRANCHCD") = "0" Then
                ' 値存在チェック
                CODENAME_get("JRBRANCHCD", WW_ROW("JRDEPBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・ＪＲ発支社支店コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・ＪＲ発支社支店コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 着駅コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ARRSTATION", WW_ROW("ARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("ARRSTATION")) AndAlso
               Not WW_ROW("ARRSTATION") = "0" Then
                ' 値存在チェック
                'CODENAME_get("ARRSTATION", WW_ROW("ARRSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                'If Not isNormal(WW_RtnSW) Then
                '    WW_CheckMES1 = "・着駅コードエラーです。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            End If
        Else
            WW_CheckMES1 = "・着駅コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' ＪＲ着支社支店コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "JRARRBRANCHCD", WW_ROW("JRARRBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("JRARRBRANCHCD")) AndAlso
               Not WW_ROW("JRARRBRANCHCD") = "0" Then
                ' 値存在チェック
                CODENAME_get("JRBRANCHCD", WW_ROW("JRARRBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・ＪＲ着支社支店コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・ＪＲ着支社支店コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 使用目的
        Master.CheckField(Master.USERCAMP, "PURPOSE", WW_ROW("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・使用目的エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 発受託人コード
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", WW_ROW("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("DEPTRUSTEECD")) AndAlso
               Not WW_ROW("DEPTRUSTEECD") = "0" Then
                ' 値存在チェック
                CODENAME_get("DEPTRUSTEECD", WW_ROW("DEPTRUSTEECD"), WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・発受託人コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 発受託人サブコード
        If Not WW_ROW("DEPTRUSTEESUBCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", WW_ROW("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("DEPTRUSTEESUBCD")) Then
                    ' 値存在チェック
                    CODENAME_get("DEPTRUSTEESUBCD", WW_ROW("DEPTRUSTEESUBCD"), WW_ROW("DEPSTATION"), WW_ROW("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・発受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        ' コンテナ記号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNTYPE", WW_ROW("CTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("CTNTYPE")) Then
                ' 値存在チェック
                CODENAME_get("CTNTYPE", WW_ROW("CTNTYPE"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・コンテナ記号エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・コンテナ記号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' コンテナ番号（開始）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNSTNO", WW_ROW("CTNSTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("CTNSTNO")) AndAlso
               Not WW_ROW("CTNSTNO") = "0" Then
                ' 値存在チェック
                CODENAME_get("CTNSTNO", WW_ROW("CTNSTNO"), WW_ROW("CTNTYPE"), WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' コンテナ番号（終了）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNENDNO", WW_ROW("CTNENDNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("CTNENDNO")) AndAlso
               Not WW_ROW("CTNENDNO") = "0" Then
                ' 値存在チェック
                CODENAME_get("CTNENDNO", WW_ROW("CTNENDNO"), WW_ROW("CTNTYPE"), WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-現行開始適用日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRCURSTYMD", WW_ROW("SPRCURSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 画面表示の書式を変更
            If Not String.IsNullOrEmpty(WW_ROW("SPRCURSTYMD")) Then
                Try
                    WW_ChkDate = Integer.Parse(WW_ROW("SPRCURSTYMD").Replace("/", ""))
                    If WW_ChkDate <= 9999 Then
                        ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                        WW_ROW("SPRCURSTYMD") = WW_ChkDate
                        WW_ChkDate8str = 20000000 + WW_ChkDate
                        WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                        CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        'Else
                        '    WW_ROW("SPRCURSTYMD") = CDate(WW_ROW("SPRCURSTYMD")).ToString("yyyy/MM/dd")
                    End If
                    WW_SPRCURSTYMD_KETA = WW_ChkDate

                Catch ex As Exception
                    WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
                    WW_CheckMES2 = "日付以外が入力されています。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End Try
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-現行終了適用日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRCURENDYMD", WW_ROW("SPRCURENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 画面表示の書式を変更
            If Not String.IsNullOrEmpty(WW_ROW("SPRCURENDYMD")) Then
                Try
                    WW_ChkDate = Integer.Parse(WW_ROW("SPRCURENDYMD").Replace("/", ""))
                    If WW_ChkDate <= 9999 Then
                        ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                        WW_ROW("SPRCURENDYMD") = WW_ChkDate
                        WW_ChkDate8str = 20000000 + WW_ChkDate
                        WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                        CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        'Else
                        '    WW_ROW("SPRCURENDYMD") = CDate(WW_ROW("SPRCURENDYMD")).ToString("yyyy/MM/dd")
                    End If
                    WW_SPRCURENDYMD_KETA = WW_ChkDate

                Catch ex As Exception
                    WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
                    WW_CheckMES2 = "日付以外が入力されています。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End Try
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-現行適用率
        Master.CheckField(Master.USERCAMP, "SPRCURAPPLYRATE", WW_ROW("SPRCURAPPLYRATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            '適用率を数値に変換する
            If String.IsNullOrEmpty(WW_ROW("SPRCURAPPLYRATE")) Then
                WW_SPRCURAPPLYRATE_INT = 0
            Else
                WW_SPRCURAPPLYRATE_INT = CType(WW_ROW("SPRCURAPPLYRATE"), Integer)
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（現行）適用率エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-現行端数処理区分1
        Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN1", WW_SPRCURROUNDKBN1, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_SPRCURROUNDKBN1) AndAlso
               Not WW_SPRCURROUNDKBN1 = "0" Then
                ' 値存在チェック
                CODENAME_get("SPRCURROUNDKBN1", WW_SPRCURROUNDKBN1, WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-現行端数処理区分2
        Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN2", WW_SPRCURROUNDKBN2, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_SPRCURROUNDKBN2) AndAlso
               Not WW_SPRCURROUNDKBN2 = "0" Then
                ' 値存在チェック
                CODENAME_get("SPRCURROUNDKBN2", WW_SPRCURROUNDKBN2, WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分2エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（現行）端数処理区分2エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-次期開始適用日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRNEXTSTYMD", WW_ROW("SPRNEXTSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 画面表示の書式を変更
            If Not String.IsNullOrEmpty(WW_ROW("SPRNEXTSTYMD")) Then
                Try
                    WW_ChkDate = Integer.Parse(WW_ROW("SPRNEXTSTYMD").Replace("/", ""))
                    If WW_ChkDate <= 9999 Then
                        ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                        WW_ROW("SPRNEXTSTYMD") = WW_ChkDate
                        WW_ChkDate8str = 20000000 + WW_ChkDate
                        WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                        CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        'Else
                        '    WW_ROW("SPRNEXTSTYMD") = CDate(WW_ROW("SPRNEXTSTYMD")).ToString("yyyy/MM/dd")
                    End If
                    WW_SPRNEXTSTYMD_KETA = WW_ChkDate

                Catch ex As Exception
                    WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                    WW_CheckMES2 = "日付以外が入力されています。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End Try
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-次期終了適用日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRNEXTENDYMD", WW_ROW("SPRNEXTENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 画面表示の書式を変更
            If Not String.IsNullOrEmpty(WW_ROW("SPRNEXTENDYMD")) Then
                Try
                    WW_ChkDate = Integer.Parse(WW_ROW("SPRNEXTENDYMD").Replace("/", ""))
                    If WW_ChkDate <= 9999 Then
                        ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                        WW_ROW("SPRNEXTENDYMD") = WW_ChkDate
                        WW_ChkDate8str = 20000000 + WW_ChkDate
                        WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                        CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        'Else
                        '    WW_ROW("SPRNEXTENDYMD") = CDate(WW_ROW("SPRNEXTENDYMD")).ToString("yyyy/MM/dd")
                    End If
                    WW_SPRNEXTENDYMD_KETA = WW_ChkDate

                Catch ex As Exception
                    WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                    WW_CheckMES2 = "日付以外が入力されています。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End Try
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-次期適用率
        Master.CheckField(Master.USERCAMP, "SPRNEXTAPPLYRATE", WW_ROW("SPRNEXTAPPLYRATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            '適用率を数値に変換する
            If String.IsNullOrEmpty(WW_ROW("SPRNEXTAPPLYRATE")) Then
                WW_SPRNEXTAPPLYRATE_INT = 0
            Else
                WW_SPRNEXTAPPLYRATE_INT = CType(WW_ROW("SPRNEXTAPPLYRATE"), Integer)
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（次期）適用率エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-次期端数処理区分1
        Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN1", WW_SPRNEXTROUNDKBN1, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_SPRNEXTROUNDKBN1) AndAlso
               Not WW_SPRNEXTROUNDKBN1 = "0" Then
                ' 値存在チェック
                CODENAME_get("SPRNEXTROUNDKBN1", WW_SPRNEXTROUNDKBN1, WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-次期端数処理区分2
        Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN2", WW_SPRNEXTROUNDKBN2, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_SPRNEXTROUNDKBN2) AndAlso
               Not WW_SPRNEXTROUNDKBN2 = "0" Then
                ' 値存在チェック
                CODENAME_get("SPRNEXTROUNDKBN2", WW_SPRNEXTROUNDKBN2, WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分2エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・特例置換項目（次期）端数処理区分2エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '○ 項目間の整合チェック

        '発駅コード、ＪＲ発支社支店コードの同時入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("DEPSTATION")) AndAlso
           Not WW_ROW("DEPSTATION") = "0" Then
            If Not String.IsNullOrEmpty(WW_ROW("JRDEPBRANCHCD")) AndAlso
                Not WW_ROW("JRDEPBRANCHCD") = "0" Then
                WW_CheckMES1 = "・発駅コード＆ＪＲ発支社支店コードエラーです。"
                WW_CheckMES2 = "同時入力は行えません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '発駅コード、ＪＲ発支社支店コードどちらも未入力はエラー
        If String.IsNullOrEmpty(WW_ROW("DEPSTATION")) OrElse
            WW_ROW("DEPSTATION") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("JRDEPBRANCHCD")) OrElse
            WW_ROW("JRDEPBRANCHCD") = "0" Then
                WW_CheckMES1 = "・発駅コード＆ＪＲ発支社支店コードエラーです。"
                WW_CheckMES2 = "何れかを入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '着駅コード、ＪＲ着支社支店コードの同時入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("ARRSTATION")) AndAlso
           Not WW_ROW("ARRSTATION") = "0" Then
            If Not String.IsNullOrEmpty(WW_ROW("JRARRBRANCHCD")) AndAlso
             Not WW_ROW("JRARRBRANCHCD") = "0" Then
                WW_CheckMES1 = "・着駅コード＆ＪＲ着支社支店コードエラーです。"
                WW_CheckMES2 = "同時入力は行えません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '着駅コード、ＪＲ着支社支店コードどちらも未入力はエラー
        If String.IsNullOrEmpty(WW_ROW("ARRSTATION")) OrElse
            WW_ROW("ARRSTATION") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("JRARRBRANCHCD")) OrElse
            WW_ROW("JRARRBRANCHCD") = "0" Then
                WW_CheckMES1 = "・着駅コード＆ＪＲ着支社支店コードエラーです。"
                WW_CheckMES2 = "何れかを入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '発受託人コードコード入力時、発駅コードの入力が必要
        If Not WW_ROW("DEPTRUSTEECD") = "0" OrElse
           Not WW_ROW("DEPTRUSTEESUBCD") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("DEPSTATION")) OrElse
            WW_ROW("DEPSTATION") = "0" Then
                WW_CheckMES1 = "・発受託人コード＆発受託人サブコードエラーです。"
                WW_CheckMES2 = "発受託人コードを入力する場合、発駅コードも入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        'コンテナ番号（開始）入力時、コンテナ番号（終了）未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("CTNSTNO")) AndAlso
           Not WW_ROW("CTNSTNO") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("CTNENDNO")) OrElse
             WW_ROW("CTNENDNO") = "0" Then
                WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = "コンテナ番号（開始）を入力する場合、コンテナ番号（終了）も入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        'コンテナ番号（終了）入力時、コンテナ番号（開始）未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("CTNENDNO")) AndAlso
           Not WW_ROW("CTNENDNO") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("CTNSTNO")) OrElse
            WW_ROW("CTNSTNO") = "0" Then
                WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
                WW_CheckMES2 = "コンテナ番号（終了）を入力する場合、コンテナ番号（開始）も入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        'コンテナ番号（開始）、コンテナ番号（終了）の大小関係チェック
        If Not String.IsNullOrEmpty(WW_ROW("CTNSTNO")) AndAlso
           Not WW_ROW("CTNSTNO") = "0" AndAlso
           Not String.IsNullOrEmpty(WW_ROW("CTNENDNO")) AndAlso
           Not WW_ROW("CTNENDNO") = "0" Then
            If WW_ROW("CTNSTNO") > WW_ROW("CTNENDNO") Then
                WW_CheckMES1 = "・コンテナ番号（開始）＆コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = "コンテナ番号大小入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '現行開始適用日入力時、現行終了適用日未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("SPRCURSTYMD")) AndAlso
             String.IsNullOrEmpty(WW_ROW("SPRCURENDYMD")) Then
            WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
            WW_CheckMES2 = "開始適用日を入力する場合、終了適用日も入力して下さい。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '現行終了適用日入力時、現行開始適用日未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("SPRCURENDYMD")) AndAlso
             String.IsNullOrEmpty(WW_ROW("SPRCURSTYMD")) Then
            WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
            WW_CheckMES2 = "終了適用日を入力する場合、開始適用日も入力して下さい。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '現行開始適用日、現行終了適用日の入力が年月日または年月で揃っているかチェック
        If Not WW_SPRCURSTYMD_KETA = 0 AndAlso
           Not WW_SPRCURENDYMD_KETA = 0 Then
            If WW_SPRCURSTYMD_KETA > 9999 AndAlso
               WW_SPRCURENDYMD_KETA < 9999 OrElse
               WW_SPRCURSTYMD_KETA < 9999 AndAlso
               WW_SPRCURENDYMD_KETA > 9999 Then
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日＆特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = "入力内容を年月日または月日で揃えて下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '現行開始適用日、現行終了適用日の大小関係チェック
        If WW_SPRCURSTYMD_KETA > 9999 AndAlso
           WW_SPRCURENDYMD_KETA > 9999 Then
            If WW_SPRCURSTYMD_KETA > WW_SPRCURENDYMD_KETA Then
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日＆特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = "大小入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '次期開始適用日入力時、次期終了適用日未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("SPRNEXTSTYMD")) AndAlso
             String.IsNullOrEmpty(WW_ROW("SPRNEXTENDYMD")) Then
            WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
            WW_CheckMES2 = "開始適用日を入力する場合、終了適用日も入力して下さい。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '次期終了適用日入力時、次期開始適用日未入力はエラー
        If Not String.IsNullOrEmpty(WW_ROW("SPRNEXTENDYMD")) AndAlso
             String.IsNullOrEmpty(WW_ROW("SPRNEXTSTYMD")) Then
            WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
            WW_CheckMES2 = "終了適用日を入力する場合、開始適用日も入力して下さい。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '次期開始適用日、次期終了適用日の入力が年月日または年月で揃っているかチェック
        If Not WW_SPRNEXTSTYMD_KETA = 0 AndAlso
           Not WW_SPRNEXTENDYMD_KETA = 0 Then
            If WW_SPRNEXTSTYMD_KETA > 9999 AndAlso
               WW_SPRNEXTENDYMD_KETA < 9999 OrElse
               WW_SPRNEXTSTYMD_KETA < 9999 AndAlso
               WW_SPRNEXTENDYMD_KETA > 9999 Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "入力内容を年月日または月日で揃えて下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '次期開始適用日、次期終了適用日の大小関係チェック
        If WW_SPRNEXTSTYMD_KETA > 9999 AndAlso
           WW_SPRNEXTENDYMD_KETA > 9999 Then
            If WW_SPRNEXTSTYMD_KETA > WW_SPRNEXTENDYMD_KETA Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "大小入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '現行終了適用日、次期開始適用日の大小関係チェック
        If WW_SPRCURENDYMD_KETA > 9999 AndAlso
           WW_SPRNEXTSTYMD_KETA > 9999 Then
            If WW_SPRCURENDYMD_KETA > WW_SPRNEXTSTYMD_KETA Then
                WW_CheckMES1 = "・特例置換項目（現行）終了適用日＆特例置換項目（次期）開始適用日エラーです。"
                WW_CheckMES2 = "大小入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '次期適用日を入力する場合、次期適用率も入力が必要
        If Not WW_SPRNEXTSTYMD_KETA = 0 OrElse
           Not WW_SPRNEXTENDYMD_KETA = 0 Then
            If WW_SPRNEXTAPPLYRATE_INT = 0 Then
                WW_CheckMES1 = "・特例置換項目（次期）適用率エラーです。"
                WW_CheckMES2 = "次期開始適用日または次期終了適用日を入力する場合、次期適用率も入力して下さい。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '次期適用率を入力する場合、次期適用日も入力が必要
        If Not WW_SPRNEXTAPPLYRATE_INT = 0 Then
            If WW_SPRNEXTSTYMD_KETA = 0 OrElse
               WW_SPRNEXTENDYMD_KETA = 0 Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "次期適用率を入力する場合、次期開始適用日及び次期終了適用日も入力して下さい。"
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
    Protected Sub REKTRMEXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '回送運賃適用率マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        BIGCTNCD")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0013_REKTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND JRDEPBRANCHCD  = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND ARRSTATION     = @ARRSTATION")
        SQLStr.AppendLine("    AND JRARRBRANCHCD  = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード

                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = WW_ROW("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = WW_ROW("JRARRBRANCHCD")               'ＪＲ着支社支店コード

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
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_REKTRM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013_REKTRM SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0116_REKTRHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
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
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
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
        SQLStr.AppendLine("        LNG.LNM0013_REKTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND JRDEPBRANCHCD  = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND ARRSTATION     = @ARRSTATION")
        SQLStr.AppendLine("    AND JRARRBRANCHCD  = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = WW_ROW("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = WW_ROW("JRARRBRANCHCD")               'ＪＲ着支社支店コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0116_REKTRHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0116_REKTRHIST  INSERT"
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
    ''' <param name="I_VALUE3"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByVal I_VALUE1 As String,
                               ByVal I_VALUE2 As String,
                               ByVal I_VALUE3 As String,
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
                Case "BIGCTNCD"                   '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"                '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, I_VALUE2))
                Case "DEPSTATION"                 '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE1, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "JRBRANCHCD"              'ＪＲ発支社支店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "JRBRANCHCD"))
                Case "ARRSTATION"                 '着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE1, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"               '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, I_VALUE2))
                Case "DEPTRUSTEESUBCD"            '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, I_VALUE2, I_VALUE3))
                Case "CTNTYPE"                    'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE1, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNSTNO"                    'コンテナ番号（開始）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE1, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, I_VALUE2))
                Case "CTNENDNO"                   'コンテナ番号（終了）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE1, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, I_VALUE2))
                Case "SPRCURROUNDKBN1"             '特例置換項目-現行端数処理区分1
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU1"))
                Case "SPRCURROUNDKBN2"             '特例置換項目-現行端数処理区分2
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU2"))
                Case "SPRNEXTROUNDKBN1"            '特例置換項目-次期端数処理区分1
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU1"))
                Case "SPRNEXTROUNDKBN2"            '特例置換項目-次期端数処理区分2
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU2"))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class

