﻿''************************************************************
' 固定費マスタメンテ変更履歴画面
' 作成日 2025/01/20
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/01/20 新規作成
'          : 2025/05/15 統合版に変更
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.Drawing
Imports GrapeCity.Documents.Excel

''' <summary>
''' 固定費マスタ変更履歴
''' </summary>
''' <remarks></remarks>
Public Class LNM0007KoteihiHistory
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0007tbl As DataTable                                  '一覧格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    '〇 帳票用
    Private Const CONST_COLOR_HATCHING_MODIFY As String = "#FFFF00" '変更項目強調表示色(黄)
    Private Const CONST_COLOR_HATCHING_HEADER As String = "#002060" 'ヘッダ網掛け色
    Private Const CONST_COLOR_FONT_HEADER As String = "#FFFFFF" 'ヘッダフォント色
    Private Const CONST_COLOR_BLACK As String = "#000000" '黒
    Private Const CONST_COLOR_RED As String = "#FF0000" '赤

    Private Const CONST_HEIGHT_PER_ROW As Integer = 15 'セルのコメントの一行あたりの高さ
    Private Const CONST_DATA_START_ROW As Integer = 3 'データ開始行

    '〇 タブ用
    Private Const CONST_COLOR_TAB_ACTIVE As String = "#FFFFFF"　'アクティブ
    Private Const CONST_COLOR_TAB_INACTIVE As String = "#D9D9D9"  '非アクティブ

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
                    Master.RecoverTable(LNM0007tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0007WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0007WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND", "LNM0007L"  '戻るボタン押下（LNM0007Lは、パンくずより）
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_SelectMODIFYYMChange"  '変更年月フィールドチェンジ
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                '変更日取得
                                MODIFYDDGet(SQLcon)
                                '変更ユーザ取得
                                MODIFYUSERGet(SQLcon)
                            End Using
                        Case "WF_SelectMODIFYDDChange"  '変更日フィールドチェンジ
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                '変更ユーザ取得
                                MODIFYUSERGet(SQLcon)
                            End Using
                        Case "WF_ButtonMODIFYVIEW"  '表示するボタン押下
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()  ' DataBase接続
                                MAPDataGet(SQLcon)
                                Master.SaveTable(LNM0007tbl)
                                '〇 一覧の件数を取得
                                Me.ListCount.Text = "件数：" + LNM0007tbl.Rows.Count.ToString()
                            End Using
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_TARGETTABLEChange" Then
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
            If Not IsNothing(LNM0007tbl) Then
                LNM0007tbl.Clear()
                LNM0007tbl.Dispose()
                LNM0007tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0007WRKINC.MAPIDH
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

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '変更年月取得
            MODIFYYMGet(SQLcon)
            '変更日取得
            MODIFYDDGet(SQLcon)
            '変更ユーザ取得
            MODIFYUSERGet(SQLcon)
        End Using

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
            SQLcon.Open()  ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0007tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0007tbl)

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

        '変更箇所を強調表示
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ModifyHatching();", True)

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNM0007tbl) Then
            LNM0007tbl = New DataTable
        End If

        If LNM0007tbl.Columns.Count <> 0 Then
            LNM0007tbl.Columns.Clear()
        End If

        LNM0007tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを予算分類変更履歴から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                               ")
        SQLStr.AppendLine("     1                                                                            AS 'SELECT'                 ")
        SQLStr.AppendLine("   , 0                                                                            AS HIDDEN                   ")
        SQLStr.AppendLine("   , 0                                                                            AS LINECNT                  ")
        SQLStr.AppendLine("   , ''                                                                           AS OPERATION                ")
        SQLStr.AppendLine("   , UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DELFLG), '')                                                    AS DELFLG                   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TORICODE), '')                                    AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TORINAME), '')                                    AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ORGCODE), '')                                     AS ORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ORGNAME), '')                                     AS ORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KASANORGCODE), '')                                AS KASANORGCODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KASANORGNAME), '')                                AS KASANORGNAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TARGETYM), '')                                    AS TARGETYM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SYABAN), '')                                      AS SYABAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(RIKUBAN), '')                                     AS RIKUBAN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SYAGATA), '')                                     AS SYAGATA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SYAGATANAME), '')                                 AS SYAGATANAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SYABARA), '')                                     AS SYABARA              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEASONKBN), '')                                   AS SEASONKBN              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEASONSTART), '')                                 AS SEASONSTART              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEASONEND), '')                                   AS SEASONEND              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KOTEIHIM), '')                                    AS KOTEIHIM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KOTEIHID), '')                                    AS KOTEIHID              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KAISU), '')                                       AS KAISU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(GENGAKU), '')                                     AS GENGAKU              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(AMOUNT), '')                                      AS AMOUNT              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ACCOUNTCODE), '')                                 AS ACCOUNTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ACCOUNTNAME), '')                                 AS ACCOUNTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEGMENTCODE), '')                                 AS SEGMENTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEGMENTNAME), '')                                 AS SEGMENTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(JOTPERCENTAGE), '')                               AS JOTPERCENTAGE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ENEXPERCENTAGE), '')                              AS ENEXPERCENTAGE      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU3), '')                                      AS BIKOU3              ")

        '画面表示用
        '季節料金判定区分
        SQLStr.AppendLine("   , ''                                                                       AS SCRSEASONKBN        ")
        '車腹
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SYABARA), '') = '' THEN ''                                                 ")
        SQLStr.AppendLine("      ELSE  FORMAT(SYABARA,3)                                                                        ")
        SQLStr.AppendLine("     END AS SCRSYABARA                                                                               ")
        '固定費(月額)
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(KOTEIHIM), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(KOTEIHIM,0)                                                                       ")
        SQLStr.AppendLine("     END AS SCRKOTEIHIM                                                                              ")
        '固定費(日額)
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(KOTEIHID), '') = '' THEN ''                                                ")
        SQLStr.AppendLine("      ELSE  FORMAT(KOTEIHID,0)                                                                       ")
        SQLStr.AppendLine("     END AS SCRKOTEIHID                                                                              ")
        '減額費用
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GENGAKU), '') = '' THEN ''                                                 ")
        SQLStr.AppendLine("      ELSE  FORMAT(GENGAKU,0)                                                                        ")
        SQLStr.AppendLine("     END AS SCRGENGAKU                                                                               ")
        '請求額
        SQLStr.AppendLine("   , CASE                                                                                            ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(AMOUNT), '') = '' THEN ''                                                  ")
        SQLStr.AppendLine("      ELSE  FORMAT(AMOUNT,0)                                                                         ")
        SQLStr.AppendLine("     END AS SCRAMOUNT                                                                                ")
        '割合JOT
        SQLStr.AppendLine("   , ''                                                                       AS SCRJOTPERCENTAGE   ")
        '割合ENEX
        SQLStr.AppendLine("   , ''                                                                       AS SCRENEXPERCENTAGE  ")

        SQLStr.AppendLine("   , CASE                 ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='2' AND COALESCE(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='2' AND COALESCE(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='3' AND COALESCE(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 削除' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='3' AND COALESCE(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 削除' ")
        SQLStr.AppendLine("      ELSE ''                                                                                          ")
        SQLStr.AppendLine("    END AS OPERATEKBNNAME                                                                              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MODIFYKBN), '')                                                 AS MODIFYKBN                 ")
        SQLStr.AppendLine("   ,CASE COALESCE(RTRIM(MODIFYKBN), '')                                                                          ")
        SQLStr.AppendLine("      WHEN '1' THEN '新規'                                                                                     ")
        SQLStr.AppendLine("      WHEN '2' THEN '変更前'                                                                                   ")
        SQLStr.AppendLine("      WHEN '3' THEN '変更後'                                                                                   ")
        SQLStr.AppendLine("      ELSE ''                                                                                                  ")
        SQLStr.AppendLine("    END AS MODIFYKBNNAME                                                                                       ")
        'SQLStr.AppendLine("   , FORMAT(MODIFYYMD, 'yyyy/MM/dd HH:mm:ss')                                     AS MODIFYYMD                 ")
        SQLStr.AppendLine("   , DATE_FORMAT(MODIFYYMD, '%Y/%m/%d %T')                                     AS MODIFYYMD                 ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MODIFYUSER), '')                                           AS MODIFYUSER                ")
        SQLStr.AppendLine(" FROM                                                                                                          ")
        SQLStr.AppendLine("     LNG.LNT0006_FIXEDHIST                                                                                     ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/%d')  = @MODIFYYMD                                                  ")
        Else
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/01')  = @MODIFYYMD                                                  ")
        End If
        '変更ユーザが指定されている場合
        If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
            SQLStr.AppendLine(" AND COALESCE(RTRIM(MODIFYUSER), '')  =  @MODIFYUSER ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                                              ")
        SQLStr.AppendLine("    MODIFYYMD DESC                                                                                     ")
        SQLStr.AppendLine("    ,TARGETYM                                                           ")
        SQLStr.AppendLine("    ,TORICODE                                                           ")
        SQLStr.AppendLine("    ,ORGCODE                                                            ")
        SQLStr.AppendLine("    ,SYABAN                                                             ")
        SQLStr.AppendLine("    ,SEASONKBN                                                          ")
        SQLStr.AppendLine("    ,MODIFYKBN                                                          ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.VarChar, 10)         '変更年月
                '変更日が指定されている場合
                If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/" + WF_DDL_MODIFYDD.SelectedValue
                Else
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/01"
                End If

                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザ
                '変更ユーザが指定されている場合
                If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
                    P_MODIFYUSER.Value = WF_DDL_MODIFYUSER.SelectedValue
                Else
                    P_MODIFYUSER.Value = ""
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0007row As DataRow In LNM0007tbl.Rows
                    i += 1
                    LNM0007row("LINECNT") = i        'LINECNT

                    Select Case LNM0007row("SEASONKBN").ToString
                        Case "0" : LNM0007row("SCRSEASONKBN") = "通年"
                        Case "1" : LNM0007row("SCRSEASONKBN") = "夏季料金"
                        Case "2" : LNM0007row("SCRSEASONKBN") = "冬季料金"
                        Case Else : LNM0007row("SCRSEASONKBN") = ""
                    End Select

                    '割合JOT
                    Select Case LNM0007row("JOTPERCENTAGE").ToString
                        Case "" : LNM0007row("SCRJOTPERCENTAGE") = ""
                        Case Else : LNM0007row("SCRJOTPERCENTAGE") = LNM0007row("JOTPERCENTAGE").ToString & "%"
                    End Select

                    '割合ENEX
                    Select Case LNM0007row("ENEXPERCENTAGE").ToString
                        Case "" : LNM0007row("SCRENEXPERCENTAGE") = ""
                        Case Else : LNM0007row("SCRENEXPERCENTAGE") = LNM0007row("ENEXPERCENTAGE").ToString & "%"
                    End Select

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007H SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007H Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更年月取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYYMGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        'SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'yyyy/MM') AS MODIFYYM ")
        SQLStr.AppendLine("     DATE_FORMAT(MODIFYYMD, '%Y/%m') AS MODIFYYM ")
        SQLStr.AppendLine(" FROM LNG.LNT0006_FIXEDHIST ")
        SQLStr.AppendLine(" ORDER BY MODIFYYM DESC     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_CURRENTMONTH As String = Date.Now.ToString("yyyy/MM")

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYYM.Items.Clear()
                    'WF_DDL_MODIFYYM.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYYM.Items.Add(WW_ROW("MODIFYYM"))
                    Next

                    '当月がドロップダウンリストに存在しない場合追加
                    If WF_DDL_MODIFYYM.Items.FindByValue(WW_CURRENTMONTH) Is Nothing Then
                        WF_DDL_MODIFYYM.Items.Insert(0, WW_CURRENTMONTH)
                    End If

                    WF_DDL_MODIFYYM.SelectedValue = WW_CURRENTMONTH

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_FIXEDHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0006_FIXEDHIST Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更日取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYDDGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        'SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'dd') AS MODIFYDD ")
        SQLStr.AppendLine("     DATE_FORMAT(MODIFYYMD, '%d') AS MODIFYDD ")
        SQLStr.AppendLine(" FROM LNG.LNT0006_FIXEDHIST ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM')  = @MODIFYYM                                                         ")
        SQLStr.AppendLine("      DATE_FORMAT(MODIFYYMD, '%Y/%m')  = @MODIFYYM                                                         ")
        SQLStr.AppendLine(" ORDER BY MODIFYDD ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYM As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYM", MySqlDbType.VarChar, 7)         '変更年月
                P_MODIFYYM.Value = WF_DDL_MODIFYYM.SelectedValue

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYDD.Items.Clear()
                    WF_DDL_MODIFYDD.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYDD.Items.Add(WW_ROW("MODIFYDD"))
                    Next

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_FIXEDHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0006_FIXEDHIST Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更ユーザ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYUSERGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("     MODIFYUSER ")
        SQLStr.AppendLine(" FROM LNG.LNT0006_FIXEDHIST ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/dd')  = @MODIFYYMD                                                  ")
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/%d')  = @MODIFYYMD                                                  ")
        Else
            'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/01')  = @MODIFYYMD                                                  ")
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/01')  = @MODIFYYMD                                                  ")
        End If
        SQLStr.AppendLine(" ORDER BY MODIFYUSER  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.VarChar, 10)         '変更年月日
                '変更日が指定されている場合
                If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/" + WF_DDL_MODIFYDD.SelectedValue
                Else
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/01"
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYUSER.Items.Clear()
                    WF_DDL_MODIFYUSER.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYUSER.Items.Add(WW_ROW("MODIFYUSER"))
                    Next

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_FIXEDHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0006_FIXEDHIST Select"
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
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            If LNM0007row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0007row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0007tbl)

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

        '変更箇所を強調表示
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ModifyHatching();", True)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        'パンくずから検索を選択した場合
        If WF_ButtonClick.Value = "LNM0007S" Then
            Master.MAPID = LNM0007WRKINC.MAPIDL
        End If

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
        Dim TBLview As New DataView(LNM0007tbl)
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
        Dim WW_MAXCOL As Integer = 0
        WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = Left(WF_DDL_MODIFYYM.SelectedValue, 4) + "年" + Right(WF_DDL_MODIFYYM.SelectedValue, 2) + "月"

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        SetDETAIL(wb, wb.ActiveSheet, WW_ACTIVEROW)

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '明細の線を引く
        Dim WW_MAXRANGE As String = wb.ActiveSheet.Cells(WW_ACTIVEROW - 1, WW_MAXCOL).Address
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Dotted
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeLeft).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeRight).LineStyle = BorderLineStyle.Thin

        'ヘッダ設定
        SetHEADER(wb.ActiveSheet, WW_MAXCOL)

        'その他設定
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_MODIFY)
        wb.ActiveSheet.Range("B2").Value = "は変更項目"
        wb.ActiveSheet.Range("C1").Value = "固定費マスタ変更履歴一覧"

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

        Dim FileName As String = ""
        Dim FilePath As String
        Select Case WW_FILETYPE
            Case LNM0007WRKINC.FILETYPE.EXCEL
                FileName = "固定費マスタ変更履歴.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                'Case LNM0007WRKINC.FILETYPE.PDF
                '    FileName = "固定費マスタ変更履歴.pdf"
                '    FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '    '保存
                '    wb.Save(FilePath, SaveFileFormat.Pdf)

                '    'ダウンロード
                '    WF_PrintURL.Value = UrlRoot & FileName
                '    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
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
            '.Color = Color.FromArgb(0, 0, 0)
            .Name = "Meiryo UI"
            .Size = 11
        End With
        '配置
        sheet.Columns.VerticalAlignment = VerticalAlignment.Center
        'sheet.Rows.HorizontalAlignment = HorizontalAlignment.Center
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
    Public Sub SetHEADER(ByVal sheet As IWorksheet, ByVal WW_MAXCOL As Integer)
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

        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = "操作区分"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = "変更区分"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = "変更日時"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = "変更USER"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.DELFLG).Value = "削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.TORICODE).Value = "取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.ORGCODE).Value = "部門コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.ORGNAME).Value = "部門名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.TARGETYM).Value = "対象年月"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABAN).Value = "車番"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.RIKUBAN).Value = "陸事番号"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SYAGATA).Value = "車型"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SYAGATANAME).Value = "車型名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABARA).Value = "車腹"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONKBN).Value = "季節料金判定区分"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONSTART).Value = "季節料金判定開始月日"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONEND).Value = "季節料金判定終了月日"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHIM).Value = "固定費(月額)"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHID).Value = "固定費(日額)"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.KAISU).Value = "回数"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.GENGAKU).Value = "減額費用"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.AMOUNT).Value = "請求額"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.ACCOUNTCODE).Value = "勘定科目コード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.ACCOUNTNAME).Value = "勘定科目名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SEGMENTCODE).Value = "セグメントコード"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.SEGMENTNAME).Value = "セグメント名"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = "割合JOT"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = "割合ENEX"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU1).Value = "備考1"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU2).Value = "備考2"
        sheet.Cells(WW_HEADERROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU3).Value = "備考3"

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

        For Each Row As DataRow In LNM0007tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = Row("OPERATEKBNNAME") '操作区分
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = Row("MODIFYKBNNAME") '変更区分
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = Row("MODIFYYMD") '変更日時
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = Row("MODIFYUSER") '変更USER
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.TARGETYM).Value = Row("TARGETYM") '対象年月	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABAN).Value = Row("SYABAN") '車番	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.RIKUBAN).Value = Row("RIKUBAN") '陸事番号	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYAGATA).Value = Row("SYAGATA") '車型	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYAGATANAME).Value = Row("SYAGATANAME") '車型名	

            '車腹
            If Row("SYABARA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABARA).Value = Row("SYABARA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABARA).Value = CDbl(Row("SYABARA"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONKBN).Value = Row("SEASONKBN") '季節料金判定区分	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONSTART).Value = Row("SEASONSTART") '季節料金判定開始月日	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SEASONEND).Value = Row("SEASONEND") '季節料金判定終了月日	

            '固定費(月額)
            If Row("KOTEIHIM") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHIM).Value = Row("KOTEIHIM")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHIM).Value = CDbl(Row("KOTEIHIM"))
            End If

            '固定費(日額)
            If Row("KOTEIHID") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHID).Value = Row("KOTEIHID")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHID).Value = CDbl(Row("KOTEIHID"))
            End If

            '回数
            If Row("KAISU") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KAISU).Value = Row("KAISU")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KAISU).Value = CDbl(Row("KAISU"))
            End If

            '減額費用
            If Row("GENGAKU") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.GENGAKU).Value = Row("GENGAKU")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.GENGAKU).Value = CDbl(Row("GENGAKU"))
            End If

            '請求額
            If Row("AMOUNT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.AMOUNT).Value = Row("AMOUNT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.AMOUNT).Value = CDbl(Row("AMOUNT"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ACCOUNTCODE).Value = Row("ACCOUNTCODE") '勘定科目コード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '勘定科目名
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SEGMENTCODE).Value = Row("SEGMENTCODE") 'セグメントコード
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SEGMENTNAME).Value = Row("SEGMENTNAME") 'セグメント名

            '割合JOT
            If Row("JOTPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = Row("JOTPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = CDbl(Row("JOTPERCENTAGE"))
            End If

            '割合ENEX
            If Row("ENEXPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = Row("ENEXPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = CDbl(Row("ENEXPERCENTAGE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2	
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3	

            '金額を数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.SYABARA).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHIM).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KOTEIHID).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.KAISU).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.GENGAKU).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.AMOUNT).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0007WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Style = DecStyle

            '変更区分が変更後の行の場合
            If Row("MODIFYKBN") = LNM0007WRKINC.MODIFYKBN.AFTDATA Then
                '変更箇所を塗りつぶし
                SetMODIFYHATCHING(sheet, WW_ACTIVEROW)
            End If

            WW_ACTIVEROW += 1
        Next
    End Sub

    ''' <summary>
    ''' 変更箇所を塗りつぶし
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetMODIFYHATCHING(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)
        '最大列(RANGE)、変更チェック開始列を取得
        Dim WW_MAXCOL As Integer = 0
        Dim WW_STCOL As Integer = 0

        WW_MAXCOL = [Enum].GetValues(GetType(LNM0007WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()
        WW_STCOL = LNM0007WRKINC.HISTORYEXCELCOL.DELFLG   '削除フラグ

        '開始列から最大列まで変更前後の値を確認
        For index As Integer = WW_STCOL To WW_MAXCOL
            '変更前と変更後が不一致の場合
            If Not Convert.ToString(sheet.Cells(WW_ACTIVEROW - 1, index).Value) = Convert.ToString(sheet.Cells(WW_ACTIVEROW, index).Value) Then

                '変更後の背景色を塗りつぶし
                sheet.Cells(WW_ACTIVEROW, index).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_MODIFY)

                '変更後のフォント色を変える
                sheet.Cells(WW_ACTIVEROW, index).Font.Color = ColorTranslator.FromHtml(CONST_COLOR_RED)
                sheet.Cells(WW_ACTIVEROW, index).Font.Bold = True

            End If
        Next
    End Sub
#End Region

End Class

