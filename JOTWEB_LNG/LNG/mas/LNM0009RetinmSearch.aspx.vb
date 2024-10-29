''************************************************************
' ＪＲ賃率マスタメンテ検索画面
' 作成日 2024/02/05
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴:2024/02/05 新規作成
'         :
''************************************************************
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports MySQL.Data.MySqlClient

''' <summary>
''' ＪＲ賃率マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class LNM0009RetinmSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力

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
        Master.MAPID = LNM0009WRKINC.MAPIDS

        TxtKiro.Focus()
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
            TxtKiro.Text = ""         '発駅コード

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0009L Then
            ' 実行画面からの遷移
            TxtKiro.Text = work.WF_SEL_KIRO_S.Text            'キロ程
            ' 論理削除フラグ
            If work.WF_SEL_DELFLG_S.Text = "1" Then
                ChkDelDataFlg.Checked = True
            Else
                ChkDelDataFlg.Checked = False
            End If
            '○ 名称設定処理

        End If

        '画面の選択項目設定
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'TC車種名
            If GETSELECTLIST(SQLcon, "KIRO", Me.DLISTKiro.InnerHtml) = False Then Exit Sub
            Me.TxtKiro.Attributes("list") = Me.DLISTKiro.ClientID

        End Using

        ' 入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtKiro.Attributes("onkeyPress") = "CheckNum()"               'キロ程

        '○ RightBox情報設定
        rightview.MAPIDS = LNM0009WRKINC.MAPIDS
        rightview.MAPID = LNM0009WRKINC.MAPIDL
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
        Master.EraseCharToIgnore(TxtKiro.Text)         'キロ程

        '○ チェック処理
        WW_Check(WW_ErrSW)
        If WW_ErrSW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_KIRO_S.Text = TxtKiro.Text                'キロ程
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
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        WW_Dummy = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""

        'キロ程
        Master.CheckField(Master.USERCAMP, "KIRO", TxtKiro.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "キロ程", needsPopUp:=True)
            TxtKiro.Focus()
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
        Master.TransitionPrevPage(, LNM0009WRKINC.TITLEKBNS)

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 選択候補取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GETSELECTLIST(ByVal SQLcon As MySqlConnection,
                                     ByVal I_FIELD As String,
                                     ByRef O_OPTIONS As String) As Boolean

        GETSELECTLIST = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    KIRO ")
        SQLStr.AppendLine(" FROM lng.LNM0009_RETINM ")
        SQLStr.AppendLine(" WHERE DELFLG = '0' ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      KIRO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_OPTIONS As String = ""
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)
                    If WW_Tbl.Rows.Count > 0 Then
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_OPTIONS += "<option>" + Row("KIRO").ToString + "</option>"
                        Next
                    Else
                        WW_OPTIONS += "<option>" + "　" + "</option>"
                    End If
                    O_OPTIONS = WW_OPTIONS
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0009_RETINM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0009_RETINM Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Function
        End Try
        GETSELECTLIST = True

    End Function

End Class
