''************************************************************
' キロ程マスタメンテ一覧画面
' 作成日 2023/10/25
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴:2023/10/25 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' キロ程マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0011RekmtmList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0011tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNM0011Exceltbl As New DataTable                        'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0011tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0011WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0011WRKINC.FILETYPE.PDF)
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
            If Not IsNothing(LNM0011tbl) Then
                LNM0011tbl.Clear()
                LNM0011tbl.Dispose()
                LNM0011tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0011WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0011S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0011D Then
            Master.RecoverTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNM0011tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0011tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0011tbl)

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

        If IsNothing(LNM0011tbl) Then
            LNM0011tbl = New DataTable
        End If

        If LNM0011tbl.Columns.Count <> 0 Then
            LNM0011tbl.Columns.Clear()
        End If

        LNM0011tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをキロ程マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                                       ")
        SQLStr.AppendLine("     1                                                                            AS 'SELECT'                 ")
        SQLStr.AppendLine("   , 0                                                                            AS HIDDEN                   ")
        SQLStr.AppendLine("   , 0                                                                            AS LINECNT                  ")
        SQLStr.AppendLine("   , ''                                                                           AS OPERATION                ")
        SQLStr.AppendLine("   , LNM0011.UPDTIMSTP                                                            AS UPDTIMSTP                ")
        SQLStr.AppendLine("   , coalesce(RTRIM(LNM0011.DELFLG), '')                                            AS DELFLG                   ")
        SQLStr.AppendLine("   , coalesce(RTRIM(LNM0011.DEPSTATION), '')                                        AS DEPSTATION               ")
        SQLStr.AppendLine("   , coalesce(RTRIM(LNM0011.ARRSTATION), '')                                        AS ARRSTATION               ")
        SQLStr.AppendLine("   , coalesce(FORMAT(LNM0011.FROMYMD, 'yyyy/MM/dd'), '')                            AS FROMYMD                  ")
        SQLStr.AppendLine("   , coalesce(RTRIM(LNM0011.KIRO), '')                                              AS KIRO                     ")
        SQLStr.AppendLine(" FROM                                                                                                         ")
        SQLStr.AppendLine("     LNG.LNM0011_REKMTM LNM0011                                                                               ")
        SQLStr.AppendLine(" WHERE LNM0011.DELFLG  = @DELFLG ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ' 発駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION_S.Text) Then
            SQLStr.AppendLine("    AND LNM0011.DEPSTATION   = @DEPSTATION")
        End If
        ' 着駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ARRSTATION_S.Text) Then
            SQLStr.AppendLine("    AND LNM0011.ARRSTATION   = @ARRSTATION")
        End If

        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("    LNM0011.DEPSTATION")
        SQLStr.AppendLine("   ,LNM0011.ARRSTATION")
        SQLStr.AppendLine("   ,LNM0011.FROMYMD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)  '削除フラグ
                P_DELFLG.Value = work.WF_SEL_DELFLG_S.Text

                If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION_S.Text) Then
                    Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)  '発駅コード
                    P_DEPSTATION.Value = work.WF_SEL_DEPSTATION_S.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_ARRSTATION_S.Text) Then
                    Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)  '着駅コード
                    P_ARRSTATION.Value = work.WF_SEL_ARRSTATION_S.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0011tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0011row As DataRow In LNM0011tbl.Rows
                    i += 1
                    LNM0011row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011L Select"
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
        For Each LNM0011row As DataRow In LNM0011tbl.Rows
            If LNM0011row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0011row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0011tbl)

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
        work.WF_SEL_DEPSTATION.Text = ""                                                      '発駅コード
        work.WF_SEL_ARRSTATION.Text = ""                                                      '着駅コード
        work.WF_SEL_FROMYMD.Text = ""                                                         '摘要年月日
        work.WF_SEL_KIRO.Text = ""                                                            'キロ程
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                           '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0011tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0011RekmtmHistory.aspx")
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
        Dim TBLview As New DataView(LNM0011tbl)
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

        work.WF_SEL_LINECNT.Text = LNM0011tbl.Rows(WW_LineCNT)("LINECNT")                                    '選択行
        work.WF_SEL_DELFLG.Text = LNM0011tbl.Rows(WW_LineCNT)("DELFLG")                                      '削除フラグ
        work.WF_SEL_DEPSTATION.Text = LNM0011tbl.Rows(WW_LineCNT)("DEPSTATION")                              '発駅コード
        work.WF_SEL_ARRSTATION.Text = LNM0011tbl.Rows(WW_LineCNT)("ARRSTATION")                              '着駅コード
        work.WF_SEL_FROMYMD.Text = LNM0011tbl.Rows(WW_LineCNT)("FROMYMD")                                    '摘要年月日
        work.WF_SEL_KIRO.Text = LNM0011tbl.Rows(WW_LineCNT)("KIRO")                                          'キロ程
        work.WF_SEL_TIMESTAMP.Text = LNM0011tbl.Rows(WW_LineCNT)("UPDTIMSTP")                                'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                          '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0011row As DataRow In LNM0011tbl.Rows
            Select Case LNM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0011tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0011tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0011tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0011tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0011tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0011tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0011tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)

        ' 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then  '発駅コード
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_DEPSTATION.Text, work.WF_SEL_ARRSTATION.Text,
                                work.WF_SEL_FROMYMD.Text, work.WF_SEL_TIMESTAMP.Text)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0011WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "キロ程マスタ一覧"
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
            Case LNM0011WRKINC.FILETYPE.EXCEL
                FileName = "キロ程マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0011WRKINC.FILETYPE.PDF
                FileName = "キロ程マスタ.pdf"
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
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発駅コード
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '着駅コード
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.FROMYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '摘要年月日

        '入力不要列網掛け
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発駅名称
        sheet.Columns(LNM0011WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '着駅名称

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
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION).Value = "（必須）発駅コード"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = "発駅名称"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION).Value = "（必須）着駅コード"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Value = "着駅名称"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.FROMYMD).Value = "（必須）摘要年月日"
        sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.KIRO).Value = "キロ程"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '発駅コード、着駅コード
            COMMENT_get(SQLcon, "STATION", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                '発駅コード
                sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                '着駅コード
                sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
        End Using

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        Dim WW_DEPSTATION As String
        Dim WW_ARRSTATION As String

        Dim WW_DEPSTATIONNM As String
        Dim WW_ARRSTATIONNM As String

        For Each Row As DataRow In LNM0011tbl.Rows
            WW_DEPSTATION = Row("DEPSTATION") '発駅コード
            WW_ARRSTATION = Row("ARRSTATION") '着駅コード

            '名称取得
            WW_DEPSTATIONNM = ""
            WW_ARRSTATIONNM = ""

            CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称
            CODENAME_get("STATION", WW_ARRSTATION, WW_Dummy, WW_Dummy, WW_ARRSTATIONNM, WW_RtnSW) '着駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION).Value = WW_DEPSTATION '発駅コード
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = WW_DEPSTATIONNM '発駅名称
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION).Value = WW_ARRSTATION '着駅コード
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATIONNM).Value = WW_ARRSTATIONNM '発駅名称
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.FROMYMD).Value = Row("FROMYMD") '摘要年月日
            sheet.Cells(WW_ACTIVEROW, LNM0011WRKINC.INOUTEXCELCOL.KIRO).Value = Row("KIRO") 'キロ程

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
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                Case "STATION"   '発駅コード・着駅コード
                    WW_PrmData = work.CreateStationParam(Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STATION
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
        filePath = "D:\キロ程マスタ一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNM0011Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0011WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0011WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "キロ程マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0011Exceltbl) Then
            LNM0011Exceltbl = New DataTable
        End If
        If LNM0011Exceltbl.Columns.Count <> 0 Then
            LNM0011Exceltbl.Columns.Clear()
        End If
        LNM0011Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\REKMTMEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "REKMTMEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0011Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0011WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0011WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0011_REKMTM ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0011Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_REKMTM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011_REKMTM Select"
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

        Dim LNM0011Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0011Exceltblrow = LNM0011Exceltbl.NewRow

            'LINECNT
            LNM0011Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '発駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0011WRKINC.INOUTEXCELCOL.DEPSTATION))
            WW_DATATYPE = DataTypeHT("DEPSTATION")
            LNM0011Exceltblrow("DEPSTATION") = LNM0011WRKINC.DataConvert("発駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '着駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0011WRKINC.INOUTEXCELCOL.ARRSTATION))
            WW_DATATYPE = DataTypeHT("ARRSTATION")
            LNM0011Exceltblrow("ARRSTATION") = LNM0011WRKINC.DataConvert("着駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '摘要年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0011WRKINC.INOUTEXCELCOL.FROMYMD))
            WW_DATATYPE = DataTypeHT("FROMYMD")
            LNM0011Exceltblrow("FROMYMD") = LNM0011WRKINC.DataConvert("摘要年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'キロ程
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0011WRKINC.INOUTEXCELCOL.KIRO))
            WW_DATATYPE = DataTypeHT("KIRO")
            LNM0011Exceltblrow("KIRO") = LNM0011WRKINC.DataConvert("キロ程", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0011WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0011Exceltblrow("DELFLG") = LNM0011WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0011Exceltbl.Rows.Add(LNM0011Exceltblrow)

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
        SQLStr.AppendLine("        LNG.LNM0011_REKMTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(DEPSTATION, '0')             = @DEPSTATION ")
        SQLStr.AppendLine("    AND  coalesce(ARRSTATION, '0')             = @ARRSTATION ")
        SQLStr.AppendLine("    AND  coalesce(FROMYMD, '0')             = @FROMYMD ")
        SQLStr.AppendLine("    AND  coalesce(KIRO, '0')             = @KIRO ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日
                Dim P_KIRO As MySqlParameter = SQLcmd.Parameters.Add("@KIRO", MySqlDbType.Decimal, 7, 1)               'キロ程
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_DEPSTATION.Value = WW_ROW("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(WW_ROW("FROMYMD"))          '摘要年月日
                P_KIRO.Value = WW_ROW("KIRO")                       'キロ程
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_REKMTM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011_REKMTM SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0011_REKMTM LNM0011")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @DEPSTATION AS DEPSTATION ")
        SQLStr.AppendLine("             ,@ARRSTATION AS ARRSTATION ")
        SQLStr.AppendLine("             ,@FROMYMD AS FROMYMD ")
        SQLStr.AppendLine("             ,@KIRO AS KIRO ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0011.DEPSTATION = EXCEL.DEPSTATION ")
        SQLStr.AppendLine("         AND LNM0011.ARRSTATION = EXCEL.ARRSTATION ")
        SQLStr.AppendLine("         AND LNM0011.FROMYMD = EXCEL.FROMYMD ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0011.KIRO =  EXCEL.KIRO")
        SQLStr.AppendLine("         ,LNM0011.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0011.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0011.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0011.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0011.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @DEPSTATION  ")
        SQLStr.AppendLine("        ,@ARRSTATION  ")
        SQLStr.AppendLine("        ,@FROMYMD  ")
        SQLStr.AppendLine("        ,@KIRO  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日
                Dim P_KIRO As MySqlParameter = SQLcmd.Parameters.Add("@KIRO", MySqlDbType.Decimal, 7, 1)               'キロ程
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
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(WW_ROW("FROMYMD"))          '摘要年月日
                P_KIRO.Value = WW_ROW("KIRO")                       'キロ程
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_REKMTM  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0011_REKMTM  INSERTUPDATE"
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

        ' 発駅コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DEPSTATION", WW_ROW("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            'CODENAME_get("STATION", WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
            'If Not isNormal(WW_RtnSW) Then
            '    WW_CheckMES1 = "・発駅コード入力エラー"
            '    WW_CheckMES2 = "マスタに存在しません。"
            '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
        Else
            WW_CheckMES1 = "・発駅コード入力エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 着駅コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ARRSTATION", WW_ROW("ARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            'CODENAME_get("STATION", WW_ROW("ARRSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
            'If Not isNormal(WW_RtnSW) Then
            '    WW_CheckMES1 = "・着駅コード入力エラー"
            '    WW_CheckMES2 = "マスタに存在しません。"
            '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
        Else
            WW_CheckMES1 = "・着駅コード入力エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 摘要年月日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FROMYMD", WW_ROW("FROMYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("FROMYMD")) Then
                WW_ROW("FROMYMD") = CDate(WW_ROW("FROMYMD")).ToString("yyyy/MM/dd")
            End If
        Else
            WW_CheckMES1 = "・摘要年月日エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' キロ程(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KIRO", WW_ROW("KIRO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・キロ程エラーです。"
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

        'キロ程マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DEPSTATION")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0011_REKMTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND ARRSTATION        = @ARRSTATION")
        SQLStr.AppendLine("    AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日

                P_DEPSTATION.Value = WW_ROW("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(WW_ROW("FROMYMD"))          '摘要年月日

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
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_REKMTM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011_REKMTM Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0098_REKMTHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
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
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
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
        SQLStr.AppendLine("        LNG.LNM0011_REKMTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND ARRSTATION        = @ARRSTATION")
        SQLStr.AppendLine("    AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = WW_ROW("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(WW_ROW("FROMYMD"))          '摘要年月日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0098_REKMTHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0098_REKMTHIST  INSERT"
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
                Case "STATION"            '発駅コード・着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE1, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class

