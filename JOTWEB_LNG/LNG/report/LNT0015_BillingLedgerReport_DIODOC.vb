Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' リース料請求台帳作成クラス
''' </summary>
Public Class LNT0015_BillingLedgerReport_DIODOC

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

    Private CS0050SESSION As New CS0050SESSION                              'セッション情報操作処理

    Private PrintPageNum As Int32 = 1                                       '現在ページ数　※初期値：1
    Private PrintOutputRowIdx As Int32 = 1                                  '出力位置（行）　※初期値：1
    Private PrintPageRowCnt As Int32 = 1                                    'ページ内出力件数　※初期値：1
    Private PrintPageBreakFlg As Boolean = True                             '改頁フラグ　※初期値：True
    Private PrintNameOutputFlg As Boolean = True                            '名称出力フラグ　※初期値：True（改頁後に名称を再度出力させるフラグ）
    Private PrintaddsheetFlg As Boolean = False                             'シート追加フラグ　※初期値：False
    Private PrintAllInvFlg As String = ""                                   '未承認出力フラグ
    Private PrintReportFileName As String = ""                              'ファイル名
    Private PrintTaxRate As Double = 0                                      '税率
    Private TORITotal As Integer = 0                                        '請求件数
    Private TaxableAmount10Total As Long = 0                                '課税対象額（10%）
    Private ConsumptionTax10Total As Long = 0                               '消費税額（10%）
    Private TotalMoney10 As Long = 0                                        '合計金額（10%）
    Private TaxableAmount08Total As Long = 0                                '課税対象額（8%）
    Private ConsumptionTax08Total As Long = 0                               '消費税額（8%）
    Private TotalMoney08 As Long = 0                                        '合計金額（8%）
    Private TaxableAmount00Total As Long = 0                                '課税対象額（非課税）
    Private TotalMoney00 As Long = 0                                        '合計金額（非課税）
    Private TaxableAmount00_NonTotal As Long = 0                            '課税対象額（不課税）
    Private TotalMoney00_Non As Long = 0                                    '合計金額（不課税）
    Private TaxableAmountTotal As Long = 0                                  '課税対象額
    Private BillingAmountTotal As Long = 0                                  '請求額
    Private ExclusionTotal As Long = 0                                      '計上のみ金額
    Private FirstFLG As String = "0"                                        '初回フラグ
    Private CtnSalseFLG As String = "0"                                     '売却用フラグ

    Private Const REPORT_ID As String = "LNT0015"                           '帳票ID
    Private Const REPORT_NAME As String = "請求台帳"                        '帳票名
    Private Const REPORT_NAME_RENTAL As String =
        "【　レ　ン　タ　ル　料　請　求　台　帳　】"                　　　  'レンタル帳票名
    Private Const REPORT_NAME_LEASE As String =
        "【　リ　ー　ス　料　請　求　台　帳　】"                 　　　　　 'リース帳票名
    Private Const REPORT_NAME_WRITE As String =
        "【　手　書　き　請　求　書　請　求　台　帳　】"                 　 '手書き帳票名
    Private Const REPORT_NAME_CTN As String =
        "【　売　却　請　求　書　請　求　台　帳　】"                     　 '売却コンテナ帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 35                    '改頁行

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, type As String, dblTaxRate As Double)
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
            Me.WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To Me.WW_Workbook.Worksheets.Count - 1
                If Me.WW_Workbook.Worksheets(i).Name = REPORT_NAME Then
                    Me.WW_SheetNo = i
                ElseIf Me.WW_Workbook.Worksheets(i).Name = "temp" Then
                    Me.WW_tmpSheetNo = i
                End If
            Next

            'タイプによって帳票名を分ける
            If type = "1" Then
                Me.PrintReportFileName = "レンタル料請求台帳"
            ElseIf type = "2" Then
                Me.PrintReportFileName = "リース料請求台帳"
            ElseIf type = "3" Then
                Me.PrintReportFileName = "手書き請求書請求台帳"
            ElseIf type = "4" Then
                Me.PrintReportFileName = "売却請求書請求台帳"
            End If

            PrintTaxRate = dblTaxRate

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(maxPage As Hashtable, CtnFlg As String) As String
        Dim TmpFileName As String = Me.PrintReportFileName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte
        Dim INVOICEORGCODE As String = ""
        CtnSalseFLG = CtnFlg

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows

                '請求提出部店変更判定
                If Not INVOICEORGCODE.Equals(OutputRowData("INVOICEORGCODE").ToString) Then
                    INVOICEORGCODE = OutputRowData("INVOICEORGCODE").ToString
                    Me.PrintaddsheetFlg = True
                    If FirstFLG = "1" Then
                        '合計行出力
                        Me.EditTotalAreaFormat()
                        Me.EditTotalArea()

                    End If
                End If

                '行数による改頁判定
                If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                    Me.PrintPageBreakFlg = True
                End If

                'シート追加
                If Me.PrintaddsheetFlg Then
                    TrySetExcelWorkSheet(PrintOutputRowIdx, OutputRowData("NAMES").ToString, PrintPageNum, "請求台帳")
                    If FirstFLG = "0" Then
                        'シート名称を選択した支店に変更する
                        Me.WW_Workbook.Worksheets(WW_SheetNo).Name = OutputRowData("NAMES").ToString
                    End If
                    'シートが切り替わり、ページ数リセット
                    Me.PrintPageNum = 1
                End If

                '改頁の場合、ヘッダ出力（初回出力も含む）
                If Me.PrintPageBreakFlg Or Me.PrintaddsheetFlg Then
                    '頁数分母取得
                    Dim MAX_PAGE As String = CStr(maxPage(OutputRowData("INVOICEORGCODE")))
                    '〇ヘッダー出力
                    Me.EditHeaderArea(OldRowData, OutputRowData, MAX_PAGE)
                    Me.EditDetailAreaFormat(OldRowData, OutputRowData)
                    Me.PrintPageBreakFlg = False
                    Me.PrintaddsheetFlg = False
                    Me.PrintNameOutputFlg = True
                End If

                '〇明細出力
                Me.EditDetailArea(OldRowData, OutputRowData)
                Me.PrintNameOutputFlg = False

                '前回出力明細データ保持
                OldRowData = OutputRowData

                FirstFLG = "1"
            Next

            '合計行出力
            Me.EditTotalAreaFormat()
            Me.EditTotalArea()

            'テンプレート削除
            Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()
            Me.WW_Workbook.Worksheets(0).Delete()

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.WW_Workbook.Save(TmpFilePath, SaveFileFormat.Xlsx)
            End SyncLock

            'ストリーム生成
            Using fs As New IO.FileStream(TmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                Dim binaryLength = Convert.ToInt32(fs.Length)
                ReDim RetByte(binaryLength)
                fs.Read(RetByte, 0, binaryLength)
                fs.Flush()
            End Using
            Return UrlRoot & TmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
        End Try

    End Function

    ''' <summary>
    ''' 出力件数加算
    ''' </summary>
    Private Sub AddPrintRowCnt(ByVal pAddCnt As Int32)
        'EXCEL出力位置加算
        Me.PrintOutputRowIdx += pAddCnt
        'ページ内出力件数加算（ページが切り替わるタイミングで初期化される。改頁判定で使用）
        Me.PrintPageRowCnt += pAddCnt
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
    ''' 帳票ヘッダ出力
    ''' </summary>
    Private Sub EditHeaderArea(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow,
        ByVal maxPage As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim title As String = ""

        Try
            '初回ページは設定しない
            If Not Me.PrintaddsheetFlg And Me.FirstFLG = "1" Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("B{0}:M{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B1:M5")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            'タイトル判別
            If pOutputRowData("INVOICETYPE").ToString = "2" Then
                title = REPORT_NAME_RENTAL
            ElseIf pOutputRowData("INVOICETYPE").ToString = "3" Then
                title = REPORT_NAME_LEASE
            ElseIf pOutputRowData("INVOICETYPE").ToString = "4" Then
                title = REPORT_NAME_WRITE
            ElseIf pOutputRowData("INVOICETYPE").ToString = "5" Then
                title = REPORT_NAME_CTN
            End If

            '対象年月セル編集
            Dim WkYMDateYearStr As String = Left(pOutputRowData("YMDATE").ToString, 4)
            Dim WkYMDateMonthStr As String = Right(pOutputRowData("YMDATE").ToString, 2)
            Dim WkTargetDateStr As String = WkYMDateYearStr + "年" + WkYMDateMonthStr + "月度"

            Me.PrintPageRowCnt = 1
            '◯対象日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr
            '〇帳票タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = title
            '〇処理日
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMDATE")
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMTIME")
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum.ToString + "/" + maxPage + "頁"

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '〇請求提出部店
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value =
                pOutputRowData("NAMES").ToString() + "(" + pOutputRowData("INVOICEORGCODE").ToString() + ")"

            '出力件数加算
            Me.AddPrintRowCnt(4)

            'ページ数加算
            Me.PrintPageNum += 1

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub
    ''' <summary>
    ''' 明細枠出力
    ''' </summary>
    Private Sub EditDetailAreaFormat(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B6:M35")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

    End Sub
    ''' <summary>
    ''' 帳票明細出力
    ''' </summary>
    Private Sub EditDetailArea(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )

        Dim WkTaxableAmount As Long = 0
        Dim ConsumptionTax As Long = 0
        Dim WkTaxableAmount08 As Long = 0
        Dim ConsumptionTax08 As Long = 0
        Dim WkTaxableAmount00 As Long = 0
        Dim WkTaxableAmount00_Non As Long = 0

        Try
            '当該年月セル編集
            Dim WkKEIJOYStr As String = Left(pOutputRowData("KEIJOYM").ToString, 4)
            Dim WkKEIJOMStr As String = Right(pOutputRowData("KEIJOYM").ToString, 2)
            Dim WkKEIJOYMStr As String = WkKEIJOYStr + "年" + WkKEIJOMStr + "月分"

            If pOutputRowData("INVOICETYPE").ToString = "2" Then

                If pOutputRowData("APPROTYPE").ToString = "1" Then
                    WkTaxableAmount =
                        CLng(pOutputRowData("RENTALTOTAL10").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString) +
                        CLng(pOutputRowData("NITTSUFREESEND").ToString) +
                        CLng(pOutputRowData("MANAGEFEE").ToString) +
                        CLng(pOutputRowData("SHIPBURDENFEE").ToString) +
                        CLng(pOutputRowData("PICKUPFEE").ToString) +
                        CLng(pOutputRowData("INCOMEADJUSTFEE").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("RENTALTOTAL08").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("RENTADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("RENTADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                Else
                    WkTaxableAmount =
                        CLng(pOutputRowData("RENTALTOTAL10").ToString) +
                        CLng(pOutputRowData("LEASETOTAL10").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("LEASETOTAL08").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("RENTADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("RENTADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                End If

            ElseIf pOutputRowData("INVOICETYPE").ToString = "3" Then

                If pOutputRowData("APPROTYPE").ToString = "1" Then
                    WkTaxableAmount =
                        CLng(pOutputRowData("LEASETOTAL10").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("LEASETOTAL08").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("RENTADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("RENTADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                Else
                    WkTaxableAmount =
                        CLng(pOutputRowData("RENTALTOTAL10").ToString) +
                        CLng(pOutputRowData("LEASETOTAL10").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("LEASETOTAL08").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("RENTADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("RENTADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                End If

            ElseIf pOutputRowData("INVOICETYPE").ToString = "4" Then

                If pOutputRowData("APPROTYPE").ToString = "1" Then
                    WkTaxableAmount =
                        CLng(pOutputRowData("WRITERENTALTOTAL10").ToString) +
                        CLng(pOutputRowData("WRITELEASETOTAL10").ToString) +
                        CLng(pOutputRowData("WRITERENTALJUSTMENT10").ToString) +
                        CLng(pOutputRowData("WRITELEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("WRITELEASETOTAL08").ToString) +
                        CLng(pOutputRowData("WRITERENTALJUSTMENT08").ToString) +
                        CLng(pOutputRowData("WRITELEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("WRITERENTALJUSTMENT00").ToString) +
                        CLng(pOutputRowData("WRITELEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("WRITERENTALJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("WRITELEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                Else
                    WkTaxableAmount =
                        CLng(pOutputRowData("RENTALTOTAL10").ToString) +
                        CLng(pOutputRowData("LEASETOTAL10").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT10").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE10").ToString)
                    WkTaxableAmount08 =
                        CLng(pOutputRowData("LEASETOTAL08").ToString) +
                        CLng(pOutputRowData("RENTADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT08").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE08").ToString)
                    WkTaxableAmount00 =
                        CLng(pOutputRowData("RENTADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00").ToString)
                    WkTaxableAmount00_Non =
                        CLng(pOutputRowData("RENTADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("LEASEADJUSTMENT00_NON").ToString) +
                        CLng(pOutputRowData("MISCELLANEOUSEXPENSE00_NON").ToString)

                    ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                    ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
                End If

            ElseIf pOutputRowData("INVOICETYPE").ToString = "5" Then
                WkTaxableAmount = CLng(pOutputRowData("CTNTOTAL10").ToString)
                WkTaxableAmount08 = CLng(pOutputRowData("CTNTOTAL08").ToString)
                WkTaxableAmount00 = CLng(pOutputRowData("CTNTOTAL00").ToString)
                WkTaxableAmount00_Non = CLng(pOutputRowData("CTNTOTAL00_NON").ToString)

                ConsumptionTax = CLng(pOutputRowData("TOTALTAX10").ToString)
                ConsumptionTax08 = CLng(pOutputRowData("TOTALTAX08").ToString)
            End If

            '合計金額
            Dim Total As Long =
                WkTaxableAmount + ConsumptionTax
            Dim Total08 As Long =
                WkTaxableAmount08 + ConsumptionTax08

            '請求額
            Dim BillingAmount As Long =
                Total + Total08 + WkTaxableAmount00 + WkTaxableAmount00_Non

            '取引先名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORINAME")
            '当該年月
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = WkKEIJOYMStr
            '課税対象額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount
            '課税対象額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount08
            '課税対象額(非課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount00
            '課税対象額(不課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount00_Non
            '課税対象額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount + WkTaxableAmount08 + WkTaxableAmount00 + WkTaxableAmount00_Non
            '銀行名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BANKNAME")
            '計上のみ金額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("EXCLUSIONFEE")

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '取引先部門名称
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORIDIVNAME")
            If CtnSalseFLG = "0" Then
                '摘要
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SLIPDESCRIPTION1")
            Else
                '摘要
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "コンテナご購入代 " & pOutputRowData("CNT").ToString & "個"
            End If
            '消費税額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = ConsumptionTax
            '消費税額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = ConsumptionTax08
            '請求額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = BillingAmount
            '支店名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BANKBRANCHNAME")

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '取引先名（コード）
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORICODE")
            '摘要
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SLIPDESCRIPTION2")
            '合計金額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = Total
            '合計金額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = Total08
            '合計金額(非課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount00
            '合計金額(不課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = WkTaxableAmount00_Non
            '入金日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPOSITDAY")
            '口座番号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE1").ToString + "　" + pOutputRowData("ACCOUNTNUMBER").ToString


            TORITotal += 1
            TaxableAmount10Total += WkTaxableAmount
            ConsumptionTax10Total += ConsumptionTax
            TotalMoney10 += Total
            TaxableAmount08Total += WkTaxableAmount08
            ConsumptionTax08Total += ConsumptionTax08
            TotalMoney08 += Total08
            TaxableAmount00Total += WkTaxableAmount00
            TotalMoney00 += WkTaxableAmount00
            TaxableAmount00_NonTotal += WkTaxableAmount00_Non
            TotalMoney00_Non += WkTaxableAmount00_Non
            TaxableAmountTotal += WkTaxableAmount + WkTaxableAmount08 + WkTaxableAmount00 + WkTaxableAmount00_Non
            BillingAmountTotal += BillingAmount
            ExclusionTotal += CLng(pOutputRowData("EXCLUSIONFEE"))

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 合計枠出力
    ''' </summary>
    Private Sub EditTotalAreaFormat()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B41:M43")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

    End Sub

    ''' <summary>
    ''' 帳票合計出力
    ''' </summary>
    Private Sub EditTotalArea()

        Try

            '取引先名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = "支店計"
            '当該年月
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "請求先"
            '課税対象額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = TaxableAmount10Total
            '課税対象額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = TaxableAmount08Total
            '課税対象額(非課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = TaxableAmount00Total
            '課税対象額(不課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = TaxableAmount00_NonTotal
            '課税対象額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = TaxableAmountTotal
            '計上のみ金額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = ExclusionTotal

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '摘要
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = TORITotal.ToString + "件"
            '消費税額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = ConsumptionTax10Total
            '消費税額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = ConsumptionTax08Total
            '請求額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = BillingAmountTotal

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '合計金額(10%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = TotalMoney10
            '合計金額(8%)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = TotalMoney08
            '合計金額(非課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = TotalMoney00
            '合計金額(不課税)
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = TotalMoney00_Non

            TORITotal = 0
            TaxableAmount10Total = 0
            ConsumptionTax10Total = 0
            TotalMoney10 = 0
            TaxableAmount08Total = 0
            ConsumptionTax08Total = 0
            TotalMoney08 = 0
            TaxableAmount00Total = 0
            TotalMoney00 = 0
            TaxableAmount00_NonTotal = 0
            TotalMoney00_Non = 0
            TaxableAmountTotal = 0
            BillingAmountTotal = 0
            ExclusionTotal = 0

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub
End Class
