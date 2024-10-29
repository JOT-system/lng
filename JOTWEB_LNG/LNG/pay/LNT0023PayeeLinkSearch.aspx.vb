''************************************************************
' 支払先マスタメンテ検索画面
' 作成日 2024/05/15
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴:2024/05/15 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 支払先マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNT0023PayeeLinkSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    ''' <summary>
    ''' 名称格納Hashtable
    ''' </summary>
    Private WW_TORINAMEht As Hashtable '会社名格納HT
    Private WW_CLIENTNAMEht As Hashtable '顧客名格納HT

    ''' <summary>
    ''' 共通関数宣言(BASEDLL)
    ''' </summary>
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

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
                    Case "mspClientSingleRowSelected"  '[共通]顧客選択ポップアップで行選択
                        RowSelected_mspClientSingle()
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
        Master.MAPID = LNT0023WRKINC.MAPIDS

        TxtToriCode.Focus()
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
            TxtToriCode.Text = ""         '支払先コード
            TxtClientCode.Text = ""         '顧客コード

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0023L Then
            ' 実行画面からの遷移
            TxtToriCode.Text = work.WF_SEL_TORICODE_S.Text            '支払先コード
            TxtClientCode.Text = work.WF_SEL_CLIENTCODE_S.Text            '顧客コード
            ' 論理削除フラグ
            If work.WF_SEL_DELFLG_S.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理
            CODENAME_get("TORICODE", TxtToriCode.Text, LblToriCodeName.Text, WW_Dummy)            '支払先コード
            CODENAME_get("CLIENTCODE", TxtClientCode.Text, LblClientCodeName.Text, WW_Dummy)            '顧客コード

        End If

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtToriCode.Attributes("onkeyPress") = "CheckNum()"               '支払先コード
        ' 入力するテキストボックスは数値(0～9、ハイフン(-))のみ可能とする。
        Me.TxtClientCode.Attributes("onkeyPress") = "CheckTel()"               '顧客コード

        '○ RightBox情報設定
        rightview.MAPIDS = LNT0023WRKINC.MAPIDS
        rightview.MAPID = LNT0023WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(TxtToriCode.Text)         '支払先コード
        Master.EraseCharToIgnore(TxtClientCode.Text)         '顧客コード

        '○ チェック処理
        'WW_Check(WW_ErrSW)
        'If WW_ErrSW = "ERR" Then
        '    Exit Sub
        'End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_TORICODE_S.Text = TxtToriCode.Text                '支払先コード
        work.WF_SEL_CLIENTCODE_S.Text = TxtClientCode.Text                '顧客コード
        ' 削除フラグ
        If ChkDelDataFlg.Checked = True Then
            work.WF_SEL_DELFLG_S.Text = "1"
        Else
            work.WF_SEL_DELFLG_S.Text = "0"
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ メニュー画面遷移
        Master.TransitionPrevPage(, LNT0023WRKINC.TITLEKBNS)

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
                Select Case WF_FIELD.Value
                    Case "TxtToriCode",       '支払先コード
                         "TxtClientCode"        '顧客コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspClientSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub

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
            Case "TxtToriCode"               '支払先コード
                CODENAME_get("TORICODE", TxtToriCode.Text, LblToriCodeName.Text, WW_Dummy)
                TxtToriCode.Focus()
            Case "TxtClientCode"             '顧客コード
                CODENAME_get("CLIENTCODE", TxtClientCode.Text, LblClientCodeName.Text, WW_Dummy)
                TxtClientCode.Focus()

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

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "TxtToriCode"               '支払先コード
                TxtToriCode.Focus()
            Case "TxtClientCode"               '顧客コード
                TxtClientCode.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 顧客検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspClientSingle()

        Me.mspClientSingle.InitPopUp()
        Me.mspClientSingle.SelectionMode = ListSelectionMode.Single
        Me.mspClientSingle.SQL = CmnSearchSQL.GetClientSQL(TxtToriCode.Text, TxtClientCode.Text)

        Me.mspClientSingle.KeyFieldName = "KEYCODE"
        Me.mspClientSingle.DispFieldList.AddRange(CmnSearchSQL.GetClientTitle)

        Me.mspClientSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 顧客選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspClientSingle()

        Dim selData = Me.mspClientSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtToriCode.ID, TxtClientCode.ID
                Me.TxtToriCode.Text = selData("TORICODE").ToString
                Me.LblToriCodeName.Text = selData("TORINAME").ToString
                Me.TxtClientCode.Text = selData("CLIENTCODE").ToString
                Me.LblClientCodeName.Text = selData("CLIENTNAME").ToString

                Me.TxtToriCode.Focus()

        End Select

        'ポップアップの非表示
        Me.mspClientSingle.HidePopUp()

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
        Dim WW_TORINAMEht = New Hashtable '会社名格納HT
        Dim WW_CLIENTNAMEht = New Hashtable '顧客名格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            work.CODENAMEGetPAYEE(SQLcon, WW_TORINAMEht, WW_CLIENTNAMEht)
        End Using

        Try
            Select Case I_FIELD
                'Case "STATION"            '発駅コード・着駅コード
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "TORICODE"            '支払先コード
                    If WW_TORINAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_TORINAMEht(I_VALUE)
                        O_RTN = C_MESSAGE_NO.NORMAL
                    End If
                Case "CLIENTCODE"            '顧客コード
                    If WW_CLIENTNAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_CLIENTNAMEht(I_VALUE)
                        O_RTN = C_MESSAGE_NO.NORMAL
                    End If

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
