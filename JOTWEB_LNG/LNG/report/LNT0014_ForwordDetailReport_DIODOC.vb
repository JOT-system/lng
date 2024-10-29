Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 回送費明細表作成クラス
''' </summary>
Public Class LNT0014_ForwordDetailReport_DIODOC

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

    Private Const REPORT_ID As String = "LNT0014"                           '帳票ID
    Private Const REPORT_NAME As String = "レンタルコンテナ回送費明細表"    '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 50                    '改頁行（合計明細行は含めない）

    '合計金額クラス
    Private Class LNT0014_TotalDataClass
        Public PrescribedFareTotal As Long = 0     '所定運賃
        Public DiscountTotal As Long = 0           '割引
        Public ApplicableFareTotal As Long = 0     '適用運賃
        Public ShippingFeeTotal As Long = 0        '発送料
        Public CommissionTotal As Long = 0         '手数料
        Public AmountOfAdjustmentTotal As Long = 0 '費用加減額
        Public SubTotal As Long = 0                '小計
        Public Total As Long = 0                   '合計

        '金額クリア処理
        Public Sub Clear()
            Me.PrescribedFareTotal = 0
            Me.DiscountTotal = 0
            Me.ApplicableFareTotal = 0
            Me.ShippingFeeTotal = 0
            Me.CommissionTotal = 0
            Me.AmountOfAdjustmentTotal = 0
            Me.SubTotal = 0
            Me.Total = 0
        End Sub

        '金額加算処理
        Public Sub CalcAdd(PrescribedFare As Long, Discount As Long, ApplicableFare As Long,
            ShippingFee As Long, Commission As Long, AmountOfAdjustment As Long, SubAmount As Long, Amount As Long)
            Me.PrescribedFareTotal += PrescribedFare
            Me.DiscountTotal += Discount
            Me.ApplicableFareTotal += ApplicableFare
            Me.ShippingFeeTotal += ShippingFee
            Me.CommissionTotal += Commission
            Me.AmountOfAdjustmentTotal += AmountOfAdjustment
            Me.SubTotal += SubAmount
            Me.Total += Amount
        End Sub
    End Class

    '合計金額クラス保持変数
    Private AllTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private BranchTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private PaymentTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private ArrStationTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()
    Private BigCtnTotalRow As LNT0014_TotalDataClass = New LNT0014_TotalDataClass()

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
        Dim TmpFileName As String = REPORT_NAME & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）
            Dim OldAddsubTable As New DataTable

            '金額追加のヘッダー設定
            For Each Col As DataColumn In PrintData(0).Table.Columns
                OldAddsubTable.Columns.Add(Col.ColumnName)
            Next

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows
                '小計出力判定
                If OldRowData IsNot Nothing Then
                    '種別計
                    If Not OldRowData("JOTDEPBRANCHCD").ToString.Equals(OutputRowData("JOTDEPBRANCHCD").ToString) OrElse
                       Not OldRowData("TORICODE").ToString.Equals(OutputRowData("TORICODE").ToString) OrElse
                       Not OldRowData("DEPSTATIONNM").ToString.Equals(OutputRowData("DEPSTATIONNM").ToString) OrElse
                       Not OldRowData("ARRSTATIONNM").ToString.Equals(OutputRowData("ARRSTATIONNM").ToString) OrElse
                       Not OldRowData("BIGCTNNM").ToString.Equals(OutputRowData("BIGCTNNM").ToString) Then
                        If OutputRowData("AMOUNTTYPE").ToString = "0" Then
                            '〇小計（種別計）出力
                            Me.EditBigCtnTotalArea()
                        End If
                    End If

                    '着駅計
                    If Not OldRowData("JOTDEPBRANCHCD").ToString.Equals(OutputRowData("JOTDEPBRANCHCD").ToString) OrElse
                       Not OldRowData("TORICODE").ToString.Equals(OutputRowData("TORICODE").ToString) OrElse
                       Not OldRowData("DEPSTATIONNM").ToString.Equals(OutputRowData("DEPSTATIONNM").ToString) OrElse
                       Not OldRowData("ARRSTATIONNM").ToString.Equals(OutputRowData("ARRSTATIONNM").ToString) Then
                        If OutputRowData("AMOUNTTYPE").ToString = "0" Then
                            '〇小計（着駅計）出力
                            Me.EditArrStationTotalArea()
                        End If
                    End If

                    '支払先計
                    If Not OldRowData("JOTDEPBRANCHCD").ToString.Equals(OutputRowData("JOTDEPBRANCHCD").ToString) OrElse
                       Not OldRowData("TORICODE").ToString.Equals(OutputRowData("TORICODE").ToString) Then
                        If OldRowData("AMOUNTTYPE").ToString = "1" Then
                            'テーブルから金額追加出力
                            For Each OutputAddsubData As DataRow In OldAddsubTable.Rows
                                Me.EditAddsubDetailArea(OutputAddsubData)
                            Next
                            OldAddsubTable.Clear()
                            '行数による改頁判定
                            If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                                '〇ヘッダー出力
                                Me.EditHeaderArea(OldRowData, OldRowData)
                                Me.PrintPageBreakFlg = False
                                Me.PrintNameOutputFlg = True
                            End If
                        End If
                        '〇小計（支払先計）出力
                        Me.EditPaymentTotalArea()
                        Me.PrintPageBreakFlg = True
                    End If

                    '支店計
                    If Not OldRowData("JOTDEPBRANCHCD").ToString.Equals(OutputRowData("JOTDEPBRANCHCD").ToString) Then
                        '〇小計（支店計）出力
                        Me.EditBranchTotalArea()
                        Me.PrintPageBreakFlg = True
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
                    OldAddsubTable.ImportRow(OutputRowData)
                End If
                Me.PrintNameOutputFlg = False

                '前回出力明細データ保持
                OldRowData = OutputRowData
            Next

            '〇小計（種別計）出力
            Me.EditBigCtnTotalArea()
            '〇小計（着駅計）出力
            Me.EditArrStationTotalArea()
            '最終行金額追加出力
            If OldRowData("AMOUNTTYPE").ToString = "1" Then
                For Each OutputAddsubData As DataRow In OldAddsubTable.Rows
                    Me.EditAddsubDetailArea(OutputAddsubData)
                Next
            End If
            '〇小計（支払先計）出力
            Me.EditPaymentTotalArea()
            '〇小計（支店計）出力
            Me.EditBranchTotalArea()
            '〇総合計出力
            Me.EditAllTotalArea()

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
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:N5")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim WkYMDateYearStr As String = Left(pOutputRowData("KEIJOYM").ToString, 4)
            Dim WkYMDateMonthStr As String = Right(pOutputRowData("KEIJOYM").ToString, 2)
            Dim WkTargetDateStr As String = WkYMDateYearStr + "年" + WkYMDateMonthStr + "月分"

            Me.PrintPageRowCnt = 1
            '〇タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = REPORT_NAME
            '◯対象日付（YYYY年MM月分（1日～31日））
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr
            '〇処理日
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMDATE")
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMTIME")
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum

            '出力件数加算
            Me.AddPrintRowCnt(1)
            '〇支店名
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("JOTDEPBRANCHNM")

            '出力件数加算
            Me.AddPrintRowCnt(1)
            '〇支払先名
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value =
                pOutputRowData("TORINAME").ToString() + "　" + pOutputRowData("TORIDIVNAME").ToString()

            '出力件数加算
            Me.AddPrintRowCnt(3)

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
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:N8")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '発駅
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("DEPSTATIONNM").ToString.Equals(pOutputRowData("DEPSTATIONNM").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPSTATIONNM")
        End If
        '着駅
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("DEPSTATIONNM").ToString.Equals(pOutputRowData("DEPSTATIONNM").ToString) OrElse
               Not pOldRowData("ARRSTATIONNM").ToString.Equals(pOutputRowData("ARRSTATIONNM").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRSTATIONNM")
        End If
        '種別
        If Me.PrintNameOutputFlg OrElse
               Not pOldRowData("DEPSTATIONNM").ToString.Equals(pOutputRowData("DEPSTATIONNM").ToString) OrElse
               Not pOldRowData("ARRSTATIONNM").ToString.Equals(pOutputRowData("ARRSTATIONNM").ToString) OrElse
               Not pOldRowData("BIGCTNNM").ToString.Equals(pOutputRowData("BIGCTNNM").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BIGCTNNM")
        End If
        '発送月日
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHIPYMD")
        'コンテナ番号（記号 - 番号）
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CTNTYPE").ToString + " - " + pOutputRowData("CTNNO").ToString
        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("JRFIXEDFARE")
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("OWNDISCOUNTFEE")
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("FREESENDFEE")
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHIPFEE")
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("COMMISSIONFEE")
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("COSTADJUSTFEE")
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TOTALCOST")
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TOTALCOST")

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計金額加算
        '総合計
        Me.AllTotalRow.CalcAdd(
            CLng(pOutputRowData("JRFIXEDFARE").ToString),
            CLng(pOutputRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOutputRowData("FREESENDFEE").ToString),
            CLng(pOutputRowData("SHIPFEE").ToString),
            CLng(pOutputRowData("COMMISSIONFEE").ToString),
            CLng(pOutputRowData("COSTADJUSTFEE").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString))
        '支店計
        Me.BranchTotalRow.CalcAdd(
            CLng(pOutputRowData("JRFIXEDFARE").ToString),
            CLng(pOutputRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOutputRowData("FREESENDFEE").ToString),
            CLng(pOutputRowData("SHIPFEE").ToString),
            CLng(pOutputRowData("COMMISSIONFEE").ToString),
            CLng(pOutputRowData("COSTADJUSTFEE").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString))
        
        '支払先計
        Me.PaymentTotalRow.CalcAdd(
            CLng(pOutputRowData("JRFIXEDFARE").ToString),
            CLng(pOutputRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOutputRowData("FREESENDFEE").ToString),
            CLng(pOutputRowData("SHIPFEE").ToString),
            CLng(pOutputRowData("COMMISSIONFEE").ToString),
            CLng(pOutputRowData("COSTADJUSTFEE").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString))
        
        '着駅計
        Me.ArrStationTotalRow.CalcAdd(
            CLng(pOutputRowData("JRFIXEDFARE").ToString),
            CLng(pOutputRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOutputRowData("FREESENDFEE").ToString),
            CLng(pOutputRowData("SHIPFEE").ToString),
            CLng(pOutputRowData("COMMISSIONFEE").ToString),
            CLng(pOutputRowData("COSTADJUSTFEE").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString))
        
        '種別計
        Me.BigCtnTotalRow.CalcAdd(
            CLng(pOutputRowData("JRFIXEDFARE").ToString),
            CLng(pOutputRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOutputRowData("FREESENDFEE").ToString),
            CLng(pOutputRowData("SHIPFEE").ToString),
            CLng(pOutputRowData("COMMISSIONFEE").ToString),
            CLng(pOutputRowData("COSTADJUSTFEE").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString),
            CLng(pOutputRowData("TOTALCOST").ToString))
        
    End Sub

    ''' <summary>
    ''' 帳票総合計出力
    ''' </summary>
    Private Sub EditAllTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A22:N23")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.PrescribedFareTotal
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.DiscountTotal
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.ApplicableFareTotal
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.ShippingFeeTotal
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.CommissionTotal
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.AmountOfAdjustmentTotal
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.SubTotal
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.AllTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(2)

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
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A19:N20")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.PrescribedFareTotal
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.DiscountTotal
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.ApplicableFareTotal
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.ShippingFeeTotal
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.CommissionTotal
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.AmountOfAdjustmentTotal
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.SubTotal
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.BranchTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(2)

        '合計（支店計）クリア
        Me.BranchTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票小計（支払先計）出力
    ''' </summary>
    Private Sub EditPaymentTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A16:N17")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.PrescribedFareTotal
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.DiscountTotal
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.ApplicableFareTotal
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.ShippingFeeTotal
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.CommissionTotal
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.AmountOfAdjustmentTotal
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.SubTotal
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.PaymentTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(2)

        '合計（支払先計）クリア
        Me.PaymentTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票小計（着駅計）出力
    ''' </summary>
    Private Sub EditArrStationTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A13:N14")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.PrescribedFareTotal
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.DiscountTotal
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.ApplicableFareTotal
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.ShippingFeeTotal
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.CommissionTotal
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.AmountOfAdjustmentTotal
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.SubTotal
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.ArrStationTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(2)

        '小計（着駅計）クリア
        Me.ArrStationTotalRow.Clear()

    End Sub

    ''' <summary>
    ''' 帳票小計（種別計）出力
    ''' </summary>
    Private Sub EditBigCtnTotalArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:N11")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '所定運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.PrescribedFareTotal
        '割引
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.DiscountTotal
        '適用運賃
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.ApplicableFareTotal
        '発送料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.ShippingFeeTotal
        '手数料
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.CommissionTotal
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.AmountOfAdjustmentTotal
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.SubTotal
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Me.BigCtnTotalRow.Total

        '出力件数加算
        Me.AddPrintRowCnt(2)

        '小計（種別計）クリア
        Me.BigCtnTotalRow.Clear()

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
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A25:N25")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '種別
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "金額追加"
        '費用加減額
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = CLng(pOldRowData("COSTADJUSTFEE").ToString)
        '小計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = CLng(pOldRowData("COSTADJUSTFEE").ToString)
        '合計
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = CLng(pOldRowData("COSTADJUSTFEE").ToString)

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計金額加算
        '総合計
        Me.AllTotalRow.CalcAdd(
            CLng(pOldRowData("JRFIXEDFARE").ToString),
            CLng(pOldRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOldRowData("FREESENDFEE").ToString),
            CLng(pOldRowData("SHIPFEE").ToString),
            CLng(pOldRowData("COMMISSIONFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString))
        '支店計
        Me.BranchTotalRow.CalcAdd(
            CLng(pOldRowData("JRFIXEDFARE").ToString),
            CLng(pOldRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOldRowData("FREESENDFEE").ToString),
            CLng(pOldRowData("SHIPFEE").ToString),
            CLng(pOldRowData("COMMISSIONFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString))

        '支払先計
        Me.PaymentTotalRow.CalcAdd(
            CLng(pOldRowData("JRFIXEDFARE").ToString),
            CLng(pOldRowData("OWNDISCOUNTFEE").ToString),
            CLng(pOldRowData("FREESENDFEE").ToString),
            CLng(pOldRowData("SHIPFEE").ToString),
            CLng(pOldRowData("COMMISSIONFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString),
            CLng(pOldRowData("COSTADJUSTFEE").ToString))

    End Sub

End Class
