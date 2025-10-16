''************************************************************
' 実勢単価履歴画面
' 作成日 2025/08/16
' 更新日 
' 作成者 三宅
' 更新者 
'
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' 実勢単価履歴登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNT0031DieselPriceHist
    Inherits Page

    '○ 検索結果格納Table
    Private LNT0031tbl As DataTable         '一覧格納用テーブル
    Private LNT0031INPtbl As DataTable      '入力格納用テーブル
    Private LNT0031UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNT0031Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    Private Const CONST_BTNLOCK As String = "<div><img  id=""btnLock{0}"" type=""image"" src=""../img/lockkey.png"" width=""20px"" height=""20px"" /></div>"
    Private Const CONST_BTNUNLOCK As String = "<div><img  id=""btnLock{0}"" type=""image"" src=""../img/unlockkey.png"" width=""20px"" height=""20px"" /></div>"
    Private Const CONST_BTNDEL As String = "<div><input class=""btn-sticky"" id=""btnDel{0}""　type=""button"" value=""削除"" readonly /></div>"

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
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新

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
                    Master.RecoverTable(LNT0031tbl)
                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0031tbl, pnlListArea) Then
                        Master.SaveTable(LNT0031tbl)
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '行追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          '変更ボタン押下
                            WF_UPDATE_Click(WW_ErrSW)
                            If WW_ErrSW = C_MESSAGE_NO.NORMAL Then
                                GridViewInitialize()
                            Else
                                DisplayGrid()
                            End If
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNT0031WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNT0031WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND", "LNM0019L" '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                            GridViewInitialize()
                        Case "WF_ButtonLockClick"       'ロックボタン押下（確認ポップアップ表示）
                            WF_ButtonFLGCTRL_Click("LOCK")
                        Case "WF_ButtonUnLockClick"     'アンロックボタン押下（確認ポップアップ表示）
                            WF_ButtonFLGCTRL_Click("UNLOCK")
                        Case "WF_ButtonDelClick"        '削除ボタン押下（確認ポップアップ表示）
                            WF_ButtonFLGCTRL_Click("DEL")
                        Case "btnCommonConfirmOk"       'ロック、アンロック、削除ポップアップ（はい）押下時のＤＢ更新処理
                            WF_ButtonFLGUPDATE_Click()
                            GridViewInitialize()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" AndAlso
                       Not WF_ButtonClick.Value = "WF_ButtonUPDATE" AndAlso
                       Not WF_ButtonClick.Value = "btnCommonConfirmOk" Then
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
            If Not IsNothing(LNT0031tbl) Then
                LNT0031tbl.Clear()
                LNT0031tbl.Dispose()
                LNT0031tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0031WRKINC.MAPIDL
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()
        '○ 入力情報（INPtbl）保存先のファイル名
        WW_CreateXMLSaveFile()

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

        If String.IsNullOrEmpty(Master.VIEWID) Then
            rightview2.MAPIDS = Master.MAPID
            rightview2.MAPID = Master.MAPID
            rightview2.COMPCODE = Master.USERCAMP
            rightview2.MAPVARI = Master.MAPvariant
            rightview2.PROFID = Master.PROF_VIEW
            rightview2.MENUROLE = Master.ROLE_MENU
            rightview2.MAPROLE = Master.ROLE_MAP
            rightview2.VIEWROLE = Master.ROLE_VIEWPROF
            rightview2.RPRTROLE = Master.ROLE_RPRTPROF
            rightview2.Initialize("画面レイアウト設定", WW_Dummy)
            Master.VIEWID = rightview2.GetViewId(Master.USERCAMP)
        End If
        Master.CreateXMLSaveFile()

        WF_DIESELPRICESITENAME.Text = work.WF_SEL_DIESELPRICESITENAME.Text
        WF_DIESELPRICESITEKBNNAME.Text = work.WF_SEL_DIESELPRICESITEKBNNAME.Text

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
        Master.SaveTable(LNT0031tbl)
        '○ 初期データ保存
        Master.SaveTable(LNT0031tbl, work.WF_SEL_INPTBL.Text)

        '〇 一覧ヘッダを設定
        Me.ListCount.Text = "件数：" + LNT0031tbl.Rows.Count.ToString()
        WF_DIESELPRICESITENAME.Text = work.WF_SEL_DIESELPRICESITENAME.Text
        WF_DIESELPRICESITEKBNNAME.Text = work.WF_SEL_DIESELPRICESITEKBNNAME.Text

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0031tbl)
        Dim WW_RowFilterCMD As New StringBuilder
        WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        TBLview.RowFilter = WW_RowFilterCMD.ToString

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
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

        If IsNothing(LNT0031tbl) Then
            LNT0031tbl = New DataTable
        End If

        If LNT0031tbl.Columns.Count <> 0 Then
            LNT0031tbl.Columns.Clear()
        End If

        LNT0031tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを実勢単価履歴から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                      AS 'SELECT'              ")
        SQLStr.AppendLine("   , 0                                                                      AS HIDDEN                ")
        SQLStr.AppendLine("   , 0                                                                      AS LINECNT               ")
        SQLStr.AppendLine("   , ''                                                                     AS OPERATION             ")
        SQLStr.AppendLine("   , LNT0031.UPDTIMSTP                                                      AS UPDTIMSTP             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.DIESELPRICESITEID), '')                         AS DIESELPRICESITEID     ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DIESELPRICESITENAME), '')                       AS DIESELPRICESITENAME   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.DIESELPRICESITEBRANCH), '')                     AS DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DIESELPRICESITEKBNNAME), '')                    AS DIESELPRICESITEKBNNAME")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNM0020.DISPLAYNAME), '')                               AS DISPLAYNAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.TARGETYEAR), '')                                AS TARGETYEAR            ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE1, 0)                                      AS DIESELPRICE1          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE2, 0)                                      AS DIESELPRICE2          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE3, 0)                                      AS DIESELPRICE3          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE4, 0)                                      AS DIESELPRICE4          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE5, 0)                                      AS DIESELPRICE5          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE6, 0)                                      AS DIESELPRICE6          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE7, 0)                                      AS DIESELPRICE7          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE8, 0)                                      AS DIESELPRICE8          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE9, 0)                                      AS DIESELPRICE9          ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE10, 0)                                     AS DIESELPRICE10         ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE11, 0)                                     AS DIESELPRICE11         ")
        SQLStr.AppendLine("   , COALESCE(LNT0031.DIESELPRICE12, 0)                                     AS DIESELPRICE12         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.LOCKFLG), '')                                   AS LOCKFLG               ")
        SQLStr.AppendLine("   , ''                                                                     AS LOCKFLGBTN            ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(LNT0031.LOCKYMD, '%Y/%m/%d'), '')                 AS LOCKYMD               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.LOCKUSER), '')                                  AS LOCKUSER              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0031.DELFLG), '')                                    AS DELFLG                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNS0006.VALUE1), '')                                    AS DELFLGNAME            ")
        SQLStr.AppendLine("   , '0'                                                                    AS ADDFLG                ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNT0031_DIESELPRICEHIST LNT0031                                                             ")

        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          KEYCODE                                                                                    ")
        SQLStr.AppendLine("         ,VALUE1                                                                                     ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0006_FIXVALUE                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          CAMPCODE = @CAMPCODE                                                                       ")
        SQLStr.AppendLine("      AND CLASS = 'DELFLG'                                                                           ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0006                                                                                        ")
        SQLStr.AppendLine("      ON  LNT0031.DELFLG = LNS0006.KEYCODE                                                           ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          DIESELPRICESITEID                                                                          ")
        SQLStr.AppendLine("         ,DIESELPRICESITENAME                                                                        ")
        SQLStr.AppendLine("         ,DIESELPRICESITEBRANCH                                                                      ")
        SQLStr.AppendLine("         ,DIESELPRICESITEKBNNAME                                                                     ")
        SQLStr.AppendLine("         ,DISPLAYNAME                                                                                ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          LNG.LNM0020_DIESELPRICESITE                                                                ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNM0020                                                                                        ")
        SQLStr.AppendLine("      ON  LNT0031.DIESELPRICESITEID = LNM0020.DIESELPRICESITEID                                      ")
        SQLStr.AppendLine("      AND LNT0031.DIESELPRICESITEBRANCH = LNM0020.DIESELPRICESITEBRANCH                              ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     LNT0031.DIESELPRICESITEID = @DIESELPRICESITEID                                                  ")
        SQLStr.AppendLine(" AND LNT0031.DIESELPRICESITEBRANCH = @DIESELPRICESITEBRANCH                                          ")
        SQLStr.AppendLine(" AND LNT0031.DELFLG = '0'                                                                            ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     LNT0031.TARGETYEAR                                                                              ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                '会社
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                P_CAMPCODE.Value = Master.USERCAMP

                '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)
                P_DIESELPRICESITEID.Value = work.WF_SEL_DIESELPRICESITEID.Text

                '実勢軽油価格参照先ID枝番
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)
                P_DIESELPRICESITEBRANCH.Value = work.WF_SEL_DIESELPRICESITEBRANCH.Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0031tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0031tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0031row As DataRow In LNT0031tbl.Rows
                    i += 1
                    LNT0031row("LINECNT") = i        'LINECNT

                    If LNT0031row("LOCKFLG") = "0" Then
                        LNT0031row("LOCKFLGBTN") = String.Format(CONST_BTNUNLOCK, i)
                        LNT0031row("DELFLGNAME") = String.Format(CONST_BTNDEL, i)
                    Else
                        LNT0031row("LOCKFLGBTN") = String.Format(CONST_BTNLOCK, i)
                        LNT0031row("DELFLGNAME") = ""
                    End If
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST Select"
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
    ''' 行追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '空白行がある場合は、行追加しない（空白行の半手は、対象年の入力有無で判定）
        Dim selRow() = LNT0031tbl.Select("TARGETYEAR=''")
        If selRow.Count > 0 Then
            Exit Sub
        End If

        Dim LNT0031row As DataRow = LNT0031tbl.NewRow

        LNT0031row("SELECT") = "1"
        LNT0031row("HIDDEN") = "0"
        LNT0031row("LINECNT") = LNT0031tbl.Rows.Count + 1
        LNT0031row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        LNT0031row("UPDTIMSTP") = Date.Now
        LNT0031row("DIESELPRICESITEID") = work.WF_SEL_DIESELPRICESITEID.Text
        LNT0031row("DIESELPRICESITENAME") = work.WF_SEL_DIESELPRICESITENAME.Text
        LNT0031row("DIESELPRICESITEBRANCH") = work.WF_SEL_DIESELPRICESITEBRANCH.Text
        LNT0031row("DIESELPRICESITEKBNNAME") = work.WF_SEL_DIESELPRICESITEKBNNAME.Text
        LNT0031row("DISPLAYNAME") = work.WF_SEL_DISPLAYNAME.Text
        LNT0031row("TARGETYEAR") = ""
        LNT0031row("DIESELPRICE1") = "0.00"
        LNT0031row("DIESELPRICE2") = "0.00"
        LNT0031row("DIESELPRICE3") = "0.00"
        LNT0031row("DIESELPRICE4") = "0.00"
        LNT0031row("DIESELPRICE5") = "0.00"
        LNT0031row("DIESELPRICE6") = "0.00"
        LNT0031row("DIESELPRICE7") = "0.00"
        LNT0031row("DIESELPRICE8") = "0.00"
        LNT0031row("DIESELPRICE9") = "0.00"
        LNT0031row("DIESELPRICE10") = "0.00"
        LNT0031row("DIESELPRICE11") = "0.00"
        LNT0031row("DIESELPRICE12") = "0.00"
        LNT0031row("LOCKFLG") = "0"
        LNT0031row("LOCKFLGBTN") = String.Format(CONST_BTNUNLOCK, LNT0031tbl.Rows.Count + 1)
        LNT0031row("LOCKYMD") = DBNull.Value
        LNT0031row("LOCKUSER") = ""
        LNT0031row("DELFLG") = "0"
        LNT0031row("DELFLGNAME") = String.Format(CONST_BTNDEL, LNT0031tbl.Rows.Count + 1)
        LNT0031row("ADDFLG") = "1"

        LNT0031tbl.Rows.Add(LNT0031row)

        '○ 画面表示データ保存
        Master.SaveTable(LNT0031tbl)

    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click(ByRef oRtn As String)

        oRtn = Messages.C_MESSAGE_NO.NORMAL

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNT0031INPtbl(oRtn)
        If Not isNormal(oRtn) Then
            Exit Sub
        End If

        Master.RecoverTable(LNT0031tbl, work.WF_SEL_INPTBL.Text)
        '変更チェック
        '○ 追加変更判定
        For Each LNT0031INProw As DataRow In LNT0031INPtbl.Rows

            ' 既存レコードとの比較
            For Each LNT0031row As DataRow In LNT0031tbl.Rows
                ' KEY項目が等しい時（対象年は、変更される場合があるため判定せず、行番号で判断する。他のIDとID枝番は念のため）
                If LNT0031row("DIESELPRICESITEID") = LNT0031INProw("DIESELPRICESITEID") AndAlso                         '実勢軽油価格参照先ID
                    LNT0031row("DIESELPRICESITEBRANCH") = LNT0031INProw("DIESELPRICESITEBRANCH") AndAlso                '実勢軽油価格参照先ID枝番
                    LNT0031row("LINECNT") = LNT0031INProw("LINECNT") Then                                               '行番号
                    ' KEY項目以外の項目の差異をチェック
                    If LNT0031row("OPERATION") = LNT0031INProw("OPERATION") AndAlso
                        LNT0031row("TARGETYEAR") = LNT0031INProw("TARGETYEAR") AndAlso                                  '対象年
                        LNT0031row("DIESELPRICE1") = LNT0031INProw("DIESELPRICE1") AndAlso                              '1月実勢単価
                        LNT0031row("DIESELPRICE2") = LNT0031INProw("DIESELPRICE2") AndAlso                              '2月実勢単価
                        LNT0031row("DIESELPRICE3") = LNT0031INProw("DIESELPRICE3") AndAlso                              '3月実勢単価
                        LNT0031row("DIESELPRICE4") = LNT0031INProw("DIESELPRICE4") AndAlso                              '4月実勢単価
                        LNT0031row("DIESELPRICE5") = LNT0031INProw("DIESELPRICE5") AndAlso                              '5月実勢単価
                        LNT0031row("DIESELPRICE6") = LNT0031INProw("DIESELPRICE6") AndAlso                              '6月実勢単価
                        LNT0031row("DIESELPRICE7") = LNT0031INProw("DIESELPRICE7") AndAlso                              '7月実勢単価
                        LNT0031row("DIESELPRICE8") = LNT0031INProw("DIESELPRICE8") AndAlso                              '8月実勢単価
                        LNT0031row("DIESELPRICE9") = LNT0031INProw("DIESELPRICE9") AndAlso                              '9月実勢単価
                        LNT0031row("DIESELPRICE10") = LNT0031INProw("DIESELPRICE10") AndAlso                            '10月実勢単価
                        LNT0031row("DIESELPRICE11") = LNT0031INProw("DIESELPRICE11") AndAlso                            '11月実勢単価
                        LNT0031row("DIESELPRICE12") = LNT0031INProw("DIESELPRICE12") AndAlso                            '12月実勢単価
                        LNT0031row("DELFLG") = LNT0031INProw("DELFLG") AndAlso                                          '削除フラグ
                        LNT0031row("LOCKFLG") = LNT0031INProw("LOCKFLG") Then                                           'ロックフラグ

                        If String.IsNullOrEmpty(LNT0031INProw("OPERATION")) Then
                            ' 変更がある時は「操作」の項目を「更新」に設定する
                            LNT0031INProw("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                        Else
                            ' 変更がない時は「操作」の項目は空白にする
                            LNT0031INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        End If
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNT0031INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If

                    Exit For
                End If
            Next
        Next

        ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
        Dim selStr As String = String.Format("OPERATION<>'{0}'", C_LIST_OPERATION_CODE.NODATA)
        Dim selRow() = LNT0031INPtbl.Select(selStr)
        If selRow.Count = 0 Then
            Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
            oRtn = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        End If

        '------------------------------------------
        'テーブル更新処理
        '------------------------------------------
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続

            TblUpdate(SQLcon, LNT0031INPtbl, oRtn)
        End Using

        '更新結果を画面表示テーブルに反映
        Dim findFlg As Boolean = False
        For Each LNT0031INProw As DataRow In LNT0031INPtbl.Rows
            For Each LNT0031row As DataRow In LNT0031tbl.Rows
                ' KEY項目が等しい時、画面内容に入れ替える
                If LNT0031row("DIESELPRICESITEID") = LNT0031INProw("DIESELPRICESITEID") AndAlso                         '実勢軽油価格参照先ID
                    LNT0031row("DIESELPRICESITEBRANCH") = LNT0031INProw("DIESELPRICESITEBRANCH") AndAlso                '実勢軽油価格参照先ID枝番
                    LNT0031row("LINECNT") = LNT0031INProw("LINECNT") Then                                               '行番号
                    LNT0031row.ItemArray = LNT0031INProw.ItemArray
                    findFlg = True
                    Exit For
                End If
            Next
            '存在しない場合、新規追加する
            If findFlg = False Then
                Dim LNT0031row As DataRow = LNT0031tbl.NewRow
                LNT0031row.ItemArray = LNT0031INProw.ItemArray
                LNT0031tbl.Rows.Add(LNT0031row)
            End If
            findFlg = False
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0031tbl, work.WF_SEL_INPTBL.Text)

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToLNT0031INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        If IsNothing(LNT0031INPtbl) Then
            LNT0031INPtbl = LNT0031tbl.Clone
        Else
            LNT0031INPtbl.Clear()
        End If

        Dim WW_TEXT As String = ""
        Dim WW_DATATYPE As String = ""
        Dim WW_RESULT As Boolean

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        For i As Integer = 0 To LNT0031tbl.Rows.Count - 1

            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "LOCKFLG" & (i + 1))) AndAlso
               Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOCKFLG" & (i + 1))) = "1" Then
                Continue For
            End If

            Dim LNT0031INProw As DataRow = LNT0031INPtbl.NewRow
            LNT0031INProw.ItemArray = LNT0031tbl.Rows(i).ItemArray

            'LNT0031INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            LNT0031INProw("SELECT") = 1
            LNT0031INProw("HIDDEN") = 0

            LNT0031INProw("DELFLG") = C_DELETE_FLG.ALIVE             '削除フラグ

            '対象年
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TARGETYEAR" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TARGETYEAR" & (i + 1)))
                WW_DATATYPE = LNT0031INProw("TARGETYEAR").GetType.Name.ToString
                LNT0031INProw("TARGETYEAR") = LNT0031WRKINC.DataConvert("対象年", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0031INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0031INProw("TARGETYEAR"))
            Else
                LNT0031INProw("TARGETYEAR") = ""
            End If

            '1月～12月実勢単価
            For j As Integer = 1 To 12
                Dim DispName As String = "txt" & pnlListArea.ID & "DIESELPRICE" & j & (i + 1)
                Dim ColNmae As String = "DIESELPRICE" & j

                WW_TEXT = Convert.ToString(Request.Form(DispName))
                WW_DATATYPE = LNT0031INProw(ColNmae).GetType.Name.ToString
                LNT0031INProw(ColNmae) = LNT0031WRKINC.DataConvert(j & "月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0031INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0031INProw(ColNmae))

            Next

            'ロックフラグ
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOCKFLG" & (i + 1)))
            WW_DATATYPE = LNT0031INProw("LOCKFLG").GetType.Name.ToString
            LNT0031INProw("LOCKFLG") = LNT0031WRKINC.DataConvert("ロックフラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0031INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0031INProw("LOCKFLG"))

            '削除フラグ
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1)))
            WW_DATATYPE = LNT0031INProw("DELFLG").GetType.Name.ToString
            LNT0031INProw("DELFLG") = LNT0031WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0031INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0031INProw("DELFLG"))

            LNT0031INPtbl.Rows.Add(LNT0031INProw)
        Next

    End Sub

    ''' <summary>
    ''' 削除／ロック／アンロックボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFLGCTRL_Click(ByVal iCTRL As String)

        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        Dim Msg1 As String = ""
        Dim Msg2 As String = ""
        Dim MsgType As String = C_MESSAGE_TYPE.INF

        If iCTRL = "LOCK" Then
            Msg1 = LNT0031tbl(WW_LineCNT)("TARGETYEAR") & "年をロックします。よろしいですか"
        End If
        If iCTRL = "UNLOCK" Then
            Msg1 = LNT0031tbl(WW_LineCNT)("TARGETYEAR") & "年をアンロックします。よろしいですか"
        End If
        If iCTRL = "DEL" Then
            Msg1 = LNT0031tbl(WW_LineCNT)("TARGETYEAR") & "年を削除します。よろしいですか"
        End If

        Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, MsgType, Msg1, Msg2, True, "", True)

        'パラメタの保存（"LOCK"、"UNLOCK"、"DEL")
        WF_FLGPARM.Value = iCTRL

    End Sub

    ''' <summary>
    ''' 削除／ロック／アンロックボタン押下時のDB更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFLGUPDATE_Click()
        '○ LINECNT取得
        Dim WW_LineCNT As Integer = 0
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        If WF_FLGPARM.Value = "LOCK" Then
            LNT0031tbl(WW_LineCNT)("LOCKFLG") = "1"
            LNT0031tbl(WW_LineCNT)("LOCKFLGBTN") = String.Format(CONST_BTNLOCK, WW_LineCNT + 1)
            LNT0031tbl(WW_LineCNT)("LOCKYMD") = Date.Now.ToString("yyyy/MM/dd")
            LNT0031tbl(WW_LineCNT)("LOCKUSER") = Master.USERID
            LNT0031tbl(WW_LineCNT)("DELFLGNAME") = ""
        End If

        If WF_FLGPARM.Value = "UNLOCK" Then
            LNT0031tbl(WW_LineCNT)("LOCKFLG") = "0"
            LNT0031tbl(WW_LineCNT)("LOCKFLGBTN") = String.Format(CONST_BTNUNLOCK, WW_LineCNT + 1)
            LNT0031tbl(WW_LineCNT)("LOCKYMD") = ""
            LNT0031tbl(WW_LineCNT)("LOCKUSER") = ""
            LNT0031tbl(WW_LineCNT)("DELFLGNAME") = String.Format(CONST_BTNDEL, WW_LineCNT + 1)
        End If

        If WF_FLGPARM.Value = "DEL" Then
            LNT0031tbl(WW_LineCNT)("DELFLG") = C_DELETE_FLG.DELETE
            LNT0031tbl(WW_LineCNT)("DELFLGNAME") = ""
        End If

        '-------------------------------------------
        'ＤＢ更新
        '-------------------------------------------
        Dim WW_ROW As DataRow
        WW_ROW = LNT0031tbl.Rows(WW_LineCNT)
        Dim DATENOW As Date = Date.Now
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '削除、ロックフラグだけ更新にする
            ''履歴登録(変更前)
            'InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.ALIVE, LNM0019WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
            'If Not isNormal(WW_ErrSW) Then
            '    Exit Sub
            'End If
            '削除フラグ有効化
            FlgUpdate(SQLcon, WW_ROW, DATENOW, WF_FLGPARM.Value)
            If Not isNormal(WW_ErrSW) Then
                Exit Sub
            End If
            ''履歴登録(変更後)
            'InsertHist(SQLcon, WW_ROW, C_DELETE_FLG.DELETE, LNM0019WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
            'If Not isNormal(WW_ErrSW) Then
            '    Exit Sub
            'End If

            '更新完了メッセージを表示
            'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0031tbl)

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNT0031SurchargePatternHistory.aspx")
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0031row As DataRow In LNT0031tbl.Rows
            If LNT0031row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0031row("SELECT") = WW_DataCNT
            End If
        Next

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNT0031tbl.Rows.Count.ToString()
        WF_DIESELPRICESITENAME.Text = work.WF_SEL_DIESELPRICESITENAME.Text
        WF_DIESELPRICESITEKBNNAME.Text = work.WF_SEL_DIESELPRICESITEKBNNAME.Text

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
        Dim TBLview As DataView = New DataView(LNT0031tbl)

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
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
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

        'Master.TransitionPrevPage()
        Server.Transfer("~/LNG/mas/LNM0019SurchargePatternList.aspx")

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
        Dim TBLview As New DataView(LNT0031tbl)
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
    ''' 削除、ロックフラグの更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    ''' <remarks></remarks>
    Public Sub FlgUpdate(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_NOW As Date, ByVal iCTRL As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNT0031_DIESELPRICEHIST             ")
        SQLStr.Append(" SET                                         ")
        If iCTRL = "DEL" Then
            SQLStr.Append("     DELFLG               = @DELFLG      ")
        Else
            SQLStr.Append("     LOCKFLG              = @LOCKFLG     ")
            SQLStr.Append("    ,LOCKYMD              = @LOCKYMD     ")
            SQLStr.Append("    ,LOCKUSER             = @LOCKUSER    ")
        End If
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.Append("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.Append("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)            '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)    '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                           '対象年
                Dim P_LOCKFLG As MySqlParameter = SQLcmd.Parameters.Add("@LOCKFLG", MySqlDbType.VarChar, 1)                                 'ロックフラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                                   '削除フラグ
                Dim P_LOCKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LOCKYMD", MySqlDbType.DateTime)                                   'ロック実行年月日
                Dim P_LOCKUSER As MySqlParameter = SQLcmd.Parameters.Add("@LOCKUSER", MySqlDbType.VarChar, 20)                              'ロック実行者
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                                '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                            '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                                '更新プログラムＩＤ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年
                P_LOCKFLG.Value = WW_ROW("LOCKFLG")                                 'ロックフラグ
                If WW_ROW("LOCKFLG") = "0" Then
                    P_LOCKYMD.Value = DBNull.Value                      'ロック実行年月日
                    P_LOCKUSER.Value = ""                               'ロック実行者
                Else
                    P_LOCKYMD.Value = Date.Now                          'ロック実行年月日
                    P_LOCKUSER.Value = Master.USERID                    'ロック実行者
                End If
                P_DELFLG.Value = WW_ROW("DELFLG")                                   '削除フラグ

                P_UPDYMD.Value = WW_NOW                              '更新年月日
                P_UPDUSER.Value = Master.USERID                      '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name         '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

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
                Case "ORG"              '組織コード
                    LNT0031WRKINC.getOrgName(I_VALUE, O_TEXT, O_RTN)
                Case "DELFLG", "LOCKFLG"          '削除フラグ、ロックフラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "TORICODE"
                    LNT0031WRKINC.getToriName(I_VALUE, O_TEXT, O_RTN)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
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
        'UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", CS0050SESSION.HTTPS_GET, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNT0031WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0

        WW_STROW = WW_ACTIVEROW
        SetDETAIL(wb, wb.ActiveSheet, WW_ACTIVEROW)
        WW_ENDROW = WW_ACTIVEROW - 1

        'シート全体設定
        SetALL(wb.ActiveSheet)

        'プルダウンリスト作成
        SetPULLDOWNLIST(wb, WW_STROW, WW_ENDROW)

        '入力不可列設定
        SetCOLLOCKED(wb.ActiveSheet, WW_STROW, WW_ENDROW)

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
        wb.ActiveSheet.Range("C1").Value = "実勢単価履歴一覧"
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
            Case LNT0031WRKINC.FILETYPE.EXCEL
                FileName = "実勢単価履歴.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNT0031WRKINC.FILETYPE.PDF
                FileName = "実勢単価履歴.pdf"
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
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '実勢軽油価格参照先ID
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '実勢軽油価格参照先ID枝番
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.TARGETYEAR).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '対象年
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE1).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '1月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE2).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '2月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE3).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '3月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE4).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '4月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE5).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '5月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE6).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '6月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE7).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '7月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE8).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '8月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE9).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '9月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE10).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '10月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE11).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '11月実勢単価
        'sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE12).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '12月実勢単価
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'ロックフラグ
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ

        '入力不要列網掛け
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先名
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '実勢軽油価格参照先区分名
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.LOCKYMD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'ロック実行年月日
        sheet.Columns(LNT0031WRKINC.INOUTEXCELCOL.LOCKUSER).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) 'ロック実行者

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
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Value = "（必須）実勢軽油価格参照先ID"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Value = "実勢軽油価格参照先名"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Value = "（必須）実勢軽油価格参照先ID枝番"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Value = "実勢軽油価格参照先区分名"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.TARGETYEAR).Value = "（必須）対象年"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE1).Value = "1月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE2).Value = "2月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE3).Value = "3月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE4).Value = "4月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE5).Value = "5月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE6).Value = "6月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE7).Value = "7月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE8).Value = "8月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE9).Value = "9月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE10).Value = "10月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE11).Value = "11月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE12).Value = "12月実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG).Value = "（必須）ロックフラグ"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKYMD).Value = "ロック実行年月日"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKUSER).Value = "ロック実行者"
        sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"

        Dim WW_TEXT As String = ""
        Dim WW_TEXTLIST = New StringBuilder
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            'COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
            '        .Width = 50
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            'ロックフラグ
            COMMENT_get(SQLcon, "LOCKFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

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
        'SETFIXVALUELIST(subsheet, "DELFLG", LNT0031WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If

        'ロックフラグ
        SETFIXVALUELIST(subsheet, "LOCKFLG", LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If
        '削除フラグ
        SETFIXVALUELIST(subsheet, "DELFLG", LNT0031WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        'メインシートをアクティブにする
        mainsheet.Activate()
        'サブシートを非表示にする
        subsheet.Visible = Visibility.Hidden
    End Sub

    ''' <summary>
    ''' 入力不可列設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetCOLLOCKED(ByVal sheet As IWorksheet, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'Dim WW_STRANGE As IRange
        'Dim WW_ENDRANGE As IRange

        ''シートの保護をかけるとリボンも操作できなくなるため
        ''データの入力規則で対応(該当セルの入力可能文字数を0にする)

        ''枝番
        'WW_STRANGE = sheet.Cells(WW_STROW, LNT0031WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNT0031WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'With sheet.Range(WW_STRANGE.Address & ":" & WW_ENDRANGE.Address).Validation
        '    .Add(type:=ValidationType.TextLength, validationOperator:=ValidationOperator.LessEqual, formula1:=0)
        'End With

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        '数値書式(整数)
        Dim IntStyle As IStyle = wb.Styles.Add("IntStyle")
        IntStyle.NumberFormat = "#,##0_);[Red](#,##0)"

        '数値書式(小数点含む)
        Dim DecStyle As IStyle = wb.Styles.Add("DecStyle")
        DecStyle.NumberFormat = "#,##0.000_);[Red](#,##0.000)"

        '数値書式(小数点含む)
        Dim DecStyle2 As IStyle = wb.Styles.Add("DecStyle2")
        DecStyle2.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        'Dim WW_DEPSTATION As String

        'Dim WW_DEPSTATIONNM As String

        For Each Row As DataRow In LNT0031tbl.Rows
            'WW_DEPSTATION = Row("DEPSTATION") '発駅コード

            '名称取得
            'CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEID).Value = Row("DIESELPRICESITEID") '実勢軽油価格参照先ID
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME).Value = Row("DIESELPRICESITENAME") '実勢軽油価格参照先名
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH).Value = Row("DIESELPRICESITEBRANCH") '実勢軽油価格参照先ID枝番
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME).Value = Row("DIESELPRICESITEKBNNAME") '実勢軽油価格参照先区分名
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.TARGETYEAR).Value = Row("TARGETYEAR") '対象年
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE1).Value = Row("DIESELPRICE1") '1月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE2).Value = Row("DIESELPRICE2") '2月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE3).Value = Row("DIESELPRICE3") '3月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE4).Value = Row("DIESELPRICE4") '4月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE5).Value = Row("DIESELPRICE5") '5月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE6).Value = Row("DIESELPRICE6") '6月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE7).Value = Row("DIESELPRICE7") '7月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE8).Value = Row("DIESELPRICE8") '8月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE9).Value = Row("DIESELPRICE9") '9月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE10).Value = Row("DIESELPRICE10") '10月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE11).Value = Row("DIESELPRICE11") '11月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE12).Value = Row("DIESELPRICE12") '12月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG).Value = Row("LOCKFLG") 'ロックフラグ
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKYMD).Value = Row("LOCKYMD") 'ロック実行年月日
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKUSER).Value = Row("LOCKUSER") 'ロック実行者
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ

            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE1).Style = DecStyle2 '1月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE2).Style = DecStyle2 '2月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE3).Style = DecStyle2 '3月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE4).Style = DecStyle2 '4月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE5).Style = DecStyle2 '5月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE6).Style = DecStyle2 '6月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE7).Style = DecStyle2 '7月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE8).Style = DecStyle2 '8月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE9).Style = DecStyle2 '9月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE10).Style = DecStyle2 '10月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE11).Style = DecStyle2 '11月実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE12).Style = DecStyle2 '12月実勢単価

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
                Case "DELFLG", "LOCKFLG"   '削除フラグ、ロックフラグ
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
                Case "DELFLG", "LOCKFLG"   '削除フラグ、ロックフラグ
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

    ' ******************************************************************************
    ' ***  更新処理                                                              ***
    ' ******************************************************************************
#Region "ｱｯﾌﾟﾛｰﾄﾞ"
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "実勢単価履歴の更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNT0031Exceltbl) Then
            LNT0031Exceltbl = New DataTable
        End If
        If LNT0031Exceltbl.Columns.Count <> 0 Then
            LNT0031Exceltbl.Columns.Clear()
        End If
        LNT0031Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\TANKAEXCEL"
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
        Dim fileNameHead As String = "TANKAEXCEL_TMP_"

        'ファイルパスの決定
        Dim newfileName As String = fileNameHead & DateTime.Now.ToString("yyyyMMddHHmmss") & "." & fileExtention
        Dim filePath As String = fileUploadPath & "\" & newfileName
        'ファイルの保存
        WF_UPLOAD_BTN.SaveAs(filePath)

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続
            'Excelデータ格納用テーブルに格納する
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("データ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ErrSW)
            If WW_ErrSW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")

            'テーブル更新処理
            TblUpdate(SQLcon, LNT0031Exceltbl, WW_ErrSW)

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
        SQLStr.AppendLine("        ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("        ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("        ,TARGETYEAR  ")
        SQLStr.AppendLine("        ,DIESELPRICE1  ")
        SQLStr.AppendLine("        ,DIESELPRICE2  ")
        SQLStr.AppendLine("        ,DIESELPRICE3  ")
        SQLStr.AppendLine("        ,DIESELPRICE4  ")
        SQLStr.AppendLine("        ,DIESELPRICE5  ")
        SQLStr.AppendLine("        ,DIESELPRICE6  ")
        SQLStr.AppendLine("        ,DIESELPRICE7  ")
        SQLStr.AppendLine("        ,DIESELPRICE8  ")
        SQLStr.AppendLine("        ,DIESELPRICE9  ")
        SQLStr.AppendLine("        ,DIESELPRICE10 ")
        SQLStr.AppendLine("        ,DIESELPRICE11 ")
        SQLStr.AppendLine("        ,DIESELPRICE12 ")
        SQLStr.AppendLine("        ,LOCKFLG  ")
        SQLStr.AppendLine("        ,LOCKYMD  ")
        SQLStr.AppendLine("        ,LOCKUSER  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" ,'0' AS ADDFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNT0031_DIESELPRICEHIST ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0031Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        Try
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

            Dim LNT0031Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNT0031Exceltblrow = LNT0031Exceltbl.NewRow

                'LINECNT
                LNT0031Exceltblrow("LINECNT") = WW_LINECNT

                '実勢軽油価格参照先ID
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEID))
                WW_DATATYPE = DataTypeHT("DIESELPRICESITEID")
                LNT0031Exceltblrow("DIESELPRICESITEID") = LNT0031WRKINC.DataConvert("実勢軽油価格参照先ID", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                ''実勢軽油価格参照先名
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITENAME))
                'WW_DATATYPE = DataTypeHT("DIESELPRICESITENAME")
                'LNT0031Exceltblrow("DIESELPRICESITENAME") = LNT0031WRKINC.DataConvert("実勢軽油価格参照先名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                '実勢軽油価格参照先ID枝番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEBRANCH))
                WW_DATATYPE = DataTypeHT("DIESELPRICESITEBRANCH")
                LNT0031Exceltblrow("DIESELPRICESITEBRANCH") = LNT0031WRKINC.DataConvert("実勢軽油価格参照先ID枝番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                ''実勢軽油価格参照先区分名
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICESITEKBNNAME))
                'WW_DATATYPE = DataTypeHT("DIESELPRICESITEKBNNAME")
                'LNT0031Exceltblrow("DIESELPRICESITEKBNNAME") = LNT0031WRKINC.DataConvert("実勢軽油価格参照先区分名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                '対象年
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.TARGETYEAR))
                WW_DATATYPE = DataTypeHT("TARGETYEAR")
                LNT0031Exceltblrow("TARGETYEAR") = LNT0031WRKINC.DataConvert("対象年", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '1月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE1))
                WW_DATATYPE = DataTypeHT("DIESELPRICE1")
                LNT0031Exceltblrow("DIESELPRICE1") = LNT0031WRKINC.DataConvert("1月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '2月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE2))
                WW_DATATYPE = DataTypeHT("DIESELPRICE2")
                LNT0031Exceltblrow("DIESELPRICE2") = LNT0031WRKINC.DataConvert("2月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '3月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE3))
                WW_DATATYPE = DataTypeHT("DIESELPRICE3")
                LNT0031Exceltblrow("DIESELPRICE3") = LNT0031WRKINC.DataConvert("3月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '4月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE4))
                WW_DATATYPE = DataTypeHT("DIESELPRICE4")
                LNT0031Exceltblrow("DIESELPRICE4") = LNT0031WRKINC.DataConvert("4月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '5月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE5))
                WW_DATATYPE = DataTypeHT("DIESELPRICE5")
                LNT0031Exceltblrow("DIESELPRICE5") = LNT0031WRKINC.DataConvert("5月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '6月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE6))
                WW_DATATYPE = DataTypeHT("DIESELPRICE6")
                LNT0031Exceltblrow("DIESELPRICE6") = LNT0031WRKINC.DataConvert("6月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '7月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE7))
                WW_DATATYPE = DataTypeHT("DIESELPRICE7")
                LNT0031Exceltblrow("DIESELPRICE7") = LNT0031WRKINC.DataConvert("7月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '8月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE8))
                WW_DATATYPE = DataTypeHT("DIESELPRICE8")
                LNT0031Exceltblrow("DIESELPRICE8") = LNT0031WRKINC.DataConvert("8月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '9月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE9))
                WW_DATATYPE = DataTypeHT("DIESELPRICE9")
                LNT0031Exceltblrow("DIESELPRICE9") = LNT0031WRKINC.DataConvert("9月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '10月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE10))
                WW_DATATYPE = DataTypeHT("DIESELPRICE10")
                LNT0031Exceltblrow("DIESELPRICE10") = LNT0031WRKINC.DataConvert("10月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '11月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE11))
                WW_DATATYPE = DataTypeHT("DIESELPRICE11")
                LNT0031Exceltblrow("DIESELPRICE11") = LNT0031WRKINC.DataConvert("11月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '12月実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DIESELPRICE12))
                WW_DATATYPE = DataTypeHT("DIESELPRICE12")
                LNT0031Exceltblrow("DIESELPRICE12") = LNT0031WRKINC.DataConvert("12月実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                'ロックフラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKFLG))
                WW_DATATYPE = DataTypeHT("LOCKFLG")
                LNT0031Exceltblrow("LOCKFLG") = LNT0031WRKINC.DataConvert("ロックフラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                ''ロック実行年月日
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKYMD))
                'WW_DATATYPE = DataTypeHT("LOCKYMD")
                'LNT0031Exceltblrow("LOCKYMD") = LNT0031WRKINC.DataConvert("ロック実行年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                ''ロック実行者
                'WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.LOCKUSER))
                'WW_DATATYPE = DataTypeHT("LOCKUSER")
                'LNT0031Exceltblrow("LOCKUSER") = LNT0031WRKINC.DataConvert("ロック実行者", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                'If WW_RESULT = False Then
                '    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                'End If
                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0031WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNT0031Exceltblrow("DELFLG") = LNT0031WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If

                '画面操作のためのフラグ、アップロードの場合は、常にゼロとする
                LNT0031Exceltblrow("ADDFLG") = "0"
                '登録
                LNT0031Exceltbl.Rows.Add(LNT0031Exceltblrow)

                WW_LINECNT = WW_LINECNT + 1
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, "アップロードファイル不正、内容を確認してください。", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "アップロードファイル不正、内容を確認してください。"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.OIL_FREE_MESSAGE
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = "ERR"
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' テーブル更新処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="iTbl"></param>
    ''' <remarks></remarks>
    Protected Sub TblUpdate(ByVal SQLcon As MySqlConnection, ByVal iTbl As DataTable, ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL

        Dim WW_ErrData As Boolean = False
        Dim DATENOW As DateTime = Date.Now

        '件数初期化
        Dim WW_UplInsCnt As Integer = 0                             'アップロード件数(登録)
        Dim WW_UplUpdCnt As Integer = 0                             'アップロード件数(更新)
        Dim WW_UplDelCnt As Integer = 0                             'アップロード件数(削除)
        Dim WW_UplErrCnt As Integer = 0                             'アップロード件数(エラー)
        Dim WW_UplUnnecessaryCnt As Integer = 0                     'アップロード件数(更新不要)
        Dim WW_DBDataCheck As String = ""

        For Each Row As DataRow In iTbl.Rows

            'テーブルに同一データが存在しない場合
            If Not SameDataChk(SQLcon, Row) = False Then
                '項目チェックスキップ(削除フラグが無効から有効になった場合)
                Dim SkipChk = ValidationSkipChk(SQLcon, Row)
                If SkipChk = True Then
                    ''履歴登録(変更前)
                    'InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNT0031WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    '削除フラグのみ更新する
                    SetDelflg(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If
                    ''履歴登録(変更後)
                    'InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNT0031WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    WW_UplDelCnt += 1
                    Continue For
                End If

                '項目チェック
                INPTableCheck(SQLcon, Row, WW_ErrSW)
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
                If WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    'InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.AFTDATA
                End If


                '件数カウント
                Select Case True
                    Case Row("DELFLG") = "1" '削除の場合
                        WW_UplDelCnt += 1
                    Case WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.NEWDATA '新規の場合
                        WW_UplInsCnt += 1
                    Case Else
                        WW_UplUpdCnt += 1
                End Select

                '登録、更新する
                InsUpdExcelData(SQLcon, Row, DATENOW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If

                '履歴登録(新規・変更後)
                'InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                'If Not isNormal(WW_ErrSW) Then
                '    Exit Sub
                'End If

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
            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        Else
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport(WW_OutPutCount)
            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
        End If

        'Rightboxを表示する
        If WW_UplErrCnt = 0 Then
            'エラーなし
            WF_RightboxOpen.Value = "OpenI"
        Else
            'エラーあり
            WF_RightboxOpen.Value = "Open"
        End If

        oRtn = WW_ErrSW

    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    Select")
        SQLStr.AppendLine("        LOCKFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE1, '')           = @DIESELPRICE1 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE2, '')           = @DIESELPRICE2 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE3, '')           = @DIESELPRICE3 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE4, '')           = @DIESELPRICE4 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE5, '')           = @DIESELPRICE5 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE6, '')           = @DIESELPRICE6 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE7, '')           = @DIESELPRICE7 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE8, '')           = @DIESELPRICE8 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE9, '')           = @DIESELPRICE9 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE10, '')          = @DIESELPRICE10 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE11, '')          = @DIESELPRICE11 ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICE12, '')          = @DIESELPRICE12 ")
        SQLStr.AppendLine("    AND  COALESCE(LOCKFLG, '')                = @LOCKFLG ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')                 = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)     '対象年
                Dim P_DIESELPRICE1 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE1", MySqlDbType.Decimal, 5)     '1月実勢単価
                Dim P_DIESELPRICE2 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE2", MySqlDbType.Decimal, 5)     '2月実勢単価
                Dim P_DIESELPRICE3 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE3", MySqlDbType.Decimal, 5)     '3月実勢単価
                Dim P_DIESELPRICE4 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE4", MySqlDbType.Decimal, 5)     '4月実勢単価
                Dim P_DIESELPRICE5 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE5", MySqlDbType.Decimal, 5)     '5月実勢単価
                Dim P_DIESELPRICE6 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE6", MySqlDbType.Decimal, 5)     '6月実勢単価
                Dim P_DIESELPRICE7 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE7", MySqlDbType.Decimal, 5)     '7月実勢単価
                Dim P_DIESELPRICE8 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE8", MySqlDbType.Decimal, 5)     '8月実勢単価
                Dim P_DIESELPRICE9 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE9", MySqlDbType.Decimal, 5)     '9月実勢単価
                Dim P_DIESELPRICE10 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE10", MySqlDbType.Decimal, 5)     '10月実勢単価
                Dim P_DIESELPRICE11 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE11", MySqlDbType.Decimal, 5)     '11月実勢単価
                Dim P_DIESELPRICE12 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE12", MySqlDbType.Decimal, 5)     '12月実勢単価
                Dim P_LOCKFLG As MySqlParameter = SQLcmd.Parameters.Add("@LOCKFLG", MySqlDbType.VarChar, 1)     'ロックフラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")           '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")           '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")           '対象年
                P_DIESELPRICE1.Value = WW_ROW("DIESELPRICE1")           '1月実勢単価
                P_DIESELPRICE2.Value = WW_ROW("DIESELPRICE2")           '2月実勢単価
                P_DIESELPRICE3.Value = WW_ROW("DIESELPRICE3")           '3月実勢単価
                P_DIESELPRICE4.Value = WW_ROW("DIESELPRICE4")           '4月実勢単価
                P_DIESELPRICE5.Value = WW_ROW("DIESELPRICE5")           '5月実勢単価
                P_DIESELPRICE6.Value = WW_ROW("DIESELPRICE6")           '6月実勢単価
                P_DIESELPRICE7.Value = WW_ROW("DIESELPRICE7")           '7月実勢単価
                P_DIESELPRICE8.Value = WW_ROW("DIESELPRICE8")           '8月実勢単価
                P_DIESELPRICE9.Value = WW_ROW("DIESELPRICE9")           '9月実勢単価
                P_DIESELPRICE10.Value = WW_ROW("DIESELPRICE10")           '10月実勢単価
                P_DIESELPRICE11.Value = WW_ROW("DIESELPRICE11")           '11月実勢単価
                P_DIESELPRICE12.Value = WW_ROW("DIESELPRICE12")           '12月実勢単価
                P_LOCKFLG.Value = WW_ROW("LOCKFLG")           'ロックフラグ
                P_DELFLG.Value = WW_ROW("DELFLG")           '削除フラグ

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try
        SameDataChk = True
    End Function

    '' <summary>
    '' 更新前の削除フラグが"0"、ロックフラグが'0'(アンロック）でアップロードした削除フラグが"1"の場合の場合Trueを返す
    '' </summary>
    Protected Function ValidationSkipChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        ValidationSkipChk = False
        'アップロードした削除フラグが"1"以外の場合処理を終了する
        If Not WW_ROW("DELFLG") = C_DELETE_FLG.DELETE Then
            Exit Function
        End If

        '一意キーが未入力の場合処理を終了する
        If WW_ROW("DIESELPRICESITEID") = "" OrElse
            WW_ROW("DIESELPRICESITEBRANCH") = "" OrElse
            WW_ROW("TARGETYEAR") = "" Then
            Exit Function
        End If

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        LOCKFLG")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)             '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                            '対象年

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年

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
                        If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                           WW_Tbl.Rows(0)("LOCKFLG") = C_DELETE_FLG.ALIVE Then
                            ValidationSkipChk = True
                            Exit Function
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try
    End Function

    '' <summary>
    '' ロックフラグが"1"（ロック中）の場合、Trueを返す
    '' </summary>
    Protected Function ValidationLockChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        ValidationLockChk = False

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        LOCKFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')                 = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)             '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                            '対象年
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                                    '削除フラグ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年
                P_DELFLG.Value = C_DELETE_FLG.ALIVE                                 '削除フラグ

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
                        '更新前のロックフラグがロック中の場合
                        If WW_Tbl.Rows(0)("LOCKFLG") = "1" Then
                            ValidationLockChk = True
                            Exit Function
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST SELECT"
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
        SQLStr.Append("     LNG.LNT0031_DIESELPRICEHIST             ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.Append("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.Append("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)            '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)    '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                           '対象年
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                                '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                            '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                                '更新プログラムＩＤ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年
                P_UPDYMD.Value = WW_DATENOW                                 '更新年月日
                P_UPDUSER.Value = Master.USERID                             '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                       '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name                '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST UPDATE"
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

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("        DIESELPRICESITEID  ")
        SQLStr.AppendLine("       ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("       ,TARGETYEAR  ")
        SQLStr.AppendLine("       ,DIESELPRICE1  ")
        SQLStr.AppendLine("       ,DIESELPRICE2  ")
        SQLStr.AppendLine("       ,DIESELPRICE3  ")
        SQLStr.AppendLine("       ,DIESELPRICE4  ")
        SQLStr.AppendLine("       ,DIESELPRICE5  ")
        SQLStr.AppendLine("       ,DIESELPRICE6  ")
        SQLStr.AppendLine("       ,DIESELPRICE7  ")
        SQLStr.AppendLine("       ,DIESELPRICE8  ")
        SQLStr.AppendLine("       ,DIESELPRICE9  ")
        SQLStr.AppendLine("       ,DIESELPRICE10  ")
        SQLStr.AppendLine("       ,DIESELPRICE11  ")
        SQLStr.AppendLine("       ,DIESELPRICE12  ")
        SQLStr.AppendLine("       ,LOCKFLG  ")
        SQLStr.AppendLine("       ,LOCKYMD  ")
        SQLStr.AppendLine("       ,LOCKUSER  ")
        SQLStr.AppendLine("       ,DELFLG  ")
        SQLStr.AppendLine("       ,INITYMD  ")
        SQLStr.AppendLine("       ,INITUSER  ")
        SQLStr.AppendLine("       ,INITTERMID  ")
        SQLStr.AppendLine("       ,INITPGID  ")
        SQLStr.AppendLine("       ,RECEIVEYMD  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("        @DIESELPRICESITEID  ")
        SQLStr.AppendLine("       ,@DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("       ,@TARGETYEAR  ")
        SQLStr.AppendLine("       ,@DIESELPRICE1  ")
        SQLStr.AppendLine("       ,@DIESELPRICE2  ")
        SQLStr.AppendLine("       ,@DIESELPRICE3  ")
        SQLStr.AppendLine("       ,@DIESELPRICE4  ")
        SQLStr.AppendLine("       ,@DIESELPRICE5  ")
        SQLStr.AppendLine("       ,@DIESELPRICE6  ")
        SQLStr.AppendLine("       ,@DIESELPRICE7  ")
        SQLStr.AppendLine("       ,@DIESELPRICE8  ")
        SQLStr.AppendLine("       ,@DIESELPRICE9  ")
        SQLStr.AppendLine("       ,@DIESELPRICE10  ")
        SQLStr.AppendLine("       ,@DIESELPRICE11  ")
        SQLStr.AppendLine("       ,@DIESELPRICE12  ")
        SQLStr.AppendLine("       ,@LOCKFLG  ")
        SQLStr.AppendLine("       ,@LOCKYMD  ")
        SQLStr.AppendLine("       ,@LOCKUSER  ")
        SQLStr.AppendLine("       ,@DELFLG  ")
        SQLStr.AppendLine("       ,@INITYMD  ")
        SQLStr.AppendLine("       ,@INITUSER  ")
        SQLStr.AppendLine("       ,@INITTERMID  ")
        SQLStr.AppendLine("       ,@INITPGID  ")
        SQLStr.AppendLine("       ,@RECEIVEYMD  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      DIESELPRICESITEID =  @DIESELPRICESITEID")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH =  @DIESELPRICESITEBRANCH")
        SQLStr.AppendLine("     ,TARGETYEAR =  @TARGETYEAR")
        SQLStr.AppendLine("     ,DIESELPRICE1 =  @DIESELPRICE1")
        SQLStr.AppendLine("     ,DIESELPRICE2 =  @DIESELPRICE2")
        SQLStr.AppendLine("     ,DIESELPRICE3 =  @DIESELPRICE3")
        SQLStr.AppendLine("     ,DIESELPRICE4 =  @DIESELPRICE4")
        SQLStr.AppendLine("     ,DIESELPRICE5 =  @DIESELPRICE5")
        SQLStr.AppendLine("     ,DIESELPRICE6 =  @DIESELPRICE6")
        SQLStr.AppendLine("     ,DIESELPRICE7 =  @DIESELPRICE7")
        SQLStr.AppendLine("     ,DIESELPRICE8 =  @DIESELPRICE8")
        SQLStr.AppendLine("     ,DIESELPRICE9 =  @DIESELPRICE9")
        SQLStr.AppendLine("     ,DIESELPRICE10 =  @DIESELPRICE10")
        SQLStr.AppendLine("     ,DIESELPRICE11 =  @DIESELPRICE11")
        SQLStr.AppendLine("     ,DIESELPRICE12 =  @DIESELPRICE12")
        SQLStr.AppendLine("     ,LOCKFLG =  @LOCKFLG")
        SQLStr.AppendLine("     ,LOCKYMD =  @LOCKYMD")
        SQLStr.AppendLine("     ,LOCKUSER =  @LOCKUSER")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("     ,RECEIVEYMD =  @RECEIVEYMD")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)     '対象年
                Dim P_DIESELPRICE1 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE1", MySqlDbType.Decimal, 5)     '1月実勢単価
                Dim P_DIESELPRICE2 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE2", MySqlDbType.Decimal, 5)     '2月実勢単価
                Dim P_DIESELPRICE3 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE3", MySqlDbType.Decimal, 5)     '3月実勢単価
                Dim P_DIESELPRICE4 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE4", MySqlDbType.Decimal, 5)     '4月実勢単価
                Dim P_DIESELPRICE5 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE5", MySqlDbType.Decimal, 5)     '5月実勢単価
                Dim P_DIESELPRICE6 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE6", MySqlDbType.Decimal, 5)     '6月実勢単価
                Dim P_DIESELPRICE7 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE7", MySqlDbType.Decimal, 5)     '7月実勢単価
                Dim P_DIESELPRICE8 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE8", MySqlDbType.Decimal, 5)     '8月実勢単価
                Dim P_DIESELPRICE9 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE9", MySqlDbType.Decimal, 5)     '9月実勢単価
                Dim P_DIESELPRICE10 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE10", MySqlDbType.Decimal, 5)     '10月実勢単価
                Dim P_DIESELPRICE11 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE11", MySqlDbType.Decimal, 5)     '11月実勢単価
                Dim P_DIESELPRICE12 As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICE12", MySqlDbType.Decimal, 5)     '12月実勢単価
                Dim P_LOCKFLG As MySqlParameter = SQLcmd.Parameters.Add("@LOCKFLG", MySqlDbType.VarChar, 1)     'ロックフラグ
                Dim P_LOCKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LOCKYMD", MySqlDbType.DateTime)     'ロック実行年月日
                Dim P_LOCKUSER As MySqlParameter = SQLcmd.Parameters.Add("@LOCKUSER", MySqlDbType.VarChar, 20)     'ロック実行者
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                'DB更新
                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")           '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")           '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")           '対象年
                P_DIESELPRICE1.Value = WW_ROW("DIESELPRICE1")           '1月実勢単価
                P_DIESELPRICE2.Value = WW_ROW("DIESELPRICE2")           '2月実勢単価
                P_DIESELPRICE3.Value = WW_ROW("DIESELPRICE3")           '3月実勢単価
                P_DIESELPRICE4.Value = WW_ROW("DIESELPRICE4")           '4月実勢単価
                P_DIESELPRICE5.Value = WW_ROW("DIESELPRICE5")           '5月実勢単価
                P_DIESELPRICE6.Value = WW_ROW("DIESELPRICE6")           '6月実勢単価
                P_DIESELPRICE7.Value = WW_ROW("DIESELPRICE7")           '7月実勢単価
                P_DIESELPRICE8.Value = WW_ROW("DIESELPRICE8")           '8月実勢単価
                P_DIESELPRICE9.Value = WW_ROW("DIESELPRICE9")           '9月実勢単価
                P_DIESELPRICE10.Value = WW_ROW("DIESELPRICE10")         '10月実勢単価
                P_DIESELPRICE11.Value = WW_ROW("DIESELPRICE11")         '11月実勢単価
                P_DIESELPRICE12.Value = WW_ROW("DIESELPRICE12")         '12月実勢単価
                P_LOCKFLG.Value = WW_ROW("LOCKFLG")                     'ロックフラグ
                If WW_ROW("LOCKFLG") = "0" Then
                    P_LOCKYMD.Value = DBNull.Value                      'ロック実行年月日
                    P_LOCKUSER.Value = ""                               'ロック実行者
                Else
                    P_LOCKYMD.Value = Date.Now                          'ロック実行年月日
                    P_LOCKUSER.Value = Master.USERID                    'ロック実行者
                End If
                P_DELFLG.Value = WW_ROW("DELFLG")                       '削除フラグ


                P_INITYMD.Value = WW_DATENOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0031_DIESELPRICEHIST  INSERTUPDATE"
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
    Protected Sub INPTableCheck(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim NowDate As DateTime = Date.Now

        WW_LineErr = ""

        '実勢軽油価格参照先ID(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESITEID", WW_ROW("DIESELPRICESITEID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢軽油価格参照先IDエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '実勢軽油価格参照先ID枝番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESITEBRANCH", WW_ROW("DIESELPRICESITEBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢軽油価格参照先ID枝番エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '対象年(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TARGETYEAR", WW_ROW("TARGETYEAR"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・対象年エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '1月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・1月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '2月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・2月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '3月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・3月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '4月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・4月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '5月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・5月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '6月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・6月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '7月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・7月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '8月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・8月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '9月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・9月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '10月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・10月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '11月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE11"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・11月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '12月実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICE", WW_ROW("DIESELPRICE12"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・12月実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        'ロックフラグ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "LOCKFLG", WW_ROW("LOCKFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("LOCKFLG", WW_ROW("LOCKFLG"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・ロックフラグ入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・ロックフラグエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        Dim LockChk = ValidationLockChk(SQLcon, WW_ROW)
        If LockChk = True Then
            WW_CheckMES1 = "・更新対象外エラー"
            WW_CheckMES2 = "ロック中です。データ更新できません。"
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

        '実勢単価履歴に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DIESELPRICESITEID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)                '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)        '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                               '対象年

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年

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
                        WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0031_DIESELPRICEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DIESELPRICEHIST SELECT"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0032_SURCHARGEPATTERNHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("        LNG.LNT0031_DIESELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD")           '有効開始日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNT0031WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNT0031WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNT0031WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNT0031WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0032_SURCHARGEPATTERNHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0032_SURCHARGEPATTERNHIST  INSERT"
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


