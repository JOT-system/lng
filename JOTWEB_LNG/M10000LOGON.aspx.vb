Option Strict On
Imports MySql.Data.MySqlClient
Imports System.Net
Imports GrapeCity.Documents.Excel


Public Class M10000LOGON
    Inherits System.Web.UI.Page

    'セッション情報
    Private CS0050Session As New CS0050SESSION

    '画面ID  
    Private Const MAPID As String = "M10000"

    Private Const C_MAX_MISS_PASSWORD_COUNT As Integer = 6      'パスワード入力失敗の最大回数
    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■　初期処理　■■■
        '共通セッション情報
        '   Class         : クラス(プロジェクト直下のクラス)
        '   Userid        : ユーザID
        '   APSRVname     : APサーバー名称
        '   Term          : 操作端末(端末操作情報として利用)

        '   DBcon         : DB接続文字列 
        '   LOGdir        : ログ出力ディレクトリ 
        '   PDFdir        : PDF用ワークのディレクトリ
        '   FILEdir       : FILE格納ディレクトリ
        '   JNLdir        : 更新ジャーナル格納ディレクトリ

        '   MAPmapid      : 画面間IF(MAPID)


        If IsPostBack Then
            PassWord.Attributes.Add("value", PassWord.Text)

            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonOK"
                        WF_ButtonOK_Click(sender, e)
                End Select
            End If
        Else
            '〇初期化処理
            Initialize()
        End If

        Master.LOGINCOMP = WF_TERMCAMP.Text
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '■■■　セッション変数設定　■■■
        Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Master.dispHelp = False

        Master.MAPID = MAPID
        '○ 固定項目設定
        CS0050Session.USERID = "INIT"
        CS0050Session.APSV_ID = "INIT"
        CS0050Session.APSV_COMPANY = "INIT"
        CS0050Session.APSV_ORG = "INIT"
        CS0050Session.SELECTED_COMPANY = "INIT"
        CS0050Session.DRIVERS = ""
        If String.IsNullOrEmpty(CS0050Session.USERID) Then
            'Else
            '    Master.SetMAPValue()
            'ログアウト判定
            InsertLogonYMDMaster("2")
        End If
        CS001INIFILE.CS0001INIFILEget()
        If Not isNormal(CS001INIFILE.ERR) Then
            Master.Output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '■■■ 初期画面表示 ■■■
        '■■■　初期メッセージ表示　■■■
        Dim WW_File As String

        For Each tempFile As String In System.IO.Directory.GetFiles(
            CS0050Session.UPLOAD_PATH & "\XML_TMP", "*", System.IO.SearchOption.AllDirectories)
            ' ファイルパスからファイル名を取得
            WW_File = tempFile
            Do
                WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 200)
            Loop Until InStr(WW_File, "\") = 0

            '本日作成以外のファイルは削除
            If Mid(WW_File, 1, 8) <> Date.Now.ToString("yyyyMMdd") Then
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            End If
        Next
        'ガイダンスエリアの表示
        Using SQLcon As MySqlConnection = CS0050Session.getConnection

            SQLcon.Open() 'DataBase接続(Open)
            Using guidDt As DataTable = GetGuidanceData(SQLcon)
                Me.repGuidance.DataSource = guidDt
                Me.repGuidance.DataBind()
            End Using
        End Using
        UserID.Focus()

    End Sub
    ''' <summary>
    '''　OKボタン押下時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOK_Click(sender As Object, e As EventArgs)

        '■■■　初期処理　■■■

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        '   Dim CS0009MESSAGEout As New CS0009MESSAGEout        'メッセージ出力 out
        'Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態
        Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み


        '○オンラインサービス判定
        '画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        '   CS0008ONLINEstat.CS0008ONLINEstat()
        '  If isNormal(CS0008ONLINEstat.ERR) Then
        'オンラインサービス停止時、ログオン画面へ遷移
        ' If CS0008ONLINEstat.ONLINESW = 0 Then Exit Sub

        'Else
        'Master.Output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
        'Exit Sub
        'End If

        '■■■　メイン処理　■■■
        '〇ID、パスワードのいずれかが未入力なら抜ける
        If String.IsNullOrEmpty(UserID.Text) OrElse String.IsNullOrEmpty(PassWord.Text) Then Exit Sub

        '○ 入力文字内の禁止文字排除
        '   画面UserID内の使用禁止文字排除
        Master.EraseCharToIgnore(UserID.Text)
        '### 20240222 START パスワードポリシー対応 ※パスワードに記号を用いるため廃止
        'Master.EraseCharToIgnore(PassWord.Text)
        '### 20240222 END   パスワードポリシー対応 ※パスワードに記号を用いるため廃止

        '○ 画面UserIDのDB(lns0001_user)存在チェック
        Dim WW_USERID As String = String.Empty
        Dim WW_PASSWORD As String = String.Empty
        Dim WW_USERCAMP As String = String.Empty
        Dim WW_ORG As String = String.Empty
        Dim WW_STYMD As Date = Date.Now
        Dim WW_ENDYMD As Date = Date.Now
        Dim WW_MISSCNT As Integer = 0
        Dim WW_UPDYMD As Date
        Dim WW_UPDTIMSTP As Date
        '20191101-追加-START
        Dim WW_MENUROLE As String = String.Empty
        Dim WW_MAPROLE As String = String.Empty
        Dim WW_VIEWPROFID As String = String.Empty
        Dim WW_RPRTPROFID As String = String.Empty
        'Dim WW_APPROVALID As String = String.Empty
        '20191101-追加-END
        Dim WW_MAPID As String = String.Empty
        Dim WW_VARIANT As String = String.Empty
        Dim WW_PASSENDYMD As String = String.Empty
        Dim WW_err As String = String.Empty
        Dim WW_RTN As String = String.Empty
        Dim WW_LOGONYMD As String = Date.Now.ToString("yyyy/MM/dd")
        Dim WW_URL As String = String.Empty
        Dim WW_MENUURL As String = String.Empty
        Dim WW_chk As String = String.Empty
        'Userメニューリスト設定
        Dim WW_UserMenuList As New List(Of CS0050SESSION.UserMenuCostomItem)


        'セッションアウト後の再INIファイル読取り
        CS001INIFILE.CS0001INIFILEget()
        If Not isNormal(CS001INIFILE.ERR) Then
            Master.Output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        'DataBase接続文字
        Using SQLcon As MySqlConnection = CS0050Session.getConnection

            SQLcon.Open() 'DataBase接続(Open)

            ' パスワード　証明書オープン
            'Try
            '    Dim SQLOpen_Str As String = "OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotctn"
            '    Using SQLOpencmd As New MySqlCommand(SQLOpen_Str, SQLcon)
            '        SQLOpencmd.ExecuteNonQuery()
            '    End Using

            'Catch ex As Exception
            '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "lns0002_userpass OPEN")
            '    CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
            '    CS0011LOGWRITE.INFPOSI = "lns0002_userpass OPEN"                           '
            '    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            '    CS0011LOGWRITE.TEXT = ex.ToString()
            '    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
            '    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            '    Exit Sub
            'End Try

            Try
                'lns0001_user検索SQL文
                Dim sqlStat As New StringBuilder
                sqlStat.AppendLine("SELECT rtrim(A.USERID)   as USERID")
                sqlStat.AppendLine("      ,rtrim(A.CAMPCODE) as CAMPCODE")
                sqlStat.AppendLine("      ,rtrim(A.ORG)      as ORG")
                sqlStat.AppendLine("      ,A.STYMD")
                sqlStat.AppendLine("      ,A.ENDYMD")
                sqlStat.AppendLine("      ,CAST(AES_DECRYPT(PASSWORD, 'loginpasskey') AS CHAR) as PASSWORD")
                sqlStat.AppendLine("      ,B.MISSCNT")
                sqlStat.AppendLine("      ,A.INITYMD")
                sqlStat.AppendLine("      ,A.UPDYMD")
                sqlStat.AppendLine("      ,A.UPDTIMSTP")
                sqlStat.AppendLine("      ,rtrim(A.MENUROLE)   as MENUROLE")
                sqlStat.AppendLine("      ,rtrim(A.MAPROLE)    as MAPROLE")
                sqlStat.AppendLine("      ,rtrim(A.VIEWPROFID) as VIEWPROFID")
                sqlStat.AppendLine("      ,rtrim(A.RPRTPROFID) as RPRTPROFID")
                sqlStat.AppendLine("      ,rtrim(A.MAPID)      as MAPID")
                sqlStat.AppendLine("      ,rtrim(A.VARIANT)    as VARIANT")
                'sqlStat.AppendLine("      ,rtrim(A.APPROVALID) as APPROVALID")
                sqlStat.AppendLine("      ,B.PASSENDYMD        as PASSENDYMD")
                sqlStat.AppendLine("  FROM        COM.lns0001_user       A")
                sqlStat.AppendLine("  INNER JOIN  COM.lns0002_userpass   B")
                sqlStat.AppendLine("    ON B.USERID      = A.USERID")
                sqlStat.AppendLine("   and B.DELFLG     <> @P4 ")
                sqlStat.AppendLine(" Where A.USERID      = @P1 ")
                sqlStat.AppendLine("   and A.STYMD      <= @P2")
                sqlStat.AppendLine("   and A.ENDYMD     >= @P3")
                sqlStat.AppendLine("   and B.PASSENDYMD >= @P3")
                sqlStat.AppendLine("   and A.DELFLG     <> @P4")

                Using SQLcmd As New MySqlCommand(sqlStat.ToString, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 20).Value = UserID.Text
                        .Add("@P2", MySqlDbType.Date).Value = Date.Now
                        .Add("@P3", MySqlDbType.Date).Value = Date.Now
                        .Add("@P4", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        WW_err = C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR
                        If SQLdr.Read Then
                            WW_USERID = Convert.ToString(SQLdr("USERID"))
                            WW_PASSWORD = Convert.ToString(SQLdr("PASSWORD"))
                            WW_USERCAMP = Convert.ToString(SQLdr("CAMPCODE"))
                            WW_ORG = Convert.ToString(SQLdr("ORG"))
                            WW_STYMD = CDate(SQLdr("STYMD"))
                            WW_ENDYMD = CDate(SQLdr("ENDYMD"))
                            WW_MISSCNT = CInt(SQLdr("MISSCNT"))
                            If SQLdr("UPDYMD") Is DBNull.Value Then
                                WW_UPDYMD = System.DateTime.UtcNow
                            Else
                                WW_UPDYMD = CDate(SQLdr("UPDYMD"))
                            End If
                            WW_UPDTIMSTP = CType(SQLdr("UPDTIMSTP"), Date)
                            '20191101-追加-START
                            WW_MENUROLE = Convert.ToString(SQLdr("MENUROLE"))
                            WW_MAPROLE = Convert.ToString(SQLdr("MAPROLE"))
                            WW_VIEWPROFID = Convert.ToString(SQLdr("VIEWPROFID"))
                            WW_RPRTPROFID = Convert.ToString(SQLdr("RPRTPROFID"))
                            'WW_APPROVALID = Convert.ToString(SQLdr("APPROVALID"))
                            '20191101-追加-END
                            WW_MAPID = Convert.ToString(SQLdr("MAPID"))
                            WW_VARIANT = Convert.ToString(SQLdr("VARIANT"))
                            WW_PASSENDYMD = Convert.ToString(SQLdr("PASSENDYMD"))

                            WW_err = C_MESSAGE_NO.NORMAL
                        End If

                    End Using
                End Using

            Catch ex As Exception
                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()


                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "lns0001_user SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "lns0001_user SELECT"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'ユーザID誤り
            'If Not isNormal(WW_err) OrElse
            '    UserID.Text = C_DEFAULT_DATAKEY OrElse
            '    UserID.Text = "INIT" Then

            If Not isNormal(WW_err) Then
                Master.Output(C_MESSAGE_NO.LOGIN_IDPSW_ERROR, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                Exit Sub
            End If

            '○ パスワードチェック
            'ユーザあり　かつ　(パスワード誤り　または　パスワード6回以上誤り)
            If (PassWord.Text <> WW_PASSWORD) Then

                Master.Output(C_MESSAGE_NO.LOGIN_PSWINPUT_ERROR, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                WW_chk = "err"

            ElseIf (WW_MISSCNT >= C_MAX_MISS_PASSWORD_COUNT) Then

                Master.Output(C_MESSAGE_NO.LOGIN_PSWNUM_ERROR, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                WW_chk = "err"

            End If

            If WW_chk = "err" Then
                'パスワードエラー回数のカウントUP
                Try
                    'S0014_USER更新SQL文
                    Dim SQL_Str As String =
                         "Update COM.lns0002_userpass " _
                       & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                       & "Where  USERID  = @P3 "
                    Using SQLcmd As New MySqlCommand(SQL_Str, SQLcon)
                        Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Int32)
                        Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.DateTime)
                        Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 20)
                        If WW_MISSCNT = 999 Then
                            PARA1.Value = WW_MISSCNT
                        Else
                            PARA1.Value = WW_MISSCNT + 1
                        End If
                        PARA2.Value = Date.Now
                        PARA3.Value = UserID.Text
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception

                    'SQL コネクションクローズ
                    SQLcon.Close()
                    SQLcon.Dispose()


                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "lns0002_userpass UPDATE")
                    CS0011LOGWRITE.INFSUBCLASS = "Main"
                    CS0011LOGWRITE.INFPOSI = "lns0002_userpass Update"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                    CS0011LOGWRITE.CS0011LOGWrite()
                End Try
                UserID.Focus()
                Exit Sub
            End If

            '○ パスワードチェックＯＫ時処理
            'セッション情報（ユーザＩＤ）設定
            CS0050Session.USERID = UserID.Text

            'ミスカウントクリア
            Try
                'S0014_USER更新SQL文
                Dim SQL_Str As String =
                     "Update COM.lns0002_userpass " _
                   & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                   & "Where  USERID  = @P3 "
                Using SQLcmd As New MySqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Int32)
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.DateTime)
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 20)
                    PARA1.Value = 0
                    PARA2.Value = Date.Now
                    PARA3.Value = UserID.Text
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception

                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()

                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "lns0002_userpass UPDATE")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "lns0002_userpass Update"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()
                Exit Sub
            End Try

            'SQL コネクションクローズ
            SQLcon.Close()
            SQLcon.Dispose()

            '■■■　終了処理　■■■

            '○ パスワードチェックＯＫ時、メニュー画面へ遷移するＵＲＬの取得
            'ユーザマスタより、MAPIDを取得
            GetURL(WW_PASSENDYMD, WW_MAPID, WW_URL)

        End Using

        CS0050Session.VIEW_MAPID = WW_MAPID
        '20191101-追加-START
        CS0050Session.VIEW_MENU_MODE = WW_MENUROLE
        CS0050Session.VIEW_MAP_MODE = WW_MAPID
        CS0050Session.VIEW_VIEWPROF_MODE = WW_VIEWPROFID
        CS0050Session.VIEW_RPRTPROF_MODE = WW_RPRTPROFID
        'CS0050Session.VIEW_APPROVALID = WW_APPROVALID
        '20191101-追加-END
        CS0050Session.VIEW_MAP_VARIANT = WW_VARIANT
        CS0050Session.MAP_ETC = ""
        CS0050Session.VIEW_PERMIT = ""
        CS0050Session.UserMenuCostomList = WW_UserMenuList
        CS0050Session.APSV_COMPANY = WW_USERCAMP
        CS0050Session.APSV_ORG = WW_ORG
        CS0050Session.TERM_COMPANY = WW_USERCAMP
        CS0050Session.TERM_ORG = WW_ORG
        Master.MAPID = WW_MAPID
        Master.USERCAMP = WW_USERCAMP
        '20191101-追加-START
        Master.ROLE_MENU = WW_MENUROLE
        Master.ROLE_MAP = WW_MAPID
        Master.ROLE_VIEWPROF = WW_VIEWPROFID
        Master.ROLE_RPRTPROF = WW_RPRTPROFID
        'Master.ROLE_APPROVALID = WW_APPROVALID
        '20191101-追加-END
        Master.MAPvariant = WW_VARIANT
        Master.MAPpermitcode = ""
        CS0050Session.LOGONDATE = WW_LOGONYMD


        'DioDocsライセンスキー取得
        'DataBase接続文字
        Using SQLcon As MySqlConnection = CS0050Session.getConnection

            SQLcon.Open() 'DataBase接続(Open)
            Try
                'lns0001_user検索SQL文
                Dim sqlStat As New StringBuilder
                sqlStat.AppendLine("SELECT rtrim(A.LICENSEKEY)   as LICENSEKEY")
                sqlStat.AppendLine(" FROM        COM.LNS0004_LICENSE       A")
                sqlStat.AppendLine(" WHERE A.LICENSETYPE     = @P1")
                sqlStat.AppendLine("   AND A.LICENSEGET      = @P2")
                sqlStat.AppendLine("   AND A.DELFLG         <> @P3")

                Using SQLcmd As New MySqlCommand(sqlStat.ToString, SQLcon)

                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 10).Value = "diodocs"
                        .Add("@P2", MySqlDbType.VarChar, 20).Value = CS0050Session.LICENSE_GET
                        .Add("@P3", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            Workbook.SetLicenseKey(Convert.ToString(SQLdr("LICENSEKEY")))
                        End If
                    End Using

                End Using

            Catch ex As Exception
                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()


                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0004_LICENSE SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "LNS0004_LICENSE SELECT"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'SQL コネクションクローズ
            SQLcon.Close()
            SQLcon.Dispose()
        End Using

        'DioDocsライセンスキー取得
        'DataBase接続文字
        Using SQLcon As MySqlConnection = CS0050Session.getConnection

            SQLcon.Open() 'DataBase接続(Open)
            Try
                'lns0001_user検索SQL文
                Dim sqlStat As New StringBuilder
                sqlStat.AppendLine("SELECT rtrim(A.LICENSEKEY)   as LICENSEKEY")
                sqlStat.AppendLine(" FROM        COM.LNS0004_LICENSE       A")
                sqlStat.AppendLine(" WHERE A.LICENSETYPE     = @P1")
                sqlStat.AppendLine("   AND A.LICENSEGET      = @P2")
                sqlStat.AppendLine("   AND A.DELFLG         <> @P3")

                Using SQLcmd As New MySqlCommand(sqlStat.ToString, SQLcon)

                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 10).Value = "diodocs"
                        .Add("@P2", MySqlDbType.VarChar, 20).Value = CS0050Session.LICENSE_GET
                        .Add("@P3", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            Workbook.SetLicenseKey(Convert.ToString(SQLdr("LICENSEKEY")))
                        End If
                    End Using

                End Using

            Catch ex As Exception
                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()


                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0004_LICENSE SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "LNS0004_LICENSE SELECT"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'SQL コネクションクローズ
            SQLcon.Close()
            SQLcon.Dispose()
        End Using

        'セッション情報_IPアドレス設定処理
        SetIpAddress()

        'ログオン日付マスタ登録
        'Dim CmnCtn As New CmnCtn
        Master.SetMAPValue()
        InsertLogonYMDMaster("1")
        'CmnLNG.InsertLogonYMDMaster("1", Master)

        '画面遷移実行
        If CS0050Session.USERID <> "INIT" Then
            Server.Transfer(WW_URL)
        End If

    End Sub

    ''' <summary>
    ''' 遷移先URLの取得
    ''' </summary>
    ''' <param name="I_PASSENDYMD"></param>
    ''' <param name="I_MAPID"></param>
    ''' <param name="O_URL"></param>
    ''' <remarks></remarks>
    Protected Sub GetURL(ByVal I_PASSENDYMD As String, ByVal I_MAPID As String, ByRef O_URL As String)

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

        Dim WW_URL As String = ""
        Try
            'DataBase接続文字
            Using SQLcon As MySqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'LNS0007_URL検索SQL文
                Dim SQL_Str As String =
                     "SELECT rtrim(URL) as URL " _
                   & " FROM  COM.LNS0007_URL " _
                   & " Where MAPID    = @P1 " _
                   & "   and STYMD   <= @P2 " _
                   & "   and ENDYMD  >= @P3 " _
                   & "   and DELFLG  <> @P4 "
                Using SQLcmd As New MySqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 50)
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 1)
                    PARA1.Value = I_MAPID

                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        O_URL = Convert.ToString(SQLdr("URL"))
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0007_URL SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "GetURL"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "LNS0007_URL SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 表示用のガイダンスデータ取得
    ''' </summary>
    ''' <param name="sqlCon">MySqlConnection</param>
    ''' <returns>ガイダンスデータ</returns>
    Private Function GetGuidanceData(sqlCon As MySqlConnection) As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            .Add("GUIDANCENO", GetType(String))
            .Add("ENTRYDATE", GetType(String))
            .Add("TYPE", GetType(String))
            .Add("TITLE", GetType(String))
            .Add("NAIYOU", GetType(String))
            .Add("FILE1", GetType(String))
        End With
        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT GD.GUIDANCENO")
            sqlStat.AppendLine("      ,date_format(GD.INITYMD,'%Y/%m/%d') AS ENTRYDATE")
            sqlStat.AppendLine("      ,GD.TYPE                       AS TYPE")
            sqlStat.AppendLine("      ,GD.TITLE                      AS TITLE")
            sqlStat.AppendLine("      ,GD.NAIYOU                     AS NAIYOU")
            sqlStat.AppendLine("      ,GD.FILE1                      AS FILE1")
            sqlStat.AppendLine("  FROM com.LNS0008_GUIDANCE GD")
            sqlStat.AppendLine(" WHERE CURDATE() BETWEEN GD.FROMYMD AND GD.ENDYMD")
            sqlStat.AppendLine("   AND DELFLG = @DELFLG_NO")
            sqlStat.AppendLine("   AND OUTFLG = '1'")
            sqlStat.AppendLine(" ORDER BY (CASE WHEN GD.TYPE = 'E' THEN '1'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'W' THEN '2'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'I' THEN '3'")
            sqlStat.AppendLine("                ELSE '9'")
            sqlStat.AppendLine("            END)")
            sqlStat.AppendLine("          ,GD.INITYMD DESC")
            '他のフラグや最大取得件数（条件がある場合）はあとで
            Using sqlGuidCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlGuidCmd.Parameters.Add("@DELFLG_NO", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlGuidDr As MySqlDataReader = sqlGuidCmd.ExecuteReader()
                    Dim dr As DataRow
                    While sqlGuidDr.Read
                        dr = retDt.NewRow
                        dr("GUIDANCENO") = sqlGuidDr("GUIDANCENO")
                        dr("ENTRYDATE") = sqlGuidDr("ENTRYDATE")
                        dr("TYPE") = sqlGuidDr("TYPE")
                        dr("TITLE") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("TITLE")))
                        dr("NAIYOU") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("NAIYOU"))).Replace(ControlChars.CrLf, ControlChars.VerticalTab & "<br />").Replace(ControlChars.Cr, ControlChars.VerticalTab & "<br />").Replace(ControlChars.Lf, ControlChars.VerticalTab & "<br />")
                        dr("NAIYOU") = Convert.ToString(dr("NAIYOU")).Replace(ControlChars.VerticalTab, ControlChars.CrLf)
                        dr("FILE1") = Convert.ToString(sqlGuidDr("FILE1"))

                        retDt.Rows.Add(dr)
                    End While
                End Using

            End Using

            'SQLコネクションクローズ
            sqlCon.Close()
            sqlCon.Dispose()

        Catch ex As Exception

            'SQLコネクションクローズ
            sqlCon.Close()
            sqlCon.Dispose()

        End Try

        Return retDt
    End Function

    ''' <summary>
    ''' セッション情報_IPアドレス設定処理
    ''' </summary>
    Public Sub SetIpAddress()
        '〇 IPアドレス取得
        Dim RemoteIp As String = Request.UserHostAddress
        Dim RemoteIp3 As String = ""
        Dim ClientIP As String = ""
        Try

            RemoteIp = Request.UserHostAddress
            Dim ClientIphEntry As IPHostEntry = Dns.GetHostEntry(RemoteIp)
            For Each ipAddr As IPAddress In ClientIphEntry.AddressList
                'IPv4にする
                If ipAddr.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                    RemoteIp = ipAddr.ToString
                End If
            Next
            If RemoteIp.LastIndexOf(".") < 0 Then
                'Exit Sub
            Else
                RemoteIp3 = Mid(RemoteIp, 1, RemoteIp.LastIndexOf("."))
            End If
        Catch ex As Exception
            'Exit Sub
        End Try
        CS0050Session.TERMIPADDRESS = RemoteIp
    End Sub

    ''' <summary>
    ''' ログオン日付マスタTBL追加処理
    ''' </summary>
    ''' <param name="I_KBN">区分</param>
    Public Sub InsertLogonYMDMaster(ByVal I_KBN As String)
        '◯ログオン日付マスタTBL
        Dim sqlLogonYMDStat As New StringBuilder
        sqlLogonYMDStat.AppendLine("INSERT INTO COM.LNS0003_LOGONYMD")
        sqlLogonYMDStat.AppendLine("   (TERMID,USERID,IPADDRESS,LOGYMD,KBN,")
        sqlLogonYMDStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,INITPGID,")
        sqlLogonYMDStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,UPDPGID,RECEIVEYMD )")
        sqlLogonYMDStat.AppendLine("    VALUES")
        sqlLogonYMDStat.AppendLine("   (@TERMID,@USERID,@IPADDRESS,@LOGYMD,@KBN,")
        sqlLogonYMDStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,@INITPGID,")
        sqlLogonYMDStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@UPDPGID,@RECEIVEYMD )")
        Dim dtNow = Date.Now.ToString("yyyy/MM/dd HH:mm:ss.FFF")
        Using SQLcon As MySqlConnection = CS0050Session.getConnection
            SQLcon.Open()       'DataBase接続
            Using sqlTran As MySqlTransaction = SQLcon.BeginTransaction
                Using sqlLogonYMDCmd As New MySqlCommand(sqlLogonYMDStat.ToString, SQLcon, sqlTran)
                    With sqlLogonYMDCmd.Parameters
                        .Add("TERMID", MySqlDbType.VarChar).Value = Master.USERTERMID
                        .Add("USERID", MySqlDbType.VarChar).Value = CS0050Session.USERID
                        .Add("IPADDRESS", MySqlDbType.VarChar).Value = CS0050Session.TERMIPADDRESS
                        .Add("LOGYMD", MySqlDbType.VarChar).Value = dtNow
                        .Add("KBN", MySqlDbType.VarChar).Value = I_KBN

                        .Add("DELFLG", MySqlDbType.VarChar).Value = "0"
                        .Add("INITYMD", MySqlDbType.VarChar).Value = Date.Now
                        .Add("INITUSER", MySqlDbType.VarChar).Value = Master.USERID
                        .Add("INITTERMID", MySqlDbType.VarChar).Value = Master.USERTERMID
                        .Add("INITPGID", MySqlDbType.VarChar).Value = Me.GetType().BaseType.Name
                        .Add("UPDYMD", MySqlDbType.VarChar).Value = Date.Now
                        .Add("UPDUSER", MySqlDbType.VarChar).Value = Master.USERID
                        .Add("UPDTERMID", MySqlDbType.VarChar).Value = Master.USERTERMID
                        .Add("UPDPGID", MySqlDbType.VarChar).Value = Me.GetType().BaseType.Name
                        .Add("RECEIVEYMD", MySqlDbType.VarChar).Value = C_DEFAULT_YMD
                    End With
                    sqlLogonYMDCmd.CommandTimeout = 300
                    sqlLogonYMDCmd.ExecuteNonQuery()
                End Using
                'ここまで来たらコミット
                sqlTran.Commit()
            End Using
        End Using

    End Sub
End Class



