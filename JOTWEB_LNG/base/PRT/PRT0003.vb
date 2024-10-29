Option Strict On
Option Explicit On

Imports MySQL.Data.MySqlClient

''' <summary>
'''     コンテナ留置先一覧呼び出し
''' </summary>
Public Class PRT0003PutContainer

    ''' <summary>
    ''' 年月日(開始)
    ''' </summary>
    Public Property FROMYMD() As String
    ''' <summary>
    ''' 年月日(終了)
    ''' </summary>
    Public Property TOYMD() As String
    ''' <summary>
    ''' 支店
    ''' </summary>
    Public Property BRANCHCODE As String
    ''' <summary>
    ''' 処理
    ''' </summary>
    Public Property MODE As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    Public Property USERID As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    Public Property TERMID As String
    ''' <summary>
    ''' 作成URL
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' ERRプロパティ
    ''' </summary>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "PRT0001DailyShipment"

    ''' <summary>
    ''' データ格納用変数
    ''' </summary>
    Public Property ALLDT As DataTable

    ''' <summary>
    ''' 帳票IDから各プログラムを呼び出し
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateReport(LastFLG As String, FirstFLG As String, BEDT As DataTable)
        Dim sm As CS0050SESSION = New CS0050SESSION()
        Dim WW_URL As String = ""
        Dim OfficeCode As String = ""

        If BRANCHCODE <> "" Then
            If BRANCHCODE <> "011312" AndAlso BRANCHCODE <> "011308" Then
                OfficeCode = BRANCHCODE
            End If
        End If

        '●In PARAMチェック
        'PARAM02: TOYMD
        If IsNothing(TOYMD) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TOYMD"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        '帳票表示データ取得処理
        Dim dt As DataTable = Me.PutContainerDataGet()
        'データ0件時
        If dt.Rows.Count = 0 AndAlso
           BEDT Is Nothing Then
            ERR = C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR
            Exit Sub
        End If

        If BEDT Is Nothing Then
            Me.ALLDT = dt
            BEDT = dt
        Else
            For Each dtRow As DataRow In dt.Rows
                BEDT.ImportRow(dtRow)
            Next
            Me.ALLDT = BEDT
        End If

        Try
            If LastFLG = "1" Then
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)
                    MySqlConnection.ClearPool(SQLcon)
                    Dim Report As New LNT0012_PutContainerReport_DIODOC("LNT0012S", "コンテナ留置先一覧_TEMPLATE.xlsx", BEDT)
                    Try
                        WW_URL = Report.CreateExcelPrintData(OfficeCode, "1")
                        ERR = C_MESSAGE_NO.NORMAL
                    Catch ex As Exception
                        Throw
                    End Try
                End Using
            End If

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0005_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '作成されたURLを返す
        If isNormal(ERR) Then
            URL = WW_URL
        End If

    End Sub

    ''' <summary>
    ''' コンテナ留置先一覧データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function PutContainerDataGet() As DataTable
        Dim sm As CS0050SESSION = New CS0050SESSION()

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        'コンテナ留置先一覧作成用ワークファイル作成
        Using SQLcon As MySqlConnection = sm.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[INS_PUTCTNDATA]"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piFROM", MySqlDbType.Date)             ' 開始日
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTO", MySqlDbType.Date)               ' 終了日
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piMODE", MySqlDbType.VarChar, 1)       ' 処理
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDUSER", MySqlDbType.VarChar, 20)   ' ユーザ
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piUPDTERMID", MySqlDbType.VarChar, 20) ' 端末
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                PARA1.Value = CDate(FROMYMD)
                PARA2.Value = CDate(TOYMD)
                PARA3.Value = MODE
                PARA4.Value = USERID
                PARA5.Value = TERMID
                PARA6.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                SQLcmd.ExecuteReader()

            End Using

        End Using

        'コンテナ留置先一覧データ取得
        Using SQLcon As MySqlConnection = sm.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_PUTCTNDATA]"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piOFFICECODE", MySqlDbType.VarChar, 6) ' 支店
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If BRANCHCODE <> "" Then
                    If BRANCHCODE <> "011312" AndAlso BRANCHCODE <> "011308" Then
                        PARA1.Value = BRANCHCODE
                    Else
                        PARA1.Value = DBNull.Value
                    End If
                Else
                    PARA1.Value = DBNull.Value
                End If
                PARA2.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function
End Class