''************************************************************
' 画面名称   ：コンテナ留置先一覧
' 作成日     ：2022/12/21
' 作成者     ：伊藤
' 最終更新日 ：2024/09/20
' 最終更新者 ：星
' バージョン ：ver2
' 
' 修正履歴：2024/09/20 ver2 星 メニュー出力日付別シート分け
''************************************************************
Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' コンテナ留置先一覧帳票作成クラス
''' </summary>
Public Class LNT0012_PutContainerReport_DIODOC

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
                If WW_Workbook.Worksheets(i).Name = "コンテナ留置先一覧" Then
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
    Public Function CreateExcelPrintData(OfficeCode As String, menu As String) As String ' 2024/09/20 ver2 星 CHG
        Dim ReportName As String = "コンテナ留置先一覧_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim lastRow As DataRow = Nothing
            Dim lastlastRow As DataRow = Nothing ' 2024/09/20 ver2 星 ADD
            Dim idx As Int32 = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 0
            Dim row_cnt As Int32 = 0
            Dim Mode As Integer = 0
            Dim SetCol As Integer = 6
            Dim BigcdTotal As Long = 0
            Dim BranchTotal As Long = 0
            Dim StationTotal As Long = 0
            Dim PutCtnTotal As Long = 0
            ' 2024/09/20 ver2 星 ADD START
            Dim FROMYMD As String = ""
            Dim LastFROMYMD As String = ""
            Dim PrintaddsheetFlg As Boolean = False
            Dim seetname As String = ""
            ' 2024/09/20 ver2 星 ADD END

            For Each row As DataRow In PrintData.Rows

                ' 2024/09/20 ver2 星 ADD START
                If menu = "1" Then

                    FROMYMD = row("DATAYMD").ToString
                    seetname = CDate(row("DATAYMD")).ToString("yyyyMMdd")

                    If LastFROMYMD <> "" AndAlso
                       LastFROMYMD <> FROMYMD Then
                        PrintaddsheetFlg = True
                        If lastlastRow Is Nothing Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                            '〇支店計
                            EditBranchTotalArea(idx, row, lastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                        Else
                            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                            '〇支店計
                            EditBranchTotalArea(idx, lastRow, lastlastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                        End If
                    ElseIf LastFROMYMD = "" Then
                        PrintaddsheetFlg = True
                    Else
                        PrintaddsheetFlg = False
                    End If

                    If PrintaddsheetFlg = True Then
                        LastFROMYMD = row("DATAYMD").ToString
                        '〇シート設定
                        TrySetExcelWorkSheet(idx, row("ORGNAME").ToString + seetname, PageNum, "コンテナ留置先一覧")
                        row_cnt = 0
                        idx = 1
                    End If
                End If
                ' 2024/09/20 ver2 星 ADD END

                row_cnt += 1

                '1行目
                If lastRow Is Nothing OrElse
                   PrintaddsheetFlg = True Then ' 2024/09/20 ver2 星 ADD
                    If menu = "0" Then          ' 2024/09/20 ver2 星 ADD
                        '〇シート設定
                        TrySetExcelWorkSheet(idx, row("ORGNAME").ToString, PageNum, "コンテナ留置先一覧")
                    End If                      ' 2024/09/20 ver2 星 ADD
                    '〇ヘッダー情報セット
                    EditHeaderArea(idx, CDate(row("DATAYMD")), row("BIGCTNNM").ToString, row("ORGNAME").ToString, PageNum)

                Else '2行目以降
                    '前行と大分類、支店、駅、留置先、受託人が一致する場合
                    If lastRow("BIGCTNCD").ToString() = row("BIGCTNCD").ToString() AndAlso
                    lastRow("ORGCODE").ToString() = row("ORGCODE").ToString() AndAlso
                    lastRow("NOWSTATIONCD").ToString() = row("NOWSTATIONCD").ToString() Then
                        Mode = 1
                    Else
                        '支店が不一致の場合
                        If lastRow("ORGCODE").ToString() <> row("ORGCODE").ToString() Then
                            '〇支店計
                            EditBranchTotalArea(idx, row, lastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                            If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                                '〇シート設定
                                TrySetExcelWorkSheet(idx, row("ORGNAME").ToString, PageNum, "コンテナ留置先一覧")
                                ' 2024/09/20 ver2 星 ADD START
                            ElseIf menu = "1" Then
                                '〇シート設定
                                TrySetExcelWorkSheet(idx, row("ORGNAME").ToString + seetname, PageNum, "コンテナ留置先一覧")
                            End If
                            ' 2024/09/20 ver2 星 ADD END
                            '〇ヘッダー情報セット
                            EditHeaderArea(idx, CDate(row("DATAYMD")), row("BIGCTNNM").ToString, row("ORGNAME").ToString, PageNum)
                        Else
                            '大分類が不一致の場合
                            If lastRow("BIGCTNCD").ToString() <> row("BIGCTNCD").ToString() Then
                                '〇支店計
                                EditBranchTotalArea(idx, row, lastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                                '〇改頁
                                EditPage(idx, row, lastRow, PageNum)
                            Else
                                '駅が不一致の場合
                                If lastRow("NOWSTATIONCD").ToString() <> row("NOWSTATIONCD").ToString() Then
                                    '〇発駅計
                                    EditStationTotalArea(idx, lastRow, PageNum, StationTotal)
                                End If
                            End If
                        End If
                    End If

                End If

                '明細セット
                EditDetailArea(idx, row, lastRow, PageNum, BigcdTotal, BranchTotal, StationTotal, PutCtnTotal, SetCol, Mode)

                '最後に出力した行を保存
                lastRow = row

                If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                    '最終レコードの場合
                    If row_cnt = PrintData.Rows.Count Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                        '〇支店計
                        EditBranchTotalArea(idx, row, lastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                        Exit For
                    End If
                End If ' 2024/09/20 ver2 星 ADD

            Next

            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                If lastlastRow Is Nothing Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                    '〇支店計
                    EditBranchTotalArea(idx, lastRow, lastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                Else
                    ' 2024/09/20 ver2 星 ADD END
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                    '〇支店計
                    EditBranchTotalArea(idx, lastRow, lastlastRow, PageNum, StationTotal, BranchTotal, OfficeCode)
                End If
            End If ' 2024/09/20 ver2 星 ADD

            'テンプレート削除
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()
            WW_Workbook.Worksheets(0).Delete()

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
    ''' Excel作業シート設定
    ''' </summary>
    ''' <param name="sheetName"></param>
    Protected Function TrySetExcelWorkSheet(ByRef idx As Integer, ByVal sheetName As String, ByRef PageNum As Integer, Optional ByVal templateSheetName As String = Nothing) As Boolean
        Dim result As Boolean = False
        Dim WW_sheetExist As String = "OFF"
        Dim CopySheetNo As Integer = 0

        Try
            'シート名取得
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If Not String.IsNullOrWhiteSpace(templateSheetName) AndAlso WW_Workbook.Worksheets(i).Name = templateSheetName Then
                    CopySheetNo = i
                ElseIf Not String.IsNullOrWhiteSpace(sheetName) AndAlso WW_Workbook.Worksheets(i).Name = sheetName Then
                    WW_SheetNo = i
                    WW_sheetExist = "ON"
                End If
            Next

            If WW_sheetExist = "ON" Then
                result = True
            Else
                Dim copy_worksheet = WW_Workbook.Worksheets(CopySheetNo).Copy
                copy_worksheet.Name = sheetName
                WW_SheetNo = WW_Workbook.Worksheets.Count - 1
                idx = 1
                PageNum += 1
            End If

        Catch ex As Exception
            WW_Workbook = Nothing
            Throw
        End Try
        Return result
    End Function

    ''' <summary>
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea(
        ByRef idx As Integer,
        ByVal DataYMD As Date,
        ByVal Bigcdname As String,
        ByVal orgname As String,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B2:U6")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "LNT0012"
            '〇大分類
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Bigcdname
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = DataYMD
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = pageNum
            '〇支店名
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + (idx + 2).ToString()).Value = orgname
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("V" + (idx + 4).ToString()).Value = "0"

            '行高設定
            WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx + 1)).RowHeight = CDbl("10.5")
            WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx + 3)).RowHeight = CDbl("27")

            If idx > 58 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:U{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 5

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
         ByVal lastrow As DataRow,
         ByRef PageNum As Integer,
         ByRef BigcdTotal As Long,
         ByRef BranchTotal As Long,
         ByRef StationTotal As Long,
         ByRef PutCtnTotal As Long,
         ByRef SetCol As Integer,
         ByVal Mode As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim ChangeFLG As String = "0"
        Dim ArrYMD As DateTime
        Dim month As Integer = 0
        Dim Day As Integer = 0
        Dim DetailArea As IRange = Nothing
        Dim TotalRowFLG As String = WW_Workbook.Worksheets(WW_SheetNo).Range("V" + (idx - 1).ToString()).Text

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B9:U9")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        'セット
        '現在駅名称、留置先名称、着受託人名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("NOWSTATIONNM")
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("PUTNM")
        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("ARRTRUSTEECDNM")
        If TotalRowFLG = "" And lastrow IsNot Nothing Then
            If row("NOWSTATIONNM").ToString = lastrow("NOWSTATIONNM").ToString Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                If row("PUTNM").ToString = lastrow("PUTNM").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = ""
                    If row("ARRTRUSTEECDNM").ToString = lastrow("ARRTRUSTEECDNM").ToString Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = ""
                    End If
                End If
            End If
        End If

        Dim TEST As String = row("CTNNO01").ToString
        If lastrow IsNot Nothing Then
            If row("NOWSTATIONNM").ToString <> lastrow("NOWSTATIONNM").ToString OrElse
             row("PUTNM").ToString <> lastrow("PUTNM").ToString OrElse
             row("ARRTRUSTEECDNM").ToString <> lastrow("ARRTRUSTEECDNM").ToString Then
                If row("ORGCODE").ToString = lastrow("ORGCODE").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                Else
                    WW_Workbook.Worksheets(WW_SheetNo - 1).Range("D" + SetCol.ToString()).Value = PutCtnTotal
                End If
                PutCtnTotal = 0
                SetCol = idx
            End If
        End If

        '個数
        BigcdTotal += 1
        BranchTotal += 1
        StationTotal += 1
        PutCtnTotal += 1
        'コンテナ番号1、コンテナ記号1
        If row("CTNTYPE01") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("CTNNO01").ToString & row("CTNTYPE01").ToString
        Else
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("CTNNO01").ToString
        End If
        '状態区分1
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("STATUSKBNNM01")
        '発駅名称1
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = row("DEPSTATIONNM01")
        '着日1
        If row("ARRYMD01") IsNot DBNull.Value Then
            ArrYMD = CDate(row("ARRYMD01"))
            month = ArrYMD.Month
            Day = ArrYMD.Day
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = month & "." & Day
        End If
        'コンテナ番号2、コンテナ記号2
        If row("CTNNO02") IsNot DBNull.Value Then
            If row("CTNTYPE02") IsNot DBNull.Value Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("CTNNO02").ToString & row("CTNTYPE02").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("CTNNO02").ToString
            End If
            '個数
            BigcdTotal += 1
            BranchTotal += 1
            StationTotal += 1
            PutCtnTotal += 1
        End If
        '状態区分2
        If row("STATUSKBNNM02") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = row("STATUSKBNNM02")
        End If
        '発駅名称2
        If row("DEPSTATIONNM02") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = row("DEPSTATIONNM02")
        End If
        '着日2
        If row("ARRYMD02") IsNot DBNull.Value Then
            ArrYMD = CDate(row("ARRYMD02"))
            month = ArrYMD.Month
            Day = ArrYMD.Day
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = month & "." & Day
        End If
        'コンテナ番号3、コンテナ記号3
        If row("CTNNO03") IsNot DBNull.Value Then
            If row("CTNTYPE03") IsNot DBNull.Value Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("CTNNO03").ToString & row("CTNTYPE03").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("CTNNO03").ToString
            End If
            '個数
            BigcdTotal += 1
            BranchTotal += 1
            StationTotal += 1
            PutCtnTotal += 1
        End If
        '状態区分3
        If row("STATUSKBNNM03") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = row("STATUSKBNNM03")
        End If
        '発駅名称3
        If row("DEPSTATIONNM03") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = row("DEPSTATIONNM03")
        End If
        '着日3
        If row("ARRYMD03") IsNot DBNull.Value Then
            ArrYMD = CDate(row("ARRYMD03"))
            month = ArrYMD.Month
            Day = ArrYMD.Day
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = month & "." & Day
        End If
        'コンテナ番号4、コンテナ記号4
        If row("CTNNO04") IsNot DBNull.Value Then
            If row("CTNTYPE04") IsNot DBNull.Value Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("CTNNO04").ToString & row("CTNTYPE04").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("CTNNO04").ToString
            End If
            '個数
            BigcdTotal += 1
            BranchTotal += 1
            StationTotal += 1
            PutCtnTotal += 1
        End If
        '状態区分4
        If row("STATUSKBNNM04") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("STATUSKBNNM04")
        End If
        '発駅名称4
        If row("DEPSTATIONNM04") IsNot DBNull.Value Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("DEPSTATIONNM04")
        End If
        '着日4
        If row("ARRYMD04") IsNot DBNull.Value Then
            ArrYMD = CDate(row("ARRYMD04"))
            month = ArrYMD.Month
            Day = ArrYMD.Day
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = month & "." & Day
        End If

        '罫線設定
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString() & ":" & "U" + idx.ToString())
        If TotalRowFLG = "1" Then
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        End If
        idx += 1

        '改頁判断
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(row("DATAYMD")), row("BIGCTNNM").ToString, row("ORGNAME").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 改頁処理
    ''' </summary>
    Private Sub EditPage(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal lastrow As DataRow,
         ByRef PageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim DetailArea As IRange = Nothing

        '罫線設定
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx - 1).ToString() & ":" & "U" + (idx - 1).ToString())
        DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin

        '改頁
        While 0 = 0
            Modcnt = idx Mod 59
            If Modcnt = 0 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:U{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
                PageNum += 1
                EditHeaderArea(idx, CDate(row("DATAYMD")), row("BIGCTNNM").ToString, row("ORGNAME").ToString, PageNum)
                Exit While
            Else
                idx += 1
            End If
        End While

    End Sub

    ''' <summary>
    ''' 支店計
    ''' </summary>
    Private Sub EditBranchTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef StationTotal As Long,
        ByRef BranchTotal As Long,
        ByVal OfficeCode As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0

        '〇駅計
        EditStationTotalArea(idx, lastrow, PageNum, StationTotal)

        '〇算出
        '合計行コピー
        If OfficeCode = "" OrElse OfficeCode = "999999" OrElse OfficeCode = CheckOffice(OfficeCode) Then
            '支店計
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B15:U15")
        Else
            '営業所計
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B18:U18")
        End If
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '数量セット
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = BranchTotal
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
        idx += 1

        BranchTotal = 0

    End Sub

    ''' <summary>
    ''' 駅計
    ''' </summary>
    Private Sub EditStationTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef StationTotal As Long
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B12:U12")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '数量セット
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = StationTotal
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
        idx += 1

        StationTotal = 0
        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString() & ":" & "U" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(lastrow("DATAYMD")), lastrow("BIGCTNNM").ToString, lastrow("ORGNAME").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 画面選択組織コードの支店確認
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function CheckOffice(Code As String) As String
        Dim dt As New DataTable
        Dim OfficeCode As String = ""
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine(" SELECT                                                       ")
        sqlStat.AppendLine("     ORGCODE                                          as CODE ")
        sqlStat.AppendLine(" FROM                                                         ")
        sqlStat.AppendLine("     com.LNS0014_ORG WITH(nolock)                             ")
        sqlStat.AppendLine(" WHERE                                                        ")
        sqlStat.AppendLine("     ORGSELECTFLAG = 1                                        ")
        sqlStat.AppendLine(" AND CLASS01 = 1                                              ")
        sqlStat.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD                       ")
        sqlStat.AppendLine(" AND DELFLG = @DELFLG                                         ")
        sqlStat.AppendLine(" AND ORGCODE = @CODE                                          ")
        sqlStat.AppendLine(" ORDER BY                                                     ")
        sqlStat.AppendLine("     ORGCODE                                                  ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                    .Add("@CODE", MySqlDbType.VarChar).Value = Code
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    dt.Load(sqlDr)
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        If dt.Rows.Count > 0 Then
            OfficeCode = dt.Rows(0)("CODE").ToString
        End If
        Return OfficeCode

    End Function
End Class
