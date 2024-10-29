''************************************************************
' 帳票出力画面
' 作成日 2022/10/28
' 更新日 2024/09/20
' 作成者 牧野
' 更新者 星
'
' 修正履歴:2022/10/28    新規作成
'         :2023/06/01    新規帳票追加対応 (コンテナ回送費明細(発駅・受託人別))
'         :2023/09/05    新規帳票追加対応 (科目別集計表)
'         :2024/09/20 星 メニュー帳票出力日付シート分け対応による引数追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 帳票出力画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0012ReportOutput
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIT0012Reporttbl As DataTable                         '帳票用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    ''' <summary>
    ''' 共通処理結果
    ''' </summary>
    Private WW_ErrSW As String
    Private WW_RtnSW As String
    Private WW_Dummy As String

    ''' <summary>
    ''' ストアド名称取得変数
    ''' </summary>
    Private SqlName As String = ""

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonOUTPUT"              '出力ボタン押下
                        WF_ButtonOUTPUT_Click()
                    Case "WF_ButtonEND"                 '戻るボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FiledDBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        WF_FiledChange()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case WF_ORGCODE_ALL.ID,          '部店変更時処理
                            WF_ORGCODE.ID,
                            WF_ORGCODE_MULTIPLE.ID
                        WF_ORGCODE_Change()
                    Case WF_PERIODTYPE_DDL.ID           '期間種別リスト変更
                        WF_FISCALYEAR_TextChange()

                    Case WF_FISCALYEAR.ID               '年度変更
                        WF_FISCALYEAR_TextChange()
                    Case "mspToriSingleRowSelected"         '[共通]取引先選択ポップアップで行選択
                        RowSelected_mspToriSingle()
                    Case "mspStationSingleRowSelected"  '[共通]駅選択ポップアップで行選択
                        RowSelected_mspStationSingle()
                End Select
            End If
        Else
            '○ 初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0012WRKINC.MAPID

        'TxtReportId.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '請求書種類の初期化
        Me.InitInvtype()

        '○ 画面の値設定
        WW_MAPValueSet()

        ' ドロップダウンリスト初期設定
        dropDownInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ' メニューからの画面遷移
        ' 画面間の情報クリア
        work.Initialize()

        ' 初期変数設定処理
        TxtReportId.Text = "PRT0001A"                                               '帳票ID
        TxtReportName.Text = "発送日報（A）"                                        '帳票名
        TxtStYMDCode.Text = Date.Now.AddDays(-1).ToString("yyyy/MM/dd")             '年月日(開始)
        TxtEndYMDCode.Text = ""                                                     '年月日(終了)
        TxtShipYMDFrom.Text = New DateTime(Date.Now.Year, Date.Now.Month, 1).ToString("yyyy/MM/dd")  '発送年月日(FROM)
        TxtShipYMDTo.Text = Date.Now.ToString("yyyy/MM/dd")  '発送年月日(TO)

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtPayeeCode.Attributes("onkeyPress") = "CheckNum()"

        'コンテナ動静表
        '経理資産区分リスト
        WF_ACCOUNTINGASSETSKBN_DDL.Items.Clear()
        WF_ACCOUNTINGASSETSKBN_DDL.Items.Add(New ListItem("レンタル", "01"))
        WF_ACCOUNTINGASSETSKBN_DDL.Items.Add(New ListItem("リース", "02"))

        '発駅・通運別合計表の初期値設定
        TxtSort.Text = "1"
        CODENAME_get("SORT", TxtSort.Text, LblSort.Text, WW_Dummy)

        '○ 請求先・勘定科目別・計上店別営業収入計上一覧(全勘定科目)
        '期間種別リスト
        WF_PERIODTYPE_DDL.Items.Clear()
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("任意期間", "0001"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("１Ｑ", "0002"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("２Ｑ", "0003"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("３Ｑ", "0004"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("４Ｑ", "0005"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("上半期", "0006"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("下半期", "0007"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("年間", "0008"))
        '年度
        If Date.Now.Month >= 4 Then
            WF_FISCALYEAR.Text = Date.Now.Year
        Else
            WF_FISCALYEAR.Text = Date.Now.AddYears(-1).Year
        End If
        '期間
        If Date.Now.Month >= 4 Then
            WF_PERIOD_FROM.Text = CDate(Date.Now.Year & "/04/01")
        Else
            WF_PERIOD_FROM.Text = CDate(Date.Now.AddYears(-1).Year & "/04/01")
        End If
        WF_PERIOD_TO.Text = Date.Now.ToString("yyyy/MM/dd")
        '期間種別出力条件初期化
        WF_PERIODTYPE_DDL.SelectedIndex = 0
        WF_PERIODTYPE_Change()

        '○ RightBox情報設定
        rightview.MAPID = LNT0012WRKINC.MAPID
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)

        '部店選択表示初期設定
        WF_ORGCODE_Init()

        '取引先
        Dim retToriList As DropDownList = CmnSearchSQL.getDdlTori()
        If retToriList.Items.Count > 0 Then
            Me.hdnSelectTori.Items.AddRange(retToriList.Items.Cast(Of ListItem).ToArray)
        End If

        '明細種類初期値
        Me.WF_INVTYPE.SelectSingleItem("1")

    End Sub

    ''' <summary>
    ''' 請求書種類初期化
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitInvtype()

        WF_INVTYPE.SelectionMode = ListSelectionMode.Multiple
        WF_INVTYPE.NeedsPostbackAfterSelect = False

        Dim dt As DataTable = CmnLNG.GetFixValueTbl(Master.USERCAMP, "DETAILTYPEINV")
        WF_INVTYPE.SetTileValues(dt)

    End Sub

    ''' <summary>
    ''' 部店選択表示初期設定
    ''' </summary>
    Protected Sub WF_ORGCODE_Init()

        '支店ドロップダウンの生成
        '支店ドロップダウンのクリア
        Me.ddlSelectOffice.Items.Clear()
        '支店ドロップダウンの生成
        Dim retOfficeList As DropDownList = CmnLNG.getCmbOfficeList()
        If retOfficeList.Items.Count > 0 Then
            Me.ddlSelectOffice.Items.AddRange(retOfficeList.Items.Cast(Of ListItem).ToArray)
            For index As Integer = 0 To ddlSelectOffice.Items.Count - 1
                If ddlSelectOffice.Items(index).Value = Master.USER_AFFILIATION Then
                    ddlSelectOffice.Items(index).Selected = True
                End If
            Next
        End If

        '支店ドロップダウンの生成
        '支店ドロップダウンのクリア
        Me.ddlSelectLeaseOffice.Items.Clear()
        '支店ドロップダウンの生成
        Dim retLeaseOfficeList As DropDownList = CmnLNG.getCmbLeaseOfficeList()
        If retLeaseOfficeList.Items.Count > 0 Then
            Me.ddlSelectLeaseOffice.Items.AddRange(retLeaseOfficeList.Items.Cast(Of ListItem).ToArray)
            For index As Integer = 0 To ddlSelectLeaseOffice.Items.Count - 1
                If ddlSelectLeaseOffice.Items(index).Value = Master.USER_ORG Then
                    ddlSelectLeaseOffice.Items(index).Selected = True
                End If
            Next
        End If

        '選択値がない場合のみ、全支店選択を初期化
        If WF_ORGCODE_ALL.chklGrc0001SelectionBox.Items.Count = 0 Then
            WF_ORGCODE_ALL.SelectionMode = ListSelectionMode.Single
            WF_ORGCODE_ALL.NeedsPostbackAfterSelect = True
            Dim dt = New DataTable()
            dt.Columns.Add("key", Type.GetType("System.String"))
            dt.Columns.Add("value", Type.GetType("System.String"))
            Dim dr = dt.NewRow
            dr("key") = "000000"
            dr("value") = "全支店"
            dt.Rows.Add(dr)
            WF_ORGCODE_ALL.SetTileValues(dt)
        End If

        '部店別選択の初期化
        WF_ORGCODE.SelectionMode = ListSelectionMode.Single
        'WF_ORGCODE.SelectionMode = ListSelectionMode.Multiple
        WF_ORGCODE.NeedsPostbackAfterSelect = True
        WF_ORGCODE.SetTileValues(
            GetOfficeData()
        )
        WF_ORGCODE_MULTIPLE.SelectionMode = ListSelectionMode.Multiple
        WF_ORGCODE_MULTIPLE.NeedsPostbackAfterSelect = False
        WF_ORGCODE_MULTIPLE.SetTileValues(
            GetOfficeData()
        )

        '全支店を初期選択
        If Master.USER_ORG <> "011312" AndAlso Master.USER_ORG <> "011308" Then
            WF_ORGCODE_ALL.UnSelectAll()
            WF_ORGCODE.SelectSingleItem(Master.USER_ORG)
            WF_ORGCODE_MULTIPLE.UnSelectAll()
            WF_ORGCODE_MULTIPLE.SelectSingleItem(Master.USER_ORG)
        Else
            WF_ORGCODE_ALL.SelectAll()
            WF_ORGCODE.UnSelectAll()
            WF_ORGCODE_MULTIPLE.SelectAll()
        End If

    End Sub

    ''' <summary>
    ''' 部店変更時処理
    ''' </summary>
    Private Sub WF_ORGCODE_Change()
        Select Case WF_ButtonClick.Value
            Case WF_ORGCODE_ALL.ID
                If WF_ORGCODE_ALL.HasSelectedValue() Then
                    WF_ORGCODE.UnSelectAll()
                End If
            Case WF_ORGCODE.ID
                If WF_ORGCODE.HasSelectedValue() Then
                    WF_ORGCODE_ALL.UnSelectAll()
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 年度テキスト変更時処理
    ''' </summary>
    Private Sub WF_FISCALYEAR_TextChange()

        '期間種別が任意期間以外の場合
        If WF_PERIODTYPE_DDL.SelectedValue <> "0001" Then
            '年度が正しくない場合
            If Not (Len(WF_FISCALYEAR.Text) = 4 AndAlso IsNumeric(WF_FISCALYEAR.Text)) Then
                Master.Output("10060", C_MESSAGE_TYPE.ERR, I_PARA01:="年度に日付", I_PARA02:="設定")
                Exit Sub
            End If
        End If
        '期間の年月日を反映
        WF_PERIODTYPE_Change()

    End Sub

    ''' <summary>
    ''' 期間種別名変更時処理
    ''' </summary>
    Private Sub WF_PERIODTYPE_Change()

        '表示の使用可否初期化
        WF_PERIOD_FROM.Enabled = False
        WF_PERIOD_TO.Enabled = False

        Select Case WF_PERIODTYPE_DDL.SelectedValue
            Case "0001"     '任意期間
                '期間開始
                WF_PERIOD_FROM.Enabled = True
                '期間終了
                WF_PERIOD_TO.Enabled = True
            Case "0002"     '１Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/06/30")
            Case "0003"     '２Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/07/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/09/30")
            Case "0004"     '３Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/10/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/12/31")
            Case "0005"     '４Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/01/01").AddYears(1)
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)
            Case "0006"     '上半期
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/09/30")
            Case "0007"     '下半期
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/10/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)
            Case "0008"     '年間
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)

        End Select
    End Sub

    ''' <summary>
    ''' ドロップダウンリスト初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub dropDownInitialize()

        ''初期表示時のみリスト作成
        'If Not IsPostBack Then
        '    '○帳票名ドロップダウンの生成
        '    '帳票名ドロップダウンのクリア
        '    Me.ddlReportId.Items.Clear()
        '    '帳票名ドロップダウンの生成
        '    Dim retReportList As DropDownList = Me.getCmbReportId()
        '    If retReportList.Items.Count > 0 Then
        '        Me.ddlReportId.Items.AddRange(retReportList.Items.Cast(Of ListItem).ToArray)
        '    End If

        'End If

        ''○ 選択した計上年月を非表示項目へセット
        'If Me.ddlReportId.Items.Count > 0 Then
        '    Me.hdnReport.Value = Me.ddlReportId.SelectedValue
        'End If

    End Sub

    ''' <summary>
    ''' 帳票名選択用コンボボックス作成
    ''' </summary>
    ''' <returns></returns>
    Public Function getCmbReportId() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT                                    ")
        sqlStat.AppendLine("     KEYCODE AS CODE                      ")
        sqlStat.AppendLine("    ,VALUE1 AS NAME                       ")
        sqlStat.AppendLine(" FROM COM.LNS0006_FIXVALUE with(nolock)   ")
        sqlStat.AppendLine(" WHERE CLASS = 'REPORTLIST'               ")
        sqlStat.AppendLine("   AND DELFLG = @DELFLG                   ")
        sqlStat.AppendLine("   AND CURDATE() BETWEEN STYMD AND ENDYMD ")
        sqlStat.AppendLine(" ORDER BY KEYCODE                         ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    While sqlDr.Read
                        Dim listItm As New ListItem(Convert.ToString(sqlDr("NAME")), Convert.ToString(sqlDr("CODE")))
                        retList.Items.Add(listItm)
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0012 getCmbReportId", needsPopUp:=True)
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' 出力ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOUTPUT_Click()

        Try

            Dim type As String = ""
            Dim PRTID As String = ""
            Dim OfficeCode As String = ""

            '帳票ID取得
            PRTID = Left(Me.TxtReportId.Text, 7)

            '必須入力チェック
            Dim err As String = ""
            WW_FieldCheck(PRTID, err)
            If err = "ERR" Then
                Exit Sub
            End If

            If WF_ORGCODE_ALL.HasSelectedValue() Then
                OfficeCode = ""
            ElseIf WF_ORGCODE.HasSelectedValue() Then
                OfficeCode = ""
                For Each item As ListItem In WF_ORGCODE.GetSelectedListData.Items
                    OfficeCode &= If(OfficeCode.Length > 0, "," & item.Value, item.Value)
                Next
            End If

            Select Case PRTID
                '○発送日報
                Case "PRT0001"
                    If Me.TxtReportId.Text = "PRT0001A" Then
                        type = "A"
                    ElseIf Me.TxtReportId.Text = "PRT0001B" Then
                        type = "B"
                    End If
                    '選択した支店によって変更する
                    If ddlSelectOffice.SelectedValue = "ALL" Then
                        OfficeCode = ""
                    Else
                        OfficeCode = ddlSelectOffice.SelectedValue
                    End If
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.DailyShipmentDataGet(PRTID, type)
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    Else
                        If dt.Rows.Count > 3000 AndAlso type = "B" Then
                            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "対象件数が3000件を超えています。", needsPopUp:=True)
                            Exit Sub
                        End If
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_DailyShipmentReport_DIODOC(Master.MAPID, "発送日報_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(OfficeCode, type, "0") ' 2024/09/20 星 CHG
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇他駅発送明細
                Case "PRT0002"
                    '選択した支店によって変更する
                    If ddlSelectOffice.SelectedValue = "ALL" Then
                        OfficeCode = ""
                    Else
                        OfficeCode = ddlSelectOffice.SelectedValue
                    End If
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.DailyShipmentDataGet(PRTID, type)
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_OtherStationReport_DIODOC(Master.MAPID, "他駅発送明細_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(OfficeCode, "0") ' 2024/09/20 星 CHG
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇コンテナ留置先一覧
                Case "PRT0003"
                    '選択した支店によって変更する
                    If ddlSelectOffice.SelectedValue = "ALL" Then
                        OfficeCode = ""
                    Else
                        OfficeCode = ddlSelectOffice.SelectedValue
                    End If
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.PutContainerDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_PutContainerReport_DIODOC(Master.MAPID, "コンテナ留置先一覧_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(OfficeCode, "0") ' 2024/09/20 星 CHG
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇品目別販売実績表 冷＆Ｓ
                Case "PRT0004"
                    '選択した支店によって変更する
                    If ddlSelectOffice.SelectedValue = "ALL" Then
                        OfficeCode = ""
                    Else
                        OfficeCode = ddlSelectOffice.SelectedValue
                    End If
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.SalesResultsDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_SalesResultsReport_DIODOC(Master.MAPID, "品目別販売実績表_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(OfficeCode, CDate(Me.TxtStYMDCode.Text), CDate(Me.TxtEndYMDCode.Text), "0") ' 2024/09/20 星 CHG
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

               '〇コンテナ動静表
                Case "PRT0005"
                    '選択した支店によって変更する
                    If ddlSelectOffice.SelectedValue = "ALL" Then
                        OfficeCode = ""
                    Else
                        OfficeCode = ddlSelectOffice.SelectedValue
                    End If
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.MovementContainerDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_MovementContainerReport_DIODOC(Master.MAPID, "コンテナ動静表_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(OfficeCode, CDate(Me.TxtStYMDCode.Text))
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇発駅・通運別合計表
                Case "PRT0006"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.TransportTotalDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim DepTrustee As Integer = 0
                    Dim Sort As String = TxtSort.Text
                    Dim AddSub As String = TxtAddSub.Text
                    If TxtDepTrustee.Text = "" Then
                        DepTrustee = 0
                    Else
                        DepTrustee = TxtDepTrustee.Text
                    End If
                    If TxtAddSub.Text = "" Then
                        AddSub = "1"
                    End If

                    Dim Report As New LNT0012_TransportTotalReport_DIODOC(Master.MAPID, "発駅・通運別合計表_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(DepTrustee, Sort, AddSub)
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇リース料明細チェックリスト
                Case "PRT0007"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim ReportType As String = TxtBranchBase.Text
                    If TxtBranchBase.Text = "" Then
                        ReportType = "1"
                    End If
                    Dim dt As DataTable = Me.LeaseFeeDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_LeaseFeeReport_DIODOC(Master.MAPID, "リース料明細チェックリスト_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(ReportType)
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇支店間流動表(金額)
                Case "PRT0008"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.InterBranchChartAmountDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_InterBranchChartAmountReport_DIODOC(Master.MAPID, "支店間流動表(金額)_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(TxtStackFree.Text)
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇支店間流動表(個数)・前年対比
                Case "PRT0009"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.InterBranchChartQuantityDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_InterBranchChartQuantityReport_DIODOC(Master.MAPID, "支店間流動表・前年対比_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                    '〇支店間流動表(個数)
                    'Case "PRT0009"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    '   Dim dt As DataTable = Me.InterBranchChartQuantityDataGet()
                    'データ0件時
                    '  If dt.Rows.Count = 0 Then
                    ' Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                    'Exit Sub
                    ' End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    'Dim Report As New LNT0012_InterBranchChartQuantityReport_DIODOC(Master.MAPID, "支店間流動表(個数)_TEMPLATE.xlsx", dt)
                    'Dim url As String
                    'Try
                    'url = Report.CreateExcelPrintData()
                    'Catch ex As Exception
                    'Throw
                    'End Try

                    '○ 別画面でExcelを表示
                    'WF_PrintURL.Value = url
                    'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '〇発駅・通運別合計表(期間)
                Case "PRT0010"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.TransportTotalPeriodDataGet()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Sort As String = TxtSort.Text
                    Dim AddSub As String = TxtAddSub.Text
                    If TxtAddSub.Text = "" Then
                        AddSub = "1"
                    End If

                    Dim Report As New LNT0012_TransportTotalPeriodReport_DIODOC(Master.MAPID, "発駅・通運別合計表(期間)_TEMPLATE.xlsx", dt)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData(Sort, AddSub)
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                ' 営業日報
                Case "LNT0010"
                    Dim url As String
                    If Me.TxtReportId.Text = "LNT0010_ALL" Then
                        '営業日報(全社)帳票出力処理
                        Dim Report As New LNT0010_SelesReport_ALL_DIODOC("LNT0010", "LNT0010_ALL" & ".xlsx")
                        Try
                            url = Report.CreateExcelPrintData(work.WF_SEL_CAMPCODE.Text, Me.TxtStYMDCode.Text)
                        Catch ex As Exception
                            Throw
                        End Try
                        WF_PrintURL.Value = url
                    ElseIf Me.TxtReportId.Text = "LNT0010_shiten" Then
                        '営業日報(支店別)帳票出力処理
                        Dim Report As New LNT0010_SelesReport_SHITEN_DIODOC("LNT0010", "LNT0010_shiten" & ".xlsx")
                        Try
                            url = Report.CreateExcelPrintData(work.WF_SEL_CAMPCODE.Text, Me.TxtStYMDCode.Text)
                        Catch ex As Exception
                            Throw
                        End Try
                        WF_PrintURL.Value = url
                    End If

                    If WF_PrintURL.Value = "" Then
                        Master.Output(C_MESSAGE_NO.CTN_SEL_NOTDATA, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Else
                        '○ 別画面でExcelを表示
                        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                    End If

                ' コンテナ回送費明細(発駅・受託人別)
                Case "PRT0011"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.GetRessnfList()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_RessnfListReport_DIODOC(Master.MAPID, "コンテナ回送費明細(発駅・受託人別)_TEMPLATE.xlsx", dt, CDate(TxtDownloadMonth.Text & "/01"))
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                ' レンタルコンテナ回送費明細(コンテナ別)
                Case "PRT0012"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.GetRentalCTNList()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_RentalCTNListReport_DIODOC(Master.MAPID, "レンタルコンテナ回送費明細(コンテナ別)_TEMPLATE.xlsx", dt, Me.TxtStYMDCode.Text, Me.TxtEndYMDCode.Text)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                'リース満了一覧表
                Case "PRT0013"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.GetLeaseExpirationList()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_LeaseExpirationList_DIODOC(Master.MAPID, "リース満了一覧表_TEMPLATE.xlsx", dt, CDate(TxtDownloadMonth.Text & "/01"), 1)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                '請求先・勘定科目別・計上店別営業収入計上一覧(全勘定科目)
                Case "PRT0014"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.GetByAccountPrintData()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim fiscalyear As String = WF_FISCALYEAR.Text
                    Dim reportType As Integer = 0
                    Dim yearMonthFrom As String = WF_PERIOD_FROM.Text
                    Dim yearMonthTo As String = WF_PERIOD_TO.Text
                    '期間種別の設定
                    If WF_PERIODTYPE_DDL.SelectedValue.Equals("0001") Then
                        '任意期間
                        reportType = 1
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0002") Then
                        '１Ｑ
                        reportType = 2
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0003") Then
                        '２Ｑ
                        reportType = 3
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0004") Then
                        '３Ｑ
                        reportType = 4
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0005") Then
                        '４Ｑ
                        reportType = 5
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0006") Then
                        '上半期
                        reportType = 6
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0007") Then
                        '下半期
                        reportType = 7
                    ElseIf WF_PERIODTYPE_DDL.SelectedValue.Equals("0008") Then
                        '年間
                        reportType = 8
                    End If


                    Dim Report As New LNT0012_BILLINGACCOUNTBYDEPARTMENTLIST_DIODOC(Master.MAPID, "請求先・勘定科目・計上部店別営業収入計上一覧表_TEMPLATE.xlsx", dt, fiscalyear, reportType, yearMonthFrom, yearMonthTo)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '科目別集計表
                Case "PRT0015"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    Dim dt As DataTable = Me.GetAccountSummaryList()
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="条件で", I_PARA02:="データ", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_AccountSummaryList_DIODOC(Master.MAPID, "科目別集計表_TEMPLATE.xlsx", dt, CDate(TxtDownloadMonth.Text & "/01"), 1)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                '使用料明細
                Case "PRT0016"

                    '請求書種類取得
                    Me.Save_SearchItem()

                    '選択していなかった場合両方取得
                    If work.WF_SRC_CONTRALNMODE.Text = "" Then
                        work.WF_SRC_CONTRALNMODE.Text = "1,2,4,5"
                    End If

                    If work.WF_SRC_CONTRALNMODE.Text.Contains("1") Then
                        '******************************
                        '帳票表示データ取得処理
                        '******************************
                        Dim dt As DataTable = Me.UsefeeDetailDataGet(PRTID, type, "1")
                        'データ0件時
                        If dt.Rows.Count = 0 Then
                            Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                            Exit Sub
                        End If

                        '******************************
                        '帳票作成処理の実行
                        '******************************
                        Dim Report As New LNT0012_UsefeeDetailReport_DIODOC(Master.MAPID, "使用料明細表_TEMPLATE.xlsx", dt)
                        Dim url As String
                        Try
                            url = Report.CreateExcelPrintData()
                        Catch ex As Exception
                            Throw
                        End Try

                        '○ 別画面でExcelを表示
                        WF_PrintURL.Value = url
                        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                    End If
               '〇回送運賃報告書
                Case "PRT0017"
                    '******************************
                    '帳票表示データ取得処理
                    '******************************
                    OfficeCode = ""
                    For Each item As ListItem In WF_ORGCODE_MULTIPLE.GetSelectedListData.Items
                        OfficeCode &= If(OfficeCode.Length > 0, "," & item.Value, item.Value)
                    Next

                    Dim dt As DataTable = Me.FreeSendFeeDataGet(OfficeCode)
                    'データ0件時
                    If dt.Rows.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="条件で", I_PARA02:="データ", needsPopUp:=True)
                        Exit Sub
                    End If

                    '******************************
                    '帳票作成処理の実行
                    '******************************
                    Dim Report As New LNT0012_FreeSendFeeReport_DIODOC(Master.MAPID, "回送運賃報告書_TEMPLATE.xlsx", dt, Me.TxtDownloadMonth.Text)
                    Dim url As String
                    Try
                        url = Report.CreateExcelPrintData()
                    Catch ex As Exception
                        Throw
                    End Try

                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            End Select

        Catch sqlex As MySqlException
            CS0011LOGWRITE.INFSUBCLASS = "LNT0012S " & SqlName          'SUBクラス名 + ストアド名称
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = sqlex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "LNT0012S"                     'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
        End Try

        ''○ チェック処理
        'WW_Check(WW_ErrSW)
        'If WW_ErrSW = "ERR" Then
        '    Exit Sub
        'End If

        ''○ 画面レイアウト設定
        'If String.IsNullOrEmpty(Master.VIEWID) Then
        '    Master.VIEWID = rightview.GetViewId(Master.USERCAMP)
        'End If

        'Master.CheckParmissionCode(Master.USERCAMP)
        'If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
        '    ' 画面遷移
        '    Master.TransitionPage()
        'End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        WW_Dummy = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""


        ' 年月日(From)
        Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
            TxtStYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 年月日(To)
        Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
            TxtEndYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 日付大小チェック
        If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
            Try
                If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                    Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                TxtStYMDCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 必須項目チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_FieldCheck(ByVal PRTID As String, ByRef O_RTN As String)

        O_RTN = ""
        WW_Dummy = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""

        ' 帳票ID
        Master.CheckField(Master.USERCAMP, "REPORTID", Me.TxtReportId.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "帳票ID", needsPopUp:=True)
            TxtReportId.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '組織コード
        If WF_ORGCODE_ALL.Visible AndAlso WF_ORGCODE.Visible AndAlso WF_ORGCODE_MULTIPLE.Visible Then
            '部店選択がない場合
            If Not WF_ORGCODE_ALL.HasSelectedValue() AndAlso
                Not WF_ORGCODE.HasSelectedValue() AndAlso
                 Not WF_ORGCODE_MULTIPLE.HasSelectedValue() Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象支店", needsPopUp:=True)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        Select Case PRTID
            '〇発送日報
            Case "PRT0001"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If

            '〇他駅発送明細
            Case "PRT0002"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If

            '〇コンテナ留置先一覧
            Case "PRT0003"
                ' 年月日(To)
                Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                        TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
                    Else
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                        TxtEndYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                    TxtEndYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If

                '処理
                If String.IsNullOrEmpty(TxtMode.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "処理", needsPopUp:=True)
                    TxtMode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtMode.Text <> "0" AndAlso TxtMode.Text <> "1" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="処理", I_PARA02:="０～１", needsPopUp:=True)
                        TxtMode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇品目別販売実績表
            Case "PRT0004"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 年月日(To)
                Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                        TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
                    Else
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                        TxtEndYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                    TxtEndYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If

            '〇コンテナ動静表
            Case "PRT0005"
                ' 対象日付
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象日付", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

            '〇発駅・通運別合計表
            Case "PRT0006"
                '請求年月もしくは発送年月日の入力
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) AndAlso String.IsNullOrEmpty(TxtShipYMDFrom.Text) AndAlso String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="請求年月もしくは発送年月日From,To", needsPopUp:=True)
                    TxtShipYMDFrom.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '発送年月日の未入力
                If String.IsNullOrEmpty(TxtShipYMDFrom.Text) OrElse String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    '請求年月の未入力
                    If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="請求年月もしくは発送年月日From,To", needsPopUp:=True)
                        TxtShipYMDFrom.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

                ' 発送年月日日付大小チェック
                If Not String.IsNullOrEmpty(TxtShipYMDFrom.Text) AndAlso Not String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    Try
                        If TxtShipYMDFrom.Text > TxtShipYMDTo.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtShipYMDFrom.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtShipYMDFrom.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If

                '受託人指定
                If Not String.IsNullOrEmpty(TxtTrustee.Text) Then
                    If TxtTrustee.Text <> "0" AndAlso TxtTrustee.Text <> "1" AndAlso TxtTrustee.Text <> "2" AndAlso TxtTrustee.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="受託人指定", I_PARA02:="０～３", needsPopUp:=True)
                        TxtTrustee.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '加減額表示指定
                If Not String.IsNullOrEmpty(TxtAddSub.Text) Then
                    If TxtAddSub.Text <> "1" AndAlso TxtAddSub.Text <> "2" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="加減額表示指定", I_PARA02:="１～２", needsPopUp:=True)
                        TxtAddSub.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇リース料明細チェックリスト
            Case "PRT0007"
                '対象年月
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象年月", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '処理
                If Not String.IsNullOrEmpty(TxtBranchBase.Text) Then
                    If TxtBranchBase.Text <> "1" AndAlso TxtBranchBase.Text <> "2" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="処理", I_PARA02:="１～２", needsPopUp:=True)
                        TxtBranchBase.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇支店間流動表(金額)
            Case "PRT0008"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 年月日(To)
                Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                        TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
                    Else
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                        TxtEndYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                    TxtEndYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If
                '発着ベース
                If String.IsNullOrEmpty(TxtDepArrBase.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "ベース", needsPopUp:=True)
                    TxtDepArrBase.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtDepArrBase.Text <> "1" AndAlso TxtDepArrBase.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="ベース", I_PARA02:="１、３", needsPopUp:=True)
                        TxtDepArrBase.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '積空区分
                If String.IsNullOrEmpty(TxtStackFree.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "積空区分", needsPopUp:=True)
                    TxtStackFree.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtStackFree.Text <> "1" AndAlso TxtStackFree.Text <> "3" AndAlso TxtStackFree.Text <> "5" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="積空区分", I_PARA02:="１、３、５", needsPopUp:=True)
                        TxtStackFree.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '出力設定
                If String.IsNullOrEmpty(TxtReportSetting.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "出力設定", needsPopUp:=True)
                    TxtReportSetting.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtReportSetting.Text <> "1" AndAlso TxtReportSetting.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="出力設定", I_PARA02:="１、３", needsPopUp:=True)
                        TxtReportSetting.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇支店間流動表(金額)
            Case "PRT0009"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 年月日(To)
                Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                        TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
                    Else
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                        TxtEndYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                    TxtEndYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If
                '発着ベース
                If String.IsNullOrEmpty(TxtDepArrBase.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "ベース", needsPopUp:=True)
                    TxtDepArrBase.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtDepArrBase.Text <> "1" AndAlso TxtDepArrBase.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="ベース", I_PARA02:="１、３", needsPopUp:=True)
                        TxtDepArrBase.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '入れ替え
                If String.IsNullOrEmpty(TxtReplace.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "入れ替え", needsPopUp:=True)
                    TxtReplace.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtReplace.Text <> "1" AndAlso TxtReplace.Text <> "5" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="入れ替え", I_PARA02:="１、５", needsPopUp:=True)
                        TxtReplace.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '出力設定
                If String.IsNullOrEmpty(TxtReportSetting.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "出力設定", needsPopUp:=True)
                    TxtReportSetting.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtReportSetting.Text <> "1" AndAlso TxtReportSetting.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="出力設定", I_PARA02:="１、３", needsPopUp:=True)
                        TxtReportSetting.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇発駅・通運別合計表(期間)
            Case "PRT0010"
                '請求年月もしくは発送年月日の未入力
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) AndAlso String.IsNullOrEmpty(TxtBllingToYM.Text) AndAlso String.IsNullOrEmpty(TxtShipYMDFrom.Text) AndAlso String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="請求年月もしくは発送年月日", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '発送年月日の未入力
                If String.IsNullOrEmpty(TxtShipYMDFrom.Text) OrElse String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    '請求年月の未入力
                    If String.IsNullOrEmpty(TxtDownloadMonth.Text) OrElse String.IsNullOrEmpty(TxtBllingToYM.Text) Then
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="請求年月もしくは発送年月日", needsPopUp:=True)
                        TxtShipYMDFrom.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

                ' 請求年月日付大小チェック
                If Not String.IsNullOrEmpty(TxtDownloadMonth.Text) AndAlso Not String.IsNullOrEmpty(TxtBllingToYM.Text) Then
                    Try
                        If TxtDownloadMonth.Text > TxtBllingToYM.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtDownloadMonth.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If
                ' 発送年月日日付大小チェック
                If Not String.IsNullOrEmpty(TxtShipYMDFrom.Text) AndAlso Not String.IsNullOrEmpty(TxtShipYMDTo.Text) Then
                    Try
                        If TxtShipYMDFrom.Text > TxtShipYMDTo.Text Then
                            Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtShipYMDFrom.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtShipYMDFrom.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If
                '並び順
                If String.IsNullOrEmpty(TxtSort.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "並び順", needsPopUp:=True)
                    TxtSort.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    If TxtSort.Text <> "1" AndAlso TxtSort.Text <> "2" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="並び順", I_PARA02:="１～２", needsPopUp:=True)
                        TxtSort.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '受託人指定
                If Not String.IsNullOrEmpty(TxtTrustee.Text) Then
                    If TxtTrustee.Text <> "0" AndAlso TxtTrustee.Text <> "1" AndAlso TxtTrustee.Text <> "2" AndAlso TxtTrustee.Text <> "3" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="受託人指定", I_PARA02:="０～３", needsPopUp:=True)
                        TxtTrustee.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '加減額表示指定
                If Not String.IsNullOrEmpty(TxtAddSub.Text) Then
                    If TxtAddSub.Text <> "1" AndAlso TxtAddSub.Text <> "2" Then
                        Master.Output(C_MESSAGE_NO.CTN_DATE_UPDSTART, C_MESSAGE_TYPE.ERR, I_PARA01:="加減額表示指定", I_PARA02:="１～２", needsPopUp:=True)
                        TxtAddSub.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If

            '〇発送日報
            Case "LNT0010"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象日付", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

            '○コンテナ回送費明細（発駅・受託人別）
            Case "PRT0011"
                '対象年月
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象年月", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

            '○ コンテナ回送費明細(コンテナ別)
            Case "PRT0012"
                ' 年月日(From)
                Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                        TxtStYMDCode.Text = CDate(TxtStYMDCode.Text).ToString("yyyy/MM/dd")
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(開始)", needsPopUp:=True)
                    TxtStYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 年月日(To)
                Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                        TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text).ToString("yyyy/MM/dd")
                    Else
                        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                        TxtEndYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日(終了)", needsPopUp:=True)
                    TxtEndYMDCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

                ' 日付大小チェック
                If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                    Try
                        If TxtStYMDCode.Text > TxtEndYMDCode.Text Then
                            Master.Output(C_MESSAGE_NO.CTN_LEASE_FINAL_DATEFROMTO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            TxtStYMDCode.Focus()
                            O_RTN = "ERR"
                            Exit Sub
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtStYMDCode.Text & ":" & TxtEndYMDCode.Text)
                        TxtStYMDCode.Focus()
                        O_RTN = "ERR"
                        Exit Sub
                    End Try
                End If
            '○ リース満了一覧表
            Case "PRT0013"
                '対象年月
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象年月", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            '○ 請求先・勘定科目別・計上店別営業収入計上一覧(全勘定科目)
            Case "PRT0014"
                '期間種別が任意期間以外の場合
                If WF_PERIODTYPE_DDL.SelectedValue <> "0001" Then
                    '年度が正しくない場合
                    If Not (Len(WF_FISCALYEAR.Text) = 4 AndAlso IsNumeric(WF_FISCALYEAR.Text)) Then
                        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="年度に日付", I_PARA02:="設定", needsPopUp:=True)
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
                '期間開始日が未定の場合
                If String.IsNullOrEmpty(WF_PERIOD_FROM.Text) Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="期間開始日", I_PARA02:="設定", needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '期間開始日に日付が設定されていない場合
                If Not IsDate(WF_PERIOD_FROM.Text) Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="期間開始日", I_PARA02:="日付設定", needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '期間終了日が未定の場合
                If String.IsNullOrEmpty(WF_PERIOD_TO.Text) Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="期間終了日", I_PARA02:="設定", needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '期間終了日に日付が設定されていない場合
                If Not IsDate(WF_PERIOD_TO.Text) Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="期間終了日", I_PARA02:="日付設定", needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '期間開始日と期間終了日の範囲が反転
                If CDate(WF_PERIOD_FROM.Text) > CDate(WF_PERIOD_TO.Text) Then
                    Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
            '○ 科目別集計表
            Case "PRT0015"
                '対象年月
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象年月", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            '〇回送運賃報告書
            Case "PRT0017"
                '対象支店
                If WF_ORGCODE_MULTIPLE.HasSelectedValue() = False Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象支店", needsPopUp:=True)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '対象年月
                If String.IsNullOrEmpty(TxtDownloadMonth.Text) Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "対象年月", needsPopUp:=True)
                    TxtDownloadMonth.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If

        End Select

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0012WRKINC.TITLEKBNS)

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledDBClick()

        Dim WW_prmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                leftview.Visible = True
                If CInt(WF_LeftMViewChange.Value) = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "TxtStYMDCode"         '有効年月日(From)
                            .WF_Calendar.Text = Me.TxtStYMDCode.Text
                        Case "TxtEndYMDCode"        '有効年月日(To)
                            .WF_Calendar.Text = Me.TxtEndYMDCode.Text
                        Case "TxtShipYMDFrom"       '発送年月日(From)
                            .WF_Calendar.Text = Me.TxtShipYMDFrom.Text
                        Case "TxtShipYMDTo"         '発送年月日(To)
                            .WF_Calendar.Text = Me.TxtShipYMDTo.Text
                        Case "WF_PERIOD_FROM"       '期間(From)
                            .WF_Calendar.Text = Me.WF_PERIOD_FROM.Text
                        Case "WF_PERIOD_TO"         '期間(To)
                            .WF_Calendar.Text = Me.WF_PERIOD_TO.Text

                    End Select
                    .ActiveCalendar()

                Else
                    Select Case WF_FIELD.Value
                        Case "TxtReportId"          '帳票名
                            WW_prmData = work.CreateREPORTParam(Master.USERCAMP, "REPORTLIST")
                        Case "TxtOrgCode"           '対象支店
                            WW_prmData = work.CreateUORGParam(Master.USERCAMP)
                        Case "TxtStaCode"           '駅
                            leftview.Visible = False
                            '検索画面
                            DisplayView_mspStationSingle()
                            '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                            WF_LeftboxOpen.Value = ""
                            Exit Sub
                        Case "TxtCtnType"           'コンテナ記号
                            WW_prmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE)
                        Case "TxtStCtnNo"           'コンテナ番号(開始)
                            WW_prmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCtnType.Text)
                        Case "TxtEndCtnNo"          'コンテナ番号(終了)
                            WW_prmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCtnType.Text)
                        Case "TxtMode"              '処理種別
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "MODE")
                        Case "TxtJurisdiction"      '所管部
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "JURISDICTIONCD")
                        Case "TxtSearch"            '検索種別
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "SEARCH")
                        Case "TxtDepTrustee"        '発受託人コード
                            WW_prmData = work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtStaCode.Text)
                        Case "TxtDepTrusteeSub"     '発受託人サブコード
                            WW_prmData = work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtStaCode.Text, TxtDepTrustee.Text)
                        Case "TxtEndDay"            '締め日
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "INVCYCL")
                        Case "TxtSort"              '並び順
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "SORT")
                        Case "TxtKeijoBase"         '計上ベース
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "KEIJOBASE")
                        Case "TxtTrustee"           '受託人指定
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "TRUSTEEKBN")
                        Case "TxtBilling"           '請求先指定
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "BILLINGKBN")
                        Case "TxtAddSub"            '加減額表示指定
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "ADDSUBKBN")
                        Case "TxtBranchBase"         '処理種別
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "BRANCHBASE")
                        Case "TxtDepArrBase"         '発着ベース
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "DEPARRBASE")
                        Case "TxtStackFree"          '積空区分
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "STACKFREE")
                        Case "TxtBigCtnCd"           '大分類コード
                            WW_prmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                        Case "TxtReportSetting"      '出力設定
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "REPORTSETTING")
                        Case "TxtReplace"            '入れ替え
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "REPLACE")
                        Case "TxtPayeeCode"          '支払先コード
                            leftview.Visible = False
                            '検索画面
                            DisplayView_mspToriSingle(TxtPayeeCode.Text)
                            '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                            WF_LeftboxOpen.Value = ""
                            Exit Sub
                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_prmData)
                    .ActiveListBox()
                End If
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledChange()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "TxtReportId"          '帳票名
                CODENAME_get("REPORTID", TxtReportId.Text, TxtReportName.Text, WW_Dummy)
                TxtReportId.Focus()
            Case "TxtStaCode"           '駅コード
                CODENAME_get("STATION", TxtStaCode.Text, LblStaName.Text, WW_Dummy)
                TxtStaCode.Focus()
            Case "TxtMode"              '処理種別
                CODENAME_get("MODE", TxtMode.Text, LblMode.Text, WW_Dummy)
                TxtMode.Focus()
            Case "TxtDepTrustee"        '発受託人コード
                CODENAME_get("DEPTRUSTEE", TxtDepTrustee.Text, LblDepTrustee.Text, WW_Dummy)
                TxtDepTrustee.Focus()
            Case "TxtDepTrusteeSub"     '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUB", TxtDepTrusteeSub.Text, LblDepTrusteeSub.Text, WW_Dummy)
                TxtDepTrusteeSub.Focus()
            Case "TxtSort"              '並び順
                CODENAME_get("SORT", TxtSort.Text, LblSort.Text, WW_Dummy)
                TxtSort.Focus()
            Case "TxtTrustee"           '受託人指定
                CODENAME_get("TRUSTEE", TxtTrustee.Text, LblTrustee.Text, WW_Dummy)
                TxtTrustee.Focus()
            Case "TxtAddSub"            '加減額表示指定
                CODENAME_get("ADDSUB", TxtAddSub.Text, LblAddSub.Text, WW_Dummy)
                TxtAddSub.Focus()
            Case "TxtBranchBase"        '処理種別
                CODENAME_get("BRANCHBASE", TxtBranchBase.Text, LblBranchBase.Text, WW_Dummy)
                TxtBranchBase.Focus()
            Case "TxtDepArrBase"        '発着ベース
                CODENAME_get("DEPARRBASE", TxtDepArrBase.Text, LblDepArrBase.Text, WW_Dummy)
                TxtDepArrBase.Focus()
            Case "TxtStackFree"         '積空区分
                CODENAME_get("STACKFREE", TxtStackFree.Text, LblStackFree.Text, WW_Dummy)
                TxtStackFree.Focus()
            Case "TxtBigCtnCd"          '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCtnCd.Text, LblBigCtnCd.Text, WW_Dummy)
                TxtBigCtnCd.Focus()
            Case "TxtReportSetting"     '出力設定
                CODENAME_get("REPORTSETTING", TxtReportSetting.Text, LblReportSetting.Text, WW_Dummy)
                TxtReportSetting.Focus()
            Case "TxtReplace"           '入れ替え
                CODENAME_get("REPLACE", TxtReplace.Text, LblReplace.Text, WW_Dummy)
                TxtReplace.Focus()
            Case "TxtPayeeCode"         '支払先
                CODENAME_get("PAYEE", TxtPayeeCode.Text, LblPayeeName.Text, WW_Dummy)
                TxtReplace.Focus()

        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 取引先検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspToriSingle(Optional ByVal prmkey As String = "")

        Me.mspToriSingle.InitPopUp()
        Me.mspToriSingle.SelectionMode = ListSelectionMode.Single
        Me.mspToriSingle.SQL = CmnSearchSQL.GetToriSQL

        Me.mspToriSingle.KeyFieldName = "KEYCODE"
        Me.mspToriSingle.DispFieldList.AddRange(CmnSearchSQL.GetToriTitle)

        '画面表示する絞り込みドロップダウンの設定(組織コード)
        Me.mspToriSingle.FilterField.Add("ORGNAMES", "提出部店")

        Me.mspToriSingle.ShowPopUpList(prmkey)

        '組織名取得
        Dim orgName = Master.USER_ORGNAME
        Me.mspToriSingle.ddlFilterInit("", orgName)

    End Sub

    ''' <summary>
    ''' 取引先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspToriSingle()

        Dim selData = Me.mspToriSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtPayeeCode.ID
                Me.TxtPayeeCode.Text = selData("TORICODE").ToString
                Me.LblPayeeName.Text = selData("TORINAME").ToString & selData("DIVNAME").ToString
                Me.TxtPayeeCode.Focus()

                'Case TxtDepStationName.ID
                '    Me.TxtDepStationCode.Text = selData("STATION").ToString
                '    Me.TxtDepStationName.Text = selData("NAMES").ToString
                '    Me.TxtDepStationName.Focus()

                'Case TxtArrStationName.ID
                '    Me.TxtArrStationCode.Text = selData("STATION").ToString
                '    Me.TxtArrStationName.Text = selData("NAMES").ToString
                '    Me.TxtArrStationName.Focus()
        End Select

        'ポップアップの非表示
        Me.mspToriSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 駅検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspStationSingle()

        Me.mspStationSingle.InitPopUp()
        Me.mspStationSingle.SelectionMode = ListSelectionMode.Single
        Me.mspStationSingle.SQL = CmnSearchSQL.GetStationSQL(work.WF_SEL_CAMPCODE.Text)

        Me.mspStationSingle.KeyFieldName = "KEYCODE"
        Me.mspStationSingle.DispFieldList.AddRange(CmnSearchSQL.GetStationTitle)

        Me.mspStationSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 駅選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Me.TxtStaCode.Text = selData("STATION").ToString
        Me.LblStaName.Text = selData("NAMES").ToString
        Me.TxtStaCode.Focus()


        'ポップアップの非表示
        Me.mspStationSingle.HidePopUp()

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
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "TxtReportId"          '帳票名
                TxtReportId.Text = WW_SelectValue
                TxtReportName.Text = WW_SelectText
                TxtReportId.Focus()
            Case "TxtStYMDCode"         '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtStYMDCode.Text = ""
                    Else
                        Me.TxtStYMDCode.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.TxtStYMDCode.Focus()
            Case "TxtEndYMDCode"        '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtEndYMDCode.Text = ""
                    Else
                        Me.TxtEndYMDCode.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.TxtEndYMDCode.Focus()
            Case "TxtShipYMDFrom"         '発送年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtShipYMDFrom.Text = ""
                    Else
                        Me.TxtShipYMDFrom.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.TxtShipYMDFrom.Focus()
            Case "TxtShipYMDTo"        '発送年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtShipYMDTo.Text = ""
                    Else
                        Me.TxtShipYMDTo.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.TxtShipYMDTo.Focus()
            Case "WF_PERIOD_FROM"         '期間(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.WF_PERIOD_FROM.Text = ""
                    Else
                        Me.WF_PERIOD_FROM.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.WF_PERIOD_FROM.Focus()
            Case "WF_PERIOD_TO"        '期間(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.WF_PERIOD_TO.Text = ""
                    Else
                        Me.WF_PERIOD_TO.Text = WW_DATE.ToString("yyyy/MM/dd")
                    End If

                Catch ex As Exception
                End Try
                Me.WF_PERIOD_TO.Focus()
            Case "TxtStaCode"           '駅コード
                TxtStaCode.Text = WW_SelectValue
                LblStaName.Text = WW_SelectText
                TxtStaCode.Focus()
            Case "TxtCtnType"           'コンテナ記号
                TxtCtnType.Text = WW_SelectValue
                TxtCtnType.Focus()
            Case "TxtStCtnNo"           'コンテナ番号(開始)
                TxtStCtnNo.Text = WW_SelectValue
                TxtStCtnNo.Focus()
            Case "TxtEndCtnNo"          'コンテナ番号(終了)
                TxtEndCtnNo.Text = WW_SelectValue
                TxtEndCtnNo.Focus()
            Case "TxtMode"              '処理種別
                TxtMode.Text = WW_SelectValue
                LblMode.Text = WW_SelectText
                TxtMode.Focus()
            Case "TxtDepTrustee"        '発受託人コード
                TxtDepTrustee.Text = WW_SelectValue
                LblDepTrustee.Text = WW_SelectText
                TxtDepTrustee.Focus()
            Case "TxtDepTrusteeSub"     '発受託人サブコード
                TxtDepTrusteeSub.Text = WW_SelectValue
                LblDepTrusteeSub.Text = WW_SelectText
                TxtDepTrusteeSub.Focus()
            Case "TxtSort"              '並び順
                TxtSort.Text = WW_SelectValue
                LblSort.Text = WW_SelectText
                TxtSort.Focus()
            Case "TxtTrustee"           '受託人指定
                TxtTrustee.Text = WW_SelectValue
                LblTrustee.Text = WW_SelectText
                TxtTrustee.Focus()
            Case "TxtAddSub"            '加減額表示指定
                TxtAddSub.Text = WW_SelectValue
                LblAddSub.Text = WW_SelectText
                TxtAddSub.Focus()
            Case "TxtBranchBase"        '処理種別
                TxtBranchBase.Text = WW_SelectValue
                LblBranchBase.Text = WW_SelectText
                TxtBranchBase.Focus()
            Case "TxtDepArrBase"        '発着ベース
                TxtDepArrBase.Text = WW_SelectValue
                LblDepArrBase.Text = WW_SelectText
                TxtDepArrBase.Focus()
            Case "TxtStackFree"         '積空区分
                TxtStackFree.Text = WW_SelectValue
                LblStackFree.Text = WW_SelectText
                TxtStackFree.Focus()
            Case "TxtBigCtnCd"          '大分類コード
                TxtBigCtnCd.Text = WW_SelectValue
                LblBigCtnCd.Text = WW_SelectText
                TxtBigCtnCd.Focus()
            Case "TxtReportSetting"     '出力設定
                TxtReportSetting.Text = WW_SelectValue
                LblReportSetting.Text = WW_SelectText
                TxtReportSetting.Focus()
            Case "TxtReplace"           '入れ替え
                TxtReplace.Text = WW_SelectValue
                LblReplace.Text = WW_SelectText
                TxtReplace.Focus()
            Case "TxtPayeeCode"         '支払先
                TxtPayeeCode.Text = WW_SelectValue
                LblPayeeName.Text = WW_SelectText
                TxtPayeeCode.Focus()
        End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "TxtReportId"          '帳票名
                TxtReportId.Focus()
            Case "TxtStaCode"           '駅コード
                TxtStaCode.Focus()
            Case "TxtCtnType"           'コンテナ記号
                TxtCtnType.Focus()
            Case "TxtStCtnNo"           'コンテナ番号(開始)
                TxtStCtnNo.Focus()
            Case "TxtEndCtnNo"          'コンテナ番号(終了)
                TxtEndCtnNo.Focus()
            Case "TxtMode"              '処理種別
                TxtMode.Focus()
            Case "TxtDepTrustee"        '発受託人コード
                TxtDepTrustee.Focus()
            Case "TxtDepTrusteeSub"     '発受託人サブコード
                TxtDepTrusteeSub.Focus()
            Case "TxtSort"              '並び順
                TxtSort.Focus()
            Case "TxtTrustee"           '受託人指定
                TxtTrustee.Focus()
            Case "TxtAddSub"            '加減額表示指定
                TxtAddSub.Focus()
            Case "TxtBranchBase"        '処理種別
                TxtBranchBase.Focus()
            Case "TxtDepArrBase"        '発着ベース
                TxtDepArrBase.Focus()
            Case "TxtStackFree"         '積空区分
                TxtStackFree.Focus()
            Case "TxtBigCtnCd"          '大分類コード
                TxtBigCtnCd.Focus()
            Case "TxtReportSetting"     '出力設定
                TxtReportSetting.Focus()
            Case "TxtReplace"           '入れ替え
                TxtReplace.Focus()
            Case "TxtPayeeCode"         '支払先
                TxtPayeeCode.Focus()
        End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

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
                Case "ORG"              '対象支店
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, Master.USERCAMP))
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, Master.USERCAMP))
                    End If
                Case "REPORTID"         '帳票ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REPORT, I_VALUE, O_TEXT, O_RTN, work.CreateREPORTParam(Master.USERCAMP, "REPORTLIST"))
                Case "STATION"          '駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "MODE"             '処理
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "MODE"))
                Case "JURISDICTION"     '経理資産区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ACCOUNTINGASSETSKBN, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "ACCOUNTINGASSETSKBN"))
                Case "DEPTRUSTEE"       '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtStaCode.Text))
                Case "DEPTRUSTEESUB"    '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtStaCode.Text, TxtDepTrustee.Text))
                Case "ENDDAY"           '締め日
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_INVCYCL, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "INVCYCL"))
                Case "SORT"             '並び順
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SORT, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "SORT"))
                Case "KEIJOBASE"        '計上ベース
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEIJOBASE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "KEIJOBASE"))
                Case "TRUSTEE"          '発託人指定
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRUSTEEKBN, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "TRUSTEEKBN"))
                Case "BILLING"          '請求先指定
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BILLINGKBN, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "BILLINGKBN"))
                Case "ADDSUB"           '加減額表示指定
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ADDSUBKBN, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "ADDSUBKBN"))
                Case "BRANCHBASE"       '処理種別
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BRANCHBASE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "BRANCHBASE"))
                Case "DEPARRBASE"       '発着ベース
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRBASE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DEPARRBASE"))
                Case "STACKFREE"        '積空区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STACKFREE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "STACKFREE"))
                Case "BIGCTNCD"         '大分類
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "REPORTSETTING"    '出力設定
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REPORTSETTING, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "REPORTSETTING"))
                Case "REPLACE"          '入れ替え
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REPLACE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "REPLACE"))
                Case "PAYEE"            '支払先
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "KEKKJM"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 発送日報データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function DailyShipmentDataGet(ReportID As String, type As String) As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                If ReportID = "PRT0001" Then
                    If type = "A" Then
                        SQLcmd.CommandText = "lng.[PRT_DAILY_SHIPMENTREPORT_A]"
                        SqlName = "PRT_DAILY_SHIPMENTREPORT_A"
                    ElseIf type = "B" Then
                        SQLcmd.CommandText = "lng.[PRT_DAILY_SHIPMENTREPORT_B]"
                        SqlName = "PRT_DAILY_SHIPMENTREPORT_B"
                    End If
                ElseIf ReportID = "PRT0002" Then
                    SQLcmd.CommandText = "lng.[PRT_OTHERSTATION_SHIPMENTDETAIL]"
                    SqlName = "PRT_OTHERSTATION_SHIPMENTDETAIL"
                End If
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)             ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)               ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piOFFICECODE", MySqlDbType.VarChar, 6) ' 支店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                If Me.TxtEndYMDCode.Text <> "" Then
                    PARA2.Value = CDate(Me.TxtEndYMDCode.Text)
                Else
                    PARA2.Value = DBNull.Value
                End If
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA3.Value = DBNull.Value
                Else
                    PARA3.Value = ddlSelectOffice.SelectedValue
                End If
                PARA4.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' コンテナ留置先一覧データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function PutContainerDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        'コンテナ留置先一覧作成用ワークファイル作成
        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[INS_PUTCTNDATA]"
                SqlName = "INS_PUTCTNDATA"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)             ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)               ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piMODE", MySqlDbType.VarChar, 1)       ' 処理
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDUSER", MySqlDbType.VarChar, 20)   ' ユーザ
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDTERMID", MySqlDbType.VarChar, 20) ' 端末
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If Me.TxtStYMDCode.Text <> "" Then
                    PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                Else
                    PARA1.Value = DBNull.Value
                End If
                PARA2.Value = CDate(Me.TxtEndYMDCode.Text)
                PARA3.Value = Me.TxtMode.Text
                PARA4.Value = Master.USERID
                PARA5.Value = Master.USERTERMID
                PARA6.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                SQLcmd.ExecuteReader()

            End Using

        End Using

        'コンテナ留置先一覧データ取得
        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_PUTCTNDATA]"
                SqlName = "PRT_PUTCTNDATA"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piOFFICECODE", MySqlDbType.VarChar, 6) ' 支店
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA1.Value = DBNull.Value
                Else
                    PARA1.Value = ddlSelectOffice.SelectedValue
                End If
                PARA2.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 品目別販売実績表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function SalesResultsDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_SALESRESULTS_ITEM]"
                SqlName = "PRT_SALESRESULTS_ITEM"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)               ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)                 ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHOFFICE", MySqlDbType.VarChar, 6) ' 支店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piDEPSTATION", MySqlDbType.Int32, 6)       ' 発駅
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)   ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                PARA2.Value = CDate(Me.TxtEndYMDCode.Text)
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA3.Value = DBNull.Value
                Else
                    PARA3.Value = ddlSelectOffice.SelectedValue
                End If
                If Me.TxtStaCode.Text <> "" Then
                    PARA4.Value = Me.TxtStaCode.Text
                Else
                    PARA4.Value = DBNull.Value
                End If

                PARA5.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt

    End Function

    ''' <summary>
    ''' コンテナ動静表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function MovementContainerDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        'コンテナ運用ワークファイル作成
        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[INS_REUNYF_WK]"
                SqlName = "INS_REUNYF_WK"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.Date)               ' 処理日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piCTNTYPE", MySqlDbType.VarChar, 5)      ' コンテナ記号
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piSTRCTNNO", MySqlDbType.Int32, 8)         ' コンテナ記号(開始)
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piENDCTNNO", MySqlDbType.Int32, 8)         ' コンテナ記号(終了)
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piSTATION", MySqlDbType.Int32, 6)          ' 現在駅
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@piJURISDICTION", MySqlDbType.VarChar, 2) ' 経理資産区分
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDUSER", MySqlDbType.VarChar, 20)     ' ユーザ
                Dim PARA9 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDTERMID", MySqlDbType.VarChar, 20)   ' 端末
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                If Me.TxtCtnType.Text <> "" Then
                    PARA2.Value = Me.TxtCtnType.Text
                Else
                    PARA2.Value = DBNull.Value
                End If
                If Me.TxtStCtnNo.Text <> "" Then
                    PARA3.Value = Me.TxtStCtnNo.Text
                Else
                    PARA3.Value = DBNull.Value
                End If
                If Me.TxtEndCtnNo.Text <> "" Then
                    PARA4.Value = Me.TxtEndCtnNo.Text
                Else
                    PARA4.Value = DBNull.Value
                End If
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA5.Value = DBNull.Value
                Else
                    PARA5.Value = ddlSelectOffice.SelectedValue
                End If
                If Me.TxtStaCode.Text <> "" Then
                    PARA6.Value = Me.TxtStaCode.Text
                Else
                    PARA6.Value = DBNull.Value
                End If
                PARA7.Value = Me.WF_ACCOUNTINGASSETSKBN_DDL.SelectedValue
                PARA8.Value = Master.USERID
                PARA9.Value = Master.USERTERMID
                PARA10.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                SQLcmd.ExecuteReader()

            End Using

        End Using

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_MOVEMENTCTNDATA]"
                SqlName = "PRT_MOVEMENTCTNDATA"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.Date)               ' 処理日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piCTNTYPE", MySqlDbType.VarChar, 5)      ' コンテナ記号
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piSTRCTNNO", MySqlDbType.Int32, 8)         ' コンテナ記号(開始)
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piENDCTNNO", MySqlDbType.Int32, 8)         ' コンテナ記号(終了)
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piSTATION", MySqlDbType.Int32, 6)          ' 現在駅
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@piJURISDICTION", MySqlDbType.VarChar, 2) ' 経理資産区分
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@piSTAGNATION", MySqlDbType.Int32, 4)       ' 停滞日数
                Dim PARA9 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)   ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                If Me.TxtCtnType.Text <> "" Then
                    PARA2.Value = Me.TxtCtnType.Text
                Else
                    PARA2.Value = DBNull.Value
                End If
                If Me.TxtStCtnNo.Text <> "" Then
                    PARA3.Value = Me.TxtStCtnNo.Text
                Else
                    PARA3.Value = DBNull.Value
                End If
                If Me.TxtEndCtnNo.Text <> "" Then
                    PARA4.Value = Me.TxtEndCtnNo.Text
                Else
                    PARA4.Value = DBNull.Value
                End If
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA5.Value = DBNull.Value
                Else
                    PARA5.Value = ddlSelectOffice.SelectedValue
                End If
                If Me.TxtStaCode.Text <> "" Then
                    PARA6.Value = Me.TxtStaCode.Text
                Else
                    PARA6.Value = DBNull.Value
                End If
                PARA7.Value = Me.WF_ACCOUNTINGASSETSKBN_DDL.SelectedValue
                If Me.TxtStagnation.Text <> "" Then
                    PARA8.Value = CInt(Me.TxtStagnation.Text)
                Else
                    PARA8.Value = 0
                End If
                PARA9.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 発駅・通運別合計表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function TransportTotalDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_TRANSPORTTOTAL]"
                SqlName = "PRT_TRANSPORTTOTAL"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEECD", MySqlDbType.Int32, 5)        ' 受託人
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEESUBCD", MySqlDbType.Int32, 5)     ' 受託人サブ
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piSTATION", MySqlDbType.Int32, 6)          ' 駅
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piBILLINGYM", MySqlDbType.VarChar, 7)    ' 計上年月
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPFROM", MySqlDbType.VarChar, 10)    ' 発送年月日FROM
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPTO", MySqlDbType.VarChar, 10)      ' 発送年月日TO
                Dim PARA9 As MySqlParameter = SQLcmd.Parameters.Add("@piSORT", MySqlDbType.VarChar, 1)         ' 並び順
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEEKBN", MySqlDbType.VarChar, 1)  ' 受託人絞り込み
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@piADDSUBKBN", MySqlDbType.VarChar, 1)   ' 加減額表示設定
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA1.Value = DBNull.Value
                Else
                    PARA1.Value = ddlSelectOffice.SelectedValue
                End If
                If Me.TxtDepTrustee.Text <> "" Then
                    PARA2.Value = Me.TxtDepTrustee.Text
                Else
                    PARA2.Value = DBNull.Value
                End If

                If Me.TxtDepTrusteeSub.Text <> "" Then
                    PARA3.Value = Me.TxtDepTrusteeSub.Text
                Else
                    PARA3.Value = DBNull.Value
                End If
                If Me.TxtStaCode.Text <> "" Then
                    PARA4.Value = Me.TxtStaCode.Text
                Else
                    PARA4.Value = DBNull.Value
                End If
                If Me.TxtDownloadMonth.Text <> "" Then
                    PARA6.Value = Me.TxtDownloadMonth.Text
                    PARA7.Value = DBNull.Value
                    PARA8.Value = DBNull.Value
                Else
                    PARA6.Value = DBNull.Value
                    PARA7.Value = Me.TxtShipYMDFrom.Text
                    PARA8.Value = Me.TxtShipYMDTo.Text
                End If
                If Me.TxtSort.Text <> "" Then
                    PARA9.Value = Me.TxtSort.Text
                Else
                    PARA9.Value = "1"
                End If
                If Me.TxtTrustee.Text <> "" Then
                    PARA11.Value = Me.TxtTrustee.Text
                Else
                    PARA11.Value = "0"
                End If
                If Me.TxtAddSub.Text <> "" Then
                    PARA13.Value = Me.TxtAddSub.Text
                Else
                    PARA13.Value = "1"
                End If
                PARA14.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' リース料明細チェックリストデータ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function LeaseFeeDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_LEASEFEE_CHEAKLIST]"
                SqlName = "PRT_LEASEFEE_CHEAKLIST"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piMODE", MySqlDbType.VarChar, 1)         ' 処理
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTARGETYM", MySqlDbType.VarChar, 7)     ' 対象年月
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)   ' メッセージ

                If Me.TxtBranchBase.Text <> "" Then
                    PARA1.Value = Me.TxtBranchBase.Text
                Else
                    PARA1.Value = "1"
                End If
                PARA2.Value = Me.TxtDownloadMonth.Text
                If ddlSelectLeaseOffice.SelectedValue = "ALL" Then
                    PARA3.Value = DBNull.Value
                Else
                    PARA3.Value = ddlSelectLeaseOffice.SelectedValue
                End If
                PARA4.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 支店間流動表(金額)データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function InterBranchChartAmountDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_INTERBRANCHCHART_AMOUNT]"
                SqlName = "PRT_INTERBRANCHCHART_AMOUNT"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)               ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)                 ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piBASE", MySqlDbType.VarChar, 1)         ' ベース
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piSTACKFREE", MySqlDbType.Int32, 1)        ' 積空区分
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piBIGCTNCD", MySqlDbType.VarChar, 2)     ' 大分類
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piREPORT", MySqlDbType.VarChar, 1)       ' 出力設定
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)   ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                PARA2.Value = CDate(Me.TxtEndYMDCode.Text)
                PARA3.Value = Me.TxtDepArrBase.Text
                PARA4.Value = Me.TxtStackFree.Text
                If Me.TxtBigCtnCd.Text <> "" Then
                    PARA5.Value = Me.TxtBigCtnCd.Text
                Else
                    PARA5.Value = DBNull.Value
                End If
                PARA6.Value = Me.TxtReportSetting.Text
                PARA7.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 支店間流動表(個数)・前年対比データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function InterBranchChartQuantityDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_INTERBRANCHCHART_QUANTITY]"
                SqlName = "PRT_INTERBRANCHCHART_QUANTITY"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)               ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)                 ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piBASE", MySqlDbType.VarChar, 1)         ' ベース
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piREPLACE", MySqlDbType.VarChar, 1)      ' 最新マスタ
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piBIGCTNCD", MySqlDbType.VarChar, 2)     ' 大分類
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piREPORT", MySqlDbType.VarChar, 1)       ' 出力設定
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)   ' メッセージ

                PARA1.Value = CDate(Me.TxtStYMDCode.Text)
                PARA2.Value = CDate(Me.TxtEndYMDCode.Text)
                PARA3.Value = Me.TxtDepArrBase.Text
                PARA4.Value = Me.TxtReplace.Text
                If Me.TxtBigCtnCd.Text <> "" Then
                    PARA5.Value = Me.TxtBigCtnCd.Text
                Else
                    PARA5.Value = DBNull.Value
                End If
                PARA6.Value = Me.TxtReportSetting.Text
                PARA7.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 発駅・通運別合計表(期間)データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function TransportTotalPeriodDataGet() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_TRANSPORTTOTAL_PERIOD]"
                SqlName = "PRT_TRANSPORTTOTAL_PERIOD"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEECD", MySqlDbType.Int32, 5)        ' 受託人
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEESUBCD", MySqlDbType.Int32, 5)     ' 受託人サブ
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piSTATION", MySqlDbType.Int32, 6)          ' 駅
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piBILLINGFROMYM", MySqlDbType.VarChar, 7) ' 計上年月(FROM)
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@piBILLINGTOYM", MySqlDbType.VarChar, 7)  ' 計上年月(TO)
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPFROM", MySqlDbType.VarChar, 10)    ' 発送年月日(FROM)
                Dim PARA9 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPTO", MySqlDbType.VarChar, 10)      ' 発送年月日(TO)
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@piSORT", MySqlDbType.VarChar, 1)        ' 並び順
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEEKBN", MySqlDbType.VarChar, 1)  ' 受託人絞り込み
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@piADDSUBKBN", MySqlDbType.VarChar, 1)   ' 加減額表示設定
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA1.Value = DBNull.Value
                Else
                    PARA1.Value = ddlSelectOffice.SelectedValue
                End If
                If Me.TxtDepTrustee.Text <> "" Then
                    PARA2.Value = Me.TxtDepTrustee.Text
                Else
                    PARA2.Value = DBNull.Value
                End If

                If Me.TxtDepTrusteeSub.Text <> "" Then
                    PARA3.Value = Me.TxtDepTrusteeSub.Text
                Else
                    PARA3.Value = DBNull.Value
                End If
                If Me.TxtStaCode.Text <> "" Then
                    PARA4.Value = Me.TxtStaCode.Text
                Else
                    PARA4.Value = DBNull.Value
                End If
                If Me.TxtDownloadMonth.Text <> "" Then
                    PARA6.Value = Me.TxtDownloadMonth.Text
                Else
                    PARA6.Value = DBNull.Value
                End If
                If Me.TxtBllingToYM.Text <> "" Then
                    PARA7.Value = Me.TxtBllingToYM.Text
                Else
                    PARA7.Value = DBNull.Value
                End If
                If Me.TxtShipYMDFrom.Text <> "" AndAlso Me.TxtDownloadMonth.Text = "" AndAlso Me.TxtBllingToYM.Text = "" Then
                    PARA8.Value = Me.TxtShipYMDFrom.Text
                Else
                    PARA8.Value = DBNull.Value
                End If
                If Me.TxtShipYMDTo.Text <> "" AndAlso Me.TxtDownloadMonth.Text = "" AndAlso Me.TxtBllingToYM.Text = "" Then
                    PARA9.Value = Me.TxtShipYMDTo.Text
                Else
                    PARA9.Value = DBNull.Value
                End If
                PARA10.Value = Me.TxtSort.Text
                If Me.TxtTrustee.Text <> "" Then
                    PARA11.Value = Me.TxtTrustee.Text
                Else
                    PARA11.Value = "0"
                End If
                If Me.TxtAddSub.Text <> "" Then
                    PARA12.Value = Me.TxtAddSub.Text
                Else
                    PARA12.Value = "1"
                End If
                PARA13.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' コンテナ回送費明細(発駅・受託人別)データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetRessnfList() As DataTable


        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_RESSNFITEM]"
                SqlName = "PRT_RESSNFITEM"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYM", MySqlDbType.VarChar, 6)     ' 対象年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODES", MySqlDbType.VarChar, 100)        ' 支店

                '対象年月
                If Me.TxtDownloadMonth.Text <> "" Then
                    PARA1.Value = Me.TxtDownloadMonth.Text.Replace("/", "")
                Else
                    PARA1.Value = DBNull.Value
                End If

                '支店
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA2.Value = DBNull.Value
                Else
                    PARA2.Value = ddlSelectOffice.SelectedValue
                End If

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using


        Return dt
    End Function


    ''' <summary>
    ''' レンタルコンテナ回送費明細(発駅・受託人別)データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetRentalCTNList() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_RENTAL_CONTAINER]"
                SqlName = "PRT_RENTAL_CONTAINER"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMFROM", MySqlDbType.VarChar, 10)     ' 年月日FROM
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMTO", MySqlDbType.VarChar, 10)     ' 年月日TO
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODES", MySqlDbType.VarChar, 100)    ' 対象支店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     ' 支払先コード

                '年月日
                PARA1.Value = Me.TxtStYMDCode.Text
                PARA2.Value = Me.TxtEndYMDCode.Text

                '支店
                If ddlSelectOffice.SelectedValue = "ALL" Then
                    PARA3.Value = DBNull.Value
                Else
                    PARA3.Value = ddlSelectOffice.SelectedValue
                End If

                '支払先コード
                If TxtPayeeCode.Text <> "" Then
                    PARA4.Value = TxtPayeeCode.Text
                Else
                    PARA4.Value = DBNull.Value
                End If

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using


        Return dt
    End Function

    ''' <summary>
    ''' リース満了一覧表　データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetLeaseExpirationList() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_LEASE_EXPIRATION_LIST_BILLOUTPUTORG]"
                SqlName = "PRT_LEASE_EXPIRATION_LIST_BILLOUTPUTORG"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYM", MySqlDbType.VarChar, 6)     ' 計上年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODES", MySqlDbType.VarChar, 100)    ' 対象支店
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@ALLBRANCH", MySqlDbType.Int32)    ' 全支店計出力


                '年月日
                PARA1.Value = Me.TxtDownloadMonth.Text.Replace("/", "")
                '支店
                If ddlSelectLeaseOffice.SelectedValue = "ALL" Then
                    PARA2.Value = DBNull.Value
                    PARA3.Value = 1
                Else
                    PARA2.Value = ddlSelectLeaseOffice.SelectedValue
                    PARA3.Value = 0
                End If

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using


        Return dt
    End Function

    ''' <summary>
    ''' 請求先・勘定科目別・計上店別営業収入計上一覧(全勘定科目)　データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetByAccountPrintData() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_BILLING_ACCOUNT_BY_DEPARTMENT_LIST]"
                SqlName = "PRT_BILLING_ACCOUNT_BY_DEPARTMENT_LIST"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMFROM", MySqlDbType.VarChar, 10)     ' 計上年月FROM
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMTO", MySqlDbType.VarChar, 10)    ' 計上年月TO
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@SORT", MySqlDbType.VarChar, 1)    ' 出力方法指定


                '計上年月FROM
                PARA1.Value = Me.WF_PERIOD_FROM.Text
                '計上年月TO
                PARA2.Value = Me.WF_PERIOD_TO.Text
                '出力方法指定
                PARA3.Value = "1"

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 科目別集計表　データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetAccountSummaryList() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_ACCOUNT_SUMMARY]"
                SqlName = "PRT_ACCOUNT_SUMMARY"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.VarChar, 6)     ' 計上年月

                '年月日
                PARA1.Value = Me.TxtDownloadMonth.Text.Replace("/", "")
                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt

    End Function

    ''' <summary>
    ''' 請求書種類取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Save_SearchItem()

        '会社コード
        work.WF_SEL_CAMPCODE.Text = Master.USERCAMP
        '状況
        Dim strSelStatus As String = ""
        Dim selList As ListBox = CType(Me.WF_INVTYPE.GetSelectedListData, ListBox)
        'チェックされている項目を取得
        For intCnt As Integer = 0 To selList.Items.Count - 1
            If intCnt > 0 Then
                strSelStatus = strSelStatus & ","
            End If
            strSelStatus = strSelStatus & "'" & CStr(selList.Items(intCnt).Value) & "'"
        Next
        work.WF_SRC_CONTRALNMODE.Text = strSelStatus

    End Sub

    ''' <summary>
    ''' 使用料明細データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function UsefeeDetailDataGet(ReportID As String, type As String, INVICETYPE As String) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                SQLcmd.CommandText = "lng.[PRT_USEFEE_DETAIL]"
                SqlName = "PRT_USEFEE_DETAIL"

                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.VarChar, 10)            ' 年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piOFFICECODE", MySqlDbType.VarChar, 6)       ' 支店
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piINVOICETYPE", MySqlDbType.VarChar, 2)      ' 請求書種類

                PARA1.Value = TxtDownloadMonth.Text.Replace("/", "")
                If ddlSelectLeaseOffice.SelectedValue = "ALL" OrElse ddlSelectLeaseOffice.SelectedValue = "" Then
                    PARA2.Value = DBNull.Value
                Else
                    PARA2.Value = ddlSelectLeaseOffice.SelectedValue
                End If

                If INVICETYPE = "1" Then
                    PARA3.Value = INVICETYPE
                ElseIf INVICETYPE = "2" Then
                    PARA3.Value = INVICETYPE
                ElseIf INVICETYPE = "3" Then
                    PARA3.Value = INVICETYPE
                End If

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' 回送運賃報告書　データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function FreeSendFeeDataGet(ByVal OfficeCode As String) As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_FREESENDFEE]"
                SqlName = "PRT_FREESENDFEE"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     ' 対象年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODES", MySqlDbType.VarChar, 100)    ' 対象支店

                '対象年月
                PARA1.Value = Me.TxtDownloadMonth.Text.Replace("/", "")
                '対象支店
                PARA2.Value = OfficeCode

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using


        Return dt
    End Function

    ''' <summary>
    ''' 請求部店取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOfficeCode() As String
        'エラーコード初期化
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Dim Org As String = ""
        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine(" SELECT")
            SQLBldr.AppendLine("     ORGCODE ")
            SQLBldr.AppendLine(" FROM")
            SQLBldr.AppendLine("     com.LNS0019_ORG")
            SQLBldr.AppendLine(" WHERE")
            SQLBldr.AppendLine("     CTNFLG = '1'")
            SQLBldr.AppendLine(" AND CLASS01 IN (1,2)")
            SQLBldr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD")

            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0012R GetOfficeCode")

                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:LNT0012R GetOfficeCode"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'エラーコードをDBエラーに設定
                WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            End Try

        End Using

        '支店コードをカンマ区切りに変更
        For Each row As DataRow In dt.Rows
            Org &= If(Org.Length > 0, "," & row("ORGCODE").ToString, row("ORGCODE").ToString)
        Next

        Return Org

    End Function

    ''' <summary>
    ''' 画面表示部店取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOfficeData() As DataTable
        'エラーコード初期化
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Dim dt = New DataTable
        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine(" SELECT")
            SQLBldr.AppendLine("     ORGCODE AS [key] ")
            SQLBldr.AppendLine("     , RTRIM(NAME) AS [value]  ")
            SQLBldr.AppendLine(" FROM")
            SQLBldr.AppendLine("     com.LNS0019_ORG")
            SQLBldr.AppendLine(" WHERE")
            SQLBldr.AppendLine("     CTNFLG = '1'")
            SQLBldr.AppendLine(" AND CLASS01 IN (1,2)")
            SQLBldr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD")

            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0012R GetOfficeData")

                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:LNT0012R GetOfficeData"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'エラーコードをDBエラーに設定
                WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            End Try

        End Using

        Return dt
    End Function

End Class
