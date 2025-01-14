''************************************************************
' ユーザーマスタメンテナンス・一覧画面
' 作成日 2024/12/02
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/02 新規作成
'          : 
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' ユーザマスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNS0001UserList
    Inherits Page

    '○ 検索結果格納Table
    Private LNS0001tbl As DataTable         '一覧格納用テーブル
    Private LNS0001UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNS0001Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー
    Private Const ADDDATE As Integer = 90                           '有効期限追加日数

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
                    Master.RecoverTable(LNS0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNS0001WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNS0001WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
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
            If Not IsNothing(LNS0001tbl) Then
                LNS0001tbl.Clear()
                LNS0001tbl.Dispose()
                LNS0001tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNS0001WRKINC.MAPIDL
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

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0001S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0001D Then
            Master.RecoverTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNS0001tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNS0001tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNS0001tbl)

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

        If IsNothing(LNS0001tbl) Then
            LNS0001tbl = New DataTable
        End If

        If LNS0001tbl.Columns.Count <> 0 Then
            LNS0001tbl.Columns.Clear()
        End If

        LNS0001tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザマスタ、ユーザーパスワードマスタから取得する
        Dim SQLStr As String =
              " Select                                                                                              " _
            & "     1                                                                        AS 'SELECT'            " _
            & "   , 0                                                                        AS HIDDEN              " _
            & "   , 0                                                                        AS LINECNT             " _
            & "   , ''                                                                       AS OPERATION           " _
            & "   , LNS0001.UPDTIMSTP                                                        AS UPDTIMSTP           " _
            & "   , coalesce(RTRIM(LNS0001.DELFLG), '')                                      AS DELFLG              " _
            & "   , coalesce(RTRIM(LNS0001.USERID), '')                                      AS USERID              " _
            & "   , coalesce(RTRIM(LNS0001.STAFFNAMES), '')                                  AS STAFFNAMES          " _
            & "   , coalesce(RTRIM(LNS0001.STAFFNAMEL), '')                                  AS STAFFNAMEL          " _
            & "   , coalesce(RTRIM(LNS0001.MAPID), '')                                       AS MAPID               " _
            & "   , CAST(AES_DECRYPT(PASSWORD, 'loginpasskey') AS CHAR)                      AS PASSWORD            " _
            & "   , coalesce(RTRIM(LNS0002.MISSCNT), '')                                     AS MISSCNT             " _
            & "   , coalesce(DATE_FORMAT(LNS0002.PASSENDYMD, '%Y/%m/%d'), '')                AS PASSENDYMD          " _
            & "   , coalesce(DATE_FORMAT(LNS0001.STYMD, '%Y/%m/%d'), '')                     AS STYMD               " _
            & "   , coalesce(DATE_FORMAT(LNS0001.ENDYMD, '%Y/%m/%d'), '')                    AS ENDYMD              " _
            & "   , coalesce(RTRIM(LNS0001.CAMPCODE), '')                                    AS CAMPCODE            " _
            & "   , coalesce(RTRIM(LNS0001.ORG), '')                                         AS ORG                 " _
            & "   , coalesce(RTRIM(LNS0001.EMAIL), '')                                       AS EMAIL               " _
            & "   , coalesce(RTRIM(LNS0001.MENUROLE), '')                                    AS MENUROLE            " _
            & "   , coalesce(RTRIM(LNS0001.MAPROLE), '')                                     AS MAPROLE             " _
            & "   , coalesce(RTRIM(LNS0001.VIEWPROFID), '')                                  AS VIEWPROFID          " _
            & "   , coalesce(RTRIM(LNS0001.RPRTPROFID), '')                                  AS RPRTPROFID          " _
            & "   , coalesce(RTRIM(LNS0001.VARIANT), '')                                     AS VARIANT             " _
            & " FROM                                                                                                " _
            & "     COM.LNS0001_USER LNS0001                                                                        " _
            & " INNER JOIN COM.LNS0002_USERPASS LNS0002                                                             " _
            & "     ON  LNS0002.USERID = LNS0001.USERID                                                             " _
            & " INNER JOIN LNG.LNM0002_ORG LNM0002                                                                  " _
            & "     ON  LNM0002.ORGCODE = LNS0001.ORG                                                               " _
            & "     AND CURDATE() BETWEEN LNM0002.STYMD AND LNM0002.ENDYMD                                          " _
            & "     AND LNM0002.DELFLG = '0'                                                                        "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 会社コード
        If Not String.IsNullOrEmpty(work.WF_SEL_CAMPCODE.Text) Then
            SQLWhereStr = " WHERE                      " _
                        & "     LNS0001.CAMPCODE = @P1 "
        End If
        ' 有効年月日(From)
        If Not String.IsNullOrEmpty(work.WF_SEL_STYMD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                         " _
                            & "    ((LNS0001.STYMD <= @P2 " _
                            & "          AND LNS0001.ENDYMD >=  @P2 "
            Else
                SQLWhereStr &= "    AND ((LNS0001.STYMD <= @P2 "
                SQLWhereStr &= "              AND LNS0001.ENDYMD >=  @P2) "
            End If
        End If
        ' 有効年月日(To)
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            '有効年月日(From)が必須項目のため不要
            'If String.IsNullOrEmpty(SQLWhereStr) Then
            '    SQLWhereStr = " WHERE                         " _
            '                & "     LNS0001.ENDYMD >= @P3     "
            'Else
            SQLWhereStr &= "        OR (LNS0001.STYMD <= @P3 "
            SQLWhereStr &= "            AND LNS0001.ENDYMD >= @P3) "
            SQLWhereStr &= "        OR (LNS0001.STYMD >= @P3 "
            SQLWhereStr &= "            AND LNS0001.ENDYMD <= @P3)) "
        Else
            SQLWhereStr &= "        OR (LNS0001.STYMD <= @P2 "
            SQLWhereStr &= "            AND LNS0001.ENDYMD >= @P2) "
            SQLWhereStr &= "        OR (LNS0001.STYMD >= @P2 "
            SQLWhereStr &= "            AND LNS0001.ENDYMD <= @P2)) "
        End If
        ' 組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                     " _
                            & "     LNS0001.ORG = @P4     "
            Else
                SQLWhereStr &= "    AND LNS0001.ORG = @P4 "
            End If
        ElseIf Master.USER_ORG <> CONST_OFFICECODE_SYSTEM AndAlso Master.USER_ORG <> CONST_OFFICECODE_011310 Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                     " _
                            & "     LNM0002.CONTROLCODE = @P4     "
            Else
                SQLWhereStr &= "    AND LNM0002.CONTROLCODE = @P4 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNS0001.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNS0001.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY           " _
            & "     LNS0001.ORG    " _
            & "   , LNS0001.USERID "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_CAMPCODE.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 20)  '会社コード
                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_STYMD.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)          '有効年月日(From)
                    PARA2.Value = work.WF_SEL_STYMD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)          '有効年月日(To)
                    PARA3.Value = work.WF_SEL_ENDYMD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6)   '組織コード
                    PARA4.Value = work.WF_SEL_ORG.Text
                ElseIf Master.USER_ORG <> CONST_OFFICECODE_SYSTEM Or Master.USER_ORG <> CONST_OFFICECODE_011310 Then
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6)   '組織コード
                    PARA4.Value = Master.USER_ORG
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNS0001row As DataRow In LNS0001tbl.Rows
                    i += 1
                    LNS0001row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001L Select"
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
        work.WF_SEL_USERID.Text = ""                                             'ユーザID
        work.WF_SEL_STAFFNAMES.Text = ""                                         '社員名（短）
        work.WF_SEL_STAFFNAMEL.Text = ""                                         '社員名（長）
        work.WF_SEL_MAPID.Text = ""                                              '画面ＩＤ
        work.WF_SEL_PASSWORD.Text = ""                                           'パスワード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_MISSCNT.Text)  '誤り回数
        work.WF_SEL_PASSENDYMD.Text = ""                                         'パスワード有効期限
        work.WF_SEL_STYMD2.Text = ""                                             '開始年月日
        work.WF_SEL_ENDYMD2.Text = ""                                            '終了年月日
        work.WF_SEL_ORG2.Text = ""                                               '組織コード
        work.WF_SEL_EMAIL.Text = ""                                              'メールアドレス
        'work.WF_SEL_MENUROLE.Text = ""                                           'メニュー表示制御ロール
        'work.WF_SEL_MAPROLE.Text = ""                                            '画面参照更新制御ロール
        'work.WF_SEL_VIEWPROFID.Text = ""                                         '画面表示項目制御ロール
        'work.WF_SEL_RPRTPROFID.Text = ""                                         'エクセル出力制御ロール
        'work.WF_SEL_VARIANT.Text = ""                                            '画面初期値ロール
        'work.WF_SEL_APPROVALID.Text = ""                                         '承認権限ロール
        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNS0001tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNS0001UserHistory.aspx")
    End Sub


    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNS0001tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNS0001tbl)

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
        Dim TBLview As New DataView(LNS0001tbl)
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

        Dim WW_DBDataCheck As String = ""

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_USER_CAMPCODE.Text = Master.USERCAMP                         '操作ユーザー会社コード
        work.WF_SEL_LINECNT.Text = LNS0001tbl.Rows(WW_LineCNT)("LINECNT")        '選択行
        work.WF_SEL_USERID.Text = LNS0001tbl.Rows(WW_LineCNT)("USERID")          'ユーザID
        work.WF_SEL_STAFFNAMES.Text = LNS0001tbl.Rows(WW_LineCNT)("STAFFNAMES")  '社員名（短）
        work.WF_SEL_STAFFNAMEL.Text = LNS0001tbl.Rows(WW_LineCNT)("STAFFNAMEL")  '社員名（長）
        work.WF_SEL_MAPID.Text = LNS0001tbl.Rows(WW_LineCNT)("MAPID")            '画面ＩＤ
        work.WF_SEL_PASSWORD.Text = LNS0001tbl.Rows(WW_LineCNT)("PASSWORD")      'パスワード
        work.WF_SEL_MISSCNT.Text = LNS0001tbl.Rows(WW_LineCNT)("MISSCNT")        '誤り回数
        work.WF_SEL_PASSENDYMD.Text = LNS0001tbl.Rows(WW_LineCNT)("PASSENDYMD")  'パスワード有効期限
        work.WF_SEL_STYMD2.Text = LNS0001tbl.Rows(WW_LineCNT)("STYMD")           '開始年月日
        work.WF_SEL_ENDYMD2.Text = LNS0001tbl.Rows(WW_LineCNT)("ENDYMD")         '終了年月日
        work.WF_SEL_ORG2.Text = LNS0001tbl.Rows(WW_LineCNT)("ORG")               '組織コード
        work.WF_SEL_EMAIL.Text = LNS0001tbl.Rows(WW_LineCNT)("EMAIL")            'メールアドレス
        'work.WF_SEL_MENUROLE.Text = LNS0001tbl.Rows(WW_LineCNT)("MENUROLE")      'メニュー表示制御ロール
        'work.WF_SEL_MAPROLE.Text = LNS0001tbl.Rows(WW_LineCNT)("MAPROLE")        '画面参照更新制御ロール
        'work.WF_SEL_VIEWPROFID.Text = LNS0001tbl.Rows(WW_LineCNT)("VIEWPROFID")  '画面表示項目制御ロール
        'work.WF_SEL_RPRTPROFID.Text = LNS0001tbl.Rows(WW_LineCNT)("RPRTPROFID")  'エクセル出力制御ロール
        'work.WF_SEL_VARIANT.Text = LNS0001tbl.Rows(WW_LineCNT)("VARIANT")        '画面初期値ロール
        'work.WF_SEL_APPROVALID.Text = LNS0001tbl.Rows(WW_LineCNT)("APPROVALID")  '承認権限ロール
        work.WF_SEL_DELFLG.Text = LNS0001tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNS0001tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_USERID.Text, work.WF_SEL_STYMD2.Text, work.WF_SEL_TIMESTAMP.Text)
        End Using

        If Not isNormal(WW_DBDataCheck) Then
            Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

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
                Case "CAMPCODE"         '会社コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, Master.USERCAMP))
                    End If
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP))
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP))
                    End If
                Case "MENU"             'メニュー表示制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "MAP"              '画面参照更新制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "VIEW"             '画面表示項目制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "XML"              'エクセル出力制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "APPROVAL"         '承認権限ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  更新処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' ユーザーマスタ更新前処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNS0001tbl_UPD()

        ' 追加/更新の場合、DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            For Each LNS0001INProw As DataRow In LNS0001tbl.Rows
                ' マスタ更新
                UpdateMaster(SQLcon, LNS0001INProw)
            Next

        End Using

    End Sub

    ''' <summary>
    ''' ユーザマスタ更新処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByRef LNS0001row As DataRow)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ユーザマスタ)
        Dim SQLStr As String =
              "     INSERT INTO COM.LNS0001_USER            " _
            & "        (DELFLG                              " _
            & "       , USERID                              " _
            & "       , STAFFNAMES                          " _
            & "       , STAFFNAMEL                          " _
            & "       , MAPID                               " _
            & "       , STYMD                               " _
            & "       , ENDYMD                              " _
            & "       , CAMPCODE                            " _
            & "       , ORG                                 " _
            & "       , EMAIL                               " _
            & "       , MENUROLE                            " _
            & "       , MAPROLE                             " _
            & "       , VIEWPROFID                          " _
            & "       , RPRTPROFID                          " _
            & "       , VARIANT                             " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID                            " _
            & "       , UPDYMD                              " _
            & "       , UPDUSER                             " _
            & "       , UPDTERMID                           " _
            & "       , UPDPGID                             " _
            & "       , RECEIVEYMD)                         " _
            & "     VALUES                                  " _
            & "        (@P00                                " _
            & "       , @P01                                " _
            & "       , @P02                                " _
            & "       , @P03                                " _
            & "       , @P04                                " _
            & "       , @P08                                " _
            & "       , @P09                                " _
            & "       , @P10                                " _
            & "       , @P11                                " _
            & "       , @P12                                " _
            & "       , @P13                                " _
            & "       , @P14                                " _
            & "       , @P15                                " _
            & "       , @P16                                " _
            & "       , @P17                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23                                " _
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27)                               " _
            & "     ON DUPLICATE KEY UPDATE                 " _
            & "         DELFLG     = @P00                   " _
            & "       , STAFFNAMES = @P02                   " _
            & "       , STAFFNAMEL = @P03                   " _
            & "       , MAPID      = @P04                   " _
            & "       , ENDYMD     = @P09                   " _
            & "       , ORG        = @P11                   " _
            & "       , EMAIL      = @P12                   " _
            & "       , UPDYMD     = @P23                   " _
            & "       , UPDUSER    = @P24                   " _
            & "       , UPDTERMID  = @P25                   " _
            & "       , UPDPGID    = @P26                   " _
            & "       , RECEIVEYMD = @P27                   "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , USERID                                 " _
            & "   , STAFFNAMES                             " _
            & "   , STAFFNAMEL                             " _
            & "   , MAPID                                  " _
            & "   , STYMD                                  " _
            & "   , ENDYMD                                 " _
            & "   , CAMPCODE                               " _
            & "   , ORG                                    " _
            & "   , EMAIL                                  " _
            & "   , MENUROLE                               " _
            & "   , MAPROLE                                " _
            & "   , VIEWPROFID                             " _
            & "   , RPRTPROFID                             " _
            & "   , VARIANT                                " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0001_USER                       " _
            & " WHERE                                      " _
            & "         USERID = @P01                      " _
            & "     AND STYMD  = @P08                      "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザID
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 20)        '社員名（短）
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 50)        '社員名（長）
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 20)        '画面ＩＤ
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.Date)                '開始年月日
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.Date)                '終了年月日
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 2)         '会社コード
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 6)         '組織コード
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 128)       'メールアドレス
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 20)        'メニュー表示制御ロール
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 20)        '画面参照更新制御ロール
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 20)        '画面表示項目制御ロール
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 20)        'エクセル出力制御ロール
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 20)        '画面初期値ロール
                'Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 20)        '承認権限ロール
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.DateTime)            '登録年月日
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)            '更新年月日
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザID
                Dim JPARA08 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P08", MySqlDbType.Date)            '開始年月日

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNS0001row("DELFLG")                            '削除フラグ
                PARA01.Value = LNS0001row("USERID")                            'ユーザID
                PARA02.Value = LNS0001row("STAFFNAMES")                        '社員名（短）
                PARA03.Value = LNS0001row("STAFFNAMEL")                        '社員名（長）
                PARA04.Value = LNS0001row("MAPID")                             '画面ＩＤ

                If Not String.IsNullOrEmpty(RTrim(LNS0001row("STYMD"))) Then   '開始年月日
                    PARA08.Value = RTrim(LNS0001row("STYMD"))
                Else
                    PARA08.Value = C_DEFAULT_YMD
                End If

                If Not String.IsNullOrEmpty(RTrim(LNS0001row("ENDYMD"))) Then  '終了年月日
                    PARA09.Value = RTrim(LNS0001row("ENDYMD"))
                Else
                    PARA09.Value = C_DEFAULT_YMD
                End If

                PARA10.Value = LNS0001row("CAMPCODE")                          '会社コード
                PARA11.Value = LNS0001row("ORG")                               '組織コード
                PARA12.Value = LNS0001row("EMAIL")                             'メールアドレス
                'PARA13.Value = LNS0001row("MENUROLE")                          'メニュー表示制御ロール
                'PARA14.Value = LNS0001row("MAPROLE")                           '画面参照更新制御ロール
                'PARA15.Value = LNS0001row("VIEWPROFID")                        '画面表示項目制御ロール
                'PARA16.Value = LNS0001row("RPRTPROFID")                        'エクセル出力制御ロール
                'PARA17.Value = LNS0001row("VARIANT")                           '画面初期値ロール

                Master.GetFirstValue(Master.USERCAMP, "MENUROLE", PARA13.Value) 'メニュー表示制御ロール
                Master.GetFirstValue(Master.USERCAMP, "MAPROLE", PARA14.Value) '画面参照更新制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VIEWPROFID", PARA15.Value) '画面表示項目制御ロール
                Master.GetFirstValue(Master.USERCAMP, "RPRTPROFID", PARA16.Value) 'エクセル出力制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VARIANT", PARA17.Value) '画面初期値ロール

                'PARA18.Value = LNS0001row("APPROVALID")                        '承認権限ロール
                PARA19.Value = WW_DateNow                                      '登録年月日
                PARA20.Value = Master.USERID                                   '登録ユーザーＩＤ
                PARA21.Value = Master.USERTERMID                               '登録端末
                PARA22.Value = Me.GetType().BaseType.Name                      '登録プログラムＩＤ
                PARA23.Value = WW_DateNow                                      '更新年月日
                PARA24.Value = Master.USERID                                   '更新ユーザーＩＤ
                PARA25.Value = Master.USERTERMID                               '更新端末
                PARA26.Value = Me.GetType().BaseType.Name                      '更新プログラムＩＤ
                PARA27.Value = C_DEFAULT_YMD                                   '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNS0001row("USERID")                          'ユーザID
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("STYMD"))) Then  '開始年月日
                    JPARA08.Value = RTrim(LNS0001row("STYMD"))
                Else
                    JPARA08.Value = C_DEFAULT_YMD
                End If

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0001UPDtbl) Then
                        LNS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0001UPDtbl.Clear()
                    LNS0001UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0001D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0001UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"               'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                   'ログ出力

                        rightview.AddErrorReport("DB更新ジャーナル出力エラーが発生しました。システム管理者にお問い合わせ下さい。")
                        WW_ErrSW = CS0020JOURNAL.ERR
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D UPDATE_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '○ DB更新SQL(ユーザパスワードマスタ)
        SQLStr =
              "     INSERT INTO COM.LNS0002_USERPASS                                  " _
            & "        (DELFLG                                                        " _
            & "       , USERID                                                        " _
            & "       , PASSWORD                                                      " _
            & "       , MISSCNT                                                       " _
            & "       , PASSENDYMD                                                    " _
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
            & "        (@P00                                                          " _
            & "       , @P01                                                          " _
            & "       , AES_ENCRYPT(@P04, 'loginpasskey')                             " _
            & "       , @P06                                                          " _
            & "       , @P07                                                          " _
            & "       , @P19                                                          " _
            & "       , @P20                                                          " _
            & "       , @P21                                                          " _
            & "       , @P22                                                          " _
            & "       , @P23                                                          " _
            & "       , @P24                                                          " _
            & "       , @P25                                                          " _
            & "       , @P26                                                          " _
            & "       , @P27)                                                         " _
            & "     ON DUPLICATE KEY UPDATE                                           " _
            & "         DELFLG     = @P00                                             " _
            & "       , PASSWORD   = AES_ENCRYPT(@P05, 'loginpasskey')                " _
            & "       , MISSCNT    = @P06                                             " _
            & "       , PASSENDYMD = @P07                                             " _
            & "       , UPDYMD     = @P23                                             " _
            & "       , UPDUSER    = @P24                                             " _
            & "       , UPDTERMID  = @P25                                             " _
            & "       , UPDPGID    = @P26                                             " _
            & "       , RECEIVEYMD = @P27                                             "

        '○ 更新ジャーナル出力SQL
        SQLJnl =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , USERID                                 " _
            & "   , PASSWORD                               " _
            & "   , MISSCNT                                " _
            & "   , PASSENDYMD                             " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0002_USERPASS                   " _
            & " WHERE                                      " _
            & "     USERID = @P01                          "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ(ユーザパスワードマスタ)
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザID
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 200)       '初期パスワード
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 200)       'パスワード
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.Int32)                 '誤り回数
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.Date)                'パスワード有効期限
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.DateTime)            '登録年月日
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)            '更新年月日
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザID

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNS0001row("DELFLG")                                  '削除フラグ
                PARA01.Value = LNS0001row("USERID")                                  'ユーザID
                Master.GetFirstValue(Master.USERCAMP, "FIRSTPASSWORD", PARA04.Value) '初期パスワード
                PARA05.Value = LNS0001row("PASSWORD")                                'パスワード
                If Not String.IsNullOrEmpty(LNS0001row("MISSCNT")) Then              '誤り回数
                    PARA06.Value = LNS0001row("MISSCNT")
                Else
                    PARA06.Value = "0"
                End If
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("PASSENDYMD"))) Then  'パスワード有効期限
                    PARA07.Value = RTrim(LNS0001row("PASSENDYMD"))
                Else
                    PARA07.Value = C_DEFAULT_YMD
                End If
                PARA19.Value = WW_DateNow                                          '登録年月日
                PARA20.Value = Master.USERID                                       '登録ユーザーＩＤ
                PARA21.Value = Master.USERTERMID                                   '登録端末
                PARA22.Value = Me.GetType().BaseType.Name                          '登録プログラムＩＤ
                PARA23.Value = WW_DateNow                                          '更新年月日
                PARA24.Value = Master.USERID                                       '更新ユーザーＩＤ
                PARA25.Value = Master.USERTERMID                                   '更新端末
                PARA26.Value = Me.GetType().BaseType.Name                          '更新プログラムＩＤ
                PARA27.Value = C_DEFAULT_YMD                                       '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNS0001row("USERID")  'ユーザID

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0001UPDtbl) Then
                        LNS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0001UPDtbl.Clear()
                    LNS0001UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0001D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0001UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"               'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                   'ログ出力

                        rightview.AddErrorReport("DB更新ジャーナル出力エラーが発生しました。システム管理者にお問い合わせ下さい。")
                        WW_ErrSW = CS0020JOURNAL.ERR
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS000CL UPDATE_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS000CL UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
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
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNS0001WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0

        WW_STROW = WW_ACTIVEROW
        SetDETAIL(wb.ActiveSheet, WW_ACTIVEROW)
        WW_ENDROW = WW_ACTIVEROW - 1

        'プルダウンリスト作成
        SetPULLDOWNLIST(wb, WW_STROW, WW_ENDROW)

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
        wb.ActiveSheet.Range("C1").Value = "ユーザマスタ一覧"
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
            Case LNS0001WRKINC.FILETYPE.EXCEL
                FileName = "ユーザマスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNS0001WRKINC.FILETYPE.PDF
                FileName = "ユーザマスタ.pdf"
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
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.USERID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'ユーザーID
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.PASSWORD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'パスワード
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.MISSCNT).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '誤り回数
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.PASSENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'パスワード有効期限
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.STYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '開始年月日
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.ENDYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '終了年月日
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.ORG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '組織コード
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMES).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '社員名（短）
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMEL).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '社員名（長）
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.EMAIL).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'メールアドレス
        sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.MAPID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '画面ＩＤ
        'sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.MENUROLE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'メニュー表示制御ロール
        'sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.MAPROLE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '画面参照更新制御ロール
        'sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '画面表示項目制御ロール
        'sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'エクセル出力制御ロール

        '入力不要列網掛け
        'sheet.Columns(LNS0001WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発駅名称

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
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.USERID).Value = "（必須）ユーザーID"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.PASSWORD).Value = "（必須）パスワード"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MISSCNT).Value = "（必須）誤り回数"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.PASSENDYMD).Value = "（必須）パスワード有効期限"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.STYMD).Value = "（必須）開始年月日"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.ENDYMD).Value = "（必須）終了年月日"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.ORG).Value = "（必須）組織コード"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMES).Value = "（必須）社員名（短）"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMEL).Value = "（必須）社員名（長）"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.EMAIL).Value = "（必須）メールアドレス"
        sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MAPID).Value = "（必須）画面ＩＤ"
        'sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE).Value = "（必須）メニュー表示制御ロール"
        'sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE).Value = "（必須）画面参照更新制御ロール"
        'sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID).Value = "（必須）画面表示項目制御ロール"
        'sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID).Value = "（必須）エクセル出力制御ロール"
        'sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL._VARIANT).Value = "画面初期値ロール"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            'パスワード
            WW_TEXT = "パスワードは「英字大文字・小文字・数字・記号を含む12文字以上30文字以下」で設定してください。"
            sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.PASSWORD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.PASSWORD).Comment.Shape
                .Width = 300
                .Height = 50
            End With

            '組織コード
            COMMENT_get(SQLcon, "ORG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.ORG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.ORG).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            ''メニュー表示制御ロール
            'COMMENT_get(SQLcon, "MENU", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE).Comment.Shape
            '        .Width = 150
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            ''画面参照更新制御ロール
            'COMMENT_get(SQLcon, "MAP", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE).Comment.Shape
            '        .Width = 150
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            ''画面表示項目制御ロール
            'COMMENT_get(SQLcon, "VIEW", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID).Comment.Shape
            '        .Width = 150
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            ''エクセル出力制御ロール
            'COMMENT_get(SQLcon, "XML", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID).Comment.Shape
            '        .Width = 150
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

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
        SETFIXVALUELIST(subsheet, "DELFLG", LNS0001WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        '組織コード
        SETFIXVALUELIST(subsheet, "ORG", LNS0001WRKINC.INOUTEXCELCOL.ORG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.ORG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.ORG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.ORG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.ORG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        ''メニュー表示制御ロール
        'SETFIXVALUELIST(subsheet, "MENU", LNS0001WRKINC.INOUTEXCELCOL.MENUROLE, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If

        ''画面参照更新制御ロール
        'SETFIXVALUELIST(subsheet, "MAP", LNS0001WRKINC.INOUTEXCELCOL.MAPROLE, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If

        ''画面表示項目制御ロール
        'SETFIXVALUELIST(subsheet, "VIEW", LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If

        ''エクセル出力制御ロール
        'SETFIXVALUELIST(subsheet, "XML", LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If

        'メインシートをアクティブにする
        mainsheet.Activate()
        'サブシートを非表示にする
        subsheet.Visible = Visibility.Hidden
    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNS0001tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.USERID).Value = Row("USERID") 'ユーザーID
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.PASSWORD).Value = LNS0001WRKINC.FILEDOWNLOAD_PASSWORD
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.MISSCNT).Value = Row("MISSCNT") '誤り回数
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.PASSENDYMD).Value = Row("PASSENDYMD") 'パスワード有効期限
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.STYMD).Value = Row("STYMD") '開始年月日
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.ENDYMD).Value = Row("ENDYMD") '終了年月日
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.ORG).Value = Row("ORG") '組織コード
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMES).Value = Row("STAFFNAMES") '社員名（短）
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMEL).Value = Row("STAFFNAMEL") '社員名（長）
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.EMAIL).Value = Row("EMAIL") 'メールアドレス
            sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.MAPID).Value = Row("MAPID") '画面ＩＤ
            'sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE).Value = Row("MENUROLE") 'メニュー表示制御ロール
            'sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE).Value = Row("MAPROLE") '画面参照更新制御ロール
            'sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID).Value = Row("VIEWPROFID") '画面表示項目制御ロール
            'sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID).Value = Row("RPRTPROFID") 'エクセル出力制御ロール
            'sheet.Cells(WW_ACTIVEROW, LNS0001WRKINC.INOUTEXCELCOL._VARIANT).Value = Row("VARIANT") '画面初期値ロール

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
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP)
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP)
                    End If
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ORG
                Case "MENU"             'メニュー表示制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "MAP"             '画面参照更新制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "VIEW"             '画面表示項目制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "XML"             'エクセル出力制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
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
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP)
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP)
                    End If
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ORG
                Case "MENU"             'メニュー表示制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "MAP"             '画面参照更新制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "VIEW"             '画面表示項目制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "XML"             'エクセル出力制御ロール
                    WW_PrmData = work.CreateRoleList(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_ROLE
                Case "DELFLG"   '削除フラグ
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

#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\ユーザマスタ一括アップロードテスト.xlsx"

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

            Dim WW_USERM_HASDIFFERENCE As Boolean = False              'ユーザマスタ同一データチェック用(true:差異あり)
            For Each Row As DataRow In LNS0001Exceltbl.Rows
                'テーブルに同一データが存在しない場合、またはパスワード変更がある場合
                WW_USERM_HASDIFFERENCE = SameDataChk(SQLcon, Row)
                If Not WW_USERM_HASDIFFERENCE = False Or Row("PASSUPD") = "1" Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNS0001WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNS0001WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNS0001WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNS0001WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.BEFDATA Then
                        If WW_USERM_HASDIFFERENCE = True Then
                            '履歴登録(変更前)
                            InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                            If Not isNormal(WW_ErrSW) Then
                                Exit Sub
                            End If
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    If WW_USERM_HASDIFFERENCE = True Then
                        '履歴登録(新規・変更後)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "ユーザマスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNS0001Exceltbl) Then
            LNS0001Exceltbl = New DataTable
        End If
        If LNS0001Exceltbl.Columns.Count <> 0 Then
            LNS0001Exceltbl.Columns.Clear()
        End If
        LNS0001Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\USEREXCEL"
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
        Dim fileNameHead As String = "USEREXCEL_TMP_"

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
            Dim WW_USERM_HASDIFFERENCE As Boolean = False              'ユーザマスタ同一データチェック用(true:差異あり)

            For Each Row As DataRow In LNS0001Exceltbl.Rows
                'テーブルに同一データが存在しない場合、またはパスワード変更がある場合
                WW_USERM_HASDIFFERENCE = SameDataChk(SQLcon, Row)
                If Not WW_USERM_HASDIFFERENCE = False Or Row("PASSUPD") = "1" Then
                    '項目チェックスキップ(削除フラグが無効から有効になった場合)
                    If ValidationSkipChk(SQLcon, Row) = True Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNS0001WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '削除フラグのみ更新する
                        SetDelflg(SQLcon, Row, DATENOW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '履歴登録(変更後)
                        InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNS0001WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        WW_UplDelCnt += 1
                        Continue For
                    End If

                    '項目チェック
                    Master.MAPID = LNS0001WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ErrSW)
                    Master.MAPID = LNS0001WRKINC.MAPIDL
                    If Not isNormal(WW_ErrSW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If


                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.BEFDATA Then
                        If WW_USERM_HASDIFFERENCE = True Then
                            '履歴登録(変更前)
                            InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                            If Not isNormal(WW_ErrSW) Then
                                Exit Sub
                            End If
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.AFTDATA
                    End If


                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.NEWDATA '新規の場合
                            WW_UplInsCnt += 1
                        Case Else
                            WW_UplUpdCnt += 1
                    End Select

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    If WW_USERM_HASDIFFERENCE = True Then
                        '履歴登録(新規・変更後)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
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
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("   0   AS LINECNT ")
        SQLStr.AppendLine("  ,''  AS PASSUPD ") 'パスワード変更確認用
        SQLStr.AppendLine("  ,''  AS BEFOREENDYMD ") '更新前
        SQLStr.AppendLine("        ,A.USERID  ")
        SQLStr.AppendLine("  ,''  AS  PASSWORD  ")
        SQLStr.AppendLine("        ,B.MISSCNT  ")
        SQLStr.AppendLine("        ,B.PASSENDYMD  ")
        SQLStr.AppendLine("        ,A.STYMD  ")
        SQLStr.AppendLine("        ,A.ENDYMD  ")
        SQLStr.AppendLine("        ,A.ORG  ")
        SQLStr.AppendLine("        ,A.STAFFNAMES  ")
        SQLStr.AppendLine("        ,A.STAFFNAMEL  ")
        SQLStr.AppendLine("        ,A.EMAIL  ")
        SQLStr.AppendLine("        ,A.MENUROLE  ")
        SQLStr.AppendLine("        ,A.MAPROLE  ")
        SQLStr.AppendLine("        ,A.VIEWPROFID  ")
        SQLStr.AppendLine("        ,A.RPRTPROFID  ")
        SQLStr.AppendLine("        ,A.MAPID  ")
        SQLStr.AppendLine("        ,A.VARIANT  ")
        SQLStr.AppendLine("        ,A.DELFLG  ")
        SQLStr.AppendLine(" FROM COM.LNS0001_USER A ,COM.LNS0002_USERPASS B ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001_USER SELECT"
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

        Dim LNS0001Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNS0001Exceltblrow = LNS0001Exceltbl.NewRow

            'LINECNT
            LNS0001Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            'ユーザーID
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.USERID))
            WW_DATATYPE = DataTypeHT("USERID")
            LNS0001Exceltblrow("USERID") = LNS0001WRKINC.DataConvert("ユーザーID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'パスワード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.PASSWORD))
            WW_DATATYPE = DataTypeHT("PASSWORD")
            LNS0001Exceltblrow("PASSWORD") = LNS0001WRKINC.DataConvert("パスワード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '誤り回数
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.MISSCNT))
            WW_DATATYPE = DataTypeHT("MISSCNT")
            LNS0001Exceltblrow("MISSCNT") = LNS0001WRKINC.DataConvert("誤り回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'パスワード有効期限
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.PASSENDYMD))
            WW_DATATYPE = DataTypeHT("PASSENDYMD")
            LNS0001Exceltblrow("PASSENDYMD") = LNS0001WRKINC.DataConvert("パスワード有効期限", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '開始年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.STYMD))
            WW_DATATYPE = DataTypeHT("STYMD")
            LNS0001Exceltblrow("STYMD") = LNS0001WRKINC.DataConvert("開始年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '終了年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.ENDYMD))
            WW_DATATYPE = DataTypeHT("ENDYMD")
            LNS0001Exceltblrow("ENDYMD") = LNS0001WRKINC.DataConvert("終了年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.ORG))
            WW_DATATYPE = DataTypeHT("ORG")
            LNS0001Exceltblrow("ORG") = LNS0001WRKINC.DataConvert("組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '社員名（短）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMES))
            WW_DATATYPE = DataTypeHT("STAFFNAMES")
            LNS0001Exceltblrow("STAFFNAMES") = LNS0001WRKINC.DataConvert("社員名（短）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '社員名（長）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.STAFFNAMEL))
            WW_DATATYPE = DataTypeHT("STAFFNAMEL")
            LNS0001Exceltblrow("STAFFNAMEL") = LNS0001WRKINC.DataConvert("社員名（長）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'メールアドレス
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.EMAIL))
            WW_DATATYPE = DataTypeHT("EMAIL")
            LNS0001Exceltblrow("EMAIL") = LNS0001WRKINC.DataConvert("メールアドレス", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '画面ＩＤ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.MAPID))
            WW_DATATYPE = DataTypeHT("MAPID")
            LNS0001Exceltblrow("MAPID") = LNS0001WRKINC.DataConvert("画面ＩＤ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            ''メニュー表示制御ロール
            'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.MENUROLE))
            'WW_DATATYPE = DataTypeHT("MENUROLE")
            'LNS0001Exceltblrow("MENUROLE") = LNS0001WRKINC.DataConvert("メニュー表示制御ロール", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If
            ''画面参照更新制御ロール
            'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.MAPROLE))
            'WW_DATATYPE = DataTypeHT("MAPROLE")
            'LNS0001Exceltblrow("MAPROLE") = LNS0001WRKINC.DataConvert("画面参照更新制御ロール", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If
            ''画面表示項目制御ロール
            'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.VIEWPROFID))
            'WW_DATATYPE = DataTypeHT("VIEWPROFID")
            'LNS0001Exceltblrow("VIEWPROFID") = LNS0001WRKINC.DataConvert("画面表示項目制御ロール", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If
            ''エクセル出力制御ロール
            'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.RPRTPROFID))
            'WW_DATATYPE = DataTypeHT("RPRTPROFID")
            'LNS0001Exceltblrow("RPRTPROFID") = LNS0001WRKINC.DataConvert("エクセル出力制御ロール", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If

            ''画面初期値ロール
            'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL._VARIANT))
            'WW_DATATYPE = DataTypeHT("VARIANT")
            'LNS0001Exceltblrow("VARIANT") = LNS0001WRKINC.DataConvert("画面初期値ロール", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If

            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNS0001WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNS0001Exceltblrow("DELFLG") = LNS0001WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            'パスワード変更確認
            If LNS0001Exceltblrow("PASSWORD").ToString.Replace("*", "") = "" Then
                LNS0001Exceltblrow("PASSUPD") = "0" '変更なし
            Else
                LNS0001Exceltblrow("PASSUPD") = "1" '変更あり
            End If

            '登録
            LNS0001Exceltbl.Rows.Add(LNS0001Exceltblrow)

        Next
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        ''パスワード変更がある場合
        'If WW_ROW("PASSUPD") = "1" Then
        '    SameDataChk = True
        '    Exit Function
        'End If

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        USERID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(USERID, '')             = @USERID ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(ORG, '')             = @ORG ")
        SQLStr.AppendLine("    AND  COALESCE(STAFFNAMES, '')             = @STAFFNAMES ")
        SQLStr.AppendLine("    AND  COALESCE(STAFFNAMEL, '')             = @STAFFNAMEL ")
        SQLStr.AppendLine("    AND  COALESCE(EMAIL, '')             = @EMAIL ")
        'SQLStr.AppendLine("    AND  COALESCE(MENUROLE, '')             = @MENUROLE ")
        'SQLStr.AppendLine("    AND  COALESCE(MAPROLE, '')             = @MAPROLE ")
        'SQLStr.AppendLine("    AND  COALESCE(VIEWPROFID, '')             = @VIEWPROFID ")
        'SQLStr.AppendLine("    AND  COALESCE(RPRTPROFID, '')             = @RPRTPROFID ")
        SQLStr.AppendLine("    AND  COALESCE(MAPID, '')             = @MAPID ")
        'SQLStr.AppendLine("    AND  COALESCE(VARIANT, '')             = @VARIANT ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_MISSCNT As MySqlParameter = SQLcmd.Parameters.Add("@MISSCNT", MySqlDbType.Int32)     '誤り回数
                Dim P_PASSENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@PASSENDYMD", MySqlDbType.Date)     'パスワード有効期限
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日
                Dim P_ORG As MySqlParameter = SQLcmd.Parameters.Add("@ORG", MySqlDbType.VarChar, 6)     '組織コード
                Dim P_STAFFNAMES As MySqlParameter = SQLcmd.Parameters.Add("@STAFFNAMES", MySqlDbType.VarChar, 20)     '社員名（短）
                Dim P_STAFFNAMEL As MySqlParameter = SQLcmd.Parameters.Add("@STAFFNAMEL", MySqlDbType.VarChar, 50)     '社員名（長）
                Dim P_EMAIL As MySqlParameter = SQLcmd.Parameters.Add("@EMAIL", MySqlDbType.VarChar, 128)     'メールアドレス
                'Dim P_MENUROLE As MySqlParameter = SQLcmd.Parameters.Add("@MENUROLE", MySqlDbType.VarChar, 20)     'メニュー表示制御ロール
                'Dim P_MAPROLE As MySqlParameter = SQLcmd.Parameters.Add("@MAPROLE", MySqlDbType.VarChar, 20)     '画面参照更新制御ロール
                'Dim P_VIEWPROFID As MySqlParameter = SQLcmd.Parameters.Add("@VIEWPROFID", MySqlDbType.VarChar, 20)     '画面表示項目制御ロール
                'Dim P_RPRTPROFID As MySqlParameter = SQLcmd.Parameters.Add("@RPRTPROFID", MySqlDbType.VarChar, 20)     'エクセル出力制御ロール
                Dim P_MAPID As MySqlParameter = SQLcmd.Parameters.Add("@MAPID", MySqlDbType.VarChar, 20)     '画面ＩＤ
                'Dim P_VARIANT As MySqlParameter = SQLcmd.Parameters.Add("@VARIANT", MySqlDbType.VarChar, 20)     '画面初期値ロール

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_MISSCNT.Value = WW_ROW("MISSCNT")           '誤り回数
                P_PASSENDYMD.Value = WW_ROW("PASSENDYMD")           'パスワード有効期限
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日
                P_ORG.Value = WW_ROW("ORG")           '組織コード
                P_STAFFNAMES.Value = WW_ROW("STAFFNAMES")           '社員名（短）
                P_STAFFNAMEL.Value = WW_ROW("STAFFNAMEL")           '社員名（長）
                P_EMAIL.Value = WW_ROW("EMAIL")           'メールアドレス
                'P_MENUROLE.Value = WW_ROW("MENUROLE")           'メニュー表示制御ロール
                'P_MAPROLE.Value = WW_ROW("MAPROLE")           '画面参照更新制御ロール
                'P_VIEWPROFID.Value = WW_ROW("VIEWPROFID")           '画面表示項目制御ロール
                'P_RPRTPROFID.Value = WW_ROW("RPRTPROFID")           'エクセル出力制御ロール
                P_MAPID.Value = WW_ROW("MAPID")           '画面ＩＤ
                'P_VARIANT.Value = WW_ROW("VARIANT")           '画面初期値ロール

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001_USER SELECT"
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
        If WW_ROW("USERID") = "" OrElse
            WW_ROW("STYMD") = Date.MinValue OrElse
            WW_ROW("ENDYMD") = Date.MinValue Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(USERID, '')             = @USERID ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日

                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001_USER Select"
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
        SQLStr.Append("     COM.LNS0001_USER                        ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(USERID, '')  = @USERID ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日
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
            CS0011LOGWrite.INFPOSI = "DB:LNS0001L UPDATE"
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

        'ユーザマスタ
        Dim SQLStrUSER = New StringBuilder
        SQLStrUSER.AppendLine("  INSERT INTO COM.LNS0001_USER")
        SQLStrUSER.AppendLine("   (  ")
        SQLStrUSER.AppendLine("      USERID  ")
        SQLStrUSER.AppendLine("     ,STYMD  ")
        SQLStrUSER.AppendLine("     ,ENDYMD  ")
        SQLStrUSER.AppendLine("     ,CAMPCODE  ")
        SQLStrUSER.AppendLine("     ,ORG  ")
        SQLStrUSER.AppendLine("     ,STAFFNAMES  ")
        SQLStrUSER.AppendLine("     ,STAFFNAMEL  ")
        SQLStrUSER.AppendLine("     ,EMAIL  ")
        SQLStrUSER.AppendLine("     ,MENUROLE  ")
        SQLStrUSER.AppendLine("     ,MAPROLE  ")
        SQLStrUSER.AppendLine("     ,VIEWPROFID  ")
        SQLStrUSER.AppendLine("     ,RPRTPROFID  ")
        SQLStrUSER.AppendLine("     ,MAPID  ")
        SQLStrUSER.AppendLine("     ,VARIANT  ")
        SQLStrUSER.AppendLine("     ,DELFLG  ")
        SQLStrUSER.AppendLine("     ,INITYMD  ")
        SQLStrUSER.AppendLine("     ,INITUSER  ")
        SQLStrUSER.AppendLine("     ,INITTERMID  ")
        SQLStrUSER.AppendLine("     ,INITPGID  ")
        SQLStrUSER.AppendLine("   )  ")
        SQLStrUSER.AppendLine("   VALUES  ")
        SQLStrUSER.AppendLine("   (  ")
        SQLStrUSER.AppendLine("      @USERID  ")
        SQLStrUSER.AppendLine("     ,@STYMD  ")
        SQLStrUSER.AppendLine("     ,@ENDYMD  ")
        SQLStrUSER.AppendLine("     ,@CAMPCODE  ")
        SQLStrUSER.AppendLine("     ,@ORG  ")
        SQLStrUSER.AppendLine("     ,@STAFFNAMES  ")
        SQLStrUSER.AppendLine("     ,@STAFFNAMEL  ")
        SQLStrUSER.AppendLine("     ,@EMAIL  ")
        SQLStrUSER.AppendLine("     ,@MENUROLE  ")
        SQLStrUSER.AppendLine("     ,@MAPROLE  ")
        SQLStrUSER.AppendLine("     ,@VIEWPROFID  ")
        SQLStrUSER.AppendLine("     ,@RPRTPROFID  ")
        SQLStrUSER.AppendLine("     ,@MAPID  ")
        SQLStrUSER.AppendLine("     ,@VARIANT  ")
        SQLStrUSER.AppendLine("     ,@DELFLG  ")
        SQLStrUSER.AppendLine("     ,@INITYMD  ")
        SQLStrUSER.AppendLine("     ,@INITUSER  ")
        SQLStrUSER.AppendLine("     ,@INITTERMID  ")
        SQLStrUSER.AppendLine("     ,@INITPGID  ")
        SQLStrUSER.AppendLine("   )   ")
        SQLStrUSER.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStrUSER.AppendLine("      USERID =  @USERID ")
        SQLStrUSER.AppendLine("     ,STYMD =  @STYMD ")
        SQLStrUSER.AppendLine("     ,ENDYMD =  @ENDYMD ")
        SQLStrUSER.AppendLine("     ,CAMPCODE =  @CAMPCODE ")
        SQLStrUSER.AppendLine("     ,ORG =  @ORG ")
        SQLStrUSER.AppendLine("     ,STAFFNAMES =  @STAFFNAMES ")
        SQLStrUSER.AppendLine("     ,STAFFNAMEL =  @STAFFNAMEL ")
        SQLStrUSER.AppendLine("     ,EMAIL =  @EMAIL ")
        SQLStrUSER.AppendLine("     ,MAPID =  @MAPID ")
        SQLStrUSER.AppendLine("     ,DELFLG =  @DELFLG ")
        SQLStrUSER.AppendLine("     ,UPDYMD =  @UPDYMD ")
        SQLStrUSER.AppendLine("     ,UPDUSER =  @UPDUSER ")
        SQLStrUSER.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
        SQLStrUSER.AppendLine("     ,UPDPGID =  @UPDPGID ")
        SQLStrUSER.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrUSER.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 2)     '会社コード
                Dim P_ORG As MySqlParameter = SQLcmd.Parameters.Add("@ORG", MySqlDbType.VarChar, 6)     '組織コード
                Dim P_STAFFNAMES As MySqlParameter = SQLcmd.Parameters.Add("@STAFFNAMES", MySqlDbType.VarChar, 20)     '社員名（短）
                Dim P_STAFFNAMEL As MySqlParameter = SQLcmd.Parameters.Add("@STAFFNAMEL", MySqlDbType.VarChar, 50)     '社員名（長）
                Dim P_EMAIL As MySqlParameter = SQLcmd.Parameters.Add("@EMAIL", MySqlDbType.VarChar, 128)     'メールアドレス
                Dim P_MENUROLE As MySqlParameter = SQLcmd.Parameters.Add("@MENUROLE", MySqlDbType.VarChar, 20)     'メニュー表示制御ロール
                Dim P_MAPROLE As MySqlParameter = SQLcmd.Parameters.Add("@MAPROLE", MySqlDbType.VarChar, 20)     '画面参照更新制御ロール
                Dim P_VIEWPROFID As MySqlParameter = SQLcmd.Parameters.Add("@VIEWPROFID", MySqlDbType.VarChar, 20)     '画面表示項目制御ロール
                Dim P_RPRTPROFID As MySqlParameter = SQLcmd.Parameters.Add("@RPRTPROFID", MySqlDbType.VarChar, 20)     'エクセル出力制御ロール
                Dim P_MAPID As MySqlParameter = SQLcmd.Parameters.Add("@MAPID", MySqlDbType.VarChar, 20)     '画面ＩＤ
                Dim P_VARIANT As MySqlParameter = SQLcmd.Parameters.Add("@VARIANT", MySqlDbType.VarChar, 20)     '画面初期値ロール
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
                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日
                P_CAMPCODE.Value = Master.USERCAMP           '会社コード
                P_ORG.Value = WW_ROW("ORG")           '組織コード
                P_STAFFNAMES.Value = WW_ROW("STAFFNAMES")           '社員名（短）
                P_STAFFNAMEL.Value = WW_ROW("STAFFNAMEL")           '社員名（長）
                P_EMAIL.Value = WW_ROW("EMAIL")           'メールアドレス
                P_MAPID.Value = WW_ROW("MAPID")           '画面ＩＤ

                'P_MENUROLE.Value = WW_ROW("MENUROLE")           'メニュー表示制御ロール
                'P_MAPROLE.Value = WW_ROW("MAPROLE")           '画面参照更新制御ロール
                'P_VIEWPROFID.Value = WW_ROW("VIEWPROFID")           '画面表示項目制御ロール
                'P_RPRTPROFID.Value = WW_ROW("RPRTPROFID")           'エクセル出力制御ロール
                'P_VARIANT.Value = WW_ROW("VARIANT")           '画面初期値ロール

                Master.GetFirstValue(Master.USERCAMP, "MENUROLE", P_MENUROLE.Value) 'メニュー表示制御ロール
                Master.GetFirstValue(Master.USERCAMP, "MAPROLE", P_MAPROLE.Value) '画面参照更新制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VIEWPROFID", P_VIEWPROFID.Value) '画面表示項目制御ロール
                Master.GetFirstValue(Master.USERCAMP, "RPRTPROFID", P_RPRTPROFID.Value) 'エクセル出力制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VARIANT", P_VARIANT.Value) '画面初期値ロール

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNS0001_USER  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        'ユーザーパスワードマスタ
        Dim SQLStrUPA = New StringBuilder
        SQLStrUPA.AppendLine("  INSERT INTO COM.LNS0002_USERPASS")
        SQLStrUPA.AppendLine("   (  ")
        SQLStrUPA.AppendLine("      USERID  ")

        'パスワード変更あり※(UPDATE文と項目あわせないとエラーでコケる)
        If WW_ROW("PASSUPD") = "1" Then
            SQLStrUPA.AppendLine("     ,PASSWORD  ")
        End If

        SQLStrUPA.AppendLine("     ,MISSCNT  ")
        SQLStrUPA.AppendLine("     ,PASSENDYMD  ")
        SQLStrUPA.AppendLine("     ,DELFLG  ")
        SQLStrUPA.AppendLine("     ,INITYMD  ")
        SQLStrUPA.AppendLine("     ,INITUSER  ")
        SQLStrUPA.AppendLine("     ,INITTERMID  ")
        SQLStrUPA.AppendLine("   )  ")
        SQLStrUPA.AppendLine("   VALUES  ")
        SQLStrUPA.AppendLine("   (  ")
        SQLStrUPA.AppendLine("      @USERID  ")

        'パスワード変更あり※(UPDATE文と項目あわせないとエラーでコケる)
        If WW_ROW("PASSUPD") = "1" Then
            SQLStrUPA.AppendLine("     ,AES_ENCRYPT(@PASSWORD, 'loginpasskey')  ")
        End If

        SQLStrUPA.AppendLine("     ,@MISSCNT  ")
        SQLStrUPA.AppendLine("     ,@PASSENDYMD  ")
        SQLStrUPA.AppendLine("     ,@DELFLG  ")
        SQLStrUPA.AppendLine("     ,@INITYMD  ")
        SQLStrUPA.AppendLine("     ,@INITUSER  ")
        SQLStrUPA.AppendLine("     ,@INITTERMID  ")
        SQLStrUPA.AppendLine("   )   ")
        SQLStrUPA.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStrUPA.AppendLine("      USERID =  @USERID ")

        'パスワード変更あり
        If WW_ROW("PASSUPD") = "1" Then
            SQLStrUPA.AppendLine("     ,PASSWORD =  AES_ENCRYPT(@PASSWORD, 'loginpasskey') ")
        End If

        SQLStrUPA.AppendLine("     ,MISSCNT =  @MISSCNT")
        SQLStrUPA.AppendLine("     ,PASSENDYMD =  @PASSENDYMD ")
        SQLStrUPA.AppendLine("     ,DELFLG =  @DELFLG ")
        SQLStrUPA.AppendLine("     ,UPDYMD =  @UPDYMD ")
        SQLStrUPA.AppendLine("     ,UPDUSER =  @UPDUSER ")
        SQLStrUPA.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
        SQLStrUPA.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStrUPA.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_MISSCNT As MySqlParameter = SQLcmd.Parameters.Add("@MISSCNT", MySqlDbType.Int32)     '誤り回数
                Dim P_PASSENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@PASSENDYMD", MySqlDbType.Date)     'パスワード有効期限

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末

                'DB更新
                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_MISSCNT.Value = WW_ROW("MISSCNT")           '誤り回数
                P_PASSENDYMD.Value = WW_ROW("PASSENDYMD")           'パスワード有効期限

                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                P_INITYMD.Value = WW_DATENOW                '登録年月日
                P_INITUSER.Value = Master.USERID               '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID               '登録端末
                P_UPDYMD.Value = WW_DATENOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末

                'パスワード変更あり
                If WW_ROW("PASSUPD") = "1" Then
                    Dim P_PASSWORD As MySqlParameter = SQLcmd.Parameters.Add("@PASSWORD", MySqlDbType.VarChar, 200)     'パスワード
                    P_PASSWORD.Value = WW_ROW("PASSWORD")           'パスワード
                End If

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNS0001_USER  INSERTUPDATE"
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

        ' ユーザーID(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "USERID", WW_ROW("USERID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・ユーザーID入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 社員名（短）(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "STAFFNAMES", WW_ROW("STAFFNAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・社員名（短）入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 社員名（長）(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "STAFFNAMEL", WW_ROW("STAFFNAMEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・社員名（長）入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 誤り回数(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "MISSCNT", WW_ROW("MISSCNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・誤り回数入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        'パスワード変更ありの場合のみチェックする
        If WW_ROW("PASSUPD") = "1" Then
            If Not WW_ROW("DELFLG").ToString = C_DELETE_FLG.DELETE AndAlso
                Not LNS0001WRKINC.ChkUserPassword(WW_ROW("PASSWORD"), WW_CheckMES2) Then
                WW_CheckMES1 = "・パスワード入力エラーです。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        If WW_ROW("PASSUPD") = "1" Then
            ' パスワード有効期限
            NowDate = NowDate.AddDays(ADDDATE)
            WW_ROW("PASSENDYMD") = CDate(NowDate).ToShortDateString
        End If
        ' 開始年月日(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "STYMD", WW_ROW("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            WW_ROW("STYMD") = CDate(WW_ROW("STYMD")).ToString("yyyy/MM/dd")
        Else
            WW_CheckMES1 = "・開始年月日エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 終了年月日(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ENDYMD", WW_ROW("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Date.Now > WW_ROW("ENDYMD") Then
                WW_CheckMES1 = "・終了年月日エラーです。"
                WW_CheckMES2 = "過去日入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_ROW("ENDYMD") = CDate(WW_ROW("ENDYMD")).ToString("yyyy/MM/dd")
            End If
        Else
            WW_CheckMES1 = "・終了年月日エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '' 会社コード(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "CAMPCODE", WW_ROW("CAMPCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If isNormal(WW_CS0024FCheckerr) Then
        '    ' 名称存在チェック
        '    CODENAME_get("CAMPCODE", WW_ROW("CAMPCODE"), WW_Dummy, WW_RtnSW)
        '    If Not isNormal(WW_RtnSW) Then
        '        WW_CheckMES1 = "・会社コード入力エラーです。"
        '        WW_CheckMES2 = "マスタに存在しません。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If
        'Else
        '    WW_CheckMES1 = "・会社コード入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ' 組織コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ORG", WW_ROW("ORG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            '情シス、高圧ガス以外
            If LNS0001WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                Dim WW_OrgPermitHt As New Hashtable
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    '操作権限のある組織コード一覧を取得
                    work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                    '操作権限のある組織コード一覧に含まれていない場合
                    If WW_OrgPermitHt.ContainsKey(WW_ROW("ORG")) = False And WW_ROW("ORG") <> Master.ROLE_ORG Then
                        WW_CheckMES1 = "・組織コード入力エラーです。"
                        WW_CheckMES2 = "対象の組織コードは登録権限がありません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End Using
            End If
        Else
            WW_CheckMES1 = "・組織コード入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' メールアドレス(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "EMAIL", WW_ROW("EMAIL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・メールアドレス入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '' メニュー表示制御ロール(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "MENUROLE", WW_ROW("MENUROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If isNormal(WW_CS0024FCheckerr) Then
        '    ' 名称存在チェック
        '    CODENAME_get("MENU", WW_ROW("MENUROLE"), WW_Dummy, WW_RtnSW)
        '    If Not isNormal(WW_RtnSW) Then
        '        WW_CheckMES1 = "・メニュー表示制御ロール入力エラーです。"
        '        WW_CheckMES2 = "マスタに存在しません。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If
        'Else
        '    WW_CheckMES1 = "・メニュー表示制御ロール入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        '' 画面参照更新制御ロール(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "MAPROLE", WW_ROW("MAPROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If isNormal(WW_CS0024FCheckerr) Then
        '    ' 名称存在チェック
        '    CODENAME_get("MAP", WW_ROW("MAPROLE"), WW_Dummy, WW_RtnSW)
        '    If Not isNormal(WW_RtnSW) Then
        '        WW_CheckMES1 = "・画面参照更新制御ロール入力エラーです。"
        '        WW_CheckMES2 = "マスタに存在しません。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If
        'Else
        '    WW_CheckMES1 = "・画面参照更新制御ロール入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ''画面表示項目制御ロール(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "VIEWPROFID", WW_ROW("VIEWPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If isNormal(WW_CS0024FCheckerr) Then
        '    '名称存在チェック
        '    CODENAME_get("VIEW", WW_ROW("VIEWPROFID"), WW_Dummy, WW_RtnSW)
        '    If Not isNormal(WW_RtnSW) Then
        '        WW_CheckMES1 = "・画面表示項目制御ロール入力エラーです。"
        '        WW_CheckMES2 = "マスタに存在しません。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If
        'Else
        '    WW_CheckMES1 = "・画面表示項目制御ロール入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ''エクセル出力制御ロール(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "RPRTPROFID", WW_ROW("RPRTPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If isNormal(WW_CS0024FCheckerr) Then
        '    '名称存在チェック
        '    CODENAME_get("XML", WW_ROW("RPRTPROFID"), WW_Dummy, WW_RtnSW)
        '    If Not isNormal(WW_RtnSW) Then
        '        WW_CheckMES1 = "・エクセル出力制御ロール入力エラーです。"
        '        WW_CheckMES2 = "マスタに存在しません。"
        '        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '        WW_LineErr = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If
        'Else
        '    WW_CheckMES1 = "・エクセル出力制御ロール入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ''画面初期値ロール(バリデーションチェック）
        'Master.CheckField(Master.USERCAMP, "VARIANT", WW_ROW("VARIANT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・画面初期値ロール入力エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If

        ' 日付大小チェック
        If Not String.IsNullOrEmpty(WW_ROW("STYMD")) AndAlso
                Not String.IsNullOrEmpty(WW_ROW("ENDYMD")) Then
            If CDate(WW_ROW("STYMD")) > CDate(WW_ROW("ENDYMD")) Then
                WW_CheckMES1 = "・開始年月日＆終了年月日エラーです。"
                WW_CheckMES2 = "日付大小入力エラー"
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
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        'ユーザマスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        USERID")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(USERID, '')             = @USERID ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日

                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日

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
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001_USER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001_USER Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0002_USERHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      USERID  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,CAMPCODE  ")
        SQLStr.AppendLine("     ,ORG  ")
        SQLStr.AppendLine("     ,STAFFNAMES  ")
        SQLStr.AppendLine("     ,STAFFNAMEL  ")
        SQLStr.AppendLine("     ,EMAIL  ")
        SQLStr.AppendLine("     ,MENUROLE  ")
        SQLStr.AppendLine("     ,MAPROLE  ")
        SQLStr.AppendLine("     ,VIEWPROFID  ")
        SQLStr.AppendLine("     ,RPRTPROFID  ")
        SQLStr.AppendLine("     ,MAPID  ")
        SQLStr.AppendLine("     ,VARIANT  ")
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
        SQLStr.AppendLine("      USERID  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,CAMPCODE  ")
        SQLStr.AppendLine("     ,ORG  ")
        SQLStr.AppendLine("     ,STAFFNAMES  ")
        SQLStr.AppendLine("     ,STAFFNAMEL  ")
        SQLStr.AppendLine("     ,EMAIL  ")
        SQLStr.AppendLine("     ,MENUROLE  ")
        SQLStr.AppendLine("     ,MAPROLE  ")
        SQLStr.AppendLine("     ,VIEWPROFID  ")
        SQLStr.AppendLine("     ,RPRTPROFID  ")
        SQLStr.AppendLine("     ,MAPID  ")
        SQLStr.AppendLine("     ,VARIANT  ")
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
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(USERID, '') = @USERID ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_USERID.Value = WW_ROW("USERID")           'ユーザーID
                P_STYMD.Value = WW_ROW("STYMD")           '開始年月日
                P_ENDYMD.Value = WW_ROW("ENDYMD")           '終了年月日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002_USERHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0002_USERHIST  INSERT"
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
                Case "CAMPCODE"         '会社コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE1, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE1, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, Master.USERCAMP))
                    End If
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP))
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP))
                    End If
                Case "MENU"             'メニュー表示制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE1, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "MAP"              '画面参照更新制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE1, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "VIEW"             '画面表示項目制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE1, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "XML"              'エクセル出力制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE1, O_TEXT, O_RTN, work.CreateRoleList(Master.USERCAMP, I_FIELD))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class


