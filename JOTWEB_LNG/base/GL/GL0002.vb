Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 部署情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0002OrgList
    Inherits GL0000
    ''' <summary>
    ''' 権限チェックの要否
    ''' </summary>
    Public Enum LS_AUTHORITY_WITH
        ''' <summary>
        ''' 権限確認無
        ''' </summary>
        NO_AUTHORITY
        ''' <summary>
        ''' 権限確認無/自分の部署に関連するもののみ
        ''' </summary>
        NO_AUTHORITY_WITH_ORG
        ''' <summary>
        ''' 権限確認無/全部署
        ''' </summary>
        NO_AUTHORITY_WITH_ALL
        ''' <summary>
        ''' 権限確認無/会社内全部署
        ''' </summary>
        NO_AUTHORITY_WITH_CMPORG
        ''' <summary>
        ''' ユーザ権限確認
        ''' </summary>
        USER
        ''' <summary>
        ''' 端末権限確認
        ''' </summary>
        MACHINE
        ''' <summary>
        ''' 両権限確認
        ''' </summary>
        BOTH
        ''' <summary>
        ''' 支店のみ
        ''' </summary>
        BRANCH_ONLY
    End Enum

    ''' <summary>
    ''' 部署レベルのカテゴリ
    ''' </summary>
    Public Class C_CATEGORY_LIST
        ''' <summary>
        ''' 営業部門 作業部署 設置部署 車庫
        ''' </summary>
        Public Const CARAGE As String = "車庫"
        ''' <summary>
        ''' 営業部門 管理部門 管理部署 支店
        ''' </summary>　
        Public Const BRANCH_OFFICE As String = "支店"
        ''' <summary>
        ''' 管理部門 所属部署 事業所
        ''' </summary>
        Public Const OFFICE_PLACE As String = "事業所"
        ''' <summary>
        ''' 管理部門 管理部署 受託 部
        ''' </summary>
        Public Const DEPARTMENT As String = "部"
        ''' <summary>
        ''' 管理部門 管理部署　役員
        ''' </summary>
        Public Const OFFICER As String = "役員"
    End Class
    ''' <summary>
    '''　権限チェック区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AUTHWITH() As LS_AUTHORITY_WITH
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' 所属部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 権限フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PERMISSION() As String
    ''' <summary>
    ''' 部署取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>"</remarks>
    Public Property Categorys() As String()
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "GL0002OrgLst"


    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理

        'PARAM 01: Categorys
        If checkParam(METHOD_NAME, Categorys) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM EXTRA01: STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA02: ENDYMD
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
            Select Case AUTHWITH
                Case LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ALL
                    getOrgAllList(SQLcon, "1")
                Case LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG
                    getOrgAllList(SQLcon, "2")
                Case LS_AUTHORITY_WITH.BRANCH_ONLY
                    getOrgBranchList(SQLcon)
                Case Else
                    getOrgList(SQLcon)
            End Select

        End Using

    End Sub

    ''' <summary>
    ''' 部署一覧取得
    ''' </summary>
    Protected Sub getOrgList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用部署取得
        '○ User権限によりDB(LNG.LNM0002_ORG)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                                 " _
                & "   rtrim(A.ORGCODE)     as CODE      ,  " _
                & "   rtrim(A.NAME)        as NAMES     ,  " _
                & "   rtrim(A.ORGCODE)     as CATEGORY  ,  " _
                & "   ''                   as SEQ          " _
                & " FROM       LNG.LNM0002_ORG A           " _
                & " Where                                  " _
                & "         A.CONTROLCODE  = @P2           " _
                & "   and   A.STYMD       <= @P3           " _
                & "   and   A.ENDYMD      >= @P4           " _
                & "   and   A.DELFLG      <> @P5           "
            If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P1 "
            SQLStr = SQLStr & " GROUP BY A.ORGCODE , A.NAME , A.ORGCODE "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.ORGCODE "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case Else
            End Select

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = CAMPCODE
                    .Add("@P2", MySqlDbType.VarChar, 6).Value = ORGCODE
                    .Add("@P3", MySqlDbType.Date).Value = STYMD
                    .Add("@P4", MySqlDbType.Date).Value = ENDYMD
                    .Add("@P5", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0002_ORG Select"
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
    ''' 全部署一覧取得
    ''' </summary>
    Protected Sub getOrgAllList(ByVal SQLcon As MySqlConnection, ByVal COMPANYCODE_FLG As String)
        '●Leftボックス用部署取得
        '○ User権限によりDB(LNG.LNM0002_ORG)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                                 " _
                & "   rtrim(A.ORGCODE)     as CODE      ,  " _
                & "   rtrim(A.NAME)        as NAMES     ,  " _
                & "   rtrim(A.ORGCODE)     as CATEGORY  ,  " _
                & "   ''                   as SEQ          " _
                & " FROM       LNG.LNM0002_ORG A           " _
                & " Where                                  " _
                & "         A.STYMD   <= @P1               " _
                & "   and   A.ENDYMD  >= @P2               " _
                & "   and   A.DELFLG  <> @P3               "

            If COMPANYCODE_FLG <> "1" Then
                If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr &= " and A.CAMPCODE = @P0 "
            End If
            SQLStr = SQLStr & " GROUP BY A.ORGCODE , A.NAME , A.ORGCODE "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.ORGCODE "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case Else
            End Select

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 20).Value = CAMPCODE
                    .Add("@P1", MySqlDbType.Date).Value = STYMD
                    .Add("@P2", MySqlDbType.Date).Value = ENDYMD
                    .Add("@P3", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0002_ORG Select"
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
    ''' 主要支店一覧取得
    ''' </summary>
    Protected Sub getOrgBranchList(ByVal SQLcon As MySqlConnection)
        '●Leftボックス用部署取得
        '○ User権限によりDB(LNG.LNM0002_ORG)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                                   " _
                & "   RTRIM(ORGCODE)     AS CODE      ,      " _
                & "   RTRIM(NAME)        AS NAMES     ,      " _
                & "   RTRIM(ORGCODE)     AS CATEGORY  ,      " _
                & "   ''                 AS SEQ              " _
                & " FROM       LNG.LNM0002_ORG with(nolock)  " _
                & " WHERE                                    " _
                & "         DELFLG   <> @P0                  " _
                & "   AND   CAMPCODE  = @P1                  " _
                & "   AND   STYMD    <= @P2                  " _
                & "   AND   ENDYMD   >= @P3                  " _
                & "   AND   CTNFLG    = '1'                  " _
                & "   AND   CLASS01  IN(1,2,4)               " _
                & " ORDER BY                                 " _
                & "         ORGCODE                          "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = CAMPCODE
                    .Add("@P2", MySqlDbType.Date).Value = STYMD
                    .Add("@P3", MySqlDbType.Date).Value = ENDYMD
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
            CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNM0002_ORG Select"
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
    ''' 一覧登録時のチェック処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function extracheck(ByVal I_SQLDR As MySqlDataReader) As Boolean
        Return (IsNothing(Me.Categorys) OrElse Categorys.Contains(Convert.ToString(I_SQLDR("CATEGORY"))))

    End Function
End Class

