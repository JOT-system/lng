Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' ユーザーマスタ情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0026UserList
    Inherits GL0000

    '''' <summary>
    '''' 承認権限ロール1(第一承認者)
    '''' </summary>
    '''' <returns></returns>
    'Public Property APPROVALID1() As String

    '''' <summary>
    '''' 承認権限ロール2(最終承認者)
    '''' </summary>
    '''' <returns></returns>
    'Public Property APPROVALID2() As String

    ''' <summary>
    ''' ユーザーID
    ''' </summary>
    ''' <returns></returns>
    Public Property USERID() As String

    ''' <summary>
    ''' 組織コード
    ''' </summary>
    ''' <returns></returns>
    Public Property ORGCODE() As String

    '''' <summary>
    '''' 承認権限ロールID
    '''' </summary>
    'Protected Friend Class C_APPROVALID
    '    ''' <summary>
    '    ''' 第一承認者
    '    ''' </summary>
    '    Public Const ROLE_1 As String = "approval_1"
    '    ''' <summary>
    '    ''' 最終承認者
    '    ''' </summary>
    '    Public Const ROLE_2 As String = "approval_2"
    'End Class

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
            getUserList(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' ユーザーマスタ一覧取得
    ''' </summary>
    Public Sub getUserList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用発受託人コード取得
        '○ User権限によりDB(LNM0002_RECONM)検索
        Try
            '検索SQL文
            Dim SQLStr As String
            SQLStr = " SELECT                               " _
                   & "     T01.CODE AS CODE                 " _
                   & "    ,T01.NAMES AS NAMES               " _
                   & " FROM                                 " _
                   & " (SELECT DISTINCT                     " _
                   & "      RTRIM(A.USERID) AS CODE         " _
                   & "     ,RTRIM(A.STAFFNAMEL) AS NAMES    " _
                   & "  FROM    COM.LNS0001_USER A          " _
                   & "  WHERE                               " _
                   & "          A.DELFLG   <> @P0           " _
                   & "      AND @P1 >= A.STYMD              " _
                   & "      AND @P1 <= A.ENDYMD             "
            '' 承認権限ロール
            'If Not String.IsNullOrEmpty(APPROVALID1) And Not String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール1，2どちらも設定されている場合
            '    SQLStr &= "     AND (A.APPROVALID = @P2 OR A.APPROVALID = @P3)   "

            'ElseIf Not String.IsNullOrEmpty(APPROVALID1) And String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール1のみ設定されている場合
            '    SQLStr &= "     AND A.APPROVALID = @P2      "

            'ElseIf String.IsNullOrEmpty(APPROVALID1) And Not String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール2のみ設定されている場合
            '    SQLStr &= "     AND A.APPROVALID = @P3      "
            'End If
            ' ユーザーID
            If Not String.IsNullOrEmpty(USERID) Then
                SQLStr &= "     AND A.USERID  = @P4         "
            End If
            ' 組織コード
            If Not String.IsNullOrEmpty(ORGCODE) Then
                SQLStr &= "     AND A.ORG  = @P5            "
            End If
            SQLStr &= " UNION ALL                           " _
                   & "  SELECT DISTINCT                     " _
                   & "      RTRIM(B.USERID) AS CODE         " _
                   & "     ,RTRIM(B.STAFFNAMEL) AS NAMES    " _
                   & "  FROM                                " _
                   & "      LNG.LNM0002_ORG A               " _
                   & "  INNER JOIN                          " _
                   & "      COM.LNS0001_USER B              " _
                   & "  ON                                  " _
                   & "      B.ORG = A.CONTROLCODE           " _
                   & "  AND B.DELFLG   <> @P0               " _
                   & "  AND @P1 >= B.STYMD                  " _
                   & "  AND @P1 <= B.ENDYMD                 "
            '' 承認権限ロール
            'If Not String.IsNullOrEmpty(APPROVALID1) And Not String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール1，2どちらも設定されている場合
            '    SQLStr &= "     AND (B.APPROVALID = @P2 OR B.APPROVALID = @P3)   "

            'ElseIf Not String.IsNullOrEmpty(APPROVALID1) And String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール1のみ設定されている場合
            '    SQLStr &= "     AND B.APPROVALID = @P2      "

            'ElseIf String.IsNullOrEmpty(APPROVALID1) And Not String.IsNullOrEmpty(APPROVALID2) Then
            '    '承認権限ロール2のみ設定されている場合
            '    SQLStr &= "     AND B.APPROVALID = @P3      "
            'End If
            ' ユーザーID
            If Not String.IsNullOrEmpty(USERID) Then
                SQLStr &= "     AND B.USERID  = @P4         "
            End If
            SQLStr &= " WHERE                               " _
                   & "      A.DELFLG   <> @P0               "
            If Not String.IsNullOrEmpty(ORGCODE) Then
                SQLStr &= "  AND A.ORGCODE = @P5            "
            End If
            SQLStr &= "  )T01                               " _
                   & "  GROUP BY CODE, NAMES                " _
                   & "  ORDER BY CODE                       "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE  '削除フラグ
                    .Add("@P1", MySqlDbType.Date).Value = STYMD                       '開始年月日
                    'If Not String.IsNullOrEmpty(APPROVALID1) Then
                    '    .Add("@P2", MySqlDbType.VarChar, 20).Value = APPROVALID1     '承認権限ロール1
                    'End If
                    'If Not String.IsNullOrEmpty(APPROVALID2) Then
                    '    .Add("@P3", MySqlDbType.VarChar, 20).Value = APPROVALID2     '承認権限ロール2
                    'End If
                    If Not String.IsNullOrEmpty(USERID) Then
                        .Add("@P4", MySqlDbType.VarChar, 20).Value = USERID          'ユーザーID
                    End If
                    If Not String.IsNullOrEmpty(ORGCODE) Then
                        .Add("@P5", MySqlDbType.VarChar, 6).Value = ORGCODE          '組織コード
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0026"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:lns0001_user Select"
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

