Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Public Class LNT0001InvoiceOutputTNG
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0                                      '届先シート
    Private WW_SheetNoInv As Integer = 0                                   '請求書シート
    Private WW_SheetNoYuu As Integer = 0                                   '電力融通シート
    Private WW_SheetNoDetail As Integer = 0                                '届先明細テンプレートシート
    Private WW_SheetNoEnex As Integer = 0                                  '実績（エネックス東北使用）テンプレートシート

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private TodokeData As DataTable
    Private SheetData As DataTable
    Private YuuduuSheetData As DataTable
    Private KaisuuData As DataTable
    Private HolidayRate As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private TaishoLastDD As String = ""
    Private OutputFileName As String = ""

    Private USERID As String = ""
    Private USERTERMID As String = ""

    Private PrintKagamiRowIdx As Int32 = 0                                  '出力位置（鏡行）  　※初期値：0
    Private PrintYuuduuRowIdx As Int32 = 0                                  '出力位置（融通行）　※初期値：0
    Private PrintOutputRowIdx As Int32 = 12                                 '出力位置（行）    　※初期値：12
    Private PrintMaxRowIdx As Int32 = 0                                     '最終位置（行）    　※初期値：0
    Private PrintTotalFirstRowIdx As Int32 = 0                              '合計最初位置（行）  ※初期値：0
    Private PrintTotalLastRowIdx As Int32 = 0                               '合計最終位置（行）  ※初期値：0
    Private PrintTotalRowIdx As Int32 = 0                                   '合計位置（行）      ※初期値：0
    Private PrintSuuRowIdx As Int32 = 2                                     '数量位置（行）      ※初期値：2
    Private PrintEnexRowIdx As Int32 = 3                                    '実績（エネックス）位置（行）      ※初期値：3
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
                ElseIf WW_Workbook.Worksheets(i).Name = "電力融通（JOT入力）" Then
                    WW_SheetNoYuu = i
                ElseIf WW_Workbook.Worksheets(i).Name = "WORK（明細）" Then
                    WW_SheetNoDetail = i
                End If
            Next

            '帳票出力データ取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                '帳票出力データ取得
                PrintData = GetPrintData(SQLcon)
                '届先別シート情報データ取得
                TodokeData = GetTodokeData(SQLcon)
                ''シート情報データ取得
                'SheetData = GetSheetData(SQLcon)
                ''電力融通シート情報データ取得
                'YuuduuSheetData = GetYuuduuSheetData(SQLcon)
                '東北電力使用回数データ取得
                KaisuuData = GetKaisuuData(SQLcon)
                '休日割増単価マスタ取得
                HolidayRate = GetHolidayRate(SQLcon)
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
        'Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Dim CS0050SESSION As New CS0050SESSION
        Dim TODOKECODE As String = ""
        Dim TODOKENAME As String = ""
        Dim SHUKABASHO As String = ""
        Dim SHUKANAME As String = ""
        Dim FirstFLG As String = "1"
        Dim DataExist As String = "0"
        Dim NichiShukuCount As Integer = 0
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim OldTodokecode As String = ""

        Try

            '***** 届先別シート作成 TODO処理 ここから *****

            PrintKagamiRowIdx = 60
            '〇出荷場所、届先情報データループ
            For Each TodokeRowData As DataRow In TodokeData.Rows
                'テンプレートシートの検索
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = "WORK（明細）" Then
                        WW_SheetNoDetail = i
                    End If
                Next
                '出力シートの作成（テンプレートシートをコピーして届先、出荷場所別シートを作成）
                Dim copy_worksheet As IWorksheet = Me.WW_Workbook.Worksheets(Me.WW_SheetNoDetail).CopyBefore(Me.WW_Workbook.Worksheets(Me.WW_SheetNoDetail))
                copy_worksheet.Name = Left(Convert.ToString(TodokeRowData("SHEETNAME")), 31)
                copy_worksheet.Visible = Visibility.Visible

                '届先、出荷場所別シートの編集
                WW_SheetNo = CInt(copy_worksheet.Index)
                PrintOutputRowIdx = 12
                PrintMaxRowIdx = 63
                COL_MONTH = "L"
                COL_DAY1 = "M"
                COL_DAY2 = "N"
                COL_SHAGOU = "O"
                COL_SUURYOU = "P"
                FirstFLG = "1"
                DataExist = "0"
                NichiShukuCount = 0

                '--------------------------
                'ヘッダーの編集
                '--------------------------
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A5").Value = StrConv(TaishoYYYY, VbStrConv.Wide) & "年 " & StrConv(TaishoMM, VbStrConv.Wide) & "月 分 Ｌ Ｎ Ｇ 運 賃 明 細 書 　"
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A9").Value = Convert.ToString(TodokeRowData("TITLENAME"))

                '--------------------------
                '日別配送実績の編集
                '--------------------------
                Dim SelKey As String = "TODOKECODE ='" & Convert.ToString(TodokeRowData("TODOKECODE")) & "' and SHUKABASHO ='" & Convert.ToString(TodokeRowData("SHUKABASHO")) & "'"
                Dim SortKey As String = "TODOKEDATE ASC, SHUKADATE ASC, GYOMUTANKNUM ASC"
                For Each OutPutRowData As DataRow In PrintData.Select(SelKey, SortKey)
                    DataExist = "1"
                    '◯明細の設定
                    EditDetailArea(OutPutRowData, FirstFLG)
                    '営業日区分が休日割増単価マスタに存在するか
                    If HolidayRate.Rows(0)("RANGECODE").ToString.IndexOf(Convert.ToString(OutPutRowData("WORKINGDAY"))) >= 0 Then
                        NichiShukuCount += 1
                    End If
                Next

                '車番毎集計の編集
                'If DataExist = "1" Then
                Dim dt As New DataTable

                    PrintOutputRowIdx = 67
                    PrintTotalFirstRowIdx = PrintOutputRowIdx
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        dt = GetTankaData(SQLcon, Convert.ToString(TodokeRowData("TODOKECODE")), Convert.ToString(TodokeRowData("SHUKABASHO")), "1")
                        For Each Row As DataRow In dt.Rows
                            '◯合計の設定
                            EditTotalArea(Row, TodokeRowData)
                        Next
                    End Using
                    '◯合計の設定
                    PrintTotalLastRowIdx = 86
                    EditTotalLastArea(TodokeRowData)
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Visible = Visibility.Visible

                'End If

                '日・祝日割増料金
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L9").Value = NichiShukuCount

                '--------------------------
                '請求書（鏡）の編集
                '--------------------------
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & PrintKagamiRowIdx.ToString).Value = Convert.ToString(TodokeRowData("TODOKENAME"))

                If Convert.ToString(TodokeRowData("ORGCODE")) = "020402" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("K" & PrintKagamiRowIdx.ToString).Value = "EX東北/輸送費"
                End If

                If DataExist = "1" Then
                    If OldTodokecode = Convert.ToString(TodokeRowData("TODOKECODE")) Then
                        '同じ行届先の場合、足し算
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & (PrintKagamiRowIdx - 1).ToString).Formula &= "+'" & Convert.ToString(TodokeRowData("SHEETNAME")) & "'!E" & Me.PrintTotalRowIdx.ToString
                    Else
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Formula = "='" & Convert.ToString(TodokeRowData("SHEETNAME")) & "'!E" & Me.PrintTotalRowIdx.ToString
                        PrintKagamiRowIdx += 1
                    End If
                Else
                    If OldTodokecode = Convert.ToString(TodokeRowData("TODOKECODE")) Then
                        '同じ行届先の場合、足し算
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & (PrintKagamiRowIdx - 1).ToString).Formula &= "+0"
                    Else
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & PrintKagamiRowIdx.ToString).Value = 0
                        PrintKagamiRowIdx += 1
                    End If
                End If
                '同じ行届先の判定用
                OldTodokecode = Convert.ToString(TodokeRowData("TODOKECODE"))

                '数量欄の編集
                srcRange = Nothing
                destRange = Nothing
                srcRange = WW_Workbook.Worksheets(WW_SheetNoInv).Range("Q2:R2")
                destRange = WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & PrintSuuRowIdx.ToString)
                srcRange.Copy(destRange)

                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & PrintSuuRowIdx.ToString).Value = Convert.ToString(TodokeRowData("SHEETNAME"))
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & PrintSuuRowIdx.ToString).Formula = "='" & Convert.ToString(TodokeRowData("SHEETNAME")) & "'!L6"
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & PrintSuuRowIdx.ToString).NumberFormat = ""
                PrintSuuRowIdx += 1

                '--------------------------
                '実績（エネックス東北使用）シートの編集
                '--------------------------
                WW_SheetNoEnex = WW_Workbook.Worksheets.Count - 1   '出荷場所、届先シートが増えるため常に最終シート№を取得
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoEnex).Range("B" & PrintEnexRowIdx.ToString).Value = Convert.ToString(TodokeRowData("SHEETNAME"))
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoEnex).Range("C" & PrintEnexRowIdx.ToString).Formula = "='" & Convert.ToString(TodokeRowData("SHEETNAME")) & "'!L6"
                If Convert.ToString(TodokeRowData("ORGCODE")) = "020402" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoEnex).Range("H" & PrintEnexRowIdx.ToString).Value = "EX東北"
                Else
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoEnex).Range("H" & PrintEnexRowIdx.ToString).Value = "EX新潟"
                End If
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoEnex).Rows(PrintEnexRowIdx - 1).Hidden = False
                PrintEnexRowIdx += 1

            Next

            '数量欄（合計行）の編集
            srcRange = Nothing
            destRange = Nothing
            srcRange = WW_Workbook.Worksheets(WW_SheetNoInv).Range("Q3:R3")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & Me.PrintSuuRowIdx.ToString)
            srcRange.Copy(destRange)

            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & Me.PrintSuuRowIdx.ToString()).Formula = "=SUM(O2:O" & (Me.PrintSuuRowIdx - 1).ToString() & ")"

            '***** 届先別シート作成 TODO処理 ここまで *****


            '***** 電力融通シート作成 TODO処理 ここから *****
            PrintKagamiRowIdx = 33
            PrintYuuduuRowIdx = 10
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続
                '〇電力融通シート情報データループ
                For Each KaisuuRowData As DataRow In KaisuuData.Rows
                    FirstFLG = "1"
                    DataExist = "0"

                    '◯鏡の設定
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & Convert.ToString(PrintKagamiRowIdx)).Value = Convert.ToString(KaisuuRowData("SYABANNAME")) & "（固定費）"
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & Convert.ToString(PrintKagamiRowIdx)).Formula = "='電力融通（JOT入力）'!G" & Convert.ToString(PrintYuuduuRowIdx)
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(PrintKagamiRowIdx - 1).Hidden = False
                    If Not String.IsNullOrEmpty(Convert.ToString(KaisuuRowData("YUUDUU"))) Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("K" & Convert.ToString(PrintKagamiRowIdx)).Value = "EX東北/固定費"
                    End If
                    PrintKagamiRowIdx += 1

                    '◯電力融通の設定
                    If Not String.IsNullOrEmpty(Convert.ToString(KaisuuRowData("YUUDUU"))) Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("B" & Convert.ToString(PrintYuuduuRowIdx)).Value = KaisuuRowData("SYABANNAME")
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("C" & Convert.ToString(PrintYuuduuRowIdx)).Value = KaisuuRowData("KOTEIHIM")
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("D" & Convert.ToString(PrintYuuduuRowIdx)).Value = KaisuuRowData("KOTEIHID")
                        If Convert.ToString(KaisuuRowData("KAISU")) <> "0" Then
                            Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("E" & Convert.ToString(PrintYuuduuRowIdx)).Value = Convert.ToString(KaisuuRowData("KAISU"))
                        End If
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("F" & Convert.ToString(PrintYuuduuRowIdx)).Formula = "D" & Convert.ToString(PrintYuuduuRowIdx) & "*E" & Convert.ToString(PrintYuuduuRowIdx)
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("G" & Convert.ToString(PrintYuuduuRowIdx)).Formula = "C" & Convert.ToString(PrintYuuduuRowIdx) & "-D" & Convert.ToString(PrintYuuduuRowIdx) & "*E" & Convert.ToString(PrintYuuduuRowIdx)
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Rows(PrintYuuduuRowIdx - 1).Hidden = False
                        PrintYuuduuRowIdx += 1
                    End If
                Next
            End Using

            '***** 電力融通シート作成 TODO処理 ここまで *****

            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("L2").Value = CInt(TaishoYYYY)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("L3").Value = CInt(TaishoMM)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("L4").Value = CInt(TaishoLastDD)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("L5").Value = CInt(TaishoYYYY)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("L6").Value = CInt(TaishoMM)

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
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea(ByVal pOutputRowData As DataRow, ByRef FirstFLG As String)

        Try
            '届日(月)
            If FirstFLG = "1" Then
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_MONTH + Me.PrintOutputRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
                FirstFLG = "0"
            Else
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_MONTH + Me.PrintOutputRowIdx.ToString()).Value = ""
            End If
            '届日(日)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_DAY1 + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
            '出荷日(日)
            If Not pOutputRowData("SHUKADATE") Is DBNull.Value Then
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_DAY2 + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
            End If
            '車号
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_SHAGOU + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_SUURYOU + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")

            '最大行まで出力したら列変更
            If Me.PrintOutputRowIdx = PrintMaxRowIdx Then
                COL_MONTH = "Q"
                COL_DAY1 = "R"
                COL_DAY2 = "S"
                COL_SHAGOU = "T"
                COL_SUURYOU = "U"

                '初回フラグリセット
                FirstFLG = "1"
                '出力行リセット
                Me.PrintOutputRowIdx = 12
            Else
                '出力件数加算
                Me.AddPrintRowCnt(1)
            End If

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の合計設定
    ''' </summary>
    Private Sub EditTotalArea(ByVal pOutputRowData As DataRow, ByVal pSheetRowData As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行コピー
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Rows(PrintMaxRowIdx - 1).Hidden = False

            Dim Fomula1 As String = "=COUNTIF($D$12:$D$" & PrintMaxRowIdx.ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ")+COUNTIF($I$12:$I$" & PrintMaxRowIdx.ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ")"
            Dim Fomula2 As String = "=SUMIF($D$12:$D$" & PrintMaxRowIdx.ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ",$E$12:$E$" & PrintMaxRowIdx.ToString & ")+SUMIF($I$12:$I$" & PrintMaxRowIdx.ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ",$J$12:$J$" & PrintMaxRowIdx.ToString & ")"
            Dim Fomula3 As String = "=ROUND(E" & Me.PrintOutputRowIdx.ToString() & "*F" & Me.PrintOutputRowIdx.ToString() & ",0)"

            '車号
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHABAN")
            '車数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula1
            '単価
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TANKA")
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula2
            '金額
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula3

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の合計設定
    ''' </summary>
    Private Sub EditTotalLastArea(ByVal pTodokeRowData As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行コピー
            Dim Fomula1 As String = "=SUM(D" & Me.PrintTotalFirstRowIdx.ToString() & ":D" & Me.PrintTotalLastRowIdx.ToString() & ")"
            Dim Fomula2 As String = "=SUM(F" & Me.PrintTotalFirstRowIdx.ToString() & ":G" & Me.PrintTotalLastRowIdx.ToString() & ")"
            Dim Fomula3 As String = "=SUM(H" & Me.PrintTotalFirstRowIdx.ToString() & ":I" & Me.PrintTotalLastRowIdx.ToString() & ")"

            '車数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + (Me.PrintTotalLastRowIdx + 1).ToString()).Formula = Fomula1
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + (Me.PrintTotalLastRowIdx + 1).ToString()).Formula = Fomula2
            '金額
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + (Me.PrintTotalLastRowIdx + 1).ToString()).Formula = Fomula3

            '出力件数加算
            'Me.AddPrintRowCnt(1)
            ''行クリア（テンプレートのごみをクリアしておく（行削除、行追加）
            ''最終行の取得
            'Dim lastRow As Integer = WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Row + WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Rows.Count - 1
            'For i As Integer = Me.PrintOutputRowIdx To lastRow
            '    WW_Workbook.Worksheets(Me.WW_SheetNo).Range(i.ToString + ":" + i.ToString).Delete()
            '    WW_Workbook.Worksheets(Me.WW_SheetNo).Range(i.ToString + ":" + i.ToString).Insert()
            'Next

            '出力件数加算
            PrintOutputRowIdx = PrintTotalLastRowIdx + 1
            Me.AddPrintRowCnt(2)

            '合計
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pTodokeRowData("TODOKENAME")
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=H" & (Me.PrintOutputRowIdx - 2).ToString()
            '出力件数加算
            Me.AddPrintRowCnt(1)

            '日・祝日割増料金
            'Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=D" & Me.PrintOutputRowIdx.ToString() & "*20000"
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=D" & Me.PrintOutputRowIdx.ToString() & "*" & HolidayRate.Rows(0)("TANKA").ToString
            '出力件数加算
            Me.AddPrintRowCnt(1)

            '小計
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=SUM(E" & (Me.PrintOutputRowIdx - 2).ToString() & ":E" & (Me.PrintOutputRowIdx - 1).ToString() & ")"
            '出力件数加算
            Me.AddPrintRowCnt(1)

            '合計
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=E" & (Me.PrintOutputRowIdx - 1).ToString()
            PrintTotalRowIdx = PrintOutputRowIdx
            '出力件数加算
            Me.AddPrintRowCnt(1)

            '消費税
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=ROUND(E" & (Me.PrintOutputRowIdx - 1).ToString() & "*0.1,0)"
            '出力件数加算
            Me.AddPrintRowCnt(1)

            'ご請求合計
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + Me.PrintOutputRowIdx.ToString()).Formula = "=SUM(E" & (Me.PrintOutputRowIdx - 2).ToString() & ":E" & (Me.PrintOutputRowIdx - 1).ToString() & ")"
            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 文字列置換
    ''' </summary>
    Private Function StrReplace(ByVal Str As String) As String

        Dim RetrunStr As String = ""

        RetrunStr = Str.Replace("（出荷地）", "")
        RetrunStr = RetrunStr.Replace("（ＴＮＧ）", "")

        Return RetrunStr
    End Function

    ''' <summary>
    ''' 帳票出力データ取得
    ''' </summary>
    Private Function GetPrintData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.TORICODE "
        SQLStr &= " , A01.TODOKECODE "
        SQLStr &= " , A01.TODOKENAME "
        SQLStr &= " , A01.SHUKABASHO "
        SQLStr &= " , A01.SHUKANAME "
        SQLStr &= " , A01.TODOKEDATE "
        SQLStr &= " , CASE "
        SQLStr &= "       WHEN A01.SHUKADATE = A01.TODOKEDATE THEN NULL "
        SQLStr &= "       ELSE A01.SHUKADATE "
        SQLStr &= "   END AS SHUKADATE "
        SQLStr &= " , A01.GYOMUTANKNUM "
        SQLStr &= " , A01.ZISSEKI * 1000 AS ZISSEKI "
        SQLStr &= " , A01.BRANCHCODE "
        SQLStr &= " , A02.WORKINGDAY AS WORKINGDAY "

        '-- FROM
        SQLStr &= " FROM LNG.LNT0001_ZISSEKI A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN LNG.LNM0016_CALENDAR A02 "
        SQLStr &= "     ON A02.TORICODE = A01.TORICODE"
        SQLStr &= "     AND A02.YMD = A01.TODOKEDATE"

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.TORICODE = '{0}' ", "0175300000")
        SQLStr &= String.Format(" AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.ZISSEKI <> '{0}' ", "0")
        SQLStr &= String.Format(" AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   A01.TODOKECODE "
        SQLStr &= " , A01.SHUKABASHO "
        SQLStr &= " , A01.TODOKEDATE "
        SQLStr &= " , A01.GYOMUTANKNUM "
        SQLStr &= " , A01.SHUKADATE "

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
    ''' 届先一覧取得
    ''' </summary>
    Private Function GetTodokeData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.ORGCODE"
        SQLStr &= " , A01.AVOCADOSHUKABASHO AS SHUKABASHO"
        SQLStr &= " , REPLACE(A01.AVOCADOSHUKANAME,'（ＴＮＧ）','')  AS SHUKANAME"
        SQLStr &= " , A01.AVOCADOTODOKECODE AS TODOKECODE"
        SQLStr &= " , REPLACE(A01.AVOCADOTODOKENAME,'（ＴＮＧ）','') AS TODOKENAME "
        SQLStr &= " ,  CONCAT(REPLACE(A01.AVOCADOSHUKANAME,'（ＴＮＧ）',''), ' ～ ', REPLACE(A01.AVOCADOTODOKENAME,'（ＴＮＧ）','')) AS TITLENAME "
        SQLStr &= " ,  CONCAT(REPLACE(REPLACE(A01.AVOCADOTODOKENAME,'（ＴＮＧ）',''),'・',''), '(', REPLACE(REPLACE(A01.AVOCADOSHUKANAME,'（ＴＮＧ）',''),'・',''),')') AS SHEETNAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175300000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.STYMD  <= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.ENDYMD >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

        '-- GROUP BY
        SQLStr &= " GROUP BY "
        SQLStr &= "   A01.ORGCODE"
        SQLStr &= " , A01.AVOCADOSHUKABASHO"
        SQLStr &= " , A01.AVOCADOSHUKANAME"
        SQLStr &= " , A01.AVOCADOTODOKECODE"
        SQLStr &= " , A01.AVOCADOTODOKENAME"

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   A01.ORGCODE DESC"
        SQLStr &= " , A01.AVOCADOTODOKECODE"
        SQLStr &= " , A01.AVOCADOSHUKABASHO DESC"

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
    Private Function GetSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.KEYCODE01 AS TODOKECODE "
        SQLStr &= " , A01.KEYCODE02 AS SHUKABASHO "
        SQLStr &= " , A01.KEYCODE03 AS TODOKENAME "
        SQLStr &= " , A01.KEYCODE04 AS SHEETNO "
        SQLStr &= " , A01.VALUE01 AS TODOKENAME_INV "
        SQLStr &= " , A01.VALUE02 AS SHEETDISPLAY "
        SQLStr &= " , A01.VALUE03 AS MAXROW "
        SQLStr &= " , A01.VALUE04 AS KAGAMIROW "
        SQLStr &= " , A01.VALUE05 AS KAGAMIQTYROW "
        SQLStr &= " , A01.VALUE06 AS SHEETNAME "
        SQLStr &= " , A01.VALUE07 AS TITLENAME "
        SQLStr &= " , A01.VALUE08 AS TOTALNAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.CLASS = '{0}' ", "TNG_TODOKESHEET_INFO")

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   CAST(A01.KEYCODE04 AS SIGNED) "

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
    ''' 電力融通シート情報データ取得
    ''' </summary>
    Private Function GetYuuduuSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.KEYCODE01 AS SYABAN "
        SQLStr &= " , A01.KEYCODE02 "
        SQLStr &= " , A01.VALUE01 AS SYABANNAME "
        SQLStr &= " , A01.VALUE02 AS ROWDISPLAY "
        SQLStr &= " , A01.VALUE03 AS ROWNO "
        SQLStr &= " , A01.VALUE04 AS KAGAMIROW "
        SQLStr &= " , A01.VALUE05 AS KAGAMINAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.CLASS = '{0}' ", "TNG_YUUDUU_INFO")

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   CAST(A01.VALUE03 AS SIGNED) "

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
    ''' 東北電力使用回数データ取得
    ''' </summary>
    Private Function GetKaisuuData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.SYABAN"
        SQLStr &= " , CONCAT(A01.SYABAN,'号車') AS SYABANNAME"
        SQLStr &= " , A01.KOTEIHIM "
        SQLStr &= " , IFNULL(A01.KOTEIHID,0) AS KOTEIHID "
        SQLStr &= " , IFNULL(A02.KAISU,0) AS KAISU "
        SQLStr &= " , IFNULL(A01.KOTEIHID,0) * IFNULL(A02.KAISU,0) AS GENGAKU "
        SQLStr &= " , A01.KOTEIHIM - IFNULL(A01.KOTEIHID,0) * IFNULL(A02.KAISU,0) AS GOUKEI "
        SQLStr &= " , A01.ORGCODE "
        SQLStr &= " , F01.VALUE1 AS YUUDUU "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_FIXED A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN ( "
        SQLStr &= "           SELECT"
        SQLStr &= "               DATE_FORMAT(A12.TODOKEDATE, '%Y/%m/01') as TODOKEDATE"
        SQLStr &= "              ,A11.SYABAN     as SYABAN"
        SQLStr &= "              ,COUNT(A12.TORICODE) AS KAISU "
        SQLStr &= "           FROM LNG.LNM0007_FIXED A11 "
        SQLStr &= "           INNER JOIN LNG.LNT0001_ZISSEKI A12 "
        SQLStr &= "               ON A12.TORICODE = '0175400000' "
        SQLStr &= "               AND A12.GYOMUTANKNUM = A11.SYABAN "
        SQLStr &= String.Format(" AND DATE_FORMAT(A12.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= "               AND A12.ZISSEKI <> 0 "
        SQLStr &= "               AND A12.DELFLG = '0' "
        SQLStr &= "           WHERE "
        SQLStr &= String.Format("     A11.TORICODE  = '{0}' ", "0175300000")
        SQLStr &= String.Format(" AND A11.TARGETYM  = '{0}' ", TaishoYm.Replace("/", ""))
        SQLStr &= "               AND A11.DELFLG   = '0' "
        SQLStr &= "           GROUP BY "
        SQLStr &= "               DATE_FORMAT(A12.TODOKEDATE, '%Y/%m/01') "
        SQLStr &= "              ,A11.SYABAN "
        SQLStr &= "           ) A02 "
        SQLStr &= String.Format(" ON A02.TODOKEDATE >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format("AND A02.TODOKEDATE <= '{0}' ", Date.Parse(TaishoYm + "/" + "01").AddDays(-(Date.Parse(TaishoYm + "/" + "01").Day - 1)).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd"))
        SQLStr &= "              AND A02.SYABAN      = A01.SYABAN "
        SQLStr &= " LEFT JOIN  COM.LNS0006_FIXVALUE F01"
        SQLStr &= String.Format(" ON  F01.CAMPCODE = '{0}' ", "01")
        SQLStr &= String.Format(" AND F01.CLASS    = '{0}' ", "TNG_YUUDUU_ORG")
        SQLStr &= String.Format(" AND F01.STYMD   <= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND F01.ENDYMD  >= '{0}' ", TaishoYm & "/01")
        SQLStr &= "               AND F01.DELFLG   = '0' "
        SQLStr &= "               AND F01.KEYCODE  = A01.ORGCODE "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE  = '{0}' ", "0175300000")
        SQLStr &= String.Format(" AND A01.TARGETYM  = '{0}' ", TaishoYm.Replace("/", ""))
        SQLStr &= String.Format(" AND A01.DELFLG   <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   A01.ORGCODE DESC"
        SQLStr &= " , A01.SYABAN"
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
    ''' 単価データ取得
    ''' </summary>
    Private Function GetTankaData(ByVal SQLcon As MySqlConnection, ByVal TODOKECODE As String, ByVal SHUKABASHO As String, ByVal BRANCHCODE As String) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.SHABAN"
        SQLStr &= " , A01.TANKA "

        '-- FROM
        'SQLStr &= " FROM LNG.LNM0006_TANKA A01 "
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175300000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format(" AND A01.AVOCADOSHUKABASHO = '{0}' ", SHUKABASHO)
        SQLStr &= String.Format(" AND A01.AVOCADOTODOKECODE = '{0}' ", TODOKECODE)
        SQLStr &= String.Format(" AND A01.BRANCHCODE = '{0}' ", BRANCHCODE)
        SQLStr &= String.Format(" AND A01.STYMD  <= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.ENDYMD >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format(" AND A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

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
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175300000")

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
End Class
