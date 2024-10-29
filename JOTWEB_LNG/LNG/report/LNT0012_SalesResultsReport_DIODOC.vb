''************************************************************
' 画面名称   ：品目別販売実績表
' 作成日     ：2022/12/22
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
''' 品目別販売実績表帳票作成クラス
''' </summary>
Public Class LNT0012_SalesResultsReport_DIODOC

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
                If WW_Workbook.Worksheets(i).Name = "品目別販売実績表" Then
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
    Public Function CreateExcelPrintData(OfficeCode As String, FromYMD As Date, ToYMD As Date, menu As String) As String ' 2024/09/20 ver2 星 CHG
        Dim ReportName As String = "品目別販売実績表_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim lastRow As DataRow = Nothing
            Dim lastlastRow As DataRow = Nothing ' 2024/09/20 ver2 星 ADD
            Dim idx As Int32 = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 1
            Dim row_cnt As Int32 = 0
            Dim Mode As Integer = 0
            Dim ItemTotal(6, 2) As Long
            Dim BigCateTotal(6, 2) As Long
            Dim AssortmentTotal(6, 2) As Long
            Dim AllTotal(6, 2) As Long
            Dim PrintaddsheetFlg As Boolean = False ' 2024/09/20 ver2 星 ADD
            Dim LastFROMYMD As String = ""          ' 2024/09/20 ver2 星 ADD

            '初期化
            For i As Integer = 1 To 6
                ItemTotal(i, 1) = 0
                ItemTotal(i, 2) = 0
                BigCateTotal(i, 1) = 0
                BigCateTotal(i, 2) = 0
                BigCateTotal(i, 1) = 0
                BigCateTotal(i, 2) = 0
                AllTotal(i, 1) = 0
                AllTotal(i, 2) = 0
            Next

            For Each row As DataRow In PrintData.Rows

                ' 2024/09/20 ver2 星 ADD START
                If menu = "1" Then

                    FromYMD = CDate(row("FROMYMD").ToString)
                    ToYMD = CDate(row("TOYMD").ToString)

                    If LastFROMYMD <> "" AndAlso
                       LastFROMYMD <> row("FROMYMD").ToString Then
                        PrintaddsheetFlg = True
                        If lastlastRow Is Nothing Then
                            '〇総合計
                            EditAllTotalArea(idx, row, lastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, OfficeCode)
                        Else
                            '〇総合計
                            EditAllTotalArea(idx, lastRow, lastlastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, OfficeCode)
                        End If
                    ElseIf LastFROMYMD = "" Then
                        PrintaddsheetFlg = True
                    Else
                        PrintaddsheetFlg = False
                    End If

                    If PrintaddsheetFlg = True Then
                        LastFROMYMD = row("FROMYMD").ToString
                        Dim seetname As String = CDate(row("FROMYMD")).ToString("yyyyMMdd")
                        TrySetExcelWorkSheet(idx, seetname, PageNum, "品目別販売実績表")
                        Me.WW_Workbook.Worksheets(WW_SheetNo).Name = seetname
                        'シートが切り替わり、ページ数リセット
                        PageNum = 1
                        row_cnt = 0
                        idx = 1

                        '初期化
                        For i As Integer = 1 To 6S
                            ItemTotal(i, 1) = 0
                            ItemTotal(i, 2) = 0
                            BigCateTotal(i, 1) = 0
                            BigCateTotal(i, 2) = 0
                            BigCateTotal(i, 1) = 0
                            BigCateTotal(i, 2) = 0
                            AllTotal(i, 1) = 0
                            AllTotal(i, 2) = 0
                            AssortmentTotal(i, 1) = 0
                            AssortmentTotal(i, 2) = 0
                        Next

                    End If

                End If
                ' 2024/09/20 ver2 星 ADD END

                row_cnt += 1

                '1行目
                If lastRow Is Nothing OrElse
                   PrintaddsheetFlg = True Then ' 2024/09/20 ver2 星 ADD
                    If menu = "0" Then          ' 2024/09/20 ver2 星 ADD
                        '〇ヘッダー情報セット
                        EditHeaderArea(idx, FromYMD, ToYMD, row("JOTDEPBRANCHNM").ToString, PageNum)
                        ' 2024/09/20 ver2 星 ADD START
                    ElseIf menu = "1" Then
                        '〇ヘッダー情報セット
                        EditHeaderArea(idx, CDate(row("FROMYMD").ToString), CDate(row("TOYMD").ToString), row("JOTDEPBRANCHNM").ToString, PageNum)
                    End If
                    ' 2024/09/20 ver2 星 ADD END

                Else '2行目以降
                    '前行と支店、品類、品目が一致する場合
                    If lastRow("JOTDEPBRANCHCD").ToString() = row("JOTDEPBRANCHCD").ToString() AndAlso
                    lastRow("ASSORTMENTCD").ToString() = row("ASSORTMENTCD").ToString() AndAlso
                    lastRow("BIGCATEGCD").ToString() = row("BIGCATEGCD").ToString() Then
                        Mode = 1
                    Else
                        '支店が不一致の場合
                        If lastRow("JOTDEPBRANCHCD").ToString() <> row("JOTDEPBRANCHCD").ToString() Then
                            '〇支店計
                            EditBranchTotalArea(idx, row, lastRow, FromYMD, ToYMD, PageNum, AssortmentTotal, BigCateTotal, ItemTotal)
                            '〇改頁
                            EditPage(idx, row, lastRow, FromYMD, ToYMD, PageNum)
                        Else
                            '品類が不一致の場合
                            If lastRow("ASSORTMENTCD").ToString() <> row("ASSORTMENTCD").ToString() Then
                                '〇品類計
                                EditBigCateTotalArea(idx, row, lastRow, FromYMD, ToYMD, PageNum, BigCateTotal, ItemTotal)
                            Else
                                '品目が不一致の場合
                                If lastRow("BIGCATEGCD").ToString() <> row("BIGCATEGCD").ToString() Then
                                    '〇品目計
                                    EditItemTotalArea(idx, lastRow, FromYMD, ToYMD, PageNum, ItemTotal)
                                End If
                            End If
                        End If
                    End If

                End If

                '明細セット
                EditDetailArea(idx, row, lastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, Mode)

                '最後に出力した行を保存
                lastlastRow = lastRow ' 2024/09/20 ver2 星 ADD
                lastRow = row

                If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                    '最終レコードの場合
                    If row_cnt = PrintData.Rows.Count Then
                        '〇総合計
                        EditAllTotalArea(idx, row, lastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, OfficeCode)
                        Exit For
                    End If
                End If ' 2024/09/20 ver2 星 ADD
            Next


            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                If lastlastRow Is Nothing Then
                    '〇総合計
                    EditAllTotalArea(idx, lastRow, lastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, OfficeCode)
                Else
                    '〇総合計
                    EditAllTotalArea(idx, lastRow, lastlastRow, FromYMD, ToYMD, PageNum, AllTotal, AssortmentTotal, BigCateTotal, ItemTotal, OfficeCode)
                End If
            End If
            ' 2024/09/20 ver2 星 ADD END

            'テンプレート削除
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()
            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                WW_Workbook.Worksheets(0).Delete()
            End If
            ' 2024/09/20 ver2 星 ADD END

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
        ByVal FromYMD As Date,
        ByVal ToYMD As Date,
        ByVal jotdepbranchnm As String,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B2:V7")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "LNT0012"
            '〇FROM日付
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = FromYMD
            '◯TO日付
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = ToYMD
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = DateTime.Now
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = pageNum
            '〇支店名
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + (idx + 1).ToString()).Value = jotdepbranchnm
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("V" + (idx + 5).ToString()).Value = "0"

            If idx > 58 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:U{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 6

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

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
            For intCnt As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If Not String.IsNullOrWhiteSpace(templateSheetName) AndAlso WW_Workbook.Worksheets(intCnt).Name = templateSheetName Then
                    CopySheetNo = intCnt
                ElseIf Not String.IsNullOrWhiteSpace(sheetName) AndAlso WW_Workbook.Worksheets(intCnt).Name = sheetName Then
                    WW_SheetNo = intCnt
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
            End If

        Catch ex As Exception
            WW_Workbook = Nothing
            Throw
        End Try
        Return result
    End Function

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal lastrow As DataRow,
         ByVal FromYMD As Date,
         ByVal ToYMD As Date,
         ByRef PageNum As Integer,
         ByRef AllTotal(,) As Long,
         ByRef AssortmentTotal(,) As Long,
         ByRef BigCateTotal(,) As Long,
         ByRef ItemTotal(,) As Long,
         ByVal Mode As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        '〇品類名
        '品類名行コピー
        If lastrow IsNot Nothing Then
            If row("ASSORTMENTNM").ToString <> lastrow("ASSORTMENTNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B10:V10")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = row("ASSORTMENTNM")
                idx += 1
            End If
        Else
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B10:V10")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = row("ASSORTMENTNM")
            idx += 1
        End If
        '改頁判断
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "U" + (idx - 1).ToString())
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, row("JOTDEPBRANCHNM").ToString, PageNum)
        End If

        '〇品目名
        '品目名行コピー
        If lastrow IsNot Nothing Then
            If row("BIGCATEGNM").ToString <> lastrow("BIGCATEGNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B13:V13")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("BIGCATEGNM")
                '罫線設定
                Dim TotalRowFLG As String = WW_Workbook.Worksheets(WW_SheetNo).Range("V" + (idx - 1).ToString()).Text
                DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "U" + idx.ToString())
                If TotalRowFLG = "1" Then
                    DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                End If
                idx += 1
            End If
        Else
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B13:V13")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("BIGCATEGNM")
            idx += 1
        End If
        '改頁判断
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "U" + (idx - 1).ToString())
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, row("JOTDEPBRANCHNM").ToString, PageNum)
        End If

        '〇明細
        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B16:V16")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '品名
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("ITEMNAME")
        '<<冷蔵>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("QUANTITY_1")
        AssortmentTotal(1, 1) += CType(row("QUANTITY_1"), Long)
        BigCateTotal(1, 1) += CType(row("QUANTITY_1"), Long)
        ItemTotal(1, 1) += CType(row("QUANTITY_1"), Long)
        '使用料（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("USEFEE_1")
        AssortmentTotal(1, 2) += CType(row("USEFEE_EX_1"), Long)
        BigCateTotal(1, 2) += CType(row("USEFEE_EX_1"), Long)
        ItemTotal(1, 2) += CType(row("USEFEE_EX_1"), Long)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = row("QUANTITY_2")
        AssortmentTotal(2, 1) += CType(row("QUANTITY_2"), Long)
        BigCateTotal(2, 1) += CType(row("QUANTITY_2"), Long)
        ItemTotal(2, 1) += CType(row("QUANTITY_2"), Long)
        '使用料（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = row("USEFEE_2")
        AssortmentTotal(2, 2) += CType(row("USEFEE_EX_2"), Long)
        BigCateTotal(2, 2) += CType(row("USEFEE_EX_2"), Long)
        ItemTotal(2, 2) += CType(row("USEFEE_EX_2"), Long)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("QUANTITY_3")
        AssortmentTotal(3, 1) += CType(row("QUANTITY_3"), Long)
        BigCateTotal(3, 1) += CType(row("QUANTITY_3"), Long)
        ItemTotal(3, 1) += CType(row("QUANTITY_3"), Long)
        '対比（個数）
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = row("QUANTITY_C_3")
        '使用料（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = row("USEFEE_3")
        AssortmentTotal(3, 2) += CType(row("USEFEE_EX_1"), Long) - CType(row("USEFEE_EX_2"), Long)
        BigCateTotal(3, 2) += CType(row("USEFEE_EX_1"), Long) - CType(row("USEFEE_EX_2"), Long)
        ItemTotal(3, 2) += CType(row("USEFEE_EX_1"), Long) - CType(row("USEFEE_EX_2"), Long)
        '対比（使用料）
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("USEFEE_C_3")

        '<<S-UR>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("QUANTITY_4")
        AssortmentTotal(4, 1) += CType(row("QUANTITY_4"), Long)
        BigCateTotal(4, 1) += CType(row("QUANTITY_4"), Long)
        ItemTotal(4, 1) += CType(row("QUANTITY_4"), Long)
        '使用料（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = row("USEFEE_4")
        AssortmentTotal(4, 2) += CType(row("USEFEE_EX_4"), Long)
        BigCateTotal(4, 2) += CType(row("USEFEE_EX_4"), Long)
        ItemTotal(4, 2) += CType(row("USEFEE_EX_4"), Long)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = row("QUANTITY_5")
        AssortmentTotal(5, 1) += CType(row("QUANTITY_5"), Long)
        BigCateTotal(5, 1) += CType(row("QUANTITY_5"), Long)
        ItemTotal(5, 1) += CType(row("QUANTITY_5"), Long)
        '使用料（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = row("USEFEE_5")
        AssortmentTotal(5, 2) += CType(row("USEFEE_EX_5"), Long)
        BigCateTotal(5, 2) += CType(row("USEFEE_EX_5"), Long)
        ItemTotal(5, 2) += CType(row("USEFEE_EX_5"), Long)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("QUANTITY_6")
        AssortmentTotal(6, 1) += CType(row("QUANTITY_6"), Long)
        BigCateTotal(6, 1) += CType(row("QUANTITY_6"), Long)
        ItemTotal(6, 1) += CType(row("QUANTITY_6"), Long)
        '対比（個数）
        WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("QUANTITY_C_6")
        '使用料（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("USEFEE_6")
        AssortmentTotal(6, 2) += CType(row("USEFEE_EX_4"), Long) - CType(row("USEFEE_EX_5"), Long)
        BigCateTotal(6, 2) += CType(row("USEFEE_EX_4"), Long) - CType(row("USEFEE_EX_5"), Long)
        ItemTotal(6, 2) += CType(row("USEFEE_EX_4"), Long) - CType(row("USEFEE_EX_5"), Long)
        '対比（使用料）
        WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = row("USEFEE_C_6")
        idx += 1

        '総合計加算
        If row("JOTDEPBRANCHCD").ToString = "011301" Then
            AllTotal(1, 1) += CType(row("QUANTITY_1"), Long)
            AllTotal(1, 2) += CType(row("USEFEE_EX_1"), Long)
            AllTotal(2, 1) += CType(row("QUANTITY_2"), Long)
            AllTotal(2, 2) += CType(row("USEFEE_EX_2"), Long)
            AllTotal(3, 1) += CType(row("QUANTITY_3"), Long)
            AllTotal(3, 2) += CType(row("USEFEE_EX_1"), Long) - CType(row("USEFEE_EX_2"), Long)
            AllTotal(4, 1) += CType(row("QUANTITY_4"), Long)
            AllTotal(4, 2) += CType(row("USEFEE_EX_4"), Long)
            AllTotal(5, 1) += CType(row("QUANTITY_5"), Long)
            AllTotal(5, 2) += CType(row("USEFEE_EX_5"), Long)
            AllTotal(6, 1) += CType(row("QUANTITY_6"), Long)
            AllTotal(6, 2) += CType(row("USEFEE_EX_4"), Long) - CType(row("USEFEE_EX_5"), Long)
        End If

        '改頁判断
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "U" + (idx - 1).ToString())
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, row("JOTDEPBRANCHNM").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 改頁処理
    ''' </summary>
    Private Sub EditPage(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal lastrow As DataRow,
         ByVal FromYMD As Date,
         ByVal ToYMD As Date,
         ByRef PageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim DetailArea As IRange = Nothing

        '罫線設定
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "U" + (idx - 1).ToString())
        DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin

        '改頁
        While 0 = 0
            Modcnt = idx Mod 62
            If Modcnt = 0 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:U{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
                PageNum += 1
                EditHeaderArea(idx, FromYMD, ToYMD, row("JOTDEPBRANCHNM").ToString, PageNum)
                Exit While
            Else
                idx += 1
            End If
        End While

    End Sub

    ''' <summary>
    ''' 総合計
    ''' </summary>
    Private Sub EditAllTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByVal FromYMD As Date,
        ByVal ToYMD As Date,
        ByRef PageNum As Integer,
        ByRef AllTotal(,) As Long,
        ByRef AssortmentTotal(,) As Long,
        ByRef BigCateTotal(,) As Long,
        ByRef ItemTotal(,) As Long,
        ByVal OfficeCode As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim Calc As Double = 0

        '〇品類計
        EditBigCateTotalArea(idx, row, lastrow, FromYMD, ToYMD, PageNum, BigCateTotal, ItemTotal)

        If OfficeCode = "" OrElse OfficeCode = "999999" Then
            '〇算出
            '合計行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B28:V28")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '<<冷蔵>>
            '個数（実績）
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = AllTotal(1, 1)
            '使用料（実績）
            Calc = AllTotal(1, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Math.Round(Calc, 1)
            '個数（前年実績）
            WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = AllTotal(2, 1)
            '使用料（前年実績）
            Calc = AllTotal(2, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Math.Round(Calc, 1)
            '個数（前年実績対比）
            WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = AllTotal(3, 1)
            '対比（個数）
            If AllTotal(1, 1) <> 0 AndAlso AllTotal(2, 1) <> 0 Then
                Calc = AllTotal(1, 1) * 100 / AllTotal(2, 1) - 100
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Math.Round(Calc, 1)
            Else
                If AllTotal(1, 1) <> 0 AndAlso AllTotal(2, 1) = 0 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = -100
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = 0
                End If
            End If
            '使用料（前年実績対比）
            Calc = AllTotal(3, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Math.Round(Calc, 1)
            '対比（使用料）
            If AllTotal(1, 2) <> 0 AndAlso AllTotal(2, 2) <> 0 Then
                Calc = AllTotal(1, 2) * 100 / AllTotal(2, 2) - 100
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Math.Round(Calc, 1)
            Else
                If AllTotal(1, 2) <> 0 AndAlso AllTotal(2, 2) = 0 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = -100
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = 0
                End If
            End If

            '<<S-UR>>
            '個数（実績）
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = AllTotal(4, 1)
            '使用料（実績）
            Calc = AllTotal(4, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Math.Round(Calc, 1)
            '個数（前年実績）
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = AllTotal(5, 1)
            '使用料（前年実績）
            Calc = AllTotal(5, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = Math.Round(Calc, 1)
            '個数（前年実績対比）
            WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = AllTotal(6, 1)
            '対比（個数）
            If AllTotal(4, 1) <> 0 AndAlso AllTotal(5, 1) <> 0 Then
                Calc = AllTotal(4, 1) * 100 / AllTotal(5, 1) - 100
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = Math.Round(Calc, 1)
            Else
                If AllTotal(4, 1) <> 0 AndAlso AllTotal(5, 1) = 0 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = -100
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = 0
                End If
            End If
            '使用料（前年実績対比）
            Calc = AllTotal(6, 2) / 1000
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "#,##0"
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Math.Round(Calc, 1)
            '対比（使用料）
            If AllTotal(4, 2) <> 0 AndAlso AllTotal(5, 2) <> 0 Then
                Calc = AllTotal(4, 2) * 100 / AllTotal(5, 2) - 100
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = Math.Round(Calc, 1)
            Else
                If AllTotal(4, 2) <> 0 AndAlso AllTotal(5, 2) = 0 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = -100
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
                End If
            End If
            WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
            idx += 1

            'クリア
            For i As Integer = 1 To 6
                AllTotal(i, 1) = 0
                AllTotal(i, 2) = 0
            Next
        End If

    End Sub

    ''' <summary>
    ''' 支店計
    ''' </summary>
    Private Sub EditBranchTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByVal FromYMD As Date,
        ByVal ToYMD As Date,
        ByRef PageNum As Integer,
        ByRef AssortmentTotal(,) As Long,
        ByRef BigCateTotal(,) As Long,
        ByRef ItemTotal(,) As Long
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim Calc As Double = 0

        '〇品類計
        EditBigCateTotalArea(idx, row, lastrow, FromYMD, ToYMD, PageNum, BigCateTotal, ItemTotal)

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B25:V25")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '<<冷蔵>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = AssortmentTotal(1, 1)
        '使用料（実績）
        Calc = AssortmentTotal(1, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = AssortmentTotal(2, 1)
        '使用料（前年実績）
        Calc = AssortmentTotal(2, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = AssortmentTotal(3, 1)
        '対比（個数）
        If AssortmentTotal(1, 1) <> 0 AndAlso AssortmentTotal(2, 1) <> 0 Then
            Calc = AssortmentTotal(1, 1) * 100 / AssortmentTotal(2, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If AssortmentTotal(1, 1) <> 0 AndAlso AssortmentTotal(2, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = AssortmentTotal(3, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If AssortmentTotal(1, 2) <> 0 AndAlso AssortmentTotal(2, 2) <> 0 Then
            Calc = AssortmentTotal(1, 2) * 100 / AssortmentTotal(2, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If AssortmentTotal(1, 2) <> 0 AndAlso AssortmentTotal(2, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = 0
            End If
        End If

        '<<S-UR>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = AssortmentTotal(4, 1)
        '使用料（実績）
        Calc = AssortmentTotal(4, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = AssortmentTotal(5, 1)
        '使用料（前年実績）
        Calc = AssortmentTotal(5, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = AssortmentTotal(6, 1)
        '対比（個数）
        If AssortmentTotal(4, 1) <> 0 AndAlso AssortmentTotal(5, 1) <> 0 Then
            Calc = AssortmentTotal(4, 1) * 100 / AssortmentTotal(5, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If AssortmentTotal(4, 1) <> 0 AndAlso AssortmentTotal(5, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = AssortmentTotal(6, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If AssortmentTotal(4, 2) <> 0 AndAlso AssortmentTotal(5, 2) <> 0 Then
            Calc = AssortmentTotal(4, 2) * 100 / AssortmentTotal(5, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If AssortmentTotal(4, 2) <> 0 AndAlso AssortmentTotal(5, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
            End If
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
        idx += 1

        'クリア
        For i As Integer = 1 To 6
            AssortmentTotal(i, 1) = 0
            AssortmentTotal(i, 2) = 0
        Next

        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "U" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, lastrow("JOTDEPBRANCHNM").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 品類計
    ''' </summary>
    Private Sub EditBigCateTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByVal FromYMD As Date,
        ByVal ToYMD As Date,
        ByRef PageNum As Integer,
        ByRef BigCateTotal(,) As Long,
        ByRef ItemTotal(,) As Long
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim Calc As Double = 0

        '〇品目計
        EditItemTotalArea(idx, lastrow, FromYMD, ToYMD, PageNum, ItemTotal)

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B22:V22")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '<<冷蔵>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = BigCateTotal(1, 1)
        '使用料（実績）
        Calc = BigCateTotal(1, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = BigCateTotal(2, 1)
        '使用料（前年実績）
        Calc = BigCateTotal(2, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = BigCateTotal(3, 1)
        '対比（個数）
        If BigCateTotal(1, 1) <> 0 AndAlso BigCateTotal(2, 1) <> 0 Then
            Calc = BigCateTotal(1, 1) * 100 / BigCateTotal(2, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If BigCateTotal(1, 1) <> 0 AndAlso BigCateTotal(2, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = BigCateTotal(3, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If BigCateTotal(1, 2) <> 0 AndAlso BigCateTotal(2, 2) <> 0 Then
            Calc = BigCateTotal(1, 2) * 100 / BigCateTotal(2, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If BigCateTotal(1, 2) <> 0 AndAlso BigCateTotal(2, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = 0
            End If
        End If

        '<<S-UR>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = BigCateTotal(4, 1)
        '使用料（実績）
        Calc = BigCateTotal(4, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = BigCateTotal(5, 1)
        '使用料（前年実績）
        Calc = BigCateTotal(5, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = BigCateTotal(6, 1)
        '対比（個数）
        If BigCateTotal(4, 1) <> 0 AndAlso BigCateTotal(5, 1) <> 0 Then
            Calc = BigCateTotal(4, 1) * 100 / BigCateTotal(5, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If BigCateTotal(4, 1) <> 0 AndAlso BigCateTotal(5, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = BigCateTotal(6, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If BigCateTotal(4, 2) <> 0 AndAlso BigCateTotal(5, 2) <> 0 Then
            Calc = BigCateTotal(4, 2) * 100 / BigCateTotal(5, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If BigCateTotal(4, 2) <> 0 AndAlso BigCateTotal(5, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
            End If
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
        idx += 1

        'クリア
        For i As Integer = 1 To 6
            BigCateTotal(i, 1) = 0
            BigCateTotal(i, 2) = 0
        Next

        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "U" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, lastrow("JOTDEPBRANCHNM").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 品目計
    ''' </summary>
    Private Sub EditItemTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByVal FromYMD As Date,
        ByVal ToYMD As Date,
        ByRef PageNum As Integer,
        ByRef ItemTotal(,) As Long
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim Calc As Double = 0

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B19:V19")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '<<冷蔵>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = ItemTotal(1, 1)
        '使用料（実績）
        Calc = ItemTotal(1, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = ItemTotal(2, 1)
        '使用料（前年実績）
        Calc = ItemTotal(2, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = ItemTotal(3, 1)
        '対比（個数）
        If ItemTotal(1, 1) <> 0 AndAlso ItemTotal(2, 1) <> 0 Then
            Calc = ItemTotal(1, 1) * 100 / ItemTotal(2, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If ItemTotal(1, 1) <> 0 AndAlso ItemTotal(2, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = ItemTotal(3, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If ItemTotal(1, 2) <> 0 AndAlso ItemTotal(2, 2) <> 0 Then
            Calc = ItemTotal(1, 2) * 100 / ItemTotal(2, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If ItemTotal(1, 2) <> 0 AndAlso ItemTotal(2, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = 0
            End If
        End If

        '<<S-UR>>
        '個数（実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = ItemTotal(4, 1)
        '使用料（実績）
        Calc = ItemTotal(4, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績）
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = ItemTotal(5, 1)
        '使用料（前年実績）
        Calc = ItemTotal(5, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = Math.Round(Calc, 1)
        '個数（前年実績対比）
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = ItemTotal(6, 1)
        '対比（個数）
        If ItemTotal(4, 1) <> 0 AndAlso ItemTotal(5, 1) <> 0 Then
            Calc = ItemTotal(4, 1) * 100 / ItemTotal(5, 1) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If ItemTotal(4, 1) <> 0 AndAlso ItemTotal(5, 1) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = 0
            End If
        End If
        '使用料（前年実績対比）
        Calc = ItemTotal(6, 2) / 1000
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "#,##0"
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Math.Round(Calc, 1)
        '対比（使用料）
        If ItemTotal(4, 2) <> 0 AndAlso ItemTotal(5, 2) <> 0 Then
            Calc = ItemTotal(4, 2) * 100 / ItemTotal(5, 2) - 100
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = Math.Round(Calc, 1)
        Else
            If ItemTotal(4, 2) <> 0 AndAlso ItemTotal(5, 2) = 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = -100
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
            End If
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = "1"
        idx += 1

        'クリア
        For i As Integer = 1 To 6
            ItemTotal(i, 1) = 0
            ItemTotal(i, 2) = 0
        Next

        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 62
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "U" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, FromYMD, ToYMD, lastrow("JOTDEPBRANCHNM").ToString, PageNum)
        End If

    End Sub

End Class
