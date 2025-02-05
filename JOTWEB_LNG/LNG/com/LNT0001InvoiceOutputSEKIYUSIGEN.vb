Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputSEKIYUSIGEN
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNo03 As Integer() = {0, 0, 0}                           '// 既存シート用(東北)
    Private WW_ArrSheetNo01 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(新潟・庄内)
    Private WW_ArrSheetNo02 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(秋田)
    Private WW_ArrSheetNo03 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(東北)
    Private WW_ArrSheetNo04 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(茨城)

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
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
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal calcNumber As Integer = 1,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTankData = printTankDataClass
            Me.PrintKoteihiData = printKoteihiDataClass
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

            Dim j As Integer() = {0, 0, 0, 0}
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "入力表" Then
                ElseIf WW_Workbook.Worksheets(i).Name = "寺岡製作所（相馬出荷・東北）" Then
                    WW_SheetNo03(0) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "鶴岡ガス（相馬出荷・東北）" Then
                    WW_SheetNo03(1) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "若松ガス（相馬出荷・東北）" Then
                    WW_SheetNo03(2) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP6" + (j(0) + 1).ToString("00") Then
                    WW_ArrSheetNo01(j(0)) = i
                    j(0) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP7" + (j(1) + 1).ToString("00") Then
                    WW_ArrSheetNo01(j(1)) = i
                    j(1) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP8" + (j(2) + 1).ToString("00") Then
                    WW_ArrSheetNo01(j(2)) = i
                    j(2) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP9" + (j(3) + 1).ToString("00") Then
                    WW_ArrSheetNo01(j(3)) = i
                    j(3) += 1
                End If
            Next

        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class
