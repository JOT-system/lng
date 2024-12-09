Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' ファイナンスリース対象項目情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0025FinanceItemList
    Inherits GL0000

    ''' <summary>
    ''' 検索対象項目区分
    ''' </summary>
    Public Enum LS_FINANCEITEM_WITH
        ''' <summary>
        ''' 組織コード
        ''' </summary>
        ORG_CD
    End Enum

    ''' <summary>
    ''' 計上年月入力値
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KEIJYOYM() As String

    ''' <summary>
    ''' 対象項目区分
    ''' </summary>
    ''' <returns></returns>
    Public Property FINANCEITEMWITH() As LS_FINANCEITEM_WITH

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
            Select Case FINANCEITEMWITH
                Case LS_FINANCEITEM_WITH.ORG_CD
                    getKeijyoOrgList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' 営業収入決済条件マスタ取引先一覧取得
    ''' </summary>
    Protected Sub getKeijyoOrgList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                                                " _
                   & "      RTRIM(LEVIES.KEIJOORGCD)                AS CODE  " _
                   & "     ,RTRIM(ORG.NAME)                         AS NAMES " _
                   & "     ,''                                      AS SEQ   " _
                   & " FROM                                                  " _
                   & "     LNG.LNT0065_FL_LEVIES LEVIES                      " _
                   & " INNER JOIN COM.LNS0014_ORG ORG                        " _
                   & "     ON  ORG.CAMPCODE = '01'                           " _
                   & "     AND LEVIES.KEIJOORGCD = ORG.ORGCODE               " _
                   & "     AND CURDATE() BETWEEN ORG.STYMD AND ORG.ENDYMD    " _
                   & "     AND ORG.DELFLG = @P0                              " _
                   & " WHERE                                                 " _
                   & "         LEVIES.DELFLG   = @P0                         " _
                   & "     AND LEVIES.KEIJYOYM = @P1                         " _
                   & " GROUP BY                                              " _
                   & "       LEVIES.KEIJOORGCD                               " _
                   & "     , ORG.NAME                                        " _
                   & " ORDER BY                                              " _
                   & "     CODE                                              "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.ALIVE  '削除フラグ
                    .Add("@P1", MySqlDbType.VarChar, 6).Value = KEIJYOYM            '計上年月
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0025"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0014_ORG Select"
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

