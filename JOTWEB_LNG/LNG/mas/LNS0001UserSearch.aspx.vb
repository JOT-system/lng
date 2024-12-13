﻿''************************************************************
' ユーザーマスタメンテ検索画面
' 作成日 2024/12/02
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/02 新規作成
'          : 
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' ユーザマスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNS0001UserSearch
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
        Master.MAPID = LNS0001WRKINC.MAPIDS

        TxtStYMDCode.Focus()
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
            Master.GetFirstValue(Master.USERCAMP, "STYMD", TxtStYMDCode.Text)  '有効年月日(From)
            TxtStYMDCode.Text = TxtStYMDCode.Text.ToString
            TxtEndYMDCode.Text = ""                                            '有効年月日(To)
            TxtOrgCode.Text = ""                                               '組織コード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0001L Then
            ' 実行画面からの遷移
            TxtStYMDCode.Text = work.WF_SEL_STYMD.Text    '有効年月日(From)
            TxtEndYMDCode.Text = work.WF_SEL_ENDYMD.Text  '有効年月日(To)
            TxtOrgCode.Text = work.WF_SEL_ORG.Text        '組織コード
            ' 論理削除フラグ
            If work.WF_SEL_DELDATAFLG.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理
            CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)  '組織コード
        End If
        Master.GetFirstValue(Master.USERCAMP, "CAMPCODE", TxtCampCode.Text)  '会社コード

        ' 会社コード・組織コードを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"

        ' 有効年月日(開始)・有効年月日(終了)を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtStYMDCode.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtEndYMDCode.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNS0001WRKINC.MAPIDS
        rightview.MAPID = LNS0001WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(TxtStYMDCode.Text)             '有効年月日(From)
        Master.EraseCharToIgnore(TxtEndYMDCode.Text)            '有効年月日(To)
        Master.EraseCharToIgnore(TxtOrgCode.Text)               '組織コード

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_STYMD.Text = TxtStYMDCode.Text.ToString     '有効年月日(From)
        work.WF_SEL_ENDYMD.Text = TxtEndYMDCode.Text.ToString   '有効年月日(To)
        work.WF_SEL_ORG.Text = TxtOrgCode.Text                  '組織コード
        ' 論理削除フラグ
        If ChkDelDataFlg.Checked = True Then
            work.WF_SEL_DELDATAFLG.Text = "1"
        Else
            work.WF_SEL_DELDATAFLG.Text = "0"
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
        Dim WW_StrDate As Date
        Dim WW_EndDate As Date

        ' 有効年月日(From)
        If TxtStYMDCode.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "有効年月日(From)", needsPopUp:=True)
            TxtStYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(Master.USERCAMP, "STYMD", TxtStYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtStYMDCode.Text) Then
                TxtStYMDCode.Text = CDate(TxtStYMDCode.Text)
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "有効年月日(From)", needsPopUp:=True)
            TxtStYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 有効年月日(To)
        'If TxtEndYMDCode.Text = "" Then
        '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "有効年月日(To)", needsPopUp:=True)
        '    TxtStYMDCode.Focus()
        '    O_RTN = "ERR"
        '    Exit Sub
        'End If
        Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYMDCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
                TxtEndYMDCode.Text = CDate(TxtEndYMDCode.Text)
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "有効年月日(To)", needsPopUp:=True)
            TxtEndYMDCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 日付大小チェック
        If Not String.IsNullOrEmpty(TxtStYMDCode.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYMDCode.Text) Then
            Try
                Date.TryParse(TxtStYMDCode.Text, WW_StrDate)
                Date.TryParse(TxtEndYMDCode.Text, WW_EndDate)

                If WW_StrDate > WW_EndDate Then
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
        ' 組織コード
        Master.CheckField(Master.USERCAMP, "ORG", TxtOrgCode.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtOrgCode.Text) Then
                ' 名称存在チェック
                CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "組織コード : " & TxtOrgCode.Text, needsPopUp:=True)
                    TxtOrgCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "組織コード", needsPopUp:=True)
            TxtOrgCode.Focus()
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
        Master.TransitionPrevPage(, LNS0001WRKINC.TITLEKBNS)

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "TxtStYMDCode"         '有効年月日(From)
                                .WF_Calendar.Text = TxtStYMDCode.Text
                            Case "TxtEndYMDCode"        '有効年月日(To)
                                .WF_Calendar.Text = TxtEndYMDCode.Text
                        End Select
                        .ActiveCalendar()
                    Case Else
                        If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                            ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                            WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, TxtCampCode.Text)
                        Else
                            ' その他の場合、操作ユーザーの組織のみ取得
                            WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, TxtCampCode.Text)
                        End If

                        .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledChange()

        '○ 変更した項目の名称をセット
        CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_RtnSW)  '組織コード
        TxtOrgCode.Focus()

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
            Case "TxtStYMDCode"             '有効年月日(From)
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        TxtStYMDCode.Text = ""
                    Else
                        TxtStYMDCode.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtStYMDCode.Focus()
            Case "TxtEndYMDCode"            '有効年月日(To)
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        TxtEndYMDCode.Text = ""
                    Else
                        TxtEndYMDCode.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception

                End Try
                TxtEndYMDCode.Focus()
            Case "TxtOrgCode"               '組織コード
                TxtOrgCode.Text = WW_SelectValue
                LblOrgName.Text = WW_SelectText
                TxtOrgCode.Focus()
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
            Case "TxtStYMDCode"             '有効年月日(From)
                TxtStYMDCode.Focus()
            Case "TxtEndYMDCode"            '有効年月日(To)
                TxtEndYMDCode.Focus()
            Case "TxtOrgCode"               '組織コード
                TxtOrgCode.Focus()
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

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, TxtCampCode.Text))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, TxtCampCode.Text))
                    End If
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, TxtCampCode.Text))
                    Else
                        ' その他の場合、操作ユーザーの管轄組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, TxtCampCode.Text))
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class