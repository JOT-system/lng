﻿''************************************************************
' 休日割増単価マスタメンテ変更履歴画面
' 作成日 2025/07/01
' 更新日 
' 作成者 三宅
' 更新者 
'
' 修正履歴 : 2025/07/01 新規作成
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.Drawing
Imports GrapeCity.Documents.Excel

''' <summary>
''' 休日割増単価マスタ変更履歴
''' </summary>
''' <remarks></remarks>
Public Class LNM0017HolidayRateHistory
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0017tbl As DataTable                                  '一覧格納用テーブル

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
                    Master.RecoverTable(LNM0017tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0017WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0017WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND", "LNM0017L"  '戻るボタン押下（LNM0017Lは、パンくずより）
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
                                Master.SaveTable(LNM0017tbl)
                                '〇 一覧の件数を取得
                                Me.ListCount.Text = "件数：" + LNM0017tbl.Rows.Count.ToString()
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
            If Not IsNothing(LNM0017tbl) Then
                LNM0017tbl.Clear()
                LNM0017tbl.Dispose()
                LNM0017tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0017WRKINC.MAPIDH
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
        Master.SaveTable(LNM0017tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0017tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0017tbl)

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

        If IsNothing(LNM0017tbl) Then
            LNM0017tbl = New DataTable
        End If

        If LNM0017tbl.Columns.Count <> 0 Then
            LNM0017tbl.Columns.Clear()
        End If

        LNM0017tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを予算分類変更履歴から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                        AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                        AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                        AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                                       AS OPERATION           ")
        SQLStr.AppendLine("   , LNT29.UPDTIMSTP                                                          AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , LNT29.ID                                                                 AS ID                  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.TORICODE), '')                                      AS TORICODE            ")
        SQLStr.AppendLine("   , (SELECT L06.TORINAME                                                                            ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.TORICODE = LNT29.TORICODE                                                            ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.TORINAME                                                                          ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS TORINAME                                                                                    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.ORDERORGCODE), '')                                  AS ORDERORGCODE        ")
        SQLStr.AppendLine("   , (SELECT L06.ORGNAME                                                                             ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.ORGCODE = LNT29.ORDERORGCODE                                                         ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.ORGNAME                                                                           ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS ORDERORGNAME                                                                                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.ORDERORGCATEGORY), '')                              AS ORDERORGCATEGORY    ")
        SQLStr.AppendLine("   , (CASE LNT29.ORDERORGCATEGORY                                                                    ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS ORDERORGCATEGORYNAME                                                                        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.SHUKABASHO), '')                                    AS SHUKABASHO          ")
        SQLStr.AppendLine("   , (SELECT L06.AVOCADOSHUKANAME                                                                    ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.AVOCADOSHUKABASHO = LNT29.SHUKABASHO                                                 ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.AVOCADOSHUKANAME                                                                  ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS SHUKABASHONAME                                                                              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.SHUKABASHOCATEGORY), '')                            AS SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("   , (CASE LNT29.SHUKABASHOCATEGORY                                                                  ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS SHUKABASHOCATEGORYNAME                                                                      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.TODOKECODE), '')                                    AS TODOKECODE          ")
        SQLStr.AppendLine("   , (SELECT L06.AVOCADOTODOKENAME                                                                   ")
        SQLStr.AppendLine("       FROM  LNG.LNM0006_NEWTANKA L06                                                                ")
        SQLStr.AppendLine("      WHERE L06.AVOCADOTODOKECODE = LNT29.TODOKECODE                                                 ")
        SQLStr.AppendLine("      AND   L06.DELFLG = '0'                                                                         ")
        SQLStr.AppendLine("      ORDER BY L06.AVOCADOTODOKENAME                                                                 ")
        SQLStr.AppendLine("      LIMIT  1                                                                                       ")
        SQLStr.AppendLine("    ) AS TODOKENAME                                                                                  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.TODOKECATEGORY), '')                                AS TODOKECATEGORY      ")
        SQLStr.AppendLine("   , (CASE LNT29.TODOKECATEGORY                                                                      ")
        SQLStr.AppendLine("         WHEN '1' THEN '対象'                                                                        ")
        SQLStr.AppendLine("         WHEN ''  THEN ''                                                                            ")
        SQLStr.AppendLine("         ELSE '除外'                                                                                 ")
        SQLStr.AppendLine("     END                                                                                             ")
        SQLStr.AppendLine("    ) AS TODOKECATEGORYNAME                                                                          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.RANGECODE), '')                                     AS RANGECODE           ")
        SQLStr.AppendLine("   , (SELECT GROUP_CONCAT(LNS06.VALUE1 ORDER BY LOCATE(LNS06.KEYCODE, LNT29.RANGECODE) SEPARATOR '／') ")
        SQLStr.AppendLine("        FROM COM.LNS0006_FIXVALUE LNS06                                                              ")
        SQLStr.AppendLine("       WHERE LNS06.CAMPCODE = '01'                                                                   ")
        SQLStr.AppendLine("         AND LNS06.CLASS    = 'HOLIDAYRANGE'                                                         ")
        SQLStr.AppendLine("         AND LOCATE(LNS06.KEYCODE, LNT29.RANGECODE) > 0                                              ")
        SQLStr.AppendLine("         AND CURDATE() BETWEEN LNS06.STYMD AND LNS06.ENDYMD                                          ")
        SQLStr.AppendLine("         AND LNS06.DELFLG <> '1'                                                                     ")
        SQLStr.AppendLine("       ORDER BY LNS06.KEYCODE                                                                        ")
        SQLStr.AppendLine("    ) AS RANGENAME                                                                                   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.GYOMUTANKNUMFROM), '')                              AS GYOMUTANKNUMFROM    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.GYOMUTANKNUMTO), '')                                AS GYOMUTANKNUMTO      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.TANKA),'')                                          AS TANKA               ")
        SQLStr.AppendLine("   , FORMAT(LNT29.TANKA,0)                                                    AS SRCTANKA            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.DELFLG), '')                                        AS DELFLG              ")

        SQLStr.AppendLine("   , CASE                 ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(LNT29.OPERATEKBN), '') ='2' AND COALESCE(RTRIM(LNT29.MODIFYKBN), '') ='2' THEN '変更前 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(LNT29.OPERATEKBN), '') ='2' AND COALESCE(RTRIM(LNT29.MODIFYKBN), '') ='3' THEN '変更後 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(LNT29.OPERATEKBN), '') ='3' AND COALESCE(RTRIM(LNT29.MODIFYKBN), '') ='2' THEN '変更前 削除' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(LNT29.OPERATEKBN), '') ='3' AND COALESCE(RTRIM(LNT29.MODIFYKBN), '') ='3' THEN '変更後 削除' ")
        SQLStr.AppendLine("      ELSE ''                                                                                         ")
        SQLStr.AppendLine("    END AS OPERATEKBNNAME                                                                             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.MODIFYKBN), '')                                     AS MODIFYKBN            ")
        SQLStr.AppendLine("   ,CASE COALESCE(RTRIM(LNT29.MODIFYKBN), '')                                                         ")
        SQLStr.AppendLine("      WHEN '1' THEN '新規'                                                                            ")
        SQLStr.AppendLine("      WHEN '2' THEN '変更前'                                                                          ")
        SQLStr.AppendLine("      WHEN '3' THEN '変更後'                                                                          ")
        SQLStr.AppendLine("      ELSE ''                                                                                         ")
        SQLStr.AppendLine("    END AS MODIFYKBNNAME                                                                              ")
        SQLStr.AppendLine("   , DATE_FORMAT(LNT29.MODIFYYMD, '%Y/%m/%d %T')                              AS MODIFYYMD            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT29.MODIFYUSER), '')                                    AS MODIFYUSER           ")
        SQLStr.AppendLine(" FROM                                                                                                 ")
        SQLStr.AppendLine("     LNG.LNT0029_HOLIDAYRATEHIST LNT29                                                                ")
        SQLStr.AppendLine(" WHERE                                                                                                ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            SQLStr.AppendLine("    DATE_FORMAT(LNT29.MODIFYYMD,'%Y/%m/%d')  = @MODIFYYMD                                         ")
        Else
            SQLStr.AppendLine("    DATE_FORMAT(LNT29.MODIFYYMD,'%Y/%m/01')  = @MODIFYYMD                                         ")
        End If
        '変更ユーザが指定されている場合
        If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
            SQLStr.AppendLine(" AND COALESCE(RTRIM(LNT29.MODIFYUSER), '')  =  @MODIFYUSER                                        ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                                             ")
        SQLStr.AppendLine("    MODIFYYMD DESC                                                                                    ")
        SQLStr.AppendLine("    ,TORICODE                                                                                         ")
        SQLStr.AppendLine("    ,ID                                                                                               ")
        SQLStr.AppendLine("    ,MODIFYKBN                                                                                        ")
        SQLStr.AppendLine("    ,ORDERORGCODE                                                                                     ")
        SQLStr.AppendLine("    ,SHUKABASHO                                                                                       ")
        SQLStr.AppendLine("    ,TODOKECODE                                                                                       ")

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
                        LNM0017tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0017tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0017row As DataRow In LNM0017tbl.Rows
                    i += 1
                    LNM0017row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDYRATEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0029_HOLIDYRATEHIST Select"
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
        SQLStr.AppendLine(" FROM LNG.LNT0029_HOLIDAYRATEHIST ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDAYRATEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0029_HOLIDAYRATEHIST Select"
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
        SQLStr.AppendLine(" FROM LNG.LNT0029_HOLIDAYRATEHIST ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDAYRATEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0029_HOLIDAYRATEHIST Select"
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
        SQLStr.AppendLine(" FROM LNG.LNT0029_HOLIDAYRATEHIST ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDAYRATEHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0029_HOLIDAYRATEHIST Select"
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
        For Each LNM0017row As DataRow In LNM0017tbl.Rows
            If LNM0017row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0017row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0017tbl)

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
        If WF_ButtonClick.Value = "LNM0017S" Then
            Master.MAPID = LNM0017WRKINC.MAPIDL
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
        Dim TBLview As New DataView(LNM0017tbl)
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
        WW_MAXCOL = [Enum].GetValues(GetType(LNM0017WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

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
        wb.ActiveSheet.Range("C1").Value = "休日割増単価マスタ変更履歴一覧"

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
            Case LNM0017WRKINC.FILETYPE.EXCEL
                FileName = "休日割増単価マスタ変更履歴.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                'Case LNM0017WRKINC.FILETYPE.PDF
                '    FileName = "休日割増単価マスタ変更履歴.pdf"
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

        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = "操作区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = "変更区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = "変更日時"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = "変更USER"

        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.DELFLG).Value = "削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.ID).Value = "ユニークID"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TORICODE).Value = "取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TORINAME).Value = "取引先名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCODE).Value = "受注受付部署コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGNAME).Value = "受注受付部署名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCATEGORY).Value = "受注受付部署判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCATEGORYNAME).Value = "受注受付部署判定区分名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHO).Value = "出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHONAME).Value = "出荷場所名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHOCATEGORY).Value = "出荷場所判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHOCATEGORYNAME).Value = "出荷場所判定名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECODE).Value = "届先コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKENAME).Value = "届先名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECATEGORY).Value = "届先判定区分"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECATEGORYNAME).Value = "届先判定区分名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.RANGECODE).Value = "休日範囲コード"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.RANGENAME).Value = "休日範囲名"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.GYOMUTANKNUMFROM).Value = "車番（開始）"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.GYOMUTANKNUMTO).Value = "車番（終了）"
        sheet.Cells(WW_HEADERROW, LNM0017WRKINC.HISTORYEXCELCOL.TANKA).Value = "単価"

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        '数値書式(整数)
        Dim IntStyle As IStyle = wb.Styles.Add("IntStyle")
        IntStyle.NumberFormat = "#,##0_);[Red](#,##0)"

        For Each Row As DataRow In LNM0017tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = Row("OPERATEKBNNAME") '操作区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = Row("MODIFYKBNNAME") '変更区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = Row("MODIFYYMD") '変更日時
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = Row("MODIFYUSER") '変更USER

            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.ID).Value = Row("ID") 'ユニークID
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCODE).Value = Row("ORDERORGCODE") '受注受付部署コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGNAME).Value = Row("ORDERORGNAME") '受注受付部署名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCATEGORY).Value = Row("ORDERORGCATEGORY") '受注受付部署判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.ORDERORGCATEGORYNAME).Value = Row("ORDERORGCATEGORYNAME") '受注受付部署判定区分名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHO).Value = Row("SHUKABASHO") '出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHONAME).Value = Row("SHUKABASHONAME") '出荷場所名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHOCATEGORY).Value = Row("SHUKABASHOCATEGORY") '出荷場所判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.SHUKABASHOCATEGORYNAME).Value = Row("SHUKABASHOCATEGORYNAME") '出荷場所判定名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECODE).Value = Row("TODOKECODE") '届先コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKENAME).Value = Row("TODOKENAME") '届先名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECATEGORY).Value = Row("TODOKECATEGORY") '届先判定区分
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TODOKECATEGORYNAME).Value = Row("TODOKECATEGORYNAME") '届先判定区分名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.RANGECODE).Value = Row("RANGECODE") '休日範囲コード
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.RANGENAME).Value = Row("RANGENAME") '休日範囲名
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.GYOMUTANKNUMFROM).Value = Row("GYOMUTANKNUMFROM") '車番（開始）
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.GYOMUTANKNUMTO).Value = Row("GYOMUTANKNUMTO") '車番（終了）

            '単価
            If Convert.ToString(Row("TANKA")) = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TANKA).Value = Row("TANKA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TANKA).Value = CInt(Row("TANKA"))
            End If
            '金額を数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0017WRKINC.HISTORYEXCELCOL.TANKA).Style = IntStyle

            '変更区分が変更後の行の場合
            If Row("MODIFYKBN") = LNM0017WRKINC.MODIFYKBN.AFTDATA Then
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

        WW_MAXCOL = [Enum].GetValues(GetType(LNM0017WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()
        WW_STCOL = LNM0017WRKINC.HISTORYEXCELCOL.DELFLG   '削除フラグ

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

