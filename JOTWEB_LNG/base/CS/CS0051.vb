Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' ユーザ情報を取得する
''' </summary>
''' <remarks></remarks>
Public Class CS0051UserInfo : Implements IDisposable
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    Public Property USERID As String
    ''' <summary>
    ''' 範囲開始日付
    ''' </summary>
    Public Property STYMD As Date
    ''' <summary>
    ''' 範囲終了日付
    ''' </summary>
    Public Property ENDYMD As Date
    ''' <summary>
    ''' 所属会社
    ''' </summary>
    Public Property CAMPCODE As String
    ''' <summary>
    ''' 所属組織
    ''' </summary>
    Public Property ORG As String
    ''' <summary>
    ''' 所属組織名称
    ''' </summary>
    Public Property ORGNAME As String
    ''' <summary>
    ''' 所轄支店コード
    ''' </summary>
    Public Property CONTROLCODE As String
    ''' <summary>
    ''' 所轄支店名称
    ''' </summary>
    Public Property CONTROLNAME As String
    ''' <summary>
    ''' 社員コード
    ''' </summary>
    Public Property STAFFCODE As String
    ''' <summary>
    ''' 社員名（短）
    ''' </summary>
    Public Property STAFFNAMES As String
    ''' <summary>
    ''' 社員名（長）
    ''' </summary>
    Public Property STAFFNAMEL As String
    ''' <summary>
    ''' メールアドレス
    ''' </summary>
    Public Property EMAIL As String
    ''' <summary>
    ''' 初期表示画面ＩＤ
    ''' </summary>
    Public Property MAPID As String
    ''' <summary>
    ''' メニュー表示用変数
    ''' </summary>
    Public Property MAPVARI As String
    ''' <summary>
    ''' メニュー権限
    ''' </summary>
    Public Property MENUROLE As String
    ''' <summary>
    ''' 更新権限
    ''' </summary>
    Public Property MAPROLE As String
    ''' <summary>
    ''' 画面表示権限
    ''' </summary>
    Public Property VIEWPROFROLE As String
    ''' <summary>
    ''' エクセル出力権限
    ''' </summary>
    Public Property RPRTPROFROLE As String
    '''' <summary>
    '''' 承認権限
    '''' </summary>
    'Public Property APPROVALIDROLE As String
    ''' <summary>
    ''' 部署権限
    ''' </summary>
    Public Property ORGROLE As String
    ''' <summary>
    ''' 画面プロファイルID
    ''' </summary>
    Public Property VIEWPROFID As String
    ''' <summary>
    ''' 帳票プロファイルID
    ''' </summary>
    Public Property RPRTPROFID As String
    ''' <summary>
    ''' 所属サーバID
    ''' </summary>
    Public Property SERVERID As String
    ''' <summary>
    ''' 所属サーバ名称
    ''' </summary>
    Public Property SERVERNAMES As String

    ''' <summary>
    ''' 所属サーバIPアドレス
    ''' </summary>
    Public Property SERVERIP As String

    ''' <summary>
    ''' エラーメッセージ
    ''' </summary>
    Public Property ERR As String


    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME = "getInfo"
    ''' <summary>
    ''' 取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getInfo()
        '●In PARAMチェック
        'PARAM01:ユーザID
        If IsNothing(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        '●初期化処理
        CAMPCODE = ""
        ORG = ""
        CONTROLCODE = ""
        '        STAFFCODE = ""
        STAFFNAMES = ""
        STAFFNAMEL = ""
        EMAIL = ""
        MAPID = ""
        MAPVARI = ""
        '        CAMPROLE = ""
        MENUROLE = ""
        MAPROLE = ""
        '        ORGROLE = ""
        VIEWPROFID = ""
        RPRTPROFID = ""
        'セッション管理
        Dim sm As New CS0050SESSION

        'EXTRA PARAM01:STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If

        'EXTRA PARAM01:ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
            ENDYMD = Date.Now
        End If

        '●ユーザ情報取得
        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'Message検索SQL文
            Dim SQLStr As New System.Text.StringBuilder
            SQLStr.AppendLine("SELECT")
            SQLStr.AppendLine("   rtrim(UR.CAMPCODE) as CAMPCODE ")
            SQLStr.AppendLine(" , rtrim(UR.ORG) as ORG")
            SQLStr.AppendLine(" , rtrim(ORG.NAME) as ORGNAME")
            SQLStr.AppendLine(" , rtrim(UR.STAFFNAMES) as STAFFNAMES")
            SQLStr.AppendLine(" , rtrim(UR.STAFFNAMEL) as STAFFNAMEL")
            SQLStr.AppendLine(" , rtrim(UR.EMAIL) as EMAIL")
            SQLStr.AppendLine(" , rtrim(UR.MAPID) as MAPID")
            SQLStr.AppendLine(" , rtrim(UR.VARIANT) as VARIANT")
            SQLStr.AppendLine(" , rtrim(UR.MENUROLE) as MENUROLE")
            SQLStr.AppendLine(" , rtrim(UR.MAPROLE) as MAPROLE")
            SQLStr.AppendLine(" , rtrim(UR.VIEWPROFID) as VIEWPROFID")
            SQLStr.AppendLine(" , rtrim(UR.RPRTPROFID) as RPRTPROFID")
            'SQLStr.AppendLine(" , rtrim(UR.APPROVALID) as APPROVALID")
            SQLStr.AppendLine(" , rtrim(ORG.CONTROLCODE) as CONTROLCODE")
            SQLStr.AppendLine(" , rtrim(CNTRL.NAME) as CONTROLNAME")
            SQLStr.AppendLine("FROM  COM.lns0001_user UR")
            SQLStr.AppendLine("INNER JOIN COM.LNS0014_ORG ORG")
            SQLStr.AppendLine(" ON ORG.CAMPCODE = UR.CAMPCODE")
            SQLStr.AppendLine("   and ORG.ORGCODE = UR.ORG")
            SQLStr.AppendLine("   and ORG.STYMD <= @P3 ")
            SQLStr.AppendLine("   and ORG.ENDYMD >= @P2 ")
            SQLStr.AppendLine("   and ORG.DELFLG = @P4 ")
            SQLStr.AppendLine("LEFT JOIN COM.LNS0014_ORG CNTRL")
            SQLStr.AppendLine(" ON CNTRL.CAMPCODE = UR.CAMPCODE")
            SQLStr.AppendLine("   and CNTRL.ORGCODE = ORG.CONTROLCODE")
            SQLStr.AppendLine("   and CNTRL.STYMD <= @P3 ")
            SQLStr.AppendLine("   and CNTRL.ENDYMD >= @P2 ")
            SQLStr.AppendLine("   and CNTRL.DELFLG = @P4 ")
            SQLStr.AppendLine("WHERE UR.USERID = @P1 ")
            SQLStr.AppendLine("   and UR.STYMD <= @P3 ")
            SQLStr.AppendLine("   and UR.ENDYMD >= @P2 ")
            SQLStr.AppendLine("   and UR.DELFLG = @P4 ")

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = USERID
                    .Add("@P2", MySqlDbType.Date).Value = STYMD
                    .Add("@P3", MySqlDbType.Date).Value = ENDYMD
                    .Add("@P4", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.ALIVE
                End With

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    If SQLdr.Read Then
                        CAMPCODE = Convert.ToString(SQLdr("CAMPCODE"))
                        ORG = Convert.ToString(SQLdr("ORG"))
                        ORGNAME = Convert.ToString(SQLdr("ORGNAME"))
                        CONTROLCODE = Convert.ToString(SQLdr("CONTROLCODE"))
                        CONTROLNAME = Convert.ToString(SQLdr("CONTROLNAME"))
                        STAFFNAMES = Convert.ToString(SQLdr("STAFFNAMES"))
                        STAFFNAMEL = Convert.ToString(SQLdr("STAFFNAMEL"))
                        EMAIL = Convert.ToString(SQLdr("EMAIL"))
                        MAPID = Convert.ToString(SQLdr("MAPID"))
                        MAPVARI = Convert.ToString(SQLdr("VARIANT"))
                        MENUROLE = Convert.ToString(SQLdr("MENUROLE"))
                        VIEWPROFROLE = Convert.ToString(SQLdr("VIEWPROFID"))
                        RPRTPROFROLE = Convert.ToString(SQLdr("RPRTPROFID"))
                        'APPROVALIDROLE = Convert.ToString(SQLdr("APPROVALID"))
                        MAPROLE = Convert.ToString(SQLdr("MAPROLE"))
                        VIEWPROFID = Convert.ToString(SQLdr("VIEWPROFID"))
                        RPRTPROFID = Convert.ToString(SQLdr("RPRTPROFID"))
                        ERR = C_MESSAGE_NO.NORMAL
                    Else
                        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:lns0001_user Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR

            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' 所属サーバ情報取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BelongtoServer()
        '●In PARAMチェック
        'PARAM01:所属部署
        If IsNothing(ORG) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORG"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        'PARAM02:所属会社
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        '●初期化処理
        SERVERID = String.Empty
        SERVERNAMES = String.Empty
        SERVERIP = String.Empty
        'セッション管理
        Dim sm As New CS0050SESSION

        'EXTRA PARAM01:STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If

        'EXTRA PARAM01:ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
            ENDYMD = Date.Now
        End If

        '●端末情報取得
        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'Message検索SQL文
            Dim SQLStr As String =
                     "SELECT " _
                   & "   rtrim(TERMID) as TERMID " _
                   & " , rtrim(IPADDR) as IPADDR " _
                   & " , rtrim(TERMNAME) as TERMNAMES " _
                   & " FROM  com.LNS0001_TERM " _
                   & " Where TERMORG    = @P1 " _
                   & "   and TERMCAMP   = @P6 " _
                   & "   and TERMCLASS  = @P5 " _
                   & "   and STYMD     <= @P3 " _
                   & "   and ENDYMD    >= @P2 " _
                   & "   and DELFLG    <> @P4 " _
                   & " ORDER BY TERMCLASS ASC "
            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 30).Value = ORG
                    .Add("@P2", MySqlDbType.Date).Value = STYMD
                    .Add("@P3", MySqlDbType.Date).Value = ENDYMD
                    .Add("@P4", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    .Add("@P5", MySqlDbType.VarChar, 1).Value = C_TERMCLASS.BASE
                    .Add("@P6", MySqlDbType.VarChar, 20).Value = CAMPCODE
                End With


                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    If SQLdr.Read Then
                        SERVERID = Convert.ToString(SQLdr("TERMID"))
                        SERVERIP = Convert.ToString(SQLdr("IPADDR"))
                        SERVERNAMES = Convert.ToString(SQLdr("TERMNAMES"))
                        ERR = C_MESSAGE_NO.NORMAL
                    Else
                        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0001_TERM Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR

            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose(ByVal isDispose As Boolean)
        If isDispose Then

        End If
    End Sub
End Class


