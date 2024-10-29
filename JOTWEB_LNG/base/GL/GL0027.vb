Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 帳票マスタ情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0027ReportList
    Inherits GL0000

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
            getReportList(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' 帳票マスタ一覧取得
    ''' </summary>
    Protected Sub getReportList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0023_SHIPPER)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                  " _
                   & "     RTRIM(A.REPORTID)       AS CODE  , " _
                   & "     RTRIM(A.REPORTNAME)     AS NAMES   " _
                   & " FROM    COM.LNS0024_REPORT A           " _
                   & " WHERE                                  " _
                   & "         A.DELFLG   <> @P0              " _
                   & " ORDER BY SORTORDER                     "

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
            CS0011LOGWRITE.INFSUBCLASS = "GL0027"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0024_REPORT Select"
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

