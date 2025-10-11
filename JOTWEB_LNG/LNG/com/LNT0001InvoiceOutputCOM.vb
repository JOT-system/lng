Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySql.Data.MySqlClient
Public Class LNT0001InvoiceOutputCOM
    Private WW_Workbook As New Workbook  '共通
    Private WW_SheetNoUnchin As Integer = 0
    Private WW_SheetNoKotei As Integer = 0
    Private WW_SheetNoSprate As Integer = 0
    Private WW_SheetNoTmp As Integer = 0

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private TaishoYm As String = ""
    Private TaishoYYYY As String = ""
    Private TaishoMM As String = ""
    Private OutputFileName As String = ""

    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CMNPTS As New CmnParts                                  '共通関数

    Private Const CONST_ROUND As Integer = 1                        '四捨五入
    Private Const CONST_FLOOR As Integer = 2                        '切り捨て
    Private Const CONST_CEILING As Integer = 3                      '切り上げ

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="outputFileName">(出力用)Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, outputFileName As String,
                   Optional ByVal taishoYm As String = Nothing,
                   Optional ByVal defaultDatakey As String = C_DEFAULT_DATAKEY)
        Try
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
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", CS0050SESSION.HTTPS_GET, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルopen
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "運賃明細" Then
                    WW_SheetNoUnchin = i
                End If
                If WW_Workbook.Worksheets(i).Name = "固定費明細" Then
                    WW_SheetNoKotei = i
                End If
                If WW_Workbook.Worksheets(i).Name = "特別料金" Then
                    WW_SheetNoSprate = i
                End If
                If WW_Workbook.Worksheets(i).Name = "TEMP" Then
                    WW_SheetNoTmp = i
                End If
            Next

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "コンストラクタ"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal UnchinData As DataTable, ByVal KoteiData As DataTable, ByVal EtcData As DataTable) As String
        Dim tmpFileName As String = Date.Parse(TaishoYm + "/" + "01").ToString("yyyy年MM月_") & Me.OutputFileName & "（共通）.xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯運賃明細の設定
            PrintData = UnchinData
            EditUnchinArea()

            '◯固定費明細の設定
            PrintData = KoteiData
            EditKoteiArea()

            '◯その他請求（特別料金）明細の設定
            PrintData = EtcData
            EditEtcArea()

            WW_Workbook.Worksheets(WW_SheetNoUnchin).Activate()
            WW_Workbook.Worksheets(WW_SheetNoTmp).Delete()

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
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "CreateExcelPrintData"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元にThrow
        Finally
        End Try

    End Function

    ''' <summary>
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Try
            Dim ymEdit As String = "（" + Me.TaishoYYYY + "年" + Me.TaishoMM + "月分）"
            '運賃明細
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("A3").Value = ymEdit
            '固定費明細
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("A3").Value = ymEdit

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "EditHeaderArea"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw
        Finally
        End Try
    End Sub
    ''' <summary>
    ''' 帳票の運賃明細設定
    ''' </summary>
    Private Sub EditUnchinArea()
        Try
            Dim stLine As Integer = 7
            Dim lineUcnt As Integer = stLine
            Dim rowInx As Integer = stLine
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim hiddenA As Boolean = True
            Dim hiddenB As Boolean = True
            Dim hiddenC As Boolean = True
            Dim hiddenD As Boolean = True
            Dim hiddenE As Boolean = True
            Dim hiddenF As Boolean = True
            Dim hiddenH As Boolean = True
            Dim hiddenI As Boolean = True
            Dim hiddenL As Boolean = True

            Const COL_ORDERORGNAME As String = "A"    '営業所
            Const COL_SHUKANAME As String = "B"       '出荷場所
            Const COL_TODOKENAME As String = "C"      '届先
            Const COL_SYAGATA As String = "D"         '車型
            Const COL_SYABARA As String = "E"         '車腹
            Const COL_GYOMUTANKNUM As String = "F"    '車番
            Const COL_TANKA As String = "G"           '単価
            Const COL_COUNT As String = "H"           '数量（回数・台数）
            Const COL_ZISSEKI As String = "I"         '数量
            Const COL_YUSOUHI As String = "J"         '小計（輸送費）
            Const COL_TAXAMT As String = "K"          '税額
            Const COL_TSUKORYO As String = "L"        '通行料
            Const COL_TOTAL As String = "M"           '合計

            '一旦、明細をクリアしておく（行削除）
            '運賃明細の最終行を取得
            Dim lastRow As Integer = 0
            lastRow = WW_Workbook.Worksheets(Me.WW_SheetNoUnchin).UsedRange.Row + WW_Workbook.Worksheets(Me.WW_SheetNoUnchin).UsedRange.Rows.Count - 1
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(rowInx.ToString + ":" + lastRow.ToString).Delete()

            '運賃明細
            For Each PrintDatarow As DataRow In PrintData.Rows
                'TEMPシートからフォーマット（行）をコピー
                srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A2:M2")
                destRange = WW_Workbook.Worksheets(Me.WW_SheetNoUnchin).Range("A" & lineUcnt.ToString())
                srcRange.Copy(destRange)

                '値が入っていない項目列を非表示にするための判定
                '◯ 営業所
                If Not String.IsNullOrEmpty(PrintDatarow("ORDERORGNAME").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_ORDERORGNAME & lineUcnt.ToString).Value = PrintDatarow("ORDERORGNAME").ToString()
                    hiddenA = False
                End If
                '◯ 出荷場所
                If Not String.IsNullOrEmpty(PrintDatarow("SHUKANAME").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_SHUKANAME & lineUcnt.ToString).Value = PrintDatarow("SHUKANAME").ToString()
                    hiddenB = False
                End If
                '◯ 届先名
                If Not String.IsNullOrEmpty(PrintDatarow("TODOKENAME").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TODOKENAME & lineUcnt.ToString).Value = PrintDatarow("TODOKENAME").ToString()
                    hiddenC = False
                End If
                '◯ 車型
                If Not String.IsNullOrEmpty(PrintDatarow("SYAGATA").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_SYAGATA & lineUcnt.ToString).Value = PrintDatarow("SYAGATA").ToString()
                    hiddenD = False
                End If
                '◯ 車腹
                If Not String.IsNullOrEmpty(PrintDatarow("SYABARA").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_SYABARA & lineUcnt.ToString).Value = PrintDatarow("SYABARA").ToString()
                    hiddenE = False
                End If
                '◯ 車番
                If Not String.IsNullOrEmpty(PrintDatarow("GYOMUTANKNUM").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_GYOMUTANKNUM & lineUcnt.ToString).Value = PrintDatarow("GYOMUTANKNUM").ToString()
                    hiddenF = False
                End If
                '◯ 単価
                If Not String.IsNullOrEmpty(PrintDatarow("TANKA").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TANKA & lineUcnt.ToString).Value = Int32.Parse(PrintDatarow("TANKA").ToString())
                End If
                '◯ 数量（回数・台数）
                If Not String.IsNullOrEmpty(PrintDatarow("COUNT").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_COUNT & lineUcnt.ToString).Value = Int32.Parse(PrintDatarow("COUNT").ToString())
                    hiddenH = False
                End If
                '◯ 数量
                If Not String.IsNullOrEmpty(PrintDatarow("ZISSEKI").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_ZISSEKI & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("ZISSEKI").ToString())
                    hiddenI = False
                End If
                '◯ 小計（輸送費）
                If Not String.IsNullOrEmpty(PrintDatarow("YUSOUHI").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_YUSOUHI & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("YUSOUHI").ToString())
                End If
                '◯ 税額
                If Not String.IsNullOrEmpty(PrintDatarow("TAXRATE").ToString) Then
                    '小計（輸送費）×税率
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TAXAMT & lineUcnt.ToString).Formula = "=" & COL_YUSOUHI & lineUcnt.ToString & "*" & (Int32.Parse(PrintDatarow("TAXRATE").ToString()) / 100)
                End If
                '◯ 通行料
                If Not String.IsNullOrEmpty(PrintDatarow("TSUKORYO").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TSUKORYO & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("TSUKORYO").ToString())
                    hiddenL = False
                End If
                '◯ 合計
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TOTAL & lineUcnt.ToString).Formula = "=" & COL_YUSOUHI & lineUcnt.ToString & "+" & COL_TAXAMT & lineUcnt.ToString & "+" & COL_TSUKORYO & lineUcnt.ToString  'J+K+L

                lineUcnt += 1
            Next

            '合計行
            'TEMPシートからフォーマット（行）をコピー
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A4:M4")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNoUnchin).Range("A" & lineUcnt.ToString())
            srcRange.Copy(destRange)

            If PrintData.Rows.Count > 0 Then
                '◯ 数量（回数・台数）
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_COUNT & lineUcnt.ToString).Formula = "=SUM(" & COL_COUNT & stLine.ToString & ":" & COL_COUNT & (lineUcnt - 1).ToString & ")"
                '◯ 数量
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_ZISSEKI & lineUcnt.ToString).Formula = "=SUM(" & COL_ZISSEKI & stLine.ToString & ":" & COL_ZISSEKI & (lineUcnt - 1).ToString & ")"
                '◯ 小計
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_YUSOUHI & lineUcnt.ToString).Formula = "=SUM(" & COL_YUSOUHI & stLine.ToString & ":" & COL_YUSOUHI & (lineUcnt - 1).ToString & ")"
                '◯ 税額
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TAXAMT & lineUcnt.ToString).Formula = "=SUM(" & COL_TAXAMT & stLine.ToString & ":" & COL_TAXAMT & (lineUcnt - 1).ToString & ")"
                '◯ 通行料
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TSUKORYO & lineUcnt.ToString).Formula = "=SUM(" & COL_TSUKORYO & stLine.ToString & ":" & COL_TSUKORYO & (lineUcnt - 1).ToString & ")"
                '◯ 合計
                WW_Workbook.Worksheets(WW_SheetNoUnchin).Range(COL_TOTAL & lineUcnt.ToString).Formula = "=SUM(" & COL_TOTAL & stLine.ToString & ":" & COL_TOTAL & (lineUcnt - 1).ToString & ")"
            End If

            '列の非表示
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("A:A").Hidden = hiddenA
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("B:B").Hidden = hiddenB
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("C:C").Hidden = hiddenC
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("D:D").Hidden = hiddenD
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("E:E").Hidden = hiddenE
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("F:F").Hidden = hiddenF
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("H:H").Hidden = hiddenH
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("I:I").Hidden = hiddenI
            WW_Workbook.Worksheets(WW_SheetNoUnchin).Range("L:L").Hidden = hiddenL

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "EditUnchinArea"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の固定費明細設定
    ''' </summary>
    Private Sub EditKoteiArea()
        Try
            Dim stLine As Integer = 7
            Dim lineUcnt As Integer = stLine
            Dim rowInx As Integer = stLine
            Dim no As Integer = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim hiddenA As Boolean = False
            Dim hiddenB As Boolean = False
            Dim hiddenC As Boolean = False
            Dim hiddenD As Boolean = False
            Dim hiddenE As Boolean = False
            Dim hiddenF As Boolean = False

            Const COL_NO As String = "A"              '№
            Const COL_ORGNAME As String = "B"         '営業所
            Const COL_SYAGATANAME As String = "C"     '車型
            Const COL_SYABARA As String = "D"         '車腹
            Const COL_SYABAN As String = "E"          '車番
            Const COL_RIKUBAN As String = "F"         '陸事番号
            Const COL_KOTEIHI As String = "G"         '固定費
            Const COL_CHOSEI As String = "H"          '調整額
            Const COL_TOTAL As String = "I"           '小計
            Const COL_COMMENT As String = "J"         '調整事由

            '一旦、明細をクリアしておく（行削除）
            '固定費明細の最終行を取得
            Dim lastRow As Integer = 0
            lastRow = WW_Workbook.Worksheets(Me.WW_SheetNoKotei).UsedRange.Row + WW_Workbook.Worksheets(Me.WW_SheetNoKotei).UsedRange.Rows.Count - 1
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range(rowInx.ToString + ":" + lastRow.ToString).Delete()

            If PrintData.Rows.Count = 0 Then
                'シートの非表示
                WW_Workbook.Worksheets(WW_SheetNoKotei).Visible = Visibility.Hidden
                Exit Sub
            End If

            '固定費明細
            For Each PrintDatarow As DataRow In PrintData.Rows
                'TEMPシートからフォーマット（行）をコピー
                srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A7:K7")
                destRange = WW_Workbook.Worksheets(Me.WW_SheetNoKotei).Range("A" & lineUcnt.ToString())
                srcRange.Copy(destRange)

                '◯ №
                WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_NO & lineUcnt.ToString).Value = no
                '◯ 営業所
                If Not String.IsNullOrEmpty(PrintDatarow("ORGNAME").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_ORGNAME & lineUcnt.ToString).Value = PrintDatarow("ORGNAME").ToString()
                    hiddenB = False
                End If
                '◯ 車型
                If Not String.IsNullOrEmpty(PrintDatarow("SYAGATANAME").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_SYAGATANAME & lineUcnt.ToString).Value = PrintDatarow("SYAGATANAME").ToString()
                    hiddenC = False
                End If
                '◯ 車腹
                If Not String.IsNullOrEmpty(PrintDatarow("SYABARA").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_SYABARA & lineUcnt.ToString).Value = PrintDatarow("SYABARA").ToString()
                    hiddenD = False
                End If
                '◯ 車番
                If Not String.IsNullOrEmpty(PrintDatarow("SYABAN").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_SYABAN & lineUcnt.ToString).Value = PrintDatarow("SYABAN").ToString()
                    hiddenE = False
                End If
                '◯ 陸事番号
                If Not String.IsNullOrEmpty(PrintDatarow("RIKUBAN").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_RIKUBAN & lineUcnt.ToString).Value = PrintDatarow("RIKUBAN").ToString()
                    hiddenF = False
                End If
                '◯ 固定費
                If Not String.IsNullOrEmpty(PrintDatarow("KOTEIHI").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_KOTEIHI & lineUcnt.ToString).Value = Int32.Parse(PrintDatarow("KOTEIHI").ToString())
                End If
                '◯ 調整額
                If Not String.IsNullOrEmpty(PrintDatarow("CHOSEI").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_CHOSEI & lineUcnt.ToString).Value = Int32.Parse(PrintDatarow("CHOSEI").ToString())
                End If
                '◯ 小計
                WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_TOTAL & lineUcnt.ToString).Formula = "=" & COL_KOTEIHI & lineUcnt.ToString & "+" & COL_CHOSEI & lineUcnt.ToString   'G+H
                '◯ 調整事由
                If Not String.IsNullOrEmpty(PrintDatarow("COMMENT").ToString) Then
                    WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_COMMENT & lineUcnt.ToString).Value = PrintDatarow("COMMENT").ToString()
                End If

                no += 1
                lineUcnt += 1

            Next

            '合計行
            'TEMPシートからフォーマット（行）をコピー
            srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A9:K9")
            destRange = WW_Workbook.Worksheets(Me.WW_SheetNoKotei).Range("A" & lineUcnt.ToString())
            srcRange.Copy(destRange)

            If PrintData.Rows.Count > 0 Then
                '◯ 固定費
                WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_KOTEIHI & lineUcnt.ToString).Formula = "=SUM(" & COL_KOTEIHI & stLine.ToString & ":" & COL_KOTEIHI & (lineUcnt - 1).ToString & ")"
                '◯ 調整額
                WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_CHOSEI & lineUcnt.ToString).Formula = "=SUM(" & COL_CHOSEI & stLine.ToString & ":" & COL_CHOSEI & (lineUcnt - 1).ToString & ")"
                '◯ 小計
                WW_Workbook.Worksheets(WW_SheetNoKotei).Range(COL_TOTAL & lineUcnt.ToString).Formula = "=SUM(" & COL_TOTAL & stLine.ToString & ":" & COL_TOTAL & (lineUcnt - 1).ToString & ")"
            End If

            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("A:A").Hidden = hiddenA
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("B:B").Hidden = hiddenB
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("C:C").Hidden = hiddenC
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("D:D").Hidden = hiddenD
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("E:E").Hidden = hiddenE
            WW_Workbook.Worksheets(WW_SheetNoKotei).Range("F:F").Hidden = hiddenF

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "EditKoteiArea"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のその他請求（特別料金）明細設定
    ''' </summary>
    Private Sub EditEtcArea()
        Try
            Dim stLine As Integer = 7
            Dim lineUcnt As Integer = stLine
            Dim rowInx As Integer = stLine
            Dim no As Integer = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim newWorkSheet As IWorksheet = Nothing
            Dim hiddenA As Boolean = False
            Dim hiddenB As Boolean = False
            Dim hiddenC As Boolean = False
            Dim hiddenD As Boolean = False
            Dim hiddenE As Boolean = False

            Const COL_NO As String = "A"              '№
            Const COL_BIGCATENAME As String = "B"     '大分類
            Const COL_MIDCATENAME As String = "C"     '中分類
            Const COL_SMALLCATENAME As String = "D"   '小分類
            Const COL_TANKA As String = "E"           '単価
            Const COL_COUNT As String = "F"           '数量（回・台）
            Const COL_QUANTITY As String = "G"        '数量
            Const COL_TOTAL As String = "H"           '小計

            Dim query = From row In PrintData.AsEnumerable()
                        Group row By
                            GROUPCODE = row.Field(Of String)("GROUPCODE")
                        Into Group
                        Select New With {
                            .GROUPCODE = GROUPCODE
                        }
            For Each dtRow In query
                newWorkSheet = WW_Workbook.Worksheets(Me.WW_SheetNoSprate).Copy()
                newWorkSheet.Name = WW_Workbook.Worksheets(Me.WW_SheetNoSprate).Name & " " & dtRow.GROUPCODE

                '一旦、明細をクリアしておく（行削除）
                '固定費明細の最終行を取得
                Dim lastRow As Integer = 0
                lastRow = newWorkSheet.UsedRange.Row + newWorkSheet.UsedRange.Rows.Count - 1
                newWorkSheet.Range(rowInx.ToString + ":" + lastRow.ToString).Delete()

                If PrintData.Rows.Count = 0 Then
                    'シートの非表示
                    newWorkSheet.Visible = Visibility.Hidden
                    Exit Sub
                End If

                '固定費明細
                Dim whereStr As String = "GROUPCODE = '" & dtRow.GROUPCODE & "'"
                For Each PrintDatarow As DataRow In PrintData.Select(whereStr)
                    'TEMPシートからフォーマット（行）をコピー
                    srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A12:K12")
                    destRange = newWorkSheet.Range("A" & lineUcnt.ToString())
                    srcRange.Copy(destRange)

                    '◯ №
                    newWorkSheet.Range(COL_NO & lineUcnt.ToString).Value = no
                    '◯ 大分類
                    If Not String.IsNullOrEmpty(PrintDatarow("BIGCATENAME").ToString) Then
                        newWorkSheet.Range(COL_BIGCATENAME & lineUcnt.ToString).Value = PrintDatarow("BIGCATENAME").ToString()
                        hiddenB = False
                    End If
                    '◯ 中分類
                    If Not String.IsNullOrEmpty(PrintDatarow("MIDCATENAME").ToString) Then
                        newWorkSheet.Range(COL_MIDCATENAME & lineUcnt.ToString).Value = PrintDatarow("MIDCATENAME").ToString()
                        hiddenC = False
                    End If
                    '◯ 小分類
                    If Not String.IsNullOrEmpty(PrintDatarow("SMALLCATENAME").ToString) Then
                        newWorkSheet.Range(COL_SMALLCATENAME & lineUcnt.ToString).Value = PrintDatarow("SMALLCATENAME").ToString()
                        hiddenD = False
                    End If
                    '◯ 単価
                    If Not String.IsNullOrEmpty(PrintDatarow("TANKA").ToString) Then
                        newWorkSheet.Range(COL_TANKA & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("TANKA").ToString())
                        hiddenE = False
                    End If
                    '◯ 回数・台数
                    If Not String.IsNullOrEmpty(PrintDatarow("COUNT").ToString) Then
                        newWorkSheet.Range(COL_COUNT & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("COUNT").ToString())
                    End If
                    '◯ 数量
                    If Not String.IsNullOrEmpty(PrintDatarow("QUANTITY").ToString) Then
                        newWorkSheet.Range(COL_QUANTITY & lineUcnt.ToString).Value = Double.Parse(PrintDatarow("QUANTITY").ToString())
                    End If
                    '◯ 小計
                    If Not String.IsNullOrEmpty(PrintDatarow("COUNT").ToString) Then
                        newWorkSheet.Range(COL_TOTAL & lineUcnt.ToString).Formula = "=" & COL_TANKA & lineUcnt.ToString & "*" & COL_COUNT & lineUcnt.ToString
                    End If
                    If Not String.IsNullOrEmpty(PrintDatarow("QUANTITY").ToString) Then
                        newWorkSheet.Range(COL_TOTAL & lineUcnt.ToString).Formula = "=" & COL_TANKA & lineUcnt.ToString & "*" & COL_QUANTITY & lineUcnt.ToString
                    End If

                    no += 1
                    lineUcnt += 1

                Next

                '合計行
                'TEMPシートからフォーマット（行）をコピー
                srcRange = WW_Workbook.Worksheets(WW_SheetNoTmp).Range("A14:K14")
                destRange = newWorkSheet.Range("A" & lineUcnt.ToString())
                srcRange.Copy(destRange)

                If PrintData.Rows.Count > 0 Then
                    '◯ 回数・台数
                    'newWorkSheet.Range(COL_COUNT & lineUcnt.ToString).Formula = "=SUM(" & COL_COUNT & stLine.ToString & ":" & COL_COUNT & (lineUcnt - 1).ToString & ")"
                    '◯ 数量
                    newWorkSheet.Range(COL_QUANTITY & lineUcnt.ToString).Formula = "=SUM(" & COL_QUANTITY & stLine.ToString & ":" & COL_QUANTITY & (lineUcnt - 1).ToString & ")"
                    '◯ 小計
                    newWorkSheet.Range(COL_TOTAL & lineUcnt.ToString).Formula = "=SUM(" & COL_TOTAL & stLine.ToString & ":" & COL_TOTAL & (lineUcnt - 1).ToString & ")"
                End If

                newWorkSheet.Range("A:A").Hidden = hiddenA
                newWorkSheet.Range("B:B").Hidden = hiddenB
                newWorkSheet.Range("C:C").Hidden = hiddenC
                newWorkSheet.Range("D:D").Hidden = hiddenD
                newWorkSheet.Range("E:E").Hidden = hiddenE

                no = 1
                lineUcnt = stLine
            Next

            WW_Workbook.Worksheets(Me.WW_SheetNoSprate).Delete()

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "EditEtcArea"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw
        Finally
        End Try
    End Sub

