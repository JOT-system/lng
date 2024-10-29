''************************************************************
' レンタルコンテナ運用個数表画面
' 作成日 2023/02/15
' 更新日 
' 作成者 星
' 更新者 
'
' 修正履歴:2023/02/15 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' レンタルコンテナ運用個数表画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0016OperatinNumReportOutput
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIT0016Reporttbl As DataTable                   '帳票用テーブル

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
        Master.MAPID = LNT0016WRKINC.MAPID

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

        ' 初期変数設定処理
        txtDownloadMonth.Text = Format(Date.Now, "yyyy/MM")
        CODENAME_get("ORG", TxtCtnClass.Text, Me.LblCtnClass.Text, WW_Dummy)          'コンテナ種別名

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtCtnClass.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPID = LNT0016WRKINC.MAPID
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)

    End Sub

    ''' <summary>
    ''' 年月取得処理
    ''' </summary>
    ''' <param name="strTarget">対象の年月日（スラッシュ有）</param>
    ''' <returns>年月</returns>
    ''' <remarks></remarks>
    Private Function getKeijoYM(ByVal strTarget As String) As String

        Dim strRetKeijoYM As String = ""

        strRetKeijoYM = strTarget
        If Len(strRetKeijoYM) >= 7 Then
            strRetKeijoYM = Strings.Left(strRetKeijoYM, 7)
        End If

        getKeijoYM = strRetKeijoYM

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
            Dim url As String = ""
            Dim printErrFlg As Boolean = False
            WF_PrintURL.Value = ""

            '○ 入力文字置き換え(使用禁止文字排除)
            Master.EraseCharToIgnore(TxtCtnClass.Text)       'コンテナ種別

            '******************************
            '帳票表示データ取得処理
            '******************************
            Dim dt As DataTable = Me.OperatingQuantityDataGet(PRTID, type)
                'データ0件時
                If dt.Rows.Count = 0 Then
                printErrFlg = True
            End If

            '******************************
            '帳票作成処理の実行
            '******************************
            If Not printErrFlg Then
                Dim Report As New LNT0016_OperatinNumReport_DIODOC(Master.MAPID, "レンタルコンテナ運用個数表_TEMPLATE.xlsx", dt, type)
                Try
                    url = Report.CreateExcelPrintData(MAXPAGE_GET(dt), TxtCtnClass.Text)
                    WF_PrintURL1.Value = url
                Catch ex As Exception
                    Throw
                End Try
            End If

            '両方データ0件時
            If printErrFlg Then
                Master.Output(C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ABORT, I_PARA01:="期間内で", I_PARA02:="実績", needsPopUp:=True)
                Exit Sub
            End If

            '○ 別画面でExcelを表示
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_AccountingDownload();", True)

        Catch sqlex As MySqlException
            CS0011LOGWRITE.INFSUBCLASS = "LNT0016S " & SqlName          'SUBクラス名 + ストアド名称
            CS0011LOGWRITE.INFPOSI = "WF_ButtonOUTPUT_Click"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = sqlex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。システム管理者へ連絡して下さい。", needsPopUp:=True)
            Exit Sub

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "LNT0016S"   'SUBクラス名
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0016WRKINC.TITLEKBNS)

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
                If CInt(WF_LeftMViewChange.Value) = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE Then
                    Select Case WF_FIELD.Value
                        Case "TxtCtnClass"           'コンテナ種別
                            WW_prmData = work.CreateFIXParam(Master.USERCAMP, "CTNCLASS")
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
            Case "TxtCtnClass"          'コンテナ種別
                CODENAME_get("CTN", TxtCtnClass.Text, LblCtnClass.Text, WW_Dummy)
                TxtCtnClass.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

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
            Case "TxtCtnClass"           'コンテナ種別
                TxtCtnClass.Text = WW_SelectValue
                LblCtnClass.Text = WW_SelectText
                TxtCtnClass.Focus()

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
            Case "TxtCtnClass"          'コンテナ種別
                TxtCtnClass.Focus()
        End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 頁数分母取得
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <remarks></remarks>
    Private Function MAXPAGE_GET(dt As DataTable)
        '大分類コードの件数を取得
        Dim intPageCnt As Integer = 0
        Dim BigCtnCdchk As String = ""

        For Each OutputRowData As DataRow In dt.Rows
            Dim strBigCtnCd As String = ""

            strBigCtnCd = OutputRowData("BIGCTNCD").ToString

            '大分類コードが変わったとき頁数分母加算
            If Not strBigCtnCd.Equals(BigCtnCdchk) Then
                intPageCnt += 1
            End If
            BigCtnCdchk = strBigCtnCd
        Next

        '合計頁が存在するときのみ頁数分母加算
        If TxtCtnClass.Text = "88" Or TxtCtnClass.Text = "" Then
            intPageCnt += 1
        Else
            '全種別以外は1頁のみ
            intPageCnt = 1
        End If

        Return intPageCnt

    End Function

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
                Case "CTN"              'コンテナ種別
                    'If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                    ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "CTNCLASS"))
                    'Else
                    ' その他の場合、操作ユーザーの組織のみ取得
                    'leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "CTNCLASS"))
                    'End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' レンタルコンテナ運用個数表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function OperatingQuantityDataGet(ReportID As String, type As String) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                SQLcmd.CommandText = "lng.[PRT_OPERATION_NUM]"
                SqlName = "PRT_OPERATION_NUM"

                SQLcmd.Parameters.Clear()
                Dim PARA0 As MySqlParameter = SQLcmd.Parameters.Add("@piTARGETYM", MySqlDbType.VarChar, 7)          ' 対象年月
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROMDATE", MySqlDbType.VarChar, 10)         ' FROM年月日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTODATE", MySqlDbType.VarChar, 10)           ' TO年月日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piCTNCLASS", MySqlDbType.VarChar, 2)          ' コンテナ種別
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piMARKFLG", MySqlDbType.VarChar, 1)           ' 当月判別フラグ

                Dim fromDate As DateTime = DateTime.Parse(txtDownloadMonth.Text + "/01")
                Dim toDate As Date

                PARA0.Value = txtDownloadMonth.Text

                PARA1.Value = fromDate.AddDays(-1).ToString("yyyy/MM/dd")

                If txtDownloadMonth.Text = Format(Date.Now, "yyyy/MM") Then
                    toDate = Date.Now
                    PARA4.Value = "1"
                Else
                    '月初日をセット
                    toDate = New Date(Left(txtDownloadMonth.Text.ToString, 4), Right(txtDownloadMonth.Text.ToString, 2), 1)
                    '月末日の取得
                    toDate = toDate.AddMonths(1).AddDays(-1)
                    PARA4.Value = "0"
                End If
                PARA2.Value = toDate.ToString("yyyy/MM/dd")

                If TxtCtnClass.Text = "" Then
                    PARA3.Value = "88"
                Else
                    PARA3.Value = TxtCtnClass.Text
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

End Class
