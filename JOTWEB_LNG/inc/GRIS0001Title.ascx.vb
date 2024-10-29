Option Strict On
Imports MySQL.Data.MySqlClient

Public Class GRIS0001Title
    Inherits UserControl

    Private Const MENUID As String = "M00001"
    Private Const LOGONID As String = "M10000"
    Private Const SCHEDULEID As String = "MB0006"

    Private Const CONFIG_ENV_KEY As String = "Environment"
    Private Const CONFIG_ENV_TEST As String = "TEST"

    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION

    ''' <summary>
    ''' 全画面共通-タイトル設定
    ''' </summary>
    ''' <param name="I_MAPID">画面ID</param>
    ''' <param name="I_MAPVARI"></param>
    ''' <param name="I_USERCOMP">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub SetTitle(ByVal I_MAPID As String, ByVal I_MAPVARI As String, ByVal I_USERCOMP As String, ByRef O_RTN As String, Optional ByVal I_USERID As String = Nothing)

        Dim CS0017ForwardURL As New CS0017ForwardURL        '画面遷移先情報取得
        Dim GS0001CAMPget As New GS0001CAMPget              '会社情報取得
        Dim CS0015TITLEcamp As New CS0015TITLEcamp          '会社コード取得
        Dim CS0050Session As New CS0050SESSION
        Dim CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得
        Dim KankyoName As String = ""                       '環境名称

        '初期化
        O_RTN = C_MESSAGE_NO.NORMAL
        'ID、表題設定
        WF_TITLEID.Text = "画面ID: " & I_MAPID

        If CS0050Session.ENVIRONMENTFLG = "1" Then
            KankyoName = "（検証）"
        ElseIf CS0050Session.ENVIRONMENTFLG = "0" Then
            KankyoName = "（local）"
        End If

        If I_MAPID = LOGONID Then

            ' システム名称タイトル取得
            Try
                '検索SQL文
                Dim SQLStr As String =
                         "SELECT rtrim(A.VALUE1) as NAMES " _
                       & " FROM  COM.LNS0006_FIXVALUE A " _
                       & " Where  " _
                       & "       A.CAMPCODE   = '01' " _
                       & "   and A.CLASS = 'SYSTEMNAME' " _
                       & "   and A.KEYCODE = 'NAME' " _
                       & "   and A.DELFLG  <> '1' "

                'DataBase接続文字
                Using SQLcon As MySqlConnection = CS0050Session.getConnection,
                      SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.HasRows = True Then
                            While SQLdr.Read
                                WF_TITLETEXT.Text = Convert.ToString(SQLdr("NAMES")) + KankyoName
                            End While
                        Else
                            WF_TITLETEXT.Text = "業務メニュー" + KankyoName
                        End If

                    End Using

                End Using
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try

            'ID、表題設定
            WF_TITLEID.Text = "画面ID: Logon"
            WF_TITLECAMP.Text = ""
            '現在日付設定
            WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        ElseIf I_MAPID = SCHEDULEID Then
            WF_TITLEID.Text = "画面ID: MB0006"
            WF_TITLETEXT.Text = "個人スケジュール" + KankyoName
            WF_TITLECAMP.Text = ""
            '現在日付設定
            WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        ElseIf I_MAPID = MENUID Then
            If IsNothing(I_USERID) OrElse I_USERID = "INIT" Then
                Exit Sub
            End If
            Dim WW_FIND As Boolean = False
            Try
                '検索SQL文
                Dim SQLStr As String =
                         "SELECT rtrim(A.MAPNAMES) as NAMES " _
                       & " FROM  COM.LNS0009_PROFMMAP A " _
                       & " Where  " _
                       & "       A.MAPIDP   = @P1 " _
                       & "   and A.VARIANTP = @P2 " _
                       & "   and A.TITLEKBN = 'H' " _
                       & "   and A.STYMD   <= @P3 " _
                       & "   and A.ENDYMD  >= @P4 " _
                       & "   and A.DELFLG  <> @P5 "

                'DataBase接続文字
                Using SQLcon As MySqlConnection = CS0050Session.getConnection,
                      SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)

                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 50).Value = I_MAPID
                        .Add("@P2", MySqlDbType.VarChar, 50).Value = I_MAPVARI
                        .Add("@P3", MySqlDbType.Date).Value = Date.Now
                        .Add("@P4", MySqlDbType.Date).Value = Date.Now
                        .Add("@P5", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.HasRows = True Then
                            While SQLdr.Read
                                WF_TITLETEXT.Text = Convert.ToString(SQLdr("NAMES")) + KankyoName
                                WW_FIND = True
                            End While
                        Else
                            WF_TITLETEXT.Text = "業務メニュー" + KankyoName
                            WW_FIND = False
                        End If

                    End Using

                End Using
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
            If Not WW_FIND Then
                '自画面MAPID・変数より名称を取得
                CS0017ForwardURL.MAPID = I_MAPID
                CS0017ForwardURL.VARI = I_MAPVARI
                CS0017ForwardURL.getPreviusURL()
                If isNormal(CS0017ForwardURL.ERR) Then
                    '遷移先画面＝子画面　
                    If (I_MAPVARI <> C_DEFAULT_DATAKEY) Then
                        WF_TITLETEXT.Text = CS0017ForwardURL.NAMES + KankyoName
                    Else
                        WF_TITLETEXT.Text = "業務メニュー" + KankyoName
                    End If
                Else
                    Exit Sub
                End If
            End If
        Else
            If IsNothing(I_USERID) OrElse I_USERID = "INIT" Then
                Exit Sub
            End If

            '自画面MAPID・変数より名称を取得
            CS0017ForwardURL.MAPID = I_MAPID
            CS0017ForwardURL.VARI = I_MAPVARI
            CS0017ForwardURL.getPreviusURL()
            If isNormal(CS0017ForwardURL.ERR) Then
                '遷移先画面＝子画面　
                WF_TITLETEXT.Text = CS0017ForwardURL.NAMES + KankyoName
            Else
            End If
        End If

        If Not I_MAPID = LOGONID Then
            'ユーザ名
            CS0051UserInfo.USERID = I_USERID
            CS0051UserInfo.getInfo()
            If isNormal(CS0051UserInfo.ERR) Then
                WF_USERNAME.Text = "ﾛｸﾞｲﾝ :  " & CS0051UserInfo.STAFFNAMES
            Else
                O_RTN = CS0051UserInfo.ERR
                Exit Sub
            End If
        Else
            WF_USERNAME.Text = ""
        End If

        '会社設定
        'ユーザID設定されている場合はユーザIDから取得する
        If String.IsNullOrEmpty(I_USERID) OrElse Not String.IsNullOrEmpty(I_USERCOMP) Then
            GS0001CAMPget.CAMPCODE = I_USERCOMP
            GS0001CAMPget.STYMD = Date.Now
            GS0001CAMPget.ENDYMD = Date.Now
            GS0001CAMPget.GS0001CAMPget()
            If isNormal(GS0001CAMPget.ERR) Then
                WF_TITLECAMP.Text = GS0001CAMPget.NAMES
            Else
                O_RTN = GS0001CAMPget.ERR
                Exit Sub
            End If
        Else
            Dim complist As ListBox = New ListBox()
            CS0015TITLEcamp.USERID = I_USERID
            CS0015TITLEcamp.List = complist
            CS0015TITLEcamp.CS0015TITLEcamp()
            If CS0015TITLEcamp.ERR = C_MESSAGE_NO.NORMAL Then
                WF_TITLECAMP.Text = complist.SelectedItem.Text
            Else
                O_RTN = GS0001CAMPget.ERR
                Exit Sub
            End If
        End If
        '現在日付設定
        WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        'タイトル部CSS設定
        ' Web.configに[appSettings][key="Environment"]の値により設定
        Select Case ConfigurationManager.AppSettings(CONFIG_ENV_KEY)
            Case CONFIG_ENV_TEST
                titlebox.Attributes("class") = "titlebox_TEST"
            Case Else
                titlebox.Attributes("class") = "titlebox"
        End Select
    End Sub

    ''' <summary>
    ''' ヘッダー左下文言設定メソッド
    ''' </summary>
    ''' <param name="leftBottomText"></param>
    Public Sub SetLeftBottomMessage(leftBottomText As String)
        Me.lblCommonHeaderLeftBottom.Text = leftBottomText
    End Sub

    ''' <summary>
    ''' ヘッダー文言変更メソッド
    ''' </summary>
    ''' <param name="Text"></param>
    Public Sub ChgTitleText(Text As String)
        Me.WF_TITLETEXT.Text = Text
    End Sub

End Class