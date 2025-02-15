﻿Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 駅マスタ情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0022StationList
    Inherits GL0000

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 組織コード
    ''' </summary>
    ''' <returns></returns>
    Public Property ORGCODE() As String

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
            getStationList(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' 固定値マスタ一覧取得
    ''' </summary>
    Protected Sub getStationList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0002_RECONM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT DISTINCT                 " _
                   & "     RTRIM(A.STATION) AS CODE  , " _
                   & "     RTRIM(A.NAMES)   AS NAMES , " _
                   & "     ''               AS SEQ     " _
                   & " FROM    COM.LNS0020_STATION A   " _
                   & " WHERE                           " _
                   & "         A.DELFLG   <> @P0       " _
                   & "     AND A.CAMPCODE  = @P1       "
            ' 組織コード
            If Not String.IsNullOrEmpty(ORGCODE) Then
                SQLStr &= "     AND A.ORGCODE = @P2    "
            End If
            SQLStr &= " ORDER BY CODE                  "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = CAMPCODE             '会社コード
                    If Not String.IsNullOrEmpty(ORGCODE) Then
                        .Add("@P2", MySqlDbType.VarChar, 20).Value = ORGCODE          '組織コード
                    End If
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0022"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0020_STATION Select"
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

