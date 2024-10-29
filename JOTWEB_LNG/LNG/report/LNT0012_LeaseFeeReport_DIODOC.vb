Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' リース料明細チェックリスト帳票作成クラス
''' </summary>
Public Class LNT0012_LeaseFeeReport_DIODOC

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

    Private WW_HeadFlg As String = "0"

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
                If WW_Workbook.Worksheets(i).Name = "リース料明細チェックリスト" Then
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
    Public Function CreateExcelPrintData(type As String) As String
        Dim ReportName As String = ""
        If type = "1" Then
            ReportName = "請求書提出店所別 リース料明細チェックリスト"
        Else
            ReportName = "収入計上店所別 リース料明細チェックリスト"
        End If
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim lastRow As DataRow = Nothing
            Dim idx As Int32 = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 1
            Dim row_cnt As Int32 = 0
            Dim Count(4) As Long
            Dim TunTotal(4) As Double
            Dim FeeTotal(4) As Long
            Dim TaxTotal(4) As Long
            Dim Key_Branch_New As Integer = 0
            Dim Key_Branch_Old As Integer = 0

            For i As Integer = 0 To 4
                Count(i) = 0
                TunTotal(i) = 0
                FeeTotal(i) = 0
                TaxTotal(i) = 0
            Next

            For Each row As DataRow In PrintData.Rows

                row_cnt += 1

                '1行目
                If lastRow Is Nothing Then
                    '〇ヘッダー情報セット
                    EditHeaderArea(row, idx, type, PageNum)

                Else '2行目以降
                    If type = "1" Then
                        Key_Branch_New = CType(row("INVOICEOUTORGCD"), Integer)
                        Key_Branch_Old = CType(lastRow("INVOICEOUTORGCD"), Integer)
                    Else
                        Key_Branch_New = CType(row("KEIJOORGCD"), Integer)
                        Key_Branch_Old = CType(lastRow("KEIJOORGCD"), Integer)
                    End If
                    '支店が不一致の場合
                    If Key_Branch_New <> Key_Branch_Old Then
                        '〇支店計
                        EditBranchcdTotalArea(idx, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)
                        If WW_HeadFlg = "0" Then
                            '〇改頁
                            EditPage(idx, row, PageNum, type)
                        Else
                            PageNum += 1
                            EditHeaderArea(row, idx, type, PageNum)
                        End If

                    Else
                        '請求先コードが不一致の場合
                        If row("TORICODE").ToString <> lastRow("TORICODE").ToString Then
                            '〇請求先計
                            EditToricdTotalArea(idx, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)
                        Else
                            '大分類が不一致の場合
                            If row("BIGCTNCD").ToString <> lastRow("BIGCTNCD").ToString Then
                                '〇大分類計
                                EditBigctncdTotalArea(idx, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)
                            Else
                                '契約形態が不一致の場合
                                If row("CONTRALNTYPE").ToString <> lastRow("CONTRALNTYPE").ToString Then
                                    '〇契約形態計
                                    EditContraLNTotalArea(idx, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)
                                End If
                            End If
                        End If
                    End If
                End If
                '明細セット
                EditDetailArea(idx, row, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)

                '最後に出力した行を保存
                lastRow = row

                '最終レコードの場合
                If row_cnt = PrintData.Rows.Count Then
                    '〇総合計
                    EditTotalArea(idx, lastRow, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)
                    Exit For
                End If
            Next

            'シート名変更
            If type = "1" Then
                WW_Workbook.Worksheets(WW_SheetNo).Name = "請求書提出部店基準"
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Name = "収入計上部店基準"
            End If

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
        ByVal type As String,
        ByVal pageNum As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B2:Q6")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇対象年月
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = CType(row("TARGETYM").ToString & "/1", Date)
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = DateTime.Now
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + (idx + 2).ToString()).Value = pageNum

            '請求書提出店所別
            If type = "1" Then
                '〇タイトル
                WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = "請求書提出店所別　リース料明細チェックリスト"
                '〇支店名
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx + 1).ToString()).Value = "請求書提出店所　" & row("INVOICEOUTORGNM").ToString
            Else
                '収入計上店所別
                '〇タイトル
                WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = "収入計上店所別　リース料明細チェックリスト"
                '〇支店名
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (idx + 1).ToString()).Value = "収入店所　" & row("KEIJOORGNM").ToString
                '〇ヘッダー変更
                WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + (idx + 3).ToString()).Value = "請求書発行部署"
            End If
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 4).ToString()).Value = "1"

            If idx > 36 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:Q{0}", idx))
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
         ByRef Count() As Long,
         ByRef TunTotal() As Double,
         ByRef FeeTotal() As Long,
         ByRef TaxTotal() As Long,
         ByVal type As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim RowFLG As String = ""
        Dim daycount As Integer = 0
        Dim amount As Long = 0
        Dim unitamount As Double = 0
        Dim htZerit As Hashtable = Nothing
        Dim taxrate As Double = 0
        Dim taxamount As Long = 0

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B9:Q9")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇セット
        '請求先コード
        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("TORICODE")
        '請求会社名
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("TORINAME")
        '請求会社部門名
        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = row("TORIDIVNAME")

        RowFLG = WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + (idx - 1).ToString()).Text
        If RowFLG <> "1" And lastrow IsNot Nothing Then
            If row("TORICODE").ToString = lastrow("TORICODE").ToString Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                If row("TORINAME").ToString = lastrow("TORINAME").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = ""
                    If row("TORIDIVNAME").ToString = lastrow("TORIDIVNAME").ToString Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = ""
                    End If
                End If
            End If
        End If
        '契約形態
        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("CONTRALNTYPENM")
        'コンテナ種別
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("BIGCTNNM")
        '品名
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("CTNTYPE")
        'コンテナ番号
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = row("CTNNO")
        '屯数
        If row("CARGOWEIGHT").ToString <> "0.0" Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = row("CARGOWEIGHT")
        '使用日数(自)
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("LEASEMONTHSTARTYMD")
        '使用日数(至)
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = row("LEASEMONTHENDYMD")
        '日数
        daycount = (CType(row("LEASEMONTHENDYMD"), Date) - CType(row("LEASEMONTHSTARTYMD"), Date)).Days + 1
        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = daycount
        '単価、金額
        If row("DAILYRATE") Is DBNull.Value Then
            amount = CType(row("MONTHLEASEFEE"), Long)
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("MONTHLEASEFEE")
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("MONTHLEASEFEE")
        Else
            If CType(row("DAILYRATE"), Integer) <> 0 Then
                amount = CType(row("DAILYRATE"), Long)
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Math.Round(CType(row("DAILYRATE"), Long) / daycount, MidpointRounding.AwayFromZero)
                WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("DAILYRATE")
            Else
                amount = CType(row("MONTHLEASEFEE"), Long)
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("MONTHLEASEFEE")
                WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("MONTHLEASEFEE")
            End If
        End If
        '税率
        taxrate = CType(row("TAXRATE"), Integer) / 100
        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = CInt(row("TAXRATE").ToString)
        '税額
        taxamount = CType(Math.Round(amount * taxrate, MidpointRounding.AwayFromZero), Long)
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Math.Round(amount * taxrate, MidpointRounding.AwayFromZero)

        If type = "1" Then
            '計上部店
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = row("KEIJOORGNM")
        Else
            '請求書発行部署
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = row("INVOICEOUTORGNM")
        End If

        For i As Integer = 0 To 4
            Count(i) += 1
            TunTotal(i) += CType(row("CARGOWEIGHT"), Double)
            FeeTotal(i) += amount
            TaxTotal(i) += taxamount
        Next

        idx += 1

    End Sub

    ''' <summary>
    ''' 総合計
    ''' </summary>
    Private Sub EditTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Count() As Long,
        ByRef TunTotal() As Double,
        ByRef FeeTotal() As Long,
        ByRef TaxTotal() As Long,
        ByVal type As String
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim headflg As String = "0"

        '支店計
        EditBranchcdTotalArea(idx, row, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        Modcnt = (idx + 1) Mod 36
        If Modcnt = 0 Then
            headflg = "1"
        End If

        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B28:Q29")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇件数
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Count(4)
        '〇
        If TunTotal(4) <> 0 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = TunTotal(4)
        '〇合計金額
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = FeeTotal(4)
        '〇合計税額
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = TaxTotal(4)

        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = "1"

        idx += 2

        If headflg = "1" Then
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If

        Count(4) = 0
        TunTotal(4) = 0
        FeeTotal(4) = 0
        TaxTotal(4) = 0

    End Sub

    ''' <summary>
    ''' 支店計
    ''' </summary>
    Private Sub EditBranchcdTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Count() As Long,
        ByRef TunTotal() As Double,
        ByRef FeeTotal() As Long,
        ByRef TaxTotal() As Long,
        ByVal type As String
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        WW_HeadFlg = "0"

        '請求先計
        EditToricdTotalArea(idx, row, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        Modcnt = (idx + 1) Mod 36
        If Modcnt = 0 Then
            WW_HeadFlg = "1"
        End If

        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B24:Q25")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇件数
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Count(3)
        '〇
        If TunTotal(3) <> 0 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = TunTotal(3)
        '〇合計金額
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = FeeTotal(3)
        '〇合計税額
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = TaxTotal(3)

        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = "1"

        idx += 2

        Count(3) = 0
        TunTotal(3) = 0
        FeeTotal(3) = 0
        TaxTotal(3) = 0

    End Sub

    ''' <summary>
    ''' 請求先計
    ''' </summary>
    Private Sub EditToricdTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Count() As Long,
        ByRef TunTotal() As Double,
        ByRef FeeTotal() As Long,
        ByRef TaxTotal() As Long,
        ByVal type As String
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim headflg As String = "0"

        '大分類計
        EditBigctncdTotalArea(idx, row, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        Modcnt = (idx + 1) Mod 36
        If Modcnt = 0 Then
            headflg = "1"
        End If

        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B20:Q21")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇件数
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Count(2)
        '〇
        If TunTotal(2) <> 0 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = TunTotal(2)
        '〇合計金額
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = FeeTotal(2)
        '〇合計税額
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = TaxTotal(2)

        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = "1"

        idx += 2

        If headflg = "1" Then
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If

        Count(2) = 0
        TunTotal(2) = 0
        FeeTotal(2) = 0
        TaxTotal(2) = 0

    End Sub

    ''' <summary>
    ''' 大分類計
    ''' </summary>
    Private Sub EditBigctncdTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Count() As Long,
        ByRef TunTotal() As Double,
        ByRef FeeTotal() As Long,
        ByRef TaxTotal() As Long,
        ByVal type As String
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim headflg As String = "0"

        '契約形態計
        EditContraLNTotalArea(idx, row, PageNum, Count, TunTotal, FeeTotal, TaxTotal, type)

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        Modcnt = (idx + 1) Mod 36
        If Modcnt = 0 Then
            headflg = "1"
        End If

        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B16:Q17")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇大分類名称
        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("BIGCTNNM").ToString.TrimEnd & "コンテナ"
        '〇件数
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Count(1)
        '〇合計屯数
        If TunTotal(1) <> 0 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = TunTotal(1)
        '〇合計金額
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = FeeTotal(1)
        '〇合計税額
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = TaxTotal(1)

        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = "1"

        idx += 2

        If headflg = "1" Then
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If

        Count(1) = 0
        TunTotal(1) = 0
        FeeTotal(1) = 0
        TaxTotal(1) = 0

    End Sub

    ''' <summary>
    ''' 契約形態計
    ''' </summary>
    Private Sub EditContraLNTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByRef PageNum As Integer,
        ByRef Count() As Long,
        ByRef TunTotal() As Double,
        ByRef FeeTotal() As Long,
        ByRef TaxTotal() As Long,
        ByVal type As String
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0
        Dim headflg As String = "0"

        '改頁判断
        Modcnt = idx Mod 36
        If Modcnt = 0 Then
            idx += 1
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If
        Modcnt = (idx + 1) Mod 36
        If Modcnt = 0 Then
            headflg = "1"
        End If

        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B12:Q13")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇大分類名称
        If row("CONTRALNTYPE").ToString = "1" Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("CONTRALNTYPENM").ToString.TrimEnd & "計"
        Else
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("CONTRALNTYPENM").ToString.TrimEnd & "リース計"
        End If
        '〇件数
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Count(0)
        '〇合計屯数
        If TunTotal(0) <> 0 Then
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).NumberFormat = "##.0"
        End If
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = TunTotal(0)
        '〇合計金額
        WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = FeeTotal(0)
        '〇合計税額
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = TaxTotal(0)

        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = "1"

        idx += 2

        If headflg = "1" Then
            PageNum += 1
            EditHeaderArea(row, idx, type, PageNum)
        End If

        Count(0) = 0
        TunTotal(0) = 0
        FeeTotal(0) = 0
        TaxTotal(0) = 0

    End Sub

    ''' <summary>
    ''' 改頁処理
    ''' </summary>
    Private Sub EditPage(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByVal type As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        '改頁
        While 0 = 0
            Modcnt = idx Mod 36
            If Modcnt = 0 Then
                idx += 1
                PageNum += 1
                EditHeaderArea(row, idx, type, PageNum)
                Exit While
            Else
                idx += 1
            End If
        End While

    End Sub

End Class
