Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' タイトル会社取得
''' </summary>
''' <remarks>CAMP権限によりDB(LNS0005_ROLE)とDB(OIS0011_SRVAUTHOR)を検索して両方許可のある会社コードを取得する。</remarks>
Public Class CS0015TITLEcamp

    ''' <summary>
    ''' 会社コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property List() As ListBox

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0015TITLEcamp"

    ''' <summary>
    ''' タイトルに設定する会社の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0015TITLEcamp()

        '●In PARAMチェック
        'PARAM01: List
        If IsNothing(List) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "List"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        Dim W_OBJ As ListBox = List
        Dim W_OBJ_USER_CAMPCODE As New List(Of String)
        Dim W_OBJ_USER_NAMES As New List(Of String)
        Dim W_OBJ_USER_PERMIT As New List(Of String)

        Dim W_OBJ_SRV_CAMPCODE As New List(Of String)
        Dim W_OBJ_SRV_NAMES As New List(Of String)
        Dim W_OBJ_SRV_PERMIT As New List(Of String)

        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            MySqlConnection.ClearPool(SQLcon)
            '●タイトル会社取得
            '○ User権限によりDB(LNS0005_ROLE)検索
            Try
                'DataBase接続文字
                '検索SQL文
                Dim SQLStr As String =
                     "SELECT rtrim(A.CAMPCODE) as CAMPCODE " _
                   & "     , rtrim(A.NAME) as NAME  " _
                   & "     , rtrim(MAX( B.PERMITCODE )) as PERMITCODE " _
                   & " FROM  LNG.LNM0001_CAMP A " _
                   & " INNER JOIN COM.LNS0005_ROLE B ON " _
                   & "       B.PERMITCODE >= 1 " _
                   & "   and B.DELFLG  <> @P5 " _
                   & " INNER JOIN COM.lns0001_user C  ON " _
                   & "       C.USERID   = @P1 " _
                   & "   and C.CAMPCODE = A.CAMPCODE " _
                   & "   and C.MENUROLE = B.ROLE " _
                   & "   and C.MAPID = B.CODE " _
                   & "   and C.STYMD   <= @P3 " _
                   & "   and C.ENDYMD  >= @P4 " _
                   & "   and C.DELFLG  <> @P5 " _
                   & " WHERE A.DELFLG  <> @P5 " _
                   & "GROUP BY A.CAMPCODE , A.NAME " _
                   & "ORDER BY A.CAMPCODE "

                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 20).Value = USERID
                        '.Add("@P2", MySqlDbType.VarChar, 20).Value = C_ROLE_VARIANT.USER_COMP
                        .Add("@P3", MySqlDbType.Date).Value = Date.Now
                        .Add("@P4", MySqlDbType.Date).Value = Date.Now
                        .Add("@P5", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        Dim i As Integer = 0
                        While SQLdr.Read
                            i = i + 1
                            W_OBJ_USER_CAMPCODE.Add(Convert.ToString(SQLdr("CAMPCODE")))
                            W_OBJ_USER_NAMES.Add(Convert.ToString(SQLdr("NAME")))
                            W_OBJ_USER_PERMIT.Add(Convert.ToString(SQLdr("PERMITCODE")))
                        End While

                        'Close
                        SQLdr.Close() 'Reader(Close)
                    End Using
                End Using
            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:CS0015TITLEcamp Select"           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End Using

        '○出力編集(I_List)
        For i As Integer = 0 To W_OBJ_USER_CAMPCODE.Count - 1
            W_OBJ.Items.Add(New ListItem(W_OBJ_USER_NAMES(i), W_OBJ_USER_CAMPCODE(i)))
        Next i

        'デフォルト選択位置設定
        For i As Integer = 0 To W_OBJ.Items.Count - 1
            If W_OBJ.Items(i).Value = CAMPCODE OrElse i = 0 Then
                W_OBJ.SelectedIndex = i
            End If
        Next

        List = W_OBJ
        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class
