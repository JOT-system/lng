Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputCENERGY_ELNESS
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    'Private WW_SheetNoSKKoteihi As Integer = 0
    'Private WW_SheetNoUnchin As Integer = 0
    Private WW_SheetNoCalendar As Integer = 0
    Private WW_SheetNoMaster As Integer = 0
    Private WW_SheetNoEvertMonth As Integer = 0
    Private WW_SheetNoTitle As Integer = 0
    Private WW_SheetStandards As Integer() = {0, 0, 0}
    Private WW_DicCenergyList As New Dictionary(Of String, String)
    Private WW_DicElNessList As New Dictionary(Of String, String)
    Private WW_ArrSheetNoCenergy As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// シーエナジー(シート)用
    Private WW_ArrSheetNoElNess As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}    '// エルネス　　(シート)用

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
    Private PrintTogouSprate As DataTable
    Private PrintCalendarData As DataTable
    Private PrintHolidayRateData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private OutputOrgCode As String = ""
    Private OutputFileName As String = ""
    Private calcZissekiNumber As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <param name="printDataClass">帳票データ</param>
    ''' <param name="printTankDataClass"></param>
    ''' <param name="printKoteihiDataClass">固定費マスタ</param>
    ''' <param name="printCalendarDataClass">カレンダーマスタ</param>
    ''' <param name="dicCenergyList">業務車番格納(３〇〇)</param>
    ''' <param name="dicElNessList"> 業務車番格納(６〇〇)</param>
    ''' <param name="printHolidayRateDataClass">休日割増単価マスタ</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printCalendarDataClass As DataTable,
                   dicCenergyList As Dictionary(Of String, String), dicElNessList As Dictionary(Of String, String),
                   Optional ByVal printTogouSprateDataClass As DataTable = Nothing,
                   Optional ByVal printHolidayRateDataClass As DataTable = Nothing,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal calcNumber As Integer = 1,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTankData = printTankDataClass
            Me.PrintKoteihiData = printKoteihiDataClass
            Me.PrintCalendarData = printCalendarDataClass
            'Me.PrintSKKoteichiData = printSKKoteichiDataClass
            Me.PrintTogouSprate = printTogouSprateDataClass
            Me.PrintHolidayRateData = printHolidayRateDataClass
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputOrgCode = orgCode
            Me.OutputFileName = outputFileName
            Me.calcZissekiNumber = calcNumber
            ReDim WW_ArrSheetNoCenergy(dicCenergyList.Count - 1)
            ReDim WW_ArrSheetNoElNess(dicElNessList.Count - 1)

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

            '〇シーエナジー(シート)用
            For Each dic In dicCenergyList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = indexKey Then
                        WW_DicCenergyList.Add(indexKey, i.ToString())
                        Exit For
                    End If
                Next
            Next
            '〇エルネス(シート)用
            For Each dic In dicElNessList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = indexKey Then
                        WW_DicElNessList.Add(indexKey, i.ToString())
                        Exit For
                    End If
                Next
            Next

            Dim j As Integer() = {0, 0, 0, 0, 0}
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "入力表" Then

                ElseIf WW_Workbook.Worksheets(i).Name = "毎月更新" Then
                    '〇共通(シート[毎月更新])
                    WW_SheetNoEvertMonth = i
                ElseIf WW_Workbook.Worksheets(i).Name = "表題" Then
                    '〇共通(シート[表題])
                    WW_SheetNoTitle = i
                ElseIf WW_Workbook.Worksheets(i).Name = "301" Then
                    '〇シーエナジー(シート[届先別])
                    WW_SheetNoCalendar = i
                ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                    '〇共通(シート[ﾏｽﾀ])
                    WW_SheetNoMaster = i
                ElseIf WW_Workbook.Worksheets(i).Name = "基準(川越・知多)" Then
                    '〇共通(シート[基準(川越・知多)])
                    WW_SheetStandards(0) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "基準(上越)" Then
                    '〇共通(シート[基準(上越)])
                    WW_SheetStandards(1) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "基準" Then
                    '〇共通(シート[基準])
                    WW_SheetStandards(2) = i

                End If
            Next

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

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            '◯(固定費・単価)の設定
            EditKoteihiTankaArea()
            '***** TODO処理 ここまで *****
            '★ [毎月更新]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoEvertMonth).Visible = Visibility.Hidden
            '★ [表題]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoTitle).Visible = Visibility.Hidden
            '★ [ﾏｽﾀ]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoMaster).Visible = Visibility.Hidden
            ''★ [固定値]シート非表示
            'For Each i In WW_ArrSheetNoKoteichi
            '    WW_Workbook.Worksheets(i).Visible = Visibility.Hidden
            'Next

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
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Try
            '◯ 年月
            WW_Workbook.Worksheets(WW_SheetNoMaster).Range("A1").Value = Integer.Parse(Me.TaishoYYYY)
            WW_Workbook.Worksheets(WW_SheetNoMaster).Range("A2").Value = Integer.Parse(Me.TaishoMM)
            '〇 日(末日)
            Dim lastDay As String = Me.TaishoYYYY + "/" + Me.TaishoMM + "/01"
            lastDay = Date.Parse(lastDay).AddMonths(1).AddDays(-1).ToString("dd")
            WW_Workbook.Worksheets(WW_SheetNoMaster).Range("A3").Value = Integer.Parse(lastDay)

            '〇カレンダー設定
            Dim iCalendarLine As Integer = 6
            For Each PrintCalendarDatarow As DataRow In PrintCalendarData.Rows
                If PrintCalendarDatarow("WORKINGDAY").ToString() <> "0" Then
                    '★シーエナジー用
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range("M" + iCalendarLine.ToString("000")).Value = PrintCalendarDatarow("YMD")
                    '★エルネス用
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range("N" + iCalendarLine.ToString("000")).Value = PrintCalendarDatarow("YMD")
                    iCalendarLine += 1
                End If
            Next

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea()
        Try
            '〇[３〇〇]シート設定
            For Each DicCenergyList In WW_DicCenergyList
                Dim iCnt As Integer = 0
                Dim iSheetNo As Integer = CInt(DicCenergyList.Value)
                Dim condition As String = "GYOMUTANKNUM='{0}'"
                'Dim condition As String = "CENERGYELNESS_SHUKACODE<>'' AND GYOMUTANKNUM='{0}'"
                condition = String.Format(condition, DicCenergyList.Key)
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE, TODOKEDATE, CENERGYELNESS_SHUKACODE, CENERGYELNESS_TODOKECODE")
                    If PrintDatarow("CENERGYELNESS_SHUKACODE").ToString() = "" Then Continue For
                    Dim iStartCnt As Integer = CInt(PrintDatarow("SETSTARTLINE"))
                    iStartCnt = iStartCnt + iCnt
                    'If PrintDatarow("TODOKECODE").ToString() <> DicCenergyList.Key Then
                    '    Continue For
                    'End If
                    '◯ 配送日
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL01").ToString() + iStartCnt.ToString()).Value = Date.Parse(PrintDatarow("TODOKEDATE").ToString())
                    '◯ コード(出荷基地)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL02").ToString() + iStartCnt.ToString()).Value = CInt(PrintDatarow("CENERGYELNESS_SHUKACODE").ToString())
                    '◯ コード(届　先)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL03").ToString() + iStartCnt.ToString()).Value = CInt(PrintDatarow("CENERGYELNESS_TODOKECODE").ToString())
                    '◯ コード(計量№)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL04").ToString() + iStartCnt.ToString()).Value = ""
                    '◯ コード(出荷量(T))
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL05").ToString() + iStartCnt.ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                    iCnt += 1
                Next
            Next

            '〇[６〇〇]シート設定
            For Each DicElNessList In WW_DicElNessList
                Dim iCnt As Integer = 0
                Dim iSheetNo As Integer = CInt(DicElNessList.Value)
                Dim condition As String = "GYOMUTANKNUM='{0}'"
                'Dim condition As String = "CENERGYELNESS_SHUKACODE<>'' AND GYOMUTANKNUM='{0}'"
                condition = String.Format(condition, DicElNessList.Key)
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE, TODOKEDATE, CENERGYELNESS_SHUKACODE, CENERGYELNESS_TODOKECODE")
                    If PrintDatarow("CENERGYELNESS_SHUKACODE").ToString() = "" Then Continue For
                    Dim iStartCnt As Integer = CInt(PrintDatarow("SETSTARTLINE"))
                    iStartCnt = iStartCnt + iCnt
                    'If PrintDatarow("TODOKECODE").ToString() <> DicCenergyList.Key Then
                    '    Continue For
                    'End If
                    '◯ 配送日
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL01").ToString() + iStartCnt.ToString()).Value = Date.Parse(PrintDatarow("TODOKEDATE").ToString())
                    '◯ コード(出荷基地)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL02").ToString() + iStartCnt.ToString()).Value = CInt(PrintDatarow("CENERGYELNESS_SHUKACODE").ToString())
                    '◯ コード(届　先)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL03").ToString() + iStartCnt.ToString()).Value = CInt(PrintDatarow("CENERGYELNESS_TODOKECODE").ToString())
                    '◯ コード(計量№)
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL04").ToString() + iStartCnt.ToString()).Value = ""
                    '◯ コード(出荷量(T))
                    WW_Workbook.Worksheets(iSheetNo).Range(PrintDatarow("SETCELL05").ToString() + iStartCnt.ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                    iCnt += 1
                Next
            Next

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 帳票の(固定費・単価)の設定
    ''' </summary>
    Private Sub EditKoteihiTankaArea()

        Try
            '■基準シート
            '①基準(川越・知多)
            For Each PrintTankDatarow As DataRow In PrintTankData.Select(String.Format("GRPNO='{0}'", "1"))
                '・往復距離(km)
                Dim setCellROUNDTRIP As String = ""
                If PrintTankDatarow("SHUKABASHO").ToString() = "100" Then
                    setCellROUNDTRIP = PrintTankDatarow("SHEET_CELLNO01").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(0)).Range(setCellROUNDTRIP).Value = Double.Parse(PrintTankDatarow("ROUNDTRIP").ToString())
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "500" Then
                    setCellROUNDTRIP = PrintTankDatarow("SHEET_CELLNO03").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(0)).Range(setCellROUNDTRIP).Value = Double.Parse(PrintTankDatarow("ROUNDTRIP").ToString())
                End If

                '・通行料(円)
                Dim setCellTOLLFEE As String = ""
                If PrintTankDatarow("SHUKABASHO").ToString() = "100" Then
                    setCellTOLLFEE = PrintTankDatarow("SHEET_CELLNO02").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(0)).Range(setCellTOLLFEE).Value = Double.Parse(PrintTankDatarow("TOLLFEE").ToString())
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "500" Then
                    setCellTOLLFEE = PrintTankDatarow("SHEET_CELLNO04").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(0)).Range(setCellTOLLFEE).Value = Double.Parse(PrintTankDatarow("TOLLFEE").ToString())
                End If

            Next
            '②基準(上越)
            For Each PrintTankDatarow As DataRow In PrintTankData.Select(String.Format("GRPNO='{0}'", "2"))
                '・往復距離(km)
                Dim setCellROUNDTRIP As String = ""
                If PrintTankDatarow("SHUKABASHO").ToString() = "900" Then
                    setCellROUNDTRIP = PrintTankDatarow("SHEET_CELLNO01").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(1)).Range(setCellROUNDTRIP).Value = Double.Parse(PrintTankDatarow("ROUNDTRIP").ToString())
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "100" Then
                    setCellROUNDTRIP = PrintTankDatarow("SHEET_CELLNO03").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(1)).Range(setCellROUNDTRIP).Value = Double.Parse(PrintTankDatarow("ROUNDTRIP").ToString())
                End If

                '・通行料(円)
                Dim setCellTOLLFEE As String = ""
                If PrintTankDatarow("SHUKABASHO").ToString() = "900" Then
                    setCellTOLLFEE = PrintTankDatarow("SHEET_CELLNO02").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(1)).Range(setCellTOLLFEE).Value = Double.Parse(PrintTankDatarow("TOLLFEE").ToString())
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "100" Then
                    setCellTOLLFEE = PrintTankDatarow("SHEET_CELLNO04").ToString() + PrintTankDatarow("MASTERNO").ToString()
                    WW_Workbook.Worksheets(WW_SheetStandards(1)).Range(setCellTOLLFEE).Value = Double.Parse(PrintTankDatarow("TOLLFEE").ToString())
                End If

            Next
            '③基準
            For Each PrintTankDatarow As DataRow In PrintTankData.Select(String.Format("GRPNO='{0}'", "3"))
                '・運賃(円)
                Dim setCellGyo As Integer = Integer.Parse(PrintTankDatarow("MASTERNO").ToString())
                If PrintTankDatarow("SHUKABASHO").ToString() = "100" Then
                    '### そのまま
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "300" Then
                    setCellGyo += 101
                ElseIf PrintTankDatarow("SHUKABASHO").ToString() = "500" Then
                    setCellGyo += 202
                End If

                Dim setCellFARE As String = ""
                setCellFARE = PrintTankDatarow("SHEET_CELLNO01").ToString() + setCellGyo.ToString()
                WW_Workbook.Worksheets(WW_SheetStandards(2)).Range(setCellFARE).Value = Double.Parse(PrintTankDatarow("TANKA").ToString())
            Next

            '■基本料金(基準(川越・知多)・基準(上越))※３〇〇車番
            '〇[マスタ]シート
            '★シーエナジーについて(車番、単価(距離単価)、設定セルNo)をグルーピング
            Dim queryK = From row In PrintTankData.AsEnumerable()
                         Where row.Field(Of String)("TORICODE") = BaseDllConst.CONST_TORICODE_0110600000 AndAlso row.Field(Of Decimal)("TANKA") <> 0
                         Group row By SYAGOU = row.Field(Of String)("SYAGOU"),
                                      SYABARA = row.Field(Of Decimal)("SYABARA"),
                                      TANKA = row.Field(Of Decimal)("TANKA"),
                                      MASTER_CELLKYORITANKA = row.Field(Of String)("MASTER_CELLKYORITANKA"),
                                      MASTER_CELLLINE = row.Field(Of String)("MASTER_CELLLINE") Into Group
                         Select New With {
                            .SYAGOU = SYAGOU,
                            .SYABARA = SYABARA,
                            .TANKA = TANKA,
                            .MASTER_CELLKYORITANKA = MASTER_CELLKYORITANKA,
                            .MASTER_CELLLINE = MASTER_CELLLINE
                        }

            '・車番
            '・単位
            '□距離単価
            For Each result In queryK
                Dim resSyagou = result.SYAGOU
                Dim resTanka = result.TANKA
                Dim resKyoriTanka = result.MASTER_CELLKYORITANKA
                Dim resCellLine = result.MASTER_CELLLINE
                Dim setCellNum As String = resKyoriTanka + resCellLine
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value = resTanka
            Next
            '□基本運賃
            For Each PrintKoteihiDatarow As DataRow In PrintKoteihiData.Select(String.Format("TORICODE='{0}'", BaseDllConst.CONST_TORICODE_0110600000))
                Dim setCellNum As String = PrintKoteihiDatarow("KOTEIHI_CELL03").ToString()
                setCellNum &= PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value = Integer.Parse(PrintKoteihiDatarow("KOTEIHI").ToString())
            Next

            '■基本料金(基準(川越・上越・富山))　　　※６〇〇車番
            '□車番
            For Each PrintKoteihiDatarow As DataRow In PrintKoteihiData.Select(String.Format("TORICODE='{0}'", BaseDllConst.CONST_TORICODE_0238900000))
                Dim setCellNum As String = ""
                '〇季節料金判定区分("1"(通常), "2"(冬季))
                If PrintKoteihiDatarow("SEASONKBN").ToString() = "1" Then
                    '□基本運賃(通常(夏季))
                    setCellNum = PrintKoteihiDatarow("KOTEIHI_CELL02").ToString()
                ElseIf PrintKoteihiDatarow("SEASONKBN").ToString() = "2" Then
                    '□基本運賃(冬季)
                    setCellNum = PrintKoteihiDatarow("KOTEIHI_CELL03").ToString()
                ElseIf PrintKoteihiDatarow("SEASONKBN").ToString() = "0" Then
                    '□基本運賃(通年)
                    setCellNum = PrintKoteihiDatarow("KOTEIHI_CELL02").ToString()
                Else
                    Continue For
                End If
                '★行セルが未設定の場合SKIP
                If setCellNum = "" Then Continue For

                '★基本運賃設定
                setCellNum &= PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value = Integer.Parse(PrintKoteihiDatarow("KOTEIHI").ToString())
                '□基本運賃(通年)の場合
                If PrintKoteihiDatarow("SEASONKBN").ToString() = "0" Then
                    '★基本運賃設定(□基本運賃(冬季))
                    setCellNum = PrintKoteihiDatarow("KOTEIHI_CELL03").ToString()
                    setCellNum &= PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value = Integer.Parse(PrintKoteihiDatarow("KOTEIHI").ToString())
                End If

                '★車番(未設定)チェック
                setCellNum = "A" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()
                Dim syabanName As String = ""
                Try
                    syabanName = WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value.ToString()
                Catch ex As Exception
                End Try
                If syabanName = "" Then
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCellNum).Value = CInt(PrintKoteihiDatarow("SYABAN").ToString())
                End If
            Next

            '★季節判定
            Select Case Me.TaishoMM
                '〇04月～11月(通常)
                Case "04", "05", "06", "07", "08", "09", "10", "11"
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range("O6").Value = 1
                '〇12月～03月(冬季)
                Case "12", "01", "02", "03"
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range("O6").Value = 2
            End Select

            '■シーエナジー(休日運賃)
            Dim conditionSub As String = "RANGE_SUNDAY='1' OR RANGE_HOLIDAY='1' "
            For Each PrintHolidayRateDatarow As DataRow In PrintHolidayRateData.Select(conditionSub)
                If PrintHolidayRateDatarow("SETMASTERCELL").ToString() = "" Then Continue For
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("E{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())
            Next
            '■シーエナジー(年末年始料金)
            conditionSub = "RANGE_YEAREND_NEWYEAR='1' "
            For Each PrintHolidayRateDatarow As DataRow In PrintHolidayRateData.Select(conditionSub)
                If PrintHolidayRateDatarow("SETMASTERCELL").ToString() = "" Then Continue For
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("F{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())
            Next

        Catch ex As Exception

        End Try

    End Sub

End Class
