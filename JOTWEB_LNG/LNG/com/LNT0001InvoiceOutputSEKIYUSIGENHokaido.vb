Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Public Class LNT0001InvoiceOutputSEKIYUSIGENHokaido
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoSeikyuMeisai As Integer = 0
    Private WW_SheetNoUchiwake As Integer = 0
    Private WW_SheetNoMuroran As Integer = 0
    Private WW_SheetNoTotal As Integer = 0
    Private WW_SheetNoTotal01 As Integer = 0
    Private WW_SheetNoTotal02 As Integer = 0
    Private WW_SheetNoCalendar As Integer = 0
    Private WW_SheetNoMaster As Integer = 0
    Private WW_SheetNo01Dic As New Dictionary(Of String, Integer)                       '// 既存シート用(石狩)
    Private WW_ArrSheetNo01 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '// 追加(ＮＯ)シート用(北ガス出荷)
    Private WW_ArrSheetName01 As String() = {"", "", "", "", "", "", "", "", "", ""}    '// 追加(名称)シート用(北ガス出荷)
    Private WW_ArrSheetNo02 As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '// 追加(ＮＯ)シート用(ほくでん出荷)
    Private WW_ArrSheetName02 As String() = {"", "", "", "", "", "", "", "", "", ""}    '// 追加(名称)シート用(ほくでん出荷)
    Private WW_ArrSheetNoKoteichi As Integer() = {0, 0, 0, 0, 0}                        '// 単価シート用
    Private WW_DicIshikariList As Dictionary(Of String, String)
    Private WW_ReportOtherNo As String() = {"⓪", "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧", "⑨", "⑩"}

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
    ''' <param name="printSKKoteihiDataClass"></param>
    ''' <param name="printCalendarDataClass">カレンダーマスタ</param>
    ''' <param name="dicIshikariList">>石狩(届先)格納</param>
    ''' <param name="printHolidayRateDataClass">休日割増単価マスタ</param>
    Public Sub New(mapId As String, orgCode As String, excelFileName As String, outputFileName As String, printDataClass As DataTable,
               printTankDataClass As DataTable, printKoteihiDataClass As DataTable, printSKKoteihiDataClass As DataTable, printCalendarDataClass As DataTable,
               dicIshikariList As Dictionary(Of String, String),
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
            Me.PrintTogouSprate = printTogouSprateDataClass
            Me.PrintHolidayRateData = printHolidayRateDataClass
            Me.WW_DicIshikariList = dicIshikariList
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

            '〇[石狩]シート設定
            For Each dic In dicIshikariList
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

                ElseIf WW_Workbook.Worksheets(i).Name = "請求明細" Then
                    '〇共通(シート[請求明細])
                    WW_SheetNoSeikyuMeisai = i
                ElseIf WW_Workbook.Worksheets(i).Name = "内訳" Then
                    '〇共通(シート[内訳])
                    WW_SheetNoUchiwake = i
                ElseIf WW_Workbook.Worksheets(i).Name = "室蘭ガスサーチャージ" Then
                    '〇共通(シート[室蘭ガスサーチャージ])
                    WW_SheetNoMuroran = i
                ElseIf WW_Workbook.Worksheets(i).Name = "合計表（入力不要）" Then
                    '〇共通(シート[合計表（入力不要）])
                    WW_SheetNoTotal = i
                ElseIf WW_Workbook.Worksheets(i).Name = "合計表（入力不要）北ガス" Then
                    '〇共通(シート[合計表（入力不要）北ガス])
                    WW_SheetNoTotal01 = i
                ElseIf WW_Workbook.Worksheets(i).Name = "合計表（入力不要）ほくでん" Then
                    '〇共通(シート[合計表（入力不要）ほくでん])
                    WW_SheetNoTotal02 = i
                ElseIf WW_Workbook.Worksheets(i).Name = "①KG石狩～釧路(40ft) " Then
                    '〇SK(シート[届先別])
                    WW_SheetNoCalendar = i
                ElseIf WW_Workbook.Worksheets(i).Name = "ﾏｽﾀ" Then
                    '〇共通(シート[ﾏｽﾀ])
                    WW_SheetNoMaster = i
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP9" + (j(0) + 1).ToString("00") + "1" Then
                    WW_ArrSheetNo01(j(0)) = i
                    WW_ArrSheetName01(j(0)) = WW_Workbook.Worksheets(i).Name
                    j(0) += 1
                ElseIf WW_Workbook.Worksheets(i).Name = "TMP9" + (j(1) + 1).ToString("00") + "2" Then
                    WW_ArrSheetNo02(j(1)) = i
                    WW_ArrSheetName02(j(1)) = WW_Workbook.Worksheets(i).Name
                    j(1) += 1
                    'ElseIf WW_Workbook.Worksheets(i).Name = "①KG石狩～釧路(40ft) " _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)新潟②" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(新潟・庄内)秋田" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(東北)" _
                    '    OrElse WW_Workbook.Worksheets(i).Name = "固定値(茨城)" Then
                    '    WW_ArrSheetNoKoteichi(j(1)) = i
                    '    j(1) += 1
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
            EditKoteihiTankaArea()
            '***** TODO処理 ここまで *****
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

            '〇カレンダー設定
            Dim iCalendarLine As Integer = 4
            For Each PrintCalendarDatarow As DataRow In PrintCalendarData.Rows
                If PrintCalendarDatarow("WORKINGDAY").ToString() <> "0" Then
                    WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AG" + iCalendarLine.ToString("00")).Value = "1"
                Else
                    WW_Workbook.Worksheets(WW_SheetNoCalendar).Range("AG" + iCalendarLine.ToString("00")).Value = "0"
                End If
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
            'Dim condition As String = "SETCELL01<>'' AND GROUPNO_REP='{0}' AND SETCELL03='{1}'"
            Dim condition As String = "SETCELL01<>'' AND GROUPNO_REP='{0}' AND SHEETNAME_REP='{1}'"
            Dim conditionSb As String = ""
            Dim todokeCode As String = ""
            Dim grpNo As String = ""
            Dim sheetName As String = ""
            Dim cellStay As String = ""

            '★計算エンジンの無効化
            WW_Workbook.EnableCalculation = False

            '〇[①KG石狩～釧路(40ft)]  , [②ＫＧ石狩～釧路(ﾛｰﾘｰ)]
            '　[③ＫＧ石狩～室蘭(40ft)], [④ＫＧ石狩～室蘭(ﾛｰﾘｰ)]
            '　[⑤ＫＧ石狩～JSW(40ft)]
            '　[⑥北電～室蘭バンカリング(ﾛｰﾘｰ)]
            '　[⑦北電～ＳＫ勇払(40ft)], [⑧北電～ＳＫ勇払（ﾛｰﾘｰ)]
            '★シート設定
            For Each dicSheetNo01 In WW_SheetNo01Dic
                '届名
                todokeCode = dicSheetNo01.Key.Substring(0, 6)
                'GRPNo
                grpNo = dicSheetNo01.Key.Substring(6, 1)
                'If todokeCode <> BaseDllConst.CONST_TODOKECODE_003561 Then Continue For

                '★シート名取得
                sheetName = WW_DicIshikariList(dicSheetNo01.Key)
                '★条件設定
                conditionSb = String.Format(condition, grpNo, sheetName)
                '〇セル設定
                EditDetailAreaSub(conditionSb, todokeCode, dicSheetNo01)

                'conditionSb = String.Format(condition, "1", "コンテナ")
                'EditDetailAreaSub(conditionSb, todokeCode, dicSheetNo01)

                'conditionSb = String.Format(condition, "1", "ローリー")
                'EditDetailAreaSub(conditionSb, todokeCode, dicSheetNo01)
            Next

            '〇届名称(追加)用設定
            cellStay = ""
            For Each PrintDatarow As DataRow In PrintData.Select("TODOKECELL_REP<>''", "ROWSORTNO, SHUKADATE")
                Try
                    '★ シート「届先」表示
                    Dim iDisp As Integer = Integer.Parse(PrintDatarow("SHEETDISPLAY_REP").ToString())
                    Dim arrSheetNo As Integer = WW_ArrSheetNo01(iDisp)
                    If PrintDatarow("SETCELL03").ToString() = "ローリー" Then
                        arrSheetNo = WW_ArrSheetNo02(iDisp)
                    End If
                    WW_Workbook.Worksheets(arrSheetNo).Visible = Visibility.Visible

                    '★ シート名(変更前取得)
                    Dim beforeSheetName As String = WW_Workbook.Worksheets(arrSheetNo).Name

                    '★ シート「合計表（入力不要）」の追加分[納入先]表示
                    Dim sheetTotalFlg As Boolean = True
                    Select Case beforeSheetName
                        '〇合計表（入力不要）※北ガス出荷
                        Case WW_ArrSheetName01(0), WW_ArrSheetName02(0)
                            WW_Workbook.Worksheets(WW_SheetNoTotal01).Range(String.Format("{0}:{1}", "M", "Q")).Hidden = False
                        Case WW_ArrSheetName01(1), WW_ArrSheetName02(1)
                            WW_Workbook.Worksheets(WW_SheetNoTotal01).Range(String.Format("{0}:{1}", "Q", "V")).Hidden = False
                        Case WW_ArrSheetName01(2), WW_ArrSheetName02(2)
                            WW_Workbook.Worksheets(WW_SheetNoTotal01).Range(String.Format("{0}:{1}", "W", "AA")).Hidden = False
                        Case WW_ArrSheetName01(3), WW_ArrSheetName02(3)
                            WW_Workbook.Worksheets(WW_SheetNoTotal01).Range(String.Format("{0}:{1}", "AB", "AF")).Hidden = False
                        Case WW_ArrSheetName01(4), WW_ArrSheetName02(4)
                            WW_Workbook.Worksheets(WW_SheetNoTotal01).Range(String.Format("{0}:{1}", "AG", "AK")).Hidden = False

                        '〇合計表（入力不要）※ほくでん出荷
                        Case WW_ArrSheetName01(5), WW_ArrSheetName02(5)
                            WW_Workbook.Worksheets(WW_SheetNoTotal02).Range(String.Format("{0}:{1}", "K", "O")).Hidden = False
                        Case WW_ArrSheetName01(6), WW_ArrSheetName02(6)
                            WW_Workbook.Worksheets(WW_SheetNoTotal02).Range(String.Format("{0}:{1}", "P", "T")).Hidden = False
                        Case WW_ArrSheetName01(7), WW_ArrSheetName02(7)
                            WW_Workbook.Worksheets(WW_SheetNoTotal02).Range(String.Format("{0}:{1}", "U", "Y")).Hidden = False
                        Case WW_ArrSheetName01(8), WW_ArrSheetName02(8)
                            WW_Workbook.Worksheets(WW_SheetNoTotal02).Range(String.Format("{0}:{1}", "Z", "AD")).Hidden = False
                        Case WW_ArrSheetName01(9), WW_ArrSheetName02(9)
                            WW_Workbook.Worksheets(WW_SheetNoTotal02).Range(String.Format("{0}:{1}", "AE", "AI")).Hidden = False

                        Case Else
                            sheetTotalFlg = False
                    End Select

                    '★ [合計表（入力不要）]シート表示
                    If sheetTotalFlg = True Then
                        WW_Workbook.Worksheets(WW_SheetNoTotal).Visible = Visibility.Hidden
                        WW_Workbook.Worksheets(WW_SheetNoTotal01).Visible = Visibility.Visible
                        WW_Workbook.Worksheets(WW_SheetNoTotal02).Visible = Visibility.Visible
                    End If

                    '★ シート名変更
                    WW_Workbook.Worksheets(arrSheetNo).Name = PrintDatarow("SHEETNAME_REP").ToString()
                    '◯ 出荷日
                    WW_Workbook.Worksheets(arrSheetNo).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
                    '◯ 実績数量
                    WW_Workbook.Worksheets(arrSheetNo).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber

                Catch ex As Exception
                End Try

            Next

            '★シート「マスタ」設定
            cellStay = ""
            For Each PrintDatarow As DataRow In PrintData.Select("TODOKECELL_REP<>''", "ROWSORTNO, SHUKADATE")
                If cellStay <> "" AndAlso cellStay = PrintDatarow("TODOKECELL_REP").ToString() Then
                    Continue For
                End If

                Try
                    If PrintDatarow("SETCELL03").ToString() = "ローリー" Then
                        WW_Workbook.Worksheets(WW_SheetNoMaster).Range("G" + PrintDatarow("MASTERCELL_REP").ToString()).Value = PrintDatarow("TODOKENAME_REP").ToString()
                    Else
                        WW_Workbook.Worksheets(WW_SheetNoMaster).Range("F" + PrintDatarow("MASTERCELL_REP").ToString()).Value = PrintDatarow("TODOKENAME_REP").ToString()
                    End If
                Catch ex As Exception
                End Try
                '表示用セル保管
                cellStay = PrintDatarow("TODOKECELL_REP").ToString()
            Next

            '★計算エンジンの有効化
            WW_Workbook.EnableCalculation = True

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(サブ)
    ''' </summary>
    Private Sub EditDetailAreaSub(ByVal condition As String, ByVal todokeCode As String, ByVal dicSheetNo01 As KeyValuePair(Of String, Integer))

        For Each PrintDatarow As DataRow In PrintData.Select(condition, "ROWSORTNO, SHUKADATE")
            If PrintDatarow("TODOKECODE").ToString() <> todokeCode Then
                Continue For
            End If
            '◯ 出荷日
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("SETCELL01").ToString()).Value = Date.Parse(PrintDatarow("SHUKADATE").ToString())
            '◯ 実績数量
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("SETCELL02").ToString()).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString()) * Me.calcZissekiNumber
            '★ 納入指定時間
            If PrintDatarow("ORDERORGCODE_REP").ToString() = BaseDllConst.CONST_TODOKECODE_006915 Then
                '[室蘭港バンカリング]のみ
                WW_Workbook.Worksheets(dicSheetNo01.Value).Range("AP" + PrintDatarow("SETLINE").ToString()).Value = PrintDatarow("SHITEITIME").ToString()
                'WW_Workbook.Worksheets(dicSheetNo01.Value).Range("AP" + PrintDatarow("SETLINE").ToString()).Value = DateTime.Parse(PrintDatarow("SHITEITIME").ToString()).ToShortTimeString()
            End If
        Next

        Dim conditionSb As String = condition
        conditionSb &= " AND DISPLAYCELL_START<>'' "
        Dim cellStay As String = ""
        '〇陸事番号(追加)用設定
        For Each PrintDatarow As DataRow In PrintData.Select(conditionSb)
            If cellStay <> "" AndAlso cellStay = PrintDatarow("DISPLAYCELL_START").ToString() Then
                Continue For
            End If
            '〇シート「入力表」
            '★ 表示
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(String.Format("{0}:{1}", PrintDatarow("DISPLAYCELL_START").ToString(), PrintDatarow("DISPLAYCELL_END").ToString())).Hidden = False
            '★ コンテナ番号
            WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("DISPLAYCELL_START").ToString() + "2").Value = PrintDatarow("ROLLY_CONTAINER").ToString()
            '★ 業務番号
            If PrintDatarow("ORDERORGCODE_REP").ToString() = BaseDllConst.CONST_TODOKECODE_006915 _
                OrElse PrintDatarow("ORDERORGCODE_REP").ToString() = BaseDllConst.CONST_TODOKECODE_005834 Then
                '[ＳＫ勇払（工場）] OR [室蘭港バンカリング]のみ
                WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("DISPLAYCELL_START").ToString() + "3").Value = PrintDatarow("GYOMUTANKNUM_REP").ToString()
            End If
            ''★ 受注数量
            'Dim dblZyutyu As Double = Math.Round(Double.Parse(PrintDatarow("ZYUTYU").ToString()), 1, MidpointRounding.AwayFromZero)
            'WW_Workbook.Worksheets(dicSheetNo01.Value).Range(PrintDatarow("DISPLAYCELL_END").ToString() + "4").Value = dblZyutyu.ToString() + "t"

            '表示用セル保管
            cellStay = PrintDatarow("DISPLAYCELL_START").ToString()
        Next

    End Sub

    ''' <summary>
    ''' 帳票のSK固定費設定
    ''' </summary>
    Private Sub EditKoteihiTankaArea()

        Try
            '★計算エンジンの無効化
            WW_Workbook.EnableCalculation = False

            '〇[単価][固定費]設定
            'For Each PrintKoteihiDatarow As DataRow In PrintKoteihiData.Select("KOTEIHI_CELLNUM<>''")
            '    '〇シート「内訳」
            '    '★ 月額固定費
            '    If PrintKoteihiDatarow("BIGCATEGORYCODE").ToString() = "3" _
            '        AndAlso PrintKoteihiDatarow("CATEGORYCODE").ToString() = "3" Then
            '        '〇３）バンカリング追加人件費
            '        WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range("M" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("TANKA").ToString())
            '    Else
            '        WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range("F" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("TANKA").ToString())

            '        '★数量
            '        If PrintKoteihiDatarow("KUBUN").ToString() = "9" Then
            '            WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range("H" + PrintKoteihiDatarow("KOTEIHI_CELLNUM").ToString()).Value = Integer.Parse(PrintKoteihiDatarow("QUANTITY").ToString())
            '        End If
            '    End If
            'Next

            '〇[単価]設定(統合版単価マスタ)
            '〇シート「ﾏｽﾀ」
            For Each PrintTankDatarow As DataRow In PrintTankData.Rows
                If PrintTankDatarow("MASTERNO").ToString() = "" Then Continue For
                Dim setCell As String = ""
                Select Case PrintTankDatarow("SYABARA").ToString()
                    Case "15.700"
                        setCell = "B" + PrintTankDatarow("MASTERNO").ToString()
                    Case "14.000"
                        setCell = "C" + PrintTankDatarow("MASTERNO").ToString()
                    Case "13.200"
                        setCell = "D" + PrintTankDatarow("MASTERNO").ToString()
                    Case "40.000"
                        setCell = "E" + PrintTankDatarow("MASTERNO").ToString()
                    Case Else
                        Continue For
                End Select
                WW_Workbook.Worksheets(WW_SheetNoMaster).Range(setCell).Value = Double.Parse(PrintTankDatarow("TANKA").ToString())

                '表示セル("1"表示)
                If PrintTankDatarow("TODOKESHEET_DISPLAYFLG").ToString() = "1" Then
                    '〇シート「ﾏｽﾀ」
                    '★ 納入先
                    WW_Workbook.Worksheets(WW_SheetNoMaster).Range("A" + PrintTankDatarow("MASTERNO").ToString()).Value = PrintTankDatarow("TODOKENAME").ToString()

                    '★ 表示(範囲)設定
                    Dim cellRange As Integer = 6
                    If PrintTankDatarow("AVOCADOSHUKABASHO").ToString() = "006456" Then
                        cellRange = 7
                    End If

                    '★ 表示(内訳シート)
                    Dim cellStart As Integer = CInt(PrintTankDatarow("TODOKESHEET_CELL").ToString())
                    Dim cellEnd As Integer = CInt(PrintTankDatarow("TODOKESHEET_CELL").ToString()) + cellRange
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(String.Format("{0}:{1}", cellStart.ToString(), cellEnd.ToString())).Hidden = False

                End If

            Next

            '〇[単価][固定費]設定(統合版特別料金マスタ)
            Dim uchiwakeNo As String = "B"      '-- 内訳NO
            Dim uchiwakeName As String = "C"    '-- 内訳名称
            Dim uchiwakeTanka As String = "F"   '-- 単価
            Dim uchiwakeAmount As String = "H"  '-- 数量
            Dim uchiwakeTaxable As String = "M" '-- 課税対象額
            '　内訳(No)保持用
            'Dim uchiwakeHokkaido As Integer() = {0, 0, 0}   '-- 北海道ガス石狩LNG基地出荷分((SK)釧路ガス, (SK)室蘭ガス, JSW)
            'Dim uchiwakeHokuden As Integer() = {0, 0}       '-- ほくでん石狩LNG基地出荷分(ＳＫ勇払（工場）, 室蘭港バンカリング)
            Dim uchiwakeSyaryou As Integer() = {0}          '-- 車両固定運賃
            Dim uchiwakeContainaer As Integer() = {0}       '-- コンテナ料金
            Dim uchiwakeBunkering As Integer() = {0}        '-- バンカリング追加人件費
            Dim uchiwakeOther As Integer() = {0}            '-- その他
            '★(新)レイアウト対応
            If PrintTogouSprate.Rows(0)("KOTEIHI_CONVERT").ToString() = "SEKIYU_HKD_KOTEIHI2" Then
                uchiwakeNo = "C"                '-- 内訳NO
                uchiwakeName = "D"              '-- 内訳名称
                uchiwakeTanka = "E"             '-- 単価
                uchiwakeAmount = "F"            '-- 数量
                uchiwakeTaxable = "J"           '-- 課税対象額
            End If
            For Each PrintTogouSpraterow As DataRow In PrintTogouSprate.Select("KOTEIHI_CELLNUM<>''")
                '〇シート「内訳」
                '★ 月額固定費
                If PrintTogouSpraterow("BIGCATECODE").ToString() = "5" Then
                    '〇３）バンカリング追加人件費
                    '★ Ｎｏ設定
                    uchiwakeBunkering(0) += 1
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeNo + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = uchiwakeBunkering(0)
                    '★ 名称
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeName + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = PrintTogouSpraterow("SMALLCATENAME").ToString().Replace(PrintTogouSpraterow("MIDCATENAME").ToString() + "　", "")
                    '★ 単価
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeTaxable + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("TANKA").ToString())
                    '★ 表示
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(String.Format("{0}:{0}", PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString())).Hidden = False

                ElseIf PrintTogouSpraterow("BIGCATECODE").ToString() = "7" Then
                    Dim otDetailNo As Integer = 0
                    otDetailNo = CInt(PrintTogouSpraterow("SMALLCATECODE").ToString())
                    '★ その他
                    '・Ｎｏ設定
                    uchiwakeOther(0) += 1
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeNo + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = uchiwakeOther(0)
                    '・名称設定
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeName + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = PrintTogouSpraterow("SMALLCATENAME").ToString()
                    'WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeName + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = WW_ReportOtherNo(otDetailNo) + PrintTogouSpraterow("SMALLCATENAME").ToString()
                    '・金額設定
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeTaxable + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("TANKA").ToString())
                    '★ 表示
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(String.Format("{0}:{0}", PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString())).Hidden = False

                Else
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeTanka + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("TANKA").ToString())

                    '〇車両固定運賃・コンテナ料金・追加人件費
                    'If PrintTogouSpraterow("CALCUNIT").ToString() = "式" Then
                    If (PrintTogouSpraterow("BIGCATECODE").ToString() = "3" _
                            OrElse PrintTogouSpraterow("BIGCATECODE").ToString() = "4" _
                            OrElse PrintTogouSpraterow("BIGCATECODE").ToString() = "5") _
                        AndAlso PrintTogouSpraterow("QUANTITY").ToString() <> "0.00" Then
                        Try
                            '★ Ｎｏ設定
                            Select Case PrintTogouSpraterow("BIGCATECODE").ToString()
                                '車両固定運賃
                                Case "3"
                                    uchiwakeSyaryou(0) += 1
                                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeNo + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = uchiwakeSyaryou(0)
                                'コンテナ料金
                                Case "4"
                                    uchiwakeContainaer(0) += 1
                                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeNo + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = uchiwakeContainaer(0)
                            End Select
                            '★ 名称
                            WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeName + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = PrintTogouSpraterow("SMALLCATENAME").ToString().Replace(PrintTogouSpraterow("MIDCATENAME").ToString() + "　", "")
                            'Dim cellNo As String = WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeNo + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value.ToString()
                            'WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeName + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = cellNo + PrintTogouSpraterow("SMALLCATENAME").ToString()
                            '★ 数量
                            WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeAmount + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("QUANTITY").ToString())
                            '★ 表示
                            WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(String.Format("{0}:{0}", PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString())).Hidden = False
                        Catch ex As Exception
                        End Try

                        '### [1）勇払向け] かつ [⑤日祝割増運賃] #################################################
                        'または
                        '### [2)．日本製鉄室蘭製鉄所　構内バース向け] かつ [①陸上輸送分（15.7t）]以外 ###########
                    ElseIf (PrintTogouSpraterow("BIGCATECODE").ToString() = "2" _
                            AndAlso PrintTogouSpraterow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005834 _
                            AndAlso PrintTogouSpraterow("SMALLCATECODE").ToString() = "5") _
                        OrElse (PrintTogouSpraterow("BIGCATECODE").ToString() = "2" _
                            AndAlso PrintTogouSpraterow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_006915 _
                            AndAlso PrintTogouSpraterow("SMALLCATECODE").ToString() <> "1") Then
                        Try
                            WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(uchiwakeAmount + PrintTogouSpraterow("KOTEIHI_CELLNUM").ToString()).Value = Double.Parse(PrintTogouSpraterow("QUANTITY").ToString())
                        Catch ex As Exception
                        End Try
                    End If
                End If
            Next

            '〇届先(休日割増単価)設定
            If Me.OutputOrgCode = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                '■石狩営業所(日祝割増)
                Dim conditionSub As String = "RANGE_SUNDAY='1' OR RANGE_HOLIDAY='1' OR RANGE_YEAREND_NEWYEAR='1' OR RANGE_MAYDAY='1' "
                For Each PrintHolidayRateDatarow As DataRow In PrintHolidayRateData.Select(conditionSub)
                    If PrintHolidayRateDatarow("SETMASTERCELL").ToString() = "" Then Continue For
                    WW_Workbook.Worksheets(WW_SheetNoUchiwake).Range(String.Format("{0}{1}", uchiwakeTanka, PrintHolidayRateDatarow("SETMASTERCELL").ToString())).Value = Integer.Parse(PrintHolidayRateDatarow("TANKA").ToString())
                Next
            End If

            '〇[室蘭ガスサーチャージ]設定
            For Each PrintSKKoteihiDatarow As DataRow In PrintSKSurchargeData.Select(String.Format("TODOKECODE='{0}'", BaseDllConst.CONST_TODOKECODE_003563))
                '走行距離
                WW_Workbook.Worksheets(WW_SheetNoMuroran).Range("G19").Value = Decimal.Parse(PrintSKKoteihiDatarow("KYORI").ToString())
                '実勢軽油価格
                WW_Workbook.Worksheets(WW_SheetNoMuroran).Range("E24").Value = Decimal.Parse(PrintSKKoteihiDatarow("KEIYU").ToString())
                '基準価格
                WW_Workbook.Worksheets(WW_SheetNoMuroran).Range("G24").Value = Decimal.Parse(PrintSKKoteihiDatarow("KIZYUN").ToString())
                '輸送回数
                WW_Workbook.Worksheets(WW_SheetNoMuroran).Range("G31").Value = Integer.Parse(PrintSKKoteihiDatarow("KAISU").ToString())
                '燃料使用量
                WW_Workbook.Worksheets(WW_SheetNoMuroran).Range("I31").Value = Integer.Parse(PrintSKKoteihiDatarow("USAGECHARGE").ToString())
            Next

            ''〇[日本製鉄室蘭製鉄所構内バース サーチャージ]設定
            'For Each PrintSKKoteihiDatarow As DataRow In PrintSKSurchargeData.Select(String.Format("TODOKECODE='{0}'", BaseDllConst.CONST_TODOKECODE_006915))
            '    '走行距離(km/回)
            '    '燃料使用量(㍑/回)
            '    '実勢軽油価格(年度(４月～３月))
            '    '基準価格
            '    '輸送回数
            'Next

            ''〇[釧路ガス サーチャージ]設定
            'For Each PrintSKKoteihiDatarow As DataRow In PrintSKSurchargeData.Select(String.Format("TODOKECODE='{0}'", BaseDllConst.CONST_TODOKECODE_003561))
            '    '走行距離(km/回)
            '    '実勢軽油価格(直近６ヶ月)
            '    '基準価格
            '    '燃料使用量(㍑/回)
            '    '①15.7ｔ車(直近４ヶ月)
            '    '②14.0ｔ車(直近４ヶ月)
            '    '③13.2ｔ車(直近４ヶ月)
            '    '④40ｆｔ車(直近４ヶ月)
            'Next

            '★計算エンジンの有効化
            WW_Workbook.EnableCalculation = True

        Catch ex As Exception

        End Try

    End Sub

End Class
