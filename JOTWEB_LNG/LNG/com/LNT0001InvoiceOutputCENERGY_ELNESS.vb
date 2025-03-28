Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputCENERGY_ELNESS
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    'Private WW_SheetNoSKKoteihi As Integer = 0
    'Private WW_SheetNoUnchin As Integer = 0
    Private WW_SheetNoCalendar As Integer = 0
    Private WW_SheetNoMaster As Integer = 0

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
    Private PrintCalendarData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private OutputOrgCode As String = ""
    Private OutputFileName As String = ""
    Private calcZissekiNumber As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <param name="printDataClass">帳票データ</param>
    ''' <param name="printTankDataClass"></param>
    ''' <param name="printKoteihiDataClass">固定費マスタ</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printCalendarDataClass As DataTable,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal calcNumber As Integer = 1,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTankData = printTankDataClass
            Me.PrintKoteihiData = printKoteihiDataClass
            Me.PrintCalendarData = printCalendarDataClass
            'Me.PrintSKKoteichiData = printSKKoteichiDataClass
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputOrgCode = orgCode
            Me.OutputFileName = outputFileName
            Me.calcZissekiNumber = calcNumber

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

            Dim j As Integer() = {0, 0, 0, 0, 0}
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "入力表" Then
                    'ElseIf WW_Workbook.Worksheets(i).Name = "寺岡製作所（相馬出荷・東北）" Then
                    '    WW_SheetNo03(0) = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "鶴岡ガス（相馬出荷・東北）" Then
                    '    WW_SheetNo03(1) = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "若松ガス（相馬出荷・東北）" Then
                    '    WW_SheetNo03(2) = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "固定運賃" Then
                    '    '〇共通(シート[固定運賃])
                    '    WW_SheetNoSKKoteihi = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "従量運賃" Then
                    '    '〇共通(シート[従量運賃])
                    '    WW_SheetNoUnchin = i
                ElseIf WW_Workbook.Worksheets(i).Name = "301" Then
                    '〇シーエナジー(シート[届先別])
                    WW_SheetNoCalendar = i
                ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                    '〇共通(シート[ﾏｽﾀ])
                    WW_SheetNoMaster = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "TMP6" + (j(0) + 1).ToString("00") Then
                    '    WW_ArrSheetNo01(j(0)) = i
                    '    j(0) += 1
                    'ElseIf WW_Workbook.Worksheets(i).Name = "TMP7" + (j(1) + 1).ToString("00") Then
                    '    WW_ArrSheetNo02(j(1)) = i
                    '    j(1) += 1
                    'ElseIf WW_Workbook.Worksheets(i).Name = "TMP8" + (j(2) + 1).ToString("00") Then
                    '    WW_ArrSheetNo03(j(2)) = i
                    '    j(2) += 1
                    'ElseIf WW_Workbook.Worksheets(i).Name = "TMP9" + (j(3) + 1).ToString("00") Then
                    '    WW_ArrSheetNo04(j(3)) = i
                    '    j(3) += 1
                    'ElseIf WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)新潟①" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)新潟②" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)秋田" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(東北)" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(茨城)" Then
                    '    WW_ArrSheetNoKoteichi(j(4)) = i
                    '    j(4) += 1
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
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            ''◯(固定費・単価)の設定
            'EditKoteihiTankaArea()
            '***** TODO処理 ここまで *****
            '★ [ﾏｽﾀ]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoMaster).Visible = Visibility.Hidden
            ''★ [固定値]シート非表示
            'For Each i In WW_ArrSheetNoKoteichi
            '    WW_Workbook.Worksheets(i).Visible = Visibility.Hidden
            'Next

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

    End Sub

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea()

    End Sub

End Class
