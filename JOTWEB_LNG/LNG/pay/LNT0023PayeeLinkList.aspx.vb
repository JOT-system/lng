''************************************************************
' 支払先マスタメンテ一覧画面
' 作成日 2024/05/15
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴:2024/05/15 新規作成
'         :2024/08/02 星 送信時、送信チェック判定追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 支払先マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNT0023PayeeLinkList
    Inherits Page

    ''' <summary>
    ''' 文字列タイプ
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum STRINGTYPE
        NONE
        SQL_SERVER
        AP_SERVER
        LOG_DIR
        JNL_DIR
        PDF_DIR
        UPF_DIR
        SYS_DIR
        WEBAPI_URL
        WEBAPI_ACCOUNT
        WEBAPI_TOKEN_SYS
        WEBAPI_TOKEN_OIL
        WEBAPI_TOKEN_KAN
        WEBAPI_TOKEN_TYU
        WEBAPI_RENKEIFLG
        LICENSE
        ENVIRONMENT
    End Enum

    Private Const IniFileC As String = "C:\APPL\APPLINI\CTN\JOTWEB_LNG.ini"
    Private Const IniFileD As String = "D:\APPL\APPLINI\CTN\JOTWEB_LNG.ini"

    '○ 検索結果格納Table
    Private LNT0023tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNT0023Exceltbl As New DataTable                        'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    Private Const CONST_CHKLINK As String = "<div><input class=""chk-detail"" id=""chkLink"" type=""checkbox"" onchange=""chkLinkOnChange();"" /></div>"
    Private Const CONST_CHKLINK_CHECKED As String = "<div><input class=""chk-detail"" id=""chkLink"" type=""checkbox"" checked onchange=""chkLinkOnChange();"" /></div>"
    Private Const CONST_BTNLINK As String = "<div><input class=""btn-sticky"" id=""btnLink""　type=""button"" value=""送信"" readonly onclick=""btnLinkClick();"" /></div>"

    Private Const CONST_SCHEMA_KEN As String = "101449"                 '楽々明細APIスキーマID(検証)
    Private Const CONST_IMPORT_KEN As String = "100727"                 '楽々明細APIインポートID(検証)
    Private Const CONST_SCHEMA_HON As String = "101240"                 '楽々明細APIスキーマID(本番)
    Private Const CONST_IMPORT_HON As String = "100657"                 '楽々明細APIインポートID(本番)

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
                    Master.RecoverTable(LNT0023tbl)
                    '○ 画面入力状態保存
                    InputStatusSave()

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNT0023WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNT0023WRKINC.FILETYPE.PDF)
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
                        Case "WF_LinkButtonClick"       '送信ボタン押下
                            WF_LinkButton_Click()
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
            If Not IsNothing(LNT0023tbl) Then
                LNT0023tbl.Clear()
                LNT0023tbl.Dispose()
                LNT0023tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0023WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0023S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0023D Then
            Master.RecoverTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNT0023tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNT0023tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0023tbl)

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

        If IsNothing(LNT0023tbl) Then
            LNT0023tbl = New DataTable
        End If

        If LNT0023tbl.Columns.Count <> 0 Then
            LNT0023tbl.Columns.Clear()
        End If

        LNT0023tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを支払先マスタから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                               ")
        SQLStr.AppendLine("     1                                                                    AS 'SELECT'                 ")
        SQLStr.AppendLine("   , 0                                                                    AS HIDDEN                   ")
        SQLStr.AppendLine("   , 0                                                                    AS LINECNT                  ")
        SQLStr.AppendLine("   , ''                                                                   AS OPERATION                ")
        SQLStr.AppendLine("   , UPDTIMSTP                                                            AS UPDTIMSTP                ")
        SQLStr.AppendLine("   , coalesce(RTRIM(DELFLG), '')                                            AS DELFLG                   ")
        SQLStr.AppendLine("   , coalesce(RTRIM(TORICODE), '')                                          AS TORICODE                 ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CLIENTCODE), '')                                        AS CLIENTCODE               ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INVOICENUMBER), '')                                     AS INVOICENUMBER            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CLIENTNAME), '')                                        AS CLIENTNAME               ")
        SQLStr.AppendLine("   , coalesce(RTRIM(TORINAME), '')                                          AS TORINAME                 ")
        SQLStr.AppendLine("   , coalesce(RTRIM(TORIDIVNAME), '')                                       AS TORIDIVNAME              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKCODE), '')                                       AS PAYBANKCODE              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKNAME), '')                                       AS PAYBANKNAME              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKNAMEKANA), '')                                   AS PAYBANKNAMEKANA          ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKBRANCHCODE), '')                                 AS PAYBANKBRANCHCODE        ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKBRANCHNAME), '')                                 AS PAYBANKBRANCHNAME        ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYBANKBRANCHNAMEKANA), '')                             AS PAYBANKBRANCHNAMEKANA    ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYACCOUNTTYPENAME), '')                                AS PAYACCOUNTTYPENAME       ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYACCOUNTTYPE), '')                                    AS PAYACCOUNTTYPE           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYACCOUNT), '')                                        AS PAYACCOUNT               ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYACCOUNTNAME), '')                                    AS PAYACCOUNTNAME           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYORBANKCODE), '')                                     AS PAYORBANKCODE            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(PAYTAXCALCUNIT), '')                                    AS PAYTAXCALCUNIT           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(LINKSTATUS), '')                                        AS LINKSTATUS               ")
        SQLStr.AppendLine("   , IIF(LASTLINKYMD IS NULL, '', FORMAT(LASTLINKYMD, 'yyyy/MM/dd HH:mm:ss'))        AS LASTLINKYMD   ")

        '連携用
        'SQLStr.AppendLine(" , '" + CONST_CHKLINK + "'                                                AS LINKCHK                　")
        'SQLStr.AppendLine(" , '" + CONST_BTNLINK + "'                                                AS LINKBTN                　")
        SQLStr.AppendLine("   , ''                                                                   AS LINKCHK             ")
        SQLStr.AppendLine("   , ''                                                                   AS LINKBTN             ")
        SQLStr.AppendLine("   , ''                                                                   AS LINKSTATUSNM             ")

        SQLStr.AppendLine(" FROM                                                                                                 ")
        SQLStr.AppendLine("     LNG.LNT0072_PAYEE                                                                                ")
        SQLStr.AppendLine(" WHERE DELFLG  = @DELFLG ")

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ' 支払先コード
        If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE_S.Text) Then
            SQLStr.AppendLine("    AND TORICODE   = @TORICODE")
        End If
        ' 顧客コード
        If Not String.IsNullOrEmpty(work.WF_SEL_CLIENTCODE_S.Text) Then
            SQLStr.AppendLine("    AND CLIENTCODE   = @CLIENTCODE")
        End If

        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("    LASTLINKYMD DESC")
        SQLStr.AppendLine("   ,TORICODE")
        SQLStr.AppendLine("   ,CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)  '削除フラグ
                P_DELFLG.Value = work.WF_SEL_DELFLG_S.Text

                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE_S.Text) Then
                    Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)  '支払先コード
                    P_TORICODE.Value = work.WF_SEL_TORICODE_S.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_CLIENTCODE_S.Text) Then
                    Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)  '顧客コード
                    P_CLIENTCODE.Value = work.WF_SEL_CLIENTCODE_S.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0023tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0023tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0023row As DataRow In LNT0023tbl.Rows
                    i += 1
                    LNT0023row("LINECNT") = i        'LINECNT
                    LNT0023row("LINKBTN") = CONST_BTNLINK
                    LNT0023row("LINKCHK") = CONST_CHKLINK
                    'LNT0023row("LINKCHK") = CONST_CHKLINK_CHECKED

                    ' 連携状態(画面表示用)
                    Select Case LNT0023row("LINKSTATUS")
                        Case "1"
                            LNT0023row("LINKSTATUSNM") = "連携済"
                        Case Else
                            LNT0023row("LINKSTATUSNM") = "未連携"

                    End Select
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023L Select"
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
        For Each LNT0023row As DataRow In LNT0023tbl.Rows
            If LNT0023row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0023row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNT0023tbl)

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
    ''' 入力状態の保存
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InputStatusSave()
        If WF_lineCntLIST.Value = "" Then Exit Sub

        Dim WW_lineCntArray As String() = WF_lineCntLIST.Value.Split(",")
        Dim WW_CHKLINKLISTArray As String() = WF_CHKLINKLIST.Value.Split(",")
        Dim WW_LINECNT As Integer = 0

        Try

            For index As Integer = 0 To WW_lineCntArray.Count - 1
                WW_LINECNT = WW_lineCntArray(index) - 1

                If WW_CHKLINKLISTArray(index) = "有" Then
                    LNT0023tbl.Rows(WW_LINECNT)("LINKCHK") = CONST_CHKLINK_CHECKED
                Else
                    LNT0023tbl.Rows(WW_LINECNT)("LINKCHK") = CONST_CHKLINK
                End If
            Next

            Master.SaveTable(LNT0023tbl)

        Catch ex As Exception
        End Try
        WF_lineCntLIST.Value = ""
    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        work.WF_SEL_LINECNT.Text = ""                                                         '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)                '削除
        work.WF_SEL_TORICODE.Text = ""                                                        '支払先コード
        work.WF_SEL_CLIENTCODE.Text = ""                                                      '顧客コード
        work.WF_SEL_INVOICENUMBER.Text = ""                                                   'インボイス登録番号
        work.WF_SEL_CLIENTNAME.Text = ""                                                      '顧客名
        work.WF_SEL_TORINAME.Text = ""                                                        '会社名
        work.WF_SEL_TORIDIVNAME.Text = ""                                                     '部門名
        'work.WF_SEL_PAYBANKCODE.Text = "0000"                                                 '振込先銀行コード
        work.WF_SEL_PAYBANKCODE.Text = ""
        work.WF_SEL_PAYBANKNAME.Text = ""                                                     '振込先銀行名
        work.WF_SEL_PAYBANKNAMEKANA.Text = ""                                                 '振込先銀行名カナ
        'work.WF_SEL_PAYBANKBRANCHCODE.Text = "000"                                            '振込先支店コード
        work.WF_SEL_PAYBANKBRANCHCODE.Text = ""
        work.WF_SEL_PAYBANKBRANCHNAME.Text = ""                                               '振込先支店名
        work.WF_SEL_PAYBANKBRANCHNAMEKANA.Text = ""                                           '振込先支店名カナ
        work.WF_SEL_PAYACCOUNTTYPENAME.Text = ""                                              '預金種別
        work.WF_SEL_PAYACCOUNTTYPE.Text = "0"                                                 '預金種別コード
        work.WF_SEL_PAYACCOUNT.Text = ""                                                      '口座番号
        work.WF_SEL_PAYACCOUNTNAME.Text = ""                                                  '口座名義
        'work.WF_SEL_PAYORBANKCODE.Text = "0000"                                               '支払元銀行コード
        work.WF_SEL_PAYORBANKCODE.Text = ""
        work.WF_SEL_PAYTAXCALCUNIT.Text = "総額"                                              '消費税計算処理区分
        work.WF_SEL_LINKSTATUS.Text = "0"                                                     '連携状態区分
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                           '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNT0023tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/pay/LNT0023PayeeLinkHistory.aspx")
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
        Dim TBLview As New DataView(LNT0023tbl)
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

        work.WF_SEL_LINECNT.Text = LNT0023tbl.Rows(WW_LineCNT)("LINECNT")                                    '選択行
        work.WF_SEL_DELFLG.Text = LNT0023tbl.Rows(WW_LineCNT)("DELFLG")                                      '削除フラグ
        work.WF_SEL_TORICODE.Text = LNT0023tbl.Rows(WW_LineCNT)("TORICODE")                                  '支払先コード
        work.WF_SEL_CLIENTCODE.Text = LNT0023tbl.Rows(WW_LineCNT)("CLIENTCODE")                              '顧客コード
        work.WF_SEL_INVOICENUMBER.Text = LNT0023tbl.Rows(WW_LineCNT)("INVOICENUMBER")                        'インボイス登録番号
        work.WF_SEL_CLIENTNAME.Text = LNT0023tbl.Rows(WW_LineCNT)("CLIENTNAME")                              '顧客名
        work.WF_SEL_TORINAME.Text = LNT0023tbl.Rows(WW_LineCNT)("TORINAME")                                  '会社名
        work.WF_SEL_TORIDIVNAME.Text = LNT0023tbl.Rows(WW_LineCNT)("TORIDIVNAME")                            '部門名
        work.WF_SEL_PAYBANKCODE.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKCODE")                            '振込先銀行コード
        work.WF_SEL_PAYBANKNAME.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKNAME")                            '振込先銀行名
        work.WF_SEL_PAYBANKNAMEKANA.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKNAMEKANA")                    '振込先銀行名カナ
        work.WF_SEL_PAYBANKBRANCHCODE.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHCODE")                '振込先支店コード
        work.WF_SEL_PAYBANKBRANCHNAME.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHNAME")                '振込先支店名
        work.WF_SEL_PAYBANKBRANCHNAMEKANA.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHNAMEKANA")        '振込先支店名カナ
        work.WF_SEL_PAYACCOUNTTYPENAME.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTTYPENAME")              '預金種別
        work.WF_SEL_PAYACCOUNTTYPE.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTTYPE")                      '預金種別コード
        work.WF_SEL_PAYACCOUNT.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNT")                              '口座番号
        work.WF_SEL_PAYACCOUNTNAME.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTNAME")                      '口座名義
        work.WF_SEL_PAYORBANKCODE.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYORBANKCODE")                        '支払元銀行コード
        work.WF_SEL_PAYTAXCALCUNIT.Text = LNT0023tbl.Rows(WW_LineCNT)("PAYTAXCALCUNIT")                      '消費税計算処理区分
        work.WF_SEL_LINKSTATUS.Text = LNT0023tbl.Rows(WW_LineCNT)("LINKSTATUS")                              '連携状態区分

        work.WF_SEL_TIMESTAMP.Text = LNT0023tbl.Rows(WW_LineCNT)("UPDTIMSTP")                                'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                          '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNT0023row As DataRow In LNT0023tbl.Rows
            Select Case LNT0023row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNT0023tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNT0023tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNT0023tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNT0023tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNT0023tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNT0023tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNT0023tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)

        ' 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then  '支払先コード
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_TORICODE.Text, work.WF_SEL_CLIENTCODE.Text,
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNT0023WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "支払先マスタ一覧"
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
            Case LNT0023WRKINC.FILETYPE.EXCEL
                FileName = "支払先マスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNT0023WRKINC.FILETYPE.PDF
                FileName = "支払先マスタ.pdf"
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
        sheet.Columns(LNT0023WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNT0023WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '支払先コード
        sheet.Columns(LNT0023WRKINC.INOUTEXCELCOL.CLIENTCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '顧客コード

        '入力不要列網掛け
        sheet.Columns(LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '連携状態区分
        sheet.Columns(LNT0023WRKINC.INOUTEXCELCOL.LASTLINKYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '最終連携日

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
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）支払先コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTCODE).Value = "（必須）顧客コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.INVOICENUMBER).Value = "インボイス登録番号"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTNAME).Value = "顧客名"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.TORINAME).Value = "会社名"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.TORIDIVNAME).Value = "部門名"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKCODE).Value = "振込先銀行コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAME).Value = "振込先銀行名"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAMEKANA).Value = "振込先銀行名カナ"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCODE).Value = "振込先支店コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAME).Value = "振込先支店名"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAMEKANA).Value = "振込先支店名カナ"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPENAME).Value = "預金種別"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).Value = "預金種別コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNT).Value = "口座番号"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTNAME).Value = "口座名義"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYORBANKCODE).Value = "支払元銀行コード"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.PAYTAXCALCUNIT).Value = "消費税計算処理区分"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS).Value = "連携状態区分"
        sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.LASTLINKYMD).Value = "最終連携日"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            Dim WW_LINKSTATUS As New StringBuilder
            WW_LINKSTATUS.AppendLine("0：未連携")
            WW_LINKSTATUS.AppendLine("1：連携済")

            '連携状態区分
            sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS).AddComment(WW_LINKSTATUS.ToString)
            With sheet.Cells(WW_HEADERROW, LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS).Comment.Shape
                .Width = 150
                .Height = 50
            End With

        End Using

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        For Each Row As DataRow In LNT0023tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '支払先コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTCODE).Value = Row("CLIENTCODE") '顧客コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.INVOICENUMBER).Value = Row("INVOICENUMBER") 'インボイス登録番号	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTNAME).Value = Row("CLIENTNAME") '顧客名	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '会社名	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.TORIDIVNAME).Value = Row("TORIDIVNAME") '部門名	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKCODE).Value = Row("PAYBANKCODE") '振込先銀行コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAME).Value = Row("PAYBANKNAME") '振込先銀行名	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAMEKANA).Value = Row("PAYBANKNAMEKANA") '振込先銀行名カナ	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCODE).Value = Row("PAYBANKBRANCHCODE") '振込先支店コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAME).Value = Row("PAYBANKBRANCHNAME") '振込先支店名	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAMEKANA).Value = Row("PAYBANKBRANCHNAMEKANA") '振込先支店名カナ	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPENAME).Value = Row("PAYACCOUNTTYPENAME") '預金種別	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE).Value = Row("PAYACCOUNTTYPE") '預金種別コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNT).Value = Row("PAYACCOUNT") '口座番号	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTNAME).Value = Row("PAYACCOUNTNAME") '口座名義	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYORBANKCODE).Value = Row("PAYORBANKCODE") '支払元銀行コード	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.PAYTAXCALCUNIT).Value = Row("PAYTAXCALCUNIT") '消費税計算処理区分	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS).Value = Row("LINKSTATUS") '連携状態区分	
            sheet.Cells(WW_ACTIVEROW, LNT0023WRKINC.INOUTEXCELCOL.LASTLINKYMD).Value = Row("LASTLINKYMD") '最終連携日	

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
        filePath = "D:\支払先マスタ一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNT0023Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNT0023WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNT0023WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "支払先マスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNT0023Exceltbl) Then
            LNT0023Exceltbl = New DataTable
        End If
        If LNT0023Exceltbl.Columns.Count <> 0 Then
            LNT0023Exceltbl.Columns.Clear()
        End If
        LNT0023Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\PAYEELINKEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "PAYEELINKEXCEL_TMP_"

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

            For Each Row As DataRow In LNT0023Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNT0023WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNT0023WRKINC.MAPIDL
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
                    If WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        'SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNT0072_PAYEE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0023Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0072_PAYEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0072_PAYEE Select"
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

        Dim LNT0023Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNT0023Exceltblrow = LNT0023Exceltbl.NewRow

            'LINECNT
            LNT0023Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '支払先コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.TORICODE))
            If WW_TEXT = "" Then
                WW_CheckMES1 = "・[支払先コード]を取得できませんでした。"
                WW_CheckMES2 = "入力必須項目です。"
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            Else
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNT0023Exceltblrow("TORICODE") = LNT0023WRKINC.DataConvert("支払先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
            End If
            '顧客コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTCODE))
            If WW_TEXT = "" Then
                WW_CheckMES1 = "・[顧客コード]を取得できませんでした。"
                WW_CheckMES2 = "入力必須項目です。"
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            Else
                WW_DATATYPE = DataTypeHT("CLIENTCODE")
                LNT0023Exceltblrow("CLIENTCODE") = LNT0023WRKINC.DataConvert("顧客コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
            End If

            'インボイス登録番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.INVOICENUMBER))
            WW_DATATYPE = DataTypeHT("INVOICENUMBER")
            LNT0023Exceltblrow("INVOICENUMBER") = LNT0023WRKINC.DataConvert("インボイス登録番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '顧客名
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.CLIENTNAME))
            WW_DATATYPE = DataTypeHT("CLIENTNAME")
            LNT0023Exceltblrow("CLIENTNAME") = LNT0023WRKINC.DataConvert("顧客名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '会社名
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.TORINAME))
            WW_DATATYPE = DataTypeHT("TORINAME")
            LNT0023Exceltblrow("TORINAME") = LNT0023WRKINC.DataConvert("会社名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '部門名
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.TORIDIVNAME))
            WW_DATATYPE = DataTypeHT("TORIDIVNAME")
            LNT0023Exceltblrow("TORIDIVNAME") = LNT0023WRKINC.DataConvert("部門名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先銀行コード
            WW_TEXT = Strings.Right("0000" + Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKCODE)), 4)
            WW_DATATYPE = DataTypeHT("PAYBANKCODE")
            LNT0023Exceltblrow("PAYBANKCODE") = LNT0023WRKINC.DataConvert("振込先銀行コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先銀行名
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAME))
            WW_DATATYPE = DataTypeHT("PAYBANKNAME")
            LNT0023Exceltblrow("PAYBANKNAME") = LNT0023WRKINC.DataConvert("振込先銀行名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先銀行名カナ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKNAMEKANA))
            WW_DATATYPE = DataTypeHT("PAYBANKNAMEKANA")
            LNT0023Exceltblrow("PAYBANKNAMEKANA") = LNT0023WRKINC.DataConvert("振込先銀行名カナ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先支店コード
            WW_TEXT = Strings.Right("000" + Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHCODE)), 3)
            WW_DATATYPE = DataTypeHT("PAYBANKBRANCHCODE")
            LNT0023Exceltblrow("PAYBANKBRANCHCODE") = LNT0023WRKINC.DataConvert("振込先支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先支店名
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAME))
            WW_DATATYPE = DataTypeHT("PAYBANKBRANCHNAME")
            LNT0023Exceltblrow("PAYBANKBRANCHNAME") = LNT0023WRKINC.DataConvert("振込先支店名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '振込先支店名カナ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYBANKBRANCHNAMEKANA))
            WW_DATATYPE = DataTypeHT("PAYBANKBRANCHNAMEKANA")
            LNT0023Exceltblrow("PAYBANKBRANCHNAMEKANA") = LNT0023WRKINC.DataConvert("振込先支店名カナ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '預金種別
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPENAME))
            WW_DATATYPE = DataTypeHT("PAYACCOUNTTYPENAME")
            LNT0023Exceltblrow("PAYACCOUNTTYPENAME") = LNT0023WRKINC.DataConvert("預金種別", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '預金種別コード
            WW_TEXT = Strings.Right("0" + Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTTYPE)), 1)
            WW_DATATYPE = DataTypeHT("PAYACCOUNTTYPE")
            LNT0023Exceltblrow("PAYACCOUNTTYPE") = LNT0023WRKINC.DataConvert("預金種別コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '口座番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNT))
            WW_DATATYPE = DataTypeHT("PAYACCOUNT")
            LNT0023Exceltblrow("PAYACCOUNT") = LNT0023WRKINC.DataConvert("口座番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '口座名義
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYACCOUNTNAME))
            WW_DATATYPE = DataTypeHT("PAYACCOUNTNAME")
            LNT0023Exceltblrow("PAYACCOUNTNAME") = LNT0023WRKINC.DataConvert("口座名義", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '支払元銀行コード
            WW_TEXT = Strings.Right("0000" + Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYORBANKCODE)), 4)
            WW_DATATYPE = DataTypeHT("PAYORBANKCODE")
            LNT0023Exceltblrow("PAYORBANKCODE") = LNT0023WRKINC.DataConvert("支払元銀行コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '消費税計算処理区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.PAYTAXCALCUNIT))
            WW_DATATYPE = DataTypeHT("PAYTAXCALCUNIT")
            LNT0023Exceltblrow("PAYTAXCALCUNIT") = LNT0023WRKINC.DataConvert("消費税計算処理区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            ''連携状態区分
            'WW_TEXT = Strings.Right("0" + Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.LINKSTATUS)), 1)
            'WW_DATATYPE = DataTypeHT("LINKSTATUS")
            'LNT0023Exceltblrow("LINKSTATUS") = LNT0023WRKINC.DataConvert("連携状態区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            'If WW_RESULT = False Then
            '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            'End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0023WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNT0023Exceltblrow("DELFLG") = LNT0023WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNT0023Exceltbl.Rows.Add(LNT0023Exceltblrow)

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
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  coalesce(CLIENTCODE, '')             = @CLIENTCODE ")
        SQLStr.AppendLine("    AND  coalesce(INVOICENUMBER, '')             = @INVOICENUMBER ")
        SQLStr.AppendLine("    AND  coalesce(CLIENTNAME, '')             = @CLIENTNAME ")
        SQLStr.AppendLine("    AND  coalesce(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  coalesce(TORIDIVNAME, '')             = @TORIDIVNAME ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKCODE, '')             = @PAYBANKCODE ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKNAME, '')             = @PAYBANKNAME ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKNAMEKANA, '')             = @PAYBANKNAMEKANA ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKBRANCHCODE, '')             = @PAYBANKBRANCHCODE ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKBRANCHNAME, '')             = @PAYBANKBRANCHNAME ")
        SQLStr.AppendLine("    AND  coalesce(PAYBANKBRANCHNAMEKANA, '')             = @PAYBANKBRANCHNAMEKANA ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTTYPENAME, '')             = @PAYACCOUNTTYPENAME ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTTYPE, '')             = @PAYACCOUNTTYPE ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNT, '0')             = @PAYACCOUNT ")
        SQLStr.AppendLine("    AND  coalesce(PAYACCOUNTNAME, '')             = @PAYACCOUNTNAME ")
        SQLStr.AppendLine("    AND  coalesce(PAYORBANKCODE, '')             = @PAYORBANKCODE ")
        SQLStr.AppendLine("    AND  coalesce(PAYTAXCALCUNIT, '')             = @PAYTAXCALCUNIT ")
        'SQLStr.AppendLine("    AND  coalesce(LINKSTATUS, '')             = @LINKSTATUS ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード
                Dim P_INVOICENUMBER As MySqlParameter = SQLcmd.Parameters.Add("@INVOICENUMBER", MySqlDbType.VarChar, 14)         'インボイス登録番号
                Dim P_CLIENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTNAME", MySqlDbType.VarChar, 32)         '顧客名
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 32)         '会社名
                Dim P_TORIDIVNAME As MySqlParameter = SQLcmd.Parameters.Add("@TORIDIVNAME", MySqlDbType.VarChar, 32)         '部門名
                Dim P_PAYBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKCODE", MySqlDbType.VarChar, 4)         '振込先銀行コード
                Dim P_PAYBANKNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAME", MySqlDbType.VarChar, 30)         '振込先銀行名
                Dim P_PAYBANKNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAMEKANA", MySqlDbType.VarChar, 30)         '振込先銀行名カナ
                Dim P_PAYBANKBRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHCODE", MySqlDbType.VarChar, 3)         '振込先支店コード
                Dim P_PAYBANKBRANCHNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAME", MySqlDbType.VarChar, 30)         '振込先支店名
                Dim P_PAYBANKBRANCHNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAMEKANA", MySqlDbType.VarChar, 30)         '振込先支店名カナ
                Dim P_PAYACCOUNTTYPENAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPENAME", MySqlDbType.VarChar, 10)         '預金種別
                Dim P_PAYACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPE", MySqlDbType.VarChar, 1)         '預金種別コード
                Dim P_PAYACCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNT", MySqlDbType.VarChar, 8)         '口座番号
                Dim P_PAYACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNAME", MySqlDbType.VarChar, 30)         '口座名義
                Dim P_PAYORBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYORBANKCODE", MySqlDbType.VarChar, 4)         '支払元銀行コード
                Dim P_PAYTAXCALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@PAYTAXCALCUNIT", MySqlDbType.VarChar, 10)         '消費税計算処理区分
                'Dim P_LINKSTATUS As MySqlParameter = SQLcmd.Parameters.Add("@LINKSTATUS", MySqlDbType.VarChar, 1)         '連携状態区分
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = WW_ROW("CLIENTCODE")               '顧客コード
                P_INVOICENUMBER.Value = WW_ROW("INVOICENUMBER")               'インボイス登録番号
                P_CLIENTNAME.Value = WW_ROW("CLIENTNAME")               '顧客名
                P_TORINAME.Value = WW_ROW("TORINAME")               '会社名
                P_TORIDIVNAME.Value = WW_ROW("TORIDIVNAME")               '部門名
                P_PAYBANKCODE.Value = WW_ROW("PAYBANKCODE")               '振込先銀行コード
                P_PAYBANKNAME.Value = WW_ROW("PAYBANKNAME")               '振込先銀行名
                P_PAYBANKNAMEKANA.Value = WW_ROW("PAYBANKNAMEKANA")               '振込先銀行名カナ
                P_PAYBANKBRANCHCODE.Value = WW_ROW("PAYBANKBRANCHCODE")               '振込先支店コード
                P_PAYBANKBRANCHNAME.Value = WW_ROW("PAYBANKBRANCHNAME")               '振込先支店名
                P_PAYBANKBRANCHNAMEKANA.Value = WW_ROW("PAYBANKBRANCHNAMEKANA")               '振込先支店名カナ
                P_PAYACCOUNTTYPENAME.Value = WW_ROW("PAYACCOUNTTYPENAME")               '預金種別
                P_PAYACCOUNTTYPE.Value = WW_ROW("PAYACCOUNTTYPE")               '預金種別コード
                P_PAYACCOUNT.Value = WW_ROW("PAYACCOUNT")               '口座番号
                P_PAYACCOUNTNAME.Value = WW_ROW("PAYACCOUNTNAME")               '口座名義
                P_PAYORBANKCODE.Value = WW_ROW("PAYORBANKCODE")               '支払元銀行コード
                P_PAYTAXCALCUNIT.Value = WW_ROW("PAYTAXCALCUNIT")               '消費税計算処理区分
                'P_LINKSTATUS.Value = WW_ROW("LINKSTATUS")               '連携状態区分
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0072_PAYEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0072_PAYEE SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNT0072_PAYEE LNT0072")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @TORICODE AS TORICODE ")
        SQLStr.AppendLine("             ,@CLIENTCODE AS CLIENTCODE ")
        SQLStr.AppendLine("             ,@INVOICENUMBER AS INVOICENUMBER ")
        SQLStr.AppendLine("             ,@CLIENTNAME AS CLIENTNAME ")
        SQLStr.AppendLine("             ,@TORINAME AS TORINAME ")
        SQLStr.AppendLine("             ,@TORIDIVNAME AS TORIDIVNAME ")
        SQLStr.AppendLine("             ,@PAYBANKCODE AS PAYBANKCODE ")
        SQLStr.AppendLine("             ,@PAYBANKNAME AS PAYBANKNAME ")
        SQLStr.AppendLine("             ,@PAYBANKNAMEKANA AS PAYBANKNAMEKANA ")
        SQLStr.AppendLine("             ,@PAYBANKBRANCHCODE AS PAYBANKBRANCHCODE ")
        SQLStr.AppendLine("             ,@PAYBANKBRANCHNAME AS PAYBANKBRANCHNAME ")
        SQLStr.AppendLine("             ,@PAYBANKBRANCHNAMEKANA AS PAYBANKBRANCHNAMEKANA ")
        SQLStr.AppendLine("             ,@PAYACCOUNTTYPENAME AS PAYACCOUNTTYPENAME ")
        SQLStr.AppendLine("             ,@PAYACCOUNTTYPE AS PAYACCOUNTTYPE ")
        SQLStr.AppendLine("             ,@PAYACCOUNT AS PAYACCOUNT ")
        SQLStr.AppendLine("             ,@PAYACCOUNTNAME AS PAYACCOUNTNAME ")
        SQLStr.AppendLine("             ,@PAYORBANKCODE AS PAYORBANKCODE ")
        SQLStr.AppendLine("             ,@PAYTAXCALCUNIT AS PAYTAXCALCUNIT ")
        SQLStr.AppendLine("             ,@LINKSTATUS AS LINKSTATUS ")
        'SQLStr.AppendLine("             ,@LASTLINKYMD AS LASTLINKYMD ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNT0072.TORICODE = EXCEL.TORICODE ")
        SQLStr.AppendLine("         AND LNT0072.CLIENTCODE = EXCEL.CLIENTCODE ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNT0072.INVOICENUMBER =  EXCEL.INVOICENUMBER")
        SQLStr.AppendLine("         ,LNT0072.CLIENTNAME =  EXCEL.CLIENTNAME")
        SQLStr.AppendLine("         ,LNT0072.TORINAME =  EXCEL.TORINAME")
        SQLStr.AppendLine("         ,LNT0072.TORIDIVNAME =  EXCEL.TORIDIVNAME")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKCODE =  EXCEL.PAYBANKCODE")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKNAME =  EXCEL.PAYBANKNAME")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKNAMEKANA =  EXCEL.PAYBANKNAMEKANA")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKBRANCHCODE =  EXCEL.PAYBANKBRANCHCODE")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKBRANCHNAME =  EXCEL.PAYBANKBRANCHNAME")
        SQLStr.AppendLine("         ,LNT0072.PAYBANKBRANCHNAMEKANA =  EXCEL.PAYBANKBRANCHNAMEKANA")
        SQLStr.AppendLine("         ,LNT0072.PAYACCOUNTTYPENAME =  EXCEL.PAYACCOUNTTYPENAME")
        SQLStr.AppendLine("         ,LNT0072.PAYACCOUNTTYPE =  EXCEL.PAYACCOUNTTYPE")
        SQLStr.AppendLine("         ,LNT0072.PAYACCOUNT =  EXCEL.PAYACCOUNT")
        SQLStr.AppendLine("         ,LNT0072.PAYACCOUNTNAME =  EXCEL.PAYACCOUNTNAME")
        SQLStr.AppendLine("         ,LNT0072.PAYORBANKCODE =  EXCEL.PAYORBANKCODE")
        SQLStr.AppendLine("         ,LNT0072.PAYTAXCALCUNIT =  EXCEL.PAYTAXCALCUNIT")
        SQLStr.AppendLine("         ,LNT0072.LINKSTATUS =  EXCEL.LINKSTATUS")
        'SQLStr.AppendLine("         ,LNT0072.LASTLINKYMD =  EXCEL.LASTLINKYMD")
        SQLStr.AppendLine("         ,LNT0072.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNT0072.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNT0072.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNT0072.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNT0072.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @TORICODE  ")
        SQLStr.AppendLine("        ,@CLIENTCODE  ")
        SQLStr.AppendLine("        ,@INVOICENUMBER  ")
        SQLStr.AppendLine("        ,@CLIENTNAME  ")
        SQLStr.AppendLine("        ,@TORINAME  ")
        SQLStr.AppendLine("        ,@TORIDIVNAME  ")
        SQLStr.AppendLine("        ,@PAYBANKCODE  ")
        SQLStr.AppendLine("        ,@PAYBANKNAME  ")
        SQLStr.AppendLine("        ,@PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,@PAYACCOUNT  ")
        SQLStr.AppendLine("        ,@PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,@PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,@PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,@LINKSTATUS  ")
        SQLStr.AppendLine("        ,@LASTLINKYMD  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード
                Dim P_INVOICENUMBER As MySqlParameter = SQLcmd.Parameters.Add("@INVOICENUMBER", MySqlDbType.VarChar, 14)         'インボイス登録番号
                Dim P_CLIENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTNAME", MySqlDbType.VarChar, 32)         '顧客名
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 32)         '会社名
                Dim P_TORIDIVNAME As MySqlParameter = SQLcmd.Parameters.Add("@TORIDIVNAME", MySqlDbType.VarChar, 32)         '部門名
                Dim P_PAYBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKCODE", MySqlDbType.VarChar, 4)         '振込先銀行コード
                Dim P_PAYBANKNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAME", MySqlDbType.VarChar, 30)         '振込先銀行名
                Dim P_PAYBANKNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAMEKANA", MySqlDbType.VarChar, 30)         '振込先銀行名カナ
                Dim P_PAYBANKBRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHCODE", MySqlDbType.VarChar, 3)         '振込先支店コード
                Dim P_PAYBANKBRANCHNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAME", MySqlDbType.VarChar, 30)         '振込先支店名
                Dim P_PAYBANKBRANCHNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAMEKANA", MySqlDbType.VarChar, 30)         '振込先支店名カナ
                Dim P_PAYACCOUNTTYPENAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPENAME", MySqlDbType.VarChar, 10)         '預金種別
                Dim P_PAYACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPE", MySqlDbType.VarChar, 1)         '預金種別コード
                Dim P_PAYACCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNT", MySqlDbType.VarChar, 8)         '口座番号
                Dim P_PAYACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNAME", MySqlDbType.VarChar, 30)         '口座名義
                Dim P_PAYORBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYORBANKCODE", MySqlDbType.VarChar, 4)         '支払元銀行コード
                Dim P_PAYTAXCALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@PAYTAXCALCUNIT", MySqlDbType.VarChar, 10)         '消費税計算処理区分
                Dim P_LINKSTATUS As MySqlParameter = SQLcmd.Parameters.Add("@LINKSTATUS", MySqlDbType.VarChar, 1)         '連携状態区分
                Dim P_LASTLINKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LASTLINKYMD", MySqlDbType.DateTime)         '最終連携日
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
                P_TORICODE.Value = WW_ROW("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = WW_ROW("CLIENTCODE")               '顧客コード
                P_INVOICENUMBER.Value = WW_ROW("INVOICENUMBER")               'インボイス登録番号
                P_CLIENTNAME.Value = WW_ROW("CLIENTNAME")               '顧客名
                P_TORINAME.Value = WW_ROW("TORINAME")               '会社名
                P_TORIDIVNAME.Value = WW_ROW("TORIDIVNAME")               '部門名
                P_PAYBANKCODE.Value = WW_ROW("PAYBANKCODE")               '振込先銀行コード
                P_PAYBANKNAME.Value = WW_ROW("PAYBANKNAME")               '振込先銀行名
                P_PAYBANKNAMEKANA.Value = WW_ROW("PAYBANKNAMEKANA")               '振込先銀行名カナ
                P_PAYBANKBRANCHCODE.Value = WW_ROW("PAYBANKBRANCHCODE")               '振込先支店コード
                P_PAYBANKBRANCHNAME.Value = WW_ROW("PAYBANKBRANCHNAME")               '振込先支店名
                P_PAYBANKBRANCHNAMEKANA.Value = WW_ROW("PAYBANKBRANCHNAMEKANA")               '振込先支店名カナ
                P_PAYACCOUNTTYPENAME.Value = WW_ROW("PAYACCOUNTTYPENAME")               '預金種別
                P_PAYACCOUNTTYPE.Value = WW_ROW("PAYACCOUNTTYPE")               '預金種別コード
                P_PAYACCOUNT.Value = WW_ROW("PAYACCOUNT")               '口座番号
                P_PAYACCOUNTNAME.Value = WW_ROW("PAYACCOUNTNAME")               '口座名義
                P_PAYORBANKCODE.Value = WW_ROW("PAYORBANKCODE")               '支払元銀行コード
                P_PAYTAXCALCUNIT.Value = WW_ROW("PAYTAXCALCUNIT")               '消費税計算処理区分
                'P_LINKSTATUS.Value = WW_ROW("LINKSTATUS")               '連携状態区分
                P_LINKSTATUS.Value = "0"
                P_LASTLINKYMD.Value = DBNull.Value               '最終連携日
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0072_PAYEE  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0072_PAYEE  INSERTUPDATE"
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
        ' 支払先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・支払先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 顧客コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CLIENTCODE", WW_ROW("CLIENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・顧客コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' インボイス登録番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INVOICENUMBER", WW_ROW("INVOICENUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・インボイス登録番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 顧客名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CLIENTNAME", WW_ROW("CLIENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・顧客名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 会社名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・会社名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORIDIVNAME", WW_ROW("TORIDIVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先銀行コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKCODE", WW_ROW("PAYBANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先銀行コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先銀行名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKNAME", WW_ROW("PAYBANKNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先銀行名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先銀行名カナ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKNAMEKANA", WW_ROW("PAYBANKNAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先銀行名カナエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先支店コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHCODE", WW_ROW("PAYBANKBRANCHCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先支店コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先支店名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHNAME", WW_ROW("PAYBANKBRANCHNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先支店名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 振込先支店名カナ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHNAMEKANA", WW_ROW("PAYBANKBRANCHNAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・振込先支店名カナエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 預金種別(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPENAME", WW_ROW("PAYACCOUNTTYPENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・預金種別エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 預金種別コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPE", WW_ROW("PAYACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・預金種別コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 口座番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYACCOUNT", WW_ROW("PAYACCOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・口座番号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 口座名義(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYACCOUNTNAME", WW_ROW("PAYACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・口座名義エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 支払元銀行コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYORBANKCODE", WW_ROW("PAYORBANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・支払元銀行コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 消費税計算処理区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PAYTAXCALCUNIT", WW_ROW("PAYTAXCALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・消費税計算処理区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '' 連携状態区分(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "LINKSTATUS", WW_ROW("LINKSTATUS"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・連携状態区分エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
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

        '支払先マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード

                P_TORICODE.Value = WW_ROW("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = WW_ROW("CLIENTCODE")               '顧客コード

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
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0072_PAYEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0072_PAYEE Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0138_PAYEEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
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
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
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
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_TORICODE.Value = WW_ROW("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = WW_ROW("CLIENTCODE")               '顧客コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0138_PAYEEHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0138_PAYEEHIST  INSERT"
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
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

#Region "連携処理"
    ''' <summary>
    ''' 送信ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LinkButton_Click()

        Dim WW_LineCNT As Integer = 0

        '○ LINECNT取得
        Try
            WW_LineCNT = CLng(WF_SelectedIndex.Value) - 1
        Catch ex As Exception
            Exit Sub
        End Try

        '2024/08/02 星ADD START
        '送信チェックがされていない場合は終了
        If LNT0023tbl.Rows(WW_LineCNT)("LINKCHK") = CONST_CHKLINK Then
            Master.Output(C_MESSAGE_NO.CTN_ERR_PAYEELINK_CHK, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If
        '2024/08/02 星ADD END

        Dim csvData As New DataTable
        Dim OTFileName As String = "LNT0023PayeeLink.csv"
        Dim OTFileName2 As String = "LNT0023PayeeLink" + DateTime.Now.ToString("yyyyMMddHHmmss") & ".CSV"
        Dim OTFilePath As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")

        Dim WW_TORICODE As String = ""
        Dim WW_CLIENTCODE As String = ""
        Dim DATENOW As DateTime = Date.Now

        Try
            'CSV出力処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'CSV用データテーブル作成
                csvData = WW_GetCsvData(SQLcon, WW_LineCNT)
                If csvData.Rows.Count = 0 Then
                    Exit Sub
                End If

                ''******************************
                ''CSV作成処理の実行
                ''******************************
                Using repCbj = New CsvCreate(csvData,
                                         I_FolderPath:=OTFilePath,
                                         I_FileName:=OTFileName,
                                         I_PlacedFileName:=OTFileName2)
                    Dim url As String
                    Try
                        url = repCbj.ConvertDataTableToCsv(True, blnSeparate:=True)

                    Catch ex As Exception
                        '異常終了メッセージ
                        Master.Output(C_MESSAGE_NO.CTN_ALIGNMENT_ERROR, C_MESSAGE_TYPE.ERR, "LNT0023L_Renkei", needsPopUp:=True)
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url

                    'フォルダーパス取得
                    Dim UploadRootPath As String = CS0050SESSION.UPLOAD_PATH & "\" &
                                               "PRINTWORK" & "\" &
                                               CS0050SESSION.USERID & "\" &
                                               OTFileName

                    'INI設定取得
                    Dim apibaseUrl As String = ""
                    Dim apiaccount As String = ""
                    Dim apitoken As String = ""
                    Dim webapiflg As String = ""
                    Dim environmentflg As String = ""

                    GetINI(apibaseUrl, apiaccount, apitoken, webapiflg, environmentflg, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '楽々WEBAPI連携
                    If webapiflg <> "0" Then
                        WF_RRRenkei(UploadRootPath, apibaseUrl, apiaccount, apitoken, environmentflg, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If

                        '連携済みに更新、変更履歴に登録する
                        WW_TORICODE = LNT0023tbl.Rows(WW_LineCNT)("TORICODE")
                        WW_CLIENTCODE = LNT0023tbl.Rows(WW_LineCNT)("CLIENTCODE")

                        InsLINKHIST(SQLcon, WW_TORICODE, WW_CLIENTCODE, LNT0023WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ERR_SW)
                        If WW_ERR_SW = "ERR" Then
                            WF_RightboxOpen.Value = "Open"
                            Exit Sub
                        End If

                        UpdLINKSTATUS(SQLcon, WW_TORICODE, WW_CLIENTCODE, DATENOW, WW_ERR_SW)
                        If WW_ERR_SW = "ERR" Then
                            WF_RightboxOpen.Value = "Open"
                            Exit Sub
                        End If

                        InsLINKHIST(SQLcon, WW_TORICODE, WW_CLIENTCODE, LNT0023WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ERR_SW)
                        If WW_ERR_SW = "ERR" Then
                            WF_RightboxOpen.Value = "Open"
                            Exit Sub
                        End If

                        'チェックを外す
                        LNT0023tbl.Rows(WW_LineCNT)("LINKCHK") = CONST_CHKLINK
                        LNT0023tbl.Rows(WW_LineCNT)("LINKSTATUS") = "1"
                        LNT0023tbl.Rows(WW_LineCNT)("LINKSTATUSNM") = "連携済"
                        LNT0023tbl.Rows(WW_LineCNT)("LASTLINKYMD") = DATENOW.ToString("yyyy/MM/dd HH:mm:ss")

                        Master.SaveTable(LNT0023tbl)
                    End If
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_ALIGNMENT_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023L_Renkei", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023L Renkei"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力

            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 連携用データ取得
    ''' </summary>
    Public Function WW_GetCsvData(ByVal SQLcon As MySqlConnection, ByVal WW_LineCNT As Integer) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    '" + LNT0023tbl.Rows(WW_LineCNT)("TORICODE") + "' AS 外部コード")                             '外部コード
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("CLIENTCODE") + "' AS 顧客コード")                         '顧客コード
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("INVOICENUMBER") + "' AS インボイス登録番号")              'インボイス登録番号
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("CLIENTNAME") + "' AS 顧客名")                             '顧客名
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKCODE") + "' AS 振込先銀行コード")                  '振込先銀行コード
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKNAME") + "' AS 振込先銀行名")                      '振込先銀行名
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKNAMEKANA") + "' AS 振込先銀行名カナ")              '振込先銀行名カナ
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHCODE") + "' AS 振込先支店コード")            '振込先支店コード
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHNAME") + "' AS 振込先支店名")                '振込先支店名
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYBANKBRANCHNAMEKANA") + "' AS 振込先支店名カナ")        '振込先支店名カナ
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTTYPENAME") + "' AS 預金種別")                   '預金種別
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTTYPE") + "' AS 預金種別コード")                 '預金種別コード
        SQLBldr.AppendLine("    , '" + Right("0000000" + LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNT"), 7) + "' AS 口座番号")     '口座番号
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYACCOUNTNAME") + "' AS 口座名義")                       '口座名義
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYORBANKCODE") + "' AS 支払元銀行コード")                '支払元銀行コード
        SQLBldr.AppendLine("    , '" + LNT0023tbl.Rows(WW_LineCNT)("PAYTAXCALCUNIT") + "' AS 消費税計算処理区分")             '消費税計算処理区分

        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next
                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using
        End Using
        '取得データ返却
        Return dt
    End Function

    ''' <summary>
    ''' 連携状態更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdLINKSTATUS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_TORICODE As String,
                               ByVal WW_CLIENTCODE As String,
                               ByVal WW_NOW As Date,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    UPDATE LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("        SET LINKSTATUS = '1'")
        SQLStr.AppendLine("           ,LASTLINKYMD =  @LASTLINKYMD")
        SQLStr.AppendLine("           ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("           ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("           ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("           ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '外部コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード
                Dim P_LASTLINKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LASTLINKYMD", MySqlDbType.DateTime)         '最終連携日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_TORICODE               '外部コード
                P_CLIENTCODE.Value = WW_CLIENTCODE               '顧客コード
                P_LASTLINKYMD.Value = WW_NOW                '最終連携日
                P_UPDYMD.Value = WW_NOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0072_PAYEE UPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0072_PAYEE UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 履歴登録(連携状態)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsLINKHIST(ByVal SQLcon As MySqlConnection,
                               ByVal WW_TORICODE As String,
                               ByVal WW_CLIENTCODE As String,
                               ByVal WW_MODIFYKBN As String,
                               ByVal WW_NOW As Date,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0138_PAYEEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
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
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  AS LINKSTATUS")
        SQLStr.AppendLine("        ,LASTLINKYMD  AS LASTLINKYMD")
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
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_TORICODE.Value = WW_TORICODE               '支払先コード
                P_CLIENTCODE.Value = WW_CLIENTCODE               '顧客コード

                P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0138_PAYEEHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0138_PAYEEHIST  INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 楽々WEBAPI連携
    ''' </summary>
    Protected Sub WF_RRRenkei(ByVal filePath As String,
                              ByVal apibaseUrl As String,
                              ByVal apiaccount As String,
                              ByVal apitoken As String,
                              ByVal environmentflg As String,
                              ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        Try
            '楽々請求API実行
            'Dim CS0050SESSION As New CS0050SESSION
            'Dim apibaseUrl = CS0050SESSION.WEBAPIURL
            'Dim apiaccount = CS0050SESSION.WEBAPIACCOUNT
            'Dim apitoken = CS0050SESSION.WEBAPITOKEN

            'WebAPI実行クラスの宣言
            Dim rakurakuApi As New CS0053WebApi(apibaseUrl, apiaccount, apitoken)
            'アップロードAPIの実行
            Dim retVal = rakurakuApi.Upload(filePath)
            If retVal.Trim = "" Then 'ここにきて空白はありえないが念の為
                Throw New Exception("FileUploadApiに失敗しました。（FileId返却無し）")
                O_RTN = "ERR"
            End If

            'アップロードしたAPIを実行
            Dim wk_schema As String = ""    'スキーマID
            Dim wk_import As String = ""    'インポートID

            If environmentflg = "1" Then
                wk_schema = CONST_SCHEMA_KEN
                wk_import = CONST_IMPORT_KEN
            ElseIf environmentflg = "2" Then
                wk_schema = CONST_SCHEMA_HON
                wk_import = CONST_IMPORT_HON
            End If

            'アップロードしたAPIを実行
            Dim retApiSend = rakurakuApi.CsvImport(retVal, wk_schema, wk_import)
            'ここまでくれば正常扱い
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF, messageBoxTitle:="支払先マスタ", needsPopUp:=True)
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023L WF_RRRenkei", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023L WF_RRRenkei"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            O_RTN = "ERR"
            Exit Sub
        End Try
    End Sub


    Protected Sub GetINI(ByRef INI_WEBAPIURL As String,
                         ByRef INI_WEBAPIACCOUNT As String,
                         ByRef INI_WEBAPITOKENSYSTEM As String,
                         ByRef INI_WEBAPIFLG As String,
                         ByRef INI_ENVIRONMENTFLG As String,
                         ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        Dim IniString As String = ""
        Dim IniType As Integer = STRINGTYPE.NONE
        Dim IniBuf As String = ""
        Dim IniRef As Integer = 0

        Dim INIFILE As String = ""
        'WebConfigのAPPStrringsに指定したパス優先
        If ConfigurationManager.AppSettings.AllKeys.Contains("InifilePath") AndAlso
           ConfigurationManager.AppSettings("InifilePath") <> "" Then
            'WebConfigの設定が存在したら
            'ファイルの存在有無に関わらず最優先
            INIFILE = ConfigurationManager.AppSettings("InifilePath")
            If IO.File.Exists(INIFILE) = False Then
                '存在しない場合
                O_RTN = "ERR"
                Master.Output(C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, "WebConfigに定義したIniファイルが存在しません")
                Exit Sub
            End If
        Else
            'WebConfigの設定が存在しない場合は
            '固定パスCとDを
            INIFILE = IniFileC
            If Not File.Exists(IniFileC) Then INIFILE = IniFileD
        End If

        Using sr As StreamReader = New StreamReader(INIFILE, Encoding.UTF8)
            Try

                'ファイル内容の文字情報を全て読み込む
                While (Not sr.EndOfStream)
                    IniBuf = sr.ReadLine().Replace(vbTab, "")

                    '文字列のコメント除去
                    If InStr(IniBuf, "'") >= 1 Then
                        IniRef = InStr(IniBuf, "'") - 1
                    Else
                        IniRef = Len(IniBuf)
                    End If
                    IniBuf = Mid(IniBuf, 1, IniRef)

                    '####楽々API関連情報の取得↓#####
                    'URL
                    If IniBuf.IndexOf("<webapi url>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_URL Then
                        IniType = STRINGTYPE.WEBAPI_URL
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi url>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi url>", "")
                            IniString = IniString.Replace("</webapi url>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            INI_WEBAPIURL = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    'アカウント
                    If IniBuf.IndexOf("<webapi account>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_ACCOUNT Then
                        IniType = STRINGTYPE.WEBAPI_ACCOUNT
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi account>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi account>", "")
                            IniString = IniString.Replace("</webapi account>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            INI_WEBAPIACCOUNT = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    'システム部用トークン
                    If IniBuf.IndexOf("<webapi tokenSystem>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_TOKEN_SYS Then
                        IniType = STRINGTYPE.WEBAPI_TOKEN_SYS
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi tokenSystem>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi tokenSystem>", "")
                            IniString = IniString.Replace("</webapi tokenSystem>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            INI_WEBAPITOKENSYSTEM = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    '連携実行フラグ
                    If IniBuf.IndexOf("<webapi renkeiflg>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_RENKEIFLG Then
                        IniType = STRINGTYPE.WEBAPI_RENKEIFLG
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi renkeiflg>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi renkeiflg>", "")
                            IniString = IniString.Replace("</webapi renkeiflg>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            INI_WEBAPIFLG = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    '####楽々API関連情報の取得↑#####

                    '環境判定用
                    If IniBuf.IndexOf("<environment flg>") >= 0 OrElse IniType = STRINGTYPE.ENVIRONMENT Then
                        IniType = STRINGTYPE.ENVIRONMENT
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</environment flg>") >= 0 Then
                            IniString = IniString.Replace("<environment flg>", "")
                            IniString = IniString.Replace("</environment flg>", "")
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            INI_ENVIRONMENTFLG = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                End While
            Catch ex As Exception
                O_RTN = "ERR"
                Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "Iniファイルの読込に失敗しました")
                Exit Sub
            End Try
        End Using
    End Sub
#End Region




End Class

