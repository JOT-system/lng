Option Strict On
Option Explicit On

Imports MySQL.Data.MySqlClient

''' <summary>
''' 帳票呼び出し
''' </summary>
Public Class PRT0000ReportCall

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    Public Property REPORTID() As String
    ''' <summary>
    ''' 処理日
    ''' </summary>
    Public Property TARGETDATE() As Date
    ''' <summary>
    ''' 会社
    ''' </summary>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 支店
    ''' </summary>
    Public Property BRANCHCODE() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    Public Property USERID As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    Public Property TERMID As String
    ''' <summary>
    ''' 作成URL1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL1() As String
    ''' <summary>
    ''' 作成URL2
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL2() As String

    ''' <summary>
    ''' ERRプロパティ
    ''' </summary>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "PRT0000ReportCall"

    ''' <summary>
    ''' データ格納用変数
    ''' </summary>
    Public Property ALLDT As DataTable

    ''' <summary>
    ''' データ格納用変数
    ''' </summary>
    Public Property ALLDT2 As DataTable

    ''' <summary>
    ''' 帳票IDから各プログラムを呼び出し
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReportCall(LastFLG As String, FirstFLG As String, BEDT As DataTable, BEDT2 As DataTable)
        Dim sm As CS0050SESSION = New CS0050SESSION()
        Dim WW_URL1 As String = ""
        Dim WW_URL2 As String = ""
        Dim WW_DATE As String = Format(TARGETDATE, "yyyy/MM/dd")
        '●In PARAMチェック
        'PARAM01: REPORTID
        If IsNothing(REPORTID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM 02: TARGETDATE
        If IsNothing(TARGETDATE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TARGETDATE"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM 03: ORGCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM 04: BRANCHCODE
        If IsNothing(BRANCHCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "BRANCHCODE"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM 05: USERID
        If IsNothing(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        Dim WW_REPORT_CHECK As Integer = 0
        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                '●帳票マスタ存在チェック
                WW_REPORT_CHECK = CheckReportId(SQLcon, REPORTID)
            End Using

            If WW_REPORT_CHECK = 1 Then
                '帳票ID存在
                Select Case REPORTID
                    '〇発送日報
                    Case "PRT0001A"
                        Dim PRT0001DailyShipment As New PRT0001DailyShipment
                        PRT0001DailyShipment.FROMYMD = WW_DATE          '年月日(開始)
                        PRT0001DailyShipment.TOYMD = WW_DATE            '年月日(終了)
                        PRT0001DailyShipment.BRANCHCODE = BRANCHCODE    '支店
                        PRT0001DailyShipment.CreateReport(LastFLG, FirstFLG, BEDT, BEDT2)
                        Me.ALLDT = PRT0001DailyShipment.ALLDT
                        Me.ALLDT2 = PRT0001DailyShipment.ALLDT2
                        If isNormal(PRT0001DailyShipment.ERR) Then
                            WW_URL1 = PRT0001DailyShipment.URL1
                            If PRT0001DailyShipment.URL2 <> "" Then
                                WW_URL2 = PRT0001DailyShipment.URL2
                            End If
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = PRT0001DailyShipment.ERR
                        End If

                    '〇他駅発送明細
                    Case "PRT0002"
                        Dim PRT0002OtherStation As New PRT0002OtherStation
                        PRT0002OtherStation.FROMYMD = WW_DATE          '年月日(開始)
                        PRT0002OtherStation.TOYMD = WW_DATE            '年月日(終了)
                        PRT0002OtherStation.BRANCHCODE = BRANCHCODE    '支店
                        PRT0002OtherStation.CreateReport(LastFLG, FirstFLG, BEDT)
                        Me.ALLDT = PRT0002OtherStation.ALLDT
                        If isNormal(PRT0002OtherStation.ERR) Then
                            WW_URL1 = PRT0002OtherStation.URL
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = PRT0002OtherStation.ERR
                        End If

                    '〇コンテナ留置先一覧
                    Case "PRT0003"
                        Dim PRT0003PutContainer As New PRT0003PutContainer
                        PRT0003PutContainer.FROMYMD = WW_DATE          '年月日(開始)  
                        PRT0003PutContainer.TOYMD = WW_DATE            '年月日(終了)
                        PRT0003PutContainer.BRANCHCODE = BRANCHCODE    '支店
                        PRT0003PutContainer.MODE = "1"                 '処理
                        PRT0003PutContainer.USERID = USERID            'ユーザID
                        PRT0003PutContainer.TERMID = TERMID            '端末ID
                        PRT0003PutContainer.CreateReport(LastFLG, FirstFLG, BEDT)
                        Me.ALLDT = PRT0003PutContainer.ALLDT
                        If isNormal(PRT0003PutContainer.ERR) Then
                            WW_URL1 = PRT0003PutContainer.URL
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = PRT0003PutContainer.ERR
                        End If

                    '〇品目別販売実績表 冷＆Ｓ
                    Case "PRT0004"
                        Dim PRT0004SalesResults As New PRT0004SalesResults
                        PRT0004SalesResults.FROMYMD = WW_DATE          '年月日(開始)
                        PRT0004SalesResults.TOYMD = WW_DATE            '年月日(終了)
                        PRT0004SalesResults.BRANCHCODE = BRANCHCODE    '支店
                        PRT0004SalesResults.CreateReport(LastFLG, FirstFLG, BEDT)
                        Me.ALLDT = PRT0004SalesResults.ALLDT
                        If isNormal(PRT0004SalesResults.ERR) Then
                            WW_URL1 = PRT0004SalesResults.URL
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = PRT0004SalesResults.ERR
                        End If

                        '〇コンテナ動静表
                        'Case "PRT0005"
                        'Dim PRT0005MovementContainer As New PRT0005MovementContainer
                        'WW_DATE = Date.Now.ToString("yyyy/MM/dd")
                        'PRT0005MovementContainer.TARGETYMD = WW_DATE        '処理日
                        'PRT0005MovementContainer.BRANCHCODE = BRANCHCODE    '支店
                        'PRT0005MovementContainer.JURISDICTION = "1"         '所管部
                        'PRT0005MovementContainer.USERID = USERID            'ユーザID
                        'PRT0005MovementContainer.TERMID = TERMID            '端末ID
                        'PRT0005MovementContainer.CreateReport()
                        'If isNormal(PRT0005MovementContainer.ERR) Then
                        'WW_URL = PRT0005MovementContainer.URL
                        'ERR = C_MESSAGE_NO.NORMAL
                        'Else
                        'ERR = PRT0005MovementContainer.ERR
                        'End If

                    '〇発駅・通運別合計表
                    Case "PRT0006"
                        Dim PRT0006TransportTotal As New PRT0006TransportTotal
                        WW_DATE = Date.Now.AddMonths(-1).ToString("yyyy/MM")
                        PRT0006TransportTotal.TARGETYM = WW_DATE         '請求年月
                        PRT0006TransportTotal.BRANCHCODE = BRANCHCODE    '支店
                        PRT0006TransportTotal.CreateReport()
                        If isNormal(PRT0006TransportTotal.ERR) Then
                            WW_URL1 = PRT0006TransportTotal.URL
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = PRT0006TransportTotal.ERR
                        End If

                        '〇リース料明細チェックリスト
                        'Case "PRT0007"
                        'Dim PRT0007LeaseFee As New PRT0007LeaseFee
                        'WW_DATE = Date.Now.ToString("yyyy/MM")
                        'PRT0007LeaseFee.TARGETYM = WW_DATE         '対象年月
                        'PRT0007LeaseFee.BRANCHCODE = BRANCHCODE    '支店
                        'PRT0007LeaseFee.TYPE = "1"                 '帳票タイプ
                        'PRT0007LeaseFee.CreateReport()
                        'If isNormal(PRT0007LeaseFee.ERR) Then
                        'WW_URL = PRT0007LeaseFee.URL
                        'ERR = C_MESSAGE_NO.NORMAL
                        'Else
                        'ERR = PRT0007LeaseFee.ERR
                        'End If

                        '〇支店間流動表(金額)
                        'Case "PRT0008"
                        'Dim PRT0008InterBranchAmount As New PRT0008InterBranchAmount
                        'PRT0008InterBranchAmount.FROMYMD = WW_DATE          '年月日(開始)
                        'PRT0008InterBranchAmount.TOYMD = WW_DATE            '年月日(終了)
                        'PRT0008InterBranchAmount.STACKFREE = 1              '積空区分
                        'PRT0008InterBranchAmount.CreateReport()
                        'If isNormal(PRT0008InterBranchAmount.ERR) Then
                        'WW_URL = PRT0008InterBranchAmount.URL
                        'ERR = C_MESSAGE_NO.NORMAL
                        'Else
                        'ERR = PRT0008InterBranchAmount.ERR
                        'End If

                        '〇支店間流動表(個数)
                        'Case "PRT0009"
                        'Dim PRT0009InterBranchQuantity As New PRT0009InterBranchQuantity
                        'PRT0009InterBranchQuantity.FROMYMD = WW_DATE          '年月日(開始)
                        'PRT0009InterBranchQuantity.TOYMD = WW_DATE            '年月日(終了)
                        'PRT0009InterBranchQuantity.CreateReport()
                        'If isNormal(PRT0009InterBranchQuantity.ERR) Then
                        'WW_URL = PRT0009InterBranchQuantity.URL
                        'ERR = C_MESSAGE_NO.NORMAL
                        'Else
                        'ERR = PRT0009InterBranchQuantity.ERR
                        'End If

                        '〇発駅・通運別合計表(期間)
                        'Case "PRT0010"
                        'Dim PRT0010TransportTotalPeriod As New PRT0010TransportTotalPeriod
                        'WW_DATE = Date.Now.ToString("yyyy/MM")
                        'PRT0010TransportTotalPeriod.FROMYM = WW_DATE           '請求年月(開始)
                        'PRT0010TransportTotalPeriod.TOYM = WW_DATE             '請求年月(終了)
                        'PRT0010TransportTotalPeriod.BRANCHCODE = BRANCHCODE    '支店
                        'PRT0010TransportTotalPeriod.CreateReport()
                        'If isNormal(PRT0010TransportTotalPeriod.ERR) Then
                        'WW_URL = PRT0010TransportTotalPeriod.URL
                        'ERR = C_MESSAGE_NO.NORMAL
                        'Else
                        'ERR = PRT0010TransportTotalPeriod.ERR
                        'End If

                    ' 営業日報
                    Case "LNT0010_ALL"
                        Dim LNT0010SelesReport As New LNT0010SelesReport
                        LNT0010SelesReport.TARGETDATE = WW_DATE       '年月日(開始)
                        LNT0010SelesReport.CAMPCODE = CAMPCODE        '会社コード
                        LNT0010SelesReport.CreateReport()
                        If isNormal(LNT0010SelesReport.ERR) Then
                            WW_URL1 = LNT0010SelesReport.URL1
                            WW_URL2 = LNT0010SelesReport.URL2
                            ERR = C_MESSAGE_NO.NORMAL
                        Else
                            ERR = LNT0010SelesReport.ERR
                        End If

                End Select

            Else
                '帳票ID未存在
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:LNS0024_REPORT Select"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = "帳票ID未存在"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End If

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0024_REPORT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '作成されたURLを返す
        If isNormal(ERR) Then
            URL1 = WW_URL1
            URL2 = WW_URL2
        End If

    End Sub

    ''' <summary>
    ''' 帳票IDから帳票マスタを取得する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="REPORTID"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function CheckReportId(ByVal SQLcon As MySqlConnection, ByVal REPORTID As String) As Integer
        Dim WW_Check As Integer = 0
        '検索SQL文
        Try
            Dim SQLStr As String =
                 " SELECT " _
               & "              rtrim(A.REPORTID)    AS REPORTID   " _
               & " FROM        COM.LNS0024_REPORT                A             " _
               & " WHERE                                               " _
               & "           A.REPORTID    = @P1                       " _
               & "       and A.DELFLG     <> @P2                       " _
               & " ORDER BY A.SORTORDER                                "

            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = REPORTID
                    .Add("@P2", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR

                If SQLdr.Read Then
                    WW_Check = 1
                    ERR = C_MESSAGE_NO.NORMAL
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNS0024_REPORT Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            CheckReportId = 0
            Exit Function
        End Try
        CheckReportId = WW_Check
    End Function

End Class