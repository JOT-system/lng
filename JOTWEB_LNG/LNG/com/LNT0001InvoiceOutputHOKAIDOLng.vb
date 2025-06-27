Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputHOKAIDOLng
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoYusouhiMeisai As Integer = 0
    Private WW_SheetNoCalendar As Integer = 0
    Private WW_SheetNoHoliday As Integer = 0
    Private WW_SheetNoMaster As Integer = 0
    Private WW_ArrSheetNo01 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(出荷場所：石狩)
    Private WW_ArrSheetNo02 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}   '// 追加シート用(出荷場所：釧路)
    Private WW_ArrCalendarWeek As String() = {"日", "月", "火", "水", "木", "金", "土"}

    Private WW_DicHokkaidoLNGList As Dictionary(Of String, String)
    Private WW_SheetNo01Dic As New Dictionary(Of String, Integer)           '// 既存シート用

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
    Private PrintKihonFeeAData As DataTable
    Private PrintKihonSyabanFeeAData As DataTable
    Private PrintTogouSprate As DataTable
    Private PrintCalendarData As DataTable
    Private PrintHolidayRateData As DataTable
    Private PrintHolidayRateNumData As DataTable
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
    ''' <param name="dicHokkaidoLNGList">北海道LNG(届先)格納</param>
    ''' <param name="printHolidayRateDataClass">休日割増単価マスタ</param>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printKihonFeeADataClass As DataTable, printKihonSyabanFeeADataClass As DataTable, printCalendarDataClass As DataTable,
                   dicHokkaidoLNGList As Dictionary(Of String, String),
                   Optional ByVal printTogouSprateDataClass As DataTable = Nothing,
                   Optional ByVal printHolidayRateDataClass As DataTable = Nothing,
                   Optional ByVal printHolidayRateNumDataClass As DataTable = Nothing,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal calcNumber As Integer = 1,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTankData = printTankDataClass
            Me.PrintKoteihiData = printKoteihiDataClass
            Me.PrintKihonFeeAData = printKihonFeeADataClass
            Me.PrintKihonSyabanFeeAData = printKihonSyabanFeeADataClass
            Me.PrintCalendarData = printCalendarDataClass
            Me.PrintTogouSprate = printTogouSprateDataClass
            Me.PrintHolidayRateData = printHolidayRateDataClass
            Me.PrintHolidayRateNumData = printHolidayRateNumDataClass
            Me.WW_DicHokkaidoLNGList = dicHokkaidoLNGList
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputOrgCode = orgCode
            Me.OutputFileName = outputFileName
            Me.calcZissekiNumber = calcNumber

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

            '〇[北海道LNG]シート設定
            For Each dic In dicHokkaidoLNGList
                Dim indexKey = dic.Key
                Dim strValue = dic.Value
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = strValue Then
                        WW_SheetNo01Dic.Add(indexKey, i)
                        Exit For
                    End If
                Next
            Next

            Dim j As Integer() = {0, 0}
            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "入力表" Then

                ElseIf WW_Workbook.Worksheets(i).Name = "輸送費明細" Then
                    '〇共通(シート[輸送費明細])
                    WW_SheetNoYusouhiMeisai = i
                ElseIf WW_Workbook.Worksheets(i).Name = "休日" Then
                    '〇共通(シート[休日])
                    WW_SheetNoCalendar = i
                ElseIf WW_Workbook.Worksheets(i).Name = "祝日" Then
                    '〇共通(シート[祝日])
                    WW_SheetNoHoliday = i
                ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                    '〇共通(シート[ﾏｽﾀ])
                    WW_SheetNoMaster = i
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP1" + (j(0) + 1).ToString("00") Then
                    '〇追加用(シート[石狩])
                    WW_ArrSheetNo01(j(0)) = i
                    j(0) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP2" + (j(1) + 1).ToString("00") Then
                    '〇追加用(シート[釧路])
                    WW_ArrSheetNo02(j(1)) = i
                    j(1) += 1
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
            EditTogouMasterArea()
            '***** TODO処理 ここまで *****
            '★ [祝日]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoHoliday).Visible = Visibility.Hidden
            '★ [ﾏｽﾀ]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoMaster).Visible = Visibility.Hidden

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
            '◯ 年月日(シート[休日])
            WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("A2").Value = Date.Parse(Me.TaishoYm + "/" + "01")

            '〇カレンダー設定(シート[祝日]の設定)
            Dim iCalendarLine As Integer = 1
            For Each PrintCalendarDatarow As DataRow In PrintCalendarData.Rows
                If PrintCalendarDatarow("WORKINGDAY").ToString() = "0" Then
                    '平日はSKIP
                    Continue For
                ElseIf PrintCalendarDatarow("WORKINGDAY").ToString() = "1" Then
                    If PrintCalendarDatarow("PUBLICHOLIDAYNAME").ToString() = "" Then
                        '日曜(祝日以外)はSKIP
                        Continue For
                    Else
                        WW_Workbook.Worksheets(WW_SheetNoHoliday).Range("D" + iCalendarLine.ToString("00")).Value = "※日曜のため未入力"
                    End If
                ElseIf PrintCalendarDatarow("WORKINGDAY").ToString() = "5" Then
                    'メーデーはSKIP
                    Continue For
                Else
                    WW_Workbook.Worksheets(WW_SheetNoHoliday).Range("A" + iCalendarLine.ToString("00")).Value = PrintCalendarDatarow("YMD")
                End If
                WW_Workbook.Worksheets(WW_SheetNoHoliday).Range("B" + iCalendarLine.ToString("00")).Value = WW_ArrCalendarWeek(CInt(PrintCalendarDatarow("WEEKDAY")))
                WW_Workbook.Worksheets(WW_SheetNoHoliday).Range("C" + iCalendarLine.ToString("00")).Value = PrintCalendarDatarow("PUBLICHOLIDAYNAME")
                iCalendarLine += 1
            Next
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(メイン)
    ''' </summary>
    Private Sub EditDetailArea()

        Try
            Dim condition As String = "SETCELL01<>'' AND SHEETNAME_REP='{0}'"
            Dim conditionSb As String = ""
            Dim todokeCode As String = ""
            Dim sheetName As String = ""
            Dim sheetNo As Integer = 0

            '★計算エンジンの無効化
            WW_Workbook.EnableCalculation = False

            '★シート設定
            For Each dicSheetNo01 In WW_SheetNo01Dic
                '届名
                todokeCode = dicSheetNo01.Key.Substring(0, 6)
                '★シート名取得
                sheetName = WW_DicHokkaidoLNGList(dicSheetNo01.Key)
                '★条件設定
                conditionSb = String.Format(condition, sheetName)
                '〇セル設定
                EditDetailAreaSub(conditionSb, todokeCode, dicSheetNo01, sheetName)

            Next

            '★計算エンジンの有効化
            WW_Workbook.EnableCalculation = True

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細設定(サブ)
    ''' </summary>
    Private Sub EditDetailAreaSub(ByVal condition As String, ByVal todokeCode As String, ByVal dicSheetNo01 As KeyValuePair(Of String, Integer), ByVal sheetName As String)
        For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE, TODOKEDATE")
            If PrintDatarow("TODOKECODE").ToString() <> todokeCode Then
                Continue For
            End If
            '◯ 実績数量
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
            '★ (列)表示
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(String.Format("{0}:{0}", PrintDatarow("SETCELL").ToString())).Hidden = False
            '★ (行)表示
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(String.Format("{0}:{0}", PrintDatarow("SETLINE").ToString())).Hidden = False

            Try
                '〇届名称(追加)用設定(追加ではない場合はSKIP)
                If PrintDatarow("TODOKECELL_REP").ToString() = "" Then Continue For

                Dim iDisp As Integer = 0
                If PrintDatarow("SHEETDISPLAY_REP").ToString() <> "" Then
                    iDisp = CInt(PrintDatarow("SHEETDISPLAY_REP").ToString())
                End If

                Dim WW_ArrSheetNo As Integer = 0
                If sheetName.Substring(0, 4) = "TMP1" Then
                    WW_ArrSheetNo = WW_ArrSheetNo01(iDisp)
                    '★ シート名変更
                    WW_Workbook.Worksheets(WW_ArrSheetNo).Name = PrintDatarow("TODOKENAME_REP").ToString()
                ElseIf sheetName.Substring(0, 4) = "TMP2" Then
                    WW_ArrSheetNo = WW_ArrSheetNo02(iDisp)
                    '★ シート名変更
                    WW_Workbook.Worksheets(WW_ArrSheetNo).Name = PrintDatarow("TODOKENAME_REP").ToString()
                Else
                    WW_ArrSheetNo = dicSheetNo01.Value
                End If

                '★ シート表示
                WW_Workbook.Worksheets(WW_ArrSheetNo).Visible = Visibility.Visible

            Catch ex As Exception
            End Try

        Next
    End Sub

    ''' <summary>
    ''' 帳票の統合版マスタ設定
    ''' </summary>
    Private Sub EditTogouMasterArea()
        '統合版単価設定
        EditTogouTankaArea()

        '統合版固定費設定
        EditTogouFixedArea()

        '統合版特別料金設定
        EditTogouSprateArea()

    End Sub


    ''' <summary>
    ''' 統合版単価設定
    ''' </summary>
    Private Sub EditTogouTankaArea()
        '〇[単価]設定(統合版単価マスタ)
        Dim i As Integer = 0
        For Each PrintTankDatarow As DataRow In PrintTankData.Select("", "TODOKESHEET_CELL, SYAGATA, TANKAKBN")
            Dim j As Integer = 0
            '★設定セルが未設定の場合SKIP
            If PrintTankDatarow("TODOKESHEET_CELL").ToString() = "" Then Continue For
            Dim setTODOKESHEET_CELL As Integer = CInt(PrintTankDatarow("TODOKESHEET_CELL").ToString())

            '〇シート「輸送費明細」
            '★ 3.従量料金
            Dim cellGYO As String = ""
            i += 1
            If PrintTankDatarow("AVOCADOSHUKABASHO").ToString() = "003554" Then
                cellGYO = "G"
            Else
                cellGYO = "H"
            End If

            '★大岡技研室蘭工場(独自仕様)
            If PrintTankDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_004830 Then
                '車型が"2"(トレーラ)
                If PrintTankDatarow("SYAGATA").ToString() = "2" Then
                    setTODOKESHEET_CELL += 1
                End If
            ElseIf PrintTankDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_003630 _
                AndAlso PrintTankDatarow("AVOCADOSHUKABASHO").ToString() = "003554" Then
                '★（浜中）大塚製薬(独自仕様)
                If PrintTankDatarow("TODOKEBRANCHCODE").ToString() = "02" Then
                    '枝番が"02"(ENEX所属車単価)
                    j = 1
                    i -= 1
                ElseIf PrintTankDatarow("TODOKEBRANCHCODE").ToString() = "01" Then
                    '枝番が"01"(浜中単価)
                    setTODOKESHEET_CELL += 1
                    i += 1
                End If
            End If

            '--No
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("A" + setTODOKESHEET_CELL.ToString()).Value = i - j
            '--従量料金
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range(cellGYO + setTODOKESHEET_CELL.ToString()).Value = Double.Parse(PrintTankDatarow("TANKA").ToString())
            '★ 表示
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range(String.Format("{0}:{0}", setTODOKESHEET_CELL.ToString())).Hidden = False
        Next

    End Sub

    ''' <summary>
    ''' 統合版固定費設定
    ''' </summary>
    Private Sub EditTogouFixedArea()
        '〇[固定費]設定(統合版固定費マスタ)
        For Each PrintKihonFeeADatarow As DataRow In PrintKihonFeeAData.Select("SETCELLNO<>''")
            '〇シート「輸送費明細」
            '★ 1.基本料金A
            '--車番
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("D" + PrintKihonFeeADatarow("SETCELLNO").ToString()).Value = PrintKihonFeeADatarow("SYABAN").ToString()
            '--台数
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("E" + PrintKihonFeeADatarow("SETCELLNO").ToString()).Value = Integer.Parse(PrintKihonFeeADatarow("SYAKO_COUNT").ToString())
            '--単価
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("F" + PrintKihonFeeADatarow("SETCELLNO").ToString()).Value = Double.Parse(PrintKihonFeeADatarow("KOTEIHIM").ToString())
        Next

    End Sub

    ''' <summary>
    ''' 統合版特別料金設定
    ''' </summary>
    Private Sub EditTogouSprateArea()
        '〇[固定費]設定(統合版特別料金マスタ)
        '〇シート「輸送費明細」
        '★2.基本料金B（3309号車）
        Dim kihonBNo As String = "99"
        Dim kihonB_CellNo As Integer = 5
        For Each PrintTogouSpraterow As DataRow In PrintTogouSprate.Select(String.Format("BIGCATECODE='{0}'", kihonBNo))
            Dim kihonB_CellNoSub As Integer = kihonB_CellNo
            kihonB_CellNoSub += CInt(PrintTogouSpraterow("SMALLCATECODE").ToString())
            '★月額単価(車両費・運行維持費)
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("M" + kihonB_CellNoSub.ToString()).Value = Double.Parse(PrintTogouSpraterow("TANKA").ToString())
        Next

        '★4.その他
        For Each PrintTogouSpraterow As DataRow In PrintTogouSprate.Select(String.Format("KOTEIHI_CELLNUM<>'' AND BIGCATECODE<>'{0}'", kihonBNo))
            '★ 4.その他(委託料)
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("E" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("TANKA").ToString())

            '★ 4.その他(その他)
            If PrintTogouSpraterow("BIGCATECODE").ToString() = "5" Then
                '★ 明細名称
                Dim cellNo As String = WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("R" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Text
                cellNo &= PrintTogouSpraterow("SMALLCATENAME").ToString()
                WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("B" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = cellNo
                '★ 回数
                WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("G" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("QUANTITY").ToString())

                '★ 4.宿泊費・待機料金・その他
            ElseIf PrintTogouSpraterow("BIGCATECODE").ToString() = "7" _
                OrElse PrintTogouSpraterow("BIGCATECODE").ToString() = "8" _
                OrElse PrintTogouSpraterow("BIGCATECODE").ToString() = "9" Then
                '★ 回数
                WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range("G" + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("QUANTITY").ToString())

            End If
            '★ 表示
            WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range(String.Format("{0}:{0}", PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString())).Hidden = False
        Next

        '〇届先(休日割増単価)設定
        If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '■北海道LNG(日祝割増)
            Dim condition As String = "RANGE_SUNDAY='1' OR RANGE_HOLIDAY='1' OR RANGE_YEAREND_NEWYEAR='1' OR RANGE_MAYDAY='1' "
            For Each PrintHolidayRateDatarow As DataRow In PrintHolidayRateData.Select(condition)
                If PrintHolidayRateDatarow("SETMASTERCELL").ToString() = "" Then Continue For

                '★単価
                WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range(String.Format("E{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())

                '★回数(取得用)
                Dim conditionSub As String = "GRPKEY='{0}'"
                conditionSub = String.Format(conditionSub, PrintHolidayRateDatarow("SHUKABASHOCODE_LNM0005").ToString())
                For Each PrintHolidayRateNumDatarow As DataRow In PrintHolidayRateNumData.Select(conditionSub)
                    WW_Workbook.Worksheets(WW_SheetNoYusouhiMeisai).Range(String.Format("G{0}", PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateNumDatarow("GRPCNT").ToString())
                Next

            Next
        End If

    End Sub

End Class
