''************************************************************
' ユーザーマスタメンテナンス・一覧画面
' 作成日 2021/12/24
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2021/12/24 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' ユーザマスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNS0002UserList
    Inherits Page

    '○ 検索結果格納Table
    Private LNS0002tbl As DataTable         '一覧格納用テーブル
    'Private LNS0002tblSP As DataTable       'スプレッドシート用テーブル
    Private LNS0002UPDtbl As DataTable      '更新用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 20                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー
    Private Const ADDDATE As Integer = 90                           '有効期限追加日数

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
                    Master.RecoverTable(LNS0002tbl)

                    'アップロードボタン押下時
                    If WF_ButtonClick.Value = "WF_ButtonUPLOAD" Then
                        WF_ButtonUPLOAD_Click()
                        '○ 初期化処理
                        Initialize()
                    Else
                        Select Case WF_ButtonClick.Value
                            Case "WF_ButtonINSERT"          '追加ボタン押下
                                WF_ButtonINSERT_Click()
                            Case "WF_ButtonEND"             '戻るボタン押下
                                WF_ButtonEND_Click()
                            Case "WF_GridDBclick"           'GridViewダブルクリック
                                WF_Grid_DBClick()
                            Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                                WF_ButtonFIRST_Click()
                            Case "WF_ButtonLAST"            '最終頁ボタン押下
                                WF_ButtonLAST_Click()
                        End Select

                        '○ 一覧再表示処理
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
            If Not IsNothing(LNS0002tbl) Then
                LNS0002tbl.Clear()
                LNS0002tbl.Dispose()
                LNS0002tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNS0002WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0002S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0002D Then
            Master.RecoverTable(LNS0002tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNS0002tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNS0002tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNS0002tbl)

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

        If IsNothing(LNS0002tbl) Then
            LNS0002tbl = New DataTable
        End If

        If LNS0002tbl.Columns.Count <> 0 Then
            LNS0002tbl.Columns.Clear()
        End If

        LNS0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザマスタ、ユーザーパスワードマスタから取得する
        Dim SQLStr As String =
              " Select                                                                                              " _
            & "     1                                                                        AS 'SELECT'            " _
            & "   , 0                                                                        AS HIDDEN              " _
            & "   , 0                                                                        AS LINECNT             " _
            & "   , ''                                                                       AS OPERATION           " _
            & "   , LNS0002.UPDTIMSTP                                                        AS UPDTIMSTP           " _
            & "   , coalesce(RTRIM(LNS0002.DELFLG), '')                                      AS DELFLG              " _
            & "   , coalesce(RTRIM(LNS0002.USERID), '')                                      AS USERID              " _
            & "   , coalesce(RTRIM(LNS0002.STAFFNAMES), '')                                  AS STAFFNAMES          " _
            & "   , coalesce(RTRIM(LNS0002.STAFFNAMEL), '')                                  AS STAFFNAMEL          " _
            & "   , coalesce(RTRIM(LNS0002.MAPID), '')                                       AS MAPID               " _
            & "   , CAST(AES_DECRYPT(PASSWORD, 'loginpasskey') AS CHAR)                      AS PASSWORD            " _
            & "   , coalesce(RTRIM(LNS0003.MISSCNT), '')                                     AS MISSCNT             " _
            & "   , coalesce(DATE_FORMAT(LNS0003.PASSENDYMD, '%Y/%m/%d'), '')                AS PASSENDYMD          " _
            & "   , coalesce(DATE_FORMAT(LNS0002.STYMD, '%Y/%m/%d'), '')                     AS STYMD               " _
            & "   , coalesce(DATE_FORMAT(LNS0002.ENDYMD, '%Y/%m/%d'), '')                    AS ENDYMD              " _
            & "   , coalesce(RTRIM(LNS0002.CAMPCODE), '')                                    AS CAMPCODE            " _
            & "   , coalesce(RTRIM(LNS0002.ORG), '')                                         AS ORG                 " _
            & "   , coalesce(RTRIM(LNS0002.EMAIL), '')                                       AS EMAIL               " _
            & "   , coalesce(RTRIM(LNS0002.MENUROLE), '')                                    AS MENUROLE            " _
            & "   , coalesce(RTRIM(LNS0002.MAPROLE), '')                                     AS MAPROLE             " _
            & "   , coalesce(RTRIM(LNS0002.VIEWPROFID), '')                                  AS VIEWPROFID          " _
            & "   , coalesce(RTRIM(LNS0002.RPRTPROFID), '')                                  AS RPRTPROFID          " _
            & "   , coalesce(RTRIM(LNS0002.VARIANT), '')                                     AS VARIANT             " _
            & "   , coalesce(RTRIM(LNS0002.APPROVALID), '')                                  AS APPROVALID          " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN01), '')                         AS INITIALDISPLAYKBN01 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN02), '')                         AS INITIALDISPLAYKBN02 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN03), '')                         AS INITIALDISPLAYKBN03 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN04), '')                         AS INITIALDISPLAYKBN04 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN05), '')                         AS INITIALDISPLAYKBN05 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN06), '')                         AS INITIALDISPLAYKBN06 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN07), '')                         AS INITIALDISPLAYKBN07 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN08), '')                         AS INITIALDISPLAYKBN08 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN11), '')                         AS INITIALDISPLAYKBN11 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN12), '')                         AS INITIALDISPLAYKBN12 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN13), '')                         AS INITIALDISPLAYKBN13 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN14), '')                         AS INITIALDISPLAYKBN14 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN15), '')                         AS INITIALDISPLAYKBN15 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN16), '')                         AS INITIALDISPLAYKBN16 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN17), '')                         AS INITIALDISPLAYKBN17 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN18), '')                         AS INITIALDISPLAYKBN18 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN21), '')                         AS INITIALDISPLAYKBN21 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN22), '')                         AS INITIALDISPLAYKBN22 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN23), '')                         AS INITIALDISPLAYKBN23 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN24), '')                         AS INITIALDISPLAYKBN24 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN25), '')                         AS INITIALDISPLAYKBN25 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN26), '')                         AS INITIALDISPLAYKBN26 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN27), '')                         AS INITIALDISPLAYKBN27 " _
            & "   , coalesce(RTRIM(LNS0002.INITIALDISPLAYKBN28), '')                         AS INITIALDISPLAYKBN28 " _
            & " FROM                                                                                                " _
            & "     COM.LNS0002_USER LNS0002                                                                        " _
            & " INNER JOIN COM.LNS0003_USERPASS LNS0003                                                             " _
            & "     ON  LNS0003.USERID = LNS0002.USERID                                                             " _
            & "     AND LNS0003.DELFLG = LNS0002.DELFLG                                                             " _
            & " INNER JOIN COM.LNS0019_ORG LNS0019                                                                  " _
            & "     ON  LNS0019.ORGCODE = LNS0002.ORG                                                               " _
            & "     AND CURDATE() BETWEEN LNS0019.STYMD AND LNS0019.ENDYMD                                          " _
            & "     AND LNS0019.DELFLG = LNS0002.DELFLG                                                             "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 会社コード
        If Not String.IsNullOrEmpty(work.WF_SEL_CAMPCODE.Text) Then
            SQLWhereStr = " WHERE                      " _
                        & "     LNS0002.CAMPCODE = @P1 "
        End If
        ' 有効年月日(From)
        If Not String.IsNullOrEmpty(work.WF_SEL_STYMD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                         " _
                            & "    ((LNS0002.STYMD <= @P2 " _
                            & "          AND LNS0002.ENDYMD >=  @P2 "
            Else
                SQLWhereStr &= "    AND ((LNS0002.STYMD <= @P2 "
                SQLWhereStr &= "              AND LNS0002.ENDYMD >=  @P2) "
            End If
        End If
        ' 有効年月日(To)
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            '有効年月日(From)が必須項目のため不要
            'If String.IsNullOrEmpty(SQLWhereStr) Then
            '    SQLWhereStr = " WHERE                         " _
            '                & "     LNS0002.ENDYMD >= @P3     "
            'Else
            SQLWhereStr &= "        OR (LNS0002.STYMD <= @P3 "
            SQLWhereStr &= "            AND LNS0002.ENDYMD >= @P3) "
            SQLWhereStr &= "        OR (LNS0002.STYMD >= @P3 "
            SQLWhereStr &= "            AND LNS0002.ENDYMD <= @P3)) "
        Else
            SQLWhereStr &= "        OR (LNS0002.STYMD <= @P2 "
            SQLWhereStr &= "            AND LNS0002.ENDYMD >= @P2) "
            SQLWhereStr &= "        OR (LNS0002.STYMD >= @P2 "
            SQLWhereStr &= "            AND LNS0002.ENDYMD <= @P2)) "
        End If
        ' 組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                     " _
                            & "     LNS0002.ORG = @P4     "
            Else
                SQLWhereStr &= "    AND LNS0002.ORG = @P4 "
            End If
        ElseIf Master.USER_ORG <> CONST_OFFICECODE_SYSTEM AndAlso Master.USER_ORG <> CONST_OFFICECODE_011310 Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                     " _
                            & "     LNS0019.CONTROLCODE = @P4     "
            Else
                SQLWhereStr &= "    AND LNS0019.CONTROLCODE = @P4 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNS0002.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNS0002.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY           " _
            & "     LNS0002.ORG    " _
            & "   , LNS0002.USERID "

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
                        LNS0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNS0002row As DataRow In LNS0002tbl.Rows
                    i += 1
                    LNS0002row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0002L SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0002L Select"
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
        work.WF_SEL_MENUROLE.Text = ""                                           'メニュー表示制御ロール
        work.WF_SEL_MAPROLE.Text = ""                                            '画面参照更新制御ロール
        work.WF_SEL_VIEWPROFID.Text = ""                                         '画面表示項目制御ロール
        work.WF_SEL_RPRTPROFID.Text = ""                                         'エクセル出力制御ロール
        work.WF_SEL_VARIANT.Text = ""                                            '画面初期値ロール
        work.WF_SEL_APPROVALID.Text = ""                                         '承認権限ロール
        work.WF_SEL_TIMESTAMP.Text = ""         　                               'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNS0002tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNS0002tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNS0008row As DataRow In LNS0002tbl.Rows
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
        Dim TBLview As DataView = New DataView(LNS0002tbl)

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
        Dim TBLview As New DataView(LNS0002tbl)
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

    ''' <summary>
    ''' ｱｯﾌﾟﾛｰﾄﾞ(Excel読込)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPLOAD_Click()

        'Dim ret As Boolean

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        If (inpFileUpload.HasFile) Then

            '■アップロードFILE格納ディレクトリ取得
            Try
                '〇 アップロードFILE格納フォルダ確認＆作成(...\UPLOAD_TMP)
                Dim WW_Dir As String = ""
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP"
                If Not Directory.Exists(WW_Dir) Then
                    Directory.CreateDirectory(WW_Dir)
                End If

                ' 〇アップロードFILE格納フォルダ確認＆作成(...\UPLOAD_TMP\ユーザーID)
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID
                If Not Directory.Exists(WW_Dir) Then
                    Directory.CreateDirectory(WW_Dir)
                End If

                ' 〇アップロードFILE格納フォルダ内不要ファイル削除(すべて削除)
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID
                For Each tempFile As String In Directory.GetFiles(WW_Dir, "*.*")
                    ' ファイルパスからファイル名を取得
                    File.Delete(tempFile)
                Next

            Catch ex As Exception
                'エラーリターン(textStatus:errorとなる)
                Context.Response.StatusCode = 300
                Exit Sub
            End Try

            '■アップロードFILE格納
            Try
                'Dim filepath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID & "\"
                'Dim filename As String = filepath & inpFileUpload.PostedFile.FileName
                'inpFileUpload.SaveAs(filename)

                ''〇 スプレッドシート初期化
                'FpSpread1.Sheets(0).Reset()

                ''〇 Excelファイルからインポート
                'ret = FpSpread1.OpenExcel(filename, FarPoint.Excel.ExcelOpenFlags.RowAndColumnHeaders)

                'If ret = False Then
                '    Response.Write("エラー：ファイルを開けません。ファイルパス：" & filename)
                'Else
                '    '○ スプレッドシート初期設定
                '    InitSpread(FpSpread1)

                '    '○ スプレッドシート用のデータテーブル作成
                '    CreateSpreadTable2(FpSpread1.Sheets(0))

                '    '○ スプレッドシート作成
                '    SetSpreadStyle(FpSpread1.Sheets(0))

                'End If
            Catch ex As System.Exception
                Response.Write(ex.Message.ToString())
            End Try

            '○ 項目チェック
            INPTableCheck(WW_ErrSW)

            '○ テーブル反映
            If isNormal(WW_ErrSW) Then
                LNS0002tbl_UPD()
                '○ 後処理
                If isNormal(WW_ErrSW) Then
                    work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
                End If
            End If

        End If

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDOWNLOAD_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = LNS0002tbl                       'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 印刷(PDF出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = LNS0002tbl                       'データ参照Table
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

    ''' <summary>
    ''' スプレッドシート退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFileSP()
        work.WF_SEL_SPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "SPTBL.txt"

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
        work.WF_SEL_LINECNT.Text = LNS0002tbl.Rows(WW_LineCNT)("LINECNT")        '選択行
        work.WF_SEL_USERID.Text = LNS0002tbl.Rows(WW_LineCNT)("USERID")          'ユーザID
        work.WF_SEL_STAFFNAMES.Text = LNS0002tbl.Rows(WW_LineCNT)("STAFFNAMES")  '社員名（短）
        work.WF_SEL_STAFFNAMEL.Text = LNS0002tbl.Rows(WW_LineCNT)("STAFFNAMEL")  '社員名（長）
        work.WF_SEL_MAPID.Text = LNS0002tbl.Rows(WW_LineCNT)("MAPID")            '画面ＩＤ
        work.WF_SEL_PASSWORD.Text = LNS0002tbl.Rows(WW_LineCNT)("PASSWORD")      'パスワード
        work.WF_SEL_MISSCNT.Text = LNS0002tbl.Rows(WW_LineCNT)("MISSCNT")        '誤り回数
        work.WF_SEL_PASSENDYMD.Text = LNS0002tbl.Rows(WW_LineCNT)("PASSENDYMD")  'パスワード有効期限
        work.WF_SEL_STYMD2.Text = LNS0002tbl.Rows(WW_LineCNT)("STYMD")           '開始年月日
        work.WF_SEL_ENDYMD2.Text = LNS0002tbl.Rows(WW_LineCNT)("ENDYMD")         '終了年月日
        work.WF_SEL_ORG2.Text = LNS0002tbl.Rows(WW_LineCNT)("ORG")               '組織コード
        work.WF_SEL_EMAIL.Text = LNS0002tbl.Rows(WW_LineCNT)("EMAIL")            'メールアドレス
        work.WF_SEL_MENUROLE.Text = LNS0002tbl.Rows(WW_LineCNT)("MENUROLE")      'メニュー表示制御ロール
        work.WF_SEL_MAPROLE.Text = LNS0002tbl.Rows(WW_LineCNT)("MAPROLE")        '画面参照更新制御ロール
        work.WF_SEL_VIEWPROFID.Text = LNS0002tbl.Rows(WW_LineCNT)("VIEWPROFID")  '画面表示項目制御ロール
        work.WF_SEL_RPRTPROFID.Text = LNS0002tbl.Rows(WW_LineCNT)("RPRTPROFID")  'エクセル出力制御ロール
        work.WF_SEL_VARIANT.Text = LNS0002tbl.Rows(WW_LineCNT)("VARIANT")        '画面初期値ロール
        work.WF_SEL_APPROVALID.Text = LNS0002tbl.Rows(WW_LineCNT)("APPROVALID")  '承認権限ロール
        work.WF_SEL_DELFLG.Text = LNS0002tbl.Rows(WW_LineCNT)("DELFLG")          '削除フラグ
        work.WF_SEL_TIMESTAMP.Text = LNS0002tbl.Rows(WW_LineCNT)("UPDTIMSTP")    'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                              '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNS0002tbl, work.WF_SEL_INPTBL.Text)

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

    ' ******************************************************************************
    ' ***  アップロードチェック処理                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_StyDateFlag As String = ""
        Dim WW_NewPassEndDate As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim NowDate As DateTime = Date.Now

        '○ 画面操作権限チェック
        ' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・ユーザ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNS0002INProw As DataRow In LNS0002tbl.Rows

            WW_LineErr = ""

            ' 削除フラグ
            Master.CheckField(Master.USERCAMP, "DELFLG", LNS0002INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNS0002INProw("DELFLG"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・削除コード入力エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・削除コードエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' ユーザID
            Master.CheckField(Master.USERCAMP, "USERID", LNS0002INProw("USERID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・ユーザID入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 社員名（短）
            Master.CheckField(Master.USERCAMP, "STAFFNAMES", LNS0002INProw("STAFFNAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・社員名（短）入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 社員名（長）
            Master.CheckField(Master.USERCAMP, "STAFFNAMEL", LNS0002INProw("STAFFNAMEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・社員名（長）入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 誤り回数
            Master.CheckField(Master.USERCAMP, "MISSCNT", LNS0002INProw("MISSCNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・誤り回数入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' パスワード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PASSWORD", LNS0002INProw("PASSWORD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・パスワード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' パスワード有効期限
            If LNS0002INProw("PASSWORD") <> work.WF_SEL_PASSWORD.Text Then
                NowDate = NowDate.AddDays(ADDDATE)
                LNS0002INProw("PASSENDYMD") = CDate(NowDate).ToShortDateString
            End If

            ' 開始年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STYMD", LNS0002INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNS0002INProw("STYMD") = CDate(LNS0002INProw("STYMD")).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 終了年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ENDYMD", LNS0002INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Date.Now > LNS0002INProw("ENDYMD") And LNS0002INProw("ENDYMD") <> work.WF_SEL_ENDYMD.Text Then
                    WW_CheckMES1 = "・終了年月日エラー"
                    WW_CheckMES2 = "過去日入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    LNS0002INProw("ENDYMD") = CDate(LNS0002INProw("ENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・終了年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 会社コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CAMPCODE", LNS0002INProw("CAMPCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("CAMPCODE", LNS0002INProw("CAMPCODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・会社コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・会社コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORG", LNS0002INProw("ORG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNS0002INProw("ORG"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・組織コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・組織コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' メールアドレス(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "EMAIL", LNS0002INProw("EMAIL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・メールアドレス入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' メニュー表示制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MENUROLE", LNS0002INProw("MENUROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MENU", LNS0002INProw("MENUROLE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・メニュー表示制御ロール入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・メニュー表示制御ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 画面参照更新制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MAPROLE", LNS0002INProw("MAPROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MAP", LNS0002INProw("MAPROLE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・画面参照更新制御ロール入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・画面参照更新制御ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面表示項目制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "VIEWPROFID", LNS0002INProw("VIEWPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '名称存在チェック
                CODENAME_get("VIEW", LNS0002INProw("VIEWPROFID"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・画面表示項目制御ロール入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・画面表示項目制御ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エクセル出力制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "RPRTPROFID", LNS0002INProw("RPRTPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '名称存在チェック
                CODENAME_get("XML", LNS0002INProw("RPRTPROFID"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・エクセル出力制御ロール入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・エクセル出力制御ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面初期値ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "VARIANT", LNS0002INProw("VARIANT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・画面初期値ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '承認権限ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "APPROVALID", LNS0002INProw("APPROVALID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNS0002INProw("APPROVALID")) Then
                    '名称存在チェック
                    CODENAME_get("APPROVAL", LNS0002INProw("APPROVALID"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・承認権限ロール入力エラー"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・承認権限ロール入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If Not String.IsNullOrEmpty(LNS0002INProw("STYMD")) AndAlso Not String.IsNullOrEmpty(LNS0002INProw("ENDYMD")) Then
                If CDate(LNS0002INProw("STYMD")) > CDate(LNS0002INProw("ENDYMD")) Then
                    WW_CheckMES1 = "・開始年月日＆終了年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ErrMes As String = ""
        WW_ErrMes = MESSAGE1
        If Not String.IsNullOrEmpty(MESSAGE2) Then
            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

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
    Protected Sub LNS0002tbl_UPD()

        ' 追加/更新の場合、DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            For Each LNS0002INProw As DataRow In LNS0002tbl.Rows
                ' マスタ更新
                UpdateMaster(SQLcon, LNS0002INProw)
            Next

        End Using

    End Sub

    ''' <summary>
    ''' ユーザマスタ更新処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByRef LNS0002row As DataRow)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ユーザマスタ)
        Dim SQLStr As String =
              "     INSERT INTO COM.LNS0002_USER            " _
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
            & "       , APPROVALID                          " _
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
            & "       , @P18                                " _
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
            & "       , MENUROLE   = @P13                   " _
            & "       , MAPROLE    = @P14                   " _
            & "       , VIEWPROFID = @P15                   " _
            & "       , RPRTPROFID = @P16                   " _
            & "       , VARIANT    = @P17                   " _
            & "       , APPROVALID = @P18                   " _
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
            & "   , APPROVALID                             " _
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
            & "     COM.LNS0002_USER                       " _
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
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 20)        '承認権限ロール
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
                PARA00.Value = LNS0002row("DELFLG")                            '削除フラグ
                PARA01.Value = LNS0002row("USERID")                            'ユーザID
                PARA02.Value = LNS0002row("STAFFNAMES")                        '社員名（短）
                PARA03.Value = LNS0002row("STAFFNAMEL")                        '社員名（長）
                PARA04.Value = LNS0002row("MAPID")                             '画面ＩＤ

                If Not String.IsNullOrEmpty(RTrim(LNS0002row("STYMD"))) Then   '開始年月日
                    PARA08.Value = RTrim(LNS0002row("STYMD"))
                Else
                    PARA08.Value = C_DEFAULT_YMD
                End If

                If Not String.IsNullOrEmpty(RTrim(LNS0002row("ENDYMD"))) Then  '終了年月日
                    PARA09.Value = RTrim(LNS0002row("ENDYMD"))
                Else
                    PARA09.Value = C_DEFAULT_YMD
                End If

                PARA10.Value = LNS0002row("CAMPCODE")                          '会社コード
                PARA11.Value = LNS0002row("ORG")                               '組織コード
                PARA12.Value = LNS0002row("EMAIL")                             'メールアドレス
                PARA13.Value = LNS0002row("MENUROLE")                          'メニュー表示制御ロール
                PARA14.Value = LNS0002row("MAPROLE")                           '画面参照更新制御ロール
                PARA15.Value = LNS0002row("VIEWPROFID")                        '画面表示項目制御ロール
                PARA16.Value = LNS0002row("RPRTPROFID")                        'エクセル出力制御ロール
                PARA17.Value = LNS0002row("VARIANT")                           '画面初期値ロール
                PARA18.Value = LNS0002row("APPROVALID")                        '承認権限ロール
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
                JPARA01.Value = LNS0002row("USERID")                          'ユーザID
                If Not String.IsNullOrEmpty(RTrim(LNS0002row("STYMD"))) Then  '開始年月日
                    JPARA08.Value = RTrim(LNS0002row("STYMD"))
                Else
                    JPARA08.Value = C_DEFAULT_YMD
                End If

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0002UPDtbl) Then
                        LNS0002UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0002UPDtbl.Clear()
                    LNS0002UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0002UPDrow As DataRow In LNS0002UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0002D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0002D UPDATE_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0002D UPDATE_INSERT"
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
              "     INSERT INTO COM.LNS0003_USERPASS                                  " _
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
            & "     COM.LNS0003_USERPASS                   " _
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
                PARA00.Value = LNS0002row("DELFLG")                                  '削除フラグ
                PARA01.Value = LNS0002row("USERID")                                  'ユーザID
                Master.GetFirstValue(Master.USERCAMP, "FIRSTPASSWORD", PARA04.Value) '初期パスワード
                PARA05.Value = LNS0002row("PASSWORD")                                'パスワード
                If Not String.IsNullOrEmpty(LNS0002row("MISSCNT")) Then              '誤り回数
                    PARA06.Value = LNS0002row("MISSCNT")
                Else
                    PARA06.Value = "0"
                End If
                If Not String.IsNullOrEmpty(RTrim(LNS0002row("PASSENDYMD"))) Then  'パスワード有効期限
                    PARA07.Value = RTrim(LNS0002row("PASSENDYMD"))
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
                JPARA01.Value = LNS0002row("USERID")  'ユーザID

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0002UPDtbl) Then
                        LNS0002UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0002UPDtbl.Clear()
                    LNS0002UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0002UPDrow As DataRow In LNS0002UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0002D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0002UPDrow
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

End Class


