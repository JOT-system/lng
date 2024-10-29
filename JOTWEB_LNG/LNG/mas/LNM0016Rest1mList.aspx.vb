''************************************************************
' 使用料特例１マスタメンテ一覧画面
' 作成日 2022/02/18
' 更新日 2023/10/02
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/18 新規作成
'          : 2023/10/02 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 使用料特例１マスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0016Rest1mList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0016tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                          '添付ファイルテーブル
    Private LNM0016Exceltbl As New DataTable                        'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数
    Private Const CONST_USEFEERATE_INT_NUM As Integer = 1           '使用料率整数部分桁数
    Private Const CONST_USEFEERATE_DEC_NUM As Integer = 4           '使用料率小数部分桁数

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
                    Master.RecoverTable(LNM0016tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0016WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0016WRKINC.FILETYPE.PDF)
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
            If Not IsNothing(LNM0016tbl) Then
                LNM0016tbl.Clear()
                LNM0016tbl.Dispose()
                LNM0016tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0016WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0016S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0016D Then
            Master.RecoverTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)
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
        Master.SaveTable(LNM0016tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0016tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0016tbl)

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

        If IsNothing(LNM0016tbl) Then
            LNM0016tbl = New DataTable
        End If

        If LNM0016tbl.Columns.Count <> 0 Then
            LNM0016tbl.Columns.Clear()
        End If

        LNM0016tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを使用料特例１マスタから取得する
        Dim SQLStr As String =
              " Select                                                                                                       " _
            & "     1                                                                            AS 'SELECT'                 " _
            & "   , 0                                                                            AS HIDDEN                   " _
            & "   , 0                                                                            AS LINECNT                  " _
            & "   , ''                                                                           AS OPERATION                " _
            & "   , LNM0016.UPDTIMSTP                                                            AS UPDTIMSTP                " _
            & "   , coalesce(RTRIM(LNM0016.DELFLG), '')                                            AS DELFLG                   " _
            & "   , coalesce(RTRIM(LNM0016.ORGCODE), '')                                           AS ORGCODE                  " _
            & "   , coalesce(RTRIM(LNM0016.BIGCTNCD), '')                                          AS BIGCTNCD                 " _
            & "   , coalesce(RTRIM(LNM0016.MIDDLECTNCD), '')                                       AS MIDDLECTNCD              " _
            & "   , coalesce(RTRIM(LNM0016.DEPSTATION), '')                                        AS DEPSTATION               " _
            & "   , coalesce(RTRIM(LNM0016.DEPTRUSTEECD), '')                                      AS DEPTRUSTEECD             " _
            & "   , coalesce(RTRIM(LNM0016.DEPTRUSTEESUBCD), '')                                   AS DEPTRUSTEESUBCD          " _
            & "   , coalesce(RTRIM(LNM0016.PRIORITYNO), '')                                        AS PRIORITYNO               " _
            & "   , coalesce(RTRIM(LNM0016.PURPOSE), '')                                           AS PURPOSE                  " _
            & "   , coalesce(RTRIM(LNM0016.SLCCTNTYPE), '')                                        AS SLCCTNTYPE               " _
            & "   , coalesce(RTRIM(LNM0016.SLCCTNSTNO), '')                                        AS SLCCTNSTNO               " _
            & "   , coalesce(RTRIM(LNM0016.SLCCTNENDNO), '')                                       AS SLCCTNENDNO              " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRDEPBRANCHCD), '')                                  AS SLCJRDEPBRANCHCD         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCD1), '')                                  AS SLCDEPSHIPPERCD1         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCD2), '')                                  AS SLCDEPSHIPPERCD2         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCD3), '')                                  AS SLCDEPSHIPPERCD3         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCD4), '')                                  AS SLCDEPSHIPPERCD4         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCD5), '')                                  AS SLCDEPSHIPPERCD5         " _
            & "   , coalesce(RTRIM(LNM0016.SLCDEPSHIPPERCDCOND), '')                               AS SLCDEPSHIPPERCDCOND      " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRARRBRANCHCD), '')                                  AS SLCJRARRBRANCHCD         " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRARRBRANCHCDCOND), '')                              AS SLCJRARRBRANCHCDCOND     " _
            & "   , coalesce(RTRIM(LNM0016.SLCJOTARRORGCODE), '')                                  AS SLCJOTARRORGCODE         " _
            & "   , coalesce(RTRIM(LNM0016.SLCJOTARRORGCODECOND), '')                              AS SLCJOTARRORGCODECOND     " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION1), '')                                    AS SLCARRSTATION1           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION2), '')                                    AS SLCARRSTATION2           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION3), '')                                    AS SLCARRSTATION3           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION4), '')                                    AS SLCARRSTATION4           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION5), '')                                    AS SLCARRSTATION5           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION6), '')                                    AS SLCARRSTATION6           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION7), '')                                    AS SLCARRSTATION7           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION8), '')                                    AS SLCARRSTATION8           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION9), '')                                    AS SLCARRSTATION9           " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATION10), '')                                   AS SLCARRSTATION10          " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRSTATIONCOND), '')                                 AS SLCARRSTATIONCOND        " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRTRUSTEECD), '')                                   AS SLCARRTRUSTEECD          " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRTRUSTEECDCOND), '')                               AS SLCARRTRUSTEECDCOND      " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRTRUSTEESUBCD), '')                                AS SLCARRTRUSTEESUBCD       " _
            & "   , coalesce(RTRIM(LNM0016.SLCARRTRUSTEESUBCDCOND), '')                            AS SLCARRTRUSTEESUBCDCOND   " _
            & "   , coalesce(RTRIM(LNM0016.SLCSTMD), '')                                           AS SLCSTMD                  " _
            & "   , coalesce(RTRIM(LNM0016.SLCENDMD), '')                                          AS SLCENDMD                 " _
            & "   , coalesce(FORMAT(LNM0016.SLCSTSHIPYMD, 'yyyy/MM/dd'), '')                       AS SLCSTSHIPYMD             " _
            & "   , coalesce(FORMAT(LNM0016.SLCENDSHIPYMD, 'yyyy/MM/dd'), '')                      AS SLCENDSHIPYMD            " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD1), '')                                      AS SLCJRITEMCD1             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD2), '')                                      AS SLCJRITEMCD2             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD3), '')                                      AS SLCJRITEMCD3             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD4), '')                                      AS SLCJRITEMCD4             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD5), '')                                      AS SLCJRITEMCD5             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD6), '')                                      AS SLCJRITEMCD6             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD7), '')                                      AS SLCJRITEMCD7             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD8), '')                                      AS SLCJRITEMCD8             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD9), '')                                      AS SLCJRITEMCD9             " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCD10), '')                                     AS SLCJRITEMCD10            " _
            & "   , coalesce(RTRIM(LNM0016.SLCJRITEMCDCOND), '')                                   AS SLCJRITEMCDCOND          " _
            & "   , coalesce(RTRIM(LNM0016.SPRUSEFEE), '')                                         AS SPRUSEFEE                " _
            & "   , coalesce(RTRIM(LNM0016.SPRUSEFEERATE), '')                                     AS SPRUSEFEERATE            " _
            & "   , NULLIF(RTRIM(LNM0016.SPRUSEFEERATEROUND), 0)                                 AS SPRUSEFEERATEROUND       " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0016.SPRUSEFEERATEROUND), 0), ''),1,1)      AS SPRUSEFEERATEROUND1       " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0016.SPRUSEFEERATEROUND), 0), ''),2,1)      AS SPRUSEFEERATEROUND2       " _
            & "   , coalesce(RTRIM(LNM0016.SPRUSEFEERATEADDSUB), '')                               AS SPRUSEFEERATEADDSUB       " _
            & "   , NULLIF(RTRIM(LNM0016.SPRUSEFEERATEADDSUBCOND), 0)                            AS SPRUSEFEERATEADDSUBCOND   " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0016.SPRUSEFEERATEADDSUBCOND), 0), ''),1,1) AS SPRUSEFEERATEADDSUBCOND1  " _
            & "   , SUBSTRING(coalesce(NULLIF(RTRIM(LNM0016.SPRUSEFEERATEADDSUBCOND), 0), ''),2,1) AS SPRUSEFEERATEADDSUBCOND2  " _
            & "   , coalesce(RTRIM(LNM0016.SPRROUNDPOINTKBN), '')                                  AS SPRROUNDPOINTKBN          " _
            & "   , coalesce(RTRIM(LNM0016.SPRUSEFREESPE), '')                                     AS SPRUSEFREESPE             " _
            & "   , coalesce(RTRIM(LNM0016.SPRNITTSUFREESENDFEE), '')                              AS SPRNITTSUFREESENDFEE      " _
            & "   , coalesce(RTRIM(LNM0016.SPRMANAGEFEE), '')                                      AS SPRMANAGEFEE              " _
            & "   , coalesce(RTRIM(LNM0016.SPRSHIPBURDENFEE), '')                                  AS SPRSHIPBURDENFEE          " _
            & "   , coalesce(RTRIM(LNM0016.SPRSHIPFEE), '')                                        AS SPRSHIPFEE                " _
            & "   , coalesce(RTRIM(LNM0016.SPRARRIVEFEE), '')                                      AS SPRARRIVEFEE              " _
            & "   , coalesce(RTRIM(LNM0016.SPRPICKUPFEE), '')                                      AS SPRPICKUPFEE              " _
            & "   , coalesce(RTRIM(LNM0016.SPRDELIVERYFEE), '')                                    AS SPRDELIVERYFEE            " _
            & "   , coalesce(RTRIM(LNM0016.SPROTHER1), '')                                         AS SPROTHER1                 " _
            & "   , coalesce(RTRIM(LNM0016.SPROTHER2), '')                                         AS SPROTHER2                 " _
            & "   , coalesce(RTRIM(LNM0016.SPRFITKBN), '')                                         AS SPRFITKBN                 " _
            & "   , coalesce(RTRIM(LNM0016.SPRCONTRACTCD), '')                                     AS SPRCONTRACTCD             " _
            & "   , coalesce(RTRIM(LNM0016.BEFOREORGCODE), '')                                     AS BEFOREORGCODE             " _
            & "   , coalesce(RTRIM(LNM0016.BEFORESLCJOTARRORGCODE), '')                            AS BEFORESLCJOTARRORGCODE    " _
            & " FROM                                                                                                          " _
            & "     LNG.LNM0016_REST1M LNM0016                                                                                "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' 組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLWhereStr = " WHERE                     " _
                        & "     LNM0016.ORGCODE = @P1 "
        End If
        ' 大分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_BIGCTNCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                          " _
                            & "     LNM0016.BIGCTNCD = @P2     "
            Else
                SQLWhereStr &= "    AND LNM0016.BIGCTNCD = @P2 "
            End If
        End If
        ' 中分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLECTNCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                             " _
                            & "     LNM0016.MIDDLECTNCD = @P3     "
            Else
                SQLWhereStr &= "    AND LNM0016.MIDDLECTNCD = @P3 "
            End If
        End If
        ' 発駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                            " _
                            & "     LNM0016.DEPSTATION = @P4     "
            Else
                SQLWhereStr &= "    AND LNM0016.DEPSTATION = @P4 "
            End If
        End If
        ' 発受託人コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEECD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                              " _
                            & "     LNM0016.DEPTRUSTEECD = @P5     "
            Else
                SQLWhereStr &= "    AND LNM0016.DEPTRUSTEECD = @P5 "
            End If
        End If
        ' 発受託人サブコード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEESUBCD.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                                 " _
                            & "     LNM0016.DEPTRUSTEESUBCD = @P6     "
            Else
                SQLWhereStr &= "    AND LNM0016.DEPTRUSTEESUBCD = @P6 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                  " _
                            & "     LNM0016.DELFLG = 0 "
            Else
                SQLWhereStr &= "    AND LNM0016.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY                     " _
            & "     LNM0016.ORGCODE          " _
            & "   , LNM0016.BIGCTNCD         " _
            & "   , LNM0016.MIDDLECTNCD      " _
            & "   , LNM0016.DEPSTATION       " _
            & "   , LNM0016.DEPTRUSTEECD     " _
            & "   , LNM0016.DEPTRUSTEESUBCD  "

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
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6)  '発駅コード
                    PARA4.Value = work.WF_SEL_DEPSTATION.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEECD.Text) Then
                    Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 5)  '発受託人コード
                    PARA5.Value = work.WF_SEL_DEPTRUSTEECD.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEESUBCD.Text) Then
                    Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 6)  '発受託人サブコード
                    PARA6.Value = work.WF_SEL_DEPTRUSTEESUBCD.Text
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0016tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0016tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0016row As DataRow In LNM0016tbl.Rows
                    i += 1
                    LNM0016row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016L Select"
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
        For Each LNM0016row As DataRow In LNM0016tbl.Rows
            If LNM0016row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0016row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0016tbl)

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
        work.WF_SEL_DEPSTATION2.Text = ""                                                     '発駅コード
        work.WF_SEL_DEPTRUSTEECD2.Text = ""                                                   '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD2.Text = ""                                                '発受託人サブコード
        work.WF_SEL_PRIORITYNO.Text = ""                                                      '優先順位
        work.WF_SEL_PURPOSE.Text = ""                                                         '使用目的
        work.WF_SEL_SLCCTNTYPE.Text = ""                                                      '選択比較項目-コンテナ記号
        work.WF_SEL_SLCCTNSTNO.Text = ""                                                      '選択比較項目-コンテナ番号（開始）
        work.WF_SEL_SLCCTNENDNO.Text = ""                                                     '選択比較項目-コンテナ番号（終了）
        work.WF_SEL_SLCJRDEPBRANCHCD.Text = ""                                                '選択比較項目-ＪＲ発支社支店コード
        work.WF_SEL_SLCDEPSHIPPERCD1.Text = ""                                                '選択比較項目-発荷主コード１
        work.WF_SEL_SLCDEPSHIPPERCD2.Text = ""                                                '選択比較項目-発荷主コード２
        work.WF_SEL_SLCDEPSHIPPERCD3.Text = ""                                                '選択比較項目-発荷主コード３
        work.WF_SEL_SLCDEPSHIPPERCD4.Text = ""                                                '選択比較項目-発荷主コード４
        work.WF_SEL_SLCDEPSHIPPERCD5.Text = ""                                                '選択比較項目-発荷主コード５
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCDEPSHIPPERCDCOND.Text)   '選択比較項目-発荷主ＣＤ比較条件
        work.WF_SEL_SLCJRARRBRANCHCD.Text = ""                                                '選択比較項目-ＪＲ着支社支店コード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCJRARRBRANCHCDCOND.Text)  '選択比較項目-ＪＲ着支社支店ＣＤ比較
        work.WF_SEL_SLCJOTARRORGCODE.Text = ""                                                '選択比較項目-ＪＯＴ着組織コード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCJOTARRORGCODECOND.Text)  '選択比較項目-ＪＯＴ着組織ＣＤ比較
        work.WF_SEL_SLCARRSTATION1.Text = ""                                                  '選択比較項目-着駅コード１
        work.WF_SEL_SLCARRSTATION2.Text = ""                                                  '選択比較項目-着駅コード２
        work.WF_SEL_SLCARRSTATION3.Text = ""                                                  '選択比較項目-着駅コード３
        work.WF_SEL_SLCARRSTATION4.Text = ""                                                  '選択比較項目-着駅コード４
        work.WF_SEL_SLCARRSTATION5.Text = ""                                                  '選択比較項目-着駅コード５
        work.WF_SEL_SLCARRSTATION6.Text = ""                                                  '選択比較項目-着駅コード６
        work.WF_SEL_SLCARRSTATION7.Text = ""                                                  '選択比較項目-着駅コード７
        work.WF_SEL_SLCARRSTATION8.Text = ""                                                  '選択比較項目-着駅コード８
        work.WF_SEL_SLCARRSTATION9.Text = ""                                                  '選択比較項目-着駅コード９
        work.WF_SEL_SLCARRSTATION10.Text = ""                                                 '選択比較項目-着駅コード１０
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCARRSTATIONCOND.Text)     '選択比較項目-着駅コード比較条件
        work.WF_SEL_SLCARRTRUSTEECD.Text = ""                                                 '選択比較項目-着受託人コード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCARRTRUSTEECDCOND.Text)   '選択比較項目-着受託人ＣＤ比較条件
        work.WF_SEL_SLCARRTRUSTEESUBCD.Text = ""                                              '選択比較項目-着受託人サブコード
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCARRTRUSTEESUBCDCOND.Text) '選択比較項目-着受託人サブＣＤ比較
        work.WF_SEL_SLCSTMD.Text = ""                                                         '選択比較項目-開始月日
        work.WF_SEL_SLCENDMD.Text = ""                                                        '選択比較項目-終了月日
        work.WF_SEL_SLCSTSHIPYMD.Text = ""                                                    '選択比較項目-開始発送年月日
        work.WF_SEL_SLCENDSHIPYMD.Text = ""                                                   '選択比較項目-終了発送年月日
        work.WF_SEL_SLCJRITEMCD1.Text = ""                                                    '選択比較項目-ＪＲ品目コード１
        work.WF_SEL_SLCJRITEMCD2.Text = ""                                                    '選択比較項目-ＪＲ品目コード２
        work.WF_SEL_SLCJRITEMCD3.Text = ""                                                    '選択比較項目-ＪＲ品目コード３
        work.WF_SEL_SLCJRITEMCD4.Text = ""                                                    '選択比較項目-ＪＲ品目コード４
        work.WF_SEL_SLCJRITEMCD5.Text = ""                                                    '選択比較項目-ＪＲ品目コード５
        work.WF_SEL_SLCJRITEMCD6.Text = ""                                                    '選択比較項目-ＪＲ品目コード６
        work.WF_SEL_SLCJRITEMCD7.Text = ""                                                    '選択比較項目-ＪＲ品目コード７
        work.WF_SEL_SLCJRITEMCD8.Text = ""                                                    '選択比較項目-ＪＲ品目コード８
        work.WF_SEL_SLCJRITEMCD9.Text = ""                                                    '選択比較項目-ＪＲ品目コード９
        work.WF_SEL_SLCJRITEMCD10.Text = ""                                                   '選択比較項目-ＪＲ品目コード１０
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SLCJRITEMCDCOND.Text)       '選択比較項目-ＪＲ品目コード比較
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRUSEFEE.Text)             '特例置換項目-使用料金額
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRUSEFEERATE.Text)         '特例置換項目-使用料率
        work.WF_SEL_SPRUSEFEERATEROUND.Text = ""                                              '特例置換項目-使用料率端数整理
        work.WF_SEL_SPRUSEFEERATEROUND1.Text = ""                                             '特例置換項目-使用料率端数整理1
        work.WF_SEL_SPRUSEFEERATEROUND2.Text = ""                                             '特例置換項目-使用料率端数整理2
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRUSEFEERATEADDSUB.Text)   '特例置換項目-使用料率加減額
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND.Text = ""                                         '特例置換項目-使用料率加減額端数整理
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND1.Text = ""                                        '特例置換項目-使用料率加減額端数整理1
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND2.Text = ""                                        '特例置換項目-使用料率加減額端数整理2
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRROUNDPOINTKBN.Text)      '特例置換項目-端数処理時点区分
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRUSEFREESPE.Text)         '特例置換項目-使用料無料特認
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRNITTSUFREESENDFEE.Text)  '特例置換項目-通運負担回送運賃
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRMANAGEFEE.Text)          '特例置換項目-運行管理料
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRSHIPBURDENFEE.Text)      '特例置換項目-荷主負担運賃
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRSHIPFEE.Text)            '特例置換項目-発送料
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRARRIVEFEE.Text)          '特例置換項目-到着料
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRPICKUPFEE.Text)          '特例置換項目-集荷料
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRDELIVERYFEE.Text)        '特例置換項目-配達料
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPROTHER1.Text)             '特例置換項目-その他１
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPROTHER2.Text)             '特例置換項目-その他２
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SPRFITKBN.Text)             '特例置換項目-適合区分
        work.WF_SEL_SPRCONTRACTCD.Text = ""                                                   '特例置換項目-契約コード
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                           '詳細画面更新メッセージ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0016tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0016Rest1mHistory.aspx")
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
        Dim TBLview As New DataView(LNM0016tbl)
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

        work.WF_SEL_LINECNT.Text = LNM0016tbl.Rows(WW_LineCNT)("LINECNT")                                    '選択行
        work.WF_SEL_DELFLG.Text = LNM0016tbl.Rows(WW_LineCNT)("DELFLG")                                      '削除フラグ
        work.WF_SEL_ORG2.Text = LNM0016tbl.Rows(WW_LineCNT)("ORGCODE")                                       '組織コード
        work.WF_SEL_BIGCTNCD2.Text = LNM0016tbl.Rows(WW_LineCNT)("BIGCTNCD")                                 '大分類コード
        work.WF_SEL_MIDDLECTNCD2.Text = LNM0016tbl.Rows(WW_LineCNT)("MIDDLECTNCD")                           '中分類コード
        work.WF_SEL_DEPSTATION2.Text = LNM0016tbl.Rows(WW_LineCNT)("DEPSTATION")                             '発駅コード
        work.WF_SEL_DEPTRUSTEECD2.Text = LNM0016tbl.Rows(WW_LineCNT)("DEPTRUSTEECD")                         '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD2.Text = LNM0016tbl.Rows(WW_LineCNT)("DEPTRUSTEESUBCD")                   '発受託人サブコード
        work.WF_SEL_PRIORITYNO.Text = LNM0016tbl.Rows(WW_LineCNT)("PRIORITYNO")                              '優先順位
        work.WF_SEL_PURPOSE.Text = LNM0016tbl.Rows(WW_LineCNT)("PURPOSE")                                    '使用目的
        work.WF_SEL_SLCCTNTYPE.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCCTNTYPE")                              '選択比較項目-コンテナ記号
        work.WF_SEL_SLCCTNSTNO.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCCTNSTNO")                              '選択比較項目-コンテナ番号（開始）
        work.WF_SEL_SLCCTNENDNO.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCCTNENDNO")                            '選択比較項目-コンテナ番号（終了）
        work.WF_SEL_SLCJRDEPBRANCHCD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRDEPBRANCHCD")                  '選択比較項目-ＪＲ発支社支店コード
        work.WF_SEL_SLCDEPSHIPPERCD1.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCD1")                  '選択比較項目-発荷主コード１
        work.WF_SEL_SLCDEPSHIPPERCD2.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCD2")                  '選択比較項目-発荷主コード２
        work.WF_SEL_SLCDEPSHIPPERCD3.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCD3")                  '選択比較項目-発荷主コード３
        work.WF_SEL_SLCDEPSHIPPERCD4.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCD4")                  '選択比較項目-発荷主コード４
        work.WF_SEL_SLCDEPSHIPPERCD5.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCD5")                  '選択比較項目-発荷主コード５
        work.WF_SEL_SLCDEPSHIPPERCDCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCDEPSHIPPERCDCOND")            '選択比較項目-発荷主ＣＤ比較条件
        work.WF_SEL_SLCJRARRBRANCHCD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRARRBRANCHCD")                  '選択比較項目-ＪＲ着支社支店コード
        work.WF_SEL_SLCJRARRBRANCHCDCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRARRBRANCHCDCOND")          '選択比較項目-ＪＲ着支社支店ＣＤ比較
        work.WF_SEL_SLCJOTARRORGCODE.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJOTARRORGCODE")                  '選択比較項目-ＪＯＴ着組織コード
        work.WF_SEL_SLCJOTARRORGCODECOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJOTARRORGCODECOND")          '選択比較項目-ＪＯＴ着組織ＣＤ比較
        work.WF_SEL_SLCARRSTATION1.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION1")                      '選択比較項目-着駅コード１
        work.WF_SEL_SLCARRSTATION2.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION2")                      '選択比較項目-着駅コード２
        work.WF_SEL_SLCARRSTATION3.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION3")                      '選択比較項目-着駅コード３
        work.WF_SEL_SLCARRSTATION4.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION4")                      '選択比較項目-着駅コード４
        work.WF_SEL_SLCARRSTATION5.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION5")                      '選択比較項目-着駅コード５
        work.WF_SEL_SLCARRSTATION6.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION6")                      '選択比較項目-着駅コード６
        work.WF_SEL_SLCARRSTATION7.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION7")                      '選択比較項目-着駅コード７
        work.WF_SEL_SLCARRSTATION8.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION8")                      '選択比較項目-着駅コード８
        work.WF_SEL_SLCARRSTATION9.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION9")                      '選択比較項目-着駅コード９
        work.WF_SEL_SLCARRSTATION10.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATION10")                    '選択比較項目-着駅コード１０
        work.WF_SEL_SLCARRSTATIONCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRSTATIONCOND")                '選択比較項目-着駅コード比較条件
        work.WF_SEL_SLCARRTRUSTEECD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRTRUSTEECD")                    '選択比較項目-着受託人コード
        work.WF_SEL_SLCARRTRUSTEECDCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRTRUSTEECDCOND")            '選択比較項目-着受託人ＣＤ比較条件
        work.WF_SEL_SLCARRTRUSTEESUBCD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRTRUSTEESUBCD")              '選択比較項目-着受託人サブコード
        work.WF_SEL_SLCARRTRUSTEESUBCDCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCARRTRUSTEESUBCDCOND")      '選択比較項目-着受託人サブＣＤ比較
        work.WF_SEL_SLCSTMD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCSTMD")                                    '選択比較項目-開始月日
        work.WF_SEL_SLCENDMD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCENDMD")                                  '選択比較項目-終了月日
        work.WF_SEL_SLCSTSHIPYMD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCSTSHIPYMD")                          '選択比較項目-開始発送年月日
        work.WF_SEL_SLCENDSHIPYMD.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCENDSHIPYMD")                        '選択比較項目-終了発送年月日
        work.WF_SEL_SLCJRITEMCD1.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD1")                          '選択比較項目-ＪＲ品目コード１
        work.WF_SEL_SLCJRITEMCD2.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD2")                          '選択比較項目-ＪＲ品目コード２
        work.WF_SEL_SLCJRITEMCD3.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD3")                          '選択比較項目-ＪＲ品目コード３
        work.WF_SEL_SLCJRITEMCD4.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD4")                          '選択比較項目-ＪＲ品目コード４
        work.WF_SEL_SLCJRITEMCD5.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD5")                          '選択比較項目-ＪＲ品目コード５
        work.WF_SEL_SLCJRITEMCD6.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD6")                          '選択比較項目-ＪＲ品目コード６
        work.WF_SEL_SLCJRITEMCD7.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD7")                          '選択比較項目-ＪＲ品目コード７
        work.WF_SEL_SLCJRITEMCD8.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD8")                          '選択比較項目-ＪＲ品目コード８
        work.WF_SEL_SLCJRITEMCD9.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD9")                          '選択比較項目-ＪＲ品目コード９
        work.WF_SEL_SLCJRITEMCD10.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCD10")                        '選択比較項目-ＪＲ品目コード１０
        work.WF_SEL_SLCJRITEMCDCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SLCJRITEMCDCOND")                    '選択比較項目-ＪＲ品目コード比較
        work.WF_SEL_SPRUSEFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEE")                                '特例置換項目-使用料金額
        work.WF_SEL_SPRUSEFEERATE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATE")                        '特例置換項目-使用料率
        work.WF_SEL_SPRUSEFEERATEROUND.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEROUND")              '特例置換項目-使用料率端数整理
        work.WF_SEL_SPRUSEFEERATEROUND1.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEROUND1")            '特例置換項目-使用料率端数整理1
        work.WF_SEL_SPRUSEFEERATEROUND2.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEROUND2")            '特例置換項目-使用料率端数整理2
        work.WF_SEL_SPRUSEFEERATEADDSUB.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEADDSUB")            '特例置換項目-使用料率加減額
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEADDSUBCOND")    '特例置換項目-使用料率加減額端数整理
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND1.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEADDSUBCOND1")  '特例置換項目-使用料率加減額端数整理1
        work.WF_SEL_SPRUSEFEERATEADDSUBCOND2.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFEERATEADDSUBCOND2")  '特例置換項目-使用料率加減額端数整理2
        work.WF_SEL_SPRROUNDPOINTKBN.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRROUNDPOINTKBN")                  '特例置換項目-端数処理時点区分
        work.WF_SEL_SPRUSEFREESPE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRUSEFREESPE")                        '特例置換項目-使用料無料特認
        work.WF_SEL_SPRNITTSUFREESENDFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRNITTSUFREESENDFEE")          '特例置換項目-通運負担回送運賃
        work.WF_SEL_SPRMANAGEFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRMANAGEFEE")                          '特例置換項目-運行管理料
        work.WF_SEL_SPRSHIPBURDENFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRSHIPBURDENFEE")                  '特例置換項目-荷主負担運賃
        work.WF_SEL_SPRSHIPFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRSHIPFEE")                              '特例置換項目-発送料
        work.WF_SEL_SPRARRIVEFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRARRIVEFEE")                          '特例置換項目-到着料
        work.WF_SEL_SPRPICKUPFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRPICKUPFEE")                          '特例置換項目-集荷料
        work.WF_SEL_SPRDELIVERYFEE.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRDELIVERYFEE")                      '特例置換項目-配達料
        work.WF_SEL_SPROTHER1.Text = LNM0016tbl.Rows(WW_LineCNT)("SPROTHER1")                                '特例置換項目-その他１
        work.WF_SEL_SPROTHER2.Text = LNM0016tbl.Rows(WW_LineCNT)("SPROTHER2")                                '特例置換項目-その他２
        work.WF_SEL_SPRFITKBN.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRFITKBN")                                '特例置換項目-適合区分
        work.WF_SEL_SPRCONTRACTCD.Text = LNM0016tbl.Rows(WW_LineCNT)("SPRCONTRACTCD")                        '特例置換項目-契約コード
        work.WF_SEL_TIMESTAMP.Text = LNM0016tbl.Rows(WW_LineCNT)("UPDTIMSTP")                                'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                          '詳細画面更新メッセージ

        '○ 状態をクリア
        For Each LNM0016row As DataRow In LNM0016tbl.Rows
            Select Case LNM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0016tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0016tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0016tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0016tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0016tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0016tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0016tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)

        ' 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG2.Text) Then  '組織コード
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                work.WF_SEL_ORG2.Text, work.WF_SEL_BIGCTNCD2.Text,
                                work.WF_SEL_MIDDLECTNCD2.Text, work.WF_SEL_DEPSTATION2.Text,
                                work.WF_SEL_DEPTRUSTEECD2.Text, work.WF_SEL_DEPTRUSTEESUBCD2.Text,
                                work.WF_SEL_PRIORITYNO.Text, work.WF_SEL_TIMESTAMP.Text)
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0016WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "使用料特例マスタ１一覧"
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
            Case LNM0016WRKINC.FILETYPE.EXCEL
                FileName = "使用料特例マスタ１.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0016WRKINC.FILETYPE.PDF
                FileName = "使用料特例マスタ１.pdf"
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
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '組織コード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '大分類コード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '中分類コード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発駅コード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発受託人コード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '発受託人サブコード
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.PRIORITYNO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '優先順位

        '入力不要列網掛け
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.ORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '組織名称
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.BIGCTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '大分類名称
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '中分類名称
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発駅名称
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEENM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発受託人名称
        sheet.Columns(LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '発受託人サブ名称

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
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）組織コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.ORGNAME).Value = "組織名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = "（必須）大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = "大分類名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = "（必須）中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = "中分類名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION).Value = "（必須）発駅コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = "発駅名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = "（必須）発受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEENM).Value = "発受託人名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = "（必須）発受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM).Value = "発受託人サブ名称"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.PRIORITYNO).Value = "（必須）優先順位"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.PURPOSE).Value = "使用目的"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNTYPE).Value = "選択比較項目-コンテナ記号"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNSTNO).Value = "選択比較項目-コンテナ番号（開始）"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNENDNO).Value = "選択比較項目-コンテナ番号（終了）"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRDEPBRANCHCD).Value = "選択比較項目-ＪＲ発支社支店コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD1).Value = "選択比較項目-発荷主コード１"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD2).Value = "選択比較項目-発荷主コード２"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD3).Value = "選択比較項目-発荷主コード３"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD4).Value = "選択比較項目-発荷主コード４"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD5).Value = "選択比較項目-発荷主コード５"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCDCOND).Value = "選択比較項目-発荷主ＣＤ比較条件"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCD).Value = "選択比較項目-ＪＲ着支社支店コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCDCOND).Value = "選択比較項目-ＪＲ着支社支店ＣＤ比較"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODE).Value = "選択比較項目-ＪＯＴ着組織コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODECOND).Value = "選択比較項目-ＪＯＴ着組織ＣＤ比較"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION1).Value = "選択比較項目-着駅コード１"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION2).Value = "選択比較項目-着駅コード２"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION3).Value = "選択比較項目-着駅コード３"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION4).Value = "選択比較項目-着駅コード４"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION5).Value = "選択比較項目-着駅コード５"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION6).Value = "選択比較項目-着駅コード６"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION7).Value = "選択比較項目-着駅コード７"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION8).Value = "選択比較項目-着駅コード８"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION9).Value = "選択比較項目-着駅コード９"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION10).Value = "選択比較項目-着駅コード１０"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATIONCOND).Value = "選択比較項目-着駅コード比較条件"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECD).Value = "選択比較項目-着受託人コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECDCOND).Value = "選択比較項目-着受託人ＣＤ比較条件"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCD).Value = "選択比較項目-着受託人サブコード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCDCOND).Value = "選択比較項目-着受託人サブＣＤ比較"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTMD).Value = "選択比較項目-開始月日"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDMD).Value = "選択比較項目-終了月日"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTSHIPYMD).Value = "選択比較項目-開始発送年月日"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDSHIPYMD).Value = "選択比較項目-終了発送年月日"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD1).Value = "選択比較項目-ＪＲ品目コード１"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD2).Value = "選択比較項目-ＪＲ品目コード２"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD3).Value = "選択比較項目-ＪＲ品目コード３"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD4).Value = "選択比較項目-ＪＲ品目コード４"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD5).Value = "選択比較項目-ＪＲ品目コード５"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD6).Value = "選択比較項目-ＪＲ品目コード６"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD7).Value = "選択比較項目-ＪＲ品目コード７"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD8).Value = "選択比較項目-ＪＲ品目コード８"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD9).Value = "選択比較項目-ＪＲ品目コード９"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD10).Value = "選択比較項目-ＪＲ品目コード１０"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCDCOND).Value = "選択比較項目-ＪＲ品目コード比較"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEE).Value = "特例置換項目-使用料金額"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATE).Value = "特例置換項目-使用料率"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEROUND).Value = "特例置換項目-使用料率端数整理"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUB).Value = "特例置換項目-使用料率加減額"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUBCOND).Value = "特例置換項目-使用料率加減額端数整理"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRROUNDPOINTKBN).Value = "特例置換項目-端数処理時点区分"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFREESPE).Value = "特例置換項目-使用料無料特認"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRNITTSUFREESENDFEE).Value = "特例置換項目-通運負担回送運賃"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRMANAGEFEE).Value = "特例置換項目-運行管理料"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPBURDENFEE).Value = "特例置換項目-荷主負担運賃"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPFEE).Value = "特例置換項目-発送料"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRARRIVEFEE).Value = "特例置換項目-到着料"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRPICKUPFEE).Value = "特例置換項目-集荷料"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRDELIVERYFEE).Value = "特例置換項目-配達料"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER1).Value = "特例置換項目-その他１"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER2).Value = "特例置換項目-その他２"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRFITKBN).Value = "特例置換項目-適合区分"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRCONTRACTCD).Value = "特例置換項目-契約コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BEFOREORGCODE).Value = "変換前組織コード"
        sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BEFORESLCJOTARRORGCODE).Value = "変換前 選択比較項目-ＪＯＴ着組織コード"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '組織コード
            COMMENT_get(SQLcon, "ORGCODE", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                '組織コード
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.ORGCODE).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.ORGCODE).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '大分類コード
            COMMENT_get(SQLcon, "BIGCTNCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '中分類コード
            COMMENTCHILD_get(SQLcon, "MIDDLECTNCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '発駅コード
            COMMENT_get(SQLcon, "STATION", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＲ発支社支店コード
            COMMENT_get(SQLcon, "JRBRANCHCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRDEPBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRDEPBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-発荷主ＣＤ比較条件
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCDCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCDCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＲ着支社支店コード
            COMMENT_get(SQLcon, "JRBRANCHCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCD).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＲ着支社支店ＣＤ比較
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCDCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCDCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＯＴ着組織コード
            COMMENT_get(SQLcon, "ORGCODE", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODE).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODE).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＯＴ着組織ＣＤ比較
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODECOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODECOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-着駅コード１～１０
            COMMENT_get(SQLcon, "STATION", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION1).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION1).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION2).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION2).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION3).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION3).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION4).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION4).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION5).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION5).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION6).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION6).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION7).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION7).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION8).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION8).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION9).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION9).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With

                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION10).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION10).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-着駅コード比較条件
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATIONCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATIONCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-着受託人ＣＤ比較条件
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECDCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECDCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-着受託人サブＣＤ比較
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCDCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCDCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＲ品目コード１～１０
            COMMENTFIX_get(SQLcon, "ITEM", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD1).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD1).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD2).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD2).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD3).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD3).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD4).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD4).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD5).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD5).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD6).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD6).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD7).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD7).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD8).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD8).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD9).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD9).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD10).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD10).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '選択比較項目-ＪＲ品目コード比較
            COMMENT_get(SQLcon, "COMPARECONDKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCDCOND).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCDCOND).Comment.Shape
                    .Width = 150
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            Dim WW_HASUU As New StringBuilder
            WW_HASUU.AppendLine("11:１円未満切捨て")
            WW_HASUU.AppendLine("12:１０円未満切捨て")
            WW_HASUU.AppendLine("13:５０円刻みで切捨て")
            WW_HASUU.AppendLine("14:１００円刻みで切捨て")
            WW_HASUU.AppendLine("15:５００円刻みで切捨て")
            WW_HASUU.AppendLine("16:１０００円刻みで切捨て")
            WW_HASUU.AppendLine("21:１円未満四捨五入")
            WW_HASUU.AppendLine("22:１０円未満四捨五入")
            WW_HASUU.AppendLine("23:５０刻みで四捨五入")
            WW_HASUU.AppendLine("24:１００円刻みで四捨五入")
            WW_HASUU.AppendLine("25:５００円刻みで四捨五入")
            WW_HASUU.AppendLine("26:１０００円刻みで四捨五入  ")
            WW_HASUU.AppendLine("31:１円未満切り上げ")
            WW_HASUU.AppendLine("32:１０円未満切り上げ")
            WW_HASUU.AppendLine("33:５０円刻みで切り上げ")
            WW_HASUU.AppendLine("34:１００円刻みで切り上げ")
            WW_HASUU.AppendLine("35:５００円刻みで切り上げ")
            WW_HASUU.AppendLine("36:１０００円刻みで切り上げ")

            '特例置換項目-使用料率端数整理
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEROUND).AddComment(WW_HASUU.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEROUND).Comment.Shape
                .Width = 150
                .Height = 250
            End With


            '特例置換項目-使用料率加減額端数整理
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUBCOND).AddComment(WW_HASUU.ToString)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUBCOND).Comment.Shape
                .Width = 150
                .Height = 250
            End With

            '特例置換項目-端数処理時点区分
            COMMENT_get(SQLcon, "HASUUPOINTKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRROUNDPOINTKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRROUNDPOINTKBN).Comment.Shape
                    .Width = 500
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '特例置換項目-使用料無料特認
            COMMENT_get(SQLcon, "USEFREEKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFREESPE).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFREESPE).Comment.Shape
                    .Width = 500
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '特例置換項目-適合区分
            COMMENT_get(SQLcon, "FITKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRFITKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SPRFITKBN).Comment.Shape
                    .Width = 500
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '○ コメントに表示が難しいデータは別シートに作成
            WW_TEXT = "シート:受託人一覧参照"
            '発受託人コード、選択比較項目-着受託人コード
            SETSUBSHEET(wb, "REKEJM")
            '発受託人コード
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '選択比較項目-着受託人コード
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECD).Comment.Shape
                .Width = 150
                .Height = 30
            End With


            '選択比較項目-発荷主コード１～5
            WW_TEXT = "シート:発荷主一覧参照"
            SETSUBSHEET(wb, "SHIPPER")
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD1).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD1).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD2).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD2).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD3).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD3).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD4).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD4).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD5).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD5).Comment.Shape
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
        Dim WW_DEPSTATION As String
        Dim WW_DEPTRUSTEECD As String
        Dim WW_DEPTRUSTEESUBCD As String


        Dim WW_ORGNAME As String
        Dim WW_BIGCTNNM As String
        Dim WW_MIDDLECTNNM As String
        Dim WW_DEPSTATIONNM As String
        Dim WW_DEPTRUSTEENM As String
        Dim WW_DEPTRUSTEESUBNM As String

        For Each Row As DataRow In LNM0016tbl.Rows
            WW_ORGCODE = Row("ORGCODE") '組織コード
            WW_BIGCTNCD = Row("BIGCTNCD") '大分類コード
            WW_MIDDLECTNCD = Row("MIDDLECTNCD") '中分類コード
            WW_DEPSTATION = Row("DEPSTATION") '発駅コード
            WW_DEPTRUSTEECD = Row("DEPTRUSTEECD") '発受託人コード
            WW_DEPTRUSTEESUBCD = Row("DEPTRUSTEESUBCD") '発受託人サブコード

            '名称取得
            WW_ORGNAME = ""
            WW_BIGCTNNM = ""
            WW_MIDDLECTNNM = ""
            WW_DEPSTATIONNM = ""
            WW_DEPTRUSTEENM = ""
            WW_DEPTRUSTEESUBNM = ""

            CODENAME_get("ORG", WW_ORGCODE, WW_Dummy, WW_Dummy, WW_ORGNAME, WW_RtnSW) '組織名称
            CODENAME_get("BIGCTNCD", WW_BIGCTNCD, WW_Dummy, WW_Dummy, WW_BIGCTNNM, WW_RtnSW) '大分類名称
            CODENAME_get("MIDDLECTNCD", WW_MIDDLECTNCD, WW_BIGCTNCD, WW_Dummy, WW_MIDDLECTNNM, WW_RtnSW) '中分類名称
            CODENAME_get("STATION", WW_DEPSTATION, WW_Dummy, WW_Dummy, WW_DEPSTATIONNM, WW_RtnSW) '発駅名称
            CODENAME_get("DEPTRUSTEECD", WW_DEPTRUSTEECD, WW_DEPSTATION, WW_Dummy, WW_DEPTRUSTEENM, WW_RtnSW) '発受託人名称
            CODENAME_get("DEPTRUSTEESUBCD", WW_DEPTRUSTEESUBCD, WW_DEPSTATION, WW_DEPTRUSTEECD, WW_DEPTRUSTEESUBNM, WW_RtnSW) '発受託人サブ名称

            '値
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.ORGCODE).Value = WW_ORGCODE '組織コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.ORGNAME).Value = WW_ORGNAME '組織名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = WW_BIGCTNCD '大分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNNM).Value = WW_BIGCTNNM '大分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = WW_MIDDLECTNCD '中分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNNM).Value = WW_MIDDLECTNNM '中分類名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION).Value = WW_DEPSTATION '発駅コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATIONNM).Value = WW_DEPSTATIONNM '発駅名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD).Value = WW_DEPTRUSTEECD '発受託人コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEENM).Value = WW_DEPTRUSTEENM '発受託人名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD).Value = WW_DEPTRUSTEESUBCD '発受託人サブコード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBNM).Value = WW_DEPTRUSTEESUBNM '発受託人サブ名称
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.PRIORITYNO).Value = Row("PRIORITYNO") '優先順位
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.PURPOSE).Value = Row("PURPOSE") '使用目的
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNTYPE).Value = Row("SLCCTNTYPE") '選択比較項目-コンテナ記号
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNSTNO).Value = Row("SLCCTNSTNO") '選択比較項目-コンテナ番号（開始）
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNENDNO).Value = Row("SLCCTNENDNO") '選択比較項目-コンテナ番号（終了）
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRDEPBRANCHCD).Value = Row("SLCJRDEPBRANCHCD") '選択比較項目-ＪＲ発支社支店コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD1).Value = Row("SLCDEPSHIPPERCD1") '選択比較項目-発荷主コード１
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD2).Value = Row("SLCDEPSHIPPERCD2") '選択比較項目-発荷主コード２
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD3).Value = Row("SLCDEPSHIPPERCD3") '選択比較項目-発荷主コード３
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD4).Value = Row("SLCDEPSHIPPERCD4") '選択比較項目-発荷主コード４
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD5).Value = Row("SLCDEPSHIPPERCD5") '選択比較項目-発荷主コード５
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCDCOND).Value = Row("SLCDEPSHIPPERCDCOND") '選択比較項目-発荷主ＣＤ比較条件
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCD).Value = Row("SLCJRARRBRANCHCD") '選択比較項目-ＪＲ着支社支店コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCDCOND).Value = Row("SLCJRARRBRANCHCDCOND") '選択比較項目-ＪＲ着支社支店ＣＤ比較
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODE).Value = Row("SLCJOTARRORGCODE") '選択比較項目-ＪＯＴ着組織コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODECOND).Value = Row("SLCJOTARRORGCODECOND") '選択比較項目-ＪＯＴ着組織ＣＤ比較
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION1).Value = Row("SLCARRSTATION1") '選択比較項目-着駅コード１
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION2).Value = Row("SLCARRSTATION2") '選択比較項目-着駅コード２
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION3).Value = Row("SLCARRSTATION3") '選択比較項目-着駅コード３
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION4).Value = Row("SLCARRSTATION4") '選択比較項目-着駅コード４
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION5).Value = Row("SLCARRSTATION5") '選択比較項目-着駅コード５
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION6).Value = Row("SLCARRSTATION6") '選択比較項目-着駅コード６
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION7).Value = Row("SLCARRSTATION7") '選択比較項目-着駅コード７
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION8).Value = Row("SLCARRSTATION8") '選択比較項目-着駅コード８
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION9).Value = Row("SLCARRSTATION9") '選択比較項目-着駅コード９
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION10).Value = Row("SLCARRSTATION10") '選択比較項目-着駅コード１０
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATIONCOND).Value = Row("SLCARRSTATIONCOND") '選択比較項目-着駅コード比較条件
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECD).Value = Row("SLCARRTRUSTEECD") '選択比較項目-着受託人コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECDCOND).Value = Row("SLCARRTRUSTEECDCOND") '選択比較項目-着受託人ＣＤ比較条件
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCD).Value = Row("SLCARRTRUSTEESUBCD") '選択比較項目-着受託人サブコード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCDCOND).Value = Row("SLCARRTRUSTEESUBCDCOND") '選択比較項目-着受託人サブＣＤ比較
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTMD).Value = Row("SLCSTMD") '選択比較項目-開始月日
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDMD).Value = Row("SLCENDMD") '選択比較項目-終了月日
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTSHIPYMD).Value = Row("SLCSTSHIPYMD") '選択比較項目-開始発送年月日
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDSHIPYMD).Value = Row("SLCENDSHIPYMD") '選択比較項目-終了発送年月日
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD1).Value = Row("SLCJRITEMCD1") '選択比較項目-ＪＲ品目コード１
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD2).Value = Row("SLCJRITEMCD2") '選択比較項目-ＪＲ品目コード２
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD3).Value = Row("SLCJRITEMCD3") '選択比較項目-ＪＲ品目コード３
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD4).Value = Row("SLCJRITEMCD4") '選択比較項目-ＪＲ品目コード４
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD5).Value = Row("SLCJRITEMCD5") '選択比較項目-ＪＲ品目コード５
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD6).Value = Row("SLCJRITEMCD6") '選択比較項目-ＪＲ品目コード６
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD7).Value = Row("SLCJRITEMCD7") '選択比較項目-ＪＲ品目コード７
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD8).Value = Row("SLCJRITEMCD8") '選択比較項目-ＪＲ品目コード８
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD9).Value = Row("SLCJRITEMCD9") '選択比較項目-ＪＲ品目コード９
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD10).Value = Row("SLCJRITEMCD10") '選択比較項目-ＪＲ品目コード１０
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCDCOND).Value = Row("SLCJRITEMCDCOND") '選択比較項目-ＪＲ品目コード比較
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEE).Value = Row("SPRUSEFEE") '特例置換項目-使用料金額
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATE).Value = Row("SPRUSEFEERATE") '特例置換項目-使用料率
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEROUND).Value = Row("SPRUSEFEERATEROUND") '特例置換項目-使用料率端数整理
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUB).Value = Row("SPRUSEFEERATEADDSUB") '特例置換項目-使用料率加減額
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUBCOND).Value = Row("SPRUSEFEERATEADDSUBCOND") '特例置換項目-使用料率加減額端数整理
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRROUNDPOINTKBN).Value = Row("SPRROUNDPOINTKBN") '特例置換項目-端数処理時点区分
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFREESPE).Value = Row("SPRUSEFREESPE") '特例置換項目-使用料無料特認
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRNITTSUFREESENDFEE).Value = Row("SPRNITTSUFREESENDFEE") '特例置換項目-通運負担回送運賃
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRMANAGEFEE).Value = Row("SPRMANAGEFEE") '特例置換項目-運行管理料
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPBURDENFEE).Value = Row("SPRSHIPBURDENFEE") '特例置換項目-荷主負担運賃
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPFEE).Value = Row("SPRSHIPFEE") '特例置換項目-発送料
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRARRIVEFEE).Value = Row("SPRARRIVEFEE") '特例置換項目-到着料
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRPICKUPFEE).Value = Row("SPRPICKUPFEE") '特例置換項目-集荷料
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRDELIVERYFEE).Value = Row("SPRDELIVERYFEE") '特例置換項目-配達料
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER1).Value = Row("SPROTHER1") '特例置換項目-その他１
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER2).Value = Row("SPROTHER2") '特例置換項目-その他２
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRFITKBN).Value = Row("SPRFITKBN") '特例置換項目-適合区分
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.SPRCONTRACTCD).Value = Row("SPRCONTRACTCD") '特例置換項目-契約コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.BEFOREORGCODE).Value = Row("BEFOREORGCODE") '変換前組織コード
            sheet.Cells(WW_ACTIVEROW, LNM0016WRKINC.INOUTEXCELCOL.BEFORESLCJOTARRORGCODE).Value = Row("BEFORESLCJOTARRORGCODE") '変換前 選択比較項目-ＪＯＴ着組織コード

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
            Select Case I_FIELD
                Case "REKEJM"
                    subsheet.Name = "受託人一覧"
                Case "SHIPPER"
                    subsheet.Name = "発荷主一覧"
            End Select

            '○入力リスト取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Select Case I_FIELD
                    Case "REKEJM"
                        SETREKEJMLIST(SQLcon, subsheet)
                    Case "SHIPPER"
                        SETSHIPPERLIST(SQLcon, subsheet)
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
                Case "DELFLG",            '削除フラグ
                      "JRBRANCHCD",        'JR支社支店コード
                      "COMPARECONDKBN",    '比較条件区分
                      "HASUU1",            '端数区分１
                      "HASUU2",            '端数区分２
                      "HASUUPOINTKBN",     '端数時点区分
                      "USEFREEKBN",        '使用料無料区分
                      "FITKBN"
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE

                Case "ORGCODE" '組織コード
                    WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG
                Case "BIGCTNCD"                       '大分類コード
                    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_CLASS
                Case "STATION"                       '発駅コード、選択比較項目-着駅コード１～１０
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
        filePath = "D:\使用料特例マスタ１一括アップロードテスト.xlsx"

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
            For Each Row As DataRow In LNM0016Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0016WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0016WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REST1MEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.AFTDATA
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "使用料特例マスタ１の更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0016Exceltbl) Then
            LNM0016Exceltbl = New DataTable
        End If
        If LNM0016Exceltbl.Columns.Count <> 0 Then
            LNM0016Exceltbl.Columns.Clear()
        End If
        LNM0016Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\REST1MEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "REST1MEXCEL_TMP_"

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

            For Each Row As DataRow In LNM0016Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    Master.MAPID = LNM0016WRKINC.MAPIDD
                    INPTableCheck(Row, WW_ERR_SW)
                    Master.MAPID = LNM0016WRKINC.MAPIDL
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    REST1MEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.NEWDATA '新規の場合
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
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0016_REST1M ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0016Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016_REST1M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016_REST1M Select"
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

        Dim LNM0016Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0016Exceltblrow = LNM0016Exceltbl.NewRow

            'LINECNT
            LNM0016Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            '組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.ORGCODE))
            WW_DATATYPE = DataTypeHT("ORGCODE")
            LNM0016Exceltblrow("ORGCODE") = LNM0016WRKINC.DataConvert("組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '大分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.BIGCTNCD))
            WW_DATATYPE = DataTypeHT("BIGCTNCD")
            LNM0016Exceltblrow("BIGCTNCD") = LNM0016WRKINC.DataConvert("大分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '中分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.MIDDLECTNCD))
            WW_DATATYPE = DataTypeHT("MIDDLECTNCD")
            LNM0016Exceltblrow("MIDDLECTNCD") = LNM0016WRKINC.DataConvert("中分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発駅コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.DEPSTATION))
            WW_DATATYPE = DataTypeHT("DEPSTATION")
            LNM0016Exceltblrow("DEPSTATION") = LNM0016WRKINC.DataConvert("発駅コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEECD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEECD")
            LNM0016Exceltblrow("DEPTRUSTEECD") = LNM0016WRKINC.DataConvert("発受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '発受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.DEPTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("DEPTRUSTEESUBCD")
            LNM0016Exceltblrow("DEPTRUSTEESUBCD") = LNM0016WRKINC.DataConvert("発受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '優先順位
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.PRIORITYNO))
            If WW_TEXT = "" Then
                WW_CheckMES1 = "・[優先順位]を取得できませんでした。"
                WW_CheckMES2 = "入力必須項目です。"
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            Else
                WW_DATATYPE = DataTypeHT("PRIORITYNO")
                LNM0016Exceltblrow("PRIORITYNO") = LNM0016WRKINC.DataConvert("優先順位", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
            End If
            '使用目的
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.PURPOSE))
            WW_DATATYPE = DataTypeHT("PURPOSE")
            LNM0016Exceltblrow("PURPOSE") = LNM0016WRKINC.DataConvert("使用目的", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-コンテナ記号
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNTYPE)) = "" Then
                WW_TEXT = Strings.StrConv(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNTYPE)), VbStrConv.Narrow)
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("SLCCTNTYPE")
            LNM0016Exceltblrow("SLCCTNTYPE") = LNM0016WRKINC.DataConvert("選択比較項目-コンテナ記号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-コンテナ番号（開始）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNSTNO))
            WW_DATATYPE = DataTypeHT("SLCCTNSTNO")
            LNM0016Exceltblrow("SLCCTNSTNO") = LNM0016WRKINC.DataConvert("選択比較項目-コンテナ番号（開始）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-コンテナ番号（終了）
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCCTNENDNO))
            WW_DATATYPE = DataTypeHT("SLCCTNENDNO")
            LNM0016Exceltblrow("SLCCTNENDNO") = LNM0016WRKINC.DataConvert("選択比較項目-コンテナ番号（終了）", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ発支社支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRDEPBRANCHCD))
            WW_DATATYPE = DataTypeHT("SLCJRDEPBRANCHCD")
            LNM0016Exceltblrow("SLCJRDEPBRANCHCD") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ発支社支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主コード１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD1))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCD1")
            LNM0016Exceltblrow("SLCDEPSHIPPERCD1") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主コード１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主コード２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD2))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCD2")
            LNM0016Exceltblrow("SLCDEPSHIPPERCD2") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主コード２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主コード３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD3))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCD3")
            LNM0016Exceltblrow("SLCDEPSHIPPERCD3") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主コード３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主コード４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD4))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCD4")
            LNM0016Exceltblrow("SLCDEPSHIPPERCD4") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主コード４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主コード５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCD5))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCD5")
            LNM0016Exceltblrow("SLCDEPSHIPPERCD5") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主コード５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-発荷主ＣＤ比較条件
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCDEPSHIPPERCDCOND))
            WW_DATATYPE = DataTypeHT("SLCDEPSHIPPERCDCOND")
            LNM0016Exceltblrow("SLCDEPSHIPPERCDCOND") = LNM0016WRKINC.DataConvert("選択比較項目-発荷主ＣＤ比較条件", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ着支社支店コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCD))
            WW_DATATYPE = DataTypeHT("SLCJRARRBRANCHCD")
            LNM0016Exceltblrow("SLCJRARRBRANCHCD") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ着支社支店コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ着支社支店ＣＤ比較
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRARRBRANCHCDCOND))
            WW_DATATYPE = DataTypeHT("SLCJRARRBRANCHCDCOND")
            LNM0016Exceltblrow("SLCJRARRBRANCHCDCOND") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ着支社支店ＣＤ比較", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＯＴ着組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODE))
            WW_DATATYPE = DataTypeHT("SLCJOTARRORGCODE")
            LNM0016Exceltblrow("SLCJOTARRORGCODE") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＯＴ着組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＯＴ着組織ＣＤ比較
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJOTARRORGCODECOND))
            WW_DATATYPE = DataTypeHT("SLCJOTARRORGCODECOND")
            LNM0016Exceltblrow("SLCJOTARRORGCODECOND") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＯＴ着組織ＣＤ比較", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION1))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION1")
            LNM0016Exceltblrow("SLCARRSTATION1") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION2))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION2")
            LNM0016Exceltblrow("SLCARRSTATION2") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION3))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION3")
            LNM0016Exceltblrow("SLCARRSTATION3") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION4))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION4")
            LNM0016Exceltblrow("SLCARRSTATION4") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION5))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION5")
            LNM0016Exceltblrow("SLCARRSTATION5") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION6))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION6")
            LNM0016Exceltblrow("SLCARRSTATION6") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION7))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION7")
            LNM0016Exceltblrow("SLCARRSTATION7") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION8))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION8")
            LNM0016Exceltblrow("SLCARRSTATION8") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION9))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION9")
            LNM0016Exceltblrow("SLCARRSTATION9") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード１０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATION10))
            WW_DATATYPE = DataTypeHT("SLCARRSTATION10")
            LNM0016Exceltblrow("SLCARRSTATION10") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード１０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着駅コード比較条件
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRSTATIONCOND))
            WW_DATATYPE = DataTypeHT("SLCARRSTATIONCOND")
            LNM0016Exceltblrow("SLCARRSTATIONCOND") = LNM0016WRKINC.DataConvert("選択比較項目-着駅コード比較条件", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着受託人コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECD))
            WW_DATATYPE = DataTypeHT("SLCARRTRUSTEECD")
            LNM0016Exceltblrow("SLCARRTRUSTEECD") = LNM0016WRKINC.DataConvert("選択比較項目-着受託人コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着受託人ＣＤ比較条件
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEECDCOND))
            WW_DATATYPE = DataTypeHT("SLCARRTRUSTEECDCOND")
            LNM0016Exceltblrow("SLCARRTRUSTEECDCOND") = LNM0016WRKINC.DataConvert("選択比較項目-着受託人ＣＤ比較条件", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着受託人サブコード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCD))
            WW_DATATYPE = DataTypeHT("SLCARRTRUSTEESUBCD")
            LNM0016Exceltblrow("SLCARRTRUSTEESUBCD") = LNM0016WRKINC.DataConvert("選択比較項目-着受託人サブコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-着受託人サブＣＤ比較
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCARRTRUSTEESUBCDCOND))
            WW_DATATYPE = DataTypeHT("SLCARRTRUSTEESUBCDCOND")
            LNM0016Exceltblrow("SLCARRTRUSTEESUBCDCOND") = LNM0016WRKINC.DataConvert("選択比較項目-着受託人サブＣＤ比較", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-開始月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTMD))
            WW_DATATYPE = DataTypeHT("SLCSTMD")
            LNM0016Exceltblrow("SLCSTMD") = LNM0016WRKINC.DataConvert("選択比較項目-開始月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-終了月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDMD))
            WW_DATATYPE = DataTypeHT("SLCENDMD")
            LNM0016Exceltblrow("SLCENDMD") = LNM0016WRKINC.DataConvert("選択比較項目-終了月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-開始発送年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCSTSHIPYMD))
            WW_DATATYPE = DataTypeHT("SLCSTSHIPYMD")
            LNM0016Exceltblrow("SLCSTSHIPYMD") = LNM0016WRKINC.DataConvert("選択比較項目-開始発送年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-終了発送年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCENDSHIPYMD))
            WW_DATATYPE = DataTypeHT("SLCENDSHIPYMD")
            LNM0016Exceltblrow("SLCENDSHIPYMD") = LNM0016WRKINC.DataConvert("選択比較項目-終了発送年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD1))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD1")
            LNM0016Exceltblrow("SLCJRITEMCD1") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD2))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD2")
            LNM0016Exceltblrow("SLCJRITEMCD2") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD3))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD3")
            LNM0016Exceltblrow("SLCJRITEMCD3") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD4))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD4")
            LNM0016Exceltblrow("SLCJRITEMCD4") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD5))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD5")
            LNM0016Exceltblrow("SLCJRITEMCD5") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD6))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD6")
            LNM0016Exceltblrow("SLCJRITEMCD6") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD7))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD7")
            LNM0016Exceltblrow("SLCJRITEMCD7") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD8))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD8")
            LNM0016Exceltblrow("SLCJRITEMCD8") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD9))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD9")
            LNM0016Exceltblrow("SLCJRITEMCD9") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード１０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCD10))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCD10")
            LNM0016Exceltblrow("SLCJRITEMCD10") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード１０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '選択比較項目-ＪＲ品目コード比較
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SLCJRITEMCDCOND))
            WW_DATATYPE = DataTypeHT("SLCJRITEMCDCOND")
            LNM0016Exceltblrow("SLCJRITEMCDCOND") = LNM0016WRKINC.DataConvert("選択比較項目-ＪＲ品目コード比較", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料金額
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEE))
            WW_DATATYPE = DataTypeHT("SPRUSEFEE")
            LNM0016Exceltblrow("SPRUSEFEE") = LNM0016WRKINC.DataConvert("特例置換項目-使用料金額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料率
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATE))
            WW_DATATYPE = DataTypeHT("SPRUSEFEERATE")
            LNM0016Exceltblrow("SPRUSEFEERATE") = LNM0016WRKINC.DataConvert("特例置換項目-使用料率", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料率端数整理
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEROUND))
            WW_DATATYPE = DataTypeHT("SPRUSEFEERATEROUND")
            LNM0016Exceltblrow("SPRUSEFEERATEROUND") = LNM0016WRKINC.DataConvert("特例置換項目-使用料率端数整理", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料率加減額
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUB))
            WW_DATATYPE = DataTypeHT("SPRUSEFEERATEADDSUB")
            LNM0016Exceltblrow("SPRUSEFEERATEADDSUB") = LNM0016WRKINC.DataConvert("特例置換項目-使用料率加減額", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料率加減額端数整理
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFEERATEADDSUBCOND))
            WW_DATATYPE = DataTypeHT("SPRUSEFEERATEADDSUBCOND")
            LNM0016Exceltblrow("SPRUSEFEERATEADDSUBCOND") = LNM0016WRKINC.DataConvert("特例置換項目-使用料率加減額端数整理", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-端数処理時点区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRROUNDPOINTKBN))
            WW_DATATYPE = DataTypeHT("SPRROUNDPOINTKBN")
            LNM0016Exceltblrow("SPRROUNDPOINTKBN") = LNM0016WRKINC.DataConvert("特例置換項目-端数処理時点区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-使用料無料特認
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRUSEFREESPE))
            WW_DATATYPE = DataTypeHT("SPRUSEFREESPE")
            LNM0016Exceltblrow("SPRUSEFREESPE") = LNM0016WRKINC.DataConvert("特例置換項目-使用料無料特認", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-通運負担回送運賃
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRNITTSUFREESENDFEE))
            WW_DATATYPE = DataTypeHT("SPRNITTSUFREESENDFEE")
            LNM0016Exceltblrow("SPRNITTSUFREESENDFEE") = LNM0016WRKINC.DataConvert("特例置換項目-通運負担回送運賃", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-運行管理料
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRMANAGEFEE))
            WW_DATATYPE = DataTypeHT("SPRMANAGEFEE")
            LNM0016Exceltblrow("SPRMANAGEFEE") = LNM0016WRKINC.DataConvert("特例置換項目-運行管理料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-荷主負担運賃
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPBURDENFEE))
            WW_DATATYPE = DataTypeHT("SPRSHIPBURDENFEE")
            LNM0016Exceltblrow("SPRSHIPBURDENFEE") = LNM0016WRKINC.DataConvert("特例置換項目-荷主負担運賃", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-発送料
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRSHIPFEE))
            WW_DATATYPE = DataTypeHT("SPRSHIPFEE")
            LNM0016Exceltblrow("SPRSHIPFEE") = LNM0016WRKINC.DataConvert("特例置換項目-発送料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-到着料
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRARRIVEFEE))
            WW_DATATYPE = DataTypeHT("SPRARRIVEFEE")
            LNM0016Exceltblrow("SPRARRIVEFEE") = LNM0016WRKINC.DataConvert("特例置換項目-到着料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-集荷料
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRPICKUPFEE))
            WW_DATATYPE = DataTypeHT("SPRPICKUPFEE")
            LNM0016Exceltblrow("SPRPICKUPFEE") = LNM0016WRKINC.DataConvert("特例置換項目-集荷料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-配達料
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRDELIVERYFEE))
            WW_DATATYPE = DataTypeHT("SPRDELIVERYFEE")
            LNM0016Exceltblrow("SPRDELIVERYFEE") = LNM0016WRKINC.DataConvert("特例置換項目-配達料", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-その他１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER1))
            WW_DATATYPE = DataTypeHT("SPROTHER1")
            LNM0016Exceltblrow("SPROTHER1") = LNM0016WRKINC.DataConvert("特例置換項目-その他１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-その他２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPROTHER2))
            WW_DATATYPE = DataTypeHT("SPROTHER2")
            LNM0016Exceltblrow("SPROTHER2") = LNM0016WRKINC.DataConvert("特例置換項目-その他２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-適合区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRFITKBN))
            WW_DATATYPE = DataTypeHT("SPRFITKBN")
            LNM0016Exceltblrow("SPRFITKBN") = LNM0016WRKINC.DataConvert("特例置換項目-適合区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '特例置換項目-契約コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.SPRCONTRACTCD))
            WW_DATATYPE = DataTypeHT("SPRCONTRACTCD")
            LNM0016Exceltblrow("SPRCONTRACTCD") = LNM0016WRKINC.DataConvert("特例置換項目-契約コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.BEFOREORGCODE))
            WW_DATATYPE = DataTypeHT("BEFOREORGCODE")
            LNM0016Exceltblrow("BEFOREORGCODE") = LNM0016WRKINC.DataConvert("変換前組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '変換前 選択比較項目-ＪＯＴ着組織コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.BEFORESLCJOTARRORGCODE))
            WW_DATATYPE = DataTypeHT("BEFORESLCJOTARRORGCODE")
            LNM0016Exceltblrow("BEFORESLCJOTARRORGCODE") = LNM0016WRKINC.DataConvert("変換前 選択比較項目-ＪＯＴ着組織コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0016WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0016Exceltblrow("DELFLG") = LNM0016WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0016Exceltbl.Rows.Add(LNM0016Exceltblrow)

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
        SQLStr.AppendLine("        LNG.LNM0016_REST1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(BIGCTNCD, '')             = @BIGCTNCD ")
        SQLStr.AppendLine("    AND  coalesce(MIDDLECTNCD, '')             = @MIDDLECTNCD ")
        SQLStr.AppendLine("    AND  coalesce(DEPSTATION, '0')             = @DEPSTATION ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEECD, '0')             = @DEPTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(DEPTRUSTEESUBCD, '0')             = @DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(PRIORITYNO, '0')             = @PRIORITYNO ")
        SQLStr.AppendLine("    AND  coalesce(PURPOSE, '')             = @PURPOSE ")
        SQLStr.AppendLine("    AND  coalesce(SLCCTNTYPE, '')             = @SLCCTNTYPE ")
        SQLStr.AppendLine("    AND  coalesce(SLCCTNSTNO, '0')             = @SLCCTNSTNO ")
        SQLStr.AppendLine("    AND  coalesce(SLCCTNENDNO, '0')             = @SLCCTNENDNO ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRDEPBRANCHCD, '0')             = @SLCJRDEPBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCD1, '0')             = @SLCDEPSHIPPERCD1 ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCD2, '0')             = @SLCDEPSHIPPERCD2 ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCD3, '0')             = @SLCDEPSHIPPERCD3 ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCD4, '0')             = @SLCDEPSHIPPERCD4 ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCD5, '0')             = @SLCDEPSHIPPERCD5 ")
        SQLStr.AppendLine("    AND  coalesce(SLCDEPSHIPPERCDCOND, '0')             = @SLCDEPSHIPPERCDCOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRARRBRANCHCD, '0')             = @SLCJRARRBRANCHCD ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRARRBRANCHCDCOND, '0')             = @SLCJRARRBRANCHCDCOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCJOTARRORGCODE, '')             = @SLCJOTARRORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(SLCJOTARRORGCODECOND, '0')             = @SLCJOTARRORGCODECOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION1, '0')             = @SLCARRSTATION1 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION2, '0')             = @SLCARRSTATION2 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION3, '0')             = @SLCARRSTATION3 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION4, '0')             = @SLCARRSTATION4 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION5, '0')             = @SLCARRSTATION5 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION6, '0')             = @SLCARRSTATION6 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION7, '0')             = @SLCARRSTATION7 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION8, '0')             = @SLCARRSTATION8 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION9, '0')             = @SLCARRSTATION9 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATION10, '0')             = @SLCARRSTATION10 ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRSTATIONCOND, '0')             = @SLCARRSTATIONCOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRTRUSTEECD, '0')             = @SLCARRTRUSTEECD ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRTRUSTEECDCOND, '0')             = @SLCARRTRUSTEECDCOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRTRUSTEESUBCD, '0')             = @SLCARRTRUSTEESUBCD ")
        SQLStr.AppendLine("    AND  coalesce(SLCARRTRUSTEESUBCDCOND, '0')             = @SLCARRTRUSTEESUBCDCOND ")
        SQLStr.AppendLine("    AND  coalesce(SLCSTMD, '')             = @SLCSTMD ")
        SQLStr.AppendLine("    AND  coalesce(SLCENDMD, '')             = @SLCENDMD ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(SLCSTSHIPYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@SLCSTSHIPYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(SLCENDSHIPYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@SLCENDSHIPYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD1, '0')             = @SLCJRITEMCD1 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD2, '0')             = @SLCJRITEMCD2 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD3, '0')             = @SLCJRITEMCD3 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD4, '0')             = @SLCJRITEMCD4 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD5, '0')             = @SLCJRITEMCD5 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD6, '0')             = @SLCJRITEMCD6 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD7, '0')             = @SLCJRITEMCD7 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD8, '0')             = @SLCJRITEMCD8 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD9, '0')             = @SLCJRITEMCD9 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCD10, '0')             = @SLCJRITEMCD10 ")
        SQLStr.AppendLine("    AND  coalesce(SLCJRITEMCDCOND, '0')             = @SLCJRITEMCDCOND ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFEE, '0')             = @SPRUSEFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFEERATE, '0')             = @SPRUSEFEERATE ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFEERATEROUND, '0')             = @SPRUSEFEERATEROUND ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFEERATEADDSUB, '0')             = @SPRUSEFEERATEADDSUB ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFEERATEADDSUBCOND, '0')             = @SPRUSEFEERATEADDSUBCOND ")
        SQLStr.AppendLine("    AND  coalesce(SPRROUNDPOINTKBN, '0')             = @SPRROUNDPOINTKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRUSEFREESPE, '0')             = @SPRUSEFREESPE ")
        SQLStr.AppendLine("    AND  coalesce(SPRNITTSUFREESENDFEE, '0')             = @SPRNITTSUFREESENDFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRMANAGEFEE, '0')             = @SPRMANAGEFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRSHIPBURDENFEE, '0')             = @SPRSHIPBURDENFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRSHIPFEE, '0')             = @SPRSHIPFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRARRIVEFEE, '0')             = @SPRARRIVEFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRPICKUPFEE, '0')             = @SPRPICKUPFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPRDELIVERYFEE, '0')             = @SPRDELIVERYFEE ")
        SQLStr.AppendLine("    AND  coalesce(SPROTHER1, '0')             = @SPROTHER1 ")
        SQLStr.AppendLine("    AND  coalesce(SPROTHER2, '0')             = @SPROTHER2 ")
        SQLStr.AppendLine("    AND  coalesce(SPRFITKBN, '0')             = @SPRFITKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPRCONTRACTCD, '')             = @SPRCONTRACTCD ")
        SQLStr.AppendLine("    AND  coalesce(BEFOREORGCODE, '')             = @BEFOREORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(BEFORESLCJOTARRORGCODE, '')             = @BEFORESLCJOTARRORGCODE ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_SLCCTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNTYPE", MySqlDbType.VarChar, 5)         '選択比較項目-コンテナ記号
                Dim P_SLCCTNSTNO As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNSTNO", MySqlDbType.VarChar, 8)         '選択比較項目-コンテナ番号（開始）
                Dim P_SLCCTNENDNO As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNENDNO", MySqlDbType.VarChar, 8)         '選択比較項目-コンテナ番号（終了）
                Dim P_SLCJRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRDEPBRANCHCD", MySqlDbType.Int32)         '選択比較項目-ＪＲ発支社支店コード
                Dim P_SLCDEPSHIPPERCD1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD1", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード１
                Dim P_SLCDEPSHIPPERCD2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD2", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード２
                Dim P_SLCDEPSHIPPERCD3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD3", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード３
                Dim P_SLCDEPSHIPPERCD4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD4", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード４
                Dim P_SLCDEPSHIPPERCD5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD5", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード５
                Dim P_SLCDEPSHIPPERCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-発荷主ＣＤ比較条件
                Dim P_SLCJRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRARRBRANCHCD", MySqlDbType.Int32)         '選択比較項目-ＪＲ着支社支店コード
                Dim P_SLCJRARRBRANCHCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRARRBRANCHCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＲ着支社支店ＣＤ比較
                Dim P_SLCJOTARRORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@SLCJOTARRORGCODE", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＯＴ着組織コード
                Dim P_SLCJOTARRORGCODECOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJOTARRORGCODECOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＯＴ着組織ＣＤ比較
                Dim P_SLCARRSTATION1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION1", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード１
                Dim P_SLCARRSTATION2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION2", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード２
                Dim P_SLCARRSTATION3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION3", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード３
                Dim P_SLCARRSTATION4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION4", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード４
                Dim P_SLCARRSTATION5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION5", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード５
                Dim P_SLCARRSTATION6 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION6", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード６
                Dim P_SLCARRSTATION7 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION7", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード７
                Dim P_SLCARRSTATION8 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION8", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード８
                Dim P_SLCARRSTATION9 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION9", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード９
                Dim P_SLCARRSTATION10 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION10", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード１０
                Dim P_SLCARRSTATIONCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATIONCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着駅コード比較条件
                Dim P_SLCARRTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEECD", MySqlDbType.VarChar, 5)         '選択比較項目-着受託人コード
                Dim P_SLCARRTRUSTEECDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEECDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着受託人ＣＤ比較条件
                Dim P_SLCARRTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '選択比較項目-着受託人サブコード
                Dim P_SLCARRTRUSTEESUBCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEESUBCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着受託人サブＣＤ比較
                Dim P_SLCSTMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCSTMD", MySqlDbType.VarChar, 4)         '選択比較項目-開始月日
                Dim P_SLCENDMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCENDMD", MySqlDbType.VarChar, 4)         '選択比較項目-終了月日
                Dim P_SLCSTSHIPYMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCSTSHIPYMD", MySqlDbType.Date)         '選択比較項目-開始発送年月日
                Dim P_SLCENDSHIPYMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCENDSHIPYMD", MySqlDbType.Date)         '選択比較項目-終了発送年月日
                Dim P_SLCJRITEMCD1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD1", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード１
                Dim P_SLCJRITEMCD2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD2", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード２
                Dim P_SLCJRITEMCD3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD3", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード３
                Dim P_SLCJRITEMCD4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD4", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード４
                Dim P_SLCJRITEMCD5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD5", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード５
                Dim P_SLCJRITEMCD6 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD6", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード６
                Dim P_SLCJRITEMCD7 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD7", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード７
                Dim P_SLCJRITEMCD8 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD8", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード８
                Dim P_SLCJRITEMCD9 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD9", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード９
                Dim P_SLCJRITEMCD10 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD10", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード１０
                Dim P_SLCJRITEMCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＲ品目コード比較
                Dim P_SPRUSEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-使用料金額
                Dim P_SPRUSEFEERATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATE", MySqlDbType.Decimal, 5, 4)         '特例置換項目-使用料率
                Dim P_SPRUSEFEERATEROUND As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEROUND", MySqlDbType.VarChar, 2)         '特例置換項目-使用料率端数整理
                Dim P_SPRUSEFEERATEADDSUB As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEADDSUB", MySqlDbType.VarChar, 7)         '特例置換項目-使用料率加減額
                Dim P_SPRUSEFEERATEADDSUBCOND As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEADDSUBCOND", MySqlDbType.VarChar, 2)         '特例置換項目-使用料率加減額端数整理
                Dim P_SPRROUNDPOINTKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRROUNDPOINTKBN", MySqlDbType.VarChar, 2)         '特例置換項目-端数処理時点区分
                Dim P_SPRUSEFREESPE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFREESPE", MySqlDbType.VarChar, 2)         '特例置換項目-使用料無料特認
                Dim P_SPRNITTSUFREESENDFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRNITTSUFREESENDFEE", MySqlDbType.VarChar, 7)         '特例置換項目-通運負担回送運賃
                Dim P_SPRMANAGEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRMANAGEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-運行管理料
                Dim P_SPRSHIPBURDENFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRSHIPBURDENFEE", MySqlDbType.VarChar, 7)         '特例置換項目-荷主負担運賃
                Dim P_SPRSHIPFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRSHIPFEE", MySqlDbType.VarChar, 7)         '特例置換項目-発送料
                Dim P_SPRARRIVEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRIVEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-到着料
                Dim P_SPRPICKUPFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRPICKUPFEE", MySqlDbType.VarChar, 7)         '特例置換項目-集荷料
                Dim P_SPRDELIVERYFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRDELIVERYFEE", MySqlDbType.VarChar, 7)         '特例置換項目-配達料
                Dim P_SPROTHER1 As MySqlParameter = SQLcmd.Parameters.Add("@SPROTHER1", MySqlDbType.VarChar, 7)         '特例置換項目-その他１
                Dim P_SPROTHER2 As MySqlParameter = SQLcmd.Parameters.Add("@SPROTHER2", MySqlDbType.VarChar, 7)         '特例置換項目-その他２
                Dim P_SPRFITKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRFITKBN", MySqlDbType.VarChar, 2)         '特例置換項目-適合区分
                Dim P_SPRCONTRACTCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCONTRACTCD", MySqlDbType.VarChar, 5)         '特例置換項目-契約コード
                Dim P_BEFOREORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREORGCODE", MySqlDbType.VarChar, 6)         '変換前組織コード
                Dim P_BEFORESLCJOTARRORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFORESLCJOTARRORGCODE", MySqlDbType.VarChar, 6)         '変換前 選択比較項目-ＪＯＴ着組織コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_PURPOSE.Value = WW_ROW("PURPOSE")               '使用目的
                P_SLCCTNTYPE.Value = WW_ROW("SLCCTNTYPE")               '選択比較項目-コンテナ記号
                P_SLCCTNSTNO.Value = WW_ROW("SLCCTNSTNO")               '選択比較項目-コンテナ番号（開始）
                P_SLCCTNENDNO.Value = WW_ROW("SLCCTNENDNO")               '選択比較項目-コンテナ番号（終了）
                P_SLCJRDEPBRANCHCD.Value = WW_ROW("SLCJRDEPBRANCHCD")               '選択比較項目-ＪＲ発支社支店コード
                P_SLCDEPSHIPPERCD1.Value = WW_ROW("SLCDEPSHIPPERCD1")               '選択比較項目-発荷主コード１
                P_SLCDEPSHIPPERCD2.Value = WW_ROW("SLCDEPSHIPPERCD2")               '選択比較項目-発荷主コード２
                P_SLCDEPSHIPPERCD3.Value = WW_ROW("SLCDEPSHIPPERCD3")               '選択比較項目-発荷主コード３
                P_SLCDEPSHIPPERCD4.Value = WW_ROW("SLCDEPSHIPPERCD4")               '選択比較項目-発荷主コード４
                P_SLCDEPSHIPPERCD5.Value = WW_ROW("SLCDEPSHIPPERCD5")               '選択比較項目-発荷主コード５
                P_SLCDEPSHIPPERCDCOND.Value = WW_ROW("SLCDEPSHIPPERCDCOND")               '選択比較項目-発荷主ＣＤ比較条件
                P_SLCJRARRBRANCHCD.Value = WW_ROW("SLCJRARRBRANCHCD")               '選択比較項目-ＪＲ着支社支店コード
                P_SLCJRARRBRANCHCDCOND.Value = WW_ROW("SLCJRARRBRANCHCDCOND")               '選択比較項目-ＪＲ着支社支店ＣＤ比較
                P_SLCJOTARRORGCODE.Value = WW_ROW("SLCJOTARRORGCODE")               '選択比較項目-ＪＯＴ着組織コード
                P_SLCJOTARRORGCODECOND.Value = WW_ROW("SLCJOTARRORGCODECOND")               '選択比較項目-ＪＯＴ着組織ＣＤ比較
                P_SLCARRSTATION1.Value = WW_ROW("SLCARRSTATION1")               '選択比較項目-着駅コード１
                P_SLCARRSTATION2.Value = WW_ROW("SLCARRSTATION2")               '選択比較項目-着駅コード２
                P_SLCARRSTATION3.Value = WW_ROW("SLCARRSTATION3")               '選択比較項目-着駅コード３
                P_SLCARRSTATION4.Value = WW_ROW("SLCARRSTATION4")               '選択比較項目-着駅コード４
                P_SLCARRSTATION5.Value = WW_ROW("SLCARRSTATION5")               '選択比較項目-着駅コード５
                P_SLCARRSTATION6.Value = WW_ROW("SLCARRSTATION6")               '選択比較項目-着駅コード６
                P_SLCARRSTATION7.Value = WW_ROW("SLCARRSTATION7")               '選択比較項目-着駅コード７
                P_SLCARRSTATION8.Value = WW_ROW("SLCARRSTATION8")               '選択比較項目-着駅コード８
                P_SLCARRSTATION9.Value = WW_ROW("SLCARRSTATION9")               '選択比較項目-着駅コード９
                P_SLCARRSTATION10.Value = WW_ROW("SLCARRSTATION10")               '選択比較項目-着駅コード１０
                P_SLCARRSTATIONCOND.Value = WW_ROW("SLCARRSTATIONCOND")               '選択比較項目-着駅コード比較条件
                P_SLCARRTRUSTEECD.Value = WW_ROW("SLCARRTRUSTEECD")               '選択比較項目-着受託人コード
                P_SLCARRTRUSTEECDCOND.Value = WW_ROW("SLCARRTRUSTEECDCOND")               '選択比較項目-着受託人ＣＤ比較条件
                P_SLCARRTRUSTEESUBCD.Value = WW_ROW("SLCARRTRUSTEESUBCD")               '選択比較項目-着受託人サブコード
                P_SLCARRTRUSTEESUBCDCOND.Value = WW_ROW("SLCARRTRUSTEESUBCDCOND")               '選択比較項目-着受託人サブＣＤ比較
                P_SLCSTMD.Value = WW_ROW("SLCSTMD")               '選択比較項目-開始月日
                P_SLCENDMD.Value = WW_ROW("SLCENDMD")               '選択比較項目-終了月日

                '選択比較項目-開始発送年月日
                If Not WW_ROW("SLCSTSHIPYMD") = Date.MinValue Then
                    P_SLCSTSHIPYMD.Value = WW_ROW("SLCSTSHIPYMD")
                Else
                    P_SLCSTSHIPYMD.Value = DBNull.Value
                End If

                '選択比較項目-終了発送年月日
                If Not WW_ROW("SLCENDSHIPYMD") = Date.MinValue Then
                    P_SLCENDSHIPYMD.Value = WW_ROW("SLCENDSHIPYMD")
                Else
                    P_SLCENDSHIPYMD.Value = DBNull.Value
                End If

                P_SLCJRITEMCD1.Value = WW_ROW("SLCJRITEMCD1")               '選択比較項目-ＪＲ品目コード１
                P_SLCJRITEMCD2.Value = WW_ROW("SLCJRITEMCD2")               '選択比較項目-ＪＲ品目コード２
                P_SLCJRITEMCD3.Value = WW_ROW("SLCJRITEMCD3")               '選択比較項目-ＪＲ品目コード３
                P_SLCJRITEMCD4.Value = WW_ROW("SLCJRITEMCD4")               '選択比較項目-ＪＲ品目コード４
                P_SLCJRITEMCD5.Value = WW_ROW("SLCJRITEMCD5")               '選択比較項目-ＪＲ品目コード５
                P_SLCJRITEMCD6.Value = WW_ROW("SLCJRITEMCD6")               '選択比較項目-ＪＲ品目コード６
                P_SLCJRITEMCD7.Value = WW_ROW("SLCJRITEMCD7")               '選択比較項目-ＪＲ品目コード７
                P_SLCJRITEMCD8.Value = WW_ROW("SLCJRITEMCD8")               '選択比較項目-ＪＲ品目コード８
                P_SLCJRITEMCD9.Value = WW_ROW("SLCJRITEMCD9")               '選択比較項目-ＪＲ品目コード９
                P_SLCJRITEMCD10.Value = WW_ROW("SLCJRITEMCD10")               '選択比較項目-ＪＲ品目コード１０
                P_SLCJRITEMCDCOND.Value = WW_ROW("SLCJRITEMCDCOND")               '選択比較項目-ＪＲ品目コード比較
                P_SPRUSEFEE.Value = WW_ROW("SPRUSEFEE")               '特例置換項目-使用料金額
                P_SPRUSEFEERATE.Value = WW_ROW("SPRUSEFEERATE")               '特例置換項目-使用料率
                P_SPRUSEFEERATEROUND.Value = WW_ROW("SPRUSEFEERATEROUND")               '特例置換項目-使用料率端数整理
                P_SPRUSEFEERATEADDSUB.Value = WW_ROW("SPRUSEFEERATEADDSUB")               '特例置換項目-使用料率加減額
                P_SPRUSEFEERATEADDSUBCOND.Value = WW_ROW("SPRUSEFEERATEADDSUBCOND")               '特例置換項目-使用料率加減額端数整理
                P_SPRROUNDPOINTKBN.Value = WW_ROW("SPRROUNDPOINTKBN")               '特例置換項目-端数処理時点区分
                P_SPRUSEFREESPE.Value = WW_ROW("SPRUSEFREESPE")               '特例置換項目-使用料無料特認
                P_SPRNITTSUFREESENDFEE.Value = WW_ROW("SPRNITTSUFREESENDFEE")               '特例置換項目-通運負担回送運賃
                P_SPRMANAGEFEE.Value = WW_ROW("SPRMANAGEFEE")               '特例置換項目-運行管理料
                P_SPRSHIPBURDENFEE.Value = WW_ROW("SPRSHIPBURDENFEE")               '特例置換項目-荷主負担運賃
                P_SPRSHIPFEE.Value = WW_ROW("SPRSHIPFEE")               '特例置換項目-発送料
                P_SPRARRIVEFEE.Value = WW_ROW("SPRARRIVEFEE")               '特例置換項目-到着料
                P_SPRPICKUPFEE.Value = WW_ROW("SPRPICKUPFEE")               '特例置換項目-集荷料
                P_SPRDELIVERYFEE.Value = WW_ROW("SPRDELIVERYFEE")               '特例置換項目-配達料
                P_SPROTHER1.Value = WW_ROW("SPROTHER1")               '特例置換項目-その他１
                P_SPROTHER2.Value = WW_ROW("SPROTHER2")               '特例置換項目-その他２
                P_SPRFITKBN.Value = WW_ROW("SPRFITKBN")               '特例置換項目-適合区分
                P_SPRCONTRACTCD.Value = WW_ROW("SPRCONTRACTCD")               '特例置換項目-契約コード
                P_BEFOREORGCODE.Value = WW_ROW("BEFOREORGCODE")               '変換前組織コード
                P_BEFORESLCJOTARRORGCODE.Value = WW_ROW("BEFORESLCJOTARRORGCODE")               '変換前 選択比較項目-ＪＯＴ着組織コード
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016_REST1M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016_REST1M SELECT"
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
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0016_REST1M LNM0016")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @ORGCODE AS ORGCODE ")
        SQLStr.AppendLine("             ,@BIGCTNCD AS BIGCTNCD ")
        SQLStr.AppendLine("             ,@MIDDLECTNCD AS MIDDLECTNCD ")
        SQLStr.AppendLine("             ,@DEPSTATION AS DEPSTATION ")
        SQLStr.AppendLine("             ,@DEPTRUSTEECD AS DEPTRUSTEECD ")
        SQLStr.AppendLine("             ,@DEPTRUSTEESUBCD AS DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@PRIORITYNO AS PRIORITYNO ")
        SQLStr.AppendLine("             ,@PURPOSE AS PURPOSE ")
        SQLStr.AppendLine("             ,@SLCCTNTYPE AS SLCCTNTYPE ")
        SQLStr.AppendLine("             ,@SLCCTNSTNO AS SLCCTNSTNO ")
        SQLStr.AppendLine("             ,@SLCCTNENDNO AS SLCCTNENDNO ")
        SQLStr.AppendLine("             ,@SLCJRDEPBRANCHCD AS SLCJRDEPBRANCHCD ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCD1 AS SLCDEPSHIPPERCD1 ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCD2 AS SLCDEPSHIPPERCD2 ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCD3 AS SLCDEPSHIPPERCD3 ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCD4 AS SLCDEPSHIPPERCD4 ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCD5 AS SLCDEPSHIPPERCD5 ")
        SQLStr.AppendLine("             ,@SLCDEPSHIPPERCDCOND AS SLCDEPSHIPPERCDCOND ")
        SQLStr.AppendLine("             ,@SLCJRARRBRANCHCD AS SLCJRARRBRANCHCD ")
        SQLStr.AppendLine("             ,@SLCJRARRBRANCHCDCOND AS SLCJRARRBRANCHCDCOND ")
        SQLStr.AppendLine("             ,@SLCJOTARRORGCODE AS SLCJOTARRORGCODE ")
        SQLStr.AppendLine("             ,@SLCJOTARRORGCODECOND AS SLCJOTARRORGCODECOND ")
        SQLStr.AppendLine("             ,@SLCARRSTATION1 AS SLCARRSTATION1 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION2 AS SLCARRSTATION2 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION3 AS SLCARRSTATION3 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION4 AS SLCARRSTATION4 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION5 AS SLCARRSTATION5 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION6 AS SLCARRSTATION6 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION7 AS SLCARRSTATION7 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION8 AS SLCARRSTATION8 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION9 AS SLCARRSTATION9 ")
        SQLStr.AppendLine("             ,@SLCARRSTATION10 AS SLCARRSTATION10 ")
        SQLStr.AppendLine("             ,@SLCARRSTATIONCOND AS SLCARRSTATIONCOND ")
        SQLStr.AppendLine("             ,@SLCARRTRUSTEECD AS SLCARRTRUSTEECD ")
        SQLStr.AppendLine("             ,@SLCARRTRUSTEECDCOND AS SLCARRTRUSTEECDCOND ")
        SQLStr.AppendLine("             ,@SLCARRTRUSTEESUBCD AS SLCARRTRUSTEESUBCD ")
        SQLStr.AppendLine("             ,@SLCARRTRUSTEESUBCDCOND AS SLCARRTRUSTEESUBCDCOND ")
        SQLStr.AppendLine("             ,@SLCSTMD AS SLCSTMD ")
        SQLStr.AppendLine("             ,@SLCENDMD AS SLCENDMD ")
        SQLStr.AppendLine("             ,@SLCSTSHIPYMD AS SLCSTSHIPYMD ")
        SQLStr.AppendLine("             ,@SLCENDSHIPYMD AS SLCENDSHIPYMD ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD1 AS SLCJRITEMCD1 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD2 AS SLCJRITEMCD2 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD3 AS SLCJRITEMCD3 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD4 AS SLCJRITEMCD4 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD5 AS SLCJRITEMCD5 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD6 AS SLCJRITEMCD6 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD7 AS SLCJRITEMCD7 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD8 AS SLCJRITEMCD8 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD9 AS SLCJRITEMCD9 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCD10 AS SLCJRITEMCD10 ")
        SQLStr.AppendLine("             ,@SLCJRITEMCDCOND AS SLCJRITEMCDCOND ")
        SQLStr.AppendLine("             ,@SPRUSEFEE AS SPRUSEFEE ")
        SQLStr.AppendLine("             ,@SPRUSEFEERATE AS SPRUSEFEERATE ")
        SQLStr.AppendLine("             ,@SPRUSEFEERATEROUND AS SPRUSEFEERATEROUND ")
        SQLStr.AppendLine("             ,@SPRUSEFEERATEADDSUB AS SPRUSEFEERATEADDSUB ")
        SQLStr.AppendLine("             ,@SPRUSEFEERATEADDSUBCOND AS SPRUSEFEERATEADDSUBCOND ")
        SQLStr.AppendLine("             ,@SPRROUNDPOINTKBN AS SPRROUNDPOINTKBN ")
        SQLStr.AppendLine("             ,@SPRUSEFREESPE AS SPRUSEFREESPE ")
        SQLStr.AppendLine("             ,@SPRNITTSUFREESENDFEE AS SPRNITTSUFREESENDFEE ")
        SQLStr.AppendLine("             ,@SPRMANAGEFEE AS SPRMANAGEFEE ")
        SQLStr.AppendLine("             ,@SPRSHIPBURDENFEE AS SPRSHIPBURDENFEE ")
        SQLStr.AppendLine("             ,@SPRSHIPFEE AS SPRSHIPFEE ")
        SQLStr.AppendLine("             ,@SPRARRIVEFEE AS SPRARRIVEFEE ")
        SQLStr.AppendLine("             ,@SPRPICKUPFEE AS SPRPICKUPFEE ")
        SQLStr.AppendLine("             ,@SPRDELIVERYFEE AS SPRDELIVERYFEE ")
        SQLStr.AppendLine("             ,@SPROTHER1 AS SPROTHER1 ")
        SQLStr.AppendLine("             ,@SPROTHER2 AS SPROTHER2 ")
        SQLStr.AppendLine("             ,@SPRFITKBN AS SPRFITKBN ")
        SQLStr.AppendLine("             ,@SPRCONTRACTCD AS SPRCONTRACTCD ")
        SQLStr.AppendLine("             ,@BEFOREORGCODE AS BEFOREORGCODE ")
        SQLStr.AppendLine("             ,@BEFORESLCJOTARRORGCODE AS BEFORESLCJOTARRORGCODE ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0016.ORGCODE = EXCEL.ORGCODE ")
        SQLStr.AppendLine("         AND LNM0016.BIGCTNCD = EXCEL.BIGCTNCD ")
        SQLStr.AppendLine("         AND LNM0016.MIDDLECTNCD = EXCEL.MIDDLECTNCD ")
        SQLStr.AppendLine("         AND LNM0016.DEPSTATION = EXCEL.DEPSTATION ")
        SQLStr.AppendLine("         AND LNM0016.DEPTRUSTEECD = EXCEL.DEPTRUSTEECD ")
        SQLStr.AppendLine("         AND LNM0016.DEPTRUSTEESUBCD = EXCEL.DEPTRUSTEESUBCD ")
        SQLStr.AppendLine("         AND LNM0016.PRIORITYNO = EXCEL.PRIORITYNO ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0016.PURPOSE =  EXCEL.PURPOSE")
        SQLStr.AppendLine("         ,LNM0016.SLCCTNTYPE =  EXCEL.SLCCTNTYPE")
        SQLStr.AppendLine("         ,LNM0016.SLCCTNSTNO =  EXCEL.SLCCTNSTNO")
        SQLStr.AppendLine("         ,LNM0016.SLCCTNENDNO =  EXCEL.SLCCTNENDNO")
        SQLStr.AppendLine("         ,LNM0016.SLCJRDEPBRANCHCD =  EXCEL.SLCJRDEPBRANCHCD")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCD1 =  EXCEL.SLCDEPSHIPPERCD1")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCD2 =  EXCEL.SLCDEPSHIPPERCD2")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCD3 =  EXCEL.SLCDEPSHIPPERCD3")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCD4 =  EXCEL.SLCDEPSHIPPERCD4")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCD5 =  EXCEL.SLCDEPSHIPPERCD5")
        SQLStr.AppendLine("         ,LNM0016.SLCDEPSHIPPERCDCOND =  EXCEL.SLCDEPSHIPPERCDCOND")
        SQLStr.AppendLine("         ,LNM0016.SLCJRARRBRANCHCD =  EXCEL.SLCJRARRBRANCHCD")
        SQLStr.AppendLine("         ,LNM0016.SLCJRARRBRANCHCDCOND =  EXCEL.SLCJRARRBRANCHCDCOND")
        SQLStr.AppendLine("         ,LNM0016.SLCJOTARRORGCODE =  EXCEL.SLCJOTARRORGCODE")
        SQLStr.AppendLine("         ,LNM0016.SLCJOTARRORGCODECOND =  EXCEL.SLCJOTARRORGCODECOND")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION1 =  EXCEL.SLCARRSTATION1")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION2 =  EXCEL.SLCARRSTATION2")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION3 =  EXCEL.SLCARRSTATION3")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION4 =  EXCEL.SLCARRSTATION4")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION5 =  EXCEL.SLCARRSTATION5")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION6 =  EXCEL.SLCARRSTATION6")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION7 =  EXCEL.SLCARRSTATION7")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION8 =  EXCEL.SLCARRSTATION8")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION9 =  EXCEL.SLCARRSTATION9")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATION10 =  EXCEL.SLCARRSTATION10")
        SQLStr.AppendLine("         ,LNM0016.SLCARRSTATIONCOND =  EXCEL.SLCARRSTATIONCOND")
        SQLStr.AppendLine("         ,LNM0016.SLCARRTRUSTEECD =  EXCEL.SLCARRTRUSTEECD")
        SQLStr.AppendLine("         ,LNM0016.SLCARRTRUSTEECDCOND =  EXCEL.SLCARRTRUSTEECDCOND")
        SQLStr.AppendLine("         ,LNM0016.SLCARRTRUSTEESUBCD =  EXCEL.SLCARRTRUSTEESUBCD")
        SQLStr.AppendLine("         ,LNM0016.SLCARRTRUSTEESUBCDCOND =  EXCEL.SLCARRTRUSTEESUBCDCOND")
        SQLStr.AppendLine("         ,LNM0016.SLCSTMD =  EXCEL.SLCSTMD")
        SQLStr.AppendLine("         ,LNM0016.SLCENDMD =  EXCEL.SLCENDMD")
        SQLStr.AppendLine("         ,LNM0016.SLCSTSHIPYMD =  EXCEL.SLCSTSHIPYMD")
        SQLStr.AppendLine("         ,LNM0016.SLCENDSHIPYMD =  EXCEL.SLCENDSHIPYMD")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD1 =  EXCEL.SLCJRITEMCD1")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD2 =  EXCEL.SLCJRITEMCD2")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD3 =  EXCEL.SLCJRITEMCD3")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD4 =  EXCEL.SLCJRITEMCD4")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD5 =  EXCEL.SLCJRITEMCD5")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD6 =  EXCEL.SLCJRITEMCD6")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD7 =  EXCEL.SLCJRITEMCD7")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD8 =  EXCEL.SLCJRITEMCD8")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD9 =  EXCEL.SLCJRITEMCD9")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCD10 =  EXCEL.SLCJRITEMCD10")
        SQLStr.AppendLine("         ,LNM0016.SLCJRITEMCDCOND =  EXCEL.SLCJRITEMCDCOND")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFEE =  EXCEL.SPRUSEFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFEERATE =  EXCEL.SPRUSEFEERATE")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFEERATEROUND =  EXCEL.SPRUSEFEERATEROUND")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFEERATEADDSUB =  EXCEL.SPRUSEFEERATEADDSUB")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFEERATEADDSUBCOND =  EXCEL.SPRUSEFEERATEADDSUBCOND")
        SQLStr.AppendLine("         ,LNM0016.SPRROUNDPOINTKBN =  EXCEL.SPRROUNDPOINTKBN")
        SQLStr.AppendLine("         ,LNM0016.SPRUSEFREESPE =  EXCEL.SPRUSEFREESPE")
        SQLStr.AppendLine("         ,LNM0016.SPRNITTSUFREESENDFEE =  EXCEL.SPRNITTSUFREESENDFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRMANAGEFEE =  EXCEL.SPRMANAGEFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRSHIPBURDENFEE =  EXCEL.SPRSHIPBURDENFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRSHIPFEE =  EXCEL.SPRSHIPFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRARRIVEFEE =  EXCEL.SPRARRIVEFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRPICKUPFEE =  EXCEL.SPRPICKUPFEE")
        SQLStr.AppendLine("         ,LNM0016.SPRDELIVERYFEE =  EXCEL.SPRDELIVERYFEE")
        SQLStr.AppendLine("         ,LNM0016.SPROTHER1 =  EXCEL.SPROTHER1")
        SQLStr.AppendLine("         ,LNM0016.SPROTHER2 =  EXCEL.SPROTHER2")
        SQLStr.AppendLine("         ,LNM0016.SPRFITKBN =  EXCEL.SPRFITKBN")
        SQLStr.AppendLine("         ,LNM0016.SPRCONTRACTCD =  EXCEL.SPRCONTRACTCD")
        SQLStr.AppendLine("         ,LNM0016.BEFOREORGCODE =  EXCEL.BEFOREORGCODE")
        SQLStr.AppendLine("         ,LNM0016.BEFORESLCJOTARRORGCODE =  EXCEL.BEFORESLCJOTARRORGCODE")
        SQLStr.AppendLine("         ,LNM0016.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0016.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0016.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0016.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0016.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
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
        SQLStr.AppendLine("        ,@DEPSTATION  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,@DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@PRIORITYNO  ")
        SQLStr.AppendLine("        ,@PURPOSE  ")
        SQLStr.AppendLine("        ,@SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,@SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,@SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,@SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,@SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,@SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,@SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,@SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,@SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,@SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,@SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,@SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,@SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,@SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,@SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,@SLCSTMD  ")
        SQLStr.AppendLine("        ,@SLCENDMD  ")
        SQLStr.AppendLine("        ,@SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,@SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,@SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,@SPRUSEFEE  ")
        SQLStr.AppendLine("        ,@SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,@SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,@SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,@SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,@SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,@SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,@SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,@SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,@SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,@SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,@SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,@SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,@SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,@SPROTHER1  ")
        SQLStr.AppendLine("        ,@SPROTHER2  ")
        SQLStr.AppendLine("        ,@SPRFITKBN  ")
        SQLStr.AppendLine("        ,@SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,@BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,@BEFORESLCJOTARRORGCODE  ")
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
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_PURPOSE As MySqlParameter = SQLcmd.Parameters.Add("@PURPOSE", MySqlDbType.VarChar, 42)         '使用目的
                Dim P_SLCCTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNTYPE", MySqlDbType.VarChar, 5)         '選択比較項目-コンテナ記号
                Dim P_SLCCTNSTNO As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNSTNO", MySqlDbType.VarChar, 8)         '選択比較項目-コンテナ番号（開始）
                Dim P_SLCCTNENDNO As MySqlParameter = SQLcmd.Parameters.Add("@SLCCTNENDNO", MySqlDbType.VarChar, 8)         '選択比較項目-コンテナ番号（終了）
                Dim P_SLCJRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRDEPBRANCHCD", MySqlDbType.Int32)         '選択比較項目-ＪＲ発支社支店コード
                Dim P_SLCDEPSHIPPERCD1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD1", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード１
                Dim P_SLCDEPSHIPPERCD2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD2", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード２
                Dim P_SLCDEPSHIPPERCD3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD3", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード３
                Dim P_SLCDEPSHIPPERCD4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD4", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード４
                Dim P_SLCDEPSHIPPERCD5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCD5", MySqlDbType.VarChar, 6)         '選択比較項目-発荷主コード５
                Dim P_SLCDEPSHIPPERCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCDEPSHIPPERCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-発荷主ＣＤ比較条件
                Dim P_SLCJRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRARRBRANCHCD", MySqlDbType.Int32)         '選択比較項目-ＪＲ着支社支店コード
                Dim P_SLCJRARRBRANCHCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRARRBRANCHCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＲ着支社支店ＣＤ比較
                Dim P_SLCJOTARRORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@SLCJOTARRORGCODE", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＯＴ着組織コード
                Dim P_SLCJOTARRORGCODECOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJOTARRORGCODECOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＯＴ着組織ＣＤ比較
                Dim P_SLCARRSTATION1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION1", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード１
                Dim P_SLCARRSTATION2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION2", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード２
                Dim P_SLCARRSTATION3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION3", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード３
                Dim P_SLCARRSTATION4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION4", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード４
                Dim P_SLCARRSTATION5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION5", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード５
                Dim P_SLCARRSTATION6 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION6", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード６
                Dim P_SLCARRSTATION7 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION7", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード７
                Dim P_SLCARRSTATION8 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION8", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード８
                Dim P_SLCARRSTATION9 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION9", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード９
                Dim P_SLCARRSTATION10 As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATION10", MySqlDbType.VarChar, 6)         '選択比較項目-着駅コード１０
                Dim P_SLCARRSTATIONCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRSTATIONCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着駅コード比較条件
                Dim P_SLCARRTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEECD", MySqlDbType.VarChar, 5)         '選択比較項目-着受託人コード
                Dim P_SLCARRTRUSTEECDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEECDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着受託人ＣＤ比較条件
                Dim P_SLCARRTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '選択比較項目-着受託人サブコード
                Dim P_SLCARRTRUSTEESUBCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCARRTRUSTEESUBCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-着受託人サブＣＤ比較
                Dim P_SLCSTMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCSTMD", MySqlDbType.VarChar, 4)         '選択比較項目-開始月日
                Dim P_SLCENDMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCENDMD", MySqlDbType.VarChar, 4)         '選択比較項目-終了月日
                Dim P_SLCSTSHIPYMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCSTSHIPYMD", MySqlDbType.Date)         '選択比較項目-開始発送年月日
                Dim P_SLCENDSHIPYMD As MySqlParameter = SQLcmd.Parameters.Add("@SLCENDSHIPYMD", MySqlDbType.Date)         '選択比較項目-終了発送年月日
                Dim P_SLCJRITEMCD1 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD1", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード１
                Dim P_SLCJRITEMCD2 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD2", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード２
                Dim P_SLCJRITEMCD3 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD3", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード３
                Dim P_SLCJRITEMCD4 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD4", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード４
                Dim P_SLCJRITEMCD5 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD5", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード５
                Dim P_SLCJRITEMCD6 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD6", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード６
                Dim P_SLCJRITEMCD7 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD7", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード７
                Dim P_SLCJRITEMCD8 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD8", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード８
                Dim P_SLCJRITEMCD9 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD9", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード９
                Dim P_SLCJRITEMCD10 As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCD10", MySqlDbType.VarChar, 6)         '選択比較項目-ＪＲ品目コード１０
                Dim P_SLCJRITEMCDCOND As MySqlParameter = SQLcmd.Parameters.Add("@SLCJRITEMCDCOND", MySqlDbType.VarChar, 1)         '選択比較項目-ＪＲ品目コード比較
                Dim P_SPRUSEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-使用料金額
                Dim P_SPRUSEFEERATE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATE", MySqlDbType.Decimal, 5, 4)        '特例置換項目-使用料率
                Dim P_SPRUSEFEERATEROUND As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEROUND", MySqlDbType.VarChar, 2)         '特例置換項目-使用料率端数整理
                Dim P_SPRUSEFEERATEADDSUB As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEADDSUB", MySqlDbType.VarChar, 7)         '特例置換項目-使用料率加減額
                Dim P_SPRUSEFEERATEADDSUBCOND As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFEERATEADDSUBCOND", MySqlDbType.VarChar, 2)         '特例置換項目-使用料率加減額端数整理
                Dim P_SPRROUNDPOINTKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRROUNDPOINTKBN", MySqlDbType.VarChar, 2)         '特例置換項目-端数処理時点区分
                Dim P_SPRUSEFREESPE As MySqlParameter = SQLcmd.Parameters.Add("@SPRUSEFREESPE", MySqlDbType.VarChar, 2)         '特例置換項目-使用料無料特認
                Dim P_SPRNITTSUFREESENDFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRNITTSUFREESENDFEE", MySqlDbType.VarChar, 7)         '特例置換項目-通運負担回送運賃
                Dim P_SPRMANAGEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRMANAGEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-運行管理料
                Dim P_SPRSHIPBURDENFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRSHIPBURDENFEE", MySqlDbType.VarChar, 7)         '特例置換項目-荷主負担運賃
                Dim P_SPRSHIPFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRSHIPFEE", MySqlDbType.VarChar, 7)         '特例置換項目-発送料
                Dim P_SPRARRIVEFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRARRIVEFEE", MySqlDbType.VarChar, 7)         '特例置換項目-到着料
                Dim P_SPRPICKUPFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRPICKUPFEE", MySqlDbType.VarChar, 7)         '特例置換項目-集荷料
                Dim P_SPRDELIVERYFEE As MySqlParameter = SQLcmd.Parameters.Add("@SPRDELIVERYFEE", MySqlDbType.VarChar, 7)         '特例置換項目-配達料
                Dim P_SPROTHER1 As MySqlParameter = SQLcmd.Parameters.Add("@SPROTHER1", MySqlDbType.VarChar, 7)         '特例置換項目-その他１
                Dim P_SPROTHER2 As MySqlParameter = SQLcmd.Parameters.Add("@SPROTHER2", MySqlDbType.VarChar, 7)         '特例置換項目-その他２
                Dim P_SPRFITKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPRFITKBN", MySqlDbType.VarChar, 2)         '特例置換項目-適合区分
                Dim P_SPRCONTRACTCD As MySqlParameter = SQLcmd.Parameters.Add("@SPRCONTRACTCD", MySqlDbType.VarChar, 5)         '特例置換項目-契約コード
                Dim P_BEFOREORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFOREORGCODE", MySqlDbType.VarChar, 6)         '変換前組織コード
                Dim P_BEFORESLCJOTARRORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@BEFORESLCJOTARRORGCODE", MySqlDbType.VarChar, 6)         '変換前 選択比較項目-ＪＯＴ着組織コード
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
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位
                P_PURPOSE.Value = WW_ROW("PURPOSE")               '使用目的

                '選択比較項目-コンテナ記号
                If Not WW_ROW("SLCCTNTYPE") = "" Then
                    P_SLCCTNTYPE.Value = WW_ROW("SLCCTNTYPE")
                Else
                    P_SLCCTNTYPE.Value = DBNull.Value
                End If

                '選択比較項目-コンテナ番号（開始）
                If Not WW_ROW("SLCCTNSTNO") = "0" Then
                    P_SLCCTNSTNO.Value = WW_ROW("SLCCTNSTNO")
                Else
                    P_SLCCTNSTNO.Value = DBNull.Value
                End If

                '選択比較項目-コンテナ番号（終了）
                If Not WW_ROW("SLCCTNENDNO") = "0" Then
                    P_SLCCTNENDNO.Value = WW_ROW("SLCCTNENDNO")
                Else
                    P_SLCCTNENDNO.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ発支社支店コード
                If Not WW_ROW("SLCJRDEPBRANCHCD") = "0" Then
                    P_SLCJRDEPBRANCHCD.Value = WW_ROW("SLCJRDEPBRANCHCD")
                Else
                    P_SLCJRDEPBRANCHCD.Value = DBNull.Value
                End If

                '選択比較項目-発荷主コード１
                If Not WW_ROW("SLCDEPSHIPPERCD1") = "0" Then
                    P_SLCDEPSHIPPERCD1.Value = WW_ROW("SLCDEPSHIPPERCD1")
                Else
                    P_SLCDEPSHIPPERCD1.Value = DBNull.Value
                End If

                '選択比較項目-発荷主コード２
                If Not WW_ROW("SLCDEPSHIPPERCD2") = "0" Then
                    P_SLCDEPSHIPPERCD2.Value = WW_ROW("SLCDEPSHIPPERCD2")
                Else
                    P_SLCDEPSHIPPERCD2.Value = DBNull.Value
                End If

                '選択比較項目-発荷主コード３
                If Not WW_ROW("SLCDEPSHIPPERCD3") = "0" Then
                    P_SLCDEPSHIPPERCD3.Value = WW_ROW("SLCDEPSHIPPERCD3")
                Else
                    P_SLCDEPSHIPPERCD3.Value = DBNull.Value
                End If

                '選択比較項目-発荷主コード４
                If Not WW_ROW("SLCDEPSHIPPERCD4") = "0" Then
                    P_SLCDEPSHIPPERCD4.Value = WW_ROW("SLCDEPSHIPPERCD4")
                Else
                    P_SLCDEPSHIPPERCD4.Value = DBNull.Value
                End If

                '選択比較項目-発荷主コード５
                If Not WW_ROW("SLCDEPSHIPPERCD5") = "0" Then
                    P_SLCDEPSHIPPERCD5.Value = WW_ROW("SLCDEPSHIPPERCD5")
                Else
                    P_SLCDEPSHIPPERCD5.Value = DBNull.Value
                End If

                P_SLCDEPSHIPPERCDCOND.Value = WW_ROW("SLCDEPSHIPPERCDCOND")               '選択比較項目-発荷主ＣＤ比較条件

                '選択比較項目-ＪＲ着支社支店コード
                If Not WW_ROW("SLCJRARRBRANCHCD") = "0" Then
                    P_SLCJRARRBRANCHCD.Value = WW_ROW("SLCJRARRBRANCHCD")
                Else
                    P_SLCJRARRBRANCHCD.Value = DBNull.Value
                End If

                P_SLCJRARRBRANCHCDCOND.Value = WW_ROW("SLCJRARRBRANCHCDCOND")               '選択比較項目-ＪＲ着支社支店ＣＤ比較

                '選択比較項目-ＪＯＴ着組織コード
                If Not WW_ROW("SLCJOTARRORGCODE") = "" Then
                    P_SLCJOTARRORGCODE.Value = WW_ROW("SLCJOTARRORGCODE")
                Else
                    P_SLCJOTARRORGCODE.Value = DBNull.Value
                End If

                P_SLCJOTARRORGCODECOND.Value = WW_ROW("SLCJOTARRORGCODECOND")               '選択比較項目-ＪＯＴ着組織ＣＤ比較

                '選択比較項目-着駅コード１
                If Not WW_ROW("SLCARRSTATION1") = "0" Then
                    P_SLCARRSTATION1.Value = WW_ROW("SLCARRSTATION1")
                Else
                    P_SLCARRSTATION1.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード２
                If Not WW_ROW("SLCARRSTATION2") = "0" Then
                    P_SLCARRSTATION2.Value = WW_ROW("SLCARRSTATION2")
                Else
                    P_SLCARRSTATION2.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード３
                If Not WW_ROW("SLCARRSTATION3") = "0" Then
                    P_SLCARRSTATION3.Value = WW_ROW("SLCARRSTATION3")
                Else
                    P_SLCARRSTATION3.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード４
                If Not WW_ROW("SLCARRSTATION4") = "0" Then
                    P_SLCARRSTATION4.Value = WW_ROW("SLCARRSTATION4")
                Else
                    P_SLCARRSTATION4.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード５
                If Not WW_ROW("SLCARRSTATION5") = "0" Then
                    P_SLCARRSTATION5.Value = WW_ROW("SLCARRSTATION5")
                Else
                    P_SLCARRSTATION5.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード６
                If Not WW_ROW("SLCARRSTATION6") = "0" Then
                    P_SLCARRSTATION6.Value = WW_ROW("SLCARRSTATION6")
                Else
                    P_SLCARRSTATION6.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード７
                If Not WW_ROW("SLCARRSTATION7") = "0" Then
                    P_SLCARRSTATION7.Value = WW_ROW("SLCARRSTATION7")
                Else
                    P_SLCARRSTATION7.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード８
                If Not WW_ROW("SLCARRSTATION8") = "0" Then
                    P_SLCARRSTATION8.Value = WW_ROW("SLCARRSTATION8")
                Else
                    P_SLCARRSTATION8.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード９
                If Not WW_ROW("SLCARRSTATION9") = "0" Then
                    P_SLCARRSTATION9.Value = WW_ROW("SLCARRSTATION9")
                Else
                    P_SLCARRSTATION9.Value = DBNull.Value
                End If

                '選択比較項目-着駅コード１０
                If Not WW_ROW("SLCARRSTATION10") = "0" Then
                    P_SLCARRSTATION10.Value = WW_ROW("SLCARRSTATION10")
                Else
                    P_SLCARRSTATION10.Value = DBNull.Value
                End If


                P_SLCARRSTATIONCOND.Value = WW_ROW("SLCARRSTATIONCOND")               '選択比較項目-着駅コード比較条件

                '選択比較項目-着受託人コード
                If Not WW_ROW("SLCARRTRUSTEECD") = "0" Then
                    P_SLCARRTRUSTEECD.Value = WW_ROW("SLCARRTRUSTEECD")
                Else
                    P_SLCARRTRUSTEECD.Value = DBNull.Value
                End If

                P_SLCARRTRUSTEECDCOND.Value = WW_ROW("SLCARRTRUSTEECDCOND")               '選択比較項目-着受託人ＣＤ比較条件

                '選択比較項目-着受託人サブコード
                If Not WW_ROW("SLCARRTRUSTEESUBCD") = "0" Then
                    P_SLCARRTRUSTEESUBCD.Value = WW_ROW("SLCARRTRUSTEESUBCD")
                Else
                    P_SLCARRTRUSTEESUBCD.Value = DBNull.Value
                End If

                P_SLCARRTRUSTEESUBCDCOND.Value = WW_ROW("SLCARRTRUSTEESUBCDCOND")               '選択比較項目-着受託人サブＣＤ比較

                '選択比較項目-開始月日
                If Not WW_ROW("SLCSTMD") = "" Then
                    P_SLCSTMD.Value = WW_ROW("SLCSTMD")
                Else
                    P_SLCSTMD.Value = DBNull.Value
                End If

                '選択比較項目-終了月日
                If Not WW_ROW("SLCENDMD") = "" Then
                    P_SLCENDMD.Value = WW_ROW("SLCENDMD")
                Else
                    P_SLCENDMD.Value = DBNull.Value
                End If

                '選択比較項目-開始発送年月日
                If Not WW_ROW("SLCSTSHIPYMD") = Date.MinValue Then
                    P_SLCSTSHIPYMD.Value = WW_ROW("SLCSTSHIPYMD")
                Else
                    P_SLCSTSHIPYMD.Value = DBNull.Value
                End If

                '選択比較項目-終了発送年月日
                If Not WW_ROW("SLCENDSHIPYMD") = Date.MinValue Then
                    P_SLCENDSHIPYMD.Value = WW_ROW("SLCENDSHIPYMD")
                Else
                    P_SLCENDSHIPYMD.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード１
                If Not WW_ROW("SLCJRITEMCD1") = "0" Then
                    P_SLCJRITEMCD1.Value = WW_ROW("SLCJRITEMCD1")
                Else
                    P_SLCJRITEMCD1.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード２
                If Not WW_ROW("SLCJRITEMCD2") = "0" Then
                    P_SLCJRITEMCD2.Value = WW_ROW("SLCJRITEMCD2")
                Else
                    P_SLCJRITEMCD2.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード３
                If Not WW_ROW("SLCJRITEMCD3") = "0" Then
                    P_SLCJRITEMCD3.Value = WW_ROW("SLCJRITEMCD3")
                Else
                    P_SLCJRITEMCD3.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード４
                If Not WW_ROW("SLCJRITEMCD4") = "0" Then
                    P_SLCJRITEMCD4.Value = WW_ROW("SLCJRITEMCD4")
                Else
                    P_SLCJRITEMCD4.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード５
                If Not WW_ROW("SLCJRITEMCD5") = "0" Then
                    P_SLCJRITEMCD5.Value = WW_ROW("SLCJRITEMCD5")
                Else
                    P_SLCJRITEMCD5.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード６
                If Not WW_ROW("SLCJRITEMCD6") = "0" Then
                    P_SLCJRITEMCD6.Value = WW_ROW("SLCJRITEMCD6")
                Else
                    P_SLCJRITEMCD6.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード７
                If Not WW_ROW("SLCJRITEMCD7") = "0" Then
                    P_SLCJRITEMCD7.Value = WW_ROW("SLCJRITEMCD7")
                Else
                    P_SLCJRITEMCD7.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード８
                If Not WW_ROW("SLCJRITEMCD8") = "0" Then
                    P_SLCJRITEMCD8.Value = WW_ROW("SLCJRITEMCD8")
                Else
                    P_SLCJRITEMCD8.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード９
                If Not WW_ROW("SLCJRITEMCD9") = "0" Then
                    P_SLCJRITEMCD9.Value = WW_ROW("SLCJRITEMCD9")
                Else
                    P_SLCJRITEMCD9.Value = DBNull.Value
                End If

                '選択比較項目-ＪＲ品目コード１０
                If Not WW_ROW("SLCJRITEMCD10") = "0" Then
                    P_SLCJRITEMCD10.Value = WW_ROW("SLCJRITEMCD10")
                Else
                    P_SLCJRITEMCD10.Value = DBNull.Value
                End If

                P_SLCJRITEMCDCOND.Value = WW_ROW("SLCJRITEMCDCOND")               '選択比較項目-ＪＲ品目コード比較
                P_SPRUSEFEE.Value = WW_ROW("SPRUSEFEE")               '特例置換項目-使用料金額
                P_SPRUSEFEERATE.Value = WW_ROW("SPRUSEFEERATE")               '特例置換項目-使用料率
                P_SPRUSEFEERATEROUND.Value = WW_ROW("SPRUSEFEERATEROUND")               '特例置換項目-使用料率端数整理
                P_SPRUSEFEERATEADDSUB.Value = WW_ROW("SPRUSEFEERATEADDSUB")               '特例置換項目-使用料率加減額
                P_SPRUSEFEERATEADDSUBCOND.Value = WW_ROW("SPRUSEFEERATEADDSUBCOND")               '特例置換項目-使用料率加減額端数整理
                P_SPRROUNDPOINTKBN.Value = WW_ROW("SPRROUNDPOINTKBN")               '特例置換項目-端数処理時点区分
                P_SPRUSEFREESPE.Value = WW_ROW("SPRUSEFREESPE")               '特例置換項目-使用料無料特認
                P_SPRNITTSUFREESENDFEE.Value = WW_ROW("SPRNITTSUFREESENDFEE")               '特例置換項目-通運負担回送運賃
                P_SPRMANAGEFEE.Value = WW_ROW("SPRMANAGEFEE")               '特例置換項目-運行管理料
                P_SPRSHIPBURDENFEE.Value = WW_ROW("SPRSHIPBURDENFEE")               '特例置換項目-荷主負担運賃
                P_SPRSHIPFEE.Value = WW_ROW("SPRSHIPFEE")               '特例置換項目-発送料
                P_SPRARRIVEFEE.Value = WW_ROW("SPRARRIVEFEE")               '特例置換項目-到着料
                P_SPRPICKUPFEE.Value = WW_ROW("SPRPICKUPFEE")               '特例置換項目-集荷料
                P_SPRDELIVERYFEE.Value = WW_ROW("SPRDELIVERYFEE")               '特例置換項目-配達料
                P_SPROTHER1.Value = WW_ROW("SPROTHER1")               '特例置換項目-その他１
                P_SPROTHER2.Value = WW_ROW("SPROTHER2")               '特例置換項目-その他２
                P_SPRFITKBN.Value = WW_ROW("SPRFITKBN")               '特例置換項目-適合区分

                '特例置換項目-契約コード
                If Not WW_ROW("SPRCONTRACTCD") = "" Then
                    P_SPRCONTRACTCD.Value = WW_ROW("SPRCONTRACTCD")
                Else
                    P_SPRCONTRACTCD.Value = DBNull.Value
                End If

                '変換前組織コード
                If Not WW_ROW("BEFOREORGCODE") = "" Then
                    P_BEFOREORGCODE.Value = WW_ROW("BEFOREORGCODE")
                Else
                    P_BEFOREORGCODE.Value = DBNull.Value
                End If

                '変換前 選択比較項目-ＪＯＴ着組織コード
                If Not WW_ROW("BEFORESLCJOTARRORGCODE") = "" Then
                    P_BEFORESLCJOTARRORGCODE.Value = WW_ROW("BEFORESLCJOTARRORGCODE")
                Else
                    P_BEFORESLCJOTARRORGCODE.Value = DBNull.Value
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016_REST1M  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0016_REST1M  INSERTUPDATE"
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
        Dim WW_RATE() As String
        Dim WW_RATEChk As Boolean

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
        ' 組織コード(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "ORG", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("ORG", WW_ROW("ORGCODE"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
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
            ' 名称存在チェック
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
        ' 発駅コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPSTATION", WW_ROW("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            'CODENAME_get("STATION", WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
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
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DEPTRUSTEECD", WW_ROW("DEPTRUSTEECD"), WW_ROW("DEPSTATION"), WW_Dummy, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 発受託人サブコード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", WW_ROW("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DEPTRUSTEESUBCD", WW_ROW("DEPTRUSTEESUBCD"), WW_ROW("DEPSTATION"), WW_ROW("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人サブコードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 優先順位(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PRIORITYNO", WW_ROW("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・優先順位エラーです。"
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
        ' 選択比較項目-コンテナ記号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCCTNTYPE", WW_ROW("SLCCTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCCTNTYPE")) Then
                ' 名称存在チェック
                CODENAME_get("CTNTYPE", WW_ROW("SLCCTNTYPE"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-コンテナ番号（開始）(バリデーションチェック)
        If Not WW_ROW("SLCCTNSTNO") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCCTNSTNO", WW_ROW("SLCCTNSTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCCTNSTNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", WW_ROW("SLCCTNSTNO"), WW_ROW("SLCCTNTYPE"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        ' 選択比較項目-コンテナ番号（終了）(バリデーションチェック)
        If Not WW_ROW("SLCCTNENDNO") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCCTNENDNO", WW_ROW("SLCCTNENDNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCCTNENDNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", WW_ROW("SLCCTNENDNO"), WW_ROW("SLCCTNTYPE"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' コンテナ番号大小チェック(コンテナ番号（開始）・コンテナ番号（終了）)
        If Not WW_ROW("SLCCTNSTNO") = "0" Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCCTNSTNO")) AndAlso
                Not String.IsNullOrEmpty(WW_ROW("SLCCTNENDNO")) Then
                If CInt(WW_ROW("SLCCTNSTNO")) > CInt(WW_ROW("SLCCTNENDNO")) Then
                    WW_CheckMES1 = "・選択比較項目-コンテナ番号(開始)＆選択比較項目-コンテナ番号(終了)エラー"
                    WW_CheckMES2 = "コンテナ番号大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 選択比較項目-ＪＲ発支社支店コード(バリデーションチェック)
        If Not WW_ROW("SLCJRDEPBRANCHCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRDEPBRANCHCD", WW_ROW("SLCJRDEPBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRDEPBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("JRBRANCHCD", WW_ROW("SLCJRDEPBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ発支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ発支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主コード１(バリデーションチェック)
        If Not WW_ROW("SLCDEPSHIPPERCD1") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD1", WW_ROW("SLCDEPSHIPPERCD1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCD1")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SLCDEPSHIPPERCD1"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主コード２(バリデーションチェック)
        If Not WW_ROW("SLCDEPSHIPPERCD2") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD2", WW_ROW("SLCDEPSHIPPERCD2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCD2")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SLCDEPSHIPPERCD2"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主コード３(バリデーションチェック)
        If Not WW_ROW("SLCDEPSHIPPERCD3") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD3", WW_ROW("SLCDEPSHIPPERCD3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCD3")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SLCDEPSHIPPERCD3"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主コード４(バリデーションチェック)
        If Not WW_ROW("SLCDEPSHIPPERCD4") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD4", WW_ROW("SLCDEPSHIPPERCD4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCD4")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SLCDEPSHIPPERCD4"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主コード５(バリデーションチェック)
        If Not WW_ROW("SLCDEPSHIPPERCD5") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD5", WW_ROW("SLCDEPSHIPPERCD5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCD5")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", WW_ROW("SLCDEPSHIPPERCD5"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-発荷主ＣＤ比較条件(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCDCOND", WW_ROW("SLCDEPSHIPPERCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCDEPSHIPPERCDCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCDEPSHIPPERCDCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-発荷主ＣＤ比較条件エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-発荷主ＣＤ比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-ＪＲ着支社支店コード(バリデーションチェック)
        If Not WW_ROW("SLCJRARRBRANCHCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRARRBRANCHCD", WW_ROW("SLCJRARRBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRARRBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("JRBRANCHCD", WW_ROW("SLCJRARRBRANCHCD"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ着支社支店ＣＤ比較(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCJRARRBRANCHCDCOND", WW_ROW("SLCJRARRBRANCHCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCJRARRBRANCHCDCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCJRARRBRANCHCDCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店ＣＤ比較エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店ＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-ＪＯＴ着組織コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCJOTARRORGCODE", WW_ROW("SLCJOTARRORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCJOTARRORGCODE")) Then
                ' 名称存在チェック
                CODENAME_get("ORG", WW_ROW("SLCJOTARRORGCODE"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-ＪＯＴ着組織ＣＤ比較(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCJOTARRORGCODECOND", WW_ROW("SLCJOTARRORGCODECOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCJOTARRORGCODECOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCJOTARRORGCODECOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織ＣＤ比較エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織ＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-着駅１コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION1") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION1", WW_ROW("SLCARRSTATION1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION1")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION1"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード１エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅２コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION2") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION2", WW_ROW("SLCARRSTATION2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION2")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION2"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード２エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅３コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION3") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION3", WW_ROW("SLCARRSTATION3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION3")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION3"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード３エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅４コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION4") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION4", WW_ROW("SLCARRSTATION4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION4")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION4"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード４エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅５コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION5") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION5", WW_ROW("SLCARRSTATION5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION5")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION5"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード５エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅６コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION6") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION6", WW_ROW("SLCARRSTATION6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION6")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION6"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード６エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード６エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅７コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION7") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION7", WW_ROW("SLCARRSTATION7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION7")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION7"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード７エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード７エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅８コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION8") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION8", WW_ROW("SLCARRSTATION8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION8")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION8"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード８エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅９コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION9") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION9", WW_ROW("SLCARRSTATION9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION9")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION9"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード９エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅１０コード(バリデーションチェック)
        If Not WW_ROW("SLCARRSTATION10") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION10", WW_ROW("SLCARRSTATION10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATION10")) Then
                    ' 名称存在チェック
                    'CODENAME_get("STATION", WW_ROW("SLCARRSTATION10"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    'If Not isNormal(WW_RtnSW) Then
                    '    WW_CheckMES1 = "・選択比較項目-着駅コード１０エラーです。"
                    '    WW_CheckMES2 = "マスタに存在しません。"
                    '    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード１０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着駅コード比較条件(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCARRSTATIONCOND", WW_ROW("SLCARRSTATIONCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCARRSTATIONCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCARRSTATIONCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-着駅コード比較条件エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-着駅コード比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-着受託人コード(バリデーションチェック)
        If Not WW_ROW("SLCARRTRUSTEECD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEECD", WW_ROW("SLCARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRTRUSTEECD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEECD", WW_ROW("SLCARRTRUSTEECD"), WW_ROW("SLCARRSTATION1"), WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着受託人ＣＤ比較条件(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEECDCOND", WW_ROW("SLCARRTRUSTEECDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCARRTRUSTEECDCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCARRTRUSTEECDCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-着受託人ＣＤ比較条件エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-着受託人ＣＤ比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-着受託人サブコード(バリデーションチェック)
        If Not WW_ROW("SLCARRTRUSTEESUBCD") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEESUBCD", WW_ROW("SLCARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCARRTRUSTEESUBCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEESUBCD", WW_ROW("SLCARRTRUSTEESUBCD"), WW_ROW("SLCARRSTATION1"), WW_ROW("SLCARRTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-着受託人サブＣＤ比較(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEESUBCDCOND", WW_ROW("SLCARRTRUSTEESUBCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCARRTRUSTEESUBCDCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCARRTRUSTEESUBCDCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-着受託人サブＣＤ比較エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-着受託人サブＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 選択比較項目-開始月日を年月日(YYYY/MM/DD)に変更(2000年指定)
        If Not String.IsNullOrEmpty(WW_ROW("SLCSTMD")) Then
            If WW_ROW("SLCSTMD").ToString.Length = 3 Then
                WW_SlcStMD = DateTime.ParseExact(String.Concat("20000", WW_ROW("SLCSTMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
            ElseIf WW_ROW("SLCSTMD").ToString.Length = 4 Then
                WW_SlcStMD = DateTime.ParseExact(String.Concat("2000", WW_ROW("SLCSTMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・選択比較項目-開始月日エラーです。"
                WW_CheckMES2 = "月日ではありません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-開始月日(バリデーションチェック)
        If Not String.IsNullOrEmpty(WW_SlcStMD) Then
                Master.CheckField(Master.USERCAMP, "SLCSTMD", WW_SlcStMD, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・選択比較項目-開始月日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        ' 選択比較項目-終了月日を年月日(YYYY/MM/DD)に変更(2000年指定)
        If Not String.IsNullOrEmpty(WW_ROW("SLCENDMD")) Then
            If WW_ROW("SLCENDMD").ToString.Length = 3 Then
                WW_SlcEndMD = DateTime.ParseExact(String.Concat("20000", WW_ROW("SLCENDMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
            ElseIf WW_ROW("SLCENDMD").ToString.Length = 4 Then
                WW_SlcEndMD = DateTime.ParseExact(String.Concat("2000", WW_ROW("SLCENDMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・選択比較項目-終了月日エラーです。"
                WW_CheckMES2 = "月日ではありません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-終了月日(バリデーションチェック)
        If Not String.IsNullOrEmpty(WW_SlcEndMD) Then
            Master.CheckField(Master.USERCAMP, "SLCENDMD", WW_SlcStMD, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・選択比較項目-終了月日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        If Not WW_ROW("SLCSTSHIPYMD") = Date.MinValue Then
            ' 選択比較項目-開始発送年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCSTSHIPYMD", WW_ROW("SLCSTSHIPYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCSTSHIPYMD")) Then
                    WW_ROW("SLCSTSHIPYMD") = CDate(WW_ROW("SLCSTSHIPYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-開始発送年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-終了発送年月日(バリデーションチェック)
        If Not WW_ROW("SLCENDSHIPYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "SLCENDSHIPYMD", WW_ROW("SLCENDSHIPYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCENDSHIPYMD")) Then
                    WW_ROW("SLCENDSHIPYMD") = CDate(WW_ROW("SLCENDSHIPYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-終了発送年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 日付大小チェック(選択比較項目-開始発送年月日・終了発送年月日)
        If Not String.IsNullOrEmpty(WW_ROW("SLCSTSHIPYMD")) AndAlso
                Not String.IsNullOrEmpty(WW_ROW("SLCENDSHIPYMD")) Then
            If CDate(WW_ROW("SLCSTSHIPYMD")) > CDate(WW_ROW("SLCENDSHIPYMD")) Then
                WW_CheckMES1 = "・選択比較項目-開始発送年月日&終了発送年月日エラー"
                WW_CheckMES2 = "日付大小入力エラー"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード１(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD1") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD1", WW_ROW("SLCJRITEMCD1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD1")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD1"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード２(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD2") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD2", WW_ROW("SLCJRITEMCD2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD2")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD2"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード３(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD3") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD3", WW_ROW("SLCJRITEMCD3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD3")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD3"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード４(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD4") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD4", WW_ROW("SLCJRITEMCD4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD4")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD4"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード５(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD5") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD5", WW_ROW("SLCJRITEMCD5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD5")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD5"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード６(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD6") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD6", WW_ROW("SLCJRITEMCD6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD6")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD6"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード６エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード６エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード７(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD7") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD7", WW_ROW("SLCJRITEMCD7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD7")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD7"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード７エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード７エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード８(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD8") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD8", WW_ROW("SLCJRITEMCD8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD8")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD8"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード８エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード９(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD9") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD9", WW_ROW("SLCJRITEMCD9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD9")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD9"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード１０(バリデーションチェック)
        If Not WW_ROW("SLCJRITEMCD10") = "0" Then
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD10", WW_ROW("SLCJRITEMCD10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCD10")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", WW_ROW("SLCJRITEMCD10"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 選択比較項目-ＪＲ品目コード比較(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SLCJRITEMCDCOND", WW_ROW("SLCJRITEMCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SLCJRITEMCDCOND")) Then
                ' 名称存在チェック
                CODENAME_get("COMPARECONDKBN", WW_ROW("SLCJRITEMCDCOND"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード比較エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-使用料金額(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRUSEFEE", WW_ROW("SPRUSEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料金額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-使用料率(バリデーションチェック)
        ' 整数、小数桁数チェック(使用料率)
        WW_RATEChk = True
        WW_RATE = WW_ROW("SPRUSEFEERATE").ToString.Split(".")
        If WW_RATE(0).Length > CONST_USEFEERATE_INT_NUM Then '整数部桁数
            WW_RATEChk = False
        End If
        If WW_RATE.Length = 2 Then '小数部有
            If WW_RATE(1).Length > CONST_USEFEERATE_DEC_NUM Then '小数部桁数
                WW_RATEChk = False
            End If
        End If

        If WW_RATEChk = True Then
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATE", WW_ROW("SPRUSEFEERATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・特例置換項目-使用料率エラーです。"
            WW_CheckMES2 = "入力した数値が大きすぎます。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 入力値チェック(使用料率)
        If Not WW_ROW("SPRUSEFEERATE") = "0" Then
            If String.IsNullOrEmpty(WW_ROW("SPRUSEFEERATE")) OrElse
                WW_ROW("SPRUSEFEERATE") = "0" Then
                If String.IsNullOrEmpty(WW_ROW("SPRUSEFEE")) OrElse
                        WW_ROW("SPRUSEFEE") = "0" Then
                    ' 入力値チェック(使用料金額&使用料率)
                    WW_CheckMES1 = "・特例置換項目-使用料金額・使用料率入力エラーです。"
                    WW_CheckMES2 = "どちらかを入力してください。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(WW_ROW("SPRUSEFEERATEADDSUB")) AndAlso
                        WW_ROW("SPRUSEFEERATEADDSUB") <> "0" Then
                    ' 入力値チェック(使用料率&使用料率加減額)
                    WW_CheckMES1 = "・特例置換項目-使用料率・使用料率加減額入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率が未入力です。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If

        ' 特例置換項目-使用料率加減額(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEADDSUB", WW_ROW("SPRUSEFEERATEADDSUB"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・特例置換項目-使用料率加減額エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 特例置換項目-端数処理時点区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRROUNDPOINTKBN", WW_ROW("SPRROUNDPOINTKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SPRROUNDPOINTKBN")) Then
                ' 名称存在チェック
                CODENAME_get("HASUUPOINTKBN", WW_ROW("SPRROUNDPOINTKBN"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目-端数処理時点区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・特例置換項目-端数処理時点区分です。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 入力値チェック(使用料率&端数処理時点区分) else (使用料率加減額&端数処理時点区分)
        If String.IsNullOrEmpty(WW_ROW("SPRROUNDPOINTKBN")) Then
            If Not String.IsNullOrEmpty(WW_ROW("SPRUSEFEERATE")) OrElse
                    WW_ROW("SPRUSEFEERATE") = "0" Then
                WW_CheckMES1 = "・特例置換項目-使用料率・端数処理時点区分入力エラーです。"
                WW_CheckMES2 = "特例置換項目-端数処理時点区分が未入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf Not String.IsNullOrEmpty(WW_ROW("SPRUSEFEERATEADDSUB")) OrElse
                   WW_ROW("SPRUSEFEERATEADDSUB") = "0" Then
                WW_CheckMES1 = "・特例置換項目-使用料率加減額・端数処理時点区分入力エラーです。"
                WW_CheckMES2 = "特例置換項目-使用料率加減額が未入力です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 特例置換項目-使用料無料特認(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRUSEFREESPE", WW_ROW("SPRUSEFREESPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SPRUSEFREESPE")) Then
                ' 名称存在チェック
                CODENAME_get("USEFREEKBN", WW_ROW("SPRUSEFREESPE"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-通運負担回送運賃(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRNITTSUFREESENDFEE", WW_ROW("SPRNITTSUFREESENDFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-通運負担回送運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-運行管理料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRMANAGEFEE", WW_ROW("SPRMANAGEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-運行管理料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-荷主負担運賃(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRSHIPBURDENFEE", WW_ROW("SPRSHIPBURDENFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-荷主負担運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-発送料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRSHIPFEE", WW_ROW("SPRSHIPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-発送料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-到着料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRARRIVEFEE", WW_ROW("SPRARRIVEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-到着料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-集荷料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRPICKUPFEE", WW_ROW("SPRPICKUPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-集荷料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-配達料(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRDELIVERYFEE", WW_ROW("SPRDELIVERYFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-配達料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-その他１(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPROTHER1", WW_ROW("SPROTHER1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-その他２(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPROTHER2", WW_ROW("SPROTHER2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 特例置換項目-適合区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRFITKBN", WW_ROW("SPRFITKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SPRFITKBN")) Then
                ' 名称存在チェック
                CODENAME_get("FITKBN", WW_ROW("SPRFITKBN"), WW_Dummy, WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・特例置換項目-適合区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        ' 入力値チェック(大分類コード&特例置換項目-適合区分)
        If WW_ROW("BIGCTNCD") = "10" AndAlso
                WW_ROW("SPRFITKBN") <> "0" AndAlso
                WW_ROW("SPRFITKBN") <> "1" AndAlso
                WW_ROW("SPRFITKBN") <> "2" OrElse
                WW_ROW("BIGCTNCD") <> "10" AndAlso
                WW_ROW("SPRFITKBN") <> "0" Then
            WW_CheckMES1 = "・大分類コード・特例置換項目-適合区分入力エラーです。"
            WW_CheckMES2 = "特例置換項目-適合区分が不適切です。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 特例置換項目-契約コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPRCONTRACTCD", WW_ROW("SPRCONTRACTCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-契約コードエラーです。"
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
    Protected Sub REST1MEXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '使用料特例マスタ１に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0016_REST1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                P_ORGCODE.Value = WW_ROW("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位

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
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016_REST1M SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016_REST1M Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0095_REST1HIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
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
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
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
        SQLStr.AppendLine("        LNG.LNM0016_REST1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

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
                P_DEPSTATION.Value = WW_ROW("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = WW_ROW("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = WW_ROW("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = WW_ROW("PRIORITYNO")               '優先順位

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0095_REST1HIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0095_REST1HIST  INSERT"
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
                Case "ORG"                '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE1, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"           '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"        '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE1, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, I_VALUE2))
                Case "STATION"            '発駅コード・着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE1, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"       '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, I_VALUE2))
                Case "DEPTRUSTEESUBCD"    '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, I_VALUE2, I_VALUE3))
                Case "CTNTYPE"            'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE1, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNNO"              'コンテナ番号（開始/終了）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE1, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, I_VALUE2))
                Case "SHIPPER"            '荷主コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPER, I_VALUE1, O_TEXT, O_RTN)
                Case "ARRTRUSTEECD"       '着受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, I_VALUE2))
                Case "ARRTRUSTEESUBCD"    '着受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE1, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, I_VALUE2, I_VALUE3))
                Case "ITEM"               '品目コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ITEM, I_VALUE1, O_TEXT, O_RTN)
                Case "JRBRANCHCD",        'JR支社支店コード
                     "COMPARECONDKBN",    '比較条件区分
                     "HASUU1",            '端数区分１
                     "HASUU2",            '端数区分２
                     "HASUUPOINTKBN",     '端数時点区分
                     "USEFREEKBN",        '使用料無料区分
                     "FITKBN"             '適合区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE1, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

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

#End Region






End Class

