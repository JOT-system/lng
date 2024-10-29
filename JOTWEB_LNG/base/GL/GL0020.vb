Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' コンテナ情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0020ContenaList
    Inherits GL0000
    ''' <summary>
    ''' コンテナチェックの要否
    ''' </summary>
    Public Enum LS_CONTENA_WITH
        ''' <summary>
        ''' コンテナ記号
        ''' </summary>
        CTN_TYPE
        ''' <summary>
        ''' コンテナ番号
        ''' </summary>
        CTN_NO
    End Enum

    ''' <summary>
    ''' コンテナチェック区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CONTENAWITH() As LS_CONTENA_WITH

    ''' <summary>
    ''' コンテナ記号
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CTNTYPE() As String

    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        Try
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try
        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            MySqlConnection.ClearPool(SQLcon)
            Select Case CONTENAWITH
                ' コンテナ記号
                Case LS_CONTENA_WITH.CTN_TYPE
                    getCTNTypeList(SQLcon)
                ' コンテナ番号
                Case LS_CONTENA_WITH.CTN_NO
                    getCTNNoList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' コンテナ記号一覧取得
    ''' </summary>
    Protected Sub getCTNTypeList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0002_RECONM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                               " _
                   & "     RTRIM(VALUE1) AS CODE            " _
                   & "    ,RTRIM(VALUE1) AS NAMES           " _
                   & "    ,''            AS SEQ             " _
                   & " FROM                                 " _
                   & "     COM.LNS0006_FIXVALUE             " _
                   & " WHERE                                " _
                   & "     VALUE2  = '1'                    " _
                   & " AND CLASS   = 'ACCOUNTINGASSETSCD'   " _
                   & " AND DELFLG <> @P0                    " _
                   & " ORDER BY CODE                        "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                End With

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○出力編集
                    addListData(SQLdr)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0020"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0002_RECONM Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' コンテナ番号一覧取得
    ''' </summary>
    Protected Sub getCTNNoList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人サブコード取得
        '○ User権限によりDB(LNM0002_RECONM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                        " _
                   & "     RTRIM(A.CTNNO) AS CODE  , " _
                   & "     RTRIM(A.CTNNO) AS NAMES , " _
                   & "     ''             AS SEQ     " _
                   & " FROM    LNG.LNM0002_RECONM A  " _
                   & " WHERE                         " _
                   & "         A.DELFLG  <> @P0      " _
                   & "     AND A.CTNTYPE  = @P1      " _
                   & " ORDER BY CODE                 "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 5).Value = CTNTYPE              'コンテナ記号
                End With

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○出力編集
                    addListData(SQLdr)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0020"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0002_RECONM Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class

