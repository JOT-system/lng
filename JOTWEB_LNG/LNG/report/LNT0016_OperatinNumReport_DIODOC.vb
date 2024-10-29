Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' レンタルコンテナ運用個数表作成クラス
''' </summary>
Public Class LNT0016_OperatinNumReport_DIODOC

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
    Private PrintNewPageRowCnt As Int32 = 0                                 '改頁時明細出力位置（行）　※初期値：0
    Private PrintPageBreakFlg As Boolean = True                             '改頁フラグ　※初期値：True
    Private PrintNameOutputFlg As Boolean = True                            '名称出力フラグ　※初期値：True（改頁後に名称を再度出力させるフラグ）
    Private PrintaddsheetFlg As Boolean = False                             'シート追加フラグ　※初期値：False
    Private PrintTotalOnlyFlg As Boolean = True                             '合計頁のみフラグ ※初期値：True
    Private PrintReportFileName As String = "レンタルコンテナ運用個数表"    'ファイル名

    Private Const REPORT_ID As String = "LNT0016"                           '帳票ID
    Private Const REPORT_NAME As String = "レンタルコンテナ運用個数表"      '帳票名
    Private Const PRINT_PAGE_BREAK_MAX_ROW As Int32 = 40                    '改頁行

    '合計個数クラス
    Private Class LNT0016_TotalDataClass
        Public Quantity0 As Int32 = 0     '前月末
        Public Quantity1 As Int32 = 0     '1日
        Public Quantity2 As Int32 = 0     '2日
        Public Quantity3 As Int32 = 0     '3日
        Public Quantity4 As Int32 = 0     '4日
        Public Quantity5 As Int32 = 0     '5日
        Public Quantity6 As Int32 = 0     '6日
        Public Quantity7 As Int32 = 0     '7日
        Public Quantity8 As Int32 = 0     '8日
        Public Quantity9 As Int32 = 0     '9日
        Public Quantity10 As Int32 = 0    '10日
        Public Quantity11 As Int32 = 0    '11日
        Public Quantity12 As Int32 = 0    '12日
        Public Quantity13 As Int32 = 0    '13日
        Public Quantity14 As Int32 = 0    '14日
        Public Quantity15 As Int32 = 0    '15日
        Public Quantity16 As Int32 = 0    '16日
        Public Quantity17 As Int32 = 0    '17日
        Public Quantity18 As Int32 = 0    '18日
        Public Quantity19 As Int32 = 0    '19日
        Public Quantity20 As Int32 = 0    '20日
        Public Quantity21 As Int32 = 0    '21日
        Public Quantity22 As Int32 = 0    '22日
        Public Quantity23 As Int32 = 0    '23日
        Public Quantity24 As Int32 = 0    '24日
        Public Quantity25 As Int32 = 0    '25日
        Public Quantity26 As Int32 = 0    '26日
        Public Quantity27 As Int32 = 0    '27日
        Public Quantity28 As Int32 = 0    '28日
        Public Quantity29 As Int32 = 0    '29日
        Public Quantity30 As Int32 = 0    '30日
        Public Quantity31 As Int32 = 0    '31日

        '金額クリア処理
        Public Sub Clear()
            Me.Quantity0 = 0
            Me.Quantity1 = 0
            Me.Quantity2 = 0
            Me.Quantity3 = 0
            Me.Quantity4 = 0
            Me.Quantity5 = 0
            Me.Quantity6 = 0
            Me.Quantity7 = 0
            Me.Quantity8 = 0
            Me.Quantity9 = 0
            Me.Quantity10 = 0
            Me.Quantity11 = 0
            Me.Quantity12 = 0
            Me.Quantity13 = 0
            Me.Quantity14 = 0
            Me.Quantity15 = 0
            Me.Quantity16 = 0
            Me.Quantity17 = 0
            Me.Quantity18 = 0
            Me.Quantity19 = 0
            Me.Quantity20 = 0
            Me.Quantity21 = 0
            Me.Quantity22 = 0
            Me.Quantity23 = 0
            Me.Quantity24 = 0
            Me.Quantity25 = 0
            Me.Quantity26 = 0
            Me.Quantity27 = 0
            Me.Quantity28 = 0
            Me.Quantity29 = 0
            Me.Quantity30 = 0
            Me.Quantity31 = 0
        End Sub

        '個数加算処理
        Public Sub CalcAdd(DataRowParam As DataRow)
            Me.Quantity0 += ExIntParse(DataRowParam("QUANTITY_0").ToString)
            Me.Quantity1 += ExIntParse(DataRowParam("QUANTITY_1").ToString)
            Me.Quantity2 += ExIntParse(DataRowParam("QUANTITY_2").ToString)
            Me.Quantity3 += ExIntParse(DataRowParam("QUANTITY_3").ToString)
            Me.Quantity4 += ExIntParse(DataRowParam("QUANTITY_4").ToString)
            Me.Quantity5 += ExIntParse(DataRowParam("QUANTITY_5").ToString)
            Me.Quantity6 += ExIntParse(DataRowParam("QUANTITY_6").ToString)
            Me.Quantity7 += ExIntParse(DataRowParam("QUANTITY_7").ToString)
            Me.Quantity8 += ExIntParse(DataRowParam("QUANTITY_8").ToString)
            Me.Quantity9 += ExIntParse(DataRowParam("QUANTITY_9").ToString)
            Me.Quantity10 += ExIntParse(DataRowParam("QUANTITY_10").ToString)
            Me.Quantity11 += ExIntParse(DataRowParam("QUANTITY_11").ToString)
            Me.Quantity12 += ExIntParse(DataRowParam("QUANTITY_12").ToString)
            Me.Quantity13 += ExIntParse(DataRowParam("QUANTITY_13").ToString)
            Me.Quantity14 += ExIntParse(DataRowParam("QUANTITY_14").ToString)
            Me.Quantity15 += ExIntParse(DataRowParam("QUANTITY_15").ToString)
            Me.Quantity16 += ExIntParse(DataRowParam("QUANTITY_16").ToString)
            Me.Quantity17 += ExIntParse(DataRowParam("QUANTITY_17").ToString)
            Me.Quantity18 += ExIntParse(DataRowParam("QUANTITY_18").ToString)
            Me.Quantity19 += ExIntParse(DataRowParam("QUANTITY_19").ToString)
            Me.Quantity20 += ExIntParse(DataRowParam("QUANTITY_20").ToString)
            Me.Quantity21 += ExIntParse(DataRowParam("QUANTITY_21").ToString)
            Me.Quantity22 += ExIntParse(DataRowParam("QUANTITY_22").ToString)
            Me.Quantity23 += ExIntParse(DataRowParam("QUANTITY_23").ToString)
            Me.Quantity24 += ExIntParse(DataRowParam("QUANTITY_24").ToString)
            Me.Quantity25 += ExIntParse(DataRowParam("QUANTITY_25").ToString)
            Me.Quantity26 += ExIntParse(DataRowParam("QUANTITY_26").ToString)
            Me.Quantity27 += ExIntParse(DataRowParam("QUANTITY_27").ToString)
            Me.Quantity28 += ExIntParse(DataRowParam("QUANTITY_28").ToString)
            Me.Quantity29 += ExIntParse(DataRowParam("QUANTITY_29").ToString)
            Me.Quantity30 += ExIntParse(DataRowParam("QUANTITY_30").ToString)
            Me.Quantity31 += ExIntParse(DataRowParam("QUANTITY_31").ToString)
        End Sub

        '売却・冷却個数加算処理
        Public Sub ExclusionAdd(DataRowParam As DataRow)
            Me.Quantity0 = 0
            If Not DataRowParam("WEEKDAY_1").ToString = "" Then
                Me.Quantity1 += ExclusionData(DataRowParam("QUANTITY_0").ToString, DataRowParam("QUANTITY_1").ToString)
            End If
            If Not DataRowParam("WEEKDAY_2").ToString = "" Then
                Me.Quantity2 += ExclusionData(DataRowParam("QUANTITY_1").ToString, DataRowParam("QUANTITY_2").ToString)
            End If
            If Not DataRowParam("WEEKDAY_3").ToString = "" Then
                Me.Quantity3 += ExclusionData(DataRowParam("QUANTITY_2").ToString, DataRowParam("QUANTITY_3").ToString)
            End If
            If Not DataRowParam("WEEKDAY_4").ToString = "" Then
                Me.Quantity4 += ExclusionData(DataRowParam("QUANTITY_3").ToString, DataRowParam("QUANTITY_4").ToString)
            End If
            If Not DataRowParam("WEEKDAY_5").ToString = "" Then
                Me.Quantity5 += ExclusionData(DataRowParam("QUANTITY_4").ToString, DataRowParam("QUANTITY_5").ToString)
            End If
            If Not DataRowParam("WEEKDAY_6").ToString = "" Then
                Me.Quantity6 += ExclusionData(DataRowParam("QUANTITY_5").ToString, DataRowParam("QUANTITY_6").ToString)
            End If
            If Not DataRowParam("WEEKDAY_7").ToString = "" Then
                Me.Quantity7 += ExclusionData(DataRowParam("QUANTITY_6").ToString, DataRowParam("QUANTITY_7").ToString)
            End If
            If Not DataRowParam("WEEKDAY_8").ToString = "" Then
                Me.Quantity8 += ExclusionData(DataRowParam("QUANTITY_7").ToString, DataRowParam("QUANTITY_8").ToString)
            End If
            If Not DataRowParam("WEEKDAY_9").ToString = "" Then
                Me.Quantity9 += ExclusionData(DataRowParam("QUANTITY_8").ToString, DataRowParam("QUANTITY_9").ToString)
            End If
            If Not DataRowParam("WEEKDAY_10").ToString = "" Then
                Me.Quantity10 += ExclusionData(DataRowParam("QUANTITY_9").ToString, DataRowParam("QUANTITY_10").ToString)
            End If
            If Not DataRowParam("WEEKDAY_11").ToString = "" Then
                Me.Quantity11 += ExclusionData(DataRowParam("QUANTITY_10").ToString, DataRowParam("QUANTITY_11").ToString)
            End If
            If Not DataRowParam("WEEKDAY_12").ToString = "" Then
                Me.Quantity12 += ExclusionData(DataRowParam("QUANTITY_11").ToString, DataRowParam("QUANTITY_12").ToString)
            End If
            If Not DataRowParam("WEEKDAY_13").ToString = "" Then
                Me.Quantity13 += ExclusionData(DataRowParam("QUANTITY_12").ToString, DataRowParam("QUANTITY_13").ToString)
            End If
            If Not DataRowParam("WEEKDAY_14").ToString = "" Then
                Me.Quantity14 += ExclusionData(DataRowParam("QUANTITY_13").ToString, DataRowParam("QUANTITY_14").ToString)
            End If
            If Not DataRowParam("WEEKDAY_15").ToString = "" Then
                Me.Quantity15 += ExclusionData(DataRowParam("QUANTITY_14").ToString, DataRowParam("QUANTITY_15").ToString)
            End If
            If Not DataRowParam("WEEKDAY_16").ToString = "" Then
                Me.Quantity16 += ExclusionData(DataRowParam("QUANTITY_15").ToString, DataRowParam("QUANTITY_16").ToString)
            End If
            If Not DataRowParam("WEEKDAY_17").ToString = "" Then
                Me.Quantity17 += ExclusionData(DataRowParam("QUANTITY_16").ToString, DataRowParam("QUANTITY_17").ToString)
            End If
            If Not DataRowParam("WEEKDAY_18").ToString = "" Then
                Me.Quantity18 += ExclusionData(DataRowParam("QUANTITY_17").ToString, DataRowParam("QUANTITY_18").ToString)
            End If
            If Not DataRowParam("WEEKDAY_19").ToString = "" Then
                Me.Quantity19 += ExclusionData(DataRowParam("QUANTITY_18").ToString, DataRowParam("QUANTITY_19").ToString)
            End If
            If Not DataRowParam("WEEKDAY_20").ToString = "" Then
                Me.Quantity20 += ExclusionData(DataRowParam("QUANTITY_19").ToString, DataRowParam("QUANTITY_20").ToString)
            End If
            If Not DataRowParam("WEEKDAY_21").ToString = "" Then
                Me.Quantity21 += ExclusionData(DataRowParam("QUANTITY_20").ToString, DataRowParam("QUANTITY_21").ToString)
            End If
            If Not DataRowParam("WEEKDAY_22").ToString = "" Then
                Me.Quantity22 += ExclusionData(DataRowParam("QUANTITY_21").ToString, DataRowParam("QUANTITY_22").ToString)
            End If
            If Not DataRowParam("WEEKDAY_23").ToString = "" Then
                Me.Quantity23 += ExclusionData(DataRowParam("QUANTITY_22").ToString, DataRowParam("QUANTITY_23").ToString)
            End If
            If Not DataRowParam("WEEKDAY_24").ToString = "" Then
                Me.Quantity24 += ExclusionData(DataRowParam("QUANTITY_23").ToString, DataRowParam("QUANTITY_24").ToString)
            End If
            If Not DataRowParam("WEEKDAY_25").ToString = "" Then
                Me.Quantity25 += ExclusionData(DataRowParam("QUANTITY_24").ToString, DataRowParam("QUANTITY_25").ToString)
            End If
            If Not DataRowParam("WEEKDAY_26").ToString = "" Then
                Me.Quantity26 += ExclusionData(DataRowParam("QUANTITY_25").ToString, DataRowParam("QUANTITY_26").ToString)
            End If
            If Not DataRowParam("WEEKDAY_27").ToString = "" Then
                Me.Quantity27 += ExclusionData(DataRowParam("QUANTITY_26").ToString, DataRowParam("QUANTITY_27").ToString)
            End If
            If Not DataRowParam("WEEKDAY_28").ToString = "" Then
                Me.Quantity28 += ExclusionData(DataRowParam("QUANTITY_27").ToString, DataRowParam("QUANTITY_28").ToString)
            End If
            If Not DataRowParam("WEEKDAY_29").ToString = "" Then
                Me.Quantity29 += ExclusionData(DataRowParam("QUANTITY_28").ToString, DataRowParam("QUANTITY_29").ToString)
            End If
            If Not DataRowParam("WEEKDAY_30").ToString = "" Then
                Me.Quantity30 += ExclusionData(DataRowParam("QUANTITY_29").ToString, DataRowParam("QUANTITY_30").ToString)
            End If
            If Not DataRowParam("WEEKDAY_31").ToString = "" Then
                Me.Quantity31 += ExclusionData(DataRowParam("QUANTITY_30").ToString, DataRowParam("QUANTITY_31").ToString)
            End If
        End Sub

        Private Function ExIntParse(StrVal As String) As Int32
            If Not Int32.TryParse(StrVal, 10) Then
                Return 0
            End If

            Return Int32.Parse(StrVal)

        End Function

        ''' <summary>
        ''' 売却除外数計算
        ''' </summary>
        Public Shared Function ExclusionData(
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

            '前日数量が当日数量以下の場合0を入れる
            If IntQUANTITY_1 <= IntQUANTITY_2 Then
                Return 0
            End If

            Return IntQUANTITY_1 - IntQUANTITY_2

        End Function
    End Class

    '合計個数クラス保持変数
    Private NormarlNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private NewNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private NormarlNewNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private SpotLeaseNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private HokkaidoNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private TouhokuNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private KantouNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private TyubuNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private KansaiNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private KyusyuNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private BranchNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private PossessionNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()
    Private ExclusionNumTotal As LNT0016_TotalDataClass = New LNT0016_TotalDataClass()

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, type As String)
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
    Public Function CreateExcelPrintData(MaxPage As String, CtnClass As String) As String
        Dim TmpFileName As String = Me.PrintReportFileName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim TmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, TmpFileName)
        Dim RetByte() As Byte
        Dim INVOICEORGCODE As String = ""
        Dim MAX_PAGE As String = MaxPage

        Try
            '初期化処理
            Dim OldRowData As DataRow = Nothing     'ブレイク判定用（直前の明細データ保持）
            Dim WeekRowData As DataRow = Nothing

            '出力データループ
            For Each OutputRowData As DataRow In PrintData.Rows

                '合計行のみ選択時1度だけ行う
                If Me.PrintTotalOnlyFlg Then

                    '行数による改頁判定
                    If Me.PrintPageRowCnt > PRINT_PAGE_BREAK_MAX_ROW Then
                        '明細出力行を次頁に合わせる
                        Me.PrintNewPageRowCnt += 40
                        Me.PrintPageBreakFlg = True
                    End If

                    'シート追加
                    If Not Me.PrintaddsheetFlg Then
                        WeekRowData = OutputRowData
                        TrySetExcelWorkSheet(PrintOutputRowIdx, "運用個数表".ToString, PrintPageNum, "レンタルコンテナ運用個数表") 'OutputRowData("BIGCTNNM")
                        Me.PrintaddsheetFlg = True
                    End If

                    '合計頁のみ選択した場合ヘッダ出力を行わない
                    If Not CtnClass = "99" Then

                        '改頁の場合、ヘッダ出力（初回出力も含む）
                        If Me.PrintPageBreakFlg Then
                            '〇ヘッダー出力
                            Me.EditHeaderArea(OutputRowData, MAX_PAGE)
                            Me.EditDetailAreaFormat()
                            Me.EditWeekData(Me.PrintOutputRowIdx, OutputRowData)
                            Me.PrintPageBreakFlg = False
                        End If

                    Else

                        Me.PrintTotalOnlyFlg = False

                    End If

                End If

                '個数出力
                Me.EditRowData(Me.PrintOutputRowIdx, OutputRowData, CtnClass)

                '前回出力明細データ保持
                OldRowData = OutputRowData

            Next

            If CtnClass = "88" Or CtnClass = "" Or CtnClass = "99" Then
                '合計頁作成
                EditTotalData(MAX_PAGE, OldRowData, WeekRowData)
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
        ByVal pOutputRowData As DataRow,
        ByVal maxPage As String,
        Optional ByVal TotalFlg As String = "0"
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '初回ページは設定しない
            If Not Me.PrintNewPageRowCnt = 0 Then
                '印刷範囲設定
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:R{0}", Me.PrintOutputRowIdx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            'ヘッダー行コピー
            srcRange = Me.WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:R3")
            destRange = Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '対象年月セル編集
            Dim Nowdt As DateTime = DateTime.Now
            Dim WkNowDateStr As String = Nowdt.ToString("yyyy.MM.dd")
            Dim WkNowTimeStr As String = Nowdt.ToString("HH.mm")
            Dim toDate As Date

            If pOutputRowData("TARGETYM").ToString = Format(Date.Now, "yyyy/MM") Then
                toDate = Date.Now
            Else
                '月初日をセット
                toDate = New Date(CType(Left(pOutputRowData("TARGETYM").ToString, 4), Int32), CType(Right(pOutputRowData("TARGETYM").ToString, 2), Int32), 1)
                '月末日の取得
                toDate = toDate.AddMonths(1).AddDays(-1)
            End If

            Dim WkTargetDateStr As String = toDate.ToString("yyyy 年 MM 月 dd 日")


            Me.PrintPageRowCnt = 1
            '〇処理日
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + Me.PrintOutputRowIdx.ToString()).Value = WkNowDateStr
            '〇処理時間
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + Me.PrintOutputRowIdx.ToString()).Value = WkNowTimeStr
            '〇頁数
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + Me.PrintOutputRowIdx.ToString()).Value = Me.PrintPageNum.ToString + "/" + maxPage

            '出力件数加算
            Me.AddPrintRowCnt(1)

            '〇帳票タイトル
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = REPORT_NAME
            '◯対象日付
            Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + Me.PrintOutputRowIdx.ToString()).Value = WkTargetDateStr

            '出力件数加算
            Me.AddPrintRowCnt(1)

            If TotalFlg = "0" Then
                '〇コンテナ種別
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("BIGCTNNM")
            ElseIf TotalFlg = "1" Then
                '合計頁の場合
                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = "合計"
            End If
            '出力件数加算
            Me.AddPrintRowCnt(1)

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
    Private Sub EditDetailAreaFormat()
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing

        Try

            '明細枠コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A4:R40")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" + Me.PrintOutputRowIdx.ToString())
            srcRange.Copy(destRange)

            '出力件数加算
            Me.AddPrintRowCnt(1)

        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 曜日出力
    ''' </summary>
    Private Sub EditWeekData(
        ByVal pOldRowData As Int32,
        ByVal pOutputRowData As DataRow
     )

        Dim WkTaxableAmount As Long = 0
        Dim ConsumptionTax As Long = 0


        Try

            '1日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_1")
            '2日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_2")
            '3日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_3")
            '4日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_4")
            '5日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_5")
            '6日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_6")
            '7日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_7")
            '8日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_8")
            '9日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_9")
            '10日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_10")
            '11日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_11")
            '12日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_12")
            '13日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_13")
            '14日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_14")
            '15日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData.ToString()).Value = pOutputRowData("WEEKDAY_15")

            '16日以降の行数
            Dim pOldRowData2 As Int32 = pOldRowData + 16

            '16日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_16")
            '17日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_17")
            '18日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_18")
            '19日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_19")
            '20日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_20")
            '21日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_21")
            '22日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_22")
            '23日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_23")
            '24日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_24")
            '25日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_25")
            '26日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_26")
            '27日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_27")
            '28日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_28")
            '29日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_29")
            '30日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_30")
            '31日曜日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData2.ToString()).Value = pOutputRowData("WEEKDAY_31")

            '出力件数加算
            AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票明細出力
    ''' </summary>
    Private Sub EditRowData(
        ByVal pOldRowData As Int32,
        ByVal pOutputRowData As DataRow,
        ByVal CtnClass As String
     )

        Dim WkTaxableAmount As Long = 0
        Dim ConsumptionTax As Long = 0
        Dim LastRowFlg As Boolean = False
        Dim RcdCatFlg As Boolean = False

        Select Case pOutputRowData("RCDCATCD").ToString
            Case "01" '通常
                pOldRowData = 6 + Me.PrintNewPageRowCnt
                NormarlNumTotal.CalcAdd(pOutputRowData)
            Case "02" '新規投入
                pOldRowData = 7 + Me.PrintNewPageRowCnt
                NewNumTotal.CalcAdd(pOutputRowData)
            Case "03" '通常・新規投入（計）
                pOldRowData = 8 + Me.PrintNewPageRowCnt
                NormarlNewNumTotal.CalcAdd(pOutputRowData)
            Case "04" 'スポットリース個数
                pOldRowData = 9 + Me.PrintNewPageRowCnt
                SpotLeaseNumTotal.CalcAdd(pOutputRowData)
                'レコード種別が必要ない行
                'RcdCatFlg = True
            Case "05" '運用除外（北海道）
                pOldRowData = 10 + Me.PrintNewPageRowCnt
                HokkaidoNumTotal.CalcAdd(pOutputRowData)
            Case "06" '運用除外（東北）
                pOldRowData = 11 + Me.PrintNewPageRowCnt
                TouhokuNumTotal.CalcAdd(pOutputRowData)
            Case "09" '運用除外（関東）
                pOldRowData = 12 + Me.PrintNewPageRowCnt
                KantouNumTotal.CalcAdd(pOutputRowData)
            Case "10" '運用除外（中部）
                pOldRowData = 13 + Me.PrintNewPageRowCnt
                TyubuNumTotal.CalcAdd(pOutputRowData)
            Case "11" '運用除外（関西）
                pOldRowData = 14 + Me.PrintNewPageRowCnt
                KansaiNumTotal.CalcAdd(pOutputRowData)
            Case "12" '運用除外（九州）
                pOldRowData = 15 + Me.PrintNewPageRowCnt
                KyusyuNumTotal.CalcAdd(pOutputRowData)
            Case "13" '運用除外（小計）
                pOldRowData = 16 + Me.PrintNewPageRowCnt
                BranchNumTotal.CalcAdd(pOutputRowData)
            Case "14" '保有個数
                pOldRowData = 17 + Me.PrintNewPageRowCnt
                PossessionNumTotal.CalcAdd(pOutputRowData)
            '2024/08/23 星ADD START
            Case "15" '売　却　・　除　却　個　数
                pOldRowData = 18 + Me.PrintNewPageRowCnt
                ExclusionNumTotal.CalcAdd(pOutputRowData)
                '2024/08/23 星ADD END
                'ページの最終行
                LastRowFlg = True
        End Select

        Try
            If Not CtnClass = "99" Then

                '前月末
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_0")
                '1日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_1")
                '2日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_2")
                '3日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_3")
                '4日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_4")
                '5日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_5")
                '6日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_6")
                '7日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_7")
                '8日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_8")
                '9日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_9")
                '10日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_10")
                '11日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_11")
                '12日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_12")
                '13日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_13")
                '14日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_14")
                '15日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData.ToString()).Value = pOutputRowData("QUANTITY_15")

                '16日以降の行数
                Dim pOldRowData2 As Int32 = pOldRowData + 16

                '16日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_16")
                '17日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_17")
                '18日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_18")
                '19日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_19")
                '20日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_20")
                '21日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_21")
                '22日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_22")
                '23日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_23")
                '24日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_24")
                '25日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_25")
                '26日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_26")
                '27日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_27")
                '28日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_28")
                '29日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_29")
                '30日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_30")
                '31日
                WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData2.ToString()).Value = pOutputRowData("QUANTITY_31")

                'ページの最終行だった場合
                If LastRowFlg Then
                    '売却除外数出力
                    'EditExclusionData(pOldRowData, pOutputRowData, CtnClass) '2024/08/23 星DEL
                    '※次頁に合わせる
                    AddPrintRowCnt(35)
                End If

            End If

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 売却除外数出力
    ''' </summary>
    Private Sub EditExclusionData(
                ByVal pOldRowData As Int32,
                ByVal pOutputRowData As DataRow,
                ByVal CtnClass As String
     )

        '行を合わせる
        pOldRowData += 1

        '1日
        If Not pOutputRowData("WEEKDAY_1").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_0").ToString, pOutputRowData("QUANTITY_1").ToString)
        End If
        '2日
        If Not pOutputRowData("WEEKDAY_2").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_1").ToString, pOutputRowData("QUANTITY_2").ToString)
        End If
        '3日
        If Not pOutputRowData("WEEKDAY_3").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_2").ToString, pOutputRowData("QUANTITY_3").ToString)
        End If
        '4日
        If Not pOutputRowData("WEEKDAY_4").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_3").ToString, pOutputRowData("QUANTITY_4").ToString)
        End If
        '5日
        If Not pOutputRowData("WEEKDAY_5").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_4").ToString, pOutputRowData("QUANTITY_5").ToString)
        End If
        '6日
        If Not pOutputRowData("WEEKDAY_6").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_5").ToString, pOutputRowData("QUANTITY_6").ToString)
        End If
        '7日
        If Not pOutputRowData("WEEKDAY_7").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_6").ToString, pOutputRowData("QUANTITY_7").ToString)
        End If
        '8日
        If Not pOutputRowData("WEEKDAY_8").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_7").ToString, pOutputRowData("QUANTITY_8").ToString)
        End If
        '9日
        If Not pOutputRowData("WEEKDAY_9").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_8").ToString, pOutputRowData("QUANTITY_9").ToString)
        End If
        '10日
        If Not pOutputRowData("WEEKDAY_10").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_9").ToString, pOutputRowData("QUANTITY_10").ToString)
        End If
        '11日
        If Not pOutputRowData("WEEKDAY_11").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_10").ToString, pOutputRowData("QUANTITY_11").ToString)
        End If
        '12日
        If Not pOutputRowData("WEEKDAY_12").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_11").ToString, pOutputRowData("QUANTITY_12").ToString)
        End If
        '13日
        If Not pOutputRowData("WEEKDAY_13").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_12").ToString, pOutputRowData("QUANTITY_13").ToString)
        End If
        '14日
        If Not pOutputRowData("WEEKDAY_14").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_13").ToString, pOutputRowData("QUANTITY_14").ToString)
        End If
        '15日
        If Not pOutputRowData("WEEKDAY_15").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_14").ToString, pOutputRowData("QUANTITY_15").ToString)
        End If

        '16日以降の行数
        Dim pOldRowData2 As Int32 = pOldRowData + 16

        If Not pOutputRowData("WEEKDAY_16").ToString = "" Then
            '16日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_15").ToString, pOutputRowData("QUANTITY_16").ToString)
        End If
        '17日
        If Not pOutputRowData("WEEKDAY_17").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_16").ToString, pOutputRowData("QUANTITY_17").ToString)
        End If
        '18日
        If Not pOutputRowData("WEEKDAY_18").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_17").ToString, pOutputRowData("QUANTITY_18").ToString)
        End If
        '19日
        If Not pOutputRowData("WEEKDAY_19").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_18").ToString, pOutputRowData("QUANTITY_19").ToString)
        End If
        '20日
        If Not pOutputRowData("WEEKDAY_20").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_19").ToString, pOutputRowData("QUANTITY_20").ToString)
        End If
        '21日
        If Not pOutputRowData("WEEKDAY_21").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_20").ToString, pOutputRowData("QUANTITY_21").ToString)
        End If
        '22日
        If Not pOutputRowData("WEEKDAY_22").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_21").ToString, pOutputRowData("QUANTITY_22").ToString)
        End If
        '23日
        If Not pOutputRowData("WEEKDAY_23").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_22").ToString, pOutputRowData("QUANTITY_23").ToString)
        End If
        '24日
        If Not pOutputRowData("WEEKDAY_24").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_23").ToString, pOutputRowData("QUANTITY_24").ToString)
        End If
        '25日
        If Not pOutputRowData("WEEKDAY_25").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_24").ToString, pOutputRowData("QUANTITY_25").ToString)
        End If
        '26日
        If Not pOutputRowData("WEEKDAY_26").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_25").ToString, pOutputRowData("QUANTITY_26").ToString)
        End If
        '27日
        If Not pOutputRowData("WEEKDAY_27").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_26").ToString, pOutputRowData("QUANTITY_27").ToString)
        End If
        '28日
        If Not pOutputRowData("WEEKDAY_28").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_27").ToString, pOutputRowData("QUANTITY_28").ToString)
        End If
        '29日
        If Not pOutputRowData("WEEKDAY_29").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_28").ToString, pOutputRowData("QUANTITY_29").ToString)
        End If
        '30日
        If Not pOutputRowData("WEEKDAY_30").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_29").ToString, pOutputRowData("QUANTITY_30").ToString)
        End If
        '31日
        If Not pOutputRowData("WEEKDAY_31").ToString = "" Then
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData2.ToString()).Value = LNT0016_TotalDataClass.ExclusionData(pOutputRowData("QUANTITY_30").ToString, pOutputRowData("QUANTITY_31").ToString)
        End If

    End Sub


    ''' <summary>
    ''' 合計明細
    ''' </summary>
    Private Sub EditTotalData(
                 ByVal MAX_PAGE As String,
                 ByVal OldRowData As DataRow,
                 ByVal WeekRowData As DataRow
     )
        '〇ヘッダー出力
        Me.EditHeaderArea(OldRowData, MAX_PAGE, "1")
        Me.EditDetailAreaFormat()

        '曜日出力
        EditWeekData(Me.PrintOutputRowIdx, WeekRowData)

        '合計明細出力 
        TotalEditRowData(Me.PrintOutputRowIdx, Me.NormarlNumTotal, True)    '通常
        TotalEditRowData(Me.PrintOutputRowIdx, Me.NewNumTotal, True)        '新規投入
        TotalEditRowData(Me.PrintOutputRowIdx, Me.NormarlNewNumTotal, True) '通常・新規投入（計）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.SpotLeaseNumTotal, False) 'スポットリース個数
        TotalEditRowData(Me.PrintOutputRowIdx, Me.HokkaidoNumTotal, True)   '運用除外（北海道）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.TouhokuNumTotal, True)    '運用除外（東北）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.KantouNumTotal, True)     '運用除外（関東）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.TyubuNumTotal, True)      '運用除外（中部）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.KansaiNumTotal, True)     '運用除外（関西）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.KyusyuNumTotal, True)     '運用除外（九州）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.BranchNumTotal, True)     '運用除外（小計）
        TotalEditRowData(Me.PrintOutputRowIdx, Me.PossessionNumTotal, True) '保有個数
        TotalEditRowData(Me.PrintOutputRowIdx, Me.ExclusionNumTotal, False) '売却除外数


    End Sub

    ''' <summary>
    ''' 合計明細出力
    ''' </summary>
    Private Sub TotalEditRowData(
        ByVal pOldRowData As Int32,
        ByVal TotalOutputRowData As LNT0016_TotalDataClass,
        ByVal RcdCatFlg As Boolean
     )

        Dim WkTaxableAmount As Long = 0
        Dim ConsumptionTax As Long = 0
        Dim LastRowFlg As Boolean = False

        Try

            '前月末
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity0
            '1日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity1
            '2日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity2
            '3日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity3
            '4日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity4
            '5日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity5
            '6日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity6
            '7日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity7
            '8日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity8
            '9日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity9
            '10日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity10
            '11日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity11
            '12日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity12
            '13日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity13
            '14日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity14
            '15日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData.ToString()).Value = TotalOutputRowData.Quantity15

            '16日以降の行数
            Dim pOldRowData2 As Int32 = pOldRowData + 16

            '16日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("C" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity16
            '17日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("D" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity17
            '18日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity18
            '19日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("F" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity19
            '20日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("G" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity20
            '21日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("H" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity21
            '22日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("I" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity22
            '23日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("J" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity23
            '24日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("K" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity24
            '25日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("L" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity25
            '26日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("M" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity26
            '27日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("N" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity27
            '28日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("O" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity28
            '29日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("P" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity29
            '30日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("Q" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity30
            '31日
            WW_Workbook.Worksheets(Me.WW_SheetNo).Range("R" + pOldRowData2.ToString()).Value = TotalOutputRowData.Quantity31

            '行数加算
            AddPrintRowCnt(1)

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

End Class
