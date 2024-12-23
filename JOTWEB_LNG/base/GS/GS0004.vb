﻿Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' 画面メモ情報更新
''' </summary>
''' <remarks></remarks>
Public Class GS0004MEMOset
    Inherits GS0000

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' メモ情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MEMO() As String

    ''' <summary>
    ''' 実行名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "GS0004MEMOset"
    ''' <summary>
    ''' メモ欄更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0004MEMOset()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: MAPID
        If checkParam(METHOD_NAME, MAPID) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM02: MEMO
        If checkParam(METHOD_NAME, _MEMO) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If


        '●画面メモ情報更新
        '○ DB(OIS0000_MEMO)更新
        Try
            'OIS0000_MEMO更新SQL文
            'Dim SQLStr As String =
            '        " DECLARE @hensuu as bigint ;                                                                    " _
            '             & " set @hensuu = 0 ;                                                                       " _
            '             & " DECLARE hensuu CURSOR FOR                                                               " _
            '             & "   SELECT UPDTIMSTP AS hensuu                                                            " _
            '             & "     FROM  COM.OIS0000_MEMO                                                              " _
            '             & "     WHERE USERID =@P2                                                                   " _
            '             & "       and MAPID = @P3 ;                                                                 " _
            '             & " OPEN hensuu ;                                                                                  " _
            '             & " FETCH NEXT FROM hensuu INTO @hensuu ;                                                          " _
            '             & " IF ( @@FETCH_STATUS = 0 )                                                                      " _
            '             & "    UPDATE   COM.OIS0000_MEMO                                                                         " _
            '             & "       SET                                                                                      " _
            '             & "         MEMO       = @P1 ,                                                                     " _
            '             & "         UPDYMD     = @P4 ,                                                                     " _
            '             & "         UPDUSER    = @P5 ,                                                                     " _
            '             & "         UPDTERMID  = @P6 ,                                                                     " _
            '             & "         RECEIVEYMD = @P7                                                                       " _
            '             & "     WHERE                                                                                      " _
            '             & "            USERID     = @P2                                                                    " _
            '             & "       And  MAPID      = @P3                                                                    " _
            '             & " IF ( @@FETCH_STATUS <> 0 )                                                                     " _
            '             & "    INSERT INTO COM.OIS0000_MEMO                                                                      " _
            '             & "       (USERID , MAPID , MEMO, DELFLG  ,                                                        " _
            '             & "        INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD)                                    " _
            '             & "        VALUES (@P2,@P3,@P1,@P8,                                                                " _
            '             & "        @P4,@P4,@P5,@P6,@P7) ;                                                                  " _
            '             & " CLOSE hensuu ;                                                                                 " _
            '             & " DEALLOCATE hensuu ; "
            Dim SQLStr As String =
                           "    INSERT INTO COM.OIS0000_MEMO                                                                " _
                         & "       (USERID , MAPID , MEMO, DELFLG  ,                                                        " _
                         & "        INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD)                                    " _
                         & "        VALUES (@P2,@P3,@P1,@P8,                                                                " _
                         & "        @P4,@P4,@P5,@P6,@P7)                                                                    " _
                         & "     ON DUPLICATE KEY UPDATE                                                                    " _
                         & "         MEMO       = @P1 ,                                                                     " _
                         & "         UPDYMD     = @P4 ,                                                                     " _
                         & "         UPDUSER    = @P5 ,                                                                     " _
                         & "         UPDTERMID  = @P6 ,                                                                     " _
                         & "         RECEIVEYMD = @P7                                                                       " _
            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 500).Value = MEMO
                    .Add("@P2", MySqlDbType.VarChar, 20).Value = USERID
                    .Add("@P3", MySqlDbType.VarChar, 50).Value = MAPID
                    .Add("@P4", MySqlDbType.DateTime).Value = Date.Now
                    .Add("@P5", MySqlDbType.VarChar, 20).Value = USERID
                    .Add("@P6", MySqlDbType.VarChar, 30).Value = TERMID
                    .Add("@P7", MySqlDbType.DateTime).Value = C_DEFAULT_YMD
                    .Add("@P8", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.ALIVE
                End With
                SQLcmd.ExecuteNonQuery()
            End Using

            ERR = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0000_MEMO Update"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class
