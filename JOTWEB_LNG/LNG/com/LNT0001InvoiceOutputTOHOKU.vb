Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Public Class LNT0001InvoiceOutputTOHOKU
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0                                      '処理シート
    Private WW_SheetNoInput As Integer = 0                                 '入力シート
    Private WW_SheetNoInv As Integer = 0                                   '請求書シート
    Private WW_SheetNoTui As Integer = 0                                   '追加料金・日曜日料金シート
    Private WW_SheetNoSha As Integer = 0                                   '車号シート
    Private WW_SheetNoHai As Integer = 0                                   '配送先シート
    Private WW_SheetNoTmp As Integer = 0                                   'テンプレートシート

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
    Private KaisuuData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private TaishoLastDD As String = ""
    Private OutputFileName As String = ""

    Private USERID As String = ""
    Private USERTERMID As String = ""

    Private PrintOutputRowIdx As Int32 = 3                                  '出力位置（行）    　※初期値：3
    Private PrintMaxRowIdx As Int32 = 0                                     '最終位置（行）    　※初期値：0
    Private PrintTotalFirstRowIdx As Int32 = 0                              '合計最初位置（行）  ※初期値：0
    Private PrintTotalLastRowIdx As Int32 = 0                               '合計最終位置（行）  ※初期値：0
    Private PrintTotalRowIdx As Int32 = 0                                   '合計位置（行）      ※初期値：0
    Private PrintSuuRowIdx As Int32 = 25                                    '数量位置（行）      ※初期値：24
    Private PrintHaiRowIdx As Int32 = 6                                     '配送先位置（行）    ※初期値：6
    Private PrintShaRowIdx As Int32 = 12                                    '車号位置（行）      ※初期値：12
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
                If WW_Workbook.Worksheets(i).Name = "入力シート" Then
                    WW_SheetNoInput = i
                ElseIf WW_Workbook.Worksheets(i).Name = "請求書" Then
                    WW_SheetNoInv = i
                ElseIf WW_Workbook.Worksheets(i).Name = "追加料金・日曜日料金（JOT入力）" Then
                    WW_SheetNoTui = i
                ElseIf WW_Workbook.Worksheets(i).Name = "車号" Then
                    WW_SheetNoSha = i
                ElseIf WW_Workbook.Worksheets(i).Name = "配送先" Then
                    WW_SheetNoHai = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_SheetNoTmp = i
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
                '単価データ取得
                TankaData = GetTankaData(SQLcon)

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
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsm"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Dim CS0050SESSION As New CS0050SESSION
        Dim TODOKECODE As String = ""
        Dim TODOKENAME As String = ""
        Dim SHUKABASHO As String = ""
        Dim SHUKANAME As String = ""
        Dim FirstFLG As String = "1"
        Dim DataExist As String = "0"
        Dim NichiCount As Integer = 0
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try

            '***** 入力シート作成 TODO処理 ここから *****

            For Each OutPutRowData As DataRow In PrintData.Rows
                EditDetailArea(OutPutRowData)
            Next

            '***** 入力シート作成 TODO処理 ここまで *****

            '***** 追加料金・日曜日料金シート作成 TODO処理 ここから *****

            For Each TuiSheetRowData As DataRow In TuiSheetData.Rows
                If Convert.ToString(TuiSheetRowData("SHEETDISPLAY")) = "1" AndAlso Convert.ToString(TuiSheetRowData("EXISTFLG")) = "1" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("B" & TuiSheetRowData("IREKAEROW").ToString).Value = Convert.ToString(TuiSheetRowData("GYOMUTANKTNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("B" & TuiSheetRowData("NICHIYOUROW").ToString).Value = Convert.ToString(TuiSheetRowData("GYOMUTANKTNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Rows(CInt(TuiSheetRowData("IREKAEROW")) - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Rows(CInt(TuiSheetRowData("NICHIYOUROW")) - 1).Hidden = False
                End If

                If Convert.ToString(TuiSheetRowData("NICHIYOUCNT")) <> "0" Then
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D" & TuiSheetRowData("NICHIYOUROW").ToString).Value = Convert.ToString(TuiSheetRowData("NICHIYOUCNT"))
                    NichiCount += Convert.ToInt32(TuiSheetRowData("NICHIYOUCNT"))
                End If

                Me.WW_Workbook.Worksheets(Me.WW_SheetNoTui).Range("D63").Value = NichiCount
            Next

            '***** 追加料金・日曜日料金シート作成 TODO処理 ここまで *****

            '***** 車号別シート作成 TODO処理 ここから *****

            For Each ShaSheetRowData As DataRow In ShaSheetData.Rows
                If Convert.ToString(ShaSheetRowData("SHEETDISPLAY")) = "1" AndAlso Convert.ToString(ShaSheetRowData("EXISTFLG")) = "1" Then
                    WW_SheetNo = CInt(ShaSheetRowData("SHEETNO"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Visible = Visibility.Visible
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Name = Convert.ToString(ShaSheetRowData("SHEETNAME"))

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoSha).Range("A" & PrintShaRowIdx.ToString).EntireRow.Insert()
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoSha).Range("A" & PrintShaRowIdx.ToString).Value = Convert.ToInt32(ShaSheetRowData("GYOMUTANKNUM"))
                    PrintShaRowIdx += 1

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(CInt(ShaSheetRowData("KAGAMIROW")) - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & ShaSheetRowData("KAGAMIROW").ToString).Value = Convert.ToString(ShaSheetRowData("SHEETNAME"))
                End If
            Next

            '***** 車号別シート作成 TODO処理 ここまで *****

            '***** 届先別シート作成 TODO処理 ここから *****

            For Each HaiSheetRowData As DataRow In HaiSheetData.Rows
                If Convert.ToString(HaiSheetRowData("SHEETDISPLAY")) = "1" AndAlso Convert.ToString(HaiSheetRowData("EXISTFLG")) = "1" Then
                    WW_SheetNo = CInt(HaiSheetRowData("SHEETNO"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Visible = Visibility.Visible
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Name = Convert.ToString(HaiSheetRowData("SHEETNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A9").Value = Convert.ToString(HaiSheetRowData("SHEETNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B11").Value = Convert.ToString(HaiSheetRowData("TODOKENAME"))

                    PrintTankaRowIdx = 17
                    Dim DataCount As Integer = TankaData.Select("TODOKECODE ='" & Convert.ToString(HaiSheetRowData("TODOKECODE")) & "' and SHUKABASHO = '" & Convert.ToString(HaiSheetRowData("SHUKABASHO")) & "'").Count
                    If DataCount > 0 Then
                        Dim OutPutRowData As DataRow() = TankaData.Select("TODOKECODE ='" & Convert.ToString(HaiSheetRowData("TODOKECODE")) & "' and SHUKABASHO = '" & Convert.ToString(HaiSheetRowData("SHUKABASHO")) & "'")
                        If OutPutRowData.Length > 0 Then
                            For i As Integer = 0 To OutPutRowData.Length - 1
                                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("B" & PrintTankaRowIdx.ToString).Value = OutPutRowData(i)("SHABAN")
                                Me.WW_Workbook.Worksheets(Me.WW_SheetNo).Range("E" & PrintTankaRowIdx.ToString).Value = OutPutRowData(i)("TANKA")
                                PrintTankaRowIdx += 1
                            Next
                        End If
                    End If

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Range("A" & PrintHaiRowIdx.ToString & ":B" & PrintHaiRowIdx.ToString).EntireRow.Insert()
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Range("A" & PrintHaiRowIdx.ToString).Value = Convert.ToString(HaiSheetRowData("SHEETNAME"))
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoHai).Range("B" & PrintHaiRowIdx.ToString).Value = Convert.ToString(HaiSheetRowData("TODOKENAME"))
                    PrintHaiRowIdx += 1

                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(CInt(HaiSheetRowData("KAGAMIROW")) - 1).Hidden = False
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Rows(CInt(HaiSheetRowData("KAGAMITOTALROW")) - 1).Hidden = False
                    If Convert.ToString(HaiSheetRowData("SHUKABASHO")) = "004756" Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & HaiSheetRowData("KAGAMIROW").ToString).Value = " " & Convert.ToString(HaiSheetRowData("TODOKENAME")) & "向け(仙台)"
                    ElseIf Convert.ToString(HaiSheetRowData("SHUKABASHO")) = "002800" Then
                        Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & HaiSheetRowData("KAGAMIROW").ToString).Value = " " & Convert.ToString(HaiSheetRowData("TODOKENAME")) & "向け(新潟)"
                    End If
                    Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("C" & HaiSheetRowData("KAGAMITOTALROW").ToString).Value = Convert.ToString(HaiSheetRowData("TODOKENAME")) & "向け 合計"
                End If
            Next

            '***** 届先別シート作成 TODO処理 ここまで *****


            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("M2").Value = CInt(TaishoYYYY)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("M3").Value = CInt(TaishoMM)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("M4").Value = CInt(TaishoLastDD)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("M5").Value = CInt(TaishoYYYY)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInv).Range("M6").Value = CInt(TaishoMM)

            Me.WW_Workbook.Calculate()

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(tmpFilePath, SaveFileFormat.Xlsm)
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
    Private Sub EditDetailArea(ByVal pOutputRowData As DataRow)

        Try
            '届日(月)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("A" + Me.PrintOutputRowIdx.ToString()).Value = Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "MM") & "/"
            '届日(日)
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("B" + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("TODOKEDATE").ToString), "dd"))
            '出荷日(日)
            If Not pOutputRowData("SHUKADATE") Is DBNull.Value Then
                Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("C" + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(Format(Date.Parse(pOutputRowData("SHUKADATE").ToString), "dd"))
            End If
            '車号
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("D" + Me.PrintOutputRowIdx.ToString()).Value = Convert.ToInt32(pOutputRowData("GYOMUTANKNUM"))
            '数量
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("E" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ZISSEKI")
            '発送元
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("F" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("SHUKANAME")
            '配送先
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("G" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("TODOKENAME")
            '入力支店
            Me.WW_Workbook.Worksheets(Me.WW_SheetNoInput).Range("H" + Me.PrintOutputRowIdx.ToString()).Value = pOutputRowData("ORDERORGNAME")

            '出力件数加算
            Me.AddPrintRowCnt(1)

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
        SQLStr &= "    ,CASE "
        SQLStr &= "         WHEN A01.SHUKADATE = A01.TODOKEDATE THEN NULL "
        SQLStr &= "         ELSE A01.SHUKADATE "
        SQLStr &= "     END AS SHUKADATE "
        SQLStr &= "    ,A01.GYOMUTANKNUM "
        SQLStr &= "    ,A01.ZISSEKI * 1000 AS ZISSEKI "
        SQLStr &= "    ,A02.VALUE01 AS SHUKANAME "
        SQLStr &= "    ,A03.VALUE01 AS TODOKENAME "
        SQLStr &= "    ,CASE A01.ORDERORG "
        SQLStr &= "         WHEN '020402' THEN '東北支店' "
        SQLStr &= "         WHEN '021502' THEN '新潟支店' "
        SQLStr &= "     END ORDERORGNAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNT0001_ZISSEKI A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT A02 "
        SQLStr &= "     ON A02.CLASS = 'TOHOKU_DENRYOKU_NAME'"
        SQLStr &= "     AND A02.KEYCODE01 = A01.SHUKABASHO"
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT A03 "
        SQLStr &= "     ON A03.CLASS = 'TOHOKU_DENRYOKU_NAME'"
        SQLStr &= "     AND A03.KEYCODE01 = A01.TODOKECODE"

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
        SQLStr &= "   A01.TODOKEDATE "
        SQLStr &= " , A01.GYOMUTANKNUM "
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
        SQLStr &= "     A01.KEYCODE01 AS SHUKABASHO "
        SQLStr &= "    ,A02.VALUE01 AS SHUKANAME "
        SQLStr &= "    ,A01.KEYCODE02 AS TODOKECODE "
        SQLStr &= "    ,A03.VALUE01 AS TODOKENAME "
        SQLStr &= "    ,A01.VALUE01 AS SHEETNAME "
        SQLStr &= "    ,A01.VALUE02 AS SHEETDISPLAY "
        SQLStr &= "    ,A01.VALUE03 AS SHEETNO "
        SQLStr &= "    ,A01.VALUE04 AS MAXROW "
        SQLStr &= "    ,A01.VALUE05 AS KAGAMIROW "
        SQLStr &= "    ,A01.VALUE06 AS KAGAMITOTALROW "
        SQLStr &= "    ,CASE"
        SQLStr &= "         WHEN A04.SHUKABASHO IS NOT NULL THEN '1'"
        SQLStr &= "         ELSE '0'"
        SQLStr &= "     END EXISTFLG"
        SQLStr &= "    ,A01.KEYCODE04"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT A01 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT A02 "
        SQLStr &= "     ON A02.CLASS = 'TOHOKU_DENRYOKU_NAME'"
        SQLStr &= "     AND A02.KEYCODE01 = A01.KEYCODE01"
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT A03 "
        SQLStr &= "     ON A03.CLASS = 'TOHOKU_DENRYOKU_NAME'"
        SQLStr &= "     AND A03.KEYCODE01 = A01.KEYCODE02"
        SQLStr &= " LEFT JOIN("
        SQLStr &= "     SELECT "
        SQLStr &= "         A01.SHUKABASHO "
        SQLStr &= "        ,A01.TODOKECODE "
        SQLStr &= "     FROM LNG.LNT0001_ZISSEKI A01 "
        SQLStr &= "     WHERE "
        SQLStr &= String.Format("          A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format("      AND A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format("      AND A01.ORDERORG IN ({0}) ", "'020402','021502'")
        SQLStr &= String.Format("      AND A01.ZISSEKI <> '{0}' ", "0")
        SQLStr &= String.Format("      AND A01.LOADUNLOTYPE <> '{0}' ", "積込")
        SQLStr &= String.Format("      AND DATE_FORMAT(A01.TODOKEDATE,'%Y/%m') = '{0}' ", TaishoYm)
        SQLStr &= "     GROUP BY "
        SQLStr &= "       A01.SHUKABASHO "
        SQLStr &= "      ,A01.TODOKECODE "
        SQLStr &= " )A04 "
        SQLStr &= " ON A04.SHUKABASHO = A01.KEYCODE01 "
        SQLStr &= " AND A04.TODOKECODE = A01.KEYCODE02 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.CLASS = '{0}' ", "TOHOKU_DENRYOKU_TODO")

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
    ''' 車号シート情報データ取得
    ''' </summary>
    Private Function GetShaSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.KEYCODE01 AS GYOMUTANKNUM "
        SQLStr &= "    ,A01.VALUE01 AS SHEETNAME "
        SQLStr &= "    ,A01.VALUE02 AS SHEETDISPLAY "
        SQLStr &= "    ,A01.VALUE03 AS SHEETNO "
        SQLStr &= "    ,A01.VALUE04 AS KAGAMIROW "
        SQLStr &= "    ,CASE"
        SQLStr &= "         WHEN A02.GYOMUTANKNUM IS NOT NULL THEN '1'"
        SQLStr &= "         ELSE '0'"
        SQLStr &= "     END EXISTFLG"
        SQLStr &= "    ,A01.KEYCODE02"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT A01 "

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
        SQLStr &= " ON A02.GYOMUTANKNUM = A01.KEYCODE01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.CLASS = '{0}' ", "TOHOKU_DENRYOKU_SHA")

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
    ''' 追加料金・日曜日料金シート情報データ取得
    ''' </summary>
    Private Function GetTuiSheetData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "     A01.KEYCODE01 AS GYOMUTANKNUM "
        SQLStr &= "    ,A01.VALUE01 AS GYOMUTANKTNAME "
        SQLStr &= "    ,A01.VALUE02 AS SHEETDISPLAY "
        SQLStr &= "    ,A01.VALUE03 AS IREKAEROW "
        SQLStr &= "    ,A01.VALUE04 AS NICHIYOUROW "
        SQLStr &= "    ,CASE"
        SQLStr &= "         WHEN A03.GYOMUTANKNUM IS NOT NULL THEN '1'"
        SQLStr &= "         ELSE '0'"
        SQLStr &= "     END EXISTFLG"
        SQLStr &= "    ,IFNULL(A02.NICHIYOUCNT,0) AS NICHIYOUCNT "
        SQLStr &= "    ,A01.KEYCODE02"

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT A01 "

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
        SQLStr &= " ON A02.GYOMUTANKNUM = A01.KEYCODE01 "
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
        SQLStr &= " )A03 "
        SQLStr &= " ON A03.GYOMUTANKNUM = A01.KEYCODE01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND A01.CLASS = '{0}' ", "TOHOKU_DENRYOKU_TUI")

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
    ''' 単価データ取得
    ''' </summary>
    Private Function GetTankaData(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt As New DataTable

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   A01.SHABAN"
        SQLStr &= " , A01.TANKA "
        SQLStr &= " , A01.AVOCADOTODOKECODE "
        SQLStr &= " , A01.AVOCADOSHUKABASHO "
        SQLStr &= " , A01.BRANCHCODE "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA A01 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     A01.TORICODE = '{0}' ", "0175400000")
        SQLStr &= String.Format(" AND A01.ORGCODE IN ({0}) ", "'020402','021502'")
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
    ''' 固定費マスタ更新
    ''' </summary>
    Private Sub UpdKoteihi(ByVal SQLcon As MySqlConnection, ByVal Row As DataRow)

        Dim WW_DATE As Date = Date.Now

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0009_TNGKOTEIHI                  ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     KAISU               = @KAISU            ")
        SQLStr.Append("   , KINGAKU             = @KINGAKU          ")
        SQLStr.Append("   , UPDYMD              = @UPDYMD           ")
        SQLStr.Append("   , UPDUSER             = @UPDUSER          ")
        SQLStr.Append("   , UPDTERMID           = @UPDTERMID        ")
        SQLStr.Append("   , UPDPGID             = @UPDPGID          ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("       TAISHOYM  = @TAISHOYM                 ")
        SQLStr.Append("   AND SYABAN    = @SYABAN                   ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)    '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)       '車番
                Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)          '使用回数
                Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 8)      '金額
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)          '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20) '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ

                P_TAISHOYM.Value = TaishoYm.Replace("/", "")  '対象年月
                P_SYABAN.Value = Row("SYABAN")                '車番
                P_KAISU.Value = Row("KAISU")                  '使用回数
                P_KINGAKU.Value = Row("GOUKEI")               '金額
                P_UPDYMD.Value = WW_DATE                      '更新年月日
                P_UPDUSER.Value = USERID                      '更新ユーザーＩＤ
                P_UPDTERMID.Value = USERTERMID                '更新端末
                P_UPDPGID.Value = "LNT0001InvoiceOutputTNG"   '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001 UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class
