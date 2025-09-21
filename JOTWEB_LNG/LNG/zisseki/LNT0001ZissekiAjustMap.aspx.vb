''************************************************************
' 調整画面管理
' 作成日 2025/04/10
' 更新日 
' 作成者 
' 更新者 
'
' 修正履歴 
''************************************************************
Imports GrapeCity.Documents.Excel
Imports Newtonsoft.Json
Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports System.Drawing

Public Class LNT0001ZissekiAjustMap_aspx
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                 '実績（アボカド）データ格納用テーブル
    Private LNT0030tbl As DataTable                                 'サーチャージ料金データ格納用テーブル
    Private LNT0031tbl As DataTable                                 '実勢単価履歴データ格納用テーブル
    Private WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 15                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 15                 'マウススクロール時稼働行数
    Private Const CONST_TANKAADJUST As String = "単価調整"          '「単価調整」文言
    Private Const CONST_SURCHARGE As String = "サーチャージ"        '「サーチャージ」文言
    Private Const CONST_LOCK As String = "<div><img id=""imgLock{0}"" src=""../img/lockkey.png"" width=""20px"" height=""20px"" /></div>"
    Private Const CONST_UNLOCK As String = "<div><img id=""imgLock{0}"" src=""../img/unlockkey.png"" width=""20px"" height=""20px"" /></div>"
    Private Const CONST_ROUND As Integer = 1                        '四捨五入
    Private Const CONST_FLOOR As Integer = 2                        '切り捨て
    Private Const CONST_CEILING As Integer = 3                      '切り上げ

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザー情報取得
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              '固定値マスタ
    Private CMNPTS As New CmnParts                                  '共通関数

    '○ 共通処理結果
    Private WW_ErrSW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード
    Private WW_GridPositionARROW As Integer

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

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Select Case Me.WF_TARGETTABLE.SelectedItem.Text
                        Case CONST_TANKAADJUST
                            '単価調整（実績データ）
                            Master.RecoverTable(LNT0001tbl)
                            '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                            If CS0013ProfView.SetDispListTextBoxValues(LNT0001tbl, pnlListArea) Then
                                Master.SaveTable(LNT0001tbl)
                            End If
                        Case CONST_SURCHARGE
                            'サーチャージ（サーチャージ料金データ）
                            Master.RecoverTable(LNT0030tbl, WF_XMLsaveF30.Value)
                            Master.RecoverTable(LNT0031tbl, WF_XMLsaveF31.Value)
                            '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                            If CS0013ProfView.SetDispListTextBoxValues(LNT0030tbl, pnlListArea) Then
                                Master.SaveTable(LNT0030tbl, WF_XMLsaveF30.Value)       'サーチャージ用
                            End If
                    End Select
                    '★戻り値(初期化)
                    WW_ErrSW = ""
                    WW_GridPositionARROW = 0
                    Dim WW_SURCHARGEUPD As Boolean = False   'サーチャージでの更新ボタン押下有無を判定するための変数
                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"                    'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"                   '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"               '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonREFLECT"                     '反映ボタン押下
                            WF_ButtonREFLECT_Click()
                        Case "WF_ButtonPAGE",                       'ページボタン押下時処理
                             "WF_ButtonFIRST",
                             "WF_ButtonPREVIOUS",
                             "WF_ButtonNEXT",
                             "WF_ButtonLAST"
                            Me.WF_ButtonPAGE_Click()
                        Case "WF_ButtonUPDATE"                      '保存ボタンクリック
                            WF_ButtonUPDATE(WW_SURCHARGEUPD)
                        Case "WF_ButtonCLEAR", "LNT0002L"           '戻るボタンクリック
                            WF_ButtonEND_Click()
                        Case "WF_SelectCALENDARChange"              'カレンダー変更時
                            WF_TARGETTABLEInitialize()
                        Case "WF_TARGETTABLEChange"                 '対象選択クリック
                            WF_TARGETTABLEInitialize()
                        Case "WF_ButtonSearch"                      '検索ボタンクリック
                            WF_ButtonSearch_Click()
                        Case "WF_MouseWheelUp"
                            Me.WF_ButtonPAGE_Click()
                        Case "WF_MouseWheelDown"
                            Me.WF_ButtonPAGE_Click()
                        Case "WF_ButtonRelease"                     '解除ボタンクリック
                            WF_ButtonRelease_Click()
                        Case "WF_Field_DBClick"                     'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ListboxDBclick"                    '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                         '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListChange"                        'リスト変更
                            WF_ListChange()
                        Case "WF_ButtonPDF"                         '請求書プレビュー
                            WF_EXCELPDF(LNT0001WRKINC.FILETYPE.PDF)
                    End Select
                    If WW_ErrSW = "ERR" _
                        OrElse WF_ButtonClick.Value = "WF_ButtonSearch" _
                        OrElse WF_ButtonClick.Value = "WF_ButtonRelease" _
                        OrElse WF_ButtonClick.Value = "WF_TARGETTABLEChange" _
                        OrElse WF_ButtonClick.Value = "WF_SelectCALENDARChange" _
                        OrElse WW_SURCHARGEUPD = True Then
                        '※一覧再表示処理[未実施]
                    Else
                        '○ 一覧再表示処理
                        DisplayGrid(gridPosition:=WW_GridPositionARROW)
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
            If Not IsNothing(LNT0001tbl) Then
                LNT0001tbl.Clear()
                LNT0001tbl.Dispose()
                LNT0001tbl = Nothing
            End If
            If Not IsNothing(LNT0030tbl) Then
                LNT0030tbl.Clear()
                LNT0030tbl.Dispose()
                LNT0030tbl = Nothing
            End If
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
        Master.MAPID = LNT0001WRKINC.MAPIDAJ
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()
        '○ サーチャージ情報保存先のファイル名
        WW_CreateXMLSaveFile("LNT0030")
        WW_CreateXMLSaveFile("LNT0031")

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

        ''○ GridView初期設定
        'GridViewInitialize()

        ''〇 更新画面からの遷移もしくは、アップロード完了の場合、更新完了メッセージを出力
        'If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
        '    Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
        '    work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        'End If
    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '★ドロップダウンリスト（調整種類）作成
        Dim dtAjustType As New DataTable
        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
        GS0007FIXVALUElst.CLAS = "AJUSTTYPE"
        GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = "KEYCODE ASC"
        dtAjustType = GS0007FIXVALUElst.GS0007FIXVALUETbl()
        If Not isNormal(GS0007FIXVALUElst.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "固定値取得エラー")
            Exit Sub
        End If
        '〇対象(調整種類)
        setDDLItem(dtAjustType, "KEYCODE", "VALUE1", Me.WF_TARGETTABLE)

        '★対象年月
        WF_TaishoYm.Value = work.WF_SEL_TARGETYM.Text
        WF_TaishoYmhdn.Value = work.WF_SEL_TARGETYM.Text
        '★フィルタ設定(日)
        setDDLDay(yyyyMM:=WF_TaishoYm.Value)

        '★ドロップダウンリスト（単価(枝番)）作成
        Dim dtBranchCodeType As New DataTable
        CMNPTS.SelectNewTanka_BRANCHCODE(work.WF_SEL_TORICODE.Text, WF_TaishoYm.Value & "/01", dtBranchCodeType)
        setDDLItem(dtBranchCodeType, "BRANCHCODE", "BRANCHCODE", Me.WF_BRANCHCODE, blankFlg:=False)

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            '------------------------------------------
            '〇実績データ取得
            '------------------------------------------
            setZisseki(SQLcon)

            '------------------------------------------
            '〇サージャージ料金データ取得（とりあえず枠を作る？）
            '------------------------------------------
            setSurcharge(SQLcon)
            setDieselprice(SQLcon)

        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)                            '単価調整用
        Master.SaveTable(LNT0030tbl, WF_XMLsaveF30.Value)       'サーチャージ用
        Master.SaveTable(LNT0031tbl, WF_XMLsaveF31.Value)       '実勢価格履歴

        '★届先
        setDDLListItem(LNT0001tbl, "TODOKECODE", "TODOKENAME", Me.ddlTODOKE)
        '★陸事番号
        setDDLListItem(LNT0001tbl, "TANKNUMBER", "TANKNUMBER", Me.ddlTANKNUMBER)
        '★業務車番
        setDDLListItem(LNT0001tbl, "GYOMUTANKNUM", "GYOMUTANKNUM", Me.ddlGYOMUTANKNUM)

        '〇検索エリアを非表示
        'Me.pnlSpecialFEEArea.Visible = False
        Me.pnlPriceArea.Visible = False
        'Me.pnlFixedCostsArea.Visible = False
        Me.pnlSurchargeArea.Visible = False

        '〇 表示中ページ
        Me.WF_NOWPAGECNT.Text = "1"
        '〇 最終ページ
        Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNT0001tbl.Rows.Count) / CONST_DISPROWCOUNT)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

    End Sub

    ''' <summary>
    ''' フィルタ(日付)設定
    ''' </summary>
    Private Sub setDDLDay(Optional yyyyMM As String = Nothing)
        '★フィルタ設定(日)
        Dim resDayFirst As New List(Of ListItem)
        Dim resDayEnd As New List(Of ListItem)
        Me.ddlDayFirst.Items.Clear()
        Me.ddlDayEnd.Items.Clear()

        '◯月の末日を取得
        Dim lastDay As String = Now.ToString("yyyy/MM") + "/01"
        If Not IsNothing(yyyyMM) Then
            lastDay = yyyyMM + "/01"
        End If
        lastDay = Date.Parse(lastDay).AddMonths(1).AddDays(-1).ToString("dd")
        For iDay = 1 To Integer.Parse(lastDay) Step 1
            resDayFirst.Add(New ListItem(iDay.ToString("00"), iDay.ToString("00")))
            resDayEnd.Add(New ListItem(iDay.ToString("00"), iDay.ToString("00")))
        Next
        Me.ddlDayFirst.Items.AddRange(resDayFirst.ToArray)
        Me.ddlDayEnd.Items.AddRange(resDayEnd.ToArray)
        Me.ddlDayEnd.SelectedValue = lastDay

    End Sub

    Private Sub setDDLItem(ByVal dt As DataTable, ByVal ItemCode As String, ByVal ItemaName As String, ByRef ddlList As DropDownList, Optional blankFlg As Boolean = True)

        Dim resTrainFlagList As New List(Of ListItem)
        Dim itemList = From wrkitm In dt Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By g Select g, h
        'Dim itemList = From wrkitm In LNT0001tbl Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By CDec(g) Select g, h
        ddlList.Items.Clear()
        resTrainFlagList = New List(Of ListItem)
        If blankFlg = True Then resTrainFlagList.Add(New ListItem("", ""))
        For Each itemLists In itemList
            resTrainFlagList.Add(New ListItem(itemLists.h, itemLists.g))
        Next
        ddlList.Items.AddRange(resTrainFlagList.ToArray)

    End Sub

    Private Sub setDDLListItem(ByVal dt As DataTable, ByVal ItemCode As String, ByVal ItemaName As String, ByRef ddlList As ListBox)

        Dim resTrainFlagList As New List(Of ListItem)
        Dim itemList = From wrkitm In dt Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By g Select g, h
        'Dim itemList = From wrkitm In LNT0001tbl Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By CDec(g) Select g, h
        ddlList.Items.Clear()
        resTrainFlagList = New List(Of ListItem)
        'resTrainFlagList.Add(New ListItem("", ""))
        For Each itemLists In itemList
            resTrainFlagList.Add(New ListItem(itemLists.h, itemLists.g))
        Next
        ddlList.Items.AddRange(resTrainFlagList.ToArray)

    End Sub

    Private Sub setZisseki(ByVal SQLcon As MySqlConnection,
                           Optional ByVal WF_TODOKE As String = Nothing,
                           Optional ByVal WF_TANKNUMBER As String = Nothing,
                           Optional ByVal WF_GYOMUTANKNO As String = Nothing)
        If IsNothing(LNT0001tbl) Then
            LNT0001tbl = New DataTable
        End If
        If LNT0001tbl.Columns.Count <> 0 Then
            LNT0001tbl.Columns.Clear()
        End If
        LNT0001tbl.Clear()

        Dim SQLStr As String = CMNPTS.SelectZissekiSQL(work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                                                       WF_TODOKE:=WF_TODOKE, WF_TANKNUMBER:=WF_TANKNUMBER, WF_GYOMUTANKNO:=WF_GYOMUTANKNO, WF_TORIORG_MAP:=work.WF_SEL_ORGCODE_MAP.Text)

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar)  '部署
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)  '届日FROM
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)  '届日TO
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar)  '前月
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar)  '取引先コード
                PARA1.Value = work.WF_SEL_ORGCODE.Text
                'If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                '    PARA2.Value = WF_TaishoYm.Value & "/01"
                '    PARA3.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                'Else
                '    PARA2.Value = Date.Now.ToString("yyyy/MM") & "/01"
                '    PARA3.Value = Date.Now.ToString("yyyy/MM") & DateTime.DaysInMonth(Date.Now.Year, Date.Now.Month).ToString("/00")
                'End If
                PARA2.Value = WF_TaishoYm.Value & "/" & ddlDayFirst.SelectedValue
                PARA3.Value = WF_TaishoYm.Value & "/" & ddlDayEnd.SelectedValue
                Dim lastMonth As String = Date.Parse(Me.WF_TaishoYm.Value + "/01").AddMonths(-1).ToString("yyyy/MM")
                PARA4.Value = lastMonth
                PARA5.Value = work.WF_SEL_TORICODE.Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0001row As DataRow In LNT0001tbl.Rows
                    i += 1
                    LNT0001row("LINECNT") = i        'LINECNT
                    LNT0001row("TANKNUMBER") = Replace(LNT0001row("TANKNUMBER").ToString(), Space(1), String.Empty)
                Next
            End Using

            '★届先
            setDDLListItem(LNT0001tbl, "TODOKECODE", "TODOKENAME", Me.ddlTODOKE)
            '★陸事番号
            setDDLListItem(LNT0001tbl, "TANKNUMBER", "TANKNUMBER", Me.ddlTANKNUMBER)
            '★業務車番
            setDDLListItem(LNT0001tbl, "GYOMUTANKNUM", "GYOMUTANKNUM", Me.ddlGYOMUTANKNUM)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001AJ SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001AJ Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '★単価マスタより(単価調整)取得
        Dim dtTankaAjust As New DataTable
        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
        GS0007FIXVALUElst.CLAS = "NEWTANKA"
        '★条件(開始～終了)
        GS0007FIXVALUElst.ADDITIONAL_FROM_TO = WF_TaishoYm.Value + "/01"
        '★条件
        '-- 取引先コード
        GS0007FIXVALUElst.ADDITIONAL_CONDITION = String.Format(" AND VALUE2 = '{0}' ", work.WF_SEL_TORICODE.Text)
        '-- 単価区分"1"(調整単価)
        GS0007FIXVALUElst.ADDITIONAL_CONDITION &= " AND VALUE19 = '1' "
        dtTankaAjust = GS0007FIXVALUElst.GS0007FIXVALUETbl()

        For Each dtTankaAjustrow As DataRow In dtTankaAjust.Rows
            Dim condition As String = " TORICODE='{0}' AND ORDERORGCODE='{1}' AND TODOKECODE='{2}' "
            '取引コード
            Dim toriCode As String = dtTankaAjustrow("VALUE2").ToString()
            '部署コード
            Dim orgCode As String = dtTankaAjustrow("VALUE4").ToString()
            '届先コード
            Dim avocadoTodokeCode As String = dtTankaAjustrow("VALUE8").ToString()
            condition = String.Format(condition, toriCode, orgCode, avocadoTodokeCode)

            Dim gyomuTankNo As String = dtTankaAjustrow("VALUE11").ToString()
            If (toriCode = BaseDllConst.CONST_TORICODE_0132800000 _
                AndAlso orgCode <> BaseDllConst.CONST_ORDERORGCODE_020104) _
                OrElse toriCode = BaseDllConst.CONST_TORICODE_0110600000 _
                OrElse toriCode = BaseDllConst.CONST_TORICODE_0238900000 Then
                '業務車番
                condition &= String.Format(" AND GYOMUTANKNUM='{0}' ", dtTankaAjustrow("VALUE10").ToString())
            End If

            '単価(枝番)
            condition &= String.Format(" AND BRANCHCODE='{0}' ", dtTankaAjustrow("KEYCODE").ToString())

            For Each LNT0001row As DataRow In LNT0001tbl.Select(condition)
                LNT0001row("BRANCHNAME") = dtTankaAjustrow("VALUE1").ToString()
            Next

        Next

    End Sub

    Private Sub setSurchargeZisseki(ByVal SQLcon As MySqlConnection,
                            ByVal StTodokeDate As String,
                            ByVal EndTodokeDate As String,
                            ByRef oTBL As DataTable)
        If IsNothing(oTBL) Then
            oTBL = New DataTable
        End If
        If oTBL.Columns.Count <> 0 Then
            oTBL.Columns.Clear()
        End If
        oTBL.Clear()

        Dim SQLStr As String = String.Format(CMNPTS.SelectSurcgargeZissekiSQL(), work.WF_SEL_ORGCODE.Text)              '部門コード（カンマ区切り複数部署：請求書出力にかかわる部署全て）

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_STTODOKEDATE As MySqlParameter = SQLcmd.Parameters.Add("@STTODOKEDATE", MySqlDbType.Date)         '届日（開始）
                Dim P_ENDTODOKEDATE As MySqlParameter = SQLcmd.Parameters.Add("@ENDTODOKEDATE", MySqlDbType.Date)       '届日（終了）

                P_TORICODE.Value = work.WF_SEL_TORICODE.Text
                P_STTODOKEDATE.Value = StTodokeDate
                P_ENDTODOKEDATE.Value = EndTodokeDate

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        oTBL.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    oTBL.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001_ZISSEKI SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001_ZISSEKI Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    Private Sub setSurcharge(ByVal SQLcon As MySqlConnection,
                           Optional ByVal WF_SURCHRGEPATTERNCODE As String = "")
        If IsNothing(LNT0030tbl) Then
            LNT0030tbl = New DataTable
        End If
        If LNT0030tbl.Columns.Count <> 0 Then
            LNT0030tbl.Columns.Clear()
        End If
        LNT0030tbl.Clear()

        Dim SQLStr As String = String.Format(CMNPTS.SelectSurchargeSQL(), work.WF_SEL_ORGCODE.Text)                     '部門コード（カンマ区切り複数部署：請求書出力にかかわる部署全て）

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar, 6)           '請求年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                'Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar, 2)     'パターンコード

                P_CAMPCODE.Value = Master.USERCAMP
                P_SEIKYUYM.Value = WF_TaishoYm.Value.Replace("/", "")
                If work.WF_SEL_TORICODE.Text = BaseDllConst.CONST_TORICODE_0110600000 Then
                    '★シーエナジーの場合、北陸エルネスとする
                    P_TORICODE.Value = BaseDllConst.CONST_TORICODE_0238900000
                Else
                    P_TORICODE.Value = work.WF_SEL_TORICODE.Text
                End If

                'P_PATTERNCODE.Value = WF_SURCHRGEPATTERNCODE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0030tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0030tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    i += 1
                    LNT0030row("LINECNT") = i        'LINECNT
                    If LNT0030row("DISTANCEUPDFLG") = "0" OrElse
                       LNT0030row("SHIPPINGCOUNTUPDFLG") = "0" OrElse
                       LNT0030row("FUELRESULTUPDFLG") = "0" Then
                        LNT0030row("OPERATION") = "1"
                    End If
                    LNT0030row("DIESELPRICEROUNDLEN") = If(LNT0030row("DIESELPRICEROUNDLEN") - 1 > 0, LNT0030row("DIESELPRICEROUNDLEN") - 1, 0)      '実勢単価端数処理（桁数）
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    Private Sub setDieselprice(ByVal SQLcon As MySqlConnection)
        If IsNothing(LNT0031tbl) Then
            LNT0031tbl = New DataTable
        End If
        If LNT0031tbl.Columns.Count <> 0 Then
            LNT0031tbl.Columns.Clear()
        End If
        LNT0031tbl.Clear()

        Dim SQLStr As String = String.Format(CMNPTS.SelectDieselpriceSQL(), work.WF_SEL_ORGCODE.Text)                                       '部門コード（カンマ区切り複数部署：請求書出力にかかわる部署全て）

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                Dim P_STTARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@STTARGETYEAR", MySqlDbType.VarChar, 4)                       '対象年From
                Dim P_ENDTARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@ENDTARGETYEAR", MySqlDbType.VarChar, 4)                     '対象年To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)                              '取引先コード

                P_CAMPCODE.Value = Master.USERCAMP
                Dim stYear As String = CInt(Left(WF_TaishoYm.Value, 4)) - 1
                Dim endYear As String = Left(WF_TaishoYm.Value, 4)
                P_STTARGETYEAR.Value = stYear
                P_ENDTARGETYEAR.Value = endYear
                If work.WF_SEL_TORICODE.Text = BaseDllConst.CONST_TORICODE_0110600000 Then
                    '★シーエナジーの場合、北陸エルネスとする
                    P_TORICODE.Value = BaseDllConst.CONST_TORICODE_0238900000
                Else
                    P_TORICODE.Value = work.WF_SEL_TORICODE.Text
                End If

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
                        LNT0031row("LOCKFLGBTN") = String.Format(CONST_UNLOCK, i)
                    Else
                        LNT0031row("LOCKFLGBTN") = String.Format(CONST_LOCK, i)
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

        Dim dtSurchargepttern As New DataTable

    End Sub
    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        Dim TBLview As DataView = New DataView
        Dim TBLview2 As DataView = New DataView

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case Me.WF_TARGETTABLE.SelectedItem.Text
                Case CONST_TANKAADJUST
                    '-------------------------------------------
                    '単価調整（実績データ）
                    '-------------------------------------------
                    'setZisseki(SQLcon)
                    setZisseki(SQLcon,
                       WF_TODOKE:=WF_TODOKECODEhdn.Value,
                       WF_TANKNUMBER:=WF_TANKNUMBERhdn.Value,
                       WF_GYOMUTANKNO:=WF_GYOMUTANKNOhdn.Value)
                    '○ 画面表示データ保存
                    Master.SaveTable(LNT0001tbl)
                    '○ 一覧表示データ編集(性能対策)
                    TBLview = New DataView(LNT0001tbl)

                Case CONST_SURCHARGE
                    '-------------------------------------------
                    'サーチャージ（サーチャージ料金データ）
                    '-------------------------------------------
                    setSurcharge(SQLcon)

                    '-------------------------------------------
                    'サーチャージ（実勢単価履歴料金データ）
                    '-------------------------------------------
                    setDieselprice(SQLcon)

                    '-------------------------------------------
                    'サーチャージ計算
                    '-------------------------------------------
                    SurchargeCalc(SQLcon)

                    '○ 画面表示データ保存
                    Master.SaveTable(LNT0030tbl, WF_XMLsaveF30.Value)       'サーチャージ用
                    '○ 一覧表示データ編集(性能対策)
                    TBLview = New DataView(LNT0030tbl)

                    '○ 画面表示データ保存
                    Master.SaveTable(LNT0031tbl, WF_XMLsaveF31.Value)
                    '○ 一覧表示データ編集(性能対策)
                    TBLview2 = New DataView(LNT0031tbl)
                Case Else
                    Exit Sub
            End Select
        End Using

        '〇 一覧の件数を取得
        'Me.ListCount.Text = "件数：" + LNT0002tbl.Rows.Count.ToString()

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        '----------------------------------------------------------
        '単価調整、サーチャージの共有画面
        '----------------------------------------------------------
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        'CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.MAPID = work.WF_SEL_CONTROLTYPE.Text
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '----------------------------------------------------------
        'サーチャージの場合、実勢単価履歴画面表示用（２段表示）
        '----------------------------------------------------------
        If Me.WF_TARGETTABLE.SelectedItem.Text = CONST_SURCHARGE Then
            Dim WW_MAPID As String = LNT0001WRKINC.MAPIDAJD
            CS0013ProfView.CAMPCODE = Master.USERCAMP
            CS0013ProfView.PROFID = Master.PROF_VIEW
            'CS0013ProfView.MAPID = Master.MAPID
            CS0013ProfView.MAPID = WW_MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview2.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea2
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
            CS0013ProfView.LEVENT = "ondblclick"
            'CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()
            If Not isNormal(CS0013ProfView.ERR) Then
                Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
                Exit Sub
            End If
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

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid(Optional ByVal gridPosition As Integer = Nothing)
        Dim WW_GridPosition As Integer = gridPosition   '表示位置(開始)
        Dim WW_DataCNT As Integer = 0                   '(絞り込み後)有効Data数

        Dim TBLview As DataView = New DataView
        Dim TBLview2 As DataView = New DataView

        '○ 画面(GridView)表示
        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST
                '----------------------------
                '単価調整
                '----------------------------
                For Each LNT0001row As DataRow In LNT0001tbl.Rows
                    If LNT0001row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0001row("SELECT") = WW_DataCNT
                    End If
                Next
                TBLview = New DataView(LNT0001tbl)
            Case CONST_SURCHARGE
                '----------------------------
                'サーチャージ
                '----------------------------
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    If LNT0030row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0030row("SELECT") = WW_DataCNT
                    End If
                Next
                TBLview = New DataView(LNT0030tbl)

                For Each LNT0031row As DataRow In LNT0031tbl.Rows
                    If LNT0031row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0031row("SELECT") = WW_DataCNT
                    End If
                Next
                TBLview2 = New DataView(LNT0031tbl)
            Case Else
                Exit Sub
        End Select

        '○ 表示対象行カウント(絞り込み対象)

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

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If


        '○ ソート
        TBLview.Sort = "LINECNT"
        'TBLview.RowFilter = "SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        'CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.MAPID = work.WF_SEL_CONTROLTYPE.Text
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView_TEST.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '----------------------------------------------------------
        'サーチャージの場合、実勢単価履歴画面表示用（２段表示）
        '----------------------------------------------------------
        If Me.WF_TARGETTABLE.SelectedItem.Text = CONST_SURCHARGE Then
            Dim WW_MAPID As String = LNT0001WRKINC.MAPIDAJD
            CS0013ProfView.CAMPCODE = Master.USERCAMP
            CS0013ProfView.PROFID = Master.PROF_VIEW
            'CS0013ProfView.MAPID = Master.MAPID
            CS0013ProfView.MAPID = WW_MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview2.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea2
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
            CS0013ProfView.LEVENT = "ondblclick"
            'CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()
            If Not isNormal(CS0013ProfView.ERR) Then
                Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
                Exit Sub
            End If
        End If

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
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        'チェックボックス判定
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If LNT0001tbl.Rows(i)("OPERATIONCB") = "on" Then
                    LNT0001tbl.Rows(i)("OPERATIONCB") = ""
                Else
                    LNT0001tbl.Rows(i)("OPERATIONCB") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0001tbl.Rows(i)("OPERATIONCB") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0001tbl.Rows(i)("OPERATIONCB") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 反映ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonREFLECT_Click()

        Dim Msg As String = ""
        If LNT0001tbl.Select("OPERATIONCB='on'").Count = 0 Then
            Msg = "明細が選択されていません。ご確認ください。"
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
            Exit Sub
        End If

        Dim dtTankaInfo As New DataTable
        Dim todokeCodeHozon As String = ""
        Dim gyomuTanknumHozon As String = ""
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select("OPERATIONCB='on'", "TORICODE,ORDERORGCODE,TODOKECODE,GYOMUTANKNUM")
            '〇初回、または届先が変更になった場合
            If todokeCodeHozon = "" _
                OrElse todokeCodeHozon <> LNT0001tblrow("TODOKECODE").ToString() _
                OrElse gyomuTanknumHozon <> LNT0001tblrow("GYOMUTANKNUM").ToString() Then
                '★単価情報取得
                GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
                GS0007FIXVALUElst.CLAS = "NEWTANKA"
                GS0007FIXVALUElst.ADDITIONAL_CONDITION =
                " AND VALUE2 = '" + LNT0001tblrow("TORICODE").ToString() & "'" &            '取扱店コード
                " AND VALUE4 = '" + LNT0001tblrow("ORDERORGCODE").ToString() & "'" &        '部門コード
                " AND VALUE8 = '" + LNT0001tblrow("TODOKECODE").ToString() & "'"            '実績届先コード
                If LNT0001tblrow("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0132800000 _
                                    AndAlso LNT0001tblrow("ORDERORGCODE").ToString() <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION &=
                    " AND VALUE10 = '" + LNT0001tblrow("GYOMUTANKNUM").ToString() & "'"     '業務車番
                End If
                '★条件
                If WF_BRANCHCODE.SelectedValue <> "1" Then
                    '-- 単価区分"1"(調整単価)
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION &= " AND VALUE19 = '1' "
                End If
                '-- 単価(※設定した枝番)
                GS0007FIXVALUElst.ADDITIONAL_CONDITION &= String.Format(" AND KEYCODE = '{0}' ", WF_BRANCHCODE.SelectedValue)
                '★データ取得
                dtTankaInfo = GS0007FIXVALUElst.GS0007FIXVALUETbl()
                If Not isNormal(GS0007FIXVALUElst.ERR) Then
                    'Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "単価情報取得エラー")
                    Continue For
                End If
            End If

            '★入力した値が単価マスタに存在するか確認
            Dim condition As String = " KEYCODE='{0}' "
            condition = String.Format(condition, WF_BRANCHCODE.SelectedValue)
            If dtTankaInfo.Select(condition).Count = 0 Then
                Msg = "対象の届先は、選択した単価(枝番)は存在しません。"
                Msg &= String.Format("<br>届先【{0}】届先名【{1}】", LNT0001tblrow("TODOKECODE").ToString(), LNT0001tblrow("TODOKENAME").ToString())
                Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
                Exit For
            End If
            For Each dtTankaInforow As DataRow In dtTankaInfo.Select(condition)
                LNT0001tblrow("OPERATION") = "1"
                LNT0001tblrow("BRANCHCODE") = dtTankaInforow("KEYCODE")
                LNT0001tblrow("BRANCHNAME") = dtTankaInforow("VALUE1")
            Next

            '★届先コード保管
            todokeCodeHozon = LNT0001tblrow("TODOKECODE").ToString()
            '★業務車番保管
            gyomuTanknumHozon = LNT0001tblrow("GYOMUTANKNUM").ToString()

        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' ページボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_ButtonPAGE_Click()

        'Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim intLineNo As Integer = 0
        Dim intPage As Integer = 0

        Select Case WF_ButtonClick.Value
            'Case "WF_ButtonPAGE"            '指定ページボタン押下
            '    intPage = CInt(Me.TxtPageNo.Text.PadLeft(5, "0"c))
            '    If intPage < 1 Then
            '        intPage = 1
            '    End If
            Case "WF_ButtonFIRST"           '先頭ページボタン押下
                intPage = 1
            Case "WF_ButtonPREVIOUS",       '前ページボタン押下
                 "WF_MouseWheelDown"
                intPage = CInt(Me.WF_NOWPAGECNT.Text)
                If intPage > 1 Then
                    intPage += -1
                End If
            Case "WF_ButtonNEXT",           '次ページボタン押下
                 "WF_MouseWheelUp"
                intPage = CInt(Me.WF_NOWPAGECNT.Text)
                If intPage < CInt(Me.WF_TOTALPAGECNT.Text) Then
                    intPage += 1
                End If
            Case "WF_ButtonLAST"            '最終ページボタン押下
                intPage = CInt(Me.WF_TOTALPAGECNT.Text)
        End Select
        Me.WF_NOWPAGECNT.Text = intPage.ToString

        If WF_ButtonClick.Value = "WF_MouseWheelDown" _
            OrElse WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            '※マウス操作については
            '　後続処理で計算するため、ここでは未実施
        Else
            If intPage = 1 Then
                WW_GridPositionARROW = 1
            Else
                WW_GridPositionARROW = (intPage - 1) * CONST_SCROLLCOUNT + 1
            End If
            WF_GridPosition.Text = WW_GridPositionARROW
        End If

        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST
                '--------------------------
                '単価調整
                '--------------------------
                For Each LNT0001row As DataRow In LNT0001tbl.Rows
                    If LNT0001row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0001row("SELECT") = WW_DataCNT
                    End If
                Next
            Case CONST_SURCHARGE
                '--------------------------
                'サーチャージ
                '--------------------------
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    If LNT0030row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0030row("SELECT") = WW_DataCNT
                    End If
                Next
                For Each LNT0031row As DataRow In LNT0031tbl.Rows
                    If LNT0031row("HIDDEN") = 0 Then
                        WW_DataCNT += 1
                        ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                        LNT0031row("SELECT") = WW_DataCNT
                    End If
                Next
        End Select
    End Sub

    ''' <summary>
    ''' 保存ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE(ByRef iSURCHARGE As Boolean)

        Dim Msg = ""
        If WF_TARGETTABLE.SelectedValue = "" Then
            Msg = "対象から調整する内容を選択してください。"
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
            WW_ErrSW = "ERR"
            Exit Sub
        End If

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST
                '〇変更対象が存在するか確認
                If LNT0001tbl.Select("OPERATION='1'").Count = 0 Then
                    Msg = "単価調整の変更はありません。"
                    Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
                    Exit Sub
                End If

                '〇変更対象(更新)
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    For Each LNT0001row As DataRow In LNT0001tbl.Select("OPERATION='1'")
                        Dim condition As String = " RECONO = '{0}' AND ORDERORG = '{1}' "
                        condition = String.Format(condition,
                                                  LNT0001row("RECONO").ToString(),
                                                  LNT0001row("ORDERORG").ToString())

                        CMNPTS.UpdateTableCRT(SQLcon, "LNG.LNT0001_ZISSEKI", condition,
                                              "BRANCHCODE", LNT0001row("BRANCHCODE").ToString())

                        '★変更対象(初期化)
                        LNT0001row("OPERATION") = ""
                    Next

                End Using

                '○ 画面表示データ保存
                Master.SaveTable(LNT0001tbl)

            Case CONST_SURCHARGE
                iSURCHARGE = True

                '〇変更対象が存在するか確認
                If LNT0030tbl.Select("OPERATION='1'").Count = 0 Then
                    Msg = "サーチャージ料金の変更はありません。"
                    Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
                    Exit Sub
                End If

                '〇変更対象(更新)
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    For Each LNT0030row As DataRow In LNT0030tbl.Select("OPERATION='1'")
                        Dim condition As String = "    SEIKYUYM = '{0}' " _
                                                & "AND SEIKYUBRANCH = '{1}' " _
                                                & "AND SEIKYUDATEFROM = '{2}' " _
                                                & "AND SEIKYUDATETO = '{3}' " _
                                                & "AND TORICODE = '{4}' " _
                                                & "AND ORGCODE = '{5}' " _
                                                & "AND PATTERNCODE = '{6}' " _
                                                & "AND AVOCADOSHUKABASHO = '{7}' " _
                                                & "AND AVOCADOTODOKECODE = '{8}' " _
                                                & "AND SHAGATA = '{9}' " _
                                                & "AND SHABARA = {10} " _
                                                & "And SHABAN = '{11}'"
                        condition = String.Format(condition,
                                                  LNT0030row("SEIKYUYM"),
                                                  LNT0030row("SEIKYUBRANCH"),
                                                  LNT0030row("SEIKYUDATEFROM"),
                                                  LNT0030row("SEIKYUDATETO"),
                                                  LNT0030row("TORICODE"),
                                                  LNT0030row("ORGCODE"),
                                                  LNT0030row("PATTERNCODE"),
                                                  LNT0030row("AVOCADOSHUKABASHO"),
                                                  LNT0030row("AVOCADOTODOKECODE"),
                                                  LNT0030row("SHAGATA"),
                                                  LNT0030row("SHABARA"),
                                                  LNT0030row("SHABAN")
                                                 )

                        Dim upditem As String = " DISTANCE = {0}, SHIPPINGCOUNT = {1}, FUELRESULT = {2}, UPDYMD = '{3}', UPDUSER = '{4}', UPDTERMID = '{5}', UPDPGID = '{6}'"
                        upditem = String.Format(upditem,
                                                LNT0030row("DISTANCE"),
                                                LNT0030row("SHIPPINGCOUNT"),
                                                LNT0030row("FUELRESULT"),
                                                Date.Now,
                                                Master.USERID,
                                                Master.USERTERMID,
                                                Me.GetType().BaseType.Name
                                               )
                        CMNPTS.UpdateTableCRT2(SQLcon, "LNG.LNT0030_SURCHARGEFEE", condition, upditem)

                        '★変更対象(初期化)
                        LNT0030row("OPERATION") = ""
                    Next
                End Using

                '○ 画面表示データ保存
                'Master.SaveTable(LNT0030tbl, WF_XMLsaveF30.Value)       'サーチャージ用
                GridViewInitialize()

            Case Else
                Exit Sub
        End Select


    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '★対象年月
        work.WF_SEL_TARGETYM.Text = WF_TaishoYm.Value

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 対象選択クリック
    ''' </summary>
    Private Sub WF_TARGETTABLEInitialize()
        '〇検索エリアを非表示
        Me.pnlPriceArea.Visible = False
        Me.pnlSurchargeArea.Visible = False

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST
                '-----------------------------
                '単価調整
                '-----------------------------
                work.WF_SEL_CONTROLTYPE.Text = LNT0001WRKINC.MAPIDAJ
                Me.headtitle.InnerText = "実績単価調整画面"
                Me.pnlListArea2.Visible = False
                Me.pnlPriceArea.Visible = True
                '〇対象年月(変更)
                If WF_TaishoYm.Value <> WF_TaishoYmhdn.Value Then
                    '★フィルタ設定(日)
                    setDDLDay(yyyyMM:=WF_TaishoYm.Value)
                    WF_TaishoYmhdn.Value = WF_TaishoYm.Value
                    '〇最初ページ(初期化)
                    Me.WF_NOWPAGECNT.Text = "1"
                End If
                '○ GridView初期設定
                GridViewInitialize()
                '〇 最終ページ(再取得)
                Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNT0001tbl.Rows.Count) / CONST_DISPROWCOUNT)

            Case CONST_SURCHARGE
                '-----------------------------
                'サーチャージ
                '-----------------------------
                work.WF_SEL_CONTROLTYPE.Text = LNT0001WRKINC.MAPIDAJS
                Me.headtitle.InnerText = "サーチャージ画面"
                Me.pnlListArea2.Visible = True
                Me.pnlSurchargeArea.Visible = True
                '○ GridView初期設定
                GridViewInitialize()
            Case Else
                Exit Sub
        End Select

    End Sub

    ''' <summary>
    ''' 検索ボタン押下
    ''' </summary>
    Private Sub WF_ButtonSearch_Click()

        Dim todokeCode As String = ""

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST
                SetConditionTankaAjust()

                Dim dayFirstSelectIndex = ddlDayFirst.SelectedIndex
                Dim dayEndSelectIndex = ddlDayEnd.SelectedIndex

                If WF_TaishoYm.Value <> WF_TaishoYmhdn.Value Then
                    '★フィルタ設定(日)
                    setDDLDay(yyyyMM:=WF_TaishoYm.Value)
                    WF_TaishoYmhdn.Value = WF_TaishoYm.Value
                End If

                Dim msg As String = ""
                If Integer.Parse(ddlDayFirst.SelectedValue) > Integer.Parse(ddlDayEnd.SelectedValue) Then
                    msg = "届日の指定(開始と終了)が逆転しています。確認をお願いします。"
                    Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=msg, needsPopUp:=True)
                    WW_ErrSW = "ERR"
                    Exit Sub
                End If

            Case CONST_SURCHARGE
                Me.pnlSurchargeArea.Visible = True
            Case Else
                Exit Sub
        End Select

        '○ GridView初期設定
        GridViewInitialize()

        '〇 最初ページ(初期化)
        Me.WF_NOWPAGECNT.Text = "1"
        '〇 最終ページ(再取得)
        Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNT0001tbl.Rows.Count) / CONST_DISPROWCOUNT)

    End Sub

    ''' <summary>
    ''' 解除ボタン押下
    ''' </summary>
    Private Sub WF_ButtonRelease_Click()
        '〇選択内容初期化
        '届先
        Me.WF_TODOKECODEhdn.Value = ""
        '陸事番号
        Me.WF_TANKNUMBERhdn.Value = ""
        '業務車番
        Me.WF_GYOMUTANKNOhdn.Value = ""
        '★フィルタ設定(日)
        setDDLDay(yyyyMM:=WF_TaishoYm.Value)
        WF_TaishoYmhdn.Value = WF_TaishoYm.Value
        '○ GridView初期設定
        GridViewInitialize()

        '〇 最初ページ(初期化)
        Me.WF_NOWPAGECNT.Text = "1"
        '〇 最終ページ(再取得)
        Me.WF_TOTALPAGECNT.Text = Math.Floor((CONST_DISPROWCOUNT + LNT0001tbl.Rows.Count) / CONST_DISPROWCOUNT)

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR

                    Case Else   '以外
                        '会社コード
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "BRANCHCODE", "BRANCHNAME"
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = "01"
                                '○ LINECNT取得
                                Dim WW_LINECNT As Integer = 0
                                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                                '○ 対象ヘッダー取得
                                Dim updHeader = LNT0001tbl.AsEnumerable.
                                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                                If IsNothing(updHeader) Then Exit Sub
                                '★条件(開始～終了)
                                prmData.Item(C_PARAMETERS.LP_ADDITINALFROMTO) = WF_TaishoYm.Value + "/01"
                                '★条件(その他)
                                prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) =
                                    " AND VALUE19 = '1'" &                                            '単価用途(単価調整)
                                    " AND VALUE2 = '" + updHeader("TORICODE").ToString() & "'" &      '取扱店コード
                                    " AND VALUE4 = '" + updHeader("ORDERORGCODE").ToString() & "'" &  '部門コード
                                    " AND VALUE8 = '" + updHeader("TODOKECODE").ToString() & "'"      '実績届先コード

                                If updHeader("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0132800000 _
                                    AndAlso updHeader("ORDERORGCODE").ToString() <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                                    '★石油資源開発(本州)の場合
                                    prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) &=
                                    " AND VALUE10 = '" + updHeader("GYOMUTANKNUM").ToString() & "'"   '業務車番
                                ElseIf updHeader("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0110600000 _
                                    OrElse updHeader("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0238900000 Then
                                    '★シーエナジー(またはエルネス)の場合
                                    prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) &=
                                    " AND VALUE10 = '" + updHeader("GYOMUTANKNUM").ToString() & "'"   '業務車番
                                ElseIf updHeader("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0175400000 Then
                                    '★東北電力の場合
                                    prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) &=
                                    " AND VALUE10 = '" + updHeader("GYOMUTANKNUM").ToString() & "'"   '業務車番
                                End If

                                WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_BRANCHCODE
                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, prmData)
                        .ActiveListBox()
                End Select
            End With

        End If
    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case CONST_TANKAADJUST

                '○ 対象ヘッダー取得
                Dim updHeader = LNT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧の件数取得
                Dim intListCnt As Integer = LNT0001tbl.Rows.Count

                '○ 設定項目取得
                '対象フォーム項目取得
                Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
                'Dim GetValue() As String = WW_GetValue
                Dim dtTankaInfo As New DataTable

                Select Case WF_FIELD.Value
                    Case "BRANCHCODE"
                        '★単価情報取得
                        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
                        GS0007FIXVALUElst.CLAS = "NEWTANKA"
                        GS0007FIXVALUElst.ADDITIONAL_CONDITION =
                        " AND VALUE2 = '" + updHeader("TORICODE").ToString() & "'" &            '取扱店コード
                        " AND VALUE4 = '" + updHeader("ORDERORGCODE").ToString() & "'" &        '部門コード
                        " AND VALUE8 = '" + updHeader("TODOKECODE").ToString() & "'"            '実績届先コード
                        If updHeader("TORICODE").ToString() = BaseDllConst.CONST_TORICODE_0132800000 _
                                            AndAlso updHeader("ORDERORGCODE").ToString() <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                            GS0007FIXVALUElst.ADDITIONAL_CONDITION &=
                            " AND VALUE10 = '" + updHeader("GYOMUTANKNUM").ToString() & "'"     '業務車番
                        End If
                        '★条件
                        '-- 単価区分"1"(調整単価)
                        GS0007FIXVALUElst.ADDITIONAL_CONDITION &= " AND VALUE19 = '1' "

                        dtTankaInfo = GS0007FIXVALUElst.GS0007FIXVALUETbl()
                        If Not isNormal(GS0007FIXVALUElst.ERR) Then
                            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "単価情報取得エラー")
                            Exit Sub
                        End If

                        ' 画面入力テーブル項目設定
                        Dim WW_NRow = dtTankaInfo.NewRow
                        WW_NRow("KEYCODE") = "1"
                        WW_NRow("VALUE1") = ""
                        dtTankaInfo.Rows.Add(WW_NRow)

                        '★入力した値が単価マスタに存在するか確認
                        Dim condition As String = " KEYCODE='{0}' "
                        condition = String.Format(condition, WW_ListValue)
                        If dtTankaInfo.Select(condition).Count = 0 Then
                            Exit Select
                        End If
                        For Each dtTankaInforow As DataRow In dtTankaInfo.Select(condition)
                            updHeader("OPERATION") = "1"
                            updHeader("BRANCHCODE") = dtTankaInforow("KEYCODE")
                            updHeader("BRANCHNAME") = dtTankaInforow("VALUE1")
                        Next

                End Select

                '○ 画面表示データ保存
                Master.SaveTable(LNT0001tbl)

            Case CONST_SURCHARGE
                '○ 対象ヘッダー取得
                Dim updHeader = LNT0030tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '○ 設定項目取得
                '対象フォーム項目取得
                Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
                If String.IsNullOrEmpty(WW_ListValue) Then WW_ListValue = 0

                Dim WW_CS0024FCheckerr As String = ""
                Dim WW_CS0024FCheckReport As String = ""
                Select Case WF_FIELD.Value
                    Case "DISTANCE"
                        updHeader("OPERATION") = "1"
                        '距離(バリデーションチェック)
                        Master.CheckField(Master.USERCAMP, "DISTANCE", WW_ListValue, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                        If isNormal(WW_CS0024FCheckerr) Then
                            updHeader("DISTANCE") = WW_ListValue
                        Else
                            Exit Sub
                        End If
                    Case "SHIPPINGCOUNT"
                        updHeader("OPERATION") = "1"
                        '輸送回数(バリデーションチェック)
                        Master.CheckField(Master.USERCAMP, "SHIPPINGCOUNT", WW_ListValue, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                        If isNormal(WW_CS0024FCheckerr) Then
                            updHeader("SHIPPINGCOUNT") = WW_ListValue
                        Else
                            Exit Sub
                        End If
                    Case "FUELRESULT"
                        updHeader("OPERATION") = "1"
                        '燃料使用量(バリデーションチェック)
                        Master.CheckField(Master.USERCAMP, "FUELRESULT", updHeader("FUELRESULT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                        If isNormal(WW_CS0024FCheckerr) Then
                            updHeader("FUELRESULT") = WW_ListValue
                        Else
                            Exit Sub
                        End If
                End Select
                'サーチャージ計算
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    SurchargeCalc(SQLcon)
                End Using

                '○ 画面表示データ保存
                Master.SaveTable(LNT0030tbl, WF_XMLsaveF30.Value)       'サーチャージ用
        End Select
    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.ActiveViewIdx = 2 Then
            '■ LeftBoxマルチ対応 - START
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            WW_SelectText = selectedLeftTableVal("VALUE1")
            '■ LeftBoxマルチ対応 - END
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "BRANCHCODE"
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(LNT0001tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = LNT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                If WW_SETVALUE = "" Then
                    updHeader.Item("OPERATION") = "1"
                    updHeader.Item("BRANCHCODE") = "1"
                    updHeader.Item("BRANCHNAME") = ""
                Else
                    updHeader.Item("OPERATION") = "1"
                    updHeader.Item("BRANCHCODE") = WW_SETVALUE
                    updHeader.Item("BRANCHNAME") = WW_SETTEXT
                End If

                '○ 画面表示データ保存
                Master.SaveTable(LNT0001tbl)

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' リストボックス選択内容取得(単価調整)
    ''' </summary>
    Private Sub SetConditionTankaAjust()
        '★届先取得
        'Me.WF_TODOKECODEhdn.Value = ""
        'Me.WF_TODOKENAMEhdn.Value = ""
        If Me.ddlTODOKE.Items.Count > 0 Then
            Dim SelectedCount As Integer = 0
            Dim intSelCnt As Integer = 0
            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To ddlTODOKE.Items.Count - 1
                If ddlTODOKE.Items(index).Selected = True Then
                    SelectedCount += 1
                    If intSelCnt = 0 Then
                        Me.WF_TODOKECODEhdn.Value = ddlTODOKE.Items(index).Value
                        'Me.WF_TODOKENAMEhdn.Value = ddlTODOKE.Items(index).Text
                        intSelCnt = 1
                    Else
                        Me.WF_TODOKECODEhdn.Value = Me.WF_TODOKECODEhdn.Value & "," & ddlTODOKE.Items(index).Value
                        'Me.WF_TODOKENAMEhdn.Value = Me.WF_TODOKENAMEhdn.Value & "," & ddlTODOKE.Items(index).Text
                        intSelCnt = 2
                    End If
                End If
            Next
        End If

        '★陸事番号取得
        'Me.WF_TANKNUMBERhdn.Value = ""
        If Me.ddlTANKNUMBER.Items.Count > 0 Then
            Dim SelectedCount As Integer = 0
            Dim intSelCnt As Integer = 0
            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To ddlTANKNUMBER.Items.Count - 1
                If ddlTANKNUMBER.Items(index).Selected = True Then
                    SelectedCount += 1
                    If intSelCnt = 0 Then
                        Me.WF_TANKNUMBERhdn.Value = "'" & ddlTANKNUMBER.Items(index).Value & "'"
                        intSelCnt = 1
                    Else
                        Me.WF_TANKNUMBERhdn.Value = Me.WF_TANKNUMBERhdn.Value & "," & "'" & ddlTANKNUMBER.Items(index).Value & "'"
                        intSelCnt = 2
                    End If
                End If
            Next
        End If

        '★業務車番取得
        'Me.WF_GYOMUTANKNOhdn.Value = ""
        If Me.ddlGYOMUTANKNUM.Items.Count > 0 Then
            Dim SelectedCount As Integer = 0
            Dim intSelCnt As Integer = 0
            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To ddlGYOMUTANKNUM.Items.Count - 1
                If ddlGYOMUTANKNUM.Items(index).Selected = True Then
                    SelectedCount += 1
                    If intSelCnt = 0 Then
                        Me.WF_GYOMUTANKNOhdn.Value = ddlGYOMUTANKNUM.Items(index).Value
                        intSelCnt = 1
                    Else
                        Me.WF_GYOMUTANKNOhdn.Value = Me.WF_GYOMUTANKNOhdn.Value & "," & ddlGYOMUTANKNUM.Items(index).Value
                        intSelCnt = 2
                    End If
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' サーチャージ計算
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SurchargeCalc(ByVal SQLcon As MySqlConnection)

        Dim LNT0030CALCtbl As DataTable = LNT0030tbl.Clone

        '--------------------------------------------
        '実績データ（輸送回数）取得(LNT0001)
        '--------------------------------------------
        Dim oldSeikyuDate As String = ""
        Dim LNT0001SELtbl As DataTable = New DataTable
        For Each LNT0030row As DataRow In LNT0030tbl.Rows
            '１回だけで良いはず（実績データを取引先、該当部署、届日（開始～終了）で一旦全て取得する）
            If oldSeikyuDate <> LNT0030row("SEIKYUDATEFROM") Then
                Dim workTbl As DataTable = New DataTable
                setSurchargeZisseki(SQLcon, LNT0030row("SEIKYUDATEFROM"), LNT0030row("SEIKYUDATETO"), workTbl)
                LNT0001SELtbl.Merge(workTbl)
            End If
            oldSeikyuDate = LNT0030row("SEIKYUDATEFROM")
        Next

        '--------------------------------------------
        '月別に分解（FROM月～TO月の分）
        '--------------------------------------------
        For Each LNT0030row As DataRow In LNT0030tbl.Rows
            If CDate(LNT0030row("SEIKYUDATEFROM")).ToString("yyyyMM") = CDate(LNT0030row("SEIKYUDATETO")).ToString("yyyyMM") Then
                'そのままコピー（開始、終了が同一月の場合）
                Dim LNT0030CALCrow As DataRow = LNT0030CALCtbl.NewRow
                LNT0030CALCrow.ItemArray = LNT0030row.ItemArray
                LNT0030CALCtbl.Rows.Add(LNT0030CALCrow)
            Else
                ' 例　LNT0030tbl：2025/10　2025/07/01～2025/09/30の場合、以下の３件に増幅するし、月別に実績をぶつける
                '                 2025/10　2025/07/01～2025/07/31　他の項目は同じ
                '                 2025/10　2025/08/01～2025/08/31　他の項目は同じ
                '                 2025/10　2025/09/01～2025/09/30　他の項目は同じ

                Dim Wyear As Integer = CDate(LNT0030row("SEIKYUDATETO")).Year - CDate(LNT0030row("SEIKYUDATEFROM")).Year
                Dim Wmonth As Integer = CDate(LNT0030row("SEIKYUDATETO")).Month - CDate(LNT0030row("SEIKYUDATEFROM")).Month
                Dim loopCnt As Integer = Wyear * 12 + Wmonth

                For j As Integer = 0 To loopCnt
                    Dim LNT0030CALCrow As DataRow = LNT0030CALCtbl.NewRow
                    LNT0030CALCrow.ItemArray = LNT0030row.ItemArray
                    LNT0030CALCrow("SEIKYUDATEFROM") = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(j).ToString("yyyy/MM/dd")
                    LNT0030CALCrow("SEIKYUDATETO") = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(j).ToString("yyyy/MM") &
                                                  DateTime.DaysInMonth(CDate(LNT0030CALCrow("SEIKYUDATEFROM")).Year, CDate(LNT0030CALCrow("SEIKYUDATEFROM")).Month).ToString("/00")
                    LNT0030CALCrow("SHIPPINGCOUNT") = 0
                    LNT0030CALCrow("FUELRESULT") = 0
                    LNT0030CALCrow("SURCHARGE") = 0
                    LNT0030CALCtbl.Rows.Add(LNT0030CALCrow)
                Next
            End If
        Next

        '--------------------------------------------
        '月別に輸送回数を設定
        '--------------------------------------------
        Dim query = LNT0001SELtbl.AsEnumerable()
        Dim result As IEnumerable(Of Object) = Nothing

        'サーチャージ料金別に輸送回数を取得
        For Each LNT0030CALCrow As DataRow In LNT0030CALCtbl.Rows
            'サーチャージパタンにより集計を切り替える
            Select Case LNT0030CALCrow("PATTERNCODE")
                Case "01"
                    '荷主単位
                    result = From row In LNT0001SELtbl.AsEnumerable()
                             Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                   row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                   row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE")
                             Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                          TORICODE = row.Field(Of String)("TORICODE") Into Group
                             Select New With {
                                        .ORDERORG = ORDERORG,
                                        .TORICODE = TORICODE,
                                        .KAISU = Group.Count()
                                    }
                Case "02"
                    '届先単位
                    If String.IsNullOrEmpty(LNT0030CALCrow("AVOCADOSHUKABASHO")) Then
                        '出荷場所指定なし
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("TODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              TODOKECODE = row.Field(Of String)("TODOKECODE") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .TODOKECODE = TODOKECODE,
                                            .KAISU = Group.Count()
                                        }
                    Else
                        '出荷場所指定あり
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("SHUKABASHO") = LNT0030CALCrow("AVOCADOSHUKABASHO") AndAlso
                                       row.Field(Of String)("TODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                                              TODOKECODE = row.Field(Of String)("TODOKECODE") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .SHUKABASHO = SHUKABASHO,
                                            .TODOKECODE = TODOKECODE,
                                            .KAISU = Group.Count()
                                        }
                    End If
                Case "03"
                    '車型単位
                    If String.IsNullOrEmpty(LNT0030CALCrow("AVOCADOTODOKECODE")) Then
                        '届先指定なし
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("SYAGATA") = LNT0030CALCrow("SHAGATA")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              SHAGATA = row.Field(Of String)("SYAGATA") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .SHAGATA = SHAGATA,
                                            .KAISU = Group.Count()
                                        }
                    Else
                        '届先指定あり
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("TODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE") AndAlso
                                       row.Field(Of String)("SYAGATA") = LNT0030CALCrow("SHAGATA")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              TODOKECODE = row.Field(Of String)("TODOKECODE"),
                                              SHAGATA = row.Field(Of String)("SYAGATA") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .TODOKECODE = TODOKECODE,
                                            .SHAGATA = SHAGATA,
                                            .KAISU = Group.Count()
                                        }
                    End If

                Case "04"
                    '車腹単位
                    If String.IsNullOrEmpty(LNT0030CALCrow("AVOCADOTODOKECODE")) Then
                        '届先指定なし
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("SYABARA") = LNT0030CALCrow("SHABARA")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              SHABARA = row.Field(Of String)("SYABARA") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .SHABARA = SHABARA,
                                            .KAISU = Group.Count()
                                        }
                    Else
                        '届先指定あり
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("TODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE") AndAlso
                                       row.Field(Of String)("SYABARA") = LNT0030CALCrow("SHABARA")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              TODOKECODE = row.Field(Of String)("TODOKECODE"),
                                              SHABARA = row.Field(Of String)("SYABARA") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .TODOKECODE = TODOKECODE,
                                            .SHABARA = SHABARA,
                                            .KAISU = Group.Count()
                                        }
                    End If
                Case "05"
                    '車番単位
                    If String.IsNullOrEmpty(LNT0030CALCrow("AVOCADOTODOKECODE")) Then
                        '届先指定なし
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("GYOMUTANKNUM") = LNT0030CALCrow("SHABAN")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              SHABAN = row.Field(Of String)("GYOMUTANKNUM") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .SHABAN = SHABAN,
                                            .KAISU = Group.Count()
                                        }
                    Else
                        '届先指定あり
                        result = From row In LNT0001SELtbl.AsEnumerable()
                                 Where row.Field(Of String)("TODOKEDATE") >= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                                       row.Field(Of String)("TODOKEDATE") <= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                                       row.Field(Of String)("ORDERORG") = LNT0030CALCrow("ORGCODE") AndAlso
                                       row.Field(Of String)("TODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE") AndAlso
                                       row.Field(Of String)("GYOMUTANKNUM") = LNT0030CALCrow("SHABAN")
                                 Group row By ORDERORG = row.Field(Of String)("ORDERORG"),
                                              TODOKECODE = row.Field(Of String)("TODOKECODE"),
                                              SHABAN = row.Field(Of String)("GYOMUTANKNUM") Into Group
                                 Select New With {
                                            .ORDERORG = ORDERORG,
                                            .TODOKECODE = TODOKECODE,
                                            .SHABAN = SHABAN,
                                            .KAISU = Group.Count()
                                        }
                    End If
            End Select

            '輸送回数設定
            For Each dtRow In result
                LNT0030CALCrow("SHIPPINGCOUNT") = dtRow.KAISU
            Next

        Next

        '元のレコードに輸送回数設定
        For Each LNT0030CALCrow In LNT0030CALCtbl.Rows
            For Each LNT0030row In LNT0030tbl.Rows
                If LNT0030row("SEIKYUDATEFROM") <= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                   LNT0030row("SEIKYUDATETO") >= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                   LNT0030row("AVOCADOSHUKABASHO") = LNT0030CALCrow("AVOCADOSHUKABASHO") AndAlso
                   LNT0030row("AVOCADOTODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE") AndAlso
                   LNT0030row("SHAGATA") = LNT0030CALCrow("SHAGATA") AndAlso
                   LNT0030row("SHABARA") = LNT0030CALCrow("SHABARA") AndAlso
                   LNT0030row("SHABAN") = LNT0030CALCrow("SHABAN") Then
                    If LNT0030row("SHIPPINGCOUNTUPDFLG") = "0" Then
                        LNT0030row("OPERATION") = "1"
                        LNT0030row("SHIPPINGCOUNT") += LNT0030CALCrow("SHIPPINGCOUNT")
                    End If
                    Exit For
                End If
            Next
        Next

        '-------------------------------------------------
        'サーチャージ計算
        '-------------------------------------------------
        Select Case work.WF_SEL_TORICODE.Text
            Case CONST_TORICODE_0005700000
                ' ＥＮＥＯＳ
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    '実勢単価は同じ月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(0).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice

                    '輸送回数×標準走行距離/燃費×(実際単価-基準単価)
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("SHIPPINGCOUNT") * LNT0030row("DISTANCE") / wNenpi,
                                                                2,
                                                                CONST_ROUND)
                    LNT0030row("SURCHARGE") = Rounding(LNT0030row("SHIPPINGCOUNT") * LNT0030row("DISTANCE") / wNenpi * (LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")),
                                                               0,
                                                               LNT0030row("SURCHARGEROUNDMETHOD"))
                Next
            Case CONST_TORICODE_0045300000
                ' エスジーリキッドサービス
            Case CONST_TORICODE_0051200000
                ' ＤＧＥ
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    '実勢単価は同じ月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(0).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice

                    '(a)燃料使用量（軽油使用量）＝(届先毎の配送回数)×走行距離÷燃費
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("SHIPPINGCOUNT") * LNT0030row("DISTANCE") / wNenpi,
                                                                2,
                                                                CONST_ROUND)
                    '(b)軽油代の運賃収入＝(a)燃料使用量×基準軽油単価
                    Dim Amt As Decimal = LNT0030row("FUELRESULT") * LNT0030row("DIESELPRICESTANDARD")
                    '(c)加重平均軽油単価＝(b)軽油代の運賃収入÷(a)燃料使用量
                    Dim Tanka As Decimal = 0
                    If LNT0030row("FUELRESULT") = 0 Then
                        Tanka = 0
                    Else
                        Tanka = Rounding(Amt / LNT0030row("FUELRESULT"), 2, CONST_ROUND)
                    End If

                    LNT0030row("SURCHARGE") = Rounding((LNT0030row("DIESELPRICECURRENT") - Tanka) * LNT0030row("FUELRESULT"),
                                                       0,
                                                       LNT0030row("SURCHARGEROUNDMETHOD"))
                Next
            Case CONST_TORICODE_0045200000
                ' エスケイ産業
            Case CONST_TORICODE_0132800000
                ' 石油資源開発
                '--------------------------------------------------------
                '本州（茨城営業所）の場合（とりあえず部署で判定）
                '--------------------------------------------------------
                For Each LNT0030row In LNT0030tbl.Select("ORGCODE = '020804'")
                    '実勢単価は同じ月のものを取得
                    For Each LNT0031row In LNT0031tbl.Rows
                        If Left(LNT0030row("SEIKYUDATEFROM"), 4) = LNT0031row("TARGETYEAR") Then
                            Dim wMonth As Integer = CDate(LNT0030row("SEIKYUDATEFROM")).ToString("MM")
                            '実勢単価
                            LNT0030row("DIESELPRICECURRENT") = LNT0031row("DIESELPRICE" & wMonth)
                            Exit For
                        End If
                    Next
                    '輸送回数×標準走行距離(411.2km)/燃費(2.3km)×(実際単価-基準単価)
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("SHIPPINGCOUNT") * LNT0030row("DISTANCE") / wNenpi,
                                                                2,
                                                                CONST_ROUND)
                    LNT0030row("SURCHARGE") = Rounding(LNT0030row("SHIPPINGCOUNT") * LNT0030row("DISTANCE") / wNenpi * (LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")),
                                                               0,
                                                               LNT0030row("SURCHARGEROUNDMETHOD"))
                Next

                '--------------------------------------------------------
                '釧路ガスの判定をサーチャージパターン（車番単位）で判定
                '--------------------------------------------------------
                For Each LNT0030row In LNT0030tbl.Select("ORGCODE = '020104' and PATTERNCODE = '05'")
                    '実勢単価は前月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(-1).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice
                    '(基準燃料使用量(車両毎に異なる[協定書で定義されている])×輸送回数×(実際単価-基準単価))÷2
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("FUELBASE") * LNT0030row("SHIPPINGCOUNT") / wNenpi,
                                                        2,
                                                        CONST_ROUND)
                    LNT0030row("SURCHARGE") = Rounding(LNT0030row("FUELBASE") * LNT0030row("SHIPPINGCOUNT") * (LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")) / 2,
                                                       0,
                                                       LNT0030row("SURCHARGEROUNDMETHOD"))
                Next

                '--------------------------------------------------------
                '室蘭の判定をサーチャージパターン（届先単位）で判定
                '※月別に計算し、積み上げる
                '--------------------------------------------------------
                Dim WW_FIND As Boolean = False
                For Each LNT0030CALCrow In LNT0030CALCtbl.Select("ORGCODE = '020104' and PATTERNCODE = '02'")
                    WW_FIND = True
                    '実勢単価は前月のものを取得
                    For Each LNT0031row In LNT0031tbl.Rows
                        Dim previousMonth As String = CDate(LNT0030CALCrow("SEIKYUDATEFROM")).AddMonths(-1).ToString
                        If Left(previousMonth, 4) = LNT0031row("TARGETYEAR") Then
                            Dim wMonth As Integer = CDate(previousMonth).ToString("MM")
                            '実勢単価
                            LNT0030CALCrow("DIESELPRICECURRENT") = LNT0031row("DIESELPRICE" & wMonth)
                            Exit For
                        End If
                    Next
                    '燃料使用量(130ℓ)×輸送回数×(実際単価-基準単価)
                    LNT0030CALCrow("FUELRESULT") = LNT0030CALCrow("FUELBASE")
                    LNT0030CALCrow("SURCHARGE") = Rounding(LNT0030CALCrow("FUELBASE") * LNT0030CALCrow("SHIPPINGCOUNT") * (LNT0030CALCrow("DIESELPRICECURRENT") - LNT0030CALCrow("DIESELPRICESTANDARD")),
                                                           0,
                                                           LNT0030CALCrow("SURCHARGEROUNDMETHOD"))
                Next
                '元のレコードに戻す
                If WW_FIND = True Then
                    For Each LNT0030CALCrow In LNT0030CALCtbl.Rows
                        For Each LNT0030row In LNT0030tbl.Rows
                            If LNT0030row("SEIKYUDATEFROM") <= LNT0030CALCrow("SEIKYUDATEFROM") AndAlso
                               LNT0030row("SEIKYUDATETO") >= LNT0030CALCrow("SEIKYUDATETO") AndAlso
                               LNT0030row("AVOCADOSHUKABASHO") = LNT0030CALCrow("AVOCADOSHUKABASHO") AndAlso
                               LNT0030row("AVOCADOTODOKECODE") = LNT0030CALCrow("AVOCADOTODOKECODE") AndAlso
                               LNT0030row("SHAGATA") = LNT0030CALCrow("SHAGATA") AndAlso
                               LNT0030row("SHABARA") = LNT0030CALCrow("SHABARA") AndAlso
                               LNT0030row("SHABAN") = LNT0030CALCrow("SHABAN") Then
                                LNT0030row("DIESELPRICECURRENT") += LNT0030CALCrow("DIESELPRICECURRENT")
                                If LNT0030row("DISTANCEUPDFLG") = "0" Then
                                    LNT0030row("OPERATION") = "1"
                                End If
                                If LNT0030row("FUELRESULTUPDFLG") = "0" Then
                                    LNT0030row("OPERATION") = "1"
                                    LNT0030row("FUELRESULT") += LNT0030CALCrow("FUELRESULT")
                                End If
                                LNT0030row("SURCHARGE") += LNT0030CALCrow("SURCHARGE")
                                Exit For
                            End If
                        Next
                    Next
                    For Each LNT0030row In LNT0030tbl.Rows
                        LNT0030row("DIESELPRICECURRENT") = Rounding(LNT0030row("DIESELPRICECURRENT") / (DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1),
                                                                    LNT0030row("DIESELPRICEROUNDLEN"),
                                                                    LNT0030row("DIESELPRICEROUNDMETHOD"))

                    Next
                End If

            Case CONST_TORICODE_0110600000, CONST_TORICODE_0238900000
                ' シーエナジー、北陸エルネス
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(-1).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（当年３月から翌年２月の平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice

                    '納入先の往復距離(情報無し)×輸送回数総計(当年４月から翌３月)÷燃費(2.5km)
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("DISTANCE") = LNT0030row("ROUNDTRIP")
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("ROUNDTRIP") * LNT0030row("SHIPPINGCOUNT") / wNenpi,
                                                        2,
                                                        CONST_ROUND)
                    If LNT0030row("DIESELPRICESTANDARD") + LNT0030row("ADJUSTMENT") < DieselPrice Then
                        LNT0030row("SURCHARGE") = LNT0030row("FUELBASE") * (DieselPrice - (LNT0030row("DIESELPRICESTANDARD") + LNT0030row("ADJUSTMENT")))
                    ElseIf LNT0030row("DIESELPRICESTANDARD") - LNT0030row("ADJUSTMENT") > DieselPrice Then
                        LNT0030row("SURCHARGE") = LNT0030row("FUELBASE") * (DieselPrice - (LNT0030row("DIESELPRICESTANDARD") + LNT0030row("ADJUSTMENT")))
                    Else
                        LNT0030row("SURCHARGE") = 0
                    End If
                Next

            Case CONST_TORICODE_0239900000
                ' 北海道ＬＮＧ
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    '実勢単価は同じ月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(0).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice

                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("DISTANCE") * LNT0030row("SHIPPINGCOUNT") / wNenpi,
                                                        2,
                                                        CONST_ROUND)
                    '実際単価－基準単価が10円以内の場合は精算対象外
                    If LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD") <= 10 Then
                        LNT0030row("SURCHARGE") = 0
                    Else
                        '((実際単価-基準単価)÷1.1×総走行距離(特殊車両通行許可申請書記載の往復距離×輸送回数)÷平均燃費(2.5km))÷2
                        LNT0030row("SURCHARGE") = Rounding(((LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")) / 1.1 * (LNT0030row("DISTANCE") * LNT0030row("SHIPPINGCOUNT")) / wNenpi) / 2, 1, LNT0030row("SURCHARGEROUNDMETHOD"))
                    End If
                Next
            Case CONST_TORICODE_0175300000
                ' 東北天然ガス
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    '実勢単価は前月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(-1).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice
                    '(基準燃料使用量(車両毎に異なる[協定書で定義されている])×輸送回数×(実際単価-基準単価))÷2
                    LNT0030row("FUELRESULT") = LNT0030row("FUELBASE")
                    LNT0030row("SURCHARGE") = Rounding(LNT0030row("FUELBASE") * LNT0030row("SHIPPINGCOUNT") * (LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")) / 2,
                                                       0,
                                                       LNT0030row("SURCHARGEROUNDMETHOD"))
                Next
            Case CONST_TORICODE_0175400000
                ' 東北電力
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    '実勢単価は同じ月のものを取得（平均単価）
                    Dim stMonth As String = CDate(LNT0030row("SEIKYUDATEFROM")).AddMonths(0).ToString
                    Dim sumPrice As Decimal = 0
                    Dim DieselPrice As Decimal = 0
                    Dim MonthCnt As Integer = DateDiff(DateInterval.Month, CDate(LNT0030row("SEIKYUDATEFROM")), CDate(LNT0030row("SEIKYUDATETO"))) + 1
                    For i As Integer = 0 To MonthCnt - 1
                        Dim nextMonth As String = CDate(stMonth).AddMonths(i).ToString
                        Dim wMonth As Integer = CDate(nextMonth).ToString("MM")
                        For Each LNT0031row In LNT0031tbl.Rows
                            If LNT0031row("TARGETYEAR") = Left(nextMonth, 4) Then
                                sumPrice += LNT0031row("DIESELPRICE" & wMonth)
                            End If
                        Next
                    Next
                    DieselPrice = Rounding(sumPrice / MonthCnt,
                                           LNT0030row("DIESELPRICEROUNDLEN"),
                                           LNT0030row("DIESELPRICEROUNDMETHOD"))

                    '実勢単価（平均単価）
                    LNT0030row("DIESELPRICECURRENT") = DieselPrice

                    '軽油使用量(走行距離÷平均燃費)×輸送回数×（差額）
                    Dim wNenpi As Decimal = 1
                    If LNT0030row("NENPI") > 0 Then
                        wNenpi = LNT0030row("NENPI")
                    End If
                    LNT0030row("FUELRESULT") = Rounding(LNT0030row("DISTANCE") / wNenpi * LNT0030row("SHIPPINGCOUNT"),
                                                        2,
                                                        CONST_ROUND)
                    LNT0030row("SURCHARGE") = Rounding(LNT0030row("DISTANCE") / wNenpi * LNT0030row("SHIPPINGCOUNT") * (LNT0030row("DIESELPRICECURRENT") - LNT0030row("DIESELPRICESTANDARD")),
                                                       0,
                                                       LNT0030row("SURCHARGEROUNDMETHOD"))
                Next
        End Select

    End Sub
    Public Function Rounding(ByVal iNum As Double, ByVal iLength As Integer, ByVal iRoundMethod As String) As Decimal
        Dim rtnNum As Decimal = 0
        Select Case iRoundMethod
            Case CONST_ROUND
                '四捨五入
                rtnNum = Math.Round(iNum, iLength, MidpointRounding.AwayFromZero)
            Case CONST_FLOOR
                '切り捨て
                rtnNum = Math.Floor(iNum * 10 ^ iLength) / 10 ^ iLength
            Case CONST_CEILING
                '切り上げ
                rtnNum = Math.Ceiling(iNum * 10 ^ iLength) / 10 ^ iLength
        End Select

        Return rtnNum
    End Function

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile(ByVal iKeyWord As String)
        WF_XMLsaveF30.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & iKeyWord & "TBL.txt"
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNT0001WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

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
        'wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        'wb.ActiveSheet.Range("B2").Value = "は入力必須"
        wb.ActiveSheet.Range("C1").Value = "サーチャージ料金一覧"
        'wb.ActiveSheet.Range("C2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY)
        'wb.ActiveSheet.Range("D2").Value = "は入力不要"

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
            Case LNT0001WRKINC.FILETYPE.EXCEL
                FileName = "サーチャージ料金.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNT0001WRKINC.FILETYPE.PDF
                FileName = "サーチャージ料金.pdf"
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
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.SEIKYUYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求年月
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求対象期間From
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求対象期間To
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'パターンコード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '出荷場所コード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '届先コード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '距離計算方式

        '入力不要列網掛け
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '請求年月枝番
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.TORINAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '取引先名
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.ORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '部門名
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.KASANORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '加算先部門コード
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.KASANORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '加算先部門名
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '出荷場所名
        'sheet.Columns(LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '届先名

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
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUYM).Value = "請求年月"
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Value = "請求年月枝番"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Value = "請求対象期間From"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Value = "請求対象期間To"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.TORICODE).Value = "取引先コード"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.ORGCODE).Value = "部門コード"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名"
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名"
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE).Value = "（必須）パターンコード"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = "出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = "出荷場所名"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = "届先コード"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = "届先名"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA).Value = "車型"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHABARA).Value = "車腹"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHABAN).Value = "車番"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = "基準単価"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = "実勢単価"
        'sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = "（必須）距離計算方式"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DISTANCE).Value = "距離"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = "輸送回数"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.NENPI).Value = "燃費"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.FUELBASE).Value = "基準燃料使用量"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.FUELRESULT).Value = "燃料使用量"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.ADJUSTMENT).Value = "精算調整幅"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SURCHARGE).Value = "サーチャージ"
        sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.MEMO).Value = "計算式メモ"

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
            '    sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
            '        .Width = 50
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            'パターンコード
            'COMMENT_get(SQLcon, "SURCHARGEPATTERN", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE).Comment.Shape
            '        .Width = 100
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If

            '車型
            COMMENT_get(SQLcon, "SHAGATA", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA).Comment.Shape
                    .Width = 70
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '距離計算方式
            'COMMENT_get(SQLcon, "CALCMETHOD", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD).Comment.Shape
            '        .Width = 200
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
        'SETFIXVALUELIST(subsheet, "DELFLG", LNT0001WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNT0001WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If
        'サーチャージパターンコード
        'SETFIXVALUELIST(subsheet, "SURCHARGEPATTERN", LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE)
        '    WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
        '    With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
        '        .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
        '    End With
        'End If
        '車型
        SETFIXVALUELIST(subsheet, "SHAGATA", LNT0001WRKINC.INOUTEXCELCOL.SHAGATA, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If
        '距離計算方式
        'SETFIXVALUELIST(subsheet, "CALCMETHOD", LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD, WW_FIXENDROW)
        'If Not WW_FIXENDROW = -1 Then
        '    WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD)
        '    WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD)
        '    WW_SUB_STRANGE = subsheet.Cells(0, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD)
        '    WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD)
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
    ''' 入力不可列設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetCOLLOCKED(ByVal sheet As IWorksheet, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'Dim WW_STRANGE As IRange
        'Dim WW_ENDRANGE As IRange

        ''シートの保護をかけるとリボンも操作できなくなるため
        ''データの入力規則で対応(該当セルの入力可能文字数を0にする)

        ''枝番
        'WW_STRANGE = sheet.Cells(WW_STROW, LNT0001WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNT0001WRKINC.INOUTEXCELCOL.BRANCHCODE)
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

        For Each Row As DataRow In LNT0030tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUYM).Value = Row("SEIKYUYM") '請求年月
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Value = Row("SEIKYUBRANCH") '請求年月枝番
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Value = Row("SEIKYUDATEFROM") '請求対象期間From
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Value = Row("SEIKYUDATETO") '請求対象期間To
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.PATTERNCODE).Value = Row("PATTERNCODE") 'パターンコード
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = Row("AVOCADOSHUKABASHO") '出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = Row("AVOCADOSHUKANAME") '出荷場所名
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = Row("AVOCADOTODOKECODE") '届先コード
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = Row("AVOCADOTODOKENAME") '届先名
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHAGATA).Value = Row("SHAGATA") '車型
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHABARA).Value = Row("SHABARA") '車腹
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHABAN).Value = Row("SHABAN") '車番
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = Row("DIESELPRICESTANDARD") '基準単価
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = Row("DIESELPRICECURRENT") '実勢単価
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = Row("CALCMETHOD") '距離計算方式
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DISTANCE).Value = Row("DISTANCE") '距離
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = Row("SHIPPINGCOUNT") '輸送回数
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.NENPI).Value = Row("NENPI") '燃費
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.FUELBASE).Value = Row("FUELBASE") '基準燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.FUELRESULT).Value = Row("FUELRESULT") '燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.ADJUSTMENT).Value = Row("ADJUSTMENT") '精算調整幅
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SURCHARGE).Value = Row("SURCHARGE") 'サーチャージ
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.MEMO).Value = Row("MEMO") '計算式メモ
            'sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ

            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHABARA).Style = DecStyle '車腹
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Style = DecStyle2 '基準単価
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Style = DecStyle2 '実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.DISTANCE).Style = DecStyle2 '距離
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Style = IntStyle '輸送回数
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.NENPI).Style = DecStyle2 '燃費
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.FUELBASE).Style = DecStyle2 '基準燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.FUELRESULT).Style = DecStyle2 '燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.ADJUSTMENT).Style = DecStyle2 '精算調整幅
            sheet.Cells(WW_ACTIVEROW, LNT0001WRKINC.INOUTEXCELCOL.SURCHARGE).Style = IntStyle 'サーチャージ

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
                Case "DELFLG", "SURCHARGEPATTERN", "CALCMETHOD", "SHAGATA"   '削除フラグ、サージャージパターンコード、距離計算方式、車型
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
                Case "DELFLG", "SURCHARGEPATTERN", "CALCMETHOD", "SHAGATA"   '削除フラグ、サーチャージパターン、距離計算方式、車型
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
End Class