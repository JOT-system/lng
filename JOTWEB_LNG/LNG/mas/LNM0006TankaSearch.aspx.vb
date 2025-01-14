''************************************************************
' 単価マスタメンテ検索画面
' 作成日 2024/12/16
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/16 新規作成
'          : 
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports MySql.Data.MySqlClient

''' <summary>
''' 単価マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNM0006TankaSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力

    ''' <summary>
    ''' 共通処理結果
    ''' </summary>
    Private WW_ErrSW As String
    Private WW_RtnSW As String
    Private WW_Dummy As String

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
                    Case "WF_ButtonSEARCH"              '検索ボタン押下
                        WF_ButtonSEARCH_Click()
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
                    Case "mspToriOrgCodeSingleRowSelected"  '[共通]取引先部門コード選択ポップアップで行選択
                        RowSelected_mspToriOrgCodeSingle()
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
        Master.MAPID = LNM0006WRKINC.MAPIDS

        WF_StYMDCode.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '○ ドロップダウンリスト生成
        createListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' ドロップダウン生成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub createListBox()

        Try
            '部門ドロップダウンのクリア
            Me.ddlSelectORG.Items.Clear()
            Me.ddlSelectORG.Items.Add("")

            '部門ドロップダウンの生成
            'Dim retOfficeList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "ORGCODEDROP")
            Dim retOfficeList As DropDownList = CmnLng.getDowpDownFixedList("02", "ORGCODEDROP")

            If retOfficeList.Items.Count > 0 Then
                For index As Integer = 0 To retOfficeList.Items.Count - 1
                    ddlSelectORG.Items.Add(New ListItem(retOfficeList.Items(index).Text, retOfficeList.Items(index).Value))
                Next
            End If

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = New StackFrame(0, False).GetMethod.DeclaringType.FullName  ' クラス名
            CS0011LOGWrite.INFPOSI = Reflection.MethodBase.GetCurrentMethod.Name                    ' メソッド名
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then
            ' メニューからの画面遷移
            '情シス、高圧ガス以外
            If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                ' 画面遷移
                Master.TransitionPage()
            End If

            ' 画面間の情報クリア
            work.Initialize()

            ' 初期変数設定処理
            'Master.GetFirstValue(Master.USERCAMP, "STYMD", WF_StYMDCode.Value)  '有効開始日
            WF_StYMDCode.Value = Date.Now.ToString("yyyy/MM/dd")

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0006L Then
            ' 実行画面からの遷移
            '情シス、高圧ガス以外
            If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                '○ メニュー画面遷移
                Master.TransitionPrevPage(, LNM0006WRKINC.TITLEKBNS)
            End If

            WF_StYMDCode.Value = work.WF_SEL_STYMD_S.Text    '有効開始日
            TxtTORICode.Text = work.WF_SEL_TORICODE_S.Text   '取引先コード
            ddlSelectORG.SelectedValue = work.WF_SEL_ORGCODE_S.Text     '部門コード

            ' 論理削除フラグ
            If work.WF_SEL_DELFLG_S.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理
            CODENAME_get("TORICODE", TxtTORICode.Text, LblTORIName.Text, WW_Dummy)  '取引先コード
        End If
        'Master.GetFirstValue(Master.USERCAMP, "CAMPCODE", TxtCampCode.Text)  '会社コード
        TxtCampCode.Text = Master.USERCAMP

        ' 取引先コードを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtTORICode.Attributes("onkeyPress") = "CheckNum()"

        ' 有効年月日(開始)を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_StYMDCode.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNM0006WRKINC.MAPIDS
        rightview.MAPID = LNM0006WRKINC.MAPIDL
        rightview.COMPCODE = TxtCampCode.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_Dummy)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU


        '○ 名称設定処理
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_Dummy)  '会社コード

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSEARCH_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_StYMDCode.Value)             '有効開始日
        Master.EraseCharToIgnore(TxtTORICode.Text)               '取引先コード

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_STYMD_S.Text = WF_StYMDCode.Value.ToString     '有効開始日
        work.WF_SEL_TORICODE_S.Text = TxtTORICode.Text             '取引先コード
        work.WF_SEL_ORGCODE_S.Text = ddlSelectORG.SelectedValue               '部門コード

        ' 論理削除フラグ
        If ChkDelDataFlg.Checked = True Then
            work.WF_SEL_DELFLG_S.Text = "1"
        Else
            work.WF_SEL_DELFLG_S.Text = "0"
        End If

        '○ 画面レイアウト設定
        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightview.GetViewId(TxtCampCode.Text)
        End If

        Master.CheckParmissionCode(TxtCampCode.Text)
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
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""

        ' 有効開始日
        If WF_StYMDCode.Value = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "有効開始日", needsPopUp:=True)
            WF_StYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(Master.USERCAMP, "STYMD", WF_StYMDCode.Value, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WF_StYMDCode.Value) Then
                WF_StYMDCode.Value = CDate(WF_StYMDCode.Value)
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "有効開始日", needsPopUp:=True)
            WF_StYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '取引先コード
        Master.CheckField(Master.USERCAMP, "TORICODE", TxtTORICode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtTORICode.Text) Then
                ' 名称存在チェック
                CODENAME_get("TORICODE", TxtTORICode.Text, LblTORIName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "取引先コード : " & TxtTORICode.Text, needsPopUp:=True)
                    TxtTORICode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "取引先コード", needsPopUp:=True)
            TxtTORICode.Focus()
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
        Master.TransitionPrevPage(, LNM0006WRKINC.TITLEKBNS)

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledDBClick()

        Dim WW_PrmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                ' フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    Case "TxtTORICode"       '取引先コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspToriOrgCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub

                End Select
                .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledChange()

        '○ 変更した項目の名称をセット
        'CODENAME_get("ORG", ddlSelectORG.SelectedValue, LblOrgName.Text, WW_RtnSW)  '組織コード
        'TxtOrgCode.Focus()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "TxtTORICode"
                CODENAME_get("TORICODE", TxtTORICode.Text, LblTORIName.Text, WW_RtnSW)  '取引先コード
                TxtTORICode.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
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
        Dim WW_SelectDate As Date

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_StYMDCode"             '有効開始日
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        WF_StYMDCode.Value = ""
                    Else
                        WF_StYMDCode.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                WF_StYMDCode.Focus()
                'Case "WF_EndYMDCode"            '有効終了日
                '    Try
                '        Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                '        If WW_SelectDate < C_DEFAULT_YMD Then
                '            WF_EndYMDCode.Value = ""
                '        Else
                '            WF_EndYMDCode.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                '        End If
                '    Catch ex As Exception

                '    End Try
                '    WF_EndYMDCode.Focus()
                'Case "TxtOrgCode"               '組織コード
                '    ddlSelectORG.SelectedValue = WW_SelectValue
                '    LblOrgName.Text = WW_SelectText
                '    TxtOrgCode.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
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
            Case "WF_StYMDCode"             '有効開始日
                WF_StYMDCode.Focus()

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 取引先部門コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspToriOrgCodeSingle()

        Me.mspToriOrgCodeSingle.InitPopUp()
        Me.mspToriOrgCodeSingle.SelectionMode = ListSelectionMode.Single
        Me.mspToriOrgCodeSingle.SQL = CmnSearchSQL.GetTankaToriOrgSQL(ddlSelectORG.SelectedValue)

        Me.mspToriOrgCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspToriOrgCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaToriOrgTitle)

        Me.mspToriOrgCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 取引先部門コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspToriOrgCodeSingle()

        Dim selData = Me.mspToriOrgCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtTORICode.ID
                Me.TxtTORICode.Text = selData("TORICODE").ToString '取引先コード
                Me.LblTORIName.Text = selData("TORINAME").ToString '取引先名
        End Select

        'ポップアップの非表示
        Me.mspToriOrgCodeSingle.HidePopUp()

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

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "TORICODE"             '取引先コード
                    work.CODENAMEGetTORI(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, TxtCampCode.Text))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, TxtCampCode.Text))
                    End If
                Case "TORICODE"              '取引先コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                        O_RTN = C_MESSAGE_NO.NORMAL
                    End If

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
