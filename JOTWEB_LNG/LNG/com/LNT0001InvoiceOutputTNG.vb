Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Public Class LNT0001InvoiceOutputTNG
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0                                      '届先シート
    Private WW_SheetNoInv As Integer = 0                                   '請求書シート
    Private WW_SheetNoYuu As Integer = 0                                   '電力融通シート
    Private WW_SheetNoTmp As Integer = 0                                   'テンプレートシート

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
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

    Private PrintOutputRowIdx As Int32 = 12                                 '出力位置（行）    　※初期値：12
    Private PrintMaxRowIdx As Int32 = 0                                     '最終位置（行）    　※初期値：0
    Private PrintTotalFirstRowIdx As Int32 = 0                              '合計最初位置（行）  ※初期値：0
    Private PrintTotalLastRowIdx As Int32 = 0                               '合計最終位置（行）  ※初期値：0
    Private PrintTotalRowIdx As Int32 = 0                                   '合計位置（行）      ※初期値：0
    Private PrintSuuRowIdx As Int32 = 2                                     '数量位置（行）      ※初期値：2
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
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_SheetNoTmp = i
                End If
            Next

            '帳票出力データ取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続

                '帳票出力データ取得
                PrintData = GetPrintData(SQLcon)
                'シート情報データ取得
                SheetData = GetSheetData(SQLcon)
                '電力融通シート情報データ取得
                YuuduuSheetData = GetYuuduuSheetData(SQLcon)
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
        Dim OldKagamirow As Integer = 0

        Try

            '***** 届先別シート作成 TODO処理 ここから *****

            '〇シート情報データループ
            For Each SheetRowData As DataRow In SheetData.Rows
                WW_SheetNo = CInt(SheetRowData("SHEETNO"))
                PrintOutputRowIdx = 12
                PrintMaxRowIdx = CInt(SheetRowData("MAXROW"))
                COL_MONTH = "L"
                COL_DAY1 = "M"
                COL_DAY2 = "N"
                COL_SHAGOU = "O"
                COL_SUURYOU = "P"
                FirstFLG = "1"
                DataExist = "0"
                NichiShukuCount = 0

                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A5").Value = StrConv(TaishoYYYY, VbStrConv.Wide) & "年 " & StrConv(TaishoMM, VbStrConv.Wide) & "月 分 Ｌ Ｎ Ｇ 運 賃 明 細 書 　"

                Dim DataCount As Integer = PrintData.Select("TODOKECODE ='" & Convert.ToString(SheetRowData("TODOKECODE")) & "' and SHUKABASHO ='" & Convert.ToString(SheetRowData("SHUKABASHO")) & "'").Count
                If DataCount > 0 Then
                    Dim OutPutRowData As DataRow() = PrintData.Select("TODOKECODE ='" & Convert.ToString(SheetRowData("TODOKECODE")) & "' and SHUKABASHO ='" & Convert.ToString(SheetRowData("SHUKABASHO")) & "'")
                    If OutPutRowData.Length > 0 Then
                        DataExist = "1"
                        For i As Integer = 0 To OutPutRowData.Length - 1
                            '◯明細の設定
                            EditDetailArea(OutPutRowData, i, FirstFLG)
                            '営業日区分が休日割増単価マスタに存在するか
                            If HolidayRate.Rows(0)("RANGECODE").ToString.IndexOf(Convert.ToString(OutPutRowData(i)("WORKINGDAY"))) >= 0 Then
                                NichiShukuCount += 1
                            End If
                        Next
                    End If
                End If

                '届先別シートの編集
                If DataExist = "1" AndAlso Convert.ToString(SheetRowData("SHEETDISPLAY")) = "1" Then
                    Dim dt As New DataTable

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A9").Value = Convert.ToString(SheetRowData("TITLENAME"))
                    PrintOutputRowIdx = Convert.ToInt32(SheetRowData("MAXROW")) + 4
                    PrintTotalFirstRowIdx = Convert.ToInt32(SheetRowData("MAXROW")) + 4
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        dt = GetTankaData(SQLcon, Convert.ToString(SheetRowData("TODOKECODE")), Convert.ToString(SheetRowData("SHUKABASHO")), "1")
                        For Each Row As DataRow In dt.Rows
                            '◯合計の設定
                            EditTotalArea(Row, SheetRowData)
                        Next
                    End Using
                    PrintTotalLastRowIdx = PrintOutputRowIdx - 1
                    '◯合計の設定
                    EditTotalLastArea(SheetRowData)
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Visible = Visibility.Visible
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Name = Convert.ToString(SheetRowData("SHEETNAME"))

                End If

                '日・祝日割増料金
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L9").Value = NichiShukuCount

                '請求書（鏡）の編集
                If Convert.ToString(SheetRowData("SHEETDISPLAY")) = "1" Then

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(CInt(SheetRowData("KAGAMIROW")) - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & SheetRowData("KAGAMIROW").ToString).Value = Convert.ToString(SheetRowData("TODOKENAME_INV"))
                    If DataExist = "1" Then
                        If OldKagamirow = CInt(SheetRowData("KAGAMIROW")) Then
                            '同じ行番号の場合、足し算
                            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & SheetRowData("KAGAMIROW").ToString).Formula &= "+'" & Convert.ToString(SheetRowData("SHEETNAME")) & "'!E" & Me.PrintTotalRowIdx.ToString
                        Else
                            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & SheetRowData("KAGAMIROW").ToString).Formula = "='" & Convert.ToString(SheetRowData("SHEETNAME")) & "'!E" & Me.PrintTotalRowIdx.ToString
                        End If
                    Else
                        If OldKagamirow = CInt(SheetRowData("KAGAMIROW")) Then
                            '同じ行番号の場合、足し算
                            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & SheetRowData("KAGAMIROW").ToString).Formula &= "+0"
                        Else
                            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & SheetRowData("KAGAMIROW").ToString).Value = 0
                        End If
                    End If

                    srcRange = Nothing
                    destRange = Nothing
                    srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("O2:P2")
                    destRange = WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & SheetRowData("KAGAMIQTYROW").ToString)
                    srcRange.Copy(destRange)

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & SheetRowData("KAGAMIQTYROW").ToString).Value = Convert.ToString(SheetRowData("SHEETNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & SheetRowData("KAGAMIQTYROW").ToString).Formula = "='" & Convert.ToString(SheetRowData("SHEETNAME")) & "'!L6"
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & SheetRowData("KAGAMIQTYROW").ToString).NumberFormat = ""
                    PrintSuuRowIdx += 1

                    '同じ行番号の判定用
                    OldKagamirow = CInt(SheetRowData("KAGAMIROW"))
                End If
            Next

            srcRange = Nothing
            destRange = Nothing
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("L25:M25")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("N" & Me.PrintSuuRowIdx.ToString)
            srcRange.Copy(destRange)

            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("O" & Me.PrintSuuRowIdx.ToString()).Formula = "=SUM(O2:O" & (Me.PrintSuuRowIdx - 1).ToString() & ")"

            '***** 届先別シート作成 TODO処理 ここまで *****


            '***** 電力融通シート作成 TODO処理 ここから *****
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()  ' DataBase接続
                '〇電力融通シート情報データループ
                For Each YuuduuSheetRowData As DataRow In YuuduuSheetData.Rows
                    FirstFLG = "1"
                    DataExist = "0"

                    Dim DataCount As Integer = KaisuuData.Select("SYABAN ='" & Convert.ToString(YuuduuSheetRowData("SYABAN")) & "'").Count
                    If DataCount > 0 Then
                        Dim OutPutRowData As DataRow() = KaisuuData.Select("SYABAN ='" & Convert.ToString(YuuduuSheetRowData("SYABAN")) & "'")
                        If OutPutRowData.Length > 0 Then
                            DataExist = "1"
                            For i As Integer = 0 To OutPutRowData.Length - 1
                                '◯明細の設定
                                If Convert.ToString(YuuduuSheetRowData("ROWDISPLAY")) = "1" Then
                                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("B" & Convert.ToString(YuuduuSheetRowData("ROWNO"))).Value = YuuduuSheetRowData("SYABANNAME")
                                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & Convert.ToString(YuuduuSheetRowData("KAGAMIROW"))).Value = YuuduuSheetRowData("KAGAMINAME")
                                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & Convert.ToString(YuuduuSheetRowData("KAGAMIROW"))).Formula = "='電力融通（JOT入力）'!G" & Convert.ToString(YuuduuSheetRowData("ROWNO"))
                                End If
                                Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("C" & Convert.ToString(YuuduuSheetRowData("ROWNO"))).Value = OutPutRowData(i)("KOTEIHIM")
                                Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("D" & Convert.ToString(YuuduuSheetRowData("ROWNO"))).Value = OutPutRowData(i)("KOTEIHID")
                                If Convert.ToString(OutPutRowData(i)("KAISU")) <> "0" Then
                                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Range("E" & Convert.ToString(YuuduuSheetRowData("ROWNO"))).Value = Convert.ToString(OutPutRowData(i)("KAISU"))
                                End If
                            Next
                        End If
                    Else
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & Convert.ToString(YuuduuSheetRowData("KAGAMIROW"))).Value = YuuduuSheetRowData("KAGAMINAME")
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("D" & Convert.ToString(YuuduuSheetRowData("KAGAMIROW"))).Value = 0
                    End If

                    'If DataExist = "1" AndAlso Convert.ToString(YuuduuSheetRowData("ROWDISPLAY")) = "1" Then
                    If Convert.ToString(YuuduuSheetRowData("ROWDISPLAY")) = "1" Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoYuu).Rows(CInt(YuuduuSheetRowData("ROWNO")) - 1).Hidden = False
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(CInt(YuuduuSheetRowData("KAGAMIROW")) - 1).Hidden = False
                        'If Convert.ToString(YuuduuSheetRowData("SYABAN")) = "324" Then
                        '    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(34).Hidden = True
                        'ElseIf Convert.ToString(YuuduuSheetRowData("SYABAN")) = "330" Then
                        '    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(35).Hidden = True
                        'ElseIf Convert.ToString(YuuduuSheetRowData("SYABAN")) = "359" Then
                        '    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(49).Hidden = True
                        'End If
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
    Private Sub EditDetailArea(ByVal pOutputRowData As DataRow(), ByVal row As Integer, ByRef FirstFLG As String)

        Try
            '届日(月)
            If FirstFLG = "1" Then
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_MONTH + Me.PrintOutputRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData(row)("TODOKEDATE").ToString), "MM") & "/"
                FirstFLG = "0"
            Else
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_MONTH + Me.PrintOutputRowIdx.ToString()).Value = ""
            End If
            '届日(日)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_DAY1 + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData(row)("TODOKEDATE").ToString), "dd"))
            '出荷日(日)
            If Not pOutputRowData(row)("SHUKADATE") Is DBNull.Value Then
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_DAY2 + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData(row)("SHUKADATE").ToString), "dd"))
            End If
            '車号
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_SHAGOU + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData(row)("GYOMUTANKNUM"))
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range(COL_SUURYOU + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData(row)("ZISSEKI")

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
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("B3:I3")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            Dim Fomula1 As String = "=COUNTIF($D$12:$D$" & pSheetRowData("MAXROW").ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ")+COUNTIF($I$12:$I$" & pSheetRowData("MAXROW").ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ")"
            Dim Fomula2 As String = "=SUMIF($D$12:$D$" & pSheetRowData("MAXROW").ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ",$E$12:$E$" & pSheetRowData("MAXROW").ToString & ")+SUMIF($I$12:$I$" & pSheetRowData("MAXROW").ToString & ",B" & Me.PrintOutputRowIdx.ToString() & ",$J$12:$J$" & pSheetRowData("MAXROW").ToString & ")"
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
    Private Sub EditTotalLastArea(ByVal pSheetRowData As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("B4:I4")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            Dim Fomula1 As String = "=SUM(D" & Me.PrintTotalFirstRowIdx.ToString() & ":D" & Me.PrintTotalLastRowIdx.ToString() & ")"
            Dim Fomula2 As String = "=SUM(F" & Me.PrintTotalFirstRowIdx.ToString() & ":G" & Me.PrintTotalLastRowIdx.ToString() & ")"
            Dim Fomula3 As String = "=SUM(H" & Me.PrintTotalFirstRowIdx.ToString() & ":I" & Me.PrintTotalLastRowIdx.ToString() & ")"

            '車数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula1
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula2
            '金額
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + Me.PrintOutputRowIdx.ToString()).Formula = Fomula3

            '出力件数加算
            Me.AddPrintRowCnt(1)
            '行クリア（テンプレートのごみをクリアしておく（行削除、行追加）
            '最終行の取得
            Dim lastRow As Integer = WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Row + WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Rows.Count - 1
            For i As Integer = Me.PrintOutputRowIdx To lastRow
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range(i.ToString + ":" + i.ToString).Delete()
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range(i.ToString + ":" + i.ToString).Insert()
            Next

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A6:F11")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '合計
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = pSheetRowData("TOTALNAME")
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
        SQLStr &= " , A01.KOTEIHIM "
        SQLStr &= " , IFNULL(A01.KOTEIHID,0) AS KOTEIHID "
        SQLStr &= " , IFNULL(A02.KAISU,0) AS KAISU "
        SQLStr &= " , IFNULL(A01.KOTEIHID,0) * IFNULL(A02.KAISU,0) AS GENGAKU "
        SQLStr &= " , A01.KOTEIHIM - IFNULL(A01.KOTEIHID,0) * IFNULL(A02.KAISU,0) AS GOUKEI "

        '-- FROM
        'SQLStr &= " FROM LNG.LNM0009_TNGKOTEIHI A01 "
        SQLStr &= " FROM LNG.LNM0007_FIXED A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN ( "
        SQLStr &= "           SELECT"
        SQLStr &= "               DATE_FORMAT(A12.TODOKEDATE, '%Y/%m/01') as TODOKEDATE"
        SQLStr &= "              ,A11.SYABAN     as SYABAN"
        SQLStr &= "              ,COUNT(A12.TORICODE) AS KAISU "
        'SQLStr &= "           FROM LNG.LNM0009_TNGKOTEIHI A11 "
        SQLStr &= "           FROM LNG.LNM0007_FIXED A11 "
        SQLStr &= "           INNER JOIN LNG.LNT0001_ZISSEKI A12 "
        SQLStr &= "               ON A12.TORICODE = '0175400000' "
        SQLStr &= "               AND A12.GYOMUTANKNUM = A11.SYABAN "
        SQLStr &= String.Format(" AND DATE_FORMAT(A12.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= "               AND A12.ZISSEKI <> 0 "
        SQLStr &= "               AND A12.DELFLG = '0' "
        SQLStr &= "           WHERE "
        'SQLStr &= String.Format("     A11.STYMD   <= '{0}' ", TaishoYm & "/01")
        'SQLStr &= String.Format(" AND A11.ENDYMD  >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format("     A11.TARGETYM  = '{0}' ", TaishoYm.Replace("/", ""))
        SQLStr &= "               AND A11.DELFLG   = '0' "
        SQLStr &= "           GROUP BY "
        SQLStr &= "               DATE_FORMAT(A12.TODOKEDATE, '%Y/%m/01') "
        SQLStr &= "              ,A11.SYABAN "
        SQLStr &= "           ) A02 "
        'SQLStr &= "           ON  A02.TODOKEDATE  >= A01.STYMD "
        'SQLStr &= "           AND A02.TODOKEDATE  <= A01.ENDYMD "
        SQLStr &= String.Format(" ON A02.TODOKEDATE >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format("AND A02.TODOKEDATE <= '{0}' ", Date.Parse(TaishoYm + "/" + "01").AddDays(-(Date.Parse(TaishoYm + "/" + "01").Day - 1)).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd"))
        SQLStr &= "              AND A02.SYABAN      = A01.SYABAN "

        '-- WHERE
        SQLStr &= " WHERE "
        'SQLStr &= String.Format("     A01.STYMD   <= '{0}' ", TaishoYm & "/01")
        'SQLStr &= String.Format(" AND A01.ENDYMD  >= '{0}' ", TaishoYm & "/01")
        SQLStr &= String.Format("     A01.TARGETYM  = '{0}' ", TaishoYm.Replace("/", ""))
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
