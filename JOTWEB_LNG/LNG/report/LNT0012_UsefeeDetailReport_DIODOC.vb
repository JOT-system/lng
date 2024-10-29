Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 使用料明細表作成クラス
''' </summary>
Public Class LNT0012_UsefeeDetailReport_DIODOC

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
    Private PrintBranchAllFlg As Boolean = False                            '支店フラグ　※初期値：False
    Private PrintNameOutputFlg As Boolean = True                            '名称出力フラグ　※初期値：True（改頁後に名称を再度出力させるフラグ）

    Private Const REPORT_NAME As String = "使用料明細表"                    '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 50                    '改頁行（合計明細行は含めない）

    '合計金額クラス
    Private Class LNT0014_TotalDataClass
        Public QuantityTotal As Long = 0           '個数
        Public UseFeeTotal As Long = 0             '使用料
        Public NittsuFreesendTotal As Long = 0     '通運負担回送運賃
        Public ManageFeeTotal As Long = 0          '運行管理料(元請輸送費)
        Public ShipburdenFeeTotal As Long = 0      '荷主負担運賃(元請輸送費)
        Public PicupFeeTotal As Long = 0           '集荷料(元請輸送費)
        Public IncomeadjustFeeTotal As Long = 0    '加減額
        Public Total As Long = 0                   '請求合計

        '金額クリア処理
        Public Sub Clear()
            Me.QuantityTotal = 0
            Me.UseFeeTotal = 0
            Me.NittsuFreesendTotal = 0
            Me.ManageFeeTotal = 0
            Me.ShipburdenFeeTotal = 0
            Me.PicupFeeTotal = 0
            Me.IncomeadjustFeeTotal = 0
            Me.Total = 0
        End Sub

        '金額加算処理
        Public Sub CalcAdd(Quantity As Long, UseFee As Long, NittsuFreesend As Long,
            ManageFee As Long, ShipburdenFee As Long, PicupFee As Long, IncomeadjustFee As Long, Total As Long)
            Me.QuantityTotal += Quantity
            Me.UseFeeTotal += UseFee
            Me.NittsuFreesendTotal += NittsuFreesend
            Me.ManageFeeTotal += ManageFee
            Me.ShipburdenFeeTotal += ShipburdenFee
            Me.PicupFeeTotal += PicupFee
            Me.IncomeadjustFeeTotal += IncomeadjustFee
            Me.Total += Total
        End Sub
    End Class

    '合計金額クラス保持変数
    Private AllTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private BranchTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private InvoiceTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()

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
            Me.WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To Me.WW_Workbook.Worksheets.Count - 1
                If Me.WW_Workbook.Worksheets(i).Name = REPORT_NAME Then
                    Me.WW_SheetNo = i
                ElseIf Me.WW_Workbook.Worksheets(i).Name = "temp" Then
                    Me.WW_tmpSheetNo = i
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
    Public Function CreateExcelPrintData() As String
        Dim TmpFileName As String = REPORT_NAME & "_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）
            Dim OldAddsubTable As New DataTable

            '金額追加のヘッダー設定
            'For Each Col As DataColumn In PrintData(0).Table.Columns
            '    OldAddsubTable.Columns.Add(Col.ColumnName)
            'Next

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows
                '小計出力判定
                If OldRowData IsNot Nothing Then
                    '支払先計
                    If Not OldRowData("INVFILINGDEPT").ToString.Equals(OutputRowData("INVFILINGDEPT").ToString) OrElse
                       Not OldRowData("TORICODE").ToString.Equals(OutputRowData("TORICODE").ToString) Then
                        '〇小計（支払先計）出力
                        Me.EditInvoiceTotalArea()
                    End If

                    '支店計
                    If Not OldRowData("INVFILINGDEPT").ToString.Equals(OutputRowData("INVFILINGDEPT").ToString) Then
                        '〇小計（支店計）出力
                        Me.EditBranchTotalArea()
                        Me.PrintPageBreakFlg = True
                        PrintBranchAllFlg = True
                    End If

                    '行数による改頁判定
                    If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                        Me.PrintPageBreakFlg = True
                    End If

                End If

                '改頁の場合、ヘッダ出力（初回出力も含む）
                If Me.PrintPageBreakFlg Then
                    '〇ヘッダー出力
                    Me.EditHeaderArea(OldRowData, OutputRowData)
                    Me.PrintPageBreakFlg = False
                    Me.PrintNameOutputFlg = True
                End If

                '〇明細出力
                If OutputRowData("AMOUNTTYPE").ToString = "0" Then
                    Me.EditDetailArea(OldRowData, OutputRowData)
                End If
                '金額追加テーブル格納
                If OutputRowData("AMOUNTTYPE").ToString = "1" Then
                    Me.EditAddsubDetailArea(OutputRowData)
                End If
                Me.PrintNameOutputFlg = False

                '前回出力明細データ保持
                OldRowData = OutputRowData
            Next

            '最終行金額追加出力
            If OldRowData("AMOUNTTYPE").ToString = "1" Then
                For Each OutputAddsubData As DataRow In OldAddsubTable.Rows
                    Me.EditAddsubDetailArea(OutputAddsubData)
                Next
            End If
            '〇小計（支払先計）出力
            Me.EditInvoiceTotalArea()
            '〇小計（支店計）出力
            Me.EditBranchTotalArea()
            '全支店選択時総合計出力
            If PrintBranchAllFlg Then
                '〇総合計出力
                Me.EditAllTotalArea()
            End If

            '印刷範囲設定
            Dim pagebreak As IRange = Nothing
            pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:N{0}", Me.PrintOutputRowIdx))
            WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)

            'テンプレート削除
            Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()

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
    ''' 帳票ヘッダ出力
    ''' </summary>
    Private Sub EditHeaderArea(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '初回ページは設定しない
            If pOldRowData IsNot Nothing Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:N{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:R4")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim WkYMDateYearStr As String = Left(pOutputRowData("KEIJOYM").ToString, 4)
            Dim WkYMDateMonthStr As String = Right(pOutputRowData("KEIJOYM").ToString, 2)
            Dim WkTargetDateStr As String = WkYMDateYearStr + "年" + WkYMDateMonthStr + "月分"

            Me.PrintPageRowCnt = 1
            '〇タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = "（" + pOutputRowData("INVOICETYPE").ToString + "）"
            '◯対象日付（YYYY年MM月分（1日～31日））
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr
            '〇処理日
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMDATE")
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMTIME")
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum

            '出力件数加算
            Me.AddPrintRowCnt(2)
            '〇支店名
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("INVORGNAME")

            '出力件数加算
            Me.AddPrintRowCnt(2)

            'ページ数加算
            Me.PrintPageNum += 1

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票明細出力
    ''' </summary>
    Private Sub EditDetailArea(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:R7")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)
        '請求先名称
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("TORICODE").ToString.Equals(pOutputRowData("TORICODE").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORINAME").ToString + pOutputRowData("TORIDIVNAME").ToString
        End If
        '請求先コード
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("TORICODE").ToString.Equals(pOutputRowData("TORICODE").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORICODE")
        End If
        '発駅
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("DEPNAME").ToString.Equals(pOutputRowData("DEPNAME").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPNAME")
        End If
        '着駅
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("DEPNAME").ToString.Equals(pOutputRowData("DEPNAME").ToString) OrElse
               Not pOldRowData("ARRNAME").ToString.Equals(pOutputRowData("ARRNAME").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRNAME")
        End If
        '発送月日
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHIPYMD")
        'コンテナ番号（記号 - 番号）
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CTNTYPE").ToString + " - " + pOutputRowData("CTNNO").ToString
        '個数
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("QUANTITY")
        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("JRFIXEDFARE")
        '私有割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("OWNDISCOUNTFEE")
        '割戻
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("RETURNFARE")
        '固定使用料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("FIXEDFEE")
        '使用料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("USEFEE")
        '通運負担回送運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("NITTSUFREESEND")
        '運行管理料（元請輸送費）
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("MANAGEFEE")
        '荷主負担運賃（元請輸送費）
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHIPBURDENFEE")
        '集荷量（元請輸送費）
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("PICKUPFEE")
        '加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("INCOMEADJUSTFEE")
        '請求合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = CInt(pOutputRowData("FIXEDFEE")) + CInt(pOutputRowData("USEFEE")) + CInt(pOutputRowData("NITTSUFREESEND")) +
                                                                                                   CInt(pOutputRowData("MANAGEFEE")) + CInt(pOutputRowData("SHIPBURDENFEE")) +
                                                                                                   CInt(pOutputRowData("PICKUPFEE")) + CInt(pOutputRowData("INCOMEADJUSTFEE"))

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計金額加算
        '総合計
        Me.AllTotalRow.CalcAdd(
            CLng(pOutputRowData("QUANTITY").ToString),
            CLng(pOutputRowData("USEFEE").ToString),
            CLng(pOutputRowData("NITTSUFREESEND").ToString),
            CLng(pOutputRowData("MANAGEFEE").ToString),
            CLng(pOutputRowData("SHIPBURDENFEE").ToString),
            CLng(pOutputRowData("PICKUPFEE").ToString),
            CLng(pOutputRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOutputRowData("FIXEDFEE")) + CLng(pOutputRowData("USEFEE")) + CLng(pOutputRowData("NITTSUFREESEND")) +
            CLng(pOutputRowData("MANAGEFEE")) + CLng(pOutputRowData("SHIPBURDENFEE")) +
            CLng(pOutputRowData("PICKUPFEE")) + CLng(pOutputRowData("INCOMEADJUSTFEE")))

        '支店計
        Me.BranchTotalRow.CalcAdd(
            CLng(pOutputRowData("QUANTITY").ToString),
            CLng(pOutputRowData("USEFEE").ToString),
            CLng(pOutputRowData("NITTSUFREESEND").ToString),
            CLng(pOutputRowData("MANAGEFEE").ToString),
            CLng(pOutputRowData("SHIPBURDENFEE").ToString),
            CLng(pOutputRowData("PICKUPFEE").ToString),
            CLng(pOutputRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOutputRowData("FIXEDFEE")) + CLng(pOutputRowData("USEFEE")) + CLng(pOutputRowData("NITTSUFREESEND")) +
            CLng(pOutputRowData("MANAGEFEE")) + CLng(pOutputRowData("SHIPBURDENFEE")) +
            CLng(pOutputRowData("PICKUPFEE")) + CLng(pOutputRowData("INCOMEADJUSTFEE")))

        '支払先計
        Me.InvoiceTotalRow.CalcAdd(
            CLng(pOutputRowData("QUANTITY").ToString),
            CLng(pOutputRowData("USEFEE").ToString),
            CLng(pOutputRowData("NITTSUFREESEND").ToString),
            CLng(pOutputRowData("MANAGEFEE").ToString),
            CLng(pOutputRowData("SHIPBURDENFEE").ToString),
            CLng(pOutputRowData("PICKUPFEE").ToString),
            CLng(pOutputRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOutputRowData("FIXEDFEE")) + CLng(pOutputRowData("USEFEE")) + CLng(pOutputRowData("NITTSUFREESEND")) +
            CLng(pOutputRowData("MANAGEFEE")) + CLng(pOutputRowData("SHIPBURDENFEE")) +
            CLng(pOutputRowData("PICKUPFEE")) + CLng(pOutputRowData("INCOMEADJUSTFEE")))

    End Sub

    ''' <summary>
    ''' 帳票総合計出力
    ''' </summary>
    Private Sub EditAllTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A14:R14")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '支店計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = "総合計"
        '個数
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.QuantityTotal
        '使用料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.UseFeeTotal
        '通運負担回送運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.NittsuFreesendTotal
        '運行管理料(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.ManageFeeTotal
        '荷主負担運賃(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.ShipburdenFeeTotal
        '集荷量(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.PicupFeeTotal
        '加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.IncomeadjustFeeTotal
        '請求合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '総合計クリア
        Me.AllTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票小計（支店計）出力
    ''' </summary>
    Private Sub EditBranchTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A12:R12")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '支店計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = "支店計"
        '個数
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.QuantityTotal
        '使用料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.UseFeeTotal
        '通運負担回送運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.NittsuFreesendTotal
        '運行管理料(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.ManageFeeTotal
        '荷主負担運賃(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.ShipburdenFeeTotal
        '集荷量(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.PicupFeeTotal
        '加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.IncomeadjustFeeTotal
        '請求合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計（支店計）クリア
        Me.BranchTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票小計（支払先計）出力
    ''' </summary>
    Private Sub EditInvoiceTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:R10")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '支計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = "支払先計"
        '個数
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.QuantityTotal
        '使用料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.UseFeeTotal
        '通運負担回送運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.NittsuFreesendTotal
        '運行管理料(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.ManageFeeTotal
        '荷主負担運賃(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.ShipburdenFeeTotal
        '集荷量(元請輸送費)
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.PicupFeeTotal
        '加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.IncomeadjustFeeTotal
        '請求合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = Me.InvoiceTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計（支払先計）クリア
        Me.InvoiceTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票加減額明細出力
    ''' </summary>
    Private Sub EditAddsubDetailArea(
        ByVal pOldRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:R7")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '種別
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = "加減額"
        '発駅
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("DEPNAME").ToString
        '着駅
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("ARRNAME").ToString
        '金額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = CLng(pOldRowData("USEFEE").ToString)
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = CLng(pOldRowData("USEFEE").ToString)

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計金額加算
        '総合計
        Me.AllTotalRow.CalcAdd(
            CLng(pOldRowData("QUANTITY").ToString),
            CLng(pOldRowData("USEFEE").ToString),
            CLng(pOldRowData("NITTSUFREESEND").ToString),
            CLng(pOldRowData("MANAGEFEE").ToString),
            CLng(pOldRowData("SHIPBURDENFEE").ToString),
            CLng(pOldRowData("PICKUPFEE").ToString),
            CLng(pOldRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOldRowData("FIXEDFEE")) + CLng(pOldRowData("USEFEE")) + CLng(pOldRowData("NITTSUFREESEND")) +
            CLng(pOldRowData("MANAGEFEE")) + CLng(pOldRowData("SHIPBURDENFEE")) +
            CLng(pOldRowData("PICKUPFEE")) + CLng(pOldRowData("INCOMEADJUSTFEE")))

        '支店計
        Me.BranchTotalRow.CalcAdd(
            CLng(pOldRowData("QUANTITY").ToString),
            CLng(pOldRowData("USEFEE").ToString),
            CLng(pOldRowData("NITTSUFREESEND").ToString),
            CLng(pOldRowData("MANAGEFEE").ToString),
            CLng(pOldRowData("SHIPBURDENFEE").ToString),
            CLng(pOldRowData("PICKUPFEE").ToString),
            CLng(pOldRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOldRowData("FIXEDFEE")) + CLng(pOldRowData("USEFEE")) + CLng(pOldRowData("NITTSUFREESEND")) +
            CLng(pOldRowData("MANAGEFEE")) + CLng(pOldRowData("SHIPBURDENFEE")) +
            CLng(pOldRowData("PICKUPFEE")) + CLng(pOldRowData("INCOMEADJUSTFEE")))

        '支払先計
        Me.InvoiceTotalRow.CalcAdd(
            CLng(pOldRowData("QUANTITY").ToString),
            CLng(pOldRowData("USEFEE").ToString),
            CLng(pOldRowData("NITTSUFREESEND").ToString),
            CLng(pOldRowData("MANAGEFEE").ToString),
            CLng(pOldRowData("SHIPBURDENFEE").ToString),
            CLng(pOldRowData("PICKUPFEE").ToString),
            CLng(pOldRowData("INCOMEADJUSTFEE").ToString),
            CLng(pOldRowData("FIXEDFEE")) + CLng(pOldRowData("USEFEE")) + CLng(pOldRowData("NITTSUFREESEND")) +
            CLng(pOldRowData("MANAGEFEE")) + CLng(pOldRowData("SHIPBURDENFEE")) +
            CLng(pOldRowData("PICKUPFEE")) + CLng(pOldRowData("INCOMEADJUSTFEE")))

    End Sub

End Class
