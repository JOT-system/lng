''************************************************************
' 営業収入計上一覧表画面
' 作成日 2023/06/28
' 更新日 
' 作成者 星
' 更新者 
'
' 修正履歴:2023/06/28 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 営業収入計上一覧表画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0020OperatIncomeReportOutput
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIT0020Reporttbl As DataTable                   '帳票用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    Private Const CONST_VBFLG As String = "1"               'VB呼び出し元フラグ(プロシージャ呼び出し用)

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
                        WF_FIELD_DBClick()

                    Case "WF_ButtonSel",                '(左ボックス)選択ボタン押下
                         "WF_ListboxDBclick"            '(左ボックス)ダブルクリック
                        WF_ButtonSel_Click()

                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()

                    Case WF_PERIODTYPE_DDL.ID           '期間種別リスト変更
                        WF_FISCALYEAR_TextChange()

                    Case WF_FISCALYEAR.ID               '年度変更
                        WF_FISCALYEAR_TextChange()

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
        Master.MAPID = LNT0020WRKINC.MAPID

        'TxtReportId.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ' メニューからの画面遷移
        ' 画面間の情報クリア
        work.Initialize()

        '帳票種類
        WF_REPORTNAME.Items.Add(New ListItem("全勘定科目", "00"))
        WF_REPORTNAME.Items.Add(New ListItem("リースのみ", "01"))

        '期間種別リスト
        WF_PERIODTYPE_DDL.Items.Clear()
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("任意期間", "00"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("１Ｑ", "01"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("２Ｑ", "02"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("３Ｑ", "03"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("４Ｑ", "04"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("上半期", "05"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("下半期", "06"))
        WF_PERIODTYPE_DDL.Items.Add(New ListItem("年間", "07"))

        '○ RightBox情報設定
        rightview.MAPID = LNT0020WRKINC.MAPID
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)


        '期間種別出力条件初期化
        WF_PERIODTYPE_DDL.SelectedIndex = 0
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
            Case "00"     '任意期間
                '期間開始
                WF_PERIOD_FROM.Enabled = True
                '期間終了
                WF_PERIOD_TO.Enabled = True
            Case "01"     '１Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/06/30")
            Case "02"     '２Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/07/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/09/30")
            Case "03"     '３Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/10/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/12/31")
            Case "04"     '４Ｑ
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/01/01").AddYears(1)
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)
            Case "05"     '上半期
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/09/30")
            Case "06"     '下半期
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/10/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)
            Case "07"     '年間
                '期間開始
                WF_PERIOD_FROM.Text = CDate(WF_FISCALYEAR.Text & "/04/01")
                '期間終了
                WF_PERIOD_TO.Text = CDate(WF_FISCALYEAR.Text & "/03/31").AddYears(1)

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
    ''' 出力ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOUTPUT_Click()
        Try
            Dim type As String = ""
            Dim PRTID As String = ""
            Dim OfficeCode As String = ""
            Dim url As String = ""
            Dim printErrFlg_1 As Boolean = False
            Dim select_1_Flg As Boolean = False
            WF_PrintURL1.Value = ""

            Dim Fromdt As Date = DateTime.Parse(WF_PERIOD_FROM.Text)
            Dim Todt As Date = DateTime.Parse(WF_PERIOD_TO.Text)

            Dim Fromstr As String = Format(Fromdt, "yyyy年MM月dd日")
            Dim Tostr As String = Format(Todt, "yyyy年MM月dd日")

            '必須入力チェック
            Dim err As String = ""
            WW_FieldCheck(PRTID, err)
            If err = "ERR" Then
                Exit Sub
            End If

            '******************************
            '帳票表示データ取得処理
            '******************************
            select_1_Flg = True
            type = "1"
            Dim dt As DataTable = Me.OperatingIncomeDataGet(PRTID, type)
            'データ0件時
            If dt.Rows.Count = 0 Then
                printErrFlg_1 = True
            End If

            '******************************
            '帳票作成処理の実行
            '******************************
            If Not printErrFlg_1 Then
                Dim Report As New LNT0020_OperatIncomeReport_DIODOC(Master.MAPID, "勘定科目別・計上店別営業収入計上一覧表_TEMPLETE.xlsx", dt)
                Try
                    url = Report.CreateExcelPrintData(Fromstr, Tostr, WF_PERIODTYPE_DDL.SelectedValue, WF_FISCALYEAR.Text, WF_REPORTNAME.SelectedValue)
                    WF_PrintURL1.Value = url
                Catch ex As Exception
                    Throw
                End Try
            End If

            'データ0件時
            If printErrFlg_1 Then
                Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                Exit Sub
            End If

            '○ 別画面でExcelを表示
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_AccountingDownload();", True)

        Catch sqlex As MySqlException
            CS0011LOGWRITE.INFSUBCLASS = "LNT0020S " & SqlName          'SUBクラス名 + ストアド名称
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = sqlex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "LNT0020S"   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub
        End Try

        '○ 画面レイアウト設定
        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightview.GetViewId(Master.USERCAMP)
        End If

        Master.CheckParmissionCode(Master.USERCAMP)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            ' 画面遷移
            Master.TransitionPage()
        End If

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
            Case "WF_PERIOD_FROM"  '期間FROM
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    WF_PERIOD_FROM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                '○ フォーカス設定
                WF_PERIOD_FROM.Focus()

            Case "WF_PERIOD_TO"    '期間TO
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    WF_PERIOD_TO.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                '○ フォーカス設定
                WF_PERIOD_FROM.Focus()
        End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

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

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0020WRKINC.TITLEKBNS)

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
                        .ActiveCalendar()
                End Select
            End With

        End If

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 営業収入計上一覧表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function OperatingIncomeDataGet(ReportID As String, type As String) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                SQLcmd.CommandText = "lng.[PRT_OPERATING_INCOME]"
                SqlName = "PRT_OPERATING_INCOME"

                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piPRINTTYPE", MySqlDbType.VarChar, 2)        ' 帳票名（すべてorリースのみ）
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piPERIODTYPE", MySqlDbType.VarChar, 2)       ' 期間種別
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piPERIODFROM", MySqlDbType.Date)             ' 期間FROM
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piPERIODTO", MySqlDbType.Date)               ' 期間TO

                PARA1.Value = WF_REPORTNAME.SelectedValue
                PARA2.Value = WF_PERIODTYPE_DDL.SelectedValue
                PARA3.Value = WF_PERIOD_FROM.Text
                PARA4.Value = WF_PERIOD_TO.Text

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

End Class
