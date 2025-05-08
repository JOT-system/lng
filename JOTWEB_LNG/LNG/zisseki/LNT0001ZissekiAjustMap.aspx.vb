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
Public Class LNT0001ZissekiAjustMap_aspx
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                 '実績（アボカド）データ格納用テーブル
    Private WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 60                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

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
                    Master.RecoverTable(LNT0001tbl)
                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0001tbl, pnlListArea) Then
                        Master.SaveTable(LNT0001tbl)
                    End If
                    '★戻り値(初期化)
                    WW_ErrSW = ""
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '保存ボタンクリック
                            WF_ButtonUPDATE()
                        Case "WF_ButtonCLEAR"           '戻るボタンクリック
                            WF_ButtonEND_Click()
                        Case "WF_TARGETTABLEChange"     '対象選択クリック
                            WF_TARGETTABLEInitialize()
                        Case "WF_ButtonSearch"          '検索ボタンクリック
                            WF_ButtonSearch_Click()

                        Case "WF_MouseWheelUp"

                        Case "WF_ButtonRelease"         '解除ボタンクリック
                            WF_ButtonRelease_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                    End Select
                    If WW_ErrSW <> "ERR" _
                        AndAlso WF_ButtonClick.Value <> "WF_ButtonSearch" _
                        AndAlso WF_ButtonClick.Value <> "WF_ButtonRelease" _
                        AndAlso WF_ButtonClick.Value <> "WF_TARGETTABLEChange" Then
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
            If Not IsNothing(LNT0001tbl) Then
                LNT0001tbl.Clear()
                LNT0001tbl.Dispose()
                LNT0001tbl = Nothing
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

        '〇実績データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            setZisseki(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

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
        'Me.pnlSurchargeArea.Visible = False

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

    Private Sub setDDLItem(ByVal dt As DataTable, ByVal ItemCode As String, ByVal ItemaName As String, ByRef ddlList As DropDownList)

        Dim resTrainFlagList As New List(Of ListItem)
        Dim itemList = From wrkitm In dt Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By g Select g, h
        'Dim itemList = From wrkitm In LNT0001tbl Group By g = Convert.ToString(wrkitm(ItemCode)), h = Convert.ToString(wrkitm(ItemaName)) Into Group Order By CDec(g) Select g, h
        ddlList.Items.Clear()
        resTrainFlagList = New List(Of ListItem)
        resTrainFlagList.Add(New ListItem("", ""))
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
                                                       WF_TODOKE:=WF_TODOKE, WF_TANKNUMBER:=WF_TANKNUMBER, WF_GYOMUTANKNO:=WF_GYOMUTANKNO)

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
        GS0007FIXVALUElst.ADDITIONAL_CONDITION = " AND VALUE11 = 'TYOSEI' "
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

            '枝番
            condition &= String.Format(" AND BRANCHCODE='{0}' ", dtTankaAjustrow("KEYCODE").ToString())

            For Each LNT0001row As DataRow In LNT0001tbl.Select(condition)
                LNT0001row("BRANCHNAME") = dtTankaAjustrow("VALUE16").ToString()
            Next

        Next

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            'setZisseki(SQLcon)
            setZisseki(SQLcon,
                       WF_TODOKE:=WF_TODOKECODEhdn.Value,
                       WF_TANKNUMBER:=WF_TANKNUMBERhdn.Value,
                       WF_GYOMUTANKNO:=WF_GYOMUTANKNOhdn.Value)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        '〇 一覧の件数を取得
        'Me.ListCount.Text = "件数：" + LNT0002tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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
    Protected Sub DisplayGrid()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0001row As DataRow In LNT0001tbl.Rows
            If LNT0001row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0001row("SELECT") = WW_DataCNT
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

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

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
    ''' 保存ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE()

        Dim Msg = ""
        If WF_TARGETTABLE.SelectedValue = "" Then
            Msg = "対象から調整する内容を選択してください。"
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
            WW_ErrSW = "ERR"
            Exit Sub
        End If

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case "特別料金"
            Case "単価調整"
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

            Case "固定費調整"
            Case "サーチャージ"
            Case Else
                Exit Sub
        End Select

        Master.SaveTable(LNT0001tbl)

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
        'Me.pnlSpecialFEEArea.Visible = False
        Me.pnlPriceArea.Visible = False
        'Me.pnlFixedCostsArea.Visible = False
        'Me.pnlSurchargeArea.Visible = False

        Select Case Me.WF_TARGETTABLE.SelectedItem.Text
            Case "特別料金"
                'Me.pnlSpecialFEEArea.Visible = True
            Case "単価調整"
                work.WF_SEL_CONTROLTYPE.Text = LNT0001WRKINC.MAPIDAJ
                Me.pnlPriceArea.Visible = True
                '〇対象年月(変更)
                If WF_TaishoYm.Value <> WF_TaishoYmhdn.Value Then
                    '★フィルタ設定(日)
                    setDDLDay(yyyyMM:=WF_TaishoYm.Value)
                    WF_TaishoYmhdn.Value = WF_TaishoYm.Value
                End If
                '○ GridView初期設定
                GridViewInitialize()
            Case "固定費調整"
                'Me.pnlFixedCostsArea.Visible = True
            Case "サーチャージ"
                'Me.pnlSurchargeArea.Visible = True
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
            Case "特別料金"
                'Me.pnlSpecialFEEArea.Visible = True
            Case "単価調整"
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

            Case "固定費調整"
                'Me.pnlFixedCostsArea.Visible = True
            Case "サーチャージ"
                'Me.pnlSurchargeArea.Visible = True
            Case Else
                Exit Sub
        End Select

        '○ GridView初期設定
        GridViewInitialize()

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
                                    " AND VALUE11 = 'TYOSEI'" &                                       '単価用途(単価調整)
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
                GS0007FIXVALUElst.ADDITIONAL_CONDITION &= " AND VALUE11 = 'TYOSEI' "

                dtTankaInfo = GS0007FIXVALUElst.GS0007FIXVALUETbl()
                If Not isNormal(GS0007FIXVALUElst.ERR) Then
                    Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "単価情報取得エラー")
                    Exit Sub
                End If

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

        Master.SaveTable(LNT0001tbl)

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

                updHeader.Item("OPERATION") = "1"
                updHeader.Item("BRANCHCODE") = WW_SETVALUE
                updHeader.Item("BRANCHNAME") = WW_SETTEXT

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
        Me.WF_TODOKECODEhdn.Value = ""
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
        Me.WF_TANKNUMBERhdn.Value = ""
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
        Me.WF_GYOMUTANKNOhdn.Value = ""
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

End Class