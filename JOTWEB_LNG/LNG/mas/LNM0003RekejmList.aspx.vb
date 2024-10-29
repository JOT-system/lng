''************************************************************
' コンテナ決済マスタメンテ一覧画面
' 作成日 2022/01/26
' 更新日 2023/12/28
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/01/26 新規作成
'          : 2023/12/28 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コンテナ決済マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0003RekejmList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0003tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                           '添付ファイルテーブル
    Private LNM0003Exceltbl As New DataTable                         'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0003tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0003WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0003WRKINC.FILETYPE.PDF)
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
            If Not IsNothing(LNM0003tbl) Then
                LNM0003tbl.Clear()
                LNM0003tbl.Dispose()
                LNM0003tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0003WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0003S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0003D Then
            Master.RecoverTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNM0003tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0003tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0003tbl)

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

        If IsNothing(LNM0003tbl) Then
            LNM0003tbl = New DataTable
        End If

        If LNM0003tbl.Columns.Count <> 0 Then
            LNM0003tbl.Columns.Clear()
        End If

        LNM0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをコンテナ決済マスタから取得する
        Dim SQLStr As String =
              " Select                                                                                  " _
            & "     1                                                            AS 'SELECT'             " _
            & "   , 0                                                            AS HIDDEN               " _
            & "   , 0                                                            AS LINECNT              " _
            & "   , ''                                                           AS OPERATION            " _
            & "   , LNM0003.UPDTIMSTP                                            AS UPDTIMSTP            " _
            & "   , coalesce(RTRIM(LNM0003.DELFLG), '')                            AS DELFLG               " _
            & "   , coalesce(RTRIM(LNM0003.DEPSTATION), '')                        AS DEPSTATION           " _
            & "   , coalesce(RTRIM(LNM0003.DEPTRUSTEECD), '')                      AS DEPTRUSTEECD         " _
            & "   , coalesce(RTRIM(LNM0003.DEPTRUSTEESUBCD), '')                   AS DEPTRUSTEESUBCD      " _
            & "   , coalesce(RTRIM(LNM0003.DEPTRUSTEENM), '')                      AS DEPTRUSTEENM         " _
            & "   , coalesce(RTRIM(LNM0003.DEPTRUSTEESUBNM), '')                   AS DEPTRUSTEESUBNM      " _
            & "   , coalesce(RTRIM(LNM0003.DEPTRUSTEESUBKANA), '')                 AS DEPTRUSTEESUBKANA    " _
            & "   , coalesce(RTRIM(LNM0003.TORICODE), '')                          AS TORICODE             " _
            & "   , coalesce(RTRIM(LNM0003.ELIGIBLEINVOICENUMBER), '')             AS ELIGIBLEINVOICENUMBER " _
            & "   , coalesce(RTRIM(LNM0003.INVKEIJYOBRANCHCD), '')                 AS INVKEIJYOBRANCHCD    " _
            & "   , coalesce(RTRIM(LNM0003.INVCYCL), '')                           AS INVCYCL              " _
            & "   , coalesce(RTRIM(LNM0003.INVFILINGDEPT), '')                     AS INVFILINGDEPT        " _
            & "   , coalesce(RTRIM(LNM0003.INVKESAIKBN), '')                       AS INVKESAIKBN          " _
            & "   , coalesce(RTRIM(LNM0003.INVSUBCD), '')                          AS INVSUBCD             " _
            & "   , coalesce(RTRIM(LNM0003.PAYKEIJYOBRANCHCD), '')                 AS PAYKEIJYOBRANCHCD    " _
            & "   , coalesce(RTRIM(LNM0003.PAYFILINGBRANCH), '')                   AS PAYFILINGBRANCH      " _
            & "   , coalesce(RTRIM(LNM0003.TAXCALCUNIT), '')                       AS TAXCALCUNIT          " _
            & "   , coalesce(RTRIM(LNM0003.PAYKESAIKBN), '')                       AS PAYKESAIKBN          " _
            & "   , coalesce(RTRIM(LNM0003.PAYBANKCD), '')                         AS PAYBANKCD            " _
            & "   , coalesce(RTRIM(LNM0003.PAYBANKBRANCHCD), '')                   AS PAYBANKBRANCHCD      " _
            & "   , coalesce(RTRIM(LNM0003.PAYACCOUNTTYPE), '')                    AS PAYACCOUNTTYPE       " _
            & "   , coalesce(RTRIM(LNM0003.PAYACCOUNTNO), '')                      AS PAYACCOUNTNO         " _
            & "   , coalesce(RTRIM(LNM0003.PAYACCOUNTNM), '')                      AS PAYACCOUNTNM         " _
            & "   , coalesce(RTRIM(LNM0003.PAYTEKIYO), '')                         AS PAYTEKIYO            " _
            & "   , coalesce(RTRIM(LNM0003.BEFOREINVKEIJYOBRANCHCD), '')           AS BEFOREINVKEIJYOBRANCHCD  " _
            & "   , coalesce(RTRIM(LNM0003.BEFOREINVFILINGDEPT), '')               AS BEFOREINVFILINGDEPT  " _
            & "   , coalesce(RTRIM(LNM0003.BEFOREPAYKEIJYOBRANCHCD), '')           AS BEFOREPAYKEIJYOBRANCHCD  " _
            & "   , coalesce(RTRIM(LNM0003.BEFOREPAYFILINGBRANCH), '')             AS BEFOREPAYFILINGBRANCH  " _
            & " FROM                                                                                     " _
            & "     LNG.LNM0003_REKEJM LNM0003                                                           "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then
            SQLWhereStr = " WHERE                        " _
                        & "     LNM0003.DEPSTATION = @P1 "
        End If
        ' 発受託人コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEECD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                              " _
                            & "     LNM0003.DEPTRUSTEECD = @P2     "
            Else
                SQLWhereStr &= "    AND LNM0003.DEPTRUSTEECD = @P2 "
            End If
        End If
        ' 発受託人サブコード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEESUBCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                                 " _
                            & "     LNM0003.DEPTRUSTEESUBCD = @P3     "
            Else
                SQLWhereStr &= "    AND LNM0003.DEPTRUSTEESUBCD = @P3 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNM0003.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNM0003.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY                    " _
            & "     LNM0003.DEPSTATION       " _
            & "   , LNM0003.DEPTRUSTEECD     " _
            & "   , LNM0003.DEPTRUSTEESUBCD  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6)  '発駅コード
                    PARA1.Value = work.WF_SEL_DEPSTATION.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEECD.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 5)  '発受託人コード
                    PARA2.Value = work.WF_SEL_DEPTRUSTEECD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEESUBCD.Text) Then
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 3)  '発受託人サブコード
                    PARA3.Value = work.WF_SEL_DEPTRUSTEESUBCD.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0003row As DataRow In LNM0003tbl.Rows
                    i += 1
                    LNM0003row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003L Select"
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
        For Each LNM0003row As DataRow In LNM0003tbl.Rows
            If LNM0003row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0003row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0003tbl)

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

        work.WF_SEL_LINECNT.Text = ""                                           '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)  '削除
        work.WF_SEL_DEPSTATION2.Text = ""                                       '発駅コード
        work.WF_SEL_DEPTRUSTEECD2.Text = ""                                     '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD2.Text = ""                                  '発受託人サブコード
        work.WF_SEL_DEPTRUSTEENM.Text = ""                                      '発受託人名称
        work.WF_SEL_DEPTRUSTEESUBNM.Text = ""                                   '発受託人サブ名称
        work.WF_SEL_DEPTRUSTEESUBKANA.Text = ""                                 '発受託人名称（カナ）
        work.WF_SEL_TORICODE.Text = ""                                          '取引先コード
        work.WF_SEL_ELIGIBLEINVOICENUMBER.Text = ""                             '適格請求書登録番号
        work.WF_SEL_INVKEIJYOBRANCHCD.Text = ""                                 '請求項目 計上店コード
        work.WF_SEL_INVCYCL.Text = ""                                           '請求項目 請求サイクル
        work.WF_SEL_INVFILINGDEPT.Text = ""                                     '請求項目 請求書提出部店
        work.WF_SEL_INVKESAIKBN.Text = ""                                       '請求項目 請求書決済区分
        work.WF_SEL_INVSUBCD.Text = ""                                          '請求項目 請求書細分コード
        work.WF_SEL_PAYKEIJYOBRANCHCD.Text = ""                                 '支払項目 費用計上店コード
        work.WF_SEL_PAYFILINGBRANCH.Text = ""                                   '支払項目 支払書提出支店
        work.WF_SEL_TAXCALCUNIT.Text = ""                                       '支払項目 消費税計算単位
        work.WF_SEL_PAYKESAIKBN.Text = ""                                       '支払項目 決済区分
        work.WF_SEL_PAYBANKCD.Text = ""                                         '支払項目 銀行コード
        work.WF_SEL_PAYBANKBRANCHCD.Text = ""                                   '支払項目 銀行支店コード
        work.WF_SEL_PAYACCOUNTTYPE.Text = ""                                    '支払項目 口座種別
        work.WF_SEL_PAYACCOUNTNO.Text = ""                                      '支払項目 口座番号
        work.WF_SEL_PAYACCOUNTNM.Text = ""                                      '支払項目 口座名義人
        work.WF_SEL_PAYTEKIYO.Text = ""                                         '支払項目 支払摘要
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                             '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0003RekejmHistory.aspx")
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
        Dim TBLview As New DataView(LNM0003tbl)
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

        work.WF_SEL_LINECNT.Text = LNM0003tbl.Rows(WW_LineCNT)("LINECNT")                             '選択行
        work.WF_SEL_DELFLG.Text = LNM0003tbl.Rows(WW_LineCNT)("DELFLG")                               '削除フラグ
        work.WF_SEL_DEPSTATION2.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPSTATION")                      '発駅コード
        work.WF_SEL_DEPTRUSTEECD2.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPTRUSTEECD")                  '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD2.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPTRUSTEESUBCD")            '発受託人サブコード
        work.WF_SEL_DEPTRUSTEENM.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPTRUSTEENM")                   '発受託人名称
        work.WF_SEL_DEPTRUSTEESUBNM.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPTRUSTEESUBNM")             '発受託人サブ名称
        work.WF_SEL_DEPTRUSTEESUBKANA.Text = LNM0003tbl.Rows(WW_LineCNT)("DEPTRUSTEESUBKANA")         '発受託人名称（カナ）
        work.WF_SEL_TORICODE.Text = LNM0003tbl.Rows(WW_LineCNT)("TORICODE")                           '取引先コード
        work.WF_SEL_ELIGIBLEINVOICENUMBER.Text = LNM0003tbl.Rows(WW_LineCNT)("ELIGIBLEINVOICENUMBER") '適格請求書登録番号
        work.WF_SEL_INVKEIJYOBRANCHCD.Text = LNM0003tbl.Rows(WW_LineCNT)("INVKEIJYOBRANCHCD")         '請求項目 計上店コード
        work.WF_SEL_INVCYCL.Text = LNM0003tbl.Rows(WW_LineCNT)("INVCYCL")                             '請求項目 請求サイクル
        work.WF_SEL_INVFILINGDEPT.Text = LNM0003tbl.Rows(WW_LineCNT)("INVFILINGDEPT")                 '請求項目 請求書提出部店
        work.WF_SEL_INVKESAIKBN.Text = LNM0003tbl.Rows(WW_LineCNT)("INVKESAIKBN")                     '請求項目 請求書決済区分
        work.WF_SEL_INVSUBCD.Text = LNM0003tbl.Rows(WW_LineCNT)("INVSUBCD")                           '請求項目 請求書細分コード
        work.WF_SEL_PAYKEIJYOBRANCHCD.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYKEIJYOBRANCHCD")         '支払項目 費用計上店コード
        work.WF_SEL_PAYFILINGBRANCH.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYFILINGBRANCH")             '支払項目 支払書提出支店
        work.WF_SEL_TAXCALCUNIT.Text = LNM0003tbl.Rows(WW_LineCNT)("TAXCALCUNIT")                     '支払項目 消費税計算単位
        work.WF_SEL_PAYKESAIKBN.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYKESAIKBN")                     '支払項目 決済区分
        work.WF_SEL_PAYBANKCD.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYBANKCD")                         '支払項目 銀行コード
        work.WF_SEL_PAYBANKBRANCHCD.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYBANKBRANCHCD")             '支払項目 銀行支店コード
        work.WF_SEL_PAYACCOUNTTYPE.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYACCOUNTTYPE")               '支払項目 口座種別
        work.WF_SEL_PAYACCOUNTNO.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYACCOUNTNO")                   '支払項目 口座番号
        work.WF_SEL_PAYACCOUNTNM.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYACCOUNTNM")                   '支払項目 口座名義人
        work.WF_SEL_PAYTEKIYO.Text = LNM0003tbl.Rows(WW_LineCNT)("PAYTEKIYO")                         '支払項目 支払摘要
        work.WF_SEL_TIMESTAMP.Text = LNM0003tbl.Rows(WW_LineCNT)("UPDTIMSTP")                         'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                   '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0003row As DataRow In LNM0003tbl.Rows
            Select Case LNM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0003tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0003tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0003tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0003tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0003tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0003tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION2.Text) Then
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_DEPSTATION2.Text,
                                work.WF_SEL_DEPTRUSTEECD2.Text,
                                work.WF_SEL_DEPTRUSTEESUBCD2.Text,
                                work.WF_SEL_TIMESTAMP.Text)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0003WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "コンテナ取引先マスタ一覧"
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
            Case LNM0003WRKINC.FILETYPE.EXCEL
                FileName = "コンテナ取引先マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0003WRKINC.FILETYPE.PDF
                FileName = "コンテナ取引先マスタ.pdf"
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
        sheet.Columns(LNM0003WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発駅コード
        sheet.Columns(LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発受託人コード
        sheet.Columns(LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発受託人サブコード

        '入力不要列網掛け

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
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION).Value = "（必須）発駅コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = "（必須）発受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = "（必須）発受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEENM).Value = "発受託人名称"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM).Value = "発受託人サブ名称"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBKANA).Value = "発受託人名称（カナ）"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.TORICODE).Value = "取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.ELIGIBLEINVOICENUMBER).Value = "適格請求書登録番号"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKEIJYOBRANCHCD).Value = "請求項目 計上店コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVCYCL).Value = "請求項目 請求サイクル"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Value = "請求項目 請求書提出部店"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKESAIKBN).Value = "請求項目 請求書決済区分"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVSUBCD).Value = "請求項目 請求書細分コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKEIJYOBRANCHCD).Value = "支払項目 費用計上店コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYFILINGBRANCH).Value = "支払項目 支払書提出支店"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.TAXCALCUNIT).Value = "支払項目 消費税計算単位"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKESAIKBN).Value = "支払項目 決済区分"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKCD).Value = "支払項目 銀行コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCD).Value = "支払項目 銀行支店コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).Value = "支払項目 口座種別"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNO).Value = "支払項目 口座番号"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNM).Value = "支払項目 口座名義人"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYTEKIYO).Value = "支払項目 支払摘要"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVKEIJYOBRANCHCD).Value = "変換前 請求項目 計上店コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT).Value = "変換前 請求項目 請求書提出部店"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYKEIJYOBRANCHCD).Value = "変換前 支払項目 費用計上店コード"
        sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYFILINGBRANCH).Value = "変換前 支払項目 支払書提出支店"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '発駅コード
            COMMENT_get(SQLcon, "DEPSTATION", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '請求項目 計上店コード
            COMMENT_get(SQLcon, "INVKEIJYOBRANCHCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKEIJYOBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKEIJYOBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '請求項目 請求サイクル
            COMMENT_get(SQLcon, "INVCYCL", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVCYCL).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVCYCL).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '請求項目 請求書提出部店
            COMMENT_get(SQLcon, "INVFILINGDEPT", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVFILINGDEPT).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '請求項目 請求書決済区分
            COMMENT_get(SQLcon, "INVKESAIKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKESAIKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.INVKESAIKBN).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '支払項目 費用計上店コード
            COMMENT_get(SQLcon, "PAYKEIJYOBRANCHCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKEIJYOBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKEIJYOBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '支払項目 支払書提出支店
            COMMENT_get(SQLcon, "PAYFILINGBRANCH", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYFILINGBRANCH).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYFILINGBRANCH).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '支払項目消費税計算単位
            COMMENT_get(SQLcon, "TAXCALCUNIT", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.TAXCALCUNIT).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.TAXCALCUNIT).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '支払項目 口座種別
            Dim WW_PAYACCOUNTTYPE As New StringBuilder
            WW_PAYACCOUNTTYPE.AppendLine("1:当座預金")
            WW_PAYACCOUNTTYPE.AppendLine("2:普通預金")
            sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).AddComment(WW_PAYACCOUNTTYPE.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).Comment.Shape
                .Width = 150
                .Height = 60
            End With

        End Using

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)


        For Each Row As DataRow In LNM0003tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION).Value = Row("DEPSTATION") '発駅コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = Row("DEPTRUSTEECD") '発受託人コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = Row("DEPTRUSTEESUBCD") '発受託人サブコード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEENM).Value = Row("DEPTRUSTEENM") '発受託人名称	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM).Value = Row("DEPTRUSTEESUBNM") '発受託人サブ名称	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBKANA).Value = Row("DEPTRUSTEESUBKANA") '発受託人名称（カナ）	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.ELIGIBLEINVOICENUMBER).Value = Row("ELIGIBLEINVOICENUMBER") '適格請求書登録番号	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.INVKEIJYOBRANCHCD).Value = Row("INVKEIJYOBRANCHCD") '請求項目 計上店コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.INVCYCL).Value = Row("INVCYCL") '請求項目 請求サイクル	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Value = Row("INVFILINGDEPT") '請求項目 請求書提出部店	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.INVKESAIKBN).Value = Row("INVKESAIKBN") '請求項目 請求書決済区分	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.INVSUBCD).Value = Row("INVSUBCD") '請求項目 請求書細分コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKEIJYOBRANCHCD).Value = Row("PAYKEIJYOBRANCHCD") '支払項目 費用計上店コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYFILINGBRANCH).Value = Row("PAYFILINGBRANCH") '支払項目 支払書提出支店	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.TAXCALCUNIT).Value = Row("TAXCALCUNIT") '支払項目 消費税計算単位	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKESAIKBN).Value = Row("PAYKESAIKBN") '支払項目 決済区分	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKCD).Value = Row("PAYBANKCD") '支払項目 銀行コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCD).Value = Row("PAYBANKBRANCHCD") '支払項目 銀行支店コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).Value = Row("PAYACCOUNTTYPE") '支払項目 口座種別	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNO).Value = Row("PAYACCOUNTNO") '支払項目 口座番号	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNM).Value = Row("PAYACCOUNTNM") '支払項目 口座名義人	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.PAYTEKIYO).Value = Row("PAYTEKIYO") '支払項目 支払摘要	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVKEIJYOBRANCHCD).Value = Row("BEFOREINVKEIJYOBRANCHCD") '変換前 請求項目 計上店コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT).Value = Row("BEFOREINVFILINGDEPT") '変換前 請求項目 請求書提出部店	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYKEIJYOBRANCHCD).Value = Row("BEFOREPAYKEIJYOBRANCHCD") '変換前 支払項目 費用計上店コード	
            sheet.Cells(WW_ACTIVEROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYFILINGBRANCH).Value = Row("BEFOREPAYFILINGBRANCH") '変換前 支払項目 支払書提出支店	

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
                Case "DEPSTATION"         '発駅コード
                    WW_PrmData = work.CreateStationParam(Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STATION
                Case "INVKEIJYOBRANCHCD",  '請求項目計上店コード
                     "INVFILINGDEPT",      '請求項目請求書提出部店
                     "PAYKEIJYOBRANCHCD",  '支払項目費用計上店コード
                     "PAYFILINGBRANCH"     '支払項目支払書提出支店
                    WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ORG
                Case "INVCYCL",            '請求項目請求サイクル
                     "TAXCALCUNIT",        '支払項目消費税計算単位
                     "DELFLG"              '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                Case "INVKESAIKBN"        '請求項目請求書決済区分
                    WW_PrmData = work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.INV_KESAI_KBN)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_KEKKJM
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
        filePath = "D:\コンテナ取引先マスタ一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNM0003Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0003WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0003WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REKEJMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "コンテナ取引先マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0003Exceltbl) Then
            LNM0003Exceltbl = New DataTable
        End If
        If LNM0003Exceltbl.Columns.Count <> 0 Then
            LNM0003Exceltbl.Columns.Clear()
        End If
        LNM0003Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\REKEJMEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "REKEJMEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0003Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0003WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0003WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REKEJMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0003_REKEJM ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0003Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
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

        Dim LNM0003Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0003Exceltblrow = LNM0003Exceltbl.NewRow

            'LINECNT
            LNM0003Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '発駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPSTATION))
            WW_DATATYPE = DataTypeHT("DEPSTATION")
            LNM0003Exceltblrow("DEPSTATION") = LNM0003WRKINC.DataConvert("発駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEECD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEECD")
            LNM0003Exceltblrow("DEPTRUSTEECD") = LNM0003WRKINC.DataConvert("発受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEESUBCD")
            LNM0003Exceltblrow("DEPTRUSTEESUBCD") = LNM0003WRKINC.DataConvert("発受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人名称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEENM))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEENM")
            LNM0003Exceltblrow("DEPTRUSTEENM") = LNM0003WRKINC.DataConvert("発受託人名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人サブ名称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEESUBNM")
            LNM0003Exceltblrow("DEPTRUSTEESUBNM") = LNM0003WRKINC.DataConvert("発受託人サブ名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人名称（カナ）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBKANA))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEESUBKANA")
            LNM0003Exceltblrow("DEPTRUSTEESUBKANA") = LNM0003WRKINC.DataConvert("発受託人名称（カナ）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.TORICODE))
            WW_DATATYPE = DataTypeHT("TORICODE")
            LNM0003Exceltblrow("TORICODE") = LNM0003WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '適格請求書登録番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.ELIGIBLEINVOICENUMBER))
            WW_DATATYPE = DataTypeHT("ELIGIBLEINVOICENUMBER")
            LNM0003Exceltblrow("ELIGIBLEINVOICENUMBER") = LNM0003WRKINC.DataConvert("適格請求書登録番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求項目 計上店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.INVKEIJYOBRANCHCD))
            WW_DATATYPE = DataTypeHT("INVKEIJYOBRANCHCD")
            LNM0003Exceltblrow("INVKEIJYOBRANCHCD") = LNM0003WRKINC.DataConvert("請求項目 計上店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求項目 請求サイクル
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.INVCYCL))
            WW_DATATYPE = DataTypeHT("INVCYCL")
            LNM0003Exceltblrow("INVCYCL") = LNM0003WRKINC.DataConvert("請求項目 請求サイクル", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求項目 請求書提出部店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.INVFILINGDEPT))
            WW_DATATYPE = DataTypeHT("INVFILINGDEPT")
            LNM0003Exceltblrow("INVFILINGDEPT") = LNM0003WRKINC.DataConvert("請求項目 請求書提出部店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求項目 請求書決済区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.INVKESAIKBN))
            WW_DATATYPE = DataTypeHT("INVKESAIKBN")
            LNM0003Exceltblrow("INVKESAIKBN") = LNM0003WRKINC.DataConvert("請求項目 請求書決済区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求項目 請求書細分コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.INVSUBCD))
            WW_DATATYPE = DataTypeHT("INVSUBCD")
            LNM0003Exceltblrow("INVSUBCD") = LNM0003WRKINC.DataConvert("請求項目 請求書細分コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 費用計上店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKEIJYOBRANCHCD))
            WW_DATATYPE = DataTypeHT("PAYKEIJYOBRANCHCD")
            LNM0003Exceltblrow("PAYKEIJYOBRANCHCD") = LNM0003WRKINC.DataConvert("支払項目 費用計上店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 支払書提出支店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYFILINGBRANCH))
            WW_DATATYPE = DataTypeHT("PAYFILINGBRANCH")
            LNM0003Exceltblrow("PAYFILINGBRANCH") = LNM0003WRKINC.DataConvert("支払項目 支払書提出支店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 消費税計算単位
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.TAXCALCUNIT))
            WW_DATATYPE = DataTypeHT("TAXCALCUNIT")
            LNM0003Exceltblrow("TAXCALCUNIT") = LNM0003WRKINC.DataConvert("支払項目 消費税計算単位", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 決済区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYKESAIKBN))
            WW_DATATYPE = DataTypeHT("PAYKESAIKBN")
            LNM0003Exceltblrow("PAYKESAIKBN") = LNM0003WRKINC.DataConvert("支払項目 決済区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 銀行コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKCD))
            WW_DATATYPE = DataTypeHT("PAYBANKCD")
            LNM0003Exceltblrow("PAYBANKCD") = LNM0003WRKINC.DataConvert("支払項目 銀行コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 銀行支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCD))
            WW_DATATYPE = DataTypeHT("PAYBANKBRANCHCD")
            LNM0003Exceltblrow("PAYBANKBRANCHCD") = LNM0003WRKINC.DataConvert("支払項目 銀行支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 口座種別
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE))
            WW_DATATYPE = DataTypeHT("PAYACCOUNTTYPE")
            LNM0003Exceltblrow("PAYACCOUNTTYPE") = LNM0003WRKINC.DataConvert("支払項目 口座種別", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 口座番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNO))
            WW_DATATYPE = DataTypeHT("PAYACCOUNTNO")
            LNM0003Exceltblrow("PAYACCOUNTNO") = LNM0003WRKINC.DataConvert("支払項目 口座番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 口座名義人
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYACCOUNTNM))
            WW_DATATYPE = DataTypeHT("PAYACCOUNTNM")
            LNM0003Exceltblrow("PAYACCOUNTNM") = LNM0003WRKINC.DataConvert("支払項目 口座名義人", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払項目 支払摘要
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.PAYTEKIYO))
            WW_DATATYPE = DataTypeHT("PAYTEKIYO")
            LNM0003Exceltblrow("PAYTEKIYO") = LNM0003WRKINC.DataConvert("支払項目 支払摘要", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前 請求項目 計上店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVKEIJYOBRANCHCD))
            WW_DATATYPE = DataTypeHT("BEFOREINVKEIJYOBRANCHCD")
            LNM0003Exceltblrow("BEFOREINVKEIJYOBRANCHCD") = LNM0003WRKINC.DataConvert("変換前 請求項目 計上店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前 請求項目 請求書提出部店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT))
            WW_DATATYPE = DataTypeHT("BEFOREINVFILINGDEPT")
            LNM0003Exceltblrow("BEFOREINVFILINGDEPT") = LNM0003WRKINC.DataConvert("変換前 請求項目 請求書提出部店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前 支払項目 費用計上店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYKEIJYOBRANCHCD))
            WW_DATATYPE = DataTypeHT("BEFOREPAYKEIJYOBRANCHCD")
            LNM0003Exceltblrow("BEFOREPAYKEIJYOBRANCHCD") = LNM0003WRKINC.DataConvert("変換前 支払項目 費用計上店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前 支払項目 支払書提出支店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.BEFOREPAYFILINGBRANCH))
            WW_DATATYPE = DataTypeHT("BEFOREPAYFILINGBRANCH")
            LNM0003Exceltblrow("BEFOREPAYFILINGBRANCH") = LNM0003WRKINC.DataConvert("変換前 支払項目 支払書提出支店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0003WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0003Exceltblrow("DELFLG") = LNM0003WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0003Exceltbl.Rows.Add(LNM0003Exceltblrow)

        Next
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DEPSTATION")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0003_REKEJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(DEPSTATION, '0')             = @DEPSTATION ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEECD, '0')             = @DEPTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEESUBCD, '0')             = @DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEENM, '')             = @DEPTRUSTEENM ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEESUBNM, '')             = @DEPTRUSTEESUBNM ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEESUBKANA, '')             = @DEPTRUSTEESUBKANA ")
        SQLStr.AppendLine("    AND  coalesce(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  coalesce(ELIGIBLEINVOICENUMBER, '')             = @ELIGIBLEINVOICENUMBER ")
        SQLStr.AppendLine("    AND  coalesce(INVKEIJYOBRANCHCD, '')             = @INVKEIJYOBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(INVCYCL, '0')             = @INVCYCL ")
        SQLStr.AppendLine("    AND  coalesce(INVFILINGDEPT, '')             = @INVFILINGDEPT ")
        SQLStr.AppendLine("    AND  coalesce(INVKESAIKBN, '0')             = @INVKESAIKBN ")
        SQLStr.AppendLine("    AND  coalesce(INVSUBCD, '0')             = @INVSUBCD ")
        SQLStr.AppendLine("    AND  coalesce(PAYKEIJYOBRANCHCD, '')             = @PAYKEIJYOBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(PAYFILINGBRANCH, '')             = @PAYFILINGBRANCH ")
        SQLStr.AppendLine("    AND  coalesce(TAXCALCUNIT, '0')             = @TAXCALCUNIT ")
        SQLStr.AppendLine("    AND  coalesce(PAYKESAIKBN, '')             = @PAYKESAIKBN ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKCD, '0')             = @PAYBANKCD ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKBRANCHCD, '0')             = @PAYBANKBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTTYPE, '0')             = @PAYACCOUNTTYPE ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTNO, '')             = @PAYACCOUNTNO ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTNM, '')             = @PAYACCOUNTNM ")
        SQLStr.AppendLine("    AND  coalesce(PAYTEKIYO, '')             = @PAYTEKIYO ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREINVKEIJYOBRANCHCD, '')             = @BEFOREINVKEIJYOBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREINVFILINGDEPT, '')             = @BEFOREINVFILINGDEPT ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREPAYKEIJYOBRANCHCD, '')             = @BEFOREPAYKEIJYOBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREPAYFILINGBRANCH, '')             = @BEFOREPAYFILINGBRANCH ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_DEPTRUSTEENM As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEENM", MySqlDbType.VarChar, 32)         '発受託人名称
                Dim P_DEPTRUSTEESUBNM As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBNM", MySqlDbType.VarChar, 18)         '発受託人サブ名称
                Dim P_DEPTRUSTEESUBKANA As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBKANA", MySqlDbType.VarChar, 20)         '発受託人名称（カナ）
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_ELIGIBLEINVOICENUMBER As MySqlParameter = SQLcmd.Parameters.Add("@ELIGIBLEINVOICENUMBER", MySqlDbType.VarChar, 20)         '適格請求書登録番号
                Dim P_INVKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@INVKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '請求項目 計上店コード
                Dim P_INVCYCL As MySqlParameter = SQLcmd.Parameters.Add("@INVCYCL", MySqlDbType.VarChar, 2)         '請求項目 請求サイクル
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求項目 請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求項目 請求書決済区分
                Dim P_INVSUBCD As MySqlParameter = SQLcmd.Parameters.Add("@INVSUBCD", MySqlDbType.VarChar, 2)         '請求項目 請求書細分コード
                Dim P_PAYKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '支払項目 費用計上店コード
                Dim P_PAYFILINGBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@PAYFILINGBRANCH", MySqlDbType.VarChar, 6)         '支払項目 支払書提出支店
                Dim P_TAXCALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@TAXCALCUNIT", MySqlDbType.VarChar, 2)         '支払項目 消費税計算単位
                Dim P_PAYKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@PAYKESAIKBN", MySqlDbType.VarChar, 1)         '支払項目 決済区分
                Dim P_PAYBANKCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKCD", MySqlDbType.VarChar, 4)         '支払項目 銀行コード
                Dim P_PAYBANKBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHCD", MySqlDbType.VarChar, 3)         '支払項目 銀行支店コード
                Dim P_PAYACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPE", MySqlDbType.VarChar, 1)         '支払項目 口座種別
                Dim P_PAYACCOUNTNO As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNO", MySqlDbType.VarChar, 8)         '支払項目 口座番号
                Dim P_PAYACCOUNTNM As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNM", MySqlDbType.VarChar, 30)         '支払項目 口座名義人
                Dim P_PAYTEKIYO As MySqlParameter = SQLcmd.Parameters.Add("@PAYTEKIYO", MySqlDbType.VarChar, 42)         '支払項目 支払摘要
                Dim P_BEFOREINVKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '変換前 請求項目 計上店コード
                Dim P_BEFOREINVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVFILINGDEPT", MySqlDbType.VarChar, 6)         '変換前 請求項目 請求書提出部店
                Dim P_BEFOREPAYKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREPAYKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '変換前 支払項目 費用計上店コード
                Dim P_BEFOREPAYFILINGBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREPAYFILINGBRANCH", MySqlDbType.VarChar, 6)         '変換前 支払項目 支払書提出支店
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_DEPTRUSTEENM.Value = WW_ROW("DEPTRUSTEENM")               '発受託人名称
                P_DEPTRUSTEESUBNM.Value = WW_ROW("DEPTRUSTEESUBNM")               '発受託人サブ名称
                P_DEPTRUSTEESUBKANA.Value = WW_ROW("DEPTRUSTEESUBKANA")               '発受託人名称（カナ）
                P_TORICODE.Value = WW_ROW("TORICODE")               '取引先コード
                P_ELIGIBLEINVOICENUMBER.Value = WW_ROW("ELIGIBLEINVOICENUMBER")               '適格請求書登録番号
                P_INVKEIJYOBRANCHCD.Value = WW_ROW("INVKEIJYOBRANCHCD")               '請求項目 計上店コード
                P_INVCYCL.Value = WW_ROW("INVCYCL")               '請求項目 請求サイクル
                P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")               '請求項目 請求書提出部店
                P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")               '請求項目 請求書決済区分
                P_INVSUBCD.Value = WW_ROW("INVSUBCD")               '請求項目 請求書細分コード
                P_PAYKEIJYOBRANCHCD.Value = WW_ROW("PAYKEIJYOBRANCHCD")               '支払項目 費用計上店コード
                P_PAYFILINGBRANCH.Value = WW_ROW("PAYFILINGBRANCH")               '支払項目 支払書提出支店
                P_TAXCALCUNIT.Value = WW_ROW("TAXCALCUNIT")               '支払項目 消費税計算単位
                P_PAYKESAIKBN.Value = WW_ROW("PAYKESAIKBN")               '支払項目 決済区分
                P_PAYBANKCD.Value = WW_ROW("PAYBANKCD")               '支払項目 銀行コード
                P_PAYBANKBRANCHCD.Value = WW_ROW("PAYBANKBRANCHCD")               '支払項目 銀行支店コード
                P_PAYACCOUNTTYPE.Value = WW_ROW("PAYACCOUNTTYPE")               '支払項目 口座種別
                P_PAYACCOUNTNO.Value = WW_ROW("PAYACCOUNTNO")               '支払項目 口座番号
                P_PAYACCOUNTNM.Value = WW_ROW("PAYACCOUNTNM")               '支払項目 口座名義人
                P_PAYTEKIYO.Value = WW_ROW("PAYTEKIYO")               '支払項目 支払摘要
                P_BEFOREINVKEIJYOBRANCHCD.Value = WW_ROW("BEFOREINVKEIJYOBRANCHCD")               '変換前 請求項目 計上店コード
                P_BEFOREINVFILINGDEPT.Value = WW_ROW("BEFOREINVFILINGDEPT")               '変換前 請求項目 請求書提出部店
                P_BEFOREPAYKEIJYOBRANCHCD.Value = WW_ROW("BEFOREPAYKEIJYOBRANCHCD")               '変換前 支払項目 費用計上店コード
                P_BEFOREPAYFILINGBRANCH.Value = WW_ROW("BEFOREPAYFILINGBRANCH")               '変換前 支払項目 支払書提出支店
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003_REKEJM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003_REKEJM SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0003_REKEJM LNM0003")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @DEPSTATION AS DEPSTATION ")
        SQLStr.AppendLine("             ,@DEPTRUSTEECD AS DEPTRUSTEECD ")
        SQLStr.AppendLine("             ,@DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@DEPTRUSTEENM AS DEPTRUSTEENM ")
        SQLStr.AppendLine("             ,@DEPTRUSTEESUBNM AS DEPTRUSTEESUBNM ")
        SQLStr.AppendLine("             ,@DEPTRUSTEESUBKANA AS DEPTRUSTEESUBKANA ")
        SQLStr.AppendLine("             ,@TORICODE AS TORICODE ")
        SQLStr.AppendLine("             ,@ELIGIBLEINVOICENUMBER AS ELIGIBLEINVOICENUMBER ")
        SQLStr.AppendLine("             ,@INVKEIJYOBRANCHCD AS INVKEIJYOBRANCHCD ")
        SQLStr.AppendLine("             ,@INVCYCL AS INVCYCL ")
        SQLStr.AppendLine("             ,@INVFILINGDEPT AS INVFILINGDEPT ")
        SQLStr.AppendLine("             ,@INVKESAIKBN AS INVKESAIKBN ")
        SQLStr.AppendLine("             ,@INVSUBCD AS INVSUBCD ")
        SQLStr.AppendLine("             ,@PAYKEIJYOBRANCHCD AS PAYKEIJYOBRANCHCD ")
        SQLStr.AppendLine("             ,@PAYFILINGBRANCH AS PAYFILINGBRANCH ")
        SQLStr.AppendLine("             ,@TAXCALCUNIT AS TAXCALCUNIT ")
        SQLStr.AppendLine("             ,@PAYKESAIKBN AS PAYKESAIKBN ")
        SQLStr.AppendLine("             ,@PAYBANKCD AS PAYBANKCD ")
        SQLStr.AppendLine("             ,@PAYBANKBRANCHCD AS PAYBANKBRANCHCD ")
        SQLStr.AppendLine("             ,@PAYACCOUNTTYPE AS PAYACCOUNTTYPE ")
        SQLStr.AppendLine("             ,@PAYACCOUNTNO AS PAYACCOUNTNO ")
        SQLStr.AppendLine("             ,@PAYACCOUNTNM AS PAYACCOUNTNM ")
        SQLStr.AppendLine("             ,@PAYTEKIYO AS PAYTEKIYO ")
        SQLStr.AppendLine("             ,@BEFOREINVKEIJYOBRANCHCD AS BEFOREINVKEIJYOBRANCHCD ")
        SQLStr.AppendLine("             ,@BEFOREINVFILINGDEPT AS BEFOREINVFILINGDEPT ")
        SQLStr.AppendLine("             ,@BEFOREPAYKEIJYOBRANCHCD AS BEFOREPAYKEIJYOBRANCHCD ")
        SQLStr.AppendLine("             ,@BEFOREPAYFILINGBRANCH AS BEFOREPAYFILINGBRANCH ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0003.DEPSTATION = EXCEL.DEPSTATION ")
        SQLStr.AppendLine("         AND LNM0003.DEPTRUSTEECD = EXCEL.DEPTRUSTEECD ")
        SQLStr.AppendLine("         AND LNM0003.DEPTRUSTEESUBCD = EXCEL.DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0003.DEPTRUSTEENM =  EXCEL.DEPTRUSTEENM")
        SQLStr.AppendLine("         ,LNM0003.DEPTRUSTEESUBNM =  EXCEL.DEPTRUSTEESUBNM")
        SQLStr.AppendLine("         ,LNM0003.DEPTRUSTEESUBKANA =  EXCEL.DEPTRUSTEESUBKANA")
        SQLStr.AppendLine("         ,LNM0003.TORICODE =  EXCEL.TORICODE")
        SQLStr.AppendLine("         ,LNM0003.ELIGIBLEINVOICENUMBER =  EXCEL.ELIGIBLEINVOICENUMBER")
        SQLStr.AppendLine("         ,LNM0003.INVKEIJYOBRANCHCD =  EXCEL.INVKEIJYOBRANCHCD")
        SQLStr.AppendLine("         ,LNM0003.INVCYCL =  EXCEL.INVCYCL")
        SQLStr.AppendLine("         ,LNM0003.INVFILINGDEPT =  EXCEL.INVFILINGDEPT")
        SQLStr.AppendLine("         ,LNM0003.INVKESAIKBN =  EXCEL.INVKESAIKBN")
        SQLStr.AppendLine("         ,LNM0003.INVSUBCD =  EXCEL.INVSUBCD")
        SQLStr.AppendLine("         ,LNM0003.PAYKEIJYOBRANCHCD =  EXCEL.PAYKEIJYOBRANCHCD")
        SQLStr.AppendLine("         ,LNM0003.PAYFILINGBRANCH =  EXCEL.PAYFILINGBRANCH")
        SQLStr.AppendLine("         ,LNM0003.TAXCALCUNIT =  EXCEL.TAXCALCUNIT")
        SQLStr.AppendLine("         ,LNM0003.PAYKESAIKBN =  EXCEL.PAYKESAIKBN")
        SQLStr.AppendLine("         ,LNM0003.PAYBANKCD =  EXCEL.PAYBANKCD")
        SQLStr.AppendLine("         ,LNM0003.PAYBANKBRANCHCD =  EXCEL.PAYBANKBRANCHCD")
        SQLStr.AppendLine("         ,LNM0003.PAYACCOUNTTYPE =  EXCEL.PAYACCOUNTTYPE")
        SQLStr.AppendLine("         ,LNM0003.PAYACCOUNTNO =  EXCEL.PAYACCOUNTNO")
        SQLStr.AppendLine("         ,LNM0003.PAYACCOUNTNM =  EXCEL.PAYACCOUNTNM")
        SQLStr.AppendLine("         ,LNM0003.PAYTEKIYO =  EXCEL.PAYTEKIYO")
        SQLStr.AppendLine("         ,LNM0003.BEFOREINVKEIJYOBRANCHCD =  EXCEL.BEFOREINVKEIJYOBRANCHCD")
        SQLStr.AppendLine("         ,LNM0003.BEFOREINVFILINGDEPT =  EXCEL.BEFOREINVFILINGDEPT")
        SQLStr.AppendLine("         ,LNM0003.BEFOREPAYKEIJYOBRANCHCD =  EXCEL.BEFOREPAYKEIJYOBRANCHCD")
        SQLStr.AppendLine("         ,LNM0003.BEFOREPAYFILINGBRANCH =  EXCEL.BEFOREPAYFILINGBRANCH")
        SQLStr.AppendLine("         ,LNM0003.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0003.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0003.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0003.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0003.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @DEPSTATION  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,@TORICODE  ")
        SQLStr.AppendLine("        ,@ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,@INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,@INVCYCL  ")
        SQLStr.AppendLine("        ,@INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,@INVKESAIKBN  ")
        SQLStr.AppendLine("        ,@INVSUBCD  ")
        SQLStr.AppendLine("        ,@PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,@PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,@TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,@PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,@PAYBANKCD  ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,@PAYTEKIYO  ")
        SQLStr.AppendLine("        ,@BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,@BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,@BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,@BEFOREPAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_DEPTRUSTEENM As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEENM", MySqlDbType.VarChar, 32)         '発受託人名称
                Dim P_DEPTRUSTEESUBNM As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBNM", MySqlDbType.VarChar, 18)         '発受託人サブ名称
                Dim P_DEPTRUSTEESUBKANA As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBKANA", MySqlDbType.VarChar, 20)         '発受託人名称（カナ）
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_ELIGIBLEINVOICENUMBER As MySqlParameter = SQLcmd.Parameters.Add("@ELIGIBLEINVOICENUMBER", MySqlDbType.VarChar, 20)         '適格請求書登録番号
                Dim P_INVKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@INVKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '請求項目 計上店コード
                Dim P_INVCYCL As MySqlParameter = SQLcmd.Parameters.Add("@INVCYCL", MySqlDbType.VarChar, 2)         '請求項目 請求サイクル
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求項目 請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求項目 請求書決済区分
                Dim P_INVSUBCD As MySqlParameter = SQLcmd.Parameters.Add("@INVSUBCD", MySqlDbType.VarChar, 2)         '請求項目 請求書細分コード
                Dim P_PAYKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '支払項目 費用計上店コード
                Dim P_PAYFILINGBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@PAYFILINGBRANCH", MySqlDbType.VarChar, 6)         '支払項目 支払書提出支店
                Dim P_TAXCALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@TAXCALCUNIT", MySqlDbType.VarChar, 2)         '支払項目 消費税計算単位
                Dim P_PAYKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@PAYKESAIKBN", MySqlDbType.VarChar, 1)         '支払項目 決済区分
                Dim P_PAYBANKCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKCD", MySqlDbType.VarChar, 4)         '支払項目 銀行コード
                Dim P_PAYBANKBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHCD", MySqlDbType.VarChar, 3)         '支払項目 銀行支店コード
                Dim P_PAYACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPE", MySqlDbType.VarChar, 1)         '支払項目 口座種別
                Dim P_PAYACCOUNTNO As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNO", MySqlDbType.VarChar, 8)         '支払項目 口座番号
                Dim P_PAYACCOUNTNM As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNM", MySqlDbType.VarChar, 30)         '支払項目 口座名義人
                Dim P_PAYTEKIYO As MySqlParameter = SQLcmd.Parameters.Add("@PAYTEKIYO", MySqlDbType.VarChar, 42)         '支払項目 支払摘要
                Dim P_BEFOREINVKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '変換前 請求項目 計上店コード
                Dim P_BEFOREINVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVFILINGDEPT", MySqlDbType.VarChar, 6)         '変換前 請求項目 請求書提出部店
                Dim P_BEFOREPAYKEIJYOBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREPAYKEIJYOBRANCHCD", MySqlDbType.VarChar, 6)         '変換前 支払項目 費用計上店コード
                Dim P_BEFOREPAYFILINGBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREPAYFILINGBRANCH", MySqlDbType.VarChar, 6)         '変換前 支払項目 支払書提出支店
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
                '発駅コード
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")
                '発受託人コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")
                '発受託人サブコード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")
                P_DEPTRUSTEENM.Value = WW_ROW("DEPTRUSTEENM")               '発受託人名称
                '発受託人サブ名称
                If Not WW_ROW("DEPTRUSTEESUBNM") = "" Then
                    P_DEPTRUSTEESUBNM.Value = WW_ROW("DEPTRUSTEESUBNM")
                Else
                    P_DEPTRUSTEESUBNM.Value = DBNull.Value
                End If
                '発受託人名称（カナ）
                If Not WW_ROW("DEPTRUSTEESUBKANA") = "" Then
                    P_DEPTRUSTEESUBKANA.Value = WW_ROW("DEPTRUSTEESUBKANA")
                Else
                    P_DEPTRUSTEESUBKANA.Value = DBNull.Value
                End If
                '取引先コード
                If Not WW_ROW("TORICODE") = "" Then
                    P_TORICODE.Value = WW_ROW("TORICODE")
                Else
                    P_TORICODE.Value = DBNull.Value
                End If
                '適格請求書登録番号
                If Not WW_ROW("ELIGIBLEINVOICENUMBER") = "" Then
                    P_ELIGIBLEINVOICENUMBER.Value = WW_ROW("ELIGIBLEINVOICENUMBER")
                Else
                    P_ELIGIBLEINVOICENUMBER.Value = DBNull.Value
                End If

                ' 請求項目は全てNULLかNOT NULLを登録
                If Not Trim(Convert.ToString(WW_ROW("INVKEIJYOBRANCHCD"))) = "" OrElse
                    Not Trim(Convert.ToString(WW_ROW("INVCYCL"))) = "0" OrElse
                    Not Trim(Convert.ToString(WW_ROW("INVFILINGDEPT"))) = "" OrElse
                    Not Trim(Convert.ToString(WW_ROW("INVKESAIKBN"))) = "0" Then

                    P_INVKEIJYOBRANCHCD.Value = WW_ROW("INVKEIJYOBRANCHCD") '請求項目 計上店コード
                    P_INVCYCL.Value = WW_ROW("INVCYCL")                     '請求項目 請求サイクル
                    P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")         '請求項目 請求書提出部店
                    P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")             '請求項目 請求書決済区分
                    P_INVSUBCD.Value = WW_ROW("INVSUBCD")                   '請求項目 請求書細分コード
                Else
                    P_INVKEIJYOBRANCHCD.Value = DBNull.Value                '請求項目 計上店コード
                    P_INVCYCL.Value = DBNull.Value                          '請求項目 請求サイクル
                    P_INVFILINGDEPT.Value = DBNull.Value                    '請求項目 請求書提出部店
                    P_INVKESAIKBN.Value = DBNull.Value                      '請求項目 請求書決済区分
                    P_INVSUBCD.Value = DBNull.Value                         '請求項目 請求書細分コード
                End If

                '支払項目 費用計上店コード
                If Not WW_ROW("PAYKEIJYOBRANCHCD") = "" Then
                    P_PAYKEIJYOBRANCHCD.Value = WW_ROW("PAYKEIJYOBRANCHCD")
                Else
                    P_PAYKEIJYOBRANCHCD.Value = DBNull.Value
                End If
                '支払項目 支払書提出支店
                If Not WW_ROW("PAYFILINGBRANCH") = "" Then
                    P_PAYFILINGBRANCH.Value = WW_ROW("PAYFILINGBRANCH")
                Else
                    P_PAYFILINGBRANCH.Value = DBNull.Value
                End If
                '支払項目 消費税計算単位
                If Not WW_ROW("TAXCALCUNIT") = "0" Then
                    P_TAXCALCUNIT.Value = WW_ROW("TAXCALCUNIT")
                Else
                    P_TAXCALCUNIT.Value = DBNull.Value
                End If
                '支払項目 決済区分
                If Not WW_ROW("PAYKESAIKBN") = "" Then
                    P_PAYKESAIKBN.Value = WW_ROW("PAYKESAIKBN")
                Else
                    P_PAYKESAIKBN.Value = DBNull.Value
                End If
                '支払項目 銀行コード
                If Not WW_ROW("PAYBANKCD") = "0" Then
                    P_PAYBANKCD.Value = WW_ROW("PAYBANKCD")
                Else
                    P_PAYBANKCD.Value = DBNull.Value
                End If
                '支払項目 銀行支店コード
                If Not WW_ROW("PAYBANKBRANCHCD") = "0" Then
                    P_PAYBANKBRANCHCD.Value = WW_ROW("PAYBANKBRANCHCD")
                Else
                    P_PAYBANKBRANCHCD.Value = DBNull.Value
                End If
                '支払項目 口座種別
                If Not WW_ROW("PAYACCOUNTTYPE") = "0" Then
                    P_PAYACCOUNTTYPE.Value = WW_ROW("PAYACCOUNTTYPE")
                Else
                    P_PAYACCOUNTTYPE.Value = DBNull.Value
                End If
                '支払項目 口座番号
                If Not WW_ROW("PAYACCOUNTNO") = "" Then
                    P_PAYACCOUNTNO.Value = WW_ROW("PAYACCOUNTNO")
                Else
                    P_PAYACCOUNTNO.Value = DBNull.Value
                End If
                '支払項目 口座名義人
                If Not WW_ROW("PAYACCOUNTNM") = "" Then
                    P_PAYACCOUNTNM.Value = WW_ROW("PAYACCOUNTNM")
                Else
                    P_PAYACCOUNTNM.Value = DBNull.Value
                End If
                '支払項目 支払摘要
                If Not WW_ROW("PAYTEKIYO") = "" Then
                    P_PAYTEKIYO.Value = WW_ROW("PAYTEKIYO")
                Else
                    P_PAYTEKIYO.Value = DBNull.Value
                End If
                '変換前 請求項目 計上店コード
                If Not WW_ROW("BEFOREINVKEIJYOBRANCHCD") = "" Then
                    P_BEFOREINVKEIJYOBRANCHCD.Value = WW_ROW("BEFOREINVKEIJYOBRANCHCD")
                Else
                    P_BEFOREINVKEIJYOBRANCHCD.Value = DBNull.Value
                End If
                '変換前 請求項目 請求書提出部店
                If Not WW_ROW("BEFOREINVFILINGDEPT") = "" Then
                    P_BEFOREINVFILINGDEPT.Value = WW_ROW("BEFOREINVFILINGDEPT")
                Else
                    P_BEFOREINVFILINGDEPT.Value = DBNull.Value
                End If
                '変換前 支払項目 費用計上店コード
                If Not WW_ROW("BEFOREPAYKEIJYOBRANCHCD") = "" Then
                    P_BEFOREPAYKEIJYOBRANCHCD.Value = WW_ROW("BEFOREPAYKEIJYOBRANCHCD")
                Else
                    P_BEFOREPAYKEIJYOBRANCHCD.Value = DBNull.Value
                End If
                '変換前 支払項目 支払書提出支店
                If Not WW_ROW("BEFOREPAYFILINGBRANCH") = "" Then
                    P_BEFOREPAYFILINGBRANCH.Value = WW_ROW("BEFOREPAYFILINGBRANCH")
                Else
                    P_BEFOREPAYFILINGBRANCH.Value = DBNull.Value
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003_REST1M  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0003_REST1M  INSERTUPDATE"
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
        Dim WW_PayKesaiKbn As Integer

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
        ' 発駅コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPSTATION", WW_ROW("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            'CODENAME_get("DEPSTATION", WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
            'If Not isNormal(WW_RtnSW) Then
            '    WW_CheckMES1 = "・発駅コードエラーです。"
            '    WW_CheckMES2 = "マスタに存在しません。"
            '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
        Else
            WW_CheckMES1 = "・発駅コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 発受託人コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", WW_ROW("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・発受託人コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 発受託人名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEENM", WW_ROW("DEPTRUSTEENM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・発受託人名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 発受託人名称（カナ）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBKANA", WW_ROW("DEPTRUSTEESUBKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・発受託人名称（カナ）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 発受託人サブコード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", WW_ROW("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・発受託人サブコードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 発受託人サブ名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBNM", WW_ROW("DEPTRUSTEESUBNM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・発受託人サブ名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' グループ必須チェック(請求項目)
        If Not Trim(Convert.ToString(WW_ROW("INVKEIJYOBRANCHCD"))) = "" Then

            '請求項目 計上店コード
            If Trim(Convert.ToString(WW_ROW("INVKEIJYOBRANCHCD"))) = "" Then
                WW_CheckMES1 = "・請求項目 計上店コードエラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '請求項目 請求サイクル
            If Trim(Convert.ToString(WW_ROW("INVCYCL"))) = "0" Then
                WW_CheckMES1 = "・請求項目 請求サイクルエラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '請求項目 請求書提出部店
            If Trim(Convert.ToString(WW_ROW("INVFILINGDEPT"))) = "" Then
                WW_CheckMES1 = "・請求項目 請求書提出部店エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '請求項目 請求書決済区分
            If Trim(Convert.ToString(WW_ROW("INVKESAIKBN"))) = "0" Then
                WW_CheckMES1 = "・請求項目 請求書決済区分エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            ''請求項目 請求書細分コード
            'If Trim(Convert.ToString(WW_ROW("INVSUBCD"))) = "0" Then
            '    WW_CheckMES1 = "・請求項目 請求書細分コードエラーです。"
            '    WW_CheckMES2 = "必須入力です。"
            '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            'End If

            ' 請求項目計上店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVKEIJYOBRANCHCD", WW_ROW("INVKEIJYOBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("INVKEIJYOBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("INVKEIJYOBRANCHCD", WW_ROW("INVKEIJYOBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・請求項目計上店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・請求項目計上店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 請求項目請求サイクル(バリデーションチェック)
            If Not WW_ROW("INVCYCL") = "0" Then
                Master.CheckField(Master.USERCAMP, "INVCYCL", WW_ROW("INVCYCL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(WW_ROW("INVCYCL")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVCYCL", WW_ROW("INVCYCL"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目請求サイクルエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目請求サイクルエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 請求項目請求書提出部店(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVFILINGDEPT", WW_ROW("INVFILINGDEPT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("INVFILINGDEPT")) Then
                    ' 名称存在チェック
                    CODENAME_get("INVFILINGDEPT", WW_ROW("INVFILINGDEPT"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・請求項目請求書提出部店エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・請求項目請求書提出部店エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 請求項目請求書決済区分(バリデーションチェック)
            If Not WW_ROW("INVKESAIKBN") = "0" Then
                Master.CheckField(Master.USERCAMP, "INVKESAIKBN", WW_ROW("INVKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(WW_ROW("INVKESAIKBN")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVKESAIKBN", WW_ROW("INVKESAIKBN"), WW_ROW("TORICODE"), WW_ROW("INVFILINGDEPT"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目請求書決済区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目請求書決済区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 請求項目請求書細分コード(バリデーションチェック)
            If Not WW_ROW("INVSUBCD") = "0" Then
                Master.CheckField(Master.USERCAMP, "INVSUBCD", WW_ROW("INVSUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    '    If Not String.IsNullOrEmpty(WW_ROW("INVSUBCD")) Then
                    '        ' 名称存在チェック
                    '        CODENAME_get("INVSUBCD", WW_ROW("INVSUBCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    '        If Not isNormal(WW_RtnSW) Then
                    '            WW_CheckMES1 = "・請求項目請求書細分コードエラーです。"
                    '            WW_CheckMES2 = "マスタに存在しません。"
                    '            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '            WW_LineErr = "ERR"
                    '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    '        End If
                    '    End If
                    'Else
                    WW_CheckMES1 = "・請求項目請求書細分コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If

        ' グループ必須チェック(請求項目)
        If Not Trim(Convert.ToString(WW_ROW("PAYKEIJYOBRANCHCD"))) = "" Then

            '支払項目 費用計上店コード
            If Trim(Convert.ToString(WW_ROW("PAYKEIJYOBRANCHCD"))) = "" Then
                WW_CheckMES1 = "・支払項目 費用計上店コードエラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 支払書提出支店
            If Trim(Convert.ToString(WW_ROW("PAYFILINGBRANCH"))) = "" Then
                WW_CheckMES1 = "・支払項目 支払書提出支店エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 消費税計算単位
            If Trim(Convert.ToString(WW_ROW("TAXCALCUNIT"))) = "0" Then
                WW_CheckMES1 = "・支払項目 消費税計算単位エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 銀行コード
            If Trim(Convert.ToString(WW_ROW("PAYBANKCD"))) = "0" Then
                WW_CheckMES1 = "・支払項目 銀行コードエラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 銀行支店コード
            If Trim(Convert.ToString(WW_ROW("PAYBANKBRANCHCD"))) = "0" Then
                WW_CheckMES1 = "・支払項目 銀行支店コードエラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 口座種別
            If Trim(Convert.ToString(WW_ROW("PAYACCOUNTTYPE"))) = "0" Then
                WW_CheckMES1 = "・支払項目 口座種別エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 口座番号
            If Trim(Convert.ToString(WW_ROW("PAYACCOUNTNO"))) = "" Then
                WW_CheckMES1 = "・支払項目 口座番号エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            '支払項目 口座名義人
            If Trim(Convert.ToString(WW_ROW("PAYACCOUNTNM"))) = "" Then
                WW_CheckMES1 = "・支払項目 口座名義人エラーです。"
                WW_CheckMES2 = "必須入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            End If

            ' 支払項目費用計上店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYKEIJYOBRANCHCD", WW_ROW("PAYKEIJYOBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("PAYKEIJYOBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("PAYKEIJYOBRANCHCD", WW_ROW("PAYKEIJYOBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・支払項目費用計上店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・支払項目費用計上店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目支払書提出支店(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYFILINGBRANCH", WW_ROW("PAYFILINGBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("PAYFILINGBRANCH")) Then
                    ' 名称存在チェック
                    CODENAME_get("PAYFILINGBRANCH", WW_ROW("PAYFILINGBRANCH"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・支払項目支払書提出支店エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・支払項目支払書提出支店エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目消費税計算単位(バリデーションチェック)
            If Not WW_ROW("TAXCALCUNIT") = "0" Then
                Master.CheckField(Master.USERCAMP, "TAXCALCUNIT", WW_ROW("TAXCALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(WW_ROW("TAXCALCUNIT")) Then
                        ' 名称存在チェック
                        CODENAME_get("TAXCALCUNIT", WW_ROW("TAXCALCUNIT"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・支払項目消費税計算単位エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目消費税計算単位エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 支払項目決済区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYKESAIKBN", WW_ROW("PAYKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("PAYKESAIKBN")) Then
                    WW_PayKesaiKbn = Integer.Parse(WW_ROW("PAYKESAIKBN"))
                    If 1 >= WW_PayKesaiKbn OrElse WW_PayKesaiKbn >= 7 Then
                        WW_CheckMES1 = "・支払項目決済区分エラーです。"
                        WW_CheckMES2 = "2～6で入力してください。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・支払項目決済区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目銀行コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYBANKCD", WW_ROW("PAYBANKCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目銀行コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目銀行支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHCD", WW_ROW("PAYBANKBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目銀行支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目口座種別(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPE", WW_ROW("PAYACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目口座種別エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目口座番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTNO", WW_ROW("PAYACCOUNTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目口座番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目口座名義人(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTNM", WW_ROW("PAYACCOUNTNM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目口座名義人エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払項目支払摘要(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYTEKIYO", WW_ROW("PAYTEKIYO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払項目支払摘要エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        '' グループ必須チェック(請求項目)
        'If Not Trim(Convert.ToString(WW_ROW("INVKEIJYOBRANCHCD"))) = "" OrElse
        '        Not Trim(Convert.ToString(WW_ROW("INVCYCL"))) = "0" OrElse
        '        Not Trim(Convert.ToString(WW_ROW("INVFILINGDEPT"))) = "" OrElse
        '        Not Trim(Convert.ToString(WW_ROW("INVKESAIKBN"))) = "0" Then

        '    '請求項目 計上店コード
        '    If Trim(Convert.ToString(WW_ROW("INVKEIJYOBRANCHCD"))) = "" Then
        '        WW_CheckMES1 = "・請求項目 計上店コードエラーです。"
        '        WW_CheckMES2 = "必須入力です。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
        '    End If

        '    '請求項目 請求サイクル
        '    If Trim(Convert.ToString(WW_ROW("INVCYCL"))) = "0" Then
        '        WW_CheckMES1 = "・請求項目 請求サイクルエラーです。"
        '        WW_CheckMES2 = "必須入力です。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
        '    End If

        '    '請求項目 請求書提出部店
        '    If Trim(Convert.ToString(WW_ROW("INVFILINGDEPT"))) = "" Then
        '        WW_CheckMES1 = "・請求項目 請求書提出部店エラーです。"
        '        WW_CheckMES2 = "必須入力です。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
        '    End If

        '    '請求項目 請求書決済区分
        '    If Trim(Convert.ToString(WW_ROW("INVKESAIKBN"))) = "0" Then
        '        WW_CheckMES1 = "・請求項目 請求書決済区分エラーです。"
        '        WW_CheckMES2 = "必須入力です。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
        '    End If

        '    ''請求項目 請求書細分コード
        '    'If Trim(Convert.ToString(WW_ROW("INVSUBCD"))) = "0" Then
        '    '    WW_CheckMES1 = "・請求項目 請求書細分コードエラーです。"
        '    '    WW_CheckMES2 = "必須入力です。"
        '    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    '    WW_LineErr = "ERR"
        '    '    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
        '    'End If
        'End If

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
                Case "DEPSTATION"         '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE1, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "INVKEIJYOBRANCHCD"  '請求項目計上店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "INVCYCL"            '請求項目請求サイクル
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "INVFILINGDEPT"      '請求項目請求書提出部店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "INVKESAIKBN"        '請求項目請求書決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE1, O_TEXT, O_RTN, work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.INV_KESAI_KBN, I_VALUE2, I_VALUE3))
                Case "PAYKEIJYOBRANCHCD"  '支払項目費用計上店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "PAYFILINGBRANCH"    '支払項目支払書提出支店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "TAXCALCUNIT"        '支払項目消費税計算単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub REKEJMEXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        'コンテナ決済マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DEPSTATION")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0003_REKEJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD        = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD     = @DEPTRUSTEESUBCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード

                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード

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
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.NEWDATA '新規
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0114_REKEJHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
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
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
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
        SQLStr.AppendLine("        LNG.LNM0003_REKEJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD        = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD     = @DEPTRUSTEESUBCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0114_REKEJHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0114_REKEJHIST  INSERT"
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



End Class

