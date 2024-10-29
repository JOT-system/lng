Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 輸送個数表作成クラス
''' </summary>
Public Class LNT0017_TsptQuantityReport_DIODOC

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
    Private PrintMaxPageCnt As Int32 = 0                                    'ページ数分母件数　※初期値：0
    Private PrintOutputPageRow As New ArrayList                             'ページ数出力行
    Private PrintPageBreakFlg As Boolean = True                             '改頁フラグ　※初期値：True
    Private PrintNameOutputFlg As Boolean = True                            '名称出力フラグ　※初期値：True（改頁後に名称を再度出力させるフラグ）
    Private PrintBranchOutputFlg As Boolean = True                          '支店出力フラグ　※初期値：True
    Private PrintaddsheetFlg As Boolean = False                             'シート追加フラグ　※初期値：False
    Private PrintFirstRowFlg As Boolean = True                              '初行フラグ　※初期値：True

    Private Const REPORT_ID As String = "LNT0017"                                   '帳票ID
    Private Const REPORT_NAME As String = "コンテナ形式別　輸送キロ程別　輸送個数"  '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 41                            '改頁行

    '合計金額クラス
    Private Class LNT0017_TotalDataClass
        Public Total_01 As Long = 0                   '1～200 / 1601～1700
        Public Total_02 As Long = 0                   '201～300 / 1701～1800
        Public Total_03 As Long = 0                   '301～400 / 1801～1900
        Public Total_04 As Long = 0                   '401～500 / 1901～2000
        Public Total_05 As Long = 0                   '501～600 / 2001～2100
        Public Total_06 As Long = 0                   '601～700 / 2101～2200
        Public Total_07 As Long = 0                   '701～800 / 2201～2300
        Public Total_08 As Long = 0                   '801～900 / 2301～2400
        Public Total_09 As Long = 0                   '901～1000 / 2401～2500
        Public Total_10 As Long = 0                   '1001～1100 / 2501～2600
        Public Total_11 As Long = 0                   '1101～1200 / 2601～2700
        Public Total_12 As Long = 0                   '1201～1300 / 2700～
        Public Total_13 As Long = 0                   '1301～1400 / 個数合計
        Public Total_14 As Long = 0                   '1401～1500 / 平均キロ
        Public Total_15 As Long = 0                   '1501～1600

        '金額クリア処理
        Public Sub Clear()
            Me.Total_01 = 0
            Me.Total_02 = 0
            Me.Total_03 = 0
            Me.Total_04 = 0
            Me.Total_05 = 0
            Me.Total_06 = 0
            Me.Total_07 = 0
            Me.Total_08 = 0
            Me.Total_09 = 0
            Me.Total_10 = 0
            Me.Total_11 = 0
            Me.Total_12 = 0
            Me.Total_13 = 0
            Me.Total_14 = 0
            Me.Total_15 = 0
        End Sub

        '金額加算処理
        Public Sub CalcAdd(DataRowparam As DataRow)
            Me.Total_01 += ExIntParse(DataRowparam("VALUE_1").ToString)
            Me.Total_02 += ExIntParse(DataRowparam("VALUE_2").ToString)
            Me.Total_03 += ExIntParse(DataRowparam("VALUE_3").ToString)
            Me.Total_04 += ExIntParse(DataRowparam("VALUE_4").ToString)
            Me.Total_05 += ExIntParse(DataRowparam("VALUE_5").ToString)
            Me.Total_06 += ExIntParse(DataRowparam("VALUE_6").ToString)
            Me.Total_07 += ExIntParse(DataRowparam("VALUE_7").ToString)
            Me.Total_08 += ExIntParse(DataRowparam("VALUE_8").ToString)
            Me.Total_09 += ExIntParse(DataRowparam("VALUE_9").ToString)
            Me.Total_10 += ExIntParse(DataRowparam("VALUE_10").ToString)
            Me.Total_11 += ExIntParse(DataRowparam("VALUE_11").ToString)
            Me.Total_12 += ExIntParse(DataRowparam("VALUE_12").ToString)
            Me.Total_13 += ExIntParse(DataRowparam("VALUE_13").ToString)
            Me.Total_14 += ExIntParse(DataRowparam("VALUE_14").ToString)
            Me.Total_15 += ExIntParse(DataRowparam("VALUE_15").ToString)
        End Sub
        Public Shared Function ExIntParse(StrVal As String) As Int32
            If Not Int32.TryParse(StrVal, 10) Then
                Return 0
            End If

            Return Int32.Parse(StrVal)

        End Function
    End Class

    '合計金額クラス保持変数
    Private RowTotal As LNT0017_TotalDataClass = New LNT0017_TotalDataClass()

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
    Public Function CreateExcelPrintData(Fromdate As String, Todate As String) As String
        Dim TmpFileName As String = REPORT_NAME & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows
                '小計出力判定
                If OldRowData IsNot Nothing Then
                    '小計
                    If Not OldRowData("JOTARRBRANCHCD").ToString.Equals(OutputRowData("JOTARRBRANCHCD").ToString) Then
                        '〇小計出力
                        Me.EditBigCtnTotalArea(OldRowData)
                        '〇支店名を1行だけ出力
                        Me.PrintBranchOutputFlg = True
                        Me.PrintFirstRowFlg = True
                    End If

                End If

                '行数による改頁判定
                If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                    Me.PrintPageBreakFlg = True
                End If

                '改頁の場合、ヘッダ出力（初回出力も含む）
                If Me.PrintPageBreakFlg Then
                    '〇ヘッダー出力（前半と後半でヘッダを分ける）
                    If OutputRowData("HALFKBN").ToString = "1" Then
                        Me.EditHeaderArea_1(OldRowData, OutputRowData, Fromdate, Todate)
                    ElseIf OutputRowData("HALFKBN").ToString = "2" Then
                        Me.EditHeaderArea_2(OldRowData, OutputRowData, Fromdate, Todate)
                    End If
                    Me.PrintPageBreakFlg = False
                    Me.PrintNameOutputFlg = True
                End If

                '〇明細出力
                Me.EditDetailArea(OldRowData, OutputRowData)
                Me.PrintNameOutputFlg = False
                Me.PrintBranchOutputFlg = False
                Me.PrintFirstRowFlg = False

                '前回出力明細データ保持
                OldRowData = OutputRowData
            Next

            '〇小計出力
            Me.EditBigCtnTotalArea(OldRowData)

            '頁数出力
            EditOutputPage()

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
    ''' 前半帳票ヘッダ出力
    ''' </summary>
    Private Sub EditHeaderArea_1(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow,
        ByVal Fromdate As String,
        ByVal Todate As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダーを作成するたび頁数分母加算
            Me.PrintMaxPageCnt += 1
            '途切れた場合支店を表示する
            Me.PrintBranchOutputFlg = True
            '初回ページは設定しない
            If pOldRowData IsNot Nothing Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:Q{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:Q5")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim Nowdt As DateTime = DateTime.Now
            Dim WkNowDateStr As String = Nowdt.ToString("yyyy.MM.dd")

            '積空区分判定
            Dim StkFreKBN As String = ""
            If pOutputRowData("STACKFREEKBN").ToString = "1" Then
                StkFreKBN = "積"
            ElseIf pOutputRowData("STACKFREEKBN").ToString = "2" Then
                StkFreKBN = "空"
            End If

            '対象日付（FROM ～ TO）編集
            Dim From_Date As Date = DateTime.Parse(Fromdate)
            Dim StrFrom_Date As String = Format(From_Date, "yyyy年 MM月 dd日")
            Dim To_Date As Date = DateTime.Parse(Todate)
            Dim StrTo_Date As String = Format(To_Date, "yyyy年 MM月 dd日")

            Me.PrintPageRowCnt = 1
            '◯日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = WkNowDateStr
            '〇頁数保存
            Me.PrintOutputPageRow.Add(Me.PrintOutputRowIdx.ToString())

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '〇帳票タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = REPORT_NAME
            '〇積空区分
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = StkFreKBN
            '◯対象日付（FROM ～ TO）
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = StrFrom_Date + " ～ " + StrTo_Date

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
    ''' 後半帳票ヘッダ出力
    ''' </summary>
    Private Sub EditHeaderArea_2(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow,
        ByVal Fromdate As String,
        ByVal Todate As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダーを作成するたび頁数分母加算
            Me.PrintMaxPageCnt += 1
            '途切れた場合支店を表示する
            Me.PrintBranchOutputFlg = True
            '初回ページは設定しない
            If pOldRowData IsNot Nothing Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:Q{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A24:Q28")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim Nowdt As DateTime = DateTime.Now
            Dim WkNowDateStr As String = Nowdt.ToString("yyyy.MM.dd")

            '積空区分判定
            Dim StkFreKBN As String = ""
            If pOutputRowData("STACKFREEKBN").ToString = "1" Then
                StkFreKBN = "積"
            ElseIf pOutputRowData("STACKFREEKBN").ToString = "2" Then
                StkFreKBN = "空"
            End If

            '対象日付（FROM ～ TO）編集
            Dim From_Date As Date = DateTime.Parse(Fromdate)
            Dim StrFrom_Date As String = Format(From_Date, "yyyy年 MM月 dd日")
            Dim To_Date As Date = DateTime.Parse(Todate)
            Dim StrTo_Date As String = Format(To_Date, "yyyy年 MM月 dd日")

            Me.PrintPageRowCnt = 1
            '◯日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = WkNowDateStr
            '〇頁数保存
            Me.PrintOutputPageRow.Add(Me.PrintOutputRowIdx.ToString())

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '〇帳票タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = REPORT_NAME
            '〇積空区分
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = StkFreKBN
            '◯対象日付（FROM ～ TO）
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = StrFrom_Date + " ～ " + StrTo_Date

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
    ''' 帳票明細出力
    ''' </summary>
    Private Sub EditDetailArea(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        If PrintFirstRowFlg Then
            '明細初行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A6:Q6")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)
        Else
            '2行目コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:Q7")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)
        End If

        '支店
        If Me.PrintBranchOutputFlg = True Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("JOTARRBRANCHNM")
        End If
        '大分類
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BIGCTNNM")
        '1～ / 1601～1700
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_1")
        '201～300 / 1701～1800
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_2")
        '301～400 / 1801～1900
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_3")
        '401～500 / 1901～2000
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_4")
        '501～600 / 2001～2100
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_5")
        '601～700 / 2101～2200
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_6")
        '701～800 / 2201～2300
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_7")
        '801～900 / 2301～2400
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_8")
        '901～1000 / 2401～2500
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_9")
        '1001～1100 / 2501～2600
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_10")
        '1101～1200 / 2601～2700
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_11")
        '1201～1300 / 2700～
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_12")
        '前半と後半で表示させる列を変える
        If pOutputRowData("HALFKBN").ToString = "1" Then
            '1301～1400
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_13")
            '1401～1500
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_14")
            '1501～1600
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_15")
            '後半時、合計個数と平均キロを1列増やす
        ElseIf pOutputRowData("HALFKBN").ToString = "2" Then
            '個数合計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_13")
            'キロ平均
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("VALUE_14")
        End If

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '合計金額加算
        '小計
        Me.RowTotal.CalcAdd(pOutputRowData)

    End Sub

    ''' <summary>
    ''' 帳票小計出力
    ''' </summary>
    Private Sub EditBigCtnTotalArea(ByVal pOldRowData As DataRow)
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:Q8")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        '全体のキロ平均出す
        Dim Kilo_Ave As Int32 = 0
        Kilo_Ave = ExclusionData(RowTotal.Total_15.ToString, RowTotal.Total_13.ToString)

        '大分類
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = "計"
        '1～ / 1601～1700
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_01
        '201～300 / 1701～1800
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_02
        '301～400 / 1801～1900
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_03
        '401～500 / 1901～2000
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_04
        '501～600 / 2001～2100
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_05
        '601～700 / 2101～2200
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_06
        '701～800 / 2201～2300
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_07
        '801～900 / 2301～2400
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_08
        '901～1000 / 2401～2500
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_09
        '1001～1100 / 2501～2600
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_10
        '1101～1200 / 2601～2700
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_11
        '1201～1300 / 2700～
        WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_12
        '前半と後半で表示させる列を変える
        If pOldRowData("HALFKBN").ToString = "1" Then
            '1301～1400
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_13
            '1401～1500
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_14
            '1501～1600
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_15
            '後半時、合計個数と平均キロを1列増やす
        ElseIf pOldRowData("HALFKBN").ToString = "2" Then
            '個数合計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = RowTotal.Total_13
            'キロ平均
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = Kilo_Ave
        End If

        '出力件数加算
        Me.AddPrintRowCnt(1)

        '小計（種別計）クリア
        Me.RowTotal.Clear()

        '全社計を出力し終えた時点で改頁
        If pOldRowData("JOTARRBRANCHCD").ToString.Equals("AAAAAA") Then
            Me.PrintPageBreakFlg = True
        End If

    End Sub

    ''' <summary>
    ''' 売却除外数計算
    ''' </summary>
    Private Function ExclusionData(
                    ByVal QUANTITY_1 As String,
                    ByVal QUANTITY_2 As String
         ) As Int32

        Dim IntQUANTITY_1 As Int32 = 0
        Dim IntQUANTITY_2 As Int32 = 0

        If Integer.TryParse(QUANTITY_1, 10) Then
            IntQUANTITY_1 = Integer.Parse(QUANTITY_1)
        Else
            IntQUANTITY_1 = 0
        End If

        If Integer.TryParse(QUANTITY_2, 10) Then
            IntQUANTITY_2 = Integer.Parse(QUANTITY_2)
        Else
            IntQUANTITY_2 = 0
        End If

        '合計個数が0の場合計算しない
        If IntQUANTITY_2 = 0 Then
            Return 0
        End If

        Return IntQUANTITY_1 \ IntQUANTITY_2

    End Function

    ''' <summary>
    ''' 頁数出力
    ''' </summary>
    Private Sub EditOutputPage()

        Dim PageNum As Int32 = 0
        For Each PageRow In Me.PrintOutputPageRow
            '頁数加算
            PageNum += 1
            '〇頁数出力
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + PageRow.ToString()).Value = PageNum.ToString & "/" & Me.PrintMaxPageCnt
        Next
    End Sub

End Class
