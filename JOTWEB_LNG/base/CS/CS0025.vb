﻿Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' 権限チェック（マスタチェック）
''' </summary>
''' <remarks></remarks>
Public Structure CS0025AUTHORget

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String

    ''' <summary>
    ''' OBJECTコード
    ''' </summary>
    ''' <value>OBJECT</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OBJCODE() As String

    ''' <summary>
    ''' CODE
    ''' </summary>
    ''' <value>CODE</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CODE() As String

    ''' <summary>
    ''' 有効日（開始）
    ''' </summary>
    ''' <value>有効日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As String

    ''' <summary>
    ''' 有効日（終了）
    ''' </summary>
    ''' <value>有効日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As String

    ''' <summary>
    ''' 権限コード
    ''' </summary>
    ''' <value>権限</value>
    ''' <returns></returns>
    ''' <remarks>0；権限無 1:参照権限 2:参照・更新権限</remarks>
    Public Property PERMITCODE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr),ERR:10003(権限エラー)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0025AUTHORget"
    ''' <summary>
    ''' 権限チェック
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0025AUTHORget()


        '●In PARAMチェック
        'PARAM01: USERID
        If IsNothing(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM02: OBJCODE
        If IsNothing(OBJCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0025AUTHORget"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "OBJCODE"                '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM03: CODE
        If IsNothing(CODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0025AUTHORget"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CODE"                   '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●権限チェック（マスタチェック）　…　ユーザ操作権限取得

        Try
            '検索SQL文
            Dim SQLStr As String =
                     "SELECT rtrim(B.PERMITCODE) as PERMITCODE        " _
                   & " FROM       COM.LNS0001_USER        A                 " _
                   & " INNER JOIN COM.LNS0005_ROLE        B              ON " _
                   & "       B.OBJECT   = @P2                         " _
                   & "   and B.ROLE     = CASE B.OBJECT               " _
                   & "                    WHEN 'MAP'  THEN A.MAPROLE  " _
                   & "                    WHEN 'VIEW' THEN A.VIEWPROFID " _
                   & "                    WHEN 'RPRT' THEN A.RPRTPROFID " _
                   & "                    END                         " _
                   & "   and B.CAMPCODE = A.CAMPCODE                  " _
                   & "   and B.CODE     = @P3                         " _
                   & "   and B.STYMD   <= @P4                         " _
                   & "   and B.ENDYMD  >= @P5                         " _
                   & "   and B.DELFLG  <> '1'                         " _
                   & " Where A.USERID   = @P1                         " _
                   & "   and A.STYMD   <= @P4                         " _
                   & "   and A.ENDYMD  >= @P5                         " _
                   & "   and A.DELFLG  <> '1'                         " _
                   & "ORDER BY B.SEQ                                  "
            '  "SELECT rtrim(B.PERMITCODE) as PERMITCODE        " _
            '& " FROM       COM.LNS0001_USER        A                 " _
            '& " INNER JOIN COM.LNS0005_ROLE        B              ON " _
            '& "       B.OBJECT   = @P2                         " _
            '& "   and B.ROLE     = CASE B.OBJECT               " _
            '& "                    WHEN 'ORG'  THEN A.ORGROLE  " _
            '& "                    WHEN 'CAMP' THEN A.CAMPROLE " _
            '& "                    WHEN 'MAP'  THEN A.MAPROLE  " _
            '& "                    END                         " _
            '& "   and B.CAMPCODE = A.CAMPCODE                  " _
            '& "   and B.CODE     = @P3                         " _
            '& "   and B.STYMD   <= @P4                         " _
            '& "   and B.ENDYMD  >= @P5                         " _
            '& "   and B.DELFLG  <> '1'                         " _
            '& " Where A.USERID   = @P1                         " _
            '& "   and A.STYMD   <= @P4                         " _
            '& "   and A.ENDYMD  >= @P5                         " _
            '& "   and A.DELFLG  <> '1'                         " _
            '& "ORDER BY B.SEQ                                  "

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = USERID
                    .Add("@P2", MySqlDbType.VarChar, 20).Value = OBJCODE
                    .Add("@P3", MySqlDbType.VarChar, 20).Value = CODE
                    .Add("@P4", MySqlDbType.Date).Value = ENDYMD
                    .Add("@P5", MySqlDbType.Date).Value = STYMD
                    .Add("@P6", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '権限コード初期値(権限なし)設定
                    PERMITCODE = ""
                    ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

                    If SQLdr.Read Then
                        PERMITCODE = SQLdr("PERMITCODE").ToString
                        ERR = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0005_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure
