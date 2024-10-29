''************************************************************
' ガイダンスマスタメンテ検索画面
' 作成日 2022/02/28
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/02/28 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' ガイダンスマスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNS0008GuidanceSearch
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
        Master.MAPID = LNS0008WRKINC.MAPIDS

        TxtFromYmd.Focus()
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
            Master.GetFirstValue(Master.USERCAMP, "FROMYMD", TxtFromYmd.Text)  '掲載開始日
            Master.GetFirstValue(Master.USERCAMP, "ENDYMD", TxtEndYmd.Text)    '掲載終了日
            ' チェックリスト(対象フラグ)の初期値を設定
            Dim chklList = LNS0008WRKINC.GetNewDisplayFlags()                  '対象フラグList
            If chklList IsNot Nothing AndAlso chklList.Count <> 0 Then
                chklList = (From itm In chklList Order By itm.DispOrder).ToList
            End If
            work.WF_SEL_DISPFLAGS_LIST.Text = work.EncodeDisplayFlags(chklList)
            ChklFlags.DataSource = chklList
            ChklFlags.DataTextField = "DispName"
            ChklFlags.DataValueField = "FieldName"
            ChklFlags.DataBind()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0008L Then
            ' 実行画面からの遷移
            TxtFromYmd.Text = work.WF_SEL_FROMYMD.Text                               '掲載開始日
            TxtEndYmd.Text = work.WF_SEL_ENDYMD.Text                                 '掲載終了日
            Dim chklList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)  '対象フラグList(選択内容)
            ChklFlags.DataSource = chklList
            ChklFlags.DataTextField = "DispName"
            ChklFlags.DataValueField = "FieldName"
            ChklFlags.DataBind()
            ' 論理削除フラグ
            If work.WF_SEL_DELDATAFLG.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
        End If

        ' 有効年月日(開始)・有効年月日(終了)を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        TxtFromYmd.Attributes("onkeyPress") = "CheckCalendar()"  '掲載開始日
        TxtEndYmd.Attributes("onkeyPress") = "CheckCalendar()"   '掲載終了日

        '○ RightBox情報設定
        rightview.MAPIDS = LNS0008WRKINC.MAPIDS
        rightview.MAPID = LNS0008WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(TxtFromYmd.Text)  '掲載開始日
        Master.EraseCharToIgnore(TxtEndYmd.Text)   '掲載終了日

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_FROMYMD.Text = TxtFromYmd.Text                               '掲載開始日
        work.WF_SEL_ENDYMD.Text = TxtEndYmd.Text                                 '掲載終了日
        ' 受け渡し用にエンコード
        Dim chklList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)
        ' チェックボックスの状態をリストに設定
        chklList = work.SetSelectedDispFlags(ChklFlags, chklList)
        work.WF_SEL_DISPFLAGS_LIST.Text = work.EncodeDisplayFlags(chklList)      '対象フラグList(選択内容)
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
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_StrDate As Date
        Dim WW_EndDate As Date

        ' 掲載開始日
        Master.CheckField(Master.USERCAMP, "FROMYMD", TxtFromYmd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtFromYmd.Text) Then
                TxtFromYmd.Text = CDate(TxtFromYmd.Text).ToString("yyyy/MM/dd")
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "掲載開始日", needsPopUp:=True)
            TxtFromYmd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 掲載終了日
        Master.CheckField(Master.USERCAMP, "ENDYMD", TxtEndYmd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(TxtEndYmd.Text) Then
                TxtEndYmd.Text = CDate(TxtEndYmd.Text).ToString("yyyy/MM/dd")
            End If
        Else
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "掲載終了日", needsPopUp:=True)
            TxtEndYmd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 日付大小チェック
        If Not String.IsNullOrEmpty(TxtFromYmd.Text) AndAlso Not String.IsNullOrEmpty(TxtEndYmd.Text) Then
            Try
                Date.TryParse(TxtFromYmd.Text, WW_StrDate)
                Date.TryParse(TxtEndYmd.Text, WW_EndDate)

                If WW_StrDate > WW_EndDate Then
                    Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    TxtFromYmd.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtFromYmd.Text & ":" & TxtEndYmd.Text)
                TxtFromYmd.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
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
        Master.TransitionPrevPage(, LNS0008WRKINC.TITLEKBNS)

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
                ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                Select Case WF_FIELD.Value
                    Case "TxtFromYmd"         '掲載開始日
                        .WF_Calendar.Text = TxtFromYmd.Text
                    Case "TxtEndYmd"        '掲載終了日
                        .WF_Calendar.Text = TxtEndYmd.Text
                End Select
                .ActiveCalendar()
            End With
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
            Case "TxtFromYmd"             '掲載開始日
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        TxtFromYmd.Text = ""
                    Else
                        TxtFromYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtFromYmd.Focus()
            Case "TxtEndYmd"            '掲載終了日
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        TxtEndYmd.Text = ""
                    Else
                        TxtEndYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception

                End Try
                TxtEndYmd.Focus()
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
            Case "TxtFromYmd"             '掲載開始日
                TxtFromYmd.Focus()
            Case "TxtEndYmd"            '掲載終了日
                TxtEndYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' チェックボックスデータバインド時イベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>ChklFlags.DataBind()時にチェックの状態を設定する</remarks>
    Private Sub chklFlags_DataBound(sender As Object, e As EventArgs) Handles ChklFlags.DataBound
        Dim chklObj As CheckBoxList = DirectCast(sender, CheckBoxList)
        Dim chkBindItm As List(Of LNS0008WRKINC.DisplayFlag) = DirectCast(chklObj.DataSource, List(Of LNS0008WRKINC.DisplayFlag))
        For i = 0 To chklObj.Items.Count - 1 Step 1
            chklObj.Items(i).Selected = chkBindItm(i).Checked
        Next
    End Sub

End Class
