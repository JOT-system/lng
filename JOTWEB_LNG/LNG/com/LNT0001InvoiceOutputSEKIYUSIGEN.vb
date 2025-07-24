Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputSEKIYUSIGEN
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoSKKoteihi As Integer = 0
    Private WW_SheetNoShosenMitsui As Integer = 0
    Private WW_SheetNoUnchin As Integer = 0
    Private WW_SheetNoCalendar As Integer = 0
    Private WW_SheetNoMaster As Integer = 0
    Private WW_SheetNo01Dic As New Dictionary(Of String, Integer)           '// 既存シート用(新潟)
    Private WW_SheetNo02Dic As New Dictionary(Of String, Integer)           '// 既存シート用(庄内)
    Private WW_SheetNo03Dic As New Dictionary(Of String, Integer)           '// 既存シート用(東北)
    Private WW_SheetNo04Dic As New Dictionary(Of String, Integer)           '// 既存シート用(茨城)
    Private WW_ArrSheetNo01 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(新潟・庄内)
    Private WW_ArrSheetNo02 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(秋田)
    Private WW_ArrSheetNo03 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(東北)
    Private WW_ArrSheetNo04 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(茨城)
    Private WW_ArrSheetNoKoteichi As Integer() = {0, 0, 0, 0, 0}            '// 単価シート用

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
    Private PrintSKSurchargeData As DataTable
    Private PrintCalendarData As DataTable
    Private PrintSKKoteichiData As DataTable
    Private PrintTogouSprate As DataTable
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
    ''' <param name="dicNigataList">新潟(届先)格納</param>
    ''' <param name="dicSyonaiList">庄内(届先)格納</param>
    ''' <param name="dicTouhokuList">東北(届先)格納</param>
    ''' <param name="dicIbarakiList">茨城(届先)格納</param>
    ''' <param name="printHolidayRateDataClass">休日割増単価マスタ</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printSKKoteihiDataClass As DataTable, printCalendarDataClass As DataTable, printSKKoteichiDataClass As DataTable,
                   dicNigataList As Dictionary(Of String, String), dicSyonaiList As Dictionary(Of String, String), dicTouhokuList As Dictionary(Of String, String), dicIbarakiList As Dictionary(Of String, String),
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
            Me.PrintSKSurchargeData = printSKKoteihiDataClass
            Me.PrintCalendarData = printCalendarDataClass
            Me.PrintSKKoteichiData = printSKKoteichiDataClass
            Me.PrintTogouSprate = printTogouSprateDataClass
            Me.PrintHolidayRateData = printHolidayRateDataClass
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputOrgCode = orgCode
            Me.OutputFileName = outputFileName
            Me.calcZissekiNumber = calcNumber
            'ReDim WW_SheetNo01(dicNigataList.Count - 1)
            'ReDim WW_SheetNo02(dicSyonaiList.Count - 1)
            'ReDim WW_SheetNo03(dicTouhokuList.Count - 1)
            'ReDim WW_SheetNo04(dicIbarakiList.Count - 1)

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

            'Dim iNum As Integer = 0
            '〇[新潟]シート設定
            For Each dic In dicNigataList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = strValue Then
                        WW_SheetNo01Dic.Add(indexKey, i)
                        Exit For
                    End If
                Next
            Next
            '〇[庄内]シート設定
            For Each dic In dicSyonaiList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = strValue Then
                        WW_SheetNo02Dic.Add(indexKey, i)
                        Exit For
                    End If
                Next
            Next
            '〇[東北]シート設定
            For Each dic In dicTouhokuList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = strValue Then
                        WW_SheetNo03Dic.Add(indexKey, i)
                        'WW_SheetNo03(iNum) = i
                        'iNum += 1
                        Exit For
                    End If
                Next
            Next
            '〇[茨城]シート設定
            For Each dic In dicIbarakiList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = strValue Then
                        WW_SheetNo04Dic.Add(indexKey, i)
                        Exit For
                    End If
                Next
            Next
            Dim j As Integer() = {0, 0, 0, 0, 0}
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "入力表" Then
                    'ElseIf WW_Workbook.Worksheets(i).Name = "寺岡製作所（相馬出荷・東北）" Then
                    '    WW_SheetNo03(0) = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "鶴岡ガス（相馬出荷・東北）" Then
                    '    WW_SheetNo03(1) = i
                    'ElseIf WW_Workbook.Worksheets(i).Name = "若松ガス（相馬出荷・東北）" Then
                    '    WW_SheetNo03(2) = i
                ElseIf WW_Workbook.Worksheets(i).Name = "固定運賃" Then
                    '〇共通(シート[固定運賃])
                    WW_SheetNoSKKoteihi = i
                ElseIf WW_Workbook.Worksheets(i).Name = "従量運賃" Then
                    '〇共通(シート[従量運賃])
                    WW_SheetNoUnchin = i
                ElseIf WW_Workbook.Worksheets(i).Name = "若松ｶﾞｽ(玉川)" Then
                    '〇SK(シート[届先別])
                    WW_SheetNoCalendar = i
                ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                    '〇共通(シート[ﾏｽﾀ])
                    WW_SheetNoMaster = i
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP6" + (j(0) + 1).ToString("00") Then
                    WW_ArrSheetNo01(j(0)) = i
                    j(0) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP7" + (j(1) + 1).ToString("00") Then
                    WW_ArrSheetNo02(j(1)) = i
                    j(1) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP8" + (j(2) + 1).ToString("00") Then
                    WW_ArrSheetNo03(j(2)) = i
                    j(2) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP9" + (j(3) + 1).ToString("00") Then
                    WW_ArrSheetNo04(j(3)) = i
                    j(3) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)新潟①" _
                    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)新潟②" _
                    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)秋田" _
                    OrElse WW_Workbook.Worksheets(i).Name = "固定値(東北)" _
                    OrElse WW_Workbook.Worksheets(i).Name = "固定値(茨城)" Then
                    WW_ArrSheetNoKoteichi(j(4)) = i
                    j(4) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "サーチャージ明細（商船三井）" Then
                    '〇共通(シート[サーチャージ明細（商船三井）])
                    WW_SheetNoShosenMitsui = i
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
            '★ [ﾏｽﾀ]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoMaster).Visible = Visibility.Hidden
            '★ [固定値]シート非表示
            For Each i In WW_ArrSheetNoKoteichi
                WW_Workbook.Worksheets(i).Visible = Visibility.Hidden
            Next

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
        Dim dayCellsSub As String() = {"", "", ""}
        Try
            '◯ 年月
            WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AD1").Value = Integer.Parse(Me.TaishoYYYY)
            WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AD2").Value = Integer.Parse(Me.TaishoMM)

            '〇カレンダー設定
            Dim iCalendarLine As Integer = 5
            For Each PrintCalendarDatarow As DataRow In PrintCalendarData.Rows
                If PrintCalendarDatarow("WORKINGDAY").ToString() <> "0" Then
                    WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AE" + iCalendarLine.ToString("00")).Value = "1"
                Else
                    WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AE" + iCalendarLine.ToString("00")).Value = "0"
                End If
                iCalendarLine += 1
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
            Dim condition As String = "SETCELL01<>'' AND GROUPNO_REP='{0}'"
            '〇[新潟]シート設定
            For Each dicSheetNo01 In WW_SheetNo01Dic
                condition = String.Format(condition, "1")
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE")
                    If PrintDatarow("TODOKECODE").ToString() <> dicSheetNo01.Key Then
                        Continue For
                    End If
                    '◯ 届先名
                    WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                Next
            Next
            '〇[庄内]シート設定
            condition = "SETCELL01<>'' AND GROUPNO_REP='{0}'"
            For Each dicSheetNo02 In WW_SheetNo02Dic
                condition = String.Format(condition, "2")
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE")
                    If PrintDatarow("TODOKECODE").ToString() <> dicSheetNo02.Key Then
                        Continue For
                    End If
                    '◯ 届先名
                    WW_Workbook.Worksheets(dicSheetNo02.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(dicSheetNo02.Value).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                Next
            Next
            '〇[東北]シート設定
            condition = "SETCELL01<>'' AND GROUPNO_REP='{0}'"
            For Each dicSheetNo03 In WW_SheetNo03Dic
                condition = String.Format(condition, "3")
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE")
                    If PrintDatarow("TODOKECODE").ToString() <> dicSheetNo03.Key Then
                        Continue For
                    End If
                    '◯ 届先名
                    WW_Workbook.Worksheets(dicSheetNo03.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(dicSheetNo03.Value).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                Next
            Next
            '〇[茨城]シート設定
            condition = "SETCELL01<>'' AND GROUPNO_REP='{0}'"
            For Each dicSheetNo04 In WW_SheetNo04Dic
                condition = String.Format(condition, "4")
                For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE")
                    If PrintDatarow("TODOKECODE").ToString() <> dicSheetNo04.Key Then
                        Continue For
                    End If
                    '◯ 届先名
                    WW_Workbook.Worksheets(dicSheetNo04.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(dicSheetNo04.Value).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                Next
            Next
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 帳票のSK固定費設定
    ''' </summary>
    Private Sub EditKoteihiTankaArea()
        Try
            '★計算エンジンの無効化
            WW_Workbook.EnableCalculation = False

            '〇業務番号(固定費)設定(※陸事番号)
            For Each PrintKoteihiDatarow As DataRow In PrintKoteihiData.Select("KOTEIHI_CELLNUM<>''")
                '〇シート「固定運賃」
                '★ 月額固定費
                WW_Workbook.Worksheets(WW_SheetNoSKKoteihi).Range("E" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("GETSUGAKU").ToString())
                '★ 減額固定費
                WW_Workbook.Worksheets(WW_SheetNoSKKoteihi).Range("G" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("GENGAKU").ToString())

                '※陸事番号(固定費)(追加)用設定
                If PrintKoteihiDatarow("KOTEIHI_DISPLAY").ToString() = "1" Then
                    '★ 車番
                    WW_Workbook.Worksheets(WW_SheetNoSKKoteihi).Range("D" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("SYABAN").ToString())
                    '★ 表示
                    WW_Workbook.Worksheets(WW_SheetNoSKKoteihi).Range(String.Format("{0}:{0}", PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString())).Hidden = False
                End If
            Next

            '〇届名称(追加)用設定
            For Each PrintDatarow As DataRow In PrintData.Select("TODOKECELL_REP<>''")
                '〇シート「従量運賃」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(String.Format("{0}:{0}", PrintDatarow("TODOKECELL_REP").ToString())).Hidden = False

                '〇シート「マスタ」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("{0}:{0}", PrintDatarow("MASTERCELL_REP").ToString())).Hidden = False
                '★ 設定(配送先)
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("A{0}", PrintDatarow("MASTERCELL_REP").ToString())).Value = PrintDatarow("TODOKENAME_REP").ToString()
                '★ 設定(向け先)
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("F{0}", PrintDatarow("MASTERCELL_REP").ToString())).Value = PrintDatarow("SHEETNAME_REP").ToString()

                Try
                    Dim iDisp As Integer = Integer.Parse(PrintDatarow("SHEETDISPLAY_REP").ToString())
                    If PrintDatarow("GROUPNO_REP").ToString() = "1" Then
                        '★ シート表示
                        WW_Workbook.Worksheets(WW_ArrSheetNo01(iDisp)).Visible = Visibility.Visible
                        '★ シート名変更
                        WW_Workbook.Worksheets(WW_ArrSheetNo01(iDisp)).Name = PrintDatarow("TODOKENAME_REP").ToString()

                    ElseIf PrintDatarow("GROUPNO_REP").ToString() = "2" Then
                        '★ シート表示
                        WW_Workbook.Worksheets(WW_ArrSheetNo02(iDisp)).Visible = Visibility.Visible
                        '★ シート名変更
                        WW_Workbook.Worksheets(WW_ArrSheetNo02(iDisp)).Name = PrintDatarow("TODOKENAME_REP").ToString()

                    ElseIf PrintDatarow("GROUPNO_REP").ToString() = "3" Then
                        '★ シート表示
                        WW_Workbook.Worksheets(WW_ArrSheetNo03(iDisp)).Visible = Visibility.Visible
                        '★ シート名変更
                        WW_Workbook.Worksheets(WW_ArrSheetNo03(iDisp)).Name = PrintDatarow("TODOKENAME_REP").ToString()

                    ElseIf PrintDatarow("GROUPNO_REP").ToString() = "4" Then
                        '★ シート表示
                        WW_Workbook.Worksheets(WW_ArrSheetNo04(iDisp)).Visible = Visibility.Visible
                        '★ シート名変更
                        WW_Workbook.Worksheets(WW_ArrSheetNo04(iDisp)).Name = PrintDatarow("TODOKENAME_REP").ToString()

                    End If
                Catch ex As Exception
                End Try

            Next

            '〇(その他)届名称(追加)用設定
            For Each PrintTogouSpraterow As DataRow In PrintTogouSprate.Select("KOTEIHI_CELLNUM<>''")
                '〇シート「従量運賃」
                '★ 配送先
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("D" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = PrintTogouSpraterow("SMALLCATENAME").ToString()
                '★ 輸送数量
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("K" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = ""
                '★ 課税対象額
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("O" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Decimal.Parse(PrintTogouSpraterow("TANKA").ToString())
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(String.Format("{0}:{0}", PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString())).Hidden = False
            Next

            '〇不積セル(行)設定
            SetDeadFreightCell()

            '〇届先(単価)設定
            For Each PrintDatarow As DataRow In PrintSKKoteichiData.Rows
                If PrintDatarow("TANKA").ToString() = "" Then Continue For
                Dim iTanka As Integer = Integer.Parse(PrintDatarow("TANKA").ToString())
                Dim iSheetNum As Integer = Integer.Parse(PrintDatarow("GRPNO").ToString()) - 1
                Dim setCell As String = PrintDatarow("KOTEICHI_YOKOCELL").ToString() + PrintDatarow("SET_CELL").ToString()

                ''■単価調整の場合
                'If PrintDatarow("BRANCHCODE").ToString() = "02" Then
                '    '★個別設定項目
                '    SetIndividualItem(PrintDatarow, WW_ArrSheetNoKoteichi(iSheetNum), iTanka)
                'Else
                WW_Workbook.Worksheets(WW_ArrSheetNoKoteichi(iSheetNum)).Range(setCell).Value = iTanka
                'End If

                If PrintDatarow("MEISAI_HYOJIFLG").ToString() = "1" Then
                    setCell = PrintDatarow("KOTEICHI_YOKOCELL").ToString() + "3"
                    WW_Workbook.Worksheets(WW_ArrSheetNoKoteichi(iSheetNum)).Range(setCell).Value = PrintDatarow("KOTEICHI_GYOMU").ToString()
                End If
            Next

            '■石油資源開発(本州)(休日加算金)
            Dim conditionSub As String = "RANGE_SUNDAY='1' OR RANGE_HOLIDAY='1' OR RANGE_YEAREND_NEWYEAR='1' OR RANGE_MAYDAY='1' "
            For Each PrintHolidayRateDatarow As DataRow In PrintHolidayRateData.Select(conditionSub)
                If PrintHolidayRateDatarow("SETMASTERCELL").ToString() = "" Then Continue For
                'WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("D{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(String.Format("E{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())
            Next

            ''〇[商船三井サーチャージ]設定
            'For Each PrintSKSurchargeDatarow As DataRow In PrintSKSurchargeData.Select(String.Format("TODOKECODE='{0}'", BaseDllConst.CONST_TODOKECODE_007110))
            '    '走行距離
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("E18").Value = Decimal.Parse(PrintSKSurchargeDatarow("KYORI").ToString())
            '    '燃費
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("K18").Value = Decimal.Parse(PrintSKSurchargeDatarow("KEIYU").ToString())
            '    '実勢軽油価格
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("E23").Value = Decimal.Parse(PrintSKSurchargeDatarow("KEIYU").ToString())
            '    '基準価格
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("G23").Value = Decimal.Parse(PrintSKSurchargeDatarow("KIZYUN").ToString())
            '    '輸送回数
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("G18").Value = Integer.Parse(PrintSKSurchargeDatarow("KAISU").ToString())
            '    '燃料使用量
            '    WW_Workbook.Worksheets(WW_SheetNoShosenMitsui).Range("K30").Value = Integer.Parse(PrintSKSurchargeDatarow("USAGECHARGE").ToString())
            'Next

            '★計算エンジンの有効化
            WW_Workbook.EnableCalculation = True

        Catch ex As Exception

        End Try
    End Sub

    Private Sub SetIndividualItem(ByVal PrintDatarow As DataRow, ByVal sheetNo As Integer, ByVal tanka As Integer)
        Dim setCell As String = ""

        Select Case PrintDatarow("TODOKENO").ToString()
            '〇若松ガス
            Case BaseDllConst.CONST_TODOKECODE_002025
                If PrintDatarow("KOTEICHI_GYOMUNO").ToString() = "326" Then
                    '■業務車番(333)※1.5回転
                    setCell = "O" + PrintDatarow("SET_CELL").ToString()
                Else
                    Exit Sub
                End If

            '〇ﾃｰﾌﾞﾙﾏｰｸ新潟魚沼工場
            Case BaseDllConst.CONST_TODOKECODE_002019
                If PrintDatarow("KOTEICHI_GYOMUNO").ToString() = "333" Then
                    '■業務車番(333)※不積単価
                    setCell = "K" + PrintDatarow("SET_CELL").ToString()
                ElseIf PrintDatarow("KOTEICHI_GYOMUNO").ToString() = "334" Then
                    '■業務車番(334)※不積単価
                    setCell = "O" + PrintDatarow("SET_CELL").ToString()
                Else
                    Exit Sub
                End If

            Case Else
                Exit Sub
        End Select

        WW_Workbook.Worksheets(sheetNo).Range(setCell).Value = tanka

    End Sub

    ''' <summary>
    ''' 不積セル(行)設定
    ''' </summary>
    Private Sub SetDeadFreightCell()
        '〇新潟②
        For Each PrintDatarow As DataRow In PrintSKKoteichiData.Select("GRPNO = '2' ")
            Dim iSET_CELL As Integer = 0
            iSET_CELL = CInt(PrintDatarow("SET_CELL").ToString())
            Select Case PrintDatarow("BRANCHCODE").ToString()
                '■不積料金
                Case "02"
                    iSET_CELL += 28
            End Select
            PrintDatarow("SET_CELL") = iSET_CELL.ToString()
        Next

        '〇新潟①
        For Each PrintDatarow As DataRow In PrintSKKoteichiData.Select("GRPNO = '1' ")
            Dim iSET_CELL As Integer = 0
            iSET_CELL = CInt(PrintDatarow("SET_CELL").ToString())
            Select Case PrintDatarow("BRANCHCODE").ToString()
                '■不積料金
                Case "02"
                    iSET_CELL += 28
                '■1.5回転単価
                Case "03"
                    iSET_CELL += 56
                    PrintDatarow("GRPNO") = "2"
                '■不積1.5回転単価
                Case "04"
                    iSET_CELL += 84
                    PrintDatarow("GRPNO") = "2"
            End Select
            PrintDatarow("SET_CELL") = iSET_CELL.ToString()
        Next

        '〇秋田
        For Each PrintDatarow As DataRow In PrintSKKoteichiData.Select("GRPNO = '3' ")
            Dim iSET_CELL As Integer = 0
            iSET_CELL = CInt(PrintDatarow("SET_CELL").ToString())
            Select Case PrintDatarow("BRANCHCODE").ToString()
                '■不積料金
                Case "02", "04"
                    iSET_CELL += 18
            End Select
            PrintDatarow("SET_CELL") = iSET_CELL.ToString()
        Next

        '〇東北・茨城
        For Each PrintDatarow As DataRow In PrintSKKoteichiData.Select("GRPNO IN ('4','5') ")
            Dim iSET_CELL As Integer = 0
            iSET_CELL = CInt(PrintDatarow("SET_CELL").ToString())
            Select Case PrintDatarow("BRANCHCODE").ToString()
                '■不積料金
                Case "02"
                    iSET_CELL += 18
            End Select
            PrintDatarow("SET_CELL") = iSET_CELL.ToString()
        Next
    End Sub

End Class
