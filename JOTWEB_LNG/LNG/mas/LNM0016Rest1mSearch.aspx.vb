''************************************************************
' 使用料特例１マスタメンテ検索画面
' 作成日 2022/02/18
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴:2022/02/18 新規作成
'         :
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 使用料特例１マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNM0016Rest1mSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

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
                    Case "mspStationSingleRowSelected" '[共通]駅選択ポップアップで行選択
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
        Master.MAPID = LNM0016WRKINC.MAPIDS

        TxtOrgCode.Focus()
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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then
            ' メニューからの画面遷移
            ' 画面間の情報クリア
            work.Initialize()

            ' 初期変数設定処理
            TxtOrgCode.Text = ""          '組織コード
            TxtBigCTNCD.Text = ""         '大分類コード
            TxtMiddleCTNCD.Text = ""      '中分類コード
            TxtDepStation.Text = ""       '発駅コード
            TxtDepTrusteeCd.Text = ""     '発受託人コード
            TxtDepTrusteeSubCd.Text = ""  '発受託人サブコード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0016L Then
            ' 実行画面からの遷移
            TxtOrgCode.Text = work.WF_SEL_ORG.Text                      '組織コード
            TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD.Text                '大分類コード
            TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD.Text          '中分類コード
            TxtDepStation.Text = work.WF_SEL_DEPSTATION.Text            '発駅コード
            TxtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD.Text        '発受託人コード
            TxtDepTrusteeSubCd.Text = work.WF_SEL_DEPTRUSTEESUBCD.Text  '発受託人サブコード
            ' 論理削除フラグ
            If work.WF_SEL_DELDATAFLG.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理
            CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)                                  '組織コード
            CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)                       '大分類コード
            CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)              '中分類コード
            CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)                    '発駅コード
            CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)           '発受託人コード
            CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_Dummy)  '発受託人コード
        End If

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNM0016WRKINC.MAPIDS
        rightview.MAPID = LNM0016WRKINC.MAPIDL
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
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSEARCH_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(TxtOrgCode.Text)          '組織コード
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)         '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)      '中分類コード
        Master.EraseCharToIgnore(TxtDepStation.Text)       '発駅コード
        Master.EraseCharToIgnore(TxtDepTrusteeCd.Text)     '発受託人コード
        Master.EraseCharToIgnore(TxtDepTrusteeSubCd.Text)  '発受託人サブコード

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_ORG.Text = TxtOrgCode.Text                      '組織コード
        work.WF_SEL_BIGCTNCD.Text = TxtBigCTNCD.Text                '大分類コード
        work.WF_SEL_MIDDLECTNCD.Text = TxtMiddleCTNCD.Text          '中分類コード
        work.WF_SEL_DEPSTATION.Text = TxtDepStation.Text            '発駅コード
        work.WF_SEL_DEPTRUSTEECD.Text = TxtDepTrusteeCd.Text        '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD.Text = TxtDepTrusteeSubCd.Text  '発受託人サブコード
        ' 論理削除フラグ
        If ChkDelDataFlg.Checked = True Then
            work.WF_SEL_DELDATAFLG.Text = "1"
        Else
            work.WF_SEL_DELDATAFLG.Text = "0"
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

        ' 組織コード
        Master.CheckField(Master.USERCAMP, "ORG", TxtOrgCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "組織コード : " & TxtOrgCode.Text, needsPopUp:=True)
                TxtOrgCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "組織コード", needsPopUp:=True)
            TxtOrgCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 大分類コード
        Master.CheckField(Master.USERCAMP, "BIGCTNCD", TxtBigCTNCD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtBigCTNCD.Text) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "大分類コード : " & TxtBigCTNCD.Text, needsPopUp:=True)
                    TxtBigCTNCD.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "大分類コード", needsPopUp:=True)
            TxtBigCTNCD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 中分類コード
        Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", TxtMiddleCTNCD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtMiddleCTNCD.Text) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "中分類コード : " & TxtMiddleCTNCD.Text, needsPopUp:=True)
                    TxtMiddleCTNCD.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "中分類コード", needsPopUp:=True)
            TxtMiddleCTNCD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 発駅コード
        Master.CheckField(Master.USERCAMP, "DEPSTATION", TxtDepStation.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtDepStation.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStationName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発駅コード : " & TxtDepStation.Text, needsPopUp:=True)
                    TxtDepStation.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発駅コード", needsPopUp:=True)
            TxtDepStation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 発受託人コード
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", TxtDepTrusteeCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtDepTrusteeCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発受託人コード : " & TxtDepTrusteeCd.Text, needsPopUp:=True)
                    TxtDepTrusteeCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発受託人コード", needsPopUp:=True)
            TxtDepTrusteeCd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 発受託人サブコード
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtDepTrusteeSubCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発受託人サブコード : " & TxtDepTrusteeSubCd.Text, needsPopUp:=True)
                    TxtDepTrusteeSubCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発受託人サブコード", needsPopUp:=True)
            TxtDepTrusteeSubCd.Focus()
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
        Master.TransitionPrevPage(, LNM0016WRKINC.TITLEKBNS)

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
                .Visible = true
                Select Case WF_FIELD.Value
                    Case "TxtOrgCode"          '組織コード
                        WW_prmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtBigCTNCD"         '大分類コード
                        WW_prmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    Case "TxtMiddleCTNCD"      '中分類コード
                        WW_prmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                    Case "TxtDepStation"       '発駅コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspStationSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtDepTrusteeCd"     '発受託人コード
                        WW_prmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text)
                    Case "TxtDepTrusteeSubCd"  '発受託人サブコード
                        WW_prmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text)
                End Select
                .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_prmData)
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
        Select Case WF_FIELD.Value
            Case "TxtOrgCode"          '組織コード
                CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
                TxtOrgCode.Focus()
            Case "TxtBigCTNCD"         '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
                TxtBigCTNCD.Focus()
            Case "TxtMiddleCTNCD"      '中分類コード
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
                TxtMiddleCTNCD.Focus()
            Case "TxtDepStation"       '発駅コード
                CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStationName.Text, WW_RtnSW)
            Case "TxtDepTrusteeCd"     '発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_RtnSW)
                TxtDepTrusteeCd.Focus()
            Case "TxtDepTrusteeSubCd"  '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_RtnSW)
                TxtDepTrusteeSubCd.Focus()
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

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "TxtOrgCode"          '組織コード
                TxtOrgCode.Text = WW_SelectValue
                LblOrgName.Text = WW_SelectText
                TxtOrgCode.Focus()
            Case "TxtBigCTNCD"         '大分類コード
                TxtBigCTNCD.Text = WW_SelectValue
                LblBigCTNCDName.Text = WW_SelectText
                TxtBigCTNCD.Focus()
            Case "TxtMiddleCTNCD"      '中分類コード
                TxtMiddleCTNCD.Text = WW_SelectValue
                LblMiddleCTNCDName.Text = WW_SelectText
                TxtMiddleCTNCD.Focus()
            Case "TxtDepStation"       '発駅コード
                TxtDepStation.Text = WW_SelectValue
                LblDepStationName.Text = WW_SelectText
                TxtDepStation.Focus()
            Case "TxtDepTrusteeCd"     '発受託人コード
                TxtDepTrusteeCd.Text = WW_SelectValue
                LblDepTrusteeCdName.Text = WW_SelectText
                TxtDepTrusteeCd.Focus()
            Case "TxtDepTrusteeSubCd"  '発受託人サブコード
                TxtDepTrusteeSubCd.Text = WW_SelectValue
                LblDepTrusteeSubCdName.Text = WW_SelectText
                TxtDepTrusteeSubCd.Focus()
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
            Case "TxtOrgCode"          '組織コード
                TxtOrgCode.Focus()
            Case "TxtBigCTNCD"         '大分類コード
                TxtBigCTNCD.Focus()
            Case "TxtMiddleCTNCD"      '中分類コード
                TxtMiddleCTNCD.Focus()
            Case "TxtDepStation"       '発駅コード
                TxtDepStation.Focus()
            Case "TxtDepTrusteeCd"     '発受託人コード
                TxtDepTrusteeCd.Focus()
            Case "TxtDepTrusteeSubCd"  '発受託人サブコード
                TxtDepTrusteeSubCd.Focus()
        End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
        Me.TxtDepStation.Text = selData("STATION").ToString
        Me.LblDepStationName.Text = selData("NAMES").ToString
        Me.TxtDepStation.Focus()

        'ポップアップの非表示
        Me.mspStationSingle.HidePopUp()

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
                Case "ORG"              '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"         '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"      '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "DEPSTATION"       '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"     '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text))
                Case "DEPTRUSTEESUBCD"  '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
