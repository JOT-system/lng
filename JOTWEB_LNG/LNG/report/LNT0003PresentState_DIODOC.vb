Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 現況表作成クラス
''' </summary>
Public Class LNT0003PresentState_DIODOC

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
    Private PrintOutputRowIdx As Int32 = 0                                  '出力位置（行）　※初期値：1
    Private PrintPageRowCnt As Int32 = 1                                    'ページ内出力件数　※初期値：1
    Private PrintNameOutputFlg As String = "1"                              '名称出力フラグ　※初期値：1
    Private PrintPageOutputFlg As Boolean = True                            '改頁時状態区分行保存フラグ　※初期値：True
    Private PrintPageBreakFlg As Boolean = True                             '改頁フラグ　※初期値：True
    Private PrintaddsheetFlg As Boolean = True                              'シート追加フラグ　※初期値：True
    Private PrintCopyRowFlg As Boolean = False                              '行コピーフラグ　※初期値：False
    Private PrintTrainNowFlg As Boolean = True                              '列車現在フラグ　※初期値：True
    Private PrintOutputcount As String = "1"                                '1行に表示するコンテナを制御する
    Private PrintSituationLineFLG As String = "0"                           '状態区分罫線フラグ

    Private PrintTotalBerthcount As Int32 = 0                               '停泊計カウント
    Private PrintTotalStationcount As Int32 = 0                             '駅計カウント
    Private PrintTotalBanchcount As Int32 = 0                               '支店計カウント
    Private PrintTotalKindscount As Int32 = 0                               '種別計カウント
    Private PrintTotalStatuscount As Int32 = 0                              '状態計カウント
    Private PrintTotalStatusRow As Int32 = 0                                '状態計出力位置保存

    Private Const REPORT_ID As String = "LNT0003"                           '帳票ID
    Private Const REPORT_NAME As String = "コンテナ現況表"                  '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 50                    '改頁行（合計明細行は含めない）

    '合計金額クラス
    Private Class LNT0003_TotalDataClass
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
        Dim BIGCTNCODE As String = ""
        Dim ORGCODE As String = ""
        Dim ARRSTACODE As String = ""
        Dim STATUSKBNNO As String = ""
        Dim ARRTRUSTEECD As String = ""

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows

                '初回は判定しない
                If OldRowData IsNot Nothing Then

                    '着受託人変更判定
                    If Not ARRTRUSTEECD.Equals(OutputRowData("ARRTRUSTEECD").ToString) Then
                        PrintOutputcount = "1"
                        Me.PrintNameOutputFlg = "3"
                    End If
                    '状態区分変更判定
                    If Not STATUSKBNNO.Equals(OutputRowData("STATUSKBNNO").ToString) Then
                        PrintOutputcount = "1"
                        Me.PrintNameOutputFlg = "2"
                        Me.PrintSituationLineFLG = "1"
                        Me.TotalStutsArea()
                        '次回が列車現在の場合停泊計表示
                        If OutputRowData("STATUSKBNNO").ToString = "7" Then
                            TotalBerthArea()
                            PrintTrainNowFlg = False
                        End If
                    End If
                    '現在駅変更判定
                    If Not ARRSTACODE.Equals(OutputRowData("ARRSTACODE").ToString) Then
                        PrintOutputcount = "1"
                        Me.PrintNameOutputFlg = "1"
                        Me.TotalStutsArea()
                        '状態区分とセットで表示する
                        If PrintTrainNowFlg Then
                            TotalBerthArea()
                        End If
                        TotalStationArea()
                        PrintTrainNowFlg = True
                    End If
                    '支店変更判定
                    If Not ORGCODE.Equals(OutputRowData("ORGCODE").ToString) Then
                        PrintOutputcount = "1"
                        Me.PrintNameOutputFlg = "1"
                        TotalBanchArea()
                        Me.PrintPageBreakFlg = True
                        Me.TotalStutsArea()
                    End If
                    '種別変更判定
                    If Not BIGCTNCODE.Equals(OutputRowData("BIGCTNCD").ToString) Then
                        PrintOutputcount = "1"
                        Me.PrintNameOutputFlg = "1"
                        Me.PrintaddsheetFlg = True
                        '種別が変わっても駅が変わらない場合
                        If ARRSTACODE.Equals(OutputRowData("ARRSTACODE").ToString) Then
                            TotalStationArea()
                        End If
                        '種別が変わっても支店が変わらない場合
                        If ORGCODE.Equals(OutputRowData("ORGCODE").ToString) Then
                            TotalBanchArea()
                        End If
                        TotalKindsArea()
                        TotalStutsArea()
                        If PrintOutputRowIdx < 23 Then
                            tempRowDel()
                        End If
                    End If
                End If

                '改行判定
                If PrintOutputcount = "1" OrElse
                   Not BIGCTNCODE.Equals(OutputRowData("BIGCTNCD").ToString) OrElse
                   Not ORGCODE.Equals(OutputRowData("ORGCODE").ToString) OrElse
                   Not ARRSTACODE.Equals(OutputRowData("ARRSTACODE").ToString) OrElse
                   Not STATUSKBNNO.Equals(OutputRowData("STATUSKBNNO").ToString) OrElse
                   Not ARRTRUSTEECD.Equals(OutputRowData("ARRTRUSTEECD").ToString) Then
                    Me.AddPrintRowCnt(1)
                    PrintCopyRowFlg = True
                End If

                If OldRowData IsNot Nothing Then
                    '行数による改頁判定
                    If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                        Me.PrintPageBreakFlg = True
                    End If

                End If

                ''シート追加
                If Me.PrintaddsheetFlg Then
                    TrySetExcelWorkSheet(PrintOutputRowIdx, OutputRowData("BIGCTNNAME").ToString, PrintPageNum, "コンテナ現況表")
                    'シートが切り替わり、ページ数リセット
                    Me.PrintPageNum = 1
                    Me.PrintaddsheetFlg = False
                    Me.PrintPageBreakFlg = True
                    OldRowData = Nothing
                End If

                'シート追加・改頁の場合、ヘッダ出力（初回出力も含む）
                If Me.PrintPageBreakFlg Then
                    '〇ヘッダー出力
                    Me.EditHeaderArea(OldRowData, OutputRowData)
                    PrintNameOutputFlg = "1"
                    Me.PrintPageBreakFlg = False
                End If

                '〇明細出力
                Me.EditDetailArea(OldRowData, OutputRowData)

                '前回出力明細データ保持
                OldRowData = OutputRowData
                BIGCTNCODE = OutputRowData("BIGCTNCD").ToString
                ORGCODE = OutputRowData("ORGCODE").ToString
                ARRSTACODE = OutputRowData("ARRSTACODE").ToString
                STATUSKBNNO = OutputRowData("STATUSKBNNO").ToString
                ARRTRUSTEECD = OutputRowData("ARRTRUSTEECD").ToString

            Next

            '〇状態区分計出力
            Me.TotalStutsArea()
            '〇停泊計出力
            Me.TotalBerthArea()
            '〇駅計出力
            Me.TotalStationArea()
            '〇支店計出力
            Me.TotalBanchArea()
            '〇種別計出力
            Me.TotalKindsArea()

            '23行以下の場合テンプレート行削除
            If PrintOutputRowIdx < 23 Then
                tempRowDel()
            End If

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
        ByVal pOutputRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '初回ページは設定しない
            If pOldRowData IsNot Nothing Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:AB{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:AB4")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim WkYDateYearStr As String = Format(Date.Now, "yyyy")
            Dim WkMDateMonthStr As String = Format(Date.Now, "MM")
            Dim WkDateStr As String = Format(Date.Now, "dd")
            Dim WkTargetDateStr As String = WkYDateYearStr + "年" + WkMDateMonthStr + "月" + WkDateStr + "日" + "現在"
            Dim WkNowDateYearStr As String = Format(Date.Now, "yyyy.MM.dd")

            Me.PrintPageRowCnt = 1
            '〇コンテナ種別
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BIGCTNNAME")
            '◯対象日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr
            '〇タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = "コ　ン　テ　ナ　現　況　表"
            '〇会社名
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("U" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CAMPNAME")
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Z" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("AA" + Me.PrintOutputRowIdx.ToString()).Value = WkNowDateYearStr

            '出力件数加算
            Me.AddPrintRowCnt(1)
            '〇支店名
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("V" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ORGNAME")

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
        Dim addItem As String = ""
        Dim addItem2 As String = ""
        Dim addItem10 As String = ""
        Dim memo As String = ""
        Dim ARRTRUSTEENM As String = ""

        '明細行コピー
        If PrintCopyRowFlg Then
            If Me.PrintPageRowCnt >= 50 Then
                '頁内最後の行の場合
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:AB7")
            Else
                '状態変化時罫線を引く
                If PrintSituationLineFLG = "1" Then
                    If pOutputRowData("STATUSKBNNO").ToString = "7" Then
                        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A22:AB22")
                        PrintSituationLineFLG = "0"
                    Else
                        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A19:AB19")
                        PrintSituationLineFLG = "0"
                    End If
                Else
                        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A6:AB6")
                End If
            End If
                destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)
            PrintCopyRowFlg = False
        End If

        addItem2 = pOutputRowData("ADDITEM2").ToString.Replace("　", "")
        addItem10 = pOutputRowData("ADDITEM10").ToString.Replace("　", "")

        '付帯項目
        If addItem2 = "" Then
            addItem = addItem10
        ElseIf addItem10 = "" Then
            addItem = addItem2
        End If

        '現在駅
        If Me.PrintNameOutputFlg = "1" OrElse
           Not pOldRowData("ARRSTACODE").ToString.Equals(pOutputRowData("ARRSTACODE").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRSTANAME")
        End If
        '状態区分
        If Me.PrintNameOutputFlg = "1" OrElse
           Me.PrintNameOutputFlg = "2" OrElse
           Not pOldRowData("STATUSKBNNO").ToString.Equals(pOutputRowData("STATUSKBNNO").ToString) Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("STATUSKBN")
            If PrintPageOutputFlg Then
                '状態区分出力行保存
                PrintTotalStatusRow = PrintOutputRowIdx
                PrintPageOutputFlg = False
            End If
        End If
        '着受託人
        If Me.PrintNameOutputFlg = "1" OrElse
           Me.PrintNameOutputFlg = "2" OrElse
           Me.PrintNameOutputFlg = "3" OrElse
           Not pOldRowData("ARRTRUSTEECD").ToString.Equals(pOutputRowData("ARRTRUSTEECD").ToString) Then
            If pOutputRowData("ARRTRUSTEENM").ToString = DBNull.Value.ToString Then
                ARRTRUSTEENM = "？？？？？？？？？？？？？？"
            Else
                ARRTRUSTEENM = pOutputRowData("ARRTRUSTEENM").ToString
            End If
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = ARRTRUSTEENM
        End If

        Me.PrintNameOutputFlg = "0"

        If Not pOutputRowData("MEMO").ToString = DBNull.Value.ToString Then
            ''状態は5文字まで表示可
            'If pOutputRowData("MEMO").ToString.Length > 5 Then
            '    memo = pOutputRowData("MEMO").ToString.Substring(0, 5) + "..."
            'Else
            '    memo = pOutputRowData("MEMO").ToString
            'End If
            memo = pOutputRowData("MEMO").ToString
        End If

        '1行のコンテナ個数を3つまでにする
        If PrintOutputcount = "1" Then
            '番号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CTNNO").ToString
            '付帯項目
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = addItem
            '記号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("OPERATIONKBNKG")
            '発駅
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPSTANAME").ToString
            '着日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRIVEDATE")
            '状態
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CONTSTATUS")
            'メモ
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = memo

            PrintOutputcount = "2"

        ElseIf PrintOutputcount = "2" Then
            '番号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CTNNO").ToString
            '付帯項目
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = addItem
            '記号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("OPERATIONKBNKG")
            '発駅
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPSTANAME").ToString
            '着日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRIVEDATE")
            '状態
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("S" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CONTSTATUS")
            'メモ
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("T" + Me.PrintOutputRowIdx.ToString()).Value = memo

            PrintOutputcount = "3"

        ElseIf PrintOutputcount = "3" Then
            '番号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("U" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CTNNO").ToString
            '付帯項目
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("V" + Me.PrintOutputRowIdx.ToString()).Value = addItem
            '記号
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("W" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("OPERATIONKBNKG")
            '発駅
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("X" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("DEPSTANAME").ToString
            '着日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Y" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ARRIVEDATE")
            '状態
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Z" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("CONTSTATUS")
            'メモ
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("AA" + Me.PrintOutputRowIdx.ToString()).Value = memo

            PrintOutputcount = "1"

        End If

        '各合計加算
        '状態区分が列車現在の場合状態区分計を加算しない
        If Not pOutputRowData("STATUSKBNNO").ToString = "7" Then
            PrintTotalBerthcount += 1
        End If
        PrintTotalStationcount += 1
        PrintTotalBanchcount += 1
        PrintTotalKindscount += 1
        PrintTotalStatuscount += 1

    End Sub

    ''' <summary>
    ''' 状態区分計出力
    ''' </summary>
    Private Sub TotalStutsArea()

        If Not PrintTotalStatuscount = 0 Then
            'コンテナ個数
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintTotalStatusRow.ToString()).Value = PrintTotalStatuscount.ToString + "個"
            PrintPageOutputFlg = True
        End If
        '状態区分計リセット
        PrintTotalStatuscount = 0

    End Sub
    ''' <summary>
    ''' 停泊計出力
    ''' </summary>
    Private Sub TotalBerthArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        If PrintTotalBerthcount > 0 Then

            Me.AddPrintRowCnt(1)

            '明細行コピー
            If Me.PrintPageRowCnt >= 50 Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:AB10")
            Else
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A9:AB9")
            End If
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '停泊計出力
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = PrintTotalBerthcount.ToString + "個"

            End If

            '停泊計リセット
            PrintTotalBerthcount = 0

    End Sub

    ''' <summary>
    ''' 駅計出力
    ''' </summary>
    Private Sub TotalStationArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Me.AddPrintRowCnt(1)

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A12:AB12")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '駅計出力
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = PrintTotalStationcount.ToString + "個"

        '駅計リセット
        PrintTotalStationcount = 0
    End Sub

    ''' <summary>
    ''' 支店計出力
    ''' </summary>
    Private Sub TotalBanchArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Me.AddPrintRowCnt(1)

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A13:AB13")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '停泊計出力
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = PrintTotalBanchcount.ToString + "個"

        '停泊計リセット
        PrintTotalBanchcount = 0

    End Sub

    ''' <summary>
    ''' 種別計出力
    ''' </summary>
    Private Sub TotalKindsArea()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Me.AddPrintRowCnt(1)

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A14:AB14")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '停泊計出力
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = PrintTotalKindscount.ToString + "個"

        '停泊計リセット
        PrintTotalKindscount = 0

    End Sub

    ''' <summary>
    ''' テンプレート行削除
    ''' </summary>
    Private Sub tempRowDel()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Me.AddPrintRowCnt(1)

        For PrintOutputRowIdx = PrintOutputRowIdx To 23
            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A25:AB25")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)
        Next

    End Sub
End Class
