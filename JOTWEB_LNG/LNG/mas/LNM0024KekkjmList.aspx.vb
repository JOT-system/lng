''************************************************************
' 営業収入決済条件マスタメンテ一覧画面
' 作成日 2022/05/19
' 更新日 2023/10/26
' 作成者 瀬口
' 更新者 大浜
'
' 修正履歴 : 2022/05/19 新規作成
'          : 2023/10/26 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 営業収入決済条件マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0024KekkjmList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0024tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNM0024Exceltbl As New DataTable                        'Excelデータ格納用テーブル

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
                    Master.RecoverTable(LNM0024tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0024WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0024WRKINC.FILETYPE.PDF)
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
            If Not IsNothing(LNM0024tbl) Then
                LNM0024tbl.Clear()
                LNM0024tbl.Dispose()
                LNM0024tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0024WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0024S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0024D Then
            Master.RecoverTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNM0024tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0024tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0024tbl)

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

        If IsNothing(LNM0024tbl) Then
            LNM0024tbl = New DataTable
        End If

        If LNM0024tbl.Columns.Count <> 0 Then
            LNM0024tbl.Columns.Clear()
        End If

        LNM0024tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを営業収入決済条件マスタから取得する
        Dim SQLStr As String =
            " OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotctn; " _
            & " Select " _
            & "     1                                                        AS 'SELECT'               " _
            & "   , 0                                                        AS HIDDEN                 " _
            & "   , 0                                                        AS LINECNT                " _
            & "   , ''                                                       AS OPERATION              " _
            & "   , LNM0024.UPDTIMSTP                                        AS UPDTIMSTP              " _
            & "   , coalesce(RTRIM(LNM0024.DELFLG), '')                        AS DELFLG                 " _
            & "   , coalesce(RTRIM(LNM0024.TORICODE), '')                      AS TORICODE               " _
            & "   , coalesce(RTRIM(LNM0024.INVFILINGDEPT), '')                 AS INVFILINGDEPT          " _
            & "   , coalesce(RTRIM(LNM0024.INVKESAIKBN), '')                   AS INVKESAIKBN            " _
            & "   , coalesce(RTRIM(LNM0024.TORINAME), '')                      AS TORINAME               " _
            & "   , coalesce(RTRIM(LNM0024.TORINAMES), '')                     AS TORINAMES              " _
            & "   , coalesce(RTRIM(LNM0024.TORINAMEKANA), '')                  AS TORINAMEKANA           " _
            & "   , coalesce(RTRIM(LNM0024.TORIDIVNAME), '')                   AS TORIDIVNAME            " _
            & "   , coalesce(RTRIM(LNM0024.TORICHARGE), '')                    AS TORICHARGE             " _
            & "   , coalesce(RTRIM(LNM0024.TORIKBN), '')                       AS TORIKBN                " _
            & "   , coalesce(RTRIM(LNM0024.POSTNUM1), '')                      AS POSTNUM1               " _
            & "   , coalesce(RTRIM(LNM0024.POSTNUM2), '')                      AS POSTNUM2               " _
            & "   , coalesce(RTRIM(LNM0024.ADDR1), '')                         AS ADDR1                  " _
            & "   , coalesce(RTRIM(LNM0024.ADDR2), '')                         AS ADDR2                  " _
            & "   , coalesce(RTRIM(LNM0024.ADDR3), '')                         AS ADDR3                  " _
            & "   , coalesce(RTRIM(LNM0024.ADDR4), '')                         AS ADDR4                  " _
            & "   , coalesce(RTRIM(LNM0024.TEL), '')                           AS TEL                    " _
            & "   , coalesce(RTRIM(LNM0024.FAX), '')                           AS FAX                    " _
            & "   , coalesce(RTRIM(LNM0024.MAIL), '')                          AS MAIL                   " _
            & "   , coalesce(RTRIM(LNM0024.BANKCODE), '')                      AS BANKCODE               " _
            & "   , coalesce(RTRIM(LNM0024.BANKBRANCHCODE), '')                AS BANKBRANCHCODE         " _
            & "   , coalesce(RTRIM(LNM0024.ACCOUNTTYPE), '')                   AS ACCOUNTTYPE            " _
            & "   , coalesce(RTRIM(LNM0024.ACCOUNTNUMBER), '')                 AS ACCOUNTNUMBER          " _
            & "   , coalesce(RTRIM(LNM0024.ACCOUNTNAME), '')                   AS ACCOUNTNAME            " _
            & "   , coalesce(RTRIM(LNM0024.INACCOUNTCD), '')                   AS INACCOUNTCD            " _
            & "   , coalesce(RTRIM(LNM0024.TAXCALCULATION), '')                AS TAXCALCULATION         " _
            & "   , coalesce(RTRIM(LNM0024.ACCOUNTINGMONTH), '')               AS ACCOUNTINGMONTH        " _
            & "   , coalesce(RTRIM(LNM0024.DEPOSITDAY), '')                    AS DEPOSITDAY             " _
            & "   , coalesce(RTRIM(LNM0024.DEPOSITMONTHKBN), '')               AS DEPOSITMONTHKBN        " _
            & "   , coalesce(RTRIM(LNM0024.CLOSINGDAY), '')                    AS CLOSINGDAY             " _
            & "   , coalesce(RTRIM(LNM0024.SLIPDESCRIPTION1), '')              AS SLIPDESCRIPTION1       " _
            & "   , coalesce(RTRIM(LNM0024.SLIPDESCRIPTION2), '')              AS SLIPDESCRIPTION2       " _
            & "   , coalesce(RTRIM(LNM0024.NEXTMONTHUNSETTLEDKBN), '')         AS NEXTMONTHUNSETTLEDKBN  " _
            & "   , coalesce(RTRIM(LNM0024.BEFOREINVFILINGDEPT), '')           AS BEFOREINVFILINGDEPT  " _
            & "   , coalesce(LNM0024.UPDYMD, '')                               AS UPDYMD                 " _
            & " FROM                                                                                   " _
            & "     LNG.LNM0024_KEKKJM LNM0024                                                         "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する

        Dim SQLWhereStr As String = ""

        ' 取引先コード
        If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
            SQLWhereStr = " WHERE                        " _
                        & "     LNM0024.TORICODE      = @P1 "
        End If

        ' 請求書提出部店
        If Not String.IsNullOrEmpty(work.WF_SEL_INVFILINGDEPT.Text) Then
            SQLWhereStr = " WHERE                        " _
                        & "     LNM0024.INVFILINGDEPT = @P3 "
        End If

        ' 請求書決済区分
        If Not String.IsNullOrEmpty(work.WF_SEL_INVKESAIKBN.Text) Then
            SQLWhereStr = " WHERE                        " _
                        & "     LNM0024.INVKESAIKBN = @P4 "
        End If

        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                      " _
                            & "     LNM0024.DELFLG = 0     "
            Else
                SQLWhereStr &= "    AND LNM0024.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY" _
            & "    LNM0024.TORICODE" _
            & "  , LNM0024.INVFILINGDEPT" _
            & "  , LNM0024.INVKESAIKBN"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@P0", MySqlDbType.VarChar, 1)         '削除フラグ
                PARA0.Value = C_DELETE_FLG.DELETE

                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10)    '取引先コード
                    PARA1.Value = work.WF_SEL_TORICODE.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_INVFILINGDEPT.Text) Then
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 6)      '請求書提出部店
                    PARA3.Value = work.WF_SEL_INVFILINGDEPT.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_INVKESAIKBN.Text) Then
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.Decimal, 2)      '請求書決済区分
                    PARA4.Value = work.WF_SEL_INVKESAIKBN.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0024tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0024tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0024row As DataRow In LNM0024tbl.Rows
                    i += 1
                    LNM0024row("LINECNT") = i                                                'LINECNT
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024L Select"
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
        For Each LNM0024row As DataRow In LNM0024tbl.Rows
            If LNM0024row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0024row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0024tbl)

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

        work.WF_SEL_TORICODE2.Text = ""                                           '取引先コード
        work.WF_SEL_INVFILINGDEPT2.Text = ""                                      '請求書提出部店
        work.WF_SEL_INVKESAIKBN2.Text = "0"                                       '請求書決済区分
        work.WF_SEL_TORINAME.Text = ""                                            '取引先名称
        work.WF_SEL_TORINAMES.Text = ""                                           '取引先略称
        work.WF_SEL_TORINAMEKANA.Text = ""                                        '取引先カナ名称
        work.WF_SEL_TORIDIVNAME.Text = ""                                         '取引先部門名称
        work.WF_SEL_TORICHARGE.Text = ""                                          '取引先担当者
        work.WF_SEL_TORIKBN.Text = ""                                             '取引先区分
        work.WF_SEL_POSTNUM1.Text = ""                                            '郵便番号（上）
        work.WF_SEL_POSTNUM2.Text = ""                                            '郵便番号（下）
        work.WF_SEL_ADDR1.Text = ""                                               '住所１
        work.WF_SEL_ADDR2.Text = ""                                               '住所２
        work.WF_SEL_ADDR3.Text = ""                                               '住所３
        work.WF_SEL_ADDR4.Text = ""                                               '住所４
        work.WF_SEL_TEL.Text = ""                                                 '電話番号
        work.WF_SEL_FAX.Text = ""                                                 'ＦＡＸ番号
        work.WF_SEL_MAIL.Text = ""                                                'メールアドレス
        work.WF_SEL_BANKCODE.Text = ""                                            '銀行コード
        work.WF_SEL_BANKBRANCHCODE.Text = ""                                      '支店コード
        work.WF_SEL_ACCOUNTTYPE.Text = ""                                         '口座種別
        work.WF_SEL_ACCOUNTNUMBER.Text = ""                                       '口座番号
        work.WF_SEL_ACCOUNTNAME.Text = ""                                         '口座名義
        work.WF_SEL_INACCOUNTCD.Text = ""                                         '社内口座コード
        work.WF_SEL_TAXCALCULATION.Text = "1"                                     '税計算区分
        work.WF_SEL_DEPOSITDAY.Text = "31"                                        '入金日
        work.WF_SEL_DEPOSITMONTHKBN.Text = "1"                                    '入金区分
        work.WF_SEL_CLOSINGDAY.Text = "31"                                        '計上締日
        work.WF_SEL_ACCOUNTINGMONTH.Text = "0"                                    '計上月区分
        work.WF_SEL_SLIPDESCRIPTION1.Text = ""                                    '伝票摘要１
        work.WF_SEL_SLIPDESCRIPTION2.Text = ""                                    '伝票摘要２
        work.WF_SEL_NEXTMONTHUNSETTLEDKBN.Text = "0"                              '運賃翌月未決済区分
        work.WF_SEL_DELFLG.Text = "0"                                             '削除
        work.WF_SEL_UPDYMD.Text = ""         　                                   '更新年月日
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                               '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0024tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

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
        CS0030REPORT.TBLDATA = LNM0024tbl                       'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        CS0030REPORT.TBLDATA = LNM0024tbl                       'データ参照Table
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
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0024KekkjmHistory.aspx")
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
        Dim TBLview As New DataView(LNM0024tbl)
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

        Dim WW_LineCNT As Integer = 0
        Dim WW_DBDataCheck As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0024tbl.Rows(WW_LineCNT)("LINECNT")                              '選択行

        work.WF_SEL_TORICODE2.Text = LNM0024tbl.Rows(WW_LineCNT)("TORICODE")                           '取引先コード
        work.WF_SEL_INVFILINGDEPT2.Text = LNM0024tbl.Rows(WW_LineCNT)("INVFILINGDEPT")                 '請求書提出部店
        work.WF_SEL_INVKESAIKBN2.Text = LNM0024tbl.Rows(WW_LineCNT)("INVKESAIKBN")                     '請求書決済区分
        work.WF_SEL_TORINAME.Text = LNM0024tbl.Rows(WW_LineCNT)("TORINAME")                            '取引先名称
        work.WF_SEL_TORINAMES.Text = LNM0024tbl.Rows(WW_LineCNT)("TORINAMES")                          '取引先略称
        work.WF_SEL_TORINAMEKANA.Text = LNM0024tbl.Rows(WW_LineCNT)("TORINAMEKANA")                    '取引先カナ名称
        work.WF_SEL_TORIDIVNAME.Text = LNM0024tbl.Rows(WW_LineCNT)("TORIDIVNAME")                      '取引先部門名称
        work.WF_SEL_TORICHARGE.Text = LNM0024tbl.Rows(WW_LineCNT)("TORICHARGE")                        '取引先担当者
        work.WF_SEL_TORIKBN.Text = LNM0024tbl.Rows(WW_LineCNT)("TORIKBN")                              '取引先区分
        work.WF_SEL_POSTNUM1.Text = LNM0024tbl.Rows(WW_LineCNT)("POSTNUM1")                            '郵便番号（上）
        work.WF_SEL_POSTNUM2.Text = LNM0024tbl.Rows(WW_LineCNT)("POSTNUM2")                            '郵便番号（下）
        work.WF_SEL_ADDR1.Text = LNM0024tbl.Rows(WW_LineCNT)("ADDR1")                                  '住所１
        work.WF_SEL_ADDR2.Text = LNM0024tbl.Rows(WW_LineCNT)("ADDR2")                                  '住所２
        work.WF_SEL_ADDR3.Text = LNM0024tbl.Rows(WW_LineCNT)("ADDR3")                                  '住所３
        work.WF_SEL_ADDR4.Text = LNM0024tbl.Rows(WW_LineCNT)("ADDR4")                                  '住所４
        work.WF_SEL_TEL.Text = LNM0024tbl.Rows(WW_LineCNT)("TEL")                                      '電話番号
        work.WF_SEL_FAX.Text = LNM0024tbl.Rows(WW_LineCNT)("FAX")                                      'ＦＡＸ番号
        work.WF_SEL_MAIL.Text = LNM0024tbl.Rows(WW_LineCNT)("MAIL")                                    'メールアドレス
        work.WF_SEL_BANKCODE.Text = LNM0024tbl.Rows(WW_LineCNT)("BANKCODE")                            '銀行コード
        work.WF_SEL_BANKBRANCHCODE.Text = LNM0024tbl.Rows(WW_LineCNT)("BANKBRANCHCODE")                '支店コード
        work.WF_SEL_ACCOUNTTYPE.Text = LNM0024tbl.Rows(WW_LineCNT)("ACCOUNTTYPE")                      '口座種別
        work.WF_SEL_ACCOUNTNUMBER.Text = LNM0024tbl.Rows(WW_LineCNT)("ACCOUNTNUMBER")                  '口座番号
        work.WF_SEL_ACCOUNTNAME.Text = LNM0024tbl.Rows(WW_LineCNT)("ACCOUNTNAME")                      '口座名義
        work.WF_SEL_INACCOUNTCD.Text = LNM0024tbl.Rows(WW_LineCNT)("INACCOUNTCD")                      '社内口座コード
        work.WF_SEL_TAXCALCULATION.Text = LNM0024tbl.Rows(WW_LineCNT)("TAXCALCULATION")                '税計算区分
        work.WF_SEL_DEPOSITDAY.Text = LNM0024tbl.Rows(WW_LineCNT)("DEPOSITDAY")                        '入金日
        work.WF_SEL_DEPOSITMONTHKBN.Text = LNM0024tbl.Rows(WW_LineCNT)("DEPOSITMONTHKBN")              '入金月区分
        work.WF_SEL_CLOSINGDAY.Text = LNM0024tbl.Rows(WW_LineCNT)("CLOSINGDAY")                        '計上締日
        work.WF_SEL_ACCOUNTINGMONTH.Text = LNM0024tbl.Rows(WW_LineCNT)("ACCOUNTINGMONTH")              '計上月区分
        work.WF_SEL_SLIPDESCRIPTION1.Text = LNM0024tbl.Rows(WW_LineCNT)("SLIPDESCRIPTION1")            '伝票摘要１
        work.WF_SEL_SLIPDESCRIPTION2.Text = LNM0024tbl.Rows(WW_LineCNT)("SLIPDESCRIPTION2")            '伝票摘要２
        work.WF_SEL_NEXTMONTHUNSETTLEDKBN.Text = LNM0024tbl.Rows(WW_LineCNT)("NEXTMONTHUNSETTLEDKBN")  '運賃翌月未決済区分

        work.WF_SEL_DELFLG.Text = LNM0024tbl.Rows(WW_LineCNT)("DELFLG")                                '削除
        work.WF_SEL_UPDYMD.Text = LNM0024tbl.Rows(WW_LineCNT)("UPDYMD")                                '更新年月日
        work.WF_SEL_UPDTIMSTP.Text = LNM0024tbl.Rows(WW_LineCNT)("UPDTIMSTP")                          'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                    '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0024row As DataRow In LNM0024tbl.Rows
            Select Case LNM0024row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0024tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0024tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0024tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0024tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0024tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0024tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0024tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        If Not work.WF_SEL_TORICODE2.Text = "" Then
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_TORICODE2.Text, work.WF_SEL_INVFILINGDEPT2.Text,
                                work.WF_SEL_INVKESAIKBN2.Text, work.WF_SEL_UPDTIMSTP.Text)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0024WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "営業収入決済条件マスタ一覧"
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
            Case LNM0024WRKINC.FILETYPE.EXCEL
                FileName = "営業収入決済条件マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0024WRKINC.FILETYPE.PDF
                FileName = "営業収入決済条件マスタ.pdf"
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
        sheet.Columns(LNM0024WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0024WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求書提出部店
        sheet.Columns(LNM0024WRKINC.INOUTEXCELCOL.INVKESAIKBN).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求書決済区分

        '入力不要列網掛け
        sheet.Columns(LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPTNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '請求書提出部店名称

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
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Value = "（必須）請求書提出部店"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPTNM).Value = "請求書提出部店名称"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INVKESAIKBN).Value = "（必須）請求書決済区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMES).Value = "取引先略称"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMEKANA).Value = "取引先カナ名称"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORIDIVNAME).Value = "取引先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORICHARGE).Value = "取引先担当者"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORIKBN).Value = "取引先区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM1).Value = "郵便番号（上）"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM2).Value = "郵便番号（下）"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR1).Value = "住所1"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR2).Value = "住所2"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR3).Value = "住所3"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR4).Value = "住所4"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TEL).Value = "電話番号"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.FAX).Value = "FAX番号"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.MAIL).Value = "メールアドレス"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.BANKCODE).Value = "銀行コード"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.BANKBRANCHCODE).Value = "支店コード"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTTYPE).Value = "口座種別"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNUMBER).Value = "口座番号"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = "口座名義"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INACCOUNTCD).Value = "社内口座コード"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TAXCALCULATION).Value = "税計算区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTINGMONTH).Value = "計上月区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITDAY).Value = "入金日"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITMONTHKBN).Value = "入金月区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.CLOSINGDAY).Value = "計上締日"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION1).Value = "伝票摘要1"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION2).Value = "伝票摘要2"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.NEXTMONTHUNSETTLEDKBN).Value = "運賃翌月未決済区分"
        sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT).Value = "変換前請求書提出部店"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '請求書提出部店
            COMMENT_get(SQLcon, "INVFILINGDEPT", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '取引先区分
            Dim WW_TORIKBN As New StringBuilder
            WW_TORIKBN.AppendLine("1:JR")
            WW_TORIKBN.AppendLine("2:運送会社")
            WW_TORIKBN.AppendLine("3:船会社")
            WW_TORIKBN.AppendLine("4:デポ")
            WW_TORIKBN.AppendLine("5:荷主")
            WW_TORIKBN.AppendLine("6:納入先")
            WW_TORIKBN.AppendLine("7：メーカー")
            sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORIKBN).AddComment(WW_TORIKBN.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TORIKBN).Comment.Shape
                .Width = 80
                .Height = 100
            End With

            '口座種別
            Dim WW_ACCOUNTTYPE As New StringBuilder
            WW_ACCOUNTTYPE.AppendLine("1:普通")
            WW_ACCOUNTTYPE.AppendLine("2:当座")
            sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTTYPE).AddComment(WW_ACCOUNTTYPE.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTTYPE).Comment.Shape
                .Width = 50
                .Height = 30
            End With

            '社内口座コード
            Dim WW_INACCOUNTCD As New StringBuilder
            WW_INACCOUNTCD.AppendLine("0001：三井住友銀行")
            WW_INACCOUNTCD.AppendLine("0002：三菱UFJ銀行")
            WW_INACCOUNTCD.AppendLine("0003：みずほ銀行")
            WW_INACCOUNTCD.AppendLine("0004：三井住友信託銀行")
            sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INACCOUNTCD).AddComment(WW_INACCOUNTCD.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.INACCOUNTCD).Comment.Shape
                .Width = 130
                .Height = 60
            End With

            '税計算区分
            Dim WW_TAXCALCULATION As New StringBuilder
            WW_TAXCALCULATION.AppendLine("1：合計額に対して税率を掛ける")
            WW_TAXCALCULATION.AppendLine("2：明細金額ごとに税率を掛ける")
            sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TAXCALCULATION).AddComment(WW_TAXCALCULATION.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.TAXCALCULATION).Comment.Shape
                .Width = 200
                .Height = 30
            End With

            '計上月区分
            '0：当月 1：翌月 2：翌々月 3：三ヶ月後 4：四ヶ月後
            Dim WW_ACCOUNTINGMONTH As New StringBuilder
            WW_ACCOUNTINGMONTH.AppendLine("0：当月")
            WW_ACCOUNTINGMONTH.AppendLine("1：翌月")
            WW_ACCOUNTINGMONTH.AppendLine("2：翌々月")
            WW_ACCOUNTINGMONTH.AppendLine("3：三ヶ月後")
            WW_ACCOUNTINGMONTH.AppendLine("4：四ヶ月後")
            sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTINGMONTH).AddComment(WW_ACCOUNTINGMONTH.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTINGMONTH).Comment.Shape
                .Width = 80
                .Height = 70
            End With

            '入金月区分
            COMMENT_get(SQLcon, "DEPOSITMONTHKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITMONTHKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITMONTHKBN).Comment.Shape
                    .Width = 80
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

        Dim WW_INVFILINGDEPT As String

        Dim WW_INVFILINGDEPTNM As String

        For Each Row As DataRow In LNM0024tbl.Rows
            WW_INVFILINGDEPT = Row("INVFILINGDEPT") '請求書提出部店

            '名称取得
            WW_INVFILINGDEPTNM = ""
            CODENAME_get("INVFILINGDEPT", WW_INVFILINGDEPT, WW_INVFILINGDEPTNM, WW_RtnSW) '請求書提出部店名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT).Value = WW_INVFILINGDEPT '請求書提出部店
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPTNM).Value = WW_INVFILINGDEPTNM '請求書提出部店名称
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.INVKESAIKBN).Value = Row("INVKESAIKBN") '請求書決済区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMES).Value = Row("TORINAMES") '取引先略称
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMEKANA).Value = Row("TORINAMEKANA") '取引先カナ名称
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORIDIVNAME).Value = Row("TORIDIVNAME") '取引先部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORICHARGE).Value = Row("TORICHARGE") '取引先担当者
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TORIKBN).Value = Row("TORIKBN") '取引先区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM1).Value = Row("POSTNUM1") '郵便番号（上）
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM2).Value = Row("POSTNUM2") '郵便番号（下）
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR1).Value = Row("ADDR1") '住所1
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR2).Value = Row("ADDR2") '住所2
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR3).Value = Row("ADDR3") '住所3
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR4).Value = Row("ADDR4") '住所4
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TEL).Value = Row("TEL") '電話番号
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.FAX).Value = Row("FAX") 'FAX番号
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.MAIL).Value = Row("MAIL") 'メールアドレス
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.BANKCODE).Value = Row("BANKCODE") '銀行コード
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.BANKBRANCHCODE).Value = Row("BANKBRANCHCODE") '支店コード
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTTYPE).Value = Row("ACCOUNTTYPE") '口座種別
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNUMBER).Value = Row("ACCOUNTNUMBER") '口座番号
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '口座名義
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.INACCOUNTCD).Value = Row("INACCOUNTCD") '社内口座コード
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.TAXCALCULATION).Value = Row("TAXCALCULATION") '税計算区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTINGMONTH).Value = Row("ACCOUNTINGMONTH") '計上月区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITDAY).Value = Row("DEPOSITDAY") '入金日
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITMONTHKBN).Value = Row("DEPOSITMONTHKBN") '入金月区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.CLOSINGDAY).Value = Row("CLOSINGDAY") '計上締日
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION1).Value = Row("SLIPDESCRIPTION1") '伝票摘要1
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION2).Value = Row("SLIPDESCRIPTION2") '伝票摘要2
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.NEXTMONTHUNSETTLEDKBN).Value = Row("NEXTMONTHUNSETTLEDKBN") '運賃翌日未決済区分
            sheet.Cells(WW_ACTIVEROW, LNM0024WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT).Value = Row("BEFOREINVFILINGDEPT") '変換前請求書提出部店

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
                Case "DELFLG"                 '削除フラグ
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                Case "INVFILINGDEPT"          '請求書提出部店
                    WW_PrmData = work.CreateUORGParam(Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG
                Case "DEPOSITMONTHKBN"        '入金月区分
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DEPOSITMONTHKBN")
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
    ''' セル表示用のコメント取得(固定値)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_CNT"></param>
    ''' <remarks></remarks>
    Protected Sub COMMENTFIX_get(ByVal SQLcon As MySqlConnection,
                                      ByVal I_FIELD As String,
                                      ByRef O_TEXT As String,
                                      ByRef O_CNT As Integer)

        O_TEXT = ""
        O_CNT = 0

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(KEYCODE) AS KEYCODE ")
        SQLStr.AppendLine("    ,RTRIM(VALUE1) AS VALUE1 ")
        SQLStr.AppendLine(" FROM LNG.VIW0001_FIXVALUE ")
        SQLStr.AppendLine(" WHERE DELFLG <> @DELFLG ")
        SQLStr.AppendLine("   AND CAMPCODE = @CAMPCODE ")
        SQLStr.AppendLine("   AND CLASS = @CLASS ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      KEYCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                '会社コード
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                P_CAMPCODE.Value = Master.USERCAMP

                '分類コード
                Dim P_CLASS As MySqlParameter = SQLcmd.Parameters.Add("@CLASS", MySqlDbType.VarChar, 20)
                P_CLASS.Value = I_FIELD

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        For Each Row As DataRow In WW_Tbl.Rows
                            If Not Trim(Row("KEYCODE")) = "" Then
                                prmDataList.AppendLine(Row("KEYCODE") + "：" + Row("VALUE1"))
                            End If
                        Next
                        O_TEXT = prmDataList.ToString
                        O_CNT = WW_Tbl.Rows.Count
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "VIW0001_FIXVALUE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:VIW0001_FIXVALUE Select"
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
        filePath = "D:\営業収入決済条件マスタ一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNM0024Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0024WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0024WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "営業収入決済条件マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0024Exceltbl) Then
            LNM0024Exceltbl = New DataTable
        End If
        If LNM0024Exceltbl.Columns.Count <> 0 Then
            LNM0024Exceltbl.Columns.Clear()
        End If
        LNM0024Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\KEKKJMEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "KEKKJMEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0024Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0024WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0024WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0024_KEKKJM ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0024Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024_KEKKJM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024_KEKKJM Select"
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

        Dim LNM0024Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0024Exceltblrow = LNM0024Exceltbl.NewRow

            'LINECNT
            LNM0024Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '取引先コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORICODE))
            WW_DATATYPE = DataTypeHT("TORICODE")
            LNM0024Exceltblrow("TORICODE") = LNM0024WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求書提出部店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.INVFILINGDEPT))
            WW_DATATYPE = DataTypeHT("INVFILINGDEPT")
            LNM0024Exceltblrow("INVFILINGDEPT") = LNM0024WRKINC.DataConvert("請求書提出部店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '請求書決済区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.INVKESAIKBN))
            WW_DATATYPE = DataTypeHT("INVKESAIKBN")
            LNM0024Exceltblrow("INVKESAIKBN") = LNM0024WRKINC.DataConvert("請求書決済区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先名称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAME))
            WW_DATATYPE = DataTypeHT("TORINAME")
            LNM0024Exceltblrow("TORINAME") = LNM0024WRKINC.DataConvert("取引先名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先略称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMES))
            WW_DATATYPE = DataTypeHT("TORINAMES")
            LNM0024Exceltblrow("TORINAMES") = LNM0024WRKINC.DataConvert("取引先略称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先カナ名称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORINAMEKANA))
            WW_DATATYPE = DataTypeHT("TORINAMEKANA")
            LNM0024Exceltblrow("TORINAMEKANA") = LNM0024WRKINC.DataConvert("取引先カナ名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先部門名称
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORIDIVNAME))
            WW_DATATYPE = DataTypeHT("TORIDIVNAME")
            LNM0024Exceltblrow("TORIDIVNAME") = LNM0024WRKINC.DataConvert("取引先部門名称", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先担当者
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORICHARGE))
            WW_DATATYPE = DataTypeHT("TORICHARGE")
            LNM0024Exceltblrow("TORICHARGE") = LNM0024WRKINC.DataConvert("取引先担当者", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '取引先区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TORIKBN))
            WW_DATATYPE = DataTypeHT("TORIKBN")
            LNM0024Exceltblrow("TORIKBN") = LNM0024WRKINC.DataConvert("取引先区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '郵便番号（上）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM1))
            WW_DATATYPE = DataTypeHT("POSTNUM1")
            LNM0024Exceltblrow("POSTNUM1") = LNM0024WRKINC.DataConvert("郵便番号（上）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '郵便番号（下）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.POSTNUM2))
            WW_DATATYPE = DataTypeHT("POSTNUM2")
            LNM0024Exceltblrow("POSTNUM2") = LNM0024WRKINC.DataConvert("郵便番号（下）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '住所１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR1))
            WW_DATATYPE = DataTypeHT("ADDR1")
            LNM0024Exceltblrow("ADDR1") = LNM0024WRKINC.DataConvert("住所１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '住所２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR2))
            WW_DATATYPE = DataTypeHT("ADDR2")
            LNM0024Exceltblrow("ADDR2") = LNM0024WRKINC.DataConvert("住所２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '住所３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR3))
            WW_DATATYPE = DataTypeHT("ADDR3")
            LNM0024Exceltblrow("ADDR3") = LNM0024WRKINC.DataConvert("住所３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '住所４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ADDR4))
            WW_DATATYPE = DataTypeHT("ADDR4")
            LNM0024Exceltblrow("ADDR4") = LNM0024WRKINC.DataConvert("住所４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '電話番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TEL))
            WW_DATATYPE = DataTypeHT("TEL")
            LNM0024Exceltblrow("TEL") = LNM0024WRKINC.DataConvert("電話番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'ＦＡＸ番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.FAX))
            WW_DATATYPE = DataTypeHT("FAX")
            LNM0024Exceltblrow("FAX") = LNM0024WRKINC.DataConvert("ＦＡＸ番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'メールアドレス
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.MAIL))
            WW_DATATYPE = DataTypeHT("MAIL")
            LNM0024Exceltblrow("MAIL") = LNM0024WRKINC.DataConvert("メールアドレス", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '銀行コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.BANKCODE))
            WW_DATATYPE = DataTypeHT("BANKCODE")
            LNM0024Exceltblrow("BANKCODE") = LNM0024WRKINC.DataConvert("銀行コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.BANKBRANCHCODE))
            WW_DATATYPE = DataTypeHT("BANKBRANCHCODE")
            LNM0024Exceltblrow("BANKBRANCHCODE") = LNM0024WRKINC.DataConvert("支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '口座種別
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTTYPE))
            WW_DATATYPE = DataTypeHT("ACCOUNTTYPE")
            LNM0024Exceltblrow("ACCOUNTTYPE") = LNM0024WRKINC.DataConvert("口座種別", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '口座番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNUMBER))
            WW_DATATYPE = DataTypeHT("ACCOUNTNUMBER")
            LNM0024Exceltblrow("ACCOUNTNUMBER") = LNM0024WRKINC.DataConvert("口座番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '口座名義
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTNAME))
            WW_DATATYPE = DataTypeHT("ACCOUNTNAME")
            LNM0024Exceltblrow("ACCOUNTNAME") = LNM0024WRKINC.DataConvert("口座名義", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '社内口座コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.INACCOUNTCD))
            WW_DATATYPE = DataTypeHT("INACCOUNTCD")
            LNM0024Exceltblrow("INACCOUNTCD") = LNM0024WRKINC.DataConvert("社内口座コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '税計算区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.TAXCALCULATION))
            WW_DATATYPE = DataTypeHT("TAXCALCULATION")
            LNM0024Exceltblrow("TAXCALCULATION") = LNM0024WRKINC.DataConvert("税計算区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '計上月区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.ACCOUNTINGMONTH))
            WW_DATATYPE = DataTypeHT("ACCOUNTINGMONTH")
            LNM0024Exceltblrow("ACCOUNTINGMONTH") = LNM0024WRKINC.DataConvert("計上月区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '計上締日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.CLOSINGDAY))
            WW_DATATYPE = DataTypeHT("CLOSINGDAY")
            LNM0024Exceltblrow("CLOSINGDAY") = LNM0024WRKINC.DataConvert("計上締日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '入金日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITDAY))
            WW_DATATYPE = DataTypeHT("DEPOSITDAY")
            LNM0024Exceltblrow("DEPOSITDAY") = LNM0024WRKINC.DataConvert("入金日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '入金月区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.DEPOSITMONTHKBN))
            WW_DATATYPE = DataTypeHT("DEPOSITMONTHKBN")
            LNM0024Exceltblrow("DEPOSITMONTHKBN") = LNM0024WRKINC.DataConvert("入金月区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '伝票摘要１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION1))
            WW_DATATYPE = DataTypeHT("SLIPDESCRIPTION1")
            LNM0024Exceltblrow("SLIPDESCRIPTION1") = LNM0024WRKINC.DataConvert("伝票摘要１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '伝票摘要２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.SLIPDESCRIPTION2))
            WW_DATATYPE = DataTypeHT("SLIPDESCRIPTION2")
            LNM0024Exceltblrow("SLIPDESCRIPTION2") = LNM0024WRKINC.DataConvert("伝票摘要２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '運賃翌月未決済区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.NEXTMONTHUNSETTLEDKBN))
            WW_DATATYPE = DataTypeHT("NEXTMONTHUNSETTLEDKBN")
            LNM0024Exceltblrow("NEXTMONTHUNSETTLEDKBN") = LNM0024WRKINC.DataConvert("運賃翌月未決済区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前請求書提出部店
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.BEFOREINVFILINGDEPT))
            WW_DATATYPE = DataTypeHT("BEFOREINVFILINGDEPT")
            LNM0024Exceltblrow("BEFOREINVFILINGDEPT") = LNM0024WRKINC.DataConvert("変換前請求書提出部店", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0024WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0024Exceltblrow("DELFLG") = LNM0024WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0024Exceltbl.Rows.Add(LNM0024Exceltblrow)

        Next
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0024_KEKKJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  coalesce(INVFILINGDEPT, '')             = @INVFILINGDEPT ")
        SQLStr.AppendLine("    AND  coalesce(INVKESAIKBN, '0')             = @INVKESAIKBN ")
        SQLStr.AppendLine("    AND  coalesce(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  coalesce(TORINAMES, '')             = @TORINAMES ")
        SQLStr.AppendLine("    AND  coalesce(TORINAMEKANA, '')             = @TORINAMEKANA ")
        SQLStr.AppendLine("    AND  coalesce(TORIDIVNAME, '')             = @TORIDIVNAME ")
        SQLStr.AppendLine("    AND  coalesce(TORICHARGE, '')             = @TORICHARGE ")
        SQLStr.AppendLine("    AND  coalesce(TORIKBN, '')             = @TORIKBN ")
        SQLStr.AppendLine("    AND  coalesce(POSTNUM1, '')             = @POSTNUM1 ")
        SQLStr.AppendLine("    AND  coalesce(POSTNUM2, '')             = @POSTNUM2 ")
        SQLStr.AppendLine("    AND  coalesce(ADDR1, '')             = @ADDR1 ")
        SQLStr.AppendLine("    AND  coalesce(ADDR2, '')             = @ADDR2 ")
        SQLStr.AppendLine("    AND  coalesce(ADDR3, '')             = @ADDR3 ")
        SQLStr.AppendLine("    AND  coalesce(ADDR4, '')             = @ADDR4 ")
        SQLStr.AppendLine("    AND  coalesce(TEL, '')             = @TEL ")
        SQLStr.AppendLine("    AND  coalesce(FAX, '')             = @FAX ")
        SQLStr.AppendLine("    AND  coalesce(MAIL, '')             = @MAIL ")
        SQLStr.AppendLine("    AND  coalesce(BANKCODE, '')             = @BANKCODE ")
        SQLStr.AppendLine("    AND  coalesce(BANKBRANCHCODE, '')             = @BANKBRANCHCODE ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTTYPE, '')             = @ACCOUNTTYPE ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTNUMBER, '')             = @ACCOUNTNUMBER ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTNAME, '')             = @ACCOUNTNAME ")
        SQLStr.AppendLine("    AND  coalesce(INACCOUNTCD, '')             = @INACCOUNTCD ")
        SQLStr.AppendLine("    AND  coalesce(TAXCALCULATION, '')             = @TAXCALCULATION ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTINGMONTH, '')             = @ACCOUNTINGMONTH ")
        SQLStr.AppendLine("    AND  coalesce(CLOSINGDAY, '0')             = @CLOSINGDAY ")
        SQLStr.AppendLine("    AND  coalesce(DEPOSITDAY, '0')             = @DEPOSITDAY ")
        SQLStr.AppendLine("    AND  coalesce(DEPOSITMONTHKBN, '')             = @DEPOSITMONTHKBN ")
        SQLStr.AppendLine("    AND  coalesce(SLIPDESCRIPTION1, '')             = @SLIPDESCRIPTION1 ")
        SQLStr.AppendLine("    AND  coalesce(SLIPDESCRIPTION2, '')             = @SLIPDESCRIPTION2 ")
        SQLStr.AppendLine("    AND  coalesce(NEXTMONTHUNSETTLEDKBN, '0')             = @NEXTMONTHUNSETTLEDKBN ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREINVFILINGDEPT, '')             = @BEFOREINVFILINGDEPT ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 100)         '取引先名称
                Dim P_TORINAMES As MySqlParameter = SQLcmd.Parameters.Add("@TORINAMES", MySqlDbType.VarChar, 50)         '取引先略称
                Dim P_TORINAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@TORINAMEKANA", MySqlDbType.VarChar, 100)         '取引先カナ名称
                Dim P_TORIDIVNAME As MySqlParameter = SQLcmd.Parameters.Add("@TORIDIVNAME", MySqlDbType.VarChar, 50)         '取引先部門名称
                Dim P_TORICHARGE As MySqlParameter = SQLcmd.Parameters.Add("@TORICHARGE", MySqlDbType.VarChar, 20)         '取引先担当者
                Dim P_TORIKBN As MySqlParameter = SQLcmd.Parameters.Add("@TORIKBN", MySqlDbType.VarChar, 1)         '取引先区分
                Dim P_POSTNUM1 As MySqlParameter = SQLcmd.Parameters.Add("@POSTNUM1", MySqlDbType.VarChar, 3)         '郵便番号（上）
                Dim P_POSTNUM2 As MySqlParameter = SQLcmd.Parameters.Add("@POSTNUM2", MySqlDbType.VarChar, 4)         '郵便番号（下）
                Dim P_ADDR1 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR1", MySqlDbType.VarChar, 120)         '住所１
                Dim P_ADDR2 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR2", MySqlDbType.VarChar, 120)         '住所２
                Dim P_ADDR3 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR3", MySqlDbType.VarChar, 120)         '住所３
                Dim P_ADDR4 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR4", MySqlDbType.VarChar, 120)         '住所４
                Dim P_TEL As MySqlParameter = SQLcmd.Parameters.Add("@TEL", MySqlDbType.VarChar, 15)         '電話番号
                Dim P_FAX As MySqlParameter = SQLcmd.Parameters.Add("@FAX", MySqlDbType.VarChar, 15)         'ＦＡＸ番号
                Dim P_MAIL As MySqlParameter = SQLcmd.Parameters.Add("@MAIL", MySqlDbType.VarChar, 128)         'メールアドレス
                Dim P_BANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@BANKCODE", MySqlDbType.VarChar, 10)         '銀行コード
                Dim P_BANKBRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BANKBRANCHCODE", MySqlDbType.VarChar, 10)         '支店コード
                Dim P_ACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTTYPE", MySqlDbType.VarChar, 1)         '口座種別
                Dim P_ACCOUNTNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNUMBER", MySqlDbType.VarChar, 10)         '口座番号
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 30)         '口座名義
                Dim P_INACCOUNTCD As MySqlParameter = SQLcmd.Parameters.Add("@INACCOUNTCD", MySqlDbType.VarChar, 4)         '社内口座コード
                Dim P_TAXCALCULATION As MySqlParameter = SQLcmd.Parameters.Add("@TAXCALCULATION", MySqlDbType.VarChar, 1)         '税計算区分
                Dim P_ACCOUNTINGMONTH As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGMONTH", MySqlDbType.VarChar, 1)         '計上月区分
                Dim P_CLOSINGDAY As MySqlParameter = SQLcmd.Parameters.Add("@CLOSINGDAY", MySqlDbType.Int32)         '計上締日
                Dim P_DEPOSITDAY As MySqlParameter = SQLcmd.Parameters.Add("@DEPOSITDAY", MySqlDbType.VarChar, 2)         '入金日
                Dim P_DEPOSITMONTHKBN As MySqlParameter = SQLcmd.Parameters.Add("@DEPOSITMONTHKBN", MySqlDbType.VarChar, 1)         '入金月区分
                Dim P_SLIPDESCRIPTION1 As MySqlParameter = SQLcmd.Parameters.Add("@SLIPDESCRIPTION1", MySqlDbType.VarChar, 42)         '伝票摘要１
                Dim P_SLIPDESCRIPTION2 As MySqlParameter = SQLcmd.Parameters.Add("@SLIPDESCRIPTION2", MySqlDbType.VarChar, 42)         '伝票摘要２
                Dim P_NEXTMONTHUNSETTLEDKBN As MySqlParameter = SQLcmd.Parameters.Add("@NEXTMONTHUNSETTLEDKBN", MySqlDbType.VarChar, 1)         '運賃翌月未決済区分
                Dim P_BEFOREINVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVFILINGDEPT", MySqlDbType.VarChar, 6)         '変換前請求書提出部店
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")               '請求書提出部店
                P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")               '請求書決済区分
                P_TORINAME.Value = WW_ROW("TORINAME")               '取引先名称
                P_TORINAMES.Value = WW_ROW("TORINAMES")               '取引先略称
                P_TORINAMEKANA.Value = WW_ROW("TORINAMEKANA")               '取引先カナ名称
                P_TORIDIVNAME.Value = WW_ROW("TORIDIVNAME")               '取引先部門名称
                P_TORICHARGE.Value = WW_ROW("TORICHARGE")               '取引先担当者
                P_TORIKBN.Value = WW_ROW("TORIKBN")               '取引先区分
                P_POSTNUM1.Value = WW_ROW("POSTNUM1")               '郵便番号（上）
                P_POSTNUM2.Value = WW_ROW("POSTNUM2")               '郵便番号（下）
                P_ADDR1.Value = WW_ROW("ADDR1")               '住所１
                P_ADDR2.Value = WW_ROW("ADDR2")               '住所２
                P_ADDR3.Value = WW_ROW("ADDR3")               '住所３
                P_ADDR4.Value = WW_ROW("ADDR4")               '住所４
                P_TEL.Value = WW_ROW("TEL")               '電話番号
                P_FAX.Value = WW_ROW("FAX")               'ＦＡＸ番号
                P_MAIL.Value = WW_ROW("MAIL")               'メールアドレス
                P_BANKCODE.Value = WW_ROW("BANKCODE")               '銀行コード
                P_BANKBRANCHCODE.Value = WW_ROW("BANKBRANCHCODE")               '支店コード
                P_ACCOUNTTYPE.Value = WW_ROW("ACCOUNTTYPE")               '口座種別
                P_ACCOUNTNUMBER.Value = WW_ROW("ACCOUNTNUMBER")               '口座番号
                P_ACCOUNTNAME.Value = WW_ROW("ACCOUNTNAME")               '口座名義
                P_INACCOUNTCD.Value = WW_ROW("INACCOUNTCD")               '社内口座コード
                P_TAXCALCULATION.Value = WW_ROW("TAXCALCULATION")               '税計算区分
                P_ACCOUNTINGMONTH.Value = WW_ROW("ACCOUNTINGMONTH")               '計上月区分
                P_CLOSINGDAY.Value = WW_ROW("CLOSINGDAY")               '計上締日
                P_DEPOSITDAY.Value = WW_ROW("DEPOSITDAY")               '入金日
                P_DEPOSITMONTHKBN.Value = WW_ROW("DEPOSITMONTHKBN")               '入金月区分
                P_SLIPDESCRIPTION1.Value = WW_ROW("SLIPDESCRIPTION1")               '伝票摘要１
                P_SLIPDESCRIPTION2.Value = WW_ROW("SLIPDESCRIPTION2")               '伝票摘要２
                P_NEXTMONTHUNSETTLEDKBN.Value = WW_ROW("NEXTMONTHUNSETTLEDKBN")               '運賃翌月未決済区分
                P_BEFOREINVFILINGDEPT.Value = WW_ROW("BEFOREINVFILINGDEPT")               '変換前請求書提出部店
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024_KEKKJM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024_KEKKJM SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0024_KEKKJM LNM0024")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @TORICODE AS TORICODE ")
        SQLStr.AppendLine("             ,@INVFILINGDEPT AS INVFILINGDEPT ")
        SQLStr.AppendLine("             ,@INVKESAIKBN AS INVKESAIKBN ")
        SQLStr.AppendLine("             ,@TORINAME AS TORINAME ")
        SQLStr.AppendLine("             ,@TORINAMES AS TORINAMES ")
        SQLStr.AppendLine("             ,@TORINAMEKANA AS TORINAMEKANA ")
        SQLStr.AppendLine("             ,@TORIDIVNAME AS TORIDIVNAME ")
        SQLStr.AppendLine("             ,@TORICHARGE AS TORICHARGE ")
        SQLStr.AppendLine("             ,@TORIKBN AS TORIKBN ")
        SQLStr.AppendLine("             ,@POSTNUM1 AS POSTNUM1 ")
        SQLStr.AppendLine("             ,@POSTNUM2 AS POSTNUM2 ")
        SQLStr.AppendLine("             ,@ADDR1 AS ADDR1 ")
        SQLStr.AppendLine("             ,@ADDR2 AS ADDR2 ")
        SQLStr.AppendLine("             ,@ADDR3 AS ADDR3 ")
        SQLStr.AppendLine("             ,@ADDR4 AS ADDR4 ")
        SQLStr.AppendLine("             ,@TEL AS TEL ")
        SQLStr.AppendLine("             ,@FAX AS FAX ")
        SQLStr.AppendLine("             ,@MAIL AS MAIL ")
        SQLStr.AppendLine("             ,@BANKCODE AS BANKCODE ")
        SQLStr.AppendLine("             ,@BANKBRANCHCODE AS BANKBRANCHCODE ")
        SQLStr.AppendLine("             ,@ACCOUNTTYPE AS ACCOUNTTYPE ")
        SQLStr.AppendLine("             ,@ACCOUNTNUMBER AS ACCOUNTNUMBER ")
        SQLStr.AppendLine("             ,@ACCOUNTNAME AS ACCOUNTNAME ")
        SQLStr.AppendLine("             ,@INACCOUNTCD AS INACCOUNTCD ")
        SQLStr.AppendLine("             ,@TAXCALCULATION AS TAXCALCULATION ")
        SQLStr.AppendLine("             ,@ACCOUNTINGMONTH AS ACCOUNTINGMONTH ")
        SQLStr.AppendLine("             ,@CLOSINGDAY AS CLOSINGDAY ")
        SQLStr.AppendLine("             ,@DEPOSITDAY AS DEPOSITDAY ")
        SQLStr.AppendLine("             ,@DEPOSITMONTHKBN AS DEPOSITMONTHKBN ")
        SQLStr.AppendLine("             ,@SLIPDESCRIPTION1 AS SLIPDESCRIPTION1 ")
        SQLStr.AppendLine("             ,@SLIPDESCRIPTION2 AS SLIPDESCRIPTION2 ")
        SQLStr.AppendLine("             ,@NEXTMONTHUNSETTLEDKBN AS NEXTMONTHUNSETTLEDKBN ")
        SQLStr.AppendLine("             ,@BEFOREINVFILINGDEPT AS BEFOREINVFILINGDEPT ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0024.TORICODE = EXCEL.TORICODE ")
        SQLStr.AppendLine("         AND LNM0024.INVFILINGDEPT = EXCEL.INVFILINGDEPT ")
        SQLStr.AppendLine("         AND LNM0024.INVKESAIKBN = EXCEL.INVKESAIKBN ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0024.TORINAME =  EXCEL.TORINAME")
        SQLStr.AppendLine("         ,LNM0024.TORINAMES =  EXCEL.TORINAMES")
        SQLStr.AppendLine("         ,LNM0024.TORINAMEKANA =  EXCEL.TORINAMEKANA")
        SQLStr.AppendLine("         ,LNM0024.TORIDIVNAME =  EXCEL.TORIDIVNAME")
        SQLStr.AppendLine("         ,LNM0024.TORICHARGE =  EXCEL.TORICHARGE")
        SQLStr.AppendLine("         ,LNM0024.TORIKBN =  EXCEL.TORIKBN")
        SQLStr.AppendLine("         ,LNM0024.POSTNUM1 =  EXCEL.POSTNUM1")
        SQLStr.AppendLine("         ,LNM0024.POSTNUM2 =  EXCEL.POSTNUM2")
        SQLStr.AppendLine("         ,LNM0024.ADDR1 =  EXCEL.ADDR1")
        SQLStr.AppendLine("         ,LNM0024.ADDR2 =  EXCEL.ADDR2")
        SQLStr.AppendLine("         ,LNM0024.ADDR3 =  EXCEL.ADDR3")
        SQLStr.AppendLine("         ,LNM0024.ADDR4 =  EXCEL.ADDR4")
        SQLStr.AppendLine("         ,LNM0024.TEL =  EXCEL.TEL")
        SQLStr.AppendLine("         ,LNM0024.FAX =  EXCEL.FAX")
        SQLStr.AppendLine("         ,LNM0024.MAIL =  EXCEL.MAIL")
        SQLStr.AppendLine("         ,LNM0024.BANKCODE =  EXCEL.BANKCODE")
        SQLStr.AppendLine("         ,LNM0024.BANKBRANCHCODE =  EXCEL.BANKBRANCHCODE")
        SQLStr.AppendLine("         ,LNM0024.ACCOUNTTYPE =  EXCEL.ACCOUNTTYPE")
        SQLStr.AppendLine("         ,LNM0024.ACCOUNTNUMBER =  EXCEL.ACCOUNTNUMBER")
        SQLStr.AppendLine("         ,LNM0024.ACCOUNTNAME =  EXCEL.ACCOUNTNAME")
        SQLStr.AppendLine("         ,LNM0024.INACCOUNTCD =  EXCEL.INACCOUNTCD")
        SQLStr.AppendLine("         ,LNM0024.TAXCALCULATION =  EXCEL.TAXCALCULATION")
        SQLStr.AppendLine("         ,LNM0024.ACCOUNTINGMONTH =  EXCEL.ACCOUNTINGMONTH")
        SQLStr.AppendLine("         ,LNM0024.CLOSINGDAY =  EXCEL.CLOSINGDAY")
        SQLStr.AppendLine("         ,LNM0024.DEPOSITDAY =  EXCEL.DEPOSITDAY")
        SQLStr.AppendLine("         ,LNM0024.DEPOSITMONTHKBN =  EXCEL.DEPOSITMONTHKBN")
        SQLStr.AppendLine("         ,LNM0024.SLIPDESCRIPTION1 =  EXCEL.SLIPDESCRIPTION1")
        SQLStr.AppendLine("         ,LNM0024.SLIPDESCRIPTION2 =  EXCEL.SLIPDESCRIPTION2")
        SQLStr.AppendLine("         ,LNM0024.NEXTMONTHUNSETTLEDKBN =  EXCEL.NEXTMONTHUNSETTLEDKBN")
        SQLStr.AppendLine("         ,LNM0024.BEFOREINVFILINGDEPT =  EXCEL.BEFOREINVFILINGDEPT")
        SQLStr.AppendLine("         ,LNM0024.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0024.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0024.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0024.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0024.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @TORICODE  ")
        SQLStr.AppendLine("        ,@INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,@INVKESAIKBN  ")
        SQLStr.AppendLine("        ,@TORINAME  ")
        SQLStr.AppendLine("        ,@TORINAMES  ")
        SQLStr.AppendLine("        ,@TORINAMEKANA  ")
        SQLStr.AppendLine("        ,@TORIDIVNAME  ")
        SQLStr.AppendLine("        ,@TORICHARGE  ")
        SQLStr.AppendLine("        ,@TORIKBN  ")
        SQLStr.AppendLine("        ,@POSTNUM1  ")
        SQLStr.AppendLine("        ,@POSTNUM2  ")
        SQLStr.AppendLine("        ,@ADDR1  ")
        SQLStr.AppendLine("        ,@ADDR2  ")
        SQLStr.AppendLine("        ,@ADDR3  ")
        SQLStr.AppendLine("        ,@ADDR4  ")
        SQLStr.AppendLine("        ,@TEL  ")
        SQLStr.AppendLine("        ,@FAX  ")
        SQLStr.AppendLine("        ,@MAIL  ")
        SQLStr.AppendLine("        ,@BANKCODE  ")
        SQLStr.AppendLine("        ,@BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,@ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,@ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,@ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,@INACCOUNTCD  ")
        SQLStr.AppendLine("        ,@TAXCALCULATION  ")
        SQLStr.AppendLine("        ,@ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,@CLOSINGDAY  ")
        SQLStr.AppendLine("        ,@DEPOSITDAY  ")
        SQLStr.AppendLine("        ,@DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,@SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,@SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,@NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,@BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 100)         '取引先名称
                Dim P_TORINAMES As MySqlParameter = SQLcmd.Parameters.Add("@TORINAMES", MySqlDbType.VarChar, 50)         '取引先略称
                Dim P_TORINAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@TORINAMEKANA", MySqlDbType.VarChar, 100)         '取引先カナ名称
                Dim P_TORIDIVNAME As MySqlParameter = SQLcmd.Parameters.Add("@TORIDIVNAME", MySqlDbType.VarChar, 50)         '取引先部門名称
                Dim P_TORICHARGE As MySqlParameter = SQLcmd.Parameters.Add("@TORICHARGE", MySqlDbType.VarChar, 20)         '取引先担当者
                Dim P_TORIKBN As MySqlParameter = SQLcmd.Parameters.Add("@TORIKBN", MySqlDbType.VarChar, 1)         '取引先区分
                Dim P_POSTNUM1 As MySqlParameter = SQLcmd.Parameters.Add("@POSTNUM1", MySqlDbType.VarChar, 3)         '郵便番号（上）
                Dim P_POSTNUM2 As MySqlParameter = SQLcmd.Parameters.Add("@POSTNUM2", MySqlDbType.VarChar, 4)         '郵便番号（下）
                Dim P_ADDR1 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR1", MySqlDbType.VarChar, 120)         '住所１
                Dim P_ADDR2 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR2", MySqlDbType.VarChar, 120)         '住所２
                Dim P_ADDR3 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR3", MySqlDbType.VarChar, 120)         '住所３
                Dim P_ADDR4 As MySqlParameter = SQLcmd.Parameters.Add("@ADDR4", MySqlDbType.VarChar, 120)         '住所４
                Dim P_TEL As MySqlParameter = SQLcmd.Parameters.Add("@TEL", MySqlDbType.VarChar, 15)         '電話番号
                Dim P_FAX As MySqlParameter = SQLcmd.Parameters.Add("@FAX", MySqlDbType.VarChar, 15)         'ＦＡＸ番号
                Dim P_MAIL As MySqlParameter = SQLcmd.Parameters.Add("@MAIL", MySqlDbType.VarChar, 128)         'メールアドレス
                Dim P_BANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@BANKCODE", MySqlDbType.VarChar, 10)         '銀行コード
                Dim P_BANKBRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BANKBRANCHCODE", MySqlDbType.VarChar, 10)         '支店コード
                Dim P_ACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTTYPE", MySqlDbType.VarChar, 1)         '口座種別
                Dim P_ACCOUNTNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNUMBER", MySqlDbType.VarChar, 10)         '口座番号
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 30)         '口座名義
                Dim P_INACCOUNTCD As MySqlParameter = SQLcmd.Parameters.Add("@INACCOUNTCD", MySqlDbType.VarChar, 4)         '社内口座コード
                Dim P_TAXCALCULATION As MySqlParameter = SQLcmd.Parameters.Add("@TAXCALCULATION", MySqlDbType.VarChar, 1)         '税計算区分
                Dim P_ACCOUNTINGMONTH As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGMONTH", MySqlDbType.VarChar, 1)         '計上月区分
                Dim P_CLOSINGDAY As MySqlParameter = SQLcmd.Parameters.Add("@CLOSINGDAY", MySqlDbType.Int32)         '計上締日
                Dim P_DEPOSITDAY As MySqlParameter = SQLcmd.Parameters.Add("@DEPOSITDAY", MySqlDbType.VarChar, 2)         '入金日
                Dim P_DEPOSITMONTHKBN As MySqlParameter = SQLcmd.Parameters.Add("@DEPOSITMONTHKBN", MySqlDbType.VarChar, 1)         '入金月区分
                Dim P_SLIPDESCRIPTION1 As MySqlParameter = SQLcmd.Parameters.Add("@SLIPDESCRIPTION1", MySqlDbType.VarChar, 42)         '伝票摘要１
                Dim P_SLIPDESCRIPTION2 As MySqlParameter = SQLcmd.Parameters.Add("@SLIPDESCRIPTION2", MySqlDbType.VarChar, 42)         '伝票摘要２
                Dim P_NEXTMONTHUNSETTLEDKBN As MySqlParameter = SQLcmd.Parameters.Add("@NEXTMONTHUNSETTLEDKBN", MySqlDbType.VarChar, 1)         '運賃翌月未決済区分
                Dim P_BEFOREINVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREINVFILINGDEPT", MySqlDbType.VarChar, 6)         '変換前請求書提出部店
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
                P_TORICODE.Value = WW_ROW("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")               '請求書提出部店
                P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")               '請求書決済区分
                P_TORINAME.Value = WW_ROW("TORINAME")               '取引先名称


                '取引先略称
                If Not WW_ROW("TORINAMES") = "" Then
                    P_TORINAMES.Value = WW_ROW("TORINAMES")
                Else
                    P_TORINAMES.Value = DBNull.Value
                End If

                '取引先カナ名称
                If Not WW_ROW("TORINAMEKANA") = "" Then
                    P_TORINAMEKANA.Value = WW_ROW("TORINAMEKANA")
                Else
                    P_TORINAMEKANA.Value = DBNull.Value
                End If

                '取引先部門名称
                If Not WW_ROW("TORIDIVNAME") = "" Then
                    P_TORIDIVNAME.Value = WW_ROW("TORIDIVNAME")
                Else
                    P_TORIDIVNAME.Value = DBNull.Value
                End If

                '取引先担当者
                If Not WW_ROW("TORICHARGE") = "" Then
                    P_TORICHARGE.Value = WW_ROW("TORICHARGE")
                Else
                    P_TORICHARGE.Value = DBNull.Value
                End If

                '取引先区分
                If Not WW_ROW("TORIKBN") = "" Then
                    P_TORIKBN.Value = WW_ROW("TORIKBN")
                Else
                    P_TORIKBN.Value = DBNull.Value
                End If

                '郵便番号（上）
                If Not WW_ROW("POSTNUM1") = "" Then
                    P_POSTNUM1.Value = WW_ROW("POSTNUM1")
                Else
                    P_POSTNUM1.Value = DBNull.Value
                End If

                '郵便番号（下）
                If Not WW_ROW("POSTNUM2") = "" Then
                    P_POSTNUM2.Value = WW_ROW("POSTNUM2")
                Else
                    P_POSTNUM2.Value = DBNull.Value
                End If

                '住所１
                If Not WW_ROW("ADDR1") = "" Then
                    P_ADDR1.Value = WW_ROW("ADDR1")
                Else
                    P_ADDR1.Value = DBNull.Value
                End If

                '住所２
                If Not WW_ROW("ADDR2") = "" Then
                    P_ADDR2.Value = WW_ROW("ADDR2")
                Else
                    P_ADDR2.Value = DBNull.Value
                End If

                '住所３
                If Not WW_ROW("ADDR3") = "" Then
                    P_ADDR3.Value = WW_ROW("ADDR3")
                Else
                    P_ADDR3.Value = DBNull.Value
                End If

                '住所４
                If Not WW_ROW("ADDR4") = "" Then
                    P_ADDR4.Value = WW_ROW("ADDR4")
                Else
                    P_ADDR4.Value = DBNull.Value
                End If

                '電話番号
                If Not WW_ROW("TEL") = "" Then
                    P_TEL.Value = WW_ROW("TEL")
                Else
                    P_TEL.Value = DBNull.Value
                End If

                'ＦＡＸ番号
                If Not WW_ROW("FAX") = "" Then
                    P_FAX.Value = WW_ROW("FAX")
                Else
                    P_FAX.Value = DBNull.Value
                End If

                'メールアドレス
                If Not WW_ROW("MAIL") = "" Then
                    P_MAIL.Value = WW_ROW("MAIL")
                Else
                    P_MAIL.Value = DBNull.Value
                End If

                '銀行コード
                If Not WW_ROW("BANKCODE") = "" Then
                    P_BANKCODE.Value = WW_ROW("BANKCODE")
                Else
                    P_BANKCODE.Value = DBNull.Value
                End If

                '支店コード
                If Not WW_ROW("BANKBRANCHCODE") = "" Then
                    P_BANKBRANCHCODE.Value = WW_ROW("BANKBRANCHCODE")
                Else
                    P_BANKBRANCHCODE.Value = DBNull.Value
                End If

                '口座種別
                If Not WW_ROW("ACCOUNTTYPE") = "" Then
                    P_ACCOUNTTYPE.Value = WW_ROW("ACCOUNTTYPE")
                Else
                    P_ACCOUNTTYPE.Value = DBNull.Value
                End If

                '口座番号
                If Not WW_ROW("ACCOUNTNUMBER") = "" Then
                    P_ACCOUNTNUMBER.Value = WW_ROW("ACCOUNTNUMBER")
                Else
                    P_ACCOUNTNUMBER.Value = DBNull.Value
                End If

                '口座名義
                If Not WW_ROW("ACCOUNTNAME") = "" Then
                    P_ACCOUNTNAME.Value = WW_ROW("ACCOUNTNAME")
                Else
                    P_ACCOUNTNAME.Value = DBNull.Value
                End If

                '社内口座コード
                If Not WW_ROW("INACCOUNTCD") = "" Then
                    P_INACCOUNTCD.Value = WW_ROW("INACCOUNTCD")
                Else
                    P_INACCOUNTCD.Value = DBNull.Value
                End If

                '税計算区分
                If Not WW_ROW("TAXCALCULATION") = "" Then
                    P_TAXCALCULATION.Value = WW_ROW("TAXCALCULATION")
                Else
                    P_TAXCALCULATION.Value = DBNull.Value
                End If

                '計上月区分
                If Not WW_ROW("ACCOUNTINGMONTH") = "" Then
                    P_ACCOUNTINGMONTH.Value = WW_ROW("ACCOUNTINGMONTH")
                Else
                    P_ACCOUNTINGMONTH.Value = DBNull.Value
                End If

                '計上締日
                If Not WW_ROW("CLOSINGDAY") = "0" Then
                    P_CLOSINGDAY.Value = WW_ROW("CLOSINGDAY")
                Else
                    P_CLOSINGDAY.Value = DBNull.Value
                End If

                '入金日
                If Not WW_ROW("DEPOSITDAY") = "0" Then
                    P_DEPOSITDAY.Value = WW_ROW("DEPOSITDAY")
                Else
                    P_DEPOSITDAY.Value = DBNull.Value
                End If

                '入金月区分
                If Not WW_ROW("DEPOSITMONTHKBN") = "" Then
                    P_DEPOSITMONTHKBN.Value = WW_ROW("DEPOSITMONTHKBN")
                Else
                    P_DEPOSITMONTHKBN.Value = DBNull.Value
                End If

                '伝票摘要１
                If Not WW_ROW("SLIPDESCRIPTION1") = "" Then
                    P_SLIPDESCRIPTION1.Value = WW_ROW("SLIPDESCRIPTION1")
                Else
                    P_SLIPDESCRIPTION1.Value = DBNull.Value
                End If

                '伝票摘要２
                If Not WW_ROW("SLIPDESCRIPTION2") = "" Then
                    P_SLIPDESCRIPTION2.Value = WW_ROW("SLIPDESCRIPTION2")
                Else
                    P_SLIPDESCRIPTION2.Value = DBNull.Value
                End If

                '運賃翌月未決済区分
                P_NEXTMONTHUNSETTLEDKBN.Value = WW_ROW("NEXTMONTHUNSETTLEDKBN")

                '変換前請求書提出部店
                If Not WW_ROW("BEFOREINVFILINGDEPT") = "" Then
                    P_BEFOREINVFILINGDEPT.Value = WW_ROW("BEFOREINVFILINGDEPT")
                Else
                    P_BEFOREINVFILINGDEPT.Value = DBNull.Value
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024_KEKKJM  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0024_KEKKJM  INSERTUPDATE"
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
            ' 値存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            '***2023/12/14 コンテナ決済マスタ(LNM0003_REKEJM)側でも同様のチェック処理をしており互いに登録できなくなるためこちらのチェックを外す
            '' 値存在チェック
            'CODENAME_get("TORICODE", WW_ROW("TORICODE"), WW_Dummy, WW_RtnSW)
            'If Not isNormal(WW_RtnSW) Then
            '    WW_CheckMES1 = "・得意先コードエラーです。"
            '    WW_CheckMES2 = "マスタに存在しません。"
            '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
        Else
            WW_CheckMES1 = "・得意先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 請求書提出部店(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INVFILINGDEPT", WW_ROW("INVFILINGDEPT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 値存在チェック
            CODENAME_get("INVFILINGDEPT", WW_ROW("INVFILINGDEPT"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・請求書提出部店エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・請求書提出部店エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If


        ' 請求書決済区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INVKESAIKBN", WW_ROW("INVKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求書決済区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
        End If

        ' 取引先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先略称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAMES", WW_ROW("TORINAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先略称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先カナ名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAMEKANA", WW_ROW("TORINAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先カナ名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORIDIVNAME", WW_ROW("TORIDIVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先担当者(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICHARGE", WW_ROW("TORICHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先担当者エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORIKBN", WW_ROW("TORIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 郵便番号（上）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "POSTNUM1", WW_ROW("POSTNUM1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・郵便番号（上）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 郵便番号（下）(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "POSTNUM2", WW_ROW("POSTNUM2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・郵便番号（下）エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 住所１(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ADDR1", WW_ROW("ADDR1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・住所１エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 住所２(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ADDR2", WW_ROW("ADDR2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・住所２エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 住所３(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ADDR3", WW_ROW("ADDR3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・住所３エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 住所４(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ADDR4", WW_ROW("ADDR4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・住所４エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 電話番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TEL", WW_ROW("TEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・電話番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' ＦＡＸ番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FAX", WW_ROW("FAX"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・ＦＡＸ番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' メールアドレス(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MAIL", WW_ROW("MAIL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・メールアドレスエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 銀行コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BANKCODE", WW_ROW("BANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・銀行コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 支店コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BANKBRANCHCODE", WW_ROW("BANKBRANCHCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・支店コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 口座種別(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTTYPE", WW_ROW("ACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・口座種別エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 口座番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTNUMBER", WW_ROW("ACCOUNTNUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・口座番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 口座名義(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", WW_ROW("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・口座名義エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 社内口座コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INACCOUNTCD", WW_ROW("INACCOUNTCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・社内口座コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 入金日(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPOSITDAY", WW_ROW("DEPOSITDAY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・入金日エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 入金月区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPOSITMONTHKBN", WW_ROW("DEPOSITMONTHKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・入金月区分です。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 計上月区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTINGMONTH", WW_ROW("ACCOUNTINGMONTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・計上月区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 伝票摘要１(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLIPDESCRIPTION1", WW_ROW("SLIPDESCRIPTION1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・伝票摘要１エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 伝票摘要２(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLIPDESCRIPTION2", WW_ROW("SLIPDESCRIPTION2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・伝票摘要２エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 運賃翌月未決済区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "NEXTMONTHUNSETTLEDKBN", WW_ROW("NEXTMONTHUNSETTLEDKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・運賃翌月未決済区分エラーです。"
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

        O_RTN = C_MESSAGE_NO.NORMAL

        'マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0024_KEKKJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE        = @TORICODE")
        SQLStr.AppendLine("    AND INVFILINGDEPT   = @INVFILINGDEPT")
        SQLStr.AppendLine("    AND INVKESAIKBN     = @INVKESAIKBN")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分

                P_TORICODE.Value = WW_ROW("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")             '請求書提出部店
                P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")       '請求書決済区分


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
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0097_KEKKJHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
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
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
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
        SQLStr.AppendLine("        LNG.LNM0024_KEKKJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND INVFILINGDEPT    = @INVFILINGDEPT")
        SQLStr.AppendLine("    AND INVKESAIKBN      = @INVKESAIKBN")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_TORICODE.Value = WW_ROW("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = WW_ROW("INVFILINGDEPT")             '請求書提出部店
                P_INVKESAIKBN.Value = WW_ROW("INVKESAIKBN")       '請求書決済区分

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0097_KEKKJHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0097_KEKKJHIST  INSERT"
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
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByVal I_VALUE As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "TORICODE"               '取引先コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE))
                Case "INVFILINGDEPT"               '請求書提出部店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateUORGParam(Master.USERCAMP))
                Case "INVKESAIKBN"                 '請求書決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "INVKESAIKBN"))
                Case "NEXTMONTHUNSETTLEDKB"       '運賃翌月未決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "NEXTMONTHUNSETTLEDKBN"))
                Case "DELFLG"                      '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "OUTPUTID"                    '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"                       '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

