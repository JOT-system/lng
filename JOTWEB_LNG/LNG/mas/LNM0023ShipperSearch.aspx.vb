﻿''************************************************************
' 荷主マスタメンテ検索画面
' 作成日 2022/03/03
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴:2022/03/03 新規作成
'         :
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 荷主マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNM0023ShipperSearch
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
        Master.MAPID = LNM0023WRKINC.MAPIDS

        TxtShipperCd.Focus()
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
            TxtShipperCd.Text = ""  '荷主コード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0023L Then
            ' 実行画面からの遷移
            TxtShipperCd.Text = work.WF_SEL_SHIPPERCD.Text  '荷主コード
            ' 論理削除フラグ
            If work.WF_SEL_DELDATAFLG.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理
            CODENAME_get("SHIPPER", TxtShipperCd.Text, LblShipperCdName.Text, WW_Dummy)  '荷主コード
        End If

        ' コンテナ番号を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtShipperCd.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = LNM0023WRKINC.MAPIDS
        rightview.MAPID = LNM0023WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(TxtShipperCd.Text)  '荷主コード

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_SHIPPERCD.Text = TxtShipperCd.Text  '荷主コード
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

        ' 荷主コード
        Master.CheckField(Master.USERCAMP, "SHIPPERCD", TxtShipperCd.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("SHIPPER", TxtShipperCd.Text, LblShipperCdName.Text, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "荷主コード : " & TxtShipperCd.Text, needsPopUp:=True)
                TxtShipperCd.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "荷主コード", needsPopUp:=True)
            TxtShipperCd.Focus()
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
        Master.TransitionPrevPage(, LNM0023WRKINC.TITLEKBNS)

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
            Case "TxtShipperCd"       '荷主コード
                CODENAME_get("SHIPPER", TxtShipperCd.Text, LblShipperCdName.Text, WW_RtnSW)
                TxtShipperCd.Focus()
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
            Case "TxtShipperCd"       '荷主コード
                TxtShipperCd.Text = WW_SelectValue
                LblShipperCdName.Text = WW_SelectText
                TxtShipperCd.Focus()
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
            Case "TxtShipperCd"       '荷主コード
                TxtShipperCd.Focus()
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
                Case "SHIPPER"  '荷主コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPER, I_VALUE, O_TEXT, O_RTN)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
