Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputSAIBU
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoTmp As Integer = 0

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private OutputFileName As String = ""

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <param name="printDataClass">帳票データ</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputFileName = outputFileName
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                          "PRINTFORMAT",
                                                          defaultDatakey,
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
                '今日の日付が先頭のファイル名の場合は残す
                If fileName.StartsWith(keepFilePrefix) Then
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

            'ファイルopen
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "明細書" Then
                    WW_SheetNo = i
                End If
            Next

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData() As String
        'Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            '***** TODO処理 ここまで *****

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(tmpFilePath, SaveFileFormat.Xlsx)
            End SyncLock

            'ストリーム生成
            Using fs As New IO.FileStream(tmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                Dim binaryLength = Convert.ToInt32(fs.Length)
                ReDim retByte(binaryLength)
                fs.Read(retByte, 0, binaryLength)
                fs.Flush()
            End Using
            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
        End Try

    End Function

    ''' <summary>
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Try
            '〇 年月（鏡用）
            Dim lastDate As String = Me.TaishoYYYY + "/" + Me.TaishoMM + "/01"
            lastDate = Date.Parse(lastDate).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
            WW_Workbook.Worksheets(WW_SheetNoTmp).Range("D1").Value = Date.Parse(lastDate)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub
    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea()
        Try
            Dim Formula1 As String = ""
            Dim Formula2 As String = ""
            For Each PrintDatarow As DataRow In PrintData.Select("SETCELL01<>''", "ROWSORTNO")
                '◯ 届先名
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL01").ToString()).Value = PrintDatarow("TODOKENAME").ToString()
                '◯ 単価
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("TANKA").ToString())

                '合計行の編集（エコア合計（1回転＋2回転））
                '変換マスタ（lnm0005_convert）の内容を参照
                If PrintDatarow("TODOKECLASS").ToString = "1" Then
                    Formula1 = "=" & PrintDatarow("SETCELL03").ToString
                    Formula2 = "=" & PrintDatarow("SETCELL04").ToString
                ElseIf PrintDatarow("TODOKECLASS").ToString <> "T" Then
                    Formula1 &= "+" & PrintDatarow("SETCELL03").ToString
                    Formula2 &= "+" & PrintDatarow("SETCELL04").ToString
                End If

                '合計行の場合、上記で編集した数式を設定
                If PrintDatarow("TODOKECLASS").ToString = "T" Then
                    '◯ 台数
                    WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL03").ToString()).Formula = Formula1
                    '◯ 実績数量
                    WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL04").ToString()).Formula = Formula2
                Else
                    '◯ 台数
                    WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL03").ToString()).Value = Double.Parse(PrintDatarow("DAISU").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL04").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString())
                End If
            Next
        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub
End Class
