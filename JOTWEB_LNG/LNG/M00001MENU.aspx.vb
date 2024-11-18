Option Strict On
Imports JOTWEB_LNG
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox
''' <summary>
''' メニュー画面クラス
''' </summary>
Public Class M00001MENU
    Inherits System.Web.UI.Page
    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    Public Property SelectedGuidanceNo As String = ""
    ''' <summary>
    '''  パスワードの変更依頼（期限切れまで何日前からか）
    ''' </summary>
    Private Const C_PASSWORD_CHANGE_LIMIT_COUNT As Integer = 31
    Private Const C_VSNAME_LEFTNAVIDATA_1 As String = "VS_MENU_LEFT_NAVI_1"
    Private Const C_VSNAME_LEFTNAVIDATA_2 As String = "VS_MENU_LEFT_NAVI_2"
    Private Const C_VSNAME_LEFTNAVIDATA_3 As String = "VS_MENU_LEFT_NAVI_3"
    Private Const C_VSNAME_LEFTNAVIDATA_4 As String = "VS_MENU_LEFT_NAVI_4"
    Private Const C_VSNAME_LEFTNAVIDATA_5 As String = "VS_MENU_LEFT_NAVI_5"
    Private Const C_VSNAME_LEFTNAVIDATA_6 As String = "VS_MENU_LEFT_NAVI_6"
    Private Const C_VSNAME_LEFTNAVIDATA_7 As String = "VS_MENU_LEFT_NAVI_7"
    Private Const C_VSNAME_LEFTNAVIDATA_8 As String = "VS_MENU_LEFT_NAVI_8"
    Private Const C_VSNAME_LEFTNAVIDATA_9 As String = "VS_MENU_LEFT_NAVI_9"
    Private Const C_VSNAME_LEFTNAVIDATA_10 As String = "VS_MENU_LEFT_NAVI_10"
    Private Const C_VSNAME_LEFTNAVIDATA_11 As String = "VS_MENU_LEFT_NAVI_11"
    Private Const C_VSNAME_LEFTNAVIDATA_12 As String = "VS_MENU_LEFT_NAVI_12"
    Private Const C_VSNAME_LEFTNAVIDATA_13 As String = "VS_MENU_LEFT_NAVI_13"
    Private Const C_VSNAME_LEFTNAVIDATA_14 As String = "VS_MENU_LEFT_NAVI_14"
    Private Const C_VSNAME_LEFTNAVIDATA_15 As String = "VS_MENU_LEFT_NAVI_15"

    ''' <summary>
    ''' ページロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '左ナビの開閉状態をcookieに記憶（Initializeで復元します）
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav1, "chkTopItem1")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav2, "chkTopItem2")
            Me.LeftNavColleLNToSaveCookie(Me.repLeftNav3, "chkTopItem3")
            Me.LeftNavColleLNToSaveCookie(Me.repLeftNav4, "chkTopItem4")
            Me.LeftNavColleLNToSaveCookie(Me.repLeftNav5, "chkTopItem5")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav6, "chkTopItem6")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav7, "chkTopItem7")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav8, "chkTopItem8")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav9, "chkTopItem9")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav10, "chkTopItem10")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav11, "chkTopItem11")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav12, "chkTopItem12")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav13, "chkTopItem13")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav14, "chkTopItem14")
            'Me.LeftNavColleLNToSaveCookie(Me.repLeftNav15, "chkTopItem15")

            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                If WF_ButtonClick.Value.StartsWith("WF_ButtonShowGuidance") Then
                    WF_ButtonShowGuidance_Click()
                    Return
                End If
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonLeftNavi1"
                        BtnLeftNavi_Click(1)
                    Case "WF_ButtonLeftNavi2"
                        BtnLeftNavi_Click(2)
                    Case "WF_ButtonLeftNavi3"
                        BtnLeftNavi_Click(3)
                    Case "WF_ButtonLeftNavi4"
                        BtnLeftNavi_Click(4)
                    Case "WF_ButtonLeftNavi5"
                        BtnLeftNavi_Click(5)
                    Case "WF_ButtonLeftNavi6"
                        BtnLeftNavi_Click(6)
                    Case "WF_ButtonLeftNavi7"
                        BtnLeftNavi_Click(7)
                    Case "WF_ButtonLeftNavi8"
                        BtnLeftNavi_Click(8)
                    Case "WF_ButtonLeftNavi9"
                        BtnLeftNavi_Click(9)
                    Case "WF_ButtonLeftNavi10"
                        BtnLeftNavi_Click(10)
                    Case "WF_ButtonLeftNavi11"
                        BtnLeftNavi_Click(11)
                    Case "WF_ButtonLeftNavi12"
                        BtnLeftNavi_Click(12)
                    Case "WF_ButtonLeftNavi13"
                        BtnLeftNavi_Click(13)
                    Case "WF_ButtonLeftNavi14"
                        BtnLeftNavi_Click(14)
                    Case "WF_ButtonLeftNavi15"
                        BtnLeftNavi_Click(15)
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        FIELD_Change()
                End Select
            End If
        Else
            '★★★ 初期画面表示 ★★★
            Initialize()
            WF_ButtonClick.Value = ""
            WF_ApprovalId.Value = Master.ROLE_APPROVALID
        End If

    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        Master.MAPID = GRM00001WRKINC.MAPID
        Dim menuButtonList1 As List(Of MenuItem) = Nothing
        Dim menuButtonList2 As List(Of MenuItem) = Nothing

        Dim menuButtonList3 As List(Of MenuItem) = Nothing      '受注管理(レンタル)
        Dim menuButtonList4 As List(Of MenuItem) = Nothing      '帳票(レンタル)
        Dim menuButtonList5 As List(Of MenuItem) = Nothing      'マスタ管理(レンタル)

        Dim menuButtonList6 As List(Of MenuItem) = Nothing      '受注管理(リース)
        Dim menuButtonList7 As List(Of MenuItem) = Nothing      '帳票(リース)
        Dim menuButtonList8 As List(Of MenuItem) = Nothing      'マスタ管理(リース)

        Dim menuButtonList9 As List(Of MenuItem) = Nothing      '受注管理(収入管理)
        Dim menuButtonList10 As List(Of MenuItem) = Nothing     '帳票(収入管理)
        Dim menuButtonList11 As List(Of MenuItem) = Nothing     'マスタ管理(収入管理)

        Dim menuButtonList12 As List(Of MenuItem) = Nothing     'マスタ管理①(マスタ管理)
        Dim menuButtonList13 As List(Of MenuItem) = Nothing     'マスタ管理②(マスタ管理)

        Dim menuButtonList14 As List(Of MenuItem) = Nothing     'コンテナ販売

        Dim menuButtonList15 As List(Of MenuItem) = Nothing     '受注管理(リース)(支店利用)

        Using sqlCon As MySqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            'メニューボタン情報の取得
            Try
                'menuButtonList1 = GetMenuItemList(sqlCon, "1")
                'menuButtonList2 = GetMenuItemList(sqlCon, "2")
                menuButtonList3 = GetMenuItemList(sqlCon, "3")
                menuButtonList4 = GetMenuItemList(sqlCon, "4")
                menuButtonList5 = GetMenuItemList(sqlCon, "5")
                'menuButtonList6 = GetMenuItemList(sqlCon, "6")
                'menuButtonList7 = GetMenuItemList(sqlCon, "7")
                'menuButtonList8 = GetMenuItemList(sqlCon, "8")
                'menuButtonList9 = GetMenuItemList(sqlCon, "9")
                'menuButtonList10 = GetMenuItemList(sqlCon, "A")
                'menuButtonList11 = GetMenuItemList(sqlCon, "B")
                'menuButtonList12 = GetMenuItemList(sqlCon, "C")
                'menuButtonList13 = GetMenuItemList(sqlCon, "D")
                'menuButtonList14 = GetMenuItemList(sqlCon, "E")
                'menuButtonList15 = GetMenuItemList(sqlCon, "F")

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0008_UPROFMAP SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                Return
            End Try
            'Me.BB.InnerHtml = "<h1>動的なヘッダー</h1>"

            '取得したデータを画面に展開
            'ViewState(C_VSNAME_LEFTNAVIDATA_1) = menuButtonList1
            'Me.repLeftNav1.DataSource = menuButtonList1
            'Me.repLeftNav1.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_2) = menuButtonList2
            'Me.repLeftNav2.DataSource = menuButtonList2
            'Me.repLeftNav2.DataBind()
            ViewState(C_VSNAME_LEFTNAVIDATA_3) = menuButtonList3
            Me.repLeftNav3.DataSource = menuButtonList3
            Me.repLeftNav3.DataBind()
            ViewState(C_VSNAME_LEFTNAVIDATA_4) = menuButtonList4
            Me.repLeftNav4.DataSource = menuButtonList4
            Me.repLeftNav4.DataBind()
            ViewState(C_VSNAME_LEFTNAVIDATA_5) = menuButtonList5
            Me.repLeftNav5.DataSource = menuButtonList5
            Me.repLeftNav5.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_6) = menuButtonList6
            'Me.repLeftNav6.DataSource = menuButtonList6
            'Me.repLeftNav6.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_7) = menuButtonList7
            'Me.repLeftNav7.DataSource = menuButtonList7
            'Me.repLeftNav7.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_8) = menuButtonList8
            'Me.repLeftNav8.DataSource = menuButtonList8
            'Me.repLeftNav8.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_9) = menuButtonList9
            'Me.repLeftNav9.DataSource = menuButtonList9
            'Me.repLeftNav9.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_10) = menuButtonList10
            'Me.repLeftNav10.DataSource = menuButtonList10
            'Me.repLeftNav10.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_11) = menuButtonList11
            'Me.repLeftNav11.DataSource = menuButtonList11
            'Me.repLeftNav11.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_12) = menuButtonList12
            'Me.repLeftNav12.DataSource = menuButtonList12
            'Me.repLeftNav12.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_13) = menuButtonList13
            'Me.repLeftNav13.DataSource = menuButtonList13
            'Me.repLeftNav13.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_14) = menuButtonList14
            'Me.repLeftNav14.DataSource = menuButtonList14
            'Me.repLeftNav14.DataBind()
            'ViewState(C_VSNAME_LEFTNAVIDATA_15) = menuButtonList15
            'Me.repLeftNav15.DataSource = menuButtonList15
            'Me.repLeftNav15.DataBind()

            'ガイダンスマスタの表示
            'ガイダンスデータ取得
            Try
                Dim guidanceDt As DataTable = GetGuidanceData(sqlCon)
                Me.repGuidance.DataSource = guidanceDt
                Me.repGuidance.DataBind()
                If guidanceDt.Rows.Count = 0 Then
                    guidanceArea.Visible = False
                End If

                'ガイダンス(右)
                Using splTran = sqlCon.BeginTransaction
                    'SQL実行
                    Dim guidDt As DataTable = CmnGuidanceData.GetGuidanceData(sqlCon, splTran, Master.USERID)
                    Me.repGuidance1.DataSource = guidDt
                    Me.repGuidance1.DataBind()
                    If guidDt.Rows.Count = 0 Then
                        guidanceBoxWrapper.Visible = False
                    End If
                End Using
            Catch ex As Exception
            End Try

        End Using
    End Sub
    ''' <summary>
    ''' 左ナビゲーションボタン押下時処理
    ''' </summary>
    Protected Sub BtnLeftNavi_Click(intID As Integer)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap
        Dim strIDNAME As String = ""
        Select Case intID
            Case 1
                strIDNAME = C_VSNAME_LEFTNAVIDATA_1
            Case 2
                strIDNAME = C_VSNAME_LEFTNAVIDATA_2
            Case 3
                strIDNAME = C_VSNAME_LEFTNAVIDATA_3
            Case 4
                strIDNAME = C_VSNAME_LEFTNAVIDATA_4
            Case 5
                strIDNAME = C_VSNAME_LEFTNAVIDATA_5
            Case 6
                strIDNAME = C_VSNAME_LEFTNAVIDATA_6
            Case 7
                strIDNAME = C_VSNAME_LEFTNAVIDATA_7
            Case 8
                strIDNAME = C_VSNAME_LEFTNAVIDATA_8
            Case 9
                strIDNAME = C_VSNAME_LEFTNAVIDATA_9
            Case 10
                strIDNAME = C_VSNAME_LEFTNAVIDATA_10
            Case 11
                strIDNAME = C_VSNAME_LEFTNAVIDATA_11
            Case 12
                strIDNAME = C_VSNAME_LEFTNAVIDATA_12
            Case 13
                strIDNAME = C_VSNAME_LEFTNAVIDATA_13
            Case 14
                strIDNAME = C_VSNAME_LEFTNAVIDATA_14
            Case 15
                strIDNAME = C_VSNAME_LEFTNAVIDATA_15
        End Select
        Dim leftNaviList = DirectCast(ViewState(strIDNAME), List(Of MenuItem))
        'ありえないがメニュー表示リストが存在しない場合はそのまま終了
        If leftNaviList Is Nothing OrElse
           IsNumeric(Me.hdnPosiCol.Value) = False OrElse
           IsNumeric(Me.hdnRowLine.Value) = False Then
            Return
        End If
        Dim posiRow As Integer = CInt(Me.hdnRowLine.Value)
        Dim posiCol As Integer = CInt(Me.hdnPosiCol.Value)
        Dim rowLine As Integer = CInt(Me.hdnRowLine.Value)
        Me.hdnPosiCol.Value = ""
        Me.hdnRowLine.Value = ""
        Dim menuItm As MenuItem = Nothing
        Dim qMenuItm = From itm In leftNaviList Where itm.PosiCol = posiCol
        If rowLine = 1 Then
            menuItm = qMenuItm.FirstOrDefault
        Else
            If qMenuItm.Any Then
                menuItm = (From itm In qMenuItm(0).ChildMenuItem Where itm.RowLine = rowLine).FirstOrDefault
            End If
        End If
        'ありえないが選択したメニューアイテムが存在しない場合はそのまま終了
        If menuItm Is Nothing Then
            Return
        End If

        If menuItm.Reportflg = "1" Then
            '    '★★★ ボタン押下時、帳票出力 ★★★
            '    Dim WW_DATE As Date = Date.Now.AddDays(-1)
            '    Dim WW_WORKINGDATE As Date = Nothing
            '    Dim daycount As Integer = 0
            '    '前稼働日取得
            '    Using sqlCon As MySqlConnection = CS0050Session.getConnection
            '        Try
            '            sqlCon.Open()
            '            WW_WORKINGDATE = GetWorkingDate(sqlCon, WW_DATE)

            '        Catch ex As Exception
            '            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "前稼働日の取得に失敗しました。", needsPopUp:=True)

            '            CS0011LOGWRITE.INFSUBCLASS = "Main"
            '            CS0011LOGWRITE.INFPOSI = "LNS0021_CALENDAR SELECT"
            '            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            '            CS0011LOGWRITE.TEXT = ex.ToString()
            '            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            '            CS0011LOGWRITE.CS0011LOGWrite()
            '            Return
            '        End Try
            '    End Using

            '    daycount = (WW_DATE.Date - WW_WORKINGDATE.Date).Days

            '    Dim PRT0000ReportCall As New PRT0000ReportCall
            '    Dim PrintCount As Integer = 0
            '    ClearURLValue()
            '    Dim FirstFLG As String = "0"
            '    Dim LastFLG As String = "0"
            '    Dim ALLDT As DataTable = Nothing
            '    Dim ALLDT2 As DataTable = Nothing

            '    For i As Integer = 0 To daycount
            '        If i = daycount Then
            '            If daycount = 0 Then
            '                FirstFLG = "1"
            '                LastFLG = "1"
            '            Else
            '                FirstFLG = "0"
            '                LastFLG = "1"
            '            End If
            '        ElseIf i = 0 Then
            '            FirstFLG = "1"
            '            LastFLG = "0"
            '        Else
            '            FirstFLG = "0"
            '            LastFLG = "0"
            '        End If
            '        PRT0000ReportCall.REPORTID = menuItm.Reportid
            '        PRT0000ReportCall.TARGETDATE = WW_WORKINGDATE
            '        PRT0000ReportCall.CAMPCODE = Master.USERCAMP
            '        PRT0000ReportCall.BRANCHCODE = Master.USER_AFFILIATION
            '        PRT0000ReportCall.USERID = Master.USERID
            '        PRT0000ReportCall.TERMID = Master.USERTERMID
            '        PRT0000ReportCall.ALLDT = ALLDT
            '        PRT0000ReportCall.ALLDT2 = ALLDT2
            '        PRT0000ReportCall.ReportCall(LastFLG, FirstFLG, ALLDT, ALLDT2)
            '        ALLDT = PRT0000ReportCall.ALLDT
            '        ALLDT2 = PRT0000ReportCall.ALLDT2
            '        If isNormal(PRT0000ReportCall.ERR) Then
            '            PrintCount += 1
            '            SetURLValue(PRT0000ReportCall.URL1, PrintCount)
            '            If PRT0000ReportCall.URL2 <> "" Then
            '                PrintCount += 1
            '                SetURLValue(PRT0000ReportCall.URL2, PrintCount)
            '            End If
            '        End If
            '        If menuItm.Reportid = "PRT0006" Then
            '            Exit For
            '        End If
            '        WW_WORKINGDATE = WW_WORKINGDATE.AddDays(1)
            '    Next
            '    If PrintCount > 0 Then
            '        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelDownload();", True)
            '    Else
            '        Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
            '    End If


        Else
            '★★★ ボタン押下時、画面遷移（左） ★★★

            '○画面遷移権限チェック（左）
            CS0007CheckAuthority.MAPID = menuItm.MapId
            CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
            CS0007CheckAuthority.check()
            If Not isNormal(CS0007CheckAuthority.ERR) Then
                Master.Output(CS0007CheckAuthority.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & menuItm.MapId)
                Master.ShowMessage()

                Exit Sub
            End If
            'セッション変数クリア
            Dim eraseSessionNames As New List(Of String) From {"Selected_STYMD", "Selected_ENDYMD",
                "Selected_USERIDFrom", "Selected_USERIDTo", "Selected_USERIDG1", "Selected_USERIDG2", "Selected_USERIDG3", "Selected_USERIDG4", "Selected_USERIDG5", "Selected_USERIDG6",
                "Selected_MAPIDPFrom", "Selected_MAPIDPTo", "Selected_MAPIDPG1", "Selected_MAPIDPG2", "Selected_MAPIDPG3", "Selected_MAPIDPG4", "Selected_MAPIDPG5", "Selected_MAPIDPG6",
                "Selected_MAPIDFrom", "Selected_MAPIDTo", "Selected_MAPIDG1", "Selected_MAPIDG2", "Selected_MAPIDG3", "Selected_MAPIDG4", "Selected_MAPIDG5", "Selected_MAPIDG6"}

            For Each eraseSessionName In eraseSessionNames
                HttpContext.Current.Session(eraseSessionName) = ""
            Next

            'ボタン押下時、画面遷移
            Server.Transfer(menuItm.Url)
        End If
    End Sub
    ''' <summary>
    ''' ガイダンスリンク押下時
    ''' </summary>
    Private Sub WF_ButtonShowGuidance_Click()
        Dim guidanceNo As String = WF_ButtonClick.Value.Replace("WF_ButtonShowGuidance", "")
        Me.SelectedGuidanceNo = guidanceNo
        'ボタン押下時、画面遷移
        Server.Transfer(Me.WF_HdnGuidanceUrl.Value)
    End Sub
    ''' <summary>
    ''' メニューボタン情報を取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetMenuItemList(sqlCon As MySqlConnection, ByVal strTitleKbn As String, Optional ByVal strHeadKbn As Integer = 1) As List(Of MenuItem)
        Dim retItm As New List(Of MenuItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT A.POSICOL")
        sqlStat.AppendLine("      ,A.POSIROW AS ROWLINE")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPID,''))      as MAPID")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.VARIANT,''))    as VARIANT")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.TITLENAMES,'')) as TITLE")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPNAMES,''))   as NAMES")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPNAMEL,''))   as NAMEL")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.REPORTFLG,''))   as REPORTFLG")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.REPORTID,''))   as REPORTID")
        sqlStat.AppendLine("      ,rtrim(coalesce(B.URL,''))        as URL")
        sqlStat.AppendLine("  FROM      COM.LNS0009_PROFMMAP           A")
        sqlStat.AppendLine("  LEFT JOIN COM.LNS0007_URL                B")
        sqlStat.AppendLine("    ON B.MAPID    = A.MAPID")
        sqlStat.AppendLine("   AND B.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND B.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND B.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" WHERE A.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("   AND A.MAPIDP   = @MAPIDP")
        sqlStat.AppendLine("   AND A.VARIANTP = @VARIANTP")
        sqlStat.AppendLine("   AND A.TITLEKBN = @TITLEKBN")
        sqlStat.AppendLine("   AND A.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND A.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND A.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY A.POSICOL,A.POSIROW")
        Using dt As New DataTable
            Using sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)

                With sqlCmd.Parameters
                    .Add("@CAMPCODE", MySqlDbType.VarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                    .Add("@MAPIDP", MySqlDbType.VarChar, 50).Value = Master.MAPID
                    .Add("@VARIANTP", MySqlDbType.VarChar, 50).Value = Master.ROLE_MENU
                    .Add("@STYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    .Add("@TITLEKBN", MySqlDbType.VarChar, 1).Value = strTitleKbn
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    dt.Load(sqlDr)
                    sqlDr.Close()
                End Using 'sqlDr
            End Using 'sqlCmd
            '取得結果を元にメニューアイテムクラスに格納
            '上位リストのみを取得()
            Dim topLevelList = From dr As DataRow In dt Where dr("ROWLINE").Equals(strHeadKbn)
            Dim childItems As List(Of DataRow) = Nothing
            '上位回送のリストループROWLINEが"1"のみ
            For Each topLevelItm In topLevelList
                Dim posiCol As Integer = CInt(topLevelItm("POSICOL"))
                childItems = (From dr As DataRow In dt Where dr("POSICOL").Equals(posiCol) AndAlso dr("ROWLINE").Equals(4)).ToList

                Dim retTopLevelItm = New MenuItem
                retTopLevelItm.PosiCol = CInt(topLevelItm("POSICOL"))
                retTopLevelItm.RowLine = CInt(topLevelItm("ROWLINE"))
                retTopLevelItm.MapId = Convert.ToString(topLevelItm("MAPID"))
                retTopLevelItm.Variant = Convert.ToString(topLevelItm("VARIANT"))
                retTopLevelItm.Title = Convert.ToString(topLevelItm("TITLE"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMES"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMEL"))
                retTopLevelItm.Reportflg = Convert.ToString(topLevelItm("REPORTFLG"))
                retTopLevelItm.Reportid = Convert.ToString(topLevelItm("REPORTID"))
                retTopLevelItm.Url = Convert.ToString(topLevelItm("URL"))

                If childItems.Count = 0 Then
                    '子供を完全に持たない
                    '一応意味はないがコケると困るので
                    If retTopLevelItm.Url = "" Then
                        retTopLevelItm.Url = "~/LNG/ex/page_404.html"
                    End If

                ElseIf childItems.Count = 1 Then
                    With childItems(0)
                        If retTopLevelItm.MapId = "" Then
                            retTopLevelItm.MapId = Convert.ToString(.Item("MAPID"))
                        End If
                        If retTopLevelItm.Variant = "" Then
                            retTopLevelItm.Variant = Convert.ToString(.Item("VARIANT"))
                        End If
                        If retTopLevelItm.Title = "" Then
                            retTopLevelItm.Title = Convert.ToString(.Item("TITLE"))
                        End If
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = Convert.ToString(.Item("URL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = "~/LNG/ex/page_404.html"
                        End If
                    End With
                Else
                    '名前が無ければ子供の先頭の名称を付与
                    With childItems(0)
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                    End With
                    For Each childItem In childItems
                        Dim retChildItm = New MenuItem
                        retChildItm.PosiCol = CInt(childItem("POSICOL"))
                        retChildItm.RowLine = CInt(childItem("ROWLINE"))
                        retChildItm.MapId = Convert.ToString(childItem("MAPID"))
                        retChildItm.Variant = Convert.ToString(childItem("VARIANT"))
                        retChildItm.Title = Convert.ToString(childItem("TITLE"))
                        retChildItm.Names = Convert.ToString(childItem("NAMES"))
                        retChildItm.Namel = Convert.ToString(childItem("NAMEL"))
                        retChildItm.Url = Convert.ToString(childItem("URL"))
                        If retChildItm.Url = "" Then
                            retChildItm.Url = "~/LNG/ex/page_404.html"
                        End If
                        retTopLevelItm.ChildMenuItem.Add(retChildItm)
                    Next childItem

                End If
                childItems = Nothing
                If retTopLevelItm.Names = "" Then
                    retTopLevelItm.Names = "　"
                End If

                Dim keyName As String = MP0000Base.GetBase64Str(retTopLevelItm.Names)
                Dim val As String = MP0000Base.LoadCookie(keyName, Me)
                Dim isOpen As Boolean = False
                If val <> "" Then
                    isOpen = Convert.ToBoolean(val)
                End If
                retTopLevelItm.OpenChild = isOpen
                retItm.Add(retTopLevelItm)
            Next topLevelItm

        End Using 'dt
        Return retItm

    End Function
    ''' <summary>
    ''' 表示用のガイダンスデータ取得
    ''' </summary>
    ''' <param name="sqlCon">MySqlConnection</param>
    ''' <returns>ガイダンスデータ</returns>
    Private Function GetGuidanceData(sqlCon As MySqlConnection) As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            .Add("GUIDANCENO", GetType(String))
            .Add("ENTRYDATE", GetType(String))
            .Add("TYPE", GetType(String))
            .Add("TITLE", GetType(String))
            .Add("NAIYOU", GetType(String))
            .Add("FILE1", GetType(String))
        End With
        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT GD.GUIDANCENO")
            sqlStat.AppendLine("      ,date_format(GD.INITYMD,'%Y/%m/%d') AS ENTRYDATE")
            sqlStat.AppendLine("      ,GD.TYPE                       AS TYPE")
            sqlStat.AppendLine("      ,GD.TITLE                      AS TITLE")
            sqlStat.AppendLine("      ,GD.NAIYOU                     AS NAIYOU")
            sqlStat.AppendLine("      ,GD.FILE1                      AS FILE1")
            sqlStat.AppendLine("  FROM com.LNS0008_GUIDANCE GD")
            sqlStat.AppendLine(" WHERE CURDATE() BETWEEN GD.FROMYMD AND GD.ENDYMD")
            sqlStat.AppendLine("   AND DELFLG = @DELFLG_NO")
            sqlStat.AppendLine("   AND OUTFLG <> '1'")
            Dim userOrg = Master.USER_ORG
            If Not {"jot_lng_1", "jot_sys_1"}.Contains(CS0050Session.VIEW_MENU_MODE) Then
                Dim targetDispFlags = LNS0008WRKINC.GetNewDisplayFlags
                Dim showDispFlag = (From flg In targetDispFlags Where flg.OfficeCode = userOrg Select flg.FieldName).FirstOrDefault
                If showDispFlag <> "" Then
                    sqlStat.AppendFormat("   AND {0} = '1'", showDispFlag).AppendLine()
                Else
                    sqlStat.AppendLine("   AND 1 = 2")
                End If
            End If
            sqlStat.AppendLine(" ORDER BY (CASE WHEN GD.TYPE = 'E' THEN '1'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'W' THEN '2'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'I' THEN '3'")
            sqlStat.AppendLine("                ELSE '9'")
            sqlStat.AppendLine("            END)")
            sqlStat.AppendLine("          ,GD.INITYMD DESC")
            '他のフラグや最大取得件数（条件がある場合）はあとで
            Using sqlGuidCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlGuidCmd.Parameters.Add("@DELFLG_NO", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlGuidDr As MySqlDataReader = sqlGuidCmd.ExecuteReader()
                    Dim dr As DataRow
                    While sqlGuidDr.Read
                        dr = retDt.NewRow
                        dr("GUIDANCENO") = sqlGuidDr("GUIDANCENO")
                        dr("ENTRYDATE") = sqlGuidDr("ENTRYDATE")
                        dr("TYPE") = sqlGuidDr("TYPE")
                        dr("TITLE") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("TITLE")))
                        dr("NAIYOU") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("NAIYOU"))).Replace(ControlChars.CrLf, "<br />").Replace(ControlChars.Cr, "<br />").Replace(ControlChars.Lf, "<br />")
                        dr("FILE1") = Convert.ToString(sqlGuidDr("FILE1"))

                        retDt.Rows.Add(dr)
                    End While
                End Using

            End Using
            sqlStat = New StringBuilder
            sqlStat.AppendLine("SELECT URL.URL")
            sqlStat.AppendLine("  FROM COM.LNS0007_URL URL")
            sqlStat.AppendLine(" WHERE URL.MAPID = @MAPID")
            sqlStat.AppendLine("   AND CURDATE() BETWEEN URL.STYMD AND URL.ENDYMD")
            sqlStat.AppendLine("   AND URL.DELFLG = @DELFLG")

            Using sqlGuidUrlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                With sqlGuidUrlCmd.Parameters
                    .Add("@MAPID", MySqlDbType.VarChar).Value = LNS0008WRKINC.MAPIDC
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Dim urlVal = sqlGuidUrlCmd.ExecuteScalar
                Me.WF_HdnGuidanceUrl.Value = Convert.ToString(urlVal)
            End Using
        Catch ex As Exception
            Return retDt
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' 支店名取得
    ''' </summary>
    ''' <param name="sqlCon">MySqlConnection</param>
    ''' <returns>ガイダンスデータ</returns>
    Private Function GetShitenNmae(sqlCon As MySqlConnection) As String
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable()
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim strName As String = ""

        Try
            Dim sqlStat As New StringBuilder
            With sqlText
                .AppendLine("SELECT")
                .AppendLine("    CASE A01.CONTROLCODE")
                .AppendLine("        WHEN '' THEN A01.NAME")
                .AppendLine("        ELSE A02.NAME")
                .AppendLine("    END AS NAME")
                .AppendLine("FROM")
                .AppendLine("    com.LNS0019_ORG A01")
                .AppendLine("    LEFT JOIN com.LNS0019_ORG A02")
                .AppendLine("        ON A02.ORGCODE = A01.CONTROLCODE")
                .AppendLine("WHERE")
                .AppendLine("    A01.ORGCODE = '" & Master.USER_ORG & "'")
            End With

            Using tran = sqlCon.BeginTransaction
                'SQL実行
                CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, tran)
            End Using

            If sqlRetSet.Rows.Count > 0 Then
                strName = GetStringValue(sqlRetSet, 0, "NAME")
            End If
        Catch ex As Exception
            Return ""
        End Try

        Return strName
    End Function
    ''' <summary>
    ''' DataTableの指定位置からString値を取得する
    ''' </summary>
    ''' <param name="objOutputData">DataTable</param>
    ''' <param name="nRow">行</param>
    ''' <param name="strCol">列</param>
    ''' <param name="strDefault">規定値</param>
    ''' <returns>取得データ</returns>
    ''' <remarks>値がDBNULLの場合は規定値が返却される</remarks>
    Private Shared Function GetStringValue(ByVal objOutputData As DataTable, ByVal nRow As Integer, ByVal strCol As String, Optional ByVal strDefault As String = "") As String
        Dim strRet As String = strDefault
        Dim objCell As Object = objOutputData.Rows(nRow)(strCol)

        If Not IsDBNull(objCell) Then
            strRet = objCell.ToString
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' カレンダーマスタから前稼働日を取得する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="TAREGETDATE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetWorkingDate(ByVal SQLcon As MySqlConnection, ByVal TAREGETDATE As Date) As Date
        Dim WorkingDate As Date = Nothing
        '検索SQL文
        Try
            Dim SQLStr As String =
                 " SELECT TOP 1 " _
               & "     rtrim(A.WORKINGYMD) AS WORKINGYMD   " _
               & " FROM COM.LNS0021_CALENDAR A             " _
               & " WHERE                                   " _
               & "           A.WORKINGYMD <= @P1           " _
               & "       and A.CALENDARKBN = '01'          " _
               & "       and A.WORKINGKBN = '0'            " _
               & "       and A.DELFLG     <> @P2           " _
               & " ORDER BY A.WORKINGYMD DESC              "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.Date).Value = TAREGETDATE
                    .Add("@P2", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    WorkingDate = CDate(SQLdr("WORKINGYMD"))
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Return WorkingDate
        End Try

        Return WorkingDate

    End Function

    ''' <summary>
    ''' URLをクリアする
    ''' </summary>
    Private Sub ClearURLValue()

        WF_PrintURL01.Value = ""
        WF_PrintURL02.Value = ""
        WF_PrintURL03.Value = ""
        WF_PrintURL04.Value = ""
        WF_PrintURL05.Value = ""
        WF_PrintURL06.Value = ""
        WF_PrintURL07.Value = ""
        WF_PrintURL08.Value = ""
        WF_PrintURL09.Value = ""
        WF_PrintURL10.Value = ""
        WF_PrintURL11.Value = ""
        WF_PrintURL12.Value = ""
        WF_PrintURL13.Value = ""
        WF_PrintURL14.Value = ""
        WF_PrintURL15.Value = ""

    End Sub

    ''' <summary>
    ''' 作成されたURLをセットする
    ''' </summary>
    ''' <param name="URL"></param>
    ''' <param name="Printcount"></param>
    Private Sub SetURLValue(ByVal URL As String, ByVal Printcount As Integer)

        Select Case Printcount
            Case 1
                WF_PrintURL01.Value = URL
            Case 2
                WF_PrintURL02.Value = URL
            Case 3
                WF_PrintURL03.Value = URL
            Case 4
                WF_PrintURL04.Value = URL
            Case 5
                WF_PrintURL05.Value = URL
            Case 6
                WF_PrintURL06.Value = URL
            Case 7
                WF_PrintURL07.Value = URL
            Case 8
                WF_PrintURL08.Value = URL
            Case 9
                WF_PrintURL09.Value = URL
            Case 10
                WF_PrintURL10.Value = URL
            Case 11
                WF_PrintURL11.Value = URL
            Case 12
                WF_PrintURL12.Value = URL
            Case 13
                WF_PrintURL13.Value = URL
            Case 14
                WF_PrintURL14.Value = URL
            Case 15
                WF_PrintURL15.Value = URL
        End Select

    End Sub
    ''' <summary>
    ''' 左ナビの開閉状態をcookieに保存
    ''' </summary>
    Private Sub LeftNavColleLNToSaveCookie(ByVal repTargetNav As Repeater, ByVal strTopItem As String)
        '左ナビの表示アイテムが無い場合は終了
        If repTargetNav Is Nothing OrElse repTargetNav.Items.Count = 0 Then
            Return
        End If
        For Each repItm As RepeaterItem In repTargetNav.Items
            Dim chkObj As CheckBox = DirectCast(repItm.FindControl(strTopItem), CheckBox)
            If chkObj Is Nothing Then
                Continue For
            End If

            Dim keyName As String = MP0000Base.GetBase64Str(chkObj.Text)
            Dim val As String = Convert.ToString(chkObj.Checked)
            MP0000Base.SaveCookie(keyName, val, Me)
        Next repItm
    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub FIELD_Change()

    End Sub

    ''' <summary>
    ''' 画面表示用遷移ボタンアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class MenuItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ChildMenuItem = New List(Of MenuItem)
            Me.OpenChild = False
        End Sub
        ''' <summary>
        ''' 列表示(PROFMAP:POSICOL)
        ''' </summary>
        ''' <returns></returns>
        Public Property PosiCol As Integer
        ''' <summary>
        ''' 行位置(PROFMAP:POSIROW) ⇒ 親クラスリストとして利用する場合は"1"のみ、子で再帰利用している箇所は"1"以外
        ''' </summary>
        ''' <returns></returns>
        Public Property RowLine As Integer
        ''' <summary>
        ''' 画面ＩＤ(PROFMAP:MAPID)
        ''' </summary>
        ''' <returns></returns>
        Public Property MapId As String
        ''' <summary>
        ''' 変数(PROFMAP:VARIANT)
        ''' </summary>
        ''' <returns></returns>
        Public Property [Variant] As String
        ''' <summary>
        ''' タイトル名称(PROFMAP:TITLENAMES)⇒左ナビのCSSクラス名として設定
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title As String
        ''' <summary>
        ''' 画面名称（短）(PROFMAP:MAPNAMES) ⇒ ボタン名称に設定
        ''' </summary>
        ''' <returns></returns>
        Public Property Names As String
        ''' <summary>
        ''' 画面名称（長）(PROFMAP:MAPNAMEL) ⇒ 現状当プロパティに投入のみ未使用
        ''' </summary>
        ''' <returns></returns>
        Public Property Namel As String
        '''<summary>
        '''帳票フラグ (PROFMAP:REPORTFLG)
        '''</summary>
        '''<returns></returns>
        Public Property Reportflg As String
        '''<summary>
        '''帳票ID (PROFMAP:REPORTID)
        '''</summary>
        '''<returns></returns>
        Public Property Reportid As String
        ''' <summary>
        ''' URL（URLマスタ：URL）チルダ付き（アプリルート相対）の遷移URL
        ''' </summary>
        ''' <returns></returns>
        Public Property Url As String
        ''' <summary>
        ''' POSICOLが同一でROWLINが1以外の子データを格納
        ''' </summary>
        ''' <returns></returns>
        Public Property ChildMenuItem As List(Of MenuItem)
        ''' <summary>
        ''' 子要素の表示状態
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>現状未使用：ポストバック発生時に閉じてしまったら利用検討</remarks>
        Public Property OpenChild As Boolean = False

        ''' <summary>
        ''' 子要素を持っているか（デザイン判定用：▼表示判定）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>ある程度「孫・ひ孫」対応できる構造だが現状「子」のみ</remarks>
        Public ReadOnly Property HasChild As Boolean
            Get
                If ChildMenuItem Is Nothing OrElse ChildMenuItem.Count = 0 Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property
        ''' <summary>
        ''' 次ページ遷移情報を持つか(True：次画面遷移あり、False：次画面遷移無し)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasNextPageInfo As Boolean
            Get
                'MAPIDを持つか持たないかで判定
                If Me.MapId.Trim.Equals("") Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

    End Class

#Region "ViewStateを圧縮 個人用ペインでかなり大きくなると予想"
    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
#End Region
End Class