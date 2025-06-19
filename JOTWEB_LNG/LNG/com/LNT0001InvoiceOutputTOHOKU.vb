Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Public Class LNT0001InvoiceOutputTOHOKU
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0                                      '処理シート
    'Private WW_SheetNoInput As Integer = 0                                 '入力シート
    Private WW_SheetNoInv As Integer = 0                                   '請求書シート
    Private WW_SheetNoTui As Integer = 0                                   '追加料金・日曜日料金シート
    Private WW_SheetNoHai As Integer = 0                                   '配送先シート
    'Private WW_SheetNoTmp As Integer = 0                                   'テンプレートシート
    Private WW_SheetNoFix As Integer = 0                                   'WORK（固定費）シート
    Private WW_SheetNoDetail As Integer = 0                                'WORK（明細）シート

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private HaiSheetData As DataTable
    Private ShaSheetData As DataTable
    Private TankaData As DataTable
    Private TuiSheetData As DataTable
    Private HolidayRate As DataTable
    Private SpRate As DataTable
    Private KaisuuData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private TaishoLastDD As String = ""
    Private OutputFileName As String = ""

    Private USERID As String = ""
    Private USERTERMID As String = ""

    Private PrintOutputRowIdx As Int32 = 3                                  '出力位置（行）    　※初期値：3
    Private PrintFixRowIdx As Int32 = 0                                     '出力位置（行）    　※初期値：0
    Private PrintFixRowIdx2 As Int32 = 0                                    '出力位置（行）    　※初期値：0
    Private PrintKagamiRowIdx As Int32 = 36                                 '鏡出力位置（行）  　※初期値：36
    Private PrintMaxRowIdx As Int32 = 0                                     '最終位置（行）    　※初期値：0
    Private PrintTotalFirstRowIdx As Int32 = 0                              '合計最初位置（行）  ※初期値：0
    Private PrintTotalLastRowIdx As Int32 = 0                               '合計最終位置（行）  ※初期値：0
    Private PrintTotalRowIdx As Int32 = 0                                   '合計位置（行）      ※初期値：0
    Private PrintSuuRowIdx As Int32 = 25                                    '数量位置（行）      ※初期値：24
    Private PrintHaiRowIdx As Int32 = 6                                     '配送先位置（行）    ※初期値：6
    Private PrintShaRowIdx As Int32 = 2                                     '車号位置（行）      ※初期値：12
    Private PrintTankaRowIdx As Int32 = 17                                  '単価位置（行）      ※初期値：17
    Private PrintaddsheetFlg As Boolean = False                             'シート追加フラグ　  ※初期値：False
    Private TodokeCodeCHGFlg As Boolean = False                             '届先変更フラグ    　※初期値：False
    Private ShukaBashoCHGFlg As Boolean = False                             '出荷場所変更フラグ　※初期値：False

    Private COL_MONTH As String = "L"                                       '届日(月)
    Private COL_DAY1 As String = "M"                                        '届日(日)
    Private COL_DAY2 As String = "N"                                        '出荷日(日)
    Private COL_SHAGOU As String = "O"                                      '車号
    Private COL_SUURYOU As String = "P"                                     '数量

    Private CS0011LOGWrite As New CS0011LOGWrite                            'ログ出力

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, outputFileName As String, user_id As String, term_id As String,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.TaishoLastDD = Date.Parse(taishoYm + "/" + "01").AddDays(-(Date.Parse(taishoYm + "/" + "01").Day - 1)).AddMonths(1).AddDays(-1).ToString("dd")
            Me.OutputFileName = outputFileName
            USERID = user_id
            USERTERMID = term_id
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                          "PRINTFORMAT",
                                                          defaultDatakey,
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
                '今日の日付が先頭のファイル名の場合は残す
                If fileName.StartsWith(keepFilePrefix) Then
                    Continue For
                End If
                Try
                    IO.File.Delete(targetFile)
                Catch ex As Exception
                    '削除時のエラーは無視
                End Try
            Next targetFile
            'URLのルートを表示
            'Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", CS0050SESSION.HTTPS_GET, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルopen
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "請求書" Then
                    WW_SheetNoInv = i
                ElseIf WW_Workbook.Worksheets(i).Name = "追加料金・日曜日料金（JOT入力）" Then
                    WW_SheetNoTui = i
                ElseIf WW_Workbook.Worksheets(i).Name = "配送先" Then
                    WW_SheetNoHai = i
                ElseIf WW_Workbook.Worksheets(i).Name = "WORK（固定費）" Then
                    WW_SheetNoFix = i
                ElseIf WW_Workbook.Worksheets(i).Name = "WORK（明細）" Then
                    WW_SheetNoDetail = i
                End If
            Next

            '帳票出力データ取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                '帳票出力データ取得
                PrintData = GetPrintData(SQLcon)
                '届先シート情報データ取得
                HaiSheetData = GetHaiSheetData(SQLcon)
                '追加料金・日曜日料金シート情報データ取得
                TuiSheetData = GetTuiSheetData(SQLcon)
                '車号シート情報データ取得
                ShaSheetData = GetShaSheetData(SQLcon)
                '統合版単価マスタ取得
                TankaData = GetTankaData(SQLcon)
                '休日割増単価マスタ取得
                HolidayRate = GetHolidayRate(SQLcon)
                '統合版特別料金マスタ取得
                SpRate = GetSpRate(SQLcon)

            End Using

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData() As String
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Dim CS0050SESSION As New CS0050SESSION
        Dim NichiCount As Integer = 0
        Dim TuiCount As Integer = 0
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '***** 請求書シート作成  *****
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("H3").Value = TaishoYYYY & "年” & TaishoMM & "月" & TaishoLastDD & "日"
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C16").Value = TaishoYYYY & "年” & TaishoMM & "月分"
            Dim NextMonth As Date = CDate(TaishoYm + "/" + "01").AddMonths(1)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("H21").Value = TaishoYYYY & "年” & NextMonth.ToString("MM") & "月 末日"
            '***** 請求書シート作成  *****

            '***** 追加料金・日曜日料金シート作成 TODO処理 ここから *****
            PrintFixRowIdx = 5
            PrintFixRowIdx2 = 38
            For Each TuiSheetRowData As DataRow In TuiSheetData.Rows
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("B" & PrintFixRowIdx.ToString).Value = Convert.ToString(TuiSheetRowData("GYOMUTANKTNAME"))
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("B" & PrintFixRowIdx2.ToString).Value = Convert.ToString(TuiSheetRowData("GYOMUTANKTNAME"))
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Rows(PrintFixRowIdx - 1).Hidden = False
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Rows(PrintFixRowIdx2 - 1).Hidden = False

                '統合版特別料金マスタより
                For Each SpRateRow As DataRow In SpRate.Rows
                    Dim SyabanConv = ToHalfWidth(Convert.ToString(SpRateRow("DETAILNAME")))
                    If SyabanConv Like Convert.ToString(TuiSheetRowData("GYOMUTANKTNAME")) & "*" Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("C" & PrintFixRowIdx.ToString).Value = SpRateRow("TANKA")
                        Exit For
                    End If
                Next
                If Val(Convert.ToString(TuiSheetRowData("SPKAISU"))) > 0 Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D" & PrintFixRowIdx.ToString).Value = Convert.ToString(TuiSheetRowData("SPKAISU"))
                End If
                TuiCount += Convert.ToInt32(TuiSheetRowData("SPKAISU"))
                PrintFixRowIdx += 1

                '日曜日割増運賃
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("C" & PrintFixRowIdx2.ToString).Value = HolidayRate.Rows(0)("TANKA")
                If Convert.ToString(TuiSheetRowData("NICHIYOUCNT")) <> "0" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D" & PrintFixRowIdx2.ToString).Value = Convert.ToString(TuiSheetRowData("NICHIYOUCNT"))
                    NichiCount += Convert.ToInt32(TuiSheetRowData("NICHIYOUCNT"))
                End If
                PrintFixRowIdx2 += 1
            Next
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D34").Value = TuiCount.ToString("0")
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D67").Value = NichiCount

            '***** 追加料金・日曜日料金シート作成 TODO処理 ここまで *****

            '***** 車号別シート作成 TODO処理 ここから *****
            For Each ShaSheetRowData As DataRow In ShaSheetData.Rows
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = "WORK（固定費）" Then
                        WW_SheetNoFix = i
                        Exit For
                    End If
                Next

                If Convert.ToString(ShaSheetRowData("EXISTFLG")) <> "0" Then
                    '車番別、固定費シート作成（
                    Dim copy_worksheet As IWorksheet = Me.WW_Workbook.Worksheets(Me.WW_SheetNoFix).CopyBefore(Me.WW_Workbook.Worksheets(Me.WW_SheetNoFix))
                    copy_worksheet.Name = Convert.ToString(ShaSheetRowData("SHEETNAME"))
                    copy_worksheet.Visible = Visibility.Visible

                    '固定費（月額、日額）
                    If Convert.ToString(ShaSheetRowData("EXISTFLG")) = "2" Then
                        '東北天然ガス専用車両（融通車両）
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("I8").Formula = "=D56*I9"
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("I9").Value = ShaSheetRowData("KOTEIHID")
                    Else
                        '東北電力専用車両
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("I8").Value = ShaSheetRowData("KOTEIHIM")
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("I9").Value = ShaSheetRowData("KOTEIHID")
                    End If

                    '支店別、日別、明細出力
                    Dim OLD_ORG As String = ""
                    Dim SortKey As String = "ORDERORGNAME ASC, TODOKEDATE ASC, SHUKADATE ASC, TODOKECODE ASC, SHUKABASHO ASC"
                    Dim Selkey As String = "GYOMUTANKNUM = '" & Convert.ToString(ShaSheetRowData("GYOMUTANKNUM")) & "'"

                    For Each OutPutRowData As DataRow In PrintData.Select(Selkey, SortKey)
                        If OLD_ORG <> Convert.ToString(OutPutRowData("ORDERORGNAME")) Then
                            OLD_ORG = Convert.ToString(OutPutRowData("ORDERORGNAME"))
                            PrintFixRowIdx = 12
                        End If
                        EditFixDetail("1", copy_worksheet.Index, OutPutRowData)
                    Next

                    Dim TodokeList As New List(Of String)

                    '日別、明細出力
                    PrintFixRowIdx = 12
                    SortKey = "TODOKEDATE ASC, SHUKADATE ASC, TODOKECODE ASC, SHUKABASHO ASC"
                    For Each OutPutRowData As DataRow In PrintData.Select(Selkey, SortKey)
                        EditFixDetail("2", copy_worksheet.Index, OutPutRowData)

                        '届先を保存（重複なし）
                        If Not TodokeList.Contains(Convert.ToString(OutPutRowData("TODOKENAME"))) Then
                            TodokeList.Add(Convert.ToString(OutPutRowData("TODOKENAME")))
                        End If
                    Next

                    '配送先別集計
                    PrintFixRowIdx = 59
                    'とりあえず、名前の降順にソート
                    TodokeList.Sort(Function(x, y) y.CompareTo(x))
                    For i As Integer = 0 To TodokeList.Count - 1
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("A" + Me.PrintFixRowIdx.ToString()).Value = TodokeList(i)
                        PrintFixRowIdx += 1
                    Next

                    '請求書（鏡）出力
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                    If Convert.ToString(ShaSheetRowData("EXISTFLG")) = "2" Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = Convert.ToString(ShaSheetRowData("SHEETNAME")) & "※TNG配属車両"
                    Else
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = Convert.ToString(ShaSheetRowData("SHEETNAME"))
                    End If
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Formula = "='" & Convert.ToString(ShaSheetRowData("SHEETNAME")) & "'!E76"

                    PrintKagamiRowIdx += 1
                End If
            Next

            '***** 車号別シート作成 TODO処理 ここまで *****

            '***** 届先別シート作成 TODO処理 ここから *****
            For Each HaiSheetRowData As DataRow In HaiSheetData.Rows
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = "WORK（明細）" Then
                        WW_SheetNoDetail = i
                        Exit For
                    End If
                Next

                '出荷場所、届先別、固定費シート作成（
                Dim copy_worksheet As IWorksheet = Me.WW_Workbook.Worksheets(Me.WW_SheetNoDetail).CopyBefore(Me.WW_Workbook.Worksheets(Me.WW_SheetNoDetail))
                copy_worksheet.Name = Convert.ToString(HaiSheetRowData("SHEETNAME"))
                copy_worksheet.Visible = Visibility.Visible

                Dim OLD_ORG As String = ""
                Dim SelKey As String = "SHUKABASHO = '" & Convert.ToString(HaiSheetRowData("SHUKABASHO")) & "' and TODOKECODE = '" & Convert.ToString(HaiSheetRowData("TODOKECODE")) & "'"
                Dim SortKey As String = "ORDERORGNAME ASC, TODOKEDATE ASC, SHUKADATE ASC, SHUKABASHO ASC, TODOKECODE ASC"
                For Each OutPutRowData As DataRow In PrintData.Select(SelKey, SortKey)
                    If OLD_ORG <> Convert.ToString(OutPutRowData("ORDERORGNAME")) Then
                        OLD_ORG = Convert.ToString(OutPutRowData("ORDERORGNAME"))
                        PrintFixRowIdx = 12
                    End If
                    EditTODOKEDetail("1", copy_worksheet.Index, OutPutRowData)
                Next

                PrintFixRowIdx = 12
                SortKey = " TODOKEDATE ASC, SHUKADATE ASC, SHUKABASHO ASC, TODOKECODE ASC"
                For Each OutPutRowData As DataRow In PrintData.Select(SelKey, SortKey)
                    EditTODOKEDetail("2", copy_worksheet.Index, OutPutRowData)
                Next

                '車番別、単価設定（実績がある時のみ）
                SelKey = "TODOKECODE ='" & Convert.ToString(HaiSheetRowData("TODOKECODE")) & "' and SHUKABASHO = '" & Convert.ToString(HaiSheetRowData("SHUKABASHO")) & "'"
                If PrintData.Select(SelKey, SortKey).Count > 0 Then
                    '実績がある時のみ単価設定
                    PrintTankaRowIdx = 17
                    For Each OutPutRowData As DataRow In TankaData.Select(SelKey)
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("B" & PrintTankaRowIdx.ToString).Value = OutPutRowData("SHABAN")
                        Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("E" & PrintTankaRowIdx.ToString).Value = OutPutRowData("TANKA")
                        If OutPutRowData("CALCKBN").ToString = "トン" Then
                            Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("H" & PrintTankaRowIdx.ToString).Formula = "=ROUND(E" & Me.PrintTankaRowIdx.ToString() & "*" & "F" & Me.PrintTankaRowIdx.ToString() & ",0)"
                        ElseIf OutPutRowData("CALCKBN").ToString = "回" Then
                            Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("H" & PrintTankaRowIdx.ToString).Formula = "=ROUND(E" & Me.PrintTankaRowIdx.ToString() & "*" & "D" & Me.PrintTankaRowIdx.ToString() & ",0)"
                        Else
                            Me.WW_Workbook.Worksheets(copy_worksheet.Index).Range("H" & PrintTankaRowIdx.ToString).Formula = "=ROUND(E" & Me.PrintTankaRowIdx.ToString() & "*" & "D" & Me.PrintTankaRowIdx.ToString() & ",0)"
                        End If
                        PrintTankaRowIdx += 1
                    Next
                End If
            Next

            '請求書（鏡）出力
            PrintKagamiRowIdx = 57
            Dim OutFlg As Integer = 0
            For Each HaiSheetRowData As DataRow In HaiSheetData.Select("", "TODOKECODE ASC, SHUKABASHO DESC")
                Dim TitleStr As String = Mid(Convert.ToString(HaiSheetRowData("SHEETNAME")), InStr(Convert.ToString(HaiSheetRowData("SHEETNAME")), "～") + 1, Convert.ToString(HaiSheetRowData("SHEETNAME")).Length)
                If Convert.ToString(HaiSheetRowData("SHUKABASHO")) = "006932" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = " " & TitleStr & "向け(新潟)"
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Formula = "='" & Convert.ToString(HaiSheetRowData("SHEETNAME")) & "'!F12"
                    PrintKagamiRowIdx += 1
                    OutFlg = 1
                ElseIf Convert.ToString(HaiSheetRowData("SHUKABASHO")) = "004756" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = " " & TitleStr & "向け(仙台)"
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Formula = "='" & Convert.ToString(HaiSheetRowData("SHEETNAME")) & "'!F12"
                    PrintKagamiRowIdx += 1
                    OutFlg = 2
                End If
                '合計行
                If OutFlg = 2 Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = " " & TitleStr & "向け 合計"
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Formula = "=D" & (PrintKagamiRowIdx - 2).ToString & "+ D" & (PrintKagamiRowIdx - 1).ToString
                    PrintKagamiRowIdx += 1
                End If
            Next

            '***** 届先別シート作成 TODO処理 ここまで *****

            '***** 配送先別シート作成  *****
            Dim query = From row In PrintData.AsEnumerable()
                        Group row By TODOKECODE = row.Field(Of String)("TODOKECODE"), TODOKENAME = row.Field(Of String)("TODOKENAME") Into Group
                        Order By TODOKECODE Ascending
                        Select New With {
                            .TODOKECODE = TODOKECODE,
                            .TODOKENAME = TODOKENAME,
                            .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of Decimal)("ZISSEKI")))
                        }

            PrintFixRowIdx = 2
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "配送先" Then
                    WW_SheetNoHai = i
                    Exit For
                End If
            Next
            For Each result In query
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Rows(PrintFixRowIdx - 1).Hidden = False
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Range("A" & PrintFixRowIdx.ToString).Value = result.TODOKENAME
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Range("B" & PrintFixRowIdx.ToString).Value = result.ZISSEKI / 1000
                PrintFixRowIdx += 1
            Next
            '***** 配送先別シート作成  *****

            Me.WW_Workbook.Calculate()

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
    ''' 出力件数加算
    ''' </summary>
    Private Sub AddPrintRowCnt(ByVal pAddCnt As Int32)

        'EXCEL出力位置加算
        Me.PrintOutputRowIdx += pAddCnt

    End Sub

    ''' <summary>
    ''' Excel作業シート設定
    ''' </summary>
    ''' <param name="sheetName"></param>
    Protected Function TrySetExcelWorkSheet(ByRef idx As Integer, ByVal sheetName As String, Optional ByVal templateSheetName As String = Nothing) As Boolean
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
                idx = 12
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
    Private Sub EditHeaderArea(ByVal TODOKENAME As String, ByVal SHUKANAME As String)
        Try
            '〇 タイトル
            WW_Workbook.Worksheets(WW_SheetNo).Range("A5").Value = StrConv(Me.TaishoYYYY, VbStrConv.Wide) & "年 " & StrConv(Me.TaishoMM, VbStrConv.Wide) & "月 分 Ｌ Ｎ Ｇ 運 賃 明 細 書 　"
            '〇 出荷場所～届先
            WW_Workbook.Worksheets(WW_SheetNo).Range("A9").Value = SHUKANAME & "～" & TODOKENAME

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 固定費の明細設定
    ''' </summary>
    Private Sub EditFixDetail(ByVal pKbn As String, ByVal pSheetNo As Integer, ByVal pOutputRowData As DataRow)

        Try
            If pKbn = "2" Then
                '支店をまとめた出力
                '届日(月)
                Me.WW_Workbook.Worksheets(pSheetNo).Range("L" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                '届日(日)
                Me.WW_Workbook.Worksheets(pSheetNo).Range("M" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                '出荷日(日)
                If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("N" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                End If
                '車号
                Me.WW_Workbook.Worksheets(pSheetNo).Range("O" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                '数量
                Me.WW_Workbook.Worksheets(pSheetNo).Range("P" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
                '発送元
                Me.WW_Workbook.Worksheets(pSheetNo).Range("Q" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("SHUKANAME")
                '配送先
                Me.WW_Workbook.Worksheets(pSheetNo).Range("R" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("TODOKENAME")

            Else
                '支店別の出力
                If Convert.ToString(pOutputRowData("ORDERORGNAME")) = "新潟支店" Then
                    '届日(月)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("T" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                    '届日(日)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("U" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                    '出荷日(日)
                    If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                        Me.WW_Workbook.Worksheets(pSheetNo).Range("V" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                    End If
                    '車号
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("W" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                    '数量
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("X" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
                    '発送元
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("Y" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("SHUKANAME")
                    '配送先
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("Z" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("TODOKENAME")
                End If

                If Convert.ToString(pOutputRowData("ORDERORGNAME")) = "東北支店" Then
                    '届日(月)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AB" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                    '届日(日)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AC" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                    '出荷日(日)
                    If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                        Me.WW_Workbook.Worksheets(pSheetNo).Range("AD" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                    End If
                    '車号
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AE" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                    '数量
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AF" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
                    '発送元
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AG" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("SHUKANAME")
                    '配送先
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AH" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("TODOKENAME")
                End If

            End If

            '出力件数加算
            PrintFixRowIdx += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 届先別の明細設定
    ''' </summary>
    Private Sub EditTODOKEDetail(ByVal pKbn As String, ByVal pSheetNo As Integer, ByVal pOutputRowData As DataRow)

        Try
            If pKbn = "2" Then
                '支店をまとめた出力
                '届日(月)
                Me.WW_Workbook.Worksheets(pSheetNo).Range("L" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                '届日(日)
                Me.WW_Workbook.Worksheets(pSheetNo).Range("M" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                '出荷日(日)
                If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("N" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                End If
                '車号
                Me.WW_Workbook.Worksheets(pSheetNo).Range("O" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                '数量
                Me.WW_Workbook.Worksheets(pSheetNo).Range("P" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")

            Else
                '支店別の出力
                If Convert.ToString(pOutputRowData("ORDERORGNAME")) = "新潟支店" Then
                    '届日(月)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("R" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                    '届日(日)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("S" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                    '出荷日(日)
                    If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                        Me.WW_Workbook.Worksheets(pSheetNo).Range("T" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                    End If
                    '車号
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("U" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                    '数量
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("V" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
                End If
                If Convert.ToString(pOutputRowData("ORDERORGNAME")) = "東北支店" Then
                    '届日(月)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("X" + Me.PrintFixRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                    '届日(日)
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("Y" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
                    '出荷日(日)
                    If pOutputRowData("TODOKEDATE").ToString <> pOutputRowData("SHUKADATE").ToString Then
                        Me.WW_Workbook.Worksheets(pSheetNo).Range("Z" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
                    End If
                    '車号
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AA" + Me.PrintFixRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
                    '数量
                    Me.WW_Workbook.Worksheets(pSheetNo).Range("AB" + Me.PrintFixRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
                End If
            End If

            '出力件数加算
            PrintFixRowIdx += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票出力データ取得
    ''' </summary>
    Private Function GetPrintData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.TODOKEDATE "
        SQLStr &= "    ,A01.SHUKADATE "
        SQLStr &= "    ,A01.GYOMUTANKNUM "
        SQLStr &= "    ,A01.ZISSEKI * 1000 AS ZISSEKI "
        SQLStr &= "    ,CASE SHUKABASHO WHEN '006928' "
        SQLStr &= "     THEN (SELECT SHUKABASHO "
        SQLStr &= "             FROM LNG.LNT0001_ZISSEKI"
        SQLStr &= "            WHERE "
        SQLStr &= String.Format("     DELFLG      <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND TORICODE     = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND ORDERORG    IN ({0}) ", "'020402','021502'")
        SQLStr &= "               AND GYOMUTANKNUM = A01.GYOMUTANKNUM "
        SQLStr &= "               AND TRIP         = A01.TRIP -1 "
        SQLStr &= "               AND TODOKEDATE   =  A01.TODOKEDATE) "
        SQLStr &= "     ELSE A01.SHUKABASHO "
        SQLStr &= "     END AS SHUKABASHO "
        SQLStr &= "    ,CASE SHUKABASHO WHEN '006928' "
        SQLStr &= "     THEN (SELECT SHUKANAME "
        SQLStr &= "             FROM LNG.LNT0001_ZISSEKI"
        SQLStr &= "            WHERE "
        SQLStr &= String.Format("     DELFLG      <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND TORICODE     = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND ORDERORG    IN ({0}) ", "'020402','021502'")
        SQLStr &= "               AND GYOMUTANKNUM = A01.GYOMUTANKNUM "
        SQLStr &= "               AND TRIP         = A01.TRIP -1 "
        SQLStr &= "               AND TODOKEDATE   =  A01.TODOKEDATE) "
        SQLStr &= "     ELSE A01.SHUKANAME "
        SQLStr &= "     END AS SHUKANAME "
        SQLStr &= "    ,A01.TODOKECODE "
        SQLStr &= "    ,REPLACE(A01.TODOKENAME,'（東北電力）','') AS TODOKENAME "
        SQLStr &= "    ,CASE A01.ORDERORG "
        SQLStr &= "         WHEN '020402' THEN '東北支店' "
        SQLStr &= "         WHEN '021502' THEN '新潟支店' "
        SQLStr &= "     END ORDERORGNAME "
        SQLStr &= "    ,A01.BRANCHCODE "
        SQLStr &= "    ,A01.TRIP "

        '-- FROM
        SQLStr &= " FROM LNG.LNT0001_ZISSEKI A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.ZISSEKI <> '{0}' ", "0")
        SQLStr &= String.Format(" AND A01.LOADUNLOTYPE <> '{0}' ", "積込")
        SQLStr &= String.Format(" AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   Left(A01.GYOMUTANKNUM,1) DESC "
        SQLStr &= " , A01.GYOMUTANKNUM "
        SQLStr &= " , A01.ORDERORG "
        SQLStr &= " , A01.TODOKEDATE "
        SQLStr &= " , A01.SHUKADATE "
        SQLStr &= " , A01.TODOKECODE "
        SQLStr &= " , A01.SHUKABASHO "
        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function

    ''' <summary>
    ''' 届先別シート情報データ取得
    ''' </summary>
    Private Function GetHaiSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.ORGCODE AS ORDERORG "
        SQLStr &= "    ,A01.AVOCADOSHUKABASHO AS SHUKABASHO "
        SQLStr &= "    ,A01.AVOCADOSHUKANAME AS SHUKANAME "
        SQLStr &= "    ,A01.AVOCADOTODOKECODE AS TODOKECODE "
        SQLStr &= "    ,REPLACE(A01.AVOCADOTODOKENAME,'（東北電力）','') AS TODOKENAME "
        SQLStr &= "    ,CONCAT(REPLACE(A01.AVOCADOSHUKANAME,'・',''),'～',REPLACE(A01.AVOCADOTODOKENAME,'（東北電力）','')) AS SHEETNAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.STYMD  <= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.ENDYMD >= '{0}' ", TaishoYm & "/01")
        'SQLStr &= String.Format(" AND A01.BRANCHCODE = '{0}' ", "1")

        '-- GROUP BY
        SQLStr &= "     GROUP BY "
        SQLStr &= "       A01.ORGCODE "
        SQLStr &= "      ,A01.AVOCADOSHUKABASHO "
        SQLStr &= "      ,A01.AVOCADOSHUKANAME "
        SQLStr &= "      ,A01.AVOCADOTODOKECODE "
        SQLStr &= "      ,A01.AVOCADOTODOKENAME "

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   A01.AVOCADOSHUKABASHO, A01.AVOCADOTODOKECODE"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function

    ''' <summary>
    ''' 車号シート情報データ取得
    ''' </summary>
    Private Function GetShaSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.SYABAN AS GYOMUTANKNUM "
        SQLStr &= "    ,CONCAT(A01.SYABAN,'号車（固定費）') AS SHEETNAME "
        SQLStr &= "    ,CASE"
        SQLStr &= "         WHEN A01.TORICODE = '0175300000' AND A02.GYOMUTANKNUM IS NOT NULL THEN '2'"
        SQLStr &= "         WHEN A01.TORICODE = '0175300000' AND A02.GYOMUTANKNUM IS     NULL THEN '0'"
        SQLStr &= "         ELSE '1'"
        SQLStr &= "     END EXISTFLG"
        SQLStr &= "    ,A01.KOTEIHIM"
        SQLStr &= "    ,A01.KOTEIHID"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_FIXED A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN("
        SQLStr &= "     SELECT "
        SQLStr &= "         A01.GYOMUTANKNUM "
        SQLStr &= "     FROM LNG.LNT0001_ZISSEKI A01 "
        SQLStr &= "     WHERE "
        SQLStr &= String.Format("          A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format("      AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format("      AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format("      AND A01.ZISSEKI <> '{0}' ", "0")
        SQLStr &= String.Format("      AND A01.LOADUNLOTYPE <> '{0}' ", "積込")
        SQLStr &= String.Format("      AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= "     GROUP BY "
        SQLStr &= "       A01.GYOMUTANKNUM "
        SQLStr &= " )A02 "
        SQLStr &= " ON A02.GYOMUTANKNUM = A01.SYABAN "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.TORICODE IN ({0}) ", "'0175400000','0175300000'")
        SQLStr &= String.Format(" AND A01.ORGCODE  IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.TARGETYM = '{0}' ", TaishoYm.Replace("/", ""))

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   LEFT(A01.SYABAN,1) DESC, A01.SYABAN"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function

    ''' <summary>
    ''' 追加料金・日曜日料金シート情報データ取得
    ''' </summary>
    Private Function GetTuiSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.SYABAN AS GYOMUTANKNUM "
        SQLStr &= "    ,CONCAT(A01.SYABAN,'号車') AS GYOMUTANKTNAME "
        SQLStr &= "    ,IFNULL(A02.NICHIYOUCNT,0) AS NICHIYOUCNT "
        SQLStr &= "    ,IFNULL(A03.CNT,0) AS SPKAISU"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_FIXED A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN("
        SQLStr &= "     SELECT "
        SQLStr &= "         A01.GYOMUTANKNUM "
        SQLStr &= "        ,COUNT(A01.GYOMUTANKNUM) NICHIYOUCNT "
        SQLStr &= "     FROM LNG.LNT0001_ZISSEKI A01 "
        SQLStr &= "     LEFT JOIN LNG.LNM0016_CALENDAR A02 "
        SQLStr &= "         ON A02.TORICODE = A01.TORICODE"
        SQLStr &= "         AND A02.YMD = A01.TODOKEDATE"
        SQLStr &= "     WHERE "
        SQLStr &= String.Format("          A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format("      AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format("      AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format("      AND A01.ZISSEKI <> '{0}' ", "0")
        SQLStr &= String.Format("      AND A01.LOADUNLOTYPE <> '{0}' ", "積込")
        SQLStr &= String.Format("      AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= String.Format("      AND A02.WEEKDAY = '{0}' ", "0")
        SQLStr &= "     GROUP BY "
        SQLStr &= "       A01.GYOMUTANKNUM "
        SQLStr &= " )A02 "
        SQLStr &= " ON A02.GYOMUTANKNUM = A01.SYABAN "
        SQLStr &= " LEFT JOIN("
        SQLStr &= "     SELECT "
        SQLStr &= "         A02.GYOMUTANKNUM "
        SQLStr &= "        ,COUNT(*) AS CNT "
        SQLStr &= "     FROM ( "
        SQLStr &= "         SELECT "
        SQLStr &= "             A01.TODOKEDATE "
        SQLStr &= "            ,A01.GYOMUTANKNUM "
        SQLStr &= "         FROM LNG.LNT0001_ZISSEKI A01 "
        SQLStr &= "         WHERE "
        SQLStr &= String.Format("          A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format("      AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format("      AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format("      AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= "         GROUP BY "
        SQLStr &= "          A01.TODOKEDATE "
        SQLStr &= "         ,A01.GYOMUTANKNUM "
        SQLStr &= "         HAVING COUNT(*) >= 4 "
        SQLStr &= "     )A02 "
        SQLStr &= "     GROUP BY "
        SQLStr &= "          A02.GYOMUTANKNUM "
        SQLStr &= " )A03 "
        SQLStr &= " ON A03.GYOMUTANKNUM = A01.SYABAN "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.TORICODE IN ({0}) ", "'0175400000','0175300000'")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.TARGETYM = '{0}' ", TaishoYm.Replace("/", ""))

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   LEFT(A01.SYABAN,1) DESC, A01.SYABAN "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function

    ''' <summary>
    ''' 統合版単価マスタ取得
    ''' </summary>
    Private Function GetTankaData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.SHABAN            AS SHABAN"
        SQLStr &= " , A01.TANKA             AS TANKA"
        SQLStr &= " , A01.AVOCADOTODOKECODE AS TODOKECODE"
        SQLStr &= " , A01.AVOCADOSHUKABASHO AS SHUKABASHO"
        SQLStr &= " , A01.BRANCHCODE        AS BRANCHCODE"
        SQLStr &= " , A01.CALCKBN           AS CALCKBN"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.STYMD  <= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.ENDYMD >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= " ORDER BY LEFT(A01.SHABAN,1) DESC, A01.SHABAN "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function
    ''' <summary>
    ''' 休日祝日割増単価マスタ取得
    ''' </summary>
    Private Function GetHolidayRate(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.RANGECODE"
        SQLStr &= " , A01.TANKA "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0017_HOLIDAYRATE A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.DELFLG  <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function
    ''' <summary>
    ''' 統合版特別料金マスタ取得
    ''' </summary>
    Private Function GetSpRate(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.DETAILNAME"
        SQLStr &= " , A01.TANKA "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0014_SPRATE A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TARGETYM = '{0}' ", TaishoYm.Replace("/", ""))
        SQLStr &= String.Format(" AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.DISPLAYFLG  = '{0}' ", "1")
        SQLStr &= String.Format(" AND A01.DELFLG  <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= " ORDER BY A01.DETAILID "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return dt
    End Function
    ''' <summary>
    ''' 全角数字を半角に変換
    ''' </summary>
    Public Function ToHalfWidth(num As String) As String
        Dim result As String = ""
        For Each c As Char In num
            If c >= "０"c AndAlso c <= "９"c Then
                result &= ChrW(AscW(c) - &HFEE0)
            Else
                result &= c
            End If
        Next
        Return result
    End Function
End Class
