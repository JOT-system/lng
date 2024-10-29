Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' コンテナ取引先情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0017CtnCustomerList
    Inherits GL0000
    ''' <summary>
    ''' コンテナ取引先チェックの要否
    ''' </summary>
    Public Enum LS_CUSTOMER_WITH
        ''' <summary>
        ''' 受託人コード
        ''' </summary>
        TRUSTEE_CD
        ''' <summary>
        ''' 受託人サブコード
        ''' </summary>
        TRUSTEE_SUBCD
    End Enum

    ''' <summary>
    ''' コンテナ取引先チェック区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CUSTOMERWITH() As LS_CUSTOMER_WITH

    ''' <summary>
    ''' 駅コード入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STATION() As String

    ''' <summary>
    ''' 受託人コード入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TRUSTEECD() As String

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
            Select Case CUSTOMERWITH
                ' 受託人コード
                Case LS_CUSTOMER_WITH.TRUSTEE_CD
                    getDepTrusteeCdList(SQLcon)
                ' 受託人サブコード
                Case LS_CUSTOMER_WITH.TRUSTEE_SUBCD
                    getDepTrusteeSubCdList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' 受託人コード一覧取得
    ''' </summary>
    Protected Sub getDepTrusteeCdList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0003_REKEJM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT DISTINCT                          "
            SQLStr &= "     RTRIM(A.DEPTRUSTEECD)    AS CODE  , "
            SQLStr &= "     RTRIM(A.DEPTRUSTEENM)    AS NAMES , "
            SQLStr &= "     ''                       AS SEQ     "
            SQLStr &= " FROM    LNG.LNM0003_REKEJM A            "
            SQLStr &= " WHERE                                   "
            SQLStr &= "         A.DELFLG     <> @P0             "

            If Not String.IsNullOrEmpty(STATION) Then
                SQLStr &= "     AND A.DEPSTATION  = @P1             "
            End If

            SQLStr &= " ORDER BY CODE                           "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 6).Value = STATION           '発駅コード
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0017"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0003_REKEJM Select"
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
    ''' 受託人サブコード一覧取得
    ''' </summary>
    Protected Sub getDepTrusteeSubCdList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人サブコード取得
        '○ User権限によりDB(LNM0003_REKEJM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                  "
            SQLStr &= "     RTRIM(A.DEPTRUSTEESUBCD) AS CODE  , "
            SQLStr &= "     RTRIM(A.DEPTRUSTEESUBNM) AS NAMES , "
            SQLStr &= "     ''                       AS SEQ     "
            SQLStr &= " FROM    LNG.LNM0003_REKEJM A            "
            SQLStr &= " WHERE                                   "
            SQLStr &= "          A.DELFLG       <> @P0          "

            If Not String.IsNullOrEmpty(STATION) Then
                SQLStr &= "      AND A.DEPSTATION    = @P1          "
            End If
            If Not String.IsNullOrEmpty(TRUSTEECD) Then
                SQLStr &= "      AND A.DEPTRUSTEECD  = @P2          "
            End If

            SQLStr &= " ORDER BY CODE                           "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 6).Value = STATION           '発駅コード
                    .Add("@P2", MySqlDbType.VarChar, 5).Value = TRUSTEECD         '発受託人コード
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0017"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0003_REKEJM Select"
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

