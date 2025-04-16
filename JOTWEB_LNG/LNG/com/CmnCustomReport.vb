'Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient

''' <summary>
''' カスタムレポート作成Factory
''' </summary>
''' <remarks>
''' Usingを利用しなくてもいいようFactoryパターンを使用
''' </remarks>
Public Class CmnCustomReport

    '○ 共通関数宣言(BASEDLL)
    Protected CS0011LOGWrite As New CS0011LOGWrite  'ログ出力
    Protected CS0050SESSION As New CS0050SESSION    'セッション情報操作処理

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Protected ExcelTemplatePath As String = ""
    Protected UploadRootPath As String = ""
    Protected UrlRoot As String = ""

    ''' <summary>
    ''' 出力対象のシート名
    ''' </summary>
    Protected OutputSheetNames As New List(Of String)

    ''' <summary>
    ''' Spread
    ''' </summary>
    Protected WW_Workbook As New Workbook
    Protected WW_DeleteSheet As IWorksheet
    Protected WW_SheetNo As Integer = 0
    Protected WW_tmpSheetNo As Integer = 0
    Protected WW_SheetExist As String = ""

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="excelFileName">EXCELテンプレートファイル名</param>
    ''' <param name="mapId">MAPID</param>
    Protected Sub New(excelFileName As String, mapId As String)
        Try
            ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                       "PRINTFORMAT", C_DEFAULT_DATAKEY, mapId, excelFileName)
            UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                    "PRINTWORK", CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(UploadRootPath) = False Then
                IO.Directory.CreateDirectory(UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(UploadRootPath, "*.*")
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
            'UrlRoot = String.Format("{0}://{1}/{3}/{2}/",
            '                        HttpContext.Current.Request.Url.Scheme,
            '                        HttpContext.Current.Request.Url.Host,
            '                        CS0050SESSION.USERID,
            '                        CS0050SESSION.PRINT_ROOT_URL_NAME)
            UrlRoot = String.Format("{0}://{1}/{3}/{2}/",
                                    CS0050SESSION.HTTPS_GET,
                                    HttpContext.Current.Request.Url.Host,
                                    CS0050SESSION.USERID,
                                    CS0050SESSION.PRINT_ROOT_URL_NAME)
            'ファイルopen
            WW_Workbook.Open(Me.ExcelTemplatePath)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Excel作業シート設定
    ''' </summary>
    ''' <param name="sheetName"></param>
    Protected Function TrySetExcelWorkSheet(ByVal pagecnt As Integer,
                                            ByVal dltflg As String,
                                            ByVal sheetName As String,
                                            Optional ByVal templateSheetName As String = Nothing) As Boolean
        Dim result As Boolean = False
        Dim WW_sheetExist As String = "OFF"

        Try
            'シート名取得
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If Not String.IsNullOrWhiteSpace(templateSheetName) AndAlso
                    WW_Workbook.Worksheets(i).Name = templateSheetName Then
                    WW_SheetNo = pagecnt
                    WW_tmpSheetNo = i
                    WW_sheetExist = "ON"
                    Dim copy_worksheet = WW_Workbook.Worksheets(i).Copy
                    copy_worksheet.Name = sheetName

                ElseIf Not String.IsNullOrWhiteSpace(sheetName) AndAlso
                    dltflg = "1" AndAlso
                    WW_Workbook.Worksheets(i).Name = sheetName Then
                    WW_SheetNo = i
                    WW_sheetExist = "ON"
                End If
            Next

            If WW_sheetExist = "ON" Then
                result = True
            End If

        Catch ex As Exception
            WW_Workbook = Nothing
            Throw
        Finally
            If Not result Then
                WW_Workbook = Nothing
            End If
        End Try
        Return result
    End Function

    ''' <summary>
    ''' 出力シートのみ残す
    ''' </summary>
    ''' <param name="isReverse">シート順反転</param>
    Protected Sub LeaveOnlyOutputSheets(Optional ByVal isReverse As Boolean = False)
        Dim fstflg As String = ""
        Try
            '○出力シートのみ残す
            If OutputSheetNames IsNot Nothing AndAlso OutputSheetNames.Any() Then
                Dim allSeetName As New Dictionary(Of String, Integer)
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    allSeetName.Add(WW_Workbook.Worksheets(i).Name, i)
                Next

                For Each sheetName As String In allSeetName.
                    Where(Function(x) Not OutputSheetNames.Contains(x.Key)).
                    OrderByDescending(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(0, "1", sheetName) Then
                        If allSeetName.ContainsKey(sheetName) = True Then
                            WW_DeleteSheet = WW_Workbook.Worksheets(WW_SheetNo)
                            WW_DeleteSheet.Delete()
                        End If
                    End If
                Next

                '○シート順反転
                If isReverse Then
                    For i As Integer = WW_Workbook.Worksheets.Count - 1 To 0 Step -1
                        WW_Workbook.Worksheets(i).MoveBefore(WW_Workbook.Worksheets(0))
                    Next
                End If

                '先頭シートを選択
                WW_Workbook.Worksheets(0).Select()

            End If

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Sub

    ''' <summary>
    ''' Excel保存処理
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <param name="uploadFilePath"></param>
    Protected Sub ExcelSaveAs(filePath As String, Optional uploadFilePath As String = Nothing)
        Try
            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(filePath, SaveFileFormat.Xlsx)
            End SyncLock
            '★別名が設定されている場合
            If Not String.IsNullOrEmpty(uploadFilePath) AndAlso filePath <> uploadFilePath Then
                '作成したファイルを指定パスに配置する。
                System.IO.File.Copy(filePath, uploadFilePath)
            End If
        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Sub

    ''' <summary>
    ''' 複数ファイルダウンロード用
    ''' </summary>
    ''' <param name="urlList"></param>
    ''' <returns></returns>
    Public Shared Function CreateUrlJson(ByVal urlList As List(Of String)) As String
        If urlList IsNot Nothing AndAlso urlList.Any() Then
            Return String.Format("[{0}]", String.Join(",", urlList.Select(Function(url) String.Format("{{""url"": ""{0}""}}", url)).ToArray()))
        End If
        Return ""
    End Function

End Class
