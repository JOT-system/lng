Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 大中小分類情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0016ClassList
    Inherits GL0000
    ''' <summary>
    ''' 権限チェックの要否
    ''' </summary>
    Public Enum LS_CLASS_WITH
        ''' <summary>
        ''' 大分類
        ''' </summary>
        BIG_CLASS
        ''' <summary>
        ''' 中分類
        ''' </summary>
        MIDDLE_CLASS
        ''' <summary>
        ''' 小分類
        ''' </summary>
        SMALL_CLASS
    End Enum

    ''' <summary>
    ''' 分類チェック区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLASSWITH() As LS_CLASS_WITH
    ''' <summary>
    ''' 大分類入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BIGCTNCD() As String
    ''' <summary>
    ''' 中分類入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MIDDLECTNCD() As String

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
            Select Case CLASSWITH
                ' 大分類
                Case LS_CLASS_WITH.BIG_CLASS
                    getBigClassList(SQLcon)
                ' 中分類
                Case LS_CLASS_WITH.MIDDLE_CLASS
                    getMiddleClassList(SQLcon)
                ' 小分類
                Case LS_CLASS_WITH.SMALL_CLASS
                    getSmallClassList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' 大分類一覧取得
    ''' </summary>
    Protected Sub getBigClassList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用分類取得
        '○ User権限によりDB(LNM0022_CLASS)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT DISTINCT                        " _
                   & "     RTRIM(A.BIGCTNCD)       AS CODE  , " _
                   & "     RTRIM(A.KANJI1)         AS NAMES , " _
                   & "     ''                      AS SEQ     " _
                   & " FROM    LNG.LNM0022_CLASS A            " _
                   & " WHERE                                  " _
                   & "     A.DELFLG <> @P0                    " _
                   & " ORDER BY CODE                          "

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
            CS0011LOGWRITE.INFSUBCLASS = "GL0016"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0022_CLASS Select"
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
    ''' 中分類一覧取得
    ''' </summary>
    Protected Sub getMiddleClassList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用分類取得
        '○ User権限によりDB(LNM0022_CLASS)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT DISTINCT                        " _
                   & "     RTRIM(A.MIDDLECTNCD)    AS CODE  , " _
                   & "     RTRIM(A.KANJI2)         AS NAMES , " _
                   & "     ''                      AS SEQ     " _
                   & " FROM    LNG.LNM0022_CLASS A            " _
                   & " WHERE                                  " _
                   & "         A.DELFLG   <> @P0              " _
                   & "     AND A.BIGCTNCD  = @P1              " _
                   & " ORDER BY CODE                          "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 2).Value = BIGCTNCD             '大分類コード
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0016"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0022_CLASS Select"
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
    ''' 小分類一覧取得
    ''' </summary>
    Protected Sub getSmallClassList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用分類取得
        '○ User権限によりDB(LNM0022_CLASS)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                 " _
                   & "     RTRIM(A.SMALLCTNCD)     AS CODE  , " _
                   & "     RTRIM(A.KANJI3)         AS NAMES , " _
                   & "     ''                      AS SEQ     " _
                   & " FROM    LNG.LNM0022_CLASS A            " _
                   & " WHERE                                  " _
                   & "         A.DELFLG      <> @P0           " _
                   & "     AND A.BIGCTNCD     = @P1           " _
                   & "     AND A.MIDDLECTNCD  = @P2           " _
                   & " ORDER BY CODE                          "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 2).Value = BIGCTNCD             '大分類コード
                    .Add("@P2", MySqlDbType.VarChar, 2).Value = MIDDLECTNCD          '中分類コード
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0016"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0022_CLASS Select"
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

