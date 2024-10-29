Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient

''' <summary>
''' 科目別集計表帳票作成クラス
''' </summary>
Public Class LNT0012_AccountSummaryList_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    '出力年月
    Private YearMonth As Date
    Private FormatType As Integer

    Private WW_Workbook As New Workbook
    Private WW_SheetNo As Integer = 0
    Private WW_tmpSheetNo As Integer = 0
    Private WW_InsDate As Date
    Private WW_CampCode As String = ""
    Private WW_KeyYMD As String = ""

    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, YearMonth As Date, FormatType As Integer)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.YearMonth = YearMonth
            Me.FormatType = FormatType
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      mapId, excelFileName)
            Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(Me.UploadRootPath) = False Then
                IO.Directory.CreateDirectory(Me.UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
            Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
            For Each targetFile In targetFiles
                Dim fileName As String = IO.Path.GetFileName(targetFile)
                '今日の日付がファイル名の日付の場合は残す
                If fileName.Contains(keepFilePrefix) Then
                    Continue For
                End If
                Try
                    IO.File.Delete(targetFile)
                Catch ex As Exception
                    '削除時のエラーは無視
                End Try
            Next targetFile
            'URLのルートを表示
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルOPEN
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "科目別集計表" Then
                    WW_SheetNo = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_tmpSheetNo = i
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' 帳票作成
    ''' 
    ''' ※帳票ヘッダ部への出力内容等があるなら引数として渡す
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreateExcelPrintData() As String

        Dim tmpFileName As String = "科目別集計表(" &
            Me.YearMonth.ToString("yyyy年MM月") & ")_" &
            DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '年月
            WW_Workbook.Worksheets(0).Range(1, 4).Value = Me.YearMonth.ToString("yyyy年M月分")

            Dim i As Integer = 4
            Dim DebitTotal As Double = 0
            Dim CreditTotal As Double = 0
            For Each Prow As DataRow In Me.PrintData.Rows

                i += 1
                WW_Workbook.Worksheets(0).Range(i, 0).Value = Prow("ACCOUNTCODE")   '科目コード
                WW_Workbook.Worksheets(0).Range(i, 1).Value = Prow("ACCOUNTNAME")   '科目名称
                WW_Workbook.Worksheets(0).Range(i, 2).Value = Prow("DEBIT_AMOUNT")  '借方金額
                WW_Workbook.Worksheets(0).Range(i, 3).Value = Prow("CREDIT_AMOUNT") '貸方金額
                WW_Workbook.Worksheets(0).Range(i, 4).Value = Prow("DIFFERENCE")    '差額
                '合計算出
                DebitTotal = DebitTotal + CInt(Prow("DEBIT_AMOUNT"))    '借方金額
                CreditTotal = CreditTotal + CInt(Prow("CREDIT_AMOUNT")) '貸方金額

            Next
            '合計
            i += 1
            WW_Workbook.Worksheets(0).Range(i, 1).Value = "合計"
            WW_Workbook.Worksheets(0).Range(i, 2).Value = DebitTotal  '借方金額
            WW_Workbook.Worksheets(0).Range(i, 3).Value = CreditTotal '貸方金額

            '印刷範囲指定
            Dim rowCnt As Integer = PrintData.Rows.Count
            WW_Workbook.Worksheets(0).PageSetup.PrintArea = "$A$1: $E$" + (rowCnt + 6).ToString

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(tmpFilePath, SaveFileFormat.Xlsx)
            End SyncLock

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

End Class
