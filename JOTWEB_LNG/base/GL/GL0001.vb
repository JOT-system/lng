﻿'Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 会社情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0001CompList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_COMPANY_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' ロール指定
        ''' </summary>
        ROLE
    End Enum
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPEMODE As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String

    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "GS0001CAMPget"


    ''' <summary>
    ''' 会社情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM EXTRA01: ROLECODE
        If checkParam(METHOD_NAME, ROLECODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
            ENDYMD = Date.Now
        End If

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
            Select Case TYPEMODE
                Case CInt(LC_COMPANY_TYPE.ROLE).ToString
                    getCompList(SQLcon)
                Case CInt(LC_COMPANY_TYPE.ALL).ToString
                    getCompAllList(SQLcon)
            End Select

        End Using
    End Sub
    ''' <summary>
    ''' 全会社一覧取得
    ''' </summary>
    Protected Sub getCompAllList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用会社取得
        '○　DB(LNG.LNM0001_CAMP)検索
        Try

            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                        " _
                & " rtrim(A.CAMPCODE) as CODE  ,  " _
                & " rtrim(A.NAME)    as NAMES    " _
                & " FROM  LNG.LNM0001_CAMP A        " _
                & " WHERE                         " _
                & "       A.STYMD   <= @P4        " _
                & "   and A.ENDYMD  >= @P3        " _
                & "   and A.DELFLG  <> @P5        " _
                & " GROUP BY A.CAMPCODE , A.NAME "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.CAMPCODE, A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.CAMPCODE "
                Case C_DEFAULT_SORT.SEQ
                Case Else
            End Select

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.Date)
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 1)
                PARA3.Value = STYMD
                PARA4.Value = ENDYMD
                PARA5.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0001"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0001_CAMP Select"
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
    ''' 会社一覧取得
    ''' </summary>
    Protected Sub getCompList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(LNG.LNM0001_CAMP)検索
        Try

            '検索SQL文
            Dim SQLStr As String =
                    "SELECT " _
                & " rtrim(A.CAMPCODE) as CODE ," _
                & " rtrim(A.NAME) as NAMES " _
                & " FROM  LNG.LNM0001_CAMP A " _
                & " INNER JOIN COM.LNS0005_ROLE B ON " _
                & "       B.CAMPCODE     = A.CAMPCODE " _
                & "   and B.ROLE     = @P1 " _
                & "   and B.STYMD   <= @P4 " _
                & "   and B.ENDYMD  >= @P3 " _
                & "   and B.DELFLG  <> @P5 " _
                & " Where " _
                & "       A.STYMD   <= @P4 " _
                & "   and A.ENDYMD  >= @P3 " _
                & "   and A.DELFLG  <> @P5 " _
                & " GROUP BY A.CAMPCODE , A.NAME "
            '    "SELECT " _
            '& " rtrim(A.CAMPCODE) as CODE ," _
            '& " rtrim(A.NAMES) as NAMES " _
            '& " FROM  LNG.LNM0001_CAMP A " _
            '& " INNER JOIN COM.LNS0005_ROLE B ON " _
            '& "       B.CODE     = A.CAMPCODE " _
            '& "   and B.ROLE     = @P1 " _
            '& "   and B.OBJECT   = @P2 " _
            '& "   and B.STYMD   <= @P4 " _
            '& "   and B.ENDYMD  >= @P3 " _
            '& "   and B.DELFLG  <> @P5 " _
            '& " Where " _
            '& "       A.STYMD   <= @P4 " _
            '& "   and A.ENDYMD  >= @P3 " _
            '& "   and A.DELFLG  <> @P5 " _
            '& " GROUP BY A.CAMPCODE , A.NAMES "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.CAMPCODE, A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.CAMPCODE "
                Case C_DEFAULT_SORT.SEQ
                Case Else
            End Select
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 20)
                '                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 20)
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.Date)
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 1)
                PARA1.Value = ROLECODE
                '                PARA2.Value = C_ROLE_VARIANT.USER_COMP
                PARA3.Value = STYMD
                PARA4.Value = ENDYMD
                PARA5.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0001"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0001_CAMP Select"
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

