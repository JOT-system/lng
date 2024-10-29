Option Strict On
Option Explicit On

Imports MySQL.Data.MySqlClient

''' <summary>
''' 発駅・通運別合計表呼び出し
''' </summary>
Public Class PRT0006TransportTotal

    ''' <summary>
    ''' 請求年月
    ''' </summary>
    Public Property TARGETYM() As String
    ''' <summary>
    ''' 支店
    ''' </summary>
    Public Property BRANCHCODE As String
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
    Public Const METHOD_NAME As String = "PRT0006TransportTotal"

    ''' <summary>
    ''' 帳票IDから各プログラムを呼び出し
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateReport()
        Dim sm As CS0050SESSION = New CS0050SESSION()
        Dim WW_URL As String = ""
        Dim OfficeCode As String = ""

        If BRANCHCODE <> "" Then
            If BRANCHCODE <> "011312" AndAlso BRANCHCODE <> "011308" Then
                OfficeCode = BRANCHCODE
            End If
        End If

        '●In PARAMチェック
        'PARAM01: TARGETYM
        If IsNothing(TARGETYM) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TARGETYM"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        '帳票表示データ取得処理
        Dim dt As DataTable = Me.TransportTotalDataGet()
        'データ0件時
        If dt.Rows.Count = 0 Then
            ERR = C_MESSAGE_NO.NO_REPORT_DATA_EXISTS_ERROR
            Exit Sub
        End If

        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                Dim Report As New LNT0012_TransportTotalReport_DIODOC("LNT0012S", "発駅・通運別合計表_TEMPLATE.xlsx", dt)
                Try
                    'WW_URL = Report.CreateExcelPrintData(0, "2", "2", "1")
                    WW_URL = Report.CreateExcelPrintData(0, "1", "1")
                    ERR = C_MESSAGE_NO.NORMAL
                Catch ex As Exception
                    Throw
                End Try
            End Using

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
    ''' 発駅・通運別合計表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function TransportTotalDataGet() As DataTable
        Dim sm As CS0050SESSION = New CS0050SESSION()

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = sm.getConnection
            SQLcon.Open()

            Using SQLcmd As New MySqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "lng.[PRT_TRANSPORTTOTAL]"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@piBRANCHCD", MySqlDbType.VarChar, 6)     ' 支店
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEECD", MySqlDbType.Int32, 5)        ' 受託人
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEESUBCD", MySqlDbType.Int32, 5)     ' 受託人サブ
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@piSTATION", MySqlDbType.Int32, 6)          ' 駅
                'Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@piENDDAY", MySqlDbType.Int32, 2)           ' 締め日
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@piBILLINGYM", MySqlDbType.VarChar, 7)    ' 請求年月
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPFROM", MySqlDbType.Date)               ' 発送年月日(FROM)
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@piSHIPTO", MySqlDbType.Date)                 ' 発送年月日(TO)
                Dim PARA9 As MySqlParameter = SQLcmd.Parameters.Add("@piSORT", MySqlDbType.VarChar, 1)         ' 並び順
                'Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@piBASE", MySqlDbType.VarChar, 1)   　   ' ベース
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@piTRUSTEEKBN", MySqlDbType.VarChar, 1)  ' 受託人絞り込み
                'Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@piBILLINGKBN", MySqlDbType.VarChar, 1)  ' 請求先絞り込み
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@piADDSUBKBN", MySqlDbType.VarChar, 1)   ' 加減額表示設定
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@poMessage", MySqlDbType.VarChar, 1000)  ' メッセージ

                If BRANCHCODE <> "" Then
                    If BRANCHCODE <> "011312" AndAlso BRANCHCODE <> "011308" Then
                        PARA1.Value = BRANCHCODE
                    Else
                        PARA1.Value = DBNull.Value
                    End If
                Else
                    PARA1.Value = DBNull.Value
                End If
                PARA2.Value = DBNull.Value
                PARA3.Value = DBNull.Value
                PARA4.Value = DBNull.Value
                'PARA5.Value = 31
                PARA6.Value = DBNull.Value
                PARA7.Value = New DateTime(Date.Now.Year, Date.Now.Month, 1).ToString("yyyy/MM/dd")
                PARA8.Value = Date.Now.ToString("yyyy/MM/dd")
                PARA9.Value = "1"
                'PARA9.Value = "2"
                'PARA10.Value = "2"
                PARA11.Value = "0"
                'PARA12.Value = "2"
                PARA13.Value = "1"
                PARA14.Direction = ParameterDirection.Output

                SQLcmd.CommandTimeout = 0
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function
End Class