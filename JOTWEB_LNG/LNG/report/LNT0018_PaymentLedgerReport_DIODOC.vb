Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 支払台帳作成クラス
''' </summary>
Public Class LNT0018_PaymentLedgerReport_DIODOC

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
    Private PrintReportFileName As String = ""                              'ファイル名
    Private TORITotal As Integer = 0                                        '支払先総件数
    Private JrfixedfareTotal As Long = 0                                    '所定運賃合計
    Private OwndisCountFeeTotal As Long = 0                                 '私有割引合計
    Private ShipFeeTotal As Long = 0                                        '発送料合計
    Private OtherFeeTotal As Long = 0                                       'その他料金合計
    Private FreesendFeeTotal As Long = 0                                    '回送運賃合計
    Private PayaddsubTotal As Long = 0                                      '加減額合計
    Private PayTotal As Long = 0                                            '支払額合計
    Private TaxFeeTotal As Long = 0                                         '消費税合計
    Private PayGrandTotal As Long = 0                                       '全支払先支払総合計
    Private PaymentQTYTotal As Long = 0                                     '回送個数合計
    Private FirstFLG As String = "0"                                        '初回フラグ
    Private ExistFLG As String = "0"                                        '支払予定日マスタ存在フラグ
    Private Schedatepayment As Date                                         '支払予定日

    Private Const REPORT_ID As String = "LNT0018"                           '帳票ID
    Private Const REPORT_NAME As String = "支払台帳"                        '帳票名
    Private Const REPORT_NAME_RENTAL As String =
                                    "【　回　送　費　支　払　台　帳　】"    '帳票タイトル
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 34                    '改頁行

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
    Public Function CreateExcelPrintData(maxPage As Hashtable, prmExitFlg As String, prmSchedatepayment As Date) As String
        Dim TmpFileName As String = Me.PrintReportFileName & "支払台帳_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte
        Dim PAYMENTORGCODE As String = ""

        ExistFLG = prmExitFlg
        Schedatepayment = prmSchedatepayment

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows

                '支払提出部店変更判定
                If Not PAYMENTORGCODE.Equals(OutputRowData("PAYMENTORGCODE").ToString) Then
                    PAYMENTORGCODE = OutputRowData("PAYMENTORGCODE").ToString
                    Me.PrintaddsheetFlg = True
                    If FirstFLG = "1" Then
                        '合計行出力
                        Me.EditDetailTotalAreaFormat()
                        Me.EditDtailTotalArea()
                    End If
                End If

                '行数による改頁判定
                If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                    Me.PrintPageBreakFlg = True
                End If

                'シート追加
                If Me.PrintaddsheetFlg Then
                    TrySetExcelWorkSheet(PrintOutputRowIdx, OutputRowData("NAMES").ToString, PrintPageNum, "支払台帳")
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
                    Dim MAX_PAGE As String = CStr(maxPage(OutputRowData("PAYMENTORGCODE")))
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
            Me.EditDetailTotalAreaFormat()
            Me.EditDtailTotalArea()

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

        Try
            '初回ページは設定しない
            If Not Me.PrintaddsheetFlg And FirstFLG = "1" Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("B{0}:M{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B1:M4")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim WkYMDateYearStr As String = Left(pOutputRowData("YMDATE").ToString, 4)
            Dim WkYMDateMonthStr As String = Right(pOutputRowData("YMDATE").ToString, 2)
            Dim WkTargetDateStr As String = WkYMDateYearStr + "年" + WkYMDateMonthStr + "月度"

            Me.PrintPageRowCnt = 1
            '◯対象日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr
            '〇帳票タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = REPORT_NAME_RENTAL
            '〇処理日
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMDATE")
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SYSTEMTIME")
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum.ToString + "/" + maxPage

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '〇支払提出部店
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value =
                pOutputRowData("NAMES").ToString() + "(" + pOutputRowData("PAYMENTORGCODE").ToString() + ")"

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
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B5:M34")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

    End Sub

    ''' <summary>
    ''' 合計枠出力
    ''' </summary>
    Private Sub EditDetailTotalAreaFormat()

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B40:M41")
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
        Dim Totaltax As Integer = 0

        Try
            '当該年月セル編集
            Dim WkKEIJOYStr As String = Left(pOutputRowData("YMDATE").ToString, 4)
            Dim WkKEIJOMStr As String = Right(pOutputRowData("YMDATE").ToString, 2)
            Dim WkKEIJOYMStr As String = WkKEIJOYStr + "年" + WkKEIJOMStr + "月分"

            'その他合計
            Dim OtherTotal As Long =
                CInt(pOutputRowData("OTHER1FEE").ToString) + CInt(pOutputRowData("OTHER2FEE").ToString)

            '回送運賃
            Dim forwardPay As Long =
                CInt(pOutputRowData("FREESENDFEE").ToString) - CInt(pOutputRowData("OTHER1FEE").ToString)

            '支払額
            Dim Total As Long =
                forwardPay + CInt(pOutputRowData("SHIPFEE").ToString) + CInt(pOutputRowData("PAYADDSUB").ToString) + CInt(pOutputRowData("OTHER1FEE").ToString)

            '消費税
            If pOutputRowData("TAXCALCUNIT").ToString = "1" Then
                Totaltax = taxMeisai(pOutputRowData("TORICODE").ToString, pOutputRowData("YMDATE").ToString,
                                     pOutputRowData("PAYMENTORGCODE").ToString, pOutputRowData("SCHEDATEPAYMENT").ToString,
                                     pOutputRowData("CLOSINGDATE").ToString, pOutputRowData("DEPOSITMONTHKBN").ToString,
                                     CDec(pOutputRowData("ZERIT"))) + CInt(pOutputRowData("PAYADDSUBTAX").ToString)
            Else
                Totaltax = CInt(Math.Ceiling(Total * CDec(pOutputRowData("ZERIT").ToString)))
            End If

            '支払額計
            Dim TaxTotal As Long =
                Total + Totaltax

            '取引先名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORINAME")
            '取引先コード
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORICODE")
            '所定運賃
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("JRFIXEDFARE")
            '発送料
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHIPFEE")
            '回送運賃
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = forwardPay
            '支払額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Total
            '総支払額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = TaxTotal
            '銀行名/支店名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("PAYBANKNAME").ToString + " / " + pOutputRowData("PAYBANKBRANCHNAME").ToString
            '名義人
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("PAYACCOUNTNAME")

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '取引先部門名
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TORIDIVNAME")
            '私有割引
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = CInt(pOutputRowData("OWNDISCOUNTFEE")) * -1
            'その他料金
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = OtherTotal
            '加減額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = CInt(pOutputRowData("PAYADDSUB").ToString)
            '消費税
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = Totaltax
            '回送個数
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("PAYMENTQTY")
            '支払予定日
            If ExistFLG = "1" Then
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = Schedatepayment
            Else
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SCHEDATEPAYMENT")
            End If
            '種別/口座番号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE1").ToString + "　 " + pOutputRowData("PAYACCOUNT").ToString

            '出力件数加算
            Me.AddPrintRowCnt(1)

            TORITotal += 1                                                  '支払先総件数加算
            JrfixedfareTotal += CLng(pOutputRowData("JRFIXEDFARE"))         '所定運賃合計加算
            OwndisCountFeeTotal += CLng(pOutputRowData("OWNDISCOUNTFEE"))   '私有割引合計加算
            ShipFeeTotal += CLng(pOutputRowData("SHIPFEE"))                 '発送料合計加算
            OtherFeeTotal += OtherTotal                                     'その他料金合計加算
            FreesendFeeTotal += forwardPay                                  '回送運賃合計加算
            PayaddsubTotal += CLng(pOutputRowData("PAYADDSUB").ToString)    '加減額合計加算
            PayTotal += Total                                               '支払額合計加算
            TaxFeeTotal += Totaltax                                         '消費税合計加算
            PayGrandTotal += TaxTotal                                       '全支払先支払総合計加算
            PaymentQTYTotal += CLng(pOutputRowData("PAYMENTQTY"))           '回送個数合計加算

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票合計出力
    ''' </summary>
    Private Sub EditDtailTotalArea()

        Try

            '支店計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = "支店計"
            '支払先
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "支払先"
            '所定運賃計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = JrfixedfareTotal
            '発送料計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = ShipFeeTotal
            '回送運賃計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = FreesendFeeTotal
            '支払額計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = PayTotal
            '総支払額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = PayGrandTotal

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '私有割引
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = OwndisCountFeeTotal * -1
            '支払先件数
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = TORITotal.ToString + "件"
            'その他料金
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = OtherFeeTotal
            '加減額
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = PayaddsubTotal
            '消費税
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = TaxFeeTotal
            '回送個数計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = PaymentQTYTotal

            JrfixedfareTotal = 0
            ShipFeeTotal = 0
            FreesendFeeTotal = 0
            PayTotal = 0
            PaymentQTYTotal = 0
            OwndisCountFeeTotal = 0
            TORITotal = 0
            OtherFeeTotal = 0
            PayaddsubTotal = 0
            TaxFeeTotal = 0
            PayGrandTotal = 0

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 税計算区分が明細時消費税取得処理
    ''' </summary>
    ''' <returns>SQL実行結果</returns>
    ''' <remarks></remarks>
    Private Function taxMeisai(ByRef TORICODE As String, ByRef KEIJOYM As String,
                               ByRef PAYFILINGBRANCH As String, ByRef SCHEDATEPAYMENT As String,
                               ByRef CLOSINGDAY As String, ByRef DEPOSITMONTHKBN As String,
                               ByRef ZERIT As Double) As Integer

        Dim meisaiTaxFee As Integer = 0
        Dim meisaiTax As Integer = 0
        Dim dblMeisaiTax As Double = 0
        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT DISTINCT")
            SQLBldr.AppendLine("    SORTNO")
            SQLBldr.AppendLine("    , KEIJOYM")
            SQLBldr.AppendLine("    , TORICODE")
            SQLBldr.AppendLine("    , PAYFILINGBRANCH")
            SQLBldr.AppendLine("    , SCHEDATEPAYMENT")
            SQLBldr.AppendLine("    , DEPOSITMONTHKBN")
            SQLBldr.AppendLine("    , CLOSINGDAY")
            SQLBldr.AppendLine("	, KEIJOBRANCH")
            SQLBldr.AppendLine("	, KEIJOBRANCHNAME")
            SQLBldr.AppendLine("    , DEPSTATIONCODE")
            SQLBldr.AppendLine("    , DEPSTATIONNAME")
            SQLBldr.AppendLine("    , ARRSTATIONCODE")
            SQLBldr.AppendLine("    , ARRSTATIONNAME")
            SQLBldr.AppendLine("    , BIGCTNCODE")
            SQLBldr.AppendLine("    , BIGCTNNAME")
            SQLBldr.AppendLine("    , CTN_COUNT")
            SQLBldr.AppendLine("    , SUM_FREESENDFEE")
            SQLBldr.AppendLine("    , SUM_OTHERFEE")
            SQLBldr.AppendLine("    , SUM_SHIPFEE")
            SQLBldr.AppendLine("    , SUM_USEFEE")
            SQLBldr.AppendLine("    , TAXCALCUNIT")
            SQLBldr.AppendLine("    , UPDATEFLG")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("    ( ")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        D01.SORTNO")
            SQLBldr.AppendLine("        , D01.KEIJOYM")
            SQLBldr.AppendLine("        , D01.TORICODE")
            SQLBldr.AppendLine("        , D01.PAYFILINGBRANCH")
            SQLBldr.AppendLine("        , D01.SCHEDATEPAYMENT")
            SQLBldr.AppendLine("        , D01.DEPOSITMONTHKBN")
            SQLBldr.AppendLine("        , D01.CLOSINGDAY")
            SQLBldr.AppendLine("		, D01.KEIJOBRANCH")
            SQLBldr.AppendLine("    	, D05.NAME AS KEIJOBRANCHNAME")
            SQLBldr.AppendLine("        , D01.DEPSTATION AS DEPSTATIONCODE")
            SQLBldr.AppendLine("        , D02.NAMES AS DEPSTATIONNAME")
            SQLBldr.AppendLine("        , D01.ARRSTATION AS ARRSTATIONCODE")
            SQLBldr.AppendLine("        , D03.NAMES AS ARRSTATIONNAME")
            SQLBldr.AppendLine("        , D01.BIGCTNCD AS BIGCTNCODE")
            SQLBldr.AppendLine("        , TRIM(REPLACE(D04.KANJI1,'　','')) AS BIGCTNNAME")
            SQLBldr.AppendLine("        , D01.CTN_COUNT AS CTN_COUNT")
            SQLBldr.AppendLine("        , D01.SUM_FREESENDFEE")
            SQLBldr.AppendLine("        , D01.SUM_OTHER1FEE + D01.SUM_OTHER2FEE AS SUM_OTHERFEE")
            SQLBldr.AppendLine("        , D01.SUM_SHIPFEE")
            SQLBldr.AppendLine("        , D01.SUM_FREESENDFEE + D01.SUM_OTHER1FEE + D01.SUM_OTHER2FEE + D01.SUM_SHIPFEE + D01.SUM_PAYADDSUB AS SUM_USEFEE")
            SQLBldr.AppendLine("        , D01.TAXCALCUNIT")
            SQLBldr.AppendLine("        , D01.SUM_FREESENDFEETAX")
            SQLBldr.AppendLine("        , D01.SUM_OTHER1FEETAX + D01.SUM_OTHER2FEETAX AS SUM_OTHERFEETAX")
            SQLBldr.AppendLine("        , D01.SUM_SHIPFEETAX")
            SQLBldr.AppendLine("        , D01.SUM_PAYADDTAX")
            SQLBldr.AppendLine("		, D01.UPDATEFLG")
            SQLBldr.AppendLine("    FROM")
            SQLBldr.AppendLine("        ( ")
            SQLBldr.AppendLine("            SELECT")
            SQLBldr.AppendLine("                '1' AS SORTNO")
            SQLBldr.AppendLine("                , A01.KEIJOYM")
            SQLBldr.AppendLine("                , A01.TORICODE")
            SQLBldr.AppendLine("                , A01.PAYFILINGBRANCH")
            SQLBldr.AppendLine("                , A01.SCHEDATEPAYMENT")
            SQLBldr.AppendLine("                , A01.DEPOSITMONTHKBN")
            SQLBldr.AppendLine("                , FORMAT(A01.CLOSINGDATE, 'dd') AS CLOSINGDAY")
            SQLBldr.AppendLine("				, A01.PAYKEIJYOBRANCHCD AS KEIJOBRANCH")
            SQLBldr.AppendLine("                , A01.JURISDICTIONCD")
            SQLBldr.AppendLine("                , STR(A01.DEPSTATION) AS DEPSTATION")
            SQLBldr.AppendLine("                , STR(A01.ARRSTATION) AS ARRSTATION")
            SQLBldr.AppendLine("                , A01.BIGCTNCD AS BIGCTNCD")
            SQLBldr.AppendLine("				, COUNT(A01.TORICODE) AS CTN_COUNT")
            SQLBldr.AppendLine("                , SUM(coalesce(A01.FREESENDFEE,0) - coalesce(A01.OTHER1FEE,0) + coalesce(A01.COMMISSIONFEE, 0) + coalesce(A01.COSTADJUSTFEE, 0)) AS SUM_FREESENDFEE")
            SQLBldr.AppendLine("                , SUM(coalesce(A01.OTHER1FEE,0)) AS SUM_OTHER1FEE")
            SQLBldr.AppendLine("                , SUM(coalesce(A01.OTHER2FEE,0)) AS SUM_OTHER2FEE")
            SQLBldr.AppendLine("                , SUM(coalesce(A01.SHIPFEE,0)) AS SUM_SHIPFEE")
            SQLBldr.AppendLine("                , 0 AS SUM_PAYADDSUB")
            SQLBldr.AppendLine("                , MAX(A01.TAXCALCUNIT) AS TAXCALCUNIT")
            SQLBldr.AppendLine("                , SUM(A01.FREESENDFEETAX) AS SUM_FREESENDFEETAX")
            SQLBldr.AppendLine("                , SUM(A01.OTHER1FEETAX) AS SUM_OTHER1FEETAX")
            SQLBldr.AppendLine("                , SUM(A01.OTHER2FEETAX) AS SUM_OTHER2FEETAX")
            SQLBldr.AppendLine("                , SUM(A01.SHIPFEETAX) AS SUM_SHIPFEETAX")
            SQLBldr.AppendLine("                , 0 AS SUM_PAYADDTAX")
            SQLBldr.AppendLine("				, NULL as UPDATEFLG")
            SQLBldr.AppendLine("		, MAX(A01.STACKFREEKBN) AS STACKFREEKBN")
            SQLBldr.AppendLine("		, MAX(A01.ACCOUNTINGASSETSKBN) AS ACCOUNTINGASSETSKBN")
            SQLBldr.AppendLine("            FROM")
            SQLBldr.AppendLine("                (SELECT")
            SQLBldr.AppendLine("                      *")
            SQLBldr.AppendLine("                      ,CEILING((coalesce(A02.FREESENDFEE, 0) - coalesce(A02.OTHER1FEE, 0) + coalesce(A02.COMMISSIONFEE, 0) + coalesce(A02.COSTADJUSTFEE, 0)) * @P03) AS FREESENDFEETAX")
            SQLBldr.AppendLine("                      ,CEILING(coalesce(A02.OTHER1FEE, 0) * @P03) AS OTHER1FEETAX")
            SQLBldr.AppendLine("                      ,CEILING(coalesce(A02.OTHER2FEE, 0) * @P03) AS OTHER2FEETAX")
            SQLBldr.AppendLine("                      ,CEILING(coalesce(A02.SHIPFEE, 0) * @P03) AS SHIPFEETAX")
            SQLBldr.AppendLine("                 FROM lng.LNT0017_RESSNF A02")
            SQLBldr.AppendLine("                 WHERE")
            SQLBldr.AppendLine("                     A02.DELFLG = @P01")
            SQLBldr.AppendLine("                     AND A02.STACKFREEKBN = '2'")
            SQLBldr.AppendLine("                     AND A02.ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
            SQLBldr.AppendLine("                     AND A02.ACCOUNTINGASSETSKBN = '1'")
            SQLBldr.AppendLine("                ) A01")
            SQLBldr.AppendLine("            GROUP BY")
            SQLBldr.AppendLine("                A01.KEIJOYM")
            SQLBldr.AppendLine("                , A01.TORICODE")
            SQLBldr.AppendLine("                , A01.PAYFILINGBRANCH")
            SQLBldr.AppendLine("                , A01.SCHEDATEPAYMENT")
            SQLBldr.AppendLine("                , A01.DEPOSITMONTHKBN")
            SQLBldr.AppendLine("                , FORMAT(A01.CLOSINGDATE, 'dd')")
            SQLBldr.AppendLine("                , A01.JURISDICTIONCD")
            SQLBldr.AppendLine("                , A01.DEPSTATION")
            SQLBldr.AppendLine("                , A01.ARRSTATION")
            SQLBldr.AppendLine("                , A01.BIGCTNCD")
            SQLBldr.AppendLine("				, A01.PAYKEIJYOBRANCHCD")
            SQLBldr.AppendLine("        ) D01")
            SQLBldr.AppendLine("        LEFT JOIN com.LNS0020_STATION D02")
            SQLBldr.AppendLine("            ON D02.CAMPCODE = @P02")
            SQLBldr.AppendLine("            AND STR(D02.STATION) = D01.DEPSTATION")
            SQLBldr.AppendLine("        LEFT JOIN com.LNS0020_STATION D03")
            SQLBldr.AppendLine("            ON D03.CAMPCODE = @P02")
            SQLBldr.AppendLine("            AND STR(D03.STATION) = D01.ARRSTATION")
            SQLBldr.AppendLine("        LEFT JOIN lng.LNM0022_CLASS D04")
            SQLBldr.AppendLine("            ON D04.BIGCTNCD = TRIM(D01.BIGCTNCD)")
            SQLBldr.AppendLine("            AND D04.DELFLG = @P01")
            SQLBldr.AppendLine("        LEFT JOIN com.LNS0014_ORG D05")
            SQLBldr.AppendLine("            ON D05.ORGCODE = D01.KEIJOBRANCH")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        D01.TORICODE = '" & TORICODE & "'")
            SQLBldr.AppendLine("    AND D01.KEIJOYM = '" & KEIJOYM & "'")
            SQLBldr.AppendLine("    AND D01.PAYFILINGBRANCH = '" & PAYFILINGBRANCH & "'")
            SQLBldr.AppendLine("    AND D01.SCHEDATEPAYMENT = '" & SCHEDATEPAYMENT & "'")
            SQLBldr.AppendLine("    AND D01.DEPOSITMONTHKBN = '" & DEPOSITMONTHKBN & "'")
            SQLBldr.AppendLine("    AND D01.CLOSINGDAY = '" & CLOSINGDAY & "'")
            SQLBldr.AppendLine("    AND D01.JURISDICTIONCD = '14'")
            SQLBldr.AppendLine(") MAIN")
            SQLBldr.AppendLine("ORDER BY")
            SQLBldr.AppendLine("    SORTNO")
            SQLBldr.AppendLine("	, DEPSTATIONCODE")
            SQLBldr.AppendLine("	, ARRSTATIONCODE")

            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.Decimal)     '税率

                    PARA01.Value = C_DELETE_FLG.ALIVE
                    PARA02.Value = "01"
                    PARA03.Value = ZERIT.ToString

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

                '明細の値セット
                For Each rowData As DataRow In dt.Rows

                    dblMeisaiTax = CDec(CInt(rowData("SUM_USEFEE")) / CInt(rowData("CTN_COUNT")) * ZERIT)
                    meisaiTax = CInt(Math.Ceiling(dblMeisaiTax))
                    meisaiTax = meisaiTax * CInt(rowData("CTN_COUNT"))

                    meisaiTaxFee += meisaiTax

                Next

            Catch ex As Exception
            End Try

        End Using

        Return meisaiTaxFee

    End Function
End Class
