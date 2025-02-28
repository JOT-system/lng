Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputReport
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoTmp01 As Integer = 0
    Private WW_SheetNoTmp02 As Integer = 0
    Private WW_SheetNoTmp03 As Integer = 0
    Private WW_SheetNoTmp04 As Integer = 0
    Private WW_SheetNoTmp05 As Integer = 0
    Private WW_SheetNoTmp06 As Integer = 0
    Private WW_SheetNoTobuGas As Integer = 0
    Private WW_SheetNoMitsuiES As Integer = 0
    Private WW_ArrSheetNo As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTankData As DataTable
    Private PrintKoteihiData As DataTable
    Private PrintHachinoheSprateData As DataTable
    Private PrintEneosComfeeData As DataTable
    Private PrintCalendarData As DataTable
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
    ''' <param name="printKoteihiDataClass">固定費マスタ</param>
    ''' <param name="printHachinoheSprateDataClass">八戸特別料金マスタ</param>
    ''' <param name="printEneosComfeeDataClass">ENEOS業務委託料マスタ</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
                   printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printCalendarDataClass As DataTable,
                   Optional ByVal printHachinoheSprateDataClass As DataTable = Nothing,
                   Optional ByVal printEneosComfeeDataClass As DataTable = Nothing,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal calcNumber As Integer = 1,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTankData = printTankDataClass
            Me.PrintKoteihiData = printKoteihiDataClass
            Me.PrintCalendarData = printCalendarDataClass
            Me.PrintHachinoheSprateData = printHachinoheSprateDataClass
            Me.PrintEneosComfeeData = printEneosComfeeDataClass
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
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルopen
            WW_Workbook.Open(Me.ExcelTemplatePath)

            If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 _
                OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 _
                OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "01" _
                OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "02" _
                OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "03" _
                OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022801 Then
                Dim j As Integer = 0
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = "入力表" _
                        OrElse WW_Workbook.Worksheets(i).Name = "実績入力表" Then
                        WW_SheetNo = i
                    ElseIf (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 AndAlso WW_Workbook.Worksheets(i).Name = "東北電力　TMEJ内サテライト") _
                        OrElse (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 AndAlso WW_Workbook.Worksheets(i).Name = "加藤製油") _
                        OrElse (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022801 AndAlso WW_Workbook.Worksheets(i).Name = "日本板硝子") _
                        OrElse (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "01" AndAlso WW_Workbook.Worksheets(i).Name = "東洋ウレタン") _
                        OrElse (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "02" AndAlso WW_Workbook.Worksheets(i).Name = "新宮ガス") _
                        OrElse (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "03" AndAlso WW_Workbook.Worksheets(i).Name = "リコー") Then
                        '〇共通(シート[(共有用)届先])
                        WW_SheetNoTmp01 = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "固定費" Then
                        '〇共通(シート[固定費])
                        WW_SheetNoTmp02 = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "届先毎" _
                        OrElse WW_Workbook.Worksheets(i).Name = "水島（届先別）" Then
                        '〇ENEOS(シート[届先別])
                        WW_SheetNoTmp03 = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                        '〇共通(シート[ﾏｽﾀ])
                        WW_SheetNoTmp04 = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "八戸業務委託料" _
                        OrElse WW_Workbook.Worksheets(i).Name = "水島輸送分請求書" Then
                        '〇ENEOS(シート[業務委託料])
                        WW_SheetNoTmp05 = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "請求書明細" Then
                        '〇DAIGAS(シート[請求書明細])
                        WW_SheetNoTmp06 = i
                    ElseIf (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 AndAlso WW_Workbook.Worksheets(i).Name = "東部瓦斯") Then
                        '〇ENEOS(シート[東部瓦斯])
                        WW_SheetNoTobuGas = i
                    ElseIf (Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 AndAlso WW_Workbook.Worksheets(i).Name = "三井Ｅ＆Ｓ") Then
                        '〇ENEOS(シート[三井Ｅ＆Ｓ])
                        WW_SheetNoMitsuiES = i
                    ElseIf WW_Workbook.Worksheets(i).Name = "TMP" + (j + 1).ToString("00") Then
                        WW_ArrSheetNo(j) = i
                        j += 1
                    End If
                Next
            End If

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

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            '***** TODO処理 ここまで *****
            '★ [ﾏｽﾀ]シート非表示
            WW_Workbook.Worksheets(WW_SheetNoTmp04).Visible = Visibility.Hidden

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
            Select Case Me.OutputOrgCode
                Case BaseDllConst.CONST_ORDERORGCODE_020202,
                     BaseDllConst.CONST_ORDERORGCODE_023301,
                     BaseDllConst.CONST_ORDERORGCODE_022801
                    WW_Workbook.Worksheets(WW_SheetNo).Range("B1").Value = Integer.Parse(Me.TaishoYYYY)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("B2").Value = Integer.Parse(Me.TaishoMM)
                    dayCellsSub = {"91", "94", "97"}
                Case BaseDllConst.CONST_ORDERORGCODE_022702 + "01",
                     BaseDllConst.CONST_ORDERORGCODE_022702 + "02",
                     BaseDllConst.CONST_ORDERORGCODE_022702 + "03"
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C4").Value = Integer.Parse(Me.TaishoYYYY)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E4").Value = Integer.Parse(Me.TaishoMM)
                    dayCellsSub = {"36", "37", "38"}
            End Select

            '〇 日付(セルチェック)
            'Dim dayCells As String() = {"91", "94", "97"}
            Dim dayCells As String() = dayCellsSub
            Dim lastDay As String = Date.Parse(Me.TaishoYYYY + "/" + Me.TaishoMM + "/01").AddMonths(1).AddDays(-1).ToString("dd")
            Dim i As Integer = 0
            For Each dayCell As String In dayCells
                '★月末日チェック
                Dim blnFlg As Boolean = True
                If Integer.Parse(lastDay) = 28 Then
                ElseIf Integer.Parse(lastDay) = 29 Then
                    If i < 1 Then blnFlg = False
                ElseIf Integer.Parse(lastDay) = 30 Then
                    If i < 2 Then blnFlg = False
                ElseIf Integer.Parse(lastDay) = 31 Then
                    blnFlg = False
                End If

                '★チェックがTRUE
                If blnFlg = True Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + dayCell).Value = ""
                    WW_Workbook.Worksheets(WW_SheetNo).Range("B" + dayCell).Value = ""
                End If
                i += 1
            Next

            '〇 年月（鏡用）
            Dim lastDate As String = Me.TaishoYYYY + "/" + Me.TaishoMM + "/01"
            lastDate = Date.Parse(lastDate).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
            WW_Workbook.Worksheets(WW_SheetNoTmp01).Range("I1").Value = Date.Parse(lastDate)

            '〇カレンダー設定
            Dim iCalendarLine As Integer = 12
            If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "03" Then
                iCalendarLine = 13
            End If
            For Each PrintCalendarDatarow As DataRow In PrintCalendarData.Rows
                If PrintCalendarDatarow("WORKINGDAY").ToString() <> "0" Then
                    WW_Workbook.Worksheets(WW_SheetNoTmp01).Range("J" + iCalendarLine.ToString("00")).Value = "1"
                Else
                    WW_Workbook.Worksheets(WW_SheetNoTmp01).Range("J" + iCalendarLine.ToString("00")).Value = "0"
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
            Dim cellStay As String = ""
            Dim condition As String = ""

            'For Each PrintDatarow As DataRow In PrintData.Select("SETCELL01<>''", "ROWSORTNO, TODOKEDATE")
            For Each PrintDatarow As DataRow In PrintData.Select("SETCELL01<>''", "ROWSORTNO, SHUKADATE")
                '◯ 届先名
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL01").ToString()).Value = PrintDatarow("TODOKENAME_REP").ToString()
                '◯ 実績数量
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
                '◯ 備考
                If PrintDatarow("SETCELL03").ToString() = "" Then Continue For
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("SETCELL03").ToString()).Value = PrintDatarow("REMARK_REP").ToString()
            Next

            '★八戸営業所の場合([東部瓦斯]独自対応)
            If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 Then
                '届日メインで設定
                EditDetailAreaTobugas(BaseDllConst.CONST_TODOKECODE_005487, "AND TODOKEDATE_ORDER<>'3'", "C", "D")
                EditDetailAreaTobugas(BaseDllConst.CONST_TODOKECODE_005487, "AND TODOKEDATE_ORDER='3'", "E", "F")

            ElseIf Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 Then
                '★水島営業所の場合([三井Ｅ＆Ｓ]独自対応)
                '※仮で「受注数量」が8.000を基準とし実施
                EditDetailAreaMitsuiES(BaseDllConst.CONST_TODOKECODE_004002, " AND ZYUTYU_STR IN ('8.000','10.000')", "C", "D", True)
                EditDetailAreaMitsuiES(BaseDllConst.CONST_TODOKECODE_004002, " AND ZYUTYU_STR NOT IN ('8.000','10.000')", "E", "F", False)

            End If

            '★計算エンジンの無効化
            WW_Workbook.EnableCalculation = False

            '〇陸事番号(追加)用設定
            For Each PrintDatarow As DataRow In PrintData.Select("DISPLAYCELL_START<>''")
                If cellStay <> "" AndAlso cellStay = PrintDatarow("DISPLAYCELL_START").ToString() Then
                    Continue For
                End If
                '〇シート「入力表」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{1}", PrintDatarow("DISPLAYCELL_START").ToString(), PrintDatarow("DISPLAYCELL_END").ToString())).Hidden = False
                '★ 陸事番号
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("DISPLAYCELL_START").ToString() + "4").Value = PrintDatarow("TANKNUMBER").ToString()
                '★ 受注数量
                Dim dblZyutyu As Double = Math.Round(Double.Parse(PrintDatarow("ZYUTYU").ToString()), 1, MidpointRounding.AwayFromZero)
                WW_Workbook.Worksheets(WW_SheetNo).Range(PrintDatarow("DISPLAYCELL_END").ToString() + "4").Value = dblZyutyu.ToString() + "t"

                '〇シート「固定費」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoTmp02).Range(String.Format("{0}:{0}", PrintDatarow("DISPLAYCELL_KOTEICHI").ToString())).Hidden = False
                '★ トラクタ
                If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 _
                    OrElse Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_023301 Then
                    WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("E" + PrintDatarow("DISPLAYCELL_KOTEICHI").ToString()).Value = PrintDatarow("TRACTORNUMBER").ToString()
                End If
                '★ トレーラ
                WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("F" + PrintDatarow("DISPLAYCELL_KOTEICHI").ToString()).Value = PrintDatarow("TANKNUMBER").ToString()

                '〇シート「請求書明細」
                If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_022702 + "01" Then
                    Dim cellNum As Integer = 46
                    cellNum += Integer.Parse(PrintDatarow("DISPLAYCELL_KOTEICHI").ToString())
                    '★ 表示
                    WW_Workbook.Worksheets(WW_SheetNoTmp05).Range(String.Format("{0}:{0}", cellNum.ToString())).Hidden = False
                End If

                '表示用セル保管
                cellStay = PrintDatarow("DISPLAYCELL_START").ToString()
            Next

            '〇届名称(追加)用設定
            cellStay = ""
            For Each PrintDatarow As DataRow In PrintData.Select("TODOKECELL_REP<>''")
                If cellStay <> "" AndAlso cellStay = PrintDatarow("TODOKECELL_REP").ToString() Then
                    Continue For
                End If
                '〇シート「届先毎」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoTmp03).Range(String.Format("{0}:{0}", PrintDatarow("TODOKECELL_REP").ToString())).Hidden = False

                '〇シート「マスタ」
                '★ 表示
                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("{0}:{0}", PrintDatarow("MASTERCELL_REP").ToString())).Hidden = False
                '★ 設定(配送先)
                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("A{0}", PrintDatarow("MASTERCELL_REP").ToString())).Value = PrintDatarow("TODOKENAME_REP").ToString()
                '〇水島営業所の場合
                If PrintDatarow("ORDERORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_023301 Then
                    '★ 設定(向け先)
                    WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("E{0}", PrintDatarow("MASTERCELL_REP").ToString())).Value = PrintDatarow("SHEETNAME_REP").ToString()
                End If

                Try
                    '★ シート表示
                    Dim iDisp As Integer = Integer.Parse(PrintDatarow("SHEETDISPLAY_REP").ToString())
                    WW_Workbook.Worksheets(WW_ArrSheetNo(iDisp)).Visible = Visibility.Visible
                    '★ シート名変更
                    WW_Workbook.Worksheets(WW_ArrSheetNo(iDisp)).Name = PrintDatarow("TODOKENAME_REP").ToString()
                Catch ex As Exception
                End Try

                '表示用セル保管
                cellStay = PrintDatarow("TODOKECELL_REP").ToString()
            Next

            '〇陸事番号(固定費)設定
            For Each PrintKoteihiDatarow As DataRow In PrintKoteihiData.Select("KOTEIHI_CELLNUM<>''")
                '〇シート「固定費」
                '★ 月額固定費
                WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("G" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("KOTEIHI").ToString())
            Next
            '〇陸事番号(固定費(八戸人員/八戸出荷))設定
            For Each PrintHachinoheSprateDatarow As DataRow In PrintHachinoheSprateData.Rows
                '〇シート「固定費」
                If PrintHachinoheSprateDatarow("RECONAME").ToString() = "追加人員固定費" Then
                    '★ 追加人員固定費
                    If PrintHachinoheSprateDatarow("RECOID").ToString() = "1" Then
                        WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("G39").Value = Integer.Parse(PrintHachinoheSprateDatarow("KINGAKU").ToString())
                    ElseIf PrintHachinoheSprateDatarow("RECOID").ToString() = "2" Then
                        WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("G40").Value = Integer.Parse(PrintHachinoheSprateDatarow("KINGAKU").ToString())
                    End If
                ElseIf PrintHachinoheSprateDatarow("RECONAME").ToString() = "八戸ターミナル負担分" Then
                    '★ 八戸ターミナル負担分
                    WW_Workbook.Worksheets(WW_SheetNoTmp02).Range("G41").Value = Integer.Parse(PrintHachinoheSprateDatarow("KINGAKU").ToString())
                End If
            Next

            '〇届先(単価)設定
            For Each PrintDatarow As DataRow In PrintTankData.Select("", "MASTERNO")
                If PrintDatarow("MASTERNO").ToString() = "" OrElse PrintDatarow("MASTERNO").ToString() = "0" Then Continue For
                '〇シート「マスタ」
                Dim iTanka As Integer = Integer.Parse(PrintDatarow("TANKA").ToString())
                If Convert.ToString(PrintDatarow("SYAGATA")) = "1" Then
                    '★単車
                    WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("B{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                ElseIf Convert.ToString(PrintDatarow("SYAGATA")) = "2" Then
                    '★トレーラ
                    '〇水島営業所(三井Ｅ＆Ｓ, コカ・コーラ)独自仕様
                    If PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_023301 _
                        AndAlso (PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_004002) _
                        AndAlso PrintDatarow("TODOKEBRANCHCODE").ToString() = "02" Then
                        WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("D{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka

                    ElseIf PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_023301 _
                        AndAlso (PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005509) Then
                        Select Case PrintDatarow("TODOKEBRANCHCODE").ToString()
                            Case "02"
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("D{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            Case "03"
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("E{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            Case "04"
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("F{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            Case "05"
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("G{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                        End Select

                        '〇西日本支店車庫(泉北)独自仕様
                    ElseIf PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_022702 Then
                        Dim cellValue As String = ""
                        cellValue = WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("A{0}", PrintDatarow("MASTERNO").ToString())).Value.ToString()

                        '☆(日本栄船)独自仕様
                        If PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_004916 _
                            AndAlso PrintDatarow("SYUBETSU").ToString() = "運行単価" _
                            AndAlso PrintDatarow("BIKOU1").ToString() = "2名乗車" Then
                            WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("B{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka

                            '☆(昭和産業㈱)独自仕様※[休日加算金]以外
                        ElseIf PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005866 _
                            AndAlso PrintDatarow("SYUBETSU").ToString() <> "休日加算金" Then
                            If cellValue = "昭和産業1" _
                                AndAlso PrintDatarow("SYUBETSU").ToString() = "トン単価" _
                                AndAlso PrintDatarow("BIKOU1").ToString() = "1運行目" Then
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("C{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            ElseIf cellValue = "昭和産業2" _
                                AndAlso PrintDatarow("SYUBETSU").ToString() = "トン単価" _
                                AndAlso PrintDatarow("BIKOU1").ToString() = "2運行目" Then
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("C{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            End If

                        Else
                            WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("C{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                        End If

                        '〇姫路営業所独自仕様
                    ElseIf PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_022801 Then
                        '☆(ナガセケムテックス)独自仕様
                        If PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_006880 _
                            AndAlso PrintDatarow("BIKOU1").ToString() = "2運行目" Then
                            WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("D{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                        Else
                            WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("C{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                        End If
                    Else
                        WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("C{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                    End If
                Else
                    '〇西日本支店車庫(泉北)独自仕様
                    If PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_022702 Then
                        '★休日加算金
                        If PrintDatarow("SYUBETSU").ToString() = "休日加算金" Then

                            '(日本栄船)独自仕様
                            If PrintDatarow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_004916 _
                                AndAlso PrintDatarow("BIKOU1").ToString() = "3名乗車" Then
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("E{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            Else
                                WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("D{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                            End If

                        End If

                        '〇姫路営業所独自仕様
                    ElseIf PrintDatarow("ORGCODE").ToString() = BaseDllConst.CONST_ORDERORGCODE_022801 Then
                        '★日祝配送
                        If PrintDatarow("SYUBETSU").ToString() = "日祝配送" Then
                            WW_Workbook.Worksheets(WW_SheetNoTmp04).Range(String.Format("E{0}", PrintDatarow("MASTERNO").ToString())).Value = iTanka
                        End If
                    End If
                End If
            Next

            '〇ENEOS業務委託料
            condition = ""
            If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020202 Then
                If Me.TaishoMM = "12" Then
                    condition = "RECOID='2'"
                Else
                    condition = "RECOID='1'"
                End If
            End If
            For Each PrintEneosComfeeDatarow As DataRow In PrintEneosComfeeData.Select(condition)
                WW_Workbook.Worksheets(WW_SheetNoTmp05).Range("E22").Value = Integer.Parse(PrintEneosComfeeDatarow("KINGAKU").ToString())
            Next

            '★計算エンジンの有効化
            WW_Workbook.EnableCalculation = True

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定([東部瓦斯]独自対応)
    ''' </summary>
    Private Sub EditDetailAreaTobugas(ByVal todokeCode As String, ByVal todokeOrder As String, ByVal cellNum As String, ByVal cellCnt As String)
        Dim zissekiNum As Double = 0    '【数量 （t）】設定用
        Dim zissekiCnt As Integer = 0   '【台数】設定用
        Dim cellStart As Integer = 12   '[設定行]
        Dim todokeDate As String = ""   '[届日]保管用
        Dim condition As String = String.Format("TODOKECODE='{0}' {1} ", todokeCode, todokeOrder)
        For Each PrintDatarow As DataRow In PrintData.Select(condition, "TODOKEDATE, TODOKEDATE_ORDER")
            Dim lineNum As Integer = Integer.Parse(Date.Parse(PrintDatarow("TODOKEDATE").ToString()).ToString("dd")) - 1
            lineNum += cellStart
            If todokeDate = "" OrElse todokeDate <> PrintDatarow("TODOKEDATE").ToString() Then
                zissekiNum = Double.Parse(PrintDatarow("ZISSEKI").ToString())
                zissekiCnt = 1
            Else
                zissekiNum += Double.Parse(PrintDatarow("ZISSEKI").ToString())
                zissekiCnt += 1
            End If

            WW_Workbook.Worksheets(WW_SheetNoTobuGas).Range(cellNum + lineNum.ToString()).Value = zissekiNum
            WW_Workbook.Worksheets(WW_SheetNoTobuGas).Range(cellCnt + lineNum.ToString()).Value = zissekiCnt

            todokeDate = PrintDatarow("TODOKEDATE").ToString()
        Next
    End Sub

    ''' <summary>
    ''' 帳票の明細設定([三井Ｅ＆Ｓ]独自対応)
    ''' </summary>
    Private Sub EditDetailAreaMitsuiES(ByVal todokeCode As String, ByVal tonNum As String, ByVal cellNum As String, ByVal cellCnt As String,
                                       Optional ByVal okFlg As Boolean = False)
        Dim zissekiNum As Double = 0                    '【数量 （t）】設定用
        Dim zissekiCnt As Integer = 0                   '【台数】設定用
        Dim cellStart As Integer = 12                   '[設定行]
        Dim syukaDate As String = ""                    '[出荷日]保管用
        Dim condition As String = String.Format("TODOKECODE='{0}'", todokeCode)
        Dim dtDummy As DataTable = PrintData.Copy
        dtDummy.Columns.Add("ZYUTYU_STR", Type.GetType("System.String"))
        For Each dtDummyrow As DataRow In dtDummy.Select(condition)
            dtDummyrow("ZYUTYU_STR") = dtDummyrow("ZYUTYU").ToString()
        Next
        condition &= tonNum

        For Each PrintDatarow As DataRow In dtDummy.Select(condition, "SHUKADATE")
            Dim lineNum As Integer = Integer.Parse(Date.Parse(PrintDatarow("SHUKADATE").ToString()).ToString("dd")) - 1
            lineNum += cellStart
            If syukaDate = "" OrElse syukaDate <> PrintDatarow("SHUKADATE").ToString() Then
                zissekiNum = Double.Parse(PrintDatarow("ZISSEKI").ToString())
                zissekiCnt = 1
            Else
                zissekiNum += Double.Parse(PrintDatarow("ZISSEKI").ToString())
                zissekiCnt += 1
            End If

            WW_Workbook.Worksheets(WW_SheetNoMitsuiES).Range(cellNum + lineNum.ToString()).Value = zissekiNum
            WW_Workbook.Worksheets(WW_SheetNoMitsuiES).Range(cellCnt + lineNum.ToString()).Value = zissekiCnt

            syukaDate = PrintDatarow("SHUKADATE").ToString()
        Next
    End Sub

End Class