#Region "運賃明細編集"
    ''' <summary>
    ''' 運賃明細集計（ＥＮＥＯＳ）
    ''' </summary>
    Public Sub SumUnchinENEOS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME")
                        Into Group
                    Select New With {
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub

    ''' <summary>
    ''' 運賃明細集計（東北天然ガス）
    ''' </summary>
    Public Sub SumUnchinTNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "SHUKABASHO,TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                        SHUKANAME = row.Field(Of String)("SHUKANAME"),
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        GYOMUTANKNUM = row.Field(Of String)("GYOMUTANKNUM"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .SHUKABASHO = SHUKABASHO,
                        .SHUKANAME = SHUKANAME,
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .GYOMUTANKNUM = GYOMUTANKNUM,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("SHUKABASHO") = dtRow.SHUKABASHO
            prtRow("SHUKANAME") = dtRow.SHUKANAME
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("GYOMUTANKNUM") = dtRow.GYOMUTANKNUM
            prtRow("TANKA") = dtRow.TANKA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（東北電力）
    ''' </summary>
    Public Sub SumUnchinTOHOKU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "SHUKABASHO,TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                        SHUKANAME = row.Field(Of String)("SHUKANAME"),
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        GYOMUTANKNUM = row.Field(Of String)("GYOMUTANKNUM"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .SHUKABASHO = SHUKABASHO,
                        .SHUKANAME = SHUKANAME,
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .GYOMUTANKNUM = GYOMUTANKNUM,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("SHUKABASHO") = dtRow.SHUKABASHO
            prtRow("SHUKANAME") = dtRow.SHUKANAME
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("GYOMUTANKNUM") = dtRow.GYOMUTANKNUM
            prtRow("TANKA") = dtRow.TANKA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（西部ガス（エスジーリキッドサービス））
    ''' </summary>
    Public Sub SumUnchinSAIBU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("TANKA") = dtRow.TANKA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_FLOOR)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（エスケイ産業）
    ''' </summary>
    Public Sub SumUnchinESUKEI(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()
        Dim view As DataView = iTbl.DefaultView
        view.Sort = "TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        TAXRATE = row.Field(Of String)("TAXRATE"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .TAXRATE = TAXRATE,
                        .TANKA = TANKA,
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }
        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("TAXRATE") = dtRow.TAXRATE
            prtRow("TANKA") = dtRow.TANKA
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub

    ''' <summary>
    ''' 運賃明細集計（石油資源開発（本州））
    ''' </summary>
    Public Sub SumUnchinSEKIYUSHIGEN(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORDERORGCODE,SHUKABASHO,TODOKECODE,GYOMUTANKNUM"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORDERORGCODE = row.Field(Of String)("ORDERORGCODE"),
                        ORDERORGNAME = row.Field(Of String)("ORDERORGNAME"),
                        SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                        SHUKANAME = row.Field(Of String)("SHUKANAME"),
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        GYOMUTANKNUM = row.Field(Of String)("GYOMUTANKNUM")
                        Into Group
                    Select New With {
                        .ORDERORGCODE = ORDERORGCODE,
                        .ORDERORGNAME = ORDERORGNAME,
                        .SHUKABASHO = SHUKABASHO,
                        .SHUKANAME = SHUKANAME,
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .GYOMUTANKNUM = GYOMUTANKNUM,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORDERORGCODE") = dtRow.ORDERORGCODE
            prtRow("ORDERORGNAME") = dtRow.ORDERORGNAME
            prtRow("SHUKABASHO") = dtRow.SHUKABASHO
            prtRow("SHUKANAME") = dtRow.SHUKANAME
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("GYOMUTANKNUM") = dtRow.GYOMUTANKNUM
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_FLOOR)
            oTbl.Rows.Add(prtRow)
        Next
    End Sub

    ''' <summary>
    ''' 運賃明細集計（石油資源開発（北海道））
    ''' </summary>
    Public Sub SumUnchinSEKIYUSHIGENHokkaido(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "SHUKABASHO,TODOKECODE,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                        SHUKANAME = row.Field(Of String)("SHUKANAME"),
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .SHUKABASHO = SHUKABASHO,
                        .SHUKANAME = SHUKANAME,
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .SYABARA = SYABARA,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("SHUKABASHO") = dtRow.SHUKABASHO
            prtRow("SHUKANAME") = dtRow.SHUKANAME
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("TANKA") = dtRow.TANKA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（ＤＡＩＧＡＳ）
    ''' </summary>
    Public Sub SumUnchinDAIGAS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "TODOKECODE,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .SYABARA = SYABARA,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("TANKA") = dtRow.TANKA
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（北海道ＬＮＧ）
    ''' </summary>
    Public Sub SumUnchinHOKKAIDOLNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "SHUKABASHO,TODOKECODE"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        SHUKABASHO = row.Field(Of String)("SHUKABASHO"),
                        SHUKANAME = row.Field(Of String)("SHUKANAME"),
                        TODOKECODE = row.Field(Of String)("TODOKECODE"),
                        TODOKENAME = row.Field(Of String)("TODOKENAME"),
                        TANKA = row.Field(Of String)("TANKA")
                        Into Group
                    Select New With {
                        .SHUKABASHO = SHUKABASHO,
                        .SHUKANAME = SHUKANAME,
                        .TODOKECODE = TODOKECODE,
                        .TODOKENAME = TODOKENAME,
                        .TANKA = TANKA,
                        .COUNT = Group.Count(),
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("SHUKABASHO") = dtRow.SHUKABASHO
            prtRow("SHUKANAME") = dtRow.SHUKANAME
            prtRow("TODOKECODE") = dtRow.TODOKECODE
            prtRow("TODOKENAME") = dtRow.TODOKENAME
            prtRow("TANKA") = dtRow.TANKA
            prtRow("COUNT") = dtRow.COUNT
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 運賃明細集計（シーエナジー／エルネス）
    ''' </summary>
    Public Sub SumUnchinCENERGY(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GYOMUTANKNUM"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        GYOMUTANKNUM = row.Field(Of String)("GYOMUTANKNUM")
                        Into Group
                    Select New With {
                        .GYOMUTANKNUM = GYOMUTANKNUM,
                        .ZISSEKI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("ZISSEKI"))),
                        .YUSOUHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("YUSOUHI"))),
                        .TSUKORYO = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("TSUKORYO")))
                        }

        Dim prtRow As DataRow
        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("GYOMUTANKNUM") = dtRow.GYOMUTANKNUM
            prtRow("ZISSEKI") = dtRow.ZISSEKI
            prtRow("YUSOUHI") = Rounding(dtRow.YUSOUHI, 0, CONST_ROUND)
            prtRow("TSUKORYO") = Rounding(dtRow.TSUKORYO * 0.55 / 1.1, 0, CONST_ROUND)
            oTbl.Rows.Add(prtRow)
        Next

    End Sub

