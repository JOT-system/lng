'Option Strict On
'Option Explicit On

Imports JOTWEB_LNG.GRIS0005LeftBox
''' <summary>
''' 受注検索画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0001OrderSearch
    Inherits System.Web.UI.Page

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                  '検索ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '戻るボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        WF_FIELD_Change()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case "WF_RIGHT_VIEW_DBClick"        '右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
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

        '初期フォーカス
        TxtDateStart.Focus()
        '値の初期化
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        'MAPID
        Master.MAPID = LNT0001WRKINC.MAPIDS
        '一覧表示
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            '会社コード
            Me.WF_CAMPCODE.Text = Master.USERCAMP
            '年月日(発送日From(検索用))
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATESTART", TxtDateStart.Text)
            '年月日(発送日To(検索用))
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATEEND", TxtDateEnd.Text)
            '所管部
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "JURISDICTIONCD", TxtJurisdictionCd.Text)
            'JOT発店所
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "JOTDEPBRANCHCD", TxtJotdepbranchCd.Text)
            '積空区分
            RdBStack.Checked = True
            RdBFree.Checked = False
            '発駅コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepStation.Text)
            '発受託人
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DEPTRUSTEECD", TxtDepTrusteeCd.Text)
            'コンテナ記号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CTNTYPE", TxtCtnType.Text)
            'コンテナ番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CTNNO", TxtCtnNo.Text)
            '状態
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STATUS", TxtStatus.Text)

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001L Then   '一覧画面からの遷移
            '〇画面項目設定処理
            '会社コード
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '年月日(積込日From(検索用))
            TxtDateStart.Text = work.WF_SEL_DATE.Text
            '年月日(積込日To(検索用))
            TxtDateEnd.Text = work.WF_SEL_DATE_TO.Text
            '所管部
            TxtJurisdictionCd.Text = work.WF_SEL_JURISDICTIONCDMAP.Text
            'JOT発店所
            TxtJotdepbranchCd.Text = work.WF_SEL_JOTDEPBRANCHCDMAP.Text

            '積空区分
            If work.WF_SEL_STACKFREEKBNCD.Text = "1" Then
                Me.RdBStack.Checked = True
            Else
                Me.RdBFree.Checked = True
            End If

            '発駅コード
            TxtDepStation.Text = work.WF_SEL_DEPSTATIONMAP.Text
            '発受託人
            TxtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECDMAP.Text
            'コンテナ記号
            TxtCtnType.Text = work.WF_SEL_CTNTYPE.Text
            'コンテナ番号
            TxtCtnNo.Text = work.WF_SEL_CTNNO.Text
            '状態
            TxtStatus.Text = work.WF_SEL_STATUSCODE.Text
            '受注キャンセルフラグ
            If work.WF_SEL_ORDERCANCELFLG.Text = "1" Then
                Me.ChkOrderCancelFlg.Checked = True
            Else
                Me.ChkOrderCancelFlg.Checked = False
            End If
            '対象外フラグ
            If work.WF_SEL_NOTSELFLG.Text = "1" Then
                Me.ChkNotSelFlg.Checked = True
            Else
                Me.ChkNotSelFlg.Checked = False
            End If
        End If

        'テキストボックスは数値(0～9)＋記号(/)のみ可能とする
        '発発送日
        Me.TxtDateStart.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtDateEnd.Attributes("onkeyPress") = "CheckCalendar()"

        'テキストボックスは数値(0～9)のみ可能とする。
        '所管部
        Me.TxtJurisdictionCd.Attributes("onkeyPress") = "CheckNum()"
        'JOT発店所
        Me.TxtJotdepbranchCd.Attributes("onkeyPress") = "CheckNum()"
        '発駅コード
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"
        '発受託人
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"
        'コンテナ記号
        Me.TxtCtnType.Attributes("onkeyPress") = "CheckNumAZ()"
        'コンテナ番号
        Me.TxtCtnNo.Attributes("onkeyPress") = "CheckNum()"
        '状態
        Me.TxtStatus.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNT0001WRKINC.MAPIDS
        rightview.MAPID = LNT0001WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '所管部
        CODENAME_get("JURISDICTIONCD", TxtJurisdictionCd.Text, LblJurisdictionName.Text, WW_DUMMY)
        'JOT発店所
        CODENAME_get("JOTDEPBRANCHCD", TxtJotdepbranchCd.Text, LblJotdepbranchName.Text, WW_DUMMY)
        '発駅コード 
        CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStation.Text, WW_DUMMY)
        '発受託人
        CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCd.Text, WW_DUMMY)
        'コンテナ記号 
        CODENAME_get("CTNTYPE", TxtCtnType.Text, TxtCtnType.Text, WW_DUMMY)
        'コンテナ番号
        CODENAME_get("CTNNO", TxtCtnNo.Text, TxtCtnNo.Text, WW_DUMMY)
        '状態
        CODENAME_get("STATUS", TxtStatus.Text, LblStatusName.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        '年月日(積込日From(検索用))
        Master.EraseCharToIgnore(TxtDateStart.Text)
        '年月日(積込日To(検索用))
        Master.EraseCharToIgnore(TxtDateEnd.Text)
        '所管部
        Master.EraseCharToIgnore(TxtJurisdictionCd.Text)
        'JOT発店所
        Master.EraseCharToIgnore(TxtJotdepbranchCd.Text)
        '発駅コード 
        Master.EraseCharToIgnore(TxtDepStation.Text)
        '発受託人
        Master.EraseCharToIgnore(TxtDepTrusteeCd.Text)
        'コンテナ記号
        Master.EraseCharToIgnore(TxtCtnType.Text)
        'コンテナ番号
        Master.EraseCharToIgnore(TxtCtnNo.Text)
        '状態
        Master.EraseCharToIgnore(TxtStatus.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '年月日(積込日From(検索用))
        work.WF_SEL_DATE.Text = TxtDateStart.Text
        '年月日(積込日To(検索用))
        work.WF_SEL_DATE_TO.Text = TxtDateEnd.Text
        '所管部
        work.WF_SEL_JURISDICTIONCDMAP.Text = TxtJurisdictionCd.Text
        work.WF_SEL_JURISDICTIONCD.Text = TxtJurisdictionCd.Text
        work.WF_SEL_JURISDICTIONNM.Text = LblJurisdictionName.Text

        '積空区分
        If Me.RdBStack.Checked = True Then
            work.WF_SEL_STACKFREEKBNCD.Text = "1"
        Else
            work.WF_SEL_STACKFREEKBNCD.Text = "2"
        End If

        'JOT発店所
        work.WF_SEL_JOTDEPBRANCHCDMAP.Text = TxtJotdepbranchCd.Text
        work.WF_SEL_JOTDEPBRANCHCD.Text = TxtJotdepbranchCd.Text
        work.WF_SEL_JOTDEPBRANCHNM.Text = LblJotdepbranchName.Text
        '発駅コード
        work.WF_SEL_DEPSTATIONMAP.Text = TxtDepStation.Text
        work.WF_SEL_DEPSTATION.Text = TxtDepStation.Text
        work.WF_SEL_DEPSTATIONNM.Text = LblDepStation.Text
        '発受託人
        work.WF_SEL_DEPTRUSTEECDMAP.Text = TxtDepTrusteeCd.Text
        work.WF_SEL_DEPTRUSTEECD.Text = TxtDepTrusteeCd.Text
        work.WF_SEL_DEPTRUSTEENM.Text = LblDepTrusteeCd.Text
        'コンテナ記号
        work.WF_SEL_CTNTYPE.Text = TxtCtnType.Text
        'コンテナ番号
        work.WF_SEL_CTNNO.Text = TxtCtnNo.Text
        '状態
        work.WF_SEL_STATUSCODE.Text = TxtStatus.Text
        work.WF_SEL_STATUS.Text = LblStatusName.Text

        '受注キャンセルフラグ
        If Me.ChkOrderCancelFlg.Checked = True Then
            work.WF_SEL_ORDERCANCELFLG.Text = "1"
        Else
            work.WF_SEL_ORDERCANCELFLG.Text = "0"
        End If

        '対象外フラグ
        If Me.ChkNotSelFlg.Checked = True Then
            work.WF_SEL_NOTSELFLG.Text = "1"
        Else
            work.WF_SEL_NOTSELFLG.Text = "0"
        End If

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
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
        Dim WW_TEXT As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '年月日(発送日From)
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", TxtDateStart.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDateStart.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発送日From", needsPopUp:=True)
            TxtDateStart.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '年月日(発送日To)
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", TxtDateEnd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDateEnd.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発送日To", needsPopUp:=True)
            TxtDateEnd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '所管部
        If TxtJurisdictionCd.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "JURISDICTIONCD", TxtJurisdictionCd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "所管部", needsPopUp:=True)
                TxtJurisdictionCd.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        'JOT発店所
        If TxtJotdepbranchCd.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "JOTDEPBRANCHCD", TxtJotdepbranchCd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "JOT発店所", needsPopUp:=True)
                TxtJotdepbranchCd.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '発駅コード
        If TxtDepStation.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "DEPSTATION", TxtDepStation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発駅コード", needsPopUp:=True)
                TxtDepStation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '発受託人
        If TxtDepTrusteeCd.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "DEPTRUSTEECD", TxtDepTrusteeCd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発受託人", needsPopUp:=True)
                TxtDepTrusteeCd.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        'コンテナ記号
        If TxtCtnType.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "CTNTYPE", TxtCtnType.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "コンテナ記号", needsPopUp:=True)
                TxtCtnType.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        'コンテナ番号
        If TxtCtnNo.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "CTNNO", TxtCtnNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                TxtCtnNo.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '状態
        If TxtStatus.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "STATUS", TxtStatus.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                TxtStatus.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage(, LNT0001WRKINC.TITLEKBNS)

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
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim strPrmCompanyCd As String = work.WF_SEL_CAMPCODE.Text
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = strPrmCompanyCd

                    '所管部
                    If WF_FIELD.Value = "TxtJurisdictionCd" Then
                        prmData = work.CreateJURISDICTIONParam(strPrmCompanyCd, TxtJurisdictionCd.Text)
                    End If

                    '発受託人
                    If WF_FIELD.Value = "TxtDepTrusteeCd" Then
                        Dim strStation As String = ""
                        Dim strDepTrusTee As String = ""

                        strStation = Me.TxtDepStation.Text
                        strDepTrusTee = Me.TxtDepTrusteeCd.Text

                        '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                        prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    End If

                    'コンテナ番号
                    If WF_FIELD.Value = "TxtCtnNo" Then
                        '〇 画面(コンテナ記号).テキストボックスで設定した値で絞る
                        prmData = work.CreateCTNNOParam(Me.TxtCtnType.Text, Me.TxtCtnNo.Text)
                    End If

                    '左リストボックス設定処理
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    '一覧表示
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "TxtDateStart"
                            .WF_Calendar.Text = TxtDateStart.Text
                        Case "TxtDateEnd"
                            .WF_Calendar.Text = TxtDateEnd.Text
                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '所管部
            Case "TxtJurisdictionCd"
                CODENAME_get("JURISDICTIONCD", TxtJurisdictionCd.Text, LblJurisdictionName.Text, WW_RTN_SW)
            'JOT発店所
            Case "TxtJotdepbranchCd"
                CODENAME_get("JOTDEPBRANCHCD", TxtJotdepbranchCd.Text, LblJotdepbranchName.Text, WW_RTN_SW)
            '発駅コード
            Case "TxtDepStation"
                CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStation.Text, WW_RTN_SW)
            '発受託人
            Case "TxtDepTrusteeCd"
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCd.Text, WW_RTN_SW)
            'コンテナ記号 
            Case "TxtCtnType"
                CODENAME_get("CTNTYPE", TxtCtnType.Text, TxtCtnType.Text, WW_RTN_SW)
            'コンテナ番号
            Case "TxtCtnNo"
                CODENAME_get("CTNNO", TxtCtnNo.Text, TxtCtnNo.Text, WW_RTN_SW)
            '状態
            Case "TxtStatus"
                CODENAME_get("STATUS", TxtStatus.Text, LblStatusName.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

            '会社コード
            Case "WF_CAMPCODE"
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            '発送日From
            Case "TxtDateStart"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtDateStart.Text = ""
                    Else
                        TxtDateStart.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDateStart.Focus()

            '発送日To
            Case "TxtDateEnd"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtDateEnd.Text = ""
                    Else
                        Me.TxtDateEnd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtDateEnd.Focus()

            '所管部
            Case "TxtJurisdictionCd"
                TxtJurisdictionCd.Text = WW_SelectValue
                LblJurisdictionName.Text = WW_SelectText
                TxtJurisdictionCd.Focus()

            'JOT発店所
            Case "TxtJotdepbranchCd"
                TxtJotdepbranchCd.Text = WW_SelectValue
                LblJotdepbranchName.Text = WW_SelectText
                TxtJotdepbranchCd.Focus()

            '発駅コード
            Case "TxtDepStation"
                TxtDepStation.Text = WW_SelectValue
                LblDepStation.Text = WW_SelectText
                TxtDepStation.Focus()

            '発受託人
            Case "TxtDepTrusteeCd"
                TxtDepTrusteeCd.Text = WW_SelectValue
                LblDepTrusteeCd.Text = WW_SelectText
                TxtDepTrusteeCd.Focus()

            'コンテナ記号
            Case "TxtCtnType"
                Me.TxtCtnType.Text = WW_SelectValue
                Me.TxtCtnType.Focus()

            'コンテナ番号
            Case "TxtCtnNo"
                Me.TxtCtnNo.Text = WW_SelectValue
                Me.TxtCtnNo.Focus()

            '状態
            Case "TxtStatus"
                TxtStatus.Text = WW_SelectValue
                LblStatusName.Text = WW_SelectText
                TxtStatus.Focus()

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "TxtDateStart"         '年月日(積込日From)
                TxtDateStart.Focus()
            Case "TxtDateEnd"           '年月日(積込日To)
                TxtDateEnd.Focus()
            Case "TxtJurisdictionCd"    '所管部
                TxtJurisdictionCd.Focus()
            Case "TxtJotdepbranchCd"    'JOT発店所
                TxtJotdepbranchCd.Focus()
            Case "TxtDepStation"        '発駅コード
                TxtDepStation.Focus()
            Case "TxtDepTrusteeCd"      '発受託人
                TxtDepTrusteeCd.Focus()
            Case "TxtCtnType"           'コンテナ記号
                TxtCtnType.Focus()
            Case "TxtCtnNo"             'コンテナ番号
                TxtCtnNo.Focus()
            Case "TxtStatus"            '状態
                TxtStatus.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)

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

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD

                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "JURISDICTIONCD"   '所管部
                    prmData = work.CreateJURISDICTIONParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JURISDICTION, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "JOTDEPBRANCHCD"   'JOT発店所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOTDEPBRANCH, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DEPSTATION"       '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPSTATION, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DEPTRUSTEECD"     '発受託人
                    Dim strStation As String = ""
                    Dim strDepTrusTee As String = ""

                    strStation = Me.TxtDepStation.Text
                    strDepTrusTee = Me.TxtDepTrusteeCd.Text

                    '〇 一覧(発受託人).テキストボックスで設定した値で絞る
                    prmData = work.CreateDEPTRUSTEEParam(strStation, strDepTrusTee)
                    '名称取得
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPTRUSTEECD, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "CTNTYPE"          'コンテナ記号 
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CTNTYPE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "CTNNO"            'コンテナ番号
                    prmData = work.CreateCTNNOParam(Me.TxtCtnType.Text, Me.TxtCtnNo.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CTNNO, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "STATUS"           '状態
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class