Option Strict On
Option Explicit On

Imports MySQL.Data.MySqlClient

''' <summary>
''' 営業日報呼び出し
''' </summary>
Public Class LNT0010SelesReport

    ''' <summary>
    '''対象日付
    ''' </summary>
    Public Property TARGETDATE() As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    Public Property CAMPCODE As String
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
    Public Const METHOD_NAME As String = "LNT0010SelesReport"

    ''' <summary>
    ''' 帳票IDから各プログラムを呼び出し
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateReport()
        Dim sm As CS0050SESSION = New CS0050SESSION()
        Dim WW_URL1 As String = ""
        Dim WW_URL2 As String = ""

        '●In PARAMチェック
        'PARAM01: FROMYMD
        If IsNothing(TARGETDATE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TARGETDATE"               '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        Try
            '営業日報(全社)帳票出力処理
            Dim Report As New LNT0010_SelesReport_ALL_DIODOC("LNT0010", "LNT0010_ALL" & ".xlsx")
            Try
                WW_URL1 = Report.CreateExcelPrintData(CAMPCODE, TARGETDATE)
                ERR = C_MESSAGE_NO.NORMAL
            Catch ex As Exception
                Throw
            End Try

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNT0010_ALL Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        Try
            '営業日報(支店別)帳票出力処理
            Dim Report As New LNT0010_SelesReport_SHITEN_DIODOC("LNT0010", "LNT0010_shiten" & ".xlsx")
            Try
                WW_URL2 = Report.CreateExcelPrintData(CAMPCODE, TARGETDATE)
                ERR = C_MESSAGE_NO.NORMAL
            Catch ex As Exception
                Throw
            End Try

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:LNT0010_shiten Select"
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
End Class