#End Region

#Region "固定費"

    ''' <summary>
    ''' 固定費明細集計（ＥＮＥＯＳ）
    ''' </summary>
    Public Sub SumFixedENEOS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（東北天然ガス）
    ''' </summary>
    Public Sub SumFixedTNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（東北電力）
    ''' </summary>
    Public Sub SumFixedTOHOKU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（西部ガス（エスジーリキッドサービス））
    ''' </summary>
    Public Sub SumFixedSAIBU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（エスケイ産業）
    ''' </summary>
    Public Sub SumFixedESUKEI(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（石油資源開発（本州分））
    ''' </summary>
    Public Sub SumFixedSEKIYUSHIGEN(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（石油資源開発（北海道））
    ''' </summary>
    Public Sub SumFixedSEKIYUSHIGENHokkaido(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（ＤＡＩＧＡＳ）
    ''' </summary>
    Public Sub SumFixedDAIGAS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（北海道ＬＮＧ）
    ''' </summary>
    Public Sub SumFixedHOKKAIDOLNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' 固定費明細集計（シーエナジー／エルネス）
    ''' </summary>
    Public Sub SumFixedCENERGY(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "ORGCODE,SYAGATA,SYABAN,RIKUBAN,SYABARA"
        iTbl = view.ToTable

        Dim query = From row In iTbl.AsEnumerable()
                    Group row By
                        ORGNAME = row.Field(Of String)("ORGNAME"),
                        SYAGATANAME = row.Field(Of String)("SYAGATANAME"),
                        SYABARA = row.Field(Of String)("SYABARA"),
                        SYABAN = row.Field(Of String)("SYABAN"),
                        RIKUBAN = row.Field(Of String)("RIKUBAN"),
                        COMMENT = row.Field(Of String)("BIKOU3")
                    Into Group
                    Select New With {
                        .ORGNAME = ORGNAME,
                        .SYAGATANAME = SYAGATANAME,
                        .SYABARA = SYABARA,
                        .SYABAN = SYABAN,
                        .RIKUBAN = RIKUBAN,
                        .COMMENT = COMMENT,
                        .KOTEIHI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("KOTEIHIM"))),
                        .CHOSEI = Group.Sum(Function(r) Convert.ToDecimal(r.Field(Of String)("GENGAKU")))
                    }

        Dim prtRow As DataRow

        For Each dtRow In query
            prtRow = oTbl.NewRow
            prtRow("ORGNAME") = dtRow.ORGNAME
            prtRow("SYAGATANAME") = dtRow.SYAGATANAME
            prtRow("SYABARA") = dtRow.SYABARA
            prtRow("SYABAN") = dtRow.SYABAN
            prtRow("RIKUBAN") = dtRow.RIKUBAN
            prtRow("KOTEIHI") = dtRow.KOTEIHI
            prtRow("CHOSEI") = dtRow.CHOSEI
            prtRow("COMMENT") = dtRow.COMMENT
            oTbl.Rows.Add(prtRow)
        Next

    End Sub
