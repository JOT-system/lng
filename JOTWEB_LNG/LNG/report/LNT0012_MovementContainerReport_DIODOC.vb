Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 品目別販売実績表帳票作成クラス
''' </summary>
Public Class LNT0012_MovementContainerReport_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

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
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
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
                If WW_Workbook.Worksheets(i).Name = "コンテナ動静表" Then
                    WW_SheetNo = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_tmpSheetNo = i
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(OfficeCode As String, YMD As Date) As String
        Dim ReportName As String = "コンテナ動静表_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim idx As Int32 = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 1
            Dim row_cnt As Int32 = 0
            Dim fstflg As String = "0"
            Dim Mode As Integer = 0

            For Each row As DataRow In PrintData.Rows

                '1行目
                If fstflg = "0" Then
                    '〇ヘッダー情報セット
                    EditHeaderArea(idx, YMD, PageNum)
                    fstflg = "1"
                End If

                '明細セット
                EditDetailArea(idx, row, YMD, PageNum, row_cnt)

            Next

            'テンプレート削除
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()

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
    Private Sub EditHeaderArea(
        ByRef idx As Integer,
        ByVal YMD As Date,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A2:S5")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "LNT0012"
            '〇対象日付
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = YMD
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = DateTime.Now
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = pageNum
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + (idx + 3).ToString()).Value = "0"

            If idx > 52 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:S{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 4

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal YMD As Date,
         ByRef PageNum As Integer,
         ByVal row_cnt As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        '改頁判断
        If row_cnt = 8 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "S" + (idx - 1).ToString())
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, YMD, PageNum)
            row_cnt = 0
        End If

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:S13")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇セット
        '現在駅コード
        WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = row("NOWSTATIONCD")
        '現在駅名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("NOWSTATIONNM")
        'コンテナ記号
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("CTNTYPE").ToString & "-"
        'コンテナ番号
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = row("CTNNO")
        '前月末情報
        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("MOVEMENT_BEF")
        '今月情報
        '1日
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("MOVEMENT_NOW_01")
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_01")
        '2日
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("MOVEMENT_NOW_02")
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_02")
        '3日
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = row("MOVEMENT_NOW_03")
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_03")
        '4日
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = row("MOVEMENT_NOW_04")
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_04")
        '5日
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("MOVEMENT_NOW_05")
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_05")
        '6日
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = row("MOVEMENT_NOW_06")
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_06")
        '7日
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = row("MOVEMENT_NOW_07")
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_07")
        '8日
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("MOVEMENT_NOW_08")
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_08")
        '9日
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("MOVEMENT_NOW_09")
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_09")
        '10日
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = row("MOVEMENT_NOW_10")
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + (idx + 1).ToString()).Value = row("MOVEMENT_AFT_10")
        '11日
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_11")
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_11")
        '12日
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_12")
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_12")
        '13日
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_13")
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_13")
        '14日
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_14")
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_14")
        '15日
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_15")
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_15")
        '16日
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_16")
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_16")
        '17日
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_17")
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_17")
        '18日
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_18")
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_18")
        '19日
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_19")
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_19")
        '20日
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + (idx + 2).ToString()).Value = row("MOVEMENT_NOW_20")
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + (idx + 3).ToString()).Value = row("MOVEMENT_AFT_20")
        '21日
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_21")
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_21")
        '22日
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_22")
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_22")
        '23日
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_23")
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_23")
        '24日
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_24")
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_24")
        '25日
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_25")
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_25")
        '26日
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_26")
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_26")
        '27日
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_27")
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_27")
        '28日
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_28")
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_28")
        '29日
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_29")
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_29")
        '30日
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_30")
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_30")
        '31日
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + (idx + 4).ToString()).Value = row("MOVEMENT_NOW_31")
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + (idx + 5).ToString()).Value = row("MOVEMENT_AFT_31")

        '日数
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("SEKI")
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = row("KUU")
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 2).ToString()).Value = row("KEN")
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 3).ToString()).Value = row("STAGNATION")
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 4).ToString()).Value = row("OFFCNT")


        idx += 6
        row_cnt += 1

    End Sub

End Class
