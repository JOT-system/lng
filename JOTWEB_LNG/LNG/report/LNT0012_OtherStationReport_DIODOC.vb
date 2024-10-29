''************************************************************
' 画面名称   ：他駅発送明細
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
''' 他駅発送明細帳票作成クラス
''' </summary>
Public Class LNT0012_OtherStationReport_DIODOC

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
                If WW_Workbook.Worksheets(i).Name = "他駅発送明細" Then
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
        Dim ReportName As String = "他駅発送明細_"
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
            Dim TotalKBN As Integer = 0
            Dim TrusteeKBN As Integer = 0
            Dim BigCtncdKBN As Integer = 0
            Dim Quantity1(2) As Long
            Dim Quantity2(2, 6) As Long
            Dim Quantity3(2, 6) As Long
            Dim Quantity4(2) As Long
            Dim StackFreeKBN As Integer = 0
            ' 2024/09/20 ver2 星 ADD START
            Dim FROMYMD As String = ""
            Dim LastFROMYMD As String = ""
            Dim PrintaddsheetFlg As Boolean = False
            Dim seetname As String = ""
            ' 2024/09/20 ver2 星 ADD END

            '配列初期化
            For A As Integer = 1 To 2
                Quantity1(A) = 0
                Quantity4(A) = 0
                For B As Integer = 1 To 6
                    Quantity2(A, B) = 0
                    Quantity3(A, B) = 0
                Next
            Next

            For Each row As DataRow In PrintData.Rows

                ' 2024/09/20 ver2 星 ADD START
                If menu = "1" Then

                    FROMYMD = row("FROMDAY").ToString
                    seetname = CDate(row("FROMDAY")).ToString("yyyyMMdd")

                    If LastFROMYMD <> "" AndAlso
                       LastFROMYMD <> FROMYMD Then
                        PrintaddsheetFlg = True
                        If lastlastRow Is Nothing Then
                            '〇支店計
                            EditBranchTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                        Else
                            '〇支店計
                            EditBranchTotalArea(idx, lastRow, lastlastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                        End If
                    ElseIf LastFROMYMD = "" Then
                        PrintaddsheetFlg = True
                    Else
                        PrintaddsheetFlg = False
                    End If

                    If PrintaddsheetFlg = True Then
                        LastFROMYMD = row("FROMDAY").ToString
                        '〇シート設定
                        TrySetExcelWorkSheet(idx, row("ORGNAME").ToString + seetname, PageNum, "他駅発送明細")
                        'シートが切り替わり、ページ数リセット
                        PageNum = 1
                        row_cnt = 0
                        idx = 1
                        '配列初期化
                        For A As Integer = 1 To 2
                            Quantity1(A) = 0
                            Quantity4(A) = 0
                            For B As Integer = 1 To 6
                                Quantity2(A, B) = 0
                                Quantity3(A, B) = 0
                            Next
                        Next
                    End If
                End If
                ' 2024/09/20 ver2 星 ADD END

                row_cnt += 1

                '1行目
                If lastRow Is Nothing OrElse
                   PrintaddsheetFlg = True Then ' 2024/09/20 ver2 星 ADD
                    If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                        '〇シート設定
                        TrySetExcelWorkSheet(idx, row("ORGNAME").ToString, PageNum, "他駅発送明細")
                    End If ' 2024/09/20 ver2 星 ADD
                    '〇ヘッダー情報セット
                    EditHeaderArea(idx, CDate(row("SHIPYMD")), row("ORGNAME").ToString, PageNum)

                Else '2行目以降
                    '前行と支店、発送年月日、発駅、積空区分が一致する場合
                    If lastRow("ORGCODE").ToString() = row("ORGCODE").ToString() AndAlso
                    lastRow("SHIPYMD").ToString() = row("SHIPYMD").ToString() AndAlso
                    lastRow("STATION").ToString() = row("STATION").ToString() AndAlso
                    lastRow("STACKFREEKBN").ToString() = row("STACKFREEKBN").ToString() Then
                        Mode = 1
                    Else
                        '支店が不一致の場合
                        If lastRow("ORGCODE").ToString() <> row("ORGCODE").ToString() Then
                            '〇支店計
                            EditBranchTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                            If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                                '〇シート設定
                                TrySetExcelWorkSheet(idx, row("ORGNAME").ToString, PageNum, "他駅発送明細")
                                ' 2024/09/20 ver2 星 ADD START
                            ElseIf menu = "1" Then
                                TrySetExcelWorkSheet(idx, row("ORGNAME").ToString + seetname, PageNum, "他駅発送明細")
                            End If
                            ' 2024/09/20 ver2 星 ADD END
                            '〇ヘッダー情報セット
                            EditHeaderArea(idx, CDate(row("SHIPYMD")), row("ORGNAME").ToString, PageNum)
                        Else
                            '発送年月日が不一致の場合
                            If lastRow("SHIPYMD").ToString() <> row("SHIPYMD").ToString() Then
                                '〇発駅計
                                EditStationTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN)
                                '〇改頁
                                EditPage(idx, row, lastRow, PageNum)
                            Else
                                '発駅が不一致の場合
                                If lastRow("STATION").ToString() <> row("STATION").ToString() Then
                                    '〇発駅計
                                    EditStationTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN)
                                Else
                                    '積空区分が不一致の場合
                                    If lastRow("STACKFREEKBN").ToString() <> row("STACKFREEKBN").ToString() Then
                                        '積空区分別計
                                        EditStackFreeTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                '明細セット
                EditDetailArea(idx, row, lastRow, PageNum, Mode)

                '数量、料金加算
                '〇大分類コード区分
                Dim BigCtncd As String = row("BIGCTNCD").ToString
                If BigCtncd = "10" Then
                    BigCtncdKBN = 1
                End If
                If BigCtncd = "11" Then
                    BigCtncdKBN = 2
                End If
                If BigCtncd = "15" Then
                    BigCtncdKBN = 3
                End If
                If BigCtncd = "20" Then
                    BigCtncdKBN = 4
                End If
                If BigCtncd = "35" Then
                    BigCtncdKBN = 5
                End If
                If BigCtncd = "25" Then
                    BigCtncdKBN = 6
                End If

                '〇セット
                If row("STACKFREEKBN") IsNot DBNull.Value Then
                    StackFreeKBN = CType(row("STACKFREEKBN"), Integer)
                End If
                Quantity4(1) += CType(row("QUANTITY"), Long)
                Quantity4(2) += CType(row("QUANTITY"), Long)
                If StackFreeKBN = 1 Then
                    Quantity1(1) += CType(row("QUANTITY"), Long)
                    Quantity2(1, BigCtncdKBN) += CType(row("QUANTITY"), Long)
                    Quantity2(2, BigCtncdKBN) += CType(row("QUANTITY"), Long)
                Else
                    Quantity1(2) += CType(row("QUANTITY"), Long)
                    Quantity3(1, BigCtncdKBN) += CType(row("QUANTITY"), Long)
                    Quantity3(2, BigCtncdKBN) += CType(row("QUANTITY"), Long)
                End If

                '最後に出力した行を保存
                lastlastRow = lastRow ' 2024/09/20 ver2 星 ADD
                lastRow = row

                If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                    '最終レコードの場合
                    If row_cnt = PrintData.Rows.Count Then
                        '〇支店計
                        EditBranchTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                        Exit For
                    End If

                End If ' 2024/09/20 ver2 星 ADD

            Next

            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                If lastlastRow Is Nothing Then
                    '〇支店計
                    EditBranchTotalArea(idx, lastRow, lastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                Else
                    '〇支店計
                    EditBranchTotalArea(idx, lastRow, lastlastRow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFreeKBN, OfficeCode, 0)
                End If
            End If
            ' 2024/09/20 ver2 星 ADD END

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
                PageNum = 1
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
        ByVal ShipYMD As Date,
        ByVal orgname As String,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B2:AC5")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "LNT0012"
            '◯発送日
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = ShipYMD
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("AA" + idx.ToString()).Value = pageNum
            '〇処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("AC" + idx.ToString()).Value = Now
            '〇支店名
            WW_Workbook.Worksheets(WW_SheetNo).Range("W" + (idx + 2).ToString()).Value = orgname
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("AD" + (idx + 3).ToString()).Value = "0"

            '行高設定
            WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx + 1)).RowHeight = CDbl("10.5")
            WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx + 2)).RowHeight = CDbl("20.25")
            WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx + 3)).RowHeight = CDbl("36.75")

            If idx > 58 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:AC{0}", idx))
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
         ByVal lastrow As DataRow,
         ByRef PageNum As Integer,
         ByVal Mode As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim StackFreeKBN As Integer = 0
        Dim Modcnt As Integer = 0
        Dim DetailArea As IRange = Nothing
        Dim TotalRowFLG As String = WW_Workbook.Worksheets(WW_SheetNo).Range("AD" + (idx - 1).ToString()).Text

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B8:AC8")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        'セット
        '変更後着駅名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("AFTERARRSTATIONNM")
        If TotalRowFLG = "" And lastrow IsNot Nothing Then
            If row("AFTERARRSTATIONNM").ToString = lastrow("AFTERARRSTATIONNM").ToString Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
            End If
        End If
        'コンテナ記号
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("CTNTYPE")
        'コンテナ番号
        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("CTNNO")
        '発駅名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("DEPSTATIONNM")
        '発受託人名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = row("DEPTRUSTEENM")
        '発荷主名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("DEPSHIPPERNM")
        '品名
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = row("ITEMNAME")
        '列車番号(発)
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("DEPTRAINNO")
        '着駅名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("ARRSTATIONNM")
        '到着年月
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = row("ARRPLANMM").ToString & "." & row("ARRPLANDD").ToString
        '列車番号(着)
        WW_Workbook.Worksheets(WW_SheetNo).Range("X" + idx.ToString()).Value = row("ARRTRAINNO")
        '着受託人名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("Z" + idx.ToString()).Value = row("ARRTRUSTEENM")

        '罫線設定
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString() & ":" & "AB" + idx.ToString())
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
            EditHeaderArea(idx, CDate(row("SHIPYMD")), row("ORGNAME").ToString, PageNum)
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
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx - 1).ToString() & ":" & "AB" + (idx - 1).ToString())
        DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin

        '改頁
        While 0 = 0
            Modcnt = idx Mod 59
            If Modcnt = 0 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:AC{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
                PageNum += 1
                EditHeaderArea(idx, CDate(row("SHIPYMD")), row("ORGNAME").ToString, PageNum)
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
        ByRef Quantity1() As Long,
        ByRef Quantity2(,) As Long,
        ByRef Quantity3(,) As Long,
        ByRef Quantity4() As Long,
        ByVal StackFree As Integer,
        ByVal Officecode As String,
        ByVal LastFlg As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim NAMEFLG As String = "0"
        Dim PAGEFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇発駅計
        EditStationTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFree)

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B18:AB18")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '数量セット
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = Quantity4(2)
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = Quantity2(2, 1)
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Quantity3(2, 1)
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Quantity2(2, 2)
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity3(2, 2)
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = Quantity2(2, 3)
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Quantity3(2, 3)
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = Quantity2(2, 4)
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Quantity3(2, 4)
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = Quantity2(2, 5)
        WW_Workbook.Worksheets(WW_SheetNo).Range("X" + idx.ToString()).Value = Quantity3(2, 5)
        WW_Workbook.Worksheets(WW_SheetNo).Range("Z" + idx.ToString()).Value = Quantity2(2, 6)
        WW_Workbook.Worksheets(WW_SheetNo).Range("AB" + idx.ToString()).Value = Quantity3(2, 6)
        idx += 1

        For i As Integer = 1 To 6
            Quantity2(2, i) = 0
            Quantity3(2, i) = 0
        Next
        Quantity4(2) = 0

    End Sub

    ''' <summary>
    ''' 発駅計
    ''' </summary>
    Private Sub EditStationTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1() As Long,
        ByRef Quantity2(,) As Long,
        ByRef Quantity3(,) As Long,
        ByRef Quantity4() As Long,
        ByVal StackFree As Integer
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇積空区分別計
        EditStackFreeTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, Quantity3, Quantity4, StackFree)

        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B15:AB15")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        '数量セット
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = Quantity4(1)
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = Quantity2(1, 1)
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Quantity3(1, 1)
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Quantity2(1, 2)
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity3(1, 2)
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = Quantity2(1, 3)
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Quantity3(1, 3)
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = Quantity2(1, 4)
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = Quantity3(1, 4)
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = Quantity2(1, 5)
        WW_Workbook.Worksheets(WW_SheetNo).Range("X" + idx.ToString()).Value = Quantity3(1, 5)
        WW_Workbook.Worksheets(WW_SheetNo).Range("Z" + idx.ToString()).Value = Quantity2(1, 6)
        WW_Workbook.Worksheets(WW_SheetNo).Range("AB" + idx.ToString()).Value = Quantity3(1, 6)
        idx += 1

        For i As Integer = 1 To 6
            Quantity2(1, i) = 0
            Quantity3(1, i) = 0
        Next
        Quantity4(1) = 0
        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString() & ":" & "AB" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("ORGNAME").ToString, PageNum)
        End If

    End Sub

    ''' <summary>
    ''' 積空区分別計
    ''' </summary>
    Private Sub EditStackFreeTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1() As Long,
        ByRef Quantity2(,) As Long,
        ByRef Quantity3(,) As Long,
        ByRef Quantity4() As Long,
        ByVal StackFree As Integer
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0

        '〇算出
        '合計行コピー
        If StackFree = 1 Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B11:AB11")
        Else
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B12:AB12")
        End If
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        If StackFree = 1 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = Quantity1(1)
            Quantity1(1) = 0
        Else
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = Quantity1(2)
            Quantity1(2) = 0
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("AD" + idx.ToString()).Value = "1"
        idx += 1
        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString() & ":" & "AB" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("ORGNAME").ToString, PageNum)
        End If


    End Sub

End Class