#End Region

#Region "その他"

    ''' <summary>
    ''' その他請求（特別料金）明細集計（ＥＮＥＯＳ）
    ''' </summary>
    Public Sub SumEtcENEOS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（東北天然ガス）
    ''' </summary>
    Public Sub SumEtcTNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（東北電力）
    ''' </summary>
    Public Sub SumEtcTOHOKU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（西部ガス（エスケイリキッドサービス）
    ''' </summary>
    Public Sub SumEtcSAIBU(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（エスケイ産業）
    ''' </summary>
    Public Sub SumEtcESUKEI(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（石油資源開発（本州））
    ''' </summary>
    Public Sub SumEtcSEKIYUSHIGEN(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（石油資源開発（北海道））
    ''' </summary>
    Public Sub SumEtcSEKIYUSHIGENHokkaido(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（ＤＡＩＧＡＳ）
    ''' </summary>
    Public Sub SumEtcDAIGAS(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（北海道ＬＮＧ）
    ''' </summary>
    Public Sub SumEtcHOKKAIDOLNG(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
    ''' <summary>
    ''' その他請求（特別料金）明細集計（シーエナジー／エルネス）
    ''' </summary>
    Public Sub SumEtcCENERGY(ByRef iTbl As DataTable, ByRef oTbl As DataTable)

        oTbl.Clear()

        Dim view As DataView = iTbl.DefaultView
        view.Sort = "GROUPCODE,BIGCATECODE,BIGCATENAME,MIDCATECODE"
        iTbl = view.ToTable

        Dim prtRow As DataRow

        For Each dtRow In iTbl.Rows
            prtRow = oTbl.NewRow
            prtRow("GROUPCODE") = dtRow("GROUPCODE")
            prtRow("BIGCATENAME") = dtRow("BIGCATENAME")
            prtRow("MIDCATENAME") = dtRow("MIDCATENAME")
            prtRow("SMALLCATENAME") = dtRow("SMALLCATENAME")
            prtRow("CALCUNIT") = dtRow("CALCUNIT")
            prtRow("DISPLAYFLG") = dtRow("DISPLAYFLG")
            prtRow("ASSESSMENTFLG") = dtRow("ASSESSMENTFLG")
            prtRow("TANKA") = dtRow("TANKA")
            If dtRow("CALCUNIT") = "トン単価" Then
                prtRow("QUANTITY") = dtRow("QUANTITY")
                prtRow("COUNT") = ""
            Else
                prtRow("QUANTITY") = ""
                prtRow("COUNT") = dtRow("QUANTITY")
            End If

            oTbl.Rows.Add(prtRow)
        Next

    End Sub
#End Region

    ''' <summary>
    ''' 端数処理
    ''' </summary>
    Public Function Rounding(ByVal iNum As Double, ByVal iLength As Integer, ByVal iRoundMethod As Integer) As Decimal
        Dim rtnNum As Decimal = 0
        Select Case iRoundMethod
            Case CONST_ROUND
                '四捨五入
                rtnNum = Math.Round(iNum, iLength, MidpointRounding.AwayFromZero)
            Case CONST_FLOOR
                '切り捨て
                rtnNum = Math.Floor(iNum * 10 ^ iLength) / 10 ^ iLength
            Case CONST_CEILING
                '切り上げ
                rtnNum = Math.Ceiling(iNum * 10 ^ iLength) / 10 ^ iLength
        End Select

        Return rtnNum
    End Function

    ''' <summary>
    ''' 帳票の運賃明細設定
    ''' </summary>
    Public Sub CreUnchinTable(ByRef oTbl As DataTable)
        If IsNothing(oTbl) Then
            oTbl = New DataTable
        End If

        If oTbl.Columns.Count <> 0 Then
            oTbl.Columns.Clear()
        End If

        oTbl.Clear()
        oTbl.Columns.Add("ORDERORGCODE", Type.GetType("System.String"))    '営業所コード
        oTbl.Columns.Add("ORDERORGNAME", Type.GetType("System.String"))    '営業所名
        oTbl.Columns.Add("SHUKABASHO", Type.GetType("System.String"))      '出荷場所コード
        oTbl.Columns.Add("SHUKANAME", Type.GetType("System.String"))       '出荷場所名
        oTbl.Columns.Add("TODOKECODE", Type.GetType("System.String"))      '届先コード
        oTbl.Columns.Add("TODOKENAME", Type.GetType("System.String"))      '届先名
        oTbl.Columns.Add("SYAGATA", Type.GetType("System.String"))         '車型
        oTbl.Columns.Add("SYABARA", Type.GetType("System.String"))         '車腹
        oTbl.Columns.Add("GYOMUTANKNUM", Type.GetType("System.String"))    '業務車番
        oTbl.Columns.Add("TANKA", Type.GetType("System.String"))           '単価
        oTbl.Columns.Add("COUNT", Type.GetType("System.String"))           '回数・台数
        oTbl.Columns.Add("ZISSEKI", Type.GetType("System.String"))         '実績数量
        oTbl.Columns.Add("YUSOUHI", Type.GetType("System.String"))         '輸送費
        oTbl.Columns.Add("TAXRATE", Type.GetType("System.String"))         '税率
        oTbl.Columns.Add("TAXAMT", Type.GetType("System.String"))          '税額
        oTbl.Columns.Add("TSUKORYO", Type.GetType("System.String"))        '通行料
        'oTbl.Columns.Add("TOTAL", Type.GetType("System.String"))           '合計額

    End Sub
    ''' <summary>
    ''' 帳票の固定費明細設定
    ''' </summary>
    Public Sub CreKoteiTable(ByRef oTbl As DataTable)
        If IsNothing(oTbl) Then
            oTbl = New DataTable
        End If

        If oTbl.Columns.Count <> 0 Then
            oTbl.Columns.Clear()
        End If

        oTbl.Clear()
        oTbl.Columns.Add("ORGCODE", Type.GetType("System.String"))         '営業所コード
        oTbl.Columns.Add("ORGNAME", Type.GetType("System.String"))         '営業所名
        oTbl.Columns.Add("SYAGATA", Type.GetType("System.String"))         '車型
        oTbl.Columns.Add("SYAGATANAME", Type.GetType("System.String"))     '車型名
        oTbl.Columns.Add("SYABARA", Type.GetType("System.String"))         '車腹
        oTbl.Columns.Add("SYABAN", Type.GetType("System.String"))          '業務車番
        oTbl.Columns.Add("RIKUBAN", Type.GetType("System.String"))         '陸事車番
        oTbl.Columns.Add("COUNT", Type.GetType("System.String"))           '回数・台数
        oTbl.Columns.Add("KOTEIHI", Type.GetType("System.String"))         '固定費
        oTbl.Columns.Add("CHOSEI", Type.GetType("System.String"))          '調整額
        'oTbl.Columns.Add("TOTAL", Type.GetType("System.String"))           '小計
        oTbl.Columns.Add("COMMENT", Type.GetType("System.String"))         '調整事由

    End Sub
    ''' <summary>
    ''' 帳票のその他請求（特別料金）明細設定
    ''' </summary>
    Public Sub CreEtcTable(ByRef oTbl As DataTable)
        If IsNothing(oTbl) Then
            oTbl = New DataTable
        End If

        If oTbl.Columns.Count <> 0 Then
            oTbl.Columns.Clear()
        End If

        oTbl.Clear()
        oTbl.Columns.Add("GROUPCODE", Type.GetType("System.String"))       '合算
        oTbl.Columns.Add("BIGCATECODE", Type.GetType("System.String"))     '大分類コード
        oTbl.Columns.Add("BIGCATENAME", Type.GetType("System.String"))     '大分類名
        oTbl.Columns.Add("MIDCATECODE", Type.GetType("System.String"))     '中分類コード
        oTbl.Columns.Add("MIDCATENAME", Type.GetType("System.String"))     '中分類名
        oTbl.Columns.Add("SMALLCATECODE", Type.GetType("System.String"))   '小分類コード
        oTbl.Columns.Add("SMALLCATENAME", Type.GetType("System.String"))   '小分類名
        oTbl.Columns.Add("CALCUNIT", Type.GetType("System.String"))        '計算単位
        oTbl.Columns.Add("DISPLAYFLG", Type.GetType("System.String"))      '表示フラグ
        oTbl.Columns.Add("ASSESSMENTFLG", Type.GetType("System.String"))   '鑑分けフラグ
        oTbl.Columns.Add("TANKA", Type.GetType("System.String"))           '単価
        oTbl.Columns.Add("COUNT", Type.GetType("System.String"))           '回数・台数
        oTbl.Columns.Add("QUANTITY", Type.GetType("System.String"))        '数量

    End Sub

End Class
