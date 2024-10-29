Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 請求摘要情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0019InvSubCdList
    Inherits GL0000

    ''' <summary>
    ''' 取引先コード入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String

    ''' <summary>
    ''' 請求書提出部店入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property INVFILINGDEPT() As String

    ''' <summary>
    ''' 請求書決済区分入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property INVKESAIKBN() As String

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
            getInvSubCdList(SQLcon)

        End Using

    End Sub

    ''' <summary>
    ''' 請求書決済区分一覧取得
    ''' </summary>
    Protected Sub getInvSubCdList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0025_KEKSBM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                " _
                   & "     RTRIM(A.INVSUBCD)      AS CODE  , " _
                   & "     RTRIM(A.INVSUBCD)      AS NAMES , " _
                   & "     ''                     AS SEQ     " _
                   & " FROM    LNG.LNM0025_KEKSBM A          " _
                   & " WHERE                                 " _
                   & "         A.DELFLG        <> @P0        " _
                   & "     AND A.TORICODE       = @P1        " _
                   & "     AND A.INVFILINGDEPT  = @P2        " _
                   & "     AND A.INVKESAIKBN    = @P3        " _
                   & " ORDER BY CODE                         "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 10).Value = TORICODE            '取引先コード
                    .Add("@P2", MySqlDbType.VarChar, 6).Value = INVFILINGDEPT        '請求書提出部店
                    .Add("@P3", MySqlDbType.VarChar, 2).Value = INVKESAIKBN          '請求書決済区分
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0019"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0025_KEKSBM Select"
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

