Option Strict On
Imports System.IO
''' <summary>
''' iniファイル情報取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0001INIFILEget

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 文字列タイプ
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum STRINGTYPE
        NONE
        SQL_SERVER
        AP_SERVER
        LOG_DIR
        JNL_DIR
        PDF_DIR
        UPF_DIR
        SYS_DIR
        WEBAPI_URL
        WEBAPI_ACCOUNT
        WEBAPI_TOKEN_SYS
        WEBAPI_TOKEN_OIL
        WEBAPI_TOKEN_KAN
        WEBAPI_TOKEN_TYU
        WEBAPI_RENKEIFLG
        LICENSE
        ENVIRONMENT
        PRINTROOT
    End Enum

    Private Const IniFileC As String = "C:\APPL\APPLINI\CTN\JOTWEB_LNG.ini"
    Private Const IniFileD As String = "D:\APPL\APPLINI\CTN\JOTWEB_LNG.ini"

    ''' <summary>
    ''' iniファイル情報取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0001INIFILEget()

        Dim CS0050SESSION As New CS0050SESSION

        ERR = C_MESSAGE_NO.NORMAL

        Dim IniString As String = ""
        Dim IniType As Integer = STRINGTYPE.NONE
        Dim IniBuf As String = ""
        Dim IniRef As Integer = 0

        Dim INIFILE As String = ""
        'WebConfigのAPPStrringsに指定したパス優先
        If ConfigurationManager.AppSettings.AllKeys.Contains("InifilePath") AndAlso
           ConfigurationManager.AppSettings("InifilePath") <> "" Then
            'WebConfigの設定が存在したら
            'ファイルの存在有無に関わらず最優先
            INIFILE = ConfigurationManager.AppSettings("InifilePath")
            If IO.File.Exists(INIFILE) = False Then
                '存在しない場合は例外スロー(503エラー Service Unavailable)
                Throw New HttpException(503, "WebConfigに定義したIniファイルが存在しません")
            End If
        Else
            'WebConfigの設定が存在しない場合は
            '固定パスCとDを
            INIFILE = IniFileC
            If Not File.Exists(IniFileC) Then INIFILE = IniFileD
        End If

        Using sr As StreamReader = New StreamReader(INIFILE, Encoding.UTF8)
            Try

                'ファイル内容の文字情報を全て読み込む
                While (Not sr.EndOfStream)
                    IniBuf = sr.ReadLine().Replace(vbTab, "")

                    '文字列のコメント除去
                    If InStr(IniBuf, "'") >= 1 Then
                        IniRef = InStr(IniBuf, "'") - 1
                    Else
                        IniRef = Len(IniBuf)
                    End If
                    IniBuf = Mid(IniBuf, 1, IniRef)

                    'SQLサーバー接続文字
                    If IniBuf.IndexOf("<sql server>") >= 0 OrElse IniType = STRINGTYPE.SQL_SERVER Then
                        IniType = STRINGTYPE.SQL_SERVER
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</sql server>") >= 0 Then
                            IniString = IniString.Replace("<sql server>", "")
                            IniString = IniString.Replace("</sql server>", "")
                            IniString = IniString.Replace("<connection string>", "")
                            IniString = IniString.Replace("</connection string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.DBCon = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'APサーバー名称
                    If IniBuf.IndexOf("<ap server>") >= 0 OrElse IniType = STRINGTYPE.AP_SERVER Then
                        IniType = STRINGTYPE.AP_SERVER
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</ap server>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<ap server>", "")
                            IniString = IniString.Replace("</ap server>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.APSV_ID = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'Log出力Dir(パス)
                    If IniBuf.IndexOf("<log directory>") >= 0 OrElse IniType = STRINGTYPE.LOG_DIR Then
                        IniType = STRINGTYPE.LOG_DIR
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</log directory>") >= 0 Then
                            IniString = IniString.Replace("<log directory>", "")
                            IniString = IniString.Replace("</log directory>", "")
                            IniString = IniString.Replace("<directory string>", "")
                            IniString = IniString.Replace("</directory string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("path=", "")

                            CS0050SESSION.LOG_PATH = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'jnl出力Dir(パス)
                    If IniBuf.IndexOf("<jnl directory>") >= 0 OrElse IniType = STRINGTYPE.JNL_DIR Then
                        IniType = STRINGTYPE.JNL_DIR
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</jnl directory>") >= 0 Then
                            IniString = IniString.Replace("<jnl directory>", "")
                            IniString = IniString.Replace("</jnl directory>", "")
                            IniString = IniString.Replace("<directory string>", "")
                            IniString = IniString.Replace("</directory string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("path=", "")

                            CS0050SESSION.JORNAL_PATH = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'PDF出力Dir(パス)
                    If IniBuf.IndexOf("<PDF directory>") >= 0 OrElse IniType = STRINGTYPE.PDF_DIR Then
                        IniType = STRINGTYPE.PDF_DIR
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</PDF directory>") >= 0 Then
                            IniString = IniString.Replace("<PDF directory>", "")
                            IniString = IniString.Replace("</PDF directory>", "")
                            IniString = IniString.Replace("<directory string>", "")
                            IniString = IniString.Replace("</directory string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("path=", "")

                            CS0050SESSION.PDF_PATH = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'File出力Dir(パス)
                    If IniBuf.IndexOf("<File directory>") >= 0 OrElse IniType = STRINGTYPE.UPF_DIR Then
                        IniType = STRINGTYPE.UPF_DIR
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</File directory>") >= 0 Then
                            IniString = IniString.Replace("<File directory>", "")
                            IniString = IniString.Replace("</File directory>", "")
                            IniString = IniString.Replace("<directory string>", "")
                            IniString = IniString.Replace("</directory string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("path=", "")

                            CS0050SESSION.UPLOAD_PATH = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'システム格納Dir(パス)
                    If IniBuf.IndexOf("<Sys directory>") >= 0 OrElse IniType = STRINGTYPE.SYS_DIR Then
                        IniType = STRINGTYPE.SYS_DIR
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</Sys directory>") >= 0 Then
                            IniString = IniString.Replace("<Sys directory>", "")
                            IniString = IniString.Replace("</Sys directory>", "")
                            IniString = IniString.Replace("<directory string>", "")
                            IniString = IniString.Replace("</directory string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("path=", "")

                            CS0050SESSION.SYSTEM_PATH = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    'ダウンロードファイルURL
                    If IniBuf.IndexOf("<print root>") >= 0 OrElse IniType = STRINGTYPE.PRINTROOT Then
                        IniType = STRINGTYPE.PRINTROOT
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</print root>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<print root>", "")
                            IniString = IniString.Replace("</print root>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.PRINT_ROOT_URL_NAME = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                    '####楽々API関連情報の取得↓#####
                    'URL
                    If IniBuf.IndexOf("<webapi url>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_URL Then
                        IniType = STRINGTYPE.WEBAPI_URL
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi url>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi url>", "")
                            IniString = IniString.Replace("</webapi url>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.WEBAPIURL = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    'アカウント
                    If IniBuf.IndexOf("<webapi account>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_ACCOUNT Then
                        IniType = STRINGTYPE.WEBAPI_ACCOUNT
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi account>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi account>", "")
                            IniString = IniString.Replace("</webapi account>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.WEBAPIACCOUNT = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    'システム部用トークン
                    If IniBuf.IndexOf("<webapi tokenSystem>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_TOKEN_SYS Then
                        IniType = STRINGTYPE.WEBAPI_TOKEN_SYS
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi tokenSystem>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi tokenSystem>", "")
                            IniString = IniString.Replace("</webapi tokenSystem>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.WEBAPITOKENSYSTEM = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    '連携実行フラグ
                    If IniBuf.IndexOf("<webapi renkeiflg>") >= 0 OrElse IniType = STRINGTYPE.WEBAPI_RENKEIFLG Then
                        IniType = STRINGTYPE.WEBAPI_RENKEIFLG
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</webapi renkeiflg>") >= 0 Then
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace("<webapi renkeiflg>", "")
                            IniString = IniString.Replace("</webapi renkeiflg>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.WEBAPIFLG = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If
                    '####楽々API関連情報の取得↑#####

                    'ライセンス用
                    If IniBuf.IndexOf("<diodocs license>") >= 0 OrElse IniType = STRINGTYPE.LICENSE Then
                        IniType = STRINGTYPE.LICENSE
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</diodocs license>") >= 0 Then
                            IniString = IniString.Replace("<diodocs license>", "")
                            IniString = IniString.Replace("</diodocs license>", "")
                            IniString = IniString.Replace("<license string>", "")
                            IniString = IniString.Replace("</license string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.LICENSE_GET = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE

                            '2025/04/16 ADD
                            If CS0050SESSION.LICENSE_GET.ToUpper = "HONBAN" Then
                                CS0050SESSION.HTTPS_GET = "https"
                            Else
                                CS0050SESSION.HTTPS_GET = "http"
                            End If
                            '2025/04/16 ADD ENDED
                        End If
                    End If

                    '環境判定用
                    If IniBuf.IndexOf("<environment flg>") >= 0 OrElse IniType = STRINGTYPE.ENVIRONMENT Then
                        IniType = STRINGTYPE.ENVIRONMENT
                        IniString &= IniBuf

                        If IniBuf.IndexOf("</environment flg>") >= 0 Then
                            IniString = IniString.Replace("<environment flg>", "")
                            IniString = IniString.Replace("</environment flg>", "")
                            IniString = IniString.Replace("<name string>", "")
                            IniString = IniString.Replace("</name string>", "")
                            IniString = IniString.Replace(ControlChars.Quote, "")
                            IniString = IniString.Replace("value=", "")

                            CS0050SESSION.ENVIRONMENTFLG = Trim(IniString)
                            IniString = ""
                            IniType = STRINGTYPE.NONE
                        End If
                    End If

                End While
            Catch ex As Exception
                ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End Try

        End Using
    End Sub

End Structure
