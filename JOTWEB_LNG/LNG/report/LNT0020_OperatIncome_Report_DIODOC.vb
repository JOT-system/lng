Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 営業収入計上一覧表作成クラス
''' </summary>
Public Class LNT0020_OperatIncomeReport_DIODOC

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
    Private PrintRowBreakFlg As Boolean = False                             '改行フラグ　※初期値：False
    Private PrintTotalRowFlg As Boolean = False                             '合計行フラグ　※初期値：False
    Private PrintReportFileName As String = ""                              'ファイル名
    Private CTNTotal As Integer = 0                                         'コンテナ部合計
    Private HokkaidoTotal As Integer = 0                                    '北海道支店合計
    Private TouhokuTotal As Integer = 0                                     '東北支店合計
    Private KantouTotal As Integer = 0                                      '関東支店合計
    Private TyubuTotal As Integer = 0                                       '中部支店合計
    Private KansaiTotal As Integer = 0                                      '関西支店合計
    Private KyusyuTotal As Integer = 0                                      '九州支店合計
    Private KeiriTotal As Integer = 0                                       '経理部合計
    Private HonsyaTotal As Integer = 0                                      '本社合計
    Private Total As Integer = 0                                            '支店合計
    Private SegCTNTotal As Integer = 0                                      'セグメントコンテナ部合計
    Private SegHokkaidoTotal As Integer = 0                                 'セグメント北海道支店合計
    Private SegTouhokuTotal As Integer = 0                                  'セグメント東北支店合計
    Private SegKantouTotal As Integer = 0                                   'セグメント関東支店合計
    Private SegTyubuTotal As Integer = 0                                    'セグメント中部支店合計
    Private SegKansaiTotal As Integer = 0                                   'セグメント関西支店合計
    Private SegKyusyuTotal As Integer = 0                                   'セグメント九州支店合計
    Private SegKeiriTotal As Integer = 0                                    'セグメント経理部合計
    Private SegHonsyaTotal As Integer = 0                                   'セグメント本社合計
    Private SegTotal As Integer = 0                                         'セグメント合計
    Private GrandCTNTotal As Integer = 0                                    '総合コンテナ部合計
    Private GrandHokkaidoTotal As Integer = 0                               '総合北海道支店合計
    Private GrandTouhokuTotal As Integer = 0                                '総合東北支店合計
    Private GrandKantouTotal As Integer = 0                                 '総合関東支店合計
    Private GrandTyubuTotal As Integer = 0                                  '総合中部支店合計
    Private GrandKansaiTotal As Integer = 0                                 '総合関西支店合計
    Private GrandKyusyuTotal As Integer = 0                                 '総合九州支店合計
    Private GrandKeiriTotal As Integer = 0                                  '総合経理部合計
    Private GrandHonsyaTotal As Integer = 0                                 '総合本社合計
    Private GrandTotal As Integer = 0                                       '総合計

    Private Const REPORT_ID As String = "LNT0020"                                   '帳票ID
    Private Const REPORT_NAME As String = "勘定科目別・計上店別営業収入計上一覧表"  '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 50                            '改頁行

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
    Public Function CreateExcelPrintData(Fromdate As String, Todate As String, PeriodType As String, FiscalYear As String, ReportName As String) As String
        Dim TitleName As String = ""
        Select Case ReportName
            Case "00"
                TitleName = ""
            Case "01"
                TitleName = "リース"
        End Select
        Dim TmpFileName As String = ""
        Select Case PeriodType
            Case "00"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & Fromdate & "～" & Todate & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "01"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "１Ｑ" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "02"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "２Ｑ" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "03"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "３Ｑ" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "04"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "４Ｑ" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "05"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "上期" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "06"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "下期" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "07"
                TmpFileName = Me.PrintReportFileName & TitleName & "勘定科目別・計上店別営業収入計上一覧表（" & FiscalYear & "年間" & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        End Select
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte
        Dim oldSEGMENTCODE As String = ""
        Dim oldACCOUNTCODE As String = ""

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows

                'セグメント変更判定
                If Not oldSEGMENTCODE.Equals(OutputRowData("SEGMENTCODE").ToString) AndAlso
                   Not oldSEGMENTCODE = "" Then
                    Me.PrintRowBreakFlg = True
                    Me.PrintTotalRowFlg = True
                End If

                '勘定科目変更判定
                If Not oldACCOUNTCODE.Equals(OutputRowData("ACCOUNTCODE").ToString) AndAlso
                   Not oldACCOUNTCODE = "" Then
                    Me.PrintRowBreakFlg = True
                End If

                '行数による改頁判定
                If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                    Me.PrintPageBreakFlg = True
                End If

                '改頁の場合、ヘッダ出力（初回出力も含む）
                If Me.PrintPageBreakFlg Then
                    '〇ヘッダー出力
                    Me.EditHeaderArea(OldRowData, OutputRowData, Fromdate, Todate, FiscalYear)
                    Me.PrintPageBreakFlg = False
                End If

                '〇明細出力
                If PrintRowBreakFlg Then
                    Me.EditDetailArea(OldRowData)
                    Me.PrintRowBreakFlg = False
                End If

                'セグメント合計
                If Me.PrintTotalRowFlg Then
                    EditDetailTotalArea(OldRowData)
                    Me.PrintTotalRowFlg = False
                End If

                '明細合計加算
                Me.BranchTotalAdd(OldRowData, OutputRowData)

                '前回出力明細データ保持
                OldRowData = OutputRowData
                oldSEGMENTCODE = OutputRowData("SEGMENTCODE").ToString
                oldACCOUNTCODE = OutputRowData("ACCOUNTCODE").ToString
            Next

            '〇明細出力
            Me.EditDetailArea(OldRowData)
            'セグメント合計出力
            EditDetailTotalArea(OldRowData)
            '総合計出力
            EditDetailGrandTotalArea()

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
        ByVal pOutputRowData As DataRow,
        ByVal FromDate As String,
        ByVal ToDate As String,
        ByVal FiscalYear As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim tirleName As String = ""

        Try
            '初回ページは設定しない
            If pOldRowData IsNot Nothing Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:N{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            Select Case pOutputRowData("PERIODTYPE").ToString
                Case "01"
                    tirleName = "1Q"
                Case "02"
                    tirleName = "2Q"
                Case "03"
                    tirleName = "3Q"
                Case "04"
                    tirleName = "4Q"
                Case "05"
                    tirleName = "上期"
                Case "06"
                    tirleName = "下期"
                Case "07"
                    tirleName = "年間"
            End Select

            'タイトルコピー
            If pOutputRowData("PRINTTYPE").ToString = "00" Then
                '全勘定科目
                srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:N2")
                destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
                srcRange.Copy(destRange)
                If pOutputRowData("PERIODTYPE").ToString = "00" Then
                    WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "勘定科目別・計上店別営業収入計上一覧表　（ " & FromDate & "～" & ToDate & " ）"
                Else
                    WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "勘定科目別・計上店別営業収入計上一覧表　（ " & FiscalYear & "年" & tirleName & " ）"
                End If
            ElseIf pOutputRowData("PRINTTYPE").ToString = "01" Then
                'リースのみ
                srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:N2")
                destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
                srcRange.Copy(destRange)
                If pOutputRowData("PERIODTYPE").ToString = "00" Then
                    WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "リース勘定科目別・計上店別営業収入計上一覧表　（ " & FromDate & "～" & ToDate & " ）"
                Else
                    WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = "リース勘定科目別・計上店別営業収入計上一覧表　（ " & FiscalYear & "年" & tirleName & " ）"
                End If
            End If
            Me.AddPrintRowCnt(2)
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:N8")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            Me.PrintPageRowCnt = 1

            '出力件数加算
            Me.AddPrintRowCnt(2)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票明細出力
    ''' </summary>
    Private Sub BranchTotalAdd(
        ByVal pOldRowData As DataRow,
        ByVal pOutputRowData As DataRow
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        Try
            Select Case pOutputRowData("ORGCODE").ToString
                Case "011312"  'コンテナ部
                    CTNTotal += CInt(pOutputRowData("TOTAL"))
                Case "010102"  '北海道支店
                    HokkaidoTotal += CInt(pOutputRowData("TOTAL"))
                Case "010401"  '東北支店
                    TouhokuTotal += CInt(pOutputRowData("TOTAL"))
                Case "011402"  '関東支店
                    KantouTotal += CInt(pOutputRowData("TOTAL"))
                Case "012401"  '中部支店
                    TyubuTotal += CInt(pOutputRowData("TOTAL"))
                Case "012701"  '関西支店
                    KansaiTotal += CInt(pOutputRowData("TOTAL"))
                Case "014001"  '九州支店
                    KyusyuTotal += CInt(pOutputRowData("TOTAL"))
                Case "011307"  '経理部
                    KeiriTotal += CInt(pOutputRowData("TOTAL"))
                Case "011301"  '本社
                    HonsyaTotal += CInt(pOutputRowData("TOTAL"))
            End Select
            Total += CInt(pOutputRowData("TOTAL"))

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票明細出力
    ''' </summary>
    Private Sub EditDetailArea(
        ByVal pOldRowData As DataRow
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:N10")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        Try

            '勘定科目コード
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("ACCOUNTCODE")
            '勘定科目名称
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("ACCOUNTNAME")
            'セグメントコード
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("SEGMENTCODE")
            'セグメント名称
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("SEGMENTNAME")
            'コンテナ部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = CTNTotal
            '北海道支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = HokkaidoTotal
            '東北支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = TouhokuTotal
            '関東支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = KantouTotal
            '中部支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = TyubuTotal
            '関西支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = KansaiTotal
            '九州支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = KyusyuTotal
            '経理部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = KeiriTotal
            '本社
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = HonsyaTotal
            '合計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = Total

            '総合計加算
            SegCTNTotal += CTNTotal
            SegHokkaidoTotal += HokkaidoTotal
            SegTouhokuTotal += TouhokuTotal
            SegKantouTotal += KantouTotal
            SegTyubuTotal += TyubuTotal
            SegKansaiTotal += KansaiTotal
            SegKyusyuTotal += KyusyuTotal
            SegKeiriTotal += KeiriTotal
            SegHonsyaTotal += HonsyaTotal
            SegTotal += Total

            'セグメント合計リセット
            CTNTotal = 0
            HokkaidoTotal = 0
            TouhokuTotal = 0
            KantouTotal = 0
            TyubuTotal = 0
            KansaiTotal = 0
            KyusyuTotal = 0
            KeiriTotal = 0
            HonsyaTotal = 0
            Total = 0

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票セグメント合計出力
    ''' </summary>
    Private Sub EditDetailTotalArea(
        ByVal pOldRowData As DataRow
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A12:N12")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        Try

            '合計行名称
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pOldRowData("SEGMENTNAME").ToString + "計"
            'コンテナ部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = SegCTNTotal
            '北海道支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = SegHokkaidoTotal
            '東北支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = SegTouhokuTotal
            '関東支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = SegKantouTotal
            '中部支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = SegTyubuTotal
            '関西支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = SegKansaiTotal
            '九州支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = SegKyusyuTotal
            '経理部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = SegKeiriTotal
            '本社
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = SegHonsyaTotal
            '合計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = SegTotal

            '総合計加算
            GrandCTNTotal += SegCTNTotal
            GrandHokkaidoTotal += SegHokkaidoTotal
            GrandTouhokuTotal += SegTouhokuTotal
            GrandKantouTotal += SegKantouTotal
            GrandTyubuTotal += SegTyubuTotal
            GrandKansaiTotal += SegKansaiTotal
            GrandKyusyuTotal += SegKyusyuTotal
            GrandKeiriTotal += SegKeiriTotal
            GrandHonsyaTotal += SegHonsyaTotal
            GrandTotal += SegTotal

            'セグメント合計リセット
            SegCTNTotal = 0
            SegHokkaidoTotal = 0
            SegTouhokuTotal = 0
            SegKantouTotal = 0
            SegTyubuTotal = 0
            SegKansaiTotal = 0
            SegKyusyuTotal = 0
            SegKeiriTotal = 0
            SegHonsyaTotal = 0
            SegTotal = 0

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票総合計出力
    ''' </summary>
    Private Sub EditDetailGrandTotalArea()

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A14:N14")
        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
        srcRange.Copy(destRange)

        Try

            'コンテナ部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = GrandCTNTotal
            '北海道支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = GrandHokkaidoTotal
            '東北支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = GrandTouhokuTotal
            '関東支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = GrandKantouTotal
            '中部支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + Me.PrintOutputRowIdx.ToString()).Value = GrandTyubuTotal
            '関西支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = GrandKansaiTotal
            '九州支店
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + Me.PrintOutputRowIdx.ToString()).Value = GrandKyusyuTotal
            '経理部
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + Me.PrintOutputRowIdx.ToString()).Value = GrandKeiriTotal
            '本社
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + Me.PrintOutputRowIdx.ToString()).Value = GrandHonsyaTotal
            '合計
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + Me.PrintOutputRowIdx.ToString()).Value = GrandTotal

            'セグメント合計リセット
            GrandCTNTotal = 0
            GrandHokkaidoTotal = 0
            GrandTouhokuTotal = 0
            GrandKantouTotal = 0
            GrandTyubuTotal = 0
            GrandKansaiTotal = 0
            GrandKyusyuTotal = 0
            GrandKeiriTotal = 0
            GrandHonsyaTotal = 0
            GrandTotal = 0

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

End Class
