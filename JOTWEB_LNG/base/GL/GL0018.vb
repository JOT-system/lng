Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 営業収入決済条件情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0018InvKesaiKbnList
    Inherits GL0000
    ''' <summary>
    ''' 営業収入決済条件取得項目区分
    ''' </summary>
    Public Enum LS_INVOICE_WITH
        ''' <summary>
        ''' 取引先コード
        ''' </summary>
        TORICODE
        ''' <summary>
        ''' 請求書決済区分
        ''' </summary>
        INV_KESAI_KBN
    End Enum

    ''' <summary>
    ''' 営業収入決済条件項目区分
    ''' </summary>
    ''' <returns></returns>
    Public Property INVOICEWITH() As LS_INVOICE_WITH

    ''' <summary>
    ''' 取引先コード入力値
    ''' </summary>
    ''' <returns></returns>
    Public Property TORICODE() As String

    ''' <summary>
    ''' 請求書提出部店入力値
    ''' </summary>
    ''' <returns></returns>
    Public Property INVFILINGDEPT() As String

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
            Select Case INVOICEWITH
                Case LS_INVOICE_WITH.TORICODE
                    getToriCodeList(SQLcon)
                Case LS_INVOICE_WITH.INV_KESAI_KBN
                    getInvKesaiKbnList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' 取引先コード一覧取得
    ''' </summary>
    Protected Sub getToriCodeList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0024_KEKKJM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                                     " _
                   & "     RTRIM(MAX(A.TORICODE)) AS CODE  ,                      " _
                   & "     RTRIM(MAX(A.TORINAME + coalesce(A.TORIDIVNAME,''))) AS NAMES , " _
                   & "     ''                          AS SEQ                     " _
                   & " FROM    LNG.LNM0024_KEKKJM A                               " _
                   & " WHERE                                                      " _
                   & "     A.DELFLG <> @P0                                        " _
                   & " GROUP BY                                                   " _
                   & "     A.TORICODE                                             " _
                   & "   , A.TORINAME                                             " _
                   & " ORDER BY CODE                                              "

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
            CS0011LOGWRITE.INFSUBCLASS = "GL0018"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0024_KEKKJM Select"
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
    ''' 請求書決済区分一覧取得
    ''' </summary>
    Protected Sub getInvKesaiKbnList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0024_KEKKJM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                                 " _
                   & "     RTRIM(A.INVKESAIKBN)                    AS CODE  , " _
                   & "     RTRIM(A.INVKESAIKBN)                    AS NAMES , " _
                   & "     ''                                      AS SEQ     " _
                   & " FROM    LNG.LNM0024_KEKKJM A                           " _
                   & " WHERE                                                  " _
                   & "         A.DELFLG        <> @P0                         " _
                   & "     AND A.TORICODE       = @P1                         " _
                   & "     AND A.INVFILINGDEPT  = @P2                         " _
                   & " ORDER BY CODE                                          "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 10).Value = TORICODE            '取引先コード
                    .Add("@P2", MySqlDbType.VarChar, 6).Value = INVFILINGDEPT        '請求書提出部店
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0018"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0024_KEKKJM Select"
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

