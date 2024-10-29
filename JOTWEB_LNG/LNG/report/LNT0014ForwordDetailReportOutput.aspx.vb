''************************************************************
' レンタルコンテナ回送費明細表画面
' 作成日 2023/01/11
' 更新日 
' 作成者 星
' 更新者 
'
' 修正履歴:2023/01/11 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' レンタルコンテナ回送費明細表画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0014ForwordDetailReportOutput
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIT0014Reporttbl As DataTable                   '帳票用テーブル

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
                    Case "mspToriSingleRowSelected"     '[共通]取引先選択ポップアップで行選択
                        RowSelected_mspToriSingle()
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
        Master.MAPID = LNT0014WRKINC.MAPID

        'TxtReportId.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

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

        ' 今月の1日を取得
        Dim Today As DateTime = DateTime.Today
        Dim FirstDay As New DateTime(Today.Year, Today.Month, 1)

        ' 初期変数設定処理
        TxtOrgCode.Text = Master.USER_ORG                                           '対象支店
        CODENAME_get("ORG", TxtOrgCode.Text, Me.LblOrgName.Text, WW_Dummy)          '対象支店名
        TxtPayee.Text = ""                                                          '支払先

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtPayee.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPID = LNT0014WRKINC.MAPID
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)

        '取引先
        Dim retToriList As DropDownList = CmnSearchSQL.getDdlTori()
        If retToriList.Items.Count > 0 Then
            Me.hdnSelectTori.Items.AddRange(retToriList.Items.Cast(Of ListItem).ToArray)
        End If

    End Sub

    ''' <summary>
    ''' ドロップダウンリスト初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub dropDownInitialize()
        '初期表示時のみリスト作成
        If Not IsPostBack Then
            '○年月ドロップダウンの生成
            '　年月ドロップダウンのクリア
            'Me.ddlYm.Items.Clear()
            ''　年月ドロップダウンの生成
            'Dim retYmList As DropDownList = Me.getCmbYm()
            'If retYmList.Items.Count > 0 Then
            '    Me.ddlYm.Items.AddRange(retYmList.Items.Cast(Of ListItem).ToArray)
            'End If

        End If

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

            '○ 入力文字置き換え(使用禁止文字排除)
            Master.EraseCharToIgnore(TxtOrgCode.Text)       '対象支店
            Master.EraseCharToIgnore(TxtPayee.Text)  '支払先

            '必須入力チェック
            Dim err As String = ""
            WW_Check(err)
            If err = "ERR" Then
                Exit Sub
            End If

            '******************************
            '帳票表示データ取得処理
            '******************************
            Dim dt As DataTable = Me.ForwardDetailDataGet(PRTID, type)
            'データ0件時
            If dt.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                Exit Sub
            End If

            '******************************
            '帳票作成処理の実行
            '******************************
            Dim Report As New LNT0014_ForwordDetailReport_DIODOC(Master.MAPID, "レンタルコンテナ回送費明細表_TEMPLATE.xlsx", dt)
            Dim url As String
            Try
                url = Report.CreateExcelPrintData()
            Catch ex As Exception
                Throw
            End Try

            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        Catch sqlex As MySqlException
            CS0011LOGWRITE.INFSUBCLASS = "LNT0014S " & SqlName          'SUBクラス名 + ストアド名称
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = sqlex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "LNT0014S"   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub
        End Try

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

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
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)
        O_RTN = ""
        WW_Dummy = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""

        ' 計上年月
        If TxtDownloadMonth.Text = "" Then
            Master.Output(C_MESSAGE_NO.CTN_INPUT_ERR, C_MESSAGE_TYPE.ERR, "計上年月", needsPopUp:=True)
            TxtDownloadMonth.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0014WRKINC.TITLEKBNS)

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
                        'Case "TxtStYMDCode"         '有効年月日(From)
                        '    .WF_Calendar.Text = Me.TxtStYMDCode.Text
                        'Case "TxtEndYMDCode"        '有効年月日(To)
                        '    .WF_Calendar.Text = Me.TxtEndYMDCode.Text

                    End Select
                    .ActiveCalendar()

                Else
                    Select Case WF_FIELD.Value
                        Case "TxtOrgCode"           '対象支店
                            WW_prmData = work.CreateUORGParam(Master.USERCAMP)
                        Case "TxtPayee"           '取引先
                            leftview.Visible = False
                            '検索画面
                            DisplayView_mspToriSingle(TxtPayee.Text)
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
            Case "TxtOrgCode"          '対象支店
                CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
                TxtOrgCode.Focus()
            Case "TxtPayee"     '支払先
                CODENAME_get("KEKKJM", TxtPayee.Text, LblPayee.Text, WW_Dummy)
                TxtPayee.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 取引先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspToriSingle()

        Dim selData = Me.mspToriSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtPayee.ID
                Me.TxtPayee.Text = selData("TORICODE").ToString
                Me.LblPayee.Text = selData("TORINAME").ToString & selData("DIVNAME").ToString
                Me.LblPayee.Focus()

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
    ''' 取引先検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspToriSingle(Optional ByVal prmKey As String = "")

        Me.mspToriSingle.InitPopUp()
        Me.mspToriSingle.SelectionMode = ListSelectionMode.Single
        Me.mspToriSingle.SQL = CmnSearchSQL.GetToriSQL

        Me.mspToriSingle.KeyFieldName = "KEYCODE"
        Me.mspToriSingle.DispFieldList.AddRange(CmnSearchSQL.GetToriTitle)

        '画面表示する絞り込みドロップダウンの設定(組織コード)
        Me.mspToriSingle.FilterField.Add("ORGNAMES", "提出部店")

        Me.mspToriSingle.ShowPopUpList(prmKey)

        '組織名取得
        Dim orgName = Master.USER_ORGNAME
        Me.mspToriSingle.ddlFilterInit("", orgName)

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
            Case "TxtOrgCode"           '対象支店
                TxtOrgCode.Text = WW_SelectValue
                LblOrgName.Text = WW_SelectText
                TxtOrgCode.Focus()
            Case "TxtPayee"      '支払先
                TxtPayee.Text = WW_SelectValue
                LblPayee.Text = WW_SelectText
                TxtOrgCode.Focus()
                'Case "TxtStYMDCode"         '有効年月日(From)
                '    Dim WW_DATE As Date
                '    Try
                '        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                '        If WW_DATE < C_DEFAULT_YMD Then
                '            Me.TxtStYMDCode.Text = ""
                '        Else
                '            Me.TxtStYMDCode.Text = WW_DATE.ToString("yyyy/MM/dd")
                '        End If

                '    Catch ex As Exception
                '    End Try
                '    Me.TxtStYMDCode.Focus()
                'Case "TxtEndYMDCode"        '有効年月日(To)
                '    Dim WW_DATE As Date
                '    Try
                '        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                '        If WW_DATE < C_DEFAULT_YMD Then
                '            Me.TxtEndYMDCode.Text = ""
                '        Else
                '            Me.TxtEndYMDCode.Text = WW_DATE.ToString("yyyy/MM/dd")
                '        End If

                '    Catch ex As Exception
                '    End Try
                '    Me.TxtEndYMDCode.Focus()
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
            Case "TxtOrgCode"          '対象支店
                TxtOrgCode.Focus()
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
                Case "KEKKJM"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateSiharaisakiParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' レンタルコンテナ回送費データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function ForwardDetailDataGet(ReportID As String, type As String) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                SQLcmd.CommandText = "lng.[PRT_FORWARD_DETAIL]"
                SqlName = "PRT_FORWARD_DETAIL"

                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piDATE", MySqlDbType.VarChar, 10)            ' 年月
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piOFFICECODE", MySqlDbType.VarChar, 6)       ' 支店
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piPAYMENTCODE", MySqlDbType.VarChar, 10)     ' 支払先

                PARA1.Value = TxtDownloadMonth.Text.Replace("/", "")
                If Me.TxtOrgCode.Text = "011312" OrElse Me.TxtOrgCode.Text = "011308" OrElse Me.TxtOrgCode.Text = "" Then
                    PARA2.Value = DBNull.Value
                Else
                    PARA2.Value = Me.TxtOrgCode.Text
                End If
                If Me.TxtPayee.Text = "" Then
                    PARA3.Value = DBNull.Value
                Else
                    PARA3.Value = Me.TxtPayee.Text
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

End Class
