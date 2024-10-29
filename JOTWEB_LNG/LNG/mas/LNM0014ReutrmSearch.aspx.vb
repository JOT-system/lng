''************************************************************
' 通運発送料マスタメンテ検索画面
' 作成日 2022/03/01
' 更新日 
' 作成者 瀬口
' 更新者 
'
' 修正履歴 : 2022/03/01 新規作成
'          : 
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 通運発送料マスタ登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class LNM0014ReutrmSearch
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
        Master.MAPID = LNM0014WRKINC.MAPIDS

        txtBigCtnCd.Focus()
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
            txtBigCtnCd.Text = ""                                                        '大分類コード
            txtMiddleCtnCd.Text = ""                                                     '中分類コード
            txtDepStation.Text = ""                                                      '発駅コード
            txtDepTrusteeCd.Text = ""                                                    '発受託人コード
            txtDepTrusteeSubCd.Text = ""                                                 '発受託人サブコード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0014L Then
            ' 実行画面からの遷移
            txtBigCtnCd.Text = work.WF_SEL_BIGCTNCD.Text                                 '大分類コード
            txtMiddleCtnCd.Text = work.WF_SEL_MIDDLECTNCD.Text                           '中分類コード
            txtDepStation.Text = work.WF_SEL_DEPSTATION.Text                             '発駅コード
            txtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD.Text                         '発受託人コード
            txtDepTrusteeSubCd.Text = work.WF_SEL_DEPTRUSTEESUBCD.Text                   '発受託人サブコード

            ' 論理削除フラグ
            If work.WF_SEL_DELDATAFLG.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If

            CODENAME_get("BIGCTNCD", txtBigCtnCd.Text, lblBigCtnCdName.Text, WW_Dummy)                         '大分類コード
            CODENAME_get("MIDDLECTNCD", txtMiddleCtnCd.Text, lblMiddleCtnCdName.Text, WW_Dummy)                '中分類コード
            CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationName.Text, WW_Dummy)                   '発駅コード
            CODENAME_get("DEPTRUSTEECD", txtDepTrusteeCd.Text, lblDepTrusteeCdName.Text, WW_Dummy)             '発受託人コード
            CODENAME_get("DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, lblDepTrusteeSubCdName.Text, WW_Dummy)    '発受託人サブコード
        End If

        ' テキストボックスに数値(0～9)のみ可能とする項目のチェック
        Me.txtBigCtnCd.Attributes("onkeyPress") = "CheckNum()"
        Me.txtMiddleCtnCd.Attributes("onkeyPress") = "CheckNum()"
        Me.txtDepStation.Attributes("onkeyPress") = "CheckNum()"
        Me.txtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"
        Me.txtDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNM0014WRKINC.MAPIDS
        rightview.MAPID = LNM0014WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(txtBigCtnCd.Text)                 '大分類コード
        Master.EraseCharToIgnore(txtMiddleCtnCd.Text)              '中分類コード
        Master.EraseCharToIgnore(txtDepStation.Text)               '発駅コード
        Master.EraseCharToIgnore(txtDepTrusteeCd.Text)             '発受託人コード
        Master.EraseCharToIgnore(txtDepTrusteeSubCd.Text)          '発受託人サブコード

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_BIGCTNCD.Text = txtBigCtnCd.Text               '大分類コード
        work.WF_SEL_MIDDLECTNCD.Text = txtMiddleCtnCd.Text         '中分類コード
        work.WF_SEL_DEPSTATION.Text = txtDepStation.Text           '発駅コード
        work.WF_SEL_DEPTRUSTEECD.Text = txtDepTrusteeCd.Text       '発受託人コード
        work.WF_SEL_DEPTRUSTEESUBCD.Text = txtDepTrusteeSubCd.Text '発受託人サブコード
        ' 論理削除フラグ
        If ChkDelDataFlg.Checked = True Then
            work.WF_SEL_DELDATAFLG.Text = "1"
        Else
            work.WF_SEL_DELDATAFLG.Text = "0"
        End If

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
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

        ' 大分類コード
        Master.CheckField(Master.USERCAMP, "BIGCTNCD", txtBigCtnCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(txtBigCtnCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", txtBigCtnCd.Text, lblBigCtnCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "大分類コード : " & txtBigCtnCd.Text, needsPopUp:=True)
                    txtBigCtnCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "大分類コード", needsPopUp:=True)
            txtBigCtnCd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        ' 中分類コード
        Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", txtMiddleCtnCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(txtMiddleCtnCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", txtMiddleCtnCd.Text, lblMiddleCtnCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "中分類コード : " & txtMiddleCtnCd.Text, needsPopUp:=True)
                    txtMiddleCtnCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "中分類コード", needsPopUp:=True)
            txtMiddleCtnCd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        ' 発駅コード
        Master.CheckField(Master.USERCAMP, "DEPSTATION", txtDepStation.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(txtDepStation.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発駅コード : " & txtDepStation.Text, needsPopUp:=True)
                    txtDepStation.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発駅コード", needsPopUp:=True)
            txtDepStation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        ' 発受託人コード
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", txtDepTrusteeCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(txtDepTrusteeCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEECD", txtDepTrusteeCd.Text, lblDepTrusteeCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発受託人コード : " & txtDepTrusteeCd.Text, needsPopUp:=True)
                    txtDepTrusteeCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発受託人コード", needsPopUp:=True)
            txtDepTrusteeCd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        ' 発受託人サブコード
        Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(txtDepTrusteeSubCd.Text) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, lblDepTrusteeSubCdName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "発受託人サブコード : " & txtDepTrusteeSubCd.Text, needsPopUp:=True)
                    txtDepTrusteeSubCd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "発受託人サブコード", needsPopUp:=True)
            txtDepTrusteeSubCd.Focus()
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
        Master.TransitionPrevPage(, LNM0014WRKINC.TITLEKBNS)

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
                    Case "txtBigCtnCd"            '大分類コード
                        WW_prmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    Case "txtMiddleCtnCd"         '中分類コード
                        WW_prmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, txtBigCtnCd.Text)
                    Case "txtDepStation"          '発駅コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspStationSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "txtDepTrusteeCd"        '発受託人コード
                        WW_prmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtDepStation.Text)
                    Case "txtDepTrusteeSubCd"     '発受託人サブコード
                        WW_prmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtDepStation.Text, txtDepTrusteeCd.Text)
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
            Case "txtBigCtnCd"       '大分類コード
                CODENAME_get("BIGCTNCD", txtBigCtnCd.Text, lblBigCtnCdName.Text, WW_RtnSW)
                txtBigCtnCd.Focus()
            Case "txtMiddleCtnCd"    '中分類コード
                CODENAME_get("MIDDLECTNCD", txtMiddleCtnCd.Text, lblMiddleCtnCdName.Text, WW_RtnSW)
                txtMiddleCtnCd.Focus()
            Case "txtDepStation"          '発駅コード
                CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationName.Text, WW_Dummy)
                txtDepStation.Focus()
            Case "txtDepTrusteeCd"        '発受託人コード
                CODENAME_get("DEPTRUSTEECD", txtDepTrusteeCd.Text, lblDepTrusteeCdName.Text, WW_Dummy)
                txtDepTrusteeCd.Focus()
            Case "txtDepTrusteeSubCd"     '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, lblDepTrusteeSubCdName.Text, WW_Dummy)
                txtDepTrusteeSubCd.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

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
        Me.txtDepStation.Text = selData("STATION").ToString
        Me.lblDepStationName.Text = selData("NAMES").ToString
        Me.txtDepStation.Focus()

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


        Dim WW_Test As Integer = 0

        WW_Test.ToString()

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "txtBigCtnCd"              '大分類コード
                txtBigCtnCd.Text = WW_SelectValue
                lblBigCtnCdName.Text = WW_SelectText
                txtBigCtnCd.Focus()
            Case "txtMiddleCtnCd"           '中分類コード
                txtMiddleCtnCd.Text = WW_SelectValue
                lblMiddleCtnCdName.Text = WW_SelectText
                txtMiddleCtnCd.Focus()
            Case "txtDepStation"            '発駅コード
                txtDepStation.Text = WW_SelectValue
                lblDepStationName.Text = WW_SelectText
                txtDepStation.Focus()
            Case "txtDepTrusteeCd"          '発受託人コード
                txtDepTrusteeCd.Text = WW_SelectValue
                lblDepTrusteeCdName.Text = WW_SelectText
                txtDepTrusteeCd.Focus()
            Case "txtDepTrusteeSubCd"       '発受託人サブコード
                txtDepTrusteeSubCd.Text = WW_SelectValue
                lblDepTrusteeSubCdName.Text = WW_SelectText
                txtDepTrusteeSubCd.Focus()
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
            Case "txtBigCtnCd"                       '大分類コード
                txtBigCtnCd.Focus()
            Case "txtMiddleCtnCd"                    '中分類コード
                txtMiddleCtnCd.Focus()
            Case "txtDepStation"                     '発駅コード
                txtDepStation.Focus()
            Case "txtDepTrusteeCd"                   '発受託人コード
                txtDepTrusteeCd.Focus()
            Case "txtDepTrusteeSubCd"                '発受託人サブコード
                txtDepTrusteeSubCd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
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
        Dim WW_prmData As New Hashtable

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        WW_prmData.Item(C_PARAMETERS.LP_BIGCTNCD) = txtBigCtnCd.Text

        Try
            Select Case I_FIELD
                Case "BIGCTNCD"            '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"         '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, txtBigCtnCd.Text))
                Case "DEPSTATION"          '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"        '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtDepStation.Text))
                Case "DEPTRUSTEESUBCD"     '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtDepStation.Text, txtDepTrusteeCd.Text))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
