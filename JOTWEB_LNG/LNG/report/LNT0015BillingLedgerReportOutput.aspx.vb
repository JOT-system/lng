''************************************************************
' 請求台帳画面
' 作成日 2023/01/20
' 更新日 
' 作成者 星
' 更新者 
'
' 修正履歴:2023/01/20 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 請求台帳画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0015BillingLedgerReportOutput
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIT0015Reporttbl As DataTable                   '帳票用テーブル

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
                    Case WF_ORGCODE_ALL.ID,          '部店変更時処理
                     WF_ORGCODE.ID
                        WF_ORGCODE_Change()
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
        Master.MAPID = LNT0015WRKINC.MAPID

        'TxtReportId.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

        '請求書種類の初期化
        Me.InitInvtype()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ' メニューからの画面遷移
        ' 画面間の情報クリア
        work.Initialize()

        'チェックボックスをチェック状態とする
        Me.CHKALL.Checked = True

        '○ RightBox情報設定
        rightview.MAPID = LNT0015WRKINC.MAPID
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)

        Dim keijoym As String = getKeijoYM() & "01"
        Me.txtDownloadMonth.Text = Format(DateTime.ParseExact(keijoym, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None), "yyyy/MM")

        '部店選択表示初期設定
        WF_ORGCODE_Init()

    End Sub

    ''' <summary>
    ''' 部店選択表示初期設定
    ''' </summary>
    Protected Sub WF_ORGCODE_Init()

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

        '全支店を初期選択
        If Master.USER_ORG <> "011308" Then
            WF_ORGCODE_ALL.UnSelectAll()
            WF_ORGCODE.SelectSingleItem(Master.USER_ORG)
        Else
            WF_ORGCODE_ALL.SelectAll()
            WF_ORGCODE.UnSelectAll()
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
    ''' 請求書種類初期化
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitInvtype()

        WF_INVTYPE.SelectionMode = ListSelectionMode.Multiple
        WF_INVTYPE.NeedsPostbackAfterSelect = False

        Dim dt As DataTable = CmnLNG.GetFixValueTbl(Master.USERCAMP, "INVOICETYPE")
        WF_INVTYPE.SetTileValues(dt)

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
            Dim printErrFlg_2 As Boolean = False
            Dim printErrFlg_3 As Boolean = False
            Dim printErrFlg_4 As Boolean = False
            Dim select_1_Flg As Boolean = False
            Dim select_2_Flg As Boolean = False
            Dim select_3_Flg As Boolean = False
            Dim select_4_Flg As Boolean = False
            Dim chkBoxFlg As String = ""
            Dim dblTaxRate As Double = 0
            Dim SelectSqlName As String = ""
            WF_PrintURL1.Value = ""
            WF_PrintURL2.Value = ""
            WF_PrintURL3.Value = ""
            WF_PrintURL4.Value = ""

            '必須入力チェック
            Dim err As String = ""
            WW_FieldCheck(PRTID, err)
            If err = "ERR" Then
                Exit Sub
            End If

            '請求書種類取得
            Me.Save_SearchItem()

            '〇消費税取得処理
            Dim htZerit As Hashtable = GetZerit(txtDownloadMonth.Text.Replace("/", "") + "01")
            '税率
            dblTaxRate = CInt(htZerit("SETVAL1").ToString) / 100

            If Me.CHKALL.Checked = True Then
                chkBoxFlg = "1"
            Else
                chkBoxFlg = "0"
            End If

            '選択していなかった場合両方取得
            If work.WF_SRC_CONTRALNMODE.Text = "" Then
                work.WF_SRC_CONTRALNMODE.Text = "1,2,4,5"
            End If

            '請求書種類判定　：使用料
            If work.WF_SRC_CONTRALNMODE.Text.Contains("1") Then

                '******************************
                '帳票表示データ取得処理
                '******************************
                SelectSqlName = "lng.[PRT_RENTALBILLING_LEDGER]"
                SqlName = "PRT_RENTALBILLING_LEDGER"
                select_1_Flg = True
                type = "1"
                Dim dt As DataTable = Me.BillingLedgerDataGet(PRTID, type, chkBoxFlg, SelectSqlName)
                'データ0件時
                If dt.Rows.Count = 0 Then
                    printErrFlg_1 = True
                End If

                '******************************
                '帳票作成処理の実行
                '******************************
                If Not printErrFlg_1 Then
                    Dim Report As New LNT0015_BillingLedgerReport_DIODOC(Master.MAPID, "請求台帳_TEMPLATE.xlsx", dt, type, dblTaxRate)
                    Try
                        url = Report.CreateExcelPrintData(MAXPAGE_GET(dt), "0")
                        WF_PrintURL1.Value = url
                    Catch ex As Exception
                        Throw
                    End Try
                End If
            End If

            '請求書種類判定　：リース
            If work.WF_SRC_CONTRALNMODE.Text.Contains("2") Then

                '******************************
                '帳票表示データ取得処理
                '******************************
                SelectSqlName = "lng.[PRT_LEASEBILLING_LEDGER]"
                SqlName = "PRT_LEASEBILLING_LEDGER"
                select_2_Flg = True
                type = "2"
                Dim dt As DataTable = Me.BillingLedgerDataGet(PRTID, type, chkBoxFlg, SelectSqlName)
                'データ0件時
                If dt.Rows.Count = 0 Then
                    printErrFlg_2 = True
                End If

                '******************************
                '帳票作成処理の実行
                '******************************
                If Not printErrFlg_2 Then
                    Dim Report As New LNT0015_BillingLedgerReport_DIODOC(Master.MAPID, "請求台帳_TEMPLATE.xlsx", dt, type, dblTaxRate)
                    Try
                        url = Report.CreateExcelPrintData(MAXPAGE_GET(dt), "0")
                        WF_PrintURL2.Value = url
                    Catch ex As Exception
                        Throw
                    End Try
                End If
            End If

            '請求書種類判定　：手書き
            If work.WF_SRC_CONTRALNMODE.Text.Contains("4") Then

                '******************************
                '帳票表示データ取得処理
                '******************************
                SelectSqlName = "lng.[PRT_WRITEBILLING_LEDGER]"
                SqlName = "PRT_WRITEBILLING_LEDGER"
                select_3_Flg = True
                type = "3"
                Dim dt As DataTable = Me.BillingLedgerDataGet(PRTID, type, chkBoxFlg, SelectSqlName)
                'データ0件時
                If dt.Rows.Count = 0 Then
                    printErrFlg_3 = True
                End If

                '******************************
                '帳票作成処理の実行
                '******************************
                If Not printErrFlg_3 Then
                    Dim Report As New LNT0015_BillingLedgerReport_DIODOC(Master.MAPID, "請求台帳_TEMPLATE.xlsx", dt, type, dblTaxRate)
                    Try
                        url = Report.CreateExcelPrintData(MAXPAGE_GET(dt), "0")
                        WF_PrintURL3.Value = url
                    Catch ex As Exception
                        Throw
                    End Try
                End If
            End If

            '請求書種類判定　：売却コンテナ
            If work.WF_SRC_CONTRALNMODE.Text.Contains("5") Then

                '******************************
                '帳票表示データ取得処理
                '******************************
                SelectSqlName = "lng.[PRT_CTNSALEBILLING_LEDGER]"
                SqlName = "PRT_CTNSALEBILLING_LEDGER"
                select_4_Flg = True
                type = "4"
                Dim dt As DataTable = Me.BillingLedgerDataGet(PRTID, type, chkBoxFlg, SelectSqlName)
                'データ0件時
                If dt.Rows.Count = 0 Then
                    printErrFlg_4 = True
                End If

                '******************************
                '帳票作成処理の実行
                '******************************
                If Not printErrFlg_4 Then
                    Dim Report As New LNT0015_BillingLedgerReport_DIODOC(Master.MAPID, "請求台帳(売却用)_TEMPLATE.xlsx", dt, type, dblTaxRate)
                    Try
                        url = Report.CreateExcelPrintData(MAXPAGE_GET(dt), "1")
                        WF_PrintURL4.Value = url
                    Catch ex As Exception
                        Throw
                    End Try
                End If
            End If

            '両方選択していたかチェック
            If select_1_Flg AndAlso select_2_Flg AndAlso select_3_Flg AndAlso select_4_Flg Then
                '両方データ0件時
                If printErrFlg_1 AndAlso printErrFlg_2 AndAlso printErrFlg_3 AndAlso printErrFlg_4 Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                    Exit Sub
                End If
            Else
                '片方選択でデータ0件時
                If printErrFlg_1 OrElse printErrFlg_2 OrElse printErrFlg_3 OrElse printErrFlg_4 Then
                    Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                    Exit Sub
                End If
            End If

            '○ 別画面でExcelを表示
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_AccountingDownload();", True)

        Catch sqlex As MySqlException
            CS0011LOGWRITE.INFSUBCLASS = "LNT0015S " & SqlName          'SUBクラス名 + ストアド名称
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = sqlex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "LNT0015S"   'SUBクラス名
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

        '部店選択がない場合
        If Not WF_ORGCODE_ALL.HasSelectedValue() AndAlso
            Not WF_ORGCODE.HasSelectedValue() Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "請求提出部店", needsPopUp:=True)
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0015WRKINC.TITLEKBNS)

    End Sub

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
    ''' 頁数分母取得
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <remarks></remarks>
    Private Function MAXPAGE_GET(dt As DataTable)
        '請求提出部店ごとの件数を取得
        Dim OrgRows As Hashtable = New Hashtable
        Dim intCnt As Integer = 0
        Dim OrgCdchk As String = ""
        Dim firstflg As String = True

        For Each OutputRowData As DataRow In dt.Rows
            Dim strOrgCd As String = ""

            strOrgCd = OutputRowData("INVOICEORGCODE").ToString

            '請求提出部店が変わっていなかったら件数をカウントする
            If strOrgCd.Equals(OrgCdchk) OrElse firstflg Then
                intCnt += 1
                firstflg = False
            Else
                '請求提出部店が切り替わった時10で割り、少数第一位切り上げ（頁数分母取得）
                intCnt = Math.Ceiling(intCnt / 10)
                '保存する
                OrgRows(OrgCdchk) = intCnt
                intCnt = 1
            End If
            OrgCdchk = strOrgCd
        Next

        '最後のシートの頁数分母取得
        intCnt = Math.Ceiling(intCnt / 10)
        '保存する
        OrgRows(OrgCdchk) = intCnt

        Return OrgRows

    End Function

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 請求台帳データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function BillingLedgerDataGet(ReportID As String, type As String, chkBoxFlg As String, SelectSqlName As String) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                If chkBoxFlg = "1" Then
                    SQLcmd.CommandText = SelectSqlName
                Else
                    If type = "4" Then
                        SQLcmd.CommandText = "lng.[PRT_CTNSALEBILLING_CONFIRM_LEDGER]"
                        SqlName = "PRT_CTNSALEBILLING_CONFIRM_LEDGER"
                    Else
                        SQLcmd.CommandText = "lng.[PRT_BILLING_LEDGER]"
                        SqlName = "PRT_BILLING_LEDGER"
                    End If
                End If

                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.VarChar, 6)               ' 年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piINVERGCODE", MySqlDbType.VarChar, 400)       ' 請求提出部店
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piINVOICETYPE", MySqlDbType.VarChar, 1)        ' 請求書種類

                PARA1.Value = Format(DateTime.Parse(txtDownloadMonth.Text), "yyyyMM")
                If WF_ORGCODE_ALL.HasSelectedValue() Then
                    PARA2.Value = GetOfficeCode()
                ElseIf WF_ORGCODE.HasSelectedValue() Then
                    PARA2.Value = ""
                    For Each item As ListItem In WF_ORGCODE.GetSelectedListData.Items
                        PARA2.Value &= If(PARA2.Value.Length > 0, "," & item.Value, item.Value)
                    Next
                End If
                If type = "1" Then
                    PARA3.Value = 2
                ElseIf type = "2" Then
                    PARA3.Value = 3
                ElseIf type = "3" Then
                    PARA3.Value = 4
                ElseIf type = "4" Then
                    PARA3.Value = 5
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
    ''' 年月選択用コンボボックス作成
    ''' </summary>
    ''' <returns></returns>
    Public Function getCmbYm() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("  VALUE1 as CODE")
        sqlStat.AppendLine("FROM COM.LNS0006_FIXVALUE with(nolock)")
        sqlStat.AppendLine("WHERE CLASS = 'SEIKYUKEIJOYMFROM'")
        sqlStat.AppendLine("  And KEYCODE = '1'")
        sqlStat.AppendLine("  And CURDATE() BETWEEN STYMD AND ENDYMD ")
        sqlStat.AppendLine("  And DELFLG = @DELFLG")

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

                    'SELECT値を取得
                    Dim wkKeijoYm As String = ""
                    While sqlDr.Read
                        wkKeijoYm = Convert.ToString(sqlDr("CODE"))
                    End While

                    '現在年月を取得
                    Dim nengetsu As String = DateTime.Now.ToString("yyyyMM")
                    Dim nengetsuDisp As String = DateTime.Now.ToString("yyyy/MM")
                    '
                    'SELECT値の年月まで日付を遡っていく
                    Do Until nengetsu < wkKeijoYm
                        '表示値は"/"編集ありのものを、コードは"/"編集なしのものを設定
                        'Dim listItm As New ListItem(Convert.ToString(sqlDr("nengetsuDisp")), Convert.ToString(sqlDr("nengetsu")))
                        Dim listItm As New ListItem(nengetsuDisp, nengetsu)
                        retList.Items.Add(listItm)

                        '年月に-1月をしていく
                        nengetsuDisp = Date.Parse(nengetsuDisp + "/01").AddMonths(-1).ToString("yyyy/MM")
                        nengetsu = nengetsuDisp.Replace("/", "")
                    Loop

                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = New StackFrame(0, False).GetMethod.DeclaringType.FullName  ' クラス名
            CS0011LOGWRITE.INFPOSI = Reflection.MethodBase.GetCurrentMethod.Name                    ' メソッド名
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             ' ログ出力
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' 税率取得処理
    ''' </summary>
    ''' <param name="strPrmPlanDepYMD">発送年月日</param>
    Private Function GetZerit(ByVal strPrmPlanDepYMD As String) As Hashtable

        Dim htZerit As New Hashtable
        Dim param As New Dictionary(Of String, String)

        'パラメータ作成
        param.Add("@piSHIPYMD", strPrmPlanDepYMD)   '発送年月日
        param.Add("@piVBFLG", CONST_VBFLG)          'VBから呼ばれたかのフラグ
        '戻り値 VBでは未使用
        param.Add("@poMSTFLG", "")
        param.Add("@poRetSetVal1", "")
        param.Add("@poRetSetVal2", "")
        param.Add("@poRetSetVal3", "")
        param.Add("@poNextFlg", "")
        param.Add("@poDispNextFlg", "")
        param.Add("@poRetSetVal2Nm", "")
        param.Add("@poRetSetVal3Nm", "")

        Try
            Using SQLcon As MySqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続

                Using tran = SQLcon.BeginTransaction

                    Dim dtTinr As DataTable = Nothing
                    '税率取得処理 ストアド実行
                    CS0050Session.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_GET_ZERIT, param, dtTinr, tran)
                    '戻り値設定
                    If dtTinr.Rows.Count > 0 Then
                        htZerit("MSTFLG") = dtTinr.Rows(0)("MSTFLG").ToString
                        htZerit("SETVAL1") = dtTinr.Rows(0)("SETVAL1").ToString
                        htZerit("SETVAL2") = dtTinr.Rows(0)("SETVAL2").ToString
                        htZerit("SETVAL3") = dtTinr.Rows(0)("SETVAL3").ToString
                        htZerit("NEXTFLGCODE") = dtTinr.Rows(0)("NEXTFLGCODE").ToString
                        htZerit("NEXTFLG") = dtTinr.Rows(0)("NEXTFLG").ToString
                        htZerit("SETVAL2NM") = dtTinr.Rows(0)("SETVAL2NM").ToString
                        htZerit("SETVAL3NM") = dtTinr.Rows(0)("SETVAL3NM").ToString
                    Else
                        htZerit("MSTFLG") = ""
                        htZerit("SETVAL1") = ""
                        htZerit("SETVAL2") = ""
                        htZerit("SETVAL3") = ""
                        htZerit("NEXTFLGCODE") = ""
                        htZerit("NEXTFLG") = ""
                        htZerit("SETVAL2NM") = ""
                        htZerit("SETVAL3NM") = ""
                    End If

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ERR, "税率取得エラー", needsPopUp:=True)

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNT0013L GetZerit"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        '返却
        Return htZerit

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
            SQLBldr.AppendLine(" AND CLASS01 IN (1,2,4)")
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
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015L GetOfficeData")

                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:LNT0015L GetOfficeData"
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
            SQLBldr.AppendLine(" AND CLASS01 IN (1,4)")
            SQLBldr.AppendLine(" AND @1 BETWEEN STYMD AND ENDYMD")

            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@1", MySqlDbType.Date)     ' 対象年月

                    '対象年月
                    If Me.txtDownloadMonth.Text <> "" Then
                        PARA1.Value = CDate(txtDownloadMonth.Text & "/01")
                    Else
                        PARA1.Value = DBNull.Value
                    End If

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
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015L GetOfficeCode")

                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:LNT0015L GetOfficeCode"
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
    ''' 計上年月初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Function getKeijoYM() As String
        Dim keijoYm As String = Format(Now, "yyyyMM")
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT TOP 1")
        sqlStat.AppendLine("       KEIJOYM AS KEIJOYM")
        sqlStat.AppendLine("  FROM LNG.LNT0081_ACCOUNT_CLOSE with(nolock)")
        sqlStat.AppendLine(" WHERE CLOSETYPE = '1'")
        sqlStat.AppendLine("   And CLOSESTATUS = '0'")
        sqlStat.AppendLine("   And DELFLG = @DELFLG")
        sqlStat.AppendLine(" ORDER BY KEIJOYM DESC")

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
                        Return keijoYm
                    End If
                    While sqlDr.Read
                        keijoYm = sqlDr("KEIJOYM").ToString
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_READDATA_ERR, C_MESSAGE_TYPE.ABORT, "LNT0015L getKeijoYM", needsPopUp:=True)

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNT0015L getKeijoYM"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.CTN_READDATA_ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
        End Try

        Return keijoYm

    End Function

End Class
