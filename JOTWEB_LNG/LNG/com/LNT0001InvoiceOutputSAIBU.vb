Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySql.Data.MySqlClient
Public Class LNT0001InvoiceOutputSAIBU
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNo As Integer = 0
    Private WW_SheetNoTmp As Integer = 0

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private InputData As DataTable
    Private PrintData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private OutputFileName As String = ""

    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CMNPTS As New CmnParts                                  '共通関数

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, outputFileName As String, inputDataClass As DataTable,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
            Me.InputData = inputDataClass
            'Me.PrintData = printDataClass
            Me.TaishoYm = taishoYm
            Me.TaishoYYYY = Date.Parse(taishoYm + "/" + "01").ToString("yyyy")
            Me.TaishoMM = Date.Parse(taishoYm + "/" + "01").ToString("MM")
            Me.OutputFileName = outputFileName
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
                If WW_Workbook.Worksheets(i).Name = "明細書" Then
                    WW_SheetNo = i
                End If
                If WW_Workbook.Worksheets(i).Name = "TEMP" Then
                    WW_SheetNoTmp = i
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
        'Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            '◯データ編集
            ReportCheck()
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            '***** TODO処理 ここまで *****

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
    ''' (帳票)項目チェック処理（西部ガス）
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ReportCheck()

        Dim TODOKE_003769 As String = "003769"      'エコア中津ガス
        Dim LNT0001Tanktbl As New DataTable
        Dim LNT0001Koteihitbl As New DataTable
        Dim dtKyushuTodoke As New DataTable
        Dim arrToriCode As String() = {"", ""}

        arrToriCode(0) = BaseDllConst.CONST_TORICODE_0045300000
        arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_024001
        Using SQLcon As MySql.Data.MySqlClient.MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            'CMNPTS.SelectCONVERTMaster(SQLcon, "SAIBU_KYUSHU_TODOKE", dtKyushuTodoke, I_ORDERBY_KEY:="VALUE01")
            SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", "SAIBU_KYUSHU_TODOKE", LNT0001Tanktbl)
            SelectKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm, LNT0001Koteihitbl)
        End Using

        '固定費
        Dim queryK = From row In LNT0001Koteihitbl.AsEnumerable()
                     Group row By TORICODE = row.Field(Of String)("TORICODE") Into Group
                     Select New With {
                            .TORICODE = TORICODE,
                            .DAISU = Group.Count(),
                            .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of Decimal)("KOTEIHIM")))
                        }

        '届先毎、枝番毎グルーピングして数量をサマリー（LINQを使う）
        Dim query = From row In InputData.AsEnumerable()
                    Group row By TODOKECODE = row.Field(Of String)("TODOKECODE"), BRANCHCODE = row.Field(Of String)("BRANCHCODE") Into Group
                    Select New With {
                            .TODOKECODE = TODOKECODE,
                            .BRANCHCODE = BRANCHCODE,
                            .DAISU = Group.Count(),
                            .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI")))
                        }

        PrintData = New DataTable
        PrintData.Columns.Add("TODOKECLASS", Type.GetType("System.String"))
        PrintData.Columns.Add("TODOKEMULTI", Type.GetType("System.String"))
        PrintData.Columns.Add("TODOKECODE", Type.GetType("System.String"))
        PrintData.Columns.Add("TODOKENAME", Type.GetType("System.String"))
        PrintData.Columns.Add("BRANCHCODE", Type.GetType("System.Decimal"))
        PrintData.Columns.Add("TANKA", Type.GetType("System.Int32"))
        PrintData.Columns.Add("DAISU", Type.GetType("System.Int32"))
        PrintData.Columns.Add("ZISSEKI", Type.GetType("System.Decimal"))
        PrintData.Columns.Add("AMT", Type.GetType("System.Decimal"))

        Dim prtRow As DataRow

        '〇請求書出力情報を保存
        '固定費設定
        For Each result In queryK
            prtRow = PrintData.NewRow
            prtRow("TODOKECLASS") = "K"
            prtRow("TODOKECODE") = ""
            prtRow("TODOKENAME") = result.DAISU & "台"
            prtRow("BRANCHCODE") = "1"
            prtRow("TANKA") = 0
            prtRow("DAISU") = 0
            prtRow("ZISSEKI") = 0
            prtRow("AMT") = result.KOTEIHI
            PrintData.Rows.Add(prtRow)
        Next
        '明細行設定
        For Each result As DataRow In LNT0001Tanktbl.Rows
            prtRow = PrintData.NewRow
            prtRow("TODOKECLASS") = result("BRANCHCODE")
            prtRow("TODOKEMULTI") = "OFF"
            prtRow("TODOKECODE") = result("AVOCADOTODOKECODE")
            prtRow("TODOKENAME") = result("AVOCADOTODOKENAME")
            prtRow("BRANCHCODE") = result("BRANCHCODE")
            prtRow("TANKA") = result("TANKA")
            prtRow("DAISU") = 0
            prtRow("ZISSEKI") = 0
            prtRow("AMT") = 0
            For Each dtRow In query
                If prtRow("TODOKECODE").ToString = dtRow.TODOKECODE AndAlso
                   prtRow("BRANCHCODE").ToString = dtRow.BRANCHCODE Then
                    prtRow("DAISU") = dtRow.DAISU
                    prtRow("ZISSEKI") = dtRow.ZISSEKI
                    Exit For
                End If
            Next
            PrintData.Rows.Add(prtRow)

            If result("BRANCHCODE").ToString > "1" Then
                prtRow = PrintData.NewRow
                prtRow("TODOKECLASS") = "T"
                prtRow("TODOKEMULTI") = "OFF"
                prtRow("TODOKECODE") = result("AVOCADOTODOKECODE")
                prtRow("TODOKENAME") = result("AVOCADOTODOKENAME")
                prtRow("BRANCHCODE") = 0
                prtRow("TANKA") = 0
                prtRow("DAISU") = 0
                prtRow("ZISSEKI") = 0
                prtRow("AMT") = 0
                PrintData.Rows.Add(prtRow)
            End If
        Next
        'ローリー変動費計
        prtRow = PrintData.NewRow
        prtRow("TODOKECLASS") = "H"
        prtRow("TODOKEMULTI") = "OFF"
        prtRow("TODOKECODE") = ""
        prtRow("TODOKENAME") = "ローリー変動費計"
        prtRow("BRANCHCODE") = 0
        prtRow("TANKA") = 0
        prtRow("DAISU") = 0
        prtRow("ZISSEKI") = 0
        prtRow("AMT") = 0
        PrintData.Rows.Add(prtRow)

        '複数回転のマーキング（枝番が１以上のものが存在すれば、同一届先をマーキング）
        For Each result As DataRow In LNT0001Tanktbl.Select("BRANCHCODE > '1'")
            For Each multirow As DataRow In PrintData.Select("TODOKECODE = '" & Convert.ToString(result("AVOCADOTODOKECODE")) & "'")
                multirow("TODOKEMULTI") = "ON"
            Next
        Next


    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Try
            '〇 年月（鏡用）
            Dim lastDate As String = Me.TaishoYYYY + "/" + Me.TaishoMM + "/01"
            lastDate = Date.Parse(lastDate).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
            WW_Workbook.Worksheets(WW_SheetNo).Range("D1").Value = Date.Parse(lastDate)

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
            Dim lineKcnt As Integer = 7
            Dim lineHcnt As Integer = 10
            Dim lineTcnt As Integer = 0
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim rowInx As Integer = 11
            Dim Formula1 As String = ""
            Dim Formula2 As String = ""
            Dim FormulaT1 As String = ""
            Dim FormulaT2 As String = ""
            Dim FormulaT3 As String = ""
            Dim KoteihiCell As String = ""
            Dim FirstFlg As Boolean = False

            '一旦、ローリー変動費をクリアしておく（行削除）
            '最終行の取得
            Dim lastRow As Integer = WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Row + WW_Workbook.Worksheets(Me.WW_SheetNo).UsedRange.Rows.Count - 1
            WW_Workbook.Worksheets(WW_SheetNo).Range(rowInx.ToString + ":" + lastRow.ToString).Delete()

            For Each PrintDatarow As DataRow In PrintData.Rows
                If PrintDatarow("TODOKECLASS").ToString = "K" Then
                    '固定費の編集
                    '◯ 届先名
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" & lineKcnt.ToString).Value = PrintDatarow("TODOKENAME").ToString()
                    '◯ 固定費
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" & lineKcnt.ToString).Value = Double.Parse(PrintDatarow("AMT").ToString())

                    KoteihiCell = "E" & lineKcnt.ToString

                ElseIf PrintDatarow("TODOKECLASS").ToString = "H" Then
                    lineTcnt = lineHcnt + 1
                    '２．変動費合計の編集
                    srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A9:E10")
                    destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" & lineTcnt.ToString())
                    '行の高さコピー
                    destRange.Rows(0).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A9").RowHeight
                    destRange.Rows(1).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A10").RowHeight
                    srcRange.Copy(destRange)

                    lineTcnt += 1
                    '◯ 届先名
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" & lineTcnt.ToString).Value = PrintDatarow("TODOKENAME").ToString()
                    '◯ 台数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" & lineTcnt.ToString).Formula = FormulaT1
                    '◯ 数量
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" & lineTcnt.ToString).Formula = FormulaT2
                    '◯ 輸送費
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" & lineTcnt.ToString).Formula = FormulaT3

                    '３．合計の編集
                    Dim TtlRow As Integer = lineTcnt + 1
                    srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A11:E15")
                    destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" & TtlRow.ToString())
                    '行の高さコピー
                    destRange.Rows(0).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A11").RowHeight
                    destRange.Rows(1).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A12").RowHeight
                    destRange.Rows(2).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A13").RowHeight
                    destRange.Rows(3).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A14").RowHeight
                    destRange.Rows(4).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A15").RowHeight
                    srcRange.Copy(destRange)
                    '◯ 輸送費（小計＝固定費＋変動費）
                    TtlRow += 2
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" & TtlRow.ToString()).Formula = "=" & KoteihiCell & "+" & "E" & lineTcnt.ToString()
                    '◯ 輸送費（消費税、合計はEXCELに任せる）
                Else
                    '変動費明細
                    '行コピー（枠組みの作成）
                    If PrintDatarow("TODOKECLASS").ToString = "1" Then
                        lineHcnt += 1
                        srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A3:E4")
                        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" & lineHcnt.ToString())
                        '行の高さコピー
                        destRange.Rows(0).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A3").RowHeight
                        destRange.Rows(1).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A4").RowHeight
                        srcRange.Copy(destRange)
                    ElseIf PrintDatarow("TODOKECLASS").ToString = "2" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A7:E8")
                        destRange = WW_Workbook.Worksheets(Me.WW_SheetNo).Range("A" & (lineHcnt + 1).ToString())
                        '行の高さコピー
                        destRange.Rows(0).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A7").RowHeight
                        destRange.Rows(1).RowHeight = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A8").RowHeight
                        srcRange.Copy(destRange)
                    End If

                    '届先別の輸送費編集
                    lineHcnt += 1

                    '合計行の編集（エコア合計（1回転＋2回転））
                    If PrintDatarow("TODOKECLASS").ToString = "1" Then
                        Formula1 = "=" & "C" & lineHcnt.ToString
                        Formula2 = "=" & "D" & lineHcnt.ToString
                    ElseIf PrintDatarow("TODOKECLASS").ToString <> "T" Then
                        Formula1 &= "+" & "C" & lineHcnt.ToString
                        Formula2 &= "+" & "D" & lineHcnt.ToString
                    End If

                    '合計行の場合、上記で編集した数式を設定
                    If PrintDatarow("TODOKECLASS").ToString = "T" Then
                        '◯ 届先
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" & lineHcnt.ToString).Value = PrintDatarow("TODOKENAME").ToString() & "（合　計）"
                        '◯ 台数
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" & lineHcnt.ToString).Formula = Formula1
                        '◯ 実績数量
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" & lineHcnt.ToString).Formula = Formula2
                    Else
                        '◯ 届先名
                        Dim AddStr As String = ""
                        If PrintDatarow("TODOKEMULTI").ToString = "OFF" Then
                            AddStr = ""
                        Else
                            AddStr = "（" & PrintDatarow("BRANCHCODE").ToString & "回転）"
                        End If
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" & lineHcnt.ToString).Value = PrintDatarow("TODOKENAME").ToString() & AddStr
                        '◯ 単価
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" & lineHcnt.ToString).Value = Double.Parse(PrintDatarow("TANKA").ToString())
                        '◯ 台数
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" & lineHcnt.ToString).Value = Double.Parse(PrintDatarow("DAISU").ToString())
                        '◯ 実績数量
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" & lineHcnt.ToString).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString())
                    End If

                    '変動費の合計行の数式編集
                    If IsNumeric(PrintDatarow("TODOKECLASS")) Then
                        If FirstFlg = False Then
                            FirstFlg = True
                            FormulaT1 &= "=C" & lineHcnt.ToString
                            FormulaT2 &= "=D" & lineHcnt.ToString
                            FormulaT3 &= "=E" & lineHcnt.ToString
                        Else
                            FormulaT1 &= "+C" & lineHcnt.ToString
                            FormulaT2 &= "+D" & lineHcnt.ToString
                            FormulaT3 &= "+E" & lineHcnt.ToString
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub
    ''' <summary>
    ''' 単価マスタTBL検索
    ''' </summary>
    Public Sub SelectTANKAMaster(ByVal SQLcon As MySqlConnection,
                                 ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByVal I_CLASS As String, ByRef O_dtTANKAMas As DataTable,
                                 Optional ByVal I_TODOKECODE As String = Nothing)
        If IsNothing(O_dtTANKAMas) Then
            O_dtTANKAMas = New DataTable
        End If
        If O_dtTANKAMas.Columns.Count <> 0 Then
            O_dtTANKAMas.Columns.Clear()
        End If
        O_dtTANKAMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0006.TORICODE "
        SQLStr &= "   ,LNM0006.TORINAME "
        SQLStr &= "   ,LNM0006.ORGCODE "
        SQLStr &= "   ,LNM0006.ORGNAME "
        SQLStr &= "   ,LNM0006.KASANORGCODE "
        SQLStr &= "   ,LNM0006.KASANORGNAME "
        SQLStr &= "   ,LNM0006.AVOCADOSHUKABASHO "
        SQLStr &= "   ,LNM0006.AVOCADOSHUKANAME "
        SQLStr &= "   ,LNM0006.SHUKABASHO "
        SQLStr &= "   ,LNM0006.SHUKANAME "
        SQLStr &= "   ,LNM0006.AVOCADOTODOKECODE "
        SQLStr &= "   ,LNM0006.AVOCADOTODOKENAME "
        SQLStr &= "   ,LNM0006.TODOKECODE "
        SQLStr &= "   ,LNM0006.TODOKENAME "
        SQLStr &= "   ,LNM0006.TANKNUMBER "
        SQLStr &= "   ,LNM0006.SHABAN "
        SQLStr &= "   ,LNM0006.STYMD "
        SQLStr &= "   ,LNM0006.ENDYMD "
        SQLStr &= "   ,LNM0006.BRANCHCODE "
        SQLStr &= "   ,LNM0006.TANKAKBN "
        SQLStr &= "   ,LNM0006.MEMO "
        SQLStr &= "   ,LNM0006.TANKA "
        SQLStr &= "   ,LNM0006.CALCKBN "
        SQLStr &= "   ,LNM0006.ROUNDTRIP "
        SQLStr &= "   ,LNM0006.TOLLFEE "
        SQLStr &= "   ,LNM0006.SYAGATA "
        SQLStr &= "   ,LNM0006.SYAGATANAME "
        SQLStr &= "   ,LNM0006.SYABARA "
        SQLStr &= "   ,LNM0006.BIKOU1 "
        SQLStr &= "   ,LNM0006.BIKOU2 "
        SQLStr &= "   ,LNM0006.BIKOU3 "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA LNM0006 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0006.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0006.TORICODE = '{0}' ", I_TORICODE)
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0006.ORGCODE = '{0}' ", I_ORGCODE)
        End If
        SQLStr &= String.Format(" AND LNM0006.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0006.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_TODOKECODE) Then
            SQLStr &= String.Format(" AND LNM0006.AVOCADOTODOKECODE = '{0}' ", I_TODOKECODE)
        End If

        '-- ORDER BY
        SQLStr &= " ORDER BY LNM0006.AVOCADOTODOKECODE, LNM0006.BRANCHCODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTANKAMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTANKAMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 固定費マスタTBL検索
    ''' </summary>
    Public Sub SelectKOTEIHIMaster(ByVal SQLcon As MySqlConnection,
                                   ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtKOTEIHIMas As DataTable)
        If IsNothing(O_dtKOTEIHIMas) Then
            O_dtKOTEIHIMas = New DataTable
        End If
        If O_dtKOTEIHIMas.Columns.Count <> 0 Then
            O_dtKOTEIHIMas.Columns.Clear()
        End If
        O_dtKOTEIHIMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0007.TORICODE "
        SQLStr &= "   ,LNM0007.TORINAME "
        SQLStr &= "   ,LNM0007.ORGCODE "
        SQLStr &= "   ,LNM0007.ORGNAME "
        SQLStr &= "   ,LNM0007.KASANORGCODE "
        SQLStr &= "   ,LNM0007.KASANORGNAME "
        SQLStr &= "   ,LNM0007.TARGETYM "
        SQLStr &= "   ,LNM0007.SYABAN "
        SQLStr &= "   ,LNM0007.RIKUBAN "
        SQLStr &= "   ,LNM0007.SYAGATA "
        SQLStr &= "   ,LNM0007.SYAGATANAME "
        SQLStr &= "   ,LNM0007.SYABARA "
        SQLStr &= "   ,LNM0007.SEASONKBN "
        SQLStr &= "   ,LNM0007.SEASONSTART "
        SQLStr &= "   ,LNM0007.SEASONEND "
        SQLStr &= "   ,LNM0007.KOTEIHIM "
        SQLStr &= "   ,LNM0007.KOTEIHID "
        SQLStr &= "   ,LNM0007.KAISU "
        SQLStr &= "   ,LNM0007.GENGAKU "
        SQLStr &= "   ,LNM0007.AMOUNT "
        SQLStr &= "   ,LNM0007.BIKOU1 "
        SQLStr &= "   ,LNM0007.BIKOU2 "
        SQLStr &= "   ,LNM0007.BIKOU3 "
        SQLStr &= " FROM LNG.LNM0007_FIXED LNM0007 "
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0007.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0007.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0007.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0007.TARGETYM = '{0}' ", I_TAISHOYM.Replace("/", ""))

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtKOTEIHIMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtKOTEIHIMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub
End Class
