Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 支店間流動表(金額)帳票作成クラス
''' </summary>
Public Class LNT0012_InterBranchChartAmountReport_DIODOC

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

    Private Const Branchcd_Hokkaido = "25"
    Private Const Branchcd_Tohoku = "30"
    Private Const Branchcd_Kantou = "65"
    Private Const Branchcd_Chubu = "75"
    Private Const Branchcd_Kansai = "80"
    Private Const Branchcd_Kyushu = "90"

    Private Const Total_Num = 8
    Private Const Outdistrict_Num = 9
    Private Const Net_Num = 10

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
                If WW_Workbook.Worksheets(i).Name = "支店間流動表(金額)" Then
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
    Public Function CreateExcelPrintData(StackFree As Integer) As String
        Dim ReportName As String = "支店間流動表(金額)_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim lastRow As DataRow = Nothing
            Dim idx As Int32 = 0
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 0
            Dim row_cnt As Int32 = 0
            Dim Fee(Outdistrict_Num, Net_Num, 3) As Long

            For i As Integer = 1 To Outdistrict_Num
                For j As Integer = 1 To Net_Num
                    For k As Integer = 1 To 3
                        Fee(i, j, k) = 0
                    Next
                Next
            Next

            For Each row As DataRow In PrintData.Rows

                row_cnt += 1

                If lastRow IsNot Nothing Then '2行目以降
                    '対象日付FROMが不一致の場合
                    If CDate(row("FROMYMD")) <> CDate(lastRow("FROMYMD")) Then
                        '〇大分類計
                        EditBigctncdTotalArea(idx, lastRow, PageNum, Fee)
                    Else
                        '大分類が不一致の場合
                        If row("BIGCTNCD").ToString <> lastRow("BIGCTNCD").ToString Then
                            '〇大分類計
                            EditBigctncdTotalArea(idx, lastRow, PageNum, Fee)
                        End If
                    End If
                End If

                '金額加算
                If row("JOTDEPBRANCHCD") IsNot DBNull.Value AndAlso row("JOTARRBRANCHCD") IsNot DBNull.Value Then
                    EditFeeArea(row, Fee, StackFree)
                End If

                '最後に出力した行を保存
                lastRow = row

                '最終レコードの場合
                If row_cnt = PrintData.Rows.Count Then
                    '〇大分類計
                    EditBigctncdTotalArea(idx, lastRow, PageNum, Fee)
                    Exit For
                End If

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
        ByVal row As DataRow,
        ByRef idx As Integer,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A2:L7")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能ID
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "LNT0012"
            '〇大分類名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("BIGCTNNM")
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = DateTime.Now
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = pageNum
            '〇ベース
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx + 1).ToString()).Value = row("HEADER_1")
            '〇対象日FROM
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + (idx + 1).ToString()).Value = row("FROMYMD")
            '〇対象日TO
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 1).ToString()).Value = row("TOYMD")
            '〇積空区分
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx + 2).ToString()).Value = row("HEADER_2")

            '行高調整
            WW_Workbook.Worksheets(WW_SheetNo).Rows(idx + 2).RowHeight = 7

            If idx > 30 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:K{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 6

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 大分類計
    ''' </summary>
    Private Sub EditBigctncdTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Fee(,,) As Long
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim rate As Double = 0
        Dim calc As Double = 0
        Dim Fee1 As Long = 0
        Dim Fee2 As Long = 0
        Dim skipflg As String = "0"

        '改頁
        idx += 1
        PageNum += 1
        EditHeaderArea(row, idx, PageNum)

        For AA As Integer = 1 To 7
            For BB As Integer = 1 To 2
                Fee(AA, Outdistrict_Num, BB) = Fee(AA, Total_Num, BB) - Fee(AA, AA, BB)
                Fee(AA, Net_Num, BB) = Fee(Total_Num, AA, BB) - Fee(AA, Total_Num, BB)
                Fee(Outdistrict_Num, AA, BB) = Fee(Total_Num, AA, BB) - Fee(AA, AA, BB)
            Next
        Next

        For AA As Integer = 1 To Outdistrict_Num
            For BB As Integer = 1 To Outdistrict_Num
                If Fee(AA, BB, 1) <> 0 AndAlso Fee(AA, BB, 2) <> 0 Then
                    Fee1 = CType(Fee(AA, BB, 1) / 1000, Long)
                    Fee2 = CType(Fee(AA, BB, 2) / 1000, Long)
                    If Fee1 <> 0 AndAlso Fee2 <> 0 Then
                        rate = Fee(AA, BB, 1) * 100 / Fee(AA, BB, 2)
                        Fee(AA, BB, 3) = CType(Math.Round(rate), Long)
                    End If
                End If
            Next
        Next

        For AA As Integer = 1 To Outdistrict_Num

            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:K12")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

            If AA = 7 Then
                Continue For
            End If

            For CC As Integer = 1 To 3

                skipflg = "0"

                If CC = 1 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = BranchNameGet(AA)
                End If

                If CC = 3 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + (idx + 2).ToString()).Value = Fee(AA, 1, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" + (idx + 2).ToString()).Value = Fee(AA, 2, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" + (idx + 2).ToString()).Value = Fee(AA, 3, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 2).ToString()).Value = Fee(AA, 4, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 2).ToString()).Value = Fee(AA, 5, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 2).ToString()).Value = Fee(AA, 6, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 2).ToString()).Value = Fee(AA, Total_Num, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 2).ToString()).Value = Fee(AA, Outdistrict_Num, CC) / 100
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 2).ToString()).Value = ""
                    skipflg = "1"
                End If

                If skipflg = "0" Then
                    '北海道
                    If Fee(AA, 1, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 1, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '東北
                    If Fee(AA, 2, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 2, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '関東
                    If Fee(AA, 3, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 3, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '中部
                    If Fee(AA, 4, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 4, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '関西
                    If Fee(AA, 5, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 5, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '九州
                    If Fee(AA, 6, CC) = 0 Then
                        calc = 0
                    Else
                        calc = Math.Round(Fee(AA, 6, CC) / 1000, MidpointRounding.AwayFromZero)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + (idx + 1).ToString()).Value = calc
                    End If
                    If AA = Outdistrict_Num Then
                        Fee(Outdistrict_Num, Total_Num, CC) += CType(calc, Long)
                    End If

                    '合計
                    If AA < Outdistrict_Num Then
                        If Fee(AA, Total_Num, CC) = 0 Then
                            calc = 0
                        Else
                            calc = Math.Round(Fee(AA, Total_Num, CC) / 1000, MidpointRounding.AwayFromZero)
                        End If
                    Else
                        calc = 0
                    End If
                    If AA = Outdistrict_Num Then
                        calc = Fee(Outdistrict_Num, Total_Num, CC)
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + (idx + 1).ToString()).Value = calc
                    End If

                    '地区以外
                    If AA < Outdistrict_Num Then
                        If Fee(AA, Outdistrict_Num, CC) = 0 Then
                            calc = 0
                        Else
                            If AA < Total_Num Then
                                calc = Math.Round(Fee(AA, Outdistrict_Num, CC) / 1000, MidpointRounding.AwayFromZero)
                                Fee(Total_Num, Outdistrict_Num, CC) += CType(calc, Long)
                            Else
                                calc = Fee(Total_Num, Outdistrict_Num, CC)
                            End If
                        End If
                    Else
                        calc = 0
                    End If
                    If CC = 1 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = calc
                    ElseIf CC = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + (idx + 1).ToString()).Value = calc
                    End If
                End If
                '差引
                If CC <> 3 Then
                    If AA < Total_Num Then
                        If Fee(AA, Net_Num, CC) = 0 Then
                            calc = 0
                        Else
                            calc = Math.Round(Fee(AA, Net_Num, CC) / 1000, MidpointRounding.AwayFromZero)
                        End If
                    Else
                        calc = 0
                    End If
                End If
                If CC = 1 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = calc
                ElseIf CC = 2 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 1).ToString()).Value = calc
                End If

            Next

            idx += 3

        Next

        For i As Integer = 1 To Outdistrict_Num
            For j As Integer = 1 To Net_Num
                For k As Integer = 1 To 3
                    Fee(i, j, k) = 0
                Next
            Next
        Next

    End Sub

    ''' <summary>
    ''' 金額加算
    ''' </summary>
    Private Sub EditFeeArea(
        ByVal row As DataRow,
        ByRef Fee(,,) As Long,
        ByVal StackFree As Integer
     )

        Dim AA As Integer = 0
        Dim BB As Integer = 0
        Dim UseFee As Long = 0
        Dim FreesendFee As Long = 0

        '発支店
        If row("JOTDEPOLDCD").ToString = Branchcd_Hokkaido Then
            AA = 1
        ElseIf row("JOTDEPOLDCD").ToString = Branchcd_Tohoku Then
            AA = 2
        ElseIf row("JOTDEPOLDCD").ToString = Branchcd_Kantou Then
            AA = 3
        ElseIf row("JOTDEPOLDCD").ToString = Branchcd_Chubu Then
            AA = 4
        ElseIf row("JOTDEPOLDCD").ToString = Branchcd_Kansai Then
            AA = 5
        ElseIf row("JOTDEPOLDCD").ToString = Branchcd_Kyushu Then
            AA = 6
        End If
        '着支店
        If row("JOTARROLDCD").ToString = Branchcd_Hokkaido Then
            BB = 1
        ElseIf row("JOTARROLDCD").ToString = Branchcd_Tohoku Then
            BB = 2
        ElseIf row("JOTARROLDCD").ToString = Branchcd_Kantou Then
            BB = 3
        ElseIf row("JOTARROLDCD").ToString = Branchcd_Chubu Then
            BB = 4
        ElseIf row("JOTARROLDCD").ToString = Branchcd_Kansai Then
            BB = 5
        ElseIf row("JOTARROLDCD").ToString = Branchcd_Kyushu Then
            BB = 6
        End If

        Dim YaerKBN As Integer = CInt(row("YEARKBN"))
        '積
        If row("STACKFREEKBN").ToString = "1" And StackFree = 1 Then
            UseFee = CType(row("USEFEE"), Long) + CType(row("NITTSUFREESEND"), Long) + CType(row("SHIPBURDENFEE"), Long)
            Fee(AA, BB, YaerKBN) += UseFee
            Fee(Total_Num, BB, YaerKBN) += UseFee
            Fee(AA, Total_Num, YaerKBN) += UseFee
            Fee(Total_Num, Total_Num, YaerKBN) += UseFee
        End If
        '空(発送料含む)
        If CInt(row("STACKFREEKBN")) = 2 And StackFree = 3 Then
            FreesendFee = CType(row("FREESENDFEE"), Long) + CType(row("SHIPFEE"), Long)
            Fee(AA, BB, YaerKBN) += FreesendFee
            Fee(Total_Num, BB, YaerKBN) += FreesendFee
            Fee(AA, Total_Num, YaerKBN) += FreesendFee
            Fee(Total_Num, Total_Num, YaerKBN) += FreesendFee
        End If
        '空
        If CInt(row("STACKFREEKBN")) = 2 And StackFree = 5 Then
            FreesendFee = CType(row("FREESENDFEE"), Long)
            Fee(AA, BB, YaerKBN) += FreesendFee
            Fee(Total_Num, BB, YaerKBN) += FreesendFee
            Fee(AA, Total_Num, YaerKBN) += FreesendFee
            Fee(Total_Num, Total_Num, YaerKBN) += FreesendFee
        End If

    End Sub

    ''' <summary>
    ''' 支店名
    ''' </summary>
    Private Function BranchNameGet(ByVal BranchRow As Integer) As String

        Dim BranchName As String = ""

        Select Case BranchRow
            Case 1
                BranchName = "北海道"
            Case 2
                BranchName = "東北"
            Case 3
                BranchName = "関東"
            Case 4
                BranchName = "中部"
            Case 5
                BranchName = "関西"
            Case 6
                BranchName = "九州"
            Case Total_Num
                BranchName = "合計"
            Case Outdistrict_Num
                BranchName = "地区外"
        End Select

        Return BranchName
    End Function
End Class
